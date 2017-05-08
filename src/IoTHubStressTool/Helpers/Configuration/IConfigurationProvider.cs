namespace StressLoadDemo.Helpers.Configuration
{
    public interface IConfigurationProvider
    {
        string GetConfigValue(string configName);
        void PutConfigValue(string configName, string configValue);
    }
}
