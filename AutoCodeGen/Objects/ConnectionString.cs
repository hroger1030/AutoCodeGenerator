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

using System.Collections.Generic;
using System.Text;

namespace AutoCodeGen
{
    public class ConnectionString
    {
        // connection string options
        //http://msdn2.microsoft.com/en-us/library/system.data.sqlclient.SQLConn.connectionstring(VS.71).aspx

        protected string _Name;
        protected Dictionary<string, string> _Parameters;

        /// <summary>
        /// Creates generic local connection
        /// </summary>
        public ConnectionString()
        {
            _Name = "SQLConnection";
            _Parameters = new Dictionary<string, string>();

            _Parameters.Add("Data Source", ".");
            _Parameters.Add("Initial Catalog", "Master");
        }

        public ConnectionString(string Name)
        {
            _Name = Name;
            _Parameters = new Dictionary<string, string>();
        }

        public void AddParameter(string ParameterName, string ParameterValue)
        {
            if (_Parameters.ContainsKey(ParameterName))
                _Parameters[ParameterName] = ParameterValue;
            else
                _Parameters.Add(ParameterName, ParameterValue);
        }

        public void RemoveParameter(string ParameterName)
        {
            if (_Parameters.ContainsKey(ParameterName))
                _Parameters.Remove(ParameterName);
        }

        /// <summary>
        /// Creates a connection string with out any extra markup.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();

            bool first_KVP_flag = true;

            foreach (var kvp in _Parameters)
            {
                if (first_KVP_flag == false)
                    sb.Append(";");
                else
                    first_KVP_flag = false;

                sb.Append(kvp.Key + "=" + kvp.Value);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates a connection string that is formatted for use in a web.config file.
        /// </summary>
        public string ToConfigString()
        {
            var sb = new StringBuilder();

            sb.Append("<add name=\"" + _Name + "\" ");
            sb.Append("connectionString=\"");

            foreach (KeyValuePair<string, string> kvp in _Parameters)
                sb.Append(kvp.Key + "=" + kvp.Value + ";");

            sb.Append("\" />");

            return sb.ToString();
        }
    }
}
