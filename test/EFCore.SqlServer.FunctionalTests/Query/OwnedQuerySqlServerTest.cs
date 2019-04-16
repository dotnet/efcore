// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class OwnedQuerySqlServerTest : RelationalOwnedQueryTestBase<OwnedQuerySqlServerTest.OwnedQuerySqlServerFixture>
    {
        public OwnedQuerySqlServerTest(OwnedQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void Query_with_owned_entity_equality_operator()
        {
            base.Query_with_owned_entity_equality_operator();

            AssertSql(
                @"SELECT [a].[Id], [a].[Discriminator], [t].[Id], [t0].[Id], [t0].[BranchAddress_Country_Name], [t0].[BranchAddress_Country_PlanetId], [t1].[Id], [t2].[Id], [t2].[PersonAddress_Country_Name], [t2].[PersonAddress_Country_PlanetId], [t3].[Id], [t4].[Id], [t4].[LeafAAddress_Country_Name], [t4].[LeafAAddress_Country_PlanetId], [t5].[Id]
FROM [OwnedPerson] AS [a]
LEFT JOIN (
    SELECT [a.BranchAddress].*
    FROM [OwnedPerson] AS [a.BranchAddress]
    WHERE [a.BranchAddress].[Discriminator] IN (N'LeafA', N'Branch')
) AS [t] ON [a].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [a.BranchAddress.Country].*
    FROM [OwnedPerson] AS [a.BranchAddress.Country]
    WHERE [a.BranchAddress.Country].[Discriminator] IN (N'LeafA', N'Branch')
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [a.PersonAddress].*
    FROM [OwnedPerson] AS [a.PersonAddress]
    WHERE [a.PersonAddress].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t1] ON [a].[Id] = [t1].[Id]
LEFT JOIN (
    SELECT [a.PersonAddress.Country].*
    FROM [OwnedPerson] AS [a.PersonAddress.Country]
    WHERE [a.PersonAddress.Country].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t2] ON [t1].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [a.LeafAAddress].*
    FROM [OwnedPerson] AS [a.LeafAAddress]
    WHERE [a.LeafAAddress].[Discriminator] = N'LeafA'
) AS [t3] ON [a].[Id] = [t3].[Id]
LEFT JOIN (
    SELECT [a.LeafAAddress.Country].*
    FROM [OwnedPerson] AS [a.LeafAAddress.Country]
    WHERE [a.LeafAAddress.Country].[Discriminator] = N'LeafA'
) AS [t4] ON [t3].[Id] = [t4].[Id]
CROSS JOIN [OwnedPerson] AS [b]
LEFT JOIN (
    SELECT [b.LeafBAddress].*
    FROM [OwnedPerson] AS [b.LeafBAddress]
    WHERE [b.LeafBAddress].[Discriminator] = N'LeafB'
) AS [t5] ON [b].[Id] = [t5].[Id]
WHERE [a].[Discriminator] = N'LeafA'
ORDER BY [a].[Id]");
        }

        public override void Query_for_base_type_loads_all_owned_navs()
        {
            base.Query_for_base_type_loads_all_owned_navs();

            // See issue #10067
            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [t].[Id], [t0].[Id], [t0].[LeafBAddress_Country_Name], [t0].[LeafBAddress_Country_PlanetId], [t1].[Id], [t2].[Id], [t2].[LeafAAddress_Country_Name], [t2].[LeafAAddress_Country_PlanetId], [t3].[Id], [t4].[Id], [t4].[BranchAddress_Country_Name], [t4].[BranchAddress_Country_PlanetId], [t5].[Id], [t6].[Id], [t6].[PersonAddress_Country_Name], [t6].[PersonAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [l.LeafBAddress].*
    FROM [OwnedPerson] AS [l.LeafBAddress]
    WHERE [l.LeafBAddress].[Discriminator] = N'LeafB'
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [l.LeafBAddress.Country].*
    FROM [OwnedPerson] AS [l.LeafBAddress.Country]
    WHERE [l.LeafBAddress.Country].[Discriminator] = N'LeafB'
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [l.LeafAAddress].*
    FROM [OwnedPerson] AS [l.LeafAAddress]
    WHERE [l.LeafAAddress].[Discriminator] = N'LeafA'
) AS [t1] ON [o].[Id] = [t1].[Id]
LEFT JOIN (
    SELECT [l.LeafAAddress.Country].*
    FROM [OwnedPerson] AS [l.LeafAAddress.Country]
    WHERE [l.LeafAAddress.Country].[Discriminator] = N'LeafA'
) AS [t2] ON [t1].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [b.BranchAddress].*
    FROM [OwnedPerson] AS [b.BranchAddress]
    WHERE [b.BranchAddress].[Discriminator] IN (N'LeafA', N'Branch')
) AS [t3] ON [o].[Id] = [t3].[Id]
LEFT JOIN (
    SELECT [b.BranchAddress.Country].*
    FROM [OwnedPerson] AS [b.BranchAddress.Country]
    WHERE [b.BranchAddress.Country].[Discriminator] IN (N'LeafA', N'Branch')
) AS [t4] ON [t3].[Id] = [t4].[Id]
LEFT JOIN (
    SELECT [o.PersonAddress].*
    FROM [OwnedPerson] AS [o.PersonAddress]
    WHERE [o.PersonAddress].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t5] ON [o].[Id] = [t5].[Id]
LEFT JOIN (
    SELECT [o.PersonAddress.Country].*
    FROM [OwnedPerson] AS [o.PersonAddress.Country]
    WHERE [o.PersonAddress.Country].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t6] ON [t5].[Id] = [t6].[Id]
WHERE [o].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
ORDER BY [o].[Id]",
                //
                @"SELECT [o.Orders].[Id], [o.Orders].[ClientId]
FROM [Order] AS [o.Orders]
INNER JOIN (
    SELECT DISTINCT [o0].[Id]
    FROM [OwnedPerson] AS [o0]
    LEFT JOIN (
        SELECT [l.LeafBAddress0].*
        FROM [OwnedPerson] AS [l.LeafBAddress0]
        WHERE [l.LeafBAddress0].[Discriminator] = N'LeafB'
    ) AS [t7] ON [o0].[Id] = [t7].[Id]
    LEFT JOIN (
        SELECT [l.LeafBAddress.Country0].*
        FROM [OwnedPerson] AS [l.LeafBAddress.Country0]
        WHERE [l.LeafBAddress.Country0].[Discriminator] = N'LeafB'
    ) AS [t8] ON [t7].[Id] = [t8].[Id]
    LEFT JOIN (
        SELECT [l.LeafAAddress0].*
        FROM [OwnedPerson] AS [l.LeafAAddress0]
        WHERE [l.LeafAAddress0].[Discriminator] = N'LeafA'
    ) AS [t9] ON [o0].[Id] = [t9].[Id]
    LEFT JOIN (
        SELECT [l.LeafAAddress.Country0].*
        FROM [OwnedPerson] AS [l.LeafAAddress.Country0]
        WHERE [l.LeafAAddress.Country0].[Discriminator] = N'LeafA'
    ) AS [t10] ON [t9].[Id] = [t10].[Id]
    LEFT JOIN (
        SELECT [b.BranchAddress0].*
        FROM [OwnedPerson] AS [b.BranchAddress0]
        WHERE [b.BranchAddress0].[Discriminator] IN (N'LeafA', N'Branch')
    ) AS [t11] ON [o0].[Id] = [t11].[Id]
    LEFT JOIN (
        SELECT [b.BranchAddress.Country0].*
        FROM [OwnedPerson] AS [b.BranchAddress.Country0]
        WHERE [b.BranchAddress.Country0].[Discriminator] IN (N'LeafA', N'Branch')
    ) AS [t12] ON [t11].[Id] = [t12].[Id]
    LEFT JOIN (
        SELECT [o.PersonAddress0].*
        FROM [OwnedPerson] AS [o.PersonAddress0]
        WHERE [o.PersonAddress0].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
    ) AS [t13] ON [o0].[Id] = [t13].[Id]
    LEFT JOIN (
        SELECT [o.PersonAddress.Country0].*
        FROM [OwnedPerson] AS [o.PersonAddress.Country0]
        WHERE [o.PersonAddress.Country0].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
    ) AS [t14] ON [t13].[Id] = [t14].[Id]
    WHERE [o0].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t15] ON [o.Orders].[ClientId] = [t15].[Id]
ORDER BY [t15].[Id]");
        }

        public override void No_ignored_include_warning_when_implicit_load()
        {
            base.No_ignored_include_warning_when_implicit_load();

            AssertSql(
                @"SELECT COUNT(*)
FROM [OwnedPerson] AS [o]
WHERE [o].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')");
        }

        public override void Query_for_branch_type_loads_all_owned_navs()
        {
            base.Query_for_branch_type_loads_all_owned_navs();

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [t].[Id], [t0].[Id], [t0].[LeafAAddress_Country_Name], [t0].[LeafAAddress_Country_PlanetId], [t1].[Id], [t2].[Id], [t2].[BranchAddress_Country_Name], [t2].[BranchAddress_Country_PlanetId], [t3].[Id], [t4].[Id], [t4].[PersonAddress_Country_Name], [t4].[PersonAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [l.LeafAAddress].*
    FROM [OwnedPerson] AS [l.LeafAAddress]
    WHERE [l.LeafAAddress].[Discriminator] = N'LeafA'
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [l.LeafAAddress.Country].*
    FROM [OwnedPerson] AS [l.LeafAAddress.Country]
    WHERE [l.LeafAAddress.Country].[Discriminator] = N'LeafA'
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [b.BranchAddress].*
    FROM [OwnedPerson] AS [b.BranchAddress]
    WHERE [b.BranchAddress].[Discriminator] IN (N'LeafA', N'Branch')
) AS [t1] ON [o].[Id] = [t1].[Id]
LEFT JOIN (
    SELECT [b.BranchAddress.Country].*
    FROM [OwnedPerson] AS [b.BranchAddress.Country]
    WHERE [b.BranchAddress.Country].[Discriminator] IN (N'LeafA', N'Branch')
) AS [t2] ON [t1].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [o.PersonAddress].*
    FROM [OwnedPerson] AS [o.PersonAddress]
    WHERE [o.PersonAddress].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t3] ON [o].[Id] = [t3].[Id]
LEFT JOIN (
    SELECT [o.PersonAddress.Country].*
    FROM [OwnedPerson] AS [o.PersonAddress.Country]
    WHERE [o.PersonAddress.Country].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t4] ON [t3].[Id] = [t4].[Id]
WHERE [o].[Discriminator] IN (N'LeafA', N'Branch')
ORDER BY [o].[Id]",
                //
                @"SELECT [o.Orders].[Id], [o.Orders].[ClientId]
FROM [Order] AS [o.Orders]
INNER JOIN (
    SELECT DISTINCT [o0].[Id]
    FROM [OwnedPerson] AS [o0]
    LEFT JOIN (
        SELECT [l.LeafAAddress0].*
        FROM [OwnedPerson] AS [l.LeafAAddress0]
        WHERE [l.LeafAAddress0].[Discriminator] = N'LeafA'
    ) AS [t5] ON [o0].[Id] = [t5].[Id]
    LEFT JOIN (
        SELECT [l.LeafAAddress.Country0].*
        FROM [OwnedPerson] AS [l.LeafAAddress.Country0]
        WHERE [l.LeafAAddress.Country0].[Discriminator] = N'LeafA'
    ) AS [t6] ON [t5].[Id] = [t6].[Id]
    LEFT JOIN (
        SELECT [b.BranchAddress0].*
        FROM [OwnedPerson] AS [b.BranchAddress0]
        WHERE [b.BranchAddress0].[Discriminator] IN (N'LeafA', N'Branch')
    ) AS [t7] ON [o0].[Id] = [t7].[Id]
    LEFT JOIN (
        SELECT [b.BranchAddress.Country0].*
        FROM [OwnedPerson] AS [b.BranchAddress.Country0]
        WHERE [b.BranchAddress.Country0].[Discriminator] IN (N'LeafA', N'Branch')
    ) AS [t8] ON [t7].[Id] = [t8].[Id]
    LEFT JOIN (
        SELECT [o.PersonAddress0].*
        FROM [OwnedPerson] AS [o.PersonAddress0]
        WHERE [o.PersonAddress0].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
    ) AS [t9] ON [o0].[Id] = [t9].[Id]
    LEFT JOIN (
        SELECT [o.PersonAddress.Country0].*
        FROM [OwnedPerson] AS [o.PersonAddress.Country0]
        WHERE [o.PersonAddress.Country0].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
    ) AS [t10] ON [t9].[Id] = [t10].[Id]
    WHERE [o0].[Discriminator] IN (N'LeafA', N'Branch')
) AS [t11] ON [o.Orders].[ClientId] = [t11].[Id]
ORDER BY [t11].[Id]");
        }

        public override void Query_for_leaf_type_loads_all_owned_navs()
        {
            base.Query_for_leaf_type_loads_all_owned_navs();

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [t].[Id], [t0].[Id], [t0].[LeafAAddress_Country_Name], [t0].[LeafAAddress_Country_PlanetId], [t1].[Id], [t2].[Id], [t2].[BranchAddress_Country_Name], [t2].[BranchAddress_Country_PlanetId], [t3].[Id], [t4].[Id], [t4].[PersonAddress_Country_Name], [t4].[PersonAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [l.LeafAAddress].*
    FROM [OwnedPerson] AS [l.LeafAAddress]
    WHERE [l.LeafAAddress].[Discriminator] = N'LeafA'
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [l.LeafAAddress.Country].*
    FROM [OwnedPerson] AS [l.LeafAAddress.Country]
    WHERE [l.LeafAAddress.Country].[Discriminator] = N'LeafA'
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [b.BranchAddress].*
    FROM [OwnedPerson] AS [b.BranchAddress]
    WHERE [b.BranchAddress].[Discriminator] IN (N'LeafA', N'Branch')
) AS [t1] ON [o].[Id] = [t1].[Id]
LEFT JOIN (
    SELECT [b.BranchAddress.Country].*
    FROM [OwnedPerson] AS [b.BranchAddress.Country]
    WHERE [b.BranchAddress.Country].[Discriminator] IN (N'LeafA', N'Branch')
) AS [t2] ON [t1].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [o.PersonAddress].*
    FROM [OwnedPerson] AS [o.PersonAddress]
    WHERE [o.PersonAddress].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t3] ON [o].[Id] = [t3].[Id]
