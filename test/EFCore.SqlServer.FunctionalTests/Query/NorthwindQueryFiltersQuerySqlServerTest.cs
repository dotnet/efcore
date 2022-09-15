// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindQueryFiltersQuerySqlServerTest : NorthwindQueryFiltersQueryTestBase<
    NorthwindQuerySqlServerFixture<NorthwindQueryFiltersCustomizer>>
{
    public NorthwindQueryFiltersQuerySqlServerTest(
        NorthwindQuerySqlServerFixture<NorthwindQueryFiltersCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
        //fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Count_query(bool async)
    {
        await base.Count_query(async);

        AssertSql(
            @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)
@__ef_filter__TenantPrefix_0_1='B' (Size = 40)

SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE @__ef_filter__TenantPrefix_0 = N'' OR ([c].[CompanyName] IS NOT NULL AND LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0_1)) = @__ef_filter__TenantPrefix_0)");
    }

    public override async Task Materialized_query(bool async)
    {
        await base.Materialized_query(async);

        AssertSql(
            @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)
@__ef_filter__TenantPrefix_0_1='B' (Size = 40)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE @__ef_filter__TenantPrefix_0 = N'' OR ([c].[CompanyName] IS NOT NULL AND LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0_1)) = @__ef_filter__TenantPrefix_0)");
    }

    public override async Task Find(bool async)
    {
        await base.Find(async);

        AssertSql(
            @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)
@__ef_filter__TenantPrefix_0_1='B' (Size = 40)
@__p_0='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (@__ef_filter__TenantPrefix_0 = N'' OR ([c].[CompanyName] IS NOT NULL AND LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0_1)) = @__ef_filter__TenantPrefix_0)) AND [c].[CustomerID] = @__p_0");
    }

    public override async Task Materialized_query_parameter(bool async)
    {
        await base.Materialized_query_parameter(async);

        AssertSql(
            @"@__ef_filter__TenantPrefix_0='F' (Size = 4000)
@__ef_filter__TenantPrefix_0_1='F' (Size = 40)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE @__ef_filter__TenantPrefix_0 = N'' OR ([c].[CompanyName] IS NOT NULL AND LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0_1)) = @__ef_filter__TenantPrefix_0)");
    }

    public override async Task Materialized_query_parameter_new_context(bool async)
    {
        await base.Materialized_query_parameter_new_context(async);

        AssertSql(
            @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)
@__ef_filter__TenantPrefix_0_1='B' (Size = 40)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE @__ef_filter__TenantPrefix_0 = N'' OR ([c].[CompanyName] IS NOT NULL AND LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0_1)) = @__ef_filter__TenantPrefix_0)",
            //
            @"@__ef_filter__TenantPrefix_0='T' (Size = 4000)
