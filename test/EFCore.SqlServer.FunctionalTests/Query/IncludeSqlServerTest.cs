// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class IncludeSqlServerTest : IncludeTestBase<IncludeSqlServerFixture>
    {
        private bool SupportsOffset => TestEnvironment.GetFlag(nameof(SqlServerCondition.SupportsOffset)) ?? true;

        // ReSharper disable once UnusedParameter.Local
        public IncludeSqlServerTest(IncludeSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void Include_list(bool useString)
        {
            base.Include_list(useString);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
ORDER BY [p].[ProductID]",
                //
                @"SELECT [p.OrderDetails].[OrderID], [p.OrderDetails].[ProductID], [p.OrderDetails].[Discount], [p.OrderDetails].[Quantity], [p.OrderDetails].[UnitPrice], [o.Order].[OrderID], [o.Order].[CustomerID], [o.Order].[EmployeeID], [o.Order].[OrderDate]
FROM [Order Details] AS [p.OrderDetails]
INNER JOIN [Orders] AS [o.Order] ON [p.OrderDetails].[OrderID] = [o.Order].[OrderID]
INNER JOIN (
    SELECT [p0].[ProductID]
    FROM [Products] AS [p0]
) AS [t] ON [p.OrderDetails].[ProductID] = [t].[ProductID]
ORDER BY [t].[ProductID]");
        }

        public override void Include_reference(bool useString)
        {
            base.Include_reference(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]");
        }

        public override void Include_collection(bool useString)
        {
            base.Include_collection(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]");
        }

        public override void Include_collection_with_last(bool useString)
        {
            base.Include_collection_with_last(useString);

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CompanyName] DESC, [c].[CustomerID] DESC",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(1) [c0].[CustomerID], [c0].[CompanyName]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CompanyName] DESC, [c0].[CustomerID] DESC
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CompanyName] DESC, [t].[CustomerID] DESC");
        }

        public override void Include_collection_with_last_no_orderby(bool useString)
        {
            base.Include_collection_with_last_no_orderby(useString);

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID] DESC",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(1) [c0].[CustomerID]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID] DESC
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID] DESC");
        }

        public override void Include_collection_skip_no_order_by(bool useString)
        {
            base.Include_collection_skip_no_order_by(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0='10'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
OFFSET @__p_0 ROWS",
                    //
                    @"@__p_0='10'

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
    OFFSET @__p_0 ROWS
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]");
            }
        }

        public override void Include_collection_take_no_order_by(bool useString)
        {
            base.Include_collection_take_no_order_by(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0='10'

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                    //
                    @"@__p_0='10'

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(@__p_0) [c0].[CustomerID]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]");
            }
        }

        public override void Include_collection_skip_take_no_order_by(bool useString)
        {
            base.Include_collection_skip_take_no_order_by(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0='10'
@__p_1='5'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY",
                    //
                    @"@__p_0='10'
@__p_1='5'

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]");
            }
        }

        public override void Include_reference_and_collection(bool useString)
        {
            base.Include_reference_and_collection(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
ORDER BY [o].[OrderID]",
                //
                @"SELECT [o.OrderDetails].[OrderID], [o.OrderDetails].[ProductID], [o.OrderDetails].[Discount], [o.OrderDetails].[Quantity], [o.OrderDetails].[UnitPrice]
FROM [Order Details] AS [o.OrderDetails]
INNER JOIN (
    SELECT DISTINCT [o0].[OrderID]
    FROM [Orders] AS [o0]
    LEFT JOIN [Customers] AS [o.Customer0] ON [o0].[CustomerID] = [o.Customer0].[CustomerID]
) AS [t] ON [o.OrderDetails].[OrderID] = [t].[OrderID]
ORDER BY [t].[OrderID]");
        }

        public override void Include_references_multi_level(bool useString)
        {
            base.Include_references_multi_level(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o.Order].[OrderID], [o.Order].[CustomerID], [o.Order].[EmployeeID], [o.Order].[OrderDate], [o.Order.Customer].[CustomerID], [o.Order.Customer].[Address], [o.Order.Customer].[City], [o.Order.Customer].[CompanyName], [o.Order.Customer].[ContactName], [o.Order.Customer].[ContactTitle], [o.Order.Customer].[Country], [o.Order.Customer].[Fax], [o.Order.Customer].[Phone], [o.Order.Customer].[PostalCode], [o.Order.Customer].[Region]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o.Order] ON [o].[OrderID] = [o.Order].[OrderID]
LEFT JOIN [Customers] AS [o.Order.Customer] ON [o.Order].[CustomerID] = [o.Order.Customer].[CustomerID]");
        }

        public override void Include_multiple_references_multi_level(bool useString)
        {
            base.Include_multiple_references_multi_level(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o.Product].[ProductID], [o.Product].[Discontinued], [o.Product].[ProductName], [o.Product].[SupplierID], [o.Product].[UnitPrice], [o.Product].[UnitsInStock], [o.Order].[OrderID], [o.Order].[CustomerID], [o.Order].[EmployeeID], [o.Order].[OrderDate], [o.Order.Customer].[CustomerID], [o.Order.Customer].[Address], [o.Order.Customer].[City], [o.Order.Customer].[CompanyName], [o.Order.Customer].[ContactName], [o.Order.Customer].[ContactTitle], [o.Order.Customer].[Country], [o.Order.Customer].[Fax], [o.Order.Customer].[Phone], [o.Order.Customer].[PostalCode], [o.Order.Customer].[Region]
FROM [Order Details] AS [o]
INNER JOIN [Products] AS [o.Product] ON [o].[ProductID] = [o.Product].[ProductID]
INNER JOIN [Orders] AS [o.Order] ON [o].[OrderID] = [o.Order].[OrderID]
LEFT JOIN [Customers] AS [o.Order.Customer] ON [o.Order].[CustomerID] = [o.Order.Customer].[CustomerID]");
        }

        public override void Include_multiple_references_multi_level_reverse(bool useString)
        {
            base.Include_multiple_references_multi_level_reverse(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o.Order].[OrderID], [o.Order].[CustomerID], [o.Order].[EmployeeID], [o.Order].[OrderDate], [o.Order.Customer].[CustomerID], [o.Order.Customer].[Address], [o.Order.Customer].[City], [o.Order.Customer].[CompanyName], [o.Order.Customer].[ContactName], [o.Order.Customer].[ContactTitle], [o.Order.Customer].[Country], [o.Order.Customer].[Fax], [o.Order.Customer].[Phone], [o.Order.Customer].[PostalCode], [o.Order.Customer].[Region], [o.Product].[ProductID], [o.Product].[Discontinued], [o.Product].[ProductName], [o.Product].[SupplierID], [o.Product].[UnitPrice], [o.Product].[UnitsInStock]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o.Order] ON [o].[OrderID] = [o.Order].[OrderID]
