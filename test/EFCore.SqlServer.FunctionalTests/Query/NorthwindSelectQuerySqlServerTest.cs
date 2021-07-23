// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindSelectQuerySqlServerTest : NorthwindSelectQueryRelationalTestBase<
        NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        public NorthwindSelectQuerySqlServerTest(
            NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override bool CanExecuteQueryString
            => true;

        public override async Task Projection_when_arithmetic_expression_precedence(bool async)
        {
            await base.Projection_when_arithmetic_expression_precedence(async);

            AssertSql(
                @"SELECT [o].[OrderID] / ([o].[OrderID] / 2) AS [A], ([o].[OrderID] / [o].[OrderID]) / 2 AS [B]
FROM [Orders] AS [o]");
        }

        public override async Task Projection_when_arithmetic_expressions(bool async)
        {
            await base.Projection_when_arithmetic_expressions(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[OrderID] * 2 AS [Double], [o].[OrderID] + 23 AS [Add], 100000 - [o].[OrderID] AS [Sub], [o].[OrderID] / ([o].[OrderID] / 2) AS [Divide], 42 AS [Literal], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override async Task Projection_when_arithmetic_mixed(bool async)
        {
            await base.Projection_when_arithmetic_mixed(async);

            AssertSql(
                @"@__p_0='10'

SELECT CAST([t0].[EmployeeID] AS bigint) + CAST([t].[OrderID] AS bigint) AS [Add], [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], 42 AS [Literal], [t0].[EmployeeID], [t0].[City], [t0].[Country], [t0].[FirstName], [t0].[ReportsTo], [t0].[Title]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]
CROSS JOIN (
    SELECT TOP(5) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
    ORDER BY [e].[EmployeeID]
) AS [t0]
ORDER BY [t].[OrderID]");
        }

        public override async Task Projection_when_null_value(bool async)
        {
            await base.Projection_when_null_value(async);

            AssertSql(
                @"SELECT [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Projection_when_client_evald_subquery(bool async)
        {
            await base.Projection_when_client_evald_subquery(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [o].[CustomerID], [o].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID], [o].[OrderID]");
        }

        public override async Task Project_to_object_array(bool async)
        {
            await base.Project_to_object_array(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = 1");
        }

        public override async Task Projection_of_entity_type_into_object_array(bool async)
        {
            await base.Projection_of_entity_type_into_object_array(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID]");
        }

        public override async Task Projection_of_multiple_entity_types_into_object_array(bool async)
        {
            await base.Projection_of_multiple_entity_types_into_object_array(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] < 10300
ORDER BY [o].[OrderID]");
        }

        public override async Task Projection_of_entity_type_into_object_list(bool async)
        {
            await base.Projection_of_entity_type_into_object_list(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override async Task Project_to_int_array(bool async)
        {
            await base.Project_to_int_array(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[ReportsTo]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = 1");
        }

        public override async Task Select_bool_closure_with_order_parameter_with_cast_to_nullable(bool async)
        {
            await base.Select_bool_closure_with_order_parameter_with_cast_to_nullable(async);

            AssertSql(
                @"@__boolean_0='False'

SELECT @__boolean_0
FROM [Customers] AS [c]");
        }

        public override async Task Select_scalar(bool async)
        {
            await base.Select_scalar(async);

            AssertSql(
                @"SELECT [c].[City]
FROM [Customers] AS [c]");
        }

        public override async Task Select_anonymous_one(bool async)
        {
            await base.Select_anonymous_one(async);

            AssertSql(
                @"SELECT [c].[City]
FROM [Customers] AS [c]");
        }

        public override async Task Select_anonymous_two(bool async)
        {
            await base.Select_anonymous_two(async);

            AssertSql(
                @"SELECT [c].[City], [c].[Phone]
FROM [Customers] AS [c]");
        }

        public override async Task Select_anonymous_three(bool async)
        {
            await base.Select_anonymous_three(async);

            AssertSql(
                @"SELECT [c].[City], [c].[Phone], [c].[Country]
FROM [Customers] AS [c]");
        }

        public override async Task Select_anonymous_bool_constant_true(bool async)
        {
            await base.Select_anonymous_bool_constant_true(async);

            AssertSql(
                @"SELECT [c].[CustomerID], CAST(1 AS bit) AS [ConstantTrue]
FROM [Customers] AS [c]");
        }

        public override async Task Select_anonymous_constant_in_expression(bool async)
        {
            await base.Select_anonymous_constant_in_expression(async);

            AssertSql(
                @"SELECT [c].[CustomerID], CAST(LEN([c].[CustomerID]) AS int) + 5 AS [Expression]
FROM [Customers] AS [c]");
        }

        public override async Task Select_anonymous_conditional_expression(bool async)
        {
            await base.Select_anonymous_conditional_expression(async);

            AssertSql(
                @"SELECT [p].[ProductID], CASE
    WHEN [p].[UnitsInStock] > CAST(0 AS smallint) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsAvailable]
FROM [Products] AS [p]");
        }

        public override async Task Select_constant_int(bool async)
        {
            await base.Select_constant_int(async);

            AssertSql(
                @"SELECT 0
FROM [Customers] AS [c]");
        }

        public override async Task Select_constant_null_string(bool async)
        {
            await base.Select_constant_null_string(async);

            AssertSql(
                @"SELECT NULL
FROM [Customers] AS [c]");
        }

        public override async Task Select_local(bool async)
        {
            await base.Select_local(async);

            AssertSql(
                @"@__x_0='10'

SELECT @__x_0
FROM [Customers] AS [c]");
        }

        public override async Task Select_scalar_primitive_after_take(bool async)
        {
            await base.Select_scalar_primitive_after_take(async);

            AssertSql(
                @"@__p_0='9'

SELECT TOP(@__p_0) [e].[EmployeeID]
FROM [Employees] AS [e]");
        }

        public override async Task Select_project_filter(bool async)
        {
            await base.Select_project_filter(async);

            AssertSql(
                @"SELECT [c].[CompanyName]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'");
        }

        public override async Task Select_project_filter2(bool async)
        {
            await base.Select_project_filter2(async);

            AssertSql(
                @"SELECT [c].[City]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'");
        }

        public override async Task Select_nested_collection(bool async)
        {
            await base.Select_nested_collection(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [t].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE DATEPART(year, [o].[OrderDate]) = 1997
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]
WHERE [c].[City] = N'London'
ORDER BY [c].[CustomerID], [t].[OrderID]");
        }

        public override void Select_nested_collection_multi_level()
        {
            base.Select_nested_collection_multi_level();

            AssertSql(
                @"SELECT [c].[CustomerID], [t0].[Date], [t0].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [t].[Date], [t].[OrderID], [t].[CustomerID]
    FROM (
        SELECT [o].[OrderDate] AS [Date], [o].[OrderID], [o].[CustomerID], ROW_NUMBER() OVER(PARTITION BY [o].[CustomerID] ORDER BY [o].[OrderID]) AS [row]
        FROM [Orders] AS [o]
        WHERE [o].[OrderID] < 10500
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [c].[CustomerID] = [t0].[CustomerID]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID], [t0].[CustomerID], [t0].[OrderID]");
        }

        public override void Select_nested_collection_multi_level2()
        {
            base.Select_nested_collection_multi_level2();

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE ([c].[CustomerID] = [o].[CustomerID]) AND ([o].[OrderID] < 10500)) AS [OrderDates]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'");
        }

        public override void Select_nested_collection_multi_level3()
        {
            base.Select_nested_collection_multi_level3();

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE ([o].[OrderID] < 10500) AND ([c].[CustomerID] = [o].[CustomerID])) AS [OrderDates]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'");
        }

        public override void Select_nested_collection_multi_level4()
        {
            base.Select_nested_collection_multi_level4();

            AssertSql(
                @"SELECT COALESCE((
    SELECT TOP(1) (
        SELECT COUNT(*)
        FROM [Order Details] AS [o0]
        WHERE ([o].[OrderID] = [o0].[OrderID]) AND ([o0].[OrderID] > 10))
    FROM [Orders] AS [o]
    WHERE ([c].[CustomerID] = [o].[CustomerID]) AND ([o].[OrderID] < 10500)), 0) AS [Order]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'");
        }

        public override void Select_nested_collection_multi_level5()
        {
            base.Select_nested_collection_multi_level5();

            AssertSql(
                @"SELECT COALESCE((
    SELECT TOP(1) COALESCE((
        SELECT TOP(1) [o0].[ProductID]
        FROM [Order Details] AS [o0]
        WHERE ([o].[OrderID] = [o0].[OrderID]) AND (([o0].[OrderID] <> (
            SELECT COUNT(*)
            FROM [Orders] AS [o1]
            WHERE [c].[CustomerID] = [o1].[CustomerID])) OR (
            SELECT COUNT(*)
            FROM [Orders] AS [o1]
            WHERE [c].[CustomerID] = [o1].[CustomerID]) IS NULL)), 0)
    FROM [Orders] AS [o]
    WHERE ([c].[CustomerID] = [o].[CustomerID]) AND ([o].[OrderID] < 10500)), 0) AS [Order]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'");
        }

        public override void Select_nested_collection_multi_level6()
        {
            base.Select_nested_collection_multi_level6();

            AssertSql(
                @"SELECT COALESCE((
    SELECT TOP(1) COALESCE((
        SELECT TOP(1) [o0].[ProductID]
        FROM [Order Details] AS [o0]
        WHERE ([o].[OrderID] = [o0].[OrderID]) AND ([o0].[OrderID] <> CAST(LEN([c].[CustomerID]) AS int))), 0)
    FROM [Orders] AS [o]
    WHERE ([c].[CustomerID] = [o].[CustomerID]) AND ([o].[OrderID] < 10500)), 0) AS [Order]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'");
        }

        public override async Task Select_nested_collection_count_using_anonymous_type(bool async)
        {
            await base.Select_nested_collection_count_using_anonymous_type(async);

            AssertSql(
                @"SELECT (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]) AS [Count]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'");
        }

        public override async Task New_date_time_in_anonymous_type_works(bool async)
        {
            await base.New_date_time_in_anonymous_type_works(async);

            AssertSql(
                @"SELECT 1
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'");
        }

        public override async Task Select_non_matching_value_types_int_to_long_introduces_explicit_cast(bool async)
        {
            await base.Select_non_matching_value_types_int_to_long_introduces_explicit_cast(async);

            AssertSql(
                @"SELECT CAST([o].[OrderID] AS bigint)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]");
        }

        public override async Task Select_non_matching_value_types_nullable_int_to_long_introduces_explicit_cast(bool async)
        {
            await base.Select_non_matching_value_types_nullable_int_to_long_introduces_explicit_cast(async);

            AssertSql(
                @"SELECT CAST([o].[EmployeeID] AS bigint)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]");
        }

        public override async Task Select_non_matching_value_types_nullable_int_to_int_doesnt_introduce_explicit_cast(bool async)
        {
            await base.Select_non_matching_value_types_nullable_int_to_int_doesnt_introduce_explicit_cast(async);

            AssertSql(
                @"SELECT [o].[EmployeeID]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]");
        }

        public override async Task Select_non_matching_value_types_int_to_nullable_int_doesnt_introduce_explicit_cast(bool async)
        {
            await base.Select_non_matching_value_types_int_to_nullable_int_doesnt_introduce_explicit_cast(async);

            AssertSql(
                @"SELECT [o].[OrderID]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]");
        }

        public override async Task Select_non_matching_value_types_from_binary_expression_introduces_explicit_cast(bool async)
        {
            await base.Select_non_matching_value_types_from_binary_expression_introduces_explicit_cast(async);

            AssertSql(
                @"SELECT CAST(([o].[OrderID] + [o].[OrderID]) AS bigint)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]");
        }

        public override async Task Select_non_matching_value_types_from_binary_expression_nested_introduces_top_level_explicit_cast(
            bool async)
        {
            await base.Select_non_matching_value_types_from_binary_expression_nested_introduces_top_level_explicit_cast(async);

            AssertSql(
                @"SELECT CAST((CAST([o].[OrderID] AS bigint) + CAST([o].[OrderID] AS bigint)) AS smallint)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]");
        }

        public override async Task Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast1(bool async)
        {
            await base.Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast1(async);

            AssertSql(
                @"SELECT CAST(-[o].[OrderID] AS bigint)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]");
        }

        public override async Task Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast2(bool async)
        {
            await base.Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast2(async);

            AssertSql(
                @"SELECT -CAST([o].[OrderID] AS bigint)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]");
        }

        public override async Task Select_non_matching_value_types_from_length_introduces_explicit_cast(bool async)
        {
            await base.Select_non_matching_value_types_from_length_introduces_explicit_cast(async);

            AssertSql(
                @"SELECT CAST(CAST(LEN([o].[CustomerID]) AS int) AS bigint)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]");
        }

        public override async Task Select_non_matching_value_types_from_method_call_introduces_explicit_cast(bool async)
        {
            await base.Select_non_matching_value_types_from_method_call_introduces_explicit_cast(async);

            AssertSql(
                @"SELECT CAST(ABS([o].[OrderID]) AS bigint)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]");
        }

        public override async Task Select_non_matching_value_types_from_anonymous_type_introduces_explicit_cast(bool async)
        {
            await base.Select_non_matching_value_types_from_anonymous_type_introduces_explicit_cast(async);

            AssertSql(
                @"SELECT CAST([o].[OrderID] AS bigint) AS [LongOrder], CAST([o].[OrderID] AS smallint) AS [ShortOrder], [o].[OrderID] AS [Order]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]");
        }

        public override async Task Select_conditional_with_null_comparison_in_test(bool async)
        {
            await base.Select_conditional_with_null_comparison_in_test(async);

            AssertSql(
                @"SELECT CASE
    WHEN [o].[CustomerID] IS NULL THEN CAST(1 AS bit)
    ELSE CASE
        WHEN [o].[OrderID] < 100 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
END
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'");
        }

        public override async Task Select_over_10_nested_ternary_condition(bool isAsync)
        {
            await base.Select_over_10_nested_ternary_condition(isAsync);

            AssertSql(
                @"SELECT CASE
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
FROM [Customers] AS [c]");
        }

        public override async Task Projection_in_a_subquery_should_be_liftable(bool async)
        {
            await base.Projection_in_a_subquery_should_be_liftable(async);

            AssertSql(
                @"@__p_0='1'

SELECT [e].[EmployeeID]
FROM [Employees] AS [e]
ORDER BY [e].[EmployeeID]
OFFSET @__p_0 ROWS");
        }

        public override async Task Projection_containing_DateTime_subtraction(bool async)
        {
            await base.Projection_containing_DateTime_subtraction(async);

            AssertSql(
                @"SELECT [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300");
        }

        public override async Task Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault(bool async)
        {
            await base.Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [t].[CustomerID]
    FROM (
        SELECT TOP(1) [o].[CustomerID], [o].[OrderID]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderID]
    ) AS [t]
    ORDER BY [t].[OrderID])
FROM [Customers] AS [c]");
        }

        public override async Task Project_single_element_from_collection_with_OrderBy_Skip_and_FirstOrDefault(bool async)
        {
            await base.Project_single_element_from_collection_with_OrderBy_Skip_and_FirstOrDefault(async);

            AssertSql(
                @"SELECT (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]
    OFFSET 1 ROWS FETCH NEXT 1 ROWS ONLY)
FROM [Customers] AS [c]");
        }

        public override async Task Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault(bool async)
        {
            await base.Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault(async);

            AssertSql(
                @"SELECT (
    SELECT DISTINCT TOP(1) [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
FROM [Customers] AS [c]");
        }

        public override async Task
            Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault_followed_by_projecting_length(bool async)
        {
            await base.Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault_followed_by_projecting_length(
                async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) CAST(LEN([t].[CustomerID]) AS int)
    FROM (
        SELECT DISTINCT [o].[CustomerID]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
    ) AS [t])
FROM [Customers] AS [c]");
        }

        public override async Task Project_single_element_from_collection_with_OrderBy_Take_and_SingleOrDefault(bool async)
        {
            await base.Project_single_element_from_collection_with_OrderBy_Take_and_SingleOrDefault(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [t].[CustomerID]
    FROM (
        SELECT TOP(1) [o].[CustomerID], [o].[OrderID]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderID]
    ) AS [t]
    ORDER BY [t].[OrderID])
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault_with_parameter(bool async)
        {
            await base.Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault_with_parameter(async);

            AssertSql(
                @"@__i_0='1'

SELECT (
    SELECT TOP(1) [t].[CustomerID]
    FROM (
        SELECT TOP(@__i_0) [o].[CustomerID], [o].[OrderID]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderID]
    ) AS [t]
    ORDER BY [t].[OrderID])
FROM [Customers] AS [c]");
        }

        public override async Task Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault(bool async)
        {
            await base.Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [t].[CustomerID]
    FROM (
        SELECT TOP(2) [o].[CustomerID], [o].[OrderID], [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderID], [o].[OrderDate] DESC
    ) AS [t]
    ORDER BY [t].[OrderID], [t].[OrderDate] DESC)
FROM [Customers] AS [c]");
        }

        public override async Task
            Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_followed_by_projection_of_length_property(
                bool async)
        {
            await base
                .Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_followed_by_projection_of_length_property(
                    async);

            AssertSql(
                "");
        }

        public override async Task Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_2(bool async)
        {
            await base.Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_2(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [t].[CustomerID]
    FROM (
        SELECT TOP(2) [o].[CustomerID], [o].[OrderID], [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[CustomerID], [o].[OrderDate] DESC
    ) AS [t]
    ORDER BY [t].[CustomerID], [t].[OrderDate] DESC)
FROM [Customers] AS [c]");
        }

        public override async Task Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault(bool async)
        {
            await base.Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault(async);

            AssertSql(
                @"SELECT COALESCE((
    SELECT TOP(1) [t].[OrderID]
    FROM (
        SELECT TOP(1) [o0].[OrderID], [o0].[ProductID], [p].[ProductID] AS [ProductID0], [p].[ProductName]
        FROM [Order Details] AS [o0]
        INNER JOIN [Products] AS [p] ON [o0].[ProductID] = [p].[ProductID]
        WHERE [o].[OrderID] = [o0].[OrderID]
        ORDER BY [p].[ProductName]
    ) AS [t]
    ORDER BY [t].[ProductName]), 0)
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300");
        }

        public override async Task Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(
            bool async)
        {
            await base.Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(async);

            AssertSql(
                @"SELECT [t0].[OrderID], [t0].[ProductID], [t0].[Discount], [t0].[Quantity], [t0].[UnitPrice]
FROM [Orders] AS [o]
OUTER APPLY (
    SELECT TOP(1) [t].[OrderID], [t].[ProductID], [t].[Discount], [t].[Quantity], [t].[UnitPrice]
    FROM (
        SELECT TOP(1) [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [p].[ProductName]
        FROM [Order Details] AS [o0]
        INNER JOIN [Products] AS [p] ON [o0].[ProductID] = [p].[ProductID]
        WHERE [o].[OrderID] = [o0].[OrderID]
        ORDER BY [p].[ProductName]
    ) AS [t]
    ORDER BY [t].[ProductName]
) AS [t0]
WHERE [o].[OrderID] < 10250");
        }

        public override async Task Select_datetime_year_component(bool async)
        {
            await base.Select_datetime_year_component(async);

            AssertSql(
                @"SELECT DATEPART(year, [o].[OrderDate])
FROM [Orders] AS [o]");
        }

        public override async Task Select_datetime_month_component(bool async)
        {
            await base.Select_datetime_month_component(async);

            AssertSql(
                @"SELECT DATEPART(month, [o].[OrderDate])
FROM [Orders] AS [o]");
        }

        public override async Task Select_datetime_day_of_year_component(bool async)
        {
            await base.Select_datetime_day_of_year_component(async);

            AssertSql(
                @"SELECT DATEPART(dayofyear, [o].[OrderDate])
FROM [Orders] AS [o]");
        }

        public override async Task Select_datetime_day_component(bool async)
        {
            await base.Select_datetime_day_component(async);

            AssertSql(
                @"SELECT DATEPART(day, [o].[OrderDate])
FROM [Orders] AS [o]");
        }

        public override async Task Select_datetime_hour_component(bool async)
        {
            await base.Select_datetime_hour_component(async);

            AssertSql(
                @"SELECT DATEPART(hour, [o].[OrderDate])
FROM [Orders] AS [o]");
        }

        public override async Task Select_datetime_minute_component(bool async)
        {
            await base.Select_datetime_minute_component(async);

            AssertSql(
                @"SELECT DATEPART(minute, [o].[OrderDate])
FROM [Orders] AS [o]");
        }

        public override async Task Select_datetime_second_component(bool async)
        {
            await base.Select_datetime_second_component(async);

            AssertSql(
                @"SELECT DATEPART(second, [o].[OrderDate])
FROM [Orders] AS [o]");
        }

        public override async Task Select_datetime_millisecond_component(bool async)
        {
            await base.Select_datetime_millisecond_component(async);

            AssertSql(
                @"SELECT DATEPART(millisecond, [o].[OrderDate])
FROM [Orders] AS [o]");
        }

        public override async Task Select_byte_constant(bool async)
        {
            await base.Select_byte_constant(async);

            AssertSql(
                @"SELECT CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS tinyint)
    ELSE CAST(2 AS tinyint)
END
FROM [Customers] AS [c]");
        }

        public override async Task Select_short_constant(bool async)
        {
            await base.Select_short_constant(async);

            AssertSql(
                @"SELECT CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS smallint)
    ELSE CAST(2 AS smallint)
END
FROM [Customers] AS [c]");
        }

        public override async Task Select_bool_constant(bool async)
        {
            await base.Select_bool_constant(async);

            AssertSql(
                @"SELECT CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Customers] AS [c]");
        }

        public override async Task Anonymous_projection_AsNoTracking_Selector(bool async)
        {
            await base.Anonymous_projection_AsNoTracking_Selector(async);

            AssertSql(
                @"SELECT [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override async Task Anonymous_projection_with_repeated_property_being_ordered(bool async)
        {
            await base.Anonymous_projection_with_repeated_property_being_ordered(async);

            AssertSql(
                @"SELECT [c].[CustomerID] AS [A]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override async Task Anonymous_projection_with_repeated_property_being_ordered_2(bool async)
        {
            await base.Anonymous_projection_with_repeated_property_being_ordered_2(async);

            AssertSql(
                @"SELECT [c].[CustomerID] AS [A], [o].[CustomerID] AS [B]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [o].[CustomerID]");
        }

        public override async Task Select_GetValueOrDefault_on_DateTime(bool async)
        {
            await base.Select_GetValueOrDefault_on_DateTime(async);

            AssertSql(
                @"SELECT [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override async Task Select_GetValueOrDefault_on_DateTime_with_null_values(bool async)
        {
            await base.Select_GetValueOrDefault_on_DateTime_with_null_values(async);

            AssertSql(
                @"SELECT [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override async Task Cast_on_top_level_projection_brings_explicit_Cast(bool async)
        {
            await base.Cast_on_top_level_projection_brings_explicit_Cast(async);

            AssertSql(
                @"SELECT CAST([o].[OrderID] AS float)
FROM [Orders] AS [o]");
        }

        public override async Task Projecting_nullable_struct(bool async)
        {
            await base.Projecting_nullable_struct(async);

            AssertSql(
                @"SELECT [o].[CustomerID], CASE
    WHEN ([o].[CustomerID] = N'ALFKI') AND [o].[CustomerID] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [o].[OrderID], CAST(LEN([o].[CustomerID]) AS int)
FROM [Orders] AS [o]");
        }

        public override async Task Multiple_select_many_with_predicate(bool async)
        {
            await base.Multiple_select_many_with_predicate(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
INNER JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE CAST([o0].[Discount] AS float) >= 0.25E0");
        }

        public override async Task SelectMany_without_result_selector_naked_collection_navigation(bool async)
        {
            await base.SelectMany_without_result_selector_naked_collection_navigation(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override async Task SelectMany_without_result_selector_collection_navigation_composed(bool async)
        {
            await base.SelectMany_without_result_selector_collection_navigation_composed(async);

            AssertSql(
                @"SELECT [o].[CustomerID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override async Task SelectMany_correlated_with_outer_1(bool async)
        {
            await base.SelectMany_correlated_with_outer_1(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[City] AS [o]
FROM [Customers] AS [c]
CROSS APPLY (
    SELECT [c].[City]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [t]");
        }

        public override async Task SelectMany_correlated_with_outer_2(bool async)
        {
            await base.SelectMany_correlated_with_outer_2(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM [Customers] AS [c]
CROSS APPLY (
    SELECT TOP(2) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [c].[City], [o].[OrderID]
) AS [t]");
        }

        public override async Task SelectMany_correlated_with_outer_3(bool async)
        {
            await base.SelectMany_correlated_with_outer_3(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[City] AS [o]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT [c].[City]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [t]");
        }

        public override async Task SelectMany_correlated_with_outer_4(bool async)
        {
            await base.SelectMany_correlated_with_outer_4(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT TOP(2) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [c].[City], [o].[OrderID]
) AS [t]");
        }

        public override async Task SelectMany_correlated_with_outer_5(bool async)
        {
            await base.SelectMany_correlated_with_outer_5(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[City] AS [o]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT [c].[City]
    FROM [Orders] AS [o]
    WHERE ([c].[CustomerID] <> [o].[CustomerID]) OR [o].[CustomerID] IS NULL
) AS [t]");
        }

        public override async Task SelectMany_correlated_with_outer_6(bool async)
        {
            await base.SelectMany_correlated_with_outer_6(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT TOP(2) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE ([c].[CustomerID] <> [o].[CustomerID]) OR [o].[CustomerID] IS NULL
    ORDER BY [c].[City], [o].[OrderID]
) AS [t]");
        }

        public override async Task SelectMany_correlated_with_outer_7(bool async)
        {
            await base.SelectMany_correlated_with_outer_7(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT TOP(2) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE CAST(LEN([c].[CustomerID]) AS int) >= CAST(LEN([o].[CustomerID]) AS int)
    ORDER BY [c].[City], [o].[OrderID]
) AS [t]");
        }

        public override async Task FirstOrDefault_over_empty_collection_of_value_type_returns_correct_results(bool async)
        {
            await base.FirstOrDefault_over_empty_collection_of_value_type_returns_correct_results(async);

            AssertSql(
                @"SELECT [c].[CustomerID], COALESCE((
    SELECT TOP(1) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]), 0) AS [OrderId]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'FISSA'");
        }

        public override async Task Project_non_nullable_value_after_FirstOrDefault_on_empty_collection(bool async)
        {
            await base.Project_non_nullable_value_after_FirstOrDefault_on_empty_collection(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) CAST(LEN([o].[CustomerID]) AS int)
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = N'John Doe')
FROM [Customers] AS [c]");
        }

        public override Task Member_binding_after_ctor_arguments_fails_with_client_eval(bool async)
        {
            return AssertTranslationFailed(() => base.Member_binding_after_ctor_arguments_fails_with_client_eval(async));
        }

        public override async Task Filtered_collection_projection_is_tracked(bool async)
        {
            await base.Filtered_collection_projection_is_tracked(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] > 11000
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID], [t].[OrderID]");
        }

        public override async Task Filtered_collection_projection_with_to_list_is_tracked(bool async)
        {
            await base.Filtered_collection_projection_with_to_list_is_tracked(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] > 11000
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID], [t].[OrderID]");
        }

        public override async Task SelectMany_with_collection_being_correlated_subquery_which_references_inner_and_outer_entity(
            bool async)
        {
            await base.SelectMany_with_collection_being_correlated_subquery_which_references_inner_and_outer_entity(async);

            AssertSql(
                @"SELECT [t].[OrderProperty], [t].[CustomerProperty]
FROM [Customers] AS [c]
CROSS APPLY (
    SELECT [o].[CustomerID] AS [OrderProperty], [c].[CustomerID] AS [CustomerProperty]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [t]");
        }

        public override async Task
            SelectMany_with_collection_being_correlated_subquery_which_references_non_mapped_properties_from_inner_and_outer_entity(
                bool async)
        {
            await base
                .SelectMany_with_collection_being_correlated_subquery_which_references_non_mapped_properties_from_inner_and_outer_entity(
                    async);

            AssertSql(
                @"");
        }

        public override async Task Select_with_complex_expression_that_can_be_funcletized(bool async)
        {
            await base.Select_with_complex_expression_that_can_be_funcletized(async);

            AssertSql(
                @"SELECT 0
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task Select_chained_entity_navigation_doesnt_materialize_intermittent_entities(bool async)
        {
            await base.Select_chained_entity_navigation_doesnt_materialize_intermittent_entities(async);

            AssertSql(
                @"SELECT [o].[OrderID], [c].[CustomerID], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
LEFT JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
ORDER BY [o].[OrderID], [c].[CustomerID], [o0].[OrderID]");
        }

        public override async Task Select_entity_compared_to_null(bool async)
        {
            await base.Select_entity_compared_to_null(async);

            AssertSql(
                @"SELECT CASE
    WHEN [c].[CustomerID] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[CustomerID] = N'ALFKI'");
        }

        public override async Task Explicit_cast_in_arithmetic_operation_is_preserved(bool async)
        {
            await base.Explicit_cast_in_arithmetic_operation_is_preserved(async);

            AssertSql(
                @"SELECT CAST([o].[OrderID] AS decimal(18,2)) / CAST(([o].[OrderID] + 1000) AS decimal(18,2))
FROM [Orders] AS [o]
WHERE [o].[OrderID] = 10243");
        }

        public override async Task SelectMany_whose_selector_references_outer_source(bool async)
        {
            await base.SelectMany_whose_selector_references_outer_source(async);

            AssertSql(
                @"SELECT [t].[OrderDate], [t].[CustomerCity]
FROM [Customers] AS [c]
CROSS APPLY (
    SELECT [o].[OrderDate], [c].[City] AS [CustomerCity]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [t]");
        }

        public override async Task Collection_FirstOrDefault_with_entity_equality_check_in_projection(bool async)
        {
            await base.Collection_FirstOrDefault_with_entity_equality_check_in_projection(async);

            AssertSql(" ");
        }

        public override async Task Collection_FirstOrDefault_with_nullable_unsigned_int_column(bool async)
        {
            await base.Collection_FirstOrDefault_with_nullable_unsigned_int_column(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [o].[EmployeeID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID])
FROM [Customers] AS [c]");
        }

        public override async Task ToList_Count_in_projection_works(bool async)
        {
            await base.ToList_Count_in_projection_works(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]) AS [Count]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'");
        }

        public override async Task LastOrDefault_member_access_in_projection_translates_to_server(bool async)
        {
            await base.LastOrDefault_member_access_in_projection_translates_to_server(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], (
    SELECT TOP(1) [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]) AS [OrderDate]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'");
        }

        public override async Task Projection_with_parameterized_constructor(bool async)
        {
            await base.Projection_with_parameterized_constructor(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task Projection_with_parameterized_constructor_with_member_assignment(bool async)
        {
            await base.Projection_with_parameterized_constructor_with_member_assignment(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task Collection_projection_AsNoTracking_OrderBy(bool async)
        {
            await base.Collection_projection_AsNoTracking_OrderBy(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID], [o].[OrderID]");
        }

        public override async Task Coalesce_over_nullable_uint(bool async)
        {
            await base.Coalesce_over_nullable_uint(async);

            AssertSql(
                @"SELECT COALESCE([o].[EmployeeID], 0)
FROM [Orders] AS [o]");
        }

        public override async Task Project_uint_through_collection_FirstOrDefault(bool async)
        {
            await base.Project_uint_through_collection_FirstOrDefault(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [o].[EmployeeID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID])
FROM [Customers] AS [c]");
        }

        public override async Task Project_keyless_entity_FirstOrDefault_without_orderby(bool async)
        {
            await base.Project_keyless_entity_FirstOrDefault_without_orderby(async);

            AssertSql(
                @"SELECT [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle]
    FROM (
        SELECT [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], ROW_NUMBER() OVER(PARTITION BY [c0].[CompanyName] ORDER BY (SELECT 1)) AS [row]
        FROM (
            SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]
        ) AS [c0]
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [c].[CompanyName] = [t0].[CompanyName]");
        }

        public override async Task Reverse_changes_asc_order_to_desc(bool async)
        {
            await base.Reverse_changes_asc_order_to_desc(async);

            AssertSql(
                @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]
ORDER BY [e].[EmployeeID] DESC");
        }

        public override async Task Reverse_changes_desc_order_to_asc(bool async)
        {
            await base.Reverse_changes_desc_order_to_asc(async);

            AssertSql(
                @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]
ORDER BY [e].[EmployeeID]");
        }

        public override async Task Projection_AsEnumerable_projection(bool async)
        {
            await base.Projection_AsEnumerable_projection(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] < 10750
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]
WHERE ([c].[CustomerID] LIKE N'A%') AND ((
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE ([o].[CustomerID] = [c].[CustomerID]) AND ([o].[OrderID] < 11000)) > 0)
ORDER BY [c].[CustomerID], [t].[OrderID]");
        }

        public override async Task Projection_custom_type_in_both_sides_of_ternary(bool async)
        {
            await base.Projection_custom_type_in_both_sides_of_ternary(async);

            AssertSql(
                @"SELECT CASE
    WHEN ([c].[City] = N'Seattle') AND [c].[City] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override async Task Projecting_multiple_collection_with_same_constant_works(bool async)
        {
            await base.Projecting_multiple_collection_with_same_constant_works(async);

            AssertSql(
                @"SELECT [c].[CustomerID], 1, [o].[OrderID], [o0].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
LEFT JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID], [o].[OrderID], [o0].[OrderID]");
        }

        public override async Task Custom_projection_reference_navigation_PK_to_FK_optimization(bool async)
        {
            await base.Custom_projection_reference_navigation_PK_to_FK_optimization(async);

            AssertSql(
                @"SELECT [o].[OrderID], [c].[CustomerID], [c].[City], [o].[OrderDate]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]");
        }

        public override async Task Projecting_Length_of_a_string_property_after_FirstOrDefault_on_correlated_collection(bool async)
        {
            await base.Projecting_Length_of_a_string_property_after_FirstOrDefault_on_correlated_collection(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) CAST(LEN([o].[CustomerID]) AS int)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID])
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override async Task Projecting_count_of_navigation_which_is_generic_list(bool async)
        {
            await base.Projecting_count_of_navigation_which_is_generic_list(async);

            AssertSql(
                @"SELECT (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override async Task Projecting_count_of_navigation_which_is_generic_collection(bool async)
        {
            await base.Projecting_count_of_navigation_which_is_generic_collection(async);

            AssertSql(
                @"SELECT (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override async Task Projection_take_projection_doesnt_project_intermittent_column(bool async)
        {
            await base.Projection_take_projection_doesnt_project_intermittent_column(async);

            AssertSql(
                @"@__p_0='10'

SELECT TOP(@__p_0) ([c].[CustomerID] + N' ') + COALESCE([c].[City], N'') AS [Aggregate]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override async Task Projection_skip_projection_doesnt_project_intermittent_column(bool async)
        {
            await base.Projection_skip_projection_doesnt_project_intermittent_column(async);

            AssertSql(
                @"@__p_0='7'

SELECT ([c].[CustomerID] + N' ') + COALESCE([c].[City], N'') AS [Aggregate]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
OFFSET @__p_0 ROWS");
        }

        public override async Task Projection_Distinct_projection_preserves_columns_used_for_distinct_in_subquery(bool async)
        {
            await base.Projection_Distinct_projection_preserves_columns_used_for_distinct_in_subquery(async);

            AssertSql(
                @"SELECT (COALESCE([t].[FirstLetter], N'') + N' ') + [t].[Foo] AS [Aggregate]
FROM (
    SELECT DISTINCT [c].[CustomerID], SUBSTRING([c].[CustomerID], 0 + 1, 1) AS [FirstLetter], N'Foo' AS [Foo]
    FROM [Customers] AS [c]
) AS [t]");
        }

        public override async Task Projection_take_predicate_projection(bool async)
        {
            await base.Projection_take_predicate_projection(async);

            AssertSql(
                @"@__p_0='10'

SELECT ([t].[CustomerID] + N' ') + COALESCE([t].[City], N'') AS [Aggregate]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[City]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [t]
WHERE [t].[CustomerID] LIKE N'A%'
ORDER BY [t].[CustomerID]");
        }

        public override async Task Do_not_erase_projection_mapping_when_adding_single_projection(bool async)
        {
            await base.Do_not_erase_projection_mapping_when_adding_single_projection(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [t].[OrderID], [t].[ProductID], [t].[Discount], [t].[Quantity], [t].[UnitPrice], [t].[ProductID0], [t].[Discontinued], [t].[ProductName], [t].[SupplierID], [t].[UnitPrice0], [t].[UnitsInStock], [t0].[OrderID], [t0].[ProductID], [t0].[ProductID0], [t2].[OrderID], [t2].[ProductID], [t2].[Discount], [t2].[Quantity], [t2].[UnitPrice], [t2].[ProductID0], [t2].[Discontinued], [t2].[ProductName], [t2].[SupplierID], [t2].[UnitPrice0], [t2].[UnitsInStock], [t0].[Discount], [t0].[Quantity], [t0].[UnitPrice], [t0].[Discontinued], [t0].[ProductName], [t0].[SupplierID], [t0].[UnitPrice0], [t0].[UnitsInStock]
FROM [Orders] AS [o]
LEFT JOIN (
    SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [p].[ProductID] AS [ProductID0], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice] AS [UnitPrice0], [p].[UnitsInStock]
    FROM [Order Details] AS [o0]
    INNER JOIN [Products] AS [p] ON [o0].[ProductID] = [p].[ProductID]
) AS [t] ON [o].[OrderID] = [t].[OrderID]
LEFT JOIN (
    SELECT [t1].[OrderID], [t1].[ProductID], [t1].[Discount], [t1].[Quantity], [t1].[UnitPrice], [t1].[ProductID0], [t1].[Discontinued], [t1].[ProductName], [t1].[SupplierID], [t1].[UnitPrice0], [t1].[UnitsInStock]
    FROM (
        SELECT [o1].[OrderID], [o1].[ProductID], [o1].[Discount], [o1].[Quantity], [o1].[UnitPrice], [p0].[ProductID] AS [ProductID0], [p0].[Discontinued], [p0].[ProductName], [p0].[SupplierID], [p0].[UnitPrice] AS [UnitPrice0], [p0].[UnitsInStock], ROW_NUMBER() OVER(PARTITION BY [o1].[OrderID] ORDER BY [o1].[OrderID], [o1].[ProductID], [p0].[ProductID]) AS [row]
        FROM [Order Details] AS [o1]
        INNER JOIN [Products] AS [p0] ON [o1].[ProductID] = [p0].[ProductID]
        WHERE [o1].[UnitPrice] > 10.0
    ) AS [t1]
    WHERE [t1].[row] <= 1
) AS [t0] ON [o].[OrderID] = [t0].[OrderID]
LEFT JOIN (
    SELECT [o2].[OrderID], [o2].[ProductID], [o2].[Discount], [o2].[Quantity], [o2].[UnitPrice], [p1].[ProductID] AS [ProductID0], [p1].[Discontinued], [p1].[ProductName], [p1].[SupplierID], [p1].[UnitPrice] AS [UnitPrice0], [p1].[UnitsInStock]
    FROM [Order Details] AS [o2]
    INNER JOIN [Products] AS [p1] ON [o2].[ProductID] = [p1].[ProductID]
    WHERE [o2].[UnitPrice] < 10.0
) AS [t2] ON [o].[OrderID] = [t2].[OrderID]
WHERE [o].[OrderID] < 10350
ORDER BY [o].[OrderID], [t].[OrderID], [t].[ProductID], [t].[ProductID0], [t0].[OrderID], [t0].[ProductID], [t0].[ProductID0], [t2].[OrderID], [t2].[ProductID], [t2].[ProductID0]");
        }

        public override async Task Ternary_in_client_eval_assigns_correct_types(bool async)
        {
            await base.Ternary_in_client_eval_assigns_correct_types(async);

            AssertSql(
                @"SELECT [o].[CustomerID], CASE
    WHEN [o].[OrderDate] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [o].[OrderDate], [o].[OrderID] - 10000, CASE
    WHEN [o].[OrderDate] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300
ORDER BY [o].[OrderID]");
        }

        public override async Task Projecting_after_navigation_and_distinct(bool async)
        {
            await base.Projecting_after_navigation_and_distinct(async);

            AssertSql(
                @"SELECT [t].[CustomerID], [t0].[CustomerID], [t0].[OrderID], [t0].[OrderDate]
FROM (
    SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Orders] AS [o]
    LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
) AS [t]
OUTER APPLY (
    SELECT [t].[CustomerID], [o0].[OrderID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] IN (10248, 10249, 10250) AND (([t].[CustomerID] = [o0].[CustomerID]) OR ([t].[CustomerID] IS NULL AND [o0].[CustomerID] IS NULL))
) AS [t0]
ORDER BY [t].[CustomerID], [t0].[OrderID]");
        }

        public override async Task Correlated_collection_after_distinct_with_complex_projection_containing_original_identifier(bool async)
        {
            await base.Correlated_collection_after_distinct_with_complex_projection_containing_original_identifier(async);

            AssertSql(
                @"SELECT [t].[OrderID], [t].[Complex], [t0].[Outer], [t0].[Inner], [t0].[OrderDate]
FROM (
    SELECT DISTINCT [o].[OrderID], DATEPART(month, [o].[OrderDate]) AS [Complex]
    FROM [Orders] AS [o]
) AS [t]
OUTER APPLY (
    SELECT [t].[OrderID] AS [Outer], [o0].[OrderID] AS [Inner], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] IN (10248, 10249, 10250) AND ([t].[OrderID] = [o0].[OrderID])
) AS [t0]
ORDER BY [t].[OrderID], [t0].[Inner]");
        }

        public override async Task Correlated_collection_after_distinct_not_containing_original_identifier(bool async)
        {
            await base.Correlated_collection_after_distinct_not_containing_original_identifier(async);

            AssertSql(
                @"SELECT [t].[OrderDate], [t].[CustomerID], [t0].[Outer1], [t0].[Outer2], [t0].[Inner], [t0].[OrderDate]
FROM (
    SELECT DISTINCT [o].[OrderDate], [o].[CustomerID]
    FROM [Orders] AS [o]
) AS [t]
OUTER APPLY (
    SELECT [t].[OrderDate] AS [Outer1], [t].[CustomerID] AS [Outer2], [o0].[OrderID] AS [Inner], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] IN (10248, 10249, 10250) AND (([t].[CustomerID] = [o0].[CustomerID]) OR ([t].[CustomerID] IS NULL AND [o0].[CustomerID] IS NULL))
) AS [t0]
ORDER BY [t].[OrderDate], [t].[CustomerID], [t0].[Inner]");
        }

        public override async Task Correlated_collection_after_distinct_with_complex_projection_not_containing_original_identifier(bool async)
        {
            await base.Correlated_collection_after_distinct_with_complex_projection_not_containing_original_identifier(async);

            AssertSql(
                @"SELECT [t].[OrderDate], [t].[CustomerID], [t].[Complex], [t0].[Outer1], [t0].[Outer2], [t0].[Outer3], [t0].[Inner], [t0].[OrderDate]
FROM (
    SELECT DISTINCT [o].[OrderDate], [o].[CustomerID], DATEPART(month, [o].[OrderDate]) AS [Complex]
    FROM [Orders] AS [o]
) AS [t]
OUTER APPLY (
    SELECT [t].[OrderDate] AS [Outer1], [t].[CustomerID] AS [Outer2], [t].[Complex] AS [Outer3], [o0].[OrderID] AS [Inner], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] IN (10248, 10249, 10250) AND (([t].[CustomerID] = [o0].[CustomerID]) OR ([t].[CustomerID] IS NULL AND [o0].[CustomerID] IS NULL))
) AS [t0]
ORDER BY [t].[OrderDate], [t].[CustomerID], [t].[Complex], [t0].[Inner]");
        }

        public override async Task Correlated_collection_after_groupby_with_complex_projection_containing_original_identifier(bool async)
        {
            await base.Correlated_collection_after_groupby_with_complex_projection_containing_original_identifier(async);

            AssertSql(
                @"SELECT [t].[OrderID], [t].[c], [t0].[Outer], [t0].[Inner], [t0].[OrderDate]
FROM (
    SELECT [o].[OrderID], DATEPART(month, [o].[OrderDate]) AS [c]
    FROM [Orders] AS [o]
    GROUP BY [o].[OrderID], DATEPART(month, [o].[OrderDate])
) AS [t]
OUTER APPLY (
    SELECT [t].[OrderID] AS [Outer], [o0].[OrderID] AS [Inner], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] IN (10248, 10249, 10250) AND ([t].[OrderID] = [o0].[OrderID])
) AS [t0]
ORDER BY [t].[OrderID], [t0].[Inner]");
        }

        public override async Task Select_nested_collection_deep(bool async)
        {
            await base.Select_nested_collection_deep(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [t0].[OrderID], [t0].[OrderID0], [t0].[OrderID00]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT [o].[OrderID], [t].[OrderID] AS [OrderID0], [t].[OrderID0] AS [OrderID00]
    FROM [Orders] AS [o]
    OUTER APPLY (
        SELECT [o].[OrderID], [o0].[OrderID] AS [OrderID0]
        FROM [Orders] AS [o0]
        WHERE [o].[CustomerID] = [c].[CustomerID]
    ) AS [t]
    WHERE (DATEPART(year, [o].[OrderDate]) = 1997) AND ([c].[CustomerID] = [o].[CustomerID])
) AS [t0]
WHERE [c].[City] = N'London'
ORDER BY [c].[CustomerID], [t0].[OrderID], [t0].[OrderID00]");
        }

        public override async Task Select_nested_collection_deep_distinct_no_identifiers(bool async)
        {
            await base.Select_nested_collection_deep_distinct_no_identifiers(async);

            AssertSql(
                @"SELECT [t].[City], [t1].[OrderID], [t1].[OrderID0], [t1].[OrderID00]
FROM (
    SELECT DISTINCT [c].[City]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'London'
) AS [t]
OUTER APPLY (
    SELECT [t0].[OrderID], [t2].[OrderID] AS [OrderID0], [t2].[OrderID0] AS [OrderID00]
    FROM (
        SELECT DISTINCT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE (([o].[CustomerID] = [t].[City]) OR ([o].[CustomerID] IS NULL AND [t].[City] IS NULL)) AND (DATEPART(year, [o].[OrderDate]) = 1997)
    ) AS [t0]
    OUTER APPLY (
        SELECT [t0].[OrderID], [o0].[OrderID] AS [OrderID0]
        FROM [Orders] AS [o0]
        WHERE ([t0].[CustomerID] = [t].[City]) OR ([t0].[CustomerID] IS NULL AND [t].[City] IS NULL)
    ) AS [t2]
) AS [t1]
ORDER BY [t].[City], [t1].[OrderID], [t1].[OrderID00]");
        }

        public override async Task Collection_include_over_result_of_single_non_scalar(bool async)
        {
            await base.Collection_include_over_result_of_single_non_scalar(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], [t].[OrderID0], [t].[ProductID], [t].[Discount], [t].[Quantity], [t].[UnitPrice], [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate], [o2].[OrderID], [o2].[ProductID], [o2].[Discount], [o2].[Quantity], [o2].[UnitPrice]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o0].[OrderID] AS [OrderID0], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
    FROM [Orders] AS [o]
    LEFT JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]
LEFT JOIN (
    SELECT [t1].[OrderID], [t1].[CustomerID], [t1].[EmployeeID], [t1].[OrderDate]
    FROM (
        SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], ROW_NUMBER() OVER(PARTITION BY [o1].[CustomerID] ORDER BY [o1].[OrderDate]) AS [row]
        FROM [Orders] AS [o1]
    ) AS [t1]
    WHERE [t1].[row] <= 1
) AS [t0] ON [c].[CustomerID] = [t0].[CustomerID]
LEFT JOIN [Order Details] AS [o2] ON [t0].[OrderID] = [o2].[OrderID]
WHERE [c].[CustomerID] LIKE N'F%'
ORDER BY [c].[CustomerID], [t].[OrderID], [t].[OrderID0], [t].[ProductID], [t0].[OrderID], [o2].[OrderID], [o2].[ProductID]");
        }

        public override async Task Collection_projection_selecting_outer_element_followed_by_take(bool async)
        {
            await base.Collection_projection_selecting_outer_element_followed_by_take(async);

            AssertSql(
                @"@__p_0='10'

SELECT [t].[CustomerID], [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region], [t0].[OrderID], [t0].[OrderID0], [t0].[CustomerID0], [t0].[EmployeeID], [t0].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'F%'
    ORDER BY [c].[CustomerID]
) AS [t]
OUTER APPLY (
    SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [t1].[OrderID] AS [OrderID0], [t1].[CustomerID] AS [CustomerID0], [t1].[EmployeeID], [t1].[OrderDate]
    FROM [Orders] AS [o]
    OUTER APPLY (
        SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
        FROM [Orders] AS [o0]
        WHERE [t].[CustomerID] = [o0].[CustomerID]
    ) AS [t1]
    WHERE [t].[CustomerID] = [o].[CustomerID]
) AS [t0]
ORDER BY [t].[CustomerID], [t0].[OrderID], [t0].[OrderID0]");
        }

        public override async Task Take_on_top_level_and_on_collection_projection_with_outer_apply(bool async)
        {
            await base.Take_on_top_level_and_on_collection_projection_with_outer_apply(async);

            AssertSql(
                @"SELECT [t].[OrderID], [t].[OrderDate], [t0].[OrderID], [t0].[ProductID], [t0].[Discontinued], [t0].[ProductName], [t0].[SupplierID], [t0].[UnitPrice], [t0].[UnitsInStock], [t0].[UnitPrice0], [t0].[ProductID0]
FROM (
    SELECT TOP(1) [o].[OrderID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] IS NOT NULL AND ([o].[CustomerID] LIKE N'F%')
) AS [t]
OUTER APPLY (
    SELECT [t1].[OrderID], [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock], [t1].[UnitPrice] AS [UnitPrice0], [t1].[ProductID] AS [ProductID0]
    FROM (
        SELECT [o0].[OrderID], [o0].[ProductID], [o0].[UnitPrice]
        FROM [Order Details] AS [o0]
        WHERE [t].[OrderID] = [o0].[OrderID]
        ORDER BY [o0].[OrderID] DESC
        OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY
    ) AS [t1]
    INNER JOIN [Products] AS [p] ON [t1].[ProductID] = [p].[ProductID]
) AS [t0]
ORDER BY [t].[OrderID], [t0].[OrderID] DESC, [t0].[ProductID0], [t0].[ProductID]");
        }

        public override async Task Take_on_correlated_collection_in_first(bool async)
        {
            await base.Take_on_correlated_collection_in_first(async);

            AssertSql(
                @"SELECT [t].[CustomerID], [t0].[Title], [t0].[OrderID], [t0].[CustomerID]
FROM (
    SELECT TOP(1) [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'F%'
    ORDER BY [c].[CustomerID]
) AS [t]
OUTER APPLY (
    SELECT CASE
        WHEN ([t1].[CustomerID] = [c0].[CustomerID]) OR ([t1].[CustomerID] IS NULL AND [c0].[CustomerID] IS NULL) THEN N'A'
        ELSE N'B'
    END AS [Title], [t1].[OrderID], [c0].[CustomerID], [t1].[OrderDate]
    FROM (
        SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [t].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderDate]
    ) AS [t1]
    LEFT JOIN [Customers] AS [c0] ON [t1].[CustomerID] = [c0].[CustomerID]
) AS [t0]
ORDER BY [t].[CustomerID], [t0].[OrderDate], [t0].[OrderID], [t0].[CustomerID]");
        }

        public override async Task Client_projection_via_ctor_arguments(bool async)
        {
            await base.Client_projection_via_ctor_arguments(async);

            AssertSql(
                @"SELECT [t].[CustomerID], [t].[City], [o0].[OrderID], [o0].[OrderDate], [t].[c]
FROM (
    SELECT TOP(2) [c].[CustomerID], [c].[City], (
        SELECT COUNT(*)
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]) AS [c]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'ALFKI'
) AS [t]
LEFT JOIN [Orders] AS [o0] ON [t].[CustomerID] = [o0].[CustomerID]
ORDER BY [t].[CustomerID], [o0].[OrderID]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
