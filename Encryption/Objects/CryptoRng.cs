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

namespace Encryption
{
    /// <summary>
    /// The CryptoRand class is a wrapper around the RNGCryptoServiceProvider 
    /// object, with helper methods to make implimentations simpler.
    /// </summary>
    public class CryptoRng : ICryptoRng
    {
        #region Fields

            private RNGCryptoServiceProvider _Random = new RNGCryptoServiceProvider(); 
            private int _PasswordLength;

        #endregion

        #region Methods
            
            public CryptoRng(int password_length)
            {
                _PasswordLength = password_length;
            }

            public Guid GenerateGuid()
            {
                byte[] buffer = new byte[16];
                _Random.GetBytes(buffer);

                return new Guid(buffer); 
            }

            public double GenerateDouble()
            {
                byte[] buffer = new byte[8];
                _Random.GetBytes(buffer);

                return BitConverter.ToDouble(buffer, 0);
            }

            public int GenerateInt()
            {
                byte[] buffer = new byte[4];
                _Random.GetBytes(buffer);

                return BitConverter.ToInt32(buffer, 0);
            }

            public uint GenerateUint()
            {
                byte[] buffer = new byte[4];
                _Random.GetBytes(buffer);

                return BitConverter.ToUInt32(buffer, 0);
            }

            public byte[] GenerateByteArray(int length)
            {
                byte[] buffer = new byte[length];
                _Random.GetBytes(buffer);

                return buffer;
            }

            public int GenerateInt(int max_value)
            {
                return GenerateInt(0,max_value);
            }

            public int GenerateInt(int min_value, int max_value)
            {
                // add one to max to include the max value endpoint.
                max_value++;

                if (min_value > max_value)
                {
                    min_value = min_value ^ max_value; 
                    max_value = min_value ^ max_value;
                    min_value = min_value ^ max_value;
                }

                int offset = max_value - min_value;  

                byte[] random_number = new byte[4];
                _Random.GetBytes(random_number);

                int buffer = Math.Abs(BitConverter.ToInt32(random_number, 0));
  
                return (buffer % offset) + min_value;
            }

            public uint GenerateUint(uint max_value)
            {
                return GenerateUint(0, max_value);
            }

            public uint GenerateUint(uint min_value, uint max_value)
            {
                // add one to max to include the max value endpoint.
                max_value++;

                if (min_value > max_value)
                {
                    min_value = min_value ^ max_value; 
                    max_value = min_value ^ max_value;
                    min_value = min_value ^ max_value;
                }

                uint offset = max_value - min_value;  

                byte[] random_number = new byte[4];
                _Random.GetBytes(random_number);

                uint buffer = BitConverter.ToUInt32(random_number, 0);
  
                return (buffer % offset) + min_value;
            }
        
            public string GeneratePassword()
            {
                byte[] buffer = new byte[_PasswordLength];
                _Random.GetBytes(buffer);

                return Convert.ToBase64String(buffer);
            }

        #endregion
    }
}
