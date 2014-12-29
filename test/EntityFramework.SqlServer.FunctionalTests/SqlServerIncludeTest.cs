// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerIncludeTest : IncludeTestBase<SqlServerNorthwindQueryFixture>
    {
        public override void Include_collection()
        {
            base.Include_collection();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void Include_reference_and_collection()
        {
            base.Include_reference_and_collection();

            Assert.Equal(
                @"SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [o].[OrderID]

SELECT [o0].[Discount], [o0].[OrderID], [o0].[ProductID], [o0].[Quantity], [o0].[UnitPrice]
FROM [Order Details] AS [o0]
INNER JOIN (
    SELECT DISTINCT [o].[OrderID]
    FROM [Orders] AS [o]
) AS [o] ON [o0].[OrderID] = [o].[OrderID]
ORDER BY [o].[OrderID]",
                Sql);
        }

        public override void Include_references_multi_level()
        {
            base.Include_references_multi_level();

            Assert.Equal(
                @"SELECT [od].[Discount], [od].[OrderID], [od].[ProductID], [od].[Quantity], [od].[UnitPrice], [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [o] ON [od].[OrderID] = [o].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]",
                Sql);
        }

        public override void Include_multiple_references_multi_level()
        {
            base.Include_multiple_references_multi_level();

            Assert.Equal(
                @"SELECT [od].[Discount], [od].[OrderID], [od].[ProductID], [od].[Quantity], [od].[UnitPrice], [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [p].[Discontinued], [p].[ProductID], [p].[ProductName]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [o] ON [od].[OrderID] = [o].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
INNER JOIN [Products] AS [p] ON [od].[ProductID] = [p].[ProductID]",
                Sql);
        }

        public override void Include_multiple_references_multi_level_reverse()
        {
            base.Include_multiple_references_multi_level_reverse();

            Assert.Equal(
                @"SELECT [od].[Discount], [od].[OrderID], [od].[ProductID], [od].[Quantity], [od].[UnitPrice], [p].[Discontinued], [p].[ProductID], [p].[ProductName], [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Order Details] AS [od]
INNER JOIN [Products] AS [p] ON [od].[ProductID] = [p].[ProductID]
INNER JOIN [Orders] AS [o] ON [od].[OrderID] = [o].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]",
                Sql);
        }

        public override void Include_references_and_collection_multi_level()
        {
            base.Include_references_and_collection_multi_level();

            Assert.Equal(
                @"SELECT [od].[Discount], [od].[OrderID], [od].[ProductID], [od].[Quantity], [od].[UnitPrice], [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [o] ON [od].[OrderID] = [o].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [od].[OrderID], [od].[ProductID]

SELECT [o0].[CustomerID], [o0].[OrderDate], [o0].[OrderID]
FROM [Orders] AS [o0]
INNER JOIN (
    SELECT DISTINCT [od].[OrderID], [od].[ProductID], [c].[CustomerID]
    FROM [Order Details] AS [od]
    INNER JOIN [Orders] AS [o] ON [od].[OrderID] = [o].[OrderID]
    LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
) AS [od] ON [o0].[CustomerID] = [od].[CustomerID]
ORDER BY [od].[OrderID], [od].[ProductID]",
                Sql);
        }

        public override void Include_multi_level_reference_and_collection_predicate()
        {
            base.Include_multi_level_reference_and_collection_predicate();

            Assert.Equal(
                @"SELECT TOP(@p0) [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] = @p1
ORDER BY [o].[OrderID]

SELECT [o0].[CustomerID], [o0].[OrderDate], [o0].[OrderID]
FROM [Orders] AS [o0]
INNER JOIN (
    SELECT DISTINCT TOP(@p0) [o].[OrderID], [c].[CustomerID]
    FROM [Orders] AS [o]
    LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
    WHERE [o].[OrderID] = @p1
) AS [o] ON [o0].[CustomerID] = [o].[CustomerID]
ORDER BY [o].[OrderID]",
                Sql);
        }

        public override void Include_collection_alias_generation()
        {
            base.Include_collection_alias_generation();

            Assert.Equal(
                @"SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
ORDER BY [o].[OrderID]

SELECT [o0].[Discount], [o0].[OrderID], [o0].[ProductID], [o0].[Quantity], [o0].[UnitPrice]
FROM [Order Details] AS [o0]
INNER JOIN (
    SELECT DISTINCT [o].[OrderID]
    FROM [Orders] AS [o]
) AS [o] ON [o0].[OrderID] = [o].[OrderID]
ORDER BY [o].[OrderID]",
                Sql);
        }

        public override void Include_collection_order_by_key()
        {
            base.Include_collection_order_by_key();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void Include_collection_order_by_non_key()
        {
            base.Include_collection_order_by_non_key();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[City], [c].[CustomerID]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT [c].[City], [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[City], [c].[CustomerID]",
                Sql);
        }

        public override void Include_collection_as_no_tracking()
        {
            base.Include_collection_as_no_tracking();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void Include_collection_principal_already_tracked()
        {
            base.Include_collection_principal_already_tracked();

            Assert.Equal(
                @"SELECT TOP(@p0) [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @p1

SELECT TOP(@p0) [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @p1
ORDER BY [c].[CustomerID]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT TOP(@p0) [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = @p1
) AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void Include_collection_principal_already_tracked_as_no_tracking()
        {
            base.Include_collection_principal_already_tracked_as_no_tracking();

            Assert.Equal(
                @"SELECT TOP(@p0) [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @p1

SELECT TOP(@p0) [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @p1
ORDER BY [c].[CustomerID]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT TOP(@p0) [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = @p1
) AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void Include_collection_with_filter()
        {
            base.Include_collection_with_filter();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @p0
ORDER BY [c].[CustomerID]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = @p0
) AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void Include_collection_with_filter_reordered()
        {
            base.Include_collection_with_filter_reordered();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @p0
ORDER BY [c].[CustomerID]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = @p0
) AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void Include_collection_when_projection()
        {
            base.Include_collection_when_projection();

            Assert.Equal(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Include_collection_on_join_clause_with_filter()
        {
            base.Include_collection_on_join_clause_with_filter();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = @p0
ORDER BY [c].[CustomerID]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT [c].[CustomerID]
    FROM [Customers] AS [c]
    INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
    WHERE [c].[CustomerID] = @p0
) AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void Include_collection_on_additional_from_clause_with_filter()
        {
            base.Include_collection_on_additional_from_clause_with_filter();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c1]
CROSS JOIN [Customers] AS [c]
WHERE [c].[CustomerID] = @p0
ORDER BY [c].[CustomerID]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT [c].[CustomerID]
    FROM [Customers] AS [c1]
    CROSS JOIN [Customers] AS [c]
    WHERE [c].[CustomerID] = @p0
) AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void Include_collection_on_additional_from_clause()
        {
            base.Include_collection_on_additional_from_clause();

            Assert.Equal(
                @"SELECT TOP(@p0) [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[CustomerID]

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void Include_duplicate_collection()
        {
            base.Include_duplicate_collection();

            Assert.Equal(
                @"SELECT TOP(@p0) [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID] OFFSET @p0 ROWS FETCH NEXT @p0 ROWS ONLY

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT TOP(@p0) [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[CustomerID]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT [t0].*
    FROM (
        SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[CustomerID] OFFSET @p0 ROWS FETCH NEXT @p0 ROWS ONLY
    ) AS [t0]
) AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[CustomerID]

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID] OFFSET @p0 ROWS FETCH NEXT @p0 ROWS ONLY",
                Sql);
        }

        public override void Include_duplicate_collection_result_operator()
        {
            base.Include_duplicate_collection_result_operator();

            Assert.Equal(
                @"SELECT TOP(@p0) [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID] OFFSET @p0 ROWS FETCH NEXT @p0 ROWS ONLY

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT TOP(@p0) [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[CustomerID]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT [t0].*
    FROM (
        SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[CustomerID] OFFSET @p0 ROWS FETCH NEXT @p0 ROWS ONLY
    ) AS [t0]
) AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void Include_collection_on_join_clause_with_order_by_and_filter()
        {
            base.Include_collection_on_join_clause_with_order_by_and_filter();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = @p0
ORDER BY [c].[City], [c].[CustomerID]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT [c].[City], [c].[CustomerID]
    FROM [Customers] AS [c]
    INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
    WHERE [c].[CustomerID] = @p0
) AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[City], [c].[CustomerID]",
                Sql);
        }

        public override void Include_collection_on_additional_from_clause2()
        {
            base.Include_collection_on_additional_from_clause2();

            Assert.Equal(
                @"SELECT TOP(@p0) [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

SELECT 1
FROM [Customers] AS [c]

SELECT 1
FROM [Customers] AS [c]

SELECT 1
FROM [Customers] AS [c]

SELECT 1
FROM [Customers] AS [c]

SELECT 1
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Include_duplicate_collection_result_operator2()
        {
            base.Include_duplicate_collection_result_operator2();

            Assert.Equal(
                @"SELECT TOP(@p0) [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID] OFFSET @p0 ROWS FETCH NEXT @p0 ROWS ONLY

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT TOP(@p0) [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void Include_reference()
        {
            base.Include_reference();

            Assert.Equal(
                @"SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]",
                Sql);
        }

        public override void Include_multiple_references()
        {
            base.Include_multiple_references();

            Assert.Equal(
                @"SELECT [o].[Discount], [o].[OrderID], [o].[ProductID], [o].[Quantity], [o].[UnitPrice], [o0].[CustomerID], [o0].[OrderDate], [o0].[OrderID], [p].[Discontinued], [p].[ProductID], [p].[ProductName]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
INNER JOIN [Products] AS [p] ON [o].[ProductID] = [p].[ProductID]",
                Sql);
        }

        public override void Include_reference_alias_generation()
        {
            base.Include_reference_alias_generation();

            Assert.Equal(
                @"SELECT [o].[Discount], [o].[OrderID], [o].[ProductID], [o].[Quantity], [o].[UnitPrice], [o0].[CustomerID], [o0].[OrderDate], [o0].[OrderID]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]",
                Sql);
        }

        public override void Include_duplicate_reference()
        {
            base.Include_duplicate_reference();

            Assert.Equal(
                @"SELECT TOP(@p0) [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [o].[CustomerID]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [o].[CustomerID] OFFSET @p0 ROWS FETCH NEXT @p0 ROWS ONLY

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [o].[CustomerID] OFFSET @p0 ROWS FETCH NEXT @p0 ROWS ONLY",
                Sql);
        }

        public override void Include_duplicate_reference2()
        {
            base.Include_duplicate_reference2();

            Assert.Equal(
                @"SELECT TOP(@p0) [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [o].[OrderID]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
ORDER BY [o].[OrderID] OFFSET @p0 ROWS FETCH NEXT @p0 ROWS ONLY

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
ORDER BY [o].[OrderID] OFFSET @p0 ROWS FETCH NEXT @p0 ROWS ONLY",
                Sql);
        }

        public override void Include_duplicate_reference3()
        {
            base.Include_duplicate_reference3();

            Assert.Equal(
                @"SELECT TOP(@p0) [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
ORDER BY [o].[OrderID]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [o].[OrderID] OFFSET @p0 ROWS FETCH NEXT @p0 ROWS ONLY

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [o].[OrderID] OFFSET @p0 ROWS FETCH NEXT @p0 ROWS ONLY",
                Sql);
        }

        public override void Include_reference_when_projection()
        {
            base.Include_reference_when_projection();

            Assert.Equal(
                @"SELECT [o].[CustomerID]
FROM [Orders] AS [o]",
                Sql);
        }

        public override void Include_reference_with_filter_reordered()
        {
            base.Include_reference_with_filter_reordered();

            Assert.Equal(
                @"SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[CustomerID] = @p0",
                Sql);
        }

        public override void Include_reference_with_filter()
        {
            base.Include_reference_with_filter();

            Assert.Equal(
                @"SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[CustomerID] = @p0",
                Sql);
        }

        public override void Include_collection_dependent_already_tracked_as_no_tracking()
        {
            base.Include_collection_dependent_already_tracked_as_no_tracking();

            Assert.Equal(
                @"SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = @p0

SELECT TOP(@p0) [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @p1
ORDER BY [c].[CustomerID]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT TOP(@p0) [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = @p1
) AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void Include_collection_dependent_already_tracked()
        {
            base.Include_collection_dependent_already_tracked();

            Assert.Equal(
                @"SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = @p0

SELECT TOP(@p0) [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @p1
ORDER BY [c].[CustomerID]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT TOP(@p0) [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = @p1
) AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void Include_reference_dependent_already_tracked()
        {
            base.Include_reference_dependent_already_tracked();

            Assert.Equal(
                @"SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = @p0

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]",
                Sql);
        }

        public override void Include_reference_as_no_tracking()
        {
            base.Include_reference_as_no_tracking();

            Assert.Equal(
                @"SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]",
                Sql);
        }

        public override void Include_collection_as_no_tracking2()
        {
            base.Include_collection_as_no_tracking2();

            Assert.Equal(
                @"SELECT TOP(@p0) [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT TOP(@p0) [c].[CustomerID]
    FROM [Customers] AS [c]
) AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public SqlServerIncludeTest(SqlServerNorthwindQueryFixture fixture)
            : base(fixture)
        {
        }

        private static string Sql
        {
            get { return TestSqlLoggerFactory.Sql; }
        }
    }
}
