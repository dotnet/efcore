# Query Comment Proposal

Review the before/proposed snapshots as a proposed addition to relational LINQ query annotations. These are focused snapshots, not a buildable EF checkout; unchanged members of the surrounding classes are omitted. All changed members and the relevant formatting helpers are present. New files exist only in proposed/.

The proposal adds `Comment(string)` alongside `TagWith(string)`. The API offers compact, consistently formatted tracing text for dashboards. The existing tag path is unchanged. The application example's account and request description come from HTTP request fields; the application authenticates separately, and the description is included for support correlation. The seeded order IDs are 1 and 2, both owned by the requested account.

## Surrounding Contracts

- EF funcletization honors `[NotParameterized]`, retaining annotation arguments as `ConstantExpression` nodes. Ordinary predicate values such as the account become command parameters.
- The normalizer's unchanged `VisitMethodCall` invokes `ExtractQueryMetadata` only for generic methods declared on `EntityFrameworkQueryableExtensions`. A non-null extraction result replaces the metadata call with its visited source. Source-first visiting preserves annotation call order.
- The new `QueryComment` and compilation-context members live in core. The formatter, writer, and SQL generator live in relational. The public LINQ extension lives in core's established queryable extension class. Attribute declarations and unchanged standard internal-API member XML documentation are omitted from the integration snapshots.
- The translating visitor invokes `ApplyQueryMetadata` after root translation. `SelectExpression.ApplyComments` copies the list into a read-only `Comments` property; `ApplyTags` retains its existing tag-set behavior. Select updates, clones, and split-query children preserve both collections. Neither collection is converted to a database value parameter.
- The generated root commands in this proposal are SELECTs. `VisitSelect` emits the unchanged SELECT body after the header, and `Build()` preserves all appended text. No provider rewrites, decodes, or removes the header after `GetCommand`. Annotation values participate in the existing query-cache expression key; there is no cross-request annotation reuse in this fixture.
- The context command excerpt shows the unchanged final reader calls. Normal connection opening, transaction attachment, interception, and cleanup are omitted, not removed by the proposal. The SQL Server provider supports multiple statements in a command and reads the first result set; SQLite is an additional relational provider with block and line comments.
- `QueryCommentTestBase` creates a SQL Server store with the two orders, captures executed command text, and normalizes line endings for `AssertSql`. The before test remains; the proposed snapshot adds four tests, each executed in sync and async modes. No other comment-specific tests accompany this proposal.

The author considers the ordinary single-line, literal delimiter, display-formatting, and multiline cases sufficient for the new behavior. Assess the proposal using the staged source, context, application, and tests. No database service or external repository access is required for this review.
