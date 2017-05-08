using System;

namespace StressLoadDemo.Model.DataProvider.ReceiverTool
{
    class Settings
    {
        public string ConnectionString { get;  set; }
        public string Path { get;  set; }
        public string PartitionId { get;  set; }
        public string GroupName { get;  set; }
        public DateTime StartingDateTimeUtc { get;  set; }

        public Settings()
        {
        }

    }
}
