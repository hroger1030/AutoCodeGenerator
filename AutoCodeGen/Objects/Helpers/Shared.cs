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

namespace AutoCodeGenLibrary
{
    public static class Shared
    {
        /// <summary>
        /// This object is a collection of characters that will create problems if they are used in c# class or variable names.
        /// </summary>
        public readonly static string[] CSHARP_UNDESIREABLES = new string[] { "!", "$", "%", "^", "*", "(", ")", "-", "+", "=", "{", "}", "[", "]", ":", ";", "|", "'", "<", ">", ",", ".", "?", "/", " ", "~", "`", "\"", "\\" };

        public readonly static int[] PRIME_NUMBER_LIST = new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199, 211, 223, 227, 229, 233, 239, 241, 251, 257, 263, 269, 271 };

        public readonly static string CS_SQL_PARAMETER_TEMPLATE = "new SqlParameter() {{ ParameterName = \"{0}\", SqlDbType = SqlDbType.{1}, Size = {2}, Value = {3} }},";
    }
}
