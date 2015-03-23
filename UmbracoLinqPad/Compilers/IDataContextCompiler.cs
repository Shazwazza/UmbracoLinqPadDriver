using System;
using System.Collections.Generic;

namespace UmbracoLinqPad.Compilers
{
    public interface IDataContextCompiler
    {
        string GenerateClass(string className, IDisposable realUmbracoApplicationContext);
    }
}