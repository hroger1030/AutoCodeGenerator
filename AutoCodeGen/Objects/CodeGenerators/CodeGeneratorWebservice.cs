/*
The MIT License (MIT)

Copyright (c) 2007 Roger Hill

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files 
(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, 
publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do 
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN 
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Text;

using DAL.Standard.SqlMetadata;

namespace AutoCodeGenLibrary
{
    public class CodeGeneratorWebservice : CodeGeneratorBase, IGenerator
    {
        public const string GENERATE_BASE_RESPONSE_OBJECT = "generate_base_response_object";
        public const string GENERATE_BASE_CONTROLLER = "generate_base_controller";

        private const string RESPONSE_BASE_CLASS_NAME = "ResponseBase";
        private const string PAGING_DATA_CLASS_NAME = "PagingData";
        private const string CONTROLLER_BASE_CLASS_NAME = "ControllerBase";
        private const string CONTROLLER_NAMESPACE = "WebApi";

        public eLanguage Language
        {
            get { return eLanguage.Csharp; }
        }
        public eCategory Category
        {
            get { return eCategory.RestApi; }
        }
        public IDictionary<string, string> Methods
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    { "Create WebService 2.0 Controller ", "GenerateWebServiceControllerClass" },
                    { "Create C# WebService Base Controller", "GenerateWebServiceBaseControllerClass" },
                    { "Create C# WebService Response Object", "GenerateResponseBaseClass" },
                };
            }
        }
        public IDictionary<string, bool> Options
        {
            get
            {
                return new Dictionary<string, bool>()
                {
                    //{ CONVERT_NULLABLE_FIELDS, false},
                    //{ INCLUDE_IS_DIRTY_FLAG, false},
                };
            }
        }
        public override string TabType
        {
            get { return "CSharpTabSize"; }
        }
        public int GetHash
        {
            get { return 314; }
        }

        public CodeGeneratorWebservice() { }

        public OutputObject GenerateWebServiceControllerClass(SqlTable sqlTable, List<string> namespaceIncludes, IDictionary<string, bool> options)
        {
            if (sqlTable == null)
                throw new ArgumentException("Sql table cannot be null");

            string class_name = NameFormatter.ToCSharpClassName(sqlTable.Name);

            var output = new OutputObject
            {
                Name = $"{class_name}Controller.cs",
                Type = OutputObject.eObjectType.CSharp
            };

            var sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Reflection;");
            sb.AppendLine();
            sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
            sb.AppendLine();

            if (namespaceIncludes != null && namespaceIncludes.Count > 1)
                sb.AppendLine(GenerateNamespaceIncludes(namespaceIncludes));

            sb.AppendLine($"namespace {CONTROLLER_NAMESPACE}.{NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name)}");
            sb.AppendLine("{");

            sb.AppendLine(AddTabs(1) + "[ApiController]");

            if (options[GENERATE_BASE_CONTROLLER])
                sb.AppendLine(AddTabs(1) + $"public class {sqlTable.Name}Controller : ControllerBase");
            else
                sb.AppendLine(AddTabs(1) + $"public class {sqlTable.Name}Controller : ApiController");

            sb.AppendLine(AddTabs(1) + "{");

            sb.AppendLine(AddTabs(2) + $"public {sqlTable.Name}Controller() {{ }}");
            sb.AppendLine();

            // controllers
            sb.AppendLine(GenerateGetById(sqlTable.Name));
            sb.AppendLine();

            sb.AppendLine(GenerateGetAll(sqlTable.Name));
            sb.AppendLine();

            sb.AppendLine("}");

            output.Body = sb.ToString();
            return output;
        }

        public OutputObject GenerateWebServiceBaseControllerClass()
        {
            var output = new OutputObject
            {
                Name = CONTROLLER_BASE_CLASS_NAME + ".cs",
                Type = OutputObject.eObjectType.CSharp
            };

            var sb = new StringBuilder();

            sb.AppendLine($"namespace {CONTROLLER_NAMESPACE}");
            sb.AppendLine("{");

            sb.AppendLine(AddTabs(1) + $"public class ControllerBase : ApiController");
            sb.AppendLine(AddTabs(1) + "{");
            sb.AppendLine(AddTabs(2) + "// Include whatever logic here that is to be shared by all controllers");
            sb.AppendLine("{");

            sb.AppendLine(AddTabs(2) + $"public ControllerBase() {{ }}");
            sb.AppendLine(AddTabs(1) + "}");

            sb.Append("}");

            output.Body = sb.ToString();
            return output;
        }

        public OutputObject GenerateResponseBaseClass()
        {
            var output = new OutputObject
            {
                Name = RESPONSE_BASE_CLASS_NAME + ".cs",
                Type = OutputObject.eObjectType.CSharp
            };

            var sb = new StringBuilder();

            sb.AppendLine($"namespace {CONTROLLER_NAMESPACE}");
            sb.AppendLine("{");

            // response base class

            sb.AppendLine(AddTabs(1) + "public class ResponseBase");
            sb.AppendLine(AddTabs(1) + "{");

            sb.AppendLine(AddTabs(2) + "public static readonly ResponseBase MISSING_OR_INVALID_ARGUMENTS = new ResponseBase(false, \"One or more parameters were missing or invalid\");");
            sb.AppendLine(AddTabs(2) + "public static readonly ResponseBase SERVER_ERROR = new ResponseBase(false, \"An server error occurred\");");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + "public static readonly ResponseBase DEFAULT_FAILURE = new ResponseBase(false, \"A server side error occurred\");");
            sb.AppendLine(AddTabs(2) + "public static readonly ResponseBase DEFAULT_SUCCESS = new ResponseBase(true, string.Empty);");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + "public bool Success { get; set; }");
            sb.AppendLine(AddTabs(2) + "public string Message { get; set; }");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + "public ResponseBase(bool success, string message)");
            sb.AppendLine(AddTabs(2) + "{");
            sb.AppendLine(AddTabs(3) + "Success = success;");
            sb.AppendLine(AddTabs(3) + "public string Message { get; set; }");
            sb.AppendLine(AddTabs(2) + "}");

            sb.AppendLine(AddTabs(1) + "}");
            sb.AppendLine();

            // container response class

            sb.AppendLine(AddTabs(1) + "public class ResponseObject<T> : ResponseBase");
            sb.AppendLine(AddTabs(1) + "{");
            sb.AppendLine(AddTabs(2) + "public T Data { get; set; }");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + "public ResponseObject(bool success, string message, T data) : base(success, message)");
            sb.AppendLine(AddTabs(2) + "{");
            sb.AppendLine(AddTabs(3) + "Data = data;");
            sb.AppendLine(AddTabs(2) + "}");

            sb.AppendLine(AddTabs(1) + "}");

            sb.Append("}");

            output.Body = sb.ToString();
            return output;
        }

        public OutputObject GeneratePagingClass()
        {
            var output = new OutputObject
            {
                Name = PAGING_DATA_CLASS_NAME + ".cs",
                Type = OutputObject.eObjectType.CSharp
            };

            var sb = new StringBuilder();

            sb.AppendLine("using Newtonsoft.Json;");
            sb.AppendLine();

            sb.AppendLine($"namespace {CONTROLLER_NAMESPACE}");
            sb.AppendLine("{");

            sb.AppendLine(AddTabs(1) + "public class PagingData");
            sb.AppendLine(AddTabs(1) + "{");
            sb.AppendLine(AddTabs(2) + $"[JsonProperty(PropertyName = \"skip\")]");
            sb.AppendLine(AddTabs(2) + "public int Skip { get; set; }");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + $"[JsonProperty(PropertyName = \"take\")]");
            sb.AppendLine(AddTabs(2) + "public int Take { get; set; }");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "public PagingData() { }");
            sb.AppendLine(AddTabs(1) + "}");

            sb.Append("}");

            output.Body = sb.ToString();
            return output;
        }

        private string GenerateGetById(string controllerName)
        {
            var sb = new StringBuilder();

            sb.AppendLine(AddTabs(2) + "[HttpGet]");
            sb.AppendLine(AddTabs(2) + $"[Route(\"api/v1/{controllerName.ToLower()}/{{id}}\")]");

            sb.AppendLine(AddTabs(2) + $"public ActionResult<ResponseBase> Get{controllerName}(int id)");
            sb.AppendLine(AddTabs(2) + "{");
            sb.AppendLine(AddTabs(3) + "try");
            sb.AppendLine(AddTabs(3) + "{");

            // todo: wire up to DAL

            sb.AppendLine(AddTabs(4) + "return Ok(ResponseBase.DEFAULT_SUCCESS;");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine(AddTabs(3) + "catch (Exception ex)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "// log exception here");
            sb.AppendLine(AddTabs(4) + "return StatusCode(500, ResponseBase.DEFAULT_FAILURE);");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine(AddTabs(2) + "}");

            sb.AppendLine(AddTabs(1) + "}");

            return sb.ToString();
        }

        private string GenerateGetAll(string controllerName)
        {
            // todo paging?

            var sb = new StringBuilder();

            sb.AppendLine(AddTabs(2) + "[HttpGet]");
            sb.AppendLine(AddTabs(2) + $"[Route(\"api/v1/{controllerName.ToLower()}/all\")]");

            sb.AppendLine(AddTabs(2) + $"public ActionResult<ResponseBase> GetAll{controllerName}(int id)");
            sb.AppendLine(AddTabs(2) + "{");
            sb.AppendLine(AddTabs(3) + "try");
            sb.AppendLine(AddTabs(3) + "{");

            // todo: wire up to DAL

            sb.AppendLine(AddTabs(4) + "return Ok(ResponseBase.DEFAULT_SUCCESS;");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine(AddTabs(3) + "catch (Exception ex)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "// log exception here");
            sb.AppendLine(AddTabs(4) + "return StatusCode(500, ResponseBase.DEFAULT_FAILURE);");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine(AddTabs(2) + "}");

            sb.AppendLine(AddTabs(1) + "}");

            return sb.ToString();
        }
    }
}

/*
Need:
GetById
GetAll
GetAllPaged
Insert
Update

    
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace WebApi
{
    [ApiController]
    public class HealthController : ApiControllerBase
    {
        public HealthController(IDictionary<string, object> config, ILogger log, IMetrics metrics, IDataSource datasource, IDictionary<string, User> users) : base(config, log, metrics, datasource, users) { }

        [HttpGet]
        [Route("api/v1/health/status")]
        public ActionResult<ResponseBase> GetServerHealth()
        {
            try
            {
                _Metrics.IncrementCounter(MethodBase.GetCurrentMethod().Name);
                return Ok(new ResponseBase(true, "Healthy"));
            }
            catch (Exception ex)
            {
                return ManageException(ex);
            }
        }

        [HttpGet]
        [Route("api/v1/health/time")]
        public ActionResult<ResponseBase> ServerTime()
        {
            try
            {
                _Metrics.IncrementCounter(MethodBase.GetCurrentMethod().Name);
                return Ok(new ResponseBase(true, DateTime.UtcNow.ToString()));
            }
            catch (Exception ex)
            {
                return ManageException(ex);
            }
        }

        [HttpGet]
        [Route("api/v1/health/version")]
        public ActionResult<ResponseBase> ApplicationVersion()
        {
            try
            {
                _Metrics.IncrementCounter(MethodBase.GetCurrentMethod().Name);
                return Ok(new ResponseBase(true, GetType().Assembly.GetName().Version.ToString()));
            }
            catch (Exception ex)
            {
                return ManageException(ex);
            }
        }
    }
}

 
*/
