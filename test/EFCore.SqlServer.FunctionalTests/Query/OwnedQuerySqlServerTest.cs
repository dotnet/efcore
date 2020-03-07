// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class OwnedQuerySqlServerTest : RelationalOwnedQueryTestBase<OwnedQuerySqlServerTest.OwnedQuerySqlServerFixture>
    {
        public OwnedQuerySqlServerTest(OwnedQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Query_with_owned_entity_equality_operator(bool async)
        {
            await base.Query_with_owned_entity_equality_operator(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [t0].[Id], [t0].[PersonAddress_AddressLine], [t0].[PersonAddress_ZipCode], [t1].[Id], [t1].[PersonAddress_Country_Name], [t1].[PersonAddress_Country_PlanetId], [t3].[Id], [t4].[Id], [t4].[BranchAddress_Country_Name], [t4].[BranchAddress_Country_PlanetId], [t6].[Id], [t7].[Id], [t7].[LeafAAddress_Country_Name], [t7].[LeafAAddress_Country_PlanetId], [t].[Id], [o9].[ClientId], [o9].[Id], [o9].[OrderDate]
FROM [OwnedPerson] AS [o]
CROSS JOIN (
    SELECT [o0].[Id], [o0].[Discriminator], [o0].[Name]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[Discriminator] = N'LeafB'
) AS [t]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_AddressLine], [o1].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_ZipCode] IS NOT NULL
) AS [t0] ON [o].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [o2].[Id], [o2].[PersonAddress_Country_Name], [o2].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o2]
    WHERE [o2].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t1] ON [t0].[Id] = [t1].[Id]
LEFT JOIN (
    SELECT [o3].[Id], [t2].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o3]
    INNER JOIN (
        SELECT [o4].[Id], [o4].[Discriminator], [o4].[Name]
        FROM [OwnedPerson] AS [o4]
        WHERE [o4].[Discriminator] IN (N'Branch', N'LeafA')
    ) AS [t2] ON [o3].[Id] = [t2].[Id]
) AS [t3] ON [o].[Id] = [t3].[Id]
LEFT JOIN (
    SELECT [o5].[Id], [o5].[BranchAddress_Country_Name], [o5].[BranchAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o5]
    WHERE [o5].[BranchAddress_Country_PlanetId] IS NOT NULL
) AS [t4] ON [t3].[Id] = [t4].[Id]
LEFT JOIN (
    SELECT [o6].[Id], [t5].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o6]
    INNER JOIN (
        SELECT [o7].[Id], [o7].[Discriminator], [o7].[Name]
        FROM [OwnedPerson] AS [o7]
        WHERE [o7].[Discriminator] = N'LeafA'
    ) AS [t5] ON [o6].[Id] = [t5].[Id]
) AS [t6] ON [o].[Id] = [t6].[Id]
LEFT JOIN (
    SELECT [o8].[Id], [o8].[LeafAAddress_Country_Name], [o8].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o8]
    WHERE [o8].[LeafAAddress_Country_PlanetId] IS NOT NULL
) AS [t7] ON [t6].[Id] = [t7].[Id]
LEFT JOIN [Order] AS [o9] ON [o].[Id] = [o9].[ClientId]
WHERE 0 = 1
ORDER BY [o].[Id], [t].[Id], [o9].[ClientId], [o9].[Id]");
        }

        public override async Task Query_for_base_type_loads_all_owned_navs(bool async)
        {
            await base.Query_for_base_type_loads_all_owned_navs(async);

            // See issue #10067
            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [t].[Id], [t].[PersonAddress_AddressLine], [t].[PersonAddress_ZipCode], [t0].[Id], [t0].[PersonAddress_Country_Name], [t0].[PersonAddress_Country_PlanetId], [t2].[Id], [t3].[Id], [t3].[BranchAddress_Country_Name], [t3].[BranchAddress_Country_PlanetId], [t5].[Id], [t6].[Id], [t6].[LeafBAddress_Country_Name], [t6].[LeafBAddress_Country_PlanetId], [t8].[Id], [t9].[Id], [t9].[LeafAAddress_Country_Name], [t9].[LeafAAddress_Country_PlanetId], [o11].[ClientId], [o11].[Id], [o11].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [o2].[Id], [t1].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o2]
    INNER JOIN (
        SELECT [o3].[Id], [o3].[Discriminator], [o3].[Name]
        FROM [OwnedPerson] AS [o3]
        WHERE [o3].[Discriminator] IN (N'Branch', N'LeafA')
    ) AS [t1] ON [o2].[Id] = [t1].[Id]
) AS [t2] ON [o].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [o4].[Id], [o4].[BranchAddress_Country_Name], [o4].[BranchAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o4]
    WHERE [o4].[BranchAddress_Country_PlanetId] IS NOT NULL
) AS [t3] ON [t2].[Id] = [t3].[Id]
LEFT JOIN (
    SELECT [o5].[Id], [t4].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o5]
    INNER JOIN (
        SELECT [o6].[Id], [o6].[Discriminator], [o6].[Name]
        FROM [OwnedPerson] AS [o6]
        WHERE [o6].[Discriminator] = N'LeafB'
    ) AS [t4] ON [o5].[Id] = [t4].[Id]
) AS [t5] ON [o].[Id] = [t5].[Id]
LEFT JOIN (
    SELECT [o7].[Id], [o7].[LeafBAddress_Country_Name], [o7].[LeafBAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o7]
    WHERE [o7].[LeafBAddress_Country_PlanetId] IS NOT NULL
) AS [t6] ON [t5].[Id] = [t6].[Id]
LEFT JOIN (
    SELECT [o8].[Id], [t7].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o8]
    INNER JOIN (
        SELECT [o9].[Id], [o9].[Discriminator], [o9].[Name]
        FROM [OwnedPerson] AS [o9]
        WHERE [o9].[Discriminator] = N'LeafA'
    ) AS [t7] ON [o8].[Id] = [t7].[Id]
) AS [t8] ON [o].[Id] = [t8].[Id]
LEFT JOIN (
    SELECT [o10].[Id], [o10].[LeafAAddress_Country_Name], [o10].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o10]
    WHERE [o10].[LeafAAddress_Country_PlanetId] IS NOT NULL
) AS [t9] ON [t8].[Id] = [t9].[Id]
LEFT JOIN [Order] AS [o11] ON [o].[Id] = [o11].[ClientId]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
ORDER BY [o].[Id], [o11].[ClientId], [o11].[Id]");
        }

        public override async Task No_ignored_include_warning_when_implicit_load(bool async)
        {
            await base.No_ignored_include_warning_when_implicit_load(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM [OwnedPerson] AS [o]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')");
        }

        public override async Task Query_for_branch_type_loads_all_owned_navs(bool async)
        {
            await base.Query_for_branch_type_loads_all_owned_navs(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [t].[Id], [t].[PersonAddress_AddressLine], [t].[PersonAddress_ZipCode], [t0].[Id], [t0].[PersonAddress_Country_Name], [t0].[PersonAddress_Country_PlanetId], [t2].[Id], [t3].[Id], [t3].[BranchAddress_Country_Name], [t3].[BranchAddress_Country_PlanetId], [t5].[Id], [t6].[Id], [t6].[LeafAAddress_Country_Name], [t6].[LeafAAddress_Country_PlanetId], [o8].[ClientId], [o8].[Id], [o8].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [o2].[Id], [t1].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o2]
    INNER JOIN (
        SELECT [o3].[Id], [o3].[Discriminator], [o3].[Name]
        FROM [OwnedPerson] AS [o3]
        WHERE [o3].[Discriminator] IN (N'Branch', N'LeafA')
    ) AS [t1] ON [o2].[Id] = [t1].[Id]
) AS [t2] ON [o].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [o4].[Id], [o4].[BranchAddress_Country_Name], [o4].[BranchAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o4]
    WHERE [o4].[BranchAddress_Country_PlanetId] IS NOT NULL
) AS [t3] ON [t2].[Id] = [t3].[Id]
LEFT JOIN (
    SELECT [o5].[Id], [t4].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o5]
    INNER JOIN (
        SELECT [o6].[Id], [o6].[Discriminator], [o6].[Name]
        FROM [OwnedPerson] AS [o6]
        WHERE [o6].[Discriminator] = N'LeafA'
    ) AS [t4] ON [o5].[Id] = [t4].[Id]
) AS [t5] ON [o].[Id] = [t5].[Id]
LEFT JOIN (
    SELECT [o7].[Id], [o7].[LeafAAddress_Country_Name], [o7].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o7]
    WHERE [o7].[LeafAAddress_Country_PlanetId] IS NOT NULL
) AS [t6] ON [t5].[Id] = [t6].[Id]
LEFT JOIN [Order] AS [o8] ON [o].[Id] = [o8].[ClientId]
WHERE [o].[Discriminator] IN (N'Branch', N'LeafA')
ORDER BY [o].[Id], [o8].[ClientId], [o8].[Id]");
        }

        public override async Task Query_for_branch_type_loads_all_owned_navs_tracking(bool async)
        {
            await base.Query_for_branch_type_loads_all_owned_navs_tracking(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [t].[Id], [t].[PersonAddress_AddressLine], [t].[PersonAddress_ZipCode], [t0].[Id], [t0].[PersonAddress_Country_Name], [t0].[PersonAddress_Country_PlanetId], [t2].[Id], [t3].[Id], [t3].[BranchAddress_Country_Name], [t3].[BranchAddress_Country_PlanetId], [t5].[Id], [t6].[Id], [t6].[LeafAAddress_Country_Name], [t6].[LeafAAddress_Country_PlanetId], [o8].[ClientId], [o8].[Id], [o8].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [o2].[Id], [t1].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o2]
    INNER JOIN (
        SELECT [o3].[Id], [o3].[Discriminator], [o3].[Name]
        FROM [OwnedPerson] AS [o3]
        WHERE [o3].[Discriminator] IN (N'Branch', N'LeafA')
    ) AS [t1] ON [o2].[Id] = [t1].[Id]
) AS [t2] ON [o].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [o4].[Id], [o4].[BranchAddress_Country_Name], [o4].[BranchAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o4]
    WHERE [o4].[BranchAddress_Country_PlanetId] IS NOT NULL
) AS [t3] ON [t2].[Id] = [t3].[Id]
LEFT JOIN (
    SELECT [o5].[Id], [t4].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o5]
    INNER JOIN (
        SELECT [o6].[Id], [o6].[Discriminator], [o6].[Name]
        FROM [OwnedPerson] AS [o6]
        WHERE [o6].[Discriminator] = N'LeafA'
    ) AS [t4] ON [o5].[Id] = [t4].[Id]
) AS [t5] ON [o].[Id] = [t5].[Id]
LEFT JOIN (
    SELECT [o7].[Id], [o7].[LeafAAddress_Country_Name], [o7].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o7]
    WHERE [o7].[LeafAAddress_Country_PlanetId] IS NOT NULL
) AS [t6] ON [t5].[Id] = [t6].[Id]
LEFT JOIN [Order] AS [o8] ON [o].[Id] = [o8].[ClientId]
WHERE [o].[Discriminator] IN (N'Branch', N'LeafA')
ORDER BY [o].[Id], [o8].[ClientId], [o8].[Id]");
        }

        public override async Task Query_for_leaf_type_loads_all_owned_navs(bool async)
        {
            await base.Query_for_leaf_type_loads_all_owned_navs(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [t].[Id], [t].[PersonAddress_AddressLine], [t].[PersonAddress_ZipCode], [t0].[Id], [t0].[PersonAddress_Country_Name], [t0].[PersonAddress_Country_PlanetId], [t2].[Id], [t3].[Id], [t3].[BranchAddress_Country_Name], [t3].[BranchAddress_Country_PlanetId], [t5].[Id], [t6].[Id], [t6].[LeafAAddress_Country_Name], [t6].[LeafAAddress_Country_PlanetId], [o8].[ClientId], [o8].[Id], [o8].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [o2].[Id], [t1].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o2]
    INNER JOIN (
        SELECT [o3].[Id], [o3].[Discriminator], [o3].[Name]
        FROM [OwnedPerson] AS [o3]
        WHERE [o3].[Discriminator] IN (N'Branch', N'LeafA')
    ) AS [t1] ON [o2].[Id] = [t1].[Id]
) AS [t2] ON [o].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [o4].[Id], [o4].[BranchAddress_Country_Name], [o4].[BranchAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o4]
    WHERE [o4].[BranchAddress_Country_PlanetId] IS NOT NULL
) AS [t3] ON [t2].[Id] = [t3].[Id]
LEFT JOIN (
    SELECT [o5].[Id], [t4].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o5]
    INNER JOIN (
        SELECT [o6].[Id], [o6].[Discriminator], [o6].[Name]
        FROM [OwnedPerson] AS [o6]
        WHERE [o6].[Discriminator] = N'LeafA'
    ) AS [t4] ON [o5].[Id] = [t4].[Id]
) AS [t5] ON [o].[Id] = [t5].[Id]
LEFT JOIN (
    SELECT [o7].[Id], [o7].[LeafAAddress_Country_Name], [o7].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o7]
    WHERE [o7].[LeafAAddress_Country_PlanetId] IS NOT NULL
) AS [t6] ON [t5].[Id] = [t6].[Id]
LEFT JOIN [Order] AS [o8] ON [o].[Id] = [o8].[ClientId]
WHERE [o].[Discriminator] = N'LeafA'
ORDER BY [o].[Id], [o8].[ClientId], [o8].[Id]");
        }

        public override async Task Query_when_subquery(bool async)
        {
            await base.Query_when_subquery(async);

            AssertSql(
                @"@__p_0='5'

SELECT [t0].[Id], [t0].[Discriminator], [t0].[Name], [t2].[Id], [t2].[PersonAddress_AddressLine], [t2].[PersonAddress_ZipCode], [t3].[Id], [t3].[PersonAddress_Country_Name], [t3].[PersonAddress_Country_PlanetId], [t5].[Id], [t6].[Id], [t6].[BranchAddress_Country_Name], [t6].[BranchAddress_Country_PlanetId], [t8].[Id], [t9].[Id], [t9].[LeafBAddress_Country_Name], [t9].[LeafBAddress_Country_PlanetId], [t11].[Id], [t12].[Id], [t12].[LeafAAddress_Country_Name], [t12].[LeafAAddress_Country_PlanetId], [o12].[ClientId], [o12].[Id], [o12].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [t].[Id], [t].[Discriminator], [t].[Name]
    FROM (
        SELECT DISTINCT [o].[Id], [o].[Discriminator], [o].[Name]
        FROM [OwnedPerson] AS [o]
        WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
    ) AS [t]
    ORDER BY [t].[Id]
) AS [t0]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t1] ON [t0].[Id] = [t1].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_AddressLine], [o1].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_ZipCode] IS NOT NULL
) AS [t2] ON [t0].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [o2].[Id], [o2].[PersonAddress_Country_Name], [o2].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o2]
    WHERE [o2].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t3] ON [t2].[Id] = [t3].[Id]
