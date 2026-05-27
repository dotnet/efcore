# AI Triage

The below is an AI-generated analysis and may contain inaccuracies.

## What `microsoft.entityframeworkcore.active_dbcontexts` is actually measuring today

The instrument is wired up in
[`EntityFrameworkMetrics`](https://github.com/dotnet/efcore/blob/main/src/EFCore/Infrastructure/Internal/EntityFrameworkMetrics.cs)
as an `ObservableUpDownCounter<int>` over the static counter in
[`EntityFrameworkMetricsData`](https://github.com/dotnet/efcore/blob/main/src/EFCore/Infrastructure/EntityFrameworkMetricsData.cs):

- It is **incremented in the `DbContext` constructor** via `ReportDbContextInitializing()` — see
  [`DbContext.cs#L129`](https://github.com/dotnet/efcore/blob/main/src/EFCore/DbContext.cs#L129).
- It is **decremented only when the underlying `DbContext` instance is actually torn down** (i.e. `DisposeSync`
  reaches the `else if (!_disposed)` branch and calls `ReportDbContextDisposing()`) — see
  [`DbContext.cs#L1119`](https://github.com/dotnet/efcore/blob/main/src/EFCore/DbContext.cs#L1119).

So the counter is really *"number of `DbContext` CLR instances that have been constructed but not yet finalized/disposed"*, which when pooling is involved is **not** the same as *"number of contexts currently leased / in active use"*.

## What happens with `AddDbContextPool` step-by-step

With `AddDbContextPool` the scope gets an
[`ScopedDbContextLease<TContext>`](https://github.com/dotnet/efcore/blob/main/src/EFCore/Internal/ScopedDbContextLease.cs)
constructed with `standalone: false`. When the scope is disposed the lease calls
[`DbContextLease.Release`](https://github.com/dotnet/efcore/blob/main/src/EFCore/Internal/DbContextLease.cs#L93)
→ `pool.Return(context)`.

In [`DbContextPool.Return`](https://github.com/dotnet/efcore/blob/main/src/EFCore/Internal/DbContextPool.cs#L107):

- If `++_count <= _maxSize` → the context is reset and re-enqueued. `DbContext.DisposeSync` is **never reached for that instance**, so `ReportDbContextDisposing` is **not** called and the counter stays the same. The context is now sitting idle in the pool but still counted as "active".
- Else → `PooledReturn` decrements `_count`, calls `ClearLease()` (making `_lease.IsActive == false`) and then `context.Dispose()`. That second `Dispose` does reach the `else if (!_disposed)` branch and **does** decrement `active_dbcontexts`.

So the overflow path actually *is* decremented today. The thing that is *never* decremented while the pool keeps the instance is the steady-state pool occupancy.

## Reproduction of the exact scenario in the report

Using EF Core (main) with `poolSize: 50` and the bug's 75/60 numbers:

```
start                                         active_dbcontexts = 0
after renting 75 (pool=50)                    active_dbcontexts = 75
after returning 60 of 75                      active_dbcontexts = 65
Scopes still undisposed (truly active): 15
```

That 65 breaks down as: **50 idle contexts retained in the pool** + **15 contexts still leased to the application** = 65. The 10 overflow contexts (returns 51..60) *did* get disposed and *did* decrement the counter — i.e. the report's claim that *"those contexts are also not decremented from the metric after disposal, so you get an ever-incrementing metric"* doesn't match what current code does. With repeated rent/return cycles the counter saturates at `poolSize + currently-leased`, it does not grow unboundedly.

<details><summary>minimal repro</summary>

```csharp
using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

int active = 0;
using var listener = new MeterListener();
listener.InstrumentPublished = (inst, l) =>
{
    if (inst.Name == "microsoft.entityframeworkcore.active_dbcontexts")
        l.EnableMeasurementEvents(inst);
};
listener.SetMeasurementEventCallback<int>((inst, m, _, _) => active = m);
listener.Start();

var services = new ServiceCollection();
services.AddDbContextPool<TestContext>(
    o => o.UseSqlite("Data Source=:memory:"),
    poolSize: 50);
var sp = services.BuildServiceProvider();

void Sample(string label)
{
    listener.RecordObservableInstruments();
    Console.WriteLine($"{label,-45} active_dbcontexts = {active}");
}

Sample("start");

var scopes = new List<IServiceScope>();
for (int i = 0; i < 75; i++)
{
    var s = sp.CreateScope();
    _ = s.ServiceProvider.GetRequiredService<TestContext>();
    scopes.Add(s);
}
Sample("after renting 75 (pool=50)");

for (int i = 0; i < 60; i++)
    scopes[i].Dispose();
Sample("after returning 60 of 75");

Console.WriteLine($"Scopes still undisposed (truly active): {scopes.Count - 60}");

public class TestContext(DbContextOptions<TestContext> o) : DbContext(o);
```

</details>

## What the user actually wants vs. what this metric provides

The underlying observation is real and matches the discussion in
[#35855](https://github.com/dotnet/efcore/issues/35855): `active_dbcontexts` is *not* a good signal for *"how many contexts is my application currently holding onto"* when pooling is in use. It conflates:

1. Contexts currently leased to application code (what the user wants).
2. Contexts idle inside the pool waiting to be re-leased.
3. Contexts that overflowed the pool and are still alive somewhere (shouldn't happen long-term — they get disposed on return — but transiently they exist).

To get the picture the user is after we would need additional instruments, for example:

- A gauge for *leased* (rented-out) pool contexts.
- A gauge for *idle* (pooled) contexts (`DbContextPool._count`).
- Optionally a counter for overflow / pool-exhaustion events.

Just renaming/redefining `active_dbcontexts` would be a breaking change for existing consumers, so this likely needs a small set of new instruments rather than changing the meaning of the existing one.

## Classification

- **Type:** bug — the metric's behavior is surprising and not useful for the documented purpose of monitoring pool health. The exact symptom described ("ever-incrementing metric") doesn't match code today, but the deeper complaint ("you can't tell from this metric how many contexts are actually in use") is valid and is the same root cause as the earlier issue.
- **Area labels:** `area-diagnostics` (the metric pipeline). Not provider-specific.
- **Likely duplicate / closely related:** [#35855 — Is AddDbContextPool supposed to release contexts from the pool?](https://github.com/dotnet/efcore/issues/35855) — already labelled `needs-design` / `area-diagnostics`, assigned to @roji, on the Backlog. This new issue is essentially the same underlying problem extended to the pool-overflow case, and would naturally be fixed by the same design work (adding pool-aware instruments).
- **Regression:** no — this is the design of the instrument since it was introduced; it behaves the same on 9.0.x and on `main`.
