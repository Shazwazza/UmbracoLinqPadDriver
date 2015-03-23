using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using UmbracoLinqPad.Gateway.Bootstrap;

namespace UmbracoLinqPad.Gateway
{
    /// <summary>
    /// The LinqPad data context
    /// </summary>
    public abstract class UmbracoDataContext : UmbracoDataContextBase
    {
        private readonly ConsoleApplication _application;

        /// <summary>
        /// Constructor boots the umbraco application
        /// </summary>
        /// <param name="umbracoFolder"></param>
        protected UmbracoDataContext(DirectoryInfo umbracoFolder)
            : base(umbracoFolder)
        {           
            _application = new ConsoleApplication(umbracoFolder);
            _application.StartApplication();
            ApplicationContext = ApplicationContext.Current;
        }

        public ApplicationContext ApplicationContext { get; private set; }


        public override void Dispose()
        {
            ApplicationContext.DisposeIfDisposable();
            _application.Dispose();
        }
    }
}
