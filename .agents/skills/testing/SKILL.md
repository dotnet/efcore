---
name: testing
description: 'Implementation details for EF Core test infrastructure. Use when changing test fixtures, SQL baseline assertions, test helpers, the test class hierarchy, or when adding new tests.'
user-invocable: false
---

# Testing

## Test Categories

### Unit Tests (`test/EFCore.Tests/`, `test/EFCore.Relational.Tests/`, `test/EFCore.{Provider}.Tests/`)
Isolated logic tests. Build models via `*TestHelpers.Instance.CreateConventionBuilder()`, resolve services from `CreateContextServices()`. No database needed.

### Specification Tests (provider-agnostic abstract bases)
Define WHAT to test (LINQ queries, expected results). Can't be run directly — provider tests override to verify HOW (generated SQL).

- Core → `test/EFCore.Specification.Tests/`
- Relational → `test/EFCore.Relational.Specification.Tests/`

### Functional Tests (`test/EFCore.{Provider}.FunctionalTests/`)
Concrete provider tests inheriting specification tests. Most include SQL baseline assertions.

## Test Class Hierarchy (Query Example)

```
QueryTestBase<TFixture>                                    # Core
  └─ NorthwindWhereQueryTestBase<TFixture>                 # Specification
      └─ NorthwindWhereQueryRelationalTestBase<TFixture>   # Relational specification
          └─ NorthwindWhereQuerySqlServerTest              # Provider (asserts SQL)
```

Provider override pattern:
```csharp
public override async Task Where_simple(bool async)
{
    await base.Where_simple(async);  // runs LINQ + asserts results
    AssertSql("""...""");            // asserts provider-specific SQL
}
```

## TestHelpers Hierarchy

```
TestHelpers (abstract)                  # EFCore.Specification.Tests
  ├─ InMemoryTestHelpers               # non-relational
  └─ RelationalTestHelpers (abstract)  # EFCore.Relational.Specification.Tests
      ├─ SqlServerTestHelpers
      └─ SqliteTestHelpers
```

Key methods: `CreateConventionBuilder()`, `CreateContextServices(model)`, `CreateOptions()`

## Fixtures

### SharedStoreFixtureBase<TContext>
Many tests share one database. Creates `TestStore` + pooled `DbContextFactory` in `InitializeAsync()`. Seeds data once. Use for read-heavy tests (e.g., Northwind query tests).

### NonSharedModelTestBase
Each test gets a fresh model/store. Call `InitializeAsync<TContext>(onModelCreating, seed, ...)` per test. Use for tests needing unique schemas.

## SQL Baseline Assertions

`TestSqlLoggerFactory` captures SQL. `AssertSql("""...""")` compares against expected. Set `EF_TEST_REWRITE_BASELINES=1` to auto-rewrite baselines via Roslyn.

## Workflow: Adding New Tests

1. **Specification test**: Add to `EFCore.Specification.Tests` (core) or `EFCore.Relational.Specification.Tests` (relational)
2. **Provider overrides**: Override in **every** provider functional test class (`EFCore.{Provider}.FunctionalTests`) that inherits the base with provider-appropriate assertions.
3. **Unit test**: Add to `EFCore.{Provider}.Tests`
4. Run with `EF_TEST_REWRITE_BASELINES=1` to capture initial baselines
5. Run tests with project rebuilding enabled (don't use `--no-build`) to ensure code changes are picked up
6. When testing cross-platform code (e.g., file paths, path separators), verify the fix works on both Windows and Linux/macOS

## Common Pitfalls

| Pitfall | Solution |
|---------|----------|
| Baseline mismatch (SQL or compiled model) | Re-run with `EF_TEST_REWRITE_BASELINES=1` |
| `Check_all_tests_overridden` fails | Override the new test in every inheriting provider class |
| SQL Server feature missing at lower compat level | Gate with `[SqlServerCondition(...)]`|
