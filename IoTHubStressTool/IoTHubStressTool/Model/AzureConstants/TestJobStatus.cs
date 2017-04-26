namespace StressLoadDemo.Model.AzureConstants
{
    public enum TestJobStatus
    {
        Unknown,
        Created,
        Provisioning,
        Enqueued,
        Running,
        Finished,
        Failed,
        VerificationPassed,
        VerificationFailed
    }
}
