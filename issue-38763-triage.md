# Triage: dotnet/efcore#38763 — `scaffold vs Sql Server [Embedding] [vector](1536, float32)`

**Issue:** https://github.com/dotnet/efcore/issues/38763
**Reported version:** EF Core 10.0.10, `Microsoft.EntityFrameworkCore.SqlServer`
**Verified against:** `main` @ `dbf9771` (2026-08-07)

## Verdict

**Not fixed on `main`.** The bug reproduces with the current code.

## Symptom

`dotnet ef dbcontext scaffold` against a database containing a `vector` column fails with:

```
System.InvalidOperationException: Vector properties require a positive size (number of dimensions).
   at Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal.SqlServerVectorTypeMapping..ctor(RelationalTypeMappingParameters parameters)
   at ...SqlServerVectorTypeMapping.Clone(RelationalTypeMappingParameters parameters)
   at ...RelationalTypeMapping.WithTypeMappingInfo(RelationalTypeMappingInfo& mappingInfo)
   at ...SqlServerTypeMappingSource.FindMapping(RelationalTypeMappingInfo& mappingInfo)
   ...
   at ...ScaffoldingTypeMapper.FindMapping(String storeType, ...)
```

## Root cause

The scaffolder builds the column's store type as `vector({vectorDimensions})`, where `vectorDimensions`
comes from `sys.columns.vector_dimensions`:

- `src/EFCore.SqlServer/Scaffolding/Internal/SqlServerDatabaseModelFactory.cs:703`

  ```
  {(_compatibilityLevel is >= 170 ? "[c].[vector_dimensions]" : "NULL as [vector_dimensions]")},
  ```

- `SqlServerDatabaseModelFactory.cs:774` — `var vectorDimensions = dataRecord.GetValueOrDefault<int>("vector_dimensions");`
  (`NULL` ⇒ `0`)
- `SqlServerDatabaseModelFactory.cs:979-980` — `case "vector": return $"vector({vectorDimensions})";`

So when the **database compatibility level is below 170**, the query deliberately returns `NULL` for
`vector_dimensions`, which becomes `0`, and the column's store type is scaffolded as **`vector(0)`**.

`SqlServerTypeMappingSource` then resolves `vector(0)` to `SqlServerVectorTypeMapping.Default` and clones it
with `Size = 0`, and the constructor rejects a non-positive size
(`src/EFCore.SqlServer/Storage/Internal/SqlServerVectorTypeMapping.cs:48-74`,
message `SqlServerStrings.VectorDimensionsInvalid`).

The compatibility-level gate is the incorrect condition here. Per the SQL Server docs
(["Vector data type" → Feature availability](https://learn.microsoft.com/sql/t-sql/data-types/vector-data-type)):

> The new **vector** type is available under all database compatibility levels.

`sys.columns.vector_dimensions` availability depends on the **engine version** (SQL Server 2025 / Azure SQL),
not on the database compatibility level. A database on Azure SQL or SQL Server 2025 with compatibility level
160 (or lower) can therefore contain `vector` columns while EF refuses to read their dimensions.

The `vector(1536, float32)` spelling in the issue title is just the SSMS-generated DDL; EF never sees that
string during scaffolding — it composes the store type itself.

## Reproduction / verification performed

A temporary test was run against a locally built `main` using `SqlServerTypeMappingSource.FindMapping(storeType)`
(the same entry point `ScaffoldingTypeMapper.FindMapping` uses when no CLR type is known):

| Store type | Result on `main` |
| --- | --- |
| `vector(1536)` | OK — `StoreType = vector(1536)`, `Size = 1536` |
| `vector(0)` | **`InvalidOperationException: Vector properties require a positive size (number of dimensions).`** |
| `vector(1536, float32)` | Mapping returned, but `Size` is `null` (base parser reads `1536` as *precision*, fails to parse `float32` as scale) |

The second row is an exact match for the reported stack trace and message, confirming the scaffolded store
type is `vector(0)`.

## Suggested fix direction

1. Gate the `[c].[vector_dimensions]` column selection on the **engine version / column existence** rather than
   `_compatibilityLevel >= 170` (the same likely applies to `SupportsVectorIndexes` at
   `SqlServerDatabaseModelFactory.cs:1681-1685`).
2. Defensively handle `vectorDimensions <= 0` in `GetStoreType` — e.g. log a warning and skip/omit the facet
   instead of emitting `vector(0)`, so scaffolding never produces a store type that the type mapping source
   rejects with an unactionable exception.
3. Consider a test covering scaffolding of a `vector` column on a database whose compatibility level is < 170.

## Workaround (from the issue author)

Temporarily drop the vector column, scaffold, then re-add the property manually:

```csharp
entity.Property(e => e.Embedding).HasColumnType("vector(1536, float32)");
```
