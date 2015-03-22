using System;
using System.IO;
using System.Reflection;
using UmbracoLinqPad.Proxies;

namespace UmbracoLinqPad
{
    public abstract class UmbracoDataContextBase : IDisposable
    {
        private readonly UmbracoApplicationProxy _umbracoApplication;

        protected UmbracoDataContextBase(GatewayLoader gatewayLoader, DirectoryInfo umbracoFolder)
        {

            if (gatewayLoader == null) throw new ArgumentNullException("gatewayLoader");
            if (umbracoFolder == null) throw new ArgumentNullException("umbracoFolder");

            _umbracoApplication = gatewayLoader.StartUmbracoApplication(umbracoFolder);
        }

        public void Dispose()
        {
            _umbracoApplication.Dispose();
        }
    }

}