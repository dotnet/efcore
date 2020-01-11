// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable UnusedParameter.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindMiscellaneousQuerySqlServerTest : NorthwindMiscellaneousQueryTestBase<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        public NorthwindMiscellaneousQuerySqlServerTest(NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void Shaper_command_caching_when_parameter_names_different()
        {
            base.Shaper_command_caching_when_parameter_names_different();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'",
                //
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override void Can_convert_manually_build_expression_with_default()
        {
            base.Can_convert_manually_build_expression_with_default();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[City] IS NOT NULL",
                //
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[City] IS NOT NULL");
        }

        public override void Lifting_when_subquery_nested_order_by_anonymous()
        {
            base.Lifting_when_subquery_nested_order_by_anonymous();

            AssertSql(
                @"@__p_0='2'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT [t].[CustomerID]
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[CustomerID]
    ) AS [t]
    CROSS JOIN [Customers] AS [c0]
) AS [t0] ON [o].[CustomerID] = [t0].[CustomerID]
ORDER BY [t0].[CustomerID]");
        }

        public override void Lifting_when_subquery_nested_order_by_simple()
        {
            base.Lifting_when_subquery_nested_order_by_simple();

            AssertSql(
                @"@__p_0='2'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT [t].[CustomerID]
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[CustomerID]
    ) AS [t]
    CROSS JOIN [Customers] AS [c0]
) AS [t0] ON [o].[CustomerID] = [t0].[CustomerID]
ORDER BY [t0].[CustomerID]");
        }

        [ConditionalFact(Skip = "Issue #16006")]
        public virtual void Cache_key_contexts_are_detached()
        {
            var weakRef = Scoper(
                () =>
                {
                    var context = new NorthwindRelationalContext(Fixture.CreateOptions());

                    var wr = new WeakReference(context);

                    using (context)
                    {
                        var orderDetails = context.OrderDetails;

                        Customer Query(NorthwindContext param) =>
                            (from c in context.Customers
                             from o in context.Set<Order>()
                             from od in orderDetails
                             from e1 in param.Employees
                             from e2 in param.Set<Order>()
                             select c).First();

                        Assert.NotNull(Query(context));

                        Assert.True(wr.IsAlive);

                        return wr;
                    }
                });

            GC.Collect();

            Assert.False(weakRef.IsAlive);
        }

        private static T Scoper<T>(Func<T> getter)
        {
            return getter();
        }

        public override async Task Local_dictionary(bool async)
        {
            await base.Local_dictionary(async);

            AssertSql(
                @"@__p_0='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__p_0");
        }

        public override async Task Entity_equality_self(bool async)
        {
            await base.Entity_equality_self(async);

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]");
        }

        public override async Task Entity_equality_local(bool async)
        {
            await base.Entity_equality_local(async);

            AssertSql(
                @"@__entity_equality_local_0_CustomerID='ANATR' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__entity_equality_local_0_CustomerID");
        }

        public override async Task Entity_equality_local_composite_key(bool async)
        {
            await base.Entity_equality_local_composite_key(async);

            AssertSql(
                @"@__entity_equality_local_0_OrderID='10248' (Nullable = true)
@__entity_equality_local_0_ProductID='11' (Nullable = true)

SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = @__entity_equality_local_0_OrderID) AND ([o].[ProductID] = @__entity_equality_local_0_ProductID)");
        }

        public override async Task Entity_equality_local_double_check(bool async)
        {
            await base.Entity_equality_local_double_check(async);

            AssertSql(
                @"@__entity_equality_local_0_CustomerID='ANATR' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] = @__entity_equality_local_0_CustomerID) AND (@__entity_equality_local_0_CustomerID = [c].[CustomerID])");
        }

        public override async Task Join_with_entity_equality_local_on_both_sources(bool async)
        {
            await base.Join_with_entity_equality_local_on_both_sources(async);

            AssertSql(
                @"@__entity_equality_local_0_CustomerID='ANATR' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = @__entity_equality_local_0_CustomerID
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]
WHERE [c].[CustomerID] = @__entity_equality_local_0_CustomerID");
        }

        public override async Task Entity_equality_local_inline(bool async)
        {
            await base.Entity_equality_local_inline(async);

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ANATR'");
        }

        public override async Task Entity_equality_local_inline_composite_key(bool async)
        {
            await base.Entity_equality_local_inline_composite_key(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 10248) AND ([o].[ProductID] = 11)");
        }

        public override async Task Entity_equality_null(bool async)
        {
            await base.Entity_equality_null(async);

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE 0 = 1");
        }

        public override async Task Entity_equality_null_composite_key(bool async)
        {
            await base.Entity_equality_null_composite_key(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE 0 = 1");
        }

        public override async Task Entity_equality_not_null(bool async)
        {
            await base.Entity_equality_not_null(async);

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]");
        }

        public override async Task Entity_equality_not_null_composite_key(bool async)
        {
            await base.Entity_equality_not_null_composite_key(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]");
        }

        public override async Task Entity_equality_through_nested_anonymous_type_projection(bool async)
        {
            await base.Entity_equality_through_nested_anonymous_type_projection(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[CustomerID] IS NOT NULL");
        }

        public override async Task Entity_equality_through_DTO_projection(bool async)
        {
            await base.Entity_equality_through_DTO_projection(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[CustomerID] IS NOT NULL");
        }

        public override async Task Entity_equality_through_subquery(bool async)
        {
            await base.Entity_equality_through_subquery(async);

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE (
    SELECT TOP(1) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]) IS NOT NULL");
        }

        public override async Task Entity_equality_through_include(bool async)
        {
            await base.Entity_equality_through_include(async);

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE 0 = 1");
        }

        public override async Task Entity_equality_orderby(bool async)
        {
            await base.Entity_equality_orderby(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override async Task Entity_equality_orderby_descending_composite_key(bool async)
        {
            await base.Entity_equality_orderby_descending_composite_key(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
ORDER BY [o].[OrderID] DESC, [o].[ProductID] DESC");
        }

        public override async Task Default_if_empty_top_level(bool async)
        {
            await base.Default_if_empty_top_level(async);

            AssertSql(
                @"SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title]
FROM (
    SELECT NULL AS [empty]
) AS [empty]
LEFT JOIN (
    SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
    WHERE [e].[EmployeeID] = -1
) AS [t] ON 1 = 1");
        }

        public override async Task Join_with_default_if_empty_on_both_sources(bool async)
        {
            await base.Join_with_default_if_empty_on_both_sources(async);

            AssertSql(
                @"SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title]
FROM (
    SELECT NULL AS [empty]
) AS [empty]
LEFT JOIN (
    SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
    WHERE [e].[EmployeeID] = -1
) AS [t] ON 1 = 1
INNER JOIN (
    SELECT [t0].[EmployeeID], [t0].[City], [t0].[Country], [t0].[FirstName], [t0].[ReportsTo], [t0].[Title]
    FROM (
        SELECT NULL AS [empty]
    ) AS [empty0]
    LEFT JOIN (
        SELECT [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title]
        FROM [Employees] AS [e0]
        WHERE [e0].[EmployeeID] = -1
    ) AS [t0] ON 1 = 1
) AS [t1] ON [t].[EmployeeID] = [t1].[EmployeeID]");
        }

        public override async Task Default_if_empty_top_level_followed_by_projecting_constant(bool async)
        {
            await base.Default_if_empty_top_level_followed_by_projecting_constant(async);

            AssertSql(
                @"SELECT N'Foo'
FROM (
    SELECT NULL AS [empty]
) AS [empty]
LEFT JOIN (
    SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
    WHERE [e].[EmployeeID] = -1
) AS [t] ON 1 = 1");
        }

        public override async Task Default_if_empty_top_level_positive(bool async)
        {
            await base.Default_if_empty_top_level_positive(async);

            AssertSql(
                @"SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title]
FROM (
    SELECT NULL AS [empty]
) AS [empty]
LEFT JOIN (
    SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
    WHERE [e].[EmployeeID] > 0
) AS [t] ON 1 = 1");
        }

        public override async Task Default_if_empty_top_level_projection(bool async)
        {
            await base.Default_if_empty_top_level_projection(async);

            AssertSql(
                @"SELECT [t].[EmployeeID]
FROM (
    SELECT NULL AS [empty]
) AS [empty]
LEFT JOIN (
    SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
    WHERE [e].[EmployeeID] = -1
) AS [t] ON 1 = 1");
        }

        public override async Task Where_query_composition(bool async)
        {
            await base.Where_query_composition(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE ([e].[FirstName] = (
    SELECT TOP(1) [e0].[FirstName]
    FROM [Employees] AS [e0]
    ORDER BY [e0].[EmployeeID])) OR ([e].[FirstName] IS NULL AND (
    SELECT TOP(1) [e0].[FirstName]
    FROM [Employees] AS [e0]
    ORDER BY [e0].[EmployeeID]) IS NULL)");
        }

        public override async Task Where_query_composition_is_null(bool async)
        {
            await base.Where_query_composition_is_null(async);

            AssertSql(
                @"@__p_0='3'

SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
    ORDER BY [e].[EmployeeID]
) AS [t]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE [e0].[EmployeeID] = [t].[ReportsTo]) IS NULL
ORDER BY [t].[EmployeeID]");
        }

        public override async Task Where_query_composition_is_not_null(bool async)
        {
            await base.Where_query_composition_is_not_null(async);

            AssertSql(
                @"@__p_0='4'
@__p_1='3'

SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title]
FROM (
    SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
    ORDER BY [e].[EmployeeID]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE [e0].[EmployeeID] = [t].[ReportsTo]) IS NOT NULL
ORDER BY [t].[EmployeeID]");
        }

        public override async Task Where_query_composition_entity_equality_one_element_SingleOrDefault(bool async)
        {
            await base.Where_query_composition_entity_equality_one_element_SingleOrDefault(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE [e0].[EmployeeID] = [e].[ReportsTo]) = 0");
        }

        public override async Task Where_query_composition_entity_equality_one_element_Single(bool async)
        {
            await base.Where_query_composition_entity_equality_one_element_Single(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE [e0].[EmployeeID] = [e].[ReportsTo]) = 0");
        }

        public override async Task Where_query_composition_entity_equality_one_element_FirstOrDefault(bool async)
        {
            await base.Where_query_composition_entity_equality_one_element_FirstOrDefault(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE [e0].[EmployeeID] = [e].[ReportsTo]) = 0");
        }

        public override async Task Where_query_composition_entity_equality_one_element_First(bool async)
        {
            await base.Where_query_composition_entity_equality_one_element_First(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE [e0].[EmployeeID] = [e].[ReportsTo]) = 0");
        }

        public override async Task Where_query_composition_entity_equality_no_elements_SingleOrDefault(bool async)
        {
            await base.Where_query_composition_entity_equality_no_elements_SingleOrDefault(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE [e0].[EmployeeID] = 42) = 0");
        }

        public override async Task Where_query_composition_entity_equality_no_elements_Single(bool async)
        {
            await base.Where_query_composition_entity_equality_no_elements_Single(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE [e0].[EmployeeID] = 42) = 0");
        }

        public override async Task Where_query_composition_entity_equality_no_elements_FirstOrDefault(bool async)
        {
            await base.Where_query_composition_entity_equality_no_elements_FirstOrDefault(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE [e0].[EmployeeID] = 42) = 0");
        }

        public override async Task Where_query_composition_entity_equality_no_elements_First(bool async)
        {
            await base.Where_query_composition_entity_equality_no_elements_First(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE [e0].[EmployeeID] = 42) = 0");
        }

        public override async Task Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(bool async)
        {
            await base.Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE ([e0].[EmployeeID] <> [e].[ReportsTo]) OR [e].[ReportsTo] IS NULL) = 0");
        }

        public override async Task Where_query_composition_entity_equality_multiple_elements_Single(bool async)
        {
            await base.Where_query_composition_entity_equality_multiple_elements_Single(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE ([e0].[EmployeeID] <> [e].[ReportsTo]) OR [e].[ReportsTo] IS NULL) = 0");
        }

        public override async Task Where_query_composition_entity_equality_multiple_elements_FirstOrDefault(bool async)
        {
            await base.Where_query_composition_entity_equality_multiple_elements_FirstOrDefault(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE ([e0].[EmployeeID] <> [e].[ReportsTo]) OR [e].[ReportsTo] IS NULL) = 0");
        }

        public override async Task Where_query_composition_entity_equality_multiple_elements_First(bool async)
        {
            await base.Where_query_composition_entity_equality_multiple_elements_First(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (
    SELECT TOP(1) [e0].[EmployeeID]
    FROM [Employees] AS [e0]
    WHERE ([e0].[EmployeeID] <> [e].[ReportsTo]) OR [e].[ReportsTo] IS NULL) = 0");
        }

        public override async Task Where_query_composition2(bool async)
        {
            await base.Where_query_composition2(async);

            AssertSql(
                @"@__p_0='3'

SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
) AS [t]
WHERE ([t].[FirstName] = (
    SELECT TOP(1) [e0].[FirstName]
    FROM [Employees] AS [e0]
    ORDER BY [e0].[EmployeeID])) OR ([t].[FirstName] IS NULL AND (
    SELECT TOP(1) [e0].[FirstName]
    FROM [Employees] AS [e0]
    ORDER BY [e0].[EmployeeID]) IS NULL)");
        }

        public override async Task Where_query_composition2_FirstOrDefault(bool async)
        {
            await base.Where_query_composition2_FirstOrDefault(async);

            AssertSql(
                @"@__p_0='3'

SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
) AS [t]
WHERE ([t].[FirstName] = (
    SELECT TOP(1) [e0].[FirstName]
    FROM [Employees] AS [e0]
    ORDER BY [e0].[EmployeeID])) OR ([t].[FirstName] IS NULL AND (
    SELECT TOP(1) [e0].[FirstName]
    FROM [Employees] AS [e0]
    ORDER BY [e0].[EmployeeID]) IS NULL)");
        }

        public override async Task Where_query_composition2_FirstOrDefault_with_anonymous(bool async)
        {
            await base.Where_query_composition2_FirstOrDefault_with_anonymous(async);

            AssertSql(
                @"@__p_0='3'

SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
) AS [t]
WHERE ([t].[FirstName] = (
    SELECT TOP(1) [e0].[FirstName]
    FROM [Employees] AS [e0]
    ORDER BY [e0].[EmployeeID])) OR ([t].[FirstName] IS NULL AND (
    SELECT TOP(1) [e0].[FirstName]
    FROM [Employees] AS [e0]
    ORDER BY [e0].[EmployeeID]) IS NULL)");
        }

        public override void Select_Subquery_Single()
        {
            base.Select_Subquery_Single();

            AssertSql(
                @"@__p_0='2'

SELECT TOP(@__p_0) [od].[OrderID]
FROM [Order Details] AS [od]
ORDER BY [od].[ProductID], [od].[OrderID]",
                //
                @"@_outer_OrderID='10285'

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_OrderID = [o].[OrderID]
ORDER BY [o].[OrderID]",
                //
                @"@_outer_OrderID='10294'

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_OrderID = [o].[OrderID]
ORDER BY [o].[OrderID]");
        }

        public override void Select_Where_Subquery_Deep_Single()
        {
            base.Select_Where_Subquery_Deep_Single();

            AssertSql(
                @"@__p_0='2'

SELECT TOP(@__p_0) [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 10344) AND ((
    SELECT TOP(1) (
        SELECT TOP(1) [c].[City]
        FROM [Customers] AS [c]
        WHERE [o0].[CustomerID] = [c].[CustomerID])
    FROM [Orders] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID]) = N'Seattle')");
        }

        public override void Select_Where_Subquery_Deep_First()
        {
            base.Select_Where_Subquery_Deep_First();

            AssertSql(
                @"@__p_0='2'

SELECT TOP(@__p_0) [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE (
    SELECT TOP(1) (
        SELECT TOP(1) [c].[City]
        FROM [Customers] AS [c]
        WHERE [o0].[CustomerID] = [c].[CustomerID])
    FROM [Orders] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID]) = N'Seattle'");
        }

        public override void Select_Where_Subquery_Equality()
        {
            base.Select_Where_Subquery_Equality();

            AssertSql(
                @"@__p_0='1'

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT TOP(2) [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
        FROM [Order Details] AS [o0]
        ORDER BY [o0].[OrderID]
    ) AS [t0]
    WHERE ((
        SELECT TOP(1) [c].[Country]
        FROM [Customers] AS [c]
        WHERE [c].[CustomerID] = [t].[CustomerID]
        ORDER BY [c].[CustomerID]) = (
        SELECT TOP(1) [c0].[Country]
        FROM [Orders] AS [o1]
        INNER JOIN [Customers] AS [c0] ON [o1].[CustomerID] = [c0].[CustomerID]
        WHERE [o1].[OrderID] = [t0].[OrderID]
        ORDER BY [o1].[OrderID], [c0].[CustomerID])) OR ((
        SELECT TOP(1) [c].[Country]
        FROM [Customers] AS [c]
        WHERE [c].[CustomerID] = [t].[CustomerID]
        ORDER BY [c].[CustomerID]) IS NULL AND (
        SELECT TOP(1) [c0].[Country]
        FROM [Orders] AS [o1]
        INNER JOIN [Customers] AS [c0] ON [o1].[CustomerID] = [c0].[CustomerID]
        WHERE [o1].[OrderID] = [t0].[OrderID]
        ORDER BY [o1].[OrderID], [c0].[CustomerID]) IS NULL)) > 0
ORDER BY [t].[OrderID]");
        }

        public override async Task Where_subquery_anon(bool async)
        {
            await base.Where_subquery_anon(async);

            AssertSql(
                @"@__p_0='3'

SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title], [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
    ORDER BY [e].[EmployeeID]
) AS [t]
CROSS JOIN (
    SELECT TOP(5) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t0]
WHERE [t].[EmployeeID] = [t0].[EmployeeID]
ORDER BY [t].[EmployeeID]");
        }

        public override async Task Where_subquery_anon_nested(bool async)
        {
            await base.Where_subquery_anon_nested(async);

            AssertSql(
                @"@__p_0='3'

SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title], [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate], [t1].[CustomerID], [t1].[Address], [t1].[City], [t1].[CompanyName], [t1].[ContactName], [t1].[ContactTitle], [t1].[Country], [t1].[Fax], [t1].[Phone], [t1].[PostalCode], [t1].[Region]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
    ORDER BY [e].[EmployeeID]
) AS [t]
CROSS JOIN (
    SELECT TOP(5) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t0]
CROSS JOIN (
    SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [t1]
WHERE [t].[City] = N'Seattle'
ORDER BY [t].[EmployeeID]");
        }

        public override async Task OrderBy_SelectMany(bool async)
        {
            await base.OrderBy_SelectMany(async);

            AssertSql(
                @"SELECT [c].[ContactName], [t].[OrderID]
FROM [Customers] AS [c]
CROSS JOIN (
    SELECT TOP(3) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]
WHERE [c].[CustomerID] = [t].[CustomerID]
ORDER BY [c].[CustomerID]");
        }

        public override async Task Let_any_subquery_anonymous(bool async)
        {
            await base.Let_any_subquery_anonymous(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE [o].[CustomerID] = [c].[CustomerID]) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [hasOrders]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID]");
        }

        public override async Task OrderBy_arithmetic(bool async)
        {
            await base.OrderBy_arithmetic(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
ORDER BY [e].[EmployeeID] - [e].[EmployeeID]");
        }

        public override async Task OrderBy_condition_comparison(bool async)
        {
            await base.OrderBy_condition_comparison(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
ORDER BY CASE
    WHEN [p].[UnitsInStock] > CAST(0 AS smallint) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [p].[ProductID]");
        }

        public override async Task OrderBy_ternary_conditions(bool async)
        {
            await base.OrderBy_ternary_conditions(async);

            // issue #18774
            //            AssertSql(
            //                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
            //FROM [Products] AS [p]
            //ORDER BY CASE
            //    WHEN (([p].[UnitsInStock] > CAST(10 AS smallint)) AND ([p].[ProductID] > 40)) OR (([p].[UnitsInStock] <= CAST(10 AS smallint)) AND ([p].[ProductID] <= 40))
            //    THEN CAST(1 AS bit) ELSE CAST(0 AS bit)
            //END, [p].[ProductID]");
        }

        public override void OrderBy_any()
        {
            base.OrderBy_any();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE ([c].[CustomerID] = [o].[CustomerID]) AND ([o].[OrderID] > 11000)) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [c].[CustomerID]");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task Skip(bool async)
        {
            await base.Skip(async);

            AssertSql(
                @"@__p_0='5'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
OFFSET @__p_0 ROWS");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task Skip_no_orderby(bool async)
        {
            await base.Skip_no_orderby(async);

            AssertSql(
                @"@__p_0='5'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY (SELECT 1)
OFFSET @__p_0 ROWS");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task Skip_orderby_const(bool async)
        {
            await base.Skip_orderby_const(async);

            AssertSql(
                @"@__p_0='5'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY (SELECT 1)
OFFSET @__p_0 ROWS");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task Skip_Take(bool async)
        {
            await base.Skip_Take(async);

            AssertSql(
                @"@__p_0='5'
@__p_1='10'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactName]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task Join_Customers_Orders_Skip_Take(bool async)
        {
            await base.Join_Customers_Orders_Skip_Take(async);

            AssertSql(
                @"@__p_0='10'
@__p_1='5'

SELECT [c].[ContactName], [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [o].[OrderID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY");
        }

        public override async Task Join_Customers_Orders_Skip_Take_followed_by_constant_projection(bool async)
        {
            await base.Join_Customers_Orders_Skip_Take_followed_by_constant_projection(async);

            AssertSql(
                @"@__p_0='10'
@__p_1='5'

SELECT N'Foo'
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [o].[OrderID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task Join_Customers_Orders_Projection_With_String_Concat_Skip_Take(bool async)
        {
            await base.Join_Customers_Orders_Projection_With_String_Concat_Skip_Take(async);

            AssertSql(
                @"@__p_0='10'
@__p_1='5'

SELECT (COALESCE([c].[ContactName], N'') + N' ') + COALESCE([c].[ContactTitle], N'') AS [Contact], [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [o].[OrderID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task Join_Customers_Orders_Orders_Skip_Take_Same_Properties(bool async)
        {
            await base.Join_Customers_Orders_Orders_Skip_Take_Same_Properties(async);

            AssertSql(
                @"@__p_0='10'
@__p_1='5'

SELECT [o].[OrderID], [c].[CustomerID] AS [CustomerIDA], [c0].[CustomerID] AS [CustomerIDB], [c].[ContactName] AS [ContactNameA], [c0].[ContactName] AS [ContactNameB]
FROM [Orders] AS [o]
INNER JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
INNER JOIN [Customers] AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
ORDER BY [o].[OrderID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY");
        }

        public override async Task Ternary_should_not_evaluate_both_sides(bool async)
        {
            await base.Ternary_should_not_evaluate_both_sides(async);

            AssertSql(
                @"@__p_0='none' (Size = 4000)
@__p_1='none' (Size = 4000)
@__p_2='none' (Size = 4000)

SELECT [c].[CustomerID], @__p_0 AS [Data1], @__p_1 AS [Data2], @__p_2 AS [Data3]
FROM [Customers] AS [c]");
        }

        public override async Task Ternary_should_not_evaluate_both_sides_with_parameter(bool async)
        {
            await base.Ternary_should_not_evaluate_both_sides_with_parameter(async);

            AssertSql(
                @"SELECT CAST(1 AS bit) AS [Data1]
FROM [Orders] AS [o]");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task Take_Skip(bool async)
        {
            await base.Take_Skip(async);

            AssertSql(
                @"@__p_0='10'
@__p_1='5'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactName]
) AS [t]
ORDER BY [t].[ContactName]
OFFSET @__p_1 ROWS");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task Take_Skip_Distinct(bool async)
        {
            await base.Take_Skip_Distinct(async);

            AssertSql(
                @"@__p_0='10'
@__p_1='5'

SELECT DISTINCT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
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
        public override async Task Take_Skip_Distinct_Caching(bool async)
        {
            await base.Take_Skip_Distinct_Caching(async);

            AssertSql(
                @"@__p_0='10'
@__p_1='5'

SELECT DISTINCT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactName]
    ) AS [t]
    ORDER BY [t].[ContactName]
    OFFSET @__p_1 ROWS
) AS [t0]",
                //
                @"@__p_0='15'
@__p_1='10'

SELECT DISTINCT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactName]
    ) AS [t]
    ORDER BY [t].[ContactName]
    OFFSET @__p_1 ROWS
) AS [t0]");
        }

        public override async Task Take_Distinct_Count(bool async)
        {
            await base.Take_Distinct_Count(async);

            AssertSql(
                @"@__p_0='5'

SELECT COUNT(*)
FROM (
    SELECT DISTINCT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
    FROM (
        SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
        FROM [Orders] AS [o]
    ) AS [t]
) AS [t0]");
        }

        public override async Task Take_Where_Distinct_Count(bool async)
        {
            await base.Take_Where_Distinct_Count(async);

            AssertSql(
                @"@__p_0='5'

SELECT COUNT(*)
FROM (
    SELECT DISTINCT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
    FROM (
        SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [o].[CustomerID] = N'FRANK'
    ) AS [t]
) AS [t0]");
        }

        public override async Task Queryable_simple(bool async)
        {
            await base.Queryable_simple(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Queryable_simple_anonymous(bool async)
        {
            await base.Queryable_simple_anonymous(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Queryable_nested_simple(bool async)
        {
            await base.Queryable_nested_simple(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Queryable_simple_anonymous_projection_subquery(bool async)
        {
            await base.Queryable_simple_anonymous_projection_subquery(async);

            AssertSql(
                @"@__p_0='91'

SELECT TOP(@__p_0) [c].[City]
FROM [Customers] AS [c]");
        }

        public override async Task Queryable_simple_anonymous_subquery(bool async)
        {
            await base.Queryable_simple_anonymous_subquery(async);

            AssertSql(
                @"@__p_0='91'

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Take_simple(bool async)
        {
            await base.Take_simple(async);

            AssertSql(
                @"@__p_0='10'

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override async Task Take_simple_parameterized(bool async)
        {
            await base.Take_simple_parameterized(async);

            AssertSql(
                @"@__p_0='10'

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override async Task Take_simple_projection(bool async)
        {
            await base.Take_simple_projection(async);

            AssertSql(
                @"@__p_0='10'

SELECT TOP(@__p_0) [c].[City]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override async Task Take_subquery_projection(bool async)
        {
            await base.Take_subquery_projection(async);

            AssertSql(
                @"@__p_0='2'

SELECT TOP(@__p_0) [c].[City]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override async Task OrderBy_Take_Count(bool async)
        {
            await base.OrderBy_Take_Count(async);

            AssertSql(
                @"@__p_0='5'

SELECT COUNT(*)
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]");
        }

        public override async Task Take_OrderBy_Count(bool async)
        {
            await base.Take_OrderBy_Count(async);

            AssertSql(
                @"@__p_0='5'

SELECT COUNT(*)
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
) AS [t]");
        }

        public override async Task Any_simple(bool async)
        {
            await base.Any_simple(async);

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c]) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Any_predicate(bool async)
        {
            await base.Any_predicate(async);

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        WHERE [c].[ContactName] IS NOT NULL AND ([c].[ContactName] LIKE N'A%')) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Any_nested_negated(bool async)
        {
            await base.Any_nested_negated(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE NOT (EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] IS NOT NULL AND ([o].[CustomerID] LIKE N'A%')))");
        }

        public override async Task Any_nested_negated2(bool async)
        {
            await base.Any_nested_negated2(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] <> N'London') OR [c].[City] IS NULL) AND NOT (EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] IS NOT NULL AND ([o].[CustomerID] LIKE N'A%')))");
        }

        public override async Task Any_nested_negated3(bool async)
        {
            await base.Any_nested_negated3(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE NOT (EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] IS NOT NULL AND ([o].[CustomerID] LIKE N'A%'))) AND (([c].[City] <> N'London') OR [c].[City] IS NULL)");
        }

        public override async Task Any_nested(bool async)
        {
            await base.Any_nested(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] IS NOT NULL AND ([o].[CustomerID] LIKE N'A%'))");
        }

        public override async Task Any_nested2(bool async)
        {
            await base.Any_nested2(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] <> N'London') OR [c].[City] IS NULL) AND EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] IS NOT NULL AND ([o].[CustomerID] LIKE N'A%'))");
        }

        public override async Task Any_nested3(bool async)
        {
            await base.Any_nested3(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] IS NOT NULL AND ([o].[CustomerID] LIKE N'A%')) AND (([c].[City] <> N'London') OR [c].[City] IS NULL)");
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
    WHERE ([c].[CustomerID] = [o].[CustomerID]) AND ([o].[EmployeeID] = 1))");
        }

        public override async Task All_top_level(bool async)
        {
            await base.All_top_level(async);

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        WHERE [c].[ContactName] IS NULL OR NOT ([c].[ContactName] LIKE N'A%')) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task All_top_level_column(bool async)
        {
            await base.All_top_level_column(async);

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        WHERE (([c].[ContactName] <> N'') OR [c].[ContactName] IS NULL) AND ([c].[ContactName] IS NULL OR (LEFT([c].[ContactName], LEN([c].[ContactName])) <> [c].[ContactName]))) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task All_top_level_subquery(bool async)
        {
            await base.All_top_level_subquery(async);

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        WHERE NOT (EXISTS (
            SELECT 1
            FROM [Customers] AS [c0]
            WHERE EXISTS (
                SELECT 1
                FROM [Customers] AS [c1]
                WHERE [c].[CustomerID] = [c1].[CustomerID])))) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task All_top_level_subquery_ef_property(bool async)
        {
            await base.All_top_level_subquery_ef_property(async);

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        WHERE NOT (EXISTS (
            SELECT 1
            FROM [Customers] AS [c0]
            WHERE EXISTS (
                SELECT 1
                FROM [Customers] AS [c1]
                WHERE [c].[CustomerID] = [c1].[CustomerID])))) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Where_select_many_or(bool async)
        {
            await base.Where_select_many_or(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE ([c].[City] = N'London') OR ([e].[City] = N'London')");
        }

        public override async Task Where_select_many_or2(bool async)
        {
            await base.Where_select_many_or2(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE ([c].[City] = N'London') OR ([c].[City] = N'Berlin')");
        }

        public override async Task Where_select_many_or3(bool async)
        {
            await base.Where_select_many_or3(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE (([c].[City] = N'London') OR ([c].[City] = N'Berlin')) OR ([c].[City] = N'Seattle')");
        }

        public override async Task Where_select_many_or4(bool async)
        {
            await base.Where_select_many_or4(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE ((([c].[City] = N'London') OR ([c].[City] = N'Berlin')) OR ([c].[City] = N'Seattle')) OR ([c].[City] = N'Lisboa')");
        }

        public override async Task Where_select_many_or_with_parameter(bool async)
        {
            await base.Where_select_many_or_with_parameter(async);

            AssertSql(
                @"@__london_0='London' (Size = 4000)
@__lisboa_1='Lisboa' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE ((([c].[City] = @__london_0) OR ([c].[City] = N'Berlin')) OR ([c].[City] = N'Seattle')) OR ([c].[City] = @__lisboa_1)");
        }

        public override async Task SelectMany_simple_subquery(bool async)
        {
            await base.SelectMany_simple_subquery(async);

            AssertSql(
                @"@__p_0='9'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
) AS [t]
CROSS JOIN [Customers] AS [c]");
        }

        public override async Task SelectMany_simple1(bool async)
        {
            await base.SelectMany_simple1(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
CROSS JOIN [Customers] AS [c]");
        }

        public override async Task SelectMany_simple2(bool async)
        {
            await base.SelectMany_simple2(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e0].[FirstName]
FROM [Employees] AS [e]
CROSS JOIN [Customers] AS [c]
CROSS JOIN [Employees] AS [e0]");
        }

        public override async Task SelectMany_entity_deep(bool async)
        {
            await base.SelectMany_entity_deep(async);

            AssertSql(
                @"SELECT [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title], [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title], [e2].[EmployeeID], [e2].[City], [e2].[Country], [e2].[FirstName], [e2].[ReportsTo], [e2].[Title]
FROM [Employees] AS [e]
CROSS JOIN [Employees] AS [e0]
CROSS JOIN [Employees] AS [e1]
CROSS JOIN [Employees] AS [e2]");
        }

        public override async Task SelectMany_projection1(bool async)
        {
            await base.SelectMany_projection1(async);

            AssertSql(
                @"SELECT [e].[City], [e0].[Country]
FROM [Employees] AS [e]
CROSS JOIN [Employees] AS [e0]");
        }

        public override async Task SelectMany_projection2(bool async)
        {
            await base.SelectMany_projection2(async);

            AssertSql(
                @"SELECT [e].[City], [e0].[Country], [e1].[FirstName]
FROM [Employees] AS [e]
CROSS JOIN [Employees] AS [e0]
CROSS JOIN [Employees] AS [e1]");
        }

        public override async Task SelectMany_Count(bool async)
        {
            await base.SelectMany_Count(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]");
        }

        public override async Task SelectMany_LongCount(bool async)
        {
            await base.SelectMany_LongCount(async);

            AssertSql(
                @"SELECT COUNT_BIG(*)
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]");
        }

        public override async Task SelectMany_OrderBy_ThenBy_Any(bool async)
        {
            await base.SelectMany_OrderBy_ThenBy_Any(async);

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        CROSS JOIN [Orders] AS [o]) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Join_Where_Count(bool async)
        {
            await base.Join_Where_Count(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task Where_Join_Any(bool async)
        {
            await base.Where_Join_Any(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] = N'ALFKI') AND EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE ([c].[CustomerID] = [o].[CustomerID]) AND ([o].[OrderDate] = '2008-10-24T00:00:00.000'))");
        }

        public override async Task Where_Join_Exists(bool async)
        {
            await base.Where_Join_Exists(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] = N'ALFKI') AND EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE ([c].[CustomerID] = [o].[CustomerID]) AND ([o].[OrderDate] = '2008-10-24T00:00:00.000'))");
        }

        public override async Task Where_Join_Exists_Inequality(bool async)
        {
            await base.Where_Join_Exists_Inequality(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] = N'ALFKI') AND EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE ([c].[CustomerID] = [o].[CustomerID]) AND (([o].[OrderDate] <> '2008-10-24T00:00:00.000') OR [o].[OrderDate] IS NULL))");
        }

        public override async Task Where_Join_Exists_Constant(bool async)
        {
            await base.Where_Join_Exists_Constant(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] = N'ALFKI') AND EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE 0 = 1)");
        }

        public override async Task Where_Join_Not_Exists(bool async)
        {
            await base.Where_Join_Not_Exists(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] = N'ALFKI') AND NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE 0 = 1)");
        }

        public override async Task Join_OrderBy_Count(bool async)
        {
            await base.Join_OrderBy_Count(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override async Task Multiple_joins_Where_Order_Any(bool async)
        {
            await base.Multiple_joins_Where_Order_Any(async);

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
        INNER JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
        WHERE [c].[City] = N'London') THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Where_join_select(bool async)
        {
            await base.Where_join_select(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task Where_orderby_join_select(bool async)
        {
            await base.Where_orderby_join_select(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] <> N'ALFKI'
ORDER BY [c].[CustomerID]");
        }

        public override async Task Where_join_orderby_join_select(bool async)
        {
            await base.Where_join_orderby_join_select(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
INNER JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE [c].[CustomerID] <> N'ALFKI'
ORDER BY [c].[CustomerID]");
        }

        public override async Task Where_select_many(bool async)
        {
            await base.Where_select_many(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task Where_orderby_select_many(bool async)
        {
            await base.Where_orderby_select_many(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]");
        }

        public override async Task SelectMany_cartesian_product_with_ordering(bool async)
        {
            await base.SelectMany_cartesian_product_with_ordering(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[City]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE ([c].[City] = [e].[City]) OR ([c].[City] IS NULL AND [e].[City] IS NULL)
ORDER BY [e].[City], [c].[CustomerID] DESC");
        }

        public override async Task SelectMany_Joined_DefaultIfEmpty(bool async)
        {
            await base.SelectMany_Joined_DefaultIfEmpty(async);

            AssertSql(
                @"SELECT [c].[ContactName], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override async Task SelectMany_Joined_DefaultIfEmpty2(bool async)
        {
            await base.SelectMany_Joined_DefaultIfEmpty2(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override async Task SelectMany_Joined_DefaultIfEmpty3(bool async)
        {
            await base.SelectMany_Joined_DefaultIfEmpty3(async);

            AssertSql(
                @"SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE EXISTS (
        SELECT 1
        FROM [Order Details] AS [o0]
        WHERE [o].[OrderID] = [o0].[OrderID])
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]");
        }

        public override async Task SelectMany_Joined_Take(bool async)
        {
            await base.SelectMany_Joined_Take(async);

            AssertSql(
                @"SELECT [c].[ContactName], [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
    FROM (
        SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], ROW_NUMBER() OVER(PARTITION BY [o].[CustomerID] ORDER BY [o].[OrderID]) AS [row]
        FROM [Orders] AS [o]
    ) AS [t]
    WHERE [t].[row] <= 4
) AS [t0] ON [c].[CustomerID] = [t0].[CustomerID]");
        }

        public override async Task Take_with_single(bool async)
        {
            await base.Take_with_single(async);

            AssertSql(
                @"@__p_0='1'

SELECT TOP(2) [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [t]
ORDER BY [t].[CustomerID]");
        }

        public override async Task Take_with_single_select_many(bool async)
        {
            await base.Take_with_single_select_many(async);

            AssertSql(
                @"@__p_0='1'

SELECT TOP(2) [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [t].[OrderID], [t].[CustomerID0], [t].[EmployeeID], [t].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID] AS [CustomerID0], [o].[EmployeeID], [o].[OrderDate]
    FROM [Customers] AS [c]
    CROSS JOIN [Orders] AS [o]
    ORDER BY [c].[CustomerID], [o].[OrderID]
) AS [t]
ORDER BY [t].[CustomerID], [t].[OrderID]");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task Distinct_Skip(bool async)
        {
            await base.Distinct_Skip(async);

            AssertSql(
                @"@__p_0='5'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
) AS [t]
ORDER BY [t].[CustomerID]
OFFSET @__p_0 ROWS");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task Distinct_Skip_Take(bool async)
        {
            await base.Distinct_Skip_Take(async);

            AssertSql(
                @"@__p_0='5'
@__p_1='10'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
) AS [t]
ORDER BY [t].[ContactName]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task Skip_Distinct(bool async)
        {
            await base.Skip_Distinct(async);

            AssertSql(
                @"@__p_0='5'

SELECT DISTINCT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactName]
    OFFSET @__p_0 ROWS
) AS [t]");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task Skip_Take_Distinct(bool async)
        {
            await base.Skip_Take_Distinct(async);

            AssertSql(
                @"@__p_0='5'
@__p_1='10'

SELECT DISTINCT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactName]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task Skip_Take_Any(bool async)
        {
            await base.Skip_Take_Any(async);

            AssertSql(
                @"@__p_0='5'
@__p_1='10'

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactName]
        OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task Skip_Take_All(bool async)
        {
            await base.Skip_Take_All(async);

            AssertSql(
                @"@__p_0='4'
@__p_1='7'

SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM (
            SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
            FROM [Customers] AS [c]
            ORDER BY [c].[CustomerID]
            OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
        ) AS [t]
        WHERE NOT ([t].[CustomerID] LIKE N'B%')) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task Take_All(bool async)
        {
            await base.Take_All(async);

            AssertSql(
                @"@__p_0='4'

SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM (
            SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
            FROM [Customers] AS [c]
            ORDER BY [c].[CustomerID]
        ) AS [t]
        WHERE NOT ([t].[CustomerID] LIKE N'A%')) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task Skip_Take_Any_with_predicate(bool async)
        {
            await base.Skip_Take_Any_with_predicate(async);

            AssertSql(
                @"@__p_0='5'
@__p_1='7'

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM (
            SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
            FROM [Customers] AS [c]
            ORDER BY [c].[CustomerID]
            OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
        ) AS [t]
        WHERE [t].[CustomerID] LIKE N'C%') THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task Take_Any_with_predicate(bool async)
        {
            await base.Take_Any_with_predicate(async);

            AssertSql(
                @"@__p_0='5'

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM (
            SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
            FROM [Customers] AS [c]
            ORDER BY [c].[CustomerID]
        ) AS [t]
        WHERE [t].[CustomerID] LIKE N'B%') THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task OrderBy(bool async)
        {
            await base.OrderBy(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override async Task OrderBy_true(bool async)
        {
            await base.OrderBy_true(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task OrderBy_integer(bool async)
        {
            await base.OrderBy_integer(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task OrderBy_parameter(bool async)
        {
            await base.OrderBy_parameter(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task OrderBy_anon(bool async)
        {
            await base.OrderBy_anon(async);

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override async Task OrderBy_anon2(bool async)
        {
            await base.OrderBy_anon2(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override async Task Distinct_Take(bool async)
        {
            await base.Distinct_Take(async);

            AssertSql(
                @"@__p_0='5'

SELECT TOP(@__p_0) [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM (
    SELECT DISTINCT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
) AS [t]
ORDER BY [t].[OrderID]");
        }

        public override async Task Distinct_Take_Count(bool async)
        {
            await base.Distinct_Take_Count(async);

            AssertSql(
                @"@__p_0='5'

SELECT COUNT(*)
FROM (
    SELECT DISTINCT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
) AS [t]");
        }

        public override async Task OrderBy_shadow(bool async)
        {
            await base.OrderBy_shadow(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
ORDER BY [e].[Title], [e].[EmployeeID]");
        }

        public override async Task OrderBy_multiple(bool async)
        {
            await base.OrderBy_multiple(async);

            AssertSql(
                @"SELECT [c].[City]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[Country], [c].[City]");
        }

        public override async Task OrderBy_ThenBy_Any(bool async)
        {
            await base.OrderBy_ThenBy_Any(async);

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c]) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task OrderBy_correlated_subquery1(bool async)
        {
            await base.OrderBy_correlated_subquery1(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c0]
        WHERE [c0].[CustomerID] = [c].[CustomerID]) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [c].[CustomerID]");
        }

        public override async Task OrderBy_correlated_subquery2(bool async)
        {
            await base.OrderBy_correlated_subquery2(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[OrderID] <= 10250) AND (((
    SELECT TOP(1) [c].[City]
    FROM [Customers] AS [c]
    ORDER BY CASE
        WHEN EXISTS (
            SELECT 1
            FROM [Customers] AS [c0]
            WHERE [c0].[CustomerID] = N'ALFKI') THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END) <> N'Nowhere') OR (
    SELECT TOP(1) [c].[City]
    FROM [Customers] AS [c]
    ORDER BY CASE
        WHEN EXISTS (
            SELECT 1
            FROM [Customers] AS [c0]
            WHERE [c0].[CustomerID] = N'ALFKI') THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END) IS NULL)");
        }

        public override async Task Where_subquery_recursive_trivial(bool async)
        {
            await base.Where_subquery_recursive_trivial(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE EXISTS (
    SELECT 1
    FROM [Employees] AS [e0]
    WHERE EXISTS (
        SELECT 1
        FROM [Employees] AS [e1]))
ORDER BY [e].[EmployeeID]");
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

        public override void Select_DTO_constructor_distinct_with_navigation_translated_to_server()
        {
            base.Select_DTO_constructor_distinct_with_navigation_translated_to_server();

            AssertSql(
                @"SELECT DISTINCT [c].[City]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
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

        public override void Select_nested_collection_count_using_DTO()
        {
            base.Select_nested_collection_count_using_DTO();

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Id], (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]) AS [Count]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'");
        }

        public override async Task Select_DTO_with_member_init_distinct_in_subquery_translated_to_server(bool async)
        {
            await base.Select_DTO_with_member_init_distinct_in_subquery_translated_to_server(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT DISTINCT [o].[CustomerID], [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
) AS [t]
INNER JOIN [Customers] AS [c] ON [t].[CustomerID] = [c].[CustomerID]");
        }

        public override async Task Select_DTO_with_member_init_distinct_in_subquery_translated_to_server_2(bool async)
        {
            await base.Select_DTO_with_member_init_distinct_in_subquery_translated_to_server_2(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT DISTINCT [o].[CustomerID], [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
) AS [t]
INNER JOIN [Customers] AS [c] ON [t].[CustomerID] = [c].[CustomerID]");
        }

        public override void Select_DTO_with_member_init_distinct_in_subquery_used_in_projection_translated_to_server()
        {
            base.Select_DTO_with_member_init_distinct_in_subquery_used_in_projection_translated_to_server();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[CustomerID] AS [Id], [t].[OrderID] AS [Count]
FROM [Customers] AS [c]
CROSS JOIN (
    SELECT DISTINCT [o].[CustomerID], [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
) AS [t]
WHERE [c].[CustomerID] LIKE N'A%'");
        }

        public override async Task Select_correlated_subquery_projection(bool async)
        {
            await base.Select_correlated_subquery_projection(async);

            AssertSql(
                @"@__p_0='3'

SELECT TOP(@__p_0) [cc].[CustomerID]
FROM [Customers] AS [cc]
ORDER BY [cc].[CustomerID]",
                //
                @"@_outer_CustomerID='ALFKI' (Size = 5)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = @_outer_CustomerID",
                //
                @"@_outer_CustomerID='ANATR' (Size = 5)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = @_outer_CustomerID",
                //
                @"@_outer_CustomerID='ANTON' (Size = 5)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = @_outer_CustomerID");
        }

        public override async Task Select_correlated_subquery_filtered(bool async)
        {
            await base.Select_correlated_subquery_filtered(async);

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID]",
                //
                @"@_outer_CustomerID='ALFKI' (Size = 5)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = @_outer_CustomerID",
                //
                @"@_outer_CustomerID='ANATR' (Size = 5)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = @_outer_CustomerID",
                //
                @"@_outer_CustomerID='ANTON' (Size = 5)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = @_outer_CustomerID",
                //
                @"@_outer_CustomerID='AROUT' (Size = 5)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = @_outer_CustomerID");
        }

        public override async Task Select_correlated_subquery_ordered(bool async)
        {
            await base.Select_correlated_subquery_ordered(async);

            AssertSql(
                @"@__p_0='3'

SELECT TOP(@__p_0) [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
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

        public override async Task Where_subquery_on_bool(bool async)
        {
            await base.Where_subquery_on_bool(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE N'Chai' IN (
    SELECT [p0].[ProductName]
    FROM [Products] AS [p0]
)");
        }

        public override async Task Where_subquery_on_collection(bool async)
        {
            await base.Where_subquery_on_collection(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE CAST(5 AS smallint) IN (
    SELECT [o].[Quantity]
    FROM [Order Details] AS [o]
    WHERE [o].[ProductID] = [p].[ProductID]
)");
        }

        public override async Task Select_many_cross_join_same_collection(bool async)
        {
            await base.Select_many_cross_join_same_collection(async);

            AssertSql(
                @"SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c]
CROSS JOIN [Customers] AS [c0]");
        }

        public override async Task OrderBy_null_coalesce_operator(bool async)
        {
            await base.OrderBy_null_coalesce_operator(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY COALESCE([c].[Region], N'ZZ'), [c].[CustomerID]");
        }

        public override async Task Select_null_coalesce_operator(bool async)
        {
            await base.Select_null_coalesce_operator(async);

            // issue #16038
//            AssertSql(
//                @"SELECT [c].[CustomerID], [c].[CompanyName], COALESCE([c].[Region], N'ZZ') AS [Region]
//FROM [Customers] AS [c]
//ORDER BY [Region], [c].[CustomerID]");
        }

        public override async Task OrderBy_conditional_operator(bool async)
        {
            await base.OrderBy_conditional_operator(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY CASE
    WHEN [c].[Region] IS NULL THEN N'ZZ'
    ELSE [c].[Region]
END, [c].[CustomerID]");
        }

        public override async Task Null_Coalesce_Short_Circuit(bool async)
        {
            await base.Null_Coalesce_Short_Circuit(async);

            AssertSql(
                @"@__p_0='False'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], @__p_0 AS [Test]
FROM (
    SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
) AS [t]");
        }

        public override async Task Null_Coalesce_Short_Circuit_with_server_correlated_leftover(bool async)
        {
            await base.Null_Coalesce_Short_Circuit_with_server_correlated_leftover(async);

            AssertSql(
                @"SELECT CAST(0 AS bit) AS [Result]
FROM [Customers] AS [c]");
        }

        public override async Task OrderBy_conditional_operator_where_condition_false(bool async)
        {
            await base.OrderBy_conditional_operator_where_condition_false(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[City]");
        }

        public override async Task OrderBy_comparison_operator(bool async)
        {
            await base.OrderBy_comparison_operator(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY CASE
    WHEN ([c].[Region] = N'ASK') AND [c].[Region] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Projection_null_coalesce_operator(bool async)
        {
            await base.Projection_null_coalesce_operator(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[CompanyName], COALESCE([c].[Region], N'ZZ') AS [Region]
FROM [Customers] AS [c]");
        }

        public override async Task Filter_coalesce_operator(bool async)
        {
            await base.Filter_coalesce_operator(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE COALESCE([c].[CompanyName], [c].[ContactName]) = N'The Big Cheese'");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task Take_skip_null_coalesce_operator(bool async)
        {
            await base.Take_skip_null_coalesce_operator(async);

            AssertSql(
                @"@__p_0='10'
@__p_1='5'

SELECT DISTINCT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [t].[c]
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], COALESCE([c].[Region], N'ZZ') AS [c]
        FROM [Customers] AS [c]
        ORDER BY COALESCE([c].[Region], N'ZZ')
    ) AS [t]
    ORDER BY [t].[c]
    OFFSET @__p_1 ROWS
) AS [t0]");
        }

        public override async Task Select_take_null_coalesce_operator(bool async)
        {
            await base.Select_take_null_coalesce_operator(async);

            // issue #16038
//            AssertSql(
//                @"@__p_0='5'

//SELECT TOP(@__p_0) [c].[CustomerID], [c].[CompanyName], COALESCE([c].[Region], N'ZZ') AS [Region]
//FROM [Customers] AS [c]
//ORDER BY [Region]");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task Select_take_skip_null_coalesce_operator(bool async)
        {
            await base.Select_take_skip_null_coalesce_operator(async);

            AssertSql(
                @"@__p_0='10'
@__p_1='5'

SELECT [t].[CustomerID], [t].[CompanyName], COALESCE([t].[Region], N'ZZ') AS [Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], COALESCE([c].[Region], N'ZZ') AS [c]
    FROM [Customers] AS [c]
    ORDER BY COALESCE([c].[Region], N'ZZ')
) AS [t]
ORDER BY [t].[c]
OFFSET @__p_1 ROWS");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task Select_take_skip_null_coalesce_operator2(bool async)
        {
            await base.Select_take_skip_null_coalesce_operator2(async);

            AssertSql(
                @"@__p_0='10'
@__p_1='5'

SELECT [t].[CustomerID], [t].[CompanyName], [t].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], COALESCE([c].[Region], N'ZZ') AS [c]
    FROM [Customers] AS [c]
    ORDER BY COALESCE([c].[Region], N'ZZ')
) AS [t]
ORDER BY [t].[c]
OFFSET @__p_1 ROWS");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task Select_take_skip_null_coalesce_operator3(bool async)
        {
            await base.Select_take_skip_null_coalesce_operator3(async);

            AssertSql(
                @"@__p_0='10'
@__p_1='5'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], COALESCE([c].[Region], N'ZZ') AS [c]
    FROM [Customers] AS [c]
    ORDER BY COALESCE([c].[Region], N'ZZ')
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

        public override async Task DateTime_parse_is_inlined(bool async)
        {
            await base.DateTime_parse_is_inlined(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] > '1998-01-01T12:00:00.000'");
        }

        public override async Task DateTime_parse_is_parameterized_when_from_closure(bool async)
        {
            await base.DateTime_parse_is_parameterized_when_from_closure(async);

            AssertSql(
                @"@__Parse_0='1998-01-01T12:00:00' (Nullable = true) (DbType = DateTime)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] > @__Parse_0");
        }

        public override async Task New_DateTime_is_inlined(bool async)
        {
            await base.New_DateTime_is_inlined(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] > '1998-01-01T12:00:00.000'");
        }

        public override async Task New_DateTime_is_parameterized_when_from_closure(bool async)
        {
            await base.New_DateTime_is_parameterized_when_from_closure(async);

            AssertSql(
                @"@__p_0='1998-01-01T12:00:00' (Nullable = true) (DbType = DateTime)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] > @__p_0",
                //
                @"@__p_0='1998-01-01T11:00:00' (Nullable = true) (DbType = DateTime)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] > @__p_0");
        }

        public override async Task Environment_newline_is_funcletized(bool async)
        {
            await base.Environment_newline_is_funcletized(async);

            AssertSql(
                @"@__NewLine_0='
' (Size = 5)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (@__NewLine_0 = N'') OR (CHARINDEX(@__NewLine_0, [c].[CustomerID]) > 0)");
        }

        public override async Task String_concat_with_navigation1(bool async)
        {
            await base.String_concat_with_navigation1(async);

            AssertSql(
                @"SELECT (COALESCE([o].[CustomerID], N'') + N' ') + COALESCE([c].[City], N'')
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]");
        }

        public override async Task String_concat_with_navigation2(bool async)
        {
            await base.String_concat_with_navigation2(async);

            AssertSql(
                @"SELECT (COALESCE([c].[City], N'') + N' ') + COALESCE([c].[City], N'')
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]");
        }

        public override void Select_bitwise_or()
        {
            base.Select_bitwise_or();

            AssertSql(
                @"SELECT [c].[CustomerID], CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END | CASE
    WHEN [c].[CustomerID] = N'ANATR' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Value]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void Select_bitwise_or_multiple()
        {
            base.Select_bitwise_or_multiple();

            AssertSql(
                @"SELECT [c].[CustomerID], (CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END | CASE
    WHEN [c].[CustomerID] = N'ANATR' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END) | CASE
    WHEN [c].[CustomerID] = N'ANTON' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Value]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void Select_bitwise_and()
        {
            base.Select_bitwise_and();

            AssertSql(
                @"SELECT [c].[CustomerID], CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END & CASE
    WHEN [c].[CustomerID] = N'ANATR' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Value]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void Select_bitwise_and_or()
        {
            base.Select_bitwise_and_or();

            AssertSql(
                @"SELECT [c].[CustomerID], (CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END & CASE
    WHEN [c].[CustomerID] = N'ANATR' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END) | CASE
    WHEN [c].[CustomerID] = N'ANTON' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Value]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override async Task Where_bitwise_or_with_logical_or(bool async)
        {
            await base.Where_bitwise_or_with_logical_or(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ((CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END | CASE
    WHEN [c].[CustomerID] = N'ANATR' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END) = CAST(1 AS bit)) OR ([c].[CustomerID] = N'ANTON')");
        }

        public override async Task Where_bitwise_and_with_logical_and(bool async)
        {
            await base.Where_bitwise_and_with_logical_and(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ((CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END & CASE
    WHEN [c].[CustomerID] = N'ANATR' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END) = CAST(1 AS bit)) AND ([c].[CustomerID] = N'ANTON')");
        }

        public override async Task Where_bitwise_or_with_logical_and(bool async)
        {
            await base.Where_bitwise_or_with_logical_and(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ((CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END | CASE
    WHEN [c].[CustomerID] = N'ANATR' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END) = CAST(1 AS bit)) AND ([c].[Country] = N'Germany')");
        }

        public override async Task Where_bitwise_and_with_logical_or(bool async)
        {
            await base.Where_bitwise_and_with_logical_or(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ((CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END & CASE
    WHEN [c].[CustomerID] = N'ANATR' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END) = CAST(1 AS bit)) OR ([c].[CustomerID] = N'ANTON')");
        }

        public override async Task Where_bitwise_binary_not(bool async)
        {
            await base.Where_bitwise_binary_not(async);

            AssertSql(
                @"@__negatedId_0='-10249'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ~[o].[OrderID] = @__negatedId_0");
        }

        public override async Task Where_bitwise_binary_and(bool async)
        {
            await base.Where_bitwise_binary_and(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[OrderID] & 10248) = 10248");
        }

        public override async Task Where_bitwise_binary_or(bool async)
        {
            await base.Where_bitwise_binary_or(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[OrderID] | 10248) = 10248");
        }

        public override void Select_bitwise_or_with_logical_or()
        {
            base.Select_bitwise_or_with_logical_or();

            AssertSql(
                @"SELECT [c].[CustomerID], CASE
    WHEN ((CASE
        WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END | CASE
        WHEN [c].[CustomerID] = N'ANATR' THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END) = CAST(1 AS bit)) OR ([c].[CustomerID] = N'ANTON') THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
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
        WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END & CASE
        WHEN [c].[CustomerID] = N'ANATR' THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END) = CAST(1 AS bit)) AND ([c].[CustomerID] = N'ANTON') THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Value]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override async Task Handle_materialization_properly_when_more_than_two_query_sources_are_involved(bool async)
        {
            await base.Handle_materialization_properly_when_more_than_two_query_sources_are_involved(async);

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
CROSS JOIN [Employees] AS [e]
ORDER BY [c].[CustomerID]");
        }

        public override async Task Parameter_extraction_short_circuits_1(bool async)
        {
            await base.Parameter_extraction_short_circuits_1(async);

            AssertSql(
                @"@__dateFilter_Value_Month_0='7'
@__dateFilter_Value_Year_1='1996'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[OrderID] < 10400) AND (([o].[OrderDate] IS NOT NULL AND (DATEPART(month, [o].[OrderDate]) = @__dateFilter_Value_Month_0)) AND (DATEPART(year, [o].[OrderDate]) = @__dateFilter_Value_Year_1))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10400");
        }

        public override async Task Parameter_extraction_short_circuits_2(bool async)
        {
            await base.Parameter_extraction_short_circuits_2(async);

            AssertSql(
                @"@__dateFilter_Value_Month_0='7'
@__dateFilter_Value_Year_1='1996'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[OrderID] < 10400) AND (([o].[OrderDate] IS NOT NULL AND (DATEPART(month, [o].[OrderDate]) = @__dateFilter_Value_Month_0)) AND (DATEPART(year, [o].[OrderDate]) = @__dateFilter_Value_Year_1))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE 0 = 1");
        }

        public override async Task Parameter_extraction_short_circuits_3(bool async)
        {
            await base.Parameter_extraction_short_circuits_3(async);

            AssertSql(
                @"@__dateFilter_Value_Month_0='7'
@__dateFilter_Value_Year_1='1996'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[OrderID] < 10400) OR (([o].[OrderDate] IS NOT NULL AND (DATEPART(month, [o].[OrderDate]) = @__dateFilter_Value_Month_0)) AND (DATEPART(year, [o].[OrderDate]) = @__dateFilter_Value_Year_1))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override async Task Subquery_member_pushdown_does_not_change_original_subquery_model(bool async)
        {
            await base.Subquery_member_pushdown_does_not_change_original_subquery_model(async);

            AssertSql(
                @"@__p_0='3'

SELECT [t].[OrderID] AS [OrderId], (
    SELECT TOP(1) [c].[City]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = [t].[CustomerID]) AS [City]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]
ORDER BY (
    SELECT TOP(1) [c0].[City]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = [t].[CustomerID])");
        }

        public override async Task Subquery_member_pushdown_does_not_change_original_subquery_model2(bool async)
        {
            await base.Subquery_member_pushdown_does_not_change_original_subquery_model2(async);

            AssertSql(
                @"@__p_0='3'

SELECT [t].[OrderID] AS [OrderId], (
    SELECT TOP(1) [c].[City]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = [t].[CustomerID]) AS [City]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]
ORDER BY (
    SELECT TOP(1) [c0].[City]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = [t].[CustomerID])");
        }

        public override async Task Query_expression_with_to_string_and_contains(bool async)
        {
            await base.Query_expression_with_to_string_and_contains(async);

            AssertSql(
                @"SELECT [o].[CustomerID]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL AND (CHARINDEX(N'10', CONVERT(VARCHAR(10), [o].[EmployeeID])) > 0)");
        }

        public override async Task Select_expression_long_to_string(bool async)
        {
            await base.Select_expression_long_to_string(async);

            AssertSql(
                @"SELECT CONVERT(VARCHAR(20), CAST([o].[OrderID] AS bigint)) AS [ShipName]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL");
        }

        public override async Task Select_expression_int_to_string(bool async)
        {
            await base.Select_expression_int_to_string(async);

            AssertSql(
                @"SELECT CONVERT(VARCHAR(11), [o].[OrderID]) AS [ShipName]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL");
        }

        public override async Task ToString_with_formatter_is_evaluated_on_the_client(bool async)
        {
            await base.ToString_with_formatter_is_evaluated_on_the_client(async);

            AssertSql(
                @"SELECT [o].[OrderID]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL",
                //
                @"SELECT [o].[OrderID]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL");
        }

        public override async Task Select_expression_other_to_string(bool async)
        {
            await base.Select_expression_other_to_string(async);

            AssertSql(
                @"SELECT CONVERT(VARCHAR(100), [o].[OrderDate]) AS [ShipName]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL");
        }

        public override async Task Select_expression_date_add_year(bool async)
        {
            await base.Select_expression_date_add_year(async);

            AssertSql(
                @"SELECT DATEADD(year, CAST(1 AS int), [o].[OrderDate]) AS [OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL");
        }

        public override async Task Select_expression_datetime_add_month(bool async)
        {
            await base.Select_expression_datetime_add_month(async);

            AssertSql(
                @"SELECT DATEADD(month, CAST(1 AS int), [o].[OrderDate]) AS [OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL");
        }

        public override async Task Select_expression_datetime_add_hour(bool async)
        {
            await base.Select_expression_datetime_add_hour(async);

            AssertSql(
                @"SELECT DATEADD(hour, CAST(1.0E0 AS int), [o].[OrderDate]) AS [OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL");
        }

        public override async Task Select_expression_datetime_add_minute(bool async)
        {
            await base.Select_expression_datetime_add_minute(async);

            AssertSql(
                @"SELECT DATEADD(minute, CAST(1.0E0 AS int), [o].[OrderDate]) AS [OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL");
        }

        public override async Task Select_expression_datetime_add_second(bool async)
        {
            await base.Select_expression_datetime_add_second(async);

            AssertSql(
                @"SELECT DATEADD(second, CAST(1.0E0 AS int), [o].[OrderDate]) AS [OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL");
        }

        public override async Task Select_expression_date_add_milliseconds_above_the_range(bool async)
        {
            await base.Select_expression_date_add_milliseconds_above_the_range(async);

            AssertSql(
                @"SELECT [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL");
        }

        public override async Task Select_expression_date_add_milliseconds_below_the_range(bool async)
        {
            await base.Select_expression_date_add_milliseconds_below_the_range(async);

            AssertSql(
                @"SELECT [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL");
        }

        public override async Task Select_expression_date_add_milliseconds_large_number_divided(bool async)
        {
            await base.Select_expression_date_add_milliseconds_large_number_divided(async);

            AssertSql(
                @"@__millisecondsPerDay_0='86400000'

SELECT DATEADD(millisecond, CAST(CAST((CAST(DATEPART(millisecond, [o].[OrderDate]) AS bigint) % @__millisecondsPerDay_0) AS float) AS int), DATEADD(day, CAST(CAST((CAST(DATEPART(millisecond, [o].[OrderDate]) AS bigint) / @__millisecondsPerDay_0) AS float) AS int), [o].[OrderDate])) AS [OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL");
        }

        public override async Task Select_expression_references_are_updated_correctly_with_subquery(bool async)
        {
            await base.Select_expression_references_are_updated_correctly_with_subquery(async);

            AssertSql(
                @"@__nextYear_0='2017'

SELECT DISTINCT DATEPART(year, [o].[OrderDate])
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL AND (DATEPART(year, [o].[OrderDate]) < @__nextYear_0)");
        }

        public override void DefaultIfEmpty_without_group_join()
        {
            base.DefaultIfEmpty_without_group_join();

            AssertSql(
                @"SELECT [t].[CustomerID]
FROM (
    SELECT NULL AS [empty]
) AS [empty]
LEFT JOIN (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'London'
) AS [t] ON 1 = 1
WHERE [t].[CustomerID] IS NOT NULL");
        }

        public override async Task DefaultIfEmpty_in_subquery(bool async)
        {
            await base.DefaultIfEmpty_in_subquery(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [o].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [o].[OrderID] IS NOT NULL");
        }

        public override async Task DefaultIfEmpty_in_subquery_not_correlated(bool async)
        {
            await base.DefaultIfEmpty_in_subquery_not_correlated(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [t0].[OrderID]
FROM [Customers] AS [c]
CROSS JOIN (
    SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
    FROM (
        SELECT NULL AS [empty]
    ) AS [empty]
    LEFT JOIN (
        SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [o].[OrderID] > 15000
    ) AS [t] ON 1 = 1
) AS [t0]");
        }

        public override async Task DefaultIfEmpty_in_subquery_nested(bool async)
        {
            await base.DefaultIfEmpty_in_subquery_nested(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [t0].[OrderID], [o0].[OrderDate]
FROM [Customers] AS [c]
CROSS JOIN (
    SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
    FROM (
        SELECT NULL AS [empty]
    ) AS [empty]
    LEFT JOIN (
        SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [o].[OrderID] > 15000
    ) AS [t] ON 1 = 1
) AS [t0]
LEFT JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE ([c].[City] = N'Seattle') AND ([t0].[OrderID] IS NOT NULL AND [o0].[OrderID] IS NOT NULL)
ORDER BY [t0].[OrderID], [o0].[OrderDate]");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task OrderBy_skip_take(bool async)
        {
            await base.OrderBy_skip_take(async);

            AssertSql(
                @"@__p_0='5'
@__p_1='8'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactTitle], [c].[ContactName]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task OrderBy_skip_skip_take(bool async)
        {
            await base.OrderBy_skip_skip_take(async);

            AssertSql(
                @"@__p_0='5'
@__p_1='8'
@__p_2='3'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactTitle], [c].[ContactName]
    OFFSET @__p_0 ROWS
) AS [t]
ORDER BY [t].[ContactTitle], [t].[ContactName]
OFFSET @__p_1 ROWS FETCH NEXT @__p_2 ROWS ONLY");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task OrderBy_skip_take_take(bool async)
        {
            await base.OrderBy_skip_take_take(async);

            AssertSql(
                @"@__p_2='3'
@__p_0='5'
@__p_1='8'

SELECT TOP(@__p_2) [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactTitle], [c].[ContactName]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
ORDER BY [t].[ContactTitle], [t].[ContactName]");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task OrderBy_skip_take_take_take_take(bool async)
        {
            await base.OrderBy_skip_take_take_take_take(async);

            AssertSql(
                @"@__p_0='5'
@__p_3='8'
@__p_2='10'
@__p_1='15'

SELECT TOP(@__p_0) [t1].[CustomerID], [t1].[Address], [t1].[City], [t1].[CompanyName], [t1].[ContactName], [t1].[ContactTitle], [t1].[Country], [t1].[Fax], [t1].[Phone], [t1].[PostalCode], [t1].[Region]
FROM (
    SELECT TOP(@__p_3) [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
    FROM (
        SELECT TOP(@__p_2) [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
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
        public override async Task OrderBy_skip_take_skip_take_skip(bool async)
        {
            await base.OrderBy_skip_take_skip_take_skip(async);

            AssertSql(
                @"@__p_0='5'
@__p_1='15'
@__p_2='2'
@__p_3='8'

SELECT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
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
OFFSET @__p_0 ROWS");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task OrderBy_skip_take_distinct(bool async)
        {
            await base.OrderBy_skip_take_distinct(async);

            AssertSql(
                @"@__p_0='5'
@__p_1='15'

SELECT DISTINCT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactTitle], [c].[ContactName]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task OrderBy_coalesce_take_distinct(bool async)
        {
            await base.OrderBy_coalesce_take_distinct(async);

            AssertSql(
                @"@__p_0='15'

SELECT DISTINCT [t].[ProductID], [t].[Discontinued], [t].[ProductName], [t].[SupplierID], [t].[UnitPrice], [t].[UnitsInStock]
FROM (
    SELECT TOP(@__p_0) [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock], COALESCE([p].[UnitPrice], 0.0) AS [c]
    FROM [Products] AS [p]
    ORDER BY COALESCE([p].[UnitPrice], 0.0)
) AS [t]");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task OrderBy_coalesce_skip_take_distinct(bool async)
        {
            await base.OrderBy_coalesce_skip_take_distinct(async);

            AssertSql(
                @"@__p_0='5'
@__p_1='15'

SELECT DISTINCT [t].[ProductID], [t].[Discontinued], [t].[ProductName], [t].[SupplierID], [t].[UnitPrice], [t].[UnitsInStock]
FROM (
    SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock], COALESCE([p].[UnitPrice], 0.0) AS [c]
    FROM [Products] AS [p]
    ORDER BY COALESCE([p].[UnitPrice], 0.0)
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task OrderBy_coalesce_skip_take_distinct_take(bool async)
        {
            await base.OrderBy_coalesce_skip_take_distinct_take(async);

            AssertSql(
                @"@__p_0='5'
@__p_1='15'

SELECT DISTINCT TOP(@__p_0) [t].[ProductID], [t].[Discontinued], [t].[ProductName], [t].[SupplierID], [t].[UnitPrice], [t].[UnitsInStock]
FROM (
    SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock], COALESCE([p].[UnitPrice], 0.0) AS [c]
    FROM [Products] AS [p]
    ORDER BY COALESCE([p].[UnitPrice], 0.0)
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public override async Task OrderBy_skip_take_distinct_orderby_take(bool async)
        {
            await base.OrderBy_skip_take_distinct_orderby_take(async);

            AssertSql(
                @"@__p_2='8'
@__p_0='5'
@__p_1='15'

SELECT TOP(@__p_2) [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT DISTINCT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
    FROM (
        SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactTitle], [c].[ContactName]
        OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
    ) AS [t]
) AS [t0]
ORDER BY [t0].[ContactTitle]");
        }

        public override async Task No_orderby_added_for_fully_translated_manually_constructed_LOJ(bool async)
        {
            await base.No_orderby_added_for_fully_translated_manually_constructed_LOJ(async);

            AssertSql(
                @"SELECT [e].[City] AS [City1], [e0].[City] AS [City2]
FROM [Employees] AS [e]
LEFT JOIN [Employees] AS [e0] ON [e].[EmployeeID] = [e0].[ReportsTo]");
        }

        public override async Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ(bool async)
        {
            await base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID] AS [Id1], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]");
        }

        public override async Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition1(
            bool async)
        {
            await base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition1(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID] AS [Id1], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON ([o].[CustomerID] = [c].[CustomerID]) AND ([o].[OrderID] = 10000)");
        }

        public override async Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2(
            bool async)
        {
            await base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID] AS [Id1], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON ([o].[OrderID] = 10000) AND ([o].[CustomerID] = [c].[CustomerID])");
        }

        public override async Task Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ(bool async)
        {
            await base.Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ(async);

            AssertSql(
                @"SELECT [e1].[EmployeeID], [e1].[City] AS [City1], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title], [e2].[EmployeeID], [e2].[City], [e2].[Country], [e2].[FirstName], [e2].[ReportsTo], [e2].[Title]
FROM [Employees] AS [e1]
LEFT JOIN [Employees] AS [e2] ON [e1].[EmployeeID] = [e2].[ReportsTo]
ORDER BY [e1].[EmployeeID]");
        }

        public override async Task Contains_with_DateTime_Date(bool async)
        {
            await base.Contains_with_DateTime_Date(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE CONVERT(date, [o].[OrderDate]) IN ('1996-07-04T00:00:00.000', '1996-07-16T00:00:00.000')",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE CONVERT(date, [o].[OrderDate]) IN ('1996-07-04T00:00:00.000')");
        }

        public override async Task Contains_with_subquery_involving_join_binds_to_correct_table(bool async)
        {
            await base.Contains_with_subquery_involving_join_binds_to_correct_table(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[OrderID] > 11000) AND [o].[OrderID] IN (
    SELECT [o0].[OrderID]
    FROM [Order Details] AS [o0]
    INNER JOIN [Products] AS [p] ON [o0].[ProductID] = [p].[ProductID]
    WHERE [p].[ProductName] = N'Chai'
)");
        }

        public override async Task Complex_query_with_repeated_query_model_compiles_correctly(bool async)
        {
            await base.Complex_query_with_repeated_query_model_compiles_correctly(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] = N'ALFKI') AND EXISTS (
    SELECT 1
    FROM [Customers] AS [c0]
    WHERE EXISTS (
        SELECT 1
        FROM [Customers] AS [c1]))");
        }

        public override async Task Complex_query_with_repeated_nested_query_model_compiles_correctly(bool async)
        {
            await base.Complex_query_with_repeated_nested_query_model_compiles_correctly(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] = N'ALFKI') AND EXISTS (
    SELECT 1
    FROM [Customers] AS [c0]
    WHERE EXISTS (
        SELECT 1
        FROM [Customers] AS [c1]
        WHERE EXISTS (
            SELECT DISTINCT 1
            FROM (
                SELECT TOP(10) [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
                FROM [Customers] AS [c2]
                ORDER BY [c2].[CustomerID]
            ) AS [t])))");
        }

        public override async Task Anonymous_member_distinct_where(bool async)
        {
            await base.Anonymous_member_distinct_where(async);

            AssertSql(
                @"SELECT DISTINCT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task Anonymous_member_distinct_orderby(bool async)
        {
            await base.Anonymous_member_distinct_orderby(async);

            AssertSql(
                @"SELECT [t].[CustomerID]
FROM (
    SELECT DISTINCT [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [t]
ORDER BY [t].[CustomerID]");
        }

        public override async Task Anonymous_member_distinct_result(bool async)
        {
            await base.Anonymous_member_distinct_result(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [t]
WHERE [t].[CustomerID] LIKE N'A%'");
        }

        public override async Task Anonymous_complex_distinct_where(bool async)
        {
            await base.Anonymous_complex_distinct_where(async);

            AssertSql(
                @"SELECT DISTINCT [c].[CustomerID] + COALESCE([c].[City], N'') AS [A]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] + COALESCE([c].[City], N'')) = N'ALFKIBerlin'");
        }

        public override async Task Anonymous_complex_distinct_orderby(bool async)
        {
            await base.Anonymous_complex_distinct_orderby(async);

            AssertSql(
                @"SELECT [t].[c] AS [A]
FROM (
    SELECT DISTINCT [c].[CustomerID] + COALESCE([c].[City], N'') AS [c]
    FROM [Customers] AS [c]
) AS [t]
ORDER BY [t].[c]");
        }

        public override async Task Anonymous_complex_distinct_result(bool async)
        {
            await base.Anonymous_complex_distinct_result(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[CustomerID] + COALESCE([c].[City], N'') AS [c]
    FROM [Customers] AS [c]
) AS [t]
WHERE [t].[c] IS NOT NULL AND ([t].[c] LIKE N'A%')");
        }

        public override async Task Anonymous_complex_orderby(bool async)
        {
            await base.Anonymous_complex_orderby(async);

            AssertSql(
                @"SELECT [c].[CustomerID] + COALESCE([c].[City], N'') AS [A]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID] + COALESCE([c].[City], N'')");
        }

        public override async Task Anonymous_subquery_orderby(bool async)
        {
            await base.Anonymous_subquery_orderby(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID] DESC) AS [A]
FROM [Customers] AS [c]
WHERE (
    SELECT COUNT(*)
    FROM [Orders] AS [o0]
    WHERE [c].[CustomerID] = [o0].[CustomerID]) > 1
ORDER BY (
    SELECT TOP(1) [o1].[OrderDate]
    FROM [Orders] AS [o1]
    WHERE [c].[CustomerID] = [o1].[CustomerID]
    ORDER BY [o1].[OrderID] DESC)");
        }

        public override async Task DTO_member_distinct_where(bool async)
        {
            await base.DTO_member_distinct_where(async);

            AssertSql(
                @"SELECT DISTINCT [c].[CustomerID] AS [Property]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task DTO_member_distinct_orderby(bool async)
        {
            await base.DTO_member_distinct_orderby(async);

            AssertSql(
                @"SELECT [t].[CustomerID] AS [Property]
FROM (
    SELECT DISTINCT [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [t]
ORDER BY [t].[CustomerID]");
        }

        public override async Task DTO_member_distinct_result(bool async)
        {
            await base.DTO_member_distinct_result(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [t]
WHERE [t].[CustomerID] LIKE N'A%'");
        }

        public override async Task DTO_complex_distinct_where(bool async)
        {
            await base.DTO_complex_distinct_where(async);

            AssertSql(
                @"SELECT DISTINCT [c].[CustomerID] + COALESCE([c].[City], N'') AS [Property]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] + COALESCE([c].[City], N'')) = N'ALFKIBerlin'");
        }

        public override async Task DTO_complex_distinct_orderby(bool async)
        {
            await base.DTO_complex_distinct_orderby(async);

            AssertSql(
                @"SELECT [t].[c] AS [Property]
FROM (
    SELECT DISTINCT [c].[CustomerID] + COALESCE([c].[City], N'') AS [c]
    FROM [Customers] AS [c]
) AS [t]
ORDER BY [t].[c]");
        }

        public override async Task DTO_complex_distinct_result(bool async)
        {
            await base.DTO_complex_distinct_result(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[CustomerID] + COALESCE([c].[City], N'') AS [c]
    FROM [Customers] AS [c]
) AS [t]
WHERE [t].[c] IS NOT NULL AND ([t].[c] LIKE N'A%')");
        }

        public override async Task DTO_complex_orderby(bool async)
        {
            await base.DTO_complex_orderby(async);

            // issue #18775
            //            AssertSql(
            //                @"SELECT [c].[CustomerID] + [c].[City] AS [Property]
            //FROM [Customers] AS [c]
            //ORDER BY [Property]");
        }

        public override async Task DTO_subquery_orderby(bool async)
        {
            await base.DTO_subquery_orderby(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID] DESC) AS [Property]
FROM [Customers] AS [c]
WHERE (
    SELECT COUNT(*)
    FROM [Orders] AS [o0]
    WHERE [c].[CustomerID] = [o0].[CustomerID]) > 1
ORDER BY (
    SELECT TOP(1) [o1].[OrderDate]
    FROM [Orders] AS [o1]
    WHERE [c].[CustomerID] = [o1].[CustomerID]
    ORDER BY [o1].[OrderID] DESC)");
        }

        public override async Task Include_with_orderby_skip_preserves_ordering(bool async)
        {
            await base.Include_with_orderby_skip_preserves_ordering(async);

            AssertSql(
                @"@__p_0='40'
@__p_1='5'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE ([c].[CustomerID] <> N'VAFFE') AND ([c].[CustomerID] <> N'DRACD')
    ORDER BY [c].[City], [c].[CustomerID]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[City], [t].[CustomerID], [o].[OrderID]");
        }

        public override async Task Int16_parameter_can_be_used_for_int_column(bool async)
        {
            await base.Int16_parameter_can_be_used_for_int_column(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] = 10300");
        }

        public override async Task Subquery_is_null_translated_correctly(bool async)
        {
            await base.Subquery_is_null_translated_correctly(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT TOP(1) [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID] DESC) IS NULL");
        }

        public override async Task Subquery_is_not_null_translated_correctly(bool async)
        {
            await base.Subquery_is_not_null_translated_correctly(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT TOP(1) [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID] DESC) IS NOT NULL");
        }

        public override async Task Select_take_average(bool async)
        {
            await base.Select_take_average(async);

            AssertSql(
                @"@__p_0='10'

SELECT AVG(CAST([t].[OrderID] AS float))
FROM (
    SELECT TOP(@__p_0) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]");
        }

        public override async Task Select_take_count(bool async)
        {
            await base.Select_take_count(async);

            AssertSql(
                @"@__p_0='7'

SELECT COUNT(*)
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
) AS [t]");
        }

        public override async Task Select_orderBy_take_count(bool async)
        {
            await base.Select_orderBy_take_count(async);

            AssertSql(
                @"@__p_0='7'

SELECT COUNT(*)
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[Country]
) AS [t]");
        }

        public override async Task Select_take_long_count(bool async)
        {
            await base.Select_take_long_count(async);

            AssertSql(
                @"@__p_0='7'

SELECT COUNT_BIG(*)
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
) AS [t]");
        }

        public override async Task Select_orderBy_take_long_count(bool async)
        {
            await base.Select_orderBy_take_long_count(async);

            AssertSql(
                @"@__p_0='7'

SELECT COUNT_BIG(*)
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[Country]
) AS [t]");
        }

        public override async Task Select_take_max(bool async)
        {
            await base.Select_take_max(async);

            AssertSql(
                @"@__p_0='10'

SELECT MAX([t].[OrderID])
FROM (
    SELECT TOP(@__p_0) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]");
        }

        public override async Task Select_take_min(bool async)
        {
            await base.Select_take_min(async);

            AssertSql(
                @"@__p_0='10'

SELECT MIN([t].[OrderID])
FROM (
    SELECT TOP(@__p_0) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]");
        }

        public override async Task Select_take_sum(bool async)
        {
            await base.Select_take_sum(async);

            AssertSql(
                @"@__p_0='10'

SELECT SUM([t].[OrderID])
FROM (
    SELECT TOP(@__p_0) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]");
        }

        public override async Task Select_skip_average(bool async)
        {
            await base.Select_skip_average(async);

            AssertSql(
                @"@__p_0='10'

SELECT AVG(CAST([t].[OrderID] AS float))
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS
) AS [t]");
        }

        public override async Task Select_skip_count(bool async)
        {
            await base.Select_skip_count(async);

            AssertSql(
                @"@__p_0='7'

SELECT COUNT(*)
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY (SELECT 1)
    OFFSET @__p_0 ROWS
) AS [t]");
        }

        public override async Task Select_orderBy_skip_count(bool async)
        {
            await base.Select_orderBy_skip_count(async);

            AssertSql(
                @"@__p_0='7'

SELECT COUNT(*)
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[Country]
    OFFSET @__p_0 ROWS
) AS [t]");
        }

        public override async Task Select_skip_long_count(bool async)
        {
            await base.Select_skip_long_count(async);

            AssertSql(
                @"@__p_0='7'

SELECT COUNT_BIG(*)
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY (SELECT 1)
    OFFSET @__p_0 ROWS
) AS [t]");
        }

        public override async Task Select_orderBy_skip_long_count(bool async)
        {
            await base.Select_orderBy_skip_long_count(async);

            AssertSql(
                @"@__p_0='7'

SELECT COUNT_BIG(*)
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[Country]
    OFFSET @__p_0 ROWS
) AS [t]");
        }

        public override async Task Select_skip_max(bool async)
        {
            await base.Select_skip_max(async);

            AssertSql(
                @"@__p_0='10'

SELECT MAX([t].[OrderID])
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS
) AS [t]");
        }

        public override async Task Select_skip_min(bool async)
        {
            await base.Select_skip_min(async);

            AssertSql(
                @"@__p_0='10'

SELECT MIN([t].[OrderID])
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS
) AS [t]");
        }

        public override async Task Select_skip_sum(bool async)
        {
            await base.Select_skip_sum(async);

            AssertSql(
                @"@__p_0='10'

SELECT SUM([t].[OrderID])
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS
) AS [t]");
        }

        public override async Task Select_distinct_average(bool async)
        {
            await base.Select_distinct_average(async);

            AssertSql(
                @"SELECT AVG(CAST([t].[OrderID] AS float))
FROM (
    SELECT DISTINCT [o].[OrderID]
    FROM [Orders] AS [o]
) AS [t]");
        }

        public override async Task Select_distinct_count(bool async)
        {
            await base.Select_distinct_count(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
) AS [t]");
        }

        public override async Task Select_distinct_long_count(bool async)
        {
            await base.Select_distinct_long_count(async);

            AssertSql(
                @"SELECT COUNT_BIG(*)
FROM (
    SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
) AS [t]");
        }

        public override async Task Select_distinct_max(bool async)
        {
            await base.Select_distinct_max(async);

            AssertSql(
                @"SELECT MAX([t].[OrderID])
FROM (
    SELECT DISTINCT [o].[OrderID]
    FROM [Orders] AS [o]
) AS [t]");
        }

        public override async Task Select_distinct_min(bool async)
        {
            await base.Select_distinct_min(async);

            AssertSql(
                @"SELECT MIN([t].[OrderID])
FROM (
    SELECT DISTINCT [o].[OrderID]
    FROM [Orders] AS [o]
) AS [t]");
        }

        public override async Task Select_distinct_sum(bool async)
        {
            await base.Select_distinct_sum(async);

            AssertSql(
                @"SELECT SUM([t].[OrderID])
FROM (
    SELECT DISTINCT [o].[OrderID]
    FROM [Orders] AS [o]
) AS [t]");
        }

        public override async Task Comparing_to_fixed_string_parameter(bool async)
        {
            await base.Comparing_to_fixed_string_parameter(async);

            AssertSql(
                @"@__prefix_0='A' (Size = 4000)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE (@__prefix_0 = N'') OR (LEFT([c].[CustomerID], LEN(@__prefix_0)) = @__prefix_0)");
        }

        public override async Task Comparing_entities_using_Equals(bool async)
        {
            await base.Comparing_entities_using_Equals(async);

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Id1], [c0].[CustomerID] AS [Id2]
FROM [Customers] AS [c]
CROSS JOIN [Customers] AS [c0]
WHERE ([c].[CustomerID] LIKE N'ALFKI%') AND ([c].[CustomerID] = [c0].[CustomerID])
ORDER BY [c].[CustomerID]");
        }

        public override async Task Comparing_different_entity_types_using_Equals(bool async)
        {
            await base.Comparing_different_entity_types_using_Equals(async);

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
WHERE 0 = 1");
        }

        public override async Task Comparing_entity_to_null_using_Equals(bool async)
        {
            await base.Comparing_entity_to_null_using_Equals(async);

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID]");
        }

        public override async Task Comparing_navigations_using_Equals(bool async)
        {
            await base.Comparing_navigations_using_Equals(async);

            AssertSql(
                @"SELECT [o].[OrderID] AS [Id1], [o0].[OrderID] AS [Id2]
FROM [Orders] AS [o]
CROSS JOIN [Orders] AS [o0]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
LEFT JOIN [Customers] AS [c0] ON [o0].[CustomerID] = [c0].[CustomerID]
WHERE ([o].[CustomerID] IS NOT NULL AND ([o].[CustomerID] LIKE N'A%')) AND (([c].[CustomerID] = [c0].[CustomerID]) OR ([c].[CustomerID] IS NULL AND [c0].[CustomerID] IS NULL))
ORDER BY [o].[OrderID], [o0].[OrderID]");
        }

        public override async Task Comparing_navigations_using_static_Equals(bool async)
        {
            await base.Comparing_navigations_using_static_Equals(async);

            AssertSql(
                @"SELECT [o].[OrderID] AS [Id1], [o0].[OrderID] AS [Id2]
FROM [Orders] AS [o]
CROSS JOIN [Orders] AS [o0]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
LEFT JOIN [Customers] AS [c0] ON [o0].[CustomerID] = [c0].[CustomerID]
WHERE ([o].[CustomerID] IS NOT NULL AND ([o].[CustomerID] LIKE N'A%')) AND (([c].[CustomerID] = [c0].[CustomerID]) OR ([c].[CustomerID] IS NULL AND [c0].[CustomerID] IS NULL))
ORDER BY [o].[OrderID], [o0].[OrderID]");
        }

        public override async Task Comparing_non_matching_entities_using_Equals(bool async)
        {
            await base.Comparing_non_matching_entities_using_Equals(async);

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Id1], [o].[OrderID] AS [Id2]
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
WHERE 0 = 1");
        }

        public override async Task Comparing_non_matching_collection_navigations_using_Equals(bool async)
        {
            await base.Comparing_non_matching_collection_navigations_using_Equals(async);

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Id1], [o].[OrderID] AS [Id2]
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
WHERE 0 = 1");
        }

        public override async Task Comparing_collection_navigation_to_null(bool async)
        {
            await base.Comparing_collection_navigation_to_null(async);

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE 0 = 1");
        }

        public override async Task Comparing_collection_navigation_to_null_complex(bool async)
        {
            await base.Comparing_collection_navigation_to_null_complex(async);

            AssertSql(
                @"SELECT [o].[ProductID], [o].[OrderID]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
WHERE ([o].[OrderID] < 10250) AND [c].[CustomerID] IS NOT NULL
ORDER BY [o].[OrderID], [o].[ProductID]");
        }

        public override async Task Compare_collection_navigation_with_itself(bool async)
        {
            await base.Compare_collection_navigation_with_itself(async);

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'");
        }

        public override async Task Compare_two_collection_navigations_with_different_query_sources(bool async)
        {
            await base.Compare_two_collection_navigations_with_different_query_sources(async);

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Id1], [c0].[CustomerID] AS [Id2]
FROM [Customers] AS [c]
CROSS JOIN [Customers] AS [c0]
WHERE (([c].[CustomerID] = N'ALFKI') AND ([c0].[CustomerID] = N'ALFKI')) AND ([c].[CustomerID] = [c0].[CustomerID])");
        }

        public override async Task Compare_two_collection_navigations_using_equals(bool async)
        {
            await base.Compare_two_collection_navigations_using_equals(async);

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Id1], [c0].[CustomerID] AS [Id2]
FROM [Customers] AS [c]
CROSS JOIN [Customers] AS [c0]
WHERE (([c].[CustomerID] = N'ALFKI') AND ([c0].[CustomerID] = N'ALFKI')) AND ([c].[CustomerID] = [c0].[CustomerID])");
        }

        public override async Task Compare_two_collection_navigations_with_different_property_chains(bool async)
        {
            await base.Compare_two_collection_navigations_with_different_property_chains(async);

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Id1], [o].[OrderID] AS [Id2]
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
LEFT JOIN [Customers] AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
WHERE ([c].[CustomerID] = N'ALFKI') AND ([c].[CustomerID] = [c0].[CustomerID])
ORDER BY [c].[CustomerID], [o].[OrderID]");
        }

        public override async Task OrderBy_ThenBy_same_column_different_direction(bool async)
        {
            await base.OrderBy_ThenBy_same_column_different_direction(async);

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID]");
        }

        public override async Task OrderBy_OrderBy_same_column_different_direction(bool async)
        {
            await base.OrderBy_OrderBy_same_column_different_direction(async);

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY [c].[CustomerID] DESC");
        }

        public override async Task Complex_nested_query_doesnt_try_binding_to_grandparent_when_parent_returns_complex_result(bool async)
        {
            await base.Complex_nested_query_doesnt_try_binding_to_grandparent_when_parent_returns_complex_result(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [t].[c], [t].[CustomerID], [t].[OrderID]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT (
        SELECT COUNT(*)
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]) AS [c], [c].[CustomerID], [o0].[OrderID]
    FROM [Orders] AS [o0]
    WHERE [c].[CustomerID] = [o0].[CustomerID]
) AS [t]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID], [t].[OrderID]");
        }

        public override async Task Complex_nested_query_properly_binds_to_grandparent_when_parent_returns_scalar_result(bool async)
        {
            await base.Complex_nested_query_properly_binds_to_grandparent_when_parent_returns_scalar_result(async);

            AssertSql(
                @"SELECT [c].[CustomerID], (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE ([c].[CustomerID] = [o].[CustomerID]) AND ((
        SELECT COUNT(*)
        FROM [Orders] AS [o0]
        WHERE [c].[CustomerID] = [o0].[CustomerID]) > 0)) AS [OuterOrders]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task OrderBy_Dto_projection_skip_take(bool async)
        {
            await base.OrderBy_Dto_projection_skip_take(async);

            AssertSql(
                @"@__p_0='5'
@__p_1='10'

SELECT [c].[CustomerID] AS [Id]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY");
        }

        public override void Streaming_chained_sync_query()
        {
            base.Streaming_chained_sync_query();

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]",
                //
                @"@_outer_CustomerID='ALFKI' (Size = 5)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = @_outer_CustomerID",
                //
                @"SELECT [y.Customer].[CustomerID], [y.Customer].[Address], [y.Customer].[City], [y.Customer].[CompanyName], [y.Customer].[ContactName], [y.Customer].[ContactTitle], [y.Customer].[Country], [y.Customer].[Fax], [y.Customer].[Phone], [y.Customer].[PostalCode], [y.Customer].[Region]
FROM [Customers] AS [y.Customer]",
                //
                @"@_outer_CustomerID='ANATR' (Size = 5)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = @_outer_CustomerID");
        }

        public override async Task Join_take_count_works(bool async)
        {
            await base.Join_take_count_works(async);

            AssertSql(
                @"@__p_0='5'

SELECT COUNT(*)
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [t].[CustomerID] AS [CustomerID0], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
    FROM [Orders] AS [o]
    INNER JOIN (
        SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        WHERE [c].[CustomerID] = N'ALFKI'
    ) AS [t] ON [o].[CustomerID] = [t].[CustomerID]
    WHERE ([o].[OrderID] > 690) AND ([o].[OrderID] < 710)
) AS [t0]");
        }

        public override async Task OrderBy_empty_list_contains(bool async)
        {
            await base.OrderBy_empty_list_contains(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task OrderBy_empty_list_does_not_contains(bool async)
        {
            await base.OrderBy_empty_list_does_not_contains(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Manual_expression_tree_typed_null_equality()
        {
            base.Manual_expression_tree_typed_null_equality();

            AssertSql(
                @"SELECT [c].[City]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] < 10300");
        }

        public override async Task Let_subquery_with_multiple_occurrences(bool async)
        {
            await base.Let_subquery_with_multiple_occurrences(async);

            AssertSql(
                @"SELECT (
    SELECT COUNT(*)
    FROM [Order Details] AS [o]
    WHERE ([o0].[OrderID] = [o].[OrderID]) AND ([o].[Quantity] < CAST(10 AS smallint))) AS [Count]
FROM [Orders] AS [o0]
WHERE EXISTS (
    SELECT 1
    FROM [Order Details] AS [o1]
    WHERE ([o0].[OrderID] = [o1].[OrderID]) AND ([o1].[Quantity] < CAST(10 AS smallint)))");
        }

        public override async Task Let_entity_equality_to_null(bool async)
        {
            await base.Let_entity_equality_to_null(async);

            AssertSql(
                @"SELECT [c].[CustomerID], (
    SELECT TOP(1) [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderDate]) AS [OrderDate]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] LIKE N'A%') AND (
    SELECT TOP(1) [o0].[OrderID]
    FROM [Orders] AS [o0]
    WHERE [c].[CustomerID] = [o0].[CustomerID]
    ORDER BY [o0].[OrderDate]) IS NOT NULL");
        }

        public override async Task Let_entity_equality_to_other_entity(bool async)
        {
            await base.Let_entity_equality_to_other_entity(async);

            AssertSql(
                @"SELECT [c].[CustomerID], (
    SELECT TOP(1) [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderDate]) AS [A]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] LIKE N'A%') AND (((
    SELECT TOP(1) [o0].[OrderID]
    FROM [Orders] AS [o0]
    WHERE [c].[CustomerID] = [o0].[CustomerID]
    ORDER BY [o0].[OrderDate]) <> 0) OR (
    SELECT TOP(1) [o0].[OrderID]
    FROM [Orders] AS [o0]
    WHERE [c].[CustomerID] = [o0].[CustomerID]
    ORDER BY [o0].[OrderDate]) IS NULL)");
        }

        public override async Task Collection_navigation_equal_to_null_for_subquery(bool async)
        {
            await base.Collection_navigation_equal_to_null_for_subquery(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT TOP(1) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]) IS NULL");
        }

        public override async Task Dependent_to_principal_navigation_equal_to_null_for_subquery(bool async)
        {
            await base.Dependent_to_principal_navigation_equal_to_null_for_subquery(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT TOP(1) [c0].[CustomerID]
    FROM [Orders] AS [o]
    LEFT JOIN [Customers] AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]) IS NULL");
        }

        public override async Task Collection_navigation_equality_rewrite_for_subquery(bool async)
        {
            await base.Collection_navigation_equality_rewrite_for_subquery(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] LIKE N'A%') AND (((
    SELECT TOP(1) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
    ORDER BY [o].[OrderID]) = (
    SELECT TOP(1) [o0].[OrderID]
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] > 10500
    ORDER BY [o0].[OrderID])) OR ((
    SELECT TOP(1) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
    ORDER BY [o].[OrderID]) IS NULL AND (
    SELECT TOP(1) [o0].[OrderID]
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] > 10500
    ORDER BY [o0].[OrderID]) IS NULL))");
        }

        public override async Task Inner_parameter_in_nested_lambdas_gets_preserved(bool async)
        {
            await base.Inner_parameter_in_nested_lambdas_gets_preserved(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE ([c].[CustomerID] = [o].[CustomerID]) AND ([c].[CustomerID] = [o].[CustomerID])) > 0");
        }

        public override async Task Convert_to_nullable_on_nullable_value_is_ignored(bool async)
        {
            await base.Convert_to_nullable_on_nullable_value_is_ignored(async);

            AssertSql(
                @"SELECT [o].[OrderDate]
FROM [Orders] AS [o]");
        }

        public override async Task Navigation_inside_interpolated_string_is_expanded(bool async)
        {
            await base.Navigation_inside_interpolated_string_is_expanded(async);

            AssertSql(
                @"SELECT [c].[City]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]");
        }

        public override async Task OrderBy_object_type_server_evals(bool async)
        {
            await base.OrderBy_object_type_server_evals(async);

            AssertSql(
                @"@__p_0='0'
@__p_1='20'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [o].[OrderID], [o].[OrderDate], [c].[CustomerID], [c].[City]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY");
        }

        public override async Task AsQueryable_in_query_server_evals(bool async)
        {
            await base.AsQueryable_in_query_server_evals(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [t].[OrderDate], [t].[OrderID]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT TOP(1) [o].[OrderDate], [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE ([c].[CustomerID] = [o].[CustomerID]) AND (DATEPART(year, [o].[OrderDate]) = 1998)
    ORDER BY [o].[OrderID]
) AS [t]
ORDER BY [c].[CustomerID], [t].[OrderID]");
        }

        public override async Task Subquery_DefaultIfEmpty_Any(bool async)
        {
            await base.Subquery_DefaultIfEmpty_Any(async);

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM (
            SELECT NULL AS [empty]
        ) AS [empty]
        LEFT JOIN (
            SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
            FROM [Employees] AS [e]
            WHERE [e].[EmployeeID] = -1
        ) AS [t] ON 1 = 1) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Projection_skip_collection_projection(bool async)
        {
            await base.Projection_skip_collection_projection(async);

            AssertSql(
                @"@__p_0='5'

SELECT [t].[OrderID], [o0].[ProductID], [o0].[OrderID]
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS
) AS [t]
LEFT JOIN [Order Details] AS [o0] ON [t].[OrderID] = [o0].[OrderID]
ORDER BY [t].[OrderID], [o0].[OrderID], [o0].[ProductID]");
        }

        public override async Task Projection_take_collection_projection(bool async)
        {
            await base.Projection_take_collection_projection(async);

            AssertSql(
                @"@__p_0='10'

SELECT [t].[OrderID], [o0].[ProductID], [o0].[OrderID]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
    ORDER BY [o].[OrderID]
) AS [t]
LEFT JOIN [Order Details] AS [o0] ON [t].[OrderID] = [o0].[OrderID]
ORDER BY [t].[OrderID], [o0].[OrderID], [o0].[ProductID]");
        }

        public override async Task Projection_skip_take_collection_projection(bool async)
        {
            await base.Projection_skip_take_collection_projection(async);

            AssertSql(
                @"@__p_0='5'
@__p_1='10'

SELECT [t].[OrderID], [o0].[ProductID], [o0].[OrderID]
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
LEFT JOIN [Order Details] AS [o0] ON [t].[OrderID] = [o0].[OrderID]
ORDER BY [t].[OrderID], [o0].[OrderID], [o0].[ProductID]");
        }

        public override async Task Projection_skip_projection(bool async)
        {
            await base.Projection_skip_projection(async);

            AssertSql(
                @"@__p_0='5'

SELECT [c].[City]
FROM (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS
) AS [t]
LEFT JOIN [Customers] AS [c] ON [t].[CustomerID] = [c].[CustomerID]
ORDER BY [t].[OrderID]");
        }

        public override async Task Projection_take_projection(bool async)
        {
            await base.Projection_take_projection(async);

            AssertSql(
                @"@__p_0='10'

SELECT [c].[City]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
    ORDER BY [o].[OrderID]
) AS [t]
LEFT JOIN [Customers] AS [c] ON [t].[CustomerID] = [c].[CustomerID]
ORDER BY [t].[OrderID]");
        }

        public override async Task Projection_skip_take_projection(bool async)
        {
            await base.Projection_skip_take_projection(async);

            AssertSql(
                @"@__p_0='5'
@__p_1='10'

SELECT [c].[City]
FROM (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
LEFT JOIN [Customers] AS [c] ON [t].[CustomerID] = [c].[CustomerID]
ORDER BY [t].[OrderID]");
        }

        public override async Task Collection_projection_skip(bool async)
        {
            await base.Collection_projection_skip(async);

            AssertSql(
                @"@__p_0='5'

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS
) AS [t]
LEFT JOIN [Order Details] AS [o0] ON [t].[OrderID] = [o0].[OrderID]
ORDER BY [t].[OrderID], [o0].[OrderID], [o0].[ProductID]");
        }

        public override async Task Collection_projection_take(bool async)
        {
            await base.Collection_projection_take(async);

            AssertSql(
                @"@__p_0='10'

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
    ORDER BY [o].[OrderID]
) AS [t]
LEFT JOIN [Order Details] AS [o0] ON [t].[OrderID] = [o0].[OrderID]
ORDER BY [t].[OrderID], [o0].[OrderID], [o0].[ProductID]");
        }

        public override async Task Collection_projection_skip_take(bool async)
        {
            await base.Collection_projection_skip_take(async);

            AssertSql(
                @"@__p_0='5'
@__p_1='10'

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
    ORDER BY [o].[OrderID]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
LEFT JOIN [Order Details] AS [o0] ON [t].[OrderID] = [o0].[OrderID]
ORDER BY [t].[OrderID], [o0].[OrderID], [o0].[ProductID]");
        }

        public override async Task Anonymous_projection_skip_empty_collection_FirstOrDefault(bool async)
        {
            await base.Anonymous_projection_skip_empty_collection_FirstOrDefault(async);

            AssertSql(
                @"@__p_0='0'

SELECT [t1].[OrderID], [t1].[CustomerID], [t1].[EmployeeID], [t1].[OrderDate]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'FISSA'
    ORDER BY (SELECT 1)
    OFFSET @__p_0 ROWS
) AS [t]
LEFT JOIN (
    SELECT [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate]
    FROM (
        SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], ROW_NUMBER() OVER(PARTITION BY [o].[CustomerID] ORDER BY [o].[OrderID]) AS [row]
        FROM [Orders] AS [o]
    ) AS [t0]
    WHERE [t0].[row] <= 1
) AS [t1] ON [t].[CustomerID] = [t1].[CustomerID]");
        }

        public override async Task Anonymous_projection_take_empty_collection_FirstOrDefault(bool async)
        {
            await base.Anonymous_projection_take_empty_collection_FirstOrDefault(async);

            AssertSql(
                @"@__p_0='1'

SELECT [t1].[OrderID], [t1].[CustomerID], [t1].[EmployeeID], [t1].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'FISSA'
) AS [t]
LEFT JOIN (
    SELECT [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate]
    FROM (
        SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], ROW_NUMBER() OVER(PARTITION BY [o].[CustomerID] ORDER BY [o].[OrderID]) AS [row]
        FROM [Orders] AS [o]
    ) AS [t0]
    WHERE [t0].[row] <= 1
) AS [t1] ON [t].[CustomerID] = [t1].[CustomerID]");
        }

        public override async Task Anonymous_projection_skip_take_empty_collection_FirstOrDefault(bool async)
        {
            await base.Anonymous_projection_skip_take_empty_collection_FirstOrDefault(async);

            AssertSql(
                @"@__p_0='0'
@__p_1='1'

SELECT [t1].[OrderID], [t1].[CustomerID], [t1].[EmployeeID], [t1].[OrderDate]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'FISSA'
    ORDER BY (SELECT 1)
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
LEFT JOIN (
    SELECT [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate]
    FROM (
        SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], ROW_NUMBER() OVER(PARTITION BY [o].[CustomerID] ORDER BY [o].[OrderID]) AS [row]
        FROM [Orders] AS [o]
    ) AS [t0]
    WHERE [t0].[row] <= 1
) AS [t1] ON [t].[CustomerID] = [t1].[CustomerID]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
