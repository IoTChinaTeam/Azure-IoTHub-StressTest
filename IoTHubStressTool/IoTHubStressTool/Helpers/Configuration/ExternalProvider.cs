using System.Configuration;

namespace StressLoadDemo.Helpers.Configuration
{
    public class ExternalProvider : IConfigurationProvider
    {
        public string GetConfigValue(string configName)
        {
            try
            {
                return ConfigurationManager.AppSettings[configName];
            }
            catch
            {
                return string.Empty;
            }
        }

        public void PutConfigValue(string configName, string configValue)
        {
            //not implemented.
        }
    }
}
