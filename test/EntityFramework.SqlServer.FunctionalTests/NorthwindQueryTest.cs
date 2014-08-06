// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Advanced;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;

#if K10
using System.Threading;
#endif

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    // TODO: Test non-SqlServer SQL (i.e. generated from Relational base) elsewhere
    public class NorthwindQueryTest : NorthwindQueryTestBase, IClassFixture<NorthwindQueryFixture>
    {
        public override void Count_with_predicate()
        {
            base.Count_with_predicate();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM [Orders] AS o
WHERE o.[CustomerID] = @p0",
                _fixture.Sql);
        }

        public override void Distinct_Count()
        {
            base.Distinct_Count();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
    FROM [Customers] AS c
) AS t0",
                _fixture.Sql);
        }

        [Fact]
        public override void Select_Distinct_Count()
        {
            base.Select_Distinct_Count();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT c.[City]
    FROM [Customers] AS c
) AS t0",
                _fixture.Sql);
        }

        public override void Take_Distinct_Count()
        {
            base.Take_Distinct_Count();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT *
    FROM (
        SELECT TOP 5 o.[CustomerID], o.[OrderDate], o.[OrderID]
        FROM [Orders] AS o
    ) AS t0
) AS t1",
                _fixture.Sql);
        }

        public override void Queryable_simple()
        {
            base.Queryable_simple();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c",
                _fixture.Sql);
        }

        public override void Queryable_simple_anonymous()
        {
            base.Queryable_simple_anonymous();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c",
                _fixture.Sql);
        }

        public override void Queryable_nested_simple()
        {
            base.Queryable_nested_simple();

            Assert.Equal(
                @"SELECT c3.[Address], c3.[City], c3.[CompanyName], c3.[ContactName], c3.[ContactTitle], c3.[Country], c3.[CustomerID], c3.[Fax], c3.[Phone], c3.[PostalCode], c3.[Region]
FROM [Customers] AS c3",
                _fixture.Sql);
        }

        public override void Take_simple()
        {
            base.Take_simple();

            Assert.Equal(
                @"SELECT TOP 10 c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c
ORDER BY c.[CustomerID]",
                _fixture.Sql);
        }

        public override void Take_simple_projection()
        {
            base.Take_simple_projection();

            Assert.Equal(
                @"SELECT TOP 10 c.[City]
FROM [Customers] AS c
ORDER BY c.[CustomerID]",
                _fixture.Sql);
        }

        public override void Any_simple()
        {
            base.Any_simple();

            Assert.Equal(
                @"SELECT CASE WHEN (
    EXISTS (
        SELECT 1
        FROM [Customers] AS c
    )
) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END",
                _fixture.Sql);
        }

        public override async Task Any_simple_async()
        {
            await base.Any_simple_async();

            Assert.Equal(
                @"SELECT CASE WHEN (
    EXISTS (
        SELECT 1
        FROM [Customers] AS c
    )
) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END",
                _fixture.Sql);
        }

        public override void Any_predicate()
        {
            base.Any_predicate();

            Assert.Equal(
                @"SELECT CASE WHEN (
    EXISTS (
        SELECT 1
        FROM [Customers] AS c
        WHERE c.[ContactName] LIKE @p0 + '%'
    )
) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END",
                _fixture.Sql);
        }

        public override void All_top_level()
        {
            base.All_top_level();

            Assert.Equal(
                @"SELECT CASE WHEN (
    NOT EXISTS (
        SELECT 1
        FROM [Customers] AS c
        WHERE NOT c.[ContactName] LIKE @p0 + '%'
    )
) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END",
                _fixture.Sql);
        }

        public override void Select_scalar()
        {
            base.Select_scalar();

            Assert.Equal(
                @"SELECT c.[City]
FROM [Customers] AS c",
                _fixture.Sql);
        }

        public override void Select_scalar_primitive_after_take()
        {
            base.Select_scalar_primitive_after_take();

            Assert.Equal(
                @"SELECT TOP 9 e.[City], e.[Country], e.[EmployeeID], e.[FirstName], e.[ReportsTo], e.[Title]
FROM [Employees] AS e",
                _fixture.Sql);
        }

        public override void Where_simple()
        {
            base.Where_simple();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c
WHERE c.[City] = @p0",
                _fixture.Sql);
        }

        public override void Where_simple_shadow()
        {
            base.Where_simple_shadow();

            Assert.Equal(
                 @"SELECT e.[City], e.[Country], e.[EmployeeID], e.[FirstName], e.[ReportsTo], e.[Title]
FROM [Employees] AS e
WHERE e.[Title] = @p0",
                 _fixture.Sql);
        }

        public override void Where_simple_shadow_projection()
        {
            base.Where_simple_shadow_projection();

            Assert.Equal(
                  @"SELECT e.[Title]
FROM [Employees] AS e
WHERE e.[Title] = @p0",
                  _fixture.Sql);
        }

        public override void Where_comparison_nullable_type_not_null()
        {
            base.Where_comparison_nullable_type_not_null();

            Assert.Equal(
                @"SELECT e.[City], e.[Country], e.[EmployeeID], e.[FirstName], e.[ReportsTo], e.[Title]
FROM [Employees] AS e
WHERE e.[ReportsTo] = @p0",
                _fixture.Sql);
        }

        public override void Where_comparison_nullable_type_null()
        {
            base.Where_comparison_nullable_type_null();

            Assert.Equal(
                @"SELECT e.[City], e.[Country], e.[EmployeeID], e.[FirstName], e.[ReportsTo], e.[Title]
FROM [Employees] AS e
WHERE e.[ReportsTo] IS NULL",
                _fixture.Sql);
        }

        public override void Where_client()
        {
            base.Where_client();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c",
                _fixture.Sql);
        }

        public override void First_client_predicate()
        {
            base.First_client_predicate();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c
ORDER BY c.[CustomerID]",
                _fixture.Sql);
        }

        public override void Where_equals_method_string()
        {
            base.Where_equals_method_string();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c
WHERE c.[City] = @p0",
                _fixture.Sql);
        }

        public override void Where_equals_method_int()
        {
            base.Where_equals_method_int();

            Assert.Equal(
                @"SELECT e.[City], e.[Country], e.[EmployeeID], e.[FirstName], e.[ReportsTo], e.[Title]
FROM [Employees] AS e
WHERE e.[EmployeeID] = @p0",
                _fixture.Sql);
        }

        public override void Where_string_length()
        {
            base.Where_string_length();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c",
                _fixture.Sql);
        }

        public override void Where_is_null()
        {
            base.Where_is_null();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c
WHERE c.[City] IS NULL",
                _fixture.Sql);
        }

        public override void Where_is_not_null()
        {
            base.Where_is_not_null();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c
WHERE c.[City] IS NOT NULL",
                _fixture.Sql);
        }

        public override void Where_null_is_null()
        {
            base.Where_null_is_null();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c
WHERE 1 = 1",
                _fixture.Sql);
        }

        public override void Where_constant_is_null()
        {
            base.Where_constant_is_null();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c
WHERE 1 = 0",
                _fixture.Sql);
        }

        public override void Where_null_is_not_null()
        {
            base.Where_null_is_not_null();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c
WHERE 1 = 0",
                _fixture.Sql);
        }

        public override void Where_constant_is_not_null()
        {
            base.Where_constant_is_not_null();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c
WHERE 1 = 1",
                _fixture.Sql);
        }

        public override void Where_simple_reversed()
        {
            base.Where_simple_reversed();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c
WHERE @p0 = c.[City]",
                _fixture.Sql);
        }

        public override void Where_identity_comparison()
        {
            base.Where_identity_comparison();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c
WHERE c.[City] = c.[City]",
                _fixture.Sql);
        }

        public override async Task Where_simple_async()
        {
            await base.Where_simple_async();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c
WHERE c.[City] = @p0",
                _fixture.Sql);
        }

        public override void Where_select_many_or()
        {
            base.Where_select_many_or();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region], e.[City], e.[Country], e.[EmployeeID], e.[FirstName], e.[ReportsTo], e.[Title]
