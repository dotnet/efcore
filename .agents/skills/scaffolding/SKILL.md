---
name: scaffolding
description: 'Implementation details for EF Core scaffolding (reverse engineering). Use when changing ef dbcontext scaffold pipeline implementation, database schema reading, CSharpModelGenerator, or related classes.'
user-invocable: false
---

# Scaffolding

Generates C# code from database schemas (reverse engineering).

## When Not to Use

- Working on compiled model generation (`dotnet ef dbcontext optimize`)

## Reverse Engineering

Pipeline: `IDatabaseModelFactory` (reads schema) → `IScaffoldingModelFactory` (builds EF model) → `IModelCodeGenerator` (generates C#)
- `IReverseEngineerScaffolder` — orchestrates full pipeline

## Schema Query Compatibility

Provider schema readers often query one-to-many metadata tables. Preserve anti-semi semantics when adapting SQL for a limited engine: replacing `NOT EXISTS` with a `LEFT JOIN` plus a null filter is incorrect when an object can have multiple metadata rows, even when the excluded-property predicate is in the join condition. The excluded row does not join but an unrelated row does; its non-null joined key keeps the object in the result. Prefer an equivalent non-correlated anti-set form, such as `NOT IN (SELECT key ... WHERE excluded-condition)`, when nullability and key semantics permit it.
