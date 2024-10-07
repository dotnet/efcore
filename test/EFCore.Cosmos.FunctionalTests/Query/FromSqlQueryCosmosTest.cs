// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

public class FromSqlQueryCosmosTest : QueryTestBase<NorthwindQueryCosmosFixture<NoopModelCustomizer>>
{
    private static readonly string _eol = Environment.NewLine;

    public FromSqlQueryCosmosTest(
        NorthwindQueryCosmosFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected NorthwindContext CreateContext()
        => Fixture.CreateContext();

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public Task FromSqlRaw_queryable_simple(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                using var context = CreateContext();
                var query = context.Set<Customer>().FromSqlRaw(
                    """SELECT * FROM root c WHERE c["$type"] = "Customer" AND c["ContactName"] LIKE '%z%'""");

                var actual = a
                    ? await query.ToArrayAsync()
                    : query.ToArray();

                Assert.Equal(14, actual.Length);
                Assert.Equal(14, context.ChangeTracker.Entries().Count());

                AssertSql(
                    """
SELECT VALUE s
FROM (
    SELECT * FROM root c WHERE c["$type"] = "Customer" AND c["ContactName"] LIKE '%z%'
) s
""");
            });

    [ConditionalFact]
    public async Task FromSqlRaw_queryable_incorrect_discriminator_throws()
    {
        using var context = CreateContext();
        var query = context.Set<Order>().FromSqlRaw(
            """
SELECT * FROM root c WHERE c["$type"] = "OrderDetail"
""");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToArrayAsync());

