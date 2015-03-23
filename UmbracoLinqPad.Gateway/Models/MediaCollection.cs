using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Models;
using UmbracoLinqPad.Models;

namespace UmbracoLinqPad.Gateway.Models
{
    public class MediaCollection<T> : IEnumerable<T>
        where T : IGeneratedContentBase
    {
        private readonly Assembly _generatedAssembly;
        private readonly UmbracoDataContext _dataContext;
        private readonly string _contentTypeAlias;
        private readonly IMediaType _contentType;

        public MediaCollection(UmbracoDataContext dataContext, string contentTypeAlias)
        {
            if (dataContext == null) throw new ArgumentNullException("dataContext");

            //NOTE: This is strange i know but linqpad subclasses our data context in it's own assembly, we need
            // a ref to our own generated assembly to get th types from that
            _generatedAssembly = dataContext.GetType().BaseType.Assembly;

            _dataContext = dataContext;
            _contentTypeAlias = contentTypeAlias;
            _contentType = _dataContext.ApplicationContext.Services.ContentTypeService.GetMediaType(contentTypeAlias);
            if (_contentType == null) throw new ArgumentException("No content type found with alias " + contentTypeAlias);
        }

        public IEnumerator<T> GetEnumerator()
        {
            var content = _dataContext.ApplicationContext.Services.MediaService.GetMediaOfMediaType(_contentType.Id);

            //convert to the generated type (needs to be from the generated assembly)
            var genType = _generatedAssembly.GetType("Umbraco.Generated." + _contentTypeAlias);
            if (genType == null)
                throw new InvalidOperationException("No generated type found: " + "Umbraco.Generated." + _contentTypeAlias +
                                                    " data context assembly: " + _generatedAssembly);
            return content.Select(x => ModelMapper.FromIMedia<T>(genType, x)).GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


    }
}