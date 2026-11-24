The `Microsoft.EntityFrameworkCore.Sqlite.Core` package contains the code for the SQLite EF Core database provider. However, it does not automatically bring in any SQLite native binary, instead requiring that the application install and initialize the binary to use.

## Usage

Only use this package if you need to change to a different SQLite native binary that the one supplied by [Microsoft.EntityFrameworkCore.Sqlite](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Sqlite).

To use this "Core" package, also install a [SQLite binary package](https://www.nuget.org/profiles/SQLitePCLRaw) and initialize it with `SQLitePCL.Batteries_V2.Init();` or similar. See [github.com/ericsink/SQLitePCL.raw](https://github.com/ericsink/SQLitePCL.raw) for more information.

Following this, call `UseSqlite` just as you when using [Microsoft.EntityFrameworkCore.Sqlite](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Sqlite).

## Getting started with EF Core

See [Getting started with EF Core](https://learn.microsoft.com/ef/core/get-started/overview/install) for more information about EF NuGet packages, including which to install when getting started.

## Additional documentation

See [SQLite EF Core Database Provider](https://learn.microsoft.com/ef/core/providers/sqlite/) for more information about the features of this database provider.

## Feedback

If you encounter a bug or issues with this package,you can [open an Github issue](https://github.com/dotnet/efcore/issues/new/choose). For more details, see [getting support](https://github.com/dotnet/efcore/blob/main/.github/SUPPORT.md).