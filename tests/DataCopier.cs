using System;
using System.IO;
using System.Collections.Generic;
using System.Data.SqlClient;

using Xunit;
using dcp.lib;

namespace tests
{
    public class DataCopierTests
    {
        
        public DataCopierTests()
        {
        }
#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Dispose()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
        }
        [Fact]
        public void MakeWriterReturnsSqlServer()
        {
            var opts = new DataCopier.Options {};
            using (var writer = (new DataCopier()).makeWriter($"sql://{tests.SqlServerWriterTests.CONNECTIONSTRINGWITHOUTDATABASE}", opts))
            {
                Assert.True(writer is dcp.lib.Writers.SqlServer);
            }
        }
        [Fact]
        public void MakeReaderReturnsXlsx()
        {
            var opts = new DataCopier.Options {};
            using (var reader = (new DataCopier()).makeReader(tests.XlsxTests.FILENAME, opts))
            {
                Assert.True(reader is dcp.lib.Readers.Xlsx);
            }
        }
        [Fact]
        public void ConvertReturnsCorrectRowCount()
        {
            var opts = new DataCopier.Options {};
            opts.outputTable = tests.SqlServerWriterTests.TABLENAME;

            var count = DataCopier.Convert(
                opts,
                tests.XlsxTests.FILENAME,
                $"sql://{tests.SqlServerWriterTests.CONNECTIONSTRINGWITHDATABASE}"
            );
            Assert.Equal(3,count);
        }
    }
}

