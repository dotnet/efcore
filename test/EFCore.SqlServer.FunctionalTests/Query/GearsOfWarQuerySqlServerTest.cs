// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class GearsOfWarQuerySqlServerTest : GearsOfWarQueryRelationalTestBase<GearsOfWarQuerySqlServerFixture>
    {
#pragma warning disable IDE0060 // Remove unused parameter
        public GearsOfWarQuerySqlServerTest(GearsOfWarQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
#pragma warning restore IDE0060 // Remove unused parameter
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override bool CanExecuteQueryString
            => true;

        public override async Task Negate_on_binary_expression(bool async)
        {
            await base.Negate_on_binary_expression(async);

            AssertSql(
                @"SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE [s].[Id] = -([s].[Id] + [s].[Id])");
        }

        public override async Task Negate_on_column(bool async)
        {
            await base.Negate_on_column(async);

            AssertSql(
                @"SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE [s].[Id] = -[s].[Id]");
        }

        public override async Task Negate_on_like_expression(bool async)
        {
            await base.Negate_on_like_expression(async);

            AssertSql(
                @"SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE [s].[Name] IS NOT NULL AND NOT ([s].[Name] LIKE N'us%')");
        }

        public override async Task Entity_equality_empty(bool async)
        {
            await base.Entity_equality_empty(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE 0 = 1");
        }

        public override async Task Include_multiple_one_to_one_and_one_to_many(bool async)
        {
            await base.Include_multiple_one_to_one_and_one_to_many(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
LEFT JOIN [Weapons] AS [w] ON [g].[FullName] = [w].[OwnerFullName]
ORDER BY [t].[Id], [g].[Nickname], [g].[SquadId], [w].[Id]");
        }

        public override async Task Include_multiple_one_to_one_optional_and_one_to_one_required(bool async)
        {
            await base.Include_multiple_one_to_one_optional_and_one_to_one_required(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
LEFT JOIN [Squads] AS [s] ON [g].[SquadId] = [s].[Id]");
        }

        public override async Task Include_multiple_circular(bool async)
        {
            await base.Include_multiple_circular(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location], [c].[Nation], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gears] AS [g]
INNER JOIN [Cities] AS [c] ON [g].[CityOfBirthName] = [c].[Name]
LEFT JOIN [Gears] AS [g0] ON [c].[Name] = [g0].[AssignedCityName]
ORDER BY [g].[Nickname], [g].[SquadId], [c].[Name], [g0].[Nickname], [g0].[SquadId]");
        }

        public override async Task Include_multiple_circular_with_filter(bool async)
        {
            await base.Include_multiple_circular_with_filter(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location], [c].[Nation], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gears] AS [g]
INNER JOIN [Cities] AS [c] ON [g].[CityOfBirthName] = [c].[Name]
LEFT JOIN [Gears] AS [g0] ON [c].[Name] = [g0].[AssignedCityName]
WHERE [g].[Nickname] = N'Marcus'
ORDER BY [g].[Nickname], [g].[SquadId], [c].[Name], [g0].[Nickname], [g0].[SquadId]");
        }

        public override async Task Include_using_alternate_key(bool async)
        {
            await base.Include_using_alternate_key(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Gears] AS [g]
LEFT JOIN [Weapons] AS [w] ON [g].[FullName] = [w].[OwnerFullName]
WHERE [g].[Nickname] = N'Marcus'
ORDER BY [g].[Nickname], [g].[SquadId], [w].[Id]");
        }

        public override async Task Include_navigation_on_derived_type(bool async)
        {
            await base.Include_navigation_on_derived_type(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Gears] AS [g0] ON ([g].[Nickname] = [g0].[LeaderNickname]) AND ([g].[SquadId] = [g0].[LeaderSquadId])
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId]");
        }

        public override async Task String_based_Include_navigation_on_derived_type(bool async)
        {
            await base.String_based_Include_navigation_on_derived_type(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Gears] AS [g0] ON ([g].[Nickname] = [g0].[LeaderNickname]) AND ([g].[SquadId] = [g0].[LeaderSquadId])
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId]");
        }

        public override async Task Select_Where_Navigation_Included(bool async)
        {
            await base.Select_Where_Navigation_Included(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE [g].[Nickname] = N'Marcus'");
        }

        public override async Task Include_with_join_reference1(bool async)
        {
            await base.Include_with_join_reference1(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location], [c].[Nation]
FROM [Gears] AS [g]
INNER JOIN [Tags] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
INNER JOIN [Cities] AS [c] ON [g].[CityOfBirthName] = [c].[Name]");
        }

        public override async Task Include_with_join_reference2(bool async)
        {
            await base.Include_with_join_reference2(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location], [c].[Nation]
FROM [Tags] AS [t]
INNER JOIN [Gears] AS [g] ON ([t].[GearSquadId] = [g].[SquadId]) AND ([t].[GearNickName] = [g].[Nickname])
INNER JOIN [Cities] AS [c] ON [g].[CityOfBirthName] = [c].[Name]");
        }

        public override async Task Include_with_join_collection1(bool async)
        {
            await base.Include_with_join_collection1(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Id], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Gears] AS [g]
INNER JOIN [Tags] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
LEFT JOIN [Weapons] AS [w] ON [g].[FullName] = [w].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Id], [w].[Id]");
        }

        public override async Task Include_with_join_collection2(bool async)
        {
            await base.Include_with_join_collection2(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Id], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Tags] AS [t]
INNER JOIN [Gears] AS [g] ON ([t].[GearSquadId] = [g].[SquadId]) AND ([t].[GearNickName] = [g].[Nickname])
LEFT JOIN [Weapons] AS [w] ON [g].[FullName] = [w].[OwnerFullName]
ORDER BY [t].[Id], [g].[Nickname], [g].[SquadId], [w].[Id]");
        }

        public override async Task Include_where_list_contains_navigation(bool async)
        {
            await base.Include_where_list_contains_navigation(async);

            AssertSql(
                @"SELECT [t].[Id]
FROM [Tags] AS [t]",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
WHERE [t].[Id] IS NOT NULL AND [t].[Id] IN ('34c8d86e-a4ac-4be5-827f-584dda348a07', 'df36f493-463f-4123-83f9-6b135deeb7ba', 'a8ad98f9-e023-4e2a-9a70-c2728455bd34', '70534e05-782c-4052-8720-c2c54481ce5f', 'a7be028a-0cf2-448f-ab55-ce8bc5d8cf69', 'b39a6fba-9026-4d69-828e-fd7068673e57')");
        }

        public override async Task Include_where_list_contains_navigation2(bool async)
        {
            await base.Include_where_list_contains_navigation2(async);

            AssertSql(
                @"SELECT [t].[Id]
FROM [Tags] AS [t]",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Gears] AS [g]
INNER JOIN [Cities] AS [c] ON [g].[CityOfBirthName] = [c].[Name]
LEFT JOIN [Tags] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
WHERE [c].[Location] IS NOT NULL AND [t].[Id] IN ('34c8d86e-a4ac-4be5-827f-584dda348a07', 'df36f493-463f-4123-83f9-6b135deeb7ba', 'a8ad98f9-e023-4e2a-9a70-c2728455bd34', '70534e05-782c-4052-8720-c2c54481ce5f', 'a7be028a-0cf2-448f-ab55-ce8bc5d8cf69', 'b39a6fba-9026-4d69-828e-fd7068673e57')");
        }

        public override async Task Navigation_accessed_twice_outside_and_inside_subquery(bool async)
        {
            await base.Navigation_accessed_twice_outside_and_inside_subquery(async);

            AssertSql(
                @"SELECT [t].[Id]
FROM [Tags] AS [t]",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
WHERE [t].[Id] IS NOT NULL AND [t].[Id] IN ('34c8d86e-a4ac-4be5-827f-584dda348a07', 'df36f493-463f-4123-83f9-6b135deeb7ba', 'a8ad98f9-e023-4e2a-9a70-c2728455bd34', '70534e05-782c-4052-8720-c2c54481ce5f', 'a7be028a-0cf2-448f-ab55-ce8bc5d8cf69', 'b39a6fba-9026-4d69-828e-fd7068673e57')");
        }

        public override async Task Include_with_join_multi_level(bool async)
        {
            await base.Include_with_join_multi_level(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location], [c].[Nation], [t].[Id], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gears] AS [g]
INNER JOIN [Tags] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
INNER JOIN [Cities] AS [c] ON [g].[CityOfBirthName] = [c].[Name]
LEFT JOIN [Gears] AS [g0] ON [c].[Name] = [g0].[AssignedCityName]
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Id], [c].[Name], [g0].[Nickname], [g0].[SquadId]");
        }

        public override async Task Include_with_join_and_inheritance1(bool async)
        {
            await base.Include_with_join_and_inheritance1(async);

            AssertSql(
                @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [c].[Name], [c].[Location], [c].[Nation]
FROM [Tags] AS [t]
INNER JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
    FROM [Gears] AS [g]
    WHERE [g].[Discriminator] = N'Officer'
) AS [t0] ON ([t].[GearSquadId] = [t0].[SquadId]) AND ([t].[GearNickName] = [t0].[Nickname])
INNER JOIN [Cities] AS [c] ON [t0].[CityOfBirthName] = [c].[Name]");
        }

        public override async Task Include_with_join_and_inheritance_with_orderby_before_and_after_include(bool async)
        {
            await base.Include_with_join_and_inheritance_with_orderby_before_and_after_include(async);

            AssertSql(
                @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t].[Id], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Tags] AS [t]
INNER JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
    FROM [Gears] AS [g]
    WHERE [g].[Discriminator] = N'Officer'
) AS [t0] ON ([t].[GearSquadId] = [t0].[SquadId]) AND ([t].[GearNickName] = [t0].[Nickname])
LEFT JOIN [Gears] AS [g0] ON ([t0].[Nickname] = [g0].[LeaderNickname]) AND ([t0].[SquadId] = [g0].[LeaderSquadId])
ORDER BY [t0].[HasSoulPatch], [t0].[Nickname] DESC, [t].[Id], [t0].[SquadId], [g0].[Nickname], [g0].[SquadId]");
        }

        public override async Task Include_with_join_and_inheritance2(bool async)
        {
            await base.Include_with_join_and_inheritance2(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Id], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Gears] AS [g]
INNER JOIN [Tags] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
LEFT JOIN [Weapons] AS [w] ON [g].[FullName] = [w].[OwnerFullName]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Id], [w].[Id]");
        }

        public override async Task Include_with_join_and_inheritance3(bool async)
        {
            await base.Include_with_join_and_inheritance3(async);

            AssertSql(
                @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t].[Id], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Tags] AS [t]
INNER JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
    FROM [Gears] AS [g]
    WHERE [g].[Discriminator] = N'Officer'
) AS [t0] ON ([t].[GearSquadId] = [t0].[SquadId]) AND ([t].[GearNickName] = [t0].[Nickname])
LEFT JOIN [Gears] AS [g0] ON ([t0].[Nickname] = [g0].[LeaderNickname]) AND ([t0].[SquadId] = [g0].[LeaderSquadId])
ORDER BY [t].[Id], [t0].[Nickname], [t0].[SquadId], [g0].[Nickname], [g0].[SquadId]");
        }

        public override async Task Include_with_nested_navigation_in_order_by(bool async)
        {
            await base.Include_with_nested_navigation_in_order_by(async);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Weapons] AS [w]
