using System;
using System.IO;
using System.Collections.Generic;
using System.Data.SqlClient;

using Xunit;
using dcp.lib;

namespace tests
{
    public class SqlTests
    {
        
        readonly dcp.lib.Formatters.Sql formatter;
        List<dcp.lib.Formatters.Sql.Column> schema = new List<dcp.lib.Formatters.Sql.Column>();
        public SqlTests()
        {
            var opts = new DataCopier.Options();
            opts.outputTable = "testTable";
            formatter = new dcp.lib.Formatters.Sql(opts, schema);

        }
#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Dispose()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
        }
        [Fact]
        public void int2int()
        {
            schema.Add(new dcp.lib.Formatters.Sql.Column(1, false, "int", "colname"));
            var row = new dcp.lib.Row();
            row.Add(1);
            Assert.Equal("INSERT INTO [testTable] VALUES (1);",formatter.Format(row));
        }
        [Fact]
        public void null2nullableInt()
        {
            schema.Add(new dcp.lib.Formatters.Sql.Column(1, true , "int", "colname"));
            var row = new dcp.lib.Row();
            row.Add(null);
            Assert.Equal("INSERT INTO [testTable] VALUES (NULL);",formatter.Format(row));
        }
        [Fact]
        public void null2nonNullableInt()
        {
            schema.Add(new dcp.lib.Formatters.Sql.Column(1, false, "int", "colname"));
            var row = new dcp.lib.Row();
            row.Add(null);
            var ex = Record.Exception(() => formatter.Format(row));

            Assert.IsType<ArgumentNullException>(ex);
            Assert.Contains(schema[0].columnName, ex.Message);
        }
        [Fact]
        public void string2int()
        {
            schema.Add(new dcp.lib.Formatters.Sql.Column(1, false, "int", "colname"));
            var row = new dcp.lib.Row();
            row.Add("1");
            Assert.Equal("INSERT INTO [testTable] VALUES (1);",formatter.Format(row));
        }
        [Fact]
        public void emptyString2nullableInt()
        {
            schema.Add(new dcp.lib.Formatters.Sql.Column(1, true , "int", "colname"));
            var row = new dcp.lib.Row();
            row.Add("");
            Assert.Equal("INSERT INTO [testTable] VALUES (NULL);",formatter.Format(row));
        }
        [Fact]
        public void emptyString2nonNullableInt()
        {
            schema.Add(new dcp.lib.Formatters.Sql.Column(1, false , "int", "colname"));
            var row = new dcp.lib.Row();
            row.Add("");
            Assert.Equal("INSERT INTO [testTable] VALUES (DEFAULT);",formatter.Format(row));
        }
        [Fact]
        public void datetime2datetime()
        {
            schema.Add(new dcp.lib.Formatters.Sql.Column(1, false, "datetime", "colname"));
            var row = new dcp.lib.Row();
            row.Add(new DateTime(1961,3,14,1,2,3));
            Assert.Equal("INSERT INTO [testTable] VALUES ('1961-03-14 01:02:03');",formatter.Format(row));
        }
        [Fact]
        public void null2nullableDatetime()
        {
            schema.Add(new dcp.lib.Formatters.Sql.Column(1, true, "datetime", "colname"));
            var row = new dcp.lib.Row();
            row.Add(null);
            Assert.Equal("INSERT INTO [testTable] VALUES (NULL);",formatter.Format(row));
        }
        [Fact]
        public void null2nonNullableDatetime()
        {
            schema.Add(new dcp.lib.Formatters.Sql.Column(1, false, "datetime", "colname"));
            var row = new dcp.lib.Row();
            row.Add(null);
            var ex = Record.Exception(() => formatter.Format(row));

            Assert.IsType<ArgumentNullException>(ex);
            Assert.Contains(schema[0].columnName, ex.Message);
        }
        [Fact]
        public void string2datetime()
        {
            schema.Add(new dcp.lib.Formatters.Sql.Column(1, false, "datetime", "colname"));
            var row = new dcp.lib.Row();
            row.Add("1961-03-14 01:02:03");
            Assert.Equal("INSERT INTO [testTable] VALUES ('1961-03-14 01:02:03');",formatter.Format(row));
        }
        [Fact]
        public void emptyString2nullableDatetime()
        {
            schema.Add(new dcp.lib.Formatters.Sql.Column(1, true , "datetime", "colname"));
            var row = new dcp.lib.Row();
            row.Add(" ");
            Assert.Equal("INSERT INTO [testTable] VALUES (NULL);",formatter.Format(row));
        }
        [Fact]
        public void emptyString2nonNullableDatetime()
        {
            schema.Add(new dcp.lib.Formatters.Sql.Column(1, false, "datetime", "colname"));
            var row = new dcp.lib.Row();
            row.Add("");
            Assert.Equal("INSERT INTO [testTable] VALUES (DEFAULT);",formatter.Format(row));
        }

        [Fact]
        public void string2string()
        {
            schema.Add(new dcp.lib.Formatters.Sql.Column(1, true , "varchar", "colname"));
            var row = new dcp.lib.Row();
            row.Add("\"here\"");

            Assert.Equal("INSERT INTO [testTable] VALUES ('here');",formatter.Format(row));
        }
        [Fact]
        public void null2nullableString()
        {
            schema.Add(new dcp.lib.Formatters.Sql.Column(1, true , "varchar", "colname"));
            var row = new dcp.lib.Row();
            row.Add(null);

            Assert.Equal("INSERT INTO [testTable] VALUES (NULL);",formatter.Format(row));
        }
        [Fact]
        public void null2nonNullableString()
        {
            schema.Add(new dcp.lib.Formatters.Sql.Column(1, false , "varchar", "colname"));
            var row = new dcp.lib.Row();
            row.Add(null);

            var ex = Record.Exception(() => formatter.Format(row));

            Assert.IsType<ArgumentNullException>(ex);
            Assert.Contains(schema[0].columnName, ex.Message);
        }
        [Fact]
        public void emptyString2string()
        {
            schema.Add(new dcp.lib.Formatters.Sql.Column(1, true , "varchar", "colname"));
            var row = new dcp.lib.Row();
            row.Add("");

            Assert.Equal("INSERT INTO [testTable] VALUES ('');",formatter.Format(row));
        }
        [Fact]
        public void whitespaceString2string()
        {
            schema.Add(new dcp.lib.Formatters.Sql.Column(1, true, "varchar", "colname"));
            var row = new dcp.lib.Row();
            row.Add(" ");

            Assert.Equal("INSERT INTO [testTable] VALUES (' ');",formatter.Format(row));
        }
        [Fact]
        public void DoesNotQuoteStringTwice()
        {
            schema.Add(new dcp.lib.Formatters.Sql.Column(1, true, "varchar", "colname"));
            var row = new dcp.lib.Row();
            row.Add("\"here\"");

            Assert.Equal("INSERT INTO [testTable] VALUES ('here');",formatter.Format(row));
        }
        [Fact]
        public void EscapesStringWithSingleQuote()
        {
            schema.Add(new dcp.lib.Formatters.Sql.Column(1, true , "varchar", "colname"));
            var row = new dcp.lib.Row();
            row.Add("tw'o");

            Assert.Equal("INSERT INTO [testTable] VALUES ('tw''o');",formatter.Format(row));
        }
    }
}

