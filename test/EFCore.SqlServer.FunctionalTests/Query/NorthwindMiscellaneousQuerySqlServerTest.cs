// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit.Sdk;

// ReSharper disable UnusedParameter.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

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

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Shaper_command_caching_when_parameter_names_different(bool async)
    {
        await base.Shaper_command_caching_when_parameter_names_different(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""",
            //
            """
SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task Can_convert_manually_build_expression_with_default(bool async)
    {
        await base.Can_convert_manually_build_expression_with_default(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[City] IS NOT NULL
""",
            //
            """
SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[City] IS NOT NULL
""");
    }

    public override async Task Lifting_when_subquery_nested_order_by_anonymous(bool async)
    {
        await base.Lifting_when_subquery_nested_order_by_anonymous(async);

        AssertSql(
            """
@__p_0='2'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT [c1].[CustomerID]
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID]
        FROM [Customers] AS [c]
        ORDER BY [c].[CustomerID]
    ) AS [c1]
    CROSS JOIN [Customers] AS [c0]
) AS [s] ON [o].[CustomerID] = [s].[CustomerID]
ORDER BY [s].[CustomerID]
""");
    }

    public override async Task Lifting_when_subquery_nested_order_by_simple(bool async)
    {
        await base.Lifting_when_subquery_nested_order_by_simple(async);

        AssertSql(
            """
@__p_0='2'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT [c1].[CustomerID]
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID]
        FROM [Customers] AS [c]
        ORDER BY [c].[CustomerID]
    ) AS [c1]
    CROSS JOIN [Customers] AS [c0]
) AS [s] ON [o].[CustomerID] = [s].[CustomerID]
ORDER BY [s].[CustomerID]
""");
    }

    private static T Scoper<T>(Func<T> getter)
        => getter();

    public override async Task Local_dictionary(bool async)
    {
        await base.Local_dictionary(async);

        AssertSql(
            """
@__p_0='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__p_0
""");
    }

    public override async Task Entity_equality_self(bool async)
    {
        await base.Entity_equality_self(async);

        AssertSql(
            """
SELECT [c].[CustomerID]
FROM [Customers] AS [c]
""");
    }

    public override async Task Entity_equality_local(bool async)
    {
        await base.Entity_equality_local(async);

        AssertSql(
            """
@__entity_equality_local_0_CustomerID='ANATR' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__entity_equality_local_0_CustomerID
""");
    }

    public override async Task Entity_equality_local_composite_key(bool async)
    {
        await base.Entity_equality_local_composite_key(async);

        AssertSql(
            """
@__entity_equality_local_0_OrderID='10248' (Nullable = true)
@__entity_equality_local_0_ProductID='11' (Nullable = true)

SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE [o].[OrderID] = @__entity_equality_local_0_OrderID AND [o].[ProductID] = @__entity_equality_local_0_ProductID
""");
    }

    public override async Task Entity_equality_local_double_check(bool async)
    {
        await base.Entity_equality_local_double_check(async);

        AssertSql(
            """
@__entity_equality_local_0_CustomerID='ANATR' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__entity_equality_local_0_CustomerID AND @__entity_equality_local_0_CustomerID = [c].[CustomerID]
""");
    }

    public override async Task Join_with_entity_equality_local_on_both_sources(bool async)
    {
        await base.Join_with_entity_equality_local_on_both_sources(async);

        AssertSql(
            """
@__entity_equality_local_0_CustomerID='ANATR' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = @__entity_equality_local_0_CustomerID
) AS [c1] ON [c].[CustomerID] = [c1].[CustomerID]
WHERE [c].[CustomerID] = @__entity_equality_local_0_CustomerID
""");
    }

    public override async Task Entity_equality_local_inline(bool async)
    {
        await base.Entity_equality_local_inline(async);

        AssertSql(
            """
SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ANATR'
""");
    }

    public override async Task Entity_equality_local_inline_composite_key(bool async)
    {
        await base.Entity_equality_local_inline_composite_key(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE [o].[OrderID] = 10248 AND [o].[ProductID] = 11
""");
    }

    public override async Task Entity_equality_null(bool async)
    {
        await base.Entity_equality_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE 0 = 1
""");
    }

    public override async Task Entity_equality_null_composite_key(bool async)
    {
        await base.Entity_equality_null_composite_key(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE 0 = 1
""");
    }

    public override async Task Entity_equality_not_null(bool async)
    {
        await base.Entity_equality_not_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID]
FROM [Customers] AS [c]
""");
    }

    public override async Task Entity_equality_not_null_composite_key(bool async)
    {
        await base.Entity_equality_not_null_composite_key(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
""");
    }

    public override async Task Entity_equality_through_nested_anonymous_type_projection(bool async)
    {
        await base.Entity_equality_through_nested_anonymous_type_projection(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[CustomerID] IS NOT NULL
""");
    }

    public override async Task Entity_equality_through_DTO_projection(bool async)
    {
        await base.Entity_equality_through_DTO_projection(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[CustomerID] IS NOT NULL
""");
    }

    public override async Task Entity_equality_through_subquery(bool async)
    {
        await base.Entity_equality_through_subquery(async);

        AssertSql(
            """
SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
""");
    }

    public override async Task Entity_equality_through_include(bool async)
    {
        await base.Entity_equality_through_include(async);

        AssertSql(
            """
SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE 0 = 1
""");
    }

    public override async Task Entity_equality_orderby(bool async)
    {
        await base.Entity_equality_orderby(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Entity_equality_orderby_descending_composite_key(bool async)
    {
        await base.Entity_equality_orderby_descending_composite_key(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
ORDER BY [o].[OrderID] DESC, [o].[ProductID] DESC
""");
    }

    public override async Task Entity_equality_orderby_subquery(bool async)
    {
        await base.Entity_equality_orderby_subquery(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY (
    SELECT TOP(1) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
""");
    }

    public override async Task Entity_equality_orderby_descending_subquery_composite_key(bool async)
    {
        await base.Entity_equality_orderby_descending_subquery_composite_key(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
ORDER BY (
    SELECT TOP(1) [o0].[OrderID]
    FROM [Order Details] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID]) DESC, (
    SELECT TOP(1) [o1].[ProductID]
    FROM [Order Details] AS [o1]
    WHERE [o].[OrderID] = [o1].[OrderID]) DESC
""");
    }

    public override async Task Default_if_empty_top_level(bool async)
    {
        await base.Default_if_empty_top_level(async);

        AssertSql(
            """
SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
FROM (
    SELECT 1 AS empty
) AS [e0]
LEFT JOIN (
    SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
    WHERE [e].[EmployeeID] = -1
) AS [e1] ON 1 = 1
""");
    }

    public override async Task Join_with_default_if_empty_on_both_sources(bool async)
    {
        await base.Join_with_default_if_empty_on_both_sources(async);

        AssertSql(
            """
SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
FROM (
    SELECT 1 AS empty
) AS [e0]
LEFT JOIN (
    SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
    WHERE [e].[EmployeeID] = -1
) AS [e1] ON 1 = 1
INNER JOIN (
    SELECT [e4].[EmployeeID]
    FROM (
        SELECT 1 AS empty
    ) AS [e3]
    LEFT JOIN (
        SELECT [e2].[EmployeeID]
        FROM [Employees] AS [e2]
        WHERE [e2].[EmployeeID] = -1
    ) AS [e4] ON 1 = 1
) AS [s] ON [e1].[EmployeeID] = [s].[EmployeeID]
""");
    }

    public override async Task Default_if_empty_top_level_followed_by_projecting_constant(bool async)
    {
        await base.Default_if_empty_top_level_followed_by_projecting_constant(async);

        AssertSql(
            """
SELECT N'Foo'
FROM (
    SELECT 1 AS empty
) AS [e0]
LEFT JOIN (
    SELECT 1 AS empty
    FROM [Employees] AS [e]
    WHERE [e].[EmployeeID] = -1
) AS [e1] ON 1 = 1
""");
    }

    public override async Task Default_if_empty_top_level_positive(bool async)
    {
        await base.Default_if_empty_top_level_positive(async);

        AssertSql(
            """
SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
FROM (
    SELECT 1 AS empty
) AS [e0]
LEFT JOIN (
    SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
    WHERE [e].[EmployeeID] > 0
) AS [e1] ON 1 = 1
""");
    }

    public override async Task Default_if_empty_top_level_projection(bool async)
    {
        await base.Default_if_empty_top_level_projection(async);

        AssertSql(
            """
SELECT COALESCE([e1].[EmployeeID], 0)
FROM (
    SELECT 1 AS empty
) AS [e0]
LEFT JOIN (
    SELECT [e].[EmployeeID]
    FROM [Employees] AS [e]
    WHERE [e].[EmployeeID] = -1
) AS [e1] ON 1 = 1
""");
    }

    public override async Task Where_query_composition(bool async)
    {
        await base.Where_query_composition(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[FirstName] = (
    SELECT TOP(1) [e0].[FirstName]
    FROM [Employees] AS [e0]
    ORDER BY [e0].[EmployeeID])
""");
    }

    public override async Task Where_query_composition_is_null(bool async)
    {
        await base.Where_query_composition_is_null(async);

        AssertSql(
            """
@__p_0='3'

SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
    ORDER BY [e].[EmployeeID]
) AS [e1]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Employees] AS [e0]
    WHERE [e0].[EmployeeID] = [e1].[ReportsTo])
ORDER BY [e1].[EmployeeID]
""");
    }

    public override async Task Where_query_composition_is_not_null(bool async)
    {
        await base.Where_query_composition_is_not_null(async);

        AssertSql(
            """
@__p_0='4'
@__p_1='3'

SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
FROM (
    SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
    ORDER BY [e].[EmployeeID]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [e1]
WHERE EXISTS (
    SELECT 1
    FROM [Employees] AS [e0]
    WHERE [e0].[EmployeeID] = [e1].[ReportsTo])
ORDER BY [e1].[EmployeeID]
""");
    }

    public override async Task Where_query_composition_entity_equality_one_element_SingleOrDefault(bool async)
    {
        await base.Where_query_composition_entity_equality_one_element_SingleOrDefault(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE [e0].[EmployeeID] = [e].[ReportsTo]) = 0
""");
    }

    public override async Task Where_query_composition_entity_equality_one_element_Single(bool async)
    {
        await base.Where_query_composition_entity_equality_one_element_Single(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE [e0].[EmployeeID] = [e].[ReportsTo]) = 0
""");
    }

    public override async Task Where_query_composition_entity_equality_one_element_FirstOrDefault(bool async)
    {
        await base.Where_query_composition_entity_equality_one_element_FirstOrDefault(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE [e0].[EmployeeID] = [e].[ReportsTo]) = 0
""");
    }

    public override async Task Where_query_composition_entity_equality_one_element_First(bool async)
    {
        await base.Where_query_composition_entity_equality_one_element_First(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE [e0].[EmployeeID] = [e].[ReportsTo]) = 0
""");
    }

    public override async Task Where_query_composition_entity_equality_no_elements_SingleOrDefault(bool async)
    {
        await base.Where_query_composition_entity_equality_no_elements_SingleOrDefault(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE [e0].[EmployeeID] = 42) = 0
""");
    }

    public override async Task Where_query_composition_entity_equality_no_elements_Single(bool async)
    {
        await base.Where_query_composition_entity_equality_no_elements_Single(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE [e0].[EmployeeID] = 42) = 0
""");
    }

    public override async Task Where_query_composition_entity_equality_no_elements_FirstOrDefault(bool async)
    {
        await base.Where_query_composition_entity_equality_no_elements_FirstOrDefault(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE [e0].[EmployeeID] = 42) = 0
""");
    }

    public override async Task Where_query_composition_entity_equality_no_elements_First(bool async)
    {
        await base.Where_query_composition_entity_equality_no_elements_First(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE [e0].[EmployeeID] = 42) = 0
""");
    }

    public override async Task Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(bool async)
    {
        await base.Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE [e0].[EmployeeID] <> [e].[ReportsTo] OR [e].[ReportsTo] IS NULL
    ORDER BY [e0].[EmployeeID]) = 1
