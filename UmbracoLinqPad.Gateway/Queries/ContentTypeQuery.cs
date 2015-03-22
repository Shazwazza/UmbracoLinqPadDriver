using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Services;
using UmbracoLinqPad.Queries;

namespace UmbracoLinqPad.Gateway.Queries
{
    public class ContentTypeQuery : IContentTypeQuery
    {
        private readonly IContentTypeService _contentTypeService;

        public ContentTypeQuery(IContentTypeService contentTypeService)
        {
            _contentTypeService = contentTypeService;
        }

        public IEnumerable<string> GetAllContentTypeAliases()
        {
            return _contentTypeService.GetAllContentTypes().Select(x => x.Alias);
        }
    }
}
