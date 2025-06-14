// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindAggregateOperatorsQuerySqlServerTest : NorthwindAggregateOperatorsQueryRelationalTestBase<
    NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
{
    public NorthwindAggregateOperatorsQuerySqlServerTest(
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

    public override async Task Last_when_no_order_by(bool async)
    {
        await base.Last_when_no_order_by(async);

        AssertSql();
    }

    public override async Task LastOrDefault_when_no_order_by(bool async)
    {
        await base.LastOrDefault_when_no_order_by(async);

        AssertSql();
    }

    public override async Task Contains_with_local_tuple_array_closure(bool async)
        => await AssertTranslationFailed(() => base.Contains_with_local_tuple_array_closure(async));

    public override async Task Array_cast_to_IEnumerable_Contains_with_constant(bool async)
    {
        await base.Array_cast_to_IEnumerable_Contains_with_constant(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ALFKI', N'WRONG')
""");
    }

    public override async Task Contains_over_keyless_entity_throws(bool async)
    {
        await base.Contains_over_keyless_entity_throws(async);

        AssertSql(
            """
SELECT TOP(1) [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]
) AS [m]
""");
    }

    public override async Task Enumerable_min_is_mapped_to_Queryable_1(bool async)
    {
        await base.Enumerable_min_is_mapped_to_Queryable_1(async);

        AssertSql(
            """
SELECT (
    SELECT MIN(CAST([o].[OrderID] AS float))
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
FROM [Customers] AS [c]
""");
    }

    public override async Task Enumerable_min_is_mapped_to_Queryable_2(bool async)
    {
        await base.Enumerable_min_is_mapped_to_Queryable_2(async);

        AssertSql(
            """
SELECT (
    SELECT MIN(CAST([o].[OrderID] AS float))
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
FROM [Customers] AS [c]
""");
    }

    public override async Task Average_with_unmapped_property_access_throws_meaningful_exception(bool async)
    {
        await base.Average_with_unmapped_property_access_throws_meaningful_exception(async);

        AssertSql();
    }

    public override async Task Sum_over_empty_returns_zero(bool async)
    {
        await base.Sum_over_empty_returns_zero(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID]), 0)
FROM [Orders] AS [o]
WHERE [o].[OrderID] = 42
""");
    }

    public override async Task Average_over_default_returns_default(bool async)
    {
        await base.Average_over_default_returns_default(async);

        AssertSql(
            """
SELECT AVG(CAST([o].[OrderID] - 10248 AS float))
FROM [Orders] AS [o]
WHERE [o].[OrderID] = 10248
""");
    }

    public override async Task Max_over_default_returns_default(bool async)
    {
        await base.Max_over_default_returns_default(async);

        AssertSql(
            """
SELECT MAX([o].[OrderID] - 10248)
FROM [Orders] AS [o]
WHERE [o].[OrderID] = 10248
""");
    }

    public override async Task Min_over_default_returns_default(bool async)
    {
        await base.Min_over_default_returns_default(async);

        AssertSql(
            """
SELECT MIN([o].[OrderID] - 10248)
FROM [Orders] AS [o]
WHERE [o].[OrderID] = 10248
""");
    }

    public override async Task Average_after_default_if_empty_does_not_throw(bool async)
    {
        await base.Average_after_default_if_empty_does_not_throw(async);

        AssertSql(
            """
SELECT AVG(CAST(COALESCE([o0].[OrderID], 0) AS float))
FROM (
    SELECT 1 AS empty
) AS [e]
LEFT JOIN (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] = 10243
) AS [o0] ON 1 = 1
""");
    }

    public override async Task Max_after_default_if_empty_does_not_throw(bool async)
    {
        await base.Max_after_default_if_empty_does_not_throw(async);

        AssertSql(
            """
SELECT MAX(COALESCE([o0].[OrderID], 0))
FROM (
    SELECT 1 AS empty
) AS [e]
LEFT JOIN (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] = 10243
) AS [o0] ON 1 = 1
""");
    }

    public override async Task Min_after_default_if_empty_does_not_throw(bool async)
    {
        await base.Min_after_default_if_empty_does_not_throw(async);

        AssertSql(
            """
SELECT MIN(COALESCE([o0].[OrderID], 0))
FROM (
    SELECT 1 AS empty
) AS [e]
LEFT JOIN (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] = 10243
) AS [o0] ON 1 = 1
""");
    }

    public override async Task Sum_with_no_data_cast_to_nullable(bool async)
    {
        await base.Sum_with_no_data_cast_to_nullable(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID]), 0)
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 0
""");
    }

    public override async Task Sum_with_no_data_nullable(bool async)
    {
        await base.Sum_with_no_data_nullable(async);

        AssertSql(
            """
SELECT COALESCE(SUM([p].[SupplierID]), 0)
FROM [Products] AS [p]
""");
    }

    public override async Task Sum_with_no_arg_empty(bool async)
    {
        await base.Sum_with_no_arg_empty(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID]), 0)
FROM [Orders] AS [o]
WHERE [o].[OrderID] = 42
""");
    }

    public override async Task Min_no_data(bool async)
    {
        await base.Min_no_data(async);

        AssertSql(
            """
SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]
WHERE [o].[OrderID] = -1
""");
    }

    public override async Task Min_no_data_nullable(bool async)
    {
        await base.Min_no_data_nullable(async);

        AssertSql(
            """
SELECT MIN([p].[SupplierID])
FROM [Products] AS [p]
WHERE [p].[SupplierID] = -1
""");
    }

    public override async Task Min_no_data_cast_to_nullable(bool async)
    {
        await base.Min_no_data_cast_to_nullable(async);

        AssertSql(
            """
SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]
WHERE [o].[OrderID] = -1
""");
    }

    public override async Task Min_no_data_subquery(bool async)
    {
        await base.Min_no_data_subquery(async);

        AssertSql(
            """
SELECT (
    SELECT MIN([o].[OrderID])
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID] AND [o].[OrderID] = -1)
FROM [Customers] AS [c]
""");
    }

    public override async Task Max_no_data(bool async)
    {
        await base.Max_no_data(async);

        AssertSql(
            """
SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]
WHERE [o].[OrderID] = -1
""");
    }

    public override async Task Max_no_data_nullable(bool async)
    {
        await base.Max_no_data_nullable(async);

        AssertSql(
            """
SELECT MAX([p].[SupplierID])
FROM [Products] AS [p]
WHERE [p].[SupplierID] = -1
""");
    }

    public override async Task Max_no_data_cast_to_nullable(bool async)
    {
        await base.Max_no_data_cast_to_nullable(async);

        AssertSql(
            """
SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]
WHERE [o].[OrderID] = -1
""");
    }

    public override async Task Max_no_data_subquery(bool async)
    {
        await base.Max_no_data_subquery(async);

        AssertSql(
            """
SELECT (
    SELECT MAX([o].[OrderID])
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID] AND [o].[OrderID] = -1)
FROM [Customers] AS [c]
""");
    }

    public override async Task Average_no_data(bool async)
    {
        await base.Average_no_data(async);

        AssertSql(
            """
SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
WHERE [o].[OrderID] = -1
""");
    }

    public override async Task Average_no_data_nullable(bool async)
    {
        await base.Average_no_data_nullable(async);

        AssertSql(
            """
SELECT AVG(CAST([p].[SupplierID] AS float))
FROM [Products] AS [p]
WHERE [p].[SupplierID] = -1
""");
    }

    public override async Task Average_no_data_cast_to_nullable(bool async)
    {
        await base.Average_no_data_cast_to_nullable(async);

        AssertSql(
            """
SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
WHERE [o].[OrderID] = -1
""");
    }

    public override async Task Average_no_data_subquery(bool async)
    {
        await base.Average_no_data_subquery(async);

        AssertSql(
            """
SELECT (
    SELECT AVG(CAST([o].[OrderID] AS float))
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID] AND [o].[OrderID] = -1)
FROM [Customers] AS [c]
""");
    }

    public override async Task Count_with_no_predicate(bool async)
    {
        await base.Count_with_no_predicate(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
""");
    }

    public override async Task Count_with_order_by(bool async)
    {
        await base.Count_with_order_by(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
""");
    }

    public override async Task Where_OrderBy_Count_client_eval(bool async)
    {
        await base.Where_OrderBy_Count_client_eval(async);

        AssertSql();
    }

    public override async Task OrderBy_Where_Count_client_eval(bool async)
    {
        await base.OrderBy_Where_Count_client_eval(async);

        AssertSql();
    }

    public override async Task OrderBy_Where_Count_client_eval_mixed(bool async)
    {
        await base.OrderBy_Where_Count_client_eval_mixed(async);

        AssertSql();
    }

    public override async Task OrderBy_Count_with_predicate_client_eval(bool async)
    {
        await base.OrderBy_Count_with_predicate_client_eval(async);

        AssertSql();
    }

    public override async Task OrderBy_Count_with_predicate_client_eval_mixed(bool async)
    {
        await base.OrderBy_Count_with_predicate_client_eval_mixed(async);

        AssertSql();
    }

    public override async Task OrderBy_Where_Count_with_predicate_client_eval(bool async)
    {
        await base.OrderBy_Where_Count_with_predicate_client_eval(async);

        AssertSql();
    }

    public override async Task OrderBy_Where_Count_with_predicate_client_eval_mixed(bool async)
    {
        await base.OrderBy_Where_Count_with_predicate_client_eval_mixed(async);

        AssertSql();
    }

    public override async Task OrderBy_client_Take(bool async)
    {
        await base.OrderBy_client_Take(async);

        AssertSql(
            """
@p='10'

SELECT TOP(@p) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
ORDER BY (SELECT 1)
""");
    }

    public override async Task Single_Throws(bool async)
    {
        await base.Single_Throws(async);

        AssertSql(
            """
SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task Where_Single(bool async)
    {
        await base.Where_Single(async);

        AssertSql(
            """
SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task SingleOrDefault_Throws(bool async)
    {
        await base.SingleOrDefault_Throws(async);

        AssertSql(
            """
SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task SingleOrDefault_Predicate(bool async)
    {
        await base.SingleOrDefault_Predicate(async);

        AssertSql(
            """
SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task Where_SingleOrDefault(bool async)
    {
        await base.Where_SingleOrDefault(async);

        AssertSql(
            """
SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task First(bool async)
    {
        await base.First(async);

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactName]
""");
    }

    public override async Task First_Predicate(bool async)
    {
        await base.First_Predicate(async);

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[ContactName]
""");
    }

    public override async Task Where_First(bool async)
    {
        await base.Where_First(async);

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[ContactName]
""");
    }

    public override async Task FirstOrDefault(bool async)
    {
        await base.FirstOrDefault(async);

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactName]
""");
    }

    public override async Task FirstOrDefault_Predicate(bool async)
    {
        await base.FirstOrDefault_Predicate(async);

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[ContactName]
""");
    }

    public override async Task Where_FirstOrDefault(bool async)
    {
        await base.Where_FirstOrDefault(async);

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[ContactName]
""");
    }

    public override async Task Select_All(bool async)
    {
        await base.Select_All(async);

        AssertSql(
            """
SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE [o].[CustomerID] <> N'ALFKI' OR [o].[CustomerID] IS NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Sum_with_no_arg(bool async)
    {
        await base.Sum_with_no_arg(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID]), 0)
FROM [Orders] AS [o]
""");
    }

    public override async Task Sum_with_binary_expression(bool async)
    {
        await base.Sum_with_binary_expression(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID] * 2), 0)
FROM [Orders] AS [o]
""");
    }

    public override async Task Sum_with_arg(bool async)
    {
        await base.Sum_with_arg(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID]), 0)
