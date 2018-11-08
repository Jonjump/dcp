using System;
using System.IO;
using System.Collections.Generic;
using OfficeOpenXml;

namespace dcp.lib.Readers
{
    public class Xlsx : Reader
    {
        private readonly Stream stream;
        private readonly DataCopier.Options opts;
        private readonly ExcelPackage excel;
        private readonly ExcelWorksheet worksheet;
        private int nextRowNumber;
        private readonly int lastRowNumber;
        private readonly int startColumnNumber;
        private readonly int endColumnNumber;
       public Xlsx(Stream stream, DataCopier.Options opts)
       {
           this.opts = opts;
           this.stream = stream;
           this.excel = new ExcelPackage(stream);
           this.worksheet = excel.Workbook.Worksheets[this.opts.worksheetNumber];
           if (this.worksheet.Dimension == null) throw new Exception($"no data in worksheet {this.opts.worksheetNumber}");

           nextRowNumber = this.opts.skipRows + 1;
           lastRowNumber = this.worksheet.Dimension.End.Row - this.opts.leaveRows;

           startColumnNumber = this.opts.skipColumns + 1;
           endColumnNumber = this.worksheet.Dimension.End.Column;
           if (this.opts.maxColumns!= null) endColumnNumber = Math.Min((int)this.opts.maxColumns,endColumnNumber);
       }
       public void Dispose ()
       {
           excel.Dispose();
           stream.Dispose();
       } 
       public List<Row> Next(int rowsToRead)
       {
           var rows = new List<Row>();
           for (var i=0; i< rowsToRead; i++)
           {
               var row = readRow();
               if (row == null) break;
               rows.Add(row);
           }
           return rows;
       }
       private Row readRow()
       {
           if (nextRowNumber>lastRowNumber) return null;
           var row = new Row();
           for (var columnNumber = startColumnNumber; columnNumber<= endColumnNumber; columnNumber++)
           {
               var cell = worksheet.Cells[nextRowNumber,columnNumber];
               var value = cell.Value;
               var format = cell.Style.Numberformat.Format;
               row.Add(value);
           }
           nextRowNumber +=1;
           return row;
       } 
    }
}
