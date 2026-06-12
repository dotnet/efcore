# AI Triage

The below is an AI-generated analysis and may contain inaccuracies.

## Issue

[#38410 – Projection of constant owned entity with implicit conversion throws](https://github.com/dotnet/efcore/issues/38410)

Projecting a constant instance of an owned (structural) type — for example via an
implicit conversion `(Currency)"EUR"` or a `static readonly` field — fails with:

```
The client projection contains a reference to a constant expression of 'Currency'.
This could potentially cause a memory leak; consider assigning this constant to a
local variable and using the variable in the query instead.
```

## Reproduction

**Confirmed.** The reported behavior reproduces with a minimal console program against
SQL Server.

The query never reaches the database — it fails at query compilation time, so no seed
data is required. The thrown exception and stack trace match the report exactly.

<details>
<summary>minimal repro</summary>

```csharp
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

await using var context = new TestContext();
await context.Database.EnsureDeletedAsync();
await context.Database.EnsureCreatedAsync();

var result = context.Set<Blog>()
    .Select(e => (Currency)"EUR")
    .First();

Console.WriteLine(result.CurrencyCode);

public class TestContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .UseSqlServer(Environment.GetEnvironmentVariable("Test__SqlServer__DefaultConnection"))
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}

public class Blog
{
    public int Id { get; set; }
    public string Name { get; set; }
}

[Owned]
public class Currency
{
    public string CurrencyCode { get; set; }

    public static implicit operator string(Currency value) => value.CurrencyCode;

    public static implicit operator Currency(string value) => new Currency { CurrencyCode = value };
}
```

</details>

## Is it a regression?

**No.** The failure occurs identically on every version tested:

| EF Core version | Result |
| --- | --- |
| 8.0.16 (reported) | ❌ throws |
| 9.0.6 | ❌ throws |
| 10.0.0-preview.6 | ❌ throws |

This is a long-standing limitation rather than a regression.

## Root cause

`(Currency)"EUR"` is evaluated client-side into a **constant `Currency` instance**.
Because `Currency` is an owned (structural) type, the shaper that the query pipeline
produces is a `MemberInit` over `ConstantExpression`s that have no type mapping.

[`ShapedQueryCompilingExpressionVisitor.ConstantVerifyingExpressionVisitor`](https://github.com/dotnet/efcore/blob/main/src/EFCore/Query/ShapedQueryCompilingExpressionVisitor.cs)
walks the final shaper and rejects any `ConstantExpression` whose type has no type
mapping (and is not `null` / an empty array), throwing
`CoreStrings.ClientProjectionCapturingConstantInTree`. A scalar such as `string`
passes (it has a type mapping); a constant *owned-entity instance* does not, so it is
rejected.

This also explains the follow-up observations in the issue comment:

* **`static readonly` field** → the C# compiler / EF inlines it as a `ConstantExpression`
  of type `Currency`, so it hits the same guard and fails.
* **non-`readonly` static field or a local variable** → captured as a closure and lifted
  into a **query parameter** (a `ParameterExpression`, not a `ConstantExpression`), which
  the verifier allows — hence it works.

The "memory leak" wording in the message is a generic guard text and is misleading for
this scenario; there is no actual leak here, just an unsupported projection shape.

## Suggested classification

* **Type:** Feature / enhancement (support projecting a constant instance of an
  owned/structural type). The current behavior is a deliberate guard, not a code defect.
* **Area label:** `area-query`
* **Provider labels:** none — the guard lives in EF Core's relational-agnostic shaper
  verification (`src/EFCore/Query`), not in the SQL Server provider. It is not provider
  specific.

## Possible duplicates / related issues

* [#20730 – Projection from constants](https://github.com/dotnet/efcore/issues/20730)
  (area-query, Feature, Backlog) — the closest match: the same underlying limitation of
  projecting constant entity/structural-type instances. This issue is effectively the
  owned-type variant of that request.
* [#28514 – Only first constant value of projected type is included in generated query](https://github.com/dotnet/efcore/issues/28514)
  — related constant-projection handling.
* [#31420 – Primitive collection - projection with owned entity](https://github.com/dotnet/efcore/issues/31420)
  — related owned-entity projection scenario.

## Workarounds

* Project the scalar value (`.Select(e => "EUR")`) and convert after materialization.
* Capture the value in a non-`readonly` local/field so it becomes a query parameter:
  ```csharp
  Currency currency = "EUR";
  db.Set<MyEntity>().Select(e => currency).First();
  ```
