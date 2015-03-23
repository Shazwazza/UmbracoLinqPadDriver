using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Services;
using UmbracoLinqPad.Compilers;

namespace UmbracoLinqPad.Gateway.Compilers
{
    public class ContentItemsCompiler : IContentItemsCompiler
    {
       
        public IEnumerable<string> GenerateClasses(IDisposable realUmbracoApplicationContext)
        {
            var appContext = realUmbracoApplicationContext as ApplicationContext;
            if (appContext == null) throw new ArgumentException("realUmbracoApplicationContext is not of type " + typeof(ApplicationContext));
            
            foreach (var contentType in appContext.Services.ContentTypeService.GetAllContentTypes())
            {
                var sb = new StringBuilder();

                sb.Append("public class ");
                //TODO: The class needs to be prefixed with "Content_" or "Media_" like we have for the DataContext properties.
                sb.Append(contentType.Alias);
                sb.AppendLine(" : UmbracoLinqPad.Models.IGeneratedContentBase"); //implements
                sb.AppendLine(" {"); //open class

                sb.AppendLine("public string ContentTypeAlias { get; set; }");
                sb.AppendLine("public string Name { get; set; }");

                //TODO: Fill in the properties

                foreach (var property in contentType.CompositionPropertyTypes.Select(x => x.Alias))
                {
                    sb.Append("public object ");
                    sb.Append(property);
                    sb.AppendLine(" { get; set; }");
                }

                sb.AppendLine("}"); //end class
                yield return sb.ToString();    
            }

            
        }

    }
}
