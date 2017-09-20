// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class GroupByQuerySqlServerTest : GroupByQueryTestBase<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        // ReSharper disable once UnusedParameter.Local
        public GroupByQuerySqlServerTest(NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void GroupBy_Property_Select_Average()
        {
            base.GroupBy_Property_Select_Average();

            AssertSql(
                @"SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_Select_Count()
        {
            base.GroupBy_Property_Select_Count();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_Select_LongCount()
        {
            base.GroupBy_Property_Select_LongCount();

            AssertSql(
                @"SELECT COUNT_BIG(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_Select_Max()
        {
            base.GroupBy_Property_Select_Max();

            AssertSql(
                @"SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_Select_Min()
        {
            base.GroupBy_Property_Select_Min();

            AssertSql(
                @"SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_Select_Sum()
        {
            base.GroupBy_Property_Select_Sum();

            AssertSql(
                @"SELECT SUM([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_Select_Sum_Min_Max_Avg()
        {
            base.GroupBy_Property_Select_Sum_Min_Max_Avg();

            AssertSql(
                @"SELECT SUM([o].[OrderID]), MIN([o].[OrderID]), MAX([o].[OrderID]), AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_Select_Key_Average()
        {
            base.GroupBy_Property_Select_Key_Average();

            AssertSql(
                @"SELECT [o].[CustomerID], AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_Select_Key_Count()
        {
            base.GroupBy_Property_Select_Key_Count();

            AssertSql(
                @"SELECT [o].[CustomerID], COUNT(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_Select_Key_LongCount()
        {
            base.GroupBy_Property_Select_Key_LongCount();

            AssertSql(
                @"SELECT [o].[CustomerID], COUNT_BIG(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_Select_Key_Max()
        {
            base.GroupBy_Property_Select_Key_Max();

            AssertSql(
                @"SELECT [o].[CustomerID], MAX([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_Select_Key_Min()
        {
            base.GroupBy_Property_Select_Key_Min();

            AssertSql(
                @"SELECT [o].[CustomerID], MIN([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_Select_Key_Sum()
        {
            base.GroupBy_Property_Select_Key_Sum();

            AssertSql(
                @"SELECT [o].[CustomerID], SUM([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_Select_Key_Sum_Min_Max_Avg()
        {
            base.GroupBy_Property_Select_Key_Sum_Min_Max_Avg();

            AssertSql(
                @"SELECT [o].[CustomerID], SUM([o].[OrderID]), MIN([o].[OrderID]), MAX([o].[OrderID]), AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_Select_Sum_Min_Key_Max_Avg()
        {
            base.GroupBy_Property_Select_Sum_Min_Key_Max_Avg();

            AssertSql(
                @"SELECT SUM([o].[OrderID]), MIN([o].[OrderID]), [o].[CustomerID], MAX([o].[OrderID]), AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_anonymous_Select_Average()
        {
            base.GroupBy_anonymous_Select_Average();

            AssertSql(
                @"SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_anonymous_Select_Count()
        {
            base.GroupBy_anonymous_Select_Count();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_anonymous_Select_LongCount()
        {
            base.GroupBy_anonymous_Select_LongCount();

            AssertSql(
                @"SELECT COUNT_BIG(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_anonymous_Select_Max()
        {
            base.GroupBy_anonymous_Select_Max();

            AssertSql(
                @"SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_anonymous_Select_Min()
        {
            base.GroupBy_anonymous_Select_Min();

            AssertSql(
                @"SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_anonymous_Select_Sum()
        {
            base.GroupBy_anonymous_Select_Sum();

            AssertSql(
                @"SELECT SUM([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_anonymous_Select_Sum_Min_Max_Avg()
        {
            base.GroupBy_anonymous_Select_Sum_Min_Max_Avg();

            AssertSql(
                @"SELECT SUM([o].[OrderID]), MIN([o].[OrderID]), MAX([o].[OrderID]), AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Composite_Select_Average()
        {
            base.GroupBy_Composite_Select_Average();

            AssertSql(
                @"SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override void GroupBy_Composite_Select_Count()
        {
            base.GroupBy_Composite_Select_Count();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override void GroupBy_Composite_Select_LongCount()
        {
            base.GroupBy_Composite_Select_LongCount();

            AssertSql(
                @"SELECT COUNT_BIG(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override void GroupBy_Composite_Select_Max()
        {
            base.GroupBy_Composite_Select_Max();

            AssertSql(
                @"SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override void GroupBy_Composite_Select_Min()
        {
            base.GroupBy_Composite_Select_Min();

            AssertSql(
                @"SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override void GroupBy_Composite_Select_Sum()
        {
            base.GroupBy_Composite_Select_Sum();

            AssertSql(
                @"SELECT SUM([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override void GroupBy_Composite_Select_Sum_Min_Max_Avg()
        {
            base.GroupBy_Composite_Select_Sum_Min_Max_Avg();

            AssertSql(
                @"SELECT SUM([o].[OrderID]), MIN([o].[OrderID]), MAX([o].[OrderID]), AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override void GroupBy_Composite_Select_Key_Average()
        {
            base.GroupBy_Composite_Select_Key_Average();

            AssertSql(
                @"SELECT [o].[CustomerID], [o].[EmployeeID], AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override void GroupBy_Composite_Select_Key_Count()
        {
            base.GroupBy_Composite_Select_Key_Count();

            AssertSql(
                @"SELECT [o].[CustomerID], [o].[EmployeeID], COUNT(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override void GroupBy_Composite_Select_Key_LongCount()
        {
            base.GroupBy_Composite_Select_Key_LongCount();

            AssertSql(
                @"SELECT [o].[CustomerID], [o].[EmployeeID], COUNT_BIG(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override void GroupBy_Composite_Select_Key_Max()
        {
            base.GroupBy_Composite_Select_Key_Max();

            AssertSql(
                @"SELECT [o].[CustomerID], [o].[EmployeeID], MAX([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override void GroupBy_Composite_Select_Key_Min()
        {
            base.GroupBy_Composite_Select_Key_Min();

            AssertSql(
                @"SELECT [o].[CustomerID], [o].[EmployeeID], MIN([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override void GroupBy_Composite_Select_Key_Sum()
        {
            base.GroupBy_Composite_Select_Key_Sum();

            AssertSql(
                @"SELECT [o].[CustomerID], [o].[EmployeeID], SUM([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override void GroupBy_Composite_Select_Key_Sum_Min_Max_Avg()
        {
            base.GroupBy_Composite_Select_Key_Sum_Min_Max_Avg();

            AssertSql(
                @"SELECT [o].[CustomerID], [o].[EmployeeID], SUM([o].[OrderID]), MIN([o].[OrderID]), MAX([o].[OrderID]), AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override void GroupBy_Composite_Select_Sum_Min_Key_Max_Avg()
        {
            base.GroupBy_Composite_Select_Sum_Min_Key_Max_Avg();

            AssertSql(
                @"SELECT SUM([o].[OrderID]), MIN([o].[OrderID]), [o].[CustomerID], [o].[EmployeeID], MAX([o].[OrderID]), AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override void GroupBy_Composite_Select_Sum_Min_Key_flattened_Max_Avg()
        {
            base.GroupBy_Composite_Select_Sum_Min_Key_flattened_Max_Avg();

            AssertSql(
                @"SELECT SUM([o].[OrderID]), MIN([o].[OrderID]), [o].[CustomerID], [o].[EmployeeID], MAX([o].[OrderID]), AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override void GroupBy_Composite_Select_Sum_Min_part_Key_flattened_Max_Avg()
        {
            base.GroupBy_Composite_Select_Sum_Min_part_Key_flattened_Max_Avg();

            AssertSql(
                @"SELECT SUM([o].[OrderID]), MIN([o].[OrderID]), [o].[CustomerID], MAX([o].[OrderID]), AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override void GroupBy_Property_scalar_element_selector_Average()
        {
            base.GroupBy_Property_scalar_element_selector_Average();

            AssertSql(
                @"SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_scalar_element_selector_Count()
        {
            base.GroupBy_Property_scalar_element_selector_Count();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_scalar_element_selector_LongCount()
        {
            base.GroupBy_Property_scalar_element_selector_LongCount();

            AssertSql(
                @"SELECT COUNT_BIG(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_scalar_element_selector_Max()
        {
            base.GroupBy_Property_scalar_element_selector_Max();

            AssertSql(
                @"SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_scalar_element_selector_Min()
        {
            base.GroupBy_Property_scalar_element_selector_Min();

            AssertSql(
                @"SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_scalar_element_selector_Sum()
        {
            base.GroupBy_Property_scalar_element_selector_Sum();

            AssertSql(
                @"SELECT SUM([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_scalar_element_selector_Sum_Min_Max_Avg()
        {
            base.GroupBy_Property_scalar_element_selector_Sum_Min_Max_Avg();

            AssertSql(
                @"SELECT SUM([o].[OrderID]), MIN([o].[OrderID]), MAX([o].[OrderID]), AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_anonymous_element_selector_Average()
        {
            base.GroupBy_Property_anonymous_element_selector_Average();

            AssertSql(
                @"SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_anonymous_element_selector_Count()
        {
            base.GroupBy_Property_anonymous_element_selector_Count();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_anonymous_element_selector_LongCount()
        {
            base.GroupBy_Property_anonymous_element_selector_LongCount();

            AssertSql(
                @"SELECT COUNT_BIG(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_anonymous_element_selector_Max()
        {
            base.GroupBy_Property_anonymous_element_selector_Max();

            AssertSql(
                @"SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_anonymous_element_selector_Min()
        {
            base.GroupBy_Property_anonymous_element_selector_Min();

            AssertSql(
                @"SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_anonymous_element_selector_Sum()
        {
            base.GroupBy_Property_anonymous_element_selector_Sum();

            AssertSql(
                @"SELECT SUM([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_Property_anonymous_element_selector_Sum_Min_Max_Avg()
        {
            base.GroupBy_Property_anonymous_element_selector_Sum_Min_Max_Avg();

            AssertSql(
                @"SELECT SUM([o].[OrderID]), MIN([o].[EmployeeID]), MAX([o].[EmployeeID]), AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupBy_with_result_selector()
        {
            base.GroupBy_with_result_selector();

            AssertSql(
                @"SELECT SUM([o].[OrderID]), MIN([o].[OrderID]), MAX([o].[OrderID]), AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void OrderBy_GroupBy_Aggregate()
        {
            base.OrderBy_GroupBy_Aggregate();

            AssertSql(
                @"SELECT SUM([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override void OrderBy_Skip_GroupBy_Aggregate()
        {
            base.OrderBy_Skip_GroupBy_Aggregate();

            AssertSql(
                @"@__p_0='80'

SELECT AVG(CAST([t].[OrderID] AS float))
FROM (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS
) AS [t]
GROUP BY [t].[CustomerID]");
        }

        public override void OrderBy_Take_GroupBy_Aggregate()
        {
            base.OrderBy_Take_GroupBy_Aggregate();

            AssertSql(
                @"@__p_0='500'

SELECT MIN([t].[OrderID])
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]
GROUP BY [t].[CustomerID]");
        }

        public override void OrderBy_Skip_Take_GroupBy_Aggregate()
        {
            base.OrderBy_Skip_Take_GroupBy_Aggregate();

            AssertSql(
                @"@__p_0='80'
@__p_1='500'

SELECT MAX([t].[OrderID])
FROM (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
GROUP BY [t].[CustomerID]");
        }

        public override void Distinct_GroupBy_Aggregate()
        {
            base.Distinct_GroupBy_Aggregate();

            AssertSql(
                @"SELECT [t].[CustomerID], COUNT(*)
FROM (
    SELECT DISTINCT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
) AS [t]
GROUP BY [t].[CustomerID]");
        }

        public override void Anonymous_projection_Distinct_GroupBy_Aggregate()
        {
            base.Anonymous_projection_Distinct_GroupBy_Aggregate();

            AssertSql(
                @"SELECT [t].[EmployeeID], COUNT(*)
FROM (
    SELECT DISTINCT [o].[OrderID], [o].[EmployeeID]
    FROM [Orders] AS [o]
) AS [t]
GROUP BY [t].[EmployeeID]");
        }

        public override void SelectMany_GroupBy_Aggregate()
        {
            base.SelectMany_GroupBy_Aggregate();

            AssertSql(
                @"SELECT [c.Orders].[EmployeeID], COUNT(*)
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [c.Orders] ON [c].[CustomerID] = [c.Orders].[CustomerID]
GROUP BY [c.Orders].[EmployeeID]");
        }

        public override void Join_GroupBy_Aggregate()
        {
            base.Join_GroupBy_Aggregate();

            AssertSql(
                @"SELECT [c].[CustomerID], AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
INNER JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [c].[CustomerID]");
        }

        public override void Join_complex_GroupBy_Aggregate()
        {
            base.Join_complex_GroupBy_Aggregate();

            AssertSql(
                @"@__p_0='100'
@__p_1='10'
@__p_2='50'

SELECT [t0].[CustomerID], AVG(CAST([t].[OrderID] AS float))
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10400
    ORDER BY [o].[OrderDate]
) AS [t]
INNER JOIN (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] NOT IN (N'DRACD', N'FOLKO')
    ORDER BY [c].[City]
    OFFSET @__p_1 ROWS FETCH NEXT @__p_2 ROWS ONLY
) AS [t0] ON [t].[CustomerID] = [t0].[CustomerID]
GROUP BY [t0].[CustomerID]");
        }

        public override void GroupJoin_GroupBy_Aggregate()
        {
            base.GroupJoin_GroupBy_Aggregate();

            AssertSql(
                @"SELECT [o].[CustomerID], AVG(CAST([o].[OrderID] AS float))
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupJoin_GroupBy_Aggregate_2()
        {
            base.GroupJoin_GroupBy_Aggregate_2();

            AssertSql(
                @"SELECT [c].[CustomerID], MAX([c].[City])
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
GROUP BY [c].[CustomerID]");
        }

        public override void GroupJoin_GroupBy_Aggregate_3()
        {
            base.GroupJoin_GroupBy_Aggregate_3();

            AssertSql(
                @"SELECT [o].[CustomerID], AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
INNER JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [o].[CustomerID]");
        }

        public override void GroupJoin_complex_GroupBy_Aggregate()
        {
            base.GroupJoin_complex_GroupBy_Aggregate();

            AssertSql(
                @"@__p_0='10'
@__p_1='50'
@__p_2='100'

SELECT [t0].[CustomerID], AVG(CAST([t0].[OrderID] AS float))
FROM (
    SELECT [c].*
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] NOT IN (N'DRACD', N'FOLKO')
    ORDER BY [c].[City]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
INNER JOIN (
    SELECT TOP(@__p_2) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10400
    ORDER BY [o].[OrderDate]
) AS [t0] ON [t].[CustomerID] = [t0].[CustomerID]
WHERE [t0].[OrderID] > 10300
GROUP BY [t0].[CustomerID]");
        }

        public override void Self_join_GroupBy_Aggregate()
        {
            base.Self_join_GroupBy_Aggregate();

            AssertSql(
                @"SELECT [o].[CustomerID], AVG(CAST([o2].[OrderID] AS float))
FROM [Orders] AS [o]
INNER JOIN [Orders] AS [o2] ON [o].[OrderID] = [o2].[OrderID]
WHERE [o].[OrderID] < 10400
GROUP BY [o].[CustomerID]");
        }

        public override void Union_simple_groupby()
        {
            base.Union_simple_groupby();

            AssertSql(" ");
        }

        public override void GroupBy_Sum_constant()
        {
            base.GroupBy_Sum_constant();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
ORDER BY [o].[CustomerID]");
        }

        public override void Distinct_GroupBy_OrderBy_key()
        {
            base.Distinct_GroupBy_OrderBy_key();

            AssertSql(
                @"SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM (
    SELECT DISTINCT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
) AS [t]
ORDER BY [t].[CustomerID]");
        }

        public override void Select_nested_collection_with_groupby()
        {
            base.Select_nested_collection_with_groupby();

            AssertSql(
                @"SELECT (
    SELECT CASE
        WHEN EXISTS (
            SELECT 1
            FROM [Orders] AS [o0]
            WHERE [c].[CustomerID] = [o0].[CustomerID])
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END
), [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')",
                //
                @"@_outer_CustomerID='ALFKI' (Size = 5)

SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate]
FROM [Orders] AS [o1]
WHERE @_outer_CustomerID = [o1].[CustomerID]
ORDER BY [o1].[OrderID]",
                //
                @"@_outer_CustomerID='ANATR' (Size = 5)

SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate]
FROM [Orders] AS [o1]
WHERE @_outer_CustomerID = [o1].[CustomerID]
ORDER BY [o1].[OrderID]",
                //
                @"@_outer_CustomerID='ANTON' (Size = 5)

SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate]
FROM [Orders] AS [o1]
WHERE @_outer_CustomerID = [o1].[CustomerID]
ORDER BY [o1].[OrderID]",
                //
                @"@_outer_CustomerID='AROUT' (Size = 5)

SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate]
FROM [Orders] AS [o1]
WHERE @_outer_CustomerID = [o1].[CustomerID]
ORDER BY [o1].[OrderID]");
        }

        public override void Select_GroupBy_All()
        {
            base.Select_GroupBy_All();

            AssertSql(
                @"SELECT [o].[OrderID] AS [Order], [o].[CustomerID] AS [Customer]
FROM [Orders] AS [o]
ORDER BY [o].[CustomerID]");
        }

        public override void GroupBy_anonymous()
        {
            base.GroupBy_anonymous();

            AssertSql(
                @"SELECT [c].[City], [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[City]");
        }

        public override void GroupBy_anonymous_with_where()
        {
            base.GroupBy_anonymous_with_where();

            AssertSql(
                @"SELECT [c].[City], [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[Country] IN (N'Argentina', N'Austria', N'Brazil', N'France', N'Germany', N'USA')
ORDER BY [c].[City]");
        }

        public override void GroupBy_anonymous_subquery()
        {
            base.GroupBy_anonymous_subquery();

            AssertSql(" ");
        }

        public override void GroupBy_nested_order_by_enumerable()
        {
            base.GroupBy_nested_order_by_enumerable();

            AssertSql(
                @"SELECT [c].[Country], [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[Country]");
        }

        public override void GroupBy_join_default_if_empty_anonymous()
        {
            base.GroupBy_join_default_if_empty_anonymous();

            AssertSql(
                @"SELECT [order0].[OrderID], [order0].[CustomerID], [order0].[EmployeeID], [order0].[OrderDate], [orderDetail0].[OrderID], [orderDetail0].[ProductID], [orderDetail0].[Discount], [orderDetail0].[Quantity], [orderDetail0].[UnitPrice]
FROM [Orders] AS [order0]
LEFT JOIN [Order Details] AS [orderDetail0] ON [order0].[OrderID] = [orderDetail0].[OrderID]
ORDER BY [order0].[OrderID]");
        }

        public override void GroupBy_SelectMany()
        {
            base.GroupBy_SelectMany();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[City]");
        }

        public override void GroupBy_simple()
        {
            base.GroupBy_simple();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
ORDER BY [o].[CustomerID]");
        }

        public override void GroupBy_simple2()
        {
            base.GroupBy_simple2();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
ORDER BY [o].[CustomerID]");
        }

        public override void GroupBy_first()
        {
            base.GroupBy_first();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[CustomerID]");
        }

        public override void GroupBy_with_element_selector()
        {
            base.GroupBy_with_element_selector();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
ORDER BY [o].[CustomerID]");
        }

        public override void GroupBy_with_element_selector2()
        {
            base.GroupBy_with_element_selector2();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
ORDER BY [o].[CustomerID]");
        }

        public override void GroupBy_with_element_selector3()
        {
            base.GroupBy_with_element_selector3();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
ORDER BY [e].[EmployeeID]");
        }

        public override void GroupBy_DateTimeOffset_Property()
        {
            base.GroupBy_DateTimeOffset_Property();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL
ORDER BY DATEPART(month, [o].[OrderDate])");
        }

        public override void OrderBy_GroupBy_SelectMany()
        {
            base.OrderBy_GroupBy_SelectMany();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
ORDER BY [o].[CustomerID], [o].[OrderID]");
        }

        public override void OrderBy_GroupBy_SelectMany_shadow()
        {
            base.OrderBy_GroupBy_SelectMany_shadow();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
ORDER BY [e].[EmployeeID]");
        }

        public override void GroupBy_with_orderby()
        {
            base.GroupBy_with_orderby();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
ORDER BY [o].[CustomerID], [o].[OrderID]");
        }

        public override void GroupBy_with_orderby_and_anonymous_projection()
        {
            base.GroupBy_with_orderby_and_anonymous_projection();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
ORDER BY [o].[CustomerID]");
        }

        public override void GroupBy_with_orderby_take_skip_distinct()
        {
            base.GroupBy_with_orderby_take_skip_distinct();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
ORDER BY [o].[CustomerID]");
        }

        public override void GroupBy_join_anonymous()
        {
            base.GroupBy_join_anonymous();

            AssertSql(
                @"SELECT [order0].[OrderID], [order0].[CustomerID], [order0].[EmployeeID], [order0].[OrderDate], [orderDetail0].[OrderID], [orderDetail0].[ProductID], [orderDetail0].[Discount], [orderDetail0].[Quantity], [orderDetail0].[UnitPrice]
FROM [Orders] AS [order0]
LEFT JOIN [Order Details] AS [orderDetail0] ON [order0].[OrderID] = [orderDetail0].[OrderID]
ORDER BY [order0].[OrderID]");
        }

        public override void GroupBy_Distinct()
        {
            base.GroupBy_Distinct();

            AssertSql(
                @"SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o0]
ORDER BY [o0].[CustomerID]");
        }

        public override void OrderBy_Skip_GroupBy()
        {
            base.OrderBy_Skip_GroupBy();

            AssertSql(
                @"@__p_0='800'

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderDate]
    OFFSET @__p_0 ROWS
) AS [t]
ORDER BY [t].[CustomerID]");
        }

        public override void OrderBy_Take_GroupBy()
        {
            base.OrderBy_Take_GroupBy();

            AssertSql(
                @"@__p_0='50'

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderDate]
) AS [t]
ORDER BY [t].[CustomerID]");
        }

        public override void OrderBy_Skip_Take_GroupBy()
        {
            base.OrderBy_Skip_Take_GroupBy();

            AssertSql(
                @"@__p_0='450'
@__p_1='50'

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderDate]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
ORDER BY [t].[CustomerID]");
        }

        public override void Select_Distinct_GroupBy()
        {
            base.Select_Distinct_GroupBy();

            AssertSql(
                @"SELECT [t].[CustomerID], [t].[EmployeeID]
FROM (
    SELECT DISTINCT [o].[CustomerID], [o].[EmployeeID]
    FROM [Orders] AS [o]
) AS [t]
ORDER BY [t].[CustomerID]");
        }

        public override void GroupBy_with_aggregate_through_navigation_property()
        {
            base.GroupBy_with_aggregate_through_navigation_property();

            AssertSql(
                @"SELECT [c].[OrderID], [c].[CustomerID], [c].[EmployeeID], [c].[OrderDate]
FROM [Orders] AS [c]
ORDER BY [c].[EmployeeID]",
                //
                @"SELECT [i.Customer0].[CustomerID], [i.Customer0].[Region]
FROM [Customers] AS [i.Customer0]",
                //
                @"SELECT [i.Customer0].[CustomerID], [i.Customer0].[Region]
FROM [Customers] AS [i.Customer0]");
        }

        public override void GroupBy_Shadow()
        {
            base.GroupBy_Shadow();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE ([e].[Title] = N'Sales Representative') AND ([e].[EmployeeID] = 1)
ORDER BY [e].[Title]");
        }

        public override void GroupBy_Shadow2()
        {
            base.GroupBy_Shadow2();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE ([e].[Title] = N'Sales Representative') AND ([e].[EmployeeID] = 1)
ORDER BY [e].[Title]");
        }

        public override void GroupBy_Shadow3()
        {
            base.GroupBy_Shadow3();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = 1
ORDER BY [e].[EmployeeID]");
        }

        public override void Select_GroupBy()
        {
            base.Select_GroupBy();

            AssertSql(
                @"SELECT [o].[OrderID] AS [Order], [o].[CustomerID] AS [Customer]
FROM [Orders] AS [o]
ORDER BY [o].[CustomerID]");
        }

        public override void Select_GroupBy_SelectMany()
        {
            base.Select_GroupBy_SelectMany();

            AssertSql(
                @"SELECT [o].[OrderID] AS [Order], [o].[CustomerID] AS [Customer]
FROM [Orders] AS [o]
ORDER BY [o].[OrderID]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
