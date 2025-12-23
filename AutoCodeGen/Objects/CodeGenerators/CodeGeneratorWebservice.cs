///*
//The MIT License (MIT)

//Copyright (c) 2007 Roger Hill

//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files 
//(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, 
//publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do 
//so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
//MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
//FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN 
//CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//*/

//using System;
//using System.Collections.Generic;
//using System.Text;
//using AutoCodeGen.Objects.CodeGenerators;
//using DAL.Standard.SqlMetadata;

//namespace AutoCodeGenLibrary
//{
//    public class CodeGeneratorWebservice : CodeGeneratorBase, IGenerator
//    {
//        public const string GENERATE_BASE_RESPONSE_OBJECT = "generate_base_response_object";
//        public const string GENERATE_BASE_CONTROLLER = "generate_base_controller";
//        public const string INCLUDE_SESSION_TOKEN = "include_session_token";

//        private const string RESPONSE_BASE_CLASS_NAME = "ResponseBase";
//        private const string PAGING_DATA_CLASS_NAME = "PagingData";
//        private const string CONTROLLER_BASE_CLASS_NAME = "ControllerBase";
//        private const string CONTROLLER_NAMESPACE = "WebApi";

//        public eLanguage Language
//        {
//            get { return eLanguage.Csharp; }
//        }
//        public eCategory Category
//        {
//            get { return eCategory.RestApi; }
//        }
//        public List<string> Methods
//        {
//            get
//            {
//                return new List<string>()
//                {
//                    { "Create WebService 2.0 Controller ", "GenerateWebServiceControllerClass" },
//                    { "Create C# WebService Base Controller", "GenerateWebServiceBaseControllerClass" },
//                    { "Create C# WebService Response Object", "GenerateResponseBaseClass" },
//                    { "Create C# Paging container object", "GeneratePagingClass" },
//                };
//            }
//        }
//        public Dictionary<string, bool> Options
//        {
//            get
//            {
//                return new Dictionary<string, bool>()
//                {
//                    { GENERATE_BASE_CONTROLLER, true},
//                    { INCLUDE_SESSION_TOKEN, true},
//                };
//            }
//        }

//        public CodeGeneratorWebservice() { }

//        public OutputObject GenerateWebServiceControllerClass(SqlTable sqlTable, List<string> namespaceIncludes, IDictionary<string, bool> options)
//        {
//            if (sqlTable == null)
//                throw new ArgumentException("Sql table cannot be null");

//            string controllerName = NameFormatter.ToCSharpClassName(sqlTable.Name);

//            var output = new OutputObject
//            {
//                FileName = $"{controllerName}Controller.cs",
//                Type = OutputObject.eObjectType.CSharp
//            };

//            var sb = new StringBuilder();

//            sb.AppendLine();
//            sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
//            sb.AppendLine("using System;");
//            sb.AppendLine("using TelemetryManager;");
//            sb.AppendLine();

//            if (namespaceIncludes != null && namespaceIncludes.Count > 1)
//                sb.AppendLine(GenerateNamespaceIncludes(namespaceIncludes));

//            sb.AppendLine($"namespace {CONTROLLER_NAMESPACE}");
//            sb.AppendLine("{");

//            sb.AppendLine(AddTabs(1) + "[ApiController]");

//            if (options[GENERATE_BASE_CONTROLLER])
//                sb.AppendLine(AddTabs(1) + $"public class {controllerName}Controller : ApiControllerBase");
//            else
//                sb.AppendLine(AddTabs(1) + $"public class {controllerName}Controller : ControllerBase");

//            sb.AppendLine(AddTabs(1) + "{");

//            sb.AppendLine(AddTabs(2) + $"public {controllerName}Controller(IConfigProvider config, ILogger log, IMetrics metrics, IDataProvider datasource, ISecurityProvider security)");
//            sb.AppendLine(AddTabs(3) + $": base(config, log, metrics, datasource, security) {{ }}");
//            sb.AppendLine();

//            // controllers
//            sb.AppendLine(GenerateGetById(controllerName, options));
//            sb.AppendLine();

//            sb.AppendLine(GenerateGetAll(controllerName, options));
//            sb.AppendLine();

//            //sb.AppendLine(GenerateGetAllPaged(sqlTable.Name));
//            //sb.AppendLine();

//            //sb.AppendLine(GenerateInsert(sqlTable.Name));
//            //sb.AppendLine();

