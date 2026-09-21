The `Microsoft.EntityFrameworkCore.Proxies` package contains implementations of dynamic proxies for [lazy-loading](https://learn.microsoft.com/ef/core/querying/related-data/lazy#lazy-loading-with-proxies) and/or [change-tracking](https://learn.microsoft.com/ef/core/change-tracking/change-detection#change-tracking-proxies).

## Usage

Call `UseLazyLoadingProxies` when configuring your `DbContext` to enable lazy-loading proxies. For example:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder.UseLazyLoadingProxies();
```

Call `UseChangeTrackingProxies` when configuring your `DbContext` to enable change-tracking proxies. For example:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder.UseChangeTrackingProxies();
```

Call both methods for proxies that implement both lazy-loading and change-tracking. For example:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder
        .UseLazyLoadingProxies()
        .UseChangeTrackingProxies();
```

## Getting started with EF Core

See [Getting started with EF Core](https://learn.microsoft.com/ef/core/get-started/overview/install) for more information about EF NuGet packages, including which to install when getting started.

## Feedback

If you encounter a bug or issues with this package,you can [open an Github issue](https://github.com/dotnet/efcore/issues/new/choose). For more details, see [getting support](https://github.com/dotnet/efcore/blob/main/.github/SUPPORT.md).