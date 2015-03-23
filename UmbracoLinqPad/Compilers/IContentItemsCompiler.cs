using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbracoLinqPad.Compilers
{
    public interface IContentItemsCompiler
    {
        IEnumerable<string> GenerateClasses(IDisposable realUmbracoApplicationContext);
    }
}