LEFT JOIN [Gears] AS [g] ON [w].[OwnerFullName] = [g].[FullName]
LEFT JOIN [Cities] AS [c] ON [g].[CityOfBirthName] = [c].[Name]
WHERE ([g].[Nickname] <> N'Paduk') OR [g].[Nickname] IS NULL
ORDER BY [c].[Name], [w].[Id]");
        }

        public override async Task Where_enum(bool async)
        {
            await base.Where_enum(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Rank] = 4");
        }

        public override async Task Where_nullable_enum_with_constant(bool async)
        {
            await base.Where_nullable_enum_with_constant(async);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] = 1");
        }

        public override async Task Where_nullable_enum_with_null_constant(bool async)
        {
            await base.Where_nullable_enum_with_null_constant(async);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] IS NULL");
        }

        public override async Task Where_nullable_enum_with_non_nullable_parameter(bool async)
        {
            await base.Where_nullable_enum_with_non_nullable_parameter(async);

            AssertSql(
                @"@__ammunitionType_0='1'

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] = @__ammunitionType_0");
        }

        public override async Task Where_nullable_enum_with_nullable_parameter(bool async)
        {
            await base.Where_nullable_enum_with_nullable_parameter(async);

            AssertSql(
                @"@__ammunitionType_0='1' (Nullable = true)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] = @__ammunitionType_0",
                //
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] IS NULL");
        }

        public override async Task Where_bitwise_and_enum(bool async)
        {
            await base.Where_bitwise_and_enum(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[Rank] & 2) > 0",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[Rank] & 2) = 2");
        }

        public override async Task Where_bitwise_and_integral(bool async)
        {
            await base.Where_bitwise_and_integral(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[Rank] & 1) = 1",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE (CAST([g].[Rank] AS bigint) & CAST(1 AS bigint)) = CAST(1 AS bigint)",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE (CAST([g].[Rank] AS smallint) & CAST(1 AS smallint)) = CAST(1 AS smallint)");
        }

        public override async Task Where_bitwise_and_nullable_enum_with_constant(bool async)
        {
            await base.Where_bitwise_and_nullable_enum_with_constant(async);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & 1) > 0");
        }

        public override async Task Where_bitwise_and_nullable_enum_with_null_constant(bool async)
        {
            await base.Where_bitwise_and_nullable_enum_with_null_constant(async);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & NULL) > 0");
        }

        public override async Task Where_bitwise_and_nullable_enum_with_non_nullable_parameter(bool async)
        {
            await base.Where_bitwise_and_nullable_enum_with_non_nullable_parameter(async);

            AssertSql(
                @"@__ammunitionType_0='1'

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & @__ammunitionType_0) > 0");
        }

        public override async Task Where_bitwise_and_nullable_enum_with_nullable_parameter(bool async)
        {
            await base.Where_bitwise_and_nullable_enum_with_nullable_parameter(async);

            AssertSql(
                @"@__ammunitionType_0='1' (Nullable = true)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & @__ammunitionType_0) > 0",
                //
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & NULL) > 0");
        }

        public override async Task Where_bitwise_or_enum(bool async)
        {
            await base.Where_bitwise_or_enum(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[Rank] | 2) > 0");
        }

        public override async Task Bitwise_projects_values_in_select(bool async)
        {
            await base.Bitwise_projects_values_in_select(async);

            AssertSql(
                @"SELECT TOP(1) CASE
    WHEN ([g].[Rank] & 2) = 2 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [BitwiseTrue], CASE
    WHEN ([g].[Rank] & 2) = 4 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [BitwiseFalse], [g].[Rank] & 2 AS [BitwiseValue]
FROM [Gears] AS [g]
WHERE ([g].[Rank] & 2) = 2");
        }

        public override async Task Where_enum_has_flag(bool async)
        {
            await base.Where_enum_has_flag(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[Rank] & 2) = 2",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[Rank] & 18) = 18",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[Rank] & 1) = 1",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[Rank] & 1) = 1",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE (2 & [g].[Rank]) = [g].[Rank]");
        }

        public override async Task Where_enum_has_flag_subquery(bool async)
        {
            await base.Where_enum_has_flag_subquery(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[Rank] & COALESCE((
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId]), 0)) = COALESCE((
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId]), 0)",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE (2 & COALESCE((
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId]), 0)) = COALESCE((
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId]), 0)");
        }

        public override async Task Where_enum_has_flag_subquery_with_pushdown(bool async)
        {
            await base.Where_enum_has_flag_subquery_with_pushdown(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE (([g].[Rank] & (
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId])) = (
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId])) OR (
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId]) IS NULL",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ((2 & (
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId])) = (
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId])) OR (
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId]) IS NULL");
        }

        public override async Task Where_enum_has_flag_subquery_client_eval(bool async)
        {
            await base.Where_enum_has_flag_subquery_client_eval(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE (([g].[Rank] & (
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId])) = (
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId])) OR (
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId]) IS NULL");
        }

        public override async Task Where_enum_has_flag_with_non_nullable_parameter(bool async)
        {
            await base.Where_enum_has_flag_with_non_nullable_parameter(async);

            AssertSql(
                @"@__parameter_0='2'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[Rank] & @__parameter_0) = @__parameter_0");
        }

        public override async Task Where_has_flag_with_nullable_parameter(bool async)
        {
            await base.Where_has_flag_with_nullable_parameter(async);

            AssertSql(
                @"@__parameter_0='2' (Nullable = true)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[Rank] & @__parameter_0) = @__parameter_0");
        }

        public override async Task Select_enum_has_flag(bool async)
        {
            await base.Select_enum_has_flag(async);

            AssertSql(
                @"SELECT TOP(1) CASE
    WHEN ([g].[Rank] & 2) = 2 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [hasFlagTrue], CASE
    WHEN ([g].[Rank] & 4) = 4 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [hasFlagFalse]
FROM [Gears] AS [g]
WHERE ([g].[Rank] & 2) = 2");
        }

        public override async Task Where_count_subquery_without_collision(bool async)
        {
            await base.Where_count_subquery_without_collision(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE (
    SELECT COUNT(*)
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]) = 2");
        }

        public override async Task Where_any_subquery_without_collision(bool async)
        {
            await base.Where_any_subquery_without_collision(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE EXISTS (
    SELECT 1
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName])");
        }

        public override async Task Select_inverted_boolean(bool async)
        {
            await base.Select_inverted_boolean(async);

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = CAST(0 AS bit) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Manual]
FROM [Weapons] AS [w]
WHERE [w].[IsAutomatic] = CAST(1 AS bit)");
        }

        public override async Task Select_comparison_with_null(bool async)
        {
            await base.Select_comparison_with_null(async);

            AssertSql(
                @"@__ammunitionType_0='1' (Nullable = true)

SELECT [w].[Id], CASE
    WHEN ([w].[AmmunitionType] = @__ammunitionType_0) AND [w].[AmmunitionType] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Cartridge]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] = @__ammunitionType_0",
                //
                @"SELECT [w].[Id], CASE
    WHEN [w].[AmmunitionType] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Cartridge]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] IS NULL");
        }

        public override async Task Select_null_parameter(bool async)
        {
            await base.Select_null_parameter(async);

            AssertSql(
                @"@__ammunitionType_0='1' (Nullable = true)

SELECT [w].[Id], @__ammunitionType_0 AS [AmmoType]
FROM [Weapons] AS [w]",
                //
                @"SELECT [w].[Id], NULL AS [AmmoType]
FROM [Weapons] AS [w]",
                //
                @"@__ammunitionType_0='2' (Nullable = true)

SELECT [w].[Id], @__ammunitionType_0 AS [AmmoType]
FROM [Weapons] AS [w]",
                //
                @"SELECT [w].[Id], NULL AS [AmmoType]
FROM [Weapons] AS [w]");
        }

        public override async Task Select_ternary_operation_with_boolean(bool async)
        {
            await base.Select_ternary_operation_with_boolean(async);

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = CAST(1 AS bit) THEN 1
    ELSE 0
END AS [Num]
FROM [Weapons] AS [w]");
        }

        public override async Task Select_ternary_operation_with_inverted_boolean(bool async)
        {
            await base.Select_ternary_operation_with_inverted_boolean(async);

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = CAST(0 AS bit) THEN 1
    ELSE 0
END AS [Num]
FROM [Weapons] AS [w]");
        }

        public override async Task Select_ternary_operation_with_has_value_not_null(bool async)
        {
            await base.Select_ternary_operation_with_has_value_not_null(async);

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN [w].[AmmunitionType] IS NOT NULL AND ([w].[AmmunitionType] = 1) THEN N'Yes'
    ELSE N'No'
END AS [IsCartridge]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] IS NOT NULL AND ([w].[AmmunitionType] = 1)");
        }

        public override async Task Select_ternary_operation_multiple_conditions(bool async)
        {
            await base.Select_ternary_operation_multiple_conditions(async);

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN ([w].[AmmunitionType] = 2) AND ([w].[SynergyWithId] = 1) THEN N'Yes'
    ELSE N'No'
END AS [IsCartridge]
FROM [Weapons] AS [w]");
        }

        public override async Task Select_ternary_operation_multiple_conditions_2(bool async)
        {
            await base.Select_ternary_operation_multiple_conditions_2(async);

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN ([w].[IsAutomatic] = CAST(0 AS bit)) AND ([w].[SynergyWithId] = 1) THEN N'Yes'
    ELSE N'No'
END AS [IsCartridge]
FROM [Weapons] AS [w]");
        }

        public override async Task Select_multiple_conditions(bool async)
        {
            await base.Select_multiple_conditions(async);

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN ([w].[IsAutomatic] = CAST(0 AS bit)) AND (([w].[SynergyWithId] = 1) AND [w].[SynergyWithId] IS NOT NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsCartridge]
FROM [Weapons] AS [w]");
        }

        public override async Task Select_nested_ternary_operations(bool async)
        {
            await base.Select_nested_ternary_operations(async);

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = CAST(0 AS bit) THEN CASE
        WHEN [w].[AmmunitionType] = 1 THEN N'ManualCartridge'
        ELSE N'Manual'
    END
    ELSE N'Auto'
END AS [IsManualCartridge]
FROM [Weapons] AS [w]");
        }

        public override async Task Null_propagation_optimization1(bool async)
        {
            await base.Null_propagation_optimization1(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[LeaderNickname] = N'Marcus') AND [g].[LeaderNickname] IS NOT NULL");
        }

        public override async Task Null_propagation_optimization2(bool async)
        {
            await base.Null_propagation_optimization2(async);

            // issue #16050
            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE CASE
    WHEN [g].[LeaderNickname] IS NULL THEN NULL
    ELSE CASE
        WHEN [g].[LeaderNickname] IS NOT NULL AND ([g].[LeaderNickname] LIKE N'%us') THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
END = CAST(1 AS bit)");
        }

        public override async Task Null_propagation_optimization3(bool async)
        {
            await base.Null_propagation_optimization3(async);

            // issue #16050
            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN CASE
        WHEN [g].[LeaderNickname] LIKE N'%us' THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    ELSE NULL
END = CAST(1 AS bit)");
        }

        public override async Task Null_propagation_optimization4(bool async)
        {
            await base.Null_propagation_optimization4(async);

            // issue #16050
            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE (CASE
    WHEN [g].[LeaderNickname] IS NULL THEN NULL
    ELSE CAST(LEN([g].[LeaderNickname]) AS int)
END = 5) AND CASE
    WHEN [g].[LeaderNickname] IS NULL THEN NULL
    ELSE CAST(LEN([g].[LeaderNickname]) AS int)
END IS NOT NULL");
        }

        public override async Task Null_propagation_optimization5(bool async)
        {
            await base.Null_propagation_optimization5(async);

            // issue #16050
            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE (CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN CAST(LEN([g].[LeaderNickname]) AS int)
    ELSE NULL
END = 5) AND CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN CAST(LEN([g].[LeaderNickname]) AS int)
    ELSE NULL
END IS NOT NULL");
        }

        public override async Task Null_propagation_optimization6(bool async)
        {
            await base.Null_propagation_optimization6(async);

            // issue #16050
            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE (CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN CAST(LEN([g].[LeaderNickname]) AS int)
    ELSE NULL
END = 5) AND CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN CAST(LEN([g].[LeaderNickname]) AS int)
    ELSE NULL
END IS NOT NULL");
        }

        public override async Task Select_null_propagation_optimization7(bool async)
        {
            await base.Select_null_propagation_optimization7(async);

            // issue #16050
            AssertSql(
                @"SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN [g].[LeaderNickname] + [g].[LeaderNickname]
    ELSE NULL
END
FROM [Gears] AS [g]");
        }

        public override async Task Select_null_propagation_optimization8(bool async)
        {
            await base.Select_null_propagation_optimization8(async);

            AssertSql(
                @"SELECT [g].[LeaderNickname] + [g].[LeaderNickname]
FROM [Gears] AS [g]");
        }

        public override async Task Select_null_propagation_optimization9(bool async)
        {
            await base.Select_null_propagation_optimization9(async);

            AssertSql(
                @"SELECT CAST(LEN([g].[FullName]) AS int)
FROM [Gears] AS [g]");
        }

        public override async Task Select_null_propagation_negative1(bool async)
        {
            await base.Select_null_propagation_negative1(async);

            AssertSql(
                @"SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN CASE
        WHEN CAST(LEN([g].[Nickname]) AS int) = 5 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    ELSE NULL
END
FROM [Gears] AS [g]");
        }

        public override async Task Select_null_propagation_negative2(bool async)
        {
            await base.Select_null_propagation_negative2(async);

            AssertSql(
                @"SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN [g0].[LeaderNickname]
    ELSE NULL
END
FROM [Gears] AS [g]
CROSS JOIN [Gears] AS [g0]");
        }

        public override async Task Select_null_propagation_negative3(bool async)
        {
            await base.Select_null_propagation_negative3(async);

            AssertSql(
                @"SELECT [g0].[Nickname], CASE
    WHEN [g0].[Nickname] IS NOT NULL AND [g0].[SquadId] IS NOT NULL THEN CASE
        WHEN [g0].[LeaderNickname] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    ELSE NULL
END AS [Condition]
FROM [Gears] AS [g]
LEFT JOIN [Gears] AS [g0] ON [g].[HasSoulPatch] = CAST(1 AS bit)
ORDER BY [g0].[Nickname]");
        }

        public override async Task Select_null_propagation_negative4(bool async)
        {
            await base.Select_null_propagation_negative4(async);

            AssertSql(
                @"SELECT CASE
    WHEN [g0].[Nickname] IS NOT NULL AND [g0].[SquadId] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [g0].[Nickname]
FROM [Gears] AS [g]
LEFT JOIN [Gears] AS [g0] ON [g].[HasSoulPatch] = CAST(1 AS bit)
ORDER BY [g0].[Nickname]");
        }

        public override async Task Select_null_propagation_negative5(bool async)
        {
            await base.Select_null_propagation_negative5(async);

            AssertSql(
                @"SELECT CASE
    WHEN [g0].[Nickname] IS NOT NULL AND [g0].[SquadId] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [g0].[Nickname]
FROM [Gears] AS [g]
LEFT JOIN [Gears] AS [g0] ON [g].[HasSoulPatch] = CAST(1 AS bit)
ORDER BY [g0].[Nickname]");
        }

        public override async Task Select_null_propagation_negative6(bool async)
        {
            await base.Select_null_propagation_negative6(async);

            AssertSql(
                @"SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN CASE
        WHEN CAST(LEN([g].[LeaderNickname]) AS int) <> CAST(LEN([g].[LeaderNickname]) AS int) THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    ELSE NULL
END
FROM [Gears] AS [g]");
        }

        public override async Task Select_null_propagation_negative7(bool async)
        {
            await base.Select_null_propagation_negative7(async);

            AssertSql(
                @"SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN CAST(1 AS bit)
    ELSE NULL
END
FROM [Gears] AS [g]");
        }

        public override async Task Select_null_propagation_negative8(bool async)
        {
            await base.Select_null_propagation_negative8(async);

            AssertSql(
                @"SELECT CASE
    WHEN [s].[Id] IS NOT NULL THEN [c].[Name]
    ELSE NULL
END
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
LEFT JOIN [Squads] AS [s] ON [g].[SquadId] = [s].[Id]
LEFT JOIN [Cities] AS [c] ON [g].[AssignedCityName] = [c].[Name]");
        }

        public override async Task Select_null_propagation_negative9(bool async)
        {
            await base.Select_null_propagation_negative9(async);

            AssertSql(
                @"SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN COALESCE(CASE
        WHEN CAST(LEN([g].[Nickname]) AS int) = 5 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END, CAST(0 AS bit))
    ELSE NULL
END
FROM [Gears] AS [g]");
        }

        public override async Task Select_null_propagation_works_for_navigations_with_composite_keys(bool async)
        {
            await base.Select_null_propagation_works_for_navigations_with_composite_keys(async);

            AssertSql(
                @"SELECT [g].[Nickname]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])");
        }

        public override async Task Select_null_propagation_works_for_multiple_navigations_with_composite_keys(bool async)
        {
            await base.Select_null_propagation_works_for_multiple_navigations_with_composite_keys(async);

            AssertSql(
                @"SELECT CASE
    WHEN [c].[Name] IS NOT NULL THEN [c].[Name]
    ELSE NULL
END
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
LEFT JOIN [Tags] AS [t0] ON (([g].[Nickname] = [t0].[GearNickName]) OR ([g].[Nickname] IS NULL AND [t0].[GearNickName] IS NULL)) AND (([g].[SquadId] = [t0].[GearSquadId]) OR ([g].[SquadId] IS NULL AND [t0].[GearSquadId] IS NULL))
LEFT JOIN [Gears] AS [g0] ON ([t0].[GearNickName] = [g0].[Nickname]) AND ([t0].[GearSquadId] = [g0].[SquadId])
LEFT JOIN [Cities] AS [c] ON [g0].[AssignedCityName] = [c].[Name]");
        }

        public override async Task Select_conditional_with_anonymous_type_and_null_constant(bool async)
        {
            await base.Select_conditional_with_anonymous_type_and_null_constant(async);

            AssertSql(
                @"SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [g].[HasSoulPatch]
FROM [Gears] AS [g]
ORDER BY [g].[Nickname]");
        }

        public override async Task Select_conditional_with_anonymous_types(bool async)
        {
            await base.Select_conditional_with_anonymous_types(async);

            AssertSql(
                @"SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [g].[Nickname], [g].[FullName]
FROM [Gears] AS [g]
ORDER BY [g].[Nickname]");
        }

        public override async Task Where_conditional_equality_1(bool async)
        {
            await base.Where_conditional_equality_1(async);

            AssertSql(
                @"SELECT [g].[Nickname]
FROM [Gears] AS [g]
WHERE [g].[LeaderNickname] IS NULL
ORDER BY [g].[Nickname]");
        }

        public override async Task Where_conditional_equality_2(bool async)
        {
            await base.Where_conditional_equality_2(async);

            AssertSql(
                @"SELECT [g].[Nickname]
FROM [Gears] AS [g]
WHERE [g].[LeaderNickname] IS NULL
ORDER BY [g].[Nickname]");
        }

        public override async Task Where_conditional_equality_3(bool async)
        {
            await base.Where_conditional_equality_3(async);

            AssertSql(
                @"SELECT [g].[Nickname]
FROM [Gears] AS [g]
ORDER BY [g].[Nickname]");
        }

        public override async Task Select_coalesce_with_anonymous_types(bool async)
        {
            await base.Select_coalesce_with_anonymous_types(async);

            AssertSql(
                @"SELECT [g].[LeaderNickname], [g].[FullName]
FROM [Gears] AS [g]
ORDER BY [g].[Nickname]");
        }

        public override async Task Where_compare_anonymous_types(bool async)
        {
            await base.Where_compare_anonymous_types(async);

            AssertSql(
                " ");
        }

        public override async Task Where_member_access_on_anonymous_type(bool async)
        {
            await base.Where_member_access_on_anonymous_type(async);

            AssertSql(
                @"SELECT [g].[Nickname]
FROM [Gears] AS [g]
WHERE [g].[LeaderNickname] = N'Marcus'");
        }

        public override async Task Where_compare_anonymous_types_with_uncorrelated_members(bool async)
        {
            await base.Where_compare_anonymous_types_with_uncorrelated_members(async);

            AssertSql(
                @"SELECT [g].[Nickname]
FROM [Gears] AS [g]
WHERE 0 = 1");
        }

        public override async Task Select_Where_Navigation_Scalar_Equals_Navigation_Scalar(bool async)
        {
            await base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t0].[Id], [t0].[GearNickName], [t0].[GearSquadId], [t0].[IssueDate], [t0].[Note]
FROM [Tags] AS [t]
CROSS JOIN [Tags] AS [t0]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
LEFT JOIN [Gears] AS [g0] ON ([t0].[GearNickName] = [g0].[Nickname]) AND ([t0].[GearSquadId] = [g0].[SquadId])
WHERE ([g].[Nickname] = [g0].[Nickname]) OR ([g].[Nickname] IS NULL AND [g0].[Nickname] IS NULL)");
        }

        public override async Task Select_Singleton_Navigation_With_Member_Access(bool async)
        {
            await base.Select_Singleton_Navigation_With_Member_Access(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE ([g].[Nickname] = N'Marcus') AND (([g].[CityOfBirthName] <> N'Ephyra') OR [g].[CityOfBirthName] IS NULL)");
        }

        public override async Task Select_Where_Navigation(bool async)
        {
            await base.Select_Where_Navigation(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE [g].[Nickname] = N'Marcus'");
        }

        public override async Task Select_Where_Navigation_Equals_Navigation(bool async)
        {
            await base.Select_Where_Navigation_Equals_Navigation(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t0].[Id], [t0].[GearNickName], [t0].[GearSquadId], [t0].[IssueDate], [t0].[Note]
FROM [Tags] AS [t]
CROSS JOIN [Tags] AS [t0]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
LEFT JOIN [Gears] AS [g0] ON ([t0].[GearNickName] = [g0].[Nickname]) AND ([t0].[GearSquadId] = [g0].[SquadId])
WHERE (([g].[Nickname] = [g0].[Nickname]) OR ([g].[Nickname] IS NULL AND [g0].[Nickname] IS NULL)) AND (([g].[SquadId] = [g0].[SquadId]) OR ([g].[SquadId] IS NULL AND [g0].[SquadId] IS NULL))");
        }

        public override async Task Select_Where_Navigation_Null(bool async)
        {
            await base.Select_Where_Navigation_Null(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE [g].[Nickname] IS NULL OR [g].[SquadId] IS NULL");
        }

        public override async Task Select_Where_Navigation_Null_Reverse(bool async)
        {
            await base.Select_Where_Navigation_Null_Reverse(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE [g].[Nickname] IS NULL OR [g].[SquadId] IS NULL");
        }

        public override async Task Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected(bool async)
        {
            await base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected(async);

            AssertSql(
                @"SELECT [t].[Id] AS [Id1], [t0].[Id] AS [Id2]
FROM [Tags] AS [t]
CROSS JOIN [Tags] AS [t0]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
LEFT JOIN [Gears] AS [g0] ON ([t0].[GearNickName] = [g0].[Nickname]) AND ([t0].[GearSquadId] = [g0].[SquadId])
WHERE ([g].[Nickname] = [g0].[Nickname]) OR ([g].[Nickname] IS NULL AND [g0].[Nickname] IS NULL)");
        }

        public override async Task Optional_Navigation_Null_Coalesce_To_Clr_Type(bool async)
        {
            await base.Optional_Navigation_Null_Coalesce_To_Clr_Type(async);

            AssertSql(
                @"SELECT TOP(1) COALESCE([w0].[IsAutomatic], CAST(0 AS bit)) AS [IsAutomatic]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY [w].[Id]");
        }

        public override async Task Where_subquery_boolean(bool async)
        {
            await base.Where_subquery_boolean(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE COALESCE((
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]), CAST(0 AS bit)) = CAST(1 AS bit)");
        }

        public override async Task Where_subquery_boolean_with_pushdown(bool async)
        {
            await base.Where_subquery_boolean_with_pushdown(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE (
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]) = CAST(1 AS bit)");
        }

        public override async Task Where_subquery_distinct_firstordefault_boolean(bool async)
        {
            await base.Where_subquery_distinct_firstordefault_boolean(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[HasSoulPatch] = CAST(1 AS bit)) AND (COALESCE((
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
    ) AS [t]
    ORDER BY [t].[Id]), CAST(0 AS bit)) = CAST(1 AS bit))");
        }

        public override async Task Where_subquery_distinct_firstordefault_boolean_with_pushdown(bool async)
        {
            await base.Where_subquery_distinct_firstordefault_boolean_with_pushdown(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[HasSoulPatch] = CAST(1 AS bit)) AND ((
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
    ) AS [t]
    ORDER BY [t].[Id]) = CAST(1 AS bit))");
        }

        public override async Task Where_subquery_distinct_first_boolean(bool async)
        {
            await base.Where_subquery_distinct_first_boolean(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[HasSoulPatch] = CAST(1 AS bit)) AND ((
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
    ) AS [t]
    ORDER BY [t].[Id]) = CAST(1 AS bit))
ORDER BY [g].[Nickname]");
        }

        public override async Task Where_subquery_distinct_singleordefault_boolean1(bool async)
        {
            await base.Where_subquery_distinct_singleordefault_boolean1(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[HasSoulPatch] = CAST(1 AS bit)) AND (COALESCE((
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE ([g].[FullName] = [w].[OwnerFullName]) AND ([w].[Name] LIKE N'%Lancer%')
    ) AS [t]), CAST(0 AS bit)) = CAST(1 AS bit))
ORDER BY [g].[Nickname]");
        }

        public override async Task Where_subquery_distinct_singleordefault_boolean2(bool async)
        {
            await base.Where_subquery_distinct_singleordefault_boolean2(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[HasSoulPatch] = CAST(1 AS bit)) AND (COALESCE((
    SELECT DISTINCT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE ([g].[FullName] = [w].[OwnerFullName]) AND ([w].[Name] LIKE N'%Lancer%')), CAST(0 AS bit)) = CAST(1 AS bit))
ORDER BY [g].[Nickname]");
        }

        public override async Task Where_subquery_distinct_singleordefault_boolean_with_pushdown(bool async)
        {
            await base.Where_subquery_distinct_singleordefault_boolean_with_pushdown(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[HasSoulPatch] = CAST(1 AS bit)) AND ((
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE ([g].[FullName] = [w].[OwnerFullName]) AND ([w].[Name] LIKE N'%Lancer%')
    ) AS [t]) = CAST(1 AS bit))
ORDER BY [g].[Nickname]");
        }

        public override async Task Where_subquery_distinct_lastordefault_boolean(bool async)
        {
            await base.Where_subquery_distinct_lastordefault_boolean(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE (
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
    ) AS [t]
    ORDER BY [t].[Id] DESC) = CAST(0 AS bit)
ORDER BY [g].[Nickname]");
        }

        public override async Task Where_subquery_distinct_last_boolean(bool async)
        {
            await base.Where_subquery_distinct_last_boolean(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[HasSoulPatch] = CAST(0 AS bit)) AND ((
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
    ) AS [t]
    ORDER BY [t].[Id] DESC) = CAST(1 AS bit))
ORDER BY [g].[Nickname]");
        }

        public override async Task Where_subquery_distinct_orderby_firstordefault_boolean(bool async)
        {
            await base.Where_subquery_distinct_orderby_firstordefault_boolean(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[HasSoulPatch] = CAST(1 AS bit)) AND (COALESCE((
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
    ) AS [t]
    ORDER BY [t].[Id]), CAST(0 AS bit)) = CAST(1 AS bit))");
        }

        public override async Task Where_subquery_distinct_orderby_firstordefault_boolean_with_pushdown(bool async)
        {
            await base.Where_subquery_distinct_orderby_firstordefault_boolean_with_pushdown(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[HasSoulPatch] = CAST(1 AS bit)) AND ((
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
    ) AS [t]
    ORDER BY [t].[Id]) = CAST(1 AS bit))");
        }

        public override async Task Where_subquery_union_firstordefault_boolean(bool async)
        {
            await base.Where_subquery_union_firstordefault_boolean(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[HasSoulPatch] = CAST(1 AS bit)) AND ((
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
        UNION
        SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
        FROM [Weapons] AS [w0]
        WHERE [g].[FullName] = [w0].[OwnerFullName]
    ) AS [t]
    ORDER BY [t].[Id]) = CAST(1 AS bit))");
        }

        public override async Task Where_subquery_join_firstordefault_boolean(bool async)
        {
            await base.Where_subquery_join_firstordefault_boolean(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[HasSoulPatch] = CAST(1 AS bit)) AND ((
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    INNER JOIN (
        SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
        FROM [Weapons] AS [w0]
        WHERE [g].[FullName] = [w0].[OwnerFullName]
    ) AS [t] ON [w].[Id] = [t].[Id]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]) = CAST(1 AS bit))");
        }

        public override async Task Where_subquery_left_join_firstordefault_boolean(bool async)
        {
            await base.Where_subquery_left_join_firstordefault_boolean(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[HasSoulPatch] = CAST(1 AS bit)) AND ((
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    LEFT JOIN (
        SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
        FROM [Weapons] AS [w0]
        WHERE [g].[FullName] = [w0].[OwnerFullName]
    ) AS [t] ON [w].[Id] = [t].[Id]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]) = CAST(1 AS bit))");
        }

        public override async Task Where_subquery_concat_firstordefault_boolean(bool async)
        {
            await base.Where_subquery_concat_firstordefault_boolean(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[HasSoulPatch] = CAST(1 AS bit)) AND ((
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
        UNION ALL
        SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
        FROM [Weapons] AS [w0]
        WHERE [g].[FullName] = [w0].[OwnerFullName]
    ) AS [t]
    ORDER BY [t].[Id]) = CAST(1 AS bit))");
        }

        public override async Task Concat_with_count(bool async)
        {
            await base.Concat_with_count(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
    FROM [Gears] AS [g0]
) AS [t]");
        }

        public override async Task Concat_scalars_with_count(bool async)
        {
            await base.Concat_scalars_with_count(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT [g].[Nickname]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [g0].[FullName] AS [Nickname]
    FROM [Gears] AS [g0]
) AS [t]");
        }

        public override async Task Concat_anonymous_with_count(bool async)
        {
            await base.Concat_anonymous_with_count(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g].[Nickname] AS [Name]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], [g0].[FullName] AS [Name]
    FROM [Gears] AS [g0]
) AS [t]");
        }

        public override async Task Concat_with_scalar_projection(bool async)
        {
            await base.Concat_with_scalar_projection(async);

            AssertSql(
                @"SELECT [t].[Nickname]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
    FROM [Gears] AS [g0]
) AS [t]");
        }

        public override async Task Select_navigation_with_concat_and_count(bool async)
        {
            await base.Select_navigation_with_concat_and_count(async);

            AssertSql(
                @"SELECT (
    SELECT COUNT(*)
    FROM (
        SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
        UNION ALL
        SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
        FROM [Weapons] AS [w0]
        WHERE [g].[FullName] = [w0].[OwnerFullName]
    ) AS [t])
FROM [Gears] AS [g]
WHERE [g].[HasSoulPatch] = CAST(0 AS bit)");
        }

        public override async Task Concat_with_collection_navigations(bool async)
        {
            await base.Concat_with_collection_navigations(async);

            AssertSql(
                @"SELECT (
    SELECT COUNT(*)
    FROM (
        SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
        UNION
        SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
        FROM [Weapons] AS [w0]
        WHERE [g].[FullName] = [w0].[OwnerFullName]
    ) AS [t])
FROM [Gears] AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit)");
        }

        public override async Task Union_with_collection_navigations(bool async)
        {
            await base.Union_with_collection_navigations(async);

            AssertSql(
                @"SELECT (
    SELECT COUNT(*)
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
        FROM [Gears] AS [g0]
        WHERE ([g].[Nickname] = [g0].[LeaderNickname]) AND ([g].[SquadId] = [g0].[LeaderSquadId])
        UNION
        SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOfBirthName], [g1].[Discriminator], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[Rank]
        FROM [Gears] AS [g1]
        WHERE ([g].[Nickname] = [g1].[LeaderNickname]) AND ([g].[SquadId] = [g1].[LeaderSquadId])
    ) AS [t])
FROM [Gears] AS [g]
WHERE [g].[Discriminator] = N'Officer'");
        }

        public override async Task Select_subquery_distinct_firstordefault(bool async)
        {
            await base.Select_subquery_distinct_firstordefault(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [t].[Name]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
    ) AS [t]
    ORDER BY [t].[Id])
FROM [Gears] AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit)");
        }

        public override async Task Singleton_Navigation_With_Member_Access(bool async)
        {
            await base.Singleton_Navigation_With_Member_Access(async);

            AssertSql(
                @"SELECT [g].[CityOfBirthName] AS [B]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE ([g].[Nickname] = N'Marcus') AND (([g].[CityOfBirthName] <> N'Ephyra') OR [g].[CityOfBirthName] IS NULL)");
        }

        public override async Task GroupJoin_Composite_Key(bool async)
        {
            await base.GroupJoin_Composite_Key(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Tags] AS [t]
INNER JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])");
        }

        public override async Task Join_navigation_translated_to_subquery_composite_key(bool async)
        {
            await base.Join_navigation_translated_to_subquery_composite_key(async);

            AssertSql(
                @"SELECT [g].[FullName], [t0].[Note]
FROM [Gears] AS [g]
INNER JOIN (
    SELECT [t].[Note], [g0].[FullName]
    FROM [Tags] AS [t]
    LEFT JOIN [Gears] AS [g0] ON ([t].[GearNickName] = [g0].[Nickname]) AND ([t].[GearSquadId] = [g0].[SquadId])
) AS [t0] ON [g].[FullName] = [t0].[FullName]");
        }

        public override async Task Join_with_order_by_on_inner_sequence_navigation_translated_to_subquery_composite_key(bool async)
        {
            await base.Join_with_order_by_on_inner_sequence_navigation_translated_to_subquery_composite_key(async);

            AssertSql(
                @"SELECT [g].[FullName], [t0].[Note]
FROM [Gears] AS [g]
INNER JOIN (
    SELECT [t].[Note], [g0].[FullName]
    FROM [Tags] AS [t]
    LEFT JOIN [Gears] AS [g0] ON ([t].[GearNickName] = [g0].[Nickname]) AND ([t].[GearSquadId] = [g0].[SquadId])
) AS [t0] ON [g].[FullName] = [t0].[FullName]");
        }

        public override async Task Join_with_order_by_without_skip_or_take(bool async)
        {
            await base.Join_with_order_by_without_skip_or_take(async);

            AssertSql(
                @"SELECT [t].[Name], [g].[FullName]
FROM [Gears] AS [g]
INNER JOIN (
    SELECT [w].[Name], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]");
        }

        public override async Task Join_with_order_by_without_skip_or_take_nested(bool async)
        {
            await base.Join_with_order_by_without_skip_or_take_nested(async);

            AssertSql(
                @"SELECT [t0].[Name], [t].[FullName]
FROM [Squads] AS [s]
INNER JOIN (
    SELECT [g].[SquadId], [g].[FullName]
    FROM [Gears] AS [g]
) AS [t] ON [s].[Id] = [t].[SquadId]
INNER JOIN (
    SELECT [w].[Name], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
) AS [t0] ON [t].[FullName] = [t0].[OwnerFullName]");
        }

        public override async Task Collection_with_inheritance_and_join_include_joined(bool async)
        {
            await base.Collection_with_inheritance_and_join_include_joined(async);

            AssertSql(
                @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t1].[Id], [t1].[GearNickName], [t1].[GearSquadId], [t1].[IssueDate], [t1].[Note]
FROM [Tags] AS [t]
INNER JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
    FROM [Gears] AS [g]
    WHERE [g].[Discriminator] = N'Officer'
) AS [t0] ON ([t].[GearSquadId] = [t0].[SquadId]) AND ([t].[GearNickName] = [t0].[Nickname])
LEFT JOIN [Tags] AS [t1] ON ([t0].[Nickname] = [t1].[GearNickName]) AND ([t0].[SquadId] = [t1].[GearSquadId])");
        }

        public override async Task Collection_with_inheritance_and_join_include_source(bool async)
        {
            await base.Collection_with_inheritance_and_join_include_source(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t0].[Id], [t0].[GearNickName], [t0].[GearSquadId], [t0].[IssueDate], [t0].[Note]
FROM [Gears] AS [g]
INNER JOIN [Tags] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
LEFT JOIN [Tags] AS [t0] ON ([g].[Nickname] = [t0].[GearNickName]) AND ([g].[SquadId] = [t0].[GearSquadId])
WHERE [g].[Discriminator] = N'Officer'");
        }

        public override async Task Non_unicode_string_literal_is_used_for_non_unicode_column(bool async)
        {
            await base.Non_unicode_string_literal_is_used_for_non_unicode_column(async);

            AssertSql(
                @"SELECT [c].[Name], [c].[Location], [c].[Nation]
FROM [Cities] AS [c]
WHERE [c].[Location] = 'Unknown'");
        }

        public override async Task Non_unicode_string_literal_is_used_for_non_unicode_column_right(bool async)
        {
            await base.Non_unicode_string_literal_is_used_for_non_unicode_column_right(async);

            AssertSql(
                @"SELECT [c].[Name], [c].[Location], [c].[Nation]
FROM [Cities] AS [c]
WHERE 'Unknown' = [c].[Location]");
        }

        public override async Task Non_unicode_parameter_is_used_for_non_unicode_column(bool async)
        {
            await base.Non_unicode_parameter_is_used_for_non_unicode_column(async);

            AssertSql(
                @"@__value_0='Unknown' (Size = 100) (DbType = AnsiString)

SELECT [c].[Name], [c].[Location], [c].[Nation]
FROM [Cities] AS [c]
WHERE [c].[Location] = @__value_0");
        }

        public override async Task Non_unicode_string_literals_in_contains_is_used_for_non_unicode_column(bool async)
        {
            await base.Non_unicode_string_literals_in_contains_is_used_for_non_unicode_column(async);

            AssertSql(
                @"SELECT [c].[Name], [c].[Location], [c].[Nation]
FROM [Cities] AS [c]
WHERE [c].[Location] IN ('Unknown', 'Jacinto''s location', 'Ephyra''s location')");
        }

        public override async Task Non_unicode_string_literals_is_used_for_non_unicode_column_with_subquery(bool async)
        {
            await base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_subquery(async);

            AssertSql(
                @"SELECT [c].[Name], [c].[Location], [c].[Nation]
FROM [Cities] AS [c]
WHERE ([c].[Location] = 'Unknown') AND ((
    SELECT COUNT(*)
    FROM [Gears] AS [g]
    WHERE ([c].[Name] = [g].[CityOfBirthName]) AND ([g].[Nickname] = N'Paduk')) = 1)");
        }

        public override async Task Non_unicode_string_literals_is_used_for_non_unicode_column_in_subquery(bool async)
        {
            await base.Non_unicode_string_literals_is_used_for_non_unicode_column_in_subquery(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
INNER JOIN [Cities] AS [c] ON [g].[CityOfBirthName] = [c].[Name]
WHERE ([g].[Nickname] = N'Marcus') AND ([c].[Location] = 'Jacinto''s location')");
        }

        public override async Task Non_unicode_string_literals_is_used_for_non_unicode_column_with_contains(bool async)
        {
            await base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_contains(async);

            AssertSql(
                @"SELECT [c].[Name], [c].[Location], [c].[Nation]
FROM [Cities] AS [c]
WHERE [c].[Location] LIKE '%Jacinto%'");
        }

        public override async Task Non_unicode_string_literals_is_used_for_non_unicode_column_with_concat(bool async)
        {
            await base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_concat(async);

            AssertSql(
                @"SELECT [c].[Name], [c].[Location], [c].[Nation]
FROM [Cities] AS [c]
WHERE COALESCE([c].[Location], '') + 'Added' LIKE '%Add%'");
        }

        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1();

            // Issue#16897
            AssertSql(
                @"SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Gears] AS [g]
LEFT JOIN [Gears] AS [g0] ON [g].[LeaderNickname] = [g0].[Nickname]
LEFT JOIN [Weapons] AS [w] ON [g].[FullName] = [w].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId], [w].[Id]");
        }

        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2();

            // Issue#16897
            AssertSql(
                @"SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], [g].[Nickname], [g].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Gears] AS [g0] ON [g].[LeaderNickname] = [g0].[Nickname]
LEFT JOIN [Weapons] AS [w] ON [g0].[FullName] = [w].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId], [w].[Id]");
        }

        public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3(bool async)
        {
            await base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3(async);

            // Issue#16897
            AssertSql(
                @"SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], [g].[Nickname], [g].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Gears] AS [g]
LEFT JOIN [Gears] AS [g0] ON [g].[LeaderNickname] = [g0].[Nickname]
LEFT JOIN [Weapons] AS [w] ON [g0].[FullName] = [w].[OwnerFullName]
LEFT JOIN [Weapons] AS [w0] ON [g].[FullName] = [w0].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId], [w].[Id], [w0].[Id]");
        }

        public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result4(bool async)
        {
            await base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result4(async);

            // Issue#16897
            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [g2].*
    FROM [Gears] AS [g2]
    WHERE [g2].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [g].[LeaderNickname] = [t].[Nickname]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[FullName], [t].[FullName]",
                //
                @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT DISTINCT [g0].[FullName]
    FROM [Gears] AS [g0]
    LEFT JOIN (
        SELECT [g20].*
        FROM [Gears] AS [g20]
        WHERE [g20].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t0] ON [g0].[LeaderNickname] = [t0].[Nickname]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t1] ON [g.Weapons].[OwnerFullName] = [t1].[FullName]
ORDER BY [t1].[FullName]",
                //
                @"SELECT [g2.Weapons].[Id], [g2.Weapons].[AmmunitionType], [g2.Weapons].[IsAutomatic], [g2.Weapons].[Name], [g2.Weapons].[OwnerFullName], [g2.Weapons].[SynergyWithId]
FROM [Weapons] AS [g2.Weapons]
INNER JOIN (
    SELECT DISTINCT [t2].[FullName], [g1].[FullName] AS [FullName0]
    FROM [Gears] AS [g1]
    LEFT JOIN (
        SELECT [g21].*
        FROM [Gears] AS [g21]
        WHERE [g21].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t2] ON [g1].[LeaderNickname] = [t2].[Nickname]
    WHERE [g1].[Discriminator] IN (N'Officer', N'Gear')
) AS [t3] ON [g2.Weapons].[OwnerFullName] = [t3].[FullName]
ORDER BY [t3].[FullName0], [t3].[FullName]");
        }

        public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result(bool async)
        {
            await base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result(async);

            // Issue#16897
            AssertSql(
                @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [g].[Nickname], [g].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] = N'Officer'
) AS [t] ON [g].[LeaderNickname] = [t].[Nickname]
LEFT JOIN [Weapons] AS [w] ON [t].[FullName] = [w].[OwnerFullName]
LEFT JOIN [Weapons] AS [w0] ON [g].[FullName] = [w0].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Nickname], [t].[SquadId], [w].[Id], [w0].[Id]");
        }

        public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result(bool async)
        {
            await base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result(async);

            // Issue#16897
            AssertSql(
                @"SELECT CASE
    WHEN [g0].[Nickname] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Gears] AS [g]
LEFT JOIN [Gears] AS [g0] ON [g].[LeaderNickname] = [g0].[Nickname]
LEFT JOIN [Weapons] AS [w] ON [g0].[FullName] = [w].[OwnerFullName]
LEFT JOIN [Weapons] AS [w0] ON [g].[FullName] = [w0].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [w].[Id], [w0].[Id]");
        }

        public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result(bool async)
        {
            await base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [g2].*
    FROM [Gears] AS [g2]
    WHERE [g2].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [g].[LeaderNickname] = [t].[Nickname]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[FullName], [t].[FullName]",
                //
                @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT DISTINCT [g0].[FullName]
    FROM [Gears] AS [g0]
    LEFT JOIN (
        SELECT [g20].*
        FROM [Gears] AS [g20]
        WHERE [g20].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t0] ON [g0].[LeaderNickname] = [t0].[Nickname]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND ([g0].[Nickname] IS NOT NULL AND [t0].[Nickname] IS NULL)
) AS [t1] ON [g.Weapons].[OwnerFullName] = [t1].[FullName]
ORDER BY [t1].[FullName]",
                //
                @"SELECT [g2.Weapons].[Id], [g2.Weapons].[AmmunitionType], [g2.Weapons].[IsAutomatic], [g2.Weapons].[Name], [g2.Weapons].[OwnerFullName], [g2.Weapons].[SynergyWithId]
FROM [Weapons] AS [g2.Weapons]
INNER JOIN (
    SELECT DISTINCT [t2].[FullName], [g1].[FullName] AS [FullName0]
    FROM [Gears] AS [g1]
    LEFT JOIN (
        SELECT [g21].*
        FROM [Gears] AS [g21]
        WHERE [g21].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t2] ON [g1].[LeaderNickname] = [t2].[Nickname]
    WHERE [g1].[Discriminator] IN (N'Officer', N'Gear') AND [t2].[Nickname] IS NOT NULL
) AS [t3] ON [g2.Weapons].[OwnerFullName] = [t3].[FullName]
ORDER BY [t3].[FullName0], [t3].[FullName]");
        }

        public override async Task Coalesce_operator_in_predicate(bool async)
        {
            await base.Coalesce_operator_in_predicate(async);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE COALESCE([w].[IsAutomatic], CAST(0 AS bit)) = CAST(1 AS bit)");
        }

        public override async Task Coalesce_operator_in_predicate_with_other_conditions(bool async)
        {
            await base.Coalesce_operator_in_predicate_with_other_conditions(async);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] = 1) AND (COALESCE([w].[IsAutomatic], CAST(0 AS bit)) = CAST(1 AS bit))");
        }

        public override async Task Coalesce_operator_in_projection_with_other_conditions(bool async)
        {
            await base.Coalesce_operator_in_projection_with_other_conditions(async);

            AssertSql(
                @"SELECT CASE
    WHEN (([w].[AmmunitionType] = 1) AND [w].[AmmunitionType] IS NOT NULL) AND (COALESCE([w].[IsAutomatic], CAST(0 AS bit)) = CAST(1 AS bit)) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Weapons] AS [w]");
        }

        public override async Task Optional_navigation_type_compensation_works_with_predicate(bool async)
        {
            await base.Optional_navigation_type_compensation_works_with_predicate(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE (([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL) AND ([g].[HasSoulPatch] = CAST(1 AS bit))");
        }

        public override async Task Optional_navigation_type_compensation_works_with_predicate2(bool async)
        {
            await base.Optional_navigation_type_compensation_works_with_predicate2(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE [g].[HasSoulPatch] = CAST(1 AS bit)");
        }

        public override async Task Optional_navigation_type_compensation_works_with_predicate_negated(bool async)
        {
            await base.Optional_navigation_type_compensation_works_with_predicate_negated(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE [g].[HasSoulPatch] = CAST(0 AS bit)");
        }

        public override async Task Optional_navigation_type_compensation_works_with_predicate_negated_complex1(bool async)
        {
            await base.Optional_navigation_type_compensation_works_with_predicate_negated_complex1(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE CASE
    WHEN [g].[HasSoulPatch] = CAST(1 AS bit) THEN CAST(1 AS bit)
    ELSE [g].[HasSoulPatch]
END = CAST(0 AS bit)");
        }

        public override async Task Optional_navigation_type_compensation_works_with_predicate_negated_complex2(bool async)
        {
            await base.Optional_navigation_type_compensation_works_with_predicate_negated_complex2(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE CASE
    WHEN [g].[HasSoulPatch] = CAST(0 AS bit) THEN CAST(0 AS bit)
    ELSE [g].[HasSoulPatch]
END = CAST(0 AS bit)");
        }

        public override async Task Optional_navigation_type_compensation_works_with_conditional_expression(bool async)
        {
            await base.Optional_navigation_type_compensation_works_with_conditional_expression(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE CASE
    WHEN [g].[HasSoulPatch] = CAST(1 AS bit) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CAST(1 AS bit)");
        }

        public override async Task Optional_navigation_type_compensation_works_with_binary_expression(bool async)
        {
            await base.Optional_navigation_type_compensation_works_with_binary_expression(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE ([g].[HasSoulPatch] = CAST(1 AS bit)) OR ([t].[Note] LIKE N'%Cole%')");
        }

        public override async Task Optional_navigation_type_compensation_works_with_binary_and_expression(bool async)
        {
            await base.Optional_navigation_type_compensation_works_with_binary_and_expression(async);

            AssertSql(
                @"SELECT CASE
    WHEN ([g].[HasSoulPatch] = CAST(1 AS bit)) AND ([t].[Note] LIKE N'%Cole%') THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])");
        }

        public override async Task Optional_navigation_type_compensation_works_with_projection(bool async)
        {
            await base.Optional_navigation_type_compensation_works_with_projection(async);

            AssertSql(
                @"SELECT [g].[SquadId]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL");
        }

        public override async Task Optional_navigation_type_compensation_works_with_projection_into_anonymous_type(bool async)
        {
            await base.Optional_navigation_type_compensation_works_with_projection_into_anonymous_type(async);

            AssertSql(
                @"SELECT [g].[SquadId]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL");
        }

        public override async Task Optional_navigation_type_compensation_works_with_DTOs(bool async)
        {
            await base.Optional_navigation_type_compensation_works_with_DTOs(async);

            AssertSql(
                @"SELECT [g].[SquadId] AS [Id]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL");
        }

        public override async Task Optional_navigation_type_compensation_works_with_list_initializers(bool async)
        {
            await base.Optional_navigation_type_compensation_works_with_list_initializers(async);

            AssertSql(
                @"SELECT [g].[SquadId], [g].[SquadId] + 1
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL
ORDER BY [t].[Note]");
        }

        public override async Task Optional_navigation_type_compensation_works_with_array_initializers(bool async)
        {
            await base.Optional_navigation_type_compensation_works_with_array_initializers(async);

            AssertSql(
                @"SELECT [g].[SquadId]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL");
        }

        public override async Task Optional_navigation_type_compensation_works_with_orderby(bool async)
        {
            await base.Optional_navigation_type_compensation_works_with_orderby(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL
ORDER BY [g].[SquadId]");
        }

        public override async Task Optional_navigation_type_compensation_works_with_all(bool async)
        {
            await base.Optional_navigation_type_compensation_works_with_all(async);

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Tags] AS [t]
        LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
        WHERE (([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL) AND ([g].[HasSoulPatch] = CAST(0 AS bit))) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Optional_navigation_type_compensation_works_with_negated_predicate(bool async)
        {
            await base.Optional_navigation_type_compensation_works_with_negated_predicate(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE (([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL) AND ([g].[HasSoulPatch] = CAST(0 AS bit))");
        }

        public override async Task Optional_navigation_type_compensation_works_with_contains(bool async)
        {
            await base.Optional_navigation_type_compensation_works_with_contains(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE (([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL) AND EXISTS (
    SELECT 1
    FROM [Gears] AS [g0]
    WHERE [g0].[SquadId] = [g].[SquadId])");
        }

        public override async Task Optional_navigation_type_compensation_works_with_skip(bool async)
        {
            await base.Optional_navigation_type_compensation_works_with_skip(async);

            AssertSql(
                @"SELECT [t0].[SquadId]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL
ORDER BY [t].[Note]",
                //
                @"@_outer_SquadId='1'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]
OFFSET @_outer_SquadId ROWS",
                //
                @"@_outer_SquadId='1'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]
OFFSET @_outer_SquadId ROWS",
                //
                @"@_outer_SquadId='1'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]
OFFSET @_outer_SquadId ROWS",
                //
                @"@_outer_SquadId='1'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]
OFFSET @_outer_SquadId ROWS",
                //
                @"@_outer_SquadId='2'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]
OFFSET @_outer_SquadId ROWS");
        }

        public override async Task Optional_navigation_type_compensation_works_with_take(bool async)
        {
            await base.Optional_navigation_type_compensation_works_with_take(async);

            AssertSql(
                @"SELECT [t0].[SquadId]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL
ORDER BY [t].[Note]",
                //
                @"@_outer_SquadId='1'

SELECT TOP(@_outer_SquadId) [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]",
                //
                @"@_outer_SquadId='1'

SELECT TOP(@_outer_SquadId) [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]",
                //
                @"@_outer_SquadId='1'

SELECT TOP(@_outer_SquadId) [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]",
                //
                @"@_outer_SquadId='1'

SELECT TOP(@_outer_SquadId) [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]",
                //
                @"@_outer_SquadId='2'

SELECT TOP(@_outer_SquadId) [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]");
        }

        public override async Task Select_correlated_filtered_collection(bool async)
        {
            await base.Select_correlated_filtered_collection(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [c].[Name], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[SynergyWithId]
FROM [Gears] AS [g]
INNER JOIN [Cities] AS [c] ON [g].[CityOfBirthName] = [c].[Name]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
    FROM [Weapons] AS [w]
    WHERE ([w].[Name] <> N'Lancer') OR [w].[Name] IS NULL
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
WHERE [c].[Name] IN (N'Ephyra', N'Hanover')
ORDER BY [g].[Nickname], [g].[SquadId], [c].[Name], [t].[Id]");
        }

        public override async Task Select_correlated_filtered_collection_with_composite_key(bool async)
        {
            await base.Select_correlated_filtered_collection_with_composite_key(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
    FROM [Gears] AS [g0]
    WHERE [g0].[Nickname] <> N'Dom'
) AS [t] ON ([g].[Nickname] = [t].[LeaderNickname]) AND ([g].[SquadId] = [t].[LeaderSquadId])
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Nickname], [t].[SquadId]");
        }

        public override async Task Select_correlated_filtered_collection_works_with_caching(bool async)
        {
            await base.Select_correlated_filtered_collection_works_with_caching(async);

            AssertSql(
                @"SELECT [t].[Id], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON [t].[GearNickName] = [g].[Nickname]
ORDER BY [t].[Note], [t].[Id], [g].[Nickname], [g].[SquadId]");
        }

        public override async Task Join_predicate_value_equals_condition(bool async)
        {
            await base.Join_predicate_value_equals_condition(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
INNER JOIN [Weapons] AS [w] ON [w].[SynergyWithId] IS NOT NULL");
        }

        public override async Task Join_predicate_value(bool async)
        {
            await base.Join_predicate_value(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
INNER JOIN [Weapons] AS [w] ON [g].[HasSoulPatch] = CAST(1 AS bit)");
        }

        public override async Task Join_predicate_condition_equals_condition(bool async)
        {
            await base.Join_predicate_condition_equals_condition(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
INNER JOIN [Weapons] AS [w] ON [w].[SynergyWithId] IS NOT NULL");
        }

        public override async Task Left_join_predicate_value_equals_condition(bool async)
        {
            await base.Left_join_predicate_value_equals_condition(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Weapons] AS [w] ON [w].[SynergyWithId] IS NOT NULL");
        }

        public override async Task Left_join_predicate_value(bool async)
        {
            await base.Left_join_predicate_value(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Weapons] AS [w] ON [g].[HasSoulPatch] = CAST(1 AS bit)");
        }

        public override async Task Left_join_predicate_condition_equals_condition(bool async)
        {
            await base.Left_join_predicate_condition_equals_condition(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Weapons] AS [w] ON [w].[SynergyWithId] IS NOT NULL");
        }

        public override async Task Where_datetimeoffset_now(bool async)
        {
            await base.Where_datetimeoffset_now(async);

            AssertSql(
                @"SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE [m].[Timeline] <> SYSDATETIMEOFFSET()");
        }

        public override async Task Where_datetimeoffset_utcnow(bool async)
        {
            await base.Where_datetimeoffset_utcnow(async);

            AssertSql(
                @"SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE [m].[Timeline] <> CAST(SYSUTCDATETIME() AS datetimeoffset)");
        }

        public override async Task Where_datetimeoffset_date_component(bool async)
        {
            await base.Where_datetimeoffset_date_component(async);

            AssertSql(
                @"@__Date_0='0001-01-01T00:00:00.0000000'

SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE CONVERT(date, [m].[Timeline]) > @__Date_0");
        }

        public override async Task Where_datetimeoffset_year_component(bool async)
        {
            await base.Where_datetimeoffset_year_component(async);

            AssertSql(
                @"SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(year, [m].[Timeline]) = 2");
        }

        public override async Task Where_datetimeoffset_month_component(bool async)
        {
            await base.Where_datetimeoffset_month_component(async);

            AssertSql(
                @"SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(month, [m].[Timeline]) = 1");
        }

        public override async Task Where_datetimeoffset_dayofyear_component(bool async)
        {
            await base.Where_datetimeoffset_dayofyear_component(async);

            AssertSql(
                @"SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(dayofyear, [m].[Timeline]) = 2");
        }

        public override async Task Where_datetimeoffset_day_component(bool async)
        {
            await base.Where_datetimeoffset_day_component(async);

            AssertSql(
                @"SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(day, [m].[Timeline]) = 2");
        }

        public override async Task Where_datetimeoffset_hour_component(bool async)
        {
            await base.Where_datetimeoffset_hour_component(async);

            AssertSql(
                @"SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(hour, [m].[Timeline]) = 10");
        }

        public override async Task Where_datetimeoffset_minute_component(bool async)
        {
            await base.Where_datetimeoffset_minute_component(async);

            AssertSql(
                @"SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(minute, [m].[Timeline]) = 0");
        }

        public override async Task Where_datetimeoffset_second_component(bool async)
        {
            await base.Where_datetimeoffset_second_component(async);

            AssertSql(
                @"SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(second, [m].[Timeline]) = 0");
        }

        public override async Task Where_datetimeoffset_millisecond_component(bool async)
        {
            await base.Where_datetimeoffset_millisecond_component(async);

            AssertSql(
                @"SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(millisecond, [m].[Timeline]) = 0");
        }

        public override async Task DateTimeOffset_DateAdd_AddMonths(bool async)
        {
            await base.DateTimeOffset_DateAdd_AddMonths(async);

            AssertSql(
                @"SELECT DATEADD(month, CAST(1 AS int), [m].[Timeline])
FROM [Missions] AS [m]");
        }

        public override async Task DateTimeOffset_DateAdd_AddDays(bool async)
        {
            await base.DateTimeOffset_DateAdd_AddDays(async);

            AssertSql(
                @"SELECT DATEADD(day, CAST(1.0E0 AS int), [m].[Timeline])
FROM [Missions] AS [m]");
        }

        public override async Task DateTimeOffset_DateAdd_AddHours(bool async)
        {
            await base.DateTimeOffset_DateAdd_AddHours(async);

            AssertSql(
                @"SELECT DATEADD(hour, CAST(1.0E0 AS int), [m].[Timeline])
FROM [Missions] AS [m]");
        }

        public override async Task DateTimeOffset_DateAdd_AddMinutes(bool async)
        {
            await base.DateTimeOffset_DateAdd_AddMinutes(async);

            AssertSql(
                @"SELECT DATEADD(minute, CAST(1.0E0 AS int), [m].[Timeline])
FROM [Missions] AS [m]");
        }

        public override async Task DateTimeOffset_DateAdd_AddSeconds(bool async)
        {
            await base.DateTimeOffset_DateAdd_AddSeconds(async);

            AssertSql(
                @"SELECT DATEADD(second, CAST(1.0E0 AS int), [m].[Timeline])
FROM [Missions] AS [m]");
        }

        public override async Task DateTimeOffset_DateAdd_AddMilliseconds(bool async)
        {
            await base.DateTimeOffset_DateAdd_AddMilliseconds(async);

            AssertSql(
                @"SELECT DATEADD(millisecond, CAST(300.0E0 AS int), [m].[Timeline])
FROM [Missions] AS [m]");
        }

        public override async Task Where_datetimeoffset_milliseconds_parameter_and_constant(bool async)
        {
            await base.Where_datetimeoffset_milliseconds_parameter_and_constant(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Missions] AS [m]
WHERE [m].[Timeline] = '1902-01-02T10:00:00.1234567+01:30'");
        }

        public override async Task Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used(
            bool async)
        {
            await base.Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gears] AS [g]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname]
ORDER BY [t].[GearNickName]");
        }

        public override async Task Complex_predicate_with_AndAlso_and_nullable_bool_property(bool async)
        {
            await base.Complex_predicate_with_AndAlso_and_nullable_bool_property(async);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Gears] AS [g] ON [w].[OwnerFullName] = [g].[FullName]
WHERE ([w].[Id] <> 50) AND ([g].[HasSoulPatch] = CAST(0 AS bit))");
        }

        public override async Task Distinct_with_optional_navigation_is_translated_to_sql(bool async)
        {
            await base.Distinct_with_optional_navigation_is_translated_to_sql(async);

            AssertSql(
                @"SELECT DISTINCT [g].[HasSoulPatch]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
WHERE ([t].[Note] <> N'Foo') OR [t].[Note] IS NULL");
        }

        public override async Task Sum_with_optional_navigation_is_translated_to_sql(bool async)
        {
            await base.Sum_with_optional_navigation_is_translated_to_sql(async);

            AssertSql(
                @"SELECT COALESCE(SUM([g].[SquadId]), 0)
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
WHERE ([t].[Note] <> N'Foo') OR [t].[Note] IS NULL");
        }

        public override async Task Count_with_optional_navigation_is_translated_to_sql(bool async)
        {
            await base.Count_with_optional_navigation_is_translated_to_sql(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
WHERE ([t].[Note] <> N'Foo') OR [t].[Note] IS NULL");
        }

        public override async Task FirstOrDefault_with_manually_created_groupjoin_is_translated_to_sql(bool async)
        {
            await base.FirstOrDefault_with_manually_created_groupjoin_is_translated_to_sql(async);

            AssertSql(
                @"SELECT TOP(1) [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
LEFT JOIN [Gears] AS [g] ON [s].[Id] = [g].[SquadId]
WHERE [s].[Name] = N'Kilo'");
        }

        public override async Task Any_with_optional_navigation_as_subquery_predicate_is_translated_to_sql(bool async)
        {
            await base.Any_with_optional_navigation_as_subquery_predicate_is_translated_to_sql(async);

            AssertSql(
                @"SELECT [s].[Name]
FROM [Squads] AS [s]
WHERE NOT (EXISTS (
    SELECT 1
    FROM [Gears] AS [g]
    LEFT JOIN [Tags] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
    WHERE ([s].[Id] = [g].[SquadId]) AND ([t].[Note] = N'Dom''s Tag')))");
        }

        public override async Task All_with_optional_navigation_is_translated_to_sql(bool async)
        {
            await base.All_with_optional_navigation_is_translated_to_sql(async);

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Gears] AS [g]
        LEFT JOIN [Tags] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
        WHERE ([t].[Note] = N'Foo') AND [t].[Note] IS NOT NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Contains_with_local_nullable_guid_list_closure(bool async)
        {
            await base.Contains_with_local_nullable_guid_list_closure(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
WHERE [t].[Id] IN ('d2c26679-562b-44d1-ab96-23d1775e0926', '23cbcf9b-ce14-45cf-aafa-2c2667ebfdd3', 'ab1b82d7-88db-42bd-a132-7eef9aa68af4')");
        }

        public override async Task Unnecessary_include_doesnt_get_added_complex_when_projecting_EF_Property(bool async)
        {
            await base.Unnecessary_include_doesnt_get_added_complex_when_projecting_EF_Property(async);

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
ORDER BY [g].[Rank]");
        }

        public override async Task Multiple_order_bys_are_properly_lifted_from_subquery_created_by_include(bool async)
        {
            await base.Multiple_order_bys_are_properly_lifted_from_subquery_created_by_include(async);

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[HasSoulPatch] = CAST(0 AS bit)
ORDER BY [g].[FullName]");
        }

        public override async Task Order_by_is_properly_lifted_from_subquery_with_same_order_by_in_the_outer_query(bool async)
        {
            await base.Order_by_is_properly_lifted_from_subquery_with_same_order_by_in_the_outer_query(async);

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[HasSoulPatch] = CAST(0 AS bit)
ORDER BY [g].[FullName]");
        }

        public override async Task Where_is_properly_lifted_from_subquery_created_by_include(bool async)
        {
            await base.Where_is_properly_lifted_from_subquery_created_by_include(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
WHERE ([g].[FullName] <> N'Augustus Cole') AND ([g].[HasSoulPatch] = CAST(0 AS bit))
ORDER BY [g].[FullName]");
        }

        public override async Task Subquery_is_lifted_from_main_from_clause_of_SelectMany(bool async)
        {
            await base.Subquery_is_lifted_from_main_from_clause_of_SelectMany(async);

            AssertSql(
                @"SELECT [g].[FullName] AS [Name1], [g0].[FullName] AS [Name2]
FROM [Gears] AS [g]
CROSS JOIN [Gears] AS [g0]
WHERE ([g].[HasSoulPatch] = CAST(1 AS bit)) AND ([g0].[HasSoulPatch] = CAST(0 AS bit))
ORDER BY [g].[FullName]");
        }

        public override async Task Subquery_containing_SelectMany_projecting_main_from_clause_gets_lifted(bool async)
        {
            await base.Subquery_containing_SelectMany_projecting_main_from_clause_gets_lifted(async);

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
CROSS JOIN [Tags] AS [t]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
ORDER BY [g].[FullName]");
        }

        public override async Task Subquery_containing_join_projecting_main_from_clause_gets_lifted(bool async)
        {
            await base.Subquery_containing_join_projecting_main_from_clause_gets_lifted(async);

            AssertSql(
                @"SELECT [g].[Nickname]
FROM [Gears] AS [g]
INNER JOIN [Tags] AS [t] ON [g].[Nickname] = [t].[GearNickName]
ORDER BY [g].[Nickname]");
        }

        public override async Task Subquery_containing_left_join_projecting_main_from_clause_gets_lifted(bool async)
        {
            await base.Subquery_containing_left_join_projecting_main_from_clause_gets_lifted(async);

            AssertSql(
                @"SELECT [g].[Nickname]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [t] ON [g].[Nickname] = [t].[GearNickName]
ORDER BY [g].[Nickname]");
        }

        public override async Task Subquery_containing_join_gets_lifted_clashing_names(bool async)
        {
            await base.Subquery_containing_join_gets_lifted_clashing_names(async);

            AssertSql(
                @"SELECT [g].[Nickname]
FROM [Gears] AS [g]
INNER JOIN [Tags] AS [t] ON [g].[Nickname] = [t].[GearNickName]
INNER JOIN [Tags] AS [t0] ON [g].[Nickname] = [t0].[GearNickName]
WHERE ([t].[GearNickName] <> N'Cole Train') OR [t].[GearNickName] IS NULL
ORDER BY [g].[Nickname], [t0].[Id]");
        }

        public override async Task Subquery_created_by_include_gets_lifted_nested(bool async)
        {
            await base.Subquery_created_by_include_gets_lifted_nested(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location], [c].[Nation]
FROM [Gears] AS [g]
INNER JOIN [Cities] AS [c] ON [g].[CityOfBirthName] = [c].[Name]
WHERE EXISTS (
    SELECT 1
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]) AND ([g].[HasSoulPatch] = CAST(0 AS bit))
ORDER BY [g].[Nickname]");
        }

        public override async Task Subquery_is_lifted_from_additional_from_clause(bool async)
        {
            await base.Subquery_is_lifted_from_additional_from_clause(async);

            AssertSql(
                @"SELECT [g].[FullName] AS [Name1], [t].[FullName] AS [Name2]
FROM [Gears] AS [g]
CROSS JOIN (
    SELECT [g0].[FullName], [g0].[HasSoulPatch]
    FROM [Gears] AS [g0]
) AS [t]
WHERE ([g].[HasSoulPatch] = CAST(1 AS bit)) AND ([t].[HasSoulPatch] = CAST(0 AS bit))
ORDER BY [g].[FullName]");
        }

        public override async Task Subquery_with_result_operator_is_not_lifted(bool async)
        {
            await base.Subquery_with_result_operator_is_not_lifted(async);

            AssertSql(
                @"@__p_0='2'

SELECT [t].[FullName]
FROM (
    SELECT TOP(@__p_0) [g].[FullName], [g].[Rank]
    FROM [Gears] AS [g]
    WHERE [g].[HasSoulPatch] = CAST(0 AS bit)
    ORDER BY [g].[FullName]
) AS [t]
ORDER BY [t].[Rank]");
        }

        public override async Task Skip_with_orderby_followed_by_orderBy_is_pushed_down(bool async)
        {
            await base.Skip_with_orderby_followed_by_orderBy_is_pushed_down(async);

            AssertSql(
                @"@__p_0='1'

SELECT [t].[FullName]
FROM (
    SELECT [g].[FullName], [g].[Rank]
    FROM [Gears] AS [g]
    WHERE [g].[HasSoulPatch] = CAST(0 AS bit)
    ORDER BY [g].[FullName]
    OFFSET @__p_0 ROWS
) AS [t]
ORDER BY [t].[Rank]");
        }

        public override async Task Take_without_orderby_followed_by_orderBy_is_pushed_down1(bool async)
        {
            await base.Take_without_orderby_followed_by_orderBy_is_pushed_down1(async);

            AssertSql(
                @"@__p_0='999'

SELECT [t].[FullName]
FROM (
    SELECT TOP(@__p_0) [g].[FullName], [g].[Rank]
    FROM [Gears] AS [g]
    WHERE [g].[HasSoulPatch] = CAST(0 AS bit)
) AS [t]
ORDER BY [t].[Rank]");
        }

        public override async Task Take_without_orderby_followed_by_orderBy_is_pushed_down2(bool async)
        {
            await base.Take_without_orderby_followed_by_orderBy_is_pushed_down2(async);

            AssertSql(
                @"@__p_0='999'

SELECT [t].[FullName]
FROM (
    SELECT TOP(@__p_0) [g].[FullName], [g].[Rank]
    FROM [Gears] AS [g]
    WHERE [g].[HasSoulPatch] = CAST(0 AS bit)
) AS [t]
ORDER BY [t].[Rank]");
        }

        public override async Task Take_without_orderby_followed_by_orderBy_is_pushed_down3(bool async)
        {
            await base.Take_without_orderby_followed_by_orderBy_is_pushed_down3(async);

            AssertSql(
                @"@__p_0='999'

SELECT [t].[FullName]
FROM (
    SELECT TOP(@__p_0) [g].[FullName], [g].[Rank]
    FROM [Gears] AS [g]
    WHERE [g].[HasSoulPatch] = CAST(0 AS bit)
) AS [t]
ORDER BY [t].[FullName], [t].[Rank]");
        }

        public override async Task Select_length_of_string_property(bool async)
        {
            await base.Select_length_of_string_property(async);

            AssertSql(
                @"SELECT [w].[Name], CAST(LEN([w].[Name]) AS int) AS [Length]
FROM [Weapons] AS [w]");
        }

        public override async Task Client_method_on_collection_navigation_in_outer_join_key(bool async)
        {
            await base.Client_method_on_collection_navigation_in_outer_join_key(async);

            AssertSql(
                @"SELECT [g].[FullName], [g].[Nickname]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')",
                //
                @"@_outer_FullName1='Damon Baird' (Size = 450)

SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapons] AS [w0]
WHERE @_outer_FullName1 = [w0].[OwnerFullName]",
                //
                @"@_outer_FullName1='Augustus Cole' (Size = 450)

SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapons] AS [w0]
WHERE @_outer_FullName1 = [w0].[OwnerFullName]",
                //
                @"@_outer_FullName1='Dominic Santiago' (Size = 450)

SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapons] AS [w0]
WHERE @_outer_FullName1 = [w0].[OwnerFullName]",
                //
                @"@_outer_FullName1='Marcus Fenix' (Size = 450)

SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapons] AS [w0]
WHERE @_outer_FullName1 = [w0].[OwnerFullName]",
                //
                @"@_outer_FullName1='Garron Paduk' (Size = 450)

SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapons] AS [w0]
WHERE @_outer_FullName1 = [w0].[OwnerFullName]",
                //
                @"SELECT [o].[FullName], [o].[Nickname] AS [o]
FROM [Gears] AS [o]
WHERE ([o].[Discriminator] = N'Officer') AND ([o].[HasSoulPatch] = 1)",
                //
                @"@_outer_FullName='Damon Baird' (Size = 450)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE @_outer_FullName = [w].[OwnerFullName]",
                //
                @"@_outer_FullName='Marcus Fenix' (Size = 450)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE @_outer_FullName = [w].[OwnerFullName]");
        }

        public override async Task Member_access_on_derived_entity_using_cast(bool async)
        {
            await base.Member_access_on_derived_entity_using_cast(async);

            AssertSql(
                @"SELECT [f].[Name], [f].[Eradicated]
FROM [Factions] AS [f]
ORDER BY [f].[Name]");
        }

        public override async Task Member_access_on_derived_materialized_entity_using_cast(bool async)
        {
            await base.Member_access_on_derived_materialized_entity_using_cast(async);

            AssertSql(
                @"SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated]
FROM [Factions] AS [f]
ORDER BY [f].[Name]");
        }

        public override async Task Member_access_on_derived_entity_using_cast_and_let(bool async)
        {
            await base.Member_access_on_derived_entity_using_cast_and_let(async);

            AssertSql(
                @"SELECT [f].[Name], [f].[Eradicated]
FROM [Factions] AS [f]
ORDER BY [f].[Name]");
        }

        public override async Task Property_access_on_derived_entity_using_cast(bool async)
        {
            await base.Property_access_on_derived_entity_using_cast(async);

            AssertSql(
                @"SELECT [f].[Name], [f].[Eradicated]
FROM [Factions] AS [f]
ORDER BY [f].[Name]");
        }

        public override async Task Navigation_access_on_derived_entity_using_cast(bool async)
        {
            await base.Navigation_access_on_derived_entity_using_cast(async);

            AssertSql(
                @"SELECT [f].[Name], [t].[ThreatLevel] AS [Threat]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [l].[Name], [l].[ThreatLevel]
    FROM [LocustLeaders] AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
ORDER BY [f].[Name]");
        }

        public override async Task Navigation_access_on_derived_materialized_entity_using_cast(bool async)
        {
            await base.Navigation_access_on_derived_materialized_entity_using_cast(async);

            AssertSql(
                @"SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated], [t].[ThreatLevel] AS [Threat]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [l].[Name], [l].[ThreatLevel]
    FROM [LocustLeaders] AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
ORDER BY [f].[Name]");
        }

        public override async Task Navigation_access_via_EFProperty_on_derived_entity_using_cast(bool async)
        {
            await base.Navigation_access_via_EFProperty_on_derived_entity_using_cast(async);

            AssertSql(
                @"SELECT [f].[Name], [t].[ThreatLevel] AS [Threat]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [l].[Name], [l].[ThreatLevel]
    FROM [LocustLeaders] AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
ORDER BY [f].[Name]");
        }

        public override async Task Navigation_access_fk_on_derived_entity_using_cast(bool async)
        {
            await base.Navigation_access_fk_on_derived_entity_using_cast(async);

            AssertSql(
                @"SELECT [f].[Name], [t].[Name] AS [CommanderName]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [l].[Name]
    FROM [LocustLeaders] AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
ORDER BY [f].[Name]");
        }

        public override async Task Collection_navigation_access_on_derived_entity_using_cast(bool async)
        {
            await base.Collection_navigation_access_on_derived_entity_using_cast(async);

            AssertSql(
                @"SELECT [f].[Name], (
    SELECT COUNT(*)
    FROM [LocustLeaders] AS [l]
    WHERE [f].[Id] = [l].[LocustHordeId]) AS [LeadersCount]
FROM [Factions] AS [f]
ORDER BY [f].[Name]");
        }

        public override async Task Collection_navigation_access_on_derived_entity_using_cast_in_SelectMany(bool async)
        {
            await base.Collection_navigation_access_on_derived_entity_using_cast_in_SelectMany(async);

            AssertSql(
                @"SELECT [f].[Name], [l].[Name] AS [LeaderName]
FROM [Factions] AS [f]
INNER JOIN [LocustLeaders] AS [l] ON [f].[Id] = [l].[LocustHordeId]
ORDER BY [l].[Name]");
        }

        public override async Task Include_on_derived_entity_using_OfType(bool async)
        {
            await base.Include_on_derived_entity_using_OfType(async);

            AssertSql(
                @"SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated], [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [l0].[Name], [l0].[Discriminator], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
    FROM [LocustLeaders] AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
LEFT JOIN [LocustLeaders] AS [l0] ON [f].[Id] = [l0].[LocustHordeId]
ORDER BY [f].[Name], [f].[Id], [t].[Name], [l0].[Name]");
        }

        public override async Task Distinct_on_subquery_doesnt_get_lifted(bool async)
        {
            await base.Distinct_on_subquery_doesnt_get_lifted(async);

            AssertSql(
                @"SELECT [t].[HasSoulPatch]
FROM (
    SELECT DISTINCT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
    FROM [Gears] AS [g]
) AS [t]");
        }

        public override async Task Cast_result_operator_on_subquery_is_properly_lifted_to_a_convert(bool async)
        {
            await base.Cast_result_operator_on_subquery_is_properly_lifted_to_a_convert(async);

            AssertSql(
                @"SELECT [f].[Eradicated]
FROM [Factions] AS [f]");
        }

        public override async Task Comparing_two_collection_navigations_composite_key(bool async)
        {
            await base.Comparing_two_collection_navigations_composite_key(async);

            AssertSql(
                @"SELECT [g].[Nickname] AS [Nickname1], [g0].[Nickname] AS [Nickname2]
FROM [Gears] AS [g]
CROSS JOIN [Gears] AS [g0]
WHERE ([g].[Nickname] = [g0].[Nickname]) AND ([g].[SquadId] = [g0].[SquadId])
ORDER BY [g].[Nickname]");
        }

        public override async Task Comparing_two_collection_navigations_inheritance(bool async)
        {
            await base.Comparing_two_collection_navigations_inheritance(async);

            AssertSql(
                @"SELECT [f].[Name], [t].[Nickname]
FROM [Factions] AS [f]
CROSS JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[HasSoulPatch]
    FROM [Gears] AS [g]
    WHERE [g].[Discriminator] = N'Officer'
) AS [t]
LEFT JOIN (
    SELECT [l].[Name], [l].[DefeatedByNickname], [l].[DefeatedBySquadId]
    FROM [LocustLeaders] AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t0] ON [f].[CommanderName] = [t0].[Name]
LEFT JOIN [Gears] AS [g0] ON ([t0].[DefeatedByNickname] = [g0].[Nickname]) AND ([t0].[DefeatedBySquadId] = [g0].[SquadId])
WHERE ([t].[HasSoulPatch] = CAST(1 AS bit)) AND (([g0].[Nickname] = [t].[Nickname]) AND ([g0].[SquadId] = [t].[SquadId]))");
        }

        public override async Task Comparing_entities_using_Equals_inheritance(bool async)
        {
            await base.Comparing_entities_using_Equals_inheritance(async);

            AssertSql(
                @"SELECT [g].[Nickname] AS [Nickname1], [t].[Nickname] AS [Nickname2]
FROM [Gears] AS [g]
CROSS JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] = N'Officer'
) AS [t]
WHERE ([g].[Nickname] = [t].[Nickname]) AND ([g].[SquadId] = [t].[SquadId])
ORDER BY [g].[Nickname], [t].[Nickname]");
        }

        public override async Task Contains_on_nullable_array_produces_correct_sql(bool async)
        {
            await base.Contains_on_nullable_array_produces_correct_sql(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Cities] AS [c] ON [g].[AssignedCityName] = [c].[Name]
WHERE ([g].[SquadId] < 2) AND (([c].[Name] = N'Ephyra') OR [c].[Name] IS NULL)");
        }

        public override async Task Optional_navigation_with_collection_composite_key(bool async)
        {
            await base.Optional_navigation_with_collection_composite_key(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE ([g].[Discriminator] = N'Officer') AND ((
    SELECT COUNT(*)
    FROM [Gears] AS [g0]
    WHERE (([g].[Nickname] IS NOT NULL AND [g].[SquadId] IS NOT NULL) AND (([g].[Nickname] = [g0].[LeaderNickname]) AND ([g].[SquadId] = [g0].[LeaderSquadId]))) AND ([g0].[Nickname] = N'Dom')) > 0)");
        }

        public override async Task Select_null_conditional_with_inheritance(bool async)
        {
            await base.Select_null_conditional_with_inheritance(async);

            AssertSql(
                @"SELECT CASE
    WHEN [f].[CommanderName] IS NOT NULL THEN [f].[CommanderName]
    ELSE NULL
END
FROM [Factions] AS [f]");
        }

        public override async Task Select_null_conditional_with_inheritance_negative(bool async)
        {
            await base.Select_null_conditional_with_inheritance_negative(async);

            AssertSql(
                @"SELECT CASE
    WHEN [f].[CommanderName] IS NOT NULL THEN [f].[Eradicated]
    ELSE NULL
END
FROM [Factions] AS [f]");
        }

        public override async Task Project_collection_navigation_with_inheritance1(bool async)
        {
            await base.Project_collection_navigation_with_inheritance1(async);

            AssertSql(
                @"SELECT [f].[Id], [t].[Name], [f0].[Id], [l0].[Name], [l0].[Discriminator], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [l].[Name]
    FROM [LocustLeaders] AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
LEFT JOIN [Factions] AS [f0] ON [t].[Name] = [f0].[CommanderName]
LEFT JOIN [LocustLeaders] AS [l0] ON [f0].[Id] = [l0].[LocustHordeId]
ORDER BY [f].[Id], [t].[Name], [f0].[Id], [l0].[Name]");
        }

        public override async Task Project_collection_navigation_with_inheritance2(bool async)
        {
            await base.Project_collection_navigation_with_inheritance2(async);

            AssertSql(
                @"SELECT [f].[Id], [t].[Name], [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [l].[Name], [l].[DefeatedByNickname], [l].[DefeatedBySquadId]
    FROM [LocustLeaders] AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
LEFT JOIN [Gears] AS [g] ON ([t].[DefeatedByNickname] = [g].[Nickname]) AND ([t].[DefeatedBySquadId] = [g].[SquadId])
LEFT JOIN [Gears] AS [g0] ON (([g].[Nickname] = [g0].[LeaderNickname]) OR ([g].[Nickname] IS NULL AND [g0].[LeaderNickname] IS NULL)) AND ([g].[SquadId] = [g0].[LeaderSquadId])
ORDER BY [f].[Id], [t].[Name], [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId]");
        }

        public override async Task Project_collection_navigation_with_inheritance3(bool async)
        {
            await base.Project_collection_navigation_with_inheritance3(async);

            AssertSql(
                @"SELECT [f].[Id], [t].[Name], [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [l].[Name], [l].[DefeatedByNickname], [l].[DefeatedBySquadId]
    FROM [LocustLeaders] AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
LEFT JOIN [Gears] AS [g] ON ([t].[DefeatedByNickname] = [g].[Nickname]) AND ([t].[DefeatedBySquadId] = [g].[SquadId])
LEFT JOIN [Gears] AS [g0] ON (([g].[Nickname] = [g0].[LeaderNickname]) OR ([g].[Nickname] IS NULL AND [g0].[LeaderNickname] IS NULL)) AND ([g].[SquadId] = [g0].[LeaderSquadId])
ORDER BY [f].[Id], [t].[Name], [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId]");
        }

        public override async Task Include_reference_on_derived_type_using_string(bool async)
        {
            await base.Include_reference_on_derived_type_using_string(async);

            AssertSql(
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [LocustLeaders] AS [l]
LEFT JOIN [Gears] AS [g] ON ([l].[DefeatedByNickname] = [g].[Nickname]) AND ([l].[DefeatedBySquadId] = [g].[SquadId])");
        }

        public override async Task Include_reference_on_derived_type_using_string_nested1(bool async)
        {
            await base.Include_reference_on_derived_type_using_string_nested1(async);

            AssertSql(
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [LocustLeaders] AS [l]
LEFT JOIN [Gears] AS [g] ON ([l].[DefeatedByNickname] = [g].[Nickname]) AND ([l].[DefeatedBySquadId] = [g].[SquadId])
LEFT JOIN [Squads] AS [s] ON [g].[SquadId] = [s].[Id]");
        }

        public override async Task Include_reference_on_derived_type_using_string_nested2(bool async)
        {
            await base.Include_reference_on_derived_type_using_string_nested2(async);

            AssertSql(
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Name], [t].[Location], [t].[Nation]
FROM [LocustLeaders] AS [l]
LEFT JOIN [Gears] AS [g] ON ([l].[DefeatedByNickname] = [g].[Nickname]) AND ([l].[DefeatedBySquadId] = [g].[SquadId])
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], [c].[Name], [c].[Location], [c].[Nation]
    FROM [Gears] AS [g0]
    INNER JOIN [Cities] AS [c] ON [g0].[CityOfBirthName] = [c].[Name]
) AS [t] ON (([g].[Nickname] = [t].[LeaderNickname]) OR ([g].[Nickname] IS NULL AND [t].[LeaderNickname] IS NULL)) AND ([g].[SquadId] = [t].[LeaderSquadId])
ORDER BY [l].[Name], [g].[Nickname], [g].[SquadId], [t].[Nickname], [t].[SquadId], [t].[Name]");
        }

        public override async Task Include_reference_on_derived_type_using_lambda(bool async)
        {
            await base.Include_reference_on_derived_type_using_lambda(async);

            AssertSql(
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [LocustLeaders] AS [l]
LEFT JOIN [Gears] AS [g] ON ([l].[DefeatedByNickname] = [g].[Nickname]) AND ([l].[DefeatedBySquadId] = [g].[SquadId])");
        }

        public override async Task Include_reference_on_derived_type_using_lambda_with_soft_cast(bool async)
        {
            await base.Include_reference_on_derived_type_using_lambda_with_soft_cast(async);

            AssertSql(
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [LocustLeaders] AS [l]
LEFT JOIN [Gears] AS [g] ON ([l].[DefeatedByNickname] = [g].[Nickname]) AND ([l].[DefeatedBySquadId] = [g].[SquadId])");
        }

        public override async Task Include_reference_on_derived_type_using_lambda_with_tracking(bool async)
        {
            await base.Include_reference_on_derived_type_using_lambda_with_tracking(async);

            AssertSql(
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [LocustLeaders] AS [l]
LEFT JOIN [Gears] AS [g] ON ([l].[DefeatedByNickname] = [g].[Nickname]) AND ([l].[DefeatedBySquadId] = [g].[SquadId])");
        }

        public override async Task Include_collection_on_derived_type_using_string(bool async)
        {
            await base.Include_collection_on_derived_type_using_string(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Gears] AS [g0] ON ([g].[Nickname] = [g0].[LeaderNickname]) AND ([g].[SquadId] = [g0].[LeaderSquadId])
ORDER BY [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId]");
        }

        public override async Task Include_collection_on_derived_type_using_lambda(bool async)
        {
            await base.Include_collection_on_derived_type_using_lambda(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Gears] AS [g0] ON ([g].[Nickname] = [g0].[LeaderNickname]) AND ([g].[SquadId] = [g0].[LeaderSquadId])
ORDER BY [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId]");
        }

        public override async Task Include_collection_on_derived_type_using_lambda_with_soft_cast(bool async)
        {
            await base.Include_collection_on_derived_type_using_lambda_with_soft_cast(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Gears] AS [g0] ON ([g].[Nickname] = [g0].[LeaderNickname]) AND ([g].[SquadId] = [g0].[LeaderSquadId])
ORDER BY [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId]");
        }

        public override async Task Include_base_navigation_on_derived_entity(bool async)
        {
            await base.Include_base_navigation_on_derived_entity(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
LEFT JOIN [Weapons] AS [w] ON [g].[FullName] = [w].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Id], [w].[Id]");
        }

        public override async Task ThenInclude_collection_on_derived_after_base_reference(bool async)
        {
            await base.ThenInclude_collection_on_derived_after_base_reference(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
LEFT JOIN [Weapons] AS [w] ON [g].[FullName] = [w].[OwnerFullName]
ORDER BY [t].[Id], [g].[Nickname], [g].[SquadId], [w].[Id]");
        }

        public override async Task ThenInclude_collection_on_derived_after_derived_reference(bool async)
        {
            await base.ThenInclude_collection_on_derived_after_derived_reference(async);

            AssertSql(
                @"SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated], [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
    FROM [LocustLeaders] AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
LEFT JOIN [Gears] AS [g] ON ([t].[DefeatedByNickname] = [g].[Nickname]) AND ([t].[DefeatedBySquadId] = [g].[SquadId])
LEFT JOIN [Gears] AS [g0] ON (([g].[Nickname] = [g0].[LeaderNickname]) OR ([g].[Nickname] IS NULL AND [g0].[LeaderNickname] IS NULL)) AND ([g].[SquadId] = [g0].[LeaderSquadId])
ORDER BY [f].[Id], [t].[Name], [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId]");
        }

        public override async Task ThenInclude_collection_on_derived_after_derived_collection(bool async)
        {
            await base.ThenInclude_collection_on_derived_after_derived_collection(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Nickname0], [t].[SquadId0], [t].[AssignedCityName0], [t].[CityOfBirthName0], [t].[Discriminator0], [t].[FullName0], [t].[HasSoulPatch0], [t].[LeaderNickname0], [t].[LeaderSquadId0], [t].[Rank0]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], [g1].[Nickname] AS [Nickname0], [g1].[SquadId] AS [SquadId0], [g1].[AssignedCityName] AS [AssignedCityName0], [g1].[CityOfBirthName] AS [CityOfBirthName0], [g1].[Discriminator] AS [Discriminator0], [g1].[FullName] AS [FullName0], [g1].[HasSoulPatch] AS [HasSoulPatch0], [g1].[LeaderNickname] AS [LeaderNickname0], [g1].[LeaderSquadId] AS [LeaderSquadId0], [g1].[Rank] AS [Rank0]
    FROM [Gears] AS [g0]
    LEFT JOIN [Gears] AS [g1] ON ([g0].[Nickname] = [g1].[LeaderNickname]) AND ([g0].[SquadId] = [g1].[LeaderSquadId])
) AS [t] ON ([g].[Nickname] = [t].[LeaderNickname]) AND ([g].[SquadId] = [t].[LeaderSquadId])
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Nickname], [t].[SquadId], [t].[Nickname0], [t].[SquadId0]");
        }

        public override async Task ThenInclude_reference_on_derived_after_derived_collection(bool async)
        {
            await base.ThenInclude_reference_on_derived_after_derived_collection(async);

            AssertSql(
                @"SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated], [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator0], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator] AS [Discriminator0], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
    FROM [LocustLeaders] AS [l]
    LEFT JOIN [Gears] AS [g] ON ([l].[DefeatedByNickname] = [g].[Nickname]) AND ([l].[DefeatedBySquadId] = [g].[SquadId])
) AS [t] ON [f].[Id] = [t].[LocustHordeId]
ORDER BY [f].[Id], [t].[Name], [t].[Nickname], [t].[SquadId]");
        }

        public override async Task Multiple_derived_included_on_one_method(bool async)
        {
            await base.Multiple_derived_included_on_one_method(async);

            AssertSql(
                @"SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated], [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
    FROM [LocustLeaders] AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
LEFT JOIN [Gears] AS [g] ON ([t].[DefeatedByNickname] = [g].[Nickname]) AND ([t].[DefeatedBySquadId] = [g].[SquadId])
LEFT JOIN [Gears] AS [g0] ON (([g].[Nickname] = [g0].[LeaderNickname]) OR ([g].[Nickname] IS NULL AND [g0].[LeaderNickname] IS NULL)) AND ([g].[SquadId] = [g0].[LeaderSquadId])
ORDER BY [f].[Id], [t].[Name], [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId]");
        }

        public override async Task Include_on_derived_multi_level(bool async)
        {
            await base.Include_on_derived_multi_level(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Id], [t].[Banner], [t].[Banner5], [t].[InternalNumber], [t].[Name], [t].[SquadId0], [t].[MissionId]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s0].[SquadId] AS [SquadId0], [s0].[MissionId]
    FROM [Gears] AS [g0]
    INNER JOIN [Squads] AS [s] ON [g0].[SquadId] = [s].[Id]
    LEFT JOIN [SquadMissions] AS [s0] ON [s].[Id] = [s0].[SquadId]
) AS [t] ON ([g].[Nickname] = [t].[LeaderNickname]) AND ([g].[SquadId] = [t].[LeaderSquadId])
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Nickname], [t].[SquadId], [t].[Id], [t].[SquadId0], [t].[MissionId]");
        }

        public override async Task Projecting_nullable_bool_in_conditional_works(bool async)
        {
            await base.Projecting_nullable_bool_in_conditional_works(async);

            AssertSql(
                @"SELECT CASE
    WHEN [g].[Nickname] IS NOT NULL AND [g].[SquadId] IS NOT NULL THEN [g].[HasSoulPatch]
    ELSE CAST(0 AS bit)
END AS [Prop]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])");
        }

        public override async Task Enum_ToString_is_client_eval(bool async)
        {
            await base.Enum_ToString_is_client_eval(async);

            AssertSql(
                @"SELECT [g].[Rank]
FROM [Gears] AS [g]
ORDER BY [g].[SquadId], [g].[Nickname]");
        }

        public override async Task ToString_boolean_property_non_nullable(bool async)
        {
            await base.ToString_boolean_property_non_nullable(async);

            AssertSql(
                @"SELECT CASE
    WHEN [w].[IsAutomatic] = CAST(0 AS bit) THEN N'False'
    ELSE N'True'
END
FROM [Weapons] AS [w]");
        }

        public override async Task ToString_boolean_property_nullable(bool async)
        {
            await base.ToString_boolean_property_nullable(async);

            AssertSql(
                @"SELECT CASE
    WHEN [f].[Eradicated] = CAST(0 AS bit) THEN N'False'
    WHEN [f].[Eradicated] = CAST(1 AS bit) THEN N'True'
    ELSE NULL
END
FROM [Factions] AS [f]");
        }

        public override async Task Correlated_collections_naked_navigation_with_ToList(bool async)
        {
            await base.Correlated_collections_naked_navigation_with_ToList(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Gears] AS [g]
LEFT JOIN [Weapons] AS [w] ON [g].[FullName] = [w].[OwnerFullName]
WHERE [g].[Nickname] <> N'Marcus'
ORDER BY [g].[Nickname], [g].[SquadId], [w].[Id]");
        }

        public override async Task Correlated_collections_naked_navigation_with_ToList_followed_by_projecting_count(bool async)
        {
            await base.Correlated_collections_naked_navigation_with_ToList_followed_by_projecting_count(async);

            AssertSql(
                @"SELECT (
    SELECT COUNT(*)
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName])
FROM [Gears] AS [g]
WHERE [g].[Nickname] <> N'Marcus'
ORDER BY [g].[Nickname]");
        }

        public override async Task Correlated_collections_naked_navigation_with_ToArray(bool async)
        {
            await base.Correlated_collections_naked_navigation_with_ToArray(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Gears] AS [g]
LEFT JOIN [Weapons] AS [w] ON [g].[FullName] = [w].[OwnerFullName]
WHERE [g].[Nickname] <> N'Marcus'
ORDER BY [g].[Nickname], [g].[SquadId], [w].[Id]");
        }

        public override async Task Correlated_collections_basic_projection(bool async)
        {
            await base.Correlated_collections_basic_projection(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[SynergyWithId]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
    FROM [Weapons] AS [w]
    WHERE ([w].[IsAutomatic] = CAST(1 AS bit)) OR (([w].[Name] <> N'foo') OR [w].[Name] IS NULL)
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
WHERE [g].[Nickname] <> N'Marcus'
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Id]");
        }

        public override async Task Correlated_collections_basic_projection_explicit_to_list(bool async)
        {
            await base.Correlated_collections_basic_projection_explicit_to_list(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[SynergyWithId]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
    FROM [Weapons] AS [w]
    WHERE ([w].[IsAutomatic] = CAST(1 AS bit)) OR (([w].[Name] <> N'foo') OR [w].[Name] IS NULL)
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
WHERE [g].[Nickname] <> N'Marcus'
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Id]");
        }

        public override async Task Correlated_collections_basic_projection_explicit_to_array(bool async)
        {
            await base.Correlated_collections_basic_projection_explicit_to_array(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[SynergyWithId]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
    FROM [Weapons] AS [w]
    WHERE ([w].[IsAutomatic] = CAST(1 AS bit)) OR (([w].[Name] <> N'foo') OR [w].[Name] IS NULL)
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
WHERE [g].[Nickname] <> N'Marcus'
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Id]");
        }

        public override async Task Correlated_collections_basic_projection_ordered(bool async)
        {
            await base.Correlated_collections_basic_projection_ordered(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[SynergyWithId]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
    FROM [Weapons] AS [w]
    WHERE ([w].[IsAutomatic] = CAST(1 AS bit)) OR (([w].[Name] <> N'foo') OR [w].[Name] IS NULL)
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
WHERE [g].[Nickname] <> N'Marcus'
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Name] DESC, [t].[Id]");
        }

        public override async Task Correlated_collections_basic_projection_composite_key(bool async)
        {
            await base.Correlated_collections_basic_projection_composite_key(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [t].[Nickname], [t].[FullName], [t].[SquadId]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[FullName], [g0].[SquadId], [g0].[LeaderNickname], [g0].[LeaderSquadId]
    FROM [Gears] AS [g0]
    WHERE [g0].[HasSoulPatch] = CAST(0 AS bit)
) AS [t] ON ([g].[Nickname] = [t].[LeaderNickname]) AND ([g].[SquadId] = [t].[LeaderSquadId])
WHERE ([g].[Discriminator] = N'Officer') AND ([g].[Nickname] <> N'Foo')
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Nickname], [t].[SquadId]");
        }

        public override async Task Correlated_collections_basic_projecting_single_property(bool async)
        {
            await base.Correlated_collections_basic_projecting_single_property(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [t].[Name], [t].[Id]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [w].[Name], [w].[Id], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
    WHERE ([w].[IsAutomatic] = CAST(1 AS bit)) OR (([w].[Name] <> N'foo') OR [w].[Name] IS NULL)
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
WHERE [g].[Nickname] <> N'Marcus'
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Id]");
        }

        public override async Task Correlated_collections_basic_projecting_constant(bool async)
        {
            await base.Correlated_collections_basic_projecting_constant(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [t].[c], [t].[Id]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT N'BFG' AS [c], [w].[Id], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
    WHERE ([w].[IsAutomatic] = CAST(1 AS bit)) OR (([w].[Name] <> N'foo') OR [w].[Name] IS NULL)
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
WHERE [g].[Nickname] <> N'Marcus'
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Id]");
        }

        public override async Task Correlated_collections_basic_projecting_constant_bool(bool async)
        {
            await base.Correlated_collections_basic_projecting_constant_bool(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [t].[c], [t].[Id]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT CAST(1 AS bit) AS [c], [w].[Id], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
    WHERE ([w].[IsAutomatic] = CAST(1 AS bit)) OR (([w].[Name] <> N'foo') OR [w].[Name] IS NULL)
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
WHERE [g].[Nickname] <> N'Marcus'
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Id]");
        }

        public override async Task Correlated_collections_projection_of_collection_thru_navigation(bool async)
        {
            await base.Correlated_collections_projection_of_collection_thru_navigation(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [s].[Id], [t].[SquadId], [t].[MissionId]
FROM [Gears] AS [g]
INNER JOIN [Squads] AS [s] ON [g].[SquadId] = [s].[Id]
LEFT JOIN (
    SELECT [s0].[SquadId], [s0].[MissionId]
    FROM [SquadMissions] AS [s0]
    WHERE [s0].[MissionId] <> 17
) AS [t] ON [s].[Id] = [t].[SquadId]
WHERE [g].[Nickname] <> N'Marcus'
ORDER BY [g].[FullName], [g].[Nickname], [g].[SquadId], [s].[Id], [t].[SquadId], [t].[MissionId]");
        }

        public override async Task Correlated_collections_project_anonymous_collection_result(bool async)
        {
            await base.Correlated_collections_project_anonymous_collection_result(async);

            AssertSql(
                @"SELECT [s].[Name], [s].[Id], [g].[FullName], [g].[Rank], [g].[Nickname], [g].[SquadId]
FROM [Squads] AS [s]
LEFT JOIN [Gears] AS [g] ON [s].[Id] = [g].[SquadId]
WHERE [s].[Id] < 20
ORDER BY [s].[Id], [g].[Nickname], [g].[SquadId]");
        }

        public override async Task Correlated_collections_nested(bool async)
        {
            await base.Correlated_collections_nested(async);

            AssertSql(
                @"SELECT [s].[Id], [t0].[SquadId], [t0].[MissionId], [t0].[Id], [t0].[SquadId0], [t0].[MissionId0]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [s0].[SquadId], [s0].[MissionId], [m].[Id], [t].[SquadId] AS [SquadId0], [t].[MissionId] AS [MissionId0]
    FROM [SquadMissions] AS [s0]
    INNER JOIN [Missions] AS [m] ON [s0].[MissionId] = [m].[Id]
    LEFT JOIN (
        SELECT [s1].[SquadId], [s1].[MissionId]
        FROM [SquadMissions] AS [s1]
        WHERE [s1].[SquadId] < 7
    ) AS [t] ON [m].[Id] = [t].[MissionId]
    WHERE [s0].[MissionId] < 42
) AS [t0] ON [s].[Id] = [t0].[SquadId]
ORDER BY [s].[Id], [t0].[SquadId], [t0].[MissionId], [t0].[Id], [t0].[SquadId0], [t0].[MissionId0]");
        }

        public override async Task Correlated_collections_nested_mixed_streaming_with_buffer1(bool async)
        {
            await base.Correlated_collections_nested_mixed_streaming_with_buffer1(async);

            AssertSql(
                @"SELECT [s].[Id], [t0].[SquadId], [t0].[MissionId], [t0].[Id], [t0].[SquadId0], [t0].[MissionId0]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [s0].[SquadId], [s0].[MissionId], [m].[Id], [t].[SquadId] AS [SquadId0], [t].[MissionId] AS [MissionId0]
    FROM [SquadMissions] AS [s0]
    INNER JOIN [Missions] AS [m] ON [s0].[MissionId] = [m].[Id]
    LEFT JOIN (
        SELECT [s1].[SquadId], [s1].[MissionId]
        FROM [SquadMissions] AS [s1]
        WHERE [s1].[SquadId] < 2
    ) AS [t] ON [m].[Id] = [t].[MissionId]
    WHERE [s0].[MissionId] < 3
) AS [t0] ON [s].[Id] = [t0].[SquadId]
ORDER BY [s].[Id], [t0].[SquadId], [t0].[MissionId], [t0].[Id], [t0].[SquadId0], [t0].[MissionId0]");
        }

        public override async Task Correlated_collections_nested_mixed_streaming_with_buffer2(bool async)
        {
            await base.Correlated_collections_nested_mixed_streaming_with_buffer2(async);

            AssertSql(
                @"SELECT [s].[Id], [t0].[SquadId], [t0].[MissionId], [t0].[Id], [t0].[SquadId0], [t0].[MissionId0]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [s0].[SquadId], [s0].[MissionId], [m].[Id], [t].[SquadId] AS [SquadId0], [t].[MissionId] AS [MissionId0]
    FROM [SquadMissions] AS [s0]
    INNER JOIN [Missions] AS [m] ON [s0].[MissionId] = [m].[Id]
    LEFT JOIN (
        SELECT [s1].[SquadId], [s1].[MissionId]
        FROM [SquadMissions] AS [s1]
        WHERE [s1].[SquadId] < 7
    ) AS [t] ON [m].[Id] = [t].[MissionId]
    WHERE [s0].[MissionId] < 42
) AS [t0] ON [s].[Id] = [t0].[SquadId]
ORDER BY [s].[Id], [t0].[SquadId], [t0].[MissionId], [t0].[Id], [t0].[SquadId0], [t0].[MissionId0]");
        }

        public override async Task Correlated_collections_nested_with_custom_ordering(bool async)
        {
            await base.Correlated_collections_nested_with_custom_ordering(async);

            AssertSql(
                @"SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t0].[FullName], [t0].[Nickname], [t0].[SquadId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[SynergyWithId]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [g0].[FullName], [g0].[Nickname], [g0].[SquadId], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[SynergyWithId], [g0].[Rank], [g0].[LeaderNickname], [g0].[LeaderSquadId]
    FROM [Gears] AS [g0]
    LEFT JOIN (
        SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE ([w].[Name] <> N'Bar') OR [w].[Name] IS NULL
    ) AS [t] ON [g0].[FullName] = [t].[OwnerFullName]
    WHERE [g0].[FullName] <> N'Foo'
) AS [t0] ON ([g].[Nickname] = [t0].[LeaderNickname]) AND ([g].[SquadId] = [t0].[LeaderSquadId])
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[HasSoulPatch] DESC, [g].[Nickname], [g].[SquadId], [t0].[Rank], [t0].[Nickname], [t0].[SquadId], [t0].[IsAutomatic], [t0].[Id]");
        }

        public override async Task Correlated_collections_same_collection_projected_multiple_times(bool async)
        {
            await base.Correlated_collections_same_collection_projected_multiple_times(async);

            AssertSql(
                @"SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[SynergyWithId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[SynergyWithId]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
    FROM [Weapons] AS [w]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit)
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
LEFT JOIN (
    SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
    FROM [Weapons] AS [w0]
    WHERE [w0].[IsAutomatic] = CAST(1 AS bit)
) AS [t0] ON [g].[FullName] = [t0].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Id], [t0].[Id]");
        }

        public override async Task Correlated_collections_similar_collection_projected_multiple_times(bool async)
        {
            await base.Correlated_collections_similar_collection_projected_multiple_times(async);

            AssertSql(
                @"SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[SynergyWithId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[SynergyWithId]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
    FROM [Weapons] AS [w]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit)
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
LEFT JOIN (
    SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
    FROM [Weapons] AS [w0]
    WHERE [w0].[IsAutomatic] = CAST(0 AS bit)
) AS [t0] ON [g].[FullName] = [t0].[OwnerFullName]
ORDER BY [g].[Rank], [g].[Nickname], [g].[SquadId], [t].[OwnerFullName], [t].[Id], [t0].[IsAutomatic], [t0].[Id]");
        }

        public override async Task Correlated_collections_different_collections_projected(bool async)
        {
            await base.Correlated_collections_different_collections_projected(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [t].[Name], [t].[IsAutomatic], [t].[Id], [t0].[Nickname], [t0].[Rank], [t0].[SquadId]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [w].[Name], [w].[IsAutomatic], [w].[Id], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit)
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[Rank], [g0].[SquadId], [g0].[FullName], [g0].[LeaderNickname], [g0].[LeaderSquadId]
    FROM [Gears] AS [g0]
) AS [t0] ON ([g].[Nickname] = [t0].[LeaderNickname]) AND ([g].[SquadId] = [t0].[LeaderSquadId])
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[FullName], [g].[Nickname], [g].[SquadId], [t].[Id], [t0].[FullName], [t0].[Nickname], [t0].[SquadId]");
        }

        public override async Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys(bool async)
        {
            await base.Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys(async);

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
WHERE ([g].[Discriminator] = N'Officer') AND EXISTS (
    SELECT 1
    FROM [Gears] AS [g0]
    WHERE ([g].[Nickname] = [g0].[LeaderNickname]) AND ([g].[SquadId] = [g0].[LeaderSquadId]))
ORDER BY [g].[HasSoulPatch] DESC, [t].[Note]");
        }

        public override async Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery(bool async)
        {
            await base.Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery(async);

            AssertSql(
                @"SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t].[Id], [g1].[Nickname], [g1].[SquadId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[SynergyWithId], [t0].[Nickname], [t0].[SquadId]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
LEFT JOIN [Gears] AS [g1] ON ([t].[GearNickName] = [g1].[Nickname]) AND ([t].[GearSquadId] = [g1].[SquadId])
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [g2].[Nickname], [g2].[SquadId]
    FROM [Weapons] AS [w]
    LEFT JOIN [Gears] AS [g2] ON [w].[OwnerFullName] = [g2].[FullName]
) AS [t0] ON [g1].[FullName] = [t0].[OwnerFullName]
WHERE ([g].[Discriminator] = N'Officer') AND EXISTS (
    SELECT 1
    FROM [Gears] AS [g0]
    WHERE ([g].[Nickname] = [g0].[LeaderNickname]) AND ([g].[SquadId] = [g0].[LeaderSquadId]))
ORDER BY [g].[HasSoulPatch] DESC, [t].[Note], [g].[Nickname], [g].[SquadId], [t].[Id], [g1].[Nickname], [g1].[SquadId], [t0].[IsAutomatic], [t0].[Nickname] DESC, [t0].[Id], [t0].[SquadId]");
        }

        public override async Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery_duplicated_orderings(
            bool async)
        {
            await base.Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery_duplicated_orderings(async);

            AssertSql(
                @"SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t].[Id], [g1].[Nickname], [g1].[SquadId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[SynergyWithId], [t0].[Nickname], [t0].[SquadId]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
LEFT JOIN [Gears] AS [g1] ON ([t].[GearNickName] = [g1].[Nickname]) AND ([t].[GearSquadId] = [g1].[SquadId])
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [g2].[Nickname], [g2].[SquadId]
    FROM [Weapons] AS [w]
    LEFT JOIN [Gears] AS [g2] ON [w].[OwnerFullName] = [g2].[FullName]
) AS [t0] ON [g1].[FullName] = [t0].[OwnerFullName]
WHERE ([g].[Discriminator] = N'Officer') AND EXISTS (
    SELECT 1
    FROM [Gears] AS [g0]
    WHERE ([g].[Nickname] = [g0].[LeaderNickname]) AND ([g].[SquadId] = [g0].[LeaderSquadId]))
ORDER BY [g].[HasSoulPatch] DESC, [t].[Note], [g].[Nickname], [g].[SquadId], [t].[Id], [g1].[Nickname], [g1].[SquadId], [t0].[IsAutomatic], [t0].[Nickname] DESC, [t0].[Id], [t0].[SquadId]");
        }

        public override async Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery_complex_orderings(
            bool async)
        {
            await base.Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery_complex_orderings(async);

            AssertSql(
                @"SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t].[Id], [g1].[Nickname], [g1].[SquadId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[SynergyWithId], [t0].[Nickname], [t0].[SquadId]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
LEFT JOIN [Gears] AS [g1] ON ([t].[GearNickName] = [g1].[Nickname]) AND ([t].[GearSquadId] = [g1].[SquadId])
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [g2].[Nickname], [g2].[SquadId], (
        SELECT COUNT(*)
        FROM [Weapons] AS [w0]
        WHERE [g2].[FullName] IS NOT NULL AND ([g2].[FullName] = [w0].[OwnerFullName])) AS [c]
    FROM [Weapons] AS [w]
    LEFT JOIN [Gears] AS [g2] ON [w].[OwnerFullName] = [g2].[FullName]
) AS [t0] ON [g1].[FullName] = [t0].[OwnerFullName]
WHERE ([g].[Discriminator] = N'Officer') AND EXISTS (
    SELECT 1
    FROM [Gears] AS [g0]
    WHERE ([g].[Nickname] = [g0].[LeaderNickname]) AND ([g].[SquadId] = [g0].[LeaderSquadId]))
ORDER BY [g].[HasSoulPatch] DESC, [t].[Note], [g].[Nickname], [g].[SquadId], [t].[Id], [g1].[Nickname], [g1].[SquadId], [t0].[Id] DESC, [t0].[c], [t0].[Nickname], [t0].[SquadId]");
        }

        public override async Task Correlated_collections_multiple_nested_complex_collections(bool async)
        {
            await base.Correlated_collections_multiple_nested_complex_collections(async);

            AssertSql(
                @"SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t].[Id], [g1].[Nickname], [g1].[SquadId], [t1].[FullName], [t1].[Nickname], [t1].[SquadId], [t1].[Id], [t1].[Nickname0], [t1].[SquadId0], [t1].[Id0], [t1].[Name], [t1].[IsAutomatic], [t1].[Id1], [t1].[Nickname00], [t1].[HasSoulPatch], [t1].[SquadId00], [t3].[Id], [t3].[AmmunitionType], [t3].[IsAutomatic], [t3].[Name], [t3].[OwnerFullName], [t3].[SynergyWithId], [t3].[Nickname], [t3].[SquadId]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
LEFT JOIN [Gears] AS [g1] ON ([t].[GearNickName] = [g1].[Nickname]) AND ([t].[GearSquadId] = [g1].[SquadId])
LEFT JOIN (
    SELECT [g2].[FullName], [g2].[Nickname], [g2].[SquadId], [t0].[Id], [t0].[Nickname] AS [Nickname0], [t0].[SquadId] AS [SquadId0], [t0].[Id0], [t0].[Name], [t0].[IsAutomatic], [t0].[Id1], [t0].[Nickname0] AS [Nickname00], [t0].[HasSoulPatch], [t0].[SquadId0] AS [SquadId00], [g2].[Rank], [t0].[IsAutomatic0], [g2].[LeaderNickname], [g2].[LeaderSquadId]
    FROM [Gears] AS [g2]
    LEFT JOIN (
        SELECT [w].[Id], [g3].[Nickname], [g3].[SquadId], [s].[Id] AS [Id0], [w0].[Name], [w0].[IsAutomatic], [w0].[Id] AS [Id1], [t2].[Nickname] AS [Nickname0], [t2].[HasSoulPatch], [t2].[SquadId] AS [SquadId0], [w].[IsAutomatic] AS [IsAutomatic0], [w].[OwnerFullName]
        FROM [Weapons] AS [w]
        LEFT JOIN [Gears] AS [g3] ON [w].[OwnerFullName] = [g3].[FullName]
        LEFT JOIN [Squads] AS [s] ON [g3].[SquadId] = [s].[Id]
        LEFT JOIN [Weapons] AS [w0] ON [g3].[FullName] = [w0].[OwnerFullName]
        LEFT JOIN (
            SELECT [g4].[Nickname], [g4].[HasSoulPatch], [g4].[SquadId]
            FROM [Gears] AS [g4]
        ) AS [t2] ON [s].[Id] = [t2].[SquadId]
        WHERE ([w].[Name] <> N'Bar') OR [w].[Name] IS NULL
    ) AS [t0] ON [g2].[FullName] = [t0].[OwnerFullName]
    WHERE [g2].[FullName] <> N'Foo'
) AS [t1] ON ([g].[Nickname] = [t1].[LeaderNickname]) AND ([g].[SquadId] = [t1].[LeaderSquadId])
LEFT JOIN (
    SELECT [w1].[Id], [w1].[AmmunitionType], [w1].[IsAutomatic], [w1].[Name], [w1].[OwnerFullName], [w1].[SynergyWithId], [g5].[Nickname], [g5].[SquadId]
    FROM [Weapons] AS [w1]
    LEFT JOIN [Gears] AS [g5] ON [w1].[OwnerFullName] = [g5].[FullName]
) AS [t3] ON [g1].[FullName] = [t3].[OwnerFullName]
WHERE ([g].[Discriminator] = N'Officer') AND EXISTS (
    SELECT 1
    FROM [Gears] AS [g0]
    WHERE ([g].[Nickname] = [g0].[LeaderNickname]) AND ([g].[SquadId] = [g0].[LeaderSquadId]))
ORDER BY [g].[HasSoulPatch] DESC, [t].[Note], [g].[Nickname], [g].[SquadId], [t].[Id], [g1].[Nickname], [g1].[SquadId], [t1].[Rank], [t1].[Nickname], [t1].[SquadId], [t1].[IsAutomatic0], [t1].[Id], [t1].[Nickname0], [t1].[SquadId0], [t1].[Id0], [t1].[Id1], [t1].[Nickname00], [t1].[SquadId00], [t3].[IsAutomatic], [t3].[Nickname] DESC, [t3].[Id], [t3].[SquadId]");
        }

        public override async Task Correlated_collections_inner_subquery_selector_references_outer_qsre(bool async)
        {
            await base.Correlated_collections_inner_subquery_selector_references_outer_qsre(async);

            AssertSql(
                @"SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t].[ReportName], [t].[OfficerName], [t].[Nickname], [t].[SquadId]
FROM [Gears] AS [g]
OUTER APPLY (
    SELECT [g0].[FullName] AS [ReportName], [g].[FullName] AS [OfficerName], [g0].[Nickname], [g0].[SquadId]
    FROM [Gears] AS [g0]
    WHERE ([g].[Nickname] = [g0].[LeaderNickname]) AND ([g].[SquadId] = [g0].[LeaderSquadId])
) AS [t]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Nickname], [t].[SquadId]");
        }

        public override async Task Correlated_collections_inner_subquery_predicate_references_outer_qsre(bool async)
        {
            await base.Correlated_collections_inner_subquery_predicate_references_outer_qsre(async);

            AssertSql(
                @"SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t].[ReportName], [t].[Nickname], [t].[SquadId]
FROM [Gears] AS [g]
OUTER APPLY (
    SELECT [g0].[FullName] AS [ReportName], [g0].[Nickname], [g0].[SquadId]
    FROM [Gears] AS [g0]
    WHERE ([g].[FullName] <> N'Foo') AND (([g].[Nickname] = [g0].[LeaderNickname]) AND ([g].[SquadId] = [g0].[LeaderSquadId]))
) AS [t]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Nickname], [t].[SquadId]");
        }

        public override async Task Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up(bool async)
        {
            await base.Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up(async);

            AssertSql(
                @"SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t0].[FullName], [t0].[Nickname], [t0].[SquadId], [t0].[Name], [t0].[Nickname0], [t0].[Id]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [g0].[FullName], [g0].[Nickname], [g0].[SquadId], [t].[Name], [t].[Nickname] AS [Nickname0], [t].[Id], [g0].[LeaderNickname], [g0].[LeaderSquadId]
    FROM [Gears] AS [g0]
    OUTER APPLY (
        SELECT [w].[Name], [g0].[Nickname], [w].[Id]
        FROM [Weapons] AS [w]
        WHERE (([w].[Name] <> N'Bar') OR [w].[Name] IS NULL) AND ([g0].[FullName] = [w].[OwnerFullName])
    ) AS [t]
    WHERE [g0].[FullName] <> N'Foo'
) AS [t0] ON ([g].[Nickname] = [t0].[LeaderNickname]) AND ([g].[SquadId] = [t0].[LeaderSquadId])
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [t0].[Nickname], [t0].[SquadId], [t0].[Id]");
        }

        public override async Task Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up(bool async)
        {
            await base.Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up(async);

            AssertSql(
                @"SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t0].[FullName], [t0].[Nickname], [t0].[SquadId], [t0].[Name], [t0].[Nickname0], [t0].[Id]
FROM [Gears] AS [g]
OUTER APPLY (
    SELECT [g0].[FullName], [g0].[Nickname], [g0].[SquadId], [t].[Name], [t].[Nickname] AS [Nickname0], [t].[Id]
    FROM [Gears] AS [g0]
    LEFT JOIN (
        SELECT [w].[Name], [g].[Nickname], [w].[Id], [w].[OwnerFullName]
        FROM [Weapons] AS [w]
        WHERE ([w].[Name] <> N'Bar') OR [w].[Name] IS NULL
    ) AS [t] ON [g0].[FullName] = [t].[OwnerFullName]
    WHERE ([g0].[FullName] <> N'Foo') AND (([g].[Nickname] = [g0].[LeaderNickname]) AND ([g].[SquadId] = [g0].[LeaderSquadId]))
) AS [t0]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [t0].[Nickname], [t0].[SquadId], [t0].[Id]");
        }

        public override async Task Correlated_collections_on_select_many(bool async)
        {
            await base.Correlated_collections_on_select_many(async);

            AssertSql(
                @"SELECT [g].[Nickname], [s].[Name], [g].[SquadId], [s].[Id], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[SynergyWithId], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank]
FROM [Gears] AS [g]
CROSS JOIN [Squads] AS [s]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
    FROM [Weapons] AS [w]
    WHERE ([w].[IsAutomatic] = CAST(1 AS bit)) OR (([w].[Name] <> N'foo') OR [w].[Name] IS NULL)
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
    FROM [Gears] AS [g0]
    WHERE [g0].[HasSoulPatch] = CAST(0 AS bit)
) AS [t0] ON [s].[Id] = [t0].[SquadId]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
ORDER BY [g].[Nickname], [s].[Id] DESC, [g].[SquadId], [t].[Id], [t0].[Nickname], [t0].[SquadId]");
        }

        public override async Task Correlated_collections_with_Skip(bool async)
        {
            await base.Correlated_collections_with_Skip(async);

            AssertSql(
                @"SELECT [s].[Id]
FROM [Squads] AS [s]
ORDER BY [s].[Name]",
                //
                @"@_outer_Id='1'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE @_outer_Id = [g].[SquadId]
ORDER BY [g].[Nickname]
OFFSET 1 ROWS",
                //
                @"@_outer_Id='2'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE @_outer_Id = [g].[SquadId]
ORDER BY [g].[Nickname]
OFFSET 1 ROWS");
        }

        public override async Task Correlated_collections_with_Take(bool async)
        {
            await base.Correlated_collections_with_Take(async);

            AssertSql(
                @"SELECT [s].[Id]
FROM [Squads] AS [s]
ORDER BY [s].[Name]",
                //
                @"@_outer_Id='1'

SELECT TOP(2) [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE @_outer_Id = [g].[SquadId]
ORDER BY [g].[Nickname]",
                //
                @"@_outer_Id='2'

SELECT TOP(2) [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE @_outer_Id = [g].[SquadId]
ORDER BY [g].[Nickname]");
        }

        public override async Task Correlated_collections_with_Distinct(bool async)
        {
            await base.Correlated_collections_with_Distinct(async);

            AssertSql(
                @"SELECT [s].[Id], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank]
FROM [Squads] AS [s]
OUTER APPLY (
    SELECT DISTINCT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
        FROM [Gears] AS [g]
        WHERE [s].[Id] = [g].[SquadId]
        ORDER BY [g].[Nickname]
        OFFSET 0 ROWS
    ) AS [t]
) AS [t0]
ORDER BY [s].[Name], [s].[Id], [t0].[Nickname], [t0].[SquadId]");
        }

        public override async Task Correlated_collections_with_FirstOrDefault(bool async)
        {
            await base.Correlated_collections_with_FirstOrDefault(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [g].[FullName]
    FROM [Gears] AS [g]
    WHERE [s].[Id] = [g].[SquadId]
    ORDER BY [g].[Nickname])
FROM [Squads] AS [s]
ORDER BY [s].[Name]");
        }

        public override async Task Correlated_collections_on_left_join_with_predicate(bool async)
        {
            await base.Correlated_collections_on_left_join_with_predicate(async);

            AssertSql(
                @"SELECT [g].[Nickname], [t].[Id], [g].[SquadId], [w].[Name], [w].[Id]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON [t].[GearNickName] = [g].[Nickname]
LEFT JOIN [Weapons] AS [w] ON [g].[FullName] = [w].[OwnerFullName]
WHERE [g].[HasSoulPatch] = CAST(0 AS bit)
ORDER BY [t].[Id], [g].[Nickname], [g].[SquadId], [w].[Id]");
        }

        public override async Task Correlated_collections_on_left_join_with_null_value(bool async)
        {
            await base.Correlated_collections_on_left_join_with_null_value(async);

            AssertSql(
                @"SELECT [t].[Id], [g].[Nickname], [g].[SquadId], [w].[Name], [w].[Id]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON [t].[GearNickName] = [g].[Nickname]
LEFT JOIN [Weapons] AS [w] ON [g].[FullName] = [w].[OwnerFullName]
ORDER BY [t].[Note], [t].[Id], [g].[Nickname], [g].[SquadId], [w].[Id]");
        }

        public override async Task Correlated_collections_left_join_with_self_reference(bool async)
        {
            await base.Correlated_collections_left_join_with_self_reference(async);

            AssertSql(
                @"SELECT [t].[Note], [t].[Id], [t0].[Nickname], [t0].[SquadId], [g0].[FullName], [g0].[Nickname], [g0].[SquadId]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId]
    FROM [Gears] AS [g]
    WHERE [g].[Discriminator] = N'Officer'
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname]
LEFT JOIN [Gears] AS [g0] ON (([t0].[Nickname] = [g0].[LeaderNickname]) OR ([t0].[Nickname] IS NULL AND [g0].[LeaderNickname] IS NULL)) AND ([t0].[SquadId] = [g0].[LeaderSquadId])
ORDER BY [t].[Id], [t0].[Nickname], [t0].[SquadId], [g0].[Nickname], [g0].[SquadId]");
        }

        public override async Task Correlated_collections_deeply_nested_left_join(bool async)
        {
            await base.Correlated_collections_deeply_nested_left_join(async);

            AssertSql(
                @"SELECT [t].[Id], [g].[Nickname], [g].[SquadId], [s].[Id], [t0].[Nickname], [t0].[SquadId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[SynergyWithId]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON [t].[GearNickName] = [g].[Nickname]
LEFT JOIN [Squads] AS [s] ON [g].[SquadId] = [s].[Id]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [t1].[Id], [t1].[AmmunitionType], [t1].[IsAutomatic], [t1].[Name], [t1].[OwnerFullName], [t1].[SynergyWithId]
    FROM [Gears] AS [g0]
    LEFT JOIN (
        SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [w].[IsAutomatic] = CAST(1 AS bit)
    ) AS [t1] ON [g0].[FullName] = [t1].[OwnerFullName]
    WHERE [g0].[HasSoulPatch] = CAST(1 AS bit)
) AS [t0] ON [s].[Id] = [t0].[SquadId]
ORDER BY [t].[Note], [g].[Nickname] DESC, [t].[Id], [g].[SquadId], [s].[Id], [t0].[Nickname], [t0].[SquadId], [t0].[Id]");
        }

        public override async Task Correlated_collections_from_left_join_with_additional_elements_projected_of_that_join(bool async)
        {
            await base.Correlated_collections_from_left_join_with_additional_elements_projected_of_that_join(async);

            AssertSql(
                @"SELECT [w].[Id], [g].[Nickname], [g].[SquadId], [s].[Id], [t0].[Nickname], [t0].[SquadId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[SynergyWithId], [t0].[Rank]
FROM [Weapons] AS [w]
LEFT JOIN [Gears] AS [g] ON [w].[OwnerFullName] = [g].[FullName]
LEFT JOIN [Squads] AS [s] ON [g].[SquadId] = [s].[Id]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[SynergyWithId], [g0].[Rank], [g0].[FullName]
    FROM [Gears] AS [g0]
    LEFT JOIN (
        SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
        FROM [Weapons] AS [w0]
        WHERE [w0].[IsAutomatic] = CAST(0 AS bit)
    ) AS [t] ON [g0].[FullName] = [t].[OwnerFullName]
) AS [t0] ON [s].[Id] = [t0].[SquadId]
ORDER BY [w].[Name], [w].[Id], [g].[Nickname], [g].[SquadId], [s].[Id], [t0].[FullName] DESC, [t0].[Nickname], [t0].[SquadId], [t0].[Id]");
        }

        public override async Task Correlated_collections_complex_scenario1(bool async)
        {
            await base.Correlated_collections_complex_scenario1(async);

            AssertSql(
                @"SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t0].[Id], [t0].[Nickname], [t0].[SquadId], [t0].[Id0], [t0].[Nickname0], [t0].[HasSoulPatch], [t0].[SquadId0]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [w].[Id], [g0].[Nickname], [g0].[SquadId], [s].[Id] AS [Id0], [t].[Nickname] AS [Nickname0], [t].[HasSoulPatch], [t].[SquadId] AS [SquadId0], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
    LEFT JOIN [Gears] AS [g0] ON [w].[OwnerFullName] = [g0].[FullName]
    LEFT JOIN [Squads] AS [s] ON [g0].[SquadId] = [s].[Id]
    LEFT JOIN (
        SELECT [g1].[Nickname], [g1].[HasSoulPatch], [g1].[SquadId]
        FROM [Gears] AS [g1]
    ) AS [t] ON [s].[Id] = [t].[SquadId]
) AS [t0] ON [g].[FullName] = [t0].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [t0].[Id], [t0].[Nickname], [t0].[SquadId], [t0].[Id0], [t0].[Nickname0], [t0].[SquadId0]");
        }

        public override async Task Correlated_collections_complex_scenario2(bool async)
        {
            await base.Correlated_collections_complex_scenario2(async);

            AssertSql(
                @"SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t1].[FullName], [t1].[Nickname], [t1].[SquadId], [t1].[Id], [t1].[Nickname0], [t1].[SquadId0], [t1].[Id0], [t1].[Nickname00], [t1].[HasSoulPatch], [t1].[SquadId00]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [g0].[FullName], [g0].[Nickname], [g0].[SquadId], [t0].[Id], [t0].[Nickname] AS [Nickname0], [t0].[SquadId] AS [SquadId0], [t0].[Id0], [t0].[Nickname0] AS [Nickname00], [t0].[HasSoulPatch], [t0].[SquadId0] AS [SquadId00], [g0].[LeaderNickname], [g0].[LeaderSquadId]
    FROM [Gears] AS [g0]
    LEFT JOIN (
        SELECT [w].[Id], [g1].[Nickname], [g1].[SquadId], [s].[Id] AS [Id0], [t].[Nickname] AS [Nickname0], [t].[HasSoulPatch], [t].[SquadId] AS [SquadId0], [w].[OwnerFullName]
        FROM [Weapons] AS [w]
        LEFT JOIN [Gears] AS [g1] ON [w].[OwnerFullName] = [g1].[FullName]
        LEFT JOIN [Squads] AS [s] ON [g1].[SquadId] = [s].[Id]
        LEFT JOIN (
            SELECT [g2].[Nickname], [g2].[HasSoulPatch], [g2].[SquadId]
            FROM [Gears] AS [g2]
        ) AS [t] ON [s].[Id] = [t].[SquadId]
    ) AS [t0] ON [g0].[FullName] = [t0].[OwnerFullName]
) AS [t1] ON ([g].[Nickname] = [t1].[LeaderNickname]) AND ([g].[SquadId] = [t1].[LeaderSquadId])
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [t1].[Nickname], [t1].[SquadId], [t1].[Id], [t1].[Nickname0], [t1].[SquadId0], [t1].[Id0], [t1].[Nickname00], [t1].[SquadId00]");
        }

        public override async Task Correlated_collections_with_funky_orderby_complex_scenario1(bool async)
        {
            await base.Correlated_collections_with_funky_orderby_complex_scenario1(async);

            AssertSql(
                @"SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t0].[Id], [t0].[Nickname], [t0].[SquadId], [t0].[Id0], [t0].[Nickname0], [t0].[HasSoulPatch], [t0].[SquadId0]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [w].[Id], [g0].[Nickname], [g0].[SquadId], [s].[Id] AS [Id0], [t].[Nickname] AS [Nickname0], [t].[HasSoulPatch], [t].[SquadId] AS [SquadId0], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
    LEFT JOIN [Gears] AS [g0] ON [w].[OwnerFullName] = [g0].[FullName]
    LEFT JOIN [Squads] AS [s] ON [g0].[SquadId] = [s].[Id]
    LEFT JOIN (
        SELECT [g1].[Nickname], [g1].[HasSoulPatch], [g1].[SquadId]
        FROM [Gears] AS [g1]
    ) AS [t] ON [s].[Id] = [t].[SquadId]
) AS [t0] ON [g].[FullName] = [t0].[OwnerFullName]
ORDER BY [g].[FullName], [g].[Nickname] DESC, [g].[SquadId], [t0].[Id], [t0].[Nickname], [t0].[SquadId], [t0].[Id0], [t0].[Nickname0], [t0].[SquadId0]");
        }

        public override async Task Correlated_collections_with_funky_orderby_complex_scenario2(bool async)
        {
            await base.Correlated_collections_with_funky_orderby_complex_scenario2(async);

            AssertSql(
                @"SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t1].[FullName], [t1].[Nickname], [t1].[SquadId], [t1].[Id], [t1].[Nickname0], [t1].[SquadId0], [t1].[Id0], [t1].[Nickname00], [t1].[HasSoulPatch], [t1].[SquadId00]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [g0].[FullName], [g0].[Nickname], [g0].[SquadId], [t0].[Id], [t0].[Nickname] AS [Nickname0], [t0].[SquadId] AS [SquadId0], [t0].[Id0], [t0].[Nickname0] AS [Nickname00], [t0].[HasSoulPatch], [t0].[SquadId0] AS [SquadId00], [g0].[HasSoulPatch] AS [HasSoulPatch0], [t0].[IsAutomatic], [t0].[Name], [g0].[LeaderNickname], [g0].[LeaderSquadId]
    FROM [Gears] AS [g0]
    LEFT JOIN (
        SELECT [w].[Id], [g1].[Nickname], [g1].[SquadId], [s].[Id] AS [Id0], [t].[Nickname] AS [Nickname0], [t].[HasSoulPatch], [t].[SquadId] AS [SquadId0], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName]
        FROM [Weapons] AS [w]
        LEFT JOIN [Gears] AS [g1] ON [w].[OwnerFullName] = [g1].[FullName]
        LEFT JOIN [Squads] AS [s] ON [g1].[SquadId] = [s].[Id]
        LEFT JOIN (
            SELECT [g2].[Nickname], [g2].[HasSoulPatch], [g2].[SquadId]
            FROM [Gears] AS [g2]
        ) AS [t] ON [s].[Id] = [t].[SquadId]
    ) AS [t0] ON [g0].[FullName] = [t0].[OwnerFullName]
) AS [t1] ON ([g].[Nickname] = [t1].[LeaderNickname]) AND ([g].[SquadId] = [t1].[LeaderSquadId])
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[HasSoulPatch], [g].[LeaderNickname], [g].[FullName], [g].[Nickname], [g].[SquadId], [t1].[FullName], [t1].[HasSoulPatch0] DESC, [t1].[Nickname], [t1].[SquadId], [t1].[IsAutomatic], [t1].[Name] DESC, [t1].[Id], [t1].[Nickname0], [t1].[SquadId0], [t1].[Id0], [t1].[Nickname00], [t1].[SquadId00]");
        }

        public override async Task Correlated_collection_with_top_level_FirstOrDefault(bool async)
        {
            await base.Correlated_collection_with_top_level_FirstOrDefault(async);

            AssertSql(
                @"SELECT [t].[Nickname], [t].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM (
    SELECT TOP(1) [g].[Nickname], [g].[SquadId], [g].[FullName]
    FROM [Gears] AS [g]
    ORDER BY [g].[Nickname]
) AS [t]
LEFT JOIN [Weapons] AS [w] ON [t].[FullName] = [w].[OwnerFullName]
ORDER BY [t].[Nickname], [t].[SquadId], [w].[Id]");
        }

        public override async Task Correlated_collection_with_top_level_Count(bool async)
        {
            await base.Correlated_collection_with_top_level_Count(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Gears] AS [g]");
        }

        public override async Task Correlated_collection_with_top_level_Last_with_orderby_on_outer(bool async)
        {
            await base.Correlated_collection_with_top_level_Last_with_orderby_on_outer(async);

            AssertSql(
                @"SELECT [t].[Nickname], [t].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM (
    SELECT TOP(1) [g].[Nickname], [g].[SquadId], [g].[FullName]
    FROM [Gears] AS [g]
    ORDER BY [g].[FullName]
) AS [t]
LEFT JOIN [Weapons] AS [w] ON [t].[FullName] = [w].[OwnerFullName]
ORDER BY [t].[FullName], [t].[Nickname], [t].[SquadId], [w].[Id]");
        }

        public override async Task Correlated_collection_with_top_level_Last_with_order_by_on_inner(bool async)
        {
            await base.Correlated_collection_with_top_level_Last_with_order_by_on_inner(async);

            AssertSql(
                @"SELECT [t].[Nickname], [t].[SquadId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[SynergyWithId]
FROM (
    SELECT TOP(1) [g].[Nickname], [g].[SquadId], [g].[FullName]
    FROM [Gears] AS [g]
    ORDER BY [g].[FullName] DESC
) AS [t]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
    FROM [Weapons] AS [w]
) AS [t0] ON [t].[FullName] = [t0].[OwnerFullName]
ORDER BY [t].[FullName] DESC, [t].[Nickname], [t].[SquadId], [t0].[Name], [t0].[Id]");
        }

        public override async Task Null_semantics_on_nullable_bool_from_inner_join_subquery_is_fully_applied(bool async)
        {
            await base.Null_semantics_on_nullable_bool_from_inner_join_subquery_is_fully_applied(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[CapitalName], [t].[Discriminator], [t].[Name], [t].[CommanderName], [t].[Eradicated]
FROM [LocustLeaders] AS [l]
INNER JOIN (
    SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated]
    FROM [Factions] AS [f]
    WHERE [f].[Name] = N'Swarm'
) AS [t] ON [l].[Name] = [t].[CommanderName]
WHERE ([t].[Eradicated] <> CAST(1 AS bit)) OR [t].[Eradicated] IS NULL");
        }

        public override async Task Null_semantics_on_nullable_bool_from_left_join_subquery_is_fully_applied(bool async)
        {
            await base.Null_semantics_on_nullable_bool_from_left_join_subquery_is_fully_applied(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[CapitalName], [t].[Discriminator], [t].[Name], [t].[CommanderName], [t].[Eradicated]
FROM [LocustLeaders] AS [l]
LEFT JOIN (
    SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated]
    FROM [Factions] AS [f]
    WHERE [f].[Name] = N'Swarm'
) AS [t] ON [l].[Name] = [t].[CommanderName]
WHERE ([t].[Eradicated] <> CAST(1 AS bit)) OR [t].[Eradicated] IS NULL");
        }

        public override async Task Include_on_derived_type_with_order_by_and_paging(bool async)
        {
            await base.Include_on_derived_type_with_order_by_and_paging(async);

            AssertSql(
                @"@__p_0='10'

SELECT [t0].[Name], [t0].[Discriminator], [t0].[LocustHordeId], [t0].[ThreatLevel], [t0].[ThreatLevelByte], [t0].[ThreatLevelNullableByte], [t0].[DefeatedByNickname], [t0].[DefeatedBySquadId], [t0].[HighCommandId], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[Discriminator0], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Id], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM (
    SELECT TOP(@__p_0) [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator] AS [Discriminator0], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Id], [t].[Note]
    FROM [LocustLeaders] AS [l]
    LEFT JOIN [Gears] AS [g] ON ([l].[DefeatedByNickname] = [g].[Nickname]) AND ([l].[DefeatedBySquadId] = [g].[SquadId])
    LEFT JOIN [Tags] AS [t] ON (([g].[Nickname] = [t].[GearNickName]) OR ([g].[Nickname] IS NULL AND [t].[GearNickName] IS NULL)) AND (([g].[SquadId] = [t].[GearSquadId]) OR ([g].[SquadId] IS NULL AND [t].[GearSquadId] IS NULL))
    ORDER BY [t].[Note]
) AS [t0]
LEFT JOIN [Weapons] AS [w] ON [t0].[FullName] = [w].[OwnerFullName]
ORDER BY [t0].[Note], [t0].[Name], [t0].[Nickname], [t0].[SquadId], [t0].[Id], [w].[Id]");
        }

        public override async Task Select_required_navigation_on_derived_type(bool async)
        {
            await base.Select_required_navigation_on_derived_type(async);

            AssertSql(
                @"SELECT [l0].[Name]
FROM [LocustLeaders] AS [l]
LEFT JOIN [LocustHighCommands] AS [l0] ON [l].[HighCommandId] = [l0].[Id]");
        }

        public override async Task Select_required_navigation_on_the_same_type_with_cast(bool async)
        {
            await base.Select_required_navigation_on_the_same_type_with_cast(async);

            AssertSql(
                @"SELECT [c].[Name]
FROM [Gears] AS [g]
INNER JOIN [Cities] AS [c] ON [g].[CityOfBirthName] = [c].[Name]");
        }

        public override async Task Where_required_navigation_on_derived_type(bool async)
        {
            await base.Where_required_navigation_on_derived_type(async);

            AssertSql(
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [LocustLeaders] AS [l]
LEFT JOIN [LocustHighCommands] AS [l0] ON [l].[HighCommandId] = [l0].[Id]
WHERE [l0].[IsOperational] = CAST(1 AS bit)");
        }

        public override async Task Outer_parameter_in_join_key(bool async)
        {
            await base.Outer_parameter_in_join_key(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [t0].[Note], [t0].[Id], [t0].[Nickname], [t0].[SquadId]
FROM [Gears] AS [g]
OUTER APPLY (
    SELECT [t].[Note], [t].[Id], [g0].[Nickname], [g0].[SquadId]
    FROM [Tags] AS [t]
    INNER JOIN [Gears] AS [g0] ON [g].[FullName] = [g0].[FullName]
) AS [t0]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [t0].[Id], [t0].[Nickname], [t0].[SquadId]");
        }

        public override async Task Outer_parameter_in_join_key_inner_and_outer(bool async)
        {
            await base.Outer_parameter_in_join_key_inner_and_outer(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [t0].[Note], [t0].[Id], [t0].[Nickname], [t0].[SquadId]
FROM [Gears] AS [g]
OUTER APPLY (
    SELECT [t].[Note], [t].[Id], [g0].[Nickname], [g0].[SquadId]
    FROM [Tags] AS [t]
    INNER JOIN [Gears] AS [g0] ON [g].[FullName] = [g].[Nickname]
) AS [t0]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [t0].[Id], [t0].[Nickname], [t0].[SquadId]");
        }

        public override async Task Outer_parameter_in_group_join_with_DefaultIfEmpty(bool async)
        {
            await base.Outer_parameter_in_group_join_with_DefaultIfEmpty(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [t0].[Note], [t0].[Id], [t0].[Nickname], [t0].[SquadId]
FROM [Gears] AS [g]
OUTER APPLY (
    SELECT [t].[Note], [t].[Id], [g0].[Nickname], [g0].[SquadId]
    FROM [Tags] AS [t]
    LEFT JOIN [Gears] AS [g0] ON [g].[FullName] = [g0].[FullName]
) AS [t0]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [t0].[Id], [t0].[Nickname], [t0].[SquadId]");
        }

        public override async Task Negated_bool_ternary_inside_anonymous_type_in_projection(bool async)
        {
            await base.Negated_bool_ternary_inside_anonymous_type_in_projection(async);

            AssertSql(
                @"SELECT CASE
    WHEN CASE
        WHEN [g].[HasSoulPatch] = CAST(1 AS bit) THEN CAST(1 AS bit)
        ELSE COALESCE([g].[HasSoulPatch], CAST(1 AS bit))
    END = CAST(0 AS bit) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [c]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])");
        }

        public override async Task Order_by_entity_qsre(bool async)
        {
            await base.Order_by_entity_qsre(async);

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
LEFT JOIN [Cities] AS [c] ON [g].[AssignedCityName] = [c].[Name]
ORDER BY [c].[Name], [g].[Nickname] DESC");
        }

        public override async Task Order_by_entity_qsre_with_inheritance(bool async)
        {
            await base.Order_by_entity_qsre_with_inheritance(async);

            AssertSql(
                @"SELECT [l].[Name]
FROM [LocustLeaders] AS [l]
INNER JOIN [LocustHighCommands] AS [l0] ON [l].[HighCommandId] = [l0].[Id]
WHERE [l].[Discriminator] = N'LocustCommander'
ORDER BY [l0].[Id], [l].[Name]");
        }

        public override async Task Order_by_entity_qsre_composite_key(bool async)
        {
            await base.Order_by_entity_qsre_composite_key(async);

            AssertSql(
                @"SELECT [w].[Name]
FROM [Weapons] AS [w]
LEFT JOIN [Gears] AS [g] ON [w].[OwnerFullName] = [g].[FullName]
ORDER BY [g].[Nickname], [g].[SquadId], [w].[Id]");
        }

        public override async Task Order_by_entity_qsre_with_other_orderbys(bool async)
        {
            await base.Order_by_entity_qsre_with_other_orderbys(async);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Gears] AS [g] ON [w].[OwnerFullName] = [g].[FullName]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY [w].[IsAutomatic], [g].[Nickname] DESC, [g].[SquadId] DESC, [w0].[Id], [w].[Name]");
        }

        public override async Task Join_on_entity_qsre_keys(bool async)
        {
            await base.Join_on_entity_qsre_keys(async);

            AssertSql(
                @"SELECT [w].[Name] AS [Name1], [w0].[Name] AS [Name2]
FROM [Weapons] AS [w]
INNER JOIN [Weapons] AS [w0] ON [w].[Id] = [w0].[Id]");
        }

        public override async Task Join_on_entity_qsre_keys_composite_key(bool async)
        {
            await base.Join_on_entity_qsre_keys_composite_key(async);

            AssertSql(
                @"SELECT [g].[FullName] AS [GearName1], [g0].[FullName] AS [GearName2]
FROM [Gears] AS [g]
INNER JOIN [Gears] AS [g0] ON ([g].[Nickname] = [g0].[Nickname]) AND ([g].[SquadId] = [g0].[SquadId])");
        }

        public override async Task Join_on_entity_qsre_keys_inheritance(bool async)
        {
            await base.Join_on_entity_qsre_keys_inheritance(async);

            AssertSql(
                @"SELECT [g].[FullName] AS [GearName], [t].[FullName] AS [OfficerName]
FROM [Gears] AS [g]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[FullName]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] = N'Officer'
) AS [t] ON ([g].[Nickname] = [t].[Nickname]) AND ([g].[SquadId] = [t].[SquadId])");
        }

        public override async Task Join_on_entity_qsre_keys_outer_key_is_navigation(bool async)
        {
            await base.Join_on_entity_qsre_keys_outer_key_is_navigation(async);

            AssertSql(
                @"SELECT [w].[Name] AS [Name1], [w1].[Name] AS [Name2]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
INNER JOIN [Weapons] AS [w1] ON [w0].[Id] = [w1].[Id]");
        }

        public override async Task Join_on_entity_qsre_keys_inner_key_is_navigation(bool async)
        {
            await base.Join_on_entity_qsre_keys_inner_key_is_navigation(async);

            AssertSql(
                @"SELECT [c].[Name] AS [CityName], [t].[Nickname] AS [GearNickname]
FROM [Cities] AS [c]
INNER JOIN (
    SELECT [g].[Nickname], [c0].[Name]
    FROM [Gears] AS [g]
    LEFT JOIN [Cities] AS [c0] ON [g].[AssignedCityName] = [c0].[Name]
) AS [t] ON [c].[Name] = [t].[Name]");
        }

        public override async Task Join_on_entity_qsre_keys_inner_key_is_navigation_composite_key(bool async)
        {
            await base.Join_on_entity_qsre_keys_inner_key_is_navigation_composite_key(async);

            AssertSql(
                @"SELECT [g].[Nickname], [t0].[Note]
FROM [Gears] AS [g]
INNER JOIN (
    SELECT [t].[Note], [g0].[Nickname], [g0].[SquadId]
    FROM [Tags] AS [t]
    LEFT JOIN [Gears] AS [g0] ON ([t].[GearNickName] = [g0].[Nickname]) AND ([t].[GearSquadId] = [g0].[SquadId])
    WHERE [t].[Note] IN (N'Cole''s Tag', N'Dom''s Tag')
) AS [t0] ON ([g].[Nickname] = [t0].[Nickname]) AND ([g].[SquadId] = [t0].[SquadId])");
        }

        public override async Task Join_on_entity_qsre_keys_inner_key_is_nested_navigation(bool async)
        {
            await base.Join_on_entity_qsre_keys_inner_key_is_nested_navigation(async);

            AssertSql(
                @"SELECT [s].[Name] AS [SquadName], [t].[Name] AS [WeaponName]
FROM [Squads] AS [s]
INNER JOIN (
    SELECT [w].[Name], [s0].[Id] AS [Id0]
    FROM [Weapons] AS [w]
    LEFT JOIN [Gears] AS [g] ON [w].[OwnerFullName] = [g].[FullName]
    LEFT JOIN [Squads] AS [s0] ON [g].[SquadId] = [s0].[Id]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit)
) AS [t] ON [s].[Id] = [t].[Id0]");
        }

        public override async Task GroupJoin_on_entity_qsre_keys_inner_key_is_nested_navigation(bool async)
        {
            await base.GroupJoin_on_entity_qsre_keys_inner_key_is_nested_navigation(async);

            AssertSql(
                @"SELECT [s].[Name] AS [SquadName], [t].[Name] AS [WeaponName]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [w].[Name], [s0].[Id] AS [Id0]
    FROM [Weapons] AS [w]
    LEFT JOIN [Gears] AS [g] ON [w].[OwnerFullName] = [g].[FullName]
    LEFT JOIN [Squads] AS [s0] ON [g].[SquadId] = [s0].[Id]
) AS [t] ON [s].[Id] = [t].[Id0]");
        }

        public override async Task Streaming_correlated_collection_issue_11403(bool async)
        {
            await base.Streaming_correlated_collection_issue_11403(async);

            AssertSql(
                @"SELECT [t].[Nickname], [t].[SquadId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[SynergyWithId]
FROM (
    SELECT TOP(1) [g].[Nickname], [g].[SquadId], [g].[FullName]
    FROM [Gears] AS [g]
    ORDER BY [g].[Nickname]
) AS [t]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
    FROM [Weapons] AS [w]
    WHERE [w].[IsAutomatic] = CAST(0 AS bit)
) AS [t0] ON [t].[FullName] = [t0].[OwnerFullName]
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Id]");
        }

        public override async Task Project_one_value_type_from_empty_collection(bool async)
        {
            await base.Project_one_value_type_from_empty_collection(async);

            AssertSql(
                @"SELECT [s].[Name], COALESCE((
    SELECT TOP(1) [g].[SquadId]
    FROM [Gears] AS [g]
    WHERE ([s].[Id] = [g].[SquadId]) AND ([g].[HasSoulPatch] = CAST(1 AS bit))), 0) AS [SquadId]
FROM [Squads] AS [s]
WHERE [s].[Name] = N'Kilo'");
        }

        public override async Task Project_one_value_type_converted_to_nullable_from_empty_collection(bool async)
        {
            await base.Project_one_value_type_converted_to_nullable_from_empty_collection(async);

            AssertSql(
                @"SELECT [s].[Name], (
    SELECT TOP(1) [g].[SquadId]
    FROM [Gears] AS [g]
    WHERE ([s].[Id] = [g].[SquadId]) AND ([g].[HasSoulPatch] = CAST(1 AS bit))) AS [SquadId]
FROM [Squads] AS [s]
WHERE [s].[Name] = N'Kilo'");
        }

        public override async Task Project_one_value_type_with_client_projection_from_empty_collection(bool async)
        {
            await base.Project_one_value_type_with_client_projection_from_empty_collection(async);

            AssertSql(
                @"SELECT [s].[Name], [t0].[SquadId], [t0].[LeaderSquadId], [t0].[c]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [t].[SquadId], [t].[LeaderSquadId], [t].[c]
    FROM (
        SELECT [g].[SquadId], [g].[LeaderSquadId], 1 AS [c], ROW_NUMBER() OVER(PARTITION BY [g].[SquadId] ORDER BY [g].[Nickname], [g].[SquadId]) AS [row]
        FROM [Gears] AS [g]
        WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [s].[Id] = [t0].[SquadId]
WHERE [s].[Name] = N'Kilo'");
        }

        public override async Task Filter_on_subquery_projecting_one_value_type_from_empty_collection(bool async)
        {
            await base.Filter_on_subquery_projecting_one_value_type_from_empty_collection(async);

            AssertSql(
                @"SELECT [s].[Name]
FROM [Squads] AS [s]
WHERE ([s].[Name] = N'Kilo') AND (COALESCE((
    SELECT TOP(1) [g].[SquadId]
    FROM [Gears] AS [g]
    WHERE ([g].[Discriminator] IN (N'Officer', N'Gear') AND ([s].[Id] = [g].[SquadId])) AND ([g].[HasSoulPatch] = CAST(1 AS bit))
), 0) <> 0)");
        }

        public override async Task Select_subquery_projecting_single_constant_int(bool async)
        {
            await base.Select_subquery_projecting_single_constant_int(async);

            AssertSql(
                @"SELECT [s].[Name], COALESCE((
    SELECT TOP(1) 42
    FROM [Gears] AS [g]
    WHERE ([s].[Id] = [g].[SquadId]) AND ([g].[HasSoulPatch] = CAST(1 AS bit))), 0) AS [Gear]
FROM [Squads] AS [s]");
        }

        public override async Task Select_subquery_projecting_single_constant_string(bool async)
        {
            await base.Select_subquery_projecting_single_constant_string(async);

            AssertSql(
                @"SELECT [s].[Name], (
    SELECT TOP(1) N'Foo'
    FROM [Gears] AS [g]
    WHERE ([s].[Id] = [g].[SquadId]) AND ([g].[HasSoulPatch] = CAST(1 AS bit))) AS [Gear]
FROM [Squads] AS [s]");
        }

        public override async Task Select_subquery_projecting_single_constant_bool(bool async)
        {
            await base.Select_subquery_projecting_single_constant_bool(async);

            AssertSql(
                @"SELECT [s].[Name], COALESCE((
    SELECT TOP(1) CAST(1 AS bit)
    FROM [Gears] AS [g]
    WHERE ([s].[Id] = [g].[SquadId]) AND ([g].[HasSoulPatch] = CAST(1 AS bit))), CAST(0 AS bit)) AS [Gear]
FROM [Squads] AS [s]");
        }

        public override async Task Select_subquery_projecting_single_constant_inside_anonymous(bool async)
        {
            await base.Select_subquery_projecting_single_constant_inside_anonymous(async);

            AssertSql(
                @"SELECT [s].[Name], [t0].[One]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [t].[One], [t].[SquadId]
    FROM (
        SELECT 1 AS [One], [g].[SquadId], ROW_NUMBER() OVER(PARTITION BY [g].[SquadId] ORDER BY [g].[Nickname], [g].[SquadId]) AS [row]
        FROM [Gears] AS [g]
        WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [s].[Id] = [t0].[SquadId]");
        }

        public override async Task Select_subquery_projecting_multiple_constants_inside_anonymous(bool async)
        {
            await base.Select_subquery_projecting_multiple_constants_inside_anonymous(async);

            AssertSql(
                @"SELECT [s].[Name], [t0].[True1], [t0].[False1], [t0].[c]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [t].[True1], [t].[False1], [t].[c], [t].[SquadId]
    FROM (
        SELECT CAST(1 AS bit) AS [True1], CAST(0 AS bit) AS [False1], 1 AS [c], [g].[SquadId], ROW_NUMBER() OVER(PARTITION BY [g].[SquadId] ORDER BY [g].[Nickname], [g].[SquadId]) AS [row]
        FROM [Gears] AS [g]
        WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [s].[Id] = [t0].[SquadId]");
        }

        public override async Task Include_with_order_by_constant(bool async)
        {
            await base.Include_with_order_by_constant(async);

            AssertSql(
                @"SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Squads] AS [s]
LEFT JOIN [Gears] AS [g] ON [s].[Id] = [g].[SquadId]
ORDER BY [s].[Id], [g].[Nickname], [g].[SquadId]");
        }

        public override async Task Correlated_collection_order_by_constant(bool async)
        {
            await base.Correlated_collection_order_by_constant(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [w].[Name], [w].[Id]
FROM [Gears] AS [g]
LEFT JOIN [Weapons] AS [w] ON [g].[FullName] = [w].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [w].[Id]");
        }

        public override async Task Select_subquery_projecting_single_constant_null_of_non_mapped_type(bool async)
        {
            await base.Select_subquery_projecting_single_constant_null_of_non_mapped_type(async);

            AssertSql(
                @"SELECT [s].[Name], [t0].[c]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [t].[c], [t].[SquadId]
    FROM (
        SELECT 1 AS [c], [g].[SquadId], ROW_NUMBER() OVER(PARTITION BY [g].[SquadId] ORDER BY [g].[Nickname], [g].[SquadId]) AS [row]
        FROM [Gears] AS [g]
        WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [s].[Id] = [t0].[SquadId]");
        }

        public override async Task Select_subquery_projecting_single_constant_of_non_mapped_type(bool async)
        {
            await base.Select_subquery_projecting_single_constant_of_non_mapped_type(async);

            AssertSql(
                @"SELECT [s].[Name], [t0].[c]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [t].[c], [t].[SquadId]
    FROM (
        SELECT 1 AS [c], [g].[SquadId], ROW_NUMBER() OVER(PARTITION BY [g].[SquadId] ORDER BY [g].[Nickname], [g].[SquadId]) AS [row]
        FROM [Gears] AS [g]
        WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [s].[Id] = [t0].[SquadId]");
        }

        public override async Task Include_collection_OrderBy_aggregate(bool async)
        {
            await base.Include_collection_OrderBy_aggregate(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Gears] AS [g0] ON ([g].[Nickname] = [g0].[LeaderNickname]) AND ([g].[SquadId] = [g0].[LeaderSquadId])
WHERE [g].[Discriminator] = N'Officer'
ORDER BY (
    SELECT COUNT(*)
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]), [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId]");
        }

        public override async Task Include_collection_with_complex_OrderBy2(bool async)
        {
            await base.Include_collection_with_complex_OrderBy2(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Gears] AS [g0] ON ([g].[Nickname] = [g0].[LeaderNickname]) AND ([g].[SquadId] = [g0].[LeaderSquadId])
WHERE [g].[Discriminator] = N'Officer'
ORDER BY (
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]), [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId]");
        }

        public override async Task Include_collection_with_complex_OrderBy3(bool async)
        {
            await base.Include_collection_with_complex_OrderBy3(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Gears] AS [g0] ON ([g].[Nickname] = [g0].[LeaderNickname]) AND ([g].[SquadId] = [g0].[LeaderSquadId])
WHERE [g].[Discriminator] = N'Officer'
ORDER BY COALESCE((
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]), CAST(0 AS bit)), [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId]");
        }

        public override async Task Correlated_collection_with_complex_OrderBy(bool async)
        {
            await base.Correlated_collection_with_complex_OrderBy(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
    FROM [Gears] AS [g0]
    WHERE [g0].[HasSoulPatch] = CAST(0 AS bit)
) AS [t] ON ([g].[Nickname] = [t].[LeaderNickname]) AND ([g].[SquadId] = [t].[LeaderSquadId])
WHERE [g].[Discriminator] = N'Officer'
ORDER BY (
    SELECT COUNT(*)
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]), [g].[Nickname], [g].[SquadId], [t].[Nickname], [t].[SquadId]");
        }

        public override async Task Correlated_collection_with_very_complex_order_by(bool async)
        {
            await base.Correlated_collection_with_very_complex_order_by(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOfBirthName], [g1].[Discriminator], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[Rank]
    FROM [Gears] AS [g1]
    WHERE [g1].[HasSoulPatch] = CAST(0 AS bit)
) AS [t] ON ([g].[Nickname] = [t].[LeaderNickname]) AND ([g].[SquadId] = [t].[LeaderSquadId])
WHERE [g].[Discriminator] = N'Officer'
ORDER BY (
    SELECT COUNT(*)
    FROM [Weapons] AS [w]
    WHERE ([g].[FullName] = [w].[OwnerFullName]) AND ([w].[IsAutomatic] = COALESCE((
        SELECT TOP(1) [g0].[HasSoulPatch]
        FROM [Gears] AS [g0]
        WHERE [g0].[Nickname] = N'Marcus'), CAST(0 AS bit)))), [g].[Nickname], [g].[SquadId], [t].[Nickname], [t].[SquadId]");
        }

        public override async Task Cast_to_derived_type_after_OfType_works(bool async)
        {
            await base.Cast_to_derived_type_after_OfType_works(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] = N'Officer'");
        }

        public override async Task Select_subquery_boolean(bool async)
        {
            await base.Select_subquery_boolean(async);

            AssertSql(
                @"SELECT COALESCE((
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]), CAST(0 AS bit))
FROM [Gears] AS [g]");
        }

        public override async Task Select_subquery_boolean_with_pushdown(bool async)
        {
            await base.Select_subquery_boolean_with_pushdown(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id])
FROM [Gears] AS [g]");
        }

        public override async Task Select_subquery_int_with_inside_cast_and_coalesce(bool async)
        {
            await base.Select_subquery_int_with_inside_cast_and_coalesce(async);

            AssertSql(
                @"SELECT COALESCE((
    SELECT TOP(1) [w].[Id]
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]), 42)
FROM [Gears] AS [g]");
        }

        public override async Task Select_subquery_int_with_outside_cast_and_coalesce(bool async)
        {
            await base.Select_subquery_int_with_outside_cast_and_coalesce(async);

            AssertSql(
                @"SELECT COALESCE(COALESCE((
    SELECT TOP(1) [w].[Id]
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]), 0), 42)
FROM [Gears] AS [g]");
        }

        public override async Task Select_subquery_int_with_pushdown_and_coalesce(bool async)
        {
            await base.Select_subquery_int_with_pushdown_and_coalesce(async);

            AssertSql(
                @"SELECT COALESCE((
    SELECT TOP(1) [w].[Id]
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]), 42)
FROM [Gears] AS [g]");
        }

        public override async Task Select_subquery_int_with_pushdown_and_coalesce2(bool async)
        {
            await base.Select_subquery_int_with_pushdown_and_coalesce2(async);

            AssertSql(
                @"SELECT COALESCE((
    SELECT TOP(1) [w].[Id]
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]), (
    SELECT TOP(1) [w0].[Id]
    FROM [Weapons] AS [w0]
    WHERE [g].[FullName] = [w0].[OwnerFullName]
    ORDER BY [w0].[Id]))
FROM [Gears] AS [g]");
        }

        public override async Task Select_subquery_boolean_empty(bool async)
        {
            await base.Select_subquery_boolean_empty(async);

            AssertSql(
                @"SELECT COALESCE((
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE ([g].[FullName] = [w].[OwnerFullName]) AND ([w].[Name] = N'BFG')
    ORDER BY [w].[Id]), CAST(0 AS bit))
FROM [Gears] AS [g]");
        }

        public override async Task Select_subquery_boolean_empty_with_pushdown(bool async)
        {
            await base.Select_subquery_boolean_empty_with_pushdown(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE ([g].[FullName] = [w].[OwnerFullName]) AND ([w].[Name] = N'BFG')
    ORDER BY [w].[Id])
FROM [Gears] AS [g]");
        }

        public override async Task Select_subquery_distinct_singleordefault_boolean1(bool async)
        {
            await base.Select_subquery_distinct_singleordefault_boolean1(async);

            AssertSql(
                @"SELECT COALESCE((
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE ([g].[FullName] = [w].[OwnerFullName]) AND ([w].[Name] LIKE N'%Lancer%')
    ) AS [t]), CAST(0 AS bit))
FROM [Gears] AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit)");
        }

        public override async Task Select_subquery_distinct_singleordefault_boolean2(bool async)
        {
            await base.Select_subquery_distinct_singleordefault_boolean2(async);

            AssertSql(
                @"SELECT COALESCE((
    SELECT DISTINCT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE ([g].[FullName] = [w].[OwnerFullName]) AND ([w].[Name] LIKE N'%Lancer%')), CAST(0 AS bit))
FROM [Gears] AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit)");
        }

        public override async Task Select_subquery_distinct_singleordefault_boolean_with_pushdown(bool async)
        {
            await base.Select_subquery_distinct_singleordefault_boolean_with_pushdown(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE ([g].[FullName] = [w].[OwnerFullName]) AND ([w].[Name] LIKE N'%Lancer%')
    ) AS [t])
FROM [Gears] AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit)");
        }

        public override async Task Select_subquery_distinct_singleordefault_boolean_empty1(bool async)
        {
            await base.Select_subquery_distinct_singleordefault_boolean_empty1(async);

            AssertSql(
                @"SELECT COALESCE((
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE ([g].[FullName] = [w].[OwnerFullName]) AND ([w].[Name] = N'BFG')
    ) AS [t]), CAST(0 AS bit))
FROM [Gears] AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit)");
        }

        public override async Task Select_subquery_distinct_singleordefault_boolean_empty2(bool async)
        {
            await base.Select_subquery_distinct_singleordefault_boolean_empty2(async);

            AssertSql(
                @"SELECT COALESCE((
    SELECT DISTINCT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE ([g].[FullName] = [w].[OwnerFullName]) AND ([w].[Name] = N'BFG')), CAST(0 AS bit))
FROM [Gears] AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit)");
        }

        public override async Task Select_subquery_distinct_singleordefault_boolean_empty_with_pushdown(bool async)
        {
            await base.Select_subquery_distinct_singleordefault_boolean_empty_with_pushdown(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE ([g].[FullName] = [w].[OwnerFullName]) AND ([w].[Name] = N'BFG')
    ) AS [t])
FROM [Gears] AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit)");
        }

        public override async Task Cast_subquery_to_base_type_using_typed_ToList(bool async)
        {
            await base.Cast_subquery_to_base_type_using_typed_ToList(async);

            AssertSql(
                @"SELECT [c].[Name], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Nickname], [g].[Rank], [g].[SquadId]
FROM [Cities] AS [c]
LEFT JOIN [Gears] AS [g] ON [c].[Name] = [g].[AssignedCityName]
WHERE [c].[Name] = N'Ephyra'
ORDER BY [c].[Name], [g].[Nickname], [g].[SquadId]");
        }

        public override async Task Cast_ordered_subquery_to_base_type_using_typed_ToArray(bool async)
        {
            await base.Cast_ordered_subquery_to_base_type_using_typed_ToArray(async);

            AssertSql(
                @"SELECT [c].[Name], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Nickname], [t].[Rank], [t].[SquadId]
FROM [Cities] AS [c]
LEFT JOIN (
    SELECT [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Nickname], [g].[Rank], [g].[SquadId], [g].[AssignedCityName]
    FROM [Gears] AS [g]
) AS [t] ON [c].[Name] = [t].[AssignedCityName]
WHERE [c].[Name] = N'Ephyra'
ORDER BY [c].[Name], [t].[Nickname] DESC, [t].[SquadId]");
        }

        public override async Task Correlated_collection_with_complex_order_by_funcletized_to_constant_bool(bool async)
        {
            await base.Correlated_collection_with_complex_order_by_funcletized_to_constant_bool(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname], [g].[SquadId], [g].[FullName]",
                //
                @"SELECT [t].[c], [t].[Nickname], [t].[SquadId], [t].[FullName], [g.Weapons].[Name], [g.Weapons].[OwnerFullName]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT CAST(0 AS bit) AS [c], [g0].[Nickname], [g0].[SquadId], [g0].[FullName]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
ORDER BY [t].[c] DESC, [t].[Nickname], [t].[SquadId], [t].[FullName]");
        }

        public override async Task Double_order_by_on_nullable_bool_coming_from_optional_navigation(bool async)
        {
            await base.Double_order_by_on_nullable_bool_coming_from_optional_navigation(async);

            AssertSql(
                @"SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY [w0].[IsAutomatic], [w0].[Id]");
        }

        public override async Task Double_order_by_on_Like(bool async)
        {
            await base.Double_order_by_on_Like(async);

            AssertSql(
                @"SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY CASE
    WHEN [w0].[Name] LIKE N'%Lancer' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Double_order_by_on_is_null(bool async)
        {
            await base.Double_order_by_on_is_null(async);

            AssertSql(
                @"SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY CASE
    WHEN [w0].[Name] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Double_order_by_on_string_compare(bool async)
        {
            await base.Double_order_by_on_string_compare(async);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
ORDER BY CASE
    WHEN ([w].[Name] = N'Marcus'' Lancer') AND [w].[Name] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [w].[Id]");
        }

        public override async Task Double_order_by_binary_expression(bool async)
        {
            await base.Double_order_by_binary_expression(async);

            AssertSql(
                @"SELECT [w].[Id] + 2 AS [Binary]
FROM [Weapons] AS [w]
ORDER BY [w].[Id] + 2");
        }

        public override async Task String_compare_with_null_conditional_argument(bool async)
        {
            await base.String_compare_with_null_conditional_argument(async);

            AssertSql(
                @"SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY CASE
    WHEN ([w0].[Name] = N'Marcus'' Lancer') AND [w0].[Name] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task String_compare_with_null_conditional_argument2(bool async)
        {
            await base.String_compare_with_null_conditional_argument2(async);

            AssertSql(
                @"SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY CASE
    WHEN (N'Marcus'' Lancer' = [w0].[Name]) AND [w0].[Name] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task String_concat_with_null_conditional_argument(bool async)
        {
            await base.String_concat_with_null_conditional_argument(async);

            AssertSql(
                @"SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY COALESCE([w0].[Name], N'') + CAST(5 AS nvarchar(max))");
        }

        public override async Task String_concat_with_null_conditional_argument2(bool async)
        {
            await base.String_concat_with_null_conditional_argument2(async);

            AssertSql(
                @"SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY COALESCE([w0].[Name], N'') + N'Marcus'' Lancer'");
        }

        public override async Task String_concat_on_various_types(bool async)
        {
            await base.String_concat_on_various_types(async);

            AssertSql(
                "");
        }

        public override async Task Time_of_day_datetimeoffset(bool async)
        {
            await base.Time_of_day_datetimeoffset(async);

            AssertSql(
                @"SELECT CONVERT(time, [m].[Timeline])
FROM [Missions] AS [m]");
        }

        public override async Task GroupBy_Property_Include_Select_Average(bool async)
        {
            await base.GroupBy_Property_Include_Select_Average(async);

            AssertSql(
                @"SELECT AVG(CAST([g].[SquadId] AS float))
FROM [Gears] AS [g]
GROUP BY [g].[Rank]");
        }

        public override async Task GroupBy_Property_Include_Select_Sum(bool async)
        {
            await base.GroupBy_Property_Include_Select_Sum(async);

            AssertSql(
                @"SELECT COALESCE(SUM([g].[SquadId]), 0)
FROM [Gears] AS [g]
GROUP BY [g].[Rank]");
        }

        public override async Task GroupBy_Property_Include_Select_Count(bool async)
        {
            await base.GroupBy_Property_Include_Select_Count(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Gears] AS [g]
GROUP BY [g].[Rank]");
        }

        public override async Task GroupBy_Property_Include_Select_LongCount(bool async)
        {
            await base.GroupBy_Property_Include_Select_LongCount(async);

            AssertSql(
                @"SELECT COUNT_BIG(*)
FROM [Gears] AS [g]
GROUP BY [g].[Rank]");
        }

        public override async Task GroupBy_Property_Include_Select_Min(bool async)
        {
            await base.GroupBy_Property_Include_Select_Min(async);

            AssertSql(
                @"SELECT MIN([g].[SquadId])
FROM [Gears] AS [g]
GROUP BY [g].[Rank]");
        }

        public override async Task GroupBy_Property_Include_Aggregate_with_anonymous_selector(bool async)
        {
            await base.GroupBy_Property_Include_Aggregate_with_anonymous_selector(async);

            AssertSql(
                @"SELECT [g].[Nickname] AS [Key], COUNT(*) AS [c]
FROM [Gears] AS [g]
GROUP BY [g].[Nickname]
ORDER BY [g].[Nickname]");
        }

        public override async Task Group_by_entity_key_with_include_on_that_entity_with_key_in_result_selector(bool async)
        {
            await base.Group_by_entity_key_with_include_on_that_entity_with_key_in_result_selector(async);

            AssertSql(
                "");
        }

        public override async Task Group_by_entity_key_with_include_on_that_entity_with_key_in_result_selector_using_EF_Property(
            bool async)
        {
            await base.Group_by_entity_key_with_include_on_that_entity_with_key_in_result_selector_using_EF_Property(async);

            AssertSql(
                "");
        }

        public override async Task Group_by_with_include_with_entity_in_result_selector(bool async)
        {
            await base.Group_by_with_include_with_entity_in_result_selector(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.CityOfBirth].[Name], [g.CityOfBirth].[Location], [g.CityOfBirth].[Nation]
FROM [Gears] AS [g]
INNER JOIN [Cities] AS [g.CityOfBirth] ON [g].[CityOfBirthName] = [g.CityOfBirth].[Name]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Rank]");
        }

        public override async Task GroupBy_Property_Include_Select_Max(bool async)
        {
            await base.GroupBy_Property_Include_Select_Max(async);

            AssertSql(
                @"SELECT MAX([g].[SquadId])
FROM [Gears] AS [g]
GROUP BY [g].[Rank]");
        }

        public override async Task Include_with_group_by_and_FirstOrDefault_gets_properly_applied(bool async)
        {
            await base.Include_with_group_by_and_FirstOrDefault_gets_properly_applied(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.CityOfBirth].[Name], [g.CityOfBirth].[Location], [g.CityOfBirth].[Nation]
FROM [Gears] AS [g]
INNER JOIN [Cities] AS [g.CityOfBirth] ON [g].[CityOfBirthName] = [g.CityOfBirth].[Name]
ORDER BY [g].[Rank]");
        }

        public override async Task Include_collection_with_Cast_to_base(bool async)
        {
            await base.Include_collection_with_Cast_to_base(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Gears] AS [g]
LEFT JOIN [Weapons] AS [w] ON [g].[FullName] = [w].[OwnerFullName]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [w].[Id]");
        }

        public override async Task Include_with_client_method_and_member_access_still_applies_includes(bool async)
        {
            await base.Include_with_client_method_and_member_access_still_applies_includes(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])");
        }

        public override async Task Include_with_projection_of_unmapped_property_still_gets_applied(bool async)
        {
            await base.Include_with_projection_of_unmapped_property_still_gets_applied(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Gears] AS [g]
LEFT JOIN [Weapons] AS [w] ON [g].[FullName] = [w].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [w].[Id]");
        }

        public override async Task Multiple_includes_with_client_method_around_entity_and_also_projecting_included_collection()
        {
            await base.Multiple_includes_with_client_method_around_entity_and_also_projecting_included_collection();

            AssertSql(
                @"SELECT [s].[Name], [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[SynergyWithId]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
    FROM [Gears] AS [g]
    LEFT JOIN [Weapons] AS [w] ON [g].[FullName] = [w].[OwnerFullName]
) AS [t] ON [s].[Id] = [t].[SquadId]
WHERE [s].[Name] = N'Delta'
ORDER BY [s].[Id], [t].[Nickname], [t].[SquadId], [t].[Id]");
        }

        public override async Task OrderBy_same_expression_containing_IsNull_correctly_deduplicates_the_ordering(bool async)
        {
            await base.OrderBy_same_expression_containing_IsNull_correctly_deduplicates_the_ordering(async);

            AssertSql(
                @"SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN CASE
        WHEN CAST(LEN([g].[Nickname]) AS int) = 5 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    ELSE NULL
END
FROM [Gears] AS [g]
ORDER BY CASE
    WHEN CASE
        WHEN [g].[LeaderNickname] IS NOT NULL THEN CASE
            WHEN CAST(LEN([g].[Nickname]) AS int) = 5 THEN CAST(1 AS bit)
            ELSE CAST(0 AS bit)
        END
        ELSE NULL
    END IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task GetValueOrDefault_in_projection(bool async)
        {
            await base.GetValueOrDefault_in_projection(async);

            AssertSql(
                @"SELECT COALESCE([w].[SynergyWithId], 0)
FROM [Weapons] AS [w]");
        }

        public override async Task GetValueOrDefault_in_filter(bool async)
        {
            await base.GetValueOrDefault_in_filter(async);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE COALESCE([w].[SynergyWithId], 0) = 0");
        }

        public override async Task GetValueOrDefault_in_filter_non_nullable_column(bool async)
        {
            await base.GetValueOrDefault_in_filter_non_nullable_column(async);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE COALESCE([w].[Id], 0) = 0");
        }

        public override async Task GetValueOrDefault_in_order_by(bool async)
        {
            await base.GetValueOrDefault_in_order_by(async);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
ORDER BY COALESCE([w].[SynergyWithId], 0), [w].[Id]");
        }

        public override async Task GetValueOrDefault_with_argument(bool async)
        {
            await base.GetValueOrDefault_with_argument(async);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE COALESCE([w].[SynergyWithId], [w].[Id]) = 1");
        }

        public override async Task GetValueOrDefault_with_argument_complex(bool async)
        {
            await base.GetValueOrDefault_with_argument_complex(async);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE COALESCE([w].[SynergyWithId], CAST(LEN([w].[Name]) AS int) + 42) > 10");
        }

        public override async Task Filter_with_complex_predicate_containing_subquery(bool async)
        {
            await base.Filter_with_complex_predicate_containing_subquery(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[FullName] <> N'Dom') AND (
    SELECT TOP(1) [w].[Id]
    FROM [Weapons] AS [w]
    WHERE ([g].[FullName] = [w].[OwnerFullName]) AND ([w].[IsAutomatic] = CAST(1 AS bit))
    ORDER BY [w].[Id]) IS NOT NULL");
        }

        public override async Task Query_with_complex_let_containing_ordering_and_filter_projecting_firstOrDefault_element_of_let(
            bool async)
        {
            await base.Query_with_complex_let_containing_ordering_and_filter_projecting_firstOrDefault_element_of_let(async);

            AssertSql(
                @"SELECT [g].[Nickname], (
    SELECT TOP(1) [w].[Name]
    FROM [Weapons] AS [w]
    WHERE ([g].[FullName] = [w].[OwnerFullName]) AND ([w].[IsAutomatic] = CAST(1 AS bit))
    ORDER BY [w].[AmmunitionType] DESC) AS [WeaponName]
FROM [Gears] AS [g]
WHERE [g].[Nickname] <> N'Dom'");
        }

        public override async Task
            Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation(bool async)
        {
            await base.Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation(async);

            AssertSql(
                @"");
        }

        public override async Task
            Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation_complex(bool async)
        {
            await base.Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation_complex(
                async);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
LEFT JOIN [Squads] AS [s] ON [g].[SquadId] = [s].[Id]
WHERE (SUBSTRING([t].[Note], 0 + 1, CAST(LEN([s].[Name]) AS int)) = [t].[GearNickName]) OR (([t].[Note] IS NULL OR [s].[Name] IS NULL) AND [t].[GearNickName] IS NULL)");
        }

        public override async Task Filter_with_new_Guid(bool async)
        {
            await base.Filter_with_new_Guid(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
WHERE [t].[Id] = 'df36f493-463f-4123-83f9-6b135deeb7ba'");
        }

        public override async Task Filter_with_new_Guid_closure(bool async)
        {
            await base.Filter_with_new_Guid_closure(async);

            AssertSql(
                @"@__p_0='df36f493-463f-4123-83f9-6b135deeb7bd'

SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [Tags] AS [t]
WHERE [t].[Id] = @__p_0",
                //
                @"@__p_0='b39a6fba-9026-4d69-828e-fd7068673e57'

SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [Tags] AS [t]
WHERE [t].[Id] = @__p_0");
        }

        public override async Task OfTypeNav1(bool async)
        {
            await base.OfTypeNav1(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
LEFT JOIN [Tags] AS [t0] ON ([g].[Nickname] = [t0].[GearNickName]) AND ([g].[SquadId] = [t0].[GearSquadId])
WHERE ((([t].[Note] <> N'Foo') OR [t].[Note] IS NULL) AND ([g].[Discriminator] = N'Officer')) AND (([t0].[Note] <> N'Bar') OR [t0].[Note] IS NULL)");
        }

        public override async Task OfTypeNav2(bool async)
        {
            await base.OfTypeNav2(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
LEFT JOIN [Cities] AS [c] ON [g].[AssignedCityName] = [c].[Name]
WHERE ((([t].[Note] <> N'Foo') OR [t].[Note] IS NULL) AND ([g].[Discriminator] = N'Officer')) AND (([c].[Location] <> 'Bar') OR [c].[Location] IS NULL)");
        }

        public override async Task OfTypeNav3(bool async)
        {
            await base.OfTypeNav3(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
INNER JOIN [Weapons] AS [w] ON [g].[FullName] = [w].[OwnerFullName]
LEFT JOIN [Tags] AS [t0] ON ([g].[Nickname] = [t0].[GearNickName]) AND ([g].[SquadId] = [t0].[GearSquadId])
WHERE ((([t].[Note] <> N'Foo') OR [t].[Note] IS NULL) AND ([g].[Discriminator] = N'Officer')) AND (([t0].[Note] <> N'Bar') OR [t0].[Note] IS NULL)");
        }

        public override void Nav_rewrite_Distinct_with_convert()
        {
            base.Nav_rewrite_Distinct_with_convert();

            AssertSql(
                @"");
        }

        public override void Nav_rewrite_Distinct_with_convert_anonymous()
        {
            base.Nav_rewrite_Distinct_with_convert_anonymous();

            AssertSql(
                @"");
        }

        public override async Task Nav_rewrite_with_convert1(bool async)
        {
            await base.Nav_rewrite_with_convert1(async);

            AssertSql(
                @"SELECT [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId]
FROM [Factions] AS [f]
LEFT JOIN [Cities] AS [c] ON [f].[CapitalName] = [c].[Name]
LEFT JOIN (
    SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
    FROM [LocustLeaders] AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
WHERE ([c].[Name] <> N'Foo') OR [c].[Name] IS NULL");
        }

        public override async Task Nav_rewrite_with_convert2(bool async)
        {
            await base.Nav_rewrite_with_convert2(async);

            AssertSql(
                @"SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated]
FROM [Factions] AS [f]
LEFT JOIN [Cities] AS [c] ON [f].[CapitalName] = [c].[Name]
LEFT JOIN (
    SELECT [l].[Name]
    FROM [LocustLeaders] AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
WHERE (([c].[Name] <> N'Foo') OR [c].[Name] IS NULL) AND (([t].[Name] <> N'Bar') OR [t].[Name] IS NULL)");
        }

        public override async Task Nav_rewrite_with_convert3(bool async)
        {
            await base.Nav_rewrite_with_convert3(async);

            AssertSql(
                @"SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated]
FROM [Factions] AS [f]
LEFT JOIN [Cities] AS [c] ON [f].[CapitalName] = [c].[Name]
LEFT JOIN (
    SELECT [l].[Name]
    FROM [LocustLeaders] AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
WHERE (([c].[Name] <> N'Foo') OR [c].[Name] IS NULL) AND (([t].[Name] <> N'Bar') OR [t].[Name] IS NULL)");
        }

        public override async Task Where_contains_on_navigation_with_composite_keys(bool async)
        {
            await base.Where_contains_on_navigation_with_composite_keys(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Gear', N'Officer') AND EXISTS (
    SELECT 1
    FROM [Cities] AS [c]
    WHERE EXISTS (
        SELECT 1
        FROM [Gears] AS [g0]
        WHERE ([g0].[Discriminator] IN (N'Gear', N'Officer') AND ([c].[Name] = [g0].[CityOfBirthName])) AND (([g0].[Nickname] = [g].[Nickname]) AND ([g0].[SquadId] = [g].[SquadId]))))");
        }

        public override async Task Include_with_complex_order_by(bool async)
        {
            await base.Include_with_complex_order_by(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Gears] AS [g]
LEFT JOIN [Weapons] AS [w0] ON [g].[FullName] = [w0].[OwnerFullName]
ORDER BY (
    SELECT TOP(1) [w].[Name]
    FROM [Weapons] AS [w]
    WHERE ([g].[FullName] = [w].[OwnerFullName]) AND ([w].[Name] LIKE N'%Gnasher%')), [g].[Nickname], [g].[SquadId], [w0].[Id]");
        }

        public override async Task Anonymous_projection_take_followed_by_projecting_single_element_from_collection_navigation(bool async)
        {
            await base.Anonymous_projection_take_followed_by_projecting_single_element_from_collection_navigation(async);

            AssertSql(
                @"");
        }

        public override async Task Bool_projection_from_subquery_treated_appropriately_in_where(bool async)
        {
            await base.Bool_projection_from_subquery_treated_appropriately_in_where(async);

            AssertSql(
                @"SELECT [c].[Name], [c].[Location], [c].[Nation]
FROM [Cities] AS [c]
WHERE (
    SELECT TOP(1) [g].[HasSoulPatch]
    FROM [Gears] AS [g]
    ORDER BY [g].[Nickname], [g].[SquadId]) = CAST(1 AS bit)");
        }

        public override async Task DateTimeOffset_Contains_Less_than_Greater_than(bool async)
        {
            await base.DateTimeOffset_Contains_Less_than_Greater_than(async);

            AssertSql(
                @"@__start_0='1902-01-01T10:00:00.1234567+01:30'
@__end_1='1902-01-03T10:00:00.1234567+01:30'

SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE ((@__start_0 <= CAST(CONVERT(date, [m].[Timeline]) AS datetimeoffset)) AND ([m].[Timeline] < @__end_1)) AND ([m].[Timeline] = '1902-01-02T10:00:00.1234567+01:30')");
        }

        public override async Task Navigation_inside_interpolated_string_expanded(bool async)
        {
            await base.Navigation_inside_interpolated_string_expanded(async);

            AssertSql(
                @"SELECT CASE
    WHEN [w].[SynergyWithId] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [w0].[OwnerFullName]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]");
        }

        public override async Task Left_join_projection_using_coalesce_tracking(bool async)
        {
            await base.Left_join_projection_using_coalesce_tracking(async);

            AssertSql(
                @"SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Gears] AS [g0] ON [g].[LeaderNickname] = [g0].[Nickname]");
        }

        public override async Task Left_join_projection_using_conditional_tracking(bool async)
        {
            await base.Left_join_projection_using_conditional_tracking(async);

            AssertSql(
                @"SELECT CASE
    WHEN [g0].[Nickname] IS NULL OR [g0].[SquadId] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Gears] AS [g0] ON [g].[LeaderNickname] = [g0].[Nickname]");
        }

        public override async Task Project_collection_navigation_nested_with_take_composite_key(bool async)
        {
            await base.Project_collection_navigation_nested_with_take_composite_key(async);

            AssertSql(
                @"SELECT [t].[Id], [g].[Nickname], [g].[SquadId], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
LEFT JOIN (
    SELECT [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[Discriminator], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], ROW_NUMBER() OVER(PARTITION BY [g0].[LeaderNickname], [g0].[LeaderSquadId] ORDER BY [g0].[Nickname], [g0].[SquadId]) AS [row]
        FROM [Gears] AS [g0]
    ) AS [t1]
    WHERE [t1].[row] <= 50
) AS [t0] ON (([g].[Nickname] = [t0].[LeaderNickname]) OR ([g].[Nickname] IS NULL AND [t0].[LeaderNickname] IS NULL)) AND ([g].[SquadId] = [t0].[LeaderSquadId])
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [t].[Id], [g].[Nickname], [g].[SquadId], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Nickname], [t0].[SquadId]");
        }

        public override async Task Project_collection_navigation_nested_composite_key(bool async)
        {
            await base.Project_collection_navigation_nested_composite_key(async);

            AssertSql(
                @"SELECT [t].[Id], [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
LEFT JOIN [Gears] AS [g0] ON (([g].[Nickname] = [g0].[LeaderNickname]) OR ([g].[Nickname] IS NULL AND [g0].[LeaderNickname] IS NULL)) AND ([g].[SquadId] = [g0].[LeaderSquadId])
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [t].[Id], [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId]");
        }

        public override async Task Null_checks_in_correlated_predicate_are_correctly_translated(bool async)
        {
            await base.Null_checks_in_correlated_predicate_are_correctly_translated(async);

            AssertSql(
                @"SELECT [t].[Id], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON (([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])) AND [t].[Note] IS NOT NULL
ORDER BY [t].[Id], [g].[Nickname], [g].[SquadId]");
        }

        public override async Task SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector(bool async)
        {
            await base.SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector(async);

            AssertSql(
                @"@__isAutomatic_0='True'

SELECT [g].[Nickname], [g].[FullName], CASE
    WHEN [t].[Id] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Collection]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [w].[Id], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
    WHERE [w].[IsAutomatic] = @__isAutomatic_0
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]");
        }

        public override async Task SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector_not_equal(bool async)
        {
            await base.SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector_not_equal(async);

            AssertSql(
                @"@__isAutomatic_0='True'

SELECT [g].[Nickname], [g].[FullName], CASE
    WHEN [t].[Id] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Collection]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [w].[Id], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
    WHERE [w].[IsAutomatic] <> @__isAutomatic_0
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]");
        }

        public override async Task SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector_order_comparison(bool async)
        {
            await base.SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector_order_comparison(async);

            AssertSql(
                @"@__prm_0='1'

SELECT [g].[Nickname], [g].[FullName], CASE
    WHEN [t].[Id] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Collection]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [w].[Id], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
    WHERE [w].[Id] > @__prm_0
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]");
        }

        public override async Task Join_with_inner_being_a_subquery_projecting_single_property(bool async)
        {
            await base.Join_with_inner_being_a_subquery_projecting_single_property(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
INNER JOIN [Gears] AS [g0] ON [g].[Nickname] = [g0].[Nickname]");
        }

        public override async Task Join_with_inner_being_a_subquery_projecting_anonymous_type_with_single_property(bool async)
        {
            await base.Join_with_inner_being_a_subquery_projecting_anonymous_type_with_single_property(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
INNER JOIN [Gears] AS [g0] ON [g].[Nickname] = [g0].[Nickname]");
        }

        public override async Task Navigation_based_on_complex_expression1(bool async)
        {
            await base.Navigation_based_on_complex_expression1(async);

            AssertSql(
                @"");
        }

        public override async Task Navigation_based_on_complex_expression2(bool async)
        {
            await base.Navigation_based_on_complex_expression2(async);

            AssertSql(
                @"SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [l].[Name]
    FROM [LocustLeaders] AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
WHERE [t].[Name] IS NOT NULL");
        }

        public override async Task Navigation_based_on_complex_expression3(bool async)
        {
            await base.Navigation_based_on_complex_expression3(async);

            AssertSql(
                @"SELECT [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
    FROM [LocustLeaders] AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]");
        }

        public override async Task Navigation_based_on_complex_expression4(bool async)
        {
            await base.Navigation_based_on_complex_expression4(async);

            AssertSql(
                @"");
        }

        public override async Task Navigation_based_on_complex_expression5(bool async)
        {
            await base.Navigation_based_on_complex_expression5(async);

            AssertSql(
                @"");
        }

        public override async Task Navigation_based_on_complex_expression6(bool async)
        {
            await base.Navigation_based_on_complex_expression6(async);

            AssertSql(
                @"");
        }

        public override async Task Select_as_operator(bool async)
        {
            await base.Select_as_operator(async);

            AssertSql(
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [LocustLeaders] AS [l]");
        }

        public override async Task Select_datetimeoffset_comparison_in_projection(bool async)
        {
            await base.Select_datetimeoffset_comparison_in_projection(async);

            AssertSql(
                @"SELECT CASE
    WHEN [m].[Timeline] > SYSDATETIMEOFFSET() THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Missions] AS [m]");
        }

        public override async Task OfType_in_subquery_works(bool async)
        {
            await base.OfType_in_subquery_works(async);

            AssertSql(
                @"SELECT [t].[Name], [t].[Location], [t].[Nation]
FROM [Gears] AS [g]
INNER JOIN (
    SELECT [c].[Name], [c].[Location], [c].[Nation], [g0].[LeaderNickname], [g0].[LeaderSquadId]
    FROM [Gears] AS [g0]
    LEFT JOIN [Cities] AS [c] ON [g0].[AssignedCityName] = [c].[Name]
    WHERE [g0].[Discriminator] = N'Officer'
) AS [t] ON ([g].[Nickname] = [t].[LeaderNickname]) AND ([g].[SquadId] = [t].[LeaderSquadId])
WHERE [g].[Discriminator] = N'Officer'");
        }

        public override async Task Nullable_bool_comparison_is_translated_to_server(bool async)
        {
            await base.Nullable_bool_comparison_is_translated_to_server(async);

            AssertSql(
                @"SELECT CASE
    WHEN ([f].[Eradicated] = CAST(1 AS bit)) AND [f].[Eradicated] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsEradicated]
FROM [Factions] AS [f]");
        }

        public override async Task Acessing_reference_navigation_collection_composition_generates_single_query(bool async)
        {
            await base.Acessing_reference_navigation_collection_composition_generates_single_query(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [t].[Id], [t].[IsAutomatic], [t].[Name], [t].[Id0]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [w].[Id], [w].[IsAutomatic], [w0].[Name], [w0].[Id] AS [Id0], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
    LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Id], [t].[Id0]");
        }

        public override async Task Reference_include_chain_loads_correctly_when_middle_is_null(bool async)
        {
            await base.Reference_include_chain_loads_correctly_when_middle_is_null(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
LEFT JOIN [Squads] AS [s] ON [g].[SquadId] = [s].[Id]
ORDER BY [t].[Note]");
        }

        public override async Task Accessing_property_of_optional_navigation_in_child_projection_works(bool async)
        {
            await base.Accessing_property_of_optional_navigation_in_child_projection_works(async);

            AssertSql(
                @"SELECT CASE
    WHEN [g].[Nickname] IS NOT NULL AND [g].[SquadId] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [t].[Id], [g].[Nickname], [g].[SquadId], [t0].[Nickname], [t0].[Id], [t0].[SquadId]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
LEFT JOIN (
    SELECT [g0].[Nickname], [w].[Id], [g0].[SquadId], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
    LEFT JOIN [Gears] AS [g0] ON [w].[OwnerFullName] = [g0].[FullName]
) AS [t0] ON [g].[FullName] = [t0].[OwnerFullName]
ORDER BY [t].[Note], [t].[Id], [g].[Nickname], [g].[SquadId], [t0].[Id], [t0].[Nickname], [t0].[SquadId]");
        }

        public override async Task Collection_navigation_ofType_filter_works(bool async)
        {
            await base.Collection_navigation_ofType_filter_works(async);

            AssertSql(
                @"SELECT [c].[Name], [c].[Location], [c].[Nation]
FROM [Cities] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Gears] AS [g]
    WHERE (([c].[Name] = [g].[CityOfBirthName]) AND ([g].[Discriminator] = N'Officer')) AND ([g].[Nickname] = N'Marcus'))");
        }

        public override async Task Query_reusing_parameter_doesnt_declare_duplicate_parameter(bool async)
        {
            await base.Query_reusing_parameter_doesnt_declare_duplicate_parameter(async);

            AssertSql(
                @"@__prm_Inner_Nickname_0='Marcus' (Size = 450)

SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM (
    SELECT DISTINCT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
    FROM [Gears] AS [g]
    WHERE ([g].[Nickname] <> @__prm_Inner_Nickname_0) AND ([g].[Nickname] <> @__prm_Inner_Nickname_0)
) AS [t]
ORDER BY [t].[FullName]");
        }

        public override async Task Query_reusing_parameter_with_inner_query_doesnt_declare_duplicate_parameter(bool async)
        {
            await base.Query_reusing_parameter_with_inner_query_doesnt_declare_duplicate_parameter(async);

            AssertSql(
                @"@__squadId_0='1'

SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
    FROM [Gears] AS [g]
    INNER JOIN [Squads] AS [s] ON [g].[SquadId] = [s].[Id]
    WHERE EXISTS (
        SELECT 1
        FROM [Squads] AS [s0]
        WHERE ([s0].[Id] = @__squadId_0) AND ([s0].[Id] = [s].[Id]))
    UNION ALL
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
    FROM [Gears] AS [g0]
    INNER JOIN [Squads] AS [s1] ON [g0].[SquadId] = [s1].[Id]
    WHERE EXISTS (
        SELECT 1
        FROM [Squads] AS [s2]
        WHERE ([s2].[Id] = @__squadId_0) AND ([s2].[Id] = [s1].[Id]))
) AS [t]
ORDER BY [t].[FullName]");
        }

        public override async Task Query_reusing_parameter_with_inner_query_expression_doesnt_declare_duplicate_parameter(bool async)
        {
            await base.Query_reusing_parameter_with_inner_query_expression_doesnt_declare_duplicate_parameter(async);

            AssertSql(
                @"@__gearId_0='1'

SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE EXISTS (
    SELECT 1
    FROM [Gears] AS [g]
    WHERE (([s].[Id] = [g].[SquadId]) AND ([g].[SquadId] = @__gearId_0)) AND ([g].[SquadId] = @__gearId_0))");
        }

        public override async Task Query_reusing_parameter_doesnt_declare_duplicate_parameter_complex(bool async)
        {
            await base.Query_reusing_parameter_doesnt_declare_duplicate_parameter_complex(async);

            AssertSql(
                @"@__entity_equality_prm_Inner_Squad_0_Id='1' (Nullable = true)

SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM (
    SELECT DISTINCT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
    FROM [Gears] AS [g]
    INNER JOIN [Squads] AS [s] ON [g].[SquadId] = [s].[Id]
    WHERE [s].[Id] = @__entity_equality_prm_Inner_Squad_0_Id
) AS [t]
INNER JOIN [Squads] AS [s0] ON [t].[SquadId] = [s0].[Id]
WHERE [s0].[Id] = @__entity_equality_prm_Inner_Squad_0_Id
ORDER BY [t].[FullName]");
        }

        public override async Task Complex_GroupBy_after_set_operator(bool async)
        {
            await base.Complex_GroupBy_after_set_operator(async);

            AssertSql(
                @"SELECT [t].[Name], [t].[Count], COALESCE(SUM([t].[Count]), 0) AS [Sum]
FROM (
    SELECT [c].[Name], (
        SELECT COUNT(*)
        FROM [Weapons] AS [w0]
        WHERE [g].[FullName] = [w0].[OwnerFullName]) AS [Count]
    FROM [Gears] AS [g]
    LEFT JOIN [Cities] AS [c] ON [g].[AssignedCityName] = [c].[Name]
    UNION ALL
    SELECT [c0].[Name], (
        SELECT COUNT(*)
        FROM [Weapons] AS [w]
        WHERE [g0].[FullName] = [w].[OwnerFullName]) AS [Count]
    FROM [Gears] AS [g0]
    INNER JOIN [Cities] AS [c0] ON [g0].[CityOfBirthName] = [c0].[Name]
) AS [t]
GROUP BY [t].[Name], [t].[Count]");
        }

        public override async Task Complex_GroupBy_after_set_operator_using_result_selector(bool async)
        {
            await base.Complex_GroupBy_after_set_operator_using_result_selector(async);

            AssertSql(
                @"SELECT [t].[Name], [t].[Count], COALESCE(SUM([t].[Count]), 0) AS [Sum]
FROM (
    SELECT [c].[Name], (
        SELECT COUNT(*)
        FROM [Weapons] AS [w0]
        WHERE [g].[FullName] = [w0].[OwnerFullName]) AS [Count]
    FROM [Gears] AS [g]
    LEFT JOIN [Cities] AS [c] ON [g].[AssignedCityName] = [c].[Name]
    UNION ALL
    SELECT [c0].[Name], (
        SELECT COUNT(*)
        FROM [Weapons] AS [w]
        WHERE [g0].[FullName] = [w].[OwnerFullName]) AS [Count]
    FROM [Gears] AS [g0]
    INNER JOIN [Cities] AS [c0] ON [g0].[CityOfBirthName] = [c0].[Name]
) AS [t]
GROUP BY [t].[Name], [t].[Count]");
        }

        public override async Task Left_join_with_GroupBy_with_composite_group_key(bool async)
        {
            await base.Left_join_with_GroupBy_with_composite_group_key(async);

            AssertSql(
                @"SELECT [g].[CityOfBirthName], [g].[HasSoulPatch]
FROM [Gears] AS [g]
INNER JOIN [Squads] AS [s] ON [g].[SquadId] = [s].[Id]
LEFT JOIN [Tags] AS [t] ON [g].[Nickname] = [t].[GearNickName]
GROUP BY [g].[CityOfBirthName], [g].[HasSoulPatch]");
        }

        public override async Task GroupBy_with_boolean_grouping_key(bool async)
        {
            await base.GroupBy_with_boolean_grouping_key(async);

            AssertSql(
                @"SELECT [g].[CityOfBirthName], [g].[HasSoulPatch], CASE
    WHEN [g].[Nickname] = N'Marcus' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsMarcus], COUNT(*) AS [Count]
FROM [Gears] AS [g]
GROUP BY [g].[CityOfBirthName], [g].[HasSoulPatch], CASE
    WHEN [g].[Nickname] = N'Marcus' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task GroupBy_with_boolean_groupin_key_thru_navigation_access(bool async)
        {
            await base.GroupBy_with_boolean_groupin_key_thru_navigation_access(async);

            AssertSql(
                @"SELECT [g].[HasSoulPatch], LOWER([s].[Name]) AS [Name]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
LEFT JOIN [Squads] AS [s] ON [g].[SquadId] = [s].[Id]
GROUP BY [g].[HasSoulPatch], [s].[Name]");
        }

        public override async Task Group_by_over_projection_with_multiple_properties_accessed_thru_navigation(bool async)
        {
            await base.Group_by_over_projection_with_multiple_properties_accessed_thru_navigation(async);

            AssertSql(
                @"SELECT [c].[Name]
FROM [Gears] AS [g]
INNER JOIN [Cities] AS [c] ON [g].[CityOfBirthName] = [c].[Name]
LEFT JOIN [Cities] AS [c0] ON [g].[AssignedCityName] = [c0].[Name]
INNER JOIN [Squads] AS [s] ON [g].[SquadId] = [s].[Id]
GROUP BY [c].[Name]");
        }

        public override async Task Group_by_on_StartsWith_with_null_parameter_as_argument(bool async)
        {
            await base.Group_by_on_StartsWith_with_null_parameter_as_argument(async);

            AssertSql(
                @"SELECT CAST(0 AS bit)
FROM [Gears] AS [g]");
        }

        public override async Task Group_by_with_having_StartsWith_with_null_parameter_as_argument(bool async)
        {
            await base.Group_by_with_having_StartsWith_with_null_parameter_as_argument(async);

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
GROUP BY [g].[FullName]
HAVING 0 = 1");
        }

        public override async Task Select_StartsWith_with_null_parameter_as_argument(bool async)
        {
            await base.Select_StartsWith_with_null_parameter_as_argument(async);

            AssertSql(
                @"SELECT CAST(0 AS bit)
FROM [Gears] AS [g]");
        }

        public override async Task Select_null_parameter_is_not_null(bool async)
        {
            await base.Select_null_parameter_is_not_null(async);

            AssertSql(
                @"@__p_0='False'

SELECT @__p_0
FROM [Gears] AS [g]");
        }

        public override async Task Where_null_parameter_is_not_null(bool async)
        {
            await base.Where_null_parameter_is_not_null(async);

            AssertSql(
                @"@__p_0='False'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE @__p_0 = CAST(1 AS bit)");
        }

        public override async Task OrderBy_StartsWith_with_null_parameter_as_argument(bool async)
        {
            await base.OrderBy_StartsWith_with_null_parameter_as_argument(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
ORDER BY [g].[Nickname]");
        }

        public override async Task OrderBy_Contains_empty_list(bool async)
        {
            await base.OrderBy_Contains_empty_list(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]");
        }

        public override async Task Where_with_enum_flags_parameter(bool async)
        {
            await base.Where_with_enum_flags_parameter(async);

            AssertSql(
                @"@__rank_0='1' (Nullable = true)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[Rank] & @__rank_0) = @__rank_0",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]",
                //
                @"@__rank_0='2' (Nullable = true)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[Rank] | @__rank_0) <> @__rank_0",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE 0 = 1");
        }

        public override async Task FirstOrDefault_navigation_access_entity_equality_in_where_predicate_apply_peneding_selector(bool async)
        {
            await base.FirstOrDefault_navigation_access_entity_equality_in_where_predicate_apply_peneding_selector(async);

            AssertSql(
                @"SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated]
FROM [Factions] AS [f]
LEFT JOIN [Cities] AS [c] ON [f].[CapitalName] = [c].[Name]
WHERE ([c].[Name] = (
    SELECT TOP(1) [c0].[Name]
    FROM [Gears] AS [g]
    INNER JOIN [Cities] AS [c0] ON [g].[CityOfBirthName] = [c0].[Name]
    ORDER BY [g].[Nickname])) OR ([c].[Name] IS NULL AND (
    SELECT TOP(1) [c0].[Name]
    FROM [Gears] AS [g]
    INNER JOIN [Cities] AS [c0] ON [g].[CityOfBirthName] = [c0].[Name]
    ORDER BY [g].[Nickname]) IS NULL)");
        }

        public override async Task Bitwise_operation_with_non_null_parameter_optimizes_null_checks(bool async)
        {
            await base.Bitwise_operation_with_non_null_parameter_optimizes_null_checks(async);

            AssertSql(
                @"@__ranks_0='134'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE ([g].[Rank] & @__ranks_0) <> 0",
                //
                @"@__ranks_0='134'

SELECT CASE
    WHEN ([g].[Rank] | @__ranks_0) = @__ranks_0 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Gears] AS [g]",
                //
                @"@__ranks_0='134'

SELECT CASE
    WHEN ([g].[Rank] | ([g].[Rank] | (@__ranks_0 | ([g].[Rank] | @__ranks_0)))) = @__ranks_0 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Gears] AS [g]");
        }

        public override async Task Bitwise_operation_with_null_arguments(bool async)
        {
            await base.Bitwise_operation_with_null_arguments(async);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] IS NULL",
                //
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] IS NULL",
                //
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] IS NULL",
                //
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]",
                //
                @"@__prm_0='2' (Nullable = true)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE (([w].[AmmunitionType] & @__prm_0) <> 0) OR [w].[AmmunitionType] IS NULL",
                //
                @"@__prm_0='1' (Nullable = true)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & @__prm_0) = @__prm_0");
        }

        public override async Task Logical_operation_with_non_null_parameter_optimizes_null_checks(bool async)
        {
            await base.Logical_operation_with_non_null_parameter_optimizes_null_checks(async);

            AssertSql(
                @"@__prm_0='True'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[HasSoulPatch] <> @__prm_0",
                //
                @"@__prm_0='False'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[HasSoulPatch] <> @__prm_0");
        }

        public override async Task Cast_OfType_works_correctly(bool async)
        {
            await base.Cast_OfType_works_correctly(async);

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] = N'Officer'");
        }

        public override async Task Join_inner_source_custom_projection_followed_by_filter(bool async)
        {
            await base.Join_inner_source_custom_projection_followed_by_filter(async);

            AssertSql(
                @"SELECT CASE
    WHEN [f].[Name] = N'Locust' THEN CAST(1 AS bit)
    ELSE NULL
END AS [IsEradicated], [f].[CommanderName], [f].[Name]
FROM [LocustLeaders] AS [l]
INNER JOIN [Factions] AS [f] ON [l].[Name] = [f].[CommanderName]
WHERE (CASE
    WHEN [f].[Name] = N'Locust' THEN CAST(1 AS bit)
    ELSE NULL
END <> CAST(1 AS bit)) OR CASE
    WHEN [f].[Name] = N'Locust' THEN CAST(1 AS bit)
    ELSE NULL
END IS NULL");
        }

        public override async Task Byte_array_contains_literal(bool async)
        {
            await base.Byte_array_contains_literal(async);

            AssertSql(
                @"SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE CHARINDEX(0x01, [s].[Banner]) > 0");
        }

        public override async Task Byte_array_filter_by_length_literal(bool async)
        {
            await base.Byte_array_filter_by_length_literal(async);

            AssertSql(
                @"SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE CAST(DATALENGTH([s].[Banner]) AS int) = 1");
        }

        public override async Task Byte_array_filter_by_length_parameter(bool async)
        {
            await base.Byte_array_filter_by_length_parameter(async);

            AssertSql(
                @"@__p_0='1'

SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE CAST(DATALENGTH([s].[Banner]) AS int) = @__p_0");
        }

        public override void Byte_array_filter_by_length_parameter_compiled()
        {
            base.Byte_array_filter_by_length_parameter_compiled();

            AssertSql(
                @"@__byteArrayParam='0x2A80' (Size = 8000)

SELECT COUNT(*)
FROM [Squads] AS [s]
WHERE CAST(DATALENGTH([s].[Banner]) AS int) = CAST(DATALENGTH(@__byteArrayParam) AS int)");
        }

        public override async Task Byte_array_contains_parameter(bool async)
        {
            await base.Byte_array_contains_parameter(async);

            AssertSql(
                @"@__someByte_0='1' (Size = 1)

SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE CHARINDEX(CAST(@__someByte_0 AS varbinary(max)), [s].[Banner]) > 0");
        }

        public override async Task Byte_array_filter_by_length_literal_does_not_cast_on_varbinary_n(bool async)
        {
            await base.Byte_array_filter_by_length_literal_does_not_cast_on_varbinary_n(async);

            AssertSql(
                @"SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE DATALENGTH([s].[Banner5]) = 5");
        }

        public override async Task Conditional_expression_with_test_being_simplified_to_constant_simple(bool isAsync)
        {
            await base.Conditional_expression_with_test_being_simplified_to_constant_simple(isAsync);

            AssertSql(
                @"@__prm_0='True'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE CASE
    WHEN [g].[HasSoulPatch] = @__prm_0 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CAST(1 AS bit)");
        }

        public override async Task Conditional_expression_with_test_being_simplified_to_constant_complex(bool isAsync)
        {
            await base.Conditional_expression_with_test_being_simplified_to_constant_complex(isAsync);

            AssertSql(
                @"@__prm_0='True'
@__prm2_1='Dom's Lancer' (Size = 4000)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE CASE
    WHEN [g].[HasSoulPatch] = @__prm_0 THEN CASE
        WHEN ((
            SELECT TOP(1) [w].[Name]
            FROM [Weapons] AS [w]
            WHERE [w].[Id] = [g].[SquadId]) = @__prm2_1) AND (
            SELECT TOP(1) [w].[Name]
            FROM [Weapons] AS [w]
            WHERE [w].[Id] = [g].[SquadId]) IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    ELSE CAST(0 AS bit)
END = CAST(1 AS bit)");
        }

        public override async Task OrderBy_bool_coming_from_optional_navigation(bool async)
        {
            await base.OrderBy_bool_coming_from_optional_navigation(async);

            AssertSql(
                @"SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY [w0].[IsAutomatic]");
        }

        public override async Task DateTimeOffset_Date_returns_datetime(bool async)
        {
            await base.DateTimeOffset_Date_returns_datetime(async);

            AssertSql(
                @"@__dateTimeOffset_Date_0='0002-03-01T00:00:00.0000000'

SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE CONVERT(date, [m].[Timeline]) >= @__dateTimeOffset_Date_0");
        }

        public override async Task Conditional_with_conditions_evaluating_to_false_gets_optimized(bool async)
        {
            await base.Conditional_with_conditions_evaluating_to_false_gets_optimized(async);

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]");
        }

        public override async Task Conditional_with_conditions_evaluating_to_true_gets_optimized(bool async)
        {
            await base.Conditional_with_conditions_evaluating_to_true_gets_optimized(async);

            AssertSql(
                @"SELECT [g].[CityOfBirthName]
FROM [Gears] AS [g]");
        }

        public override async Task Projecting_required_string_column_compared_to_null_parameter(bool async)
        {
            await base.Projecting_required_string_column_compared_to_null_parameter(async);

            AssertSql(
                @"SELECT CAST(0 AS bit)
FROM [Gears] AS [g]");
        }

        public override async Task Byte_array_filter_by_SequenceEqual(bool isAsync)
        {
            await base.Byte_array_filter_by_SequenceEqual(isAsync);

            AssertSql(
                @"@__byteArrayParam_0='0x0405060708' (Size = 5)

SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE [s].[Banner5] = @__byteArrayParam_0");
        }

        public override async Task Group_by_nullable_property_HasValue_and_project_the_grouping_key(bool async)
        {
            await base.Group_by_nullable_property_HasValue_and_project_the_grouping_key(async);

            AssertSql(
                @"SELECT CASE
    WHEN [w].[SynergyWithId] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Weapons] AS [w]
GROUP BY CASE
    WHEN [w].[SynergyWithId] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Group_by_nullable_property_and_project_the_grouping_key_HasValue(bool async)
        {
            await base.Group_by_nullable_property_and_project_the_grouping_key_HasValue(async);

            AssertSql(
                @"SELECT CASE
    WHEN [w].[SynergyWithId] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Weapons] AS [w]
GROUP BY [w].[SynergyWithId]");
        }

        public override async Task Checked_context_with_cast_does_not_fail(bool isAsync)
        {
            await base.Checked_context_with_cast_does_not_fail(isAsync);

            AssertSql(
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [LocustLeaders] AS [l]
WHERE CAST([l].[ThreatLevel] AS tinyint) >= CAST(5 AS tinyint)");
        }

        public override async Task Checked_context_with_addition_does_not_fail(bool isAsync)
        {
            await base.Checked_context_with_addition_does_not_fail(isAsync);

            AssertSql(
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [LocustLeaders] AS [l]
WHERE CAST([l].[ThreatLevel] AS bigint) >= (CAST(5 AS bigint) + CAST([l].[ThreatLevel] AS bigint))");
        }

        public override async Task TimeSpan_Hours(bool async)
        {
            await base.TimeSpan_Hours(async);

            AssertSql(
                @"SELECT DATEPART(hour, [m].[Duration])
FROM [Missions] AS [m]");
        }

        public override async Task TimeSpan_Minutes(bool async)
        {
            await base.TimeSpan_Minutes(async);

            AssertSql(
                @"SELECT DATEPART(minute, [m].[Duration])
FROM [Missions] AS [m]");
        }

        public override async Task TimeSpan_Seconds(bool async)
        {
            await base.TimeSpan_Seconds(async);

            AssertSql(
                @"SELECT DATEPART(second, [m].[Duration])
FROM [Missions] AS [m]");
        }

        public override async Task TimeSpan_Milliseconds(bool async)
        {
            await base.TimeSpan_Milliseconds(async);

            AssertSql(
                @"SELECT DATEPART(millisecond, [m].[Duration])
FROM [Missions] AS [m]");
        }

        public override async Task Where_TimeSpan_Hours(bool async)
        {
            await base.Where_TimeSpan_Hours(async);

            AssertSql(
                @"SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(hour, [m].[Duration]) = 1");
        }

        public override async Task Where_TimeSpan_Minutes(bool async)
        {
            await base.Where_TimeSpan_Minutes(async);

            AssertSql(
                @"SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(minute, [m].[Duration]) = 1");
        }

        public override async Task Where_TimeSpan_Seconds(bool async)
        {
            await base.Where_TimeSpan_Seconds(async);

            AssertSql(
                @"SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(second, [m].[Duration]) = 1");
        }

        public override async Task Where_TimeSpan_Milliseconds(bool async)
        {
            await base.Where_TimeSpan_Milliseconds(async);

            AssertSql(
                @"SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(millisecond, [m].[Duration]) = 1");
        }

        public override async Task Contains_on_collection_of_byte_subquery(bool async)
        {
            await base.Contains_on_collection_of_byte_subquery(async);

            AssertSql(
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [LocustLeaders] AS [l]
WHERE EXISTS (
    SELECT 1
    FROM [LocustLeaders] AS [l0]
    WHERE [l0].[ThreatLevelByte] = [l].[ThreatLevelByte])");
        }

        public override async Task Contains_on_collection_of_nullable_byte_subquery(bool async)
        {
            await base.Contains_on_collection_of_nullable_byte_subquery(async);

            AssertSql(
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [LocustLeaders] AS [l]
WHERE EXISTS (
    SELECT 1
    FROM [LocustLeaders] AS [l0]
    WHERE ([l0].[ThreatLevelNullableByte] = [l].[ThreatLevelNullableByte]) OR ([l0].[ThreatLevelNullableByte] IS NULL AND [l].[ThreatLevelNullableByte] IS NULL))");
        }

        public override async Task Contains_on_collection_of_nullable_byte_subquery_null_constant(bool async)
        {
            await base.Contains_on_collection_of_nullable_byte_subquery_null_constant(async);

            AssertSql(
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [LocustLeaders] AS [l]
WHERE EXISTS (
    SELECT 1
    FROM [LocustLeaders] AS [l0]
    WHERE [l0].[ThreatLevelNullableByte] IS NULL)");
        }

        public override async Task Contains_on_collection_of_nullable_byte_subquery_null_parameter(bool async)
        {
            await base.Contains_on_collection_of_nullable_byte_subquery_null_parameter(async);

            AssertSql(
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [LocustLeaders] AS [l]
WHERE EXISTS (
    SELECT 1
    FROM [LocustLeaders] AS [l0]
    WHERE [l0].[ThreatLevelNullableByte] IS NULL)");
        }

        public override async Task Contains_on_byte_array_property_using_byte_column(bool async)
        {
            await base.Contains_on_byte_array_property_using_byte_column(async);

            AssertSql(
                @"SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [Squads] AS [s]
CROSS JOIN [LocustLeaders] AS [l]
WHERE CHARINDEX(CAST([l].[ThreatLevelByte] AS varbinary(max)), [s].[Banner]) > 0");
        }

        public override async Task Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion(
            bool async)
        {
            await base.Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion(async);

            AssertSql(
                @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [LocustLeaders] AS [l]
CROSS APPLY (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
    FROM [Gears] AS [g]
    WHERE EXISTS (
        SELECT 1
        FROM [LocustLeaders] AS [l0]
        WHERE [l0].[ThreatLevelByte] = [l].[ThreatLevelByte])
) AS [t]");
        }

        public override async Task Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion_negated(
            bool async)
        {
            await base.Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion_negated(async);

            AssertSql(
                @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [LocustLeaders] AS [l]
CROSS APPLY (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
    FROM [Gears] AS [g]
    WHERE NOT (EXISTS (
        SELECT 1
        FROM [LocustLeaders] AS [l0]
        WHERE [l0].[ThreatLevelByte] = [l].[ThreatLevelByte]))
) AS [t]");
        }

        public override async Task Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion(bool async)
        {
            await base.Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion(async);

            AssertSql(
                @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [LocustLeaders] AS [l]
CROSS APPLY (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
    FROM [Gears] AS [g]
    WHERE EXISTS (
        SELECT 1
        FROM [LocustLeaders] AS [l0]
        WHERE ([l0].[ThreatLevelNullableByte] = [l].[ThreatLevelNullableByte]) OR ([l0].[ThreatLevelNullableByte] IS NULL AND [l].[ThreatLevelNullableByte] IS NULL))
) AS [t]");
        }

        public override async Task Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion_negated(bool async)
        {
            await base.Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion_negated(async);

            AssertSql(
                @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [LocustLeaders] AS [l]
CROSS APPLY (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
    FROM [Gears] AS [g]
    WHERE NOT (EXISTS (
        SELECT 1
        FROM [LocustLeaders] AS [l0]
        WHERE ([l0].[ThreatLevelNullableByte] = [l].[ThreatLevelNullableByte]) OR ([l0].[ThreatLevelNullableByte] IS NULL AND [l].[ThreatLevelNullableByte] IS NULL)))
) AS [t]");
        }

        public override async Task Enum_closure_typed_as_underlying_type_generates_correct_parameter_type(bool async)
        {
            await base.Enum_closure_typed_as_underlying_type_generates_correct_parameter_type(async);

            AssertSql(
                @"@__prm_0='1' (Nullable = true)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE @__prm_0 = [w].[AmmunitionType]");
        }

        public override async Task Enum_flags_closure_typed_as_underlying_type_generates_correct_parameter_type(bool async)
        {
            await base.Enum_flags_closure_typed_as_underlying_type_generates_correct_parameter_type(async);

            AssertSql(
                @"@__prm_0='133'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE (@__prm_0 & [g].[Rank]) = [g].[Rank]");
        }

        public override async Task Enum_flags_closure_typed_as_different_type_generates_correct_parameter_type(bool async)
        {
            await base.Enum_flags_closure_typed_as_different_type_generates_correct_parameter_type(async);

            AssertSql(
                @"@__prm_0='5'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE (@__prm_0 & CAST([g].[Rank] AS int)) = CAST([g].[Rank] AS int)");
        }

        public override async Task Constant_enum_with_same_underlying_value_as_previously_parameterized_int(bool async)
        {
            await base.Constant_enum_with_same_underlying_value_as_previously_parameterized_int(async);

            AssertSql(
                @"@__p_0='1'

SELECT TOP(@__p_0) [g].[Rank] & 1
FROM [Gears] AS [g]
ORDER BY [g].[Nickname]");
        }

        public override async Task Enum_array_contains(bool async)
        {
            await base.Enum_array_contains(async);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
WHERE [w0].[Id] IS NOT NULL AND (([w0].[AmmunitionType] = 1) OR [w0].[AmmunitionType] IS NULL)");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task DataLength_function_for_string_parameter(bool async)
        {
            await AssertQueryScalar(
                async,
                ss => ss.Set<Mission>().Select(m => EF.Functions.DataLength(m.CodeName)),
                ss => ss.Set<Mission>().Select(m => (int?)(m.CodeName.Length * 2)));

            AssertSql(
                @"SELECT CAST(DATALENGTH([m].[CodeName]) AS int)
FROM [Missions] AS [m]");
        }

        public override async Task CompareTo_used_with_non_unicode_string_column_and_constant(bool async)
        {
            await base.CompareTo_used_with_non_unicode_string_column_and_constant(async);

            AssertSql(
                @"SELECT [c].[Name], [c].[Location], [c].[Nation]
FROM [Cities] AS [c]
WHERE [c].[Location] = 'Unknown'");
        }

        public override async Task Coalesce_used_with_non_unicode_string_column_and_constant(bool async)
        {
            await base.Coalesce_used_with_non_unicode_string_column_and_constant(async);

            AssertSql(
                @"SELECT COALESCE([c].[Location], 'Unknown')
FROM [Cities] AS [c]");
        }

        public override async Task Groupby_anonymous_type_with_navigations_followed_up_by_anonymous_projection_and_orderby(bool async)
        {
            await base.Groupby_anonymous_type_with_navigations_followed_up_by_anonymous_projection_and_orderby(async);

            AssertSql(
                @"SELECT [c].[Name], [c].[Location], COUNT(*) AS [Count]
FROM [Weapons] AS [w]
LEFT JOIN [Gears] AS [g] ON [w].[OwnerFullName] = [g].[FullName]
LEFT JOIN [Cities] AS [c] ON [g].[CityOfBirthName] = [c].[Name]
GROUP BY [c].[Name], [c].[Location]
ORDER BY [c].[Location]");
        }

        public override async Task SelectMany_predicate_with_non_equality_comparison_converted_to_inner_join(bool async)
        {
            await base.SelectMany_predicate_with_non_equality_comparison_converted_to_inner_join(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Gears] AS [g]
INNER JOIN [Weapons] AS [w] ON ([g].[FullName] <> [w].[OwnerFullName]) OR [w].[OwnerFullName] IS NULL
ORDER BY [g].[Nickname], [w].[Id]");
        }

        public override async Task SelectMany_predicate_with_non_equality_comparison_DefaultIfEmpty_converted_to_left_join(bool async)
        {
            await base.SelectMany_predicate_with_non_equality_comparison_DefaultIfEmpty_converted_to_left_join(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Gears] AS [g]
LEFT JOIN [Weapons] AS [w] ON ([g].[FullName] <> [w].[OwnerFullName]) OR [w].[OwnerFullName] IS NULL
ORDER BY [g].[Nickname], [w].[Id]");
        }

        public override async Task SelectMany_predicate_after_navigation_with_non_equality_comparison_DefaultIfEmpty_converted_to_left_join(
            bool async)
        {
            await base.SelectMany_predicate_after_navigation_with_non_equality_comparison_DefaultIfEmpty_converted_to_left_join(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[SynergyWithId]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
    FROM [Weapons] AS [w]
    LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
) AS [t] ON ([g].[FullName] <> [t].[OwnerFullName]) OR [t].[OwnerFullName] IS NULL
ORDER BY [g].[Nickname], [t].[Id]");
        }

        public override async Task SelectMany_without_result_selector_and_non_equality_comparison_converted_to_join(bool async)
        {
            await base.SelectMany_without_result_selector_and_non_equality_comparison_converted_to_join(async);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Gears] AS [g]
LEFT JOIN [Weapons] AS [w] ON ([g].[FullName] <> [w].[OwnerFullName]) OR [w].[OwnerFullName] IS NULL");
        }

        public override async Task Filtered_collection_projection_with_order_comparison_predicate_converted_to_join(bool async)
        {
            await base.Filtered_collection_projection_with_order_comparison_predicate_converted_to_join(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Gears] AS [g]
LEFT JOIN [Weapons] AS [w] ON ([g].[FullName] = [w].[OwnerFullName]) AND ([g].[SquadId] < [w].[Id])
ORDER BY [g].[Nickname], [g].[SquadId], [w].[Id]");
        }

        public override async Task Filtered_collection_projection_with_order_comparison_predicate_converted_to_join2(bool async)
        {
            await base.Filtered_collection_projection_with_order_comparison_predicate_converted_to_join2(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Gears] AS [g]
LEFT JOIN [Weapons] AS [w] ON ([g].[FullName] = [w].[OwnerFullName]) AND ([g].[SquadId] <= [w].[Id])
ORDER BY [g].[Nickname], [g].[SquadId], [w].[Id]");
        }

        public override async Task Filtered_collection_projection_with_order_comparison_predicate_converted_to_join3(bool async)
        {
            await base.Filtered_collection_projection_with_order_comparison_predicate_converted_to_join3(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Gears] AS [g]
LEFT JOIN [Weapons] AS [w] ON ([g].[FullName] = [w].[OwnerFullName]) AND ([g].[SquadId] >= [w].[Id])
ORDER BY [g].[Nickname], [g].[SquadId], [w].[Id]");
        }

        public override async Task SelectMany_predicate_with_non_equality_comparison_with_Take_doesnt_convert_to_join(bool async)
        {
            await base.SelectMany_predicate_with_non_equality_comparison_with_Take_doesnt_convert_to_join(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[SynergyWithId]
FROM [Gears] AS [g]
CROSS APPLY (
    SELECT TOP(3) [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
    FROM [Weapons] AS [w]
    WHERE ([w].[OwnerFullName] <> [g].[FullName]) OR [w].[OwnerFullName] IS NULL
    ORDER BY [w].[Id]
) AS [t]
ORDER BY [g].[Nickname], [t].[Id]");
        }

        public override async Task FirstOrDefault_over_int_compared_to_zero(bool async)
        {
            await base.FirstOrDefault_over_int_compared_to_zero(async);

            AssertSql(
                @"SELECT [s].[Name]
FROM [Squads] AS [s]
WHERE ([s].[Name] = N'Kilo') AND (COALESCE((
    SELECT TOP(1) [g].[SquadId]
    FROM [Gears] AS [g]
    WHERE ([s].[Id] = [g].[SquadId]) AND ([g].[HasSoulPatch] = CAST(1 AS bit))), 0) <> 0)");
        }

        public override async Task Correlated_collection_with_inner_collection_references_element_two_levels_up(bool async)
        {
            await base.Correlated_collection_with_inner_collection_references_element_two_levels_up(async);

            AssertSql(
                @"SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t].[ReportName], [t].[OfficerName], [t].[Nickname], [t].[SquadId]
FROM [Gears] AS [g]
OUTER APPLY (
    SELECT [g0].[FullName] AS [ReportName], [g].[FullName] AS [OfficerName], [g0].[Nickname], [g0].[SquadId]
    FROM [Gears] AS [g0]
    WHERE ([g].[Nickname] = [g0].[LeaderNickname]) AND ([g].[SquadId] = [g0].[LeaderSquadId])
) AS [t]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Nickname], [t].[SquadId]");
        }

        public override async Task Accessing_derived_property_using_hard_and_soft_cast(bool async)
        {
            await base.Accessing_derived_property_using_hard_and_soft_cast(async);

            AssertSql(
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [LocustLeaders] AS [l]
WHERE ([l].[Discriminator] = N'LocustCommander') AND (([l].[HighCommandId] <> 0) OR [l].[HighCommandId] IS NULL)",
                //
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [LocustLeaders] AS [l]
WHERE ([l].[Discriminator] = N'LocustCommander') AND (([l].[HighCommandId] <> 0) OR [l].[HighCommandId] IS NULL)");
        }

        public override async Task Cast_to_derived_followed_by_include_and_FirstOrDefault(bool async)
        {
            await base.Cast_to_derived_followed_by_include_and_FirstOrDefault(async);

            AssertSql(
                @"SELECT TOP(1) [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [LocustLeaders] AS [l]
LEFT JOIN [Gears] AS [g] ON ([l].[DefeatedByNickname] = [g].[Nickname]) AND ([l].[DefeatedBySquadId] = [g].[SquadId])
WHERE [l].[Name] LIKE N'%Queen%'");
        }

        public override async Task Correlated_collection_take(bool async)
        {
            await base.Correlated_collection_take(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [c].[Name], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[SynergyWithId], [c].[Location], [c].[Nation]
FROM [Gears] AS [g]
INNER JOIN [Cities] AS [c] ON [g].[CityOfBirthName] = [c].[Name]
LEFT JOIN (
    SELECT [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[SynergyWithId]
    FROM (
        SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], ROW_NUMBER() OVER(PARTITION BY [w].[OwnerFullName] ORDER BY [w].[Id]) AS [row]
        FROM [Weapons] AS [w]
    ) AS [t]
    WHERE [t].[row] <= 10
) AS [t0] ON [g].[FullName] = [t0].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [c].[Name], [t0].[OwnerFullName], [t0].[Id]");
        }

        public override async Task FirstOrDefault_on_empty_collection_of_DateTime_in_subquery(bool async)
        {
            await base.FirstOrDefault_on_empty_collection_of_DateTime_in_subquery(async);

            AssertSql(
                @"SELECT [g].[Nickname], COALESCE((
    SELECT TOP(1) [t1].[IssueDate]
    FROM [Tags] AS [t1]
    WHERE [t1].[GearNickName] = [g].[FullName]
    ORDER BY [t1].[Id]), '0001-01-01T00:00:00.0000000') AS [invalidTagIssueDate]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
WHERE [t].[IssueDate] > COALESCE((
    SELECT TOP(1) [t0].[IssueDate]
    FROM [Tags] AS [t0]
    WHERE [t0].[GearNickName] = [g].[FullName]
    ORDER BY [t0].[Id]), '0001-01-01T00:00:00.0000000')");
        }

        public override async Task First_on_byte_array(bool async)
        {
            await base.First_on_byte_array(async);

            AssertSql(
                @"SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE CAST(SUBSTRING([s].[Banner], 1, 1) AS tinyint) = CAST(2 AS tinyint)");
        }

        public override async Task Array_access_on_byte_array(bool async)
        {
            await base.Array_access_on_byte_array(async);

            AssertSql(
                @"SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE CAST(SUBSTRING([s].[Banner5], 2 + 1, 1) AS tinyint) = CAST(6 AS tinyint)");
        }

        public override async Task Project_shadow_properties(bool async)
        {
            await base.Project_shadow_properties(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[AssignedCityName]
FROM [Gears] AS [g]");
        }

        public override async Task Project_discriminator_columns(bool async)
        {
            await base.Project_discriminator_columns(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[Discriminator]
FROM [Gears] AS [g]",
                //
                @"SELECT [g].[Nickname], [g].[Discriminator]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] = N'Officer'",
                //
                @"SELECT [f].[Id], [f].[Discriminator]
FROM [Factions] AS [f]",
                //
                @"SELECT [f].[Id], [f].[Discriminator]
FROM [Factions] AS [f]",
                //
                @"SELECT [l].[Name], [l].[Discriminator]
FROM [LocustLeaders] AS [l]",
                //
                @"SELECT [l].[Name], [l].[Discriminator]
FROM [LocustLeaders] AS [l]
WHERE [l].[Discriminator] = N'LocustCommander'");
        }

        public override async Task Composite_key_entity_equal(bool async)
        {
            await base.Composite_key_entity_equal(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gears] AS [g]
CROSS JOIN [Gears] AS [g0]
WHERE ([g].[Nickname] = [g0].[Nickname]) AND ([g].[SquadId] = [g0].[SquadId])");
        }

        public override async Task Composite_key_entity_not_equal(bool async)
        {
            await base.Composite_key_entity_not_equal(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gears] AS [g]
CROSS JOIN [Gears] AS [g0]
WHERE ([g].[Nickname] <> [g0].[Nickname]) OR ([g].[SquadId] <> [g0].[SquadId])");
        }

        public override async Task Composite_key_entity_equal_null(bool async)
        {
            await base.Composite_key_entity_equal_null(async);

            AssertSql(
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [LocustLeaders] AS [l]
LEFT JOIN [Gears] AS [g] ON ([l].[DefeatedByNickname] = [g].[Nickname]) AND ([l].[DefeatedBySquadId] = [g].[SquadId])
WHERE ([l].[Discriminator] = N'LocustCommander') AND ([g].[Nickname] IS NULL OR [g].[SquadId] IS NULL)");
        }

        public override async Task Composite_key_entity_not_equal_null(bool async)
        {
            await base.Composite_key_entity_not_equal_null(async);

            AssertSql(
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [LocustLeaders] AS [l]
LEFT JOIN [Gears] AS [g] ON ([l].[DefeatedByNickname] = [g].[Nickname]) AND ([l].[DefeatedBySquadId] = [g].[SquadId])
WHERE ([l].[Discriminator] = N'LocustCommander') AND ([g].[Nickname] IS NOT NULL AND [g].[SquadId] IS NOT NULL)");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public async Task FreeText_with_binary_column()
        {
            using var context = CreateContext();
            var result = await context.Missions.SingleAsync(e => EF.Functions.FreeText(EF.Property<byte[]>(e, "BriefingDocument"), "bombing"));

            Assert.Equal(1, result.Id);

            AssertSql(
                @"SELECT TOP(2) [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE FREETEXT([m].[BriefingDocument], N'bombing')");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public async Task FreeText_with_binary_column_and_language_term()
        {
            using var context = CreateContext();
            var result = await context.Missions.SingleAsync(e => EF.Functions.FreeText(EF.Property<byte[]>(e, "BriefingDocument"), "bombing", 1033));

            Assert.Equal(1, result.Id);

            AssertSql(
                @"SELECT TOP(2) [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE FREETEXT([m].[BriefingDocument], N'bombing', LANGUAGE 1033)");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public async Task Contains_with_binary_column()
        {
            using var context = CreateContext();
            var result = await context.Missions.SingleAsync(e => EF.Functions.Contains(EF.Property<byte[]>(e, "BriefingDocument"), "bomb"));

            Assert.Equal(1, result.Id);

            AssertSql(
                @"SELECT TOP(2) [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE CONTAINS([m].[BriefingDocument], N'bomb')");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public async Task Contains_with_binary_column_and_language_term()
        {
            using var context = CreateContext();
            var result = await context.Missions.SingleAsync(e => EF.Functions.Contains(EF.Property<byte[]>(e, "BriefingDocument"), "bomb", 1033));

            Assert.Equal(1, result.Id);

            AssertSql(
                @"SELECT TOP(2) [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE CONTAINS([m].[BriefingDocument], N'bomb', LANGUAGE 1033)");
        }

        public override async Task Projecting_property_converted_to_nullable_with_comparison(bool async)
        {
            await base.Projecting_property_converted_to_nullable_with_comparison(async);

            AssertSql(
                @"SELECT [t].[Note], CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [g].[Nickname], [g].[SquadId], [g].[HasSoulPatch]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[SquadId]
    ELSE NULL
END = 1");
        }

        public override async Task Projecting_property_converted_to_nullable_with_addition(bool async)
        {
            await base.Projecting_property_converted_to_nullable_with_addition(async);

            AssertSql(
                @"SELECT [t].[Note], CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [g].[Nickname], [g].[SquadId], [g].[HasSoulPatch]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE (CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[SquadId]
    ELSE NULL
END + 1) = 2");
        }

        public override async Task Projecting_property_converted_to_nullable_with_addition_and_final_projection(bool async)
        {
            await base.Projecting_property_converted_to_nullable_with_addition_and_final_projection(async);

            AssertSql(
                @"SELECT [t].[Note], CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[SquadId]
    ELSE NULL
END + 1 AS [Value]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[Nickname]
    ELSE NULL
END IS NOT NULL");
        }

        public override async Task Projecting_property_converted_to_nullable_with_conditional(bool async)
        {
            await base.Projecting_property_converted_to_nullable_with_conditional(async);

            AssertSql(
                @"SELECT CASE
    WHEN ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL THEN CASE
        WHEN [t].[GearNickName] IS NOT NULL THEN [g].[SquadId]
        ELSE NULL
    END
    ELSE -1
END
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])");
        }

        public override async Task Projecting_property_converted_to_nullable_with_function_call(bool async)
        {
            await base.Projecting_property_converted_to_nullable_with_function_call(async);

            AssertSql(
                @"SELECT SUBSTRING(CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[Nickname]
    ELSE NULL
END, 0 + 1, 3)
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])");
        }

        public override async Task Projecting_property_converted_to_nullable_with_function_call2(bool async)
        {
            await base.Projecting_property_converted_to_nullable_with_function_call2(async);

            AssertSql(
                @"SELECT [t].[Note], SUBSTRING([t].[Note], 0 + 1, CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[SquadId]
    ELSE NULL
END) AS [Function]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[Nickname]
    ELSE NULL
END IS NOT NULL");
        }

        public override async Task Projecting_property_converted_to_nullable_into_element_init(bool async)
        {
            await base.Projecting_property_converted_to_nullable_into_element_init(async);

            AssertSql(
                @"SELECT CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN CAST(LEN([g].[Nickname]) AS int)
    ELSE NULL
END, CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[SquadId]
    ELSE NULL
END, CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[SquadId]
    ELSE NULL
END + 1
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[Nickname]
    ELSE NULL
END IS NOT NULL
ORDER BY [t].[Note]");
        }

        public override async Task Projecting_property_converted_to_nullable_into_member_assignment(bool async)
        {
            await base.Projecting_property_converted_to_nullable_into_member_assignment(async);

            AssertSql(
                @"SELECT CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[SquadId]
    ELSE NULL
END AS [Id]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[Nickname]
    ELSE NULL
END IS NOT NULL
ORDER BY [t].[Note]");
        }

        public override async Task Projecting_property_converted_to_nullable_into_new_array(bool async)
        {
            await base.Projecting_property_converted_to_nullable_into_new_array(async);

            AssertSql(
                @"SELECT CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN CAST(LEN([g].[Nickname]) AS int)
    ELSE NULL
END, CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[SquadId]
    ELSE NULL
END, CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[SquadId]
    ELSE NULL
END + 1
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[Nickname]
    ELSE NULL
END IS NOT NULL
ORDER BY [t].[Note]");
        }

        public override async Task Projecting_property_converted_to_nullable_into_unary(bool async)
        {
            await base.Projecting_property_converted_to_nullable_into_unary(async);

            AssertSql(
                @"SELECT [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[Nickname]
    ELSE NULL
END IS NOT NULL AND (CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[HasSoulPatch]
    ELSE NULL
END = CAST(0 AS bit))
ORDER BY [t].[Note]");
        }

        public override async Task Projecting_property_converted_to_nullable_into_member_access(bool async)
        {
            await base.Projecting_property_converted_to_nullable_into_member_access(async);

            AssertSql(
                @"SELECT [g].[Nickname]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
WHERE (DATEPART(month, [t].[IssueDate]) <> 5) OR [t].[IssueDate] IS NULL
ORDER BY [g].[Nickname]");
        }

        public override async Task Projecting_property_converted_to_nullable_and_use_it_in_order_by(bool async)
        {
            await base.Projecting_property_converted_to_nullable_and_use_it_in_order_by(async);

            AssertSql(
                @"SELECT [t].[Note], CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [g].[Nickname], [g].[SquadId], [g].[HasSoulPatch]
FROM [Tags] AS [t]
LEFT JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[Nickname]
    ELSE NULL
END IS NOT NULL
ORDER BY CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[SquadId]
    ELSE NULL
END, [t].[Note]");
        }

        public override async Task Correlated_collection_with_distinct_projecting_identifier_column(bool async)
        {
            await base.Correlated_collection_with_distinct_projecting_identifier_column(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [t].[Id], [t].[Name]
FROM [Gears] AS [g]
OUTER APPLY (
    SELECT DISTINCT [w].[Id], [w].[Name]
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
) AS [t]
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Id]");
        }

        public override async Task Correlated_collection_with_distinct_projecting_identifier_column_and_correlation_key(bool async)
        {
            await base.Correlated_collection_with_distinct_projecting_identifier_column_and_correlation_key(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [t].[Id], [t].[Name], [t].[OwnerFullName]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT DISTINCT [w].[Id], [w].[Name], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Id]");
        }

        public override async Task Correlated_collection_with_distinct_projecting_identifier_column_composite_key(bool async)
        {
            await base.Correlated_collection_with_distinct_projecting_identifier_column_composite_key(async);

            AssertSql(
                @"SELECT [s].[Id], [t].[Nickname], [t].[SquadId], [t].[HasSoulPatch]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT DISTINCT [g].[Nickname], [g].[SquadId], [g].[HasSoulPatch]
    FROM [Gears] AS [g]
) AS [t] ON [s].[Id] = [t].[SquadId]
ORDER BY [s].[Id], [t].[Nickname], [t].[SquadId]");
        }

        public override async Task Correlated_collection_with_distinct_not_projecting_identifier_column(bool async)
        {
            await base.Correlated_collection_with_distinct_not_projecting_identifier_column(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [t].[Name], [t].[IsAutomatic]
FROM [Gears] AS [g]
OUTER APPLY (
    SELECT DISTINCT [w].[Name], [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
) AS [t]
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Name], [t].[IsAutomatic]");
        }

        public override async Task Correlated_collection_with_groupby_not_projecting_identifier_column_but_only_grouping_key_in_final_projection(bool async)
        {
            await base.Correlated_collection_with_groupby_not_projecting_identifier_column_but_only_grouping_key_in_final_projection(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [t].[Key]
FROM [Gears] AS [g]
OUTER APPLY (
    SELECT [w].[IsAutomatic] AS [Key]
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    GROUP BY [w].[IsAutomatic]
) AS [t]
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Key]");
        }

        public override async Task Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection(bool async)
        {
            await base.Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [t].[Key], [t].[Count]
FROM [Gears] AS [g]
OUTER APPLY (
    SELECT [w].[IsAutomatic] AS [Key], COUNT(*) AS [Count]
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    GROUP BY [w].[IsAutomatic]
) AS [t]
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Key]");
        }

        public override async Task Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection_multiple_grouping_keys(bool async)
        {
            await base.Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection_multiple_grouping_keys(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [t].[IsAutomatic], [t].[Name], [t].[Count]
FROM [Gears] AS [g]
OUTER APPLY (
    SELECT [w].[IsAutomatic], [w].[Name], COUNT(*) AS [Count]
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    GROUP BY [w].[IsAutomatic], [w].[Name]
) AS [t]
ORDER BY [g].[Nickname], [g].[SquadId], [t].[IsAutomatic], [t].[Name]");
        }

        public override async Task Correlated_collection_via_SelectMany_with_Distinct_missing_indentifying_columns_in_projection(bool async)
        {
            await base.Correlated_collection_via_SelectMany_with_Distinct_missing_indentifying_columns_in_projection(async);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [t].[HasSoulPatch]
FROM [Gears] AS [g]
OUTER APPLY (
    SELECT DISTINCT [g1].[HasSoulPatch]
    FROM [Weapons] AS [w]
    LEFT JOIN [Gears] AS [g0] ON [w].[OwnerFullName] = [g0].[FullName]
    LEFT JOIN [Cities] AS [c] ON [g0].[AssignedCityName] = [c].[Name]
    INNER JOIN [Gears] AS [g1] ON [c].[Name] = [g1].[CityOfBirthName]
    WHERE [g].[FullName] = [w].[OwnerFullName]
) AS [t]
ORDER BY [g].[Nickname], [g].[SquadId], [t].[HasSoulPatch]");
        }

        public override async Task Correlated_collection_after_distinct_3_levels(bool async)
        {
            await base.Correlated_collection_after_distinct_3_levels(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[Name], [t1].[Nickname], [t1].[FullName], [t1].[HasSoulPatch], [t1].[Id], [t1].[Name], [t1].[Nickname0], [t1].[FullName0], [t1].[HasSoulPatch0], [t1].[Id0]
FROM (
    SELECT DISTINCT [s].[Id], [s].[Name]
    FROM [Squads] AS [s]
) AS [t]
OUTER APPLY (
    SELECT [t0].[Nickname], [t0].[FullName], [t0].[HasSoulPatch], [t2].[Id], [t2].[Name], [t2].[Nickname] AS [Nickname0], [t2].[FullName] AS [FullName0], [t2].[HasSoulPatch] AS [HasSoulPatch0], [t2].[Id0]
    FROM (
        SELECT DISTINCT [g].[Nickname], [g].[FullName], [g].[HasSoulPatch]
        FROM [Gears] AS [g]
        WHERE [g].[SquadId] = [t].[Id]
    ) AS [t0]
    OUTER APPLY (
        SELECT [t].[Id], [t].[Name], [t0].[Nickname], [t0].[FullName], [t0].[HasSoulPatch], [w].[Id] AS [Id0]
        FROM [Weapons] AS [w]
        WHERE [t0].[FullName] = [w].[OwnerFullName]
    ) AS [t2]
) AS [t1]
ORDER BY [t].[Id], [t1].[Nickname], [t1].[FullName], [t1].[HasSoulPatch], [t1].[Id0]");
        }

        public override async Task Correlated_collection_after_distinct_3_levels_without_original_identifiers(bool async)
        {
            await base.Correlated_collection_after_distinct_3_levels_without_original_identifiers(async);

            AssertSql(
                @"SELECT [t].[Length], [t2].[HasSoulPatch], [t2].[CityOfBirthName], [t2].[Id], [t2].[Length], [t2].[HasSoulPatch0]
FROM (
    SELECT DISTINCT CAST(LEN([s].[Name]) AS int) AS [Length]
    FROM [Squads] AS [s]
) AS [t]
OUTER APPLY (
    SELECT [t0].[HasSoulPatch], [t0].[CityOfBirthName], [t1].[Id], [t1].[Length], [t1].[HasSoulPatch] AS [HasSoulPatch0]
    FROM (
        SELECT DISTINCT [g].[HasSoulPatch], [g].[CityOfBirthName]
        FROM [Gears] AS [g]
        WHERE CAST(LEN([g].[Nickname]) AS int) = [t].[Length]
    ) AS [t0]
    OUTER APPLY (
        SELECT [w].[Id], [t].[Length], [t0].[HasSoulPatch]
        FROM [Weapons] AS [w]
        WHERE [t0].[CityOfBirthName] = [w].[OwnerFullName]
    ) AS [t1]
) AS [t2]
ORDER BY [t].[Length], [t2].[HasSoulPatch], [t2].[CityOfBirthName], [t2].[Id]");
        }

        [ConditionalTheory(Skip = "#24507")]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Where_DateOnly_Year(bool async)
        {
            await base.Where_DateOnly_Year(async);

            AssertSql("");
        }

        [ConditionalTheory(Skip = "#24507")]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Where_DateOnly_Month(bool async)
        {
            await base.Where_DateOnly_Month(async);

            AssertSql("");
        }

        [ConditionalTheory(Skip = "#24507")]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Where_DateOnly_Day(bool async)
        {
            await base.Where_DateOnly_Day(async);

            AssertSql("");
        }

        [ConditionalTheory(Skip = "#24507")]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Where_DateOnly_DayOfYear(bool async)
        {
            await base.Where_DateOnly_DayOfYear(async);

            AssertSql("");
        }

        [ConditionalTheory(Skip = "#24507")]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Where_DateOnly_DayOfWeek(bool async)
        {
            await base.Where_DateOnly_DayOfWeek(async);

            AssertSql("");
        }

        [ConditionalTheory(Skip = "#24507")]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Where_DateOnly_AddYears(bool async)
        {
            await base.Where_DateOnly_AddYears(async);

            AssertSql("");
        }

        [ConditionalTheory(Skip = "#24507")]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Where_DateOnly_AddMonths(bool async)
        {
            await base.Where_DateOnly_AddMonths(async);

            AssertSql("");
        }

        [ConditionalTheory(Skip = "#24507")]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Where_DateOnly_AddDays(bool async)
        {
            await base.Where_DateOnly_AddDays(async);

            AssertSql("");
        }

        [ConditionalTheory(Skip = "#24507")]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Where_TimeOnly_Hour(bool async)
        {
            await base.Where_TimeOnly_Hour(async);

            AssertSql("");
        }

        [ConditionalTheory(Skip = "#24507")]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Where_TimeOnly_Minute(bool async)
        {
            await base.Where_TimeOnly_Minute(async);

            AssertSql("");
        }

        [ConditionalTheory(Skip = "#24507")]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Where_TimeOnly_Second(bool async)
        {
            await base.Where_TimeOnly_Second(async);

            AssertSql("");
        }

        [ConditionalTheory(Skip = "#24507")]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Where_TimeOnly_Millisecond(bool async)
        {
            await base.Where_TimeOnly_Millisecond(async);

            AssertSql("");
        }

        [ConditionalTheory(Skip = "#24507")]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Where_TimeOnly_AddHours(bool async)
        {
            await base.Where_TimeOnly_AddHours(async);

            AssertSql("");
        }

        [ConditionalTheory(Skip = "#24507")]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Where_TimeOnly_AddMinutes(bool async)
        {
            await base.Where_TimeOnly_AddMinutes(async);

            AssertSql("");
        }

        [ConditionalTheory(Skip = "#24507")]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Where_TimeOnly_Add_TimeSpan(bool async)
        {
            await base.Where_TimeOnly_Add_TimeSpan(async);

            AssertSql("");
        }

        [ConditionalTheory(Skip = "#24507")]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Where_TimeOnly_IsBetween(bool async)
        {
            await base.Where_TimeOnly_IsBetween(async);

            AssertSql("");
        }

        [ConditionalTheory(Skip = "#24507")]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Where_TimeOnly_subtract_TimeOnly(bool async)
        {
            await base.Where_TimeOnly_subtract_TimeOnly(async);

            AssertSql("");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
