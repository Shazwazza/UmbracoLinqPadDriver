using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Models;
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
            
            //Do content types 
            foreach (var contentType in appContext.Services.ContentTypeService.GetAllContentTypes())
            {
                var sb = new StringBuilder();
                BuildClass(sb, contentType, "Content");
                yield return sb.ToString();    
            }

            //Do media types
            foreach (var mediaType in appContext.Services.ContentTypeService.GetAllMediaTypes())
            {
                var sb = new StringBuilder();
                BuildClass(sb, mediaType, "Media");
                yield return sb.ToString();
            }            
        }

        private void BuildClass(StringBuilder sb, IContentTypeComposition contentType, string category)
        {
            sb.Append("public class ");
            sb.AppendFormat("{0}_{1}", category, contentType.Alias); //class name prefixed with category (i.e. Content)
            sb.AppendLine(" : UmbracoLinqPad.Models.IGeneratedContentBase"); //implements
            sb.AppendLine(" {"); //open class

            sb.AppendLine("public string ContentTypeAlias { get; set; }");
            sb.AppendLine("public string Name { get; set; }");

            //Fill in the properties
            foreach (var property in contentType.CompositionPropertyTypes.Select(x => x.Alias))
            {
                sb.Append("public object ");
                sb.Append(property);
                sb.AppendLine(" { get; set; }");
            }

            sb.AppendLine("}"); //end class
        }

    }
}
