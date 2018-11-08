using System;
using System.Collections.Generic;
using System.Linq;

namespace dcp.lib.Formatters
{
    public class Sql : Formatter
    {
        const string NULL = "NULL";
        const string DEFAULT = "DEFAULT";
        public struct Column
        {
            public readonly int ordinalNumber;
            public readonly bool isNullable;
            public readonly string typeName;
            public readonly string columnName;
            public Column (int ordinalNumber, bool isNullable, string typeName, string columnName)
            {
                this.ordinalNumber = ordinalNumber;
                this.isNullable = isNullable;
                this.typeName = typeName;
                this.columnName = columnName;
            }
        }
        List<Column> schema;
        readonly string tableName;
        static Dictionary<Type, Func<Object, string>> fieldFormatters = new Dictionary<Type, Func<Object, string>> {
                { typeof(int), (x) => ((int)x).ToString() },
                { typeof(double), (x) => ((double)x).ToString() },
                { typeof(DateTime), (x) => addSqlQuotes_Escape(((DateTime)x).ToString("yyyy-MM-dd hh:mm:ss")) }
        };
        static Func<string, bool, string> quoted = (x, isNullable) => addSqlQuotes_Escape(x); 
        static Func<string, bool, string> quoted_whitespaceIsNull = (x, isNullable) => {
            if (String.IsNullOrWhiteSpace(x)) return (isNullable) ? NULL : DEFAULT;
            return addSqlQuotes_Escape(whitespaceIsNull(x, isNullable));
        };
        static Func<string, bool, string> whitespaceIsNull = (x, isNullable) => {
            if (String.IsNullOrWhiteSpace(x)) return (isNullable) ? NULL : DEFAULT;
            return x;
        };
        static Dictionary<string, Func<string, bool, string>> stringFormatters = new Dictionary<string, Func<string, bool, string>> {

            { "bigint", whitespaceIsNull},
            { "decimal", whitespaceIsNull},
            { "float", whitespaceIsNull},
            { "numeric", whitespaceIsNull},
            { "image", whitespaceIsNull},
            { "int", whitespaceIsNull},
            { "money", whitespaceIsNull},
            { "rowversion", whitespaceIsNull},
            { "real", whitespaceIsNull},
            { "smallint", whitespaceIsNull},
            { "smallmoney", whitespaceIsNull},
            { "tinyint", whitespaceIsNull},
            { "varbinary", whitespaceIsNull},

            { "char", quoted},
            { "nchar", quoted},
            { "ntext", quoted},
            { "nvarchar", quoted},
            { "text", quoted},
            { "varchar", quoted},

            { "binary", quoted_whitespaceIsNull},
            { "bit",  quoted_whitespaceIsNull},
            { "date", quoted_whitespaceIsNull},
            { "datetime", quoted_whitespaceIsNull},
            { "datetime2", quoted_whitespaceIsNull},
            { "datetimeoffset", quoted_whitespaceIsNull},
            { "smalldatetime", quoted_whitespaceIsNull},
            { "time", quoted_whitespaceIsNull},
            { "timestamp", quoted_whitespaceIsNull},
            { "uniqueidentifier", quoted_whitespaceIsNull},
            { "xml", quoted_whitespaceIsNull},
        };
        public Sql(DataCopier.Options opts, List<Column> schema)
        {
            this.tableName = opts.outputTable;
            this.schema = schema;
        }
        public override Object Format(Row row)
        {
            return $"INSERT INTO [{tableName}] VALUES ({String.Join(",", getValues(row))});";
        }
        List<string> getValues(Row row)
        {
            if (schema.Count<row.Count) throw new Exception($"Database has only {schema.Count} columns, but row has {row.Count} values");
            List<string> values = new List<string>();
            for (var i = 0; i < row.Count; i++)
            {
                var field = row[i];
                var column = schema[i];
                values.Add(getValue(row[i], schema[i]));
            }
            return values;
        }
        string getValue(Object field, Column column)
        {
            if (field == null)
            {
                if (column.isNullable)  return NULL;
                throw new ArgumentNullException($"Column ${column.columnName} cannot be null");
            }
            if (field is string) return stringFormatters[column.typeName](stripQuotes((string)field), column.isNullable);
            return fieldFormatters[field.GetType()](field);
        }
        static string addSqlQuotes_Escape(string s)
        {
            return $"'{s.Replace("'","''")}'";
        }
        static string stripQuotes(Object field)
        {
            var s = (string)field;
            if (s.Length < 2) return s;
            if ((s[0] == '"') && (s[s.Length - 1] == '"')) return s.Substring(1, s.Length - 2);
            if ((s[0] == '\'') && (s[s.Length - 1] == '\'')) return s.Substring(1, s.Length - 2);
            return s;
        }

    }
}


