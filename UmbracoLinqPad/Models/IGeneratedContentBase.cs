using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbracoLinqPad.Models
{
    public interface IGeneratedContentBase
    {
        string ContentTypeAlias { get; set; }
        string Name { get; set; }
    }
}
