// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindSplitIncludeQuerySqlServerTest : NorthwindSplitIncludeQueryTestBase<
    NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
{
    public NorthwindSplitIncludeQuerySqlServerTest(
        NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Include_list(bool async)
    {
        await base.Include_list(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[ProductID] % 17 = 5 AND [p].[UnitPrice] < 20.0
ORDER BY [p].[ProductID]
""",
            //
            """
SELECT [s].[OrderID], [s].[ProductID], [s].[Discount], [s].[Quantity], [s].[UnitPrice], [s].[OrderID0], [s].[CustomerID], [s].[EmployeeID], [s].[OrderDate], [p].[ProductID]
FROM [Products] AS [p]
INNER JOIN (
    SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID] AS [OrderID0], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Order Details] AS [o]
    INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
) AS [s] ON [p].[ProductID] = [s].[ProductID]
WHERE [p].[ProductID] % 17 = 5 AND [p].[UnitPrice] < 20.0
ORDER BY [p].[ProductID]
""");
    }

    public override async Task Include_reference(bool async)
    {
        await base.Include_reference(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Include_when_result_operator(bool async)
    {
        await base.Include_when_result_operator(async);

        AssertSql(
            """
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c]) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Include_collection(bool async)
    {
        await base.Include_collection(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID]
""",
            //
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Include_collection_with_last(bool async)
    {
        await base.Include_collection_with_last(async);

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CompanyName] DESC, [c].[CustomerID]
""",
            //
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c0].[CustomerID]
FROM (
    SELECT TOP(1) [c].[CustomerID], [c].[CompanyName]
    FROM [Customers] AS [c]
    ORDER BY [c].[CompanyName] DESC
) AS [c0]
INNER JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [c0].[CompanyName] DESC, [c0].[CustomerID]
""");
    }

    public override async Task Include_collection_skip_no_order_by(bool async)
        => Assert.Equal(
            SqlServerStrings.SplitQueryOffsetWithoutOrderBy,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Include_collection_skip_no_order_by(async))).Message);

    public override async Task Include_collection_take_no_order_by(bool async)
    {
        await base.Include_collection_take_no_order_by(async);

        AssertSql(
            """
@__p_0='10'

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""",
            //
            """
@__p_0='10'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c0].[CustomerID]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [c0]
INNER JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [c0].[CustomerID]
""");
    }

    public override async Task Include_collection_skip_take_no_order_by(bool async)
        => Assert.Equal(
            SqlServerStrings.SplitQueryOffsetWithoutOrderBy,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Include_collection_skip_take_no_order_by(async))).Message);

    public override async Task Include_reference_and_collection(bool async)
    {
        await base.Include_reference_and_collection(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[CustomerID] LIKE N'F%'
ORDER BY [o].[OrderID], [c].[CustomerID]
""",
            //
            """
SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [o].[OrderID], [c].[CustomerID]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
INNER JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE [o].[CustomerID] LIKE N'F%'
ORDER BY [o].[OrderID], [c].[CustomerID]
""");
    }

    [ConditionalFact]
    public virtual void ToQueryString_for_include_reference_and_collection()
    {
        using var context = CreateContext();

        Assert.Equal(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [o].[OrderID], [c].[CustomerID]


"""
            + RelationalStrings.SplitQueryString,
            context.Set<Order>().Include(o => o.Customer).Include(o => o.OrderDetails).AsSplitQuery().ToQueryString(),
            ignoreLineEndingDifferences: true,
            ignoreWhiteSpaceDifferences: true);
    }

    public override async Task Include_references_multi_level(bool async)
    {
        await base.Include_references_multi_level(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] % 23 = 13
""");
    }

    public override async Task Include_multiple_references_multi_level(bool async)
    {
        await base.Include_multiple_references_multi_level(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
INNER JOIN [Products] AS [p] ON [o].[ProductID] = [p].[ProductID]
WHERE [o].[OrderID] % 23 = 13
""");
    }

    public override async Task Include_multiple_references_multi_level_reverse(bool async)
    {
        await base.Include_multiple_references_multi_level_reverse(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Order Details] AS [o]
INNER JOIN [Products] AS [p] ON [o].[ProductID] = [p].[ProductID]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] % 23 = 13
""");
    }

    public override async Task Include_references_and_collection_multi_level(bool async)
    {
        await base.Include_references_and_collection_multi_level(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] % 23 = 13 AND [o].[UnitPrice] < 10.0
ORDER BY [o].[OrderID], [o].[ProductID], [o0].[OrderID], [c].[CustomerID]
""",
            //
            """
SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [o].[OrderID], [o].[ProductID], [o0].[OrderID], [c].[CustomerID]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
INNER JOIN [Orders] AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
WHERE [o].[OrderID] % 23 = 13 AND [o].[UnitPrice] < 10.0
ORDER BY [o].[OrderID], [o].[ProductID], [o0].[OrderID], [c].[CustomerID]
""");
    }

    public override async Task Include_multi_level_reference_and_collection_predicate(bool async)
    {
        await base.Include_multi_level_reference_and_collection_predicate(async);

        AssertSql(
            """
SELECT TOP(2) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] = 10248
ORDER BY [o].[OrderID], [c].[CustomerID]
""",
            //
            """
SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [s].[OrderID], [s].[CustomerID]
FROM (
    SELECT TOP(1) [o].[OrderID], [c].[CustomerID]
    FROM [Orders] AS [o]
    LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
    WHERE [o].[OrderID] = 10248
) AS [s]
INNER JOIN [Orders] AS [o0] ON [s].[CustomerID] = [o0].[CustomerID]
ORDER BY [s].[OrderID], [s].[CustomerID]
""");
    }

    public override async Task Include_multi_level_collection_and_then_include_reference_predicate(bool async)
    {
        await base.Include_multi_level_collection_and_then_include_reference_predicate(async);

        AssertSql(
            """
SELECT TOP(2) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] = 10248
ORDER BY [o].[OrderID]
""",
            //
            """
