﻿namespace OJS.Workers.LocalWorker
{
    using System;
    using System.Configuration;

    using log4net;

    public static class Settings
    {
        private static readonly ILog Logger;

        static Settings()
        {
            Logger = LogManager.GetLogger("Settings");
        }

        public static string MavenPath => GetSetting("MavenPath");

        public static string CSharpCompilerPath => GetSetting("CSharpCompilerPath");

        public static string CPlusPlusGccCompilerPath => GetSetting("CPlusPlusGccCompilerPath");

        public static string NUnitConsoleRunnerPath => GetSetting("NUnitConsoleRunnerPath");

        public static string MsBuildExecutablePath => GetSetting("MsBuildExecutablePath");

        public static string NuGetExecutablePath => GetSetting("NuGetExecutablePath");

        public static string JavaCompilerPath => GetSetting("JavaCompilerPath");

        public static string JavaExecutablePath => GetSetting("JavaExecutablePath");

        public static string JavaLibsPath => GetSetting("JavaLibsPath");

        public static string RubyPath => GetSetting("RubyPath");

        public static string NodeJsExecutablePath => GetSetting("NodeJsExecutablePath");

        public static string MochaModulePath => GetSetting("MochaModulePath");

        public static string ChaiModulePath => GetSetting("ChaiModulePath");

        public static string JsDomModulePath => GetSetting("JsDomModulePath");

        public static string JQueryModulePath => GetSetting("JQueryModulePath");

        public static string HandlebarsModulePath => GetSetting("HandlebarsModulePath");

        public static string SinonModulePath => GetSetting("SinonModulePath");

        public static string SinonJsDomModulePath => GetSetting("SinonJsDomModulePath");

        public static string SinonChaiModulePath => GetSetting("SinonChaiModulePath");

        public static string UnderscoreModulePath => GetSetting("UnderscoreModulePath");

        public static string BrowserifyModulePath => GetSetting("BrowserifyModulePath");

        public static string BabelifyModulePath => GetSetting("BabelifyModulePath");

        public static string Es2015ImportPluginPath => GetSetting("ES2015ImportPluginPath");

        public static string BabelCoreModulePath => GetSetting("BabelCoreModulePath");

        public static string ReactJsxPluginPath => GetSetting("ReactJsxPluginPath");

        public static string ReactModulePath => GetSetting("ReactModulePath");

        public static string ReactDomModulePath => GetSetting("ReactDOMModulePath");

        public static string BootstrapModulePath => GetSetting("BootstrapModulePath");

        public static string BootstrapCssPath => GetSetting("BootstrapCssPath");

        public static string PythonExecutablePath => GetSetting("PythonExecutablePath");

        public static string PhpCgiExecutablePath => GetSetting("PhpCgiExecutablePath");

        public static string PhpCliExecutablePath => GetSetting("PhpCliExecutablePath");

        public static int NodeJsBaseTimeUsedInMilliseconds => GetSettingOrDefault("NodeJsBaseTimeUsedInMilliseconds", 0);

        public static int NodeJsBaseMemoryUsedInBytes => GetSettingOrDefault("NodeJsBaseMemoryUsedInBytes", 0);

        public static string SqlServerLocalDbMasterDbConnectionString => GetSetting("SqlServerLocalDbMasterDbConnectionString");

        public static string SqlServerLocalDbRestrictedUserId => GetSetting("SqlServerLocalDbRestrictedUserId");

        public static string SqlServerLocalDbRestrictedUserPassword => GetSetting("SqlServerLocalDbRestrictedUserPassword");

        public static string MySqlSysDbConnectionString => GetSetting("MySqlSysDbConnectionString");

        public static string MySqlRestrictedUserId => GetSetting("MySqlRestrictedUserId");

        public static string MySqlRestrictedUserPassword => GetSetting("MySqlRestrictedUserPassword");

        public static int ThreadsCount => GetSettingOrDefault("ThreadsCount", 2);

        private static string GetSetting(string settingName)
        {
            if (ConfigurationManager.AppSettings[settingName] == null)
            {
                Logger.FatalFormat("{0} setting not found in App.config file!", settingName);
                throw new Exception($"{settingName} setting not found in App.config file!");
            }

            return ConfigurationManager.AppSettings[settingName];
        }

        private static T GetSettingOrDefault<T>(string settingName, T defaultValue)
        {
            if (ConfigurationManager.AppSettings[settingName] == null)
            {
                return defaultValue;
            }

            return (T)Convert.ChangeType(ConfigurationManager.AppSettings[settingName], typeof(T));
        }
    }
}
