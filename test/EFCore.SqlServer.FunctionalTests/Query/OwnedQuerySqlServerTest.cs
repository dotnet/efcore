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
            Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void Query_with_owned_entity_equality_operator()
        {
            base.Query_with_owned_entity_equality_operator();

            AssertSql(
                @"SELECT [t].[Id], [t].[Discriminator], [o0].[Id], [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId], [o2].[Id], [o3].[Id], [o3].[BranchAddress_Country_Name], [o3].[BranchAddress_Country_PlanetId], [o4].[Id], [o5].[Id], [o5].[LeafAAddress_Country_Name], [o5].[LeafAAddress_Country_PlanetId]
FROM (
    SELECT [o].[Id], [o].[Discriminator]
    FROM [OwnedPerson] AS [o]
    WHERE [o].[Discriminator] = N'LeafA'
) AS [t]
CROSS JOIN (
    SELECT [o6].[Id], [o6].[Discriminator]
    FROM [OwnedPerson] AS [o6]
    WHERE [o6].[Discriminator] = N'LeafB'
) AS [t0]
LEFT JOIN [OwnedPerson] AS [o0] ON [t].[Id] = [o0].[Id]
LEFT JOIN [OwnedPerson] AS [o1] ON [o0].[Id] = [o1].[Id]
LEFT JOIN [OwnedPerson] AS [o2] ON [t].[Id] = [o2].[Id]
LEFT JOIN [OwnedPerson] AS [o3] ON [o2].[Id] = [o3].[Id]
LEFT JOIN [OwnedPerson] AS [o4] ON [t].[Id] = [o4].[Id]
LEFT JOIN [OwnedPerson] AS [o5] ON [o4].[Id] = [o5].[Id]
WHERE CAST(0 AS bit) = CAST(1 AS bit)");
        }

        public override void Query_for_base_type_loads_all_owned_navs()
        {
            base.Query_for_base_type_loads_all_owned_navs();

            // See issue #10067
            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o0].[Id], [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId], [o2].[Id], [o3].[Id], [o3].[BranchAddress_Country_Name], [o3].[BranchAddress_Country_PlanetId], [o4].[Id], [o5].[Id], [o5].[LeafBAddress_Country_Name], [o5].[LeafBAddress_Country_PlanetId], [o6].[Id], [o7].[Id], [o7].[LeafAAddress_Country_Name], [o7].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN [OwnedPerson] AS [o0] ON [o].[Id] = [o0].[Id]
LEFT JOIN [OwnedPerson] AS [o1] ON [o0].[Id] = [o1].[Id]
LEFT JOIN [OwnedPerson] AS [o2] ON [o].[Id] = [o2].[Id]
LEFT JOIN [OwnedPerson] AS [o3] ON [o2].[Id] = [o3].[Id]
LEFT JOIN [OwnedPerson] AS [o4] ON [o].[Id] = [o4].[Id]
LEFT JOIN [OwnedPerson] AS [o5] ON [o4].[Id] = [o5].[Id]
LEFT JOIN [OwnedPerson] AS [o6] ON [o].[Id] = [o6].[Id]
LEFT JOIN [OwnedPerson] AS [o7] ON [o6].[Id] = [o7].[Id]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')");
        }

        public override void No_ignored_include_warning_when_implicit_load()
        {
            base.No_ignored_include_warning_when_implicit_load();

            AssertSql(
                @"SELECT COUNT(*)
FROM [OwnedPerson] AS [o]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')");
        }

        public override void Query_for_branch_type_loads_all_owned_navs()
        {
            base.Query_for_branch_type_loads_all_owned_navs();

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o0].[Id], [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId], [o2].[Id], [o3].[Id], [o3].[BranchAddress_Country_Name], [o3].[BranchAddress_Country_PlanetId], [o4].[Id], [o5].[Id], [o5].[LeafAAddress_Country_Name], [o5].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN [OwnedPerson] AS [o0] ON [o].[Id] = [o0].[Id]
LEFT JOIN [OwnedPerson] AS [o1] ON [o0].[Id] = [o1].[Id]
LEFT JOIN [OwnedPerson] AS [o2] ON [o].[Id] = [o2].[Id]
LEFT JOIN [OwnedPerson] AS [o3] ON [o2].[Id] = [o3].[Id]
LEFT JOIN [OwnedPerson] AS [o4] ON [o].[Id] = [o4].[Id]
LEFT JOIN [OwnedPerson] AS [o5] ON [o4].[Id] = [o5].[Id]
WHERE [o].[Discriminator] IN (N'Branch', N'LeafA')");
        }

        public override void Query_for_leaf_type_loads_all_owned_navs()
        {
            base.Query_for_leaf_type_loads_all_owned_navs();

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o0].[Id], [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId], [o2].[Id], [o3].[Id], [o3].[BranchAddress_Country_Name], [o3].[BranchAddress_Country_PlanetId], [o4].[Id], [o5].[Id], [o5].[LeafAAddress_Country_Name], [o5].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN [OwnedPerson] AS [o0] ON [o].[Id] = [o0].[Id]
LEFT JOIN [OwnedPerson] AS [o1] ON [o0].[Id] = [o1].[Id]
LEFT JOIN [OwnedPerson] AS [o2] ON [o].[Id] = [o2].[Id]
LEFT JOIN [OwnedPerson] AS [o3] ON [o2].[Id] = [o3].[Id]
LEFT JOIN [OwnedPerson] AS [o4] ON [o].[Id] = [o4].[Id]
LEFT JOIN [OwnedPerson] AS [o5] ON [o4].[Id] = [o5].[Id]
WHERE [o].[Discriminator] = N'LeafA'");
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

