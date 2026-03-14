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
using System.Text;

namespace AutoCodeGen
{
    public class GoLang_ApiMethods : IGenerator
    {
        private const string FEATURE_POGLO_CLASS = "POGLO class";

        private static readonly HashSet<char> _UndesirableChars =
        [
            '!', '$', '%', '^', '*', '(', ')', '-', '+', '\\', '=',
            '{', '}', '[', ']', ':', ';', '|', '\'', '<', '>', ',',
            '.', '?', '/', '~', '`', '@', '#', '"', ' ', '\t', '&'
        ];

        public string Language => "go";
        public string Version => "1.26";
        public string Category => "middle tier";
        public string Name => "Go Lang";
        public string Description => "Generates various API objects, ORM objects, POGLOs, and other classes based on database tables.";
        public string[] FeatureNames => [FEATURE_POGLO_CLASS];

        public Dictionary<string, string> DefaultOptions => [];

        public OutputObject Process(string feature, SqlTable sqlTable, Dictionary<string, string> options)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(feature, nameof(feature));
            ArgumentNullException.ThrowIfNull(sqlTable, nameof(sqlTable));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            return feature switch
            {
                FEATURE_POGLO_CLASS => GeneratePoGlo(sqlTable, options),

                _ => throw new ArgumentOutOfRangeException($"Mode {feature} is not supported by {Name} generator."),
            };
        }

        public OutputObject GeneratePoGlo(SqlTable sqlTable, Dictionary<string, string> options)
        {
            ArgumentNullException.ThrowIfNull(sqlTable);
            ArgumentNullException.ThrowIfNull(options);

            string className = ToExportedName(sqlTable.Name);
            var sb = new StringBuilder();

            sb.AppendLine("package models");
            sb.AppendLine();
            sb.AppendLine("import (");
            sb.AppendLine(Formatter.AddTabs(1) + "\"time\"");
            sb.AppendLine(Formatter.AddTabs(1) + "\"github.com/google/uuid\"");
            sb.AppendLine(Formatter.AddTabs(1) + "\"github.com/shopspring/decimal\"");
            sb.AppendLine(")");
            sb.AppendLine();
            sb.AppendLine($"type {className} struct {{");

            foreach (var col in sqlTable.Columns.Values)
            {
                string propName = ToExportedName(col.Name);  
                string typeHint = SqlTypeToGoType(col);
                sb.AppendLine(Formatter.AddTabs(1) + $"{propName} {typeHint}");
            }

            sb.AppendLine("}");

            return new OutputObject
            {
                FileName = $"{ToExportedName(sqlTable.Name)}.go",
                Body = sb.ToString(),
                OutputPath = $"{Language}\\{Version}\\poglos",
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
        private static string ToExportedName(string input)
        {
            return Formatter.ToPascalCase(input, _UndesirableChars);
        }

        /// <summary>
        /// Returns the GO mapping of the SQL datatype.
        /// </summary>
        private static string SqlTypeToGoType(SqlColumn sqlColumn)
        {
            string baseType = sqlColumn.SqlDataType switch
            {
                SqlDbType.BigInt => "int64",
                SqlDbType.Binary => "[]byte",
                SqlDbType.Bit => "bool",
                SqlDbType.Char => "string",
                SqlDbType.Date => "time.Time",
                SqlDbType.DateTime => "time.Time",
                SqlDbType.DateTime2 => "time.Time",
                SqlDbType.DateTimeOffset => "time.Time",
                SqlDbType.Decimal => "decimal.Decimal",
                SqlDbType.Float => "float64",
                SqlDbType.Image => "[]byte",
                SqlDbType.Int => "int32",
                SqlDbType.Money => "decimal.Decimal",
                SqlDbType.NChar => "string",
                SqlDbType.NText => "string",
                SqlDbType.NVarChar => "string",
                SqlDbType.Real => "float32",
                SqlDbType.SmallDateTime => "time.Time",
                SqlDbType.SmallInt => "int16",
                SqlDbType.SmallMoney => "decimal.Decimal",
                SqlDbType.Text => "string",
                SqlDbType.Time => "time.Time",
                SqlDbType.Timestamp => "string",
                SqlDbType.TinyInt => "uint8",
                SqlDbType.Udt => "[]byte",
                SqlDbType.UniqueIdentifier => "uuid.UUID",
                SqlDbType.VarBinary => "[]byte",
                SqlDbType.VarChar => "string",
                SqlDbType.Variant => "[]byte",
                SqlDbType.Xml => "string",
                SqlDbType.Structured => $"// NO TYPE AVAILABLE FOR {sqlColumn.SqlDataType}",
                _ => $"// NO TYPE AVAILABLE FOR {sqlColumn.SqlDataType}",
            };

            return sqlColumn.IsNullable ? $"*{baseType}" : baseType;
        }
    }
}