LEFT JOIN (
    SELECT [o.PersonAddress.Country].*
    FROM [OwnedPerson] AS [o.PersonAddress.Country]
    WHERE [o.PersonAddress.Country].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t4] ON [t3].[Id] = [t4].[Id]
WHERE [o].[Discriminator] = N'LeafA'
ORDER BY [o].[Id]",
                //
                @"SELECT [o.Orders].[Id], [o.Orders].[ClientId]
FROM [Order] AS [o.Orders]
INNER JOIN (
    SELECT DISTINCT [o0].[Id]
    FROM [OwnedPerson] AS [o0]
    LEFT JOIN (
        SELECT [l.LeafAAddress0].*
        FROM [OwnedPerson] AS [l.LeafAAddress0]
        WHERE [l.LeafAAddress0].[Discriminator] = N'LeafA'
    ) AS [t5] ON [o0].[Id] = [t5].[Id]
    LEFT JOIN (
        SELECT [l.LeafAAddress.Country0].*
        FROM [OwnedPerson] AS [l.LeafAAddress.Country0]
        WHERE [l.LeafAAddress.Country0].[Discriminator] = N'LeafA'
    ) AS [t6] ON [t5].[Id] = [t6].[Id]
    LEFT JOIN (
        SELECT [b.BranchAddress0].*
        FROM [OwnedPerson] AS [b.BranchAddress0]
        WHERE [b.BranchAddress0].[Discriminator] IN (N'LeafA', N'Branch')
    ) AS [t7] ON [o0].[Id] = [t7].[Id]
    LEFT JOIN (
        SELECT [b.BranchAddress.Country0].*
        FROM [OwnedPerson] AS [b.BranchAddress.Country0]
        WHERE [b.BranchAddress.Country0].[Discriminator] IN (N'LeafA', N'Branch')
    ) AS [t8] ON [t7].[Id] = [t8].[Id]
    LEFT JOIN (
        SELECT [o.PersonAddress0].*
        FROM [OwnedPerson] AS [o.PersonAddress0]
        WHERE [o.PersonAddress0].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
    ) AS [t9] ON [o0].[Id] = [t9].[Id]
    LEFT JOIN (
        SELECT [o.PersonAddress.Country0].*
        FROM [OwnedPerson] AS [o.PersonAddress.Country0]
        WHERE [o.PersonAddress.Country0].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
    ) AS [t10] ON [t9].[Id] = [t10].[Id]
    WHERE [o0].[Discriminator] = N'LeafA'
) AS [t11] ON [o.Orders].[ClientId] = [t11].[Id]
ORDER BY [t11].[Id]");
        }

        public override void Query_when_group_by()
        {
            base.Query_when_group_by();

            AssertSql(
                @"SELECT [op].[Id], [op].[Discriminator], [t].[Id], [t0].[Id], [t0].[LeafBAddress_Country_Name], [t0].[LeafBAddress_Country_PlanetId], [t1].[Id], [t2].[Id], [t2].[LeafAAddress_Country_Name], [t2].[LeafAAddress_Country_PlanetId], [t3].[Id], [t4].[Id], [t4].[BranchAddress_Country_Name], [t4].[BranchAddress_Country_PlanetId], [t5].[Id], [t6].[Id], [t6].[PersonAddress_Country_Name], [t6].[PersonAddress_Country_PlanetId]
FROM [OwnedPerson] AS [op]
LEFT JOIN (
    SELECT [op.LeafBAddress].*
    FROM [OwnedPerson] AS [op.LeafBAddress]
    WHERE [op.LeafBAddress].[Discriminator] = N'LeafB'
) AS [t] ON [op].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [op.LeafBAddress.Country].*
    FROM [OwnedPerson] AS [op.LeafBAddress.Country]
    WHERE [op.LeafBAddress.Country].[Discriminator] = N'LeafB'
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [op.LeafAAddress].*
    FROM [OwnedPerson] AS [op.LeafAAddress]
    WHERE [op.LeafAAddress].[Discriminator] = N'LeafA'
) AS [t1] ON [op].[Id] = [t1].[Id]
LEFT JOIN (
    SELECT [op.LeafAAddress.Country].*
    FROM [OwnedPerson] AS [op.LeafAAddress.Country]
    WHERE [op.LeafAAddress.Country].[Discriminator] = N'LeafA'
) AS [t2] ON [t1].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [op.BranchAddress].*
    FROM [OwnedPerson] AS [op.BranchAddress]
    WHERE [op.BranchAddress].[Discriminator] IN (N'LeafA', N'Branch')
) AS [t3] ON [op].[Id] = [t3].[Id]
LEFT JOIN (
    SELECT [op.BranchAddress.Country].*
    FROM [OwnedPerson] AS [op.BranchAddress.Country]
    WHERE [op.BranchAddress.Country].[Discriminator] IN (N'LeafA', N'Branch')
) AS [t4] ON [t3].[Id] = [t4].[Id]
LEFT JOIN (
    SELECT [op.PersonAddress].*
    FROM [OwnedPerson] AS [op.PersonAddress]
    WHERE [op.PersonAddress].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t5] ON [op].[Id] = [t5].[Id]
