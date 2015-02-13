// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.SqlClient;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Microsoft.Data.Entity.Storage;
using Xunit;

#if ASPNETCORE50
using System.Threading;
#endif

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class QuerySqlServerTest : QueryTestBase<NorthwindQuerySqlServerFixture>
    {
        public override void Where_simple_closure()
        {
            base.Where_simple_closure();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_0",
                Sql);
        }

        public override void Where_simple_closure_via_query_cache_nullable_type_reverse()
        {
            base.Where_simple_closure_via_query_cache_nullable_type_reverse();

            Assert.Equal(
                @"SELECT [e].[City], [e].[Country], [e].[EmployeeID], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL

SELECT [e].[City], [e].[Country], [e].[EmployeeID], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__reportsTo_0

SELECT [e].[City], [e].[Country], [e].[EmployeeID], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__reportsTo_0",
                Sql);
        }

        public override void Where_simple_closure_via_query_cache_nullable_type()
        {
            base.Where_simple_closure_via_query_cache_nullable_type();

            Assert.Equal(
                @"SELECT [e].[City], [e].[Country], [e].[EmployeeID], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__reportsTo_0

SELECT [e].[City], [e].[Country], [e].[EmployeeID], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__reportsTo_0

SELECT [e].[City], [e].[Country], [e].[EmployeeID], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL",
                Sql);
        }

        public override void Where_new_instance_field_access_closure_via_query_cache()
        {
            base.Where_new_instance_field_access_closure_via_query_cache();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__p_InstanceFieldValue_0

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__p_InstanceFieldValue_0",
                Sql);
        }

        public override void Where_nested_property_access_closure_via_query_cache()
        {
            base.Where_nested_property_access_closure_via_query_cache();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_Nested_InstancePropertyValue_0

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_Nested_InstancePropertyValue_0",
                Sql);
        }

        public override void Where_nested_field_access_closure_via_query_cache()
        {
            base.Where_nested_field_access_closure_via_query_cache();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_Nested_InstanceFieldValue_0

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_Nested_InstanceFieldValue_0",
                Sql);
        }

        public override void Where_static_property_access_closure_via_query_cache()
        {
            base.Where_static_property_access_closure_via_query_cache();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__StaticPropertyValue_0

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__StaticPropertyValue_0",
                Sql);
        }

        public override void Where_property_access_closure_via_query_cache()
        {
            base.Where_property_access_closure_via_query_cache();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_InstancePropertyValue_0

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_InstancePropertyValue_0",
                Sql);
        }

        public override void Where_static_field_access_closure_via_query_cache()
        {
            base.Where_static_field_access_closure_via_query_cache();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__StaticFieldValue_0

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__StaticFieldValue_0",
                Sql);
        }

        public override void Where_field_access_closure_via_query_cache()
        {
            base.Where_field_access_closure_via_query_cache();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_InstanceFieldValue_0

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_InstanceFieldValue_0",
                Sql);
        }

        public override void Where_method_call_closure_via_query_cache()
        {
            base.Where_method_call_closure_via_query_cache();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__GetCity_0

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__GetCity_0",
                Sql);
        }

        public override void Where_method_call_nullable_type_reverse_closure_via_query_cache()
        {
            base.Where_method_call_nullable_type_reverse_closure_via_query_cache();

            Assert.Equal(
                @"SELECT [e].[City], [e].[Country], [e].[EmployeeID], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] > @__city_NullableInt_0

SELECT [e].[City], [e].[Country], [e].[EmployeeID], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] > @__city_NullableInt_0",
                Sql);
        }

        public override void Where_method_call_nullable_type_closure_via_query_cache()
        {
            base.Where_method_call_nullable_type_closure_via_query_cache();

            Assert.Equal(
                @"SELECT [e].[City], [e].[Country], [e].[EmployeeID], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__city_Int_0

SELECT [e].[City], [e].[Country], [e].[EmployeeID], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__city_Int_0",
                Sql);
        }

        public override void Where_simple_closure_via_query_cache()
        {
            base.Where_simple_closure_via_query_cache();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_0

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_0",
                Sql);
        }

        public override void Count_with_predicate()
        {
            base.Count_with_predicate();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = 'ALFKI'",
                Sql);
        }

        public override void Sum_with_no_arg()
        {
            base.Sum_with_no_arg();

            Assert.Equal(
                @"SELECT SUM([o].[OrderID])
FROM [Orders] AS [o]",
                Sql);
        }

        public override void Sum_with_arg()
        {
            base.Sum_with_arg();

            Assert.Equal(
                @"SELECT SUM([o].[OrderID])
FROM [Orders] AS [o]",
                Sql);
        }

        public override void Sum_with_binary_expression()
        {
            base.Sum_with_binary_expression();

            Assert.Equal(
                @"SELECT [o].[OrderID]
FROM [Orders] AS [o]",
                Sql);
        }

        public override void Min_with_no_arg()
        {
            base.Min_with_no_arg();

            Assert.Equal(
                @"SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]",
                Sql);
        }

        public override void Min_with_arg()
        {
            base.Min_with_arg();

            Assert.Equal(
                @"SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]",
                Sql);
        }

        public override void Max_with_no_arg()
        {
            base.Max_with_no_arg();

            Assert.Equal(
                @"SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]",
                Sql);
        }

        public override void Max_with_arg()
        {
            base.Max_with_arg();

            Assert.Equal(
                @"SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]",
                Sql);
        }

        public override void Distinct_Count()
        {
            base.Distinct_Count();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
) AS [t0]",
                Sql);
        }

        [Fact]
        public override void Select_Distinct_Count()
        {
            base.Select_Distinct_Count();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[City]
    FROM [Customers] AS [c]
) AS [t0]",
                Sql);
        }

        public override void Skip()
        {
            base.Skip();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID] OFFSET 5 ROWS",
                Sql);
        }

        public override void Skip_Take()
        {
            base.Skip_Take();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactName] OFFSET 5 ROWS FETCH NEXT 10 ROWS ONLY",
                Sql);
        }

        public override void Take_Skip()
        {
            base.Take_Skip();

            Assert.Equal(
                @"SELECT [t0].*
FROM (
    SELECT TOP(10) [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactName]
) AS [t0]
ORDER BY [t0].[ContactName] OFFSET 5 ROWS",
                Sql);
        }

        public override void Take_Skip_Distinct()
        {
            base.Take_Skip_Distinct();

            Assert.Equal(
                @"SELECT DISTINCT [t1].*
FROM (
    SELECT [t0].*
    FROM (
        SELECT TOP(10) [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactName]
    ) AS [t0]
    ORDER BY [t0].[ContactName] OFFSET 5 ROWS
) AS [t1]",
                Sql);
        }

        public void Skip_when_no_order_by()
        {
            Assert.Throws<DataStoreException>(() => AssertQuery<Customer>(cs => cs.Skip(5).Take(10)));
        }

        public override void Take_Distinct_Count()
        {
            base.Take_Distinct_Count();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT TOP(5) [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
    FROM [Orders] AS [o]
) AS [t0]",
                Sql);
        }

        public override void Queryable_simple()
        {
            base.Queryable_simple();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Queryable_simple_anonymous()
        {
            base.Queryable_simple_anonymous();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Queryable_nested_simple()
        {
            base.Queryable_nested_simple();

            Assert.Equal(
                @"SELECT [c3].[Address], [c3].[City], [c3].[CompanyName], [c3].[ContactName], [c3].[ContactTitle], [c3].[Country], [c3].[CustomerID], [c3].[Fax], [c3].[Phone], [c3].[PostalCode], [c3].[Region]
FROM [Customers] AS [c3]",
                Sql);
        }

        public override void Take_simple()
        {
            base.Take_simple();

            Assert.Equal(
                @"SELECT TOP(10) [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void Take_simple_projection()
        {
            base.Take_simple_projection();

            Assert.Equal(
                @"SELECT TOP(10) [c].[City]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void Any_simple()
        {
            base.Any_simple();

            Assert.Equal(
                @"SELECT CASE WHEN (
    EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
    )
) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END",
                Sql);
        }

        public override void Any_predicate()
        {
            base.Any_predicate();

            Assert.Equal(
                @"SELECT CASE WHEN (
    EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        WHERE [c].[ContactName] LIKE 'A' + '%'
    )
) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END",
                Sql);
        }

        public override void All_top_level()
        {
            base.All_top_level();

            Assert.Equal(
                @"SELECT CASE WHEN (
    NOT EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        WHERE NOT [c].[ContactName] LIKE 'A' + '%'
    )
) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END",
                Sql);
        }

        public override void Select_scalar()
        {
            base.Select_scalar();

            Assert.Equal(
                @"SELECT [c].[City]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Select_scalar_primitive_after_take()
        {
            base.Select_scalar_primitive_after_take();

            Assert.Equal(
                @"SELECT TOP(9) [e].[City], [e].[Country], [e].[EmployeeID], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]",
                Sql);
        }

        public override void Where_simple()
        {
            base.Where_simple();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = 'London'",
                Sql);
        }

        public override void Where_simple_shadow()
        {
            base.Where_simple_shadow();

            Assert.Equal(
                @"SELECT [e].[City], [e].[Country], [e].[EmployeeID], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[Title] = 'Sales Representative'",
                Sql);
        }

        public override void Where_simple_shadow_projection()
        {
            base.Where_simple_shadow_projection();

            Assert.Equal(
                @"SELECT [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[Title] = 'Sales Representative'",
                Sql);
        }

        public override void Where_comparison_nullable_type_not_null()
        {
            base.Where_comparison_nullable_type_not_null();

            Assert.Equal(
                @"SELECT [e].[City], [e].[Country], [e].[EmployeeID], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = 2",
                Sql);
        }

        public override void Where_comparison_nullable_type_null()
        {
            base.Where_comparison_nullable_type_null();

            Assert.Equal(
                @"SELECT [e].[City], [e].[Country], [e].[EmployeeID], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL",
                Sql);
        }

        public override void Where_client()
        {
            base.Where_client();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void First_client_predicate()
        {
            base.First_client_predicate();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void Last()
        {
            base.Last();

            Assert.Equal(
                @"SELECT TOP(1) [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactName] DESC",
                Sql);
        }

        public override void Last_when_no_order_by()
        {
            base.Last_when_no_order_by();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = 'ALFKI'",
                Sql);
        }

        public override void Last_Predicate()
        {
            base.Last_Predicate();

            Assert.Equal(
                @"SELECT TOP(1) [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = 'London'
ORDER BY [c].[ContactName] DESC",
                Sql);
        }

        public override void Where_Last()
        {
            base.Where_Last();

            Assert.Equal(
                @"SELECT TOP(1) [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = 'London'
ORDER BY [c].[ContactName] DESC",
                Sql);
        }

        public override void LastOrDefault()
        {
            base.LastOrDefault();

            Assert.Equal(
                @"SELECT TOP(1) [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactName] DESC",
                Sql);
        }

        public override void LastOrDefault_Predicate()
        {
            base.LastOrDefault_Predicate();

            Assert.Equal(
                @"SELECT TOP(1) [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = 'London'
ORDER BY [c].[ContactName] DESC",
                Sql);
        }

        public override void Where_LastOrDefault()
        {
            base.Where_LastOrDefault();

            Assert.Equal(
                @"SELECT TOP(1) [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = 'London'
ORDER BY [c].[ContactName] DESC",
                Sql);
        }

        public override void Where_equals_method_string()
        {
            base.Where_equals_method_string();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = 'London'",
                Sql);
        }

        public override void Where_equals_method_int()
        {
            base.Where_equals_method_int();

            Assert.Equal(
                @"SELECT [e].[City], [e].[Country], [e].[EmployeeID], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = 1",
                Sql);
        }

        public override void Where_string_length()
        {
            base.Where_string_length();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Where_is_null()
        {
            base.Where_is_null();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] IS NULL",
                Sql);
        }

        public override void Where_is_not_null()
        {
            base.Where_is_not_null();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] IS NOT NULL",
                Sql);
        }

        public override void Where_null_is_null()
        {
            base.Where_null_is_null();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 1 = 1",
                Sql);
        }

        public override void Where_constant_is_null()
        {
            base.Where_constant_is_null();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 1 = 0",
                Sql);
        }

        public override void Where_null_is_not_null()
        {
            base.Where_null_is_not_null();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 1 = 0",
                Sql);
        }

        public override void Where_constant_is_not_null()
        {
            base.Where_constant_is_not_null();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 1 = 1",
                Sql);
        }

        public override void Where_simple_reversed()
        {
            base.Where_simple_reversed();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 'London' = [c].[City]",
                Sql);
        }

        public override void Where_identity_comparison()
        {
            base.Where_identity_comparison();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = [c].[City]",
                Sql);
        }

        public override void Where_select_many_or()
        {
            base.Where_select_many_or();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[City], [e].[Country], [e].[EmployeeID], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE ([c].[City] = 'London' OR [e].[City] = 'London')",
                Sql);
        }

        public override void Where_select_many_or2()
        {
            base.Where_select_many_or2();

            Assert.StartsWith(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[City], [e].[Country], [e].[EmployeeID], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE ([c].[City] = 'London' OR [c].[City] = 'Berlin')",
                Sql);
        }

        public override void Where_select_many_and()
        {
            base.Where_select_many_and();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[City], [e].[Country], [e].[EmployeeID], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE (([c].[City] = 'London' AND [c].[Country] = 'UK') AND ([e].[City] = 'London' AND [e].[Country] = 'UK'))",
                Sql);
        }

        public override void Select_project_filter()
        {
            base.Select_project_filter();

            Assert.Equal(
                @"SELECT [c].[CompanyName]
FROM [Customers] AS [c]
WHERE [c].[City] = 'London'",
                Sql);
        }

        public override void Select_project_filter2()
        {
            base.Select_project_filter2();

            Assert.Equal(
                @"SELECT [c].[City]
FROM [Customers] AS [c]
WHERE [c].[City] = 'London'",
                Sql);
        }

        public override void SelectMany_mixed()
        {
            base.SelectMany_mixed();

            Assert.Equal(3873, Sql.Length);
            Assert.StartsWith(
                @"SELECT [e1].[City], [e1].[Country], [e1].[EmployeeID], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
FROM [Employees] AS [e1]

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void SelectMany_simple1()
        {
            base.SelectMany_simple1();

            Assert.Equal(
                @"SELECT [e].[City], [e].[Country], [e].[EmployeeID], [e].[FirstName], [e].[ReportsTo], [e].[Title], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Employees] AS [e]
CROSS JOIN [Customers] AS [c]",
                Sql);
        }

        public override void SelectMany_simple2()
        {
            base.SelectMany_simple2();

            Assert.Equal(
                @"SELECT [e1].[City], [e1].[Country], [e1].[EmployeeID], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e2].[FirstName]
FROM [Employees] AS [e1]
CROSS JOIN [Customers] AS [c]
CROSS JOIN [Employees] AS [e2]",
                Sql);
        }

        public override void SelectMany_entity_deep()
        {
            base.SelectMany_entity_deep();

            Assert.Equal(
                @"SELECT [e1].[City], [e1].[Country], [e1].[EmployeeID], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title], [e2].[City], [e2].[Country], [e2].[EmployeeID], [e2].[FirstName], [e2].[ReportsTo], [e2].[Title], [e3].[City], [e3].[Country], [e3].[EmployeeID], [e3].[FirstName], [e3].[ReportsTo], [e3].[Title]
FROM [Employees] AS [e1]
CROSS JOIN [Employees] AS [e2]
CROSS JOIN [Employees] AS [e3]",
                Sql);
        }

        public override void SelectMany_projection1()
        {
            base.SelectMany_projection1();

            Assert.Equal(
                @"SELECT [e1].[City], [e2].[Country]
FROM [Employees] AS [e1]
CROSS JOIN [Employees] AS [e2]",
                Sql);
        }

        public override void SelectMany_projection2()
        {
            base.SelectMany_projection2();

            Assert.Equal(
                @"SELECT [e1].[City], [e2].[Country], [e3].[FirstName]
FROM [Employees] AS [e1]
CROSS JOIN [Employees] AS [e2]
CROSS JOIN [Employees] AS [e3]",
                Sql);
        }

        public override void SelectMany_Count()
        {
            base.SelectMany_Count();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]", Sql);
        }

        public override void SelectMany_OrderBy_ThenBy_Any()
        {
            base.SelectMany_OrderBy_ThenBy_Any();

            Assert.Equal(
                @"SELECT CASE WHEN (
    EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        CROSS JOIN [Orders] AS [o]
    )
) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END", Sql);
        }

        public override void Join_customers_orders_projection()
        {
            base.Join_customers_orders_projection();

            Assert.Equal(
                @"SELECT [c].[ContactName], [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]",
                Sql);
        }

        public override void Join_customers_orders_entities()
        {
            base.Join_customers_orders_entities();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]",
                Sql);
        }

        public override void Join_composite_key()
        {
            base.Join_composite_key();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON ([c].[CustomerID] = [o].[CustomerID] AND [c].[CustomerID] = [o].[CustomerID])",
                Sql);
        }

        public override void Join_client_new_expression()
        {
            base.Join_client_new_expression();

            Assert.Equal(
                @"SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Join_select_many()
        {
            base.Join_select_many();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [e].[City], [e].[Country], [e].[EmployeeID], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
CROSS JOIN [Employees] AS [e]",
                Sql);
        }

        public override void Join_Where_Count()
        {
            base.Join_Where_Count();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = 'ALFKI'", Sql);
        }

        public override void Join_OrderBy_Count()
        {
            //// issue 1251
            Assert.Throws<SqlException>(() => base.Join_OrderBy_Count());
        }

        public override void Multiple_joins_Where_Order_Any()
        {
            base.Multiple_joins_Where_Order_Any();

            Assert.Equal(
                @"SELECT CASE WHEN (
    EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        INNER JOIN [Orders] AS [or] ON [c].[CustomerID] = [or].[CustomerID]
        INNER JOIN [Order Details] AS [od] ON [or].[OrderID] = [od].[OrderID]
        WHERE [c].[City] = 'London'
    )
) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END", Sql);
        }

        public override void GroupBy_Distinct()
        {
            base.GroupBy_Distinct();

            Assert.Equal(
                @"SELECT DISTINCT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]",
                Sql);
        }

        public override void GroupBy_Count()
        {
            base.GroupBy_Count();

            Assert.Equal(
                @"SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]",
                Sql);
        }

        public override void SelectMany_cartesian_product_with_ordering()
        {
            base.SelectMany_cartesian_product_with_ordering();

            Assert.StartsWith(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[City]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] = [e].[City]
ORDER BY [e].[City], [c].[CustomerID] DESC",
                Sql);
        }

        public override void GroupJoin_customers_orders_count()
        {
            base.GroupJoin_customers_orders_count();

            Assert.Equal(
                @"SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Take_with_single()
        {
            base.Take_with_single();

            Assert.Equal(
                @"SELECT TOP(2) [t0].*
FROM (
    SELECT TOP(1) [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [t0]",
                Sql);
        }

        public override void Take_with_single_select_many()
        {
            base.Take_with_single_select_many();

            Assert.Equal(
                @"SELECT TOP(2) [t0].*
FROM (
    SELECT TOP(1) [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[CustomerID] AS [c0], [o].[OrderDate], [o].[OrderID]
    FROM [Customers] AS [c]
    CROSS JOIN [Orders] AS [o]
    ORDER BY [c].[CustomerID], [o].[OrderID]
) AS [t0]",
                Sql);
        }

        public override void Distinct()
        {
            base.Distinct();

            Assert.Equal(
                @"SELECT DISTINCT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Distinct_Scalar()
        {
            base.Distinct_Scalar();

            Assert.Equal(
                @"SELECT DISTINCT [c].[City]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void OrderBy_client_mixed()
        {
            base.OrderBy_client_mixed();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void OrderBy_multiple_queries()
        {
            base.OrderBy_multiple_queries();

            Assert.Equal(
                @"SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void OrderBy_Distinct()
        {
            base.OrderBy_Distinct();

            Assert.Equal(
                @"SELECT DISTINCT [c].[City]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Distinct_OrderBy()
        {
            base.Distinct_OrderBy();

            Assert.Equal(
                @"SELECT DISTINCT [c].[City]
FROM [Customers] AS [c]",
                //ORDER BY c.[City]", // TODO: Sub-query flattening
                Sql);
        }

        public override void OrderBy_shadow()
        {
            base.OrderBy_shadow();

            Assert.Equal(
                @"SELECT [e].[City], [e].[Country], [e].[EmployeeID], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
ORDER BY [e].[Title], [e].[EmployeeID]",
                Sql);
        }

        public override void OrderBy_multiple()
        {
            base.OrderBy_multiple();

            Assert.Equal(
                @"SELECT [c].[City]
FROM [Customers] AS [c]
ORDER BY [c].[Country], [c].[CustomerID]",
                Sql);
        }

        public override void OrderBy_ThenBy_Any()
        {
            base.OrderBy_ThenBy_Any();

            Assert.Equal(
                @"SELECT CASE WHEN (
    EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
    )
) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END",
                Sql);
        }

        public override void Where_subquery_recursive_trivial()
        {
            base.Where_subquery_recursive_trivial();

            Assert.Equal(2632, Sql.Length);
            Assert.StartsWith(
                @"SELECT [e1].[City], [e1].[Country], [e1].[EmployeeID], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
FROM [Employees] AS [e1]
ORDER BY [e1].[EmployeeID]

SELECT [e2].[City], [e2].[Country], [e2].[EmployeeID], [e2].[FirstName], [e2].[ReportsTo], [e2].[Title]
FROM [Employees] AS [e2]

SELECT CASE WHEN (
    EXISTS (
        SELECT 1
        FROM [Employees] AS [e3]
    )
) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END

SELECT [e2].[City], [e2].[Country], [e2].[EmployeeID], [e2].[FirstName], [e2].[ReportsTo], [e2].[Title]
FROM [Employees] AS [e2]",
                Sql);
        }

        public override void Where_false()
        {
            base.Where_false();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 1 = 0",
                Sql);
        }

        public override void Where_primitive()
        {
            base.Where_primitive();

            Assert.Equal(
                @"SELECT TOP(9) [e].[EmployeeID]
FROM [Employees] AS [e]",
                Sql);
        }

        public override void Where_bool_member()
        {
            base.Where_bool_member();

            Assert.Equal(
                @"SELECT [p].[Discontinued], [p].[ProductID], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 1",
                Sql);
        }

        public override void Where_bool_member_false()
        {
            base.Where_bool_member_false();

            Assert.Equal(
                @"SELECT [p].[Discontinued], [p].[ProductID], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE NOT [p].[Discontinued] = 1",
                Sql);
        }

        public override void Where_bool_member_shadow()
        {
            base.Where_bool_member_shadow();

            Assert.Equal(
                @"SELECT [p].[Discontinued], [p].[ProductID], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 1",
                Sql);
        }

        public override void Where_bool_member_false_shadow()
        {
            base.Where_bool_member_false_shadow();

            Assert.Equal(
                @"SELECT [p].[Discontinued], [p].[ProductID], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE NOT [p].[Discontinued] = 1",
                Sql);
        }

        public override void Where_short_member_comparison()
        {
            base.Where_short_member_comparison();

            Assert.Equal(
                @"SELECT [p].[Discontinued], [p].[ProductID], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[UnitsInStock] > 10",
                Sql);
        }

        public override void Where_true()
        {
            base.Where_true();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 1 = 1",
                Sql);
        }

        public override void Where_compare_constructed_equal()
        {
            base.Where_compare_constructed_equal();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Where_compare_constructed_multi_value_equal()
        {
            base.Where_compare_constructed_multi_value_equal();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Where_compare_constructed_multi_value_not_equal()
        {
            base.Where_compare_constructed_multi_value_not_equal();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Where_compare_constructed()
        {
            base.Where_compare_constructed();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Where_compare_null()
        {
            base.Where_compare_null();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[City] IS NULL AND [c].[Country] = 'UK')",
                Sql);
        }

        public override void Single_Predicate()
        {
            base.Single_Predicate();

            Assert.Equal(
                @"SELECT TOP(2) [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = 'ALFKI'",
                Sql);
        }

        public override void Projection_when_null_value()
        {
            base.Projection_when_null_value();

            Assert.Equal(
                @"SELECT [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void String_StartsWith_Literal()
        {
            base.String_StartsWith_Literal();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE 'M' + '%'",
                Sql);
        }

        public override void String_StartsWith_Identity()
        {
            base.String_StartsWith_Identity();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE [c].[ContactName] + '%'",
                Sql);
        }

        public override void String_StartsWith_Column()
        {
            base.String_StartsWith_Column();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE [c].[ContactName] + '%'",
                Sql);
        }

        public override void String_StartsWith_MethodCall()
        {
            base.String_StartsWith_MethodCall();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE @__LocalMethod1_0 + '%'",
                Sql);
        }

        public override void String_EndsWith_Literal()
        {
            base.String_EndsWith_Literal();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE '%' + 'b'",
                Sql);
        }

        public override void String_EndsWith_Identity()
        {
            base.String_EndsWith_Identity();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE '%' + [c].[ContactName]",
                Sql);
        }

        public override void String_EndsWith_Column()
        {
            base.String_EndsWith_Column();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE '%' + [c].[ContactName]",
                Sql);
        }

        public override void String_EndsWith_MethodCall()
        {
            base.String_EndsWith_MethodCall();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE '%' + @__LocalMethod2_0",
                Sql);
        }

        public override void String_Contains_Literal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains("M")), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains("M") || c.ContactName.Contains("m")), // case-sensitive
                entryCount: 34);

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE '%' + 'M' + '%'",
                Sql);
        }

        public override void String_Contains_Identity()
        {
            base.String_Contains_Identity();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE '%' + [c].[ContactName] + '%'",
                Sql);
        }

        public override void String_Contains_Column()
        {
            base.String_Contains_Column();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE '%' + [c].[ContactName] + '%'",
                Sql);
        }

        public override void String_Contains_MethodCall()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains(LocalMethod1())), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains(LocalMethod1()) || c.ContactName.Contains(LocalMethod2())), // case-sensitive
                entryCount: 34);

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE '%' + @__LocalMethod1_0 + '%'",
                Sql);
        }

        public override void Select_nested_collection()
        {
            base.Select_nested_collection();

            Assert.StartsWith(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[City] = 'London'
ORDER BY [c].[CustomerID]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
ORDER BY [o].[OrderID]

",
                Sql);
        }

        public override void Select_correlated_subquery_projection()
        {
            base.Select_correlated_subquery_projection();

            Assert.StartsWith(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]

",
                Sql);
        }

        public override void Select_correlated_subquery_ordered()
        {
            base.Select_correlated_subquery_ordered();

            Assert.StartsWith(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]

",
                Sql);
        }

        public override void Select_many_cross_join_same_collection()
        {
            base.Select_many_cross_join_same_collection();

            Assert.Equal(
                @"SELECT [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[CustomerID], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c]
CROSS JOIN [Customers] AS [c0]",
                Sql);
        }

        public override void Join_same_collection_multiple()
        {
            base.Join_same_collection_multiple();

            Assert.Equal(
                @"SELECT [c3].[Address], [c3].[City], [c3].[CompanyName], [c3].[ContactName], [c3].[ContactTitle], [c3].[Country], [c3].[CustomerID], [c3].[Fax], [c3].[Phone], [c3].[PostalCode], [c3].[Region]
FROM [Customers] AS [o]
INNER JOIN [Customers] AS [c2] ON [o].[CustomerID] = [c2].[CustomerID]
INNER JOIN [Customers] AS [c3] ON [o].[CustomerID] = [c3].[CustomerID]",
                Sql);
        }

        public override void Join_same_collection_force_alias_uniquefication()
        {
            base.Join_same_collection_force_alias_uniquefication();

            Assert.Equal(
                @"SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [o0].[CustomerID], [o0].[OrderDate], [o0].[OrderID]
FROM [Orders] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[CustomerID] = [o0].[CustomerID]",
                Sql);
        }

        public override void Where_chain()
        {
            base.Where_chain();

            Assert.Equal(
                @"SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = 'QUICK' AND [o].[OrderDate] > @__p_0)",
                Sql);
        }

        public QuerySqlServerTest(NorthwindQuerySqlServerFixture fixture)
            : base(fixture)
        {
        }

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
