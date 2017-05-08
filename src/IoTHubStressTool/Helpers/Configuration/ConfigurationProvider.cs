using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace StressLoadDemo.Helpers.Configuration
{
    class ConfigurationProvider : IConfigurationProvider
    {
        Dictionary<string, string> configurations;
        public ConfigurationProvider()
        {
            configurations = new Dictionary<string, string>();
        }

        public string GetConfigValue(string configName)
        {
            return configurations.First(p => p.Key == configName).Value;
        }

        public void PutConfigValue(string configName, string configValue)
        {
            configurations.Add(configName, configValue);
        }
    }
}
