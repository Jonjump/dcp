using System;
using System.IO;
using System.Collections.Generic;
using OfficeOpenXml;
using Xunit;
using dcp.lib;
using dcp.lib.Readers;
using System.Data.SqlClient;

namespace tests
{
    [CollectionDefinition("SqlServer ReaderTests", DisableParallelization = true)]
    public class SqlServerReaderTestsCollection { }

    [Collection("SqlServer ReaderTests")]
    public class SqlServerReaderTests
    {
        const string SERVER = "(localdb)\\MSSQLLocalDB";
        public const string TABLENAME = "dcpSqlServerReaderTestTable";
        const string DATABASENAME = "dcpSqlServerReaderTestDatabase";

        public static string CONNECTIONSTRINGWITHOUTDATABASE = $"Server={SERVER};Trusted_Connection=True;";
        public static string CONNECTIONSTRINGWITHDATABASE = $"Database={DATABASENAME};Server={SERVER};Trusted_Connection=True;";
        SqlConnection connection;
        public SqlServerReaderTests()
        {
            connection = new SqlConnection(CONNECTIONSTRINGWITHOUTDATABASE);
            connection.Open();
            createCleanDatabase();
            createCleanTable();
            addRowsToTable();
        }
#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Dispose()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
            dropDatabase();
            connection.Close();
            connection.Dispose();
        }
        void runNonQuery(string sql)
        {
            using (var cmd = new SqlCommand(sql, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }
        private void createCleanDatabase()
        {
            runNonQuery($"IF DB_ID('{DATABASENAME}') IS NOT NULL DROP DATABASE {DATABASENAME}; CREATE DATABASE {DATABASENAME};");
        }
        private void dropDatabase()
        {
            runNonQuery($"IF DB_ID('{DATABASENAME}') IS NOT NULL DROP DATABASE {DATABASENAME};");
        }
        private void createCleanTable()
        {
            runNonQuery($"USE {DATABASENAME}; IF OBJECT_ID (N'{TABLENAME}', N'U') IS NOT NULL DROP TABLE {TABLENAME}; CREATE TABLE {TABLENAME} (col1 int, col2 nvarchar(3), col3 DateTime); USE tempdb;");
        }
        private void addRowsToTable()
        {
            runNonQuery($"USE {DATABASENAME}; INSERT INTO {TABLENAME} VALUES (91,'$$1','2017-02-01'); INSERT INTO {TABLENAME} VALUES (92,'$$2','2017-02-02'); INSERT INTO {TABLENAME} VALUES (93,'$$3','2017-02-03'); USE tempdb;");
        }
        [Fact]
        public void ReadsFirstRowValue()
        {
            var opts = new DataCopier.Options();
            opts.inputTable = TABLENAME;
            using (var reader = new SqlServer(CONNECTIONSTRINGWITHDATABASE, opts))
            {
                var rows = reader.Next(1);
                Assert.Equal(91, rows[0][0]);
            }
        }
        [Fact]
        public void ReadsLastRowValue()
        {
            var opts = new DataCopier.Options();
            opts.inputTable = TABLENAME;
            using (var reader = new SqlServer(CONNECTIONSTRINGWITHDATABASE, opts))
            {
                var rows = reader.Next(1);
                Assert.Equal(new DateTime(2017, 2, 1), rows[0][2]);
            }
        }
        [Fact]
        public void SkipsColumns()
        {
            var opts = new DataCopier.Options();
            opts.inputTable = TABLENAME;
            opts.skipColumns = 1;
            using (var reader = new SqlServer(CONNECTIONSTRINGWITHDATABASE, opts))
            {
                var rows = reader.Next(1);
                Assert.Equal("$$1", rows[0][0]);
            }
        }
        [Fact]
        public void UsesMaxColumns()
        {
            var opts = new DataCopier.Options();
            opts.inputTable = TABLENAME;
            opts.maxColumns = 1;
            using (var reader = new SqlServer(CONNECTIONSTRINGWITHDATABASE, opts))
            {
                var rows = reader.Next(1);
                Assert.Equal(91, rows[0][0]);
                Assert.Single(rows[0]);
            }
        }
        [Fact]
        public void ReturnsCorrectNumberOfRows()
        {
            var opts = new DataCopier.Options();
            opts.inputTable = TABLENAME;
            using (var reader = new SqlServer(CONNECTIONSTRINGWITHDATABASE, opts))
            {
                var rows = reader.Next(3);
                Assert.Equal(3, rows.Count);
            }
        }
        [Fact]
        public void ReturnsShortListIfNotEnoughRows()
        {
            var opts = new DataCopier.Options();
            opts.inputTable = TABLENAME;
            using (var reader = new SqlServer(CONNECTIONSTRINGWITHDATABASE, opts))
            {
                var rows = reader.Next(9);
                Assert.Equal(3, rows.Count);
            }
        }
        [Fact]
        public void LeavesRows()
        {
            var opts = new DataCopier.Options();
            opts.inputTable = TABLENAME;
            opts.leaveRows = 1;
            using (var reader = new SqlServer(CONNECTIONSTRINGWITHDATABASE, opts))
            {
                var rows = reader.Next(9);
                Assert.Equal(2, rows.Count);
            }
        }
    }
}
