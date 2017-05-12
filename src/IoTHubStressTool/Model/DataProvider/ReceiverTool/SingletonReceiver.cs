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
            if (receiver != null&&receiver.PartitionId == partitionId)
            {
                return receiver;
            }
            else
            {
                receiver?.Close();
                receiver = consumerGroup.CreateReceiver(partitionId, startDateTimeUtc);
                return receiver;
            }
        }
    }
}
