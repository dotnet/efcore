// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    public class QuerySqlServerTest : QueryTestBase<NorthwindQuerySqlServerFixture>
    {
        public QuerySqlServerTest(NorthwindQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void Lifting_when_subquery_nested_order_by_anonymous()
        {
            base.Lifting_when_subquery_nested_order_by_anonymous();

            AssertSql(
                @"@__p_0: 2

SELECT [c1_Orders].[OrderID], [c1_Orders].[CustomerID], [c1_Orders].[EmployeeID], [c1_Orders].[OrderDate], [t0].[CustomerID]
FROM [Orders] AS [c1_Orders]
INNER JOIN (
    SELECT DISTINCT [t].[CustomerID]
    FROM (
        SELECT TOP(@__p_0) [c].*
        FROM [Customers] AS [c]
        ORDER BY [c].[CustomerID]
    ) AS [t]
    CROSS JOIN [Customers] AS [c2]
) AS [t0] ON [c1_Orders].[CustomerID] = [t0].[CustomerID]");
        }

        public override void Lifting_when_subquery_nested_order_by_simple()
        {
            base.Lifting_when_subquery_nested_order_by_simple();

            // TODO: Avoid unnecessary pushdown of subquery. See Issue#8094
            AssertContainsSql(
                @"@__p_0: 2

SELECT [t0].[CustomerID]
FROM (
    SELECT DISTINCT [t].[CustomerID]
    FROM (
        SELECT TOP(@__p_0) [c].*
        FROM [Customers] AS [c]
        ORDER BY [c].[CustomerID]
    ) AS [t]
    CROSS JOIN [Customers] AS [c2]
) AS [t0]",
                //
                @"SELECT [c1_Orders].[OrderID], [c1_Orders].[CustomerID], [c1_Orders].[EmployeeID], [c1_Orders].[OrderDate]
FROM [Orders] AS [c1_Orders]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CoreCLR, SkipReason = "Failing after netcoreapp2.0 upgrade")]
        public virtual void Cache_key_contexts_are_detached()
        {
            MakeGarbage(CreateContext(), out var wr);

            GC.Collect();

            Assert.False(wr.IsAlive);
        }

        private static void MakeGarbage(NorthwindContext context, out WeakReference wr)
        {
            wr = new WeakReference(context);

            using (context)
            {
                var orderDetails = context.OrderDetails;

                Func<NorthwindContext, Customer> query
                    = param
                        => (from c in context.Customers
                            from o in context.Set<Order>()
                            from od in orderDetails
                            from e1 in param.Employees
                            from e2 in param.Set<Order>()
                            select c).First();

                query(context);

                Assert.True(wr.IsAlive);
            }
        }

        public override void Project_to_object_array()
        {
            base.Project_to_object_array();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = 1");
        }

        public override void Project_to_int_array()
        {
            base.Project_to_int_array();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[ReportsTo]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = 1");
        }

        public override void Local_array()
        {
            base.Local_array();

            AssertSql(
                @"@__get_Item_0: ALFKI (Size = 450)

SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__get_Item_0");
        }

        public override void Entity_equality_self()
        {
            base.Entity_equality_self();

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = [c].[CustomerID]");
        }

        public override void Entity_equality_local()
        {
            base.Entity_equality_local();

            AssertSql(
                @"@__local_0_CustomerID: ANATR (Nullable = false) (Size = 450)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__local_0_CustomerID");
        }

        public override void Entity_equality_local_inline()
        {
            base.Entity_equality_local_inline();

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ANATR'");
        }

        public override void Entity_equality_null()
        {
            base.Entity_equality_null();

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IS NULL");
        }

        public override void Entity_equality_not_null()
        {
            base.Entity_equality_not_null();

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IS NOT NULL");
        }

        public override void Queryable_reprojection()
        {
            base.Queryable_reprojection();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Default_if_empty_top_level()
        {
            base.Default_if_empty_top_level();

            AssertSql(
                @"SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title]
FROM (
    SELECT NULL AS [empty]
) AS [empty]
LEFT JOIN (
    SELECT [c].[EmployeeID], [c].[City], [c].[Country], [c].[FirstName], [c].[ReportsTo], [c].[Title]
    FROM [Employees] AS [c]
    WHERE [c].[EmployeeID] = -1
) AS [t] ON 1 = 1");
        }

        public override void Default_if_empty_top_level_positive()
        {
            base.Default_if_empty_top_level_positive();

            AssertSql(
                @"SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title]
FROM (
    SELECT NULL AS [empty]
) AS [empty]
LEFT JOIN (
    SELECT [c].[EmployeeID], [c].[City], [c].[Country], [c].[FirstName], [c].[ReportsTo], [c].[Title]
    FROM [Employees] AS [c]
    WHERE [c].[EmployeeID] > 0
) AS [t] ON 1 = 1");
        }

        public override void Default_if_empty_top_level_arg()
        {
            base.Default_if_empty_top_level_arg();

            AssertSql(
                @"SELECT [c].[EmployeeID], [c].[City], [c].[Country], [c].[FirstName], [c].[ReportsTo], [c].[Title]
FROM [Employees] AS [c]
WHERE [c].[EmployeeID] = -1");
        }

        public override void Default_if_empty_top_level_projection()
        {
            base.Default_if_empty_top_level_projection();

            AssertSql(
                @"SELECT [t].[EmployeeID]
FROM (
    SELECT NULL AS [empty]
) AS [empty]
LEFT JOIN (
    SELECT [e].[EmployeeID]
    FROM [Employees] AS [e]
    WHERE [e].[EmployeeID] = -1
) AS [t] ON 1 = 1");
        }

        public override void Where_query_composition()
        {
            base.Where_query_composition();

            AssertSql(
                @"SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
FROM [Employees] AS [e1]
WHERE [e1].[FirstName] = (
    SELECT TOP(1) [e].[FirstName]
    FROM [Employees] AS [e]
    ORDER BY [e].[EmployeeID]
)");
        }

        public override void Where_query_composition_is_null()
        {
            base.Where_query_composition_is_null();

            AssertSql(
                @"@__p_0: 3

SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
) AS [t]",
                //
                @"@_outer_ReportsTo: 2 (Nullable = true)

SELECT TOP(2) [e2].[EmployeeID], [e2].[City], [e2].[Country], [e2].[FirstName], [e2].[ReportsTo], [e2].[Title]
FROM [Employees] AS [e2]
WHERE [e2].[EmployeeID] = @_outer_ReportsTo",
                //
                @"SELECT TOP(2) [e2].[EmployeeID], [e2].[City], [e2].[Country], [e2].[FirstName], [e2].[ReportsTo], [e2].[Title]
FROM [Employees] AS [e2]
WHERE [e2].[EmployeeID] IS NULL",
                //
                @"@_outer_ReportsTo: 2 (Nullable = true)

SELECT TOP(2) [e2].[EmployeeID], [e2].[City], [e2].[Country], [e2].[FirstName], [e2].[ReportsTo], [e2].[Title]
FROM [Employees] AS [e2]
WHERE [e2].[EmployeeID] = @_outer_ReportsTo");
        }

        public override void Where_query_composition_is_not_null()
        {
            base.Where_query_composition_is_null();

            AssertSql(
                @"@__p_0: 3

SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
) AS [t]",
                //
                @"@_outer_ReportsTo: 2 (Nullable = true)

SELECT TOP(2) [e2].[EmployeeID], [e2].[City], [e2].[Country], [e2].[FirstName], [e2].[ReportsTo], [e2].[Title]
FROM [Employees] AS [e2]
WHERE [e2].[EmployeeID] = @_outer_ReportsTo",
                //
                @"SELECT TOP(2) [e2].[EmployeeID], [e2].[City], [e2].[Country], [e2].[FirstName], [e2].[ReportsTo], [e2].[Title]
FROM [Employees] AS [e2]
WHERE [e2].[EmployeeID] IS NULL",
                //
                @"@_outer_ReportsTo: 2 (Nullable = true)

SELECT TOP(2) [e2].[EmployeeID], [e2].[City], [e2].[Country], [e2].[FirstName], [e2].[ReportsTo], [e2].[Title]
FROM [Employees] AS [e2]
WHERE [e2].[EmployeeID] = @_outer_ReportsTo");
        }

        public override void Where_query_composition_entity_equality_one_element_SingleOrDefault()
        {
            base.Where_query_composition_entity_equality_one_element_SingleOrDefault();

            AssertSql(
                @"@__p_0: 3

SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
) AS [t]",
                //
                @"@_outer_ReportsTo: 2 (Nullable = true)

SELECT TOP(2) [e20].[EmployeeID]
FROM [Employees] AS [e20]
WHERE [e20].[EmployeeID] = @_outer_ReportsTo",
                //
                @"SELECT TOP(2) [e20].[EmployeeID]
FROM [Employees] AS [e20]
WHERE [e20].[EmployeeID] IS NULL",
                //
                @"@_outer_ReportsTo: 2 (Nullable = true)

SELECT TOP(2) [e20].[EmployeeID]
FROM [Employees] AS [e20]
WHERE [e20].[EmployeeID] = @_outer_ReportsTo");
        }

        public override void Where_query_composition_entity_equality_one_element_FirstOrDefault()
        {
            base.Where_query_composition_entity_equality_one_element_FirstOrDefault();

            AssertSql(
                @"SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
FROM [Employees] AS [e1]
WHERE (
    SELECT TOP(1) [e2].[EmployeeID]
    FROM [Employees] AS [e2]
    WHERE [e2].[EmployeeID] = [e1].[ReportsTo]
) = 0");
        }

        public override void Where_query_composition_entity_equality_no_elements_SingleOrDefault()
        {
            base.Where_query_composition_entity_equality_no_elements_SingleOrDefault();

            AssertSql(
                @"@__p_0: 3

SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
) AS [t]",
                //
                @"SELECT TOP(2) [e20].[EmployeeID]
FROM [Employees] AS [e20]
WHERE [e20].[EmployeeID] = 42",
                //
                @"SELECT TOP(2) [e20].[EmployeeID]
FROM [Employees] AS [e20]
WHERE [e20].[EmployeeID] = 42",
                //
                @"SELECT TOP(2) [e20].[EmployeeID]
FROM [Employees] AS [e20]
WHERE [e20].[EmployeeID] = 42");
        }

        public override void Where_query_composition_entity_equality_no_elements_FirstOrDefault()
        {
            base.Where_query_composition_entity_equality_no_elements_FirstOrDefault();

            AssertSql(
                @"SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
FROM [Employees] AS [e1]
WHERE (
    SELECT TOP(1) [e2].[EmployeeID]
    FROM [Employees] AS [e2]
    WHERE [e2].[EmployeeID] = 42
) = 0");
        }

        public override void Where_query_composition_entity_equality_multiple_elements_FirstOrDefault()
        {
            base.Where_query_composition_entity_equality_multiple_elements_FirstOrDefault();

            AssertSql(
                @"SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
FROM [Employees] AS [e1]
WHERE (
    SELECT TOP(1) [e2].[EmployeeID]
    FROM [Employees] AS [e2]
    WHERE ([e2].[EmployeeID] <> [e1].[ReportsTo]) OR [e1].[ReportsTo] IS NULL
) = 0");
        }

        public override void Where_query_composition2()
        {
            base.Where_query_composition2();

            AssertSql(
                @"@__p_0: 3

SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
) AS [t]",
                //
                @"SELECT TOP(1) [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title]
FROM [Employees] AS [e0]
ORDER BY [e0].[EmployeeID]",
                //
                @"SELECT TOP(1) [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title]
FROM [Employees] AS [e0]
ORDER BY [e0].[EmployeeID]",
                //
                @"SELECT TOP(1) [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title]
FROM [Employees] AS [e0]
ORDER BY [e0].[EmployeeID]");
        }

        public override void Where_query_composition2_FirstOrDefault()
        {
            base.Where_query_composition2_FirstOrDefault();

            AssertSql(
                @"@__p_0: 3

SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
) AS [t]
WHERE [t].[FirstName] = (
    SELECT TOP(1) [e0].[FirstName]
    FROM [Employees] AS [e0]
    ORDER BY [e0].[EmployeeID]
)");
        }

        public override void Where_query_composition2_FirstOrDefault_with_anonymous()
        {
            base.Where_query_composition2_FirstOrDefault_with_anonymous();

            AssertSql(
                @"@__p_0: 3

SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
) AS [t]",
                //
                @"SELECT TOP(1) [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title]
FROM [Employees] AS [e0]
ORDER BY [e0].[EmployeeID]",
                //
                @"SELECT TOP(1) [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title]
FROM [Employees] AS [e0]
ORDER BY [e0].[EmployeeID]",
                //
                @"SELECT TOP(1) [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title]
FROM [Employees] AS [e0]
ORDER BY [e0].[EmployeeID]");
        }

        public override void Where_shadow_subquery_FirstOrDefault()
        {
            base.Where_shadow_subquery_FirstOrDefault();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[Title] = (
    SELECT TOP(1) [e2].[Title]
    FROM [Employees] AS [e2]
    ORDER BY [e2].[Title]
)");
        }

        public override void Select_Subquery_Single()
        {
            base.Select_Subquery_Single();

            AssertSql(
                @"@__p_0: 2

SELECT TOP(@__p_0) [od].[OrderID]
FROM [Order Details] AS [od]
ORDER BY [od].[ProductID], [od].[OrderID]",
                //
                @"@_outer_OrderID: 10285

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_OrderID = [o].[OrderID]
ORDER BY [o].[OrderID]",
                //
                @"@_outer_OrderID: 10294

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_OrderID = [o].[OrderID]
ORDER BY [o].[OrderID]");
        }

        public override void Select_Where_Subquery_Deep_Single()
        {
            base.Select_Where_Subquery_Deep_Single();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE [od].[OrderID] = 10344",
                //
                @"@_outer_OrderID: 10344

SELECT TOP(2) [o0].[CustomerID]
FROM [Orders] AS [o0]
WHERE @_outer_OrderID = [o0].[OrderID]",
                //
                @"@_outer_CustomerID1: WHITC (Size = 450)

SELECT TOP(2) [c2].[City]
FROM [Customers] AS [c2]
WHERE @_outer_CustomerID1 = [c2].[CustomerID]",
                //
                @"@_outer_OrderID: 10344

SELECT TOP(2) [o0].[CustomerID]
FROM [Orders] AS [o0]
WHERE @_outer_OrderID = [o0].[OrderID]",
                //
                @"@_outer_CustomerID1: WHITC (Size = 450)

SELECT TOP(2) [c2].[City]
FROM [Customers] AS [c2]
WHERE @_outer_CustomerID1 = [c2].[CustomerID]");
        }

        public override void Select_Where_Subquery_Deep_First()
        {
            base.Select_Where_Subquery_Deep_First();

            AssertSql(
                @"@__p_0: 2

SELECT TOP(@__p_0) [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE (
    SELECT TOP(1) (
        SELECT TOP(1) [c].[City]
        FROM [Customers] AS [c]
        WHERE [o].[CustomerID] = [c].[CustomerID]
    )
    FROM [Orders] AS [o]
    WHERE [od].[OrderID] = [o].[OrderID]
) = N'Seattle'");
        }

        public override void Select_Where_Subquery_Equality()
        {
            base.Select_Where_Subquery_Equality();

            AssertSql(
                @"@__p_0: 1

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
) AS [t]
ORDER BY [t].[OrderID]",
                //
                @"SELECT [t1].[OrderID]
FROM (
    SELECT TOP(2) [od0].*
    FROM [Order Details] AS [od0]
    ORDER BY [od0].[OrderID]
) AS [t1]",
                //
                @"@_outer_CustomerID2: VINET (Size = 450)

SELECT TOP(1) [c3].[Country]
FROM [Customers] AS [c3]
WHERE [c3].[CustomerID] = @_outer_CustomerID2
ORDER BY [c3].[CustomerID]",
                //
                @"@_outer_OrderID1: 10248

SELECT TOP(1) [c4].[Country]
FROM [Orders] AS [o20]
INNER JOIN [Customers] AS [c4] ON [o20].[CustomerID] = [c4].[CustomerID]
WHERE [o20].[OrderID] = @_outer_OrderID1
ORDER BY [o20].[OrderID], [c4].[CustomerID]",
                //
                @"@_outer_CustomerID2: VINET (Size = 450)

SELECT TOP(1) [c3].[Country]
FROM [Customers] AS [c3]
WHERE [c3].[CustomerID] = @_outer_CustomerID2
ORDER BY [c3].[CustomerID]",
                //
                @"@_outer_OrderID1: 10248

SELECT TOP(1) [c4].[Country]
FROM [Orders] AS [o20]
INNER JOIN [Customers] AS [c4] ON [o20].[CustomerID] = [c4].[CustomerID]
WHERE [o20].[OrderID] = @_outer_OrderID1
ORDER BY [o20].[OrderID], [c4].[CustomerID]");
        }

        public override void Where_subquery_anon()
        {
            base.Where_subquery_anon();

            AssertSql(
                @"@__p_0: 3

SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title], [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
) AS [t]
CROSS JOIN (
    SELECT TOP(5) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
) AS [t0]");
        }

        public override void Where_subquery_anon_nested()
        {
            base.Where_subquery_anon_nested();

            AssertSql(
                @"@__p_0: 3

SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title], [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate], [t1].[CustomerID], [t1].[Address], [t1].[City], [t1].[CompanyName], [t1].[ContactName], [t1].[ContactTitle], [t1].[Country], [t1].[Fax], [t1].[Phone], [t1].[PostalCode], [t1].[Region]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
) AS [t]
CROSS JOIN (
    SELECT TOP(5) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
) AS [t0]
CROSS JOIN (
    SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
) AS [t1]
WHERE [t].[City] = N'London'");
        }

        public override void Where_subquery_correlated()
        {
            base.Where_subquery_correlated();

            AssertSql(
                @"SELECT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
FROM [Customers] AS [c1]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c2]
    WHERE [c1].[CustomerID] = [c2].[CustomerID])");
        }

        public override void Where_subquery_correlated_client_eval()
        {
            base.Where_subquery_correlated_client_eval();

            AssertSql(
                @"@__p_0: 5

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
) AS [t]
ORDER BY [t].[CustomerID]",
                //
                @"@_outer_CustomerID: ALFKI (Size = 450)

SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
FROM [Customers] AS [c2]
WHERE @_outer_CustomerID = [c2].[CustomerID]",
                //
                @"@_outer_CustomerID: ANATR (Size = 450)

SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
FROM [Customers] AS [c2]
WHERE @_outer_CustomerID = [c2].[CustomerID]",
                //
                @"@_outer_CustomerID: ANTON (Size = 450)

SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
FROM [Customers] AS [c2]
WHERE @_outer_CustomerID = [c2].[CustomerID]",
                //
                @"@_outer_CustomerID: AROUT (Size = 450)

SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
FROM [Customers] AS [c2]
WHERE @_outer_CustomerID = [c2].[CustomerID]",
                //
                @"@_outer_CustomerID: BERGS (Size = 450)

SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
FROM [Customers] AS [c2]
WHERE @_outer_CustomerID = [c2].[CustomerID]");
        }

        public override void OrderBy_SelectMany()
        {
            base.OrderBy_SelectMany();

            AssertSql(
                @"SELECT [c].[ContactName], [t].[OrderID]
FROM [Customers] AS [c]
CROSS JOIN (
    SELECT TOP(3) [o].*
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]
WHERE [c].[CustomerID] = [t].[CustomerID]
ORDER BY [c].[CustomerID]");
        }

        public override void Let_any_subquery_anonymous()
        {
            base.Let_any_subquery_anonymous();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')
ORDER BY [c].[CustomerID]",
                //
                @"@_outer_CustomerID: ALFKI (Size = 450)

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o0]
        WHERE [o0].[CustomerID] = @_outer_CustomerID)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                //
                @"@_outer_CustomerID: ANATR (Size = 450)

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o0]
        WHERE [o0].[CustomerID] = @_outer_CustomerID)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                //
                @"@_outer_CustomerID: ANTON (Size = 450)

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o0]
        WHERE [o0].[CustomerID] = @_outer_CustomerID)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                //
                @"@_outer_CustomerID: AROUT (Size = 450)

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o0]
        WHERE [o0].[CustomerID] = @_outer_CustomerID)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void GroupBy_anonymous()
        {
            base.GroupBy_anonymous();

            AssertSql(
                @"SELECT [c].[City], [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[City]");
        }

        public override void GroupBy_anonymous_with_where()
        {
            base.GroupBy_anonymous_with_where();

            AssertSql(
                @"SELECT [c].[City], [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[Country] IN (N'Argentina', N'Austria', N'Brazil', N'France', N'Germany', N'USA')
ORDER BY [c].[City]");
        }

        public override void GroupBy_nested_order_by_enumerable()
        {
            base.GroupBy_nested_order_by_enumerable();

            AssertSql(
                @"SELECT [c0].[Country], [c0].[CustomerID]
FROM [Customers] AS [c0]
ORDER BY [c0].[Country]");
        }

        public override void GroupBy_join_default_if_empty_anonymous()
        {
            base.GroupBy_join_default_if_empty_anonymous();

            AssertSql(
                @"SELECT [order0].[OrderID], [order0].[CustomerID], [order0].[EmployeeID], [order0].[OrderDate], [orderDetail0].[OrderID], [orderDetail0].[ProductID], [orderDetail0].[Discount], [orderDetail0].[Quantity], [orderDetail0].[UnitPrice]
FROM [Orders] AS [order0]
LEFT JOIN [Order Details] AS [orderDetail0] ON [order0].[OrderID] = [orderDetail0].[OrderID]
ORDER BY [order0].[OrderID]");
        }

        public override void Where_simple_closure()
        {
            base.Where_simple_closure();

            AssertSql(
                @"@__city_0: London (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_0");
        }

        public override void Where_indexer_closure()
        {
            base.Where_indexer_closure();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_simple_closure_constant()
        {
            base.Where_simple_closure_constant();

            AssertSql(
                @"@__predicate_0: True

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE @__predicate_0 = 1");
        }

        public override void Where_simple_closure_via_query_cache_nullable_type_reverse()
        {
            base.Where_simple_closure_via_query_cache_nullable_type_reverse();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL",
                //
                @"@__reportsTo_0: 5 (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__reportsTo_0",
                //
                @"@__reportsTo_0: 2 (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__reportsTo_0");
        }

        public override void Where_simple_closure_via_query_cache_nullable_type()
        {
            base.Where_simple_closure_via_query_cache_nullable_type();

            AssertSql(
                @"@__reportsTo_0: 2 (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__reportsTo_0",
                //
                @"@__reportsTo_0: 5 (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__reportsTo_0",
                //
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL");
        }

        public override void Where_new_instance_field_access_closure_via_query_cache()
        {
            base.Where_new_instance_field_access_closure_via_query_cache();

            AssertSql(
                @"@__InstanceFieldValue_0: London (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__InstanceFieldValue_0",
                //
                @"@__InstanceFieldValue_0: Seattle (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__InstanceFieldValue_0");
        }

        public override void Where_nested_property_access_closure_via_query_cache()
        {
            base.Where_nested_property_access_closure_via_query_cache();

            AssertSql(
                @"@__city_Nested_InstancePropertyValue_0: London (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_Nested_InstancePropertyValue_0",
                //
                @"@__city_Nested_InstancePropertyValue_0: Seattle (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_Nested_InstancePropertyValue_0");
        }

        public override void Where_nested_field_access_closure_via_query_cache()
        {
            base.Where_nested_field_access_closure_via_query_cache();

            AssertSql(
                @"@__city_Nested_InstanceFieldValue_0: London (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_Nested_InstanceFieldValue_0",
                //
                @"@__city_Nested_InstanceFieldValue_0: Seattle (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_Nested_InstanceFieldValue_0");
        }

        public override void Where_static_property_access_closure_via_query_cache()
        {
            base.Where_static_property_access_closure_via_query_cache();

            AssertSql(
                @"@__StaticPropertyValue_0: London (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__StaticPropertyValue_0",
                //
                @"@__StaticPropertyValue_0: Seattle (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__StaticPropertyValue_0");
        }

        public override void Where_property_access_closure_via_query_cache()
        {
            base.Where_property_access_closure_via_query_cache();

            AssertSql(
                @"@__city_InstancePropertyValue_0: London (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_InstancePropertyValue_0",
                //
                @"@__city_InstancePropertyValue_0: Seattle (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_InstancePropertyValue_0");
        }

        public override void Where_static_field_access_closure_via_query_cache()
        {
            base.Where_static_field_access_closure_via_query_cache();

            AssertSql(
                @"@__StaticFieldValue_0: London (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__StaticFieldValue_0",
                //
                @"@__StaticFieldValue_0: Seattle (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__StaticFieldValue_0");
        }

        public override void Where_field_access_closure_via_query_cache()
        {
            base.Where_field_access_closure_via_query_cache();

            AssertSql(
                @"@__city_InstanceFieldValue_0: London (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_InstanceFieldValue_0",
                //
                @"@__city_InstanceFieldValue_0: Seattle (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_InstanceFieldValue_0");
        }

        public override void Where_method_call_closure_via_query_cache()
        {
            base.Where_method_call_closure_via_query_cache();

            AssertSql(
                @"@__GetCity_0: London (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__GetCity_0",
                //
                @"@__GetCity_0: Seattle (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__GetCity_0");
        }

        public override void Where_method_call_nullable_type_reverse_closure_via_query_cache()
        {
            base.Where_method_call_nullable_type_reverse_closure_via_query_cache();

            AssertSql(
                @"@__city_NullableInt_0: 1 (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] > @__city_NullableInt_0",
                //
                @"@__city_NullableInt_0: 5 (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] > @__city_NullableInt_0");
        }

        public override void Where_method_call_nullable_type_closure_via_query_cache()
        {
            base.Where_method_call_nullable_type_closure_via_query_cache();

            AssertSql(
                @"@__city_Int_0: 2

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__city_Int_0",
                //
                @"@__city_Int_0: 5

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__city_Int_0");
        }

        public override void Where_simple_closure_via_query_cache()
        {
            base.Where_simple_closure_via_query_cache();

            AssertSql(
                @"@__city_0: London (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_0",
                //
                @"@__city_0: Seattle (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_0");
        }

        public override void Where_subquery_closure_via_query_cache()
        {
            base.Where_subquery_closure_via_query_cache();

            AssertSql(
                @"@__customerID_0: ALFKI (Size = 450)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE ([o].[CustomerID] = @__customerID_0) AND ([o].[CustomerID] = [c].[CustomerID]))",
                //
                @"@__customerID_0: ANATR (Size = 450)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE ([o].[CustomerID] = @__customerID_0) AND ([o].[CustomerID] = [c].[CustomerID]))");
        }

        public override void Count_with_predicate()
        {
            base.Count_with_predicate();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'");
        }

        public override void Where_OrderBy_Count()
        {
            base.Where_OrderBy_Count();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'");
        }

        public override void OrderBy_Where_Count()
        {
            base.OrderBy_Where_Count();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'");
        }

        public override void OrderBy_Count_with_predicate()
        {
            base.OrderBy_Count_with_predicate();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'");
        }

        public override void OrderBy_Where_Count_with_predicate()
        {
            base.OrderBy_Where_Count_with_predicate();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE ([o].[OrderID] > 10) AND (([o].[CustomerID] <> N'ALFKI') OR [o].[CustomerID] IS NULL)");
        }

        public override void Where_OrderBy_Count_client_eval()
        {
            base.Where_OrderBy_Count_client_eval();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override void Where_OrderBy_Count_client_eval_mixed()
        {
            base.Where_OrderBy_Count_client_eval_mixed();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[OrderID] > 10");
        }

        public override void OrderBy_Where_Count_client_eval()
        {
            base.OrderBy_Where_Count_client_eval();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override void OrderBy_Where_Count_client_eval_mixed()
        {
            base.OrderBy_Where_Count_client_eval_mixed();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override void OrderBy_Count_with_predicate_client_eval()
        {
            base.OrderBy_Count_with_predicate_client_eval();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override void OrderBy_Count_with_predicate_client_eval_mixed()
        {
            base.OrderBy_Count_with_predicate_client_eval_mixed();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override void OrderBy_Where_Count_with_predicate_client_eval()
        {
            base.OrderBy_Where_Count_with_predicate_client_eval();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override void OrderBy_Where_Count_with_predicate_client_eval_mixed()
        {
            base.OrderBy_Where_Count_with_predicate_client_eval_mixed();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] <> N'ALFKI') OR [o].[CustomerID] IS NULL");
        }

        public override void OrderBy_client_Take()
        {
            base.OrderBy_client_Take();

            AssertSql(
                @"@__p_1: 10

SELECT TOP(@__p_1) [o].[EmployeeID], [o].[City], [o].[Country], [o].[FirstName], [o].[ReportsTo], [o].[Title]
FROM [Employees] AS [o]
ORDER BY (SELECT 1)");
        }

        public override void OrderBy_arithmetic()
        {
            base.OrderBy_arithmetic();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
ORDER BY [e].[EmployeeID] - [e].[EmployeeID]");
        }

        public override void OrderBy_condition_comparison()
        {
            base.OrderBy_condition_comparison();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
ORDER BY CASE
    WHEN [p].[UnitsInStock] > 0
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END, [p].[ProductID]");
        }

        public override void OrderBy_ternary_conditions()
        {
            base.OrderBy_ternary_conditions();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
ORDER BY CASE
    WHEN (([p].[UnitsInStock] > 10) AND ([p].[ProductID] > 40)) OR (([p].[UnitsInStock] <= 10) AND ([p].[ProductID] <= 40))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END, [p].[ProductID]");
        }

        public override void OrderBy_any()
        {
            base.OrderBy_any();

            AssertSql(
                @"SELECT [p].[CustomerID], [p].[Address], [p].[City], [p].[CompanyName], [p].[ContactName], [p].[ContactTitle], [p].[Country], [p].[Fax], [p].[Phone], [p].[PostalCode], [p].[Region]
FROM [Customers] AS [p]
ORDER BY (
    SELECT CASE
        WHEN EXISTS (
            SELECT 1
            FROM [Orders] AS [o]
            WHERE ([o].[OrderID] > 11000) AND ([p].[CustomerID] = [o].[CustomerID]))
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END
), [p].[CustomerID]");
        }

        public override void Sum_with_no_arg()
        {
            base.Sum_with_no_arg();

            AssertSql(
                @"SELECT SUM([o].[OrderID])
FROM [Orders] AS [o]");
        }

        public override void Sum_with_arg()
        {
            base.Sum_with_arg();

            AssertSql(
                @"SELECT SUM([o].[OrderID])
FROM [Orders] AS [o]");
        }

        public override void Sum_with_arg_expression()
        {
            base.Sum_with_arg_expression();

            AssertSql(
                @"SELECT SUM([o].[OrderID] + [o].[OrderID])
FROM [Orders] AS [o]");
        }

        public override void Sum_with_binary_expression()
        {
            base.Sum_with_binary_expression();

            AssertSql(
                @"SELECT SUM([o].[OrderID] * 2)
FROM [Orders] AS [o]");
        }

        public override void Sum_with_division_on_decimal()
        {
            base.Sum_with_division_on_decimal();

            AssertSql(
                @"SELECT SUM([od].[Quantity] / 2.09)
FROM [Order Details] AS [od]");
        }

        public override void Sum_with_division_on_decimal_no_significant_digits()
        {
            base.Sum_with_division_on_decimal_no_significant_digits();

            AssertSql(
                @"SELECT SUM([od].[Quantity] / 2.0)
FROM [Order Details] AS [od]");
        }

        public override void Sum_with_coalesce()
        {
            base.Sum_with_coalesce();

            AssertSql(
                @"SELECT SUM(COALESCE([p].[UnitPrice], 0.0))
FROM [Products] AS [p]
WHERE [p].[ProductID] < 40");
        }

        public override void Sum_over_subquery_is_client_eval()
        {
            base.Sum_over_subquery_is_client_eval();

            AssertSql(
                @"SELECT (
    SELECT SUM([o].[OrderID])
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
)
FROM [Customers] AS [c]");
        }

        public override void Sum_on_float_column()
        {
            base.Sum_on_float_column();

            AssertSql(
                @"SELECT CAST(SUM([od].[Discount]) AS real)
FROM [Order Details] AS [od]
WHERE [od].[ProductID] = 1");
        }

        public override void Sum_on_float_column_in_subquery()
        {
            base.Sum_on_float_column_in_subquery();

            AssertSql(
                @"SELECT [o].[OrderID], (
    SELECT CAST(SUM([od].[Discount]) AS real)
    FROM [Order Details] AS [od]
    WHERE [o].[OrderID] = [od].[OrderID]
) AS [Sum]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300");
        }

        public override void Average_with_no_arg()
        {
            base.Average_with_no_arg();

            AssertSql(
                @"SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]");
        }

        public override void Average_with_binary_expression()
        {
            base.Average_with_binary_expression();

            AssertSql(
                @"SELECT AVG(CAST([o].[OrderID] * 2 AS float))
FROM [Orders] AS [o]");
        }

        public override void Average_with_arg()
        {
            base.Average_with_arg();

            AssertSql(
                @"SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]");
        }

        public override void Average_with_arg_expression()
        {
            base.Average_with_arg_expression();

            AssertSql(
                @"SELECT AVG(CAST([o].[OrderID] + [o].[OrderID] AS float))
FROM [Orders] AS [o]");
        }

        public override void Average_with_division_on_decimal()
        {
            base.Average_with_division_on_decimal();

            AssertSql(
                @"SELECT AVG(CAST([od].[Quantity] / 2.09 AS decimal(18, 2)))
FROM [Order Details] AS [od]");
        }

        public override void Average_with_division_on_decimal_no_significant_digits()
        {
            base.Average_with_division_on_decimal_no_significant_digits();

            AssertSql(
                @"SELECT AVG(CAST([od].[Quantity] / 2.0 AS decimal(18, 2)))
FROM [Order Details] AS [od]");
        }

        public override void Average_with_coalesce()
        {
            base.Average_with_coalesce();

            AssertSql(
                @"SELECT AVG(CAST(COALESCE([p].[UnitPrice], 0.0) AS decimal(18, 2)))
FROM [Products] AS [p]
WHERE [p].[ProductID] < 40");
        }

        public override void Average_over_subquery_is_client_eval()
        {
            base.Average_over_subquery_is_client_eval();

            AssertSql(
                @"SELECT (
    SELECT SUM([o].[OrderID])
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
)
FROM [Customers] AS [c]");
        }

        public override void Average_on_float_column()
        {
            base.Average_on_float_column();

            AssertSql(
                @"SELECT CAST(AVG(CAST([od].[Discount] AS real)) AS real)
FROM [Order Details] AS [od]
WHERE [od].[ProductID] = 1");
        }

        public override void Average_on_float_column_in_subquery()
        {
            base.Average_on_float_column_in_subquery();

            AssertSql(
                @"SELECT [o].[OrderID], (
    SELECT CAST(AVG(CAST([od].[Discount] AS real)) AS real)
    FROM [Order Details] AS [od]
    WHERE [o].[OrderID] = [od].[OrderID]
) AS [Sum]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300");
        }

        public override void Min_with_no_arg()
        {
            base.Min_with_no_arg();

            AssertSql(
                @"SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]");
        }

        public override void Min_with_arg()
        {
            base.Min_with_arg();

            AssertSql(
                @"SELECT MIN([o].[OrderID])
FROM [Orders] AS [o]");
        }

        public override void Min_with_coalesce()
        {
            base.Min_with_coalesce();

            AssertSql(
                @"SELECT MIN(COALESCE([p].[UnitPrice], 0.0))
FROM [Products] AS [p]
WHERE [p].[ProductID] < 40");
        }

        public override void Min_over_subquery_is_client_eval()
        {
            base.Min_over_subquery_is_client_eval();

            AssertSql(
                @"SELECT (
    SELECT SUM([o].[OrderID])
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
)
FROM [Customers] AS [c]");
        }

        public override void Max_with_no_arg()
        {
            base.Max_with_no_arg();

            AssertSql(
                @"SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]");
        }

        public override void Max_with_arg()
        {
            base.Max_with_arg();

            AssertSql(
                @"SELECT MAX([o].[OrderID])
FROM [Orders] AS [o]");
        }

        public override void Max_with_coalesce()
        {
            base.Max_with_coalesce();

            AssertSql(
                @"SELECT MAX(COALESCE([p].[UnitPrice], 0.0))
FROM [Products] AS [p]
WHERE [p].[ProductID] < 40");
        }

        public override void Max_over_subquery_is_client_eval()
        {
            base.Max_over_subquery_is_client_eval();

            AssertSql(
                @"SELECT (
    SELECT SUM([o].[OrderID])
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
)
FROM [Customers] AS [c]");
        }

        public override void Distinct_Count()
        {
            base.Distinct_Count();

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].*
    FROM [Customers] AS [c]
) AS [t]");
        }

        public override void Select_Select_Distinct_Count()
        {
            base.Select_Select_Distinct_Count();

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[City]
    FROM [Customers] AS [c]
) AS [t]");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Skip()
        {
            base.Skip();

            AssertSql(
                @"@__p_0: 5

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
OFFSET @__p_0 ROWS");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Skip_no_orderby()
        {
            base.Skip_no_orderby();

            AssertSql(
                @"@__p_0: 5

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY (SELECT 1)
OFFSET @__p_0 ROWS");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Skip_Take()
        {
            base.Skip_Take();

            AssertSql(
                @"@__p_0: 5
@__p_1: 10

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactName]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Join_Customers_Orders_Skip_Take()
        {
            base.Join_Customers_Orders_Skip_Take();

            AssertSql(
                @"@__p_0: 10
@__p_1: 5

SELECT [c].[ContactName], [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [o].[OrderID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Join_Customers_Orders_Projection_With_String_Concat_Skip_Take()
        {
            base.Join_Customers_Orders_Projection_With_String_Concat_Skip_Take();

            AssertSql(
                @"@__p_0: 10
@__p_1: 5

SELECT ([c].[ContactName] + N' ') + [c].[ContactTitle] AS [Contact], [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [o].[OrderID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Join_Customers_Orders_Orders_Skip_Take_Same_Properties()
        {
            base.Join_Customers_Orders_Orders_Skip_Take_Same_Properties();

            AssertSql(
                @"@__p_0: 10
@__p_1: 5

SELECT [o].[OrderID], [ca].[CustomerID] AS [CustomerIDA], [cb].[CustomerID] AS [CustomerIDB], [ca].[ContactName] AS [ContactNameA], [cb].[ContactName] AS [ContactNameB]
FROM [Orders] AS [o]
INNER JOIN [Customers] AS [ca] ON [o].[CustomerID] = [ca].[CustomerID]
INNER JOIN [Customers] AS [cb] ON [o].[CustomerID] = [cb].[CustomerID]
ORDER BY [o].[OrderID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Take_Skip()
        {
            base.Take_Skip();

            AssertSql(
                @"@__p_0: 10
@__p_1: 5

SELECT [t].*
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactName]
) AS [t]
ORDER BY [t].[ContactName]
OFFSET @__p_1 ROWS");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Take_Skip_Distinct()
        {
            base.Take_Skip_Distinct();

            AssertSql(
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
) AS [t0]");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Take_Skip_Distinct_Caching()
        {
            base.Take_Skip_Distinct_Caching();

            AssertSql(
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
                //
                @"@__p_0: 15
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
) AS [t0]");
        }

        public void Skip_when_no_OrderBy()
        {
            Assert.Throws<Exception>(() => AssertQuery<Customer>(cs => cs.Skip(5).Take(10)));
        }

        public override void Take_Distinct_Count()
        {
            base.Take_Distinct_Count();

            AssertSql(
                @"@__p_0: 5

SELECT COUNT(*)
FROM (
    SELECT DISTINCT [t].*
    FROM (
        SELECT TOP(@__p_0) [o].*
        FROM [Orders] AS [o]
    ) AS [t]
) AS [t0]");
        }

        public override void Take_Where_Distinct_Count()
        {
            base.Take_Where_Distinct_Count();

            AssertSql(
                @"@__p_0: 5

SELECT COUNT(*)
FROM (
    SELECT DISTINCT [t].*
    FROM (
        SELECT TOP(@__p_0) [o].*
        FROM [Orders] AS [o]
        WHERE [o].[CustomerID] = N'FRANK'
    ) AS [t]
) AS [t0]");
        }

        public override void Null_conditional_simple()
        {
            base.Null_conditional_simple();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override void Null_conditional_deep()
        {
            base.Null_conditional_deep();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE CAST(LEN([c].[CustomerID]) AS int) = 5");
        }

        public override void Queryable_simple()
        {
            base.Queryable_simple();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Queryable_simple_anonymous()
        {
            base.Queryable_simple_anonymous();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Queryable_nested_simple()
        {
            base.Queryable_nested_simple();

            AssertSql(
                @"SELECT [c3].[CustomerID], [c3].[Address], [c3].[City], [c3].[CompanyName], [c3].[ContactName], [c3].[ContactTitle], [c3].[Country], [c3].[Fax], [c3].[Phone], [c3].[PostalCode], [c3].[Region]
FROM [Customers] AS [c3]");
        }

        public override void Queryable_simple_anonymous_projection_subquery()
        {
            base.Queryable_simple_anonymous_projection_subquery();

            AssertSql(
                @"@__p_0: 91

SELECT TOP(@__p_0) [c].[City]
FROM [Customers] AS [c]");
        }

        public override void Queryable_simple_anonymous_subquery()
        {
            base.Queryable_simple_anonymous_subquery();

            AssertSql(
                @"@__p_0: 91

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Take_simple()
        {
            base.Take_simple();

            AssertSql(
                @"@__p_0: 10

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void Take_simple_parameterized()
        {
            base.Take_simple_parameterized();

            AssertSql(
                @"@__p_0: 10

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void Take_simple_projection()
        {
            base.Take_simple_projection();

            AssertSql(
                @"@__p_0: 10

SELECT TOP(@__p_0) [c].[City]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void Take_subquery_projection()
        {
            base.Take_subquery_projection();

            AssertSql(
                @"@__p_0: 2

SELECT TOP(@__p_0) [c].[City]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void OrderBy_Take_Count()
        {
            base.OrderBy_Take_Count();

            AssertSql(
                @"@__p_0: 5

SELECT COUNT(*)
FROM (
    SELECT TOP(@__p_0) [o].*
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]");
        }

        public override void Take_OrderBy_Count()
        {
            base.Take_OrderBy_Count();

            AssertSql(
                @"@__p_0: 5

SELECT COUNT(*)
FROM (
    SELECT TOP(@__p_0) [o].*
    FROM [Orders] AS [o]
) AS [t]");
        }

        public override void Any_simple()
        {
            base.Any_simple();

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c])
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void Any_predicate()
        {
            base.Any_predicate();

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        WHERE [c].[ContactName] LIKE N'A' + N'%' AND (LEFT([c].[ContactName], LEN(N'A')) = N'A'))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void Any_nested_negated()
        {
            base.Any_nested_negated();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] LIKE N'A' + N'%' AND (LEFT([o].[CustomerID], LEN(N'A')) = N'A'))");
        }

        public override void Any_nested_negated2()
        {
            base.Any_nested_negated2();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] <> N'London') OR [c].[City] IS NULL) AND NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] LIKE N'A' + N'%' AND (LEFT([o].[CustomerID], LEN(N'A')) = N'A'))");
        }

        public override void Any_nested_negated3()
        {
            base.Any_nested_negated3();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] LIKE N'A' + N'%' AND (LEFT([o].[CustomerID], LEN(N'A')) = N'A')) AND (([c].[City] <> N'London') OR [c].[City] IS NULL)");
        }

        public override void Any_nested()
        {
            base.Any_nested();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] LIKE N'A' + N'%' AND (LEFT([o].[CustomerID], LEN(N'A')) = N'A'))");
        }

        public override void Any_nested2()
        {
            base.Any_nested2();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] <> N'London') OR [c].[City] IS NULL) AND EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] LIKE N'A' + N'%' AND (LEFT([o].[CustomerID], LEN(N'A')) = N'A'))");
        }

        public override void Any_nested3()
        {
            base.Any_nested3();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] LIKE N'A' + N'%' AND (LEFT([o].[CustomerID], LEN(N'A')) = N'A')) AND (([c].[City] <> N'London') OR [c].[City] IS NULL)");
        }

        public override void Any_with_multiple_conditions_still_uses_exists()
        {
            base.Any_with_multiple_conditions_still_uses_exists();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[City] = N'London') AND EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE ([o].[EmployeeID] = 1) AND ([c].[CustomerID] = [o].[CustomerID]))");
        }

        public override void All_top_level()
        {
            base.All_top_level();

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        WHERE NOT ([c].[ContactName] LIKE N'A' + N'%') OR (LEFT([c].[ContactName], LEN(N'A')) <> N'A'))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void All_top_level_column()
        {
            base.All_top_level_column();

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        WHERE (NOT ([c].[ContactName] LIKE [c].[ContactName] + N'%') OR (LEFT([c].[ContactName], LEN([c].[ContactName])) <> [c].[ContactName])) AND (([c].[ContactName] <> N'') OR [c].[ContactName] IS NULL))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void All_top_level_subquery()
        {
            base.All_top_level_subquery();

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Customers] AS [c1]
        WHERE NOT ((
            SELECT CASE
                WHEN EXISTS (
                    SELECT 1
                    FROM [Customers] AS [c2]
                    WHERE EXISTS (
                        SELECT 1
                        FROM [Customers] AS [c3]
                        WHERE [c1].[CustomerID] = [c3].[CustomerID]))
                THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
            END
        ) = 1))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void All_top_level_subquery_ef_property()
        {
            base.All_top_level_subquery_ef_property();

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Customers] AS [c1]
        WHERE NOT ((
            SELECT CASE
                WHEN EXISTS (
                    SELECT 1
                    FROM [Customers] AS [c2]
                    WHERE EXISTS (
                        SELECT 1
                        FROM [Customers] AS [c3]
                        WHERE [c1].[CustomerID] = [c3].[CustomerID]))
                THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
            END
        ) = 1))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void Select_scalar()
        {
            base.Select_scalar();

            AssertSql(
                @"SELECT [c].[City]
FROM [Customers] AS [c]");
        }

        public override void Select_anonymous_one()
        {
            base.Select_anonymous_one();

            AssertSql(
                @"SELECT [c].[City]
FROM [Customers] AS [c]");
        }

        public override void Select_anonymous_two()
        {
            base.Select_anonymous_two();

            AssertSql(
                @"SELECT [c].[City], [c].[Phone]
FROM [Customers] AS [c]");
        }

        public override void Select_anonymous_three()
        {
            base.Select_anonymous_three();

            AssertSql(
                @"SELECT [c].[City], [c].[Phone], [c].[Country]
FROM [Customers] AS [c]");
        }

        public override void Select_anonymous_bool_constant_true()
        {
            base.Select_anonymous_bool_constant_true();

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]");
        }

        public override void Select_anonymous_constant_in_expression()
        {
            base.Select_anonymous_constant_in_expression();

            AssertSql(
                @"SELECT [c].[CustomerID], CAST(LEN([c].[CustomerID]) AS int) + 5 AS [Expression]
FROM [Customers] AS [c]");
        }

        public override void Select_anonymous_conditional_expression()
        {
            base.Select_anonymous_conditional_expression();

            AssertSql(
                @"SELECT [p].[ProductID], CASE
    WHEN [p].[UnitsInStock] > 0
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END AS [IsAvailable]
FROM [Products] AS [p]");
        }

        public override void Select_scalar_primitive_after_take()
        {
            base.Select_scalar_primitive_after_take();

            AssertSql(
                @"@__p_0: 9

SELECT TOP(@__p_0) [e].[EmployeeID]
FROM [Employees] AS [e]");
        }

        public override void Select_constant_null_string()
        {
            base.Select_constant_null_string();

            AssertSql(
                @"SELECT 1
FROM [Customers] AS [c]");
        }

        public override void Select_local()
        {
            base.Select_local();

            AssertSql(
                @"@__x_0: 10

SELECT @__x_0
FROM [Customers] AS [c]");
        }

        public override void Where_simple()
        {
            base.Where_simple();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'");
        }

        public override void Where_simple_shadow()
        {
            base.Where_simple_shadow();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[Title] = N'Sales Representative'");
        }

        public override void Where_simple_shadow_projection()
        {
            base.Where_simple_shadow_projection();

            AssertSql(
                @"SELECT [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[Title] = N'Sales Representative'");
        }

        public override void Where_comparison_nullable_type_not_null()
        {
            base.Where_comparison_nullable_type_not_null();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = 2");
        }

        public override void Where_comparison_nullable_type_null()
        {
            base.Where_comparison_nullable_type_null();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL");
        }

        public override void Where_client()
        {
            base.Where_client();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_client_and_server_top_level()
        {
            base.Where_client_and_server_top_level();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <> N'AROUT'");
        }

        public override void Where_client_or_server_top_level()
        {
            base.Where_client_or_server_top_level();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_client_and_server_non_top_level()
        {
            base.Where_client_and_server_non_top_level();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_client_deep_inside_predicate_and_server_top_level()
        {
            base.Where_client_deep_inside_predicate_and_server_top_level();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <> N'ALFKI'");
        }

        public override void First_client_predicate()
        {
            base.First_client_predicate();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void FirstOrDefault_inside_subquery_gets_server_evaluated()
        {
            base.FirstOrDefault_inside_subquery_gets_server_evaluated();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] = N'ALFKI') AND ((
    SELECT TOP(1) [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE ([o].[CustomerID] = N'ALFKI') AND ([c].[CustomerID] = [o].[CustomerID])
) = N'ALFKI')");
        }

        public override void First_inside_subquery_gets_client_evaluated()
        {
            base.First_inside_subquery_gets_client_evaluated();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'",
                //
                @"@_outer_CustomerID: ALFKI (Size = 450)

SELECT TOP(1) [o0].[CustomerID]
FROM [Orders] AS [o0]
WHERE ([o0].[CustomerID] = N'ALFKI') AND (@_outer_CustomerID = [o0].[CustomerID])");
        }

        public override void Last()
        {
            base.Last();

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactName] DESC");
        }

        public override void Last_when_no_order_by()
        {
            base.Last_when_no_order_by();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override void Last_Predicate()
        {
            base.Last_Predicate();

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[ContactName] DESC");
        }

        public override void Where_Last()
        {
            base.Where_Last();

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[ContactName] DESC");
        }

        public override void LastOrDefault()
        {
            base.LastOrDefault();

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactName] DESC");
        }

        public override void LastOrDefault_Predicate()
        {
            base.LastOrDefault_Predicate();

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[ContactName] DESC");
        }

        public override void Where_LastOrDefault()
        {
            base.Where_LastOrDefault();

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[ContactName] DESC");
        }

        public override void Where_equals_method_string()
        {
            base.Where_equals_method_string();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'");
        }

        public override void Where_equals_method_int()
        {
            base.Where_equals_method_int();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = 1");
        }

        public override void Where_equals_using_object_overload_on_mismatched_types()
        {
            base.Where_equals_using_object_overload_on_mismatched_types();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 0 = 1");

            Assert.True(
                Fixture.TestSqlLoggerFactory.Log.Contains(
                    "Possible unintended use of method Equals(object) for arguments of different types: 'e.EmployeeID', '__longPrm_0'. This comparison will always return 'false'."));
        }

        public override void Where_equals_using_int_overload_on_mismatched_types()
        {
            base.Where_equals_using_int_overload_on_mismatched_types();

            AssertSql(
                @"@__shortPrm_0: 1 (DbType = Int32)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = @__shortPrm_0");
        }

        public override void Where_equals_on_mismatched_types_int_nullable_int()
        {
            base.Where_equals_on_mismatched_types_int_nullable_int();

            AssertSql(
                @"@__intPrm_0: 2

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__intPrm_0",
                //
                @"@__intPrm_0: 2

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE @__intPrm_0 = [e].[ReportsTo]");
        }

        public override void Where_equals_on_mismatched_types_nullable_int_long()
        {
            base.Where_equals_on_mismatched_types_nullable_int_long();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 0 = 1",
                //
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 0 = 1");

            Assert.True(
                Fixture.TestSqlLoggerFactory.Log.Contains(
                    "Possible unintended use of method Equals(object) for arguments of different types: 'e.ReportsTo', '__longPrm_0'. This comparison will always return 'false'."));

            Assert.True(
                Fixture.TestSqlLoggerFactory.Log.Contains(
                    "Possible unintended use of method Equals(object) for arguments of different types: '__longPrm_0', 'e.ReportsTo'. This comparison will always return 'false'."));
        }

        public override void Where_equals_on_mismatched_types_nullable_long_nullable_int()
        {
            base.Where_equals_on_mismatched_types_nullable_long_nullable_int();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 0 = 1",
                //
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 0 = 1");

            Assert.True(
                Fixture.TestSqlLoggerFactory.Log.Contains(
                    "Possible unintended use of method Equals(object) for arguments of different types: '__nullableLongPrm_0', 'e.ReportsTo'. This comparison will always return 'false'."));

            Assert.True(
                Fixture.TestSqlLoggerFactory.Log.Contains(
                    "Possible unintended use of method Equals(object) for arguments of different types: 'e.ReportsTo', '__nullableLongPrm_0'. This comparison will always return 'false'."));
        }

        public override void Where_equals_on_matched_nullable_int_types()
        {
            base.Where_equals_on_matched_nullable_int_types();

            AssertSql(
                @"@__nullableIntPrm_0: 2 (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE @__nullableIntPrm_0 = [e].[ReportsTo]",
                //
                @"@__nullableIntPrm_0: 2 (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__nullableIntPrm_0");
        }

        public override void Where_equals_on_null_nullable_int_types()
        {
            base.Where_equals_on_null_nullable_int_types();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL",
                //
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL");
        }

        public override void Where_string_length()
        {
            base.Where_string_length();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE CAST(LEN([c].[City]) AS int) = 6");
        }

        public override void Where_datetime_date_component()
        {
            base.Where_datetime_date_component();

            AssertSql(
                @"@__myDatetime_0: 05/04/1998 00:00:00

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE CONVERT(date, [o].[OrderDate]) = @__myDatetime_0");
        }

        public override void Where_datetime_day_component()
        {
            base.Where_datetime_day_component();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(day, [o].[OrderDate]) = 4");
        }

        public override void Where_date_add_year_constant_component()
        {
            base.Where_date_add_year_constant_component();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(year, DATEADD(year, -1, [o].[OrderDate])) = 1997");
        }

        public override void Where_datetime_year_component()
        {
            base.Where_datetime_year_component();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(year, [o].[OrderDate]) = 1998");
        }

        public override void Where_datetime_dayOfYear_component()
        {
            base.Where_datetime_dayOfYear_component();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(dayofyear, [o].[OrderDate]) = 68");
        }

        public override void Where_datetime_month_component()
        {
            base.Where_datetime_month_component();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(month, [o].[OrderDate]) = 4");
        }

        public override void Where_datetime_hour_component()
        {
            base.Where_datetime_hour_component();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(hour, [o].[OrderDate]) = 14");
        }

        public override void Where_datetime_minute_component()
        {
            base.Where_datetime_minute_component();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(minute, [o].[OrderDate]) = 23");
        }

        public override void Where_datetime_second_component()
        {
            base.Where_datetime_second_component();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(second, [o].[OrderDate]) = 44");
        }

        public override void Where_datetime_millisecond_component()
        {
            base.Where_datetime_millisecond_component();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(millisecond, [o].[OrderDate]) = 88");
        }

        public override void Where_datetime_now()
        {
            base.Where_datetime_now();

            AssertSql(
                @"@__myDatetime_0: 04/10/2015 00:00:00

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE GETDATE() <> @__myDatetime_0");
        }

        public override void Where_datetime_utcnow()
        {
            base.Where_datetime_utcnow();

            AssertSql(
                @"@__myDatetime_0: 04/10/2015 00:00:00

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE GETUTCDATE() <> @__myDatetime_0");
        }

        public override void Where_is_null()
        {
            base.Where_is_null();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] IS NULL");
        }

        public override void Where_is_not_null()
        {
            base.Where_is_not_null();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] IS NOT NULL");
        }

        public override void Where_null_is_null()
        {
            base.Where_null_is_null();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_constant_is_null()
        {
            base.Where_constant_is_null();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1");
        }

        public override void Where_null_is_not_null()
        {
            base.Where_null_is_not_null();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1");
        }

        public override void Where_constant_is_not_null()
        {
            base.Where_constant_is_not_null();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_simple_reversed()
        {
            base.Where_simple_reversed();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE N'London' = [c].[City]");
        }

        public override void Where_identity_comparison()
        {
            base.Where_identity_comparison();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[City] = [c].[City]) OR ([c].[City] IS NULL AND [c].[City] IS NULL)");
        }

        public override void Where_select_many_or()
        {
            base.Where_select_many_or();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE ([c].[City] = N'London') OR ([e].[City] = N'London')");
        }

        public override void Where_select_many_or2()
        {
            base.Where_select_many_or2();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] IN (N'London', N'Berlin')");
        }

        public override void Where_select_many_or3()
        {
            base.Where_select_many_or3();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] IN (N'London', N'Berlin', N'Seattle')");
        }

        public override void Where_select_many_or4()
        {
            base.Where_select_many_or4();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] IN (N'London', N'Berlin', N'Seattle', N'Lisboa')");
        }

        public override void Where_select_many_or_with_parameter()
        {
            base.Where_select_many_or_with_parameter();

            AssertSql(
                @"@__london_0: London (Size = 4000)
@__lisboa_1: Lisboa (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] IN (@__london_0, N'Berlin', N'Seattle', @__lisboa_1)");
        }

        public override void Where_in_optimization_multiple()
        {
            base.Where_in_optimization_multiple();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE ([c].[City] IN (N'London', N'Berlin') OR ([c].[CustomerID] = N'ALFKI')) OR ([c].[CustomerID] = N'ABCDE')");
        }

        public override void Where_not_in_optimization1()
        {
            base.Where_not_in_optimization1();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE (([c].[City] <> N'London') OR [c].[City] IS NULL) AND (([e].[City] <> N'London') OR [e].[City] IS NULL)");
        }

        public override void Where_not_in_optimization2()
        {
            base.Where_not_in_optimization2();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] NOT IN (N'London', N'Berlin')");
        }

        public override void Where_not_in_optimization3()
        {
            base.Where_not_in_optimization3();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] NOT IN (N'London', N'Berlin', N'Seattle')");
        }

        public override void Where_not_in_optimization4()
        {
            base.Where_not_in_optimization4();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] NOT IN (N'London', N'Berlin', N'Seattle', N'Lisboa')");
        }

        public override void Where_select_many_and()
        {
            base.Where_select_many_and();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE (([c].[City] = N'London') AND ([c].[Country] = N'UK')) AND (([e].[City] = N'London') AND ([e].[Country] = N'UK'))");
        }

        public override void Select_project_filter()
        {
            base.Select_project_filter();

            AssertSql(
                @"SELECT [c].[CompanyName]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'");
        }

        public override void Select_project_filter2()
        {
            base.Select_project_filter2();

            AssertSql(
                @"SELECT [c].[City]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'");
        }

        public override void SelectMany_mixed()
        {
            base.SelectMany_mixed();

            AssertSql(
                @"@__p_0: 2

SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
    ORDER BY [e].[EmployeeID]
) AS [t]",
                //
                @"SELECT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [t0]",
                //
                @"SELECT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [t0]",
                //
                @"SELECT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [t0]",
                //
                @"SELECT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [t0]");
        }

        public override void SelectMany_simple_subquery()
        {
            base.SelectMany_simple_subquery();

            AssertSql(
                @"@__p_0: 9

SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
) AS [t]
CROSS JOIN [Customers] AS [c]");
        }

        public override void SelectMany_simple1()
        {
            base.SelectMany_simple1();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Employees] AS [e]
CROSS JOIN [Customers] AS [c]");
        }

        public override void SelectMany_simple2()
        {
            base.SelectMany_simple2();

            AssertSql(
                @"SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e2].[FirstName] AS [FirstName0]
FROM [Employees] AS [e1]
CROSS JOIN [Customers] AS [c]
CROSS JOIN [Employees] AS [e2]");
        }

        public override void SelectMany_entity_deep()
        {
            base.SelectMany_entity_deep();

            AssertSql(
                @"SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title], [e2].[EmployeeID], [e2].[City], [e2].[Country], [e2].[FirstName], [e2].[ReportsTo], [e2].[Title], [e3].[EmployeeID], [e3].[City], [e3].[Country], [e3].[FirstName], [e3].[ReportsTo], [e3].[Title], [e4].[EmployeeID], [e4].[City], [e4].[Country], [e4].[FirstName], [e4].[ReportsTo], [e4].[Title]
FROM [Employees] AS [e1]
CROSS JOIN [Employees] AS [e2]
CROSS JOIN [Employees] AS [e3]
CROSS JOIN [Employees] AS [e4]");
        }

        public override void SelectMany_projection1()
        {
            base.SelectMany_projection1();

            AssertSql(
                @"SELECT [e1].[City], [e2].[Country]
FROM [Employees] AS [e1]
CROSS JOIN [Employees] AS [e2]");
        }

        public override void SelectMany_projection2()
        {
            base.SelectMany_projection2();

            AssertSql(
                @"SELECT [e1].[City], [e2].[Country], [e3].[FirstName]
FROM [Employees] AS [e1]
CROSS JOIN [Employees] AS [e2]
CROSS JOIN [Employees] AS [e3]");
        }

        public override void SelectMany_Count()
        {
            base.SelectMany_Count();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]");
        }

        public override void SelectMany_LongCount()
        {
            base.SelectMany_LongCount();

            AssertSql(
                @"SELECT COUNT_BIG(*)
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]");
        }

        public override void SelectMany_OrderBy_ThenBy_Any()
        {
            base.SelectMany_OrderBy_ThenBy_Any();

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        CROSS JOIN [Orders] AS [o])
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void Join_customers_orders_projection()
        {
            base.Join_customers_orders_projection();

            AssertSql(
                @"SELECT [c].[ContactName], [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override void Join_customers_orders_entities()
        {
            base.Join_customers_orders_entities();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override void Join_composite_key()
        {
            base.Join_composite_key();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON ([c].[CustomerID] = [o].[CustomerID]) AND ([c].[CustomerID] = [o].[CustomerID])");
        }

        public override void Join_complex_condition()
        {
            base.Join_complex_condition();

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [o].*
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10250
) AS [t] ON 1 = 1
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override void Join_client_new_expression()
        {
            base.Join_client_new_expression();

            AssertContainsSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Join_select_many()
        {
            base.Join_select_many();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
CROSS JOIN [Employees] AS [e]");
        }

        public override void Client_Join_select_many()
        {
            base.Client_Join_select_many();

            AssertContainsSql(
                @"@__p_1: 2

SELECT [t0].[EmployeeID], [t0].[City], [t0].[Country], [t0].[FirstName], [t0].[ReportsTo], [t0].[Title]
FROM (
    SELECT TOP(@__p_1) [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title]
    FROM [Employees] AS [e0]
) AS [t0]",
                //
                @"@__p_0: 2

SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
) AS [t]",
                //
                @"SELECT [t1].[EmployeeID], [t1].[City], [t1].[Country], [t1].[FirstName], [t1].[ReportsTo], [t1].[Title]
FROM (
    SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
    FROM [Employees] AS [e1]
    ORDER BY (SELECT 1)
    OFFSET 6 ROWS FETCH NEXT 2 ROWS ONLY
) AS [t1]",
                //
                @"SELECT [t1].[EmployeeID], [t1].[City], [t1].[Country], [t1].[FirstName], [t1].[ReportsTo], [t1].[Title]
FROM (
    SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
    FROM [Employees] AS [e1]
    ORDER BY (SELECT 1)
    OFFSET 6 ROWS FETCH NEXT 2 ROWS ONLY
) AS [t1]");
        }

        public override void Join_Where_Count()
        {
            base.Join_Where_Count();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override void Join_OrderBy_Count()
        {
            base.Join_OrderBy_Count();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override void Join_customers_orders_with_subquery()
        {
            base.Join_customers_orders_with_subquery();

            AssertContainsSql(
                @"SELECT [o20].[CustomerID], [o20].[OrderID]
FROM [Orders] AS [o20]
ORDER BY [o20].[OrderID]",
                //
                @"SELECT [c].[CustomerID], [c].[ContactName]
FROM [Customers] AS [c]");
        }

        public override void Join_customers_orders_with_subquery_with_take()
        {
            base.Join_customers_orders_with_subquery_with_take();

            AssertSql(
                @"@__p_0: 5

SELECT [c].[ContactName], [t].[OrderID]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT TOP(@__p_0) [o2].*
    FROM [Orders] AS [o2]
    ORDER BY [o2].[OrderID]
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]
WHERE [t].[CustomerID] = N'ALFKI'");
        }

        public override void Join_customers_orders_with_subquery_anonymous_property_method()
        {
            base.Join_customers_orders_with_subquery_anonymous_property_method();

            AssertContainsSql(
                @"SELECT [o20].[OrderID], [o20].[CustomerID], [o20].[EmployeeID], [o20].[OrderDate]
FROM [Orders] AS [o20]
ORDER BY [o20].[OrderID]",
                //
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]");
        }

        public override void Join_customers_orders_with_subquery_anonymous_property_method_with_take()
        {
            base.Join_customers_orders_with_subquery_anonymous_property_method_with_take();

            AssertContainsSql(
                @"@__p_0: 5

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate]
    FROM [Orders] AS [o2]
    ORDER BY [o2].[OrderID]
) AS [t]",
                //
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]");
        }

        public override void Join_customers_orders_with_subquery_predicate()
        {
            base.Join_customers_orders_with_subquery_predicate();

            AssertContainsSql(
                @"SELECT [o20].[CustomerID], [o20].[OrderID]
FROM [Orders] AS [o20]
WHERE [o20].[OrderID] > 0
ORDER BY [o20].[OrderID]",
                //
                @"SELECT [c].[CustomerID], [c].[ContactName]
FROM [Customers] AS [c]");
        }

        public override void Join_customers_orders_with_subquery_predicate_with_take()
        {
            base.Join_customers_orders_with_subquery_predicate_with_take();

            AssertSql(
                @"@__p_0: 5

SELECT [c].[ContactName], [t].[OrderID]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT TOP(@__p_0) [o2].*
    FROM [Orders] AS [o2]
    WHERE [o2].[OrderID] > 0
    ORDER BY [o2].[OrderID]
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]
WHERE [t].[CustomerID] = N'ALFKI'");
        }

        public override void Join_customers_orders_select()
        {
            base.Join_customers_orders_select();

            AssertSql(
                @"SELECT [c].[ContactName], [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override void Multiple_joins_Where_Order_Any()
        {
            base.Multiple_joins_Where_Order_Any();

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        INNER JOIN [Orders] AS [or] ON [c].[CustomerID] = [or].[CustomerID]
        INNER JOIN [Order Details] AS [od] ON [or].[OrderID] = [od].[OrderID]
        WHERE [c].[City] = N'London')
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void Where_join_select()
        {
            base.Where_join_select();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override void Where_orderby_join_select()
        {
            base.Where_orderby_join_select();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] <> N'ALFKI'
ORDER BY [c].[CustomerID]");
        }

        public override void Where_join_orderby_join_select()
        {
            base.Where_join_orderby_join_select();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
INNER JOIN [Order Details] AS [od] ON [o].[OrderID] = [od].[OrderID]
WHERE [c].[CustomerID] <> N'ALFKI'
ORDER BY [c].[CustomerID]");
        }

        public override void Where_select_many()
        {
            base.Where_select_many();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override void Where_orderby_select_many()
        {
            base.Where_orderby_select_many();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]");
        }

        public override void GroupBy_simple()
        {
            base.GroupBy_simple();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
ORDER BY [o].[CustomerID]");
        }

        public override void GroupBy_Distinct()
        {
            base.GroupBy_Distinct();

            AssertSql(
                @"SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o0]
ORDER BY [o0].[CustomerID]");
        }

        public override void GroupBy_Count()
        {
            base.GroupBy_Count();

            AssertSql(
                @"SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o0]
ORDER BY [o0].[CustomerID]");
        }

        public override void GroupBy_LongCount()
        {
            base.GroupBy_LongCount();

            AssertSql(
                @"SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o0]
ORDER BY [o0].[CustomerID]");
        }

        public override void GroupBy_DateTimeOffset_Property()
        {
            base.GroupBy_DateTimeOffset_Property();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL
ORDER BY DATEPART(month, [o].[OrderDate])");
        }

        public override void Select_GroupBy()
        {
            base.Select_GroupBy();

            AssertSql(
                @"SELECT [o].[OrderID] AS [Order], [o].[CustomerID] AS [Customer]
FROM [Orders] AS [o]
ORDER BY [o].[CustomerID]");
        }

        public override void Select_GroupBy_SelectMany()
        {
            base.Select_GroupBy_SelectMany();

            AssertSql(
                @"SELECT [o0].[OrderID] AS [Order], [o0].[CustomerID] AS [Customer]
FROM [Orders] AS [o0]
ORDER BY [o0].[OrderID]");
        }

        public override void GroupBy_with_orderby()
        {
            base.GroupBy_with_orderby();

            AssertSql(
                @"SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o0]
ORDER BY [o0].[CustomerID]");
        }

        public override void GroupBy_with_orderby_and_anonymous_projection()
        {
            base.GroupBy_with_orderby_and_anonymous_projection();

            AssertSql(
                @"SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o0]
ORDER BY [o0].[CustomerID]");
        }

        public override void GroupBy_with_orderby_take_skip_distinct()
        {
            base.GroupBy_with_orderby_take_skip_distinct();

            AssertSql(
                @"SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o0]
ORDER BY [o0].[CustomerID]");
        }

        public override void Select_All()
        {
            base.Select_All();

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE ([o].[CustomerID] <> N'ALFKI') OR [o].[CustomerID] IS NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void SelectMany_cartesian_product_with_ordering()
        {
            base.SelectMany_cartesian_product_with_ordering();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[City] AS [City0]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE ([c].[City] = [e].[City]) OR ([c].[City] IS NULL AND [e].[City] IS NULL)
ORDER BY [City0], [c].[CustomerID] DESC");
        }

        public override void GroupJoin_DefaultIfEmpty()
        {
            base.GroupJoin_DefaultIfEmpty();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override void GroupJoin_DefaultIfEmpty_multiple()
        {
            base.GroupJoin_DefaultIfEmpty_multiple();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
LEFT JOIN [Orders] AS [o2] ON [c].[CustomerID] = [o2].[CustomerID]");
        }

        public override void GroupJoin_DefaultIfEmpty2()
        {
            base.GroupJoin_DefaultIfEmpty2();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Employees] AS [e]
LEFT JOIN [Orders] AS [o] ON [e].[EmployeeID] = [o].[EmployeeID]");
        }

        public override void GroupJoin_DefaultIfEmpty3()
        {
            base.GroupJoin_DefaultIfEmpty3();

            AssertSql(
                @"@__p_0: 1

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [c].*
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]");
        }

        public override void GroupJoin_Where()
        {
            base.GroupJoin_Where();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [o].[CustomerID] = N'ALFKI'");
        }

        public override void GroupJoin_Where_OrderBy()
        {
            base.GroupJoin_Where_OrderBy();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE ([o].[CustomerID] = N'ALFKI') OR ([c].[CustomerID] = N'ANATR')
ORDER BY [c].[City]");
        }

        public override void GroupJoin_DefaultIfEmpty_Where()
        {
            base.GroupJoin_DefaultIfEmpty_Where();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [o].[OrderID] IS NOT NULL AND ([o].[CustomerID] = N'ALFKI')");
        }

        public override void Join_GroupJoin_DefaultIfEmpty_Where()
        {
            base.Join_GroupJoin_DefaultIfEmpty_Where();

            AssertSql(
                @"SELECT [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
LEFT JOIN [Orders] AS [o2] ON [c].[CustomerID] = [o2].[CustomerID]
WHERE [o2].[OrderID] IS NOT NULL AND ([o2].[CustomerID] = N'ALFKI')");
        }

        public override void GroupJoin_DefaultIfEmpty_Project()
        {
            base.GroupJoin_DefaultIfEmpty_Project();

            AssertSql(
                @"SELECT [o].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override void GroupJoin_with_different_outer_elements_with_same_key()
        {
            base.GroupJoin_with_different_outer_elements_with_same_key();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]");
        }

        public override void GroupJoin_with_different_outer_elements_with_same_key_with_predicate()
        {
            base.GroupJoin_with_different_outer_elements_with_same_key_with_predicate();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] > 11500");
        }

        public override void GroupJoin_with_different_outer_elements_with_same_key_projected_from_another_entity()
        {
            base.GroupJoin_with_different_outer_elements_with_same_key_projected_from_another_entity();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [od.Order] ON [od].[OrderID] = [od.Order].[OrderID]
LEFT JOIN [Customers] AS [c] ON [od.Order].[CustomerID] = [c].[CustomerID]");
        }

        public override void GroupJoin_simple()
        {
            base.GroupJoin_simple();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override void GroupJoin_simple2()
        {
            base.GroupJoin_simple2();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override void GroupJoin_simple_ordering()
        {
            base.GroupJoin_simple_ordering();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[City]");
        }

        public override void GroupJoin_simple_subquery()
        {
            base.GroupJoin_simple_subquery();

            AssertSql(
                @"@__p_0: 4

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]");
        }

        public override void GroupJoin_customers_orders_count()
        {
            base.GroupJoin_customers_orders_count();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID]");
        }

        public override void GroupJoin_customers_orders_count_preserves_ordering()
        {
            base.GroupJoin_customers_orders_count_preserves_ordering();

            AssertSql(
                @"@__p_0: 5

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] <> N'VAFFE'
    ORDER BY [c].[City]
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[City], [t].[CustomerID]");
        }

        public override void GroupJoin_tracking_groups()
        {
            base.GroupJoin_tracking_groups();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID]");
        }

        public override void GroupJoin_simple3()
        {
            base.GroupJoin_simple3();

            AssertSql(
                @"SELECT [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override void SelectMany_Joined_DefaultIfEmpty()
        {
            base.SelectMany_Joined_DefaultIfEmpty();

            AssertSql(
                @"SELECT [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate], [c].[ContactName]
FROM [Customers] AS [c]
CROSS APPLY (
    SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
    FROM (
        SELECT NULL AS [empty]
    ) AS [empty]
    LEFT JOIN (
        SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [o].[CustomerID] = [c].[CustomerID]
    ) AS [t] ON 1 = 1
) AS [t0]");
        }

        public override void SelectMany_Joined_DefaultIfEmpty2()
        {
            base.SelectMany_Joined_DefaultIfEmpty2();

            AssertSql(
                @"SELECT [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate]
FROM [Customers] AS [c]
CROSS APPLY (
    SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
    FROM (
        SELECT NULL AS [empty]
    ) AS [empty]
    LEFT JOIN (
        SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [o].[CustomerID] = [c].[CustomerID]
    ) AS [t] ON 1 = 1
) AS [t0]");
        }

        public override void SelectMany_Joined_Take()
        {
            base.SelectMany_Joined_Take();

            AssertSql(
                @"SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], [c].[ContactName]
FROM [Customers] AS [c]
CROSS APPLY (
    SELECT TOP(1000) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = [c].[CustomerID]
) AS [t]");
        }

        public override void Take_with_single()
        {
            base.Take_with_single();

            AssertSql(
                @"@__p_0: 1

SELECT TOP(2) [t].*
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [t]
ORDER BY [t].[CustomerID]");
        }

        public override void Take_with_single_select_many()
        {
            base.Take_with_single_select_many();

            AssertSql(
                @"@__p_0: 1

SELECT TOP(2) [t].*
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID] AS [CustomerID0], [o].[EmployeeID], [o].[OrderDate]
    FROM [Customers] AS [c]
    CROSS JOIN [Orders] AS [o]
    ORDER BY [c].[CustomerID], [o].[OrderID]
) AS [t]
ORDER BY [t].[CustomerID], [t].[OrderID]");
        }

        public override void Distinct()
        {
            base.Distinct();

            AssertSql(
                @"SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Distinct_Scalar()
        {
            base.Distinct_Scalar();

            AssertSql(
                @"SELECT DISTINCT [c].[City]
FROM [Customers] AS [c]");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Distinct_Skip() => base.Distinct_Skip();

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Distinct_Skip_Take() => base.Distinct_Skip_Take();

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Skip_Distinct() => base.Skip_Distinct();

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Skip_Take_Distinct() => base.Skip_Take_Distinct();

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Skip_Take_Any()
        {
            base.Skip_Take_Any();

            AssertSql(
                @"@__p_0: 5
@__p_1: 10

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactName]
        OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Skip_Take_All()
        {
            base.Skip_Take_All();

            AssertSql(
                @"@__p_0: 5
@__p_1: 10

SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        WHERE CAST(LEN([c].[CustomerID]) AS int) <> 5
        ORDER BY [c].[ContactName]
        OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void OrderBy()
        {
            base.OrderBy();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void OrderBy_true()
        {
            base.OrderBy_true();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY (SELECT 1)");
        }

        public override void OrderBy_integer()
        {
            base.OrderBy_integer();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY (SELECT 1)");
        }

        public override void OrderBy_parameter()
        {
            base.OrderBy_parameter();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY (SELECT 1)");
        }

        public override void OrderBy_anon()
        {
            base.OrderBy_anon();

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void OrderBy_anon2()
        {
            base.OrderBy_anon2();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void OrderBy_client_mixed()
        {
            base.OrderBy_client_mixed();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void OrderBy_multiple_queries()
        {
            base.OrderBy_multiple_queries();

            AssertContainsSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void OrderBy_Distinct()
        {
            base.OrderBy_Distinct();

            // Ordering not preserved by distinct when ordering columns not projected.
            AssertSql(
                @"SELECT DISTINCT [c].[City]
FROM [Customers] AS [c]");
        }

        public override void Distinct_OrderBy()
        {
            base.Distinct_OrderBy();

            AssertSql(
                @"SELECT [t].[Country]
FROM (
    SELECT DISTINCT [c].[Country]
    FROM [Customers] AS [c]
) AS [t]
ORDER BY [t].[Country]");
        }

        public override void Distinct_OrderBy2()
        {
            base.Distinct_OrderBy2();

            AssertSql(
                @"SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
) AS [t]
ORDER BY [t].[CustomerID]");
        }

        public override void Distinct_OrderBy3()
        {
            base.Distinct_OrderBy3();

            AssertSql(
                @"SELECT [t].[CustomerID]
FROM (
    SELECT DISTINCT [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [t]
ORDER BY [t].[CustomerID]");
        }

        public override void Take_Distinct()
        {
            base.Take_Distinct();

            AssertSql(
                @"@__p_0: 5

SELECT DISTINCT [t].*
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]");
        }

        public override void Distinct_Take()
        {
            base.Distinct_Take();

            AssertSql(
                @"@__p_0: 5

SELECT TOP(@__p_0) [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM (
    SELECT DISTINCT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
) AS [t]
ORDER BY [t].[OrderID]");
        }

        public override void Distinct_Take_Count()
        {
            base.Distinct_Take_Count();

            AssertSql(
                @"@__p_0: 5

SELECT COUNT(*)
FROM (
    SELECT DISTINCT TOP(@__p_0) [o].*
    FROM [Orders] AS [o]
) AS [t]");
        }

        public override void OrderBy_shadow()
        {
            base.OrderBy_shadow();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
ORDER BY [e].[Title], [e].[EmployeeID]");
        }

        public override void OrderBy_multiple()
        {
            base.OrderBy_multiple();

            AssertSql(
                @"SELECT [c].[City]
FROM [Customers] AS [c]
ORDER BY [c].[Country], [c].[CustomerID]");
        }

        public override void OrderBy_ThenBy_Any()
        {
            base.OrderBy_ThenBy_Any();

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c])
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void OrderBy_correlated_subquery1()
        {
            base.OrderBy_correlated_subquery1();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')
ORDER BY (
    SELECT CASE
        WHEN EXISTS (
            SELECT 1
            FROM [Customers] AS [c2]
            WHERE [c2].[CustomerID] = [c].[CustomerID])
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END
)");
        }

        public override void OrderBy_correlated_subquery2()
        {
            base.OrderBy_correlated_subquery2();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[OrderID] <= 10250) AND ((
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
) <> N'Nowhere')");
        }

        public override void Where_subquery_recursive_trivial()
        {
            base.Where_subquery_recursive_trivial();

            AssertSql(
                @"SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
FROM [Employees] AS [e1]
WHERE EXISTS (
    SELECT 1
    FROM [Employees] AS [e2]
    WHERE EXISTS (
        SELECT 1
        FROM [Employees] AS [e3]))
ORDER BY [e1].[EmployeeID]");
        }

        public override void Where_false()
        {
            base.Where_false();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1");
        }

        public override void Where_default()
        {
            base.Where_default();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[Fax] IS NULL");
        }

        public override void Where_expression_invoke()
        {
            base.Where_expression_invoke();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override void Where_ternary_boolean_condition_true()
        {
            base.Where_ternary_boolean_condition_true();

            AssertSql(
                @"@__flag_0: True

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ((@__flag_0 = 1) AND ([p].[UnitsInStock] >= 20)) OR ((@__flag_0 <> 1) AND ([p].[UnitsInStock] < 20))");
        }

        public override void Where_ternary_boolean_condition_false()
        {
            base.Where_ternary_boolean_condition_false();

            AssertSql(
                @"@__flag_0: False

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ((@__flag_0 = 1) AND ([p].[UnitsInStock] >= 20)) OR ((@__flag_0 <> 1) AND ([p].[UnitsInStock] < 20))");
        }

        public override void Where_ternary_boolean_condition_with_another_condition()
        {
            base.Where_ternary_boolean_condition_with_another_condition();

            AssertSql(
                @"@__productId_0: 15
@__flag_1: True

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ([p].[ProductID] < @__productId_0) AND (((@__flag_1 = 1) AND ([p].[UnitsInStock] >= 20)) OR ((@__flag_1 <> 1) AND ([p].[UnitsInStock] < 20)))");
        }

        public override void Where_ternary_boolean_condition_with_false_as_result_true()
        {
            base.Where_ternary_boolean_condition_with_false_as_result_true();

            AssertSql(
                @"@__flag_0: True

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE (@__flag_0 = 1) AND ([p].[UnitsInStock] >= 20)");
        }

        public override void Where_ternary_boolean_condition_with_false_as_result_false()
        {
            base.Where_ternary_boolean_condition_with_false_as_result_false();

            AssertSql(
                @"@__flag_0: False

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE (@__flag_0 = 1) AND ([p].[UnitsInStock] >= 20)");
        }

        public override void Where_concat_string_int_comparison1()
        {
            base.Where_concat_string_int_comparison1();

            AssertSql(
                @"@__i_0: 10

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] + CAST(@__i_0 AS nvarchar(max))) = [c].[CompanyName]");
        }

        public override void Where_concat_string_int_comparison2()
        {
            base.Where_concat_string_int_comparison2();

            AssertSql(
                @"@__i_0: 10

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE (CAST(@__i_0 AS nvarchar(max)) + [c].[CustomerID]) = [c].[CompanyName]");
        }

        public override void Where_concat_string_int_comparison3()
        {
            base.Where_concat_string_int_comparison3();

            AssertSql(
                @"@__i_0: 10
@__j_1: 21

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE (((CAST(@__i_0 + 20 AS nvarchar(max)) + [c].[CustomerID]) + CAST(@__j_1 AS nvarchar(max))) + CAST(42 AS nvarchar(max))) = [c].[CompanyName]");
        }

        public override void Where_primitive()
        {
            base.Where_primitive();

            AssertSql(
                @"@__p_0: 9

SELECT [t].[EmployeeID]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID]
    FROM [Employees] AS [e]
) AS [t]
WHERE [t].[EmployeeID] = 5");
        }

        public override void Where_bool_member()
        {
            base.Where_bool_member();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 1");
        }

        public override void Where_bool_member_false()
        {
            base.Where_bool_member_false();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 0");
        }

        public override void Where_bool_client_side_negated()
        {
            base.Where_bool_client_side_negated();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 1");
        }

        public override void Where_bool_member_negated_twice()
        {
            base.Where_bool_member_negated_twice();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 1");
        }

        public override void Where_bool_member_shadow()
        {
            base.Where_bool_member_shadow();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 1");
        }

        public override void Where_bool_member_false_shadow()
        {
            base.Where_bool_member_false_shadow();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 0");
        }

        public override void Where_bool_member_equals_constant()
        {
            base.Where_bool_member_equals_constant();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 1");
        }

        public override void Where_bool_member_in_complex_predicate()
        {
            base.Where_bool_member_in_complex_predicate();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE (([p].[ProductID] > 100) AND ([p].[Discontinued] = 1)) OR ([p].[Discontinued] = 1)");
        }

        public override void Where_bool_member_compared_to_binary_expression()
        {
            base.Where_bool_member_compared_to_binary_expression();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CASE
    WHEN [p].[ProductID] > 50
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void Where_not_bool_member_compared_to_binary_expression()
        {
            base.Where_not_bool_member_compared_to_binary_expression();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] <> CASE
    WHEN [p].[ProductID] > 50
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void Where_not_bool_member_compared_to_not_bool_member()
        {
            base.Where_not_bool_member_compared_to_not_bool_member();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = [p].[Discontinued]");
        }

        public override void Where_negated_boolean_expression_compared_to_another_negated_boolean_expression()
        {
            base.Where_negated_boolean_expression_compared_to_another_negated_boolean_expression();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE CASE
    WHEN [p].[ProductID] > 50
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN [p].[ProductID] > 20
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void Where_bool_parameter()
        {
            base.Where_bool_parameter();

            AssertSql(
                @"@__prm_0: True

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE @__prm_0 = 1");
        }

        public override void Where_bool_parameter_compared_to_binary_expression()
        {
            base.Where_bool_parameter_compared_to_binary_expression();

            AssertSql(
                @"@__prm_0: True

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE CASE
    WHEN [p].[ProductID] > 50
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END <> @__prm_0");
        }

        public override void Where_bool_member_and_parameter_compared_to_binary_expression_nested()
        {
            base.Where_bool_member_and_parameter_compared_to_binary_expression_nested();

            AssertSql(
                @"@__prm_0: True

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CASE
    WHEN CASE
        WHEN [p].[ProductID] > 50
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END <> @__prm_0
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void Where_de_morgan_or_optimizated()
        {
            base.Where_de_morgan_or_optimizated();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ([p].[Discontinued] = 0) AND ([p].[ProductID] >= 20)");
        }

        public override void Where_de_morgan_and_optimizated()
        {
            base.Where_de_morgan_and_optimizated();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ([p].[Discontinued] = 0) OR ([p].[ProductID] >= 20)");
        }

        public override void Where_complex_negated_expression_optimized()
        {
            base.Where_complex_negated_expression_optimized();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE (([p].[Discontinued] = 0) AND ([p].[ProductID] < 60)) AND ([p].[ProductID] > 30)");
        }

        public override void Where_short_member_comparison()
        {
            base.Where_short_member_comparison();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[UnitsInStock] > 10");
        }

        public override void Where_comparison_to_nullable_bool()
        {
            base.Where_comparison_to_nullable_bool();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE RIGHT([c].[CustomerID], LEN(N'KI')) = N'KI'");
        }

        public override void Where_true()
        {
            base.Where_true();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_compare_constructed_equal()
        {
            base.Where_compare_constructed_equal();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_compare_constructed_multi_value_equal()
        {
            base.Where_compare_constructed_multi_value_equal();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_compare_constructed_multi_value_not_equal()
        {
            base.Where_compare_constructed_multi_value_not_equal();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_compare_constructed()
        {
            base.Where_compare_constructed();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_compare_null()
        {
            base.Where_compare_null();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] IS NULL AND ([c].[Country] = N'UK')");
        }

        public override void Where_Is_on_same_type()
        {
            base.Where_Is_on_same_type();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Single_Predicate()
        {
            base.Single_Predicate();

            AssertSql(
                @"SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override void Projection_when_arithmetic_expression_precendence()
        {
            base.Projection_when_arithmetic_expression_precendence();

            AssertSql(
                @"SELECT [o].[OrderID] / ([o].[OrderID] / 2) AS [A], ([o].[OrderID] / [o].[OrderID]) / 2 AS [B]
FROM [Orders] AS [o]");
        }

        // TODO: Complex projection translation.

        //        public override void Projection_when_arithmetic_expressions()
        //        {
        //            base.Projection_when_arithmetic_expressions();
        //
        //            AssertSql(
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

            AssertSql(
                @"SELECT [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void String_StartsWith_Literal()
        {
            base.String_StartsWith_Literal();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE N'M' + N'%' AND (LEFT([c].[ContactName], LEN(N'M')) = N'M')");
        }

        public override void String_StartsWith_Identity()
        {
            base.String_StartsWith_Identity();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[ContactName] LIKE [c].[ContactName] + N'%' AND (LEFT([c].[ContactName], LEN([c].[ContactName])) = [c].[ContactName])) OR ([c].[ContactName] = N'')");
        }

        public override void String_StartsWith_Column()
        {
            base.String_StartsWith_Column();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[ContactName] LIKE [c].[ContactName] + N'%' AND (LEFT([c].[ContactName], LEN([c].[ContactName])) = [c].[ContactName])) OR ([c].[ContactName] = N'')");
        }

        public override void String_StartsWith_MethodCall()
        {
            base.String_StartsWith_MethodCall();

            AssertSql(
                @"@__LocalMethod1_0: M (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[ContactName] LIKE @__LocalMethod1_0 + N'%' AND (LEFT([c].[ContactName], LEN(@__LocalMethod1_0)) = @__LocalMethod1_0)) OR (@__LocalMethod1_0 = N'')");
        }

        public override void String_EndsWith_Literal()
        {
            base.String_EndsWith_Literal();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE RIGHT([c].[ContactName], LEN(N'b')) = N'b'");
        }

        public override void String_EndsWith_Identity()
        {
            base.String_EndsWith_Identity();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (RIGHT([c].[ContactName], LEN([c].[ContactName])) = [c].[ContactName]) OR ([c].[ContactName] = N'')");
        }

        public override void String_EndsWith_Column()
        {
            base.String_EndsWith_Column();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (RIGHT([c].[ContactName], LEN([c].[ContactName])) = [c].[ContactName]) OR ([c].[ContactName] = N'')");
        }

        public override void String_EndsWith_MethodCall()
        {
            base.String_EndsWith_MethodCall();

            AssertSql(
                @"@__LocalMethod2_0: m (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (RIGHT([c].[ContactName], LEN(@__LocalMethod2_0)) = @__LocalMethod2_0) OR (@__LocalMethod2_0 = N'')");
        }

        public override void String_Contains_Literal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains("M")), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains("M") || c.ContactName.Contains("m")), // case-sensitive
                entryCount: 34);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE CHARINDEX(N'M', [c].[ContactName]) > 0");
        }

        public override void String_Contains_Identity()
        {
            base.String_Contains_Identity();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (CHARINDEX([c].[ContactName], [c].[ContactName]) > 0) OR ([c].[ContactName] = N'')");
        }

        public override void String_Contains_Column()
        {
            base.String_Contains_Column();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (CHARINDEX([c].[ContactName], [c].[ContactName]) > 0) OR ([c].[ContactName] = N'')");
        }

        public override void String_Contains_MethodCall()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains(LocalMethod1())), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains(LocalMethod1().ToLower()) || c.ContactName.Contains(LocalMethod1().ToUpper())), // case-sensitive
                entryCount: 34);

            AssertSql(
                @"@__LocalMethod1_0: M (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (CHARINDEX(@__LocalMethod1_0, [c].[ContactName]) > 0) OR (@__LocalMethod1_0 = N'')");
        }

        public override void String_Compare_simple_zero()
        {
            base.String_Compare_simple_zero();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <> N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= N'ALFKI'");
        }

        public override void String_Compare_simple_one()
        {
            base.String_Compare_simple_one();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] < N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] >= N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] >= N'ALFKI'");
        }

        public override void String_compare_with_parameter()
        {
            base.String_compare_with_parameter();

            AssertSql(
                @"@__customer_CustomerID_0: ALFKI (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > @__customer_CustomerID_0",
                //
                @"@__customer_CustomerID_0: ALFKI (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] < @__customer_CustomerID_0",
                //
                @"@__customer_CustomerID_0: ALFKI (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= @__customer_CustomerID_0",
                //
                @"@__customer_CustomerID_0: ALFKI (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= @__customer_CustomerID_0",
                //
                @"@__customer_CustomerID_0: ALFKI (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] >= @__customer_CustomerID_0",
                //
                @"@__customer_CustomerID_0: ALFKI (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] >= @__customer_CustomerID_0");
        }

        public override void String_Compare_simple_client()
        {
            base.String_Compare_simple_client();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void String_Compare_nested()
        {
            base.String_Compare_nested();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'M' + [c].[CustomerID]",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <> UPPER([c].[CustomerID])",
                //
                @"@__ToUpper_0: ALF (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > REPLACE(N'ALFKI', @__ToUpper_0, [c].[CustomerID])",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= N'M' + [c].[CustomerID]",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > UPPER([c].[CustomerID])",
                //
                @"@__ToUpper_0: ALF (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] < REPLACE(N'ALFKI', @__ToUpper_0, [c].[CustomerID])");
        }

        public override void String_Compare_multi_predicate()
        {
            base.String_Compare_multi_predicate();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] >= N'ALFKI' AND [c].[CustomerID] < N'CACTU'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactTitle] = N'Owner' AND [c].[Country] <> N'USA'");
        }

        public override void String_Compare_to_simple_zero()
        {
            base.String_Compare_to_simple_zero();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <> N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= N'ALFKI'");
        }

        public override void String_Compare_to_simple_one()
        {
            base.String_Compare_to_simple_one();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] < N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] >= N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] >= N'ALFKI'");
        }

        public override void String_compare_to_with_parameter()
        {
            base.String_compare_to_with_parameter();

            AssertSql(
                @"@__customer_CustomerID_0: ALFKI (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > @__customer_CustomerID_0",
                //
                @"@__customer_CustomerID_0: ALFKI (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] < @__customer_CustomerID_0",
                //
                @"@__customer_CustomerID_0: ALFKI (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= @__customer_CustomerID_0",
                //
                @"@__customer_CustomerID_0: ALFKI (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= @__customer_CustomerID_0",
                //
                @"@__customer_CustomerID_0: ALFKI (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] >= @__customer_CustomerID_0",
                //
                @"@__customer_CustomerID_0: ALFKI (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] >= @__customer_CustomerID_0");
        }

        public override void String_Compare_to_simple_client()
        {
            base.String_Compare_to_simple_client();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void String_Compare_to_nested()
        {
            base.String_Compare_to_nested();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'M' + [c].[CustomerID]",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <> UPPER([c].[CustomerID])",
                //
                @"@__ToUpper_0: ALF (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > REPLACE(N'ALFKI', @__ToUpper_0, [c].[CustomerID])",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= N'M' + [c].[CustomerID]",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > UPPER([c].[CustomerID])",
                //
                @"@__ToUpper_0: ALF (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] < REPLACE(N'ALFKI', @__ToUpper_0, [c].[CustomerID])");
        }

        public override void String_Compare_to_multi_predicate()
        {
            base.String_Compare_to_multi_predicate();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] >= N'ALFKI' AND [c].[CustomerID] < N'CACTU'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactTitle] = N'Owner' AND [c].[Country] <> N'USA'");
        }

        public override void Where_math_abs1()
        {
            base.Where_math_abs1();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE ABS([od].[ProductID]) > 10");
        }

        public override void Where_math_abs2()
        {
            base.Where_math_abs2();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE ABS([od].[Quantity]) > 10");
        }

        public override void Where_math_abs3()
        {
            base.Where_math_abs3();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE ABS([od].[UnitPrice]) > 10.0");
        }

        public override void Where_math_abs_uncorrelated()
        {
            base.Where_math_abs_uncorrelated();

            AssertSql(
                @"@__Abs_0: 10

SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE @__Abs_0 < [od].[ProductID]");
        }

        public override void Where_math_ceiling1()
        {
            base.Where_math_ceiling1();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE CEILING([od].[Discount]) > 0E0");
        }

        public override void Where_math_ceiling2()
        {
            base.Where_math_ceiling2();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE CEILING([od].[UnitPrice]) > 10.0");
        }

        public override void Where_math_floor()
        {
            base.Where_math_floor();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE FLOOR([od].[UnitPrice]) > 10.0");
        }

        public override void Where_query_composition4()
        {
            base.Where_query_composition4();

            AssertSql(
                @"@__p_0: 2

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [t]",
                //
                @"SELECT 1
FROM [Customers] AS [c0]
ORDER BY [c0].[CustomerID]",
                //
                @"SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
FROM [Customers] AS [c2]",
                //
                @"SELECT 1
FROM [Customers] AS [c0]
ORDER BY [c0].[CustomerID]",
                //
                @"SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
FROM [Customers] AS [c2]");
        }

        public override void Where_math_power()
        {
            base.Where_math_power();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE POWER([od].[Discount], 2E0) > 0.0500000007450581E0");
        }

        public override void Where_math_round()
        {
            base.Where_math_round();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE ROUND([od].[UnitPrice], 0) > 10.0");
        }

        public override void Select_math_round_int()
        {
            base.Select_math_round_int();

            AssertSql(
                @"SELECT ROUND(CAST([o].[OrderID] AS float), 0) AS [A]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10250");
        }

        public override void Select_math_truncate_int()
        {
            base.Select_math_truncate_int();

            AssertSql(
                @"SELECT ROUND(CAST([o].[OrderID] AS float), 0, 1) AS [A]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10250");
        }

        public override void Where_math_round2()
        {
            base.Where_math_round2();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE ROUND([od].[UnitPrice], 2) > 100.0");
        }

        public override void Where_math_truncate()
        {
            base.Where_math_truncate();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE ROUND([od].[UnitPrice], 0, 1) > 10.0");
        }

        public override void Where_math_exp()
        {
            base.Where_math_exp();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (EXP([od].[Discount]) > 1E0)");
        }

        public override void Where_math_log10()
        {
            base.Where_math_log10();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE (([od].[OrderID] = 11077) AND ([od].[Discount] > 0E0)) AND (LOG10([od].[Discount]) < 0E0)");
        }

        public override void Where_math_log()
        {
            base.Where_math_log();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE (([od].[OrderID] = 11077) AND ([od].[Discount] > 0E0)) AND (LOG([od].[Discount]) < 0E0)");
        }

        public override void Where_math_log_new_base()
        {
            base.Where_math_log_new_base();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE (([od].[OrderID] = 11077) AND ([od].[Discount] > 0E0)) AND (LOG([od].[Discount], 7E0) < 0E0)");
        }
        
        public override void Where_math_sqrt()
        {
            base.Where_math_sqrt();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (SQRT([od].[Discount]) > 0E0)");
        }

        public override void Where_math_acos()
        {
            base.Where_math_acos();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (ACOS([od].[Discount]) > 1E0)");
        }

        public override void Where_math_asin()
        {
            base.Where_math_asin();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (ASIN([od].[Discount]) > 0E0)");
        }

        public override void Where_math_atan()
        {
            base.Where_math_atan();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (ATAN([od].[Discount]) > 0E0)");
        }

        public override void Where_math_atan2()
        {
            base.Where_math_atan2();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (ATN2([od].[Discount], 1E0) > 0E0)");
        }

        public override void Where_math_cos()
        {
            base.Where_math_cos();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (COS([od].[Discount]) > 0E0)");
        }

        public override void Where_math_sin()
        {
            base.Where_math_sin();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (SIN([od].[Discount]) > 0E0)");
        }

        public override void Where_math_tan()
        {
            base.Where_math_tan();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (TAN([od].[Discount]) > 0E0)");
        }

        public override void Where_math_sign()
        {
            base.Where_math_sign();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (SIGN([od].[Discount]) > 0)");
        }

        public override void Where_guid_newguid()
        {
            base.Where_guid_newguid();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE NEWID() <> '00000000-0000-0000-0000-000000000000'");
        }

        public override void Where_functions_nested()
        {
            base.Where_functions_nested();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE POWER(CAST(LEN([c].[CustomerID]) AS int), 2E0) = 25E0");
        }

        public override void Where_string_to_lower()
        {
            base.Where_string_to_lower();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE LOWER([c].[CustomerID]) = N'alfki'");
        }

        public override void Where_string_to_upper()
        {
            base.Where_string_to_upper();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE UPPER([c].[CustomerID]) = N'ALFKI'");
        }

        public override void Convert_ToByte()
        {
            base.Convert_ToByte();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(tinyint, CONVERT(tinyint, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(tinyint, CONVERT(decimal(18, 2), [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(tinyint, CONVERT(float, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(tinyint, CONVERT(float, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(tinyint, CONVERT(smallint, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(tinyint, CONVERT(int, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(tinyint, CONVERT(bigint, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(tinyint, CONVERT(nvarchar(max), [o].[OrderID] % 1)) >= 0)");
        }

        public override void Convert_ToDecimal()
        {
            base.Convert_ToDecimal();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(decimal(18, 2), CONVERT(tinyint, [o].[OrderID] % 1)) >= 0.0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(decimal(18, 2), CONVERT(decimal(18, 2), [o].[OrderID] % 1)) >= 0.0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(decimal(18, 2), CONVERT(float, [o].[OrderID] % 1)) >= 0.0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(decimal(18, 2), CONVERT(float, [o].[OrderID] % 1)) >= 0.0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(decimal(18, 2), CONVERT(smallint, [o].[OrderID] % 1)) >= 0.0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(decimal(18, 2), CONVERT(int, [o].[OrderID] % 1)) >= 0.0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(decimal(18, 2), CONVERT(bigint, [o].[OrderID] % 1)) >= 0.0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(decimal(18, 2), CONVERT(nvarchar(max), [o].[OrderID] % 1)) >= 0.0)");
        }

        public override void Convert_ToDouble()
        {
            base.Convert_ToDouble();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(float, CONVERT(tinyint, [o].[OrderID] % 1)) >= 0E0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(float, CONVERT(decimal(18, 2), [o].[OrderID] % 1)) >= 0E0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(float, CONVERT(float, [o].[OrderID] % 1)) >= 0E0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(float, CONVERT(float, [o].[OrderID] % 1)) >= 0E0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(float, CONVERT(smallint, [o].[OrderID] % 1)) >= 0E0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(float, CONVERT(int, [o].[OrderID] % 1)) >= 0E0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(float, CONVERT(bigint, [o].[OrderID] % 1)) >= 0E0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(float, CONVERT(nvarchar(max), [o].[OrderID] % 1)) >= 0E0)");
        }

        public override void Convert_ToInt16()
        {
            base.Convert_ToInt16();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(smallint, CONVERT(tinyint, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(smallint, CONVERT(decimal(18, 2), [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(smallint, CONVERT(float, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(smallint, CONVERT(float, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(smallint, CONVERT(smallint, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(smallint, CONVERT(int, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(smallint, CONVERT(bigint, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(smallint, CONVERT(nvarchar(max), [o].[OrderID] % 1)) >= 0)");
        }

        public override void Convert_ToInt32()
        {
            base.Convert_ToInt32();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(int, CONVERT(tinyint, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(int, CONVERT(decimal(18, 2), [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(int, CONVERT(float, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(int, CONVERT(float, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(int, CONVERT(smallint, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(int, CONVERT(int, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(int, CONVERT(bigint, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(int, CONVERT(nvarchar(max), [o].[OrderID] % 1)) >= 0)");
        }

        public override void Convert_ToInt64()
        {
            base.Convert_ToInt64();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bigint, CONVERT(tinyint, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bigint, CONVERT(decimal(18, 2), [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bigint, CONVERT(float, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bigint, CONVERT(float, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bigint, CONVERT(smallint, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bigint, CONVERT(int, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bigint, CONVERT(bigint, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bigint, CONVERT(nvarchar(max), [o].[OrderID] % 1)) >= 0)");
        }

        public override void Convert_ToString()
        {
            base.Convert_ToString();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(nvarchar(max), CONVERT(tinyint, [o].[OrderID] % 1)) <> N'10')",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(nvarchar(max), CONVERT(decimal(18, 2), [o].[OrderID] % 1)) <> N'10')",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(nvarchar(max), CONVERT(float, [o].[OrderID] % 1)) <> N'10')",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(nvarchar(max), CONVERT(float, [o].[OrderID] % 1)) <> N'10')",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(nvarchar(max), CONVERT(smallint, [o].[OrderID] % 1)) <> N'10')",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(nvarchar(max), CONVERT(int, [o].[OrderID] % 1)) <> N'10')",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(nvarchar(max), CONVERT(bigint, [o].[OrderID] % 1)) <> N'10')",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(nvarchar(max), CONVERT(nvarchar(max), [o].[OrderID] % 1)) <> N'10')");
        }

        public override void Select_nested_collection()
        {
            base.Select_nested_collection();

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[CustomerID]",
                //
                @"@_outer_CustomerID: AROUT (Size = 450)

SELECT [o].[OrderID]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = @_outer_CustomerID) AND (DATEPART(year, [o].[OrderDate]) = 1997)
ORDER BY [o].[OrderID]",
                //
                @"@_outer_CustomerID: BSBEV (Size = 450)

SELECT [o].[OrderID]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = @_outer_CustomerID) AND (DATEPART(year, [o].[OrderDate]) = 1997)
ORDER BY [o].[OrderID]",
                //
                @"@_outer_CustomerID: CONSH (Size = 450)

SELECT [o].[OrderID]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = @_outer_CustomerID) AND (DATEPART(year, [o].[OrderDate]) = 1997)
ORDER BY [o].[OrderID]",
                //
                @"@_outer_CustomerID: EASTC (Size = 450)

SELECT [o].[OrderID]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = @_outer_CustomerID) AND (DATEPART(year, [o].[OrderDate]) = 1997)
ORDER BY [o].[OrderID]",
                //
                @"@_outer_CustomerID: NORTS (Size = 450)

SELECT [o].[OrderID]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = @_outer_CustomerID) AND (DATEPART(year, [o].[OrderDate]) = 1997)
ORDER BY [o].[OrderID]",
                //
                @"@_outer_CustomerID: SEVES (Size = 450)

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
                @"@_outer_CustomerID: ALFKI (Size = 450)

SELECT TOP(3) [o].[OrderDate] AS [Date]
FROM [Orders] AS [o]
WHERE ([o].[OrderID] < 10500) AND (@_outer_CustomerID = [o].[CustomerID])",
                //
                @"@_outer_CustomerID: ANATR (Size = 450)

SELECT TOP(3) [o].[OrderDate] AS [Date]
FROM [Orders] AS [o]
WHERE ([o].[OrderID] < 10500) AND (@_outer_CustomerID = [o].[CustomerID])",
                //
                @"@_outer_CustomerID: ANTON (Size = 450)

SELECT TOP(3) [o].[OrderDate] AS [Date]
FROM [Orders] AS [o]
WHERE ([o].[OrderID] < 10500) AND (@_outer_CustomerID = [o].[CustomerID])",
                //
                @"@_outer_CustomerID: AROUT (Size = 450)

SELECT TOP(3) [o].[OrderDate] AS [Date]
FROM [Orders] AS [o]
WHERE ([o].[OrderID] < 10500) AND (@_outer_CustomerID = [o].[CustomerID])");
        }

        public override void Select_nested_collection_multi_level2()
        {
            base.Select_nested_collection_multi_level2();

            AssertSql(" ");
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
                @"SELECT (
    SELECT TOP(1) (
        SELECT COUNT(*)
        FROM [Order Details] AS [od]
        WHERE ([od].[OrderID] > 10) AND ([o].[OrderID] = [od].[OrderID])
    )
    FROM [Orders] AS [o]
    WHERE ([o].[OrderID] < 10500) AND ([c].[CustomerID] = [o].[CustomerID])
) AS [Order]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')");
        }

        public override void Select_nested_collection_multi_level5()
        {
            base.Select_nested_collection_multi_level5();

            AssertSql(
                @"SELECT (
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
) AS [Order]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')");
        }

        public override void Select_nested_collection_multi_level6()
        {
            base.Select_nested_collection_multi_level6();

            AssertSql(
                @"SELECT (
    SELECT TOP(1) (
        SELECT TOP(1) [od].[ProductID]
        FROM [Order Details] AS [od]
        WHERE ([od].[OrderID] <> CAST(LEN([c].[CustomerID]) AS int)) AND ([o].[OrderID] = [od].[OrderID])
    )
    FROM [Orders] AS [o]
    WHERE ([o].[OrderID] < 10500) AND ([c].[CustomerID] = [o].[CustomerID])
) AS [Order]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')");
        }

        public override void Select_nested_collection_with_groupby()
        {
            base.Select_nested_collection_with_groupby();

            AssertSql(
                @"SELECT (
    SELECT CASE
        WHEN EXISTS (
            SELECT 1
            FROM [Orders] AS [o0]
            WHERE [c].[CustomerID] = [o0].[CustomerID])
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END
), [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')",
                //
                @"@_outer_CustomerID1: ALFKI (Size = 450)

SELECT [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate]
FROM [Orders] AS [o2]
WHERE @_outer_CustomerID1 = [o2].[CustomerID]
ORDER BY [o2].[OrderID]",
                //
                @"@_outer_CustomerID1: ANATR (Size = 450)

SELECT [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate]
FROM [Orders] AS [o2]
WHERE @_outer_CustomerID1 = [o2].[CustomerID]
ORDER BY [o2].[OrderID]",
                //
                @"@_outer_CustomerID1: ANTON (Size = 450)

SELECT [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate]
FROM [Orders] AS [o2]
WHERE @_outer_CustomerID1 = [o2].[CustomerID]
ORDER BY [o2].[OrderID]",
                //
                @"@_outer_CustomerID1: AROUT (Size = 450)

SELECT [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate]
FROM [Orders] AS [o2]
WHERE @_outer_CustomerID1 = [o2].[CustomerID]
ORDER BY [o2].[OrderID]");
        }

        public override void Select_nested_collection_count_using_anonymous_type()
        {
            base.Select_nested_collection_count_using_anonymous_type();

            AssertSql(
                @"SELECT (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [Count]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')");
        }

        public override void Select_nested_collection_count_using_DTO()
        {
            base.Select_nested_collection_count_using_DTO();

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Id], (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [Count]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')");
        }

        public override void Select_DTO_distinct_translated_to_server()
        {
            base.Select_DTO_distinct_translated_to_server();

            AssertSql(
                @"SELECT DISTINCT 1
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300");
        }

        public override void Select_DTO_constructor_distinct_translated_to_server()
        {
            base.Select_DTO_constructor_distinct_translated_to_server();

            AssertSql(
                @"SELECT DISTINCT [o].[CustomerID]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300");
        }

        public override void Select_DTO_with_member_init_distinct_translated_to_server()
        {
            base.Select_DTO_with_member_init_distinct_translated_to_server();

            AssertSql(
                @"SELECT DISTINCT [o].[CustomerID] AS [Id], [o].[OrderID] AS [Count]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300");
        }

        public override void Select_DTO_with_member_init_distinct_in_subquery_translated_to_server()
        {
            base.Select_DTO_with_member_init_distinct_in_subquery_translated_to_server();

            AssertSql(
                @"SELECT [t].[Id], [t].[Count], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT DISTINCT [o].[CustomerID] AS [Id], [o].[OrderID] AS [Count]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
) AS [t]
CROSS JOIN [Customers] AS [c]
WHERE [c].[CustomerID] = [t].[Id]");
        }

        public override void Select_DTO_with_member_init_distinct_in_subquery_used_in_projection_translated_to_server()
        {
            base.Select_DTO_with_member_init_distinct_in_subquery_used_in_projection_translated_to_server();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[Id], [t].[Count]
FROM [Customers] AS [c]
CROSS JOIN (
    SELECT DISTINCT [o].[CustomerID] AS [Id], [o].[OrderID] AS [Count]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
) AS [t]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')");
        }

        public override void Select_correlated_subquery_projection()
        {
            base.Select_correlated_subquery_projection();

            AssertSql(
                @"@__p_0: 3

SELECT [t].[CustomerID]
FROM (
    SELECT TOP(@__p_0) [c].*
    FROM [Customers] AS [c]
) AS [t]
ORDER BY [t].[CustomerID]",
                //
                @"@_outer_CustomerID: ALFKI (Size = 450)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = @_outer_CustomerID",
                //
                @"@_outer_CustomerID: ANATR (Size = 450)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = @_outer_CustomerID",
                //
                @"@_outer_CustomerID: ANTON (Size = 450)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = @_outer_CustomerID");
        }

        public override void Select_correlated_subquery_ordered()
        {
            base.Select_correlated_subquery_ordered();

            AssertSql(
                @"@__p_0: 3

SELECT TOP(@__p_0) [c].[CustomerID]
FROM [Customers] AS [c]",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override void Where_subquery_on_bool()
        {
            base.Where_subquery_on_bool();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE N'Chai' IN (
    SELECT [p2].[ProductName]
    FROM [Products] AS [p2]
)");
        }

        public override void Where_subquery_on_collection()
        {
            base.Where_subquery_on_collection();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE 5 IN (
    SELECT [o].[Quantity]
    FROM [Order Details] AS [o]
    WHERE [o].[ProductID] = [p].[ProductID]
)");
        }

        public override void Select_many_cross_join_same_collection()
        {
            base.Select_many_cross_join_same_collection();

            AssertSql(
                @"SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c]
CROSS JOIN [Customers] AS [c0]");
        }

        public override void Join_same_collection_multiple()
        {
            base.Join_same_collection_multiple();

            AssertSql(
                @"SELECT [c3].[CustomerID], [c3].[Address], [c3].[City], [c3].[CompanyName], [c3].[ContactName], [c3].[ContactTitle], [c3].[Country], [c3].[Fax], [c3].[Phone], [c3].[PostalCode], [c3].[Region]
FROM [Customers] AS [o]
INNER JOIN [Customers] AS [c2] ON [o].[CustomerID] = [c2].[CustomerID]
INNER JOIN [Customers] AS [c3] ON [o].[CustomerID] = [c3].[CustomerID]");
        }

        public override void Join_same_collection_force_alias_uniquefication()
        {
            base.Join_same_collection_force_alias_uniquefication();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[CustomerID] = [o0].[CustomerID]");
        }

        public override void Where_chain()
        {
            base.Where_chain();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'QUICK') AND ([o].[OrderDate] > '1998-01-01T00:00:00.000')");
        }

        public override void OfType_Select()
        {
            base.OfType_Select();

            AssertSql(
                @"SELECT TOP(1) [o.Customer].[City]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
ORDER BY [o].[OrderID]");
        }

        public override void OfType_Select_OfType_Select()
        {
            base.OfType_Select_OfType_Select();

            AssertSql(
                @"SELECT TOP(1) [o.Customer].[City]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
ORDER BY [o].[OrderID]");
        }

        public override void OrderBy_null_coalesce_operator()
        {
            base.OrderBy_null_coalesce_operator();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY COALESCE([c].[Region], N'ZZ')");
        }

        public override void Select_null_coalesce_operator()
        {
            base.Select_null_coalesce_operator();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[CompanyName], COALESCE([c].[Region], N'ZZ') AS [Region]
FROM [Customers] AS [c]
ORDER BY [Region]");
        }

        public override void OrderBy_conditional_operator()
        {
            base.OrderBy_conditional_operator();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY CASE
    WHEN [c].[Region] IS NULL
    THEN N'ZZ' ELSE [c].[Region]
END");
        }

        public override void OrderBy_conditional_operator_where_condition_null()
        {
            base.OrderBy_conditional_operator_where_condition_null();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY CASE
    WHEN 0 = 1
    THEN N'ZZ' ELSE [c].[City]
END");
        }

        public override void OrderBy_comparison_operator()
        {
            base.OrderBy_comparison_operator();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY CASE
    WHEN [c].[Region] = N'ASK'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void Contains_with_subquery()
        {
            base.Contains_with_subquery();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
)");
        }

        public override void Contains_with_local_array_closure()
        {
            base.Contains_with_local_array_closure();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI')",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE')");
        }

        public override void Contains_with_subquery_and_local_array_closure()
        {
            base.Contains_with_subquery_and_local_array_closure();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c1]
    WHERE [c1].[City] IN (N'London', N'Buenos Aires') AND ([c1].[CustomerID] = [c].[CustomerID]))",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c1]
    WHERE [c1].[City] IN (N'London') AND ([c1].[CustomerID] = [c].[CustomerID]))");
        }

        public override void Contains_with_local_int_array_closure()
        {
            base.Contains_with_local_int_array_closure();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] IN (0, 1)",
                //
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] IN (0)");
        }

        public override void Contains_with_local_nullable_int_array_closure()
        {
            base.Contains_with_local_nullable_int_array_closure();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] IN (0, 1)",
                //
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] IN (0)");
        }

        public override void Contains_with_local_array_inline()
        {
            base.Contains_with_local_array_inline();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI')");
        }

        public override void Contains_with_local_list_closure()
        {
            base.Contains_with_local_list_closure();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI')");
        }

        public override void Contains_with_local_list_inline()
        {
            base.Contains_with_local_list_inline();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI')");
        }

        public override void Contains_with_local_list_inline_closure_mix()
        {
            base.Contains_with_local_list_inline_closure_mix();

            AssertSql(
                @"@__id_0: ALFKI (Size = 450)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', @__id_0)",
                //
                @"@__id_0: ANATR (Size = 450)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', @__id_0)");
        }

        public override void Contains_with_local_collection_false()
        {
            base.Contains_with_local_collection_false();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] NOT IN (N'ABCDE', N'ALFKI')");
        }

        public override void Contains_with_local_collection_complex_predicate_and()
        {
            base.Contains_with_local_collection_complex_predicate_and();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ALFKI', N'ABCDE') AND [c].[CustomerID] IN (N'ABCDE', N'ALFKI')");
        }

        public override void Contains_with_local_collection_complex_predicate_or()
        {
            base.Contains_with_local_collection_complex_predicate_or();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI', N'ALFKI', N'ABCDE')");
        }

        public override void Contains_with_local_collection_complex_predicate_not_matching_ins1()
        {
            base.Contains_with_local_collection_complex_predicate_not_matching_ins1();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ALFKI', N'ABCDE') OR [c].[CustomerID] NOT IN (N'ABCDE', N'ALFKI')");
        }

        public override void Contains_with_local_collection_complex_predicate_not_matching_ins2()
        {
            base.Contains_with_local_collection_complex_predicate_not_matching_ins2();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI') AND [c].[CustomerID] NOT IN (N'ALFKI', N'ABCDE')");
        }

        public override void Contains_with_local_collection_sql_injection()
        {
            base.Contains_with_local_collection_sql_injection();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ALFKI', N'ABC'')); GO; DROP TABLE Orders; GO; --', N'ALFKI', N'ABCDE')");
        }

        public override void Contains_with_local_collection_empty_closure()
        {
            base.Contains_with_local_collection_empty_closure();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1");
        }

        public override void Contains_with_local_collection_empty_inline()
        {
            base.Contains_with_local_collection_empty_inline();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 1 = 1");
        }

        public override void Contains_top_level()
        {
            base.Contains_top_level();

            AssertSql(
                @"@__p_0: ALFKI (Size = 4000)

SELECT CASE
    WHEN @__p_0 IN (
        SELECT [c].[CustomerID]
        FROM [Customers] AS [c]
    )
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void Substring_with_constant()
        {
            base.Substring_with_constant();

            AssertSql(
                @"SELECT TOP(1) SUBSTRING([c].[ContactName], 2, 3)
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void Substring_with_closure()
        {
            base.Substring_with_closure();

            AssertSql(
                @"@__start_0: 2

SELECT TOP(1) SUBSTRING([c].[ContactName], @__start_0 + 1, 3)
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void Substring_with_client_eval()
        {
            base.Substring_with_client_eval();

            AssertSql(
                @"SELECT TOP(1) [c].[ContactName]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void IsNullOrEmpty_in_predicate()
        {
            base.IsNullOrEmpty_in_predicate();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[Region] IS NULL OR ([c].[Region] = N'')");
        }

        public override void IsNullOrEmpty_in_projection()
        {
            base.IsNullOrEmpty_in_projection();

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Id], CASE
    WHEN [c].[Region] IS NULL OR ([c].[Region] = N'')
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END AS [Value]
FROM [Customers] AS [c]");
        }

        public override void IsNullOrEmpty_negated_in_projection()
        {
            base.IsNullOrEmpty_negated_in_projection();

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Id], CASE
    WHEN [c].[Region] IS NOT NULL AND ([c].[Region] <> N'')
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END AS [Value]
FROM [Customers] AS [c]");
        }

        public override void IsNullOrWhiteSpace_in_predicate()
        {
            base.IsNullOrWhiteSpace_in_predicate();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[Region] IS NULL OR (LTRIM(RTRIM([c].[Region])) = N'')");
        }

        [FrameworkSkipCondition(RuntimeFrameworks.CoreCLR, SkipReason = "Failing after netcoreapp2.0 upgrade")]
        public override void TrimStart_in_predicate()
        {
            base.TrimStart_in_predicate();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE LTRIM([c].[ContactTitle]) = N'Owner'");
        }

        public override void TrimStart_with_arguments_in_predicate()
        {
            base.TrimStart_with_arguments_in_predicate();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        [FrameworkSkipCondition(RuntimeFrameworks.CoreCLR, SkipReason = "Failing after netcoreapp2.0 upgrade")]
        public override void TrimEnd_in_predicate()
        {
            base.TrimEnd_in_predicate();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE RTRIM([c].[ContactTitle]) = N'Owner'");
        }

        public override void TrimEnd_with_arguments_in_predicate()
        {
            base.TrimEnd_with_arguments_in_predicate();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Trim_in_predicate()
        {
            base.Trim_in_predicate();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE LTRIM(RTRIM([c].[ContactTitle])) = N'Owner'");
        }

        public override void Trim_with_arguments_in_predicate()
        {
            base.Trim_with_arguments_in_predicate();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Projection_null_coalesce_operator()
        {
            base.Projection_null_coalesce_operator();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[CompanyName], COALESCE([c].[Region], N'ZZ') AS [Region]
FROM [Customers] AS [c]");
        }

        public override void Filter_coalesce_operator()
        {
            base.Filter_coalesce_operator();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE COALESCE([c].[CompanyName], [c].[ContactName]) = N'The Big Cheese'");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Take_skip_null_coalesce_operator()
        {
            base.Take_skip_null_coalesce_operator();

            AssertSql(
                @"@__p_0: 10
@__p_1: 5

SELECT DISTINCT [t0].*
FROM (
    SELECT [t].*
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], COALESCE([c].[Region], N'ZZ') AS [c]
        FROM [Customers] AS [c]
        ORDER BY [c]
    ) AS [t]
    ORDER BY [t].[c]
    OFFSET @__p_1 ROWS
) AS [t0]");
        }

        public override void Select_take_null_coalesce_operator()
        {
            base.Select_take_null_coalesce_operator();

            AssertSql(
                @"@__p_0: 5

SELECT TOP(@__p_0) [c].[CustomerID], [c].[CompanyName], COALESCE([c].[Region], N'ZZ') AS [Region]
FROM [Customers] AS [c]
ORDER BY [Region]");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Select_take_skip_null_coalesce_operator()
        {
            base.Select_take_skip_null_coalesce_operator();

            AssertSql(
                @"@__p_0: 10
@__p_1: 5

SELECT [t].*
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[CompanyName], COALESCE([c].[Region], N'ZZ') AS [Region]
    FROM [Customers] AS [c]
    ORDER BY [Region]
) AS [t]
ORDER BY [t].[Region]
OFFSET @__p_1 ROWS");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Select_take_skip_null_coalesce_operator2()
        {
            base.Select_take_skip_null_coalesce_operator2();

            AssertSql(
                @"@__p_0: 10
@__p_1: 5

SELECT [t].*
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[CompanyName], [c].[Region], COALESCE([c].[Region], N'ZZ') AS [c]
    FROM [Customers] AS [c]
    ORDER BY [c]
) AS [t]
ORDER BY [t].[c]
OFFSET @__p_1 ROWS");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void Select_take_skip_null_coalesce_operator3()
        {
            base.Select_take_skip_null_coalesce_operator3();

            AssertSql(
                @"@__p_0: 10
@__p_1: 5

SELECT [t].*
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], COALESCE([c].[Region], N'ZZ') AS [c]
    FROM [Customers] AS [c]
    ORDER BY [c]
) AS [t]
ORDER BY [t].[c]
OFFSET @__p_1 ROWS");
        }

        public override void Selected_column_can_coalesce()
        {
            base.Selected_column_can_coalesce();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY COALESCE([c].[Region], N'ZZ')");
        }

        public override void Does_not_change_ordering_of_projection_with_complex_projections()
        {
            base.Does_not_change_ordering_of_projection_with_complex_projections();

            AssertSql(
                @"SELECT [e].[CustomerID] AS [Id], (
    SELECT COUNT(*)
    FROM [Orders] AS [o0]
    WHERE [e].[CustomerID] = [o0].[CustomerID]
) AS [TotalOrders]
FROM [Customers] AS [e]
WHERE ([e].[ContactTitle] = N'Owner') AND ((
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [e].[CustomerID] = [o].[CustomerID]
) > 2)
ORDER BY [Id]");
        }

        public override void DateTime_parse_is_parameterized()
        {
            base.DateTime_parse_is_parameterized();

            AssertSql(
                @"@__Parse_0: 01/01/1998 12:00:00

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] > @__Parse_0");
        }

        public override void Random_next_is_not_funcletized_1()
        {
            base.Random_next_is_not_funcletized_1();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override void Random_next_is_not_funcletized_2()
        {
            base.Random_next_is_not_funcletized_2();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override void Random_next_is_not_funcletized_3()
        {
            base.Random_next_is_not_funcletized_3();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override void Random_next_is_not_funcletized_4()
        {
            base.Random_next_is_not_funcletized_4();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override void Random_next_is_not_funcletized_5()
        {
            base.Random_next_is_not_funcletized_5();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override void Random_next_is_not_funcletized_6()
        {
            base.Random_next_is_not_funcletized_6();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override void Environment_newline_is_funcletized()
        {
            base.Environment_newline_is_funcletized();

            AssertSql(
                @"@__NewLine_0: 
 (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (CHARINDEX(@__NewLine_0, [c].[CustomerID]) > 0) OR (@__NewLine_0 = N'')");
        }

        public override void String_concat_with_navigation1()
        {
            base.String_concat_with_navigation1();

            AssertSql(
                @"SELECT ([o].[CustomerID] + N' ') + [o.Customer].[City]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]");
        }

        public override void String_concat_with_navigation2()
        {
            base.String_concat_with_navigation2();

            AssertSql(
                @"SELECT ([o.Customer].[City] + N' ') + [o.Customer].[City]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]");
        }

        public override void Where_bitwise_or()
        {
            base.Where_bitwise_or();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (CASE
    WHEN [c].[CustomerID] = N'ALFKI'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END | CASE
    WHEN [c].[CustomerID] = N'ANATR'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END) = 1");
        }

        public override void Where_bitwise_and()
        {
            base.Where_bitwise_and();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (CASE
    WHEN [c].[CustomerID] = N'ALFKI'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END & CASE
    WHEN [c].[CustomerID] = N'ANATR'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END) = 1");
        }

        public override void Select_bitwise_or()
        {
            base.Select_bitwise_or();

            AssertSql(
                @"SELECT [c].[CustomerID], CASE
    WHEN [c].[CustomerID] = N'ALFKI'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END | CASE
    WHEN [c].[CustomerID] = N'ANATR'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END AS [Value]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void Select_bitwise_or_multiple()
        {
            base.Select_bitwise_or_multiple();

            AssertSql(
                @"SELECT [c].[CustomerID], (CASE
    WHEN [c].[CustomerID] = N'ALFKI'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END | CASE
    WHEN [c].[CustomerID] = N'ANATR'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END) | CASE
    WHEN [c].[CustomerID] = N'ANTON'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END AS [Value]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void Select_bitwise_and()
        {
            base.Select_bitwise_and();

            AssertSql(
                @"SELECT [c].[CustomerID], CASE
    WHEN [c].[CustomerID] = N'ALFKI'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END & CASE
    WHEN [c].[CustomerID] = N'ANATR'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END AS [Value]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void Select_bitwise_and_or()
        {
            base.Select_bitwise_and_or();

            AssertSql(
                @"SELECT [c].[CustomerID], (CASE
    WHEN [c].[CustomerID] = N'ALFKI'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END & CASE
    WHEN [c].[CustomerID] = N'ANATR'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END) | CASE
    WHEN [c].[CustomerID] = N'ANTON'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END AS [Value]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void Where_bitwise_or_with_logical_or()
        {
            base.Where_bitwise_or_with_logical_or();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ((CASE
    WHEN [c].[CustomerID] = N'ALFKI'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END | CASE
    WHEN [c].[CustomerID] = N'ANATR'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END) = 1) OR ([c].[CustomerID] = N'ANTON')");
        }

        public override void Where_bitwise_and_with_logical_and()
        {
            base.Where_bitwise_and_with_logical_and();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ((CASE
    WHEN [c].[CustomerID] = N'ALFKI'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END & CASE
    WHEN [c].[CustomerID] = N'ANATR'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END) = 1) AND ([c].[CustomerID] = N'ANTON')");
        }

        public override void Where_bitwise_or_with_logical_and()
        {
            base.Where_bitwise_or_with_logical_and();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ((CASE
    WHEN [c].[CustomerID] = N'ALFKI'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END | CASE
    WHEN [c].[CustomerID] = N'ANATR'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END) = 1) AND ([c].[Country] = N'Germany')");
        }

        public override void Where_bitwise_and_with_logical_or()
        {
            base.Where_bitwise_and_with_logical_or();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ((CASE
    WHEN [c].[CustomerID] = N'ALFKI'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END & CASE
    WHEN [c].[CustomerID] = N'ANATR'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END) = 1) OR ([c].[CustomerID] = N'ANTON')");
        }

        public override void Select_bitwise_or_with_logical_or()
        {
            base.Select_bitwise_or_with_logical_or();

            AssertSql(
                @"SELECT [c].[CustomerID], CASE
    WHEN ((CASE
        WHEN [c].[CustomerID] = N'ALFKI'
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END | CASE
        WHEN [c].[CustomerID] = N'ANATR'
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END) = 1) OR ([c].[CustomerID] = N'ANTON')
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END AS [Value]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void Select_bitwise_and_with_logical_and()
        {
            base.Select_bitwise_and_with_logical_and();

            AssertSql(
                @"SELECT [c].[CustomerID], CASE
    WHEN ((CASE
        WHEN [c].[CustomerID] = N'ALFKI'
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END & CASE
        WHEN [c].[CustomerID] = N'ANATR'
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END) = 1) AND ([c].[CustomerID] = N'ANTON')
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END AS [Value]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void Handle_materialization_properly_when_more_than_two_query_sources_are_involved()
        {
            base.Handle_materialization_properly_when_more_than_two_query_sources_are_involved();

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
CROSS JOIN [Employees] AS [e]
ORDER BY [c].[CustomerID]");
        }

        public override void Parameter_extraction_short_circuits_1()
        {
            base.Parameter_extraction_short_circuits_1();

            AssertSql(
                @"@__dateFilter_Value_Month_0: 7
@__dateFilter_Value_Year_1: 1996

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[OrderID] < 10400) AND (([o].[OrderDate] IS NOT NULL AND (DATEPART(month, [o].[OrderDate]) = @__dateFilter_Value_Month_0)) AND (DATEPART(year, [o].[OrderDate]) = @__dateFilter_Value_Year_1))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10400");
        }

        public override void Parameter_extraction_short_circuits_2()
        {
            base.Parameter_extraction_short_circuits_2();

            AssertSql(
                @"@__dateFilter_Value_Month_0: 7
@__dateFilter_Value_Year_1: 1996

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[OrderID] < 10400) AND (([o].[OrderDate] IS NOT NULL AND (DATEPART(month, [o].[OrderDate]) = @__dateFilter_Value_Month_0)) AND (DATEPART(year, [o].[OrderDate]) = @__dateFilter_Value_Year_1))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE 0 = 1");
        }

        public override void Parameter_extraction_short_circuits_3()
        {
            base.Parameter_extraction_short_circuits_3();

            AssertSql(
                @"@__dateFilter_Value_Month_0: 7
@__dateFilter_Value_Year_1: 1996

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[OrderID] < 10400) OR (([o].[OrderDate] IS NOT NULL AND (DATEPART(month, [o].[OrderDate]) = @__dateFilter_Value_Month_0)) AND (DATEPART(year, [o].[OrderDate]) = @__dateFilter_Value_Year_1))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override void Subquery_member_pushdown_does_not_change_original_subquery_model()
        {
            base.Subquery_member_pushdown_does_not_change_original_subquery_model();

            AssertSql(
                @"@__p_0: 3

SELECT [t].[CustomerID], [t].[OrderID]
FROM (
    SELECT TOP(@__p_0) [o].*
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]",
                //
                @"@_outer_CustomerID: VINET (Size = 450)

SELECT TOP(2) [c0].[City]
FROM [Customers] AS [c0]
WHERE [c0].[CustomerID] = @_outer_CustomerID",
                //
                @"@_outer_CustomerID: TOMSP (Size = 450)

SELECT TOP(2) [c0].[City]
FROM [Customers] AS [c0]
WHERE [c0].[CustomerID] = @_outer_CustomerID",
                //
                @"@_outer_CustomerID: HANAR (Size = 450)

SELECT TOP(2) [c0].[City]
FROM [Customers] AS [c0]
WHERE [c0].[CustomerID] = @_outer_CustomerID",
                //
                @"@_outer_CustomerID1: TOMSP (Size = 450)

SELECT TOP(2) [c2].[City]
FROM [Customers] AS [c2]
WHERE [c2].[CustomerID] = @_outer_CustomerID1",
                //
                @"@_outer_CustomerID1: VINET (Size = 450)

SELECT TOP(2) [c2].[City]
FROM [Customers] AS [c2]
WHERE [c2].[CustomerID] = @_outer_CustomerID1",
                //
                @"@_outer_CustomerID1: HANAR (Size = 450)

SELECT TOP(2) [c2].[City]
FROM [Customers] AS [c2]
WHERE [c2].[CustomerID] = @_outer_CustomerID1");
        }

        public override void Query_expression_with_to_string_and_contains()
        {
            base.Query_expression_with_to_string_and_contains();

            AssertSql(
                @"SELECT [o].[CustomerID]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL AND (CHARINDEX(N'10', CONVERT(VARCHAR(11), [o].[EmployeeID])) > 0)");
        }

        public override void Select_expression_long_to_string()
        {
            base.Select_expression_long_to_string();

            AssertSql(
                @"SELECT CONVERT(VARCHAR(20), [o].[OrderID]) AS [ShipName]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL");
        }

        public override void Select_expression_int_to_string()
        {
            base.Select_expression_int_to_string();

            AssertSql(
                @"SELECT CONVERT(VARCHAR(11), [o].[OrderID]) AS [ShipName]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL");
        }

        public override void ToString_with_formatter_is_evaluated_on_the_client()
        {
            base.ToString_with_formatter_is_evaluated_on_the_client();

            AssertSql(
                @"SELECT [o].[OrderID]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL",
                //
                @"SELECT [o].[OrderID]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL");
        }

        public override void Select_expression_other_to_string()
        {
            base.Select_expression_other_to_string();

            AssertSql(
                @"SELECT CONVERT(VARCHAR(100), [o].[OrderDate]) AS [ShipName]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL");
        }

        public override void Select_expression_date_add_year()
        {
            base.Select_expression_date_add_year();

            AssertSql(
                @"SELECT DATEADD(year, 1, [o].[OrderDate]) AS [OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL");
        }

        public override void Select_expression_date_add_milliseconds_above_the_range()
        {
            base.Select_expression_date_add_milliseconds_above_the_range();

            AssertSql(
                @"SELECT [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL");
        }

        public override void Select_expression_date_add_milliseconds_below_the_range()
        {
            base.Select_expression_date_add_milliseconds_below_the_range();

            AssertSql(
                @"SELECT [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL");
        }

        public override void Select_expression_date_add_milliseconds_large_number_divided()
        {
            base.Select_expression_date_add_milliseconds_large_number_divided();

            AssertSql(
                @"@__millisecondsPerDay_1: 86400000
@__millisecondsPerDay_0: 86400000

SELECT DATEADD(millisecond, DATEPART(millisecond, [o].[OrderDate]) % @__millisecondsPerDay_1, DATEADD(day, DATEPART(millisecond, [o].[OrderDate]) / @__millisecondsPerDay_0, [o].[OrderDate])) AS [OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL");
        }

        public override void Select_expression_references_are_updated_correctly_with_subquery()
        {
            base.Select_expression_references_are_updated_correctly_with_subquery();

            AssertSql(
                @"@__nextYear_0: 2017

SELECT [t].[c]
FROM (
    SELECT DISTINCT DATEPART(year, [o].[OrderDate]) AS [c]
    FROM [Orders] AS [o]
    WHERE [o].[OrderDate] IS NOT NULL
) AS [t]
WHERE [t].[c] < @__nextYear_0");
        }

        public override void DefaultIfEmpty_without_group_join()
        {
            base.DefaultIfEmpty_without_group_join();

            AssertSql(
                @"SELECT [t0].[CustomerID]
FROM (
    SELECT [t].*
    FROM (
        SELECT NULL AS [empty]
    ) AS [empty]
    LEFT JOIN (
        SELECT [c].*
        FROM [Customers] AS [c]
        WHERE [c].[City] = N'London'
    ) AS [t] ON 1 = 1
) AS [t0]
WHERE [t0].[CustomerID] IS NOT NULL");
        }

        public override void DefaultIfEmpty_in_subquery()
        {
            base.DefaultIfEmpty_in_subquery();

            AssertSql(
                @"SELECT [c].[CustomerID], [t0].[OrderID]
FROM [Customers] AS [c]
CROSS APPLY (
    SELECT [t].*
    FROM (
        SELECT NULL AS [empty]
    ) AS [empty]
    LEFT JOIN (
        SELECT [o].*
        FROM [Orders] AS [o]
        WHERE [o].[CustomerID] = [c].[CustomerID]
    ) AS [t] ON 1 = 1
) AS [t0]
WHERE [t0].[OrderID] IS NOT NULL");
        }

        public override void DefaultIfEmpty_in_subquery_nested()
        {
            base.DefaultIfEmpty_in_subquery_nested();

            AssertSql(
                @"SELECT [c].[CustomerID], [t0].[OrderID], [t2].[OrderDate]
FROM [Customers] AS [c]
CROSS APPLY (
    SELECT [t].*
    FROM (
        SELECT NULL AS [empty]
    ) AS [empty]
    LEFT JOIN (
        SELECT [o].*
        FROM [Orders] AS [o]
        WHERE [o].[OrderID] > 11000
    ) AS [t] ON 1 = 1
) AS [t0]
CROSS APPLY (
    SELECT [t1].*
    FROM (
        SELECT NULL AS [empty]
    ) AS [empty0]
    LEFT JOIN (
        SELECT [o0].*
        FROM [Orders] AS [o0]
        WHERE [o0].[CustomerID] = [c].[CustomerID]
    ) AS [t1] ON 1 = 1
) AS [t2]
WHERE ([c].[City] = N'Seattle') AND ([t0].[OrderID] IS NOT NULL AND [t2].[OrderID] IS NOT NULL)");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void OrderBy_skip_take()
        {
            base.OrderBy_skip_take();

            AssertSql(
                @"@__p_0: 5
@__p_1: 8

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactTitle], [c].[ContactName]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void OrderBy_skip_take_take()
        {
            base.OrderBy_skip_take_take();

            AssertSql(
                @"@__p_2: 3
@__p_0: 5
@__p_1: 8

SELECT TOP(@__p_2) [t].*
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactTitle], [c].[ContactName]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
ORDER BY [t].[ContactTitle], [t].[ContactName]");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void OrderBy_skip_take_take_take_take()
        {
            base.OrderBy_skip_take_take_take_take();

            AssertSql(
                @"@__p_4: 5
@__p_3: 8
@__p_2: 10
@__p_0: 5
@__p_1: 15

SELECT TOP(@__p_4) [t1].*
FROM (
    SELECT TOP(@__p_3) [t0].*
    FROM (
        SELECT TOP(@__p_2) [t].*
        FROM (
            SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
            FROM [Customers] AS [c]
            ORDER BY [c].[ContactTitle], [c].[ContactName]
            OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
        ) AS [t]
        ORDER BY [t].[ContactTitle], [t].[ContactName]
    ) AS [t0]
    ORDER BY [t0].[ContactTitle], [t0].[ContactName]
) AS [t1]
ORDER BY [t1].[ContactTitle], [t1].[ContactName]");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void OrderBy_skip_take_skip_take_skip()
        {
            base.OrderBy_skip_take_skip_take_skip();

            AssertSql(
                @"@__p_0: 5
@__p_1: 15
@__p_2: 2
@__p_3: 8
@__p_4: 5

SELECT [t0].*
FROM (
    SELECT [t].*
    FROM (
        SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactTitle], [c].[ContactName]
        OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
    ) AS [t]
    ORDER BY [t].[ContactTitle], [t].[ContactName]
    OFFSET @__p_2 ROWS FETCH NEXT @__p_3 ROWS ONLY
) AS [t0]
ORDER BY [t0].[ContactTitle], [t0].[ContactName]
OFFSET @__p_4 ROWS");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void OrderBy_skip_take_distinct()
        {
            base.OrderBy_skip_take_distinct();

            AssertSql(
                @"@__p_0: 5
@__p_1: 15

SELECT DISTINCT [t].*
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactTitle], [c].[ContactName]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void OrderBy_coalesce_take_distinct()
        {
            base.OrderBy_coalesce_take_distinct();

            AssertSql(
                @"@__p_0: 15

SELECT DISTINCT [t].*
FROM (
    SELECT TOP(@__p_0) [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
    FROM [Products] AS [p]
    ORDER BY COALESCE([p].[UnitPrice], 0.0)
) AS [t]");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void OrderBy_coalesce_skip_take_distinct()
        {
            base.OrderBy_coalesce_skip_take_distinct();

            AssertSql(
                @"@__p_0: 5
@__p_1: 15

SELECT DISTINCT [t].*
FROM (
    SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
    FROM [Products] AS [p]
    ORDER BY COALESCE([p].[UnitPrice], 0.0)
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]");

        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void OrderBy_coalesce_skip_take_distinct_take()
        {
            base.OrderBy_coalesce_skip_take_distinct_take();

            AssertSql(
                @"@__p_2: 5
@__p_0: 5
@__p_1: 15

SELECT DISTINCT TOP(@__p_2) [t].*
FROM (
    SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
    FROM [Products] AS [p]
    ORDER BY COALESCE([p].[UnitPrice], 0.0)
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override void OrderBy_skip_take_distinct_orderby_take()
        {
            base.OrderBy_skip_take_distinct_orderby_take();

            AssertSql(
                @"@__p_2: 8
@__p_0: 5
@__p_1: 15

SELECT TOP(@__p_2) [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT DISTINCT [t].*
    FROM (
        SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactTitle], [c].[ContactName]
        OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
    ) AS [t]
) AS [t0]
ORDER BY [t0].[ContactTitle]");
        }

        public override void No_orderby_added_for_fully_translated_manually_constructed_LOJ()
        {
            base.No_orderby_added_for_fully_translated_manually_constructed_LOJ();

            AssertSql(
                @"SELECT [e1].[City] AS [City1], [e2].[City] AS [City2]
FROM [Employees] AS [e1]
LEFT JOIN [Employees] AS [e2] ON [e1].[EmployeeID] = [e2].[ReportsTo]");
        }

        public override void No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ()
        {
            base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID] AS [Id1], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]");
        }

        public override void No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition1()
        {
            base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition1();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID] AS [Id1], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON ([o].[CustomerID] = [c].[CustomerID]) AND ([o].[OrderID] = 10000)");
        }

        public override void No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2()
        {
            base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID] AS [Id1], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON ([o].[OrderID] = 10000) AND ([o].[CustomerID] = [c].[CustomerID])");
        }

        public override void Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ()
        {
            base.Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ();

            AssertSql(
                @"SELECT [e1].[EmployeeID], [e1].[City] AS [City1], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title], [e2].[EmployeeID], [e2].[City], [e2].[Country], [e2].[FirstName], [e2].[ReportsTo], [e2].[Title]
FROM [Employees] AS [e1]
LEFT JOIN [Employees] AS [e2] ON [e1].[EmployeeID] = [e2].[ReportsTo]
ORDER BY [e1].[EmployeeID]");
        }

        public override void Contains_with_DateTime_Date()
        {
            base.Contains_with_DateTime_Date();

            AssertSql(
                @"SELECT [e].[OrderID], [e].[CustomerID], [e].[EmployeeID], [e].[OrderDate]
FROM [Orders] AS [e]
WHERE CONVERT(date, [e].[OrderDate]) IN ('1996-07-04T00:00:00.000', '1996-07-16T00:00:00.000')",
                //
                @"SELECT [e].[OrderID], [e].[CustomerID], [e].[EmployeeID], [e].[OrderDate]
FROM [Orders] AS [e]
WHERE CONVERT(date, [e].[OrderDate]) IN ('1996-07-04T00:00:00.000')");
        }

        public override void Contains_with_subquery_involving_join_binds_to_correct_table()
        {
            base.Contains_with_subquery_involving_join_binds_to_correct_table();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[OrderID] > 11000) AND [o].[OrderID] IN (
    SELECT [od].[OrderID]
    FROM [Order Details] AS [od]
    INNER JOIN [Products] AS [od.Product] ON [od].[ProductID] = [od.Product].[ProductID]
    WHERE [od.Product].[ProductName] = N'Chai'
)");
        }

        public override void GroupJoin_SelectMany_subquery_with_filter()
        {
            base.GroupJoin_SelectMany_subquery_with_filter();

            AssertSql(
                @"SELECT [c].[ContactName], [t].[OrderID]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [o].*
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] > 5
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]");
        }

        public override void GroupJoin_SelectMany_subquery_with_filter_orderby()
        {
            base.GroupJoin_SelectMany_subquery_with_filter_orderby();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID]");
        }

        public override void GroupJoin_SelectMany_subquery_with_filter_and_DefaultIfEmpty()
        {
            base.GroupJoin_SelectMany_subquery_with_filter_and_DefaultIfEmpty();

            AssertSql(
                @"SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], [c].[ContactName]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] > 5
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]");
        }

        public override void GroupJoin_SelectMany_subquery_with_filter_orderby_and_DefaultIfEmpty()
        {
            base.GroupJoin_SelectMany_subquery_with_filter_orderby_and_DefaultIfEmpty();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID]");
        }

        public override void Complex_query_with_repeated_query_model_compiles_correctly()
        {
            base.Complex_query_with_repeated_query_model_compiles_correctly();

            AssertSql(
                @"SELECT [outer].[CustomerID], [outer].[Address], [outer].[City], [outer].[CompanyName], [outer].[ContactName], [outer].[ContactTitle], [outer].[Country], [outer].[Fax], [outer].[Phone], [outer].[PostalCode], [outer].[Region]
FROM [Customers] AS [outer]
WHERE [outer].[CustomerID] = N'ALFKI'",
                //
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c0]
        WHERE EXISTS (
            SELECT 1
            FROM [Customers] AS [cc1]))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void Complex_query_with_repeated_nested_query_model_compiles_correctly()
        {
            base.Complex_query_with_repeated_nested_query_model_compiles_correctly();

            AssertSql(
                @"SELECT [outer].[CustomerID], [outer].[Address], [outer].[City], [outer].[CompanyName], [outer].[ContactName], [outer].[ContactTitle], [outer].[Country], [outer].[Fax], [outer].[Phone], [outer].[PostalCode], [outer].[Region]
FROM [Customers] AS [outer]
WHERE [outer].[CustomerID] = N'ALFKI'",
                //
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c0]
        WHERE EXISTS (
            SELECT 1
            FROM [Customers] AS [cc1]
            WHERE EXISTS (
                SELECT DISTINCT 1
                FROM (
                    SELECT TOP(10) [inner1].*
                    FROM [Customers] AS [inner1]
                    ORDER BY [inner1].[CustomerID]
                ) AS [t1])))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void Anonymous_member_distinct_where()
        {
            base.Anonymous_member_distinct_where();

            AssertSql(
                @"SELECT [t].[CustomerID]
FROM (
    SELECT DISTINCT [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [t]
WHERE [t].[CustomerID] = N'ALFKI'");
        }

        public override void Anonymous_member_distinct_orderby()
        {
            base.Anonymous_member_distinct_orderby();

            AssertSql(
                @"SELECT [t].[CustomerID]
FROM (
    SELECT DISTINCT [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [t]
ORDER BY [t].[CustomerID]");
        }

        public override void Anonymous_member_distinct_result()
        {
            base.Anonymous_member_distinct_result();

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [t]
WHERE [t].[CustomerID] LIKE N'A' + N'%' AND (LEFT([t].[CustomerID], LEN(N'A')) = N'A')");
        }

        public override void Anonymous_complex_distinct_where()
        {
            base.Anonymous_complex_distinct_where();

            AssertSql(
                @"SELECT [t].[A]
FROM (
    SELECT DISTINCT [c].[CustomerID] + [c].[City] AS [A]
    FROM [Customers] AS [c]
) AS [t]
WHERE [t].[A] = N'ALFKIBerlin'");
        }

        public override void Anonymous_complex_distinct_orderby()
        {
            base.Anonymous_complex_distinct_orderby();

            AssertSql(
                @"SELECT [t].[A]
FROM (
    SELECT DISTINCT [c].[CustomerID] + [c].[City] AS [A]
    FROM [Customers] AS [c]
) AS [t]
ORDER BY [t].[A]");
        }

        public override void Anonymous_complex_distinct_result()
        {
            base.Anonymous_complex_distinct_result();

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[CustomerID] + [c].[City] AS [A]
    FROM [Customers] AS [c]
) AS [t]
WHERE [t].[A] LIKE N'A' + N'%' AND (LEFT([t].[A], LEN(N'A')) = N'A')");
        }

        public override void Anonymous_complex_orderby()
        {
            base.Anonymous_complex_orderby();

            AssertSql(
                @"SELECT [c].[CustomerID] + [c].[City] AS [A]
FROM [Customers] AS [c]
ORDER BY [A]");
        }

        public override void Anonymous_subquery_orderby()
        {
            base.Anonymous_subquery_orderby();

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [o1].[OrderDate]
    FROM [Orders] AS [o1]
    WHERE [c].[CustomerID] = [o1].[CustomerID]
    ORDER BY [o1].[OrderID] DESC
) AS [A]
FROM [Customers] AS [c]
WHERE (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) > 1
ORDER BY (
    SELECT TOP(1) [o0].[OrderDate]
    FROM [Orders] AS [o0]
    WHERE [c].[CustomerID] = [o0].[CustomerID]
    ORDER BY [o0].[OrderID] DESC
)");
        }

        public override void DTO_member_distinct_where()
        {
            base.DTO_member_distinct_where();

            AssertSql(
                @"SELECT [t].[Property]
FROM (
    SELECT DISTINCT [c].[CustomerID] AS [Property]
    FROM [Customers] AS [c]
) AS [t]
WHERE [t].[Property] = N'ALFKI'");
        }

        public override void DTO_member_distinct_orderby()
        {
            base.DTO_member_distinct_orderby();

            AssertSql(
                @"SELECT [t].[Property]
FROM (
    SELECT DISTINCT [c].[CustomerID] AS [Property]
    FROM [Customers] AS [c]
) AS [t]
ORDER BY [t].[Property]");
        }

        public override void DTO_member_distinct_result()
        {
            base.DTO_member_distinct_result();

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[CustomerID] AS [Property]
    FROM [Customers] AS [c]
) AS [t]
WHERE [t].[Property] LIKE N'A' + N'%' AND (LEFT([t].[Property], LEN(N'A')) = N'A')");
        }

        public override void DTO_complex_distinct_where()
        {
            base.DTO_complex_distinct_where();

            AssertSql(
                @"SELECT [t].[Property]
FROM (
    SELECT DISTINCT [c].[CustomerID] + [c].[City] AS [Property]
    FROM [Customers] AS [c]
) AS [t]
WHERE [t].[Property] = N'ALFKIBerlin'");
        }

        public override void DTO_complex_distinct_orderby()
        {
            base.DTO_complex_distinct_orderby();

            AssertSql(
                @"SELECT [t].[Property]
FROM (
    SELECT DISTINCT [c].[CustomerID] + [c].[City] AS [Property]
    FROM [Customers] AS [c]
) AS [t]
ORDER BY [t].[Property]");
        }

        public override void DTO_complex_distinct_result()
        {
            base.DTO_complex_distinct_result();

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[CustomerID] + [c].[City] AS [Property]
    FROM [Customers] AS [c]
) AS [t]
WHERE [t].[Property] LIKE N'A' + N'%' AND (LEFT([t].[Property], LEN(N'A')) = N'A')");
        }

        public override void DTO_complex_orderby()
        {
            base.DTO_complex_orderby();

            AssertSql(
                @"SELECT [c].[CustomerID] + [c].[City] AS [Property]
FROM [Customers] AS [c]
ORDER BY [Property]");
        }

        public override void DTO_subquery_orderby()
        {
            base.DTO_subquery_orderby();

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [o1].[OrderDate]
    FROM [Orders] AS [o1]
    WHERE [c].[CustomerID] = [o1].[CustomerID]
    ORDER BY [o1].[OrderID] DESC
) AS [Property]
FROM [Customers] AS [c]
WHERE (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) > 1
ORDER BY (
    SELECT TOP(1) [o0].[OrderDate]
    FROM [Orders] AS [o0]
    WHERE [c].[CustomerID] = [o0].[CustomerID]
    ORDER BY [o0].[OrderID] DESC
)");
        }

        public override void Include_with_orderby_skip_preserves_ordering()
        {
            base.Include_with_orderby_skip_preserves_ordering();

            AssertSql(
                @"@__p_0: 40
@__p_1: 5

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <> N'VAFFE'
ORDER BY [c].[City], [c].[CustomerID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY",
                //
                @"@__p_0: 40
@__p_1: 5

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID], [c0].[City]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] <> N'VAFFE'
    ORDER BY [c0].[City], [c0].[CustomerID]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[City], [t].[CustomerID]");
        }

        public override void GroupBy_join_anonymous()
        {
            base.GroupBy_join_anonymous();

            AssertSql(
                @"SELECT [order0].[OrderID], [order0].[CustomerID], [order0].[EmployeeID], [order0].[OrderDate], [orderDetail0].[OrderID], [orderDetail0].[ProductID], [orderDetail0].[Discount], [orderDetail0].[Quantity], [orderDetail0].[UnitPrice]
FROM [Orders] AS [order0]
LEFT JOIN [Order Details] AS [orderDetail0] ON [order0].[OrderID] = [orderDetail0].[OrderID]
ORDER BY [order0].[OrderID]");
        }

        public override void Int16_parameter_can_be_used_for_int_column()
        {
            base.Int16_parameter_can_be_used_for_int_column();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] = 10300");
        }

        public override void Subquery_is_null_translated_correctly()
        {
            base.Subquery_is_null_translated_correctly();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT TOP(1) [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID] DESC
) IS NULL");
        }

        public override void Subquery_is_not_null_translated_correctly()
        {
            base.Subquery_is_not_null_translated_correctly();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT TOP(1) [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID] DESC
) IS NOT NULL");
        }

        public override void Select_take_average()
        {
            base.Select_take_average();

            AssertSql(
                @"@__p_0: 10

SELECT AVG(CAST([t].[OrderID] AS float))
FROM (
    SELECT TOP(@__p_0) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]");
        }

        public override void Select_take_count()
        {
            base.Select_take_count();

            AssertSql(
                @"@__p_0: 7

SELECT COUNT(*)
FROM (
    SELECT TOP(@__p_0) [c].*
    FROM [Customers] AS [c]
) AS [t]");
        }

        public override void Select_orderBy_take_count()
        {
            base.Select_orderBy_take_count();

            AssertSql(
                @"@__p_0: 7

SELECT COUNT(*)
FROM (
    SELECT TOP(@__p_0) [c].*
    FROM [Customers] AS [c]
    ORDER BY [c].[Country]
) AS [t]");
        }

        public override void Select_take_long_count()
        {
            base.Select_take_long_count();

            AssertSql(
                @"@__p_0: 7

SELECT COUNT_BIG(*)
FROM (
    SELECT TOP(@__p_0) [c].*
    FROM [Customers] AS [c]
) AS [t]");
        }

        public override void Select_orderBy_take_long_count()
        {
            base.Select_orderBy_take_long_count();

            AssertSql(
                @"@__p_0: 7

SELECT COUNT_BIG(*)
FROM (
    SELECT TOP(@__p_0) [c].*
    FROM [Customers] AS [c]
    ORDER BY [c].[Country]
) AS [t]");
        }

        public override void Select_take_max()
        {
            base.Select_take_max();

            AssertSql(
                @"@__p_0: 10

SELECT MAX([t].[OrderID])
FROM (
    SELECT TOP(@__p_0) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]");
        }

        public override void Select_take_min()
        {
            base.Select_take_min();

            AssertSql(
                @"@__p_0: 10

SELECT MIN([t].[OrderID])
FROM (
    SELECT TOP(@__p_0) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]");
        }

        public override void Select_take_sum()
        {
            base.Select_take_sum();

            AssertSql(
                @"@__p_0: 10

SELECT SUM([t].[OrderID])
FROM (
    SELECT TOP(@__p_0) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]");
        }

        public override void Select_skip_average()
        {
            base.Select_skip_average();

            AssertSql(
                @"@__p_0: 10

SELECT AVG(CAST([t].[OrderID] AS float))
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS
) AS [t]");
        }

        public override void Select_skip_count()
        {
            base.Select_skip_count();

            AssertSql(
                @"@__p_0: 7

SELECT COUNT(*)
FROM (
    SELECT [c].*
    FROM [Customers] AS [c]
    ORDER BY (SELECT 1)
    OFFSET @__p_0 ROWS
) AS [t]");
        }

        public override void Select_orderBy_skip_count()
        {
            base.Select_orderBy_skip_count();

            AssertSql(
                @"@__p_0: 7

SELECT COUNT(*)
FROM (
    SELECT [c].*
    FROM [Customers] AS [c]
    ORDER BY [c].[Country]
    OFFSET @__p_0 ROWS
) AS [t]");
        }

        public override void Select_skip_long_count()
        {
            base.Select_skip_long_count();

            AssertSql(
                @"@__p_0: 7

SELECT COUNT_BIG(*)
FROM (
    SELECT [c].*
    FROM [Customers] AS [c]
    ORDER BY (SELECT 1)
    OFFSET @__p_0 ROWS
) AS [t]");
        }

        public override void Select_orderBy_skip_long_count()
        {
            base.Select_orderBy_skip_long_count();

            AssertSql(
                @"@__p_0: 7

SELECT COUNT_BIG(*)
FROM (
    SELECT [c].*
    FROM [Customers] AS [c]
    ORDER BY [c].[Country]
    OFFSET @__p_0 ROWS
) AS [t]");
        }

        public override void Select_skip_max()
        {
            base.Select_skip_max();

            AssertSql(
                @"@__p_0: 10

SELECT MAX([t].[OrderID])
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS
) AS [t]");
        }

        public override void Select_skip_min()
        {
            base.Select_skip_min();

            AssertSql(
                @"@__p_0: 10

SELECT MIN([t].[OrderID])
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS
) AS [t]");
        }

        public override void Select_skip_sum()
        {
            base.Select_skip_sum();

            AssertSql(
                @"@__p_0: 10

SELECT SUM([t].[OrderID])
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS
) AS [t]");
        }

        public override void Select_distinct_average()
        {
            base.Select_distinct_average();

            AssertSql(
                @"SELECT AVG(CAST([t].[OrderID] AS float))
FROM (
    SELECT DISTINCT [o].[OrderID]
    FROM [Orders] AS [o]
) AS [t]");
        }

        public override void Select_distinct_count()
        {
            base.Select_distinct_count();

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].*
    FROM [Customers] AS [c]
) AS [t]");
        }

        public override void Select_distinct_long_count()
        {
            base.Select_distinct_long_count();

            AssertSql(
                @"SELECT COUNT_BIG(*)
FROM (
    SELECT DISTINCT [c].*
    FROM [Customers] AS [c]
) AS [t]");
        }

        public override void Select_distinct_max()
        {
            base.Select_distinct_max();

            AssertSql(
                @"SELECT MAX([t].[OrderID])
FROM (
    SELECT DISTINCT [o].[OrderID]
    FROM [Orders] AS [o]
) AS [t]");
        }

        public override void Select_distinct_min()
        {
            base.Select_distinct_min();

            AssertSql(
                @"SELECT MIN([t].[OrderID])
FROM (
    SELECT DISTINCT [o].[OrderID]
    FROM [Orders] AS [o]
) AS [t]");
        }

        public override void Select_distinct_sum()
        {
            base.Select_distinct_sum();

            AssertSql(
                @"SELECT SUM([t].[OrderID])
FROM (
    SELECT DISTINCT [o].[OrderID]
    FROM [Orders] AS [o]
) AS [t]");
        }
        public override void Comparing_entities_using_Equals()
        {
            base.Comparing_entities_using_Equals();

            AssertSql(
                @"SELECT [c1].[CustomerID] AS [Id1], [c2].[CustomerID] AS [Id2]
FROM [Customers] AS [c1]
CROSS JOIN [Customers] AS [c2]
WHERE ([c1].[CustomerID] LIKE N'ALFKI' + N'%' AND (LEFT([c1].[CustomerID], LEN(N'ALFKI')) = N'ALFKI')) AND ([c1].[CustomerID] = [c2].[CustomerID])
ORDER BY [Id1]");
        }

        public override void Comparing_different_entity_types_using_Equals()
        {
            base.Comparing_different_entity_types_using_Equals();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
WHERE ([c].[CustomerID] = N' ALFKI') AND ([o].[CustomerID] = N'ALFKI')");
        }

        public override void Comparing_entity_to_null_using_Equals()
        {
            base.Comparing_entity_to_null_using_Equals();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')
ORDER BY [c].[CustomerID]");
        }

        public override void Comparing_navigations_using_Equals()
        {
            base.Comparing_navigations_using_Equals();

            AssertSql(
                @"SELECT [o1].[OrderID] AS [Id1], [o2].[OrderID] AS [Id2]
FROM [Orders] AS [o1]
CROSS JOIN [Orders] AS [o2]
WHERE ([o1].[CustomerID] LIKE N'A' + N'%' AND (LEFT([o1].[CustomerID], LEN(N'A')) = N'A')) AND (([o1].[CustomerID] = [o2].[CustomerID]) OR ([o1].[CustomerID] IS NULL AND [o2].[CustomerID] IS NULL))
ORDER BY [Id1], [Id2]");
        }

        public override void Comparing_navigations_using_static_Equals()
        {
            base.Comparing_navigations_using_static_Equals();

            AssertSql(
                @"SELECT [o1].[OrderID] AS [Id1], [o2].[OrderID] AS [Id2]
FROM [Orders] AS [o1]
CROSS JOIN [Orders] AS [o2]
WHERE ([o1].[CustomerID] LIKE N'A' + N'%' AND (LEFT([o1].[CustomerID], LEN(N'A')) = N'A')) AND (([o1].[CustomerID] = [o2].[CustomerID]) OR ([o1].[CustomerID] IS NULL AND [o2].[CustomerID] IS NULL))
ORDER BY [Id1], [Id2]");
        }

        public override void Comparing_non_matching_entities_using_Equals()
        {
            base.Comparing_non_matching_entities_using_Equals();

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Id1], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID] AS [Id2], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override void Comparing_non_matching_collection_navigations_using_Equals()
        {
            base.Comparing_non_matching_collection_navigations_using_Equals();

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Id1], [o].[OrderID] AS [Id2]
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
WHERE 0 = 1");
        }

        public override void Comparing_collection_navigation_to_null()
        {
            base.Comparing_collection_navigation_to_null();

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IS NULL");
        }

        public override void Comparing_collection_navigation_to_null_complex()
        {
            base.Comparing_collection_navigation_to_null_complex();

            AssertSql(
                @"SELECT [od].[ProductID], [od].[OrderID]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [od.Order] ON [od].[OrderID] = [od.Order].[OrderID]
WHERE ([od].[OrderID] < 10250) AND [od.Order].[CustomerID] IS NOT NULL
ORDER BY [od].[OrderID], [od].[ProductID]");
        }

        public override void Compare_collection_navigation_with_itself()
        {
            base.Compare_collection_navigation_with_itself();

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')) AND ([c].[CustomerID] = [c].[CustomerID])");
        }

        public override void Compare_two_collection_navigations_with_different_query_sources()
        {
            base.Compare_two_collection_navigations_with_different_query_sources();

            AssertSql(
                @"SELECT [c1].[CustomerID] AS [Id1], [c2].[CustomerID] AS [Id2]
FROM [Customers] AS [c1]
CROSS JOIN [Customers] AS [c2]
WHERE (([c1].[CustomerID] = N'ALFKI') AND ([c2].[CustomerID] = N'ALFKI')) AND ([c1].[CustomerID] = [c2].[CustomerID])");
        }

        public override void Compare_two_collection_navigations_using_equals()
        {
            base.Compare_two_collection_navigations_using_equals();

            AssertSql(
                @"SELECT [c1].[CustomerID] AS [Id1], [c2].[CustomerID] AS [Id2]
FROM [Customers] AS [c1]
CROSS JOIN [Customers] AS [c2]
WHERE (([c1].[CustomerID] = N'ALFKI') AND ([c2].[CustomerID] = N'ALFKI')) AND ([c1].[CustomerID] = [c2].[CustomerID])");
        }

        public override void Compare_two_collection_navigations_with_different_property_chains()
        {
            base.Compare_two_collection_navigations_with_different_property_chains();

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Id1], [o].[OrderID] AS [Id2]
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
WHERE ([c].[CustomerID] = N'ALFKI') AND ([c].[CustomerID] = [o].[CustomerID])
ORDER BY [Id1], [Id2]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        private void AssertContainsSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, assertOrder: false);

        protected override void ClearLog() 
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
