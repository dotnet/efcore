# AI Triage

The below is an AI-generated analysis and may contain inaccuracies.

**Issue:** [#38483 — EF Core 10 parameter-name simplification (`@__p_0` → `@p`) and SQL Server plan-cache recompilation on upgrade](https://github.com/dotnet/efcore/issues/38483)

## Classification

- **Type:** Question / discussion (not a bug report or feature request).
- **Suggested area labels:** `area-query`, `area-sqlserver`.
- No security aspect identified.
- No minimal repro required (this is a question about expected behavior, not a reported defect).

## Summary of the question

In EF Core 10, generated SQL parameter names were simplified — the `__`/numeric decoration was dropped (e.g. `@__city_0` → `@city`), see [#35200](https://github.com/dotnet/efcore/pull/35200). Because SQL Server's `sp_executesql` plan cache is keyed on the exact statement text (including parameter names), the reporter is concerned that an EF Core 9 → 10 upgrade changes the statement text of essentially every parameterized query, causing a one-time cold-cache recompile of each query and a potential compilation spike on high-throughput systems.

## Analysis of the specific questions

### 1. Does the parameter-name change cause a one-time recompile of every parameterized query on upgrade?

Largely yes. The generated SQL text changed for parameterized queries, and SQL Server's plan cache for `sp_executesql` is keyed on the full statement text, so a different parameter name produces a different cache key and therefore a new cache entry (a cold-cache miss) on first execution after the upgrade. This is confirmed by the parameter-naming code paths in the repo, e.g. `src/EFCore.Relational/Query/Internal/RelationalParameterProcessor.cs` and `src/EFCore.Relational/Storage/ParameterNameGenerator.cs` (the fallback generator now emits `p0`, `p1`, … and named parameters now derive from the source name such as `@city`).

Caveat: the impact is "one-time per distinct statement text", not literally "every query type". Queries whose generated text did not change (e.g. queries with no parameters) are unaffected, and the old EF 9 cache entries simply age out of the plan cache normally rather than being actively invalidated.

### 2. Is the plan-cache churn pure (same plan, new entry) rather than a plan-quality change?

The reporter's expectation is correct: the parameter *name* is not an input to the optimizer's plan selection, so the recompiled plan is expected to be functionally identical (matching `query_plan_hash`), assuming the rest of the statement text and parameter *types* are unchanged. This is plan-cache churn (new entry, equivalent plan), not a plan-regression. Confirming `query_plan_hash` equality before/after on the workload would be the way to validate this empirically.

Note: keep this distinct from the parameterized-collection translation change (`UseParameterizedCollectionMode` / OPENJSON vs. constants), which *does* change statement text/shape and *can* affect plans. The reporter has already isolated that variable, so the residual difference is the parameter-name change alone.

### 3. Was the plan-cache impact considered, and why wasn't it a documented breaking change?

This is best answered by the EF team. Observationally: the change was treated as a SQL-text/readability improvement rather than a behavioral breaking change, since it does not change query results or plan quality — only the cache key for the first execution after upgrade. The "compilation spike on cold cache after a deploy" concern is a reasonable thing to call out, and the team may want to confirm whether it warrants a note in the EF Core 10 breaking-changes / "what's new" documentation.

### 4. Is there a supported way to retain the previous parameter naming?

Based on the current source, the new naming appears to be **unconditional** — there is no `AppContext` switch or configuration option that reverts to the EF 9 `@__name_0` scheme. (For comparison, other EF 10 behavior changes that *do* ship opt-outs use `AppContext` switches such as `Microsoft.EntityFrameworkCore.Issue31751`, `Microsoft.Data.Sqlite.Pre10TimeZoneHandling`, and `Microsoft.EntityFrameworkCore.EscapeIllegalCosmosIdCharacters`; no equivalent switch exists for parameter naming.) If a back-compat switch is desired, that would be a new feature request for the EF team to consider.

### 5. Recommended mitigation for cache-sensitive deployments

General guidance (not EF-specific):

- Treat the upgrade like any deploy that resets the relevant plan-cache entries: prefer a **staged / canary rollout** so recompiles are spread over time rather than hitting every node simultaneously.
- **Pre-warm** hot queries against the new build before directing full production traffic.
- For chronic compile-pressure concerns, SQL Server features such as **Optimize for Ad hoc Workloads** and **`sp_executesql` plan reuse** already apply; for the most expensive plans, **forced plans via Query Store** can pin a known-good plan across the text change (though the forced plan is also keyed to statement text, so it must be re-applied for the new text).
- Monitor `RESOURCE_SEMAPHORE_QUERY_COMPILE` waits and `sys.dm_exec_query_stats` compile counts around the rollout to quantify the actual spike.

## Possible related issues / references

- [#35200](https://github.com/dotnet/efcore/pull/35200) — the PR that simplified the parameter names (root cause referenced by the reporter).
- The parameterized-collection translation work (`UseParameterizedCollectionMode` / `ParameterTranslationMode`) is a separate EF 10 change the reporter has already accounted for.

## Suggested next step for maintainers

Confirm whether the team wants to (a) document the one-time plan-cache churn as an upgrade consideration in the EF Core 10 docs, and/or (b) consider an opt-out switch for the legacy parameter naming. Otherwise this can likely be answered and closed as a question.
