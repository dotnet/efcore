// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Xunit;
using Xunit.Abstractions;

#if NETSTANDARDAPP1_5
using System.Threading;
#endif

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    [MonoVersionCondition(Min = "4.2.0", SkipReason = "Queries fail on Mono < 4.2.0 due to differences in the implementation of LINQ")]
    public class QuerySqlServerTest : QueryTestBase<NorthwindQuerySqlServerFixture>
    {
        public QuerySqlServerTest(NorthwindQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        public override void Entity_equality_self()
        {
            base.Entity_equality_self();

            Assert.Equal(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = [c].[CustomerID]",
                Sql);
        }

        public override void Entity_equality_local()
        {
            base.Entity_equality_local();

            Assert.Equal(
                @"@__local_0_CustomerID: ANATR

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__local_0_CustomerID",
                Sql);
        }

        public override void Entity_equality_local_inline()
        {
            base.Entity_equality_local_inline();

            Assert.Equal(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ANATR'",
                Sql);
        }

        public override void Queryable_reprojection()
        {
            base.Queryable_reprojection();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Default_if_empty_top_level()
        {
            base.Default_if_empty_top_level();

            Assert.Equal(
                @"SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title]
FROM (
    SELECT NULL AS [empty]
) AS [empty0]
LEFT JOIN (
    SELECT [c].[EmployeeID], [c].[City], [c].[Country], [c].[FirstName], [c].[ReportsTo], [c].[Title]
    FROM [Employees] AS [c]
    WHERE [c].[EmployeeID] = -1
) AS [t] ON 1 = 1",
                Sql);
        }

        public override void Default_if_empty_top_level_positive()
        {
            base.Default_if_empty_top_level_positive();

            Assert.Equal(
                @"SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title]
FROM (
    SELECT NULL AS [empty]
) AS [empty0]
LEFT JOIN (
    SELECT [c].[EmployeeID], [c].[City], [c].[Country], [c].[FirstName], [c].[ReportsTo], [c].[Title]
    FROM [Employees] AS [c]
    WHERE [c].[EmployeeID] > 0
) AS [t] ON 1 = 1",
                Sql);
        }

        public override void Default_if_empty_top_level_arg()
        {
            base.Default_if_empty_top_level_arg();

            Assert.Equal(
                @"SELECT [c].[EmployeeID], [c].[City], [c].[Country], [c].[FirstName], [c].[ReportsTo], [c].[Title]
FROM [Employees] AS [c]
WHERE [c].[EmployeeID] = -1",
                Sql);
        }

        public override void Default_if_empty_top_level_projection()
        {
            base.Default_if_empty_top_level_projection();

            Assert.Equal(
                @"SELECT [t].[EmployeeID]
FROM (
    SELECT NULL AS [empty]
) AS [empty0]
LEFT JOIN (
    SELECT [e].[EmployeeID]
    FROM [Employees] AS [e]
    WHERE [e].[EmployeeID] = -1
) AS [t] ON 1 = 1",
                Sql);
        }

        public override void Where_query_composition()
        {
            base.Where_query_composition();

            Assert.Equal(
                @"SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
FROM [Employees] AS [e1]
WHERE [e1].[FirstName] = (
    SELECT TOP(1) [e].[FirstName]
    FROM [Employees] AS [e]
    ORDER BY [e].[EmployeeID]
)",
                Sql);
        }

        public override void Where_query_composition_is_null()
        {
            base.Where_query_composition_is_null();

            Assert.StartsWith(
                @"SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
FROM [Employees] AS [e1]

SELECT [e2].[EmployeeID], [e2].[City], [e2].[Country], [e2].[FirstName], [e2].[ReportsTo], [e2].[Title]
FROM [Employees] AS [e2]",
                Sql);
        }

        public override void Where_query_composition_is_not_null()
        {
            base.Where_query_composition_is_null();

            Assert.StartsWith(
                @"SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
FROM [Employees] AS [e1]

SELECT [e2].[EmployeeID], [e2].[City], [e2].[Country], [e2].[FirstName], [e2].[ReportsTo], [e2].[Title]
FROM [Employees] AS [e2]",
                Sql);
        }

        public override void Where_query_composition_entity_equality()
        {
            base.Where_query_composition_entity_equality();

            Assert.StartsWith(
                @"SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
FROM [Employees] AS [e1]

SELECT [e20].[EmployeeID]
FROM [Employees] AS [e20]",
                Sql);
        }

        public override void Where_query_composition2()
        {
            base.Where_query_composition2();

            Assert.StartsWith(
                @"SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
FROM [Employees] AS [e1]

SELECT TOP(1) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
ORDER BY [e].[EmployeeID]",
                Sql);
        }

        public override void Where_shadow_subquery_first()
        {
            base.Where_shadow_subquery_first();

            Assert.Equal(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[Title] = (
    SELECT TOP(1) [e2].[Title]
    FROM [Employees] AS [e2]
    ORDER BY [e2].[Title]
)",
                Sql);
        }

        public override void Select_Subquery_Single()
        {
            base.Select_Subquery_Single();

            Assert.Equal(
                @"@__p_0: 2

SELECT TOP(@__p_0) [od].[OrderID]
FROM [Order Details] AS [od]

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]",
                Sql);
        }

        public override void Select_Where_Subquery_Deep_Single()
        {
            base.Select_Where_Subquery_Deep_Single();

            Assert.StartsWith(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]

SELECT [o0].[OrderID], [o0].[CustomerID]
FROM [Orders] AS [o0]

SELECT [c0].[CustomerID], [c0].[City]
FROM [Customers] AS [c0]",
                Sql);
        }

        public override void Select_Where_Subquery_Deep_First()
        {
            base.Select_Where_Subquery_Deep_First();

            Assert.StartsWith(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]

SELECT [o0].[OrderID], [o0].[CustomerID]
FROM [Orders] AS [o0]

SELECT [c0].[CustomerID], [c0].[City]
FROM [Customers] AS [c0]",
                Sql);
        }

        public override void Select_Where_Subquery_Equality()
        {
            base.Select_Where_Subquery_Equality();

            Assert.StartsWith(
                @"@__p_0: 2

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
) AS [t]

SELECT [t1].[OrderID]
FROM (
    SELECT TOP(2) [o4].[OrderID], [o4].[ProductID], [o4].[Discount], [o4].[Quantity], [o4].[UnitPrice]
    FROM [Order Details] AS [o4]
) AS [t1]

SELECT [c5].[CustomerID], [c5].[Country]
FROM [Customers] AS [c5]

SELECT [o23].[OrderID], [c6].[Country]
FROM [Orders] AS [o23]
INNER JOIN [Customers] AS [c6] ON [o23].[CustomerID] = [c6].[CustomerID]",
                Sql);
        }

        public override void Where_subquery_anon()
        {
            base.Where_subquery_anon();

            Assert.Equal(
                @"@__p_0: 9

SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title], [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title]
    FROM [Employees] AS [e0]
) AS [t]
CROSS JOIN (
    SELECT TOP(1000) [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
) AS [t0]",
                Sql);
        }

        public override void Where_subquery_correlated()
        {
            base.Where_subquery_correlated();

            Assert.Equal(
                @"SELECT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
FROM [Customers] AS [c1]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c2]
    WHERE [c1].[CustomerID] = [c2].[CustomerID])",
                Sql);
        }

        public override void Where_subquery_correlated_client_eval()
        {
            base.Where_subquery_correlated_client_eval();

            Assert.StartsWith(
                @"SELECT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
FROM [Customers] AS [c1]

SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
FROM [Customers] AS [c2]",
                Sql);
        }

        public override void OrderBy_SelectMany()
        {
            base.OrderBy_SelectMany();

            Assert.StartsWith(
                @"SELECT [c].[CustomerID], [c].[ContactName]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

SELECT [o].[CustomerID], [o].[OrderID]
FROM [Orders] AS [o]
ORDER BY [o].[OrderID]",
                Sql);
        }

        public override void Where_simple_closure()
        {
            base.Where_simple_closure();

            Assert.Equal(
                @"@__city_0: London

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_0",
                Sql);
        }

        public override void Where_simple_closure_constant()
        {
            base.Where_simple_closure_constant();

            Assert.Equal(
                @"@__predicate_0: True

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE @__predicate_0 = 1",
                Sql);
        }

        public override void Where_simple_closure_via_query_cache_nullable_type_reverse()
        {
            base.Where_simple_closure_via_query_cache_nullable_type_reverse();

            Assert.Equal(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL

@__reportsTo_0: 5

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__reportsTo_0

@__reportsTo_0: 2

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__reportsTo_0",
                Sql);
        }

        public override void Where_simple_closure_via_query_cache_nullable_type()
        {
            base.Where_simple_closure_via_query_cache_nullable_type();

            Assert.Equal(
                @"@__reportsTo_0: 2

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__reportsTo_0

@__reportsTo_0: 5

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__reportsTo_0

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL",
                Sql);
        }

        public override void Where_new_instance_field_access_closure_via_query_cache()
        {
            base.Where_new_instance_field_access_closure_via_query_cache();

            Assert.Equal(
                @"@__InstanceFieldValue_0: London

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__InstanceFieldValue_0

@__InstanceFieldValue_0: Seattle

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__InstanceFieldValue_0",
                Sql);
        }

        public override void Where_nested_property_access_closure_via_query_cache()
        {
            base.Where_nested_property_access_closure_via_query_cache();

            Assert.Equal(
                @"@__city_Nested_InstancePropertyValue_0: London

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_Nested_InstancePropertyValue_0

@__city_Nested_InstancePropertyValue_0: Seattle

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_Nested_InstancePropertyValue_0",
                Sql);
        }

        public override void Where_nested_field_access_closure_via_query_cache()
        {
            base.Where_nested_field_access_closure_via_query_cache();

            Assert.Equal(
                @"@__city_Nested_InstanceFieldValue_0: London

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_Nested_InstanceFieldValue_0

@__city_Nested_InstanceFieldValue_0: Seattle

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_Nested_InstanceFieldValue_0",
                Sql);
        }

        public override void Where_static_property_access_closure_via_query_cache()
        {
            base.Where_static_property_access_closure_via_query_cache();

            Assert.Equal(
                @"@__StaticPropertyValue_0: London

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__StaticPropertyValue_0

@__StaticPropertyValue_0: Seattle

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__StaticPropertyValue_0",
                Sql);
        }

        public override void Where_property_access_closure_via_query_cache()
        {
            base.Where_property_access_closure_via_query_cache();

            Assert.Equal(
                @"@__city_InstancePropertyValue_0: London

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_InstancePropertyValue_0

@__city_InstancePropertyValue_0: Seattle

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_InstancePropertyValue_0",
                Sql);
        }

        public override void Where_static_field_access_closure_via_query_cache()
        {
            base.Where_static_field_access_closure_via_query_cache();

            Assert.Equal(
                @"@__StaticFieldValue_0: London

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__StaticFieldValue_0

@__StaticFieldValue_0: Seattle

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__StaticFieldValue_0",
                Sql);
        }

        public override void Where_field_access_closure_via_query_cache()
        {
            base.Where_field_access_closure_via_query_cache();

            Assert.Equal(
                @"@__city_InstanceFieldValue_0: London

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_InstanceFieldValue_0

@__city_InstanceFieldValue_0: Seattle

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_InstanceFieldValue_0",
                Sql);
        }

        public override void Where_method_call_closure_via_query_cache()
        {
            base.Where_method_call_closure_via_query_cache();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Where_method_call_nullable_type_reverse_closure_via_query_cache()
        {
            base.Where_method_call_nullable_type_reverse_closure_via_query_cache();

            Assert.Equal(
                @"@__city_NullableInt_0: 1

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] > @__city_NullableInt_0

@__city_NullableInt_0: 5

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] > @__city_NullableInt_0",
                Sql);
        }

        public override void Where_method_call_nullable_type_closure_via_query_cache()
        {
            base.Where_method_call_nullable_type_closure_via_query_cache();

            Assert.Equal(
                @"@__city_Int_0: 2

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__city_Int_0

@__city_Int_0: 5

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__city_Int_0",
                Sql);
        }

        public override void Where_simple_closure_via_query_cache()
        {
            base.Where_simple_closure_via_query_cache();

            Assert.Equal(
                @"@__city_0: London

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_0

@__city_0: Seattle

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_0",
                Sql);
        }

        public override void Where_subquery_closure_via_query_cache()
        {
            base.Where_subquery_closure_via_query_cache();

            Assert.Equal(
                @"@__customerID_0: ALFKI

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE ([o].[CustomerID] = @__customerID_0) AND ([o].[CustomerID] = [c].[CustomerID]))

@__customerID_0: ANATR

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE ([o].[CustomerID] = @__customerID_0) AND ([o].[CustomerID] = [c].[CustomerID]))",
                Sql);
        }

        public override void Count_with_predicate()
        {
            base.Count_with_predicate();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'",
                Sql);
        }

        public override void Where_OrderBy_Count()
        {
            base.Where_OrderBy_Count();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'",
                Sql);
        }

        public override void OrderBy_Where_Count()
        {
            base.OrderBy_Where_Count();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'",
                Sql);
        }

        public override void OrderBy_Count_with_predicate()
        {
            base.OrderBy_Count_with_predicate();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'",
                Sql);
        }

        public override void OrderBy_Where_Count_with_predicate()
        {
            base.OrderBy_Where_Count_with_predicate();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE ([o].[OrderID] > 10) AND (([o].[CustomerID] <> N'ALFKI') OR [o].[CustomerID] IS NULL)",
                Sql);
        }

        public override void Where_OrderBy_Count_client_eval()
        {
            base.Where_OrderBy_Count_client_eval();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]",
                Sql);
        }

        public override void Where_OrderBy_Count_client_eval_mixed()
        {
            base.Where_OrderBy_Count_client_eval_mixed();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[OrderID] > 10",
                Sql);
        }

        public override void OrderBy_Where_Count_client_eval()
        {
            base.OrderBy_Where_Count_client_eval();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]",
                Sql);
        }

        public override void OrderBy_Where_Count_client_eval_mixed()
        {
            base.OrderBy_Where_Count_client_eval_mixed();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]",
                Sql);
        }

        public override void OrderBy_Count_with_predicate_client_eval()
        {
            base.OrderBy_Count_with_predicate_client_eval();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]",
                Sql);
        }

        public override void OrderBy_Count_with_predicate_client_eval_mixed()
        {
            base.OrderBy_Count_with_predicate_client_eval_mixed();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]",
                Sql);
        }

        public override void OrderBy_Where_Count_with_predicate_client_eval()
        {
            base.OrderBy_Where_Count_with_predicate_client_eval();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]",
                Sql);
        }

        public override void OrderBy_Where_Count_with_predicate_client_eval_mixed()
        {
            base.OrderBy_Where_Count_with_predicate_client_eval_mixed();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] <> N'ALFKI') OR [o].[CustomerID] IS NULL",
                Sql);
        }

        public override void OrderBy_client_Take()
        {
            base.OrderBy_client_Take();

            Assert.Equal(
                @"SELECT [o].[EmployeeID], [o].[City], [o].[Country], [o].[FirstName], [o].[ReportsTo], [o].[Title]
FROM [Employees] AS [o]",
                Sql);
        }

        public override void OrderBy_arithmetic()
        {
            base.OrderBy_arithmetic();

            Assert.Equal(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
ORDER BY [e].[EmployeeID] - [e].[EmployeeID]",
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

        public override void Sum_with_arg_expression()
        {
            base.Sum_with_arg_expression();

            Assert.Equal(
                @"SELECT SUM([o].[OrderID] + [o].[OrderID])
FROM [Orders] AS [o]",
                Sql);
        }

        public override void Sum_with_binary_expression()
        {
            base.Sum_with_binary_expression();

            Assert.Equal(
                @"SELECT SUM([o].[OrderID] * 2)
FROM [Orders] AS [o]",
                Sql);
        }

        public override void Sum_with_division_on_decimal()
        {
            base.Sum_with_division_on_decimal();

            Assert.Equal(@"SELECT SUM([od].[Quantity] / 2.09)
FROM [Order Details] AS [od]", Sql);
        }

        public override void Sum_with_division_on_decimal_no_significant_digits()
        {
            base.Sum_with_division_on_decimal_no_significant_digits();

            Assert.Equal(@"SELECT SUM([od].[Quantity] / 2.0)
FROM [Order Details] AS [od]", Sql);
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
    SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
) AS [t]",
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
) AS [t]",
                Sql);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Skip()
        {
            base.Skip();

            Assert.Equal(
                @"@__p_0: 5

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
OFFSET @__p_0 ROWS",
                Sql);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Skip_no_orderby()
        {
            base.Skip_no_orderby();

            Assert.Equal(
                @"@__p_0: 5

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY @@ROWCOUNT
OFFSET @__p_0 ROWS",
                Sql);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Skip_Take()
        {
            base.Skip_Take();

            Assert.Equal(
                @"@__p_0: 5
@__p_1: 10

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactName]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY",
                Sql);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Join_Customers_Orders_Skip_Take()
        {
            base.Join_Customers_Orders_Skip_Take();

            Assert.Equal(
                @"@__p_0: 10
@__p_1: 5

SELECT [c].[ContactName], [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [o].[OrderID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY",
                Sql);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Join_Customers_Orders_Projection_With_String_Concat_Skip_Take()
        {
            base.Join_Customers_Orders_Projection_With_String_Concat_Skip_Take();

            Assert.Equal(
                @"@__p_0: 10
@__p_1: 5

SELECT ([c].[ContactName] + N' ') + [c].[ContactTitle], [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [o].[OrderID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY",
                Sql);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Take_Skip()
        {
            base.Take_Skip();

            Assert.Equal(
                @"@__p_0: 10
@__p_1: 5

SELECT [t].*
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactName]
) AS [t]
ORDER BY [t].[ContactName]
OFFSET @__p_1 ROWS",
                Sql);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Take_Skip_Distinct()
        {
            base.Take_Skip_Distinct();

            Assert.Equal(
                @"@__p_0: 10
@__p_1: 5

SELECT DISTINCT [t0].*
FROM (
    SELECT [t].*
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactName]
    ) AS [t]
    ORDER BY [t].[ContactName]
    OFFSET @__p_1 ROWS
) AS [t0]",
                Sql);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Take_Skip_Distinct_Caching()
        {
            base.Take_Skip_Distinct_Caching();

            Assert.Equal(
                @"@__p_0: 10
@__p_1: 5

SELECT DISTINCT [t0].*
FROM (
    SELECT [t].*
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactName]
    ) AS [t]
    ORDER BY [t].[ContactName]
    OFFSET @__p_1 ROWS
) AS [t0]

@__p_0: 15
@__p_1: 10

SELECT DISTINCT [t0].*
FROM (
    SELECT [t].*
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactName]
    ) AS [t]
    ORDER BY [t].[ContactName]
    OFFSET @__p_1 ROWS
) AS [t0]",
                Sql);
        }

        public void Skip_when_no_OrderBy()
        {
            Assert.Throws<Exception>(() => AssertQuery<Customer>(cs => cs.Skip(5).Take(10)));
        }

        public override void Take_Distinct_Count()
        {
            base.Take_Distinct_Count();

            Assert.Equal(
                @"@__p_0: 5

SELECT COUNT(*)
FROM (
    SELECT DISTINCT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
) AS [t]",
                Sql);
        }

        public override void Take_Where_Distinct_Count()
        {
            base.Take_Where_Distinct_Count();

            Assert.Equal(
                @"@__p_0: 5

SELECT COUNT(*)
FROM (
    SELECT DISTINCT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = N'FRANK'
) AS [t]",
                Sql);
        }

        public override void Queryable_simple()
        {
            base.Queryable_simple();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Queryable_simple_anonymous()
        {
            base.Queryable_simple_anonymous();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Queryable_nested_simple()
        {
            base.Queryable_nested_simple();

            Assert.Equal(
                @"SELECT [c3].[CustomerID], [c3].[Address], [c3].[City], [c3].[CompanyName], [c3].[ContactName], [c3].[ContactTitle], [c3].[Country], [c3].[Fax], [c3].[Phone], [c3].[PostalCode], [c3].[Region]
FROM [Customers] AS [c3]",
                Sql);
        }

        public override void Queryable_simple_anonymous_projection_subquery()
        {
            base.Queryable_simple_anonymous_projection_subquery();

            Assert.Equal(
                @"@__p_0: 91

SELECT [t].[City]
FROM (
    SELECT TOP(@__p_0) [c0].*
    FROM [Customers] AS [c0]
) AS [t]",
                Sql);
        }

        public override void Queryable_simple_anonymous_subquery()
        {
            base.Queryable_simple_anonymous_subquery();

            Assert.Equal(
                @"@__p_0: 91

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Take_simple()
        {
            base.Take_simple();

            Assert.Equal(
                @"@__p_0: 10

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void Take_simple_parameterized()
        {
            base.Take_simple_parameterized();

            Assert.Equal(
                @"@__p_0: 10

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void Take_simple_projection()
        {
            base.Take_simple_projection();

            Assert.Equal(
                @"@__p_0: 10

SELECT TOP(@__p_0) [c].[City]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void Take_subquery_projection()
        {
            base.Take_subquery_projection();

            Assert.Equal(
                @"@__p_0: 2

SELECT [t].[City]
FROM (
    SELECT TOP(@__p_0) [c0].*
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
) AS [t]",
                Sql);
        }

        public override void OrderBy_Take_Count()
        {
            base.OrderBy_Take_Count();

            Assert.Equal(
                @"@__p_0: 5

SELECT COUNT(*)
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]",
                Sql);
        }

        public override void Take_OrderBy_Count()
        {
            base.Take_OrderBy_Count();

            Assert.Equal(
                @"@__p_0: 5

SELECT COUNT(*)
FROM (
    SELECT TOP(@__p_0) [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
) AS [t]",
                Sql);
        }

        public override void Any_simple()
        {
            base.Any_simple();

            Assert.Equal(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c])
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void Any_predicate()
        {
            base.Any_predicate();

            Assert.Equal(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        WHERE [c].[ContactName] LIKE N'A' + N'%')
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void Any_nested_negated()
        {
            base.Any_nested_negated();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] LIKE N'A' + N'%')",
                Sql);
        }

        public override void Any_nested_negated2()
        {
            base.Any_nested_negated2();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] <> N'London') OR [c].[City] IS NULL) AND NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] LIKE N'A' + N'%')",
                Sql);
        }

        public override void Any_nested_negated3()
        {
            base.Any_nested_negated3();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] LIKE N'A' + N'%') AND (([c].[City] <> N'London') OR [c].[City] IS NULL)",
                Sql);
        }

        public override void Any_nested()
        {
            base.Any_nested();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] LIKE N'A' + N'%')",
                Sql);
        }

        public override void Any_nested2()
        {
            base.Any_nested2();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] <> N'London') OR [c].[City] IS NULL) AND EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] LIKE N'A' + N'%')",
                Sql);
        }

        public override void Any_nested3()
        {
            base.Any_nested3();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] LIKE N'A' + N'%') AND (([c].[City] <> N'London') OR [c].[City] IS NULL)",
                Sql);
        }

        public override void Any_with_multiple_conditions_still_uses_exists()
        {
            base.Any_with_multiple_conditions_still_uses_exists();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[City] = N'London') AND EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE ([o].[EmployeeID] = 1) AND ([c].[CustomerID] = [o].[CustomerID]))",
                Sql);
        }

        public override void All_top_level()
        {
            base.All_top_level();

            Assert.Equal(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        WHERE NOT ([c].[ContactName] LIKE N'A' + N'%'))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
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

        public override void Select_anonymous_one()
        {
            base.Select_anonymous_one();

            Assert.Equal(
                @"SELECT [c].[City]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Select_anonymous_two()
        {
            base.Select_anonymous_two();

            Assert.Equal(
                @"SELECT [c].[City], [c].[Phone]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Select_anonymous_three()
        {
            base.Select_anonymous_three();

            Assert.Equal(
                @"SELECT [c].[City], [c].[Phone], [c].[Country]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Select_anonymous_bool_constant_true()
        {
            base.Select_anonymous_bool_constant_true();

            Assert.Equal(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Select_anonymous_bool_constant_in_expression()
        {
            base.Select_anonymous_bool_constant_in_expression();

            Assert.Equal(
                @"SELECT [c].[CustomerID], LEN([c].[CustomerID]) + 5
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Select_anonymous_conditional_expression()
        {
            base.Select_anonymous_conditional_expression();

            Assert.Equal(
                @"SELECT [p].[ProductID], CASE
    WHEN [p].[UnitsInStock] > 0
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
FROM [Products] AS [p]",
                Sql);
        }

        public override void Select_scalar_primitive_after_take()
        {
            base.Select_scalar_primitive_after_take();

            Assert.Equal(
                @"@__p_0: 9

SELECT [t].[EmployeeID]
FROM (
    SELECT TOP(@__p_0) [e0].*
    FROM [Employees] AS [e0]
) AS [t]",
                Sql);
        }

        public override void Select_constant_null_string()
        {
            base.Select_constant_null_string();

            Assert.Equal(
                @"SELECT 1
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Select_local()
        {
            base.Select_local();

            Assert.Equal(
                @"@__x_0: 10

SELECT @__x_0
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Where_simple()
        {
            base.Where_simple();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'",
                Sql);
        }

        public override void Where_simple_shadow()
        {
            base.Where_simple_shadow();

            Assert.Equal(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[Title] = N'Sales Representative'",
                Sql);
        }

        public override void Where_simple_shadow_projection()
        {
            base.Where_simple_shadow_projection();

            Assert.Equal(
                @"SELECT [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[Title] = N'Sales Representative'",
                Sql);
        }

        public override void Where_comparison_nullable_type_not_null()
        {
            base.Where_comparison_nullable_type_not_null();

            Assert.Equal(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = 2",
                Sql);
        }

        public override void Where_comparison_nullable_type_null()
        {
            base.Where_comparison_nullable_type_null();

            Assert.Equal(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL",
                Sql);
        }

        public override void Where_client()
        {
            base.Where_client();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Where_client_and_server_top_level()
        {
            base.Where_client_and_server_top_level();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <> N'AROUT'",
                Sql);
        }

        public override void Where_client_or_server_top_level()
        {
            base.Where_client_or_server_top_level();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Where_client_and_server_non_top_level()
        {
            base.Where_client_and_server_non_top_level();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Where_client_deep_inside_predicate_and_server_top_level()
        {
            base.Where_client_deep_inside_predicate_and_server_top_level();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <> N'ALFKI'",
                Sql);
        }

        public override void First_client_predicate()
        {
            base.First_client_predicate();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void Last()
        {
            base.Last();

            Assert.Equal(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactName] DESC",
                Sql);
        }

        public override void Last_when_no_order_by()
        {
            base.Last_when_no_order_by();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'",
                Sql);
        }

        public override void Last_Predicate()
        {
            base.Last_Predicate();

            Assert.Equal(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[ContactName] DESC",
                Sql);
        }

        public override void Where_Last()
        {
            base.Where_Last();

            Assert.Equal(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[ContactName] DESC",
                Sql);
        }

        public override void LastOrDefault()
        {
            base.LastOrDefault();

            Assert.Equal(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactName] DESC",
                Sql);
        }

        public override void LastOrDefault_Predicate()
        {
            base.LastOrDefault_Predicate();

            Assert.Equal(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[ContactName] DESC",
                Sql);
        }

        public override void Where_LastOrDefault()
        {
            base.Where_LastOrDefault();

            Assert.Equal(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[ContactName] DESC",
                Sql);
        }

        public override void Where_equals_method_string()
        {
            base.Where_equals_method_string();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'",
                Sql);
        }

        public override void Where_equals_method_int()
        {
            base.Where_equals_method_int();

            Assert.Equal(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = 1",
                Sql);
        }

        public override void Where_equals_using_object_overload_on_mismatched_types()
        {
            base.Where_equals_using_object_overload_on_mismatched_types();

            Assert.Equal(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 1 = 0",
                Sql);

            Assert.True(TestSqlLoggerFactory.Log.Contains(
                "Possible unintended use of method Equals(object) for arguments of different types: 'e.EmployeeID', '__longPrm_0'. This comparison will always return 'false'."));
        }

        public override void Where_equals_using_int_overload_on_mismatched_types()
        {
            base.Where_equals_using_int_overload_on_mismatched_types();

            Assert.Equal(
                @"@__shortPrm_0: 1

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = @__shortPrm_0",
                Sql);
        }

        public override void Where_equals_on_mismatched_types_int_nullable_int()
        {
            base.Where_equals_on_mismatched_types_int_nullable_int();

            Assert.Equal(
                @"@__intPrm_0: 2

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__intPrm_0

@__intPrm_0: 2

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE @__intPrm_0 = [e].[ReportsTo]",
                Sql);
        }

        public override void Where_equals_on_mismatched_types_nullable_int_long()
        {
            base.Where_equals_on_mismatched_types_nullable_int_long();

            Assert.Equal(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 1 = 0

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 1 = 0",
                Sql);

            Assert.True(TestSqlLoggerFactory.Log.Contains(
                "Possible unintended use of method Equals(object) for arguments of different types: 'e.ReportsTo', '__longPrm_0'. This comparison will always return 'false'."));

            Assert.True(TestSqlLoggerFactory.Log.Contains(
                "Possible unintended use of method Equals(object) for arguments of different types: '__longPrm_0', 'e.ReportsTo'. This comparison will always return 'false'."));
        }

        public override void Where_equals_on_mismatched_types_nullable_long_nullable_int()
        {
            base.Where_equals_on_mismatched_types_nullable_long_nullable_int();

            Assert.Equal(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 1 = 0

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 1 = 0",
                Sql);

            Assert.True(TestSqlLoggerFactory.Log.Contains(
                "Possible unintended use of method Equals(object) for arguments of different types: '__nullableLongPrm_0', 'e.ReportsTo'. This comparison will always return 'false'."));

            Assert.True(TestSqlLoggerFactory.Log.Contains(
                "Possible unintended use of method Equals(object) for arguments of different types: 'e.ReportsTo', '__nullableLongPrm_0'. This comparison will always return 'false'."));
        }

        public override void Where_equals_on_matched_nullable_int_types()
        {
            base.Where_equals_on_matched_nullable_int_types();

            Assert.Equal(
                @"@__nullableIntPrm_0: 2

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE @__nullableIntPrm_0 = [e].[ReportsTo]

@__nullableIntPrm_0: 2

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__nullableIntPrm_0",
                Sql);
        }

        public override void Where_equals_on_null_nullable_int_types()
        {
            base.Where_equals_on_null_nullable_int_types();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Where_string_length()
        {
            base.Where_string_length();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE LEN([c].[City]) = 6",
                Sql);
        }

        public override void Where_datetime_now()
        {
            base.Where_datetime_now();

            Assert.Equal(
                @"@__myDatetime_0: 04/10/2015 00:00:00

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE GETDATE() <> @__myDatetime_0",
                Sql);
        }

        public override void Where_datetime_utcnow()
        {
            base.Where_datetime_utcnow();

            Assert.Equal(
                @"@__myDatetime_0: 04/10/2015 00:00:00

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE GETUTCDATE() <> @__myDatetime_0",
                Sql);
        }

        public override void Where_is_null()
        {
            base.Where_is_null();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] IS NULL",
                Sql);
        }

        public override void Where_is_not_null()
        {
            base.Where_is_not_null();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] IS NOT NULL",
                Sql);
        }

        public override void Where_null_is_null()
        {
            base.Where_null_is_null();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 1 = 1",
                Sql);
        }

        public override void Where_constant_is_null()
        {
            base.Where_constant_is_null();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 1 = 0",
                Sql);
        }

        public override void Where_null_is_not_null()
        {
            base.Where_null_is_not_null();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 1 = 0",
                Sql);
        }

        public override void Where_constant_is_not_null()
        {
            base.Where_constant_is_not_null();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 1 = 1",
                Sql);
        }

        public override void Where_simple_reversed()
        {
            base.Where_simple_reversed();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE N'London' = [c].[City]",
                Sql);
        }

        public override void Where_identity_comparison()
        {
            base.Where_identity_comparison();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[City] = [c].[City]) OR ([c].[City] IS NULL AND [c].[City] IS NULL)",
                Sql);
        }

        public override void Where_select_many_or()
        {
            base.Where_select_many_or();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE ([c].[City] = N'London') OR ([e].[City] = N'London')",
                Sql);
        }

        public override void Where_select_many_or2()
        {
            base.Where_select_many_or2();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] IN (N'London', N'Berlin')",
                Sql);
        }

        public override void Where_select_many_or3()
        {
            base.Where_select_many_or3();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] IN (N'London', N'Berlin', N'Seattle')",
                Sql);
        }

        public override void Where_select_many_or4()
        {
            base.Where_select_many_or4();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] IN (N'London', N'Berlin', N'Seattle', N'Lisboa')",
                Sql);
        }

        public override void Where_select_many_or_with_parameter()
        {
            base.Where_select_many_or_with_parameter();

            Assert.Equal(
                @"@__london_0: London
@__lisboa_1: Lisboa

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] IN (@__london_0, N'Berlin', N'Seattle', @__lisboa_1)",
                Sql);
        }

        public override void Where_in_optimization_multiple()
        {
            base.Where_in_optimization_multiple();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE ([c].[City] IN (N'London', N'Berlin') OR ([c].[CustomerID] = N'ALFKI')) OR ([c].[CustomerID] = N'ABCDE')",
                Sql);
        }

        public override void Where_not_in_optimization1()
        {
            base.Where_not_in_optimization1();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE (([c].[City] <> N'London') OR [c].[City] IS NULL) AND (([e].[City] <> N'London') OR [e].[City] IS NULL)",
                Sql);
        }

        public override void Where_not_in_optimization2()
        {
            base.Where_not_in_optimization2();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] NOT IN (N'London', N'Berlin')",
                Sql);
        }

        public override void Where_not_in_optimization3()
        {
            base.Where_not_in_optimization3();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] NOT IN (N'London', N'Berlin', N'Seattle')",
                Sql);
        }

        public override void Where_not_in_optimization4()
        {
            base.Where_not_in_optimization4();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] NOT IN (N'London', N'Berlin', N'Seattle', N'Lisboa')",
                Sql);
        }

        public override void Where_select_many_and()
        {
            base.Where_select_many_and();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE (([c].[City] = N'London') AND ([c].[Country] = N'UK')) AND (([e].[City] = N'London') AND ([e].[Country] = N'UK'))",
                Sql);
        }

        public override void Select_project_filter()
        {
            base.Select_project_filter();

            Assert.Equal(
                @"SELECT [c].[CompanyName]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'",
                Sql);
        }

        public override void Select_project_filter2()
        {
            base.Select_project_filter2();

            Assert.Equal(
                @"SELECT [c].[City]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'",
                Sql);
        }

        public override void SelectMany_mixed()
        {
            base.SelectMany_mixed();

            Assert.Equal(3763, Sql.Replace("\r", "").Replace("\n", "").Length); // new-line insensitive assertion
            Assert.StartsWith(
                @"SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
FROM [Employees] AS [e1]

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void SelectMany_simple_subquery()
        {
            base.SelectMany_simple_subquery();

            Assert.Equal(
                @"@__p_0: 9

SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT TOP(@__p_0) [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title]
    FROM [Employees] AS [e0]
) AS [t]
CROSS JOIN [Customers] AS [c]",
                Sql);
        }

        public override void SelectMany_simple1()
        {
            base.SelectMany_simple1();

            Assert.Equal(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Employees] AS [e]
CROSS JOIN [Customers] AS [c]",
                Sql);
        }

        public override void SelectMany_simple2()
        {
            base.SelectMany_simple2();

            Assert.Equal(
                @"SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e2].[FirstName]
FROM [Employees] AS [e1]
CROSS JOIN [Customers] AS [c]
CROSS JOIN [Employees] AS [e2]",
                Sql);
        }

        public override void SelectMany_entity_deep()
        {
            base.SelectMany_entity_deep();

            Assert.Equal(
                @"SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title], [e2].[EmployeeID], [e2].[City], [e2].[Country], [e2].[FirstName], [e2].[ReportsTo], [e2].[Title], [e3].[EmployeeID], [e3].[City], [e3].[Country], [e3].[FirstName], [e3].[ReportsTo], [e3].[Title], [e4].[EmployeeID], [e4].[City], [e4].[Country], [e4].[FirstName], [e4].[ReportsTo], [e4].[Title]
FROM [Employees] AS [e1]
CROSS JOIN [Employees] AS [e2]
CROSS JOIN [Employees] AS [e3]
CROSS JOIN [Employees] AS [e4]",
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
CROSS JOIN [Orders] AS [o]",
                Sql);
        }

        public override void SelectMany_LongCount()
        {
            base.SelectMany_LongCount();

            Assert.Equal(
                @"SELECT COUNT_BIG(*)
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]",
                Sql);
        }

        public override void SelectMany_OrderBy_ThenBy_Any()
        {
            base.SelectMany_OrderBy_ThenBy_Any();

            Assert.Equal(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        CROSS JOIN [Orders] AS [o])
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
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
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]",
                Sql);
        }

        public override void Join_composite_key()
        {
            base.Join_composite_key();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON ([c].[CustomerID] = [o].[CustomerID]) AND ([c].[CustomerID] = [o].[CustomerID])",
                Sql);
        }

        public override void Join_client_new_expression()
        {
            base.Join_client_new_expression();

            // See issue#4458
            Assert.Contains(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]",
                Sql);

            Assert.Contains(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Join_select_many()
        {
            base.Join_select_many();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
CROSS JOIN [Employees] AS [e]",
                Sql);
        }

        public override void Client_Join_select_many()
        {
            base.Client_Join_select_many();

            // See issue#4458
            Assert.Contains(
                @"SELECT [e2].[EmployeeID], [e2].[City], [e2].[Country], [e2].[FirstName], [e2].[ReportsTo], [e2].[Title]
FROM [Employees] AS [e2]",
                Sql);

            Assert.Contains(
                @"SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
FROM [Employees] AS [e1]",
                Sql);

            Assert.Contains(
                @"SELECT [e3].[EmployeeID], [e3].[City], [e3].[Country], [e3].[FirstName], [e3].[ReportsTo], [e3].[Title]
FROM [Employees] AS [e3]",
                Sql);
        }

        public override void Join_Where_Count()
        {
            base.Join_Where_Count();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'",
                Sql);
        }

        public override void Join_OrderBy_Count()
        {
            base.Join_OrderBy_Count();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]",
                Sql);
        }

        public override void Join_customers_orders_with_subquery()
        {
            base.Join_customers_orders_with_subquery();

            // See issue#4458
            Assert.Contains(
                @"SELECT [o2].[CustomerID], [o2].[OrderID]
FROM [Orders] AS [o2]
ORDER BY [o2].[OrderID]",
                Sql);

            Assert.Contains(
                @"SELECT [c].[CustomerID], [c].[ContactName]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Join_customers_orders_with_subquery_with_take()
        {
            base.Join_customers_orders_with_subquery_with_take();

            Assert.Equal(
                @"@__p_0: 5

SELECT [c].[ContactName], [t].[OrderID]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT TOP(@__p_0) [o20].*
    FROM [Orders] AS [o20]
    ORDER BY [o20].[OrderID]
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]
WHERE [t].[CustomerID] = N'ALFKI'",
                Sql);
        }

        public override void Join_customers_orders_with_subquery_anonymous_property_method()
        {
            base.Join_customers_orders_with_subquery_anonymous_property_method();

            // See issue#4458
            Assert.Contains(
                @"SELECT [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate]
FROM [Orders] AS [o2]
ORDER BY [o2].[OrderID]",
                Sql);

            Assert.Contains(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Join_customers_orders_with_subquery_anonymous_property_method_with_take()
        {
            base.Join_customers_orders_with_subquery_anonymous_property_method_with_take();

            // See issue#4458
            Assert.Contains(
                @"@__p_0: 5

SELECT TOP(@__p_0) [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate]
FROM [Orders] AS [o2]
ORDER BY [o2].[OrderID]",
                Sql);

            Assert.Contains(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Join_customers_orders_with_subquery_predicate()
        {
            base.Join_customers_orders_with_subquery_predicate();

            // See issue#4458
            Assert.Contains(
                @"SELECT [o2].[CustomerID], [o2].[OrderID]
FROM [Orders] AS [o2]
WHERE [o2].[OrderID] > 0
ORDER BY [o2].[OrderID]",
                Sql);

            Assert.Contains(
                @"SELECT [c].[CustomerID], [c].[ContactName]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Join_customers_orders_with_subquery_predicate_with_take()
        {
            base.Join_customers_orders_with_subquery_predicate_with_take();

            Assert.Equal(
                @"@__p_0: 5

SELECT [c].[ContactName], [t].[OrderID]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT TOP(@__p_0) [o20].*
    FROM [Orders] AS [o20]
    WHERE [o20].[OrderID] > 0
    ORDER BY [o20].[OrderID]
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]
WHERE [t].[CustomerID] = N'ALFKI'",
                Sql);
        }

        public override void Join_customers_orders_select()
        {
            base.Join_customers_orders_select();

            Assert.Equal(
                @"SELECT [c].[ContactName], [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]",
                Sql);
        }

        public override void Multiple_joins_Where_Order_Any()
        {
            base.Multiple_joins_Where_Order_Any();

            Assert.Equal(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        INNER JOIN [Orders] AS [or] ON [c].[CustomerID] = [or].[CustomerID]
        INNER JOIN [Order Details] AS [od] ON [or].[OrderID] = [od].[OrderID]
        WHERE [c].[City] = N'London')
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void Where_join_select()
        {
            base.Where_join_select();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'",
                Sql);
        }

        public override void Where_orderby_join_select()
        {
            base.Where_orderby_join_select();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] <> N'ALFKI'
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void Where_join_orderby_join_select()
        {
            base.Where_join_orderby_join_select();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
INNER JOIN [Order Details] AS [od] ON [o].[OrderID] = [od].[OrderID]
WHERE [c].[CustomerID] <> N'ALFKI'
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void Where_select_many()
        {
            base.Where_select_many();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
WHERE [c].[CustomerID] = N'ALFKI'",
                Sql);
        }

        public override void Where_orderby_select_many()
        {
            base.Where_orderby_select_many();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void GroupBy_simple()
        {
            base.GroupBy_simple();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void GroupBy_Distinct()
        {
            base.GroupBy_Distinct();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void GroupBy_Count()
        {
            base.GroupBy_Count();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void GroupBy_LongCount()
        {
            base.GroupBy_LongCount();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void Select_GroupBy()
        {
            base.Select_GroupBy();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID]
FROM [Orders] AS [o]
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void Select_GroupBy_SelectMany()
        {
            base.Select_GroupBy_SelectMany();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID]
FROM [Orders] AS [o]
ORDER BY [o].[OrderID]",
                Sql);
        }

        public override void SelectMany_cartesian_product_with_ordering()
        {
            base.SelectMany_cartesian_product_with_ordering();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[City]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE ([c].[City] = [e].[City]) OR ([c].[City] IS NULL AND [e].[City] IS NULL)
ORDER BY [e].[City], [c].[CustomerID] DESC",
                Sql);
        }

        public override void GroupJoin_DefaultIfEmpty()
        {
            base.GroupJoin_DefaultIfEmpty();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void GroupJoin_DefaultIfEmpty2()
        {
            base.GroupJoin_DefaultIfEmpty2();

            Assert.Equal(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Employees] AS [e]
LEFT JOIN [Orders] AS [o] ON [e].[EmployeeID] = [o].[EmployeeID]
ORDER BY [e].[EmployeeID]",
                Sql);
        }

        public override void GroupJoin_DefaultIfEmpty3()
        {
            base.GroupJoin_DefaultIfEmpty3();

            Assert.Equal(
                @"@__p_0: 1

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [c0].*
    FROM [Customers] AS [c0]
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[CustomerID]",
                Sql);
        }

        public override void GroupJoin_simple()
        {
            base.GroupJoin_simple();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void GroupJoin_simple2()
        {
            base.GroupJoin_simple2();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void GroupJoin_simple_ordering()
        {
            base.GroupJoin_simple_ordering();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[City], [c].[CustomerID]",
                Sql);
        }

        public override void GroupJoin_simple_subquery()
        {
            base.GroupJoin_simple_subquery();

            Assert.Equal(
                @"@__p_0: 4

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT TOP(@__p_0) [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    ORDER BY [o0].[OrderID]
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void GroupJoin_customers_orders_count()
        {
            base.GroupJoin_customers_orders_count();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void SelectMany_Joined_DefaultIfEmpty()
        {
            base.SelectMany_Joined_DefaultIfEmpty();

            Assert.StartsWith(
                @"SELECT [t1].[OrderID], [t1].[CustomerID], [t1].[EmployeeID], [t1].[OrderDate], [c].[ContactName]
FROM [Customers] AS [c]
CROSS APPLY (
    SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
    FROM (
        SELECT NULL AS [empty]
    ) AS [empty0]
    LEFT JOIN (
        SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
        FROM [Orders] AS [o0]
        WHERE [o0].[CustomerID] = [c].[CustomerID]
    ) AS [t] ON 1 = 1
) AS [t1]",
                Sql);
        }

        public override void SelectMany_Joined_DefaultIfEmpty2()
        {
            base.SelectMany_Joined_DefaultIfEmpty2();

            Assert.StartsWith(
                @"SELECT [t1].[OrderID], [t1].[CustomerID], [t1].[EmployeeID], [t1].[OrderDate]
FROM [Customers] AS [c]
CROSS APPLY (
    SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
    FROM (
        SELECT NULL AS [empty]
    ) AS [empty0]
    LEFT JOIN (
        SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
        FROM [Orders] AS [o0]
        WHERE [o0].[CustomerID] = [c].[CustomerID]
    ) AS [t] ON 1 = 1
) AS [t1]",
                Sql);
        }

        public override void SelectMany_Joined_Take()
        {
            base.SelectMany_Joined_Take();

            Assert.Equal(
                @"SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], [c].[ContactName]
FROM [Customers] AS [c]
CROSS APPLY (
    SELECT TOP(1000) [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    WHERE [o0].[CustomerID] = [c].[CustomerID]
) AS [t]",
                Sql);
        }

        public override void Take_with_single()
        {
            base.Take_with_single();

            Assert.Equal(
                @"@__p_0: 1

SELECT TOP(2) [t].*
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [t]",
                Sql);
        }

        public override void Take_with_single_select_many()
        {
            base.Take_with_single_select_many();

            Assert.Equal(
                @"@__p_0: 1

SELECT TOP(2) [t].*
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID] AS [c0], [o].[EmployeeID], [o].[OrderDate]
    FROM [Customers] AS [c]
    CROSS JOIN [Orders] AS [o]
    ORDER BY [c].[CustomerID], [o].[OrderID]
) AS [t]",
                Sql);
        }

        public override void Distinct()
        {
            base.Distinct();

            Assert.Equal(
                @"SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
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

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Distinct_Skip() => base.Distinct_Skip();

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Distinct_Skip_Take() => base.Distinct_Skip_Take();

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Skip_Distinct() => base.Skip_Distinct();

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Skip_Take_Distinct() => base.Skip_Take_Distinct();

        public override void OrderBy()
        {
            base.OrderBy();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void OrderBy_anon()
        {
            base.OrderBy_anon();

            Assert.Equal(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void OrderBy_anon2()
        {
            base.OrderBy_anon2();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void OrderBy_client_mixed()
        {
            base.OrderBy_client_mixed();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void OrderBy_multiple_queries()
        {
            base.OrderBy_multiple_queries();

            // See issue#4458
            Assert.Contains(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);

            Assert.Contains(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]",
                Sql);
        }

        public override void OrderBy_Distinct()
        {
            base.OrderBy_Distinct();

            Assert.Equal(
                @"SELECT DISTINCT [c].[City]
FROM [Customers] AS [c]", // Ordering not preserved by distinct when ordering columns not projected.
                Sql);
        }

        public override void Distinct_OrderBy()
        {
            base.Distinct_OrderBy();

            Assert.Equal(
                @"SELECT [t].[Country]
FROM (
    SELECT DISTINCT [c0].[Country]
    FROM [Customers] AS [c0]
) AS [t]
ORDER BY [t].[Country]",
                Sql);
        }

        public override void Distinct_OrderBy2()
        {
            base.Distinct_OrderBy2();

            Assert.Equal(
                @"SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT DISTINCT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
) AS [t]
ORDER BY [t].[CustomerID]",
                Sql);
        }

        public override void Distinct_OrderBy3()
        {
            base.Distinct_OrderBy3();

            Assert.Equal(
                @"SELECT [t].[CustomerID]
FROM (
    SELECT DISTINCT [c0].[CustomerID]
    FROM [Customers] AS [c0]
) AS [t]
ORDER BY [t].[CustomerID]",
                Sql);
        }

        public override void Take_Distinct()
        {
            base.Take_Distinct();

            Assert.Equal(
                @"@__p_0: 5

SELECT DISTINCT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
ORDER BY [o].[OrderID]",
                Sql);
        }

        public override void OrderBy_shadow()
        {
            base.OrderBy_shadow();

            Assert.Equal(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
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
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c])
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void OrderBy_correlated_subquery_lol()
        {
            base.OrderBy_correlated_subquery_lol();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY (
    SELECT CASE
        WHEN EXISTS (
            SELECT 1
            FROM [Customers] AS [c2]
            WHERE [c2].[CustomerID] = [c].[CustomerID])
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END
)",
                Sql);
        }

        public override void OrderBy_correlated_subquery_lol2()
        {
            base.OrderBy_correlated_subquery_lol2();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE (
    SELECT TOP(1) [c].[City]
    FROM [Customers] AS [c]
    ORDER BY (
        SELECT CASE
            WHEN EXISTS (
                SELECT 1
                FROM [Customers] AS [c2]
                WHERE [c2].[CustomerID] = N'ALFKI')
            THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
        END
    )
) <> N'Nowhere'",
                Sql);
        }

        public override void Where_subquery_recursive_trivial()
        {
            base.Where_subquery_recursive_trivial();

            Assert.Equal(
                @"SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
FROM [Employees] AS [e1]
WHERE EXISTS (
    SELECT 1
    FROM [Employees] AS [e2]
    WHERE EXISTS (
        SELECT 1
        FROM [Employees] AS [e3]))
ORDER BY [e1].[EmployeeID]",
                Sql);
        }

        public override void Where_false()
        {
            base.Where_false();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 1 = 0",
                Sql);
        }

        public override void Where_ternary_boolean_condition()
        {
            base.Where_ternary_boolean_condition();

            Assert.Contains(
                    @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE (@__flag_0 = 1 AND ([p].[UnitsInStock] >= 20)) OR ((@__flag_0 <> 1) AND ([p].[UnitsInStock] < 20))",
                    Sql);
        }

        public override void Where_ternary_boolean_condition_with_another_condition()
        {
            base.Where_ternary_boolean_condition_with_another_condition();

            Assert.Equal(
                    @"@__productId_0: 15
@__flag_1: True

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ([p].[ProductID] < @__productId_0) AND ((@__flag_1 = 1 AND ([p].[UnitsInStock] >= 20)) OR ((@__flag_1 <> 1) AND ([p].[UnitsInStock] < 20)))",
                    Sql);
        }

        public override void Where_ternary_boolean_condition_with_false_as_result()
        {
            base.Where_ternary_boolean_condition_with_false_as_result();

            Assert.Contains(
                    @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE @__flag_0 = 1 AND ([p].[UnitsInStock] >= 20)",
                    Sql);
        }

        public override void Where_concat_string_int_comparison1()
        {
            base.Where_concat_string_int_comparison1();

            Assert.Equal(
                @"@__i_0: 10

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] + CAST(@__i_0 AS nvarchar(max))) = [c].[CompanyName]",
                Sql);
        }

        public override void Where_concat_string_int_comparison2()
        {
            base.Where_concat_string_int_comparison2();

            Assert.Equal(
                @"@__i_0: 10

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE (CAST(@__i_0 AS nvarchar(max)) + [c].[CustomerID]) = [c].[CompanyName]",
                Sql);
        }

        public override void Where_concat_string_int_comparison3()
        {
            base.Where_concat_string_int_comparison3();

            Assert.Equal(
                @"@__i_0: 10
@__j_1: 21

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE (((CAST(@__i_0 + 20 AS nvarchar(max)) + [c].[CustomerID]) + CAST(@__j_1 AS nvarchar(max))) + CAST(42 AS nvarchar(max))) = [c].[CompanyName]",
                Sql);
        }

        public override void Where_primitive()
        {
            base.Where_primitive();

            Assert.Equal(
                @"@__p_0: 9

SELECT [t].[EmployeeID]
FROM (
    SELECT TOP(@__p_0) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
) AS [t]
WHERE [t].[EmployeeID] = 5",
                Sql);
        }

        public override void Where_bool_member()
        {
            base.Where_bool_member();

            Assert.Equal(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 1",
                Sql);
        }

        public override void Where_bool_member_false()
        {
            base.Where_bool_member_false();

            Assert.Equal(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 0",
                Sql);
        }

        public override void Where_bool_client_side_negated()
        {
            base.Where_bool_client_side_negated();

            Assert.Equal(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 1",
                Sql);
        }

        public override void Where_bool_member_negated_twice()
        {
            base.Where_bool_member_negated_twice();

            Assert.Equal(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 1",
                Sql);
        }

        public override void Where_bool_member_shadow()
        {
            base.Where_bool_member_shadow();

            Assert.Equal(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 1",
                Sql);
        }

        public override void Where_bool_member_false_shadow()
        {
            base.Where_bool_member_false_shadow();

            Assert.Equal(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 0",
                Sql);
        }

        public override void Where_bool_member_equals_constant()
        {
            base.Where_bool_member_equals_constant();

            Assert.Equal(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 1",
                Sql);
        }

        public override void Where_bool_member_in_complex_predicate()
        {
            base.Where_bool_member_in_complex_predicate();

            Assert.Equal(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE (([p].[ProductID] > 100) AND [p].[Discontinued] = 1) OR [p].[Discontinued] = 1",
                Sql);
        }

        public override void Where_bool_member_compared_to_binary_expression()
        {
            base.Where_bool_member_compared_to_binary_expression();

            Assert.Equal(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CASE
    WHEN [p].[ProductID] > 50
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void Where_not_bool_member_compared_to_binary_expression()
        {
            base.Where_not_bool_member_compared_to_binary_expression();

            Assert.Equal(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] <> CASE
    WHEN [p].[ProductID] > 50
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void Where_not_bool_member_compared_to_not_bool_member()
        {
            base.Where_not_bool_member_compared_to_not_bool_member();

            Assert.Equal(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = [p].[Discontinued]",
                Sql);
        }

        public override void Where_negated_boolean_expression_compared_to_another_negated_boolean_expression()
        {
            base.Where_negated_boolean_expression_compared_to_another_negated_boolean_expression();

            Assert.Equal(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE CASE
    WHEN [p].[ProductID] > 50
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN [p].[ProductID] > 20
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void Where_bool_parameter_compared_to_binary_expression()
        {
            base.Where_bool_parameter_compared_to_binary_expression();

            Assert.Equal(
                @"@__prm_0: True

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE CASE
    WHEN [p].[ProductID] > 50
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END <> @__prm_0",
                Sql);
        }

        public override void Where_bool_member_and_parameter_compared_to_binary_expression_nested()
        {
            base.Where_bool_member_and_parameter_compared_to_binary_expression_nested();

            Assert.Equal(
                @"@__prm_0: True

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CASE
    WHEN CASE
        WHEN [p].[ProductID] > 50
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END <> @__prm_0
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void Where_de_morgan_or_optimizated()
        {
            base.Where_de_morgan_or_optimizated();

            Assert.Equal(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 0 AND ([p].[ProductID] >= 20)",
                Sql);
        }

        public override void Where_de_morgan_and_optimizated()
        {
            base.Where_de_morgan_and_optimizated();

            Assert.Equal(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 0 OR ([p].[ProductID] >= 20)",
                Sql);
        }

        public override void Where_complex_negated_expression_optimized()
        {
            base.Where_complex_negated_expression_optimized();

            Assert.Equal(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ([p].[Discontinued] = 0 AND ([p].[ProductID] < 60)) AND ([p].[ProductID] > 30)",
                Sql);
        }

        public override void Where_short_member_comparison()
        {
            base.Where_short_member_comparison();

            Assert.Equal(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[UnitsInStock] > 10",
                Sql);
        }

        public override void Where_comparison_to_nullable_bool()
        {
            base.Where_comparison_to_nullable_bool();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'%' + N'KI'",
                Sql);
        }

        public override void Where_true()
        {
            base.Where_true();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 1 = 1",
                Sql);
        }

        public override void Where_compare_constructed_equal()
        {
            base.Where_compare_constructed_equal();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Where_compare_constructed_multi_value_equal()
        {
            base.Where_compare_constructed_multi_value_equal();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Where_compare_constructed_multi_value_not_equal()
        {
            base.Where_compare_constructed_multi_value_not_equal();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Where_compare_constructed()
        {
            base.Where_compare_constructed();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Where_compare_null()
        {
            base.Where_compare_null();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] IS NULL AND ([c].[Country] = N'UK')",
                Sql);
        }

        public override void Single_Predicate()
        {
            base.Single_Predicate();

            Assert.Equal(
                @"SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'",
                Sql);
        }

        public override void Projection_when_arithmetic_expression_precendence()
        {
            base.Projection_when_arithmetic_expression_precendence();

            Assert.Equal(
                @"SELECT [o].[OrderID] / ([o].[OrderID] / 2), ([o].[OrderID] / [o].[OrderID]) / 2
FROM [Orders] AS [o]",
                Sql);
        }

        // TODO: Complex projection translation.

//        public override void Projection_when_arithmetic_expressions()
//        {
//            base.Projection_when_arithmetic_expressions();
//
//            Assert.Equal(
//                @"SELECT [o].[OrderID], [o].[OrderID] * 2, [o].[OrderID] + 23, 100000 - [o].[OrderID], [o].[OrderID] / ([o].[OrderID] / 2)
//FROM [Orders] AS [o]",
//                Sql);
//        }
//
//        public override void Projection_when_arithmetic_mixed()
//        {
//            //base.Projection_when_arithmetic_mixed();
//        }
//
//        public override void Projection_when_arithmetic_mixed_subqueries()
//        {
//            //base.Projection_when_arithmetic_mixed_subqueries();
//        }

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
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE N'M' + N'%'",
                Sql);
        }

        public override void String_StartsWith_Identity()
        {
            base.String_StartsWith_Identity();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE [c].[ContactName] + N'%'",
                Sql);
        }

        public override void String_StartsWith_Column()
        {
            base.String_StartsWith_Column();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE [c].[ContactName] + N'%'",
                Sql);
        }

        public override void String_StartsWith_MethodCall()
        {
            base.String_StartsWith_MethodCall();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void String_EndsWith_Literal()
        {
            base.String_EndsWith_Literal();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE N'%' + N'b'",
                Sql);
        }

        public override void String_EndsWith_Identity()
        {
            base.String_EndsWith_Identity();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE N'%' + [c].[ContactName]",
                Sql);
        }

        public override void String_EndsWith_Column()
        {
            base.String_EndsWith_Column();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE N'%' + [c].[ContactName]",
                Sql);
        }

        public override void String_EndsWith_MethodCall()
        {
            base.String_EndsWith_MethodCall();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void String_Contains_Literal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains("M")), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains("M") || c.ContactName.Contains("m")), // case-sensitive
                entryCount: 34);

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE (N'%' + N'M') + N'%'",
                Sql);
        }

        public override void String_Contains_Identity()
        {
            base.String_Contains_Identity();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE (N'%' + [c].[ContactName]) + N'%'",
                Sql);
        }

        public override void String_Contains_Column()
        {
            base.String_Contains_Column();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE (N'%' + [c].[ContactName]) + N'%'",
                Sql);
        }

        public override void String_Contains_MethodCall()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains(LocalMethod1())),
                entryCount: 19);

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void String_Compare_simple_zero()
        {
            base.String_Compare_simple_zero();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <> N'ALFKI'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > N'ALFKI'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= N'ALFKI'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > N'ALFKI'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= N'ALFKI'",
                Sql);
        }

        public override void String_Compare_simple_one()
        {
            base.String_Compare_simple_one();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > N'ALFKI'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] < N'ALFKI'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= N'ALFKI'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= N'ALFKI'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] >= N'ALFKI'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] >= N'ALFKI'",
                Sql);
        }

        public override void String_Compare_simple_client()
        {
            base.String_Compare_simple_client();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void String_Compare_nested()
        {
            base.String_Compare_nested();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'M' + [c].[CustomerID]

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <> UPPER([c].[CustomerID])

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > REPLACE(N'ALFKI', UPPER(N'ALF'), [c].[CustomerID])

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= N'M' + [c].[CustomerID]

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > UPPER([c].[CustomerID])

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] < REPLACE(N'ALFKI', UPPER(N'ALF'), [c].[CustomerID])",
                Sql);
        }

        public override void String_Compare_multi_predicate()
        {
            base.String_Compare_multi_predicate();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] >= N'ALFKI' AND [c].[CustomerID] < N'CACTU'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactTitle] = N'Owner' AND [c].[Country] <> N'USA'",
                Sql);
        }

        public override void Where_math_abs1()
        {
            base.Where_math_abs1();

            Assert.Equal(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE ABS([od].[ProductID]) > 10",
                Sql);
        }

        public override void Where_math_abs2()
        {
            base.Where_math_abs2();

            Assert.Equal(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE ABS([od].[Quantity]) > 10",
                Sql);
        }

        public override void Where_math_abs3()
        {
            base.Where_math_abs3();

            Assert.Equal(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE ABS([od].[UnitPrice]) > 10.0",
                Sql);
        }

        public override void Where_math_abs_uncorrelated()
        {
            base.Where_math_abs_uncorrelated();

            Assert.Equal(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE ABS(-10) < [od].[ProductID]",
                Sql);
        }

        public override void Where_math_ceiling1()
        {
            base.Where_math_ceiling1();

            Assert.Equal(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE CEILING([od].[Discount]) > 0E0",
                Sql);
        }

        public override void Where_math_ceiling2()
        {
            base.Where_math_ceiling2();

            Assert.Equal(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE CEILING([od].[UnitPrice]) > 10.0",
                Sql);
        }

        public override void Where_math_floor()
        {
            base.Where_math_floor();

            Assert.Equal(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE FLOOR([od].[UnitPrice]) > 10.0",
                Sql);
        }

        public override void Where_query_composition4()
        {
            base.Where_query_composition4();

            Assert.StartsWith(
                @"SELECT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
FROM [Customers] AS [c1]

SELECT 1
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c0]",
                Sql);
        }

        public override void Where_math_power()
        {
            base.Where_math_power();

            Assert.Equal(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE POWER([od].[Discount], 2E0) > 0.0500000007450581E0",
                Sql);
        }

        public override void Where_math_round()
        {
            base.Where_math_round();

            Assert.Equal(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE ROUND([od].[UnitPrice], 0) > 10.0",
                Sql);
        }

        public override void Where_math_truncate()
        {
            base.Where_math_truncate();

            Assert.Equal(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE ROUND([od].[UnitPrice], 0, 1) > 10.0",
                Sql);
        }

        public override void Where_guid_newguid()
        {
            base.Where_guid_newguid();

            Assert.Equal(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE NEWID() <> '00000000-0000-0000-0000-000000000000'",
                Sql);
        }

        public override void Where_functions_nested()
        {
            base.Where_functions_nested();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE POWER(LEN([c].[CustomerID]), 2E0) = 25E0",
                Sql);
        }

        public override void Where_string_to_lower()
        {
            base.Where_string_to_lower();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE LOWER([c].[CustomerID]) = N'alfki'",
                Sql);
        }

        public override void Where_string_to_upper()
        {
            base.Where_string_to_upper();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE UPPER([c].[CustomerID]) = N'ALFKI'",
                Sql);
        }

        public override void Convert_ToByte()
        {
            base.Convert_ToByte();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(tinyint, CONVERT(tinyint, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(tinyint, CONVERT(decimal, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(tinyint, CONVERT(float, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(tinyint, CONVERT(float, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(tinyint, CONVERT(smallint, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(tinyint, CONVERT(int, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(tinyint, CONVERT(bigint, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(tinyint, CONVERT(nvarchar, [o].[OrderID] % 1)) >= 0)",
                Sql);
        }

        public override void Convert_ToDecimal()
        {
            base.Convert_ToDecimal();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(decimal, CONVERT(tinyint, [o].[OrderID] % 1)) >= 0.0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(decimal, CONVERT(decimal, [o].[OrderID] % 1)) >= 0.0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(decimal, CONVERT(float, [o].[OrderID] % 1)) >= 0.0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(decimal, CONVERT(float, [o].[OrderID] % 1)) >= 0.0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(decimal, CONVERT(smallint, [o].[OrderID] % 1)) >= 0.0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(decimal, CONVERT(int, [o].[OrderID] % 1)) >= 0.0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(decimal, CONVERT(bigint, [o].[OrderID] % 1)) >= 0.0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(decimal, CONVERT(nvarchar, [o].[OrderID] % 1)) >= 0.0)",
                Sql);
        }

        public override void Convert_ToDouble()
        {
            base.Convert_ToDouble();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(float, CONVERT(tinyint, [o].[OrderID] % 1)) >= 0E0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(float, CONVERT(decimal, [o].[OrderID] % 1)) >= 0E0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(float, CONVERT(float, [o].[OrderID] % 1)) >= 0E0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(float, CONVERT(float, [o].[OrderID] % 1)) >= 0E0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(float, CONVERT(smallint, [o].[OrderID] % 1)) >= 0E0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(float, CONVERT(int, [o].[OrderID] % 1)) >= 0E0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(float, CONVERT(bigint, [o].[OrderID] % 1)) >= 0E0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(float, CONVERT(nvarchar, [o].[OrderID] % 1)) >= 0E0)",
                Sql);
        }

        public override void Convert_ToInt16()
        {
            base.Convert_ToInt16();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(smallint, CONVERT(tinyint, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(smallint, CONVERT(decimal, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(smallint, CONVERT(float, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(smallint, CONVERT(float, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(smallint, CONVERT(smallint, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(smallint, CONVERT(int, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(smallint, CONVERT(bigint, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(smallint, CONVERT(nvarchar, [o].[OrderID] % 1)) >= 0)",
                Sql);
        }

        public override void Convert_ToInt32()
        {
            base.Convert_ToInt32();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(int, CONVERT(tinyint, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(int, CONVERT(decimal, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(int, CONVERT(float, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(int, CONVERT(float, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(int, CONVERT(smallint, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(int, CONVERT(int, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(int, CONVERT(bigint, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(int, CONVERT(nvarchar, [o].[OrderID] % 1)) >= 0)",
                Sql);
        }

        public override void Convert_ToInt64()
        {
            base.Convert_ToInt64();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bigint, CONVERT(tinyint, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bigint, CONVERT(decimal, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bigint, CONVERT(float, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bigint, CONVERT(float, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bigint, CONVERT(smallint, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bigint, CONVERT(int, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bigint, CONVERT(bigint, [o].[OrderID] % 1)) >= 0)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bigint, CONVERT(nvarchar, [o].[OrderID] % 1)) >= 0)",
                Sql);
        }

        public override void Convert_ToString()
        {
            base.Convert_ToString();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(nvarchar, CONVERT(tinyint, [o].[OrderID] % 1)) <> '10')

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(nvarchar, CONVERT(decimal, [o].[OrderID] % 1)) <> '10')

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(nvarchar, CONVERT(float, [o].[OrderID] % 1)) <> '10')

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(nvarchar, CONVERT(float, [o].[OrderID] % 1)) <> '10')

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(nvarchar, CONVERT(smallint, [o].[OrderID] % 1)) <> '10')

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(nvarchar, CONVERT(int, [o].[OrderID] % 1)) <> '10')

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(nvarchar, CONVERT(bigint, [o].[OrderID] % 1)) <> '10')

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(nvarchar, CONVERT(nvarchar, [o].[OrderID] % 1)) <> '10')",
                Sql);
        }

        public override void Select_nested_collection()
        {
            base.Select_nested_collection();

            Assert.StartsWith(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
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

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
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

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]

",
                Sql);
        }

        public override void Where_subquery_on_bool()
        {
            base.Where_subquery_on_bool();

            Assert.Equal(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE N'Chai' IN (
    SELECT [p2].[ProductName]
    FROM [Products] AS [p2]
)",
                Sql);
        }

        public override void Where_subquery_on_collection()
        {
            base.Where_subquery_on_collection();

            Assert.Equal(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE 5 IN (
    SELECT [o].[Quantity]
    FROM [Order Details] AS [o]
    WHERE [o].[ProductID] = [p].[ProductID]
)",
                Sql);
        }

        public override void Select_many_cross_join_same_collection()
        {
            base.Select_many_cross_join_same_collection();

            Assert.Equal(
                @"SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c]
CROSS JOIN [Customers] AS [c0]",
                Sql);
        }

        public override void Join_same_collection_multiple()
        {
            base.Join_same_collection_multiple();

            Assert.Equal(
                @"SELECT [c3].[CustomerID], [c3].[Address], [c3].[City], [c3].[CompanyName], [c3].[ContactName], [c3].[ContactTitle], [c3].[Country], [c3].[Fax], [c3].[Phone], [c3].[PostalCode], [c3].[Region]
FROM [Customers] AS [o]
INNER JOIN [Customers] AS [c2] ON [o].[CustomerID] = [c2].[CustomerID]
INNER JOIN [Customers] AS [c3] ON [o].[CustomerID] = [c3].[CustomerID]",
                Sql);
        }

        public override void Join_same_collection_force_alias_uniquefication()
        {
            base.Join_same_collection_force_alias_uniquefication();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[CustomerID] = [o0].[CustomerID]",
                Sql);
        }

        public override void Where_chain()
        {
            base.Where_chain();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'QUICK') AND ([o].[OrderDate] > '1998-01-01T00:00:00.000')",
                Sql);
        }

        public override void OfType_Select()
        {
            base.OfType_Select();

            Assert.Equal(
                @"SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
) AS [t]
LEFT JOIN [Customers] AS [o.Customer] ON [t].[CustomerID] = [o.Customer].[CustomerID]
ORDER BY [t].[OrderID], [t].[CustomerID]",
                Sql);
        }

        public override void OfType_Select_OfType_Select()
        {
            base.OfType_Select_OfType_Select();

            Assert.Equal(
                @"SELECT [t1].[OrderID], [t1].[CustomerID], [t1].[EmployeeID], [t1].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM (
    SELECT [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate]
    FROM (
        SELECT [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate]
        FROM [Orders] AS [o2]
    ) AS [t0]
) AS [t1]
LEFT JOIN [Customers] AS [o.Customer] ON [t1].[CustomerID] = [o.Customer].[CustomerID]
ORDER BY [t1].[OrderID], [t1].[CustomerID]",
                Sql);
        }

        public override void OrderBy_null_coalesce_operator()
        {
            base.OrderBy_null_coalesce_operator();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY COALESCE([c].[Region], N'ZZ')",
                Sql);
        }

        public override void Select_null_coalesce_operator()
        {
            base.Select_null_coalesce_operator();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[CompanyName], COALESCE([c].[Region], N'ZZ') AS [Coalesce]
FROM [Customers] AS [c]
ORDER BY [Coalesce]",
                Sql);
        }

        public override void OrderBy_conditional_operator()
        {
            base.OrderBy_conditional_operator();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY CASE
    WHEN [c].[Region] IS NULL
    THEN N'ZZ' ELSE [c].[Region]
END",
                Sql);
        }

        public override void Contains_with_subquery()
        {
            base.Contains_with_subquery();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
)",
                Sql);
        }

        public override void Contains_with_local_array_closure()
        {
            base.Contains_with_local_array_closure();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI')

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE')",
                Sql);
        }

        public override void Contains_with_subquery_and_local_array_closure()
        {
            base.Contains_with_subquery_and_local_array_closure();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c1]
    WHERE [c1].[City] IN (N'London', N'Buenos Aires') AND ([c1].[CustomerID] = [c].[CustomerID]))

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c1]
    WHERE [c1].[City] IN (N'London') AND ([c1].[CustomerID] = [c].[CustomerID]))",
                Sql);
        }

        public override void Contains_with_local_array_inline()
        {
            base.Contains_with_local_array_inline();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI')",
                Sql);
        }

        public override void Contains_with_local_list_closure()
        {
            base.Contains_with_local_list_closure();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI')",
                Sql);
        }

        public override void Contains_with_local_list_inline()
        {
            base.Contains_with_local_list_inline();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI')",
                Sql);
        }

        public override void Contains_with_local_list_inline_closure_mix()
        {
            base.Contains_with_local_list_inline_closure_mix();

            Assert.Equal(
                @"@__id_0: ALFKI

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', @__id_0)

@__id_0: ANATR

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', @__id_0)",
                Sql);
        }

        public override void Contains_with_local_collection_false()
        {
            base.Contains_with_local_collection_false();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] NOT IN (N'ABCDE', N'ALFKI')",
                Sql);
        }

        public override void Contains_with_local_collection_complex_predicate_and()
        {
            base.Contains_with_local_collection_complex_predicate_and();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ALFKI', N'ABCDE') AND [c].[CustomerID] IN (N'ABCDE', N'ALFKI')",
                Sql);
        }

        public override void Contains_with_local_collection_complex_predicate_or()
        {
            base.Contains_with_local_collection_complex_predicate_or();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI', N'ALFKI', N'ABCDE')",
                Sql);
        }

        public override void Contains_with_local_collection_complex_predicate_not_matching_ins1()
        {
            base.Contains_with_local_collection_complex_predicate_not_matching_ins1();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ALFKI', N'ABCDE') OR [c].[CustomerID] NOT IN (N'ABCDE', N'ALFKI')",
                Sql);
        }

        public override void Contains_with_local_collection_complex_predicate_not_matching_ins2()
        {
            base.Contains_with_local_collection_complex_predicate_not_matching_ins2();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI') AND [c].[CustomerID] NOT IN (N'ALFKI', N'ABCDE')",
                Sql);
        }

        public override void Contains_with_local_collection_sql_injection()
        {
            base.Contains_with_local_collection_sql_injection();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ALFKI', N'ABC'')); GO; DROP TABLE Orders; GO; --', N'ALFKI', N'ABCDE')",
                Sql);
        }

        public override void Contains_with_local_collection_empty_closure()
        {
            base.Contains_with_local_collection_empty_closure();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 1 = 0",
                Sql);
        }

        public override void Contains_with_local_collection_empty_inline()
        {
            base.Contains_with_local_collection_empty_inline();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 1 = 1",
                Sql);
        }

        public override void Contains_top_level()
        {
            base.Contains_top_level();

            Assert.Equal(
                @"@__p_0: ALFKI

SELECT CASE
    WHEN @__p_0 IN (
        SELECT [c].[CustomerID]
        FROM [Customers] AS [c]
    )
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void Substring_with_constant()
        {
            base.Substring_with_constant();

            Assert.Equal(
                @"SELECT TOP(1) SUBSTRING([c].[ContactName], 2, 3)
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Substring_with_closure()
        {
            base.Substring_with_closure();

            Assert.Equal(
                @"@__start_0: 2

SELECT TOP(1) SUBSTRING([c].[ContactName], @__start_0 + 1, 3)
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Substring_with_client_eval()
        {
            base.Substring_with_client_eval();

            Assert.Equal(
                @"SELECT TOP(1) [c].[ContactName]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Projection_null_coalesce_operator()
        {
            base.Projection_null_coalesce_operator();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[CompanyName], COALESCE([c].[Region], N'ZZ') AS [Coalesce]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Filter_coalesce_operator()
        {
            base.Filter_coalesce_operator();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE COALESCE([c].[CompanyName], [c].[ContactName]) = N'The Big Cheese'",
                Sql);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Take_skip_null_coalesce_operator()
        {
            base.Take_skip_null_coalesce_operator();

            Assert.Equal(@"@__p_0: 10
@__p_1: 5

SELECT DISTINCT [t0].*
FROM (
    SELECT [t].*
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        ORDER BY COALESCE([c].[Region], N'ZZ')
    ) AS [t]
    ORDER BY COALESCE([t].[Region], N'ZZ')
    OFFSET @__p_1 ROWS
) AS [t0]",
                Sql);
        }

        public override void Select_take_null_coalesce_operator()
        {
            base.Select_take_null_coalesce_operator();

            Assert.Equal(@"@__p_0: 5

SELECT TOP(@__p_0) [c].[CustomerID], [c].[CompanyName], COALESCE([c].[Region], N'ZZ') AS [Coalesce]
FROM [Customers] AS [c]
ORDER BY [Coalesce]",
                Sql);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Select_take_skip_null_coalesce_operator()
        {
            base.Select_take_skip_null_coalesce_operator();

            Assert.Equal(
                @"@__p_0: 10
@__p_1: 5

SELECT [t].*
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[CompanyName], COALESCE([c].[Region], N'ZZ') AS [Coalesce]
    FROM [Customers] AS [c]
    ORDER BY [Coalesce]
) AS [t]
ORDER BY [Coalesce]
OFFSET @__p_1 ROWS",
                Sql);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Select_take_skip_null_coalesce_operator2()
        {
            base.Select_take_skip_null_coalesce_operator2();

            Assert.Equal(
                @"@__p_0: 10
@__p_1: 5

SELECT [t].*
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[CompanyName], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY COALESCE([c].[Region], N'ZZ')
) AS [t]
ORDER BY COALESCE([t].[Region], N'ZZ')
OFFSET @__p_1 ROWS",
                Sql);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Select_take_skip_null_coalesce_operator3()
        {
            base.Select_take_skip_null_coalesce_operator3();

            Assert.Equal(
                @"@__p_0: 10
@__p_1: 5

SELECT [t].*
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY COALESCE([c].[Region], N'ZZ')
) AS [t]
ORDER BY COALESCE([t].[Region], N'ZZ')
OFFSET @__p_1 ROWS",
                Sql);
        }

        public override void Selected_column_can_coalesce()
        {
            base.Selected_column_can_coalesce();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY COALESCE([c].[Region], N'ZZ')",
                Sql);
        }

        public override void Does_not_change_ordering_of_projection_with_complex_projections()
        {
            base.Does_not_change_ordering_of_projection_with_complex_projections();

            Assert.StartsWith(
                @"SELECT [e].[CustomerID], (
    SELECT COUNT(*)
    FROM [Orders] AS [o1]
    WHERE [e].[CustomerID] = [o1].[CustomerID]
)
FROM [Customers] AS [e]
WHERE [e].[ContactTitle] = N'Owner'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]",
                Sql);
        }

        private const string FileLineEnding = @"
";

        private static string Sql => TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);
    }
}
