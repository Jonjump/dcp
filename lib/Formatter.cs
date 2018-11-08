using System;
using System.Collections.Generic;

namespace dcp.lib
{
    public interface FormatterOptions { };
    public abstract class Formatter
    {
        public abstract Object Format(Row row);
        public Formatter()
        {
        }
    }
}
