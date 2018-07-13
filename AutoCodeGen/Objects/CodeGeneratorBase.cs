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

using DAL.SqlMetadata;

namespace AutoCodeGenLibrary
{
    public class CodeGeneratorBase
    {
        protected static string AddTabs(int tabCount)
        {
            if (tabCount < 0)
                throw new ArgumentException($"Cannot generate a string of {tabCount} tabs");

            var output = string.Empty;

            while (tabCount > 0)
            {
                output += '\t';
                tabCount--;
            }

            return output;
        }

        protected static int GetLongestColumnLength(SqlTable sqlTable)
        {
            if (sqlTable == null)
                throw new ArgumentException("Cannot compute longest column of a null table.");

            int longest_column_length = 0;

            foreach (var column in sqlTable.Columns.Values)
            {
                if (column.Name.Length > longest_column_length)
                    longest_column_length = column.Name.Length;
            }

            return longest_column_length;
        }

        protected static string PadVariableName(string input, int longest_string_length, int additional_characters, int tab_size)
        {
            longest_string_length += tab_size + additional_characters;
            int pad_length = longest_string_length + (tab_size - (longest_string_length % tab_size));
            return input.PadRight(pad_length, ' ');
        }
    }
}