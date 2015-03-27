using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Models;
using UmbracoLinqPad.Models;

namespace UmbracoLinqPad.Gateway.Models
{
    /// <summary>
    /// The enumerable collection that Linq pad will display results for (just raw ienumerable, not iquerable)
    /// </summary>
    public abstract class ContentCollectionBase<TGeneratedContent, TContentType, TContent> : IEnumerable<TGeneratedContent>
        where TGeneratedContent : IGeneratedContentBase
        where TContentType : IContentTypeComposition
        where TContent : IContentBase
    {
        private readonly Assembly _generatedAssembly;
        private readonly string _contentTypeAlias;
        private readonly TContentType _contentType;

        protected UmbracoDataContext DataContext { get; private set; }

        protected abstract TContentType GetContentType(string contentTypeAlias);
        protected abstract IEnumerable<TContent> GetContentOfContentType(int contentTypeId);
        protected abstract T FromContent<T>(Type genType, TContent content) where T : IGeneratedContentBase;
        protected abstract string GetGeneratedTypeName(string contentTypeAlias);

        protected ContentCollectionBase(UmbracoDataContext dataContext, string contentTypeAlias)
        {
            if (dataContext == null) throw new ArgumentNullException("dataContext");

            //NOTE: This is strange i know but linqpad subclasses our data context in it's own assembly, we need
            // a ref to our own generated assembly to get th types from that
            _generatedAssembly = dataContext.GetType().BaseType.Assembly;

            DataContext = dataContext;
            _contentTypeAlias = contentTypeAlias;
            _contentType = GetContentType(contentTypeAlias);
            if (_contentType == null) throw new ArgumentException("No content type found with alias " + contentTypeAlias);
        }

        public IEnumerator<TGeneratedContent> GetEnumerator()
        {
            var content = GetContentOfContentType(_contentType.Id);

            //convert to the generated type (needs to be from the generated assembly)
            var genType = _generatedAssembly.GetType(GetGeneratedTypeName(_contentTypeAlias)); //"Umbraco.Generated." + _contentTypeAlias);
            if (genType == null)
                throw new InvalidOperationException("No generated type found: " + "Umbraco.Generated." + _contentTypeAlias +
                                                    " data context assembly: " + _generatedAssembly);

            return content.Select(x => FromContent<TGeneratedContent>(genType, x)).GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}