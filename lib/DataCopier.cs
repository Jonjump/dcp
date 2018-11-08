using System;
using System.Collections.Generic;
using System.IO;

namespace dcp.lib
{
    public class DataCopier : IDisposable
    {
        [Serializable]
        public class WriteRowsException : Exception
        {
            public WriteRowsException() { }
            public WriteRowsException(int start, int end, Exception e) 
            : base( $"Write error between rows {start} and {end}: {e.Message}", e)
            { }
        }
        public class Options
        {
            public int bufferRows = 1;
            // writer options
            public string inputTable;
            public string outputTable;
            public bool truncate = false;
            // reader options
            public int skipRows = 0;
            public int leaveRows = 0;
            public int skipColumns = 0;
            public int? maxColumns = null;
            public int worksheetNumber = 0; // indexed from 0 in .net core, previously 1
        };
        private Reader reader;
        private Writer writer;
        private Options opts;
        public DataCopier()
        {
        }
        public DataCopier(Options opts, string readerUrl, string writerUrl)
        {
            this.opts = opts;
            this.opts.bufferRows = Math.Max(1, opts.bufferRows);

            reader = makeReader(readerUrl, opts);
            writer = makeWriter(writerUrl, opts);
        }
        public static int Convert(Options opts, string readerUrl, string writerUrl)
        {
            using (var converter = new DataCopier(opts, readerUrl, writerUrl))
            {
                return converter.Convert();
            }
        }
        public int Convert()
        {
            var total = 0;
            var rows = reader.Next(opts.bufferRows);
            do
            {
                try
                {
                    writer.Next(rows);
                }
                catch (Exception e)
                {
                    throw new WriteRowsException(total+opts.skipRows+1, total+opts.skipRows+opts.bufferRows, e);
                }
                total += rows.Count;
                rows = reader.Next(opts.bufferRows);
            } while (rows.Count > 0);
            return total;
        }
        public void Dispose()
        {
            reader.Dispose();
            writer.Dispose();
        }
        internal Reader makeReader(string readerUrl, Options opts)
        {
            var url = splitUrl(readerUrl);

            switch (url.scheme)
            {
                case "sql": 
                    {
                        return new Readers.SqlServer(url.remainder, opts);
                    }
                case "oracle": throw new NotImplementedException();
                case "file":
                    switch (url.ext.ToLower())
                    {
                        case "xlsx": return new Readers.Xlsx(getReadStream(url.scheme, url.remainder), opts);
                        case "csv": return new Readers.Csv(getReadStream(url.scheme, url.remainder), opts);
                        default: throw new ArgumentException($"{url.ext} is not a recognised file type");
                    }
                default: throw new ArgumentException($"{readerUrl} is not a recognised writer type");
            }

        }
        internal Writer makeWriter(string writerUrl, Options opts)
        {
            switch (writerUrl.ToLower())
            {
                default: break;
            }


            var url = splitUrl(writerUrl);

            switch (url.scheme)
            {
                case "sql":
                    {
                        return new Writers.SqlServer(url.remainder, opts);
                    }
                case "oracle": throw new NotImplementedException();
                case "file":
                    switch (url.ext.ToLower())
                    {
                        case "csv": return new Writers.Csv(getWriteStream(url.scheme, url.remainder), opts);
                        default: throw new ArgumentException($"{url.ext} is not a recognised file type");
                    }
                default: throw new ArgumentException($"{writerUrl} is not a recognised writer type");
            }

        }
        (string scheme, string remainder, string ext) splitUrl(string url)
        {
            string scheme, remainder;
            var bits = url.Trim().Split(new string[] { "://" }, StringSplitOptions.None);
            if (bits.Length == 1)
            {
                scheme = "file";
                remainder = bits[0];
            }
            else
            {
                scheme = bits[0];
                remainder = bits[1];
            }

            var pieces = new List<string>(remainder.Split('.'));
            var ext = (pieces.Count == 1) ? "" : pieces[pieces.Count - 1];

            return (scheme, remainder, ext);
        }
        Stream getReadStream(string scheme, string remainder)
        {
            switch (scheme)
            {
                case "file":
                    {
                        return new System.IO.FileStream(remainder, FileMode.Open);
                    }
                default: throw new ArgumentException($"cannot create a stream for '{scheme}");
            }
        }
        Stream getWriteStream(string scheme, string remainder)
        {
            switch (scheme)
            {
                case "file":
                    {
                        return File.Create(remainder);
                    }
                default: throw new ArgumentException($"cannot create a stream for '{scheme}");
            }
        }
    }
}