using System;
using System.Collections.Generic;
using UmbracoLinqPad.Queries;

namespace UmbracoLinqPad.Proxies
{
    internal class UmbracoServicesContextProxy 
    {
        private readonly GatewayLoader _gatewayLoader;
        private readonly object _realUmbracoServicesContext;

        public UmbracoServicesContextProxy(GatewayLoader gatewayLoader, object realUmbracoServicesContext)
        {
            if (realUmbracoServicesContext == null) throw new ArgumentNullException("realUmbracoServicesContext");
            _gatewayLoader = gatewayLoader;
            _realUmbracoServicesContext = realUmbracoServicesContext;

            ContentTypeService = _realUmbracoServicesContext.GetPropertyValue("ContentTypeService");
            ContentTypeQuery = (IContentTypeQuery)Activator.CreateInstance(
                _gatewayLoader.GatewayAssembly.GetType("UmbracoLinqPad.Gateway.Queries.ContentTypeQuery"),
                ContentTypeService);

        }

        public object ContentTypeService { get; private set; }
        public IContentTypeQuery ContentTypeQuery { get; private set; }
    }
}