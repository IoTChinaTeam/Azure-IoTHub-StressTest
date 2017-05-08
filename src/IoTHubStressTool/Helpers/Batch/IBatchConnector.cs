using Microsoft.Azure.Batch;
using StressLoadDemo.Model.AzureObject;
using System.Threading.Tasks;

namespace StressLoadDemo.Helpers.Batch
{
    public interface IBatchConnector
    {
        string StorageConnectionString { get; }

        Task<bool> Deploy(TestJob testJob);

        Task<bool> DeleteTest(TestJob testJob);

        Task<bool> DeleteTest(BatchClient client, TestJob testJob);
    }

}
