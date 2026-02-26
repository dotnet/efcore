---
name: testing
description: 'Implementation details for EF Core test infrastructure. Use when changing test fixtures, SQL baseline assertions, test helpers, the test class hierarchy, or when adding new tests.'
user-invokable: false
---

# Testing

EF Core test infrastructure, patterns, and workflows for unit, specification, and functional tests.

## Test Categories

### Unit Tests (`test/EFCore.{Provider}.Tests/`)
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
2. **Provider override**: Override in `EFCore.{Provider}.FunctionalTests` with `AssertSql()`
3. **Unit test**: Add to `EFCore.{Provider}.Tests`
4. Run with `EF_TEST_REWRITE_BASELINES=1` to capture initial baselines

## Common Pitfalls

| Pitfall | Solution |
|---------|----------|
| SQL or Compiled model baseline mismatch | Set `EF_TEST_REWRITE_BASELINES=1` and re-run |
| `Check_all_tests_overridden` fails | Override new base test in provider test class |
