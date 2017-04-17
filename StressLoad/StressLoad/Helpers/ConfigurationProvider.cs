using System.Configuration;

namespace StressLoad
{
    public class ConfigurationProvider:IConfigurationProvider
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

    }
}
