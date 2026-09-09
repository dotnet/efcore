# AI Triage

The below is an AI-generated analysis and may contain inaccuracies.

Issue: [#38938 — GetDatabaseValues[Async] does not return ComplexCollection properties](https://github.com/dotnet/efcore/issues/38938)

## Summary

Confirmed: this still reproduces on `main` (commit `957cb20`), and it is not Npgsql-specific — it reproduces with the SQLite provider as well.

`EntityEntry.GetDatabaseValues()` / `GetDatabaseValuesAsync()` return a `PropertyValues` in which every complex **collection** property is `null`:

- `dbValues["Items"]` returns `null`
- `dbValues.ToObject()` produces an entity whose `Items` is `null`
- Consequently `entry.Reload()` also leaves the tracked entity's complex collection as `null`, silently wiping the tracked value.

Non-collection complex properties are unaffected (they were handled by [#32813](https://github.com/dotnet/efcore/pull/32813)); `OriginalValues` and `CurrentValues` for complex collections work as expected.

## Classification

- Type: **Bug**
- Suggested area labels: `area-change-tracking`, `area-complex-types` (provider-independent, so no provider label)

## Repro (SQLite, EF Core built from `main`)

Actual output:

```text
Original Items.Count: 1
Current Items.Count: 2
DB Items.Count: <null>
DB ToObject Items.Count: <null>
After Reload Items.Count: <null>
```

Expected: `DB Items.Count: 1`, `DB ToObject Items.Count: 1`, `After Reload Items.Count: 1`.

<details>
<summary>minimal repro</summary>

```csharp
using System.Collections;
using Microsoft.EntityFrameworkCore;

await using var ctx = new TestContext();
await ctx.Database.EnsureDeletedAsync();
await ctx.Database.EnsureCreatedAsync();
ctx.Things.Add(new Test { Id = 1, Items = [new ComplexItem { Title = "Title1" }] });
await ctx.SaveChangesAsync();

await using var ctx2 = new TestContext();
var thing = await ctx2.Things.SingleAsync();
thing.Items.Add(new ComplexItem { Title = "TestAdd" });

var entry = ctx2.Entry(thing);
Console.WriteLine("Original Items.Count: {0}", Count(entry.OriginalValues[nameof(Test.Items)]));
Console.WriteLine("Current Items.Count: {0}", Count(entry.CurrentValues[nameof(Test.Items)]));

var dbValues = (await entry.GetDatabaseValuesAsync())!;
Console.WriteLine("DB Items.Count: {0}", Count(dbValues[nameof(Test.Items)]));
Console.WriteLine("DB ToObject Items.Count: {0}", Count(((Test)dbValues.ToObject()).Items));

entry.Reload();
Console.WriteLine("After Reload Items.Count: {0}", Count(thing.Items));

static string Count(object o) => o is IList l ? l.Count.ToString() : "<null>";

public sealed class TestContext : DbContext
{
    public DbSet<Test> Things => Set<Test>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .UseSqlite("Data Source=repro.db")
            .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Test>(e =>
        {
            e.ToTable("things");
            e.HasKey(p => p.Id);
            e.ComplexCollection(p => p.Items, p =>
            {
                p.ToJson();
                p.IsRequired();
                p.Property(c => c.Title).IsRequired();
            });
        });
}

public sealed class Test
{
    public int Id { get; set; }
    public List<ComplexItem> Items { get; set; } = null!;
}

public class ComplexItem
{
    public string Title { get; set; } = null!;
}
```

</details>

## Analysis

The database-values path only ever materializes a flat scalar value buffer, and complex collections are excluded from that flattening by design:

- [`EntityFinder.GetDatabaseValuesQuery`](https://github.com/dotnet/efcore/blob/main/src/EFCore/Internal/EntityFinder.cs) projects `Select(BuildProjection(entityType))`, and `BuildProjection` iterates `entityType.GetFlattenedProperties()`, building an `object[]` of scalar values via `EF.Property` chains.
- [`TypeBase.GetFlattenedDeclaredProperties`](https://github.com/dotnet/efcore/blob/main/src/EFCore/Metadata/Internal/TypeBase.cs) explicitly `continue`s for `complexProperty.IsCollection`, so no element of a complex collection is ever projected. A collection also cannot be represented in a flat `object[]` value buffer, since its length is data-dependent.
- [`EntityEntry.GetDatabaseValues()`](https://github.com/dotnet/efcore/blob/main/src/EFCore/ChangeTracking/EntityEntry.cs) then constructs `new ArrayPropertyValues(InternalEntry, values)`.
- [`ArrayPropertyValues`](https://github.com/dotnet/efcore/blob/main/src/EFCore/ChangeTracking/Internal/ArrayPropertyValues.cs) initializes `_complexCollectionValues` to `new List<ArrayPropertyValues?>?[ComplexCollectionProperties.Count]` — i.e. all-`null` — and nothing on this path ever populates it. `ToObject` skips any `null` entry, which is exactly the `_complexCollectionValues[0] = null` the reporter observed via `UnsafeAccessor`.

So this is best described as **unimplemented** rather than a regression: complex collections are new in EF Core 10, and `PropertyValues` support for them was added in [#36366](https://github.com/dotnet/efcore/pull/36366) for the change-tracker-backed paths (current/original values), but the `GetDatabaseValues` projection path was not extended. Fixing it requires the database query to project the complex collections (e.g. via the JSON column / a separate shaping path) rather than a flat scalar buffer, and then populating `ArrayPropertyValues._complexCollectionValues`.

Note also the knock-on effect on `Reload()`, which is arguably the more damaging symptom: reloading an entity with a complex collection nulls out the collection on the tracked instance.

## Possible related / duplicate issues

None appear to be exact duplicates. Related:

- [#36366](https://github.com/dotnet/efcore/pull/36366) — Add complex collection support to `PropertyValues` (added the `_complexCollectionValues` mechanism that is left unpopulated here)
- [#32813](https://github.com/dotnet/efcore/pull/32813) — Handle complex types in `GetDatabaseValues` (the earlier, non-collection counterpart)
- [#38486](https://github.com/dotnet/efcore/issues/38486) — `OriginalValue` access on complex collections not working for entities in `Added` state (the issue the reporter suspected; different code path)
- [#31237](https://github.com/dotnet/efcore/issues/31237) — Complex collections epic
- [#11232](https://github.com/dotnet/efcore/issues/11232) — Owned types not included in `GetDatabaseValues()` (same shape of limitation for owned types)
