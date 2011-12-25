using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.TransientFaultHandling;
using System.Configuration;

namespace ProjectExtensions.Azure.ServiceBus.Helpers {
    
    /// <summary>
    /// Helper for Azure configuration
    /// </summary>
    public static class AzureConfigurationHelper {

        //static bool roleNotAvailable;

        /// <summary>
        /// Gets a configuration value.
        /// </summary>
        /// <param name="configKey"></param>
        /// <returns></returns>
        public static string GetConfig(string configKey) {
            Guard.ArgumentNotNull(configKey, "configKey");
            try {
                //if (!roleNotAvailable && RoleEnvironment.IsAvailable) {
                //    return RoleEnvironment.GetConfigurationSettingValue(configKey);
                //}
                return ConfigurationManager.AppSettings[configKey];
            }
            catch (Exception) {
                //roleNotAvailable = true;
                return ConfigurationManager.AppSettings[configKey];
            }
        }

        /// <summary>
        /// Gets configuration value as a boolean.
        /// </summary>
        /// <param name="configKey"></param>
        /// <returns></returns>
        public static bool GetBoolConfig(string configKey) {
            var s = GetConfig(configKey);
            bool configVal;
            bool.TryParse(s, out configVal);
            return configVal;
        }

        /// <summary>
        /// Gets configuration value as a integer.
        /// </summary>
        /// <param name="configKey"></param>
        /// <returns></returns>
        public static int GetIntConfig(string configKey) {
            var s = GetConfig(configKey);
            int configVal;
            int.TryParse(s, out configVal);
            return configVal;
        }
    }
}
