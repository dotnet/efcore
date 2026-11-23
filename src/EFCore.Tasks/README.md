The Entity Framework Core MSBuild tasks integrate EF design-time tools into the build process. They're primarily used to generate the compiled model.

This package should be referenced by the project containing the derived `DbContext`.

## Usage

Install the package into your project and if PublishAOT is true then just publish normally. Otherwise you can control code generation by the `$(EFScaffoldModelStage)` and `$(EFPrecompileQueriesStage)` properties, which can be set to either `publish` or `build` to control at what stage the code will be generated. Any other value will disable the corresponding generation.

The context project must be self-sufficient in terms of configuration. If your application normally configures the context in a different project, implement `IDesignTimeDbContextFactory<TContext>` in the context project. A separate startup project is not supported by MSBuild integration, as it would introduce an inverse build dependency. The CLI's `--startup-project` option is a separate interface.

The retained `EFStartupAssembly` property maps to the task's `StartupAssembly` parameter. This is a low-level assembly selector, not a replacement for the removed `EFStartupProject` workflow: the targets do not build or resolve a separate startup project. An explicitly supplied selector must end in lowercase `.exe`; the task changes that extension to `.dll` for the `--startup-assembly` argument. Sibling `.deps.json` and `.runtimeconfig.json` files are passed to `dotnet exec` only when present. Acceptance of a selector does not establish that the assembly can be loaded or executed, or that a separate-startup workflow is supported. Normally, leave this property unset so the context assembly is used.

See [MSBuild integration limitations](https://learn.microsoft.com/ef/core/cli/msbuild#limitations) for more information.

## Getting started with EF Core

See [Getting started with EF Core](https://learn.microsoft.com/ef/core/get-started/overview/install) for more information about EF NuGet packages, including which to install when getting started.

## Feedback

If you encounter a bug or issues with this package,you can [open an Github issue](https://github.com/dotnet/efcore/issues/new/choose). For more details, see [getting support](https://github.com/dotnet/efcore/blob/main/.github/SUPPORT.md).
