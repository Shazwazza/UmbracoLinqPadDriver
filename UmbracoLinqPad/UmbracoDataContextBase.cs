using System;
using System.IO;
using System.Reflection;
using UmbracoLinqPad.Proxies;

namespace UmbracoLinqPad
{
    public abstract class UmbracoDataContextBase : IDisposable
    {
        public DirectoryInfo UmbracoFolder { get; private set; }
        //private readonly UmbracoApplicationProxy _umbracoApplication;

        protected UmbracoDataContextBase(DirectoryInfo umbracoFolder)
        {
            UmbracoFolder = umbracoFolder;
            //if (gatewayLoader == null) throw new ArgumentNullException("gatewayLoader");
            if (umbracoFolder == null) throw new ArgumentNullException("umbracoFolder");

            //_umbracoApplication = gatewayLoader.StartUmbracoApplication(umbracoFolder);
        }

        public abstract void Dispose();
        //{
        //    _umbracoApplication.Dispose();
        //}
    }

}