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

        public GatewayLoader(Assembly gatewayAssembly, Assembly umbracoCoreAssembly)
        {
            GatewayAssembly = gatewayAssembly;
            UmbracoCoreAssembly = umbracoCoreAssembly;
            _consoleAppType= new Lazy<Type>(() => GatewayAssembly.GetType("UmbracoLinqPad.Gateway.Bootstrap.ConsoleApplication"));
        }

        public Assembly UmbracoCoreAssembly { get; private set; }
        public Assembly GatewayAssembly { get; private set; }

        private readonly Lazy<Type> _consoleAppType;

        internal UmbracoApplicationProxy StartUmbracoApplication(DirectoryInfo umbracoAppPath)
        {
            return new UmbracoApplicationProxy((IDisposable)Activator.CreateInstance(_consoleAppType.Value, umbracoAppPath), UmbracoCoreAssembly);
        }
    }
}
