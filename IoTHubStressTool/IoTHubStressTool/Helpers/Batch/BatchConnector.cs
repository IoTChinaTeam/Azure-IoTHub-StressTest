using GalaSoft.MvvmLight.Messaging;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Common;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using StressLoadDemo.Helpers.Configuration;
using StressLoadDemo.Model.AzureObject;
using StressLoadDemo.Model.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StressLoadDemo.Helpers.Batch
{
    public class BatchConnector : IBatchConnector
    {
        const string ContainerName = "stresstest";
        IConfigurationProvider configurationProvider;
        public BatchConnector(IConfigurationProvider provider = null)
        {
            configurationProvider = provider == null ? new InternalProvider() : provider;

            BatchServiceUrl = configurationProvider.GetConfigValue("BatchServiceUrl");

            var builder = new UriBuilder(BatchServiceUrl);
            BatchAccountName = builder.Host.Split('.').First();
            BatchAccountKey = configurationProvider.GetConfigValue("BatchAccountKey");

            StorageConnectionString = configurationProvider.GetConfigValue("StorageConnectionString");
            StorageAccount = CloudStorageAccount.Parse(StorageConnectionString);

            BatchOsSize = configurationProvider.GetConfigValue("SizeOfVM");
        }

        private string BatchServiceUrl { get; set; }
        private string BatchAccountName { get; set; }
        private string BatchAccountKey { get; set; }

        private CloudStorageAccount StorageAccount { get; set; }

        public string StorageConnectionString { get; private set; }

        private string BatchOsFamily { get; } = "4";

        private string BatchOsSize { get; set; }

        public async Task<bool> Deploy(TestJob testJob)
        {
            Messenger.Default.Send($"{DateTime.Now.ToString("T")} - Deploy {testJob.BatchJobId}", "RunningLog");

            BatchSharedKeyCredentials credentials = new BatchSharedKeyCredentials(BatchServiceUrl, BatchAccountName, BatchAccountKey);
            using (BatchClient batchClient = await BatchClient.OpenAsync(credentials))
            {
                batchClient.CustomBehaviors.Add(RetryPolicyProvider.LinearRetryProvider(TimeSpan.FromSeconds(10), 3));

                try
                {
                    Messenger.Default.Send($"{DateTime.Now.ToString("T")} - Create Pool ({testJob.NumofVm} core)", "RunningLog");
                    await CreatePoolIfNotExistAsync(batchClient, StorageAccount, testJob);
                    Messenger.Default.Send(
                        new DeployStatusUpdateMessage()
                        {
                            Phase = DeployPhase.PoolCreated,
                            Status = PhaseStatus.Succeeded
                        }, "DeployStatus");
                   
                }
                catch (Exception ex)
                {
                    Messenger.Default.Send($"{DateTime.Now.ToString("T")} - delete test due to {ex.Message}", "RunningLog");
                    Messenger.Default.Send(
                        new DeployStatusUpdateMessage()
                        {
                            Phase = DeployPhase.PoolCreated,
                            Status = PhaseStatus.Failed
                        },"DeployStatus" );
                    await batchClient.DeleteTest(testJob);
                }
                try
                {
                    Messenger.Default.Send($"{DateTime.Now.ToString("T")} - Submit job", "RunningLog");
                    await SubmitJobIfNotExistAsync(batchClient, StorageAccount, testJob);
                    Messenger.Default.Send(
                        new DeployStatusUpdateMessage()
                        {
                            Phase = DeployPhase.JobCreated,
                            Status = PhaseStatus.Succeeded
                        }, "DeployStatus");

                }
                catch (Exception ex)
                {
                    Messenger.Default.Send($"{DateTime.Now.ToString("T")} - delete test due to {ex.Message}", "RunningLog");
                    Messenger.Default.Send(
                        new DeployStatusUpdateMessage()
                        {
                            Phase = DeployPhase.JobCreated,
                            Status = PhaseStatus.Failed
                        }, "DeployStatus");
                    await batchClient.DeleteTest(testJob);

                }
            }

            return true;
        }


        public async Task<bool> DeleteTest(TestJob testJob)
        {
            BatchSharedKeyCredentials credentials = new BatchSharedKeyCredentials(BatchServiceUrl, BatchAccountName, BatchAccountKey);
            using (BatchClient batchClient = await BatchClient.OpenAsync(credentials))
            {
                return await batchClient.DeleteTest(testJob);
            }
        }

        public async Task<bool> DeleteTest(BatchClient client, TestJob testJob)
        {
            var job = await client.JobOperations.GetJobAsync(testJob.BatchJobId);
            Messenger.Default.Send($"{DateTime.Now.ToString("T")} - Waiting job to finish: {job.ListTasks().Count()} tasks ", "RunningLog");
            Messenger.Default.Send(
                       new DeployStatusUpdateMessage()
                       {
                           Phase = DeployPhase.JobStarted,
                           Status = PhaseStatus.Succeeded
                       }, "DeployStatus");
            Messenger.Default.Send($"Job has been started, please switch to monitor tab", "RunningLog");

            return true;
        }

        /// <summary>
        /// Creates a pool if it doesn't already exist.  If the pool already exists, this method resizes it to meet the expected
        /// targets specified in settings.
        /// </summary>
        /// <param name="batchClient">The BatchClient to use when interacting with the Batch service.</param>
        /// <param name="cloudStorageAccount">The CloudStorageAccount to upload start task required files to.</param>
        /// <returns>An asynchronous <see cref="Task"/> representing the operation.</returns>
        private async Task CreatePoolIfNotExistAsync(BatchClient batchClient, CloudStorageAccount cloudStorageAccount, TestJob testJob)
        {
            // You can learn more about os families and versions at:
            // https://azure.microsoft.com/en-us/documentation/articles/cloud-services-guestos-update-matrix/
            // Attempt to create the pool

            bool poolExisting = false;
            try
            {
                CloudPool pool = batchClient.PoolOperations.CreatePool(TestJob.BatchPoolId, testJob.SizeOfVM.ToString(), new CloudServiceConfiguration(BatchOsFamily), testJob.NumofVm);

                // azure batch max limitation: times (4x) the number of node cores
                pool.MaxTasksPerComputeNode = 4 * (int)Math.Pow(2, (int)testJob.SizeOfVM);
                pool.TaskSchedulingPolicy = new TaskSchedulingPolicy(ComputeNodeFillType.Spread);
                await pool.CommitAsync();
            }
            catch (BatchException e)
            {
                if (e.RequestInformation != null &&
                    e.RequestInformation.BatchError != null &&
                    e.RequestInformation.BatchError.Code == BatchErrorCodeStrings.PoolExists)
                {
                    poolExisting = true;
                }
                else
                {
                    throw; // Any other exception is unexpected
                }
            }

            if (poolExisting)
            {
                // The pool already existed when we tried to create it
                CloudPool existingPool = await batchClient.PoolOperations.GetPoolAsync(TestJob.BatchPoolId);

                // If the pool doesn't have the right number of nodes, isn't resizing, and doesn't have
                // automatic scaling enabled, then we need to ask it to resize
                if (existingPool.CurrentDedicated < testJob.NumofVm &&
                    existingPool.AllocationState != AllocationState.Resizing &&
                    existingPool.AutoScaleEnabled == false)
                {
                    // Resize the pool to the desired target. Note that provisioning the nodes in the pool may take some time
                    await existingPool.ResizeAsync(testJob.NumofVm);
                }
            }
        }

        /// <summary>
        /// Creates a job and adds a task to it. The task is a 
        /// custom executable which has a resource file associated with it.
        /// </summary>
        /// <param name="batchClient">The BatchClient to use when interacting with the Batch service.</param>
        /// <param name="cloudStorageAccount">The storage account to upload the files to.</param>
        /// <param name="jobId">The ID of the job.</param>
        /// <returns>The set of container names containing the jobs input files.</returns>
        private async Task<bool> SubmitJobIfNotExistAsync(BatchClient batchClient, CloudStorageAccount cloudStorageAccount, TestJob testJob)
        {
            CloudJob batchJob = batchClient.JobOperations.CreateJob();
            batchJob.Id = testJob.BatchJobId;
            batchJob.PoolInformation = new PoolInformation() { PoolId = TestJob.BatchPoolId };

            bool jobExists = false;
            try
            {
                await batchJob.CommitAsync();
            }
            catch (BatchException e)
            {
                if (e.RequestInformation != null &&
                    e.RequestInformation.BatchError != null &&
                    e.RequestInformation.BatchError.Code == BatchErrorCodeStrings.JobExists)
                {
                    jobExists = true;
                }
                else
                {
                    throw;
                }
            }

            if (jobExists)
            {
                // check whether task are there
                var job = await batchClient.JobOperations.GetJobAsync(testJob.BatchJobId);
                if (job != null && job.ListTasks().Count() > 0)
                {
                    return true;
                }
            }

            var tasksToRun = new List<CloudTask>();
            Messenger.Default.Send($"{DateTime.Now.ToString("T")} - Upload assemblies for batch", "RunningLog");

            Console.WriteLine($"{DateTime.Now.ToString("T")} - Upload assemblies for batch");
            var resourceFiles = await UploadFilesToContainerAsync(cloudStorageAccount);
            Messenger.Default.Send(
                new DeployStatusUpdateMessage()
                { Phase = DeployPhase.AssemblyUploaded,
                    Status = PhaseStatus.Succeeded },
                "DeployStatus"
                );
            var jobsPerVm = 4 * (int)Math.Pow(2, (int)testJob.SizeOfVM);
            for (int i = 0; i < jobsPerVm * testJob.NumofVm; i++)
            {
                if (string.IsNullOrEmpty(testJob.Message))
                {
                    testJob.Message = string.Empty;
                }

                var command = string.Format("DeviceLoad.exe {0} {1} {2} {3} {4} {5} {6}-{7} \"{8}\" {9}",
                    StorageConnectionString,
                    testJob.DeviceClientEndpoint,
                    testJob.DevicePerVm / jobsPerVm,
                    testJob.MessagePerMin,
                    testJob.DurationInMin,
                    testJob.BatchJobId,
                    configurationProvider.GetConfigValue("DeviceIdPrefix"),
                    i.ToString().PadLeft(4, '0'),
                    testJob.Message.Replace("\"", "\\\""),
                    testJob.Transport);

                CloudTask taskWithFiles = new CloudTask("deviceTest" + i, command);
                taskWithFiles.ResourceFiles = resourceFiles;
                await batchClient.JobOperations.AddTaskAsync(testJob.BatchJobId, taskWithFiles);
            }

            return true;
        }

        private static async Task<List<ResourceFile>> UploadFilesToContainerAsync(CloudStorageAccount cloudStorageAccount)
        {
            CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = null;
            for (int i = 1; i < 10; i++)
            {
                var name = ContainerName + i;
                container = blobClient.GetContainerReference(name);
                if (await container.ExistsAsync())
                {
                    await container.DeleteIfExistsAsync();
                    continue;
                }

                await container.CreateIfNotExistsAsync();
                break;
            }

            var resourceFiles = new List<ResourceFile>();

            // Set the expiry time and permissions for the blob shared access signature.
            // In this case, no start time is specified, so the shared access signature
            // becomes valid immediately
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(2),
                Permissions = SharedAccessBlobPermissions.Read
            };

            var files = Directory.GetFiles("JobBinary", "*.*");
            foreach (var file in files)
            {
                string blobName = Path.GetFileName(file);
                CloudBlockBlob blobData = container.GetBlockBlobReference(blobName);
                await blobData.UploadFromFileAsync(file);

                // Construct the SAS URL for blob
                string sasBlobToken = blobData.GetSharedAccessSignature(sasConstraints);
                string blobSasUri = String.Format("{0}{1}", blobData.Uri, sasBlobToken);

                resourceFiles.Add(new ResourceFile(blobSasUri, blobName));
            }

            return resourceFiles;
        }

        /// <summary>
        /// Deletes the specified containers
        /// </summary>
        /// <param name="storageAccount">The storage account with the containers to delete.</param>
        /// <param name="blobContainerName">The name of the containers created for the jobs resource files.</param>
        /// <returns>An asynchronous <see cref="Task"/> representing the operation.</returns>
        private async Task DeleteContainersAsync(CloudStorageAccount storageAccount)
        {
            var cloudBlobClient = storageAccount.CreateCloudBlobClient();
            var containers = cloudBlobClient.ListContainers(ContainerName);

            foreach (var c in containers)
            {
                await c.DeleteIfExistsAsync();
            }
        }
    }
}
