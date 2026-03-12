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
using System.Text;

namespace AutoCodeGen
{
    public static partial class NameFormatter
    {
        private static readonly HashSet<char> _Delimiters = new() { '_', '-', ' ' };

        /// <summary>
        /// method to sanitize a string by removing any characters that are not valid for use in a particular language.
        /// </summary>
        public static string RemoveInvalidCharacters(string input, HashSet<char> invalidCharacters)
        {
            if (string.IsNullOrEmpty(input) || invalidCharacters == null || invalidCharacters.Count == 0)
                return input;

            var sb = new StringBuilder(input.Length);

            foreach (char c in input)
            {
                if (!invalidCharacters.Contains(c))
                    sb.Append(c);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts a SQL column name to PascalCase suitable for use as a variable/identifier.
        /// Splits on word boundaries, removes all undesirable chars, and capitalizes the first letter of each segment.
        ///
        /// Samples:
        ///   foo_bar       -> FooBar
        ///   fooBar        -> FooBar
        ///   my-column     -> MyColumn
        ///   __weird__col  -> WeirdCol
        ///   FOO_BAR       -> FooBar
        /// </summary>
        public static string ToTitleCase(string input, HashSet<char> undesirables, HashSet<char> delimiters = null)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            if (undesirables == null)
                undesirables = new HashSet<char>();

            if (delimiters == null)
                delimiters = _Delimiters;

            var sb = new StringBuilder(input.Length);
            bool capitalizeNext = true;

            for (int i = 0; i < input.Length; i++)
            {
                if (delimiters.Contains(input[i]))
                {
                    capitalizeNext = true;
                    continue;
                }

                if (undesirables.Contains(input[i]))
                    continue;

                if (char.IsUpper(input[i]) && i > 0 && char.IsLower(input[i - 1]))
                {
                    // camelCase boundary: fooBar -> Foo|Bar
                    sb.Append(char.ToUpperInvariant(input[i]));
                    capitalizeNext = false;
                }
                else if (capitalizeNext)
                {
                    sb.Append(char.ToUpperInvariant(input[i]));
                    capitalizeNext = false;
                }
                else
                {
                    sb.Append(char.ToLowerInvariant(input[i]));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts a sql column name to a case that will work as a variable.
        /// ex: FooBar -> foo_bar
        /// </summary>
        public static string ToSnakeCase(string input, HashSet<char> undesirables)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(input);
            ArgumentNullException.ThrowIfNull(undesirables);

            var sb = new StringBuilder(input);
            bool firstFlag = true;

            foreach (char c in input)
            {
                if (undesirables.Contains(c))
                    continue;

                if (char.IsUpper(c) && !firstFlag && sb[sb.Length - 1] != '_')
                    sb.Append('_');

                sb.Append(c);

                if (firstFlag)
                    firstFlag = false;
            }

            return sb.ToString().ToLower();
        }

        /// <summary>
        /// Converts a sql column name to a case that will work as a variable.
        /// ex: FooBar -> fooBar
        /// </summary>
        public static string ToCamelCase(string input, HashSet<char> undesirables)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(input);
            ArgumentNullException.ThrowIfNull(undesirables);

            var sb = new StringBuilder(input);
            bool firstFlag = true;
            bool nextCharUpper = false;

            foreach (char c in input)
            {
                if (undesirables.Contains(c))
                    continue;

                if (firstFlag)
                {
                    firstFlag = false;
                    sb.Append(char.ToLower(c));
                }
                else
                {
                    if (c == ' ' || c == '_')
                    {
                        nextCharUpper = true;
                        continue;
                    }

                    if (nextCharUpper)
                    {
                        sb.Append(char.ToUpper(c));
                        nextCharUpper = false;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts a sql column name to a readable case that will work in a comment.
        /// ex: FooBar -> Foo Bar
        /// </summary>
        public static string ToFriendlyCase(string input, HashSet<char> undesirables)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(input);
            ArgumentNullException.ThrowIfNull(undesirables);

            var sb = new StringBuilder();
            bool firstFlag = true;

            foreach (char c in input)
            {
                if (undesirables.Contains(c))
                    continue;

                if (c == '_')
                {
                    sb.Append(' ');
                    continue;
                }

                if (char.IsUpper(c) && !firstFlag && sb[sb.Length - 1] != ' ')
                    sb.Append(' ');

                sb.Append(c);

                if (firstFlag)
                    firstFlag = false;
            }
            return sb.ToString();
        }
    }
}