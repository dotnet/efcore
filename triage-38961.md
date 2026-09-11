# AI Triage

The below is an AI-generated analysis and may contain inaccuracies.

## Issue

[#38961 - EF10 Upgrade cause Regression in query times due to linq-translations](https://github.com/dotnet/efcore/issues/38961)

## Classification

- **Type**: Bug (regression)
- **Area labels**: `area-query`, `area-perf`
- **Other labels**: `regression`, `customer-reported` (already present)

## Analysis

The reporter projects a single "latest comment" per event via a correlated subquery
(`Where` → `OrderByDescending` → `Take(1)` → `Select` → `FirstOrDefault`) inside a
`Select` projection. In EF Core 9, this translates to `OUTER APPLY` with `TOP(1)`,
which SQL Server can execute efficiently using an index seek per outer row. In EF
Core 10, the same LINQ translates instead to a `LEFT JOIN` against a subquery that
uses `ROW_NUMBER() OVER (PARTITION BY ... ORDER BY ...)` filtered to `row <= 1`.
For large tables, SQL Server's query optimizer handles the `ROW_NUMBER`/`LEFT JOIN`
shape far worse than the `APPLY` shape (window function must be computed/sorted
over the whole partition space rather than allowing a per-row seek), which matches
the reporter's observation (~100 ms vs. ~11 s).

This is caused by EF Core's "convert APPLY to JOIN" optimization
(`SelectExpression.AddJoin`, see
[`src/EFCore.Relational/Query/SqlExpressions/SelectExpression.cs`](https://github.com/dotnet/efcore/blob/main/src/EFCore.Relational/Query/SqlExpressions/SelectExpression.cs)
around the `JoinType.CrossApply or JoinType.OuterApply` handling and the
`RowNumberExpression`/`GetPartitions` logic), which attempts to rewrite `APPLY`
operations with a `Limit`/`Offset` into a `ROW_NUMBER`-based `LEFT JOIN` whenever a
join key can be extracted and there is no other outer correlation left inside the
subquery. This heuristic seems to have become more aggressive/applicable between
EF9 and EF10 for this particular shape (single-column `Take(1)` correlated
subquery), even though the resulting SQL can perform drastically worse than the
`APPLY` alternative it replaces.

### Repro confirmation

Using a minimal self-referencing model reproducing the reported shape:

```csharp
var query = ctx.Events
    .Select(e => new Response
    {
        LatestComment = e.Comments
            .Where(c => c.IsVisible)
            .OrderByDescending(c => c.CreatedDate)
            .Take(1)
            .Select(c => new CommentDto { CreatedDate = c.CreatedDate, Text = c.Text })
            .FirstOrDefault()
    });
```

**EF Core 9.0.9** generates (`OUTER APPLY`):

```sql
SELECT [c1].[CreatedDate], [c1].[Text], [c1].[c]
FROM [Events] AS [e]
OUTER APPLY (
    SELECT TOP(1) [c0].[CreatedDate], [c0].[Text], 1 AS [c]
    FROM (
        SELECT TOP(1) [c].[CreatedDate], [c].[Text]
        FROM [Comment] AS [c]
        WHERE [e].[Id] = [c].[EventId] AND [c].[IsVisible] = CAST(1 AS bit)
        ORDER BY [c].[CreatedDate] DESC
    ) AS [c0]
    ORDER BY [c0].[CreatedDate] DESC
) AS [c1]
```

**EF Core 10.0.11** generates (`LEFT JOIN` + `ROW_NUMBER()`):

```sql
SELECT [c1].[CreatedDate], [c1].[Text], [c1].[c]
FROM [Events] AS [e]
LEFT JOIN (
    SELECT [c0].[CreatedDate], [c0].[Text], [c0].[c], [c0].[EventId]
    FROM (
        SELECT [c].[CreatedDate], [c].[Text], 1 AS [c], [c].[EventId],
               ROW_NUMBER() OVER(PARTITION BY [c].[EventId] ORDER BY [c].[CreatedDate] DESC) AS [row]
        FROM [Comment] AS [c]
        WHERE [c].[IsVisible] = CAST(1 AS bit)
    ) AS [c0]
    WHERE [c0].[row] <= 1
) AS [c1] ON [e].[Id] = [c1].[EventId]
```

This confirms the regression exists independent of whether the outer `Id` is
included in the projection (in this minimal repro both variants used `LEFT JOIN`
in EF10 and both used `OUTER APPLY` in EF9). The reporter's observation that adding
`Id` to the projection "forces" `APPLY` suggests the presence/absence of the id in
the outer projection affects whether the join-key extraction/no-other-outer-reference
condition succeeds in their exact model (e.g. due to additional columns, indexes, or
other correlated references), but the underlying cause — the APPLY→JOIN/ROW_NUMBER
conversion — is the same in both cases.

<details>
<summary>minimal repro</summary>

```csharp
using Microsoft.EntityFrameworkCore;

using var ctx = new TestContext();

var query1 = ctx.Events
    .Select(e => new Response
    {
        LatestComment = e.Comments
            .Where(c => c.IsVisible)
            .OrderByDescending(c => c.CreatedDate)
            .Take(1)
            .Select(c => new CommentDto { CreatedDate = c.CreatedDate, Text = c.Text })
            .FirstOrDefault()
    });

Console.WriteLine("=== WITHOUT Id in outer projection ===");
Console.WriteLine(query1.ToQueryString());

var query2 = ctx.Events
    .Select(e => new Response
    {
        Id = e.Id,
        LatestComment = e.Comments
            .Where(c => c.IsVisible)
            .OrderByDescending(c => c.CreatedDate)
            .Take(1)
            .Select(c => new CommentDto { CreatedDate = c.CreatedDate, Text = c.Text })
            .FirstOrDefault()
    });

Console.WriteLine("=== WITH Id in outer projection ===");
Console.WriteLine(query2.ToQueryString());

public class TestContext : DbContext
{
    public DbSet<Event> Events => Set<Event>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=.;Database=Test;Trusted_Connection=True;");
}

public class Event
{
    public int Id { get; set; }
    public List<Comment> Comments { get; set; } = new();
}

public class Comment
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public bool IsVisible { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Text { get; set; } = "";
}

public class Response
{
    public int Id { get; set; }
    public CommentDto? LatestComment { get; set; }
}

public class CommentDto
{
    public DateTime CreatedDate { get; set; }
    public string Text { get; set; } = "";
}
```

</details>

### Regression status

Confirmed regression: EF Core 9.0.9 produces `OUTER APPLY` for this query shape;
EF Core 10.0.11 produces `LEFT JOIN` + `ROW_NUMBER()`, which the reporter measured
as ~100x slower (100 ms vs. 11 s) against their real dataset/indexes.

## Possible related/duplicate issues

- [#30450 - CROSS APPLY vs. ROW_NUMBER()](https://github.com/dotnet/efcore/issues/30450)
- [#17936 - JOIN instead of CROSS APPLY in generated query in SQL Server](https://github.com/dotnet/efcore/issues/17936)
- [#20608 - Query: Convert Apply to Join for collection projection](https://github.com/dotnet/efcore/issues/20608) (PR [#21295](https://github.com/dotnet/efcore/pull/21295))
- [#19825 - Query: incorrectly generating JOIN rather than APPLY for subqueries with outside references to a joined table](https://github.com/dotnet/efcore/issues/19825)
- [#27267 - Avoid automatic convert nested queries to joins](https://github.com/dotnet/efcore/issues/27267)
- [#32488 - Query: unnecessary APPLY generated where JOIN is enough is some cases](https://github.com/dotnet/efcore/issues/32488) (inverse concern, same area of logic)

None of these are exact duplicates of this report (a specific EF9→EF10 performance
regression), but they all relate to the same APPLY↔JOIN/ROW_NUMBER conversion logic
in `SelectExpression`, which is the likely area to investigate for a fix.
