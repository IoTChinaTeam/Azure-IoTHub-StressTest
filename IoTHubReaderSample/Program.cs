using CommandLine;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace IoTHubReaderSample
{
    class CommandArguments
    {
        [Argument(ArgumentType.Required, ShortName = "c", HelpText = "IoT Hub connection string")]
        public string ConnectionString;

        [Argument(ArgumentType.Required, ShortName = "e", HelpText = "Event Hub-compatible endpoint")]
        public string EventHubEndpoint;

        [Argument(ArgumentType.AtMostOnce, ShortName = "p", DefaultValue = "0", HelpText = "The ID of the interesting partition")]
        public string PartitionId;

        [Argument(ArgumentType.AtMostOnce, ShortName = "g", DefaultValue = "$Default", HelpText = "Consumer group")]
        public string GroupName;

        [Argument(ArgumentType.AtMostOnce, ShortName = "o", DefaultValue = 60, HelpText = "The starting offset for receiving messages")]
        public int OffsetInMinutes;

        public bool ReadFromSettings()
        {
            string value;

            value = ConfigurationManager.AppSettings["ConnectionString"];
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }
            ConnectionString = value;

            value = ConfigurationManager.AppSettings["EventHubEndpoint"];
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }
            EventHubEndpoint = value;

            value = ConfigurationManager.AppSettings["PartitionId"];
            PartitionId = string.IsNullOrWhiteSpace(value) ? "0" : value;

            value = ConfigurationManager.AppSettings["GroupName"];
            GroupName = string.IsNullOrWhiteSpace(value) ? "$Default" : value;

            value = ConfigurationManager.AppSettings["Offset"];
            try
            {
                OffsetInMinutes = (int)TimeSpan.Parse(value).TotalMinutes;
            }
            catch
            {
                OffsetInMinutes = 60;
            }

            return true;
        }
    }

    class Program
    {
        private static readonly TimeSpan outputInterval = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan receiveTimeout = TimeSpan.FromSeconds(10);

        static void Main(string[] args)
        {
            Console.WriteLine("IoTHub reader sample");

            var parsedArguments = new CommandArguments();
            if (!parsedArguments.ReadFromSettings())
            {
                if (!Parser.ParseArgumentsWithUsage(args, parsedArguments))
                {
                    return;
                }
            }

            var settings = new Settings(parsedArguments);
            var indicators = new Indicators();
            indicators.Reset();

            var cts = new CancellationTokenSource();
            var task = ReceiveLoop(settings, indicators, cts.Token);
            var stopwatch = Stopwatch.StartNew();

            while (true)
            {
                Thread.Sleep(outputInterval);

                Console.WriteLine($"{DateTime.Now}, running for {stopwatch.Elapsed}");
                Console.WriteLine($"PartitionId:              {settings.PartitionId}");
                Console.WriteLine($"Total # of messages:      {indicators.TotalMessages}, in last minute: {indicators.DeviceToIoTHubDelay.Count}");
                Console.WriteLine($"Overall throughput:       {(int)(indicators.TotalMessages / stopwatch.Elapsed.TotalMinutes)} messages/minute");
                Console.WriteLine($"Total # of devices:       {indicators.TotalDevices}");
                Console.WriteLine($"Avg. device-IoTHub delay: {FormatDelay(indicators.DeviceToIoTHubDelay.StreamAvg)}, in last minute: {FormatDelay(indicators.DeviceToIoTHubDelay.WindowAvg)}");
                Console.WriteLine($"Avg. E2E delay:           {FormatDelay(indicators.E2EDelay.StreamAvg)}, in last minute: {FormatDelay(indicators.E2EDelay.WindowAvg)}");
                Console.WriteLine($"Sample event content:     {indicators.SampleEvent} from [{indicators.SampleEventSender}]");
                Console.WriteLine();
            }
        }

        private static async Task ReceiveLoop(Settings settings, Indicators indicators, CancellationToken ct)
        {
            Console.WriteLine($"Opening receiver on IoT Hub '{settings.Path}', partition {settings.PartitionId}, consumer group '{settings.GroupName}', StartingDateTimeUtc = {settings.StartingDateTimeUtc}");

            var client = EventHubClient.CreateFromConnectionString(settings.ConnectionString, settings.Path);
            var consumerGroup = client.GetConsumerGroup(settings.GroupName);
            var receiver = await consumerGroup.CreateReceiverAsync(settings.PartitionId, settings.StartingDateTimeUtc);

            Console.WriteLine("Open succeeded\r\n");

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var eventData = await receiver.ReceiveAsync(receiveTimeout);
                    if (eventData == null)
                    {
                        Console.WriteLine($"* No event received for {receiveTimeout}, retry later\r\n");
                        continue;
                    }

                    indicators.Push(eventData);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception raised: {ex}");
                }
            }
        }

        private static string FormatDelay(double value)
        {
            return double.IsNaN(value) ? "N/A" : TimeSpan.FromMilliseconds(value).ToString();
        }
    }
}
