using System;
using System.Collections.Generic;

namespace dcp.lib
{
    public abstract class Writer : IDisposable
    {
        public abstract void Next(List<Row> row);
        public abstract void Dispose();

    }
}
