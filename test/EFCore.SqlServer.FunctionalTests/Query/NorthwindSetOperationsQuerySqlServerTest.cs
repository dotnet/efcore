// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindSetOperationsQuerySqlServerTest : NorthwindSetOperationsQueryRelationalTestBase<
        NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        public NorthwindSetOperationsQuerySqlServerTest(
            NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override bool CanExecuteQueryString
            => true;

        public override async Task Union(bool async)
        {
            await base.Union(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'Berlin'
UNION
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c0]
WHERE [c0].[City] = N'London'");
        }

        public override async Task Concat(bool async)
        {
            await base.Concat(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'Berlin'
UNION ALL
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c0]
WHERE [c0].[City] = N'London'");
        }

        public override async Task Intersect(bool async)
        {
            await base.Intersect(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
INTERSECT
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c0]
WHERE [c0].[ContactName] LIKE N'%Thomas%'");
        }

        public override async Task Except(bool async)
        {
            await base.Except(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
EXCEPT
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c0]
WHERE [c0].[ContactName] LIKE N'%Thomas%'");
        }

        public override async Task Union_OrderBy_Skip_Take(bool async)
        {
            await base.Union_OrderBy_Skip_Take(async);

            AssertSql(
                @"@__p_0='1'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'Berlin'
    UNION
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE [c0].[City] = N'London'
) AS [t]
ORDER BY [t].[ContactName]
OFFSET @__p_0 ROWS FETCH NEXT @__p_0 ROWS ONLY");
        }

        public override async Task Union_Where(bool async)
        {
            await base.Union_Where(async);

            AssertSql(
                @"SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'Berlin'
    UNION
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE [c0].[City] = N'London'
) AS [t]
WHERE [t].[ContactName] LIKE N'%Thomas%'");
        }

        public override async Task Union_Skip_Take_OrderBy_ThenBy_Where(bool async)
        {
            await base.Union_Skip_Take_OrderBy_ThenBy_Where(async);

            AssertSql(
                @"@__p_0='0'

SELECT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
    FROM (
        SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        WHERE [c].[City] = N'Berlin'
        UNION
        SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
        FROM [Customers] AS [c0]
        WHERE [c0].[City] = N'London'
    ) AS [t]
    ORDER BY [t].[Region], [t].[City]
    OFFSET @__p_0 ROWS
) AS [t0]
WHERE [t0].[ContactName] LIKE N'%Thomas%'
ORDER BY [t0].[Region], [t0].[City]");
        }

        public override async Task Union_Union(bool async)
        {
            await base.Union_Union(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'Berlin'
UNION
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c0]
WHERE [c0].[City] = N'London'
UNION
SELECT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
FROM [Customers] AS [c1]
WHERE [c1].[City] = N'Mannheim'");
        }

        public override async Task Union_Intersect(bool async)
        {
            await base.Union_Intersect(async);

            AssertSql(
                @"(
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'Berlin'
    UNION
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE [c0].[City] = N'London'
)
INTERSECT
SELECT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
FROM [Customers] AS [c1]
WHERE [c1].[ContactName] LIKE N'%Thomas%'");
        }

        [ConditionalTheory]
        public override async Task Union_Take_Union_Take(bool async)
        {
            await base.Union_Take_Union_Take(async);

            AssertSql(
                @"@__p_0='1'

SELECT [t2].[CustomerID], [t2].[Address], [t2].[City], [t2].[CompanyName], [t2].[ContactName], [t2].[ContactTitle], [t2].[Country], [t2].[Fax], [t2].[Phone], [t2].[PostalCode], [t2].[Region]
FROM (
    SELECT TOP(@__p_0) [t1].[CustomerID], [t1].[Address], [t1].[City], [t1].[CompanyName], [t1].[ContactName], [t1].[ContactTitle], [t1].[Country], [t1].[Fax], [t1].[Phone], [t1].[PostalCode], [t1].[Region]
    FROM (
        SELECT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
        FROM (
            SELECT TOP(@__p_0) [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
            FROM (
                SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
                FROM [Customers] AS [c]
                WHERE [c].[City] = N'Berlin'
                UNION
                SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
                FROM [Customers] AS [c0]
                WHERE [c0].[City] = N'London'
            ) AS [t]
            ORDER BY [t].[CustomerID]
        ) AS [t0]
        UNION
        SELECT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
        FROM [Customers] AS [c1]
        WHERE [c1].[City] = N'Mannheim'
    ) AS [t1]
) AS [t2]
ORDER BY [t2].[CustomerID]");
        }

        public override async Task Select_Union(bool async)
        {
            await base.Select_Union(async);

            AssertSql(
                @"SELECT [c].[Address]
FROM [Customers] AS [c]
WHERE [c].[City] = N'Berlin'
UNION
SELECT [c0].[Address]
FROM [Customers] AS [c0]
WHERE [c0].[City] = N'London'");
        }

        public override async Task Union_Select(bool async)
        {
            await base.Union_Select(async);

            AssertSql(
                @"SELECT [t].[Address]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'Berlin'
    UNION
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE [c0].[City] = N'London'
) AS [t]
WHERE [t].[Address] LIKE N'%Hanover%'");
        }

        public override async Task Union_Select_scalar(bool async)
        {
            await base.Union_Select_scalar(async);

            AssertSql(
                @"SELECT 1
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    EXCEPT
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
) AS [t]");
        }

        public override async Task Union_with_anonymous_type_projection(bool async)
        {
            await base.Union_with_anonymous_type_projection(async);

            AssertSql(
                @"SELECT [t].[CustomerID] AS [Id]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CompanyName] IS NOT NULL AND ([c].[CompanyName] LIKE N'A%')
    UNION
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE [c0].[CompanyName] IS NOT NULL AND ([c0].[CompanyName] LIKE N'B%')
) AS [t]");
        }

        public override async Task Select_Union_unrelated(bool async)
        {
            await base.Select_Union_unrelated(async);

            AssertSql(
                @"SELECT [t].[ContactName]
FROM (
    SELECT [c].[ContactName]
    FROM [Customers] AS [c]
    UNION
    SELECT [p].[ProductName] AS [ContactName]
    FROM [Products] AS [p]
) AS [t]
WHERE [t].[ContactName] IS NOT NULL AND ([t].[ContactName] LIKE N'C%')
ORDER BY [t].[ContactName]");
        }

        public override async Task Select_Union_different_fields_in_anonymous_with_subquery(bool async)
        {
            await base.Select_Union_different_fields_in_anonymous_with_subquery(async);

            AssertSql(
                @"@__p_0='1'
@__p_1='10'

SELECT [t0].[Foo], [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT [t].[Foo], [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
    FROM (
        SELECT [c].[City] AS [Foo], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        WHERE [c].[City] = N'Berlin'
        UNION
        SELECT [c0].[Region] AS [Foo], [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
        FROM [Customers] AS [c0]
        WHERE [c0].[City] = N'London'
    ) AS [t]
    ORDER BY [t].[Foo]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t0]
WHERE [t0].[Foo] = N'Berlin'
ORDER BY [t0].[Foo]");
        }

        public override async Task Union_Include(bool async)
        {
            await base.Union_Include(async);

            AssertSql(
                @"SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'Berlin'
    UNION
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE [c0].[City] = N'London'
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[CustomerID], [o].[OrderID]");
        }

        public override async Task Include_Union(bool async)
        {
            await base.Include_Union(async);

            AssertSql(
                @"SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'Berlin'
    UNION
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE [c0].[City] = N'London'
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[CustomerID], [o].[OrderID]");
        }

        public override async Task Select_Except_reference_projection(bool async)
        {
            await base.Select_Except_reference_projection(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
EXCEPT
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Orders] AS [o0]
LEFT JOIN [Customers] AS [c0] ON [o0].[CustomerID] = [c0].[CustomerID]
WHERE [o0].[CustomerID] = N'ALFKI'");
        }

        public override async Task SubSelect_Union(bool async)
        {
            await base.SubSelect_Union(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]) AS [Orders]
FROM [Customers] AS [c]
UNION
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region], (
    SELECT COUNT(*)
    FROM [Orders] AS [o0]
    WHERE [c0].[CustomerID] = [o0].[CustomerID]) AS [Orders]
FROM [Customers] AS [c0]");
        }

        public override async Task GroupBy_Select_Union(bool async)
        {
            await base.GroupBy_Select_Union(async);

            AssertSql(
                @"SELECT [c].[CustomerID], COUNT(*) AS [Count]
FROM [Customers] AS [c]
WHERE [c].[City] = N'Berlin'
GROUP BY [c].[CustomerID]
UNION
SELECT [c0].[CustomerID], COUNT(*) AS [Count]
FROM [Customers] AS [c0]
WHERE [c0].[City] = N'London'
GROUP BY [c0].[CustomerID]");
        }

        public override async Task Union_over_columns_with_different_nullability(bool async)
        {
            await base.Union_over_columns_with_different_nullability(async);

            AssertSql(
                @"SELECT N'NonNullableConstant' AS [c]
FROM [Customers] AS [c]
UNION ALL
SELECT NULL AS [c]
FROM [Customers] AS [c0]");
        }

        public override async Task Union_over_different_projection_types(bool async, string leftType, string rightType)
        {
            await base.Union_over_different_projection_types(async, leftType, rightType);

            var leftSql = GenerateSql(leftType);
            var rightSql = GenerateSql(rightType);

            switch (leftType)
            {
                case "Column":
                    leftSql = leftSql.Replace("{Alias}", "");
                    break;

                case "Binary":
                case "Constant":
                case "Function":
                case "ScalarSubquery":
                case "Unary":
                    leftSql = leftSql.Replace("{Alias}", " AS [c]");
                    break;

                default:
                    throw new ArgumentException("Unexpected type: " + leftType);
            }

            switch (rightType)
            {
                case "Column":
                    rightSql = rightSql.Replace("{Alias}", leftType == "Column" ? "" : " AS [c]");
                    break;

                case "Binary":
                case "Constant":
                case "Function":
                case "ScalarSubquery":
                case "Unary":
                    rightSql = rightSql.Replace("{Alias}", leftType == "Column" ? " AS [OrderID]" : " AS [c]");
                    break;
                default:
                    throw new ArgumentException("Unexpected type: " + rightType);
            }

            // Fix up right-side SQL as table aliases shift
            rightSql = leftType == "ScalarSubquery"
                ? rightSql.Replace("[o]", "[o1]").Replace("[o0]", "[o2]")
                : rightSql.Replace("[o0]", "[o1]").Replace("[o]", "[o0]");

            AssertSql(leftSql + Environment.NewLine + "UNION" + Environment.NewLine + rightSql);

            static string GenerateSql(string expressionType)
            {
                switch (expressionType)
                {
                    case "Column":
                        return @"SELECT [o].[OrderID]{Alias}
FROM [Orders] AS [o]";
                    case "Function":
                        return @"SELECT COUNT(*){Alias}
FROM [Orders] AS [o]
GROUP BY [o].[OrderID]";
                    case "Constant":
                        return @"SELECT 8{Alias}
FROM [Orders] AS [o]";
                    case "Unary":
                        return @"SELECT -[o].[OrderID]{Alias}
FROM [Orders] AS [o]";
                    case "Binary":
                        return @"SELECT [o].[OrderID] + 1{Alias}
FROM [Orders] AS [o]";
                    case "ScalarSubquery":
                        return @"SELECT (
    SELECT COUNT(*)
    FROM [Order Details] AS [o]
    WHERE [o0].[OrderID] = [o].[OrderID]){Alias}
FROM [Orders] AS [o0]";
                    default:
                        throw new ArgumentException("Unexpected type: " + expressionType);
                }
            }
        }

        public override async Task OrderBy_Take_Union(bool async)
        {
            await base.OrderBy_Take_Union(async);

            AssertSql(
                @"@__p_0='1'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactName]
) AS [t]
UNION
SELECT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT TOP(@__p_0) [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[ContactName]
) AS [t0]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
