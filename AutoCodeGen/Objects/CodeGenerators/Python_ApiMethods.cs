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
    public class Python_ApiMethods : IGenerator
    {
        private const string FEATURE_POPO_CLASS = "POPO class";

        private static readonly HashSet<char> _UndesirableChars =
        [
            '!', '$', '%', '^', '*', '(', ')', '-', '+', '\\', '=',
            '{', '}', '[', ']', ':', ';', '|', '\'', '<', '>', ',',
            '.', '?', '/', '~', '`', '@', '#', '"', ' ', '\t', '&'
        ];

        public string Language => "python";
        public string Version => "3.10";
        public string Category => "middle tier";
        public string Name => "Python";
        public string Description => "Generates various API objects, ORM objects, POPOs, and other classes based on database tables.";
        public string[] FeatureNames => [FEATURE_POPO_CLASS];

        public Dictionary<string, string> DefaultOptions => [];

        public OutputObject Process(string feature, SqlTable sqlTable, Dictionary<string, string> options)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(feature, nameof(feature));
            ArgumentNullException.ThrowIfNull(sqlTable, nameof(sqlTable));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            return feature switch
            {
                FEATURE_POPO_CLASS => GeneratePopo(sqlTable, options),

                _ => throw new ArgumentOutOfRangeException($"Mode {feature} is not supported by {Name} generator."),
            };
        }

        public OutputObject GeneratePopo(SqlTable sqlTable, Dictionary<string, string> options)
        {
            ArgumentNullException.ThrowIfNull(sqlTable);
            ArgumentNullException.ThrowIfNull(options);

            string className = ToClassName(sqlTable.Name);
            var sb = new StringBuilder();

            sb.AppendLine("from dataclasses import dataclass");
            sb.AppendLine("from typing import Optional");
            sb.AppendLine();

            sb.AppendLine("@dataclass");
            sb.AppendLine($"class {className}:");
            sb.AppendLine();

            foreach (var col in sqlTable.Columns.Values)
            {
                string propName = ToLocalVariable(col.Name);
                string typeHint = SqlTypeToPythonType(col);

                if (col.IsNullable)
                    sb.AppendLine(Formatter.AddTabs(1) + $"{propName}: {typeHint} = None");
                else
                    sb.AppendLine(Formatter.AddTabs(1) + $"{propName}: {typeHint} = {SqlTypeToPythonDefault(col.SqlDataType)}");
            }

            return new OutputObject
            {
                FileName = $"{className}.py",
                Body = sb.ToString(),
                OutputPath = $"{Language}\\{Version}\\popos",
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
            return Formatter.ToSnakeCase(input, _UndesirableChars);
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
        /// Returns the Python mapping of the SQL datatype.
        /// Maps actual datatypes, not datatype names.
        /// Nullable types are wrapped in Optional[T].
        ///
        /// sample: varchar(50) -> str
        /// sample: int (nullable) -> Optional[int]
        /// </summary>
        private static string SqlTypeToPythonType(SqlColumn sqlColumn)
        {
            string baseType = sqlColumn.SqlDataType switch
            {
                SqlDbType.BigInt => "int",
                SqlDbType.Binary => "bytes",
                SqlDbType.Bit => "bool",
                SqlDbType.Char => "str",
                SqlDbType.Date => "date",
                SqlDbType.DateTime => "datetime",
                SqlDbType.DateTime2 => "datetime",
                SqlDbType.DateTimeOffset => "datetime",
                SqlDbType.Decimal => "Decimal",
                SqlDbType.Float => "float",
                SqlDbType.Image => "bytes",
                SqlDbType.Int => "int",
                SqlDbType.Money => "Decimal",
                SqlDbType.NChar => "str",
                SqlDbType.NText => "str",
                SqlDbType.NVarChar => "str",
                SqlDbType.Real => "float",
                SqlDbType.SmallDateTime => "datetime",
                SqlDbType.SmallInt => "int",
                SqlDbType.SmallMoney => "Decimal",
                SqlDbType.Text => "str",
                SqlDbType.Time => "time",
                SqlDbType.Timestamp => "str",
                SqlDbType.TinyInt => "int",
                SqlDbType.Udt => "bytes",
                SqlDbType.UniqueIdentifier => "UUID",
                SqlDbType.VarBinary => "bytes",
                SqlDbType.VarChar => "str",
                SqlDbType.Variant => "bytes",
                SqlDbType.Xml => "str",
                SqlDbType.Structured => $"# NO TYPE AVAILABLE FOR {sqlColumn.SqlDataType}",

                _ => throw new Exception($"NO TYPE AVAILABLE FOR '{sqlColumn.SqlDataType}'"),
            };

            return sqlColumn.IsNullable ? $"Optional[{baseType}]" : baseType;
        }

        /// <summary>
        /// Generates a default value for the given SQL datatype, to be used in the case of non-nullable columns. 
        /// This is necessary because Python does not allow uninitialized variables, even in data classes.
        /// </summary>
        private static string SqlTypeToPythonDefault(SqlDbType sqlDbType)
        {
            return sqlDbType switch
            {
                SqlDbType.BigInt => "0",
                SqlDbType.Binary => "b''",
                SqlDbType.Bit => "False",
                SqlDbType.Char => "''",
                SqlDbType.Date => "date.min",
                SqlDbType.DateTime => "datetime.min",
                SqlDbType.DateTime2 => "datetime.min",
                SqlDbType.DateTimeOffset => "datetime.min",
                SqlDbType.Decimal => "Decimal('0')",
                SqlDbType.Float => "0.0",
                SqlDbType.Image => "b''",
                SqlDbType.Int => "0",
                SqlDbType.Money => "Decimal('0')",
                SqlDbType.NChar => "''",
                SqlDbType.NText => "''",
                SqlDbType.NVarChar => "''",
                SqlDbType.Real => "0.0",
                SqlDbType.SmallDateTime => "datetime.min",
                SqlDbType.SmallInt => "0",
                SqlDbType.SmallMoney => "Decimal('0')",
                SqlDbType.Text => "''",
                SqlDbType.Time => "time.min",
                SqlDbType.Timestamp => "''",
                SqlDbType.TinyInt => "0",
                SqlDbType.Udt => "b''",
                SqlDbType.UniqueIdentifier => "UUID(int=0)",
                SqlDbType.VarBinary => "b''",
                SqlDbType.VarChar => "''",
                SqlDbType.Variant => "b''",
                SqlDbType.Xml => "''",
                SqlDbType.Structured => "None",

                _ => throw new Exception($"NO DEFAULT VALUE AVAILABLE FOR '{sqlDbType}'"),
            };
        }
    }
}

