using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbracoLinqPad.Queries
{
    public interface IContentTypeQuery
    {
        IEnumerable<string> GetAllContentTypeAliases();
    }
}
