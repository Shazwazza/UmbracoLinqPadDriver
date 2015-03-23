using System;
using System.IO;
using System.Reflection;
using UmbracoLinqPad.Proxies;

namespace UmbracoLinqPad
{
    public abstract class UmbracoDataContextBase : IDisposable
    {
        public DirectoryInfo UmbracoFolder { get; private set; }

        protected UmbracoDataContextBase(DirectoryInfo umbracoFolder)
        {
            UmbracoFolder = umbracoFolder;
            if (umbracoFolder == null) throw new ArgumentNullException("umbracoFolder");
        }

        public abstract void Dispose();

        public void ExecutedCommand(string command)
        {
            OnCommandExecuted(command);
        }

        internal event EventHandler<string> CommandExecuted;

        private void OnCommandExecuted(string e)
        {
            var handler = CommandExecuted;
            if (handler != null) handler(this, e);
        }
    }

}