""");
    }

    public override async Task Where_query_composition_entity_equality_multiple_elements_Single(bool async)
    {
        await base.Where_query_composition_entity_equality_multiple_elements_Single(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE [e0].[EmployeeID] <> [e].[ReportsTo] OR [e].[ReportsTo] IS NULL) = 0
""");
    }

    public override async Task Where_query_composition_entity_equality_multiple_elements_FirstOrDefault(bool async)
    {
        await base.Where_query_composition_entity_equality_multiple_elements_FirstOrDefault(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE [e0].[EmployeeID] <> [e].[ReportsTo] OR [e].[ReportsTo] IS NULL) = 0
""");
    }

    public override async Task Where_query_composition_entity_equality_multiple_elements_First(bool async)
    {
        await base.Where_query_composition_entity_equality_multiple_elements_First(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE [e0].[EmployeeID] <> [e].[ReportsTo] OR [e].[ReportsTo] IS NULL) = 0
""");
    }

    public override async Task Where_query_composition2(bool async)
    {
        await base.Where_query_composition2(async);

        AssertSql(
            """
@__p_0='3'

SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
) AS [e1]
WHERE [e1].[FirstName] = (
    SELECT TOP(1) [e0].[FirstName]
    FROM [Employees] AS [e0]
    ORDER BY [e0].[EmployeeID])
""");
    }

    public override async Task Where_query_composition2_FirstOrDefault(bool async)
    {
        await base.Where_query_composition2_FirstOrDefault(async);

        AssertSql(
            """
@__p_0='3'

SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
) AS [e1]
WHERE [e1].[FirstName] = (
    SELECT TOP(1) [e0].[FirstName]
    FROM [Employees] AS [e0]
    ORDER BY [e0].[EmployeeID])
""");
    }

    public override async Task Where_query_composition2_FirstOrDefault_with_anonymous(bool async)
    {
        await base.Where_query_composition2_FirstOrDefault_with_anonymous(async);

        AssertSql(
            """
@__p_0='3'

SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
) AS [e1]
WHERE [e1].[FirstName] = (
    SELECT TOP(1) [e0].[FirstName]
    FROM [Employees] AS [e0]
    ORDER BY [e0].[EmployeeID])
""");
    }

    public override async Task Select_Subquery_Single(bool async)
    {
        await base.Select_Subquery_Single(async);

        AssertSql(
            """
@__p_0='2'

SELECT [o3].[OrderID], [o3].[CustomerID], [o3].[EmployeeID], [o3].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[ProductID]
    FROM [Order Details] AS [o]
    ORDER BY [o].[ProductID], [o].[OrderID]
) AS [o1]
LEFT JOIN (
    SELECT [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate]
    FROM (
        SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], ROW_NUMBER() OVER(PARTITION BY [o0].[OrderID] ORDER BY [o0].[OrderID]) AS [row]
        FROM [Orders] AS [o0]
    ) AS [o2]
    WHERE [o2].[row] <= 1
) AS [o3] ON [o1].[OrderID] = [o3].[OrderID]
ORDER BY [o1].[ProductID], [o1].[OrderID]
""");
    }

    public override async Task Select_Where_Subquery_Deep_Single(bool async)
    {
        await base.Select_Where_Subquery_Deep_Single(async);

        AssertSql(
            """
@__p_0='2'

SELECT TOP(@__p_0) [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE [o].[OrderID] = 10344 AND (
    SELECT TOP(1) (
        SELECT TOP(1) [c].[City]
        FROM [Customers] AS [c]
        WHERE [o0].[CustomerID] = [c].[CustomerID])
    FROM [Orders] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID]) = N'Seattle'
""");
    }

    public override async Task Select_Where_Subquery_Deep_First(bool async)
    {
        await base.Select_Where_Subquery_Deep_First(async);

        AssertSql(
            """
@__p_0='2'

SELECT TOP(@__p_0) [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE (
    SELECT TOP(1) (
        SELECT TOP(1) [c].[City]
        FROM [Customers] AS [c]
        WHERE [o0].[CustomerID] = [c].[CustomerID])
    FROM [Orders] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID]) = N'Seattle'
""");
    }

    public override async Task Select_Where_Subquery_Equality(bool async)
    {
        await base.Select_Where_Subquery_Equality(async);

        AssertSql(
            """
@__p_0='1'

SELECT [o3].[OrderID], [o3].[CustomerID], [o3].[EmployeeID], [o3].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [o3]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT TOP(2) [o0].[OrderID]
        FROM [Order Details] AS [o0]
        ORDER BY [o0].[OrderID]
    ) AS [o2]
    WHERE (
        SELECT TOP(1) [c].[Country]
        FROM [Customers] AS [c]
        WHERE [c].[CustomerID] = [o3].[CustomerID]
        ORDER BY [c].[CustomerID]) = (
        SELECT TOP(1) [c0].[Country]
        FROM [Orders] AS [o1]
        INNER JOIN [Customers] AS [c0] ON [o1].[CustomerID] = [c0].[CustomerID]
        WHERE [o1].[OrderID] = [o2].[OrderID]
        ORDER BY [o1].[OrderID], [c0].[CustomerID]) OR ((
        SELECT TOP(1) [c].[Country]
        FROM [Customers] AS [c]
        WHERE [c].[CustomerID] = [o3].[CustomerID]
        ORDER BY [c].[CustomerID]) IS NULL AND (
        SELECT TOP(1) [c0].[Country]
        FROM [Orders] AS [o1]
        INNER JOIN [Customers] AS [c0] ON [o1].[CustomerID] = [c0].[CustomerID]
        WHERE [o1].[OrderID] = [o2].[OrderID]
        ORDER BY [o1].[OrderID], [c0].[CustomerID]) IS NULL)) > 0
ORDER BY [o3].[OrderID]
""");
    }

    public override async Task Where_subquery_anon(bool async)
    {
        await base.Where_subquery_anon(async);

        AssertSql(
            """
@__p_0='3'

SELECT [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
    ORDER BY [e].[EmployeeID]
) AS [e0]
CROSS JOIN (
    SELECT TOP(5) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [o0]
WHERE [e0].[EmployeeID] = [o0].[EmployeeID]
ORDER BY [e0].[EmployeeID]
""");
    }

    public override async Task Where_subquery_anon_nested(bool async)
    {
        await base.Where_subquery_anon_nested(async);

        AssertSql(
            """
@__p_0='3'

SELECT [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
    ORDER BY [e].[EmployeeID]
) AS [e0]
CROSS JOIN (
    SELECT TOP(5) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [o0]
CROSS JOIN (
    SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c0]
WHERE [e0].[City] = N'Seattle'
ORDER BY [e0].[EmployeeID]
""");
    }

    public override async Task OrderBy_SelectMany(bool async)
    {
        await base.OrderBy_SelectMany(async);

        AssertSql(
            """
SELECT [c].[ContactName], [o0].[OrderID]
FROM [Customers] AS [c]
CROSS JOIN (
    SELECT TOP(3) [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [o0]
WHERE [c].[CustomerID] = [o0].[CustomerID]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Let_any_subquery_anonymous(bool async)
    {
        await base.Let_any_subquery_anonymous(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE [o].[CustomerID] = [c].[CustomerID]) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [hasOrders]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task OrderBy_arithmetic(bool async)
    {
        await base.OrderBy_arithmetic(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
ORDER BY [e].[EmployeeID] - [e].[EmployeeID]
""");
    }

    public override async Task OrderBy_condition_comparison(bool async)
    {
        await base.OrderBy_condition_comparison(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
ORDER BY CASE
    WHEN [p].[UnitsInStock] > CAST(0 AS smallint) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [p].[ProductID]
""");
    }

    public override async Task OrderBy_ternary_conditions(bool async)
        => await base.OrderBy_ternary_conditions(async);

    // issue #18774
    //            AssertSql(
    //                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
    //FROM [Products] AS [p]
    //ORDER BY CASE
    //    WHEN (([p].[UnitsInStock] > CAST(10 AS smallint)) AND ([p].[ProductID] > 40)) OR (([p].[UnitsInStock] <= CAST(10 AS smallint)) AND ([p].[ProductID] <= 40))
    //    THEN CAST(1 AS bit) ELSE CAST(0 AS bit)
    //END, [p].[ProductID]");
    public override async Task OrderBy_any(bool async)
    {
        await base.OrderBy_any(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID] AND [o].[OrderID] > 11000) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [c].[CustomerID]
""");
    }

    public override async Task Skip(bool async)
    {
        await base.Skip(async);

        AssertSql(
            """
@__p_0='5'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
OFFSET @__p_0 ROWS
""");
    }

    public override async Task Skip_no_orderby(bool async)
    {
        await base.Skip_no_orderby(async);

        AssertSql(
            """
@__p_0='5'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY (SELECT 1)
OFFSET @__p_0 ROWS
""");
    }

    public override async Task Skip_orderby_const(bool async)
    {
        await base.Skip_orderby_const(async);

        AssertSql(
            """
@__p_0='5'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY (SELECT 1)
OFFSET @__p_0 ROWS
""");
    }

    public override async Task Skip_Take(bool async)
    {
        await base.Skip_Take(async);

        AssertSql(
            """
@__p_0='5'
@__p_1='10'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactName]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
""");
    }

    public override async Task Join_Customers_Orders_Skip_Take(bool async)
    {
        await base.Join_Customers_Orders_Skip_Take(async);

        AssertSql(
            """
@__p_0='10'
@__p_1='5'

SELECT [c].[ContactName], [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [o].[OrderID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
""");
    }

    public override async Task Join_Customers_Orders_Skip_Take_followed_by_constant_projection(bool async)
    {
        await base.Join_Customers_Orders_Skip_Take_followed_by_constant_projection(async);

        AssertSql(
            """
@__p_0='10'
@__p_1='5'

SELECT N'Foo'
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [o].[OrderID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
""");
    }

    public override async Task Join_Customers_Orders_Projection_With_String_Concat_Skip_Take(bool async)
    {
        await base.Join_Customers_Orders_Projection_With_String_Concat_Skip_Take(async);

        AssertSql(
            """
@__p_0='10'
@__p_1='5'

SELECT COALESCE([c].[ContactName], N'') + N' ' + COALESCE([c].[ContactTitle], N'') AS [Contact], [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [o].[OrderID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
""");
    }

    public override async Task Join_Customers_Orders_Orders_Skip_Take_Same_Properties(bool async)
    {
        await base.Join_Customers_Orders_Orders_Skip_Take_Same_Properties(async);

        AssertSql(
            """
@__p_0='10'
@__p_1='5'

SELECT [o].[OrderID], [c].[CustomerID] AS [CustomerIDA], [c0].[CustomerID] AS [CustomerIDB], [c].[ContactName] AS [ContactNameA], [c0].[ContactName] AS [ContactNameB]
FROM [Orders] AS [o]
INNER JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
INNER JOIN [Customers] AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
ORDER BY [o].[OrderID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
""");
    }

    public override async Task Ternary_should_not_evaluate_both_sides(bool async)
    {
        await base.Ternary_should_not_evaluate_both_sides(async);

        AssertSql(
            """
SELECT [c].[CustomerID], N'none' AS [Data1]
FROM [Customers] AS [c]
""");
    }

    public override async Task Ternary_should_not_evaluate_both_sides_with_parameter(bool async)
    {
        await base.Ternary_should_not_evaluate_both_sides_with_parameter(async);

        AssertSql(
            """
SELECT CAST(1 AS bit) AS [Data1]
FROM [Orders] AS [o]
""");
    }

    public override async Task Take_Skip(bool async)
    {
        await base.Take_Skip(async);

        AssertSql(
            """
@__p_0='10'
@__p_1='5'

SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactName]
) AS [c0]
ORDER BY [c0].[ContactName]
OFFSET @__p_1 ROWS
""");
    }

    public override async Task Take_Skip_Distinct(bool async)
    {
        await base.Take_Skip_Distinct(async);

        AssertSql(
            """
@__p_0='10'
@__p_1='5'

SELECT DISTINCT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
FROM (
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactName]
    ) AS [c0]
    ORDER BY [c0].[ContactName]
    OFFSET @__p_1 ROWS
) AS [c1]
""");
    }

    public override async Task Take_Skip_Distinct_Caching(bool async)
    {
        await base.Take_Skip_Distinct_Caching(async);

        AssertSql(
            """
@__p_0='10'
@__p_1='5'

SELECT DISTINCT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
FROM (
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactName]
    ) AS [c0]
    ORDER BY [c0].[ContactName]
    OFFSET @__p_1 ROWS
) AS [c1]
""",
            //
            """
@__p_0='15'
@__p_1='10'

SELECT DISTINCT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
FROM (
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactName]
    ) AS [c0]
    ORDER BY [c0].[ContactName]
    OFFSET @__p_1 ROWS
) AS [c1]
""");
    }

    public override async Task Take_Distinct_Count(bool async)
    {
        await base.Take_Distinct_Count(async);

        AssertSql(
            """
@__p_0='5'

SELECT COUNT(*)
FROM (
    SELECT DISTINCT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM (
        SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
        FROM [Orders] AS [o]
    ) AS [o0]
) AS [o1]
""");
    }

    public override async Task Take_Where_Distinct_Count(bool async)
    {
        await base.Take_Where_Distinct_Count(async);

        AssertSql(
            """
@__p_0='5'

SELECT COUNT(*)
FROM (
    SELECT DISTINCT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM (
        SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [o].[CustomerID] = N'FRANK'
    ) AS [o0]
) AS [o1]
""");
    }

    public override async Task Queryable_simple(bool async)
    {
        await base.Queryable_simple(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task Queryable_simple_anonymous(bool async)
    {
        await base.Queryable_simple_anonymous(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task Queryable_nested_simple(bool async)
    {
        await base.Queryable_nested_simple(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task Queryable_simple_anonymous_projection_subquery(bool async)
    {
        await base.Queryable_simple_anonymous_projection_subquery(async);

        AssertSql(
            """
@__p_0='91'

SELECT TOP(@__p_0) [c].[City]
FROM [Customers] AS [c]
""");
    }

    public override async Task Queryable_simple_anonymous_subquery(bool async)
    {
        await base.Queryable_simple_anonymous_subquery(async);

        AssertSql(
            """
@__p_0='91'

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task Take_simple(bool async)
    {
        await base.Take_simple(async);

        AssertSql(
            """
@__p_0='10'

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Take_simple_parameterized(bool async)
    {
        await base.Take_simple_parameterized(async);

        AssertSql(
            """
@__p_0='10'

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Take_simple_projection(bool async)
    {
        await base.Take_simple_projection(async);

        AssertSql(
            """
@__p_0='10'

SELECT TOP(@__p_0) [c].[City]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Take_subquery_projection(bool async)
    {
        await base.Take_subquery_projection(async);

        AssertSql(
            """
@__p_0='2'

SELECT TOP(@__p_0) [c].[City]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task OrderBy_Take_Count(bool async)
    {
        await base.OrderBy_Take_Count(async);

        AssertSql(
            """
@__p_0='5'

SELECT COUNT(*)
FROM (
    SELECT TOP(@__p_0) 1 AS empty
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [o0]
""");
    }

    public override async Task Take_OrderBy_Count(bool async)
    {
        await base.Take_OrderBy_Count(async);

        AssertSql(
            """
@__p_0='5'

SELECT COUNT(*)
FROM (
    SELECT TOP(@__p_0) 1 AS empty
    FROM [Orders] AS [o]
) AS [o0]
""");
    }

    public override async Task Any_simple(bool async)
    {
        await base.Any_simple(async);

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

    public override async Task Any_predicate(bool async)
    {
        await base.Any_predicate(async);

        AssertSql(
            """
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        WHERE [c].[ContactName] LIKE N'A%') THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Any_nested_negated(bool async)
    {
        await base.Any_nested_negated(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] LIKE N'A%')
""");
    }

    public override async Task Any_nested_negated2(bool async)
    {
        await base.Any_nested_negated2(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[City] <> N'London' OR [c].[City] IS NULL) AND NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] LIKE N'ABC%')
""");
    }

    public override async Task Any_nested_negated3(bool async)
    {
        await base.Any_nested_negated3(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] LIKE N'ABC%') AND ([c].[City] <> N'London' OR [c].[City] IS NULL)
""");
    }

    public override async Task Any_nested(bool async)
    {
        await base.Any_nested(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] LIKE N'A%')
""");
    }

    public override async Task Any_nested2(bool async)
    {
        await base.Any_nested2(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[City] <> N'London' OR [c].[City] IS NULL) AND EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] LIKE N'A%')
""");
    }

    public override async Task Any_nested3(bool async)
    {
        await base.Any_nested3(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] LIKE N'A%') AND ([c].[City] <> N'London' OR [c].[City] IS NULL)
""");
    }

    public override async Task Any_with_multiple_conditions_still_uses_exists(bool async)
    {
        await base.Any_with_multiple_conditions_still_uses_exists(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London' AND EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID] AND [o].[EmployeeID] = 1)
""");
    }

    public override async Task All_top_level(bool async)
    {
        await base.All_top_level(async);

        AssertSql(
            """
SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        WHERE [c].[ContactName] NOT LIKE N'A%' OR [c].[ContactName] IS NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task All_top_level_column(bool async)
    {
        await base.All_top_level_column(async);

        AssertSql(
            """
SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        WHERE [c].[ContactName] IS NULL OR LEFT([c].[ContactName], LEN([c].[ContactName])) <> [c].[ContactName]) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task All_top_level_subquery(bool async)
    {
        await base.All_top_level_subquery(async);

        AssertSql(
            """
SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        WHERE NOT EXISTS (
            SELECT 1
            FROM [Customers] AS [c0]
            WHERE EXISTS (
                SELECT 1
                FROM [Customers] AS [c1]
                WHERE [c].[CustomerID] = [c1].[CustomerID]))) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task All_top_level_subquery_ef_property(bool async)
    {
        await base.All_top_level_subquery_ef_property(async);

        AssertSql(
            """
SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        WHERE NOT EXISTS (
            SELECT 1
            FROM [Customers] AS [c0]
            WHERE EXISTS (
                SELECT 1
                FROM [Customers] AS [c1]
                WHERE [c].[CustomerID] = [c1].[CustomerID]))) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Where_select_many_or(bool async)
    {
        await base.Where_select_many_or(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] = N'London' OR [e].[City] = N'London'
""");
    }

    public override async Task Where_select_many_or2(bool async)
    {
        await base.Where_select_many_or2(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] IN (N'London', N'Berlin')
""");
    }

    public override async Task Where_select_many_or3(bool async)
    {
        await base.Where_select_many_or3(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] IN (N'London', N'Berlin', N'Seattle')
""");
    }

    public override async Task Where_select_many_or4(bool async)
    {
        await base.Where_select_many_or4(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] IN (N'London', N'Berlin', N'Seattle', N'Lisboa')
""");
    }

    public override async Task Where_select_many_or_with_parameter(bool async)
    {
        await base.Where_select_many_or_with_parameter(async);

        AssertSql(
            """
@__london_0='London' (Size = 15)
@__lisboa_1='Lisboa' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] = @__london_0 OR [c].[City] = N'Berlin' OR [c].[City] = N'Seattle' OR [c].[City] = @__lisboa_1
""");
    }

    public override async Task SelectMany_simple_subquery(bool async)
    {
        await base.SelectMany_simple_subquery(async);

        AssertSql(
            """
@__p_0='9'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
) AS [e0]
CROSS JOIN [Customers] AS [c]
""");
    }

    public override async Task SelectMany_simple1(bool async)
    {
        await base.SelectMany_simple1(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
CROSS JOIN [Customers] AS [c]
""");
    }

    public override async Task SelectMany_simple2(bool async)
    {
        await base.SelectMany_simple2(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e0].[FirstName]
FROM [Employees] AS [e]
CROSS JOIN [Customers] AS [c]
CROSS JOIN [Employees] AS [e0]
""");
    }

    public override async Task SelectMany_entity_deep(bool async)
    {
        await base.SelectMany_entity_deep(async);

        AssertSql(
            """
SELECT [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title], [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title], [e2].[EmployeeID], [e2].[City], [e2].[Country], [e2].[FirstName], [e2].[ReportsTo], [e2].[Title]
FROM [Employees] AS [e]
CROSS JOIN [Employees] AS [e0]
CROSS JOIN [Employees] AS [e1]
CROSS JOIN [Employees] AS [e2]
""");
    }

    public override async Task SelectMany_projection1(bool async)
    {
        await base.SelectMany_projection1(async);

        AssertSql(
            """
SELECT [e].[City], [e0].[Country]
FROM [Employees] AS [e]
CROSS JOIN [Employees] AS [e0]
""");
    }

    public override async Task SelectMany_projection2(bool async)
    {
        await base.SelectMany_projection2(async);

        AssertSql(
            """
SELECT [e].[City], [e0].[Country], [e1].[FirstName]
FROM [Employees] AS [e]
CROSS JOIN [Employees] AS [e0]
CROSS JOIN [Employees] AS [e1]
""");
    }

    public override async Task SelectMany_Count(bool async)
    {
        await base.SelectMany_Count(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
""");
    }

    public override async Task SelectMany_LongCount(bool async)
    {
        await base.SelectMany_LongCount(async);

        AssertSql(
            """
SELECT COUNT_BIG(*)
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
""");
    }

    public override async Task SelectMany_OrderBy_ThenBy_Any(bool async)
    {
        await base.SelectMany_OrderBy_ThenBy_Any(async);

        AssertSql(
            """
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        CROSS JOIN [Orders] AS [o]) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Join_Where_Count(bool async)
    {
        await base.Join_Where_Count(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task Where_Join_Any(bool async)
    {
        await base.Where_Join_Any(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%' AND EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID] AND [o].[OrderDate] = '1998-01-15T00:00:00.000')
""");
    }

    public override async Task Where_Join_Exists(bool async)
    {
        await base.Where_Join_Exists(async);

        AssertSql();
    }

    public override async Task Where_Join_Exists_Inequality(bool async)
    {
        await base.Where_Join_Exists_Inequality(async);

        AssertSql();
    }

    public override async Task Where_Join_Exists_Constant(bool async)
    {
        await base.Where_Join_Exists_Constant(async);

        AssertSql();
    }

    public override async Task Where_Join_Not_Exists(bool async)
    {
        await base.Where_Join_Not_Exists(async);

        AssertSql();
    }

    public override async Task Join_OrderBy_Count(bool async)
    {
        await base.Join_OrderBy_Count(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
""");
    }

    public override async Task Multiple_joins_Where_Order_Any(bool async)
    {
        await base.Multiple_joins_Where_Order_Any(async);

        AssertSql(
            """
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
        INNER JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
        WHERE [c].[City] = N'London') THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Where_join_select(bool async)
    {
        await base.Where_join_select(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task Where_orderby_join_select(bool async)
    {
        await base.Where_orderby_join_select(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] <> N'ALFKI'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Where_join_orderby_join_select(bool async)
    {
        await base.Where_join_orderby_join_select(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
INNER JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE [c].[CustomerID] <> N'ALFKI'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Where_select_many(bool async)
    {
        await base.Where_select_many(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task Where_orderby_select_many(bool async)
    {
        await base.Where_orderby_select_many(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task SelectMany_cartesian_product_with_ordering(bool async)
    {
        await base.SelectMany_cartesian_product_with_ordering(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[City]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] = [e].[City] OR ([c].[City] IS NULL AND [e].[City] IS NULL)
ORDER BY [e].[City], [c].[CustomerID] DESC
""");
    }

    public override async Task SelectMany_Joined_DefaultIfEmpty(bool async)
    {
        await base.SelectMany_Joined_DefaultIfEmpty(async);

        AssertSql(
            """
SELECT [c].[ContactName], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
""");
    }

    public override async Task SelectMany_Joined_DefaultIfEmpty2(bool async)
    {
        await base.SelectMany_Joined_DefaultIfEmpty2(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
""");
    }

    public override async Task SelectMany_Joined_DefaultIfEmpty3(bool async)
    {
        await base.SelectMany_Joined_DefaultIfEmpty3(async);

        AssertSql(
            """
SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE EXISTS (
        SELECT 1
        FROM [Order Details] AS [o0]
        WHERE [o].[OrderID] = [o0].[OrderID])
) AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
""");
    }

    public override async Task SelectMany_Joined_Take(bool async)
    {
        await base.SelectMany_Joined_Take(async);

        AssertSql(
            """
SELECT [c].[ContactName], [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM (
        SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], ROW_NUMBER() OVER(PARTITION BY [o].[CustomerID] ORDER BY [o].[OrderID]) AS [row]
        FROM [Orders] AS [o]
    ) AS [o0]
    WHERE [o0].[row] <= 4
) AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
""");
    }

    public override async Task Take_with_single(bool async)
    {
        await base.Take_with_single(async);

        AssertSql(
            """
@__p_0='1'

SELECT TOP(2) [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c0]
ORDER BY [c0].[CustomerID]
""");
    }

    public override async Task Take_with_single_select_many(bool async)
    {
        await base.Take_with_single_select_many(async);

        AssertSql(
            """
@__p_0='1'

SELECT TOP(2) [s].[CustomerID], [s].[Address], [s].[City], [s].[CompanyName], [s].[ContactName], [s].[ContactTitle], [s].[Country], [s].[Fax], [s].[Phone], [s].[PostalCode], [s].[Region], [s].[OrderID], [s].[CustomerID0], [s].[EmployeeID], [s].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID] AS [CustomerID0], [o].[EmployeeID], [o].[OrderDate]
    FROM [Customers] AS [c]
    CROSS JOIN [Orders] AS [o]
    ORDER BY [c].[CustomerID], [o].[OrderID]
) AS [s]
ORDER BY [s].[CustomerID], [s].[OrderID]
""");
    }

    public override async Task Distinct_Skip(bool async)
    {
        await base.Distinct_Skip(async);

        AssertSql(
            """
@__p_0='5'

SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
) AS [c0]
ORDER BY [c0].[CustomerID]
OFFSET @__p_0 ROWS
""");
    }

    public override async Task Distinct_Skip_Take(bool async)
    {
        await base.Distinct_Skip_Take(async);

        AssertSql(
            """
@__p_0='5'
@__p_1='10'

SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
) AS [c0]
ORDER BY [c0].[ContactName]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
""");
    }

    public override async Task Skip_Distinct(bool async)
    {
        await base.Skip_Distinct(async);

        AssertSql(
            """
@__p_0='5'

SELECT DISTINCT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactName]
    OFFSET @__p_0 ROWS
) AS [c0]
""");
    }

    public override async Task Skip_Take_Distinct(bool async)
    {
        await base.Skip_Take_Distinct(async);

        AssertSql(
            """
@__p_0='5'
@__p_1='10'

SELECT DISTINCT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactName]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [c0]
""");
    }

    public override async Task Skip_Take_Any(bool async)
    {
        await base.Skip_Take_Any(async);

        AssertSql(
            """
@__p_0='5'
@__p_1='10'

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactName]
        OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Skip_Take_All(bool async)
    {
        await base.Skip_Take_All(async);

        AssertSql(
            """
@__p_0='4'
@__p_1='7'

SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM (
            SELECT [c].[CustomerID]
            FROM [Customers] AS [c]
            ORDER BY [c].[CustomerID]
            OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
        ) AS [c0]
        WHERE [c0].[CustomerID] NOT LIKE N'B%') THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Take_All(bool async)
    {
        await base.Take_All(async);

        AssertSql(
            """
@__p_0='4'

SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM (
            SELECT TOP(@__p_0) [c].[CustomerID]
            FROM [Customers] AS [c]
            ORDER BY [c].[CustomerID]
        ) AS [c0]
        WHERE [c0].[CustomerID] NOT LIKE N'A%') THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Skip_Take_Any_with_predicate(bool async)
    {
        await base.Skip_Take_Any_with_predicate(async);

        AssertSql(
            """
@__p_0='5'
@__p_1='7'

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM (
            SELECT [c].[CustomerID]
            FROM [Customers] AS [c]
            ORDER BY [c].[CustomerID]
            OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
        ) AS [c0]
        WHERE [c0].[CustomerID] LIKE N'C%') THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Take_Any_with_predicate(bool async)
    {
        await base.Take_Any_with_predicate(async);

        AssertSql(
            """
@__p_0='5'

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM (
            SELECT TOP(@__p_0) [c].[CustomerID]
            FROM [Customers] AS [c]
            ORDER BY [c].[CustomerID]
        ) AS [c0]
        WHERE [c0].[CustomerID] LIKE N'B%') THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task OrderBy(bool async)
    {
        await base.OrderBy(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task OrderBy_true(bool async)
    {
        await base.OrderBy_true(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task OrderBy_integer(bool async)
    {
        await base.OrderBy_integer(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task OrderBy_parameter(bool async)
    {
        await base.OrderBy_parameter(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task OrderBy_anon(bool async)
    {
        await base.OrderBy_anon(async);

        AssertSql(
            """
SELECT [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task OrderBy_anon2(bool async)
    {
        await base.OrderBy_anon2(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Distinct_Take(bool async)
    {
        await base.Distinct_Take(async);

        AssertSql(
            """
@__p_0='5'

SELECT TOP(@__p_0) [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM (
    SELECT DISTINCT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
) AS [o0]
ORDER BY [o0].[OrderID]
""");
    }

    public override async Task Distinct_Take_Count(bool async)
    {
        await base.Distinct_Take_Count(async);

        AssertSql(
            """
@__p_0='5'

SELECT COUNT(*)
FROM (
    SELECT DISTINCT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
) AS [o0]
""");
    }

    public override async Task OrderBy_shadow(bool async)
    {
        await base.OrderBy_shadow(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
ORDER BY [e].[Title], [e].[EmployeeID]
""");
    }

    public override async Task OrderBy_multiple(bool async)
    {
        await base.OrderBy_multiple(async);

        AssertSql(
            """
SELECT [c].[City]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[Country], [c].[City]
""");
    }

    public override async Task OrderBy_ThenBy_Any(bool async)
    {
        await base.OrderBy_ThenBy_Any(async);

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

    public override async Task OrderBy_correlated_subquery1(bool async)
    {
        await base.OrderBy_correlated_subquery1(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c0]
        WHERE [c0].[CustomerID] = [c].[CustomerID]) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [c].[CustomerID]
""");
    }

    public override async Task OrderBy_correlated_subquery2(bool async)
    {
        await base.OrderBy_correlated_subquery2(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] <= 10250 AND ((
    SELECT TOP(1) [c].[City]
    FROM [Customers] AS [c]
    ORDER BY CASE
        WHEN EXISTS (
            SELECT 1
            FROM [Customers] AS [c0]
            WHERE [c0].[CustomerID] = N'ALFKI') THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END) <> N'Nowhere' OR (
    SELECT TOP(1) [c].[City]
    FROM [Customers] AS [c]
    ORDER BY CASE
        WHEN EXISTS (
            SELECT 1
            FROM [Customers] AS [c0]
            WHERE [c0].[CustomerID] = N'ALFKI') THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END) IS NULL)
""");
    }

    public override async Task Where_subquery_recursive_trivial(bool async)
    {
        await base.Where_subquery_recursive_trivial(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE EXISTS (
    SELECT 1
    FROM [Employees] AS [e0]
    WHERE EXISTS (
        SELECT 1
        FROM [Employees] AS [e1]))
ORDER BY [e].[EmployeeID]
""");
    }

    public override async Task Select_DTO_distinct_translated_to_server(bool async)
    {
        await base.Select_DTO_distinct_translated_to_server(async);

        AssertSql(
            """
SELECT DISTINCT 1
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300
""");
    }

    public override async Task Select_DTO_constructor_distinct_translated_to_server(bool async)
    {
        await base.Select_DTO_constructor_distinct_translated_to_server(async);

        AssertSql(
            """
SELECT DISTINCT [o].[CustomerID]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300
""");
    }

    public override async Task Select_DTO_constructor_distinct_with_navigation_translated_to_server(bool async)
    {
        await base.Select_DTO_constructor_distinct_with_navigation_translated_to_server(async);

        AssertSql(
            """
SELECT DISTINCT [c].[City]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] < 10300
""");
    }

    public override async Task Select_DTO_constructor_distinct_with_collection_projection_translated_to_server(bool async)
    {
        await base.Select_DTO_constructor_distinct_with_collection_projection_translated_to_server(async);

        AssertSql(
            """
SELECT [o0].[CustomerID], [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate]
FROM (
    SELECT DISTINCT [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
) AS [o0]
LEFT JOIN [Orders] AS [o1] ON [o0].[CustomerID] = [o1].[CustomerID]
ORDER BY [o0].[CustomerID]
""");
    }

    public override async Task
        Select_DTO_constructor_distinct_with_collection_projection_translated_to_server_with_binding_after_client_eval(bool async)
    {
        // Allow binding of expressions after projection has turned to client eval. Issue #24478.
        await Assert.ThrowsAsync<TrueException>(
            () => base
                .Select_DTO_constructor_distinct_with_collection_projection_translated_to_server_with_binding_after_client_eval(async));

        AssertSql(
            """
SELECT [o0].[CustomerID], [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate]
FROM (
    SELECT DISTINCT [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
) AS [o0]
LEFT JOIN [Orders] AS [o1] ON [o0].[CustomerID] = [o1].[CustomerID]
ORDER BY [o0].[CustomerID]
""");
    }

    public override async Task Select_DTO_with_member_init_distinct_translated_to_server(bool async)
    {
        await base.Select_DTO_with_member_init_distinct_translated_to_server(async);

        AssertSql(
            """
SELECT DISTINCT [o].[CustomerID] AS [Id], [o].[OrderID] AS [Count]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300
""");
    }

    public override async Task Select_nested_collection_count_using_DTO(bool async)
    {
        await base.Select_nested_collection_count_using_DTO(async);

        AssertSql(
            """
SELECT [c].[CustomerID] AS [Id], (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]) AS [Count]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
""");
    }

    public override async Task Select_DTO_with_member_init_distinct_in_subquery_translated_to_server(bool async)
    {
        await base.Select_DTO_with_member_init_distinct_in_subquery_translated_to_server(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT DISTINCT [o].[CustomerID] AS [Id], [o].[OrderID] AS [Count]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
) AS [o0]
INNER JOIN [Customers] AS [c] ON [o0].[Id] = [c].[CustomerID]
""");
    }

    public override async Task Select_DTO_with_member_init_distinct_in_subquery_translated_to_server_2(bool async)
    {
        await base.Select_DTO_with_member_init_distinct_in_subquery_translated_to_server_2(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT DISTINCT [o].[CustomerID] AS [Id], [o].[OrderID] AS [Count]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
) AS [o0]
INNER JOIN [Customers] AS [c] ON [o0].[Id] = [c].[CustomerID]
""");
    }

    public override async Task Select_DTO_with_member_init_distinct_in_subquery_used_in_projection_translated_to_server(bool async)
    {
        await base.Select_DTO_with_member_init_distinct_in_subquery_used_in_projection_translated_to_server(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o0].[Id], [o0].[Count]
FROM [Customers] AS [c]
CROSS JOIN (
    SELECT DISTINCT [o].[CustomerID] AS [Id], [o].[OrderID] AS [Count]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
) AS [o0]
WHERE [c].[CustomerID] LIKE N'A%'
""");
    }

    public override async Task Select_correlated_subquery_filtered(bool async)
    {
        await base.Select_correlated_subquery_filtered(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Select_correlated_subquery_ordered(bool async)
    {
        await base.Select_correlated_subquery_ordered(async);

        AssertSql(
            """
@__p_0='3'

SELECT [c0].[CustomerID], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c0]
OUTER APPLY (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c0].[CustomerID] AS [CustomerID0]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID], [c0].[CustomerID]
    OFFSET 100 ROWS FETCH NEXT 2 ROWS ONLY
) AS [o0]
ORDER BY [c0].[CustomerID], [o0].[OrderID], [o0].[CustomerID0]
""");
    }

    public override async Task Where_subquery_on_bool(bool async)
    {
        await base.Where_subquery_on_bool(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE N'Chai' IN (
    SELECT [p0].[ProductName]
    FROM [Products] AS [p0]
)
""");
    }

    public override async Task Where_subquery_on_collection(bool async)
    {
        await base.Where_subquery_on_collection(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE CAST(5 AS smallint) IN (
    SELECT [o].[Quantity]
    FROM [Order Details] AS [o]
    WHERE [o].[ProductID] = [p].[ProductID]
)
""");
    }

    public override async Task Select_many_cross_join_same_collection(bool async)
    {
        await base.Select_many_cross_join_same_collection(async);

        AssertSql(
            """
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c]
CROSS JOIN [Customers] AS [c0]
""");
    }

    public override async Task OrderBy_null_coalesce_operator(bool async)
    {
        await base.OrderBy_null_coalesce_operator(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY COALESCE([c].[Region], N'ZZ'), [c].[CustomerID]
""");
    }

    public override async Task Select_null_coalesce_operator(bool async)
        => await base.Select_null_coalesce_operator(async);

    // issue #16038
    //            AssertSql(
    //                @"SELECT [c].[CustomerID], [c].[CompanyName], COALESCE([c].[Region], N'ZZ') AS [Region]
    //FROM [Customers] AS [c]
    //ORDER BY [Region], [c].[CustomerID]");
    public override async Task OrderBy_conditional_operator(bool async)
    {
        await base.OrderBy_conditional_operator(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY CASE
    WHEN [c].[Region] IS NULL THEN N'ZZ'
    ELSE [c].[Region]
END, [c].[CustomerID]
""");
    }

    public override async Task Null_Coalesce_Short_Circuit(bool async)
    {
        await base.Null_Coalesce_Short_Circuit(async);

        AssertSql(
            """
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region], CAST(0 AS bit) AS [Test]
FROM (
    SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
) AS [c0]
""");
    }

    public override async Task Null_Coalesce_Short_Circuit_with_server_correlated_leftover(bool async)
    {
        await base.Null_Coalesce_Short_Circuit_with_server_correlated_leftover(async);

        AssertSql(
            """
SELECT CAST(0 AS bit) AS [Result]
FROM [Customers] AS [c]
""");
    }

    public override async Task OrderBy_conditional_operator_where_condition_false(bool async)
    {
        await base.OrderBy_conditional_operator_where_condition_false(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[City]
""");
    }

    public override async Task OrderBy_comparison_operator(bool async)
    {
        await base.OrderBy_comparison_operator(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY CASE
    WHEN [c].[Region] = N'ASK' AND [c].[Region] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Projection_null_coalesce_operator(bool async)
    {
        await base.Projection_null_coalesce_operator(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[CompanyName], COALESCE([c].[Region], N'ZZ') AS [Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task Filter_coalesce_operator(bool async)
    {
        await base.Filter_coalesce_operator(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE COALESCE([c].[CompanyName], [c].[ContactName]) = N'The Big Cheese'
""");
    }

    public override async Task Take_skip_null_coalesce_operator(bool async)
    {
        await base.Take_skip_null_coalesce_operator(async);

        AssertSql(
            """
@__p_0='10'
@__p_1='5'

SELECT DISTINCT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
FROM (
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], COALESCE([c].[Region], N'ZZ') AS [c]
        FROM [Customers] AS [c]
        ORDER BY COALESCE([c].[Region], N'ZZ')
    ) AS [c0]
    ORDER BY [c0].[c]
    OFFSET @__p_1 ROWS
) AS [c1]
""");
    }

    public override async Task Select_take_null_coalesce_operator(bool async)
        => await base.Select_take_null_coalesce_operator(async);

    // issue #16038
    //            AssertSql(
    //                @"@__p_0='5'
    //SELECT TOP(@__p_0) [c].[CustomerID], [c].[CompanyName], COALESCE([c].[Region], N'ZZ') AS [Region]
    //FROM [Customers] AS [c]
    //ORDER BY [Region]");
    public override async Task Select_take_skip_null_coalesce_operator(bool async)
    {
        await base.Select_take_skip_null_coalesce_operator(async);

        AssertSql(
            """
@__p_0='10'
@__p_1='5'

SELECT [c0].[CustomerID], [c0].[CompanyName], COALESCE([c0].[Region], N'ZZ') AS [Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[CompanyName], [c].[Region], COALESCE([c].[Region], N'ZZ') AS [c]
    FROM [Customers] AS [c]
    ORDER BY COALESCE([c].[Region], N'ZZ')
) AS [c0]
ORDER BY [c0].[c]
OFFSET @__p_1 ROWS
""");
    }

    public override async Task Select_take_skip_null_coalesce_operator2(bool async)
    {
        await base.Select_take_skip_null_coalesce_operator2(async);

        AssertSql(
            """
@__p_0='10'
@__p_1='5'

SELECT [c0].[CustomerID], [c0].[CompanyName], [c0].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[CompanyName], [c].[Region], COALESCE([c].[Region], N'ZZ') AS [c]
    FROM [Customers] AS [c]
    ORDER BY COALESCE([c].[Region], N'ZZ')
) AS [c0]
ORDER BY [c0].[c]
OFFSET @__p_1 ROWS
""");
    }

    public override async Task Select_take_skip_null_coalesce_operator3(bool async)
    {
        await base.Select_take_skip_null_coalesce_operator3(async);

        AssertSql(
            """
@__p_0='10'
@__p_1='5'

SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], COALESCE([c].[Region], N'ZZ') AS [c]
    FROM [Customers] AS [c]
    ORDER BY COALESCE([c].[Region], N'ZZ')
) AS [c0]
ORDER BY [c0].[c]
OFFSET @__p_1 ROWS
""");
    }

    public override async Task Selected_column_can_coalesce(bool async)
    {
        await base.Selected_column_can_coalesce(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY COALESCE([c].[Region], N'ZZ')
""");
    }

    public override async Task DateTime_parse_is_inlined(bool async)
    {
        await base.DateTime_parse_is_inlined(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] > '1998-01-01T12:00:00.000'
""");
    }

    public override async Task DateTime_parse_is_parameterized_when_from_closure(bool async)
    {
        await base.DateTime_parse_is_parameterized_when_from_closure(async);

        AssertSql(
            """
@__Parse_0='1998-01-01T12:00:00.0000000' (Nullable = true) (DbType = DateTime)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] > @__Parse_0
""");
    }

    public override async Task New_DateTime_is_inlined(bool async)
    {
        await base.New_DateTime_is_inlined(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] > '1998-01-01T12:00:00.000'
""");
    }

    public override async Task New_DateTime_is_parameterized_when_from_closure(bool async)
    {
        await base.New_DateTime_is_parameterized_when_from_closure(async);

        AssertSql(
            """
@__p_0='1998-01-01T12:00:00.0000000' (Nullable = true) (DbType = DateTime)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] > @__p_0
""",
            //
            """
@__p_0='1998-01-01T11:00:00.0000000' (Nullable = true) (DbType = DateTime)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] > @__p_0
""");
    }

    public override async Task Environment_newline_is_funcletized(bool async)
    {
        await base.Environment_newline_is_funcletized(async);

        AssertSql(
            """
@__NewLine_0_contains='%
%' (Size = 5)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE @__NewLine_0_contains ESCAPE N'\'
""");
    }

    public override async Task Concat_string_int(bool async)
    {
        await base.Concat_string_int(async);

        AssertSql(
            """
SELECT CAST([o].[OrderID] AS nvarchar(max)) + COALESCE([o].[CustomerID], N'')
FROM [Orders] AS [o]
""");
    }

    public override async Task Concat_int_string(bool async)
    {
        await base.Concat_int_string(async);

        AssertSql(
            """
SELECT COALESCE([o].[CustomerID], N'') + CAST([o].[OrderID] AS nvarchar(max))
FROM [Orders] AS [o]
""");
    }

    public override async Task Concat_parameter_string_int(bool async)
    {
        await base.Concat_parameter_string_int(async);

        AssertSql(
            """
@__parameter_0='-' (Size = 4000)

SELECT @__parameter_0 + CAST([o].[OrderID] AS nvarchar(max))
FROM [Orders] AS [o]
""");
    }

    public override async Task Concat_constant_string_int(bool async)
    {
        await base.Concat_constant_string_int(async);

        AssertSql(
            """
SELECT N'-' + CAST([o].[OrderID] AS nvarchar(max))
FROM [Orders] AS [o]
""");
    }

    public override async Task String_concat_with_navigation1(bool async)
    {
        await base.String_concat_with_navigation1(async);

        AssertSql(
            """
SELECT COALESCE([o].[CustomerID], N'') + N' ' + COALESCE([c].[City], N'')
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
""");
    }

    public override async Task String_concat_with_navigation2(bool async)
    {
        await base.String_concat_with_navigation2(async);

        AssertSql(
            """
SELECT COALESCE([c].[City], N'') + N' ' + COALESCE([c].[City], N'')
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
""");
    }

    public override async Task Select_bitwise_or(bool async)
    {
        await base.Select_bitwise_or(async);

        AssertSql(
            """
SELECT [c].[CustomerID], CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END | CASE
    WHEN [c].[CustomerID] = N'ANATR' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Value]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Select_bitwise_or_multiple(bool async)
    {
        await base.Select_bitwise_or_multiple(async);

        AssertSql(
            """
SELECT [c].[CustomerID], CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END | CASE
    WHEN [c].[CustomerID] = N'ANATR' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END | CASE
    WHEN [c].[CustomerID] = N'ANTON' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Value]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Select_bitwise_and(bool async)
    {
        await base.Select_bitwise_and(async);

        AssertSql(
            """
SELECT [c].[CustomerID], CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END & CASE
    WHEN [c].[CustomerID] = N'ANATR' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Value]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Select_bitwise_and_or(bool async)
    {
        await base.Select_bitwise_and_or(async);

        AssertSql(
            """
SELECT [c].[CustomerID], (CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END & CASE
    WHEN [c].[CustomerID] = N'ANATR' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END) | CASE
    WHEN [c].[CustomerID] = N'ANTON' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Value]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Where_bitwise_or_with_logical_or(bool async)
    {
        await base.Where_bitwise_or_with_logical_or(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END | CASE
    WHEN [c].[CustomerID] = N'ANATR' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CAST(1 AS bit) OR [c].[CustomerID] = N'ANTON'
""");
    }

    public override async Task Where_bitwise_and_with_logical_and(bool async)
    {
        await base.Where_bitwise_and_with_logical_and(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END & CASE
    WHEN [c].[CustomerID] = N'ANATR' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CAST(1 AS bit) AND [c].[CustomerID] = N'ANTON'
""");
    }

    public override async Task Where_bitwise_or_with_logical_and(bool async)
    {
        await base.Where_bitwise_or_with_logical_and(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END | CASE
    WHEN [c].[CustomerID] = N'ANATR' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CAST(1 AS bit) AND [c].[Country] = N'Germany'
""");
    }

    public override async Task Where_bitwise_and_with_logical_or(bool async)
    {
        await base.Where_bitwise_and_with_logical_or(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END & CASE
    WHEN [c].[CustomerID] = N'ANATR' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CAST(1 AS bit) OR [c].[CustomerID] = N'ANTON'
""");
    }

    public override async Task Where_bitwise_binary_not(bool async)
    {
        await base.Where_bitwise_binary_not(async);

        AssertSql(
            """
@__negatedId_0='-10249'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ~[o].[OrderID] = @__negatedId_0
""");
    }

    public override async Task Where_bitwise_binary_and(bool async)
    {
        await base.Where_bitwise_binary_and(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] & 10248 = 10248
""");
    }

    public override async Task Where_bitwise_binary_or(bool async)
    {
        await base.Where_bitwise_binary_or(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] | 10248 = 10248
""");
    }

    public override async Task Select_bitwise_or_with_logical_or(bool async)
    {
        await base.Select_bitwise_or_with_logical_or(async);

        AssertSql(
            """
SELECT [c].[CustomerID], CASE
    WHEN CASE
        WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END | CASE
        WHEN [c].[CustomerID] = N'ANATR' THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END = CAST(1 AS bit) OR [c].[CustomerID] = N'ANTON' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Value]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Select_bitwise_and_with_logical_and(bool async)
    {
        await base.Select_bitwise_and_with_logical_and(async);

        AssertSql(
            """
SELECT [c].[CustomerID], CASE
    WHEN CASE
        WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END & CASE
        WHEN [c].[CustomerID] = N'ANATR' THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END = CAST(1 AS bit) AND [c].[CustomerID] = N'ANTON' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Value]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Handle_materialization_properly_when_more_than_two_query_sources_are_involved(bool async)
    {
        await base.Handle_materialization_properly_when_more_than_two_query_sources_are_involved(async);

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
CROSS JOIN [Employees] AS [e]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Parameter_extraction_short_circuits_1(bool async)
    {
        await base.Parameter_extraction_short_circuits_1(async);

        AssertSql(
            """
@__dateFilter_Value_Month_0='7'
@__dateFilter_Value_Year_1='1996'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10400 AND [o].[OrderDate] IS NOT NULL AND DATEPART(month, [o].[OrderDate]) = @__dateFilter_Value_Month_0 AND DATEPART(year, [o].[OrderDate]) = @__dateFilter_Value_Year_1
""",
            //
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10400
""");
    }

    public override async Task Parameter_extraction_short_circuits_2(bool async)
    {
        await base.Parameter_extraction_short_circuits_2(async);

        AssertSql(
            """
@__dateFilter_Value_Month_0='7'
@__dateFilter_Value_Year_1='1996'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10400 AND [o].[OrderDate] IS NOT NULL AND DATEPART(month, [o].[OrderDate]) = @__dateFilter_Value_Month_0 AND DATEPART(year, [o].[OrderDate]) = @__dateFilter_Value_Year_1
""",
            //
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE 0 = 1
""");
    }

    public override async Task Parameter_extraction_short_circuits_3(bool async)
    {
        await base.Parameter_extraction_short_circuits_3(async);

        AssertSql(
            """
@__dateFilter_Value_Month_0='7'
@__dateFilter_Value_Year_1='1996'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10400 OR ([o].[OrderDate] IS NOT NULL AND DATEPART(month, [o].[OrderDate]) = @__dateFilter_Value_Month_0 AND DATEPART(year, [o].[OrderDate]) = @__dateFilter_Value_Year_1)
""",
            //
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
""");
    }

    public override async Task Subquery_member_pushdown_does_not_change_original_subquery_model(bool async)
    {
        await base.Subquery_member_pushdown_does_not_change_original_subquery_model(async);

        AssertSql(
            """
@__p_0='3'

SELECT [o0].[OrderID] AS [OrderId], (
    SELECT TOP(1) [c0].[City]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = [o0].[CustomerID]) AS [City]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [o0]
ORDER BY (
    SELECT TOP(1) [c].[City]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = [o0].[CustomerID])
""");
    }

    public override async Task Subquery_member_pushdown_does_not_change_original_subquery_model2(bool async)
    {
        await base.Subquery_member_pushdown_does_not_change_original_subquery_model2(async);

        AssertSql(
            """
@__p_0='3'

SELECT [o0].[OrderID] AS [OrderId], (
    SELECT TOP(1) [c0].[City]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = [o0].[CustomerID]) AS [City]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [o0]
ORDER BY (
    SELECT TOP(1) [c].[City]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = [o0].[CustomerID])
""");
    }

    public override async Task Query_expression_with_to_string_and_contains(bool async)
    {
        await base.Query_expression_with_to_string_and_contains(async);

        AssertSql(
            """
SELECT [o].[CustomerID]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL AND CONVERT(varchar(10), [o].[EmployeeID]) LIKE '%7%'
""");
    }

    public override async Task Select_expression_long_to_string(bool async)
    {
        await base.Select_expression_long_to_string(async);

        AssertSql(
            """
SELECT CONVERT(varchar(20), CAST([o].[OrderID] AS bigint)) AS [ShipName]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL
""");
    }

    public override async Task Select_expression_int_to_string(bool async)
    {
        await base.Select_expression_int_to_string(async);

        AssertSql(
            """
SELECT CONVERT(varchar(11), [o].[OrderID]) AS [ShipName]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL
""");
    }

    public override async Task ToString_with_formatter_is_evaluated_on_the_client(bool async)
    {
        await base.ToString_with_formatter_is_evaluated_on_the_client(async);

        AssertSql(
            """
SELECT [o].[OrderID]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL
""",
            //
            """
SELECT [o].[OrderID]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL
""");
    }

    public override async Task Select_expression_other_to_string(bool async)
    {
        await base.Select_expression_other_to_string(async);

        AssertSql(
            """
SELECT CONVERT(varchar(100), [o].[OrderDate]) AS [ShipName]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL
""");
    }

    public override async Task Select_expression_date_add_year(bool async)
    {
        await base.Select_expression_date_add_year(async);

        AssertSql(
            """
SELECT DATEADD(year, CAST(1 AS int), [o].[OrderDate]) AS [OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL
""");
    }

    public override async Task Select_expression_datetime_add_month(bool async)
    {
        await base.Select_expression_datetime_add_month(async);

        AssertSql(
            """
SELECT DATEADD(month, CAST(1 AS int), [o].[OrderDate]) AS [OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL
""");
    }

    public override async Task Select_expression_datetime_add_hour(bool async)
    {
        await base.Select_expression_datetime_add_hour(async);

        AssertSql(
            """
SELECT DATEADD(hour, CAST(1.0E0 AS int), [o].[OrderDate]) AS [OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL
""");
    }

    public override async Task Select_expression_datetime_add_minute(bool async)
    {
        await base.Select_expression_datetime_add_minute(async);

        AssertSql(
            """
SELECT DATEADD(minute, CAST(1.0E0 AS int), [o].[OrderDate]) AS [OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL
""");
    }

    public override async Task Select_expression_datetime_add_second(bool async)
    {
        await base.Select_expression_datetime_add_second(async);

        AssertSql(
            """
SELECT DATEADD(second, CAST(1.0E0 AS int), [o].[OrderDate]) AS [OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL
""");
    }

    public override async Task Select_expression_date_add_milliseconds_above_the_range(bool async)
    {
        await base.Select_expression_date_add_milliseconds_above_the_range(async);

        AssertSql(
            """
SELECT [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL
""");
    }

    public override async Task Select_expression_date_add_milliseconds_below_the_range(bool async)
    {
        await base.Select_expression_date_add_milliseconds_below_the_range(async);

        AssertSql(
            """
SELECT [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL
""");
    }

    public override async Task Select_expression_date_add_milliseconds_large_number_divided(bool async)
    {
        await base.Select_expression_date_add_milliseconds_large_number_divided(async);

        AssertSql(
            """
@__millisecondsPerDay_0='86400000'

SELECT DATEADD(millisecond, CAST(CAST(CAST(DATEPART(millisecond, [o].[OrderDate]) AS bigint) % @__millisecondsPerDay_0 AS float) AS int), DATEADD(day, CAST(CAST(CAST(DATEPART(millisecond, [o].[OrderDate]) AS bigint) / @__millisecondsPerDay_0 AS float) AS int), [o].[OrderDate])) AS [OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL
""");
    }

    public override async Task Add_minutes_on_constant_value(bool async)
    {
        await base.Add_minutes_on_constant_value(async);

        AssertSql(
            """
SELECT DATEADD(minute, CAST(CAST([o].[OrderID] % 25 AS float) AS int), '1900-01-01T00:00:00.000') AS [Test]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10500
ORDER BY [o].[OrderID]
""");
    }

    public override async Task Select_expression_references_are_updated_correctly_with_subquery(bool async)
    {
        await base.Select_expression_references_are_updated_correctly_with_subquery(async);

        AssertSql(
            """
@__nextYear_0='2017'

SELECT DISTINCT DATEPART(year, [o].[OrderDate])
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL AND DATEPART(year, [o].[OrderDate]) < @__nextYear_0
""");
    }

    public override async Task DefaultIfEmpty_without_group_join(bool async)
    {
        await base.DefaultIfEmpty_without_group_join(async);

        AssertSql(
            """
SELECT [c0].[CustomerID]
FROM (
    SELECT 1 AS empty
) AS [e]
LEFT JOIN (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'London'
) AS [c0] ON 1 = 1
WHERE [c0].[CustomerID] IS NOT NULL
""");
    }

    public override async Task DefaultIfEmpty_in_subquery(bool async)
    {
        await base.DefaultIfEmpty_in_subquery(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [o].[OrderID] IS NOT NULL
""");
    }

    public override async Task DefaultIfEmpty_in_subquery_not_correlated(bool async)
    {
        await base.DefaultIfEmpty_in_subquery_not_correlated(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [s].[OrderID]
FROM [Customers] AS [c]
CROSS JOIN (
    SELECT [o0].[OrderID]
    FROM (
        SELECT 1 AS empty
    ) AS [e]
    LEFT JOIN (
        SELECT [o].[OrderID]
        FROM [Orders] AS [o]
        WHERE [o].[OrderID] > 15000
    ) AS [o0] ON 1 = 1
) AS [s]
""");
    }

    public override async Task DefaultIfEmpty_in_subquery_nested(bool async)
    {
        await base.DefaultIfEmpty_in_subquery_nested(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [s].[OrderID], [o1].[OrderDate]
FROM [Customers] AS [c]
CROSS JOIN (
    SELECT [o0].[OrderID]
    FROM (
        SELECT 1 AS empty
    ) AS [e]
    LEFT JOIN (
        SELECT [o].[OrderID]
        FROM [Orders] AS [o]
        WHERE [o].[OrderID] > 11050
    ) AS [o0] ON 1 = 1
) AS [s]
LEFT JOIN [Orders] AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
WHERE [c].[City] = N'Seattle' AND [s].[OrderID] IS NOT NULL AND [o1].[OrderID] IS NOT NULL
ORDER BY [s].[OrderID], [o1].[OrderDate]
""");
    }

    public override async Task DefaultIfEmpty_in_subquery_nested_filter_order_comparison(bool async)
    {
        await base.DefaultIfEmpty_in_subquery_nested_filter_order_comparison(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [s].[OrderID], [o2].[OrderDate]
FROM [Customers] AS [c]
CROSS JOIN (
    SELECT [o0].[OrderID]
    FROM (
        SELECT 1 AS empty
    ) AS [e]
    LEFT JOIN (
        SELECT [o].[OrderID]
        FROM [Orders] AS [o]
        WHERE [o].[OrderID] > 11050
    ) AS [o0] ON 1 = 1
) AS [s]
OUTER APPLY (
    SELECT [o1].[OrderID], [o1].[OrderDate]
    FROM [Orders] AS [o1]
    WHERE [o1].[OrderID] <= CAST(LEN([c].[CustomerID]) AS int) + 10250
) AS [o2]
WHERE [c].[City] = N'Seattle' AND [s].[OrderID] IS NOT NULL AND [o2].[OrderID] IS NOT NULL
ORDER BY [s].[OrderID], [o2].[OrderDate]
""");
    }

    public override async Task OrderBy_skip_take(bool async)
    {
        await base.OrderBy_skip_take(async);

        AssertSql(
            """
@__p_0='5'
@__p_1='8'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactTitle], [c].[ContactName]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
""");
    }

    public override async Task OrderBy_skip_skip_take(bool async)
    {
        await base.OrderBy_skip_skip_take(async);

        AssertSql(
            """
@__p_0='5'
@__p_1='8'
@__p_2='3'

SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactTitle], [c].[ContactName]
    OFFSET @__p_0 ROWS
) AS [c0]
ORDER BY [c0].[ContactTitle], [c0].[ContactName]
OFFSET @__p_1 ROWS FETCH NEXT @__p_2 ROWS ONLY
""");
    }

    public override async Task OrderBy_skip_take_take(bool async)
    {
        await base.OrderBy_skip_take_take(async);

        AssertSql(
            """
@__p_2='3'
@__p_0='5'
@__p_1='8'

SELECT TOP(@__p_2) [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactTitle], [c].[ContactName]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [c0]
ORDER BY [c0].[ContactTitle], [c0].[ContactName]
""");
    }

    public override async Task OrderBy_skip_take_take_take_take(bool async)
    {
        await base.OrderBy_skip_take_take_take_take(async);

        AssertSql(
            """
@__p_0='5'
@__p_3='8'
@__p_2='10'
@__p_1='15'

SELECT TOP(@__p_0) [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
FROM (
    SELECT TOP(@__p_3) [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
    FROM (
        SELECT TOP(@__p_2) [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
        FROM (
            SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
            FROM [Customers] AS [c]
            ORDER BY [c].[ContactTitle], [c].[ContactName]
            OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
        ) AS [c0]
        ORDER BY [c0].[ContactTitle], [c0].[ContactName]
    ) AS [c1]
    ORDER BY [c1].[ContactTitle], [c1].[ContactName]
) AS [c2]
ORDER BY [c2].[ContactTitle], [c2].[ContactName]
""");
    }

    public override async Task OrderBy_skip_take_skip_take_skip(bool async)
    {
        await base.OrderBy_skip_take_skip_take_skip(async);

        AssertSql(
            """
@__p_0='5'
@__p_1='15'
@__p_2='2'
@__p_3='8'

SELECT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
FROM (
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM (
        SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactTitle], [c].[ContactName]
        OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
    ) AS [c0]
    ORDER BY [c0].[ContactTitle], [c0].[ContactName]
    OFFSET @__p_2 ROWS FETCH NEXT @__p_3 ROWS ONLY
) AS [c1]
ORDER BY [c1].[ContactTitle], [c1].[ContactName]
OFFSET @__p_0 ROWS
""");
    }

    public override async Task OrderBy_skip_take_distinct(bool async)
    {
        await base.OrderBy_skip_take_distinct(async);

        AssertSql(
            """
@__p_0='5'
@__p_1='15'

SELECT DISTINCT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactTitle], [c].[ContactName]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [c0]
""");
    }

    public override async Task OrderBy_coalesce_take_distinct(bool async)
    {
        await base.OrderBy_coalesce_take_distinct(async);

        AssertSql(
            """
@__p_0='15'

SELECT DISTINCT [p0].[ProductID], [p0].[Discontinued], [p0].[ProductName], [p0].[SupplierID], [p0].[UnitPrice], [p0].[UnitsInStock]
FROM (
    SELECT TOP(@__p_0) [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
    FROM [Products] AS [p]
    ORDER BY COALESCE([p].[UnitPrice], 0.0)
) AS [p0]
""");
    }

    public override async Task OrderBy_coalesce_skip_take_distinct(bool async)
    {
        await base.OrderBy_coalesce_skip_take_distinct(async);

        AssertSql(
            """
@__p_0='5'
@__p_1='15'

SELECT DISTINCT [p0].[ProductID], [p0].[Discontinued], [p0].[ProductName], [p0].[SupplierID], [p0].[UnitPrice], [p0].[UnitsInStock]
FROM (
    SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
    FROM [Products] AS [p]
    ORDER BY COALESCE([p].[UnitPrice], 0.0)
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [p0]
""");
    }

    public override async Task OrderBy_coalesce_skip_take_distinct_take(bool async)
    {
        await base.OrderBy_coalesce_skip_take_distinct_take(async);

        AssertSql(
            """
@__p_0='5'
@__p_1='15'

SELECT DISTINCT TOP(@__p_0) [p0].[ProductID], [p0].[Discontinued], [p0].[ProductName], [p0].[SupplierID], [p0].[UnitPrice], [p0].[UnitsInStock]
FROM (
    SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
    FROM [Products] AS [p]
    ORDER BY COALESCE([p].[UnitPrice], 0.0)
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [p0]
""");
    }

    public override async Task OrderBy_skip_take_distinct_orderby_take(bool async)
    {
        await base.OrderBy_skip_take_distinct_orderby_take(async);

        AssertSql(
            """
@__p_2='8'
@__p_0='5'
@__p_1='15'

SELECT TOP(@__p_2) [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
FROM (
    SELECT DISTINCT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM (
        SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactTitle], [c].[ContactName]
        OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
    ) AS [c0]
) AS [c1]
ORDER BY [c1].[ContactTitle]
""");
    }

    public override async Task No_orderby_added_for_fully_translated_manually_constructed_LOJ(bool async)
    {
        await base.No_orderby_added_for_fully_translated_manually_constructed_LOJ(async);

        AssertSql(
            """
SELECT [e].[City] AS [City1], [e0].[City] AS [City2]
FROM [Employees] AS [e]
LEFT JOIN [Employees] AS [e0] ON [e].[EmployeeID] = [e0].[ReportsTo]
""");
    }

    public override async Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ(bool async)
    {
        await base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ(async);

        AssertSql();
    }

    public override async Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition1(
        bool async)
    {
        await base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition1(async);

        AssertSql();
    }

    public override async Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2(
        bool async)
    {
        await base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2(async);

        AssertSql();
    }

    public override async Task Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ(bool async)
    {
        await base.Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ(async);

        AssertSql();
    }

    public override async Task Contains_with_DateTime_Date(bool async)
    {
        await base.Contains_with_DateTime_Date(async);

        AssertSql(
            """
@__dates_0='["1996-07-04T00:00:00","1996-07-16T00:00:00"]' (Size = 4000)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE CONVERT(date, [o].[OrderDate]) IN (
    SELECT [d].[value]
    FROM OPENJSON(@__dates_0) WITH ([value] datetime '$') AS [d]
)
""",
            //
            """
@__dates_0='["1996-07-04T00:00:00"]' (Size = 4000)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE CONVERT(date, [o].[OrderDate]) IN (
    SELECT [d].[value]
    FROM OPENJSON(@__dates_0) WITH ([value] datetime '$') AS [d]
)
""");
    }

    public override async Task Contains_with_subquery_involving_join_binds_to_correct_table(bool async)
    {
        await base.Contains_with_subquery_involving_join_binds_to_correct_table(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] > 11000 AND [o].[OrderID] IN (
    SELECT [o0].[OrderID]
    FROM [Order Details] AS [o0]
    INNER JOIN [Products] AS [p] ON [o0].[ProductID] = [p].[ProductID]
    WHERE [p].[ProductName] = N'Chai'
)
""");
    }

    public override async Task Complex_query_with_repeated_query_model_compiles_correctly(bool async)
    {
        await base.Complex_query_with_repeated_query_model_compiles_correctly(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI' AND EXISTS (
    SELECT 1
    FROM [Customers] AS [c0]
    WHERE EXISTS (
        SELECT 1
        FROM [Customers] AS [c1]))
""");
    }

    public override async Task Complex_query_with_repeated_nested_query_model_compiles_correctly(bool async)
    {
        await base.Complex_query_with_repeated_nested_query_model_compiles_correctly(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI' AND EXISTS (
    SELECT 1
    FROM [Customers] AS [c0]
    WHERE EXISTS (
        SELECT 1
        FROM [Customers] AS [c1]
        WHERE EXISTS (
            SELECT DISTINCT 1
            FROM (
                SELECT TOP(10) 1 AS empty
                FROM [Customers] AS [c2]
                ORDER BY [c2].[CustomerID]
            ) AS [c3])))
""");
    }

    public override async Task Anonymous_member_distinct_where(bool async)
    {
        await base.Anonymous_member_distinct_where(async);

        AssertSql(
            """
SELECT DISTINCT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task Anonymous_member_distinct_orderby(bool async)
    {
        await base.Anonymous_member_distinct_orderby(async);

        AssertSql(
            """
SELECT [c0].[CustomerID]
FROM (
    SELECT DISTINCT [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [c0]
ORDER BY [c0].[CustomerID]
""");
    }

    public override async Task Anonymous_member_distinct_result(bool async)
    {
        await base.Anonymous_member_distinct_result(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'A%'
) AS [c0]
""");
    }

    public override async Task Anonymous_complex_distinct_where(bool async)
    {
        await base.Anonymous_complex_distinct_where(async);

        AssertSql(
            """
SELECT DISTINCT [c].[CustomerID] + COALESCE([c].[City], N'') AS [A]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] + COALESCE([c].[City], N'') = N'ALFKIBerlin'
""");
    }

    public override async Task Anonymous_complex_distinct_orderby(bool async)
    {
        await base.Anonymous_complex_distinct_orderby(async);

        AssertSql(
            """
SELECT [c0].[A]
FROM (
    SELECT DISTINCT [c].[CustomerID] + COALESCE([c].[City], N'') AS [A]
    FROM [Customers] AS [c]
) AS [c0]
ORDER BY [c0].[A]
""");
    }

    public override async Task Anonymous_complex_distinct_result(bool async)
    {
        await base.Anonymous_complex_distinct_result(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[CustomerID] + COALESCE([c].[City], N'') AS [A]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] + COALESCE([c].[City], N'') LIKE N'A%'
) AS [c0]
""");
    }

    public override async Task Anonymous_complex_orderby(bool async)
    {
        await base.Anonymous_complex_orderby(async);

        AssertSql(
            """
SELECT [c].[CustomerID] + COALESCE([c].[City], N'') AS [A]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID] + COALESCE([c].[City], N'')
""");
    }

    public override async Task Anonymous_subquery_orderby(bool async)
    {
        await base.Anonymous_subquery_orderby(async);

        AssertSql(
            """
SELECT (
    SELECT TOP(1) [o1].[OrderDate]
    FROM [Orders] AS [o1]
    WHERE [c].[CustomerID] = [o1].[CustomerID]
    ORDER BY [o1].[OrderID] DESC) AS [A]
FROM [Customers] AS [c]
WHERE (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]) > 1
ORDER BY (
    SELECT TOP(1) [o0].[OrderDate]
    FROM [Orders] AS [o0]
    WHERE [c].[CustomerID] = [o0].[CustomerID]
    ORDER BY [o0].[OrderID] DESC)
""");
    }

    public override async Task DTO_member_distinct_where(bool async)
    {
        await base.DTO_member_distinct_where(async);

        AssertSql(
            """
SELECT DISTINCT [c].[CustomerID] AS [Property]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task DTO_member_distinct_orderby(bool async)
    {
        await base.DTO_member_distinct_orderby(async);

        AssertSql(
            """
SELECT [c0].[Property]
FROM (
    SELECT DISTINCT [c].[CustomerID] AS [Property]
    FROM [Customers] AS [c]
) AS [c0]
ORDER BY [c0].[Property]
""");
    }

    public override async Task DTO_member_distinct_result(bool async)
    {
        await base.DTO_member_distinct_result(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[CustomerID] AS [Property]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'A%'
) AS [c0]
""");
    }

    public override async Task DTO_complex_distinct_where(bool async)
    {
        await base.DTO_complex_distinct_where(async);

        AssertSql(
            """
SELECT DISTINCT [c].[CustomerID] + COALESCE([c].[City], N'') AS [Property]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] + COALESCE([c].[City], N'') = N'ALFKIBerlin'
""");
    }

    public override async Task DTO_complex_distinct_orderby(bool async)
    {
        await base.DTO_complex_distinct_orderby(async);

        AssertSql(
            """
SELECT [c0].[Property]
FROM (
    SELECT DISTINCT [c].[CustomerID] + COALESCE([c].[City], N'') AS [Property]
    FROM [Customers] AS [c]
) AS [c0]
ORDER BY [c0].[Property]
""");
    }

    public override async Task DTO_complex_distinct_result(bool async)
    {
        await base.DTO_complex_distinct_result(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[CustomerID] + COALESCE([c].[City], N'') AS [Property]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] + COALESCE([c].[City], N'') LIKE N'A%'
) AS [c0]
""");
    }

    public override async Task DTO_complex_orderby(bool async)
        => await base.DTO_complex_orderby(async);

    // issue #18775
    //            AssertSql(
    //                @"SELECT [c].[CustomerID] + [c].[City] AS [Property]
    //FROM [Customers] AS [c]
    //ORDER BY [Property]");
    public override async Task DTO_subquery_orderby(bool async)
    {
        await base.DTO_subquery_orderby(async);

        AssertSql(
            """
SELECT (
    SELECT TOP(1) [o1].[OrderDate]
    FROM [Orders] AS [o1]
    WHERE [c].[CustomerID] = [o1].[CustomerID]
    ORDER BY [o1].[OrderID] DESC) AS [Property]
FROM [Customers] AS [c]
WHERE (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]) > 1
ORDER BY (
    SELECT TOP(1) [o0].[OrderDate]
    FROM [Orders] AS [o0]
    WHERE [c].[CustomerID] = [o0].[CustomerID]
    ORDER BY [o0].[OrderID] DESC)
""");
    }

    public override async Task Include_with_orderby_skip_preserves_ordering(bool async)
    {
        await base.Include_with_orderby_skip_preserves_ordering(async);

        AssertSql(
            """
@__p_0='40'
@__p_1='5'

SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] NOT IN (N'VAFFE', N'DRACD')
    ORDER BY [c].[City], [c].[CustomerID]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [c0]
LEFT JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [c0].[City], [c0].[CustomerID]
""");
    }

    public override async Task Int16_parameter_can_be_used_for_int_column(bool async)
    {
        await base.Int16_parameter_can_be_used_for_int_column(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] = 10300
""");
    }

    public override async Task Subquery_is_null_translated_correctly(bool async)
    {
        await base.Subquery_is_null_translated_correctly(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT TOP(1) [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID] DESC) IS NULL
""");
    }

    public override async Task Subquery_is_not_null_translated_correctly(bool async)
    {
        await base.Subquery_is_not_null_translated_correctly(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT TOP(1) [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID] DESC) IS NOT NULL
""");
    }

    public override async Task Select_take_average(bool async)
    {
        await base.Select_take_average(async);

        AssertSql(
            """
@__p_0='10'

SELECT AVG(CAST([o0].[OrderID] AS float))
FROM (
    SELECT TOP(@__p_0) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [o0]
""");
    }

    public override async Task Select_take_count(bool async)
    {
        await base.Select_take_count(async);

        AssertSql(
            """
@__p_0='7'

SELECT COUNT(*)
FROM (
    SELECT TOP(@__p_0) 1 AS empty
    FROM [Customers] AS [c]
) AS [c0]
""");
    }

    public override async Task Select_orderBy_take_count(bool async)
    {
        await base.Select_orderBy_take_count(async);

        AssertSql(
            """
@__p_0='7'

SELECT COUNT(*)
FROM (
    SELECT TOP(@__p_0) 1 AS empty
    FROM [Customers] AS [c]
    ORDER BY [c].[Country]
) AS [c0]
""");
    }

    public override async Task Select_take_long_count(bool async)
    {
        await base.Select_take_long_count(async);

        AssertSql(
            """
@__p_0='7'

SELECT COUNT_BIG(*)
FROM (
    SELECT TOP(@__p_0) 1 AS empty
    FROM [Customers] AS [c]
) AS [c0]
""");
    }

    public override async Task Select_orderBy_take_long_count(bool async)
    {
        await base.Select_orderBy_take_long_count(async);

        AssertSql(
            """
@__p_0='7'

SELECT COUNT_BIG(*)
FROM (
    SELECT TOP(@__p_0) 1 AS empty
    FROM [Customers] AS [c]
    ORDER BY [c].[Country]
) AS [c0]
""");
    }

    public override async Task Select_take_max(bool async)
    {
        await base.Select_take_max(async);

        AssertSql(
            """
@__p_0='10'

SELECT MAX([o0].[OrderID])
FROM (
    SELECT TOP(@__p_0) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [o0]
""");
    }

    public override async Task Select_take_min(bool async)
    {
        await base.Select_take_min(async);

        AssertSql(
            """
@__p_0='10'

SELECT MIN([o0].[OrderID])
FROM (
    SELECT TOP(@__p_0) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [o0]
""");
    }

    public override async Task Select_take_sum(bool async)
    {
        await base.Select_take_sum(async);

        AssertSql(
            """
@__p_0='10'

SELECT COALESCE(SUM([o0].[OrderID]), 0)
FROM (
    SELECT TOP(@__p_0) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [o0]
""");
    }

    public override async Task Select_skip_average(bool async)
    {
        await base.Select_skip_average(async);

        AssertSql(
            """
@__p_0='10'

SELECT AVG(CAST([o0].[OrderID] AS float))
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS
) AS [o0]
""");
    }

    public override async Task Select_skip_count(bool async)
    {
        await base.Select_skip_count(async);

        AssertSql(
            """
@__p_0='7'

SELECT COUNT(*)
FROM (
    SELECT 1 AS empty
    FROM [Customers] AS [c]
    ORDER BY (SELECT 1)
    OFFSET @__p_0 ROWS
) AS [c0]
""");
    }

    public override async Task Select_orderBy_skip_count(bool async)
    {
        await base.Select_orderBy_skip_count(async);

        AssertSql(
            """
@__p_0='7'

SELECT COUNT(*)
FROM (
    SELECT 1 AS empty
    FROM [Customers] AS [c]
    ORDER BY [c].[Country]
    OFFSET @__p_0 ROWS
) AS [c0]
""");
    }

    public override async Task Select_skip_long_count(bool async)
    {
        await base.Select_skip_long_count(async);

        AssertSql(
            """
@__p_0='7'

SELECT COUNT_BIG(*)
FROM (
    SELECT 1 AS empty
    FROM [Customers] AS [c]
    ORDER BY (SELECT 1)
    OFFSET @__p_0 ROWS
) AS [c0]
""");
    }

    public override async Task Select_orderBy_skip_long_count(bool async)
    {
        await base.Select_orderBy_skip_long_count(async);

        AssertSql(
            """
@__p_0='7'

SELECT COUNT_BIG(*)
FROM (
    SELECT 1 AS empty
    FROM [Customers] AS [c]
    ORDER BY [c].[Country]
    OFFSET @__p_0 ROWS
) AS [c0]
""");
    }

    public override async Task Select_skip_max(bool async)
    {
        await base.Select_skip_max(async);

        AssertSql(
            """
@__p_0='10'

SELECT MAX([o0].[OrderID])
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS
) AS [o0]
""");
    }

    public override async Task Select_skip_min(bool async)
    {
        await base.Select_skip_min(async);

        AssertSql(
            """
@__p_0='10'

SELECT MIN([o0].[OrderID])
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS
) AS [o0]
""");
    }

    public override async Task Select_skip_sum(bool async)
    {
        await base.Select_skip_sum(async);

        AssertSql(
            """
@__p_0='10'

SELECT COALESCE(SUM([o0].[OrderID]), 0)
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS
) AS [o0]
""");
    }

    public override async Task Select_distinct_average(bool async)
    {
        await base.Select_distinct_average(async);

        AssertSql(
            """
SELECT AVG(CAST([o0].[OrderID] AS float))
FROM (
    SELECT DISTINCT [o].[OrderID]
    FROM [Orders] AS [o]
) AS [o0]
""");
    }

    public override async Task Select_distinct_count(bool async)
    {
        await base.Select_distinct_count(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
) AS [c0]
""");
    }

    public override async Task Select_distinct_long_count(bool async)
    {
        await base.Select_distinct_long_count(async);

        AssertSql(
            """
SELECT COUNT_BIG(*)
FROM (
    SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
) AS [c0]
""");
    }

    public override async Task Select_distinct_max(bool async)
    {
        await base.Select_distinct_max(async);

        AssertSql(
            """
SELECT MAX([o0].[OrderID])
FROM (
    SELECT DISTINCT [o].[OrderID]
    FROM [Orders] AS [o]
) AS [o0]
""");
    }

    public override async Task Select_distinct_min(bool async)
    {
        await base.Select_distinct_min(async);

        AssertSql(
            """
SELECT MIN([o0].[OrderID])
FROM (
    SELECT DISTINCT [o].[OrderID]
    FROM [Orders] AS [o]
) AS [o0]
""");
    }

    public override async Task Select_distinct_sum(bool async)
    {
        await base.Select_distinct_sum(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o0].[OrderID]), 0)
FROM (
    SELECT DISTINCT [o].[OrderID]
    FROM [Orders] AS [o]
) AS [o0]
""");
    }

    public override async Task Comparing_to_fixed_string_parameter(bool async)
    {
        await base.Comparing_to_fixed_string_parameter(async);

        AssertSql(
            """
@__prefix_0_startswith='A%' (Size = 5)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE @__prefix_0_startswith ESCAPE N'\'
""");
    }

    public override async Task Comparing_entities_using_Equals(bool async)
    {
        await base.Comparing_entities_using_Equals(async);

        AssertSql(
            """
SELECT [c].[CustomerID] AS [Id1], [c0].[CustomerID] AS [Id2]
FROM [Customers] AS [c]
CROSS JOIN [Customers] AS [c0]
WHERE [c].[CustomerID] LIKE N'ALFKI%' AND [c].[CustomerID] = [c0].[CustomerID]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Comparing_different_entity_types_using_Equals(bool async)
    {
        await base.Comparing_different_entity_types_using_Equals(async);

        AssertSql(
            """
SELECT [c].[CustomerID]
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
WHERE 0 = 1
""");
    }

    public override async Task Comparing_entity_to_null_using_Equals(bool async)
    {
        await base.Comparing_entity_to_null_using_Equals(async);

        AssertSql(
            """
SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Comparing_navigations_using_Equals(bool async)
    {
        await base.Comparing_navigations_using_Equals(async);

        AssertSql(
            """
SELECT [o].[OrderID] AS [Id1], [o0].[OrderID] AS [Id2]
FROM [Orders] AS [o]
CROSS JOIN [Orders] AS [o0]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
LEFT JOIN [Customers] AS [c0] ON [o0].[CustomerID] = [c0].[CustomerID]
WHERE [o].[CustomerID] LIKE N'A%' AND ([c].[CustomerID] = [c0].[CustomerID] OR ([c].[CustomerID] IS NULL AND [c0].[CustomerID] IS NULL))
ORDER BY [o].[OrderID], [o0].[OrderID]
""");
    }

    public override async Task Comparing_navigations_using_static_Equals(bool async)
    {
        await base.Comparing_navigations_using_static_Equals(async);

        AssertSql(
            """
SELECT [o].[OrderID] AS [Id1], [o0].[OrderID] AS [Id2]
FROM [Orders] AS [o]
CROSS JOIN [Orders] AS [o0]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
LEFT JOIN [Customers] AS [c0] ON [o0].[CustomerID] = [c0].[CustomerID]
WHERE [o].[CustomerID] LIKE N'A%' AND ([c].[CustomerID] = [c0].[CustomerID] OR ([c].[CustomerID] IS NULL AND [c0].[CustomerID] IS NULL))
ORDER BY [o].[OrderID], [o0].[OrderID]
""");
    }

    public override async Task Comparing_non_matching_entities_using_Equals(bool async)
    {
        await base.Comparing_non_matching_entities_using_Equals(async);

        AssertSql(
            """
SELECT [c].[CustomerID] AS [Id1], [o].[OrderID] AS [Id2]
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
WHERE 0 = 1
""");
    }

    public override async Task Comparing_non_matching_collection_navigations_using_Equals(bool async)
    {
        await base.Comparing_non_matching_collection_navigations_using_Equals(async);

        AssertSql(
            """
SELECT [c].[CustomerID] AS [Id1], [o].[OrderID] AS [Id2]
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
WHERE 0 = 1
""");
    }

    public override async Task Comparing_collection_navigation_to_null(bool async)
    {
        await base.Comparing_collection_navigation_to_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE 0 = 1
""");
    }

    public override async Task Comparing_collection_navigation_to_null_complex(bool async)
    {
        await base.Comparing_collection_navigation_to_null_complex(async);

        AssertSql(
            """
SELECT [o].[ProductID], [o].[OrderID]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] < 10250 AND [c].[CustomerID] IS NOT NULL
ORDER BY [o].[OrderID], [o].[ProductID]
""");
    }

    public override async Task Compare_collection_navigation_with_itself(bool async)
    {
        await base.Compare_collection_navigation_with_itself(async);

        AssertSql(
            """
SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
""");
    }

    public override async Task Compare_two_collection_navigations_with_different_query_sources(bool async)
    {
        await base.Compare_two_collection_navigations_with_different_query_sources(async);

        AssertSql(
            """
SELECT [c].[CustomerID] AS [Id1], [c0].[CustomerID] AS [Id2]
FROM [Customers] AS [c]
CROSS JOIN [Customers] AS [c0]
WHERE [c].[CustomerID] = N'ALFKI' AND [c0].[CustomerID] = N'ALFKI' AND [c].[CustomerID] = [c0].[CustomerID]
""");
    }

    public override async Task Compare_two_collection_navigations_using_equals(bool async)
    {
        await base.Compare_two_collection_navigations_using_equals(async);

        AssertSql(
            """
SELECT [c].[CustomerID] AS [Id1], [c0].[CustomerID] AS [Id2]
FROM [Customers] AS [c]
CROSS JOIN [Customers] AS [c0]
WHERE [c].[CustomerID] = N'ALFKI' AND [c0].[CustomerID] = N'ALFKI' AND [c].[CustomerID] = [c0].[CustomerID]
""");
    }

    public override async Task Compare_two_collection_navigations_with_different_property_chains(bool async)
    {
        await base.Compare_two_collection_navigations_with_different_property_chains(async);

        AssertSql(
            """
SELECT [c].[CustomerID] AS [Id1], [o].[OrderID] AS [Id2]
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
LEFT JOIN [Customers] AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI' AND [c].[CustomerID] = [c0].[CustomerID]
ORDER BY [c].[CustomerID], [o].[OrderID]
""");
    }

    public override async Task OrderBy_ThenBy_same_column_different_direction(bool async)
    {
        await base.OrderBy_ThenBy_same_column_different_direction(async);

        AssertSql(
            """
SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task OrderBy_OrderBy_same_column_different_direction(bool async)
    {
        await base.OrderBy_OrderBy_same_column_different_direction(async);

        AssertSql(
            """
SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID] DESC
""");
    }

    public override async Task Complex_nested_query_doesnt_try_binding_to_grandparent_when_parent_returns_complex_result(bool async)
    {
        await base.Complex_nested_query_doesnt_try_binding_to_grandparent_when_parent_returns_complex_result(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o1].[InnerOrder], [o1].[Id], [o1].[OrderID]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT (
        SELECT COUNT(*)
        FROM [Orders] AS [o0]
        WHERE [c].[CustomerID] = [o0].[CustomerID]) AS [InnerOrder], [c].[CustomerID] AS [Id], [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [o1]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Complex_nested_query_properly_binds_to_grandparent_when_parent_returns_scalar_result(bool async)
    {
        await base.Complex_nested_query_properly_binds_to_grandparent_when_parent_returns_scalar_result(async);

        AssertSql(
            """
SELECT [c].[CustomerID], (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID] AND (
        SELECT COUNT(*)
        FROM [Orders] AS [o0]
        WHERE [c].[CustomerID] = [o0].[CustomerID]) > 0) AS [OuterOrders]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task OrderBy_Dto_projection_skip_take(bool async)
    {
        await base.OrderBy_Dto_projection_skip_take(async);

        AssertSql(
            """
@__p_0='5'
@__p_1='10'

SELECT [c].[CustomerID] AS [Id]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
""");
    }

    public override async Task Join_take_count_works(bool async)
    {
        await base.Join_take_count_works(async);

        AssertSql(
            """
@__p_0='5'

SELECT COUNT(*)
FROM (
    SELECT TOP(@__p_0) 1 AS empty
    FROM [Orders] AS [o]
    INNER JOIN (
        SELECT [c].[CustomerID]
        FROM [Customers] AS [c]
        WHERE [c].[CustomerID] = N'ALFKI'
    ) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
    WHERE [o].[OrderID] > 690 AND [o].[OrderID] < 710
) AS [s]
""");
    }

    public override async Task OrderBy_empty_list_contains(bool async)
    {
        await base.OrderBy_empty_list_contains(async);

        AssertSql(
            """
@__list_0='[]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY CASE
    WHEN [c].[CustomerID] IN (
        SELECT [l].[value]
        FROM OPENJSON(@__list_0) WITH ([value] nchar(5) '$') AS [l]
    ) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task OrderBy_empty_list_does_not_contains(bool async)
    {
        await base.OrderBy_empty_list_does_not_contains(async);

        AssertSql(
            """
@__list_0='[]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY CASE
    WHEN [c].[CustomerID] NOT IN (
        SELECT [l].[value]
        FROM OPENJSON(@__list_0) WITH ([value] nchar(5) '$') AS [l]
    ) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Manual_expression_tree_typed_null_equality(bool async)
    {
        await base.Manual_expression_tree_typed_null_equality(async);

        AssertSql(
            """
SELECT [c].[City]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] < 10300
""");
    }

    public override async Task Let_subquery_with_multiple_occurrences(bool async)
    {
        await base.Let_subquery_with_multiple_occurrences(async);

        AssertSql(
            """
SELECT (
    SELECT COUNT(*)
    FROM [Order Details] AS [o1]
    WHERE [o].[OrderID] = [o1].[OrderID] AND [o1].[Quantity] < CAST(10 AS smallint)) AS [Count]
FROM [Orders] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM [Order Details] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID] AND [o0].[Quantity] < CAST(10 AS smallint))
""");
    }

    public override async Task Let_entity_equality_to_null(bool async)
    {
        await base.Let_entity_equality_to_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], (
    SELECT TOP(1) [o0].[OrderDate]
    FROM [Orders] AS [o0]
    WHERE [c].[CustomerID] = [o0].[CustomerID]
    ORDER BY [o0].[OrderDate]) AS [OrderDate]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%' AND EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
""");
    }

    public override async Task Let_entity_equality_to_other_entity(bool async)
    {
        await base.Let_entity_equality_to_other_entity(async);

        AssertSql(
            """
SELECT [c].[CustomerID], (
    SELECT TOP(1) [o0].[OrderDate]
    FROM [Orders] AS [o0]
    WHERE [c].[CustomerID] = [o0].[CustomerID]
    ORDER BY [o0].[OrderDate]) AS [A]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%' AND ((
    SELECT TOP(1) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderDate]) <> 0 OR (
    SELECT TOP(1) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderDate]) IS NULL)
""");
    }

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

    public override async Task Collection_navigation_equality_rewrite_for_subquery(bool async)
    {
        await base.Collection_navigation_equality_rewrite_for_subquery(async);

        AssertSql();
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
    WHERE [c].[CustomerID] = [o].[CustomerID] AND [c].[CustomerID] = [o].[CustomerID]) > 0
""");
    }

    public override async Task Convert_to_nullable_on_nullable_value_is_ignored(bool async)
    {
        await base.Convert_to_nullable_on_nullable_value_is_ignored(async);

        AssertSql(
            """
SELECT [o].[OrderDate]
FROM [Orders] AS [o]
""");
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

    public override async Task OrderBy_object_type_server_evals(bool async)
    {
        await base.OrderBy_object_type_server_evals(async);

        AssertSql(
            """
@__p_0='0'
@__p_1='20'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [o].[OrderID], [o].[OrderDate], [c].[CustomerID], [c].[City]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
""");
    }

    public override async Task AsQueryable_in_query_server_evals(bool async)
    {
        await base.AsQueryable_in_query_server_evals(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o1].[OrderDate], [o1].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o0].[OrderDate], [o0].[OrderID], [o0].[CustomerID]
    FROM (
        SELECT [o].[OrderDate], [o].[OrderID], [o].[CustomerID], ROW_NUMBER() OVER(PARTITION BY [o].[CustomerID] ORDER BY [o].[OrderID]) AS [row]
        FROM [Orders] AS [o]
        WHERE DATEPART(year, [o].[OrderDate]) = 1998
    ) AS [o0]
    WHERE [o0].[row] <= 1
) AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
ORDER BY [c].[CustomerID], [o1].[CustomerID], [o1].[OrderID]
""");
    }

    public override async Task Subquery_DefaultIfEmpty_Any(bool async)
    {
        await base.Subquery_DefaultIfEmpty_Any(async);

        AssertSql(
            """
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM (
            SELECT 1 AS empty
        ) AS [e0]
        LEFT JOIN (
            SELECT 1 AS empty
            FROM [Employees] AS [e]
            WHERE [e].[EmployeeID] = -1
        ) AS [e1] ON 1 = 1) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Projection_skip_collection_projection(bool async)
    {
        await base.Projection_skip_collection_projection(async);

        AssertSql(
            """
@__p_0='5'

SELECT [o1].[OrderID], [o0].[ProductID], [o0].[OrderID]
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS
) AS [o1]
LEFT JOIN [Order Details] AS [o0] ON [o1].[OrderID] = [o0].[OrderID]
ORDER BY [o1].[OrderID], [o0].[OrderID]
""");
    }

    public override async Task Projection_take_collection_projection(bool async)
    {
        await base.Projection_take_collection_projection(async);

        AssertSql(
            """
@__p_0='10'

SELECT [o1].[OrderID], [o0].[ProductID], [o0].[OrderID]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
    ORDER BY [o].[OrderID]
) AS [o1]
LEFT JOIN [Order Details] AS [o0] ON [o1].[OrderID] = [o0].[OrderID]
ORDER BY [o1].[OrderID], [o0].[OrderID]
""");
    }

    public override async Task Projection_skip_take_collection_projection(bool async)
    {
        await base.Projection_skip_take_collection_projection(async);

        AssertSql(
            """
@__p_0='5'
@__p_1='10'

SELECT [o1].[OrderID], [o0].[ProductID], [o0].[OrderID]
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [o1]
LEFT JOIN [Order Details] AS [o0] ON [o1].[OrderID] = [o0].[OrderID]
ORDER BY [o1].[OrderID], [o0].[OrderID]
""");
    }

    public override async Task Projection_skip_projection(bool async)
    {
        await base.Projection_skip_projection(async);

        AssertSql(
            """
@__p_0='5'

SELECT [c].[City]
FROM (
    SELECT [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS
) AS [o0]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
ORDER BY [o0].[OrderID]
""");
    }

    public override async Task Projection_take_projection(bool async)
    {
        await base.Projection_take_projection(async);

        AssertSql(
            """
@__p_0='10'

SELECT [c].[City]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
    ORDER BY [o].[OrderID]
) AS [o0]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
ORDER BY [o0].[OrderID]
""");
    }

    public override async Task Projection_skip_take_projection(bool async)
    {
        await base.Projection_skip_take_projection(async);

        AssertSql(
            """
@__p_0='5'
@__p_1='10'

SELECT [c].[City]
FROM (
    SELECT [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [o0]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
ORDER BY [o0].[OrderID]
""");
    }

    public override async Task Collection_projection_skip(bool async)
    {
        await base.Collection_projection_skip(async);

        AssertSql(
            """
@__p_0='5'

SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS
) AS [o1]
LEFT JOIN [Order Details] AS [o0] ON [o1].[OrderID] = [o0].[OrderID]
ORDER BY [o1].[OrderID], [o0].[OrderID]
""");
    }

    public override async Task Collection_projection_take(bool async)
    {
        await base.Collection_projection_take(async);

        AssertSql(
            """
@__p_0='10'

SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
    ORDER BY [o].[OrderID]
) AS [o1]
LEFT JOIN [Order Details] AS [o0] ON [o1].[OrderID] = [o0].[OrderID]
ORDER BY [o1].[OrderID], [o0].[OrderID]
""");
    }

    public override async Task Collection_projection_skip_take(bool async)
    {
        await base.Collection_projection_skip_take(async);

        AssertSql(
            """
@__p_0='5'
@__p_1='10'

SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [o1]
LEFT JOIN [Order Details] AS [o0] ON [o1].[OrderID] = [o0].[OrderID]
ORDER BY [o1].[OrderID], [o0].[OrderID]
""");
    }

    public override async Task Anonymous_projection_skip_empty_collection_FirstOrDefault(bool async)
    {
        await base.Anonymous_projection_skip_empty_collection_FirstOrDefault(async);

        AssertSql(
            """
@__p_0='0'

SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate]
FROM (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'FISSA'
    ORDER BY (SELECT 1)
    OFFSET @__p_0 ROWS
) AS [c0]
LEFT JOIN (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM (
        SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], ROW_NUMBER() OVER(PARTITION BY [o].[CustomerID] ORDER BY [o].[OrderID]) AS [row]
        FROM [Orders] AS [o]
    ) AS [o0]
    WHERE [o0].[row] <= 1
) AS [o1] ON [c0].[CustomerID] = [o1].[CustomerID]
""");
    }

    public override async Task Anonymous_projection_take_empty_collection_FirstOrDefault(bool async)
    {
        await base.Anonymous_projection_take_empty_collection_FirstOrDefault(async);

        AssertSql(
            """
@__p_0='1'

SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'FISSA'
) AS [c0]
LEFT JOIN (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM (
        SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], ROW_NUMBER() OVER(PARTITION BY [o].[CustomerID] ORDER BY [o].[OrderID]) AS [row]
        FROM [Orders] AS [o]
    ) AS [o0]
    WHERE [o0].[row] <= 1
) AS [o1] ON [c0].[CustomerID] = [o1].[CustomerID]
""");
    }

    public override async Task Anonymous_projection_skip_take_empty_collection_FirstOrDefault(bool async)
    {
        await base.Anonymous_projection_skip_take_empty_collection_FirstOrDefault(async);

        AssertSql(
            """
@__p_0='0'
@__p_1='1'

SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate]
FROM (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'FISSA'
    ORDER BY (SELECT 1)
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [c0]
LEFT JOIN (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM (
        SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], ROW_NUMBER() OVER(PARTITION BY [o].[CustomerID] ORDER BY [o].[OrderID]) AS [row]
        FROM [Orders] AS [o]
    ) AS [o0]
    WHERE [o0].[row] <= 1
) AS [o1] ON [c0].[CustomerID] = [o1].[CustomerID]
""");
    }

    public override async Task Checked_context_with_arithmetic_does_not_fail(bool isAsync)
    {
        await base.Checked_context_with_arithmetic_does_not_fail(isAsync);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE [o].[Quantity] + CAST(1 AS smallint) = CAST(5 AS smallint) AND [o].[Quantity] - CAST(1 AS smallint) = CAST(3 AS smallint) AND [o].[Quantity] * CAST(1 AS smallint) = [o].[Quantity]
ORDER BY [o].[OrderID]
""");
    }

    public override async Task Checked_context_with_case_to_same_nullable_type_does_not_fail(bool isAsync)
    {
        await base.Checked_context_with_case_to_same_nullable_type_does_not_fail(isAsync);

        AssertSql(
            """
SELECT MAX([o].[Quantity])
FROM [Order Details] AS [o]
""");
    }

    public override async Task Entity_equality_with_null_coalesce_client_side(bool async)
    {
        await base.Entity_equality_with_null_coalesce_client_side(async);

        AssertSql(
            """
@__entity_equality_a_0_CustomerID='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__entity_equality_a_0_CustomerID
""");
    }

    public override async Task Entity_equality_contains_with_list_of_null(bool async)
    {
        await base.Entity_equality_contains_with_list_of_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task MemberInitExpression_NewExpression_is_funcletized_even_when_bindings_are_not_evaluatable(bool async)
    {
        await base.MemberInitExpression_NewExpression_is_funcletized_even_when_bindings_are_not_evaluatable(async);

        AssertSql(
            """
SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
""");
    }

    public override async Task Projecting_collection_split(bool async)
    {
        await base.Projecting_collection_split(async);

        AssertSql(
            """
SELECT [c].[CustomerID]
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

    public override async Task Projecting_collection_then_include_split(bool async)
    {
        await base.Projecting_collection_then_include_split(async);

        AssertSql(
            """
SELECT [c].[CustomerID]
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

    public override async Task Single_non_scalar_projection_after_skip_uses_join(bool async)
    {
        await base.Single_non_scalar_projection_after_skip_uses_join(async);

        AssertSql(
            """
SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM (
        SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], ROW_NUMBER() OVER(PARTITION BY [o].[CustomerID] ORDER BY [o].[OrderDate], [o].[OrderID]) AS [row]
        FROM [Orders] AS [o]
    ) AS [o0]
    WHERE 2 < [o0].[row] AND [o0].[row] <= 3
) AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
""");
    }

    public override async Task Select_distinct_Select_with_client_bindings(bool async)
    {
        await base.Select_distinct_Select_with_client_bindings(async);

        AssertSql(
            """
SELECT [o0].[c]
FROM (
    SELECT DISTINCT DATEPART(year, [o].[OrderDate]) AS [c]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 20000
) AS [o0]
""");
    }

    public override async Task ToList_over_string(bool async)
    {
        await base.ToList_over_string(async);

        AssertSql(
            """
SELECT [c].[City]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task ToArray_over_string(bool async)
    {
        await base.ToArray_over_string(async);

        AssertSql(
            """
SELECT [c].[City]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task AsEnumerable_over_string(bool async)
    {
        await base.AsEnumerable_over_string(async);

        AssertSql(
            """
SELECT [c].[City]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Pending_selector_in_cardinality_reducing_method_is_applied_before_expanding_collection_navigation_member(
        bool async)
    {
        await base.Pending_selector_in_cardinality_reducing_method_is_applied_before_expanding_collection_navigation_member(async);

        AssertSql(
            """
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE (
            SELECT TOP(1) [c0].[CustomerID]
            FROM [Orders] AS [o0]
            LEFT JOIN [Customers] AS [c0] ON [o0].[CustomerID] = [c0].[CustomerID]
            WHERE [c].[CustomerID] = [o0].[CustomerID]
            ORDER BY [o0].[OrderDate]) IS NOT NULL AND ((
            SELECT TOP(1) [c1].[CustomerID]
            FROM [Orders] AS [o1]
            LEFT JOIN [Customers] AS [c1] ON [o1].[CustomerID] = [c1].[CustomerID]
            WHERE [c].[CustomerID] = [o1].[CustomerID]
            ORDER BY [o1].[OrderDate]) = [o].[CustomerID] OR ((
            SELECT TOP(1) [c1].[CustomerID]
            FROM [Orders] AS [o1]
            LEFT JOIN [Customers] AS [c1] ON [o1].[CustomerID] = [c1].[CustomerID]
            WHERE [c].[CustomerID] = [o1].[CustomerID]
            ORDER BY [o1].[OrderDate]) IS NULL AND [o].[CustomerID] IS NULL)) AND [o].[OrderID] < 11000) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Complex]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Distinct_followed_by_ordering_on_condition(bool async)
    {
        await base.Distinct_followed_by_ordering_on_condition(async);

        AssertSql(
            """
@__p_1='5'
@__searchTerm_0='c' (Size = 15)

SELECT TOP(@__p_1) [c0].[City]
FROM (
    SELECT DISTINCT [c].[City]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] NOT IN (N'VAFFE', N'DRACD')
) AS [c0]
ORDER BY CASE
    WHEN @__searchTerm_0 = N'' THEN 0
    ELSE CHARINDEX(@__searchTerm_0, [c0].[City]) - 1
END, [c0].[City]
""");
    }

    public override async Task DefaultIfEmpty_Sum_over_collection_navigation(bool async)
    {
        await base.DefaultIfEmpty_Sum_over_collection_navigation(async);

        AssertSql(
            """
SELECT [c].[CustomerID], (
    SELECT COALESCE(SUM(COALESCE([o0].[OrderID], 0)), 0)
    FROM (
        SELECT 1 AS empty
    ) AS [e]
    LEFT JOIN (
        SELECT [o].[OrderID]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
    ) AS [o0] ON 1 = 1) AS [Sum]
FROM [Customers] AS [c]
""");
    }

    public override async Task Entity_equality_on_subquery_with_null_check(bool async)
    {
        await base.Entity_equality_on_subquery_with_null_check(async);

        AssertSql(
            """
SELECT [c].[CustomerID], CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]) OR NOT EXISTS (
        SELECT 1
        FROM [Orders] AS [o0]
        WHERE [c].[CustomerID] = [o0].[CustomerID]) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, (
    SELECT TOP(1) [o1].[OrderDate]
    FROM [Orders] AS [o1]
    WHERE [c].[CustomerID] = [o1].[CustomerID]
    ORDER BY [o1].[OrderID])
FROM [Customers] AS [c]
""");
    }

    public override async Task DefaultIfEmpty_over_empty_collection_followed_by_projecting_constant(bool async)
    {
        await base.DefaultIfEmpty_over_empty_collection_followed_by_projecting_constant(async);

        AssertSql(
            """
SELECT TOP(1) N'520'
FROM (
    SELECT 1 AS empty
) AS [e]
LEFT JOIN (
    SELECT 1 AS empty
    FROM [Customers] AS [c]
    WHERE 0 = 1
) AS [c0] ON 1 = 1
""");
    }

    public override async Task FirstOrDefault_with_predicate_nested(bool async)
    {
        await base.FirstOrDefault_with_predicate_nested(async);

        AssertSql(
            """
SELECT [c].[CustomerID], (
    SELECT TOP(1) [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]) AS [OrderDate]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task First_on_collection_in_projection(bool async)
    {
        await base.First_on_collection_in_projection(async);

        AssertSql(
            """
SELECT [c].[CustomerID], CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]) THEN (
        SELECT TOP(1) [o0].[OrderDate]
        FROM [Orders] AS [o0]
        WHERE [c].[CustomerID] = [o0].[CustomerID])
    ELSE NULL
END AS [OrderDate]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task SelectMany_correlated_subquery_hard(bool async)
    {
        await base.SelectMany_correlated_subquery_hard(async);

        AssertSql(
            """
@__p_0='91'

SELECT [c1].[City] AS [c1], [e0].[City], [e0].[c1]
FROM (
    SELECT DISTINCT [c0].[City]
    FROM (
        SELECT TOP(@__p_0) [c].[City]
        FROM [Customers] AS [c]
    ) AS [c0]
) AS [c1]
CROSS APPLY (
    SELECT TOP(9) [e].[City], [c1].[City] AS [c1]
    FROM [Employees] AS [e]
    WHERE [c1].[City] = [e].[City] OR ([c1].[City] IS NULL AND [e].[City] IS NULL)
) AS [e0]
CROSS APPLY (
    SELECT TOP(9) 1 AS empty
    FROM [Employees] AS [e1]
    WHERE [e0].[City] = [e1].[City] OR ([e0].[City] IS NULL AND [e1].[City] IS NULL)
) AS [e2]
""");
    }

    public override async Task Skip_0_Take_0_works_when_parameter(bool async)
    {
        await base.Skip_0_Take_0_works_when_parameter(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1
""",
            //
            """
@__p_0='1'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_0 ROWS ONLY
""");
    }

    public override async Task Skip_0_Take_0_works_when_constant(bool async)
    {
        await base.Skip_0_Take_0_works_when_constant(async);

        AssertSql(
            """
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE 0 = 1) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Skip_1_Take_0_works_when_constant(bool async)
    {
        await base.Skip_1_Take_0_works_when_constant(async);

        AssertSql(
            """
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE 0 = 1) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Take_0_works_when_constant(bool async)
    {
        await base.Take_0_works_when_constant(async);

        AssertSql(
            """
SELECT CASE
    WHEN EXISTS (
        SELECT TOP(0) 1
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderID]) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID]
""");
    }

    [ConditionalFact]
    public async Task Single_Predicate_Cancellation()
        => await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () =>
                await Single_Predicate_Cancellation_test(Fixture.TestSqlLoggerFactory.CancelQuery()));

    [ConditionalFact]
    public Task Query_compiler_concurrency()
    {
        const int threadCount = 50;

        var tasks = new Task[threadCount];

        for (var i = 0; i < threadCount; i++)
        {
            tasks[i] = Task.Run(
                () =>
                {
                    using var context = CreateContext();
                    using ((from c in context.Customers
                            where c.City == "London"
                            orderby c.CustomerID
                            select (from o1 in context.Orders
                                    where o1.CustomerID == c.CustomerID
                                        && o1.OrderDate.Value.Year == 1997
                                    orderby o1.OrderID
                                    select (from o2 in context.Orders
                                            where o1.CustomerID == c.CustomerID
                                            orderby o2.OrderID
                                            select o1.OrderID).ToList()).ToList())
                           .GetEnumerator())
                    {
                    }
                });
        }

        return Task.WhenAll(tasks);
    }

    [ConditionalFact]
    public Task Race_when_context_disposed_before_query_termination()
    {
        DbSet<Customer> task;

        using (var context = CreateContext())
        {
            task = context.Customers;
        }

        return Assert.ThrowsAsync<ObjectDisposedException>(() => task.SingleAsync(c => c.CustomerID == "ALFKI"));
    }

    [ConditionalFact]
    public async Task Concurrent_async_queries_are_serialized2()
    {
        using var context = CreateContext();
        await context.OrderDetails
            .Where(od => od.OrderID > 0)
            .Intersect(
                context.OrderDetails
                    .Where(od => od.OrderID > 0))
            .Intersect(
                context.OrderDetails
                    .Where(od => od.OrderID > 0)).ToListAsync();
    }

    [ConditionalFact]
    public async Task Concurrent_async_queries_when_raw_query()
    {
        using var context = CreateContext();
        await using var asyncEnumerator = context.Customers.AsAsyncEnumerable().GetAsyncEnumerator();
        while (await asyncEnumerator.MoveNextAsync())
        {
            // Outer query is buffered by default
            await context.Database.ExecuteSqlRawAsync(
                "[dbo].[CustOrderHist] @CustomerID = {0}",
                asyncEnumerator.Current.CustomerID);
        }
    }

    public override async Task Correlated_collection_with_distinct_without_default_identifiers_projecting_columns(bool async)
    {
        await base.Correlated_collection_with_distinct_without_default_identifiers_projecting_columns(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o0].[First], [o0].[Second]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT DISTINCT [o].[OrderID] AS [First], [o].[OrderDate] AS [Second]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [o0]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Correlated_collection_with_distinct_without_default_identifiers_projecting_columns_with_navigation(
        bool async)
    {
        await base.Correlated_collection_with_distinct_without_default_identifiers_projecting_columns_with_navigation(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [s].[First], [s].[Second], [s].[Third]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT DISTINCT [o].[OrderID] AS [First], [o].[OrderDate] AS [Second], [c0].[City] AS [Third]
    FROM [Orders] AS [o]
    LEFT JOIN [Customers] AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [s]
ORDER BY [c].[CustomerID], [s].[First], [s].[Second]
""");
    }

    public override async Task Select_nested_collection_with_distinct(bool async)
    {
        await base.Select_nested_collection_with_distinct(async);

        AssertSql(
            """
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [c].[CustomerID], [o1].[CustomerID]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT DISTINCT [o0].[CustomerID]
    FROM [Orders] AS [o0]
) AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task SelectMany_primitive_select_subquery(bool async)
    {
        await base.SelectMany_primitive_select_subquery(async);

        AssertSql(
            """
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Employees] AS [e]) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""",
            //
            """
@__Any_0='True'

SELECT @__Any_0
FROM [Employees] AS [e]
CROSS JOIN [Employees] AS [e0]
""");
    }

    public override async Task Throws_on_concurrent_query_first(bool async)
    {
        await base.Throws_on_concurrent_query_first(async);

        AssertSql(
            """
SELECT 1
""",
            //
            """

IF EXISTS
    (SELECT *
     FROM [sys].[objects] o
     WHERE [o].[type] = 'U'
     AND [o].[is_ms_shipped] = 0
     AND NOT EXISTS (SELECT *
         FROM [sys].[extended_properties] AS [ep]
         WHERE [ep].[major_id] = [o].[object_id]
             AND [ep].[minor_id] = 0
             AND [ep].[class] = 1
             AND [ep].[name] = N'microsoft_database_tools_support'
    )
)
SELECT 1 ELSE SELECT 0
""",
            //
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task Non_nullable_property_through_optional_navigation(bool async)
    {
        await base.Non_nullable_property_through_optional_navigation(async);

        AssertSql(
            """
SELECT CAST(LEN([c].[Region]) AS int) AS [Length]
FROM [Customers] AS [c]
""");
    }

    public override async Task OrderByDescending(bool async)
    {
        await base.OrderByDescending(async);

        AssertSql(
            """
SELECT [c].[City]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID] DESC
""");
    }

    public override async Task Take_Distinct(bool async)
    {
        await base.Take_Distinct(async);

        AssertSql(
            """
@__p_0='5'

SELECT DISTINCT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [o0]
""");
    }

    public override async Task Perform_identity_resolution_reuses_same_instances(bool async, bool useAsTracking)
    {
        await base.Perform_identity_resolution_reuses_same_instances(async, useAsTracking);

        AssertSql(
            """
SELECT [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
""",
            //
            """
@__orderIds_0='[10643,10692,10702,10835,10952,11011]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] IN (
    SELECT [o0].[value]
    FROM OPENJSON(@__orderIds_0) WITH ([value] int '$') AS [o0]
)
""");
    }

    public override async Task Context_based_client_method(bool async)
    {
        await base.Context_based_client_method(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""",
            //
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task Select_nested_collection_in_anonymous_type(bool async)
    {
        await base.Select_nested_collection_in_anonymous_type(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o0].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE DATEPART(year, [o].[OrderDate]) = 1997
) AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID], [o0].[OrderID]
""");
    }

    public override async Task OrderBy_Select(bool async)
    {
        await base.OrderBy_Select(async);

        AssertSql(
            """
SELECT [c].[ContactName]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task OrderBy_ThenBy_predicate(bool async)
    {
        await base.OrderBy_ThenBy_predicate(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[City], [c].[CustomerID]
""");
    }

    public override async Task Query_when_evaluatable_queryable_method_call_with_repository(bool async)
    {
        await base.Query_when_evaluatable_queryable_method_call_with_repository(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = [c].[CustomerID])
""",
            //
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = [c].[CustomerID])
""",
            //
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = [c].[CustomerID])
""");
    }

    public override async Task Max_on_empty_sequence_throws(bool async)
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => base.Max_on_empty_sequence_throws(async));

        AssertSql(
            """
SELECT (
    SELECT MAX([o].[OrderID])
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]) AS [Max]
FROM [Customers] AS [c]
""");
    }

    public override async Task OrderBy_Join(bool async)
    {
        await base.OrderBy_Join(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Where_Property_shadow_closure(bool async)
    {
        await base.Where_Property_shadow_closure(async);

        AssertSql(
            """
@__value_0='Sales Representative' (Size = 30)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[Title] = @__value_0
""",
            //
            """
@__value_0='Steven' (Size = 10)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[FirstName] = @__value_0
""");
    }

    public override async Task SelectMany_customer_orders(bool async)
    {
        await base.SelectMany_customer_orders(async);

        AssertSql(
            """
SELECT [c].[ContactName], [o].[OrderID]
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
WHERE [c].[CustomerID] = [o].[CustomerID]
""");
    }

    public override async Task Throws_on_concurrent_query_list(bool async)
    {
        await base.Throws_on_concurrent_query_list(async);

        AssertSql(
            """
SELECT 1
""",
            //
            """

IF EXISTS
    (SELECT *
     FROM [sys].[objects] o
     WHERE [o].[type] = 'U'
     AND [o].[is_ms_shipped] = 0
     AND NOT EXISTS (SELECT *
         FROM [sys].[extended_properties] AS [ep]
         WHERE [ep].[major_id] = [o].[object_id]
             AND [ep].[minor_id] = 0
             AND [ep].[class] = 1
             AND [ep].[name] = N'microsoft_database_tools_support'
    )
)
SELECT 1 ELSE SELECT 0
""",
            //
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task Select_Property_when_shadow(bool async)
    {
        await base.Select_Property_when_shadow(async);

        AssertSql(
            """
SELECT [e].[Title]
FROM [Employees] AS [e]
""");
    }

    public override async Task Select_Property_when_non_shadow(bool async)
    {
        await base.Select_Property_when_non_shadow(async);

        AssertSql(
            """
SELECT [o].[OrderID]
FROM [Orders] AS [o]
""");
    }

    public override async Task OrderByDescending_ThenBy(bool async)
    {
        await base.OrderByDescending_ThenBy(async);

        AssertSql(
            """
SELECT [c].[City]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID] DESC, [c].[Country]
""");
    }

    public override async Task SelectMany_correlated_subquery_simple(bool async)
    {
        await base.SelectMany_correlated_subquery_simple(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
INNER JOIN [Employees] AS [e] ON [c].[City] = [e].[City]
ORDER BY [c].[CustomerID], [e].[EmployeeID]
""");
    }

    public override async Task Select_Property_when_shadow_unconstrained_generic_method(bool async)
    {
        await base.Select_Property_when_shadow_unconstrained_generic_method(async);

        AssertSql(
            """
SELECT [e].[Title]
FROM [Employees] AS [e]
""");
    }

    public override async Task Where_Property_when_shadow(bool async)
    {
        await base.Where_Property_when_shadow(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[Title] = N'Sales Representative'
""");
    }

    public override async Task Where_Property_when_shadow_unconstrained_generic_method(bool async)
    {
        await base.Where_Property_when_shadow_unconstrained_generic_method(async);

        AssertSql(
            """
@__value_0='Sales Representative' (Size = 30)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[Title] = @__value_0
""");
    }

    public override async Task Perform_identity_resolution_reuses_same_instances_across_joins(bool async, bool useAsTracking)
    {
        await base.Perform_identity_resolution_reuses_same_instances_across_joins(async, useAsTracking);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10500
) AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
LEFT JOIN [Customers] AS [c0] ON [o0].[CustomerID] = [c0].[CustomerID]
WHERE [c].[CustomerID] LIKE N'A%'
""");
    }

    public override async Task OrderBy_scalar_primitive(bool async)
    {
        await base.OrderBy_scalar_primitive(async);

        AssertSql(
            """
SELECT [e].[EmployeeID]
FROM [Employees] AS [e]
ORDER BY [e].[EmployeeID]
""");
    }

    public override async Task Where_Property_when_non_shadow(bool async)
    {
        await base.Where_Property_when_non_shadow(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] = 10248
""");
    }

    public override async Task OrderByDescending_ThenByDescending(bool async)
    {
        await base.OrderByDescending_ThenByDescending(async);

        AssertSql(
            """
SELECT [c].[City]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID] DESC, [c].[Country] DESC
""");
    }

    public override async Task Load_should_track_results(bool async)
    {
        await base.Load_should_track_results(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task SelectMany_nested_simple(bool async)
    {
        await base.SelectMany_nested_simple(async);

        AssertSql(
            """
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c]
CROSS JOIN [Customers] AS [c0]
ORDER BY [c0].[CustomerID]
""");
    }

    public override async Task Null_parameter_name_works(bool async)
    {
        await base.Null_parameter_name_works(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1
""");
    }

    public override async Task Where_subquery_expression(bool async)
    {
        await base.Where_subquery_expression(async);

        AssertSql(
            """
SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
""",
            //
            """
@__firstOrder_OrderID_0='10248'

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE [o].[OrderID] = @__firstOrder_OrderID_0) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""",
            //
            """
@__Any_0='True'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @__Any_0 = CAST(1 AS bit)
""");
    }

    public override async Task Mixed_sync_async_in_query_cache()
    {
        await base.Mixed_sync_async_in_query_cache();

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""",
            //
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task Select_expression_datetime_add_ticks(bool async)
    {
        await base.Select_expression_datetime_add_ticks(async);

        AssertSql(
            """
SELECT [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL
""");
    }

    public override async Task Where_subquery_expression_same_parametername(bool async)
    {
        await base.Where_subquery_expression_same_parametername(async);

        AssertSql(
            """
SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
ORDER BY [o].[OrderID]
""",
            //
            """
@__firstOrder_OrderID_0='10248'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] = @__firstOrder_OrderID_0 AND ([o0].[CustomerID] = [o].[CustomerID] OR ([o0].[CustomerID] IS NULL AND [o].[CustomerID] IS NULL)))
""");
    }

    public override async Task Cast_results_to_object(bool async)
    {
        await base.Cast_results_to_object(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task Select_subquery_recursive_trivial(bool async)
    {
        await base.Select_subquery_recursive_trivial(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [s].[EmployeeID], [s].[EmployeeID0], [s].[City], [s].[Country], [s].[FirstName], [s].[ReportsTo], [s].[Title]
FROM [Employees] AS [e]
OUTER APPLY (
    SELECT [e0].[EmployeeID], [e1].[EmployeeID] AS [EmployeeID0], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
    FROM [Employees] AS [e0]
    OUTER APPLY [Employees] AS [e1]
) AS [s]
ORDER BY [e].[EmployeeID], [s].[EmployeeID], [s].[EmployeeID0]
""");
    }

    public override async Task SelectMany_primitive(bool async)
    {
        await base.SelectMany_primitive(async);

        AssertSql(
            """
SELECT [e0].[EmployeeID]
FROM [Employees] AS [e]
CROSS JOIN [Employees] AS [e0]
""");
    }

    public override async Task SelectMany_Joined(bool async)
    {
        await base.SelectMany_Joined(async);

        AssertSql(
            """
SELECT [c].[ContactName], [o].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
""");
    }

    // ReSharper disable once RedundantOverriddenMember
    public override async Task ToListAsync_can_be_canceled()
        // May or may not generate SQL depending on when cancellation happens.
        => await base.ToListAsync_can_be_canceled();

    public override async Task OrderBy_ThenBy(bool async)
    {
        await base.OrderBy_ThenBy(async);

        AssertSql(
            """
SELECT [c].[City]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID], [c].[Country]
""");
    }

    public override async Task Collection_projection_after_DefaultIfEmpty(bool async)
    {
        await base.Collection_projection_after_DefaultIfEmpty(async);

        AssertSql(
            """
SELECT [c0].[CustomerID], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT 1 AS empty
) AS [e]
LEFT JOIN (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'Seattle'
) AS [c0] ON 1 = 1
LEFT JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [c0].[CustomerID]
""");
    }

    public override async Task SelectMany_correlated_simple(bool async)
    {
        await base.SelectMany_correlated_simple(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] = [e].[City] OR ([c].[City] IS NULL AND [e].[City] IS NULL)
ORDER BY [c].[CustomerID], [e].[EmployeeID]
""");
    }

    public override void Query_composition_against_ienumerable_set()
    {
        base.Query_composition_against_ienumerable_set();

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
""");
    }

    public override async Task Using_static_string_Equals_with_StringComparison_throws_informative_error(bool async)
    {
        await base.Using_static_string_Equals_with_StringComparison_throws_informative_error(async);

        AssertSql();
    }

    public override async Task Using_string_Equals_with_StringComparison_throws_informative_error(bool async)
    {
        await base.Using_string_Equals_with_StringComparison_throws_informative_error(async);

        AssertSql();
    }

    public override async Task Random_next_is_not_funcletized_1(bool async)
    {
        await base.Random_next_is_not_funcletized_1(async);

        AssertSql();
    }

    public override async Task Random_next_is_not_funcletized_2(bool async)
    {
        await base.Random_next_is_not_funcletized_2(async);

        AssertSql();
    }

    public override async Task Random_next_is_not_funcletized_3(bool async)
    {
        await base.Random_next_is_not_funcletized_3(async);

        AssertSql();
    }

    public override async Task Random_next_is_not_funcletized_4(bool async)
    {
        await base.Random_next_is_not_funcletized_4(async);

        AssertSql();
    }

    public override async Task Random_next_is_not_funcletized_5(bool async)
    {
        await base.Random_next_is_not_funcletized_5(async);

        AssertSql();
    }

    public override async Task Random_next_is_not_funcletized_6(bool async)
    {
        await base.Random_next_is_not_funcletized_6(async);

        AssertSql();
    }

    public override async Task SelectMany_after_client_method(bool async)
    {
        await base.SelectMany_after_client_method(async);

        AssertSql();
    }

    public override async Task Client_OrderBy_GroupBy_Group_ordering_works(bool async)
    {
        await base.Client_OrderBy_GroupBy_Group_ordering_works(async);

        AssertSql();
    }

    public override async Task Client_code_using_instance_method_throws(bool async)
    {
        Assert.Equal(
            CoreStrings.ClientProjectionCapturingConstantInMethodInstance(
                "Microsoft.EntityFrameworkCore.Query.NorthwindMiscellaneousQuerySqlServerTest",
                "InstanceMethod"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Client_code_using_instance_method_throws(async))).Message);

        AssertSql();
    }

    public override async Task Client_code_using_instance_in_static_method(bool async)
    {
        Assert.Equal(
            CoreStrings.ClientProjectionCapturingConstantInMethodArgument(
                "Microsoft.EntityFrameworkCore.Query.NorthwindMiscellaneousQuerySqlServerTest",
                "StaticMethod"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Client_code_using_instance_in_static_method(async))).Message);

        AssertSql();
    }

    public override async Task Client_code_using_instance_in_anonymous_type(bool async)
    {
        Assert.Equal(
            CoreStrings.ClientProjectionCapturingConstantInTree(
                "Microsoft.EntityFrameworkCore.Query.NorthwindMiscellaneousQuerySqlServerTest"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Client_code_using_instance_in_anonymous_type(async))).Message);

        AssertSql();
    }

    public override async Task Client_code_unknown_method(bool async)
    {
        await AssertTranslationFailedWithDetails(
            () => base.Client_code_unknown_method(async),
            CoreStrings.QueryUnableToTranslateMethod(
                "Microsoft.EntityFrameworkCore.Query.NorthwindMiscellaneousQueryTestBase<Microsoft.EntityFrameworkCore.Query.NorthwindQuerySqlServerFixture<Microsoft.EntityFrameworkCore.TestUtilities.NoopModelCustomizer>>",
                nameof(UnknownMethod)));

        AssertSql();
    }

    public override async Task String_include_on_incorrect_property_throws(bool async)
    {
        await base.String_include_on_incorrect_property_throws(async);

        AssertSql();
    }

    public override async Task SkipWhile_throws_meaningful_exception(bool async)
    {
        await base.SkipWhile_throws_meaningful_exception(async);

        AssertSql();
    }

    public override async Task ToListAsync_with_canceled_token()
    {
        await base.ToListAsync_with_canceled_token();

        AssertSql();
    }

    public override async Task Mixed_sync_async_query()
    {
        await base.Mixed_sync_async_query();

        AssertSql();
    }

    public override async Task Parameter_extraction_can_throw_exception_from_user_code(bool async)
    {
        await base.Parameter_extraction_can_throw_exception_from_user_code(async);

        AssertSql();
    }

    public override async Task Parameter_extraction_can_throw_exception_from_user_code_2(bool async)
    {
        await base.Parameter_extraction_can_throw_exception_from_user_code_2(async);

        AssertSql();
    }

    public override async Task Where_query_composition3(bool async)
    {
        await base.Where_query_composition3(async);

        AssertSql();
    }

    public override async Task Where_query_composition4(bool async)
    {
        await base.Where_query_composition4(async);

        AssertSql();
    }

    public override async Task Where_query_composition5(bool async)
    {
        await base.Where_query_composition5(async);

        AssertSql();
    }

    public override async Task Where_query_composition6(bool async)
    {
        await base.Where_query_composition6(async);

        AssertSql();
    }

    public override async Task SelectMany_mixed(bool async)
    {
        await base.SelectMany_mixed(async);

        AssertSql();
    }

    public override async Task Default_if_empty_top_level_arg(bool async)
    {
        await base.Default_if_empty_top_level_arg(async);

        AssertSql();
    }

    public override async Task Default_if_empty_top_level_arg_followed_by_projecting_constant(bool async)
    {
        await base.Default_if_empty_top_level_arg_followed_by_projecting_constant(async);

        AssertSql();
    }

    public override async Task OrderBy_client_mixed(bool async)
    {
        await base.OrderBy_client_mixed(async);

        AssertSql();
    }

    public override async Task OrderBy_multiple_queries(bool async)
    {
        await base.OrderBy_multiple_queries(async);

        AssertSql();
    }

    public override void Can_cast_CreateQuery_result_to_IQueryable_T_bug_1730()
    {
        base.Can_cast_CreateQuery_result_to_IQueryable_T_bug_1730();

        AssertSql();
    }

    public override async Task IQueryable_captured_variable()
    {
        await base.IQueryable_captured_variable();

        AssertSql(
            """
SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE (
    SELECT COUNT(*)
    FROM [Orders] AS [o]) = 2
""");
    }

    public override async Task Multiple_context_instances(bool async)
    {
        await base.Multiple_context_instances(async);

        AssertSql();
    }

    public override async Task Multiple_context_instances_2(bool async)
    {
        await base.Multiple_context_instances_2(async);

        AssertSql();
    }

    public override async Task Multiple_context_instances_set(bool async)
    {
        await base.Multiple_context_instances_set(async);

        AssertSql();
    }

    public override async Task Multiple_context_instances_parameter(bool async)
    {
        await base.Multiple_context_instances_parameter(async);

        AssertSql();
    }

    public override async Task Entity_equality_through_subquery_composite_key(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Entity_equality_through_subquery_composite_key(async))).Message;

        Assert.Equal(
            CoreStrings.EntityEqualityOnCompositeKeyEntitySubqueryNotSupported("==", nameof(OrderDetail)),
            message);

        AssertSql();
    }

    public override async Task Queryable_reprojection(bool async)
    {
        await base.Queryable_reprojection(async);

        AssertSql();
    }

    public override async Task All_client(bool async)
    {
        await base.All_client(async);

        AssertSql();
    }

    public override async Task All_client_and_server_top_level(bool async)
    {
        await base.All_client_and_server_top_level(async);

        AssertSql();
    }

    public override async Task All_client_or_server_top_level(bool async)
    {
        await base.All_client_or_server_top_level(async);

        AssertSql();
    }

    public override async Task First_client_predicate(bool async)
    {
        await base.First_client_predicate(async);

        AssertSql();
    }

    public override async Task Select_correlated_subquery_filtered_returning_queryable_throws(bool async)
    {
        await base.Select_correlated_subquery_filtered_returning_queryable_throws(async);

        AssertSql();
    }

    public override async Task Select_correlated_subquery_ordered_returning_queryable_throws(bool async)
    {
        await base.Select_correlated_subquery_ordered_returning_queryable_throws(async);

        AssertSql();
    }

    public override async Task Select_correlated_subquery_ordered_returning_queryable_in_DTO_throws(bool async)
    {
        await base.Select_correlated_subquery_ordered_returning_queryable_in_DTO_throws(async);

        AssertSql();
    }

    public override async Task Select_nested_collection_in_anonymous_type_returning_ordered_queryable(bool async)
    {
        await base.Select_nested_collection_in_anonymous_type_returning_ordered_queryable(async);

        AssertSql();
    }

    public override async Task Select_subquery_recursive_trivial_returning_queryable(bool async)
    {
        await base.Select_subquery_recursive_trivial_returning_queryable(async);

        AssertSql();
    }

    public override async Task EF_Property_include_on_incorrect_property_throws(bool async)
    {
        await base.EF_Property_include_on_incorrect_property_throws(async);

        AssertSql();
    }

    public override async Task Collection_navigation_equal_to_null_for_subquery_using_ElementAtOrDefault_constant_zero(bool async)
    {
        await base.Collection_navigation_equal_to_null_for_subquery_using_ElementAtOrDefault_constant_zero(async);

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

    public override async Task Collection_navigation_equal_to_null_for_subquery_using_ElementAtOrDefault_constant_one(bool async)
    {
        await base.Collection_navigation_equal_to_null_for_subquery_using_ElementAtOrDefault_constant_one(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]
    OFFSET 1 ROWS)
""");
    }

    public override async Task Collection_navigation_equal_to_null_for_subquery_using_ElementAtOrDefault_parameter(bool async)
    {
        await base.Collection_navigation_equal_to_null_for_subquery_using_ElementAtOrDefault_parameter(async);

        AssertSql(
            """
@__prm_0='2'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]
    OFFSET @__prm_0 ROWS)
