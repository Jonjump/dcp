using System;
using System.IO;
using System.Collections.Generic;
using OfficeOpenXml;
using Xunit;
using dcp.lib;
using dcp.lib.Readers;

namespace tests
{
    public class CsvTests
    {
        public const string CSVDATA1 = "1,string,\"quotedString\",1961-14-03";
        public const string CSVDATA2 = "2,string2,\"quotedString2\",1961-14-04";
        private readonly Stream stream;
        private readonly StreamWriter writer;
        public CsvTests()
        {
            stream = new MemoryStream();
            writer = new StreamWriter(stream);
            writer.WriteLine(CSVDATA1);
            writer.WriteLine(CSVDATA2);
            writer.Flush();
            stream.Position = 0;
        }
#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Dispose()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
            writer.Dispose();
            stream.Dispose();
        }
        [Fact]
        public void ReadsFirstColumn()
        {
            var opts = new DataCopier.Options();
            using (var reader = new Csv(stream, opts))
            {
                var rows = reader.Next(1);
                Assert.Equal("1", rows[0][0]);
            }
        }
        [Fact]
        public void ReadsAllColumns()
        {
            var opts = new dcp.lib.DataCopier.Options();
            using (var reader = new Csv(stream, opts))
            {
                var row = reader.Next(1);
                Assert.Equal(4, row[0].Count);
            }
        }
        [Fact]
        public void SkipsColumns()
        {
            var opts = new dcp.lib.DataCopier.Options();
            opts.skipColumns = 1;
            using (var reader = new Csv(stream, opts))
            {
                var rows = reader.Next(1);
                Assert.Equal("string", rows[0][0]);
            }
        }
        [Fact]
        public void UsesMaxColumns()
        {
            var opts = new dcp.lib.DataCopier.Options();
            opts.maxColumns = 1;
            using (var reader = new Csv(stream, opts))
            {
                var rows = reader.Next(1);
                Assert.Equal("1", rows[0][0]);
                Assert.Single(rows[0]);
            }
        }
        [Fact]
        public void ReturnsCorrectNumberOfRows()
        {
            var opts = new dcp.lib.DataCopier.Options();
            using (var reader = new Csv(stream, opts))
            {
                var rows = reader.Next(3);
                Assert.Equal(2, rows.Count);
            }
        }
        [Fact]
        public void ReturnsShortListIfNotEnoughRows()
        {
            var opts = new dcp.lib.DataCopier.Options();
            using (var reader = new Csv(stream, opts))
            {
                var rows = reader.Next(9);
                Assert.Equal(2, rows.Count);
            }
        }
        // [Fact]
        // public void LeavesRows()
        // {
        //     var opts = new dcp.lib.DataCopier.Options();
        //     opts.leaveRows = 1;
        //     using (var reader = new Xlsx(stream, opts))
        //     {
        //         var rows = reader.Next(9);
        //         Assert.Equal(2, rows.Count);
        //     }
        // }
    }
}
