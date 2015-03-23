using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LINQPad;
using LINQPad.Extensibility.DataContext;
using Microsoft.CSharp;
using UmbracoLinqPad.Compilers;

namespace UmbracoLinqPad
{
    /// <summary>
    /// This static driver let users query any data source that looks like a Data Context - in other words,
    /// that exposes properties of type IEnumerable of T.
    /// </summary>
    public class UmbracoDynamicDriver : DynamicDataContextDriver
    {
        public override string Name { get { return "Umbraco Driver"; } }

        public override string Author { get { return "Shannon Deminick"; } }

        public override string GetConnectionDescription(IConnectionInfo cxInfo)
        {
            return "Umbraco :: " + cxInfo.AppConfigPath;
        }

        public override bool ShowConnectionDialog(IConnectionInfo cxInfo, bool isNewConnection)
        {
            // Prompt the user for a custom assembly and type name:
            return new ConnectionDialog(cxInfo).ShowDialog() == true;
        }

        public override void InitializeContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
        {
            //This is used to write output to the 'SQL' window
            var dsContext = (UmbracoDataContextBase)context;
            dsContext.CommandExecuted += (sender, s) => executionManager.SqlTranslationWriter.WriteLine(s);
        }
      
        public override ParameterDescriptor[] GetContextConstructorParameters(IConnectionInfo cxInfo)
        {
            return new[]
            {
                new ParameterDescriptor("umbracoFolder", typeof (DirectoryInfo).FullName)
            };
        }

        public override object[] GetContextConstructorArguments(IConnectionInfo cxInfo)
        {
            return new object[]
            {
                new DirectoryInfo(cxInfo.AppConfigPath)
            };
        }

        public override IEnumerable<string> GetAssembliesToAdd(IConnectionInfo cxInfo)
        {           
            var umbFolder = new DirectoryInfo(cxInfo.AppConfigPath);

            return Directory.GetFiles(Path.Combine(umbFolder.FullName, "bin"), "*.dll")
                .Concat(new[]
                {
                    "UmbracoLinqPad.Gateway.dll"
                });
        }

        public override void OnQueryFinishing(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
        {
            //There’s also an OnQueryFinishing method that you can override. Unlike TearDownContext, this runs just before the query ends, so you can Dump extra output  in this method. You can also block for as long as you like—while waiting on some background threads to finish, for instance. If the user gets tired of waiting, they’ll hit the Cancel button in which case your thread will be aborted, and the TearDownContext method will then run. (The next thing to happen is that your application domain will be torn down and recreated, unless the user’s requested otherwise in Edit | Preferences | Advanced, or has cached objects alive).
        }

        public override void TearDownContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager, object[] constructorArguments)
        {
            ((IDisposable)context).Dispose();
        }

        public override DateTime? GetLastSchemaUpdate(IConnectionInfo cxInfo)
        {
            //LINQPad calls this after the user executes an old-fashioned SQL query. If it returns a non-null value that’s later than its last value, it automatically refreshes the Schema Explorer. This is useful in that quite often, the reason for users running a SQL query is to create a new table or perform some other DDL.
            //Output from this method may also be used in the future for caching data contexts between sessions.

            return base.GetLastSchemaUpdate(cxInfo);
        }

        public override ICustomMemberProvider GetCustomDisplayMemberProvider(object objectToWrite)
        {
            return base.GetCustomDisplayMemberProvider(objectToWrite);
        }

        public override void PreprocessObjectToWrite(ref object objectToWrite, ObjectGraphInfo info)
        {
            base.PreprocessObjectToWrite(ref objectToWrite, info);
        }

        public override void DisplayObjectInGrid(object objectToDisplay, GridOptions options)
        {
            base.DisplayObjectInGrid(objectToDisplay, options);
        }

        public override DbProviderFactory GetProviderFactory(IConnectionInfo cxInfo)
        {
            //LINQPad lets users run old-fashioned SQL queries, by setting the query language to “SQL”. If it makes for your driver to support this, you can gain more control over how connections are created by overriding the following methods:
            
            //Override this to let users run raw SQL against umbraco

            return base.GetProviderFactory(cxInfo);
        }

        public override IDbConnection GetIDbConnection(IConnectionInfo cxInfo)
        {
            //LINQPad lets users run old-fashioned SQL queries, by setting the query language to “SQL”. If it makes for your driver to support this, you can gain more control over how connections are created by overriding the following methods:

            //Override this to let users run raw SQL against umbraco

            return base.GetIDbConnection(cxInfo);
        }

        public override void ExecuteESqlQuery(IConnectionInfo cxInfo, string query)
        {
            throw new Exception("ESQL queries are not supported for this type of connection");
        }      

