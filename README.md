# Entity Framework Core

Documentation on using EF Core is available at <https://docs.microsoft.com/ef/core/>.

[![Build Status](https://dnceng.visualstudio.com/public/_apis/build/status/aspnet/EntityFrameworkCore/EntityFrameworkCore-ci)](https://dnceng.visualstudio.com/public/_build/latest?definitionId=51)

## EF Core here, EF6 elsewhere

This project is for Entity Framework Core. Entity Framework 6 is still under active development at https://github.com/aspnet/EntityFramework6.

## What is EF Core?

Entity Framework (EF) Core is a lightweight and extensible version of the popular Entity Framework data access technology.

EF Core is an object-relational mapper (O/RM) that enables .NET developers to work with a database using .NET objects. It eliminates the need for most of the data-access code that developers usually need to write.

## Weekly status updates

See the [weekly status updates issue](https://github.com/aspnet/EntityFrameworkCore/issues/15403) to keep up-to-date on what is happening in the world of EF Core.

## Database Providers

The source for SQL Server, SQLite, and InMemory providers are included in this project. Additional providers are available.
For a complete list, see https://docs.microsoft.com/ef/core/providers/.

Provider               | Package name                              | Stable                      
-----------------------|-------------------------------------------|-----------------------------
SQL Server             | `Microsoft.EntityFrameworkCore.SqlServer` | [![NuGet](https://img.shields.io/nuget/v/Microsoft.EntityFrameworkCore.SqlServer.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer/) 
SQLite                 | `Microsoft.EntityFrameworkCore.SQLite`    | [![NuGet](https://img.shields.io/nuget/v/Microsoft.EntityFrameworkCore.SqlServer.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Sqlite/) 
InMemory (for testing) | `Microsoft.EntityFrameworkCore.InMemory`  | [![NuGet](https://img.shields.io/nuget/v/Microsoft.EntityFrameworkCore.InMemory.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.InMemory/) 

## Nightly builds

[Nightly builds](https://github.com/aspnet/AspNetCore/blob/master/docs/DailyBuilds.md) are a great way to validate bugs are fixed and try out new features.

## Project Wiki

More details about our project, like our release [roadmap](https://docs.microsoft.com/ef/core/what-is-new/roadmap), or [how to get and build our code](https://github.com/aspnet/EntityFrameworkCore/wiki/getting-and-building-the-code), are located in our [project wiki](https://github.com/aspnet/EntityFrameworkCore/wiki/).

## Building from source

To run a complete build on command line only, execute `build.cmd` or `build.sh` without arguments.
This will execute only the part of the build script that downloads and initializes a few required build tools and packages.

See [developer documentation](https://github.com/aspnet/EntityFrameworkCore/wiki/Getting-and-Building-the-Code) for more details.

# Code of conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).  For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

