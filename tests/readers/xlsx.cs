using System;
using System.IO;
using System.Collections.Generic;
using OfficeOpenXml;
using Xunit;
using dcp.lib;
using dcp.lib.Readers;

namespace tests
{
    public class XlsxTests
    {
        public const string FILENAME = "../../../readers/files/test1.xlsx";
        private readonly Stream stream;
        public XlsxTests()
        {
            stream = new FileStream(FILENAME,FileMode.Open,FileAccess.Read);
        }
#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Dispose()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
            stream.Dispose();
        }
        [Fact]
        public void ReadsFirstRowValue()
        {
            var opts = new DataCopier.Options();
            using (var reader = new Xlsx(stream, opts))
            {
                var rows = reader.Next(1);
                Assert.Equal(1.1, rows[0][0]);
            }
        }
        [Fact]
        public void ReadsLastRowValue()
        {
            var opts = new dcp.lib.DataCopier.Options();
            using (var reader = new Xlsx(stream, opts))
            {
                var row = reader.Next(1);
                Assert.Equal(new DateTime(2031,12,1), row[0][2]);
            }
        }
        [Fact]
        public void ReadsSelectedWorksheet()
        {
            var opts = new dcp.lib.DataCopier.Options();
            opts.worksheetNumber = 1;
            using (var reader = new Xlsx(stream, opts))
            {
                var rows = reader.Next(1);
                Assert.Equal(new DateTime(9031,12,1), rows[0][2]);
            }
        }
        [Fact]
        public void SkipsColumns()
        {
            var opts = new dcp.lib.DataCopier.Options();
            opts.skipColumns=1;
            using (var reader = new Xlsx(stream, opts))
            {
                var rows = reader.Next(1);
                Assert.Equal("one", rows[0][0]);
            }
        }
        [Fact]
        public void UsesMaxColumns()
        {
            var opts = new dcp.lib.DataCopier.Options();
            opts.maxColumns=1;
            using (var reader = new Xlsx(stream, opts))
            {
                var rows = reader.Next(1);
                Assert.Equal(1.1, rows[0][0]);
                Assert.Single(rows[0]);
            }
        }
        [Fact]
        public void ReturnsCorrectNumberOfRows()
        {
            var opts = new dcp.lib.DataCopier.Options();
            using (var reader = new Xlsx(stream, opts))
            {
                var rows = reader.Next(3);
                Assert.Equal(3,rows.Count);
            }
        }
        [Fact]
        public void ReturnsShortListIfNotEnoughRows()
        {
            var opts = new dcp.lib.DataCopier.Options();
            using (var reader = new Xlsx(stream, opts))
            {
                var rows = reader.Next(9);
                Assert.Equal(3,rows.Count);
            }
        }
        [Fact]
        public void LeavesRows()
        {
            var opts = new dcp.lib.DataCopier.Options();
            opts.leaveRows = 1;
            using (var reader = new Xlsx(stream, opts))
            {
                var rows = reader.Next(9);
                Assert.Equal(2,rows.Count);
            }
        }
    }
}
