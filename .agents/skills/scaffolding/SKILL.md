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

Provider schema readers often query one-to-many metadata tables. Preserve anti-semi semantics when adapting SQL for a limited engine: a `LEFT JOIN` containing the complete excluded-property predicate in `ON`, followed by a null filter on a non-nullable joined key, is equivalent to `NOT EXISTS`. Unrelated metadata rows do not join; any matching excluded row makes the joined key non-null and excludes the object. Do not move parts of the excluded-property predicate to `WHERE` or join all metadata rows and filter individual rows afterward. An equivalent non-correlated anti-set form, such as `NOT IN (SELECT key ... WHERE excluded-condition)`, also works when the subquery cannot return null keys. Cover objects with no metadata, unrelated metadata only, the excluded property only, and both excluded and unrelated properties.