SELECT [t0].[Id], [t0].[Discriminator], [o0].[Id], [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId], [o2].[Id], [o3].[Id], [o3].[BranchAddress_Country_Name], [o3].[BranchAddress_Country_PlanetId], [o4].[Id], [o5].[Id], [o5].[LeafBAddress_Country_Name], [o5].[LeafBAddress_Country_PlanetId], [o6].[Id], [o7].[Id], [o7].[LeafAAddress_Country_Name], [o7].[LeafAAddress_Country_PlanetId]
FROM (
    SELECT TOP(@__p_0) [t].[Id], [t].[Discriminator]
    FROM (
        SELECT DISTINCT [o].[Id], [o].[Discriminator]
        FROM [OwnedPerson] AS [o]
        WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
    ) AS [t]
    ORDER BY [t].[Id]
) AS [t0]
LEFT JOIN [OwnedPerson] AS [o0] ON [t0].[Id] = [o0].[Id]
LEFT JOIN [OwnedPerson] AS [o8] ON [t0].[Id] = [o8].[Id]
LEFT JOIN [OwnedPerson] AS [o1] ON [o8].[Id] = [o1].[Id]
LEFT JOIN [OwnedPerson] AS [o2] ON [t0].[Id] = [o2].[Id]
LEFT JOIN [OwnedPerson] AS [o3] ON [o2].[Id] = [o3].[Id]
LEFT JOIN [OwnedPerson] AS [o4] ON [t0].[Id] = [o4].[Id]
LEFT JOIN [OwnedPerson] AS [o5] ON [o4].[Id] = [o5].[Id]
LEFT JOIN [OwnedPerson] AS [o6] ON [t0].[Id] = [o6].[Id]
LEFT JOIN [OwnedPerson] AS [o7] ON [o6].[Id] = [o7].[Id]
ORDER BY [t0].[Id]");
        }

        public override void Navigation_rewrite_on_owned_reference_projecting_scalar()
        {
            base.Navigation_rewrite_on_owned_reference_projecting_scalar();

            AssertSql(
                @"SELECT [o].[PersonAddress_Country_Name]
FROM [OwnedPerson] AS [o0]
LEFT JOIN [OwnedPerson] AS [o1] ON [o0].[Id] = [o1].[Id]
LEFT JOIN [OwnedPerson] AS [o] ON [o1].[Id] = [o].[Id]
WHERE [o0].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA') AND (([o].[PersonAddress_Country_Name] = N'USA') AND [o].[PersonAddress_Country_Name] IS NOT NULL)");
        }

        public override void Navigation_rewrite_on_owned_reference_projecting_entity()
        {
            base.Navigation_rewrite_on_owned_reference_projecting_entity();

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o0].[Id], [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId], [o2].[Id], [o3].[Id], [o3].[BranchAddress_Country_Name], [o3].[BranchAddress_Country_PlanetId], [o4].[Id], [o5].[Id], [o5].[LeafBAddress_Country_Name], [o5].[LeafBAddress_Country_PlanetId], [o6].[Id], [o7].[Id], [o7].[LeafAAddress_Country_Name], [o7].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN [OwnedPerson] AS [o0] ON [o].[Id] = [o0].[Id]
LEFT JOIN [OwnedPerson] AS [o1] ON [o0].[Id] = [o1].[Id]
LEFT JOIN [OwnedPerson] AS [o2] ON [o].[Id] = [o2].[Id]
LEFT JOIN [OwnedPerson] AS [o3] ON [o2].[Id] = [o3].[Id]
LEFT JOIN [OwnedPerson] AS [o4] ON [o].[Id] = [o4].[Id]
LEFT JOIN [OwnedPerson] AS [o5] ON [o4].[Id] = [o5].[Id]
LEFT JOIN [OwnedPerson] AS [o6] ON [o].[Id] = [o6].[Id]
LEFT JOIN [OwnedPerson] AS [o7] ON [o6].[Id] = [o7].[Id]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA') AND (([o1].[PersonAddress_Country_Name] = N'USA') AND [o1].[PersonAddress_Country_Name] IS NOT NULL)");
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

        public override void Navigation_rewrite_on_owned_collection_with_composition()
        {
            base.Navigation_rewrite_on_owned_collection_with_composition();

            AssertSql(
                @"");
        }

        public override void Navigation_rewrite_on_owned_collection_with_composition_complex()
        {
            base.Navigation_rewrite_on_owned_collection_with_composition_complex();

            AssertSql(
                @"");
        }

        public override void SelectMany_on_owned_collection()
        {
            base.SelectMany_on_owned_collection();

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
                @"SELECT [p].[Id], [p].[StarId]
FROM [OwnedPerson] AS [o]
LEFT JOIN [OwnedPerson] AS [o0] ON [o].[Id] = [o0].[Id]
LEFT JOIN [OwnedPerson] AS [o1] ON [o0].[Id] = [o1].[Id]
LEFT JOIN [Planet] AS [p] ON [o1].[PersonAddress_Country_PlanetId] = [p].[Id]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')");
        }

        public override void Filter_owned_entity_chained_with_regular_entity_followed_by_projecting_owned_collection()
        {
            base.Filter_owned_entity_chained_with_regular_entity_followed_by_projecting_owned_collection();

            AssertSql(
                @"");
        }

        public override void Project_multiple_owned_navigations()
        {
            base.Project_multiple_owned_navigations();

            AssertSql(
                @"");
        }

        public override void Project_multiple_owned_navigations_with_expansion_on_owned_collections()
        {
            base.Project_multiple_owned_navigations_with_expansion_on_owned_collections();

            AssertSql(
                @"");
        }

        public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_filter()
        {
            base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_filter();

            AssertSql(
                @"");
        }

        public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property()
        {
            base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property();

            AssertSql(
                @"SELECT [p].[Id]
FROM [OwnedPerson] AS [o]
LEFT JOIN [OwnedPerson] AS [o0] ON [o].[Id] = [o0].[Id]
LEFT JOIN [OwnedPerson] AS [o1] ON [o0].[Id] = [o1].[Id]
LEFT JOIN [Planet] AS [p] ON [o1].[PersonAddress_Country_PlanetId] = [p].[Id]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')");
        }

        public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection()
        {
            base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection();

            AssertSql(
                @"SELECT [o].[Id], [m].[Id], [m].[Diameter], [m].[PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN [OwnedPerson] AS [o0] ON [o].[Id] = [o0].[Id]
LEFT JOIN [OwnedPerson] AS [o1] ON [o0].[Id] = [o1].[Id]
LEFT JOIN [Planet] AS [p] ON [o1].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Moon] AS [m] ON [p].[Id] = [m].[PlanetId]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
ORDER BY [o].[Id], [m].[Id]");
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
WHERE ([p].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson') AND [p.PersonAddress].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')) AND ([p.PersonAddress.Country].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson') AND [p.PersonAddress.Country].[PersonAddress_Country_PlanetId] IS NOT NULL)");
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
WHERE ([p].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson') AND [p.PersonAddress].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')) AND ([p.PersonAddress.Country].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson') AND [p.PersonAddress.Country].[PersonAddress_Country_PlanetId] IS NOT NULL)");
        }

        public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference()
        {
            base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference();

            AssertSql(
                @"SELECT [s].[Id], [s].[Name]
FROM [OwnedPerson] AS [o]
LEFT JOIN [OwnedPerson] AS [o0] ON [o].[Id] = [o0].[Id]
LEFT JOIN [OwnedPerson] AS [o1] ON [o0].[Id] = [o1].[Id]
LEFT JOIN [Planet] AS [p] ON [o1].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Star] AS [s] ON [p].[StarId] = [s].[Id]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')");
        }

        public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar()
        {
            base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar();

            AssertSql(
                @"SELECT [s].[Name]
FROM [OwnedPerson] AS [o]
LEFT JOIN [OwnedPerson] AS [o0] ON [o].[Id] = [o0].[Id]
LEFT JOIN [OwnedPerson] AS [o1] ON [o0].[Id] = [o1].[Id]
LEFT JOIN [Planet] AS [p] ON [o1].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Star] AS [s] ON [p].[StarId] = [s].[Id]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')");
        }

        public override void
            Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection()
        {
            base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection();

            AssertSql(
                @"SELECT [s].[Id], [s].[Name]
FROM [OwnedPerson] AS [o]
LEFT JOIN [OwnedPerson] AS [o0] ON [o].[Id] = [o0].[Id]
LEFT JOIN [OwnedPerson] AS [o1] ON [o0].[Id] = [o1].[Id]
LEFT JOIN [Planet] AS [p] ON [o1].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Star] AS [s] ON [p].[StarId] = [s].[Id]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA') AND (([s].[Name] = N'Sol') AND [s].[Name] IS NOT NULL)");

        }

        public override void Query_with_OfType_eagerly_loads_correct_owned_navigations()
        {
            base.Query_with_OfType_eagerly_loads_correct_owned_navigations();

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o0].[Id], [o1].[Id], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId], [o2].[Id], [o3].[Id], [o3].[BranchAddress_Country_Name], [o3].[BranchAddress_Country_PlanetId], [o4].[Id], [o5].[Id], [o5].[LeafAAddress_Country_Name], [o5].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN [OwnedPerson] AS [o0] ON [o].[Id] = [o0].[Id]
LEFT JOIN [OwnedPerson] AS [o1] ON [o0].[Id] = [o1].[Id]
LEFT JOIN [OwnedPerson] AS [o2] ON [o].[Id] = [o2].[Id]
LEFT JOIN [OwnedPerson] AS [o3] ON [o2].[Id] = [o3].[Id]
LEFT JOIN [OwnedPerson] AS [o4] ON [o].[Id] = [o4].[Id]
LEFT JOIN [OwnedPerson] AS [o5] ON [o4].[Id] = [o5].[Id]
WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA') AND ([o].[Discriminator] = N'LeafA')");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class OwnedQuerySqlServerFixture : RelationalOwnedQueryFixture
        {
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
        }
    }
}
