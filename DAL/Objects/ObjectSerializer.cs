using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace DAL
{
    public static class ObjectSerializer
    {
        /// <summary>
        /// Converts an object in memory into a binary string.
        /// </summary>
        public static string SerializeToBinaryString(object input)
        {
            if (input == null)
                throw new ArgumentException("Cannot serialize a null object");

            using (MemoryStream stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, input);

                return Encoding.Default.GetString(stream.ToArray());
            }
        }

        /// <summary>
        /// Deserializes object from a string.
        /// </summary>
        public static T DeserializeFromBinaryString<T>(string input) where T : class
        {
            if (input == null)
                throw new ArgumentException("Cannot serialize a null object");

            using (MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(input)))
            {
                IFormatter formatter = new BinaryFormatter();
                return (T)formatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// Converts an object in memory into a binary string.
        /// </summary>
        public static byte[] SerializeToBinaryByteArray(object input)
        {
            if (input == null)
                throw new ArgumentException("Cannot serialize a null object");

            using (MemoryStream stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, input);

                return stream.ToArray();
            }
        }

        /// <summary>
        /// Deserializes binary object from a string.
        /// </summary>
        public static T DeserializeFromBinaryByteArray<T>(byte[] input) where T : class
        {
            if (input == null)
                throw new ArgumentException("Cannot serialize a null object");

            using (MemoryStream stream = new MemoryStream(input))
            {
                IFormatter formatter = new BinaryFormatter();
                return (T)formatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// Converts an object in memory into a XML string using XML formatter. Note
        /// that there are some serious limitations on what can be serialized using this 
        /// method.
        /// </summary>
        public static string SerializeToXMLString<T>(object input)
        {
            if (input == null)
                throw new ArgumentException("Cannot serialize a null object");

            using (MemoryStream stream = new MemoryStream())
            {
                XmlSerializer formatter = new XmlSerializer(typeof(T));
                formatter.Serialize(stream, input);

                return Encoding.Default.GetString(stream.ToArray());
            }
        }        

        /// <summary>
        /// Deserializes object from a XML string using XML formatter. Note
        /// that there are some serious limitations on what can be serialized using this 
        /// method.
        /// </summary>
        public static T DeserializeXMLString<T>(string input) where T : class
        {
            if (input == null)
                throw new ArgumentException("Cannot serialize a null object");

            using (MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(input)))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(T));
                return (T)formatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// Converts an object into a Json string.
        /// </summary>
        public static string SerializeToJsonString(object input)
        {
            if (input == null)
                throw new ArgumentException("Cannot serialize a null object");

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(input);
        }        

        /// <summary>
        /// Deserializes object from a JSON string.
        /// </summary>
        public static T DeserializeJsonString<T>(string input) where T : class
        {
            if (input == null)
                throw new ArgumentException("Cannot serialize a null object");

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Deserialize<T>(input);
        }

        /// <summary>
        /// Encodes a string in base64. 
        /// </summary>
        public static string Base64Encode(string input)
        {
            if (input == null)
                throw new ArgumentException("Cannot serialize a null object");

            byte[] byte_string = new byte[input.Length];
            byte_string = Encoding.Default.GetBytes(input);  
            return Convert.ToBase64String(byte_string);
        }

        /// <summary>
        /// Decodes a base64 string. 
        /// </summary>
        public static string Base64Decode(string input)
        {
            if (input == null)
                throw new ArgumentException("Cannot serialize a null object");

            byte[] data = Convert.FromBase64String(input);
            return Encoding.Default.GetString(data);
        }

        /// <summary>
        /// Converts a string into a byte array. Assumes UTF8 compatable input.
        /// </summary>
        public static byte[] StringToByteArray(string input)
        {
            if (input == null)
                throw new ArgumentException("Cannot serialize a null object");

            return Encoding.Default.GetBytes(input);
        } 

        /// <summary>
        /// Converts a UTF8 byte array into a string.
        /// </summary>
        public static string ByteArrayToString(byte[] input)
        {
            if (input == null)
                throw new ArgumentException("Cannot serialize a null object");

            return Encoding.Default.GetString(input);
        }
    }
}
