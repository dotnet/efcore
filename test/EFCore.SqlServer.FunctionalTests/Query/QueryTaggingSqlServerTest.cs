// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryTaggingSqlServerTest : QueryTaggingTestBase<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        public QueryTaggingSqlServerTest(
            NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void Single_query_tag()
        {
            base.Single_query_tag();

            AssertSql(
                @"-- Yanni

SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void Single_query_multiple_tags()
        {
            base.Single_query_multiple_tags();

            AssertSql(
                @"-- Yanni

-- Enya

SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void Tags_on_subquery()
        {
            base.Tags_on_subquery();

            AssertSql(
                @"-- Yanni

-- Laurel

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
CROSS JOIN (
    SELECT TOP(5) [o].*
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override void Duplicate_tags()
        {
            base.Duplicate_tags();

            AssertSql(
                @"-- Yanni

SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void Tag_on_include_query()
        {
            base.Tag_on_include_query();

            AssertSql(
                @"-- Yanni

SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                //
                @"-- Yanni

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(1) [c0].[CustomerID]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]");
        }

        public override void Tag_on_scalar_query()
        {
            base.Tag_on_scalar_query();

            AssertSql(
                @"-- Yanni

SELECT TOP(1) [o].[OrderDate]
FROM [Orders] AS [o]
ORDER BY [o].[OrderID]");
        }

        public override void Single_query_multiline_tag()
        {
            base.Single_query_multiline_tag();

            AssertSql(
                @"-- Yanni
-- AND
-- Laurel

SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void Single_query_multiple_multiline_tag()
        {
            base.Single_query_multiple_multiline_tag();

            AssertSql(
                @"-- Yanni
-- AND
-- Laurel

-- Yet
-- Another
-- Multiline
-- Tag

SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void Single_query_multiline_tag_with_empty_lines()
        {
            base.Single_query_multiline_tag_with_empty_lines();

            AssertSql(
                @"-- Yanni
-- 
-- AND
-- 
-- Laurel

SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
