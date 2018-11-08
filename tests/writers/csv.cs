using System;
using System.IO;
using System.Collections.Generic;
using System.Data.SqlClient;

using Xunit;
using dcp.lib.Writers;

namespace tests
{
    public class CsvWriterTests
    {

        const string TESTFILE="test.csv";
        FileStream stream;
        dcp.lib.Row row1;
        dcp.lib.Row row2;
        public CsvWriterTests()
        {
            row1 = new dcp.lib.Row();
            row1.Add(1);
            row1.Add("two");
            row1.Add(new DateTime(1961, 3, 14, 1,2,3));
            row2 = new dcp.lib.Row();
            row2.Add(1);
            row2.Add("two");
            row2.Add(new DateTime(1961, 3, 14, 1,2,3));
            stream = File.Create(TESTFILE);
        }
#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Dispose()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
            stream.Dispose();
            File.Delete(TESTFILE);
        }
        [Fact]
        public void WritesRow()
        {
            var opts = new dcp.lib.DataCopier.Options();
            var rows = new List<dcp.lib.Row>() {row1};
            writeRows(opts, rows);

            Assert.Equal("1,two,14/03/1961 01:02:03\r\n", getResult());
        }
        [Fact]
        public void EscapesStringWithQuote()
        {
            var row = new dcp.lib.Row();
            row.Add("quoted\"string");
            var opts = new dcp.lib.DataCopier.Options();
            var rows = new List<dcp.lib.Row>() {row};
            writeRows(opts, rows);

            Assert.Equal("\"quoted\"\"string\"\r\n", getResult());
        }
        [Fact]
        public void EscapesStringWithComma()
        {
            var row = new dcp.lib.Row();
            row.Add("comma,string");
            var opts = new dcp.lib.DataCopier.Options();
            var rows = new List<dcp.lib.Row>() {row};
            writeRows(opts, rows);

            Assert.Equal("\"comma,string\"\r\n", getResult());
        }
        [Fact]
        public void EscapesNewLine()
        {
            var row = new dcp.lib.Row();
            row.Add("newline\nstring");
            var opts = new dcp.lib.DataCopier.Options();
            var rows = new List<dcp.lib.Row>() {row};
            writeRows(opts, rows);

            Assert.Equal("\"newline\nstring\"\r\n", getResult());
        }
        void writeRows(dcp.lib.DataCopier.Options opts, List<dcp.lib.Row> rows)
        {
            using (var writer = new Csv(stream,opts))
            {
                writer.Next(rows);
            }
        }
        string getResult()
        {
            return File.ReadAllText(TESTFILE);
        }
    }
}

