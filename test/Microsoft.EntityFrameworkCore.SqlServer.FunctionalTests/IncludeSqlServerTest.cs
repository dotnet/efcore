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
        private bool SupportsOffset => TestEnvironment.GetFlag(nameof(SqlServerCondition.SupportsOffset)) ?? true;

        public IncludeSqlServerTest(NorthwindQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        public override void Include_list(bool useString)
        {
            base.Include_list(useString);

            Assert.Equal(
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

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c]
    WHERE [o].[CustomerID] = [c].[CustomerID])
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void Include_collection_skip_no_order_by(bool useString)
        {
            base.Include_collection_skip_no_order_by(useString);

            if (SupportsOffset)
            {
                Assert.Equal(
                    @"@__p_0: 10

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
OFFSET @__p_0 ROWS

@__p_0: 10

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT [t].*
    FROM (
        SELECT [c].[CustomerID]
        FROM [Customers] AS [c]
        ORDER BY [c].[CustomerID]
        OFFSET @__p_0 ROWS
    ) AS [t]
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
ORDER BY [c0].[CustomerID]",
                    Sql);
            }
        }

        public override void Include_collection_take_no_order_by(bool useString)
        {
            base.Include_collection_take_no_order_by(useString);

            if (SupportsOffset)
            {
                Assert.Equal(
                    @"@__p_0: 10

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

@__p_0: 10

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT TOP(@__p_0) [c].[CustomerID]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
ORDER BY [c0].[CustomerID]",
                    Sql);
            }
        }

        public override void Include_collection_skip_take_no_order_by(bool useString)
        {
            base.Include_collection_skip_take_no_order_by(useString);

            if (SupportsOffset)
            {
                Assert.Equal(
                    @"@__p_0: 10
@__p_1: 5

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY

@__p_0: 10
@__p_1: 5

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT [t].*
    FROM (
        SELECT [c].[CustomerID]
        FROM [Customers] AS [c]
        ORDER BY [c].[CustomerID]
        OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
    ) AS [t]
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
ORDER BY [c0].[CustomerID]",
                    Sql);
            }
        }

        public override void Include_reference_and_collection(bool useString)
        {
            base.Include_reference_and_collection(useString);

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [o].[OrderID]

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Order Details] AS [o0]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o0].[OrderID] = [o].[OrderID])
ORDER BY [o0].[OrderID]",
                Sql);
        }

        public override void Include_references_multi_level(bool useString)
        {
            base.Include_references_multi_level(useString);

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]",
                Sql);
        }

        public override void Include_multiple_references_multi_level(bool useString)
        {
            base.Include_multiple_references_multi_level(useString);

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
INNER JOIN [Products] AS [p] ON [o].[ProductID] = [p].[ProductID]",
                Sql);
        }

        public override void Include_multiple_references_multi_level_reverse(bool useString)
        {
            base.Include_multiple_references_multi_level_reverse(useString);

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
INNER JOIN [Products] AS [p] ON [o].[ProductID] = [p].[ProductID]",
                Sql);
        }

        public override void Include_references_and_collection_multi_level(bool useString)
        {
            base.Include_references_and_collection_multi_level(useString);

            Assert.Equal(
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

            Assert.Equal(
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

            Assert.Equal(
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

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
ORDER BY [o].[OrderID]

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Order Details] AS [o0]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o0].[OrderID] = [o].[OrderID])
ORDER BY [o0].[OrderID]",
                Sql);
        }

        public override void Include_collection_order_by_collection_column(bool useString)
        {
            base.Include_collection_order_by_collection_column(useString);

            Assert.Equal(
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
    ) AS [c0_0], [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'W' + N'%' AND (CHARINDEX(N'W', [c].[CustomerID]) = 1)
    ORDER BY [c0_0] DESC, [c].[CustomerID]
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
ORDER BY [c0].[c0_0] DESC, [c0].[CustomerID]",
                Sql);
        }

        public override void Include_collection_order_by_key(bool useString)
        {
            base.Include_collection_order_by_key(useString);

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c]
    WHERE [o].[CustomerID] = [c].[CustomerID])
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void Include_collection_order_by_non_key(bool useString)
        {
            base.Include_collection_order_by_non_key(useString);

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[City], [c].[CustomerID]

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT [c].[City], [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
ORDER BY [c0].[City], [c0].[CustomerID]",
                Sql);
        }

        public override void Include_collection_order_by_non_key_with_take(bool useString)
        {
            base.Include_collection_order_by_non_key_with_take(useString);

            Assert.Equal(
                @"@__p_0: 10

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactTitle], [c].[CustomerID]

@__p_0: 10

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT TOP(@__p_0) [c].[ContactTitle], [c].[CustomerID]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactTitle], [c].[CustomerID]
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
ORDER BY [c0].[ContactTitle], [c0].[CustomerID]",
                Sql);
        }

        public override void Include_collection_order_by_non_key_with_skip(bool useString)
        {
            base.Include_collection_order_by_non_key_with_skip(useString);

            if (SupportsOffset)
            {
                Assert.Equal(
                    @"@__p_0: 10

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactTitle], [c].[CustomerID]
OFFSET @__p_0 ROWS

@__p_0: 10

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT [t].*
    FROM (
        SELECT [c].[ContactTitle], [c].[CustomerID]
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactTitle], [c].[CustomerID]
        OFFSET @__p_0 ROWS
    ) AS [t]
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
ORDER BY [c0].[ContactTitle], [c0].[CustomerID]",
                    Sql);
            }
        }

        public override void Include_collection_order_by_non_key_with_first_or_default(bool useString)
        {
            base.Include_collection_order_by_non_key_with_first_or_default(useString);

            Assert.Equal(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CompanyName] DESC, [c].[CustomerID]

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT TOP(1) [c].[CompanyName], [c].[CustomerID]
    FROM [Customers] AS [c]
    ORDER BY [c].[CompanyName] DESC, [c].[CustomerID]
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
ORDER BY [c0].[CompanyName] DESC, [c0].[CustomerID]",
                Sql);
        }

        public override void Include_collection_order_by_subquery(bool useString)
        {
            base.Include_collection_order_by_subquery(useString);

            Assert.Equal(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY (
    SELECT TOP(1) [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[EmployeeID]
), [c].[CustomerID]

SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o0]
INNER JOIN (
    SELECT DISTINCT TOP(1) (
        SELECT TOP(1) [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[EmployeeID]
    ) AS [c0_0], [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'ALFKI'
    ORDER BY [c0_0], [c].[CustomerID]
) AS [c0] ON [o0].[CustomerID] = [c0].[CustomerID]
ORDER BY [c0].[c0_0], [c0].[CustomerID]",
                Sql);
        }

        public override void Include_collection_as_no_tracking(bool useString)
        {
            base.Include_collection_as_no_tracking(useString);

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c]
    WHERE [o].[CustomerID] = [c].[CustomerID])
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void Include_collection_principal_already_tracked(bool useString)
        {
            base.Include_collection_principal_already_tracked(useString);

            Assert.Equal(
                @"SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'

SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT TOP(2) [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'ALFKI'
    ORDER BY [c].[CustomerID]
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
ORDER BY [c0].[CustomerID]",
                Sql);
        }

        public override void Include_collection_principal_already_tracked_as_no_tracking(bool useString)
        {
            base.Include_collection_principal_already_tracked_as_no_tracking(useString);

            Assert.Equal(
                @"SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'

SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT TOP(2) [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'ALFKI'
    ORDER BY [c].[CustomerID]
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
ORDER BY [c0].[CustomerID]",
                Sql);
        }

        public override void Include_collection_with_filter(bool useString)
        {
            base.Include_collection_with_filter(useString);

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c]
    WHERE ([c].[CustomerID] = N'ALFKI') AND ([o].[CustomerID] = [c].[CustomerID]))
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void Include_collection_with_filter_reordered(bool useString)
        {
            base.Include_collection_with_filter_reordered(useString);

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c]
    WHERE ([c].[CustomerID] = N'ALFKI') AND ([o].[CustomerID] = [c].[CustomerID]))
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void Include_collection_then_include_collection(bool useString)
        {
            base.Include_collection_then_include_collection(useString);

            Assert.Equal(
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

            Assert.Equal(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Include_collection_on_join_clause_with_filter(bool useString)
        {
            base.Include_collection_on_join_clause_with_filter(useString);

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]

SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o0]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c]
    INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
    WHERE ([c].[CustomerID] = N'ALFKI') AND ([o0].[CustomerID] = [c].[CustomerID]))
ORDER BY [o0].[CustomerID]",
                Sql);
        }

        public override void Include_collection_on_additional_from_clause_with_filter(bool useString)
        {
            base.Include_collection_on_additional_from_clause_with_filter(useString);

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c1]
CROSS JOIN [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c1]
    CROSS JOIN [Customers] AS [c]
    WHERE ([c].[CustomerID] = N'ALFKI') AND ([o].[CustomerID] = [c].[CustomerID]))
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void Include_collection_on_additional_from_clause(bool useString)
        {
            base.Include_collection_on_additional_from_clause(useString);

            Assert.Equal(
                @"@__p_0: 5

SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
FROM (
    SELECT TOP(@__p_0) [c0].*
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
) AS [t]
CROSS JOIN [Customers] AS [c2]
ORDER BY [t].[CustomerID], [c2].[CustomerID]

@__p_0: 5

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT TOP(@__p_0) [c0].*
        FROM [Customers] AS [c0]
        ORDER BY [c0].[CustomerID]
    ) AS [t]
    CROSS JOIN [Customers] AS [c2]
    WHERE [o].[CustomerID] = [c2].[CustomerID])
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void Include_duplicate_collection(bool useString)
        {
            base.Include_duplicate_collection(useString);

            if (SupportsOffset)
            {
                Assert.Equal(
                    @"@__p_0: 2

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT TOP(@__p_0) [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
) AS [t]
CROSS JOIN (
    SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
    FROM [Customers] AS [c2]
    ORDER BY [c2].[CustomerID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [t0]
ORDER BY [t].[CustomerID], [t0].[CustomerID]

@__p_0: 2

SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o0]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT TOP(@__p_0) [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
        FROM [Customers] AS [c0]
        ORDER BY [c0].[CustomerID]
    ) AS [t]
    CROSS JOIN (
        SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
        FROM [Customers] AS [c2]
        ORDER BY [c2].[CustomerID]
        OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
    ) AS [t0]
    WHERE [o0].[CustomerID] = [t0].[CustomerID])
ORDER BY [o0].[CustomerID]

@__p_0: 2

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT TOP(@__p_0) [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
        FROM [Customers] AS [c0]
        ORDER BY [c0].[CustomerID]
    ) AS [t]
    CROSS JOIN (
        SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
        FROM [Customers] AS [c2]
        ORDER BY [c2].[CustomerID]
        OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
    ) AS [t0]
    WHERE [o].[CustomerID] = [t].[CustomerID])
ORDER BY [o].[CustomerID]",
                    Sql);
            }
        }

        public override void Include_duplicate_collection_result_operator(bool useString)
        {
            base.Include_duplicate_collection_result_operator(useString);

            if (SupportsOffset)
            {
                Assert.Equal(
                    @"@__p_1: 1
@__p_0: 2

SELECT TOP(@__p_1) [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT TOP(@__p_0) [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
) AS [t]
CROSS JOIN (
    SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
    FROM [Customers] AS [c2]
    ORDER BY [c2].[CustomerID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [t0]
ORDER BY [t].[CustomerID], [t0].[CustomerID]

@__p_1: 1
@__p_0: 2

SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o0]
INNER JOIN (
    SELECT DISTINCT TOP(@__p_1) [t].[CustomerID], [t0].[CustomerID] AS [CustomerID0]
    FROM (
        SELECT TOP(@__p_0) [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
        FROM [Customers] AS [c0]
        ORDER BY [c0].[CustomerID]
    ) AS [t]
    CROSS JOIN (
        SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
        FROM [Customers] AS [c2]
        ORDER BY [c2].[CustomerID]
        OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
    ) AS [t0]
    ORDER BY [t].[CustomerID], [t0].[CustomerID]
) AS [t00] ON [o0].[CustomerID] = [t00].[CustomerID0]
ORDER BY [t00].[CustomerID], [t00].[CustomerID0]

@__p_1: 1
@__p_0: 2

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT TOP(@__p_1) [t].[CustomerID]
    FROM (
        SELECT TOP(@__p_0) [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
        FROM [Customers] AS [c0]
        ORDER BY [c0].[CustomerID]
    ) AS [t]
    CROSS JOIN (
        SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
        FROM [Customers] AS [c2]
        ORDER BY [c2].[CustomerID]
        OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
    ) AS [t0]
    ORDER BY [t].[CustomerID]
) AS [t1] ON [o].[CustomerID] = [t1].[CustomerID]
ORDER BY [t1].[CustomerID]",
                    Sql);
            }
        }

        public override void Include_collection_on_join_clause_with_order_by_and_filter(bool useString)
        {
            base.Include_collection_on_join_clause_with_order_by_and_filter(useString);

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[City], [c].[CustomerID]

SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o0]
INNER JOIN (
    SELECT DISTINCT [c].[City], [c].[CustomerID]
    FROM [Customers] AS [c]
    INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
    WHERE [c].[CustomerID] = N'ALFKI'
) AS [c0] ON [o0].[CustomerID] = [c0].[CustomerID]
ORDER BY [c0].[City], [c0].[CustomerID]",
                Sql);
        }

        public override void Include_collection_when_groupby(bool useString)
        {
            base.Include_collection_when_groupby(useString);

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[City], [c].[CustomerID]

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT [c].[City], [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'ALFKI'
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
ORDER BY [c0].[City], [c0].[CustomerID]",
                Sql);
        }

        public override void Include_collection_on_additional_from_clause2(bool useString)
        {
            base.Include_collection_on_additional_from_clause2(useString);

            Assert.Equal(
                @"@__p_0: 5

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT TOP(@__p_0) [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
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
                Assert.Equal(
                    @"@__p_0: 1
@__p_1: 2

SELECT [od.Order].[CustomerID]
FROM (
    SELECT [od0].*
    FROM [Order Details] AS [od0]
    WHERE [od0].[Quantity] = 10
    ORDER BY [od0].[OrderID], [od0].[ProductID]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
INNER JOIN [Orders] AS [od.Order] ON [t].[OrderID] = [od.Order].[OrderID]
ORDER BY [t].[OrderID], [t].[ProductID]",
                    Sql);
            }
        }

        public override void Include_duplicate_collection_result_operator2(bool useString)
        {
            base.Include_duplicate_collection_result_operator2(useString);
            if (SupportsOffset)
            {
                Assert.Equal(
                    @"@__p_1: 1
@__p_0: 2

SELECT TOP(@__p_1) [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT TOP(@__p_0) [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
) AS [t]
CROSS JOIN (
    SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
    FROM [Customers] AS [c2]
    ORDER BY [c2].[CustomerID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [t0]
ORDER BY [t].[CustomerID]

@__p_1: 1
@__p_0: 2

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT TOP(@__p_1) [t].[CustomerID]
    FROM (
        SELECT TOP(@__p_0) [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
        FROM [Customers] AS [c0]
        ORDER BY [c0].[CustomerID]
    ) AS [t]
    CROSS JOIN (
        SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
        FROM [Customers] AS [c2]
        ORDER BY [c2].[CustomerID]
        OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
    ) AS [t0]
    ORDER BY [t].[CustomerID]
) AS [t1] ON [o].[CustomerID] = [t1].[CustomerID]
ORDER BY [t1].[CustomerID]",
                    Sql);
            }
        }

        public override void Include_reference(bool useString)
        {
            base.Include_reference(useString);

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]",
                Sql);
        }

        public override void Include_multiple_references(bool useString)
        {
            base.Include_multiple_references(useString);

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
INNER JOIN [Products] AS [p] ON [o].[ProductID] = [p].[ProductID]",
                Sql);
        }

        public override void Include_reference_alias_generation(bool useString)
        {
            base.Include_reference_alias_generation(useString);

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]",
                Sql);
        }

        public override void Include_duplicate_reference(bool useString)
        {
            base.Include_duplicate_reference(useString);

            if (SupportsOffset)
            {
                Assert.Equal(
                    @"@__p_0: 2

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT TOP(@__p_0) [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    ORDER BY [o0].[CustomerID]
) AS [t]
CROSS JOIN (
    SELECT [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate]
    FROM [Orders] AS [o2]
    ORDER BY [o2].[CustomerID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [t0]
LEFT JOIN [Customers] AS [c] ON [t].[CustomerID] = [c].[CustomerID]
LEFT JOIN [Customers] AS [c0] ON [t0].[CustomerID] = [c0].[CustomerID]
ORDER BY [t].[CustomerID]",
                    Sql);
            }
        }

        public override void Include_duplicate_reference2(bool useString)
        {
            base.Include_duplicate_reference2(useString);

            if (SupportsOffset)
            {
                Assert.Equal(
                    @"@__p_0: 2

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT TOP(@__p_0) [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    ORDER BY [o0].[OrderID]
) AS [t]
CROSS JOIN (
    SELECT [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate]
    FROM [Orders] AS [o2]
    ORDER BY [o2].[OrderID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [t0]
LEFT JOIN [Customers] AS [c] ON [t].[CustomerID] = [c].[CustomerID]
ORDER BY [t].[OrderID]",
                    Sql);
            }
        }

        public override void Include_duplicate_reference3(bool useString)
        {
            base.Include_duplicate_reference3(useString);

            if (SupportsOffset)
            {
                Assert.Equal(
                    @"@__p_0: 2

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT TOP(@__p_0) [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    ORDER BY [o0].[OrderID]
) AS [t]
CROSS JOIN (
    SELECT [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate]
    FROM [Orders] AS [o2]
    ORDER BY [o2].[OrderID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [t0]
LEFT JOIN [Customers] AS [c] ON [t0].[CustomerID] = [c].[CustomerID]
ORDER BY [t].[OrderID]",
                    Sql);
            }
        }

        public override void Include_reference_when_projection(bool useString)
        {
            base.Include_reference_when_projection(useString);

            Assert.Equal(
                @"SELECT [o].[CustomerID]
FROM [Orders] AS [o]",
                Sql);
        }

        public override void Include_reference_with_filter_reordered(bool useString)
        {
            base.Include_reference_with_filter_reordered(useString);

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[CustomerID] = N'ALFKI'",
                Sql);
        }

        public override void Include_reference_with_filter(bool useString)
        {
            base.Include_reference_with_filter(useString);

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[CustomerID] = N'ALFKI'",
                Sql);
        }

        public override void Include_collection_dependent_already_tracked_as_no_tracking(bool useString)
        {
            base.Include_collection_dependent_already_tracked_as_no_tracking(useString);

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'

SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT TOP(2) [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'ALFKI'
    ORDER BY [c].[CustomerID]
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
ORDER BY [c0].[CustomerID]",
                Sql);
        }

        public override void Include_collection_dependent_already_tracked(bool useString)
        {
            base.Include_collection_dependent_already_tracked(useString);

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'

SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT TOP(2) [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'ALFKI'
    ORDER BY [c].[CustomerID]
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
ORDER BY [c0].[CustomerID]",
                Sql);
        }

        public override void Include_reference_dependent_already_tracked(bool useString)
        {
            base.Include_reference_dependent_already_tracked(useString);

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]",
                Sql);
        }

        public override void Include_reference_as_no_tracking(bool useString)
        {
            base.Include_reference_as_no_tracking(useString);

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]",
                Sql);
        }

        public override void Include_collection_as_no_tracking2(bool useString)
        {
            base.Include_collection_as_no_tracking2(useString);

            Assert.Equal(
                @"@__p_0: 5

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

@__p_0: 5

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT TOP(@__p_0) [c].[CustomerID]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
ORDER BY [c0].[CustomerID]",
                Sql);
        }

        public override void Include_with_complex_projection(bool useString)
        {
            base.Include_with_complex_projection(useString);

            Assert.Equal(
                @"SELECT [o].[CustomerID]
FROM [Orders] AS [o]",
                Sql);
        }

        public override void Include_with_take(bool useString)
        {
            base.Include_with_take(useString);

            Assert.Equal(
                @"@__p_0: 10

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[City] DESC, [c].[CustomerID]

@__p_0: 10

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT TOP(@__p_0) [c].[City], [c].[CustomerID]
    FROM [Customers] AS [c]
    ORDER BY [c].[City] DESC, [c].[CustomerID]
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
ORDER BY [c0].[City] DESC, [c0].[CustomerID]",
                Sql);
        }

        public override void Include_with_skip(bool useString)
        {
            base.Include_with_skip(useString);

            if (SupportsOffset)
            {
                Assert.Equal(
                    @"@__p_0: 80

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactName], [c].[CustomerID]
OFFSET @__p_0 ROWS

@__p_0: 80

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT [t].*
    FROM (
        SELECT [c].[ContactName], [c].[CustomerID]
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactName], [c].[CustomerID]
        OFFSET @__p_0 ROWS
    ) AS [t]
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
ORDER BY [c0].[ContactName], [c0].[CustomerID]",
                    Sql);
            }
        }

        public override void Then_include_collection_order_by_collection_column(bool useString)
        {
            base.Then_include_collection_order_by_collection_column(useString);

            Assert.Equal(
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
    ) AS [c0_0], [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'W' + N'%' AND (CHARINDEX(N'W', [c].[CustomerID]) = 1)
    ORDER BY [c0_0] DESC, [c].[CustomerID]
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
ORDER BY [c0].[c0_0] DESC, [c0].[CustomerID], [o].[OrderID]

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Order Details] AS [o0]
INNER JOIN (
    SELECT DISTINCT [c0].[c0_0], [c0].[CustomerID], [o].[OrderID]
    FROM [Orders] AS [o]
    INNER JOIN (
        SELECT DISTINCT TOP(1) (
            SELECT TOP(1) [oo].[OrderDate]
            FROM [Orders] AS [oo]
            WHERE [c].[CustomerID] = [oo].[CustomerID]
            ORDER BY [oo].[OrderDate] DESC
        ) AS [c0_0], [c].[CustomerID]
        FROM [Customers] AS [c]
        WHERE [c].[CustomerID] LIKE N'W' + N'%' AND (CHARINDEX(N'W', [c].[CustomerID]) = 1)
        ORDER BY [c0_0] DESC, [c].[CustomerID]
    ) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
) AS [o1] ON [o0].[OrderID] = [o1].[OrderID]
ORDER BY [o1].[c0_0] DESC, [o1].[CustomerID], [o1].[OrderID]",
                Sql);
        }

        public override void Include_collection_with_conditional_order_by(bool useString)
        {
            base.Include_collection_with_conditional_order_by(useString);

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY CASE
    WHEN [c].[CustomerID] LIKE N'S' + N'%' AND (CHARINDEX(N'S', [c].[CustomerID]) = 1)
    THEN 1 ELSE 2
END, [c].[CustomerID]

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT CASE
        WHEN [c].[CustomerID] LIKE N'S' + N'%' AND (CHARINDEX(N'S', [c].[CustomerID]) = 1)
        THEN 1 ELSE 2
    END AS [c0_0], [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
ORDER BY [c0].[c0_0], [c0].[CustomerID]",
                Sql);
        }

        private const string FileLineEnding = @"
";

        protected override void ClearLog() => TestSqlLoggerFactory.Reset();

        private static string Sql => TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);
    }
}
