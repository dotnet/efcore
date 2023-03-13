Microsoft.EntityFrameworkCore.SqlServer is Microsoft SQL Server EF Core Database Provider, which enables you to use Entity Framework Core with Microsoft SQL Server (including Azure SQL Database).

## Getting started

Explain how to use your package, provide clear and concise getting started instructions, including any necessary steps.

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

The SQL Server provider for Entity Framework Core supports a variety of features, including:

- Basic CRUD operations
- Transactions
- Stored procedures
- Views
- Table-valued functions
- Identity columns
- In-memory tables

## Additional documentation

For more information on using the SQL Server provider for Entity Framework Core, you can refer to the official [documentation](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/?tabs=dotnet-core-cli).

## Feedback

If you encounter a bug or would like to request a feature, [submit an Github issue](https://github.com/dotnet/efcore/issues/new/choose). For more details, see [getting support](https://github.com/dotnet/efcore/blob/main/.github/SUPPORT.md).