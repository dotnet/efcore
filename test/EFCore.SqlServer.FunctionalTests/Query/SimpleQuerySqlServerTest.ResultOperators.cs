// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class SimpleQuerySqlServerTest
    {
        public override async Task Union_with_custom_projection()
        {
            await base.Union_with_custom_projection();

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

        public override void Sum_with_no_arg()
        {
            base.Sum_with_no_arg();

            AssertSql(
                @"SELECT SUM([o].[OrderID])
FROM [Orders] AS [o]");
        }

        public override void Sum_with_binary_expression()
        {
            base.Sum_with_binary_expression();

            AssertSql(
                @"SELECT SUM([o].[OrderID] * 2)
FROM [Orders] AS [o]");
        }

        public override void Sum_with_arg()
        {
            base.Sum_with_arg();

            AssertSql(
                @"SELECT SUM([o].[OrderID])
FROM [Orders] AS [o]");
        }

        public override void Sum_with_arg_expression()
        {
            base.Sum_with_arg_expression();

            AssertSql(
                @"SELECT SUM([o].[OrderID] + [o].[OrderID])
FROM [Orders] AS [o]");
        }

        public override void Sum_with_division_on_decimal()
        {
            base.Sum_with_division_on_decimal();

            AssertSql(
                @"SELECT SUM([od].[Quantity] / 2.09)
FROM [Order Details] AS [od]");
        }

        public override void Sum_with_division_on_decimal_no_significant_digits()
        {
            base.Sum_with_division_on_decimal_no_significant_digits();

            AssertSql(
                @"SELECT SUM([od].[Quantity] / 2.0)
FROM [Order Details] AS [od]");
        }

        public override void Sum_with_coalesce()
        {
            base.Sum_with_coalesce();

            AssertSql(
                @"SELECT SUM(COALESCE([p].[UnitPrice], 0.0))
FROM [Products] AS [p]
WHERE [p].[ProductID] < 40");
        }

        public override void Sum_over_subquery_is_client_eval()
        {
            base.Sum_over_subquery_is_client_eval();

            AssertSql(
                @"SELECT (
    SELECT SUM([o].[OrderID])
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
)
FROM [Customers] AS [c]");
        }

        public override void Sum_on_float_column()
        {
            base.Sum_on_float_column();

            AssertSql(
                @"SELECT CAST(SUM([od].[Discount]) AS real)
FROM [Order Details] AS [od]
WHERE [od].[ProductID] = 1");
        }

        public override async Task Sum_on_float_column_in_subquery()
        {
            await base.Sum_on_float_column_in_subquery();

            AssertSql(
                @"SELECT [o].[OrderID], (
    SELECT CAST(SUM([od].[Discount]) AS real)
    FROM [Order Details] AS [od]
    WHERE [o].[OrderID] = [od].[OrderID]
) AS [Sum]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300");
        }

        public override void Average_with_no_arg()
        {
            base.Average_with_no_arg();

            AssertSql(
                @"SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]");
        }

        public override void Average_with_binary_expression()
        {
            base.Average_with_binary_expression();

            AssertSql(
                @"SELECT AVG(CAST([o].[OrderID] * 2 AS float))
FROM [Orders] AS [o]");
        }

        public override void Average_with_arg()
        {
            base.Average_with_arg();

            AssertSql(
                @"SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]");
        }

        public override void Average_with_arg_expression()
        {
            base.Average_with_arg_expression();

            AssertSql(
                @"SELECT AVG(CAST([o].[OrderID] + [o].[OrderID] AS float))
FROM [Orders] AS [o]");
        }

        public override void Average_with_division_on_decimal()
        {
            base.Average_with_division_on_decimal();

            AssertSql(
                @"SELECT AVG([od].[Quantity] / 2.09)
FROM [Order Details] AS [od]");
        }

        public override void Average_with_division_on_decimal_no_significant_digits()
        {
            base.Average_with_division_on_decimal_no_significant_digits();

            AssertSql(
                @"SELECT AVG([od].[Quantity] / 2.0)
FROM [Order Details] AS [od]");
        }

        public override void Average_with_coalesce()
        {
            base.Average_with_coalesce();

            AssertSql(
                @"SELECT AVG(COALESCE([p].[UnitPrice], 0.0))
FROM [Products] AS [p]
WHERE [p].[ProductID] < 40");
        }

        public override void Average_over_subquery_is_client_eval()
        {
            base.Average_over_subquery_is_client_eval();

            AssertSql(
                @"SELECT (
    SELECT SUM([o].[OrderID])
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
)
FROM [Customers] AS [c]");
        }

        public override void Average_on_float_column()
        {
            base.Average_on_float_column();

            AssertSql(
                @"SELECT CAST(AVG([od].[Discount]) AS real)
FROM [Order Details] AS [od]
WHERE [od].[ProductID] = 1");
        }

        public override async Task Average_on_float_column_in_subquery()
        {
            await base.Average_on_float_column_in_subquery();

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
                Fixture.TestSqlLoggerFactory.Log);
        }

        public override async Task Average_on_float_column_in_subquery_with_cast()
        {
            await base.Average_on_float_column_in_subquery_with_cast();

            AssertSql(
                @"SELECT [o].[OrderID], (
    SELECT CAST(AVG([od].[Discount]) AS real)
    FROM [Order Details] AS [od]
    WHERE [o].[OrderID] = [od].[OrderID]
) AS [Sum]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300");
        }

        public override void Min_with_no_arg()
        {
            base.Min_with_no_arg();

            AssertSql(
                @"SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]");
        }

        public override void Min_with_arg()
        {
            base.Min_with_arg();

            AssertSql(
                @"SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]");
        }

        public override void Min_with_coalesce()
        {
            base.Min_with_coalesce();

            AssertSql(
                @"SELECT MIN(COALESCE([p].[UnitPrice], 0.0))
FROM [Products] AS [p]
WHERE [p].[ProductID] < 40");
        }

        public override void Min_over_subquery_is_client_eval()
        {
            base.Min_over_subquery_is_client_eval();

            AssertSql(
                @"SELECT (
    SELECT SUM([o].[OrderID])
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
)
FROM [Customers] AS [c]");
        }

        public override void Max_with_no_arg()
        {
            base.Max_with_no_arg();

            AssertSql(
                @"SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]");
        }

        public override void Max_with_arg()
        {
            base.Max_with_arg();

            AssertSql(
                @"SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]");
        }

        public override void Max_with_coalesce()
        {
            base.Max_with_coalesce();

            AssertSql(
                @"SELECT MAX(COALESCE([p].[UnitPrice], 0.0))
FROM [Products] AS [p]
WHERE [p].[ProductID] < 40");
        }

        public override void Max_over_subquery_is_client_eval()
        {
            base.Max_over_subquery_is_client_eval();

            AssertSql(
                @"SELECT (
    SELECT SUM([o].[OrderID])
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
)
FROM [Customers] AS [c]");
        }

        public override void Count_with_predicate()
        {
            base.Count_with_predicate();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'");
        }

        public override void Where_OrderBy_Count()
        {
            base.Where_OrderBy_Count();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'");
        }

        public override void OrderBy_Where_Count()
        {
            base.OrderBy_Where_Count();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'");
        }

        public override void OrderBy_Count_with_predicate()
        {
            base.OrderBy_Count_with_predicate();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'");
        }

        public override void OrderBy_Where_Count_with_predicate()
        {
            base.OrderBy_Where_Count_with_predicate();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE ([o].[OrderID] > 10) AND (([o].[CustomerID] <> N'ALFKI') OR [o].[CustomerID] IS NULL)");
        }

        public override void Where_OrderBy_Count_client_eval()
        {
            base.Where_OrderBy_Count_client_eval();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override void Where_OrderBy_Count_client_eval_mixed()
        {
            base.Where_OrderBy_Count_client_eval_mixed();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[OrderID] > 10");
        }

        public override void OrderBy_Where_Count_client_eval()
        {
            base.OrderBy_Where_Count_client_eval();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override void OrderBy_Where_Count_client_eval_mixed()
        {
            base.OrderBy_Where_Count_client_eval_mixed();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override void OrderBy_Count_with_predicate_client_eval()
        {
            base.OrderBy_Count_with_predicate_client_eval();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override void OrderBy_Count_with_predicate_client_eval_mixed()
        {
            base.OrderBy_Count_with_predicate_client_eval_mixed();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override void OrderBy_Where_Count_with_predicate_client_eval()
        {
            base.OrderBy_Where_Count_with_predicate_client_eval();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override void OrderBy_Where_Count_with_predicate_client_eval_mixed()
        {
            base.OrderBy_Where_Count_with_predicate_client_eval_mixed();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] <> N'ALFKI') OR [o].[CustomerID] IS NULL");
        }

        public override async Task OrderBy_client_Take()
        {
            await base.OrderBy_client_Take();

            AssertSql(
                @"@__p_1='10'

SELECT TOP(@__p_1) [o].[EmployeeID], [o].[City], [o].[Country], [o].[FirstName], [o].[ReportsTo], [o].[Title]
FROM [Employees] AS [o]
ORDER BY (SELECT 1)");
        }

        public override async Task Distinct()
        {
            await base.Distinct();

            AssertSql(
                @"SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Distinct_Scalar()
        {
            await base.Distinct_Scalar();

            AssertSql(
                @"SELECT DISTINCT [c].[City]
FROM [Customers] AS [c]");
        }

        public override async Task OrderBy_Distinct()
        {
            await base.OrderBy_Distinct();

            // Ordering not preserved by distinct when ordering columns not projected.
            AssertSql(
                @"SELECT DISTINCT [c].[City]
FROM [Customers] AS [c]");
        }

        public override async Task Distinct_OrderBy()
        {
            await base.Distinct_OrderBy();

            AssertSql(
                @"SELECT [t].[Country]
FROM (
    SELECT DISTINCT [c].[Country]
    FROM [Customers] AS [c]
) AS [t]
ORDER BY [t].[Country]");
        }

        public override async Task Distinct_OrderBy2()
        {
            await base.Distinct_OrderBy2();

            AssertSql(
                @"SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
) AS [t]
ORDER BY [t].[CustomerID]");
        }

        public override async Task Distinct_OrderBy3()
        {
            await base.Distinct_OrderBy3();

            AssertSql(
                @"SELECT [t].[CustomerID]
FROM (
    SELECT DISTINCT [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [t]
ORDER BY [t].[CustomerID]");
        }

        public override void Distinct_Count()
        {
            base.Distinct_Count();

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].*
    FROM [Customers] AS [c]
) AS [t]");
        }

        public override void Select_Select_Distinct_Count()
        {
            base.Select_Select_Distinct_Count();

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[City]
    FROM [Customers] AS [c]
) AS [t]");
        }

        public override void Single_Predicate()
        {
            base.Single_Predicate();

            AssertSql(
                @"SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task FirstOrDefault_inside_subquery_gets_server_evaluated()
        {
            await base.FirstOrDefault_inside_subquery_gets_server_evaluated();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] = N'ALFKI') AND ((
    SELECT TOP(1) [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE ([o].[CustomerID] = N'ALFKI') AND ([c].[CustomerID] = [o].[CustomerID])
) = N'ALFKI')");
        }

        public override async Task First_inside_subquery_gets_client_evaluated()
        {
            await base.First_inside_subquery_gets_client_evaluated();

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

        public override void Last()
        {
            base.Last();

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactName] DESC");
        }

        public override void Last_when_no_order_by()
        {
            base.Last_when_no_order_by();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override void Last_Predicate()
        {
            base.Last_Predicate();

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[ContactName] DESC");
        }

        public override void Where_Last()
        {
            base.Where_Last();

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[ContactName] DESC");
        }

        public override void LastOrDefault()
        {
            base.LastOrDefault();

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactName] DESC");
        }

        public override void LastOrDefault_Predicate()
        {
            base.LastOrDefault_Predicate();

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[ContactName] DESC");
        }

        public override void Where_LastOrDefault()
        {
            base.Where_LastOrDefault();

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[ContactName] DESC");
        }

        public override async Task Contains_with_subquery()
        {
            await base.Contains_with_subquery();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
)");
        }

        public override async Task Contains_with_local_array_closure()
        {
            await base.Contains_with_local_array_closure();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI')",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE')");
        }

        public override async Task Contains_with_subquery_and_local_array_closure()
        {
            await base.Contains_with_subquery_and_local_array_closure();

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

        public override async Task Contains_with_local_int_array_closure()
        {
            await base.Contains_with_local_int_array_closure();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] IN (0, 1)",
                //
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] IN (0)");
        }

        public override async Task Contains_with_local_nullable_int_array_closure()
        {
            await base.Contains_with_local_nullable_int_array_closure();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] IN (0, 1)",
                //
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] IN (0)");
        }

        public override async Task Contains_with_local_array_inline()
        {
            await base.Contains_with_local_array_inline();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI')");
        }

        public override async Task Contains_with_local_list_closure()
        {
            await base.Contains_with_local_list_closure();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI')");
        }

        public override async Task Contains_with_local_list_closure_all_null()
        {
            await base.Contains_with_local_list_closure_all_null();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IS NULL");
        }

        public override async Task Contains_with_local_list_inline()
        {
            await base.Contains_with_local_list_inline();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI')");
        }

        public override async Task Contains_with_local_list_inline_closure_mix()
        {
            await base.Contains_with_local_list_inline_closure_mix();

            AssertSql(
                @"@__id_0='ALFKI' (Size = 5)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', @__id_0)",
                //
                @"@__id_0='ANATR' (Size = 5)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', @__id_0)");
        }

        public override async Task Contains_with_local_collection_false()
        {
            await base.Contains_with_local_collection_false();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] NOT IN (N'ABCDE', N'ALFKI')");
        }

        public override async Task Contains_with_local_collection_complex_predicate_and()
        {
            await base.Contains_with_local_collection_complex_predicate_and();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ALFKI', N'ABCDE') AND [c].[CustomerID] IN (N'ABCDE', N'ALFKI')");
        }

        public override async Task Contains_with_local_collection_complex_predicate_or()
        {
            await base.Contains_with_local_collection_complex_predicate_or();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI', N'ALFKI', N'ABCDE')");
        }

        public override async Task Contains_with_local_collection_complex_predicate_not_matching_ins1()
        {
            await base.Contains_with_local_collection_complex_predicate_not_matching_ins1();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ALFKI', N'ABCDE') OR [c].[CustomerID] NOT IN (N'ABCDE', N'ALFKI')");
        }

        public override async Task Contains_with_local_collection_complex_predicate_not_matching_ins2()
        {
            await base.Contains_with_local_collection_complex_predicate_not_matching_ins2();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI') AND [c].[CustomerID] NOT IN (N'ALFKI', N'ABCDE')");
        }

        public override async Task Contains_with_local_collection_sql_injection()
        {
            await base.Contains_with_local_collection_sql_injection();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ALFKI', N'ABC'')); GO; DROP TABLE Orders; GO; --', N'ALFKI', N'ABCDE')");
        }

        public override async Task Contains_with_local_collection_empty_closure()
        {
            await base.Contains_with_local_collection_empty_closure();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1");
        }

        public override async Task Contains_with_local_collection_empty_inline()
        {
            await base.Contains_with_local_collection_empty_inline();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Contains_top_level()
        {
            base.Contains_top_level();

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

        public override async Task Contains_with_local_tuple_array_closure()
        {
            await base.Contains_with_local_tuple_array_closure();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]",
                //
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]");
        }

        public override async Task Contains_with_local_anonymous_type_array_closure()
        {
            await base.Contains_with_local_anonymous_type_array_closure();

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

        public override void Average_with_non_matching_types_in_projection_doesnt_produce_second_explicit_cast()
        {
            base.Average_with_non_matching_types_in_projection_doesnt_produce_second_explicit_cast();

            AssertSql(
                @"SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
WHERE [o].[CustomerID] LIKE N'A' + N'%' AND (LEFT([o].[CustomerID], LEN(N'A')) = N'A')");
        }

        public override void Max_with_non_matching_types_in_projection_introduces_explicit_cast()
        {
            base.Max_with_non_matching_types_in_projection_introduces_explicit_cast();

            AssertSql(
                @"SELECT MAX(CAST([o].[OrderID] AS bigint))
FROM [Orders] AS [o]
WHERE [o].[CustomerID] LIKE N'A' + N'%' AND (LEFT([o].[CustomerID], LEN(N'A')) = N'A')");
        }

        public override void Min_with_non_matching_types_in_projection_introduces_explicit_cast()
        {
            base.Min_with_non_matching_types_in_projection_introduces_explicit_cast();

            AssertSql(
                @"SELECT MIN(CAST([o].[OrderID] AS bigint))
FROM [Orders] AS [o]
WHERE [o].[CustomerID] LIKE N'A' + N'%' AND (LEFT([o].[CustomerID], LEN(N'A')) = N'A')");
        }

        public override void OrderBy_Take_Last_gives_correct_result()
        {
            base.OrderBy_Take_Last_gives_correct_result();

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

        public override void OrderBy_Skip_Last_gives_correct_result()
        {
            base.OrderBy_Skip_Last_gives_correct_result();

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
                    @"(from char <generated>_1 in [c].CustomerID select [<generated>_1]).FirstOrDefault()"), Fixture.TestSqlLoggerFactory.Log);
        }

        public override void Project_constant_Sum()
        {
            base.Project_constant_Sum();

            AssertSql(
                @"SELECT 1
FROM [Employees] AS [e]");
        }

        public override async Task Where_subquery_any_equals_operator()
        {
            await base.Where_subquery_any_equals_operator();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI', N'ANATR')");
        }

        public override async Task Where_subquery_any_equals()
        {
            await base.Where_subquery_any_equals();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI', N'ANATR')");
        }

        public override async Task Where_subquery_any_equals_static()
        {
            await base.Where_subquery_any_equals_static();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI', N'ANATR')");
        }

        public override async Task Where_subquery_where_any()
        {
            await base.Where_subquery_where_any();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[City] = N'México D.F.') AND [c].[CustomerID] IN (N'ABCDE', N'ALFKI', N'ANATR')");
        }

        public override async Task Where_subquery_all_not_equals_operator()
        {
            await base.Where_subquery_all_not_equals_operator();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] NOT IN (N'ABCDE', N'ALFKI', N'ANATR')");
        }

        public override async Task Where_subquery_all_not_equals()
        {
            await base.Where_subquery_all_not_equals();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] NOT IN (N'ABCDE', N'ALFKI', N'ANATR')");
        }

        public override async Task Where_subquery_all_not_equals_static()
        {
            await base.Where_subquery_all_not_equals_static();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] NOT IN (N'ABCDE', N'ALFKI', N'ANATR')");
        }

        public override async Task Where_subquery_where_all()
        {
            await base.Where_subquery_where_all();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[City] = N'México D.F.') AND [c].[CustomerID] NOT IN (N'ABCDE', N'ALFKI', N'ANATR')");
        }

        public override void Cast_to_same_Type_Count_works()
        {
            base.Cast_to_same_Type_Count_works();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]");
        }
    }
}
