using Microsoft.Azure.Batch;
using System.Threading.Tasks;

namespace StressLoad
{
    public interface IBatchConnector
    {
        string StorageConnectionString { get; }

        Task<bool> Deploy(TestJob testJob);

        Task<TestJobStatus> GetStatus(TestJob testJob);

        Task<bool> DeleteTest(TestJob testJob);

        Task<bool> DeleteTest(BatchClient client, TestJob testJob);
    }
}