//            //sb.AppendLine(GenerateUpdate(sqlTable.Name));
//            //sb.AppendLine();

//            //sb.AppendLine(GenerateDelete(sqlTable.Name));
//            //sb.AppendLine();

//            sb.AppendLine("}");

//            output.Body = sb.ToString();
//            return output;
//        }

//        public OutputObject GenerateWebServiceBaseControllerClass()
//        {
//            var output = new OutputObject
//            {
//                FileName = CONTROLLER_BASE_CLASS_NAME + ".cs",
//                Type = OutputObject.eObjectType.CSharp
//            };

//            var sb = new StringBuilder();

//            sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
//            sb.AppendLine("using System;");
//            sb.AppendLine("using TelemetryManager;");
//            sb.AppendLine();

//            sb.AppendLine($"namespace {CONTROLLER_NAMESPACE}");
//            sb.AppendLine("{");

//            sb.AppendLine(AddTabs(1) + $"public class ApiControllerBase : ControllerBase");

//            sb.AppendLine(AddTabs(1) + "{");
//            sb.AppendLine(AddTabs(2) + "protected readonly ILogger _Log;");
//            sb.AppendLine(AddTabs(2) + "protected readonly IMetrics _Metric;");
//            sb.AppendLine(AddTabs(2) + "protected readonly IConfigProvider _ConfigProvider;");
//            sb.AppendLine(AddTabs(2) + "protected readonly IDataProvider _Datasource;");
//            sb.AppendLine(AddTabs(2) + "protected readonly ISecurityProvider _SecurityProvider;");
//            sb.AppendLine();

//            sb.AppendLine(AddTabs(2) + "public ApiControllerBase(IConfigProvider config, ILogger log, IMetrics metrics, IDataProvider datasource, ISecurityProvider securityProvider)");
//            sb.AppendLine(AddTabs(2) + "{");
//            sb.AppendLine(AddTabs(3) + "_ConfigProvider = config;");
//            sb.AppendLine(AddTabs(3) + "_Log = log;");
//            sb.AppendLine(AddTabs(3) + "_Metric = metrics;");
//            sb.AppendLine(AddTabs(3) + "_Datasource = datasource;");
//            sb.AppendLine(AddTabs(3) + "_SecurityProvider = securityProvider;");
//            sb.AppendLine(AddTabs(2) + "}");
//            sb.AppendLine();

//            sb.AppendLine(AddTabs(2) + "/// <summary>");
//            sb.AppendLine(AddTabs(2) + "/// Default exception handling for API calls");
//            sb.AppendLine(AddTabs(2) + "/// </summary>");
//            sb.AppendLine(AddTabs(2) + "protected ActionResult<ResponseBase> HandleException(Exception ex)");
//            sb.AppendLine(AddTabs(2) + "{");
//            sb.AppendLine(AddTabs(3) + "_Log.Error(ex);");
//            sb.AppendLine();
//            sb.AppendLine(AddTabs(2) + "    if (_ConfigProvider.GetboolKey(\"config.exceptions.returned\"))");
//            sb.AppendLine(AddTabs(2) + "        return StatusCode(500, new ResponseBase(false, ex.Message));");
//            sb.AppendLine(AddTabs(2) + "    else");
//            sb.AppendLine(AddTabs(2) + "        return StatusCode(500, ResponseBase.SERVER_ERROR);");
//            sb.AppendLine(AddTabs(2) + "}");
//            sb.AppendLine();

//            sb.AppendLine(AddTabs(1) + "}");

//            sb.Append("}");

//            output.Body = sb.ToString();
//            return output;
//        }

//        public OutputObject GenerateResponseBaseClass()
//        {
//            var output = new OutputObject
//            {
//                FileName = RESPONSE_BASE_CLASS_NAME + ".cs",
//                Type = OutputObject.eObjectType.CSharp
//            };

//            var sb = new StringBuilder();

//            sb.AppendLine($"namespace {CONTROLLER_NAMESPACE}");
//            sb.AppendLine("{");

//            // response base class

//            sb.AppendLine(AddTabs(1) + "public class ResponseBase");
//            sb.AppendLine(AddTabs(1) + "{");

