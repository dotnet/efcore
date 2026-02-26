---
name: scaffolding
description: 'Implementation details for EF Core scaffolding (reverse engineering). Use when changing ef dbcontext scaffold pipeline implementation, database schema reading, CSharpModelGenerator, or related classes.'
user-invokable: false
---

# Scaffolding

Generates C# code from database schemas (reverse engineering).

## When Not to Use

- Working on compiled model generation (`dotnet ef dbcontext optimize`)

## Reverse Engineering

Pipeline: `IDatabaseModelFactory` (reads schema) → `IScaffoldingModelFactory` (builds EF model) → `IModelCodeGenerator` (generates C#)
- `IReverseEngineerScaffolder` — orchestrates full pipeline
