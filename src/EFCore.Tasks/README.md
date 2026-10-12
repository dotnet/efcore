The Entity Framework Core MSBuild tasks integrate EF design-time tools into the build process. They're primarily used to generate the compiled model.

This package should be referenced by the project containing the derived `DbContext`.

## Usage

Install the package into your project and if PublishAOT is true then just publish normally. Otherwise you can control code generation by the `$(EFScaffoldModelStage)` and `$(EFPrecompileQueriesStage)` properties, which can be set to either `publish` or `build` to control at what stage the code will be generated. Any other value will disable the corresponding generation.

If the startup project is different from the current project it needs to be specified: `<EFStartupProject>..\Startup\Startup.csproj</EFStartupProject>`

The startup project must also reference the `Microsoft.EntityFrameworkCore.Tasks` package.

## Getting started with EF Core

See [Getting started with EF Core](https://learn.microsoft.com/ef/core/get-started/overview/install) for more information about EF NuGet packages, including which to install when getting started.

## Feedback

If you encounter a bug or issues with this package,you can [open an Github issue](https://github.com/dotnet/efcore/issues/new/choose). For more details, see [getting support](https://github.com/dotnet/efcore/blob/main/.github/SUPPORT.md).