SELECT [s].[OrderID], [s].[ProductID], [s].[Discount], [s].[Quantity], [s].[UnitPrice], [s].[ProductID0], [s].[Discontinued], [s].[ProductName], [s].[SupplierID], [s].[UnitPrice0], [s].[UnitsInStock], [o1].[OrderID]
FROM (
    SELECT TOP(1) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] = 10248
) AS [o1]
INNER JOIN (
    SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [p].[ProductID] AS [ProductID0], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice] AS [UnitPrice0], [p].[UnitsInStock]
    FROM [Order Details] AS [o0]
    INNER JOIN [Products] AS [p] ON [o0].[ProductID] = [p].[ProductID]
) AS [s] ON [o1].[OrderID] = [s].[OrderID]
ORDER BY [o1].[OrderID]
""");
    }

    public override async Task Include_collection_alias_generation(bool async)
    {
        await base.Include_collection_alias_generation(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] LIKE N'F%'
ORDER BY [o].[OrderID]
""",
            //
            """
SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [o].[OrderID]
FROM [Orders] AS [o]
INNER JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE [o].[CustomerID] LIKE N'F%'
ORDER BY [o].[OrderID]
""");
    }

    public override async Task Include_collection_order_by_collection_column(bool async)
    {
        await base.Include_collection_order_by_collection_column(async);

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'W%'
ORDER BY (
    SELECT TOP(1) [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderDate] DESC) DESC, [c].[CustomerID]
""",
            //
            """
SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c0].[CustomerID]
FROM (
    SELECT TOP(1) [c].[CustomerID], (
        SELECT TOP(1) [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderDate] DESC) AS [c]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'W%'
    ORDER BY (
        SELECT TOP(1) [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderDate] DESC) DESC
) AS [c0]
INNER JOIN [Orders] AS [o0] ON [c0].[CustomerID] = [o0].[CustomerID]
ORDER BY [c0].[c] DESC, [c0].[CustomerID]
""");
    }

    public override async Task Include_collection_order_by_key(bool async)
    {
        await base.Include_collection_order_by_key(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID]
""",
            //
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Include_collection_order_by_non_key(bool async)
    {
        await base.Include_collection_order_by_non_key(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[PostalCode], [c].[CustomerID]
""",
            //
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[PostalCode], [c].[CustomerID]
""");
    }

    public override async Task Include_collection_order_by_non_key_with_take(bool async)
    {
        await base.Include_collection_order_by_non_key_with_take(async);

        AssertSql(
            """
@__p_0='10'

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactTitle], [c].[CustomerID]
""",
            //
            """
@__p_0='10'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c0].[CustomerID]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[ContactTitle]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactTitle]
) AS [c0]
INNER JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [c0].[ContactTitle], [c0].[CustomerID]
""");
    }

    public override async Task Include_collection_order_by_non_key_with_skip(bool async)
    {
        await base.Include_collection_order_by_non_key_with_skip(async);

        AssertSql(
            """
@__p_0='2'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[ContactTitle], [c].[CustomerID]
OFFSET @__p_0 ROWS
""",
            //
            """
@__p_0='2'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c0].[CustomerID]
FROM (
    SELECT [c].[CustomerID], [c].[ContactTitle]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'F%'
    ORDER BY [c].[ContactTitle]
    OFFSET @__p_0 ROWS
) AS [c0]
INNER JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [c0].[ContactTitle], [c0].[CustomerID]
""");
    }

    public override async Task Include_collection_order_by_non_key_with_first_or_default(bool async)
    {
        await base.Include_collection_order_by_non_key_with_first_or_default(async);

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CompanyName] DESC, [c].[CustomerID]
""",
            //
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c0].[CustomerID]
FROM (
    SELECT TOP(1) [c].[CustomerID], [c].[CompanyName]
    FROM [Customers] AS [c]
    ORDER BY [c].[CompanyName] DESC
) AS [c0]
INNER JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [c0].[CompanyName] DESC, [c0].[CustomerID]
""");
    }

    public override async Task Include_collection_order_by_subquery(bool async)
    {
        await base.Include_collection_order_by_subquery(async);

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY (
    SELECT TOP(1) [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[EmployeeID]), [c].[CustomerID]
""",
            //
            """
SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c0].[CustomerID]
FROM (
    SELECT TOP(1) [c].[CustomerID], (
        SELECT TOP(1) [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[EmployeeID]) AS [c]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'ALFKI'
    ORDER BY (
        SELECT TOP(1) [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[EmployeeID])
) AS [c0]
INNER JOIN [Orders] AS [o0] ON [c0].[CustomerID] = [o0].[CustomerID]
ORDER BY [c0].[c], [c0].[CustomerID]
""");
    }

    public override async Task Include_collection_principal_already_tracked(bool async)
    {
        await base.Include_collection_principal_already_tracked(async);

        AssertSql(
            """
SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""",
            //
            """
SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]
""",
            //
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c0].[CustomerID]
FROM (
    SELECT TOP(1) [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'ALFKI'
) AS [c0]
INNER JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [c0].[CustomerID]
""");
    }

    public override async Task Include_collection_with_filter(bool async)
    {
        await base.Include_collection_with_filter(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]
""",
            //
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Include_collection_with_filter_reordered(bool async)
    {
        await base.Include_collection_with_filter_reordered(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]
""",
            //
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Include_collection_then_include_collection(bool async)
    {
        await base.Include_collection_then_include_collection(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID]
""",
            //
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID], [o].[OrderID]
""",
            //
            """
SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [c].[CustomerID], [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
INNER JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID], [o].[OrderID]
""");
    }

    public override async Task Include_collection_then_include_collection_then_include_reference(bool async)
    {
        await base.Include_collection_then_include_collection_then_include_reference(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID]
""",
            //
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID], [o].[OrderID]
""",
            //
            """
SELECT [s].[OrderID], [s].[ProductID], [s].[Discount], [s].[Quantity], [s].[UnitPrice], [s].[ProductID0], [s].[Discontinued], [s].[ProductName], [s].[SupplierID], [s].[UnitPrice0], [s].[UnitsInStock], [c].[CustomerID], [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
INNER JOIN (
    SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [p].[ProductID] AS [ProductID0], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice] AS [UnitPrice0], [p].[UnitsInStock]
    FROM [Order Details] AS [o0]
    INNER JOIN [Products] AS [p] ON [o0].[ProductID] = [p].[ProductID]
) AS [s] ON [o].[OrderID] = [s].[OrderID]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID], [o].[OrderID]
""");
    }

    public override async Task Include_collection_when_projection(bool async)
    {
        await base.Include_collection_when_projection(async);

        AssertSql(
            """
SELECT [c].[CustomerID]
FROM [Customers] AS [c]
""");
    }

    public override async Task Include_collection_with_join_clause_with_filter(bool async)
    {
        await base.Include_collection_with_join_clause_with_filter(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID], [o].[OrderID]
""",
            //
            """
SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
INNER JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID], [o].[OrderID]
""");
    }

    public override async Task Include_collection_with_left_join_clause_with_filter(bool async)
    {
        await base.Include_collection_with_left_join_clause_with_filter(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID], [o].[OrderID]
""",
            //
            """
SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [o].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
INNER JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID], [o].[OrderID]
""");
    }

    public override async Task Include_collection_with_cross_join_clause_with_filter(bool async)
    {
        await base.Include_collection_with_cross_join_clause_with_filter(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o0].[OrderID]
FROM [Customers] AS [c]
CROSS JOIN (
    SELECT TOP(5) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [o0]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID], [o0].[OrderID]
""",
            //
            """
SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [c].[CustomerID], [o0].[OrderID]
FROM [Customers] AS [c]
CROSS JOIN (
    SELECT TOP(5) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [o0]
INNER JOIN [Orders] AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID], [o0].[OrderID]
""");
    }

    public override async Task Include_collection_with_cross_apply_with_filter(bool async)
    {
        await base.Include_collection_with_cross_apply_with_filter(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o0].[OrderID]
FROM [Customers] AS [c]
CROSS APPLY (
    SELECT TOP(5) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = [c].[CustomerID]
    ORDER BY [c].[CustomerID]
) AS [o0]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID], [o0].[OrderID]
""",
            //
            """
SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [c].[CustomerID], [o0].[OrderID]
FROM [Customers] AS [c]
CROSS APPLY (
    SELECT TOP(5) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = [c].[CustomerID]
    ORDER BY [c].[CustomerID]
) AS [o0]
INNER JOIN [Orders] AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID], [o0].[OrderID]
""");
    }

    public override async Task Include_collection_with_outer_apply_with_filter(bool async)
    {
        await base.Include_collection_with_outer_apply_with_filter(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o0].[OrderID]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT TOP(5) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = [c].[CustomerID]
    ORDER BY [c].[CustomerID]
) AS [o0]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID], [o0].[OrderID]
""",
            //
            """
SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [c].[CustomerID], [o0].[OrderID]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT TOP(5) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = [c].[CustomerID]
    ORDER BY [c].[CustomerID]
) AS [o0]
INNER JOIN [Orders] AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID], [o0].[OrderID]
""");
    }

    public override async Task Include_collection_on_additional_from_clause_with_filter(bool async)
    {
        await base.Include_collection_on_additional_from_clause_with_filter(async);

        AssertSql(
            """
