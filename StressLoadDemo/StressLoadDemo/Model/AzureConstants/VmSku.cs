namespace StressLoadDemo.Model.AzureConstants
{
    public enum VmSize
    {
        small,  // 1 core /1.75G
        medium, // 2 core /3.5G
        large,  // 4 core / 7G
        extralarge  // 8 core / 14G
    }

    public class VmSku
    {
        public VmSize Size;
        public int VmCount;
    }
}
