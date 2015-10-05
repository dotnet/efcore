// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class RowNumberPagingTest : QueryTestBase<NorthwindRowNumberPagingQuerySqlServerFixture>, IDisposable
    {
        public RowNumberPagingTest(NorthwindRowNumberPagingQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        public void Dispose()
        {
            //Assert for all tests that OFFSET or FETCH is never used
            Assert.DoesNotContain("OFFSET ", Sql);
            Assert.DoesNotContain("FETCH ", Sql);
        }

        public override void Skip()
        {
            base.Skip();

            Assert.Equal(
               @"SELECT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(ORDER BY [c].[CustomerID]) AS [__RowNumber__]
    FROM [Customers] AS [c]
) AS [t0]
WHERE [t0].[__RowNumber__] > 5",
               Sql);
        }

        public override void Skip_no_orderby()
        {
            base.Skip_no_orderby();

            Assert.EndsWith(
                @"SELECT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(ORDER BY @@RowCount) AS [__RowNumber__]
    FROM [Customers] AS [c]
) AS [t0]
WHERE [t0].[__RowNumber__] > 5",
                Sql);
        }

        public override void Skip_Take()
        {
            base.Skip_Take();

            Assert.Equal(
                @"SELECT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(ORDER BY [c].[ContactName]) AS [__RowNumber__]
    FROM [Customers] AS [c]
) AS [t0]
WHERE ([t0].[__RowNumber__] > 5) AND ([t0].[__RowNumber__] <= 15)",
                Sql);
        }

        public override void Join_Customers_Orders_Skip_Take()
        {
            base.Join_Customers_Orders_Skip_Take();
            Assert.Equal(
                @"SELECT [t0].[ContactName], [t0].[OrderID]
FROM (
    SELECT [c].[ContactName], [o].[OrderID], ROW_NUMBER() OVER(ORDER BY [o].[OrderID]) AS [__RowNumber__]
    FROM [Customers] AS [c]
    INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
) AS [t0]
WHERE ([t0].[__RowNumber__] > 10) AND ([t0].[__RowNumber__] <= 15)",
                Sql);
        }

        public override void Join_Customers_Orders_Projection_With_String_Concat_Skip_Take()
        {
            base.Join_Customers_Orders_Projection_With_String_Concat_Skip_Take();
            Assert.Equal(
                @"SELECT [t0].[c0], [t0].[OrderID]
FROM (
    SELECT ([c].[ContactName] + ' ') + [c].[ContactTitle] AS [c0], [o].[OrderID], ROW_NUMBER() OVER(ORDER BY [o].[OrderID]) AS [__RowNumber__]
    FROM [Customers] AS [c]
    INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
) AS [t0]
WHERE ([t0].[__RowNumber__] > 10) AND ([t0].[__RowNumber__] <= 15)",
                Sql);
        }

        public override void Take_Skip()
        {
            base.Take_Skip();

            Assert.Equal(@"SELECT [t1].*
FROM (
    SELECT [t0].*, ROW_NUMBER() OVER(ORDER BY [t0].[ContactName]) AS [__RowNumber__]
    FROM (
        SELECT TOP(10) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactName]
    ) AS [t0]
) AS [t1]
WHERE [t1].[__RowNumber__] > 5",
                Sql);
        }

        public override void Take_Skip_Distinct()
        {
            base.Take_Skip_Distinct();

            Assert.Equal(
                @"SELECT DISTINCT [t1].*
FROM (
    SELECT [t2].*
    FROM (
        SELECT [t0].*, ROW_NUMBER() OVER(ORDER BY [t0].[ContactName]) AS [__RowNumber__]
        FROM (
            SELECT TOP(10) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
            FROM [Customers] AS [c]
            ORDER BY [c].[ContactName]
        ) AS [t0]
    ) AS [t2]
    WHERE [t2].[__RowNumber__] > 5
) AS [t1]",
                Sql);
        }

        public override void Take_skip_null_coalesce_operator()
        {
            base.Take_skip_null_coalesce_operator();

            Assert.Equal(@"SELECT DISTINCT [t1].*
FROM (
    SELECT [t2].*
    FROM (
        SELECT [t0].*, ROW_NUMBER() OVER(ORDER BY COALESCE([t0].[Region], 'ZZ')) AS [__RowNumber__]
        FROM (
            SELECT TOP(10) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
            FROM [Customers] AS [c]
            ORDER BY COALESCE([c].[Region], 'ZZ')
        ) AS [t0]
    ) AS [t2]
    WHERE [t2].[__RowNumber__] > 5
) AS [t1]", Sql);
        }

        public override void Select_take_skip_null_coalesce_operator()
        {
            base.Select_take_skip_null_coalesce_operator();

            Assert.Equal(@"SELECT [t1].*
FROM (
    SELECT [t0].*, ROW_NUMBER() OVER(ORDER BY [Coalesce]) AS [__RowNumber__]
    FROM (
        SELECT TOP(10) [c].[CustomerID], [c].[CompanyName], COALESCE([c].[Region], 'ZZ') AS [Coalesce]
        FROM [Customers] AS [c]
        ORDER BY [Coalesce]
    ) AS [t0]
) AS [t1]
WHERE [t1].[__RowNumber__] > 5", Sql);
        }

        public override void String_Contains_Literal()
        {
            // skip. This is covered in QuerySqlServerTest
            // base.String_Contains_Literal()
        }

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