//            sb.AppendLine(AddTabs(2) + "public static readonly ResponseBase MISSING_OR_INVALID_ARGUMENTS = new ResponseBase(false, \"One or more parameters were missing or invalid\");");
//            sb.AppendLine(AddTabs(2) + "public static readonly ResponseBase SERVER_ERROR = new ResponseBase(false, \"An server error occurred\");");
//            sb.AppendLine();

//            sb.AppendLine(AddTabs(2) + "public static readonly ResponseBase DEFAULT_FAILURE = new ResponseBase(false, \"A server side error occurred\");");
//            sb.AppendLine(AddTabs(2) + "public static readonly ResponseBase DEFAULT_SUCCESS = new ResponseBase(true, string.Empty);");
//            sb.AppendLine();

//            sb.AppendLine(AddTabs(2) + "public bool Success { get; set; }");
//            sb.AppendLine(AddTabs(2) + "public string Message { get; set; }");
//            sb.AppendLine();

//            sb.AppendLine(AddTabs(2) + "public ResponseBase(bool success, string message)");
//            sb.AppendLine(AddTabs(2) + "{");
//            sb.AppendLine(AddTabs(3) + "Success = success;");
//            sb.AppendLine(AddTabs(3) + "Message = message");
//            sb.AppendLine(AddTabs(2) + "}");

//            sb.AppendLine(AddTabs(1) + "}");
//            sb.AppendLine();

//            // container response class

//            sb.AppendLine(AddTabs(1) + "public class ResponseObject<T> : ResponseBase");
//            sb.AppendLine(AddTabs(1) + "{");
//            sb.AppendLine(AddTabs(2) + "public T Data { get; set; }");
//            sb.AppendLine();

//            sb.AppendLine(AddTabs(2) + "public ResponseObject(bool success, string message, T data) : base(success, message)");
//            sb.AppendLine(AddTabs(2) + "{");
//            sb.AppendLine(AddTabs(3) + "Data = data;");
//            sb.AppendLine(AddTabs(2) + "}");

//            sb.AppendLine(AddTabs(1) + "}");

//            sb.Append("}");

//            output.Body = sb.ToString();
//            return output;
//        }

//        public OutputObject GeneratePagingClass()
//        {
//            var output = new OutputObject
//            {
//                FileName = PAGING_DATA_CLASS_NAME + ".cs",
//                Type = OutputObject.eObjectType.CSharp
//            };

//            var sb = new StringBuilder();

//            sb.AppendLine("using Newtonsoft.Json;");
//            sb.AppendLine();

//            sb.AppendLine($"namespace {CONTROLLER_NAMESPACE}");
//            sb.AppendLine("{");

//            sb.AppendLine(AddTabs(1) + "public class PagingData");
//            sb.AppendLine(AddTabs(1) + "{");
//            sb.AppendLine(AddTabs(2) + $"[JsonProperty(PropertyName = \"skip\")]");
//            sb.AppendLine(AddTabs(2) + "public int Skip { get; set; }");
//            sb.AppendLine();
//            sb.AppendLine(AddTabs(2) + $"[JsonProperty(PropertyName = \"take\")]");
//            sb.AppendLine(AddTabs(2) + "public int Take { get; set; }");
//            sb.AppendLine();
//            sb.AppendLine(AddTabs(2) + "public PagingData() { }");
//            sb.AppendLine(AddTabs(1) + "}");

//            sb.Append("}");

//            output.Body = sb.ToString();
//            return output;
//        }

//        private string GenerateGetById(string controllerName, IDictionary<string, bool> options)
//        {
//            var sb = new StringBuilder();

//            sb.AppendLine(AddTabs(2) + "/// <summary>");
//            sb.AppendLine(AddTabs(2) + $"/// Loads a single {controllerName} by id");
//            sb.AppendLine(AddTabs(2) + "/// </summary>");
//            sb.AppendLine(AddTabs(2) + "[HttpGet]");
//            sb.AppendLine(AddTabs(2) + $"[Route(\"api/v1/{controllerName.ToLower()}/{{id}}\")]");

//            if (options[INCLUDE_SESSION_TOKEN])
//                sb.AppendLine(AddTabs(2) + $"public ActionResult<ResponseBase> Get{controllerName}([FromHeader] string token, int id)");
//            else
//                sb.AppendLine(AddTabs(2) + $"public ActionResult<ResponseBase> Get{controllerName}(int id)");

//            sb.AppendLine(AddTabs(2) + "{");
//            sb.AppendLine(AddTabs(3) + "try");
//            sb.AppendLine(AddTabs(3) + "{");