@__ef_filter__TenantPrefix_0_1='T' (Size = 40)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE @__ef_filter__TenantPrefix_0 = N'' OR ([c].[CompanyName] IS NOT NULL AND LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0_1)) = @__ef_filter__TenantPrefix_0)");
    }

    public override async Task Projection_query_parameter(bool async)
    {
        await base.Projection_query_parameter(async);

        AssertSql(
            @"@__ef_filter__TenantPrefix_0='F' (Size = 4000)
@__ef_filter__TenantPrefix_0_1='F' (Size = 40)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE @__ef_filter__TenantPrefix_0 = N'' OR ([c].[CompanyName] IS NOT NULL AND LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0_1)) = @__ef_filter__TenantPrefix_0)");
    }

    public override async Task Projection_query(bool async)
    {
        await base.Projection_query(async);

        AssertSql(
            @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)
@__ef_filter__TenantPrefix_0_1='B' (Size = 40)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE @__ef_filter__TenantPrefix_0 = N'' OR ([c].[CompanyName] IS NOT NULL AND LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0_1)) = @__ef_filter__TenantPrefix_0)");
    }

    public override async Task Include_query(bool async)
    {
        await base.Include_query(async);

        AssertSql(
            @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)
@__ef_filter__TenantPrefix_0_1='B' (Size = 40)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate], [t0].[CustomerID0]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [t].[CustomerID] AS [CustomerID0]
    FROM [Orders] AS [o]
    LEFT JOIN (
        SELECT [c0].[CustomerID], [c0].[CompanyName]
        FROM [Customers] AS [c0]
        WHERE @__ef_filter__TenantPrefix_0 = N'' OR ([c0].[CompanyName] IS NOT NULL AND LEFT([c0].[CompanyName], LEN(@__ef_filter__TenantPrefix_0_1)) = @__ef_filter__TenantPrefix_0)
    ) AS [t] ON [o].[CustomerID] = [t].[CustomerID]
    WHERE [t].[CustomerID] IS NOT NULL AND [t].[CompanyName] IS NOT NULL
) AS [t0] ON [c].[CustomerID] = [t0].[CustomerID]
WHERE @__ef_filter__TenantPrefix_0 = N'' OR ([c].[CompanyName] IS NOT NULL AND LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0)
ORDER BY [c].[CustomerID], [t0].[OrderID]");
    }

    public override async Task Include_query_opt_out(bool async)
    {
        await base.Include_query_opt_out(async);

        AssertSql(
            @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID]");
    }

    public override async Task Included_many_to_one_query(bool async)
    {
        await base.Included_many_to_one_query(async);

        AssertSql(
            @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)
@__ef_filter__TenantPrefix_0_1='B' (Size = 40)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM [Orders] AS [o]
LEFT JOIN (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE @__ef_filter__TenantPrefix_0 = N'' OR ([c].[CompanyName] IS NOT NULL AND LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0_1)) = @__ef_filter__TenantPrefix_0)
) AS [t] ON [o].[CustomerID] = [t].[CustomerID]
WHERE [t].[CustomerID] IS NOT NULL AND [t].[CompanyName] IS NOT NULL");
    }

    public override async Task Project_reference_that_itself_has_query_filter_with_another_reference(bool async)
    {
        await base.Project_reference_that_itself_has_query_filter_with_another_reference(async);

        AssertSql(
            @"@__ef_filter__TenantPrefix_1='B' (Size = 4000)
@__ef_filter__TenantPrefix_1_1='B' (Size = 40)
@__ef_filter___quantity_0='50'

SELECT [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate]
FROM [Order Details] AS [o]
INNER JOIN (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    LEFT JOIN (
        SELECT [c].[CustomerID], [c].[CompanyName]
        FROM [Customers] AS [c]
        WHERE @__ef_filter__TenantPrefix_1 = N'' OR ([c].[CompanyName] IS NOT NULL AND LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_1_1)) = @__ef_filter__TenantPrefix_1)
    ) AS [t] ON [o0].[CustomerID] = [t].[CustomerID]
    WHERE [t].[CustomerID] IS NOT NULL AND [t].[CompanyName] IS NOT NULL
) AS [t0] ON [o].[OrderID] = [t0].[OrderID]
WHERE [o].[Quantity] > @__ef_filter___quantity_0");
    }

    public override async Task Navs_query(bool async)
    {
        await base.Navs_query(async);

        AssertSql(
            @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)
@__ef_filter__TenantPrefix_0_1='B' (Size = 40)
@__ef_filter___quantity_1='50'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    LEFT JOIN (
        SELECT [c0].[CustomerID], [c0].[CompanyName]
        FROM [Customers] AS [c0]
        WHERE @__ef_filter__TenantPrefix_0 = N'' OR ([c0].[CompanyName] IS NOT NULL AND LEFT([c0].[CompanyName], LEN(@__ef_filter__TenantPrefix_0_1)) = @__ef_filter__TenantPrefix_0)
    ) AS [t] ON [o].[CustomerID] = [t].[CustomerID]
    WHERE [t].[CustomerID] IS NOT NULL AND [t].[CompanyName] IS NOT NULL
) AS [t0] ON [c].[CustomerID] = [t0].[CustomerID]
INNER JOIN (
    SELECT [o0].[OrderID], [o0].[Discount]
    FROM [Order Details] AS [o0]
    INNER JOIN (
        SELECT [o1].[OrderID]
        FROM [Orders] AS [o1]
        LEFT JOIN (
            SELECT [c1].[CustomerID], [c1].[CompanyName]
            FROM [Customers] AS [c1]
            WHERE @__ef_filter__TenantPrefix_0 = N'' OR ([c1].[CompanyName] IS NOT NULL AND LEFT([c1].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0)
        ) AS [t3] ON [o1].[CustomerID] = [t3].[CustomerID]
        WHERE [t3].[CustomerID] IS NOT NULL AND [t3].[CompanyName] IS NOT NULL
    ) AS [t2] ON [o0].[OrderID] = [t2].[OrderID]
    WHERE [o0].[Quantity] > @__ef_filter___quantity_1
) AS [t1] ON [t0].[OrderID] = [t1].[OrderID]
WHERE (@__ef_filter__TenantPrefix_0 = N'' OR ([c].[CompanyName] IS NOT NULL AND LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0)) AND [t1].[Discount] < CAST(10 AS real)");
    }

    [ConditionalFact]
    public void FromSql_is_composed()
    {
        using (var context = Fixture.CreateContext())
        {
            var results = context.Customers.FromSqlRaw("select * from Customers").ToList();

            Assert.Equal(7, results.Count);
        }

        AssertSql(
            @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)
@__ef_filter__TenantPrefix_0_1='B' (Size = 40)

SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region]
FROM (
    select * from Customers
) AS [m]
WHERE @__ef_filter__TenantPrefix_0 = N'' OR ([m].[CompanyName] IS NOT NULL AND LEFT([m].[CompanyName], LEN(@__ef_filter__TenantPrefix_0_1)) = @__ef_filter__TenantPrefix_0)");
    }

    [ConditionalFact]
    public void FromSql_is_composed_when_filter_has_navigation()
    {
        using (var context = Fixture.CreateContext())
        {
            var results = context.Orders.FromSqlRaw("select * from Orders").ToList();

            Assert.Equal(80, results.Count);
        }

        AssertSql(
            @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)
@__ef_filter__TenantPrefix_0_1='B' (Size = 40)

SELECT [m].[OrderID], [m].[CustomerID], [m].[EmployeeID], [m].[OrderDate]
FROM (
    select * from Orders
) AS [m]
LEFT JOIN (
    SELECT [c].[CustomerID], [c].[CompanyName]
    FROM [Customers] AS [c]
    WHERE @__ef_filter__TenantPrefix_0 = N'' OR ([c].[CompanyName] IS NOT NULL AND LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0_1)) = @__ef_filter__TenantPrefix_0)
) AS [t] ON [m].[CustomerID] = [t].[CustomerID]
WHERE [t].[CustomerID] IS NOT NULL AND [t].[CompanyName] IS NOT NULL");
    }

    public override void Compiled_query()
    {
        base.Compiled_query();

        AssertSql(
            @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)
@__ef_filter__TenantPrefix_0_1='B' (Size = 40)
@__customerID='BERGS' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (@__ef_filter__TenantPrefix_0 = N'' OR ([c].[CompanyName] IS NOT NULL AND LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0_1)) = @__ef_filter__TenantPrefix_0)) AND [c].[CustomerID] = @__customerID",
            //
            @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)
