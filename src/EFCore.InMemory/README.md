`Microsoft.EntityFrameworkCore.InMemory` is the EF Core database provider package for the built-in in-memory database.

This database provider allows Entity Framework Core to be used with an in-memory database. While it has become common to use the in-memory database for testing, this is discouraged. For more information on how to test EF Core applications, see [Testing EF Core Applications](https://learn.microsoft.com/ef/core/testing/).

## Usage

Call the `UseInMemoryDatabase` method to choose the SQL Server/Azure SQL database provider for your `DbContext`. For example:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseInMemoryDatabase("MyDatabase");
}
```

## Getting started with EF Core

See [Getting started with EF Core](https://learn.microsoft.com/ef/core/get-started/overview/install) for more information about EF NuGet packages, including which to install when getting started.

## Additional documentation

See [Microsoft SQL Server EF Core Database Provider](https://learn.microsoft.com/en-us/ef/core/providers/in-memory/) for more information about the features of this database provider.

## Feedback

If you encounter a bug or issues with this package,you can [open an Github issue](https://github.com/dotnet/efcore/issues/new/choose). For more details, see [getting support](https://github.com/dotnet/efcore/blob/main/.github/SUPPORT.md).