FROM [Customers] AS c
CROSS JOIN [Employees] AS e
WHERE (c.[City] = @p0 OR e.[City] = @p0)",
                _fixture.Sql);
        }

        public override void Where_select_many_or2()
        {
            base.Where_select_many_or2();

            Assert.StartsWith(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region], e.[City], e.[Country], e.[EmployeeID], e.[FirstName], e.[ReportsTo], e.[Title]
FROM [Customers] AS c
CROSS JOIN [Employees] AS e
WHERE (c.[City] = @p0 OR c.[City] = @p1)",
                _fixture.Sql);
        }

        public override void Where_select_many_and()
        {
            base.Where_select_many_and();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region], e.[City], e.[Country], e.[EmployeeID], e.[FirstName], e.[ReportsTo], e.[Title]
FROM [Customers] AS c
CROSS JOIN [Employees] AS e
WHERE ((c.[City] = @p0 AND c.[Country] = @p1) AND (e.[City] = @p0 AND e.[Country] = @p1))",
                _fixture.Sql);
        }

        public override void Select_project_filter()
        {
            base.Select_project_filter();

            Assert.Equal(
                @"SELECT c.[CompanyName]
FROM [Customers] AS c
WHERE c.[City] = @p0",
                _fixture.Sql);
        }

        public override async Task Select_project_filter_async()
        {
            await base.Select_project_filter_async();

            Assert.Equal(
                @"SELECT c.[CompanyName]
FROM [Customers] AS c
WHERE c.[City] = @p0",
                _fixture.Sql);
        }

        public override void Select_project_filter2()
        {
            base.Select_project_filter2();

            Assert.Equal(
                @"SELECT c.[City]
FROM [Customers] AS c
WHERE c.[City] = @p0",
                _fixture.Sql);
        }

        public override void SelectMany_mixed()
        {
            base.SelectMany_mixed();

            Assert.Equal(3427, _fixture.Sql.Length);
            Assert.StartsWith(
                @"SELECT e1.[City], e1.[Country], e1.[EmployeeID], e1.[FirstName], e1.[ReportsTo], e1.[Title]
FROM [Employees] AS e1

SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c

SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c",
                _fixture.Sql);
        }

        public override void SelectMany_simple1()
        {
            base.SelectMany_simple1();

            Assert.Equal(
                @"SELECT e.[City], e.[Country], e.[EmployeeID], e.[FirstName], e.[ReportsTo], e.[Title], c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Employees] AS e
CROSS JOIN [Customers] AS c",
                _fixture.Sql);
        }

        public override void SelectMany_simple2()
        {
            base.SelectMany_simple2();

            Assert.Equal(
                @"SELECT e1.[City], e1.[Country], e1.[EmployeeID], e1.[FirstName], e1.[ReportsTo], e1.[Title], c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region], e2.[FirstName]
FROM [Employees] AS e1
CROSS JOIN [Customers] AS c
CROSS JOIN [Employees] AS e2",
                _fixture.Sql);
        }

        public override void SelectMany_entity_deep()
        {
            base.SelectMany_entity_deep();

            Assert.Equal(
                @"SELECT e1.[City], e1.[Country], e1.[EmployeeID], e1.[FirstName], e1.[ReportsTo], e1.[Title], e2.[City], e2.[Country], e2.[EmployeeID], e2.[FirstName], e2.[ReportsTo], e2.[Title], e3.[City], e3.[Country], e3.[EmployeeID], e3.[FirstName], e3.[ReportsTo], e3.[Title]
FROM [Employees] AS e1
CROSS JOIN [Employees] AS e2
CROSS JOIN [Employees] AS e3",
                _fixture.Sql);
        }

        public override void SelectMany_projection1()
        {
            base.SelectMany_projection1();

            Assert.Equal(
                @"SELECT e1.[City], e2.[Country]
FROM [Employees] AS e1
CROSS JOIN [Employees] AS e2",
                _fixture.Sql);
        }

        public override void SelectMany_projection2()
        {
            base.SelectMany_projection2();

            Assert.Equal(
                @"SELECT e1.[City], e2.[Country], e3.[FirstName]
FROM [Employees] AS e1
CROSS JOIN [Employees] AS e2
CROSS JOIN [Employees] AS e3",
                _fixture.Sql);
        }

        public override void Join_customers_orders_projection()
        {
            base.Join_customers_orders_projection();

            Assert.Equal(
                @"SELECT c.[ContactName], o.[OrderID]
FROM [Customers] AS c
INNER JOIN [Orders] AS o ON c.[CustomerID] = o.[CustomerID]",
                _fixture.Sql);
        }

        public override void Join_customers_orders_entities()
        {
            base.Join_customers_orders_entities();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region], o.[CustomerID], o.[OrderDate], o.[OrderID]
