using System;
using System.Linq;
using Umbraco.Core.Models;
using UmbracoLinqPad.Models;

namespace UmbracoLinqPad.Gateway.Models
{
    internal class ModelMapper
    {
        public static T FromIContent<T>(Type genType, IContent content)
            where T : IGeneratedContentBase
        {
            var instance = (T)Activator.CreateInstance(genType);
            instance.ContentTypeAlias = content.ContentType.Alias;
            instance.Name = content.Name;

            var contentTypeProps = genType.GetProperties().Where(x => x.Name != "ContentTypeAlias" && x.Name != "Name");
            foreach (var contentTypeProp in contentTypeProps)
            {
                if (content.Properties.Contains(contentTypeProp.Name))
                {
                    var prop = content.Properties[contentTypeProp.Name];
                    contentTypeProp.SetValue(instance, prop.Value, null);
                }
            }

            return instance;
        }
    }
}