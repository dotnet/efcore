# Repository

[![build status](https://img.shields.io/azure-devops/build/dnceng-public/public/17/main)](https://dev.azure.com/dnceng-public/public/_build?definitionId=17) [![test results](https://img.shields.io/azure-devops/tests/dnceng-public/public/17/main)](https://dev.azure.com/dnceng-public/public/_build?definitionId=17)

This repository is home to the following [.NET Foundation](https://dotnetfoundation.org/) projects. These projects are maintained by [Microsoft](https://github.com/microsoft) and licensed under the [MIT License](LICENSE.txt).

* [Entity Framework Core](#entity-framework-core)
* [Microsoft.Data.Sqlite](#microsoftdatasqlite)

## <img alt="EF" src="./logo/ef-logo.png" width="32"/> Entity Framework Core

[![latest version](https://img.shields.io/nuget/v/Microsoft.EntityFrameworkCore)](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore) [![preview version](https://img.shields.io/nuget/vpre/Microsoft.EntityFrameworkCore)](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore/absoluteLatest) [![downloads](https://img.shields.io/nuget/dt/Microsoft.EntityFrameworkCore)](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore)

EF Core is a modern object-database mapper for .NET. It supports LINQ queries, change tracking, updates, and schema migrations. EF Core works with SQL Server, Azure SQL Database, SQLite, Azure Cosmos DB, MySQL, PostgreSQL, and other databases through a provider plugin API.

### Installation

EF Core is available on [NuGet](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore). Install the provider package corresponding to your target database. See the [list of providers](https://docs.microsoft.com/ef/core/providers/) in the docs for additional databases.

```sh
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Cosmos
```

Use the `--version` option to specify a [preview version](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore/absoluteLatest) to install.

### Daily builds

We recommend using the [daily builds](docs/DailyBuilds.md) to get the latest code and provide feedback on EF Core. These builds contain latest features and bug fixes; previews and official releases lag significantly behind.

### Basic usage

The following code demonstrates basic usage of EF Core. For a full tutorial configuring the `DbContext`, defining the model, and creating the database, see [getting started](https://docs.microsoft.com/ef/core/get-started/) in the docs.

```cs
using var db = new BloggingContext();

// Inserting data into the database
db.Add(new Blog { Url = "http://blogs.msdn.com/adonet" });
db.SaveChanges();

// Querying
var blog = db.Blogs
    .OrderBy(b => b.BlogId)
    .First();

// Updating
blog.Url = "https://devblogs.microsoft.com/dotnet";
blog.Posts.Add(
    new Post
    {
        Title = "Hello World",
        Content = "I wrote an app using EF Core!"
    });
db.SaveChanges();

// Deleting
db.Remove(blog);
db.SaveChanges();
```

### Build from source

Most people use EF Core by installing pre-build NuGet packages, as shown above. Alternately, [the code can be built and packages can be created directly on your development machine](./docs/getting-and-building-the-code.md).

### Contributing

We welcome community pull requests for bug fixes, enhancements, and documentation. See [How to contribute](./.github/CONTRIBUTING.md) for more information.

### Getting support

If you have a specific question about using these projects, we encourage you to [ask it on Stack Overflow](https://stackoverflow.com/questions/tagged/entity-framework-core*?tab=Votes). If you encounter a bug or would like to request a feature, [submit an issue](https://github.com/dotnet/efcore/issues/new/choose). For more details, see [getting support](.github/SUPPORT.md).

## Microsoft.Data.Sqlite

[![latest version](https://img.shields.io/nuget/v/Microsoft.Data.Sqlite)](https://www.nuget.org/packages/Microsoft.Data.Sqlite) [![preview version](https://img.shields.io/nuget/vpre/Microsoft.Data.Sqlite)](https://www.nuget.org/packages/Microsoft.Data.Sqlite/absoluteLatest) [![downloads](https://img.shields.io/nuget/dt/Microsoft.Data.Sqlite.Core)](https://www.nuget.org/packages/Microsoft.Data.Sqlite)

Microsoft.Data.Sqlite is a lightweight ADO.NET provider for SQLite. The EF Core provider for SQLite is built on top of this library. However, it can also be used independently or with other data access libraries.

### Installation

The latest stable version is available on [NuGet](https://www.nuget.org/packages/Microsoft.Data.Sqlite).

```sh
dotnet add package Microsoft.Data.Sqlite
```

Use the `--version` option to specify a [preview version](https://www.nuget.org/packages/Microsoft.Data.Sqlite/absoluteLatest) to install.

### Daily builds

We recommend using the [daily builds](docs/DailyBuilds.md) to get the latest code and provide feedback on Microsoft.Data.Sqlite. These builds contain latest features and bug fixes; previews and official releases lag significantly behind.

### Basic usage

This library implements the common [ADO.NET](https://docs.microsoft.com/dotnet/framework/data/adonet/) abstractions for connections, commands, data readers, and so on. For more information, see [Microsoft.Data.Sqlite](https://docs.microsoft.com/dotnet/standard/data/sqlite/) on Microsoft Docs.

```cs
using var connection = new SqliteConnection("Data Source=Blogs.db");
connection.Open();

using var command = connection.CreateCommand();
command.CommandText = "SELECT Url FROM Blogs";

using var reader = command.ExecuteReader();
while (reader.Read())
{
    var url = reader.GetString(0);
}
```

### Build from source

Most people use Microsoft.Data.Sqlite by installing pre-build NuGet packages, as shown above. Alternately, [the code can be built and packages can be created directly on your development machine](./docs/getting-and-building-the-code.md).

### Contributing

We welcome community pull requests for bug fixes, enhancements, and documentation. See [How to contribute](./.github/CONTRIBUTING.md) for more information.

### Getting support

If you have a specific question about using these projects, we encourage you to [ask it on Stack Overflow](https://stackoverflow.com/questions/tagged/microsoft.data.sqlite). If you encounter a bug or would like to request a feature, [submit an issue](https://github.com/dotnet/efcore/issues/new/choose). For more details, see [getting support](.github/SUPPORT.md).

## See also

* [Documentation](https://docs.microsoft.com/ef/core/)
* [Roadmap](https://docs.microsoft.com/ef/core/what-is-new/roadmap)
* [Weekly status updates](https://github.com/dotnet/efcore/issues/23884)
* [Release planning process](https://docs.microsoft.com/ef/core/what-is-new/release-planning)
* [How to write an EF Core provider](https://docs.microsoft.com/ef/core/providers/writing-a-provider)
* [Security](./docs/security.md)
* [Code of conduct](.github/CODE_OF_CONDUCT.md)
