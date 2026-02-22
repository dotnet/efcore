---
name: tooling
description: 'EF Core dotnet-ef CLI tool, Package Manager Console commands, EFCore.Tools PowerShell module, EFCore.Tasks MSBuild integration. Use when working on dotnet-ef commands, the ef wrapper, or tool infrastructure.'
user-invokable: false
---

# Tooling

The `dotnet ef` CLI and Visual Studio Package Manager Console commands for migrations, scaffolding, and compiled models.

## When to Use

- Adding or modifying a `dotnet ef` command
- Working on PMC (PowerShell) command wrappers
- Debugging tool invocation, project discovery, or MSBuild integration

## dotnet-ef CLI (`src/dotnet-ef/`)

`RootCommand` parses global options (`--project`, `--startup-project`, `--framework`, `--configuration`, `--runtime`, `--no-build`). Subcommands in `Commands/`: `DatabaseCommand`, `DbContextCommand`, `MigrationsCommand`. Each invokes MSBuild to build, then shells out via `dotnet exec ef.dll`, which hosts `OperationExecutor`.

## PMC (`src/EFCore.Tools/`)

PowerShell module: `Add-Migration`, `Update-Database`, `Scaffold-DbContext`, `Optimize-DbContext`, etc. Routes to `OperationExecutor`.

## MSBuild Tasks (`src/EFCore.Tasks/`)

NuGet package `Microsoft.EntityFrameworkCore.Tasks` provides build/publish-time compiled model and precompiled query generation.

### Key Classes

- `OptimizeDbContext` — public MSBuild task. Invokes `dotnet exec ef.dll dbcontext optimize`, collects generated `.g.cs` files as `[Output]`
- `OperationTaskBase` — abstract base extending `ToolTask`. Orchestrates `dotnet exec ef.dll` with assembly/deps/runtimeconfig args. Parses prefixed output into MSBuild errors/warnings

### Build Properties (user-configurable in `.csproj`)

| Property | Default | Purpose |
|----------|---------|--------|
| `EFOptimizeContext` | unset | `true` enables generation outside NativeAOT publish |
| `EFScaffoldModelStage` | `publish` | `publish` or `build` — when to generate compiled model |
| `EFPrecompileQueriesStage` | `publish` | `publish` or `build` — when to precompile queries |
| `DbContextType` | `*` | Specific `DbContext` to optimize, `*` = all |
| `EFOutputDir` | `$(IntermediateOutputPath)` | Directory for generated files |

### Build Integration Flow

Targets in `buildTransitive/Microsoft.EntityFrameworkCore.Tasks.targets`:
- **Build flow**: `_EFGenerateFilesAfterBuild` triggers after compilation when `EFOptimizeContext=true` and stage is `build`. Invokes `OptimizeDbContext` task, writes generated file list, re-triggers `Build` to compile new files.
- **Publish flow**: `_EFGenerateFilesBeforePublish` runs before `GeneratePublishDependencyFile`. Auto-activates for `PublishAOT=true`. `_EFPrepareDependenciesForPublishAOT` cascades to project references.
- **Incremental**: `_EFProcessGeneratedFiles` reads tracking files and adds `.g.cs` to `@(Compile)`. Stale files removed by `_EFPrepareForCompile`.
- **Clean**: `_EFCleanGeneratedFiles` deletes generated and tracking files.

## Testing

- CLI tests: `test/dotnet-ef.Tests/`, `test/ef.Tests/`
- EFCore.Tasks has no dedicated test project — for task/target changes, create a test project and manually run `dotnet build` / `dotnet publish` with `EFOptimizeContext=true` to verify

## Validation

- For tool changes, create a test project and manually run affected commands to verify behavior
- `dotnet ef migrations script` output matches expected DDL