FROM [Orders] AS [o]
""");
    }

    public override async Task Sum_with_arg_expression(bool async)
    {
        await base.Sum_with_arg_expression(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID] + [o].[OrderID]), 0)
FROM [Orders] AS [o]
""");
    }

    public override async Task Sum_with_division_on_decimal(bool async)
    {
        await base.Sum_with_division_on_decimal(async);

        AssertSql(
            """
SELECT COALESCE(SUM(CAST([o].[Quantity] AS decimal(18,2)) / 2.09), 0.0)
FROM [Order Details] AS [o]
""");
    }

    public override async Task Sum_with_division_on_decimal_no_significant_digits(bool async)
    {
        await base.Sum_with_division_on_decimal_no_significant_digits(async);

        AssertSql(
            """
SELECT COALESCE(SUM(CAST([o].[Quantity] AS decimal(18,2)) / 2.0), 0.0)
FROM [Order Details] AS [o]
""");
    }

    public override async Task Sum_with_coalesce(bool async)
    {
        await base.Sum_with_coalesce(async);

        AssertSql(
            """
SELECT COALESCE(SUM(COALESCE([p].[UnitPrice], 0.0)), 0.0)
FROM [Products] AS [p]
WHERE [p].[ProductID] < 40
""");
    }

    public override async Task Sum_over_subquery(bool async)
    {
        await base.Sum_over_subquery(async);

        // #34256: rewrite query to avoid "Cannot perform an aggregate function on an expression containing an aggregate or a subquery"
        AssertSql(
            """
SELECT COALESCE(SUM([s].[value]), 0)
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT COALESCE(SUM([o].[OrderID]), 0) AS [value]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [s]
""");
    }

    public override async Task Sum_over_nested_subquery(bool async)
    {
        await base.Sum_over_nested_subquery(async);

        // #34256: rewrite query to avoid "Cannot perform an aggregate function on an expression containing an aggregate or a subquery"
        AssertSql(
            """
SELECT COALESCE(SUM([s0].[value]), 0)
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT COALESCE(SUM([s].[value]), 0) AS [value]
    FROM [Orders] AS [o]
    OUTER APPLY (
        SELECT 5 + (
            SELECT COALESCE(SUM([o0].[ProductID]), 0)
            FROM [Order Details] AS [o0]
            WHERE [o].[OrderID] = [o0].[OrderID]) AS [value]
    ) AS [s]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [s0]
""");
    }

    public override async Task Sum_over_min_subquery(bool async)
    {
        await base.Sum_over_min_subquery(async);

        // #34256: rewrite query to avoid "Cannot perform an aggregate function on an expression containing an aggregate or a subquery"
        AssertSql(
            """
SELECT COALESCE(SUM([s0].[value]), 0)
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT COALESCE(SUM([s].[value]), 0) AS [value]
    FROM [Orders] AS [o]
    OUTER APPLY (
        SELECT 5 + (
            SELECT MIN([o0].[ProductID])
            FROM [Order Details] AS [o0]
            WHERE [o].[OrderID] = [o0].[OrderID]) AS [value]
    ) AS [s]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [s0]
""");
    }

    public override async Task Sum_over_scalar_returning_subquery(bool async)
    {
        await base.Sum_over_scalar_returning_subquery(async);

        // #34256: rewrite query to avoid "Cannot perform an aggregate function on an expression containing an aggregate or a subquery"
        AssertSql(
            """
SELECT COALESCE(SUM([s].[OrderID]), 0)
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT TOP(1) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [s]
""");
    }

    public override async Task Sum_over_Any_subquery(bool async)
    {
        await base.Sum_over_Any_subquery(async);

        // #34256: rewrite query to avoid "Cannot perform an aggregate function on an expression containing an aggregate or a subquery"
        AssertSql(
            """
SELECT COALESCE(SUM([s].[value]), 0)
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT CASE
        WHEN EXISTS (
            SELECT 1
            FROM [Orders] AS [o]
            WHERE [c].[CustomerID] = [o].[CustomerID]) THEN (
            SELECT TOP(1) [o0].[OrderID]
            FROM [Orders] AS [o0]
            WHERE [c].[CustomerID] = [o0].[CustomerID])
        ELSE 0
    END AS [value]
) AS [s]
""");
    }

    public override async Task Sum_over_uncorrelated_subquery(bool async)
    {
        await base.Sum_over_uncorrelated_subquery(async);

        // #34256: rewrite query to avoid "Cannot perform an aggregate function on an expression containing an aggregate or a subquery"
        AssertSql(
            """
SELECT ISNULL(SUM([s].[value]), 0)
FROM [Customers] AS [c]
CROSS JOIN (
    SELECT COUNT(*) AS [value]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] > 10300
) AS [s]
""");
    }

    public override async Task Sum_on_float_column(bool async)
    {
        await base.Sum_on_float_column(async);

        AssertSql(
            """
SELECT CAST(COALESCE(SUM([o].[Discount]), 0.0E0) AS real)
FROM [Order Details] AS [o]
WHERE [o].[ProductID] = 1
""");
    }

    public override async Task Sum_on_float_column_in_subquery(bool async)
    {
        await base.Sum_on_float_column_in_subquery(async);

        AssertSql(
            """
SELECT [o].[OrderID], (
    SELECT CAST(COALESCE(SUM([o0].[Discount]), 0.0E0) AS real)
    FROM [Order Details] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID]) AS [Sum]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300
""");
    }

    public override async Task Average_with_no_arg(bool async)
    {
        await base.Average_with_no_arg(async);

        AssertSql(
            """
SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
""");
    }

    public override async Task Average_with_binary_expression(bool async)
    {
        await base.Average_with_binary_expression(async);

        AssertSql(
            """
SELECT AVG(CAST([o].[OrderID] * 2 AS float))
FROM [Orders] AS [o]
""");
    }

    public override async Task Average_with_arg(bool async)
    {
        await base.Average_with_arg(async);

        AssertSql(
            """
SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
""");
    }

    public override async Task Average_with_arg_expression(bool async)
    {
        await base.Average_with_arg_expression(async);

        AssertSql(
            """
SELECT AVG(CAST([o].[OrderID] + [o].[OrderID] AS float))
FROM [Orders] AS [o]
""");
    }

    public override async Task Average_with_division_on_decimal(bool async)
    {
        await base.Average_with_division_on_decimal(async);

        AssertSql(
            """
SELECT AVG(CAST([o].[Quantity] AS decimal(18,2)) / 2.09)
FROM [Order Details] AS [o]
""");
    }

    public override async Task Average_with_division_on_decimal_no_significant_digits(bool async)
    {
        await base.Average_with_division_on_decimal_no_significant_digits(async);

        AssertSql(
            """
SELECT AVG(CAST([o].[Quantity] AS decimal(18,2)) / 2.0)
FROM [Order Details] AS [o]
""");
    }

    public override async Task Average_with_coalesce(bool async)
    {
        await base.Average_with_coalesce(async);

        AssertSql(
            """
SELECT AVG(COALESCE([p].[UnitPrice], 0.0))
FROM [Products] AS [p]
WHERE [p].[ProductID] < 40
""");
    }

    public override async Task Average_over_subquery(bool async)
    {
        await base.Average_over_subquery(async);

        // #34256: rewrite query to avoid "Cannot perform an aggregate function on an expression containing an aggregate or a subquery"
        AssertSql(
            """
SELECT AVG([s].[value])
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT CAST((
        SELECT COALESCE(SUM([o].[OrderID]), 0)
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]) AS float) AS [value]
) AS [s]
""");
    }

    public override async Task Average_over_nested_subquery(bool async)
    {
        await AssertAverage(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3),
            selector: c => (decimal)c.Orders.Average(o => 5 + o.OrderDetails.Average(od => od.ProductID)),
            asserter: (e, a) => Assert.Equal(e, a, precision: 3));

        // #34256: rewrite query to avoid "Cannot perform an aggregate function on an expression containing an aggregate or a subquery"
        AssertSql(
            """
@p='3'

SELECT AVG([s0].[value])
FROM (
    SELECT TOP(@p) [c].[CustomerID]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c0]
OUTER APPLY (
    SELECT CAST((
        SELECT AVG([s].[value])
        FROM [Orders] AS [o]
        OUTER APPLY (
            SELECT 5.0E0 + (
                SELECT AVG(CAST([o0].[ProductID] AS float))
                FROM [Order Details] AS [o0]
                WHERE [o].[OrderID] = [o0].[OrderID]) AS [value]
        ) AS [s]
        WHERE [c0].[CustomerID] = [o].[CustomerID]) AS decimal(18,2)) AS [value]
) AS [s0]
""");
    }

    public override async Task Average_over_max_subquery(bool async)
    {
        // Expected: 59.841269841269866666666666667
        // Actual:   59.843333
        await Assert.ThrowsAsync<EqualException>(() => base.Average_over_max_subquery(async));

        // #34256: rewrite query to avoid "Cannot perform an aggregate function on an expression containing an aggregate or a subquery"
        AssertSql(
            """
@p='3'

SELECT AVG([s0].[value])
FROM (
    SELECT TOP(@p) [c].[CustomerID]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c0]
OUTER APPLY (
    SELECT CAST((
        SELECT AVG([s].[value])
        FROM [Orders] AS [o]
        OUTER APPLY (
            SELECT CAST(5 + (
                SELECT MAX([o0].[ProductID])
                FROM [Order Details] AS [o0]
                WHERE [o].[OrderID] = [o0].[OrderID]) AS float) AS [value]
        ) AS [s]
        WHERE [c0].[CustomerID] = [o].[CustomerID]) AS decimal(18,2)) AS [value]
) AS [s0]
""");
    }

    public override async Task Average_on_float_column(bool async)
    {
        await base.Average_on_float_column(async);

        AssertSql(
            """
SELECT CAST(AVG([o].[Discount]) AS real)
FROM [Order Details] AS [o]
WHERE [o].[ProductID] = 1
""");
    }

    public override async Task Average_on_float_column_in_subquery(bool async)
    {
        await base.Average_on_float_column_in_subquery(async);

        AssertSql(
            """
SELECT [o].[OrderID], (
    SELECT CAST(AVG([o0].[Discount]) AS real)
    FROM [Order Details] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID]) AS [Sum]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300
""");
    }

    public override async Task Average_on_float_column_in_subquery_with_cast(bool async)
    {
        await base.Average_on_float_column_in_subquery_with_cast(async);

        AssertSql(
            """
SELECT [o].[OrderID], (
    SELECT CAST(AVG([o0].[Discount]) AS real)
    FROM [Order Details] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID]) AS [Sum]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300
""");
    }

    public override async Task Min_with_no_arg(bool async)
    {
        await base.Min_with_no_arg(async);

        AssertSql(
            """
SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]
""");
    }

    public override async Task Min_with_arg(bool async)
    {
        await base.Min_with_arg(async);

        AssertSql(
            """
SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]
""");
    }

    public override async Task Min_with_coalesce(bool async)
    {
        await base.Min_with_coalesce(async);

        AssertSql(
            """
SELECT MIN(COALESCE([p].[UnitPrice], 0.0))
FROM [Products] AS [p]
WHERE [p].[ProductID] < 40
""");
    }

    public override async Task Min_over_subquery(bool async)
    {
        await base.Min_over_subquery(async);

        // #34256: rewrite query to avoid "Cannot perform an aggregate function on an expression containing an aggregate or a subquery"
        AssertSql(
            """
SELECT MIN([s].[value])
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT COALESCE(SUM([o].[OrderID]), 0) AS [value]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [s]
""");
    }

    public override async Task Min_over_nested_subquery(bool async)
    {
        await base.Min_over_nested_subquery(async);

        // #34256: rewrite query to avoid "Cannot perform an aggregate function on an expression containing an aggregate or a subquery"
        AssertSql(
            """
@p='3'

SELECT MIN([s0].[value])
FROM (
    SELECT TOP(@p) [c].[CustomerID]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c0]
OUTER APPLY (
    SELECT MIN([s].[value]) AS [value]
    FROM [Orders] AS [o]
    OUTER APPLY (
        SELECT 5 + (
            SELECT MIN([o0].[ProductID])
            FROM [Order Details] AS [o0]
            WHERE [o].[OrderID] = [o0].[OrderID]) AS [value]
    ) AS [s]
    WHERE [c0].[CustomerID] = [o].[CustomerID]
) AS [s0]
""");
    }

    public override async Task Min_over_max_subquery(bool async)
    {
        await base.Min_over_max_subquery(async);

        // #34256: rewrite query to avoid "Cannot perform an aggregate function on an expression containing an aggregate or a subquery"
        AssertSql(
            """
@p='3'

SELECT MIN([s0].[value])
FROM (
    SELECT TOP(@p) [c].[CustomerID]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c0]
OUTER APPLY (
    SELECT MIN([s].[value]) AS [value]
    FROM [Orders] AS [o]
    OUTER APPLY (
        SELECT 5 + (
            SELECT MAX([o0].[ProductID])
            FROM [Order Details] AS [o0]
            WHERE [o].[OrderID] = [o0].[OrderID]) AS [value]
    ) AS [s]
    WHERE [c0].[CustomerID] = [o].[CustomerID]
) AS [s0]
""");
    }

    public override async Task Max_with_no_arg(bool async)
    {
        await base.Max_with_no_arg(async);

        AssertSql(
            """
SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]
""");
    }

    public override async Task Max_with_arg(bool async)
    {
        await base.Max_with_arg(async);

        AssertSql(
            """
SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]
""");
    }

    public override async Task Max_with_coalesce(bool async)
    {
        await base.Max_with_coalesce(async);

        AssertSql(
            """
SELECT MAX(COALESCE([p].[UnitPrice], 0.0))
FROM [Products] AS [p]
WHERE [p].[ProductID] < 40
""");
    }

    public override async Task Max_over_subquery(bool async)
    {
        await base.Max_over_subquery(async);

        // #34256: rewrite query to avoid "Cannot perform an aggregate function on an expression containing an aggregate or a subquery"
        AssertSql(
            """
SELECT MAX([s].[value])
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT COALESCE(SUM([o].[OrderID]), 0) AS [value]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [s]
""");
    }

    public override async Task Max_over_nested_subquery(bool async)
    {
        await base.Max_over_nested_subquery(async);

        // #34256: rewrite query to avoid "Cannot perform an aggregate function on an expression containing an aggregate or a subquery"
        AssertSql(
            """
@p='3'

SELECT MAX([s0].[value])
FROM (
    SELECT TOP(@p) [c].[CustomerID]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c0]
OUTER APPLY (
    SELECT MAX([s].[value]) AS [value]
    FROM [Orders] AS [o]
    OUTER APPLY (
        SELECT 5 + (
            SELECT MAX([o0].[ProductID])
            FROM [Order Details] AS [o0]
            WHERE [o].[OrderID] = [o0].[OrderID]) AS [value]
    ) AS [s]
    WHERE [c0].[CustomerID] = [o].[CustomerID]
) AS [s0]
""");
    }

    public override async Task Max_over_sum_subquery(bool async)
    {
        await base.Max_over_sum_subquery(async);

        // #34256: rewrite query to avoid "Cannot perform an aggregate function on an expression containing an aggregate or a subquery"
        AssertSql(
            """
@p='3'

SELECT MAX([s0].[value])
FROM (
    SELECT TOP(@p) [c].[CustomerID]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c0]
OUTER APPLY (
    SELECT MAX([s].[value]) AS [value]
    FROM [Orders] AS [o]
    OUTER APPLY (
        SELECT 5 + (
            SELECT COALESCE(SUM([o0].[ProductID]), 0)
            FROM [Order Details] AS [o0]
            WHERE [o].[OrderID] = [o0].[OrderID]) AS [value]
    ) AS [s]
    WHERE [c0].[CustomerID] = [o].[CustomerID]
) AS [s0]
""");
    }

    public override async Task Count_with_predicate(bool async)
    {
        await base.Count_with_predicate(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task Where_OrderBy_Count(bool async)
    {
        await base.Where_OrderBy_Count(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task OrderBy_Where_Count(bool async)
    {
        await base.OrderBy_Where_Count(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task OrderBy_Count_with_predicate(bool async)
    {
        await base.OrderBy_Count_with_predicate(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task OrderBy_Where_Count_with_predicate(bool async)
    {
        await base.OrderBy_Where_Count_with_predicate(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[OrderID] > 10 AND ([o].[CustomerID] <> N'ALFKI' OR [o].[CustomerID] IS NULL)
""");
    }

    public override async Task Distinct(bool async)
    {
        await base.Distinct(async);

        AssertSql(
            """
SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task Distinct_Scalar(bool async)
    {
        await base.Distinct_Scalar(async);

        AssertSql(
            """
SELECT DISTINCT [c].[City]
FROM [Customers] AS [c]
""");
    }

    public override async Task OrderBy_Distinct(bool async)
    {
        await base.OrderBy_Distinct(async);

        // Ordering not preserved by distinct when ordering columns not projected.
        AssertSql(
            """
SELECT DISTINCT [c].[City]
FROM [Customers] AS [c]
""");
    }

    public override async Task Distinct_OrderBy(bool async)
    {
        await base.Distinct_OrderBy(async);

        AssertSql(
            """
SELECT [c0].[Country]
FROM (
    SELECT DISTINCT [c].[Country]
    FROM [Customers] AS [c]
) AS [c0]
ORDER BY [c0].[Country]
""");
    }

    public override async Task Distinct_OrderBy2(bool async)
    {
        await base.Distinct_OrderBy2(async);

        AssertSql(
            """
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
) AS [c0]
ORDER BY [c0].[CustomerID]
""");
    }

    public override async Task Distinct_OrderBy3(bool async)
    {
        await base.Distinct_OrderBy3(async);

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

    public override async Task Distinct_Count(bool async)
    {
        await base.Distinct_Count(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
) AS [c0]
""");
    }

    public override async Task Select_Select_Distinct_Count(bool async)
    {
        await base.Select_Select_Distinct_Count(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[City]
    FROM [Customers] AS [c]
) AS [c0]
""");
    }

    public override async Task Single_Predicate(bool async)
    {
        await base.Single_Predicate(async);

        AssertSql(
            """
SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task FirstOrDefault_inside_subquery_gets_server_evaluated(bool async)
    {
        await base.FirstOrDefault_inside_subquery_gets_server_evaluated(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI' AND (
    SELECT TOP(1) [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID] AND [o].[CustomerID] = N'ALFKI') = N'ALFKI'
""");
    }

    public override async Task Multiple_collection_navigation_with_FirstOrDefault_chained(bool async)
    {
        await base.Multiple_collection_navigation_with_FirstOrDefault_chained(async);

        AssertSql(
            """
SELECT [o2].[OrderID], [o2].[ProductID], [o2].[Discount], [o2].[Quantity], [o2].[UnitPrice]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT TOP(1) [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
    FROM [Order Details] AS [o]
    WHERE (
        SELECT TOP(1) [o0].[OrderID]
        FROM [Orders] AS [o0]
        WHERE [c].[CustomerID] = [o0].[CustomerID]
        ORDER BY [o0].[OrderID]) IS NOT NULL AND (
        SELECT TOP(1) [o1].[OrderID]
        FROM [Orders] AS [o1]
        WHERE [c].[CustomerID] = [o1].[CustomerID]
        ORDER BY [o1].[OrderID]) = [o].[OrderID]
    ORDER BY [o].[ProductID]
) AS [o2]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Multiple_collection_navigation_with_FirstOrDefault_chained_projecting_scalar(bool async)
    {
        await base.Multiple_collection_navigation_with_FirstOrDefault_chained_projecting_scalar(async);

        AssertSql(
            """
SELECT (
    SELECT TOP(1) [o].[ProductID]
    FROM [Order Details] AS [o]
    WHERE (
        SELECT TOP(1) [o0].[OrderID]
        FROM [Orders] AS [o0]
        WHERE [c].[CustomerID] = [o0].[CustomerID]
        ORDER BY [o0].[OrderID]) IS NOT NULL AND (
        SELECT TOP(1) [o1].[OrderID]
        FROM [Orders] AS [o1]
        WHERE [c].[CustomerID] = [o1].[CustomerID]
        ORDER BY [o1].[OrderID]) = [o].[OrderID]
    ORDER BY [o].[ProductID])
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task First_inside_subquery_gets_client_evaluated(bool async)
    {
        await base.First_inside_subquery_gets_client_evaluated(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI' AND (
    SELECT TOP(1) [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID] AND [o].[CustomerID] = N'ALFKI') = N'ALFKI'
""");
    }

    public override async Task Last(bool async)
    {
        await base.Last(async);

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactName] DESC
""");
    }

    public override async Task Last_Predicate(bool async)
    {
        await base.Last_Predicate(async);

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[ContactName] DESC
""");
    }

    public override async Task Where_Last(bool async)
    {
        await base.Where_Last(async);

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[ContactName] DESC
""");
    }

    public override async Task LastOrDefault(bool async)
    {
        await base.LastOrDefault(async);

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactName] DESC
""");
    }

    public override async Task LastOrDefault_Predicate(bool async)
    {
        await base.LastOrDefault_Predicate(async);

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[ContactName] DESC
""");
    }

    public override async Task Where_LastOrDefault(bool async)
    {
        await base.Where_LastOrDefault(async);

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[ContactName] DESC
""");
    }

    public override async Task Contains_with_subquery(bool async)
    {
        await base.Contains_with_subquery(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
)
""");
    }

    public override async Task Contains_with_local_array_closure(bool async)
    {
        await base.Contains_with_local_array_closure(async);

        AssertSql(
            """
@ids='["ABCDE","ALFKI"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""",
            //
            """
@ids='["ABCDE"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""");
    }

    public override async Task Contains_with_subquery_and_local_array_closure(bool async)
    {
        await base.Contains_with_subquery_and_local_array_closure(async);

        AssertSql(
            """
@ids='["London","Buenos Aires"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c0]
    WHERE [c0].[City] IN (
        SELECT [i].[value]
        FROM OPENJSON(@ids) WITH ([value] nvarchar(15) '$') AS [i]
    ) AND [c0].[CustomerID] = [c].[CustomerID])
""",
            //
            """
@ids='["London"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c0]
    WHERE [c0].[City] IN (
        SELECT [i].[value]
        FROM OPENJSON(@ids) WITH ([value] nvarchar(15) '$') AS [i]
    ) AND [c0].[CustomerID] = [c].[CustomerID])
""");
    }

    public override async Task Contains_with_local_uint_array_closure(bool async)
    {
        await base.Contains_with_local_uint_array_closure(async);

        AssertSql(
            """
@ids='[0,1]' (Size = 4000)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] int '$') AS [i]
)
""",
            //
            """
@ids='[0]' (Size = 4000)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] int '$') AS [i]
)
""");
    }

    public override async Task Contains_with_local_nullable_uint_array_closure(bool async)
    {
        await base.Contains_with_local_nullable_uint_array_closure(async);

        AssertSql(
            """
@ids='[0,1]' (Size = 4000)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] int '$') AS [i]
)
""",
            //
            """
@ids='[0]' (Size = 4000)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] int '$') AS [i]
)
""");
    }

    public override async Task Contains_with_local_array_inline(bool async)
    {
        await base.Contains_with_local_array_inline(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI')
""");
    }

    public override async Task Contains_with_local_list_closure(bool async)
    {
        await base.Contains_with_local_list_closure(async);

        AssertSql(
            """
@ids='["ABCDE","ALFKI"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""");
    }

    public override async Task Contains_with_local_object_list_closure(bool async)
    {
        await base.Contains_with_local_object_list_closure(async);

        AssertSql(
            """
@ids='["ABCDE","ALFKI"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""");
    }

    public override async Task Contains_with_local_list_closure_all_null(bool async)
    {
        await base.Contains_with_local_list_closure_all_null(async);

        AssertSql(
            """
@ids='[null,null]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""");
    }

    public override async Task Contains_with_local_list_inline(bool async)
    {
        await base.Contains_with_local_list_inline(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI')
""");
    }

    public override async Task Contains_with_local_list_inline_closure_mix(bool async)
    {
        await base.Contains_with_local_list_inline_closure_mix(async);

        AssertSql(
            """
@p='["ABCDE","ALFKI"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [p].[value]
    FROM OPENJSON(@p) WITH ([value] nchar(5) '$') AS [p]
)
""",
            //
            """
@p='["ABCDE","ANATR"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [p].[value]
    FROM OPENJSON(@p) WITH ([value] nchar(5) '$') AS [p]
)
""");
    }

    public override async Task Contains_with_local_non_primitive_list_inline_closure_mix(bool async)
    {
        await base.Contains_with_local_non_primitive_list_inline_closure_mix(async);

        AssertSql(
            """
@Select='["ABCDE","ALFKI"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [s].[value]
    FROM OPENJSON(@Select) WITH ([value] nchar(5) '$') AS [s]
)
""",
            //
            """
@Select='["ABCDE","ANATR"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [s].[value]
    FROM OPENJSON(@Select) WITH ([value] nchar(5) '$') AS [s]
)
""");
    }

    public override async Task Contains_with_local_enumerable_closure(bool async)
    {
        await base.Contains_with_local_enumerable_closure(async);

        AssertSql(
            """
@ids='["ABCDE","ALFKI"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""",
            //
            """
@ids='["ABCDE"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""");
    }

    public override async Task Contains_with_local_object_enumerable_closure(bool async)
    {
        await base.Contains_with_local_object_enumerable_closure(async);

        AssertSql(
            """
@ids='["ABCDE","ALFKI"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""");
    }

    public override async Task Contains_with_local_enumerable_closure_all_null(bool async)
    {
        await base.Contains_with_local_enumerable_closure_all_null(async);

        AssertSql(
            """
@ids='[]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""");
    }

    public override async Task Contains_with_local_enumerable_inline(bool async)
    {
        // Issue #31776
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await base.Contains_with_local_enumerable_inline(async));

        AssertSql();
    }

    public override async Task Contains_with_local_enumerable_inline_closure_mix(bool async)
    {
        // Issue #31776
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await base.Contains_with_local_enumerable_inline_closure_mix(async));

        AssertSql();
    }

    public override async Task Contains_with_local_ordered_enumerable_closure(bool async)
    {
        await base.Contains_with_local_ordered_enumerable_closure(async);

        AssertSql(
            """
@ids='["ABCDE","ALFKI"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""",
            //
            """
@ids='["ABCDE"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""");
    }

    public override async Task Contains_with_local_object_ordered_enumerable_closure(bool async)
    {
        await base.Contains_with_local_object_ordered_enumerable_closure(async);

        AssertSql(
            """
@ids='["ABCDE","ALFKI"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""");
    }

    public override async Task Contains_with_local_ordered_enumerable_closure_all_null(bool async)
    {
        await base.Contains_with_local_ordered_enumerable_closure_all_null(async);

        AssertSql(
            """
@ids='[null,null]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""");
    }

    public override async Task Contains_with_local_ordered_enumerable_inline(bool async)
    {
        await base.Contains_with_local_ordered_enumerable_inline(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI')
""");
    }

    public override async Task Contains_with_local_ordered_enumerable_inline_closure_mix(bool async)
    {
        await base.Contains_with_local_ordered_enumerable_inline_closure_mix(async);

        AssertSql(
            """
@Order='["ABCDE","ALFKI"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [o].[value]
    FROM OPENJSON(@Order) WITH ([value] nchar(5) '$') AS [o]
)
""",
            //
            """
@Order='["ABCDE","ANATR"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [o].[value]
    FROM OPENJSON(@Order) WITH ([value] nchar(5) '$') AS [o]
)
""");
    }

    public override async Task Contains_with_local_read_only_collection_closure(bool async)
    {
        await base.Contains_with_local_read_only_collection_closure(async);

        AssertSql(
            """
@ids='["ABCDE","ALFKI"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""",
            //
            """
@ids='["ABCDE"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""");
    }

    public override async Task Contains_with_local_object_read_only_collection_closure(bool async)
    {
        await base.Contains_with_local_object_read_only_collection_closure(async);

        AssertSql(
            """
@ids='["ABCDE","ALFKI"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""");
    }

    public override async Task Contains_with_local_ordered_read_only_collection_all_null(bool async)
    {
        await base.Contains_with_local_ordered_read_only_collection_all_null(async);

        AssertSql(
            """
@ids='[null,null]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""");
    }

    public override async Task Contains_with_local_read_only_collection_inline(bool async)
    {
        await base.Contains_with_local_read_only_collection_inline(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI')
""");
    }

    public override async Task Contains_with_local_read_only_collection_inline_closure_mix(bool async)
    {
        await base.Contains_with_local_read_only_collection_inline_closure_mix(async);

        AssertSql(
            """
@AsReadOnly='["ABCDE","ALFKI"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [a].[value]
    FROM OPENJSON(@AsReadOnly) WITH ([value] nchar(5) '$') AS [a]
)
""",
            //
            """
@AsReadOnly='["ABCDE","ANATR"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [a].[value]
    FROM OPENJSON(@AsReadOnly) WITH ([value] nchar(5) '$') AS [a]
)
""");
    }

    public override async Task Contains_with_local_non_primitive_list_closure_mix(bool async)
    {
        await base.Contains_with_local_non_primitive_list_closure_mix(async);

        AssertSql(
            """
