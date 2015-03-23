using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Umbraco.Core;
using Umbraco.Core.Models;
using UmbracoLinqPad.Models;

namespace UmbracoLinqPad.Gateway.Models
{
    /// <summary>
    /// The enumerable collection that Linq pad will display results for
    /// </summary>
    public class ContentCollection<T> : IEnumerable<T>
        where T : IGeneratedContentBase
    {
        private readonly Assembly _generatedAssembly;
        private readonly UmbracoDataContext _dataContext;
        private readonly string _contentTypeAlias;
        private readonly IContentType _contentType;

        public ContentCollection(UmbracoDataContext dataContext, string contentTypeAlias)
        {
            if (dataContext == null) throw new ArgumentNullException("dataContext");

            //NOTE: This is strange i know but linqpad subclasses our data context in it's own assembly, we need
            // a ref to our own generated assembly to get th types from that
            _generatedAssembly = dataContext.GetType().BaseType.Assembly;

            _dataContext = dataContext;
            _contentTypeAlias = contentTypeAlias;
            _contentType = _dataContext.ApplicationContext.Services.ContentTypeService.GetContentType(contentTypeAlias);
            if (_contentType == null) throw new ArgumentException("No content type found with alias " + contentTypeAlias);
        }

        public IEnumerator<T> GetEnumerator()
        {
            var content = _dataContext.ApplicationContext.Services.ContentService.GetContentOfContentType(_contentType.Id);

            //convert to the generated type (needs to be from the generated assembly)
            var genType = _generatedAssembly.GetType("Umbraco.Generated." + _contentTypeAlias);
            if (genType == null)
                throw new InvalidOperationException("No generated type found: " + "Umbraco.Generated." + _contentTypeAlias +
                                                    " data context assembly: " + _generatedAssembly);
            return content.Select(x => FromIContent(genType, x)).GetEnumerator();
        }

      
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private T FromIContent(Type genType, IContent content)
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