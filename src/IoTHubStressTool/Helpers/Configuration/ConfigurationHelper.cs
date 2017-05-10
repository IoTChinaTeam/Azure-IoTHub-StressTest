using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressLoadDemo.Helpers.Configuration
{
    public static class ConfigurationHelper
    {
        public static string ReadConfig(string configName, string defaultValue = "")
        {
            //if the config with configName is not in app.config. no exception will be thrown, only return null.

            var configValue = ConfigurationManager.AppSettings[configName];
            if (string.IsNullOrEmpty(configValue))
            {
                return defaultValue;
            }
            else
            {
                return configValue;
            }
        }
    }
}