@Select='["ABCDE","ALFKI"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [s].[value]
    FROM OPENJSON(@Select) WITH ([value] nchar(5) '$') AS [s]
)
""");
    }

    public override async Task Contains_with_local_collection_false(bool async)
    {
        await base.Contains_with_local_collection_false(async);

        AssertSql(
            """
@ids='["ABCDE","ALFKI"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] NOT IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""");
    }

    public override async Task Contains_with_local_collection_complex_predicate_and(bool async)
    {
        await base.Contains_with_local_collection_complex_predicate_and(async);

        AssertSql(
            """
@ids='["ABCDE","ALFKI"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ALFKI', N'ABCDE') AND [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""");
    }

    public override async Task Contains_with_local_collection_complex_predicate_or(bool async)
        => await base.Contains_with_local_collection_complex_predicate_or(async);

    // issue #18791
    //            AssertSql(
    //                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    //FROM [Customers] AS [c]
    //WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI', N'ALFKI', N'ABCDE')");
    public override async Task Contains_with_local_collection_complex_predicate_not_matching_ins1(bool async)
        => await base.Contains_with_local_collection_complex_predicate_not_matching_ins1(async);

    // issue #18791
    //            AssertSql(
    //                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    //FROM [Customers] AS [c]
    //WHERE [c].[CustomerID] IN (N'ALFKI', N'ABCDE') OR [c].[CustomerID] NOT IN (N'ABCDE', N'ALFKI')");
    public override async Task Contains_with_local_collection_complex_predicate_not_matching_ins2(bool async)
        => await base.Contains_with_local_collection_complex_predicate_not_matching_ins2(async);

    // issue #18791
    //            AssertSql(
    //                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    //FROM [Customers] AS [c]
    //WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI') AND [c].[CustomerID] NOT IN (N'ALFKI', N'ABCDE')");
    public override async Task Contains_with_local_collection_sql_injection(bool async)
    {
        await base.Contains_with_local_collection_sql_injection(async);

        AssertSql(
            """
@ids='["ALFKI","ABC\u0027)); GO; DROP TABLE Orders; GO; --"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
) OR [c].[CustomerID] IN (N'ALFKI', N'ABCDE')
""");
    }

    public override async Task Contains_with_local_collection_empty_closure(bool async)
    {
        await base.Contains_with_local_collection_empty_closure(async);

        AssertSql(
            """
@ids='[]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""");
    }

    public override async Task Contains_with_local_collection_empty_inline(bool async)
    {
        await base.Contains_with_local_collection_empty_inline(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task Contains_top_level(bool async)
    {
        await base.Contains_top_level(async);

        AssertSql(
            """
@p='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT CASE
    WHEN @p IN (
        SELECT [c].[CustomerID]
        FROM [Customers] AS [c]
    ) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Contains_with_local_anonymous_type_array_closure(bool async)
    {
        await AssertTranslationFailed(() => base.Contains_with_local_anonymous_type_array_closure(async));

        AssertSql();
    }

    public override async Task OfType_Select(bool async)
    {
        await base.OfType_Select(async);

        AssertSql(
            """
SELECT TOP(1) [c].[City]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [o].[OrderID]
""");
    }

    public override async Task OfType_Select_OfType_Select(bool async)
    {
        await base.OfType_Select_OfType_Select(async);

        AssertSql(
            """
SELECT TOP(1) [c].[City]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [o].[OrderID]
""");
    }

    public override async Task Average_with_non_matching_types_in_projection_doesnt_produce_second_explicit_cast(bool async)
    {
        await base.Average_with_non_matching_types_in_projection_doesnt_produce_second_explicit_cast(async);

        AssertSql(
            """
SELECT AVG(CAST(CAST([o].[OrderID] AS bigint) AS float))
FROM [Orders] AS [o]
WHERE [o].[CustomerID] LIKE N'A%'
""");
    }

    public override async Task Max_with_non_matching_types_in_projection_introduces_explicit_cast(bool async)
    {
        await base.Max_with_non_matching_types_in_projection_introduces_explicit_cast(async);

        AssertSql(
            """
SELECT MAX(CAST([o].[OrderID] AS bigint))
FROM [Orders] AS [o]
WHERE [o].[CustomerID] LIKE N'A%'
""");
    }

    public override async Task Min_with_non_matching_types_in_projection_introduces_explicit_cast(bool async)
    {
        await base.Min_with_non_matching_types_in_projection_introduces_explicit_cast(async);

        AssertSql(
            """
SELECT MIN(CAST([o].[OrderID] AS bigint))
FROM [Orders] AS [o]
WHERE [o].[CustomerID] LIKE N'A%'
""");
    }

    public override async Task OrderBy_Take_Last_gives_correct_result(bool async)
    {
        await base.OrderBy_Take_Last_gives_correct_result(async);

        AssertSql(
            """
@p='20'

SELECT TOP(1) [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT TOP(@p) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c0]
ORDER BY [c0].[CustomerID] DESC
""");
    }

    public override async Task OrderBy_Skip_Last_gives_correct_result(bool async)
    {
        await base.OrderBy_Skip_Last_gives_correct_result(async);

        AssertSql(
            """
@p='20'

SELECT TOP(1) [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
    OFFSET @p ROWS
) AS [c0]
ORDER BY [c0].[CustomerID] DESC
""");
    }

    public override async Task Contains_over_entityType_should_rewrite_to_identity_equality(bool async)
    {
        await base.Contains_over_entityType_should_rewrite_to_identity_equality(async);

        AssertSql(
            """
SELECT TOP(2) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] = 10248
""",
            //
            """
@entity_equality_p_OrderID='10248' (Nullable = true)

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE [o].[CustomerID] = N'VINET' AND [o].[OrderID] = @entity_equality_p_OrderID) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task List_Contains_over_entityType_should_rewrite_to_identity_equality(bool async)
    {
        await base.List_Contains_over_entityType_should_rewrite_to_identity_equality(async);

        AssertSql(
            """
@entity_equality_someOrder_OrderID='10248' (Nullable = true)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID] AND [o].[OrderID] = @entity_equality_someOrder_OrderID)
""");
    }

    public override async Task List_Contains_with_constant_list(bool async)
    {
        await base.List_Contains_with_constant_list(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ALFKI', N'ANATR')
""");
    }

    public override async Task List_Contains_with_parameter_list(bool async)
    {
        await base.List_Contains_with_parameter_list(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ALFKI', N'ANATR')
""");
    }

    public override async Task Contains_with_parameter_list_value_type_id(bool async)
    {
        await base.Contains_with_parameter_list_value_type_id(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] IN (10248, 10249)
""");
    }

    public override async Task Contains_with_constant_list_value_type_id(bool async)
    {
        await base.Contains_with_constant_list_value_type_id(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] IN (10248, 10249)
""");
    }

    public override async Task IImmutableSet_Contains_with_parameter(bool async)
    {
        await base.IImmutableSet_Contains_with_parameter(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task IReadOnlySet_Contains_with_parameter(bool async)
    {
        await base.IReadOnlySet_Contains_with_parameter(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task HashSet_Contains_with_parameter(bool async)
    {
        await base.HashSet_Contains_with_parameter(async);

        AssertSql(
            """
@ids='["ALFKI"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""");
    }

    public override async Task ImmutableHashSet_Contains_with_parameter(bool async)
    {
        await base.ImmutableHashSet_Contains_with_parameter(async);

        AssertSql(
            """
@ids='["ALFKI"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""");
    }

    public override async Task Contains_over_entityType_with_null_should_rewrite_to_false(bool async)
    {
        await base.Contains_over_entityType_with_null_should_rewrite_to_false(async);

        AssertSql(
            """
SELECT CAST(0 AS bit)
""");
    }

    public override async Task Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery(bool async)
    {
        await base.Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE 0 = 1
""");
    }

    public override async Task Contains_over_entityType_with_null_in_projection(bool async)
    {
        await base.Contains_over_entityType_with_null_in_projection(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE 0 = 1
""");
    }

    public override async Task Contains_over_scalar_with_null_should_rewrite_to_identity_equality_subquery(bool async)
    {
        await base.Contains_over_scalar_with_null_should_rewrite_to_identity_equality_subquery(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o0]
    WHERE [o0].[CustomerID] = N'VINET' AND [o0].[EmployeeID] IS NULL)
""");
    }

    public override async Task Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery_negated(bool async)
    {
        await base.Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery_negated(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o0]
    WHERE [o0].[CustomerID] = N'VINET' AND [o0].[EmployeeID] IS NULL)
""");
    }

    public override async Task Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery_complex(bool async)
    {
        await base.Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery_complex(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o0]
        WHERE [o0].[CustomerID] = N'VINET' AND [o0].[EmployeeID] IS NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o1]
        WHERE ([o1].[CustomerID] <> N'VINET' OR [o1].[CustomerID] IS NULL) AND [o1].[EmployeeID] IS NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Contains_over_nullable_scalar_with_null_in_subquery_translated_correctly(bool async)
    {
        await base.Contains_over_nullable_scalar_with_null_in_subquery_translated_correctly(async);

        AssertSql(
            """
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o0]
        WHERE [o0].[CustomerID] = N'VINET' AND [o0].[EmployeeID] IS NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Orders] AS [o]
""");
    }

    public override async Task Contains_over_non_nullable_scalar_with_null_in_subquery_simplifies_to_false(bool async)
    {
        await base.Contains_over_non_nullable_scalar_with_null_in_subquery_simplifies_to_false(async);

        AssertSql(
            """
SELECT CAST(0 AS bit)
FROM [Orders] AS [o]
""");
    }

    public override async Task Contains_over_entityType_should_materialize_when_composite(bool async)
    {
        await base.Contains_over_entityType_should_materialize_when_composite(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE [o].[ProductID] = 42 AND EXISTS (
    SELECT 1
    FROM [Order Details] AS [o0]
    WHERE [o0].[OrderID] = [o].[OrderID] AND [o0].[ProductID] = [o].[ProductID])
""");
    }

    public override async Task Contains_over_entityType_should_materialize_when_composite2(bool async)
    {
        await base.Contains_over_entityType_should_materialize_when_composite2(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE [o].[ProductID] = 42 AND EXISTS (
    SELECT 1
    FROM [Order Details] AS [o0]
    WHERE [o0].[OrderID] > 42 AND [o0].[OrderID] = [o].[OrderID] AND [o0].[ProductID] = [o].[ProductID])
""");
    }

    public override async Task String_FirstOrDefault_in_projection_does_not_do_client_eval(bool async)
    {
        await base.String_FirstOrDefault_in_projection_does_not_do_client_eval(async);

        AssertSql(
            """
SELECT SUBSTRING([c].[CustomerID], 1, 1)
FROM [Customers] AS [c]
""");
    }

    public override async Task Project_constant_Sum(bool async)
    {
        await base.Project_constant_Sum(async);

        AssertSql(
            """
SELECT ISNULL(SUM(1), 0)
FROM [Employees] AS [e]
""");
    }

    public override async Task Where_subquery_any_equals_operator(bool async)
    {
        await base.Where_subquery_any_equals_operator(async);

        AssertSql(
            """
@ids='["ABCDE","ALFKI","ANATR"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""");
    }

    public override async Task Where_subquery_any_equals(bool async)
    {
        await base.Where_subquery_any_equals(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI', N'ANATR')
""");
    }

    public override async Task Where_subquery_any_equals_static(bool async)
    {
        await base.Where_subquery_any_equals_static(async);

        AssertSql(
            """
@ids='["ABCDE","ALFKI","ANATR"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""");
    }

    public override async Task Where_subquery_where_any(bool async)
    {
        await base.Where_subquery_where_any(async);

        AssertSql(
            """
@ids='["ABCDE","ALFKI","ANATR"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'México D.F.' AND [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""",
            //
            """
@ids='["ABCDE","ALFKI","ANATR"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'México D.F.' AND [c].[CustomerID] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""");
    }

    public override async Task Where_subquery_all_not_equals_operator(bool async)
    {
        await base.Where_subquery_all_not_equals_operator(async);

        AssertSql(
            """
@ids='["ABCDE","ALFKI","ANATR"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] NOT IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""");
    }

    public override async Task Where_subquery_all_not_equals(bool async)
    {
        await base.Where_subquery_all_not_equals(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] NOT IN (N'ABCDE', N'ALFKI', N'ANATR')
""");
    }

    public override async Task Where_subquery_all_not_equals_static(bool async)
    {
        await base.Where_subquery_all_not_equals_static(async);

        AssertSql(
            """
@ids='["ABCDE","ALFKI","ANATR"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] NOT IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""");
    }

    public override async Task Where_subquery_where_all(bool async)
    {
        await base.Where_subquery_where_all(async);

        AssertSql(
            """
@ids='["ABCDE","ALFKI","ANATR"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'México D.F.' AND [c].[CustomerID] NOT IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""",
            //
            """
@ids='["ABCDE","ALFKI","ANATR"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'México D.F.' AND [c].[CustomerID] NOT IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] nchar(5) '$') AS [i]
)
""");
    }

    public override async Task Cast_to_same_Type_Count_works(bool async)
    {
        await base.Cast_to_same_Type_Count_works(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Customers] AS [c]
""");
    }

    public override async Task Cast_before_aggregate_is_preserved(bool async)
    {
        await base.Cast_before_aggregate_is_preserved(async);

        AssertSql(
            """
SELECT (
    SELECT AVG(CAST([o].[OrderID] AS float))
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
FROM [Customers] AS [c]
""");
    }

    public override async Task DefaultIfEmpty_selects_only_required_columns(bool async)
    {
        await base.DefaultIfEmpty_selects_only_required_columns(async);

        AssertSql(
            """
SELECT [p].[ProductName]
FROM (
    SELECT 1 AS empty
) AS [e]
LEFT JOIN [Products] AS [p] ON 1 = 1
""");
    }

    public override async Task Collection_Last_member_access_in_projection_translated(bool async)
    {
        await base.Collection_Last_member_access_in_projection_translated(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%' AND (
    SELECT TOP(1) [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]) = [c].[CustomerID]
""");
    }

    public override async Task Collection_LastOrDefault_member_access_in_projection_translated(bool async)
    {
        await base.Collection_LastOrDefault_member_access_in_projection_translated(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%' AND (
    SELECT TOP(1) [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]) = [c].[CustomerID]
""");
    }

    public override async Task Sum_over_explicit_cast_over_column(bool async)
    {
        await base.Sum_over_explicit_cast_over_column(async);

        AssertSql(
            """
SELECT COALESCE(SUM(CAST([o].[OrderID] AS bigint)), CAST(0 AS bigint))
FROM [Orders] AS [o]
""");
    }

    public override async Task Count_on_projection_with_client_eval(bool async)
    {
        await base.Count_on_projection_with_client_eval(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
""",
            //
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
""",
            //
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
""");
    }

    public override async Task Average_on_nav_subquery_in_projection(bool async)
    {
        await base.Average_on_nav_subquery_in_projection(async);

        AssertSql(
            """
SELECT (
    SELECT AVG(CAST([o].[OrderID] AS float))
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]) AS [Ave]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Count_after_client_projection(bool async)
    {
        await base.Count_after_client_projection(async);

        AssertSql(
            """
@p='1'

SELECT COUNT(*)
FROM (
    SELECT TOP(@p) 1 AS empty
    FROM [Orders] AS [o]
) AS [o0]
""");
    }

    public override async Task All_true(bool async)
    {
        await base.All_true(async);

        AssertSql(
            """
SELECT CAST(1 AS bit)
""");
    }

    public override async Task Not_Any_false(bool async)
    {
        await base.Not_Any_false(async);

        AssertSql(
            """
SELECT [c].[CustomerID]
FROM [Customers] AS [c]
""");
    }

    public override async Task Contains_inside_aggregate_function_with_GroupBy(bool async)
    {
        await base.Contains_inside_aggregate_function_with_GroupBy(async);

        AssertSql(
            """
@cities='["London","Berlin"]' (Size = 4000)

SELECT COUNT([s].[value])
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT CASE
        WHEN [c].[City] IN (
            SELECT [c0].[value]
            FROM OPENJSON(@cities) WITH ([value] nvarchar(15) '$') AS [c0]
        ) THEN 1
    END AS [value]
) AS [s]
GROUP BY [c].[Country]
""");
    }

    public override async Task Contains_inside_Average_without_GroupBy(bool async)
    {
        await base.Contains_inside_Average_without_GroupBy(async);

        AssertSql(
            """
@cities='["London","Berlin"]' (Size = 4000)

SELECT AVG([s].[value])
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT CASE
        WHEN [c].[City] IN (
            SELECT [c0].[value]
            FROM OPENJSON(@cities) WITH ([value] nvarchar(15) '$') AS [c0]
        ) THEN 1.0E0
        ELSE 0.0E0
    END AS [value]
) AS [s]
""");
    }

    public override async Task Contains_inside_Sum_without_GroupBy(bool async)
    {
        await base.Contains_inside_Sum_without_GroupBy(async);

        AssertSql(
            """
@cities='["London","Berlin"]' (Size = 4000)

SELECT ISNULL(SUM([s].[value]), 0)
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT CASE
        WHEN [c].[City] IN (
            SELECT [c0].[value]
            FROM OPENJSON(@cities) WITH ([value] nvarchar(15) '$') AS [c0]
        ) THEN 1
        ELSE 0
    END AS [value]
) AS [s]
""");
    }

    public override async Task Contains_inside_Count_without_GroupBy(bool async)
    {
        await base.Contains_inside_Count_without_GroupBy(async);

        AssertSql(
            """
@cities='["London","Berlin"]' (Size = 4000)

SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[City] IN (
    SELECT [c0].[value]
    FROM OPENJSON(@cities) WITH ([value] nvarchar(15) '$') AS [c0]
)
""");
    }

    public override async Task Contains_inside_LongCount_without_GroupBy(bool async)
    {
        await base.Contains_inside_LongCount_without_GroupBy(async);

        AssertSql(
            """
@cities='["London","Berlin"]' (Size = 4000)

SELECT COUNT_BIG(*)
FROM [Customers] AS [c]
WHERE [c].[City] IN (
    SELECT [c0].[value]
    FROM OPENJSON(@cities) WITH ([value] nvarchar(15) '$') AS [c0]
)
""");
    }

    public override async Task Contains_inside_Max_without_GroupBy(bool async)
    {
        await base.Contains_inside_Max_without_GroupBy(async);

        AssertSql(
            """
@cities='["London","Berlin"]' (Size = 4000)

SELECT MAX([s].[value])
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT CASE
        WHEN [c].[City] IN (
            SELECT [c0].[value]
            FROM OPENJSON(@cities) WITH ([value] nvarchar(15) '$') AS [c0]
        ) THEN 1
        ELSE 0
    END AS [value]
) AS [s]
""");
    }

    public override async Task Contains_inside_Min_without_GroupBy(bool async)
    {
        await base.Contains_inside_Min_without_GroupBy(async);

        AssertSql(
            """
