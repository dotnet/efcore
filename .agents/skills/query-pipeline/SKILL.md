---
name: query-pipeline
description: 'EF Core LINQ query translation, compilation, SQL generation, shaper, materialization. Use when working on query translation, expression visitors, SqlExpressions, QuerySqlGenerator, or ShaperProcessingExpressionVisitor.'
user-invokable: false
---

# Query Pipeline

Translates LINQ expressions into database queries and materializes results.

## When to Use

- Adding or modifying query translation for a LINQ operator
- Adding a new SQL expression type or method/member translator
- Debugging incorrect SQL generation or materialization
- Working on the shaper, JSON column handling, or split queries

## Stages

1. **Preprocessing** — `QueryTranslationPreprocessor`: `NavigationExpandingExpressionVisitor` (Include, navigations, auto-includes), `QueryOptimizingExpressionVisitor`
2. **Translation** — `QueryableMethodTranslatingExpressionVisitor`: LINQ methods → `ShapedQueryExpression` (= `QueryExpression` + `ShaperExpression`). Relational: QueryExpression = `SelectExpression`
3. **Postprocessing** — `QueryTranslationPostprocessor`: `SqlNullabilityProcessor`, `SqlTreePruner`, `SqlAliasManager`, `RelationalParameterBasedSqlProcessor`
4. **Compilation** — `ShapedQueryCompilingExpressionVisitor` → executable delegate. Relational: `ShaperProcessingExpressionVisitor` builds materialization code

Entry point: `QueryCompiler.CompileQueryCore()` → `QueryCompilationContext`. Result cached in `CompiledQueryCache`.

## Key Files

| Area | Path |
|------|------|
| Compilation context | `src/EFCore/Query/QueryCompilationContext.cs` |
| Navigation expansion | `src/EFCore/Query/Internal/NavigationExpandingExpressionVisitor.cs` |
| LINQ → SQL | `src/EFCore.Relational/Query/RelationalQueryableMethodTranslatingExpressionVisitor.cs` |
| C# → SQL expressions | `src/EFCore.Relational/Query/RelationalSqlTranslatingExpressionVisitor.cs` |
| SQL AST → string | `src/EFCore.Relational/Query/QuerySqlGenerator.cs` |
| Materialization | `src/EFCore.Relational/Query/RelationalShapedQueryCompilingExpressionVisitor.ShaperProcessingExpressionVisitor.cs` |
| SQL expression factory | `src/EFCore.Relational/Query/SqlExpressionFactory.cs` |
| SQL AST nodes | `src/EFCore.Relational/Query/SqlExpressions/` |
| ExecuteUpdate/Delete | partial files on `RelationalQueryableMethodTranslatingExpressionVisitor` |

## Method/Member Translation

Provider translators implement `IMethodCallTranslator` / `IMemberTranslator`, registered via the provider's `MethodCallTranslatorProvider`. Located in `src/EFCore.{Provider}/Query/Internal/Translators/`.

Adding a new translator:
1. Implement `IMethodCallTranslator` — check `method.DeclaringType` before translating
2. Add to the provider's `*MethodCallTranslatorProvider` constructor's `AddTranslators()` call
3. Check the base relational layer first — common translations (e.g., `Math.Max`) may already exist

## Cosmos Pipeline

Parallel pipeline in `src/EFCore.Cosmos/Query/Internal/`: `CosmosQueryableMethodTranslatingExpressionVisitor`, `CosmosSqlTranslatingExpressionVisitor`, `CosmosQuerySqlGenerator`. Uses `JObject` materialization. Does not use any relational query infrastructure.

## Testing

Query specification tests: `test/EFCore.Specification.Tests/Query/`, `test/EFCore.Relational.Specification.Tests/Query/`. Provider overrides in `test/EFCore.{Provider}.FunctionalTests/Query/` with `AssertSql()` baselines.

## Validation

- `ToQueryString()` shows generated SQL without executing
- `ExpressionPrinter` dumps expression trees at any pipeline stage
- SQL baselines verified via `AssertSql()` in provider functional tests
