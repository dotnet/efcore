The `Microsoft.EntityFrameworkCore.Templates` package contains T4 templates for scaffolding (reverse engineering) a `DbContext` and entity types from an existing database.

## Usage

First, install the templates NuGet package:

```
dotnet new install Microsoft.EntityFrameworkCore.Templates
```

Next, can templates to your project:

```dotnetcli
dotnet new ef-templates
```

See [_Custom Reverse Engineering Templates_](https://learn.microsoft.com/ef/core/managing-schemas/scaffolding/templates) for more information on using T4 templates with EF Core.

## Getting started with EF Core

See [Getting started with EF Core](https://learn.microsoft.com/ef/core/get-started/overview/install) for more information about EF NuGet packages, including which to install when getting started.

## Feedback

If you encounter a bug or issues with this package,you can [open an Github issue](https://github.com/dotnet/efcore/issues/new/choose). For more details, see [getting support](https://github.com/dotnet/efcore/blob/main/.github/SUPPORT.md).
