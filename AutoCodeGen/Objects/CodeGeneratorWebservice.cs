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

using DAL.Framework.SqlMetadata;

namespace AutoCodeGenLibrary
{
    public class CodeGeneratorWebservice : CodeGeneratorBase
    {
        public CodeGeneratorWebservice() { }

        public OutputObject GenerateWebServiceControllerClass(SqlTable sqlTable, List<string> namespaceIncludes)
        {
            if (sqlTable == null)
                throw new ArgumentException("Sql table cannot be null");

            string class_name = NameFormatter.ToCSharpClassName(sqlTable.Name);

            OutputObject output = new OutputObject();
            output.Name = class_name + ".cs";
            output.Type = OutputObject.eObjectType.CSharp;

            var sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using using System.Linq;");
            sb.AppendLine("using System.Net.Http;");
            sb.AppendLine("using System.Net.Http.Headers;");
            sb.AppendLine("using System.Web.Http.Description;");
            sb.AppendLine();

            sb.AppendLine(GenerateNamespaceIncludes(namespaceIncludes));
            sb.AppendLine();

            sb.AppendLine("namespace WebService");
            sb.AppendLine("{");

            sb.AppendLine(AddTabs(1) + $"public class {sqlTable.Name}Controller : ApiController");
            sb.AppendLine(AddTabs(1) + "{");

            sb.AppendLine(AddTabs(2) + "[HttpGet]");
            sb.AppendLine(AddTabs(2) + "[Route(\"api/test\")]");
            sb.AppendLine(AddTabs(2) + "public IHttpActionResult Test()");
            sb.AppendLine(AddTabs(2) + "{");
            sb.AppendLine(AddTabs(3) + "try");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "return Ok(\"Hello World\");");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine(AddTabs(3) + "catch (Exception ex)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "return InternalServerError(ex);");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine(AddTabs(2) + "}");

            sb.AppendLine(AddTabs(1) + "}");

            sb.AppendLine("}");

            output.Body = sb.ToString();
            return output;
        }
    }
}

//namespace foo
//{
//    public class SnippetsController : ApiController
//    {
//        private IDataSource _Data;

//        public SnippetsController()
//        {
//            _Data = WebApiApplication.Data;
//        }

//        public SnippetsController(IDataSource data)
//        {
//            _Data = data;
//        }

//        [HttpGet]
//        [Route("api/snippets")]
//        public IHttpActionResult GetUserSnippetes()
//        {
//            HttpRequestHeaders headers = Request.Headers;
//            string emailAddress = string.Empty;

//            if (headers.Contains("emailAddress"))
//                emailAddress = headers.GetValues("emailAddress").First();

//            // no validation needed here

//            var result = _Data.GetSharedSnippets();

//            if (result == null || result.Length < 1)
//                return BadRequest("Failed to find any shared snippets");
//            else
//                return Ok(result);
//        }

//        [HttpGet]
//        [Route("api/snippets/{snippetId:int}")]
//        public IHttpActionResult GetSnippet(int snippetId)
//        {
//            HttpRequestHeaders headers = Request.Headers;
//            string emailAddress = string.Empty;

//            if (headers.Contains("emailAddress"))
//                emailAddress = headers.GetValues("emailAddress").First();

//            var result = _Data.GetSnippet(emailAddress, snippetId);

//            if (result == null)
//                return BadRequest($"Failed to find snippet with id of {snippetId}");
//            else
//                return Ok(result);
//        }

//        [HttpPut]
//        [Route("api/snippets/{snippetId:int}/like")]
//        public IHttpActionResult LikeSnippet(int snippetId)
//        {
//            HttpRequestHeaders headers = Request.Headers;
//            string emailAddress = string.Empty;

//            if (headers.Contains("emailAddress"))
//                emailAddress = headers.GetValues("emailAddress").First();
//            else
//                return Unauthorized();

//            if (!_Data.IsUserValid(emailAddress))
//            {
//                _Log.Warn($"Invalid user {emailAddress}");
//                return Unauthorized();
//            }

//            var result = _Data.LikeSnippet(emailAddress, snippetId);

//            if (result == null)
//                return BadRequest($"Failed to like or unlike snippet {snippetId}");

//            // go notify achievement controllers of change
//            // minor twist here, bump score of author, not person liking snippet
//            string host_name = $"{WebApiApplication.LocalHostName}api/achievements/{(int)eActivityType.Liked}";
//            var buffer = HttpRequestHelper.ApiPut(host_name, result.Owner);

//            return Ok(result);
//        }

//        [HttpPut]
//        [Route("api/snippets/{snippetId:int}/share")]
//        public IHttpActionResult ShareSnippet(int snippetId)
//        {
//            HttpRequestHeaders headers = Request.Headers;
//            string emailAddress = string.Empty;

//            if (headers.Contains("emailAddress"))
//                emailAddress = headers.GetValues("emailAddress").First();
//            else
//                return Unauthorized();

//            if (!_Data.IsUserValid(emailAddress))
//            {
//                _Log.Warn($"Invalid user {emailAddress}");
//                return Unauthorized();
//            }

//            var result = _Data.ToggleSnippetSharing(emailAddress, snippetId);

//            if (result == null)
//                return BadRequest($"Failed to share / unshare snippet {snippetId}");

//            // go notify achievement controllers of change
//            // only tic counter if we set it to shared.
//            if (result.Shared)
//            {
//                string host_name = $"{WebApiApplication.LocalHostName}api/achievements/{(int)eActivityType.Shared}";
//                var buffer = HttpRequestHelper.ApiPut(host_name, emailAddress);
//            }

//            return Ok(result);
//        }

//        [HttpPost]
//        [Route("api/snippets")]
//        [ResponseType(typeof(Snippet))]
//        public IHttpActionResult Post([FromBody] Snippet snippet)
//        {
//            if (snippet == null)
//                return BadRequest("Invalid snippet data");

//            HttpRequestHeaders headers = Request.Headers;
//            string emailAddress = string.Empty;

//            if (headers.Contains("emailAddress"))
//                emailAddress = headers.GetValues("emailAddress").First();
//            else
//                return Unauthorized();

//            if (!_Data.IsUserValid(emailAddress))
//            {
//                _Log.Warn($"Invalid user {emailAddress}");
//                return Unauthorized();
//            }

//            snippet.Owner = emailAddress;
//            snippet.Likes = 0;

//            if (snippet.Text.Length > MAX_SNIPPET_LENGTH)
//                snippet.Text = snippet.Text.Substring(0, MAX_SNIPPET_LENGTH);

//            try
//            {
//                var new_snippet = _Data.AddSnippet(snippet);

//                // go notify achievement controllers of change
//                string host_name = $"{WebApiApplication.LocalHostName}api/achievements/{(int)eActivityType.Created}";
//                var buffer = HttpRequestHelper.ApiPut(host_name, emailAddress);

//                return Ok(new_snippet);
//            }
//            catch (Exception ex)
//            {
//                _Log.Error(ex);
//                return InternalServerError(ex);
//            }
//        }
//    }
//}