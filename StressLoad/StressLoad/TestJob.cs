using System.Configuration;

namespace StressLoad
{
    public class TestJob
    {
        // use the same pool for now
        public const string BatchPoolId = "TestPool";

        // to use as table name/job name in batch service
        public string BatchJobId
        {
            get
            {
                return GetBatchJobId(this.JobId);
            }
        }

        public long JobId { get; set; }

        public TestJobStatus? Status { get; set; }

        public int NumofVm { get; set; }

        public SizeOfVMType SizeOfVM { get; set; }

        public int DevicePerVm { get; set; }

        public int MessagePerMin { get; set; }

        public int DurationInMin { get; set; }

        public string Message { get; set; }

        public string Transport { get; set; }

        public string DeviceClientEndpoint
        {
            get; set;
        }

        public static string GetBatchJobId(long jobId)
        {
            return string.Format(ConfigurationManager.AppSettings["DeviceIdPrefix"] + jobId.ToString());
        }
    }
}
