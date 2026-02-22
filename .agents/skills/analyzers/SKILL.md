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

See AnalyzerReleases.Shipped.md for a complete list of shipped diagnostics.

## Validation

- Analyzer triggers on expected code patterns in an efficient manner
- No false positives on public API usage
- Code fix produces compilable output
