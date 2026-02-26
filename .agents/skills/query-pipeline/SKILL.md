---
name: query-pipeline
description: 'Implementation details for EF Core LINQ query translation and SQL generation. Use when changing expression visitors, SqlExpressions, QuerySqlGenerator, ShaperProcessingExpressionVisitor, or related classes.'
user-invokable: false
---

# Query Pipeline

Translates LINQ expressions into database queries and materializes results.

## Stages

1. **Preprocessing** — `QueryTranslationPreprocessor`: `NavigationExpandingExpressionVisitor` (Include, navigations, auto-includes), `QueryOptimizingExpressionVisitor`
2. **Translation** — `QueryableMethodTranslatingExpressionVisitor`: LINQ methods → `ShapedQueryExpression` (= `QueryExpression` + `ShaperExpression`). Relational: `RelationalSqlTranslatingExpressionVisitor`, `SelectExpression`
3. **Postprocessing** — `QueryTranslationPostprocessor`: `SqlNullabilityProcessor`, `SqlTreePruner`, `SqlAliasManager`, `RelationalParameterBasedSqlProcessor`, `RelationalSqlProcessingExpressionVisitor`
4. **Compilation** — `ShapedQueryCompilingExpressionVisitor` → executable delegate. Relational: `ShaperProcessingExpressionVisitor` builds shaper and materialization code
5. **SQL Generation** — `QuerySqlGenerator`

## Validation

- `ToQueryString()` shows generated SQL without executing
- `ExpressionPrinter` dumps expression trees at any pipeline stage
- SQL baselines verified via `AssertSql()` in provider functional tests
