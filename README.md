# dcp
Copy bulk data to/from databases, csv, Excel.   

I wanted a utility to move data between various places, without having to write code or document schemas.  It works as a library, or via the command line tool.  It does that by reading what schema information is available from the source and target and converting where necessary.  

Thanks to making use of good libraries, on a decent network you should move several thousand lines a second.

## Getting Started
dcp is the command line tool.  The libraries are in lib. 
## Command Line 

dotnet dcp.dll -- options inputfile outputfile

  -b, --bufferRows        number of rows to buffer (default 100)

  -t, --truncate          truncate output file/database before writing output

  -i, --inputTable        database table name to read from

  -o, --outputTable       database table name to write to

  -s, --skipRows          rows to skip on read

  -l, --leaveRows         rows at end to ignore on read

  -c, --skipColumns       columns at start to skip

  -m, --maxColumns        maximum number of columns to read

  -w, --worksheet         spreadsheet worksheet number (index from 0)

  -v, --verbose           verbose, including stats

  --help                  Display this help screen.

  --version               Display version information.

  inputfile      Required. uri to read from

  outputfile     Required. uri to write to

### File formats
Files with extensions csv, xlsx will just work.   For a database source or target, the filename format is sql://connectionString.  Typically, you will be using something like "sql://Server=SERVERNAME;Database=DATABASENAME;Trusted_Connection=True;"

## FAQ
"My data copy fails, but I am not getting a specific input line number for the error" : set the buffer to 1 ("-b 1"), which will be slower, but will give you a specific line number for the problem data.

"My network is very slow and things time out" : reduce the buffer size (which is 100 by default).   Try -b 10 to start with.

"My data imports with the wrong format to my database" : obviously there is no limit to the number of odd formats people can give you in a csv file or excel.   Import it to a staging table, and then fix the format in SQL when you copy to the target table.

### Prerequisites

You will need [dotnet core](https://dotnet.microsoft.com/download)

## ToDo
Oracle, Postgres and MySql database readers and writers.

## Tests
You will need a sql server to run the sqlserver tests. A local Sqlexpress works fine.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments
