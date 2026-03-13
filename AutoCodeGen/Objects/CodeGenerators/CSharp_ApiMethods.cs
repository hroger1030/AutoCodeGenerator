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
using System.Data;
using System.Linq;
using System.Text;

namespace AutoCodeGen
{
    public class CSharp_ApiMethods : IGenerator
    {
        private const string FEATURE_POCO_CLASS = "POCO class";
        private const string FEATURE_API_ENDPOINT = "API CRUD Endpoint";
        private const string FEATURE_ORM_LOADER = "ORM Loader object";

        private static readonly HashSet<char> _UndesirableChars =
        [
            '!', '$', '%', '^', '*', '(', ')', '-', '+', '\\', '=',
            '{', '}', '[', ']', ':', ';', '|', '\'', '<', '>', ',',
            '.', '?', '/', '~', '`', '@', '#', '"', ' ', '\t', '&'
        ];

        // option names
        private const string NAMESPACE_INCLUDES = "Included namespaces";

        public string Language => "c#";
        public string Version => "8.0";
        public string Category => "middle tier";
        public string Name => "C#/.Net";
        public string Description => "Generates various API objects, ORM objects, POCOs, and other classes based on database tables.";
        public string[] FeatureNames => [FEATURE_POCO_CLASS, FEATURE_API_ENDPOINT, FEATURE_ORM_LOADER];

        public Dictionary<string, string> DefaultOptions => new()
        {
            { NAMESPACE_INCLUDES, string.Empty },
        };

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
            ArgumentNullException.ThrowIfNull(sqlTable);
            ArgumentNullException.ThrowIfNull(options);

            string className = ToClassName(sqlTable.Name);
            var sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine();

            if (!string.IsNullOrWhiteSpace(options[NAMESPACE_INCLUDES]))
            {
                var namespaces = Formatter.StringToList(options[NAMESPACE_INCLUDES]);

                foreach (var item in namespaces)
                    sb.AppendLine($"Using {item};");

                sb.AppendLine();
            }

            sb.AppendLine($"namespace {ToPropertyName(sqlTable.Database.Name)}.Orm");
            sb.AppendLine("{");

            sb.AppendLine(Formatter.AddTabs(1) + $"public class {className}");
            sb.AppendLine(Formatter.AddTabs(1) + "{");

            #region Properties Block
            ////////////////////////////////////////////////////////////////////////////////

            // Sample Output
            // public string SomeID { get; set; }

            foreach (var col in sqlTable.Columns.Values)
            {
                if (col.IsNullable)
                    sb.AppendLine(Formatter.AddTabs(2) + $"public {SQLTypeToCSharpType(col)} {ToPropertyName(col.Name)} {{ get; set; }} = new();");
                else
                    sb.AppendLine(Formatter.AddTabs(2) + $"public {SQLTypeToCSharpType(col)} {ToPropertyName(col.Name)} {{ get; set; }}");
            }

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region Default CTOR
            ////////////////////////////////////////////////////////////////////////////////
            // sample output
            //public Foo() { }

            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(2) + $"public {ToClassName(sqlTable.Name)}() {{ }}");

            ////////////////////////////////////////////////////////////////////////////////
            #endregion Default CTOR

            sb.AppendLine(Formatter.AddTabs(1) + "}");
            sb.Append('}');

