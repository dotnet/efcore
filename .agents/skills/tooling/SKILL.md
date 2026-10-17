---
name: tooling
description: 'Implementation details for the EF Core dotnet-ef CLI and tooling. Use when changing dotnet-ef commands, the ef wrapper, EFCore.Tools (PMC), or EFCore.Tasks MSBuild integration.'
user-invocable: false
---

# Tooling

The `dotnet ef` CLI and Visual Studio Package Manager Console commands for migrations, scaffolding, and compiled models.

## dotnet-ef CLI (`src/dotnet-ef/`)

`RootCommand` parses global options. Subcommands implemented in `src/ef/Commands/`. Each invokes MSBuild to build, then shells out via `dotnet exec ef.dll`, which hosts `OperationExecutor`.

## PMC (`src/EFCore.Tools/`)

PowerShell module: `Add-Migration`, `Update-Database`, `Scaffold-DbContext`, `Optimize-DbContext`, etc.

## MSBuild Tasks (`src/EFCore.Tasks/`)

NuGet package `Microsoft.EntityFrameworkCore.Tasks` provides build/publish-time compiled model and precompiled query generation. Targets in `buildTransitive/Microsoft.EntityFrameworkCore.Tasks.targets`.

## Command Option Ownership

Trace an option to the code that consumes it before changing command or operation contracts. In a change plan, explain the existing transport separately from any new operation parameter: `src/dotnet-ef/RootCommand.cs` builds the project and already forwards remaining command-line arguments to `tools/net/ef.dll`; the inner command under `src/ef/Commands/` parses command-specific options. Generic launcher forwarding does not mean a new value belongs in `IOperationExecutor`.

- Keep presentation-only behavior in the inner command. For example, suppressing the completion message from `MigrationsAddCommand.Execute()` should guard its `Reporter.WriteInformation()` call and must not change `IOperationExecutor.AddMigration()`.
- Forward a value through `OperationExecutorBase` only when the design-time operation in `Microsoft.EntityFrameworkCore.Design` needs it.
- Treat PMC and `EFCore.Tasks` as separate entry points. Change them only when the requested behavior applies to those surfaces too.

## Testing

- Keep parser/command unit tests in `test/dotnet-ef.Tests/` and `test/ef.Tests/`; use `test/dotnet-ef.FunctionalTests/` and `test/EFCore.Tasks.FunctionalTests/` for real SDK/build/publish behavior.
- Reuse `test/EFCore.Design.TestUtilities/` to stage checkout-built task/tool binaries and materialize isolated project graphs before EF packages exist. A project reference to the tasks assembly does not reproduce the package's props/targets imports or relative tool layout.
- SDK option interception requires invoking `dotnet ef`, not only the launcher DLL, inner `ef.dll`, or parser. Assert the option's generated-code/database effect, not just successful exit.
- Distinguish startup, context, migrations, and query project roles. Keep runtime/design-time migrations-assembly configuration consistent without adding inverse project references.
- Resolve the SDK through the configured `DOTNET_ROOT` and propagate it to children. Samples reference prebuilt product projects with `Targets=GetTargetPath`; do not disable building their own project graph. Clean only owned projects with reference cleaning disabled.
- Native coverage uses materialized `xunit.v3.aot` test targets, published and executed directly with real test-result counts. Running the unpublished test project is not NativeAOT validation.

## Validation

- Use `EFScaffoldModelStage` / `EFPrecompileQueriesStage` for generation stages; `EFOptimizeContext` is rejected.
- Run the owning functional suite and inspect command logs/binlogs plus generated artifacts and runtime results. Expected experimental failures belong in narrowly scoped issue-linked skips, not process-failure-to-skip fallbacks.
- `dotnet ef migrations script` output matches expected DDL
