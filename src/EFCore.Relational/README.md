`Microsoft.EntityFrameworkCore.Relational` package contains common code for relational database providers that are based on ADO.NET.

## Usage

This package is included automatically as a dependency of each relational provider package. Adding an explicit reference can be useful to ensure the latest version is used even if the provider is depending on an older version.

Boolean comparison optimization assumes that boolean values without a value converter have a two-valued database representation. For example, SQLite databases containing boolean values other than 0 and 1 can return incorrect results. To disable this optimization and restore the previous comparison behavior for all relational providers, set the AppContext switch before using EF Core:

```csharp
AppContext.SetSwitch("Microsoft.EntityFrameworkCore.DisableBooleanComparisonOptimization", true);
```

## Getting started with EF Core

See [Getting started with EF Core](https://learn.microsoft.com/ef/core/get-started/overview/install) for more information about EF NuGet packages, including which to install when getting started.

## Feedback

If you encounter a bug or issues with this package,you can [open an Github issue](https://github.com/dotnet/efcore/issues/new/choose). For more details, see [getting support](https://github.com/dotnet/efcore/blob/main/.github/SUPPORT.md).