//            sb.AppendLine(AddTabs(4) + "if (!_SecurityProvider.IsTokenValid(token))");
//            sb.AppendLine(AddTabs(5) + "return ResponseBase.ACCESS_DENIED;");
//            sb.AppendLine();

//            sb.AppendLine(AddTabs(4) + "if (id < 1)");
//            sb.AppendLine(AddTabs(5) + "return ResponseBase.MISSING_OR_INVALID_ARGUMENTS;");
//            sb.AppendLine();

//            sb.AppendLine(AddTabs(4) + $"var results = _Datasource.Load{controllerName}ById(id);");
//            sb.AppendLine();

//            sb.AppendLine(AddTabs(4) + "if (results == null)");
//            sb.AppendLine(AddTabs(5) + $"return Ok(new ResponseBase(false, $\"Unable to load {controllerName} '{{id}}'\"));");
//            sb.AppendLine();

//            sb.AppendLine(AddTabs(4) + $"return Ok(new ResponseObject<{controllerName}>(true, string.Empty, results));");

//            sb.AppendLine(AddTabs(3) + "}");
//            sb.AppendLine(AddTabs(3) + "catch (Exception ex)");
//            sb.AppendLine(AddTabs(3) + "{");

//            if (options[GENERATE_BASE_CONTROLLER])
//                sb.AppendLine(AddTabs(4) + "return HandleException(ex);");
//            else
//                sb.AppendLine(AddTabs(4) + "return StatusCode(500, ResponseBase.DEFAULT_FAILURE);");

//            sb.AppendLine(AddTabs(3) + "}");
//            sb.AppendLine(AddTabs(2) + "}");

//            return sb.ToString();
//        }

//        private string GenerateGetAll(string controllerName, IDictionary<string, bool> options)
//        {
//            var sb = new StringBuilder();

//            sb.AppendLine(AddTabs(2) + "/// <summary>");
//            sb.AppendLine(AddTabs(2) + $"/// Loads all {controllerName}");
//            sb.AppendLine(AddTabs(2) + "/// </summary>");
//            sb.AppendLine(AddTabs(2) + "[HttpGet]");
//            sb.AppendLine(AddTabs(2) + $"[Route(\"api/v1/{controllerName.ToLower()}\")]");

//            if (options[INCLUDE_SESSION_TOKEN])
//                sb.AppendLine(AddTabs(2) + $"public ActionResult<ResponseBase> Get{controllerName}List([FromHeader] string token)");
//            else
//                sb.AppendLine(AddTabs(2) + $"public ActionResult<ResponseBase> Get{controllerName}List()");

//            sb.AppendLine(AddTabs(2) + "{");
//            sb.AppendLine(AddTabs(3) + "try");
//            sb.AppendLine(AddTabs(3) + "{");

//            sb.AppendLine(AddTabs(4) + "if (!_SecurityProvider.IsTokenValid(token))");
//            sb.AppendLine(AddTabs(5) + "return ResponseBase.ACCESS_DENIED;");
//            sb.AppendLine();

//            sb.AppendLine(AddTabs(4) + $"var results = _Datasource.Load{controllerName}();");
//            sb.AppendLine();

//            sb.AppendLine(AddTabs(4) + "if (results == null)");
//            sb.AppendLine(AddTabs(5) + $"return Ok(new ResponseBase(false, $\"Unable to load {controllerName} '{{id}}'\"));");
//            sb.AppendLine();

//            sb.AppendLine(AddTabs(4) + $"return Ok(new ResponseObject<{controllerName}>(true, string.Empty, results));");

//            sb.AppendLine(AddTabs(3) + "}");
//            sb.AppendLine(AddTabs(3) + "catch (Exception ex)");
//            sb.AppendLine(AddTabs(3) + "{");

//            if (options[GENERATE_BASE_CONTROLLER])
//                sb.AppendLine(AddTabs(4) + "return HandleException(ex);");
//            else
//                sb.AppendLine(AddTabs(4) + "return StatusCode(500, ResponseBase.DEFAULT_FAILURE);");

//            sb.AppendLine(AddTabs(3) + "}");
//            sb.AppendLine(AddTabs(2) + "}");

//            sb.AppendLine(AddTabs(1) + "}");

//            return sb.ToString();
//        }
//    }
//}
