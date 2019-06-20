// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class SimpleQuerySqlServerTest
    {
        public override async Task Union(bool isAsync)
        {
            await base.Union(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[City] = N'Berlin') AND [c].[City] IS NOT NULL
UNION
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c0]
WHERE ([c0].[City] = N'London') AND [c0].[City] IS NOT NULL");
        }

        public override async Task Concat(bool isAsync)
        {
            await base.Concat(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[City] = N'Berlin') AND [c].[City] IS NOT NULL
UNION ALL
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c0]
WHERE ([c0].[City] = N'London') AND [c0].[City] IS NOT NULL");
        }

        public override async Task Intersect(bool isAsync)
        {
            await base.Intersect(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[City] = N'London') AND [c].[City] IS NOT NULL
INTERSECT
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c0]
WHERE CHARINDEX(N'Thomas', [c0].[ContactName]) > 0");
        }

        public override async Task Except(bool isAsync)
        {
            await base.Except(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[City] = N'London') AND [c].[City] IS NOT NULL
EXCEPT
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c0]
WHERE CHARINDEX(N'Thomas', [c0].[ContactName]) > 0");        }

        public override async Task Union_OrderBy_Skip_Take(bool isAsync)
        {
            await base.Union_OrderBy_Skip_Take(isAsync);

            AssertSql(
                @"@__p_0='1'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[City] = N'Berlin') AND [c].[City] IS NOT NULL
UNION
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c0]
WHERE ([c0].[City] = N'London') AND [c0].[City] IS NOT NULL
ORDER BY [ContactName]
OFFSET @__p_0 ROWS FETCH NEXT @__p_0 ROWS ONLY");
        }

        public override async Task Union_Where(bool isAsync)
        {
            await base.Union_Where(isAsync);

            AssertSql(
                @"SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE ([c].[City] = N'Berlin') AND [c].[City] IS NOT NULL
    UNION
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE ([c0].[City] = N'London') AND [c0].[City] IS NOT NULL
) AS [t]
WHERE CHARINDEX(N'Thomas', [t].[ContactName]) > 0");
        }

        public override async Task Union_Skip_Take_OrderBy_ThenBy_Where(bool isAsync)
        {
            await base.Union_Skip_Take_OrderBy_ThenBy_Where(isAsync);

            AssertSql(
                @"@__p_0='0'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE ([c].[City] = N'Berlin') AND [c].[City] IS NOT NULL
    UNION
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE ([c0].[City] = N'London') AND [c0].[City] IS NOT NULL
    ORDER BY [Region], [City]
    OFFSET @__p_0 ROWS
) AS [t]
WHERE CHARINDEX(N'Thomas', [t].[ContactName]) > 0
ORDER BY [t].[Region], [t].[City]");
        }

        public override async Task Union_Union(bool isAsync)
        {
            await base.Union_Union(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[City] = N'Berlin') AND [c].[City] IS NOT NULL
UNION
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c0]
WHERE ([c0].[City] = N'London') AND [c0].[City] IS NOT NULL
UNION
SELECT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
FROM [Customers] AS [c1]
WHERE ([c1].[City] = N'Mannheim') AND [c1].[City] IS NOT NULL");
        }

        public override async Task Union_Intersect(bool isAsync)
        {
            await base.Union_Intersect(isAsync);

            AssertSql(
                @"(
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE ([c].[City] = N'Berlin') AND [c].[City] IS NOT NULL
    UNION
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE ([c0].[City] = N'London') AND [c0].[City] IS NOT NULL
)
INTERSECT
SELECT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
FROM [Customers] AS [c1]
WHERE CHARINDEX(N'Thomas', [c1].[ContactName]) > 0");
        }

        [ConditionalTheory(Skip = "Need to push down set operation on take without orderby+skip on SQL Server, waiting on design")]
        public override async Task Union_Take_Union_Take(bool isAsync)
        {
            await base.Union_Take_Union_Take(isAsync);

            throw new NotImplementedException("Take is being ignored");
            //AssertSql(@"");
        }

        public override async Task Select_Union(bool isAsync)
        {
            await base.Select_Union(isAsync);

            AssertSql(@"SELECT [c].[Address]
FROM [Customers] AS [c]
WHERE ([c].[City] = N'Berlin') AND [c].[City] IS NOT NULL
UNION
SELECT [c0].[Address]
FROM [Customers] AS [c0]
WHERE ([c0].[City] = N'London') AND [c0].[City] IS NOT NULL");
        }

        public override async Task Union_Select(bool isAsync)
        {
            await base.Union_Select(isAsync);

            AssertSql(@"SELECT [t].[Address]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE ([c].[City] = N'Berlin') AND [c].[City] IS NOT NULL
    UNION
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE ([c0].[City] = N'London') AND [c0].[City] IS NOT NULL
) AS [t]
WHERE CHARINDEX(N'Hanover', [t].[Address]) > 0");
        }

        public override async Task Select_Union_unrelated(bool isAsync)
        {
            await base.Select_Union_unrelated(isAsync);

            AssertSql(
                @"SELECT [t].[ContactName]
FROM (
    SELECT [c].[ContactName]
    FROM [Customers] AS [c]
    UNION
    SELECT [p].[ProductName]
    FROM [Products] AS [p]
) AS [t]
WHERE [t].[ContactName] IS NOT NULL AND ([t].[ContactName] LIKE N'C%')
ORDER BY [t].[ContactName]");
        }

        public override async Task Select_Union_different_fields_in_anonymous_with_subquery(bool isAsync)
        {
            await base.Select_Union_different_fields_in_anonymous_with_subquery(isAsync);

            AssertSql(
                @"@__p_0='1'
@__p_1='10'

SELECT [t].[Foo], [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT [c].[City] AS [Foo], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE ([c].[City] = N'Berlin') AND [c].[City] IS NOT NULL
    UNION
    SELECT [c0].[Region] AS [Foo], [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE ([c0].[City] = N'London') AND [c0].[City] IS NOT NULL
    ORDER BY [Foo]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
WHERE ([t].[Foo] = N'Berlin') AND [t].[Foo] IS NOT NULL
ORDER BY [t].[Foo]");
        }
    }
}
