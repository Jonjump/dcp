using System;
using CommandLine;
using CommandLine.Text;
using System.Collections.Generic;
using dcp.lib;

namespace dcp
{
    public class cliOptions
    {

        [Value(0, Required=true,HelpText = "url to read from",MetaName="input file")]
        public string readerUrl { get; set; }
        [Value(1, Required=true,HelpText = "url to write to",MetaName="output file")]
        public string writerUrl { get; set; }
        [Option('b', "bufferRows", Required = false, HelpText = "number of rows to buffer (default 100)")]
        public int bufferRows { get; set; } = 100;
        // writer options
        [Option('t', "truncate", Required = false, HelpText = "truncate output file/database before writing output")]
        public bool truncate { get; set; } = false;
        [Option('i', "inputTable", Required = false, HelpText = "database table name to read from")]
        public string inputTable { get; set; }
        // reader options
        [Option('o', "outputTable", Required = false, HelpText = "database table name to write to")]
        public string outputTable { get; set; }
        [Option('s', "skipRows", Required = false, HelpText = "rows to skip on read")]
        public int skipRows { get; set; } = 0;
        [Option('l', "leaveRows", Required = false, HelpText = "rows at end to ignore on read")]
        public int leaveRows { get; set; } = 0;
        [Option('c', "skipColumns", Required = false, HelpText = "columns at start to skip")]
        public int skipColumns { get; set; } = 0;
        [Option('m', "maxColumns", Required = false, HelpText = "maximum number of columns to read")]
        public int? maxColumns { get; set; } = null;
        [Option('w', "worksheet", Required = false, HelpText = "spreadsheet worksheet number (index from 0)")]
        public int worksheetNumber { get; set; } = 0; // indexed from 0 in .net core, previously 1
        // cli options
        [Option('v', "verbose", Required = false, HelpText = "verbose, including stats")]
        public bool verbose { get; set; } = false; // indexed from 0 in .net core, previously 1

    }
    class Program
    {
        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<cliOptions>(args)
            .WithParsed<cliOptions>(cliOpts =>
            {
                var opts = new DataCopier.Options {
                    bufferRows = cliOpts.bufferRows,
                    //writer options
                    truncate = cliOpts.truncate,
                    inputTable = String.IsNullOrWhiteSpace(cliOpts.inputTable) ? null : cliOpts.inputTable.Trim(),
                    // reader options
                    outputTable = String.IsNullOrWhiteSpace(cliOpts.outputTable) ? null : cliOpts.outputTable.Trim(),
                    skipRows = cliOpts.skipRows,
                    leaveRows = cliOpts.leaveRows,
                    skipColumns = cliOpts.skipColumns,
                    maxColumns = cliOpts.maxColumns,
                    worksheetNumber = cliOpts.worksheetNumber
                };
                var startTime = DateTime.Now;
                int lines = 0;
                try {
                    lines = DataCopier.Convert(opts,cliOpts.readerUrl.Trim(), cliOpts.writerUrl.Trim());
                }
                catch (ArgumentException e) {
                    Console.Error.WriteLine(e.Message);
                }
                var endTime = DateTime.Now;
                var elapsedSeconds = (endTime - startTime).TotalMilliseconds/1000F;
                Console.WriteLine($"{lines} lines copied in {elapsedSeconds} seconds ({lines/elapsedSeconds} lines per second).");
                // Console.WriteLine($"{.readerUrl} {opts.writerUrl}");
            })
            .WithNotParsed<cliOptions>((errs) =>
            {
                // HelpText.AutoBuild<cliOptions>()
            });
        }
    }
}