LEFT JOIN [Customers] AS [o.Order.Customer] ON [o.Order].[CustomerID] = [o.Order.Customer].[CustomerID]
INNER JOIN [Products] AS [o.Product] ON [o].[ProductID] = [o.Product].[ProductID]");
        }

        public override void Include_references_and_collection_multi_level(bool useString)
        {
            base.Include_references_and_collection_multi_level(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o.Order].[OrderID], [o.Order].[CustomerID], [o.Order].[EmployeeID], [o.Order].[OrderDate], [o.Order.Customer].[CustomerID], [o.Order.Customer].[Address], [o.Order.Customer].[City], [o.Order.Customer].[CompanyName], [o.Order.Customer].[ContactName], [o.Order.Customer].[ContactTitle], [o.Order.Customer].[Country], [o.Order.Customer].[Fax], [o.Order.Customer].[Phone], [o.Order.Customer].[PostalCode], [o.Order.Customer].[Region]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o.Order] ON [o].[OrderID] = [o.Order].[OrderID]
LEFT JOIN [Customers] AS [o.Order.Customer] ON [o.Order].[CustomerID] = [o.Order.Customer].[CustomerID]
ORDER BY [o.Order.Customer].[CustomerID]",
                //
                @"SELECT [o.Order.Customer.Orders].[OrderID], [o.Order.Customer.Orders].[CustomerID], [o.Order.Customer.Orders].[EmployeeID], [o.Order.Customer.Orders].[OrderDate]