LEFT JOIN (
    SELECT [o3].[Id], [t4].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o3]
    INNER JOIN (
        SELECT [o4].[Id], [o4].[Discriminator], [o4].[Name]
        FROM [OwnedPerson] AS [o4]
        WHERE [o4].[Discriminator] IN (N'Branch', N'LeafA')
    ) AS [t4] ON [o3].[Id] = [t4].[Id]
) AS [t5] ON [t0].[Id] = [t5].[Id]
LEFT JOIN (
    SELECT [o5].[Id], [o5].[BranchAddress_Country_Name], [o5].[BranchAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o5]
    WHERE [o5].[BranchAddress_Country_PlanetId] IS NOT NULL
) AS [t6] ON [t5].[Id] = [t6].[Id]
LEFT JOIN (
    SELECT [o6].[Id], [t7].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o6]
    INNER JOIN (
        SELECT [o7].[Id], [o7].[Discriminator], [o7].[Name]
        FROM [OwnedPerson] AS [o7]
        WHERE [o7].[Discriminator] = N'LeafB'
    ) AS [t7] ON [o6].[Id] = [t7].[Id]
) AS [t8] ON [t0].[Id] = [t8].[Id]
LEFT JOIN (
    SELECT [o8].[Id], [o8].[LeafBAddress_Country_Name], [o8].[LeafBAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o8]
    WHERE [o8].[LeafBAddress_Country_PlanetId] IS NOT NULL
) AS [t9] ON [t8].[Id] = [t9].[Id]
LEFT JOIN (
    SELECT [o9].[Id], [t10].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o9]
    INNER JOIN (
        SELECT [o10].[Id], [o10].[Discriminator], [o10].[Name]
        FROM [OwnedPerson] AS [o10]
        WHERE [o10].[Discriminator] = N'LeafA'
    ) AS [t10] ON [o9].[Id] = [t10].[Id]
) AS [t11] ON [t0].[Id] = [t11].[Id]
LEFT JOIN (
    SELECT [o11].[Id], [o11].[LeafAAddress_Country_Name], [o11].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o11]
    WHERE [o11].[LeafAAddress_Country_PlanetId] IS NOT NULL
) AS [t12] ON [t11].[Id] = [t12].[Id]
LEFT JOIN [Order] AS [o12] ON [t0].[Id] = [o12].[ClientId]
ORDER BY [t0].[Id], [o12].[ClientId], [o12].[Id]");
        }

        public override async Task Navigation_rewrite_on_owned_reference_projecting_scalar(bool async)
        {
            await base.Navigation_rewrite_on_owned_reference_projecting_scalar(async);

            AssertSql(
                @"SELECT [t0].[PersonAddress_Country_Name]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA') AND ([t0].[PersonAddress_Country_Name] = N'USA')");
        }

        public override async Task Navigation_rewrite_on_owned_reference_projecting_entity(bool async)
        {
            await base.Navigation_rewrite_on_owned_reference_projecting_entity(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [t].[Id], [t].[PersonAddress_AddressLine], [t].[PersonAddress_ZipCode], [t0].[Id], [t0].[PersonAddress_Country_Name], [t0].[PersonAddress_Country_PlanetId], [t2].[Id], [t3].[Id], [t3].[BranchAddress_Country_Name], [t3].[BranchAddress_Country_PlanetId], [t5].[Id], [t6].[Id], [t6].[LeafBAddress_Country_Name], [t6].[LeafBAddress_Country_PlanetId], [t8].[Id], [t9].[Id], [t9].[LeafAAddress_Country_Name], [t9].[LeafAAddress_Country_PlanetId], [o11].[ClientId], [o11].[Id], [o11].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [o2].[Id], [t1].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o2]
    INNER JOIN (
        SELECT [o3].[Id], [o3].[Discriminator], [o3].[Name]
        FROM [OwnedPerson] AS [o3]
        WHERE [o3].[Discriminator] IN (N'Branch', N'LeafA')
    ) AS [t1] ON [o2].[Id] = [t1].[Id]
) AS [t2] ON [o].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [o4].[Id], [o4].[BranchAddress_Country_Name], [o4].[BranchAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o4]
    WHERE [o4].[BranchAddress_Country_PlanetId] IS NOT NULL
) AS [t3] ON [t2].[Id] = [t3].[Id]
LEFT JOIN (
    SELECT [o5].[Id], [t4].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o5]
    INNER JOIN (
        SELECT [o6].[Id], [o6].[Discriminator], [o6].[Name]
        FROM [OwnedPerson] AS [o6]
        WHERE [o6].[Discriminator] = N'LeafB'
    ) AS [t4] ON [o5].[Id] = [t4].[Id]
) AS [t5] ON [o].[Id] = [t5].[Id]
LEFT JOIN (
    SELECT [o7].[Id], [o7].[LeafBAddress_Country_Name], [o7].[LeafBAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o7]
    WHERE [o7].[LeafBAddress_Country_PlanetId] IS NOT NULL
) AS [t6] ON [t5].[Id] = [t6].[Id]
LEFT JOIN (
    SELECT [o8].[Id], [t7].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o8]
    INNER JOIN (
        SELECT [o9].[Id], [o9].[Discriminator], [o9].[Name]
        FROM [OwnedPerson] AS [o9]
        WHERE [o9].[Discriminator] = N'LeafA'
    ) AS [t7] ON [o8].[Id] = [t7].[Id]
) AS [t8] ON [o].[Id] = [t8].[Id]
LEFT JOIN (
    SELECT [o10].[Id], [o10].[LeafAAddress_Country_Name], [o10].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o10]
    WHERE [o10].[LeafAAddress_Country_PlanetId] IS NOT NULL
) AS [t9] ON [t8].[Id] = [t9].[Id]
LEFT JOIN [Order] AS [o11] ON [o].[Id] = [o11].[ClientId]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA') AND ([t0].[PersonAddress_Country_Name] = N'USA')
ORDER BY [o].[Id], [o11].[ClientId], [o11].[Id]");
        }

        public override async Task Navigation_rewrite_on_owned_collection(bool async)
        {
            await base.Navigation_rewrite_on_owned_collection(async);

            AssertSql(
                @"SELECT [o].[Id], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA') AND ((
    SELECT COUNT(*)
    FROM [Order] AS [o1]
    WHERE [o].[Id] = [o1].[ClientId]) > 0)
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Navigation_rewrite_on_owned_collection_with_composition(bool async)
        {
            await base.Navigation_rewrite_on_owned_collection_with_composition(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) CASE
        WHEN [o].[Id] <> 42 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    FROM [Order] AS [o]
    WHERE [o0].[Id] = [o].[ClientId]
    ORDER BY [o].[Id])
