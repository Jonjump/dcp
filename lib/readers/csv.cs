using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using CsvHelper;

namespace dcp.lib.Readers
{
    public class Csv : Reader
    {
        readonly Stream stream;
        readonly TextReader textReader;
        readonly CsvReader csv;
        readonly DataCopier.Options opts;
        int rowsRead = 0;
        // private readonly int lastRowNumber;
        public Csv(Stream stream, DataCopier.Options opts)
        {
            this.opts = opts;
            this.stream = stream;
            this.textReader = new StreamReader(this.stream);
            this.csv = new CsvReader(textReader);
            csv.Configuration.HasHeaderRecord = false;

            // skiprows
            while ((rowsRead<this.opts.skipRows) && csv.Read()) { rowsRead +=1; }
        }
        public void Dispose()
        {
            csv.Dispose();
            textReader.Dispose();
            stream.Dispose();
        }
        public List<Row> Next(int rowsToRead)
        {
            var rows = new List<Row>();
            for (var i = 0; i < rowsToRead; i++)
            {
                var row = readRow();
                if (row == null) break;
                rows.Add(row);
            }
            return rows;
        }
        private Row readRow()
        {
            if (!csv.Read()) return null;
            var context = csv.Context;
            var record = context.Record;

            var fields = record.ToList().Skip(opts.skipColumns);
            if (opts.maxColumns!= null) fields = fields.Take((int)opts.maxColumns);

            Row row = new Row();
            foreach (Object field in fields) { row.Add(field);}

            if (row.Count == 0) return null;

            rowsRead +=1;
            return row;
        }
    }
}
