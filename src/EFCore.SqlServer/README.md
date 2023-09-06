Microsoft.EntityFrameworkCore.SqlServer is the database provider for Microsoft SQL Server and Azure SQL. This providers allows you to use Entity Framework Core with Microsoft SQL Server and Azure SQL databases.

## Getting started

`Microsoft.EntityFrameworkCore.SqlServer` is the EF Core provider for Microsoft SQL Server and Azure SQL. See [Getting Started](https://learn.microsoft.com/ef/core/modeling/#use-fluent-api-to-configure-a-model) for more information.

### Prerequisites

- Supported database Engines: Microsoft SQL Server (2012 onwards)
- The provider references Microsoft.Data.SqlClient (not System.Data.SqlClient). If your project takes a direct dependency on SqlClient, make sure it references the Microsoft.Data.SqlClient package.

## Usage

Once you've installed the package, you can use it in your Entity Framework Core application by specifying the SQL Server provider in your DbContext's OnConfiguring method:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MyDatabase;Trusted_Connection=True;");
}
```

In this example, we're using the (localdb)\mssqllocaldb server with the MyDatabase database. You'll need to adjust the connection string to match your own SQL Server instance and database.

## Features

The SQL Server provider  supports all common features of [Entity Framework Core](https://learn.microsoft.com/ef/core/) as well as some [SQL Server-specific features](https://learn.microsoft.com/ef/core/providers/sql-server/?tabs=dotnet-core-cli) including temporal tables and memory-optimized tables.

## Additional documentation

For more information on using the SQL Server provider for Entity Framework Core, you can refer to the official [documentation](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/?tabs=dotnet-core-cli).

## Feedback

If you encounter a bug or would like to request a feature, [submit an Github issue](https://github.com/dotnet/efcore/issues/new/choose). For more details, see [getting support](https://github.com/dotnet/efcore/blob/main/.github/SUPPORT.md).