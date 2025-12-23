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
using System.Linq;
using System.Text;

namespace AutoCodeGenLibrary
{
    public static class Helper
    {
        public static string AddTabs(int count, int tabSize = 4, bool convertToSpaces = false)
        {
            if (count < 0)
                throw new ArgumentException($"Cannot generate a string of {count} tabs");

            if (tabSize < 1)
                throw new ArgumentException($"Tab size cannot be defined as less than zero white spaces");

            var sb = new StringBuilder();

            int totalCount = count;

            // do we preserve the tabs or convert to whitespace?
            if (convertToSpaces)
                totalCount *= tabSize;

            while (totalCount > 0)
            {
                if (convertToSpaces)
                    sb.Append(' ');
                else
                    sb.Append('\t');

                totalCount--;
            }

            return sb.ToString();
        }

        public static List<string> StringToList(string list, char delimiter = ';')
        {
            var output = new List<string>();

            if (list == null)
                return output;

            var buffer = list.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

            return buffer.ToList();
        }
    }
}