SELECT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region], [c].[CustomerID]
FROM [Customers] AS [c]
CROSS JOIN (
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
) AS [c1]
ORDER BY [c].[CustomerID], [c1].[CustomerID]
""",
            //
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c1].[CustomerID]
FROM [Customers] AS [c]
CROSS JOIN (
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
) AS [c1]
INNER JOIN [Orders] AS [o] ON [c1].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID], [c1].[CustomerID]
""");
    }

    public override async Task Include_collection_on_additional_from_clause(bool async)
    {
        await base.Include_collection_on_additional_from_clause(async);

        AssertSql(
            """
@__p_0='5'

SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region], [c1].[CustomerID]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c1]
CROSS JOIN (
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] LIKE N'F%'
) AS [c2]
ORDER BY [c1].[CustomerID], [c2].[CustomerID]
""",
            //
            """
@__p_0='5'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c1].[CustomerID], [c2].[CustomerID]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c1]
CROSS JOIN (
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] LIKE N'F%'
) AS [c2]
INNER JOIN [Orders] AS [o] ON [c2].[CustomerID] = [o].[CustomerID]
ORDER BY [c1].[CustomerID], [c2].[CustomerID]
""");
    }

    public override async Task Include_duplicate_collection(bool async)
    {
        await base.Include_duplicate_collection(async);

        AssertSql(
            """
@__p_0='2'

SELECT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region], [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c1]
CROSS JOIN (
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [c2]
ORDER BY [c1].[CustomerID], [c2].[CustomerID]
""",
            //
            """
@__p_0='2'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c1].[CustomerID], [c2].[CustomerID]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c1]
CROSS JOIN (
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [c2]
INNER JOIN [Orders] AS [o] ON [c1].[CustomerID] = [o].[CustomerID]
ORDER BY [c1].[CustomerID], [c2].[CustomerID]
""",
            //
            """
@__p_0='2'

SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c1].[CustomerID], [c2].[CustomerID]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c1]
CROSS JOIN (
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [c2]
INNER JOIN [Orders] AS [o0] ON [c2].[CustomerID] = [o0].[CustomerID]
ORDER BY [c1].[CustomerID], [c2].[CustomerID]
""");
    }

    public override async Task Include_duplicate_collection_result_operator(bool async)
    {
        await base.Include_duplicate_collection_result_operator(async);

        AssertSql(
            """
@__p_1='1'
@__p_0='2'

SELECT TOP(@__p_1) [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region], [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c1]
CROSS JOIN (
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [c2]
ORDER BY [c1].[CustomerID], [c2].[CustomerID]
""",
            //
            """
@__p_1='1'
@__p_0='2'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [s].[CustomerID], [s].[CustomerID0]
FROM (
    SELECT TOP(@__p_1) [c1].[CustomerID], [c2].[CustomerID] AS [CustomerID0]
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID]
        FROM [Customers] AS [c]
        ORDER BY [c].[CustomerID]
    ) AS [c1]
    CROSS JOIN (
        SELECT [c0].[CustomerID]
        FROM [Customers] AS [c0]
        ORDER BY [c0].[CustomerID]
        OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
    ) AS [c2]
    ORDER BY [c1].[CustomerID]
) AS [s]
INNER JOIN [Orders] AS [o] ON [s].[CustomerID] = [o].[CustomerID]
ORDER BY [s].[CustomerID], [s].[CustomerID0]
""",
            //
            """
@__p_1='1'
@__p_0='2'

SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [s0].[CustomerID], [s0].[CustomerID0]
FROM (
    SELECT TOP(@__p_1) [c1].[CustomerID], [c2].[CustomerID] AS [CustomerID0]
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID]
        FROM [Customers] AS [c]
        ORDER BY [c].[CustomerID]
    ) AS [c1]
    CROSS JOIN (
        SELECT [c0].[CustomerID]
        FROM [Customers] AS [c0]
        ORDER BY [c0].[CustomerID]
        OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
    ) AS [c2]
    ORDER BY [c1].[CustomerID]
) AS [s0]
INNER JOIN [Orders] AS [o0] ON [s0].[CustomerID0] = [o0].[CustomerID]
ORDER BY [s0].[CustomerID], [s0].[CustomerID0]
""");
    }

    public override async Task Include_collection_on_join_clause_with_order_by_and_filter(bool async)
    {
        await base.Include_collection_on_join_clause_with_order_by_and_filter(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[City], [c].[CustomerID], [o].[OrderID]
""",
            //
            """
SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
INNER JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[City], [c].[CustomerID], [o].[OrderID]
""");
    }

    public override async Task Include_collection_with_outer_apply_with_filter_non_equality(bool async)
    {
        await base.Include_collection_with_outer_apply_with_filter_non_equality(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o0].[OrderID]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT TOP(5) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] <> [c].[CustomerID] OR [o].[CustomerID] IS NULL
    ORDER BY [c].[CustomerID]
) AS [o0]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID], [o0].[OrderID]
""",
            //
            """
SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [c].[CustomerID], [o0].[OrderID]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT TOP(5) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] <> [c].[CustomerID] OR [o].[CustomerID] IS NULL
    ORDER BY [c].[CustomerID]
) AS [o0]
INNER JOIN [Orders] AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID], [o0].[OrderID]
""");
    }

    public override async Task Include_collection_on_additional_from_clause2(bool async)
    {
        await base.Include_collection_on_additional_from_clause2(async);

        AssertSql(
            """
@__p_0='5'

SELECT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c1]
CROSS JOIN [Customers] AS [c0]
ORDER BY [c1].[CustomerID]
""");
    }

    public override async Task Include_where_skip_take_projection(bool async)
    {
        await base.Include_where_skip_take_projection(async);

        AssertSql(
            """
@__p_0='1'
@__p_1='2'

SELECT [o0].[CustomerID]
FROM (
    SELECT [o].[OrderID], [o].[ProductID]
    FROM [Order Details] AS [o]
    WHERE [o].[Quantity] = CAST(10 AS smallint)
    ORDER BY [o].[OrderID], [o].[ProductID]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [o1]
INNER JOIN [Orders] AS [o0] ON [o1].[OrderID] = [o0].[OrderID]
ORDER BY [o1].[OrderID], [o1].[ProductID]
""");
    }

    public override async Task Include_duplicate_collection_result_operator2(bool async)
    {
        await base.Include_duplicate_collection_result_operator2(async);

        AssertSql(
            """
@__p_1='1'
@__p_0='2'

SELECT TOP(@__p_1) [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region], [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c1]
CROSS JOIN (
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [c2]
ORDER BY [c1].[CustomerID], [c2].[CustomerID]
""",
            //
            """
@__p_1='1'
@__p_0='2'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [s].[CustomerID], [s].[CustomerID0]
FROM (
    SELECT TOP(@__p_1) [c1].[CustomerID], [c2].[CustomerID] AS [CustomerID0]
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID]
        FROM [Customers] AS [c]
        ORDER BY [c].[CustomerID]
    ) AS [c1]
    CROSS JOIN (
        SELECT [c0].[CustomerID]
        FROM [Customers] AS [c0]
        ORDER BY [c0].[CustomerID]
        OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
    ) AS [c2]
    ORDER BY [c1].[CustomerID]
) AS [s]
INNER JOIN [Orders] AS [o] ON [s].[CustomerID] = [o].[CustomerID]
ORDER BY [s].[CustomerID], [s].[CustomerID0]
""");
    }

    public override async Task Include_multiple_references(bool async)
    {
        await base.Include_multiple_references(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
INNER JOIN [Products] AS [p] ON [o].[ProductID] = [p].[ProductID]
WHERE [o].[OrderID] % 23 = 13
""");
    }

    public override async Task Include_reference_alias_generation(bool async)
    {
        await base.Include_reference_alias_generation(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE [o].[OrderID] % 23 = 13
""");
    }

    public override async Task Include_duplicate_reference(bool async)
    {
        await base.Include_duplicate_reference(async);

        AssertSql(
            """
@__p_0='2'

SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate], [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[CustomerID], [o].[OrderID]
) AS [o1]
CROSS JOIN (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    ORDER BY [o0].[CustomerID], [o0].[OrderID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [o2]
LEFT JOIN [Customers] AS [c] ON [o1].[CustomerID] = [c].[CustomerID]
LEFT JOIN [Customers] AS [c0] ON [o2].[CustomerID] = [c0].[CustomerID]
ORDER BY [o1].[CustomerID], [o1].[OrderID]
""");
    }

    public override async Task Include_duplicate_reference2(bool async)
    {
        await base.Include_duplicate_reference2(async);

        AssertSql(
            """
@__p_0='2'

SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [o1]
CROSS JOIN (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    ORDER BY [o0].[OrderID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [o2]
LEFT JOIN [Customers] AS [c] ON [o1].[CustomerID] = [c].[CustomerID]
ORDER BY [o1].[OrderID]
""");
    }

    public override async Task Include_duplicate_reference3(bool async)
    {
        await base.Include_duplicate_reference3(async);

        AssertSql(
            """
@__p_0='2'

SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [o1]
CROSS JOIN (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    ORDER BY [o0].[OrderID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [o2]
LEFT JOIN [Customers] AS [c] ON [o2].[CustomerID] = [c].[CustomerID]
ORDER BY [o1].[OrderID]
""");
    }

    public override async Task Include_reference_when_projection(bool async)
    {
        await base.Include_reference_when_projection(async);

        AssertSql(
            """
SELECT [o].[CustomerID]
FROM [Orders] AS [o]
""");
    }

    public override async Task Include_reference_with_filter_reordered(bool async)
    {
        await base.Include_reference_with_filter_reordered(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task Include_reference_with_filter(bool async)
    {
        await base.Include_reference_with_filter(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task Include_collection_dependent_already_tracked(bool async)
    {
        await base.Include_collection_dependent_already_tracked(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
""",
            //
            """
SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]
""",
            //
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c0].[CustomerID]
FROM (
    SELECT TOP(1) [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'ALFKI'
) AS [c0]
INNER JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [c0].[CustomerID]
""");
    }

    public override async Task Include_reference_dependent_already_tracked(bool async)
    {
        await base.Include_reference_dependent_already_tracked(async);

        AssertSql(
            """
SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""",
            //
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task Include_with_complex_projection(bool async)
    {
        await base.Include_with_complex_projection(async);

        AssertSql(
            """
SELECT [c].[CustomerID] AS [Id]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
""");
    }

    public override async Task Include_with_complex_projection_does_not_change_ordering_of_projection(bool async)
    {
        await base.Include_with_complex_projection_does_not_change_ordering_of_projection(async);

        AssertSql(
            """
SELECT [c].[CustomerID] AS [Id], (
    SELECT COUNT(*)
    FROM [Orders] AS [o0]
    WHERE [c].[CustomerID] = [o0].[CustomerID]) AS [TotalOrders]
FROM [Customers] AS [c]
WHERE [c].[ContactTitle] = N'Owner' AND (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]) > 2
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Include_with_take(bool async)
    {
        await base.Include_with_take(async);

        AssertSql(
            """
@__p_0='10'

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactName] DESC, [c].[CustomerID]
""",
            //
            """
@__p_0='10'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c0].[CustomerID]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[ContactName]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactName] DESC
) AS [c0]
INNER JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [c0].[ContactName] DESC, [c0].[CustomerID]
""");
    }

    public override async Task Include_with_skip(bool async)
    {
        await base.Include_with_skip(async);

        AssertSql(
            """
@__p_0='80'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactName], [c].[CustomerID]
OFFSET @__p_0 ROWS
""",
            //
            """
@__p_0='80'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c0].[CustomerID]
FROM (
    SELECT [c].[CustomerID], [c].[ContactName]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactName]
    OFFSET @__p_0 ROWS
) AS [c0]
INNER JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [c0].[ContactName], [c0].[CustomerID]
""");
    }

    public override async Task Include_collection_with_multiple_conditional_order_by(bool async)
    {
        await base.Include_collection_with_multiple_conditional_order_by(async);

        AssertSql(
            """
@__p_0='5'

SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY CASE
    WHEN [o].[OrderID] > 0 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, CASE
    WHEN [c].[CustomerID] IS NOT NULL THEN [c].[City]
    ELSE N''
END, [o].[OrderID], [c].[CustomerID]
""",
            //
            """
