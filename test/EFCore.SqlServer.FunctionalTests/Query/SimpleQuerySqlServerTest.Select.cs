// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class SimpleQuerySqlServerTest
    {
        public override async Task Projection_when_arithmetic_expression_precedence(bool isAsync)
        {
            await base.Projection_when_arithmetic_expression_precedence(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID] / ([o].[OrderID] / 2) AS [A], ([o].[OrderID] / [o].[OrderID]) / 2 AS [B]
FROM [Orders] AS [o]");
        }

        public override async Task Projection_when_arithmetic_expressions(bool isAsync)
        {
            await base.Projection_when_arithmetic_expressions(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[OrderID] * 2 AS [Double], [o].[OrderID] + 23 AS [Add], 100000 - [o].[OrderID] AS [Sub], [o].[OrderID] / ([o].[OrderID] / 2) AS [Divide], 42 AS [Literal], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override async Task Projection_when_arithmetic_mixed(bool isAsync)
        {
            await base.Projection_when_arithmetic_mixed(isAsync);

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
                @"SELECT [c].[CustomerID], [o].[CustomerID], [o].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID], [o].[OrderID]");
        }

        public override async Task Project_to_object_array(bool isAsync)
        {
            await base.Project_to_object_array(isAsync);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = 1");
        }

        public override async Task Projection_of_entity_type_into_object_array(bool isAsync)
        {
            await base.Projection_of_entity_type_into_object_array(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID]");
        }

        public override async Task Projection_of_multiple_entity_types_into_object_array(bool isAsync)
        {
            await base.Projection_of_multiple_entity_types_into_object_array(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] < 10300
ORDER BY [o].[OrderID]");
        }

        public override async Task Projection_of_entity_type_into_object_list(bool isAsync)
        {
            await base.Projection_of_entity_type_into_object_list(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Project_to_int_array(bool isAsync)
        {
            await base.Project_to_int_array(isAsync);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[ReportsTo]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = 1");
        }

        public override async Task Select_bool_closure_with_order_parameter_with_cast_to_nullable(bool isAsync)
        {
            await base.Select_bool_closure_with_order_parameter_with_cast_to_nullable(isAsync);

            AssertSql(
                @"@__boolean_0='False'

SELECT @__boolean_0
FROM [Customers] AS [c]");
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
                @"SELECT [c].[CustomerID], CAST(1 AS bit) AS [ConstantTrue]
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
    WHEN [p].[UnitsInStock] > CAST(0 AS smallint) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsAvailable]
FROM [Products] AS [p]");
        }

        public override async Task Select_constant_int(bool isAsync)
        {
            await base.Select_constant_int(isAsync);

            AssertSql(
                @"SELECT 0
FROM [Customers] AS [c]");
        }

        public override async Task Select_constant_null_string(bool isAsync)
        {
            await base.Select_constant_null_string(isAsync);

            AssertSql(
                @"SELECT NULL
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
                @"SELECT [c].[CustomerID], [t].[OrderDate], [t].[OrderID]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT TOP(3) [o].[OrderDate], [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE ([c].[CustomerID] = [o].[CustomerID]) AND ([o].[OrderID] < 10500)
) AS [t]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID], [t].[OrderID]");
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
                @"SELECT (
    SELECT TOP(1) (
        SELECT COUNT(*)
        FROM [Order Details] AS [o]
        WHERE ([o0].[OrderID] = [o].[OrderID]) AND ([o].[OrderID] > 10))
    FROM [Orders] AS [o0]
    WHERE ([c].[CustomerID] = [o0].[CustomerID]) AND ([o0].[OrderID] < 10500)) AS [Order]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'");
        }

        public override void Select_nested_collection_multi_level5()
        {
            base.Select_nested_collection_multi_level5();

            AssertSql(
                @"SELECT (
    SELECT TOP(1) (
        SELECT TOP(1) [o].[ProductID]
        FROM [Order Details] AS [o]
        WHERE ([o1].[OrderID] = [o].[OrderID]) AND (([o].[OrderID] <> (
            SELECT COUNT(*)
            FROM [Orders] AS [o0]
            WHERE [c].[CustomerID] = [o0].[CustomerID])) OR (
            SELECT COUNT(*)
            FROM [Orders] AS [o0]
            WHERE [c].[CustomerID] = [o0].[CustomerID]) IS NULL))
    FROM [Orders] AS [o1]
    WHERE ([c].[CustomerID] = [o1].[CustomerID]) AND ([o1].[OrderID] < 10500)) AS [Order]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'");
        }

        public override void Select_nested_collection_multi_level6()
        {
            base.Select_nested_collection_multi_level6();

            AssertSql(
                @"SELECT (
    SELECT TOP(1) (
        SELECT TOP(1) [o].[ProductID]
        FROM [Order Details] AS [o]
        WHERE ([o0].[OrderID] = [o].[OrderID]) AND (([o].[OrderID] <> CAST(LEN([c].[CustomerID]) AS int)) OR LEN([c].[CustomerID]) IS NULL))
    FROM [Orders] AS [o0]
    WHERE ([c].[CustomerID] = [o0].[CustomerID]) AND ([o0].[OrderID] < 10500)) AS [Order]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'");
        }

        public override async Task Select_nested_collection_count_using_anonymous_type(bool isAsync)
        {
            await base.Select_nested_collection_count_using_anonymous_type(isAsync);

            AssertSql(
                @"SELECT (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]) AS [Count]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'");
        }

        public override async Task New_date_time_in_anonymous_type_works(bool isAsync)
        {
            await base.New_date_time_in_anonymous_type_works(isAsync);

            AssertSql(
                @"SELECT 1
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'");
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
                @"SELECT CAST(([o].[OrderID] + [o].[OrderID]) AS bigint)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID]");
        }

        public override async Task Select_non_matching_value_types_from_binary_expression_nested_introduces_top_level_explicit_cast(
            bool isAsync)
        {
            await base.Select_non_matching_value_types_from_binary_expression_nested_introduces_top_level_explicit_cast(isAsync);

            AssertSql(
                @"SELECT CAST((CAST([o].[OrderID] AS bigint) + CAST([o].[OrderID] AS bigint)) AS smallint)
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
ORDER BY [o].[OrderID]");
        }

        public override async Task Select_conditional_with_null_comparison_in_test(bool isAsync)
        {
            await base.Select_conditional_with_null_comparison_in_test(isAsync);

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
    ORDER BY [t].[OrderID])
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
    OFFSET 1 ROWS FETCH NEXT 1 ROWS ONLY)
FROM [Customers] AS [c]");
        }

        public override async Task Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault(bool isAsync)
        {
            await base.Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault(isAsync);

            AssertSql(
                @"SELECT (
    SELECT DISTINCT TOP(1) [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
FROM [Customers] AS [c]");
        }

        public override async Task
            Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault_followed_by_projecting_length(bool isAsync)
        {
            await base.Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault_followed_by_projecting_length(
                isAsync);

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

        public override async Task Project_single_element_from_collection_with_OrderBy_Take_and_SingleOrDefault(bool isAsync)
        {
            await base.Project_single_element_from_collection_with_OrderBy_Take_and_SingleOrDefault(isAsync);

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
    ORDER BY [t].[OrderID])
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
    ORDER BY [t].[OrderID], [t].[OrderDate] DESC)
FROM [Customers] AS [c]");
        }

        public override async Task
            Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_followed_by_projection_of_length_property(
                bool isAsync)
        {
            await base
                .Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_followed_by_projection_of_length_property(
                    isAsync);

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
        SELECT TOP(2) [o].[CustomerID], [o].[OrderID], [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[CustomerID], [o].[OrderDate] DESC
    ) AS [t]
    ORDER BY [t].[CustomerID], [t].[OrderDate] DESC)
FROM [Customers] AS [c]");
        }

        public override async Task Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault(bool isAsync)
        {
            await base.Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault(isAsync);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [t].[OrderID]
    FROM (
        SELECT TOP(1) [o].[OrderID], [o].[ProductID], [p].[ProductID] AS [ProductID0], [p].[ProductName]
        FROM [Order Details] AS [o]
        INNER JOIN [Products] AS [p] ON [o].[ProductID] = [p].[ProductID]
        WHERE [o0].[OrderID] = [o].[OrderID]
        ORDER BY [p].[ProductName]
    ) AS [t]
    ORDER BY [t].[ProductName])
FROM [Orders] AS [o0]
WHERE [o0].[OrderID] < 10300");
        }

        public override async Task Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(
            bool isAsync)
        {
            await base.Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(isAsync);

            AssertSql(
                @"SELECT [t0].[OrderID], [t0].[ProductID], [t0].[Discount], [t0].[Quantity], [t0].[UnitPrice]
FROM [Orders] AS [o]
OUTER APPLY (
    SELECT TOP(1) [t].[OrderID], [t].[ProductID], [t].[Discount], [t].[Quantity], [t].[UnitPrice], [t].[ProductID0], [t].[ProductName]
    FROM (
        SELECT TOP(1) [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [p].[ProductID] AS [ProductID0], [p].[ProductName]
        FROM [Order Details] AS [o0]
        INNER JOIN [Products] AS [p] ON [o0].[ProductID] = [p].[ProductID]
        WHERE [o].[OrderID] = [o0].[OrderID]
        ORDER BY [p].[ProductName]
    ) AS [t]
    ORDER BY [t].[ProductName]
) AS [t0]
WHERE [o].[OrderID] < 10250");
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
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS tinyint)
    ELSE CAST(2 AS tinyint)
END
FROM [Customers] AS [c]");
        }

        public override async Task Select_short_constant(bool isAsync)
        {
            await base.Select_short_constant(isAsync);

            AssertSql(
                @"SELECT CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS smallint)
    ELSE CAST(2 AS smallint)
END
FROM [Customers] AS [c]");
        }

        public override async Task Select_bool_constant(bool isAsync)
        {
            await base.Select_bool_constant(isAsync);

            AssertSql(
                @"SELECT CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Customers] AS [c]");
        }

        public override async Task Anonymous_projection_AsNoTracking_Selector(bool isAsync)
        {
            await base.Anonymous_projection_AsNoTracking_Selector(isAsync);

            AssertSql(
                @"SELECT [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override async Task Anonymous_projection_with_repeated_property_being_ordered(bool isAsync)
        {
            await base.Anonymous_projection_with_repeated_property_being_ordered(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID] AS [A]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override async Task Anonymous_projection_with_repeated_property_being_ordered_2(bool isAsync)
        {
            await base.Anonymous_projection_with_repeated_property_being_ordered_2(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID] AS [A], [o].[CustomerID] AS [B]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [o].[CustomerID]");
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

        public override async Task Projecting_nullable_struct(bool isAsync)
        {
            await base.Projecting_nullable_struct(isAsync);

            AssertSql(
                @"SELECT [o].[CustomerID], CASE
    WHEN ([o].[CustomerID] = N'ALFKI') AND [o].[CustomerID] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [o].[OrderID], CAST(LEN([o].[CustomerID]) AS int)
FROM [Orders] AS [o]");
        }

        public override async Task Multiple_select_many_with_predicate(bool isAsync)
        {
            await base.Multiple_select_many_with_predicate(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
INNER JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE CAST([o0].[Discount] AS float) >= 0.25E0");
        }

        public override async Task SelectMany_without_result_selector_naked_collection_navigation(bool isAsync)
        {
            await base.SelectMany_without_result_selector_naked_collection_navigation(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override async Task SelectMany_without_result_selector_collection_navigation_composed(bool isAsync)
        {
            await base.SelectMany_without_result_selector_collection_navigation_composed(isAsync);

            AssertSql(
                @"SELECT [o].[CustomerID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override async Task SelectMany_correlated_with_outer_1(bool isAsync)
        {
            await base.SelectMany_correlated_with_outer_1(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[City] AS [o]
FROM [Customers] AS [c]
CROSS APPLY (
    SELECT [c].[City], [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [t]");
        }

        public override async Task SelectMany_correlated_with_outer_2(bool isAsync)
        {
            await base.SelectMany_correlated_with_outer_2(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM [Customers] AS [c]
CROSS APPLY (
    SELECT TOP(2) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[City]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [c].[City], [o].[OrderID]
) AS [t]");
        }

        public override async Task SelectMany_correlated_with_outer_3(bool isAsync)
        {
            await base.SelectMany_correlated_with_outer_3(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[City] AS [o]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT [c].[City], [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [t]");
        }

        public override async Task SelectMany_correlated_with_outer_4(bool isAsync)
        {
            await base.SelectMany_correlated_with_outer_4(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT TOP(2) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[City]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [c].[City], [o].[OrderID]
) AS [t]");
        }

        public override async Task FirstOrDefault_over_empty_collection_of_value_type_returns_correct_results(bool isAsync)
        {
            await base.FirstOrDefault_over_empty_collection_of_value_type_returns_correct_results(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], (
    SELECT TOP(1) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]) AS [OrderId]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'FISSA'");
        }

        public override Task Member_binding_after_ctor_arguments_fails_with_client_eval(bool isAsync)
        {
            return AssertTranslationFailed(() => base.Member_binding_after_ctor_arguments_fails_with_client_eval(isAsync));
        }

        public override async Task Filtered_collection_projection_is_tracked(bool isAsync)
        {
            await base.Filtered_collection_projection_is_tracked(isAsync);

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

        public override async Task Filtered_collection_projection_with_to_list_is_tracked(bool isAsync)
        {
            await base.Filtered_collection_projection_with_to_list_is_tracked(isAsync);

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
            bool isAsync)
        {
            await base.SelectMany_with_collection_being_correlated_subquery_which_references_inner_and_outer_entity(isAsync);

            AssertSql(
                @"SELECT [t].[CustomerID] AS [OrderProperty], [t].[CustomerID0] AS [CustomerProperty]
FROM [Customers] AS [c]
CROSS APPLY (
    SELECT [o].[CustomerID], [c].[CustomerID] AS [CustomerID0], [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [t]");
        }

        public override async Task
            SelectMany_with_collection_being_correlated_subquery_which_references_non_mapped_properties_from_inner_and_outer_entity(
                bool isAsync)
        {
            await base
                .SelectMany_with_collection_being_correlated_subquery_which_references_non_mapped_properties_from_inner_and_outer_entity(
                    isAsync);

            AssertSql(
                @"");
        }

        public override async Task Select_with_complex_expression_that_can_be_funcletized(bool isAsync)
        {
            await base.Select_with_complex_expression_that_can_be_funcletized(isAsync);

            AssertSql(
                @"SELECT CASE
    WHEN N'' = N'' THEN 0
    ELSE CHARINDEX(N'', [c].[ContactName]) - 1
END
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task Select_chained_entity_navigation_doesnt_materialize_intermittent_entities(bool isAsync)
        {
            await base.Select_chained_entity_navigation_doesnt_materialize_intermittent_entities(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
LEFT JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
ORDER BY [o].[OrderID], [o0].[OrderID]");
        }

        public override async Task Select_entity_compared_to_null(bool isAsync)
        {
            await base.Select_entity_compared_to_null(isAsync);

            AssertSql(
                @"SELECT CASE
    WHEN [c].[CustomerID] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[CustomerID] = N'ALFKI'");
        }

        public override async Task Explicit_cast_in_arithmatic_operation_is_preserved(bool isAsync)
        {
            await base.Explicit_cast_in_arithmatic_operation_is_preserved(isAsync);

            AssertSql(
                @"SELECT CAST([o].[OrderID] AS decimal(18,2)) / CAST(([o].[OrderID] + 1000) AS decimal(18,2))
FROM [Orders] AS [o]
WHERE [o].[OrderID] = 10243");
        }

        public override async Task SelectMany_whose_selector_references_outer_source(bool isAsync)
        {
            await base.SelectMany_whose_selector_references_outer_source(isAsync);

            AssertSql(
                @"SELECT [t].[OrderDate], [t].[City] AS [CustomerCity]
FROM [Customers] AS [c]
CROSS APPLY (
    SELECT [o].[OrderDate], [c].[City], [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [t]");
        }

        public override async Task Collection_FirstOrDefault_with_entity_equality_check_in_projection(bool isAsync)
        {
            await base.Collection_FirstOrDefault_with_entity_equality_check_in_projection(isAsync);

            AssertSql(" ");
        }

        public override async Task Collection_FirstOrDefault_with_nullable_unsigned_int_column(bool isAsync)
        {
            await base.Collection_FirstOrDefault_with_nullable_unsigned_int_column(isAsync);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [o].[EmployeeID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID])
FROM [Customers] AS [c]");
        }

        public override async Task ToList_Count_in_projection_works(bool isAsync)
        {
            await base.ToList_Count_in_projection_works(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]) AS [Count]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'");
        }

        public override async Task LastOrDefault_member_access_in_projection_translates_to_server(bool isAsync)
        {
            await base.LastOrDefault_member_access_in_projection_translates_to_server(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], (
    SELECT TOP(1) [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]) AS [OrderDate]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'");
        }
    }
}
