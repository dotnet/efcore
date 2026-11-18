# Source Context

The staged `before/test/` and `proposed/test/` directories contain the original and proposed reduced test-class snapshots, respectively. The proposal adds three regressions to the SQL Server class. Selected declarations and helper excerpts follow. Unrelated members and dependencies are omitted; these files are not a standalone build.

## test/EFCore.Specification.Tests/NonSharedModelTestBase.cs

```csharp
public abstract class NonSharedModelTestBase(NonSharedFixture fixture) : IAsyncLifetime
{
    public static readonly IEnumerable<object[]> IsAsyncData = [[false], [true]];
```

## test/EFCore.Specification.Tests/Query/QueryTestBase.cs

```csharp
public abstract class QueryTestBase<TFixture> : NonSharedModelTestBase, IClassFixture<TFixture>
    where TFixture : class, IQueryFixtureBase, new()
```

```csharp
    protected TFixture Fixture { get; }
    protected NorthwindContext CreateContext() => Fixture.CreateContext();
```

```csharp
    public Task AssertQuery<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Func<TResult, object?>? elementSorter = null,
        Action<TResult, TResult>? elementAsserter = null,
        bool assertOrder = false,
        bool assertEmpty = false,
        QueryTrackingBehavior? queryTrackingBehavior = null,
        [CallerMemberName] string testMethodName = "")
        => AssertQuery(
            async, query, query, elementSorter, elementAsserter, assertOrder, assertEmpty, queryTrackingBehavior, testMethodName);

    public Task AssertQuery<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Func<TResult, object?>? elementSorter = null,
        Action<TResult, TResult>? elementAsserter = null,
        bool assertOrder = false,
        bool assertEmpty = false,
        QueryTrackingBehavior? queryTrackingBehavior = null,
        [CallerMemberName] string testMethodName = "")
        => TestOutputWrapper(() => QueryAsserter.AssertQuery(
            actualQuery, expectedQuery, elementSorter!, elementAsserter, assertOrder, assertEmpty, async,
            queryTrackingBehavior,
            testMethodName));
```

## test/EFCore.Specification.Tests/TestModels/Northwind/Order.cs

```csharp
public class Order
{
    private int? _orderId;

    public int OrderID
    {
        get => _orderId ?? 0;
        set => _orderId = value;
    }

    [MaxLength(5)]
    public string? CustomerID { get; set; }

    public uint? EmployeeID { get; set; }
    public DateTime? OrderDate { get; set; }
```

The expected Northwind data starts with OrderID 10248 and 10249; OrderID 10249 has
OrderDate 1996-07-05. `AssertQuery` compares ordered results when `assertOrder`
is true and compares anonymous projection members by value. The actual and
expected query overload permits different expressions over the same fixture data.
The staged raw SQL text is accepted by both SQL Server and SQLite.

## test/Directory.Build.props

Tests use Microsoft.Testing.Platform with xUnit. A focused command uses
`dotnet test <project> -- --filter-method "*Class.Method"`. Test assemblies must
be rebuilt after edits before a command uses `--no-build`.

## test/EFCore.Specification.Tests/TestUtilities/TestHelpers.cs

```csharp
    public static void AssertAllMethodsOverridden(Type testClass)
    {
        var methods = testClass
            .GetRuntimeMethods()
            .Where(m => m.DeclaringType != testClass
                && (Attribute.IsDefined(m, typeof(FactAttribute))
                    || Attribute.IsDefined(m, typeof(TheoryAttribute))))
            .ToList();
```

## test/EFCore.InMemory.FunctionalTests/Query/NorthwindMiscellaneousQueryInMemoryTest.cs

```csharp
public class NorthwindMiscellaneousQueryInMemoryTest(NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture)
    : NorthwindMiscellaneousQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>(fixture)
```

## test/EFCore.InMemory.FunctionalTests/Query/OwnedEntityQueryInMemoryTest.cs

```csharp
public class OwnedEntityQueryInMemoryTest(NonSharedFixture fixture)
    : OwnedEntityQueryTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => InMemoryTestStoreFactory.Instance;
}
```

## src/EFCore.Relational/Extensions/RelationalDatabaseFacadeExtensions.cs

```csharp
public static IQueryable<TResult> SqlQueryRaw<TResult>(
    this DatabaseFacade databaseFacade, string sql, params object[] parameters);
```

The core specifications reference EFCore, not EFCore.Relational. Provider projects
reference their applicable specifications.