FROM [Customers] AS c
INNER JOIN [Orders] AS o ON c.[CustomerID] = o.[CustomerID]",
                _fixture.Sql);
        }

        public override void Join_composite_key()
        {
            base.Join_composite_key();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region], o.[CustomerID], o.[OrderDate], o.[OrderID]
FROM [Customers] AS c
INNER JOIN [Orders] AS o ON (c.[CustomerID] = o.[CustomerID] AND c.[CustomerID] = o.[CustomerID])",
                _fixture.Sql);
        }

        public override void Join_select_many()
        {
            base.Join_select_many();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region], o.[CustomerID], o.[OrderDate], o.[OrderID], e.[City], e.[Country], e.[EmployeeID], e.[FirstName], e.[ReportsTo], e.[Title]
FROM [Customers] AS c
INNER JOIN [Orders] AS o ON c.[CustomerID] = o.[CustomerID]
CROSS JOIN [Employees] AS e",
                _fixture.Sql);
        }

        public override void GroupBy_Distinct()
        {
            base.GroupBy_Distinct();

            Assert.Equal(
                @"SELECT DISTINCT o.[CustomerID], o.[OrderDate], o.[OrderID]
FROM [Orders] AS o",
                _fixture.Sql);
        }

        public override void SelectMany_cartesian_product_with_ordering()
        {
            base.SelectMany_cartesian_product_with_ordering();

            Assert.StartsWith(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region], e.[City]
FROM [Customers] AS c
CROSS JOIN [Employees] AS e
WHERE c.[City] = e.[City]
ORDER BY e.[City], c.[CustomerID] DESC",
                _fixture.Sql);
        }

        public override void GroupJoin_customers_orders_count()
        {
            base.GroupJoin_customers_orders_count();

            Assert.Equal(
                @"SELECT o.[CustomerID], o.[OrderDate], o.[OrderID]
FROM [Orders] AS o

SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c",
                _fixture.Sql);
        }

        public override void Take_with_single()
        {
            base.Take_with_single();

            Assert.Equal(
                @"SELECT TOP 2 *
FROM (
    SELECT TOP 1 c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
    FROM [Customers] AS c
    ORDER BY c.[CustomerID]
) AS t0",
                _fixture.Sql);
        }

        public override void Take_with_single_select_many()
        {
            base.Take_with_single_select_many();

            Assert.Equal(
                @"SELECT TOP 2 *
FROM (
    SELECT TOP 1 c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region], o.[CustomerID] AS c0, o.[OrderDate], o.[OrderID]
    FROM [Customers] AS c
    CROSS JOIN [Orders] AS o
    ORDER BY c.[CustomerID], o.[OrderID]
) AS t0",
                _fixture.Sql);
        }

        public override void Distinct()
        {
            base.Distinct();

            Assert.Equal(
                @"SELECT DISTINCT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c",
                _fixture.Sql);
        }

        public override void Distinct_Scalar()
        {
            base.Distinct_Scalar();

            Assert.Equal(
                @"SELECT DISTINCT c.[City]
FROM [Customers] AS c",
                _fixture.Sql);
        }

        public override void OrderBy_Distinct()
        {
            base.OrderBy_Distinct();

            Assert.Equal(
                @"SELECT DISTINCT c.[City]
FROM [Customers] AS c",
                _fixture.Sql);
        }

        public override void Distinct_OrderBy()
        {
            base.Distinct_OrderBy();

            Assert.Equal(
                @"SELECT DISTINCT c.[City]
FROM [Customers] AS c",
                //ORDER BY c.[City]", // TODO: Sub-query flattening
                _fixture.Sql);
        }

        public override void OrderBy_shadow()
        {
            base.OrderBy_shadow();

            Assert.Equal(
                @"SELECT e.[City], e.[Country], e.[EmployeeID], e.[FirstName], e.[ReportsTo], e.[Title]
FROM [Employees] AS e
ORDER BY e.[Title], e.[EmployeeID]",
                _fixture.Sql);
        }

        public override void OrderBy_multiple()
        {
            base.OrderBy_multiple();

            Assert.Equal(
                @"SELECT c.[City]
FROM [Customers] AS c
ORDER BY c.[Country], c.[CustomerID]",
                _fixture.Sql);
        }

        public override void Where_subquery_recursive_trivial()
        {
            base.Where_subquery_recursive_trivial();

            Assert.Equal(2472, _fixture.Sql.Length);
            Assert.StartsWith(
                @"SELECT e1.[City], e1.[Country], e1.[EmployeeID], e1.[FirstName], e1.[ReportsTo], e1.[Title]
FROM [Employees] AS e1
ORDER BY e1.[EmployeeID]

SELECT e2.[City], e2.[Country], e2.[EmployeeID], e2.[FirstName], e2.[ReportsTo], e2.[Title]
FROM [Employees] AS e2

SELECT CASE WHEN (
    EXISTS (
        SELECT 1
        FROM [Employees] AS e3
    )
) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END

SELECT e2.[City], e2.[Country], e2.[EmployeeID], e2.[FirstName], e2.[ReportsTo], e2.[Title]
FROM [Employees] AS e2",
                _fixture.Sql);
        }

        public override void Where_false()
        {
            base.Where_false();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c
WHERE 1 = 0",
                _fixture.Sql);
        }

        public override void Where_primitive()
        {
            base.Where_primitive();

            Assert.Equal(
                @"SELECT TOP 9 e.[EmployeeID]
FROM [Employees] AS e",
                _fixture.Sql);
        }

        public override void Where_true()
        {
            base.Where_true();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c
WHERE 1 = 1",
                _fixture.Sql);
        }

        public override void Where_compare_constructed_equal()
        {
            base.Where_compare_constructed_equal();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c",
                _fixture.Sql);
        }

        public override void Where_compare_constructed_multi_value_equal()
        {
            base.Where_compare_constructed_multi_value_equal();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c",
                _fixture.Sql);
        }

        public override void Where_compare_constructed_multi_value_not_equal()
        {
            base.Where_compare_constructed_multi_value_not_equal();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c",
                _fixture.Sql);
        }

        public override void Where_compare_constructed()
        {
            base.Where_compare_constructed();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c",
                _fixture.Sql);
        }

        public override void Single_Predicate()
        {
            base.Single_Predicate();

            Assert.Equal(
                @"SELECT TOP 2 c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c
WHERE c.[CustomerID] = @p0",
                _fixture.Sql);
        }

        public override void Projection_when_null_value()
        {
            base.Projection_when_null_value();

            Assert.Equal(
                @"SELECT c.[Region]
FROM [Customers] AS c",
                _fixture.Sql);
        }

        public override void String_StartsWith_Literal()
        {
            base.String_StartsWith_Literal();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c
WHERE c.[ContactName] LIKE @p0 + '%'",
                _fixture.Sql);
        }

        public override void String_StartsWith_Identity()
        {
            base.String_StartsWith_Identity();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c
WHERE c.[ContactName] LIKE c.[ContactName] + '%'",
                _fixture.Sql);
        }

        public override void String_StartsWith_Column()
        {
            base.String_StartsWith_Column();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c
WHERE c.[ContactName] LIKE c.[ContactName] + '%'",
                _fixture.Sql);
        }

        public override void String_StartsWith_MethodCall()
        {
            base.String_StartsWith_MethodCall();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c
WHERE c.[ContactName] LIKE @p0 + '%'",
                _fixture.Sql);
        }

        public override void String_EndsWith_Literal()
        {
            base.String_EndsWith_Literal();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c
WHERE c.[ContactName] LIKE '%' + @p0",
                _fixture.Sql);
        }

        public override void String_EndsWith_Identity()
        {
            base.String_EndsWith_Identity();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c
WHERE c.[ContactName] LIKE '%' + c.[ContactName]",
                _fixture.Sql);
        }

        public override void String_EndsWith_Column()
        {
            base.String_EndsWith_Column();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c
WHERE c.[ContactName] LIKE '%' + c.[ContactName]",
                _fixture.Sql);
        }

        public override void String_EndsWith_MethodCall()
        {
            base.String_EndsWith_MethodCall();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Customers] AS c
