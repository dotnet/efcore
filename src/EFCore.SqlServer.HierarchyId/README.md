`Microsoft.EntityFrameworkCore.SqlServer.HierarchyId` enables use of [hierarchical data for SQL Server and Azure SQL]() with [Entity Framework Core](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore/) and [NetTopologySuite](https://www.nuget.org/packages/NetTopologySuite/).

## Usage

Call `UseHierarchyId` inside the call to `UseSqServer` when configuring the SQLite database provider for your `DbContext`. For example:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => options.UseSqlServer(
        "Server=localhost;Database=MyDatabase;Trusted_Connection=True;",
        b => b.HierarchyId());
```

See [_Hierarchical Data in the SQL Server EF Core Provider_](https://learn.microsoft.com/ef/core/providers/sql-server/hierarchyid) for more information on `HierarchyId` with EF Core.

## Getting started with EF Core

See [Getting started with EF Core](https://learn.microsoft.com/ef/core/get-started/overview/install) for more information about EF NuGet packages, including which to install when getting started.

## Feedback

If you encounter a bug or issues with this package,you can [open an Github issue](https://github.com/dotnet/efcore/issues/new/choose). For more details, see [getting support](https://github.com/dotnet/efcore/blob/main/.github/SUPPORT.md).
