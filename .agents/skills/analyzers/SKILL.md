---
name: analyzers
description: 'EF Core Roslyn analyzers, diagnostic analyzers, code fix providers, diagnostic suppressors. Use when working on EF1001, EF1002, InternalUsageDiagnosticAnalyzer, or StringsUsageInRawQueriesDiagnosticAnalyzer.'
user-invokable: false
---

# EF Core Analyzers

Roslyn analyzers shipped in `Microsoft.EntityFrameworkCore.Analyzers` (`src/EFCore.Analyzers/`).

## When to Use

- Adding a new diagnostic rule or code fix
- Modifying detection logic for internal API usage or SQL injection warnings
- Working on a diagnostic suppressor

## Analyzers

| ID | Analyzer | Category | Purpose |
|----|----------|----------|---------|
| EF1001 | `InternalUsageDiagnosticAnalyzer` (333 lines) | Usage | Warns on `.Internal` namespace / `[EntityFrameworkInternal]` usage. Registers operation + symbol actions for field, property, method, event, invocation, object creation, variable declaration, typeof, named type. |
| EF1002 | `StringsUsageInRawQueriesDiagnosticAnalyzer` (254 lines) | Security | Two diagnostics: interpolated string usage and string concatenation in raw SQL methods. Registers `OperationKind.Invocation` action. |
| — | `InterpolatedStringUsageInRawQueriesCodeFixProvider` | — | Fix for EF1002: `FromSqlRaw` → `FromSqlInterpolated` |
| — | `UninitializedDbSetDiagnosticSuppressor` | — | Suppresses CS8618 for `DbSet<T>` properties on `DbContext` |

## Testing

Tests in `test/EFCore.Analyzers.Tests/` use `CSharpAnalyzerVerifier<TAnalyzer>`. Test methods provide inline C# source with diagnostic location markers. Pattern:

## Validation

- Analyzer triggers on expected code patterns
- No false positives on public API usage
- Code fix produces compilable output
