The Entity Framework Core tools help with design-time development tasks. They're primarily used to manage Migrations and to scaffold a `DbContext` and entity types by reverse engineering the schema of a database.

The `Microsoft.EntityFrameworkCore.Design` package is required for either command-line or Package Manager Console-based tooling, and is a dependency of [dotnet-ef](https://www.nuget.org/packages/dotnet-ef) and [Microsoft.EntityFrameworkCore.Tools](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Tools).

## Usage

Install the package into your project and then use either [dotnet-ef](https://www.nuget.org/packages/dotnet-ef) or [Microsoft.EntityFrameworkCore.Tools](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Tools).

By default, the package will install with `PrivateAssets="All" `so that the tooling assembly will not be included with your published app. For example:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.2">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

## Getting started with EF Core

See [Getting started with EF Core](https://learn.microsoft.com/ef/core/get-started/overview/install) for more information about EF NuGet packages, including which to install when getting started.

## Feedback

If you encounter a bug or issues with this package,you can [open an Github issue](https://github.com/dotnet/efcore/issues/new/choose). For more details, see [getting support](https://github.com/dotnet/efcore/blob/main/.github/SUPPORT.md).
