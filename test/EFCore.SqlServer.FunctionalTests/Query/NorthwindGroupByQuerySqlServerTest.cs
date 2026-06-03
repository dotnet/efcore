// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindGroupByQuerySqlServerTest : NorthwindGroupByQueryRelationalTestBase<
    NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
{
    public NorthwindGroupByQuerySqlServerTest(
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

    public override async Task GroupBy_Property_Select_Average(bool async)
    {
        await base.GroupBy_Property_Select_Average(async);

        AssertSql(
            """
SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");

        // Validating that we don't generate warning when translating GroupBy. See Issue#11157
        Assert.DoesNotContain(
            "The LINQ expression 'GroupBy([o].CustomerID, [o])' could not be translated and will be evaluated locally.",
            Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
    }

    public override async Task GroupBy_Property_Select_Average_with_group_enumerable_projected(bool async)
    {
        await base.GroupBy_Property_Select_Average_with_group_enumerable_projected(async);

        AssertSql();
    }

    public override async Task GroupBy_Property_Select_Count(bool async)
    {
        await base.GroupBy_Property_Select_Count(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_Select_LongCount(bool async)
    {
        await base.GroupBy_Property_Select_LongCount(async);

        AssertSql(
            """
SELECT COUNT_BIG(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_Select_Count_with_nulls(bool async)
    {
        await base.GroupBy_Property_Select_Count_with_nulls(async);

        AssertSql(
            """
SELECT [c].[City], COUNT(*) AS [Faxes]
FROM [Customers] AS [c]
GROUP BY [c].[City]
""");
    }

    public override async Task GroupBy_Property_Select_LongCount_with_nulls(bool async)
    {
        await base.GroupBy_Property_Select_LongCount_with_nulls(async);

        AssertSql(
            """
SELECT [c].[City], COUNT_BIG(*) AS [Faxes]
FROM [Customers] AS [c]
GROUP BY [c].[City]
""");
    }

    public override async Task GroupBy_Property_Select_Max(bool async)
    {
        await base.GroupBy_Property_Select_Max(async);

        AssertSql(
            """
SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_Select_Min(bool async)
    {
        await base.GroupBy_Property_Select_Min(async);

        AssertSql(
            """
SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_Select_Sum(bool async)
    {
        await base.GroupBy_Property_Select_Sum(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID]), 0)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_Select_Sum_Min_Max_Avg(bool async)
    {
        await base.GroupBy_Property_Select_Sum_Min_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_Select_Key_Average(bool async)
    {
        await base.GroupBy_Property_Select_Key_Average(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], AVG(CAST([o].[OrderID] AS float)) AS [Average]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_Select_Key_Count(bool async)
    {
        await base.GroupBy_Property_Select_Key_Count(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], COUNT(*) AS [Count]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_Select_Key_LongCount(bool async)
    {
        await base.GroupBy_Property_Select_Key_LongCount(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], COUNT_BIG(*) AS [LongCount]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_Select_Key_Max(bool async)
    {
        await base.GroupBy_Property_Select_Key_Max(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], MAX([o].[OrderID]) AS [Max]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_Select_Key_Min(bool async)
    {
        await base.GroupBy_Property_Select_Key_Min(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], MIN([o].[OrderID]) AS [Min]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_Select_Key_Sum(bool async)
    {
        await base.GroupBy_Property_Select_Key_Sum(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], COALESCE(SUM([o].[OrderID]), 0) AS [Sum]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_Select_Key_Sum_Min_Max_Avg(bool async)
    {
        await base.GroupBy_Property_Select_Key_Sum_Min_Max_Avg(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_Select_Sum_Min_Key_Max_Avg(bool async)
    {
        await base.GroupBy_Property_Select_Sum_Min_Key_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], [o].[CustomerID] AS [Key], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_Select_key_multiple_times_and_aggregate(bool async)
    {
        await base.GroupBy_Property_Select_key_multiple_times_and_aggregate(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key1], COALESCE(SUM([o].[OrderID]), 0) AS [Sum]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_Select_Key_with_constant(bool async)
    {
        await base.GroupBy_Property_Select_Key_with_constant(async);

        AssertSql(
            """
SELECT [o0].[Name], [o0].[CustomerID] AS [Value], COUNT(*) AS [Count]
FROM (
    SELECT [o].[CustomerID], N'CustomerID' AS [Name]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[Name], [o0].[CustomerID]
""");
    }

    public override async Task GroupBy_aggregate_projecting_conditional_expression(bool async)
    {
        await base.GroupBy_aggregate_projecting_conditional_expression(async);

        AssertSql(
            """
SELECT [o].[OrderDate] AS [Key], CASE
    WHEN COUNT(*) = 0 THEN 1
    ELSE COALESCE(SUM(CASE
        WHEN [o].[OrderID] % 2 = 0 THEN 1
        ELSE 0
    END), 0) / COUNT(*)
END AS [SomeValue]
FROM [Orders] AS [o]
GROUP BY [o].[OrderDate]
""");
    }

    public override async Task GroupBy_aggregate_projecting_conditional_expression_based_on_group_key(bool async)
    {
        await base.GroupBy_aggregate_projecting_conditional_expression_based_on_group_key(async);

        AssertSql(
            """
SELECT CASE
    WHEN [o].[OrderDate] IS NULL THEN N'is null'
    ELSE N'is not null'
END AS [Key], COALESCE(SUM([o].[OrderID]), 0) AS [Sum]
FROM [Orders] AS [o]
GROUP BY [o].[OrderDate]
""");
    }

    public override async Task GroupBy_anonymous_Select_Average(bool async)
    {
        await base.GroupBy_anonymous_Select_Average(async);

        AssertSql(
            """
SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_anonymous_Select_Count(bool async)
    {
        await base.GroupBy_anonymous_Select_Count(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_anonymous_Select_LongCount(bool async)
    {
        await base.GroupBy_anonymous_Select_LongCount(async);

        AssertSql(
            """
SELECT COUNT_BIG(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_anonymous_Select_Max(bool async)
    {
        await base.GroupBy_anonymous_Select_Max(async);

        AssertSql(
            """
SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_anonymous_Select_Min(bool async)
    {
        await base.GroupBy_anonymous_Select_Min(async);

        AssertSql(
            """
SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_anonymous_Select_Sum(bool async)
    {
        await base.GroupBy_anonymous_Select_Sum(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID]), 0)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_anonymous_Select_Sum_Min_Max_Avg(bool async)
    {
        await base.GroupBy_anonymous_Select_Sum_Min_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_anonymous_with_alias_Select_Key_Sum(bool async)
    {
        await base.GroupBy_anonymous_with_alias_Select_Key_Sum(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], COALESCE(SUM([o].[OrderID]), 0) AS [Sum]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Composite_Select_Average(bool async)
    {
        await base.GroupBy_Composite_Select_Average(async);

        AssertSql(
            """
SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_Composite_Select_Count(bool async)
    {
        await base.GroupBy_Composite_Select_Count(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_Composite_Select_LongCount(bool async)
    {
        await base.GroupBy_Composite_Select_LongCount(async);

        AssertSql(
            """
SELECT COUNT_BIG(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_Composite_Select_Max(bool async)
    {
        await base.GroupBy_Composite_Select_Max(async);

        AssertSql(
            """
SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_Composite_Select_Min(bool async)
    {
        await base.GroupBy_Composite_Select_Min(async);

        AssertSql(
            """
SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_Composite_Select_Sum(bool async)
    {
        await base.GroupBy_Composite_Select_Sum(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID]), 0)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_Composite_Select_Sum_Min_Max_Avg(bool async)
    {
        await base.GroupBy_Composite_Select_Sum_Min_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_Composite_Select_Key_Average(bool async)
    {
        await base.GroupBy_Composite_Select_Key_Average(async);

        AssertSql(
            """
SELECT [o].[CustomerID], [o].[EmployeeID], AVG(CAST([o].[OrderID] AS float)) AS [Average]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_Composite_Select_Key_Count(bool async)
    {
        await base.GroupBy_Composite_Select_Key_Count(async);

        AssertSql(
            """
SELECT [o].[CustomerID], [o].[EmployeeID], COUNT(*) AS [Count]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_Composite_Select_Key_LongCount(bool async)
    {
        await base.GroupBy_Composite_Select_Key_LongCount(async);

        AssertSql(
            """
SELECT [o].[CustomerID], [o].[EmployeeID], COUNT_BIG(*) AS [LongCount]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_Composite_Select_Key_Max(bool async)
    {
        await base.GroupBy_Composite_Select_Key_Max(async);

        AssertSql(
            """
SELECT [o].[CustomerID], [o].[EmployeeID], MAX([o].[OrderID]) AS [Max]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_Composite_Select_Key_Min(bool async)
    {
        await base.GroupBy_Composite_Select_Key_Min(async);

        AssertSql(
            """
SELECT [o].[CustomerID], [o].[EmployeeID], MIN([o].[OrderID]) AS [Min]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_Composite_Select_Key_Sum(bool async)
    {
        await base.GroupBy_Composite_Select_Key_Sum(async);

        AssertSql(
            """
SELECT [o].[CustomerID], [o].[EmployeeID], COALESCE(SUM([o].[OrderID]), 0) AS [Sum]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_Composite_Select_Key_Sum_Min_Max_Avg(bool async)
    {
        await base.GroupBy_Composite_Select_Key_Sum_Min_Max_Avg(async);

        AssertSql(
            """
SELECT [o].[CustomerID], [o].[EmployeeID], COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_Composite_Select_Sum_Min_Key_Max_Avg(bool async)
    {
        await base.GroupBy_Composite_Select_Sum_Min_Key_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], [o].[CustomerID], [o].[EmployeeID], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_Composite_Select_Sum_Min_Key_flattened_Max_Avg(bool async)
    {
        await base.GroupBy_Composite_Select_Sum_Min_Key_flattened_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], [o].[CustomerID], [o].[EmployeeID], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_Dto_as_key_Select_Sum(bool async)
    {
        await base.GroupBy_Dto_as_key_Select_Sum(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], [o].[CustomerID], [o].[EmployeeID]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_Dto_as_element_selector_Select_Sum(bool async)
    {
        await base.GroupBy_Dto_as_element_selector_Select_Sum(async);

        AssertSql(
            """
SELECT COALESCE(SUM(CAST([o].[EmployeeID] AS bigint)), CAST(0 AS bigint)) AS [Sum], [o].[CustomerID] AS [Key]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Composite_Select_Dto_Sum_Min_Key_flattened_Max_Avg(bool async)
    {
        await base.GroupBy_Composite_Select_Dto_Sum_Min_Key_flattened_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], [o].[CustomerID] AS [CustomerId], [o].[EmployeeID] AS [EmployeeId], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_Composite_Select_Sum_Min_part_Key_flattened_Max_Avg(bool async)
    {
        await base.GroupBy_Composite_Select_Sum_Min_part_Key_flattened_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], [o].[CustomerID], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_Constant_Select_Sum_Min_Key_Max_Avg(bool async)
    {
        await base.GroupBy_Constant_Select_Sum_Min_Key_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o0].[OrderID]), 0) AS [Sum], MIN([o0].[OrderID]) AS [Min], [o0].[Key], MAX([o0].[OrderID]) AS [Max], AVG(CAST([o0].[OrderID] AS float)) AS [Avg]
FROM (
    SELECT [o].[OrderID], 2 AS [Key]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[Key]
""");
    }

    public override async Task GroupBy_Constant_with_element_selector_Select_Sum(bool async)
    {
        await base.GroupBy_Constant_with_element_selector_Select_Sum(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o0].[OrderID]), 0) AS [Sum]
FROM (
    SELECT [o].[OrderID], 2 AS [Key]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[Key]
""");
    }

    public override async Task GroupBy_Constant_with_element_selector_Select_Sum2(bool async)
    {
        await base.GroupBy_Constant_with_element_selector_Select_Sum2(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o0].[OrderID]), 0) AS [Sum]
FROM (
    SELECT [o].[OrderID], 2 AS [Key]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[Key]
""");
    }

    public override async Task GroupBy_Constant_with_element_selector_Select_Sum3(bool async)
    {
        await base.GroupBy_Constant_with_element_selector_Select_Sum3(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o0].[OrderID]), 0) AS [Sum]
FROM (
    SELECT [o].[OrderID], 2 AS [Key]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[Key]
""");
    }

    public override async Task GroupBy_after_predicate_Constant_Select_Sum_Min_Key_Max_Avg(bool async)
    {
        await base.GroupBy_after_predicate_Constant_Select_Sum_Min_Key_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o0].[OrderID]), 0) AS [Sum], MIN([o0].[OrderID]) AS [Min], [o0].[Key] AS [Random], MAX([o0].[OrderID]) AS [Max], AVG(CAST([o0].[OrderID] AS float)) AS [Avg]
FROM (
    SELECT [o].[OrderID], 2 AS [Key]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] > 10500
) AS [o0]
GROUP BY [o0].[Key]
""");
    }

    public override async Task GroupBy_Constant_with_element_selector_Select_Sum_Min_Key_Max_Avg(bool async)
    {
        await base.GroupBy_Constant_with_element_selector_Select_Sum_Min_Key_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o0].[OrderID]), 0) AS [Sum], [o0].[Key]
FROM (
    SELECT [o].[OrderID], 2 AS [Key]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[Key]
""");
    }

    public override async Task GroupBy_constant_with_where_on_grouping_with_aggregate_operators(bool async)
    {
        await base.GroupBy_constant_with_where_on_grouping_with_aggregate_operators(async);

        AssertSql(
            """
SELECT MIN(CASE
    WHEN 1 = [o0].[Key] THEN [o0].[OrderDate]
END) AS [Min], MAX(CASE
    WHEN 1 = [o0].[Key] THEN [o0].[OrderDate]
END) AS [Max], COALESCE(SUM(CASE
    WHEN 1 = [o0].[Key] THEN [o0].[OrderID]
END), 0) AS [Sum], AVG(CASE
    WHEN 1 = [o0].[Key] THEN CAST([o0].[OrderID] AS float)
END) AS [Average]
FROM (
    SELECT [o].[OrderID], [o].[OrderDate], 1 AS [Key]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[Key]
ORDER BY [o0].[Key]
""");
    }

    public override async Task GroupBy_param_Select_Sum_Min_Key_Max_Avg(bool async)
    {
        await base.GroupBy_param_Select_Sum_Min_Key_Max_Avg(async);

        AssertSql(
            """
@__a_0='2'

SELECT COALESCE(SUM([o0].[OrderID]), 0) AS [Sum], MIN([o0].[OrderID]) AS [Min], [o0].[Key], MAX([o0].[OrderID]) AS [Max], AVG(CAST([o0].[OrderID] AS float)) AS [Avg]
FROM (
    SELECT [o].[OrderID], @__a_0 AS [Key]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[Key]
""");
    }

    public override async Task GroupBy_param_with_element_selector_Select_Sum(bool async)
    {
        await base.GroupBy_param_with_element_selector_Select_Sum(async);

        AssertSql(
            """
@__a_0='2'

SELECT COALESCE(SUM([o0].[OrderID]), 0) AS [Sum]
FROM (
    SELECT [o].[OrderID], @__a_0 AS [Key]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[Key]
""");
    }

    public override async Task GroupBy_param_with_element_selector_Select_Sum2(bool async)
    {
        await base.GroupBy_param_with_element_selector_Select_Sum2(async);

        AssertSql(
            """
@__a_0='2'

SELECT COALESCE(SUM([o0].[OrderID]), 0) AS [Sum]
FROM (
    SELECT [o].[OrderID], @__a_0 AS [Key]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[Key]
""");
    }

    public override async Task GroupBy_param_with_element_selector_Select_Sum3(bool async)
    {
        await base.GroupBy_param_with_element_selector_Select_Sum3(async);

        AssertSql(
            """
@__a_0='2'

SELECT COALESCE(SUM([o0].[OrderID]), 0) AS [Sum]
FROM (
    SELECT [o].[OrderID], @__a_0 AS [Key]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[Key]
""");
    }

    public override async Task GroupBy_param_with_element_selector_Select_Sum_Min_Key_Max_Avg(bool async)
    {
        await base.GroupBy_param_with_element_selector_Select_Sum_Min_Key_Max_Avg(async);

        AssertSql(
            """
@__a_0='2'

SELECT COALESCE(SUM([o0].[OrderID]), 0) AS [Sum], [o0].[Key]
FROM (
    SELECT [o].[OrderID], @__a_0 AS [Key]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[Key]
""");
    }

    public override async Task GroupBy_anonymous_key_type_mismatch_with_aggregate(bool async)
    {
        await base.GroupBy_anonymous_key_type_mismatch_with_aggregate(async);

        AssertSql(
            """
SELECT COUNT(*) AS [I0], [o0].[I0] AS [I1]
FROM (
    SELECT DATEPART(year, [o].[OrderDate]) AS [I0]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[I0]
ORDER BY [o0].[I0]
""");
    }

    public override async Task GroupBy_Property_scalar_element_selector_Average(bool async)
    {
        await base.GroupBy_Property_scalar_element_selector_Average(async);

        AssertSql(
            """
SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_scalar_element_selector_Count(bool async)
    {
        await base.GroupBy_Property_scalar_element_selector_Count(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_scalar_element_selector_LongCount(bool async)
    {
        await base.GroupBy_Property_scalar_element_selector_LongCount(async);

        AssertSql(
            """
SELECT COUNT_BIG(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_scalar_element_selector_Max(bool async)
    {
        await base.GroupBy_Property_scalar_element_selector_Max(async);

        AssertSql(
            """
SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_scalar_element_selector_Min(bool async)
    {
        await base.GroupBy_Property_scalar_element_selector_Min(async);

        AssertSql(
            """
SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_scalar_element_selector_Sum(bool async)
    {
        await base.GroupBy_Property_scalar_element_selector_Sum(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID]), 0)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_scalar_element_selector_Sum_Min_Max_Avg(bool async)
    {
        await base.GroupBy_Property_scalar_element_selector_Sum_Min_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_anonymous_element_selector_Average(bool async)
    {
        await base.GroupBy_Property_anonymous_element_selector_Average(async);

        AssertSql(
            """
SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_anonymous_element_selector_Count(bool async)
    {
        await base.GroupBy_Property_anonymous_element_selector_Count(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_anonymous_element_selector_LongCount(bool async)
    {
        await base.GroupBy_Property_anonymous_element_selector_LongCount(async);

        AssertSql(
            """
SELECT COUNT_BIG(*)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_anonymous_element_selector_Max(bool async)
    {
        await base.GroupBy_Property_anonymous_element_selector_Max(async);

        AssertSql(
            """
SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_anonymous_element_selector_Min(bool async)
    {
        await base.GroupBy_Property_anonymous_element_selector_Min(async);

        AssertSql(
            """
SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_anonymous_element_selector_Sum(bool async)
    {
        await base.GroupBy_Property_anonymous_element_selector_Sum(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID]), 0)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_anonymous_element_selector_Sum_Min_Max_Avg(bool async)
    {
        await base.GroupBy_Property_anonymous_element_selector_Sum_Min_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[EmployeeID]) AS [Min], MAX([o].[EmployeeID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_element_selector_complex_aggregate(bool async)
    {
        await base.GroupBy_element_selector_complex_aggregate(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID] + 1), 0)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_element_selector_complex_aggregate2(bool async)
    {
        await base.GroupBy_element_selector_complex_aggregate2(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID] + 1), 0)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_element_selector_complex_aggregate3(bool async)
    {
        await base.GroupBy_element_selector_complex_aggregate3(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID] + 1), 0)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_element_selector_complex_aggregate4(bool async)
    {
        await base.GroupBy_element_selector_complex_aggregate4(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID] + 1), 0)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task Element_selector_with_case_block_repeated_inside_another_case_block_in_projection(bool async)
    {
        await base.Element_selector_with_case_block_repeated_inside_another_case_block_in_projection(async);

        AssertSql(
            """
SELECT [o].[OrderID], COALESCE(SUM(CASE
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
GROUP BY [o].[OrderID]
""");
    }

    public override async Task GroupBy_conditional_properties(bool async)
    {
        await base.GroupBy_conditional_properties(async);

        AssertSql(
            """
SELECT [o0].[OrderMonth], [o0].[CustomerID] AS [Customer], COUNT(*) AS [Count]
FROM (
    SELECT [o].[CustomerID], NULL AS [OrderMonth]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[OrderMonth], [o0].[CustomerID]
""");
    }

    public override async Task GroupBy_empty_key_Aggregate(bool async)
    {
        await base.GroupBy_empty_key_Aggregate(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o0].[OrderID]), 0)
FROM (
    SELECT [o].[OrderID], 1 AS [Key]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[Key]
""");
    }

    public override async Task GroupBy_empty_key_Aggregate_Key(bool async)
    {
        await base.GroupBy_empty_key_Aggregate_Key(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o0].[OrderID]), 0) AS [Sum]
FROM (
    SELECT [o].[OrderID], 1 AS [Key]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[Key]
""");
    }

    public override async Task OrderBy_GroupBy_Aggregate(bool async)
    {
        await base.OrderBy_GroupBy_Aggregate(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID]), 0)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task OrderBy_Skip_GroupBy_Aggregate(bool async)
    {
        await base.OrderBy_Skip_GroupBy_Aggregate(async);

        AssertSql(
            """
@__p_0='80'

SELECT AVG(CAST([o0].[OrderID] AS float))
FROM (
    SELECT [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS
) AS [o0]
GROUP BY [o0].[CustomerID]
""");
    }

    public override async Task OrderBy_Take_GroupBy_Aggregate(bool async)
    {
        await base.OrderBy_Take_GroupBy_Aggregate(async);

        AssertSql(
            """
@__p_0='500'

SELECT MIN([o0].[OrderID])
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [o0]
GROUP BY [o0].[CustomerID]
""");
    }

    public override async Task OrderBy_Skip_Take_GroupBy_Aggregate(bool async)
    {
        await base.OrderBy_Skip_Take_GroupBy_Aggregate(async);

        AssertSql(
            """
@__p_0='80'
@__p_1='500'

SELECT MAX([o0].[OrderID])
FROM (
    SELECT [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [o0]
GROUP BY [o0].[CustomerID]
""");
    }

    public override async Task Distinct_GroupBy_Aggregate(bool async)
    {
        await base.Distinct_GroupBy_Aggregate(async);

        AssertSql(
            """
SELECT [o0].[CustomerID] AS [Key], COUNT(*) AS [c]
FROM (
    SELECT DISTINCT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[CustomerID]
""");
    }

    public override async Task Anonymous_projection_Distinct_GroupBy_Aggregate(bool async)
    {
        await base.Anonymous_projection_Distinct_GroupBy_Aggregate(async);

        AssertSql(
            """
SELECT [o0].[EmployeeID] AS [Key], COUNT(*) AS [c]
FROM (
    SELECT DISTINCT [o].[OrderID], [o].[EmployeeID]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[EmployeeID]
""");
    }

    public override async Task SelectMany_GroupBy_Aggregate(bool async)
    {
        await base.SelectMany_GroupBy_Aggregate(async);

        AssertSql(
            """
SELECT [o].[EmployeeID] AS [Key], COUNT(*) AS [c]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
GROUP BY [o].[EmployeeID]
""");
    }

    public override async Task Join_GroupBy_Aggregate(bool async)
    {
        await base.Join_GroupBy_Aggregate(async);

        AssertSql(
            """
SELECT [c].[CustomerID] AS [Key], AVG(CAST([o].[OrderID] AS float)) AS [Count]
FROM [Orders] AS [o]
INNER JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [c].[CustomerID]
""");
    }

    public override async Task GroupBy_required_navigation_member_Aggregate(bool async)
    {
        await base.GroupBy_required_navigation_member_Aggregate(async);

        AssertSql(
            """
SELECT [o0].[CustomerID] AS [CustomerId], COUNT(*) AS [Count]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
GROUP BY [o0].[CustomerID]
""");
    }

    public override async Task Join_complex_GroupBy_Aggregate(bool async)
    {
        await base.Join_complex_GroupBy_Aggregate(async);

        AssertSql(
            """
@__p_0='100'
@__p_1='10'
@__p_2='50'

SELECT [c0].[CustomerID] AS [Key], AVG(CAST([o0].[OrderID] AS float)) AS [Count]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10400
    ORDER BY [o].[OrderDate]
) AS [o0]
INNER JOIN (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] NOT IN (N'DRACD', N'FOLKO')
    ORDER BY [c].[City]
    OFFSET @__p_1 ROWS FETCH NEXT @__p_2 ROWS ONLY
) AS [c0] ON [o0].[CustomerID] = [c0].[CustomerID]
GROUP BY [c0].[CustomerID]
""");
    }

    public override async Task GroupJoin_GroupBy_Aggregate(bool async)
    {
        await base.GroupJoin_GroupBy_Aggregate(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], AVG(CAST([o].[OrderID] AS float)) AS [Average]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [o].[OrderID] IS NOT NULL
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupJoin_GroupBy_Aggregate_2(bool async)
    {
        await base.GroupJoin_GroupBy_Aggregate_2(async);

        AssertSql(
            """
SELECT [c].[CustomerID] AS [Key], MAX([c].[City]) AS [Max]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
GROUP BY [c].[CustomerID]
""");
    }

    public override async Task GroupJoin_GroupBy_Aggregate_3(bool async)
    {
        await base.GroupJoin_GroupBy_Aggregate_3(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], AVG(CAST([o].[OrderID] AS float)) AS [Average]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupJoin_GroupBy_Aggregate_4(bool async)
    {
        await base.GroupJoin_GroupBy_Aggregate_4(async);

        AssertSql(
            """
SELECT [c].[CustomerID] AS [Value], MAX([c].[City]) AS [Max]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
GROUP BY [c].[CustomerID]
""");
    }

    public override async Task GroupJoin_GroupBy_Aggregate_5(bool async)
    {
        await base.GroupJoin_GroupBy_Aggregate_5(async);

        AssertSql(
            """
SELECT [o].[OrderID] AS [Value], AVG(CAST([o].[OrderID] AS float)) AS [Average]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [o].[OrderID]
""");
    }

    public override async Task GroupBy_optional_navigation_member_Aggregate(bool async)
    {
        await base.GroupBy_optional_navigation_member_Aggregate(async);

        AssertSql(
            """
SELECT [c].[Country], COUNT(*) AS [Count]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [c].[Country]
""");
    }

    public override async Task GroupJoin_complex_GroupBy_Aggregate(bool async)
    {
        await base.GroupJoin_complex_GroupBy_Aggregate(async);

        AssertSql(
            """
@__p_0='10'
@__p_1='50'
@__p_2='100'

SELECT [o0].[CustomerID] AS [Key], AVG(CAST([o0].[OrderID] AS float)) AS [Count]
FROM (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] NOT IN (N'DRACD', N'FOLKO')
    ORDER BY [c].[City]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [c0]
INNER JOIN (
    SELECT TOP(@__p_2) [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10400
    ORDER BY [o].[OrderDate]
) AS [o0] ON [c0].[CustomerID] = [o0].[CustomerID]
WHERE [o0].[OrderID] > 10300
GROUP BY [o0].[CustomerID]
""");
    }

    public override async Task Self_join_GroupBy_Aggregate(bool async)
    {
        await base.Self_join_GroupBy_Aggregate(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], AVG(CAST([o0].[OrderID] AS float)) AS [Count]
FROM [Orders] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE [o].[OrderID] < 10400
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_multi_navigation_members_Aggregate(bool async)
    {
        await base.GroupBy_multi_navigation_members_Aggregate(async);

        AssertSql(
            """
SELECT [o0].[CustomerID], [p].[ProductName], COUNT(*) AS [Count]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
INNER JOIN [Products] AS [p] ON [o].[ProductID] = [p].[ProductID]
GROUP BY [o0].[CustomerID], [p].[ProductName]
""");
    }

    public override async Task Union_simple_groupby(bool async)
    {
        await base.Union_simple_groupby(async);

        AssertSql(
            """
SELECT [u].[City] AS [Key], COUNT(*) AS [Total]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[ContactTitle] = N'Owner'
    UNION
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE [c0].[City] = N'México D.F.'
) AS [u]
GROUP BY [u].[City]
""");
    }

    public override async Task Select_anonymous_GroupBy_Aggregate(bool async)
    {
        await base.Select_anonymous_GroupBy_Aggregate(async);

        AssertSql(
            """
SELECT MIN([o].[OrderDate]) AS [Min], MAX([o].[OrderDate]) AS [Max], COALESCE(SUM([o].[OrderID]), 0) AS [Sum], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_principal_key_property_optimization(bool async)
    {
        await base.GroupBy_principal_key_property_optimization(async);

        AssertSql(
            """
SELECT [c].[CustomerID] AS [Key], COUNT(*) AS [Count]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [c].[CustomerID]
""");
    }

    public override async Task GroupBy_after_anonymous_projection_and_distinct_followed_by_another_anonymous_projection(bool async)
    {
        await base.GroupBy_after_anonymous_projection_and_distinct_followed_by_another_anonymous_projection(async);

        AssertSql(
            """
SELECT [o0].[CustomerID] AS [Key], COUNT(*) AS [Count]
FROM (
    SELECT DISTINCT [o].[CustomerID], [o].[OrderID]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[CustomerID]
""");
    }

    public override async Task GroupBy_complex_key_aggregate(bool async)
    {
        await base.GroupBy_complex_key_aggregate(async);

        AssertSql(
            """
SELECT [s].[Key], COUNT(*) AS [Count]
FROM (
    SELECT SUBSTRING([c].[CustomerID], 0 + 1, 1) AS [Key]
    FROM [Orders] AS [o]
    LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
) AS [s]
GROUP BY [s].[Key]
""");
    }

    public override async Task GroupBy_complex_key_aggregate_2(bool async)
    {
        await base.GroupBy_complex_key_aggregate_2(async);

        AssertSql(
            """
SELECT [o0].[Key] AS [Month], COALESCE(SUM([o0].[OrderID]), 0) AS [Total], (
    SELECT COALESCE(SUM([o1].[OrderID]), 0)
    FROM [Orders] AS [o1]
    WHERE DATEPART(month, [o1].[OrderDate]) = [o0].[Key] OR ([o1].[OrderDate] IS NULL AND [o0].[Key] IS NULL)) AS [Payment]
FROM (
    SELECT [o].[OrderID], DATEPART(month, [o].[OrderDate]) AS [Key]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[Key]
""");
    }

    public override async Task Select_collection_of_scalar_before_GroupBy_aggregate(bool async)
    {
        await base.Select_collection_of_scalar_before_GroupBy_aggregate(async);

        AssertSql(
            """
SELECT [c].[City] AS [Key], COUNT(*) AS [Count]
FROM [Customers] AS [c]
GROUP BY [c].[City]
""");
    }

    public override async Task GroupBy_OrderBy_key(bool async)
    {
        await base.GroupBy_OrderBy_key(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], COUNT(*) AS [c]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
ORDER BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_OrderBy_count(bool async)
    {
        await base.GroupBy_OrderBy_count(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], COUNT(*) AS [Count]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
ORDER BY COUNT(*), [o].[CustomerID]
""");
    }

    public override async Task GroupBy_OrderBy_count_Select_sum(bool async)
    {
        await base.GroupBy_OrderBy_count_Select_sum(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], COALESCE(SUM([o].[OrderID]), 0) AS [Sum]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
ORDER BY COUNT(*), [o].[CustomerID]
""");
    }

    public override async Task GroupBy_aggregate_Contains(bool async)
    {
        await base.GroupBy_aggregate_Contains(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o0]
    GROUP BY [o0].[CustomerID]
    HAVING COUNT(*) > 30 AND ([o0].[CustomerID] = [o].[CustomerID] OR ([o0].[CustomerID] IS NULL AND [o].[CustomerID] IS NULL)))
""");
    }

    public override async Task GroupBy_aggregate_Pushdown(bool async)
    {
        await base.GroupBy_aggregate_Pushdown(async);

        AssertSql(
            """
@__p_0='20'
@__p_1='4'

SELECT [o0].[CustomerID]
FROM (
    SELECT TOP(@__p_0) [o].[CustomerID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 10
    ORDER BY [o].[CustomerID]
) AS [o0]
ORDER BY [o0].[CustomerID]
OFFSET @__p_1 ROWS
""");
    }

    public override async Task GroupBy_aggregate_using_grouping_key_Pushdown(bool async)
    {
        await base.GroupBy_aggregate_using_grouping_key_Pushdown(async);

        AssertSql(
            """
@__p_0='20'
@__p_1='4'

SELECT [o0].[Key], [o0].[Max]
FROM (
    SELECT TOP(@__p_0) [o].[CustomerID] AS [Key], MAX([o].[CustomerID]) AS [Max]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 10
    ORDER BY [o].[CustomerID]
) AS [o0]
ORDER BY [o0].[Key]
OFFSET @__p_1 ROWS
""");
    }

    public override async Task GroupBy_aggregate_Pushdown_followed_by_projecting_Length(bool async)
    {
        await base.GroupBy_aggregate_Pushdown_followed_by_projecting_Length(async);

        AssertSql(
            """
@__p_0='20'
@__p_1='4'

SELECT CAST(LEN([o0].[CustomerID]) AS int)
FROM (
    SELECT TOP(@__p_0) [o].[CustomerID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 10
    ORDER BY [o].[CustomerID]
) AS [o0]
ORDER BY [o0].[CustomerID]
OFFSET @__p_1 ROWS
""");
    }

    public override async Task GroupBy_aggregate_Pushdown_followed_by_projecting_constant(bool async)
    {
        await base.GroupBy_aggregate_Pushdown_followed_by_projecting_constant(async);

        AssertSql(
            """
@__p_0='20'
@__p_1='4'

SELECT 5
FROM (
    SELECT TOP(@__p_0) [o].[CustomerID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 10
    ORDER BY [o].[CustomerID]
) AS [o0]
ORDER BY [o0].[CustomerID]
OFFSET @__p_1 ROWS
""");
    }

    public override async Task GroupBy_filter_key(bool async)
    {
        await base.GroupBy_filter_key(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], COUNT(*) AS [c]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
HAVING [o].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task GroupBy_filter_count(bool async)
    {
        await base.GroupBy_filter_count(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], COUNT(*) AS [Count]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
HAVING COUNT(*) > 4
""");
    }

    public override async Task GroupBy_count_filter(bool async)
    {
        await base.GroupBy_count_filter(async);

        AssertSql(
            """
SELECT [o0].[Key] AS [Name], COUNT(*) AS [Count]
FROM (
    SELECT N'Order' AS [Key]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[Key]
HAVING COUNT(*) > 0
""");
    }

    public override async Task GroupBy_filter_count_OrderBy_count_Select_sum(bool async)
    {
        await base.GroupBy_filter_count_OrderBy_count_Select_sum(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], COUNT(*) AS [Count], COALESCE(SUM([o].[OrderID]), 0) AS [Sum]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
HAVING COUNT(*) > 4
ORDER BY COUNT(*), [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Aggregate_Join(bool async)
    {
        await base.GroupBy_Aggregate_Join(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate]
FROM (
    SELECT [o].[CustomerID], MAX([o].[OrderID]) AS [LastOrderID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 5
) AS [o0]
INNER JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
INNER JOIN [Orders] AS [o1] ON [o0].[LastOrderID] = [o1].[OrderID]
""");
    }

    public override async Task GroupBy_Aggregate_Join_converted_from_SelectMany(bool async)
    {
        await base.GroupBy_Aggregate_Join_converted_from_SelectMany(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 5
) AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
""");
    }

    public override async Task GroupBy_Aggregate_LeftJoin_converted_from_SelectMany(bool async)
    {
        await base.GroupBy_Aggregate_LeftJoin_converted_from_SelectMany(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 5
) AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
""");
    }

    public override async Task Join_GroupBy_Aggregate_multijoins(bool async)
    {
        await base.Join_GroupBy_Aggregate_multijoins(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [o].[CustomerID], MAX([o].[OrderID]) AS [LastOrderID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 5
) AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
INNER JOIN [Orders] AS [o1] ON [o0].[LastOrderID] = [o1].[OrderID]
""");
    }

    public override async Task Join_GroupBy_Aggregate_single_join(bool async)
    {
        await base.Join_GroupBy_Aggregate_single_join(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o0].[LastOrderID]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [o].[CustomerID], MAX([o].[OrderID]) AS [LastOrderID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 5
) AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
""");
    }

    public override async Task Join_GroupBy_Aggregate_with_another_join(bool async)
    {
        await base.Join_GroupBy_Aggregate_with_another_join(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o0].[LastOrderID], [o1].[OrderID]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [o].[CustomerID], MAX([o].[OrderID]) AS [LastOrderID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 5
) AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
INNER JOIN [Orders] AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
""");
    }

    public override async Task Join_GroupBy_Aggregate_distinct_single_join(bool async)
    {
        await base.Join_GroupBy_Aggregate_distinct_single_join(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o1].[LastOrderID]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT DISTINCT [o0].[CustomerID], MAX([o0].[OrderID]) AS [LastOrderID]
    FROM (
        SELECT [o].[OrderID], [o].[CustomerID], DATEPART(year, [o].[OrderDate]) AS [Year]
        FROM [Orders] AS [o]
    ) AS [o0]
    GROUP BY [o0].[CustomerID], [o0].[Year]
    HAVING COUNT(*) > 5
) AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
""");
    }

    public override async Task Join_GroupBy_Aggregate_with_left_join(bool async)
    {
        await base.Join_GroupBy_Aggregate_with_left_join(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o0].[LastOrderID]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[CustomerID], MAX([o].[OrderID]) AS [LastOrderID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 5
) AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE [c].[CustomerID] LIKE N'A%'
""");
    }

    public override async Task Join_GroupBy_Aggregate_in_subquery(bool async)
    {
        await base.Join_GroupBy_Aggregate_in_subquery(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [s].[CustomerID], [s].[Address], [s].[City], [s].[CompanyName], [s].[ContactName], [s].[ContactTitle], [s].[Country], [s].[Fax], [s].[Phone], [s].[PostalCode], [s].[Region]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    INNER JOIN (
        SELECT [o0].[CustomerID]
        FROM [Orders] AS [o0]
        GROUP BY [o0].[CustomerID]
        HAVING COUNT(*) > 5
    ) AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
) AS [s] ON [o].[CustomerID] = [s].[CustomerID]
WHERE [o].[OrderID] < 10400
""");
    }

    public override async Task Join_GroupBy_Aggregate_on_key(bool async)
    {
        await base.Join_GroupBy_Aggregate_on_key(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o0].[LastOrderID]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [o].[CustomerID] AS [Key], MAX([o].[OrderID]) AS [LastOrderID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 5
) AS [o0] ON [c].[CustomerID] = [o0].[Key]
""");
    }

    public override async Task GroupBy_with_result_selector(bool async)
    {
        await base.GroupBy_with_result_selector(async);

        AssertSql(
            """
SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], MIN([o].[OrderID]) AS [Min], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Sum_constant(bool async)
    {
        await base.GroupBy_Sum_constant(async);

        AssertSql(
            """
SELECT COALESCE(SUM(1), 0)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Sum_constant_cast(bool async)
    {
        await base.GroupBy_Sum_constant_cast(async);

        AssertSql(
            """
SELECT COALESCE(SUM(CAST(1 AS bigint)), CAST(0 AS bigint))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task Distinct_GroupBy_OrderBy_key(bool async)
    {
        await base.Distinct_GroupBy_OrderBy_key(async);

        AssertSql(
            """
SELECT [o0].[CustomerID] AS [Key], COUNT(*) AS [c]
FROM (
    SELECT DISTINCT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[CustomerID]
ORDER BY [o0].[CustomerID]
""");
    }

    public override async Task Select_nested_collection_with_groupby(bool async)
    {
        await base.Select_nested_collection_with_groupby(async);

        AssertSql(
            """
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [c].[CustomerID], [o1].[OrderID]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT [o0].[OrderID]
    FROM [Orders] AS [o0]
    WHERE [c].[CustomerID] = [o0].[CustomerID]
    GROUP BY [o0].[OrderID]
) AS [o1]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Select_uncorrelated_collection_with_groupby_works(bool async)
    {
        await base.Select_uncorrelated_collection_with_groupby_works(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o0].[OrderID]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    GROUP BY [o].[OrderID]
) AS [o0]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Select_uncorrelated_collection_with_groupby_multiple_collections_work(bool async)
    {
        await base.Select_uncorrelated_collection_with_groupby_multiple_collections_work(async);

        AssertSql(
            """
SELECT [o].[OrderID], [p1].[ProductID], [p2].[c], [p2].[ProductID]
FROM [Orders] AS [o]
OUTER APPLY (
    SELECT [p].[ProductID]
    FROM [Products] AS [p]
    GROUP BY [p].[ProductID]
) AS [p1]
OUTER APPLY (
    SELECT COUNT(*) AS [c], [p0].[ProductID]
    FROM [Products] AS [p0]
    GROUP BY [p0].[ProductID]
) AS [p2]
WHERE [o].[CustomerID] LIKE N'A%'
ORDER BY [o].[OrderID], [p1].[ProductID]
""");
    }

    public override async Task Select_GroupBy_All(bool async)
    {
        await base.Select_GroupBy_All(async);

        AssertSql(
            """
SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        GROUP BY [o].[CustomerID]
        HAVING [o].[CustomerID] <> N'ALFKI' OR [o].[CustomerID] IS NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task GroupBy_Where_Average(bool async)
    {
        await base.GroupBy_Where_Average(async);

        AssertSql(
            """
SELECT AVG(CASE
    WHEN [o].[OrderID] < 10300 THEN CAST([o].[OrderID] AS float)
END)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Where_Count(bool async)
    {
        await base.GroupBy_Where_Count(async);

        AssertSql(
            """
SELECT COUNT(CASE
    WHEN [o].[OrderID] < 10300 THEN 1
END)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Where_LongCount(bool async)
    {
        await base.GroupBy_Where_LongCount(async);

        AssertSql(
            """
SELECT COUNT_BIG(CASE
    WHEN [o].[OrderID] < 10300 THEN 1
END)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Where_Max(bool async)
    {
        await base.GroupBy_Where_Max(async);

        AssertSql(
            """
SELECT MAX(CASE
    WHEN [o].[OrderID] < 10300 THEN [o].[OrderID]
END)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Where_Min(bool async)
    {
        await base.GroupBy_Where_Min(async);

        AssertSql(
            """
SELECT MIN(CASE
    WHEN [o].[OrderID] < 10300 THEN [o].[OrderID]
END)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Where_Sum(bool async)
    {
        await base.GroupBy_Where_Sum(async);

        AssertSql(
            """
SELECT COALESCE(SUM(CASE
    WHEN [o].[OrderID] < 10300 THEN [o].[OrderID]
END), 0)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Where_Count_with_predicate(bool async)
    {
        await base.GroupBy_Where_Count_with_predicate(async);

        AssertSql(
            """
SELECT COUNT(CASE
    WHEN [o].[OrderID] < 10300 AND [o].[OrderDate] IS NOT NULL AND DATEPART(year, [o].[OrderDate]) = 1997 THEN 1
END)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Where_Where_Count(bool async)
    {
        await base.GroupBy_Where_Where_Count(async);

        AssertSql(
            """
SELECT COUNT(CASE
    WHEN [o].[OrderID] < 10300 AND [o].[OrderDate] IS NOT NULL AND DATEPART(year, [o].[OrderDate]) = 1997 THEN 1
END)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Where_Select_Where_Count(bool async)
    {
        await base.GroupBy_Where_Select_Where_Count(async);

        AssertSql(
            """
SELECT COUNT(CASE
    WHEN [o].[OrderID] < 10300 AND [o].[OrderDate] IS NOT NULL AND DATEPART(year, [o].[OrderDate]) = 1997 THEN 1
END)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Where_Select_Where_Select_Min(bool async)
    {
        await base.GroupBy_Where_Select_Where_Select_Min(async);

        AssertSql(
            """
SELECT MIN(CASE
    WHEN [o].[OrderID] < 10300 AND [o].[OrderDate] IS NOT NULL AND DATEPART(year, [o].[OrderDate]) = 1997 THEN [o].[OrderID]
END)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_multiple_Count_with_predicate(bool async)
    {
        await base.GroupBy_multiple_Count_with_predicate(async);

        AssertSql(
            """
SELECT [o].[CustomerID], COUNT(*) AS [All], COUNT(CASE
    WHEN [o].[OrderID] < 11000 THEN 1
END) AS [TenK], COUNT(CASE
    WHEN [o].[OrderID] < 12000 THEN 1
END) AS [EleventK]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_multiple_Sum_with_conditional_projection(bool async)
    {
        await base.GroupBy_multiple_Sum_with_conditional_projection(async);

        AssertSql(
            """
SELECT [o].[CustomerID], COALESCE(SUM(CASE
    WHEN [o].[OrderID] < 11000 THEN [o].[OrderID]
    ELSE 0
END), 0) AS [TenK], COALESCE(SUM(CASE
    WHEN [o].[OrderID] >= 11000 THEN [o].[OrderID]
    ELSE 0
END), 0) AS [EleventK]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_multiple_Sum_with_Select_conditional_projection(bool async)
    {
        await base.GroupBy_multiple_Sum_with_Select_conditional_projection(async);

        AssertSql(
            """
SELECT [o].[CustomerID], COALESCE(SUM(CASE
    WHEN [o].[OrderID] < 11000 THEN [o].[OrderID]
    ELSE 0
END), 0) AS [TenK], COALESCE(SUM(CASE
    WHEN [o].[OrderID] >= 11000 THEN [o].[OrderID]
    ELSE 0
END), 0) AS [EleventK]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Key_as_part_of_element_selector(bool async)
    {
        await base.GroupBy_Key_as_part_of_element_selector(async);

        AssertSql(
            """
SELECT [o].[OrderID] AS [Key], AVG(CAST([o].[OrderID] AS float)) AS [Avg], MAX([o].[OrderDate]) AS [Max]
FROM [Orders] AS [o]
GROUP BY [o].[OrderID]
""");
    }

    public override async Task GroupBy_composite_Key_as_part_of_element_selector(bool async)
    {
        await base.GroupBy_composite_Key_as_part_of_element_selector(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], AVG(CAST([o].[OrderID] AS float)) AS [Avg], MAX([o].[OrderDate]) AS [Max]
FROM [Orders] AS [o]
GROUP BY [o].[OrderID], [o].[CustomerID]
""");
    }

    public override async Task GroupBy_with_aggregate_through_navigation_property(bool async)
    {
        await base.GroupBy_with_aggregate_through_navigation_property(async);

        AssertSql(
            """
SELECT (
    SELECT MAX([c].[Region])
    FROM [Orders] AS [o0]
    LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
    WHERE [o].[EmployeeID] = [o0].[EmployeeID] OR ([o].[EmployeeID] IS NULL AND [o0].[EmployeeID] IS NULL)) AS [max]
FROM [Orders] AS [o]
GROUP BY [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_with_aggregate_containing_complex_where(bool async)
    {
        await base.GroupBy_with_aggregate_containing_complex_where(async);

        AssertSql(
            """
SELECT [o].[EmployeeID] AS [Key], (
    SELECT MAX([o0].[OrderID])
    FROM [Orders] AS [o0]
    WHERE CAST([o0].[EmployeeID] AS bigint) = CAST(MAX([o].[OrderID]) * 6 AS bigint) OR ([o0].[EmployeeID] IS NULL AND MAX([o].[OrderID]) IS NULL)) AS [Max]
FROM [Orders] AS [o]
GROUP BY [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_Shadow(bool async)
    {
        await base.GroupBy_Shadow(async);

        AssertSql(
            """
SELECT (
    SELECT TOP(1) [e0].[Title]
    FROM [Employees] AS [e0]
    WHERE [e0].[Title] = N'Sales Representative' AND [e0].[EmployeeID] = 1 AND ([e].[Title] = [e0].[Title] OR ([e].[Title] IS NULL AND [e0].[Title] IS NULL)))
FROM [Employees] AS [e]
WHERE [e].[Title] = N'Sales Representative' AND [e].[EmployeeID] = 1
GROUP BY [e].[Title]
""");
    }

    public override async Task GroupBy_Shadow2(bool async)
    {
        await base.GroupBy_Shadow2(async);

        AssertSql(
            """
SELECT [e3].[EmployeeID], [e3].[City], [e3].[Country], [e3].[FirstName], [e3].[ReportsTo], [e3].[Title]
FROM (
    SELECT [e].[Title]
    FROM [Employees] AS [e]
    WHERE [e].[Title] = N'Sales Representative' AND [e].[EmployeeID] = 1
    GROUP BY [e].[Title]
) AS [e1]
LEFT JOIN (
    SELECT [e2].[EmployeeID], [e2].[City], [e2].[Country], [e2].[FirstName], [e2].[ReportsTo], [e2].[Title]
    FROM (
        SELECT [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title], ROW_NUMBER() OVER(PARTITION BY [e0].[Title] ORDER BY [e0].[EmployeeID]) AS [row]
        FROM [Employees] AS [e0]
        WHERE [e0].[Title] = N'Sales Representative' AND [e0].[EmployeeID] = 1
    ) AS [e2]
    WHERE [e2].[row] <= 1
) AS [e3] ON [e1].[Title] = [e3].[Title]
""");
    }

    public override async Task GroupBy_Shadow3(bool async)
    {
        await base.GroupBy_Shadow3(async);

        AssertSql(
            """
SELECT (
    SELECT TOP(1) [e0].[Title]
    FROM [Employees] AS [e0]
    WHERE [e0].[EmployeeID] = 1 AND [e].[EmployeeID] = [e0].[EmployeeID])
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = 1
GROUP BY [e].[EmployeeID]
""");
    }

    public override async Task GroupBy_select_grouping_list(bool async)
    {
        await base.GroupBy_select_grouping_list(async);

        AssertSql(
            """
SELECT [c1].[City], [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT [c].[City]
    FROM [Customers] AS [c]
    GROUP BY [c].[City]
) AS [c1]
LEFT JOIN [Customers] AS [c0] ON [c1].[City] = [c0].[City]
ORDER BY [c1].[City]
""");
    }

    public override async Task GroupBy_select_grouping_array(bool async)
    {
        await base.GroupBy_select_grouping_array(async);

        AssertSql(
            """
SELECT [c1].[City], [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT [c].[City]
    FROM [Customers] AS [c]
    GROUP BY [c].[City]
) AS [c1]
LEFT JOIN [Customers] AS [c0] ON [c1].[City] = [c0].[City]
ORDER BY [c1].[City]
""");
    }

    public override async Task GroupBy_select_grouping_composed_list(bool async)
    {
        await base.GroupBy_select_grouping_composed_list(async);

        AssertSql(
            """
SELECT [c1].[City], [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
FROM (
    SELECT [c].[City]
    FROM [Customers] AS [c]
    GROUP BY [c].[City]
) AS [c1]
LEFT JOIN (
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] LIKE N'A%'
) AS [c2] ON [c1].[City] = [c2].[City]
ORDER BY [c1].[City]
""");
    }

    public override async Task GroupBy_select_grouping_composed_list_2(bool async)
    {
        await base.GroupBy_select_grouping_composed_list_2(async);

        AssertSql(
            """
SELECT [c1].[City], [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT [c].[City]
    FROM [Customers] AS [c]
    GROUP BY [c].[City]
) AS [c1]
LEFT JOIN [Customers] AS [c0] ON [c1].[City] = [c0].[City]
ORDER BY [c1].[City], [c0].[CustomerID]
""");
    }

    public override async Task Select_GroupBy_SelectMany(bool async)
    {
        await base.Select_GroupBy_SelectMany(async);

        AssertSql();
    }

    public override async Task Count_after_GroupBy_aggregate(bool async)
    {
        await base.Count_after_GroupBy_aggregate(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM (
    SELECT 1 AS empty
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [o0]
""");
    }

    public override async Task LongCount_after_GroupBy_aggregate(bool async)
    {
        await base.LongCount_after_GroupBy_aggregate(async);

        AssertSql(
            """
SELECT COUNT_BIG(*)
FROM (
    SELECT 1 AS empty
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [o0]
""");
    }

    public override async Task GroupBy_Select_Distinct_aggregate(bool async)
    {
        await base.GroupBy_Select_Distinct_aggregate(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], AVG(DISTINCT (CAST([o].[OrderID] AS float))) AS [Average], COUNT(DISTINCT ([o].[EmployeeID])) AS [Count], COUNT_BIG(DISTINCT ([o].[EmployeeID])) AS [LongCount], MAX(DISTINCT ([o].[OrderDate])) AS [Max], MIN(DISTINCT ([o].[OrderDate])) AS [Min], COALESCE(SUM(DISTINCT ([o].[OrderID])), 0) AS [Sum]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_group_Distinct_Select_Distinct_aggregate(bool async)
    {
        await base.GroupBy_group_Distinct_Select_Distinct_aggregate(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], MAX(DISTINCT ([o].[OrderDate])) AS [Max]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_group_Where_Select_Distinct_aggregate(bool async)
    {
        await base.GroupBy_group_Where_Select_Distinct_aggregate(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], MAX(DISTINCT (CASE
    WHEN [o].[OrderDate] IS NOT NULL THEN [o].[OrderDate]
END)) AS [Max]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task MinMax_after_GroupBy_aggregate(bool async)
    {
        await base.MinMax_after_GroupBy_aggregate(async);

        AssertSql(
            """
SELECT MIN([o0].[c])
FROM (
    SELECT COALESCE(SUM([o].[OrderID]), 0) AS [c]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [o0]
""",
            //
            """
SELECT MAX([o0].[c])
FROM (
    SELECT COALESCE(SUM([o].[OrderID]), 0) AS [c]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [o0]
""");
    }

    public override async Task All_after_GroupBy_aggregate(bool async)
    {
        await base.All_after_GroupBy_aggregate(async);

        AssertSql(
            """
SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        GROUP BY [o].[CustomerID]
        HAVING 0 = 1) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task All_after_GroupBy_aggregate2(bool async)
    {
        await base.All_after_GroupBy_aggregate2(async);

        AssertSql(
            """
SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        GROUP BY [o].[CustomerID]
        HAVING COALESCE(SUM([o].[OrderID]), 0) < 0) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Any_after_GroupBy_aggregate(bool async)
    {
        await base.Any_after_GroupBy_aggregate(async);

        AssertSql(
            """
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        GROUP BY [o].[CustomerID]) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Count_after_GroupBy_without_aggregate(bool async)
    {
        await base.Count_after_GroupBy_without_aggregate(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM (
    SELECT 1 AS empty
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [o0]
""");
    }

    public override async Task Count_with_predicate_after_GroupBy_without_aggregate(bool async)
    {
        await base.Count_with_predicate_after_GroupBy_without_aggregate(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM (
    SELECT 1 AS empty
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 1
) AS [o0]
""");
    }

    public override async Task LongCount_after_GroupBy_without_aggregate(bool async)
    {
        await base.LongCount_after_GroupBy_without_aggregate(async);

        AssertSql(
            """
SELECT COUNT_BIG(*)
FROM (
    SELECT 1 AS empty
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [o0]
""");
    }

    public override async Task LongCount_with_predicate_after_GroupBy_without_aggregate(bool async)
    {
        await base.LongCount_with_predicate_after_GroupBy_without_aggregate(async);

        AssertSql(
            """
SELECT COUNT_BIG(*)
FROM (
    SELECT 1 AS empty
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 1
) AS [o0]
""");
    }

    public override async Task Any_after_GroupBy_without_aggregate(bool async)
    {
        await base.Any_after_GroupBy_without_aggregate(async);

        AssertSql(
            """
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        GROUP BY [o].[CustomerID]) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Any_with_predicate_after_GroupBy_without_aggregate(bool async)
    {
        await base.Any_with_predicate_after_GroupBy_without_aggregate(async);

        AssertSql(
            """
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        GROUP BY [o].[CustomerID]
        HAVING COUNT(*) > 1) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task All_with_predicate_after_GroupBy_without_aggregate(bool async)
    {
        await base.All_with_predicate_after_GroupBy_without_aggregate(async);

        AssertSql(
            """
SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        GROUP BY [o].[CustomerID]
        HAVING COUNT(*) <= 1) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task GroupBy_aggregate_followed_by_another_GroupBy_aggregate(bool async)
    {
        await base.GroupBy_aggregate_followed_by_another_GroupBy_aggregate(async);

        AssertSql(
            """
SELECT [o1].[Key0] AS [Key], COALESCE(SUM([o1].[Count]), 0) AS [Count]
FROM (
    SELECT [o0].[Count], 1 AS [Key0]
    FROM (
        SELECT COUNT(*) AS [Count]
        FROM [Orders] AS [o]
        GROUP BY [o].[CustomerID]
    ) AS [o0]
) AS [o1]
GROUP BY [o1].[Key0]
""");
    }

    public override async Task GroupBy_Count_in_projection(bool async)
    {
        await base.GroupBy_Count_in_projection(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[OrderDate], CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Order Details] AS [o0]
        WHERE [o].[OrderID] = [o0].[OrderID] AND [o0].[ProductID] < 25) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [HasOrderDetails], CASE
    WHEN (
        SELECT COUNT(*)
        FROM (
            SELECT 1 AS empty
            FROM [Order Details] AS [o1]
            INNER JOIN [Products] AS [p] ON [o1].[ProductID] = [p].[ProductID]
            WHERE [o].[OrderID] = [o1].[OrderID] AND [o1].[ProductID] < 25
            GROUP BY [p].[ProductName]
        ) AS [s]) > 1 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [HasMultipleProducts]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL
""");
    }

    public override async Task GroupBy_nominal_type_count(bool async)
    {
        await base.GroupBy_nominal_type_count(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM (
    SELECT 1 AS empty
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [o0]
""");
    }

    public override async Task GroupBy_based_on_renamed_property_simple(bool async)
    {
        await base.GroupBy_based_on_renamed_property_simple(async);

        AssertSql(
            """
SELECT [c].[City] AS [Renamed], COUNT(*) AS [Count]
FROM [Customers] AS [c]
GROUP BY [c].[City]
""");
    }

    public override async Task GroupBy_based_on_renamed_property_complex(bool async)
    {
        await base.GroupBy_based_on_renamed_property_complex(async);

        AssertSql(
            """
SELECT [c0].[Renamed] AS [Key], COUNT(*) AS [Count]
FROM (
    SELECT DISTINCT [c].[City] AS [Renamed], [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [c0]
GROUP BY [c0].[Renamed]
""");
    }

    public override async Task Join_groupby_anonymous_orderby_anonymous_projection(bool async)
    {
        await base.Join_groupby_anonymous_orderby_anonymous_projection(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
GROUP BY [c].[CustomerID], [o].[OrderDate]
ORDER BY [o].[OrderDate]
""");
    }

    public override async Task Odata_groupby_empty_key(bool async)
    {
        await base.Odata_groupby_empty_key(async);

        AssertSql(
            """
SELECT N'TotalAmount' AS [Name], COALESCE(SUM(CAST([o0].[OrderID] AS decimal(18,2))), 0.0) AS [Value]
FROM (
    SELECT [o].[OrderID], 1 AS [Key]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[Key]
""");
    }

    public override async Task GroupBy_with_group_key_access_thru_navigation(bool async)
    {
        await base.GroupBy_with_group_key_access_thru_navigation(async);

        AssertSql(
            """
SELECT [o0].[CustomerID] AS [Key], COALESCE(SUM([o].[OrderID]), 0) AS [Aggregate]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
GROUP BY [o0].[CustomerID]
""");
    }

    public override async Task GroupBy_with_group_key_access_thru_nested_navigation(bool async)
    {
        await base.GroupBy_with_group_key_access_thru_nested_navigation(async);

        AssertSql(
            """
SELECT [c].[Country] AS [Key], COALESCE(SUM([o].[OrderID]), 0) AS [Aggregate]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
GROUP BY [c].[Country]
""");
    }

    public override async Task GroupBy_with_group_key_being_navigation(bool async)
    {
        await base.GroupBy_with_group_key_being_navigation(async);

        AssertSql(
            """
SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], COALESCE(SUM([o].[OrderID]), 0) AS [Aggregate]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
GROUP BY [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
""");
    }

    public override async Task GroupBy_with_group_key_being_nested_navigation(bool async)
    {
        await base.GroupBy_with_group_key_being_nested_navigation(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], COALESCE(SUM([o].[OrderID]), 0) AS [Aggregate]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
GROUP BY [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
""");
    }

    public override async Task GroupBy_with_group_key_being_navigation_with_entity_key_projection(bool async)
    {
        await base.GroupBy_with_group_key_being_navigation_with_entity_key_projection(async);

        AssertSql(
            """
SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
GROUP BY [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
""");
    }

    public override async Task GroupBy_with_group_key_being_navigation_with_complex_projection(bool async)
    {
        await base.GroupBy_with_group_key_being_navigation_with_complex_projection(async);

        AssertSql();
    }

    public override async Task GroupBy_with_order_by_skip_and_another_order_by(bool async)
    {
        await base.GroupBy_with_order_by_skip_and_another_order_by(async);

        AssertSql(
            """
@__p_0='80'

SELECT COALESCE(SUM([o0].[OrderID]), 0)
FROM (
    SELECT [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    ORDER BY [o].[CustomerID], [o].[OrderID]
    OFFSET @__p_0 ROWS
) AS [o0]
GROUP BY [o0].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_Select_Count_with_predicate(bool async)
    {
        await base.GroupBy_Property_Select_Count_with_predicate(async);

        AssertSql(
            """
SELECT COUNT(CASE
    WHEN [o].[OrderID] < 10300 THEN 1
END)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_Property_Select_LongCount_with_predicate(bool async)
    {
        await base.GroupBy_Property_Select_LongCount_with_predicate(async);

        AssertSql(
            """
SELECT COUNT_BIG(CASE
    WHEN [o].[OrderID] < 10300 THEN 1
END)
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_orderby_projection_with_coalesce_operation(bool async)
    {
        await base.GroupBy_orderby_projection_with_coalesce_operation(async);

        AssertSql(
            """
SELECT COALESCE([c].[City], N'Unknown') AS [Locality], COUNT(*) AS [Count]
FROM [Customers] AS [c]
GROUP BY [c].[City]
ORDER BY COUNT(*) DESC, [c].[City]
""");
    }

    public override async Task GroupBy_let_orderby_projection_with_coalesce_operation(bool async)
    {
        await base.GroupBy_let_orderby_projection_with_coalesce_operation(async);

        AssertSql();
    }

    public override async Task GroupBy_Min_Where_optional_relationship(bool async)
    {
        await base.GroupBy_Min_Where_optional_relationship(async);

        AssertSql(
            """
SELECT [c].[CustomerID] AS [Key], COUNT(*) AS [Count]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [c].[CustomerID]
HAVING COUNT(*) <> 2
""");
    }

    public override async Task GroupBy_Min_Where_optional_relationship_2(bool async)
    {
        await base.GroupBy_Min_Where_optional_relationship_2(async);

        AssertSql(
            """
SELECT [c].[CustomerID] AS [Key], COUNT(*) AS [Count]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [c].[CustomerID]
HAVING COUNT(*) < 2 OR COUNT(*) > 2
""");
    }

    public override async Task GroupBy_aggregate_over_a_subquery(bool async)
    {
        await base.GroupBy_aggregate_over_a_subquery(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], (
    SELECT COUNT(*)
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = [o].[CustomerID]) AS [Count]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_aggregate_join_with_grouping_key(bool async)
    {
        await base.GroupBy_aggregate_join_with_grouping_key(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o0].[Count]
FROM (
    SELECT [o].[CustomerID] AS [Key], COUNT(*) AS [Count]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [o0]
INNER JOIN [Customers] AS [c] ON [o0].[Key] = [c].[CustomerID]
""");
    }

    public override async Task GroupBy_aggregate_join_with_group_result(bool async)
    {
        await base.GroupBy_aggregate_join_with_group_result(async);

        AssertSql(
            """
SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM (
    SELECT [o].[CustomerID] AS [Key], MAX([o].[OrderDate]) AS [LastOrderDate]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [o1]
INNER JOIN [Orders] AS [o0] ON ([o1].[Key] = [o0].[CustomerID] OR ([o1].[Key] IS NULL AND [o0].[CustomerID] IS NULL)) AND ([o1].[LastOrderDate] = [o0].[OrderDate] OR ([o1].[LastOrderDate] IS NULL AND [o0].[OrderDate] IS NULL))
""");
    }

    public override async Task GroupBy_aggregate_from_right_side_of_join(bool async)
    {
        await base.GroupBy_aggregate_from_right_side_of_join(async);

        AssertSql(
            """
@__p_0='10'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o0].[Max]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [o].[CustomerID] AS [Key], MAX([o].[OrderDate]) AS [Max]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [o0] ON [c].[CustomerID] = [o0].[Key]
ORDER BY [o0].[Max], [c].[CustomerID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_0 ROWS ONLY
""");
    }

    public override async Task GroupBy_aggregate_join_another_GroupBy_aggregate(bool async)
    {
        await base.GroupBy_aggregate_join_another_GroupBy_aggregate(async);

        AssertSql(
            """
SELECT [o1].[Key], [o1].[Total], [o2].[ThatYear]
FROM (
    SELECT [o].[CustomerID] AS [Key], COUNT(*) AS [Total]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [o1]
INNER JOIN (
    SELECT [o0].[CustomerID] AS [Key], COUNT(*) AS [ThatYear]
    FROM [Orders] AS [o0]
    WHERE DATEPART(year, [o0].[OrderDate]) = 1997
    GROUP BY [o0].[CustomerID]
) AS [o2] ON [o1].[Key] = [o2].[Key]
""");
    }

    public override async Task GroupBy_aggregate_after_skip_0_take_0(bool async)
    {
        await base.GroupBy_aggregate_after_skip_0_take_0(async);

        AssertSql(
            """
SELECT [o0].[CustomerID] AS [Key], COUNT(*) AS [Total]
FROM (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE 0 = 1
) AS [o0]
GROUP BY [o0].[CustomerID]
""");
    }

    public override async Task GroupBy_skip_0_take_0_aggregate(bool async)
    {
        await base.GroupBy_skip_0_take_0_aggregate(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], COUNT(*) AS [Total]
FROM [Orders] AS [o]
WHERE [o].[OrderID] > 10500
GROUP BY [o].[CustomerID]
HAVING 0 = 1
""");
    }

    public override async Task GroupBy_aggregate_followed_another_GroupBy_aggregate(bool async)
    {
        await base.GroupBy_aggregate_followed_another_GroupBy_aggregate(async);

        AssertSql(
            """
SELECT [o1].[CustomerID] AS [Key], COUNT(*) AS [Count]
FROM (
    SELECT [o0].[CustomerID]
    FROM (
        SELECT [o].[CustomerID], DATEPART(year, [o].[OrderDate]) AS [Year]
        FROM [Orders] AS [o]
    ) AS [o0]
    GROUP BY [o0].[CustomerID], [o0].[Year]
) AS [o1]
GROUP BY [o1].[CustomerID]
""");
    }

    public override async Task GroupBy_aggregate_without_selectMany_selecting_first(bool async)
    {
        await base.GroupBy_aggregate_without_selectMany_selecting_first(async);

        AssertSql(
            """
SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM (
    SELECT MIN([o].[OrderID]) AS [c]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [o1]
CROSS JOIN [Orders] AS [o0]
WHERE [o0].[OrderID] = [o1].[c]
""");
    }

    public override async Task GroupBy_aggregate_left_join_GroupBy_aggregate_left_join(bool async)
    {
        await base.GroupBy_aggregate_left_join_GroupBy_aggregate_left_join(async);

        AssertSql(
            """
SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM (
    SELECT MIN([o].[OrderID]) AS [c]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [t]
CROSS JOIN [Orders] AS [o0]
WHERE [o0].[OrderID] = [t].[c]
""");
    }

    public override async Task GroupBy_selecting_grouping_key_list(bool async)
    {
        await base.GroupBy_selecting_grouping_key_list(async);

        AssertSql(
            """
SELECT [o1].[CustomerID], [o0].[CustomerID], [o0].[OrderID]
FROM (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [o1]
LEFT JOIN [Orders] AS [o0] ON [o1].[CustomerID] = [o0].[CustomerID]
ORDER BY [o1].[CustomerID]
""");
    }

    public override async Task GroupBy_with_grouping_key_using_Like(bool async)
    {
        await base.GroupBy_with_grouping_key_using_Like(async);

        AssertSql(
            """
SELECT [o0].[Key], COUNT(*) AS [Count]
FROM (
    SELECT CASE
        WHEN [o].[CustomerID] LIKE N'A%' AND [o].[CustomerID] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [Key]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[Key]
""");
    }

    public override async Task GroupBy_with_grouping_key_DateTime_Day(bool async)
    {
        await base.GroupBy_with_grouping_key_DateTime_Day(async);

        AssertSql(
            """
SELECT [o0].[Key], COUNT(*) AS [Count]
FROM (
    SELECT DATEPART(day, [o].[OrderDate]) AS [Key]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[Key]
""");
    }

    public override async Task GroupBy_with_cast_inside_grouping_aggregate(bool async)
    {
        await base.GroupBy_with_cast_inside_grouping_aggregate(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], COUNT(*) AS [Count], COALESCE(SUM(CAST([o].[OrderID] AS bigint)), CAST(0 AS bigint)) AS [Sum]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task Complex_query_with_groupBy_in_subquery1(bool async)
    {
        await base.Complex_query_with_groupBy_in_subquery1(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o0].[Sum], [o0].[CustomerID]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT COALESCE(SUM([o].[OrderID]), 0) AS [Sum], [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    GROUP BY [o].[CustomerID]
) AS [o0]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Complex_query_with_groupBy_in_subquery2(bool async)
    {
        await base.Complex_query_with_groupBy_in_subquery2(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o0].[Max], [o0].[Sum], [o0].[CustomerID]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT MAX(CAST(LEN([o].[CustomerID]) AS int)) AS [Max], COALESCE(SUM([o].[OrderID]), 0) AS [Sum], [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    GROUP BY [o].[CustomerID]
) AS [o0]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Complex_query_with_groupBy_in_subquery3(bool async)
    {
        await base.Complex_query_with_groupBy_in_subquery3(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o0].[Max], [o0].[Sum], [o0].[CustomerID]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT MAX(CAST(LEN([o].[CustomerID]) AS int)) AS [Max], COALESCE(SUM([o].[OrderID]), 0) AS [Sum], [o].[CustomerID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [o0]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Group_by_with_projection_into_DTO(bool async)
    {
        await base.Group_by_with_projection_into_DTO(async);

        AssertSql(
            """
SELECT CAST([o].[OrderID] AS bigint) AS [Id], COUNT(*) AS [Count]
FROM [Orders] AS [o]
GROUP BY [o].[OrderID]
""");
    }

    public override async Task Where_select_function_groupby_followed_by_another_select_with_aggregates(bool async)
    {
        await base.Where_select_function_groupby_followed_by_another_select_with_aggregates(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], COALESCE(SUM(CASE
    WHEN 2020 - DATEPART(year, [o].[OrderDate]) <= 30 THEN [o].[OrderID]
    ELSE 0
END), 0) AS [Sum1], COALESCE(SUM(CASE
    WHEN 2020 - DATEPART(year, [o].[OrderDate]) > 30 AND 2020 - DATEPART(year, [o].[OrderDate]) <= 60 THEN [o].[OrderID]
    ELSE 0
END), 0) AS [Sum2]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] LIKE N'A%'
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task Group_by_column_project_constant(bool async)
    {
        await base.Group_by_column_project_constant(async);

        AssertSql(
            """
SELECT 42
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
ORDER BY [o].[CustomerID]
""");
    }

    public override async Task Key_plus_key_in_projection(bool async)
    {
        await base.Key_plus_key_in_projection(async);

        AssertSql(
            """
SELECT [o].[OrderID] + [o].[OrderID] AS [Value], AVG(CAST([o].[OrderID] AS float)) AS [Average]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [o].[OrderID]
""");
    }

    public override async Task Group_by_with_arithmetic_operation_inside_aggregate(bool async)
    {
        await base.Group_by_with_arithmetic_operation_inside_aggregate(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], COALESCE(SUM([o].[OrderID] + CAST(LEN([o].[CustomerID]) AS int)), 0) AS [Sum]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_scalar_subquery(bool async)
    {
        await base.GroupBy_scalar_subquery(async);

        AssertSql(
            """
SELECT [o0].[Key], COUNT(*) AS [Count]
FROM (
    SELECT (
        SELECT TOP(1) [c].[ContactName]
        FROM [Customers] AS [c]
        WHERE [c].[CustomerID] = [o].[CustomerID]) AS [Key]
    FROM [Orders] AS [o]
) AS [o0]
GROUP BY [o0].[Key]
""");
    }

    public override async Task AsEnumerable_in_subquery_for_GroupBy(bool async)
    {
        await base.AsEnumerable_in_subquery_for_GroupBy(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [s].[OrderID], [s].[CustomerID], [s].[EmployeeID], [s].[OrderDate], [s].[CustomerID0]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT [o3].[OrderID], [o3].[CustomerID], [o3].[EmployeeID], [o3].[OrderDate], [o1].[CustomerID] AS [CustomerID0]
    FROM (
        SELECT [o].[CustomerID]
        FROM [Orders] AS [o]
        WHERE [o].[CustomerID] = [c].[CustomerID]
        GROUP BY [o].[CustomerID]
    ) AS [o1]
    LEFT JOIN (
        SELECT [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate]
        FROM (
            SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], ROW_NUMBER() OVER(PARTITION BY [o0].[CustomerID] ORDER BY [o0].[OrderDate] DESC) AS [row]
            FROM [Orders] AS [o0]
            WHERE [o0].[CustomerID] = [c].[CustomerID]
        ) AS [o2]
        WHERE [o2].[row] <= 1
    ) AS [o3] ON [o1].[CustomerID] = [o3].[CustomerID]
) AS [s]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID], [s].[CustomerID0]
""");
    }

    public override async Task GroupBy_aggregate_from_multiple_query_in_same_projection(bool async)
    {
        await base.GroupBy_aggregate_from_multiple_query_in_same_projection(async);

        AssertSql(
            """
SELECT [t].[CustomerID], [t0].[Key], [t0].[C], [t0].[c0]
FROM (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [t]
OUTER APPLY (
    SELECT TOP(1) [e].[City] AS [Key], COUNT(*) + (
        SELECT COUNT(*)
        FROM [Orders] AS [o0]
        WHERE [t].[CustomerID] = [o0].[CustomerID] OR ([t].[CustomerID] IS NULL AND [o0].[CustomerID] IS NULL)) AS [C], 1 AS [c0]
    FROM [Employees] AS [e]
    WHERE [e].[City] = N'Seattle'
    GROUP BY [e].[City]
    ORDER BY (SELECT 1)
) AS [t0]
""");
    }

    public override async Task GroupBy_aggregate_from_multiple_query_in_same_projection_2(bool async)
    {
        await base.GroupBy_aggregate_from_multiple_query_in_same_projection_2(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], COALESCE((
    SELECT TOP(1) COUNT(*) + MIN([o].[OrderID])
    FROM [Employees] AS [e]
    WHERE [e].[City] = N'Seattle'
    GROUP BY [e].[City]
    ORDER BY (SELECT 1)), 0) AS [A]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_aggregate_from_multiple_query_in_same_projection_3(bool async)
    {
        await base.GroupBy_aggregate_from_multiple_query_in_same_projection_3(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], COALESCE((
    SELECT TOP(1) COUNT(*) + (
        SELECT COUNT(*)
        FROM [Orders] AS [o0]
        WHERE [o].[CustomerID] = [o0].[CustomerID] OR ([o].[CustomerID] IS NULL AND [o0].[CustomerID] IS NULL))
    FROM [Employees] AS [e]
    WHERE [e].[City] = N'Seattle'
    GROUP BY [e].[City]
    ORDER BY COUNT(*) + (
        SELECT COUNT(*)
        FROM [Orders] AS [o0]
        WHERE [o].[CustomerID] = [o0].[CustomerID] OR ([o].[CustomerID] IS NULL AND [o0].[CustomerID] IS NULL))), 0) AS [A]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_scalar_aggregate_in_set_operation(bool async)
    {
        await base.GroupBy_scalar_aggregate_in_set_operation(async);

        AssertSql(
            """
SELECT [c].[CustomerID], 0 AS [Sequence]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
UNION
SELECT [o].[CustomerID], 1 AS [Sequence]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task Select_uncorrelated_collection_with_groupby_when_outer_is_distinct(bool async)
    {
        await base.Select_uncorrelated_collection_with_groupby_when_outer_is_distinct(async);

        AssertSql(
            """
SELECT [s].[City], [p1].[ProductID], [p2].[c], [p2].[ProductID]
FROM (
    SELECT DISTINCT [c].[City]
    FROM [Orders] AS [o]
    LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
    WHERE [o].[CustomerID] LIKE N'A%'
) AS [s]
OUTER APPLY (
    SELECT [p].[ProductID]
    FROM [Products] AS [p]
    GROUP BY [p].[ProductID]
) AS [p1]
OUTER APPLY (
    SELECT COUNT(*) AS [c], [p0].[ProductID]
    FROM [Products] AS [p0]
    GROUP BY [p0].[ProductID]
) AS [p2]
ORDER BY [s].[City], [p1].[ProductID]
""");
    }

    public override async Task Select_correlated_collection_after_GroupBy_aggregate_when_identifier_does_not_change(bool async)
    {
        await base.Select_correlated_collection_after_GroupBy_aggregate_when_identifier_does_not_change(async);

        AssertSql(
            """
SELECT [c0].[CustomerID], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    GROUP BY [c].[CustomerID]
    HAVING [c].[CustomerID] LIKE N'F%'
) AS [c0]
LEFT JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [c0].[CustomerID]
""");
    }

    public override async Task Select_correlated_collection_after_GroupBy_aggregate_when_identifier_changes(bool async)
    {
        await base.Select_correlated_collection_after_GroupBy_aggregate_when_identifier_changes(async);

        AssertSql(
            """
SELECT [o1].[CustomerID], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING [o].[CustomerID] LIKE N'F%'
) AS [o1]
LEFT JOIN [Orders] AS [o0] ON [o1].[CustomerID] = [o0].[CustomerID]
ORDER BY [o1].[CustomerID]
""");
    }

    public override async Task Select_correlated_collection_after_GroupBy_aggregate_when_identifier_changes_to_complex(bool async)
        => await base.Select_correlated_collection_after_GroupBy_aggregate_when_identifier_changes_to_complex(async);

    //AssertSql(" ");
    public override async Task Complex_query_with_group_by_in_subquery5(bool async)
    {
        await base.Complex_query_with_group_by_in_subquery5(async);

        AssertSql(
            """
SELECT [s].[c], [s].[ProductID], [c1].[CustomerID], [c1].[City]
FROM (
    SELECT COALESCE(SUM([o].[ProductID] + [o].[OrderID] * 1000), 0) AS [c], [o].[ProductID], MIN([o].[OrderID] / 100) AS [c0]
    FROM [Order Details] AS [o]
    INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
    LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
    WHERE [c].[CustomerID] = N'ALFKI'
    GROUP BY [o].[ProductID]
) AS [s]
OUTER APPLY (
    SELECT [c0].[CustomerID], [c0].[City]
    FROM [Customers] AS [c0]
    WHERE CAST(LEN([c0].[CustomerID]) AS int) < [s].[c0]
) AS [c1]
ORDER BY [s].[ProductID], [c1].[CustomerID]
""");
    }

    public override async Task Complex_query_with_groupBy_in_subquery4(bool async)
    {
        await base.Complex_query_with_groupBy_in_subquery4(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [s1].[Sum], [s1].[Count], [s1].[Key]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT COALESCE(SUM([s].[OrderID]), 0) AS [Sum], (
        SELECT COUNT(*)
        FROM (
            SELECT [o0].[CustomerID], COALESCE([c1].[City], N'') + COALESCE([o0].[CustomerID], N'') AS [Key]
            FROM [Orders] AS [o0]
            LEFT JOIN [Customers] AS [c1] ON [o0].[CustomerID] = [c1].[CustomerID]
            WHERE [c].[CustomerID] = [o0].[CustomerID]
        ) AS [s0]
        LEFT JOIN [Customers] AS [c2] ON [s0].[CustomerID] = [c2].[CustomerID]
        WHERE ([s].[Key] = [s0].[Key] OR ([s].[Key] IS NULL AND [s0].[Key] IS NULL)) AND COALESCE([c2].[City], N'') + COALESCE([s0].[CustomerID], N'') LIKE N'Lon%') AS [Count], [s].[Key]
    FROM (
        SELECT [o].[OrderID], COALESCE([c0].[City], N'') + COALESCE([o].[CustomerID], N'') AS [Key]
        FROM [Orders] AS [o]
        LEFT JOIN [Customers] AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
        WHERE [c].[CustomerID] = [o].[CustomerID]
    ) AS [s]
    GROUP BY [s].[Key]
) AS [s1]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task GroupBy_aggregate_SelectMany(bool async)
    {
        await base.GroupBy_aggregate_SelectMany(async);

        AssertSql();
    }

    public override async Task Final_GroupBy_property_entity(bool async)
    {
        await base.Final_GroupBy_property_entity(async);

        AssertSql(
            """
SELECT [c].[City], [c].[CustomerID], [c].[Address], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[City]
""");
    }

    public override async Task Final_GroupBy_entity(bool async)
    {
        await base.Final_GroupBy_entity(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] < 10500
ORDER BY [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
""");
    }

    public override async Task Final_GroupBy_property_entity_non_nullable(bool async)
    {
        await base.Final_GroupBy_property_entity_non_nullable(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE [o].[OrderID] < 10500
ORDER BY [o].[OrderID]
""");
    }

    public override async Task Final_GroupBy_property_anonymous_type(bool async)
    {
        await base.Final_GroupBy_property_anonymous_type(async);

        AssertSql(
            """
SELECT [c].[City], [c].[ContactName], [c].[ContactTitle]
FROM [Customers] AS [c]
ORDER BY [c].[City]
""");
    }

    public override async Task Final_GroupBy_multiple_properties_entity(bool async)
    {
        await base.Final_GroupBy_multiple_properties_entity(async);

        AssertSql(
            """
SELECT [c].[City], [c].[Region], [c].[CustomerID], [c].[Address], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode]
FROM [Customers] AS [c]
ORDER BY [c].[City], [c].[Region]
""");
    }

    public override async Task Final_GroupBy_complex_key_entity(bool async)
    {
        await base.Final_GroupBy_complex_key_entity(async);

        AssertSql(
            """
SELECT [c0].[City], [c0].[Region], [c0].[Constant], [c0].[CustomerID], [c0].[Address], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], 1 AS [Constant]
    FROM [Customers] AS [c]
) AS [c0]
ORDER BY [c0].[City], [c0].[Region], [c0].[Constant]
""");
    }

    public override async Task Final_GroupBy_nominal_type_entity(bool async)
    {
        await base.Final_GroupBy_nominal_type_entity(async);

        AssertSql(
            """
SELECT [c0].[City], [c0].[Constant], [c0].[CustomerID], [c0].[Address], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], 1 AS [Constant]
    FROM [Customers] AS [c]
) AS [c0]
ORDER BY [c0].[City], [c0].[Constant]
""");
    }

    public override async Task Final_GroupBy_property_anonymous_type_element_selector(bool async)
    {
        await base.Final_GroupBy_property_anonymous_type_element_selector(async);

        AssertSql(
            """
SELECT [c].[City], [c].[ContactName], [c].[ContactTitle]
FROM [Customers] AS [c]
ORDER BY [c].[City]
""");
    }

    public override async Task Final_GroupBy_property_entity_Include_collection(bool async)
    {
        await base.Final_GroupBy_property_entity_Include_collection(async);

        AssertSql(
            """
SELECT [c].[City], [c].[CustomerID], [c].[Address], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[Country] = N'USA'
ORDER BY [c].[City], [c].[CustomerID]
""");
    }

    public override async Task Final_GroupBy_property_entity_projecting_collection(bool async)
    {
        await base.Final_GroupBy_property_entity_projecting_collection(async);

        AssertSql(
            """
SELECT [c].[City], [c].[CustomerID], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[Country] = N'USA'
ORDER BY [c].[City], [c].[CustomerID]
""");
    }

    public override async Task Final_GroupBy_property_entity_projecting_collection_composed(bool async)
    {
        await base.Final_GroupBy_property_entity_projecting_collection_composed(async);

        AssertSql(
            """
SELECT [c].[City], [c].[CustomerID], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 11000
) AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE [c].[Country] = N'USA'
ORDER BY [c].[City], [c].[CustomerID]
""");
    }

    public override async Task Final_GroupBy_property_entity_projecting_collection_and_single_result(bool async)
    {
        await base.Final_GroupBy_property_entity_projecting_collection_and_single_result(async);

        AssertSql(
            """
SELECT [c].[City], [c].[CustomerID], [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [o3].[OrderID], [o3].[CustomerID], [o3].[EmployeeID], [o3].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 11000
) AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
LEFT JOIN (
    SELECT [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate]
    FROM (
        SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], ROW_NUMBER() OVER(PARTITION BY [o0].[CustomerID] ORDER BY [o0].[OrderDate] DESC) AS [row]
        FROM [Orders] AS [o0]
    ) AS [o2]
    WHERE [o2].[row] <= 1
) AS [o3] ON [c].[CustomerID] = [o3].[CustomerID]
WHERE [c].[Country] = N'USA'
ORDER BY [c].[City], [c].[CustomerID]
""");
    }

    public override async Task GroupBy_Where_with_grouping_result(bool async)
    {
        await base.GroupBy_Where_with_grouping_result(async);

        AssertSql();
    }

    public override async Task GroupBy_OrderBy_with_grouping_result(bool async)
    {
        await base.GroupBy_OrderBy_with_grouping_result(async);

        AssertSql();
    }

    public override async Task GroupBy_SelectMany(bool async)
    {
        await base.GroupBy_SelectMany(async);

        AssertSql();
    }

    public override async Task OrderBy_GroupBy_SelectMany(bool async)
    {
        await base.OrderBy_GroupBy_SelectMany(async);

        AssertSql();
    }

    public override async Task OrderBy_GroupBy_SelectMany_shadow(bool async)
    {
        await base.OrderBy_GroupBy_SelectMany_shadow(async);

        AssertSql();
    }

    public override async Task GroupBy_with_orderby_take_skip_distinct_followed_by_group_key_projection(bool async)
    {
        await base.GroupBy_with_orderby_take_skip_distinct_followed_by_group_key_projection(async);

        AssertSql();
    }

    public override async Task GroupBy_Distinct(bool async)
    {
        await base.GroupBy_Distinct(async);

        AssertSql();
    }

    public override async Task GroupBy_complex_key_without_aggregate(bool async)
    {
        await base.GroupBy_complex_key_without_aggregate(async);

        AssertSql(
            """
SELECT [s1].[Key], [s3].[OrderID], [s3].[CustomerID], [s3].[EmployeeID], [s3].[OrderDate], [s3].[CustomerID0]
FROM (
    SELECT [s].[Key]
    FROM (
        SELECT SUBSTRING([c].[CustomerID], 0 + 1, 1) AS [Key]
        FROM [Orders] AS [o]
        LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
    ) AS [s]
    GROUP BY [s].[Key]
) AS [s1]
LEFT JOIN (
    SELECT [s2].[OrderID], [s2].[CustomerID], [s2].[EmployeeID], [s2].[OrderDate], [s2].[CustomerID0], [s2].[Key]
    FROM (
        SELECT [s0].[OrderID], [s0].[CustomerID], [s0].[EmployeeID], [s0].[OrderDate], [s0].[CustomerID0], [s0].[Key], ROW_NUMBER() OVER(PARTITION BY [s0].[Key] ORDER BY [s0].[OrderID], [s0].[CustomerID0]) AS [row]
        FROM (
            SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c0].[CustomerID] AS [CustomerID0], SUBSTRING([c0].[CustomerID], 0 + 1, 1) AS [Key]
            FROM [Orders] AS [o0]
            LEFT JOIN [Customers] AS [c0] ON [o0].[CustomerID] = [c0].[CustomerID]
        ) AS [s0]
    ) AS [s2]
    WHERE 1 < [s2].[row] AND [s2].[row] <= 3
) AS [s3] ON [s1].[Key] = [s3].[Key]
ORDER BY [s1].[Key], [s3].[OrderID]
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
