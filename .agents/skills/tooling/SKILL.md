---
name: tooling
description: 'Implementation details for the EF Core dotnet-ef CLI and tooling. Use when changing dotnet-ef commands, the ef wrapper, EFCore.Tools (PMC), or EFCore.Tasks MSBuild integration.'
user-invokable: false
---

# Tooling

The `dotnet ef` CLI and Visual Studio Package Manager Console commands for migrations, scaffolding, and compiled models.

## dotnet-ef CLI (`src/dotnet-ef/`)

`RootCommand` parses global options (`--project`, `--startup-project`, `--framework`, `--configuration`, `--runtime`, `--no-build`). Subcommands in `Commands/`: `DatabaseCommand`, `DbContextCommand`, `MigrationsCommand`. Each invokes MSBuild to build, then shells out via `dotnet exec ef.dll`, which hosts `OperationExecutor`.

## PMC (`src/EFCore.Tools/`)

PowerShell module: `Add-Migration`, `Update-Database`, `Scaffold-DbContext`, `Optimize-DbContext`, etc. Routes to `OperationExecutor`.

## MSBuild Tasks (`src/EFCore.Tasks/`)

NuGet package `Microsoft.EntityFrameworkCore.Tasks` provides build/publish-time compiled model and precompiled query generation.

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