            return new OutputObject
            {
                FileName = $"{className}.cs",
                Body = sb.ToString(),
                OutputPath = $"{Language}\\{Version}\\pocos",
            };
        }

        public OutputObject GenerateApiEndpoint(SqlTable sqlTable, Dictionary<string, string> options)
        {
            ArgumentNullException.ThrowIfNull(sqlTable);
            ArgumentNullException.ThrowIfNull(options);

            //TODO: FIX ALL THIS SHIT!

            string className = ToClassName(sqlTable.Name);
            string controllerName = className.ToLower();
            string uiType = $"{className}Ui";

            // Try to find a primary key column (first int/identity column, fallback to first column)
            var pkColumn = sqlTable.Columns.Values.FirstOrDefault(c => c.IsIdentity)
                           ?? sqlTable.Columns.Values.FirstOrDefault(c => c.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                           ?? sqlTable.Columns.Values.First();

            string pkName = ToPropertyName(pkColumn.Name);
            string pkType = SQLTypeToCSharpType(pkColumn);
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
                var namespaces = Formatter.StringToList(nsValue);

                foreach (var item in namespaces)
                    sb.AppendLine($"using {item};");

                sb.AppendLine();
            }

            sb.AppendLine($"namespace {ToPropertyName(sqlTable.Database.Name)}.Controllers");
            sb.AppendLine("{");
            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region Class Declaration
            ////////////////////////////////////////////////////////////////////////////////
            sb.AppendLine(Formatter.AddTabs(1) + "[ApiController]");
            sb.AppendLine(Formatter.AddTabs(1) + $"public class {className}Controller : ApiControllerBase<{className}Controller>");
            sb.AppendLine(Formatter.AddTabs(1) + "{");
            sb.AppendLine(Formatter.AddTabs(2) + $"private const string CONTROLLER_NAME = \"{controllerName}\";");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(2) + "private readonly ValidationManager _ValidationManager;");
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

            sb.AppendLine(Formatter.AddTabs(2) + $"public {className}Controller(IConfigProvider configProvider, IDataProvider dataProvider) : base(configProvider, dataProvider)");
            sb.AppendLine(Formatter.AddTabs(2) + "{");
            sb.AppendLine(Formatter.AddTabs(3) + "_ValidationManager = new ValidationManager(_ConfigProvider);");
            sb.AppendLine(Formatter.AddTabs(2) + "}");
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

            sb.AppendLine(Formatter.AddTabs(2) + $"[HttpPost($\"{{PATH_ROOT}}/{{CONTROLLER_NAME}}/create\")]");
            sb.AppendLine(Formatter.AddTabs(2) + $"public async Task<ActionResult<ResponseBase>> Create{className}(");
            sb.AppendLine(Formatter.AddTabs(3) + "[FromHeader(Name = \"Authorization\")] string accessToken,");

            for (int i = 0; i < writableColumns.Count; i++)
            {
                var col = writableColumns[i];
                string csharpType = SQLTypeToCSharpType(col);
                string propName = ToPropertyName(col.Name);
                string paramName = char.ToLower(propName[0]) + propName.Substring(1);
                string comma = (i < writableColumns.Count - 1) ? "," : ")";

                sb.AppendLine(Formatter.AddTabs(3) + $"[FromForm] {csharpType} {paramName}{comma}");
            }

            sb.AppendLine(Formatter.AddTabs(2) + "{");
            sb.AppendLine(Formatter.AddTabs(3) + "try");
            sb.AppendLine(Formatter.AddTabs(3) + "{");
            sb.AppendLine(Formatter.AddTabs(4) + "if (string.IsNullOrEmpty(accessToken))");
            sb.AppendLine(Formatter.AddTabs(5) + "return StatusCode(Http.Unauthorized, ResponseBase.AUTH_TOKEN_MISSING);");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "var user = await _AuthManager.AuthenticateAndAuthorizeUserTokenAsync(accessToken, ePermissions.User);");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "if (user == null)");
            sb.AppendLine(Formatter.AddTabs(5) + "return StatusCode(Http.Forbidden, ResponseBase.FAILED_AUTH);");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "WriteTokenHeaders(user.AccessToken);");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "// validate input");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + $"var new{className} = new {className}()");
            sb.AppendLine(Formatter.AddTabs(4) + "{");
            sb.AppendLine(Formatter.AddTabs(5) + $"{pkName} = 0,");
            sb.AppendLine(Formatter.AddTabs(5) + "AuthorId = user.Profile.Id,");

            foreach (var col in writableColumns)
            {
                string propName = ToPropertyName(col.Name);
                string paramName = char.ToLower(propName[0]) + propName.Substring(1);
                string csharpType = SQLTypeToCSharpType(col);

                // Apply SanitizeInput for string types
                if (csharpType == "string")
                    sb.AppendLine(Formatter.AddTabs(5) + $"{propName} = {paramName}.SanitizeInput(Defaults.SHORT_STRING_LENGTH),");
                else
                    sb.AppendLine(Formatter.AddTabs(5) + $"{propName} = {paramName},");
            }

            sb.AppendLine(Formatter.AddTabs(4) + "};");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + $"var validationResult = _ValidationManager.Is{className}Valid(new{className});");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "if (!validationResult.IsValid)");
            sb.AppendLine(Formatter.AddTabs(5) + $"return StatusCode(Http.UnprocessableEntity, new ResponseBase(false, \"Invalid {controllerName} properties\", validationResult.Errors));");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + $"var results = await _DataProvider.Create{className}ProfileAsync(new{className});");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "if (!results)");
            sb.AppendLine(Formatter.AddTabs(5) + $"return StatusCode(Http.InternalServerError, new ResponseBase(false, \"Failed to create {controllerName}\"));");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + $"return Ok(new ResponseBase(true, \"{className} created\", new DbObjectIdUi {{ ObjectId = new{className}.{pkName}, ObjectType = new EnumeratedValue(eObjectType.{className}) }}));");
            sb.AppendLine(Formatter.AddTabs(3) + "}");
            sb.AppendLine(Formatter.AddTabs(3) + "catch (Exception ex)");
            sb.AppendLine(Formatter.AddTabs(3) + "{");
            sb.AppendLine(Formatter.AddTabs(4) + "return HandleException(ex);");
            sb.AppendLine(Formatter.AddTabs(3) + "}");
            sb.AppendLine(Formatter.AddTabs(2) + "}");
            sb.AppendLine();
            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region Update Endpoint
            ////////////////////////////////////////////////////////////////////////////////
            // sample output
            // [HttpPut($"{PATH_ROOT}/{CONTROLLER_NAME}/update")]
            // public async Task<ActionResult<ResponseBase>> UpdateFoo(...) { ... }

            sb.AppendLine(Formatter.AddTabs(2) + $"[HttpPut($\"{{PATH_ROOT}}/{{CONTROLLER_NAME}}/update\")]");
            sb.AppendLine(Formatter.AddTabs(2) + $"public async Task<ActionResult<ResponseBase>> Update{className}(");
            sb.AppendLine(Formatter.AddTabs(3) + "[FromHeader(Name = \"Authorization\")] string accessToken,");
            sb.AppendLine(Formatter.AddTabs(3) + $"[FromForm] {pkType} {pkParamName},");

            for (int i = 0; i < writableColumns.Count; i++)
            {
                var col = writableColumns[i];

                string cSharpType = SQLTypeToCSharpType(col);
                string propName = ToPropertyName(col.Name);
                string paramName = char.ToLower(propName[0]) + propName.Substring(1);
                string comma = (i < writableColumns.Count - 1) ? "," : ")";

                sb.AppendLine(Formatter.AddTabs(3) + $"[FromForm] {cSharpType} {paramName}{comma}");
            }

            sb.AppendLine(Formatter.AddTabs(2) + "{");
            sb.AppendLine(Formatter.AddTabs(3) + "try");
            sb.AppendLine(Formatter.AddTabs(3) + "{");
            sb.AppendLine(Formatter.AddTabs(4) + "if (string.IsNullOrEmpty(accessToken))");
            sb.AppendLine(Formatter.AddTabs(5) + "return StatusCode(Http.Unauthorized, ResponseBase.AUTH_TOKEN_MISSING);");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "var user = await _AuthManager.AuthenticateAndAuthorizeUserTokenAsync(accessToken, ePermissions.Admin);");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "if (user == null)");
            sb.AppendLine(Formatter.AddTabs(5) + "return StatusCode(Http.Forbidden, ResponseBase.FAILED_AUTH);");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "WriteTokenHeaders(user.AccessToken);");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + $"if ({pkParamName} < 1)");
            sb.AppendLine(Formatter.AddTabs(5) + "return StatusCode(Http.UnprocessableEntity, ResponseBase.MISSING_OR_INVALID_ARGUMENTS);");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + $"var existing{className} = await _DataProvider.Load{className}ByIdAsync({pkParamName});");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + $"if (existing{className} == null)");
            sb.AppendLine(Formatter.AddTabs(5) + "return StatusCode(Http.UnprocessableEntity, ResponseBase.SERVER_ERROR);");
            sb.AppendLine();

            foreach (var col in writableColumns)
            {
                string propName = ToPropertyName(col.Name);
                string paramName = char.ToLower(propName[0]) + propName.Substring(1);
                string csharpType = SQLTypeToCSharpType(col);

                if (csharpType == "string")
                    sb.AppendLine(Formatter.AddTabs(4) + $"existing{className}.Profile.{propName} = {paramName}.SanitizeInput(Defaults.SHORT_STRING_LENGTH);");
                else
                    sb.AppendLine(Formatter.AddTabs(4) + $"existing{className}.Profile.{propName} = {paramName};");
            }

            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + $"var results = await _DataProvider.Update{className}ProfileAsync(existing{className}.Profile);");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "if (!results)");
            sb.AppendLine(Formatter.AddTabs(5) + $"return StatusCode(Http.InternalServerError, new ResponseBase(false, \"Failed to update {controllerName}\"));");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "return Ok(new ResponseBase(true, string.Empty, results));");
            sb.AppendLine(Formatter.AddTabs(3) + "}");
            sb.AppendLine(Formatter.AddTabs(3) + "catch (Exception ex)");
            sb.AppendLine(Formatter.AddTabs(3) + "{");
            sb.AppendLine(Formatter.AddTabs(4) + "return HandleException(ex);");
            sb.AppendLine(Formatter.AddTabs(3) + "}");
            sb.AppendLine(Formatter.AddTabs(2) + "}");
            sb.AppendLine();
            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region Get By Id Endpoint
            ////////////////////////////////////////////////////////////////////////////////
            // sample output
            // [HttpGet($"{PATH_ROOT}/{CONTROLLER_NAME}/load/{fooId}")]
            // public async Task<ActionResult<ResponseBase>> GetFoo(...) { ... }

            sb.AppendLine(Formatter.AddTabs(2) + $"[HttpGet($\"{{PATH_ROOT}}/{{CONTROLLER_NAME}}/load/{{{{{pkParamName}}}}}\")]");
            sb.AppendLine(Formatter.AddTabs(2) + $"public async Task<ActionResult<ResponseBase>> Get{className}(");
            sb.AppendLine(Formatter.AddTabs(3) + "[FromHeader(Name = \"Authorization\")] string accessToken,");
            sb.AppendLine(Formatter.AddTabs(3) + $"[FromRoute] {pkType} {pkParamName})");
            sb.AppendLine(Formatter.AddTabs(2) + "{");
            sb.AppendLine(Formatter.AddTabs(3) + "try");
            sb.AppendLine(Formatter.AddTabs(3) + "{");
            sb.AppendLine(Formatter.AddTabs(4) + "if (string.IsNullOrEmpty(accessToken))");
            sb.AppendLine(Formatter.AddTabs(5) + "return StatusCode(Http.Unauthorized, ResponseBase.AUTH_TOKEN_MISSING);");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "var user = await _AuthManager.AuthenticateAndAuthorizeUserTokenAsync(accessToken, ePermissions.User);");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "if (user == null)");
            sb.AppendLine(Formatter.AddTabs(5) + "return StatusCode(Http.Forbidden, ResponseBase.FAILED_AUTH);");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "WriteTokenHeaders(user.AccessToken);");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + $"if ({pkParamName} < 0)");
            sb.AppendLine(Formatter.AddTabs(5) + $"return StatusCode(Http.BadRequest, new ResponseBase(false, \"Invalid {pkParamName} value\"));");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + $"var results = await _DataProvider.Load{className}ByIdAsync({pkParamName});");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "if (results == null)");
            sb.AppendLine(Formatter.AddTabs(5) + $"return StatusCode(Http.InternalServerError, new ResponseBase(false, \"Failed to load {controllerName}\"));");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "return Ok(new ResponseBase(true, string.Empty, results));");
            sb.AppendLine(Formatter.AddTabs(3) + "}");
            sb.AppendLine(Formatter.AddTabs(3) + "catch (Exception ex)");
            sb.AppendLine(Formatter.AddTabs(3) + "{");
            sb.AppendLine(Formatter.AddTabs(4) + "return HandleException(ex);");
            sb.AppendLine(Formatter.AddTabs(3) + "}");
            sb.AppendLine(Formatter.AddTabs(2) + "}");
            sb.AppendLine();
            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region Load List Endpoint
            ////////////////////////////////////////////////////////////////////////////////
            // #region sample output
            // [HttpGet($"{PATH_ROOT}/{CONTROLLER_NAME}/load-{controllerName}s/skip/{skip}/take/{take}")]
            // public async Task<ActionResult<ResponseBase>> LoadUser{className}List(...) { ... }

            sb.AppendLine(Formatter.AddTabs(2) + $"[HttpGet($\"{{PATH_ROOT}}/{{CONTROLLER_NAME}}/load-{controllerName}s/skip/{{{{skip}}}}/take/{{{{take}}}}\")]");
            sb.AppendLine(Formatter.AddTabs(2) + $"public async Task<ActionResult<ResponseBase>> LoadUser{className}List(");
            sb.AppendLine(Formatter.AddTabs(3) + "[FromHeader(Name = \"Authorization\")] string accessToken,");
            sb.AppendLine(Formatter.AddTabs(3) + "[FromRoute] int skip = Defaults.DEFAULT_SKIP,");
            sb.AppendLine(Formatter.AddTabs(3) + "[FromRoute] int take = Defaults.DEFAULT_TAKE)");
            sb.AppendLine(Formatter.AddTabs(2) + "{");
            sb.AppendLine(Formatter.AddTabs(3) + "try");
            sb.AppendLine(Formatter.AddTabs(3) + "{");
            sb.AppendLine(Formatter.AddTabs(4) + "if (string.IsNullOrEmpty(accessToken))");
            sb.AppendLine(Formatter.AddTabs(5) + "return StatusCode(Http.Unauthorized, ResponseBase.AUTH_TOKEN_MISSING);");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "var user = await _AuthManager.AuthenticateAndAuthorizeUserTokenAsync(accessToken, ePermissions.User);");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "if (user == null)");
            sb.AppendLine(Formatter.AddTabs(5) + "return StatusCode(Http.Forbidden, ResponseBase.FAILED_AUTH);");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "WriteTokenHeaders(user.AccessToken);");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "if (skip < 0)");
            sb.AppendLine(Formatter.AddTabs(5) + "return StatusCode(Http.UnprocessableEntity, new ResponseBase(false, \"skip cannot be less than 0\"));");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "if (take < 1)");
            sb.AppendLine(Formatter.AddTabs(5) + "return StatusCode(Http.UnprocessableEntity, new ResponseBase(false, \"take cannot be less than 1\"));");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + $"var results = await _DataProvider.Load{className}ListByUserIdAsync(user.Profile.Id, skip, take);");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "if (results == null)");
            sb.AppendLine(Formatter.AddTabs(5) + $"return StatusCode(Http.InternalServerError, new ResponseBase(false, \"Failed to load {controllerName} list\"));");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + $"var output = new PagedList<{uiType}>()");
            sb.AppendLine(Formatter.AddTabs(4) + "{");
            sb.AppendLine(Formatter.AddTabs(5) + "Skip = skip,");
            sb.AppendLine(Formatter.AddTabs(5) + "Take = take,");
            sb.AppendLine(Formatter.AddTabs(5) + "TotalCount = results.TotalCount,");
            sb.AppendLine(Formatter.AddTabs(5) + $"PageData = results.PageData.Select(x => _UiConverter.Convert{className}ToUi(x)).ToList(),");
            sb.AppendLine(Formatter.AddTabs(4) + "};");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "return Ok(new ResponseBase(true, string.Empty, output));");
            sb.AppendLine(Formatter.AddTabs(3) + "}");
            sb.AppendLine(Formatter.AddTabs(3) + "catch (Exception ex)");
            sb.AppendLine(Formatter.AddTabs(3) + "{");
            sb.AppendLine(Formatter.AddTabs(4) + "return HandleException(ex);");
            sb.AppendLine(Formatter.AddTabs(3) + "}");
            sb.AppendLine(Formatter.AddTabs(2) + "}");
            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region Delete Endpoint
            ////////////////////////////////////////////////////////////////////////////////
            // sample output
            // [HttpDelete($"{PATH_ROOT}/{CONTROLLER_NAME}/delete/{fooId}")]
            // public async Task<ActionResult<ResponseBase>> DeleteFoo(...) { ... }

            sb.AppendLine(Formatter.AddTabs(2) + $"[HttpDelete($\"{{PATH_ROOT}}/{{CONTROLLER_NAME}}/delete/{{{{{pkParamName}}}}}\")]");
            sb.AppendLine(Formatter.AddTabs(2) + $"public async Task<ActionResult<ResponseBase>> Delete{className}(");
            sb.AppendLine(Formatter.AddTabs(3) + "[FromHeader(Name = \"Authorization\")] string accessToken,");
            sb.AppendLine(Formatter.AddTabs(3) + $"[FromRoute] {pkType} {pkParamName})");
            sb.AppendLine(Formatter.AddTabs(2) + "{");
            sb.AppendLine(Formatter.AddTabs(3) + "try");
            sb.AppendLine(Formatter.AddTabs(3) + "{");
            sb.AppendLine(Formatter.AddTabs(4) + "if (string.IsNullOrEmpty(accessToken))");
            sb.AppendLine(Formatter.AddTabs(5) + "return StatusCode(Http.Unauthorized, ResponseBase.AUTH_TOKEN_MISSING);");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "var user = await _AuthManager.AuthenticateAndAuthorizeUserTokenAsync(accessToken, ePermissions.Admin);");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "if (user == null)");
            sb.AppendLine(Formatter.AddTabs(5) + "return StatusCode(Http.Forbidden, ResponseBase.FAILED_AUTH);");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "WriteTokenHeaders(user.AccessToken);");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + $"if ({pkParamName} < 1)");
            sb.AppendLine(Formatter.AddTabs(5) + "return StatusCode(Http.UnprocessableEntity, ResponseBase.MISSING_OR_INVALID_ARGUMENTS);");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + $"var existing{className} = await _DataProvider.Load{className}ByIdAsync({pkParamName});");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + $"if (existing{className} == null)");
            sb.AppendLine(Formatter.AddTabs(5) + "return StatusCode(Http.UnprocessableEntity, ResponseBase.SERVER_ERROR);");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + $"var results = await _DataProvider.Delete{className}ByIdAsync({pkParamName});");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "if (!results)");
            sb.AppendLine(Formatter.AddTabs(5) + $"return StatusCode(Http.InternalServerError, new ResponseBase(false, \"Failed to delete {controllerName}\"));");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "return Ok(new ResponseBase(true, string.Empty, results));");
            sb.AppendLine(Formatter.AddTabs(3) + "}");
            sb.AppendLine(Formatter.AddTabs(3) + "catch (Exception ex)");
            sb.AppendLine(Formatter.AddTabs(3) + "{");
            sb.AppendLine(Formatter.AddTabs(4) + "return HandleException(ex);");
            sb.AppendLine(Formatter.AddTabs(3) + "}");
            sb.AppendLine(Formatter.AddTabs(2) + "}");
            sb.AppendLine();
            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            sb.AppendLine(Formatter.AddTabs(1) + "}");
            sb.Append('}');

            return new OutputObject
            {
                FileName = $"{className}Controller.cs",
                Body = sb.ToString(),
                OutputPath = $"{Language}\\{Version}\\controllers",
            };
        }

        public OutputObject GenerateSqlDataLoader(SqlTable sqlTable, Dictionary<string, string> options)
        {
            ArgumentNullException.ThrowIfNull(sqlTable);
            ArgumentNullException.ThrowIfNull(options);

            string className = ToClassName(sqlTable.Name);                        // e.g. "Foo"
            string namespaceName = ToPropertyName(sqlTable.Database.Name);        // e.g. "MyDatabase"
            string instanceName = ToPropertyName(sqlTable.Name);                  // e.g. "foo"

            var sb = new StringBuilder();

            #region Namespace + Class Header
            ////////////////////////////////////////////////////////////////////////////////
            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");
            sb.AppendLine(Formatter.AddTabs(1) + "public partial class SqlDataLoader");
            sb.AppendLine(Formatter.AddTabs(1) + "{");
            sb.AppendLine(Formatter.AddTabs(2) + $"// {className} object");
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

            sb.AppendLine(Formatter.AddTabs(2) + $"public async Task<bool> Create{className}Async({className} {instanceName})");
            sb.AppendLine(Formatter.AddTabs(2) + "{");
            sb.AppendLine(Formatter.AddTabs(3) + $"ArgumentNullException.ThrowIfNull({char.ToLower(className[0]) + className.Substring(1)});");

            // Guard for PK if it's not the identity (edge case)
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(3) + "var parameters = new SqlParameter[]");
            sb.AppendLine(Formatter.AddTabs(3) + "{");

            foreach (var col in sqlTable.Columns.Values)
            {
                if (col.IsNullable)
                    sb.AppendLine(Formatter.AddTabs(4) + $"new() {{ ParameterName = \"@{col.Name}\", SqlDbType = SqlDbType.{col.DataType}, Size = {col.Length}, Value = (object){className}.{ToPropertyName(col.Name)} ?? DBNull.Value }},");
                else
                    sb.AppendLine(Formatter.AddTabs(4) + $"new() {{ ParameterName = \"@{col.Name}\", SqlDbType = SqlDbType.{col.DataType}, Size = {col.Length}, Value = {className}.{ToPropertyName(col.Name)} }},");
            }

            sb.AppendLine(Formatter.AddTabs(3) + "};");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(3) + $"var count = await _Database.ExecuteNonQuerySpAsync(\"[dbo].[{className}_InsertSingle]\", parameters);");
            sb.AppendLine(Formatter.AddTabs(3) + "return (count > 0);");
            sb.AppendLine(Formatter.AddTabs(2) + "}");
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

            sb.AppendLine(Formatter.AddTabs(2) + $"public async Task<bool> Update{className}Async({className} {instanceName})");
            sb.AppendLine(Formatter.AddTabs(2) + "{");
            sb.AppendLine(Formatter.AddTabs(3) + $"ArgumentNullException.ThrowIfNull({instanceName});");

            foreach (var col in sqlTable.PkList)
            {
                if (col.BaseType == eSqlBaseType.Integer)
                    sb.AppendLine(Formatter.AddTabs(3) + $"ArgumentOutOfRangeException.ThrowIfLessThan({instanceName}.{col.Name}, 1);");
                else if (col.BaseType == eSqlBaseType.String)
                    sb.AppendLine(Formatter.AddTabs(3) + $"ArgumentException.ThrowIfNullOrEmpty({instanceName}.{col.Name}));");
                else
                    sb.AppendLine(Formatter.AddTabs(3) + $"// Unknown condition check for {instanceName}.{col.Name}");
            }

            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(3) + "var parameters = new SqlParameter[]");
            sb.AppendLine(Formatter.AddTabs(3) + "{");

            foreach (var col in sqlTable.Columns.Values)
            {
                if (col.IsNullable)
                    sb.AppendLine(Formatter.AddTabs(4) + $"new() {{ ParameterName = \"@{col.Name}\", SqlDbType = SqlDbType.{col.DataType}, Size = {col.Length} Value = (object){className}.{ToPropertyName(col.Name)} ?? DBNull.Value }},");
                else
                    sb.AppendLine(Formatter.AddTabs(4) + $"new() {{ ParameterName = \"@{col.Name}\", SqlDbType = SqlDbType.{col.DataType}, Size = {col.Length} Value = {className}.{ToPropertyName(col.Name)} }},");
            }

            sb.AppendLine(Formatter.AddTabs(3) + "};");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(3) + $"var count = await _Database.ExecuteNonQuerySpAsync(\"[dbo].[{className}_Update]\", parameters);");
            sb.AppendLine(Formatter.AddTabs(3) + "return (count > 0);");
            sb.AppendLine(Formatter.AddTabs(2) + "}");
            sb.AppendLine();
            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region Load By Pk Method
            ////////////////////////////////////////////////////////////////////////////////
            // sample output
            // public async Task<Foo> LoadFooByIdAsync(int fooId)
            // {
            //     ArgumentOutOfRangeException.ThrowIfLessThan(fooId, 1);
            //     var parameters = new SqlParameter[] { ... };
            //     static async Task<Foo> processor(SqlDataReader reader) { ... }
            //     return await _Database.ExecuteQuerySpAsync("[dbo].[Foo_LoadById]", parameters, processor);
            // }

            sb.AppendLine(Formatter.AddTabs(2) + $"public async Task<{className}> Load{className}ByPkAsync({GenerateFunctionArgs(sqlTable, eIncludedFields.PKOnly, _UndesirableChars)})");
            sb.AppendLine(Formatter.AddTabs(2) + "{");

            foreach (var col in sqlTable.PkList)
            {
                if (col.BaseType == eSqlBaseType.Integer)
                    sb.AppendLine(Formatter.AddTabs(3) + $"ArgumentOutOfRangeException.ThrowIfLessThan({instanceName}.{col.Name}, 1);");
                else if (col.BaseType == eSqlBaseType.String)
                    sb.AppendLine(Formatter.AddTabs(3) + $"ArgumentException.ThrowIfNullOrEmpty({instanceName}.{col.Name}));");
                else
                    sb.AppendLine(Formatter.AddTabs(3) + $"// Unknown condition check for {instanceName}.{col.Name}");
            }

            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(3) + "var parameters = new SqlParameter[]");
            sb.AppendLine(Formatter.AddTabs(3) + "{");

            foreach (var col in sqlTable.PkList)
            {
                sb.AppendLine(Formatter.AddTabs(4) + $"new() {{ ParameterName = \"@{col.Name}\", SqlDbType = SqlDbType.{col.SqlDataType}, Value = {ToPropertyName(col.Name)} }},");
            }

            sb.AppendLine(Formatter.AddTabs(3) + "};");
            sb.AppendLine();

            sb.AppendLine(Formatter.AddTabs(3) + $"static async Task<{className}> processor(SqlDataReader reader)");
            sb.AppendLine(Formatter.AddTabs(3) + "{");
            sb.AppendLine(Formatter.AddTabs(4) + $"var results = await {className}Reader(reader);");
            sb.AppendLine(Formatter.AddTabs(4) + "return results.FirstOrDefault();");
            sb.AppendLine(Formatter.AddTabs(3) + "}");
            sb.AppendLine();

            sb.AppendLine(Formatter.AddTabs(3) + $"return await _Database.ExecuteQuerySpAsync(\"[dbo].[{className}_LoadByPk]\", parameters, processor);");
            sb.AppendLine(Formatter.AddTabs(2) + "}");
            sb.AppendLine();
            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region Delete by Pk Method
            ////////////////////////////////////////////////////////////////////////////////
            // sample output
            // public async Task<bool> DeleteFooAsync(int fooId)
            // {
            //     ArgumentOutOfRangeException.ThrowIfLessThan(fooId, 1);
            //
            //     var parameters = new SqlParameter[]
            //     {
            //         new() { ParameterName = "@ObjectTypeId", SqlDbType = SqlDbType.Int, Value = ObjectTypeId },
            //         new() { ParameterName = "@ObjectId", SqlDbType = SqlDbType.Int, Value = ObjectId },
            //     };
            //
            //     var count = await _Database.ExecuteNonQuerySpAsync("[dbo].[Foo_Delete]", parameters);
            //     return (count > 0);
            // }

            sb.AppendLine(Formatter.AddTabs(2) + $"public async Task<bool> Delete{className}Async({GenerateFunctionArgs(sqlTable, eIncludedFields.PKOnly, _UndesirableChars)})");
            sb.AppendLine(Formatter.AddTabs(2) + "{");

            foreach (var col in sqlTable.PkList)
            {
                if (col.BaseType == eSqlBaseType.Integer)
                    sb.AppendLine(Formatter.AddTabs(3) + $"ArgumentOutOfRangeException.ThrowIfLessThan({instanceName}.{col.Name}, 1);");
                else if (col.BaseType == eSqlBaseType.String)
                    sb.AppendLine(Formatter.AddTabs(3) + $"ArgumentException.ThrowIfNullOrEmpty({instanceName}.{col.Name}));");
                else
                    sb.AppendLine(Formatter.AddTabs(3) + $"// Unknown condition check for {instanceName}.{col.Name}");
            }

            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(3) + "var parameters = new SqlParameter[]");
            sb.AppendLine(Formatter.AddTabs(3) + "{");

            foreach (var col in sqlTable.PkList)
            {
                sb.AppendLine(Formatter.AddTabs(4) + $"new() {{ ParameterName = \"@{col.Name}\", SqlDbType = SqlDbType.{col.SqlDataType}, Value = {ToPropertyName(col.Name)} }},");
            }

            sb.AppendLine(Formatter.AddTabs(3) + "};");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(3) + $"var count = await _Database.ExecuteNonQuerySpAsync(\"[dbo].[{className}_Delete]\", parameters);");
            sb.AppendLine(Formatter.AddTabs(3) + "return (count > 0);");
            sb.AppendLine(Formatter.AddTabs(2) + "}");
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

            sb.AppendLine(Formatter.AddTabs(2) + $"// Readers");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(2) + $"public static async Task<List<{className}>> {className}Reader(SqlDataReader reader)");
            sb.AppendLine(Formatter.AddTabs(2) + "{");
            sb.AppendLine(Formatter.AddTabs(3) + $"var output = new List<{className}>();");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(3) + "while (await reader.ReadAsync())");
            sb.AppendLine(Formatter.AddTabs(3) + "{");
            sb.AppendLine(Formatter.AddTabs(4) + $"var item = new {className}");
            sb.AppendLine(Formatter.AddTabs(4) + "{");

            foreach (var col in sqlTable.Columns.Values)
            {
                string propName = ToPropertyName(col.Name);
                string csharpType = SQLTypeToCSharpType(col);
                string colName = col.Name;

                string assignment;

                if (col.IsNullable)
                    assignment = $"(reader[\"{colName}\"] == DBNull.Value) ? null : ({csharpType})reader[\"{colName}\"]";
                else
                    assignment = $"({csharpType})reader[\"{colName}\"]";

                sb.AppendLine(Formatter.AddTabs(5) + $"{propName} = {assignment},");
            }

            sb.AppendLine(Formatter.AddTabs(4) + "};");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(4) + "output.Add(item);");
            sb.AppendLine(Formatter.AddTabs(3) + "}");
            sb.AppendLine();
            sb.AppendLine(Formatter.AddTabs(3) + "return output;");
            sb.AppendLine(Formatter.AddTabs(2) + "}");
            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            sb.AppendLine(Formatter.AddTabs(1) + "}");
            sb.Append('}');

            return new OutputObject
            {
                FileName = $"SqlDataLoader.{className}.cs",
                Body = sb.ToString(),
                OutputPath = $"{Language}\\{Version}\\loaders",
            };
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // Helper Methods 

        /// <summary>
        /// Returns the SQL column name formatted for a C# class local member.
        /// 
        /// Sample: FooBar -> foo_bar
        /// </summary>
        private static string ToLocalVariable(string input)
        {
            return Formatter.ToCamelCase(input, _UndesirableChars);
        }

        /// <summary>
        /// Returns the SQL column name formatted as a C# class name.
        /// 
        /// Sample: FooBar -> cFooBar
        /// </summary>
        private static string ToClassName(string input)
        {
            return Formatter.ToPascalCase(input, _UndesirableChars);
        }

        /// <summary>
        /// Returns the SQL column name formatted as a C# interface name.
        /// 
        /// Sample: FooBar -> IFooBar
        /// </summary>
        private static string ToInterfaceName(string input)
        {
            var buffer = Formatter.ToPascalCase(input, _UndesirableChars);
            return $"I{buffer}";
        }

        /// <summary>
        /// Returns the SQL column name formatted as a C# enum name.
        /// 
        /// Sample: FooBar -> eFooBar
        /// </summary>
        private static string ToEnumName(string input)
        {
            var buffer = Formatter.ToPascalCase(input, _UndesirableChars);
            return $"e{buffer}";
        }

        /// <summary>
        /// Returns the SQL column name formatted as a C# property name.
        /// 
        /// Sample: foo_bar -> FooBar
        /// </summary>
        private static string ToPropertyName(string input)
        {
            return Formatter.ToPascalCase(input, _UndesirableChars);
        }

        /// <summary>
        /// Generates a complete parameter string. 
        /// 
        /// Sample: new SqlParameter() { ParameterName = "AccountId", SqlDbType = SqlDbType.Int, Value = obj.AccountId },
        /// </summary>
        private static string ToSQLParameterString(string className, SqlColumn column)
        {
            ArgumentException.ThrowIfNullOrEmpty(className);
            ArgumentNullException.ThrowIfNull(column);

            var propertyName = ToPropertyName(column.Name);
            var columnValue = (column.IsNullable) ? $"(object){className}.{propertyName} ?? DBNull.Value" : $"{className}.{propertyName}";

            // only set length on columns that can actually vary
            switch (column.SqlDataType)
            {
                case SqlDbType.Text:
                case SqlDbType.NText:
                case SqlDbType.VarChar:
                case SqlDbType.NVarChar:
                case SqlDbType.VarBinary:
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.Xml:
                    return Formatter.AddTabs(4) + $"new() {{ ParameterName = \"@{column.Name}\", SqlDbType = SqlDbType.{column.DataType}, Size = {column.Length}, Value = {columnValue}";

                default:
                    return Formatter.AddTabs(4) + $"new() {{ ParameterName = \"@{column.Name}\", SqlDbType = SqlDbType.{column.DataType}, Value = {columnValue} }},";
            }
        }

        /// <summary>
        /// Returns a valid C# default value for the given SQL datatype.
        /// </summary>
        private static string GetDefaultValue(SqlColumn sqlColumn)
        {
            // do we have a non default value in the DB definition?
            if (sqlColumn.DefaultValue != string.Empty)
            {
                switch (sqlColumn.SqlDataType)
                {
                    case SqlDbType.BigInt: return sqlColumn.DefaultValue;
                    //case SqlDbType.Binary: return "null";
                    case SqlDbType.Bit:
                        if (sqlColumn.DefaultValue == "1")
                            return "true";
                        else
                            return "false";

                    case SqlDbType.Char: return "\"" + sqlColumn.DefaultValue + "\"";
                    case SqlDbType.Date: return sqlColumn.DefaultValue.ToLower() == "getdate()" ? "DateTime.Now;" : $"DateTime.Parse(\"{sqlColumn.DefaultValue}\")";
                    case SqlDbType.DateTime: return sqlColumn.DefaultValue.ToLower() == "getdate()" ? "DateTime.Now;" : $"DateTime.Parse(\"{sqlColumn.DefaultValue}\")";
                    case SqlDbType.DateTime2: return sqlColumn.DefaultValue.ToLower() == "getdate()" ? "DateTime.Now;" : $"DateTime.Parse(\"{sqlColumn.DefaultValue}\")";
                    case SqlDbType.DateTimeOffset: return sqlColumn.DefaultValue.ToLower() == "getdate()" ? "DateTime.Now;" : $"DateTime.Parse(\"{sqlColumn.DefaultValue}\")";
                    case SqlDbType.Decimal: return sqlColumn.DefaultValue;
                    case SqlDbType.Float: return sqlColumn.DefaultValue;
                    //case SqlDbType.Image:               return "null";
                    case SqlDbType.Int: return sqlColumn.DefaultValue;
                    case SqlDbType.Money: return sqlColumn.DefaultValue + "m";
                    case SqlDbType.NChar: return "\"" + sqlColumn.DefaultValue + "\"";
                    case SqlDbType.NText: return "\"" + sqlColumn.DefaultValue + "\"";
                    case SqlDbType.NVarChar: return "\"" + sqlColumn.DefaultValue + "\"";
                    case SqlDbType.Real: return sqlColumn.DefaultValue + "f";
                    case SqlDbType.SmallDateTime: return "\"" + DateTime.MinValue.ToString() + "\"";
                    case SqlDbType.SmallInt: return sqlColumn.DefaultValue;
                    case SqlDbType.SmallMoney: return sqlColumn.DefaultValue + "f";
                    //case SqlDbType.Structured:          return "null";         
                    case SqlDbType.Text: return "string.Empty";
                    case SqlDbType.Time: return "DateTime.Now";
                    //case SqlDbType.Timestamp:           return "string.Empty";
                    case SqlDbType.TinyInt: return sqlColumn.DefaultValue;
                    //case SqlDbType.Udt:                 return "null";
                    case SqlDbType.UniqueIdentifier: return "Guid.Empty";
                    //case SqlDbType.VarBinary:           return "null";
                    case SqlDbType.VarChar: return "\"" + sqlColumn.DefaultValue + "\"";
                    //case SqlDbType.Variant:             return "null";
                    case SqlDbType.Xml: return "\"" + sqlColumn.DefaultValue + "\"";

                    default:
                        return "// NO DEFAULT AVAILABLE FOR " + sqlColumn.SqlDataType.ToString();
                }

            }

            if (sqlColumn.IsNullable)
            {
                switch (sqlColumn.SqlDataType)
                {
                    case SqlDbType.Variant:
                        return "// NO DEFAULT AVAILABLE FOR " + sqlColumn.SqlDataType.ToString();

                    default:
                        return "null";
                }
            }
            else
            {
                switch (sqlColumn.SqlDataType)
                {
                    case SqlDbType.BigInt: return "0";
                    case SqlDbType.Binary: return "null";
                    case SqlDbType.Bit: return "false";
                    case SqlDbType.Char: return "string.Empty";
                    case SqlDbType.Date: return "DateTime.Now";
                    case SqlDbType.DateTime: return "DateTime.Now";
                    case SqlDbType.DateTime2: return "DateTime.Now";
                    case SqlDbType.DateTimeOffset: return "DateTime.Now";
                    case SqlDbType.Decimal: return "0.0m";
                    case SqlDbType.Float: return "0.0d";
                    case SqlDbType.Image: return "null";
                    case SqlDbType.Int: return "0";
                    case SqlDbType.Money: return "0.0m";
                    case SqlDbType.NChar: return "string.Empty";
                    case SqlDbType.NText: return "string.Empty";
                    case SqlDbType.NVarChar: return "string.Empty";
                    case SqlDbType.Real: return "0.0f";
                    case SqlDbType.SmallDateTime: return "DateTime.Now";
                    case SqlDbType.SmallInt: return "0";
                    case SqlDbType.SmallMoney: return "0.0f";
                    case SqlDbType.Structured: return "null";
                    case SqlDbType.Text: return "string.Empty";
                    case SqlDbType.Time: return "DateTime.Now";
                    case SqlDbType.Timestamp: return "string.Empty";
                    case SqlDbType.TinyInt: return "byte.MinValue";
                    case SqlDbType.Udt: return "null";
                    case SqlDbType.UniqueIdentifier: return "Guid.Empty";
                    case SqlDbType.VarBinary: return "null";
                    case SqlDbType.VarChar: return "string.Empty";
                    case SqlDbType.Variant: return "null";
                    case SqlDbType.Xml: return "string.Empty";

                    default:
                        return "// NO DEFAULT AVAILABLE FOR " + sqlColumn.SqlDataType.ToString();
                }
            }
        }

        /// <summary>
        /// Returns the proper cast to C# type for the given SQL datatype.
        /// 
        /// Example: int -> Convert.ToInt32
        /// </summary>
        private static string GetCastString(SqlColumn sqlColumn)
        {
            // todo: do we need to deal with a nullable type here?
            // need to check use of Binary, VarBinary, Image to make sure cast is correct

            switch (sqlColumn.SqlDataType)
            {
                case SqlDbType.BigInt: return "Convert.ToInt64";
                case SqlDbType.Binary: return "(byte[])";
                case SqlDbType.Bit: return "Convert.ToBoolean";
                case SqlDbType.Char: return "Convert.ToChar";
                case SqlDbType.Date: return "Convert.ToDateTime";
                case SqlDbType.DateTime: return "Convert.ToDateTime";
                case SqlDbType.DateTime2: return "Convert.ToDateTime";
                case SqlDbType.DateTimeOffset: return "DateTimeOffset.Parse";
                case SqlDbType.Decimal: return "Convert.ToDecimal";
                case SqlDbType.Float: return "Convert.ToDouble";
                case SqlDbType.Image: return "(byte[])";
                case SqlDbType.Int: return "Convert.ToInt32";
                case SqlDbType.Money: return "Convert.ToDecimal";
                case SqlDbType.NChar: return "Convert.ToString";
                case SqlDbType.NText: return "Convert.ToString";
                case SqlDbType.NVarChar: return "Convert.ToString";
                case SqlDbType.Real: return "Convert.ToSingle";
                case SqlDbType.SmallDateTime: return "Convert.ToDateTime";
                case SqlDbType.SmallInt: return "Convert.ToInt16";
                case SqlDbType.SmallMoney: return "Convert.ToDecimal";
                //case SqlDbType.Structured: return "null";         
                case SqlDbType.Text: return "Convert.ToString";
                case SqlDbType.Time: return "TimeSpan.Parse";
                //case SqlDbType.Timestamp: return "string.Empty";
                case SqlDbType.TinyInt: return "Convert.ToByte";
                //case SqlDbType.Udt: return "null";
                case SqlDbType.UniqueIdentifier: return "Convert.ToString";
                case SqlDbType.VarBinary: return "(byte[])";
                case SqlDbType.VarChar: return "Convert.ToString";
                //case SqlDbType.Variant: return "null";
                case SqlDbType.Xml: return "Convert.ToString";

                default:
                    return "// NO CONVERSION AVAILABLE FOR " + sqlColumn.SqlDataType.ToString();
            }
            //}
        }

        /// <summary>
        /// Returns the minimum C# value for the given SQL datatype.
        /// </summary>
        private static string GetMinValue(SqlColumn sqlColumn)
        {
            switch (sqlColumn.SqlDataType)
            {
                case SqlDbType.BigInt: return "long.MinValue";
                case SqlDbType.Binary: return "null";
                case SqlDbType.Bit: return "false";
                case SqlDbType.Char: return "string.Empty";
                case SqlDbType.Date: return "DateTime.MinValue";
                case SqlDbType.DateTime: return "DateTime.MinValue";
                case SqlDbType.DateTime2: return "DateTime.MinValue";
                case SqlDbType.DateTimeOffset: return "DateTime.MinValue";
                case SqlDbType.Decimal: return "decimal.MinValue";
                case SqlDbType.Float: return "double.MinValue";
                case SqlDbType.Image: return "null";
                case SqlDbType.Int: return "int.MinValue";
                case SqlDbType.Money: return "decimal.MinValue";
                case SqlDbType.NChar: return "string.Empty";
                case SqlDbType.NText: return "string.Empty";
                case SqlDbType.NVarChar: return "string.Empty";
                case SqlDbType.Real: return "float.MinValue";
                case SqlDbType.SmallDateTime: return "DateTime.Now";
                case SqlDbType.SmallInt: return "int.MinValue";
                case SqlDbType.SmallMoney: return "float.MinValue";
                case SqlDbType.Structured: return "null";
                case SqlDbType.Text: return "string.Empty";
                case SqlDbType.Time: return "DateTime.MinValue";
                case SqlDbType.Timestamp: return "string.Empty";
                case SqlDbType.TinyInt: return "byte.MinValue";
                case SqlDbType.Udt: return "null";
                case SqlDbType.UniqueIdentifier: return "Guid.Empty";
                case SqlDbType.VarBinary: return "null";
                case SqlDbType.VarChar: return "string.Empty";
                case SqlDbType.Variant: return "null";
                case SqlDbType.Xml: return "string.Empty";

                default:
                    return $"// NO MIN VALUE AVAILABLE FOR {sqlColumn.SqlDataType}";
            }
        }

        /// <summary>
        /// Converts table column data to function argument string.
        /// Sample: string titles, string authors, int bookCount. 
        /// </summary>
        private static string GenerateFunctionArgs(SqlTable sqlTable, eIncludedFields includeTypes, HashSet<char> undesirables)
        {
            var sb = new StringBuilder();
            bool firstFlag = true;

            foreach (SqlColumn sqlColumn in sqlTable.Columns.Values)
            {
                switch (includeTypes)
                {
                    case eIncludedFields.All:

                        if (firstFlag)
                            firstFlag = false;
                        else
                            sb.Append(", ");

                        sb.Append(SQLTypeToCSharpType(sqlColumn) + " " + Formatter.ToCamelCase(sqlColumn.Name, undesirables));
                        break;

                    case eIncludedFields.NoIdentities:

                        if (!sqlColumn.IsIdentity)
                        {
                            if (firstFlag)
                                firstFlag = false;
                            else
                                sb.Append(", ");

                            sb.Append(SQLTypeToCSharpType(sqlColumn) + " " + Formatter.ToCamelCase(sqlColumn.Name, undesirables));
                        }
                        break;

                    case eIncludedFields.PKOnly:

                        if (sqlColumn.IsPk)
                        {
                            if (firstFlag)
                                firstFlag = false;
                            else
                                sb.Append(", ");

                            sb.Append(SQLTypeToCSharpType(sqlColumn) + " " + Formatter.ToCamelCase(sqlColumn.Name, undesirables));
                        }
                        break;

                    default:
                        throw new Exception("eIncludedFields value " + includeTypes.ToString() + " is unrecognized.");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts table column data to function argument string.
        /// Does not include function types.
        /// Sample: titles, authors, bookCount. 
        /// </summary>
        private static string GenerateFunctionList(SqlTable sqlTable, eIncludedFields includeTypes, HashSet<char> undesirables)
        {
            var sb = new StringBuilder();
            bool firstFlag = true;

            foreach (SqlColumn sql_column in sqlTable.Columns.Values)
            {
                if (firstFlag)
                    firstFlag = false;
                else
                    sb.Append(", ");

                switch (includeTypes)
                {
                    case eIncludedFields.All:

                        sb.Append(Formatter.ToCamelCase(sqlTable.Name, undesirables));
                        break;

                    case eIncludedFields.NoIdentities:

                        if (!sql_column.IsIdentity)
                            sb.Append(Formatter.ToCamelCase(sqlTable.Name, undesirables));

                        break;

                    case eIncludedFields.PKOnly:

                        if (sql_column.IsPk)
                            sb.Append(Formatter.ToCamelCase(sqlTable.Name, undesirables));

                        break;

                    default:
                        throw new Exception("eIncludedFields value " + includeTypes.ToString() + " is unrecognized.");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns the CSharp mapping of the SQL datatype.
        /// Maps actual datatypes, not datatype names.
        /// 
        /// sample: varchar(50) -> string
        /// </summary>
        private static string SQLTypeToCSharpType(SqlColumn sqlColumn)
        {
            if (sqlColumn.IsNullable)
            {
                switch (sqlColumn.SqlDataType)
                {
                    case SqlDbType.BigInt: return "long?";
                    case SqlDbType.Binary: return "byte[]";
                    case SqlDbType.Bit: return "bool?";
                    case SqlDbType.Char: return "string";
                    case SqlDbType.Date: return "DateTime?";
                    case SqlDbType.DateTime: return "DateTime?";
                    case SqlDbType.DateTime2: return "DateTime?";
                    case SqlDbType.DateTimeOffset: return "DateTime?";
                    case SqlDbType.Decimal: return "decimal?";
                    case SqlDbType.Float: return "double?";
                    case SqlDbType.Image: return "byte[]";
                    case SqlDbType.Int: return "int?";
                    case SqlDbType.Money: return "decimal?";
                    case SqlDbType.NChar: return "string";
                    case SqlDbType.NText: return "string";
                    case SqlDbType.NVarChar: return "string";
                    case SqlDbType.Real: return "float?";
                    case SqlDbType.SmallDateTime: return "DateTime?";
                    case SqlDbType.SmallInt: return "short?";
                    case SqlDbType.SmallMoney: return "float?";
                    case SqlDbType.Structured: return "// NO TYPE AVAILABLE FOR " + sqlColumn.SqlDataType.ToString();
                    case SqlDbType.Text: return "string";
                    case SqlDbType.Time: return "DateTime?";
                    case SqlDbType.Timestamp: return "string";
                    case SqlDbType.TinyInt: return "byte?";
                    case SqlDbType.Udt: return "byte[]";
                    case SqlDbType.UniqueIdentifier: return "Guid?";
                    case SqlDbType.VarBinary: return "byte[]";
                    case SqlDbType.VarChar: return "string";
                    case SqlDbType.Variant: return "byte[]";
                    case SqlDbType.Xml: return "string";

                    default:
                        return "// NO TYPE AVAILABLE FOR " + sqlColumn.SqlDataType.ToString();
                }
            }
            else
            {
                switch (sqlColumn.SqlDataType)
                {
                    case SqlDbType.BigInt: return "long";
                    case SqlDbType.Binary: return "byte[]";
                    case SqlDbType.Bit: return "bool";
                    case SqlDbType.Char: return "string";
                    case SqlDbType.Date: return "DateTime";
                    case SqlDbType.DateTime: return "DateTime";
                    case SqlDbType.DateTime2: return "DateTime";
                    case SqlDbType.DateTimeOffset: return "DateTime";
                    case SqlDbType.Decimal: return "decimal";
                    case SqlDbType.Float: return "double";
                    case SqlDbType.Image: return "byte[]";
                    case SqlDbType.Int: return "int";
                    case SqlDbType.Money: return "decimal";
                    case SqlDbType.NChar: return "string";
                    case SqlDbType.NText: return "string";
                    case SqlDbType.NVarChar: return "string";
                    case SqlDbType.Real: return "float";
                    case SqlDbType.SmallDateTime: return "DateTime";
                    case SqlDbType.SmallInt: return "short";
                    case SqlDbType.SmallMoney: return "float";
                    case SqlDbType.Structured: return "// NO TYPE AVAILABLE FOR " + sqlColumn.SqlDataType.ToString();
                    case SqlDbType.Text: return "string";
                    case SqlDbType.Time: return "DateTime";
                    case SqlDbType.Timestamp: return "string";
                    case SqlDbType.TinyInt: return "byte";
                    case SqlDbType.Udt: return "byte[]";
                    case SqlDbType.UniqueIdentifier: return "Guid";
                    case SqlDbType.VarBinary: return "byte[]";
                    case SqlDbType.VarChar: return "string";
                    case SqlDbType.Variant: return "byte[]";
                    case SqlDbType.Xml: return "string";

                    default:
                        return "// NO TYPE AVAILABLE FOR " + sqlColumn.SqlDataType.ToString();
                }
            }
        }
    }
}

