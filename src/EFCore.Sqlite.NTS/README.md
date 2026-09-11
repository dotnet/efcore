`Microsoft.EntityFrameworkCore.Sqlite.NetTopologySuite` enables use of [SpatiaLite spatial data for SQLite](https://www.gaia-gis.it/fossil/libspatialite/index) with [Entity Framework Core](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore/) and [NetTopologySuite](https://www.nuget.org/packages/NetTopologySuite/).

## Usage

Call `UseNetTopologySuite` inside the call to `UseSqlite` when configuring the SQLite database provider for your `DbContext`. For example:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => options.UseSqlite("Data Source=spatialdata.dat", b => b.UseNetTopologySuite());
```

For more information on using spatial data with EF Core and SQLite, see:

- [Spatial Data in EF Core](https://learn.microsoft.com/ef/core/modeling/spatial)
- [Spatial Data in the SQLite EF Core Provider](https://learn.microsoft.com/ef/core/providers/sqlite/spatial)

## Getting started with EF Core

See [Getting started with EF Core](https://learn.microsoft.com/ef/core/get-started/overview/install) for more information about EF NuGet packages, including which to install when getting started.

## Feedback

If you encounter a bug or issues with this package,you can [open an Github issue](https://github.com/dotnet/efcore/issues/new/choose). For more details, see [getting support](https://github.com/dotnet/efcore/blob/main/.github/SUPPORT.md).