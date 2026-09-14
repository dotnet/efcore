# AI Triage

The below is an AI-generated analysis and may contain inaccuracies.

Issue: [#38977](https://github.com/dotnet/efcore/issues/38977)

## Assessment

- **Security concern:** None identified.
- **Type:** Bug.
- **Suggested labels:** `area-query`, `regression`, `breaking-change`, `needs-design`.
- **Provider labels:** None. The behavior is in the relational `QuerySqlGenerator`; it reproduces with SQLite, and the workaround also generates valid provider-delimited SQL for SQL Server.

The regression is confirmed. In EF Core 10, an unknown `SqlExpression` reached `ExpressionVisitor.VisitExtension`, which called the node's `VisitChildren` implementation. In EF Core 11, [#37533](https://github.com/dotnet/efcore/pull/37533) changed `QuerySqlGenerator.VisitExtension` so its default arm throws `InvalidOperationException`.

This is a real behavior change, although custom SQL-expression types have historically been an advanced, unsupported scenario requiring all relevant query-pipeline services to understand the new node. Restoring the unconditional `VisitChildren` fallback could hide genuinely unhandled nodes, so whether to restore it or add an explicit extension point requires design discussion.

## Reproduction

The issue's original program was run without modification other than selecting the package version:

| Version | Result |
| --- | --- |
| `Microsoft.EntityFrameworkCore.Sqlite` 10.0.5 | Generated the expected `RANK() OVER (ORDER BY ...)` SQL |
| `Microsoft.EntityFrameworkCore.Sqlite` 11.0.0-rc.1.26425.128 | Threw `InvalidOperationException: Unhandled expression '[RankExpression]' of type 'RankExpression' encountered in 'QuerySqlGenerator'.` |

EF Core 10 output:

```sql
SELECT "o"."Id", RANK() OVER (ORDER BY "o"."Price") AS "Rank"
FROM "Orders" AS "o"
```

## Workaround

For this specific `RANK()` scenario, avoid introducing a custom `SqlExpression`. Compose the syntax from built-in `SqlFunctionExpression` nodes instead. The fixed pseudo-function names arrange the required tokens, while the ordering expression remains a real child node visited by the provider. Provider-specific identifier quoting and parameter generation are therefore preserved.

Do not construct either pseudo-function name from user input.

```csharp
#:package Microsoft.EntityFrameworkCore.Sqlite@11.0.0-rc.1.26425.128
#:property PublishAot=false

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

await using var db = new AppDb();
await db.Database.OpenConnectionAsync();
await db.Database.EnsureCreatedAsync();

db.Orders.AddRange(
    new Order { Price = 30 },
    new Order { Price = 10 },
    new Order { Price = 10 });
await db.SaveChangesAsync();

var adjustment = 2.5;
var query = db.Orders
    .Select(o => new { o.Id, o.Price, Rank = Db.Rank(o.Price + adjustment) })
    .OrderBy(x => x.Id);

Console.WriteLine(query.ToQueryString());

foreach (var row in await query.ToListAsync())
{
    Console.WriteLine($"{row.Id}: {row.Price} => {row.Rank}");
}

public static class Db
{
    public static long Rank(double orderBy) => throw new NotSupportedException();
}

public class Order
{
    public int Id { get; set; }
    public double Price { get; set; }
}

public class AppDb : DbContext
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=:memory:");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder
            .HasDbFunction(typeof(Db).GetMethod(nameof(Db.Rank))!)
            .HasTranslation(
                arguments => new SqlFunctionExpression(
                    "RANK() OVER ",
                    [
                        new SqlFunctionExpression(
                            "ORDER BY",
                            [arguments[0]],
                            nullable: true,
                            argumentsPropagateNullability: [true],
                            arguments[0].Type,
                            arguments[0].TypeMapping)
                    ],
                    nullable: false,
                    argumentsPropagateNullability: [false],
                    typeof(long),
                    typeMapping: null));
}
```

A reusable library can return the same expression tree from its existing method-call translator instead of configuring `HasTranslation`.

## Workaround verification

The complete workaround above was run against SQLite using EF Core 11.0.0-rc.1.26425.128. It generated parameterized SQL and executed successfully:

```text
.param set @adjustment 2.5

SELECT "o"."Id", "o"."Price", RANK() OVER (ORDER BY("o"."Price" + @adjustment)) AS "Rank"
FROM "Orders" AS "o"
ORDER BY "o"."Id"
1: 30 => 3
2: 10 => 1
3: 10 => 1
```

The same translation was also run through SQL Server's EF Core 11 query pipeline with `ToQueryString()`:

```sql
DECLARE @adjustment float = 2.5E0;

SELECT [o].[Id], RANK() OVER (ORDER BY([o].[Price] + @adjustment)) AS [Rank]
FROM [Orders] AS [o]
```

This validates the requested scenario without a command interceptor, a custom nullability processor, or a provider-specific query SQL generator. It is a targeted workaround rather than a general replacement for custom SQL-expression support.

## Related issues

- [#26522](https://github.com/dotnet/efcore/issues/26522), **End-user way to add custom sql expression**, is the closest prior design discussion. It was closed as by-design because custom nodes require support across multiple query-pipeline services.
- [#26147](https://github.com/dotnet/efcore/issues/26147), **IMethodCallTranslatorPlugin vs SqlNullabilityProcessor**, documents the same general limitation at the nullability-processing stage.
- [#16710](https://github.com/dotnet/efcore/issues/16710) and [#18714](https://github.com/dotnet/efcore/issues/18714) concern extensibility of the former `SqlExpressionVisitor`.
- [#37533](https://github.com/dotnet/efcore/pull/37533) introduced the reported EF Core 11 behavior change.

No exact duplicate was found. #26522 is closely related, but #38977 specifically reports a newly introduced EF Core 11 regression in SQL generation.
