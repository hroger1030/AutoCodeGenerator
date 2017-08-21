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
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Encryption
{
    /// <summary>
    /// This class uses a symmetric key algorithm (Rijndael/AES) to encrypt and decrypt data.
    /// </summary>
    public class AesEncryption : IAesEncryption
    {
            private const int DEFAULT_SALT_LENGTH = 64;

            private RNGCryptoServiceProvider _Random = new RNGCryptoServiceProvider(); 

            /// <summary>
            /// Number of hash iterations used to generate password. 
            /// </summary>
            private int _Iterations;

            /// <summary>
            /// Initialization vector (or IV). This value is required to encrypt the first block of plaintext data. 
            /// For RijndaelManaged class IV must be exactly 16 ASCII characters long.
            /// </summary>
            private string _InitialVector;

            /// <summary>
            /// Size of encryption key in bits. Allowed values are: 128, 192, and 256. 
            /// </summary>
            private int _KeySize;

            public AesEncryption() { }

            public AesEncryption(string initial_vector, int password_iterations, int key_size)
            {
                _InitialVector      = initial_vector;
                _Iterations         = password_iterations;
                _KeySize            = key_size;
            }


            public string Encrypt(string plain_text, string password, string salt)
            {
                return Encrypt(plain_text, password, salt, _InitialVector, _Iterations, _KeySize);
            }     

            public byte[] Encrypt(byte[] plain_text, string password, string salt)
            {
                return Encrypt(plain_text, password, salt, _InitialVector, _Iterations, _KeySize);
            }

            public string Encrypt(string plain_text, string password, string salt, string initial_vector, int password_iterations, int key_size)
            {
                byte[] init_vector_bytes        = Encoding.UTF8.GetBytes(initial_vector);
                byte[] salt_bytes               = Encoding.UTF8.GetBytes(salt);
                byte[] password_bytes           = Encoding.UTF8.GetBytes(password);
                byte[] plain_text_bytes         = Encoding.UTF8.GetBytes(plain_text);
              
                byte[] buffer = Encrypt(plain_text_bytes, password_bytes, salt_bytes, init_vector_bytes, password_iterations, key_size);
                return Convert.ToBase64String(buffer);
            }

            public byte[] Encrypt(byte[] plain_text, string password, string salt, string initial_vector, int password_iterations, int key_size)
            {
                byte[] init_vector_bytes        = Encoding.UTF8.GetBytes(initial_vector);
                byte[] salt_bytes               = Encoding.UTF8.GetBytes(salt);
                byte[] password_bytes           = Encoding.UTF8.GetBytes(password);
              
                return Encrypt(plain_text, password_bytes, salt_bytes, init_vector_bytes, password_iterations, key_size);
            }


            public string Decrypt(string cipher_text, string password, string salt)
            {
                return Decrypt(cipher_text, password, salt, _InitialVector, _Iterations, _KeySize);
            }

            public byte[] Decrypt(byte[] cipher_text, string password, string salt)
            {
                return Decrypt(cipher_text, password, salt, _InitialVector, _Iterations, _KeySize);
            }

            public string Decrypt(string cipher_text, string password, string salt, string initial_vector, int password_iterations, int key_size)
            {
                byte[] init_vector_bytes    = Encoding.UTF8.GetBytes(initial_vector);
                byte[] salt_bytes           = Encoding.UTF8.GetBytes(salt);
                byte[] password_bytes       = Encoding.UTF8.GetBytes(password);  
                byte[] cipher_text_bytes    = Convert.FromBase64String(cipher_text);
                    
                byte[] buffer = Decrypt(cipher_text_bytes, password_bytes, salt_bytes, init_vector_bytes, password_iterations, key_size);
                return Encoding.UTF8.GetString(buffer);
            }

            public byte[] Decrypt(byte[] cipher_text, string password, string salt, string initial_vector, int password_iterations, int key_size)
            {
                byte[] init_vector_bytes    = Encoding.UTF8.GetBytes(initial_vector);
                byte[] salt_bytes           = Encoding.UTF8.GetBytes(salt);
                byte[] password_bytes       = Encoding.UTF8.GetBytes(password);  
                    
                return Decrypt(cipher_text, password_bytes, salt_bytes, init_vector_bytes, password_iterations, key_size);
            }


            private byte[] Encrypt(byte[] plain_text, byte[] password, byte[] salt, byte[] initial_vector, int password_iterations, int key_size)
            {
                if (plain_text == null || plain_text.Length < 1)
                    throw new ArgumentException("plain text is null or empty");

                if (password == null || password.Length < 1)
                    throw new ArgumentException("Password is null or empty");

                if (!IsSaltValid(salt))
                    throw new ArgumentException("Salt must be at least 8 bytes long");

                if (!IsInitialVectorValid(initial_vector))
                    throw new ArgumentException("Initial vector must be 16 characters long.");

                if (password_iterations < 1)
                    throw new ArgumentException("password must have 1 or more iterations.");

                if (!IsKeySizeValid(key_size))
                    throw new ArgumentException("Invalid key size, must be 128, 192, or 256 bytes in size");

                byte[] encryptedBytes = null;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (RijndaelManaged AES = new RijndaelManaged())
                    {
                        AES.KeySize = key_size;
                        AES.BlockSize = 128;

                        var key = new Rfc2898DeriveBytes(password, salt, password_iterations);
                        AES.Key = key.GetBytes(AES.KeySize / 8);
                        AES.IV = initial_vector;

                        AES.Mode = CipherMode.CBC;

                        using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(plain_text, 0, plain_text.Length);
                            cs.Close();
                        }

                        encryptedBytes = ms.ToArray();
                    }
                }

                return encryptedBytes;
            }

            private byte[] Decrypt(byte[] encrypted_text, byte[] password, byte[] salt, byte[] initial_vector, int password_iterations, int key_size)
            {
                if (encrypted_text == null || encrypted_text.Length < 1)
                    throw new ArgumentException("cipher text is null or empty");

                if (password == null || password.Length < 1)
                    throw new ArgumentException("Pass phrase is null or empty");

                if (!IsSaltValid(salt))
                    throw new ArgumentException("Salt must be at least 8 bytes long");

                if (!IsInitialVectorValid(initial_vector))
                    throw new ArgumentException("Initial vector must be 16 characters long.");

                if (password_iterations < 1)
                    throw new ArgumentException("password must have 1 or more iterations.");

                if (!IsKeySizeValid(key_size))
                    throw new ArgumentException("Invalid key size, must be 128, 192, or 256 bytes in size");

                byte[] decryptedBytes = null;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (RijndaelManaged AES = new RijndaelManaged())
                    {
                        AES.KeySize = key_size;
                        AES.BlockSize = 128;

                        var key = new Rfc2898DeriveBytes(password, salt, password_iterations);
                        AES.Key = key.GetBytes(AES.KeySize / 8);
                        AES.IV = initial_vector;

                        AES.Mode = CipherMode.CBC;

                        using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(encrypted_text, 0, encrypted_text.Length);
                            cs.Close();
                        }

                        decryptedBytes = ms.ToArray();
                    }
                }

                return decryptedBytes;
            }


            public string GenerateSalt()
            {
                return GenerateSalt(DEFAULT_SALT_LENGTH);
            }

            public string GenerateSalt(int length)
            {
                if (length < 1)
                    throw new ArgumentException("Salt length cannot be less than 1 byte long");

                byte[] random_number = new byte[length];
                _Random.GetBytes(random_number);

                return Convert.ToBase64String(random_number);
            }

            private bool IsKeySizeValid(int key_size)
            {
                return (key_size == 128 || key_size == 192 || key_size == 256);
            }

            private bool IsSaltValid(string salt)
            {
                byte[] salt_bytes = Encoding.ASCII.GetBytes(salt);
                return IsSaltValid(salt_bytes);
            }

            private bool IsSaltValid(byte[] salt)
            {
                return salt.Length > 7;
            }

            private bool IsInitialVectorValid(string initial_vector)
            {
                byte[] iv_bytes = Encoding.ASCII.GetBytes(initial_vector);
                return IsInitialVectorValid(iv_bytes);
            }

            private bool IsInitialVectorValid(byte[] initial_vector)
            {
                return initial_vector.Length == 16;
            }
    }
}