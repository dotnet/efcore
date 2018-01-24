// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class AsyncGearsOfWarQuerySqlServerTest : AsyncGearsOfWarQueryTestBase<GearsOfWarQuerySqlServerFixture>
    {

        public AsyncGearsOfWarQuerySqlServerTest(GearsOfWarQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Correlated_collections_naked_navigation_with_ToList()
        {
            await base.Correlated_collections_naked_navigation_with_ToList();

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] <> N'Marcus')
ORDER BY [g].[Nickname], [g].[SquadId], [g].[FullName]",
                //
                @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId], [t].[Nickname], [t].[SquadId], [t].[FullName]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[FullName]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND ([g0].[Nickname] <> N'Marcus')
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
ORDER BY [t].[Nickname], [t].[SquadId], [t].[FullName]");
        }

        public override async Task Correlated_collections_naked_navigation_with_ToArray()
        {
            await base.Correlated_collections_naked_navigation_with_ToArray();

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] <> N'Marcus')
ORDER BY [g].[Nickname], [g].[SquadId], [g].[FullName]",
                //
                @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId], [t].[Nickname], [t].[SquadId], [t].[FullName]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[FullName]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND ([g0].[Nickname] <> N'Marcus')
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
ORDER BY [t].[Nickname], [t].[SquadId], [t].[FullName]");
        }

        public override async Task Correlated_collections_basic_projection()
        {
            await base.Correlated_collections_basic_projection();

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] <> N'Marcus')
ORDER BY [g].[Nickname], [g].[SquadId], [g].[FullName]",
                //
                @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId], [t].[Nickname], [t].[SquadId], [t].[FullName]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[FullName]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND ([g0].[Nickname] <> N'Marcus')
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
WHERE ([g.Weapons].[IsAutomatic] = 1) OR (([g.Weapons].[Name] <> N'foo') OR [g.Weapons].[Name] IS NULL)
ORDER BY [t].[Nickname], [t].[SquadId], [t].[FullName]");
        }

        public override async Task Correlated_collections_basic_projection_explicit_to_list()
        {
            await base.Correlated_collections_basic_projection_explicit_to_list();

            AssertSql(
                 @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] <> N'Marcus')
ORDER BY [g].[Nickname], [g].[SquadId], [g].[FullName]",
                 //
                 @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId], [t].[Nickname], [t].[SquadId], [t].[FullName]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[FullName]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND ([g0].[Nickname] <> N'Marcus')
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
WHERE ([g.Weapons].[IsAutomatic] = 1) OR (([g.Weapons].[Name] <> N'foo') OR [g.Weapons].[Name] IS NULL)
ORDER BY [t].[Nickname], [t].[SquadId], [t].[FullName]");
        }

        public override async Task Correlated_collections_basic_projection_explicit_to_array()
        {
            await base.Correlated_collections_basic_projection_explicit_to_array();

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] <> N'Marcus')
ORDER BY [g].[Nickname], [g].[SquadId], [g].[FullName]",
                //
                @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId], [t].[Nickname], [t].[SquadId], [t].[FullName]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[FullName]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND ([g0].[Nickname] <> N'Marcus')
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
WHERE ([g.Weapons].[IsAutomatic] = 1) OR (([g.Weapons].[Name] <> N'foo') OR [g.Weapons].[Name] IS NULL)
ORDER BY [t].[Nickname], [t].[SquadId], [t].[FullName]");
        }

        public override async Task Correlated_collections_basic_projection_ordered()
        {
            await base.Correlated_collections_basic_projection_ordered();

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] <> N'Marcus')
ORDER BY [g].[Nickname], [g].[SquadId], [g].[FullName]",
                //
                @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId], [t].[Nickname], [t].[SquadId], [t].[FullName]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[FullName]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND ([g0].[Nickname] <> N'Marcus')
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
WHERE ([g.Weapons].[IsAutomatic] = 1) OR (([g.Weapons].[Name] <> N'foo') OR [g.Weapons].[Name] IS NULL)
ORDER BY [t].[Nickname], [t].[SquadId], [t].[FullName], [g.Weapons].[Name] DESC");
        }

        public override async Task Correlated_collections_basic_projection_composite_key()
        {
            await base.Correlated_collections_basic_projection_composite_key();

            AssertSql(
                @"SELECT [o].[Nickname], [o].[SquadId]
FROM [Gears] AS [o]
WHERE ([o].[Discriminator] = N'Officer') AND ([o].[Nickname] <> N'Foo')
ORDER BY [o].[Nickname], [o].[SquadId]",
                //
                @"SELECT [t].[Nickname], [t].[SquadId], [o.Reports].[Nickname] AS [Nickname0], [o.Reports].[FullName], [o.Reports].[LeaderNickname], [o.Reports].[LeaderSquadId]
FROM [Gears] AS [o.Reports]
INNER JOIN (
    SELECT [o0].[Nickname], [o0].[SquadId]
    FROM [Gears] AS [o0]
    WHERE ([o0].[Discriminator] = N'Officer') AND ([o0].[Nickname] <> N'Foo')
) AS [t] ON ([o.Reports].[LeaderNickname] = [t].[Nickname]) AND ([o.Reports].[LeaderSquadId] = [t].[SquadId])
WHERE [o.Reports].[Discriminator] IN (N'Officer', N'Gear') AND ([o.Reports].[HasSoulPatch] = 0)
ORDER BY [t].[Nickname], [t].[SquadId]");
        }

        public override async Task Correlated_collections_basic_projecting_single_property()
        {
            await base.Correlated_collections_basic_projecting_single_property();

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] <> N'Marcus')
ORDER BY [g].[Nickname], [g].[SquadId], [g].[FullName]",
                //
                @"SELECT [t].[Nickname], [t].[SquadId], [t].[FullName], [g.Weapons].[Name], [g.Weapons].[OwnerFullName]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[FullName]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND ([g0].[Nickname] <> N'Marcus')
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
WHERE ([g.Weapons].[IsAutomatic] = 1) OR (([g.Weapons].[Name] <> N'foo') OR [g.Weapons].[Name] IS NULL)
ORDER BY [t].[Nickname], [t].[SquadId], [t].[FullName]");
        }

        public override async Task Correlated_collections_basic_projecting_constant()
        {
            await base.Correlated_collections_basic_projecting_constant();

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] <> N'Marcus')
ORDER BY [g].[Nickname], [g].[SquadId], [g].[FullName]",
                //
                @"SELECT [t].[Nickname], [t].[SquadId], [t].[FullName], [g.Weapons].[OwnerFullName]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[FullName]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND ([g0].[Nickname] <> N'Marcus')
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
WHERE ([g.Weapons].[IsAutomatic] = 1) OR (([g.Weapons].[Name] <> N'foo') OR [g.Weapons].[Name] IS NULL)
ORDER BY [t].[Nickname], [t].[SquadId], [t].[FullName]");
        }

        public override async Task Correlated_collections_projection_of_collection_thru_navigation()
        {
            await base.Correlated_collections_projection_of_collection_thru_navigation();

            AssertSql(
                @"SELECT [g.Squad].[Id]
FROM [Gears] AS [g]
INNER JOIN [Squads] AS [g.Squad] ON [g].[SquadId] = [g.Squad].[Id]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] <> N'Marcus')
ORDER BY [g].[FullName], [g].[Nickname], [g].[SquadId], [g.Squad].[Id]",
                //
                @"SELECT [g.Squad.Missions].[SquadId], [g.Squad.Missions].[MissionId], [t].[FullName], [t].[Nickname], [t].[SquadId], [t].[Id]
