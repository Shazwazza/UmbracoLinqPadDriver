using System;

namespace UmbracoLinqPad.Proxies
{
    internal class UmbracoApplicationContextProxy : IDisposable
    {
        private readonly IDisposable _realUmbracoAppContext;

        public UmbracoApplicationContextProxy(GatewayLoader gatewayLoader, IDisposable realUmbracoAppContext)
        {
            _realUmbracoAppContext = realUmbracoAppContext;

            Services = new UmbracoServicesContextProxy(gatewayLoader, realUmbracoAppContext.GetPropertyValue("Services"));
        }

        public UmbracoServicesContextProxy Services { get; private set; }

        public void Dispose()
        {
            _realUmbracoAppContext.Dispose();
        }
    }
}