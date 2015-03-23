using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LinqToAnything;
using Umbraco.Core.Models;
using UmbracoLinqPad.Models;

namespace UmbracoLinqPad.Gateway.Models
{
    public class ContentQuery<T> : IQueryable<T>
        where T : IGeneratedContentBase
    {
        private readonly Assembly _generatedAssembly;
        private readonly UmbracoDataContext _dataContext;
        private readonly string _contentTypeAlias;
        private readonly IContentType _contentType;
        private readonly IQueryable<T> _baseQueryable;

        public ContentQuery(UmbracoDataContext dataContext, string contentTypeAlias)
        {
            if (dataContext == null) throw new ArgumentNullException("dataContext");

            //NOTE: This is strange i know but linqpad subclasses our data context in it's own assembly, we need
            // a ref to our own generated assembly to get th types from that
            _generatedAssembly = dataContext.GetType().BaseType.Assembly;

            _dataContext = dataContext;
            _contentTypeAlias = contentTypeAlias;
            _contentType = _dataContext.ApplicationContext.Services.ContentTypeService.GetContentType(contentTypeAlias);
            if (_contentType == null) throw new ArgumentException("No content type found with alias " + contentTypeAlias);

            _baseQueryable = new DelegateQueryable<T>(DataQuery);
        }

        private IEnumerable<T> DataQuery(QueryInfo info)
        {
            

            var content = _dataContext.ApplicationContext.Services.ContentService.GetContentOfContentType(_contentType.Id);

            //convert to the generated type (needs to be from the generated assembly)
            var genType = _generatedAssembly.GetType("Umbraco.Generated." + _contentTypeAlias);
            if (genType == null)
                throw new InvalidOperationException("No generated type found: " + "Umbraco.Generated." + _contentTypeAlias +
                                                    " data context assembly: " + _generatedAssembly);
            return content.Select(x => FromIContent(genType, x));

            //DataQuery<T> dataQuery = queryInfo => GetProducts(queryInfo.Take / queryInfo.Skip, queryInfo.Skip);
            ////notice that we have mapped Skip and Take into paging parameters
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

        public IEnumerator<T> GetEnumerator()
        {
            return _baseQueryable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Expression Expression
        {
            get { return _baseQueryable.Expression; }
        }
        
        public Type ElementType
        {
            get { return _baseQueryable.ElementType; }
        }
        
        public IQueryProvider Provider
        {
            get { return _baseQueryable.Provider; }
        }
    }
}