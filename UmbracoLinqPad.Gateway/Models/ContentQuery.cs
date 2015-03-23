using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using LinqToAnything;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using UmbracoLinqPad.Models;

namespace UmbracoLinqPad.Gateway.Models
{
    public class ContentQuery<T> : IOrderedQueryable<T>
        where T : IGeneratedContentBase
    {
        private readonly Assembly _generatedAssembly;
        private readonly UmbracoDataContext _dataContext;
        private readonly string _contentTypeAlias;
        private readonly IContentType _contentType;
        private readonly IOrderedQueryable<T> _baseQueryable;

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

        public IEnumerable<T> DataQuery(QueryInfo info)
        {
            System.Diagnostics.Debugger.Launch();

            var sb = new StringBuilder();
            sb.AppendLine("ApplicationContext.Services.ContentService.GetPagedDescendants(")
                .AppendLine("     -1,")
                .AppendFormat("     {0}, //skip", info.Skip).AppendLine()
                .AppendFormat("     {0}, //take", info.Take ?? int.MaxValue).AppendLine()
                .AppendLine("     out total,")
                .AppendFormat("     {0}, //order by", info.OrderBy == null ? "Path" : info.OrderBy.Name).AppendLine()
                .AppendFormat("     {0}, //direction", info.OrderBy == null ? "Direction.Ascending" : info.OrderBy.Direction == OrderBy.OrderByDirection.Asc ? "Direction.Ascending" : "Direction.Descending").AppendLine();
            _dataContext.ExecutedCommand(sb.ToString());

            int total;

            var content = _dataContext.ApplicationContext.Services.ContentService.GetPagedDescendants(
                -1,
                info.Skip,
                info.Take ?? int.MaxValue,
                out total,
                info.OrderBy == null ? "Path" : info.OrderBy.Name,
                info.OrderBy == null ? Direction.Ascending : info.OrderBy.Direction == OrderBy.OrderByDirection.Asc ? Direction.Ascending : Direction.Descending);

            //var content = _dataContext.ApplicationContext.Services.ContentService.GetContentOfContentType(_contentType.Id);

            //convert to the generated type (needs to be from the generated assembly)
            var genType = _generatedAssembly.GetType("Umbraco.Generated." + _contentTypeAlias);
            if (genType == null)
                throw new InvalidOperationException("No generated type found: " + "Umbraco.Generated." + _contentTypeAlias +
                                                    " data context assembly: " + _generatedAssembly);
            return content.Select(x => ModelMapper.FromIContent<T>(genType, x));
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _baseQueryable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _baseQueryable.GetEnumerator();
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