@__ef_filter__TenantPrefix_0_1='B' (Size = 40)
@__customerID='BLAUS' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (@__ef_filter__TenantPrefix_0 = N'' OR ([c].[CompanyName] IS NOT NULL AND LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0_1)) = @__ef_filter__TenantPrefix_0)) AND [c].[CustomerID] = @__customerID");
    }

    public override async Task Entity_Equality(bool async)
    {
        await base.Entity_Equality(async);

        AssertSql(
            @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)
@__ef_filter__TenantPrefix_0_1='B' (Size = 40)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
LEFT JOIN (
    SELECT [c].[CustomerID], [c].[CompanyName]
    FROM [Customers] AS [c]
    WHERE @__ef_filter__TenantPrefix_0 = N'' OR ([c].[CompanyName] IS NOT NULL AND LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0_1)) = @__ef_filter__TenantPrefix_0)
) AS [t] ON [o].[CustomerID] = [t].[CustomerID]
WHERE [t].[CustomerID] IS NOT NULL AND [t].[CompanyName] IS NOT NULL");
    }

    public override async Task Client_eval(bool async)
    {
        await base.Client_eval(async);

        AssertSql();
    }

    public override async Task Included_many_to_one_query2(bool async)
    {
        await base.Included_many_to_one_query2(async);

        AssertSql(
            @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)
@__ef_filter__TenantPrefix_0_1='B' (Size = 40)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM [Orders] AS [o]
LEFT JOIN (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE @__ef_filter__TenantPrefix_0 = N'' OR ([c].[CompanyName] IS NOT NULL AND LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0_1)) = @__ef_filter__TenantPrefix_0)
) AS [t] ON [o].[CustomerID] = [t].[CustomerID]
WHERE [t].[CustomerID] IS NOT NULL AND [t].[CompanyName] IS NOT NULL");
    }

    public override async Task Included_one_to_many_query_with_client_eval(bool async)
    {
        await base.Included_one_to_many_query_with_client_eval(async);

        AssertSql();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
