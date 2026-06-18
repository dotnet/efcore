# AI Triage — dotnet/efcore#38429: `xml` constants do not have the `N` prefix

> The below is an AI-generated analysis and may contain inaccuracies.

- **Issue:** [dotnet/efcore#38429](https://github.com/dotnet/efcore/issues/38429)
- **Type:** Bug
- **Suggested area label:** `area-sqlserver` (SQL Server provider, literal/type-mapping; surfaces during query/`ExecuteUpdate` SQL generation)
- **Affects:** SQL Server provider, all current versions (reproduced on `main` and reported on 10.0.9). This is **long-standing behavior, not a recent regression**.

## Summary

When a value typed as SQL Server `xml` is emitted as a **constant** in generated SQL (e.g. in a `SELECT`
projection or an `UPDATE ... SET` clause), EF Core does **not** prefix the string literal with `N`. The
literal is therefore a non-Unicode `varchar`, gets interpreted in the database collation's code page, and
any character outside that code page (e.g. emoji, many CJK characters) is silently replaced with `?` —
**data loss**.

Note: this only affects **literals/constants**. Values sent as **parameters** are unaffected (they go out as
`nvarchar`).

## Root cause

`xml` is mapped by `SqlServerStringTypeMapping`, created as Unicode:

```
// src/EFCore.SqlServer/Storage/Internal/SqlServerTypeMappingSource.cs
private static readonly SqlServerStringTypeMapping Xml
    = new("xml", unicode: true, storeTypePostfix: StoreTypePostfix.None);
```

But the decision to emit the `N` prefix is gated on `_isUtf16`, which additionally requires the **store type
name to start with `n`**:

```
// src/EFCore.SqlServer/Storage/Internal/SqlServerStringTypeMapping.cs:113
_isUtf16 = parameters.Unicode && parameters.StoreType.StartsWith("n", StringComparison.OrdinalIgnoreCase);
```

`xml` is the **only** SQL Server string store type that is `unicode: true` yet does not start with `n`
(the others are `nchar`, `nvarchar`, `ntext`). So for `xml`, `_isUtf16 == false`, and every `N`-prefix
branch in `GenerateNonNullSqlLiteral` is skipped.

The reporter's suggested fix (`_isUtf16 = parameters.Unicode;`) is the most direct correction. See the
*Consequences* section below for an important caveat before adopting it unconditionally.

## Minimal repro

The generated SQL can be inspected without a database via `ToQueryString()`:

<details>
<summary>minimal repro</summary>

```csharp
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

using var db = new MyDb();
// '\U0001F600' == 😀
Console.WriteLine(db.MyEntities.Select(e => e.SomeXml ?? "\U0001F600").ToQueryString());

public class MyDb : DbContext
{
    public DbSet<MyEntity> MyEntities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<MyEntity>().Property(p => p.SomeXml).HasColumnType("xml");

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Data Source=.");
}

public class MyEntity
{
    public int Id { get; set; }
    public string? SomeXml { get; set; }
}
```

</details>

**Generated SQL (verified against EF `main`):**

```sql
SELECT COALESCE([m].[SomeXml], '😀')
FROM [MyEntities] AS [m]
```

The `'😀'` literal has no `N` prefix. For comparison, the identical model with an `nvarchar(max)` column
produces `N'😀'`.

**End-to-end data loss (verified on SQL Server 2025, collation `SQL_Latin1_General_CP1_CI_AS`):**

```sql
-- current EF behavior (no N): emoji is lost
DECLARE @t TABLE(x xml);
INSERT INTO @t VALUES (COALESCE(CAST(NULL AS xml), '<r>😀</r>'));
SELECT CONVERT(varchar(50), x) FROM @t;   -- => <r>??</r>

-- with N prefix: emoji preserved
DECLARE @t2 TABLE(x xml);
INSERT INTO @t2 VALUES (COALESCE(CAST(NULL AS xml), N'<r>😀</r>'));
SELECT CONVERT(nvarchar(50), x) FROM @t2; -- => <r>😀</r>
```

Round-trip of a single emoji confirms the cause is code-page narrowing, not display:

| Literal | `CONVERT(varbinary, CAST(... AS nvarchar))` | Meaning |
|---|---|---|
| `'😀'` (no `N`) | `0x3F003F00` | two `?` (U+003F) — **lost** |
| `N'😀'` | `0x3DD800DE` | surrogate pair D83D DE00 — **preserved** |

## Workarounds

### 1. The suggested "configure the property as Unicode" — does NOT work ❌

`modelBuilder.Entity<MyEntity>().Property(p => p.SomeXml).HasColumnType("xml").IsUnicode(true)` produces the
**same** SQL (`COALESCE([m].[SomeXml], '')`, no `N`). Verified via `ToQueryString()`.

Reason: the property is already Unicode (the `xml` mapping is `unicode: true`). `IsUnicode(true)` does not
change the store type, which remains `"xml"`, so the `StartsWith("n")` guard still fails and `_isUtf16`
stays `false`. There is no model-configuration flag that flips this guard.

### 2. Force the value to be a parameter instead of a constant — works ✅ (recommended)

Capture the literal in a variable so EF parameterizes it; parameters are emitted as `nvarchar` and preserve
Unicode:

```csharp
string fallback = "";
db.MyEntities.Select(e => e.SomeXml ?? fallback);
```

Generated SQL (verified):

```sql
DECLARE @fallback nvarchar(4000) = N'';
SELECT COALESCE([m].[SomeXml], @fallback)
FROM [MyEntities] AS [m]
```

The same idea applies to `ExecuteUpdate`: pass the new value through a captured variable / closure rather
than a string literal, so it becomes a parameter (`SetProperty(e => e.SomeXml, _ => myVariable)`).

### 3. Map the column as `nvarchar(max)` instead of `xml` — works, with caveats ⚠️

`HasColumnType("nvarchar(max)")` makes literals emit `N'...'`. Only viable if you do not need server-side
`xml` typing (XQuery/`.value()`/`.nodes()`, schema validation, etc.).

### 4. Custom type mapping (advanced) ⚠️

A provider plugin returning a `SqlServerStringTypeMapping` subclass for `xml` that overrides
`GenerateNonNullSqlLiteral` (or otherwise forces the `N` prefix) fixes it without touching EF. Heavy for an
app-level fix; mainly relevant if waiting for an upstream fix is not an option.

## Consequences of making the `xml` type mapping emit Unicode (`N`) literals by default

This is the proposed real fix (e.g. `_isUtf16 = parameters.Unicode;`). It is **correct for the reported
data-loss case**, but it has a real SQL Server interaction worth calling out:

### Upside
- Eliminates silent data loss for any `xml` literal containing characters outside the database code page
  (emoji, supplementary-plane characters, many non-Latin scripts). This is the bug being fixed.
- Brings `xml` in line with `nvarchar`/`nchar`/`ntext`, all of which already emit `N`.
- Scope is narrow: `xml` is the only affected store type, so no other mapping changes behavior.

### Downside / risk — XML encoding declarations (`Msg 9402`)
SQL Server **rejects** an `nvarchar` literal that contains an explicit XML prolog encoding declaration when
it is converted to `xml`, because the declared encoding (e.g. `utf-8`) conflicts with the UTF-16 `nvarchar`
string. The non-Unicode form is accepted. Verified on SQL Server 2025:

```sql
-- current (no N): OK
DECLARE @x xml = '<?xml version="1.0" encoding="utf-8"?><r>a</r>';   -- succeeds

-- proposed (with N): fails
DECLARE @y xml = N'<?xml version="1.0" encoding="utf-8"?><r>a</r>';
-- Msg 9402: XML parsing: line 1, character 38, unable to switch the encoding
```

So flipping `xml` to emit `N` literals would **break** queries that embed an `xml` string constant whose
content begins with `<?xml ... encoding="..."?>` (a non-UTF‑16 encoding). Such prologs are common when an
application round-trips XML that was originally produced elsewhere.

Note this is a fundamental SQL Server tension, not something EF can fully paper over with the literal form:
- Without `N`: characters outside the DB code page are lost (the current bug).
- With `N`: literals carrying a conflicting `encoding="..."` prolog throw `Msg 9402`.

There is no single literal spelling that is correct for both. The genuinely robust path for callers is to
**not** embed an encoding declaration in an `xml` value that is being assigned from a string, or to send the
value as a parameter (Workaround #2), which avoids inline literal conversion entirely.

### Recommendation
- Adopt the `N`-prefix fix for `xml` (correctness; matches the other Unicode string types). The data-loss
  failure is silent and corrupts data, whereas the encoding-declaration failure is loud (`Msg 9402`) and
  has a clear remediation.
- Call out the `Msg 9402` behavior change in release notes / breaking-change docs so users who currently
  rely on `xml` string **constants** with an `encoding="..."` prolog can switch to parameters or strip the
  declaration.

## Possible duplicates / related

No existing open issue was found that specifically covers the missing `N` prefix on `xml` literals; #38429
appears to be the canonical report. Related area: the broader family of `SqlServerStringTypeMapping`
literal-generation issues and the historical `varchar` vs `nvarchar` literal handling.
