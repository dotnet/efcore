`Microsoft.Data.Sqlite` is a lightweight [ADO.NET provider]([ADO.NET](https://docs.microsoft.com/dotnet/framework/data/adonet/)) for [SQLite](https://www.sqlite.org/index.html). The EF Core provider for SQLite is built on top of this library. However, it can also be used independently or with other data access libraries.

### Installation

The latest stable version is available on [NuGet](https://www.nuget.org/packages/Microsoft.Data.Sqlite).

```sh
dotnet add package Microsoft.Data.Sqlite
```

Use the `--version` option to specify a [preview version](https://www.nuget.org/packages/Microsoft.Data.Sqlite/absoluteLatest) to install.

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

### Getting support

If you have a specific question about using these projects, we encourage you to [ask it on Stack Overflow](https://stackoverflow.com/questions/tagged/microsoft.data.sqlite). If you encounter a bug or would like to request a feature, [submit an issue](https://github.com/dotnet/efcore/issues/new/choose). For more details, see [getting support](.github/SUPPORT.md).

