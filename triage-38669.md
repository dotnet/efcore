# AI Triage

The below is an AI-generated analysis and may contain inaccuracies.

**Issue:** [#38669 — Add `AddDbContextFactory` overload with explicit `DbContextOptions` lifetime](https://github.com/dotnet/efcore/issues/38669)

## Classification

| Field | Value |
| --- | --- |
| Type | Feature request (correctly set) |
| Area | `area-dbcontext` |
| Security | None |

## Summary of the request

The reporter asks for an `AddDbContextFactory` overload that takes **two** lifetimes — a `factoryLifetime` and a separate `optionsLifetime` — mirroring the existing `AddDbContext(..., contextLifetime, optionsLifetime)`. Their goal is to keep the factory a `Singleton` while making the `DbContextOptions` (and therefore the services resolved inside `optionsAction`) resolve with a shorter lifetime, so that a per-connection service (e.g. a transient `NpgsqlConnection`) is not shared across all `DbContext` instances created by the factory.

## Evaluation — does the feature make sense?

**The proposed overload would not achieve its stated goal, and its own default combination is an invalid DI graph.** Details below.

### 1. `optionsAction` runs once per options object, not once per `CreateDbContext()`

`DbContextFactory<TContext>` captures a single `DbContextOptions<TContext>` instance in its constructor and reuses it for every `CreateDbContext()` call:

```csharp
// src/EFCore/Internal/DbContextFactory.cs
private readonly DbContextOptions<TContext> _options;

public DbContextFactory(IServiceProvider serviceProvider, DbContextOptions<TContext> options, ...)
{
    _options = options; // resolved once
}

public virtual TContext CreateDbContext() => _factory(_serviceProvider, _options); // same _options every time
```

`optionsAction` executes inside `CreateDbContextOptions` when the `DbContextOptions<TContext>` object is built — **once per options instance**, not once per created context. Because the factory holds exactly one options instance, the `NpgsqlConnection` resolved inside `optionsAction` is resolved once and embedded in the shared options, so every context gets the same connection. This is by design: `DbContextOptions` is intended to be an immutable, shareable object.

Empirically confirmed against `Microsoft.EntityFrameworkCore.Sqlite` (see repro below): with **both** `Singleton` and `Scoped` lifetimes, `optionsAction` ran exactly once and both contexts shared the same connection.

### 2. Changing only the options lifetime cannot make the connection per-context

Because the singleton factory captures one options instance:

- **Singleton factory + Transient options** → the factory resolves the transient options exactly once at construction → connection is still shared. No improvement.
- **Singleton factory + Scoped options** → **invalid**: a singleton cannot consume a scoped service. With scope validation enabled (the ASP.NET Core default in Development), this throws
  `Cannot consume scoped service 'DbContextOptions<T>' from singleton 'IDbContextFactory<T>'` — exactly the failure reported in [#27326](https://github.com/dotnet/efcore/issues/27326). Note the request's proposed signature even defaults `optionsLifetime` to `Singleton`, so the default call is identical to today's behavior; only the "problematic" combinations are newly reachable, and they either break at startup or silently keep sharing the connection.

The maintainers deliberately keep the factory's options **Singleton** for precisely this reason (design notes in [PR #25440](https://github.com/dotnet/efcore/pull/25440): *"DbContextOptions ... cannot be consumed from the default singleton factory. Changing the lifetime of DbContextOptions is a breaking change."*).

### 3. The real problem is an anti-pattern, and a supported fix already exists

The underlying issue is injecting a **stateful, non-thread-safe `DbConnection` instance** into shared `DbContextOptions`. The recommended fix requires no new API: pass a **connection string** to `UseNpgsql(...)` instead of a `DbConnection` instance. EF Core then creates and manages a separate physical connection per `DbContext`, eliminating the concurrency problem — and it works with the default singleton factory.

If distinct **options/connection instances per created context** are genuinely required, that is a different capability (building options per `CreateDbContext()` call) tracked by [#24010](https://github.com/dotnet/efcore/issues/24010) and [#34156](https://github.com/dotnet/efcore/issues/34156), or achievable with a custom `IDbContextFactory<TContext>` implementation. Adding a second lifetime parameter does not deliver this.

## Recommendation

Recommend **closing as by-design / won't-fix**. The requested overload does not solve the described concurrency problem, its default-adjacent combinations are either invalid DI graphs (scoped-from-singleton, [#27326](https://github.com/dotnet/efcore/issues/27326)) or still share the connection, and the underlying goal is served by passing a connection string. If the reporter needs per-context options, redirect to [#24010](https://github.com/dotnet/efcore/issues/24010) / [#34156](https://github.com/dotnet/efcore/issues/34156).

## Possible duplicates / related issues

- [#8797](https://github.com/dotnet/efcore/issues/8797) — *AddDbContext with ServiceLifetime.Scoped should pass scoped IServiceProvider to optionsAction* (closed). Same underlying desire: control options lifetime to share a per-scope connection across contexts.
- [#6863](https://github.com/dotnet/efcore/issues/6863) — *AddDbContext: DbContextOptions should have the same ServiceLifetime as TContext* (closed).
- [#27326](https://github.com/dotnet/efcore/issues/27326) — the exact *"Cannot consume scoped service from singleton"* error the proposal would trigger.
- [#25164](https://github.com/dotnet/efcore/issues/25164) / [PR #25440](https://github.com/dotnet/efcore/pull/25440) — design rationale for keeping the factory's options Singleton.
- [#24010](https://github.com/dotnet/efcore/issues/24010) / [#34156](https://github.com/dotnet/efcore/issues/34156) — the more appropriate feature (supply/vary options per `CreateDbContext()` call) for the reporter's real need.

<details>
<summary>empirical demonstration</summary>

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

void Run(string title, ServiceLifetime lifetime)
{
    Console.WriteLine($"\n=== {title} (lifetime={lifetime}) ===");
    var counter = 0;
    var services = new ServiceCollection();
    services.AddTransient(_ => new Conn { Id = ++counter });
    services.AddDbContextFactory<AppCtx>((sp, opts) =>
    {
        var c = sp.GetRequiredService<Conn>();
        Console.WriteLine($"  optionsAction ran -> resolved Conn#{c.Id}");
        opts.UseSqlite($"DataSource=file{c.Id}.db");
    }, lifetime);

    using var sp = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    using var scope = sp.CreateScope();
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppCtx>>();
    using var c1 = factory.CreateDbContext();
    using var c2 = factory.CreateDbContext();
    var db1 = c1.Database.GetDbConnection().DataSource;
    var db2 = c2.Database.GetDbConnection().DataSource;
    Console.WriteLine($"  ctx1 => {db1}, ctx2 => {db2}  {(db1 == db2 ? "*** SHARED ***" : "distinct")}");
}

Run("Singleton factory (default)", ServiceLifetime.Singleton);
Run("Scoped factory", ServiceLifetime.Scoped);

public class Conn { public int Id; }
public class AppCtx(DbContextOptions<AppCtx> options) : DbContext(options) { }
```

Output:

```
=== Singleton factory (default) (lifetime=Singleton) ===
  optionsAction ran -> resolved Conn#1
  ctx1 => file1.db, ctx2 => file1.db  *** SHARED ***

=== Scoped factory (lifetime=Scoped) ===
  optionsAction ran -> resolved Conn#1
  ctx1 => file1.db, ctx2 => file1.db  *** SHARED ***
```

`optionsAction` runs once and the connection is shared across contexts regardless of lifetime — confirming that a separate `optionsLifetime` parameter would not make the connection per-context.

</details>
