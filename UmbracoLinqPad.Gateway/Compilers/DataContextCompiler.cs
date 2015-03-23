using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core;
using UmbracoLinqPad.Compilers;

namespace UmbracoLinqPad.Gateway.Compilers
{
    public class DataContextCompiler : IDataContextCompiler
    {

        public string GenerateClass(string className, IDisposable realUmbracoApplicationContext)
        {
            var appContext = realUmbracoApplicationContext as ApplicationContext;
            if (appContext == null) throw new ArgumentException("realUmbracoApplicationContext is not of type " + typeof(ApplicationContext));

            var sb = new StringBuilder();

            sb.Append("public class ");
            sb.Append(className);
            sb.Append(" : UmbracoLinqPad.Gateway.UmbracoDataContext"); //inherits
            sb.AppendLine(" {"); //open class

            //constructor
            sb.Append("public ");
            sb.Append(className);
            sb.AppendLine("(DirectoryInfo umbracoFolder) : base(umbracoFolder) { }");

            foreach (var alias in appContext.Services.ContentTypeService.GetAllContentTypes().Select(x => x.Alias).Distinct())
            {
                GenerateProperty(alias, "Content", "UmbracoLinqPad.Gateway.Models.ContentCollection", sb);
            }

            foreach (var alias in appContext.Services.ContentTypeService.GetAllMediaTypes().Select(x => x.Alias).Distinct())
            {
                GenerateProperty(alias, "Media", "UmbracoLinqPad.Gateway.Models.MediaCollection", sb);
            }

            sb.AppendLine("}"); //end class
            return sb.ToString();
        }

        private void GenerateProperty(string alias, string category, string collectionType, StringBuilder sb)
        {
            sb.Append("public IEnumerable<");
            sb.Append(alias); //enumerable type
            sb.Append("> ");
            sb.AppendFormat("{0}_{1}", category, alias); //property name prefixed with category (i.e. Content)
            sb.AppendLine(" { "); //start property
            sb.Append(" get { "); //start get
            sb.AppendFormat(" return new {0}<", collectionType);
            sb.Append(alias);
            sb.Append(">(this,\""); //pass ourselves (datacontext) into the collection
            sb.Append(alias);
            sb.Append("\"); ");
            sb.AppendLine("}"); //end get
            sb.AppendLine("}"); //end property

            //TODO: When we have the ability to query content nicely from the core and implement IQueryable, then we'd enable that here

            //sb.Append("public IQueryable<");
            //sb.Append(alias); //enumerable type
            //sb.Append("> ");
            //sb.AppendLine(alias); //property name
            //sb.AppendLine(" { "); //start property
            //sb.Append(" get { "); //start get
            //sb.Append(" return new UmbracoLinqPad.Gateway.Models.ContentQuery<");
            //sb.Append(alias);
            //sb.Append(">(this,\""); //pass ourselves (datacontext) into the collection
            //sb.Append(alias);
            //sb.Append("\"); ");
            //sb.AppendLine("}"); //end get
            //sb.AppendLine("}"); //end property
        }
    }
}