using System;
using System.IO;
using System.Collections.Generic;
using System.Data.SqlClient;

using Xunit;
using dcp.lib.Writers;

namespace tests
{
    [CollectionDefinition("SqlServer WriterTests", DisableParallelization = true)]
    public class SqlServerWriterTestsCollection { }

    [Collection("SqlServer WriterTests")]
    public class SqlServerWriterTests
    {

        List<dcp.lib.Row> rows = new List<dcp.lib.Row>();
        const string SERVER = "(localdb)\\MSSQLLocalDB";
        public const string TABLENAME = "dcpSqlServerTestTable";
        const string DATABASENAME = "dcpSqlServerTestDatabase";

        public static string CONNECTIONSTRINGWITHOUTDATABASE = $"Server={SERVER};Trusted_Connection=True;";
        public static string CONNECTIONSTRINGWITHDATABASE = $"Database={DATABASENAME};Server={SERVER};Trusted_Connection=True;";
        public SqlServerWriterTests()
        {
            var row = new dcp.lib.Row();
            row.Add(1);
            row.Add("two");
            row.Add(new DateTime(1961, 3, 14, 1,2,3));
            rows.Add(row);
        }
#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Dispose()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
            dropDatabase();
        }
        [Fact]
        public void WritesRow()
        {
            createCleanDatabase();
            createCleanTable();
            var opts = new dcp.lib.DataCopier.Options();
            opts.outputTable = TABLENAME;
            writeRow(CONNECTIONSTRINGWITHDATABASE, opts);
            var result = runQuery($"USE {DATABASENAME}; SELECT * FROM {TABLENAME}");
            Assert.Equal(rows[0][0], result[0]);
        }
        [Fact]
        public void WritesLastRowValue()
        {
            createCleanDatabase();
            createCleanTable();
            var opts = new dcp.lib.DataCopier.Options();
            opts.outputTable = TABLENAME;
            writeRow(CONNECTIONSTRINGWITHDATABASE, opts);
            var result = runQuery($"USE {DATABASENAME}; SELECT * FROM {TABLENAME}");
            Assert.Equal(rows[0][2], result[2]);
        }
        [Fact]
        public void ThrowsIfStringTooLong()
        {
            createCleanDatabase();
            createCleanTable();
            var opts = new dcp.lib.DataCopier.Options();
            opts.outputTable = "testTable";
            rows[0][1]="tooLongString";
            Assert.Throws<SqlException>(() => writeRow(CONNECTIONSTRINGWITHDATABASE,opts));
        }
        [Fact]
        public void TruncatesTableIfRequested()
        {
            createCleanDatabase();
            createCleanTable();
            addRowToTable();

            var opts = new dcp.lib.DataCopier.Options();
            opts.truncate = true;
            opts.outputTable = TABLENAME;
            writeRow (CONNECTIONSTRINGWITHDATABASE, opts);

            var result = runQuery($"USE {DATABASENAME}; SELECT COUNT(col1) FROM {TABLENAME}");
            Assert.Equal(1, result[0]);
        }

        [Fact]
        public void DoesNotTruncateTableIfNotRequested()
        {
            createCleanDatabase();
            createCleanTable();
            addRowToTable();

            writeRow (CONNECTIONSTRINGWITHDATABASE, new dcp.lib.DataCopier.Options { outputTable = TABLENAME});

            var result = runQuery($"USE {DATABASENAME}; SELECT COUNT(col1) FROM {TABLENAME}");
            Assert.Equal(2, result[0]);
        }
        private void writeRow(string connectionString, dcp.lib.DataCopier.Options opts)
        {
            using (var writer = new SqlServer(connectionString, opts))
            {
                writer.Next(rows);
            }
        }
        List<Object> runQuery(string sql)
        {
            var result = new List<Object>();
            using (var connection = new SqlConnection(CONNECTIONSTRINGWITHOUTDATABASE))
            {
                connection.Open();
                using (var cmd = new SqlCommand(sql, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        for (var fieldNumber = 0; fieldNumber < reader.FieldCount; fieldNumber++)
                        {
                            result.Add(reader[fieldNumber]);
                        }
                    }
                }
            }
            return result;
        }
        void runNonQuery(string sql)
        {
            using (var connection = new SqlConnection(CONNECTIONSTRINGWITHOUTDATABASE))
            {
                connection.Open();
                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
        private void createCleanDatabase()
        {
            runNonQuery($"USE tempdb; IF DB_ID('{DATABASENAME}') IS NOT NULL DROP DATABASE {DATABASENAME}; CREATE DATABASE {DATABASENAME}");
        }
        private void dropDatabase()
        {
            runNonQuery($"USE tempdb; IF DB_ID('{DATABASENAME}') IS NOT NULL DROP DATABASE {DATABASENAME};");
        }
        private void createCleanTable()
        {
            runNonQuery($"USE {DATABASENAME}; IF OBJECT_ID (N'{TABLENAME}', N'U') IS NOT NULL DROP TABLE {TABLENAME}; CREATE TABLE {TABLENAME} (col1 int, col2 nvarchar(3), col3 DateTime)");
        }
        private void addRowToTable()
        {
            runNonQuery($"USE {DATABASENAME}; INSERT INTO {TABLENAME} VALUES (99,'$$$','2017-02-03')");
        }
    }
}

