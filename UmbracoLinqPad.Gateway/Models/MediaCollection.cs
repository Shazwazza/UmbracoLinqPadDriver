using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using UmbracoLinqPad.Models;

namespace UmbracoLinqPad.Gateway.Models
{
    /// <summary>
    /// The enumerable collection that Linq pad will display results for (just raw ienumerable, not iquerable)
    /// </summary>
    public class MediaCollection<TGeneratedContent> : ContentCollectionBase<TGeneratedContent, IMediaType, IMedia>
        where TGeneratedContent : IGeneratedContentBase
    {

        public MediaCollection(UmbracoDataContext dataContext, string contentTypeAlias)
            : base(dataContext, contentTypeAlias)
        {
        }

        protected override IMediaType GetContentType(string contentTypeAlias)
        {
            return DataContext.ApplicationContext.Services.ContentTypeService.GetMediaType(contentTypeAlias);
        }

        protected override IEnumerable<IMedia> GetContentOfContentType(int contentTypeId)
        {
            return DataContext.ApplicationContext.Services.MediaService.GetMediaOfMediaType(contentTypeId);
        }

        protected override T FromContent<T>(Type genType, IMedia content)
        {
            return ModelMapper.FromIMedia<T>(genType, content);
        }

        protected override string GetGeneratedTypeName(string contentTypeAlias)
        {
            return "Umbraco.Generated.Media_" + contentTypeAlias;
        }
    }
}