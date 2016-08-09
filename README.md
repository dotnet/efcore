Entity Framework Core
=====================

Documentation on using EF Core is available at <https://docs.efproject.net>.

[![Travis build status](https://img.shields.io/travis/aspnet/EntityFramework.svg?label=travis-ci&branch=dev&style=flat-square)](https://travis-ci.org/aspnet/EntityFramework/branches)
[![AppVeyor build status](https://img.shields.io/appveyor/ci/aspnetci/EntityFramework/dev.svg?label=appveyor&style=flat-square)](https://ci.appveyor.com/project/aspnetci/entityframework/branch/dev)

## EF Core here, EF6 elsewhere

This project is for Entity Framework Core. Entity Framework 6.x is still under active development at https://github.com/aspnet/EntityFramework6.

## What is EF Core?

Entity Framework (EF) Core is a lightweight and extensible version of the popular Entity Framework data access technology.

EF Core is an object-relational mapper (O/RM) that enables .NET developers to work with a database using .NET objects. It eliminates the need for most of the data-access code that developers usually need to write. 

## Database Providers

The source for SQL Server, SQLite, and InMemory providers are included in this project. Additional providers are available.
For a complete list, see https://docs.efproject.net/en/latest/providers/.

Provider               | Package name                              | Stable (`master` branch)    | Nightly (`dev` branch)
-----------------------|-------------------------------------------|-----------------------------|-------------------------
SQL Server             | `Microsoft.EntityFrameworkCore.SqlServer` | [![NuGet](https://img.shields.io/nuget/v/Microsoft.EntityFrameworkCore.SqlServer.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer/) | [![MyGet](https://img.shields.io/dotnet.myget/aspnetcore-dev/vpre/Microsoft.EntityFrameworkCore.SqlServer.svg?style=flat-square&label=myget)](https://dotnet.myget.org/feed/aspnetcore-dev/package/nuget/Microsoft.EntityFrameworkCore.SqlServer)
SQLite                 | `Microsoft.EntityFrameworkCore.SQLite`    | [![NuGet](https://img.shields.io/nuget/v/Microsoft.EntityFrameworkCore.SqlServer.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Sqlite/) | [![MyGet](https://img.shields.io/dotnet.myget/aspnetcore-dev/vpre/Microsoft.EntityFrameworkCore.Sqlite.svg?style=flat-square&label=myget)](https://dotnet.myget.org/feed/aspnetcore-dev/package/nuget/Microsoft.EntityFrameworkCore.Sqlite)
InMemory (for testing) | `Microsoft.EntityFrameworkCore.InMemory`  | [![NuGet](https://img.shields.io/nuget/v/Microsoft.EntityFrameworkCore.InMemory.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.InMemory/) | [![MyGet](https://img.shields.io/dotnet.myget/aspnetcore-dev/vpre/Microsoft.EntityFrameworkCore.InMemory.svg?style=flat-square&label=myget)](https://dotnet.myget.org/feed/aspnetcore-dev/package/nuget/Microsoft.EntityFrameworkCore.InMemory)

## Roadmap
For more details on the release schedule, see the [**Roadmap**](https://github.com/aspnet/EntityFramework/wiki/Roadmap) article in the wiki.
