using StressLoadDemo.Helpers.Configuration;
using StressLoadDemo.Model.AzureConstants;
using GalaSoft.MvvmLight.Messaging;

namespace StressLoadDemo.Model.AzureObject
{
    public class TestJob
    {
        // use the same pool for now
        public const string BatchPoolId = "TestPool";

        public IConfigurationProvider ConfigureProvider { get; set; }

        // to use as table name/job name in batch service
        public string BatchJobId
        {
            get
            {
                return GetBatchJobId(this.JobId,ConfigureProvider);
            }
        }

        public long JobId { get; set; }

        public TestJobStatus? Status { get; set; }

        public int NumofVm { get; set; }

        public VmSize SizeOfVM { get; set; }

        public int DevicePerVm { get; set; }

        public int MessagePerMin { get; set; }

        public int DurationInMin { get; set; }

        public string Message { get; set; }

        public string Transport { get; set; }

        public string DeviceClientEndpoint
        {
            get; set;
        }

        public static string GetBatchJobId(long jobId, IConfigurationProvider provider = null)
        {
            provider = provider ?? new InternalProvider();
            var jobid= string.Format(provider.GetConfigValue("DeviceIdPrefix") + jobId.ToString());
            Messenger.Default.Send(jobid,"BatchJobId");
            return jobid;
        }
    }
}