@cities='["London","Berlin"]' (Size = 4000)

SELECT MIN([s].[value])
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT CASE
        WHEN [c].[City] IN (
            SELECT [c0].[value]
            FROM OPENJSON(@cities) WITH ([value] nvarchar(15) '$') AS [c0]
        ) THEN 1
        ELSE 0
    END AS [value]
) AS [s]
""");
    }

    public override async Task Return_type_of_singular_operator_is_preserved(bool async)
    {
        await base.Return_type_of_singular_operator_is_preserved(async);

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID], [c].[City]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""",
            //
            """
SELECT TOP(1) [c].[CustomerID], [c].[City]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""",
            //
            """
SELECT TOP(2) [c].[CustomerID], [c].[City]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""",
            //
            """
SELECT TOP(2) [c].[CustomerID], [c].[City]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""",
            //
            """
SELECT TOP(1) [c].[CustomerID], [c].[City]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID] DESC
""",
            //
            """
SELECT TOP(1) [c].[CustomerID], [c].[City]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID] DESC
""");
    }

    public override async Task Type_casting_inside_sum(bool async)
    {
        await base.Type_casting_inside_sum(async);

        AssertSql(
            """
SELECT COALESCE(SUM(CAST([o].[Discount] AS decimal(18,2))), 0.0)
FROM [Order Details] AS [o]
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
