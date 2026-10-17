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
- For an optimization, cover both eligible and rejected shapes with exact result assertions in sync and async. Supply a concrete provider override and SQL baseline or established limitation; an abstract test base or expression-shape check alone does not prove execution coverage.

## Translation

- Translation fallback must distinguish established unsupported shapes from unexpected defects. Do not turn arbitrary exceptions into a successful fallback.
- Check whether a failed rewrite mutated state that fallback will reuse. For test coordination, establish the actual collection, assembly, and async-flow boundaries before claiming shared static state can race; theory rows are not inherently parallel, and reset in `finally` is not mutual exclusion.

## Optimizations

- Match the equivalence needed by the rewrite, not blindly by expression equality.
- Preserve context-specific null semantics. SQL UNKNOWN can be filtered like `false` in a predicate but not in a projected value or under negation; `UseRelationalNulls`, null-containing `IN`/`NOT IN`, and scalar versus composite LINQ join keys require different compensation.
- Preserve cardinality and operator order, not just matching rows. Moving predicates across outer joins, adding keys under `Distinct`/`GroupBy`, or converting correlated APPLY to JOIN can change results; per-parent pagination must retain its partitioning and ordering, and remaining outer references must prevent decorrelation.
- Share repeated subqueries only when their evaluation and row selection are equivalent. Volatile expressions, query filters introduced during later navigation expansion, and required-navigation joins can change evaluation count or which row survives a limit. Check determinism and row preservation before reuse, and collect candidates within their own lambda scope.
- Preserve SQL typing and CLR result semantics separately. Remove a cast only when its store-type effect is redundant; casts on constants can establish necessary SQL typing. SQL nullable columns do not alone describe shaper nullability: `DefaultIfEmpty` on a non-nullable scalar may require `COALESCE` to its CLR default.
- Remap dependent state when changing query shape. Subquery pushdown or join replacement must update scoped column references, predicates, orderings, projection bindings, and shapers; retain or recompute identifiers and their comparers when multiplicity or null extension changes.
- Prove pruning safety beyond the final projection. An unused join can still filter or multiply rows, so honor explicit prunability and references from other clauses. Unreferenced columns can still define distinct/set-operation membership or a scalar subquery's result; preserve those contracts and compatible set-operation projections.

## Split Queries

- When changing `SplitQueryResultCoordinator`, check both `SplitQueryingEnumerable` and `GroupBySplitQueryingEnumerable`, including their synchronous and asynchronous paths. End-of-stream validation belongs at true outer-stream exhaustion, not at a group boundary or early disposal.
- A grouped reader can reach physical EOF while assembling a final group that still must be yielded. Test that successful move separately from the subsequent terminal move, including verification failure and repeated terminal calls; cleanup must preserve the original failure.
- Account for internal buffering as well as caller materialization when assessing memory use. Earlier split-query result sets may need buffering when the provider cannot keep multiple readers active; retrying execution strategies can also require buffering.
- Buffering does not give separate query commands a consistent database snapshot. Concurrent writes between commands can produce an inconsistent result graph; consider snapshot or serializable transactions when consistency is required, accounting for provider support and transaction costs.
- Prove cross-command consistency with a writer transaction and an explicit barrier between reads, not sleeps or a prose-only scenario. Assert a root/child value combination, not just matching keys; state the isolation and retry-strategy boundaries of the test.

## Validation

- `PrecompiledQueryTestHelpers` verifies generation, compilation, and execution.
- `ToQueryString()` shows generated SQL without executing
- `ExpressionPrinter` dumps expression trees at any pipeline stage
- SQL baselines verified via `AssertSql()` in provider functional tests and the generated SQL corresponds to the LINQ query in the base method
