using System;
using System.Linq;
using System.Reflection;

namespace UmbracoLinqPad.Proxies
{
    internal class UmbracoApplicationProxy : IDisposable
    {
        private readonly IDisposable _realUmbracoApp;

        public UmbracoApplicationProxy(IDisposable realUmbracoApp, Assembly umbracoCoreAssembly)
        {
            _realUmbracoApp = realUmbracoApp;
            //start the app with reflection
            _realUmbracoApp.CallMethod("StartApplication", infos => infos.FirstOrDefault(x => x.IsPublic));

            var realAppContext = (IDisposable)umbracoCoreAssembly.GetType("Umbraco.Core.ApplicationContext").GetStaticProperty("Current");
            ApplicationContext = new UmbracoApplicationContextProxy(realAppContext);
        }

        public UmbracoApplicationContextProxy ApplicationContext { get; private set; }

        public void Dispose()
        {
            _realUmbracoApp.Dispose();
        }
    }
}