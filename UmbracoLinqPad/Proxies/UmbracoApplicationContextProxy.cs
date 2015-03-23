using System;

namespace UmbracoLinqPad.Proxies
{
    internal class UmbracoApplicationContextProxy : IDisposable
    {

        public UmbracoApplicationContextProxy(IDisposable realUmbracoAppContext)
        {
            RealUmbracoApplicationContext = realUmbracoAppContext;
        }

        public IDisposable RealUmbracoApplicationContext { get; private set; }

        public void Dispose()
        {
            RealUmbracoApplicationContext.Dispose();
        }
    }
}