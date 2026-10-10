# Source Context

The staged `before/test/` and `proposed/test/` directories contain the original and proposed reduced test-class snapshots, respectively. Selected declarations and helper excerpts follow. Unrelated members and dependencies are omitted; these files are not a standalone build.

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

## test/EFCore.Sqlite.FunctionalTests/Query/NorthwindMiscellaneousQuerySqliteTest.cs

```csharp
public class NorthwindMiscellaneousQuerySqliteTest : NorthwindMiscellaneousQueryRelationalTestBase<
    NorthwindQuerySqliteFixture<NoopModelCustomizer>>
{
    public NorthwindMiscellaneousQuerySqliteTest(
        NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }
```
