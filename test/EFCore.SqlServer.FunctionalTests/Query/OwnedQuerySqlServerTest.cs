// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class OwnedQuerySqlServerTest : OwnedQueryRelationalTestBase<OwnedQuerySqlServerTest.OwnedQuerySqlServerFixture>
    {
        public OwnedQuerySqlServerTest(OwnedQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override bool CanExecuteQueryString
            => true;

        public override async Task Query_with_owned_entity_equality_operator(bool async)
        {
            await base.Query_with_owned_entity_equality_operator(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId], [t].[Id], [o1].[ClientId], [o1].[Id], [o1].[OrderDate]
FROM [OwnedPerson] AS [o]
CROSS JOIN (
    SELECT [o0].[Id]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[Discriminator] = N'LeafB'
) AS [t]
LEFT JOIN [Order] AS [o1] ON [o].[Id] = [o1].[ClientId]
WHERE 0 = 1
ORDER BY [o].[Id], [t].[Id], [o1].[ClientId], [o1].[Id]");
        }

        public override async Task Query_for_base_type_loads_all_owned_navs(bool async)
        {
            await base.Query_for_base_type_loads_all_owned_navs(async);

            // See issue #10067
            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task No_ignored_include_warning_when_implicit_load(bool async)
        {
            await base.No_ignored_include_warning_when_implicit_load(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM [OwnedPerson] AS [o]");
        }

        public override async Task Query_for_branch_type_loads_all_owned_navs(bool async)
        {
            await base.Query_for_branch_type_loads_all_owned_navs(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE [o].[Discriminator] IN (N'Branch', N'LeafA')
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Query_for_branch_type_loads_all_owned_navs_tracking(bool async)
        {
            await base.Query_for_branch_type_loads_all_owned_navs_tracking(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE [o].[Discriminator] IN (N'Branch', N'LeafA')
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Query_for_leaf_type_loads_all_owned_navs(bool async)
        {
            await base.Query_for_leaf_type_loads_all_owned_navs(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE [o].[Discriminator] = N'LeafA'
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Query_when_subquery(bool async)
        {
            await base.Query_when_subquery(async);

            AssertSql(
                @"@__p_0='5'

SELECT [t0].[Id], [t0].[Discriminator], [t0].[Name], [t0].[PersonAddress_AddressLine], [t0].[PersonAddress_PlaceType], [t0].[PersonAddress_ZipCode], [t0].[PersonAddress_Country_Name], [t0].[PersonAddress_Country_PlanetId], [t0].[BranchAddress_BranchName], [t0].[BranchAddress_PlaceType], [t0].[BranchAddress_Country_Name], [t0].[BranchAddress_Country_PlanetId], [t0].[LeafBAddress_LeafBType], [t0].[LeafBAddress_PlaceType], [t0].[LeafBAddress_Country_Name], [t0].[LeafBAddress_Country_PlanetId], [t0].[LeafAAddress_LeafType], [t0].[LeafAAddress_PlaceType], [t0].[LeafAAddress_Country_Name], [t0].[LeafAAddress_Country_PlanetId], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [t].[Id], [t].[Discriminator], [t].[Name], [t].[PersonAddress_AddressLine], [t].[PersonAddress_PlaceType], [t].[PersonAddress_ZipCode], [t].[PersonAddress_Country_Name], [t].[PersonAddress_Country_PlanetId], [t].[BranchAddress_BranchName], [t].[BranchAddress_PlaceType], [t].[BranchAddress_Country_Name], [t].[BranchAddress_Country_PlanetId], [t].[LeafBAddress_LeafBType], [t].[LeafBAddress_PlaceType], [t].[LeafBAddress_Country_Name], [t].[LeafBAddress_Country_PlanetId], [t].[LeafAAddress_LeafType], [t].[LeafAAddress_PlaceType], [t].[LeafAAddress_Country_Name], [t].[LeafAAddress_Country_PlanetId]
    FROM (
        SELECT DISTINCT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
        FROM [OwnedPerson] AS [o]
    ) AS [t]
    ORDER BY [t].[Id]
) AS [t0]
LEFT JOIN [Order] AS [o0] ON [t0].[Id] = [o0].[ClientId]
ORDER BY [t0].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Navigation_rewrite_on_owned_reference_projecting_scalar(bool async)
        {
            await base.Navigation_rewrite_on_owned_reference_projecting_scalar(async);

            AssertSql(
                @"SELECT [o].[PersonAddress_Country_Name]
FROM [OwnedPerson] AS [o]
WHERE [o].[PersonAddress_Country_Name] = N'USA'");
        }

        public override async Task Navigation_rewrite_on_owned_reference_projecting_entity(bool async)
        {
            await base.Navigation_rewrite_on_owned_reference_projecting_entity(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE [o].[PersonAddress_Country_Name] = N'USA'
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Navigation_rewrite_on_owned_collection(bool async)
        {
            await base.Navigation_rewrite_on_owned_collection(async);

            AssertSql(
                @"SELECT [o].[Id], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE (
    SELECT COUNT(*)
    FROM [Order] AS [o1]
    WHERE [o].[Id] = [o1].[ClientId]) > 0
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Navigation_rewrite_on_owned_collection_with_composition(bool async)
        {
            await base.Navigation_rewrite_on_owned_collection_with_composition(async);

            AssertSql(
                @"SELECT COALESCE((
    SELECT TOP(1) CASE
        WHEN [o].[Id] <> 42 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    FROM [Order] AS [o]
    WHERE [o0].[Id] = [o].[ClientId]
    ORDER BY [o].[Id]), CAST(0 AS bit))
FROM [OwnedPerson] AS [o0]
ORDER BY [o0].[Id]");
        }

        public override async Task Navigation_rewrite_on_owned_collection_with_composition_complex(bool async)
        {
            await base.Navigation_rewrite_on_owned_collection_with_composition_complex(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [o0].[PersonAddress_Country_Name]
    FROM [Order] AS [o]
    LEFT JOIN [OwnedPerson] AS [o0] ON [o].[ClientId] = [o0].[Id]
    WHERE [o1].[Id] = [o].[ClientId]
    ORDER BY [o].[Id])
FROM [OwnedPerson] AS [o1]");
        }

        public override async Task SelectMany_on_owned_collection(bool async)
        {
            await base.SelectMany_on_owned_collection(async);

            AssertSql(
                @"SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
INNER JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]");
        }

        public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity(bool async)
        {
            await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity(async);

            AssertSql(
                @"SELECT [p].[Id], [p].[StarId]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]");
        }

        public override async Task Filter_owned_entity_chained_with_regular_entity_followed_by_projecting_owned_collection(bool async)
        {
            await base.Filter_owned_entity_chained_with_regular_entity_followed_by_projecting_owned_collection(async);

            AssertSql(
                @"SELECT [o].[Id], [p].[Id], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE ([p].[Id] <> 42) OR [p].[Id] IS NULL
ORDER BY [o].[Id], [p].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Project_multiple_owned_navigations(bool async)
        {
            await base.Project_multiple_owned_navigations(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [p].[Id], [p].[StarId], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
ORDER BY [o].[Id], [p].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Project_multiple_owned_navigations_with_expansion_on_owned_collections(bool async)
        {
            await base.Project_multiple_owned_navigations_with_expansion_on_owned_collections(async);

            AssertSql(
                @"SELECT (
    SELECT COUNT(*)
    FROM [Order] AS [o]
    LEFT JOIN [OwnedPerson] AS [o0] ON [o].[ClientId] = [o0].[Id]
    LEFT JOIN [Planet] AS [p] ON [o0].[PersonAddress_Country_PlanetId] = [p].[Id]
    LEFT JOIN [Star] AS [s] ON [p].[StarId] = [s].[Id]
    WHERE ([o1].[Id] = [o].[ClientId]) AND (([s].[Id] <> 42) OR [s].[Id] IS NULL)) AS [Count], [p0].[Id], [p0].[StarId]
FROM [OwnedPerson] AS [o1]
LEFT JOIN [Planet] AS [p0] ON [o1].[PersonAddress_Country_PlanetId] = [p0].[Id]
ORDER BY [o1].[Id]");
        }

        public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_filter(bool async)
        {
            await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_filter(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId], [p].[Id], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE ([p].[Id] <> 7) OR [p].[Id] IS NULL
ORDER BY [o].[Id], [p].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property(bool async)
        {
            await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property(async);

            AssertSql(
                @"SELECT [p].[Id]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]");
        }

        public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection(bool async)
        {
            await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection(async);

            AssertSql(
                @"SELECT [o].[Id], [p].[Id], [m].[Id], [m].[Diameter], [m].[PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Moon] AS [m] ON [p].[Id] = [m].[PlanetId]
ORDER BY [o].[Id], [p].[Id], [m].[Id]");
        }

        public override async Task SelectMany_on_owned_reference_followed_by_regular_entity_and_collection(bool async)
        {
            await base.SelectMany_on_owned_reference_followed_by_regular_entity_and_collection(async);

            AssertSql(
                @"SELECT [m].[Id], [m].[Diameter], [m].[PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
INNER JOIN [Moon] AS [m] ON [p].[Id] = [m].[PlanetId]");
        }

        public override async Task SelectMany_on_owned_reference_with_entity_in_between_ending_in_owned_collection(bool async)
        {
            await base.SelectMany_on_owned_reference_with_entity_in_between_ending_in_owned_collection(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[Name], [e].[StarId]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Star] AS [s] ON [p].[StarId] = [s].[Id]
INNER JOIN [Element] AS [e] ON [s].[Id] = [e].[StarId]");
        }

        public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference(bool async)
        {
            await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference(async);

            AssertSql(
                @"SELECT [s].[Id], [s].[Name], [o].[Id], [p].[Id], [e].[Id], [e].[Name], [e].[StarId]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Star] AS [s] ON [p].[StarId] = [s].[Id]
LEFT JOIN [Element] AS [e] ON [s].[Id] = [e].[StarId]
ORDER BY [o].[Id], [p].[Id], [s].[Id], [e].[Id]");
        }

        public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar(
            bool async)
        {
            await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar(async);

            AssertSql(
                @"SELECT [s].[Name]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Star] AS [s] ON [p].[StarId] = [s].[Id]");
        }

        public override async Task
            Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection(bool async)
        {
            await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection(
                async);

            AssertSql(
                @"SELECT [s].[Id], [s].[Name], [o].[Id], [p].[Id], [e].[Id], [e].[Name], [e].[StarId]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Star] AS [s] ON [p].[StarId] = [s].[Id]
LEFT JOIN [Element] AS [e] ON [s].[Id] = [e].[StarId]
WHERE [s].[Name] = N'Sol'
ORDER BY [o].[Id], [p].[Id], [s].[Id], [e].[Id]");
        }

        public override async Task Query_with_OfType_eagerly_loads_correct_owned_navigations(bool async)
        {
            await base.Query_with_OfType_eagerly_loads_correct_owned_navigations(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE [o].[Discriminator] = N'LeafA'
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Preserve_includes_when_applying_skip_take_after_anonymous_type_select(bool async)
        {
            await base.Preserve_includes_when_applying_skip_take_after_anonymous_type_select(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM [OwnedPerson] AS [o]",
                //
                @"@__p_1='0'
@__p_2='100'

SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t].[PersonAddress_AddressLine], [t].[PersonAddress_PlaceType], [t].[PersonAddress_ZipCode], [t].[PersonAddress_Country_Name], [t].[PersonAddress_Country_PlanetId], [t].[BranchAddress_BranchName], [t].[BranchAddress_PlaceType], [t].[BranchAddress_Country_Name], [t].[BranchAddress_Country_PlanetId], [t].[LeafBAddress_LeafBType], [t].[LeafBAddress_PlaceType], [t].[LeafBAddress_Country_Name], [t].[LeafBAddress_Country_PlanetId], [t].[LeafAAddress_LeafType], [t].[LeafAAddress_PlaceType], [t].[LeafAAddress_Country_Name], [t].[LeafAAddress_Country_PlanetId], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM (
    SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o]
    ORDER BY [o].[Id]
    OFFSET @__p_1 ROWS FETCH NEXT @__p_2 ROWS ONLY
) AS [t]
LEFT JOIN [Order] AS [o0] ON [t].[Id] = [o0].[ClientId]
ORDER BY [t].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Unmapped_property_projection_loads_owned_navigations(bool async)
        {
            await base.Unmapped_property_projection_loads_owned_navigations(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE [o].[Id] = 1
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Client_method_skip_loads_owned_navigations(bool async)
        {
            await base.Client_method_skip_loads_owned_navigations(async);

            AssertSql(
                @"@__p_0='1'

SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t].[PersonAddress_AddressLine], [t].[PersonAddress_PlaceType], [t].[PersonAddress_ZipCode], [t].[PersonAddress_Country_Name], [t].[PersonAddress_Country_PlanetId], [t].[BranchAddress_BranchName], [t].[BranchAddress_PlaceType], [t].[BranchAddress_Country_Name], [t].[BranchAddress_Country_PlanetId], [t].[LeafBAddress_LeafBType], [t].[LeafBAddress_PlaceType], [t].[LeafBAddress_Country_Name], [t].[LeafBAddress_Country_PlanetId], [t].[LeafAAddress_LeafType], [t].[LeafAAddress_PlaceType], [t].[LeafAAddress_Country_Name], [t].[LeafAAddress_Country_PlanetId], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM (
    SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o]
    ORDER BY [o].[Id]
    OFFSET @__p_0 ROWS
) AS [t]
LEFT JOIN [Order] AS [o0] ON [t].[Id] = [o0].[ClientId]
ORDER BY [t].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Client_method_take_loads_owned_navigations(bool async)
        {
            await base.Client_method_take_loads_owned_navigations(async);

            AssertSql(
                @"@__p_0='2'

SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t].[PersonAddress_AddressLine], [t].[PersonAddress_PlaceType], [t].[PersonAddress_ZipCode], [t].[PersonAddress_Country_Name], [t].[PersonAddress_Country_PlanetId], [t].[BranchAddress_BranchName], [t].[BranchAddress_PlaceType], [t].[BranchAddress_Country_Name], [t].[BranchAddress_Country_PlanetId], [t].[LeafBAddress_LeafBType], [t].[LeafBAddress_PlaceType], [t].[LeafBAddress_Country_Name], [t].[LeafBAddress_Country_PlanetId], [t].[LeafAAddress_LeafType], [t].[LeafAAddress_PlaceType], [t].[LeafAAddress_Country_Name], [t].[LeafAAddress_Country_PlanetId], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o]
    ORDER BY [o].[Id]
) AS [t]
LEFT JOIN [Order] AS [o0] ON [t].[Id] = [o0].[ClientId]
ORDER BY [t].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Client_method_skip_take_loads_owned_navigations(bool async)
        {
            await base.Client_method_skip_take_loads_owned_navigations(async);

            AssertSql(
                @"@__p_0='1'
@__p_1='2'

SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t].[PersonAddress_AddressLine], [t].[PersonAddress_PlaceType], [t].[PersonAddress_ZipCode], [t].[PersonAddress_Country_Name], [t].[PersonAddress_Country_PlanetId], [t].[BranchAddress_BranchName], [t].[BranchAddress_PlaceType], [t].[BranchAddress_Country_Name], [t].[BranchAddress_Country_PlanetId], [t].[LeafBAddress_LeafBType], [t].[LeafBAddress_PlaceType], [t].[LeafBAddress_Country_Name], [t].[LeafBAddress_Country_PlanetId], [t].[LeafAAddress_LeafType], [t].[LeafAAddress_PlaceType], [t].[LeafAAddress_Country_Name], [t].[LeafAAddress_Country_PlanetId], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM (
    SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o]
    ORDER BY [o].[Id]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
LEFT JOIN [Order] AS [o0] ON [t].[Id] = [o0].[ClientId]
ORDER BY [t].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Client_method_skip_loads_owned_navigations_variation_2(bool async)
        {
            await base.Client_method_skip_loads_owned_navigations_variation_2(async);

            AssertSql(
                @"@__p_0='1'

SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t].[PersonAddress_AddressLine], [t].[PersonAddress_PlaceType], [t].[PersonAddress_ZipCode], [t].[PersonAddress_Country_Name], [t].[PersonAddress_Country_PlanetId], [t].[BranchAddress_BranchName], [t].[BranchAddress_PlaceType], [t].[BranchAddress_Country_Name], [t].[BranchAddress_Country_PlanetId], [t].[LeafBAddress_LeafBType], [t].[LeafBAddress_PlaceType], [t].[LeafBAddress_Country_Name], [t].[LeafBAddress_Country_PlanetId], [t].[LeafAAddress_LeafType], [t].[LeafAAddress_PlaceType], [t].[LeafAAddress_Country_Name], [t].[LeafAAddress_Country_PlanetId], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM (
    SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o]
    ORDER BY [o].[Id]
    OFFSET @__p_0 ROWS
) AS [t]
LEFT JOIN [Order] AS [o0] ON [t].[Id] = [o0].[ClientId]
ORDER BY [t].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Client_method_take_loads_owned_navigations_variation_2(bool async)
        {
            await base.Client_method_take_loads_owned_navigations_variation_2(async);

            AssertSql(
                @"@__p_0='2'

SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t].[PersonAddress_AddressLine], [t].[PersonAddress_PlaceType], [t].[PersonAddress_ZipCode], [t].[PersonAddress_Country_Name], [t].[PersonAddress_Country_PlanetId], [t].[BranchAddress_BranchName], [t].[BranchAddress_PlaceType], [t].[BranchAddress_Country_Name], [t].[BranchAddress_Country_PlanetId], [t].[LeafBAddress_LeafBType], [t].[LeafBAddress_PlaceType], [t].[LeafBAddress_Country_Name], [t].[LeafBAddress_Country_PlanetId], [t].[LeafAAddress_LeafType], [t].[LeafAAddress_PlaceType], [t].[LeafAAddress_Country_Name], [t].[LeafAAddress_Country_PlanetId], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o]
    ORDER BY [o].[Id]
) AS [t]
LEFT JOIN [Order] AS [o0] ON [t].[Id] = [o0].[ClientId]
ORDER BY [t].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Client_method_skip_take_loads_owned_navigations_variation_2(bool async)
        {
            await base.Client_method_skip_take_loads_owned_navigations_variation_2(async);

            AssertSql(
                @"@__p_0='1'
@__p_1='2'

SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t].[PersonAddress_AddressLine], [t].[PersonAddress_PlaceType], [t].[PersonAddress_ZipCode], [t].[PersonAddress_Country_Name], [t].[PersonAddress_Country_PlanetId], [t].[BranchAddress_BranchName], [t].[BranchAddress_PlaceType], [t].[BranchAddress_Country_Name], [t].[BranchAddress_Country_PlanetId], [t].[LeafBAddress_LeafBType], [t].[LeafBAddress_PlaceType], [t].[LeafBAddress_Country_Name], [t].[LeafBAddress_Country_PlanetId], [t].[LeafAAddress_LeafType], [t].[LeafAAddress_PlaceType], [t].[LeafAAddress_Country_Name], [t].[LeafAAddress_Country_PlanetId], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM (
    SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o]
    ORDER BY [o].[Id]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
LEFT JOIN [Order] AS [o0] ON [t].[Id] = [o0].[ClientId]
ORDER BY [t].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Where_owned_collection_navigation_ToList_Count(bool async)
        {
            await base.Where_owned_collection_navigation_ToList_Count(async);

            AssertSql(
                @"SELECT [o].[Id], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE (
    SELECT COUNT(*)
    FROM [Order] AS [o1]
    WHERE [o].[Id] = [o1].[ClientId]) = 0
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Where_collection_navigation_ToArray_Count(bool async)
        {
            await base.Where_collection_navigation_ToArray_Count(async);

            AssertSql(
                @"SELECT [o].[Id], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Order] AS [o0] ON ([o].[Id] = [o0].[ClientId]) AND ([o].[Id] = [o0].[ClientId])
WHERE (
    SELECT COUNT(*)
    FROM [Order] AS [o1]
    WHERE [o].[Id] = [o1].[ClientId]) = 0
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Where_collection_navigation_AsEnumerable_Count(bool async)
        {
            await base.Where_collection_navigation_AsEnumerable_Count(async);

            AssertSql(
                @"SELECT [o].[Id], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE (
    SELECT COUNT(*)
    FROM [Order] AS [o1]
    WHERE [o].[Id] = [o1].[ClientId]) = 0
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Where_collection_navigation_ToList_Count_member(bool async)
        {
            await base.Where_collection_navigation_ToList_Count_member(async);

            AssertSql(
                @"SELECT [o].[Id], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE (
    SELECT COUNT(*)
    FROM [Order] AS [o1]
    WHERE [o].[Id] = [o1].[ClientId]) = 0
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Where_collection_navigation_ToArray_Length_member(bool async)
        {
            await base.Where_collection_navigation_ToArray_Length_member(async);

            AssertSql(
                @"SELECT [o].[Id], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Order] AS [o0] ON ([o].[Id] = [o0].[ClientId]) AND ([o].[Id] = [o0].[ClientId])
WHERE (
    SELECT COUNT(*)
    FROM [Order] AS [o1]
    WHERE [o].[Id] = [o1].[ClientId]) = 0
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Can_query_on_indexer_properties(bool async)
        {
            await base.Can_query_on_indexer_properties(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE [o].[Name] = N'Mona Cy'
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Can_query_on_owned_indexer_properties(bool async)
        {
            await base.Can_query_on_owned_indexer_properties(async);

            AssertSql(
                @"SELECT [o].[Name]
FROM [OwnedPerson] AS [o]
WHERE [o].[PersonAddress_ZipCode] = 38654");
        }

        public override async Task Can_query_on_indexer_property_when_property_name_from_closure(bool async)
        {
            await base.Can_query_on_indexer_property_when_property_name_from_closure(async);

            AssertSql(
                @"SELECT [o].[Name]
FROM [OwnedPerson] AS [o]
WHERE [o].[Name] = N'Mona Cy'");
        }

        public override async Task Can_project_indexer_properties(bool async)
        {
            await base.Can_project_indexer_properties(async);

            AssertSql(
                @"SELECT [o].[Name]
FROM [OwnedPerson] AS [o]");
        }

        public override async Task Can_project_owned_indexer_properties(bool async)
        {
            await base.Can_project_owned_indexer_properties(async);

            AssertSql(
                @"SELECT [o].[PersonAddress_AddressLine]
FROM [OwnedPerson] AS [o]");
        }

        public override async Task Can_project_indexer_properties_converted(bool async)
        {
            await base.Can_project_indexer_properties_converted(async);

            AssertSql(
                @"SELECT [o].[Name]
FROM [OwnedPerson] AS [o]");
        }

        public override async Task Can_project_owned_indexer_properties_converted(bool async)
        {
            await base.Can_project_owned_indexer_properties_converted(async);

            AssertSql(
                @"SELECT [o].[PersonAddress_AddressLine]
FROM [OwnedPerson] AS [o]");
        }

        public override async Task Can_OrderBy_indexer_properties(bool async)
        {
            await base.Can_OrderBy_indexer_properties(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
ORDER BY [o].[Name], [o].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Can_OrderBy_indexer_properties_converted(bool async)
        {
            await base.Can_OrderBy_indexer_properties_converted(async);

            AssertSql(
                @"SELECT [o].[Name]
FROM [OwnedPerson] AS [o]
ORDER BY [o].[Name], [o].[Id]");
        }

        public override async Task Can_OrderBy_owned_indexer_properties(bool async)
        {
            await base.Can_OrderBy_owned_indexer_properties(async);

            AssertSql(
                @"SELECT [o].[Name]
FROM [OwnedPerson] AS [o]
ORDER BY [o].[PersonAddress_ZipCode], [o].[Id]");
        }

        public override async Task Can_OrderBy_owened_indexer_properties_converted(bool async)
        {
            await base.Can_OrderBy_owened_indexer_properties_converted(async);

            AssertSql(
                @"SELECT [o].[Name]
FROM [OwnedPerson] AS [o]
ORDER BY [o].[PersonAddress_ZipCode], [o].[Id]");
        }

        public override async Task Can_group_by_indexer_property(bool isAsync)
        {
            await base.Can_group_by_indexer_property(isAsync);

            AssertSql(
                @"SELECT COUNT(*)
FROM [OwnedPerson] AS [o]
GROUP BY [o].[Name]");
        }

        public override async Task Can_group_by_converted_indexer_property(bool isAsync)
        {
            await base.Can_group_by_converted_indexer_property(isAsync);

            AssertSql(
                @"SELECT COUNT(*)
FROM [OwnedPerson] AS [o]
GROUP BY [o].[Name]");
        }

        public override async Task Can_group_by_owned_indexer_property(bool isAsync)
        {
            await base.Can_group_by_owned_indexer_property(isAsync);

            AssertSql(
                @"SELECT COUNT(*)
FROM [OwnedPerson] AS [o]
GROUP BY [o].[PersonAddress_ZipCode]");
        }

        public override async Task Can_group_by_converted_owned_indexer_property(bool isAsync)
        {
            await base.Can_group_by_converted_owned_indexer_property(isAsync);

            AssertSql(
                @"SELECT COUNT(*)
FROM [OwnedPerson] AS [o]
GROUP BY [o].[PersonAddress_ZipCode]");
        }

        public override async Task Can_join_on_indexer_property_on_query(bool isAsync)
        {
            await base.Can_join_on_indexer_property_on_query(isAsync);

            AssertSql(
                @"SELECT [o].[Id], [o0].[PersonAddress_Country_Name] AS [Name]
FROM [OwnedPerson] AS [o]
INNER JOIN [OwnedPerson] AS [o0] ON [o].[PersonAddress_ZipCode] = [o0].[PersonAddress_ZipCode]");
        }

        public override async Task Projecting_indexer_property_ignores_include(bool isAsync)
        {
            await base.Projecting_indexer_property_ignores_include(isAsync);

            AssertSql(
                @"SELECT [o].[PersonAddress_ZipCode] AS [Nation]
FROM [OwnedPerson] AS [o]");
        }

        public override async Task Projecting_indexer_property_ignores_include_converted(bool isAsync)
        {
            await base.Projecting_indexer_property_ignores_include_converted(isAsync);

            AssertSql(
                @"SELECT [o].[PersonAddress_ZipCode] AS [Nation]
FROM [OwnedPerson] AS [o]");
        }

        public override async Task Indexer_property_is_pushdown_into_subquery(bool isAsync)
        {
            await base.Indexer_property_is_pushdown_into_subquery(isAsync);

            AssertSql(
                @"SELECT [o].[Name]
FROM [OwnedPerson] AS [o]
WHERE (
    SELECT TOP(1) [o0].[Name]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[Id] = [o].[Id]) = N'Mona Cy'");
        }

        public override async Task Can_query_indexer_property_on_owned_collection(bool isAsync)
        {
            await base.Can_query_indexer_property_on_owned_collection(isAsync);

            AssertSql(
                @"SELECT [o].[Name]
FROM [OwnedPerson] AS [o]
WHERE (
    SELECT COUNT(*)
    FROM [Order] AS [o0]
    WHERE ([o].[Id] = [o0].[ClientId]) AND (DATEPART(year, [o0].[OrderDate]) = 2018)) = 1");
        }

        public override async Task Query_for_base_type_loads_all_owned_navs_split(bool async)
        {
            await base.Query_for_base_type_loads_all_owned_navs_split(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
ORDER BY [o].[Id]",
                //
                @"SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o].[Id]
FROM [OwnedPerson] AS [o]
INNER JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
ORDER BY [o].[Id]");
        }

        public override async Task Query_for_branch_type_loads_all_owned_navs_split(bool async)
        {
            await base.Query_for_branch_type_loads_all_owned_navs_split(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
WHERE [o].[Discriminator] IN (N'Branch', N'LeafA')
ORDER BY [o].[Id]",
                //
                @"SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o].[Id]
FROM [OwnedPerson] AS [o]
INNER JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE [o].[Discriminator] IN (N'Branch', N'LeafA')
ORDER BY [o].[Id]");
        }

        public override async Task Query_when_subquery_split(bool async)
        {
            await base.Query_when_subquery_split(async);

            AssertSql(
                @"@__p_0='5'

SELECT [t0].[Id], [t0].[Discriminator], [t0].[Name], [t0].[PersonAddress_AddressLine], [t0].[PersonAddress_PlaceType], [t0].[PersonAddress_ZipCode], [t0].[PersonAddress_Country_Name], [t0].[PersonAddress_Country_PlanetId], [t0].[BranchAddress_BranchName], [t0].[BranchAddress_PlaceType], [t0].[BranchAddress_Country_Name], [t0].[BranchAddress_Country_PlanetId], [t0].[LeafBAddress_LeafBType], [t0].[LeafBAddress_PlaceType], [t0].[LeafBAddress_Country_Name], [t0].[LeafBAddress_Country_PlanetId], [t0].[LeafAAddress_LeafType], [t0].[LeafAAddress_PlaceType], [t0].[LeafAAddress_Country_Name], [t0].[LeafAAddress_Country_PlanetId]
FROM (
    SELECT TOP(@__p_0) [t].[Id], [t].[Discriminator], [t].[Name], [t].[PersonAddress_AddressLine], [t].[PersonAddress_PlaceType], [t].[PersonAddress_ZipCode], [t].[PersonAddress_Country_Name], [t].[PersonAddress_Country_PlanetId], [t].[BranchAddress_BranchName], [t].[BranchAddress_PlaceType], [t].[BranchAddress_Country_Name], [t].[BranchAddress_Country_PlanetId], [t].[LeafBAddress_LeafBType], [t].[LeafBAddress_PlaceType], [t].[LeafBAddress_Country_Name], [t].[LeafBAddress_Country_PlanetId], [t].[LeafAAddress_LeafType], [t].[LeafAAddress_PlaceType], [t].[LeafAAddress_Country_Name], [t].[LeafAAddress_Country_PlanetId]
    FROM (
        SELECT DISTINCT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
        FROM [OwnedPerson] AS [o]
    ) AS [t]
    ORDER BY [t].[Id]
) AS [t0]
ORDER BY [t0].[Id]",
                //
                @"@__p_0='5'

SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [t0].[Id]
FROM (
    SELECT TOP(@__p_0) [t].[Id]
    FROM (
        SELECT DISTINCT [o].[Id], [o].[Discriminator], [o].[Name]
        FROM [OwnedPerson] AS [o]
    ) AS [t]
    ORDER BY [t].[Id]
) AS [t0]
INNER JOIN [Order] AS [o0] ON [t0].[Id] = [o0].[ClientId]
ORDER BY [t0].[Id]");
        }

        public override async Task Project_multiple_owned_navigations_split(bool async)
        {
            await base.Project_multiple_owned_navigations_split(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [p].[Id], [p].[StarId]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
ORDER BY [o].[Id], [p].[Id]",
                //
                @"SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o].[Id], [p].[Id]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
INNER JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
ORDER BY [o].[Id], [p].[Id]");
        }

        public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_split(bool async)
        {
            await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_split(async);

            AssertSql(
                @"SELECT [o].[Id], [p].[Id]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
ORDER BY [o].[Id], [p].[Id]",
                //
                @"SELECT [m].[Id], [m].[Diameter], [m].[PlanetId], [o].[Id], [p].[Id]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
INNER JOIN [Moon] AS [m] ON [p].[Id] = [m].[PlanetId]
ORDER BY [o].[Id], [p].[Id]");
        }

        public override async Task Query_with_OfType_eagerly_loads_correct_owned_navigations_split(bool async)
        {
            await base.Query_with_OfType_eagerly_loads_correct_owned_navigations_split(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
WHERE [o].[Discriminator] = N'LeafA'
ORDER BY [o].[Id]",
                //
                @"SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o].[Id]
FROM [OwnedPerson] AS [o]
INNER JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE [o].[Discriminator] = N'LeafA'
ORDER BY [o].[Id]");
        }

        public override async Task Unmapped_property_projection_loads_owned_navigations_split(bool async)
        {
            await base.Unmapped_property_projection_loads_owned_navigations_split(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
WHERE [o].[Id] = 1
ORDER BY [o].[Id]",
                //
                @"SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o].[Id]
FROM [OwnedPerson] AS [o]
INNER JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE [o].[Id] = 1
ORDER BY [o].[Id]");
        }

        public override async Task Can_query_on_indexer_properties_split(bool async)
        {
            await base.Can_query_on_indexer_properties_split(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
WHERE [o].[Name] = N'Mona Cy'
ORDER BY [o].[Id]",
                //
                @"SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o].[Id]
FROM [OwnedPerson] AS [o]
INNER JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE [o].[Name] = N'Mona Cy'
ORDER BY [o].[Id]");
        }

        public override async Task GroupBy_with_multiple_aggregates_on_owned_navigation_properties(bool async)
        {
            await base.GroupBy_with_multiple_aggregates_on_owned_navigation_properties(async);

            AssertSql(
                @"SELECT AVG(CAST([s].[Id] AS float)) AS [p1], COALESCE(SUM([s].[Id]), 0) AS [p2], MAX(CAST(LEN([s].[Name]) AS int)) AS [p3]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Star] AS [s] ON [p].[StarId] = [s].[Id]");
        }

        public override async Task Ordering_by_identifying_projection(bool async)
        {
            await base.Ordering_by_identifying_projection(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId], [o0].[ClientId], [o0].[Id], [o0].[OrderDate]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
ORDER BY [o].[PersonAddress_PlaceType], [o].[Id], [o0].[ClientId], [o0].[Id]");
        }

        public override async Task Using_from_sql_on_owner_generates_join_with_table_for_owned_shared_dependents(bool async)
        {
            await base.Using_from_sql_on_owner_generates_join_with_table_for_owned_shared_dependents(async);

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Name], [t].[Id], [t].[PersonAddress_AddressLine], [t].[PersonAddress_PlaceType], [t].[PersonAddress_ZipCode], [t].[Id1], [t].[PersonAddress_Country_Name], [t].[PersonAddress_Country_PlanetId], [t1].[Id], [t1].[BranchAddress_BranchName], [t1].[BranchAddress_PlaceType], [t1].[Id1], [t1].[BranchAddress_Country_Name], [t1].[BranchAddress_Country_PlanetId], [t3].[Id], [t3].[LeafBAddress_LeafBType], [t3].[LeafBAddress_PlaceType], [t3].[Id1], [t3].[LeafBAddress_Country_Name], [t3].[LeafBAddress_Country_PlanetId], [t5].[Id], [t5].[LeafAAddress_LeafType], [t5].[LeafAAddress_PlaceType], [t5].[Id1], [t5].[LeafAAddress_Country_Name], [t5].[LeafAAddress_Country_PlanetId], [t].[Id0], [t1].[Id0], [t3].[Id0], [t5].[Id0], [o8].[ClientId], [o8].[Id], [o8].[OrderDate]
FROM (
    SELECT * FROM ""OwnedPerson""
) AS [o]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_PlaceType], [o0].[PersonAddress_ZipCode], [o1].[Id] AS [Id0], [o0].[Id] AS [Id1], [o0].[PersonAddress_Country_Name], [o0].[PersonAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o0]
    INNER JOIN [OwnedPerson] AS [o1] ON [o0].[Id] = [o1].[Id]
    WHERE [o0].[PersonAddress_ZipCode] IS NOT NULL
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [o2].[Id], [o2].[BranchAddress_BranchName], [o2].[BranchAddress_PlaceType], [t0].[Id] AS [Id0], [o2].[Id] AS [Id1], [o2].[BranchAddress_Country_Name], [o2].[BranchAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o2]
    INNER JOIN (
        SELECT [o3].[Id]
        FROM [OwnedPerson] AS [o3]
        WHERE [o3].[Discriminator] IN (N'Branch', N'LeafA')
    ) AS [t0] ON [o2].[Id] = [t0].[Id]
    WHERE [o2].[BranchAddress_PlaceType] IS NOT NULL OR [o2].[BranchAddress_BranchName] IS NOT NULL
) AS [t1] ON [o].[Id] = [t1].[Id]
LEFT JOIN (
    SELECT [o4].[Id], [o4].[LeafBAddress_LeafBType], [o4].[LeafBAddress_PlaceType], [t2].[Id] AS [Id0], [o4].[Id] AS [Id1], [o4].[LeafBAddress_Country_Name], [o4].[LeafBAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o4]
    INNER JOIN (
        SELECT [o5].[Id]
        FROM [OwnedPerson] AS [o5]
        WHERE [o5].[Discriminator] = N'LeafB'
    ) AS [t2] ON [o4].[Id] = [t2].[Id]
    WHERE [o4].[LeafBAddress_PlaceType] IS NOT NULL OR [o4].[LeafBAddress_LeafBType] IS NOT NULL
) AS [t3] ON [o].[Id] = [t3].[Id]
LEFT JOIN (
    SELECT [o6].[Id], [o6].[LeafAAddress_LeafType], [o6].[LeafAAddress_PlaceType], [t4].[Id] AS [Id0], [o6].[Id] AS [Id1], [o6].[LeafAAddress_Country_Name], [o6].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o6]
    INNER JOIN (
        SELECT [o7].[Id]
        FROM [OwnedPerson] AS [o7]
        WHERE [o7].[Discriminator] = N'LeafA'
    ) AS [t4] ON [o6].[Id] = [t4].[Id]
    WHERE [o6].[LeafAAddress_LeafType] IS NOT NULL
) AS [t5] ON [o].[Id] = [t5].[Id]
LEFT JOIN [Order] AS [o8] ON [o].[Id] = [o8].[ClientId]
ORDER BY [o].[Id], [t].[Id], [t].[Id0], [t1].[Id], [t1].[Id0], [t3].[Id], [t3].[Id0], [t5].[Id], [t5].[Id0], [o8].[ClientId], [o8].[Id]");
        }

        public override async Task Projecting_collection_correlated_with_keyless_entity_after_navigation_works_using_parent_identifiers(bool async)
        {
            await base.Projecting_collection_correlated_with_keyless_entity_after_navigation_works_using_parent_identifiers(async);

            AssertSql(
                @"SELECT [b].[Throned_Value], [f].[Id], [b].[Id], [p].[Id], [p].[StarId]
FROM [Fink] AS [f]
LEFT JOIN [Barton] AS [b] ON [f].[BartonId] = [b].[Id]
LEFT JOIN [Planet] AS [p] ON ([b].[Throned_Value] <> [p].[Id]) OR [b].[Throned_Value] IS NULL
ORDER BY [f].[Id], [b].[Id], [p].[Id]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class OwnedQuerySqlServerFixture : RelationalOwnedQueryFixture
        {
            protected override ITestStoreFactory TestStoreFactory
                => SqlServerTestStoreFactory.Instance;
        }
    }
}
