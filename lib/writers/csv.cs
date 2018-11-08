using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using CsvHelper;

namespace dcp.lib.Writers
{
    public class Csv : Writer
    {
        DataCopier.Options opts;
        private readonly Stream stream;
        private readonly TextWriter textWriter;
        private readonly CsvWriter csv;
        public Csv(Stream stream, DataCopier.Options opts)
        {
            this.opts = opts;
            this.stream = stream;
            this.textWriter = new StreamWriter(this.stream);
            this.csv = new CsvWriter(textWriter);
            csv.Configuration.HasHeaderRecord = false;
        }
       public override void Dispose()
       {
            csv.Dispose();
            textWriter.Dispose();
            stream.Dispose();
       }
       public override void Next (List<Row> rows)
       {
           foreach (var row in rows)
           {
               foreach(var field in row)
               {
                csv.WriteField(field);
               }
               csv.NextRecord();
           }
           csv.Flush();
           stream.Flush();
       } 
    }
}
