# Triage: #38762 — Shaper discards null checks

**Issue:** https://github.com/dotnet/efcore/issues/38762

## Summary

The issue reports a `NullReferenceException` thrown from the generated shaper lambda
(`lambda_method...`) when a query:

1. Uses a `LeftJoin` producing an anonymous type wrapping an entity and a nullable
   (`Include`-d) navigation entity.
2. Projects that anonymous type into a positional-constructor type (a `record struct`,
   `TestRecord(Table3Entry, Table2Entry)`), duplicating the source expression into
   multiple constructor-parameter positions.
3. Adds a further `.Select(...)` on top that performs null-conditional access
   (`e.Table2Entry == null ? null : e.Table2Entry.Field1`, and nested
   `e.Table2Entry == null || e.Table2Entry.Table1Entry == null ? null : ...`) against
   members of the intermediate record's properties.

Per the issue, the shaper expression logged by the reporter shows the null checks
(`.If (...) { null } .Else { ... }`) still present in the tree, but the accesses feeding
those checks (`(new TestRecord(...)).Table2Entry).Table1Entry`) are re-evaluated
separately from the ones used inside the `.If`/`.Else` branches, rather than reusing a
single materialized/null-checked value — leading to a `NullReferenceException` when
dereferencing a null navigation at runtime.

## Verification against `main`

I reproduced the repro from the issue as an ad-hoc functional test added temporarily to
`test/EFCore.Sqlite.FunctionalTests/Query/AdHocMiscellaneousQuerySqliteTest.cs`, built at
the current tip of `main` (commit `dbf9771`), using SQLite with a `TestRecord`
record struct with the same shape (`(TestTable3 Table3Entry, TestTable2? Table2Entry)`),
navigations, and the same follow-up `.Select` with null-conditional member access
patterns described in the issue.

Running the test against the current `main` build reproduces the same failure:

```
Xunit.MicrosoftTestingPlatform.XunitException: System.NullReferenceException : Object reference not set to an instance of an object.
   at lambda_method153(Closure, QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator)
   at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()
   at Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync[TSource](IQueryable`1 source, CancellationToken cancellationToken)
```

The temporary repro test was reverted after verification (not committed) since this was
purely a diagnostic check; no fix has been applied.

## Conclusion

**Not fixed on `main`.** The bug is still present at commit `dbf9771` (current tip at
time of triage). The issue should remain open and can be labeled for investigation into
the shaper's handling of null-check reuse across duplicated member accesses that
originate from a projected record/struct constructor.
