`Microsoft.EntityFrameworkCore.Sqlite` is the EF Core database provider package for SQLite.

## Usage

Call the `UseSqlite` method to choose the SQLite database provider for your `DbContext`. For example:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseSqlite("Data Source=databse.dat");
}
```

## Getting started with EF Core

See [Getting started with EF Core](https://learn.microsoft.com/ef/core/get-started/overview/install) for more information about EF NuGet packages, including which to install when getting started.

## Additional documentation

See [SQLite EF Core Database Provider](https://learn.microsoft.com/ef/core/providers/sqlite/) for more information about the features of this database provider.

## Feedback

If you encounter a bug or issues with this package,you can [open an Github issue](https://github.com/dotnet/efcore/issues/new/choose). For more details, see [getting support](https://github.com/dotnet/efcore/blob/main/.github/SUPPORT.md).