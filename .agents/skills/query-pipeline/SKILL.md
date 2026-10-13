---
name: query-pipeline
description: 'Implementation details for EF Core LINQ query translation, SQL generation, and bulk operations (ExecuteUpdate/ExecuteDelete). Use when changing expression visitors, SqlExpressions, QuerySqlGenerator, ShaperProcessingExpressionVisitor, UpdateExpression, DeleteExpression, or related classes.'
user-invocable: false
---

# Query Pipeline

## Stages

1. **Preprocessing**
2. **Translation**
3. **Postprocessing**
4. **Compilation**
5. **SQL Generation**

## Validation

- `ToQueryString()` shows generated SQL without executing
- `ExpressionPrinter` dumps expression trees at any pipeline stage
- SQL baselines verified via `AssertSql()` in provider functional tests and the generated SQL corresponds to the LINQ query in the base method