        Assert.Equal(
            CoreStrings.UnableToDiscriminate(context.Model.FindEntityType(typeof(Order))!.DisplayName(), "OrderDetail"),
            exception.Message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public Task FromSqlRaw_queryable_simple_columns_out_of_order(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                using var context = CreateContext();
                var query = context.Set<Customer>().FromSqlRaw(
                    """
SELECT c["id"], c["$type"], c["Region"], c["PostalCode"], c["Phone"], c["Fax"], c["Country"], c["ContactTitle"], c["ContactName"], c["CompanyName"], c["City"], c["Address"] FROM root c WHERE c["$type"] = "Customer"
""");

                var actual = a
                    ? await query.ToArrayAsync()
                    : query.ToArray();

                Assert.Equal(91, actual.Length);
                Assert.Equal(91, context.ChangeTracker.Entries().Count());

                AssertSql(
                    """
SELECT VALUE s
FROM (
    SELECT c["id"], c["$type"], c["Region"], c["PostalCode"], c["Phone"], c["Fax"], c["Country"], c["ContactTitle"], c["ContactName"], c["CompanyName"], c["City"], c["Address"] FROM root c WHERE c["$type"] = "Customer"
) s
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public Task FromSqlRaw_queryable_simple_columns_out_of_order_and_extra_columns(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                using var context = CreateContext();
                var query = context.Set<Customer>().FromSqlRaw(
                    """
SELECT c["id"], c["$type"], c["Region"], c["PostalCode"], c["PostalCode"] AS Foo, c["Phone"], c["Fax"], c["Country"], c["ContactTitle"], c["ContactName"], c["CompanyName"], c["City"], c["Address"] FROM root c WHERE c["$type"] = "Customer"
""");

                var actual = a
                    ? await query.ToArrayAsync()
                    : query.ToArray();

                Assert.Equal(91, actual.Length);
                Assert.Equal(91, context.ChangeTracker.Entries().Count());

                AssertSql(
                    """
SELECT VALUE s
FROM (
    SELECT c["id"], c["$type"], c["Region"], c["PostalCode"], c["PostalCode"] AS Foo, c["Phone"], c["Fax"], c["Country"], c["ContactTitle"], c["ContactName"], c["CompanyName"], c["City"], c["Address"] FROM root c WHERE c["$type"] = "Customer"
) s
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public Task FromSqlRaw_queryable_composed(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                using var context = CreateContext();
                var query = context.Set<Customer>().FromSqlRaw(
                        """
                    SELECT * FROM root c WHERE c["$type"] = "Customer"
                    """)
                    .Where(c => c.ContactName.Contains("z"));

                var sql = query.ToQueryString();

                var actual = a
                    ? await query.ToArrayAsync()
                    : query.ToArray();

                Assert.Equal(14, actual.Length);
                Assert.Equal(14, context.ChangeTracker.Entries().Count());

                AssertSql(
                    """
SELECT VALUE s
FROM (
    SELECT * FROM root c WHERE c["$type"] = "Customer"
) s
WHERE CONTAINS(s["ContactName"], "z")
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_queryable_composed_after_removing_whitespaces(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                using var context = CreateContext();
                var query = context.Set<Customer>().FromSqlRaw(
                        _eol + "    " + _eol + _eol + _eol + "SELECT" + _eol + @"* FROM root c WHERE c[""$type""] = ""Customer""")
                    .Where(c => c.ContactName.Contains("z"));

                var actual = a
                    ? await query.ToArrayAsync()
                    : query.ToArray();

                Assert.Equal(14, actual.Length);

                AssertSql(
                    """
SELECT VALUE s
FROM (


""" + "        " + """



    SELECT
    * FROM root c WHERE c["$type"] = "Customer"
) s
WHERE CONTAINS(s["ContactName"], "z")
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public Task FromSqlRaw_queryable_composed_compiled(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                if (a)
                {
                    var query = EF.CompileAsyncQuery(
                        (NorthwindContext context) => context.Set<Customer>()
                            .FromSqlRaw(
                                """
SELECT * FROM root c WHERE c["$type"] = "Customer"
""")
                            .Where(c => c.ContactName.Contains("z")));

                    using (var context = CreateContext())
                    {
                        var actual = await query(context).ToListAsync();

                        Assert.Equal(14, actual.Count);
                    }
                }
                else
                {
                    var query = EF.CompileQuery(
                        (NorthwindContext context) => context.Set<Customer>()
                            .FromSqlRaw("""SELECT * FROM root c WHERE c["$type"] = "Customer" """)
                            .Where(c => c.ContactName.Contains("z")));

                    using (var context = CreateContext())
                    {
                        var actual = query(context).ToArray();

                        Assert.Equal(14, actual.Length);
                    }
                }

                AssertSql(
                    """
SELECT VALUE s
FROM (
    SELECT * FROM root c WHERE c["$type"] = "Customer"
) s
WHERE CONTAINS(s["ContactName"], "z")
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_queryable_composed_compiled_with_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                if (a)
                {
                    var query = EF.CompileAsyncQuery(
                        (NorthwindContext context) => context.Set<Customer>().FromSqlRaw(
                                """SELECT * FROM root c WHERE c["$type"] = "Customer" AND c["id"] = {0}""", "CONSH")
                            .Where(c => c.ContactName.Contains("z")));

                    using (var context = CreateContext())
                    {
                        var actual = await query(context).ToListAsync();

                        Assert.Single(actual);
                    }
                }
                else
                {
                    var query = EF.CompileQuery(
                        (NorthwindContext context) => context.Set<Customer>().FromSqlRaw(
                                """SELECT * FROM root c WHERE c["$type"] = "Customer" AND c["id"] = {0}""", "CONSH")
                            .Where(c => c.ContactName.Contains("z")));

                    using (var context = CreateContext())
                    {
                        var actual = query(context).ToArray();

                        Assert.Single(actual);
                    }
                }

                AssertSql(
                    """
SELECT VALUE s
FROM (
    SELECT * FROM root c WHERE c["$type"] = "Customer" AND c["id"] = "CONSH"
) s
WHERE CONTAINS(s["ContactName"], "z")
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_queryable_multiple_line_query(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                using var context = CreateContext();
                var query = context.Set<Customer>().FromSqlRaw(
                    """
SELECT *
FROM root c
WHERE c["$type"] = "Customer" AND c["City"] = 'London'
""");

                var actual = a
                    ? await query.ToArrayAsync()
                    : query.ToArray();

                Assert.Equal(6, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));

                AssertSql(
                    """
SELECT VALUE s
FROM (
    SELECT *
    FROM root c
    WHERE c["$type"] = "Customer" AND c["City"] = 'London'
) s
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_queryable_composed_multiple_line_query(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                using var context = CreateContext();
                var query = context.Set<Customer>().FromSqlRaw(
                        """
SELECT *
FROM root c
WHERE c["$type"] = "Customer"
""")
                    .Where(c => c.City == "London");

                var actual = a
                    ? await query.ToArrayAsync()
                    : query.ToArray();

                Assert.Equal(6, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));

                AssertSql(
                    """
SELECT VALUE s
FROM (
    SELECT *
    FROM root c
    WHERE c["$type"] = "Customer"
) s
WHERE (s["City"] = "London")
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public Task FromSqlRaw_queryable_with_parameters(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                var city = "London";
                var contactTitle = "Sales Representative";

                using var context = CreateContext();
                var query = context.Set<Customer>().FromSqlRaw(
                    """SELECT * FROM root c WHERE c["$type"] = "Customer" AND c["City"] = {0} AND c["ContactTitle"] = {1}""",
                    city,
                    contactTitle);

                var actual = a
                    ? await query.ToArrayAsync()
                    : query.ToArray();

                Assert.Equal(3, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
                Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));

                AssertSql(
                    """
@p0='London'
@p1='Sales Representative'

SELECT VALUE s
FROM (
    SELECT * FROM root c WHERE c["$type"] = "Customer" AND c["City"] = @p0 AND c["ContactTitle"] = @p1
) s
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public Task FromSqlRaw_queryable_with_parameters_inline(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                using var context = CreateContext();
                var query = context.Set<Customer>().FromSqlRaw(
                    """SELECT * FROM root c WHERE c["$type"] = "Customer" AND c["City"] = {0} AND c["ContactTitle"] = {1}""",
                    "London",
                    "Sales Representative");

                var actual = a
                    ? await query.ToArrayAsync()
                    : query.ToArray();

                Assert.Equal(3, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
                Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));

                AssertSql(
                    """
@p0='London'
@p1='Sales Representative'

SELECT VALUE s
FROM (
    SELECT * FROM root c WHERE c["$type"] = "Customer" AND c["City"] = @p0 AND c["ContactTitle"] = @p1
) s
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public Task FromSqlRaw_queryable_with_null_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                uint? reportsTo = null;

                using var context = CreateContext();
                var query = context.Set<Employee>().FromSqlRaw(
                    """SELECT * FROM root c WHERE c["$type"] = "Employee" AND c["ReportsTo"] = {0} OR (IS_NULL(c["ReportsTo"]) AND IS_NULL({0}))""",
                    reportsTo);

                var actual = a
                    ? await query.ToArrayAsync()
                    : query.ToArray();

                Assert.Single(actual);

                AssertSql(
                    """
@p0=null

SELECT VALUE s
FROM (
    SELECT * FROM root c WHERE c["$type"] = "Employee" AND c["ReportsTo"] = @p0 OR (IS_NULL(c["ReportsTo"]) AND IS_NULL(@p0))
) s
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public Task FromSqlRaw_queryable_with_parameters_and_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                var city = "London";
                var contactTitle = "Sales Representative";

                using var context = CreateContext();
                var query = context.Set<Customer>().FromSqlRaw(
                        """SELECT * FROM root c WHERE c["$type"] = "Customer" AND c["City"] = {0}""", city)
                    .Where(c => c.ContactTitle == contactTitle);
                var queryString = query.ToQueryString();

                var actual = a
                    ? await query.ToArrayAsync()
                    : query.ToArray();

                Assert.Equal(3, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
                Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));

                AssertSql(
                    """
@p0='London'
@__contactTitle_1='Sales Representative'

SELECT VALUE s
FROM (
    SELECT * FROM root c WHERE c["$type"] = "Customer" AND c["City"] = @p0
) s
WHERE (s["ContactTitle"] = @__contactTitle_1)
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_queryable_simple_cache_key_includes_query_string(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                using var context = CreateContext();
                var query = context.Set<Customer>()
                    .FromSqlRaw("""SELECT * FROM root c WHERE c["$type"] = "Customer" AND c["City"] = 'London'""");

                var actual = a
                    ? await query.ToArrayAsync()
                    : query.ToArray();

                Assert.Equal(6, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));

                query = context.Set<Customer>()
                    .FromSqlRaw("""SELECT * FROM root c WHERE c["$type"] = "Customer" AND c["City"] = 'Seattle'""");

                actual = a
                    ? await query.ToArrayAsync()
                    : query.ToArray();

                Assert.Single(actual);
                Assert.True(actual.All(c => c.City == "Seattle"));

                AssertSql(
                    """
SELECT VALUE s
FROM (
    SELECT * FROM root c WHERE c["$type"] = "Customer" AND c["City"] = 'London'
) s
""",
                    //
                    """
SELECT VALUE s
FROM (
    SELECT * FROM root c WHERE c["$type"] = "Customer" AND c["City"] = 'Seattle'
) s
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_queryable_with_parameters_cache_key_includes_parameters(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                var city = "London";
                var contactTitle = "Sales Representative";
                var sql =
                    """SELECT * FROM root c WHERE c["$type"] = "Customer" AND c["City"] = {0} AND c["ContactTitle"] = {1}""";

                using var context = CreateContext();
                var query = context.Set<Customer>().FromSqlRaw(sql, city, contactTitle);

                var actual = a
                    ? await query.ToArrayAsync()
                    : query.ToArray();

                Assert.Equal(3, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
                Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));

                city = "Madrid";
                contactTitle = "Accounting Manager";

                query = context.Set<Customer>().FromSqlRaw(sql, city, contactTitle);

                actual = a
                    ? await query.ToArrayAsync()
                    : query.ToArray();

                Assert.Equal(2, actual.Length);
                Assert.True(actual.All(c => c.City == "Madrid"));
                Assert.True(actual.All(c => c.ContactTitle == "Accounting Manager"));

                AssertSql(
                    """
@p0='London'
@p1='Sales Representative'

SELECT VALUE s
FROM (
    SELECT * FROM root c WHERE c["$type"] = "Customer" AND c["City"] = @p0 AND c["ContactTitle"] = @p1
) s
""",
                    //
                    """
@p0='Madrid'
@p1='Accounting Manager'

SELECT VALUE s
FROM (
    SELECT * FROM root c WHERE c["$type"] = "Customer" AND c["City"] = @p0 AND c["ContactTitle"] = @p1
) s
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_queryable_simple_as_no_tracking_not_composed(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                using var context = CreateContext();
                var query = context.Set<Customer>().FromSqlRaw(
                        """
SELECT * FROM root c WHERE c["$type"] = "Customer"
""")
                    .AsNoTracking();

                var actual = a
                    ? await query.ToArrayAsync()
                    : query.ToArray();

                Assert.Equal(91, actual.Length);
                Assert.Empty(context.ChangeTracker.Entries());

                AssertSql(
                    """
SELECT VALUE s
FROM (
    SELECT * FROM root c WHERE c["$type"] = "Customer"
) s
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_queryable_simple_projection_composed(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                using var context = CreateContext();
                var query = context.Set<Product>().FromSqlRaw(
                        """
SELECT *
FROM root c
WHERE c["$type"] = "Product" AND NOT c["Discontinued"] AND ((c["UnitsInStock"] + c["UnitsOnOrder"]) < c["ReorderLevel"])
""")
                    .Select(p => p.ProductName);

                var actual = a
                    ? await query.ToArrayAsync()
                    : query.ToArray();

                Assert.Equal(2, actual.Length);

                AssertSql(
                    """
SELECT VALUE s["ProductName"]
FROM (
    SELECT *
    FROM root c
    WHERE c["$type"] = "Product" AND NOT c["Discontinued"] AND ((c["UnitsInStock"] + c["UnitsOnOrder"]) < c["ReorderLevel"])
) s
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_composed_with_nullable_predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                using var context = CreateContext();
                var query = context.Set<Customer>().FromSqlRaw(
                        """
SELECT * FROM root c WHERE c["$type"] = "Customer"
""")
                    .Where(c => c.ContactName == c.CompanyName);

                var actual = a
                    ? await query.ToArrayAsync()
                    : query.ToArray();

                Assert.Empty(actual);

                AssertSql(
                    """
SELECT VALUE s
FROM (
    SELECT * FROM root c WHERE c["$type"] = "Customer"
) s
WHERE (s["ContactName"] = s["CompanyName"])
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_does_not_parameterize_interpolated_string(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                using var context = CreateContext();
                var propertyName = "OrderID";
                var max = 10250;
                var query = context.Orders.FromSqlRaw(
                    $$"""SELECT * FROM root c WHERE c["$type"] = "Order" AND c["{{propertyName}}"] < {0}""", max);

                var actual = a
                    ? await query.ToListAsync()
                    : query.ToList();

                Assert.Equal(2, actual.Count);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_queryable_simple_projection_not_composed(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                using var context = CreateContext();
                var query = context.Set<Customer>().FromSqlRaw(
                        """
SELECT * FROM root c WHERE c["$type"] = "Customer"
""")
                    .Select(
                        c => new { c.CustomerID, c.City })
                    .AsNoTracking();

                var actual = a
                    ? await query.ToArrayAsync()
                    : query.ToArray();

                Assert.Equal(91, actual.Length);
                Assert.Empty(context.ChangeTracker.Entries());

                AssertSql(
                    """
SELECT VALUE
{
    "CustomerID" : s["id"],
    "City" : s["City"]
}
FROM (
    SELECT * FROM root c WHERE c["$type"] = "Customer"
) s
""");
            });

    [ConditionalFact]
    public async Task FromSqlRaw_queryable_simple_with_missing_key_and_non_tracking_throws()
    {
        using var context = CreateContext();
        var query = context.Set<Order>()
            .FromSqlRaw("""SELECT * FROM root c WHERE c["$type"] = "Product" """)
            .AsNoTracking();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToArrayAsync());

        Assert.Equal(
            CoreStrings.InvalidKeyValue(
                context.Model.FindEntityType(typeof(Order))!.DisplayName(),
                "OrderID"),
            exception.Message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSql_queryable_with_parameters_interpolated(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                var city = "London";
                var contactTitle = "Sales Representative";

                await AssertQuery(
                    a,
                    ss => ((DbSet<Customer>)ss.Set<Customer>()).FromSql(
                        $"""SELECT * FROM root c WHERE c["City"] = {city} AND c["ContactTitle"] = {contactTitle}"""),
                    ss => ss.Set<Customer>().Where(x => x.City == city && x.ContactTitle == contactTitle));

                AssertSql(
                    """
@p0='London'
@p1='Sales Representative'

SELECT VALUE s
FROM (
    SELECT * FROM root c WHERE c["City"] = @p0 AND c["ContactTitle"] = @p1
) s
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSql_queryable_with_parameters_inline_interpolated(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    a,
                    ss => ((DbSet<Customer>)ss.Set<Customer>()).FromSql(
                        $"""SELECT * FROM root c WHERE c["City"] = {"London"} AND c["ContactTitle"] = {"Sales Representative"}"""),
                    ss => ss.Set<Customer>().Where(x => x.City == "London" && x.ContactTitle == "Sales Representative"));

                AssertSql(
                    """
@p0='London'
@p1='Sales Representative'

SELECT VALUE s
FROM (
    SELECT * FROM root c WHERE c["City"] = @p0 AND c["ContactTitle"] = @p1
) s
""");
            });

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