FROM [SquadMissions] AS [g.Squad.Missions]
INNER JOIN (
    SELECT [g0].[FullName], [g0].[Nickname], [g0].[SquadId], [g.Squad0].[Id]
    FROM [Gears] AS [g0]
    INNER JOIN [Squads] AS [g.Squad0] ON [g0].[SquadId] = [g.Squad0].[Id]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND ([g0].[Nickname] <> N'Marcus')
) AS [t] ON [g.Squad.Missions].[SquadId] = [t].[Id]
WHERE [g.Squad.Missions].[MissionId] <> 17
ORDER BY [t].[FullName], [t].[Nickname], [t].[SquadId], [t].[Id]");
        }

        public override async Task Correlated_collections_project_anonymous_collection_result()
        {
            await base.Correlated_collections_project_anonymous_collection_result();

            AssertSql(
                 @"SELECT [s].[Name], [s].[Id]
FROM [Squads] AS [s]
WHERE [s].[Id] < 20
ORDER BY [s].[Id]",
                 //
                 @"SELECT [t].[Id], [s.Members].[FullName], [s.Members].[Rank], [s.Members].[SquadId]
FROM [Gears] AS [s.Members]
INNER JOIN (
    SELECT [s0].[Id]
    FROM [Squads] AS [s0]
    WHERE [s0].[Id] < 20
) AS [t] ON [s.Members].[SquadId] = [t].[Id]
WHERE [s.Members].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[Id]");
        }

        public override async Task Correlated_collections_nested()
        {
            await base.Correlated_collections_nested();

            AssertSql(
                  @"SELECT [s].[Id]
FROM [Squads] AS [s]
ORDER BY [s].[Id]",
                  //
                  @"SELECT [t].[Id], [m.Mission].[Id], [s.Missions].[SquadId]
FROM [SquadMissions] AS [s.Missions]
INNER JOIN [Missions] AS [m.Mission] ON [s.Missions].[MissionId] = [m.Mission].[Id]
INNER JOIN (
    SELECT [s0].[Id]
    FROM [Squads] AS [s0]
) AS [t] ON [s.Missions].[SquadId] = [t].[Id]
WHERE [s.Missions].[MissionId] < 42
ORDER BY [t].[Id], [s.Missions].[SquadId], [s.Missions].[MissionId], [m.Mission].[Id]",
                  //
                  @"SELECT [m.Mission.ParticipatingSquads].[SquadId], [m.Mission.ParticipatingSquads].[MissionId], [t1].[Id], [t1].[SquadId], [t1].[MissionId], [t1].[Id0]
FROM [SquadMissions] AS [m.Mission.ParticipatingSquads]
INNER JOIN (
    SELECT [t0].[Id], [s.Missions0].[SquadId], [s.Missions0].[MissionId], [m.Mission0].[Id] AS [Id0]
    FROM [SquadMissions] AS [s.Missions0]
    INNER JOIN [Missions] AS [m.Mission0] ON [s.Missions0].[MissionId] = [m.Mission0].[Id]
    INNER JOIN (
        SELECT [s1].[Id]
        FROM [Squads] AS [s1]
    ) AS [t0] ON [s.Missions0].[SquadId] = [t0].[Id]
    WHERE [s.Missions0].[MissionId] < 42
) AS [t1] ON [m.Mission.ParticipatingSquads].[MissionId] = [t1].[Id0]
WHERE [m.Mission.ParticipatingSquads].[SquadId] < 7
ORDER BY [t1].[Id], [t1].[SquadId], [t1].[MissionId], [t1].[Id0]");
        }

        public override async Task Correlated_collections_nested_mixed_streaming_with_buffer1()
        {
            await base.Correlated_collections_nested_mixed_streaming_with_buffer1();

            AssertSql(
                  @"SELECT [s].[Id]
FROM [Squads] AS [s]",
                  //
                  @"@_outer_Id='1'

SELECT [m.Mission].[Id]
FROM [SquadMissions] AS [m]
INNER JOIN [Missions] AS [m.Mission] ON [m].[MissionId] = [m.Mission].[Id]
WHERE ([m].[MissionId] < 3) AND (@_outer_Id = [m].[SquadId])",
                  //
                  @"@_outer_Id1='1'

SELECT [ps].[SquadId], [ps].[MissionId]
FROM [SquadMissions] AS [ps]
WHERE ([ps].[SquadId] < 2) AND (@_outer_Id1 = [ps].[MissionId])",
                  //
                  @"@_outer_Id1='2'

SELECT [ps].[SquadId], [ps].[MissionId]
FROM [SquadMissions] AS [ps]
WHERE ([ps].[SquadId] < 2) AND (@_outer_Id1 = [ps].[MissionId])",
                  //
                  @"@_outer_Id='2'

SELECT [m.Mission].[Id]
FROM [SquadMissions] AS [m]
INNER JOIN [Missions] AS [m.Mission] ON [m].[MissionId] = [m.Mission].[Id]
WHERE ([m].[MissionId] < 3) AND (@_outer_Id = [m].[SquadId])",
                  //
                  @"@_outer_Id='2'

SELECT [m.Mission].[Id]
FROM [SquadMissions] AS [m]
INNER JOIN [Missions] AS [m.Mission] ON [m].[MissionId] = [m.Mission].[Id]
WHERE ([m].[MissionId] < 3) AND (@_outer_Id = [m].[SquadId])",
                  //
                  @"@_outer_Id='1'

SELECT [m.Mission].[Id]
FROM [SquadMissions] AS [m]
INNER JOIN [Missions] AS [m.Mission] ON [m].[MissionId] = [m.Mission].[Id]
WHERE ([m].[MissionId] < 3) AND (@_outer_Id = [m].[SquadId])",
                  //
                  @"@_outer_Id1='1'

SELECT [ps].[SquadId], [ps].[MissionId]
FROM [SquadMissions] AS [ps]
WHERE ([ps].[SquadId] < 2) AND (@_outer_Id1 = [ps].[MissionId])",
                  //
                  @"@_outer_Id1='2'

SELECT [ps].[SquadId], [ps].[MissionId]
FROM [SquadMissions] AS [ps]
WHERE ([ps].[SquadId] < 2) AND (@_outer_Id1 = [ps].[MissionId])");
        }

        public override async Task Correlated_collections_nested_mixed_streaming_with_buffer2()
        {
            await base.Correlated_collections_nested_mixed_streaming_with_buffer2();

            AssertSql(
                 @"SELECT [s].[Id]
FROM [Squads] AS [s]
ORDER BY [s].[Id]",
                 //
                 @"SELECT [t].[Id], [m.Mission].[Id], [s.Missions].[SquadId]
FROM [SquadMissions] AS [s.Missions]
INNER JOIN [Missions] AS [m.Mission] ON [s.Missions].[MissionId] = [m.Mission].[Id]
INNER JOIN (
    SELECT [s0].[Id]
    FROM [Squads] AS [s0]
) AS [t] ON [s.Missions].[SquadId] = [t].[Id]
WHERE [s.Missions].[MissionId] < 42
ORDER BY [t].[Id]",
                 //
                 @"@_outer_Id='3'

SELECT [ps].[SquadId], [ps].[MissionId]
FROM [SquadMissions] AS [ps]
WHERE ([ps].[SquadId] < 7) AND (@_outer_Id = [ps].[MissionId])",
                 //
                 @"@_outer_Id='3'

SELECT [ps].[SquadId], [ps].[MissionId]
FROM [SquadMissions] AS [ps]
WHERE ([ps].[SquadId] < 7) AND (@_outer_Id = [ps].[MissionId])",
                 //
                 @"@_outer_Id='1'

SELECT [ps].[SquadId], [ps].[MissionId]
FROM [SquadMissions] AS [ps]
WHERE ([ps].[SquadId] < 7) AND (@_outer_Id = [ps].[MissionId])",
                 //
                 @"@_outer_Id='2'

SELECT [ps].[SquadId], [ps].[MissionId]
FROM [SquadMissions] AS [ps]
WHERE ([ps].[SquadId] < 7) AND (@_outer_Id = [ps].[MissionId])",
                 //
                 @"@_outer_Id='1'

SELECT [ps].[SquadId], [ps].[MissionId]
FROM [SquadMissions] AS [ps]
WHERE ([ps].[SquadId] < 7) AND (@_outer_Id = [ps].[MissionId])",
                 //
                 @"@_outer_Id='2'

SELECT [ps].[SquadId], [ps].[MissionId]
FROM [SquadMissions] AS [ps]
WHERE ([ps].[SquadId] < 7) AND (@_outer_Id = [ps].[MissionId])");
        }

        public override async Task Correlated_collections_nested_with_custom_ordering()
        {
            await base.Correlated_collections_nested_with_custom_ordering();

            AssertSql(
                 @"SELECT [o].[FullName], [o].[Nickname], [o].[SquadId]
FROM [Gears] AS [o]
WHERE [o].[Discriminator] = N'Officer'
ORDER BY [o].[HasSoulPatch] DESC, [o].[Nickname], [o].[SquadId]",
                 //
                 @"SELECT [t].[HasSoulPatch], [t].[Nickname], [t].[SquadId], [o.Reports].[FullName], [o.Reports].[LeaderNickname], [o.Reports].[LeaderSquadId]
FROM [Gears] AS [o.Reports]
INNER JOIN (
    SELECT [o0].[HasSoulPatch], [o0].[Nickname], [o0].[SquadId]
    FROM [Gears] AS [o0]
    WHERE [o0].[Discriminator] = N'Officer'
) AS [t] ON ([o.Reports].[LeaderNickname] = [t].[Nickname]) AND ([o.Reports].[LeaderSquadId] = [t].[SquadId])
WHERE [o.Reports].[Discriminator] IN (N'Officer', N'Gear') AND ([o.Reports].[FullName] <> N'Foo')
ORDER BY [t].[HasSoulPatch] DESC, [t].[Nickname], [t].[SquadId], [o.Reports].[Rank], [o.Reports].[Nickname], [o.Reports].[SquadId], [o.Reports].[FullName]",
                 //
                 @"SELECT [o.Reports.Weapons].[Id], [o.Reports.Weapons].[AmmunitionType], [o.Reports.Weapons].[IsAutomatic], [o.Reports.Weapons].[Name], [o.Reports.Weapons].[OwnerFullName], [o.Reports.Weapons].[SynergyWithId], [t1].[HasSoulPatch], [t1].[Nickname], [t1].[SquadId], [t1].[Rank], [t1].[Nickname0], [t1].[SquadId0], [t1].[FullName]
FROM [Weapons] AS [o.Reports.Weapons]
INNER JOIN (
    SELECT [t0].[HasSoulPatch], [t0].[Nickname], [t0].[SquadId], [o.Reports0].[Rank], [o.Reports0].[Nickname] AS [Nickname0], [o.Reports0].[SquadId] AS [SquadId0], [o.Reports0].[FullName]
    FROM [Gears] AS [o.Reports0]
    INNER JOIN (
        SELECT [o1].[HasSoulPatch], [o1].[Nickname], [o1].[SquadId]
        FROM [Gears] AS [o1]
        WHERE [o1].[Discriminator] = N'Officer'
    ) AS [t0] ON ([o.Reports0].[LeaderNickname] = [t0].[Nickname]) AND ([o.Reports0].[LeaderSquadId] = [t0].[SquadId])
    WHERE [o.Reports0].[Discriminator] IN (N'Officer', N'Gear') AND ([o.Reports0].[FullName] <> N'Foo')
) AS [t1] ON [o.Reports.Weapons].[OwnerFullName] = [t1].[FullName]
WHERE ([o.Reports.Weapons].[Name] <> N'Bar') OR [o.Reports.Weapons].[Name] IS NULL
ORDER BY [t1].[HasSoulPatch] DESC, [t1].[Nickname], [t1].[SquadId], [t1].[Rank], [t1].[Nickname0], [t1].[SquadId0], [t1].[FullName], [o.Reports.Weapons].[IsAutomatic]");
        }

        public override async Task Correlated_collections_same_collection_projected_multiple_times()
        {
            await base.Correlated_collections_same_collection_projected_multiple_times();

            AssertSql(
                  @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname], [g].[SquadId], [g].[FullName]",
                  //
                  @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId], [t].[Nickname], [t].[SquadId], [t].[FullName]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[FullName]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
WHERE [g.Weapons].[IsAutomatic] = 1
ORDER BY [t].[Nickname], [t].[SquadId], [t].[FullName]",
                  //
                  @"SELECT [g.Weapons0].[Id], [g.Weapons0].[AmmunitionType], [g.Weapons0].[IsAutomatic], [g.Weapons0].[Name], [g.Weapons0].[OwnerFullName], [g.Weapons0].[SynergyWithId], [t0].[Nickname], [t0].[SquadId], [t0].[FullName]
FROM [Weapons] AS [g.Weapons0]
INNER JOIN (
    SELECT [g1].[Nickname], [g1].[SquadId], [g1].[FullName]
    FROM [Gears] AS [g1]
    WHERE [g1].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON [g.Weapons0].[OwnerFullName] = [t0].[FullName]
WHERE [g.Weapons0].[IsAutomatic] = 1
ORDER BY [t0].[Nickname], [t0].[SquadId], [t0].[FullName]");
        }

        public override async Task Correlated_collections_similar_collection_projected_multiple_times()
        {
            await base.Correlated_collections_similar_collection_projected_multiple_times();

            AssertSql(
                  @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Rank], [g].[Nickname], [g].[SquadId], [g].[FullName]",
                  //
                  @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId], [t].[Rank], [t].[Nickname], [t].[SquadId], [t].[FullName]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT [g0].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[FullName]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
WHERE [g.Weapons].[IsAutomatic] = 1
ORDER BY [t].[Rank], [t].[Nickname], [t].[SquadId], [t].[FullName], [g.Weapons].[OwnerFullName]",
                  //
                  @"SELECT [g.Weapons0].[Id], [g.Weapons0].[AmmunitionType], [g.Weapons0].[IsAutomatic], [g.Weapons0].[Name], [g.Weapons0].[OwnerFullName], [g.Weapons0].[SynergyWithId], [t0].[Rank], [t0].[Nickname], [t0].[SquadId], [t0].[FullName]
FROM [Weapons] AS [g.Weapons0]
INNER JOIN (
    SELECT [g1].[Rank], [g1].[Nickname], [g1].[SquadId], [g1].[FullName]
    FROM [Gears] AS [g1]
    WHERE [g1].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON [g.Weapons0].[OwnerFullName] = [t0].[FullName]
WHERE [g.Weapons0].[IsAutomatic] = 0
ORDER BY [t0].[Rank], [t0].[Nickname], [t0].[SquadId], [t0].[FullName], [g.Weapons0].[IsAutomatic]");
        }

        public override async Task Correlated_collections_different_collections_projected()
        {
            await base.Correlated_collections_different_collections_projected();

            AssertSql(
                  @"SELECT [o].[Nickname], [o].[FullName], [o].[SquadId]
FROM [Gears] AS [o]
WHERE [o].[Discriminator] = N'Officer'
ORDER BY [o].[FullName], [o].[Nickname], [o].[SquadId]",
                  //
                  @"SELECT [t].[FullName], [t].[Nickname], [t].[SquadId], [o.Weapons].[Name], [o.Weapons].[IsAutomatic], [o.Weapons].[OwnerFullName]
FROM [Weapons] AS [o.Weapons]
INNER JOIN (
    SELECT [o0].[FullName], [o0].[Nickname], [o0].[SquadId]
    FROM [Gears] AS [o0]
    WHERE [o0].[Discriminator] = N'Officer'
) AS [t] ON [o.Weapons].[OwnerFullName] = [t].[FullName]
WHERE [o.Weapons].[IsAutomatic] = 1
ORDER BY [t].[FullName], [t].[Nickname], [t].[SquadId]",
                  //
                  @"SELECT [t0].[FullName], [t0].[Nickname], [t0].[SquadId], [o.Reports].[Nickname] AS [Nickname0], [o.Reports].[Rank], [o.Reports].[LeaderNickname], [o.Reports].[LeaderSquadId]
FROM [Gears] AS [o.Reports]
INNER JOIN (
    SELECT [o1].[FullName], [o1].[Nickname], [o1].[SquadId]
    FROM [Gears] AS [o1]
    WHERE [o1].[Discriminator] = N'Officer'
) AS [t0] ON ([o.Reports].[LeaderNickname] = [t0].[Nickname]) AND ([o.Reports].[LeaderSquadId] = [t0].[SquadId])
WHERE [o.Reports].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t0].[FullName], [t0].[Nickname], [t0].[SquadId], [o.Reports].[FullName]");
        }

        public override async Task Correlated_collections_multiple_nested_complex_collections()
        {
            await base.Correlated_collections_multiple_nested_complex_collections();

            AssertSql(
                  @"SELECT [o].[FullName] AS [FullName0], [o].[Nickname], [o].[SquadId], [t].[FullName]
FROM [Gears] AS [o]
LEFT JOIN [Tags] AS [o.Tag] ON ([o].[Nickname] = [o.Tag].[GearNickName]) AND ([o].[SquadId] = [o.Tag].[GearSquadId])
LEFT JOIN (
    SELECT [o.Tag.Gear].*
    FROM [Gears] AS [o.Tag.Gear]
    WHERE [o.Tag.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([o.Tag].[GearNickName] = [t].[Nickname]) AND ([o.Tag].[GearSquadId] = [t].[SquadId])
WHERE ([o].[Discriminator] = N'Officer') AND EXISTS (
    SELECT 1
    FROM [Gears] AS [g]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([o].[Nickname] = [g].[LeaderNickname]) AND ([o].[SquadId] = [g].[LeaderSquadId])))
ORDER BY [o].[HasSoulPatch] DESC, [o.Tag].[Note], [o].[Nickname], [o].[SquadId], [t].[FullName]",
                  //
                  @"SELECT [t1].[HasSoulPatch], [t1].[Note], [t1].[Nickname], [t1].[SquadId], [o.Reports].[FullName], [o.Reports].[LeaderNickname], [o.Reports].[LeaderSquadId]
FROM [Gears] AS [o.Reports]
INNER JOIN (
    SELECT [o0].[HasSoulPatch], [o.Tag0].[Note], [o0].[Nickname], [o0].[SquadId]
    FROM [Gears] AS [o0]
    LEFT JOIN [Tags] AS [o.Tag0] ON ([o0].[Nickname] = [o.Tag0].[GearNickName]) AND ([o0].[SquadId] = [o.Tag0].[GearSquadId])
    LEFT JOIN (
        SELECT [o.Tag.Gear0].*
        FROM [Gears] AS [o.Tag.Gear0]
        WHERE [o.Tag.Gear0].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t0] ON ([o.Tag0].[GearNickName] = [t0].[Nickname]) AND ([o.Tag0].[GearSquadId] = [t0].[SquadId])
    WHERE ([o0].[Discriminator] = N'Officer') AND EXISTS (
        SELECT 1
        FROM [Gears] AS [g0]
        WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND (([o0].[Nickname] = [g0].[LeaderNickname]) AND ([o0].[SquadId] = [g0].[LeaderSquadId])))
) AS [t1] ON ([o.Reports].[LeaderNickname] = [t1].[Nickname]) AND ([o.Reports].[LeaderSquadId] = [t1].[SquadId])
WHERE [o.Reports].[Discriminator] IN (N'Officer', N'Gear') AND ([o.Reports].[FullName] <> N'Foo')
ORDER BY [t1].[HasSoulPatch] DESC, [t1].[Note], [t1].[Nickname], [t1].[SquadId], [o.Reports].[Rank], [o.Reports].[Nickname], [o.Reports].[SquadId], [o.Reports].[FullName]",
                  //
                  @"SELECT [t5].[HasSoulPatch], [t5].[Note], [t5].[Nickname], [t5].[SquadId], [t5].[Rank], [t5].[Nickname0], [t5].[SquadId0], [t5].[FullName], [o.Reports.Weapons].[Id] AS [Id0], [t2].[FullName], [w.Owner.Squad].[Id], [o.Reports.Weapons].[OwnerFullName]
FROM [Weapons] AS [o.Reports.Weapons]
LEFT JOIN (
    SELECT [w.Owner].*
    FROM [Gears] AS [w.Owner]
    WHERE [w.Owner].[Discriminator] IN (N'Officer', N'Gear')
) AS [t2] ON [o.Reports.Weapons].[OwnerFullName] = [t2].[FullName]
LEFT JOIN [Squads] AS [w.Owner.Squad] ON [t2].[SquadId] = [w.Owner.Squad].[Id]
INNER JOIN (
    SELECT [t4].[HasSoulPatch], [t4].[Note], [t4].[Nickname], [t4].[SquadId], [o.Reports0].[Rank], [o.Reports0].[Nickname] AS [Nickname0], [o.Reports0].[SquadId] AS [SquadId0], [o.Reports0].[FullName]
    FROM [Gears] AS [o.Reports0]
    INNER JOIN (
        SELECT [o1].[HasSoulPatch], [o.Tag1].[Note], [o1].[Nickname], [o1].[SquadId]
        FROM [Gears] AS [o1]
        LEFT JOIN [Tags] AS [o.Tag1] ON ([o1].[Nickname] = [o.Tag1].[GearNickName]) AND ([o1].[SquadId] = [o.Tag1].[GearSquadId])
        LEFT JOIN (
            SELECT [o.Tag.Gear1].*
            FROM [Gears] AS [o.Tag.Gear1]
            WHERE [o.Tag.Gear1].[Discriminator] IN (N'Officer', N'Gear')
        ) AS [t3] ON ([o.Tag1].[GearNickName] = [t3].[Nickname]) AND ([o.Tag1].[GearSquadId] = [t3].[SquadId])
        WHERE ([o1].[Discriminator] = N'Officer') AND EXISTS (
            SELECT 1
            FROM [Gears] AS [g1]
            WHERE [g1].[Discriminator] IN (N'Officer', N'Gear') AND (([o1].[Nickname] = [g1].[LeaderNickname]) AND ([o1].[SquadId] = [g1].[LeaderSquadId])))
    ) AS [t4] ON ([o.Reports0].[LeaderNickname] = [t4].[Nickname]) AND ([o.Reports0].[LeaderSquadId] = [t4].[SquadId])
    WHERE [o.Reports0].[Discriminator] IN (N'Officer', N'Gear') AND ([o.Reports0].[FullName] <> N'Foo')
) AS [t5] ON [o.Reports.Weapons].[OwnerFullName] = [t5].[FullName]
WHERE ([o.Reports.Weapons].[Name] <> N'Bar') OR [o.Reports.Weapons].[Name] IS NULL
ORDER BY [t5].[HasSoulPatch] DESC, [t5].[Note], [t5].[Nickname], [t5].[SquadId], [t5].[Rank], [t5].[Nickname0], [t5].[SquadId0], [t5].[FullName], [o.Reports.Weapons].[IsAutomatic], [o.Reports.Weapons].[Id], [t2].[FullName], [w.Owner.Squad].[Id]",
                  //
                  @"SELECT [t10].[HasSoulPatch], [t10].[Note], [t10].[Nickname], [t10].[SquadId], [t10].[Rank], [t10].[Nickname0], [t10].[SquadId0], [t10].[FullName], [t10].[IsAutomatic], [t10].[Id], [t10].[FullName0], [w.Owner.Weapons].[Name], [w.Owner.Weapons].[IsAutomatic] AS [IsAutomatic0], [w.Owner.Weapons].[OwnerFullName]
FROM [Weapons] AS [w.Owner.Weapons]
INNER JOIN (
    SELECT [t9].[HasSoulPatch], [t9].[Note], [t9].[Nickname], [t9].[SquadId], [t9].[Rank], [t9].[Nickname0], [t9].[SquadId0], [t9].[FullName], [o.Reports.Weapons0].[IsAutomatic], [o.Reports.Weapons0].[Id], [t6].[FullName] AS [FullName0]
    FROM [Weapons] AS [o.Reports.Weapons0]
    LEFT JOIN (
        SELECT [w.Owner0].*
        FROM [Gears] AS [w.Owner0]
        WHERE [w.Owner0].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t6] ON [o.Reports.Weapons0].[OwnerFullName] = [t6].[FullName]
    LEFT JOIN [Squads] AS [w.Owner.Squad0] ON [t6].[SquadId] = [w.Owner.Squad0].[Id]
    INNER JOIN (
        SELECT [t8].[HasSoulPatch], [t8].[Note], [t8].[Nickname], [t8].[SquadId], [o.Reports1].[Rank], [o.Reports1].[Nickname] AS [Nickname0], [o.Reports1].[SquadId] AS [SquadId0], [o.Reports1].[FullName]
        FROM [Gears] AS [o.Reports1]
        INNER JOIN (
            SELECT [o2].[HasSoulPatch], [o.Tag2].[Note], [o2].[Nickname], [o2].[SquadId]
            FROM [Gears] AS [o2]
            LEFT JOIN [Tags] AS [o.Tag2] ON ([o2].[Nickname] = [o.Tag2].[GearNickName]) AND ([o2].[SquadId] = [o.Tag2].[GearSquadId])
            LEFT JOIN (
                SELECT [o.Tag.Gear2].*
                FROM [Gears] AS [o.Tag.Gear2]
                WHERE [o.Tag.Gear2].[Discriminator] IN (N'Officer', N'Gear')
            ) AS [t7] ON ([o.Tag2].[GearNickName] = [t7].[Nickname]) AND ([o.Tag2].[GearSquadId] = [t7].[SquadId])
            WHERE ([o2].[Discriminator] = N'Officer') AND EXISTS (
                SELECT 1
                FROM [Gears] AS [g2]
                WHERE [g2].[Discriminator] IN (N'Officer', N'Gear') AND (([o2].[Nickname] = [g2].[LeaderNickname]) AND ([o2].[SquadId] = [g2].[LeaderSquadId])))
        ) AS [t8] ON ([o.Reports1].[LeaderNickname] = [t8].[Nickname]) AND ([o.Reports1].[LeaderSquadId] = [t8].[SquadId])
        WHERE [o.Reports1].[Discriminator] IN (N'Officer', N'Gear') AND ([o.Reports1].[FullName] <> N'Foo')
    ) AS [t9] ON [o.Reports.Weapons0].[OwnerFullName] = [t9].[FullName]
    WHERE ([o.Reports.Weapons0].[Name] <> N'Bar') OR [o.Reports.Weapons0].[Name] IS NULL
) AS [t10] ON [w.Owner.Weapons].[OwnerFullName] = [t10].[FullName0]
ORDER BY [t10].[HasSoulPatch] DESC, [t10].[Note], [t10].[Nickname], [t10].[SquadId], [t10].[Rank], [t10].[Nickname0], [t10].[SquadId0], [t10].[FullName], [t10].[IsAutomatic], [t10].[Id], [t10].[FullName0]",
                  //
                  @"SELECT [t15].[HasSoulPatch], [t15].[Note], [t15].[Nickname], [t15].[SquadId], [t15].[Rank], [t15].[Nickname0], [t15].[SquadId0], [t15].[FullName], [t15].[IsAutomatic], [t15].[Id], [t15].[Id0], [w.Owner.Squad.Members].[Nickname] AS [Nickname1], [w.Owner.Squad.Members].[HasSoulPatch] AS [HasSoulPatch0], [w.Owner.Squad.Members].[SquadId]
FROM [Gears] AS [w.Owner.Squad.Members]
INNER JOIN (
    SELECT [t14].[HasSoulPatch], [t14].[Note], [t14].[Nickname], [t14].[SquadId], [t14].[Rank], [t14].[Nickname0], [t14].[SquadId0], [t14].[FullName], [o.Reports.Weapons1].[IsAutomatic], [o.Reports.Weapons1].[Id], [w.Owner.Squad1].[Id] AS [Id0]
    FROM [Weapons] AS [o.Reports.Weapons1]
    LEFT JOIN (
        SELECT [w.Owner1].*
        FROM [Gears] AS [w.Owner1]
        WHERE [w.Owner1].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t11] ON [o.Reports.Weapons1].[OwnerFullName] = [t11].[FullName]
    LEFT JOIN [Squads] AS [w.Owner.Squad1] ON [t11].[SquadId] = [w.Owner.Squad1].[Id]
    INNER JOIN (
        SELECT [t13].[HasSoulPatch], [t13].[Note], [t13].[Nickname], [t13].[SquadId], [o.Reports2].[Rank], [o.Reports2].[Nickname] AS [Nickname0], [o.Reports2].[SquadId] AS [SquadId0], [o.Reports2].[FullName]
        FROM [Gears] AS [o.Reports2]
        INNER JOIN (
            SELECT [o3].[HasSoulPatch], [o.Tag3].[Note], [o3].[Nickname], [o3].[SquadId]
            FROM [Gears] AS [o3]
            LEFT JOIN [Tags] AS [o.Tag3] ON ([o3].[Nickname] = [o.Tag3].[GearNickName]) AND ([o3].[SquadId] = [o.Tag3].[GearSquadId])
            LEFT JOIN (
                SELECT [o.Tag.Gear3].*
                FROM [Gears] AS [o.Tag.Gear3]
                WHERE [o.Tag.Gear3].[Discriminator] IN (N'Officer', N'Gear')
            ) AS [t12] ON ([o.Tag3].[GearNickName] = [t12].[Nickname]) AND ([o.Tag3].[GearSquadId] = [t12].[SquadId])
            WHERE ([o3].[Discriminator] = N'Officer') AND EXISTS (
                SELECT 1
                FROM [Gears] AS [g3]
                WHERE [g3].[Discriminator] IN (N'Officer', N'Gear') AND (([o3].[Nickname] = [g3].[LeaderNickname]) AND ([o3].[SquadId] = [g3].[LeaderSquadId])))
        ) AS [t13] ON ([o.Reports2].[LeaderNickname] = [t13].[Nickname]) AND ([o.Reports2].[LeaderSquadId] = [t13].[SquadId])
        WHERE [o.Reports2].[Discriminator] IN (N'Officer', N'Gear') AND ([o.Reports2].[FullName] <> N'Foo')
    ) AS [t14] ON [o.Reports.Weapons1].[OwnerFullName] = [t14].[FullName]
    WHERE ([o.Reports.Weapons1].[Name] <> N'Bar') OR [o.Reports.Weapons1].[Name] IS NULL
) AS [t15] ON [w.Owner.Squad.Members].[SquadId] = [t15].[Id0]
WHERE [w.Owner.Squad.Members].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t15].[HasSoulPatch] DESC, [t15].[Note], [t15].[Nickname], [t15].[SquadId], [t15].[Rank], [t15].[Nickname0], [t15].[SquadId0], [t15].[FullName], [t15].[IsAutomatic], [t15].[Id], [t15].[Id0], [Nickname1]",
                  //
                  @"SELECT [o.Tag.Gear.Weapons].[Id], [o.Tag.Gear.Weapons].[AmmunitionType], [o.Tag.Gear.Weapons].[IsAutomatic], [o.Tag.Gear.Weapons].[Name], [o.Tag.Gear.Weapons].[OwnerFullName], [o.Tag.Gear.Weapons].[SynergyWithId], [t18].[HasSoulPatch], [t18].[Note], [t18].[Nickname], [t18].[SquadId], [t18].[FullName]
FROM [Weapons] AS [o.Tag.Gear.Weapons]
LEFT JOIN (
    SELECT [www.Owner].*
    FROM [Gears] AS [www.Owner]
    WHERE [www.Owner].[Discriminator] IN (N'Officer', N'Gear')
) AS [t16] ON [o.Tag.Gear.Weapons].[OwnerFullName] = [t16].[FullName]
INNER JOIN (
    SELECT [o4].[HasSoulPatch], [o.Tag4].[Note], [o4].[Nickname], [o4].[SquadId], [t17].[FullName]
    FROM [Gears] AS [o4]
    LEFT JOIN [Tags] AS [o.Tag4] ON ([o4].[Nickname] = [o.Tag4].[GearNickName]) AND ([o4].[SquadId] = [o.Tag4].[GearSquadId])
    LEFT JOIN (
        SELECT [o.Tag.Gear4].*
        FROM [Gears] AS [o.Tag.Gear4]
        WHERE [o.Tag.Gear4].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t17] ON ([o.Tag4].[GearNickName] = [t17].[Nickname]) AND ([o.Tag4].[GearSquadId] = [t17].[SquadId])
    WHERE ([o4].[Discriminator] = N'Officer') AND EXISTS (
        SELECT 1
        FROM [Gears] AS [g4]
        WHERE [g4].[Discriminator] IN (N'Officer', N'Gear') AND (([o4].[Nickname] = [g4].[LeaderNickname]) AND ([o4].[SquadId] = [g4].[LeaderSquadId])))
) AS [t18] ON [o.Tag.Gear.Weapons].[OwnerFullName] = [t18].[FullName]
ORDER BY [t18].[HasSoulPatch] DESC, [t18].[Note], [t18].[Nickname], [t18].[SquadId], [t18].[FullName], [o.Tag.Gear.Weapons].[IsAutomatic], [t16].[Nickname] DESC");
        }

        public override async Task Correlated_collections_inner_subquery_selector_references_outer_qsre()
        {
            await base.Correlated_collections_inner_subquery_selector_references_outer_qsre();

            AssertSql(
                @"SELECT [o].[FullName], [o].[Nickname], [o].[SquadId]
FROM [Gears] AS [o]
WHERE [o].[Discriminator] = N'Officer'",
                //
                @"@_outer_FullName='Damon Baird' (Size = 4000)
@_outer_Nickname='Baird' (Size = 450)
@_outer_SquadId='1'

SELECT [r].[FullName] AS [ReportName], @_outer_FullName AS [OfficerName]
FROM [Gears] AS [r]
WHERE [r].[Discriminator] IN (N'Officer', N'Gear') AND ((@_outer_Nickname = [r].[LeaderNickname]) AND (@_outer_SquadId = [r].[LeaderSquadId]))",
                //
                @"@_outer_FullName='Marcus Fenix' (Size = 4000)
@_outer_Nickname='Marcus' (Size = 450)
@_outer_SquadId='1'

SELECT [r].[FullName] AS [ReportName], @_outer_FullName AS [OfficerName]
FROM [Gears] AS [r]
WHERE [r].[Discriminator] IN (N'Officer', N'Gear') AND ((@_outer_Nickname = [r].[LeaderNickname]) AND (@_outer_SquadId = [r].[LeaderSquadId]))");
        }

        public override async Task Correlated_collections_inner_subquery_predicate_references_outer_qsre()
        {
            await base.Correlated_collections_inner_subquery_predicate_references_outer_qsre();

            AssertSql(
                @"SELECT [o].[FullName], [o].[Nickname], [o].[SquadId]
FROM [Gears] AS [o]
WHERE [o].[Discriminator] = N'Officer'",
                //
                @"@_outer_FullName='Damon Baird' (Size = 4000)
@_outer_Nickname='Baird' (Size = 450)
@_outer_SquadId='1'

SELECT [r].[FullName] AS [ReportName]
FROM [Gears] AS [r]
WHERE ([r].[Discriminator] IN (N'Officer', N'Gear') AND (@_outer_FullName <> N'Foo')) AND ((@_outer_Nickname = [r].[LeaderNickname]) AND (@_outer_SquadId = [r].[LeaderSquadId]))",
                //
                @"@_outer_FullName='Marcus Fenix' (Size = 4000)
@_outer_Nickname='Marcus' (Size = 450)
@_outer_SquadId='1'

SELECT [r].[FullName] AS [ReportName]
FROM [Gears] AS [r]
WHERE ([r].[Discriminator] IN (N'Officer', N'Gear') AND (@_outer_FullName <> N'Foo')) AND ((@_outer_Nickname = [r].[LeaderNickname]) AND (@_outer_SquadId = [r].[LeaderSquadId]))");
        }

        public override async Task Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up()
        {
            await base.Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up();

            AssertSql(
                @"SELECT [o].[FullName], [o].[Nickname], [o].[SquadId]
FROM [Gears] AS [o]
WHERE [o].[Discriminator] = N'Officer'
ORDER BY [o].[Nickname], [o].[SquadId]",
                //
                @"SELECT [t].[Nickname], [t].[SquadId], [o.Reports].[FullName], [o.Reports].[Nickname], [o.Reports].[LeaderNickname], [o.Reports].[LeaderSquadId]
FROM [Gears] AS [o.Reports]
INNER JOIN (
    SELECT [o0].[Nickname], [o0].[SquadId]
    FROM [Gears] AS [o0]
    WHERE [o0].[Discriminator] = N'Officer'
) AS [t] ON ([o.Reports].[LeaderNickname] = [t].[Nickname]) AND ([o.Reports].[LeaderSquadId] = [t].[SquadId])
WHERE [o.Reports].[Discriminator] IN (N'Officer', N'Gear') AND ([o.Reports].[FullName] <> N'Foo')
ORDER BY [t].[Nickname], [t].[SquadId]",
                //
                @"@_outer_Nickname='Paduk' (Size = 4000)
@_outer_FullName='Garron Paduk' (Size = 450)

SELECT [w].[Name], @_outer_Nickname AS [Nickname]
FROM [Weapons] AS [w]
WHERE (([w].[Name] <> N'Bar') OR [w].[Name] IS NULL) AND (@_outer_FullName = [w].[OwnerFullName])",
                //
                @"@_outer_Nickname='Baird' (Size = 4000)
@_outer_FullName='Damon Baird' (Size = 450)

SELECT [w].[Name], @_outer_Nickname AS [Nickname]
FROM [Weapons] AS [w]
WHERE (([w].[Name] <> N'Bar') OR [w].[Name] IS NULL) AND (@_outer_FullName = [w].[OwnerFullName])",
                //
                @"@_outer_Nickname='Cole Train' (Size = 4000)
@_outer_FullName='Augustus Cole' (Size = 450)

SELECT [w].[Name], @_outer_Nickname AS [Nickname]
FROM [Weapons] AS [w]
WHERE (([w].[Name] <> N'Bar') OR [w].[Name] IS NULL) AND (@_outer_FullName = [w].[OwnerFullName])",
                //
                @"@_outer_Nickname='Dom' (Size = 4000)
@_outer_FullName='Dominic Santiago' (Size = 450)

SELECT [w].[Name], @_outer_Nickname AS [Nickname]
FROM [Weapons] AS [w]
WHERE (([w].[Name] <> N'Bar') OR [w].[Name] IS NULL) AND (@_outer_FullName = [w].[OwnerFullName])");
        }

        public override async Task Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up()
        {
            await base.Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up();

            AssertSql(
                @"SELECT [o].[FullName], [o].[Nickname], [o].[SquadId]
FROM [Gears] AS [o]
WHERE [o].[Discriminator] = N'Officer'",
                //
                @"@_outer_Nickname='Baird' (Size = 450)
@_outer_SquadId='1'

SELECT [r].[FullName]
FROM [Gears] AS [r]
WHERE ([r].[Discriminator] IN (N'Officer', N'Gear') AND ([r].[FullName] <> N'Foo')) AND ((@_outer_Nickname = [r].[LeaderNickname]) AND (@_outer_SquadId = [r].[LeaderSquadId]))",
                //
                @"@_outer_Nickname1='Baird' (Size = 4000)
@_outer_FullName='Garron Paduk' (Size = 450)

SELECT [w].[Name], @_outer_Nickname1 AS [Nickname]
FROM [Weapons] AS [w]
WHERE (([w].[Name] <> N'Bar') OR [w].[Name] IS NULL) AND (@_outer_FullName = [w].[OwnerFullName])",
                //
                @"@_outer_Nickname='Marcus' (Size = 450)
@_outer_SquadId='1'

SELECT [r].[FullName]
FROM [Gears] AS [r]
WHERE ([r].[Discriminator] IN (N'Officer', N'Gear') AND ([r].[FullName] <> N'Foo')) AND ((@_outer_Nickname = [r].[LeaderNickname]) AND (@_outer_SquadId = [r].[LeaderSquadId]))",
                //
                @"@_outer_Nickname1='Marcus' (Size = 4000)
@_outer_FullName='Augustus Cole' (Size = 450)

SELECT [w].[Name], @_outer_Nickname1 AS [Nickname]
FROM [Weapons] AS [w]
WHERE (([w].[Name] <> N'Bar') OR [w].[Name] IS NULL) AND (@_outer_FullName = [w].[OwnerFullName])",
                //
                @"@_outer_Nickname1='Marcus' (Size = 4000)
@_outer_FullName='Damon Baird' (Size = 450)

SELECT [w].[Name], @_outer_Nickname1 AS [Nickname]
FROM [Weapons] AS [w]
WHERE (([w].[Name] <> N'Bar') OR [w].[Name] IS NULL) AND (@_outer_FullName = [w].[OwnerFullName])",
                //
                @"@_outer_Nickname1='Marcus' (Size = 4000)
@_outer_FullName='Dominic Santiago' (Size = 450)

SELECT [w].[Name], @_outer_Nickname1 AS [Nickname]
FROM [Weapons] AS [w]
WHERE (([w].[Name] <> N'Bar') OR [w].[Name] IS NULL) AND (@_outer_FullName = [w].[OwnerFullName])");
        }

        public override async Task Correlated_collections_on_select_many()
        {
            await base.Correlated_collections_on_select_many();

            AssertSql(
                @"SELECT [g].[Nickname] AS [GearNickname], [s].[Name] AS [SquadName], [g].[FullName], [s].[Id]
FROM [Gears] AS [g]
CROSS JOIN [Squads] AS [s]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)
ORDER BY [GearNickname], [s].[Id] DESC",
                //
                @"@_outer_FullName='Damon Baird' (Size = 450)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE (([w].[IsAutomatic] = 1) OR (([w].[Name] <> N'foo') OR [w].[Name] IS NULL)) AND (@_outer_FullName = [w].[OwnerFullName])",
                //
                @"@_outer_Id='2'

SELECT [m].[Nickname], [m].[SquadId], [m].[AssignedCityName], [m].[CityOrBirthName], [m].[Discriminator], [m].[FullName], [m].[HasSoulPatch], [m].[LeaderNickname], [m].[LeaderSquadId], [m].[Rank]
FROM [Gears] AS [m]
WHERE ([m].[Discriminator] IN (N'Officer', N'Gear') AND ([m].[HasSoulPatch] = 0)) AND (@_outer_Id = [m].[SquadId])",
                //
                @"@_outer_FullName='Damon Baird' (Size = 450)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE (([w].[IsAutomatic] = 1) OR (([w].[Name] <> N'foo') OR [w].[Name] IS NULL)) AND (@_outer_FullName = [w].[OwnerFullName])",
                //
                @"@_outer_Id='1'

SELECT [m].[Nickname], [m].[SquadId], [m].[AssignedCityName], [m].[CityOrBirthName], [m].[Discriminator], [m].[FullName], [m].[HasSoulPatch], [m].[LeaderNickname], [m].[LeaderSquadId], [m].[Rank]
FROM [Gears] AS [m]
WHERE ([m].[Discriminator] IN (N'Officer', N'Gear') AND ([m].[HasSoulPatch] = 0)) AND (@_outer_Id = [m].[SquadId])",
                //
                @"@_outer_FullName='Marcus Fenix' (Size = 450)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE (([w].[IsAutomatic] = 1) OR (([w].[Name] <> N'foo') OR [w].[Name] IS NULL)) AND (@_outer_FullName = [w].[OwnerFullName])",
                //
                @"@_outer_Id='2'

SELECT [m].[Nickname], [m].[SquadId], [m].[AssignedCityName], [m].[CityOrBirthName], [m].[Discriminator], [m].[FullName], [m].[HasSoulPatch], [m].[LeaderNickname], [m].[LeaderSquadId], [m].[Rank]
FROM [Gears] AS [m]
WHERE ([m].[Discriminator] IN (N'Officer', N'Gear') AND ([m].[HasSoulPatch] = 0)) AND (@_outer_Id = [m].[SquadId])",
                //
                @"@_outer_FullName='Marcus Fenix' (Size = 450)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE (([w].[IsAutomatic] = 1) OR (([w].[Name] <> N'foo') OR [w].[Name] IS NULL)) AND (@_outer_FullName = [w].[OwnerFullName])",
                //
                @"@_outer_Id='1'

SELECT [m].[Nickname], [m].[SquadId], [m].[AssignedCityName], [m].[CityOrBirthName], [m].[Discriminator], [m].[FullName], [m].[HasSoulPatch], [m].[LeaderNickname], [m].[LeaderSquadId], [m].[Rank]
FROM [Gears] AS [m]
WHERE ([m].[Discriminator] IN (N'Officer', N'Gear') AND ([m].[HasSoulPatch] = 0)) AND (@_outer_Id = [m].[SquadId])");
        }

        public override async Task Correlated_collections_with_Skip()
        {
            await base.Correlated_collections_with_Skip();

            AssertSql(
                @"SELECT [s].[Id]
FROM [Squads] AS [s]
ORDER BY [s].[Name]",
                //
                @"@_outer_Id='1'

SELECT [m].[Nickname], [m].[SquadId], [m].[AssignedCityName], [m].[CityOrBirthName], [m].[Discriminator], [m].[FullName], [m].[HasSoulPatch], [m].[LeaderNickname], [m].[LeaderSquadId], [m].[Rank]
FROM [Gears] AS [m]
WHERE [m].[Discriminator] IN (N'Officer', N'Gear') AND (@_outer_Id = [m].[SquadId])
ORDER BY [m].[Nickname]
OFFSET 1 ROWS",
                //
                @"@_outer_Id='2'

SELECT [m].[Nickname], [m].[SquadId], [m].[AssignedCityName], [m].[CityOrBirthName], [m].[Discriminator], [m].[FullName], [m].[HasSoulPatch], [m].[LeaderNickname], [m].[LeaderSquadId], [m].[Rank]
FROM [Gears] AS [m]
WHERE [m].[Discriminator] IN (N'Officer', N'Gear') AND (@_outer_Id = [m].[SquadId])
ORDER BY [m].[Nickname]
OFFSET 1 ROWS");
        }

        public override async Task Correlated_collections_with_Take()
        {
            await base.Correlated_collections_with_Take();

            AssertSql(
                @"SELECT [s].[Id]
FROM [Squads] AS [s]
ORDER BY [s].[Name]",
                //
                @"@_outer_Id='1'

SELECT TOP(2) [m].[Nickname], [m].[SquadId], [m].[AssignedCityName], [m].[CityOrBirthName], [m].[Discriminator], [m].[FullName], [m].[HasSoulPatch], [m].[LeaderNickname], [m].[LeaderSquadId], [m].[Rank]
FROM [Gears] AS [m]
WHERE [m].[Discriminator] IN (N'Officer', N'Gear') AND (@_outer_Id = [m].[SquadId])
ORDER BY [m].[Nickname]",
                //
                @"@_outer_Id='2'

SELECT TOP(2) [m].[Nickname], [m].[SquadId], [m].[AssignedCityName], [m].[CityOrBirthName], [m].[Discriminator], [m].[FullName], [m].[HasSoulPatch], [m].[LeaderNickname], [m].[LeaderSquadId], [m].[Rank]
FROM [Gears] AS [m]
WHERE [m].[Discriminator] IN (N'Officer', N'Gear') AND (@_outer_Id = [m].[SquadId])
ORDER BY [m].[Nickname]");
        }

        public override async Task Correlated_collections_with_Distinct()
        {
            await base.Correlated_collections_with_Distinct();

            AssertSql(
                @"SELECT [s].[Id]
FROM [Squads] AS [s]
ORDER BY [s].[Name]",
                //
                @"@_outer_Id='1'

SELECT DISTINCT [m].[Nickname], [m].[SquadId], [m].[AssignedCityName], [m].[CityOrBirthName], [m].[Discriminator], [m].[FullName], [m].[HasSoulPatch], [m].[LeaderNickname], [m].[LeaderSquadId], [m].[Rank]
FROM [Gears] AS [m]
WHERE [m].[Discriminator] IN (N'Officer', N'Gear') AND (@_outer_Id = [m].[SquadId])
ORDER BY [m].[Nickname]",
                //
                @"@_outer_Id='2'

SELECT DISTINCT [m].[Nickname], [m].[SquadId], [m].[AssignedCityName], [m].[CityOrBirthName], [m].[Discriminator], [m].[FullName], [m].[HasSoulPatch], [m].[LeaderNickname], [m].[LeaderSquadId], [m].[Rank]
FROM [Gears] AS [m]
WHERE [m].[Discriminator] IN (N'Officer', N'Gear') AND (@_outer_Id = [m].[SquadId])
ORDER BY [m].[Nickname]");
        }

        public override async Task Correlated_collections_with_FirstOrDefault()
        {
            await base.Correlated_collections_with_FirstOrDefault();

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [m].[FullName]
    FROM [Gears] AS [m]
    WHERE [m].[Discriminator] IN (N'Officer', N'Gear') AND ([s].[Id] = [m].[SquadId])
    ORDER BY [m].[Nickname]
)
FROM [Squads] AS [s]
ORDER BY [s].[Name]");
        }

        public override async Task Correlated_collections_on_left_join_with_predicate()
        {
            await base.Correlated_collections_on_left_join_with_predicate();

            AssertSql(
                @"SELECT [t0].[Nickname], [t0].[FullName]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gears] AS [g]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname]
WHERE ([t0].[HasSoulPatch] <> 1) OR [t0].[HasSoulPatch] IS NULL
ORDER BY [t0].[Nickname], [t0].[SquadId], [t0].[FullName]",
                //
                @"SELECT [t3].[Nickname], [t3].[SquadId], [t3].[FullName], [g.Weapons].[Name], [g.Weapons].[OwnerFullName]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT [t2].[Nickname], [t2].[SquadId], [t2].[FullName]
    FROM [Tags] AS [t1]
    LEFT JOIN (
        SELECT [g0].*
        FROM [Gears] AS [g0]
        WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t2] ON [t1].[GearNickName] = [t2].[Nickname]
    WHERE ([t2].[HasSoulPatch] <> 1) OR [t2].[HasSoulPatch] IS NULL
) AS [t3] ON [g.Weapons].[OwnerFullName] = [t3].[FullName]
ORDER BY [t3].[Nickname], [t3].[SquadId], [t3].[FullName]");
        }

        public override async Task Correlated_collections_on_left_join_with_null_value()
        {
            await base.Correlated_collections_on_left_join_with_null_value();

            AssertSql(
                @"SELECT [t0].[FullName]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gears] AS [g]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname]
ORDER BY [t].[Note], [t0].[Nickname], [t0].[SquadId], [t0].[FullName]",
                //
                @"SELECT [t3].[Note], [t3].[Nickname], [t3].[SquadId], [t3].[FullName], [g.Weapons].[Name], [g.Weapons].[OwnerFullName]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT [t1].[Note], [t2].[Nickname], [t2].[SquadId], [t2].[FullName]
    FROM [Tags] AS [t1]
    LEFT JOIN (
        SELECT [g0].*
        FROM [Gears] AS [g0]
        WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t2] ON [t1].[GearNickName] = [t2].[Nickname]
) AS [t3] ON [g.Weapons].[OwnerFullName] = [t3].[FullName]
ORDER BY [t3].[Note], [t3].[Nickname], [t3].[SquadId], [t3].[FullName]");
        }

        public override async Task Correlated_collections_left_join_with_self_reference()
        {
            await base.Correlated_collections_left_join_with_self_reference();

            AssertSql(
                @"SELECT [t].[Note], [t0].[Nickname], [t0].[SquadId]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [o].*
    FROM [Gears] AS [o]
    WHERE [o].[Discriminator] = N'Officer'
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname]
ORDER BY [t0].[Nickname], [t0].[SquadId]",
                //
                @"SELECT [t3].[Nickname], [t3].[SquadId], [o.Reports].[FullName], [o.Reports].[LeaderNickname], [o.Reports].[LeaderSquadId]
FROM [Gears] AS [o.Reports]
INNER JOIN (
    SELECT [t2].[Nickname], [t2].[SquadId]
    FROM [Tags] AS [t1]
    LEFT JOIN (
        SELECT [o0].*
        FROM [Gears] AS [o0]
        WHERE [o0].[Discriminator] = N'Officer'
    ) AS [t2] ON [t1].[GearNickName] = [t2].[Nickname]
) AS [t3] ON ([o.Reports].[LeaderNickname] = [t3].[Nickname]) AND ([o.Reports].[LeaderSquadId] = [t3].[SquadId])
WHERE [o.Reports].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t3].[Nickname], [t3].[SquadId]");
        }

        public override async Task Correlated_collections_deeply_nested_left_join()
        {
            await base.Correlated_collections_deeply_nested_left_join();

            AssertSql(
                @"SELECT [g.Squad].[Id]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gears] AS [g]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname]
LEFT JOIN [Squads] AS [g.Squad] ON [t0].[SquadId] = [g.Squad].[Id]
ORDER BY [t].[Note], [t0].[Nickname] DESC, [t0].[SquadId], [g.Squad].[Id]",
                //
                @"SELECT [t3].[Note], [t3].[Nickname], [t3].[SquadId], [t3].[Id], [g.Squad.Members].[Nickname] AS [Nickname0], [g.Squad.Members].[FullName], [g.Squad.Members].[SquadId]
FROM [Gears] AS [g.Squad.Members]
INNER JOIN (
    SELECT [t1].[Note], [t2].[Nickname], [t2].[SquadId], [g.Squad0].[Id]
    FROM [Tags] AS [t1]
    LEFT JOIN (
        SELECT [g0].*
        FROM [Gears] AS [g0]
        WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t2] ON [t1].[GearNickName] = [t2].[Nickname]
    LEFT JOIN [Squads] AS [g.Squad0] ON [t2].[SquadId] = [g.Squad0].[Id]
) AS [t3] ON [g.Squad.Members].[SquadId] = [t3].[Id]
WHERE [g.Squad.Members].[Discriminator] IN (N'Officer', N'Gear') AND ([g.Squad.Members].[HasSoulPatch] = 1)
ORDER BY [t3].[Note], [t3].[Nickname] DESC, [t3].[SquadId], [t3].[Id], [g.Squad.Members].[Nickname], [g.Squad.Members].[SquadId], [g.Squad.Members].[FullName]",
                //
                @"SELECT [g.Squad.Members.Weapons].[Id], [g.Squad.Members.Weapons].[AmmunitionType], [g.Squad.Members.Weapons].[IsAutomatic], [g.Squad.Members.Weapons].[Name], [g.Squad.Members.Weapons].[OwnerFullName], [g.Squad.Members.Weapons].[SynergyWithId], [t7].[Note], [t7].[Nickname], [t7].[SquadId], [t7].[Id], [t7].[Nickname0], [t7].[SquadId0], [t7].[FullName]
FROM [Weapons] AS [g.Squad.Members.Weapons]
INNER JOIN (
    SELECT [t6].[Note], [t6].[Nickname], [t6].[SquadId], [t6].[Id], [g.Squad.Members0].[Nickname] AS [Nickname0], [g.Squad.Members0].[SquadId] AS [SquadId0], [g.Squad.Members0].[FullName]
    FROM [Gears] AS [g.Squad.Members0]
    INNER JOIN (
        SELECT [t4].[Note], [t5].[Nickname], [t5].[SquadId], [g.Squad1].[Id]
        FROM [Tags] AS [t4]
        LEFT JOIN (
            SELECT [g1].*
            FROM [Gears] AS [g1]
            WHERE [g1].[Discriminator] IN (N'Officer', N'Gear')
        ) AS [t5] ON [t4].[GearNickName] = [t5].[Nickname]
        LEFT JOIN [Squads] AS [g.Squad1] ON [t5].[SquadId] = [g.Squad1].[Id]
    ) AS [t6] ON [g.Squad.Members0].[SquadId] = [t6].[Id]
    WHERE [g.Squad.Members0].[Discriminator] IN (N'Officer', N'Gear') AND ([g.Squad.Members0].[HasSoulPatch] = 1)
) AS [t7] ON [g.Squad.Members.Weapons].[OwnerFullName] = [t7].[FullName]
WHERE [g.Squad.Members.Weapons].[IsAutomatic] = 1
ORDER BY [t7].[Note], [t7].[Nickname] DESC, [t7].[SquadId], [t7].[Id], [t7].[Nickname0], [t7].[SquadId0], [t7].[FullName]");
        }

        public override async Task Correlated_collections_from_left_join_with_additional_elements_projected_of_that_join()
        {
            await base.Correlated_collections_from_left_join_with_additional_elements_projected_of_that_join();

            AssertSql(
                @"SELECT [w.Owner.Squad].[Id]
FROM [Weapons] AS [w]
LEFT JOIN (
    SELECT [w.Owner].*
    FROM [Gears] AS [w.Owner]
    WHERE [w.Owner].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [w].[OwnerFullName] = [t].[FullName]
LEFT JOIN [Squads] AS [w.Owner.Squad] ON [t].[SquadId] = [w.Owner.Squad].[Id]
ORDER BY [w].[Name], [w].[Id], [w.Owner.Squad].[Id]",
                //
                @"SELECT [t1].[Name], [t1].[Id], [t1].[Id0], [w.Owner.Squad.Members].[FullName], [w.Owner.Squad.Members].[Rank], [w.Owner.Squad.Members].[SquadId]
FROM [Gears] AS [w.Owner.Squad.Members]
INNER JOIN (
    SELECT [w0].[Name], [w0].[Id], [w.Owner.Squad0].[Id] AS [Id0]
    FROM [Weapons] AS [w0]
    LEFT JOIN (
        SELECT [w.Owner0].*
        FROM [Gears] AS [w.Owner0]
        WHERE [w.Owner0].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t0] ON [w0].[OwnerFullName] = [t0].[FullName]
    LEFT JOIN [Squads] AS [w.Owner.Squad0] ON [t0].[SquadId] = [w.Owner.Squad0].[Id]
) AS [t1] ON [w.Owner.Squad.Members].[SquadId] = [t1].[Id0]
WHERE [w.Owner.Squad.Members].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t1].[Name], [t1].[Id], [t1].[Id0], [w.Owner.Squad.Members].[FullName] DESC, [w.Owner.Squad.Members].[Nickname], [w.Owner.Squad.Members].[SquadId]",
                //
                @"SELECT [w.Owner.Squad.Members.Weapons].[Id], [w.Owner.Squad.Members.Weapons].[AmmunitionType], [w.Owner.Squad.Members.Weapons].[IsAutomatic], [w.Owner.Squad.Members.Weapons].[Name], [w.Owner.Squad.Members.Weapons].[OwnerFullName], [w.Owner.Squad.Members.Weapons].[SynergyWithId], [t4].[Name], [t4].[Id], [t4].[Id0], [t4].[FullName], [t4].[Nickname], [t4].[SquadId]
FROM [Weapons] AS [w.Owner.Squad.Members.Weapons]
INNER JOIN (
    SELECT [t3].[Name], [t3].[Id], [t3].[Id0], [w.Owner.Squad.Members0].[FullName], [w.Owner.Squad.Members0].[Nickname], [w.Owner.Squad.Members0].[SquadId]
    FROM [Gears] AS [w.Owner.Squad.Members0]
    INNER JOIN (
        SELECT [w1].[Name], [w1].[Id], [w.Owner.Squad1].[Id] AS [Id0]
        FROM [Weapons] AS [w1]
        LEFT JOIN (
            SELECT [w.Owner1].*
            FROM [Gears] AS [w.Owner1]
            WHERE [w.Owner1].[Discriminator] IN (N'Officer', N'Gear')
        ) AS [t2] ON [w1].[OwnerFullName] = [t2].[FullName]
        LEFT JOIN [Squads] AS [w.Owner.Squad1] ON [t2].[SquadId] = [w.Owner.Squad1].[Id]
    ) AS [t3] ON [w.Owner.Squad.Members0].[SquadId] = [t3].[Id0]
    WHERE [w.Owner.Squad.Members0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t4] ON [w.Owner.Squad.Members.Weapons].[OwnerFullName] = [t4].[FullName]
WHERE [w.Owner.Squad.Members.Weapons].[IsAutomatic] = 0
ORDER BY [t4].[Name], [t4].[Id], [t4].[Id0], [t4].[FullName] DESC, [t4].[Nickname], [t4].[SquadId], [w.Owner.Squad.Members.Weapons].[Id]");
        }

        public override async Task Correlated_collections_complex_scenario1()
        {
            await base.Correlated_collections_complex_scenario1();

            AssertSql(
                @"SELECT [r].[FullName]
FROM [Gears] AS [r]
WHERE [r].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [r].[Nickname], [r].[SquadId], [r].[FullName]",
                //
                @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[FullName], [r.Weapons].[Id] AS [Id0], [w.Owner.Squad].[Id], [r.Weapons].[OwnerFullName]
FROM [Weapons] AS [r.Weapons]
LEFT JOIN (
    SELECT [w.Owner].*
    FROM [Gears] AS [w.Owner]
    WHERE [w.Owner].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [r.Weapons].[OwnerFullName] = [t].[FullName]
LEFT JOIN [Squads] AS [w.Owner.Squad] ON [t].[SquadId] = [w.Owner.Squad].[Id]
INNER JOIN (
    SELECT [r0].[Nickname], [r0].[SquadId], [r0].[FullName]
    FROM [Gears] AS [r0]
    WHERE [r0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON [r.Weapons].[OwnerFullName] = [t0].[FullName]
ORDER BY [t0].[Nickname], [t0].[SquadId], [t0].[FullName], [r.Weapons].[Id], [w.Owner.Squad].[Id]",
                //
                @"SELECT [t3].[Nickname], [t3].[SquadId], [t3].[FullName], [t3].[Id], [t3].[Id0], [w.Owner.Squad.Members].[Nickname] AS [Nickname0], [w.Owner.Squad.Members].[HasSoulPatch], [w.Owner.Squad.Members].[SquadId]
FROM [Gears] AS [w.Owner.Squad.Members]
INNER JOIN (
    SELECT [t2].[Nickname], [t2].[SquadId], [t2].[FullName], [r.Weapons0].[Id], [w.Owner.Squad0].[Id] AS [Id0]
    FROM [Weapons] AS [r.Weapons0]
    LEFT JOIN (
        SELECT [w.Owner0].*
        FROM [Gears] AS [w.Owner0]
        WHERE [w.Owner0].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t1] ON [r.Weapons0].[OwnerFullName] = [t1].[FullName]
    LEFT JOIN [Squads] AS [w.Owner.Squad0] ON [t1].[SquadId] = [w.Owner.Squad0].[Id]
    INNER JOIN (
        SELECT [r1].[Nickname], [r1].[SquadId], [r1].[FullName]
        FROM [Gears] AS [r1]
        WHERE [r1].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t2] ON [r.Weapons0].[OwnerFullName] = [t2].[FullName]
) AS [t3] ON [w.Owner.Squad.Members].[SquadId] = [t3].[Id0]
WHERE [w.Owner.Squad.Members].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t3].[Nickname], [t3].[SquadId], [t3].[FullName], [t3].[Id], [t3].[Id0], [Nickname0]");
        }

        public override async Task Correlated_collections_complex_scenario2()
        {
            await base.Correlated_collections_complex_scenario2();

            AssertSql(
                @"SELECT [o].[FullName], [o].[Nickname], [o].[SquadId]
FROM [Gears] AS [o]
WHERE [o].[Discriminator] = N'Officer'
ORDER BY [o].[Nickname], [o].[SquadId]",
                //
                @"SELECT [t].[Nickname], [t].[SquadId], [o.Reports].[FullName], [o.Reports].[LeaderNickname], [o.Reports].[LeaderSquadId]
FROM [Gears] AS [o.Reports]
INNER JOIN (
    SELECT [o0].[Nickname], [o0].[SquadId]
    FROM [Gears] AS [o0]
    WHERE [o0].[Discriminator] = N'Officer'
) AS [t] ON ([o.Reports].[LeaderNickname] = [t].[Nickname]) AND ([o.Reports].[LeaderSquadId] = [t].[SquadId])
WHERE [o.Reports].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[Nickname], [t].[SquadId], [o.Reports].[Nickname], [o.Reports].[SquadId], [o.Reports].[FullName]",
                //
                @"SELECT [t2].[Nickname], [t2].[SquadId], [t2].[Nickname0], [t2].[SquadId0], [t2].[FullName], [o.Reports.Weapons].[Id] AS [Id0], [w.Owner.Squad].[Id], [o.Reports.Weapons].[OwnerFullName]
FROM [Weapons] AS [o.Reports.Weapons]
LEFT JOIN (
    SELECT [w.Owner].*
    FROM [Gears] AS [w.Owner]
    WHERE [w.Owner].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON [o.Reports.Weapons].[OwnerFullName] = [t0].[FullName]
LEFT JOIN [Squads] AS [w.Owner.Squad] ON [t0].[SquadId] = [w.Owner.Squad].[Id]
INNER JOIN (
    SELECT [t1].[Nickname], [t1].[SquadId], [o.Reports0].[Nickname] AS [Nickname0], [o.Reports0].[SquadId] AS [SquadId0], [o.Reports0].[FullName]
    FROM [Gears] AS [o.Reports0]
    INNER JOIN (
        SELECT [o1].[Nickname], [o1].[SquadId]
        FROM [Gears] AS [o1]
        WHERE [o1].[Discriminator] = N'Officer'
    ) AS [t1] ON ([o.Reports0].[LeaderNickname] = [t1].[Nickname]) AND ([o.Reports0].[LeaderSquadId] = [t1].[SquadId])
    WHERE [o.Reports0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t2] ON [o.Reports.Weapons].[OwnerFullName] = [t2].[FullName]
ORDER BY [t2].[Nickname], [t2].[SquadId], [t2].[Nickname0], [t2].[SquadId0], [t2].[FullName], [o.Reports.Weapons].[Id], [w.Owner.Squad].[Id]",
                //
                @"SELECT [t6].[Nickname], [t6].[SquadId], [t6].[Nickname0], [t6].[SquadId0], [t6].[FullName], [t6].[Id], [t6].[Id0], [w.Owner.Squad.Members].[Nickname] AS [Nickname1], [w.Owner.Squad.Members].[HasSoulPatch], [w.Owner.Squad.Members].[SquadId]
FROM [Gears] AS [w.Owner.Squad.Members]
INNER JOIN (
    SELECT [t5].[Nickname], [t5].[SquadId], [t5].[Nickname0], [t5].[SquadId0], [t5].[FullName], [o.Reports.Weapons0].[Id], [w.Owner.Squad0].[Id] AS [Id0]
    FROM [Weapons] AS [o.Reports.Weapons0]
    LEFT JOIN (
        SELECT [w.Owner0].*
        FROM [Gears] AS [w.Owner0]
        WHERE [w.Owner0].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t3] ON [o.Reports.Weapons0].[OwnerFullName] = [t3].[FullName]
    LEFT JOIN [Squads] AS [w.Owner.Squad0] ON [t3].[SquadId] = [w.Owner.Squad0].[Id]
    INNER JOIN (
        SELECT [t4].[Nickname], [t4].[SquadId], [o.Reports1].[Nickname] AS [Nickname0], [o.Reports1].[SquadId] AS [SquadId0], [o.Reports1].[FullName]
        FROM [Gears] AS [o.Reports1]
        INNER JOIN (
            SELECT [o2].[Nickname], [o2].[SquadId]
            FROM [Gears] AS [o2]
            WHERE [o2].[Discriminator] = N'Officer'
        ) AS [t4] ON ([o.Reports1].[LeaderNickname] = [t4].[Nickname]) AND ([o.Reports1].[LeaderSquadId] = [t4].[SquadId])
        WHERE [o.Reports1].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t5] ON [o.Reports.Weapons0].[OwnerFullName] = [t5].[FullName]
) AS [t6] ON [w.Owner.Squad.Members].[SquadId] = [t6].[Id0]
WHERE [w.Owner.Squad.Members].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t6].[Nickname], [t6].[SquadId], [t6].[Nickname0], [t6].[SquadId0], [t6].[FullName], [t6].[Id], [t6].[Id0], [Nickname1]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
