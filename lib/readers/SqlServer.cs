using System;
using System.IO;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;

namespace dcp.lib.Readers
{
    public class SqlServer : Reader
    {
        private readonly DataCopier.Options opts;
        SqlConnection connection;

        private long nextRowNumber;
        private readonly long lastRowNumber;
        private readonly int startColumnNumber;
        private readonly int endColumnNumber;
        public SqlServer(string connectionString, DataCopier.Options opts)
        {
            this.opts = opts;
            if (String.IsNullOrWhiteSpace(opts.inputTable)) throw new ArgumentException("input table not specified");
            this.connection = new SqlConnection(connectionString);
            this.connection.Open();

            nextRowNumber = this.opts.skipRows + 1;
            var maxRows = getRowCount();
            lastRowNumber = maxRows - this.opts.leaveRows;

            startColumnNumber = this.opts.skipColumns + 1;
            endColumnNumber = getColumnCount();
            if (this.opts.maxColumns != null) endColumnNumber = Math.Min((int)this.opts.maxColumns, endColumnNumber);
        }
        int getColumnCount()
        {
            using (var command = new SqlCommand($"SELECT * FROM {opts.inputTable} WHERE 1=2", connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    reader.Read();
                    return reader.FieldCount;
                }
            }
        }
        int getRowCount()
        {
            using (var command = new SqlCommand($"SELECT COUNT(*) FROM {opts.inputTable} WITH (NOLOCK);", connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    reader.Read();
                    return (int)reader[0];
                }
            }
        }
        public void Dispose()
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
        public List<Row> Next(int rowsToRead)
        {
            var lastRow = Math.Min(lastRowNumber, nextRowNumber + rowsToRead - 1);
            var s = $"SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS rownum, * from {opts.inputTable}) A WHERE A.rownum>={nextRowNumber} AND A.rownum <={lastRow};";
            var rows = new List<Row>();
            using (var command = new SqlCommand(s, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rows.Add(readRow(reader));
                    }
                }
            }
            return rows;
        }
        private Row readRow(SqlDataReader reader)
        {
            var row = new dcp.lib.Row();
            for (var columnNumber = startColumnNumber; columnNumber <= endColumnNumber; columnNumber++)
            {
                row.Add(reader[columnNumber]); // the 0 row is the row number, which we don't want
            }
            nextRowNumber += 1;
            return row;
        }
    }
}
