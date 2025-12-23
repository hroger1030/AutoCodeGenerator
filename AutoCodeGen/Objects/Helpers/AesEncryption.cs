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

namespace AutoCodeGenLibrary
{
    /// <summary>
    /// This class uses a symmetric key algorithm (AES) to encrypt and decrypt data.
    /// </summary>
    public sealed class AesEncryption
    {
        private const int DEFAULT_KEY_SIZE = 256;
        private const int BLOCK_SIZE = 128;
        private const int DEFAULT_SALT_LENGTH = 64;
        private const string DEFAULT_IV = "01234567890abcdef";
        private const int DEFAULT_ITERATIONS = 1;

        private readonly RandomNumberGenerator _Random;

        /// <summary>
        /// Number of hash iterations used to generate password. 
        /// </summary>
        private readonly int _Iterations;

        /// <summary>
        /// Initialization vector (or IV). This value is required to encrypt the first block of plaintext data. 
        /// </summary>
        private readonly string _InitialVector;

        /// <summary>
        /// Size of encryption key in bits. Allowed values are: 128, 192, and 256. 
        /// </summary>
        private readonly int _KeySize;

        public AesEncryption() : this(DEFAULT_IV, DEFAULT_ITERATIONS, DEFAULT_KEY_SIZE) { }

        public AesEncryption(string initialVector, int passwordIterations, int keySize)
        {
            _Random = RandomNumberGenerator.Create();
            _InitialVector = initialVector;
            _Iterations = passwordIterations;
            _KeySize = keySize;
        }


        public string Encrypt(string plainText, string password, string salt)
        {
            return Encrypt(plainText, password, salt, _InitialVector, _Iterations, _KeySize);
        }

        public byte[] Encrypt(byte[] plainText, string password, string salt)
        {
            return Encrypt(plainText, password, salt, _InitialVector, _Iterations, _KeySize);
        }

        public string Encrypt(string plainText, string password, string salt, string initialVector, int passwordIterations, int keySize)
        {
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(initialVector);
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            byte[] buffer = Encrypt(plainTextBytes, passwordBytes, saltBytes, initVectorBytes, passwordIterations, keySize);
            return Convert.ToBase64String(buffer);
        }

        public byte[] Encrypt(byte[] plainText, string password, string salt, string initialVector, int passwordIterations, int keySize)
        {
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(initialVector);
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            return Encrypt(plainText, passwordBytes, saltBytes, initVectorBytes, passwordIterations, keySize);
        }

        public byte[] Encrypt(byte[] plainText, byte[] password, byte[] salt, byte[] initialVector, int passwordIterations, int keySize)
        {
            if (plainText == null || plainText.Length < 1)
                throw new ArgumentException("plain text is null or empty");

            if (password == null || password.Length < 1)
                throw new ArgumentException("Password is null or empty");

            if (!IsSaltValid(salt))
                throw new ArgumentException("Salt must be at least 8 bytes long");

            if (!IsInitialVectorValid(initialVector))
                throw new ArgumentException("Initial vector must be 16 characters long.");

            if (passwordIterations < 1)
                throw new ArgumentException("password must have 1 or more iterations.");

            if (!IsKeySizeValid(keySize))
                throw new ArgumentException("Invalid key size, must be 128, 192, or 256 bytes in size");

            byte[] encryptedBytes = null;

            using (var aes = Aes.Create())
            {
                using var keyDerivation = new Rfc2898DeriveBytes(
                    password,
                    salt,
                    passwordIterations,
                    HashAlgorithmName.SHA256);

                aes.KeySize = keySize;
                aes.BlockSize = BLOCK_SIZE;
                aes.Key = keyDerivation.GetBytes(keySize / 8);
                aes.IV = initialVector;
                aes.Mode = CipherMode.CBC;

                using var ms = new MemoryStream();
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(plainText, 0, plainText.Length);
                    cs.Close();
                }

                encryptedBytes = ms.ToArray();
            }

            return encryptedBytes;
        }


        public string Decrypt(string cipherText, string password, string salt)
        {
            return Decrypt(cipherText, password, salt, _InitialVector, _Iterations, _KeySize);
        }

        public byte[] Decrypt(byte[] cipherText, string password, string salt)
        {
            return Decrypt(cipherText, password, salt, _InitialVector, _Iterations, _KeySize);
        }

        public string Decrypt(string cipherText, string password, string salt, string initialVector, int passwordIterations, int keySize)
        {
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(initialVector);
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);

            byte[] buffer = Decrypt(cipherTextBytes, passwordBytes, saltBytes, initVectorBytes, passwordIterations, keySize);
            return Encoding.UTF8.GetString(buffer);
        }

        public byte[] Decrypt(byte[] cipherText, string password, string salt, string initialVector, int passwordIterations, int keySize)
        {
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(initialVector);
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            return Decrypt(cipherText, passwordBytes, saltBytes, initVectorBytes, passwordIterations, keySize);
        }

        public byte[] Decrypt(byte[] encryptedText, byte[] password, byte[] salt, byte[] initialVector, int passwordIterations, int keySize)
        {
            if (encryptedText == null || encryptedText.Length < 1)
                throw new ArgumentException("cipher text is null or empty");

            if (password == null || password.Length < 1)
                throw new ArgumentException("Pass phrase is null or empty");

            if (!IsSaltValid(salt))
                throw new ArgumentException("Salt must be at least 8 bytes long");

            if (!IsInitialVectorValid(initialVector))
                throw new ArgumentException("Initial vector must be 16 characters long.");

            if (passwordIterations < 1)
                throw new ArgumentException("password must have 1 or more iterations.");

            if (!IsKeySizeValid(keySize))
                throw new ArgumentException("Invalid key size, must be 128, 192, or 256 bytes in size");

            byte[] decryptedBytes = null;

            using (var aes = Aes.Create())
            {
                using var keyDerivation = new Rfc2898DeriveBytes(
                    password,
                    salt,
                    passwordIterations,
                    HashAlgorithmName.SHA256);

                aes.KeySize = keySize;
                aes.BlockSize = BLOCK_SIZE;
                aes.Key = keyDerivation.GetBytes(keySize / 8);
                aes.IV = initialVector;
                aes.Mode = CipherMode.CBC;

                using var ms = new MemoryStream();
                using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(encryptedText, 0, encryptedText.Length);
                    cs.Close();
                }

                decryptedBytes = ms.ToArray();
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

            byte[] output = new byte[length];
            _Random.GetBytes(output);

            return Convert.ToBase64String(output);
        }

        public bool IsKeySizeValid(int keySize)
        {
            return keySize == 128 || keySize == 192 || keySize == 256;
        }

        public bool IsSaltValid(string salt)
        {
            byte[] saltBytes = Encoding.ASCII.GetBytes(salt);
            return IsSaltValid(saltBytes);
        }

        public bool IsSaltValid(byte[] salt)
        {
            return salt.Length > 7;
        }

        public bool IsInitialVectorValid(string initialVector)
        {
            byte[] ivBytes = Encoding.ASCII.GetBytes(initialVector);
            return IsInitialVectorValid(ivBytes);
        }

        public bool IsInitialVectorValid(byte[] initialVector)
        {
            return initialVector.Length == 16;
        }
    }
}