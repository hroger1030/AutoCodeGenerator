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
using System.Security.Cryptography;
using System.Text;

namespace Encryption
{
    public static class HashGenerator
    {
        public static string ComputePBKDF2Hash(string input, string salt, int iterations)
        {
            byte[] input_bytes = Encoding.UTF8.GetBytes(input);
            byte[] salt_bytes = Encoding.UTF8.GetBytes(salt);

            return ComputePBKDF2Hash(input_bytes, salt_bytes, iterations);
        }

        public static string ComputePBKDF2Hash(byte[] input, byte[] salt, int iterations)
        {
            if (input == null || input.Length == 0)
                throw new ArgumentException("input is null or empty");

            if (input == null || input.Length == 0)
                throw new ArgumentException("input is null or empty");

            if (iterations < 1)
                throw new ArgumentException("You must have 1 or more password iterations");

            using (var deriveBytes = new Rfc2898DeriveBytes(input, salt, iterations))
            {
                // specify that we want to randomly generate a 20-byte salt
                byte[] result = deriveBytes.GetBytes(20);
                return Convert.ToBase64String(result);
            }
        }

        /// <summary>
        /// Generates hash of 160 bits ~ 10 chars
        /// </summary>
        public static string ComputeSHA160Hash(string input)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(input);
            return ComputeSHA160Hash(buffer);
        }

        public static string ComputeSHA160Hash(byte[] input)
        {
            if (input == null || input.Length == 0)
                throw new ArgumentException("input is null or empty");

            using (var provider = new SHA1Managed())
            {
                byte[] result = provider.ComputeHash(input);
                return Convert.ToBase64String(result);
            }
        }

        /// <summary>
        /// Generates hash of 256 bits ~ 16 chars
        /// </summary>
        public static string ComputeSHA256Hash(string input)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(input);
            return ComputeSHA256Hash(buffer);
        }

        public static string ComputeSHA256Hash(byte[] input)
        {
            if (input == null || input.Length == 0)
                throw new ArgumentException("input is null or empty");

            using (var provider = new SHA256Managed())
            {
                byte[] result = provider.ComputeHash(input);
                return Convert.ToBase64String(result);
            }
        }

        /// <summary>
        /// Generates hash of 384 bits ~ 24 chars
        /// </summary>
        public static string ComputeSHA384Hash(string input)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(input);
            return ComputeSHA384Hash(buffer);
        }

        public static string ComputeSHA384Hash(byte[] input)
        {
            if (input == null || input.Length == 0)
                throw new ArgumentException("input is null or empty");

            using (var provider = new SHA384Managed())
            {
                byte[] result = provider.ComputeHash(input);
                return Convert.ToBase64String(result);
            }
        }

        /// <summary>
        /// Generates hash of 512 bits ~ 32 chars
        /// </summary>
        public static string ComputeSHA512Hash(string input)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(input);
            return ComputeSHA512Hash(buffer);
        }

        public static string ComputeSHA512Hash(byte[] input)
        {
            if (input == null || input.Length == 0)
                throw new ArgumentException("input is null or empty");

            using (var provider = new SHA512Managed())
            {
                byte[] result = provider.ComputeHash(input);
                return Convert.ToBase64String(result);
            }
        }

        /// <summary>
        /// Generates a 128 bit MD5 hash 
        /// </summary>
        public static string ComputeMD5Hash(string input)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(input);
            return ComputeMD5Hash(buffer);
        }

        public static string ComputeMD5Hash(byte[] input)
        {
            if (input == null || input.Length == 0)
                throw new ArgumentException("input is null or empty");

            using (var provider = new MD5CryptoServiceProvider())
            {
                byte[] result = provider.ComputeHash(input);
                return Convert.ToBase64String(result);
            }
        }
    }
}

