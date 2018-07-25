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

using System.Collections.Generic;
using System.Text;

using DAL.SqlMetadata;

namespace AutoCodeGenLibrary
{
    public partial class CodeGeneratorWebservice : CodeGeneratorBase
    {
        // Webservice Generation
        public static OutputObject GenerateWebServiceCodeInfrontClass(SqlTable sqlTable)
        {
            if (sqlTable == null)
                return null;

            string class_name = NameFormatter.ToCSharpClassName(sqlTable.Name);

            OutputObject output = new OutputObject();
            output.Name = class_name + ".asmx";
            output.Type = OutputObject.eObjectType.WebService;

            var sb = new StringBuilder();

            sb.AppendLine("<%@ WebService Language=\"C#\" CodeBehind=\"Service1.asmx.cs\" Class=\"WebService1.Service1\" %>");

            output.Body = sb.ToString();
            return output;
        }

        public static OutputObject GenerateWebServiceCodeBehindClass(SqlTable sqlTable, List<string> namespace_includes)
        {
            if (sqlTable == null)
                return null;

            string class_name = NameFormatter.ToCSharpClassName(sqlTable.Name);

            OutputObject output = new OutputObject();
            output.Name = class_name + ".asmx.cs";
            output.Type = OutputObject.eObjectType.CSharp;

            var sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Web;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Web.Services;");
            sb.AppendLine("using System.Web.Services.Protocols;");
            sb.AppendLine("using System.ComponentModel;");
            sb.AppendLine();

            sb.AppendLine(GenerateNamespaceIncludes(namespace_includes));
            sb.AppendLine();

            sb.AppendLine("namespace WebService1");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "[WebService(Namespace = \"http://tempuri.org/\")]");
            sb.AppendLine(AddTabs(1) + "[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]");
            sb.AppendLine(AddTabs(1) + "[ToolboxItem(false)]");
            sb.AppendLine(AddTabs(1) + "public class Service1 : System.Web.Services.WebService");
            sb.AppendLine(AddTabs(1) + "{");
            sb.AppendLine(AddTabs(2) + "[WebMethod]");
            sb.AppendLine(AddTabs(2) + "public string HelloWorld()");
            sb.AppendLine(AddTabs(2) + "{");
            sb.AppendLine(AddTabs(3) + "return \"Hello World\";");
            sb.AppendLine(AddTabs(2) + "}");
            sb.AppendLine(AddTabs(1) + "}");
            sb.AppendLine("}");

            output.Body = sb.ToString();
            return output;
        }
    }
}