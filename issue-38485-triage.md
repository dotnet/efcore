# AI Triage

The below is an AI-generated analysis and may contain inaccuracies.

**Issue:** [#38485 — Allow Shared Columns with ComplexType](https://github.com/dotnet/efcore/issues/38485)
**EF Core version reported:** 10.0.9

## Summary

In a **TPH** (Table-per-Hierarchy) mapping, when two sibling subtypes each declare a complex
property of the same type and the same column-base name, EF Core does **not** share the
underlying columns between them. Instead it appends a numeric suffix (`1`), producing a duplicate
set of columns.

For the reported model, EF generates:

```sql
CREATE TABLE "Thing" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Thing" PRIMARY KEY AUTOINCREMENT,
    "Discriminator" TEXT NOT NULL,
    "Address_City" TEXT NULL,
    "Address_Street" TEXT NULL,
    "Address_City1" TEXT NULL,
    "Address_Street1" TEXT NULL
);
```

The reporter expects the four `Address_*` columns to collapse into two shared columns
(`Address_City`, `Address_Street`), matching the column-sharing behavior that EF already applies to
**scalar** properties of the same name/type across TPH siblings.

This was confirmed reproduced on SQL Server's sibling provider (SQLite) with EF Core 10.0.9 — the
generated `CREATE TABLE` matches the report exactly.

## Classification

- **Type:** Feature / enhancement. Extending scalar TPH column-sharing to complex-type properties.
  The current suffixing behavior is the existing (by-design) behavior for distinct complex
  properties; the ask is to opt complex properties into the same de-duplication logic that scalar
  properties already use.
- **Suggested area labels:** `area-complex-types`, `area-relational-mapping`
  (the column-sharing decision is a relational TPH mapping concern). `area-model-building` is also
  plausible.

## Related issues (no exact duplicate found)

- [#31250 — Add inheritance support for complex types](https://github.com/dotnet/efcore/issues/31250)
- [#31376 — Allow mapping optional complex properties](https://github.com/dotnet/efcore/issues/31376)
- [#25210 — InMemory and SQL Server provider seems to access TPH shared columns differently](https://github.com/dotnet/efcore/issues/25210) (background on scalar TPH shared columns)

## Suggested workaround

The cleanest way to get a single shared set of columns today is to **declare the complex property
once on the base type** so the hierarchy maps a single `Address`, and keep a per-subtype,
strongly-typed (required) accessor that the EF model ignores.

The variant the reporter sketched relies on EF mapping a `protected` base property by convention —
which does **not** happen (convention only discovers public properties), so as written it produces a
table with **no** `Address` columns. It works once the protected base property is mapped
**explicitly** via the Fluent API:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    var thing = modelBuilder.Entity<Thing>();
    thing.ComplexProperty<Address>("Address");   // map the protected base property explicitly
    thing.HasDiscriminator()
        .HasValue<ThingA>("A")
        .HasValue<ThingB>("B");
}

[ComplexType]
public record class Address(string Street, string City);

public abstract class Thing
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    protected Address? Address { get; set; }       // single mapped property -> shared columns
}

public sealed class ThingA : Thing
{
    [NotMapped]
    public new required Address Address { get => base.Address!; set => base.Address = value; }
}

public sealed class ThingB : Thing
{
    [NotMapped]
    public new required Address Address { get => base.Address!; set => base.Address = value; }
}
```

This produces the desired two shared columns:

```sql
CREATE TABLE "Thing" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Thing" PRIMARY KEY AUTOINCREMENT,
    "Discriminator" TEXT NOT NULL,
    "Address_City" TEXT NULL,
    "Address_Street" TEXT NULL
);
```

A write/read round-trip was verified: `ThingA`/`ThingB` instances persist and re-read their
`Address` values correctly through the shared columns.

### Simpler alternatives

- **Hoist the complex property to the base type** as a plain public property
  (`public Address? Address { get; set; }` on `Thing`, removing it from the subtypes). This also
  yields the two shared columns, but the property becomes optional and is no longer `required`
  per-subtype.
- **Use TPC or TPT mapping** instead of TPH (as the reporter notes), where each type has its own
  table and the duplicate-column problem does not arise.

<details>
<summary>minimal repro</summary>

```csharp
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using var context = new TestContext();
Console.WriteLine(context.Database.GenerateCreateScript());

public class TestContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("DataSource=:memory:");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Thing>()
            .HasDiscriminator()
            .HasValue<ThingA>("A")
            .HasValue<ThingB>("B");
}

[ComplexType]
public record class Address(string Street, string City);

public abstract class Thing
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }
}

public sealed class ThingA : Thing
{
    [Column(nameof(Address))]
    public required Address Address { get; set; }
}

public sealed class ThingB : Thing
{
    [Column(nameof(Address))]
    public required Address Address { get; set; }
}
```

</details>
