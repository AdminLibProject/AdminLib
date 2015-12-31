using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace AdminLib.Application {
    public static class Config {

        public static Configuration configuration;

        /// <summary>
        ///     Default connection string.
        ///     If no connection string defined, then equal to NULL
        /// </summary>
        public static string defaultConnectionString {get; private set;}

        /// <summary>
        ///     Default path in wich the uploaded files will be saved.
        /// </summary>
        public static string defaultUploadPath {get; private set;}

        private static bool initialized = false;

        /******************** Static methods ********************/

        /// <summary>
        ///     Return the value of an app setting entry
        /// </summary>
        /// <param name="name">Name of the entry</param>
        /// <returns></returns>
        public static string GetApplicationSetting(string name) {

            KeyValueConfigurationElement setting;

            if (Config.configuration == null)
                return ConfigurationManager.AppSettings[name];

            setting = Config.configuration.AppSettings.Settings[name];

            if (setting == null)
                return null;
            
            return setting.Value;
        }

        /// <summary>
        ///     Return a connection string
        /// </summary>
        /// <param name="index">Index of the connection string</param>
        /// <returns></returns>
        public static string GetConnectionString(int index) {

            ConnectionStringSettings connectionStringSetting;

            if (Config.configuration != null)
                connectionStringSetting = Config.configuration.ConnectionStrings.ConnectionStrings[index];
            else
                connectionStringSetting = ConfigurationManager.ConnectionStrings[index];

            if (connectionStringSetting == null)
                return null;

            return connectionStringSetting.ConnectionString;
        }

        /// <summary>
        ///     Initialize the static properties.
        ///     If no configuration file path is provided, then it will use the current config file.
        /// </summary>
        /// <param name="configPath">Configuration file to use</param>
        public static void Initialize(string configPath = null) {

            ExeConfigurationFileMap map;

            if (Config.initialized)
                throw new Exception("Config class already initialized");

            // Loading the configuration file
            if (configPath != null) {
                map                  = new ExeConfigurationFileMap { ExeConfigFilename = configPath };
                Config.configuration = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            }

            // defaultConnectionString
            Config.defaultConnectionString = Config.GetConnectionString(0);

            // defaultUploadRootPath
            Config.defaultUploadPath = Config.GetApplicationSetting("defaultUploadPath");

            Config.initialized = true;
        }

    }
}