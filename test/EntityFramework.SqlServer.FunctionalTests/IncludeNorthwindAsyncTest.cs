// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class IncludeNorthwindAsyncTest : IncludeNorthwindAsyncTestBase, IClassFixture<NorthwindQueryFixture>
    {
        public override async Task Include_collection()
        {
            await base.Include_collection();

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
                _fixture.Sql);
        }

        public override async Task Include_collection_order_by_key()
        {
            await base.Include_collection_order_by_key();

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
                _fixture.Sql);
        }

        public override async Task Include_collection_order_by_non_key()
        {
            await base.Include_collection_order_by_non_key();

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
                _fixture.Sql);
        }

        public override async Task Include_collection_as_no_tracking()
        {
            await base.Include_collection_as_no_tracking();

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
                _fixture.Sql);
        }

        public override async Task Include_collection_principal_already_tracked()
        {
            await base.Include_collection_principal_already_tracked();

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
                _fixture.Sql);
        }

        public override async Task Include_collection_principal_already_tracked_as_no_tracking()
        {
            await base.Include_collection_principal_already_tracked_as_no_tracking();

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
                _fixture.Sql);
        }

        public override async Task Include_collection_with_filter()
        {
            await base.Include_collection_with_filter();

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
                _fixture.Sql);
        }

        public override async Task Include_collection_with_filter_reordered()
        {
            await base.Include_collection_with_filter_reordered();

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
                _fixture.Sql);
        }

        public override async Task Include_collection_when_projection()
        {
            await base.Include_collection_when_projection();

            Assert.Equal(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]",
                _fixture.Sql);
        }

        public override async Task Include_collection_on_join_clause_with_filter()
        {
            await base.Include_collection_on_join_clause_with_filter();

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
                _fixture.Sql);
        }

        public override async Task Include_collection_on_additional_from_clause_with_filter()
        {
            await base.Include_collection_on_additional_from_clause_with_filter();

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
                _fixture.Sql);
        }

        public override async Task Include_collection_on_additional_from_clause()
        {
            await base.Include_collection_on_additional_from_clause();

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
                _fixture.Sql);
        }

        public override async Task Include_multiple_collection()
        {
            await base.Include_multiple_collection();

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
                _fixture.Sql);
        }

        public override async Task Include_multiple_collection_result_operator()
        {
            await base.Include_multiple_collection_result_operator();

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
                _fixture.Sql);
        }

        public override async Task Include_collection_on_join_clause_with_order_by_and_filter()
        {
            await base.Include_collection_on_join_clause_with_order_by_and_filter();

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
                _fixture.Sql);
        }

        public override async Task Include_collection_on_additional_from_clause2()
        {
            await base.Include_collection_on_additional_from_clause2();

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
                _fixture.Sql);
        }

        public override async Task Include_multiple_collection_result_operator2()
        {
            await base.Include_multiple_collection_result_operator2();

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
                _fixture.Sql);
        }

        public override async Task Include_reference()
        {
            await base.Include_reference();

            Assert.Equal(
                @"SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [c].[CustomerID] = [o].[CustomerID]",
                _fixture.Sql);
        }

        public override async Task Include_multiple_reference()
        {
            await base.Include_multiple_reference();

            Assert.Equal(
                @"SELECT TOP(@p0) [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [o].[CustomerID]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [o].[CustomerID] OFFSET @p0 ROWS FETCH NEXT @p0 ROWS ONLY

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [o].[CustomerID] OFFSET @p0 ROWS FETCH NEXT @p0 ROWS ONLY",
                _fixture.Sql);
        }

        public override async Task Include_multiple_reference2()
        {
            await base.Include_multiple_reference2();

            Assert.Equal(
                @"SELECT TOP(@p0) [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [o].[OrderID]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
ORDER BY [o].[OrderID] OFFSET @p0 ROWS FETCH NEXT @p0 ROWS ONLY

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
ORDER BY [o].[OrderID] OFFSET @p0 ROWS FETCH NEXT @p0 ROWS ONLY",
                _fixture.Sql);
        }

        public override async Task Include_multiple_reference3()
        {
            await base.Include_multiple_reference3();

            Assert.Equal(
                @"SELECT TOP(@p0) [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
ORDER BY [o].[OrderID]

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [o].[OrderID] OFFSET @p0 ROWS FETCH NEXT @p0 ROWS ONLY

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [o].[OrderID] OFFSET @p0 ROWS FETCH NEXT @p0 ROWS ONLY",
                _fixture.Sql);
        }

        public override async Task Include_reference_when_projection()
        {
            await base.Include_reference_when_projection();

            Assert.Equal(
                @"SELECT [o].[CustomerID]
FROM [Orders] AS [o]",
                _fixture.Sql);
        }

        public override async Task Include_reference_with_filter_reordered()
        {
            await base.Include_reference_with_filter_reordered();

            Assert.Equal(
                @"SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [o].[CustomerID] = @p0",
                _fixture.Sql);
        }

        public override async Task Include_reference_with_filter()
        {
            await base.Include_reference_with_filter();

            Assert.Equal(
                @"SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [o].[CustomerID] = @p0",
                _fixture.Sql);
        }

        public override async Task Include_collection_dependent_already_tracked_as_no_tracking()
        {
            await base.Include_collection_dependent_already_tracked_as_no_tracking();

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
                _fixture.Sql);
        }

        public override async Task Include_collection_dependent_already_tracked()
        {
            await base.Include_collection_dependent_already_tracked();

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
                _fixture.Sql);
        }

        public override async Task Include_reference_dependent_already_tracked()
        {
            await base.Include_reference_dependent_already_tracked();

            Assert.Equal(
                @"SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = @p0

SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [c].[CustomerID] = [o].[CustomerID]",
                _fixture.Sql);
        }

        public override async Task Include_reference_as_no_tracking()
        {
            await base.Include_reference_as_no_tracking();

            Assert.Equal(
                @"SELECT [o].[CustomerID], [o].[OrderDate], [o].[OrderID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [c].[CustomerID] = [o].[CustomerID]",
                _fixture.Sql);
        }

        public override async Task Include_collection_as_no_tracking2()
        {
            await base.Include_collection_as_no_tracking2();

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
                _fixture.Sql);
        }

        private readonly NorthwindQueryFixture _fixture;

        public IncludeNorthwindAsyncTest(NorthwindQueryFixture fixture)
        {
            _fixture = fixture;
            _fixture.InitLogger();
        }

        protected override DbContext CreateContext()
        {
            return _fixture.CreateContext();
        }
    }
}