LEFT JOIN (
    SELECT [op.PersonAddress.Country].*
    FROM [OwnedPerson] AS [op.PersonAddress.Country]
    WHERE [op.PersonAddress.Country].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t6] ON [t5].[Id] = [t6].[Id]
WHERE [op].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
ORDER BY [op].[Id]",
                //
                @"SELECT [op.Orders].[Id], [op.Orders].[ClientId]
FROM [Order] AS [op.Orders]
INNER JOIN (
    SELECT DISTINCT [op0].[Id]
    FROM [OwnedPerson] AS [op0]
    LEFT JOIN (
        SELECT [op.LeafBAddress0].*
        FROM [OwnedPerson] AS [op.LeafBAddress0]
        WHERE [op.LeafBAddress0].[Discriminator] = N'LeafB'
    ) AS [t7] ON [op0].[Id] = [t7].[Id]
    LEFT JOIN (
        SELECT [op.LeafBAddress.Country0].*
        FROM [OwnedPerson] AS [op.LeafBAddress.Country0]
        WHERE [op.LeafBAddress.Country0].[Discriminator] = N'LeafB'
    ) AS [t8] ON [t7].[Id] = [t8].[Id]
    LEFT JOIN (
        SELECT [op.LeafAAddress0].*
        FROM [OwnedPerson] AS [op.LeafAAddress0]
        WHERE [op.LeafAAddress0].[Discriminator] = N'LeafA'
    ) AS [t9] ON [op0].[Id] = [t9].[Id]
    LEFT JOIN (
        SELECT [op.LeafAAddress.Country0].*
        FROM [OwnedPerson] AS [op.LeafAAddress.Country0]
        WHERE [op.LeafAAddress.Country0].[Discriminator] = N'LeafA'
    ) AS [t10] ON [t9].[Id] = [t10].[Id]
    LEFT JOIN (
        SELECT [op.BranchAddress0].*
        FROM [OwnedPerson] AS [op.BranchAddress0]
        WHERE [op.BranchAddress0].[Discriminator] IN (N'LeafA', N'Branch')
    ) AS [t11] ON [op0].[Id] = [t11].[Id]
    LEFT JOIN (
        SELECT [op.BranchAddress.Country0].*
        FROM [OwnedPerson] AS [op.BranchAddress.Country0]
        WHERE [op.BranchAddress.Country0].[Discriminator] IN (N'LeafA', N'Branch')
    ) AS [t12] ON [t11].[Id] = [t12].[Id]
    LEFT JOIN (
        SELECT [op.PersonAddress0].*
        FROM [OwnedPerson] AS [op.PersonAddress0]
        WHERE [op.PersonAddress0].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
    ) AS [t13] ON [op0].[Id] = [t13].[Id]
    LEFT JOIN (
        SELECT [op.PersonAddress.Country0].*
        FROM [OwnedPerson] AS [op.PersonAddress.Country0]
        WHERE [op.PersonAddress.Country0].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
    ) AS [t14] ON [t13].[Id] = [t14].[Id]
    WHERE [op0].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t15] ON [op.Orders].[ClientId] = [t15].[Id]
