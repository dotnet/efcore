# Proposed Single-Result Reuse Pass

The proposal adds `proposed-optimization.cs` to navigation expansion. Before this
addition, each member read is processed through the existing member-pushdown and
navigation-expansion paths. The new pass runs after selector/transparent-identifier
composition and before pending-selector expansion. It replaces repeated member
reads from one single-result expression with one `SelectMany` source capped at
one row, aiming to reduce repeated correlated SQL subqueries.

The surrounding pipeline provides `ProcessSelectMany`, `QueryableMethods`,
`ReplacingExpressionVisitor`, model metadata, the configured evaluatable-expression
filter, and access to applicable query filters. Navigation expansion later adds
joins for navigations and applies those query filters. Existing member pushdown
can place a member selector inside a scalar subquery before its terminal operator.
The pass participates in the shared preprocessor used by multiple providers.

`model-and-queries.cs` contains sample application queries and a programmatically
built selector used by the query-composition layer. Selector composition can reuse
the same method-call node for both reads of a `let` binding. Model classes and
seed rows are included; these excerpts are not a standalone buildable project.

Initial smoke coverage compares the SQL command count for an ordered, populated
source projecting two scalar properties. It does not yet assert results for the
other supplied shapes. No database or test dependencies are staged.

## Available integration contracts

The other partial visitor declarations provide these members; use them rather
than inventing service locators or hard-coded provider names:

- `_queryCompilationContext.Model` is the current `IModel`.
- `_evaluatableExpressionFilter.IsEvaluatableExpression(Expression, IModel)` is
	the configured core/provider expression policy.
- `_extensibilityHelper.SupportsSingleResultLifting` is the provider capability
	switch. Providers that have not opted in return false.
- `GetApplicableQueryFilters(IEntityType)` returns the enabled filter metadata
	entries, each exposing its lambda as `Expression`; ignored filters are absent.
- `HasApplicableQueryFilters(IEntityType)` tests that same enabled set.
- `ProcessSelectMany` performs the existing navigation-tree and selector
	remapping and returns the updated `PendingSelector` and `SourceElementType`.

Test excerpts may use the repository's existing query fixtures and expression
test helpers. Include concrete setup, calls, and assertions, and identify omitted
fixture bindings in analysis.md. New production helpers must have implementations;
an unexplained eligibility predicate or a test calling an invented helper is not
an integration-ready change.