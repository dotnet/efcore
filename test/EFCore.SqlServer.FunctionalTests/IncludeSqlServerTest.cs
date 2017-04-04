// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class IncludeSqlServerTest : IncludeTestBase<NorthwindQuerySqlServerFixture>
    {
        private readonly ITestOutputHelper _testOutputHelper;

        private bool SupportsOffset => TestEnvironment.GetFlag(nameof(SqlServerCondition.SupportsOffset)) ?? true;

        public IncludeSqlServerTest(NorthwindQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            _testOutputHelper = testOutputHelper;

            //TestSqlLoggerFactory.CaptureOutput(_testOutputHelper);
        }

        public override void Include_list(bool useString)
        {
            base.Include_list(useString);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
ORDER BY [p].[ProductID]

SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE EXISTS (
    SELECT 1
    FROM [Products] AS [p]
    WHERE [o].[ProductID] = [p].[ProductID])
ORDER BY [o].[ProductID]",
                Sql);
        }

        public override void Include_collection(bool useString)
        {
            base.Include_collection(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]",
                Sql);
        }

        public override void Include_collection_with_last(bool useString)
        {
            base.Include_collection_with_last(useString);

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CompanyName] DESC, [c].[CustomerID] DESC

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(1) [c0].[CustomerID], [c0].[CompanyName]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CompanyName] DESC, [c0].[CustomerID] DESC
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CompanyName] DESC, [t].[CustomerID] DESC",
                Sql);
        }

        public override void Include_collection_with_last_no_orderby(bool useString)
        {
            base.Include_collection_with_last_no_orderby(useString);

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID] DESC

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(1) [c0].[CustomerID]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID] DESC
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID] DESC",
                Sql);
        }

        public override void Include_collection_skip_no_order_by(bool useString)
        {
            base.Include_collection_skip_no_order_by(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0: 10

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
OFFSET @__p_0 ROWS

@__p_0: 10

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
    OFFSET @__p_0 ROWS
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]",
                    Sql);
            }
        }

        public override void Include_collection_take_no_order_by(bool useString)
        {
            base.Include_collection_take_no_order_by(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0: 10

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

@__p_0: 10

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(@__p_0) [c0].[CustomerID]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]",
                    Sql);
            }
        }

        public override void Include_collection_skip_take_no_order_by(bool useString)
        {
            base.Include_collection_skip_take_no_order_by(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0: 10
@__p_1: 5

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY

@__p_0: 10
@__p_1: 5

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]",
                    Sql);
            }
        }

        public override void Include_reference_and_collection(bool useString)
        {
            base.Include_reference_and_collection(useString);

            if (!useString)
            {
                AssertSql(
                    @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
ORDER BY [o].[OrderID]

SELECT [o.OrderDetails].[OrderID], [o.OrderDetails].[ProductID], [o.OrderDetails].[Discount], [o.OrderDetails].[Quantity], [o.OrderDetails].[UnitPrice]
FROM [Order Details] AS [o.OrderDetails]
INNER JOIN (
    SELECT [o0].[OrderID]
    FROM [Orders] AS [o0]
) AS [t] ON [o.OrderDetails].[OrderID] = [t].[OrderID]
ORDER BY [t].[OrderID]",
                    Sql);
            }
        }

        public override void Include_references_multi_level(bool useString)
        {
            base.Include_references_multi_level(useString);

            if (!useString)
            {
                AssertSql(
                    @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o.Order].[OrderID], [o.Order].[CustomerID], [o.Order].[EmployeeID], [o.Order].[OrderDate], [o.Order.Customer].[CustomerID], [o.Order.Customer].[Address], [o.Order.Customer].[City], [o.Order.Customer].[CompanyName], [o.Order.Customer].[ContactName], [o.Order.Customer].[ContactTitle], [o.Order.Customer].[Country], [o.Order.Customer].[Fax], [o.Order.Customer].[Phone], [o.Order.Customer].[PostalCode], [o.Order.Customer].[Region]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o.Order] ON [o].[OrderID] = [o.Order].[OrderID]
LEFT JOIN [Customers] AS [o.Order.Customer] ON [o.Order].[CustomerID] = [o.Order.Customer].[CustomerID]",
                    Sql);
            }
            else
            {
                AssertSql(
                    @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]",
                    Sql);
            }
        }

        public override void Include_multiple_references_multi_level(bool useString)
        {
            base.Include_multiple_references_multi_level(useString);

            if (!useString)
            {
                AssertSql(
                    @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o.Product].[ProductID], [o.Product].[Discontinued], [o.Product].[ProductName], [o.Product].[UnitPrice], [o.Product].[UnitsInStock], [o.Order].[OrderID], [o.Order].[CustomerID], [o.Order].[EmployeeID], [o.Order].[OrderDate], [o.Order.Customer].[CustomerID], [o.Order.Customer].[Address], [o.Order.Customer].[City], [o.Order.Customer].[CompanyName], [o.Order.Customer].[ContactName], [o.Order.Customer].[ContactTitle], [o.Order.Customer].[Country], [o.Order.Customer].[Fax], [o.Order.Customer].[Phone], [o.Order.Customer].[PostalCode], [o.Order.Customer].[Region]
FROM [Order Details] AS [o]
INNER JOIN [Products] AS [o.Product] ON [o].[ProductID] = [o.Product].[ProductID]
INNER JOIN [Orders] AS [o.Order] ON [o].[OrderID] = [o.Order].[OrderID]
LEFT JOIN [Customers] AS [o.Order.Customer] ON [o.Order].[CustomerID] = [o.Order.Customer].[CustomerID]",
                    Sql);
            }
        }

        public override void Include_multiple_references_multi_level_reverse(bool useString)
        {
            base.Include_multiple_references_multi_level_reverse(useString);

            if (!useString)
            {
                AssertSql(
                    @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o.Order].[OrderID], [o.Order].[CustomerID], [o.Order].[EmployeeID], [o.Order].[OrderDate], [o.Order.Customer].[CustomerID], [o.Order.Customer].[Address], [o.Order.Customer].[City], [o.Order.Customer].[CompanyName], [o.Order.Customer].[ContactName], [o.Order.Customer].[ContactTitle], [o.Order.Customer].[Country], [o.Order.Customer].[Fax], [o.Order.Customer].[Phone], [o.Order.Customer].[PostalCode], [o.Order.Customer].[Region], [o.Product].[ProductID], [o.Product].[Discontinued], [o.Product].[ProductName], [o.Product].[UnitPrice], [o.Product].[UnitsInStock]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o.Order] ON [o].[OrderID] = [o.Order].[OrderID]
LEFT JOIN [Customers] AS [o.Order.Customer] ON [o.Order].[CustomerID] = [o.Order.Customer].[CustomerID]
INNER JOIN [Products] AS [o.Product] ON [o].[ProductID] = [o.Product].[ProductID]",
                    Sql);
            }
        }

        public override void Include_references_and_collection_multi_level(bool useString)
        {
            base.Include_references_and_collection_multi_level(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[CustomerID]

SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate]
FROM [Orders] AS [o1]
WHERE EXISTS (
    SELECT 1
    FROM [Order Details] AS [o]
    INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
    LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
    WHERE [o1].[CustomerID] = [c].[CustomerID])
ORDER BY [o1].[CustomerID]",
                Sql);
        }

        public override void Include_multi_level_reference_and_collection_predicate(bool useString)
        {
            base.Include_multi_level_reference_and_collection_predicate(useString);

            AssertSql(
                @"SELECT TOP(2) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] = 10248
ORDER BY [c].[CustomerID]

SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o0]
INNER JOIN (
    SELECT DISTINCT TOP(2) [c].[CustomerID]
    FROM [Orders] AS [o]
    LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
    WHERE [o].[OrderID] = 10248
    ORDER BY [c].[CustomerID]
) AS [c0] ON [o0].[CustomerID] = [c0].[CustomerID]
ORDER BY [c0].[CustomerID]",
                Sql);
        }

        public override void Include_multi_level_collection_and_then_include_reference_predicate(bool useString)
        {
            base.Include_multi_level_collection_and_then_include_reference_predicate(useString);

            AssertSql(
                @"SELECT TOP(2) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] = 10248
ORDER BY [o].[OrderID]

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Order Details] AS [o0]
INNER JOIN (
    SELECT DISTINCT TOP(2) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] = 10248
    ORDER BY [o].[OrderID]
) AS [o1] ON [o0].[OrderID] = [o1].[OrderID]
INNER JOIN [Products] AS [p] ON [o0].[ProductID] = [p].[ProductID]
ORDER BY [o1].[OrderID]",
                Sql);
        }

        public override void Include_collection_alias_generation(bool useString)
        {
            base.Include_collection_alias_generation(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
ORDER BY [o].[OrderID]

SELECT [o.OrderDetails].[OrderID], [o.OrderDetails].[ProductID], [o.OrderDetails].[Discount], [o.OrderDetails].[Quantity], [o.OrderDetails].[UnitPrice]
FROM [Order Details] AS [o.OrderDetails]
INNER JOIN (
    SELECT [o0].[OrderID]
    FROM [Orders] AS [o0]
) AS [t] ON [o.OrderDetails].[OrderID] = [t].[OrderID]
ORDER BY [t].[OrderID]",
                Sql);
        }

        public override void Include_collection_order_by_collection_column(bool useString)
        {
            base.Include_collection_order_by_collection_column(useString);

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'W' + N'%' AND (CHARINDEX(N'W', [c].[CustomerID]) = 1)
ORDER BY (
    SELECT TOP(1) [oo].[OrderDate]
    FROM [Orders] AS [oo]
    WHERE [c].[CustomerID] = [oo].[CustomerID]
    ORDER BY [oo].[OrderDate] DESC
) DESC, [c].[CustomerID]

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(1) [c0].[CustomerID], (
        SELECT TOP(1) [oo1].[OrderDate]
        FROM [Orders] AS [oo1]
        WHERE [c0].[CustomerID] = [oo1].[CustomerID]
        ORDER BY [oo1].[OrderDate] DESC
    ) AS [c], (
        SELECT TOP(1) [oo0].[OrderDate]
        FROM [Orders] AS [oo0]
        WHERE [c0].[CustomerID] = [oo0].[CustomerID]
        ORDER BY [oo0].[OrderDate] DESC
    ) AS [c0]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] LIKE N'W' + N'%' AND (CHARINDEX(N'W', [c0].[CustomerID]) = 1)
    ORDER BY [c0] DESC, [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[c] DESC, [t].[CustomerID]",
                Sql);
        }

        public override void Include_collection_order_by_key(bool useString)
        {
            base.Include_collection_order_by_key(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]",
                Sql);
        }

        public override void Include_collection_order_by_non_key(bool useString)
        {
            base.Include_collection_order_by_non_key(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[City], [c].[CustomerID]

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID], [c0].[City]
    FROM [Customers] AS [c0]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[City], [t].[CustomerID]",
                Sql);
        }

        public override void Include_collection_order_by_non_key_with_take(bool useString)
        {
            base.Include_collection_order_by_non_key_with_take(useString);

            AssertSql(
                @"@__p_0: 10

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactTitle], [c].[CustomerID]

@__p_0: 10

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(@__p_0) [c0].[CustomerID], [c0].[ContactTitle]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[ContactTitle], [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[ContactTitle], [t].[CustomerID]",
                Sql);
        }

        public override void Include_collection_order_by_non_key_with_skip(bool useString)
        {
            base.Include_collection_order_by_non_key_with_skip(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0: 10

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactTitle], [c].[CustomerID]
OFFSET @__p_0 ROWS

@__p_0: 10

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID], [c0].[ContactTitle]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[ContactTitle], [c0].[CustomerID]
    OFFSET @__p_0 ROWS
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[ContactTitle], [t].[CustomerID]",
                    Sql);
            }
        }

        public override void Include_collection_order_by_non_key_with_first_or_default(bool useString)
        {
            base.Include_collection_order_by_non_key_with_first_or_default(useString);

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CompanyName] DESC, [c].[CustomerID]

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(1) [c0].[CustomerID], [c0].[CompanyName]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CompanyName] DESC, [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CompanyName] DESC, [t].[CustomerID]",
                Sql);
        }

        public override void Include_collection_order_by_subquery(bool useString)
        {
            base.Include_collection_order_by_subquery(useString);

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY (
    SELECT TOP(1) [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[EmployeeID]
), [c].[CustomerID]

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(1) [c0].[CustomerID], (
        SELECT TOP(1) [o1].[OrderDate]
        FROM [Orders] AS [o1]
        WHERE [c0].[CustomerID] = [o1].[CustomerID]
        ORDER BY [o1].[EmployeeID]
    ) AS [c], (
        SELECT TOP(1) [o0].[OrderDate]
        FROM [Orders] AS [o0]
        WHERE [c0].[CustomerID] = [o0].[CustomerID]
        ORDER BY [o0].[EmployeeID]
    ) AS [c0]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
    ORDER BY [c0], [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[c], [t].[CustomerID]",
                Sql);
        }

        public override void Include_collection_as_no_tracking(bool useString)
        {
            base.Include_collection_as_no_tracking(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]",
                Sql);
        }

        public override void Include_collection_principal_already_tracked(bool useString)
        {
            base.Include_collection_principal_already_tracked(useString);

            AssertSql(
                @"SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'

SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(1) [c0].[CustomerID]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
    ORDER BY [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]",
                Sql);
        }

        public override void Include_collection_principal_already_tracked_as_no_tracking(bool useString)
        {
            base.Include_collection_principal_already_tracked_as_no_tracking(useString);

            AssertSql(
                @"SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'

SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(1) [c0].[CustomerID]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
    ORDER BY [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]",
                Sql);
        }

        public override void Include_collection_with_filter(bool useString)
        {
            base.Include_collection_with_filter(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]",
                Sql);
        }

        public override void Include_collection_with_filter_reordered(bool useString)
        {
            base.Include_collection_with_filter_reordered(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]",
                Sql);
        }

        public override void Include_collection_then_include_collection(bool useString)
        {
            base.Include_collection_then_include_collection(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c]
    WHERE [o].[CustomerID] = [c].[CustomerID])
