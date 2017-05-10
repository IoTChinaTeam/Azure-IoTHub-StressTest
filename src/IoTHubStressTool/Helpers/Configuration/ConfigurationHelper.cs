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
            try
            {
                return ConfigurationManager.AppSettings[configName];
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