ORDER BY [t15].[Id]");
        }

        public override void Query_when_subquery()
        {
            base.Query_when_subquery();

            AssertSql(
                @"@__p_0='5'

SELECT TOP(@__p_0) [t].[Id], [t].[Discriminator], [t0].[Id], [t1].[Id], [t1].[LeafBAddress_Country_Name], [t1].[LeafBAddress_Country_PlanetId], [t2].[Id], [t3].[Id], [t3].[LeafAAddress_Country_Name], [t3].[LeafAAddress_Country_PlanetId], [t4].[Id], [t5].[Id], [t5].[BranchAddress_Country_Name], [t5].[BranchAddress_Country_PlanetId], [t6].[Id], [t7].[Id], [t7].[PersonAddress_Country_Name], [t7].[PersonAddress_Country_PlanetId]
FROM (
    SELECT DISTINCT [o].[Id], [o].[Discriminator]
    FROM [OwnedPerson] AS [o]
    WHERE [o].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t]
LEFT JOIN (
    SELECT [p.LeafBAddress].*
    FROM [OwnedPerson] AS [p.LeafBAddress]
    WHERE [p.LeafBAddress].[Discriminator] = N'LeafB'
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [p.LeafBAddress.Country].*
    FROM [OwnedPerson] AS [p.LeafBAddress.Country]
    WHERE [p.LeafBAddress.Country].[Discriminator] = N'LeafB'
) AS [t1] ON [t0].[Id] = [t1].[Id]
LEFT JOIN (
    SELECT [p.LeafAAddress].*
    FROM [OwnedPerson] AS [p.LeafAAddress]
    WHERE [p.LeafAAddress].[Discriminator] = N'LeafA'
) AS [t2] ON [t].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [p.LeafAAddress.Country].*
    FROM [OwnedPerson] AS [p.LeafAAddress.Country]
    WHERE [p.LeafAAddress.Country].[Discriminator] = N'LeafA'
) AS [t3] ON [t2].[Id] = [t3].[Id]
LEFT JOIN (
    SELECT [p.BranchAddress].*
    FROM [OwnedPerson] AS [p.BranchAddress]
    WHERE [p.BranchAddress].[Discriminator] IN (N'LeafA', N'Branch')
) AS [t4] ON [t].[Id] = [t4].[Id]
LEFT JOIN (
    SELECT [p.BranchAddress.Country].*
    FROM [OwnedPerson] AS [p.BranchAddress.Country]
    WHERE [p.BranchAddress.Country].[Discriminator] IN (N'LeafA', N'Branch')
) AS [t5] ON [t4].[Id] = [t5].[Id]
LEFT JOIN (
    SELECT [p.PersonAddress].*
    FROM [OwnedPerson] AS [p.PersonAddress]
    WHERE [p.PersonAddress].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t6] ON [t].[Id] = [t6].[Id]
LEFT JOIN (
    SELECT [p.PersonAddress.Country].*
    FROM [OwnedPerson] AS [p.PersonAddress.Country]
    WHERE [p.PersonAddress.Country].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t7] ON [t6].[Id] = [t7].[Id]
ORDER BY [t].[Id]",
                //
                @"@__p_0='5'

SELECT [p.Orders].[Id], [p.Orders].[ClientId]
FROM [Order] AS [p.Orders]
INNER JOIN (
    SELECT DISTINCT [t17].*
    FROM (
        SELECT TOP(@__p_0) [t8].[Id]
        FROM (
            SELECT DISTINCT [o0].*
            FROM [OwnedPerson] AS [o0]
            WHERE [o0].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
        ) AS [t8]
        LEFT JOIN (
            SELECT [p.LeafBAddress0].*
            FROM [OwnedPerson] AS [p.LeafBAddress0]
            WHERE [p.LeafBAddress0].[Discriminator] = N'LeafB'
        ) AS [t9] ON [t8].[Id] = [t9].[Id]
        LEFT JOIN (
            SELECT [p.LeafBAddress.Country0].*
            FROM [OwnedPerson] AS [p.LeafBAddress.Country0]
            WHERE [p.LeafBAddress.Country0].[Discriminator] = N'LeafB'
        ) AS [t10] ON [t9].[Id] = [t10].[Id]
        LEFT JOIN (
            SELECT [p.LeafAAddress0].*
            FROM [OwnedPerson] AS [p.LeafAAddress0]
            WHERE [p.LeafAAddress0].[Discriminator] = N'LeafA'
        ) AS [t11] ON [t8].[Id] = [t11].[Id]
        LEFT JOIN (
            SELECT [p.LeafAAddress.Country0].*
            FROM [OwnedPerson] AS [p.LeafAAddress.Country0]
            WHERE [p.LeafAAddress.Country0].[Discriminator] = N'LeafA'
        ) AS [t12] ON [t11].[Id] = [t12].[Id]
        LEFT JOIN (
            SELECT [p.BranchAddress0].*
            FROM [OwnedPerson] AS [p.BranchAddress0]
            WHERE [p.BranchAddress0].[Discriminator] IN (N'LeafA', N'Branch')
        ) AS [t13] ON [t8].[Id] = [t13].[Id]
        LEFT JOIN (
            SELECT [p.BranchAddress.Country0].*
            FROM [OwnedPerson] AS [p.BranchAddress.Country0]
            WHERE [p.BranchAddress.Country0].[Discriminator] IN (N'LeafA', N'Branch')
        ) AS [t14] ON [t13].[Id] = [t14].[Id]
        LEFT JOIN (
            SELECT [p.PersonAddress0].*
            FROM [OwnedPerson] AS [p.PersonAddress0]
            WHERE [p.PersonAddress0].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
        ) AS [t15] ON [t8].[Id] = [t15].[Id]
        LEFT JOIN (
            SELECT [p.PersonAddress.Country0].*
            FROM [OwnedPerson] AS [p.PersonAddress.Country0]
            WHERE [p.PersonAddress.Country0].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
        ) AS [t16] ON [t15].[Id] = [t16].[Id]
        ORDER BY [t8].[Id]
    ) AS [t17]
) AS [t18] ON [p.Orders].[ClientId] = [t18].[Id]
ORDER BY [t18].[Id]");
        }

        public override void Navigation_rewrite_on_owned_reference()
        {
            base.Navigation_rewrite_on_owned_reference();

            AssertSql(
                @"SELECT [t0].[PersonAddress_Country_Name]
FROM [OwnedPerson] AS [p]
LEFT JOIN (
    SELECT [p.PersonAddress].*
    FROM [OwnedPerson] AS [p.PersonAddress]
    WHERE [p.PersonAddress].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t] ON [p].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [p.PersonAddress.Country].*
    FROM [OwnedPerson] AS [p.PersonAddress.Country]
    WHERE [p.PersonAddress.Country].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t0] ON [t].[Id] = [t0].[Id]
WHERE [p].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson') AND ([t0].[PersonAddress_Country_Name] = N'USA')");
        }

        public override void Navigation_rewrite_on_owned_collection()
        {
            base.Navigation_rewrite_on_owned_collection();

            AssertSql(
                @"SELECT [p].[Id]
FROM [OwnedPerson] AS [p]
WHERE [p].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson') AND ((
    SELECT COUNT(*)
    FROM [Order] AS [o]
    WHERE [p].[Id] = [o].[ClientId]
) > 0)
ORDER BY [p].[Id]",
                //
                @"SELECT [p.Orders].[Id], [p.Orders].[ClientId], [t].[Id]
