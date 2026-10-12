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

## Testing

- CLI tests: `test/dotnet-ef.Tests/`, `test/ef.Tests/`
- EFCore.Tasks has no dedicated test project — for task/target changes, create a test project and manually run `dotnet build` / `dotnet publish` with `EFOptimizeContext=true` to verify

## Validation

- For tool changes, create a test project and manually run affected commands to verify behavior
- `dotnet ef migrations script` output matches expected DDL