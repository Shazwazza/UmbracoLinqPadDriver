using System.Text;
using Umbraco.Core.Services;
using UmbracoLinqPad.Compilers;

namespace UmbracoLinqPad.Gateway.Compilers
{
    public class ContentTypeCompiler : IContentTypeCompiler
    {
        private readonly IContentTypeService _contentTypeService;

        public ContentTypeCompiler(IContentTypeService contentTypeService)
        {
            _contentTypeService = contentTypeService;
        }

        public string GenerateClass(string contentTypeAlias)
        {
            var sb = new StringBuilder();
            
            sb.Append("public class ");
            sb.Append(contentTypeAlias);
            sb.AppendLine(" : UmbracoLinqPad.Models.IGeneratedContentBase"); //implements
            sb.AppendLine(" {"); //open class

            sb.AppendLine("public string ContentTypeAlias { get; set; }");
            sb.AppendLine("public string Name { get; set; }");
            //TODO: Fill in the properties

            sb.AppendLine("}"); //end class
            return sb.ToString();
        }

    }
}
