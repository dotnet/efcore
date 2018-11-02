// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class SimpleQuerySqlServerTest
    {
        public override async Task Union_with_custom_projection(bool isAsync)
        {
            await base.Union_with_custom_projection(isAsync);

            AssertSql(
                @"SELECT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
FROM [Customers] AS [c1]
WHERE [c1].[CompanyName] LIKE N'A' + N'%' AND (LEFT([c1].[CompanyName], LEN(N'A')) = N'A')",
                //
                @"SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
FROM [Customers] AS [c2]
WHERE [c2].[CompanyName] LIKE N'B' + N'%' AND (LEFT([c2].[CompanyName], LEN(N'B')) = N'B')");
        }

        public override void Select_All()
        {
            base.Select_All();

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE ([o].[CustomerID] <> N'ALFKI') OR [o].[CustomerID] IS NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override async Task Sum_with_no_arg(bool isAsync)
        {
            await base.Sum_with_no_arg(isAsync);

            AssertSql(
                @"SELECT SUM([o].[OrderID])
FROM [Orders] AS [o]");
        }

        public override async Task Sum_with_binary_expression(bool isAsync)
        {
            await base.Sum_with_binary_expression(isAsync);

            AssertSql(
                @"SELECT SUM([o].[OrderID] * 2)
FROM [Orders] AS [o]");
        }

        public override async Task Sum_with_arg(bool isAsync)
        {
            await base.Sum_with_arg(isAsync);

            AssertSql(
                @"SELECT SUM([o].[OrderID])
FROM [Orders] AS [o]");
        }

        public override async Task Sum_with_arg_expression(bool isAsync)
        {
            await base.Sum_with_arg_expression(isAsync);

            AssertSql(
                @"SELECT SUM([o].[OrderID] + [o].[OrderID])
FROM [Orders] AS [o]");
        }

        public override async Task Sum_with_division_on_decimal(bool isAsync)
        {
            await base.Sum_with_division_on_decimal(isAsync);

            AssertSql(
                @"SELECT SUM([od].[Quantity] / 2.09)
FROM [Order Details] AS [od]");
        }

        public override async Task Sum_with_division_on_decimal_no_significant_digits(bool isAsync)
        {
            await base.Sum_with_division_on_decimal_no_significant_digits(isAsync);

            AssertSql(
                @"SELECT SUM([od].[Quantity] / 2.0)
FROM [Order Details] AS [od]");
        }

        public override async Task Sum_with_coalesce(bool isAsync)
        {
            await base.Sum_with_coalesce(isAsync);

            AssertSql(
                @"SELECT SUM(COALESCE([p].[UnitPrice], 0.0))
FROM [Products] AS [p]
WHERE [p].[ProductID] < 40");
        }

        public override async Task Sum_over_subquery_is_client_eval(bool isAsync)
        {
            await base.Sum_over_subquery_is_client_eval(isAsync);

            AssertSql(
                @"SELECT (
    SELECT SUM([o].[OrderID])
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
)
FROM [Customers] AS [c]");
        }

        public override async Task Sum_over_nested_subquery_is_client_eval(bool isAsync)
        {
            await base.Sum_over_nested_subquery_is_client_eval(isAsync);
            AssertSql(
               @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]");
        }

        public override async Task Sum_over_min_subquery_is_client_eval(bool isAsync)
        {
            await base.Sum_over_min_subquery_is_client_eval(isAsync);
            AssertSql(
               @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]");
        }

        public override async Task Sum_on_float_column(bool isAsync)
        {
            await base.Sum_on_float_column(isAsync);

            AssertSql(
                @"SELECT CAST(SUM([od].[Discount]) AS real)
FROM [Order Details] AS [od]
WHERE [od].[ProductID] = 1");
        }

        public override async Task Sum_on_float_column_in_subquery(bool isAsync)
        {
            await base.Sum_on_float_column_in_subquery(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], (
    SELECT CAST(SUM([od].[Discount]) AS real)
    FROM [Order Details] AS [od]
    WHERE [o].[OrderID] = [od].[OrderID]
) AS [Sum]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300");
        }

        public override async Task Average_with_no_arg(bool isAsync)
        {
            await base.Average_with_no_arg(isAsync);

            AssertSql(
                @"SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]");
        }

        public override async Task Average_with_binary_expression(bool isAsync)
        {
            await base.Average_with_binary_expression(isAsync);

            AssertSql(
                @"SELECT AVG(CAST([o].[OrderID] * 2 AS float))
FROM [Orders] AS [o]");
        }

        public override async Task Average_with_arg(bool isAsync)
        {
            await base.Average_with_arg(isAsync);

            AssertSql(
                @"SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]");
        }

        public override async Task Average_with_arg_expression(bool isAsync)
        {
            await base.Average_with_arg_expression(isAsync);

            AssertSql(
                @"SELECT AVG(CAST([o].[OrderID] + [o].[OrderID] AS float))
FROM [Orders] AS [o]");
        }

        public override async Task Average_with_division_on_decimal(bool isAsync)
        {
            await base.Average_with_division_on_decimal(isAsync);

            AssertSql(
                @"SELECT AVG([od].[Quantity] / 2.09)
FROM [Order Details] AS [od]");
        }

        public override async Task Average_with_division_on_decimal_no_significant_digits(bool isAsync)
        {
            await base.Average_with_division_on_decimal_no_significant_digits(isAsync);

            AssertSql(
                @"SELECT AVG([od].[Quantity] / 2.0)
FROM [Order Details] AS [od]");
        }

        public override async Task Average_with_coalesce(bool isAsync)
        {
            await base.Average_with_coalesce(isAsync);

            AssertSql(
                @"SELECT AVG(COALESCE([p].[UnitPrice], 0.0))
FROM [Products] AS [p]
WHERE [p].[ProductID] < 40");
        }

        public override async Task Average_over_subquery_is_client_eval(bool isAsync)
        {
            await base.Average_over_subquery_is_client_eval(isAsync);

            AssertSql(
                @"SELECT (
    SELECT SUM([o].[OrderID])
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
)
FROM [Customers] AS [c]");
        }

        public override async Task Average_over_nested_subquery_is_client_eval(bool isAsync)
        {
            await base.Average_over_nested_subquery_is_client_eval(isAsync);
            AssertSql(
               @"@__p_0='3'

SELECT TOP(@__p_0) [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override async Task Average_over_max_subquery_is_client_eval(bool isAsync)
        {
            await base.Average_over_max_subquery_is_client_eval(isAsync);
            AssertSql(
               @"@__p_0='3'

SELECT TOP(@__p_0) [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override async Task Average_on_float_column(bool isAsync)
        {
            await base.Average_on_float_column(isAsync);

            AssertSql(
                @"SELECT CAST(AVG([od].[Discount]) AS real)
FROM [Order Details] AS [od]
WHERE [od].[ProductID] = 1");
        }

        public override async Task Average_on_float_column_in_subquery(bool isAsync)
        {
            await base.Average_on_float_column_in_subquery(isAsync);

            AssertContainsSql(
                @"SELECT [o].[OrderID]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300",
                //
                @"@_outer_OrderID='10248'

SELECT CAST(AVG([od0].[Discount]) AS real)
FROM [Order Details] AS [od0]
WHERE @_outer_OrderID = [od0].[OrderID]");

            Assert.Contains(
                RelationalStrings.LogQueryPossibleExceptionWithAggregateOperator.GenerateMessage(),
                Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
        }

        public override async Task Average_on_float_column_in_subquery_with_cast(bool isAsync)
        {
            await base.Average_on_float_column_in_subquery_with_cast(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], (
    SELECT CAST(AVG([od].[Discount]) AS real)
    FROM [Order Details] AS [od]
    WHERE [o].[OrderID] = [od].[OrderID]
) AS [Sum]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300");
        }

        public override async Task Min_with_no_arg(bool isAsync)
        {
            await base.Min_with_no_arg(isAsync);

            AssertSql(
                @"SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]");
        }

        public override async Task Min_with_arg(bool isAsync)
        {
            await base.Min_with_arg(isAsync);

            AssertSql(
                @"SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]");
        }

        public override async Task Min_with_coalesce(bool isAsync)
        {
            await base.Min_with_coalesce(isAsync);

            AssertSql(
                @"SELECT MIN(COALESCE([p].[UnitPrice], 0.0))
FROM [Products] AS [p]
WHERE [p].[ProductID] < 40");
        }

        public override async Task Min_over_subquery_is_client_eval(bool isAsync)
        {
            await base.Min_over_subquery_is_client_eval(isAsync);

            AssertSql(
                @"SELECT (
    SELECT SUM([o].[OrderID])
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
)
FROM [Customers] AS [c]");
        }

        public override async Task Min_over_nested_subquery_is_client_eval(bool isAsync)
        {
            await base.Min_over_nested_subquery_is_client_eval(isAsync);

            AssertSql(
               @"@__p_0='3'

SELECT TOP(@__p_0) [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override async Task Min_over_max_subquery_is_client_eval(bool isAsync)
        {
            await base.Min_over_max_subquery_is_client_eval(isAsync);

            AssertSql(
               @"@__p_0='3'

SELECT TOP(@__p_0) [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override async Task Max_with_no_arg(bool isAsync)
        {
            await base.Max_with_no_arg(isAsync);

            AssertSql(
                @"SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]");
        }

        public override async Task Max_with_arg(bool isAsync)
        {
            await base.Max_with_arg(isAsync);

            AssertSql(
                @"SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]");
        }

        public override async Task Max_with_coalesce(bool isAsync)
        {
            await base.Max_with_coalesce(isAsync);

            AssertSql(
                @"SELECT MAX(COALESCE([p].[UnitPrice], 0.0))
FROM [Products] AS [p]
WHERE [p].[ProductID] < 40");
        }

        public override async Task Max_over_subquery_is_client_eval(bool isAsync)
        {
            await base.Max_over_subquery_is_client_eval(isAsync);

            AssertSql(
                @"SELECT (
    SELECT SUM([o].[OrderID])
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
)
FROM [Customers] AS [c]");
        }

        public override async Task Max_over_nested_subquery_is_client_eval(bool isAsync)
        {
            await base.Max_over_nested_subquery_is_client_eval(isAsync);

            AssertSql(
               @"@__p_0='3'

SELECT TOP(@__p_0) [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override async Task Max_over_sum_subquery_is_client_eval(bool isAsync)
        {
            await base.Max_over_sum_subquery_is_client_eval(isAsync);

            AssertSql(
               @"@__p_0='3'

SELECT TOP(@__p_0) [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override async Task Count_with_predicate(bool isAsync)
        {
            await base.Count_with_predicate(isAsync);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'");
        }

        public override async Task Where_OrderBy_Count(bool isAsync)
        {
            await base.Where_OrderBy_Count(isAsync);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'");
        }

        public override async Task OrderBy_Where_Count(bool isAsync)
        {
            await base.OrderBy_Where_Count(isAsync);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'");
        }

        public override async Task OrderBy_Count_with_predicate(bool isAsync)
        {
            await base.OrderBy_Count_with_predicate(isAsync);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'");
        }

        public override async Task OrderBy_Where_Count_with_predicate(bool isAsync)
        {
            await base.OrderBy_Where_Count_with_predicate(isAsync);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE ([o].[OrderID] > 10) AND (([o].[CustomerID] <> N'ALFKI') OR [o].[CustomerID] IS NULL)");
        }

        public override async Task Where_OrderBy_Count_client_eval(bool isAsync)
        {
            await base.Where_OrderBy_Count_client_eval(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override async Task Where_OrderBy_Count_client_eval_mixed(bool isAsync)
        {
            await base.Where_OrderBy_Count_client_eval_mixed(isAsync);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[OrderID] > 10");
        }

        public override async Task OrderBy_Where_Count_client_eval(bool isAsync)
        {
            await base.OrderBy_Where_Count_client_eval(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override async Task OrderBy_Where_Count_client_eval_mixed(bool isAsync)
        {
            await base.OrderBy_Where_Count_client_eval_mixed(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override async Task OrderBy_Count_with_predicate_client_eval(bool isAsync)
        {
            await base.OrderBy_Count_with_predicate_client_eval(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override async Task OrderBy_Count_with_predicate_client_eval_mixed(bool isAsync)
        {
            await base.OrderBy_Count_with_predicate_client_eval_mixed(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override async Task OrderBy_Where_Count_with_predicate_client_eval(bool isAsync)
        {
            await base.OrderBy_Where_Count_with_predicate_client_eval(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override async Task OrderBy_Where_Count_with_predicate_client_eval_mixed(bool isAsync)
        {
            await base.OrderBy_Where_Count_with_predicate_client_eval_mixed(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] <> N'ALFKI') OR [o].[CustomerID] IS NULL");
        }

        public override async Task OrderBy_client_Take(bool isAsync)
        {
            await base.OrderBy_client_Take(isAsync);

            AssertSql(
                @"@__p_0='10'

SELECT TOP(@__p_0) [o].[EmployeeID], [o].[City], [o].[Country], [o].[FirstName], [o].[ReportsTo], [o].[Title]
FROM [Employees] AS [o]
ORDER BY (SELECT 1)");
        }

        public override async Task Distinct(bool isAsync)
        {
            await base.Distinct(isAsync);

            AssertSql(
                @"SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Distinct_Scalar(bool isAsync)
        {
            await base.Distinct_Scalar(isAsync);

            AssertSql(
                @"SELECT DISTINCT [c].[City]
FROM [Customers] AS [c]");
        }

        public override async Task OrderBy_Distinct(bool isAsync)
        {
            await base.OrderBy_Distinct(isAsync);

            // Ordering not preserved by distinct when ordering columns not projected.
            AssertSql(
                @"SELECT DISTINCT [c].[City]
FROM [Customers] AS [c]");
        }

        public override async Task Distinct_OrderBy(bool isAsync)
        {
            await base.Distinct_OrderBy(isAsync);

            AssertSql(
                @"SELECT [t].[Country]
FROM (
    SELECT DISTINCT [c].[Country]
    FROM [Customers] AS [c]
) AS [t]
ORDER BY [t].[Country]");
        }

        public override async Task Distinct_OrderBy2(bool isAsync)
        {
            await base.Distinct_OrderBy2(isAsync);

            AssertSql(
                @"SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
) AS [t]
ORDER BY [t].[CustomerID]");
        }

        public override async Task Distinct_OrderBy3(bool isAsync)
        {
            await base.Distinct_OrderBy3(isAsync);

            AssertSql(
                @"SELECT [t].[CustomerID]
FROM (
    SELECT DISTINCT [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [t]
ORDER BY [t].[CustomerID]");
        }

        public override async Task Distinct_Count(bool isAsync)
        {
            await base.Distinct_Count(isAsync);

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].*
    FROM [Customers] AS [c]
) AS [t]");
        }

        public override async Task Select_Select_Distinct_Count(bool isAsync)
        {
            await base.Select_Select_Distinct_Count(isAsync);

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[City]
    FROM [Customers] AS [c]
) AS [t]");
        }

        public override async Task Single_Predicate(bool isAsync)
        {
            await base.Single_Predicate(isAsync);

            AssertSql(
                @"SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task FirstOrDefault_inside_subquery_gets_server_evaluated(bool isAsync)
        {
            await base.FirstOrDefault_inside_subquery_gets_server_evaluated(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] = N'ALFKI') AND ((
    SELECT TOP(1) [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE ([o].[CustomerID] = N'ALFKI') AND ([c].[CustomerID] = [o].[CustomerID])
) = N'ALFKI')");
        }

        public override async Task First_inside_subquery_gets_client_evaluated(bool isAsync)
        {
            await base.First_inside_subquery_gets_client_evaluated(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'",
                //
                @"@_outer_CustomerID='ALFKI' (Size = 5)

SELECT TOP(1) [o0].[CustomerID]
FROM [Orders] AS [o0]
WHERE ([o0].[CustomerID] = N'ALFKI') AND (@_outer_CustomerID = [o0].[CustomerID])");
        }

        public override async Task Last(bool isAsync)
        {
            await base.Last(isAsync);

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactName] DESC");
        }

        public override async Task Last_when_no_order_by(bool isAsync)
        {
            await base.Last_when_no_order_by(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task Last_Predicate(bool isAsync)
        {
            await base.Last_Predicate(isAsync);

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[ContactName] DESC");
        }

        public override async Task Where_Last(bool isAsync)
        {
            await base.Where_Last(isAsync);

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[ContactName] DESC");
        }

        public override async Task LastOrDefault(bool isAsync)
        {
            await base.LastOrDefault(isAsync);

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactName] DESC");
        }

        public override async Task LastOrDefault_Predicate(bool isAsync)
        {
            await base.LastOrDefault_Predicate(isAsync);

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[ContactName] DESC");
        }

        public override async Task Where_LastOrDefault(bool isAsync)
        {
            await base.Where_LastOrDefault(isAsync);

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[ContactName] DESC");
        }

        public override async Task Contains_with_subquery(bool isAsync)
        {
            await base.Contains_with_subquery(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
)");
        }

        public override async Task Contains_with_local_array_closure(bool isAsync)
        {
            await base.Contains_with_local_array_closure(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI')",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE')");
        }

        public override async Task Contains_with_subquery_and_local_array_closure(bool isAsync)
        {
            await base.Contains_with_subquery_and_local_array_closure(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c1]
    WHERE [c1].[City] IN (N'London', N'Buenos Aires') AND ([c1].[CustomerID] = [c].[CustomerID]))",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c1]
    WHERE [c1].[City] IN (N'London') AND ([c1].[CustomerID] = [c].[CustomerID]))");
        }

        public override async Task Contains_with_local_int_array_closure(bool isAsync)
        {
            await base.Contains_with_local_int_array_closure(isAsync);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] IN (0, 1)",
                //
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] IN (0)");
        }

        public override async Task Contains_with_local_nullable_int_array_closure(bool isAsync)
        {
            await base.Contains_with_local_nullable_int_array_closure(isAsync);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] IN (0, 1)",
                //
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] IN (0)");
        }

        public override async Task Contains_with_local_array_inline(bool isAsync)
        {
            await base.Contains_with_local_array_inline(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI')");
        }

        public override async Task Contains_with_local_list_closure(bool isAsync)
        {
            await base.Contains_with_local_list_closure(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI')");
        }

        public override async Task Contains_with_local_list_closure_all_null(bool isAsync)
        {
            await base.Contains_with_local_list_closure_all_null(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IS NULL");
        }

        public override async Task Contains_with_local_list_inline(bool isAsync)
        {
            await base.Contains_with_local_list_inline(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI')");
        }

        public override async Task Contains_with_local_list_inline_closure_mix(bool isAsync)
        {
            await base.Contains_with_local_list_inline_closure_mix(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI')",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ANATR')");
        }

        public override async Task Contains_with_local_non_primitive_list_inline_closure_mix(bool isAsync)
        {
            await base.Contains_with_local_non_primitive_list_inline_closure_mix(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI')",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ANATR')");
        }

        public override async Task Contains_with_local_non_primitive_list_closure_mix(bool isAsync)
        {
            await base.Contains_with_local_non_primitive_list_closure_mix(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI')");
        }

        public override async Task Contains_with_local_collection_false(bool isAsync)
        {
            await base.Contains_with_local_collection_false(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] NOT IN (N'ABCDE', N'ALFKI')");
        }

        public override async Task Contains_with_local_collection_complex_predicate_and(bool isAsync)
        {
            await base.Contains_with_local_collection_complex_predicate_and(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ALFKI', N'ABCDE') AND [c].[CustomerID] IN (N'ABCDE', N'ALFKI')");
        }

        public override async Task Contains_with_local_collection_complex_predicate_or(bool isAsync)
        {
            await base.Contains_with_local_collection_complex_predicate_or(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI', N'ALFKI', N'ABCDE')");
        }

        public override async Task Contains_with_local_collection_complex_predicate_not_matching_ins1(bool isAsync)
        {
            await base.Contains_with_local_collection_complex_predicate_not_matching_ins1(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ALFKI', N'ABCDE') OR [c].[CustomerID] NOT IN (N'ABCDE', N'ALFKI')");
        }

        public override async Task Contains_with_local_collection_complex_predicate_not_matching_ins2(bool isAsync)
        {
            await base.Contains_with_local_collection_complex_predicate_not_matching_ins2(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI') AND [c].[CustomerID] NOT IN (N'ALFKI', N'ABCDE')");
        }

        public override async Task Contains_with_local_collection_sql_injection(bool isAsync)
        {
            await base.Contains_with_local_collection_sql_injection(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ALFKI', N'ABC'')); GO; DROP TABLE Orders; GO; --', N'ALFKI', N'ABCDE')");
        }

        public override async Task Contains_with_local_collection_empty_closure(bool isAsync)
        {
            await base.Contains_with_local_collection_empty_closure(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1");
        }

        public override async Task Contains_with_local_collection_empty_inline(bool isAsync)
        {
            await base.Contains_with_local_collection_empty_inline(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Contains_top_level(bool isAsync)
        {
            await base.Contains_top_level(isAsync);

            AssertSql(
                @"@__p_0='ALFKI' (Size = 4000)

SELECT CASE
    WHEN @__p_0 IN (
        SELECT [c].[CustomerID]
        FROM [Customers] AS [c]
    )
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override async Task Contains_with_local_tuple_array_closure(bool isAsync)
        {
            await base.Contains_with_local_tuple_array_closure(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]",
                //
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]");
        }

        public override async Task Contains_with_local_anonymous_type_array_closure(bool isAsync)
        {
            await base.Contains_with_local_anonymous_type_array_closure(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]",
                //
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]");
        }

        public override void OfType_Select()
        {
            base.OfType_Select();

            AssertSql(
                @"SELECT TOP(1) [o.Customer].[City]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
ORDER BY [o].[OrderID]");
        }

        public override void OfType_Select_OfType_Select()
        {
            base.OfType_Select_OfType_Select();

            AssertSql(
                @"SELECT TOP(1) [o.Customer].[City]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
ORDER BY [o].[OrderID]");
        }

        public override async Task Average_with_non_matching_types_in_projection_doesnt_produce_second_explicit_cast(bool isAsync)
        {
            await base.Average_with_non_matching_types_in_projection_doesnt_produce_second_explicit_cast(isAsync);

            AssertSql(
                @"SELECT AVG(CAST(CAST([o].[OrderID] AS bigint) AS float))
FROM [Orders] AS [o]
WHERE [o].[CustomerID] LIKE N'A' + N'%' AND (LEFT([o].[CustomerID], LEN(N'A')) = N'A')");
        }

        public override async Task Max_with_non_matching_types_in_projection_introduces_explicit_cast(bool isAsync)
        {
            await base.Max_with_non_matching_types_in_projection_introduces_explicit_cast(isAsync);

            AssertSql(
                @"SELECT MAX(CAST([o].[OrderID] AS bigint))
FROM [Orders] AS [o]
WHERE [o].[CustomerID] LIKE N'A' + N'%' AND (LEFT([o].[CustomerID], LEN(N'A')) = N'A')");
        }

        public override async Task Min_with_non_matching_types_in_projection_introduces_explicit_cast(bool isAsync)
        {
            await base.Min_with_non_matching_types_in_projection_introduces_explicit_cast(isAsync);

            AssertSql(
                @"SELECT MIN(CAST([o].[OrderID] AS bigint))
FROM [Orders] AS [o]
WHERE [o].[CustomerID] LIKE N'A' + N'%' AND (LEFT([o].[CustomerID], LEN(N'A')) = N'A')");
        }

        public override async Task OrderBy_Take_Last_gives_correct_result(bool isAsync)
        {
            await base.OrderBy_Take_Last_gives_correct_result(isAsync);

            AssertSql(
                @"@__p_0='20'

SELECT TOP(1) [t].*
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [t]
ORDER BY [t].[CustomerID] DESC");
        }

        public override async Task OrderBy_Skip_Last_gives_correct_result(bool isAsync)
        {
            await base.OrderBy_Skip_Last_gives_correct_result(isAsync);

            AssertSql(
                @"@__p_0='20'

SELECT TOP(1) [t].*
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
    OFFSET @__p_0 ROWS
) AS [t]
ORDER BY [t].[CustomerID] DESC");
        }

        public override void Contains_over_entityType_should_rewrite_to_identity_equality()
        {
            base.Contains_over_entityType_should_rewrite_to_identity_equality();

            AssertSql(
                @"SELECT TOP(2) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] = 10248",
                //
                @"@__p_0_OrderID='10248'

SELECT CASE
    WHEN @__p_0_OrderID IN (
        SELECT [o].[OrderID]
        FROM [Orders] AS [o]
        WHERE [o].[CustomerID] = N'VINET'
    )
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void Contains_over_entityType_should_materialize_when_composite()
        {
            base.Contains_over_entityType_should_materialize_when_composite();

            AssertSql(
                @"SELECT TOP(1) [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 10248) AND ([o].[ProductID] = 42)",
                //
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE [o].[ProductID] = 42");
        }

        public override void Paging_operation_on_string_doesnt_issue_warning()
        {
            base.Paging_operation_on_string_doesnt_issue_warning();

            Assert.DoesNotContain(
                CoreStrings.LogFirstWithoutOrderByAndFilter.GenerateMessage(
                    "(from char <generated>_1 in [c].CustomerID select [<generated>_1]).FirstOrDefault()"), Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
        }

        public override async Task Project_constant_Sum(bool isAsync)
        {
            await base.Project_constant_Sum(isAsync);

            AssertSql(
                @"SELECT 1
FROM [Employees] AS [e]");
        }

        public override async Task Where_subquery_any_equals_operator(bool isAsync)
        {
            await base.Where_subquery_any_equals_operator(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI', N'ANATR')");
        }

        public override async Task Where_subquery_any_equals(bool isAsync)
        {
            await base.Where_subquery_any_equals(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI', N'ANATR')");
        }

        public override async Task Where_subquery_any_equals_static(bool isAsync)
        {
            await base.Where_subquery_any_equals_static(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI', N'ANATR')");
        }

        public override async Task Where_subquery_where_any(bool isAsync)
        {
            await base.Where_subquery_where_any(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[City] = N'México D.F.') AND [c].[CustomerID] IN (N'ABCDE', N'ALFKI', N'ANATR')");
        }

        public override async Task Where_subquery_all_not_equals_operator(bool isAsync)
        {
            await base.Where_subquery_all_not_equals_operator(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] NOT IN (N'ABCDE', N'ALFKI', N'ANATR')");
        }

        public override async Task Where_subquery_all_not_equals(bool isAsync)
        {
            await base.Where_subquery_all_not_equals(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] NOT IN (N'ABCDE', N'ALFKI', N'ANATR')");
        }

        public override async Task Where_subquery_all_not_equals_static(bool isAsync)
        {
            await base.Where_subquery_all_not_equals_static(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] NOT IN (N'ABCDE', N'ALFKI', N'ANATR')");
        }

        public override async Task Where_subquery_where_all(bool isAsync)
        {
            await base.Where_subquery_where_all(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[City] = N'México D.F.') AND [c].[CustomerID] NOT IN (N'ABCDE', N'ALFKI', N'ANATR')");
        }

        public override async Task Cast_to_same_Type_Count_works(bool isAsync)
        {
            await base.Cast_to_same_Type_Count_works(isAsync);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]");
        }
    }
}
