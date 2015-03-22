using System.Collections.Generic;
using System.Text;
using UmbracoLinqPad.Compilers;

namespace UmbracoLinqPad.Gateway.Compilers
{
    

    public class DataContextCompiler : IDataContextCompiler
    {
      
        public string GenerateClass(string className, IEnumerable<string> contentTypeAliases)
        {
            var sb = new StringBuilder();

            sb.Append("public class ");
            sb.Append(className);
            sb.Append(" : UmbracoLinqPad.UmbracoDataContextBase"); //inherits
            sb.AppendLine(" {"); //open class

            //constructor
            sb.Append("public ");
            sb.Append(className);
            sb.AppendLine("(GatewayLoader gatewayLoader, DirectoryInfo umbracoFolder) : base(gatewayLoader, umbracoFolder) { }");

            foreach (var alias in contentTypeAliases)
            {
                sb.Append("public IEnumerable<");
                sb.Append(alias); //enumerable type
                sb.Append("> ");
                sb.AppendLine(alias); //property name
                sb.AppendLine(" { "); //start property
                sb.Append(" get { "); //start get
                sb.Append(" return new UmbracoLinqPad.Gateway.Models.ContentCollection<");
                sb.Append(alias);
                sb.Append(">(this,\""); //pass ourselves (datacontext) into the collection
                sb.Append(alias);
                sb.Append("\"); ");
                sb.AppendLine("}"); //end get
                sb.AppendLine("}"); //end property
            }

            sb.AppendLine("}"); //end class
            return sb.ToString();
        }
    }
}