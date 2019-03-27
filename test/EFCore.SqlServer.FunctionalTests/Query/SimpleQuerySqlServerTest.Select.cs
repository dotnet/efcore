// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class SimpleQuerySqlServerTest
    {
        public override async Task Projection_when_arithmetic_expression_precendence(bool isAsync)
        {
            await base.Projection_when_arithmetic_expression_precendence(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID] / ([o].[OrderID] / 2) AS [A], ([o].[OrderID] / [o].[OrderID]) / 2 AS [B]
FROM [Orders] AS [o]");
        }

        public override async Task Projection_when_arithmetic_expressions(bool isAsync)
        {
            await base.Projection_when_arithmetic_expressions(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override async Task Projection_when_arithmetic_mixed(bool isAsync)
        {
            await base.Projection_when_arithmetic_mixed(isAsync);

            AssertSql(
                @"@__p_0='10'

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], [t0].[EmployeeID], [t0].[City], [t0].[Country], [t0].[FirstName], [t0].[ReportsTo], [t0].[Title]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]
CROSS JOIN (
    SELECT TOP(5) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
    ORDER BY [e].[EmployeeID]
) AS [t0]");
        }

        public override async Task Projection_when_arithmetic_mixed_subqueries(bool isAsync)
        {
            await base.Projection_when_arithmetic_mixed_subqueries(isAsync);

            AssertSql(
                @"@__p_0='3'

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]",
                //
                @"SELECT TOP(2) [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title]
FROM [Employees] AS [e0]
ORDER BY [e0].[EmployeeID]",
                //
                @"SELECT TOP(2) [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title]
FROM [Employees] AS [e0]
ORDER BY [e0].[EmployeeID]",
                //
                @"SELECT TOP(2) [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title]
FROM [Employees] AS [e0]
ORDER BY [e0].[EmployeeID]");
        }

        public override async Task Projection_when_null_value(bool isAsync)
        {
            await base.Projection_when_null_value(isAsync);

            AssertSql(
                @"SELECT [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Projection_when_client_evald_subquery(bool isAsync)
        {
            await base.Projection_when_client_evald_subquery(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                //
                @"SELECT [t].[CustomerID], [c.Orders].[CustomerID]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]");
        }

        public override async Task Project_to_object_array(bool isAsync)
        {
            await base.Project_to_object_array(isAsync);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = 1");
        }

        public override async Task Project_to_int_array(bool isAsync)
        {
            await base.Project_to_int_array(isAsync);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[ReportsTo]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = 1");
        }

        public override async Task Select_bool_closure_with_order_by_property_with_cast_to_nullable(bool isAsync)
        {
            await base.Select_bool_closure_with_order_by_property_with_cast_to_nullable(isAsync);

            AssertSql(
                @"@__boolean_0='False'

SELECT @__boolean_0 AS [f]
FROM [Customers] AS [c]
ORDER BY (SELECT 1)");
        }

        public override async Task Select_bool_closure_with_order_parameter_with_cast_to_nullable(bool isAsync)
        {
            await base.Select_bool_closure_with_order_parameter_with_cast_to_nullable(isAsync);

            AssertSql(
                @"@__boolean_0='False'

SELECT @__boolean_0
FROM [Customers] AS [c]
ORDER BY (SELECT 1)");
        }

        public override async Task Select_scalar(bool isAsync)
        {
            await base.Select_scalar(isAsync);

            AssertSql(
                @"SELECT [c].[City]
FROM [Customers] AS [c]");
        }

        public override async Task Select_anonymous_one(bool isAsync)
        {
            await base.Select_anonymous_one(isAsync);

            AssertSql(
                @"SELECT [c].[City]
FROM [Customers] AS [c]");
        }

        public override async Task Select_anonymous_two(bool isAsync)
        {
            await base.Select_anonymous_two(isAsync);

            AssertSql(
                @"SELECT [c].[City], [c].[Phone]
FROM [Customers] AS [c]");
        }

        public override async Task Select_anonymous_three(bool isAsync)
        {
            await base.Select_anonymous_three(isAsync);

            AssertSql(
                @"SELECT [c].[City], [c].[Phone], [c].[Country]
FROM [Customers] AS [c]");
        }

        public override async Task Select_anonymous_bool_constant_true(bool isAsync)
        {
            await base.Select_anonymous_bool_constant_true(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]");
        }

        public override async Task Select_anonymous_constant_in_expression(bool isAsync)
        {
            await base.Select_anonymous_constant_in_expression(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], CAST(LEN([c].[CustomerID]) AS int) + 5 AS [Expression]
FROM [Customers] AS [c]");
        }

        public override async Task Select_anonymous_conditional_expression(bool isAsync)
        {
            await base.Select_anonymous_conditional_expression(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], CASE
    WHEN [p].[UnitsInStock] > CAST(0 AS smallint)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END AS [IsAvailable]
FROM [Products] AS [p]");
        }

        public override async Task Select_constant_int(bool isAsync)
        {
            await base.Select_constant_int(isAsync);

            AssertSql(
                @"SELECT 1
FROM [Customers] AS [c]");
        }

        public override async Task Select_constant_null_string(bool isAsync)
        {
            await base.Select_constant_null_string(isAsync);

            AssertSql(
                @"SELECT 1
FROM [Customers] AS [c]");
        }

        public override async Task Select_local(bool isAsync)
        {
            await base.Select_local(isAsync);

            AssertSql(
                @"@__x_0='10'

SELECT @__x_0
FROM [Customers] AS [c]");
        }

        public override async Task Select_scalar_primitive_after_take(bool isAsync)
        {
            await base.Select_scalar_primitive_after_take(isAsync);

            AssertSql(
                @"@__p_0='9'

SELECT TOP(@__p_0) [e].[EmployeeID]
FROM [Employees] AS [e]");
        }

        public override async Task Select_project_filter(bool isAsync)
        {
            await base.Select_project_filter(isAsync);

            AssertSql(
                @"SELECT [c].[CompanyName]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'");
        }

        public override async Task Select_project_filter2(bool isAsync)
        {
            await base.Select_project_filter2(isAsync);

            AssertSql(
                @"SELECT [c].[City]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'");
        }

        public override async Task Select_nested_collection(bool isAsync)
        {
            await base.Select_nested_collection(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[CustomerID]",
                //
                @"@_outer_CustomerID='AROUT' (Size = 5)

SELECT [o].[OrderID]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = @_outer_CustomerID) AND (DATEPART(year, [o].[OrderDate]) = 1997)
ORDER BY [o].[OrderID]",
                //
                @"@_outer_CustomerID='BSBEV' (Size = 5)

SELECT [o].[OrderID]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = @_outer_CustomerID) AND (DATEPART(year, [o].[OrderDate]) = 1997)
ORDER BY [o].[OrderID]",
                //
                @"@_outer_CustomerID='CONSH' (Size = 5)

SELECT [o].[OrderID]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = @_outer_CustomerID) AND (DATEPART(year, [o].[OrderDate]) = 1997)
ORDER BY [o].[OrderID]",
                //
                @"@_outer_CustomerID='EASTC' (Size = 5)

SELECT [o].[OrderID]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = @_outer_CustomerID) AND (DATEPART(year, [o].[OrderDate]) = 1997)
ORDER BY [o].[OrderID]",
                //
                @"@_outer_CustomerID='NORTS' (Size = 5)

SELECT [o].[OrderID]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = @_outer_CustomerID) AND (DATEPART(year, [o].[OrderDate]) = 1997)
ORDER BY [o].[OrderID]",
                //
                @"@_outer_CustomerID='SEVES' (Size = 5)

SELECT [o].[OrderID]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = @_outer_CustomerID) AND (DATEPART(year, [o].[OrderDate]) = 1997)
ORDER BY [o].[OrderID]");
        }

        public override void Select_nested_collection_multi_level()
        {
            base.Select_nested_collection_multi_level();

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')",
                //
                @"@_outer_CustomerID='ALFKI' (Size = 5)

SELECT TOP(3) [o].[OrderDate] AS [Date]
FROM [Orders] AS [o]
WHERE ([o].[OrderID] < 10500) AND (@_outer_CustomerID = [o].[CustomerID])",
                //
                @"@_outer_CustomerID='ANATR' (Size = 5)

SELECT TOP(3) [o].[OrderDate] AS [Date]
FROM [Orders] AS [o]
WHERE ([o].[OrderID] < 10500) AND (@_outer_CustomerID = [o].[CustomerID])",
                //
                @"@_outer_CustomerID='ANTON' (Size = 5)

SELECT TOP(3) [o].[OrderDate] AS [Date]
FROM [Orders] AS [o]
WHERE ([o].[OrderID] < 10500) AND (@_outer_CustomerID = [o].[CustomerID])",
                //
                @"@_outer_CustomerID='AROUT' (Size = 5)

SELECT TOP(3) [o].[OrderDate] AS [Date]
FROM [Orders] AS [o]
WHERE ([o].[OrderID] < 10500) AND (@_outer_CustomerID = [o].[CustomerID])");
        }

        public override void Select_nested_collection_multi_level2()
        {
            base.Select_nested_collection_multi_level2();

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE ([o].[OrderID] < 10500) AND ([c].[CustomerID] = [o].[CustomerID])
) AS [OrderDates]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')");
        }

        public override void Select_nested_collection_multi_level3()
        {
            base.Select_nested_collection_multi_level3();

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE ([o].[OrderID] < 10500) AND ([c].[CustomerID] = [o].[CustomerID])
) AS [OrderDates]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')");
        }

        public override void Select_nested_collection_multi_level4()
        {
            base.Select_nested_collection_multi_level4();

            AssertSql(
                @"SELECT COALESCE((
    SELECT TOP(1) (
        SELECT COUNT(*)
        FROM [Order Details] AS [od]
        WHERE ([od].[OrderID] > 10) AND ([o].[OrderID] = [od].[OrderID])
    )
    FROM [Orders] AS [o]
    WHERE ([o].[OrderID] < 10500) AND ([c].[CustomerID] = [o].[CustomerID])
), 0) AS [Order]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')");
        }

        public override void Select_nested_collection_multi_level5()
        {
            base.Select_nested_collection_multi_level5();

            AssertSql(
                @"SELECT COALESCE((
    SELECT TOP(1) (
        SELECT TOP(1) [od].[ProductID]
        FROM [Order Details] AS [od]
        WHERE ([od].[OrderID] <> (
            SELECT COUNT(*)
            FROM [Orders] AS [o0]
            WHERE [c].[CustomerID] = [o0].[CustomerID]
        )) AND ([o].[OrderID] = [od].[OrderID])
    )
    FROM [Orders] AS [o]
    WHERE ([o].[OrderID] < 10500) AND ([c].[CustomerID] = [o].[CustomerID])
), 0) AS [Order]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')");
        }

        public override void Select_nested_collection_multi_level6()
        {
            base.Select_nested_collection_multi_level6();

            AssertSql(
                @"SELECT COALESCE((
    SELECT TOP(1) (
        SELECT TOP(1) [od].[ProductID]
        FROM [Order Details] AS [od]
        WHERE ([od].[OrderID] <> CAST(LEN([c].[CustomerID]) AS int)) AND ([o].[OrderID] = [od].[OrderID])
    )
    FROM [Orders] AS [o]
    WHERE ([o].[OrderID] < 10500) AND ([c].[CustomerID] = [o].[CustomerID])
), 0) AS [Order]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')");
        }

        public override async Task Select_nested_collection_count_using_anonymous_type(bool isAsync)
        {
            await base.Select_nested_collection_count_using_anonymous_type(isAsync);

            AssertSql(
                @"SELECT (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [Count]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')");
        }

        public override async Task New_date_time_in_anonymous_type_works(bool isAsync)
        {
            await base.New_date_time_in_anonymous_type_works(isAsync);

            AssertSql(
                @"SELECT 1
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')");
        }

        public override async Task Select_non_matching_value_types_int_to_long_introduces_explicit_cast(bool isAsync)
        {
            await base.Select_non_matching_value_types_int_to_long_introduces_explicit_cast(isAsync);

            AssertSql(
                @"SELECT CAST([o].[OrderID] AS bigint)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]");
        }

        public override async Task Select_non_matching_value_types_nullable_int_to_long_introduces_explicit_cast(bool isAsync)
        {
            await base.Select_non_matching_value_types_nullable_int_to_long_introduces_explicit_cast(isAsync);

            AssertSql(
                @"SELECT CAST([o].[EmployeeID] AS bigint)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]");
        }

        public override async Task Select_non_matching_value_types_nullable_int_to_int_doesnt_introduce_explicit_cast(bool isAsync)
        {
            await base.Select_non_matching_value_types_nullable_int_to_int_doesnt_introduce_explicit_cast(isAsync);

            AssertSql(
                @"SELECT [o].[EmployeeID]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]");
        }

        public override async Task Select_non_matching_value_types_int_to_nullable_int_doesnt_introduce_explicit_cast(bool isAsync)
        {
            await base.Select_non_matching_value_types_int_to_nullable_int_doesnt_introduce_explicit_cast(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]");
        }

        public override async Task Select_non_matching_value_types_from_binary_expression_introduces_explicit_cast(bool isAsync)
        {
            await base.Select_non_matching_value_types_from_binary_expression_introduces_explicit_cast(isAsync);

            AssertSql(
                @"SELECT CAST([o].[OrderID] + [o].[OrderID] AS bigint)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]");
        }

        public override async Task Select_non_matching_value_types_from_binary_expression_nested_introduces_top_level_explicit_cast(bool isAsync)
        {
            await base.Select_non_matching_value_types_from_binary_expression_nested_introduces_top_level_explicit_cast(isAsync);

            AssertSql(
                @"SELECT CAST([o].[OrderID] + [o].[OrderID] AS smallint)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]");
        }

        public override async Task Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast1(bool isAsync)
        {
            await base.Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast1(isAsync);

            AssertSql(
                @"SELECT CAST(-[o].[OrderID] AS bigint)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]");
        }

        public override async Task Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast2(bool isAsync)
        {
            await base.Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast2(isAsync);

            AssertSql(
                @"SELECT -CAST([o].[OrderID] AS bigint)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]");
        }

        public override async Task Select_non_matching_value_types_from_length_introduces_explicit_cast(bool isAsync)
        {
            await base.Select_non_matching_value_types_from_length_introduces_explicit_cast(isAsync);

            AssertSql(
                @"SELECT CAST(CAST(LEN([o].[CustomerID]) AS int) AS bigint)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]");
        }

        public override async Task Select_non_matching_value_types_from_method_call_introduces_explicit_cast(bool isAsync)
        {
            await base.Select_non_matching_value_types_from_method_call_introduces_explicit_cast(isAsync);

            AssertSql(
                @"SELECT CAST(ABS([o].[OrderID]) AS bigint)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]");
        }

        public override async Task Select_non_matching_value_types_from_anonymous_type_introduces_explicit_cast(bool isAsync)
        {
            await base.Select_non_matching_value_types_from_anonymous_type_introduces_explicit_cast(isAsync);

            AssertSql(
                @"SELECT CAST([o].[OrderID] AS bigint) AS [LongOrder], CAST([o].[OrderID] AS smallint) AS [ShortOrder], [o].[OrderID] AS [Order]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [Order]");
        }

        public override async Task Select_conditional_with_null_comparison_in_test(bool isAsync)
        {
            await base.Select_conditional_with_null_comparison_in_test(isAsync);

            AssertSql(
                @"SELECT CASE
    WHEN [o].[CustomerID] IS NULL OR ([o].[CustomerID] IS NOT NULL AND ([o].[OrderID] < 100))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'");
        }

        public override async Task Projection_in_a_subquery_should_be_liftable(bool isAsync)
        {
            await base.Projection_in_a_subquery_should_be_liftable(isAsync);

            AssertSql(
                @"@__p_0='1'

SELECT [e].[EmployeeID]
FROM [Employees] AS [e]
ORDER BY [e].[EmployeeID]
OFFSET @__p_0 ROWS");
        }

        public override async Task Projection_containing_DateTime_subtraction(bool isAsync)
        {
            await base.Projection_containing_DateTime_subtraction(isAsync);

            AssertSql(
                @"SELECT [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300");
        }

        public override async Task Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault(bool isAsync)
        {
            await base.Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault(isAsync);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [t].[CustomerID]
    FROM (
        SELECT TOP(1) [o].[CustomerID], [o].[OrderID]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderID]
    ) AS [t]
    ORDER BY [t].[OrderID]
)
FROM [Customers] AS [c]");
        }

        public override async Task Project_single_element_from_collection_with_OrderBy_Skip_and_FirstOrDefault(bool isAsync)
        {
            await base.Project_single_element_from_collection_with_OrderBy_Skip_and_FirstOrDefault(isAsync);

            AssertSql(
                @"SELECT (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]
    OFFSET 1 ROWS FETCH NEXT 1 ROWS ONLY
)
FROM [Customers] AS [c]");
        }

        public override async Task Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault(bool isAsync)
        {
            await base.Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault(isAsync);

            AssertSql(
                @"SELECT (
    SELECT DISTINCT TOP(1) [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
)
FROM [Customers] AS [c]");
        }

        public override async Task Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault_followed_by_projecting_length(bool isAsync)
        {
            await base.Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault_followed_by_projecting_length(isAsync);

            AssertSql(
                "");
        }

        public override async Task Project_single_element_from_collection_with_OrderBy_Take_and_SingleOrDefault(bool isAsync)
        {
            await base.Project_single_element_from_collection_with_OrderBy_Take_and_SingleOrDefault(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'",
                //
                @"@_outer_CustomerID='ALFKI' (Size = 5)

SELECT TOP(2) [t0].[CustomerID]
FROM (
    SELECT TOP(1) [o0].[CustomerID], [o0].[OrderID]
    FROM [Orders] AS [o0]
    WHERE @_outer_CustomerID = [o0].[CustomerID]
    ORDER BY [o0].[OrderID]
) AS [t0]
ORDER BY [t0].[OrderID]");
        }

        public override async Task Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault_with_parameter(bool isAsync)
        {
            await base.Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault_with_parameter(isAsync);

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
    ORDER BY [t].[OrderID]
)
FROM [Customers] AS [c]");
        }

        public override async Task Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault(bool isAsync)
        {
            await base.Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault(isAsync);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [t].[CustomerID]
    FROM (
        SELECT TOP(2) [o].[CustomerID], [o].[OrderID], [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderID], [o].[OrderDate] DESC
    ) AS [t]
    ORDER BY [t].[OrderID], [t].[OrderDate] DESC
)
FROM [Customers] AS [c]");
        }

        public override async Task Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_followed_by_projection_of_length_property(bool isAsync)
        {
            await base.Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_followed_by_projection_of_length_property(isAsync);

            AssertSql(
                "");
        }

        public override async Task Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_2(bool isAsync)
        {
            await base.Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_2(isAsync);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [t].[CustomerID]
    FROM (
        SELECT TOP(2) [o].[CustomerID], [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[CustomerID], [o].[OrderDate] DESC
    ) AS [t]
    ORDER BY [t].[CustomerID], [t].[OrderDate] DESC
)
FROM [Customers] AS [c]");
        }

        public override async Task Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault(bool isAsync)
        {
            await base.Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault(isAsync);

            AssertSql(
                @"SELECT COALESCE((
    SELECT TOP(1) [t].[OrderID]
    FROM (
        SELECT TOP(1) [od].[OrderID], [od.Product].[ProductName]
        FROM [Order Details] AS [od]
        INNER JOIN [Products] AS [od.Product] ON [od].[ProductID] = [od.Product].[ProductID]
        WHERE [o].[OrderID] = [od].[OrderID]
        ORDER BY [od.Product].[ProductName]
    ) AS [t]
    ORDER BY [t].[ProductName]
), 0)
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300");
        }

        public override async Task Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(bool isAsync)
        {
            await base.Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10250",
                //
                @"@_outer_OrderID='10248'

SELECT TOP(1) [t].*
FROM (
    SELECT TOP(1) [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice], [od.Product].[ProductName]
    FROM [Order Details] AS [od]
    INNER JOIN [Products] AS [od.Product] ON [od].[ProductID] = [od.Product].[ProductID]
    WHERE @_outer_OrderID = [od].[OrderID]
    ORDER BY [od.Product].[ProductName]
) AS [t]
ORDER BY [t].[ProductName]",
                //
                @"@_outer_OrderID='10249'

SELECT TOP(1) [t].*
FROM (
    SELECT TOP(1) [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice], [od.Product].[ProductName]
    FROM [Order Details] AS [od]
    INNER JOIN [Products] AS [od.Product] ON [od].[ProductID] = [od.Product].[ProductID]
    WHERE @_outer_OrderID = [od].[OrderID]
    ORDER BY [od.Product].[ProductName]
) AS [t]
ORDER BY [t].[ProductName]");
        }

        public override async Task Select_datetime_year_component(bool isAsync)
        {
            await base.Select_datetime_year_component(isAsync);

            AssertSql(
                @"SELECT DATEPART(year, [o].[OrderDate])
FROM [Orders] AS [o]");
        }

        public override async Task Select_datetime_month_component(bool isAsync)
        {
            await base.Select_datetime_month_component(isAsync);

            AssertSql(
                @"SELECT DATEPART(month, [o].[OrderDate])
FROM [Orders] AS [o]");
        }

        public override async Task Select_datetime_day_of_year_component(bool isAsync)
        {
            await base.Select_datetime_day_of_year_component(isAsync);

            AssertSql(
                @"SELECT DATEPART(dayofyear, [o].[OrderDate])
FROM [Orders] AS [o]");
        }

        public override async Task Select_datetime_day_component(bool isAsync)
        {
            await base.Select_datetime_day_component(isAsync);

            AssertSql(
                @"SELECT DATEPART(day, [o].[OrderDate])
FROM [Orders] AS [o]");
        }

        public override async Task Select_datetime_hour_component(bool isAsync)
        {
            await base.Select_datetime_hour_component(isAsync);

            AssertSql(
                @"SELECT DATEPART(hour, [o].[OrderDate])
FROM [Orders] AS [o]");
        }

        public override async Task Select_datetime_minute_component(bool isAsync)
        {
            await base.Select_datetime_minute_component(isAsync);

            AssertSql(
                @"SELECT DATEPART(minute, [o].[OrderDate])
FROM [Orders] AS [o]");
        }

        public override async Task Select_datetime_second_component(bool isAsync)
        {
            await base.Select_datetime_second_component(isAsync);

            AssertSql(
                @"SELECT DATEPART(second, [o].[OrderDate])
FROM [Orders] AS [o]");
        }

        public override async Task Select_datetime_millisecond_component(bool isAsync)
        {
            await base.Select_datetime_millisecond_component(isAsync);

            AssertSql(
                @"SELECT DATEPART(millisecond, [o].[OrderDate])
FROM [Orders] AS [o]");
        }

        public override async Task Select_byte_constant(bool isAsync)
        {
            await base.Select_byte_constant(isAsync);

            AssertSql(
                @"SELECT CASE
    WHEN [c].[CustomerID] = N'ALFKI'
    THEN CAST(1 AS tinyint) ELSE CAST(2 AS tinyint)
END
FROM [Customers] AS [c]");
        }

        public override async Task Select_short_constant(bool isAsync)
        {
            await base.Select_short_constant(isAsync);

            AssertSql(
                @"SELECT CASE
    WHEN [c].[CustomerID] = N'ALFKI'
    THEN CAST(1 AS smallint) ELSE CAST(2 AS smallint)
END
FROM [Customers] AS [c]");
        }

        public override async Task Select_bool_constant(bool isAsync)
        {
            await base.Select_bool_constant(isAsync);

            AssertSql(
                @"SELECT CASE
    WHEN [c].[CustomerID] = N'ALFKI'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
FROM [Customers] AS [c]");
        }

        public override async Task Anonymous_projection_AsNoTracking_Selector(bool isAsync)
        {
            await base.Anonymous_projection_AsNoTracking_Selector(isAsync);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [A], [o].[OrderDate] AS [B]
FROM [Orders] AS [o]");
        }

        public override async Task Anonymous_projection_with_repeated_property_being_ordered(bool isAsync)
        {
            await base.Anonymous_projection_with_repeated_property_being_ordered(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID] AS [B]
FROM [Customers] AS [c]
ORDER BY [B]");
        }

        public override async Task Anonymous_projection_with_repeated_property_being_ordered_2(bool isAsync)
        {
            await base.Anonymous_projection_with_repeated_property_being_ordered_2(isAsync);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [B]
FROM [Orders] AS [o]
ORDER BY [B]");
        }

        public override async Task Select_GetValueOrDefault_on_DateTime(bool isAsync)
        {
            await base.Select_GetValueOrDefault_on_DateTime(isAsync);

            AssertSql(
                @"SELECT [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override async Task Select_GetValueOrDefault_on_DateTime_with_null_values(bool isAsync)
        {
            await base.Select_GetValueOrDefault_on_DateTime_with_null_values(isAsync);

            AssertSql(
                @"SELECT COALESCE([o].[OrderDate], '1753-01-01T00:00:00.000')
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override async Task Cast_on_top_level_projection_brings_explicit_Cast(bool isAsync)
        {
            await base.Cast_on_top_level_projection_brings_explicit_Cast(isAsync);

            AssertSql(
                @"SELECT CAST([o].[OrderID] AS float)
FROM [Orders] AS [o]");
        }
    }
}
