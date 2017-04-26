using Microsoft.Azure.Devices;
using System;
using System.Linq;

namespace DeviceLoad
{
    class Setting
    {
        private Setting()
        {
        }

        public static Setting Parse(string[] args)
        {
            try
            {
                if (args.Length != 9)
                {
                    throw new ArgumentException("Not supported format");
                }

                Setting setting = new Setting
                {
                    DeviceClientEndpoint = args[1].Trim('"'),
                    DevicePerVm = int.Parse(args[2]),
                    MessagePerMin = int.Parse(args[3]),
                    DurationInMin = int.Parse(args[4]),
                    BatchJobId = args[5],
                    DeviceIdPrefix = args[6].Trim('"'),
                    Message = args[7].Trim('"'),
                    Transport = args[8],
                };

                setting.IotHubManager = RegistryManager.CreateFromConnectionString(setting.DeviceClientEndpoint);
                setting.IoTHubHostName = setting.DeviceClientEndpoint.Split(';').Single(s => s.StartsWith("HostName=")).Substring(9);

                return setting;
            }
            catch (Exception)
            {
                Console.WriteLine("Register devices and send message to IotHub in given interval and given format.");
                Console.WriteLine("");
                Console.WriteLine("DeviceLoad.exe {DeviceClientEndpoint} {DevicePerVm} {MessagePerMin} {DurationInMin} {BatchJobId} {DeviceIdPrefix} {MessageFormat} {Transport}");
                Console.WriteLine("");
                Console.WriteLine("DeviceClientEndpoint: HostName={http://xx.chinacloudapp.cn};SharedAccessKeyName=owner;SharedAccessKey={key}");
                Console.WriteLine("DeviceIdPrefix: It is used to register device: {DeviceIdPrefix}-0000, {DeviceIdPrefix}-{DevicePerVm}.");
                Console.WriteLine("Message: {\"value\": %value%,\"datetime\": \"%datetime%\"}");

                return null;
            }
        }

        public string DeviceClientEndpoint { get; private set; }

        public int DevicePerVm { get; private set; }

        public int MessagePerMin { get; private set; }

        public int DurationInMin { get; private set; }

        public string BatchJobId { get; private set; }

        public string DeviceIdPrefix { get; private set; }

        public string Message { get; private set; }

        public string Transport { get; private set; }

        public RegistryManager IotHubManager { get; private set; }

        public string IoTHubHostName { get; private set; }
    }
}
