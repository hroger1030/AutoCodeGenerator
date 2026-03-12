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
using System.Text;

namespace AutoCodeGen
{
    public class Sql_StoredProcs : IGenerator
    {
        private const string OutputPath = "SQL\\";
        private const string FEATURE_INSERT = "Insert table SP";

        // option names
        private const string EXISTENCE_CHECKS = "Include existence checks";

        public string Language => "SQL";
        public string Category => "Database";
        public string Name => "MS-SQL Stored Procs";
        public string Description => "Generates MS-SQL stored procedures.";
        public string[] FeatureNames => [FEATURE_INSERT];

        public Dictionary<string, string> DefaultOptions => new()
        {
            { EXISTENCE_CHECKS, string.Empty },
        };

        public Sql_StoredProcs() { }

        public OutputObject Process(string feature, SqlTable sqlTable, Dictionary<string, string> options)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(feature, nameof(feature));
            ArgumentNullException.ThrowIfNull(sqlTable, nameof(sqlTable));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            return feature switch
            {
                FEATURE_INSERT => GenerateMethod(sqlTable, options),

                _ => throw new ArgumentOutOfRangeException($"Mode {feature} is not supported by {Name} generator."),
            };
        }

        public OutputObject GenerateMethod(SqlTable sqlTable, Dictionary<string, string> options)
        {
            string className = NameFormatter.ToCSharpClassName(sqlTable.Name);
            var sb = new StringBuilder();

            sb.AppendLine("// TODO - Add stuff");

            return new OutputObject
            {
                FileName = $"{className}.cs",
                Body = sb.ToString(),
                OutputPath = $"{OutputPath}\\stored_procedures",
            };
        }

    }
}

