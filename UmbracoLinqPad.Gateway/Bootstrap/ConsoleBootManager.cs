using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Caching;
using umbraco.interfaces;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;

namespace UmbracoLinqPad.Gateway.Bootstrap
{
    public class ConsoleBootManager : CoreBootManager
    {
        private readonly DirectoryInfo _umbracoFolder;
        private Configuration _config;

        public ConsoleBootManager(UmbracoApplicationBase umbracoApplication, DirectoryInfo umbracoFolder)
            : base(umbracoApplication)
        {
            _umbracoFolder = umbracoFolder;
        }

        /// <summary>
        /// Fires first in the application startup process before any customizations can occur
        /// </summary>
        /// <returns/>
        public override IBootManager Initialize()
        {
            //Go read the umbraco configuration, get the umbracoSettings and set it dynamically
            var configFile = new FileInfo(Path.Combine(_umbracoFolder.FullName, "web.config"));
            var configMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = configFile.FullName
            };
            _config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

            ConfigureUmbracoSettings(_config);            
            ConfigureConnectionStrings(_config);
            ConfigureAppSettings(_config);
            
            //Few folders that need to exist
            System.IO.Directory.CreateDirectory(IOHelper.MapPath("~/App_Plugins"));

            return base.Initialize();
        }

        public override IBootManager Startup(Action<ApplicationContext> afterStartup)
        {
            //This is a special case and is due to a timing issue, we need to wait until the appCtx singleton is created
            //and then we can forcefully configure the file system providers since unfortunatley some of those rely on the singleton
            ConfigureFileSystemProviders(_config);

            return base.Startup(afterStartup);
        }

        /// <summary>
        /// Disables all application level cache
        /// </summary>
        protected override CacheHelper CreateApplicationCache()
        {
            return CacheHelper.CreateDisabledCacheHelper();
        }

        /// <summary>
        /// The main problem with booting umbraco is all of the startup handlers that will not work without a web context or in a standalone
        /// mode. So this code removes all of those handlers. We of course need some of them so this attempts to just keep the startup handlers
        /// declared inside of Umbraco.Core.
        /// </summary>
        protected override void InitializeApplicationEventsResolver()
        {
            base.InitializeApplicationEventsResolver();
            
            var appEventsResolver = ApplicationEventsResolver.Current;
            //get the legacy resolver, unfortunately this needs reflection currently - this is because the core's RemoveType functionality
            //doesn't also filter the legacy ones which it needs to do (will be done soon)            
            var legacyAppEventsResolver = (ManyObjectsResolverBase<ApplicationEventsResolver, IApplicationStartupHandler>)appEventsResolver.GetFieldValue("_legacyResolver");

            //now we want to get all IApplicationStartupHandlers from the PluginManager
            var startupHandlers = PluginManager.Current.ResolveTypes<IApplicationStartupHandler>();
            //for now we're just going to remove any type that does not exist in Umbraco.Core
            foreach (var startupHandler in startupHandlers
                .Where(x => x.Namespace != null)
                .Where(x => !x.Namespace.StartsWith("Umbraco.Core")))
            {
                //This is a special case because we have legacy handlers that are not of type IApplicationEventHandler and only 
                // of type IUmbracoStartupHandler which will throw if we try to remove them here because those are handled on
                // an internal object inside of ApplicationEventsResolver. 
                if (typeof (IApplicationEventHandler).IsAssignableFrom(startupHandler))
                {
                    appEventsResolver.RemoveType(startupHandler);
                }
                else if (typeof(IApplicationStartupHandler).IsAssignableFrom(startupHandler))
                {
                    //remove all legacy handlers
                    legacyAppEventsResolver.RemoveType(startupHandler);
                }
            }
        }

        private void ConfigureConnectionStrings(Configuration config)
        {
            //Important so things like SQLCE works
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(_umbracoFolder.FullName, "App_Data"));

            //Hack to be able to set configuration strings at runtime, needs reflection due to how MS built it
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var readonlyField = typeof(ConfigurationElementCollection).GetField("bReadOnly", flags);
            readonlyField.SetValue(ConfigurationManager.ConnectionStrings, false);

            foreach (var connectionString in config.ConnectionStrings.ConnectionStrings.Cast<ConnectionStringSettings>())
            {
                ConfigurationManager.ConnectionStrings.Add(connectionString);
            }

            readonlyField.SetValue(ConfigurationManager.ConnectionStrings, true);
        }

        private void ConfigureAppSettings(Configuration config)
        {
            foreach (var setting in config.AppSettings.Settings.Cast<KeyValueConfigurationElement>())
            {
                ConfigurationManager.AppSettings.Set(setting.Key, setting.Value);
            }
        }

        private void ConfigureUmbracoSettings(Configuration config)
        {
            var umbSettings = (IUmbracoSettingsSection)config.GetSection("umbracoConfiguration/settings");
            UmbracoConfig.For.SetUmbracoSettings(umbSettings);
        }

        private void ConfigureFileSystemProviders(Configuration config)
        {
            var fileSystems = (FileSystemProvidersSection)config.GetSection("umbracoConfiguration/FileSystemProviders");
            FileSystemProviderManager.SetCurrent(new FileSystemProviderManager(fileSystems));            
        }
    }
}