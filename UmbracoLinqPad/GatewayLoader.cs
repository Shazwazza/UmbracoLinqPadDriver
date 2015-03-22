using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LINQPad;
using LINQPad.Extensibility.DataContext;
using UmbracoLinqPad.Proxies;

namespace UmbracoLinqPad
{
    /// <summary>
    /// Used to dynamically load in our gateway assembly which is not strongly typed
    /// </summary>
    public sealed class GatewayLoader
    {

        public GatewayLoader()
        {
            //GatewayAssembly = AppDomain.CurrentDomain.Load("UmbracoLinqPad.Gateway");
            //UmbracoCoreAssembly = AppDomain.CurrentDomain.Load("Umbraco.Core");

            GatewayAssembly = DataContextDriver.LoadAssemblySafely(Path.Combine(AssemblyDirectory, "UmbracoLinqPad.Gateway.dll"));
            UmbracoCoreAssembly = DataContextDriver.LoadAssemblySafely(Path.Combine(AssemblyDirectory, "Umbraco.Core.dll"));

            _consoleAppType= new Lazy<Type>(() => GatewayAssembly.GetType("UmbracoLinqPad.Gateway.Bootstrap.ConsoleApplication"));
        }

        public Assembly UmbracoCoreAssembly { get; private set; }
        public Assembly GatewayAssembly { get; private set; }

        private readonly Lazy<Type> _consoleAppType;

        internal UmbracoApplicationProxy StartUmbracoApplication(DirectoryInfo umbracoAppPath)
        {
            return new UmbracoApplicationProxy(this, (IDisposable) Activator.CreateInstance(_consoleAppType.Value, umbracoAppPath));
        }

        string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
