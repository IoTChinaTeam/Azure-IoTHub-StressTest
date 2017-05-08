using Microsoft.Azure.Batch;
using StressLoadDemo.Model.AzureObject;
using System.Threading.Tasks;

namespace StressLoadDemo.Helpers.Batch
{
      public static class BatchHelper
    {
        private static IBatchConnector connector;

        // in test, we can use mock connector
        public static IBatchConnector Connector
        {
            get
            {
                if (connector == null)
                {
                    connector = new BatchConnector();
                }

                return connector;
            }
            set
            {
                connector = value;
            }
        }

        public static async Task<bool> Deploy(this TestJob testJob)
        {
            return await Connector.Deploy(testJob);
        }

        public static async Task<bool> DeleteTest(this TestJob testJob)
        {
            return await Connector.DeleteTest(testJob);
        }

        public static async Task<bool> DeleteTest(this BatchClient client, TestJob testJob)
        {
            return await Connector.DeleteTest(client, testJob);
        }
    }
}
