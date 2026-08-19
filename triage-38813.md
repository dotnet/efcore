# AI Triage

The below is an AI-generated analysis and may contain inaccuracies.

## Conclusion

Confirmed as a longstanding query bug and an exact duplicate of [#19095](https://github.com/dotnet/efcore/issues/19095). The generated `OUTER APPLY` incorrectly moves the projected outer entity columns into the nullable inner subquery. When the inner collection is empty, SQL returns `NULL` for those columns and EF consequently materializes the outer entity as `null`.

Recommended triage:

- Type: `Bug`
- Add label: `area-query`
- Keep label: `customer-reported`
- Close as a duplicate of [#19095](https://github.com/dotnet/efcore/issues/19095)
- Reopen #19095 because [#36248](https://github.com/dotnet/efcore/pull/36248) did not fix its minimal reported query shape

No security concern was identified.

## Reproduction

Reproduced against SQL Server with a minimized model containing two outer rows, only one of which has an inner row.

<details>
<summary>minimal repro</summary>

```csharp
using Microsoft.EntityFrameworkCore;

await using (var context = new Context())
{
    await context.Database.EnsureDeletedAsync();
    await context.Database.EnsureCreatedAsync();

    var first = new Outer();
    var second = new Outer();
    second.Inners.Add(new Inner());
    context.AddRange(first, second);
    await context.SaveChangesAsync();
}

await using (var context = new Context())
{
    var results = await context.Outers
        .OrderBy(o => o.Id)
        .SelectMany(o => o.Inners.Take(1).DefaultIfEmpty().Select(i => new { Outer = o, Inner = i }))
        .ToListAsync();

    Console.WriteLine(
        $"Rows: {results.Count}; "
        + $"null outers: {results.Count(r => r.Outer is null)}; "
        + $"null inners: {results.Count(r => r.Inner is null)}");
}

sealed class Context : DbContext
{
    public DbSet<Outer> Outers => Set<Outer>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(
            Environment.GetEnvironmentVariable("Test__SqlServer__DefaultConnection"));
}

sealed class Outer
{
    public int Id { get; set; }
    public ICollection<Inner> Inners { get; } = new List<Inner>();
}

sealed class Inner
{
    public int Id { get; set; }
    public int OuterId { get; set; }
}
```

</details>

Observed:

```text
Rows: 2; null outers: 1; null inners: 1
```

Expected:

```text
Rows: 2; null outers: 0; null inners: 1
```

The bug reproduces with or without `Take(1)`.

## Affected versions

The minimized repro produced the same incorrect result with:

- EF Core 8.0.11
- EF Core 9.0.11
- EF Core 10.0.11
- EF Core 11.0.0-preview.7.26381.103

This is not a recent regression. The duplicate #19095 reported the same behavior in EF Core 3.1 preview 3.

## Technical analysis

EF Core 10.0.11 generates SQL equivalent to:

```sql
SELECT [i0].[Id], [i0].[Id0], [i0].[OuterId]
FROM [Outers] AS [o]
OUTER APPLY (
    SELECT TOP(1) [o].[Id], [i].[Id] AS [Id0], [i].[OuterId]
    FROM [Inner] AS [i]
    WHERE [o].[Id] = [i].[OuterId]
) AS [i0]
ORDER BY [o].[Id]
```

`[o].[Id]` is selected inside the `OUTER APPLY` and then read as `[i0].[Id]`. It is therefore `NULL` when the apply has no matching inner row. The outer entity should instead be shaped from columns projected directly from `[o]`.

This is the same faulty SQL shape described in #19095:

```sql
SELECT [t].[Id], ...
FROM [Membre] AS [m]
OUTER APPLY (
    SELECT [m].[Id], ...
) AS [t]
```

The current `DefaultIfEmpty` lifting logic allows lifting through `Select`. In this query, that `Select` captures the outer entity inside the collection selector. The resulting projection is treated as the nullable side of the apply, so the captured outer entity shaper is also made nullable. The relevant logic is in [`RelationalQueryableMethodTranslatingExpressionVisitor`](https://github.com/dotnet/efcore/blob/main/src/EFCore.Relational/Query/RelationalQueryableMethodTranslatingExpressionVisitor.cs).

## Related issues

- [#19095](https://github.com/dotnet/efcore/issues/19095): exact duplicate; same LINQ shape, result corruption, and generated SQL defect.
- [#33343](https://github.com/dotnet/efcore/issues/33343): related `DefaultIfEmpty` lifting bug fixed by #36248, but it concerns a nested `DefaultIfEmpty` with different semantics.
- [#30450](https://github.com/dotnet/efcore/issues/30450): related query-shape/performance discussion, but not a duplicate.
- [#30915](https://github.com/dotnet/efcore/issues/30915) and [#22517](https://github.com/dotnet/efcore/issues/22517): related nullable-side projection materialization problems, but they concern non-entity projections rather than outer entity columns being moved into the nullable side.