FROM [OwnedPerson] AS [o0]
WHERE [o0].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
ORDER BY [o0].[Id]");
        }

        public override async Task Navigation_rewrite_on_owned_collection_with_composition_complex(bool async)
        {
            await base.Navigation_rewrite_on_owned_collection_with_composition_complex(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [t1].[PersonAddress_Country_Name]
    FROM [Order] AS [o]
    LEFT JOIN (
        SELECT [o0].[Id], [o0].[Discriminator], [o0].[Name]
        FROM [OwnedPerson] AS [o0]
        WHERE [o0].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
    ) AS [t] ON [o].[ClientId] = [t].[Id]
    LEFT JOIN (
        SELECT [o1].[Id], [o1].[PersonAddress_AddressLine], [o1].[PersonAddress_ZipCode]
        FROM [OwnedPerson] AS [o1]
        WHERE [o1].[PersonAddress_ZipCode] IS NOT NULL
    ) AS [t0] ON [t].[Id] = [t0].[Id]
    LEFT JOIN (
        SELECT [o2].[Id], [o2].[PersonAddress_Country_Name], [o2].[PersonAddress_Country_PlanetId]
        FROM [OwnedPerson] AS [o2]
        WHERE [o2].[PersonAddress_Country_PlanetId] IS NOT NULL
    ) AS [t1] ON [t0].[Id] = [t1].[Id]
    WHERE [o3].[Id] = [o].[ClientId]
    ORDER BY [o].[Id])
FROM [OwnedPerson] AS [o3]
WHERE [o3].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')");
        }

        public override async Task SelectMany_on_owned_collection(bool async)
        {
            await base.SelectMany_on_owned_collection(async);

            AssertSql(
                @"SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
INNER JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')");
        }

        public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity(bool async)
        {
            await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity(async);

            AssertSql(
                @"SELECT [p].[Id], [p].[StarId]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN [Planet] AS [p] ON [t0].[PersonAddress_Country_PlanetId] = [p].[Id]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')");
        }

        public override async Task Filter_owned_entity_chained_with_regular_entity_followed_by_projecting_owned_collection(bool async)
        {
            await base.Filter_owned_entity_chained_with_regular_entity_followed_by_projecting_owned_collection(async);

            AssertSql(
                @"SELECT [o].[Id], [o2].[ClientId], [o2].[Id], [o2].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN [Planet] AS [p] ON [t0].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Order] AS [o2] ON [o].[Id] = [o2].[ClientId]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA') AND (([p].[Id] <> 42) OR [p].[Id] IS NULL)
ORDER BY [o].[Id], [o2].[ClientId], [o2].[Id]");
        }

        public override async Task Project_multiple_owned_navigations(bool async)
        {
            await base.Project_multiple_owned_navigations(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[PersonAddress_AddressLine], [t].[PersonAddress_ZipCode], [t0].[Id], [t0].[PersonAddress_Country_Name], [t0].[PersonAddress_Country_PlanetId], [p].[Id], [p].[StarId], [o].[Id], [o2].[ClientId], [o2].[Id], [o2].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN [Planet] AS [p] ON [t0].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Order] AS [o2] ON [o].[Id] = [o2].[ClientId]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
ORDER BY [o].[Id], [o2].[ClientId], [o2].[Id]");
        }

        public override async Task Project_multiple_owned_navigations_with_expansion_on_owned_collections(bool async)
        {
            await base.Project_multiple_owned_navigations_with_expansion_on_owned_collections(async);

            AssertSql(
                @"SELECT (
    SELECT COUNT(*)
    FROM [Order] AS [o]
    LEFT JOIN (
        SELECT [o0].[Id], [o0].[Discriminator], [o0].[Name]
        FROM [OwnedPerson] AS [o0]
        WHERE [o0].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
    ) AS [t] ON [o].[ClientId] = [t].[Id]
    LEFT JOIN (
        SELECT [o1].[Id], [o1].[PersonAddress_AddressLine], [o1].[PersonAddress_ZipCode]
        FROM [OwnedPerson] AS [o1]
        WHERE [o1].[PersonAddress_ZipCode] IS NOT NULL
    ) AS [t0] ON [t].[Id] = [t0].[Id]
    LEFT JOIN (
        SELECT [o2].[Id], [o2].[PersonAddress_Country_Name], [o2].[PersonAddress_Country_PlanetId]
        FROM [OwnedPerson] AS [o2]
        WHERE [o2].[PersonAddress_Country_PlanetId] IS NOT NULL
    ) AS [t1] ON [t0].[Id] = [t1].[Id]
    LEFT JOIN [Planet] AS [p] ON [t1].[PersonAddress_Country_PlanetId] = [p].[Id]
    LEFT JOIN [Star] AS [s] ON [p].[StarId] = [s].[Id]
    WHERE ([o3].[Id] = [o].[ClientId]) AND (([s].[Id] <> 42) OR [s].[Id] IS NULL)) AS [Count], [p0].[Id], [p0].[StarId]
FROM [OwnedPerson] AS [o3]
LEFT JOIN (
    SELECT [o4].[Id], [o4].[PersonAddress_AddressLine], [o4].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o4]
    WHERE [o4].[PersonAddress_ZipCode] IS NOT NULL
) AS [t2] ON [o3].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [o5].[Id], [o5].[PersonAddress_Country_Name], [o5].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o5]
    WHERE [o5].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t3] ON [t2].[Id] = [t3].[Id]
