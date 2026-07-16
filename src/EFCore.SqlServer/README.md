`Microsoft.EntityFrameworkCore.SqlServer` is the EF Core database provider package for Microsoft SQL Server and Azure SQL.

## Usage

Call the `UseSqlServer` method to choose the SQL Server/Azure SQL database provider for your `DbContext`. For example:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MyDatabase;Trusted_Connection=True;");
}
```

## Getting started with EF Core

See [Getting started with EF Core](https://learn.microsoft.com/ef/core/get-started/overview/install) for more information about EF NuGet packages, including which to install when getting started.

## Additional documentation

See [Microsoft SQL Server EF Core Database Provider](https://learn.microsoft.com/ef/core/providers/sql-server/) for more information about the features of this database provider.

## Feedback

If you encounter a bug or issues with this package,you can [open an Github issue](https://github.com/dotnet/efcore/issues/new/choose). For more details, see [getting support](https://github.com/dotnet/efcore/blob/main/.github/SUPPORT.md).