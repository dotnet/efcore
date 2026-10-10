# AI Triage

The below is an AI-generated analysis and may contain inaccuracies.

Issue: https://github.com/dotnet/efcore/issues/39206

## Classification

- Type: **Bug**
- Area: **area-query**
- Security concern: **None identified**
- Provider-specific: **No**. SQL Server exposes the problematic SQL shape, but the consistency requirement belongs to relational split-query processing.

The issue type was set to `Bug` and the `area-query` label was added. No issue comment was posted.

## Result

The reported SQL-generation defect is confirmed on **9.0.10**, **10.0.10**, and **main** at commit [`2b992857815f044304276565bf912d968c160495`](https://github.com/dotnet/efcore/commit/2b992857815f044304276565bf912d968c160495).

The projection containing the reference navigation causes the root query to be pushed into a paging subquery before the split-query key ordering is appended. Consequently, the key is present only in the outer ordering and not in the ordering that determines which rows pass through `OFFSET/FETCH`. The collection query repeats the same non-deterministic paging subquery. Separate executions can therefore select different parent rows when `Name` is not unique, leaving projected child collections empty.

The SQL defect is deterministic and sufficient to confirm the bug without relying on SQL Server to choose different tied rows during a particular execution. A test database with 20 parents sharing the same `Name` and one child each returned no empty collections in the observed runs; that does not disprove the defect because SQL does not define how tied rows are selected.

## Version comparison

| Version | Reference-navigation projection | Control without reference projection |
| --- | --- | --- |
| 9.0.10 | Paging subquery orders by `Name` only; key missing | Root query orders by `Name, Id`, but the collection paging subquery orders by `Name` only. This is the known pre-10 behavior fixed by #26808. |
| 10.0.10 | Paging subquery orders by `Name` only; key missing | Both root and collection paging queries order by `Name, Id`; #26808 works for this shape. |
| main (`2b992857`) | Paging subquery orders by `Name` only; key missing | Both root and collection paging queries order by `Name, Id`; same as 10.0.10. |

The reference-navigation projection produces this relevant fragment on all three versions:

```sql
FROM (
    SELECT [p].[Id], [p].[LocationId], [p].[Name]
    FROM [Parents] AS [p]
    ORDER BY [p].[Name]
    OFFSET @p ROWS FETCH NEXT @p1 ROWS ONLY
) AS [p0]
INNER JOIN [Location] AS [l] ON [p0].[LocationId] = [l].[Id]
ORDER BY [p0].[Name], [p0].[Id], [l].[Id]
```

On main, the collection query similarly pages before the key ordering:

```sql
SELECT [c1].[Id], [p0].[Id]
FROM (
    SELECT [p].[Id], [p].[Name]
    FROM [Parents] AS [p]
    ORDER BY [p].[Name]
    OFFSET @p ROWS FETCH NEXT @p1 ROWS ONLY
) AS [p0]
INNER JOIN [Child] AS [c1] ON [p0].[Id] = [c1].[ParentId]
ORDER BY [p0].[Name], [p0].[Id]
```

Removing `LocationName = p.Location.Name` is a useful control. On 10.0.10 and main, both split queries then place `[p].[Id]` inside the paging `ORDER BY`. On 9.0.10, the root query has the key but the collection subquery does not, matching the behavior fixed for EF 10 by #26808.

## Root-cause direction

The relevant logic is in [`SelectExpression.ApplyProjection`](https://github.com/dotnet/efcore/blob/2b992857815f044304276565bf912d968c160495/src/EFCore.Relational/Query/SqlExpressions/SelectExpression.cs#L610-L650) and its split-collection handling:

1. A single-result/reference projection combined with paging causes the outer select to be pushed down early.
2. The split-query fix from #26808 later appends identifier orderings to the current select and its clone.
3. At that point, the paging operation is already inside a subquery, so appending the identifiers affects only the outer select. It does not alter the inner ordering that controls `OFFSET/FETCH`.

The fix should preserve the #26808 invariant when an earlier pushdown was caused by a reference/single-result projection: identifier orderings must reach the select containing `Limit`/`Offset`, not only its outer wrapper. A regression test should cover a projected reference navigation plus a projected collection, non-unique ordering, and `Skip/Take`, and assert both SQL and materialized child collections.

## Duplicate search

- [#26808](https://github.com/dotnet/efcore/issues/26808) is the original bug fixed by [#34097](https://github.com/dotnet/efcore/pull/34097). Issue #39206 is best treated as an **incomplete-fix regression/follow-up**, not simply closed as a duplicate, because the exact projection/reference-navigation shape still fails in 10.0.10 and main.
- [#35144](https://github.com/dotnet/efcore/issues/35144), [#34722](https://github.com/dotnet/efcore/issues/34722), and [#36246](https://github.com/dotnet/efcore/issues/36246) were closed as duplicates of #26808 and demonstrate the older include/control shape.
- [#25260](https://github.com/dotnet/efcore/issues/25260), [#27188](https://github.com/dotnet/efcore/issues/27188), and [#31975](https://github.com/dotnet/efcore/issues/31975) report the same general split-query paging inconsistency before the #26808 fix.

No existing issue found in the search describes this EF 10 projection-with-reference-navigation gap as precisely as #39206.

## Minimal repro

<details>
<summary>minimal repro</summary>

```csharp
using Microsoft.EntityFrameworkCore;

using var db = new Db();

var query = db.Parents
    .AsSplitQuery()
    .Select(p => new
    {
        p.Id,
        p.Name,
        LocationName = p.Location.Name,
        ChildIds = p.Children.Select(c => c.Id)
    })
    .OrderBy(x => x.Name)
    .Skip(0)
    .Take(10);

var sql = query.ToQueryString();
var offset = sql.IndexOf("OFFSET");
var pagingOrderBy = sql[sql.LastIndexOf("ORDER BY", offset)..offset];

Console.WriteLine(sql);
Console.WriteLine();
Console.WriteLine($"Paging ORDER BY contains the key [p].[Id]: {pagingOrderBy.Contains("[p].[Id]")}");

class Db : DbContext
{
    public DbSet<Parent> Parents => Set<Parent>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=.;Database=NotUsed");
}

class Parent
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public Location Location { get; set; } = null!;
    public List<Child> Children { get; set; } = [];
}

class Location
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

class Child
{
    public int Id { get; set; }
}
```

</details>
