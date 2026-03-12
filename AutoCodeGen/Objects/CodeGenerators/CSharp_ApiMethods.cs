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

using DAL.Net.SqlMetadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCodeGen
{
    public class CSharp_ApiMethods : IGenerator
    {
        private const string OutputPath = "C#\\";

        private const string FEATURE_POCO_CLASS = "POCO class";
        private const string FEATURE_API_ENDPOINT = "API CRUD Endpoint";
        private const string FEATURE_ORM_LOADER = "ORM Loader object";

        // option names
        private const string NAMESPACE_INCLUDES = "Included namespaces";

        public string Language => "C#";
        public string Category => "MiddleTier";
        public string Name => "C# APIs/ORMs";
        public string Description => "Generates various API objects, ORM objects, POCOs, and other classes based on database tables.";
        public string[] FeatureNames => [FEATURE_POCO_CLASS, FEATURE_API_ENDPOINT, FEATURE_ORM_LOADER];

        public Dictionary<string, string> DefaultOptions => new()
        {
            { NAMESPACE_INCLUDES, string.Empty },
        };

        public CSharp_ApiMethods() { }

        public OutputObject Process(string feature, SqlTable sqlTable, Dictionary<string, string> options)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(feature, nameof(feature));
            ArgumentNullException.ThrowIfNull(sqlTable, nameof(sqlTable));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            return feature switch
            {
                FEATURE_POCO_CLASS => GeneratePoco(sqlTable, options),
                FEATURE_API_ENDPOINT => GenerateApiEndpoint(sqlTable, options),
                FEATURE_ORM_LOADER => GenerateSqlDataLoader(sqlTable, options),

                _ => throw new ArgumentOutOfRangeException($"Mode {feature} is not supported by {Name} generator."),
            };
        }

        public OutputObject GeneratePoco(SqlTable sqlTable, Dictionary<string, string> options)
        {
            string className = NameFormatter.ToCSharpClassName(sqlTable.Name);
            var sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine();

            if (!string.IsNullOrWhiteSpace(options[NAMESPACE_INCLUDES]))
            {
                var namespaces = Helper.StringToList(options[NAMESPACE_INCLUDES]);

                foreach (var item in namespaces)
                    sb.AppendLine($"Using {item};");

                sb.AppendLine();
            }

            sb.AppendLine($"namespace {NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name)}.Orm");
            sb.AppendLine("{");

            sb.AppendLine(Helper.AddTabs(1) + $"public class {className}");
            sb.AppendLine(Helper.AddTabs(1) + "{");

            #region Properties Block
            ////////////////////////////////////////////////////////////////////////////////

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                #region Sample Output
                //public string SomeID { get; set; }
                #endregion Sample Output

                sb.AppendLine(Helper.AddTabs(2) + $"public {NameFormatter.SQLTypeToCSharpType(sql_column)} {NameFormatter.ToCSharpPropertyName(sql_column.Name)} {{ get; set; }}");
            }

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region Default CTOR
            ////////////////////////////////////////////////////////////////////////////////
            // sample output
            //public Foo() { }

            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(2) + $"public {NameFormatter.ToCSharpClassName(sqlTable.Name)}() {{ }}");

            ////////////////////////////////////////////////////////////////////////////////
            #endregion Default CTOR

            sb.AppendLine(Helper.AddTabs(1) + "}");
            sb.Append('}');

            return new OutputObject
            {
                FileName = $"{className}.cs",
                Body = sb.ToString(),
                OutputPath = $"{OutputPath}\\pocos",
            };
        }

        public OutputObject GenerateApiEndpoint(SqlTable sqlTable, Dictionary<string, string> options)
        {
            string className = NameFormatter.ToCSharpClassName(sqlTable.Name);
            string controllerName = className.ToLower();
            string uiType = $"{className}Ui";

            // Try to find a primary key column (first int/identity column, fallback to first column)
            var pkColumn = sqlTable.Columns.Values.FirstOrDefault(c => c.IsIdentity)
                           ?? sqlTable.Columns.Values.FirstOrDefault(c => c.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                           ?? sqlTable.Columns.Values.First();

            string pkName = NameFormatter.ToCSharpPropertyName(pkColumn.Name);
            string pkType = NameFormatter.SQLTypeToCSharpType(pkColumn);
            string pkParamName = pkName.Length > 0 ? char.ToLower(pkName[0]) + pkName.Substring(1) : "id";

            // Collect non-PK columns for create/update bodies
            var writableColumns = sqlTable.Columns.Values
                .Where(c => !c.IsIdentity)
                .ToList();

            var sb = new StringBuilder();

            #region Usings + Namespace
            ////////////////////////////////////////////////////////////////////////////////
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
            sb.AppendLine();

            if (options.TryGetValue(NAMESPACE_INCLUDES, out var nsValue) && !string.IsNullOrWhiteSpace(nsValue))
            {
                var namespaces = Helper.StringToList(nsValue);

                foreach (var item in namespaces)
                    sb.AppendLine($"using {item};");

                sb.AppendLine();
            }

            sb.AppendLine($"namespace {NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name)}.Controllers");
            sb.AppendLine("{");
            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region Class Declaration
            ////////////////////////////////////////////////////////////////////////////////
            sb.AppendLine(Helper.AddTabs(1) + "[ApiController]");
            sb.AppendLine(Helper.AddTabs(1) + $"public class {className}Controller : ApiControllerBase<{className}Controller>");
            sb.AppendLine(Helper.AddTabs(1) + "{");
            sb.AppendLine(Helper.AddTabs(2) + $"private const string CONTROLLER_NAME = \"{controllerName}\";");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(2) + "private readonly ValidationManager _ValidationManager;");
            sb.AppendLine();
            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region Constructor
            ////////////////////////////////////////////////////////////////////////////////
            // sample output
            // public FooController(IConfigProvider configProvider, IDataProvider dataProvider) : base(configProvider, dataProvider)
            // {
            //     _ValidationManager = new ValidationManager(_ConfigProvider);
            // }

            sb.AppendLine(Helper.AddTabs(2) + $"public {className}Controller(IConfigProvider configProvider, IDataProvider dataProvider) : base(configProvider, dataProvider)");
            sb.AppendLine(Helper.AddTabs(2) + "{");
            sb.AppendLine(Helper.AddTabs(3) + "_ValidationManager = new ValidationManager(_ConfigProvider);");
            sb.AppendLine(Helper.AddTabs(2) + "}");
            sb.AppendLine();
            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region Create Endpoint
            ////////////////////////////////////////////////////////////////////////////////
            // region sample output
            // [HttpPost($"{PATH_ROOT}/{CONTROLLER_NAME}/create")]
            // public async Task<ActionResult<ResponseBase>> CreateFoo(
            //     [FromHeader(Name = "Authorization")] string accessToken,
            //     [FromForm] string name, ...)
            // { ... }

            sb.AppendLine(Helper.AddTabs(2) + $"[HttpPost($\"{{PATH_ROOT}}/{{CONTROLLER_NAME}}/create\")]");
            sb.AppendLine(Helper.AddTabs(2) + $"public async Task<ActionResult<ResponseBase>> Create{className}(");
            sb.AppendLine(Helper.AddTabs(3) + "[FromHeader(Name = \"Authorization\")] string accessToken,");

            for (int i = 0; i < writableColumns.Count; i++)
            {
                var col = writableColumns[i];
                string csharpType = NameFormatter.SQLTypeToCSharpType(col);
                string propName = NameFormatter.ToCSharpPropertyName(col.Name);
                string paramName = char.ToLower(propName[0]) + propName.Substring(1);
                string comma = (i < writableColumns.Count - 1) ? "," : ")";

                sb.AppendLine(Helper.AddTabs(3) + $"[FromForm] {csharpType} {paramName}{comma}");
            }

            sb.AppendLine(Helper.AddTabs(2) + "{");
            sb.AppendLine(Helper.AddTabs(3) + "try");
            sb.AppendLine(Helper.AddTabs(3) + "{");
            sb.AppendLine(Helper.AddTabs(4) + "if (string.IsNullOrEmpty(accessToken))");
            sb.AppendLine(Helper.AddTabs(5) + "return StatusCode(Http.Unauthorized, ResponseBase.AUTH_TOKEN_MISSING);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "var user = await _AuthManager.AuthenticateAndAuthorizeUserTokenAsync(accessToken, ePermissions.User);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "if (user == null)");
            sb.AppendLine(Helper.AddTabs(5) + "return StatusCode(Http.Forbidden, ResponseBase.FAILED_AUTH);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "WriteTokenHeaders(user.AccessToken);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "// validate input");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + $"var new{className} = new {className}()");
            sb.AppendLine(Helper.AddTabs(4) + "{");
            sb.AppendLine(Helper.AddTabs(5) + $"{pkName} = 0,");
            sb.AppendLine(Helper.AddTabs(5) + "AuthorId = user.Profile.Id,");

            foreach (var col in writableColumns)
            {
                string propName = NameFormatter.ToCSharpPropertyName(col.Name);
                string paramName = char.ToLower(propName[0]) + propName.Substring(1);
                string csharpType = NameFormatter.SQLTypeToCSharpType(col);

                // Apply SanitizeInput for string types
                if (csharpType == "string")
                    sb.AppendLine(Helper.AddTabs(5) + $"{propName} = {paramName}.SanitizeInput(Defaults.SHORT_STRING_LENGTH),");
                else
                    sb.AppendLine(Helper.AddTabs(5) + $"{propName} = {paramName},");
            }

            sb.AppendLine(Helper.AddTabs(4) + "};");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + $"var validationResult = _ValidationManager.Is{className}Valid(new{className});");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "if (!validationResult.IsValid)");
            sb.AppendLine(Helper.AddTabs(5) + $"return StatusCode(Http.UnprocessableEntity, new ResponseBase(false, \"Invalid {controllerName} properties\", validationResult.Errors));");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + $"var results = await _DataProvider.Create{className}ProfileAsync(new{className});");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "if (!results)");
            sb.AppendLine(Helper.AddTabs(5) + $"return StatusCode(Http.InternalServerError, new ResponseBase(false, \"Failed to create {controllerName}\"));");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + $"return Ok(new ResponseBase(true, \"{className} created\", new DbObjectIdUi {{ ObjectId = new{className}.{pkName}, ObjectType = new EnumeratedValue(eObjectType.{className}) }}));");
            sb.AppendLine(Helper.AddTabs(3) + "}");
            sb.AppendLine(Helper.AddTabs(3) + "catch (Exception ex)");
            sb.AppendLine(Helper.AddTabs(3) + "{");
            sb.AppendLine(Helper.AddTabs(4) + "return HandleException(ex);");
            sb.AppendLine(Helper.AddTabs(3) + "}");
            sb.AppendLine(Helper.AddTabs(2) + "}");
            sb.AppendLine();
            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region Update Endpoint
            ////////////////////////////////////////////////////////////////////////////////
            // sample output
            // [HttpPut($"{PATH_ROOT}/{CONTROLLER_NAME}/update")]
            // public async Task<ActionResult<ResponseBase>> UpdateFoo(...) { ... }

            sb.AppendLine(Helper.AddTabs(2) + $"[HttpPut($\"{{PATH_ROOT}}/{{CONTROLLER_NAME}}/update\")]");
            sb.AppendLine(Helper.AddTabs(2) + $"public async Task<ActionResult<ResponseBase>> Update{className}(");
            sb.AppendLine(Helper.AddTabs(3) + "[FromHeader(Name = \"Authorization\")] string accessToken,");
            sb.AppendLine(Helper.AddTabs(3) + $"[FromForm] {pkType} {pkParamName},");

            for (int i = 0; i < writableColumns.Count; i++)
            {
                var col = writableColumns[i];

                string csharpType = NameFormatter.SQLTypeToCSharpType(col);
                string propName = NameFormatter.ToCSharpPropertyName(col.Name);
                string paramName = char.ToLower(propName[0]) + propName.Substring(1);
                string comma = (i < writableColumns.Count - 1) ? "," : ")";

                sb.AppendLine(Helper.AddTabs(3) + $"[FromForm] {csharpType} {paramName}{comma}");
            }

            sb.AppendLine(Helper.AddTabs(2) + "{");
            sb.AppendLine(Helper.AddTabs(3) + "try");
            sb.AppendLine(Helper.AddTabs(3) + "{");
            sb.AppendLine(Helper.AddTabs(4) + "if (string.IsNullOrEmpty(accessToken))");
            sb.AppendLine(Helper.AddTabs(5) + "return StatusCode(Http.Unauthorized, ResponseBase.AUTH_TOKEN_MISSING);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "var user = await _AuthManager.AuthenticateAndAuthorizeUserTokenAsync(accessToken, ePermissions.Admin);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "if (user == null)");
            sb.AppendLine(Helper.AddTabs(5) + "return StatusCode(Http.Forbidden, ResponseBase.FAILED_AUTH);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "WriteTokenHeaders(user.AccessToken);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + $"if ({pkParamName} < 1)");
            sb.AppendLine(Helper.AddTabs(5) + "return StatusCode(Http.UnprocessableEntity, ResponseBase.MISSING_OR_INVALID_ARGUMENTS);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + $"var existing{className} = await _DataProvider.Load{className}ByIdAsync({pkParamName});");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + $"if (existing{className} == null)");
            sb.AppendLine(Helper.AddTabs(5) + "return StatusCode(Http.UnprocessableEntity, ResponseBase.SERVER_ERROR);");
            sb.AppendLine();

            foreach (var col in writableColumns)
            {
                string propName = NameFormatter.ToCSharpPropertyName(col.Name);
                string paramName = char.ToLower(propName[0]) + propName.Substring(1);
                string csharpType = NameFormatter.SQLTypeToCSharpType(col);

                if (csharpType == "string")
                    sb.AppendLine(Helper.AddTabs(4) + $"existing{className}.Profile.{propName} = {paramName}.SanitizeInput(Defaults.SHORT_STRING_LENGTH);");
                else
                    sb.AppendLine(Helper.AddTabs(4) + $"existing{className}.Profile.{propName} = {paramName};");
            }

            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + $"var results = await _DataProvider.Update{className}ProfileAsync(existing{className}.Profile);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "if (!results)");
            sb.AppendLine(Helper.AddTabs(5) + $"return StatusCode(Http.InternalServerError, new ResponseBase(false, \"Failed to update {controllerName}\"));");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "return Ok(new ResponseBase(true, string.Empty, results));");
            sb.AppendLine(Helper.AddTabs(3) + "}");
            sb.AppendLine(Helper.AddTabs(3) + "catch (Exception ex)");
            sb.AppendLine(Helper.AddTabs(3) + "{");
            sb.AppendLine(Helper.AddTabs(4) + "return HandleException(ex);");
            sb.AppendLine(Helper.AddTabs(3) + "}");
            sb.AppendLine(Helper.AddTabs(2) + "}");
            sb.AppendLine();
            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region Get By Id Endpoint
            ////////////////////////////////////////////////////////////////////////////////
            // sample output
            // [HttpGet($"{PATH_ROOT}/{CONTROLLER_NAME}/load/{fooId}")]
            // public async Task<ActionResult<ResponseBase>> GetFoo(...) { ... }

            sb.AppendLine(Helper.AddTabs(2) + $"[HttpGet($\"{{PATH_ROOT}}/{{CONTROLLER_NAME}}/load/{{{{{pkParamName}}}}}\")]");
            sb.AppendLine(Helper.AddTabs(2) + $"public async Task<ActionResult<ResponseBase>> Get{className}(");
            sb.AppendLine(Helper.AddTabs(3) + "[FromHeader(Name = \"Authorization\")] string accessToken,");
            sb.AppendLine(Helper.AddTabs(3) + $"[FromRoute] {pkType} {pkParamName})");
            sb.AppendLine(Helper.AddTabs(2) + "{");
            sb.AppendLine(Helper.AddTabs(3) + "try");
            sb.AppendLine(Helper.AddTabs(3) + "{");
            sb.AppendLine(Helper.AddTabs(4) + "if (string.IsNullOrEmpty(accessToken))");
            sb.AppendLine(Helper.AddTabs(5) + "return StatusCode(Http.Unauthorized, ResponseBase.AUTH_TOKEN_MISSING);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "var user = await _AuthManager.AuthenticateAndAuthorizeUserTokenAsync(accessToken, ePermissions.User);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "if (user == null)");
            sb.AppendLine(Helper.AddTabs(5) + "return StatusCode(Http.Forbidden, ResponseBase.FAILED_AUTH);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "WriteTokenHeaders(user.AccessToken);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + $"if ({pkParamName} < 0)");
            sb.AppendLine(Helper.AddTabs(5) + $"return StatusCode(Http.BadRequest, new ResponseBase(false, \"Invalid {pkParamName} value\"));");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + $"var results = await _DataProvider.Load{className}ByIdAsync({pkParamName});");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "if (results == null)");
            sb.AppendLine(Helper.AddTabs(5) + $"return StatusCode(Http.InternalServerError, new ResponseBase(false, \"Failed to load {controllerName}\"));");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "return Ok(new ResponseBase(true, string.Empty, results));");
            sb.AppendLine(Helper.AddTabs(3) + "}");
            sb.AppendLine(Helper.AddTabs(3) + "catch (Exception ex)");
            sb.AppendLine(Helper.AddTabs(3) + "{");
            sb.AppendLine(Helper.AddTabs(4) + "return HandleException(ex);");
            sb.AppendLine(Helper.AddTabs(3) + "}");
            sb.AppendLine(Helper.AddTabs(2) + "}");
            sb.AppendLine();
            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region Load List Endpoint
            ////////////////////////////////////////////////////////////////////////////////
            // #region sample output
            // [HttpGet($"{PATH_ROOT}/{CONTROLLER_NAME}/load-{controllerName}s/skip/{skip}/take/{take}")]
            // public async Task<ActionResult<ResponseBase>> LoadUser{className}List(...) { ... }

            sb.AppendLine(Helper.AddTabs(2) + $"[HttpGet($\"{{PATH_ROOT}}/{{CONTROLLER_NAME}}/load-{controllerName}s/skip/{{{{skip}}}}/take/{{{{take}}}}\")]");
            sb.AppendLine(Helper.AddTabs(2) + $"public async Task<ActionResult<ResponseBase>> LoadUser{className}List(");
            sb.AppendLine(Helper.AddTabs(3) + "[FromHeader(Name = \"Authorization\")] string accessToken,");
            sb.AppendLine(Helper.AddTabs(3) + "[FromRoute] int skip = Defaults.DEFAULT_SKIP,");
            sb.AppendLine(Helper.AddTabs(3) + "[FromRoute] int take = Defaults.DEFAULT_TAKE)");
            sb.AppendLine(Helper.AddTabs(2) + "{");
            sb.AppendLine(Helper.AddTabs(3) + "try");
            sb.AppendLine(Helper.AddTabs(3) + "{");
            sb.AppendLine(Helper.AddTabs(4) + "if (string.IsNullOrEmpty(accessToken))");
            sb.AppendLine(Helper.AddTabs(5) + "return StatusCode(Http.Unauthorized, ResponseBase.AUTH_TOKEN_MISSING);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "var user = await _AuthManager.AuthenticateAndAuthorizeUserTokenAsync(accessToken, ePermissions.User);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "if (user == null)");
            sb.AppendLine(Helper.AddTabs(5) + "return StatusCode(Http.Forbidden, ResponseBase.FAILED_AUTH);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "WriteTokenHeaders(user.AccessToken);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "if (skip < 0)");
            sb.AppendLine(Helper.AddTabs(5) + "return StatusCode(Http.UnprocessableEntity, new ResponseBase(false, \"skip cannot be less than 0\"));");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "if (take < 1)");
            sb.AppendLine(Helper.AddTabs(5) + "return StatusCode(Http.UnprocessableEntity, new ResponseBase(false, \"take cannot be less than 1\"));");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + $"var results = await _DataProvider.Load{className}ListByUserIdAsync(user.Profile.Id, skip, take);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "if (results == null)");
            sb.AppendLine(Helper.AddTabs(5) + $"return StatusCode(Http.InternalServerError, new ResponseBase(false, \"Failed to load {controllerName} list\"));");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + $"var output = new PagedList<{uiType}>()");
            sb.AppendLine(Helper.AddTabs(4) + "{");
            sb.AppendLine(Helper.AddTabs(5) + "Skip = skip,");
            sb.AppendLine(Helper.AddTabs(5) + "Take = take,");
            sb.AppendLine(Helper.AddTabs(5) + "TotalCount = results.TotalCount,");
            sb.AppendLine(Helper.AddTabs(5) + $"PageData = results.PageData.Select(x => _UiConverter.Convert{className}ToUi(x)).ToList(),");
            sb.AppendLine(Helper.AddTabs(4) + "};");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "return Ok(new ResponseBase(true, string.Empty, output));");
            sb.AppendLine(Helper.AddTabs(3) + "}");
            sb.AppendLine(Helper.AddTabs(3) + "catch (Exception ex)");
            sb.AppendLine(Helper.AddTabs(3) + "{");
            sb.AppendLine(Helper.AddTabs(4) + "return HandleException(ex);");
            sb.AppendLine(Helper.AddTabs(3) + "}");
            sb.AppendLine(Helper.AddTabs(2) + "}");
            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region Delete Endpoint
            ////////////////////////////////////////////////////////////////////////////////
            // sample output
            // [HttpDelete($"{PATH_ROOT}/{CONTROLLER_NAME}/delete/{fooId}")]
            // public async Task<ActionResult<ResponseBase>> DeleteFoo(...) { ... }

            sb.AppendLine(Helper.AddTabs(2) + $"[HttpDelete($\"{{PATH_ROOT}}/{{CONTROLLER_NAME}}/delete/{{{{{pkParamName}}}}}\")]");
            sb.AppendLine(Helper.AddTabs(2) + $"public async Task<ActionResult<ResponseBase>> Delete{className}(");
            sb.AppendLine(Helper.AddTabs(3) + "[FromHeader(Name = \"Authorization\")] string accessToken,");
            sb.AppendLine(Helper.AddTabs(3) + $"[FromRoute] {pkType} {pkParamName})");
            sb.AppendLine(Helper.AddTabs(2) + "{");
            sb.AppendLine(Helper.AddTabs(3) + "try");
            sb.AppendLine(Helper.AddTabs(3) + "{");
            sb.AppendLine(Helper.AddTabs(4) + "if (string.IsNullOrEmpty(accessToken))");
            sb.AppendLine(Helper.AddTabs(5) + "return StatusCode(Http.Unauthorized, ResponseBase.AUTH_TOKEN_MISSING);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "var user = await _AuthManager.AuthenticateAndAuthorizeUserTokenAsync(accessToken, ePermissions.Admin);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "if (user == null)");
            sb.AppendLine(Helper.AddTabs(5) + "return StatusCode(Http.Forbidden, ResponseBase.FAILED_AUTH);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "WriteTokenHeaders(user.AccessToken);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + $"if ({pkParamName} < 1)");
            sb.AppendLine(Helper.AddTabs(5) + "return StatusCode(Http.UnprocessableEntity, ResponseBase.MISSING_OR_INVALID_ARGUMENTS);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + $"var existing{className} = await _DataProvider.Load{className}ByIdAsync({pkParamName});");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + $"if (existing{className} == null)");
            sb.AppendLine(Helper.AddTabs(5) + "return StatusCode(Http.UnprocessableEntity, ResponseBase.SERVER_ERROR);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + $"var results = await _DataProvider.Delete{className}ByIdAsync({pkParamName});");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "if (!results)");
            sb.AppendLine(Helper.AddTabs(5) + $"return StatusCode(Http.InternalServerError, new ResponseBase(false, \"Failed to delete {controllerName}\"));");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "return Ok(new ResponseBase(true, string.Empty, results));");
            sb.AppendLine(Helper.AddTabs(3) + "}");
            sb.AppendLine(Helper.AddTabs(3) + "catch (Exception ex)");
            sb.AppendLine(Helper.AddTabs(3) + "{");
            sb.AppendLine(Helper.AddTabs(4) + "return HandleException(ex);");
            sb.AppendLine(Helper.AddTabs(3) + "}");
            sb.AppendLine(Helper.AddTabs(2) + "}");
            sb.AppendLine();
            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            sb.AppendLine(Helper.AddTabs(1) + "}");
            sb.Append('}');

            return new OutputObject
            {
                FileName = $"{className}Controller.cs",
                Body = sb.ToString(),
                OutputPath = $"{OutputPath}\\controllers",
            };
        }

        public OutputObject GenerateSqlDataLoader(SqlTable sqlTable, Dictionary<string, string> options)
        {
            string className = NameFormatter.ToCSharpClassName(sqlTable.Name);
            string namespaceName = NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name);

            // Identify PK column
            var pkColumn = sqlTable.Columns.Values.FirstOrDefault(c => c.IsIdentity)
                           ?? sqlTable.Columns.Values.FirstOrDefault(c => c.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                           ?? sqlTable.Columns.Values.First();

            string pkPropName = NameFormatter.ToCSharpPropertyName(pkColumn.Name);
            string pkType = NameFormatter.SQLTypeToCSharpType(pkColumn);
            string pkParamName = char.ToLower(pkPropName[0]) + pkPropName.Substring(1);

            // All non-PK columns used for insert/update parameters
            var writableColumns = sqlTable.Columns.Values
                .Where(c => !c.IsIdentity)
                .ToList();

            var sb = new StringBuilder();

            #region Namespace + Class Header
            ////////////////////////////////////////////////////////////////////////////////
            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine(Helper.AddTabs(1) + "public partial class SqlDataLoader : IDataProvider");
            sb.AppendLine(Helper.AddTabs(1) + "{");
            sb.AppendLine(Helper.AddTabs(2) + $"// {className}");
            sb.AppendLine("{");
            sb.AppendLine();
            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region Create Method
            ////////////////////////////////////////////////////////////////////////////////
            // region sample output
            // public async Task<bool> CreateFooAsync(Foo foo)
            // {
            //     ArgumentNullException.ThrowIfNull(foo);
            //
            //     var parameters = new SqlParameter[]
            //     {   
            //         new() { ParameterName = "@ObjectTypeId", SqlDbType = SqlDbType.int, Size = 4, Value = foo.Id },
            //         new() { ParameterName = "@TextField", SqlDbType = SqlDbType.varchar, Size = 100, Value = foo.TextField },
            //     };
            //
            //     var count = await _Database.ExecuteNonQuerySpAsync("[dbo].[Foo_Insert]", parameters);
            //     return (count > 0);
            // }

            sb.AppendLine(Helper.AddTabs(2) + $"public async Task<bool> Create{className}Async({className} {NameFormatter.ToCSharpLocalVariable(sqlTable.Name)})");
            sb.AppendLine(Helper.AddTabs(2) + "{");
            sb.AppendLine(Helper.AddTabs(3) + $"ArgumentNullException.ThrowIfNull({char.ToLower(className[0]) + className.Substring(1)});");

            // Guard for PK if it's not the identity (edge case)
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(3) + "var parameters = new SqlParameter[]");
            sb.AppendLine(Helper.AddTabs(3) + "{");

            foreach (var col in sqlTable.Columns.Values)
            {
                if (col.IsNullable)
                    sb.AppendLine(Helper.AddTabs(4) + $"new() {{ ParameterName = \"@{col.Name}\", SqlDbType = SqlDbType.{col.DataType}, Size = {col.Length}, Value = (object){className}.{NameFormatter.ToCSharpPropertyName(col.Name)} ?? DBNull.Value }},");
                else
                    sb.AppendLine(Helper.AddTabs(4) + $"new() {{ ParameterName = \"@{col.Name}\", SqlDbType = SqlDbType.{col.DataType}, Size = {col.Length}, Value = {className}.{NameFormatter.ToCSharpPropertyName(col.Name)} }},");
            }

            sb.AppendLine(Helper.AddTabs(3) + "};");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(3) + $"var count = await _Database.ExecuteNonQuerySpAsync(\"[dbo].[{className}_Insert]\", parameters);");
            sb.AppendLine(Helper.AddTabs(3) + "return (count > 0);");
            sb.AppendLine(Helper.AddTabs(2) + "}");
            sb.AppendLine();
            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region Update Method
            ////////////////////////////////////////////////////////////////////////////////
            // sample output
            // public async Task<bool> UpdateFooAsync(Foo foo)
            // {
            //     ArgumentNullException.ThrowIfNull(foo);
            //     ...
            //     var count = await _Database.ExecuteNonQuerySpAsync("[dbo].[Foo_Update]", parameters);
            //     return (count > 0);
            // }

            string objVarName = char.ToLower(className[0]) + className.Substring(1);

            sb.AppendLine(Helper.AddTabs(2) + $"public async Task<bool> Update{className}Async({className} {objVarName})");
            sb.AppendLine(Helper.AddTabs(2) + "{");
            sb.AppendLine(Helper.AddTabs(3) + $"ArgumentNullException.ThrowIfNull({objVarName});");
            sb.AppendLine(Helper.AddTabs(3) + $"ArgumentOutOfRangeException.ThrowIfLessThan({objVarName}.{pkPropName}, 1);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(3) + "var parameters = new SqlParameter[]");
            sb.AppendLine(Helper.AddTabs(3) + "{");

            // PK first in update
            {
                string sqlType = NameFormatter.SQLTypeToCSharpType(pkColumn);
                sb.AppendLine(Helper.AddTabs(4) + $"new() {{ ParameterName = \"@{pkPropName}\", SqlDbType = SqlDbType.{sqlType}, Value = {objVarName}.{pkPropName} }},");
            }

            foreach (var col in sqlTable.Columns.Values)
            {
                if (col.IsNullable)
                    sb.AppendLine(Helper.AddTabs(4) + $"new() {{ ParameterName = \"@{col.Name}\", SqlDbType = SqlDbType.{col.DataType}, Size = {col.Length} Value = (object){className}.{NameFormatter.ToCSharpPropertyName(col.Name)} ?? DBNull.Value }},");
                else
                    sb.AppendLine(Helper.AddTabs(4) + $"new() {{ ParameterName = \"@{col.Name}\", SqlDbType = SqlDbType.{col.DataType}, Size = {col.Length} Value = {className}.{NameFormatter.ToCSharpPropertyName(col.Name)} }},");
            }

            sb.AppendLine(Helper.AddTabs(3) + "};");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(3) + $"var count = await _Database.ExecuteNonQuerySpAsync(\"[dbo].[{className}_Update]\", parameters);");
            sb.AppendLine(Helper.AddTabs(3) + "return (count > 0);");
            sb.AppendLine(Helper.AddTabs(2) + "}");
            sb.AppendLine();
            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region Load By Id Method
            ////////////////////////////////////////////////////////////////////////////////
            // sample output
            // public async Task<Foo> LoadFooByIdAsync(int fooId)
            // {
            //     ArgumentOutOfRangeException.ThrowIfLessThan(fooId, 1);
            //     var parameters = new SqlParameter[] { ... };
            //     static async Task<Foo> processor(SqlDataReader reader) { ... }
            //     return await _Database.ExecuteQuerySpAsync("[dbo].[Foo_LoadById]", parameters, processor);
            // }

            sb.AppendLine(Helper.AddTabs(2) + $"public async Task<{className}> Load{className}ByIdAsync({pkType} {pkParamName})");
            sb.AppendLine(Helper.AddTabs(2) + "{");
            sb.AppendLine(Helper.AddTabs(3) + $"ArgumentOutOfRangeException.ThrowIfLessThan({pkParamName}, 1);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(3) + "var parameters = new SqlParameter[]");
            sb.AppendLine(Helper.AddTabs(3) + "{");
            sb.AppendLine(Helper.AddTabs(4) + $"new() {{ ParameterName = \"@{pkPropName}\", SqlDbType = SqlDbType.{NameFormatter.SQLTypeToCSharpType(pkColumn)}, Value = {pkParamName} }},");
            sb.AppendLine(Helper.AddTabs(3) + "};");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(3) + $"static async Task<{className}> processor(SqlDataReader reader)");
            sb.AppendLine(Helper.AddTabs(3) + "{");
            sb.AppendLine(Helper.AddTabs(4) + $"var results = await {className}Reader(reader);");
            sb.AppendLine(Helper.AddTabs(4) + "return results.FirstOrDefault();");
            sb.AppendLine(Helper.AddTabs(3) + "}");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(3) + $"return await _Database.ExecuteQuerySpAsync(\"[dbo].[{className}_LoadById]\", parameters, processor);");
            sb.AppendLine(Helper.AddTabs(2) + "}");
            sb.AppendLine();
            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region Delete Method
            ////////////////////////////////////////////////////////////////////////////////
            // sample output
            // public async Task<bool> DeleteFooAsync(int fooId)
            // {
            //     ArgumentOutOfRangeException.ThrowIfLessThan(fooId, 1);
            //     ...
            //     var count = await _Database.ExecuteNonQuerySpAsync("[dbo].[Foo_Delete]", parameters);
            //     return (count > 0);
            // }

            sb.AppendLine(Helper.AddTabs(2) + $"public async Task<bool> Delete{className}Async({pkType} {pkParamName})");
            sb.AppendLine(Helper.AddTabs(2) + "{");
            sb.AppendLine(Helper.AddTabs(3) + $"ArgumentOutOfRangeException.ThrowIfLessThan({pkParamName}, 1);");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(3) + "var parameters = new SqlParameter[]");
            sb.AppendLine(Helper.AddTabs(3) + "{");
            sb.AppendLine(Helper.AddTabs(4) + $"new() {{ ParameterName = \"@{pkPropName}\", SqlDbType = SqlDbType.{NameFormatter.SQLTypeToCSharpType(pkColumn)}, Value = {pkParamName} }},");
            sb.AppendLine(Helper.AddTabs(3) + "};");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(3) + $"var count = await _Database.ExecuteNonQuerySpAsync(\"[dbo].[{className}_Delete]\", parameters);");
            sb.AppendLine(Helper.AddTabs(3) + "return (count > 0);");
            sb.AppendLine(Helper.AddTabs(2) + "}");
            sb.AppendLine();
            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region Reader Method
            ////////////////////////////////////////////////////////////////////////////////
            // sample output
            // public static async Task<List<Foo>> FooReader(SqlDataReader reader)
            // {
            //     var output = new List<Foo>();
            //     while (await reader.ReadAsync())
            //     {
            //         var item = new Foo { ... };
            //         output.Add(item);
            //     }
            //     return output;
            // }

            sb.AppendLine(Helper.AddTabs(2) + $"// Readers");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(2) + $"public static async Task<List<{className}>> {className}Reader(SqlDataReader reader)");
            sb.AppendLine(Helper.AddTabs(2) + "{");
            sb.AppendLine(Helper.AddTabs(3) + $"var output = new List<{className}>();");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(3) + "while (await reader.ReadAsync())");
            sb.AppendLine(Helper.AddTabs(3) + "{");
            sb.AppendLine(Helper.AddTabs(4) + $"var item = new {className}");
            sb.AppendLine(Helper.AddTabs(4) + "{");

            foreach (var col in sqlTable.Columns.Values)
            {
                string propName = NameFormatter.ToCSharpPropertyName(col.Name);
                string csharpType = NameFormatter.SQLTypeToCSharpType(col);
                string colName = col.Name;

                string assignment;

                if (col.IsNullable)
                    assignment = $"(reader[\"{colName}\"] == DBNull.Value) ? null : ({csharpType})reader[\"{colName}\"]";
                else
                    assignment = $"({csharpType})reader[\"{colName}\"]";

                sb.AppendLine(Helper.AddTabs(5) + $"{propName} = {assignment},");
            }

            sb.AppendLine(Helper.AddTabs(4) + "};");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(4) + "output.Add(item);");
            sb.AppendLine(Helper.AddTabs(3) + "}");
            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(3) + "return output;");
            sb.AppendLine(Helper.AddTabs(2) + "}");
            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            sb.AppendLine(Helper.AddTabs(1) + "}");
            sb.Append('}');

            return new OutputObject
            {
                FileName = $"SqlDataLoader.{className}.cs",
                Body = sb.ToString(),
                OutputPath = $"{OutputPath}\\loaders",
            };
        }
    }
}