FROM [Orders] AS [o.Order.Customer.Orders]
INNER JOIN (
    SELECT DISTINCT [o.Order.Customer0].[CustomerID]
    FROM [Order Details] AS [o0]
    INNER JOIN [Orders] AS [o.Order0] ON [o0].[OrderID] = [o.Order0].[OrderID]
    LEFT JOIN [Customers] AS [o.Order.Customer0] ON [o.Order0].[CustomerID] = [o.Order.Customer0].[CustomerID]
) AS [t] ON [o.Order.Customer.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]");
        }

        public override void Include_multi_level_reference_and_collection_predicate(bool useString)
        {
            base.Include_multi_level_reference_and_collection_predicate(useString);

            AssertSql(
                @"SELECT TOP(2) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE [o].[OrderID] = 10248
ORDER BY [o.Customer].[CustomerID]",
                //
                @"SELECT [o.Customer.Orders].[OrderID], [o.Customer.Orders].[CustomerID], [o.Customer.Orders].[EmployeeID], [o.Customer.Orders].[OrderDate]
FROM [Orders] AS [o.Customer.Orders]
INNER JOIN (
    SELECT DISTINCT [t].*
    FROM (
        SELECT TOP(1) [o.Customer0].[CustomerID]
        FROM [Orders] AS [o0]
        LEFT JOIN [Customers] AS [o.Customer0] ON [o0].[CustomerID] = [o.Customer0].[CustomerID]
        WHERE [o0].[OrderID] = 10248
        ORDER BY [o.Customer0].[CustomerID]
    ) AS [t]
) AS [t0] ON [o.Customer.Orders].[CustomerID] = [t0].[CustomerID]
ORDER BY [t0].[CustomerID]");
        }

        public override void Include_multi_level_collection_and_then_include_reference_predicate(bool useString)
        {
            base.Include_multi_level_collection_and_then_include_reference_predicate(useString);

            AssertSql(
                @"SELECT TOP(2) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] = 10248
ORDER BY [o].[OrderID]",
                //
                @"SELECT [o.OrderDetails].[OrderID], [o.OrderDetails].[ProductID], [o.OrderDetails].[Discount], [o.OrderDetails].[Quantity], [o.OrderDetails].[UnitPrice], [o.Product].[ProductID], [o.Product].[Discontinued], [o.Product].[ProductName], [o.Product].[SupplierID], [o.Product].[UnitPrice], [o.Product].[UnitsInStock]
FROM [Order Details] AS [o.OrderDetails]
INNER JOIN [Products] AS [o.Product] ON [o.OrderDetails].[ProductID] = [o.Product].[ProductID]
INNER JOIN (
    SELECT TOP(1) [o0].[OrderID]
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] = 10248
    ORDER BY [o0].[OrderID]
) AS [t] ON [o.OrderDetails].[OrderID] = [t].[OrderID]
ORDER BY [t].[OrderID]");
        }

        public override void Include_collection_alias_generation(bool useString)
        {
            base.Include_collection_alias_generation(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
ORDER BY [o].[OrderID]",
                //
                @"SELECT [o.OrderDetails].[OrderID], [o.OrderDetails].[ProductID], [o.OrderDetails].[Discount], [o.OrderDetails].[Quantity], [o.OrderDetails].[UnitPrice]
FROM [Order Details] AS [o.OrderDetails]
INNER JOIN (
    SELECT [o0].[OrderID]
    FROM [Orders] AS [o0]
) AS [t] ON [o.OrderDetails].[OrderID] = [t].[OrderID]
ORDER BY [t].[OrderID]");
        }

        public override void Include_collection_order_by_collection_column(bool useString)
        {
            base.Include_collection_order_by_collection_column(useString);

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'W' + N'%' AND (LEFT([c].[CustomerID], LEN(N'W')) = N'W')
ORDER BY (
    SELECT TOP(1) [oo].[OrderDate]
    FROM [Orders] AS [oo]
    WHERE [c].[CustomerID] = [oo].[CustomerID]
    ORDER BY [oo].[OrderDate] DESC
) DESC, [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(1) [c0].[CustomerID], (
        SELECT TOP(1) [oo1].[OrderDate]
        FROM [Orders] AS [oo1]
        WHERE [c0].[CustomerID] = [oo1].[CustomerID]
        ORDER BY [oo1].[OrderDate] DESC
    ) AS [c]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] LIKE N'W' + N'%' AND (LEFT([c0].[CustomerID], LEN(N'W')) = N'W')
    ORDER BY (
        SELECT TOP(1) [oo0].[OrderDate]
        FROM [Orders] AS [oo0]
        WHERE [c0].[CustomerID] = [oo0].[CustomerID]
        ORDER BY [oo0].[OrderDate] DESC
    ) DESC, [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[c] DESC, [t].[CustomerID]");
        }

        public override void Include_collection_order_by_key(bool useString)
        {
            base.Include_collection_order_by_key(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]");
        }

        public override void Include_collection_order_by_non_key(bool useString)
        {
            base.Include_collection_order_by_non_key(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[City], [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID], [c0].[City]
    FROM [Customers] AS [c0]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[City], [t].[CustomerID]");
        }

        public override void Include_collection_order_by_non_key_with_take(bool useString)
        {
            base.Include_collection_order_by_non_key_with_take(useString);

            AssertSql(
                @"@__p_0='10'

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactTitle], [c].[CustomerID]",
                //
                @"@__p_0='10'

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(@__p_0) [c0].[CustomerID], [c0].[ContactTitle]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[ContactTitle], [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[ContactTitle], [t].[CustomerID]");
        }

        public override void Include_collection_order_by_non_key_with_skip(bool useString)
        {
            base.Include_collection_order_by_non_key_with_skip(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0='10'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactTitle], [c].[CustomerID]
OFFSET @__p_0 ROWS",
                    //
                    @"@__p_0='10'

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID], [c0].[ContactTitle]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[ContactTitle], [c0].[CustomerID]
    OFFSET @__p_0 ROWS
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[ContactTitle], [t].[CustomerID]");
            }
        }

        public override void Include_collection_order_by_non_key_with_first_or_default(bool useString)
        {
            base.Include_collection_order_by_non_key_with_first_or_default(useString);

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CompanyName] DESC, [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(1) [c0].[CustomerID], [c0].[CompanyName]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CompanyName] DESC, [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CompanyName] DESC, [t].[CustomerID]");
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
), [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(1) [c0].[CustomerID], (
        SELECT TOP(1) [o1].[OrderDate]
        FROM [Orders] AS [o1]
        WHERE [c0].[CustomerID] = [o1].[CustomerID]
        ORDER BY [o1].[EmployeeID]
    ) AS [c]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
    ORDER BY (
        SELECT TOP(1) [o0].[OrderDate]
        FROM [Orders] AS [o0]
        WHERE [c0].[CustomerID] = [o0].[CustomerID]
        ORDER BY [o0].[EmployeeID]
    ), [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[c], [t].[CustomerID]");
        }

        public override void Include_collection_as_no_tracking(bool useString)
        {
            base.Include_collection_as_no_tracking(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]");
        }

        public override void Include_collection_principal_already_tracked(bool useString)
        {
            base.Include_collection_principal_already_tracked(useString);

            AssertSql(
                @"SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'",
                //
                @"SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(1) [c0].[CustomerID]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
    ORDER BY [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]");
        }

        public override void Include_collection_principal_already_tracked_as_no_tracking(bool useString)
        {
            base.Include_collection_principal_already_tracked_as_no_tracking(useString);

            AssertSql(
                @"SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'",
                //
                @"SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(1) [c0].[CustomerID]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
    ORDER BY [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]");
        }

        public override void Include_collection_with_filter(bool useString)
        {
            base.Include_collection_with_filter(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]");
        }

        public override void Include_collection_with_filter_reordered(bool useString)
        {
            base.Include_collection_with_filter_reordered(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]");
        }

        public override void Include_collection_then_include_collection(bool useString)
        {
            base.Include_collection_then_include_collection(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID], [c.Orders].[OrderID]",
                //
                @"SELECT [c.Orders.OrderDetails].[OrderID], [c.Orders.OrderDetails].[ProductID], [c.Orders.OrderDetails].[Discount], [c.Orders.OrderDetails].[Quantity], [c.Orders.OrderDetails].[UnitPrice]
FROM [Order Details] AS [c.Orders.OrderDetails]
INNER JOIN (
    SELECT DISTINCT [c.Orders0].[OrderID], [t0].[CustomerID]
    FROM [Orders] AS [c.Orders0]
    INNER JOIN (
        SELECT [c1].[CustomerID]
        FROM [Customers] AS [c1]
    ) AS [t0] ON [c.Orders0].[CustomerID] = [t0].[CustomerID]
) AS [t1] ON [c.Orders.OrderDetails].[OrderID] = [t1].[OrderID]
ORDER BY [t1].[CustomerID], [t1].[OrderID]");
        }

        public override void Include_collection_when_projection(bool useString)
        {
            base.Include_collection_when_projection(useString);

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]");
        }

        public override void Include_collection_on_join_clause_with_filter(bool useString)
        {
            base.Include_collection_on_join_clause_with_filter(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT DISTINCT [c0].[CustomerID]
    FROM [Customers] AS [c0]
    INNER JOIN [Orders] AS [o0] ON [c0].[CustomerID] = [o0].[CustomerID]
    WHERE [c0].[CustomerID] = N'ALFKI'
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]");
        }

        public override void Include_collection_on_additional_from_clause_with_filter(bool useString)
        {
            base.Include_collection_on_additional_from_clause_with_filter(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c1]
CROSS JOIN [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT DISTINCT [c0].[CustomerID]
    FROM [Customers] AS [c10]
    CROSS JOIN [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]");
        }

        public override void Include_collection_on_additional_from_clause(bool useString)
        {
            base.Include_collection_on_additional_from_clause(useString);

            AssertSql(
                @"@__p_0='5'

SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
FROM (
    SELECT TOP(@__p_0) [c].*
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [t]
CROSS JOIN [Customers] AS [c2]
ORDER BY [c2].[CustomerID]",
                //
                @"@__p_0='5'

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
ORDER BY [t1].[CustomerID]");
        }

        public override void Include_duplicate_collection(bool useString)
        {
            base.Include_duplicate_collection(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0='2'

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
                    //
                    @"@__p_0='2'

SELECT [c1.Orders].[OrderID], [c1.Orders].[CustomerID], [c1.Orders].[EmployeeID], [c1.Orders].[OrderDate]
FROM [Orders] AS [c1.Orders]
INNER JOIN (
    SELECT DISTINCT [t1].[CustomerID]
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
) AS [t3] ON [c1.Orders].[CustomerID] = [t3].[CustomerID]
ORDER BY [t3].[CustomerID]",
                    //
                    @"@__p_0='2'

SELECT [c2.Orders].[OrderID], [c2.Orders].[CustomerID], [c2.Orders].[EmployeeID], [c2.Orders].[OrderDate]
FROM [Orders] AS [c2.Orders]
INNER JOIN (
    SELECT DISTINCT [t5].[CustomerID], [t4].[CustomerID] AS [CustomerID0]
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
) AS [t6] ON [c2.Orders].[CustomerID] = [t6].[CustomerID]
ORDER BY [t6].[CustomerID0], [t6].[CustomerID]");
            }
        }

        public override void Include_duplicate_collection_result_operator(bool useString)
        {
            base.Include_duplicate_collection_result_operator(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_1='1'
@__p_0='2'

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
ORDER BY [t].[CustomerID], [t0].[CustomerID]",
                    //
                    @"@__p_1='1'
@__p_0='2'

SELECT [c1.Orders].[OrderID], [c1.Orders].[CustomerID], [c1.Orders].[EmployeeID], [c1.Orders].[OrderDate]
FROM [Orders] AS [c1.Orders]
INNER JOIN (
    SELECT DISTINCT [t3].*
    FROM (
        SELECT TOP(@__p_1) [t1].[CustomerID]
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
    ) AS [t3]
) AS [t4] ON [c1.Orders].[CustomerID] = [t4].[CustomerID]
ORDER BY [t4].[CustomerID]",
                    //
                    @"@__p_1='1'
@__p_0='2'

SELECT [c2.Orders].[OrderID], [c2.Orders].[CustomerID], [c2.Orders].[EmployeeID], [c2.Orders].[OrderDate]
FROM [Orders] AS [c2.Orders]
INNER JOIN (
    SELECT DISTINCT [t7].*
    FROM (
        SELECT TOP(@__p_1) [t6].[CustomerID], [t5].[CustomerID] AS [CustomerID0]
        FROM (
            SELECT TOP(@__p_0) [c3].*
            FROM [Customers] AS [c3]
            ORDER BY [c3].[CustomerID]
        ) AS [t5]
        CROSS JOIN (
            SELECT [c4].*
            FROM [Customers] AS [c4]
            ORDER BY [c4].[CustomerID]
            OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
        ) AS [t6]
        ORDER BY [t5].[CustomerID], [t6].[CustomerID]
    ) AS [t7]
) AS [t8] ON [c2.Orders].[CustomerID] = [t8].[CustomerID]
ORDER BY [t8].[CustomerID0], [t8].[CustomerID]");
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
ORDER BY [c].[City], [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT DISTINCT [c0].[CustomerID], [c0].[City]
    FROM [Customers] AS [c0]
    INNER JOIN [Orders] AS [o0] ON [c0].[CustomerID] = [o0].[CustomerID]
    WHERE [c0].[CustomerID] = N'ALFKI'
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[City], [t].[CustomerID]");
        }

        public override void Include_collection_when_groupby(bool useString)
        {
            base.Include_collection_when_groupby(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[City], [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID], [c0].[City]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[City], [t].[CustomerID]");
        }

        public override void Include_collection_when_groupby_subquery(bool useString)
        {
            base.Include_collection_when_groupby_subquery(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(1) [c0].[CustomerID]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
    ORDER BY [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID], [c.Orders].[OrderID]",
                //
                @"SELECT [c.Orders.OrderDetails].[OrderID], [c.Orders.OrderDetails].[ProductID], [c.Orders.OrderDetails].[Discount], [c.Orders.OrderDetails].[Quantity], [c.Orders.OrderDetails].[UnitPrice], [o.Product].[ProductID], [o.Product].[Discontinued], [o.Product].[ProductName], [o.Product].[SupplierID], [o.Product].[UnitPrice], [o.Product].[UnitsInStock]
FROM [Order Details] AS [c.Orders.OrderDetails]
INNER JOIN [Products] AS [o.Product] ON [c.Orders.OrderDetails].[ProductID] = [o.Product].[ProductID]
INNER JOIN (
    SELECT DISTINCT [c.Orders0].[OrderID], [t0].[CustomerID]
    FROM [Orders] AS [c.Orders0]
    INNER JOIN (
        SELECT TOP(1) [c1].[CustomerID]
        FROM [Customers] AS [c1]
        WHERE [c1].[CustomerID] = N'ALFKI'
        ORDER BY [c1].[CustomerID]
    ) AS [t0] ON [c.Orders0].[CustomerID] = [t0].[CustomerID]
) AS [t1] ON [c.Orders.OrderDetails].[OrderID] = [t1].[OrderID]
ORDER BY [t1].[CustomerID], [t1].[OrderID]");
        }

        public override void Include_collection_on_additional_from_clause2(bool useString)
        {
            base.Include_collection_on_additional_from_clause2(useString);

            AssertSql(
                @"@__p_0='5'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [t]
CROSS JOIN [Customers] AS [c2]");
        }

        public override void Include_where_skip_take_projection(bool useString)
        {
            base.Include_where_skip_take_projection(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0='1'
@__p_1='2'

SELECT [od.Order].[CustomerID]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [od.Order] ON [od].[OrderID] = [od.Order].[OrderID]
WHERE [od].[Quantity] = CAST(10 AS smallint)
ORDER BY [od].[OrderID], [od].[ProductID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY");
            }
        }

        public override void Include_duplicate_collection_result_operator2(bool useString)
        {
            base.Include_duplicate_collection_result_operator2(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_1='1'
@__p_0='2'

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
ORDER BY [t].[CustomerID]",
                    //
                    @"@__p_1='1'
@__p_0='2'

SELECT [c1.Orders].[OrderID], [c1.Orders].[CustomerID], [c1.Orders].[EmployeeID], [c1.Orders].[OrderDate]
FROM [Orders] AS [c1.Orders]
INNER JOIN (
    SELECT DISTINCT [t3].*
    FROM (
        SELECT TOP(@__p_1) [t1].[CustomerID]
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
    ) AS [t3]
) AS [t4] ON [c1.Orders].[CustomerID] = [t4].[CustomerID]
ORDER BY [t4].[CustomerID]");
            }
        }

        public override void Include_multiple_references(bool useString)
        {
            base.Include_multiple_references(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o.Product].[ProductID], [o.Product].[Discontinued], [o.Product].[ProductName], [o.Product].[SupplierID], [o.Product].[UnitPrice], [o.Product].[UnitsInStock], [o.Order].[OrderID], [o.Order].[CustomerID], [o.Order].[EmployeeID], [o.Order].[OrderDate]
FROM [Order Details] AS [o]
INNER JOIN [Products] AS [o.Product] ON [o].[ProductID] = [o.Product].[ProductID]
INNER JOIN [Orders] AS [o.Order] ON [o].[OrderID] = [o.Order].[OrderID]");
        }

        public override void Include_reference_alias_generation(bool useString)
        {
            base.Include_reference_alias_generation(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o.Order].[OrderID], [o.Order].[CustomerID], [o.Order].[EmployeeID], [o.Order].[OrderDate]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o.Order] ON [o].[OrderID] = [o.Order].[OrderID]");
        }

        public override void Include_duplicate_reference(bool useString)
        {
            base.Include_duplicate_reference(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0='2'

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
LEFT JOIN [Customers] AS [o2.Customer] ON [t0].[CustomerID] = [o2.Customer].[CustomerID]");
            }
        }

        public override void Include_duplicate_reference2(bool useString)
        {
            base.Include_duplicate_reference2(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0='2'

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
) AS [t0]");
            }
        }

        public override void Include_duplicate_reference3(bool useString)
        {
            base.Include_duplicate_reference3(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0='2'

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
LEFT JOIN [Customers] AS [o2.Customer] ON [t0].[CustomerID] = [o2.Customer].[CustomerID]");
            }
        }

        public override void Include_reference_when_projection(bool useString)
        {
            base.Include_reference_when_projection(useString);

            AssertSql(
                @"SELECT [o].[CustomerID]
FROM [Orders] AS [o]");
        }

        public override void Include_reference_with_filter_reordered(bool useString)
        {
            base.Include_reference_with_filter_reordered(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE [o].[CustomerID] = N'ALFKI'");
        }

        public override void Include_reference_with_filter(bool useString)
        {
            base.Include_reference_with_filter(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE [o].[CustomerID] = N'ALFKI'");
        }

        public override void Include_collection_dependent_already_tracked_as_no_tracking(bool useString)
        {
            base.Include_collection_dependent_already_tracked_as_no_tracking(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'",
                //
                @"SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(1) [c0].[CustomerID]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
    ORDER BY [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]");
        }

        public override void Include_collection_dependent_already_tracked(bool useString)
        {
            base.Include_collection_dependent_already_tracked(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'",
                //
                @"SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(1) [c0].[CustomerID]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
    ORDER BY [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]");
        }

        public override void Include_reference_dependent_already_tracked(bool useString)
        {
            base.Include_reference_dependent_already_tracked(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]");
        }

        public override void Include_reference_as_no_tracking(bool useString)
        {
            base.Include_reference_as_no_tracking(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]");
        }

        public override void Include_collection_as_no_tracking2(bool useString)
        {
            base.Include_collection_as_no_tracking2(useString);

            AssertSql(
                @"@__p_0='5'

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                //
                @"@__p_0='5'

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(@__p_0) [c0].[CustomerID]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]");
        }

        public override void Include_with_complex_projection(bool useString)
        {
            base.Include_with_complex_projection(useString);

            AssertSql(
                @"SELECT [o].[CustomerID] AS [Id]
FROM [Orders] AS [o]");
        }

        public override void Include_with_complex_projection_does_not_change_ordering_of_projection(bool useString)
        {
            base.Include_with_complex_projection_does_not_change_ordering_of_projection(useString);

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Id], (
    SELECT COUNT(*)
    FROM [Orders] AS [o0]
    WHERE [c].[CustomerID] = [o0].[CustomerID]
) AS [TotalOrders]
FROM [Customers] AS [c]
WHERE ([c].[ContactTitle] = N'Owner') AND ((
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) > 2)
ORDER BY [Id]");
        }

        public override void Include_with_take(bool useString)
        {
            base.Include_with_take(useString);

            AssertSql(
                @"@__p_0='10'

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[City] DESC, [c].[CustomerID]",
                //
                @"@__p_0='10'

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(@__p_0) [c0].[CustomerID], [c0].[City]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[City] DESC, [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[City] DESC, [t].[CustomerID]");
        }

        public override void Include_with_skip(bool useString)
        {
            base.Include_with_skip(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0='80'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactName], [c].[CustomerID]
OFFSET @__p_0 ROWS",
                    //
                    @"@__p_0='80'

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID], [c0].[ContactName]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[ContactName], [c0].[CustomerID]
    OFFSET @__p_0 ROWS
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[ContactName], [t].[CustomerID]");
            }
        }

        public override void Then_include_collection_order_by_collection_column(bool useString)
        {
            base.Then_include_collection_order_by_collection_column(useString);

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'W' + N'%' AND (LEFT([c].[CustomerID], LEN(N'W')) = N'W')
ORDER BY (
    SELECT TOP(1) [oo].[OrderDate]
    FROM [Orders] AS [oo]
    WHERE [c].[CustomerID] = [oo].[CustomerID]
    ORDER BY [oo].[OrderDate] DESC
) DESC, [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(1) [c0].[CustomerID], (
        SELECT TOP(1) [oo1].[OrderDate]
        FROM [Orders] AS [oo1]
        WHERE [c0].[CustomerID] = [oo1].[CustomerID]
        ORDER BY [oo1].[OrderDate] DESC
    ) AS [c]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] LIKE N'W' + N'%' AND (LEFT([c0].[CustomerID], LEN(N'W')) = N'W')
    ORDER BY (
        SELECT TOP(1) [oo0].[OrderDate]
        FROM [Orders] AS [oo0]
        WHERE [c0].[CustomerID] = [oo0].[CustomerID]
        ORDER BY [oo0].[OrderDate] DESC
    ) DESC, [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[c] DESC, [t].[CustomerID], [c.Orders].[OrderID]",
                //
                @"SELECT [c.Orders.OrderDetails].[OrderID], [c.Orders.OrderDetails].[ProductID], [c.Orders.OrderDetails].[Discount], [c.Orders.OrderDetails].[Quantity], [c.Orders.OrderDetails].[UnitPrice]
FROM [Order Details] AS [c.Orders.OrderDetails]
INNER JOIN (
    SELECT DISTINCT [c.Orders0].[OrderID], [t0].[c], [t0].[CustomerID]
    FROM [Orders] AS [c.Orders0]
    INNER JOIN (
        SELECT TOP(1) [c1].[CustomerID], (
            SELECT TOP(1) [oo3].[OrderDate]
            FROM [Orders] AS [oo3]
            WHERE [c1].[CustomerID] = [oo3].[CustomerID]
            ORDER BY [oo3].[OrderDate] DESC
        ) AS [c]
        FROM [Customers] AS [c1]
        WHERE [c1].[CustomerID] LIKE N'W' + N'%' AND (LEFT([c1].[CustomerID], LEN(N'W')) = N'W')
        ORDER BY (
            SELECT TOP(1) [oo2].[OrderDate]
            FROM [Orders] AS [oo2]
            WHERE [c1].[CustomerID] = [oo2].[CustomerID]
            ORDER BY [oo2].[OrderDate] DESC
        ) DESC, [c1].[CustomerID]
    ) AS [t0] ON [c.Orders0].[CustomerID] = [t0].[CustomerID]
) AS [t1] ON [c.Orders.OrderDetails].[OrderID] = [t1].[OrderID]
ORDER BY [t1].[c] DESC, [t1].[CustomerID], [t1].[OrderID]");
        }

        public override void Include_collection_with_conditional_order_by(bool useString)
        {
            base.Include_collection_with_conditional_order_by(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY CASE
    WHEN [c].[CustomerID] LIKE N'S' + N'%' AND (LEFT([c].[CustomerID], LEN(N'S')) = N'S')
    THEN 1 ELSE 2
END, [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID], CASE
        WHEN [c0].[CustomerID] LIKE N'S' + N'%' AND (LEFT([c0].[CustomerID], LEN(N'S')) = N'S')
        THEN 1 ELSE 2
    END AS [c]
    FROM [Customers] AS [c0]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[c], [t].[CustomerID]");
        }

        public override void Include_reference_distinct_is_server_evaluated(bool useString)
        {
            base.Include_reference_distinct_is_server_evaluated(useString);

            AssertSql(
                @"SELECT DISTINCT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE [o].[OrderID] < 10250");
        }

        public override void Include_collection_distinct_is_server_evaluated(bool useString)
        {
            base.Include_collection_distinct_is_server_evaluated(useString);

            AssertSql(
                @"SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')
ORDER BY [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT DISTINCT [c0].[CustomerID]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c0].[CustomerID], LEN(N'A')) = N'A')
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]");
        }

        public override void Include_collection_OrderBy_object(bool useString)
        {
            base.Include_collection_OrderBy_object(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10250
ORDER BY [o].[OrderID]",
                //
                @"SELECT [o.OrderDetails].[OrderID], [o.OrderDetails].[ProductID], [o.OrderDetails].[Discount], [o.OrderDetails].[Quantity], [o.OrderDetails].[UnitPrice]
FROM [Order Details] AS [o.OrderDetails]
INNER JOIN (
    SELECT [o0].[OrderID]
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] < 10250
) AS [t] ON [o.OrderDetails].[OrderID] = [t].[OrderID]
ORDER BY [t].[OrderID]");
        }

        public override void Include_collection_OrderBy_empty_list_contains(bool useString)
        {
            base.Include_collection_OrderBy_empty_list_contains(useString);

            AssertSql(
                @"@__p_1='1'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')
ORDER BY (SELECT 1), [c].[CustomerID]
OFFSET @__p_1 ROWS",
                //
                @"@__p_1='1'

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID], CAST(0 AS bit) AS [c]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c0].[CustomerID], LEN(N'A')) = N'A')
    ORDER BY [c], [c0].[CustomerID]
    OFFSET @__p_1 ROWS
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[c], [t].[CustomerID]");
        }

        public override void Include_collection_OrderBy_empty_list_does_not_contains(bool useString)
        {
            base.Include_collection_OrderBy_empty_list_does_not_contains(useString);

            AssertSql(
                @"@__p_1='1'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')
ORDER BY (SELECT 1), [c].[CustomerID]
OFFSET @__p_1 ROWS",
                //
                @"@__p_1='1'

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID], CAST(1 AS bit) AS [c]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c0].[CustomerID], LEN(N'A')) = N'A')
    ORDER BY [c], [c0].[CustomerID]
    OFFSET @__p_1 ROWS
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[c], [t].[CustomerID]");
        }

        public override void Include_collection_OrderBy_list_contains(bool useString)
        {
            base.Include_collection_OrderBy_list_contains(useString);

            AssertSql(
                @"@__p_1='1'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')
ORDER BY CASE
    WHEN [c].[CustomerID] IN (N'ALFKI')
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END, [c].[CustomerID]
OFFSET @__p_1 ROWS",
                //
                @"@__p_1='1'

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID], CASE
        WHEN [c0].[CustomerID] IN (N'ALFKI')
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END AS [c]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c0].[CustomerID], LEN(N'A')) = N'A')
    ORDER BY [c], [c0].[CustomerID]
    OFFSET @__p_1 ROWS
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[c], [t].[CustomerID]");
        }

        public override void Include_collection_OrderBy_list_does_not_contains(bool useString)
        {
            base.Include_collection_OrderBy_list_does_not_contains(useString);

            AssertSql(
                @"@__p_1='1'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')
ORDER BY CASE
    WHEN [c].[CustomerID] NOT IN (N'ALFKI')
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END, [c].[CustomerID]
OFFSET @__p_1 ROWS",
                //
                @"@__p_1='1'

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID], CASE
        WHEN [c0].[CustomerID] NOT IN (N'ALFKI')
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END AS [c]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c0].[CustomerID], LEN(N'A')) = N'A')
    ORDER BY [c], [c0].[CustomerID]
    OFFSET @__p_1 ROWS
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[c], [t].[CustomerID]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
