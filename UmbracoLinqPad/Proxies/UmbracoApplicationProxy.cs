using System;
using System.Linq;

namespace UmbracoLinqPad.Proxies
{
    internal class UmbracoApplicationProxy : IDisposable
    {
        private readonly IDisposable _realUmbracoApp;

        public UmbracoApplicationProxy(GatewayLoader gatewayLoader, IDisposable realUmbracoApp)
        {            
            _realUmbracoApp = realUmbracoApp;
            //start the app with reflection
            _realUmbracoApp.CallMethod("StartApplication", infos => infos.FirstOrDefault(x => x.IsPublic));

            var realAppContext = (IDisposable)gatewayLoader.UmbracoCoreAssembly.GetType("Umbraco.Core.ApplicationContext").GetStaticProperty("Current");
            ApplicationContext = new UmbracoApplicationContextProxy(gatewayLoader, realAppContext);
        }

        public UmbracoApplicationContextProxy ApplicationContext { get; private set; }

        public void Dispose()
        {
            _realUmbracoApp.Dispose();
        }
    }
}