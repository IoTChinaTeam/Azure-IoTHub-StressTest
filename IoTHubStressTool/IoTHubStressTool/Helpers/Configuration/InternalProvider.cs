using System.Collections.Generic;
using System.Linq;

namespace StressLoadDemo.Helpers.Configuration
{
    class InternalProvider : IConfigurationProvider
    {
        Dictionary<string, string> configurations;
        public InternalProvider()
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