WHERE c.[ContactName] LIKE '%' + @p0",
                _fixture.Sql);
        }

        public override void Select_nested_collection()
        {
            base.Select_nested_collection();

            Assert.StartsWith(
                @"SELECT c.[CustomerID]
FROM [Customers] AS c
WHERE c.[City] = @p0
ORDER BY c.[CustomerID]

SELECT o.[CustomerID], o.[OrderDate], o.[OrderID]
FROM [Orders] AS o
ORDER BY o.[OrderID]

",
                _fixture.Sql);
        }

        public override void Select_correlated_subquery_projection()
        {
            base.Select_correlated_subquery_projection();

            Assert.StartsWith(
                @"SELECT c.[CustomerID]
FROM [Customers] AS c

SELECT o.[CustomerID], o.[OrderDate], o.[OrderID]
FROM [Orders] AS o

",
                _fixture.Sql);
        }

        public override void Select_correlated_subquery_ordered()
        {
            base.Select_correlated_subquery_ordered();

            Assert.StartsWith(
                @"SELECT c.[CustomerID]
FROM [Customers] AS c

SELECT o.[CustomerID], o.[OrderDate], o.[OrderID]
FROM [Orders] AS o

",
                _fixture.Sql);
        }

        public override void SelectMany_correlated_subquery_hard()
        {
            base.SelectMany_correlated_subquery_hard();
        }

        private readonly NorthwindQueryFixture _fixture;

        public NorthwindQueryTest(NorthwindQueryFixture fixture)
        {
            _fixture = fixture;
            _fixture.InitLogger();
        }

        protected override DbContext CreateContext()
        {
            return _fixture.CreateContext();
        }
    }

    public class NorthwindQueryFixture : NorthwindQueryFixtureRelationalBase, IDisposable
    {
        private readonly TestSqlLoggerFactory _loggingFactory = new TestSqlLoggerFactory();

        private readonly IServiceProvider _serviceProvider;
        private readonly DbContextOptions _options;
        private readonly SqlServerTestDatabase _testDatabase;

        public NorthwindQueryFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .UseLoggerFactory(_loggingFactory)
                    .ServiceCollection
                    .BuildServiceProvider();

            _testDatabase = SqlServerTestDatabase.Northwind().Result;

            _options
                = new DbContextOptions()
                    .UseModel(SetTableNames(CreateModel()))
                    .UseSqlServer(_testDatabase.Connection.ConnectionString);
        }

        public string Sql
        {
            get { return string.Join("\r\n\r\n", TestSqlLoggerFactory.Logger._sqlStatements); }
        }

        public void Dispose()
        {
            _testDatabase.Dispose();
        }

        public DbContext CreateContext()
        {
            return new DbContext(_serviceProvider, _options);
        }

        public void InitLogger()
        {
            _loggingFactory.Init();
        }
    }
}
