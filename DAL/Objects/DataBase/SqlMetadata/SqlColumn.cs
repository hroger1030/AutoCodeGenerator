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

namespace DAL.SqlMetadata
{
    public class SqlColumn
    {
        #region Fields

            protected SqlTable _SqlTable;
            protected string _Name;
            protected string _DataType;
            protected int _Length;
            protected int _Precision;
			protected int _Scale;
            protected bool _IsNullable;
            protected bool _IsPk;
            protected bool _IsIdentity;
            protected int _ColumnOrdinal;
            protected string _DefaultValue;

        #endregion

        #region Properties

            public SqlTable Table
            {
                get { return _SqlTable; }
                set { _SqlTable = value;}
            }

            public string Name
            {
                get { return _Name; }
                set { _Name = value; }
            }
            public string DataType
            {
                get { return _DataType; }
                set { _DataType = value; }
            }
            public int Length
            {
                get { return _Length; }
                set { _Length = value; }
            }
            public int Precision
            {
                get { return _Precision; }
                set { _Precision = value; }
            }
            public int Scale
            {
                get { return _Scale; }
                set { _Scale = value; }
            }
            public bool IsNullable
            {
                get { return _IsNullable; }
                set { _IsNullable = value; }
            }
            public bool IsPk
            {
                get { return _IsPk; }
                set { _IsPk = value; }
            }
            public bool IsIdentity
            {
                get { return _IsIdentity; }
                set { _IsIdentity = value; }
            }
            public int ColumnOrdinal
            {
                get { return _ColumnOrdinal; }
                set { _ColumnOrdinal = value; }
            }
            public string DefaultValue
            {
                get { return _DefaultValue; }
                set { _DefaultValue = value; }
            }

            public SqlDbType SqlDataType
            { 
                get 
                {
                    // HACK - SqlDbType has no entry for 'numeric'. Until the twerps in Redmond add this value:
                    if (_DataType == "numeric")
                        return SqlDbType.Decimal;

                   // HACK - SqlDbType has no entry for 'sql_variant'. Until the twerps in Redmond add this value:
                    if (_DataType == "sql_variant")
                        return SqlDbType.Variant;

                    return (SqlDbType)Enum.Parse(typeof(SqlDbType), _DataType, true);
                }
            }
            public eSqlBaseType BaseType
            { 
                get { return this.MapBaseType(); }
            }

        #endregion

        #region Methods

            public SqlColumn()
            {
                Reset();
            }

            public SqlColumn(SqlTable sql_table, string column_name) : this (sql_table, column_name, string.Empty, 0, 0, 0, false, false, false, 0, string.Empty) { }
        
            public SqlColumn(SqlTable sql_table, string column_name, string datatype, int length, int precision, int scale, 
                             bool is_nullable, bool is_pk, bool is_identity, int column_ordinal, string default_value)
            {
                _SqlTable           = sql_table;

                _Name               = column_name;
                _DataType           = datatype;
                _Length             = length;
                _Precision          = precision;
				_Scale				= scale;
                _IsNullable         = is_nullable;
                _IsPk               = is_pk;
                _IsIdentity         = is_identity;
                _ColumnOrdinal      = column_ordinal;
                _DefaultValue       = default_value;
            }

            public void Reset()
            {
                _SqlTable           = null;

                _DataType           = string.Empty;
                _Length             = 0;
                _Precision          = 0;
				_Scale				= 0;
                _IsNullable         = false;
                _IsPk               = false;
                _IsIdentity         = false;
                _ColumnOrdinal      = 0;
                _DefaultValue       = string.Empty;
            }
            
            protected eSqlBaseType MapBaseType()
            {
                SqlDbType sql_type = (SqlDbType)Enum.Parse(typeof(SqlDbType), _DataType, true);

                switch (sql_type)
                {
                    case SqlDbType.BigInt:		    	return eSqlBaseType.Integer;
                    case SqlDbType.Binary:		    	return eSqlBaseType.BinaryData;
                    case SqlDbType.Bit:			    	return eSqlBaseType.Bool;
                    case SqlDbType.Char:		    	return eSqlBaseType.String;
					case SqlDbType.Date:		    	return eSqlBaseType.Time;
                    case SqlDbType.DateTime:	    	return eSqlBaseType.Time;
					case SqlDbType.DateTime2:	    	return eSqlBaseType.Time;
					case SqlDbType.DateTimeOffset:  	return eSqlBaseType.Time;
					case SqlDbType.Decimal:		    	return eSqlBaseType.Float;
                    case SqlDbType.Float:               return eSqlBaseType.Float;
                    //case SqlDbType.Geography: 
                    //case SqlDbType.Geometry: 
                    case SqlDbType.Image:               return eSqlBaseType.BinaryData;
                    case SqlDbType.Int:                 return eSqlBaseType.Integer;
                    case SqlDbType.Money:               return eSqlBaseType.Float;
                    case SqlDbType.NChar:               return eSqlBaseType.String;
                    case SqlDbType.NText:               return eSqlBaseType.String;
                    case SqlDbType.NVarChar:            return eSqlBaseType.String;
                    case SqlDbType.Real:                return eSqlBaseType.Float;
                    case SqlDbType.SmallDateTime:       return eSqlBaseType.Time;
                    case SqlDbType.SmallInt:            return eSqlBaseType.Integer;
                    case SqlDbType.SmallMoney:          return eSqlBaseType.Float;       
                    case SqlDbType.Structured:          return eSqlBaseType.String;       
                    case SqlDbType.Text:                return eSqlBaseType.String;
                    case SqlDbType.Time:                return eSqlBaseType.Time;
                    case SqlDbType.Timestamp:           return eSqlBaseType.BinaryData;
                    case SqlDbType.TinyInt:             return eSqlBaseType.Integer;
                    case SqlDbType.Udt:                 return eSqlBaseType.String;
                    case SqlDbType.UniqueIdentifier:    return eSqlBaseType.Guid;
                    case SqlDbType.VarBinary:           return eSqlBaseType.BinaryData;
                    case SqlDbType.VarChar:             return eSqlBaseType.String;
                    case SqlDbType.Variant:             return eSqlBaseType.String;
                    case SqlDbType.Xml:                 return eSqlBaseType.String;

                    default:                    
                        return eSqlBaseType.Unknown;
                }
            }

        #endregion
    }
}
