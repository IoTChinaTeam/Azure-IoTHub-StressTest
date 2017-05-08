using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressLoadDemo.Model.DataProvider.ReceiverTool
{
    public class SingletonReceiver
    {
        private static EventHubReceiver receiver;

        public static EventHubReceiver GetReceiver(EventHubConsumerGroup consumerGroup,string partitionId,DateTime startDateTimeUtc)
        {
            if (receiver != null)
            {
                return receiver;
            }
            else
            {
                receiver = consumerGroup.CreateReceiver(partitionId, startDateTimeUtc);
                return receiver;
            }
        }
    }
}