""");
    }

    public override async Task Subquery_with_navigation_inside_inline_collection(bool async)
    {
        await base.Subquery_with_navigation_inside_inline_collection(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT COALESCE(SUM([v].[Value]), 0)
    FROM (VALUES (CAST(100 AS int)), ((
        SELECT COUNT(*)
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]))) AS [v]([Value])) > 101
""");
    }

    public override async Task Parameter_collection_Contains_with_projection_and_ordering(bool async)
    {
        await base.Parameter_collection_Contains_with_projection_and_ordering(async);

        AssertSql(
            """
@__ids_0='[10248,10249]' (Size = 4000)

SELECT [o].[Quantity] AS [Key], (
    SELECT MAX([o1].[OrderDate])
    FROM [Order Details] AS [o0]
    INNER JOIN [Orders] AS [o1] ON [o0].[OrderID] = [o1].[OrderID]
    WHERE [o0].[OrderID] IN (
        SELECT [i0].[value]
        FROM OPENJSON(@__ids_0) WITH ([value] int '$') AS [i0]
    ) AND [o].[Quantity] = [o0].[Quantity]) AS [MaxTimestamp]
FROM [Order Details] AS [o]
WHERE [o].[OrderID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@__ids_0) WITH ([value] int '$') AS [i]
)
GROUP BY [o].[Quantity]
ORDER BY (
    SELECT MAX([o1].[OrderDate])
    FROM [Order Details] AS [o0]
    INNER JOIN [Orders] AS [o1] ON [o0].[OrderID] = [o1].[OrderID]
    WHERE [o0].[OrderID] IN (
        SELECT [i0].[value]
        FROM OPENJSON(@__ids_0) WITH ([value] int '$') AS [i0]
    ) AND [o].[Quantity] = [o0].[Quantity])
""");
    }

    public override async Task Contains_over_concatenated_columns_with_different_sizes(bool async)
    {
        await base.Contains_over_concatenated_columns_with_different_sizes (async);

        AssertSql(
            """
@__data_0='["ALFKIAlfreds Futterkiste","ANATRAna Trujillo Emparedados y helados"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] + [c].[CompanyName] IN (
    SELECT [d].[value]
    FROM OPENJSON(@__data_0) WITH ([value] nvarchar(45) '$') AS [d]
)
""");
    }

    public override async Task Contains_over_concatenated_column_and_constant(bool async)
    {
        await base.Contains_over_concatenated_column_and_constant (async);

        AssertSql(
            """
@__data_0='["ALFKISomeConstant","ANATRSomeConstant","ALFKIX"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] + N'SomeConstant' IN (
    SELECT [d].[value]
    FROM OPENJSON(@__data_0) WITH ([value] nvarchar(max) '$') AS [d]
)
""");
    }

    public override async Task Contains_over_concatenated_columns_both_fixed_length(bool async)
    {
        await base.Contains_over_concatenated_columns_both_fixed_length(async);

        AssertSql(
            """
@__data_0='["ALFKIALFKI","ALFKI","ANATRAna Trujillo Emparedados y helados","ANATRANATR"]' (Size = 4000)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE COALESCE([o].[CustomerID], N'') + COALESCE([c].[CustomerID], N'') IN (
    SELECT [d].[value]
    FROM OPENJSON(@__data_0) WITH ([value] nchar(10) '$') AS [d]
)
""");
    }

    public override async Task Contains_over_concatenated_column_and_parameter(bool async)
    {
        await base.Contains_over_concatenated_column_and_parameter(async);

        AssertSql(
            """
@__someVariable_0='SomeVariable' (Size = 4000)
@__data_1='["ALFKISomeVariable","ANATRSomeVariable","ALFKIX"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] + @__someVariable_0 IN (
    SELECT [d].[value]
    FROM OPENJSON(@__data_1) WITH ([value] nvarchar(max) '$') AS [d]
)
""");
    }

    public override async Task Contains_over_concatenated_parameter_and_constant(bool async)
    {
        await base.Contains_over_concatenated_parameter_and_constant(async);

        AssertSql(
            """
@__Contains_0='True'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE @__Contains_0 = CAST(1 AS bit)
""");
    }

    public override async Task Compiler_generated_local_closure_produces_valid_parameter_name(bool async)
    {
        await base.Compiler_generated_local_closure_produces_valid_parameter_name(async);

        // No AssertSQL since compiler generated variable names are different between local and CI
        //AssertSql("");
    }

    public override async Task Static_member_access_gets_parameterized_within_larger_evaluatable(bool async)
    {
        await base.Static_member_access_gets_parameterized_within_larger_evaluatable(async);

        AssertSql(
            """
@__p_0='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__p_0
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