@__p_0='5'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [s].[OrderID], [s].[CustomerID]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [c].[CustomerID], CASE
        WHEN [o].[OrderID] > 0 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [c], CASE
        WHEN [c].[CustomerID] IS NOT NULL THEN [c].[City]
        ELSE N''
    END AS [c0]
    FROM [Orders] AS [o]
    LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
    ORDER BY CASE
        WHEN [o].[OrderID] > 0 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END, CASE
        WHEN [c].[CustomerID] IS NOT NULL THEN [c].[City]
        ELSE N''
    END
) AS [s]
INNER JOIN [Order Details] AS [o0] ON [s].[OrderID] = [o0].[OrderID]
ORDER BY [s].[c], [s].[c0], [s].[OrderID], [s].[CustomerID]
""");
    }

    public override async Task Then_include_collection_order_by_collection_column(bool async)
    {
        await base.Then_include_collection_order_by_collection_column(async);

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'W%'
ORDER BY (
    SELECT TOP(1) [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderDate] DESC) DESC, [c].[CustomerID]
""",
            //
            """
SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c0].[CustomerID]
FROM (
    SELECT TOP(1) [c].[CustomerID], (
        SELECT TOP(1) [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderDate] DESC) AS [c]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'W%'
    ORDER BY (
        SELECT TOP(1) [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderDate] DESC) DESC
) AS [c0]
INNER JOIN [Orders] AS [o0] ON [c0].[CustomerID] = [o0].[CustomerID]
ORDER BY [c0].[c] DESC, [c0].[CustomerID], [o0].[OrderID]
""",
            //
            """
SELECT [o1].[OrderID], [o1].[ProductID], [o1].[Discount], [o1].[Quantity], [o1].[UnitPrice], [c0].[CustomerID], [o0].[OrderID]
FROM (
    SELECT TOP(1) [c].[CustomerID], (
        SELECT TOP(1) [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderDate] DESC) AS [c]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'W%'
    ORDER BY (
        SELECT TOP(1) [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderDate] DESC) DESC
) AS [c0]
INNER JOIN [Orders] AS [o0] ON [c0].[CustomerID] = [o0].[CustomerID]
INNER JOIN [Order Details] AS [o1] ON [o0].[OrderID] = [o1].[OrderID]
ORDER BY [c0].[c] DESC, [c0].[CustomerID], [o0].[OrderID]
""");
    }

    public override async Task Include_collection_with_conditional_order_by(bool async)
    {
        await base.Include_collection_with_conditional_order_by(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY CASE
    WHEN [c].[CustomerID] LIKE N'S%' THEN 1
    ELSE 2
END, [c].[CustomerID]
""",
            //
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY CASE
    WHEN [c].[CustomerID] LIKE N'S%' THEN 1
    ELSE 2
END, [c].[CustomerID]
""");
    }

    public override async Task Include_collection_GroupBy_Select(bool async)
    {
        await base.Include_collection_GroupBy_Select(async);

        AssertSql(
            """
SELECT [o3].[OrderID], [o3].[CustomerID], [o3].[EmployeeID], [o3].[OrderDate], [o1].[OrderID]
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] = 10248
    GROUP BY [o].[OrderID]
) AS [o1]
LEFT JOIN (
    SELECT [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate]
    FROM (
        SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], ROW_NUMBER() OVER(PARTITION BY [o0].[OrderID] ORDER BY [o0].[OrderID]) AS [row]
        FROM [Orders] AS [o0]
        WHERE [o0].[OrderID] = 10248
    ) AS [o2]
    WHERE [o2].[row] <= 1
) AS [o3] ON [o1].[OrderID] = [o3].[OrderID]
ORDER BY [o1].[OrderID], [o3].[OrderID]
""",
            //
            """
SELECT [o6].[OrderID], [o6].[ProductID], [o6].[Discount], [o6].[Quantity], [o6].[UnitPrice], [o7].[OrderID], [o9].[OrderID]
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] = 10248
    GROUP BY [o].[OrderID]
) AS [o7]
LEFT JOIN (
    SELECT [o8].[OrderID]
    FROM (
        SELECT [o5].[OrderID], ROW_NUMBER() OVER(PARTITION BY [o5].[OrderID] ORDER BY [o5].[OrderID]) AS [row]
        FROM [Orders] AS [o5]
        WHERE [o5].[OrderID] = 10248
    ) AS [o8]
    WHERE [o8].[row] <= 1
) AS [o9] ON [o7].[OrderID] = [o9].[OrderID]
INNER JOIN [Order Details] AS [o6] ON [o9].[OrderID] = [o6].[OrderID]
ORDER BY [o7].[OrderID], [o9].[OrderID]
""");
    }

    public override async Task Include_reference_GroupBy_Select(bool async)
    {
        await base.Include_reference_GroupBy_Select(async);

        AssertSql(
            """
SELECT [s0].[OrderID], [s0].[CustomerID], [s0].[EmployeeID], [s0].[OrderDate], [s0].[CustomerID0], [s0].[Address], [s0].[City], [s0].[CompanyName], [s0].[ContactName], [s0].[ContactTitle], [s0].[Country], [s0].[Fax], [s0].[Phone], [s0].[PostalCode], [s0].[Region]
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] = 10248
    GROUP BY [o].[OrderID]
) AS [o1]
LEFT JOIN (
    SELECT [s].[OrderID], [s].[CustomerID], [s].[EmployeeID], [s].[OrderDate], [s].[CustomerID0], [s].[Address], [s].[City], [s].[CompanyName], [s].[ContactName], [s].[ContactTitle], [s].[Country], [s].[Fax], [s].[Phone], [s].[PostalCode], [s].[Region]
    FROM (
        SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID] AS [CustomerID0], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(PARTITION BY [o0].[OrderID] ORDER BY [o0].[OrderID]) AS [row]
        FROM [Orders] AS [o0]
        LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
        WHERE [o0].[OrderID] = 10248
    ) AS [s]
    WHERE [s].[row] <= 1
) AS [s0] ON [o1].[OrderID] = [s0].[OrderID]
""");
    }

    public override async Task Include_collection_Join_GroupBy_Select(bool async)
    {
        await base.Include_collection_Join_GroupBy_Select(async);

        AssertSql(
            """
SELECT [s1].[OrderID], [s1].[CustomerID], [s1].[EmployeeID], [s1].[OrderDate], [s].[OrderID], [s1].[OrderID0], [s1].[ProductID]
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    INNER JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
    WHERE [o].[OrderID] = 10248
    GROUP BY [o].[OrderID]
) AS [s]
LEFT JOIN (
    SELECT [s0].[OrderID], [s0].[CustomerID], [s0].[EmployeeID], [s0].[OrderDate], [s0].[OrderID0], [s0].[ProductID]
    FROM (
        SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [o2].[OrderID] AS [OrderID0], [o2].[ProductID], ROW_NUMBER() OVER(PARTITION BY [o1].[OrderID] ORDER BY [o1].[OrderID]) AS [row]
        FROM [Orders] AS [o1]
        INNER JOIN [Order Details] AS [o2] ON [o1].[OrderID] = [o2].[OrderID]
        WHERE [o1].[OrderID] = 10248
    ) AS [s0]
    WHERE [s0].[row] <= 1
) AS [s1] ON [s].[OrderID] = [s1].[OrderID]
ORDER BY [s].[OrderID], [s1].[OrderID], [s1].[OrderID0], [s1].[ProductID]
""",
            //
            """
SELECT [o11].[OrderID], [o11].[ProductID], [o11].[Discount], [o11].[Quantity], [o11].[UnitPrice], [s].[OrderID], [s1].[OrderID], [s1].[OrderID0], [s1].[ProductID]
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    INNER JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
    WHERE [o].[OrderID] = 10248
    GROUP BY [o].[OrderID]
) AS [s]
LEFT JOIN (
    SELECT [s0].[OrderID], [s0].[OrderID0], [s0].[ProductID]
    FROM (
        SELECT [o9].[OrderID], [o10].[OrderID] AS [OrderID0], [o10].[ProductID], ROW_NUMBER() OVER(PARTITION BY [o9].[OrderID] ORDER BY [o9].[OrderID]) AS [row]
        FROM [Orders] AS [o9]
        INNER JOIN [Order Details] AS [o10] ON [o9].[OrderID] = [o10].[OrderID]
        WHERE [o9].[OrderID] = 10248
    ) AS [s0]
    WHERE [s0].[row] <= 1
) AS [s1] ON [s].[OrderID] = [s1].[OrderID]
INNER JOIN [Order Details] AS [o11] ON [s1].[OrderID] = [o11].[OrderID]
ORDER BY [s].[OrderID], [s1].[OrderID], [s1].[OrderID0], [s1].[ProductID]
""");
    }

    public override async Task Include_reference_Join_GroupBy_Select(bool async)
    {
        await base.Include_reference_Join_GroupBy_Select(async);

        AssertSql(
            """
SELECT [s1].[OrderID], [s1].[CustomerID], [s1].[EmployeeID], [s1].[OrderDate], [s1].[CustomerID0], [s1].[Address], [s1].[City], [s1].[CompanyName], [s1].[ContactName], [s1].[ContactTitle], [s1].[Country], [s1].[Fax], [s1].[Phone], [s1].[PostalCode], [s1].[Region]
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    INNER JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
    WHERE [o].[OrderID] = 10248
    GROUP BY [o].[OrderID]
) AS [s]
LEFT JOIN (
    SELECT [s0].[OrderID], [s0].[CustomerID], [s0].[EmployeeID], [s0].[OrderDate], [s0].[CustomerID0], [s0].[Address], [s0].[City], [s0].[CompanyName], [s0].[ContactName], [s0].[ContactTitle], [s0].[Country], [s0].[Fax], [s0].[Phone], [s0].[PostalCode], [s0].[Region]
    FROM (
        SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [c].[CustomerID] AS [CustomerID0], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(PARTITION BY [o1].[OrderID] ORDER BY [o1].[OrderID]) AS [row]
        FROM [Orders] AS [o1]
        INNER JOIN [Order Details] AS [o2] ON [o1].[OrderID] = [o2].[OrderID]
        LEFT JOIN [Customers] AS [c] ON [o1].[CustomerID] = [c].[CustomerID]
        WHERE [o1].[OrderID] = 10248
    ) AS [s0]
    WHERE [s0].[row] <= 1
) AS [s1] ON [s].[OrderID] = [s1].[OrderID]
""");
    }

    public override async Task Join_Include_collection_GroupBy_Select(bool async)
    {
        await base.Join_Include_collection_GroupBy_Select(async);

        AssertSql(
            """
SELECT [s1].[OrderID], [s1].[CustomerID], [s1].[EmployeeID], [s1].[OrderDate], [s].[OrderID], [s1].[OrderID0], [s1].[ProductID]
FROM (
    SELECT [o0].[OrderID]
    FROM [Order Details] AS [o]
    INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
    WHERE [o].[OrderID] = 10248
    GROUP BY [o0].[OrderID]
) AS [s]
LEFT JOIN (
    SELECT [s0].[OrderID], [s0].[CustomerID], [s0].[EmployeeID], [s0].[OrderDate], [s0].[OrderID0], [s0].[ProductID]
    FROM (
        SELECT [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate], [o1].[OrderID] AS [OrderID0], [o1].[ProductID], ROW_NUMBER() OVER(PARTITION BY [o2].[OrderID] ORDER BY [o2].[OrderID]) AS [row]
        FROM [Order Details] AS [o1]
        INNER JOIN [Orders] AS [o2] ON [o1].[OrderID] = [o2].[OrderID]
        WHERE [o1].[OrderID] = 10248
    ) AS [s0]
    WHERE [s0].[row] <= 1
) AS [s1] ON [s].[OrderID] = [s1].[OrderID]
ORDER BY [s].[OrderID], [s1].[OrderID0], [s1].[ProductID], [s1].[OrderID]
""",
            //
            """
SELECT [o11].[OrderID], [o11].[ProductID], [o11].[Discount], [o11].[Quantity], [o11].[UnitPrice], [s].[OrderID], [s1].[OrderID0], [s1].[ProductID], [s1].[OrderID]
FROM (
    SELECT [o0].[OrderID]
    FROM [Order Details] AS [o]
    INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
    WHERE [o].[OrderID] = 10248
    GROUP BY [o0].[OrderID]
) AS [s]
LEFT JOIN (
    SELECT [s0].[OrderID], [s0].[OrderID0], [s0].[ProductID]
    FROM (
        SELECT [o10].[OrderID], [o9].[OrderID] AS [OrderID0], [o9].[ProductID], ROW_NUMBER() OVER(PARTITION BY [o10].[OrderID] ORDER BY [o10].[OrderID]) AS [row]
        FROM [Order Details] AS [o9]
        INNER JOIN [Orders] AS [o10] ON [o9].[OrderID] = [o10].[OrderID]
        WHERE [o9].[OrderID] = 10248
    ) AS [s0]
    WHERE [s0].[row] <= 1
) AS [s1] ON [s].[OrderID] = [s1].[OrderID]
INNER JOIN [Order Details] AS [o11] ON [s1].[OrderID] = [o11].[OrderID]
ORDER BY [s].[OrderID], [s1].[OrderID0], [s1].[ProductID], [s1].[OrderID]
""");
    }

    public override async Task Join_Include_reference_GroupBy_Select(bool async)
    {
        await base.Join_Include_reference_GroupBy_Select(async);

        AssertSql(
            """
SELECT [s1].[OrderID], [s1].[CustomerID], [s1].[EmployeeID], [s1].[OrderDate], [s1].[CustomerID0], [s1].[Address], [s1].[City], [s1].[CompanyName], [s1].[ContactName], [s1].[ContactTitle], [s1].[Country], [s1].[Fax], [s1].[Phone], [s1].[PostalCode], [s1].[Region]
FROM (
    SELECT [o0].[OrderID]
    FROM [Order Details] AS [o]
    INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
    GROUP BY [o0].[OrderID]
) AS [s]
LEFT JOIN (
    SELECT [s0].[OrderID], [s0].[CustomerID], [s0].[EmployeeID], [s0].[OrderDate], [s0].[CustomerID0], [s0].[Address], [s0].[City], [s0].[CompanyName], [s0].[ContactName], [s0].[ContactTitle], [s0].[Country], [s0].[Fax], [s0].[Phone], [s0].[PostalCode], [s0].[Region]
    FROM (
        SELECT [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate], [c].[CustomerID] AS [CustomerID0], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(PARTITION BY [o2].[OrderID] ORDER BY [o2].[OrderID]) AS [row]
        FROM [Order Details] AS [o1]
        INNER JOIN [Orders] AS [o2] ON [o1].[OrderID] = [o2].[OrderID]
        LEFT JOIN [Customers] AS [c] ON [o2].[CustomerID] = [c].[CustomerID]
    ) AS [s0]
    WHERE [s0].[row] <= 1
) AS [s1] ON [s].[OrderID] = [s1].[OrderID]
""");
    }

    public override async Task Include_collection_SelectMany_GroupBy_Select(bool async)
    {
        await base.Include_collection_SelectMany_GroupBy_Select(async);

        AssertSql(
            """
SELECT [s1].[OrderID], [s1].[CustomerID], [s1].[EmployeeID], [s1].[OrderDate], [s].[OrderID], [s1].[OrderID0], [s1].[ProductID]
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    CROSS JOIN [Order Details] AS [o0]
    WHERE [o].[OrderID] = 10248
    GROUP BY [o].[OrderID]
) AS [s]
LEFT JOIN (
    SELECT [s0].[OrderID], [s0].[CustomerID], [s0].[EmployeeID], [s0].[OrderDate], [s0].[OrderID0], [s0].[ProductID]
    FROM (
        SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [o2].[OrderID] AS [OrderID0], [o2].[ProductID], ROW_NUMBER() OVER(PARTITION BY [o1].[OrderID] ORDER BY [o1].[OrderID]) AS [row]
        FROM [Orders] AS [o1]
        CROSS JOIN [Order Details] AS [o2]
        WHERE [o1].[OrderID] = 10248
    ) AS [s0]
    WHERE [s0].[row] <= 1
) AS [s1] ON [s].[OrderID] = [s1].[OrderID]
ORDER BY [s].[OrderID], [s1].[OrderID], [s1].[OrderID0], [s1].[ProductID]
""",
            //
            """
SELECT [o11].[OrderID], [o11].[ProductID], [o11].[Discount], [o11].[Quantity], [o11].[UnitPrice], [s].[OrderID], [s1].[OrderID], [s1].[OrderID0], [s1].[ProductID]
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    CROSS JOIN [Order Details] AS [o0]
    WHERE [o].[OrderID] = 10248
    GROUP BY [o].[OrderID]
) AS [s]
LEFT JOIN (
    SELECT [s0].[OrderID], [s0].[OrderID0], [s0].[ProductID]
    FROM (
        SELECT [o9].[OrderID], [o10].[OrderID] AS [OrderID0], [o10].[ProductID], ROW_NUMBER() OVER(PARTITION BY [o9].[OrderID] ORDER BY [o9].[OrderID]) AS [row]
        FROM [Orders] AS [o9]
        CROSS JOIN [Order Details] AS [o10]
        WHERE [o9].[OrderID] = 10248
    ) AS [s0]
    WHERE [s0].[row] <= 1
) AS [s1] ON [s].[OrderID] = [s1].[OrderID]
INNER JOIN [Order Details] AS [o11] ON [s1].[OrderID] = [o11].[OrderID]
ORDER BY [s].[OrderID], [s1].[OrderID], [s1].[OrderID0], [s1].[ProductID]
""");
    }

    public override async Task Include_reference_SelectMany_GroupBy_Select(bool async)
    {
        await base.Include_reference_SelectMany_GroupBy_Select(async);

        AssertSql(
            """
SELECT [s1].[OrderID], [s1].[CustomerID], [s1].[EmployeeID], [s1].[OrderDate], [s1].[CustomerID0], [s1].[Address], [s1].[City], [s1].[CompanyName], [s1].[ContactName], [s1].[ContactTitle], [s1].[Country], [s1].[Fax], [s1].[Phone], [s1].[PostalCode], [s1].[Region]
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    CROSS JOIN [Order Details] AS [o0]
    WHERE [o].[OrderID] = 10248
    GROUP BY [o].[OrderID]
) AS [s]
LEFT JOIN (
    SELECT [s0].[OrderID], [s0].[CustomerID], [s0].[EmployeeID], [s0].[OrderDate], [s0].[CustomerID0], [s0].[Address], [s0].[City], [s0].[CompanyName], [s0].[ContactName], [s0].[ContactTitle], [s0].[Country], [s0].[Fax], [s0].[Phone], [s0].[PostalCode], [s0].[Region]
    FROM (
        SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [c].[CustomerID] AS [CustomerID0], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(PARTITION BY [o1].[OrderID] ORDER BY [o1].[OrderID]) AS [row]
        FROM [Orders] AS [o1]
        CROSS JOIN [Order Details] AS [o2]
        LEFT JOIN [Customers] AS [c] ON [o1].[CustomerID] = [c].[CustomerID]
        WHERE [o1].[OrderID] = 10248
    ) AS [s0]
    WHERE [s0].[row] <= 1
) AS [s1] ON [s].[OrderID] = [s1].[OrderID]
""");
    }

    public override async Task SelectMany_Include_collection_GroupBy_Select(bool async)
    {
        await base.SelectMany_Include_collection_GroupBy_Select(async);

        AssertSql(
            """
SELECT [s1].[OrderID], [s1].[CustomerID], [s1].[EmployeeID], [s1].[OrderDate], [s].[OrderID], [s1].[OrderID0], [s1].[ProductID]
FROM (
    SELECT [o0].[OrderID]
    FROM [Order Details] AS [o]
    CROSS JOIN [Orders] AS [o0]
    WHERE [o].[OrderID] = 10248
    GROUP BY [o0].[OrderID]
) AS [s]
LEFT JOIN (
    SELECT [s0].[OrderID], [s0].[CustomerID], [s0].[EmployeeID], [s0].[OrderDate], [s0].[OrderID0], [s0].[ProductID]
    FROM (
        SELECT [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate], [o1].[OrderID] AS [OrderID0], [o1].[ProductID], ROW_NUMBER() OVER(PARTITION BY [o2].[OrderID] ORDER BY [o2].[OrderID]) AS [row]
        FROM [Order Details] AS [o1]
        CROSS JOIN [Orders] AS [o2]
        WHERE [o1].[OrderID] = 10248
    ) AS [s0]
    WHERE [s0].[row] <= 1
) AS [s1] ON [s].[OrderID] = [s1].[OrderID]
ORDER BY [s].[OrderID], [s1].[OrderID0], [s1].[ProductID], [s1].[OrderID]
""",
            //
            """
SELECT [o11].[OrderID], [o11].[ProductID], [o11].[Discount], [o11].[Quantity], [o11].[UnitPrice], [s].[OrderID], [s1].[OrderID0], [s1].[ProductID], [s1].[OrderID]
FROM (
    SELECT [o0].[OrderID]
    FROM [Order Details] AS [o]
    CROSS JOIN [Orders] AS [o0]
    WHERE [o].[OrderID] = 10248
    GROUP BY [o0].[OrderID]
) AS [s]
LEFT JOIN (
    SELECT [s0].[OrderID], [s0].[OrderID0], [s0].[ProductID]
    FROM (
        SELECT [o10].[OrderID], [o9].[OrderID] AS [OrderID0], [o9].[ProductID], ROW_NUMBER() OVER(PARTITION BY [o10].[OrderID] ORDER BY [o10].[OrderID]) AS [row]
        FROM [Order Details] AS [o9]
        CROSS JOIN [Orders] AS [o10]
        WHERE [o9].[OrderID] = 10248
    ) AS [s0]
    WHERE [s0].[row] <= 1
) AS [s1] ON [s].[OrderID] = [s1].[OrderID]
INNER JOIN [Order Details] AS [o11] ON [s1].[OrderID] = [o11].[OrderID]
ORDER BY [s].[OrderID], [s1].[OrderID0], [s1].[ProductID], [s1].[OrderID]
""");
    }

    public override async Task SelectMany_Include_reference_GroupBy_Select(bool async)
    {
        await base.SelectMany_Include_reference_GroupBy_Select(async);

        AssertSql(
            """
SELECT [s1].[OrderID], [s1].[CustomerID], [s1].[EmployeeID], [s1].[OrderDate], [s1].[CustomerID0], [s1].[Address], [s1].[City], [s1].[CompanyName], [s1].[ContactName], [s1].[ContactTitle], [s1].[Country], [s1].[Fax], [s1].[Phone], [s1].[PostalCode], [s1].[Region]
FROM (
    SELECT [o0].[OrderID]
    FROM [Order Details] AS [o]
    CROSS JOIN [Orders] AS [o0]
    WHERE [o].[OrderID] = 10248
    GROUP BY [o0].[OrderID]
) AS [s]
LEFT JOIN (
    SELECT [s0].[OrderID], [s0].[CustomerID], [s0].[EmployeeID], [s0].[OrderDate], [s0].[CustomerID0], [s0].[Address], [s0].[City], [s0].[CompanyName], [s0].[ContactName], [s0].[ContactTitle], [s0].[Country], [s0].[Fax], [s0].[Phone], [s0].[PostalCode], [s0].[Region]
    FROM (
        SELECT [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate], [c].[CustomerID] AS [CustomerID0], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(PARTITION BY [o2].[OrderID] ORDER BY [o2].[OrderID]) AS [row]
        FROM [Order Details] AS [o1]
        CROSS JOIN [Orders] AS [o2]
        LEFT JOIN [Customers] AS [c] ON [o2].[CustomerID] = [c].[CustomerID]
        WHERE [o1].[OrderID] = 10248
    ) AS [s0]
    WHERE [s0].[row] <= 1
) AS [s1] ON [s].[OrderID] = [s1].[OrderID]
""");
    }

    public override async Task Include_reference_distinct_is_server_evaluated(bool async)
    {
        await base.Include_reference_distinct_is_server_evaluated(async);

        AssertSql(
            """
SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT DISTINCT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10250
) AS [o0]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
""");
    }

    public override async Task Include_collection_distinct_is_server_evaluated(bool async)
    {
        await base.Include_collection_distinct_is_server_evaluated(async);

        AssertSql(
            """
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'A%'
) AS [c0]
ORDER BY [c0].[CustomerID]
""",
            //
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c0].[CustomerID]
FROM (
    SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'A%'
) AS [c0]
INNER JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [c0].[CustomerID]
""");
    }

    public override async Task Include_collection_OrderBy_object(bool async)
    {
        await base.Include_collection_OrderBy_object(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10250
ORDER BY [o].[OrderID]
""",
            //
            """
SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [o].[OrderID]
FROM [Orders] AS [o]
INNER JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE [o].[OrderID] < 10250
ORDER BY [o].[OrderID]
""");
    }

    public override async Task Include_collection_OrderBy_empty_list_contains(bool async)
    {
        await base.Include_collection_OrderBy_empty_list_contains(async);

        AssertSql(
            """
@__list_0='[]' (Size = 4000)
@__p_1='1'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY CASE
    WHEN [c].[CustomerID] IN (
        SELECT [l].[value]
        FROM OPENJSON(@__list_0) WITH ([value] nchar(5) '$') AS [l]
    ) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [c].[CustomerID]
OFFSET @__p_1 ROWS
""",
            //
            """
@__list_0='[]' (Size = 4000)
@__p_1='1'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c0].[CustomerID]
FROM (
    SELECT [c].[CustomerID], CASE
        WHEN [c].[CustomerID] IN (
            SELECT [l].[value]
            FROM OPENJSON(@__list_0) WITH ([value] nchar(5) '$') AS [l]
        ) THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [c]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'A%'
    ORDER BY CASE
        WHEN [c].[CustomerID] IN (
            SELECT [l].[value]
            FROM OPENJSON(@__list_0) WITH ([value] nchar(5) '$') AS [l]
        ) THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    OFFSET @__p_1 ROWS
) AS [c0]
INNER JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [c0].[c], [c0].[CustomerID]
""");
    }

    public override async Task Include_collection_OrderBy_empty_list_does_not_contains(bool async)
    {
        await base.Include_collection_OrderBy_empty_list_does_not_contains(async);

        AssertSql(
            """
@__list_0='[]' (Size = 4000)
@__p_1='1'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY CASE
    WHEN [c].[CustomerID] NOT IN (
        SELECT [l].[value]
        FROM OPENJSON(@__list_0) WITH ([value] nchar(5) '$') AS [l]
    ) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [c].[CustomerID]
OFFSET @__p_1 ROWS
""",
            //
            """
@__list_0='[]' (Size = 4000)
@__p_1='1'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c0].[CustomerID]
FROM (
    SELECT [c].[CustomerID], CASE
        WHEN [c].[CustomerID] NOT IN (
            SELECT [l].[value]
            FROM OPENJSON(@__list_0) WITH ([value] nchar(5) '$') AS [l]
        ) THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [c]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'A%'
    ORDER BY CASE
        WHEN [c].[CustomerID] NOT IN (
            SELECT [l].[value]
            FROM OPENJSON(@__list_0) WITH ([value] nchar(5) '$') AS [l]
        ) THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    OFFSET @__p_1 ROWS
) AS [c0]
INNER JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [c0].[c], [c0].[CustomerID]
""");
    }

    public override async Task Include_collection_OrderBy_list_contains(bool async)
    {
        await base.Include_collection_OrderBy_list_contains(async);

        AssertSql(
            """
@__list_0='["ALFKI"]' (Size = 4000)
@__p_1='1'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY CASE
    WHEN [c].[CustomerID] IN (
        SELECT [l].[value]
        FROM OPENJSON(@__list_0) WITH ([value] nchar(5) '$') AS [l]
    ) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [c].[CustomerID]
OFFSET @__p_1 ROWS
""",
            //
            """
@__list_0='["ALFKI"]' (Size = 4000)
@__p_1='1'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c0].[CustomerID]
FROM (
    SELECT [c].[CustomerID], CASE
        WHEN [c].[CustomerID] IN (
            SELECT [l].[value]
            FROM OPENJSON(@__list_0) WITH ([value] nchar(5) '$') AS [l]
        ) THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [c]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'A%'
    ORDER BY CASE
        WHEN [c].[CustomerID] IN (
            SELECT [l].[value]
            FROM OPENJSON(@__list_0) WITH ([value] nchar(5) '$') AS [l]
        ) THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    OFFSET @__p_1 ROWS
) AS [c0]
INNER JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [c0].[c], [c0].[CustomerID]
""");
    }

    public override async Task Include_collection_OrderBy_list_does_not_contains(bool async)
    {
        await base.Include_collection_OrderBy_list_does_not_contains(async);

        AssertSql(
            """
@__list_0='["ALFKI"]' (Size = 4000)
@__p_1='1'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY CASE
    WHEN [c].[CustomerID] NOT IN (
        SELECT [l].[value]
        FROM OPENJSON(@__list_0) WITH ([value] nchar(5) '$') AS [l]
    ) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [c].[CustomerID]
OFFSET @__p_1 ROWS
""",
            //
            """
@__list_0='["ALFKI"]' (Size = 4000)
@__p_1='1'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c0].[CustomerID]
FROM (
    SELECT [c].[CustomerID], CASE
        WHEN [c].[CustomerID] NOT IN (
            SELECT [l].[value]
            FROM OPENJSON(@__list_0) WITH ([value] nchar(5) '$') AS [l]
        ) THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [c]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'A%'
    ORDER BY CASE
        WHEN [c].[CustomerID] NOT IN (
            SELECT [l].[value]
            FROM OPENJSON(@__list_0) WITH ([value] nchar(5) '$') AS [l]
        ) THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    OFFSET @__p_1 ROWS
) AS [c0]
INNER JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [c0].[c], [c0].[CustomerID]
""");
    }

    public override async Task Include_is_not_ignored_when_projection_contains_client_method_and_complex_expression(bool async)
    {
        await base.Include_is_not_ignored_when_projection_contains_client_method_and_complex_expression(async);

        AssertSql(
            """
SELECT CASE
    WHEN [e0].[EmployeeID] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title], [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title]
FROM [Employees] AS [e]
LEFT JOIN [Employees] AS [e0] ON [e].[ReportsTo] = [e0].[EmployeeID]
WHERE [e].[EmployeeID] IN (1, 2)
ORDER BY [e].[EmployeeID]
""");
    }

    public override async Task Multi_level_includes_are_applied_with_skip(bool async)
    {
        await base.Multi_level_includes_are_applied_with_skip(async);

        AssertSql(
            """
@__p_0='1'

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID]
OFFSET @__p_0 ROWS FETCH NEXT 1 ROWS ONLY
""",
            //
            """
@__p_0='1'

SELECT [o3].[OrderID], [o3].[CustomerID], [o3].[EmployeeID], [o3].[OrderDate], [c0].[CustomerID]
FROM (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'A%'
    ORDER BY [c].[CustomerID]
    OFFSET @__p_0 ROWS FETCH NEXT 1 ROWS ONLY
) AS [c0]
INNER JOIN [Orders] AS [o3] ON [c0].[CustomerID] = [o3].[CustomerID]
ORDER BY [c0].[CustomerID], [o3].[OrderID]
""",
            //
            """
@__p_0='1'

SELECT [o4].[OrderID], [o4].[ProductID], [o4].[Discount], [o4].[Quantity], [o4].[UnitPrice], [c0].[CustomerID], [o3].[OrderID]
FROM (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'A%'
    ORDER BY [c].[CustomerID]
    OFFSET @__p_0 ROWS FETCH NEXT 1 ROWS ONLY
) AS [c0]
INNER JOIN [Orders] AS [o3] ON [c0].[CustomerID] = [o3].[CustomerID]
INNER JOIN [Order Details] AS [o4] ON [o3].[OrderID] = [o4].[OrderID]
ORDER BY [c0].[CustomerID], [o3].[OrderID]
""");
    }

    public override async Task Multi_level_includes_are_applied_with_take(bool async)
    {
        await base.Multi_level_includes_are_applied_with_take(async);

        AssertSql(
            """
@__p_0='1'

SELECT TOP(1) [c0].[CustomerID]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'A%'
    ORDER BY [c].[CustomerID]
) AS [c0]
ORDER BY [c0].[CustomerID]
""",
            //
            """
@__p_0='1'

SELECT [o3].[OrderID], [o3].[CustomerID], [o3].[EmployeeID], [o3].[OrderDate], [c1].[CustomerID]
FROM (
    SELECT TOP(1) [c0].[CustomerID]
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID]
        FROM [Customers] AS [c]
        WHERE [c].[CustomerID] LIKE N'A%'
        ORDER BY [c].[CustomerID]
    ) AS [c0]
    ORDER BY [c0].[CustomerID]
) AS [c1]
INNER JOIN [Orders] AS [o3] ON [c1].[CustomerID] = [o3].[CustomerID]
ORDER BY [c1].[CustomerID], [o3].[OrderID]
""",
            //
            """
@__p_0='1'

SELECT [o4].[OrderID], [o4].[ProductID], [o4].[Discount], [o4].[Quantity], [o4].[UnitPrice], [c1].[CustomerID], [o3].[OrderID]
FROM (
    SELECT TOP(1) [c0].[CustomerID]
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID]
        FROM [Customers] AS [c]
        WHERE [c].[CustomerID] LIKE N'A%'
        ORDER BY [c].[CustomerID]
    ) AS [c0]
    ORDER BY [c0].[CustomerID]
) AS [c1]
INNER JOIN [Orders] AS [o3] ON [c1].[CustomerID] = [o3].[CustomerID]
INNER JOIN [Order Details] AS [o4] ON [o3].[OrderID] = [o4].[OrderID]
ORDER BY [c1].[CustomerID], [o3].[OrderID]
""");
    }

    public override async Task Multi_level_includes_are_applied_with_skip_take(bool async)
    {
        await base.Multi_level_includes_are_applied_with_skip_take(async);

        AssertSql(
            """
@__p_0='1'

SELECT TOP(1) [c0].[CustomerID]
FROM (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'A%'
    ORDER BY [c].[CustomerID]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_0 ROWS ONLY
) AS [c0]
ORDER BY [c0].[CustomerID]
""",
            //
            """
@__p_0='1'

SELECT [o3].[OrderID], [o3].[CustomerID], [o3].[EmployeeID], [o3].[OrderDate], [c1].[CustomerID]
FROM (
    SELECT TOP(1) [c0].[CustomerID]
    FROM (
        SELECT [c].[CustomerID]
        FROM [Customers] AS [c]
        WHERE [c].[CustomerID] LIKE N'A%'
        ORDER BY [c].[CustomerID]
        OFFSET @__p_0 ROWS FETCH NEXT @__p_0 ROWS ONLY
    ) AS [c0]
    ORDER BY [c0].[CustomerID]
) AS [c1]
INNER JOIN [Orders] AS [o3] ON [c1].[CustomerID] = [o3].[CustomerID]
ORDER BY [c1].[CustomerID], [o3].[OrderID]
""",
            //
            """
@__p_0='1'

SELECT [o4].[OrderID], [o4].[ProductID], [o4].[Discount], [o4].[Quantity], [o4].[UnitPrice], [c1].[CustomerID], [o3].[OrderID]
FROM (
    SELECT TOP(1) [c0].[CustomerID]
    FROM (
        SELECT [c].[CustomerID]
        FROM [Customers] AS [c]
        WHERE [c].[CustomerID] LIKE N'A%'
        ORDER BY [c].[CustomerID]
        OFFSET @__p_0 ROWS FETCH NEXT @__p_0 ROWS ONLY
    ) AS [c0]
    ORDER BY [c0].[CustomerID]
) AS [c1]
INNER JOIN [Orders] AS [o3] ON [c1].[CustomerID] = [o3].[CustomerID]
INNER JOIN [Order Details] AS [o4] ON [o3].[OrderID] = [o4].[OrderID]
ORDER BY [c1].[CustomerID], [o3].[OrderID]
""");
    }

    public override async Task Filtered_include_with_multiple_ordering(bool async)
    {
        await base.Filtered_include_with_multiple_ordering(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID]
""",
            //
            """
SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID]
FROM [Customers] AS [c]
CROSS APPLY (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]
    OFFSET 1 ROWS
) AS [o0]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID], [o0].[OrderDate] DESC
""");
    }

    public override async Task Outer_identifier_correctly_determined_when_doing_include_on_right_side_of_left_join(bool async)
    {
        await base.Outer_identifier_correctly_determined_when_doing_include_on_right_side_of_left_join(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[City] = N'Seattle'
ORDER BY [c].[CustomerID], [o].[OrderID]
""",
            //
            """
SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [c].[CustomerID], [o].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
INNER JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE [c].[City] = N'Seattle'
ORDER BY [c].[CustomerID], [o].[OrderID]
""");
    }

    public override async Task Include_in_let_followed_by_FirstOrDefault(bool async)
    {
        await base.Include_in_let_followed_by_FirstOrDefault(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM (
        SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], ROW_NUMBER() OVER(PARTITION BY [o].[CustomerID] ORDER BY [o].[OrderDate]) AS [row]
        FROM [Orders] AS [o]
    ) AS [o0]
    WHERE [o0].[row] <= 1
) AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID], [o1].[OrderID]
""",
            //
            """
