using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using StressLoad;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DeviceLoad
{
    class Program
    {
        private static Microsoft.Azure.Devices.Client.TransportType transport;
        private static Setting setting;
        private static Stopwatch wallclock;

        // Example:
        // DeviceLoad.exe {OutputStorageConnectionString} {DeviceClientEndpoint} {DevicePerVm} {MessagePerMin} {DurationInMin} {BatchJobId} {DeviceIdPrefix} [MessageFormat]
        // OutputStorageConnectionString: DefaultEndpointsProtocol=https;AccountName={name};AccountKey={key};EndpointSuffix={core.windows.net}
        // DeviceClientEndpoint: HostName={http://xx.chinacloudapp.cn};SharedAccessKeyName=owner;SharedAccessKey={key}
        static void Main(string[] args)
        {
            setting = Setting.Parse(args);
            if (setting == null)
            {
                Environment.Exit(-1);
            }

            ServicePointManager.DefaultConnectionLimit = 200;

            if (!Enum.TryParse(setting.Transport, out transport))
            {
                transport = Microsoft.Azure.Devices.Client.TransportType.Mqtt;
            }

            wallclock = Stopwatch.StartNew();
            var devices = CreateDevices().Result;
            SendMessages(devices).Wait();
        }

        static async Task<List<Device>> CreateDevices()
        {
            var createdDevices = new List<Device>();

            var devices = new List<Device>();
            for (int i = 0; i < setting.DevicePerVm; i++)
            {
                devices.Add(GenerateDevice(i));

                if (devices.Count == 100)
                {
                    createdDevices.AddRange(devices);
                    await AddDevices(devices);
                    devices.Clear();
                    Console.WriteLine($"{wallclock.Elapsed}: Created {createdDevices.Count} devices.");
                }
            }

            if (devices.Count > 0)
            {
                createdDevices.AddRange(devices);
                await AddDevices(devices);
                devices.Clear();
                Console.WriteLine($"{wallclock.Elapsed}: Created {createdDevices.Count} devices.");
            }

            return createdDevices;
        }

        private static async Task AddDevices(List<Device> devices)
        {
            while (true)
            {
                try
                {
                    var result = await setting.IotHubManager.AddDevices2Async(devices);
                    if (result.Errors == null ||
                        result.Errors.Length == 0 ||
                        result.Errors[0].ErrorCode == Microsoft.Azure.Devices.Common.Exceptions.ErrorCode.DeviceAlreadyExists)
                    {
                        break;
                    }

                    Console.WriteLine($"Create failed: {result.Errors[0].ErrorStatus}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Create failed: {ex.Message}.");
                }

                Task.Delay(100).Wait();

            };
        }

        private static Device GenerateDevice(int i)
        {
            var deviceId = setting.DeviceIdPrefix + "-" + i.ToString().PadLeft(10, '0');
            var device = new Device(deviceId)
            {
                Authentication = new AuthenticationMechanism
                {
                    SymmetricKey = new SymmetricKey
                    {
                        PrimaryKey = DefaultKey.Primary,
                        SecondaryKey = DefaultKey.Secondary
                    }
                }
            };

            return device;
        }
 
        static async Task SendMessages(List<Device> devices)
        {
            using (var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total"))
            using (var ramCounter = new PerformanceCounter("Memory", "Available MBytes"))
            {
                Console.WriteLine($"Opening clients for {devices.Count()} devices, transport = {transport}");
                var clients = await OpenClientsAsync(devices, cpuCounter, ramCounter);

                var tasks = clients.Select(pair => DeviceToCloudMessage(pair.Key.Id, DeviceConnectionString(setting.IoTHubHostName, pair.Key), pair.Value)).ToList();
                var wrapperTask = Task.WhenAll(tasks);

                do
                {
                    Console.WriteLine($"{wallclock.Elapsed}: {tasks.Count(t => !t.IsCompleted)} devices are running. Resource usage: CPU usage {cpuCounter.NextValue()}%, available memory {ramCounter.NextValue()}MB");
                }
                while (!wrapperTask.Wait(TimeSpan.FromSeconds(5)));
            }
        }

        private static async Task<IEnumerable<KeyValuePair<Device, DeviceClient>>> OpenClientsAsync(IEnumerable<Device> devices, PerformanceCounter cpuCounter, PerformanceCounter ramCounter)
        {
            var clients = new Dictionary<Device, DeviceClient>();

            foreach (var device in devices)
            {
                var client = await OpenDeviceClientAsync(device);
                clients.Add(device, client);

                if (clients.Count() % 100 == 0)
                {
                    Console.WriteLine($"{wallclock.Elapsed}: {clients.Count} devices were opened. Resource usage: CPU usage {cpuCounter.NextValue()}%, available memory {ramCounter.NextValue()}MB");
                }
            }

            return clients.Where(pair => pair.Value != null);
        }

        private static async Task<DeviceClient> OpenDeviceClientAsync(Device device)
        {
            var connectionString = DeviceConnectionString(setting.IoTHubHostName, device);
            var client = DeviceClient.CreateFromConnectionString(connectionString, transport);

            try
            {
                await client.OpenAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception raised while trying to open device {device.Id}: {ex}");
                client.Dispose();
                client = null;
            }

            return client;
        }

        static async Task DeviceToCloudMessage(string deviceId, string deviceConnectionString, DeviceClient deviceClient)
        {
            var currentDataValue = 0;
            var messageTemplate = setting.Message;
            if (string.IsNullOrEmpty(messageTemplate))
            {
                messageTemplate = "{\"DeviceData\": %value%,\"DeviceUtcDatetime\": \"%datetime%\"}";
            }

            var stopWatch = Stopwatch.StartNew();
            long lastCheckpoint;
            long interval = 60000 / setting.MessagePerMin;    // in miliseconds
            long expectedNumofMessage = setting.MessagePerMin * setting.DurationInMin;

            long count = 0;
            Random rnd = new Random();
            while (stopWatch.Elapsed.TotalMinutes <= setting.DurationInMin)
            {
                lastCheckpoint = stopWatch.ElapsedMilliseconds;

                currentDataValue = rnd.Next(-20, 20);
                string messageString = messageTemplate;
                messageString = messageString.Replace("%deviceId%", deviceId);
                messageString = messageString.Replace("%value%", currentDataValue.ToString());
                messageString = messageString.Replace("%datetime%", DateTime.UtcNow.ToString());

                var message = new Microsoft.Azure.Devices.Client.Message(Encoding.UTF8.GetBytes(messageString));

                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        await deviceClient.SendEventAsync(message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{stopWatch.Elapsed}: {deviceId} send message failed: {ex}");
                        await Task.Delay(1000);

                        deviceClient.Dispose();
                        deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, transport);
                        continue;
                    }

                    count++;
                    break;
                }

                // add 200ms 
                var delayTime = interval - (stopWatch.ElapsedMilliseconds - lastCheckpoint) - 200;
                if (delayTime > 0)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(delayTime));
                }
            }

            Console.WriteLine($"{stopWatch.Elapsed}: {deviceId} - Finish sending {count} message (expected: {expectedNumofMessage})");
        }

        static string DeviceConnectionString(string ioTHubHostName, Device device)
        {
            return string.Format("HostName={0};CredentialScope=Device;DeviceId={1};SharedAccessKey={2}",
                ioTHubHostName,
                device.Id,
                device.Authentication.SymmetricKey.PrimaryKey);
        }
    }
}
