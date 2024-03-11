// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindSelectQuerySqlServerTest : NorthwindSelectQueryRelationalTestBase<
    NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
{
    public NorthwindSelectQuerySqlServerTest(
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

    public override async Task Projection_when_arithmetic_expression_precedence(bool async)
    {
        await base.Projection_when_arithmetic_expression_precedence(async);

        AssertSql(
            """
SELECT [o].[OrderID] / ([o].[OrderID] / 2) AS [A], ([o].[OrderID] / [o].[OrderID]) / 2 AS [B]
FROM [Orders] AS [o]
""");
    }

    public override async Task Projection_when_arithmetic_expressions(bool async)
    {
        await base.Projection_when_arithmetic_expressions(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[OrderID] * 2 AS [Double], [o].[OrderID] + 23 AS [Add], 100000 - [o].[OrderID] AS [Sub], [o].[OrderID] / ([o].[OrderID] / 2) AS [Divide], 42 AS [Literal], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
""");
    }

    public override async Task Projection_when_arithmetic_mixed(bool async)
    {
        await base.Projection_when_arithmetic_mixed(async);

        AssertSql(
            """
@__p_0='10'

SELECT CAST([e0].[EmployeeID] AS bigint) + CAST([o0].[OrderID] AS bigint) AS [Add], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], 42 AS [Literal], [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [o0]
CROSS JOIN (
    SELECT TOP(5) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
    ORDER BY [e].[EmployeeID]
) AS [e0]
ORDER BY [o0].[OrderID]
""");
    }

    public override async Task Projection_when_null_value(bool async)
    {
        await base.Projection_when_null_value(async);

        AssertSql(
            """
SELECT [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task Projection_when_client_evald_subquery(bool async)
    {
        await base.Projection_when_client_evald_subquery(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o].[CustomerID], [o].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Project_to_object_array(bool async)
    {
        await base.Project_to_object_array(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = 1
""");
    }

    public override async Task Projection_of_entity_type_into_object_array(bool async)
    {
        await base.Projection_of_entity_type_into_object_array(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Projection_of_multiple_entity_types_into_object_array(bool async)
    {
        await base.Projection_of_multiple_entity_types_into_object_array(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] < 10300
ORDER BY [o].[OrderID]
""");
    }

    public override async Task Projection_of_entity_type_into_object_list(bool async)
    {
        await base.Projection_of_entity_type_into_object_list(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Project_to_int_array(bool async)
    {
        await base.Project_to_int_array(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[ReportsTo]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = 1
""");
    }

    public override async Task Select_bool_closure_with_order_parameter_with_cast_to_nullable(bool async)
    {
        await base.Select_bool_closure_with_order_parameter_with_cast_to_nullable(async);

        AssertSql(
            """
@__boolean_0='False'

SELECT @__boolean_0
FROM [Customers] AS [c]
""");
    }

    public override async Task Select_scalar(bool async)
    {
        await base.Select_scalar(async);

        AssertSql(
            """
SELECT [c].[City]
FROM [Customers] AS [c]
""");
    }

    public override async Task Select_anonymous_one(bool async)
    {
        await base.Select_anonymous_one(async);

        AssertSql(
            """
SELECT [c].[City]
FROM [Customers] AS [c]
""");
    }

    public override async Task Select_anonymous_two(bool async)
    {
        await base.Select_anonymous_two(async);

        AssertSql(
            """
SELECT [c].[City], [c].[Phone]
FROM [Customers] AS [c]
""");
    }

    public override async Task Select_anonymous_three(bool async)
    {
        await base.Select_anonymous_three(async);

        AssertSql(
            """
SELECT [c].[City], [c].[Phone], [c].[Country]
FROM [Customers] AS [c]
""");
    }

    public override async Task Select_anonymous_bool_constant_true(bool async)
    {
        await base.Select_anonymous_bool_constant_true(async);

        AssertSql(
            """
SELECT [c].[CustomerID], CAST(1 AS bit) AS [ConstantTrue]
FROM [Customers] AS [c]
""");
    }

    public override async Task Select_anonymous_constant_in_expression(bool async)
    {
        await base.Select_anonymous_constant_in_expression(async);

        AssertSql(
            """
SELECT [c].[CustomerID], CAST(LEN([c].[CustomerID]) AS int) + 5 AS [Expression]
FROM [Customers] AS [c]
""");
    }

    public override async Task Select_anonymous_conditional_expression(bool async)
    {
        await base.Select_anonymous_conditional_expression(async);

        AssertSql(
            """
SELECT [p].[ProductID], CASE
    WHEN [p].[UnitsInStock] > CAST(0 AS smallint) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsAvailable]
FROM [Products] AS [p]
""");
    }

    public override async Task Select_constant_int(bool async)
    {
        await base.Select_constant_int(async);

        AssertSql(
            """
SELECT 0
FROM [Customers] AS [c]
""");
    }

    public override async Task Select_constant_null_string(bool async)
    {
        await base.Select_constant_null_string(async);

        AssertSql(
            """
SELECT NULL
FROM [Customers] AS [c]
""");
    }

    public override async Task Select_local(bool async)
    {
        await base.Select_local(async);

        AssertSql(
            """
@__x_0='10'

SELECT @__x_0
FROM [Customers] AS [c]
""");
    }

    public override async Task Select_scalar_primitive_after_take(bool async)
    {
        await base.Select_scalar_primitive_after_take(async);

        AssertSql(
            """
@__p_0='9'

SELECT TOP(@__p_0) [e].[EmployeeID]
FROM [Employees] AS [e]
""");
    }

    public override async Task Select_project_filter(bool async)
    {
        await base.Select_project_filter(async);

        AssertSql(
            """
SELECT [c].[CompanyName]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
""");
    }

    public override async Task Select_project_filter2(bool async)
    {
        await base.Select_project_filter2(async);

        AssertSql(
            """
SELECT [c].[City]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
""");
    }

    public override async Task Select_nested_collection(bool async)
    {
        await base.Select_nested_collection(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o0].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE DATEPART(year, [o].[OrderDate]) = 1997
) AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE [c].[City] = N'London'
ORDER BY [c].[CustomerID], [o0].[OrderID]
""");
    }

    public override async Task Select_nested_collection_multi_level(bool async)
    {
        await base.Select_nested_collection_multi_level(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o1].[Date], [o1].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o0].[Date], [o0].[OrderID], [o0].[CustomerID]
    FROM (
        SELECT [o].[OrderDate] AS [Date], [o].[OrderID], [o].[CustomerID], ROW_NUMBER() OVER(PARTITION BY [o].[CustomerID] ORDER BY [o].[OrderID]) AS [row]
        FROM [Orders] AS [o]
        WHERE [o].[OrderID] < 10500
    ) AS [o0]
    WHERE [o0].[row] <= 3
) AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID], [o1].[CustomerID], [o1].[OrderID]
""");
    }

    public override async Task Select_nested_collection_multi_level2(bool async)
    {
        await base.Select_nested_collection_multi_level2(async);

        AssertSql(
            """
SELECT (
    SELECT TOP(1) [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID] AND [o].[OrderID] < 10500
    ORDER BY [o].[OrderID]) AS [OrderDates]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Select_nested_collection_multi_level3(bool async)
    {
        await base.Select_nested_collection_multi_level3(async);

        AssertSql(
            """
SELECT (
    SELECT TOP(1) [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10500 AND [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]) AS [OrderDates]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Select_nested_collection_multi_level4(bool async)
    {
        await base.Select_nested_collection_multi_level4(async);

        AssertSql(
            """
SELECT COALESCE((
    SELECT TOP(1) (
        SELECT COUNT(*)
        FROM [Order Details] AS [o0]
        WHERE [o].[OrderID] = [o0].[OrderID] AND [o0].[OrderID] > 10)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID] AND [o].[OrderID] < 10500
    ORDER BY [o].[OrderID]), 0) AS [Order]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Select_nested_collection_multi_level5(bool async)
    {
        await base.Select_nested_collection_multi_level5(async);

        AssertSql(
            """
SELECT COALESCE((
    SELECT TOP(1) COALESCE((
        SELECT TOP(1) [o0].[ProductID]
        FROM [Order Details] AS [o0]
        WHERE [o].[OrderID] = [o0].[OrderID] AND ([o0].[OrderID] <> (
            SELECT COUNT(*)
            FROM [Orders] AS [o1]
            WHERE [c].[CustomerID] = [o1].[CustomerID]) OR (
            SELECT COUNT(*)
            FROM [Orders] AS [o1]
            WHERE [c].[CustomerID] = [o1].[CustomerID]) IS NULL)
        ORDER BY [o0].[OrderID], [o0].[ProductID]), 0)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID] AND [o].[OrderID] < 10500
    ORDER BY [o].[OrderID]), 0) AS [Order]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Select_nested_collection_multi_level6(bool async)
    {
        await base.Select_nested_collection_multi_level6(async);

        AssertSql(
            """
SELECT COALESCE((
    SELECT TOP(1) COALESCE((
        SELECT TOP(1) [o0].[ProductID]
        FROM [Order Details] AS [o0]
        WHERE [o].[OrderID] = [o0].[OrderID] AND [o0].[OrderID] <> CAST(LEN([c].[CustomerID]) AS int)
        ORDER BY [o0].[OrderID], [o0].[ProductID]), 0)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID] AND [o].[OrderID] < 10500
    ORDER BY [o].[OrderID]), 0) AS [Order]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Select_nested_collection_count_using_anonymous_type(bool async)
    {
        await base.Select_nested_collection_count_using_anonymous_type(async);

        AssertSql(
            """
SELECT (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]) AS [Count]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
""");
    }

    public override async Task New_date_time_in_anonymous_type_works(bool async)
    {
        await base.New_date_time_in_anonymous_type_works(async);

        AssertSql(
            """
SELECT 1
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
""");
    }

    public override async Task Select_non_matching_value_types_int_to_long_introduces_explicit_cast(bool async)
    {
        await base.Select_non_matching_value_types_int_to_long_introduces_explicit_cast(async);

        AssertSql(
            """
SELECT CAST([o].[OrderID] AS bigint)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]
""");
    }

    public override async Task Select_non_matching_value_types_nullable_int_to_long_introduces_explicit_cast(bool async)
    {
        await base.Select_non_matching_value_types_nullable_int_to_long_introduces_explicit_cast(async);

        AssertSql(
            """
SELECT CAST([o].[EmployeeID] AS bigint)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]
""");
    }

    public override async Task Select_non_matching_value_types_nullable_int_to_int_doesnt_introduce_explicit_cast(bool async)
    {
        await base.Select_non_matching_value_types_nullable_int_to_int_doesnt_introduce_explicit_cast(async);

        AssertSql(
            """
SELECT [o].[EmployeeID]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]
""");
    }

    public override async Task Select_non_matching_value_types_int_to_nullable_int_doesnt_introduce_explicit_cast(bool async)
    {
        await base.Select_non_matching_value_types_int_to_nullable_int_doesnt_introduce_explicit_cast(async);

        AssertSql(
            """
SELECT [o].[OrderID]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]
""");
    }

    public override async Task Select_non_matching_value_types_from_binary_expression_introduces_explicit_cast(bool async)
    {
        await base.Select_non_matching_value_types_from_binary_expression_introduces_explicit_cast(async);

        AssertSql(
            """
SELECT CAST([o].[OrderID] + [o].[OrderID] AS bigint)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]
""");
    }

    public override async Task Select_non_matching_value_types_from_binary_expression_nested_introduces_top_level_explicit_cast(
        bool async)
    {
        await base.Select_non_matching_value_types_from_binary_expression_nested_introduces_top_level_explicit_cast(async);

        AssertSql(
            """
SELECT CAST(CAST([o].[OrderID] AS bigint) + CAST([o].[OrderID] AS bigint) AS smallint)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]
""");
    }

    public override async Task Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast1(bool async)
    {
        await base.Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast1(async);

        AssertSql(
            """
SELECT CAST(-[o].[OrderID] AS bigint)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]
""");
    }

    public override async Task Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast2(bool async)
    {
        await base.Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast2(async);

        AssertSql(
            """
SELECT -CAST([o].[OrderID] AS bigint)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]
""");
    }

    public override async Task Select_non_matching_value_types_from_length_introduces_explicit_cast(bool async)
    {
        await base.Select_non_matching_value_types_from_length_introduces_explicit_cast(async);

        AssertSql(
            """
SELECT CAST(CAST(LEN([o].[CustomerID]) AS int) AS bigint)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]
""");
    }

    public override async Task Select_non_matching_value_types_from_method_call_introduces_explicit_cast(bool async)
    {
        await base.Select_non_matching_value_types_from_method_call_introduces_explicit_cast(async);

        AssertSql(
            """
SELECT CAST(ABS([o].[OrderID]) AS bigint)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]
""");
    }

    public override async Task Select_non_matching_value_types_from_anonymous_type_introduces_explicit_cast(bool async)
    {
        await base.Select_non_matching_value_types_from_anonymous_type_introduces_explicit_cast(async);

        AssertSql(
            """
SELECT CAST([o].[OrderID] AS bigint) AS [LongOrder], CAST([o].[OrderID] AS smallint) AS [ShortOrder], [o].[OrderID] AS [Order]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]
""");
    }

    public override async Task Select_conditional_with_null_comparison_in_test(bool async)
    {
        await base.Select_conditional_with_null_comparison_in_test(async);

        AssertSql(
            """
SELECT CASE
    WHEN [o].[CustomerID] IS NULL THEN CAST(1 AS bit)
    ELSE CASE
        WHEN [o].[OrderID] < 100 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
END
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task Select_over_10_nested_ternary_condition(bool isAsync)
    {
        await base.Select_over_10_nested_ternary_condition(isAsync);

        AssertSql(
            """
SELECT CASE
    WHEN [c].[CustomerID] = N'1' THEN N'01'
    WHEN [c].[CustomerID] = N'2' THEN N'02'
    WHEN [c].[CustomerID] = N'3' THEN N'03'
    WHEN [c].[CustomerID] = N'4' THEN N'04'
    WHEN [c].[CustomerID] = N'5' THEN N'05'
    WHEN [c].[CustomerID] = N'6' THEN N'06'
    WHEN [c].[CustomerID] = N'7' THEN N'07'
    WHEN [c].[CustomerID] = N'8' THEN N'08'
    WHEN [c].[CustomerID] = N'9' THEN N'09'
    WHEN [c].[CustomerID] = N'10' THEN N'10'
    WHEN [c].[CustomerID] = N'11' THEN N'11'
    ELSE NULL
END
FROM [Customers] AS [c]
""");
    }

    public override async Task Projection_in_a_subquery_should_be_liftable(bool async)
    {
        await base.Projection_in_a_subquery_should_be_liftable(async);

        AssertSql(
            """
@__p_0='1'

SELECT [e].[EmployeeID]
FROM [Employees] AS [e]
ORDER BY [e].[EmployeeID]
OFFSET @__p_0 ROWS
""");
    }

    public override async Task Projection_containing_DateTime_subtraction(bool async)
    {
        await base.Projection_containing_DateTime_subtraction(async);

        AssertSql(
            """
SELECT [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300
""");
    }

    public override async Task Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault(bool async)
    {
        await base.Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault(async);

        AssertSql(
            """
SELECT (
    SELECT TOP(1) [o0].[CustomerID]
    FROM (
        SELECT TOP(1) [o].[CustomerID], [o].[OrderID]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderID]
    ) AS [o0]
    ORDER BY [o0].[OrderID])
FROM [Customers] AS [c]
""");
    }

    public override async Task Project_single_element_from_collection_with_OrderBy_Skip_and_FirstOrDefault(bool async)
    {
        await base.Project_single_element_from_collection_with_OrderBy_Skip_and_FirstOrDefault(async);

        AssertSql(
            """
SELECT (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]
    OFFSET 1 ROWS FETCH NEXT 1 ROWS ONLY)
FROM [Customers] AS [c]
""");
    }

    public override async Task Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault(bool async)
    {
        await base.Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault(async);

        AssertSql(
            """
SELECT (
    SELECT DISTINCT TOP(1) [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
FROM [Customers] AS [c]
""");
    }

    public override async Task
        Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault_followed_by_projecting_length(bool async)
    {
        await base.Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault_followed_by_projecting_length(
            async);

        AssertSql(
            """
SELECT (
    SELECT TOP(1) CAST(LEN([o0].[CustomerID]) AS int)
    FROM (
        SELECT DISTINCT [o].[CustomerID]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
    ) AS [o0])
FROM [Customers] AS [c]
""");
    }

    public override async Task Project_single_element_from_collection_with_OrderBy_Take_and_SingleOrDefault(bool async)
    {
        await base.Project_single_element_from_collection_with_OrderBy_Take_and_SingleOrDefault(async);

        AssertSql(
            """
SELECT (
    SELECT TOP(1) [o0].[CustomerID]
    FROM (
        SELECT TOP(1) [o].[CustomerID], [o].[OrderID]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderID]
    ) AS [o0]
    ORDER BY [o0].[OrderID])
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault_with_parameter(bool async)
    {
        await base.Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault_with_parameter(async);

        AssertSql(
            """
@__i_0='1'

SELECT (
    SELECT TOP(1) [o0].[CustomerID]
    FROM (
        SELECT TOP(@__i_0) [o].[CustomerID], [o].[OrderID]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderID]
    ) AS [o0]
    ORDER BY [o0].[OrderID])
FROM [Customers] AS [c]
""");
    }

    public override async Task Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault(bool async)
    {
        await base.Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault(async);

        AssertSql(
            """
SELECT (
    SELECT TOP(1) [o0].[CustomerID]
    FROM (
        SELECT TOP(2) [o].[CustomerID], [o].[OrderID], [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderID], [o].[OrderDate] DESC
    ) AS [o0]
    ORDER BY [o0].[OrderID], [o0].[OrderDate] DESC)
FROM [Customers] AS [c]
""");
    }

    public override async Task
        Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_followed_by_projection_of_length_property(
            bool async)
    {
        await base
            .Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_followed_by_projection_of_length_property(
                async);

        AssertSql(
            """
SELECT (
    SELECT TOP(1) [o0].[c]
    FROM (
        SELECT TOP(2) CAST(LEN([o].[CustomerID]) AS int) AS [c], [o].[OrderID], [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderID], [o].[OrderDate] DESC
    ) AS [o0]
    ORDER BY [o0].[OrderID], [o0].[OrderDate] DESC)
FROM [Customers] AS [c]
""");
    }

    public override async Task Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_2(bool async)
    {
        await base.Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_2(async);

        AssertSql(
            """
SELECT (
    SELECT TOP(1) [o0].[CustomerID]
    FROM (
        SELECT TOP(2) [o].[CustomerID], [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[CustomerID], [o].[OrderDate] DESC
    ) AS [o0]
    ORDER BY [o0].[CustomerID], [o0].[OrderDate] DESC)
FROM [Customers] AS [c]
""");
    }

    public override async Task Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault(bool async)
    {
        await base.Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault(async);

        AssertSql(
            """
SELECT COALESCE((
    SELECT TOP(1) [s].[OrderID]
    FROM (
        SELECT TOP(1) [o0].[OrderID], [p].[ProductName]
        FROM [Order Details] AS [o0]
        INNER JOIN [Products] AS [p] ON [o0].[ProductID] = [p].[ProductID]
        WHERE [o].[OrderID] = [o0].[OrderID]
        ORDER BY [p].[ProductName]
    ) AS [s]
    ORDER BY [s].[ProductName]), 0)
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300
""");
    }

    public override async Task Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(
        bool async)
    {
        await base.Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(async);

        AssertSql(
            """
SELECT [s0].[OrderID], [s0].[ProductID], [s0].[Discount], [s0].[Quantity], [s0].[UnitPrice]
FROM [Orders] AS [o]
OUTER APPLY (
    SELECT TOP(1) [s].[OrderID], [s].[ProductID], [s].[Discount], [s].[Quantity], [s].[UnitPrice]
    FROM (
        SELECT TOP(1) [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [p].[ProductName]
        FROM [Order Details] AS [o0]
        INNER JOIN [Products] AS [p] ON [o0].[ProductID] = [p].[ProductID]
        WHERE [o].[OrderID] = [o0].[OrderID]
        ORDER BY [p].[ProductName]
    ) AS [s]
    ORDER BY [s].[ProductName]
) AS [s0]
WHERE [o].[OrderID] < 10250
""");
    }

    public override async Task Select_datetime_year_component(bool async)
    {
        await base.Select_datetime_year_component(async);

        AssertSql(
            """
SELECT DATEPART(year, [o].[OrderDate])
FROM [Orders] AS [o]
""");
    }

    public override async Task Select_datetime_month_component(bool async)
    {
        await base.Select_datetime_month_component(async);

        AssertSql(
            """
SELECT DATEPART(month, [o].[OrderDate])
FROM [Orders] AS [o]
""");
    }

    public override async Task Select_datetime_day_of_year_component(bool async)
    {
        await base.Select_datetime_day_of_year_component(async);

        AssertSql(
            """
SELECT DATEPART(dayofyear, [o].[OrderDate])
FROM [Orders] AS [o]
""");
    }

    public override async Task Select_datetime_day_component(bool async)
    {
        await base.Select_datetime_day_component(async);

        AssertSql(
            """
SELECT DATEPART(day, [o].[OrderDate])
FROM [Orders] AS [o]
""");
    }

    public override async Task Select_datetime_hour_component(bool async)
    {
        await base.Select_datetime_hour_component(async);

        AssertSql(
            """
SELECT DATEPART(hour, [o].[OrderDate])
FROM [Orders] AS [o]
""");
    }

    public override async Task Select_datetime_minute_component(bool async)
    {
        await base.Select_datetime_minute_component(async);

        AssertSql(
            """
SELECT DATEPART(minute, [o].[OrderDate])
FROM [Orders] AS [o]
""");
    }

    public override async Task Select_datetime_second_component(bool async)
    {
        await base.Select_datetime_second_component(async);

        AssertSql(
            """
SELECT DATEPART(second, [o].[OrderDate])
FROM [Orders] AS [o]
""");
    }

    public override async Task Select_datetime_millisecond_component(bool async)
    {
        await base.Select_datetime_millisecond_component(async);

        AssertSql(
            """
SELECT DATEPART(millisecond, [o].[OrderDate])
FROM [Orders] AS [o]
""");
    }

    public override async Task Select_byte_constant(bool async)
    {
        await base.Select_byte_constant(async);

        AssertSql(
            """
SELECT CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS tinyint)
    ELSE CAST(2 AS tinyint)
END
FROM [Customers] AS [c]
""");
    }

    public override async Task Select_short_constant(bool async)
    {
        await base.Select_short_constant(async);

        AssertSql(
            """
SELECT CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS smallint)
    ELSE CAST(2 AS smallint)
END
FROM [Customers] AS [c]
""");
    }

    public override async Task Select_bool_constant(bool async)
    {
        await base.Select_bool_constant(async);

        AssertSql(
            """
SELECT CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Customers] AS [c]
""");
    }

    public override async Task Anonymous_projection_AsNoTracking_Selector(bool async)
    {
        await base.Anonymous_projection_AsNoTracking_Selector(async);

        AssertSql(
            """
SELECT [o].[OrderDate]
FROM [Orders] AS [o]
""");
    }

    public override async Task Anonymous_projection_with_repeated_property_being_ordered(bool async)
    {
        await base.Anonymous_projection_with_repeated_property_being_ordered(async);

        AssertSql(
            """
SELECT [c].[CustomerID] AS [A]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Anonymous_projection_with_repeated_property_being_ordered_2(bool async)
    {
        await base.Anonymous_projection_with_repeated_property_being_ordered_2(async);

        AssertSql(
            """
SELECT [c].[CustomerID] AS [A], [o].[CustomerID] AS [B]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [o].[CustomerID]
""");
    }

    public override async Task Select_GetValueOrDefault_on_DateTime(bool async)
    {
        await base.Select_GetValueOrDefault_on_DateTime(async);

        AssertSql(
            """
SELECT [o].[OrderDate]
FROM [Orders] AS [o]
""");
    }

    public override async Task Select_GetValueOrDefault_on_DateTime_with_null_values(bool async)
    {
        await base.Select_GetValueOrDefault_on_DateTime_with_null_values(async);

        AssertSql(
            """
SELECT [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
""");
    }

    public override async Task Cast_on_top_level_projection_brings_explicit_Cast(bool async)
    {
        await base.Cast_on_top_level_projection_brings_explicit_Cast(async);

        AssertSql(
            """
SELECT CAST([o].[OrderID] AS float)
FROM [Orders] AS [o]
""");
    }

    public override async Task Projecting_nullable_struct(bool async)
    {
        await base.Projecting_nullable_struct(async);

        AssertSql(
            """
SELECT [o].[CustomerID], CASE
    WHEN [o].[CustomerID] = N'ALFKI' AND [o].[CustomerID] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [o].[OrderID], CAST(LEN([o].[CustomerID]) AS int)
FROM [Orders] AS [o]
""");
    }

    public override async Task Multiple_select_many_with_predicate(bool async)
    {
        await base.Multiple_select_many_with_predicate(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
INNER JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE CAST([o0].[Discount] AS float) >= 0.25E0
""");
    }

    public override async Task SelectMany_without_result_selector_naked_collection_navigation(bool async)
    {
        await base.SelectMany_without_result_selector_naked_collection_navigation(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
""");
    }

    public override async Task SelectMany_without_result_selector_collection_navigation_composed(bool async)
    {
        await base.SelectMany_without_result_selector_collection_navigation_composed(async);

        AssertSql(
            """
SELECT [o].[CustomerID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
""");
    }

    public override async Task SelectMany_correlated_with_outer_1(bool async)
    {
        await base.SelectMany_correlated_with_outer_1(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o0].[City] AS [o]
FROM [Customers] AS [c]
CROSS APPLY (
    SELECT [c].[City]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [o0]
""");
    }

    public override async Task SelectMany_correlated_with_outer_2(bool async)
    {
        await base.SelectMany_correlated_with_outer_2(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Customers] AS [c]
CROSS APPLY (
    SELECT TOP(2) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [c].[City], [o].[OrderID]
) AS [o0]
""");
    }

    public override async Task SelectMany_correlated_with_outer_3(bool async)
    {
        await base.SelectMany_correlated_with_outer_3(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o0].[City] AS [o]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT [c].[City]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [o0]
""");
    }

    public override async Task SelectMany_correlated_with_outer_4(bool async)
    {
        await base.SelectMany_correlated_with_outer_4(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT TOP(2) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [c].[City], [o].[OrderID]
) AS [o0]
""");
    }

    public override async Task SelectMany_correlated_with_outer_5(bool async)
    {
        await base.SelectMany_correlated_with_outer_5(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o0].[City] AS [o]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT [c].[City]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] <> [o].[CustomerID] OR [o].[CustomerID] IS NULL
) AS [o0]
""");
    }

    public override async Task SelectMany_correlated_with_outer_6(bool async)
    {
        await base.SelectMany_correlated_with_outer_6(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT TOP(2) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] <> [o].[CustomerID] OR [o].[CustomerID] IS NULL
    ORDER BY [c].[City], [o].[OrderID]
) AS [o0]
""");
    }

    public override async Task SelectMany_correlated_with_outer_7(bool async)
    {
        await base.SelectMany_correlated_with_outer_7(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT TOP(2) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE CAST(LEN([c].[CustomerID]) AS int) >= CAST(LEN([o].[CustomerID]) AS int)
    ORDER BY [c].[City], [o].[OrderID]
) AS [o0]
""");
    }

    public override async Task FirstOrDefault_over_empty_collection_of_value_type_returns_correct_results(bool async)
    {
        await base.FirstOrDefault_over_empty_collection_of_value_type_returns_correct_results(async);

        AssertSql(
            """
SELECT [c].[CustomerID], COALESCE((
    SELECT TOP(1) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]), 0) AS [OrderId]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'FISSA'
""");
    }

    public override async Task Project_non_nullable_value_after_FirstOrDefault_on_empty_collection(bool async)
    {
        await base.Project_non_nullable_value_after_FirstOrDefault_on_empty_collection(async);

        AssertSql(
            """
SELECT (
    SELECT TOP(1) CAST(LEN([o].[CustomerID]) AS int)
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = N'John Doe')
FROM [Customers] AS [c]
""");
    }

    public override Task Member_binding_after_ctor_arguments_fails_with_client_eval(bool async)
        => AssertTranslationFailed(() => base.Member_binding_after_ctor_arguments_fails_with_client_eval(async));

    public override async Task Filtered_collection_projection_is_tracked(bool async)
    {
        await base.Filtered_collection_projection_is_tracked(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] > 11000
) AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Filtered_collection_projection_with_to_list_is_tracked(bool async)
    {
        await base.Filtered_collection_projection_with_to_list_is_tracked(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] > 11000
) AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task SelectMany_with_collection_being_correlated_subquery_which_references_inner_and_outer_entity(
        bool async)
    {
        await base.SelectMany_with_collection_being_correlated_subquery_which_references_inner_and_outer_entity(async);

        AssertSql(
            """
SELECT [o0].[OrderProperty], [o0].[CustomerProperty]
FROM [Customers] AS [c]
CROSS APPLY (
    SELECT [o].[CustomerID] AS [OrderProperty], [c].[CustomerID] AS [CustomerProperty]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [o0]
""");
    }

    public override async Task
        SelectMany_with_collection_being_correlated_subquery_which_references_non_mapped_properties_from_inner_and_outer_entity(
            bool async)
    {
        await AssertUnableToTranslateEFProperty(
            () => base
                .SelectMany_with_collection_being_correlated_subquery_which_references_non_mapped_properties_from_inner_and_outer_entity(
                    async));

        AssertSql();
    }

    public override async Task Select_with_complex_expression_that_can_be_funcletized(bool async)
    {
        await base.Select_with_complex_expression_that_can_be_funcletized(async);

        AssertSql(
            """
SELECT 0
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task Select_chained_entity_navigation_doesnt_materialize_intermittent_entities(bool async)
    {
        await base.Select_chained_entity_navigation_doesnt_materialize_intermittent_entities(async);

        AssertSql(
            """
SELECT [o].[OrderID], [c].[CustomerID], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
LEFT JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
ORDER BY [o].[OrderID], [c].[CustomerID]
""");
    }

    public override async Task Select_entity_compared_to_null(bool async)
    {
        await base.Select_entity_compared_to_null(async);

        AssertSql(
            """
SELECT CASE
    WHEN [c].[CustomerID] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task Explicit_cast_in_arithmetic_operation_is_preserved(bool async)
    {
        await base.Explicit_cast_in_arithmetic_operation_is_preserved(async);

        AssertSql(
            """
SELECT CAST([o].[OrderID] AS decimal(18,2)) / CAST([o].[OrderID] + 1000 AS decimal(18,2))
FROM [Orders] AS [o]
WHERE [o].[OrderID] = 10250
""");
    }

    public override async Task SelectMany_whose_selector_references_outer_source(bool async)
    {
        await base.SelectMany_whose_selector_references_outer_source(async);

        AssertSql(
            """
SELECT [o0].[OrderDate], [o0].[CustomerCity]
FROM [Customers] AS [c]
CROSS APPLY (
    SELECT [o].[OrderDate], [c].[City] AS [CustomerCity]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [o0]
""");
    }

    public override async Task Collection_FirstOrDefault_with_entity_equality_check_in_projection(bool async)
    {
        await base.Collection_FirstOrDefault_with_entity_equality_check_in_projection(async);

        AssertSql(
            """
SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]) OR NOT EXISTS (
        SELECT 1
        FROM [Orders] AS [o0]
        WHERE [c].[CustomerID] = [o0].[CustomerID]) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Customers] AS [c]
""");
    }

    public override async Task Collection_FirstOrDefault_with_nullable_unsigned_int_column(bool async)
    {
        await base.Collection_FirstOrDefault_with_nullable_unsigned_int_column(async);

        AssertSql(
            """
SELECT (
    SELECT TOP(1) [o].[EmployeeID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID])
FROM [Customers] AS [c]
""");
    }

    public override async Task ToList_Count_in_projection_works(bool async)
    {
        await base.ToList_Count_in_projection_works(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]) AS [Count]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
""");
    }

    public override async Task LastOrDefault_member_access_in_projection_translates_to_server(bool async)
    {
        await base.LastOrDefault_member_access_in_projection_translates_to_server(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], (
    SELECT TOP(1) [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]) AS [OrderDate]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
""");
    }

    public override async Task Projection_with_parameterized_constructor(bool async)
    {
        await base.Projection_with_parameterized_constructor(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task Projection_with_parameterized_constructor_with_member_assignment(bool async)
    {
        await base.Projection_with_parameterized_constructor_with_member_assignment(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task Collection_projection_AsNoTracking_OrderBy(bool async)
    {
        await base.Collection_projection_AsNoTracking_OrderBy(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Coalesce_over_nullable_uint(bool async)
    {
        await base.Coalesce_over_nullable_uint(async);

        AssertSql(
            """
SELECT COALESCE([o].[EmployeeID], 0)
FROM [Orders] AS [o]
""");
    }

    public override async Task Project_uint_through_collection_FirstOrDefault(bool async)
    {
        await base.Project_uint_through_collection_FirstOrDefault(async);

        AssertSql(
            """
SELECT (
    SELECT TOP(1) [o].[EmployeeID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID])
FROM [Customers] AS [c]
""");
    }

    public override async Task Project_keyless_entity_FirstOrDefault_without_orderby(bool async)
    {
        await base.Project_keyless_entity_FirstOrDefault_without_orderby(async);

        AssertSql(
            """
SELECT [m1].[Address], [m1].[City], [m1].[CompanyName], [m1].[ContactName], [m1].[ContactTitle]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [m0].[Address], [m0].[City], [m0].[CompanyName], [m0].[ContactName], [m0].[ContactTitle]
    FROM (
        SELECT [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], ROW_NUMBER() OVER(PARTITION BY [m].[CompanyName] ORDER BY (SELECT 1)) AS [row]
        FROM (
            SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]
        ) AS [m]
    ) AS [m0]
    WHERE [m0].[row] <= 1
) AS [m1] ON [c].[CompanyName] = [m1].[CompanyName]
""");
    }

    public override async Task Reverse_changes_asc_order_to_desc(bool async)
    {
        await base.Reverse_changes_asc_order_to_desc(async);

        AssertSql(
            """
SELECT [e].[EmployeeID]
FROM [Employees] AS [e]
ORDER BY [e].[EmployeeID] DESC
""");
    }

    public override async Task Reverse_changes_desc_order_to_asc(bool async)
    {
        await base.Reverse_changes_desc_order_to_asc(async);

        AssertSql(
            """
SELECT [e].[EmployeeID]
FROM [Employees] AS [e]
ORDER BY [e].[EmployeeID]
""");
    }

    public override async Task Reverse_after_multiple_orderbys(bool async)
    {
        await base.Reverse_after_multiple_orderbys(async);

        AssertSql(
            """
SELECT [e].[EmployeeID]
FROM [Employees] AS [e]
ORDER BY [e].[EmployeeID]
""");
    }

    public override async Task Reverse_after_orderby_thenby(bool async)
    {
        await base.Reverse_after_orderby_thenby(async);

        AssertSql(
            """
SELECT [e].[EmployeeID]
FROM [Employees] AS [e]
ORDER BY [e].[EmployeeID] DESC, [e].[City]
""");
    }

    public override async Task Reverse_in_subquery_via_pushdown(bool async)
    {
        await base.Reverse_in_subquery_via_pushdown(async);

        AssertSql(
            """
@__p_0='5'

SELECT [e1].[EmployeeID], [e1].[City]
FROM (
    SELECT DISTINCT [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title]
    FROM (
        SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
        FROM [Employees] AS [e]
        ORDER BY [e].[EmployeeID] DESC
    ) AS [e0]
) AS [e1]
""");
    }

    public override async Task Reverse_after_orderBy_and_take(bool async)
    {
        await base.Reverse_after_orderBy_and_take(async);

        AssertSql(
            """
@__p_0='5'

SELECT [e0].[EmployeeID], [e0].[City]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City]
    FROM [Employees] AS [e]
    ORDER BY [e].[EmployeeID]
) AS [e0]
ORDER BY [e0].[EmployeeID] DESC
""");
    }

    public override async Task Reverse_in_join_outer(bool async)
    {
        await base.Reverse_in_join_outer(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[City], [c].[CustomerID] DESC
""");
    }

    public override async Task Reverse_in_join_outer_with_take(bool async)
    {
        await base.Reverse_in_join_outer_with_take(async);

        AssertSql(
            """
@__p_0='20'

SELECT [c0].[CustomerID], [o].[OrderID]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c0]
INNER JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [c0].[CustomerID]
""");
    }

    public override async Task Reverse_in_join_inner(bool async)
    {
        await base.Reverse_in_join_inner(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Reverse_in_join_inner_with_skip(bool async)
    {
        await base.Reverse_in_join_inner_with_skip(async);

        AssertSql(
            """
@__p_0='2'

SELECT [c].[CustomerID], [o0].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID] DESC
    OFFSET @__p_0 ROWS
) AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Reverse_in_SelectMany(bool async)
    {
        await base.Reverse_in_SelectMany(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID] DESC
""");
    }

    public override async Task Reverse_in_SelectMany_with_Take(bool async)
    {
        await base.Reverse_in_SelectMany_with_Take(async);

        AssertSql(
            """
@__p_0='20'

SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID] DESC
) AS [c0]
CROSS APPLY (
    SELECT TOP(30) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c0].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID] DESC
) AS [o0]
ORDER BY [c0].[CustomerID] DESC
""");
    }

    public override async Task Reverse_in_projection_subquery(bool async)
    {
        await base.Reverse_in_projection_subquery(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
OUTER APPLY [Orders] AS [o]
ORDER BY [c].[CustomerID], [o].[OrderDate] DESC, [o].[OrderID]
""");
    }

    public override async Task Reverse_in_projection_subquery_single_result(bool async)
    {
        await base.Reverse_in_projection_subquery_single_result(async);

        AssertSql(
            """
SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderDate] DESC, [o].[OrderID]
) AS [o0]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Reverse_in_projection_scalar_subquery(bool async)
    {
        await base.Reverse_in_projection_scalar_subquery(async);

        AssertSql(
            """
SELECT COALESCE((
    SELECT TOP(1) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderDate] DESC, [o].[OrderID]), 0)
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Projection_AsEnumerable_projection(bool async)
    {
        await base.Projection_AsEnumerable_projection(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] < 10750
) AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
WHERE [c].[CustomerID] LIKE N'A%' AND (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = [c].[CustomerID] AND [o].[OrderID] < 11000) > 0
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Projection_custom_type_in_both_sides_of_ternary(bool async)
    {
        await base.Projection_custom_type_in_both_sides_of_ternary(async);

        AssertSql(
            """
SELECT CASE
    WHEN [c].[City] = N'Seattle' AND [c].[City] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Projecting_multiple_collection_with_same_constant_works(bool async)
    {
        await base.Projecting_multiple_collection_with_same_constant_works(async);

        AssertSql(
            """
SELECT [c].[CustomerID], 1, [o].[OrderID], [o0].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
LEFT JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID], [o].[OrderID]
""");
    }

    public override async Task Custom_projection_reference_navigation_PK_to_FK_optimization(bool async)
    {
        await base.Custom_projection_reference_navigation_PK_to_FK_optimization(async);

        AssertSql(
            """
SELECT [o].[OrderID], [c].[CustomerID], [c].[City], [o].[OrderDate]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
""");
    }

    public override async Task Projecting_Length_of_a_string_property_after_FirstOrDefault_on_correlated_collection(bool async)
    {
        await base.Projecting_Length_of_a_string_property_after_FirstOrDefault_on_correlated_collection(async);

        AssertSql(
            """
SELECT (
    SELECT TOP(1) CAST(LEN([o].[CustomerID]) AS int)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID])
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Projecting_count_of_navigation_which_is_generic_list(bool async)
    {
        await base.Projecting_count_of_navigation_which_is_generic_list(async);

        AssertSql(
            """
SELECT (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Projecting_count_of_navigation_which_is_generic_collection(bool async)
    {
        await base.Projecting_count_of_navigation_which_is_generic_collection(async);

        AssertSql(
            """
SELECT (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Projecting_count_of_navigation_which_is_generic_collection_using_convert(bool async)
    {
        await base.Projecting_count_of_navigation_which_is_generic_collection_using_convert(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Projection_take_projection_doesnt_project_intermittent_column(bool async)
    {
        await base.Projection_take_projection_doesnt_project_intermittent_column(async);

        AssertSql(
            """
@__p_0='10'

SELECT TOP(@__p_0) [c].[CustomerID] + N' ' + COALESCE([c].[City], N'') AS [Aggregate]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Projection_skip_projection_doesnt_project_intermittent_column(bool async)
    {
        await base.Projection_skip_projection_doesnt_project_intermittent_column(async);

        AssertSql(
            """
@__p_0='7'

SELECT [c].[CustomerID] + N' ' + COALESCE([c].[City], N'') AS [Aggregate]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
OFFSET @__p_0 ROWS
""");
    }

    public override async Task Projection_Distinct_projection_preserves_columns_used_for_distinct_in_subquery(bool async)
    {
        await base.Projection_Distinct_projection_preserves_columns_used_for_distinct_in_subquery(async);

        AssertSql(
            """
SELECT COALESCE([c0].[FirstLetter], N'') + N' ' + [c0].[Foo] AS [Aggregate]
FROM (
    SELECT DISTINCT [c].[CustomerID], SUBSTRING([c].[CustomerID], 0 + 1, 1) AS [FirstLetter], N'Foo' AS [Foo]
    FROM [Customers] AS [c]
) AS [c0]
""");
    }

    public override async Task Projection_take_predicate_projection(bool async)
    {
        await base.Projection_take_predicate_projection(async);

        AssertSql(
            """
@__p_0='10'

SELECT [c0].[CustomerID] + N' ' + COALESCE([c0].[City], N'') AS [Aggregate]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[City]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c0]
WHERE [c0].[CustomerID] LIKE N'A%'
ORDER BY [c0].[CustomerID]
""");
    }

    public override async Task Do_not_erase_projection_mapping_when_adding_single_projection(bool async)
    {
        await base.Do_not_erase_projection_mapping_when_adding_single_projection(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [s].[OrderID], [s].[ProductID], [s].[Discount], [s].[Quantity], [s].[UnitPrice], [s].[ProductID0], [s].[Discontinued], [s].[ProductName], [s].[SupplierID], [s].[UnitPrice0], [s].[UnitsInStock], [s1].[OrderID], [s1].[ProductID], [s1].[ProductID0], [s2].[OrderID], [s2].[ProductID], [s2].[Discount], [s2].[Quantity], [s2].[UnitPrice], [s2].[ProductID0], [s2].[Discontinued], [s2].[ProductName], [s2].[SupplierID], [s2].[UnitPrice0], [s2].[UnitsInStock], [s1].[Discount], [s1].[Quantity], [s1].[UnitPrice], [s1].[Discontinued], [s1].[ProductName], [s1].[SupplierID], [s1].[UnitPrice0], [s1].[UnitsInStock]
FROM [Orders] AS [o]
LEFT JOIN (
    SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [p].[ProductID] AS [ProductID0], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice] AS [UnitPrice0], [p].[UnitsInStock]
    FROM [Order Details] AS [o0]
    INNER JOIN [Products] AS [p] ON [o0].[ProductID] = [p].[ProductID]
) AS [s] ON [o].[OrderID] = [s].[OrderID]
LEFT JOIN (
    SELECT [s0].[OrderID], [s0].[ProductID], [s0].[Discount], [s0].[Quantity], [s0].[UnitPrice], [s0].[ProductID0], [s0].[Discontinued], [s0].[ProductName], [s0].[SupplierID], [s0].[UnitPrice0], [s0].[UnitsInStock]
    FROM (
        SELECT [o1].[OrderID], [o1].[ProductID], [o1].[Discount], [o1].[Quantity], [o1].[UnitPrice], [p0].[ProductID] AS [ProductID0], [p0].[Discontinued], [p0].[ProductName], [p0].[SupplierID], [p0].[UnitPrice] AS [UnitPrice0], [p0].[UnitsInStock], ROW_NUMBER() OVER(PARTITION BY [o1].[OrderID] ORDER BY [o1].[OrderID], [o1].[ProductID], [p0].[ProductID]) AS [row]
        FROM [Order Details] AS [o1]
        INNER JOIN [Products] AS [p0] ON [o1].[ProductID] = [p0].[ProductID]
        WHERE [o1].[UnitPrice] > 10.0
    ) AS [s0]
    WHERE [s0].[row] <= 1
) AS [s1] ON [o].[OrderID] = [s1].[OrderID]
LEFT JOIN (
    SELECT [o2].[OrderID], [o2].[ProductID], [o2].[Discount], [o2].[Quantity], [o2].[UnitPrice], [p1].[ProductID] AS [ProductID0], [p1].[Discontinued], [p1].[ProductName], [p1].[SupplierID], [p1].[UnitPrice] AS [UnitPrice0], [p1].[UnitsInStock]
    FROM [Order Details] AS [o2]
    INNER JOIN [Products] AS [p1] ON [o2].[ProductID] = [p1].[ProductID]
    WHERE [o2].[UnitPrice] < 10.0
) AS [s2] ON [o].[OrderID] = [s2].[OrderID]
WHERE [o].[OrderID] < 10350
ORDER BY [o].[OrderID], [s].[OrderID], [s].[ProductID], [s].[ProductID0], [s1].[OrderID], [s1].[ProductID], [s1].[ProductID0], [s2].[OrderID], [s2].[ProductID]
""");
    }

    public override async Task Ternary_in_client_eval_assigns_correct_types(bool async)
    {
        await base.Ternary_in_client_eval_assigns_correct_types(async);

        AssertSql(
            """
SELECT [o].[CustomerID], CASE
    WHEN [o].[OrderDate] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [o].[OrderDate], [o].[OrderID] - 10000, CASE
    WHEN [o].[OrderDate] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300
ORDER BY [o].[OrderID]
""");
    }

    public override async Task Projecting_after_navigation_and_distinct(bool async)
    {
        await base.Projecting_after_navigation_and_distinct(async);

        AssertSql(
            """
@__filteredOrderIds_0='[10248,10249,10250]' (Size = 4000)

SELECT [s].[CustomerID], [o1].[CustomerID], [o1].[OrderID], [o1].[OrderDate]
FROM (
    SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Orders] AS [o]
    LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
) AS [s]
OUTER APPLY (
    SELECT [s].[CustomerID], [o0].[OrderID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    WHERE [s].[CustomerID] IS NOT NULL AND [s].[CustomerID] = [o0].[CustomerID] AND [o0].[OrderID] IN (
        SELECT [f].[value]
        FROM OPENJSON(@__filteredOrderIds_0) WITH ([value] int '$') AS [f]
    )
) AS [o1]
ORDER BY [s].[CustomerID], [o1].[OrderID]
""");
    }

    public override async Task Correlated_collection_after_distinct_with_complex_projection_containing_original_identifier(bool async)
    {
        await base.Correlated_collection_after_distinct_with_complex_projection_containing_original_identifier(async);

        AssertSql(
            """
@__filteredOrderIds_0='[10248,10249,10250]' (Size = 4000)

SELECT [o0].[OrderID], [o0].[Complex], [o2].[Outer], [o2].[Inner], [o2].[OrderDate]
FROM (
    SELECT DISTINCT [o].[OrderID], DATEPART(month, [o].[OrderDate]) AS [Complex]
    FROM [Orders] AS [o]
) AS [o0]
OUTER APPLY (
    SELECT [o0].[OrderID] AS [Outer], [o1].[OrderID] AS [Inner], [o1].[OrderDate]
    FROM [Orders] AS [o1]
    WHERE [o1].[OrderID] = [o0].[OrderID] AND [o1].[OrderID] IN (
        SELECT [f].[value]
        FROM OPENJSON(@__filteredOrderIds_0) WITH ([value] int '$') AS [f]
    )
) AS [o2]
ORDER BY [o0].[OrderID]
""");
    }

    public override async Task Correlated_collection_after_distinct_not_containing_original_identifier(bool async)
    {
        await base.Correlated_collection_after_distinct_not_containing_original_identifier(async);

        AssertSql(
            """
@__filteredOrderIds_0='[10248,10249,10250]' (Size = 4000)

SELECT [o0].[OrderDate], [o0].[CustomerID], [o2].[Outer1], [o2].[Outer2], [o2].[Inner], [o2].[OrderDate]
FROM (
    SELECT DISTINCT [o].[OrderDate], [o].[CustomerID]
    FROM [Orders] AS [o]
) AS [o0]
OUTER APPLY (
    SELECT [o0].[OrderDate] AS [Outer1], [o0].[CustomerID] AS [Outer2], [o1].[OrderID] AS [Inner], [o1].[OrderDate]
    FROM [Orders] AS [o1]
    WHERE ([o1].[CustomerID] = [o0].[CustomerID] OR ([o1].[CustomerID] IS NULL AND [o0].[CustomerID] IS NULL)) AND [o1].[OrderID] IN (
        SELECT [f].[value]
        FROM OPENJSON(@__filteredOrderIds_0) WITH ([value] int '$') AS [f]
    )
) AS [o2]
ORDER BY [o0].[OrderDate], [o0].[CustomerID]
""");
    }

    public override async Task Correlated_collection_after_distinct_with_complex_projection_not_containing_original_identifier(bool async)
    {
        // Identifier set for Distinct. Issue #24440.
        Assert.Equal(
            RelationalStrings.InsufficientInformationToIdentifyElementOfCollectionJoin,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collection_after_distinct_with_complex_projection_not_containing_original_identifier(async)))
            .Message);

        AssertSql();
    }

    public override async Task Correlated_collection_after_groupby_with_complex_projection_containing_original_identifier(bool async)
    {
        await base.Correlated_collection_after_groupby_with_complex_projection_containing_original_identifier(async);

        AssertSql(
            """
@__filteredOrderIds_0='[10248,10249,10250]' (Size = 4000)

SELECT [o2].[OrderID], [o2].[Complex], [o3].[Outer], [o3].[Inner], [o3].[OrderDate]
FROM (
    SELECT [o0].[OrderID], [o0].[Complex]
    FROM (
        SELECT [o].[OrderID], DATEPART(month, [o].[OrderDate]) AS [Complex]
        FROM [Orders] AS [o]
    ) AS [o0]
    GROUP BY [o0].[OrderID], [o0].[Complex]
) AS [o2]
OUTER APPLY (
    SELECT [o2].[OrderID] AS [Outer], [o1].[OrderID] AS [Inner], [o1].[OrderDate]
    FROM [Orders] AS [o1]
    WHERE [o1].[OrderID] = [o2].[OrderID] AND [o1].[OrderID] IN (
        SELECT [f].[value]
        FROM OPENJSON(@__filteredOrderIds_0) WITH ([value] int '$') AS [f]
    )
) AS [o3]
ORDER BY [o2].[OrderID]
""");
    }

    public override async Task Select_nested_collection_deep(bool async)
    {
        await base.Select_nested_collection_deep(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [s].[OrderID], [s].[OrderID0], [s].[OrderID00]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT [o].[OrderID], [o1].[OrderID] AS [OrderID0], [o1].[OrderID0] AS [OrderID00]
    FROM [Orders] AS [o]
    OUTER APPLY (
        SELECT [o].[OrderID], [o0].[OrderID] AS [OrderID0]
        FROM [Orders] AS [o0]
        WHERE [o].[CustomerID] = [c].[CustomerID]
    ) AS [o1]
    WHERE [o].[CustomerID] = [c].[CustomerID] AND DATEPART(year, [o].[OrderDate]) = 1997
) AS [s]
WHERE [c].[City] = N'London'
ORDER BY [c].[CustomerID], [s].[OrderID], [s].[OrderID00]
""");
    }

    public override async Task Select_nested_collection_deep_distinct_no_identifiers(bool async)
    {
        await base.Select_nested_collection_deep_distinct_no_identifiers(async);

        AssertSql(
            """
SELECT [c0].[City], [s].[OrderID], [s].[OrderID0], [s].[OrderID00]
FROM (
    SELECT DISTINCT [c].[City]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'London'
) AS [c0]
OUTER APPLY (
    SELECT [o0].[OrderID], [o2].[OrderID] AS [OrderID0], [o2].[OrderID0] AS [OrderID00]
    FROM (
        SELECT DISTINCT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE ([o].[CustomerID] = [c0].[City] OR ([o].[CustomerID] IS NULL AND [c0].[City] IS NULL)) AND DATEPART(year, [o].[OrderDate]) = 1997
    ) AS [o0]
    OUTER APPLY (
        SELECT [o0].[OrderID], [o1].[OrderID] AS [OrderID0]
        FROM [Orders] AS [o1]
        WHERE [o0].[CustomerID] = [c0].[City] OR ([o0].[CustomerID] IS NULL AND [c0].[City] IS NULL)
    ) AS [o2]
) AS [s]
ORDER BY [c0].[City], [s].[OrderID], [s].[OrderID00]
""");
    }

    public override async Task Collection_include_over_result_of_single_non_scalar(bool async)
    {
        await base.Collection_include_over_result_of_single_non_scalar(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [s].[OrderID], [s].[CustomerID], [s].[EmployeeID], [s].[OrderDate], [s].[OrderID0], [s].[ProductID], [s].[Discount], [s].[Quantity], [s].[UnitPrice], [o4].[OrderID], [o4].[CustomerID], [o4].[EmployeeID], [o4].[OrderDate], [o2].[OrderID], [o2].[ProductID], [o2].[Discount], [o2].[Quantity], [o2].[UnitPrice]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o0].[OrderID] AS [OrderID0], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
    FROM [Orders] AS [o]
    LEFT JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
) AS [s] ON [c].[CustomerID] = [s].[CustomerID]
LEFT JOIN (
    SELECT [o3].[OrderID], [o3].[CustomerID], [o3].[EmployeeID], [o3].[OrderDate]
    FROM (
        SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], ROW_NUMBER() OVER(PARTITION BY [o1].[CustomerID] ORDER BY [o1].[OrderDate]) AS [row]
        FROM [Orders] AS [o1]
    ) AS [o3]
    WHERE [o3].[row] <= 1
) AS [o4] ON [c].[CustomerID] = [o4].[CustomerID]
LEFT JOIN [Order Details] AS [o2] ON [o4].[OrderID] = [o2].[OrderID]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID], [s].[OrderID], [s].[OrderID0], [s].[ProductID], [o4].[OrderID], [o2].[OrderID]
""");
    }

    public override async Task Collection_projection_selecting_outer_element_followed_by_take(bool async)
    {
        await base.Collection_projection_selecting_outer_element_followed_by_take(async);

        AssertSql(
            """
@__p_0='10'

SELECT [c0].[CustomerID], [s].[CustomerID], [s].[Address], [s].[City], [s].[CompanyName], [s].[ContactName], [s].[ContactTitle], [s].[Country], [s].[Fax], [s].[Phone], [s].[PostalCode], [s].[Region], [s].[OrderID], [s].[OrderID0], [s].[CustomerID0], [s].[EmployeeID], [s].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'F%'
    ORDER BY [c].[CustomerID]
) AS [c0]
OUTER APPLY (
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region], [o].[OrderID], [o1].[OrderID] AS [OrderID0], [o1].[CustomerID] AS [CustomerID0], [o1].[EmployeeID], [o1].[OrderDate]
    FROM [Orders] AS [o]
    OUTER APPLY (
        SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
        FROM [Orders] AS [o0]
        WHERE [c0].[CustomerID] = [o0].[CustomerID]
    ) AS [o1]
    WHERE [c0].[CustomerID] = [o].[CustomerID]
) AS [s]
ORDER BY [c0].[CustomerID], [s].[OrderID]
""");
    }

    public override async Task Take_on_top_level_and_on_collection_projection_with_outer_apply(bool async)
    {
        await base.Take_on_top_level_and_on_collection_projection_with_outer_apply(async);

        AssertSql(
            """
SELECT [o2].[OrderID], [o2].[OrderDate], [s].[OrderID], [s].[ProductID], [s].[Discontinued], [s].[ProductName], [s].[SupplierID], [s].[UnitPrice], [s].[UnitsInStock], [s].[UnitPrice0], [s].[ProductID0]
FROM (
    SELECT TOP(1) [o].[OrderID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] LIKE N'F%'
) AS [o2]
OUTER APPLY (
    SELECT [o1].[OrderID], [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock], [o1].[UnitPrice] AS [UnitPrice0], [o1].[ProductID] AS [ProductID0]
    FROM (
        SELECT [o0].[OrderID], [o0].[ProductID], [o0].[UnitPrice]
        FROM [Order Details] AS [o0]
        WHERE [o2].[OrderID] = [o0].[OrderID]
        ORDER BY [o0].[OrderID] DESC
        OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY
    ) AS [o1]
    INNER JOIN [Products] AS [p] ON [o1].[ProductID] = [p].[ProductID]
) AS [s]
ORDER BY [o2].[OrderID], [s].[OrderID] DESC, [s].[ProductID0]
""");
    }

    public override async Task Take_on_correlated_collection_in_first(bool async)
    {
        await base.Take_on_correlated_collection_in_first(async);

        AssertSql(
            """
SELECT [c1].[CustomerID], [s].[Title], [s].[OrderID], [s].[CustomerID]
FROM (
    SELECT TOP(1) [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'F%'
    ORDER BY [c].[CustomerID]
) AS [c1]
OUTER APPLY (
    SELECT CASE
        WHEN [o0].[CustomerID] = [c0].[CustomerID] OR ([o0].[CustomerID] IS NULL AND [c0].[CustomerID] IS NULL) THEN N'A'
        ELSE N'B'
    END AS [Title], [o0].[OrderID], [c0].[CustomerID], [o0].[OrderDate]
    FROM (
        SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [c1].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderDate]
    ) AS [o0]
    LEFT JOIN [Customers] AS [c0] ON [o0].[CustomerID] = [c0].[CustomerID]
) AS [s]
ORDER BY [c1].[CustomerID], [s].[OrderDate], [s].[OrderID]
""");
    }

    public override async Task Client_projection_via_ctor_arguments(bool async)
    {
        await base.Client_projection_via_ctor_arguments(async);

        AssertSql(
            """
SELECT [c0].[CustomerID], [c0].[City], [o].[OrderID], [o].[OrderDate], [c0].[c]
FROM (
    SELECT TOP(2) [c].[CustomerID], [c].[City], (
        SELECT COUNT(*)
        FROM [Orders] AS [o0]
        WHERE [c].[CustomerID] = [o0].[CustomerID]) AS [c]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'ALFKI'
) AS [c0]
LEFT JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [c0].[CustomerID]
""");
    }

    public override async Task Client_projection_with_string_initialization_with_scalar_subquery(bool async)
    {
        await base.Client_projection_with_string_initialization_with_scalar_subquery(async);

        AssertSql(
            """
SELECT [c].[CustomerID], (
    SELECT TOP(1) [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID] AND [o].[OrderID] < 11000), [c].[City], N'test' + COALESCE([c].[City], N'')
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task MemberInit_in_projection_without_arguments(bool async)
    {
        await base.MemberInit_in_projection_without_arguments(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task VisitLambda_should_not_be_visited_trivially(bool async)
    {
        await base.VisitLambda_should_not_be_visited_trivially(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] LIKE N'A%'
""");
    }

    public override async Task Select_anonymous_literal(bool async)
    {
        await base.Select_anonymous_literal(async);

        AssertSql(
            """
SELECT 10 AS [X]
FROM [Customers] AS [c]
""");
    }

    public override async Task Select_anonymous_nested(bool async)
    {
        await base.Select_anonymous_nested(async);

        AssertSql(
            """
SELECT [c].[City], [c].[Country]
FROM [Customers] AS [c]
""");
    }

    public override async Task Projection_when_arithmetic_mixed_subqueries(bool async)
    {
        await base.Projection_when_arithmetic_mixed_subqueries(async);

        AssertSql(
            """
@__p_0='3'

SELECT CAST([e0].[EmployeeID] AS bigint) + CAST([o0].[OrderID] AS bigint), [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [o0].[OrderID] % 2
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [o0]
CROSS JOIN (
    SELECT TOP(2) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
    ORDER BY [e].[EmployeeID]
) AS [e0]
ORDER BY [o0].[OrderID]
""");
    }

    public override async Task Select_datetime_Ticks_component(bool async)
    {
        await base.Select_datetime_Ticks_component(async);

        AssertSql(
            """
SELECT [o].[OrderDate]
FROM [Orders] AS [o]
""");
    }

    public override async Task Select_datetime_TimeOfDay_component(bool async)
    {
        await base.Select_datetime_TimeOfDay_component(async);

        AssertSql(
            """
SELECT CONVERT(time, [o].[OrderDate])
FROM [Orders] AS [o]
""");
    }

    public override async Task Select_anonymous_with_object(bool async)
    {
        await base.Select_anonymous_with_object(async);

        AssertSql(
            """
SELECT [c].[City], [c].[CustomerID], [c].[Address], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task Client_method_in_projection_requiring_materialization_1(bool async)
    {
        await base.Client_method_in_projection_requiring_materialization_1(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
""");
    }

    public override async Task Select_datetime_DayOfWeek_component(bool async)
    {
        await base.Select_datetime_DayOfWeek_component(async);

        AssertSql(
            """
SELECT [o].[OrderDate]
FROM [Orders] AS [o]
""");
    }

    public override async Task Select_scalar_primitive(bool async)
    {
        await base.Select_scalar_primitive(async);

        AssertSql(
            """
SELECT [e].[EmployeeID]
FROM [Employees] AS [e]
""");
    }

    public override async Task Client_method_in_projection_requiring_materialization_2(bool async)
    {
        await base.Client_method_in_projection_requiring_materialization_2(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
""");
    }

    public override async Task Select_anonymous_empty(bool async)
    {
        await base.Select_anonymous_empty(async);

        AssertSql(
            """
SELECT 1
FROM [Customers] AS [c]
""");
    }

    public override async Task Select_customer_table(bool async)
    {
        await base.Select_customer_table(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task Select_into(bool async)
    {
        await base.Select_into(async);

        AssertSql(
            """
SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task Select_bool_closure(bool async)
    {
        await base.Select_bool_closure(async);

        AssertSql(
            """
SELECT 1
FROM [Customers] AS [c]
""",
            //
            """
SELECT 1
FROM [Customers] AS [c]
""");
    }

    public override async Task Select_customer_identity(bool async)
    {
        await base.Select_customer_identity(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task Correlated_collection_after_groupby_with_complex_projection_not_containing_original_identifier(bool async)
    {
        await base.Correlated_collection_after_groupby_with_complex_projection_not_containing_original_identifier(async);

        AssertSql(
            """
@__filteredOrderIds_0='[10248,10249,10250]' (Size = 4000)

SELECT [o2].[CustomerID], [o2].[Complex], [o3].[Outer], [o3].[Inner], [o3].[OrderDate]
FROM (
    SELECT [o0].[CustomerID], [o0].[Complex]
    FROM (
        SELECT [o].[CustomerID], DATEPART(month, [o].[OrderDate]) AS [Complex]
        FROM [Orders] AS [o]
    ) AS [o0]
    GROUP BY [o0].[CustomerID], [o0].[Complex]
) AS [o2]
OUTER APPLY (
    SELECT [o2].[CustomerID] AS [Outer], [o1].[OrderID] AS [Inner], [o1].[OrderDate]
    FROM [Orders] AS [o1]
    WHERE ([o1].[CustomerID] = [o2].[CustomerID] OR ([o1].[CustomerID] IS NULL AND [o2].[CustomerID] IS NULL)) AND [o1].[OrderID] IN (
        SELECT [f].[value]
        FROM OPENJSON(@__filteredOrderIds_0) WITH ([value] int '$') AS [f]
    )
) AS [o3]
ORDER BY [o2].[CustomerID], [o2].[Complex]
""");
    }

    public override async Task Select_bool_closure_with_order_by_property_with_cast_to_nullable(bool async)
    {
        await base.Select_bool_closure_with_order_by_property_with_cast_to_nullable(async);

        AssertSql();
    }

    public override async Task Reverse_without_explicit_ordering(bool async)
    {
        await base.Reverse_without_explicit_ordering(async);

        AssertSql();
    }

    public override async Task List_of_list_of_anonymous_type(bool async)
    {
        await base.List_of_list_of_anonymous_type(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [s].[OrderID], [s].[OrderID0], [s].[ProductID]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[OrderID], [o0].[OrderID] AS [OrderID0], [o0].[ProductID], [o].[CustomerID]
    FROM [Orders] AS [o]
    LEFT JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
) AS [s] ON [c].[CustomerID] = [s].[CustomerID]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID], [s].[OrderID], [s].[OrderID0]
""");
    }

    public override async Task List_from_result_of_single_result(bool async)
    {
        await base.List_from_result_of_single_result(async);

        AssertSql(
            """
SELECT [c0].[CustomerID], [o].[OrderID]
FROM (
    SELECT TOP(1) [c].[CustomerID]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c0]
LEFT JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [c0].[CustomerID]
""");
    }

    public override async Task List_from_result_of_single_result_2(bool async)
    {
        await base.List_from_result_of_single_result_2(async);

        AssertSql(
            """
SELECT [c0].[CustomerID], [o].[OrderID], [o].[OrderDate]
FROM (
    SELECT TOP(1) [c].[CustomerID]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c0]
LEFT JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [c0].[CustomerID]
""");
    }

    public override async Task List_from_result_of_single_result_3(bool async)
    {
        await base.List_from_result_of_single_result_3(async);

        AssertSql(
            """
SELECT [c0].[CustomerID], [o2].[OrderID], [o0].[ProductID], [o0].[OrderID], [o2].[c]
FROM (
    SELECT TOP(1) [c].[CustomerID]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c0]
LEFT JOIN (
    SELECT [o1].[c], [o1].[OrderID], [o1].[CustomerID]
    FROM (
        SELECT 1 AS [c], [o].[OrderID], [o].[CustomerID], ROW_NUMBER() OVER(PARTITION BY [o].[CustomerID] ORDER BY [o].[OrderDate]) AS [row]
        FROM [Orders] AS [o]
    ) AS [o1]
    WHERE [o1].[row] <= 1
) AS [o2] ON [c0].[CustomerID] = [o2].[CustomerID]
LEFT JOIN [Order Details] AS [o0] ON [o2].[OrderID] = [o0].[OrderID]
ORDER BY [c0].[CustomerID], [o2].[OrderID], [o0].[OrderID]
""");
    }

    public override async Task Using_enumerable_parameter_in_projection(bool async)
    {
        await base.Using_enumerable_parameter_in_projection(async);

        AssertSql(
            """
SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Entity_passed_to_DTO_constructor_works(bool async)
    {
        await base.Entity_passed_to_DTO_constructor_works(async);

        AssertSql(
"""
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task Set_operation_in_pending_collection(bool async)
    {
        await base.Set_operation_in_pending_collection(async);

        AssertSql(
            """
@__p_0='5'

SELECT [c0].[CustomerID], [u].[OrderID]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c0]
OUTER APPLY (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = [c0].[CustomerID]
    UNION
    SELECT [o0].[OrderID]
    FROM [Orders] AS [o0]
    WHERE [o0].[CustomerID] = [c0].[CustomerID]
) AS [u]
ORDER BY [c0].[CustomerID]
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
