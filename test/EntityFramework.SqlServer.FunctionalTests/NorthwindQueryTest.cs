// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Advanced;
using Microsoft.Framework.DependencyInjection.Fallback;
using Northwind;
using Xunit;

#if K10
using System.Threading;
#else

#endif

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    // TODO: Test non-SqlServer SQL (i.e. generated from Relational base) elsewhere
    public class NorthwindQueryTest : NorthwindQueryTestBase, IClassFixture<NorthwindQueryFixture>
    {
        public override void Queryable_simple()
        {
            base.Queryable_simple();

            Assert.Equal(
                @"SELECT t0.[Address], t0.[City], t0.[CompanyName], t0.[ContactName], t0.[ContactTitle], t0.[Country], t0.[CustomerID], t0.[Fax], t0.[Phone], t0.[PostalCode], t0.[Region]
FROM [Customers] AS t0",
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
                @"SELECT t1.[Address], t1.[City], t1.[CompanyName], t1.[ContactName], t1.[ContactTitle], t1.[Country], t1.[CustomerID], t1.[Fax], t1.[Phone], t1.[PostalCode], t1.[Region]
FROM [Customers] AS t1",
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
                @"SELECT TOP 9 t1.[City], t1.[Country], t1.[EmployeeID], t1.[FirstName]
FROM [Employees] AS t1",
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
                @"SELECT e.[City], e.[Country], e.[EmployeeID], e.[FirstName]
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
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region], e.[City], e.[Country], e.[EmployeeID], e.[FirstName]
FROM [Customers] AS c, [Employees] AS e
WHERE (c.[City] = @p0 OR e.[City] = @p0)",
                _fixture.Sql);
        }

        public override void Where_select_many_or2()
        {
            base.Where_select_many_or2();

            Assert.StartsWith(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region], e.[City], e.[Country], e.[EmployeeID], e.[FirstName]
FROM [Customers] AS c, [Employees] AS e
WHERE (c.[City] = @p0 OR c.[City] = @p1)",
                _fixture.Sql);
        }

        public override void Where_select_many_and()
        {
            base.Where_select_many_and();

            Assert.Equal(
                @"SELECT c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region], e.[City], e.[Country], e.[EmployeeID], e.[FirstName]
FROM [Customers] AS c, [Employees] AS e
WHERE ((c.[City] = @p0 AND c.[Country] = @p1) AND (e.[City] = @p0 AND e.[Country] = @p1))",
                _fixture.Sql);
        }

        public override void Select_project_filter()
        {
            base.Select_project_filter();

            Assert.Equal(
                @"SELECT c.[City], c.[CompanyName]
FROM [Customers] AS c
WHERE c.[City] = @p0",
                _fixture.Sql);
        }

        public override async Task Select_project_filter_async()
        {
            await base.Select_project_filter_async();

            Assert.Equal(
                @"SELECT c.[City], c.[CompanyName]
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

            Assert.Equal(3399, _fixture.Sql.Length);
            Assert.StartsWith(
                @"SELECT e1.[City], e1.[Country], e1.[EmployeeID], e1.[FirstName]
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
                @"SELECT e.[City], e.[Country], e.[EmployeeID], e.[FirstName], c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region]
FROM [Employees] AS e, [Customers] AS c",
                _fixture.Sql);
        }

        public override void SelectMany_simple2()
        {
            base.SelectMany_simple2();

            Assert.Equal(
                @"SELECT e1.[City], e1.[Country], e1.[EmployeeID], e1.[FirstName], c.[Address], c.[City], c.[CompanyName], c.[ContactName], c.[ContactTitle], c.[Country], c.[CustomerID], c.[Fax], c.[Phone], c.[PostalCode], c.[Region], e2.[FirstName]
FROM [Employees] AS e1, [Customers] AS c, [Employees] AS e2",
                _fixture.Sql);
        }

        public override void SelectMany_entity_deep()
        {
            base.SelectMany_entity_deep();

            Assert.Equal(
                @"SELECT e1.[City], e1.[Country], e1.[EmployeeID], e1.[FirstName], e2.[City], e2.[Country], e2.[EmployeeID], e2.[FirstName], e3.[City], e3.[Country], e3.[EmployeeID], e3.[FirstName]
FROM [Employees] AS e1, [Employees] AS e2, [Employees] AS e3",
                _fixture.Sql);
        }

        public override void SelectMany_projection1()
        {
            base.SelectMany_projection1();

            Assert.Equal(
                @"SELECT e1.[City], e2.[Country]
FROM [Employees] AS e1, [Employees] AS e2",
                _fixture.Sql);
        }

        public override void SelectMany_projection2()
        {
            base.SelectMany_projection2();

            Assert.Equal(
                @"SELECT e1.[City], e2.[Country], e3.[FirstName]
FROM [Employees] AS e1, [Employees] AS e2, [Employees] AS e3",
                _fixture.Sql);
        }

        public override void Join_customers_orders()
        {
            base.Join_customers_orders();

            Assert.Equal(
                @"SELECT o.[CustomerID], o.[OrderID]
FROM [Orders] AS o

SELECT c.[CustomerID], c.[ContactName]
FROM [Customers] AS c",
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
FROM [Customers] AS c, [Employees] AS e
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
    FROM [Customers] AS c, [Orders] AS o
    ORDER BY c.[CustomerID], o.[OrderID]
) AS t0",
                _fixture.Sql);
        }

        public override void Distinct()
        {
            base.Distinct();

            Assert.Equal(
                @"SELECT DISTINCT t1.[Address], t1.[City], t1.[CompanyName], t1.[ContactName], t1.[ContactTitle], t1.[Country], t1.[CustomerID], t1.[Fax], t1.[Phone], t1.[PostalCode], t1.[Region]
FROM [Customers] AS t1",
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

            Assert.Equal(1985, _fixture.Sql.Length);
            Assert.StartsWith(
                @"SELECT e1.[City], e1.[Country], e1.[EmployeeID], e1.[FirstName]
FROM [Employees] AS e1
ORDER BY e1.[EmployeeID]

SELECT e2.[City], e2.[Country], e2.[EmployeeID], e2.[FirstName]
FROM [Employees] AS e2

SELECT e3.[City], e3.[Country], e3.[EmployeeID], e3.[FirstName]
FROM [Employees] AS e3
ORDER BY e3.[EmployeeID]

SELECT e2.[City], e2.[Country], e2.[EmployeeID], e2.[FirstName]
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

        public override void All_top_level()
        {
            base.All_top_level();

            // TODO:
            //            Assert.Equal(
            //                @"SELECT CASE WHEN (NOT EXISTS(
            //  SELECT NULL 
            //  FROM [Customers] AS c AS t0
            //  WHERE NOT (t0.[ContactName] LIKE @p0 + '%')
            //  )) THEN 1 ELSE 0 END AS [value]",
            //                _fixture.Sql);
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
                @"SELECT c.[City], c.[CustomerID]
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

    public class NorthwindQueryFixture : NorthwindQueryFixtureBase, IDisposable
    {
        private readonly TestSqlLoggerFactory _loggingFactory = new TestSqlLoggerFactory();

        private readonly IServiceProvider _serviceProvider;
        private readonly DbContextOptions _options;
        private readonly TestDatabase _testDatabase;

        public NorthwindQueryFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .UseLoggerFactory(_loggingFactory)
                    .ServiceCollection
                    .BuildServiceProvider();

            _testDatabase = TestDatabase.Northwind().Result;

            _options
                = new DbContextOptions()
                    .UseModel(SetTableNames(CreateModel()))
                    .UseSqlServer(_testDatabase.Connection.ConnectionString);
        }

        public Model SetTableNames(Model model)
        {
            model.GetEntityType(typeof(Customer)).SetTableName("Customers");
            model.GetEntityType(typeof(Employee)).SetTableName("Employees");
            model.GetEntityType(typeof(Product)).SetTableName("Products");
            model.GetEntityType(typeof(Order)).SetTableName("Orders");
            model.GetEntityType(typeof(OrderDetail)).SetTableName("OrderDetails");

            return model;
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
