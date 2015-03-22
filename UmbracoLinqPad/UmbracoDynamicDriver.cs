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
        private readonly GatewayLoader _gatewayLoader = new GatewayLoader();

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
            //// If the data context happens to be a LINQ to SQL DataContext, we can look up the SQL translation window.
            //var l2s = context as System.Data.Linq.DataContext;
            //if (l2s != null) l2s.Log = executionManager.SqlTranslationWriter;
        }
      
        public override ParameterDescriptor[] GetContextConstructorParameters(IConnectionInfo cxInfo)
        {
            return new[]
            {
                new ParameterDescriptor("gatewayLoader", typeof (GatewayLoader).FullName),
                new ParameterDescriptor("umbracoFolder", typeof (DirectoryInfo).FullName)
            };
        }

        public override object[] GetContextConstructorArguments(IConnectionInfo cxInfo)
        {
            return new object[]
            {
                _gatewayLoader,
                new DirectoryInfo(cxInfo.AppConfigPath)
            };
        }

        public override void OnQueryFinishing(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
        {
            base.OnQueryFinishing(cxInfo, context, executionManager);
        }

        public override void TearDownContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager, object[] constructorArguments)
        {
            ((IDisposable)context).Dispose();
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
            return base.GetProviderFactory(cxInfo);
        }

        public override IDbConnection GetIDbConnection(IConnectionInfo cxInfo)
        {
            return base.GetIDbConnection(cxInfo);
        }

        public override void ExecuteESqlQuery(IConnectionInfo cxInfo, string query)
        {
            base.ExecuteESqlQuery(cxInfo, query);
        }

        public override object OnCustomEvent(string eventName, params object[] data)
        {
            return base.OnCustomEvent(eventName, data);
        }

        public override List<ExplorerItem> GetSchemaAndBuildAssembly(IConnectionInfo cxInfo, AssemblyName assemblyToBuild, ref string nameSpace, ref string typeName)
        {

            nameSpace = "Umbraco.Generated";
            typeName = "UmbracoDataContext";

            using (var app = _gatewayLoader.StartUmbracoApplication(new DirectoryInfo(cxInfo.AppConfigPath)))
            {
                using (var appCtx = app.ApplicationContext)
                {
                    var contentTypeCompiler = (IContentTypeCompiler)Activator.CreateInstance(
                        _gatewayLoader.GatewayAssembly.GetType("UmbracoLinqPad.Gateway.Compilers.ContentTypeCompiler"),
                        appCtx.Services.ContentTypeService);
                    var dataContextCompiler = (IDataContextCompiler)Activator.CreateInstance(
                        _gatewayLoader.GatewayAssembly.GetType("UmbracoLinqPad.Gateway.Compilers.DataContextCompiler"));

                    var sb = new StringBuilder();
                    var ctAliases = appCtx.Services.ContentTypeQuery.GetAllContentTypeAliases().ToArray();
                    
                    //create the content type classes
                    foreach (var alias in ctAliases)
                    {
                        sb.Append(contentTypeCompiler.GenerateClass(alias));
                    }

                    //add the data context class
                    sb.Append(dataContextCompiler.GenerateClass(typeName, ctAliases));

                    BuildAssembly(sb.ToString(), assemblyToBuild, nameSpace);                  

                    return new List<ExplorerItem>
                    {
                        new ExplorerItem("Content", ExplorerItemKind.Category, ExplorerIcon.Box)
                        {
                            Children = appCtx.Services.ContentTypeQuery.GetAllContentTypeAliases()
                                .Select(x => new ExplorerItem(x, ExplorerItemKind.QueryableObject, ExplorerIcon.Table)
                                {
                                    IsEnumerable = true
                                }).ToList()                            
                        }
                    };
                }
            }
        }

        private void BuildAssembly(string code, AssemblyName name, string ns)
        {
            var sb = new StringBuilder();
            sb.Append("using System;");
            sb.Append("using System.Collections.Generic;");
            sb.Append("using System.Collections;");
            sb.Append("using System.Linq;");
            sb.Append("using UmbracoLinqPad;");
            sb.Append("using UmbracoLinqPad.Proxies;");
            sb.Append("using System.IO;");
            sb.Append("using System.Reflection;");
            sb.AppendLine("namespace ");
            sb.Append(ns);
            sb.Append(" {"); //open ns
            sb.Append(code);
            sb.Append("}"); //end ns

            // Use the CSharpCodeProvider to compile the generated code:
            CompilerResults results;
            using (var codeProvider = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } }))
            {
                var options = new CompilerParameters(
                    "System.dll System.Core.dll System.Xml.dll".Split(),
                    name.CodeBase,
                    true);

                //add this assembly reference
                options.ReferencedAssemblies.Add(typeof (UmbracoDynamicDriver).Assembly.Location);
                //add the UmbracoLinqPad.Gateway assembly reference
                options.ReferencedAssemblies.Add(_gatewayLoader.GatewayAssembly.Location);

                results = codeProvider.CompileAssemblyFromSource(options, sb.ToString());
            }
            if (results.Errors.Count > 0)
                throw new Exception
                    ("Cannot compile typed context: " + results.Errors[0].ErrorText + " (line " + results.Errors[0].Line + ")");
        }

    }


}
