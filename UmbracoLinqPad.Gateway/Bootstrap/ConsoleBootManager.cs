using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Caching;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Services;

namespace UmbracoLinqPad.Gateway.Bootstrap
{
    public class ConsoleBootManager : CoreBootManager
    {
        private readonly DirectoryInfo _umbracoFolder;

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
            var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            
            ConfigureUmbracoSettings(config);
            ConfigureConnectionStrings(config);
            ConfigureAppSettings(config);

            //Few folders that need to exist
            System.IO.Directory.CreateDirectory(IOHelper.MapPath("~/App_Plugins"));

            return base.Initialize();
        }

        /// <summary>
        /// Disables all application level cache
        /// </summary>
        protected override void CreateApplicationCache()
        {
            var cacheHelper = new CacheHelper(
                        new NullCacheProvider(),
                        new NullCacheProvider(),
                        new NullCacheProvider());
            ApplicationCache = cacheHelper;
        }

        /// <summary>
        /// The main problem with booting umbraco is all of the startup handlers that will not work without a web context or in a standalone
        /// mode. So this code removes all of those handlers. We of course need some of them so this attempts to just keep the startup handlers
        /// declared inside of Umbraco.Core.
        /// </summary>
        protected override void InitializeApplicationEventsResolver()
        {
            base.InitializeApplicationEventsResolver();

            //now remove what we want to , unfortunately this needs reflection currently
            var appEventsResolverType = Type.GetType("Umbraco.Core.ObjectResolution.ApplicationEventsResolver,Umbraco.Core", true);
            var appEventsResolver = appEventsResolverType.GetStaticProperty("Current");
            //now we want to get all IApplicationStartupHandlers from the PluginManager, again, needs reflection
            var startupHandlers = (IEnumerable<Type>)PluginManager.Current.CallMethod("ResolveApplicationStartupHandlers");
            //for now we're just going to remove any type that does not exist in Umbraco.Core
            foreach (var startupHandler in startupHandlers
                .Where(x => x.Namespace != null)
                .Where(x => !x.Namespace.StartsWith("Umbraco.Core")))
            {
                //This is a special case because we have legacy handlers that are not of type IApplicationEventHandler and only 
                // of type IUmbracoStartupHandler which will throw if we try to remove them here because those are handled on
                // an internal object inside of ApplicationEventsResolver. It's our hope that none of those handlers will interfere with
                // the core processing outside of the web... but we'll have to deal with that later since I'm sure there will be problems.
                if (typeof (IApplicationEventHandler).IsAssignableFrom(startupHandler))
                {
                    appEventsResolver.CallMethod("RemoveType", infos => infos.FirstOrDefault(x => x.IsGenericMethod == false), startupHandler);    
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
            //use reflection to set the settings
            UmbracoConfig.For.CallMethod("SetUmbracoSettings", umbSettings);
        }
    }

    public class NullCacheProvider : IRuntimeCacheProvider
    {
        public virtual void ClearAllCache()
        {
        }

        public virtual void ClearCacheItem(string key)
        {
        }

        public virtual void ClearCacheObjectTypes(string typeName)
        {
        }

        public virtual void ClearCacheObjectTypes<T>()
        {
        }

        public virtual void ClearCacheObjectTypes<T>(Func<string, T, bool> predicate)
        {
        }




        public virtual void ClearCacheByKeySearch(string keyStartsWith)
        {
        }

        public virtual void ClearCacheByKeyExpression(string regexString)
        {
        }

        public virtual IEnumerable<object> GetCacheItemsByKeySearch(string keyStartsWith)
        {
            return Enumerable.Empty<object>();
        }

        public IEnumerable<object> GetCacheItemsByKeyExpression(string regexString)
        {
            return Enumerable.Empty<object>();
        }

        public virtual object GetCacheItem(string cacheKey)
        {
            return default(object);
        }

        public virtual object GetCacheItem(string cacheKey, Func<object> getCacheItem)
        {
            return getCacheItem();
        }

        public object GetCacheItem(string cacheKey, Func<object> getCacheItem, TimeSpan? timeout, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, string[] dependentFiles = null)
        {
            return getCacheItem();
        }

        public void InsertCacheItem(string cacheKey, Func<object> getCacheItem, TimeSpan? timeout = null, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, string[] dependentFiles = null)
        {

        }
    }
}