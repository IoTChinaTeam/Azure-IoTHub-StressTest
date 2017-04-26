namespace StressLoadDemo.Model.AzureConstants
{
    public enum HubSize
    {
        S1,
        S2,
        S3
    }

    public struct HubSku
    {
        public HubSize UnitSize;
        public int UnitCount;
    }
}
