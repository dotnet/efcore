`Microsoft.EntityFrameworkCore.Abstractions` is a small package containing abstractions which may be useful for applications in places where a dependency on the full `Microsoft.EntityFrameworkCore` is not desirable.

## Usage

This package is included automatically as a dependency of the main [Microsoft.EntityFrameworkCore](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore) package. Usually, the abstractions package is only explicitly installed in places where it is undesirable to use the main package. For example, it can be installed to use mapping attributes on POCO entity types which are otherwise independent of EF Core.

## Getting started with EF Core

See [Getting started with EF Core](https://learn.microsoft.com/ef/core/get-started/overview/install) for more information about EF NuGet packages, including which to install when getting started.

## Feedback

If you encounter a bug or issues with this package,you can [open an Github issue](https://github.com/dotnet/efcore/issues/new/choose). For more details, see [getting support](https://github.com/dotnet/efcore/blob/main/.github/SUPPORT.md).
