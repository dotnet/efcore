// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class RowNumberPagingTest : SimpleQueryTestBase<NorthwindRowNumberPagingQuerySqlServerFixture>, IDisposable
    {
        public RowNumberPagingTest(NorthwindRowNumberPagingQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public void Dispose()
        {
            //Assert for all tests that OFFSET or FETCH is never used
            Assert.All(Fixture.TestSqlLoggerFactory.SqlStatements, t => Assert.DoesNotMatch("\\W+OFFSET", t));
            Assert.All(Fixture.TestSqlLoggerFactory.SqlStatements, t => Assert.DoesNotContain("FETCH", t));
        }

        public override async Task Skip(bool isAsync)
        {
            await base.Skip(isAsync);

            AssertSql(
                @"@__p_0='5'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(ORDER BY [c].[CustomerID]) AS [__RowNumber__]
    FROM [Customers] AS [c]
) AS [t]
WHERE [t].[__RowNumber__] > @__p_0");
        }

        public override async Task Skip_no_orderby(bool isAsync)
        {
            await base.Skip_no_orderby(isAsync);

            AssertSql(
                @"@__p_0='5'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(ORDER BY @@RowCount) AS [__RowNumber__]
    FROM [Customers] AS [c]
) AS [t]
WHERE [t].[__RowNumber__] > @__p_0");
        }

        public override async Task Skip_Take(bool isAsync)
        {
            await base.Skip_Take(isAsync);

            AssertSql(
                @"@__p_0='5'
@__p_1='10'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(ORDER BY [c].[ContactName]) AS [__RowNumber__]
    FROM [Customers] AS [c]
) AS [t]
WHERE ([t].[__RowNumber__] > @__p_0) AND ([t].[__RowNumber__] <= (@__p_0 + @__p_1))");
        }

        public override async Task Join_Customers_Orders_Skip_Take(bool isAsync)
        {
            await base.Join_Customers_Orders_Skip_Take(isAsync);

            AssertSql(
                @"@__p_0='10'
@__p_1='5'

SELECT [t].[ContactName], [t].[OrderID]
FROM (
    SELECT [c].[ContactName], [o].[OrderID], ROW_NUMBER() OVER(ORDER BY [o].[OrderID]) AS [__RowNumber__]
    FROM [Customers] AS [c]
    INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
) AS [t]
WHERE ([t].[__RowNumber__] > @__p_0) AND ([t].[__RowNumber__] <= (@__p_0 + @__p_1))");
        }

        public override async Task Join_Customers_Orders_Skip_Take_followed_by_constant_projection(bool isAsync)
        {
            await base.Join_Customers_Orders_Skip_Take_followed_by_constant_projection(isAsync);

            AssertSql(
                "");
        }

        public override async Task Join_Customers_Orders_Projection_With_String_Concat_Skip_Take(bool isAsync)
        {
            await base.Join_Customers_Orders_Projection_With_String_Concat_Skip_Take(isAsync);

            AssertSql(
                @"@__p_0='10'
@__p_1='5'

SELECT [t].[Contact], [t].[OrderID]
FROM (
    SELECT ([c].[ContactName] + N' ') + [c].[ContactTitle] AS [Contact], [o].[OrderID], ROW_NUMBER() OVER(ORDER BY [o].[OrderID]) AS [__RowNumber__]
    FROM [Customers] AS [c]
    INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
) AS [t]
WHERE ([t].[__RowNumber__] > @__p_0) AND ([t].[__RowNumber__] <= (@__p_0 + @__p_1))");
        }

        public override async Task Join_Customers_Orders_Orders_Skip_Take_Same_Properties(bool isAsync)
        {
            await base.Join_Customers_Orders_Orders_Skip_Take_Same_Properties(isAsync);

            AssertSql(
                @"@__p_0='10'
@__p_1='5'

SELECT [t].[OrderID], [t].[CustomerIDA], [t].[CustomerIDB], [t].[ContactNameA], [t].[ContactNameB]
FROM (
    SELECT [o].[OrderID], [ca].[CustomerID] AS [CustomerIDA], [cb].[CustomerID] AS [CustomerIDB], [ca].[ContactName] AS [ContactNameA], [cb].[ContactName] AS [ContactNameB], ROW_NUMBER() OVER(ORDER BY [o].[OrderID]) AS [__RowNumber__]
    FROM [Orders] AS [o]
    INNER JOIN [Customers] AS [ca] ON [o].[CustomerID] = [ca].[CustomerID]
    INNER JOIN [Customers] AS [cb] ON [o].[CustomerID] = [cb].[CustomerID]
) AS [t]
WHERE ([t].[__RowNumber__] > @__p_0) AND ([t].[__RowNumber__] <= (@__p_0 + @__p_1))");
        }

        public override async Task Take_Skip(bool isAsync)
        {
            await base.Take_Skip(isAsync);

            AssertSql(
                @"@__p_0='10'
@__p_1='5'

SELECT [t0].*
FROM (
    SELECT [t].*, ROW_NUMBER() OVER(ORDER BY [t].[ContactName]) AS [__RowNumber__]
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactName]
    ) AS [t]
) AS [t0]
WHERE [t0].[__RowNumber__] > @__p_1");
        }

        public override async Task Take_Skip_Distinct(bool isAsync)
        {
            await base.Take_Skip_Distinct(isAsync);

            AssertSql(
                @"@__p_0='10'
@__p_1='5'

SELECT DISTINCT [t0].*
FROM (
    SELECT [t1].*
    FROM (
        SELECT [t].*, ROW_NUMBER() OVER(ORDER BY [t].[ContactName]) AS [__RowNumber__]
        FROM (
            SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
            FROM [Customers] AS [c]
            ORDER BY [c].[ContactName]
        ) AS [t]
    ) AS [t1]
    WHERE [t1].[__RowNumber__] > @__p_1
) AS [t0]");
        }

        public override async Task Take_skip_null_coalesce_operator(bool isAsync)
        {
            await base.Take_skip_null_coalesce_operator(isAsync);

            AssertSql(
                @"@__p_0='10'
@__p_1='5'

SELECT DISTINCT [t0].*
FROM (
    SELECT [t1].*
    FROM (
        SELECT [t].*, ROW_NUMBER() OVER(ORDER BY [t].[c]) AS [__RowNumber__]
        FROM (
            SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], COALESCE([c].[Region], N'ZZ') AS [c]
            FROM [Customers] AS [c]
            ORDER BY [c]
        ) AS [t]
    ) AS [t1]
    WHERE [t1].[__RowNumber__] > @__p_1
) AS [t0]");
        }

        public override async Task Select_take_skip_null_coalesce_operator(bool isAsync)
        {
            await base.Select_take_skip_null_coalesce_operator(isAsync);

            AssertSql(
                @"@__p_0='10'
@__p_1='5'

SELECT [t0].*
FROM (
    SELECT [t].*, ROW_NUMBER() OVER(ORDER BY [t].[Region]) AS [__RowNumber__]
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID], [c].[CompanyName], COALESCE([c].[Region], N'ZZ') AS [Region]
        FROM [Customers] AS [c]
        ORDER BY [Region]
    ) AS [t]
) AS [t0]
WHERE [t0].[__RowNumber__] > @__p_1");
        }

        public override async Task String_Contains_Literal(bool isAsync)
        {
            await AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.ContactName.Contains("M")), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains("M") || c.ContactName.Contains("m")), // case-sensitive
                entryCount: 34);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE CHARINDEX(N'M', [c].[ContactName]) > 0");
        }

        public override async Task String_Contains_MethodCall(bool isAsync)
        {
            await AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.ContactName.Contains(LocalMethod1())), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains(LocalMethod1().ToLower()) || c.ContactName.Contains(LocalMethod1().ToUpper())), // case-sensitive
                entryCount: 34);

            AssertSql(
                @"@__LocalMethod1_0='M' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (CHARINDEX(@__LocalMethod1_0, [c].[ContactName]) > 0) OR (@__LocalMethod1_0 = N'')");
        }

        public override async Task OrderBy_skip_take(bool isAsync)
        {
            await base.OrderBy_skip_take(isAsync);

            AssertSql(
                @"@__p_0='5'
@__p_1='8'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(ORDER BY [c].[ContactTitle], [c].[ContactName]) AS [__RowNumber__]
    FROM [Customers] AS [c]
) AS [t]
WHERE ([t].[__RowNumber__] > @__p_0) AND ([t].[__RowNumber__] <= (@__p_0 + @__p_1))");
        }

        public override async Task OrderBy_skip_skip_take(bool isAsync)
        {
            await base.OrderBy_skip_skip_take(isAsync);

            AssertSql(
                @"@__p_0='5'
@__p_1='8'
@__p_2='3'

SELECT [t1].*
FROM (
    SELECT [t].*, ROW_NUMBER() OVER(ORDER BY [t].[ContactTitle], [t].[ContactName]) AS [__RowNumber__1]
    FROM (
        SELECT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
        FROM (
            SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(ORDER BY [c].[ContactTitle], [c].[ContactName]) AS [__RowNumber__]
            FROM [Customers] AS [c]
        ) AS [t0]
        WHERE [t0].[__RowNumber__] > @__p_0
    ) AS [t]
) AS [t1]
WHERE ([t1].[__RowNumber__1] > @__p_1) AND ([t1].[__RowNumber__1] <= (@__p_1 + @__p_2))");
        }

        public override async Task OrderBy_skip_take_take(bool isAsync)
        {
            await base.OrderBy_skip_take_take(isAsync);

            AssertSql(
                @"@__p_2='3'
@__p_0='5'
@__p_1='8'

SELECT TOP(@__p_2) [t].*
FROM (
    SELECT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
    FROM (
        SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(ORDER BY [c].[ContactTitle], [c].[ContactName]) AS [__RowNumber__]
        FROM [Customers] AS [c]
    ) AS [t0]
    WHERE ([t0].[__RowNumber__] > @__p_0) AND ([t0].[__RowNumber__] <= (@__p_0 + @__p_1))
) AS [t]
ORDER BY [t].[ContactTitle], [t].[ContactName]");
        }

        public override async Task OrderBy_skip_take_take_take_take(bool isAsync)
        {
            await base.OrderBy_skip_take_take_take_take(isAsync);

            AssertSql(
                @"@__p_0='5'
@__p_3='8'
@__p_2='10'
@__p_1='15'

SELECT TOP(@__p_0) [t1].*
FROM (
    SELECT TOP(@__p_3) [t0].*
    FROM (
        SELECT TOP(@__p_2) [t].*
        FROM (
            SELECT [t2].[CustomerID], [t2].[Address], [t2].[City], [t2].[CompanyName], [t2].[ContactName], [t2].[ContactTitle], [t2].[Country], [t2].[Fax], [t2].[Phone], [t2].[PostalCode], [t2].[Region]
            FROM (
                SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(ORDER BY [c].[ContactTitle], [c].[ContactName]) AS [__RowNumber__]
                FROM [Customers] AS [c]
            ) AS [t2]
            WHERE ([t2].[__RowNumber__] > @__p_0) AND ([t2].[__RowNumber__] <= (@__p_0 + @__p_1))
        ) AS [t]
        ORDER BY [t].[ContactTitle], [t].[ContactName]
    ) AS [t0]
    ORDER BY [t0].[ContactTitle], [t0].[ContactName]
) AS [t1]
ORDER BY [t1].[ContactTitle], [t1].[ContactName]");
        }

        public override async Task OrderBy_skip_take_skip_take_skip(bool isAsync)
        {
            await base.OrderBy_skip_take_skip_take_skip(isAsync);

            AssertSql(
                @"@__p_0='5'
@__p_1='15'
@__p_2='2'
@__p_3='8'

SELECT [t3].*
FROM (
    SELECT [t0].*, ROW_NUMBER() OVER(ORDER BY [t0].[ContactTitle], [t0].[ContactName]) AS [__RowNumber__2]
    FROM (
        SELECT [t2].*
        FROM (
            SELECT [t].*, ROW_NUMBER() OVER(ORDER BY [t].[ContactTitle], [t].[ContactName]) AS [__RowNumber__1]
            FROM (
                SELECT [t1].[CustomerID], [t1].[Address], [t1].[City], [t1].[CompanyName], [t1].[ContactName], [t1].[ContactTitle], [t1].[Country], [t1].[Fax], [t1].[Phone], [t1].[PostalCode], [t1].[Region]
                FROM (
                    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(ORDER BY [c].[ContactTitle], [c].[ContactName]) AS [__RowNumber__]
                    FROM [Customers] AS [c]
                ) AS [t1]
                WHERE ([t1].[__RowNumber__] > @__p_0) AND ([t1].[__RowNumber__] <= (@__p_0 + @__p_1))
            ) AS [t]
        ) AS [t2]
        WHERE ([t2].[__RowNumber__1] > @__p_2) AND ([t2].[__RowNumber__1] <= (@__p_2 + @__p_3))
    ) AS [t0]
) AS [t3]
WHERE [t3].[__RowNumber__2] > @__p_0");
        }

        public override async Task OrderBy_skip_take_distinct(bool isAsync)
        {
            await base.OrderBy_skip_take_distinct(isAsync);

            AssertSql(
                @"@__p_0='5'
@__p_1='15'

SELECT DISTINCT [t].*
FROM (
    SELECT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
    FROM (
        SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(ORDER BY [c].[ContactTitle], [c].[ContactName]) AS [__RowNumber__]
        FROM [Customers] AS [c]
    ) AS [t0]
    WHERE ([t0].[__RowNumber__] > @__p_0) AND ([t0].[__RowNumber__] <= (@__p_0 + @__p_1))
) AS [t]");
        }

        public override async Task OrderBy_coalesce_take_distinct(bool isAsync)
        {
            await base.OrderBy_coalesce_take_distinct(isAsync);

            AssertSql(
                @"@__p_0='15'

SELECT DISTINCT [t].*
FROM (
    SELECT TOP(@__p_0) [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
    FROM [Products] AS [p]
    ORDER BY COALESCE([p].[UnitPrice], 0.0)
) AS [t]");
        }

        public override async Task OrderBy_coalesce_skip_take_distinct(bool isAsync)
        {
            await base.OrderBy_coalesce_skip_take_distinct(isAsync);

            AssertSql(
                @"@__p_0='5'
@__p_1='15'

SELECT DISTINCT [t].*
FROM (
    SELECT [t0].[ProductID], [t0].[Discontinued], [t0].[ProductName], [t0].[SupplierID], [t0].[UnitPrice], [t0].[UnitsInStock]
    FROM (
        SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock], ROW_NUMBER() OVER(ORDER BY COALESCE([p].[UnitPrice], 0.0)) AS [__RowNumber__]
        FROM [Products] AS [p]
    ) AS [t0]
    WHERE ([t0].[__RowNumber__] > @__p_0) AND ([t0].[__RowNumber__] <= (@__p_0 + @__p_1))
) AS [t]");
        }

        public override async Task OrderBy_coalesce_skip_take_distinct_take(bool isAsync)
        {
            await base.OrderBy_coalesce_skip_take_distinct_take(isAsync);

            AssertSql(
                @"@__p_0='5'
@__p_1='15'

SELECT DISTINCT TOP(@__p_0) [t].*
FROM (
    SELECT [t0].[ProductID], [t0].[Discontinued], [t0].[ProductName], [t0].[SupplierID], [t0].[UnitPrice], [t0].[UnitsInStock]
    FROM (
        SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock], ROW_NUMBER() OVER(ORDER BY COALESCE([p].[UnitPrice], 0.0)) AS [__RowNumber__]
        FROM [Products] AS [p]
    ) AS [t0]
    WHERE ([t0].[__RowNumber__] > @__p_0) AND ([t0].[__RowNumber__] <= (@__p_0 + @__p_1))
) AS [t]");
        }

        public override async Task OrderBy_skip_take_distinct_orderby_take(bool isAsync)
        {
            await base.OrderBy_skip_take_distinct_orderby_take(isAsync);

            AssertSql(
                @"@__p_2='8'
@__p_0='5'
@__p_1='15'

SELECT TOP(@__p_2) [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT DISTINCT [t].*
    FROM (
        SELECT [t1].[CustomerID], [t1].[Address], [t1].[City], [t1].[CompanyName], [t1].[ContactName], [t1].[ContactTitle], [t1].[Country], [t1].[Fax], [t1].[Phone], [t1].[PostalCode], [t1].[Region]
        FROM (
            SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(ORDER BY [c].[ContactTitle], [c].[ContactName]) AS [__RowNumber__]
            FROM [Customers] AS [c]
        ) AS [t1]
        WHERE ([t1].[__RowNumber__] > @__p_0) AND ([t1].[__RowNumber__] <= (@__p_0 + @__p_1))
    ) AS [t]
) AS [t0]
ORDER BY [t0].[ContactTitle]");
        }

        public override async Task Skip_Take_Any(bool isAsync)
        {
            await base.Skip_Take_Any(isAsync);

            AssertSql(
                @"@__p_0='5'
@__p_1='10'

SELECT CASE
    WHEN EXISTS (
        SELECT [t].[c]
        FROM (
            SELECT 1 AS [c], ROW_NUMBER() OVER(ORDER BY [c].[ContactName]) AS [__RowNumber__]
            FROM [Customers] AS [c]
        ) AS [t]
        WHERE ([t].[__RowNumber__] > @__p_0) AND ([t].[__RowNumber__] <= (@__p_0 + @__p_1)))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override async Task Skip_Take_All(bool isAsync)
        {
            await base.Skip_Take_All(isAsync);

            AssertSql(
                @"@__p_0='4'
@__p_1='7'

SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM (
            SELECT [t0].*
            FROM (
                SELECT [c].*, ROW_NUMBER() OVER(ORDER BY [c].[CustomerID]) AS [__RowNumber__]
                FROM [Customers] AS [c]
            ) AS [t0]
            WHERE ([t0].[__RowNumber__] > @__p_0) AND ([t0].[__RowNumber__] <= (@__p_0 + @__p_1))
        ) AS [t]
        WHERE NOT ([t].[CustomerID] LIKE N'B' + N'%') OR (LEFT([t].[CustomerID], LEN(N'B')) <> N'B'))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override async Task Take_All(bool isAsync)
        {
            await base.Take_All(isAsync);

            AssertSql(
                @"@__p_0='4'

SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM (
            SELECT TOP(@__p_0) [c].*
            FROM [Customers] AS [c]
            ORDER BY [c].[CustomerID]
        ) AS [t]
        WHERE NOT ([t].[CustomerID] LIKE N'A' + N'%') OR (LEFT([t].[CustomerID], LEN(N'A')) <> N'A'))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override async Task Skip_Take_Any_with_predicate(bool isAsync)
        {
            await base.Skip_Take_Any_with_predicate(isAsync);

            AssertSql(
                @"@__p_0='5'
@__p_1='7'

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM (
            SELECT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
            FROM (
                SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(ORDER BY [c].[CustomerID]) AS [__RowNumber__]
                FROM [Customers] AS [c]
            ) AS [t0]
            WHERE ([t0].[__RowNumber__] > @__p_0) AND ([t0].[__RowNumber__] <= (@__p_0 + @__p_1))
        ) AS [t]
        WHERE [t].[CustomerID] LIKE N'C' + N'%' AND (LEFT([t].[CustomerID], LEN(N'C')) = N'C'))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override async Task Take_Any_with_predicate(bool isAsync)
        {
            await base.Take_Any_with_predicate(isAsync);

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
        WHERE [t].[CustomerID] LIKE N'B' + N'%' AND (LEFT([t].[CustomerID], LEN(N'B')) = N'B'))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override async Task Include_with_orderby_skip_preserves_ordering(bool isAsync)
        {
            await base.Include_with_orderby_skip_preserves_ordering(isAsync);

            AssertSql(
                @"@__p_0='40'
@__p_1='5'

SELECT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(ORDER BY [c].[City], [c].[CustomerID]) AS [__RowNumber__]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] NOT IN (N'VAFFE', N'DRACD')
) AS [t0]
WHERE ([t0].[__RowNumber__] > @__p_0) AND ([t0].[__RowNumber__] <= (@__p_0 + @__p_1))",
                //
                @"@__p_0='40'
@__p_1='5'

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [t1].[CustomerID], [t1].[City]
    FROM (
        SELECT [c0].[CustomerID], [c0].[City], ROW_NUMBER() OVER(ORDER BY [c0].[City], [c0].[CustomerID]) AS [__RowNumber__]
        FROM [Customers] AS [c0]
        WHERE [c0].[CustomerID] NOT IN (N'VAFFE', N'DRACD')
    ) AS [t1]
    WHERE ([t1].[__RowNumber__] > @__p_0) AND ([t1].[__RowNumber__] <= (@__p_0 + @__p_1))
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[City], [t].[CustomerID]");
        }

        public override async Task GroupJoin_customers_orders_count_preserves_ordering(bool isAsync)
        {
            await base.GroupJoin_customers_orders_count_preserves_ordering(isAsync);

            AssertSql(
                @"@__p_0='5'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] NOT IN (N'VAFFE', N'DRACD')
    ORDER BY [c].[City]
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[City], [t].[CustomerID]");
        }

        public override async Task Select_take_average(bool isAsync)
        {
            await base.Select_take_average(isAsync);

            AssertSql(
                @"@__p_0='10'

SELECT AVG(CAST([t].[OrderID] AS float))
FROM (
    SELECT TOP(@__p_0) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]");
        }

        public override async Task Select_take_count(bool isAsync)
        {
            await base.Select_take_count(isAsync);

            AssertSql(
                @"@__p_0='7'

SELECT COUNT(*)
FROM (
    SELECT TOP(@__p_0) [c].*
    FROM [Customers] AS [c]
) AS [t]");
        }

        public override async Task Select_orderBy_take_count(bool isAsync)
        {
            await base.Select_orderBy_take_count(isAsync);

            AssertSql(
                @"@__p_0='7'

SELECT COUNT(*)
FROM (
    SELECT TOP(@__p_0) [c].*
    FROM [Customers] AS [c]
    ORDER BY [c].[Country]
) AS [t]");
        }

        public override async Task Select_take_long_count(bool isAsync)
        {
            await base.Select_take_long_count(isAsync);

            AssertSql(
                @"@__p_0='7'

SELECT COUNT_BIG(*)
FROM (
    SELECT TOP(@__p_0) [c].*
    FROM [Customers] AS [c]
) AS [t]");
        }

        public override async Task Select_orderBy_take_long_count(bool isAsync)
        {
            await base.Select_orderBy_take_long_count(isAsync);

            AssertSql(
                @"@__p_0='7'

SELECT COUNT_BIG(*)
FROM (
    SELECT TOP(@__p_0) [c].*
    FROM [Customers] AS [c]
    ORDER BY [c].[Country]
) AS [t]");
        }

        public override async Task Select_take_max(bool isAsync)
        {
            await base.Select_take_max(isAsync);

            AssertSql(
                @"@__p_0='10'

SELECT MAX([t].[OrderID])
FROM (
    SELECT TOP(@__p_0) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]");
        }

        public override async Task Select_take_min(bool isAsync)
        {
            await base.Select_take_min(isAsync);

            AssertSql(
                @"@__p_0='10'

SELECT MIN([t].[OrderID])
FROM (
    SELECT TOP(@__p_0) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]");
        }

        public override async Task Select_take_sum(bool isAsync)
        {
            await base.Select_take_sum(isAsync);

            AssertSql(
                @"@__p_0='10'

SELECT SUM([t].[OrderID])
FROM (
    SELECT TOP(@__p_0) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]");
        }

        public override async Task Select_skip_average(bool isAsync)
        {
            await base.Select_skip_average(isAsync);

            AssertSql(
                @"@__p_0='10'

SELECT AVG(CAST([t].[OrderID] AS float))
FROM (
    SELECT [t0].[OrderID]
    FROM (
        SELECT [o].[OrderID], ROW_NUMBER() OVER(ORDER BY [o].[OrderID]) AS [__RowNumber__]
        FROM [Orders] AS [o]
    ) AS [t0]
    WHERE [t0].[__RowNumber__] > @__p_0
) AS [t]");
        }

        public override async Task Select_skip_count(bool isAsync)
        {
            await base.Select_skip_count(isAsync);

            AssertSql(
                @"@__p_0='7'

SELECT COUNT(*)
FROM (
    SELECT [t0].*
    FROM (
        SELECT [c].*, ROW_NUMBER() OVER(ORDER BY @@RowCount) AS [__RowNumber__]
        FROM [Customers] AS [c]
    ) AS [t0]
    WHERE [t0].[__RowNumber__] > @__p_0
) AS [t]");
        }

        public override async Task Select_orderBy_skip_count(bool isAsync)
        {
            await base.Select_orderBy_skip_count(isAsync);

            AssertSql(
                @"@__p_0='7'

SELECT COUNT(*)
FROM (
    SELECT [t0].*
    FROM (
        SELECT [c].*, ROW_NUMBER() OVER(ORDER BY [c].[Country]) AS [__RowNumber__]
        FROM [Customers] AS [c]
    ) AS [t0]
    WHERE [t0].[__RowNumber__] > @__p_0
) AS [t]");
        }

        public override async Task Select_skip_long_count(bool isAsync)
        {
            await base.Select_skip_long_count(isAsync);

            AssertSql(
                @"@__p_0='7'

SELECT COUNT_BIG(*)
FROM (
    SELECT [t0].*
    FROM (
        SELECT [c].*, ROW_NUMBER() OVER(ORDER BY @@RowCount) AS [__RowNumber__]
        FROM [Customers] AS [c]
    ) AS [t0]
    WHERE [t0].[__RowNumber__] > @__p_0
) AS [t]");
        }

        public override async Task Select_orderBy_skip_long_count(bool isAsync)
        {
            await base.Select_orderBy_skip_long_count(isAsync);

            AssertSql(
                @"@__p_0='7'

SELECT COUNT_BIG(*)
FROM (
    SELECT [t0].*
    FROM (
        SELECT [c].*, ROW_NUMBER() OVER(ORDER BY [c].[Country]) AS [__RowNumber__]
        FROM [Customers] AS [c]
    ) AS [t0]
    WHERE [t0].[__RowNumber__] > @__p_0
) AS [t]");
        }

        public override async Task Select_skip_max(bool isAsync)
        {
            await base.Select_skip_max(isAsync);

            AssertSql(
                @"@__p_0='10'

SELECT MAX([t].[OrderID])
FROM (
    SELECT [t0].[OrderID]
    FROM (
        SELECT [o].[OrderID], ROW_NUMBER() OVER(ORDER BY [o].[OrderID]) AS [__RowNumber__]
        FROM [Orders] AS [o]
    ) AS [t0]
    WHERE [t0].[__RowNumber__] > @__p_0
) AS [t]");
        }

        public override async Task Select_skip_min(bool isAsync)
        {
            await base.Select_skip_min(isAsync);

            AssertSql(
                @"@__p_0='10'

SELECT MIN([t].[OrderID])
FROM (
    SELECT [t0].[OrderID]
    FROM (
        SELECT [o].[OrderID], ROW_NUMBER() OVER(ORDER BY [o].[OrderID]) AS [__RowNumber__]
        FROM [Orders] AS [o]
    ) AS [t0]
    WHERE [t0].[__RowNumber__] > @__p_0
) AS [t]");
        }

        public override async Task Select_skip_sum(bool isAsync)
        {
            await base.Select_skip_sum(isAsync);

            AssertSql(
                @"@__p_0='10'

SELECT SUM([t].[OrderID])
FROM (
    SELECT [t0].[OrderID]
    FROM (
        SELECT [o].[OrderID], ROW_NUMBER() OVER(ORDER BY [o].[OrderID]) AS [__RowNumber__]
        FROM [Orders] AS [o]
    ) AS [t0]
    WHERE [t0].[__RowNumber__] > @__p_0
) AS [t]");
        }

        public override async Task OrderBy_Dto_projection_skip_take(bool isAsync)
        {
            await base.OrderBy_Dto_projection_skip_take(isAsync);

            AssertSql(
                @"@__p_0='5'
@__p_1='10'

SELECT [t].[Id]
FROM (
    SELECT [c].[CustomerID] AS [Id], ROW_NUMBER() OVER(ORDER BY [c].[CustomerID]) AS [__RowNumber__]
    FROM [Customers] AS [c]
) AS [t]
WHERE ([t].[__RowNumber__] > @__p_0) AND ([t].[__RowNumber__] <= (@__p_0 + @__p_1))");
        }

        public override async Task Projection_in_a_subquery_should_be_liftable(bool isAsync)
        {
            await base.Projection_in_a_subquery_should_be_liftable(isAsync);

            AssertSql(
                @"@__p_0='1'

SELECT [t].[EmployeeID]
FROM (
    SELECT [e].[EmployeeID], ROW_NUMBER() OVER(ORDER BY [e].[EmployeeID]) AS [__RowNumber__]
    FROM [Employees] AS [e]
) AS [t]
WHERE [t].[__RowNumber__] > @__p_0");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
