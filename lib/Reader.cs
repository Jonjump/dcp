using System;
using System.IO;
using System.Collections.Generic;

namespace dcp.lib
{
    public interface Reader : IDisposable
    {
        List<Row> Next(int rowsToRead);
    }
}
