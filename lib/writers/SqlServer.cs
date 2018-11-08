using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

namespace dcp.lib.Writers
{
    public class SqlServer : Writer
    {
        private static Dictionary<string, bool> needsQuotes = new Dictionary<string, bool>() {
            { "nchar", true },
            { "char", true },
            { "nvarchar", true },
            { "varchar", true },
            { "ntext", true },
            { "text", true },
            { "bigint", false },
            { "numeric", false },
            { "bit", false },
            { "smallint", false },
            { "decimal", false },
            { "smallmoney", false },
            { "int", false },
            { "tinyint", false },
            { "money", false },
            { "float", false },
            { "real", false },
            { "date", true },
            { "datetimeoffset", true },
            { "datetime", true },
            { "datetime2", true },
            { "smalldatetime", true },
            { "datetime	", true },
            { "time", true }
        };
        SqlConnection connection;
        Formatter formatter;
        DataCopier.Options opts;
        public SqlServer(string connectionString, DataCopier.Options opts)
        {
            if (String.IsNullOrWhiteSpace(opts.outputTable)) throw new ArgumentException("output table not specified");

            this.connection = new SqlConnection(connectionString);
            this.connection.Open();

            this.opts = opts;
            if (this.opts.truncate) runQuery($"TRUNCATE TABLE {this.opts.outputTable};");

            this.formatter = new Formatters.Sql(this.opts, getSchema());

        }
        List<Formatters.Sql.Column> getSchema()
        {
            using (var cmd = new SqlCommand("SELECT ORDINAL_POSITION,DATA_TYPE,IS_NULLABLE, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=@tableName AND TABLE_SCHEMA=@tableSchema ORDER BY ORDINAL_POSITION;"))
            {
                cmd.Connection = connection;

                if (opts.outputTable.Contains("."))
                {
                    var bits = opts.outputTable.Split(new char[] { '.' }, 2);
                    cmd.Parameters.AddWithValue("@tableSchema", bits[0]);
                    cmd.Parameters.AddWithValue("@tableName", bits[1]);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@tableSchema", "dbo");
                    cmd.Parameters.AddWithValue("@tableName", opts.outputTable);
                }


                var schema = new List<Formatters.Sql.Column>();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var type = ((string)reader[1]).ToLower();
                        var ordinalNumber = (int)reader[0];
                        var isNullable = ((string)reader[2]).ToUpper();
                        schema.Add(
                            new Formatters.Sql.Column(ordinalNumber, (isNullable == "YES"), type, (string)reader[3])
                        );
                    }
                }
                return schema;

            }
        }
        List<Object> runQuery(string sql)
        {
            var result = new List<Object>();
            using (var cmd = new SqlCommand(sql, connection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows) return result;
                    reader.Read(); // first line only
                    for (var fieldNumber = 0; fieldNumber < reader.FieldCount; fieldNumber++)
                    {
                        result.Add(reader[fieldNumber]);
                    }
                }
            }
            return result;
        }
        public override void Dispose()
        {
            if (connection.State != System.Data.ConnectionState.Closed)
            {
                // if we don't do this, sql hangs on to the database for a while
                using (var command = new SqlCommand($"use tempdb;", connection))
                {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            connection.Dispose();
        }
        public override void Next(List<Row> rows)
        {
            var sql = new StringBuilder();
            foreach (var row in rows)
            {
                sql.AppendLine((string)formatter.Format(row));
            }
            using (var cmd = new SqlCommand(sql.ToString(), connection))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}
