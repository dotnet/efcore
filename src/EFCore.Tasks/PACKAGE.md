The Entity Framework Core MSBuild tasks integrate EF design-time tools into the build process. They're primarily used to generate the compiled model.

This package should be referenced by the project containing the derived `DbContext`.

## Usage

Install the package into your project, set `<EFOptimizeContext Condition="'$(Configuration)'=='Release'">true</EFOptimizeContext>` and then run build normally.

If the startup project is different from the current project it needs to be specified: `<EFStartupProject>..\Startup\Startup.csproj</EFStartupProject>`

## Getting started with EF Core

See [Getting started with EF Core](https://learn.microsoft.com/ef/core/get-started/overview/install) for more information about EF NuGet packages, including which to install when getting started.

## Feedback

If you encounter a bug or issues with this package,you can [open an Github issue](https://github.com/dotnet/efcore/issues/new/choose). For more details, see [getting support](https://github.com/dotnet/efcore/blob/main/.github/SUPPORT.md).
