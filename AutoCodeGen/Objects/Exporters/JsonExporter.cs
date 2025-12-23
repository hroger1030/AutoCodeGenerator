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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AutoCodeGenLibrary
{
    public class JsonExporter : IPlugin
    {
        // method names
        private const string METHOD_GENERATE_POCO = "JSON exporter";

        // options
        private const string FORMATTING_INDENTED = "Indent JSON output";

        public eLanguage Language
        {
            get { return eLanguage.Json; }
        }

        public eCategory Category
        {
            get { return eCategory.Tools; }
        }

        public List<string> Methods
        {
            get
            {
                return new List<string>()
                {
                    METHOD_GENERATE_POCO,
                };
            }
        }
        public Dictionary<string, string> BaseOptions
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    { FORMATTING_INDENTED, true.ToString() },
                };
            }
        }

        public OutputObject Process(string method, SqlTable sqlTable, Dictionary<string, string> options)
        {
            if (string.IsNullOrEmpty(method))
                throw new ArgumentNullException(nameof(method));

            if (sqlTable == null)
                throw new ArgumentNullException(nameof(sqlTable));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            switch (method)
            {
                case METHOD_GENERATE_POCO: return DataSetToJsonString(sqlTable, options);

                default:
                    throw new NotImplementedException($"Unknown method, '{method}'");
            }
        }

        private OutputObject DataSetToJsonString(SqlTable sqlTable, Dictionary<string, string> options)
        {
            if (sqlTable == null)
                throw new ArgumentNullException(nameof(sqlTable));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var formatting = (options[FORMATTING_INDENTED] == true.ToString()) ? Formatting.Indented : Formatting.None;

            string fileName = NameFormatter.ToCSharpClassName(sqlTable.Name);
            string output = JsonConvert.SerializeObject(sqlTable, formatting);

            return new OutputObject
            {
                FileName = $"{fileName}.json",
                //Type = OutputObject.eObjectType.CSharp,
                OutputPath = "\\Export\\",
                Body = output,
            };
        }
    }
}
