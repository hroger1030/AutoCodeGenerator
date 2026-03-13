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
    public class React_CrudMethods : IGenerator
    {
        private const string FEATURE_INSERT = "Readonly Object Component";

        private static readonly HashSet<char> _UndesirableChars =
        [
            '!', '$', '%', '^', '*', '(', ')', '-', '+', '\\', '=',
            '{', '}', '[', ']', ':', ';', '|', '\'', '<', '>', ',',
            '.', '?', '/', '~', '`', '@', '#', '"', ' ', '\t', '&'
        ];

        public string Language => "react";
        public string Version => "19.0";
        public string Category => "ui";
        public string Name => "React/TypeScript";
        public string Description => "Generates react/typescript components and objects for React.";
        public string[] FeatureNames => [FEATURE_INSERT];

        public Dictionary<string, string> DefaultOptions => [];

        public OutputObject Process(string feature, SqlTable sqlTable, Dictionary<string, string> options)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(feature, nameof(feature));
            ArgumentNullException.ThrowIfNull(sqlTable, nameof(sqlTable));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            return feature switch
            {
                FEATURE_INSERT => GenerateReadonlyComponentMethod(sqlTable, options),

                _ => throw new ArgumentOutOfRangeException($"Mode {feature} is not supported by {Name} generator."),
            };
        }

        public OutputObject GenerateReadonlyComponentMethod(SqlTable sqlTable, Dictionary<string, string> options)
        {
            ArgumentNullException.ThrowIfNull(sqlTable);
            ArgumentNullException.ThrowIfNull(options);

            string objectName = ToObjectName(sqlTable.Name);
            var sb = new StringBuilder();

            sb.AppendLine("// TODO - Add stuff");

            return new OutputObject
            {
                FileName = $"{objectName}.tsx",
                Body = sb.ToString(),
                OutputPath = $"{Language}\\{Version}\\components",
            };
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// Helper Methods

        /// <summary>
        /// Returns the SQL column name formatted as a react component name.
        /// 
        /// Sample: FooBar -> FooBar
        /// </summary>
        private static string ToObjectName(string input)
        {
            return Formatter.ToPascalCase(input, _UndesirableChars);
        }
    }
}

