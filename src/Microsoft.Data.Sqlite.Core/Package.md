The `Microsoft.Data.Sqlite.Core` package contains the code for SQLite ADO.NET driver. However, it does not automatically bring in any SQLite native binary, instead requiring that the application install and initialize the binary to use.

## Usage

Only use this package if you need to change to a different SQLite native binary that the one supplied by [Microsoft.Data.Sqlite](https://www.nuget.org/packages/Microsoft.Data.Sqlite).

To use this "Core" package, also install a [SQLite binary package](https://www.nuget.org/profiles/SQLitePCLRaw) and initialize it with `SQLitePCL.Batteries_V2.Init();` or similar. See [github.com/ericsink/SQLitePCL.raw](https://github.com/ericsink/SQLitePCL.raw) for more information.

### Getting support

If you have a specific question about using these projects, we encourage you to [ask it on Stack Overflow](https://stackoverflow.com/questions/tagged/microsoft.data.sqlite). If you encounter a bug or would like to request a feature, [submit an issue](https://github.com/dotnet/efcore/issues/new/choose). For more details, see [getting support](.github/SUPPORT.md).

