---
name: query-pipeline
description: 'Implement and review EF Core query translation, projection visitors, split-query enumeration, and query regression tests. Use when changing query-pipeline code or choosing specification and provider coverage for a query regression.'
user-invocable: false
---

# Query Pipeline

## Regression Test Ownership

- Group query specification tests by a cohesive query or model trait. Do not create a generic bucket merely because tests use custom models; use the existing trait-specific hierarchy, such as owned-entity queries.
- Inspect the specification base and provider overrides before placing a regression. Keep the LINQ query, model setup, and result assertions in the shared virtual test; provider overrides call the base and add only provider-specific baselines or established limitations.
- A provider-only test can leave every other provider untested even when its SQL assertion is correct. Preserve override guards without adding a guard that forces unrelated redundant overrides.

## Translation

- Translation fallback must distinguish established unsupported shapes from unexpected defects. Do not turn arbitrary exceptions into a successful fallback.

## Split Queries

- When changing `SplitQueryResultCoordinator`, check both `SplitQueryingEnumerable` and `GroupBySplitQueryingEnumerable`, including their synchronous and asynchronous paths. End-of-stream validation belongs at true outer-stream exhaustion, not at a group boundary or early disposal.
- Account for internal buffering as well as caller materialization when assessing memory use. Earlier split-query result sets may need buffering when the provider cannot keep multiple readers active; retrying execution strategies can also require buffering.
- Buffering does not give separate query commands a consistent database snapshot. Concurrent writes between commands can produce an inconsistent result graph; consider snapshot or serializable transactions when consistency is required, accounting for provider support and transaction costs.

## Validation

- `ToQueryString()` shows generated SQL without executing
- `ExpressionPrinter` dumps expression trees at any pipeline stage
- SQL baselines verified via `AssertSql()` in provider functional tests and the generated SQL corresponds to the LINQ query in the base method
