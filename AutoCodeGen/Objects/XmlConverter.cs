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
using System.Data;
using System.IO;

namespace AutoCodeGenLibrary
{
    public static partial class XmlConverter
    {
        /// <summary>
        /// Creates a DataSet from a Xml string
        /// </summary>
        /// <param name="Xml_doc">string containing Xml source data</param>
        /// <returns>DataSet containing data</returns>
        public static DataSet XmlStringToDataSet(string Xml_doc)
        {
            if (string.IsNullOrEmpty(Xml_doc))
                throw new ArgumentException("Cannot serialize a null object");

            DataSet ds = new DataSet();

            StringReader string_reader = new StringReader(Xml_doc);
            ds.ReadXml(string_reader);

            return ds;
        }

        /// <summary>
        /// Creates an Xml string from a DataSet
        /// </summary>
        /// <param name="dt">DataSet containing source data</param>
        /// <returns>string containing Xml doc</returns>
        public static string DataSetToXmlString(DataSet data_set)
        {
            return DataSetToXmlString(data_set, string.Empty);
        }

        /// <summary>
        /// Creates an Xml string from a DataSet
        /// </summary>
        /// <param name="dt">DataSet containing source data</param>
        /// <param name="xml_namespace">string Xml namespace for Xml doc</param>
        /// <returns>string containing Xml doc</returns>
        public static string DataSetToXmlString(DataSet data_set, string xml_namespace)
        {
            data_set.Namespace = xml_namespace;

            return data_set.GetXml();
        }

        /// <summary>
        /// Creates an Xml string from a DataTable
        /// </summary>
        /// <param name="dt">DataTable containing source data</param>
        /// <returns>string containing Xml doc</returns>
        public static string DataTableToXmlString(DataTable data_table)
        {
            return DataTableToXmlString(data_table, string.Empty);
        }

        /// <summary>
        /// Creates an Xml string from a DataTable
        /// </summary>
        /// <param name="dt">DataTable containing source data</param>
        /// <param name="xml_namespace">string Xml namespace for Xml doc</param>
        /// <returns>string containing Xml doc</returns>
        public static string DataTableToXmlString(DataTable data_table, string xml_namespace)
        {
            DataSet data_set = new DataSet();
            data_set.Tables.Add(data_table);

            // parent node for table is created with name of DataSet
            data_set.DataSetName = "TableData";

            return DataSetToXmlString(data_set, xml_namespace);
        }

        /// <summary>
        /// Creates a Xml node from a string.
        /// </summary>
        /// <param name="node_name">string name of node</param>
        /// <param name="node_data">string data contained by node</param>
        /// <returns></returns>
        public static string CreateXmlNode(string node_name, string node_data)
        {
            // Fix characters in node_name
            if (node_name.Contains(" ")) node_name = node_name.Replace(" ", string.Empty);  // remove whitespace
            if (node_name.Contains("\\")) node_name = node_name.Replace("\\", string.Empty); // remove \
            if (node_name.Contains("/")) node_name = node_name.Replace("/", string.Empty);  // remove /
            if (node_name.Contains("'")) node_name = node_name.Replace("'", string.Empty);  // remove '
            if (node_name.Contains("\"")) node_name = node_name.Replace("\"", string.Empty); // remove "
            if (node_name.Contains("[")) node_name = node_name.Replace("[", string.Empty);  // remove [
            if (node_name.Contains("]")) node_name = node_name.Replace("]", string.Empty);  // remove ]
            if (node_name.Contains("&")) node_name = node_name.Replace("&", string.Empty);  // remove &
            if (node_name.Contains("<")) node_name = node_name.Replace("<", string.Empty);  // remove <
            if (node_name.Contains(">")) node_name = node_name.Replace(">", string.Empty);  // remove >

            //if (node_data.Contains("&"))    node_data = node_data.Replace(">","&amp;");     // replace &
            //if (node_data.Contains("<"))    node_data = node_data.Replace("<","&lt;");      // replace <
            //if (node_data.Contains(">"))    node_data = node_data.Replace(">","&gt;");      // replace >
            //if (node_data.Contains("\""))   node_data = node_data.Replace("\"","&quot;");   // replace "
            //if (node_data.Contains("'"))    node_data = node_data.Replace("'","&apos;");    // replace '

            return $"<{node_name}>{node_data}</{node_name}>";
        }
    }
}
