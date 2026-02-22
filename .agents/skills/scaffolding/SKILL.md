---
name: scaffolding
description: 'EF Core scaffolding (reverse engineering), CSharpModelGenerator, database schema reading, code generation. Use when working on ef dbcontext scaffold.'
user-invokable: false
---

# Scaffoldin

Generates C# code from database schemas (reverse engineering).

## When to Use

- Modifying how `dotnet ef dbcontext scaffold` generates code
- Changing how database schemas are read by a provider's `IDatabaseModelFactory`

## When not to Use

- Working on compiled model generation (`dotnet ef dbcontext optimize`)

## Reverse Engineering

Pipeline: `IDatabaseModelFactory` (reads schema) → `IScaffoldingModelFactory` (builds EF model) → `IModelCodeGenerator` (generates C#)

Key files in `src/EFCore.Design/Scaffolding/`:
- `IReverseEngineerScaffolder` — orchestrates full pipeline
- `Internal/CSharpModelGenerator.cs` — default C# generator

Provider factories: `SqlServerDatabaseModelFactory`, `SqliteDatabaseModelFactory`


## Design-Time Services

`IDesignTimeServices` — provider/plugin registers design-time services. `DesignTimeServicesBuilder` discovers them.

## Testing

Scaffolding tests: `test/EFCore.Design.Tests/Scaffolding/`.

## Validation

- Compiled model baselines in `Baselines/{testName}/` directories
- `EF_TEST_REWRITE_BASELINES=1` auto-updates baselines
