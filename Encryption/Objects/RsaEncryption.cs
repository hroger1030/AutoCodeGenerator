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
    public class RsaEncryption : IRsaEncryption
    {
        #region Constants

            /// <summary>
            /// Available if you have Microsoft Enhanced Cryptographic Provider installed.
            /// </summary>
            public static readonly int DEFAULT_ENHANCED_KEY_SIZE = 16384;
            public static readonly int DEFAULT_BASE_KEY_SIZE = 512;

        #endregion

        #region Methods

            public RsaEncryption() { }

            public bool IsValidKeySize(int key_size)
            {
                // from MS:
                // The RSACryptoServiceProvider supports key sizes from 384 bits to 16384 bits in increments of 8 bits 
                // if you have the Microsoft Enhanced Cryptographic Provider installed. It supports key sizes from 384 
                // bits to 512 bits in increments of 8 bits if you have the Microsoft Base Cryptographic Provider installed.

                return (key_size % 8) != 0;
            }

            public string Encrypt(string text, string public_key, int key_size)
            {
                var encrypted = Encrypt(Encoding.UTF8.GetBytes(text), public_key, key_size);
                return Convert.ToBase64String(encrypted);
            }

            public byte[] Encrypt(byte[] data, string public_key, int key_size)
            {
                if (data == null || data.Length == 0)
                    throw new ArgumentException("Text is null or empty");

                if (string.IsNullOrEmpty(public_key))
                    throw new ArgumentException("Public key is null or empty"); 

                if (IsValidKeySize(key_size))
                    throw new ArgumentException("Key size must be divisible by 8 bits");

                using (var provider = new RSACryptoServiceProvider(key_size))
                {
                    provider.FromXmlString(public_key);
                    return provider.Encrypt(data, true);
                }
            }

            public string DecryptText(string text, string private_key, int key_size)
            {
                var decrypted = Decrypt(Convert.FromBase64String(text), private_key, key_size);
                return Encoding.UTF8.GetString(decrypted);
            }

            public byte[] Decrypt(byte[] data, string private_key, int key_size)
            {
                if (data == null || data.Length == 0)
                    throw new ArgumentException("Text is null or empty");

                if (string.IsNullOrEmpty(private_key))
                    throw new ArgumentException("Private key is null or empty"); 

                if (IsValidKeySize(key_size))
                    throw new ArgumentException("Key size must be divisible by 8 bits");

                using (var provider = new RSACryptoServiceProvider(key_size))
                {
                    provider.FromXmlString(private_key);
                    return provider.Decrypt(data, true);
                }
            }

            public void GenerateKeys(int key_size, out string public_key, out string private_key)
            {
                if (IsValidKeySize(key_size))
                    throw new ArgumentException("Key size must be divisible by 8 bits");

                using (var provider = new RSACryptoServiceProvider(key_size))
                {
                    public_key = provider.ToXmlString(false);
                    private_key = provider.ToXmlString(true);
                }
            }

        #endregion
    }
}
