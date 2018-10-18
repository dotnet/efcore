# Entity Framework Core

Documentation on using EF Core is available at <https://docs.microsoft.com/ef/core/>.

## EF Core here, EF6 elsewhere

This project is for Entity Framework Core. Entity Framework 6 is still under active development at https://github.com/aspnet/EntityFramework6.

## What is EF Core?

Entity Framework (EF) Core is a lightweight and extensible version of the popular Entity Framework data access technology.

EF Core is an object-relational mapper (O/RM) that enables .NET developers to work with a database using .NET objects. It eliminates the need for most of the data-access code that developers usually need to write.

## Database Providers

The source for SQL Server, SQLite, and InMemory providers are included in this project. Additional providers are available.
For a complete list, see https://docs.microsoft.com/ef/core/providers/.

Provider               | Package name                              | Stable                      | Nightly (master branch)
-----------------------|-------------------------------------------|-----------------------------|-------------------------
SQL Server             | `Microsoft.EntityFrameworkCore.SqlServer` | [![NuGet](https://img.shields.io/nuget/v/Microsoft.EntityFrameworkCore.SqlServer.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer/) | [![MyGet](https://img.shields.io/dotnet.myget/aspnetcore-dev/vpre/Microsoft.EntityFrameworkCore.SqlServer.svg?style=flat-square&label=myget)](https://dotnet.myget.org/feed/aspnetcore-dev/package/nuget/Microsoft.EntityFrameworkCore.SqlServer)
SQLite                 | `Microsoft.EntityFrameworkCore.SQLite`    | [![NuGet](https://img.shields.io/nuget/v/Microsoft.EntityFrameworkCore.SqlServer.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Sqlite/) | [![MyGet](https://img.shields.io/dotnet.myget/aspnetcore-dev/vpre/Microsoft.EntityFrameworkCore.Sqlite.svg?style=flat-square&label=myget)](https://dotnet.myget.org/feed/aspnetcore-dev/package/nuget/Microsoft.EntityFrameworkCore.Sqlite)
InMemory (for testing) | `Microsoft.EntityFrameworkCore.InMemory`  | [![NuGet](https://img.shields.io/nuget/v/Microsoft.EntityFrameworkCore.InMemory.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.InMemory/) | [![MyGet](https://img.shields.io/dotnet.myget/aspnetcore-dev/vpre/Microsoft.EntityFrameworkCore.InMemory.svg?style=flat-square&label=myget)](https://dotnet.myget.org/feed/aspnetcore-dev/package/nuget/Microsoft.EntityFrameworkCore.InMemory)

## Project Wiki

More details about our project, like our release [roadmap](https://docs.microsoft.com/ef/core/what-is-new/roadmap), or [how to get and build our code](https://github.com/aspnet/EntityFrameworkCore/wiki/getting-and-building-the-code), are located in our our [project wiki](https://github.com/aspnet/EntityFrameworkCore/wiki/).

## Building from source

To run a complete build on command line only, execute `build.cmd` or `build.sh` without arguments.
This will execute only the part of the build script that downloads and initializes a few required build tools and packages.

See [developer documentation](https://github.com/aspnet/EntityFrameworkCore/wiki/Getting-and-Building-the-Code) for more details.

# Code of conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).  For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

