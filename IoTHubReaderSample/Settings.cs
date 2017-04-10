using Microsoft.Azure.Devices;
using System;
using System.Linq;

namespace IoTHubReaderSample
{
    class Settings
    {
        public string ConnectionString { get; private set; }
        public string Path { get; private set; }
        public string PartitionId { get; private set; }
        public string GroupName { get; private set; }
        public DateTime StartingDateTimeUtc { get; private set; }

        public Settings(CommandArguments parsedArguments)
        {
            var builder = IotHubConnectionStringBuilder.Create(parsedArguments.ConnectionString);

            ConnectionString = $"Endpoint={parsedArguments.EventHubEndpoint};SharedAccessKeyName={builder.SharedAccessKeyName};SharedAccessKey={builder.SharedAccessKey}";
            Path = builder.HostName.Split('.').First();
            PartitionId = parsedArguments.PartitionId;
            GroupName = parsedArguments.GroupName;
            StartingDateTimeUtc = DateTime.UtcNow - TimeSpan.FromMinutes(parsedArguments.OffsetInMinutes);
        }
    }
}
