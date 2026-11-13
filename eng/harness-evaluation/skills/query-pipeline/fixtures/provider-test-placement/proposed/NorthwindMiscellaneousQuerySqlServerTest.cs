using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindMiscellaneousQuerySqlServerTest : NorthwindMiscellaneousQueryRelationalTestBase<
    NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
{
    public NorthwindMiscellaneousQuerySqlServerTest(
        NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Collection_navigation_equal_to_null_for_subquery(bool async)
    {
        await base.Collection_navigation_equal_to_null_for_subquery(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
""");
    }

    public override async Task Dependent_to_principal_navigation_equal_to_null_for_subquery(bool async)
    {
        await base.Dependent_to_principal_navigation_equal_to_null_for_subquery(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT TOP(1) [c0].[CustomerID]
    FROM [Orders] AS [o]
    LEFT JOIN [Customers] AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]) IS NULL
""");
    }

    public override async Task Inner_parameter_in_nested_lambdas_gets_preserved(bool async)
    {
        await base.Inner_parameter_in_nested_lambdas_gets_preserved(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]) > 0
""");
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Convert_to_nullable_on_nullable_value_is_ignored(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Order>().OrderBy(order => order.OrderID).Select(order => new
            {
                order.OrderID,
                OrderDate = (DateTime?)(order.OrderID % 2 == 0 ? (DateTime?)null : order.OrderDate)!.Value
            }),
            assertOrder: true);

        AssertSql(
            """
SELECT [o].[OrderID], CASE
    WHEN [o].[OrderID] % 2 = 0 THEN NULL
    ELSE [o].[OrderDate]
END AS [OrderDate]
FROM [Orders] AS [o]
ORDER BY [o].[OrderID]
""");
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Raw_scalar_query_composes_filter_and_ordering(bool async)
    {
        using var context = CreateContext();
        var query = context.Database.SqlQueryRaw<int>("SELECT [OrderID] AS [Value] FROM [Orders]")
            .Where(orderId => orderId < 10250)
            .OrderBy(orderId => orderId);
        var orderIds = async ? await query.ToListAsync() : query.ToList();
        Assert.Equal(new[] { 10248, 10249 }, orderIds);

        AssertSql(
            """
SELECT [o].[Value]
FROM (
    SELECT [OrderID] AS [Value] FROM [Orders]
) AS [o]
WHERE [o].[Value] < 10250
ORDER BY [o].[Value]
""");
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_collection_projection_preserves_rows(bool async)
    {
        var factory = await InitializeNonSharedTest<CollectionContext>(seed: context => context.SeedAsync());
        using var context = factory.CreateDbContext();
        ClearLog();
        var query = context.Owners.OrderBy(owner => owner.Id).Select(owner => new
        {
            owner.Id,
            Codes = owner.Parts.OrderBy(part => part.Code).Select(part => part.Code).ToArray()
        });
        var rows = async ? await query.ToListAsync() : query.ToList();
        Assert.Equal(2, rows.Count);
        AssertSql(
            """
SELECT [o].[Id], [p].[Code], [p].[OwnerId], [p].[Id]
FROM [Owners] AS [o]
LEFT JOIN [Part] AS [p] ON [o].[Id] = [p].[OwnerId]
ORDER BY [o].[Id], [p].[Code], [p].[OwnerId], [p].[Id]
""");
    }

    protected class CollectionContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Owner> Owners => Set<Owner>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Owner>().OwnsMany(owner => owner.Parts, parts =>
            {
                parts.WithOwner().HasForeignKey("OwnerId");
                parts.HasKey("OwnerId", nameof(Part.Id));
                parts.Property(part => part.Id).ValueGeneratedNever();
            });

        public async Task SeedAsync()
        {
            AddRange(
                new Owner { Id = 1, Parts = [new Part { Id = 1, Code = "c" }, new Part { Id = 2, Code = "a" }, new Part { Id = 3, Code = "b" }] },
                new Owner { Id = 2 });
            await SaveChangesAsync();
        }

        public class Owner
        {
            public int Id { get; set; }
            public List<Part> Parts { get; set; } = [];
        }

        public class Part
        {
            public int Id { get; set; }
            public string Code { get; set; } = null!;
        }
    }

    public override async Task Navigation_inside_interpolated_string_is_expanded(bool async)
    {
        await base.Navigation_inside_interpolated_string_is_expanded(async);

        AssertSql(
            """
SELECT [c].[City]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}