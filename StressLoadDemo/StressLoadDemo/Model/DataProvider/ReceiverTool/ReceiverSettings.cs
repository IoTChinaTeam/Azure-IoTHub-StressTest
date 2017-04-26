using System;

namespace StressLoadDemo.Model.DataProvider.ReceiverTool
{
    class ReceiverSettings
    {
        public string ConnectionString { get; private set; }
        public string Path { get; private set; }
        public string PartitionId { get; private set; }
        public string GroupName { get; private set; }
        public DateTime StartingDateTimeUtc { get; private set; }
    }
}