FROM [Order] AS [p.Orders]
INNER JOIN (
    SELECT [p0].[Id]
    FROM [OwnedPerson] AS [p0]
    WHERE [p0].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson') AND ((
        SELECT COUNT(*)
        FROM [Order] AS [o0]
        WHERE [p0].[Id] = [o0].[ClientId]
    ) > 0)
) AS [t] ON [p.Orders].[ClientId] = [t].[Id]
ORDER BY [t].[Id]");
        }

        public override void Select_many_on_owned_collection()
        {
            base.Select_many_on_owned_collection();

            AssertSql(
                @"SELECT [p.Orders].[Id], [p.Orders].[ClientId]
FROM [OwnedPerson] AS [p]
INNER JOIN [Order] AS [p.Orders] ON [p].[Id] = [p.Orders].[ClientId]
WHERE [p].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')");
        }

        public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity()
        {
            base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity();

            AssertSql(
                @"SELECT [p.PersonAddress.Country.Planet].[Id], [p.PersonAddress.Country.Planet].[StarId]
FROM [OwnedPerson] AS [p]
LEFT JOIN (
    SELECT [p.PersonAddress].*
    FROM [OwnedPerson] AS [p.PersonAddress]
    WHERE [p.PersonAddress].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t] ON [p].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [p.PersonAddress.Country].*
    FROM [OwnedPerson] AS [p.PersonAddress.Country]
    WHERE [p.PersonAddress.Country].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN [Planet] AS [p.PersonAddress.Country.Planet] ON [t0].[PersonAddress_Country_PlanetId] = [p.PersonAddress.Country.Planet].[Id]
WHERE [p].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')");
        }

        public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property()
        {
            base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property();

            AssertSql(
                @"SELECT [p.PersonAddress.Country.Planet].[Id]
FROM [OwnedPerson] AS [p]
LEFT JOIN (
    SELECT [p.PersonAddress].*
    FROM [OwnedPerson] AS [p.PersonAddress]
    WHERE [p.PersonAddress].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t] ON [p].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [p.PersonAddress.Country].*
    FROM [OwnedPerson] AS [p.PersonAddress.Country]
    WHERE [p.PersonAddress.Country].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN [Planet] AS [p.PersonAddress.Country.Planet] ON [t0].[PersonAddress_Country_PlanetId] = [p.PersonAddress.Country.Planet].[Id]
WHERE [p].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')");
        }

        public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection()
        {
            base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection();

            AssertSql(
                @"SELECT [p.PersonAddress.Country.Planet].[Id]
FROM [OwnedPerson] AS [p]
LEFT JOIN (
    SELECT [p.PersonAddress].*
    FROM [OwnedPerson] AS [p.PersonAddress]
    WHERE [p.PersonAddress].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t] ON [p].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [p.PersonAddress.Country].*
    FROM [OwnedPerson] AS [p.PersonAddress.Country]
    WHERE [p.PersonAddress.Country].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN [Planet] AS [p.PersonAddress.Country.Planet] ON [t0].[PersonAddress_Country_PlanetId] = [p.PersonAddress.Country.Planet].[Id]
WHERE [p].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
ORDER BY [p].[Id], [p.PersonAddress.Country.Planet].[Id]",
                //
                @"SELECT [p.PersonAddress.Country.Planet.Moons].[Id], [p.PersonAddress.Country.Planet.Moons].[Diameter], [p.PersonAddress.Country.Planet.Moons].[PlanetId], [t3].[Id], [t3].[Id0]
FROM [Moon] AS [p.PersonAddress.Country.Planet.Moons]
INNER JOIN (
    SELECT [p0].[Id], [p.PersonAddress.Country.Planet0].[Id] AS [Id0]
    FROM [OwnedPerson] AS [p0]
    LEFT JOIN (
        SELECT [p.PersonAddress0].*
        FROM [OwnedPerson] AS [p.PersonAddress0]
        WHERE [p.PersonAddress0].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
    ) AS [t1] ON [p0].[Id] = [t1].[Id]
    LEFT JOIN (
        SELECT [p.PersonAddress.Country0].*
        FROM [OwnedPerson] AS [p.PersonAddress.Country0]
        WHERE [p.PersonAddress.Country0].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
    ) AS [t2] ON [t1].[Id] = [t2].[Id]
    LEFT JOIN [Planet] AS [p.PersonAddress.Country.Planet0] ON [t2].[PersonAddress_Country_PlanetId] = [p.PersonAddress.Country.Planet0].[Id]
    WHERE [p0].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t3] ON [p.PersonAddress.Country.Planet.Moons].[PlanetId] = [t3].[Id0]
ORDER BY [t3].[Id], [t3].[Id0]");
        }

        public override void SelectMany_on_owned_reference_followed_by_regular_entity_and_collection()
        {
            base.SelectMany_on_owned_reference_followed_by_regular_entity_and_collection();

            AssertSql(
                @"SELECT [p.PersonAddress.Country.Planet.Moons].[Id], [p.PersonAddress.Country.Planet.Moons].[Diameter], [p.PersonAddress.Country.Planet.Moons].[PlanetId]
FROM [OwnedPerson] AS [p]
INNER JOIN [OwnedPerson] AS [p.PersonAddress] ON [p].[Id] = [p.PersonAddress].[Id]
INNER JOIN [OwnedPerson] AS [p.PersonAddress.Country] ON [p.PersonAddress].[Id] = [p.PersonAddress.Country].[Id]
INNER JOIN [Planet] AS [p.PersonAddress.Country.Planet] ON [p.PersonAddress.Country].[PersonAddress_Country_PlanetId] = [p.PersonAddress.Country.Planet].[Id]
INNER JOIN [Moon] AS [p.PersonAddress.Country.Planet.Moons] ON [p.PersonAddress.Country.Planet].[Id] = [p.PersonAddress.Country.Planet.Moons].[PlanetId]
WHERE ([p].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson') AND [p.PersonAddress].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')) AND [p.PersonAddress.Country].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')");
        }

        public override void SelectMany_on_owned_reference_with_entity_in_between_ending_in_owned_collection()
        {
            base.SelectMany_on_owned_reference_with_entity_in_between_ending_in_owned_collection();

            AssertSql(
                @"SELECT [p.PersonAddress.Country.Planet.Star.Composition].[Id], [p.PersonAddress.Country.Planet.Star.Composition].[Name], [p.PersonAddress.Country.Planet.Star.Composition].[StarId]
FROM [OwnedPerson] AS [p]
INNER JOIN [OwnedPerson] AS [p.PersonAddress] ON [p].[Id] = [p.PersonAddress].[Id]
INNER JOIN [OwnedPerson] AS [p.PersonAddress.Country] ON [p.PersonAddress].[Id] = [p.PersonAddress.Country].[Id]
INNER JOIN [Planet] AS [p.PersonAddress.Country.Planet] ON [p.PersonAddress.Country].[PersonAddress_Country_PlanetId] = [p.PersonAddress.Country.Planet].[Id]
INNER JOIN [Star] AS [p.PersonAddress.Country.Planet.Star] ON [p.PersonAddress.Country.Planet].[StarId] = [p.PersonAddress.Country.Planet.Star].[Id]
INNER JOIN [Element] AS [p.PersonAddress.Country.Planet.Star.Composition] ON [p.PersonAddress.Country.Planet.Star].[Id] = [p.PersonAddress.Country.Planet.Star.Composition].[StarId]
WHERE ([p].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson') AND [p.PersonAddress].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')) AND [p.PersonAddress.Country].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')");
        }

        public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_count()
        {
            base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_count();

            AssertSql(
                @"SELECT (
    SELECT COUNT(*)
    FROM [Moon] AS [m]
    WHERE [p.PersonAddress.Country.Planet].[Id] = [m].[PlanetId]
)
FROM [OwnedPerson] AS [p]
LEFT JOIN (
    SELECT [p.PersonAddress].*
    FROM [OwnedPerson] AS [p.PersonAddress]
    WHERE [p.PersonAddress].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t] ON [p].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [p.PersonAddress.Country].*
    FROM [OwnedPerson] AS [p.PersonAddress.Country]
    WHERE [p.PersonAddress.Country].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN [Planet] AS [p.PersonAddress.Country.Planet] ON [t0].[PersonAddress_Country_PlanetId] = [p.PersonAddress.Country.Planet].[Id]
WHERE [p].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')");
        }

        public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference()
        {
            base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference();

            AssertSql(
                @"SELECT [p.PersonAddress.Country.Planet.Star].[Id], [p.PersonAddress.Country.Planet.Star].[Name]
FROM [OwnedPerson] AS [p]
LEFT JOIN (
    SELECT [p.PersonAddress].*
    FROM [OwnedPerson] AS [p.PersonAddress]
    WHERE [p.PersonAddress].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t] ON [p].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [p.PersonAddress.Country].*
    FROM [OwnedPerson] AS [p.PersonAddress.Country]
    WHERE [p.PersonAddress.Country].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN [Planet] AS [p.PersonAddress.Country.Planet] ON [t0].[PersonAddress_Country_PlanetId] = [p.PersonAddress.Country.Planet].[Id]
LEFT JOIN [Star] AS [p.PersonAddress.Country.Planet.Star] ON [p.PersonAddress.Country.Planet].[StarId] = [p.PersonAddress.Country.Planet.Star].[Id]
WHERE [p].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')");
        }

        public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar()
        {
            base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar();

            AssertSql(
                @"SELECT [p.PersonAddress.Country.Planet.Star].[Name]
FROM [OwnedPerson] AS [p]
LEFT JOIN (
    SELECT [p.PersonAddress].*
    FROM [OwnedPerson] AS [p.PersonAddress]
    WHERE [p.PersonAddress].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t] ON [p].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [p.PersonAddress.Country].*
    FROM [OwnedPerson] AS [p.PersonAddress.Country]
    WHERE [p.PersonAddress.Country].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN [Planet] AS [p.PersonAddress.Country.Planet] ON [t0].[PersonAddress_Country_PlanetId] = [p.PersonAddress.Country.Planet].[Id]
LEFT JOIN [Star] AS [p.PersonAddress.Country.Planet.Star] ON [p.PersonAddress.Country.Planet].[StarId] = [p.PersonAddress.Country.Planet.Star].[Id]
WHERE [p].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')");
        }

        public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection()
        {
            base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection();

            AssertSql(
                @"SELECT [p.PersonAddress.Country.Planet.Star].[Id], [p.PersonAddress.Country.Planet.Star].[Name]
FROM [OwnedPerson] AS [p]
LEFT JOIN (
    SELECT [p.PersonAddress].*
    FROM [OwnedPerson] AS [p.PersonAddress]
    WHERE [p.PersonAddress].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t] ON [p].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [p.PersonAddress.Country].*
    FROM [OwnedPerson] AS [p.PersonAddress.Country]
    WHERE [p.PersonAddress.Country].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN [Planet] AS [p.PersonAddress.Country.Planet] ON [t0].[PersonAddress_Country_PlanetId] = [p.PersonAddress.Country.Planet].[Id]
LEFT JOIN [Star] AS [p.PersonAddress.Country.Planet.Star] ON [p.PersonAddress.Country.Planet].[StarId] = [p.PersonAddress.Country.Planet.Star].[Id]
WHERE [p].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson') AND ([p.PersonAddress.Country.Planet.Star].[Name] = N'Sol')");
        }

        public override void Query_with_OfType_eagerly_loads_correct_owned_navigations()
        {
            base.Query_with_OfType_eagerly_loads_correct_owned_navigations();

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [t].[Id], [t0].[Id], [t0].[LeafAAddress_Country_Name], [t0].[LeafAAddress_Country_PlanetId], [t1].[Id], [t2].[Id], [t2].[BranchAddress_Country_Name], [t2].[BranchAddress_Country_PlanetId], [t3].[Id], [t4].[Id], [t4].[PersonAddress_Country_Name], [t4].[PersonAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [l.LeafAAddress].*
    FROM [OwnedPerson] AS [l.LeafAAddress]
    WHERE [l.LeafAAddress].[Discriminator] = N'LeafA'
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [l.LeafAAddress.Country].*
    FROM [OwnedPerson] AS [l.LeafAAddress.Country]
    WHERE [l.LeafAAddress.Country].[Discriminator] = N'LeafA'
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [b.BranchAddress].*
    FROM [OwnedPerson] AS [b.BranchAddress]
    WHERE [b.BranchAddress].[Discriminator] IN (N'LeafA', N'Branch')
) AS [t1] ON [o].[Id] = [t1].[Id]
LEFT JOIN (
    SELECT [b.BranchAddress.Country].*
    FROM [OwnedPerson] AS [b.BranchAddress.Country]
    WHERE [b.BranchAddress.Country].[Discriminator] IN (N'LeafA', N'Branch')
) AS [t2] ON [t1].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [o.PersonAddress].*
    FROM [OwnedPerson] AS [o.PersonAddress]
    WHERE [o.PersonAddress].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t3] ON [o].[Id] = [t3].[Id]
LEFT JOIN (
    SELECT [o.PersonAddress.Country].*
    FROM [OwnedPerson] AS [o.PersonAddress.Country]
    WHERE [o.PersonAddress.Country].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t4] ON [t3].[Id] = [t4].[Id]
WHERE [o].[Discriminator] = N'LeafA'
ORDER BY [o].[Id]",
                //
                @"SELECT [o.Orders].[Id], [o.Orders].[ClientId]
FROM [Order] AS [o.Orders]
INNER JOIN (
    SELECT DISTINCT [o0].[Id]
    FROM [OwnedPerson] AS [o0]
    LEFT JOIN (
        SELECT [l.LeafAAddress0].*
        FROM [OwnedPerson] AS [l.LeafAAddress0]
        WHERE [l.LeafAAddress0].[Discriminator] = N'LeafA'
    ) AS [t5] ON [o0].[Id] = [t5].[Id]
    LEFT JOIN (
        SELECT [l.LeafAAddress.Country0].*
        FROM [OwnedPerson] AS [l.LeafAAddress.Country0]
        WHERE [l.LeafAAddress.Country0].[Discriminator] = N'LeafA'
    ) AS [t6] ON [t5].[Id] = [t6].[Id]
    LEFT JOIN (
        SELECT [b.BranchAddress0].*
        FROM [OwnedPerson] AS [b.BranchAddress0]
        WHERE [b.BranchAddress0].[Discriminator] IN (N'LeafA', N'Branch')
    ) AS [t7] ON [o0].[Id] = [t7].[Id]
    LEFT JOIN (
        SELECT [b.BranchAddress.Country0].*
        FROM [OwnedPerson] AS [b.BranchAddress.Country0]
        WHERE [b.BranchAddress.Country0].[Discriminator] IN (N'LeafA', N'Branch')
    ) AS [t8] ON [t7].[Id] = [t8].[Id]
    LEFT JOIN (
        SELECT [o.PersonAddress0].*
        FROM [OwnedPerson] AS [o.PersonAddress0]
        WHERE [o.PersonAddress0].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
    ) AS [t9] ON [o0].[Id] = [t9].[Id]
    LEFT JOIN (
        SELECT [o.PersonAddress.Country0].*
        FROM [OwnedPerson] AS [o.PersonAddress.Country0]
        WHERE [o.PersonAddress.Country0].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
    ) AS [t10] ON [t9].[Id] = [t10].[Id]
    WHERE [o0].[Discriminator] = N'LeafA'
) AS [t11] ON [o.Orders].[ClientId] = [t11].[Id]
ORDER BY [t11].[Id]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class OwnedQuerySqlServerFixture : RelationalOwnedQueryFixture
        {
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
        }
    }
}