        public override List<ExplorerItem> GetSchemaAndBuildAssembly(IConnectionInfo cxInfo, AssemblyName assemblyToBuild, ref string nameSpace, ref string typeName)
        {

            nameSpace = "Umbraco.Generated";
            typeName = "GeneratedUmbracoDataContext";

            var umbFolder = new DirectoryInfo(cxInfo.AppConfigPath);

            //load all assemblies in the umbraco bin folder
            var loadedAssemblies = Directory.GetFiles(Path.Combine(umbFolder.FullName, "bin"), "*.dll").Select(LoadAssemblySafely).ToList();
            
            //we'll need to manually resolve any assemblies loaded above
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var found = loadedAssemblies.FirstOrDefault(x => x.GetName().Name == new AssemblyName(args.Name).Name);
                if (found != null)
                {
                    return found;
                }
                return null;
            };

            //Create a loader to startup the umbraco app to create the schema and the generated DataContext class

            var gatewayLoader = new GatewayLoader(
                LoadAssemblySafely(Path.Combine(GetDriverFolder(), "UmbracoLinqPad.Gateway.dll")),
                loadedAssemblies.Single(x => x.GetName().Name == "Umbraco.Core"),
                LoadAssemblySafely(Path.Combine(GetDriverFolder(), "IQToolkit.dll")));

            using (var app = gatewayLoader.StartUmbracoApplication(new DirectoryInfo(cxInfo.AppConfigPath)))
            {
                using (var appCtx = app.ApplicationContext)
                {
                    var contentItemsCompiler = (IContentItemsCompiler)Activator.CreateInstance(
                        gatewayLoader.GatewayAssembly.GetType("UmbracoLinqPad.Gateway.Compilers.ContentItemsCompiler"));
                    var dataContextCompiler = (IDataContextCompiler)Activator.CreateInstance(
                        gatewayLoader.GatewayAssembly.GetType("UmbracoLinqPad.Gateway.Compilers.DataContextCompiler"));

                    var sb = new StringBuilder();
                    
                    //create the content type classes
                    foreach (var compiled in contentItemsCompiler.GenerateClasses(appCtx.RealUmbracoApplicationContext))
                    {
                        sb.Append(compiled);
                    }

                    //add the data context class
                    sb.Append(dataContextCompiler.GenerateClass(typeName, appCtx.RealUmbracoApplicationContext));

                    var result = BuildAssembly(gatewayLoader, sb.ToString(), assemblyToBuild, nameSpace);

                    var dataContextType = result.CompiledAssembly.GetType(string.Format("{0}.{1}", nameSpace, typeName));

                    var properties = dataContextType.GetProperties()
                        //Get all properties of enumerable IGeneratedContentBase
                        .Where(x => typeof(IEnumerable<Models.IGeneratedContentBase>).IsAssignableFrom(x.PropertyType));
                    
                    return new List<ExplorerItem>
                    {
                        new ExplorerItem("Content", ExplorerItemKind.Category, ExplorerIcon.Table)
                        {
                            Children = properties
                                .Select(x => new ExplorerItem(x.Name, ExplorerItemKind.QueryableObject, ExplorerIcon.View)
                                {
                                    IsEnumerable = true
                                }).ToList()                            
                        }
                    };
                }
            }
        }

        private CompilerResults BuildAssembly(GatewayLoader gatewayLoader, string code, AssemblyName name, string ns)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using UmbracoLinqPad;");
            sb.AppendLine("using UmbracoLinqPad.Proxies;");
            sb.AppendLine("using System.IO;");
            sb.AppendLine("using System.Reflection;");
            sb.AppendLine("using UmbracoLinqPad.Gateway;");
            sb.Append("namespace ");
            sb.Append(ns);
            sb.AppendLine(" {"); //open ns
            sb.AppendLine(code);
            sb.Append("}"); //end ns

            // Use the CSharpCodeProvider to compile the generated code:
            CompilerResults results;
            using (var codeProvider = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } }))
            {
                var options = new CompilerParameters(
                    "System.dll System.Core.dll System.Xml.dll".Split(),
                    name.CodeBase,
                    true);

                //add this assembly reference (UmbracoLinqPad)
                options.ReferencedAssemblies.Add(typeof(UmbracoDynamicDriver).Assembly.Location);
                //add the UmbracoLinqPad.Gateway assembly reference
                options.ReferencedAssemblies.Add(gatewayLoader.GatewayAssembly.Location);
                //add the IQToolkit assembly reference
                options.ReferencedAssemblies.Add(gatewayLoader.IqToolkitAssembly.Location);

                results = codeProvider.CompileAssemblyFromSource(options, sb.ToString());
            }
            if (results.Errors.Count > 0)
                throw new Exception
                    ("Cannot compile typed context: " + results.Errors[0].ErrorText + " (line " + results.Errors[0].Line + ")" + "\r\n\r\n" + sb.ToString());

            return results;
        }

    }


}
