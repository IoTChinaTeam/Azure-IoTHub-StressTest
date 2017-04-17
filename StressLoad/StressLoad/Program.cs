using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StressLoad
{
    class Program
    {
        
        static void Main(string[] args)
        {
            IConfigurationProvider provider = new ConfigurationProvider();
            SizeValidateHelper.ValidateSizeOfResources(provider);
            RunStressLoad(provider,args);
            Console.WriteLine("Complete the test");
        }

        private static void RunStressLoad(IConfigurationProvider provider,string[]args)
        {
            // if necessary
            if (args.Length > 0 && args[0] == "clean")
            {
                RemoveDevices(provider).Wait();
            }
            var devicePerVm = int.Parse(provider.GetConfigValue("DevicePerVm"));
            var numofVM = int.Parse(provider.GetConfigValue("NumofVm"));
            TestJob job = new TestJob
            {
                JobId = DateTime.UtcNow.Ticks,
                DevicePerVm = devicePerVm,
                Message = provider.GetConfigValue("Message"),
                Transport = provider.GetConfigValue("Transport"),
                MessagePerMin = int.Parse(provider.GetConfigValue("MessagePerMin")),
                NumofVm = numofVM,
                SizeOfVM = (SizeOfVMType)Enum.Parse(typeof(SizeOfVMType), provider.GetConfigValue("SizeOfVM")),
                DeviceClientEndpoint = provider.GetConfigValue("DeviceClientEndpoint"),
                DurationInMin = int.Parse(provider.GetConfigValue("DurationInMin"))
            };

            job.Deploy().Wait();
            job.DeleteTest().Wait();
        }

        static async Task RemoveDevices(IConfigurationProvider provider = null)
        {
            if (provider == null)
            {
                provider = new ConfigurationProvider();
            }
            Console.WriteLine($"Start to remove devices.");

            var deviceClientEndpoint = provider.GetConfigValue("DeviceClientEndpoint");
            var iotHubManager = RegistryManager.CreateFromConnectionString(deviceClientEndpoint);

            var sw = Stopwatch.StartNew();
            int count = 0;
            while (true)
            {
                var devices1 = new List<Device>(await iotHubManager.GetDevicesAsync(10000));
                if (devices1.Count == 0)
                {
                    break;
                }
                while (devices1.Count > 0)
                {
                    var length = devices1.Count < 100 ? devices1.Count : 100;
                    count += length;
                    await iotHubManager.RemoveDevices2Async(devices1.GetRange(0, length));
                    devices1.RemoveRange(0, length);

                    Console.WriteLine($"{sw.Elapsed.TotalSeconds}:{count} devices removed.");
                }
            }
        }
    }
}
