using GalaSoft.MvvmLight.Messaging;
using StressLoadDemo.Helpers.Batch;
using StressLoadDemo.Helpers.Configuration;
using StressLoadDemo.Model.AzureConstants;
using StressLoadDemo.Model.AzureObject;
using System;
using System.Threading.Tasks;

namespace StressLoadDemo.Model.DataProvider
{
    public class StressLoadDataProvider : IStressDataProvider
    {
        public string HubOwnerConectionString { get; set; }
        public string EventHubEndpoint { get; set; }
        public string StorageAccountConectionString { get; set; }
        public string BatchUrl { get; set; }
        public string BatchKey { get; set; }
        public string DevicePerVm { get; set; }
        public string NumOfVm { get; set; }
        public string ExpectTestDuration { get; set; }
        public int MessagePerMinute { get; set; }
        public string BatchJobId { get; set; }
        public string VmSize { get; set; }

        public StressLoadDataProvider()
        {
            //register messenger to know where we are during deployment
        }


        public void Run(IConfigurationProvider provider = null)
        {
            provider = provider ?? new InternalProvider();
            provider.PutConfigValue("NumofVm", NumOfVm);
            provider.PutConfigValue("DevicePerVm", DevicePerVm);
            provider.PutConfigValue("SizeOfVM", VmSize);
            provider.PutConfigValue("MessagePerMin", MessagePerMinute.ToString());
            provider.PutConfigValue("DurationInMin", ExpectTestDuration);
            provider.PutConfigValue("DeviceIdPrefix", "TestDevice");
            provider.PutConfigValue("Message", "");
            provider.PutConfigValue("BatchServiceUrl", BatchUrl);
            provider.PutConfigValue("BatchAccountKey", BatchKey);
            provider.PutConfigValue("StorageConnectionString", StorageAccountConectionString);
            provider.PutConfigValue("DeviceClientEndpoint", HubOwnerConectionString);
            provider.PutConfigValue("EventHubEndpoint", EventHubEndpoint);
            provider.PutConfigValue("Transport", "Mqtt");
            DeployAndStartStressLoad(provider);
        }

        async Task DeployAndStartStressLoad(IConfigurationProvider provider)
        {
            try
            {
                var devicePerVm = int.Parse(provider.GetConfigValue("DevicePerVm"));
                var numofVM = int.Parse(provider.GetConfigValue("NumofVm"));
                TestJob job = new TestJob
                {
                    JobId = DateTime.UtcNow.Ticks,
                    ConfigureProvider = provider,
                    DevicePerVm = devicePerVm,
                    Message = provider.GetConfigValue("Message"),
                    Transport = provider.GetConfigValue("Transport"),
                    MessagePerMin = int.Parse(provider.GetConfigValue("MessagePerMin")),
                    NumofVm = numofVM,
                    SizeOfVM = (VmSize)Enum.Parse(typeof(VmSize), provider.GetConfigValue("SizeOfVM")),
                    DeviceClientEndpoint = provider.GetConfigValue("DeviceClientEndpoint"),
                    DurationInMin = int.Parse(provider.GetConfigValue("DurationInMin"))
                };
                var batch = new BatchConnector(provider);
                BatchHelper.Connector = batch;
                await job.Deploy();
                //StartRead();
                await job.DeleteTest();
            }
            catch(Exception e)
            {
                Messenger.Default.Send($"Terminated Due to error '{e.Message}', please check all your inputs are correct.", "RunningLog");
            }
        }
    }
}
