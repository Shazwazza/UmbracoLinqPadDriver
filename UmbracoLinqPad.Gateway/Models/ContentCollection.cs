using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Models;
using UmbracoLinqPad.Models;

namespace UmbracoLinqPad.Gateway.Models
{
    public class ContentCollection<TGeneratedContent> : ContentCollectionBase<TGeneratedContent, IContentType, IContent>
       where TGeneratedContent : IGeneratedContentBase
    {

        public ContentCollection(UmbracoDataContext dataContext, string contentTypeAlias)
            : base(dataContext, contentTypeAlias)
        {
        }

        protected override IContentType GetContentType(string contentTypeAlias)
        {
            return DataContext.ApplicationContext.Services.ContentTypeService.GetContentType(contentTypeAlias);
        }

        protected override IEnumerable<IContent> GetContentOfContentType(int contentTypeId)
        {
            return DataContext.ApplicationContext.Services.ContentService.GetContentOfContentType(contentTypeId);
        }

        protected override T FromContent<T>(Type genType, IContent content)
        {
            return ModelMapper.FromIContent<T>(genType, content);
        }

        protected override string GetGeneratedTypeName(string contentTypeAlias)
        {
            return "Umbraco.Generated.Content_" + contentTypeAlias;
        }
    }

}