LEFT JOIN [Planet] AS [p0] ON [t3].[PersonAddress_Country_PlanetId] = [p0].[Id]
WHERE [o3].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
ORDER BY [o3].[Id]");
        }

        public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_filter(bool async)
        {
            await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_filter(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [t].[Id], [t].[PersonAddress_AddressLine], [t].[PersonAddress_ZipCode], [t0].[Id], [t0].[PersonAddress_Country_Name], [t0].[PersonAddress_Country_PlanetId], [t2].[Id], [t3].[Id], [t3].[BranchAddress_Country_Name], [t3].[BranchAddress_Country_PlanetId], [t5].[Id], [t6].[Id], [t6].[LeafBAddress_Country_Name], [t6].[LeafBAddress_Country_PlanetId], [t8].[Id], [t9].[Id], [t9].[LeafAAddress_Country_Name], [t9].[LeafAAddress_Country_PlanetId], [o11].[ClientId], [o11].[Id], [o11].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN [Planet] AS [p] ON [t0].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN (
    SELECT [o2].[Id], [t1].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o2]
    INNER JOIN (
        SELECT [o3].[Id], [o3].[Discriminator], [o3].[Name]
        FROM [OwnedPerson] AS [o3]
        WHERE [o3].[Discriminator] IN (N'Branch', N'LeafA')
    ) AS [t1] ON [o2].[Id] = [t1].[Id]
) AS [t2] ON [o].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [o4].[Id], [o4].[BranchAddress_Country_Name], [o4].[BranchAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o4]
    WHERE [o4].[BranchAddress_Country_PlanetId] IS NOT NULL
) AS [t3] ON [t2].[Id] = [t3].[Id]
LEFT JOIN (
    SELECT [o5].[Id], [t4].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o5]
    INNER JOIN (
        SELECT [o6].[Id], [o6].[Discriminator], [o6].[Name]
        FROM [OwnedPerson] AS [o6]
        WHERE [o6].[Discriminator] = N'LeafB'
    ) AS [t4] ON [o5].[Id] = [t4].[Id]
) AS [t5] ON [o].[Id] = [t5].[Id]
LEFT JOIN (
    SELECT [o7].[Id], [o7].[LeafBAddress_Country_Name], [o7].[LeafBAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o7]
    WHERE [o7].[LeafBAddress_Country_PlanetId] IS NOT NULL
) AS [t6] ON [t5].[Id] = [t6].[Id]
LEFT JOIN (
    SELECT [o8].[Id], [t7].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o8]
    INNER JOIN (
        SELECT [o9].[Id], [o9].[Discriminator], [o9].[Name]
        FROM [OwnedPerson] AS [o9]
        WHERE [o9].[Discriminator] = N'LeafA'
    ) AS [t7] ON [o8].[Id] = [t7].[Id]
) AS [t8] ON [o].[Id] = [t8].[Id]
LEFT JOIN (
    SELECT [o10].[Id], [o10].[LeafAAddress_Country_Name], [o10].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o10]
    WHERE [o10].[LeafAAddress_Country_PlanetId] IS NOT NULL
) AS [t9] ON [t8].[Id] = [t9].[Id]
LEFT JOIN [Order] AS [o11] ON [o].[Id] = [o11].[ClientId]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA') AND (([p].[Id] <> 7) OR [p].[Id] IS NULL)
ORDER BY [o].[Id], [o11].[ClientId], [o11].[Id]");
        }

        public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property(bool async)
        {
            await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property(async);

            AssertSql(
                @"SELECT [p].[Id]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN [Planet] AS [p] ON [t0].[PersonAddress_Country_PlanetId] = [p].[Id]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')");
        }

        public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection(bool async)
        {
            await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection(async);

            AssertSql(
                @"SELECT [o].[Id], [m].[Id], [m].[Diameter], [m].[PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN [Planet] AS [p] ON [t0].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Moon] AS [m] ON [p].[Id] = [m].[PlanetId]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
ORDER BY [o].[Id], [m].[Id]");
        }

        public override async Task SelectMany_on_owned_reference_followed_by_regular_entity_and_collection(bool async)
        {
            await base.SelectMany_on_owned_reference_followed_by_regular_entity_and_collection(async);

            AssertSql(
                @"SELECT [m].[Id], [m].[Diameter], [m].[PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN [Planet] AS [p] ON [t0].[PersonAddress_Country_PlanetId] = [p].[Id]
INNER JOIN [Moon] AS [m] ON [p].[Id] = [m].[PlanetId]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')");
        }

        public override async Task SelectMany_on_owned_reference_with_entity_in_between_ending_in_owned_collection(bool async)
        {
            await base.SelectMany_on_owned_reference_with_entity_in_between_ending_in_owned_collection(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[Name], [e].[StarId]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN [Planet] AS [p] ON [t0].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Star] AS [s] ON [p].[StarId] = [s].[Id]
INNER JOIN [Element] AS [e] ON [s].[Id] = [e].[StarId]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')");
        }

        public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference(bool async)
        {
            await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference(async);

            AssertSql(
                @"SELECT [s].[Id], [s].[Name], [o].[Id], [e].[Id], [e].[Name], [e].[StarId]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN [Planet] AS [p] ON [t0].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Star] AS [s] ON [p].[StarId] = [s].[Id]
LEFT JOIN [Element] AS [e] ON [s].[Id] = [e].[StarId]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
ORDER BY [o].[Id], [e].[Id]");
        }

        public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar(
            bool async)
        {
            await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar(async);

            AssertSql(
                @"SELECT [s].[Name]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN [Planet] AS [p] ON [t0].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Star] AS [s] ON [p].[StarId] = [s].[Id]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')");
        }

        public override async Task
            Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection(bool async)
        {
            await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection(
                async);

            AssertSql(
                @"SELECT [s].[Id], [s].[Name], [o].[Id], [e].[Id], [e].[Name], [e].[StarId]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN [Planet] AS [p] ON [t0].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Star] AS [s] ON [p].[StarId] = [s].[Id]
LEFT JOIN [Element] AS [e] ON [s].[Id] = [e].[StarId]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA') AND ([s].[Name] = N'Sol')
ORDER BY [o].[Id], [e].[Id]");
        }

        public override async Task Query_with_OfType_eagerly_loads_correct_owned_navigations(bool async)
        {
            await base.Query_with_OfType_eagerly_loads_correct_owned_navigations(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [t].[Id], [t].[PersonAddress_AddressLine], [t].[PersonAddress_ZipCode], [t0].[Id], [t0].[PersonAddress_Country_Name], [t0].[PersonAddress_Country_PlanetId], [t2].[Id], [t3].[Id], [t3].[BranchAddress_Country_Name], [t3].[BranchAddress_Country_PlanetId], [t5].[Id], [t6].[Id], [t6].[LeafAAddress_Country_Name], [t6].[LeafAAddress_Country_PlanetId], [o8].[ClientId], [o8].[Id], [o8].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [o2].[Id], [t1].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o2]
    INNER JOIN (
        SELECT [o3].[Id], [o3].[Discriminator], [o3].[Name]
        FROM [OwnedPerson] AS [o3]
        WHERE [o3].[Discriminator] IN (N'Branch', N'LeafA')
    ) AS [t1] ON [o2].[Id] = [t1].[Id]
) AS [t2] ON [o].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [o4].[Id], [o4].[BranchAddress_Country_Name], [o4].[BranchAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o4]
    WHERE [o4].[BranchAddress_Country_PlanetId] IS NOT NULL
) AS [t3] ON [t2].[Id] = [t3].[Id]
LEFT JOIN (
    SELECT [o5].[Id], [t4].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o5]
    INNER JOIN (
        SELECT [o6].[Id], [o6].[Discriminator], [o6].[Name]
        FROM [OwnedPerson] AS [o6]
        WHERE [o6].[Discriminator] = N'LeafA'
    ) AS [t4] ON [o5].[Id] = [t4].[Id]
) AS [t5] ON [o].[Id] = [t5].[Id]
LEFT JOIN (
    SELECT [o7].[Id], [o7].[LeafAAddress_Country_Name], [o7].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o7]
    WHERE [o7].[LeafAAddress_Country_PlanetId] IS NOT NULL
) AS [t6] ON [t5].[Id] = [t6].[Id]
LEFT JOIN [Order] AS [o8] ON [o].[Id] = [o8].[ClientId]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA') AND ([o].[Discriminator] = N'LeafA')
ORDER BY [o].[Id], [o8].[ClientId], [o8].[Id]");
        }

        public override async Task Preserve_includes_when_applying_skip_take_after_anonymous_type_select(bool async)
        {
            await base.Preserve_includes_when_applying_skip_take_after_anonymous_type_select(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM [OwnedPerson] AS [o]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')",
                //
                @"@__p_1='0'
@__p_2='100'

SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t1].[Id], [t1].[PersonAddress_AddressLine], [t1].[PersonAddress_ZipCode], [t2].[Id], [t2].[PersonAddress_Country_Name], [t2].[PersonAddress_Country_PlanetId], [t4].[Id], [t5].[Id], [t5].[BranchAddress_Country_Name], [t5].[BranchAddress_Country_PlanetId], [t7].[Id], [t8].[Id], [t8].[LeafBAddress_Country_Name], [t8].[LeafBAddress_Country_PlanetId], [t10].[Id], [t11].[Id], [t11].[LeafAAddress_Country_Name], [t11].[LeafAAddress_Country_PlanetId], [o12].[ClientId], [o12].[Id], [o12].[OrderDate]
FROM (
    SELECT [o].[Id], [o].[Discriminator], [o].[Name]
    FROM [OwnedPerson] AS [o]
    WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
    ORDER BY [o].[Id]
    OFFSET @__p_1 ROWS FETCH NEXT @__p_2 ROWS ONLY
) AS [t]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_AddressLine], [o1].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_ZipCode] IS NOT NULL
) AS [t1] ON [t].[Id] = [t1].[Id]
LEFT JOIN (
    SELECT [o2].[Id], [o2].[PersonAddress_Country_Name], [o2].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o2]
    WHERE [o2].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t2] ON [t1].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [o3].[Id], [t3].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o3]
    INNER JOIN (
        SELECT [o4].[Id], [o4].[Discriminator], [o4].[Name]
        FROM [OwnedPerson] AS [o4]
        WHERE [o4].[Discriminator] IN (N'Branch', N'LeafA')
    ) AS [t3] ON [o3].[Id] = [t3].[Id]
) AS [t4] ON [t].[Id] = [t4].[Id]
LEFT JOIN (
    SELECT [o5].[Id], [o5].[BranchAddress_Country_Name], [o5].[BranchAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o5]
    WHERE [o5].[BranchAddress_Country_PlanetId] IS NOT NULL
) AS [t5] ON [t4].[Id] = [t5].[Id]
LEFT JOIN (
    SELECT [o6].[Id], [t6].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o6]
    INNER JOIN (
        SELECT [o7].[Id], [o7].[Discriminator], [o7].[Name]
        FROM [OwnedPerson] AS [o7]
        WHERE [o7].[Discriminator] = N'LeafB'
    ) AS [t6] ON [o6].[Id] = [t6].[Id]
) AS [t7] ON [t].[Id] = [t7].[Id]
LEFT JOIN (
    SELECT [o8].[Id], [o8].[LeafBAddress_Country_Name], [o8].[LeafBAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o8]
    WHERE [o8].[LeafBAddress_Country_PlanetId] IS NOT NULL
) AS [t8] ON [t7].[Id] = [t8].[Id]
LEFT JOIN (
    SELECT [o9].[Id], [t9].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o9]
    INNER JOIN (
        SELECT [o10].[Id], [o10].[Discriminator], [o10].[Name]
        FROM [OwnedPerson] AS [o10]
        WHERE [o10].[Discriminator] = N'LeafA'
    ) AS [t9] ON [o9].[Id] = [t9].[Id]
) AS [t10] ON [t].[Id] = [t10].[Id]
LEFT JOIN (
    SELECT [o11].[Id], [o11].[LeafAAddress_Country_Name], [o11].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o11]
    WHERE [o11].[LeafAAddress_Country_PlanetId] IS NOT NULL
) AS [t11] ON [t10].[Id] = [t11].[Id]
LEFT JOIN [Order] AS [o12] ON [t].[Id] = [o12].[ClientId]
ORDER BY [t].[Id], [o12].[ClientId], [o12].[Id]");
        }

        public override async Task Unmapped_property_projection_loads_owned_navigations(bool async)
        {
            await base.Unmapped_property_projection_loads_owned_navigations(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [t].[Id], [t].[PersonAddress_AddressLine], [t].[PersonAddress_ZipCode], [t0].[Id], [t0].[PersonAddress_Country_Name], [t0].[PersonAddress_Country_PlanetId], [t2].[Id], [t3].[Id], [t3].[BranchAddress_Country_Name], [t3].[BranchAddress_Country_PlanetId], [t5].[Id], [t6].[Id], [t6].[LeafBAddress_Country_Name], [t6].[LeafBAddress_Country_PlanetId], [t8].[Id], [t9].[Id], [t9].[LeafAAddress_Country_Name], [t9].[LeafAAddress_Country_PlanetId], [o11].[ClientId], [o11].[Id], [o11].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [o2].[Id], [t1].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o2]
    INNER JOIN (
        SELECT [o3].[Id], [o3].[Discriminator], [o3].[Name]
        FROM [OwnedPerson] AS [o3]
        WHERE [o3].[Discriminator] IN (N'Branch', N'LeafA')
    ) AS [t1] ON [o2].[Id] = [t1].[Id]
) AS [t2] ON [o].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [o4].[Id], [o4].[BranchAddress_Country_Name], [o4].[BranchAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o4]
    WHERE [o4].[BranchAddress_Country_PlanetId] IS NOT NULL
) AS [t3] ON [t2].[Id] = [t3].[Id]
LEFT JOIN (
    SELECT [o5].[Id], [t4].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o5]
    INNER JOIN (
        SELECT [o6].[Id], [o6].[Discriminator], [o6].[Name]
        FROM [OwnedPerson] AS [o6]
        WHERE [o6].[Discriminator] = N'LeafB'
    ) AS [t4] ON [o5].[Id] = [t4].[Id]
) AS [t5] ON [o].[Id] = [t5].[Id]
LEFT JOIN (
    SELECT [o7].[Id], [o7].[LeafBAddress_Country_Name], [o7].[LeafBAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o7]
    WHERE [o7].[LeafBAddress_Country_PlanetId] IS NOT NULL
) AS [t6] ON [t5].[Id] = [t6].[Id]
LEFT JOIN (
    SELECT [o8].[Id], [t7].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o8]
    INNER JOIN (
        SELECT [o9].[Id], [o9].[Discriminator], [o9].[Name]
        FROM [OwnedPerson] AS [o9]
        WHERE [o9].[Discriminator] = N'LeafA'
    ) AS [t7] ON [o8].[Id] = [t7].[Id]
) AS [t8] ON [o].[Id] = [t8].[Id]
LEFT JOIN (
    SELECT [o10].[Id], [o10].[LeafAAddress_Country_Name], [o10].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o10]
    WHERE [o10].[LeafAAddress_Country_PlanetId] IS NOT NULL
) AS [t9] ON [t8].[Id] = [t9].[Id]
LEFT JOIN [Order] AS [o11] ON [o].[Id] = [o11].[ClientId]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA') AND ([o].[Id] = 1)
ORDER BY [o].[Id], [o11].[ClientId], [o11].[Id]");
        }

        public override async Task Client_method_skip_loads_owned_navigations(bool async)
        {
            await base.Client_method_skip_loads_owned_navigations(async);

            AssertSql(
                @"@__p_0='1'

SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t1].[Id], [t1].[PersonAddress_AddressLine], [t1].[PersonAddress_ZipCode], [t2].[Id], [t2].[PersonAddress_Country_Name], [t2].[PersonAddress_Country_PlanetId], [t4].[Id], [t5].[Id], [t5].[BranchAddress_Country_Name], [t5].[BranchAddress_Country_PlanetId], [t7].[Id], [t8].[Id], [t8].[LeafBAddress_Country_Name], [t8].[LeafBAddress_Country_PlanetId], [t10].[Id], [t11].[Id], [t11].[LeafAAddress_Country_Name], [t11].[LeafAAddress_Country_PlanetId], [o12].[ClientId], [o12].[Id], [o12].[OrderDate]
FROM (
    SELECT [o].[Id], [o].[Discriminator], [o].[Name]
    FROM [OwnedPerson] AS [o]
    WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
    ORDER BY [o].[Id]
    OFFSET @__p_0 ROWS
) AS [t]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_AddressLine], [o1].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_ZipCode] IS NOT NULL
) AS [t1] ON [t].[Id] = [t1].[Id]
LEFT JOIN (
    SELECT [o2].[Id], [o2].[PersonAddress_Country_Name], [o2].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o2]
    WHERE [o2].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t2] ON [t1].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [o3].[Id], [t3].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o3]
    INNER JOIN (
        SELECT [o4].[Id], [o4].[Discriminator], [o4].[Name]
        FROM [OwnedPerson] AS [o4]
        WHERE [o4].[Discriminator] IN (N'Branch', N'LeafA')
    ) AS [t3] ON [o3].[Id] = [t3].[Id]
) AS [t4] ON [t].[Id] = [t4].[Id]
LEFT JOIN (
    SELECT [o5].[Id], [o5].[BranchAddress_Country_Name], [o5].[BranchAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o5]
    WHERE [o5].[BranchAddress_Country_PlanetId] IS NOT NULL
) AS [t5] ON [t4].[Id] = [t5].[Id]
LEFT JOIN (
    SELECT [o6].[Id], [t6].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o6]
    INNER JOIN (
        SELECT [o7].[Id], [o7].[Discriminator], [o7].[Name]
        FROM [OwnedPerson] AS [o7]
        WHERE [o7].[Discriminator] = N'LeafB'
    ) AS [t6] ON [o6].[Id] = [t6].[Id]
) AS [t7] ON [t].[Id] = [t7].[Id]
LEFT JOIN (
    SELECT [o8].[Id], [o8].[LeafBAddress_Country_Name], [o8].[LeafBAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o8]
    WHERE [o8].[LeafBAddress_Country_PlanetId] IS NOT NULL
) AS [t8] ON [t7].[Id] = [t8].[Id]
LEFT JOIN (
    SELECT [o9].[Id], [t9].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o9]
    INNER JOIN (
        SELECT [o10].[Id], [o10].[Discriminator], [o10].[Name]
        FROM [OwnedPerson] AS [o10]
        WHERE [o10].[Discriminator] = N'LeafA'
    ) AS [t9] ON [o9].[Id] = [t9].[Id]
) AS [t10] ON [t].[Id] = [t10].[Id]
LEFT JOIN (
    SELECT [o11].[Id], [o11].[LeafAAddress_Country_Name], [o11].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o11]
    WHERE [o11].[LeafAAddress_Country_PlanetId] IS NOT NULL
) AS [t11] ON [t10].[Id] = [t11].[Id]
LEFT JOIN [Order] AS [o12] ON [t].[Id] = [o12].[ClientId]
ORDER BY [t].[Id], [o12].[ClientId], [o12].[Id]");
        }

        public override async Task Client_method_take_loads_owned_navigations(bool async)
        {
            await base.Client_method_take_loads_owned_navigations(async);

            AssertSql(
                @"@__p_0='2'

SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t1].[Id], [t1].[PersonAddress_AddressLine], [t1].[PersonAddress_ZipCode], [t2].[Id], [t2].[PersonAddress_Country_Name], [t2].[PersonAddress_Country_PlanetId], [t4].[Id], [t5].[Id], [t5].[BranchAddress_Country_Name], [t5].[BranchAddress_Country_PlanetId], [t7].[Id], [t8].[Id], [t8].[LeafBAddress_Country_Name], [t8].[LeafBAddress_Country_PlanetId], [t10].[Id], [t11].[Id], [t11].[LeafAAddress_Country_Name], [t11].[LeafAAddress_Country_PlanetId], [o12].[ClientId], [o12].[Id], [o12].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [o].[Id], [o].[Discriminator], [o].[Name]
    FROM [OwnedPerson] AS [o]
    WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
    ORDER BY [o].[Id]
) AS [t]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_AddressLine], [o1].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_ZipCode] IS NOT NULL
) AS [t1] ON [t].[Id] = [t1].[Id]
LEFT JOIN (
    SELECT [o2].[Id], [o2].[PersonAddress_Country_Name], [o2].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o2]
    WHERE [o2].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t2] ON [t1].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [o3].[Id], [t3].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o3]
    INNER JOIN (
        SELECT [o4].[Id], [o4].[Discriminator], [o4].[Name]
        FROM [OwnedPerson] AS [o4]
        WHERE [o4].[Discriminator] IN (N'Branch', N'LeafA')
    ) AS [t3] ON [o3].[Id] = [t3].[Id]
) AS [t4] ON [t].[Id] = [t4].[Id]
LEFT JOIN (
    SELECT [o5].[Id], [o5].[BranchAddress_Country_Name], [o5].[BranchAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o5]
    WHERE [o5].[BranchAddress_Country_PlanetId] IS NOT NULL
) AS [t5] ON [t4].[Id] = [t5].[Id]
LEFT JOIN (
    SELECT [o6].[Id], [t6].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o6]
    INNER JOIN (
        SELECT [o7].[Id], [o7].[Discriminator], [o7].[Name]
        FROM [OwnedPerson] AS [o7]
        WHERE [o7].[Discriminator] = N'LeafB'
    ) AS [t6] ON [o6].[Id] = [t6].[Id]
) AS [t7] ON [t].[Id] = [t7].[Id]
LEFT JOIN (
    SELECT [o8].[Id], [o8].[LeafBAddress_Country_Name], [o8].[LeafBAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o8]
    WHERE [o8].[LeafBAddress_Country_PlanetId] IS NOT NULL
) AS [t8] ON [t7].[Id] = [t8].[Id]
LEFT JOIN (
    SELECT [o9].[Id], [t9].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o9]
    INNER JOIN (
        SELECT [o10].[Id], [o10].[Discriminator], [o10].[Name]
        FROM [OwnedPerson] AS [o10]
        WHERE [o10].[Discriminator] = N'LeafA'
    ) AS [t9] ON [o9].[Id] = [t9].[Id]
) AS [t10] ON [t].[Id] = [t10].[Id]
LEFT JOIN (
    SELECT [o11].[Id], [o11].[LeafAAddress_Country_Name], [o11].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o11]
    WHERE [o11].[LeafAAddress_Country_PlanetId] IS NOT NULL
) AS [t11] ON [t10].[Id] = [t11].[Id]
LEFT JOIN [Order] AS [o12] ON [t].[Id] = [o12].[ClientId]
ORDER BY [t].[Id], [o12].[ClientId], [o12].[Id]");
        }

        public override async Task Client_method_skip_take_loads_owned_navigations(bool async)
        {
            await base.Client_method_skip_take_loads_owned_navigations(async);

            AssertSql(
                @"@__p_0='1'
@__p_1='2'

SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t1].[Id], [t1].[PersonAddress_AddressLine], [t1].[PersonAddress_ZipCode], [t2].[Id], [t2].[PersonAddress_Country_Name], [t2].[PersonAddress_Country_PlanetId], [t4].[Id], [t5].[Id], [t5].[BranchAddress_Country_Name], [t5].[BranchAddress_Country_PlanetId], [t7].[Id], [t8].[Id], [t8].[LeafBAddress_Country_Name], [t8].[LeafBAddress_Country_PlanetId], [t10].[Id], [t11].[Id], [t11].[LeafAAddress_Country_Name], [t11].[LeafAAddress_Country_PlanetId], [o12].[ClientId], [o12].[Id], [o12].[OrderDate]
FROM (
    SELECT [o].[Id], [o].[Discriminator], [o].[Name]
    FROM [OwnedPerson] AS [o]
    WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
    ORDER BY [o].[Id]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_AddressLine], [o1].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_ZipCode] IS NOT NULL
) AS [t1] ON [t].[Id] = [t1].[Id]
LEFT JOIN (
    SELECT [o2].[Id], [o2].[PersonAddress_Country_Name], [o2].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o2]
    WHERE [o2].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t2] ON [t1].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [o3].[Id], [t3].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o3]
    INNER JOIN (
        SELECT [o4].[Id], [o4].[Discriminator], [o4].[Name]
        FROM [OwnedPerson] AS [o4]
        WHERE [o4].[Discriminator] IN (N'Branch', N'LeafA')
    ) AS [t3] ON [o3].[Id] = [t3].[Id]
) AS [t4] ON [t].[Id] = [t4].[Id]
LEFT JOIN (
    SELECT [o5].[Id], [o5].[BranchAddress_Country_Name], [o5].[BranchAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o5]
    WHERE [o5].[BranchAddress_Country_PlanetId] IS NOT NULL
) AS [t5] ON [t4].[Id] = [t5].[Id]
LEFT JOIN (
    SELECT [o6].[Id], [t6].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o6]
    INNER JOIN (
        SELECT [o7].[Id], [o7].[Discriminator], [o7].[Name]
        FROM [OwnedPerson] AS [o7]
        WHERE [o7].[Discriminator] = N'LeafB'
    ) AS [t6] ON [o6].[Id] = [t6].[Id]
) AS [t7] ON [t].[Id] = [t7].[Id]
LEFT JOIN (
    SELECT [o8].[Id], [o8].[LeafBAddress_Country_Name], [o8].[LeafBAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o8]
    WHERE [o8].[LeafBAddress_Country_PlanetId] IS NOT NULL
) AS [t8] ON [t7].[Id] = [t8].[Id]
LEFT JOIN (
    SELECT [o9].[Id], [t9].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o9]
    INNER JOIN (
        SELECT [o10].[Id], [o10].[Discriminator], [o10].[Name]
        FROM [OwnedPerson] AS [o10]
        WHERE [o10].[Discriminator] = N'LeafA'
    ) AS [t9] ON [o9].[Id] = [t9].[Id]
) AS [t10] ON [t].[Id] = [t10].[Id]
LEFT JOIN (
    SELECT [o11].[Id], [o11].[LeafAAddress_Country_Name], [o11].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o11]
    WHERE [o11].[LeafAAddress_Country_PlanetId] IS NOT NULL
) AS [t11] ON [t10].[Id] = [t11].[Id]
LEFT JOIN [Order] AS [o12] ON [t].[Id] = [o12].[ClientId]
ORDER BY [t].[Id], [o12].[ClientId], [o12].[Id]");
        }

        public override async Task Client_method_skip_loads_owned_navigations_variation_2(bool async)
        {
            await base.Client_method_skip_loads_owned_navigations_variation_2(async);

            AssertSql(
                @"@__p_0='1'

SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t1].[Id], [t1].[PersonAddress_AddressLine], [t1].[PersonAddress_ZipCode], [t2].[Id], [t2].[PersonAddress_Country_Name], [t2].[PersonAddress_Country_PlanetId], [t4].[Id], [t5].[Id], [t5].[BranchAddress_Country_Name], [t5].[BranchAddress_Country_PlanetId], [t7].[Id], [t8].[Id], [t8].[LeafBAddress_Country_Name], [t8].[LeafBAddress_Country_PlanetId], [t10].[Id], [t11].[Id], [t11].[LeafAAddress_Country_Name], [t11].[LeafAAddress_Country_PlanetId], [o12].[ClientId], [o12].[Id], [o12].[OrderDate]
FROM (
    SELECT [o].[Id], [o].[Discriminator], [o].[Name]
    FROM [OwnedPerson] AS [o]
    WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
    ORDER BY [o].[Id]
    OFFSET @__p_0 ROWS
) AS [t]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_AddressLine], [o1].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_ZipCode] IS NOT NULL
) AS [t1] ON [t].[Id] = [t1].[Id]
LEFT JOIN (
    SELECT [o2].[Id], [o2].[PersonAddress_Country_Name], [o2].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o2]
    WHERE [o2].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t2] ON [t1].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [o3].[Id], [t3].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o3]
    INNER JOIN (
        SELECT [o4].[Id], [o4].[Discriminator], [o4].[Name]
        FROM [OwnedPerson] AS [o4]
        WHERE [o4].[Discriminator] IN (N'Branch', N'LeafA')
    ) AS [t3] ON [o3].[Id] = [t3].[Id]
) AS [t4] ON [t].[Id] = [t4].[Id]
LEFT JOIN (
    SELECT [o5].[Id], [o5].[BranchAddress_Country_Name], [o5].[BranchAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o5]
    WHERE [o5].[BranchAddress_Country_PlanetId] IS NOT NULL
) AS [t5] ON [t4].[Id] = [t5].[Id]
LEFT JOIN (
    SELECT [o6].[Id], [t6].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o6]
    INNER JOIN (
        SELECT [o7].[Id], [o7].[Discriminator], [o7].[Name]
        FROM [OwnedPerson] AS [o7]
        WHERE [o7].[Discriminator] = N'LeafB'
    ) AS [t6] ON [o6].[Id] = [t6].[Id]
) AS [t7] ON [t].[Id] = [t7].[Id]
LEFT JOIN (
    SELECT [o8].[Id], [o8].[LeafBAddress_Country_Name], [o8].[LeafBAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o8]
    WHERE [o8].[LeafBAddress_Country_PlanetId] IS NOT NULL
) AS [t8] ON [t7].[Id] = [t8].[Id]
LEFT JOIN (
    SELECT [o9].[Id], [t9].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o9]
    INNER JOIN (
        SELECT [o10].[Id], [o10].[Discriminator], [o10].[Name]
        FROM [OwnedPerson] AS [o10]
        WHERE [o10].[Discriminator] = N'LeafA'
    ) AS [t9] ON [o9].[Id] = [t9].[Id]
) AS [t10] ON [t].[Id] = [t10].[Id]
LEFT JOIN (
    SELECT [o11].[Id], [o11].[LeafAAddress_Country_Name], [o11].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o11]
    WHERE [o11].[LeafAAddress_Country_PlanetId] IS NOT NULL
) AS [t11] ON [t10].[Id] = [t11].[Id]
LEFT JOIN [Order] AS [o12] ON [t].[Id] = [o12].[ClientId]
ORDER BY [t].[Id], [o12].[ClientId], [o12].[Id]");
        }

        public override async Task Client_method_take_loads_owned_navigations_variation_2(bool async)
        {
            await base.Client_method_take_loads_owned_navigations_variation_2(async);

            AssertSql(
                @"@__p_0='2'

SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t1].[Id], [t1].[PersonAddress_AddressLine], [t1].[PersonAddress_ZipCode], [t2].[Id], [t2].[PersonAddress_Country_Name], [t2].[PersonAddress_Country_PlanetId], [t4].[Id], [t5].[Id], [t5].[BranchAddress_Country_Name], [t5].[BranchAddress_Country_PlanetId], [t7].[Id], [t8].[Id], [t8].[LeafBAddress_Country_Name], [t8].[LeafBAddress_Country_PlanetId], [t10].[Id], [t11].[Id], [t11].[LeafAAddress_Country_Name], [t11].[LeafAAddress_Country_PlanetId], [o12].[ClientId], [o12].[Id], [o12].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [o].[Id], [o].[Discriminator], [o].[Name]
    FROM [OwnedPerson] AS [o]
    WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
    ORDER BY [o].[Id]
) AS [t]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_AddressLine], [o1].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_ZipCode] IS NOT NULL
) AS [t1] ON [t].[Id] = [t1].[Id]
LEFT JOIN (
    SELECT [o2].[Id], [o2].[PersonAddress_Country_Name], [o2].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o2]
    WHERE [o2].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t2] ON [t1].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [o3].[Id], [t3].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o3]
    INNER JOIN (
        SELECT [o4].[Id], [o4].[Discriminator], [o4].[Name]
        FROM [OwnedPerson] AS [o4]
        WHERE [o4].[Discriminator] IN (N'Branch', N'LeafA')
    ) AS [t3] ON [o3].[Id] = [t3].[Id]
) AS [t4] ON [t].[Id] = [t4].[Id]
LEFT JOIN (
    SELECT [o5].[Id], [o5].[BranchAddress_Country_Name], [o5].[BranchAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o5]
    WHERE [o5].[BranchAddress_Country_PlanetId] IS NOT NULL
) AS [t5] ON [t4].[Id] = [t5].[Id]
LEFT JOIN (
    SELECT [o6].[Id], [t6].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o6]
    INNER JOIN (
        SELECT [o7].[Id], [o7].[Discriminator], [o7].[Name]
        FROM [OwnedPerson] AS [o7]
        WHERE [o7].[Discriminator] = N'LeafB'
    ) AS [t6] ON [o6].[Id] = [t6].[Id]
) AS [t7] ON [t].[Id] = [t7].[Id]
LEFT JOIN (
    SELECT [o8].[Id], [o8].[LeafBAddress_Country_Name], [o8].[LeafBAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o8]
    WHERE [o8].[LeafBAddress_Country_PlanetId] IS NOT NULL
) AS [t8] ON [t7].[Id] = [t8].[Id]
LEFT JOIN (
    SELECT [o9].[Id], [t9].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o9]
    INNER JOIN (
        SELECT [o10].[Id], [o10].[Discriminator], [o10].[Name]
        FROM [OwnedPerson] AS [o10]
        WHERE [o10].[Discriminator] = N'LeafA'
    ) AS [t9] ON [o9].[Id] = [t9].[Id]
) AS [t10] ON [t].[Id] = [t10].[Id]
LEFT JOIN (
    SELECT [o11].[Id], [o11].[LeafAAddress_Country_Name], [o11].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o11]
    WHERE [o11].[LeafAAddress_Country_PlanetId] IS NOT NULL
) AS [t11] ON [t10].[Id] = [t11].[Id]
LEFT JOIN [Order] AS [o12] ON [t].[Id] = [o12].[ClientId]
ORDER BY [t].[Id], [o12].[ClientId], [o12].[Id]");
        }

        public override async Task Client_method_skip_take_loads_owned_navigations_variation_2(bool async)
        {
            await base.Client_method_skip_take_loads_owned_navigations_variation_2(async);

            AssertSql(
                @"@__p_0='1'
@__p_1='2'

SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t1].[Id], [t1].[PersonAddress_AddressLine], [t1].[PersonAddress_ZipCode], [t2].[Id], [t2].[PersonAddress_Country_Name], [t2].[PersonAddress_Country_PlanetId], [t4].[Id], [t5].[Id], [t5].[BranchAddress_Country_Name], [t5].[BranchAddress_Country_PlanetId], [t7].[Id], [t8].[Id], [t8].[LeafBAddress_Country_Name], [t8].[LeafBAddress_Country_PlanetId], [t10].[Id], [t11].[Id], [t11].[LeafAAddress_Country_Name], [t11].[LeafAAddress_Country_PlanetId], [o12].[ClientId], [o12].[Id], [o12].[OrderDate]
FROM (
    SELECT [o].[Id], [o].[Discriminator], [o].[Name]
    FROM [OwnedPerson] AS [o]
    WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
    ORDER BY [o].[Id]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_AddressLine], [o1].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_ZipCode] IS NOT NULL
) AS [t1] ON [t].[Id] = [t1].[Id]
LEFT JOIN (
    SELECT [o2].[Id], [o2].[PersonAddress_Country_Name], [o2].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o2]
    WHERE [o2].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t2] ON [t1].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [o3].[Id], [t3].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o3]
    INNER JOIN (
        SELECT [o4].[Id], [o4].[Discriminator], [o4].[Name]
        FROM [OwnedPerson] AS [o4]
        WHERE [o4].[Discriminator] IN (N'Branch', N'LeafA')
    ) AS [t3] ON [o3].[Id] = [t3].[Id]
) AS [t4] ON [t].[Id] = [t4].[Id]
LEFT JOIN (
    SELECT [o5].[Id], [o5].[BranchAddress_Country_Name], [o5].[BranchAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o5]
    WHERE [o5].[BranchAddress_Country_PlanetId] IS NOT NULL
) AS [t5] ON [t4].[Id] = [t5].[Id]
LEFT JOIN (
    SELECT [o6].[Id], [t6].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o6]
    INNER JOIN (
        SELECT [o7].[Id], [o7].[Discriminator], [o7].[Name]
        FROM [OwnedPerson] AS [o7]
        WHERE [o7].[Discriminator] = N'LeafB'
    ) AS [t6] ON [o6].[Id] = [t6].[Id]
) AS [t7] ON [t].[Id] = [t7].[Id]
LEFT JOIN (
    SELECT [o8].[Id], [o8].[LeafBAddress_Country_Name], [o8].[LeafBAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o8]
    WHERE [o8].[LeafBAddress_Country_PlanetId] IS NOT NULL
) AS [t8] ON [t7].[Id] = [t8].[Id]
LEFT JOIN (
    SELECT [o9].[Id], [t9].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o9]
    INNER JOIN (
        SELECT [o10].[Id], [o10].[Discriminator], [o10].[Name]
        FROM [OwnedPerson] AS [o10]
        WHERE [o10].[Discriminator] = N'LeafA'
    ) AS [t9] ON [o9].[Id] = [t9].[Id]
) AS [t10] ON [t].[Id] = [t10].[Id]
LEFT JOIN (
    SELECT [o11].[Id], [o11].[LeafAAddress_Country_Name], [o11].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o11]
    WHERE [o11].[LeafAAddress_Country_PlanetId] IS NOT NULL
) AS [t11] ON [t10].[Id] = [t11].[Id]
LEFT JOIN [Order] AS [o12] ON [t].[Id] = [o12].[ClientId]
ORDER BY [t].[Id], [o12].[ClientId], [o12].[Id]");
        }

        public override async Task Where_owned_collection_navigation_ToList_Count(bool async)
        {
            await base.Where_owned_collection_navigation_ToList_Count(async);

            AssertSql(
                @"SELECT [o].[Id], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA') AND ((
    SELECT COUNT(*)
    FROM [Order] AS [o1]
    WHERE [o].[Id] = [o1].[ClientId]) = 0)
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Where_collection_navigation_ToArray_Count(bool async)
        {
            await base.Where_collection_navigation_ToArray_Count(async);

            AssertSql(
                @"SELECT [o].[Id], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Order] AS [o0] ON ([o].[Id] = [o0].[ClientId]) AND ([o].[Id] = [o0].[ClientId])
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA') AND ((
    SELECT COUNT(*)
    FROM [Order] AS [o1]
    WHERE [o].[Id] = [o1].[ClientId]) = 0)
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Where_collection_navigation_AsEnumerable_Count(bool async)
        {
            await base.Where_collection_navigation_AsEnumerable_Count(async);

            AssertSql(
                @"SELECT [o].[Id], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA') AND ((
    SELECT COUNT(*)
    FROM [Order] AS [o1]
    WHERE [o].[Id] = [o1].[ClientId]) = 0)
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Where_collection_navigation_ToList_Count_member(bool async)
        {
            await base.Where_collection_navigation_ToList_Count_member(async);

            AssertSql(
                @"SELECT [o].[Id], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA') AND ((
    SELECT COUNT(*)
    FROM [Order] AS [o1]
    WHERE [o].[Id] = [o1].[ClientId]) = 0)
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Where_collection_navigation_ToArray_Length_member(bool async)
        {
            await base.Where_collection_navigation_ToArray_Length_member(async);

            AssertSql(" ");
        }

        public override async Task Can_query_on_indexer_properties(bool async)
        {
            await base.Can_query_on_indexer_properties(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [t].[Id], [t].[PersonAddress_AddressLine], [t].[PersonAddress_ZipCode], [t0].[Id], [t0].[PersonAddress_Country_Name], [t0].[PersonAddress_Country_PlanetId], [t2].[Id], [t3].[Id], [t3].[BranchAddress_Country_Name], [t3].[BranchAddress_Country_PlanetId], [t5].[Id], [t6].[Id], [t6].[LeafBAddress_Country_Name], [t6].[LeafBAddress_Country_PlanetId], [t8].[Id], [t9].[Id], [t9].[LeafAAddress_Country_Name], [t9].[LeafAAddress_Country_PlanetId], [o11].[ClientId], [o11].[Id], [o11].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [o2].[Id], [t1].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o2]
    INNER JOIN (
        SELECT [o3].[Id], [o3].[Discriminator], [o3].[Name]
        FROM [OwnedPerson] AS [o3]
        WHERE [o3].[Discriminator] IN (N'Branch', N'LeafA')
    ) AS [t1] ON [o2].[Id] = [t1].[Id]
) AS [t2] ON [o].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [o4].[Id], [o4].[BranchAddress_Country_Name], [o4].[BranchAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o4]
    WHERE [o4].[BranchAddress_Country_PlanetId] IS NOT NULL
) AS [t3] ON [t2].[Id] = [t3].[Id]
LEFT JOIN (
    SELECT [o5].[Id], [t4].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o5]
    INNER JOIN (
        SELECT [o6].[Id], [o6].[Discriminator], [o6].[Name]
        FROM [OwnedPerson] AS [o6]
        WHERE [o6].[Discriminator] = N'LeafB'
    ) AS [t4] ON [o5].[Id] = [t4].[Id]
) AS [t5] ON [o].[Id] = [t5].[Id]
LEFT JOIN (
    SELECT [o7].[Id], [o7].[LeafBAddress_Country_Name], [o7].[LeafBAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o7]
    WHERE [o7].[LeafBAddress_Country_PlanetId] IS NOT NULL
) AS [t6] ON [t5].[Id] = [t6].[Id]
LEFT JOIN (
    SELECT [o8].[Id], [t7].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o8]
    INNER JOIN (
        SELECT [o9].[Id], [o9].[Discriminator], [o9].[Name]
        FROM [OwnedPerson] AS [o9]
        WHERE [o9].[Discriminator] = N'LeafA'
    ) AS [t7] ON [o8].[Id] = [t7].[Id]
) AS [t8] ON [o].[Id] = [t8].[Id]
LEFT JOIN (
    SELECT [o10].[Id], [o10].[LeafAAddress_Country_Name], [o10].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o10]
    WHERE [o10].[LeafAAddress_Country_PlanetId] IS NOT NULL
) AS [t9] ON [t8].[Id] = [t9].[Id]
LEFT JOIN [Order] AS [o11] ON [o].[Id] = [o11].[ClientId]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA') AND ([o].[Name] = N'Mona Cy')
ORDER BY [o].[Id], [o11].[ClientId], [o11].[Id]");
        }

        public override async Task Can_query_on_owned_indexer_properties(bool async)
        {
            await base.Can_query_on_owned_indexer_properties(async);

            AssertSql(
                @"SELECT [o].[Name]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA') AND ([t].[PersonAddress_ZipCode] = 38654)");
        }

        public override async Task Can_query_on_indexer_property_when_property_name_from_closure(bool async)
        {
            await base.Can_query_on_indexer_property_when_property_name_from_closure(async);

            AssertSql(
                @"SELECT [o].[Name]
FROM [OwnedPerson] AS [o]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA') AND ([o].[Name] = N'Mona Cy')");
        }

        public override async Task Can_project_indexer_properties(bool async)
        {
            await base.Can_project_indexer_properties(async);

            AssertSql(
                @"SELECT [o].[Name]
FROM [OwnedPerson] AS [o]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')");
        }

        public override async Task Can_project_owned_indexer_properties(bool async)
        {
            await base.Can_project_owned_indexer_properties(async);

            AssertSql(
                @"SELECT [t].[PersonAddress_AddressLine]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')");
        }

        public override async Task Can_project_indexer_properties_converted(bool async)
        {
            await base.Can_project_indexer_properties_converted(async);

            AssertSql(
                @"SELECT [o].[Name]
FROM [OwnedPerson] AS [o]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')");
        }

        public override async Task Can_project_owned_indexer_properties_converted(bool async)
        {
            await base.Can_project_owned_indexer_properties_converted(async);

            AssertSql(
                @"SELECT [t].[PersonAddress_AddressLine]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')");
        }

        public override async Task Can_OrderBy_indexer_properties(bool async)
        {
            await base.Can_OrderBy_indexer_properties(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [t].[Id], [t].[PersonAddress_AddressLine], [t].[PersonAddress_ZipCode], [t0].[Id], [t0].[PersonAddress_Country_Name], [t0].[PersonAddress_Country_PlanetId], [t2].[Id], [t3].[Id], [t3].[BranchAddress_Country_Name], [t3].[BranchAddress_Country_PlanetId], [t5].[Id], [t6].[Id], [t6].[LeafBAddress_Country_Name], [t6].[LeafBAddress_Country_PlanetId], [t8].[Id], [t9].[Id], [t9].[LeafAAddress_Country_Name], [t9].[LeafAAddress_Country_PlanetId], [o11].[ClientId], [o11].[Id], [o11].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o1]
    WHERE [o1].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [o2].[Id], [t1].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o2]
    INNER JOIN (
        SELECT [o3].[Id], [o3].[Discriminator], [o3].[Name]
        FROM [OwnedPerson] AS [o3]
        WHERE [o3].[Discriminator] IN (N'Branch', N'LeafA')
    ) AS [t1] ON [o2].[Id] = [t1].[Id]
) AS [t2] ON [o].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [o4].[Id], [o4].[BranchAddress_Country_Name], [o4].[BranchAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o4]
    WHERE [o4].[BranchAddress_Country_PlanetId] IS NOT NULL
) AS [t3] ON [t2].[Id] = [t3].[Id]
LEFT JOIN (
    SELECT [o5].[Id], [t4].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o5]
    INNER JOIN (
        SELECT [o6].[Id], [o6].[Discriminator], [o6].[Name]
        FROM [OwnedPerson] AS [o6]
        WHERE [o6].[Discriminator] = N'LeafB'
    ) AS [t4] ON [o5].[Id] = [t4].[Id]
) AS [t5] ON [o].[Id] = [t5].[Id]
LEFT JOIN (
    SELECT [o7].[Id], [o7].[LeafBAddress_Country_Name], [o7].[LeafBAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o7]
    WHERE [o7].[LeafBAddress_Country_PlanetId] IS NOT NULL
) AS [t6] ON [t5].[Id] = [t6].[Id]
LEFT JOIN (
    SELECT [o8].[Id], [t7].[Id] AS [Id0]
    FROM [OwnedPerson] AS [o8]
    INNER JOIN (
        SELECT [o9].[Id], [o9].[Discriminator], [o9].[Name]
        FROM [OwnedPerson] AS [o9]
        WHERE [o9].[Discriminator] = N'LeafA'
    ) AS [t7] ON [o8].[Id] = [t7].[Id]
) AS [t8] ON [o].[Id] = [t8].[Id]
LEFT JOIN (
    SELECT [o10].[Id], [o10].[LeafAAddress_Country_Name], [o10].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o10]
    WHERE [o10].[LeafAAddress_Country_PlanetId] IS NOT NULL
) AS [t9] ON [t8].[Id] = [t9].[Id]
LEFT JOIN [Order] AS [o11] ON [o].[Id] = [o11].[ClientId]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
ORDER BY [o].[Name], [o].[Id], [o11].[ClientId], [o11].[Id]");
        }

        public override async Task Can_OrderBy_indexer_properties_converted(bool async)
        {
            await base.Can_OrderBy_indexer_properties_converted(async);

            AssertSql(
                @"SELECT [o].[Name]
FROM [OwnedPerson] AS [o]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
ORDER BY [o].[Name], [o].[Id]");
        }

        public override async Task Can_OrderBy_owned_indexer_properties(bool async)
        {
            await base.Can_OrderBy_owned_indexer_properties(async);

            AssertSql(
                @"SELECT [o].[Name]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
ORDER BY [t].[PersonAddress_ZipCode], [o].[Id]");
        }

        public override async Task Can_OrderBy_owened_indexer_properties_converted(bool async)
        {
            await base.Can_OrderBy_owened_indexer_properties_converted(async);

            AssertSql(
                @"SELECT [o].[Name]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
ORDER BY [t].[PersonAddress_ZipCode], [o].[Id]");
        }

        public override async Task Can_group_by_indexer_property(bool isAsync)
        {
            await base.Can_group_by_indexer_property(isAsync);

            AssertSql(
                @"SELECT COUNT(*)
FROM [OwnedPerson] AS [o]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
GROUP BY [o].[Name]");
        }

        public override async Task Can_group_by_converted_indexer_property(bool isAsync)
        {
            await base.Can_group_by_converted_indexer_property(isAsync);

            AssertSql(
                @"SELECT COUNT(*)
FROM [OwnedPerson] AS [o]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
GROUP BY [o].[Name]");
        }

        public override async Task Can_group_by_owned_indexer_property(bool isAsync)
        {
            await base.Can_group_by_owned_indexer_property(isAsync);

            AssertSql(
                @"SELECT COUNT(*)
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
GROUP BY [t].[PersonAddress_ZipCode]");
        }

        public override async Task Can_group_by_converted_owned_indexer_property(bool isAsync)
        {
            await base.Can_group_by_converted_owned_indexer_property(isAsync);

            AssertSql(
                @"SELECT COUNT(*)
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
GROUP BY [t].[PersonAddress_ZipCode]");
        }

        public override async Task Can_join_on_indexer_property_on_query(bool isAsync)
        {
            await base.Can_join_on_indexer_property_on_query(isAsync);

            AssertSql(
                @"SELECT [o].[Id], [t2].[PersonAddress_Country_Name] AS [Name]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
INNER JOIN (
    SELECT [o1].[Id], [o1].[Discriminator], [o1].[Name], [t0].[Id] AS [Id0], [t0].[PersonAddress_AddressLine], [t0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o1]
    LEFT JOIN (
        SELECT [o2].[Id], [o2].[PersonAddress_AddressLine], [o2].[PersonAddress_ZipCode]
        FROM [OwnedPerson] AS [o2]
        WHERE [o2].[PersonAddress_ZipCode] IS NOT NULL
    ) AS [t0] ON [o1].[Id] = [t0].[Id]
    WHERE [o1].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
) AS [t1] ON [t].[PersonAddress_ZipCode] = [t1].[PersonAddress_ZipCode]
LEFT JOIN (
    SELECT [o3].[Id], [o3].[PersonAddress_Country_Name], [o3].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o3]
    WHERE [o3].[PersonAddress_Country_PlanetId] IS NOT NULL
) AS [t2] ON [t1].[Id0] = [t2].[Id]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')");
        }

        public override async Task Projecting_indexer_property_ignores_include(bool isAsync)
        {
            await base.Projecting_indexer_property_ignores_include(isAsync);

            AssertSql(
                @"SELECT [t].[PersonAddress_ZipCode] AS [Nation]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')");
        }

        public override async Task Projecting_indexer_property_ignores_include_converted(bool isAsync)
        {
            await base.Projecting_indexer_property_ignores_include_converted(isAsync);

            AssertSql(
                @"SELECT [t].[PersonAddress_ZipCode] AS [Nation]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_ZipCode]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')");
        }

        public override async Task Indexer_property_is_pushdown_into_subquery(bool isAsync)
        {
            await base.Indexer_property_is_pushdown_into_subquery(isAsync);

            AssertSql(
                @"SELECT [o].[Name]
FROM [OwnedPerson] AS [o]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA') AND ((
    SELECT TOP(1) [o0].[Name]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA') AND ([o0].[Id] = [o].[Id])) = N'Mona Cy')");
        }

        public override async Task Can_query_indexer_property_on_owned_collection(bool isAsync)
        {
            await base.Can_query_indexer_property_on_owned_collection(isAsync);

            AssertSql(
                @"SELECT [o].[Name]
FROM [OwnedPerson] AS [o]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA') AND ((
    SELECT COUNT(*)
    FROM [Order] AS [o0]
    WHERE ([o].[Id] = [o0].[ClientId]) AND (DATEPART(year, [o0].[OrderDate]) = 2018)) = 1)");
        }

        private void AssertSql(params string[] expected)
             => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class OwnedQuerySqlServerFixture : RelationalOwnedQueryFixture
        {
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;

            protected override bool CanExecuteQueryString => true;
        }
    }
}
