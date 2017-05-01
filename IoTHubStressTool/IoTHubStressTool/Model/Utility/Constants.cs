using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressLoadDemo.Model.Utility
{
    public static class Constants
    {
        public const string BatchKey_ConfigName = "Batchkey";
        public const string HubOwnerConectionString_ConfigName = "HubOwnerConnectionString";
        public const string EventHubEndpoint_ConfigName = "EventHubEndpoint";
        public const string BatchUrl_ConfigName = "BatchUrl";
        public const string StorageAccountConectionString_ConfigName = "StorageAccountConnectionString";
        public const string TotalDevice_ConfigName = "TotalDevice";
        public const string MessageFreq_ConfigName = "MessagePerMinPerDevice";
        public const string ExpectDuration_ConfigName = "ExpectDuration";
    }
}