SELECT [o4].[OrderID], [o4].[ProductID], [o4].[Discount], [o4].[Quantity], [o4].[UnitPrice], [c].[CustomerID], [o6].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o5].[OrderID], [o5].[CustomerID]
    FROM (
        SELECT [o3].[OrderID], [o3].[CustomerID], ROW_NUMBER() OVER(PARTITION BY [o3].[CustomerID] ORDER BY [o3].[OrderDate]) AS [row]
        FROM [Orders] AS [o3]
    ) AS [o5]
    WHERE [o5].[row] <= 1
) AS [o6] ON [c].[CustomerID] = [o6].[CustomerID]
INNER JOIN [Order Details] AS [o4] ON [o6].[OrderID] = [o4].[OrderID]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID], [o6].[OrderID]
""");
    }

    public override async Task Include_multiple_references_and_collection_multi_level_reverse(bool async)
    {
        await base.Include_multiple_references_and_collection_multi_level_reverse(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Order Details] AS [o]
INNER JOIN [Products] AS [p] ON [o].[ProductID] = [p].[ProductID]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] % 23 = 13
ORDER BY [o].[OrderID], [o].[ProductID], [p].[ProductID], [o0].[OrderID], [c].[CustomerID]
""",
            //
            """
SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [o].[OrderID], [o].[ProductID], [p].[ProductID], [o0].[OrderID], [c].[CustomerID]
FROM [Order Details] AS [o]
INNER JOIN [Products] AS [p] ON [o].[ProductID] = [p].[ProductID]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
INNER JOIN [Orders] AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
WHERE [o].[OrderID] % 23 = 13
ORDER BY [o].[OrderID], [o].[ProductID], [p].[ProductID], [o0].[OrderID], [c].[CustomerID]
""");
    }

    public override async Task Include_collection_force_alias_uniquefication(bool async)
    {
        await base.Include_collection_force_alias_uniquefication(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]
""",
            //
            """
SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [o].[OrderID]
FROM [Orders] AS [o]
INNER JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]
""");
    }

    public override async Task Include_with_cycle_does_not_throw_when_AsNoTrackingWithIdentityResolution(bool async)
    {
        await base.Include_with_cycle_does_not_throw_when_AsNoTrackingWithIdentityResolution(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] < 10800
ORDER BY [o].[OrderID], [c].[CustomerID]
""",
            //
            """
SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [o].[OrderID], [c].[CustomerID]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
INNER JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE [o].[OrderID] < 10800
ORDER BY [o].[OrderID], [c].[CustomerID]
""");
    }

    public override async Task Include_reference_when_entity_in_projection(bool async)
    {
        await base.Include_reference_when_entity_in_projection(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Include_closes_reader(bool async)
    {
        await base.Include_closes_reader(async);

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""",
            //
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c0].[CustomerID]
FROM (
    SELECT TOP(1) [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [c0]
INNER JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [c0].[CustomerID]
""",
            //
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
""");
    }

    public override async Task Include_collection_then_reference(bool async)
    {
        await base.Include_collection_then_reference(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[ProductID] % 17 = 5
ORDER BY [p].[ProductID]
""",
            //
            """
SELECT [s].[OrderID], [s].[ProductID], [s].[Discount], [s].[Quantity], [s].[UnitPrice], [s].[OrderID0], [s].[CustomerID], [s].[EmployeeID], [s].[OrderDate], [p].[ProductID]
FROM [Products] AS [p]
INNER JOIN (
    SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID] AS [OrderID0], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Order Details] AS [o]
    INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
) AS [s] ON [p].[ProductID] = [s].[ProductID]
WHERE [p].[ProductID] % 17 = 5
ORDER BY [p].[ProductID]
""");
    }

    public override async Task Include_multiple_references_then_include_collection_multi_level(bool async)
    {
        await base.Include_multiple_references_then_include_collection_multi_level(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
INNER JOIN [Products] AS [p] ON [o].[ProductID] = [p].[ProductID]
WHERE [o].[OrderID] % 23 = 13
ORDER BY [o].[OrderID], [o].[ProductID], [o0].[OrderID], [c].[CustomerID], [p].[ProductID]
""",
            //
            """
SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [o].[OrderID], [o].[ProductID], [o0].[OrderID], [c].[CustomerID], [p].[ProductID]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
INNER JOIN [Products] AS [p] ON [o].[ProductID] = [p].[ProductID]
INNER JOIN [Orders] AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
WHERE [o].[OrderID] % 23 = 13
ORDER BY [o].[OrderID], [o].[ProductID], [o0].[OrderID], [c].[CustomerID], [p].[ProductID]
""");
    }

    public override async Task Include_collection_orderby_take(bool async)
    {
        await base.Include_collection_orderby_take(async);

        AssertSql(
            """
@__p_0='5'

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""",
            //
            """
@__p_0='5'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c0].[CustomerID]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c0]
INNER JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [c0].[CustomerID]
""");
    }

    public override async Task Include_references_and_collection_multi_level_predicate(bool async)
    {
        await base.Include_references_and_collection_multi_level_predicate(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] = 10248
ORDER BY [o].[OrderID], [o].[ProductID], [o0].[OrderID], [c].[CustomerID]
""",
            //
            """
SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [o].[OrderID], [o].[ProductID], [o0].[OrderID], [c].[CustomerID]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
INNER JOIN [Orders] AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
WHERE [o].[OrderID] = 10248
ORDER BY [o].[OrderID], [o].[ProductID], [o0].[OrderID], [c].[CustomerID]
""");
    }

    public override async Task Include_references_then_include_collection_multi_level_predicate(bool async)
    {
        await base.Include_references_then_include_collection_multi_level_predicate(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] = 10248
ORDER BY [o].[OrderID], [o].[ProductID], [o0].[OrderID], [c].[CustomerID]
""",
            //
            """
SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [o].[OrderID], [o].[ProductID], [o0].[OrderID], [c].[CustomerID]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
INNER JOIN [Orders] AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
WHERE [o].[OrderID] = 10248
ORDER BY [o].[OrderID], [o].[ProductID], [o0].[OrderID], [c].[CustomerID]
""");
    }

    public override async Task Include_multiple_references_then_include_multi_level(bool async)
    {
        await base.Include_multiple_references_then_include_multi_level(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
INNER JOIN [Products] AS [p] ON [o].[ProductID] = [p].[ProductID]
WHERE [o].[OrderID] % 23 = 13
""");
    }

    public override async Task Include_empty_reference_sets_IsLoaded(bool async)
    {
        await base.Include_empty_reference_sets_IsLoaded(async);

        AssertSql(
            """
SELECT TOP(1) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title], [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title]
FROM [Employees] AS [e]
LEFT JOIN [Employees] AS [e0] ON [e].[ReportsTo] = [e0].[EmployeeID]
WHERE [e0].[EmployeeID] IS NULL
""");
    }

    public override async Task Include_reference_and_collection_order_by(bool async)
    {
        await base.Include_reference_and_collection_order_by(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[CustomerID] LIKE N'F%'
ORDER BY [o].[OrderID], [c].[CustomerID]
""",
            //
            """
SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [o].[OrderID], [c].[CustomerID]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
INNER JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE [o].[CustomerID] LIKE N'F%'
ORDER BY [o].[OrderID], [c].[CustomerID]
""");
    }

    public override async Task Include_multiple_references_then_include_collection_multi_level_reverse(bool async)
    {
        await base.Include_multiple_references_then_include_collection_multi_level_reverse(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Order Details] AS [o]
INNER JOIN [Products] AS [p] ON [o].[ProductID] = [p].[ProductID]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] % 23 = 13
ORDER BY [o].[OrderID], [o].[ProductID], [p].[ProductID], [o0].[OrderID], [c].[CustomerID]
""",
            //
            """
SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [o].[OrderID], [o].[ProductID], [p].[ProductID], [o0].[OrderID], [c].[CustomerID]
FROM [Order Details] AS [o]
INNER JOIN [Products] AS [p] ON [o].[ProductID] = [p].[ProductID]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
INNER JOIN [Orders] AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
WHERE [o].[OrderID] % 23 = 13
ORDER BY [o].[OrderID], [o].[ProductID], [p].[ProductID], [o0].[OrderID], [c].[CustomerID]
""");
    }

    public override async Task Include_references_then_include_collection_multi_level(bool async)
    {
        await base.Include_references_then_include_collection_multi_level(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
WHERE [o].[ProductID] % 23 = 17 AND [o].[Quantity] < CAST(10 AS smallint)
ORDER BY [o].[OrderID], [o].[ProductID], [o0].[OrderID], [c].[CustomerID]
""",
            //
            """
SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [o].[OrderID], [o].[ProductID], [o0].[OrderID], [c].[CustomerID]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
INNER JOIN [Orders] AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
WHERE [o].[ProductID] % 23 = 17 AND [o].[Quantity] < CAST(10 AS smallint)
ORDER BY [o].[OrderID], [o].[ProductID], [o0].[OrderID], [c].[CustomerID]
""");
    }

    public override async Task Include_collection_and_reference(bool async)
    {
        await base.Include_collection_and_reference(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[CustomerID] LIKE N'F%'
ORDER BY [o].[OrderID], [c].[CustomerID]
""",
            //
            """
SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [o].[OrderID], [c].[CustomerID]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
INNER JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE [o].[CustomerID] LIKE N'F%'
ORDER BY [o].[OrderID], [c].[CustomerID]
""");
    }

    public override async Task Include_collection_then_include_collection_predicate(bool async)
    {
        await base.Include_collection_then_include_collection_predicate(async);

        AssertSql(
            """
SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]
""",
            //
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c0].[CustomerID]
FROM (
    SELECT TOP(1) [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'ALFKI'
) AS [c0]
INNER JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [c0].[CustomerID], [o].[OrderID]
""",
            //
            """
SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [c0].[CustomerID], [o].[OrderID]
FROM (
    SELECT TOP(1) [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'ALFKI'
) AS [c0]
INNER JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
INNER JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
ORDER BY [c0].[CustomerID], [o].[OrderID]
""");
    }

    public override async Task Include_references_then_include_multi_level(bool async)
    {
        await base.Include_references_then_include_multi_level(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] % 23 = 13
""");
    }

    public override async Task Include_with_cycle_does_not_throw_when_AsTracking_NoTrackingWithIdentityResolution(bool async)
    {
        await base.Include_with_cycle_does_not_throw_when_AsTracking_NoTrackingWithIdentityResolution(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] < 10800
ORDER BY [o].[OrderID], [c].[CustomerID]
""",
            //
            """
SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [o].[OrderID], [c].[CustomerID]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
INNER JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE [o].[OrderID] < 10800
ORDER BY [o].[OrderID], [c].[CustomerID]
""");
    }

    public override async Task Include_references_then_include_collection(bool async)
    {
        await base.Include_references_then_include_collection(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[CustomerID] LIKE N'F%'
ORDER BY [o].[OrderID], [c].[CustomerID]
""",
            //
            """
SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [o].[OrderID], [c].[CustomerID]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
INNER JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE [o].[CustomerID] LIKE N'F%'
ORDER BY [o].[OrderID], [c].[CustomerID]
""");
    }

    public override async Task Include_collection_single_or_default_no_result(bool async)
    {
        await base.Include_collection_single_or_default_no_result(async);

        AssertSql(
            """
SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI ?'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Include_multiple_references_then_include_multi_level_reverse(bool async)
    {
        await base.Include_multiple_references_then_include_multi_level_reverse(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Order Details] AS [o]
INNER JOIN [Products] AS [p] ON [o].[ProductID] = [p].[ProductID]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] % 23 = 13
""");
    }

    public override async Task Include_multiple_references_and_collection_multi_level(bool async)
    {
        await base.Include_multiple_references_and_collection_multi_level(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
INNER JOIN [Products] AS [p] ON [o].[ProductID] = [p].[ProductID]
WHERE [o].[OrderID] % 23 = 13
ORDER BY [o].[OrderID], [o].[ProductID], [o0].[OrderID], [c].[CustomerID], [p].[ProductID]
""",
            //
            """
SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [o].[OrderID], [o].[ProductID], [o0].[OrderID], [c].[CustomerID], [p].[ProductID]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
INNER JOIN [Products] AS [p] ON [o].[ProductID] = [p].[ProductID]
INNER JOIN [Orders] AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
WHERE [o].[OrderID] % 23 = 13
ORDER BY [o].[OrderID], [o].[ProductID], [o0].[OrderID], [c].[CustomerID], [p].[ProductID]
""");
    }

    public override async Task Repro9735(bool async)
    {
        await base.Repro9735(async);

        AssertSql(
            """
@__p_0='2'

SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY CASE
    WHEN [c].[CustomerID] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, CASE
    WHEN [c].[CustomerID] IS NOT NULL THEN [c].[CustomerID]
    ELSE N''
END, [o].[OrderID], [c].[CustomerID]
""",
            //
            """
@__p_0='2'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [s].[OrderID], [s].[CustomerID]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [c].[CustomerID], CASE
        WHEN [c].[CustomerID] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [c], CASE
        WHEN [c].[CustomerID] IS NOT NULL THEN [c].[CustomerID]
        ELSE N''
    END AS [c0]
    FROM [Orders] AS [o]
    LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
    ORDER BY CASE
        WHEN [c].[CustomerID] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END, CASE
        WHEN [c].[CustomerID] IS NOT NULL THEN [c].[CustomerID]
        ELSE N''
    END
) AS [s]
INNER JOIN [Order Details] AS [o0] ON [s].[OrderID] = [o0].[OrderID]
ORDER BY [s].[c], [s].[c0], [s].[OrderID], [s].[CustomerID]
""");
    }

    public override async Task Include_reference_single_or_default_when_no_result(bool async)
    {
        await base.Include_reference_single_or_default_when_no_result(async);

        AssertSql(
            """
SELECT TOP(2) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] = -1
""");
    }

    public override async Task Include_multi_level_reference_then_include_collection_predicate(bool async)
    {
        await base.Include_multi_level_reference_then_include_collection_predicate(async);

        AssertSql(
            """
