// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindGroupByQuerySqlServerTest : NorthwindGroupByQueryRelationalTestBase<
        NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        // ReSharper disable once UnusedParameter.Local
        public NorthwindGroupByQuerySqlServerTest(
            NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override bool CanExecuteQueryString
            => true;

        public override async Task GroupBy_Property_Select_Average(bool async)
        {
            await base.GroupBy_Property_Select_Average(async);

            AssertSql(
                @"SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");

            // Validating that we don't generate warning when translating GroupBy. See Issue#11157
            Assert.DoesNotContain(
                "The LINQ expression 'GroupBy([o].CustomerID, [o])' could not be translated and will be evaluated locally.",
                Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
        }

        public override async Task GroupBy_Property_Select_Average_with_group_enumerable_projected(bool async)
        {
            await base.GroupBy_Property_Select_Average_with_group_enumerable_projected(async);

            AssertSql(
                @"");
        }

        public override async Task GroupBy_Property_Select_Count(bool async)
        {
            await base.GroupBy_Property_Select_Count(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_Select_LongCount(bool async)
        {
            await base.GroupBy_Property_Select_LongCount(async);

            AssertSql(
                @"SELECT COUNT_BIG(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_Select_Max(bool async)
        {
            await base.GroupBy_Property_Select_Max(async);

            AssertSql(
                @"SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_Select_Min(bool async)
        {
            await base.GroupBy_Property_Select_Min(async);

            AssertSql(
                @"SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_Select_Sum(bool async)
        {
            await base.GroupBy_Property_Select_Sum(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_Select_Sum_Min_Max_Avg(bool async)
        {
            await base.GroupBy_Property_Select_Sum_Min_Max_Avg(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_Select_Key_Average(bool async)
        {
            await base.GroupBy_Property_Select_Key_Average(async);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Key], AVG(CAST([o].[OrderID] AS float)) AS [Average]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_Select_Key_Count(bool async)
        {
            await base.GroupBy_Property_Select_Key_Count(async);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Key], COUNT(*) AS [Count]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_Select_Key_LongCount(bool async)
        {
            await base.GroupBy_Property_Select_Key_LongCount(async);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Key], COUNT_BIG(*) AS [LongCount]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_Select_Key_Max(bool async)
        {
            await base.GroupBy_Property_Select_Key_Max(async);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Key], MAX([o].[OrderID]) AS [Max]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_Select_Key_Min(bool async)
        {
            await base.GroupBy_Property_Select_Key_Min(async);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Key], MIN([o].[OrderID]) AS [Min]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_Select_Key_Sum(bool async)
        {
            await base.GroupBy_Property_Select_Key_Sum(async);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Key], COALESCE(SUM([o].[OrderID]), 0) AS [Sum]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_Select_Key_Sum_Min_Max_Avg(bool async)
        {
            await base.GroupBy_Property_Select_Key_Sum_Min_Max_Avg(async);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Key], COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_Select_Sum_Min_Key_Max_Avg(bool async)
        {
            await base.GroupBy_Property_Select_Sum_Min_Key_Max_Avg(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], [o].[CustomerID] AS [Key], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_Select_key_multiple_times_and_aggregate(bool async)
        {
            await base.GroupBy_Property_Select_key_multiple_times_and_aggregate(async);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Key1], COALESCE(SUM([o].[OrderID]), 0) AS [Sum]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_Select_Key_with_constant(bool async)
        {
            await base.GroupBy_Property_Select_Key_with_constant(async);

            AssertSql(
                @"SELECT N'CustomerID' AS [Name], [o].[CustomerID] AS [Value], COUNT(*) AS [Count]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_aggregate_projecting_conditional_expression(bool async)
        {
            await base.GroupBy_aggregate_projecting_conditional_expression(async);

            AssertSql(
                @"SELECT [o].[OrderDate] AS [Key], CASE
    WHEN COUNT(*) = 0 THEN 1
    ELSE COALESCE(SUM(CASE
        WHEN ([o].[OrderID] % 2) = 0 THEN 1
        ELSE 0
    END), 0) / COUNT(*)
END AS [SomeValue]
FROM [Orders] AS [o]
GROUP BY [o].[OrderDate]");
        }

        public override async Task GroupBy_aggregate_projecting_conditional_expression_based_on_group_key(bool async)
        {
            await base.GroupBy_aggregate_projecting_conditional_expression_based_on_group_key(async);

            AssertSql(
                @"SELECT CASE
    WHEN [o].[OrderDate] IS NULL THEN N'is null'
    ELSE N'is not null'
END AS [Key], COALESCE(SUM([o].[OrderID]), 0) AS [Sum]
FROM [Orders] AS [o]
GROUP BY [o].[OrderDate]");
        }

        public override async Task GroupBy_anonymous_Select_Average(bool async)
        {
            await base.GroupBy_anonymous_Select_Average(async);

            AssertSql(
                @"SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_anonymous_Select_Count(bool async)
        {
            await base.GroupBy_anonymous_Select_Count(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_anonymous_Select_LongCount(bool async)
        {
            await base.GroupBy_anonymous_Select_LongCount(async);

            AssertSql(
                @"SELECT COUNT_BIG(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_anonymous_Select_Max(bool async)
        {
            await base.GroupBy_anonymous_Select_Max(async);

            AssertSql(
                @"SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_anonymous_Select_Min(bool async)
        {
            await base.GroupBy_anonymous_Select_Min(async);

            AssertSql(
                @"SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_anonymous_Select_Sum(bool async)
        {
            await base.GroupBy_anonymous_Select_Sum(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_anonymous_Select_Sum_Min_Max_Avg(bool async)
        {
            await base.GroupBy_anonymous_Select_Sum_Min_Max_Avg(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_anonymous_with_alias_Select_Key_Sum(bool async)
        {
            await base.GroupBy_anonymous_with_alias_Select_Key_Sum(async);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Key], COALESCE(SUM([o].[OrderID]), 0) AS [Sum]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Composite_Select_Average(bool async)
        {
            await base.GroupBy_Composite_Select_Average(async);

            AssertSql(
                @"SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override async Task GroupBy_Composite_Select_Count(bool async)
        {
            await base.GroupBy_Composite_Select_Count(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override async Task GroupBy_Composite_Select_LongCount(bool async)
        {
            await base.GroupBy_Composite_Select_LongCount(async);

            AssertSql(
                @"SELECT COUNT_BIG(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override async Task GroupBy_Composite_Select_Max(bool async)
        {
            await base.GroupBy_Composite_Select_Max(async);

            AssertSql(
                @"SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override async Task GroupBy_Composite_Select_Min(bool async)
        {
            await base.GroupBy_Composite_Select_Min(async);

            AssertSql(
                @"SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override async Task GroupBy_Composite_Select_Sum(bool async)
        {
            await base.GroupBy_Composite_Select_Sum(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override async Task GroupBy_Composite_Select_Sum_Min_Max_Avg(bool async)
        {
            await base.GroupBy_Composite_Select_Sum_Min_Max_Avg(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override async Task GroupBy_Composite_Select_Key_Average(bool async)
        {
            await base.GroupBy_Composite_Select_Key_Average(async);

            AssertSql(
                @"SELECT [o].[CustomerID], [o].[EmployeeID], AVG(CAST([o].[OrderID] AS float)) AS [Average]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override async Task GroupBy_Composite_Select_Key_Count(bool async)
        {
            await base.GroupBy_Composite_Select_Key_Count(async);

            AssertSql(
                @"SELECT [o].[CustomerID], [o].[EmployeeID], COUNT(*) AS [Count]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override async Task GroupBy_Composite_Select_Key_LongCount(bool async)
        {
            await base.GroupBy_Composite_Select_Key_LongCount(async);

            AssertSql(
                @"SELECT [o].[CustomerID], [o].[EmployeeID], COUNT_BIG(*) AS [LongCount]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override async Task GroupBy_Composite_Select_Key_Max(bool async)
        {
            await base.GroupBy_Composite_Select_Key_Max(async);

            AssertSql(
                @"SELECT [o].[CustomerID], [o].[EmployeeID], MAX([o].[OrderID]) AS [Max]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override async Task GroupBy_Composite_Select_Key_Min(bool async)
        {
            await base.GroupBy_Composite_Select_Key_Min(async);

            AssertSql(
                @"SELECT [o].[CustomerID], [o].[EmployeeID], MIN([o].[OrderID]) AS [Min]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override async Task GroupBy_Composite_Select_Key_Sum(bool async)
        {
            await base.GroupBy_Composite_Select_Key_Sum(async);

            AssertSql(
                @"SELECT [o].[CustomerID], [o].[EmployeeID], COALESCE(SUM([o].[OrderID]), 0) AS [Sum]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override async Task GroupBy_Composite_Select_Key_Sum_Min_Max_Avg(bool async)
        {
            await base.GroupBy_Composite_Select_Key_Sum_Min_Max_Avg(async);

            AssertSql(
                @"SELECT [o].[CustomerID], [o].[EmployeeID], COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override async Task GroupBy_Composite_Select_Sum_Min_Key_Max_Avg(bool async)
        {
            await base.GroupBy_Composite_Select_Sum_Min_Key_Max_Avg(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], [o].[CustomerID], [o].[EmployeeID], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override async Task GroupBy_Composite_Select_Sum_Min_Key_flattened_Max_Avg(bool async)
        {
            await base.GroupBy_Composite_Select_Sum_Min_Key_flattened_Max_Avg(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], [o].[CustomerID], [o].[EmployeeID], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override async Task GroupBy_Dto_as_key_Select_Sum(bool async)
        {
            await base.GroupBy_Dto_as_key_Select_Sum(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], [o].[CustomerID], [o].[EmployeeID]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override async Task GroupBy_Dto_as_element_selector_Select_Sum(bool async)
        {
            await base.GroupBy_Dto_as_element_selector_Select_Sum(async);

            AssertSql(
                @"SELECT COALESCE(SUM(CAST([o].[EmployeeID] AS bigint)), CAST(0 AS bigint)) AS [Sum], [o].[CustomerID] AS [Key]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Composite_Select_Dto_Sum_Min_Key_flattened_Max_Avg(bool async)
        {
            await base.GroupBy_Composite_Select_Dto_Sum_Min_Key_flattened_Max_Avg(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], [o].[CustomerID] AS [CustomerId], [o].[EmployeeID] AS [EmployeeId], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override async Task GroupBy_Composite_Select_Sum_Min_part_Key_flattened_Max_Avg(bool async)
        {
            await base.GroupBy_Composite_Select_Sum_Min_part_Key_flattened_Max_Avg(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], [o].[CustomerID], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        public override async Task GroupBy_Constant_Select_Sum_Min_Key_Max_Avg(bool async)
        {
            await base.GroupBy_Constant_Select_Sum_Min_Key_Max_Avg(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], 2 AS [Key], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]");
        }

        public override async Task GroupBy_Constant_with_element_selector_Select_Sum(bool async)
        {
            await base.GroupBy_Constant_with_element_selector_Select_Sum(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum]
FROM [Orders] AS [o]");
        }

        public override async Task GroupBy_Constant_with_element_selector_Select_Sum2(bool async)
        {
            await base.GroupBy_Constant_with_element_selector_Select_Sum2(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum]
FROM [Orders] AS [o]");
        }

        public override async Task GroupBy_Constant_with_element_selector_Select_Sum3(bool async)
        {
            await base.GroupBy_Constant_with_element_selector_Select_Sum3(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum]
FROM [Orders] AS [o]");
        }

        public override async Task GroupBy_after_predicate_Constant_Select_Sum_Min_Key_Max_Avg(bool async)
        {
            await base.GroupBy_after_predicate_Constant_Select_Sum_Min_Key_Max_Avg(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], 2 AS [Random], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
WHERE [o].[OrderID] > 10500");
        }

        public override async Task GroupBy_Constant_with_element_selector_Select_Sum_Min_Key_Max_Avg(bool async)
        {
            await base.GroupBy_Constant_with_element_selector_Select_Sum_Min_Key_Max_Avg(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], 2 AS [Key]
FROM [Orders] AS [o]");
        }

        public override async Task GroupBy_param_Select_Sum_Min_Key_Max_Avg(bool async)
        {
            await base.GroupBy_param_Select_Sum_Min_Key_Max_Avg(async);

            AssertSql(
                @"@__a_0='2'

SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], @__a_0 AS [Key], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]");
        }

        public override async Task GroupBy_param_with_element_selector_Select_Sum(bool async)
        {
            await base.GroupBy_param_with_element_selector_Select_Sum(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum]
FROM [Orders] AS [o]");
        }

        public override async Task GroupBy_param_with_element_selector_Select_Sum2(bool async)
        {
            await base.GroupBy_param_with_element_selector_Select_Sum2(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum]
FROM [Orders] AS [o]");
        }

        public override async Task GroupBy_param_with_element_selector_Select_Sum3(bool async)
        {
            await base.GroupBy_param_with_element_selector_Select_Sum3(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum]
FROM [Orders] AS [o]");
        }

        public override async Task GroupBy_param_with_element_selector_Select_Sum_Min_Key_Max_Avg(bool async)
        {
            await base.GroupBy_param_with_element_selector_Select_Sum_Min_Key_Max_Avg(async);

            AssertSql(
                @"@__a_0='2'

SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], @__a_0 AS [Key]
FROM [Orders] AS [o]");
        }

        public override async Task GroupBy_anonymous_key_type_mismatch_with_aggregate(bool async)
        {
            await base.GroupBy_anonymous_key_type_mismatch_with_aggregate(async);

            AssertSql(
                @"SELECT COUNT(*) AS [I0], DATEPART(year, [o].[OrderDate]) AS [I1]
FROM [Orders] AS [o]
GROUP BY DATEPART(year, [o].[OrderDate])
ORDER BY DATEPART(year, [o].[OrderDate])");
        }

        public override async Task GroupBy_Property_scalar_element_selector_Average(bool async)
        {
            await base.GroupBy_Property_scalar_element_selector_Average(async);

            AssertSql(
                @"SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_scalar_element_selector_Count(bool async)
        {
            await base.GroupBy_Property_scalar_element_selector_Count(async);

            AssertSql(
                @"SELECT COUNT([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_scalar_element_selector_LongCount(bool async)
        {
            await base.GroupBy_Property_scalar_element_selector_LongCount(async);

            AssertSql(
                @"SELECT COUNT_BIG([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_scalar_element_selector_Max(bool async)
        {
            await base.GroupBy_Property_scalar_element_selector_Max(async);

            AssertSql(
                @"SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_scalar_element_selector_Min(bool async)
        {
            await base.GroupBy_Property_scalar_element_selector_Min(async);

            AssertSql(
                @"SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_scalar_element_selector_Sum(bool async)
        {
            await base.GroupBy_Property_scalar_element_selector_Sum(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_scalar_element_selector_Sum_Min_Max_Avg(bool async)
        {
            await base.GroupBy_Property_scalar_element_selector_Sum_Min_Max_Avg(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_anonymous_element_selector_Average(bool async)
        {
            await base.GroupBy_Property_anonymous_element_selector_Average(async);

            AssertSql(
                @"SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_anonymous_element_selector_Count(bool async)
        {
            await base.GroupBy_Property_anonymous_element_selector_Count(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_anonymous_element_selector_LongCount(bool async)
        {
            await base.GroupBy_Property_anonymous_element_selector_LongCount(async);

            AssertSql(
                @"SELECT COUNT_BIG(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_anonymous_element_selector_Max(bool async)
        {
            await base.GroupBy_Property_anonymous_element_selector_Max(async);

            AssertSql(
                @"SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_anonymous_element_selector_Min(bool async)
        {
            await base.GroupBy_Property_anonymous_element_selector_Min(async);

            AssertSql(
                @"SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_anonymous_element_selector_Sum(bool async)
        {
            await base.GroupBy_Property_anonymous_element_selector_Sum(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_anonymous_element_selector_Sum_Min_Max_Avg(bool async)
        {
            await base.GroupBy_Property_anonymous_element_selector_Sum_Min_Max_Avg(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[EmployeeID]) AS [Min], MAX([o].[EmployeeID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_element_selector_complex_aggregate(bool async)
        {
            await base.GroupBy_element_selector_complex_aggregate(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID] + 1), 0)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_element_selector_complex_aggregate2(bool async)
        {
            await base.GroupBy_element_selector_complex_aggregate2(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID] + 1), 0)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_element_selector_complex_aggregate3(bool async)
        {
            await base.GroupBy_element_selector_complex_aggregate3(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID] + 1), 0)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_element_selector_complex_aggregate4(bool async)
        {
            await base.GroupBy_element_selector_complex_aggregate4(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID] + 1), 0)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task Element_selector_with_case_block_repeated_inside_another_case_block_in_projection(bool async)
        {
            await base.Element_selector_with_case_block_repeated_inside_another_case_block_in_projection(async);

            AssertSql(
                @"SELECT [o].[OrderID], COALESCE(SUM(CASE
    WHEN [o].[CustomerID] = N'ALFKI' THEN CASE
        WHEN [o].[OrderID] > 1000 THEN [o].[OrderID]
        ELSE -[o].[OrderID]
    END
    ELSE -CASE
        WHEN [o].[OrderID] > 1000 THEN [o].[OrderID]
        ELSE -[o].[OrderID]
    END
END), 0) AS [Aggregate]
FROM [Orders] AS [o]
GROUP BY [o].[OrderID]");
        }

        public override async Task GroupBy_empty_key_Aggregate(bool async)
        {
            await base.GroupBy_empty_key_Aggregate(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0)
FROM [Orders] AS [o]");
        }

        public override async Task GroupBy_empty_key_Aggregate_Key(bool async)
        {
            await base.GroupBy_empty_key_Aggregate_Key(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum]
FROM [Orders] AS [o]");
        }

        public override async Task OrderBy_GroupBy_Aggregate(bool async)
        {
            await base.OrderBy_GroupBy_Aggregate(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task OrderBy_Skip_GroupBy_Aggregate(bool async)
        {
            await base.OrderBy_Skip_GroupBy_Aggregate(async);

            AssertSql(
                @"@__p_0='80'

SELECT AVG(CAST([t].[OrderID] AS float))
FROM (
    SELECT [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS
) AS [t]
GROUP BY [t].[CustomerID]");
        }

        public override async Task OrderBy_Take_GroupBy_Aggregate(bool async)
        {
            await base.OrderBy_Take_GroupBy_Aggregate(async);

            AssertSql(
                @"@__p_0='500'

SELECT MIN([t].[OrderID])
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]
GROUP BY [t].[CustomerID]");
        }

        public override async Task OrderBy_Skip_Take_GroupBy_Aggregate(bool async)
        {
            await base.OrderBy_Skip_Take_GroupBy_Aggregate(async);

            AssertSql(
                @"@__p_0='80'
@__p_1='500'

SELECT MAX([t].[OrderID])
FROM (
    SELECT [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
GROUP BY [t].[CustomerID]");
        }

        public override async Task Distinct_GroupBy_Aggregate(bool async)
        {
            await base.Distinct_GroupBy_Aggregate(async);

            AssertSql(
                @"SELECT [t].[CustomerID] AS [Key], COUNT(*) AS [c]
FROM (
    SELECT DISTINCT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
) AS [t]
GROUP BY [t].[CustomerID]");
        }

        public override async Task Anonymous_projection_Distinct_GroupBy_Aggregate(bool async)
        {
            await base.Anonymous_projection_Distinct_GroupBy_Aggregate(async);

            AssertSql(
                @"SELECT [t].[EmployeeID] AS [Key], COUNT(*) AS [c]
FROM (
    SELECT DISTINCT [o].[OrderID], [o].[EmployeeID]
    FROM [Orders] AS [o]
) AS [t]
GROUP BY [t].[EmployeeID]");
        }

        public override async Task SelectMany_GroupBy_Aggregate(bool async)
        {
            await base.SelectMany_GroupBy_Aggregate(async);

            AssertSql(
                @"SELECT [o].[EmployeeID] AS [Key], COUNT(*) AS [c]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
GROUP BY [o].[EmployeeID]");
        }

        public override async Task Join_GroupBy_Aggregate(bool async)
        {
            await base.Join_GroupBy_Aggregate(async);

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Key], AVG(CAST([o].[OrderID] AS float)) AS [Count]
FROM [Orders] AS [o]
INNER JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [c].[CustomerID]");
        }

        public override async Task GroupBy_required_navigation_member_Aggregate(bool async)
        {
            await base.GroupBy_required_navigation_member_Aggregate(async);

            AssertSql(
                @"SELECT [o0].[CustomerID] AS [CustomerId], COUNT(*) AS [Count]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
GROUP BY [o0].[CustomerID]");
        }

        public override async Task Join_complex_GroupBy_Aggregate(bool async)
        {
            await base.Join_complex_GroupBy_Aggregate(async);

            AssertSql(
                @"@__p_0='100'
@__p_1='10'
@__p_2='50'

SELECT [t0].[CustomerID] AS [Key], AVG(CAST([t].[OrderID] AS float)) AS [Count]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10400
    ORDER BY [o].[OrderDate]
) AS [t]
INNER JOIN (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] NOT IN (N'DRACD', N'FOLKO')
    ORDER BY [c].[City]
    OFFSET @__p_1 ROWS FETCH NEXT @__p_2 ROWS ONLY
) AS [t0] ON [t].[CustomerID] = [t0].[CustomerID]
GROUP BY [t0].[CustomerID]");
        }

        public override async Task GroupJoin_GroupBy_Aggregate(bool async)
        {
            await base.GroupJoin_GroupBy_Aggregate(async);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Key], AVG(CAST([o].[OrderID] AS float)) AS [Average]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [o].[OrderID] IS NOT NULL
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupJoin_GroupBy_Aggregate_2(bool async)
        {
            await base.GroupJoin_GroupBy_Aggregate_2(async);

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Key], MAX([c].[City]) AS [Max]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
GROUP BY [c].[CustomerID]");
        }

        public override async Task GroupJoin_GroupBy_Aggregate_3(bool async)
        {
            await base.GroupJoin_GroupBy_Aggregate_3(async);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Key], AVG(CAST([o].[OrderID] AS float)) AS [Average]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupJoin_GroupBy_Aggregate_4(bool async)
        {
            await base.GroupJoin_GroupBy_Aggregate_4(async);

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Value], MAX([c].[City]) AS [Max]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
GROUP BY [c].[CustomerID]");
        }

        public override async Task GroupJoin_GroupBy_Aggregate_5(bool async)
        {
            await base.GroupJoin_GroupBy_Aggregate_5(async);

            AssertSql(
                @"SELECT [o].[OrderID] AS [Value], AVG(CAST([o].[OrderID] AS float)) AS [Average]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [o].[OrderID]");
        }

        public override async Task GroupBy_optional_navigation_member_Aggregate(bool async)
        {
            await base.GroupBy_optional_navigation_member_Aggregate(async);

            AssertSql(
                @"SELECT [c].[Country], COUNT(*) AS [Count]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [c].[Country]");
        }

        public override async Task GroupJoin_complex_GroupBy_Aggregate(bool async)
        {
            await base.GroupJoin_complex_GroupBy_Aggregate(async);

            AssertSql(
                @"@__p_0='10'
@__p_1='50'
@__p_2='100'

SELECT [t0].[CustomerID] AS [Key], AVG(CAST([t0].[OrderID] AS float)) AS [Count]
FROM (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] NOT IN (N'DRACD', N'FOLKO')
    ORDER BY [c].[City]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
INNER JOIN (
    SELECT TOP(@__p_2) [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10400
    ORDER BY [o].[OrderDate]
) AS [t0] ON [t].[CustomerID] = [t0].[CustomerID]
WHERE [t0].[OrderID] > 10300
GROUP BY [t0].[CustomerID]");
        }

        public override async Task Self_join_GroupBy_Aggregate(bool async)
        {
            await base.Self_join_GroupBy_Aggregate(async);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Key], AVG(CAST([o0].[OrderID] AS float)) AS [Count]
FROM [Orders] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE [o].[OrderID] < 10400
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_multi_navigation_members_Aggregate(bool async)
        {
            await base.GroupBy_multi_navigation_members_Aggregate(async);

            AssertSql(
                @"SELECT [o0].[CustomerID], [p].[ProductName], COUNT(*) AS [Count]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
INNER JOIN [Products] AS [p] ON [o].[ProductID] = [p].[ProductID]
GROUP BY [o0].[CustomerID], [p].[ProductName]");
        }

        public override async Task Union_simple_groupby(bool async)
        {
            await base.Union_simple_groupby(async);

            AssertSql(
                @"SELECT [t].[City] AS [Key], COUNT(*) AS [Total]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[ContactTitle] = N'Owner'
    UNION
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE [c0].[City] = N'México D.F.'
) AS [t]
GROUP BY [t].[City]");
        }

        public override async Task Select_anonymous_GroupBy_Aggregate(bool async)
        {
            await base.Select_anonymous_GroupBy_Aggregate(async);

            AssertSql(
                @"SELECT MIN([o].[OrderDate]) AS [Min], MAX([o].[OrderDate]) AS [Max], COALESCE(SUM([o].[OrderID]), 0) AS [Sum], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_principal_key_property_optimization(bool async)
        {
            await base.GroupBy_principal_key_property_optimization(async);

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Key], COUNT(*) AS [Count]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [c].[CustomerID]");
        }

        public override async Task GroupBy_after_anonymous_projection_and_distinct_followed_by_another_anonymous_projection(bool async)
        {
            await base.GroupBy_after_anonymous_projection_and_distinct_followed_by_another_anonymous_projection(async);

            AssertSql(
                @"SELECT [t].[CustomerID] AS [Key], COUNT(*) AS [Count]
FROM (
    SELECT DISTINCT [o].[CustomerID], [o].[OrderID]
    FROM [Orders] AS [o]
) AS [t]
GROUP BY [t].[CustomerID]");
        }

        public override async Task GroupBy_OrderBy_key(bool async)
        {
            await base.GroupBy_OrderBy_key(async);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Key], COUNT(*) AS [c]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
ORDER BY [o].[CustomerID]");
        }

        public override async Task GroupBy_OrderBy_count(bool async)
        {
            await base.GroupBy_OrderBy_count(async);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Key], COUNT(*) AS [Count]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
ORDER BY COUNT(*), [o].[CustomerID]");
        }

        public override async Task GroupBy_OrderBy_count_Select_sum(bool async)
        {
            await base.GroupBy_OrderBy_count_Select_sum(async);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Key], COALESCE(SUM([o].[OrderID]), 0) AS [Sum]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
ORDER BY COUNT(*), [o].[CustomerID]");
        }

        public override async Task GroupBy_aggregate_Contains(bool async)
        {
            await base.GroupBy_aggregate_Contains(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o0]
    GROUP BY [o0].[CustomerID]
    HAVING (COUNT(*) > 30) AND (([o0].[CustomerID] = [o].[CustomerID]) OR ([o0].[CustomerID] IS NULL AND [o].[CustomerID] IS NULL)))");
        }

        public override async Task GroupBy_aggregate_Pushdown(bool async)
        {
            await base.GroupBy_aggregate_Pushdown(async);

            AssertSql(
                @"@__p_0='20'
@__p_1='4'

SELECT [t].[CustomerID]
FROM (
    SELECT TOP(@__p_0) [o].[CustomerID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 10
    ORDER BY [o].[CustomerID]
) AS [t]
ORDER BY [t].[CustomerID]
OFFSET @__p_1 ROWS");
        }

        public override async Task GroupBy_aggregate_Pushdown_followed_by_projecting_Length(bool async)
        {
            await base.GroupBy_aggregate_Pushdown_followed_by_projecting_Length(async);

            AssertSql(
                @"@__p_0='20'
@__p_1='4'

SELECT CAST(LEN([t].[CustomerID]) AS int)
FROM (
    SELECT TOP(@__p_0) [o].[CustomerID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 10
    ORDER BY [o].[CustomerID]
) AS [t]
ORDER BY [t].[CustomerID]
OFFSET @__p_1 ROWS");
        }

        public override async Task GroupBy_aggregate_Pushdown_followed_by_projecting_constant(bool async)
        {
            await base.GroupBy_aggregate_Pushdown_followed_by_projecting_constant(async);

            AssertSql(
                @"@__p_0='20'
@__p_1='4'

SELECT 5
FROM (
    SELECT TOP(@__p_0) [o].[CustomerID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 10
    ORDER BY [o].[CustomerID]
) AS [t]
ORDER BY [t].[CustomerID]
OFFSET @__p_1 ROWS");
        }

        public override async Task GroupBy_filter_key(bool async)
        {
            await base.GroupBy_filter_key(async);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Key], COUNT(*) AS [c]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
HAVING [o].[CustomerID] = N'ALFKI'");
        }

        public override async Task GroupBy_filter_count(bool async)
        {
            await base.GroupBy_filter_count(async);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Key], COUNT(*) AS [Count]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
HAVING COUNT(*) > 4");
        }

        public override async Task GroupBy_filter_count_OrderBy_count_Select_sum(bool async)
        {
            await base.GroupBy_filter_count_OrderBy_count_Select_sum(async);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Key], COUNT(*) AS [Count], COALESCE(SUM([o].[OrderID]), 0) AS [Sum]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
HAVING COUNT(*) > 4
ORDER BY COUNT(*), [o].[CustomerID]");
        }

        public override async Task GroupBy_Aggregate_Join(bool async)
        {
            await base.GroupBy_Aggregate_Join(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM (
    SELECT [o].[CustomerID], MAX([o].[OrderID]) AS [c]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 5
) AS [t]
INNER JOIN [Customers] AS [c] ON [t].[CustomerID] = [c].[CustomerID]
INNER JOIN [Orders] AS [o0] ON [t].[c] = [o0].[OrderID]");
        }

        public override async Task Join_GroupBy_Aggregate_multijoins(bool async)
        {
            await base.Join_GroupBy_Aggregate_multijoins(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [o].[CustomerID], MAX([o].[OrderID]) AS [c]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 5
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]
INNER JOIN [Orders] AS [o0] ON [t].[c] = [o0].[OrderID]");
        }

        public override async Task Join_GroupBy_Aggregate_single_join(bool async)
        {
            await base.Join_GroupBy_Aggregate_single_join(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[c] AS [LastOrderID]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [o].[CustomerID], MAX([o].[OrderID]) AS [c]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 5
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]");
        }

        public override async Task Join_GroupBy_Aggregate_with_another_join(bool async)
        {
            await base.Join_GroupBy_Aggregate_with_another_join(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[c] AS [LastOrderID], [o0].[OrderID]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [o].[CustomerID], MAX([o].[OrderID]) AS [c]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 5
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]
INNER JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]");
        }

        public override async Task Join_GroupBy_Aggregate_with_left_join(bool async)
        {
            await base.Join_GroupBy_Aggregate_with_left_join(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[c] AS [LastOrderID]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[CustomerID], MAX([o].[OrderID]) AS [c]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 5
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]
WHERE [c].[CustomerID] LIKE N'A%'");
        }

        public override async Task Join_GroupBy_Aggregate_in_subquery(bool async)
        {
            await base.Join_GroupBy_Aggregate_in_subquery(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    INNER JOIN (
        SELECT [o0].[CustomerID]
        FROM [Orders] AS [o0]
        GROUP BY [o0].[CustomerID]
        HAVING COUNT(*) > 5
    ) AS [t] ON [c].[CustomerID] = [t].[CustomerID]
) AS [t0] ON [o].[CustomerID] = [t0].[CustomerID]
WHERE [o].[OrderID] < 10400");
        }

        public override async Task Join_GroupBy_Aggregate_on_key(bool async)
        {
            await base.Join_GroupBy_Aggregate_on_key(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[c] AS [LastOrderID]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [o].[CustomerID], MAX([o].[OrderID]) AS [c]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 5
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]");
        }

        public override async Task GroupBy_with_result_selector(bool async)
        {
            await base.GroupBy_with_result_selector(async);

            AssertSql(
                @"SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Sum_constant(bool async)
        {
            await base.GroupBy_Sum_constant(async);

            AssertSql(
                @"SELECT COALESCE(SUM(1), 0)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Sum_constant_cast(bool async)
        {
            await base.GroupBy_Sum_constant_cast(async);

            AssertSql(
                @"SELECT COALESCE(SUM(CAST(1 AS bigint)), CAST(0 AS bigint))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task Distinct_GroupBy_OrderBy_key(bool async)
        {
            await base.Distinct_GroupBy_OrderBy_key(async);

            AssertSql(
                @"SELECT [t].[CustomerID] AS [Key], COUNT(*) AS [c]
FROM (
    SELECT DISTINCT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
) AS [t]
GROUP BY [t].[CustomerID]
ORDER BY [t].[CustomerID]");
        }

        public override async Task Select_nested_collection_with_groupby(bool async)
        {
            await base.Select_nested_collection_with_groupby(async);

            AssertSql(
                @"SELECT (
    SELECT CASE
        WHEN EXISTS (
            SELECT 1
            FROM [Orders] AS [o0]
            WHERE [c].[CustomerID] = [o0].[CustomerID])
        THEN CAST(1 AS bit) ELSE CAST(0 AS bit)
    END
), [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'",
                //
                @"@_outer_CustomerID='ALFKI' (Size = 5)

SELECT [o1].[OrderID]
FROM [Orders] AS [o1]
WHERE @_outer_CustomerID = [o1].[CustomerID]
ORDER BY [o1].[OrderID]",
                //
                @"@_outer_CustomerID='ANATR' (Size = 5)

SELECT [o1].[OrderID]
FROM [Orders] AS [o1]
WHERE @_outer_CustomerID = [o1].[CustomerID]
ORDER BY [o1].[OrderID]",
                //
                @"@_outer_CustomerID='ANTON' (Size = 5)

SELECT [o1].[OrderID]
FROM [Orders] AS [o1]
WHERE @_outer_CustomerID = [o1].[CustomerID]
ORDER BY [o1].[OrderID]",
                //
                @"@_outer_CustomerID='AROUT' (Size = 5)

SELECT [o1].[OrderID]
FROM [Orders] AS [o1]
WHERE @_outer_CustomerID = [o1].[CustomerID]
ORDER BY [o1].[OrderID]");
        }

        public override async Task Select_uncorrelated_collection_with_groupby_works(bool async)
        {
            await base.Select_uncorrelated_collection_with_groupby_works(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [t].[OrderID]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    GROUP BY [o].[OrderID]
) AS [t]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID], [t].[OrderID]");
        }

        public override async Task Select_uncorrelated_collection_with_groupby_multiple_collections_work(bool async)
        {
            await base.Select_uncorrelated_collection_with_groupby_multiple_collections_work(async);

            AssertSql(
                @"SELECT [o].[OrderID], [t].[ProductID], [t0].[c], [t0].[ProductID]
FROM [Orders] AS [o]
OUTER APPLY (
    SELECT [p].[ProductID]
    FROM [Products] AS [p]
    GROUP BY [p].[ProductID]
) AS [t]
OUTER APPLY (
    SELECT COUNT(*) AS [c], [p0].[ProductID]
    FROM [Products] AS [p0]
    GROUP BY [p0].[ProductID]
) AS [t0]
WHERE [o].[CustomerID] IS NOT NULL AND ([o].[CustomerID] LIKE N'A%')
ORDER BY [o].[OrderID], [t].[ProductID], [t0].[ProductID]");
        }

        public override async Task Select_GroupBy_All(bool async)
        {
            await base.Select_GroupBy_All(async);

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        GROUP BY [o].[CustomerID]
        HAVING ([o].[CustomerID] <> N'ALFKI') OR [o].[CustomerID] IS NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task GroupBy_Where_Average(bool async)
        {
            await base.GroupBy_Where_Average(async);

            AssertSql(
                @"SELECT AVG(CAST(CASE
    WHEN [o].[OrderID] < 10300 THEN [o].[OrderID]
END AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Where_Count(bool async)
        {
            await base.GroupBy_Where_Count(async);

            AssertSql(
                @"SELECT COUNT(CASE
    WHEN [o].[OrderID] < 10300 THEN 1
END)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Where_LongCount(bool async)
        {
            await base.GroupBy_Where_LongCount(async);

            AssertSql(
                @"SELECT COUNT_BIG(CASE
    WHEN [o].[OrderID] < 10300 THEN 1
END)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Where_Max(bool async)
        {
            await base.GroupBy_Where_Max(async);

            AssertSql(
                @"SELECT MAX(CASE
    WHEN [o].[OrderID] < 10300 THEN [o].[OrderID]
END)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Where_Min(bool async)
        {
            await base.GroupBy_Where_Min(async);

            AssertSql(
                @"SELECT MIN(CASE
    WHEN [o].[OrderID] < 10300 THEN [o].[OrderID]
END)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Where_Sum(bool async)
        {
            await base.GroupBy_Where_Sum(async);

            AssertSql(
                @"SELECT COALESCE(SUM(CASE
    WHEN [o].[OrderID] < 10300 THEN [o].[OrderID]
END), 0)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Where_Count_with_predicate(bool async)
        {
            await base.GroupBy_Where_Count_with_predicate(async);

            AssertSql(
                @"SELECT COUNT(CASE
    WHEN ([o].[OrderID] < 10300) AND ([o].[OrderDate] IS NOT NULL AND (DATEPART(year, [o].[OrderDate]) = 1997)) THEN 1
END)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Where_Where_Count(bool async)
        {
            await base.GroupBy_Where_Where_Count(async);

            AssertSql(
                @"SELECT COUNT(CASE
    WHEN ([o].[OrderID] < 10300) AND ([o].[OrderDate] IS NOT NULL AND (DATEPART(year, [o].[OrderDate]) = 1997)) THEN 1
END)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Where_Select_Where_Count(bool async)
        {
            await base.GroupBy_Where_Select_Where_Count(async);

            AssertSql(
                @"SELECT COUNT(CASE
    WHEN ([o].[OrderID] < 10300) AND ([o].[OrderDate] IS NOT NULL AND (DATEPART(year, [o].[OrderDate]) = 1997)) THEN [o].[OrderDate]
END)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Where_Select_Where_Select_Min(bool async)
        {
            await base.GroupBy_Where_Select_Where_Select_Min(async);

            AssertSql(
                @"SELECT MIN(CASE
    WHEN ([o].[OrderID] < 10300) AND ([o].[OrderDate] IS NOT NULL AND (DATEPART(year, [o].[OrderDate]) = 1997)) THEN [o].[OrderID]
END)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_multiple_Count_with_predicate(bool async)
        {
            await base.GroupBy_multiple_Count_with_predicate(async);

            AssertSql(
                @"SELECT [o].[CustomerID], COUNT(*) AS [All], COUNT(CASE
    WHEN [o].[OrderID] < 11000 THEN 1
END) AS [TenK], COUNT(CASE
    WHEN [o].[OrderID] < 12000 THEN 1
END) AS [EleventK]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_multiple_Sum_with_conditional_projection(bool async)
        {
            await base.GroupBy_multiple_Sum_with_conditional_projection(async);

            AssertSql(
                @"SELECT [o].[CustomerID], COALESCE(SUM(CASE
    WHEN [o].[OrderID] < 11000 THEN [o].[OrderID]
    ELSE 0
END), 0) AS [TenK], COALESCE(SUM(CASE
    WHEN [o].[OrderID] >= 11000 THEN [o].[OrderID]
    ELSE 0
END), 0) AS [EleventK]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_multiple_Sum_with_Select_conditional_projection(bool async)
        {
            await base.GroupBy_multiple_Sum_with_Select_conditional_projection(async);

            AssertSql(
                @"SELECT [o].[CustomerID], COALESCE(SUM(CASE
    WHEN [o].[OrderID] < 11000 THEN [o].[OrderID]
    ELSE 0
END), 0) AS [TenK], COALESCE(SUM(CASE
    WHEN [o].[OrderID] >= 11000 THEN [o].[OrderID]
    ELSE 0
END), 0) AS [EleventK]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Key_as_part_of_element_selector(bool async)
        {
            await base.GroupBy_Key_as_part_of_element_selector(async);

            AssertSql(
                @"SELECT [o].[OrderID] AS [Key], AVG(CAST([o].[OrderID] AS float)) AS [Avg], MAX([o].[OrderDate]) AS [Max]
FROM [Orders] AS [o]
GROUP BY [o].[OrderID]");
        }

        public override async Task GroupBy_composite_Key_as_part_of_element_selector(bool async)
        {
            await base.GroupBy_composite_Key_as_part_of_element_selector(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], AVG(CAST([o].[OrderID] AS float)) AS [Avg], MAX([o].[OrderDate]) AS [Max]
FROM [Orders] AS [o]
GROUP BY [o].[OrderID], [o].[CustomerID]");
        }

        public override async Task GroupBy_SelectMany(bool async)
        {
            await base.GroupBy_SelectMany(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[City]");
        }

        public override async Task OrderBy_GroupBy_SelectMany(bool async)
        {
            await base.OrderBy_GroupBy_SelectMany(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
ORDER BY [o].[CustomerID], [o].[OrderID]");
        }

        public override async Task OrderBy_GroupBy_SelectMany_shadow(bool async)
        {
            await base.OrderBy_GroupBy_SelectMany_shadow(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
ORDER BY [e].[EmployeeID]");
        }

        public override async Task GroupBy_with_orderby_take_skip_distinct_followed_by_group_key_projection(bool async)
        {
            await base.GroupBy_with_orderby_take_skip_distinct_followed_by_group_key_projection(async);

            AssertSql(
                "");
        }

        public override async Task GroupBy_Distinct(bool async)
        {
            await base.GroupBy_Distinct(async);

            AssertSql(
                @"SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o0]
ORDER BY [o0].[CustomerID]");
        }

        public override async Task GroupBy_with_aggregate_through_navigation_property(bool async)
        {
            await base.GroupBy_with_aggregate_through_navigation_property(async);

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

        public override async Task GroupBy_Shadow(bool async)
        {
            await base.GroupBy_Shadow(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE ([e].[Title] = N'Sales Representative') AND ([e].[EmployeeID] = 1)
ORDER BY [e].[Title]");
        }

        public override async Task GroupBy_Shadow2(bool async)
        {
            await base.GroupBy_Shadow2(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE ([e].[Title] = N'Sales Representative') AND ([e].[EmployeeID] = 1)
ORDER BY [e].[Title]");
        }

        public override async Task GroupBy_Shadow3(bool async)
        {
            await base.GroupBy_Shadow3(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = 1
ORDER BY [e].[EmployeeID]");
        }

        public override async Task Select_GroupBy_SelectMany(bool async)
        {
            await base.Select_GroupBy_SelectMany(async);

            AssertSql(
                @"SELECT [o].[OrderID] AS [Order], [o].[CustomerID] AS [Customer]
FROM [Orders] AS [o]
ORDER BY [o].[OrderID]");
        }

        public override async Task Count_after_GroupBy_aggregate(bool async)
        {
            await base.Count_after_GroupBy_aggregate(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [t]");
        }

        public override async Task LongCount_after_GroupBy_aggregate(bool async)
        {
            await base.LongCount_after_GroupBy_aggregate(async);

            AssertSql(
                @"SELECT COUNT_BIG(*)
FROM (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [t]");
        }

        public override async Task GroupBy_Select_Distinct_aggregate(bool async)
        {
            await base.GroupBy_Select_Distinct_aggregate(async);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Key], AVG(DISTINCT (CAST([o].[OrderID] AS float))) AS [Average], COUNT(DISTINCT ([o].[EmployeeID])) AS [Count], COUNT_BIG(DISTINCT ([o].[EmployeeID])) AS [LongCount], MAX(DISTINCT ([o].[OrderDate])) AS [Max], MIN(DISTINCT ([o].[OrderDate])) AS [Min], COALESCE(SUM(DISTINCT ([o].[OrderID])), 0) AS [Sum]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_group_Distinct_Select_Distinct_aggregate(bool async)
        {
            await base.GroupBy_group_Distinct_Select_Distinct_aggregate(async);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Key], MAX(DISTINCT ([o].[OrderDate])) AS [Max]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_group_Where_Select_Distinct_aggregate(bool async)
        {
            await base.GroupBy_group_Where_Select_Distinct_aggregate(async);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Key], MAX(DISTINCT (CASE
    WHEN [o].[OrderDate] IS NOT NULL THEN [o].[OrderDate]
END)) AS [Max]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task MinMax_after_GroupBy_aggregate(bool async)
        {
            await base.MinMax_after_GroupBy_aggregate(async);

            AssertSql(
                @"SELECT MIN([t].[c])
FROM (
    SELECT COALESCE(SUM([o].[OrderID]), 0) AS [c]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [t]",
                //
                @"SELECT MAX([t].[c])
FROM (
    SELECT COALESCE(SUM([o].[OrderID]), 0) AS [c]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [t]");
        }

        public override async Task All_after_GroupBy_aggregate(bool async)
        {
            await base.All_after_GroupBy_aggregate(async);

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        GROUP BY [o].[CustomerID]
        HAVING 0 = 1) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task All_after_GroupBy_aggregate2(bool async)
        {
            await base.All_after_GroupBy_aggregate2(async);

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        GROUP BY [o].[CustomerID]
        HAVING NOT (COALESCE(SUM([o].[OrderID]), 0) >= 0)) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Any_after_GroupBy_aggregate(bool async)
        {
            await base.Any_after_GroupBy_aggregate(async);

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        GROUP BY [o].[CustomerID]) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Count_after_GroupBy_without_aggregate(bool async)
        {
            await base.Count_after_GroupBy_without_aggregate(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [t]");
        }

        public override async Task Count_with_predicate_after_GroupBy_without_aggregate(bool async)
        {
            await base.Count_with_predicate_after_GroupBy_without_aggregate(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 1
) AS [t]");
        }

        public override async Task LongCount_after_GroupBy_without_aggregate(bool async)
        {
            await base.LongCount_after_GroupBy_without_aggregate(async);

            AssertSql(
                @"SELECT COUNT_BIG(*)
FROM (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [t]");
        }

        public override async Task LongCount_with_predicate_after_GroupBy_without_aggregate(bool async)
        {
            await base.LongCount_with_predicate_after_GroupBy_without_aggregate(async);

            AssertSql(
                @"SELECT COUNT_BIG(*)
FROM (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 1
) AS [t]");
        }

        public override async Task Any_after_GroupBy_without_aggregate(bool async)
        {
            await base.Any_after_GroupBy_without_aggregate(async);

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        GROUP BY [o].[CustomerID]) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Any_with_predicate_after_GroupBy_without_aggregate(bool async)
        {
            await base.Any_with_predicate_after_GroupBy_without_aggregate(async);

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        GROUP BY [o].[CustomerID]
        HAVING COUNT(*) > 1) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task All_with_predicate_after_GroupBy_without_aggregate(bool async)
        {
            await base.All_with_predicate_after_GroupBy_without_aggregate(async);

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        GROUP BY [o].[CustomerID]
        HAVING COUNT(*) <= 1) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task GroupBy_based_on_renamed_property_simple(bool async)
        {
            await base.GroupBy_based_on_renamed_property_simple(async);

            AssertSql(
                @"SELECT [c].[City] AS [Renamed], COUNT(*) AS [Count]
FROM [Customers] AS [c]
GROUP BY [c].[City]");
        }

        public override async Task GroupBy_based_on_renamed_property_complex(bool async)
        {
            await base.GroupBy_based_on_renamed_property_complex(async);

            AssertSql(
                @"SELECT [t].[City] AS [Key], COUNT(*) AS [Count]
FROM (
    SELECT DISTINCT [c].[City], [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [t]
GROUP BY [t].[City]");
        }

        public override async Task Join_groupby_anonymous_orderby_anonymous_projection(bool async)
        {
            await base.Join_groupby_anonymous_orderby_anonymous_projection(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [o].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
GROUP BY [c].[CustomerID], [o].[OrderDate]
ORDER BY [o].[OrderDate]");
        }

        public override async Task GroupBy_with_group_key_access_thru_navigation(bool async)
        {
            await base.GroupBy_with_group_key_access_thru_navigation(async);

            AssertSql(
                @"SELECT [o0].[CustomerID] AS [Key], COALESCE(SUM([o].[OrderID]), 0) AS [Aggregate]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
GROUP BY [o0].[CustomerID]");
        }

        public override async Task GroupBy_with_group_key_access_thru_nested_navigation(bool async)
        {
            await base.GroupBy_with_group_key_access_thru_nested_navigation(async);

            AssertSql(
                @"SELECT [c].[Country] AS [Key], COALESCE(SUM([o].[OrderID]), 0) AS [Aggregate]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
GROUP BY [c].[Country]");
        }

        public override async Task GroupBy_with_group_key_being_navigation(bool async)
        {
            await base.GroupBy_with_group_key_being_navigation(async);

            AssertSql(
                @"");
        }

        public override async Task GroupBy_with_group_key_being_nested_navigation(bool async)
        {
            await base.GroupBy_with_group_key_being_nested_navigation(async);

            AssertSql(
                @"");
        }

        public override async Task GroupBy_with_group_key_being_navigation_with_entity_key_projection(bool async)
        {
            await base.GroupBy_with_group_key_being_navigation_with_entity_key_projection(async);

            AssertSql(
                @"");
        }

        public override async Task GroupBy_with_group_key_being_navigation_with_complex_projection(bool async)
        {
            await base.GroupBy_with_group_key_being_navigation_with_complex_projection(async);

            AssertSql(
                @"");
        }

        public override async Task GroupBy_with_order_by_skip_and_another_order_by(bool async)
        {
            await base.GroupBy_with_order_by_skip_and_another_order_by(async);

            AssertSql(
                @"@__p_0='80'

SELECT COALESCE(SUM([t].[OrderID]), 0)
FROM (
    SELECT [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    ORDER BY [o].[CustomerID], [o].[OrderID]
    OFFSET @__p_0 ROWS
) AS [t]
GROUP BY [t].[CustomerID]");
        }

        public override async Task GroupBy_Property_Select_Count_with_predicate(bool async)
        {
            await base.GroupBy_Property_Select_Count_with_predicate(async);

            AssertSql(
                @"SELECT COUNT(CASE
    WHEN [o].[OrderID] < 10300 THEN 1
END)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_Property_Select_LongCount_with_predicate(bool async)
        {
            await base.GroupBy_Property_Select_LongCount_with_predicate(async);

            AssertSql(
                @"SELECT COUNT_BIG(CASE
    WHEN [o].[OrderID] < 10300 THEN 1
END)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_orderby_projection_with_coalesce_operation(bool async)
        {
            await base.GroupBy_orderby_projection_with_coalesce_operation(async);

            AssertSql(
                @"SELECT COALESCE([c].[City], N'Unknown') AS [Locality], COUNT(*) AS [Count]
FROM [Customers] AS [c]
GROUP BY [c].[City]
ORDER BY COUNT(*) DESC, [c].[City]");
        }

        public override async Task GroupBy_let_orderby_projection_with_coalesce_operation(bool async)
        {
            await base.GroupBy_let_orderby_projection_with_coalesce_operation(async);

            AssertSql(" ");
        }

        public override async Task GroupBy_Min_Where_optional_relationship(bool async)
        {
            await base.GroupBy_Min_Where_optional_relationship(async);

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Key], COUNT(*) AS [Count]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [c].[CustomerID]
HAVING COUNT(*) <> 2");
        }

        public override async Task GroupBy_Min_Where_optional_relationship_2(bool async)
        {
            await base.GroupBy_Min_Where_optional_relationship_2(async);

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Key], COUNT(*) AS [Count]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [c].[CustomerID]
HAVING (COUNT(*) < 2) OR (COUNT(*) > 2)");
        }

        public override async Task GroupBy_aggregate_over_a_subquery(bool async)
        {
            await base.GroupBy_aggregate_over_a_subquery(async);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Key], (
    SELECT COUNT(*)
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = [o].[CustomerID]) AS [Count]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task GroupBy_aggregate_join_with_grouping_key(bool async)
        {
            await base.GroupBy_aggregate_join_with_grouping_key(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[c] AS [Count]
FROM (
    SELECT [o].[CustomerID], COUNT(*) AS [c]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [t]
INNER JOIN [Customers] AS [c] ON [t].[CustomerID] = [c].[CustomerID]");
        }

        public override async Task GroupBy_aggregate_join_with_group_result(bool async)
        {
            await base.GroupBy_aggregate_join_with_group_result(async);

            AssertSql(
                @"SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM (
    SELECT [o].[CustomerID], MAX([o].[OrderDate]) AS [c]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [t]
INNER JOIN [Orders] AS [o0] ON (([t].[CustomerID] = [o0].[CustomerID]) OR ([t].[CustomerID] IS NULL AND [o0].[CustomerID] IS NULL)) AND (([t].[c] = [o0].[OrderDate]) OR ([t].[c] IS NULL AND [o0].[OrderDate] IS NULL))");
        }

        public override async Task GroupBy_aggregate_from_right_side_of_join(bool async)
        {
            await base.GroupBy_aggregate_from_right_side_of_join(async);

            AssertSql(
                @"@__p_0='10'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[c] AS [Max]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [o].[CustomerID], MAX([o].[OrderDate]) AS [c]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[c], [c].[CustomerID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_0 ROWS ONLY");
        }

        public override async Task GroupBy_aggregate_join_another_GroupBy_aggregate(bool async)
        {
            await base.GroupBy_aggregate_join_another_GroupBy_aggregate(async);

            AssertSql(
                @"SELECT [t].[CustomerID] AS [Key], [t].[c] AS [Total], [t0].[c] AS [ThatYear]
FROM (
    SELECT [o].[CustomerID], COUNT(*) AS [c]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [t]
INNER JOIN (
    SELECT [o0].[CustomerID], COUNT(*) AS [c]
    FROM [Orders] AS [o0]
    WHERE DATEPART(year, [o0].[OrderDate]) = 1997
    GROUP BY [o0].[CustomerID]
) AS [t0] ON [t].[CustomerID] = [t0].[CustomerID]");
        }

        public override async Task GroupBy_with_grouping_key_using_Like(bool async)
        {
            await base.GroupBy_with_grouping_key_using_Like(async);

            AssertSql(
                @"SELECT CASE
    WHEN [o].[CustomerID] LIKE N'A%' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Key], COUNT(*) AS [Count]
FROM [Orders] AS [o]
GROUP BY CASE
    WHEN [o].[CustomerID] LIKE N'A%' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task GroupBy_with_grouping_key_DateTime_Day(bool async)
        {
            await base.GroupBy_with_grouping_key_DateTime_Day(async);

            AssertSql(
                @"SELECT DATEPART(day, [o].[OrderDate]) AS [Key], COUNT(*) AS [Count]
FROM [Orders] AS [o]
GROUP BY DATEPART(day, [o].[OrderDate])");
        }

        public override async Task GroupBy_with_cast_inside_grouping_aggregate(bool async)
        {
            await base.GroupBy_with_cast_inside_grouping_aggregate(async);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Key], COUNT(*) AS [Count], COALESCE(SUM(CAST([o].[OrderID] AS bigint)), CAST(0 AS bigint)) AS [Sum]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        public override async Task Complex_query_with_groupBy_in_subquery1(bool async)
        {
            await base.Complex_query_with_groupBy_in_subquery1(async);

            AssertSql(
                @"");
        }

        public override async Task Complex_query_with_groupBy_in_subquery2(bool async)
        {
            await base.Complex_query_with_groupBy_in_subquery2(async);

            AssertSql(
                @"");
        }

        public override async Task Complex_query_with_groupBy_in_subquery3(bool async)
        {
            await base.Complex_query_with_groupBy_in_subquery3(async);

            AssertSql(
                @"");
        }

        public override async Task Group_by_with_projection_into_DTO(bool async)
        {
            await base.Group_by_with_projection_into_DTO(async);

            AssertSql(
                @"SELECT CAST([o].[OrderID] AS bigint) AS [Id], COUNT(*) AS [Count]
FROM [Orders] AS [o]
GROUP BY [o].[OrderID]");
        }

        public override async Task Where_select_function_groupby_followed_by_another_select_with_aggregates(bool async)
        {
            await base.Where_select_function_groupby_followed_by_another_select_with_aggregates(async);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Key], COALESCE(SUM(CASE
    WHEN (2020 - DATEPART(year, [o].[OrderDate])) <= 30 THEN [o].[OrderID]
    ELSE 0
END), 0) AS [Sum1], COALESCE(SUM(CASE
    WHEN ((2020 - DATEPART(year, [o].[OrderDate])) > 30) AND ((2020 - DATEPART(year, [o].[OrderDate])) <= 60) THEN [o].[OrderID]
    ELSE 0
END), 0) AS [Sum2]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] IS NOT NULL AND ([o].[CustomerID] LIKE N'A%')
GROUP BY [o].[CustomerID]");
        }

        public override async Task Group_by_column_project_constant(bool async)
        {
            await base.Group_by_column_project_constant(async);

            AssertSql(
                @"SELECT 42
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
ORDER BY [o].[CustomerID]");
        }

        public override async Task Key_plus_key_in_projection(bool async)
        {
            await base.Key_plus_key_in_projection(async);

            AssertSql(
                @"SELECT [o].[OrderID] + [o].[OrderID] AS [Value], AVG(CAST([o].[OrderID] AS float)) AS [Average]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [o].[OrderID]");
        }

        public override async Task Group_by_with_arithmetic_operation_inside_aggregate(bool async)
        {
            await base.Group_by_with_arithmetic_operation_inside_aggregate(async);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Key], COALESCE(SUM([o].[OrderID] + CAST(LEN([o].[CustomerID]) AS int)), 0) AS [Sum]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        [ConditionalTheory(Skip = "Issue#19027")]
        public override async Task GroupBy_scalar_subquery(bool async)
        {
            await base.GroupBy_scalar_subquery(async);

            AssertSql(" ");
        }

        public override async Task GroupBy_scalar_aggregate_in_set_operation(bool async)
        {
            await base.GroupBy_scalar_aggregate_in_set_operation(async);

            AssertSql(
                @"SELECT [c].[CustomerID], 0 AS [Sequence]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
UNION
SELECT [o].[CustomerID], 1 AS [Sequence]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