ORDER BY [o].[CustomerID], [o].[OrderID]

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Order Details] AS [o0]
INNER JOIN (
    SELECT DISTINCT [o].[CustomerID], [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        WHERE [o].[CustomerID] = [c].[CustomerID])
) AS [o1] ON [o0].[OrderID] = [o1].[OrderID]
ORDER BY [o1].[CustomerID], [o1].[OrderID]",
                Sql);
        }

        public override void Include_collection_when_projection(bool useString)
        {
            base.Include_collection_when_projection(useString);

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Include_collection_on_join_clause_with_filter(bool useString)
        {
            base.Include_collection_on_join_clause_with_filter(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT DISTINCT [c0].[CustomerID]
    FROM [Customers] AS [c0]
    INNER JOIN [Orders] AS [o0] ON [c0].[CustomerID] = [o0].[CustomerID]
    WHERE [c0].[CustomerID] = N'ALFKI'
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]",
                Sql);
        }

        public override void Include_collection_on_additional_from_clause_with_filter(bool useString)
        {
            base.Include_collection_on_additional_from_clause_with_filter(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c1]
CROSS JOIN [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT DISTINCT [c0].[CustomerID]
    FROM [Customers] AS [c10]
    CROSS JOIN [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]",
                Sql);
        }

        public override void Include_collection_on_additional_from_clause(bool useString)
        {
            base.Include_collection_on_additional_from_clause(useString);

            AssertSql(
                @"@__p_0: 5

SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
FROM (
    SELECT TOP(@__p_0) [c].*
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [t]
CROSS JOIN [Customers] AS [c2]
ORDER BY [c2].[CustomerID], [t].[CustomerID]

@__p_0: 5

SELECT [c2.Orders].[OrderID], [c2.Orders].[CustomerID], [c2.Orders].[EmployeeID], [c2.Orders].[OrderDate]
FROM [Orders] AS [c2.Orders]
INNER JOIN (
    SELECT DISTINCT [c20].[CustomerID]
    FROM (
        SELECT TOP(@__p_0) [c0].*
        FROM [Customers] AS [c0]
        ORDER BY [c0].[CustomerID]
    ) AS [t0]
    CROSS JOIN [Customers] AS [c20]
) AS [t1] ON [c2.Orders].[CustomerID] = [t1].[CustomerID]
ORDER BY [t1].[CustomerID]",
                Sql);
        }

        public override void Include_duplicate_collection(bool useString)
        {
            base.Include_duplicate_collection(useString);

            if (SupportsOffset)
            {
                Assert.Contains(
                    @"@__p_0: 2

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [t]
CROSS JOIN (
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [t0]
ORDER BY [t].[CustomerID], [t0].[CustomerID]",
                    Sql);

                Assert.Contains(
                    @"@__p_0: 2

SELECT DISTINCT [t3].[CustomerID]
FROM (
    SELECT TOP(@__p_0) [c3].*
    FROM [Customers] AS [c3]
    ORDER BY [c3].[CustomerID]
) AS [t3]
CROSS JOIN (
    SELECT [c4].*
    FROM [Customers] AS [c4]
    ORDER BY [c4].[CustomerID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [t4]
ORDER BY [t3].[CustomerID]",
                    Sql);

                Assert.Contains(
                    @"SELECT [c1.Orders].[OrderID], [c1.Orders].[CustomerID], [c1.Orders].[EmployeeID], [c1.Orders].[OrderDate]
FROM [Orders] AS [c1.Orders]",
                    Sql);

                Assert.Contains(
                    @"@__p_0: 2

SELECT DISTINCT [t8].[CustomerID], [t7].[CustomerID]
FROM (
    SELECT TOP(@__p_0) [c7].*
    FROM [Customers] AS [c7]
    ORDER BY [c7].[CustomerID]
) AS [t7]
CROSS JOIN (
    SELECT [c8].*
    FROM [Customers] AS [c8]
    ORDER BY [c8].[CustomerID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [t8]
ORDER BY [t7].[CustomerID]",
                    Sql);
            }
        }

        public override void Include_duplicate_collection_result_operator(bool useString)
        {
            base.Include_duplicate_collection_result_operator(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_1: 1
@__p_0: 2

SELECT TOP(@__p_1) [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [t]
CROSS JOIN (
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [t0]
ORDER BY [t].[CustomerID], [t0].[CustomerID]

@__p_1: 1
@__p_0: 2

SELECT [c1.Orders].[OrderID], [c1.Orders].[CustomerID], [c1.Orders].[EmployeeID], [c1.Orders].[OrderDate]
FROM [Orders] AS [c1.Orders]
INNER JOIN (
    SELECT DISTINCT TOP(@__p_1) [t1].[CustomerID]
    FROM (
        SELECT TOP(@__p_0) [c1].*
        FROM [Customers] AS [c1]
        ORDER BY [c1].[CustomerID]
    ) AS [t1]
    CROSS JOIN (
        SELECT [c2].*
        FROM [Customers] AS [c2]
        ORDER BY [c2].[CustomerID]
        OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
    ) AS [t2]
    ORDER BY [t1].[CustomerID]
) AS [t3] ON [c1.Orders].[CustomerID] = [t3].[CustomerID]
ORDER BY [t3].[CustomerID]

@__p_1: 1
@__p_0: 2

SELECT [c2.Orders].[OrderID], [c2.Orders].[CustomerID], [c2.Orders].[EmployeeID], [c2.Orders].[OrderDate]
FROM [Orders] AS [c2.Orders]
INNER JOIN (
    SELECT DISTINCT TOP(@__p_1) [t5].[CustomerID], [t4].[CustomerID] AS [c0]
    FROM (
        SELECT TOP(@__p_0) [c3].*
        FROM [Customers] AS [c3]
        ORDER BY [c3].[CustomerID]
    ) AS [t4]
    CROSS JOIN (
        SELECT [c4].*
        FROM [Customers] AS [c4]
        ORDER BY [c4].[CustomerID]
        OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
    ) AS [t5]
    ORDER BY [t4].[CustomerID], [t5].[CustomerID]
) AS [t6] ON [c2.Orders].[CustomerID] = [t6].[CustomerID]
ORDER BY [t6].[c0], [t6].[CustomerID]",
                    Sql);
            }
        }

        public override void Include_collection_on_join_clause_with_order_by_and_filter(bool useString)
        {
            base.Include_collection_on_join_clause_with_order_by_and_filter(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[City], [c].[CustomerID]

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT DISTINCT [c0].[CustomerID], [c0].[City]
    FROM [Customers] AS [c0]
    INNER JOIN [Orders] AS [o0] ON [c0].[CustomerID] = [o0].[CustomerID]
    WHERE [c0].[CustomerID] = N'ALFKI'
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[City], [t].[CustomerID]",
                Sql);
        }

        public override void Include_collection_when_groupby(bool useString)
        {
            base.Include_collection_when_groupby(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[City], [c].[CustomerID]

SELECT [c.Orders0].[OrderID], [c.Orders0].[CustomerID], [c.Orders0].[EmployeeID], [c.Orders0].[OrderDate]
FROM [Orders] AS [c.Orders0]
INNER JOIN (
    SELECT [c1].[CustomerID], [c1].[City]
    FROM [Customers] AS [c1]
    WHERE [c1].[CustomerID] = N'ALFKI'
) AS [t0] ON [c.Orders0].[CustomerID] = [t0].[CustomerID]
ORDER BY [t0].[City], [t0].[CustomerID]",
                Sql);
        }

        public override void Include_collection_on_additional_from_clause2(bool useString)
        {
            base.Include_collection_on_additional_from_clause2(useString);

            AssertSql(
                @"@__p_0: 5

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [t]
CROSS JOIN [Customers] AS [c2]
ORDER BY [t].[CustomerID]",
                Sql);
        }

        public override void Include_where_skip_take_projection(bool useString)
        {
            base.Include_where_skip_take_projection(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0: 1
@__p_1: 2

SELECT [od.Order].[CustomerID]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [od.Order] ON [od].[OrderID] = [od.Order].[OrderID]
WHERE [od].[Quantity] = 10
ORDER BY [od].[OrderID], [od].[ProductID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY",
                    Sql);
            }
        }

        public override void Include_duplicate_collection_result_operator2(bool useString)
        {
            base.Include_duplicate_collection_result_operator2(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_1: 1
@__p_0: 2

SELECT TOP(@__p_1) [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [t]
CROSS JOIN (
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [t0]
ORDER BY [t].[CustomerID]

@__p_1: 1
@__p_0: 2

SELECT [c1.Orders].[OrderID], [c1.Orders].[CustomerID], [c1.Orders].[EmployeeID], [c1.Orders].[OrderDate]
FROM [Orders] AS [c1.Orders]
INNER JOIN (
    SELECT DISTINCT TOP(@__p_1) [t1].[CustomerID]
    FROM (
        SELECT TOP(@__p_0) [c1].*
        FROM [Customers] AS [c1]
        ORDER BY [c1].[CustomerID]
    ) AS [t1]
    CROSS JOIN (
        SELECT [c2].*
        FROM [Customers] AS [c2]
        ORDER BY [c2].[CustomerID]
        OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
    ) AS [t2]
    ORDER BY [t1].[CustomerID]
) AS [t3] ON [c1.Orders].[CustomerID] = [t3].[CustomerID]
ORDER BY [t3].[CustomerID]",
                    Sql);
            }
        }

        public override void Include_reference(bool useString)
        {
            base.Include_reference(useString);

            if (!useString)
            {
                AssertSql(
                    @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]",
                    Sql);
            }
        }

        public override void Include_multiple_references(bool useString)
        {
            base.Include_multiple_references(useString);

            if (!useString)
            {
                AssertSql(
                    @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o.Product].[ProductID], [o.Product].[Discontinued], [o.Product].[ProductName], [o.Product].[UnitPrice], [o.Product].[UnitsInStock], [o.Order].[OrderID], [o.Order].[CustomerID], [o.Order].[EmployeeID], [o.Order].[OrderDate]
FROM [Order Details] AS [o]
INNER JOIN [Products] AS [o.Product] ON [o].[ProductID] = [o.Product].[ProductID]
INNER JOIN [Orders] AS [o.Order] ON [o].[OrderID] = [o.Order].[OrderID]",
                    Sql);
            }
        }

        public override void Include_reference_alias_generation(bool useString)
        {
            base.Include_reference_alias_generation(useString);

            if (!useString)
            {
                AssertSql(
                    @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o.Order].[OrderID], [o.Order].[CustomerID], [o.Order].[EmployeeID], [o.Order].[OrderDate]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o.Order] ON [o].[OrderID] = [o.Order].[OrderID]",
                    Sql);
            }
        }

        public override void Include_duplicate_reference(bool useString)
        {
            base.Include_duplicate_reference(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0: 2

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], [o1.Customer].[CustomerID], [o1.Customer].[Address], [o1.Customer].[City], [o1.Customer].[CompanyName], [o1.Customer].[ContactName], [o1.Customer].[ContactTitle], [o1.Customer].[Country], [o1.Customer].[Fax], [o1.Customer].[Phone], [o1.Customer].[PostalCode], [o1.Customer].[Region], [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate], [o2.Customer].[CustomerID], [o2.Customer].[Address], [o2.Customer].[City], [o2.Customer].[CompanyName], [o2.Customer].[ContactName], [o2.Customer].[ContactTitle], [o2.Customer].[Country], [o2.Customer].[Fax], [o2.Customer].[Phone], [o2.Customer].[PostalCode], [o2.Customer].[Region]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[CustomerID]
) AS [t]
LEFT JOIN [Customers] AS [o1.Customer] ON [t].[CustomerID] = [o1.Customer].[CustomerID]
CROSS JOIN (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    ORDER BY [o0].[CustomerID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [t0]
LEFT JOIN [Customers] AS [o2.Customer] ON [t0].[CustomerID] = [o2.Customer].[CustomerID]
ORDER BY [t].[CustomerID]",
                    Sql);
            }
        }

        public override void Include_duplicate_reference2(bool useString)
        {
            base.Include_duplicate_reference2(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0: 2

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], [o1.Customer].[CustomerID], [o1.Customer].[Address], [o1.Customer].[City], [o1.Customer].[CompanyName], [o1.Customer].[ContactName], [o1.Customer].[ContactTitle], [o1.Customer].[Country], [o1.Customer].[Fax], [o1.Customer].[Phone], [o1.Customer].[PostalCode], [o1.Customer].[Region], [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]
LEFT JOIN [Customers] AS [o1.Customer] ON [t].[CustomerID] = [o1.Customer].[CustomerID]
CROSS JOIN (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    ORDER BY [o0].[OrderID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [t0]
ORDER BY [t].[OrderID]",
                    Sql);
            }
        }

        public override void Include_duplicate_reference3(bool useString)
        {
            base.Include_duplicate_reference3(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0: 2

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate], [o2.Customer].[CustomerID], [o2.Customer].[Address], [o2.Customer].[City], [o2.Customer].[CompanyName], [o2.Customer].[ContactName], [o2.Customer].[ContactTitle], [o2.Customer].[Country], [o2.Customer].[Fax], [o2.Customer].[Phone], [o2.Customer].[PostalCode], [o2.Customer].[Region]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]
CROSS JOIN (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    ORDER BY [o0].[OrderID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [t0]
LEFT JOIN [Customers] AS [o2.Customer] ON [t0].[CustomerID] = [o2.Customer].[CustomerID]
ORDER BY [t].[OrderID]",
                    Sql);
            }
        }

        public override void Include_reference_when_projection(bool useString)
        {
            base.Include_reference_when_projection(useString);

            AssertSql(
                @"SELECT [o].[CustomerID]
FROM [Orders] AS [o]",
                Sql);
        }

        public override void Include_reference_with_filter_reordered(bool useString)
        {
            base.Include_reference_with_filter_reordered(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE [o].[CustomerID] = N'ALFKI'",
                Sql);
        }

        public override void Include_reference_with_filter(bool useString)
        {
            base.Include_reference_with_filter(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE [o].[CustomerID] = N'ALFKI'",
                Sql);
        }

        public override void Include_collection_dependent_already_tracked_as_no_tracking(bool useString)
        {
            base.Include_collection_dependent_already_tracked_as_no_tracking(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'

SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(1) [c0].[CustomerID]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
    ORDER BY [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]",
                Sql);
        }

        public override void Include_collection_dependent_already_tracked(bool useString)
        {
            base.Include_collection_dependent_already_tracked(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'

SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(1) [c0].[CustomerID]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
    ORDER BY [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]",
                Sql);
        }

        public override void Include_reference_dependent_already_tracked(bool useString)
        {
            base.Include_reference_dependent_already_tracked(useString);

            if (!useString)
            {
                AssertSql(
                    @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]",
                    Sql);
            }
        }

        public override void Include_reference_as_no_tracking(bool useString)
        {
            base.Include_reference_as_no_tracking(useString);

            if (!useString)
            {
                AssertSql(
                    @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]",
                    Sql);
            }
        }

        public override void Include_collection_as_no_tracking2(bool useString)
        {
            base.Include_collection_as_no_tracking2(useString);

            AssertSql(
                @"@__p_0: 5

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

@__p_0: 5

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(@__p_0) [c0].[CustomerID]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]",
                Sql);
        }

        public override void Include_with_complex_projection(bool useString)
        {
            base.Include_with_complex_projection(useString);

            AssertSql(
                @"SELECT [o].[CustomerID]
FROM [Orders] AS [o]",
                Sql);
        }

        public override void Include_with_take(bool useString)
        {
            base.Include_with_take(useString);

            AssertSql(
                @"@__p_0: 10

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[City] DESC, [c].[CustomerID]

@__p_0: 10

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(@__p_0) [c0].[CustomerID], [c0].[City]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[City] DESC, [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[City] DESC, [t].[CustomerID]",
                Sql);
        }

        public override void Include_with_skip(bool useString)
        {
            base.Include_with_skip(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0: 80

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactName], [c].[CustomerID]
OFFSET @__p_0 ROWS

@__p_0: 80

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID], [c0].[ContactName]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[ContactName], [c0].[CustomerID]
    OFFSET @__p_0 ROWS
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[ContactName], [t].[CustomerID]",
                    Sql);
            }
        }

        public override void Then_include_collection_order_by_collection_column(bool useString)
        {
            base.Then_include_collection_order_by_collection_column(useString);

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'W' + N'%' AND (CHARINDEX(N'W', [c].[CustomerID]) = 1)
ORDER BY (
    SELECT TOP(1) [oo].[OrderDate]
    FROM [Orders] AS [oo]
    WHERE [c].[CustomerID] = [oo].[CustomerID]
    ORDER BY [oo].[OrderDate] DESC
) DESC, [c].[CustomerID]

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT TOP(1) (
        SELECT TOP(1) [oo].[OrderDate]
        FROM [Orders] AS [oo]
        WHERE [c].[CustomerID] = [oo].[CustomerID]
        ORDER BY [oo].[OrderDate] DESC
    ) AS [c], [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'W' + N'%' AND (CHARINDEX(N'W', [c].[CustomerID]) = 1)
    ORDER BY [c] DESC, [c].[CustomerID]
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
ORDER BY [c0].[c] DESC, [c0].[CustomerID], [o].[OrderID]

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Order Details] AS [o0]
INNER JOIN (
    SELECT DISTINCT [c0].[c], [c0].[CustomerID], [o].[OrderID]
    FROM [Orders] AS [o]
    INNER JOIN (
        SELECT DISTINCT TOP(1) (
            SELECT TOP(1) [oo].[OrderDate]
            FROM [Orders] AS [oo]
            WHERE [c].[CustomerID] = [oo].[CustomerID]
            ORDER BY [oo].[OrderDate] DESC
        ) AS [c], [c].[CustomerID]
        FROM [Customers] AS [c]
        WHERE [c].[CustomerID] LIKE N'W' + N'%' AND (CHARINDEX(N'W', [c].[CustomerID]) = 1)
        ORDER BY [c] DESC, [c].[CustomerID]
    ) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
) AS [o1] ON [o0].[OrderID] = [o1].[OrderID]
ORDER BY [o1].[c] DESC, [o1].[CustomerID], [o1].[OrderID]",
                Sql);
        }

        public override void Include_collection_with_conditional_order_by(bool useString)
        {
            base.Include_collection_with_conditional_order_by(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY CASE
    WHEN [c].[CustomerID] LIKE N'S' + N'%' AND (CHARINDEX(N'S', [c].[CustomerID]) = 1)
    THEN 1 ELSE 2
END, [c].[CustomerID]

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID], CASE
        WHEN [c0].[CustomerID] LIKE N'S' + N'%' AND (CHARINDEX(N'S', [c0].[CustomerID]) = 1)
        THEN 1 ELSE 2
    END AS [c]
    FROM [Customers] AS [c0]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[c], [t].[CustomerID]",
                Sql);
        }

        private const string FileLineEnding = @"
";

        protected override void ClearLog() => TestSqlLoggerFactory.Reset();

        private static string Sql => TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);

        private void AssertSql(string expected, string actual)
        {
            TestHelpers.AssertBaseline(expected, actual, _testOutputHelper);
        }
    }
}
