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

using DAL.Standard.SqlMetadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoCodeGenLibrary
{
    public class CSharp_POCO : IOutputPlugin
    {
        private const string OutputPath = "\\C#\\pocos\\";

        // option names
        private const string NAMESPACE_INCLUDES = "Included namespaces";

        public string Language => "C#";
        public string Category => "MiddleTier";
        public string Name => "POCO Generator";
        public string Description => "Generates Plain Old C# Objects (POCO) classes based on database tables.";

        public Dictionary<string, string> BaseOptions => new Dictionary<string, string>
        {
            { NAMESPACE_INCLUDES, string.Empty },
        };

        public CSharp_POCO() { }

        public OutputObject Process(SqlTable sqlTable, Dictionary<string, string> options)
        {
            if (sqlTable == null)
                throw new ArgumentNullException(nameof(sqlTable));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return GenerateCSharpPoCoClass(sqlTable, options);
        }

        public OutputObject GenerateCSharpPoCoClass(SqlTable sqlTable, Dictionary<string, string> options)
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

            #region sample output
            //public Foo() { }
            #endregion

            sb.AppendLine();
            sb.AppendLine(Helper.AddTabs(2) + $"public {NameFormatter.ToCSharpClassName(sqlTable.Name)}() {{ }}");

            ////////////////////////////////////////////////////////////////////////////////
            #endregion Default CTOR

            sb.AppendLine(Helper.AddTabs(1) + "}");
            sb.Append("}");

            return new OutputObject
            {
                FileName = $"{className}.cs",
                Body = sb.ToString(),
                OutputPath = OutputPath,
            };
        }
    }
}

