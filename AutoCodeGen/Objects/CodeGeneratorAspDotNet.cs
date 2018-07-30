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
using System.Configuration;
using System.Text;

using DAL.SqlMetadata;

namespace AutoCodeGenLibrary
{
    public class CodeGeneratorAspDotNet : CodeGeneratorBase, IGenerator
    {
        public static readonly string CREATE_AS_ASP_CONTROL = "Create As Asp Control";

        public eLanguage Language
        {
            get { return eLanguage.Csharp; }
        }
        public eCategory Category
        {
            get { return eCategory.WebApp; }
        }
        public Dictionary<string, string> Methods
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    { "CSS Sheet", "GenerateCSSSheet" },
                    { "Web Config", "GenerateWebConfig" },
                    { "Master Page Code In Front", "GenerateMasterPageCodeInFront" },
                    { "Master Page Code Behind", "GenerateMasterPageCodeBehind" },
                    { "Code Generator Asp", "CodeGeneratorAsp" },
                    { "Web Edit Page Code In Front", "GenerateWebEditPageCodeInFront" },
                    { "Web Edit Page Code Behind", "GenerateWebEditPageCodeBehind" },
                    { "Web List Page Code In Front", "GenerateWebListPageCodeInFront" },
                    { "Web List Page Code Behind", "GenerateWebListPageCodeBehind" },
                    { "Default Page Code In Front", "GenerateDefaultPageCodeInFront" },
                    { "Default Page Code Behind", "GenerateDefaultPageCodeBehind" },
                    { "Web View Page Code In Front", "GenerateWebViewPageCodeInFront" },
                };
            }
        }
        public Dictionary<string, bool> Options
        {
            get
            {
                return new Dictionary<string, bool>()
                {
                    { CREATE_AS_ASP_CONTROL, false},
                };
            }
        }

        public CodeGeneratorAspDotNet() { }

        public OutputObject GenerateCSSSheet(string databaseName)
        {
            if (string.IsNullOrEmpty(databaseName))
                return null;

            OutputObject output = new OutputObject();
            output.Name = ConfigurationManager.AppSettings["DefaultCSSFilename"];
            output.Type = OutputObject.eObjectType.Css;

            string spacer = "/*******************************************************************************/";

            var sb = new StringBuilder();

            sb.AppendLine("/*");
            sb.AppendLine(AddTabs(1) + GenerateAuthorNotice());
            sb.AppendLine("*/");

            sb.AppendLine();

            //////////////////////////////////////////////////////////////////////////////
            sb.AppendLine(spacer);
            sb.AppendLine();

            sb.AppendLine("#header");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "padding: 10px;");
            sb.AppendLine(AddTabs(1) + "background-color: #00ff00;");
            sb.AppendLine(AddTabs(1) + "border-bottom: solid 2px black;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("#menu");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "float: left;");
            sb.AppendLine(AddTabs(1) + "padding-top: 10px;");
            sb.AppendLine(AddTabs(1) + "padding-left: 10px;");
            sb.AppendLine(AddTabs(1) + "padding-right: 50px;");
            sb.AppendLine(AddTabs(1) + "padding-bottom: 200px;");
            sb.AppendLine(AddTabs(1) + "background-color: #ffff00;");
            sb.AppendLine(AddTabs(1) + "border-bottom: solid 2px black;");
            sb.AppendLine(AddTabs(1) + "border-right: solid 2px black;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("#contents");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "float: left;");
            sb.AppendLine(AddTabs(1) + "padding: 10px;");
            sb.AppendLine(AddTabs(1) + "background-color: white;");
            sb.AppendLine("}");
            sb.AppendLine();

            //////////////////////////////////////////////////////////////////////////////
            sb.AppendLine(spacer);
            sb.AppendLine();

            sb.AppendLine("HTML");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "height: 100%;");
            sb.AppendLine(AddTabs(1) + "padding: 0px;");
            sb.AppendLine(AddTabs(1) + "border: 0px;");
            sb.AppendLine(AddTabs(1) + "margin: 0px;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("Body");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "height: 100%;");
            sb.AppendLine(AddTabs(1) + "padding: 0px;");
            sb.AppendLine(AddTabs(1) + "border: 0px;");
            sb.AppendLine(AddTabs(1) + "margin: 0px;");
            sb.AppendLine(AddTabs(1) + "font-family: Arial, Sans-Serif;");
            sb.AppendLine(AddTabs(1) + "font-size: 12px;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("Table");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "border-collapse: collapse;");
            sb.AppendLine(AddTabs(1) + "empty-cells: show;");
            sb.AppendLine(AddTabs(1) + "font-family: arial;");
            sb.AppendLine(AddTabs(1) + "font-size: inherit;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("TD");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "margin: 0px;");
            sb.AppendLine(AddTabs(1) + "vertical-align: middle;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("Input");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "font-family: arial;");
            sb.AppendLine(AddTabs(1) + "font-size: inherit;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("TextArea");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "font-family: arial;");
            sb.AppendLine(AddTabs(1) + "font-size: inherit;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("CheckBox");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "font-family: arial;");
            sb.AppendLine(AddTabs(1) + "font-size: inherit;");
            sb.AppendLine("}");
            sb.AppendLine();

            //////////////////////////////////////////////////////////////////////////////
            sb.AppendLine(spacer);
            sb.AppendLine();

            sb.AppendLine(".PageTitle");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "font-size: 14px;");
            sb.AppendLine(AddTabs(1) + "font-weight: bold;");
            sb.AppendLine(AddTabs(1) + "padding-right: 10px;");
            sb.AppendLine(AddTabs(1) + "padding-top: 10px;");
            sb.AppendLine(AddTabs(1) + "padding-bottom: 10px;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine(".SectionTitle");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "font-size: 12px;");
            sb.AppendLine(AddTabs(1) + "font-weight: bold;");
            sb.AppendLine(AddTabs(1) + "padding-bottom: 10px;");
            sb.AppendLine(AddTabs(1) + "padding-top: 10px;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine(".ControlLabel");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "font-weight: bold;");
            sb.AppendLine(AddTabs(1) + "font-size: 10pt;");
            sb.AppendLine(AddTabs(1) + "padding-top: 4px;");
            sb.AppendLine(AddTabs(1) + "padding-bottom: 4px;");
            sb.AppendLine(AddTabs(1) + "padding-right: 4px;");
            sb.AppendLine(AddTabs(1) + "text-align: right;");
            sb.AppendLine(AddTabs(1) + "vertical-align:top;");
            sb.AppendLine("}");
            sb.AppendLine();

            //////////////////////////////////////////////////////////////////////////////
            sb.AppendLine(spacer);
            sb.AppendLine();

            sb.AppendLine(".RepeaterStyle");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "border-style: solid;");
            sb.AppendLine(AddTabs(1) + "border-width: 2px;");
            sb.AppendLine(AddTabs(1) + "background-color: #eeeeee;");
            sb.AppendLine(AddTabs(1) + "height: 400px;");
            sb.AppendLine(AddTabs(1) + "width: 800px;");
            sb.AppendLine(AddTabs(1) + "overflow: auto;");
            sb.AppendLine("}");
            sb.AppendLine("@media print");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + ".RepeaterStyle");
            sb.AppendLine(AddTabs(1) + "{");
            sb.AppendLine(AddTabs(2) + "overflow: visible;");
            sb.AppendLine(AddTabs(1) + "}");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine(".RepeaterHeader");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "font-weight: bold;");
            sb.AppendLine(AddTabs(1) + "font-size: 10pt;");
            sb.AppendLine(AddTabs(1) + "padding-bottom: 2px;");
            sb.AppendLine(AddTabs(1) + "padding-top: 4px;");
            sb.AppendLine(AddTabs(1) + "padding-right: 2px;");
            sb.AppendLine(AddTabs(1) + "padding-left: 4px;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine(".RepeaterCell");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "padding-bottom: 2px;");
            sb.AppendLine(AddTabs(1) + "padding-top: 4px;");
            sb.AppendLine(AddTabs(1) + "padding-right: 2px;");
            sb.AppendLine(AddTabs(1) + "padding-left: 4px;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine(".RepeaterHeaderStyle");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "border-style: solid;");
            sb.AppendLine(AddTabs(1) + "border-width: 2px 2px 0px 2px;");
            sb.AppendLine(AddTabs(1) + "background-color: #ccccff;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine(".RepeaterItemStyle");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "background-color: #ffffff;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine(".AlternatingRepeaterItemStyle");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "background-color: #ffffcc;");
            sb.AppendLine("}");
            sb.AppendLine();

            //////////////////////////////////////////////////////////////////////////////
            sb.AppendLine(spacer);
            sb.AppendLine();

            sb.AppendLine(".DetailsPanel");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "border-style: solid;");
            sb.AppendLine(AddTabs(1) + "border-width: 2px;");
            sb.AppendLine(AddTabs(1) + "background-color: #eeeeee;");
            sb.AppendLine(AddTabs(1) + "width: 600px;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine(".Spacer");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "padding: 10px;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine(".AddItemLink");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "padding-top: 10px;");
            sb.AppendLine(AddTabs(1) + "padding-right: 10px;");
            sb.AppendLine(AddTabs(1) + "padding-left: 10px;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine(".ErrorMessage");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "font-weight: bold;");
            sb.AppendLine(AddTabs(1) + "font-size: inherit;");
            sb.AppendLine(AddTabs(1) + "color: #cc0000;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine(".button1");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "border: #000000 1px solid;");
            sb.AppendLine(AddTabs(1) + "font-size: 12px;");
            sb.AppendLine(AddTabs(1) + "cursor: hand;");
            sb.AppendLine(AddTabs(1) + "color: #000000;");
            sb.AppendLine(AddTabs(1) + "background-color: #ccccff;");
            sb.AppendLine("}");
            sb.AppendLine();

            //////////////////////////////////////////////////////////////////////////////
            sb.AppendLine(spacer);
            sb.AppendLine();

            output.Body = sb.ToString();
            return output;
        }

        public OutputObject GenerateWebConfig(string connectionString)
        {
            if (connectionString == null)
                return null;

            var output = new OutputObject();
            output.Name = ConfigurationManager.AppSettings["DefaultWebConfigFilename"];
            output.Type = OutputObject.eObjectType.WebConfig;

            var sb = new StringBuilder();

            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            sb.AppendLine("<configuration>");
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "<appSettings>");
            sb.AppendLine(AddTabs(1) + "</appSettings>");
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "<connectionStrings>");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + connectionString);
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "</connectionStrings>");
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "<system.web>");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "<compilation debug=\"true\" />");
            sb.AppendLine(AddTabs(2) + "<authentication mode=\"Windows\" />");
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "</system.web>");
            sb.AppendLine();
            sb.AppendLine("</configuration>");

            output.Body = sb.ToString();
            return output;
        }

        public OutputObject GenerateMasterPageCodeInFront(string databaseName, List<string> selectedTables)
        {
            if (string.IsNullOrEmpty(databaseName))
                return null;

            var output = new OutputObject();
            output.Name = ConfigurationManager.AppSettings["DefaultMasterPageFilename"];
            output.Type = OutputObject.eObjectType.CSharp;

            var sb = new StringBuilder();

            sb.AppendLine("<%@ Master Language=\"C#\" AutoEventWireup=\"true\" CodeFile=\"" + ConfigurationManager.AppSettings["DefaultMasterPageFilename"] + ".cs\" Inherits=\"WebControls." + NameFormatter.ToCSharpPropertyName(databaseName) + ".MasterPage\" %>");
            sb.AppendLine();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine();

            sb.AppendLine("<html>");
            sb.AppendLine("<head runat=\"server\">");
            sb.AppendLine(AddTabs(1) + $"<title>{NameFormatter.ToFriendlyName(databaseName)}</title>");
            sb.AppendLine(AddTabs(1) + "<link href=\"" + ConfigurationManager.AppSettings["DefaultCSSFilename"] + ".css\" rel=\"stylesheet\" type=\"text/css\" />");
            sb.AppendLine(AddTabs(1) + "<asp:contentplaceholder id=\"head\" runat=\"server\">");
            sb.AppendLine(AddTabs(1) + "</asp:contentplaceholder>");
            sb.AppendLine("</head>");
            sb.AppendLine();

            sb.AppendLine("<body>");
            sb.AppendLine(AddTabs(1) + "<form id=\"form1\" runat=\"server\">");
            sb.AppendLine();

            sb.AppendLine(AddTabs(1) + "<div id=\"header\">");
            sb.AppendLine(AddTabs(2) + "<div class=\"PageTitle\">Database " + NameFormatter.ToFriendlyName(databaseName) + " Tools</div>");
            sb.AppendLine(AddTabs(2) + "<div>" + GenerateAuthorNotice() + "</div>");
            sb.AppendLine(AddTabs(1) + "</div>");
            sb.AppendLine();

            sb.AppendLine(AddTabs(1) + "<div id=\"menu\">");

            sb.AppendLine(AddTabs(1) + "<div class=\"SectionTitle\">Tables</div>");
            sb.AppendLine(AddTabs(1) + "<ul>");

            for (int i = 0; i < selectedTables.Count; i++)
            {
                // loop through table links here
                sb.AppendLine(AddTabs(2) + "<li><asp:HyperLink ID=\"HL" + (i + 1).ToString() + "\" NavigateUrl=\"List" + selectedTables[i] + ".aspx\" Text=\"" + selectedTables[i] + "\" runat=\"server\" /></li>");
            }

            sb.AppendLine(AddTabs(1) + "</ul>");
            sb.AppendLine(AddTabs(1) + "</div>");
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "<div id=\"contents\">");
            sb.AppendLine(AddTabs(2) + "<asp:contentplaceholder id=\"body\" runat=\"server\">");
            sb.AppendLine(AddTabs(2) + "</asp:contentplaceholder>");
            sb.AppendLine(AddTabs(1) + "</div>");
            sb.AppendLine();

            sb.AppendLine(AddTabs(1) + "</form>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            output.Body = sb.ToString();
            return output;
        }

        public OutputObject GenerateMasterPageCodeBehind(string databaseName)
        {
            if (string.IsNullOrEmpty(databaseName))
                return null;

            OutputObject output = new OutputObject();
            output.Name = ConfigurationManager.AppSettings["DefaultMasterPageFilename"] + ".cs";
            output.Type = OutputObject.eObjectType.CSharp;

            var sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Configuration;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Web;");
            sb.AppendLine("using System.Web.Security;");
            sb.AppendLine("using System.Web.UI;");
            sb.AppendLine("using System.Web.UI.WebControls;");
            sb.AppendLine("using System.Web.UI.WebControls.WebParts;");
            sb.AppendLine("using System.Web.UI.HtmlControls;");
            sb.AppendLine();

            sb.AppendLine("namespace WebControls." + NameFormatter.ToCSharpPropertyName(databaseName) + "MasterPage");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "public partial class MasterPage : System.Web.UI.MasterPage");
            sb.AppendLine(AddTabs(1) + "{");
            sb.AppendLine(AddTabs(2) + "protected void Page_Load(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(2) + "{");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "}");
            sb.AppendLine(AddTabs(1) + "}");
            sb.AppendLine("}");

            output.Body = sb.ToString();
            return output;
        }

        public OutputObject CodeGeneratorAsp(SqlTable sqlTable, List<string> namespaceIncludes)
        {
            if (sqlTable == null)
                return null;

            string view_class_name = "View" + NameFormatter.ToCSharpPropertyName(sqlTable.Name);
            int longest_column = GetLongestColumnLength(sqlTable) + sqlTable.Name.Length;

            OutputObject output = new OutputObject();
            output.Name = view_class_name + ".aspx.cs";
            output.Type = OutputObject.eObjectType.CSharp;

            namespaceIncludes.Add(NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name));

            var sb = new StringBuilder();

            #region Includes
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Configuration;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Web;");
            sb.AppendLine("using System.Web.Security;");
            sb.AppendLine("using System.Web.UI;");
            sb.AppendLine("using System.Web.UI.WebControls;");
            sb.AppendLine("using System.Web.UI.WebControls.WebParts;");
            sb.AppendLine("using System.Web.UI.HtmlControls;");
            sb.AppendLine();

            sb.AppendLine(GenerateNamespaceIncludes(namespaceIncludes));
            sb.AppendLine();

            #endregion

            sb.AppendLine("namespace WebControls." + NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name));
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "public partial class " + view_class_name + " : System.Web.UI.Page");
            sb.AppendLine(AddTabs(1) + "{");

            sb.AppendLine(AddTabs(2) + "#region Fields");
            sb.AppendLine();

            #region Fields
            sb.AppendLine(AddTabs(3) + "protected string _SQLConnection = ConfigurationManager.ConnectionStrings[\"SQLConnection\"].ConnectionString;");
            #endregion

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#region Properties");
            sb.AppendLine();

            #region Properties
            sb.AppendLine(AddTabs(3) + "protected string PageName");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get { return \"" + view_class_name + "\"; }");
            sb.AppendLine(AddTabs(3) + "}");

            sb.AppendLine(AddTabs(3) + "protected PaginationManager PageinationData");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (ViewState[\"PaginationManager\"] == null)");
            sb.AppendLine(AddTabs(6) + "ViewState[\"PaginationManager\"] = new PaginationManager();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "return (PaginationManager)ViewState[\"PaginationManager\"];");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "set");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "ViewState[\"PaginationManager\"] = value;");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");

            sb.AppendLine(AddTabs(3) + "protected string ReturnPage");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (ViewState[\"ReturnPage\"] == null)");
            sb.AppendLine(AddTabs(6) + "ViewState[\"ReturnPage\"] = \"~/Default.aspx\";");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "return (string)ViewState[\"ReturnPage\"];");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "set");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "ViewState[\"ReturnPage\"] = value;");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");

            sb.AppendLine(AddTabs(3) + "protected int EditingID");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (ViewState[\"EditingID\"] == null)");
            sb.AppendLine(AddTabs(6) + "ViewState[\"EditingID\"] = 0;");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "return (int)ViewState[\"EditingID\"];");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "set");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "ViewState[\"EditingID\"] = value;");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#region Methods");
            sb.AppendLine();

            #region BindForm Method
            sb.AppendLine(AddTabs(3) + "protected void BindForm()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + NameFormatter.ToCSharpClassName("DAL" + sqlTable.Name) + " dal_obj = new " + NameFormatter.ToCSharpClassName("DAL" + sqlTable.Name) + "(_SQLConnection);");
            sb.AppendLine(AddTabs(4) + "dal_obj.LoadSingleFromDb(this.EditingID);");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "if (dal_obj.Collection.Count > 0)");
            sb.AppendLine(AddTabs(4) + "{");

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (!sql_column.IsPk)
                {
                    // need to account for datatypes that need to be converted to a string.

                    switch (sql_column.BaseType)
                    {
                        case eSqlBaseType.Bool:
                            sb.AppendLine(AddTabs(5) + PadCSharpVariableName("chk" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Checked", longest_column, 11) + "= dal_obj.Collection[0]." + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ";");
                            break;

                        case eSqlBaseType.Guid:
                            sb.AppendLine(AddTabs(5) + PadCSharpVariableName("lbl" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text", longest_column, 11) + "= dal_obj.Collection[0]." + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".ToString();");
                            break;

                        case eSqlBaseType.Integer:
                            sb.AppendLine(AddTabs(5) + PadCSharpVariableName("lbl" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text", longest_column, 11) + "= dal_obj.Collection[0]." + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".ToString();");
                            break;

                        case eSqlBaseType.Float:
                            sb.AppendLine(AddTabs(5) + PadCSharpVariableName("lbl" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text", longest_column, 11) + "= dal_obj.Collection[0]." + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".ToString();");
                            break;

                        case eSqlBaseType.Time:
                            sb.AppendLine(AddTabs(5) + PadCSharpVariableName("lbl" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text", longest_column, 11) + "= dal_obj.Collection[0]." + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".ToString();");
                            break;

                        default:
                            sb.AppendLine(AddTabs(5) + PadCSharpVariableName("lbl" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text", longest_column, 11) + "= dal_obj.Collection[0]." + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ";");
                            break;
                    }
                }
            }
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "else");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "this.lblErrorMessage.Text = \"Load Failed\";");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region Check Credentials method
            sb.AppendLine(AddTabs(3) + "protected void CheckCredentials()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#region Events");
            sb.AppendLine();

            #region pageload event method
            sb.AppendLine(AddTabs(3) + "protected void Page_Load(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "CheckCredentials();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "if (!Page.IsPostBack)");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (Request.QueryString[\"Id\"] != null)");
            sb.AppendLine(AddTabs(6) + "this.EditingID = Convert.ToInt32(Request.QueryString[\"Id\"]);");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "if (Request.QueryString[\"ReturnPage\"] != null)");
            sb.AppendLine(AddTabs(6) + "this.ReturnPage = (string)Request.QueryString[\"ReturnPage\"];");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "if (Request.QueryString[\"PageData\"] != null)");
            sb.AppendLine(AddTabs(6) + "this.PageinationData = new PaginationManager(Request.QueryString[\"PageData\"].ToString());");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "BindForm();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");

            sb.AppendLine(AddTabs(1) + "}");
            sb.AppendLine("}");

            output.Body = sb.ToString();
            return output;
        }

        public OutputObject GenerateWebEditPageCodeInFront(SqlTable sqlTable, bool createAsAspControl)
        {
            if (sqlTable == null)
                return null;

            string list_class_name = "List" + NameFormatter.ToCSharpPropertyName(sqlTable.Name);
            string edit_class_name = "Edit" + NameFormatter.ToCSharpPropertyName(sqlTable.Name);

            OutputObject output = new OutputObject();
            output.Name = edit_class_name + ".aspx";
            output.Type = OutputObject.eObjectType.Aspx;

            var sb = new StringBuilder();

            if (createAsAspControl)
            {
                sb.AppendLine("<%@ Control Language=\"C#\" AutoEventWireup=\"true\" CodeBehind=\"" + edit_class_name + ".aspx.cs\" Inherits=\"WebControls." + NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name) + "." + edit_class_name + "\" %>");
            }
            else
            {
                sb.AppendLine("<%@ Page Language=\"C#\" MasterPageFile=\"~/MasterPage.master\" AutoEventWireup=\"True\" CodeBehind=\"" + edit_class_name + ".aspx.cs\" Inherits=\"WebControls." + NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name) + "." + edit_class_name + "\" %>");
                sb.AppendLine();
                sb.AppendLine("<asp:Content ID=\"Content1\" ContentPlaceHolderID=\"head\" Runat=\"Server\">");
                sb.AppendLine("</asp:Content>");
                sb.AppendLine();
                sb.AppendLine("<asp:Content ID=\"Content2\" ContentPlaceHolderID=\"body\" Runat=\"Server\">");
            }

            sb.AppendLine();
            sb.AppendLine("<div class=\"PageTitle\">" + NameFormatter.ToFriendlyName(sqlTable.Name) + "</div>");
            sb.AppendLine();

            sb.AppendLine("<asp:Panel CssClass=\"DetailsPanel\" ID=\"panDetails\" runat=\"server\">");
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "<div class=\"Spacer\">");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + "<table border=\"0\" width=\"100%\">");
            sb.AppendLine(AddTabs(2) + "<tr>");
            sb.AppendLine(AddTabs(3) + "<td></td>");
            sb.AppendLine(AddTabs(3) + "<td class=\"SectionTitle\"><asp:Label ID=\"lblTitle\" runat=\"server\" /></td>");
            sb.AppendLine(AddTabs(2) + "</tr>");
            sb.AppendLine();

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (!sql_column.IsPk)
                {
                    sb.AppendLine(AddTabs(2) + "<tr>");
                    sb.AppendLine(AddTabs(3) + "<td class=\"ControlLabel\">" + NameFormatter.ToFriendlyName(sql_column.Name) + ":</td>");

                    switch (sql_column.BaseType)
                    {
                        case eSqlBaseType.Bool:
                            sb.AppendLine(AddTabs(3) + "<td><asp:CheckBox ID=\"chk" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\" runat=\"server\" /></td>");
                            break;

                        case eSqlBaseType.Time:
                            sb.AppendLine(AddTabs(3) + "<td><asp:TextBox ID=\"txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\" runat=\"server\" /></td>");
                            break;

                        default:
                            if (sql_column.BaseType == eSqlBaseType.String)
                            {
                                if (sql_column.Length == -1)
                                {
                                    sb.AppendLine(AddTabs(3) + "<td><asp:TextBox ID=\"txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\" runat=\"server\" TextMode=\"MultiLine\" Rows=\"8\" Width=\"400\" /></td>");
                                }
                                else if (sql_column.Length > 100)
                                {
                                    sb.AppendLine(AddTabs(3) + "<td><asp:TextBox ID=\"txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\" runat=\"server\" Width=\"400\" Rows=\"" + (sql_column.Length / 100).ToString() + "\" TextMode=\"MultiLine\" /></td>");
                                }
                                else
                                {
                                    sb.AppendLine(AddTabs(3) + "<td><asp:TextBox ID=\"txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\" runat=\"server\" Width=\"400\" MaxLength=\"" + sql_column.Length.ToString() + "\" /></td>");
                                }
                            }
                            else
                            {
                                //This might need to have the max length fixed. ints and stuff can end up here.
                                sb.AppendLine(AddTabs(3) + "<td><asp:TextBox ID=\"txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\" runat=\"server\" Width=\"400\" MaxLength=\"" + sql_column.Length.ToString() + "\" /></td>");
                            }
                            break;
                    }

                    sb.AppendLine(AddTabs(2) + "</tr>");
                    sb.AppendLine();
                }
            }

            // buttons
            sb.AppendLine(AddTabs(2) + "<tr>");
            sb.AppendLine(AddTabs(3) + "<td></td>");
            sb.AppendLine(AddTabs(3) + "<td class=\"SectionTitle\">");
            sb.AppendLine(AddTabs(4) + "<asp:Button ID=\"btnSave\" runat=\"server\" CssClass=\"Button1\" Text=\"Save\" OnClick=\"btnSave_Click\" />");
            sb.AppendLine(AddTabs(4) + "<asp:Button ID=\"btnUndo\" runat=\"server\" CssClass=\"Button1\" Text=\"Undo Changes\" OnClick=\"btnUndo_Click\" />");
            sb.AppendLine(AddTabs(3) + "</td>");
            sb.AppendLine(AddTabs(2) + "</tr>");
            sb.AppendLine(AddTabs(2) + "</table>");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "<asp:Label ID=\"lblErrorMessage\" CssClass=\"ErrorMessage\" runat=\"server\" />");
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "</div>");
            sb.AppendLine();
            sb.AppendLine("</asp:Panel>");

            if (!createAsAspControl)
            {
                sb.AppendLine();
                sb.AppendLine("</asp:Content>");
            }

            output.Body = sb.ToString();
            return output;
        }

        public OutputObject GenerateWebEditPageCodeBehind(SqlTable sqlTable, List<string> namespaceIncludes)
        {
            if (sqlTable == null)
                return null;

            string class_name = "Edit" + NameFormatter.ToCSharpPropertyName(sqlTable.Name);
            int longest_column = GetLongestColumnLength(sqlTable) + sqlTable.Name.Length;

            OutputObject output = new OutputObject();
            output.Name = class_name + ".aspx.cs";
            output.Type = OutputObject.eObjectType.CSharp;

            namespaceIncludes.Add(NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name));

            var sb = new StringBuilder();

            #region Includes
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Configuration;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Web;");
            sb.AppendLine("using System.Web.Security;");
            sb.AppendLine("using System.Web.UI;");
            sb.AppendLine("using System.Web.UI.WebControls;");
            sb.AppendLine("using System.Web.UI.WebControls.WebParts;");
            sb.AppendLine("using System.Web.UI.HtmlControls;");
            sb.AppendLine();

            sb.AppendLine(GenerateNamespaceIncludes(namespaceIncludes));
            sb.AppendLine();

            #endregion

            sb.AppendLine("namespace WebControls." + NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name));
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "public partial class " + class_name + " : System.Web.UI.Page");
            sb.AppendLine(AddTabs(1) + "{");

            #region Fields
            sb.AppendLine(AddTabs(2) + "#region Fields");
            sb.AppendLine();
            sb.AppendLine(AddTabs(3) + "protected string _SQLConnection = ConfigurationManager.ConnectionStrings[\"SQLConnection\"].ConnectionString;");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();
            #endregion

            #region Properties
            sb.AppendLine(AddTabs(2) + "#region Properties");
            sb.AppendLine();

            sb.AppendLine(AddTabs(3) + "protected string PageName");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get { return \"" + class_name + "\"; }");
            sb.AppendLine(AddTabs(3) + "}");

            sb.AppendLine(AddTabs(3) + "protected PaginationManager PageinationData");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (ViewState[\"PaginationManager\"] == null)");
            sb.AppendLine(AddTabs(6) + "ViewState[\"PaginationManager\"] = new PaginationManager();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "return (PaginationManager)ViewState[\"PaginationManager\"];");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "set");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "ViewState[\"PaginationManager\"] = value;");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");

            sb.AppendLine(AddTabs(3) + "protected string ReturnPage");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (ViewState[\"ReturnPage\"] == null)");
            sb.AppendLine(AddTabs(6) + "ViewState[\"ReturnPage\"] = \"~/Default.aspx\";");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "return (string)ViewState[\"ReturnPage\"];");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "set");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "ViewState[\"ReturnPage\"] = value;");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");

            sb.AppendLine(AddTabs(3) + "protected int EditingID");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (ViewState[\"EditingID\"] == null)");
            sb.AppendLine(AddTabs(6) + "ViewState[\"EditingID\"] = 0;");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "return (int)ViewState[\"EditingID\"];");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "set");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "ViewState[\"EditingID\"] = value;");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#region Methods");
            sb.AppendLine();
            #endregion

            #region BindForm Method
            sb.AppendLine(AddTabs(3) + "protected void BindForm()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + NameFormatter.ToCSharpClassName("DAL" + sqlTable.Name) + " dal_obj = new " + NameFormatter.ToCSharpClassName("DAL" + sqlTable.Name) + "(_SQLConnection);");
            sb.AppendLine(AddTabs(4) + "dal_obj.LoadSingleFromDb(this.EditingID);");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "if (dal_obj.Collection.Count > 0)");
            sb.AppendLine(AddTabs(4) + "{");

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (!sql_column.IsPk)
                {
                    // need to account for datatypes that need to be converted to a string.

                    switch (sql_column.BaseType)
                    {
                        case eSqlBaseType.Bool:
                            sb.AppendLine(AddTabs(5) + PadCSharpVariableName("chk" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Checked", longest_column, 11) + "= dal_obj.Collection[0]." + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ";");
                            break;

                        case eSqlBaseType.Guid:
                            sb.AppendLine(AddTabs(5) + PadCSharpVariableName("txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text", longest_column, 11) + "= dal_obj.Collection[0]." + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".ToString();");
                            break;

                        case eSqlBaseType.Integer:
                            sb.AppendLine(AddTabs(5) + PadCSharpVariableName("txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text", longest_column, 11) + "= dal_obj.Collection[0]." + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".ToString();");
                            break;

                        case eSqlBaseType.Float:
                            sb.AppendLine(AddTabs(5) + PadCSharpVariableName("txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text", longest_column, 11) + "= dal_obj.Collection[0]." + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".ToString();");
                            break;

                        case eSqlBaseType.Time:
                            sb.AppendLine(AddTabs(5) + PadCSharpVariableName("txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text", longest_column, 11) + "= dal_obj.Collection[0]." + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".ToString();");
                            break;

                        default:
                            sb.AppendLine(AddTabs(5) + PadCSharpVariableName("txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text", longest_column, 11) + "= dal_obj.Collection[0]." + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ";");
                            break;
                    }
                }
            }
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "else");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "this.lblErrorMessage.Text = \"Load Failed\";");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region Check Credentials method
            sb.AppendLine(AddTabs(3) + "protected void CheckCredentials()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region Clear form method
            sb.AppendLine(AddTabs(3) + "protected void ClearForm()");
            sb.AppendLine(AddTabs(3) + "{");

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (!sql_column.IsPk)
                {
                    switch (sql_column.BaseType)
                    {
                        case eSqlBaseType.Bool:
                            sb.AppendLine(AddTabs(4) + PadCSharpVariableName("chk" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Checked", longest_column, 11) + "= false;");
                            break;

                        case eSqlBaseType.Time:
                            sb.AppendLine(AddTabs(4) + PadCSharpVariableName("txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text", longest_column, 11) + "= string.Empty;");
                            break;

                        default:
                            sb.AppendLine(AddTabs(4) + PadCSharpVariableName("txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text", longest_column, 11) + "= string.Empty;");
                            break;
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "lblErrorMessage.Text = string.Empty;");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region UpdateDb Method
            sb.AppendLine(AddTabs(3) + "protected bool UpdateDb()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "try");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + NameFormatter.ToCSharpClassName("DAL" + sqlTable.Name) + " dal_obj = new " + NameFormatter.ToCSharpClassName("DAL" + sqlTable.Name) + "(_SQLConnection);");
            sb.AppendLine(AddTabs(5) + NameFormatter.ToCSharpClassName(sqlTable.Name) + " obj = new " + NameFormatter.ToCSharpClassName(sqlTable.Name) + "();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "if (this.EditingID > 0)");
            sb.AppendLine(AddTabs(5) + "{");
            sb.AppendLine(AddTabs(6) + "dal_obj.LoadSingleFromDb(this.EditingID);");
            sb.AppendLine();
            sb.AppendLine(AddTabs(6) + "if (dal_obj.Collection.Count > 0)");
            sb.AppendLine(AddTabs(7) + "obj = dal_obj.Collection[0];");
            sb.AppendLine(AddTabs(5) + "}");
            sb.AppendLine();

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (!sql_column.IsPk)
                {
                    string csharp_columnname = NameFormatter.ToCSharpPropertyName(sql_column.Name);

                    sb.Append(AddTabs(5) + PadCSharpVariableName("obj." + NameFormatter.ToCSharpPropertyName(sql_column.Name), longest_column, 4));

                    switch (sql_column.BaseType)
                    {
                        case eSqlBaseType.Bool:
                            sb.AppendLine("= chk" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Checked;");
                            break;

                        case eSqlBaseType.Time:
                            sb.AppendLine("= Convert.ToDateTime(txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text);");
                            break;

                        case eSqlBaseType.Integer:
                            sb.AppendLine("= Convert.ToInt32(txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text);");
                            break;

                        case eSqlBaseType.Float:
                            sb.AppendLine("= Convert.ToDouble(txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text);");
                            break;

                        case eSqlBaseType.Guid:
                            sb.AppendLine("= new Guid(txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text);");
                            break;

                        default:
                            sb.AppendLine("= txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text;");
                            break;
                    }
                }
            }

            sb.AppendLine();

            sb.AppendLine(AddTabs(5) + "dal_obj.Save(obj);");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "catch");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "return false;");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "return true;");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region Validation method
            sb.AppendLine(AddTabs(3) + "protected bool ValidateForm()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "return true;");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#region Events");
            sb.AppendLine();

            #region pageload event method
            sb.AppendLine(AddTabs(3) + "protected void Page_Load(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "CheckCredentials();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "this.btnSave.Focus();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "if (!Page.IsPostBack)");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (Request.QueryString[\"Id\"] != null)");
            sb.AppendLine(AddTabs(6) + "this.EditingID = Convert.ToInt32(Request.QueryString[\"Id\"]);");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "if (Request.QueryString[\"ReturnPage\"] != null)");
            sb.AppendLine(AddTabs(6) + "this.ReturnPage = (string)Request.QueryString[\"ReturnPage\"];");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "if (Request.QueryString[\"PageData\"] != null)");
            sb.AppendLine(AddTabs(6) + "this.PageinationData = new PaginationManager(Request.QueryString[\"PageData\"].ToString());");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "if (this.EditingID == 0)");
            sb.AppendLine(AddTabs(5) + "{");
            sb.AppendLine(AddTabs(6) + "this.lblTitle.Text = \"Add New Item\";");
            sb.AppendLine(AddTabs(6) + "ClearForm();");
            sb.AppendLine(AddTabs(5) + "}");
            sb.AppendLine(AddTabs(5) + "else");
            sb.AppendLine(AddTabs(5) + "{");
            sb.AppendLine(AddTabs(6) + "this.lblTitle.Text = \"Edit Item\";");
            sb.AppendLine(AddTabs(6) + "BindForm();");
            sb.AppendLine(AddTabs(5) + "}");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region btnSave_Click event
            sb.AppendLine(AddTabs(3) + "protected void btnSave_Click(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "bool results = ValidateForm();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "if (results)");
            sb.AppendLine(AddTabs(5) + "results = UpdateDb();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "if (results)");
            sb.AppendLine(AddTabs(5) + "Response.Redirect(this.ReturnPage + \".aspx?PageData=\" + this.PageinationData.ToString(), true);");
            sb.AppendLine(AddTabs(4) + "else");
            sb.AppendLine(AddTabs(5) + "lblErrorMessage.Text = \"Save failed\";");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region Undo button
            sb.AppendLine(AddTabs(3) + "protected void btnUndo_Click(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "if (this.EditingID > 0)");
            sb.AppendLine(AddTabs(5) + "BindForm();");
            sb.AppendLine(AddTabs(4) + "else");
            sb.AppendLine(AddTabs(5) + "ClearForm();");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine(AddTabs(1) + "}");
            sb.AppendLine("}");

            output.Body = sb.ToString();
            return output;
        }

        public OutputObject GenerateWebListPageCodeInFront(SqlTable sqlTable, bool createAsAspControl)
        {
            if (sqlTable == null)
                return null;

            string list_object_name = "List" + NameFormatter.ToCSharpPropertyName(sqlTable.Name);
            string edit_object_name = "Edit" + NameFormatter.ToCSharpPropertyName(sqlTable.Name);
            string orm_class_name = NameFormatter.ToCSharpClassName(sqlTable.Name);
            string list_class_name = NameFormatter.ToCSharpClassName(list_object_name);

            OutputObject output = new OutputObject();
            output.Name = list_object_name + ".aspx";
            output.Type = OutputObject.eObjectType.Aspx;

            bool first_flag = true;

            var sb = new StringBuilder();

            if (createAsAspControl)
            {
                sb.AppendLine("<%@ Control Language=\"C#\" AutoEventWireup=\"true\" CodeBehind=\"" + list_object_name + ".aspx.cs\" Inherits=\"WebControls." + NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name) + "." + list_object_name + "\" %>");
            }
            else
            {
                sb.AppendLine("<%@ Page Language=\"C#\" MasterPageFile=\"~/MasterPage.master\" AutoEventWireup=\"true\" CodeBehind=\"" + list_object_name + ".aspx.cs\" Inherits=\"WebControls." + NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name) + "." + list_object_name + "\" %>");
                sb.AppendLine();
                sb.AppendLine("<asp:Content ID=\"Content1\" ContentPlaceHolderID=\"head\" Runat=\"Server\">");
                sb.AppendLine("</asp:Content>");
                sb.AppendLine();
                sb.AppendLine("<asp:Content ID=\"Content2\" ContentPlaceHolderID=\"body\" Runat=\"Server\">");
            }

            sb.AppendLine("<asp:Panel ID=\"panList\" DefaultButton=\"btnSearch\" runat=\"server\">");
            sb.AppendLine();

            sb.AppendLine(AddTabs(1) + "<div class=\"PageTitle\">" + NameFormatter.ToFriendlyName(sqlTable.Name));
            sb.AppendLine(AddTabs(1) + "<asp:LinkButton ID=\"btnAddNewItem\" OnClick=\"btnAddNewItem_Click\" runat=\"server\" Text=\"(Add new item)\" CssClass=\"AddItemLink\" />");
            sb.AppendLine(AddTabs(1) + "</div>");
            sb.AppendLine();

            sb.AppendLine(AddTabs(1) + "<asp:TextBox ID=\"txtSearch\" runat=\"server\" />");
            sb.AppendLine(AddTabs(1) + "<asp:Button ID=\"btnSearch\" runat=\"server\" CssClass=\"Button1\" Text=\"Search\" OnClick=\"btnSearch_Click\" />");
            sb.AppendLine();

            #region Repeater
            sb.AppendLine(AddTabs(1) + "<asp:Repeater ID=\"rptList\" runat=\"server\" >");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + "<HeaderTemplate>");
            sb.AppendLine(AddTabs(2) + "<table width=\"100%\">");
            sb.AppendLine(AddTabs(2) + "<tr class=\"RepeaterHeaderStyle\">");
            sb.AppendLine();

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                sb.AppendLine(AddTabs(3) + "<td><span class=\"RepeaterHeader\">" + NameFormatter.ToFriendlyName(sql_column.Name) + "</span></td>");
            }

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "</tr>");
            sb.AppendLine(AddTabs(2) + "</HeaderTemplate>");
            sb.AppendLine();

            // Item template
            sb.AppendLine(AddTabs(2) + "<ItemTemplate>");
            sb.AppendLine(AddTabs(2) + "<tr class=\"RepeaterItemStyle\">");
            sb.AppendLine();

            first_flag = true;

            foreach (var item in sqlTable.Columns.Values)
            {
                string current_item_binding = "((" + sqlTable.Database.Name + "." + orm_class_name + ")Container.DataItem)." + NameFormatter.ToCSharpPropertyNameString(item);

                if (first_flag)
                {
                    first_flag = false;
                    sb.AppendLine(AddTabs(3) + "<td valign=\"top\"><a href=\"" + edit_object_name + ".aspx?id=<%# " + current_item_binding + " %>&ReturnPage=" + list_object_name + "&PageData=<%=PageinationData.ToString()%>\"><%# " + current_item_binding + " %></a></td>");
                }
                else
                {
                    sb.AppendLine(AddTabs(3) + "<td valign=\"top\"><span class=\"RepeaterCell\"><%# " + current_item_binding + " %></span></td>");
                }
            }

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "</tr>");
            sb.AppendLine(AddTabs(2) + "</ItemTemplate>");
            sb.AppendLine();

            // Alternating item template
            sb.AppendLine(AddTabs(2) + "<AlternatingItemTemplate>");
            sb.AppendLine(AddTabs(2) + "<tr class=\"AlternatingRepeaterItemStyle\">");
            sb.AppendLine();

            first_flag = true;

            foreach (var item in sqlTable.Columns.Values)
            {
                string current_item_binding = "((" + sqlTable.Database.Name + "." + orm_class_name + ")Container.DataItem)." + NameFormatter.ToCSharpPropertyNameString(item);

                if (first_flag)
                {
                    first_flag = false;
                    sb.AppendLine(AddTabs(3) + "<td valign=\"top\"><a href=\"" + edit_object_name + ".aspx?id=<%# " + current_item_binding + " %>&ReturnPage=" + list_object_name + "&PageData=<%=PageinationData.ToString()%>\"><%# " + current_item_binding + " %></a></td>");
                }
                else
                {
                    sb.AppendLine(AddTabs(3) + "<td valign=\"top\"><span class=\"RepeaterCell\"><%# " + current_item_binding + " %></span></td>");
                }
            }

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "</tr>");
            sb.AppendLine(AddTabs(2) + "</AlternatingItemTemplate>");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + "<FooterTemplate>");
            sb.AppendLine(AddTabs(2) + "</table>");
            sb.AppendLine(AddTabs(2) + "</FooterTemplate>");
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "</asp:Repeater>");
            #endregion

            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "<asp:Button ID=\"btnFirstPage\" runat=\"server\" CssClass=\"Button1\" OnClick=\"btnFirstPage_Click\" Text=\"&lt;&lt;\" />");
            sb.AppendLine(AddTabs(1) + "<asp:Button ID=\"btnPrevPage\" runat=\"server\" CssClass=\"Button1\" OnClick=\"btnPrevPage_Click\" Text=\"&lt;\" />");
            sb.AppendLine(AddTabs(1) + "<asp:Label ID=\"lblCounter\" runat=\"server\" Text=\"Label\" />");
            sb.AppendLine(AddTabs(1) + "<asp:Button ID=\"btnNextPage\" runat=\"server\" CssClass=\"Button1\" OnClick=\"btnNextPage_Click\" Text=\"&gt;\" />");
            sb.AppendLine(AddTabs(1) + "<asp:Button ID=\"btnLastPage\" runat=\"server\" CssClass=\"Button1\" OnClick=\"btnLastPage_Click\" Text=\"&gt;&gt;\" />");
            sb.AppendLine(AddTabs(1) + "<asp:DropDownList ID=\"ddlPageSize\" runat=\"server\" OnSelectedIndexChanged=\"ddlPageSize_SelectedIndexChanged\" AutoPostBack=\"True\" /> items per page");

            sb.AppendLine();
            sb.AppendLine("</asp:Panel>");

            if (!createAsAspControl)
                sb.AppendLine("</asp:Content>");

            output.Body = sb.ToString();
            return output;
        }

        public OutputObject GenerateWebListPageCodeBehind(SqlTable sqlTable, List<string> namespaceIncludes)
        {
            if (sqlTable == null)
                return null;

            string class_name = "List" + NameFormatter.ToCSharpPropertyName(sqlTable.Name);

            OutputObject output = new OutputObject();
            output.Name = class_name + ".aspx.cs";
            output.Type = OutputObject.eObjectType.CSharp;

            namespaceIncludes.Add(NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name));

            var sb = new StringBuilder();

            #region Includes
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Configuration;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Web;");
            sb.AppendLine("using System.Web.Security;");
            sb.AppendLine("using System.Web.UI;");
            sb.AppendLine("using System.Web.UI.WebControls;");
            sb.AppendLine("using System.Web.UI.WebControls.WebParts;");
            sb.AppendLine("using System.Web.UI.HtmlControls;");
            sb.AppendLine();

            sb.AppendLine(GenerateNamespaceIncludes(namespaceIncludes));
            sb.AppendLine();

            #endregion

            sb.AppendLine("namespace WebControls." + NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name));
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "public partial class " + class_name + " : System.Web.UI.Page");
            sb.AppendLine(AddTabs(1) + "{");

            #region Fields
            sb.AppendLine(AddTabs(2) + "#region Fields");
            sb.AppendLine();
            sb.AppendLine(AddTabs(3) + "protected string _SQLConnection = ConfigurationManager.ConnectionStrings[\"SQLConnection\"].ConnectionString;");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();
            #endregion

            #region Properties
            sb.AppendLine(AddTabs(2) + "#region Properties");
            sb.AppendLine();

            sb.AppendLine(AddTabs(3) + "protected string PageName");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get { return \"" + class_name + "\"; }");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine(AddTabs(3) + "protected PaginationManager PageinationData");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (ViewState[\"PaginationManager\"] == null)");
            sb.AppendLine(AddTabs(6) + "ViewState[\"PaginationManager\"] = new PaginationManager();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "return (PaginationManager)ViewState[\"PaginationManager\"];");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "set");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "ViewState[\"PaginationManager\"] = value;");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");

            sb.AppendLine(AddTabs(3) + "protected string ReturnPage");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (ViewState[\"ReturnPage\"] == null)");
            sb.AppendLine(AddTabs(6) + "ViewState[\"ReturnPage\"] = \"~/Default.aspx\";");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "return (string)ViewState[\"ReturnPage\"];");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "set");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "ViewState[\"ReturnPage\"] = value;");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");

            sb.AppendLine(AddTabs(3) + "protected int EditingID");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (ViewState[\"EditingID\"] == null)");
            sb.AppendLine(AddTabs(6) + "ViewState[\"EditingID\"] = -1;");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "return (int)ViewState[\"EditingID\"];");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "set");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "ViewState[\"EditingID\"] = value;");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");

            sb.AppendLine(AddTabs(3) + "protected string SearchString");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (ViewState[\"SearchString\"] == null)");
            sb.AppendLine(AddTabs(6) + "ViewState[\"SearchString\"] = string.Empty;");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "return (string)ViewState[\"SearchString\"];");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "set");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "ViewState[\"SearchString\"] = value;");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();
            #endregion

            sb.AppendLine(AddTabs(2) + "#region Methods");
            sb.AppendLine();

            #region Bind Repeater method
            sb.AppendLine(AddTabs(3) + "protected void BindRepeater()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "var dal_obj = new " + NameFormatter.ToCSharpClassName("Dal" + sqlTable.Name) + "(_SQLConnection);");
            sb.AppendLine(AddTabs(4) + "dal_obj.LoadAllFromDbPaged(this.PageinationData.ItemIndex, this.PageinationData.PageSize, this.SearchString);");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "this.PageinationData.SetSize = dal_obj.GetSearchCountFromDb(this.SearchString);");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "rptList.DataSource = dal_obj.Collection;");
            sb.AppendLine(AddTabs(4) + "rptList.DataBind();");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region Bind Page Size Control method
            sb.AppendLine(AddTabs(3) + "protected void BindPageSizeControl()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "this.ddlPageSize.Items.Clear();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "this.ddlPageSize.Items.Add(new ListItem(\"10\"));");
            sb.AppendLine(AddTabs(4) + "this.ddlPageSize.Items.Add(new ListItem(\"25\"));");
            sb.AppendLine(AddTabs(4) + "this.ddlPageSize.Items.Add(new ListItem(\"50\"));");
            sb.AppendLine(AddTabs(4) + "this.ddlPageSize.Items.Add(new ListItem(\"100\"));");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "this.ddlPageSize.Text = this.PageinationData.PageSize.ToString();");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region Bind Page Counter method
            sb.AppendLine(AddTabs(3) + "protected void BindPageCounter()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "this.lblCounter.Text = string.Format(\"Items {0} - {1} of {2}\",");
            sb.AppendLine(AddTabs(4) + "this.PageinationData.FirstItemOnCurrentPage, ");
            sb.AppendLine(AddTabs(4) + "this.PageinationData.LastItemOnCurrentPage, ");
            sb.AppendLine(AddTabs(4) + "this.PageinationData.SetSize);");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region Check Credentials method
            sb.AppendLine(AddTabs(3) + "protected void CheckCredentials()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#region Events");
            sb.AppendLine();

            #region pageload event method
            sb.AppendLine(AddTabs(3) + "protected void Page_Load(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "CheckCredentials();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "this.txtSearch.Focus();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "if (!Page.IsPostBack)");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (Request.QueryString[\"ReturnPage\"] != null)");
            sb.AppendLine(AddTabs(6) + "this.ReturnPage = (string)Request.QueryString[\"ReturnPage\"];");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "if (Request.QueryString[\"PageData\"] != null)");
            sb.AppendLine(AddTabs(6) + "this.PageinationData = new PaginationManager(Request.QueryString[\"PageData\"].ToString());");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "BindPageSizeControl();");
            sb.AppendLine(AddTabs(5) + "BindRepeater();");
            sb.AppendLine(AddTabs(5) + "BindPageCounter();");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region btnPrevPage_Click method
            sb.AppendLine(AddTabs(3) + "protected void btnPrevPage_Click(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "this.PageinationData.PreviousPage();");
            sb.AppendLine(AddTabs(4) + "BindRepeater();");
            sb.AppendLine(AddTabs(4) + "BindPageCounter();");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region btnNextPage_Click method
            sb.AppendLine(AddTabs(3) + "protected void btnNextPage_Click(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "this.PageinationData.NextPage();");
            sb.AppendLine(AddTabs(4) + "BindRepeater();");
            sb.AppendLine(AddTabs(4) + "BindPageCounter();");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region btnFirstPage_Click method
            sb.AppendLine(AddTabs(3) + "protected void btnFirstPage_Click(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "this.PageinationData.JumpToFirstPage();");
            sb.AppendLine(AddTabs(4) + "BindRepeater();");
            sb.AppendLine(AddTabs(4) + "BindPageCounter();");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region btnLastPage_Click method
            sb.AppendLine(AddTabs(3) + "protected void btnLastPage_Click(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "this.PageinationData.JumpToLastPage();");
            sb.AppendLine(AddTabs(4) + "BindRepeater();");
            sb.AppendLine(AddTabs(4) + "BindPageCounter();");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region btnAddNewItem_Click method
            sb.AppendLine(AddTabs(3) + "protected void btnAddNewItem_Click(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "this.Response.Redirect(\"Edit" + NameFormatter.ToCSharpPropertyName(sqlTable.Name) + ".Aspx?id=0&ReturnPage=\" + this.PageName + \"&PageData=\" + this.PageinationData.ToString(), true);");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region btnSearch_Click method
            sb.AppendLine(AddTabs(3) + "protected void btnSearch_Click(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "this.SearchString = this.txtSearch.Text;");
            sb.AppendLine(AddTabs(4) + "this.PageinationData.ItemIndex = 1;");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "BindPageSizeControl();");
            sb.AppendLine(AddTabs(4) + "BindRepeater();");
            sb.AppendLine(AddTabs(4) + "BindPageCounter();");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region ddlPageSize_SelectedIndexChanged method
            sb.AppendLine(AddTabs(3) + "protected void ddlPageSize_SelectedIndexChanged(Object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "this.PageinationData.PageSize = Convert.ToInt32(this.ddlPageSize.Text);");
            sb.AppendLine(AddTabs(4) + "BindRepeater();");
            sb.AppendLine(AddTabs(4) + "BindPageCounter();");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine(AddTabs(1) + "}");
            sb.AppendLine("}");
            sb.AppendLine();
            output.Body = sb.ToString();
            return output;
        }

        public OutputObject GenerateDefaultPageCodeInFront(string databaseName)
        {
            if (string.IsNullOrEmpty(databaseName))
                return null;

            OutputObject output = new OutputObject();
            output.Name = ConfigurationManager.AppSettings["DefaultASPPageFilename"] + ".aspx";
            output.Type = OutputObject.eObjectType.Aspx;

            var sb = new StringBuilder();

            sb.AppendLine("<%@ Page Language=\"C#\" MasterPageFile=\"~/MasterPage.master\" AutoEventWireup=\"true\" CodeBehind=\"Default.aspx.cs\" Inherits=\"WebControls." + NameFormatter.ToCSharpPropertyName(databaseName) + ".Default\" %>");
            sb.AppendLine();

            sb.AppendLine("<asp:Content ID=\"Content1\" ContentPlaceHolderID=\"head\" Runat=\"Server\">");
            sb.AppendLine("</asp:Content>");
            sb.AppendLine();

            sb.AppendLine("<asp:Content ID=\"Content2\" ContentPlaceHolderID=\"body\" Runat=\"Server\">");
            sb.AppendLine("</asp:Content>");
            sb.AppendLine();

            output.Body = sb.ToString();
            return output;
        }

        public OutputObject GenerateDefaultPageCodeBehind(string databaseName)
        {
            if (string.IsNullOrEmpty(databaseName))
                return null;

            OutputObject output = new OutputObject();
            output.Name = ConfigurationManager.AppSettings["DefaultASPPageFilename"] + ".aspx.cs";
            output.Type = OutputObject.eObjectType.CSharp;

            var sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Configuration;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Web;");
            sb.AppendLine("using System.Web.Security;");
            sb.AppendLine("using System.Web.UI;");
            sb.AppendLine("using System.Web.UI.WebControls;");
            sb.AppendLine("using System.Web.UI.WebControls.WebParts;");
            sb.AppendLine("using System.Web.UI.HtmlControls;");
            sb.AppendLine();

            sb.AppendLine("namespace WebControls." + NameFormatter.ToCSharpPropertyName(databaseName));
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "public partial class Default : System.Web.UI.Page");
            sb.AppendLine(AddTabs(1) + "{");
            sb.AppendLine(AddTabs(2) + "protected void Page_Load(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(2) + "{");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "}");
            sb.AppendLine(AddTabs(1) + "}");
            sb.AppendLine("}");

            output.Body = sb.ToString();
            return output;
        }

        protected string GenerateRegexValidator(eSqlBaseType baseType, string controlId, string controlToValidate)
        {
            string is_bool = @"^([0-1]|true|false)$";
            string is_int = @"[-+]?[0-9]{1,10}";
            string is_float = @"^[-+]?[0-9.]+$";
            string is_guid = @"^[0-9a-fA-F]{8}[-]?[0-9a-fA-F]{4}[-]?[0-9a-fA-F]{4}[-]?[0-9a-fA-F]{4}[-]?[0-9a-fA-F]{12}$";


            string regex;

            switch (baseType)
            {
                case eSqlBaseType.Bool:
                    regex = "<asp:RegularExpressionValidator ID=\"" + controlId + "\" ControlToValidate=\"" + controlToValidate + "\" ErrorMessage=\"Value is not an boolean value\" ValidationExpression=\"" + is_bool + "\" runat=\"server\" />";
                    break;

                case eSqlBaseType.Integer:
                    regex = "<asp:RegularExpressionValidator ID=\"" + controlId + "\" ControlToValidate=\"" + controlToValidate + "\" ErrorMessage=\"Value is not an integer value\" ValidationExpression=\"" + is_int + "\" runat=\"server\" />";
                    break;

                case eSqlBaseType.Float:
                    regex = "<asp:RegularExpressionValidator ID=\"" + controlId + "\" ControlToValidate=\"" + controlToValidate + "\" ErrorMessage=\"Value is not a decimal value\" ValidationExpression=\"" + is_float + "\" runat=\"server\" />";
                    break;

                case eSqlBaseType.Guid:
                    regex = "<asp:RegularExpressionValidator ID=\"" + controlId + "\" ControlToValidate=\"" + controlToValidate + "\" ErrorMessage=\"Value is not a GUID\" ValidationExpression=\"" + is_guid + "\" runat=\"server\" />";
                    break;

                // DateTimeRegex isn't good enough yet        
                //case BaseType.Time:
                //regex = "<asp:RegularExpressionValidator ID=\"" + control_ID + "\" ControlToValidate=\"" + control_to_validate + "\" ErrorMessage=\"Value is not a valid date time\" ValidationExpression=\"" + RegularExpression.s_DateTime + "\" runat=\"server\" />"; 
                //break;

                default:
                    regex = string.Empty;
                    break;
            }

            return regex;
        }

        // todo create html 5 Single page class?
        public OutputObject GenerateWebViewPageCodeInFront(SqlTable sqlTable)
        {
            if (sqlTable == null)
                return null;

            string view_class_name = "View" + NameFormatter.ToCSharpPropertyName(sqlTable.Name);
            string table_name = NameFormatter.ToCSharpPropertyName(sqlTable.Name);

            var output = new OutputObject();
            output.Name = view_class_name + ".aspx";
            output.Type = OutputObject.eObjectType.Aspx;

            var sb = new StringBuilder();

            sb.AppendLine($"<%@ Page Language=\"C#\" MasterPageFile=\"~/MasterPage.master\" AutoEventWireup=\"True\" CodeBehind=\"{view_class_name}.aspx.cs\" Inherits=\"WebControls.{NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name)}.{view_class_name}\" %>");
            sb.AppendLine();
            sb.AppendLine("<asp:Content ID=\"Content1\" ContentPlaceHolderID=\"head\" Runat=\"Server\">");
            sb.AppendLine();

            /////////////////////////////////////////////////////////////////////////////////////////////

            sb.AppendLine(AddTabs(1) + "<script type = \"text/javascript\">");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "$(function()");
            sb.AppendLine(AddTabs(2) + "{");
            sb.AppendLine(AddTabs(3) + $"Load{table_name}();");
            sb.AppendLine(AddTabs(2) + "});");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + $"function Load{table_name}()");
            sb.AppendLine(AddTabs(2) + "{");
            sb.AppendLine(AddTabs(3) + "var data = { };");
            sb.AppendLine(AddTabs(3) + $"CallAjax(data, '{output.Name}/Load{table_name}', Load{table_name}Response);");
            sb.AppendLine(AddTabs(2) + "}");

            sb.AppendLine(AddTabs(2) + $"function Load{table_name}Response(response)");
            sb.AppendLine(AddTabs(2) + "{");
            sb.AppendLine(AddTabs(3) + "if (response.Success)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "var buffer = '';");
            sb.AppendLine(AddTabs(4) + $"var template = $('#{table_name}Template').html();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + $"response.DataList.forEach(function(i)");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "buffer += eval('`' + template + '`');");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + $"$('#{table_name}List').html(buffer);");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine(AddTabs(3) + "else");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "$('#txtMessage').text(response.Message);");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + $"function Save{table_name}()");
            sb.AppendLine(AddTabs(2) + "{");
            sb.AppendLine(AddTabs(3) + "var data = { };");

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                string colum_name = NameFormatter.ToCSharpPropertyName(sql_column.Name);
                sb.AppendLine(AddTabs(3) + $"data.{colum_name} = $('#txt{colum_name}').val();");
            }

            sb.AppendLine();
            sb.AppendLine(AddTabs(3) + $"CallAjax(data, '{output.Name}/Save{table_name}', Save{table_name}Response);");
            sb.AppendLine(AddTabs(2) + "}");

            sb.AppendLine(AddTabs(2) + $"function Save{table_name}Response(response)");
            sb.AppendLine(AddTabs(2) + "{");
            sb.AppendLine(AddTabs(3) + "if (response.Success)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + $"Load{table_name}();");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine(AddTabs(3) + "else");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "$('#txtMessage').text(response.Message);");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine(AddTabs(2) + "}");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + "</script>");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + $"<script type = 'text/template' id='{table_name}Template'>");
            sb.AppendLine(AddTabs(2) + "<tr>");

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                string colum_name = NameFormatter.ToCSharpPropertyName(sql_column.Name);
                sb.AppendLine(AddTabs(3) + $"<td>${{i.{colum_name}}}</td>");
            }

            sb.AppendLine(AddTabs(2) + "</tr>");
            sb.AppendLine(AddTabs(2) + "</script>");
            sb.AppendLine();
            sb.AppendLine("</asp:Content>");
            sb.AppendLine();

            /////////////////////////////////////////////////////////////////////////////////////////////

            sb.AppendLine("<asp:Content ID=\"Content2\" ContentPlaceHolderID=\"body\" Runat=\"Server\">");
            sb.AppendLine();

            sb.AppendLine(AddTabs(1) + $"<div id='pan{table_name}List'>");
            sb.AppendLine(AddTabs(2) + $"<div style='overflow: auto;'>");

            //< div class="SectionTitle" style="float: left;">Languages</div>
            //<div style = "float: right;" >
            //< input type="button" class="Button" value="Add New Language" onclick="$('#dialog-AddLanguage').dialog('open');" />
            //</div>
            //</div>

            //<table>
            //<thead>
            //<tr>
            //<th>Language</th>
            //<th>Family</th>
            //<th>Format</th>
            //<th>Difficulty</th>
            //<th>Rarity</th>
            //</tr>
            //</thead>
            //<tbody id = "LangaugeList" ></ tbody >
            //</ table >
            //</ div >








            sb.AppendLine();
            sb.AppendLine("<div class=\"PageTitle\">View " + NameFormatter.ToFriendlyName(sqlTable.Name) + "</div>");
            sb.AppendLine();

            sb.AppendLine("<asp:Panel CssClass=\"DetailsPanel\" ID=\"panDetails\" runat=\"server\">");
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "<div class=\"Spacer\">");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + "<table border=\"0\" width=\"100%\">");
            sb.AppendLine(AddTabs(2) + "<tr>");
            sb.AppendLine(AddTabs(3) + "<td></td>");
            sb.AppendLine(AddTabs(3) + "<td class=\"SectionTitle\"><asp:Label ID=\"lblTitle\" runat=\"server\" /></td>");
            sb.AppendLine(AddTabs(2) + "</tr>");
            sb.AppendLine();

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (!sql_column.IsPk)
                {
                    sb.AppendLine(AddTabs(2) + "<tr>");
                    sb.AppendLine(AddTabs(3) + "<td class=\"ControlLabel\">" + NameFormatter.ToFriendlyName(sql_column.Name) + ":</td>");

                    switch (sql_column.BaseType)
                    {
                        case eSqlBaseType.Bool:
                            sb.AppendLine(AddTabs(3) + "<td><asp:CheckBox ID=\"chk" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\" Enabled=\"False\"  runat=\"server\" /></td>");
                            break;

                        //case eSqlBaseType.Time:
                        //sb.AppendLine(AddTabs(3) + "<td><asp:Calendar ID=\"cal" + NameFormatter.ToCSharpPropertyName(sql_column.ColumnName) + " runat=\"server\" /></td>");
                        //break;

                        default:
                            sb.AppendLine(AddTabs(3) + "<td><asp:Label ID=\"lbl" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\" runat=\"server\" /></td>");
                            break;
                    }

                    sb.AppendLine(AddTabs(2) + "</tr>");
                    sb.AppendLine();
                }
            }

            // buttons
            sb.AppendLine(AddTabs(2) + "</table>");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "<asp:Label ID=\"lblErrorMessage\" CssClass=\"ErrorMessage\" runat=\"server\" />");
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "</div>");
            sb.AppendLine();
            sb.AppendLine("</asp:Panel>");

            output.Body = sb.ToString();
            return output;
        }
    }
}