SELECT TOP(2) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] = 10248
ORDER BY [o].[OrderID], [c].[CustomerID]
""",
            //
            """
SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [s].[OrderID], [s].[CustomerID]
FROM (
    SELECT TOP(1) [o].[OrderID], [c].[CustomerID]
    FROM [Orders] AS [o]
    LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
    WHERE [o].[OrderID] = 10248
) AS [s]
INNER JOIN [Orders] AS [o0] ON [s].[CustomerID] = [o0].[CustomerID]
ORDER BY [s].[OrderID], [s].[CustomerID]
""");
    }

    public override async Task Include_collection_with_last_no_orderby(bool async)
    {
        await base.Include_collection_with_last_no_orderby(async);

        AssertSql();
    }

    public override async Task Include_property_after_navigation(bool async)
    {
        await base.Include_property_after_navigation(async);

        AssertSql();
    }

    public override async Task Include_property(bool async)
    {
        await base.Include_property(async);

        AssertSql();
    }

    public override async Task Include_property_expression_invalid(bool async)
    {
        await base.Include_property_expression_invalid(async);

        AssertSql();
    }

    public override async Task Then_include_property_expression_invalid(bool async)
    {
        await base.Then_include_property_expression_invalid(async);

        AssertSql();
    }

    public override async Task Include_collection_with_client_filter(bool async)
    {
        await base.Include_collection_with_client_filter(async);

        AssertSql();
    }

    public override async Task Include_specified_on_non_entity_not_supported(bool async)
    {
        await base.Include_specified_on_non_entity_not_supported(async);

        AssertSql();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
