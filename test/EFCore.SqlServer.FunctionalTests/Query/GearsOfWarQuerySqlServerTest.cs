// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class GearsOfWarQuerySqlServerTest : GearsOfWarQueryTestBase<GearsOfWarQuerySqlServerFixture>
    {
        private static readonly string _eol = Environment.NewLine;

        // ReSharper disable once UnusedParameter.Local
        public GearsOfWarQuerySqlServerTest(GearsOfWarQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Entity_equality_empty(bool isAsync)
        {
            await base.Entity_equality_empty(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] IS NULL AND ([g].[SquadId] = 0))");
        }

        public override async Task Include_multiple_one_to_one_and_one_to_many(bool isAsync)
        {
            await base.Include_multiple_one_to_one_and_one_to_many(isAsync);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOrBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
ORDER BY [t0].[FullName]",
                //
                @"SELECT [t.Gear.Weapons].[Id], [t.Gear.Weapons].[AmmunitionType], [t.Gear.Weapons].[IsAutomatic], [t.Gear.Weapons].[Name], [t.Gear.Weapons].[OwnerFullName], [t.Gear.Weapons].[SynergyWithId]
FROM [Weapons] AS [t.Gear.Weapons]
INNER JOIN (
    SELECT DISTINCT [t2].[FullName]
    FROM [Tags] AS [t1]
    LEFT JOIN (
        SELECT [t.Gear0].*
        FROM [Gears] AS [t.Gear0]
        WHERE [t.Gear0].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t2] ON ([t1].[GearNickName] = [t2].[Nickname]) AND ([t1].[GearSquadId] = [t2].[SquadId])
) AS [t3] ON [t.Gear.Weapons].[OwnerFullName] = [t3].[FullName]
ORDER BY [t3].[FullName]");
        }

        public override async Task Include_multiple_one_to_one_and_one_to_many_self_reference(bool isAsync)
        {
            await base.Include_multiple_one_to_one_and_one_to_many_self_reference(isAsync);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [Weapons] AS [w]
LEFT JOIN (
    SELECT [w.Owner].*
    FROM [Gears] AS [w.Owner]
    WHERE [w.Owner].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [w].[OwnerFullName] = [t].[FullName]
ORDER BY [t].[FullName]",
                //
                @"SELECT [w.Owner.Weapons].[Id], [w.Owner.Weapons].[AmmunitionType], [w.Owner.Weapons].[IsAutomatic], [w.Owner.Weapons].[Name], [w.Owner.Weapons].[OwnerFullName], [w.Owner.Weapons].[SynergyWithId]
FROM [Weapons] AS [w.Owner.Weapons]
INNER JOIN (
    SELECT DISTINCT [t0].[FullName]
    FROM [Weapons] AS [w0]
    LEFT JOIN (
        SELECT [w.Owner0].*
        FROM [Gears] AS [w.Owner0]
        WHERE [w.Owner0].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t0] ON [w0].[OwnerFullName] = [t0].[FullName]
) AS [t1] ON [w.Owner.Weapons].[OwnerFullName] = [t1].[FullName]
ORDER BY [t1].[FullName]");
        }

        public override async Task Include_multiple_one_to_one_and_one_to_one_and_one_to_many(bool isAsync)
        {
            await base.Include_multiple_one_to_one_and_one_to_one_and_one_to_many(isAsync);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOrBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t.Gear.Squad].[Id], [t.Gear.Squad].[InternalNumber], [t.Gear.Squad].[Name]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
LEFT JOIN [Squads] AS [t.Gear.Squad] ON [t0].[SquadId] = [t.Gear.Squad].[Id]
ORDER BY [t.Gear.Squad].[Id]",
                //
                @"SELECT [t.Gear.Squad.Members].[Nickname], [t.Gear.Squad.Members].[SquadId], [t.Gear.Squad.Members].[AssignedCityName], [t.Gear.Squad.Members].[CityOrBirthName], [t.Gear.Squad.Members].[Discriminator], [t.Gear.Squad.Members].[FullName], [t.Gear.Squad.Members].[HasSoulPatch], [t.Gear.Squad.Members].[LeaderNickname], [t.Gear.Squad.Members].[LeaderSquadId], [t.Gear.Squad.Members].[Rank]
FROM [Gears] AS [t.Gear.Squad.Members]
INNER JOIN (
    SELECT DISTINCT [t.Gear.Squad0].[Id]
    FROM [Tags] AS [t1]
    LEFT JOIN (
        SELECT [t.Gear0].*
        FROM [Gears] AS [t.Gear0]
        WHERE [t.Gear0].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t2] ON ([t1].[GearNickName] = [t2].[Nickname]) AND ([t1].[GearSquadId] = [t2].[SquadId])
    LEFT JOIN [Squads] AS [t.Gear.Squad0] ON [t2].[SquadId] = [t.Gear.Squad0].[Id]
) AS [t3] ON [t.Gear.Squad.Members].[SquadId] = [t3].[Id]
WHERE [t.Gear.Squad.Members].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t3].[Id]");
        }

        public override async Task Include_multiple_one_to_one_optional_and_one_to_one_required(bool isAsync)
        {
            await base.Include_multiple_one_to_one_optional_and_one_to_one_required(isAsync);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOrBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t.Gear.Squad].[Id], [t.Gear.Squad].[InternalNumber], [t.Gear.Squad].[Name]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
LEFT JOIN [Squads] AS [t.Gear.Squad] ON [t0].[SquadId] = [t.Gear.Squad].[Id]");
        }

        public override async Task Include_multiple_circular(bool isAsync)
        {
            await base.Include_multiple_circular(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.CityOfBirth].[Name], [g.CityOfBirth].[Location]
FROM [Gears] AS [g]
INNER JOIN [Cities] AS [g.CityOfBirth] ON [g].[CityOrBirthName] = [g.CityOfBirth].[Name]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g.CityOfBirth].[Name]",
                //
                @"SELECT [g.CityOfBirth.StationedGears].[Nickname], [g.CityOfBirth.StationedGears].[SquadId], [g.CityOfBirth.StationedGears].[AssignedCityName], [g.CityOfBirth.StationedGears].[CityOrBirthName], [g.CityOfBirth.StationedGears].[Discriminator], [g.CityOfBirth.StationedGears].[FullName], [g.CityOfBirth.StationedGears].[HasSoulPatch], [g.CityOfBirth.StationedGears].[LeaderNickname], [g.CityOfBirth.StationedGears].[LeaderSquadId], [g.CityOfBirth.StationedGears].[Rank]
FROM [Gears] AS [g.CityOfBirth.StationedGears]
INNER JOIN (
    SELECT DISTINCT [g.CityOfBirth0].[Name]
    FROM [Gears] AS [g0]
    INNER JOIN [Cities] AS [g.CityOfBirth0] ON [g0].[CityOrBirthName] = [g.CityOfBirth0].[Name]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [g.CityOfBirth.StationedGears].[AssignedCityName] = [t].[Name]
WHERE [g.CityOfBirth.StationedGears].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[Name]");
        }

        public override async Task Include_multiple_circular_with_filter(bool isAsync)
        {
            await base.Include_multiple_circular_with_filter(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.CityOfBirth].[Name], [g.CityOfBirth].[Location]
FROM [Gears] AS [g]
INNER JOIN [Cities] AS [g.CityOfBirth] ON [g].[CityOrBirthName] = [g.CityOfBirth].[Name]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] = N'Marcus')
ORDER BY [g.CityOfBirth].[Name]",
                //
                @"SELECT [g.CityOfBirth.StationedGears].[Nickname], [g.CityOfBirth.StationedGears].[SquadId], [g.CityOfBirth.StationedGears].[AssignedCityName], [g.CityOfBirth.StationedGears].[CityOrBirthName], [g.CityOfBirth.StationedGears].[Discriminator], [g.CityOfBirth.StationedGears].[FullName], [g.CityOfBirth.StationedGears].[HasSoulPatch], [g.CityOfBirth.StationedGears].[LeaderNickname], [g.CityOfBirth.StationedGears].[LeaderSquadId], [g.CityOfBirth.StationedGears].[Rank]
FROM [Gears] AS [g.CityOfBirth.StationedGears]
INNER JOIN (
    SELECT DISTINCT [g.CityOfBirth0].[Name]
    FROM [Gears] AS [g0]
    INNER JOIN [Cities] AS [g.CityOfBirth0] ON [g0].[CityOrBirthName] = [g.CityOfBirth0].[Name]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND ([g0].[Nickname] = N'Marcus')
) AS [t] ON [g.CityOfBirth.StationedGears].[AssignedCityName] = [t].[Name]
WHERE [g.CityOfBirth.StationedGears].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[Name]");
        }

        public override async Task Include_using_alternate_key(bool isAsync)
        {
            await base.Include_using_alternate_key(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] = N'Marcus')
ORDER BY [g].[FullName]",
                //
                @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT [g0].[FullName]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND ([g0].[Nickname] = N'Marcus')
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
ORDER BY [t].[FullName]");
        }

        public override async Task Include_multiple_include_then_include(bool isAsync)
        {
            await base.Include_multiple_include_then_include(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.CityOfBirth].[Name], [g.CityOfBirth].[Location], [g.AssignedCity].[Name], [g.AssignedCity].[Location]
FROM [Gears] AS [g]
INNER JOIN [Cities] AS [g.CityOfBirth] ON [g].[CityOrBirthName] = [g.CityOfBirth].[Name]
LEFT JOIN [Cities] AS [g.AssignedCity] ON [g].[AssignedCityName] = [g.AssignedCity].[Name]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname], [g.AssignedCity].[Name], [g.CityOfBirth].[Name]",
                //
                @"SELECT [g.AssignedCity.BornGears].[Nickname], [g.AssignedCity.BornGears].[SquadId], [g.AssignedCity.BornGears].[AssignedCityName], [g.AssignedCity.BornGears].[CityOrBirthName], [g.AssignedCity.BornGears].[Discriminator], [g.AssignedCity.BornGears].[FullName], [g.AssignedCity.BornGears].[HasSoulPatch], [g.AssignedCity.BornGears].[LeaderNickname], [g.AssignedCity.BornGears].[LeaderSquadId], [g.AssignedCity.BornGears].[Rank], [g.Tag].[Id], [g.Tag].[GearNickName], [g.Tag].[GearSquadId], [g.Tag].[Note]
FROM [Gears] AS [g.AssignedCity.BornGears]
LEFT JOIN [Tags] AS [g.Tag] ON ([g.AssignedCity.BornGears].[Nickname] = [g.Tag].[GearNickName]) AND ([g.AssignedCity.BornGears].[SquadId] = [g.Tag].[GearSquadId])
INNER JOIN (
    SELECT DISTINCT [g.AssignedCity0].[Name], [g0].[Nickname]
    FROM [Gears] AS [g0]
    INNER JOIN [Cities] AS [g.CityOfBirth0] ON [g0].[CityOrBirthName] = [g.CityOfBirth0].[Name]
    LEFT JOIN [Cities] AS [g.AssignedCity0] ON [g0].[AssignedCityName] = [g.AssignedCity0].[Name]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [g.AssignedCity.BornGears].[CityOrBirthName] = [t].[Name]
WHERE [g.AssignedCity.BornGears].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[Nickname], [t].[Name]",
                //
                @"SELECT [g.AssignedCity.StationedGears].[Nickname], [g.AssignedCity.StationedGears].[SquadId], [g.AssignedCity.StationedGears].[AssignedCityName], [g.AssignedCity.StationedGears].[CityOrBirthName], [g.AssignedCity.StationedGears].[Discriminator], [g.AssignedCity.StationedGears].[FullName], [g.AssignedCity.StationedGears].[HasSoulPatch], [g.AssignedCity.StationedGears].[LeaderNickname], [g.AssignedCity.StationedGears].[LeaderSquadId], [g.AssignedCity.StationedGears].[Rank], [g.Tag0].[Id], [g.Tag0].[GearNickName], [g.Tag0].[GearSquadId], [g.Tag0].[Note]
FROM [Gears] AS [g.AssignedCity.StationedGears]
LEFT JOIN [Tags] AS [g.Tag0] ON ([g.AssignedCity.StationedGears].[Nickname] = [g.Tag0].[GearNickName]) AND ([g.AssignedCity.StationedGears].[SquadId] = [g.Tag0].[GearSquadId])
INNER JOIN (
    SELECT DISTINCT [g.AssignedCity1].[Name], [g1].[Nickname]
    FROM [Gears] AS [g1]
    INNER JOIN [Cities] AS [g.CityOfBirth1] ON [g1].[CityOrBirthName] = [g.CityOfBirth1].[Name]
    LEFT JOIN [Cities] AS [g.AssignedCity1] ON [g1].[AssignedCityName] = [g.AssignedCity1].[Name]
    WHERE [g1].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON [g.AssignedCity.StationedGears].[AssignedCityName] = [t0].[Name]
WHERE [g.AssignedCity.StationedGears].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t0].[Nickname], [t0].[Name]",
                //
                @"SELECT [g.CityOfBirth.BornGears].[Nickname], [g.CityOfBirth.BornGears].[SquadId], [g.CityOfBirth.BornGears].[AssignedCityName], [g.CityOfBirth.BornGears].[CityOrBirthName], [g.CityOfBirth.BornGears].[Discriminator], [g.CityOfBirth.BornGears].[FullName], [g.CityOfBirth.BornGears].[HasSoulPatch], [g.CityOfBirth.BornGears].[LeaderNickname], [g.CityOfBirth.BornGears].[LeaderSquadId], [g.CityOfBirth.BornGears].[Rank], [g.Tag1].[Id], [g.Tag1].[GearNickName], [g.Tag1].[GearSquadId], [g.Tag1].[Note]
FROM [Gears] AS [g.CityOfBirth.BornGears]
LEFT JOIN [Tags] AS [g.Tag1] ON ([g.CityOfBirth.BornGears].[Nickname] = [g.Tag1].[GearNickName]) AND ([g.CityOfBirth.BornGears].[SquadId] = [g.Tag1].[GearSquadId])
INNER JOIN (
    SELECT DISTINCT [g.CityOfBirth2].[Name], [g2].[Nickname], [g.AssignedCity2].[Name] AS [Name0]
    FROM [Gears] AS [g2]
    INNER JOIN [Cities] AS [g.CityOfBirth2] ON [g2].[CityOrBirthName] = [g.CityOfBirth2].[Name]
    LEFT JOIN [Cities] AS [g.AssignedCity2] ON [g2].[AssignedCityName] = [g.AssignedCity2].[Name]
    WHERE [g2].[Discriminator] IN (N'Officer', N'Gear')
) AS [t1] ON [g.CityOfBirth.BornGears].[CityOrBirthName] = [t1].[Name]
WHERE [g.CityOfBirth.BornGears].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t1].[Nickname], [t1].[Name0], [t1].[Name]",
                //
                @"SELECT [g.CityOfBirth.StationedGears].[Nickname], [g.CityOfBirth.StationedGears].[SquadId], [g.CityOfBirth.StationedGears].[AssignedCityName], [g.CityOfBirth.StationedGears].[CityOrBirthName], [g.CityOfBirth.StationedGears].[Discriminator], [g.CityOfBirth.StationedGears].[FullName], [g.CityOfBirth.StationedGears].[HasSoulPatch], [g.CityOfBirth.StationedGears].[LeaderNickname], [g.CityOfBirth.StationedGears].[LeaderSquadId], [g.CityOfBirth.StationedGears].[Rank], [g.Tag2].[Id], [g.Tag2].[GearNickName], [g.Tag2].[GearSquadId], [g.Tag2].[Note]
FROM [Gears] AS [g.CityOfBirth.StationedGears]
LEFT JOIN [Tags] AS [g.Tag2] ON ([g.CityOfBirth.StationedGears].[Nickname] = [g.Tag2].[GearNickName]) AND ([g.CityOfBirth.StationedGears].[SquadId] = [g.Tag2].[GearSquadId])
INNER JOIN (
    SELECT DISTINCT [g.CityOfBirth3].[Name], [g3].[Nickname], [g.AssignedCity3].[Name] AS [Name0]
    FROM [Gears] AS [g3]
    INNER JOIN [Cities] AS [g.CityOfBirth3] ON [g3].[CityOrBirthName] = [g.CityOfBirth3].[Name]
    LEFT JOIN [Cities] AS [g.AssignedCity3] ON [g3].[AssignedCityName] = [g.AssignedCity3].[Name]
    WHERE [g3].[Discriminator] IN (N'Officer', N'Gear')
) AS [t2] ON [g.CityOfBirth.StationedGears].[AssignedCityName] = [t2].[Name]
WHERE [g.CityOfBirth.StationedGears].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t2].[Nickname], [t2].[Name0], [t2].[Name]");
        }

        public override async Task Include_navigation_on_derived_type(bool isAsync)
        {
            await base.Include_navigation_on_derived_type(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId]",
                //
                @"SELECT [o.Reports].[Nickname], [o.Reports].[SquadId], [o.Reports].[AssignedCityName], [o.Reports].[CityOrBirthName], [o.Reports].[Discriminator], [o.Reports].[FullName], [o.Reports].[HasSoulPatch], [o.Reports].[LeaderNickname], [o.Reports].[LeaderSquadId], [o.Reports].[Rank]
FROM [Gears] AS [o.Reports]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] = N'Officer'
) AS [t] ON ([o.Reports].[LeaderNickname] = [t].[Nickname]) AND ([o.Reports].[LeaderSquadId] = [t].[SquadId])
WHERE [o.Reports].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[Nickname], [t].[SquadId]");
        }

        public override async Task String_based_Include_navigation_on_derived_type(bool isAsync)
        {
            await base.String_based_Include_navigation_on_derived_type(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId]",
                //
                @"SELECT [o.Reports].[Nickname], [o.Reports].[SquadId], [o.Reports].[AssignedCityName], [o.Reports].[CityOrBirthName], [o.Reports].[Discriminator], [o.Reports].[FullName], [o.Reports].[HasSoulPatch], [o.Reports].[LeaderNickname], [o.Reports].[LeaderSquadId], [o.Reports].[Rank]
FROM [Gears] AS [o.Reports]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] = N'Officer'
) AS [t] ON ([o.Reports].[LeaderNickname] = [t].[Nickname]) AND ([o.Reports].[LeaderSquadId] = [t].[SquadId])
WHERE [o.Reports].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[Nickname], [t].[SquadId]");
        }

        public override async Task Select_Where_Navigation_Included(bool isAsync)
        {
            await base.Select_Where_Navigation_Included(isAsync);

            AssertSql(
                @"SELECT [o].[Id], [o].[GearNickName], [o].[GearSquadId], [o].[Note], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [Tags] AS [o]
LEFT JOIN (
    SELECT [o.Gear].*
    FROM [Gears] AS [o.Gear]
    WHERE [o.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([o].[GearNickName] = [t].[Nickname]) AND ([o].[GearSquadId] = [t].[SquadId])
WHERE [t].[Nickname] = N'Marcus'");
        }

        public override async Task Include_with_join_reference1(bool isAsync)
        {
            await base.Include_with_join_reference1(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.CityOfBirth].[Name], [g.CityOfBirth].[Location]
FROM [Gears] AS [g]
INNER JOIN [Cities] AS [g.CityOfBirth] ON [g].[CityOrBirthName] = [g.CityOfBirth].[Name]
INNER JOIN [Tags] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Include_with_join_reference2(bool isAsync)
        {
            await base.Include_with_join_reference2(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.CityOfBirth].[Name], [g.CityOfBirth].[Location]
FROM [Tags] AS [t]
INNER JOIN [Gears] AS [g] ON ([t].[GearSquadId] = [g].[SquadId]) AND ([t].[GearNickName] = [g].[Nickname])
INNER JOIN [Cities] AS [g.CityOfBirth] ON [g].[CityOrBirthName] = [g.CityOfBirth].[Name]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Include_with_join_collection1(bool isAsync)
        {
            await base.Include_with_join_collection1(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
INNER JOIN [Tags] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[FullName]",
                //
                @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT DISTINCT [g0].[FullName]
    FROM [Gears] AS [g0]
    INNER JOIN [Tags] AS [t0] ON ([g0].[SquadId] = [t0].[GearSquadId]) AND ([g0].[Nickname] = [t0].[GearNickName])
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t1] ON [g.Weapons].[OwnerFullName] = [t1].[FullName]
ORDER BY [t1].[FullName]");
        }

        public override async Task Include_with_join_collection2(bool isAsync)
        {
            await base.Include_with_join_collection2(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Tags] AS [t]
INNER JOIN [Gears] AS [g] ON ([t].[GearSquadId] = [g].[SquadId]) AND ([t].[GearNickName] = [g].[Nickname])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[FullName]",
                //
                @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT DISTINCT [g0].[FullName]
    FROM [Tags] AS [t0]
    INNER JOIN [Gears] AS [g0] ON ([t0].[GearSquadId] = [g0].[SquadId]) AND ([t0].[GearNickName] = [g0].[Nickname])
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t1] ON [g.Weapons].[OwnerFullName] = [t1].[FullName]
ORDER BY [t1].[FullName]");
        }

        public override void Include_where_list_contains_navigation(bool isAsync)
        {
            base.Include_where_list_contains_navigation(isAsync);

            AssertSql(
                @"SELECT [t].[Id]
FROM [Tags] AS [t]");

            Assert.Contains(
                "SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Tag].[Id], [g.Tag].[GearNickName], [g.Tag].[GearSquadId], [g.Tag].[Note]" + _eol +
                "FROM [Gears] AS [g]" + _eol +
                "LEFT JOIN [Tags] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])" + _eol +
                "WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g.Tag].[Id] IS NOT NULL AND [g.Tag].[Id] IN (",
                Fixture.TestSqlLoggerFactory.SqlStatements[1]);
        }

        public override void Include_where_list_contains_navigation2(bool isAsync)
        {
            base.Include_where_list_contains_navigation2(isAsync);

            AssertSql(
                @"SELECT [t].[Id]
FROM [Tags] AS [t]");

            Assert.Contains(
                "SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Tag].[Id], [g.Tag].[GearNickName], [g.Tag].[GearSquadId], [g.Tag].[Note]" + _eol +
                "FROM [Gears] AS [g]" + _eol +
                "LEFT JOIN [Tags] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])" + _eol +
                "INNER JOIN [Cities] AS [g.CityOfBirth] ON [g].[CityOrBirthName] = [g.CityOfBirth].[Name]" + _eol +
                "WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g.CityOfBirth].[Location] IS NOT NULL AND [g.Tag].[Id] IN (",
                Fixture.TestSqlLoggerFactory.SqlStatements[1]);
        }

        public override void Navigation_accessed_twice_outside_and_inside_subquery(bool isAsync)
        {
            base.Navigation_accessed_twice_outside_and_inside_subquery(isAsync);

            AssertSql(
                @"SELECT [t].[Id]
FROM [Tags] AS [t]");

            Assert.Contains(
                "SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]" + _eol +
                "FROM [Gears] AS [g]" + _eol +
                "LEFT JOIN [Tags] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])" + _eol +
                "WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g.Tag].[Id] IS NOT NULL AND [g.Tag].[Id] IN (",
                Fixture.TestSqlLoggerFactory.SqlStatements[1]);
        }

        public override async Task Include_with_join_multi_level(bool isAsync)
        {
            await base.Include_with_join_multi_level(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.CityOfBirth].[Name], [g.CityOfBirth].[Location]
FROM [Gears] AS [g]
INNER JOIN [Cities] AS [g.CityOfBirth] ON [g].[CityOrBirthName] = [g.CityOfBirth].[Name]
INNER JOIN [Tags] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g.CityOfBirth].[Name]",
                //
                @"SELECT [g.CityOfBirth.StationedGears].[Nickname], [g.CityOfBirth.StationedGears].[SquadId], [g.CityOfBirth.StationedGears].[AssignedCityName], [g.CityOfBirth.StationedGears].[CityOrBirthName], [g.CityOfBirth.StationedGears].[Discriminator], [g.CityOfBirth.StationedGears].[FullName], [g.CityOfBirth.StationedGears].[HasSoulPatch], [g.CityOfBirth.StationedGears].[LeaderNickname], [g.CityOfBirth.StationedGears].[LeaderSquadId], [g.CityOfBirth.StationedGears].[Rank]
FROM [Gears] AS [g.CityOfBirth.StationedGears]
INNER JOIN (
    SELECT DISTINCT [g.CityOfBirth0].[Name]
    FROM [Gears] AS [g0]
    INNER JOIN [Cities] AS [g.CityOfBirth0] ON [g0].[CityOrBirthName] = [g.CityOfBirth0].[Name]
    INNER JOIN [Tags] AS [t0] ON ([g0].[SquadId] = [t0].[GearSquadId]) AND ([g0].[Nickname] = [t0].[GearNickName])
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t1] ON [g.CityOfBirth.StationedGears].[AssignedCityName] = [t1].[Name]
WHERE [g.CityOfBirth.StationedGears].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t1].[Name]");
        }

        public override async Task Include_with_join_and_inheritance1(bool isAsync)
        {
            await base.Include_with_join_and_inheritance1(isAsync);

            AssertSql(
                @"SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOrBirthName], [o].[Discriminator], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], [o.CityOfBirth].[Name], [o.CityOfBirth].[Location]
FROM [Tags] AS [t]
INNER JOIN [Gears] AS [o] ON ([t].[GearSquadId] = [o].[SquadId]) AND ([t].[GearNickName] = [o].[Nickname])
INNER JOIN [Cities] AS [o.CityOfBirth] ON [o].[CityOrBirthName] = [o.CityOfBirth].[Name]
WHERE [o].[Discriminator] = N'Officer'");
        }

        public override async Task Include_with_join_and_inheritance_with_orderby_before_and_after_include(bool isAsync)
        {
            await base.Include_with_join_and_inheritance_with_orderby_before_and_after_include(isAsync);

            AssertSql(
                "");
        }

        public override async Task Include_with_join_and_inheritance2(bool isAsync)
        {
            await base.Include_with_join_and_inheritance2(isAsync);

            AssertSql(
                @"SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOrBirthName], [o].[Discriminator], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank]
FROM [Gears] AS [o]
INNER JOIN [Tags] AS [t] ON ([o].[SquadId] = [t].[GearSquadId]) AND ([o].[Nickname] = [t].[GearNickName])
WHERE [o].[Discriminator] = N'Officer'
ORDER BY [o].[FullName]",
                //
                @"SELECT [o.Weapons].[Id], [o.Weapons].[AmmunitionType], [o.Weapons].[IsAutomatic], [o.Weapons].[Name], [o.Weapons].[OwnerFullName], [o.Weapons].[SynergyWithId]
FROM [Weapons] AS [o.Weapons]
INNER JOIN (
    SELECT DISTINCT [o0].[FullName]
    FROM [Gears] AS [o0]
    INNER JOIN [Tags] AS [t0] ON ([o0].[SquadId] = [t0].[GearSquadId]) AND ([o0].[Nickname] = [t0].[GearNickName])
    WHERE [o0].[Discriminator] = N'Officer'
) AS [t1] ON [o.Weapons].[OwnerFullName] = [t1].[FullName]
ORDER BY [t1].[FullName]");
        }

        public override async Task Include_with_join_and_inheritance3(bool isAsync)
        {
            await base.Include_with_join_and_inheritance3(isAsync);

            AssertSql(
                @"SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOrBirthName], [o].[Discriminator], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank]
FROM [Tags] AS [t]
INNER JOIN [Gears] AS [o] ON ([t].[GearSquadId] = [o].[SquadId]) AND ([t].[GearNickName] = [o].[Nickname])
WHERE [o].[Discriminator] = N'Officer'
ORDER BY [o].[Nickname], [o].[SquadId]",
                //
                @"SELECT [o.Reports].[Nickname], [o.Reports].[SquadId], [o.Reports].[AssignedCityName], [o.Reports].[CityOrBirthName], [o.Reports].[Discriminator], [o.Reports].[FullName], [o.Reports].[HasSoulPatch], [o.Reports].[LeaderNickname], [o.Reports].[LeaderSquadId], [o.Reports].[Rank]
FROM [Gears] AS [o.Reports]
INNER JOIN (
    SELECT DISTINCT [o0].[Nickname], [o0].[SquadId]
    FROM [Tags] AS [t0]
    INNER JOIN [Gears] AS [o0] ON ([t0].[GearSquadId] = [o0].[SquadId]) AND ([t0].[GearNickName] = [o0].[Nickname])
    WHERE [o0].[Discriminator] = N'Officer'
) AS [t1] ON ([o.Reports].[LeaderNickname] = [t1].[Nickname]) AND ([o.Reports].[LeaderSquadId] = [t1].[SquadId])
WHERE [o.Reports].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t1].[Nickname], [t1].[SquadId]");
        }

        public override async Task Include_with_nested_navigation_in_order_by(bool isAsync)
        {
            await base.Include_with_nested_navigation_in_order_by(isAsync);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [Weapons] AS [w]
LEFT JOIN (
    SELECT [w.Owner].*
    FROM [Gears] AS [w.Owner]
    WHERE [w.Owner].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [w].[OwnerFullName] = [t].[FullName]
LEFT JOIN [Cities] AS [w.Owner.CityOfBirth] ON [t].[CityOrBirthName] = [w.Owner.CityOfBirth].[Name]
WHERE ([t].[Nickname] <> N'Paduk') OR [t].[Nickname] IS NULL
ORDER BY [w.Owner.CityOfBirth].[Name], [w].[Id]");
        }

        public override async Task Where_enum(bool isAsync)
        {
            await base.Where_enum(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Rank] = 2)");
        }

        public override async Task Where_nullable_enum_with_constant(bool isAsync)
        {
            await base.Where_nullable_enum_with_constant(isAsync);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] = 1");
        }

        public override async Task Where_nullable_enum_with_null_constant(bool isAsync)
        {
            await base.Where_nullable_enum_with_null_constant(isAsync);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] IS NULL");
        }

        public override async Task Where_nullable_enum_with_non_nullable_parameter(bool isAsync)
        {
            await base.Where_nullable_enum_with_non_nullable_parameter(isAsync);

            AssertSql(
                @"@__ammunitionType_0='1'

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] = @__ammunitionType_0");
        }

        public override async Task Where_nullable_enum_with_nullable_parameter(bool isAsync)
        {
            await base.Where_nullable_enum_with_nullable_parameter(isAsync);

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

        public override async Task Where_bitwise_and_enum(bool isAsync)
        {
            await base.Where_bitwise_and_enum(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) > 0)",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) = 1)");
        }

        public override async Task Where_bitwise_and_integral(bool isAsync)
        {
            await base.Where_bitwise_and_integral(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) = 1)",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & CAST(1 AS bigint)) = CAST(1 AS bigint))",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) = 1)",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) = 1)");
        }

        public override async Task Where_bitwise_and_nullable_enum_with_constant(bool isAsync)
        {
            await base.Where_bitwise_and_nullable_enum_with_constant(isAsync);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & 1) > 0");
        }

        public override async Task Where_bitwise_and_nullable_enum_with_null_constant(bool isAsync)
        {
            await base.Where_bitwise_and_nullable_enum_with_null_constant(isAsync);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & NULL) > 0");
        }

        public override async Task Where_bitwise_and_nullable_enum_with_non_nullable_parameter(bool isAsync)
        {
            await base.Where_bitwise_and_nullable_enum_with_non_nullable_parameter(isAsync);

            AssertSql(
                @"@__ammunitionType_0='1'

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & @__ammunitionType_0) > 0");

            Assert.Contains(
                RelationalStrings.LogValueConversionSqlLiteralWarning
                    .GenerateMessage(
                        typeof(AmmunitionType).ShortDisplayName(),
                        new EnumToNumberConverter<AmmunitionType, int>().GetType().ShortDisplayName()),
                Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
        }

        public override async Task Where_bitwise_and_nullable_enum_with_nullable_parameter(bool isAsync)
        {
            await base.Where_bitwise_and_nullable_enum_with_nullable_parameter(isAsync);

            AssertSql(
                @"@__ammunitionType_0='1' (Nullable = true)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & @__ammunitionType_0) > 0",
                //
                @"@__ammunitionType_0='' (DbType = Int32)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & @__ammunitionType_0) > 0");

            Assert.Contains(
                RelationalStrings.LogValueConversionSqlLiteralWarning
                    .GenerateMessage(
                        typeof(AmmunitionType).ShortDisplayName(),
                        new EnumToNumberConverter<AmmunitionType, int>().GetType().ShortDisplayName()),
                Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
        }

        public override async Task Where_bitwise_or_enum(bool isAsync)
        {
            await base.Where_bitwise_or_enum(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] | 1) > 0)");
        }

        public override async Task Bitwise_projects_values_in_select(bool isAsync)
        {
            await base.Bitwise_projects_values_in_select(isAsync);

            AssertSql(
                @"SELECT TOP(1) CASE
    WHEN ([g].[Rank] & 1) = 1
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END AS [BitwiseTrue], CASE
    WHEN ([g].[Rank] & 1) = 2
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END AS [BitwiseFalse], [g].[Rank] & 1 AS [BitwiseValue]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) = 1)");
        }

        public override async Task Where_enum_has_flag(bool isAsync)
        {
            await base.Where_enum_has_flag(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) = 1)",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 9) = 9)",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) = 1)",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) = 1)",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ((1 & [g].[Rank]) = [g].[Rank])");
        }

        public override async Task Where_enum_has_flag_subquery(bool isAsync)
        {
            await base.Where_enum_has_flag_subquery(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & COALESCE((
    SELECT TOP(1) [x].[Rank]
    FROM [Gears] AS [x]
    WHERE [x].[Discriminator] IN (N'Officer', N'Gear')
    ORDER BY [x].[Nickname], [x].[SquadId]
), 0)) = COALESCE((
    SELECT TOP(1) [x].[Rank]
    FROM [Gears] AS [x]
    WHERE [x].[Discriminator] IN (N'Officer', N'Gear')
    ORDER BY [x].[Nickname], [x].[SquadId]
), 0))",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ((1 & COALESCE((
    SELECT TOP(1) [x].[Rank]
    FROM [Gears] AS [x]
    WHERE [x].[Discriminator] IN (N'Officer', N'Gear')
    ORDER BY [x].[Nickname], [x].[SquadId]
), 0)) = COALESCE((
    SELECT TOP(1) [x].[Rank]
    FROM [Gears] AS [x]
    WHERE [x].[Discriminator] IN (N'Officer', N'Gear')
    ORDER BY [x].[Nickname], [x].[SquadId]
), 0))");
        }

        public override async Task Where_enum_has_flag_subquery_with_pushdown(bool isAsync)
        {
            await base.Where_enum_has_flag_subquery_with_pushdown(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & (
    SELECT TOP(1) [x].[Rank]
    FROM [Gears] AS [x]
    WHERE [x].[Discriminator] IN (N'Officer', N'Gear')
    ORDER BY [x].[Nickname], [x].[SquadId]
)) = (
    SELECT TOP(1) [x].[Rank]
    FROM [Gears] AS [x]
    WHERE [x].[Discriminator] IN (N'Officer', N'Gear')
    ORDER BY [x].[Nickname], [x].[SquadId]
))",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ((1 & (
    SELECT TOP(1) [x].[Rank]
    FROM [Gears] AS [x]
    WHERE [x].[Discriminator] IN (N'Officer', N'Gear')
    ORDER BY [x].[Nickname], [x].[SquadId]
)) = (
    SELECT TOP(1) [x].[Rank]
    FROM [Gears] AS [x]
    WHERE [x].[Discriminator] IN (N'Officer', N'Gear')
    ORDER BY [x].[Nickname], [x].[SquadId]
))");
        }

        public override async Task Where_enum_has_flag_subquery_client_eval(bool isAsync)
        {
            await base.Where_enum_has_flag_subquery_client_eval(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')",
                //
                @"SELECT TOP(1) [x0].[Rank]
FROM [Gears] AS [x0]
WHERE [x0].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [x0].[Nickname], [x0].[SquadId]",
                //
                @"SELECT TOP(1) [x0].[Rank]
FROM [Gears] AS [x0]
WHERE [x0].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [x0].[Nickname], [x0].[SquadId]",
                //
                @"SELECT TOP(1) [x0].[Rank]
FROM [Gears] AS [x0]
WHERE [x0].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [x0].[Nickname], [x0].[SquadId]",
                //
                @"SELECT TOP(1) [x0].[Rank]
FROM [Gears] AS [x0]
WHERE [x0].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [x0].[Nickname], [x0].[SquadId]",
                //
                @"SELECT TOP(1) [x0].[Rank]
FROM [Gears] AS [x0]
WHERE [x0].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [x0].[Nickname], [x0].[SquadId]");
        }

        public override async Task Where_enum_has_flag_with_non_nullable_parameter(bool isAsync)
        {
            await base.Where_enum_has_flag_with_non_nullable_parameter(isAsync);

            AssertSql(
                @"@__parameter_0='1'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & @__parameter_0) = @__parameter_0)");
        }

        public override async Task Where_has_flag_with_nullable_parameter(bool isAsync)
        {
            await base.Where_has_flag_with_nullable_parameter(isAsync);

            AssertSql(
                @"@__parameter_0='1' (Nullable = true)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & @__parameter_0) = @__parameter_0)");
        }

        public override async Task Select_enum_has_flag(bool isAsync)
        {
            await base.Select_enum_has_flag(isAsync);

            AssertSql(
                @"SELECT TOP(1) CASE
    WHEN ([g].[Rank] & 1) = 1
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END AS [hasFlagTrue], CASE
    WHEN ([g].[Rank] & 2) = 2
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END AS [hasFlagFalse]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) = 1)");
        }

        public override async Task Where_count_subquery_without_collision(bool isAsync)
        {
            await base.Where_count_subquery_without_collision(isAsync);

            AssertSql(
                @"SELECT [w].[Nickname], [w].[SquadId], [w].[AssignedCityName], [w].[CityOrBirthName], [w].[Discriminator], [w].[FullName], [w].[HasSoulPatch], [w].[LeaderNickname], [w].[LeaderSquadId], [w].[Rank]
FROM [Gears] AS [w]
WHERE [w].[Discriminator] IN (N'Officer', N'Gear') AND ((
    SELECT COUNT(*)
    FROM [Weapons] AS [w0]
    WHERE [w].[FullName] = [w0].[OwnerFullName]
) = 2)");
        }

        public override async Task Where_any_subquery_without_collision(bool isAsync)
        {
            await base.Where_any_subquery_without_collision(isAsync);

            AssertSql(
                @"SELECT [w].[Nickname], [w].[SquadId], [w].[AssignedCityName], [w].[CityOrBirthName], [w].[Discriminator], [w].[FullName], [w].[HasSoulPatch], [w].[LeaderNickname], [w].[LeaderSquadId], [w].[Rank]
FROM [Gears] AS [w]
WHERE [w].[Discriminator] IN (N'Officer', N'Gear') AND EXISTS (
    SELECT 1
    FROM [Weapons] AS [w0]
    WHERE [w].[FullName] = [w0].[OwnerFullName])");
        }

        public override async Task Select_inverted_boolean(bool isAsync)
        {
            await base.Select_inverted_boolean(isAsync);

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = 0
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END AS [Manual]
FROM [Weapons] AS [w]
WHERE [w].[IsAutomatic] = 1");
        }

        public override async Task Select_comparison_with_null(bool isAsync)
        {
            await base.Select_comparison_with_null(isAsync);

            AssertSql(
                @"@__ammunitionType_0='1' (Nullable = true)

SELECT [w].[Id], CASE
    WHEN [w].[AmmunitionType] = @__ammunitionType_0
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END AS [Cartidge]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] = @__ammunitionType_0",
                //
                @"SELECT [w].[Id], CASE
    WHEN [w].[AmmunitionType] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END AS [Cartidge]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] IS NULL");
        }

        public override async Task Select_ternary_operation_with_boolean(bool isAsync)
        {
            await base.Select_ternary_operation_with_boolean(isAsync);

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = 1
    THEN 1 ELSE 0
END AS [Num]
FROM [Weapons] AS [w]");
        }

        public override async Task Select_ternary_operation_with_inverted_boolean(bool isAsync)
        {
            await base.Select_ternary_operation_with_inverted_boolean(isAsync);

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = 0
    THEN 1 ELSE 0
END AS [Num]
FROM [Weapons] AS [w]");
        }

        public override async Task Select_ternary_operation_with_has_value_not_null(bool isAsync)
        {
            await base.Select_ternary_operation_with_has_value_not_null(isAsync);

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN [w].[AmmunitionType] IS NOT NULL AND ([w].[AmmunitionType] = 1)
    THEN N'Yes' ELSE N'No'
END AS [IsCartidge]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] IS NOT NULL AND ([w].[AmmunitionType] = 1)");
        }

        public override async Task Select_ternary_operation_multiple_conditions(bool isAsync)
        {
            await base.Select_ternary_operation_multiple_conditions(isAsync);

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN ([w].[AmmunitionType] = 2) AND ([w].[SynergyWithId] = 1)
    THEN N'Yes' ELSE N'No'
END AS [IsCartidge]
FROM [Weapons] AS [w]");
        }

        public override async Task Select_ternary_operation_multiple_conditions_2(bool isAsync)
        {
            await base.Select_ternary_operation_multiple_conditions_2(isAsync);

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN ([w].[IsAutomatic] = 0) AND ([w].[SynergyWithId] = 1)
    THEN N'Yes' ELSE N'No'
END AS [IsCartidge]
FROM [Weapons] AS [w]");
        }

        public override async Task Select_multiple_conditions(bool isAsync)
        {
            await base.Select_multiple_conditions(isAsync);

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN ([w].[IsAutomatic] = 0) AND ([w].[SynergyWithId] = 1)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END AS [IsCartidge]
FROM [Weapons] AS [w]");
        }

        public override async Task Select_nested_ternary_operations(bool isAsync)
        {
            await base.Select_nested_ternary_operations(isAsync);

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = 0
    THEN CASE
        WHEN [w].[AmmunitionType] = 1
        THEN N'ManualCartridge' ELSE N'Manual'
    END ELSE N'Auto'
END AS [IsManualCartidge]
FROM [Weapons] AS [w]");
        }

        public override async Task Null_propagation_optimization1(bool isAsync)
        {
            await base.Null_propagation_optimization1(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[LeaderNickname] = N'Marcus')");
        }

        public override async Task Null_propagation_optimization2(bool isAsync)
        {
            await base.Null_propagation_optimization2(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (RIGHT([g].[LeaderNickname], LEN(N'us')) = N'us')");
        }

        public override async Task Null_propagation_optimization3(bool isAsync)
        {
            await base.Null_propagation_optimization3(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (RIGHT([g].[LeaderNickname], LEN(N'us')) = N'us')");
        }

        public override async Task Null_propagation_optimization4(bool isAsync)
        {
            await base.Null_propagation_optimization4(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (CAST(LEN([g].[LeaderNickname]) AS int) = 5)");
        }

        public override async Task Null_propagation_optimization5(bool isAsync)
        {
            await base.Null_propagation_optimization5(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (CAST(LEN([g].[LeaderNickname]) AS int) = 5)");
        }

        public override async Task Null_propagation_optimization6(bool isAsync)
        {
            await base.Null_propagation_optimization6(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (CAST(LEN([g].[LeaderNickname]) AS int) = 5)");
        }

        public override async Task Select_null_propagation_optimization7(bool isAsync)
        {
            await base.Select_null_propagation_optimization7(isAsync);

            AssertSql(
                @"SELECT [g].[LeaderNickname] + [g].[LeaderNickname]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Select_null_propagation_optimization8(bool isAsync)
        {
            await base.Select_null_propagation_optimization8(isAsync);

            AssertSql(
                @"SELECT [g].[LeaderNickname] + [g].[LeaderNickname]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Select_null_propagation_optimization9(bool isAsync)
        {
            await base.Select_null_propagation_optimization9(isAsync);

            AssertSql(
                @"SELECT CAST(LEN([g].[FullName]) AS int)
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Select_null_propagation_negative1(bool isAsync)
        {
            await base.Select_null_propagation_negative1(isAsync);

            AssertSql(
                @"SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL
    THEN CASE
        WHEN CAST(LEN([g].[Nickname]) AS int) = 5
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END ELSE NULL
END
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Select_null_propagation_negative2(bool isAsync)
        {
            await base.Select_null_propagation_negative2(isAsync);

            AssertSql(
                @"SELECT CASE
    WHEN [g1].[LeaderNickname] IS NOT NULL
    THEN [g2].[LeaderNickname] ELSE NULL
END
FROM [Gears] AS [g1]
CROSS JOIN [Gears] AS [g2]
WHERE [g1].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Select_null_propagation_negative3(bool isAsync)
        {
            await base.Select_null_propagation_negative3(isAsync);

            AssertSql(
                @"SELECT [t].[Nickname], CASE
    WHEN [t].[Nickname] IS NOT NULL
    THEN CASE
        WHEN [t].[LeaderNickname] IS NOT NULL
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END ELSE NULL
END AS [Condition]
FROM [Gears] AS [g1]
LEFT JOIN (
    SELECT [g2].*
    FROM [Gears] AS [g2]
    WHERE [g2].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [g1].[HasSoulPatch] = 1
WHERE [g1].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[Nickname]");
        }

        public override async Task Select_null_propagation_negative4(bool isAsync)
        {
            await base.Select_null_propagation_negative4(isAsync);

            AssertSql(
                @"SELECT CASE
    WHEN [t].[Nickname] IS NOT NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END, [t].[Nickname] AS [Item1]
FROM [Gears] AS [g1]
LEFT JOIN (
    SELECT [g2].*
    FROM [Gears] AS [g2]
    WHERE [g2].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [g1].[HasSoulPatch] = 1
WHERE [g1].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[Nickname]");
        }

        public override async Task Select_null_propagation_negative5(bool isAsync)
        {
            await base.Select_null_propagation_negative5(isAsync);

            AssertSql(
                @"SELECT CASE
    WHEN [t].[Nickname] IS NOT NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END, [t].[Nickname]
FROM [Gears] AS [g1]
LEFT JOIN (
    SELECT [g2].*
    FROM [Gears] AS [g2]
    WHERE [g2].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [g1].[HasSoulPatch] = 1
WHERE [g1].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[Nickname]");
        }

        public override async Task Select_null_propagation_negative6(bool isAsync)
        {
            await base.Select_null_propagation_negative6(isAsync);

            AssertSql(
                @"SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL
    THEN CASE
        WHEN CAST(LEN([g].[LeaderNickname]) AS int) <> CAST(LEN([g].[LeaderNickname]) AS int)
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END ELSE NULL
END
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Select_null_propagation_negative7(bool isAsync)
        {
            await base.Select_null_propagation_negative7(isAsync);

            AssertSql(
                @"SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL
    THEN CASE
        WHEN (([g].[LeaderNickname] = [g].[LeaderNickname]) AND ([g].[LeaderNickname] IS NOT NULL AND [g].[LeaderNickname] IS NOT NULL)) OR ([g].[LeaderNickname] IS NULL AND [g].[LeaderNickname] IS NULL)
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END ELSE NULL
END
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Select_null_propagation_negative8(bool isAsync)
        {
            await base.Select_null_propagation_negative8(isAsync);

            AssertSql(
                @"SELECT CASE
    WHEN [t0].[SquadId] IS NOT NULL
    THEN [t.Gear.AssignedCity].[Name] ELSE NULL
END
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
LEFT JOIN [Cities] AS [t.Gear.AssignedCity] ON [t0].[AssignedCityName] = [t.Gear.AssignedCity].[Name]");
        }

        public override async Task Select_null_propagation_works_for_navigations_with_composite_keys(bool isAsync)
        {
            await base.Select_null_propagation_works_for_navigations_with_composite_keys(isAsync);

            AssertSql(
                @"SELECT [t0].[Nickname]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])");
        }

        public override async Task Select_null_propagation_works_for_multiple_navigations_with_composite_keys(bool isAsync)
        {
            await base.Select_null_propagation_works_for_multiple_navigations_with_composite_keys(isAsync);

            AssertSql(
                @"SELECT [t.Gear.Tag.Gear.AssignedCity].[Name]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
LEFT JOIN [Tags] AS [t.Gear.Tag] ON (([t0].[Nickname] = [t.Gear.Tag].[GearNickName]) OR ([t0].[Nickname] IS NULL AND [t.Gear.Tag].[GearNickName] IS NULL)) AND (([t0].[SquadId] = [t.Gear.Tag].[GearSquadId]) OR ([t0].[SquadId] IS NULL AND [t.Gear.Tag].[GearSquadId] IS NULL))
LEFT JOIN (
    SELECT [t.Gear.Tag.Gear].*
    FROM [Gears] AS [t.Gear.Tag.Gear]
    WHERE [t.Gear.Tag.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t1] ON ([t.Gear.Tag].[GearNickName] = [t1].[Nickname]) AND ([t.Gear.Tag].[GearSquadId] = [t1].[SquadId])
LEFT JOIN [Cities] AS [t.Gear.Tag.Gear.AssignedCity] ON [t1].[AssignedCityName] = [t.Gear.Tag.Gear.AssignedCity].[Name]");
        }

        public override async Task Select_conditional_with_anonymous_type_and_null_constant(bool isAsync)
        {
            await base.Select_conditional_with_anonymous_type_and_null_constant(isAsync);

            AssertSql(
                @"SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END, [g].[HasSoulPatch]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]");
        }

        public override async Task Select_conditional_with_anonymous_types(bool isAsync)
        {
            await base.Select_conditional_with_anonymous_types(isAsync);

            AssertSql(
                @"SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END, [g].[Nickname] AS [Name], [g].[FullName] AS [Name0]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [Name]");
        }

        public override async Task Where_conditional_with_anonymous_type(bool isAsync)
        {
            await base.Where_conditional_with_anonymous_type(isAsync);

            AssertSql(
                @"SELECT [g].[LeaderNickname], [g].[HasSoulPatch], [g].[Nickname]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]");
        }

        public override async Task Select_coalesce_with_anonymous_types(bool isAsync)
        {
            await base.Select_coalesce_with_anonymous_types(isAsync);

            AssertSql(
                @"SELECT [g].[LeaderNickname] AS [Name], [g].[FullName] AS [Name0]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]");
        }

        public override async Task Where_coalesce_with_anonymous_types(bool isAsync)
        {
            await base.Where_coalesce_with_anonymous_types(isAsync);

            AssertSql(
                @"SELECT [g].[LeaderNickname], [g].[FullName], [g].[Nickname]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override void Where_compare_anonymous_types()
        {
            base.Where_compare_anonymous_types();

            AssertSql(
                "");
        }

        public override async Task Where_member_access_on_anonymous_type(bool isAsync)
        {
            await base.Where_member_access_on_anonymous_type(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[LeaderNickname] = N'Marcus')");
        }

        public override async Task Where_compare_anonymous_types_with_uncorrelated_members(bool isAsync)
        {
            await base.Where_compare_anonymous_types_with_uncorrelated_members(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname]
FROM [Gears] AS [g]
WHERE 0 = 1");
        }

        public override async Task Select_Where_Navigation_Scalar_Equals_Navigation_Scalar(bool isAsync)
        {
            await base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar(isAsync);

            AssertSql(
                @"SELECT [t1].[Id], [t1].[GearNickName], [t1].[GearSquadId], [t1].[Note], [t2].[Id], [t2].[GearNickName], [t2].[GearSquadId], [t2].[Note]
FROM [Tags] AS [t1]
LEFT JOIN (
    SELECT [t1.Gear].*
    FROM [Gears] AS [t1.Gear]
    WHERE [t1.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([t1].[GearNickName] = [t].[Nickname]) AND ([t1].[GearSquadId] = [t].[SquadId])
CROSS JOIN [Tags] AS [t2]
LEFT JOIN (
    SELECT [t2.Gear].*
    FROM [Gears] AS [t2.Gear]
    WHERE [t2.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t2].[GearNickName] = [t0].[Nickname]) AND ([t2].[GearSquadId] = [t0].[SquadId])
WHERE ([t].[Nickname] = [t0].[Nickname]) OR ([t].[Nickname] IS NULL AND [t0].[Nickname] IS NULL)");
        }

        public override async Task Select_Singleton_Navigation_With_Member_Access(bool isAsync)
        {
            await base.Select_Singleton_Navigation_With_Member_Access(isAsync);

            AssertSql(
                @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName] AS [B], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [Tags] AS [ct]
LEFT JOIN (
    SELECT [ct.Gear].*
    FROM [Gears] AS [ct.Gear]
    WHERE [ct.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([ct].[GearNickName] = [t].[Nickname]) AND ([ct].[GearSquadId] = [t].[SquadId])
WHERE ([t].[Nickname] = N'Marcus') AND (([t].[CityOrBirthName] <> N'Ephyra') OR [t].[CityOrBirthName] IS NULL)");
        }

        public override async Task Select_Where_Navigation(bool isAsync)
        {
            await base.Select_Where_Navigation(isAsync);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE [t0].[Nickname] = N'Marcus'");
        }

        public override async Task Select_Where_Navigation_Client(bool isAsync)
        {
            await base.Select_Where_Navigation_Client(isAsync);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOrBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE [t].[GearNickName] IS NOT NULL OR [t].[GearSquadId] IS NOT NULL");
        }

        public override async Task Select_Where_Navigation_Equals_Navigation(bool isAsync)
        {
            await base.Select_Where_Navigation_Equals_Navigation(isAsync);

            AssertSql(
                @"SELECT [t1].[Id], [t1].[GearNickName], [t1].[GearSquadId], [t1].[Note], [t2].[Id], [t2].[GearNickName], [t2].[GearSquadId], [t2].[Note]
FROM [Tags] AS [t1]
LEFT JOIN (
    SELECT [t1.Gear].*
    FROM [Gears] AS [t1.Gear]
    WHERE [t1.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([t1].[GearNickName] = [t].[Nickname]) AND ([t1].[GearSquadId] = [t].[SquadId])
CROSS JOIN [Tags] AS [t2]
LEFT JOIN (
    SELECT [t2.Gear].*
    FROM [Gears] AS [t2.Gear]
    WHERE [t2.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t2].[GearNickName] = [t0].[Nickname]) AND ([t2].[GearSquadId] = [t0].[SquadId])
WHERE (([t].[Nickname] = [t0].[Nickname]) OR ([t].[Nickname] IS NULL AND [t0].[Nickname] IS NULL)) AND (([t].[SquadId] = [t0].[SquadId]) OR ([t].[SquadId] IS NULL AND [t0].[SquadId] IS NULL))");
        }

        public override async Task Select_Where_Navigation_Null(bool isAsync)
        {
            await base.Select_Where_Navigation_Null(isAsync);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [Tags] AS [t]
WHERE [t].[GearNickName] IS NULL AND [t].[GearSquadId] IS NULL");
        }

        public override async Task Select_Where_Navigation_Null_Reverse(bool isAsync)
        {
            await base.Select_Where_Navigation_Null_Reverse(isAsync);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [Tags] AS [t]
WHERE [t].[GearNickName] IS NULL AND [t].[GearSquadId] IS NULL");
        }

        public override async Task Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected(bool isAsync)
        {
            await base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected(isAsync);

            AssertSql(
                @"SELECT [t1].[Id] AS [Id1], [t2].[Id] AS [Id2]
FROM [Tags] AS [t1]
LEFT JOIN (
    SELECT [t1.Gear].*
    FROM [Gears] AS [t1.Gear]
    WHERE [t1.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([t1].[GearNickName] = [t].[Nickname]) AND ([t1].[GearSquadId] = [t].[SquadId])
CROSS JOIN [Tags] AS [t2]
LEFT JOIN (
    SELECT [t2.Gear].*
    FROM [Gears] AS [t2.Gear]
    WHERE [t2.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t2].[GearNickName] = [t0].[Nickname]) AND ([t2].[GearSquadId] = [t0].[SquadId])
WHERE ([t].[Nickname] = [t0].[Nickname]) OR ([t].[Nickname] IS NULL AND [t0].[Nickname] IS NULL)");
        }

        public override async Task Optional_Navigation_Null_Coalesce_To_Clr_Type(bool isAsync)
        {
            await base.Optional_Navigation_Null_Coalesce_To_Clr_Type(isAsync);

            AssertSql(
                @"SELECT TOP(1) CAST(COALESCE([w.SynergyWith].[IsAutomatic], 0) AS bit) AS [IsAutomatic]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w.SynergyWith] ON [w].[SynergyWithId] = [w.SynergyWith].[Id]
ORDER BY [w].[Id]");
        }

        public override async Task Where_subquery_boolean(bool isAsync)
        {
            await base.Where_subquery_boolean(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (COALESCE((
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]
), 0) = 1)");
        }

        public override async Task Where_subquery_boolean_with_pushdown(bool isAsync)
        {
            await base.Where_subquery_boolean_with_pushdown(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ((
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]
) = 1)");
        }

        public override async Task Where_subquery_distinct_firstordefault_boolean(bool isAsync)
        {
            await base.Where_subquery_distinct_firstordefault_boolean(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[HasSoulPatch] = 1) AND (COALESCE((
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].*
        FROM [Weapons] AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
    ) AS [t]
    ORDER BY [t].[Id]
), 0) = 1))");
        }

        public override async Task Where_subquery_distinct_firstordefault_boolean_with_pushdown(bool isAsync)
        {
            await base.Where_subquery_distinct_firstordefault_boolean_with_pushdown(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[HasSoulPatch] = 1) AND ((
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].*
        FROM [Weapons] AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
    ) AS [t]
    ORDER BY [t].[Id]
) = 1))");
        }

        public override async Task Where_subquery_distinct_first_boolean(bool isAsync)
        {
            await base.Where_subquery_distinct_first_boolean(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)
ORDER BY [g].[Nickname]",
                //
                @"@_outer_FullName='Damon Baird' (Size = 450)

SELECT TOP(1) [t0].[IsAutomatic]
FROM (
    SELECT DISTINCT [w0].*
    FROM [Weapons] AS [w0]
    WHERE @_outer_FullName = [w0].[OwnerFullName]
) AS [t0]
ORDER BY [t0].[Id]",
                //
                @"@_outer_FullName='Marcus Fenix' (Size = 450)

SELECT TOP(1) [t0].[IsAutomatic]
FROM (
    SELECT DISTINCT [w0].*
    FROM [Weapons] AS [w0]
    WHERE @_outer_FullName = [w0].[OwnerFullName]
) AS [t0]
ORDER BY [t0].[Id]");
        }

        public override async Task Where_subquery_distinct_singleordefault_boolean1(bool isAsync)
        {
            await base.Where_subquery_distinct_singleordefault_boolean1(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)
ORDER BY [g].[Nickname]",
                //
                @"@_outer_FullName='Damon Baird' (Size = 450)

SELECT TOP(2) [t0].[IsAutomatic]
FROM (
    SELECT DISTINCT [w0].*
    FROM [Weapons] AS [w0]
    WHERE (CHARINDEX(N'Lancer', [w0].[Name]) > 0) AND (@_outer_FullName = [w0].[OwnerFullName])
) AS [t0]",
                //
                @"@_outer_FullName='Marcus Fenix' (Size = 450)

SELECT TOP(2) [t0].[IsAutomatic]
FROM (
    SELECT DISTINCT [w0].*
    FROM [Weapons] AS [w0]
    WHERE (CHARINDEX(N'Lancer', [w0].[Name]) > 0) AND (@_outer_FullName = [w0].[OwnerFullName])
) AS [t0]");
        }

        public override async Task Where_subquery_distinct_singleordefault_boolean2(bool isAsync)
        {
            await base.Where_subquery_distinct_singleordefault_boolean2(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)
ORDER BY [g].[Nickname]",
                //
                @"@_outer_FullName='Damon Baird' (Size = 450)

SELECT DISTINCT TOP(2) [w0].[IsAutomatic]
FROM [Weapons] AS [w0]
WHERE (CHARINDEX(N'Lancer', [w0].[Name]) > 0) AND (@_outer_FullName = [w0].[OwnerFullName])",
                //
                @"@_outer_FullName='Marcus Fenix' (Size = 450)

SELECT DISTINCT TOP(2) [w0].[IsAutomatic]
FROM [Weapons] AS [w0]
WHERE (CHARINDEX(N'Lancer', [w0].[Name]) > 0) AND (@_outer_FullName = [w0].[OwnerFullName])");
        }

        public override async Task Where_subquery_distinct_singleordefault_boolean_with_pushdown(bool isAsync)
        {
            await base.Where_subquery_distinct_singleordefault_boolean_with_pushdown(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)
ORDER BY [g].[Nickname]",
                //
                @"@_outer_FullName='Damon Baird' (Size = 450)

SELECT TOP(2) [t0].[IsAutomatic]
FROM (
    SELECT DISTINCT [w0].*
    FROM [Weapons] AS [w0]
    WHERE (CHARINDEX(N'Lancer', [w0].[Name]) > 0) AND (@_outer_FullName = [w0].[OwnerFullName])
) AS [t0]",
                //
                @"@_outer_FullName='Marcus Fenix' (Size = 450)

SELECT TOP(2) [t0].[IsAutomatic]
FROM (
    SELECT DISTINCT [w0].*
    FROM [Weapons] AS [w0]
    WHERE (CHARINDEX(N'Lancer', [w0].[Name]) > 0) AND (@_outer_FullName = [w0].[OwnerFullName])
) AS [t0]");
        }

        public override void Where_subquery_distinct_lastordefault_boolean()
        {
            base.Where_subquery_distinct_lastordefault_boolean();

            AssertSql(
                "");
        }

        public override void Where_subquery_distinct_last_boolean()
        {
            base.Where_subquery_distinct_last_boolean();

            AssertSql(
                "");
        }

        public override async Task Where_subquery_distinct_orderby_firstordefault_boolean(bool isAsync)
        {
            await base.Where_subquery_distinct_orderby_firstordefault_boolean(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[HasSoulPatch] = 1) AND (COALESCE((
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].*
        FROM [Weapons] AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
    ) AS [t]
    ORDER BY [t].[Id]
), 0) = 1))");
        }

        public override async Task Where_subquery_distinct_orderby_firstordefault_boolean_with_pushdown(bool isAsync)
        {
            await base.Where_subquery_distinct_orderby_firstordefault_boolean_with_pushdown(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[HasSoulPatch] = 1) AND ((
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].*
        FROM [Weapons] AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
    ) AS [t]
    ORDER BY [t].[Id]
) = 1))");
        }

        public override async Task Where_subquery_union_firstordefault_boolean(bool isAsync)
        {
            await base.Where_subquery_union_firstordefault_boolean(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)",
                //
                @"@_outer_FullName6='Damon Baird' (Size = 450)

SELECT [w6].[Id], [w6].[AmmunitionType], [w6].[IsAutomatic], [w6].[Name], [w6].[OwnerFullName], [w6].[SynergyWithId]
FROM [Weapons] AS [w6]
WHERE @_outer_FullName6 = [w6].[OwnerFullName]",
                //
                @"@_outer_FullName5='Damon Baird' (Size = 450)

SELECT [w5].[Id], [w5].[AmmunitionType], [w5].[IsAutomatic], [w5].[Name], [w5].[OwnerFullName], [w5].[SynergyWithId]
FROM [Weapons] AS [w5]
WHERE @_outer_FullName5 = [w5].[OwnerFullName]",
                //
                @"@_outer_FullName6='Marcus Fenix' (Size = 450)

SELECT [w6].[Id], [w6].[AmmunitionType], [w6].[IsAutomatic], [w6].[Name], [w6].[OwnerFullName], [w6].[SynergyWithId]
FROM [Weapons] AS [w6]
WHERE @_outer_FullName6 = [w6].[OwnerFullName]",
                //
                @"@_outer_FullName5='Marcus Fenix' (Size = 450)

SELECT [w5].[Id], [w5].[AmmunitionType], [w5].[IsAutomatic], [w5].[Name], [w5].[OwnerFullName], [w5].[SynergyWithId]
FROM [Weapons] AS [w5]
WHERE @_outer_FullName5 = [w5].[OwnerFullName]");
        }

        public override async Task Where_subquery_concat_firstordefault_boolean(bool isAsync)
        {
            await base.Where_subquery_concat_firstordefault_boolean(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)",
                //
                @"@_outer_FullName6='Damon Baird' (Size = 450)

SELECT [w6].[Id], [w6].[AmmunitionType], [w6].[IsAutomatic], [w6].[Name], [w6].[OwnerFullName], [w6].[SynergyWithId]
FROM [Weapons] AS [w6]
WHERE @_outer_FullName6 = [w6].[OwnerFullName]",
                //
                @"@_outer_FullName5='Damon Baird' (Size = 450)

SELECT [w5].[Id], [w5].[AmmunitionType], [w5].[IsAutomatic], [w5].[Name], [w5].[OwnerFullName], [w5].[SynergyWithId]
FROM [Weapons] AS [w5]
WHERE @_outer_FullName5 = [w5].[OwnerFullName]",
                //
                @"@_outer_FullName6='Marcus Fenix' (Size = 450)

SELECT [w6].[Id], [w6].[AmmunitionType], [w6].[IsAutomatic], [w6].[Name], [w6].[OwnerFullName], [w6].[SynergyWithId]
FROM [Weapons] AS [w6]
WHERE @_outer_FullName6 = [w6].[OwnerFullName]",
                //
                @"@_outer_FullName5='Marcus Fenix' (Size = 450)

SELECT [w5].[Id], [w5].[AmmunitionType], [w5].[IsAutomatic], [w5].[Name], [w5].[OwnerFullName], [w5].[SynergyWithId]
FROM [Weapons] AS [w5]
WHERE @_outer_FullName5 = [w5].[OwnerFullName]");
        }

        public override async Task Concat_with_count(bool isAsync)
        {
            await base.Concat_with_count(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')",
                //
                @"SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOrBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gears] AS [g0]
WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Concat_scalars_with_count(bool isAsync)
        {
            await base.Concat_scalars_with_count(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')",
                //
                @"SELECT [g2].[FullName]
FROM [Gears] AS [g2]
WHERE [g2].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Concat_anonymous_with_count(bool isAsync)
        {
            await base.Concat_anonymous_with_count(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname] AS [Name], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')",
                //
                @"SELECT [g2].[Nickname], [g2].[SquadId], [g2].[AssignedCityName], [g2].[CityOrBirthName], [g2].[Discriminator], [g2].[FullName] AS [Name], [g2].[HasSoulPatch], [g2].[LeaderNickname], [g2].[LeaderSquadId], [g2].[Rank]
FROM [Gears] AS [g2]
WHERE [g2].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override void Concat_with_scalar_projection()
        {
            base.Concat_with_scalar_projection();

            AssertSql(
                "");
        }

        public override async Task Select_navigation_with_concat_and_count(bool isAsync)
        {
            await base.Select_navigation_with_concat_and_count(isAsync);

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 0)",
                //
                @"@_outer_FullName2='Augustus Cole' (Size = 450)

SELECT [w2].[Id], [w2].[AmmunitionType], [w2].[IsAutomatic], [w2].[Name], [w2].[OwnerFullName], [w2].[SynergyWithId]
FROM [Weapons] AS [w2]
WHERE @_outer_FullName2 = [w2].[OwnerFullName]",
                //
                @"@_outer_FullName1='Augustus Cole' (Size = 450)

SELECT [w1].[Id], [w1].[AmmunitionType], [w1].[IsAutomatic], [w1].[Name], [w1].[OwnerFullName], [w1].[SynergyWithId]
FROM [Weapons] AS [w1]
WHERE @_outer_FullName1 = [w1].[OwnerFullName]",
                //
                @"@_outer_FullName2='Dominic Santiago' (Size = 450)

SELECT [w2].[Id], [w2].[AmmunitionType], [w2].[IsAutomatic], [w2].[Name], [w2].[OwnerFullName], [w2].[SynergyWithId]
FROM [Weapons] AS [w2]
WHERE @_outer_FullName2 = [w2].[OwnerFullName]",
                //
                @"@_outer_FullName1='Dominic Santiago' (Size = 450)

SELECT [w1].[Id], [w1].[AmmunitionType], [w1].[IsAutomatic], [w1].[Name], [w1].[OwnerFullName], [w1].[SynergyWithId]
FROM [Weapons] AS [w1]
WHERE @_outer_FullName1 = [w1].[OwnerFullName]",
                //
                @"@_outer_FullName2='Garron Paduk' (Size = 450)

SELECT [w2].[Id], [w2].[AmmunitionType], [w2].[IsAutomatic], [w2].[Name], [w2].[OwnerFullName], [w2].[SynergyWithId]
FROM [Weapons] AS [w2]
WHERE @_outer_FullName2 = [w2].[OwnerFullName]",
                //
                @"@_outer_FullName1='Garron Paduk' (Size = 450)

SELECT [w1].[Id], [w1].[AmmunitionType], [w1].[IsAutomatic], [w1].[Name], [w1].[OwnerFullName], [w1].[SynergyWithId]
FROM [Weapons] AS [w1]
WHERE @_outer_FullName1 = [w1].[OwnerFullName]");
        }

        public override async Task Concat_with_groupings(bool isAsync)
        {
            await base.Concat_with_groupings(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[LeaderNickname]",
                //
                @"SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOrBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gears] AS [g0]
WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g0].[LeaderNickname]");
        }

        public override async Task Concat_with_collection_navigations(bool isAsync)
        {
            await base.Concat_with_collection_navigations(isAsync);

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)",
                //
                @"@_outer_FullName2='Damon Baird' (Size = 450)

SELECT [w2].[Id], [w2].[AmmunitionType], [w2].[IsAutomatic], [w2].[Name], [w2].[OwnerFullName], [w2].[SynergyWithId]
FROM [Weapons] AS [w2]
WHERE @_outer_FullName2 = [w2].[OwnerFullName]",
                //
                @"@_outer_FullName1='Damon Baird' (Size = 450)

SELECT [w1].[Id], [w1].[AmmunitionType], [w1].[IsAutomatic], [w1].[Name], [w1].[OwnerFullName], [w1].[SynergyWithId]
FROM [Weapons] AS [w1]
WHERE @_outer_FullName1 = [w1].[OwnerFullName]",
                //
                @"@_outer_FullName2='Marcus Fenix' (Size = 450)

SELECT [w2].[Id], [w2].[AmmunitionType], [w2].[IsAutomatic], [w2].[Name], [w2].[OwnerFullName], [w2].[SynergyWithId]
FROM [Weapons] AS [w2]
WHERE @_outer_FullName2 = [w2].[OwnerFullName]",
                //
                @"@_outer_FullName1='Marcus Fenix' (Size = 450)

SELECT [w1].[Id], [w1].[AmmunitionType], [w1].[IsAutomatic], [w1].[Name], [w1].[OwnerFullName], [w1].[SynergyWithId]
FROM [Weapons] AS [w1]
WHERE @_outer_FullName1 = [w1].[OwnerFullName]");
        }

        public override async Task Union_with_collection_navigations(bool isAsync)
        {
            await base.Union_with_collection_navigations(isAsync);

            AssertSql(
                @"SELECT [o].[Nickname], [o].[SquadId]
FROM [Gears] AS [o]
WHERE [o].[Discriminator] = N'Officer'",
                //
                @"@_outer_Nickname2='Baird' (Size = 450)
@_outer_SquadId2='1'

SELECT [g2].[Nickname], [g2].[SquadId], [g2].[AssignedCityName], [g2].[CityOrBirthName], [g2].[Discriminator], [g2].[FullName], [g2].[HasSoulPatch], [g2].[LeaderNickname], [g2].[LeaderSquadId], [g2].[Rank]
FROM [Gears] AS [g2]
WHERE [g2].[Discriminator] IN (N'Officer', N'Gear') AND ((@_outer_Nickname2 = [g2].[LeaderNickname]) AND (@_outer_SquadId2 = [g2].[LeaderSquadId]))",
                //
                @"@_outer_Nickname1='Baird' (Size = 450)
@_outer_SquadId1='1'

SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOrBirthName], [g1].[Discriminator], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[Rank]
FROM [Gears] AS [g1]
WHERE [g1].[Discriminator] IN (N'Officer', N'Gear') AND ((@_outer_Nickname1 = [g1].[LeaderNickname]) AND (@_outer_SquadId1 = [g1].[LeaderSquadId]))",
                //
                @"@_outer_Nickname2='Marcus' (Size = 450)
@_outer_SquadId2='1'

SELECT [g2].[Nickname], [g2].[SquadId], [g2].[AssignedCityName], [g2].[CityOrBirthName], [g2].[Discriminator], [g2].[FullName], [g2].[HasSoulPatch], [g2].[LeaderNickname], [g2].[LeaderSquadId], [g2].[Rank]
FROM [Gears] AS [g2]
WHERE [g2].[Discriminator] IN (N'Officer', N'Gear') AND ((@_outer_Nickname2 = [g2].[LeaderNickname]) AND (@_outer_SquadId2 = [g2].[LeaderSquadId]))",
                //
                @"@_outer_Nickname1='Marcus' (Size = 450)
@_outer_SquadId1='1'

SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOrBirthName], [g1].[Discriminator], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[Rank]
FROM [Gears] AS [g1]
WHERE [g1].[Discriminator] IN (N'Officer', N'Gear') AND ((@_outer_Nickname1 = [g1].[LeaderNickname]) AND (@_outer_SquadId1 = [g1].[LeaderSquadId]))");
        }

        public override async Task Select_subquery_distinct_firstordefault(bool isAsync)
        {
            await base.Select_subquery_distinct_firstordefault(isAsync);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [t].[Name]
    FROM (
        SELECT DISTINCT [w].*
        FROM [Weapons] AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
    ) AS [t]
    ORDER BY [t].[Id]
)
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)");
        }

        public override async Task Singleton_Navigation_With_Member_Access(bool isAsync)
        {
            await base.Singleton_Navigation_With_Member_Access(isAsync);

            AssertSql(
                @"SELECT [t].[CityOrBirthName] AS [B]
FROM [Tags] AS [ct]
LEFT JOIN (
    SELECT [ct.Gear].*
    FROM [Gears] AS [ct.Gear]
    WHERE [ct.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([ct].[GearNickName] = [t].[Nickname]) AND ([ct].[GearSquadId] = [t].[SquadId])
WHERE ([t].[Nickname] = N'Marcus') AND (([t].[CityOrBirthName] <> N'Ephyra') OR [t].[CityOrBirthName] IS NULL)");
        }

        public override async Task GroupJoin_Composite_Key(bool isAsync)
        {
            await base.GroupJoin_Composite_Key(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Tags] AS [t]
INNER JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Join_navigation_translated_to_subquery_composite_key(bool isAsync)
        {
            await base.Join_navigation_translated_to_subquery_composite_key(isAsync);

            AssertSql(
                @"SELECT [g].[FullName], [t].[Note]
FROM [Gears] AS [g]
INNER JOIN [Tags] AS [t] ON [g].[FullName] = (
    SELECT TOP(1) [subQuery0].[FullName]
    FROM [Gears] AS [subQuery0]
    WHERE [subQuery0].[Discriminator] IN (N'Officer', N'Gear') AND (([subQuery0].[Nickname] = [t].[GearNickName]) AND ([subQuery0].[SquadId] = [t].[GearSquadId]))
)
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Join_with_order_by_on_inner_sequence_navigation_translated_to_subquery_composite_key(bool isAsync)
        {
            await base.Join_with_order_by_on_inner_sequence_navigation_translated_to_subquery_composite_key(isAsync);

            AssertSql(
                "");
        }

        public override async Task Collection_with_inheritance_and_join_include_joined(bool isAsync)
        {
            await base.Collection_with_inheritance_and_join_include_joined(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Tag].[Id], [g.Tag].[GearNickName], [g.Tag].[GearSquadId], [g.Tag].[Note]
FROM [Tags] AS [t]
INNER JOIN [Gears] AS [g] ON ([t].[GearSquadId] = [g].[SquadId]) AND ([t].[GearNickName] = [g].[Nickname])
LEFT JOIN [Tags] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE [g].[Discriminator] = N'Officer'");
        }

        public override async Task Collection_with_inheritance_and_join_include_source(bool isAsync)
        {
            await base.Collection_with_inheritance_and_join_include_source(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Tag].[Id], [g.Tag].[GearNickName], [g.Tag].[GearSquadId], [g.Tag].[Note]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
INNER JOIN [Tags] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
WHERE [g].[Discriminator] = N'Officer'");
        }

        public override async Task Non_unicode_string_literal_is_used_for_non_unicode_column(bool isAsync)
        {
            await base.Non_unicode_string_literal_is_used_for_non_unicode_column(isAsync);

            AssertSql(
                @"SELECT [c].[Name], [c].[Location]
FROM [Cities] AS [c]
WHERE [c].[Location] = 'Unknown'");
        }

        public override async Task Non_unicode_string_literal_is_used_for_non_unicode_column_right(bool isAsync)
        {
            await base.Non_unicode_string_literal_is_used_for_non_unicode_column_right(isAsync);

            AssertSql(
                @"SELECT [c].[Name], [c].[Location]
FROM [Cities] AS [c]
WHERE 'Unknown' = [c].[Location]");
        }

        public override async Task Non_unicode_parameter_is_used_for_non_unicode_column(bool isAsync)
        {
            await base.Non_unicode_parameter_is_used_for_non_unicode_column(isAsync);

            AssertSql(
                @"@__value_0='Unknown' (Size = 100) (DbType = AnsiString)

SELECT [c].[Name], [c].[Location]
FROM [Cities] AS [c]
WHERE [c].[Location] = @__value_0");
        }

        public override async Task Non_unicode_string_literals_in_contains_is_used_for_non_unicode_column(bool isAsync)
        {
            await base.Non_unicode_string_literals_in_contains_is_used_for_non_unicode_column(isAsync);

            AssertSql(
                @"SELECT [c].[Name], [c].[Location]
FROM [Cities] AS [c]
WHERE [c].[Location] IN ('Unknown', 'Jacinto''s location', 'Ephyra''s location')");
        }

        public override async Task Non_unicode_string_literals_is_used_for_non_unicode_column_with_subquery(bool isAsync)
        {
            await base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_subquery(isAsync);

            AssertSql(
                @"SELECT [c].[Name], [c].[Location]
FROM [Cities] AS [c]
WHERE ([c].[Location] = 'Unknown') AND ((
    SELECT COUNT(*)
    FROM [Gears] AS [g]
    WHERE ([g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] = N'Paduk')) AND ([c].[Name] = [g].[CityOrBirthName])
) = 1)");
        }

        public override async Task Non_unicode_string_literals_is_used_for_non_unicode_column_in_subquery(bool isAsync)
        {
            await base.Non_unicode_string_literals_is_used_for_non_unicode_column_in_subquery(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
INNER JOIN [Cities] AS [g.CityOfBirth] ON [g].[CityOrBirthName] = [g.CityOfBirth].[Name]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Nickname] = N'Marcus') AND ([g.CityOfBirth].[Location] = 'Jacinto''s location'))");
        }

        public override async Task Non_unicode_string_literals_is_used_for_non_unicode_column_with_contains(bool isAsync)
        {
            await base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_contains(isAsync);

            AssertSql(
                @"SELECT [c].[Name], [c].[Location]
FROM [Cities] AS [c]
WHERE CHARINDEX(N'Jacinto', [c].[Location]) > 0");
        }

        public override void Non_unicode_string_literals_is_used_for_non_unicode_column_with_concat()
        {
            base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_concat();

            AssertSql(
                @"SELECT [c].[Name], [c].[Location]
FROM [Cities] AS [c]
WHERE CHARINDEX('Add', [c].[Location] + 'Added') > 0");
        }

        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [g2].*
    FROM [Gears] AS [g2]
    WHERE [g2].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [g].[LeaderNickname] = [t].[Nickname]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[FullName]",
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
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND [t0].[Nickname] IS NULL
) AS [t1] ON [g.Weapons].[OwnerFullName] = [t1].[FullName]
ORDER BY [t1].[FullName]");
        }

        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2();

            AssertSql(
                @"SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOrBirthName], [g1].[Discriminator], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[Rank], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [Gears] AS [g1]
LEFT JOIN (
    SELECT [g2].*
    FROM [Gears] AS [g2]
    WHERE [g2].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [g1].[LeaderNickname] = [t].[Nickname]
WHERE [g1].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[FullName]",
                //
                @"SELECT [g2.Weapons].[Id], [g2.Weapons].[AmmunitionType], [g2.Weapons].[IsAutomatic], [g2.Weapons].[Name], [g2.Weapons].[OwnerFullName], [g2.Weapons].[SynergyWithId]
FROM [Weapons] AS [g2.Weapons]
INNER JOIN (
    SELECT DISTINCT [t0].[FullName]
    FROM [Gears] AS [g10]
    LEFT JOIN (
        SELECT [g20].*
        FROM [Gears] AS [g20]
        WHERE [g20].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t0] ON [g10].[LeaderNickname] = [t0].[Nickname]
    WHERE [g10].[Discriminator] IN (N'Officer', N'Gear') AND [t0].[Nickname] IS NOT NULL
) AS [t1] ON [g2.Weapons].[OwnerFullName] = [t1].[FullName]
ORDER BY [t1].[FullName]");
        }

        public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3(bool isAsync)
        {
            await base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [g2].*
    FROM [Gears] AS [g2]
    WHERE [g2].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [g].[LeaderNickname] = [t].[Nickname]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[FullName], [g].[FullName]",
                //
                @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT DISTINCT [g1].[FullName], [t2].[FullName] AS [FullName0]
    FROM [Gears] AS [g1]
    LEFT JOIN (
        SELECT [g21].*
        FROM [Gears] AS [g21]
        WHERE [g21].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t2] ON [g1].[LeaderNickname] = [t2].[Nickname]
    WHERE [g1].[Discriminator] IN (N'Officer', N'Gear') AND [t2].[Nickname] IS NULL
) AS [t3] ON [g.Weapons].[OwnerFullName] = [t3].[FullName]
ORDER BY [t3].[FullName0], [t3].[FullName]",
                //
                @"SELECT [g2.Weapons].[Id], [g2.Weapons].[AmmunitionType], [g2.Weapons].[IsAutomatic], [g2.Weapons].[Name], [g2.Weapons].[OwnerFullName], [g2.Weapons].[SynergyWithId]
FROM [Weapons] AS [g2.Weapons]
INNER JOIN (
    SELECT DISTINCT [t0].[FullName]
    FROM [Gears] AS [g0]
    LEFT JOIN (
        SELECT [g20].*
        FROM [Gears] AS [g20]
        WHERE [g20].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t0] ON [g0].[LeaderNickname] = [t0].[Nickname]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND [t0].[Nickname] IS NOT NULL
) AS [t1] ON [g2.Weapons].[OwnerFullName] = [t1].[FullName]
ORDER BY [t1].[FullName]");
        }

        public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result4(bool isAsync)
        {
            await base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result4(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
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

        public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result(bool isAsync)
        {
            await base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [g2].*
    FROM [Gears] AS [g2]
    WHERE [g2].[Discriminator] = N'Officer'
) AS [t] ON [g].[LeaderNickname] = [t].[Nickname]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[FullName], [g].[FullName]",
                //
                @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT DISTINCT [g1].[FullName], [t2].[FullName] AS [FullName0]
    FROM [Gears] AS [g1]
    LEFT JOIN (
        SELECT [g21].*
        FROM [Gears] AS [g21]
        WHERE [g21].[Discriminator] = N'Officer'
    ) AS [t2] ON [g1].[LeaderNickname] = [t2].[Nickname]
    WHERE [g1].[Discriminator] IN (N'Officer', N'Gear') AND [t2].[Nickname] IS NULL
) AS [t3] ON [g.Weapons].[OwnerFullName] = [t3].[FullName]
ORDER BY [t3].[FullName0], [t3].[FullName]",
                //
                @"SELECT [g2.Weapons].[Id], [g2.Weapons].[AmmunitionType], [g2.Weapons].[IsAutomatic], [g2.Weapons].[Name], [g2.Weapons].[OwnerFullName], [g2.Weapons].[SynergyWithId]
FROM [Weapons] AS [g2.Weapons]
INNER JOIN (
    SELECT DISTINCT [t0].[FullName]
    FROM [Gears] AS [g0]
    LEFT JOIN (
        SELECT [g20].*
        FROM [Gears] AS [g20]
        WHERE [g20].[Discriminator] = N'Officer'
    ) AS [t0] ON [g0].[LeaderNickname] = [t0].[Nickname]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND [t0].[Nickname] IS NOT NULL
) AS [t1] ON [g2.Weapons].[OwnerFullName] = [t1].[FullName]
ORDER BY [t1].[FullName]");
        }

        public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result(bool isAsync)
        {
            await base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [Gears] AS [g]
LEFT JOIN (
    SELECT [g2].*
    FROM [Gears] AS [g2]
    WHERE [g2].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [g].[LeaderNickname] = [t].[Nickname]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[FullName], [g].[FullName]",
                //
                @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT DISTINCT [g1].[FullName], [t2].[FullName] AS [FullName0]
    FROM [Gears] AS [g1]
    LEFT JOIN (
        SELECT [g21].*
        FROM [Gears] AS [g21]
        WHERE [g21].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t2] ON [g1].[LeaderNickname] = [t2].[Nickname]
    WHERE [g1].[Discriminator] IN (N'Officer', N'Gear') AND [t2].[Nickname] IS NULL
) AS [t3] ON [g.Weapons].[OwnerFullName] = [t3].[FullName]
ORDER BY [t3].[FullName0], [t3].[FullName]",
                //
                @"SELECT [g2.Weapons].[Id], [g2.Weapons].[AmmunitionType], [g2.Weapons].[IsAutomatic], [g2.Weapons].[Name], [g2.Weapons].[OwnerFullName], [g2.Weapons].[SynergyWithId]
FROM [Weapons] AS [g2.Weapons]
INNER JOIN (
    SELECT DISTINCT [t0].[FullName]
    FROM [Gears] AS [g0]
    LEFT JOIN (
        SELECT [g20].*
        FROM [Gears] AS [g20]
        WHERE [g20].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t0] ON [g0].[LeaderNickname] = [t0].[Nickname]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND [t0].[Nickname] IS NOT NULL
) AS [t1] ON [g2.Weapons].[OwnerFullName] = [t1].[FullName]
ORDER BY [t1].[FullName]");
        }

        public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result(bool isAsync)
        {
            await base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
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

        public override async Task Coalesce_operator_in_predicate(bool isAsync)
        {
            await base.Coalesce_operator_in_predicate(isAsync);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE COALESCE([w].[IsAutomatic], 0) = 1");
        }

        public override async Task Coalesce_operator_in_predicate_with_other_conditions(bool isAsync)
        {
            await base.Coalesce_operator_in_predicate_with_other_conditions(isAsync);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] = 1) AND (COALESCE([w].[IsAutomatic], 0) = 1)");
        }

        public override async Task Coalesce_operator_in_projection_with_other_conditions(bool isAsync)
        {
            await base.Coalesce_operator_in_projection_with_other_conditions(isAsync);

            AssertSql(
                @"SELECT CASE
    WHEN ([w].[AmmunitionType] = 1) AND (COALESCE([w].[IsAutomatic], 0) = 1)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
FROM [Weapons] AS [w]");
        }

        public override async Task Optional_navigation_type_compensation_works_with_predicate(bool isAsync)
        {
            await base.Optional_navigation_type_compensation_works_with_predicate(isAsync);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE (([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL) AND ([t0].[HasSoulPatch] = 1)");
        }

        public override async Task Optional_navigation_type_compensation_works_with_predicate2(bool isAsync)
        {
            await base.Optional_navigation_type_compensation_works_with_predicate2(isAsync);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE [t0].[HasSoulPatch] = 1");
        }

        public override async Task Optional_navigation_type_compensation_works_with_predicate_negated(bool isAsync)
        {
            await base.Optional_navigation_type_compensation_works_with_predicate_negated(isAsync);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE ([t0].[HasSoulPatch] <> 1) AND [t0].[HasSoulPatch] IS NOT NULL");
        }

        public override async Task Optional_navigation_type_compensation_works_with_predicate_negated_complex1(bool isAsync)
        {
            await base.Optional_navigation_type_compensation_works_with_predicate_negated_complex1(isAsync);

            AssertSql(
                "");
        }

        public override async Task Optional_navigation_type_compensation_works_with_predicate_negated_complex2(bool isAsync)
        {
            await base.Optional_navigation_type_compensation_works_with_predicate_negated_complex2(isAsync);

            AssertSql(
                "");
        }

        public override async Task Optional_navigation_type_compensation_works_with_conditional_expression(bool isAsync)
        {
            await base.Optional_navigation_type_compensation_works_with_conditional_expression(isAsync);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE CASE
    WHEN [t0].[HasSoulPatch] = 1
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = 1");
        }

        public override async Task Optional_navigation_type_compensation_works_with_binary_expression(bool isAsync)
        {
            await base.Optional_navigation_type_compensation_works_with_binary_expression(isAsync);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE ([t0].[HasSoulPatch] = 1) OR (CHARINDEX(N'Cole', [t].[Note]) > 0)");
        }

        public override async Task Optional_navigation_type_compensation_works_with_binary_and_expression(bool isAsync)
        {
            await base.Optional_navigation_type_compensation_works_with_binary_and_expression(isAsync);

            AssertSql(
                @"SELECT CASE
    WHEN ([t0].[HasSoulPatch] = 1) AND (CHARINDEX(N'Cole', [t].[Note]) > 0)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])");
        }

        public override async Task Optional_navigation_type_compensation_works_with_projection(bool isAsync)
        {
            await base.Optional_navigation_type_compensation_works_with_projection(isAsync);

            AssertSql(
                @"SELECT [t0].[SquadId]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL");
        }

        public override async Task Optional_navigation_type_compensation_works_with_projection_into_anonymous_type(bool isAsync)
        {
            await base.Optional_navigation_type_compensation_works_with_projection_into_anonymous_type(isAsync);

            AssertSql(
                @"SELECT [t0].[SquadId]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL");
        }

        public override async Task Optional_navigation_type_compensation_works_with_DTOs(bool isAsync)
        {
            await base.Optional_navigation_type_compensation_works_with_DTOs(isAsync);

            AssertSql(
                @"SELECT [t0].[SquadId] AS [Id]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL");
        }

        public override async Task Optional_navigation_type_compensation_works_with_list_initializers(bool isAsync)
        {
            await base.Optional_navigation_type_compensation_works_with_list_initializers(isAsync);

            AssertSql(
                @"SELECT [t0].[SquadId], [t0].[SquadId] + 1
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL
ORDER BY [t].[Note]");
        }

        public override async Task Optional_navigation_type_compensation_works_with_array_initializers(bool isAsync)
        {
            await base.Optional_navigation_type_compensation_works_with_array_initializers(isAsync);

            AssertSql(
                @"SELECT [t0].[SquadId]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL");
        }

        public override async Task Optional_navigation_type_compensation_works_with_orderby(bool isAsync)
        {
            await base.Optional_navigation_type_compensation_works_with_orderby(isAsync);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL
ORDER BY [t0].[SquadId]");
        }

        public override async Task Optional_navigation_type_compensation_works_with_groupby(bool isAsync)
        {
            await base.Optional_navigation_type_compensation_works_with_groupby(isAsync);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t0].[SquadId]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL
ORDER BY [t0].[SquadId]");
        }

        public override async Task Optional_navigation_type_compensation_works_with_all(bool isAsync)
        {
            await base.Optional_navigation_type_compensation_works_with_all(isAsync);

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Tags] AS [t]
        LEFT JOIN (
            SELECT [t.Gear].*
            FROM [Gears] AS [t.Gear]
            WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
        ) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
        WHERE (([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL) AND ([t0].[HasSoulPatch] = 0))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override async Task Optional_navigation_type_compensation_works_with_negated_predicate(bool isAsync)
        {
            await base.Optional_navigation_type_compensation_works_with_negated_predicate(isAsync);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE (([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL) AND (([t0].[HasSoulPatch] <> 1) AND [t0].[HasSoulPatch] IS NOT NULL)");
        }

        public override async Task Optional_navigation_type_compensation_works_with_contains(bool isAsync)
        {
            await base.Optional_navigation_type_compensation_works_with_contains(isAsync);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE (([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL) AND [t0].[SquadId] IN (
    SELECT [g].[SquadId]
    FROM [Gears] AS [g]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
)");
        }

        public override async Task Optional_navigation_type_compensation_works_with_skip(bool isAsync)
        {
            await base.Optional_navigation_type_compensation_works_with_skip(isAsync);

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

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]
OFFSET @_outer_SquadId ROWS",
                //
                @"@_outer_SquadId='1'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]
OFFSET @_outer_SquadId ROWS",
                //
                @"@_outer_SquadId='1'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]
OFFSET @_outer_SquadId ROWS",
                //
                @"@_outer_SquadId='1'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]
OFFSET @_outer_SquadId ROWS",
                //
                @"@_outer_SquadId='2'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]
OFFSET @_outer_SquadId ROWS");
        }

        public override async Task Optional_navigation_type_compensation_works_with_take(bool isAsync)
        {
            await base.Optional_navigation_type_compensation_works_with_take(isAsync);

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

SELECT TOP(@_outer_SquadId) [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]",
                //
                @"@_outer_SquadId='1'

SELECT TOP(@_outer_SquadId) [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]",
                //
                @"@_outer_SquadId='1'

SELECT TOP(@_outer_SquadId) [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]",
                //
                @"@_outer_SquadId='1'

SELECT TOP(@_outer_SquadId) [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]",
                //
                @"@_outer_SquadId='2'

SELECT TOP(@_outer_SquadId) [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]");
        }

        public override async Task Select_correlated_filtered_collection(bool isAsync)
        {
            await base.Select_correlated_filtered_collection(isAsync);

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND [g].[CityOrBirthName] IN (N'Ephyra', N'Hanover')
ORDER BY [g].[Nickname], [g].[SquadId], [g].[FullName]",
                //
                @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId], [t].[Nickname], [t].[SquadId], [t].[FullName]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[FullName]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND [g0].[CityOrBirthName] IN (N'Ephyra', N'Hanover')
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
WHERE ([g.Weapons].[Name] <> N'Lancer') OR [g.Weapons].[Name] IS NULL
ORDER BY [t].[Nickname], [t].[SquadId], [t].[FullName]");
        }

        public override async Task Select_correlated_filtered_collection_with_composite_key(bool isAsync)
        {
            await base.Select_correlated_filtered_collection_with_composite_key(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId]",
                //
                @"SELECT [g.Reports].[Nickname], [g.Reports].[SquadId], [g.Reports].[AssignedCityName], [g.Reports].[CityOrBirthName], [g.Reports].[Discriminator], [g.Reports].[FullName], [g.Reports].[HasSoulPatch], [g.Reports].[LeaderNickname], [g.Reports].[LeaderSquadId], [g.Reports].[Rank], [t].[Nickname], [t].[SquadId]
FROM [Gears] AS [g.Reports]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] = N'Officer'
) AS [t] ON ([g.Reports].[LeaderNickname] = [t].[Nickname]) AND ([g.Reports].[LeaderSquadId] = [t].[SquadId])
WHERE [g.Reports].[Discriminator] IN (N'Officer', N'Gear') AND ([g.Reports].[Nickname] <> N'Dom')
ORDER BY [t].[Nickname], [t].[SquadId]");
        }

        public override async Task Select_correlated_filtered_collection_works_with_caching(bool isAsync)
        {
            await base.Select_correlated_filtered_collection_works_with_caching(isAsync);

            AssertContainsSql(
                @"SELECT [t].[GearNickName]
FROM [Tags] AS [t]
ORDER BY [t].[Note]",
                //
                @"@_outer_GearNickName='Baird' (Size = 450)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] = @_outer_GearNickName)",
                //
                @"@_outer_GearNickName='Cole Train' (Size = 450)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] = @_outer_GearNickName)",
                //
                @"@_outer_GearNickName='Dom' (Size = 450)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] = @_outer_GearNickName)",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND [g].[Nickname] IS NULL",
                //
                @"@_outer_GearNickName='Marcus' (Size = 450)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] = @_outer_GearNickName)",
                //
                @"@_outer_GearNickName='Paduk' (Size = 450)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] = @_outer_GearNickName)");
        }

        public override async Task Join_predicate_value_equals_condition(bool isAsync)
        {
            await base.Join_predicate_value_equals_condition(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
INNER JOIN [Weapons] AS [w] ON [w].[SynergyWithId] IS NOT NULL
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Join_predicate_value(bool isAsync)
        {
            await base.Join_predicate_value(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
INNER JOIN [Weapons] AS [w] ON [g].[HasSoulPatch] = 1
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Join_predicate_condition_equals_condition(bool isAsync)
        {
            await base.Join_predicate_condition_equals_condition(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
INNER JOIN [Weapons] AS [w] ON CASE
    WHEN [g].[FullName] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN [w].[SynergyWithId] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Left_join_predicate_value_equals_condition(bool isAsync)
        {
            await base.Left_join_predicate_value_equals_condition(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Weapons] AS [w] ON [w].[SynergyWithId] IS NOT NULL
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Left_join_predicate_value(bool isAsync)
        {
            await base.Left_join_predicate_value(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Weapons] AS [w] ON [g].[HasSoulPatch] = 1
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Left_join_predicate_condition_equals_condition(bool isAsync)
        {
            await base.Left_join_predicate_condition_equals_condition(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Weapons] AS [w] ON CASE
    WHEN [g].[FullName] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN [w].[SynergyWithId] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Where_datetimeoffset_now(bool isAsync)
        {
            await base.Where_datetimeoffset_now(isAsync);

            AssertSql(
                @"SELECT [m].[Id], [m].[CodeName], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE [m].[Timeline] <> SYSDATETIMEOFFSET()");
        }

        public override async Task Where_datetimeoffset_utcnow(bool isAsync)
        {
            await base.Where_datetimeoffset_utcnow(isAsync);

            AssertSql(
                @"SELECT [m].[Id], [m].[CodeName], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE [m].[Timeline] <> CAST(SYSUTCDATETIME() AS datetimeoffset)");
        }

        public override async Task Where_datetimeoffset_date_component(bool isAsync)
        {
            await base.Where_datetimeoffset_date_component(isAsync);

            AssertSql(
                @"@__Date_0='0001-01-01T00:00:00'

SELECT [m].[Id], [m].[CodeName], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE CONVERT(date, [m].[Timeline]) > @__Date_0");
        }

        public override async Task Where_datetimeoffset_year_component(bool isAsync)
        {
            await base.Where_datetimeoffset_year_component(isAsync);

            AssertSql(
                @"SELECT [m].[Id], [m].[CodeName], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(year, [m].[Timeline]) = 2");
        }

        public override async Task Where_datetimeoffset_month_component(bool isAsync)
        {
            await base.Where_datetimeoffset_month_component(isAsync);

            AssertSql(
                @"SELECT [m].[Id], [m].[CodeName], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(month, [m].[Timeline]) = 1");
        }

        public override async Task Where_datetimeoffset_dayofyear_component(bool isAsync)
        {
            await base.Where_datetimeoffset_dayofyear_component(isAsync);

            AssertSql(
                @"SELECT [m].[Id], [m].[CodeName], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(dayofyear, [m].[Timeline]) = 2");
        }

        public override async Task Where_datetimeoffset_day_component(bool isAsync)
        {
            await base.Where_datetimeoffset_day_component(isAsync);

            AssertSql(
                @"SELECT [m].[Id], [m].[CodeName], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(day, [m].[Timeline]) = 2");
        }

        public override async Task Where_datetimeoffset_hour_component(bool isAsync)
        {
            await base.Where_datetimeoffset_hour_component(isAsync);

            AssertSql(
                @"SELECT [m].[Id], [m].[CodeName], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(hour, [m].[Timeline]) = 10");
        }

        public override async Task Where_datetimeoffset_minute_component(bool isAsync)
        {
            await base.Where_datetimeoffset_minute_component(isAsync);

            AssertSql(
                @"SELECT [m].[Id], [m].[CodeName], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(minute, [m].[Timeline]) = 0");
        }

        public override async Task Where_datetimeoffset_second_component(bool isAsync)
        {
            await base.Where_datetimeoffset_second_component(isAsync);

            AssertSql(
                @"SELECT [m].[Id], [m].[CodeName], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(second, [m].[Timeline]) = 0");
        }

        public override async Task Where_datetimeoffset_millisecond_component(bool isAsync)
        {
            await base.Where_datetimeoffset_millisecond_component(isAsync);

            AssertSql(
                @"SELECT [m].[Id], [m].[CodeName], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(millisecond, [m].[Timeline]) = 0");
        }

        public override async Task DateTimeOffset_DateAdd_AddMonths(bool isAsync)
        {
            await base.DateTimeOffset_DateAdd_AddMonths(isAsync);

            AssertSql(
                @"SELECT DATEADD(month, 1, [m].[Timeline])
FROM [Missions] AS [m]");
        }

        public override async Task DateTimeOffset_DateAdd_AddDays(bool isAsync)
        {
            await base.DateTimeOffset_DateAdd_AddDays(isAsync);

            AssertSql(
                @"SELECT DATEADD(day, 1.0E0, [m].[Timeline])
FROM [Missions] AS [m]");
        }

        public override async Task DateTimeOffset_DateAdd_AddHours(bool isAsync)
        {
            await base.DateTimeOffset_DateAdd_AddHours(isAsync);

            AssertSql(
                @"SELECT DATEADD(hour, 1.0E0, [m].[Timeline])
FROM [Missions] AS [m]");
        }

        public override async Task DateTimeOffset_DateAdd_AddMinutes(bool isAsync)
        {
            await base.DateTimeOffset_DateAdd_AddMinutes(isAsync);

            AssertSql(
                @"SELECT DATEADD(minute, 1.0E0, [m].[Timeline])
FROM [Missions] AS [m]");
        }

        public override async Task DateTimeOffset_DateAdd_AddSeconds(bool isAsync)
        {
            await base.DateTimeOffset_DateAdd_AddSeconds(isAsync);

            AssertSql(
                @"SELECT DATEADD(second, 1.0E0, [m].[Timeline])
FROM [Missions] AS [m]");
        }

        public override async Task DateTimeOffset_DateAdd_AddMilliseconds(bool isAsync)
        {
            await base.DateTimeOffset_DateAdd_AddMilliseconds(isAsync);

            AssertSql(
                @"SELECT DATEADD(millisecond, 300.0E0, [m].[Timeline])
FROM [Missions] AS [m]");
        }

        public override async Task Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used(bool isAsync)
        {
            await base.Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used(isAsync);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOrBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gears] AS [g]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname]
ORDER BY [t].[GearNickName]");
        }

        public override async Task Complex_predicate_with_AndAlso_and_nullable_bool_property(bool isAsync)
        {
            await base.Complex_predicate_with_AndAlso_and_nullable_bool_property(isAsync);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN (
    SELECT [w.Owner].*
    FROM [Gears] AS [w.Owner]
    WHERE [w.Owner].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [w].[OwnerFullName] = [t].[FullName]
WHERE ([w].[Id] <> 50) AND ([t].[HasSoulPatch] = 0)");
        }

        public override async Task Distinct_with_optional_navigation_is_translated_to_sql(bool isAsync)
        {
            await base.Distinct_with_optional_navigation_is_translated_to_sql(isAsync);

            AssertSql(
                @"SELECT DISTINCT [g].[HasSoulPatch]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g.Tag].[Note] <> N'Foo') OR [g.Tag].[Note] IS NULL)");
        }

        public override async Task Sum_with_optional_navigation_is_translated_to_sql(bool isAsync)
        {
            await base.Sum_with_optional_navigation_is_translated_to_sql(isAsync);

            AssertSql(
                @"SELECT SUM([g].[SquadId])
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g.Tag].[Note] <> N'Foo') OR [g.Tag].[Note] IS NULL)");
        }

        public override async Task Count_with_optional_navigation_is_translated_to_sql(bool isAsync)
        {
            await base.Count_with_optional_navigation_is_translated_to_sql(isAsync);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g.Tag].[Note] <> N'Foo') OR [g.Tag].[Note] IS NULL)");
        }

        public override async Task Count_with_unflattened_groupjoin_is_evaluated_on_client(bool isAsync)
        {
            await base.Count_with_unflattened_groupjoin_is_evaluated_on_client(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname], [g].[SquadId]");
        }

        public override async Task Distinct_with_unflattened_groupjoin_is_evaluated_on_client(bool isAsync)
        {
            await base.Distinct_with_unflattened_groupjoin_is_evaluated_on_client(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname], [g].[SquadId]");
        }

        public override async Task FirstOrDefault_with_manually_created_groupjoin_is_translated_to_sql(bool isAsync)
        {
            await base.FirstOrDefault_with_manually_created_groupjoin_is_translated_to_sql(isAsync);

            AssertSql(
                @"SELECT TOP(1) [s].[Id], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [g].*
    FROM [Gears] AS [g]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [s].[Id] = [t].[SquadId]
WHERE [s].[Name] = N'Kilo'");
        }

        public override async Task Any_with_optional_navigation_as_subquery_predicate_is_translated_to_sql(bool isAsync)
        {
            await base.Any_with_optional_navigation_as_subquery_predicate_is_translated_to_sql(isAsync);

            AssertSql(
                @"SELECT [s].[Name]
FROM [Squads] AS [s]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Gears] AS [m]
    LEFT JOIN [Tags] AS [m.Tag] ON ([m].[Nickname] = [m.Tag].[GearNickName]) AND ([m].[SquadId] = [m.Tag].[GearSquadId])
    WHERE ([m].[Discriminator] IN (N'Officer', N'Gear') AND ([m.Tag].[Note] = N'Dom''s Tag')) AND ([s].[Id] = [m].[SquadId]))");
        }

        public override async Task All_with_optional_navigation_is_translated_to_sql(bool isAsync)
        {
            await base.All_with_optional_navigation_is_translated_to_sql(isAsync);

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Gears] AS [g]
        LEFT JOIN [Tags] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
        WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g.Tag].[Note] = N'Foo'))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override async Task Non_flattened_GroupJoin_with_result_operator_evaluates_on_the_client(bool isAsync)
        {
            await base.Non_flattened_GroupJoin_with_result_operator_evaluates_on_the_client(isAsync);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOrBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gears] AS [g]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
ORDER BY [t].[GearNickName], [t].[GearSquadId]");
        }

        public override async Task Client_side_equality_with_parameter_works_with_optional_navigations(bool isAsync)
        {
            await base.Client_side_equality_with_parameter_works_with_optional_navigations(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Tag].[Note]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Contains_with_local_nullable_guid_list_closure(bool isAsync)
        {
            await base.Contains_with_local_nullable_guid_list_closure(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[GearNickName], [e].[GearSquadId], [e].[Note]
FROM [Tags] AS [e]
WHERE [e].[Id] IN ('d2c26679-562b-44d1-ab96-23d1775e0926', '23cbcf9b-ce14-45cf-aafa-2c2667ebfdd3', 'ab1b82d7-88db-42bd-a132-7eef9aa68af4')");
        }

        public override void Unnecessary_include_doesnt_get_added_complex_when_projecting_EF_Property()
        {
            base.Unnecessary_include_doesnt_get_added_complex_when_projecting_EF_Property();

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)
ORDER BY [g].[Rank]");
        }

        public override void Order_by_is_properly_lifted_from_subquery_created_by_include()
        {
            base.Order_by_is_properly_lifted_from_subquery_created_by_include();

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 0)
ORDER BY [g].[FullName], [g].[Rank]");
        }

        public override void Order_by_then_by_is_properly_lifted_from_subquery_created_by_include()
        {
            base.Order_by_then_by_is_properly_lifted_from_subquery_created_by_include();

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 0)
ORDER BY [g].[FullName], [g].[Rank], [g].[Nickname] DESC");
        }

        public override void Multiple_order_bys_are_properly_lifted_from_subquery_created_by_include()
        {
            base.Multiple_order_bys_are_properly_lifted_from_subquery_created_by_include();

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 0)
ORDER BY [g].[FullName], [g].[Nickname] DESC, [g].[Rank]");
        }

        public override void Order_by_is_properly_lifted_from_subquery_with_same_order_by_in_the_outer_query()
        {
            base.Order_by_is_properly_lifted_from_subquery_with_same_order_by_in_the_outer_query();

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 0)
ORDER BY [g].[FullName]");
        }

        public override void Where_is_properly_lifted_from_subquery_created_by_include()
        {
            base.Where_is_properly_lifted_from_subquery_created_by_include();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Tag].[Id], [g.Tag].[GearNickName], [g.Tag].[GearSquadId], [g.Tag].[Note]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE ([g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[FullName] <> N'Augustus Cole')) AND ([g].[HasSoulPatch] = 0)
ORDER BY [g].[FullName]");
        }

        public override async Task Where_and_order_by_are_properly_lifted_from_subquery_created_by_tracking(bool isAsync)
        {
            await base.Where_and_order_by_are_properly_lifted_from_subquery_created_by_tracking(isAsync);

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE ([g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[FullName] <> N'Augustus Cole')) AND ([g].[HasSoulPatch] = 0)
ORDER BY [g].[FullName], [g].[Rank]");
        }

        public override void Subquery_is_lifted_from_main_from_clause_of_SelectMany()
        {
            base.Subquery_is_lifted_from_main_from_clause_of_SelectMany();

            AssertSql(
                @"SELECT [g].[FullName] AS [Name1], [g2].[FullName] AS [Name2]
FROM [Gears] AS [g]
CROSS JOIN [Gears] AS [g2]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[HasSoulPatch] = 1) AND ([g2].[HasSoulPatch] = 0))
ORDER BY [Name1], [g].[Rank]");
        }

        public override async Task Subquery_containing_SelectMany_projecting_main_from_clause_gets_lifted(bool isAsync)
        {
            await base.Subquery_containing_SelectMany_projecting_main_from_clause_gets_lifted(isAsync);

            AssertSql(
                @"SELECT [gear].[FullName]
FROM [Gears] AS [gear]
CROSS JOIN [Tags] AS [tag]
WHERE [gear].[Discriminator] IN (N'Officer', N'Gear') AND ([gear].[HasSoulPatch] = 1)
ORDER BY [gear].[FullName], [tag].[Note]");
        }

        public override async Task Subquery_containing_join_projecting_main_from_clause_gets_lifted(bool isAsync)
        {
            await base.Subquery_containing_join_projecting_main_from_clause_gets_lifted(isAsync);

            AssertSql(
                @"SELECT [gear].[Nickname]
FROM [Gears] AS [gear]
INNER JOIN [Tags] AS [tag] ON [gear].[Nickname] = [tag].[GearNickName]
WHERE [gear].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [gear].[Nickname], [tag].[Note]");
        }

        public override async Task Subquery_containing_left_join_projecting_main_from_clause_gets_lifted(bool isAsync)
        {
            await base.Subquery_containing_left_join_projecting_main_from_clause_gets_lifted(isAsync);

            AssertSql(
                @"SELECT [gear].[Nickname]
FROM [Gears] AS [gear]
LEFT JOIN [Tags] AS [tag] ON [gear].[Nickname] = [tag].[GearNickName]
WHERE [gear].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [gear].[Nickname], [gear].[Rank]");
        }

        public override async Task Subquery_containing_join_gets_lifted_clashing_names(bool isAsync)
        {
            await base.Subquery_containing_join_gets_lifted_clashing_names(isAsync);

            AssertSql(
                @"SELECT [gear].[Nickname]
FROM [Gears] AS [gear]
INNER JOIN [Tags] AS [tag] ON [gear].[Nickname] = [tag].[GearNickName]
INNER JOIN [Tags] AS [tag0] ON [gear].[Nickname] = [tag0].[GearNickName]
WHERE [gear].[Discriminator] IN (N'Officer', N'Gear') AND (([tag].[GearNickName] <> N'Cole Train') OR [tag].[GearNickName] IS NULL)
ORDER BY [gear].[Nickname], [tag0].[Id], [tag].[Note]");
        }

        public override void Subquery_created_by_include_gets_lifted_nested()
        {
            base.Subquery_created_by_include_gets_lifted_nested();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.CityOfBirth].[Name], [g.CityOfBirth].[Location]
FROM [Gears] AS [g]
INNER JOIN [Cities] AS [g.CityOfBirth] ON [g].[CityOrBirthName] = [g.CityOfBirth].[Name]
WHERE ([g].[Discriminator] IN (N'Officer', N'Gear') AND EXISTS (
    SELECT 1
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName])) AND ([g].[HasSoulPatch] = 0)
ORDER BY [g].[Nickname], [g].[Rank]");
        }

        public override async Task Subquery_is_not_lifted_from_additional_from_clause(bool isAsync)
        {
            await base.Subquery_is_not_lifted_from_additional_from_clause(isAsync);

            AssertSql(
                @"SELECT [g1].[FullName] AS [Name1]
FROM [Gears] AS [g1]
WHERE [g1].[Discriminator] IN (N'Officer', N'Gear') AND ([g1].[HasSoulPatch] = 1)
ORDER BY [Name1]",
                //
                @"SELECT [g0].[HasSoulPatch], [g0].[FullName]
FROM [Gears] AS [g0]
WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g0].[Rank]",
                //
                @"SELECT [g0].[HasSoulPatch], [g0].[FullName]
FROM [Gears] AS [g0]
WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g0].[Rank]");
        }

        public override async Task Subquery_with_result_operator_is_not_lifted(bool isAsync)
        {
            await base.Subquery_with_result_operator_is_not_lifted(isAsync);

            AssertSql(
                @"@__p_0='2'

SELECT [t].[FullName]
FROM (
    SELECT TOP(@__p_0) [g].*
    FROM [Gears] AS [g]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 0)
    ORDER BY [g].[FullName]
) AS [t]
ORDER BY [t].[Rank]");
        }

        public override async Task Select_length_of_string_property(bool isAsync)
        {
            await base.Select_length_of_string_property(isAsync);

            AssertSql(
                @"SELECT [w].[Name], CAST(LEN([w].[Name]) AS int) AS [Length]
FROM [Weapons] AS [w]");
        }

        public override async Task Client_method_on_collection_navigation_in_predicate(bool isAsync)
        {
            await base.Client_method_on_collection_navigation_in_predicate(isAsync);

            AssertSql(
                @"SELECT [g].[FullName], [g].[Nickname]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)",
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

        public override async Task Client_method_on_collection_navigation_in_predicate_accessed_by_ef_property(bool isAsync)
        {
            await base.Client_method_on_collection_navigation_in_predicate_accessed_by_ef_property(isAsync);

            AssertSql(
                @"SELECT [g].[FullName], [g].[Nickname]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 0)",
                //
                @"@_outer_FullName='Augustus Cole' (Size = 450)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE @_outer_FullName = [w].[OwnerFullName]",
                //
                @"@_outer_FullName='Dominic Santiago' (Size = 450)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE @_outer_FullName = [w].[OwnerFullName]",
                //
                @"@_outer_FullName='Garron Paduk' (Size = 450)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE @_outer_FullName = [w].[OwnerFullName]");
        }

        public override async Task Client_method_on_collection_navigation_in_order_by(bool isAsync)
        {
            await base.Client_method_on_collection_navigation_in_order_by(isAsync);

            AssertSql(
                @"SELECT [g].[FullName], [g].[Nickname]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 0)",
                //
                @"@_outer_FullName='Augustus Cole' (Size = 450)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE @_outer_FullName = [w].[OwnerFullName]",
                //
                @"@_outer_FullName='Dominic Santiago' (Size = 450)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE @_outer_FullName = [w].[OwnerFullName]",
                //
                @"@_outer_FullName='Garron Paduk' (Size = 450)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE @_outer_FullName = [w].[OwnerFullName]");
        }

        public override async Task Client_method_on_collection_navigation_in_additional_from_clause(bool isAsync)
        {
            await base.Client_method_on_collection_navigation_in_additional_from_clause(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname] AS [g], [g].[SquadId]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] = N'Officer'",
                //
                @"@_outer_Nickname='Baird' (Size = 450)
@_outer_SquadId='1'

SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOrBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gears] AS [g0]
WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND ((@_outer_Nickname = [g0].[LeaderNickname]) AND (@_outer_SquadId = [g0].[LeaderSquadId]))",
                //
                @"@_outer_Nickname='Marcus' (Size = 450)
@_outer_SquadId='1'

SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOrBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gears] AS [g0]
WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND ((@_outer_Nickname = [g0].[LeaderNickname]) AND (@_outer_SquadId = [g0].[LeaderSquadId]))");
        }

        public override async Task Client_method_on_collection_navigation_in_outer_join_key(bool isAsync)
        {
            await base.Client_method_on_collection_navigation_in_outer_join_key(isAsync);

            AssertContainsSql(
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

        public override void Member_access_on_derived_entity_using_cast()
        {
            base.Member_access_on_derived_entity_using_cast();

            AssertSql(
                @"SELECT [f].[Name], [f].[Eradicated]
FROM [Factions] AS [f]
WHERE ([f].[Discriminator] = N'LocustHorde') AND ([f].[Discriminator] = N'LocustHorde')
ORDER BY [f].[Name]");
        }

        public override void Member_access_on_derived_materialized_entity_using_cast()
        {
            base.Member_access_on_derived_materialized_entity_using_cast();

            AssertSql(
                @"SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated]
FROM [Factions] AS [f]
WHERE ([f].[Discriminator] = N'LocustHorde') AND ([f].[Discriminator] = N'LocustHorde')
ORDER BY [f].[Name]");
        }

        public override void Member_access_on_derived_entity_using_cast_and_let()
        {
            base.Member_access_on_derived_entity_using_cast_and_let();

            AssertSql(
                @"SELECT [f].[Name], [f].[Eradicated]
FROM [Factions] AS [f]
WHERE ([f].[Discriminator] = N'LocustHorde') AND ([f].[Discriminator] = N'LocustHorde')
ORDER BY [f].[Name]");
        }

        public override void Property_access_on_derived_entity_using_cast()
        {
            base.Property_access_on_derived_entity_using_cast();

            AssertSql(
                @"SELECT [f].[Name], [f].[Eradicated]
FROM [Factions] AS [f]
WHERE ([f].[Discriminator] = N'LocustHorde') AND ([f].[Discriminator] = N'LocustHorde')
ORDER BY [f].[Name]");
        }

        public override void Navigation_access_on_derived_entity_using_cast()
        {
            base.Navigation_access_on_derived_entity_using_cast();

            AssertSql(
                @"SELECT [f].[Name], [t].[ThreatLevel] AS [Threat]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [f.Commander].*
    FROM [LocustLeaders] AS [f.Commander]
    WHERE [f.Commander].[Discriminator] = N'LocustCommander'
) AS [t] ON ([f].[Discriminator] = N'LocustHorde') AND ([f].[CommanderName] = [t].[Name])
WHERE ([f].[Discriminator] = N'LocustHorde') AND ([f].[Discriminator] = N'LocustHorde')
ORDER BY [f].[Name]");
        }

        public override void Navigation_access_on_derived_materialized_entity_using_cast()
        {
            base.Navigation_access_on_derived_materialized_entity_using_cast();

            AssertSql(
                @"SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated], [t].[ThreatLevel]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [f.Commander].*
    FROM [LocustLeaders] AS [f.Commander]
    WHERE [f.Commander].[Discriminator] = N'LocustCommander'
) AS [t] ON ([f].[Discriminator] = N'LocustHorde') AND ([f].[CommanderName] = [t].[Name])
WHERE ([f].[Discriminator] = N'LocustHorde') AND ([f].[Discriminator] = N'LocustHorde')
ORDER BY [f].[Name]");
        }

        public override void Navigation_access_via_EFProperty_on_derived_entity_using_cast()
        {
            base.Navigation_access_via_EFProperty_on_derived_entity_using_cast();

            AssertSql(
                @"SELECT [f].[Name], [t].[ThreatLevel] AS [Threat]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [f.Commander].*
    FROM [LocustLeaders] AS [f.Commander]
    WHERE [f.Commander].[Discriminator] = N'LocustCommander'
) AS [t] ON ([f].[Discriminator] = N'LocustHorde') AND ([f].[CommanderName] = [t].[Name])
WHERE ([f].[Discriminator] = N'LocustHorde') AND ([f].[Discriminator] = N'LocustHorde')
ORDER BY [f].[Name]");
        }

        public override void Navigation_access_fk_on_derived_entity_using_cast()
        {
            base.Navigation_access_fk_on_derived_entity_using_cast();

            AssertSql(
                @"SELECT [f].[Name], [f].[CommanderName]
FROM [Factions] AS [f]
WHERE ([f].[Discriminator] = N'LocustHorde') AND ([f].[Discriminator] = N'LocustHorde')
ORDER BY [f].[Name]");
        }

        public override void Collection_navigation_access_on_derived_entity_using_cast()
        {
            base.Collection_navigation_access_on_derived_entity_using_cast();

            AssertSql(
                @"SELECT [f].[Name], (
    SELECT COUNT(*)
    FROM [LocustLeaders] AS [l]
    WHERE [l].[Discriminator] IN (N'LocustCommander', N'LocustLeader') AND ([f].[Id] = [l].[LocustHordeId])
) AS [LeadersCount]
FROM [Factions] AS [f]
WHERE ([f].[Discriminator] = N'LocustHorde') AND ([f].[Discriminator] = N'LocustHorde')
ORDER BY [f].[Name]");
        }

        public override void Collection_navigation_access_on_derived_entity_using_cast_in_SelectMany()
        {
            base.Collection_navigation_access_on_derived_entity_using_cast_in_SelectMany();

            AssertSql(
                @"SELECT [f].[Name] AS [Name0], [f.Leaders].[Name] AS [LeaderName]
FROM [Factions] AS [f]
INNER JOIN [LocustLeaders] AS [f.Leaders] ON [f].[Id] = [f.Leaders].[LocustHordeId]
WHERE (([f].[Discriminator] = N'LocustHorde') AND ([f].[Discriminator] = N'LocustHorde')) AND [f.Leaders].[Discriminator] IN (N'LocustCommander', N'LocustLeader')
ORDER BY [LeaderName]");
        }

        public override void Include_on_derived_entity_using_OfType()
        {
            base.Include_on_derived_entity_using_OfType();

            AssertSql(
                @"SELECT [lh].[Id], [lh].[CapitalName], [lh].[Discriminator], [lh].[Name], [lh].[CommanderName], [lh].[Eradicated], [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[ThreatLevel], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId]
FROM [Factions] AS [lh]
LEFT JOIN (
    SELECT [lh.Commander].*
    FROM [LocustLeaders] AS [lh.Commander]
    WHERE [lh.Commander].[Discriminator] = N'LocustCommander'
) AS [t] ON [lh].[CommanderName] = [t].[Name]
WHERE [lh].[Discriminator] = N'LocustHorde'
ORDER BY [lh].[Name], [lh].[Id]",
                //
                @"SELECT [lh.Leaders].[Name], [lh.Leaders].[Discriminator], [lh.Leaders].[LocustHordeId], [lh.Leaders].[ThreatLevel], [lh.Leaders].[DefeatedByNickname], [lh.Leaders].[DefeatedBySquadId], [lh.Leaders].[HighCommandId]
FROM [LocustLeaders] AS [lh.Leaders]
INNER JOIN (
    SELECT DISTINCT [lh0].[Id], [lh0].[Name]
    FROM [Factions] AS [lh0]
    LEFT JOIN (
        SELECT [lh.Commander0].*
        FROM [LocustLeaders] AS [lh.Commander0]
        WHERE [lh.Commander0].[Discriminator] = N'LocustCommander'
    ) AS [t0] ON [lh0].[CommanderName] = [t0].[Name]
    WHERE [lh0].[Discriminator] = N'LocustHorde'
) AS [t1] ON [lh.Leaders].[LocustHordeId] = [t1].[Id]
WHERE [lh.Leaders].[Discriminator] IN (N'LocustCommander', N'LocustLeader')
ORDER BY [t1].[Name], [t1].[Id]");
        }

        public override void Include_on_derived_entity_using_subquery_with_cast()
        {
            base.Include_on_derived_entity_using_subquery_with_cast();

            AssertSql(
                @"SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated], [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[ThreatLevel], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [f.Commander].*
    FROM [LocustLeaders] AS [f.Commander]
    WHERE [f.Commander].[Discriminator] = N'LocustCommander'
) AS [t] ON ([f].[Discriminator] = N'LocustHorde') AND ([f].[CommanderName] = [t].[Name])
WHERE ([f].[Discriminator] = N'LocustHorde') AND ([f].[Discriminator] = N'LocustHorde')
ORDER BY [f].[Name], [f].[Id]",
                //
                @"SELECT [f.Leaders].[Name], [f.Leaders].[Discriminator], [f.Leaders].[LocustHordeId], [f.Leaders].[ThreatLevel], [f.Leaders].[DefeatedByNickname], [f.Leaders].[DefeatedBySquadId], [f.Leaders].[HighCommandId]
FROM [LocustLeaders] AS [f.Leaders]
INNER JOIN (
    SELECT DISTINCT [f0].[Id], [f0].[Name]
    FROM [Factions] AS [f0]
    LEFT JOIN (
        SELECT [f.Commander0].*
        FROM [LocustLeaders] AS [f.Commander0]
        WHERE [f.Commander0].[Discriminator] = N'LocustCommander'
    ) AS [t0] ON ([f0].[Discriminator] = N'LocustHorde') AND ([f0].[CommanderName] = [t0].[Name])
    WHERE ([f0].[Discriminator] = N'LocustHorde') AND ([f0].[Discriminator] = N'LocustHorde')
) AS [t1] ON [f.Leaders].[LocustHordeId] = [t1].[Id]
WHERE [f.Leaders].[Discriminator] IN (N'LocustCommander', N'LocustLeader')
ORDER BY [t1].[Name], [t1].[Id]");
        }

        public override void Include_on_derived_entity_using_subquery_with_cast_AsNoTracking()
        {
            base.Include_on_derived_entity_using_subquery_with_cast_AsNoTracking();

            AssertSql(
                @"SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated], [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[ThreatLevel], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [f.Commander].*
    FROM [LocustLeaders] AS [f.Commander]
    WHERE [f.Commander].[Discriminator] = N'LocustCommander'
) AS [t] ON ([f].[Discriminator] = N'LocustHorde') AND ([f].[CommanderName] = [t].[Name])
WHERE ([f].[Discriminator] = N'LocustHorde') AND ([f].[Discriminator] = N'LocustHorde')
ORDER BY [f].[Name], [f].[Id]",
                //
                @"SELECT [f.Leaders].[Name], [f.Leaders].[Discriminator], [f.Leaders].[LocustHordeId], [f.Leaders].[ThreatLevel], [f.Leaders].[DefeatedByNickname], [f.Leaders].[DefeatedBySquadId], [f.Leaders].[HighCommandId]
FROM [LocustLeaders] AS [f.Leaders]
INNER JOIN (
    SELECT DISTINCT [f0].[Id], [f0].[Name]
    FROM [Factions] AS [f0]
    LEFT JOIN (
        SELECT [f.Commander0].*
        FROM [LocustLeaders] AS [f.Commander0]
        WHERE [f.Commander0].[Discriminator] = N'LocustCommander'
    ) AS [t0] ON ([f0].[Discriminator] = N'LocustHorde') AND ([f0].[CommanderName] = [t0].[Name])
    WHERE ([f0].[Discriminator] = N'LocustHorde') AND ([f0].[Discriminator] = N'LocustHorde')
) AS [t1] ON [f.Leaders].[LocustHordeId] = [t1].[Id]
WHERE [f.Leaders].[Discriminator] IN (N'LocustCommander', N'LocustLeader')
ORDER BY [t1].[Name], [t1].[Id]");
        }

        public override void Include_on_derived_entity_using_subquery_with_cast_cross_product_base_entity()
        {
            base.Include_on_derived_entity_using_subquery_with_cast_cross_product_base_entity();

            AssertSql(
                @"SELECT [f2].[Id], [f2].[CapitalName], [f2].[Discriminator], [f2].[Name], [f2].[CommanderName], [f2].[Eradicated], [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[ThreatLevel], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [ff].[Id], [ff].[CapitalName], [ff].[Discriminator], [ff].[Name], [ff].[CommanderName], [ff].[Eradicated], [ff.Capital].[Name], [ff.Capital].[Location]
FROM [Factions] AS [f2]
LEFT JOIN (
    SELECT [f2.Commander].*
    FROM [LocustLeaders] AS [f2.Commander]
    WHERE [f2.Commander].[Discriminator] = N'LocustCommander'
) AS [t] ON ([f2].[Discriminator] = N'LocustHorde') AND ([f2].[CommanderName] = [t].[Name])
CROSS JOIN [Factions] AS [ff]
LEFT JOIN [Cities] AS [ff.Capital] ON [ff].[CapitalName] = [ff.Capital].[Name]
WHERE ([f2].[Discriminator] = N'LocustHorde') AND ([f2].[Discriminator] = N'LocustHorde')
ORDER BY [f2].[Name], [ff].[Name], [f2].[Id]",
                //
                @"SELECT [f2.Leaders].[Name], [f2.Leaders].[Discriminator], [f2.Leaders].[LocustHordeId], [f2.Leaders].[ThreatLevel], [f2.Leaders].[DefeatedByNickname], [f2.Leaders].[DefeatedBySquadId], [f2.Leaders].[HighCommandId]
FROM [LocustLeaders] AS [f2.Leaders]
INNER JOIN (
    SELECT DISTINCT [f20].[Id], [f20].[Name], [ff0].[Name] AS [Name0]
    FROM [Factions] AS [f20]
    LEFT JOIN (
        SELECT [f2.Commander0].*
        FROM [LocustLeaders] AS [f2.Commander0]
        WHERE [f2.Commander0].[Discriminator] = N'LocustCommander'
    ) AS [t0] ON ([f20].[Discriminator] = N'LocustHorde') AND ([f20].[CommanderName] = [t0].[Name])
    CROSS JOIN [Factions] AS [ff0]
    LEFT JOIN [Cities] AS [ff.Capital0] ON [ff0].[CapitalName] = [ff.Capital0].[Name]
    WHERE ([f20].[Discriminator] = N'LocustHorde') AND ([f20].[Discriminator] = N'LocustHorde')
) AS [t1] ON [f2.Leaders].[LocustHordeId] = [t1].[Id]
WHERE [f2.Leaders].[Discriminator] IN (N'LocustCommander', N'LocustLeader')
ORDER BY [t1].[Name], [t1].[Name0], [t1].[Id]");
        }

        public override void Distinct_on_subquery_doesnt_get_lifted()
        {
            base.Distinct_on_subquery_doesnt_get_lifted();

            AssertSql(
                @"SELECT [t].[HasSoulPatch]
FROM (
    SELECT DISTINCT [ig].*
    FROM [Gears] AS [ig]
    WHERE [ig].[Discriminator] IN (N'Officer', N'Gear')
) AS [t]");
        }

        public override void Cast_result_operator_on_subquery_is_properly_lifted_to_a_convert()
        {
            base.Cast_result_operator_on_subquery_is_properly_lifted_to_a_convert();

            AssertSql(
                @"SELECT [f].[Eradicated]
FROM [Factions] AS [f]
WHERE [f].[Discriminator] = N'LocustHorde'");
        }

        public override void Comparing_two_collection_navigations_composite_key()
        {
            base.Comparing_two_collection_navigations_composite_key();

            AssertSql(
                @"SELECT [g1].[Nickname] AS [Nickname1], [g2].[Nickname] AS [Nickname2]
FROM [Gears] AS [g1]
CROSS JOIN [Gears] AS [g2]
WHERE [g1].[Discriminator] IN (N'Officer', N'Gear') AND (([g1].[Nickname] = [g2].[Nickname]) AND ([g1].[SquadId] = [g2].[SquadId]))
ORDER BY [Nickname1]");
        }

        public override void Comparing_two_collection_navigations_inheritance()
        {
            base.Comparing_two_collection_navigations_inheritance();

            AssertSql(
                @"SELECT [f].[Name], [o].[Nickname]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [f.Commander].*
    FROM [LocustLeaders] AS [f.Commander]
    WHERE [f.Commander].[Discriminator] = N'LocustCommander'
) AS [t] ON ([f].[Discriminator] = N'LocustHorde') AND ([f].[CommanderName] = [t].[Name])
LEFT JOIN (
    SELECT [f.Commander.DefeatedBy].*
    FROM [Gears] AS [f.Commander.DefeatedBy]
    WHERE [f.Commander.DefeatedBy].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[DefeatedByNickname] = [t0].[Nickname]) AND ([t].[DefeatedBySquadId] = [t0].[SquadId])
CROSS JOIN [Gears] AS [o]
WHERE (([f].[Discriminator] = N'LocustHorde') AND (([f].[Discriminator] = N'LocustHorde') AND ([o].[HasSoulPatch] = 1))) AND (([t0].[Nickname] = [o].[Nickname]) AND ([t0].[SquadId] = [o].[SquadId]))");
        }

        public override void Comparing_entities_using_Equals_inheritance()
        {
            base.Comparing_entities_using_Equals_inheritance();

            AssertSql(
                "");
        }

        public override void Contains_on_nullable_array_produces_correct_sql()
        {
            base.Contains_on_nullable_array_produces_correct_sql();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[SquadId] < 2) AND ([g].[AssignedCityName] IN (N'Ephyra') OR [g].[AssignedCityName] IS NULL))");
        }

        public override void Optional_navigation_with_collection_composite_key()
        {
            base.Optional_navigation_with_collection_composite_key();

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE ([t0].[Discriminator] = N'Officer') AND ((
    SELECT COUNT(*)
    FROM [Gears] AS [r]
    WHERE ([r].[Discriminator] IN (N'Officer', N'Gear') AND ([r].[Nickname] = N'Dom')) AND (([t0].[Nickname] = [r].[LeaderNickname]) AND ([t0].[SquadId] = [r].[LeaderSquadId]))
) > 0)");
        }

        public override void Select_null_conditional_with_inheritance()
        {
            base.Select_null_conditional_with_inheritance();

            AssertSql(
                @"SELECT [f].[CommanderName]
FROM [Factions] AS [f]
WHERE ([f].[Discriminator] = N'LocustHorde') AND ([f].[Discriminator] = N'LocustHorde')");
        }

        public override void Select_null_conditional_with_inheritance_negative()
        {
            base.Select_null_conditional_with_inheritance_negative();

            AssertSql(
                @"SELECT CASE
    WHEN [f].[CommanderName] IS NOT NULL
    THEN [f].[Eradicated] ELSE NULL
END
FROM [Factions] AS [f]
WHERE ([f].[Discriminator] = N'LocustHorde') AND ([f].[Discriminator] = N'LocustHorde')");
        }

        public override void Project_collection_navigation_with_inheritance1()
        {
            base.Project_collection_navigation_with_inheritance1();

            AssertSql(
                @"SELECT [h].[Id], [t0].[Id]
FROM [Factions] AS [h]
LEFT JOIN (
    SELECT [h.Commander].*
    FROM [LocustLeaders] AS [h.Commander]
    WHERE [h.Commander].[Discriminator] = N'LocustCommander'
) AS [t] ON [h].[CommanderName] = [t].[Name]
LEFT JOIN (
    SELECT [h.Commander.CommandingFaction].*
    FROM [Factions] AS [h.Commander.CommandingFaction]
    WHERE [h.Commander.CommandingFaction].[Discriminator] = N'LocustHorde'
) AS [t0] ON [t].[Name] = [t0].[CommanderName]
WHERE [h].[Discriminator] = N'LocustHorde'
ORDER BY [h].[Id], [t0].[Id]",
                //
                @"SELECT [h.Commander.CommandingFaction.Leaders].[Name], [h.Commander.CommandingFaction.Leaders].[Discriminator], [h.Commander.CommandingFaction.Leaders].[LocustHordeId], [h.Commander.CommandingFaction.Leaders].[ThreatLevel], [h.Commander.CommandingFaction.Leaders].[DefeatedByNickname], [h.Commander.CommandingFaction.Leaders].[DefeatedBySquadId], [h.Commander.CommandingFaction.Leaders].[HighCommandId], [t3].[Id], [t3].[Id0]
FROM [LocustLeaders] AS [h.Commander.CommandingFaction.Leaders]
INNER JOIN (
    SELECT [h0].[Id], [t2].[Id] AS [Id0]
    FROM [Factions] AS [h0]
    LEFT JOIN (
        SELECT [h.Commander0].*
        FROM [LocustLeaders] AS [h.Commander0]
        WHERE [h.Commander0].[Discriminator] = N'LocustCommander'
    ) AS [t1] ON [h0].[CommanderName] = [t1].[Name]
    LEFT JOIN (
        SELECT [h.Commander.CommandingFaction0].*
        FROM [Factions] AS [h.Commander.CommandingFaction0]
        WHERE [h.Commander.CommandingFaction0].[Discriminator] = N'LocustHorde'
    ) AS [t2] ON [t1].[Name] = [t2].[CommanderName]
    WHERE [h0].[Discriminator] = N'LocustHorde'
) AS [t3] ON [h.Commander.CommandingFaction.Leaders].[LocustHordeId] = [t3].[Id]
WHERE [h.Commander.CommandingFaction.Leaders].[Discriminator] IN (N'LocustCommander', N'LocustLeader')
ORDER BY [t3].[Id], [t3].[Id0]");
        }

        public override void Project_collection_navigation_with_inheritance2()
        {
            base.Project_collection_navigation_with_inheritance2();

            AssertSql(
                @"SELECT [h].[Id], [t0].[Nickname], [t0].[SquadId]
FROM [Factions] AS [h]
LEFT JOIN (
    SELECT [h.Commander].*
    FROM [LocustLeaders] AS [h.Commander]
    WHERE [h.Commander].[Discriminator] = N'LocustCommander'
) AS [t] ON [h].[CommanderName] = [t].[Name]
LEFT JOIN (
    SELECT [h.Commander.DefeatedBy].*
    FROM [Gears] AS [h.Commander.DefeatedBy]
    WHERE [h.Commander.DefeatedBy].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[DefeatedByNickname] = [t0].[Nickname]) AND ([t].[DefeatedBySquadId] = [t0].[SquadId])
WHERE [h].[Discriminator] = N'LocustHorde'
ORDER BY [h].[Id], [t0].[Nickname], [t0].[SquadId]",
                //
                @"SELECT [h.Commander.DefeatedBy.Reports].[Nickname], [h.Commander.DefeatedBy.Reports].[SquadId], [h.Commander.DefeatedBy.Reports].[AssignedCityName], [h.Commander.DefeatedBy.Reports].[CityOrBirthName], [h.Commander.DefeatedBy.Reports].[Discriminator], [h.Commander.DefeatedBy.Reports].[FullName], [h.Commander.DefeatedBy.Reports].[HasSoulPatch], [h.Commander.DefeatedBy.Reports].[LeaderNickname], [h.Commander.DefeatedBy.Reports].[LeaderSquadId], [h.Commander.DefeatedBy.Reports].[Rank], [t3].[Id], [t3].[Nickname], [t3].[SquadId]
FROM [Gears] AS [h.Commander.DefeatedBy.Reports]
INNER JOIN (
    SELECT [h0].[Id], [t2].[Nickname], [t2].[SquadId]
    FROM [Factions] AS [h0]
    LEFT JOIN (
        SELECT [h.Commander0].*
        FROM [LocustLeaders] AS [h.Commander0]
        WHERE [h.Commander0].[Discriminator] = N'LocustCommander'
    ) AS [t1] ON [h0].[CommanderName] = [t1].[Name]
    LEFT JOIN (
        SELECT [h.Commander.DefeatedBy0].*
        FROM [Gears] AS [h.Commander.DefeatedBy0]
        WHERE [h.Commander.DefeatedBy0].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t2] ON ([t1].[DefeatedByNickname] = [t2].[Nickname]) AND ([t1].[DefeatedBySquadId] = [t2].[SquadId])
    WHERE [h0].[Discriminator] = N'LocustHorde'
) AS [t3] ON ([h.Commander.DefeatedBy.Reports].[LeaderNickname] = [t3].[Nickname]) AND ([h.Commander.DefeatedBy.Reports].[LeaderSquadId] = [t3].[SquadId])
WHERE [h.Commander.DefeatedBy.Reports].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t3].[Id], [t3].[Nickname], [t3].[SquadId]");
        }

        public override void Project_collection_navigation_with_inheritance3()
        {
            base.Project_collection_navigation_with_inheritance3();

            AssertSql(
                @"SELECT [f].[Id], [t0].[Nickname], [t0].[SquadId]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [f.Commander].*
    FROM [LocustLeaders] AS [f.Commander]
    WHERE [f.Commander].[Discriminator] = N'LocustCommander'
) AS [t] ON ([f].[Discriminator] = N'LocustHorde') AND ([f].[CommanderName] = [t].[Name])
LEFT JOIN (
    SELECT [f.Commander.DefeatedBy].*
    FROM [Gears] AS [f.Commander.DefeatedBy]
    WHERE [f.Commander.DefeatedBy].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[DefeatedByNickname] = [t0].[Nickname]) AND ([t].[DefeatedBySquadId] = [t0].[SquadId])
WHERE ([f].[Discriminator] = N'LocustHorde') AND ([f].[Discriminator] = N'LocustHorde')
ORDER BY [f].[Id], [t0].[Nickname], [t0].[SquadId]",
                //
                @"SELECT [f.Commander.DefeatedBy.Reports].[Nickname], [f.Commander.DefeatedBy.Reports].[SquadId], [f.Commander.DefeatedBy.Reports].[AssignedCityName], [f.Commander.DefeatedBy.Reports].[CityOrBirthName], [f.Commander.DefeatedBy.Reports].[Discriminator], [f.Commander.DefeatedBy.Reports].[FullName], [f.Commander.DefeatedBy.Reports].[HasSoulPatch], [f.Commander.DefeatedBy.Reports].[LeaderNickname], [f.Commander.DefeatedBy.Reports].[LeaderSquadId], [f.Commander.DefeatedBy.Reports].[Rank], [t3].[Id], [t3].[Nickname], [t3].[SquadId]
FROM [Gears] AS [f.Commander.DefeatedBy.Reports]
INNER JOIN (
    SELECT [f0].[Id], [t2].[Nickname], [t2].[SquadId]
    FROM [Factions] AS [f0]
    LEFT JOIN (
        SELECT [f.Commander0].*
        FROM [LocustLeaders] AS [f.Commander0]
        WHERE [f.Commander0].[Discriminator] = N'LocustCommander'
    ) AS [t1] ON ([f0].[Discriminator] = N'LocustHorde') AND ([f0].[CommanderName] = [t1].[Name])
    LEFT JOIN (
        SELECT [f.Commander.DefeatedBy0].*
        FROM [Gears] AS [f.Commander.DefeatedBy0]
        WHERE [f.Commander.DefeatedBy0].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t2] ON ([t1].[DefeatedByNickname] = [t2].[Nickname]) AND ([t1].[DefeatedBySquadId] = [t2].[SquadId])
    WHERE ([f0].[Discriminator] = N'LocustHorde') AND ([f0].[Discriminator] = N'LocustHorde')
) AS [t3] ON ([f.Commander.DefeatedBy.Reports].[LeaderNickname] = [t3].[Nickname]) AND ([f.Commander.DefeatedBy.Reports].[LeaderSquadId] = [t3].[SquadId])
WHERE [f.Commander.DefeatedBy.Reports].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t3].[Id], [t3].[Nickname], [t3].[SquadId]");
        }

        public override async Task Include_reference_on_derived_type_using_string(bool isAsync)
        {
            await base.Include_reference_on_derived_type_using_string(isAsync);

            AssertSql(
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [LocustLeaders] AS [l]
LEFT JOIN (
    SELECT [l.DefeatedBy].*
    FROM [Gears] AS [l.DefeatedBy]
    WHERE [l.DefeatedBy].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON (([l].[Discriminator] = N'LocustCommander') AND ([l].[DefeatedByNickname] = [t].[Nickname])) AND (([l].[Discriminator] = N'LocustCommander') AND ([l].[DefeatedBySquadId] = [t].[SquadId]))
WHERE [l].[Discriminator] IN (N'LocustCommander', N'LocustLeader')");
        }

        public override async Task Include_reference_on_derived_type_using_string_nested1(bool isAsync)
        {
            await base.Include_reference_on_derived_type_using_string_nested1(isAsync);

            AssertSql(
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [l.DefeatedBy.Squad].[Id], [l.DefeatedBy.Squad].[InternalNumber], [l.DefeatedBy.Squad].[Name]
FROM [LocustLeaders] AS [l]
LEFT JOIN (
    SELECT [l.DefeatedBy].*
    FROM [Gears] AS [l.DefeatedBy]
    WHERE [l.DefeatedBy].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON (([l].[Discriminator] = N'LocustCommander') AND ([l].[DefeatedByNickname] = [t].[Nickname])) AND (([l].[Discriminator] = N'LocustCommander') AND ([l].[DefeatedBySquadId] = [t].[SquadId]))
LEFT JOIN [Squads] AS [l.DefeatedBy.Squad] ON [t].[SquadId] = [l.DefeatedBy.Squad].[Id]
WHERE [l].[Discriminator] IN (N'LocustCommander', N'LocustLeader')");
        }

        public override async Task Include_reference_on_derived_type_using_string_nested2(bool isAsync)
        {
            await base.Include_reference_on_derived_type_using_string_nested2(isAsync);

            AssertSql(
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [LocustLeaders] AS [l]
LEFT JOIN (
    SELECT [l.DefeatedBy].*
    FROM [Gears] AS [l.DefeatedBy]
    WHERE [l.DefeatedBy].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON (([l].[Discriminator] = N'LocustCommander') AND ([l].[DefeatedByNickname] = [t].[Nickname])) AND (([l].[Discriminator] = N'LocustCommander') AND ([l].[DefeatedBySquadId] = [t].[SquadId]))
WHERE [l].[Discriminator] IN (N'LocustCommander', N'LocustLeader')
ORDER BY [t].[Nickname], [t].[SquadId]",
                //
                @"SELECT [l.DefeatedBy.Reports].[Nickname], [l.DefeatedBy.Reports].[SquadId], [l.DefeatedBy.Reports].[AssignedCityName], [l.DefeatedBy.Reports].[CityOrBirthName], [l.DefeatedBy.Reports].[Discriminator], [l.DefeatedBy.Reports].[FullName], [l.DefeatedBy.Reports].[HasSoulPatch], [l.DefeatedBy.Reports].[LeaderNickname], [l.DefeatedBy.Reports].[LeaderSquadId], [l.DefeatedBy.Reports].[Rank], [g.CityOfBirth].[Name], [g.CityOfBirth].[Location]
FROM [Gears] AS [l.DefeatedBy.Reports]
INNER JOIN [Cities] AS [g.CityOfBirth] ON [l.DefeatedBy.Reports].[CityOrBirthName] = [g.CityOfBirth].[Name]
INNER JOIN (
    SELECT DISTINCT [t0].[Nickname], [t0].[SquadId]
    FROM [LocustLeaders] AS [l0]
    LEFT JOIN (
        SELECT [l.DefeatedBy0].*
        FROM [Gears] AS [l.DefeatedBy0]
        WHERE [l.DefeatedBy0].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t0] ON (([l0].[Discriminator] = N'LocustCommander') AND ([l0].[DefeatedByNickname] = [t0].[Nickname])) AND (([l0].[Discriminator] = N'LocustCommander') AND ([l0].[DefeatedBySquadId] = [t0].[SquadId]))
    WHERE [l0].[Discriminator] IN (N'LocustCommander', N'LocustLeader')
) AS [t1] ON ([l.DefeatedBy.Reports].[LeaderNickname] = [t1].[Nickname]) AND ([l.DefeatedBy.Reports].[LeaderSquadId] = [t1].[SquadId])
WHERE [l.DefeatedBy.Reports].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t1].[Nickname], [t1].[SquadId]");
        }

        public override async Task Include_reference_on_derived_type_using_lambda(bool isAsync)
        {
            await base.Include_reference_on_derived_type_using_lambda(isAsync);

            AssertSql(
                @"SELECT [ll].[Name], [ll].[Discriminator], [ll].[LocustHordeId], [ll].[ThreatLevel], [ll].[DefeatedByNickname], [ll].[DefeatedBySquadId], [ll].[HighCommandId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [LocustLeaders] AS [ll]
LEFT JOIN (
    SELECT [ll.DefeatedBy].*
    FROM [Gears] AS [ll.DefeatedBy]
    WHERE [ll.DefeatedBy].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON (([ll].[Discriminator] = N'LocustCommander') AND ([ll].[DefeatedByNickname] = [t].[Nickname])) AND (([ll].[Discriminator] = N'LocustCommander') AND ([ll].[DefeatedBySquadId] = [t].[SquadId]))
WHERE [ll].[Discriminator] IN (N'LocustCommander', N'LocustLeader')");
        }

        public override async Task Include_reference_on_derived_type_using_lambda_with_soft_cast(bool isAsync)
        {
            await base.Include_reference_on_derived_type_using_lambda_with_soft_cast(isAsync);

            AssertSql(
                @"SELECT [ll].[Name], [ll].[Discriminator], [ll].[LocustHordeId], [ll].[ThreatLevel], [ll].[DefeatedByNickname], [ll].[DefeatedBySquadId], [ll].[HighCommandId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [LocustLeaders] AS [ll]
LEFT JOIN (
    SELECT [ll.DefeatedBy].*
    FROM [Gears] AS [ll.DefeatedBy]
    WHERE [ll.DefeatedBy].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON (([ll].[Discriminator] = N'LocustCommander') AND ([ll].[DefeatedByNickname] = [t].[Nickname])) AND (([ll].[Discriminator] = N'LocustCommander') AND ([ll].[DefeatedBySquadId] = [t].[SquadId]))
WHERE [ll].[Discriminator] IN (N'LocustCommander', N'LocustLeader')");
        }

        public override async Task Include_reference_on_derived_type_using_lambda_with_tracking(bool isAsync)
        {
            await base.Include_reference_on_derived_type_using_lambda_with_tracking(isAsync);

            AssertSql(
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [LocustLeaders] AS [l]
LEFT JOIN (
    SELECT [l.DefeatedBy].*
    FROM [Gears] AS [l.DefeatedBy]
    WHERE [l.DefeatedBy].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON (([l].[Discriminator] = N'LocustCommander') AND ([l].[DefeatedByNickname] = [t].[Nickname])) AND (([l].[Discriminator] = N'LocustCommander') AND ([l].[DefeatedBySquadId] = [t].[SquadId]))
WHERE [l].[Discriminator] IN (N'LocustCommander', N'LocustLeader')");
        }

        public override async Task Include_collection_on_derived_type_using_string(bool isAsync)
        {
            await base.Include_collection_on_derived_type_using_string(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname], [g].[SquadId]",
                //
                @"SELECT [o.Reports].[Nickname], [o.Reports].[SquadId], [o.Reports].[AssignedCityName], [o.Reports].[CityOrBirthName], [o.Reports].[Discriminator], [o.Reports].[FullName], [o.Reports].[HasSoulPatch], [o.Reports].[LeaderNickname], [o.Reports].[LeaderSquadId], [o.Reports].[Rank]
FROM [Gears] AS [o.Reports]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([o.Reports].[LeaderNickname] = [t].[Nickname]) AND ([o.Reports].[LeaderSquadId] = [t].[SquadId])
WHERE [o.Reports].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[Nickname], [t].[SquadId]");
        }

        public override async Task Include_collection_on_derived_type_using_lambda(bool isAsync)
        {
            await base.Include_collection_on_derived_type_using_lambda(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname], [g].[SquadId]",
                //
                @"SELECT [g.Reports].[Nickname], [g.Reports].[SquadId], [g.Reports].[AssignedCityName], [g.Reports].[CityOrBirthName], [g.Reports].[Discriminator], [g.Reports].[FullName], [g.Reports].[HasSoulPatch], [g.Reports].[LeaderNickname], [g.Reports].[LeaderSquadId], [g.Reports].[Rank]
FROM [Gears] AS [g.Reports]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([g.Reports].[LeaderNickname] = [t].[Nickname]) AND ([g.Reports].[LeaderSquadId] = [t].[SquadId])
WHERE [g.Reports].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[Nickname], [t].[SquadId]");
        }

        public override async Task Include_collection_on_derived_type_using_lambda_with_soft_cast(bool isAsync)
        {
            await base.Include_collection_on_derived_type_using_lambda_with_soft_cast(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname], [g].[SquadId]",
                //
                @"SELECT [g.Reports].[Nickname], [g.Reports].[SquadId], [g.Reports].[AssignedCityName], [g.Reports].[CityOrBirthName], [g.Reports].[Discriminator], [g.Reports].[FullName], [g.Reports].[HasSoulPatch], [g.Reports].[LeaderNickname], [g.Reports].[LeaderSquadId], [g.Reports].[Rank]
FROM [Gears] AS [g.Reports]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([g.Reports].[LeaderNickname] = [t].[Nickname]) AND ([g.Reports].[LeaderSquadId] = [t].[SquadId])
WHERE [g.Reports].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[Nickname], [t].[SquadId]");
        }

        public override async Task Include_base_navigation_on_derived_entity(bool isAsync)
        {
            await base.Include_base_navigation_on_derived_entity(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Tag].[Id], [g.Tag].[GearNickName], [g.Tag].[GearSquadId], [g.Tag].[Note]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[FullName]",
                //
                @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT DISTINCT [g0].[FullName]
    FROM [Gears] AS [g0]
    LEFT JOIN [Tags] AS [g.Tag0] ON ([g0].[Nickname] = [g.Tag0].[GearNickName]) AND ([g0].[SquadId] = [g.Tag0].[GearSquadId])
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
ORDER BY [t].[FullName]");
        }

        public override async Task ThenInclude_collection_on_derived_after_base_reference(bool isAsync)
        {
            await base.ThenInclude_collection_on_derived_after_base_reference(isAsync);

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOrBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
ORDER BY [t0].[FullName]",
                //
                @"SELECT [t.Gear.Weapons].[Id], [t.Gear.Weapons].[AmmunitionType], [t.Gear.Weapons].[IsAutomatic], [t.Gear.Weapons].[Name], [t.Gear.Weapons].[OwnerFullName], [t.Gear.Weapons].[SynergyWithId]
FROM [Weapons] AS [t.Gear.Weapons]
INNER JOIN (
    SELECT DISTINCT [t2].[FullName]
    FROM [Tags] AS [t1]
    LEFT JOIN (
        SELECT [t.Gear0].*
        FROM [Gears] AS [t.Gear0]
        WHERE [t.Gear0].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t2] ON ([t1].[GearNickName] = [t2].[Nickname]) AND ([t1].[GearSquadId] = [t2].[SquadId])
) AS [t3] ON [t.Gear.Weapons].[OwnerFullName] = [t3].[FullName]
ORDER BY [t3].[FullName]");
        }

        public override async Task ThenInclude_collection_on_derived_after_derived_reference(bool isAsync)
        {
            await base.ThenInclude_collection_on_derived_after_derived_reference(isAsync);

            AssertSql(
                @"SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated], [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[ThreatLevel], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOrBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [f.Commander].*
    FROM [LocustLeaders] AS [f.Commander]
    WHERE [f.Commander].[Discriminator] = N'LocustCommander'
) AS [t] ON ([f].[Discriminator] = N'LocustHorde') AND ([f].[CommanderName] = [t].[Name])
LEFT JOIN (
    SELECT [f.Commander.DefeatedBy].*
    FROM [Gears] AS [f.Commander.DefeatedBy]
    WHERE [f.Commander.DefeatedBy].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[DefeatedByNickname] = [t0].[Nickname]) AND ([t].[DefeatedBySquadId] = [t0].[SquadId])
WHERE [f].[Discriminator] = N'LocustHorde'
ORDER BY [t0].[Nickname], [t0].[SquadId]",
                //
                @"SELECT [f.Commander.DefeatedBy.Reports].[Nickname], [f.Commander.DefeatedBy.Reports].[SquadId], [f.Commander.DefeatedBy.Reports].[AssignedCityName], [f.Commander.DefeatedBy.Reports].[CityOrBirthName], [f.Commander.DefeatedBy.Reports].[Discriminator], [f.Commander.DefeatedBy.Reports].[FullName], [f.Commander.DefeatedBy.Reports].[HasSoulPatch], [f.Commander.DefeatedBy.Reports].[LeaderNickname], [f.Commander.DefeatedBy.Reports].[LeaderSquadId], [f.Commander.DefeatedBy.Reports].[Rank]
FROM [Gears] AS [f.Commander.DefeatedBy.Reports]
INNER JOIN (
    SELECT DISTINCT [t2].[Nickname], [t2].[SquadId]
    FROM [Factions] AS [f0]
    LEFT JOIN (
        SELECT [f.Commander0].*
        FROM [LocustLeaders] AS [f.Commander0]
        WHERE [f.Commander0].[Discriminator] = N'LocustCommander'
    ) AS [t1] ON ([f0].[Discriminator] = N'LocustHorde') AND ([f0].[CommanderName] = [t1].[Name])
    LEFT JOIN (
        SELECT [f.Commander.DefeatedBy0].*
        FROM [Gears] AS [f.Commander.DefeatedBy0]
        WHERE [f.Commander.DefeatedBy0].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t2] ON ([t1].[DefeatedByNickname] = [t2].[Nickname]) AND ([t1].[DefeatedBySquadId] = [t2].[SquadId])
    WHERE [f0].[Discriminator] = N'LocustHorde'
) AS [t3] ON ([f.Commander.DefeatedBy.Reports].[LeaderNickname] = [t3].[Nickname]) AND ([f.Commander.DefeatedBy.Reports].[LeaderSquadId] = [t3].[SquadId])
WHERE [f.Commander.DefeatedBy.Reports].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t3].[Nickname], [t3].[SquadId]");
        }

        public override async Task ThenInclude_collection_on_derived_after_derived_collection(bool isAsync)
        {
            await base.ThenInclude_collection_on_derived_after_derived_collection(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname], [g].[SquadId]",
                //
                @"SELECT [g.Reports].[Nickname], [g.Reports].[SquadId], [g.Reports].[AssignedCityName], [g.Reports].[CityOrBirthName], [g.Reports].[Discriminator], [g.Reports].[FullName], [g.Reports].[HasSoulPatch], [g.Reports].[LeaderNickname], [g.Reports].[LeaderSquadId], [g.Reports].[Rank]
FROM [Gears] AS [g.Reports]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([g.Reports].[LeaderNickname] = [t].[Nickname]) AND ([g.Reports].[LeaderSquadId] = [t].[SquadId])
WHERE [g.Reports].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[Nickname], [t].[SquadId], [g.Reports].[Nickname], [g.Reports].[SquadId]",
                //
                @"SELECT [g.Reports.Reports].[Nickname], [g.Reports.Reports].[SquadId], [g.Reports.Reports].[AssignedCityName], [g.Reports.Reports].[CityOrBirthName], [g.Reports.Reports].[Discriminator], [g.Reports.Reports].[FullName], [g.Reports.Reports].[HasSoulPatch], [g.Reports.Reports].[LeaderNickname], [g.Reports.Reports].[LeaderSquadId], [g.Reports.Reports].[Rank]
FROM [Gears] AS [g.Reports.Reports]
INNER JOIN (
    SELECT DISTINCT [g.Reports0].[Nickname], [g.Reports0].[SquadId], [t0].[Nickname] AS [Nickname0], [t0].[SquadId] AS [SquadId0]
    FROM [Gears] AS [g.Reports0]
    INNER JOIN (
        SELECT [g1].[Nickname], [g1].[SquadId]
        FROM [Gears] AS [g1]
        WHERE [g1].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t0] ON ([g.Reports0].[LeaderNickname] = [t0].[Nickname]) AND ([g.Reports0].[LeaderSquadId] = [t0].[SquadId])
    WHERE [g.Reports0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t1] ON ([g.Reports.Reports].[LeaderNickname] = [t1].[Nickname]) AND ([g.Reports.Reports].[LeaderSquadId] = [t1].[SquadId])
WHERE [g.Reports.Reports].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t1].[Nickname0], [t1].[SquadId0], [t1].[Nickname], [t1].[SquadId]");
        }

        public override async Task ThenInclude_reference_on_derived_after_derived_collection(bool isAsync)
        {
            await base.ThenInclude_reference_on_derived_after_derived_collection(isAsync);

            AssertSql(
                @"SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated]
FROM [Factions] AS [f]
WHERE [f].[Discriminator] = N'LocustHorde'
ORDER BY [f].[Id]",
                //
                @"SELECT [f.Leaders].[Name], [f.Leaders].[Discriminator], [f.Leaders].[LocustHordeId], [f.Leaders].[ThreatLevel], [f.Leaders].[DefeatedByNickname], [f.Leaders].[DefeatedBySquadId], [f.Leaders].[HighCommandId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [LocustLeaders] AS [f.Leaders]
LEFT JOIN (
    SELECT [l.DefeatedBy].*
    FROM [Gears] AS [l.DefeatedBy]
    WHERE [l.DefeatedBy].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON (([f.Leaders].[Discriminator] = N'LocustCommander') AND ([f.Leaders].[DefeatedByNickname] = [t].[Nickname])) AND (([f.Leaders].[Discriminator] = N'LocustCommander') AND ([f.Leaders].[DefeatedBySquadId] = [t].[SquadId]))
INNER JOIN (
    SELECT [f0].[Id]
    FROM [Factions] AS [f0]
    WHERE [f0].[Discriminator] = N'LocustHorde'
) AS [t0] ON [f.Leaders].[LocustHordeId] = [t0].[Id]
WHERE [f.Leaders].[Discriminator] IN (N'LocustCommander', N'LocustLeader')
ORDER BY [t0].[Id]");
        }

        public override async Task Multiple_derived_included_on_one_method(bool isAsync)
        {
            await base.Multiple_derived_included_on_one_method(isAsync);

            AssertSql(
                @"SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated], [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[ThreatLevel], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOrBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [f.Commander].*
    FROM [LocustLeaders] AS [f.Commander]
    WHERE [f.Commander].[Discriminator] = N'LocustCommander'
) AS [t] ON ([f].[Discriminator] = N'LocustHorde') AND ([f].[CommanderName] = [t].[Name])
LEFT JOIN (
    SELECT [f.Commander.DefeatedBy].*
    FROM [Gears] AS [f.Commander.DefeatedBy]
    WHERE [f.Commander.DefeatedBy].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[DefeatedByNickname] = [t0].[Nickname]) AND ([t].[DefeatedBySquadId] = [t0].[SquadId])
WHERE [f].[Discriminator] = N'LocustHorde'
ORDER BY [t0].[Nickname], [t0].[SquadId]",
                //
                @"SELECT [f.Commander.DefeatedBy.Reports].[Nickname], [f.Commander.DefeatedBy.Reports].[SquadId], [f.Commander.DefeatedBy.Reports].[AssignedCityName], [f.Commander.DefeatedBy.Reports].[CityOrBirthName], [f.Commander.DefeatedBy.Reports].[Discriminator], [f.Commander.DefeatedBy.Reports].[FullName], [f.Commander.DefeatedBy.Reports].[HasSoulPatch], [f.Commander.DefeatedBy.Reports].[LeaderNickname], [f.Commander.DefeatedBy.Reports].[LeaderSquadId], [f.Commander.DefeatedBy.Reports].[Rank]
FROM [Gears] AS [f.Commander.DefeatedBy.Reports]
INNER JOIN (
    SELECT DISTINCT [t2].[Nickname], [t2].[SquadId]
    FROM [Factions] AS [f0]
    LEFT JOIN (
        SELECT [f.Commander0].*
        FROM [LocustLeaders] AS [f.Commander0]
        WHERE [f.Commander0].[Discriminator] = N'LocustCommander'
    ) AS [t1] ON ([f0].[Discriminator] = N'LocustHorde') AND ([f0].[CommanderName] = [t1].[Name])
    LEFT JOIN (
        SELECT [f.Commander.DefeatedBy0].*
        FROM [Gears] AS [f.Commander.DefeatedBy0]
        WHERE [f.Commander.DefeatedBy0].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t2] ON ([t1].[DefeatedByNickname] = [t2].[Nickname]) AND ([t1].[DefeatedBySquadId] = [t2].[SquadId])
    WHERE [f0].[Discriminator] = N'LocustHorde'
) AS [t3] ON ([f.Commander.DefeatedBy.Reports].[LeaderNickname] = [t3].[Nickname]) AND ([f.Commander.DefeatedBy.Reports].[LeaderSquadId] = [t3].[SquadId])
WHERE [f.Commander.DefeatedBy.Reports].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t3].[Nickname], [t3].[SquadId]");
        }

        public override async Task Include_on_derived_multi_level(bool isAsync)
        {
            await base.Include_on_derived_multi_level(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname], [g].[SquadId]",
                //
                @"SELECT [g.Reports].[Nickname], [g.Reports].[SquadId], [g.Reports].[AssignedCityName], [g.Reports].[CityOrBirthName], [g.Reports].[Discriminator], [g.Reports].[FullName], [g.Reports].[HasSoulPatch], [g.Reports].[LeaderNickname], [g.Reports].[LeaderSquadId], [g.Reports].[Rank], [g.Squad].[Id], [g.Squad].[InternalNumber], [g.Squad].[Name]
FROM [Gears] AS [g.Reports]
INNER JOIN [Squads] AS [g.Squad] ON [g.Reports].[SquadId] = [g.Squad].[Id]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([g.Reports].[LeaderNickname] = [t].[Nickname]) AND ([g.Reports].[LeaderSquadId] = [t].[SquadId])
WHERE [g.Reports].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[Nickname], [t].[SquadId], [g.Squad].[Id]",
                //
                @"SELECT [g.Squad.Missions].[SquadId], [g.Squad.Missions].[MissionId]
FROM [SquadMissions] AS [g.Squad.Missions]
INNER JOIN (
    SELECT DISTINCT [g.Squad0].[Id], [t0].[Nickname], [t0].[SquadId]
    FROM [Gears] AS [g.Reports0]
    INNER JOIN [Squads] AS [g.Squad0] ON [g.Reports0].[SquadId] = [g.Squad0].[Id]
    INNER JOIN (
        SELECT [g1].[Nickname], [g1].[SquadId]
        FROM [Gears] AS [g1]
        WHERE [g1].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t0] ON ([g.Reports0].[LeaderNickname] = [t0].[Nickname]) AND ([g.Reports0].[LeaderSquadId] = [t0].[SquadId])
    WHERE [g.Reports0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t1] ON [g.Squad.Missions].[SquadId] = [t1].[Id]
ORDER BY [t1].[Nickname], [t1].[SquadId], [t1].[Id]");
        }

        public override async Task Projecting_nullable_bool_in_conditional_works(bool isAsync)
        {
            await base.Projecting_nullable_bool_in_conditional_works(isAsync);

            AssertSql(
                @"SELECT CASE
    WHEN [cg].[GearNickName] IS NOT NULL OR [cg].[GearSquadId] IS NOT NULL
    THEN [t].[HasSoulPatch] ELSE CAST(0 AS BIT)
END AS [Prop]
FROM [Tags] AS [cg]
LEFT JOIN (
    SELECT [cg.Gear].*
    FROM [Gears] AS [cg.Gear]
    WHERE [cg.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([cg].[GearNickName] = [t].[Nickname]) AND ([cg].[GearSquadId] = [t].[SquadId])");
        }

        public override async Task Enum_ToString_is_client_eval(bool isAsync)
        {
            await base.Enum_ToString_is_client_eval(isAsync);

            AssertSql(
                @"SELECT [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[SquadId], [g].[Nickname]");
        }

        public override async Task Correlated_collections_naked_navigation_with_ToList(bool isAsync)
        {
            await base.Correlated_collections_naked_navigation_with_ToList(isAsync);

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

        public override async Task Correlated_collections_naked_navigation_with_ToList_followed_by_projecting_count(bool isAsync)
        {
            await base.Correlated_collections_naked_navigation_with_ToList_followed_by_projecting_count(isAsync);

            AssertSql(
                @"SELECT (
    SELECT COUNT(*)
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
)
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] <> N'Marcus')
ORDER BY [g].[Nickname]");
        }

        public override async Task Correlated_collections_naked_navigation_with_ToArray(bool isAsync)
        {
            await base.Correlated_collections_naked_navigation_with_ToArray(isAsync);

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

        public override async Task Correlated_collections_basic_projection(bool isAsync)
        {
            await base.Correlated_collections_basic_projection(isAsync);

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

        public override async Task Correlated_collections_basic_projection_explicit_to_list(bool isAsync)
        {
            await base.Correlated_collections_basic_projection_explicit_to_list(isAsync);

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

        public override async Task Correlated_collections_basic_projection_explicit_to_array(bool isAsync)
        {
            await base.Correlated_collections_basic_projection_explicit_to_array(isAsync);

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

        public override async Task Correlated_collections_basic_projection_ordered(bool isAsync)
        {
            await base.Correlated_collections_basic_projection_ordered(isAsync);

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

        public override async Task Correlated_collections_basic_projection_composite_key(bool isAsync)
        {
            await base.Correlated_collections_basic_projection_composite_key(isAsync);

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

        public override async Task Correlated_collections_basic_projecting_single_property(bool isAsync)
        {
            await base.Correlated_collections_basic_projecting_single_property(isAsync);

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

        public override async Task Correlated_collections_basic_projecting_constant(bool isAsync)
        {
            await base.Correlated_collections_basic_projecting_constant(isAsync);

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] <> N'Marcus')
ORDER BY [g].[Nickname], [g].[SquadId], [g].[FullName]",
                //
                @"SELECT [t].[Nickname], [t].[SquadId], [t].[FullName], N'BFG', [g.Weapons].[OwnerFullName]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[FullName]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND ([g0].[Nickname] <> N'Marcus')
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
WHERE ([g.Weapons].[IsAutomatic] = 1) OR (([g.Weapons].[Name] <> N'foo') OR [g.Weapons].[Name] IS NULL)
ORDER BY [t].[Nickname], [t].[SquadId], [t].[FullName]");
        }

        public override async Task Correlated_collections_basic_projecting_constant_bool(bool isAsync)
        {
            await base.Correlated_collections_basic_projecting_constant_bool(isAsync);

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] <> N'Marcus')
ORDER BY [g].[Nickname], [g].[SquadId], [g].[FullName]",
                //
                @"SELECT [t].[Nickname], [t].[SquadId], [t].[FullName], CAST(1 AS bit), [g.Weapons].[OwnerFullName]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[FullName]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND ([g0].[Nickname] <> N'Marcus')
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
WHERE ([g.Weapons].[IsAutomatic] = 1) OR (([g.Weapons].[Name] <> N'foo') OR [g.Weapons].[Name] IS NULL)
ORDER BY [t].[Nickname], [t].[SquadId], [t].[FullName]");
        }

        public override async Task Correlated_collections_projection_of_collection_thru_navigation(bool isAsync)
        {
            await base.Correlated_collections_projection_of_collection_thru_navigation(isAsync);

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

        public override async Task Correlated_collections_project_anonymous_collection_result(bool isAsync)
        {
            await base.Correlated_collections_project_anonymous_collection_result(isAsync);

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

        public override async Task Correlated_collections_nested(bool isAsync)
        {
            await base.Correlated_collections_nested(isAsync);

            AssertSql(
                @"SELECT [s].[Id]
FROM [Squads] AS [s]
ORDER BY [s].[Id]",
                //
                @"SELECT [t].[Id], [s.Missions].[SquadId], [m.Mission].[Id]
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

        public override async Task Correlated_collections_nested_mixed_streaming_with_buffer1(bool isAsync)
        {
            await base.Correlated_collections_nested_mixed_streaming_with_buffer1(isAsync);

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

        public override async Task Correlated_collections_nested_mixed_streaming_with_buffer2(bool isAsync)
        {
            await base.Correlated_collections_nested_mixed_streaming_with_buffer2(isAsync);

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

        public override async Task Correlated_collections_nested_with_custom_ordering(bool isAsync)
        {
            await base.Correlated_collections_nested_with_custom_ordering(isAsync);

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

        public override async Task Correlated_collections_same_collection_projected_multiple_times(bool isAsync)
        {
            await base.Correlated_collections_same_collection_projected_multiple_times(isAsync);

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

        public override async Task Correlated_collections_similar_collection_projected_multiple_times(bool isAsync)
        {
            await base.Correlated_collections_similar_collection_projected_multiple_times(isAsync);

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

        public override async Task Correlated_collections_different_collections_projected(bool isAsync)
        {
            await base.Correlated_collections_different_collections_projected(isAsync);

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

        public override async Task Correlated_collections_multiple_nested_complex_collections(bool isAsync)
        {
            await base.Correlated_collections_multiple_nested_complex_collections(isAsync);

            AssertSql(
                @"SELECT [o].[FullName], [o].[Nickname], [o].[SquadId], [t].[FullName]
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
                @"SELECT [t5].[HasSoulPatch], [t5].[Note], [t5].[Nickname], [t5].[SquadId], [t5].[Rank], [t5].[Nickname0], [t5].[SquadId0], [t5].[FullName], [o.Reports.Weapons].[Id], [o.Reports.Weapons].[OwnerFullName], [t2].[FullName], [w.Owner.Squad].[Id]
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

        public override async Task Correlated_collections_inner_subquery_selector_references_outer_qsre(bool isAsync)
        {
            await base.Correlated_collections_inner_subquery_selector_references_outer_qsre(isAsync);

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

        public override async Task Correlated_collections_inner_subquery_predicate_references_outer_qsre(bool isAsync)
        {
            await base.Correlated_collections_inner_subquery_predicate_references_outer_qsre(isAsync);

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

        public override async Task Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up(bool isAsync)
        {
            await base.Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up(isAsync);

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

        public override async Task Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up(bool isAsync)
        {
            await base.Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up(isAsync);

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

        public override async Task Correlated_collections_on_select_many(bool isAsync)
        {
            await base.Correlated_collections_on_select_many(isAsync);

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

        public override async Task Correlated_collections_with_Skip(bool isAsync)
        {
            await base.Correlated_collections_with_Skip(isAsync);

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

        public override async Task Correlated_collections_with_Take(bool isAsync)
        {
            await base.Correlated_collections_with_Take(isAsync);

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

        public override async Task Correlated_collections_with_Distinct(bool isAsync)
        {
            await base.Correlated_collections_with_Distinct(isAsync);

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

        public override async Task Correlated_collections_with_FirstOrDefault(bool isAsync)
        {
            await base.Correlated_collections_with_FirstOrDefault(isAsync);

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

        public override async Task Correlated_collections_on_left_join_with_predicate(bool isAsync)
        {
            await base.Correlated_collections_on_left_join_with_predicate(isAsync);

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

        public override async Task Correlated_collections_on_left_join_with_null_value(bool isAsync)
        {
            await base.Correlated_collections_on_left_join_with_null_value(isAsync);

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

        public override async Task Correlated_collections_left_join_with_self_reference(bool isAsync)
        {
            await base.Correlated_collections_left_join_with_self_reference(isAsync);

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

        public override async Task Correlated_collections_deeply_nested_left_join(bool isAsync)
        {
            await base.Correlated_collections_deeply_nested_left_join(isAsync);

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
                @"SELECT [t3].[Note], [t3].[Nickname], [t3].[SquadId], [t3].[Id], [g.Squad.Members].[Nickname] AS [Nickname0], [g.Squad.Members].[SquadId], [g.Squad.Members].[FullName]
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

#if !Test21
        public override async Task Correlated_collections_from_left_join_with_additional_elements_projected_of_that_join(bool isAsync)
        {
            await base.Correlated_collections_from_left_join_with_additional_elements_projected_of_that_join(isAsync);

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
                @"SELECT [t1].[Name], [t1].[Id], [t1].[Id0], [w.Owner.Squad.Members].[Rank], [w.Owner.Squad.Members].[SquadId], [w.Owner.Squad.Members].[FullName]
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
#endif

        public override async Task Correlated_collections_complex_scenario1(bool isAsync)
        {
            await base.Correlated_collections_complex_scenario1(isAsync);

            AssertSql(
                @"SELECT [r].[FullName]
FROM [Gears] AS [r]
WHERE [r].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [r].[Nickname], [r].[SquadId], [r].[FullName]",
                //
                @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[FullName], [r.Weapons].[Id], [r.Weapons].[OwnerFullName], [w.Owner.Squad].[Id]
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

        public override async Task Correlated_collections_complex_scenario2(bool isAsync)
        {
            await base.Correlated_collections_complex_scenario2(isAsync);

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
                @"SELECT [t2].[Nickname], [t2].[SquadId], [t2].[Nickname0], [t2].[SquadId0], [t2].[FullName], [o.Reports.Weapons].[Id], [o.Reports.Weapons].[OwnerFullName], [w.Owner.Squad].[Id]
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

        public override async Task Correlated_collections_with_funky_orderby_complex_scenario1(bool isAsync)
        {
            await base.Correlated_collections_with_funky_orderby_complex_scenario1(isAsync);

            AssertSql(
                @"SELECT [r].[FullName]
FROM [Gears] AS [r]
WHERE [r].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [r].[FullName], [r].[Nickname] DESC, [r].[SquadId]",
                //
                @"SELECT [t0].[FullName], [t0].[Nickname], [t0].[SquadId], [r.Weapons].[Id], [r.Weapons].[OwnerFullName], [w.Owner.Squad].[Id]
FROM [Weapons] AS [r.Weapons]
LEFT JOIN (
    SELECT [w.Owner].*
    FROM [Gears] AS [w.Owner]
    WHERE [w.Owner].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [r.Weapons].[OwnerFullName] = [t].[FullName]
LEFT JOIN [Squads] AS [w.Owner.Squad] ON [t].[SquadId] = [w.Owner.Squad].[Id]
INNER JOIN (
    SELECT [r0].[FullName], [r0].[Nickname], [r0].[SquadId]
    FROM [Gears] AS [r0]
    WHERE [r0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON [r.Weapons].[OwnerFullName] = [t0].[FullName]
ORDER BY [t0].[FullName], [t0].[Nickname] DESC, [t0].[SquadId], [r.Weapons].[Id], [w.Owner.Squad].[Id]",
                //
                @"SELECT [t3].[FullName], [t3].[Nickname], [t3].[SquadId], [t3].[Id], [t3].[Id0], [w.Owner.Squad.Members].[Nickname] AS [Nickname0], [w.Owner.Squad.Members].[HasSoulPatch], [w.Owner.Squad.Members].[SquadId]
FROM [Gears] AS [w.Owner.Squad.Members]
INNER JOIN (
    SELECT [t2].[FullName], [t2].[Nickname], [t2].[SquadId], [r.Weapons0].[Id], [w.Owner.Squad0].[Id] AS [Id0]
    FROM [Weapons] AS [r.Weapons0]
    LEFT JOIN (
        SELECT [w.Owner0].*
        FROM [Gears] AS [w.Owner0]
        WHERE [w.Owner0].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t1] ON [r.Weapons0].[OwnerFullName] = [t1].[FullName]
    LEFT JOIN [Squads] AS [w.Owner.Squad0] ON [t1].[SquadId] = [w.Owner.Squad0].[Id]
    INNER JOIN (
        SELECT [r1].[FullName], [r1].[Nickname], [r1].[SquadId]
        FROM [Gears] AS [r1]
        WHERE [r1].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t2] ON [r.Weapons0].[OwnerFullName] = [t2].[FullName]
) AS [t3] ON [w.Owner.Squad.Members].[SquadId] = [t3].[Id0]
WHERE [w.Owner.Squad.Members].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t3].[FullName], [t3].[Nickname] DESC, [t3].[SquadId], [t3].[Id], [t3].[Id0], [Nickname0]");
        }

        public override async Task Correlated_collections_with_funky_orderby_complex_scenario2(bool isAsync)
        {
            await base.Correlated_collections_with_funky_orderby_complex_scenario2(isAsync);

            AssertSql(
                @"SELECT [o].[FullName], [o].[Nickname], [o].[SquadId]
FROM [Gears] AS [o]
WHERE [o].[Discriminator] = N'Officer'
ORDER BY [o].[HasSoulPatch], [o].[LeaderNickname], [o].[FullName], [o].[Nickname], [o].[SquadId]",
                //
                @"SELECT [t].[HasSoulPatch], [t].[LeaderNickname], [t].[FullName], [t].[Nickname], [t].[SquadId], [o.Reports].[FullName] AS [FullName0], [o.Reports].[LeaderNickname], [o.Reports].[LeaderSquadId]
FROM [Gears] AS [o.Reports]
INNER JOIN (
    SELECT [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[FullName], [o0].[Nickname], [o0].[SquadId]
    FROM [Gears] AS [o0]
    WHERE [o0].[Discriminator] = N'Officer'
) AS [t] ON ([o.Reports].[LeaderNickname] = [t].[Nickname]) AND ([o.Reports].[LeaderSquadId] = [t].[SquadId])
WHERE [o.Reports].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[HasSoulPatch], [t].[LeaderNickname], [t].[FullName], [t].[Nickname], [t].[SquadId], [FullName0], [o.Reports].[HasSoulPatch] DESC, [o.Reports].[Nickname], [o.Reports].[SquadId]",
                //
                @"SELECT [t2].[HasSoulPatch], [t2].[LeaderNickname], [t2].[FullName], [t2].[Nickname], [t2].[SquadId], [t2].[FullName0], [t2].[HasSoulPatch0], [t2].[Nickname0], [t2].[SquadId0], [o.Reports.Weapons].[Id], [o.Reports.Weapons].[OwnerFullName], [w.Owner.Squad].[Id]
FROM [Weapons] AS [o.Reports.Weapons]
LEFT JOIN (
    SELECT [w.Owner].*
    FROM [Gears] AS [w.Owner]
    WHERE [w.Owner].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON [o.Reports.Weapons].[OwnerFullName] = [t0].[FullName]
LEFT JOIN [Squads] AS [w.Owner.Squad] ON [t0].[SquadId] = [w.Owner.Squad].[Id]
INNER JOIN (
    SELECT [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[FullName], [t1].[Nickname], [t1].[SquadId], [o.Reports0].[FullName] AS [FullName0], [o.Reports0].[HasSoulPatch] AS [HasSoulPatch0], [o.Reports0].[Nickname] AS [Nickname0], [o.Reports0].[SquadId] AS [SquadId0]
    FROM [Gears] AS [o.Reports0]
    INNER JOIN (
        SELECT [o1].[HasSoulPatch], [o1].[LeaderNickname], [o1].[FullName], [o1].[Nickname], [o1].[SquadId]
        FROM [Gears] AS [o1]
        WHERE [o1].[Discriminator] = N'Officer'
    ) AS [t1] ON ([o.Reports0].[LeaderNickname] = [t1].[Nickname]) AND ([o.Reports0].[LeaderSquadId] = [t1].[SquadId])
    WHERE [o.Reports0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t2] ON [o.Reports.Weapons].[OwnerFullName] = [t2].[FullName0]
ORDER BY [t2].[HasSoulPatch], [t2].[LeaderNickname], [t2].[FullName], [t2].[Nickname], [t2].[SquadId], [t2].[FullName0], [t2].[HasSoulPatch0] DESC, [t2].[Nickname0], [t2].[SquadId0], [o.Reports.Weapons].[IsAutomatic], [o.Reports.Weapons].[Name] DESC, [o.Reports.Weapons].[Id], [w.Owner.Squad].[Id]",
                //
                @"SELECT [t6].[HasSoulPatch], [t6].[LeaderNickname], [t6].[FullName], [t6].[Nickname], [t6].[SquadId], [t6].[FullName0], [t6].[HasSoulPatch0], [t6].[Nickname0], [t6].[SquadId0], [t6].[IsAutomatic], [t6].[Name], [t6].[Id], [t6].[Id0], [w.Owner.Squad.Members].[Nickname] AS [Nickname1], [w.Owner.Squad.Members].[HasSoulPatch] AS [HasSoulPatch1], [w.Owner.Squad.Members].[SquadId]
FROM [Gears] AS [w.Owner.Squad.Members]
INNER JOIN (
    SELECT [t5].[HasSoulPatch], [t5].[LeaderNickname], [t5].[FullName], [t5].[Nickname], [t5].[SquadId], [t5].[FullName0], [t5].[HasSoulPatch0], [t5].[Nickname0], [t5].[SquadId0], [o.Reports.Weapons0].[IsAutomatic], [o.Reports.Weapons0].[Name], [o.Reports.Weapons0].[Id], [w.Owner.Squad0].[Id] AS [Id0]
    FROM [Weapons] AS [o.Reports.Weapons0]
    LEFT JOIN (
        SELECT [w.Owner0].*
        FROM [Gears] AS [w.Owner0]
        WHERE [w.Owner0].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t3] ON [o.Reports.Weapons0].[OwnerFullName] = [t3].[FullName]
    LEFT JOIN [Squads] AS [w.Owner.Squad0] ON [t3].[SquadId] = [w.Owner.Squad0].[Id]
    INNER JOIN (
        SELECT [t4].[HasSoulPatch], [t4].[LeaderNickname], [t4].[FullName], [t4].[Nickname], [t4].[SquadId], [o.Reports1].[FullName] AS [FullName0], [o.Reports1].[HasSoulPatch] AS [HasSoulPatch0], [o.Reports1].[Nickname] AS [Nickname0], [o.Reports1].[SquadId] AS [SquadId0]
        FROM [Gears] AS [o.Reports1]
        INNER JOIN (
            SELECT [o2].[HasSoulPatch], [o2].[LeaderNickname], [o2].[FullName], [o2].[Nickname], [o2].[SquadId]
            FROM [Gears] AS [o2]
            WHERE [o2].[Discriminator] = N'Officer'
        ) AS [t4] ON ([o.Reports1].[LeaderNickname] = [t4].[Nickname]) AND ([o.Reports1].[LeaderSquadId] = [t4].[SquadId])
        WHERE [o.Reports1].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t5] ON [o.Reports.Weapons0].[OwnerFullName] = [t5].[FullName0]
) AS [t6] ON [w.Owner.Squad.Members].[SquadId] = [t6].[Id0]
WHERE [w.Owner.Squad.Members].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t6].[HasSoulPatch], [t6].[LeaderNickname], [t6].[FullName], [t6].[Nickname], [t6].[SquadId], [t6].[FullName0], [t6].[HasSoulPatch0] DESC, [t6].[Nickname0], [t6].[SquadId0], [t6].[IsAutomatic], [t6].[Name] DESC, [t6].[Id], [t6].[Id0], [Nickname1]");
        }

        public override void Correlated_collection_with_top_level_FirstOrDefault()
        {
            base.Correlated_collection_with_top_level_FirstOrDefault();

            AssertSql(
                @"SELECT TOP(1) [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname], [g].[SquadId], [g].[FullName]",
                //
                @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId], [t].[Nickname], [t].[SquadId], [t].[FullName]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT TOP(1) [g0].[Nickname], [g0].[SquadId], [g0].[FullName]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
    ORDER BY [g0].[Nickname], [g0].[SquadId], [g0].[FullName]
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
ORDER BY [t].[Nickname], [t].[SquadId], [t].[FullName]");
        }

        public override async Task Correlated_collection_with_top_level_Count(bool isAsync)
        {
            await base.Correlated_collection_with_top_level_Count(isAsync);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override void Correlated_collection_with_top_level_Last_with_orderby_on_outer()
        {
            base.Correlated_collection_with_top_level_Last_with_orderby_on_outer();

            AssertSql(
                @"SELECT TOP(1) [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[FullName], [g].[Nickname] DESC, [g].[SquadId] DESC",
                //
                @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId], [t].[FullName], [t].[Nickname], [t].[SquadId]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT TOP(1) [g0].[FullName], [g0].[Nickname], [g0].[SquadId]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
    ORDER BY [g0].[FullName], [g0].[Nickname] DESC, [g0].[SquadId] DESC
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
ORDER BY [t].[FullName], [t].[Nickname] DESC, [t].[SquadId] DESC");
        }

        public override void Correlated_collection_with_top_level_Last_with_order_by_on_inner()
        {
            base.Correlated_collection_with_top_level_Last_with_order_by_on_inner();

            AssertSql(
                @"SELECT TOP(1) [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[FullName] DESC, [g].[Nickname] DESC, [g].[SquadId] DESC",
                //
                @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId], [t].[FullName], [t].[Nickname], [t].[SquadId]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT TOP(1) [g0].[FullName], [g0].[Nickname], [g0].[SquadId]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
    ORDER BY [g0].[FullName] DESC, [g0].[Nickname] DESC, [g0].[SquadId] DESC
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
ORDER BY [t].[FullName] DESC, [t].[Nickname] DESC, [t].[SquadId] DESC, [g.Weapons].[Name]");
        }

        public override void Include_with_group_by_and_last()
        {
            base.Include_with_group_by_and_last();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Rank], [g].[HasSoulPatch] DESC, [g].[FullName]",
                //
                @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT [g0].[FullName], [g0].[Rank], [g0].[HasSoulPatch]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
ORDER BY [t].[Rank], [t].[HasSoulPatch] DESC, [t].[FullName]");
        }

        public override void Include_with_group_by_with_composite_group_key()
        {
            base.Include_with_group_by_with_composite_group_key();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Rank], [g].[HasSoulPatch], [g].[Nickname], [g].[FullName]",
                //
                @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT [g0].[FullName], [g0].[Rank], [g0].[HasSoulPatch], [g0].[Nickname]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
ORDER BY [t].[Rank], [t].[HasSoulPatch], [t].[Nickname], [t].[FullName]");
        }

        public override void Include_with_group_by_order_by_take()
        {
            base.Include_with_group_by_order_by_take();

            AssertSql(
                @"@__p_0='3'

SELECT TOP(@__p_0) [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname], [g].[FullName]",
                //
                @"@__p_0='3'

SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT TOP(@__p_0) [g0].[FullName], [g0].[Nickname]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
    ORDER BY [g0].[Nickname], [g0].[FullName]
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
ORDER BY [t].[Nickname], [t].[FullName]");
        }

        public override void Include_with_group_by_distinct()
        {
            base.Include_with_group_by_distinct();

            AssertSql(
                @"SELECT DISTINCT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname], [g].[FullName]",
                //
                @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT DISTINCT [g0].[FullName], [g0].[Nickname]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
ORDER BY [t].[Nickname], [t].[FullName]");
        }

        public override async Task Null_semantics_on_nullable_bool_from_inner_join_subuery_is_fully_applied(bool isAsync)
        {
            await base.Null_semantics_on_nullable_bool_from_inner_join_subuery_is_fully_applied(isAsync);

            AssertSql(
                @"SELECT [t].[Id], [t].[CapitalName], [t].[Discriminator], [t].[Name], [t].[CommanderName], [t].[Eradicated]
FROM [LocustLeaders] AS [ll]
INNER JOIN (
    SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated]
    FROM [Factions] AS [f]
    WHERE ([f].[Discriminator] = N'LocustHorde') AND ([f].[Name] = N'Swarm')
) AS [t] ON [ll].[Name] = [t].[CommanderName]
WHERE [ll].[Discriminator] IN (N'LocustCommander', N'LocustLeader') AND (([t].[Eradicated] <> 1) OR [t].[Eradicated] IS NULL)");
        }

        public override async Task Null_semantics_on_nullable_bool_from_left_join_subuery_is_fully_applied(bool isAsync)
        {
            await base.Null_semantics_on_nullable_bool_from_left_join_subuery_is_fully_applied(isAsync);

            AssertSql(
                @"SELECT [t].[Id], [t].[CapitalName], [t].[Discriminator], [t].[Name], [t].[CommanderName], [t].[Eradicated]
FROM [LocustLeaders] AS [ll]
LEFT JOIN (
    SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated]
    FROM [Factions] AS [f]
    WHERE ([f].[Discriminator] = N'LocustHorde') AND ([f].[Name] = N'Swarm')
) AS [t] ON [ll].[Name] = [t].[CommanderName]
WHERE [ll].[Discriminator] IN (N'LocustCommander', N'LocustLeader') AND (([t].[Eradicated] <> 1) OR [t].[Eradicated] IS NULL)");
        }

        public override void Include_collection_group_by_reference()
        {
            base.Include_collection_group_by_reference();

            AssertSql(" ");
        }

        public override async Task Include_on_derived_type_with_order_by_and_paging(bool isAsync)
        {
            await base.Include_on_derived_type_with_order_by_and_paging(isAsync);

            AssertSql(
                @"@__p_0='10'

SELECT TOP(@__p_0) [ll].[Name], [ll].[Discriminator], [ll].[LocustHordeId], [ll].[ThreatLevel], [ll].[DefeatedByNickname], [ll].[DefeatedBySquadId], [ll].[HighCommandId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [LocustLeaders] AS [ll]
LEFT JOIN (
    SELECT [ll.DefeatedBy].*
    FROM [Gears] AS [ll.DefeatedBy]
    WHERE [ll.DefeatedBy].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON (([ll].[Discriminator] = N'LocustCommander') AND ([ll].[DefeatedByNickname] = [t].[Nickname])) AND (([ll].[Discriminator] = N'LocustCommander') AND ([ll].[DefeatedBySquadId] = [t].[SquadId]))
LEFT JOIN [Tags] AS [ll.DefeatedBy.Tag] ON (([t].[Nickname] = [ll.DefeatedBy.Tag].[GearNickName]) OR ([t].[Nickname] IS NULL AND [ll.DefeatedBy.Tag].[GearNickName] IS NULL)) AND (([t].[SquadId] = [ll.DefeatedBy.Tag].[GearSquadId]) OR ([t].[SquadId] IS NULL AND [ll.DefeatedBy.Tag].[GearSquadId] IS NULL))
WHERE [ll].[Discriminator] IN (N'LocustCommander', N'LocustLeader')
ORDER BY [ll.DefeatedBy.Tag].[Note], [t].[FullName]",
                //
                @"@__p_0='10'

SELECT [ll.DefeatedBy.Weapons].[Id], [ll.DefeatedBy.Weapons].[AmmunitionType], [ll.DefeatedBy.Weapons].[IsAutomatic], [ll.DefeatedBy.Weapons].[Name], [ll.DefeatedBy.Weapons].[OwnerFullName], [ll.DefeatedBy.Weapons].[SynergyWithId]
FROM [Weapons] AS [ll.DefeatedBy.Weapons]
INNER JOIN (
    SELECT DISTINCT [t1].*
    FROM (
        SELECT TOP(@__p_0) [t0].[FullName], [ll.DefeatedBy.Tag0].[Note]
        FROM [LocustLeaders] AS [ll0]
        LEFT JOIN (
            SELECT [ll.DefeatedBy0].*
            FROM [Gears] AS [ll.DefeatedBy0]
            WHERE [ll.DefeatedBy0].[Discriminator] IN (N'Officer', N'Gear')
        ) AS [t0] ON (([ll0].[Discriminator] = N'LocustCommander') AND ([ll0].[DefeatedByNickname] = [t0].[Nickname])) AND (([ll0].[Discriminator] = N'LocustCommander') AND ([ll0].[DefeatedBySquadId] = [t0].[SquadId]))
        LEFT JOIN [Tags] AS [ll.DefeatedBy.Tag0] ON (([t0].[Nickname] = [ll.DefeatedBy.Tag0].[GearNickName]) OR ([t0].[Nickname] IS NULL AND [ll.DefeatedBy.Tag0].[GearNickName] IS NULL)) AND (([t0].[SquadId] = [ll.DefeatedBy.Tag0].[GearSquadId]) OR ([t0].[SquadId] IS NULL AND [ll.DefeatedBy.Tag0].[GearSquadId] IS NULL))
        WHERE [ll0].[Discriminator] IN (N'LocustCommander', N'LocustLeader')
        ORDER BY [ll.DefeatedBy.Tag0].[Note], [t0].[FullName]
    ) AS [t1]
) AS [t2] ON [ll.DefeatedBy.Weapons].[OwnerFullName] = [t2].[FullName]
ORDER BY [t2].[Note], [t2].[FullName]");
        }

        public override async Task Select_required_navigation_on_derived_type(bool isAsync)
        {
            await base.Select_required_navigation_on_derived_type(isAsync);

            AssertSql(
                @"SELECT [ll.HighCommand].[Name]
FROM [LocustLeaders] AS [ll]
LEFT JOIN [LocustHighCommands] AS [ll.HighCommand] ON ([ll].[Discriminator] = N'LocustCommander') AND ([ll].[HighCommandId] = [ll.HighCommand].[Id])
WHERE [ll].[Discriminator] IN (N'LocustCommander', N'LocustLeader')");
        }

        public override async Task Where_required_navigation_on_derived_type(bool isAsync)
        {
            await base.Where_required_navigation_on_derived_type(isAsync);

            AssertSql(
                @"SELECT [ll].[Name], [ll].[Discriminator], [ll].[LocustHordeId], [ll].[ThreatLevel], [ll].[DefeatedByNickname], [ll].[DefeatedBySquadId], [ll].[HighCommandId]
FROM [LocustLeaders] AS [ll]
LEFT JOIN [LocustHighCommands] AS [ll.HighCommand] ON ([ll].[Discriminator] = N'LocustCommander') AND ([ll].[HighCommandId] = [ll.HighCommand].[Id])
WHERE [ll].[Discriminator] IN (N'LocustCommander', N'LocustLeader') AND ([ll.HighCommand].[IsOperational] = 1)");
        }

        public override async Task Outer_parameter_in_join_key(bool isAsync)
        {
            await base.Outer_parameter_in_join_key(isAsync);

            AssertSql(
                @"SELECT [o].[FullName]
FROM [Gears] AS [o]
WHERE [o].[Discriminator] = N'Officer'
ORDER BY [o].[Nickname]",
                //
                @"@_outer_FullName='Damon Baird' (Size = 450)

SELECT [t].[Note]
FROM [Tags] AS [t]
INNER JOIN [Gears] AS [g] ON @_outer_FullName = [g].[FullName]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')",
                //
                @"@_outer_FullName='Marcus Fenix' (Size = 450)

SELECT [t].[Note]
FROM [Tags] AS [t]
INNER JOIN [Gears] AS [g] ON @_outer_FullName = [g].[FullName]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Outer_parameter_in_join_key_inner_and_outer(bool isAsync)
        {
            await base.Outer_parameter_in_join_key_inner_and_outer(isAsync);

            AssertSql(
                @"SELECT [o].[FullName], [o].[Nickname]
FROM [Gears] AS [o]
WHERE [o].[Discriminator] = N'Officer'
ORDER BY [o].[Nickname]",
                //
                @"@_outer_FullName='Damon Baird' (Size = 4000)
@_outer_Nickname='Baird' (Size = 4000)

SELECT [t].[Note]
FROM [Tags] AS [t]
INNER JOIN [Gears] AS [g] ON @_outer_FullName = @_outer_Nickname
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')",
                //
                @"@_outer_FullName='Marcus Fenix' (Size = 4000)
@_outer_Nickname='Marcus' (Size = 4000)

SELECT [t].[Note]
FROM [Tags] AS [t]
INNER JOIN [Gears] AS [g] ON @_outer_FullName = @_outer_Nickname
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Outer_parameter_in_group_join_key(bool isAsync)
        {
            await base.Outer_parameter_in_group_join_key(isAsync);

            AssertSql(
                @"SELECT [o].[FullName]
FROM [Gears] AS [o]
WHERE [o].[Discriminator] = N'Officer'
ORDER BY [o].[Nickname]",
                //
                @"@_outer_FullName1='Damon Baird' (Size = 450)

SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOrBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gears] AS [g]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON @_outer_FullName1 = [t0].[FullName]
ORDER BY (SELECT 1)",
                //
                @"@_outer_FullName1='Marcus Fenix' (Size = 450)

SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOrBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gears] AS [g]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON @_outer_FullName1 = [t0].[FullName]
ORDER BY (SELECT 1)");
        }

        public override async Task Outer_parameter_in_group_join_with_DefaultIfEmpty(bool isAsync)
        {
            await base.Outer_parameter_in_group_join_with_DefaultIfEmpty(isAsync);

            AssertSql(
                @"SELECT [o].[FullName]
FROM [Gears] AS [o]
WHERE [o].[Discriminator] = N'Officer'
ORDER BY [o].[Nickname]",
                //
                @"@_outer_FullName1='Damon Baird' (Size = 450)

SELECT [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gears] AS [g]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON @_outer_FullName1 = [t0].[FullName]",
                //
                @"@_outer_FullName1='Marcus Fenix' (Size = 450)

SELECT [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gears] AS [g]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON @_outer_FullName1 = [t0].[FullName]");
        }

        public override async Task Include_with_concat(bool isAsync)
        {
            await base.Include_with_concat(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Squad].[Id], [g.Squad].[InternalNumber], [g.Squad].[Name]
FROM [Gears] AS [g]
INNER JOIN [Squads] AS [g.Squad] ON [g].[SquadId] = [g.Squad].[Id]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')",
                //
                @"SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOrBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gears] AS [g0]
WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Include_collection_with_concat(bool isAsync)
        {
            await base.Include_collection_with_concat(isAsync);

            AssertSql(
                "");
        }

        public override async Task Negated_bool_ternary_inside_anonymous_type_in_projection(bool isAsync)
        {
            await base.Negated_bool_ternary_inside_anonymous_type_in_projection(isAsync);

            AssertSql(
                @"SELECT CASE
    WHEN NOT (CASE
        WHEN [t0].[HasSoulPatch] = 1
        THEN CAST(1 AS BIT) ELSE COALESCE([t0].[HasSoulPatch], 1)
    END = 1)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END AS [c]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])");
        }

        public override async Task Order_by_entity_qsre(bool isAsync)
        {
            await base.Order_by_entity_qsre(isAsync);

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
LEFT JOIN [Cities] AS [g.AssignedCity] ON [g].[AssignedCityName] = [g.AssignedCity].[Name]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g.AssignedCity].[Name], [g].[Nickname] DESC");
        }

        public override async Task Order_by_entity_qsre_with_inheritance(bool isAsync)
        {
            await base.Order_by_entity_qsre_with_inheritance(isAsync);

            AssertSql(
                @"SELECT [lc].[Name]
FROM [LocustLeaders] AS [lc]
INNER JOIN [LocustHighCommands] AS [lc.HighCommand] ON [lc].[HighCommandId] = [lc.HighCommand].[Id]
WHERE [lc].[Discriminator] = N'LocustCommander'
ORDER BY [lc.HighCommand].[Id], [lc].[Name]");
        }

        public override async Task Order_by_entity_qsre_composite_key(bool isAsync)
        {
            await base.Order_by_entity_qsre_composite_key(isAsync);

            AssertSql(
                @"SELECT [w].[Name]
FROM [Weapons] AS [w]
LEFT JOIN (
    SELECT [w.Owner].*
    FROM [Gears] AS [w.Owner]
    WHERE [w.Owner].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [w].[OwnerFullName] = [t].[FullName]
ORDER BY [t].[Nickname], [t].[SquadId], [w].[Id]");
        }

        public override async Task Order_by_entity_qsre_with_other_orderbys(bool isAsync)
        {
            await base.Order_by_entity_qsre_with_other_orderbys(isAsync);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w.SynergyWith] ON [w].[SynergyWithId] = [w.SynergyWith].[Id]
LEFT JOIN (
    SELECT [w.Owner].*
    FROM [Gears] AS [w.Owner]
    WHERE [w.Owner].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [w].[OwnerFullName] = [t].[FullName]
ORDER BY [w].[IsAutomatic], [t].[Nickname] DESC, [t].[SquadId] DESC, [w.SynergyWith].[Id], [w].[Name]");
        }

        public override async Task Join_on_entity_qsre_keys(bool isAsync)
        {
            await base.Join_on_entity_qsre_keys(isAsync);

            AssertSql(
                @"SELECT [w1].[Name] AS [Name1], [w2].[Name] AS [Name2]
FROM [Weapons] AS [w1]
INNER JOIN [Weapons] AS [w2] ON [w1].[Id] = [w2].[Id]");
        }

        public override async Task Join_on_entity_qsre_keys_composite_key(bool isAsync)
        {
            await base.Join_on_entity_qsre_keys_composite_key(isAsync);

            AssertSql(
                @"SELECT [g1].[FullName] AS [GearName1], [g2].[FullName] AS [GearName2]
FROM [Gears] AS [g1]
INNER JOIN [Gears] AS [g2] ON ([g1].[Nickname] = [g2].[Nickname]) AND ([g1].[SquadId] = [g2].[SquadId])
WHERE [g1].[Discriminator] IN (N'Officer', N'Gear') AND [g2].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Join_on_entity_qsre_keys_inheritance(bool isAsync)
        {
            await base.Join_on_entity_qsre_keys_inheritance(isAsync);

            AssertSql(
                @"SELECT [g].[FullName] AS [GearName], [o].[FullName] AS [OfficerName]
FROM [Gears] AS [g]
INNER JOIN [Gears] AS [o] ON ([g].[Nickname] = [o].[Nickname]) AND ([g].[SquadId] = [o].[SquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([o].[Discriminator] = N'Officer')");
        }

        public override async Task Join_on_entity_qsre_keys_outer_key_is_navigation(bool isAsync)
        {
            await base.Join_on_entity_qsre_keys_outer_key_is_navigation(isAsync);

            AssertSql(
                @"SELECT [w1].[Name] AS [Name1], [w2].[Name] AS [Name2]
FROM [Weapons] AS [w1]
LEFT JOIN [Weapons] AS [w1.SynergyWith] ON [w1].[SynergyWithId] = [w1.SynergyWith].[Id]
INNER JOIN [Weapons] AS [w2] ON [w1.SynergyWith].[Id] = [w2].[Id]");
        }

        public override async Task Join_on_entity_qsre_keys_inner_key_is_navigation(bool isAsync)
        {
            await base.Join_on_entity_qsre_keys_inner_key_is_navigation(isAsync);

            AssertSql(
                @"SELECT [c].[Name] AS [CityName], [g].[Nickname] AS [GearNickname]
FROM [Cities] AS [c]
INNER JOIN [Gears] AS [g] ON [c].[Name] = (
    SELECT TOP(1) [subQuery0].[Name]
    FROM [Cities] AS [subQuery0]
    WHERE [subQuery0].[Name] = [g].[AssignedCityName]
)
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Join_on_entity_qsre_keys_inner_key_is_navigation_composite_key(bool isAsync)
        {
            await base.Join_on_entity_qsre_keys_inner_key_is_navigation_composite_key(isAsync);

            AssertContainsSql(
                @"SELECT [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM (
    SELECT [tt].*
    FROM [Tags] AS [tt]
    WHERE [tt].[Note] IN (N'Cole''s Tag', N'Dom''s Tag')
) AS [t]",
                //
                @"@_outer_GearNickName='Dom' (Size = 450)
@_outer_GearSquadId='1' (Nullable = true)

SELECT TOP(1) [subQuery].[Nickname], [subQuery].[SquadId], [subQuery].[AssignedCityName], [subQuery].[CityOrBirthName], [subQuery].[Discriminator], [subQuery].[FullName], [subQuery].[HasSoulPatch], [subQuery].[LeaderNickname], [subQuery].[LeaderSquadId], [subQuery].[Rank]
FROM [Gears] AS [subQuery]
WHERE [subQuery].[Discriminator] IN (N'Officer', N'Gear') AND (([subQuery].[Nickname] = @_outer_GearNickName) AND ([subQuery].[SquadId] = @_outer_GearSquadId))",
                //
                @"@_outer_GearNickName='Cole Train' (Size = 450)
@_outer_GearSquadId='1' (Nullable = true)

SELECT TOP(1) [subQuery].[Nickname], [subQuery].[SquadId], [subQuery].[AssignedCityName], [subQuery].[CityOrBirthName], [subQuery].[Discriminator], [subQuery].[FullName], [subQuery].[HasSoulPatch], [subQuery].[LeaderNickname], [subQuery].[LeaderSquadId], [subQuery].[Rank]
FROM [Gears] AS [subQuery]
WHERE [subQuery].[Discriminator] IN (N'Officer', N'Gear') AND (([subQuery].[Nickname] = @_outer_GearNickName) AND ([subQuery].[SquadId] = @_outer_GearSquadId))",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Join_on_entity_qsre_keys_inner_key_is_nested_navigation(bool isAsync)
        {
            await base.Join_on_entity_qsre_keys_inner_key_is_nested_navigation(isAsync);

            AssertSql(
                @"SELECT [s].[Name] AS [SquadName], [t].[Name] AS [WeaponName]
FROM [Squads] AS [s]
INNER JOIN (
    SELECT [ww].*
    FROM [Weapons] AS [ww]
    WHERE [ww].[IsAutomatic] = 1
) AS [t] ON [s].[Id] = (
    SELECT TOP(1) [subQuery.Squad0].[Id]
    FROM [Gears] AS [subQuery0]
    INNER JOIN [Squads] AS [subQuery.Squad0] ON [subQuery0].[SquadId] = [subQuery.Squad0].[Id]
    WHERE [subQuery0].[Discriminator] IN (N'Officer', N'Gear') AND ([subQuery0].[FullName] = [t].[OwnerFullName])
)");
        }

        public override async Task GroupJoin_on_entity_qsre_keys_inner_key_is_nested_navigation(bool isAsync)
        {
            await base.GroupJoin_on_entity_qsre_keys_inner_key_is_nested_navigation(isAsync);

            AssertSql(
                @"SELECT [s].[Name] AS [SquadName], [w].[Name] AS [WeaponName]
FROM [Squads] AS [s]
LEFT JOIN [Weapons] AS [w] ON [s].[Id] = (
    SELECT TOP(1) [subQuery.Squad0].[Id]
    FROM [Gears] AS [subQuery0]
    INNER JOIN [Squads] AS [subQuery.Squad0] ON [subQuery0].[SquadId] = [subQuery.Squad0].[Id]
    WHERE [subQuery0].[Discriminator] IN (N'Officer', N'Gear') AND ([subQuery0].[FullName] = [w].[OwnerFullName])
)");
        }

        public override async Task Join_with_complex_key_selector(bool isAsync)
        {
            await base.Join_with_complex_key_selector(isAsync);

            AssertContainsSql(
                @"SELECT [ii].[Nickname], [ii].[SquadId], [ii].[AssignedCityName], [ii].[CityOrBirthName], [ii].[Discriminator], [ii].[FullName], [ii].[HasSoulPatch], [ii].[LeaderNickname], [ii].[LeaderSquadId], [ii].[Rank]
FROM [Gears] AS [ii]
WHERE [ii].[Discriminator] IN (N'Officer', N'Gear')",
                //
                @"SELECT [o].[Id] AS [Id0], [o].[InternalNumber], [o].[Name], [t0].[Id]
FROM [Squads] AS [o]
INNER JOIN (
    SELECT [t].*
    FROM [Tags] AS [t]
    WHERE [t].[Note] = N'Marcus'' Tag'
) AS [t0] ON 1 = 1",
                //
                @"@_outer_Id='34c8d86e-a4ac-4be5-827f-584dda348a07'
@_outer_Id1='1'

SELECT TOP(1) [v].[Nickname], [v].[SquadId], [v].[AssignedCityName], [v].[CityOrBirthName], [v].[Discriminator], [v].[FullName], [v].[HasSoulPatch], [v].[LeaderNickname], [v].[LeaderSquadId], [v].[Rank]
FROM [Gears] AS [v]
LEFT JOIN [Tags] AS [v.Tag] ON ([v].[Nickname] = [v.Tag].[GearNickName]) AND ([v].[SquadId] = [v.Tag].[GearSquadId])
WHERE ([v].[Discriminator] IN (N'Officer', N'Gear') AND ([v.Tag].[Id] = @_outer_Id)) AND (@_outer_Id1 = [v].[SquadId])",
                //
                @"@_outer_Id='34c8d86e-a4ac-4be5-827f-584dda348a07'
@_outer_Id1='2'

SELECT TOP(1) [v].[Nickname], [v].[SquadId], [v].[AssignedCityName], [v].[CityOrBirthName], [v].[Discriminator], [v].[FullName], [v].[HasSoulPatch], [v].[LeaderNickname], [v].[LeaderSquadId], [v].[Rank]
FROM [Gears] AS [v]
LEFT JOIN [Tags] AS [v.Tag] ON ([v].[Nickname] = [v.Tag].[GearNickName]) AND ([v].[SquadId] = [v.Tag].[GearSquadId])
WHERE ([v].[Discriminator] IN (N'Officer', N'Gear') AND ([v.Tag].[Id] = @_outer_Id)) AND (@_outer_Id1 = [v].[SquadId])");
        }

        public override void Include_with_group_by_on_entity_qsre()
        {
            base.Include_with_group_by_on_entity_qsre();

            AssertSql(
                @"SELECT [s].[Id], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
ORDER BY [s].[Id]",
                //
                @"SELECT [s.Members].[Nickname], [s.Members].[SquadId], [s.Members].[AssignedCityName], [s.Members].[CityOrBirthName], [s.Members].[Discriminator], [s.Members].[FullName], [s.Members].[HasSoulPatch], [s.Members].[LeaderNickname], [s.Members].[LeaderSquadId], [s.Members].[Rank]
FROM [Gears] AS [s.Members]
INNER JOIN (
    SELECT [s0].[Id]
    FROM [Squads] AS [s0]
) AS [t] ON [s.Members].[SquadId] = [t].[Id]
WHERE [s.Members].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[Id]");
        }

        public override void Include_with_group_by_on_entity_qsre_with_composite_key()
        {
            base.Include_with_group_by_on_entity_qsre_with_composite_key();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname], [g].[SquadId], [g].[FullName]",
                //
                @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT [g0].[FullName], [g0].[Nickname], [g0].[SquadId]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
ORDER BY [t].[Nickname], [t].[SquadId], [t].[FullName]");
        }

        public override void Include_with_group_by_on_entity_navigation()
        {
            base.Include_with_group_by_on_entity_navigation();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Squad].[Id], [g.Squad].[InternalNumber], [g.Squad].[Name]
FROM [Gears] AS [g]
INNER JOIN [Squads] AS [g.Squad] ON [g].[SquadId] = [g.Squad].[Id]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 0)
ORDER BY [g.Squad].[Id], [g].[FullName]",
                //
                @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT DISTINCT [g0].[FullName], [g.Squad0].[Id]
    FROM [Gears] AS [g0]
    INNER JOIN [Squads] AS [g.Squad0] ON [g0].[SquadId] = [g.Squad0].[Id]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND ([g0].[HasSoulPatch] = 0)
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
ORDER BY [t].[Id], [t].[FullName]");
        }

        public override void Include_with_group_by_on_entity_navigation_with_inheritance()
        {
            base.Include_with_group_by_on_entity_navigation_with_inheritance();

            AssertSql(
                @"SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOrBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [l.Commander].*
    FROM [LocustLeaders] AS [l.Commander]
    WHERE [l.Commander].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
LEFT JOIN (
    SELECT [l.Commander.DefeatedBy].*
    FROM [Gears] AS [l.Commander.DefeatedBy]
    WHERE [l.Commander.DefeatedBy].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[DefeatedByNickname] = [t0].[Nickname]) AND ([t].[DefeatedBySquadId] = [t0].[SquadId])
WHERE [f].[Discriminator] = N'LocustHorde'
ORDER BY [t0].[Nickname], [t0].[SquadId], [f].[Id]",
                //
                @"SELECT [l.Leaders].[Name], [l.Leaders].[Discriminator], [l.Leaders].[LocustHordeId], [l.Leaders].[ThreatLevel], [l.Leaders].[DefeatedByNickname], [l.Leaders].[DefeatedBySquadId], [l.Leaders].[HighCommandId]
FROM [LocustLeaders] AS [l.Leaders]
INNER JOIN (
    SELECT DISTINCT [f0].[Id], [t2].[Nickname], [t2].[SquadId]
    FROM [Factions] AS [f0]
    LEFT JOIN (
        SELECT [l.Commander0].*
        FROM [LocustLeaders] AS [l.Commander0]
        WHERE [l.Commander0].[Discriminator] = N'LocustCommander'
    ) AS [t1] ON [f0].[CommanderName] = [t1].[Name]
    LEFT JOIN (
        SELECT [l.Commander.DefeatedBy0].*
        FROM [Gears] AS [l.Commander.DefeatedBy0]
        WHERE [l.Commander.DefeatedBy0].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t2] ON ([t1].[DefeatedByNickname] = [t2].[Nickname]) AND ([t1].[DefeatedBySquadId] = [t2].[SquadId])
    WHERE [f0].[Discriminator] = N'LocustHorde'
) AS [t3] ON [l.Leaders].[LocustHordeId] = [t3].[Id]
WHERE [l.Leaders].[Discriminator] IN (N'LocustCommander', N'LocustLeader')
ORDER BY [t3].[Nickname], [t3].[SquadId], [t3].[Id]");
        }

        public override void Streaming_correlated_collection_issue_11403()
        {
            base.Streaming_correlated_collection_issue_11403();

            AssertSql(
                @"SELECT TOP(1) [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]",
                //
                @"@_outer_FullName='Damon Baird' (Size = 450)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[IsAutomatic] = 0) AND (@_outer_FullName = [w].[OwnerFullName])
ORDER BY [w].[Id]");
        }

        public override async Task Project_one_value_type_from_empty_collection(bool isAsync)
        {
            await base.Project_one_value_type_from_empty_collection(isAsync);

            AssertSql(
                @"SELECT [s].[Name], COALESCE((
    SELECT TOP(1) [m].[SquadId]
    FROM [Gears] AS [m]
    WHERE ([m].[Discriminator] IN (N'Officer', N'Gear') AND ([m].[HasSoulPatch] = 1)) AND ([s].[Id] = [m].[SquadId])
), 0) AS [SquadId]
FROM [Squads] AS [s]
WHERE [s].[Name] = N'Kilo'");
        }

        public override async Task Filter_on_subquery_projecting_one_value_type_from_empty_collection(bool isAsync)
        {
            await base.Filter_on_subquery_projecting_one_value_type_from_empty_collection(isAsync);

            AssertSql(
                @"SELECT [s].[Name]
FROM [Squads] AS [s]
WHERE ([s].[Name] = N'Kilo') AND (COALESCE((
    SELECT TOP(1) [m].[SquadId]
    FROM [Gears] AS [m]
    WHERE ([m].[Discriminator] IN (N'Officer', N'Gear') AND ([m].[HasSoulPatch] = 1)) AND ([s].[Id] = [m].[SquadId])
), 0) <> 0)");
        }

        public override async Task Select_subquery_projecting_single_constant_int(bool isAsync)
        {
            await base.Select_subquery_projecting_single_constant_int(isAsync);

            AssertSql(
                @"SELECT [s].[Name], COALESCE((
    SELECT TOP(1) 42
    FROM [Gears] AS [g]
    WHERE ([g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)) AND ([s].[Id] = [g].[SquadId])
), 0) AS [Gear]
FROM [Squads] AS [s]");
        }

        public override async Task Select_subquery_projecting_single_constant_string(bool isAsync)
        {
            await base.Select_subquery_projecting_single_constant_string(isAsync);

            AssertSql(
                @"SELECT [s].[Name], (
    SELECT TOP(1) N'Foo'
    FROM [Gears] AS [g]
    WHERE ([g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)) AND ([s].[Id] = [g].[SquadId])
) AS [Gear]
FROM [Squads] AS [s]");
        }

        public override async Task Select_subquery_projecting_single_constant_bool(bool isAsync)
        {
            await base.Select_subquery_projecting_single_constant_bool(isAsync);

            AssertSql(
                @"SELECT [s].[Name], CAST(COALESCE((
    SELECT TOP(1) CAST(1 AS bit)
    FROM [Gears] AS [g]
    WHERE ([g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)) AND ([s].[Id] = [g].[SquadId])
), 0) AS bit) AS [Gear]
FROM [Squads] AS [s]");
        }

        public override async Task Select_subquery_projecting_single_constant_inside_anonymous(bool isAsync)
        {
            await base.Select_subquery_projecting_single_constant_inside_anonymous(isAsync);

            AssertSql(
                @"SELECT [s].[Name], [s].[Id]
FROM [Squads] AS [s]",
                //
                @"@_outer_Id='1'

SELECT TOP(1) 1
FROM [Gears] AS [g]
WHERE ([g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)) AND (@_outer_Id = [g].[SquadId])",
                //
                @"@_outer_Id='2'

SELECT TOP(1) 1
FROM [Gears] AS [g]
WHERE ([g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)) AND (@_outer_Id = [g].[SquadId])");
        }

        public override async Task Select_subquery_projecting_multiple_constants_inside_anonymous(bool isAsync)
        {
            await base.Select_subquery_projecting_multiple_constants_inside_anonymous(isAsync);

            AssertSql(
                @"SELECT [s].[Name], [s].[Id]
FROM [Squads] AS [s]",
                //
                @"@_outer_Id='1'

SELECT TOP(1) 1
FROM [Gears] AS [g]
WHERE ([g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)) AND (@_outer_Id = [g].[SquadId])",
                //
                @"@_outer_Id='2'

SELECT TOP(1) 1
FROM [Gears] AS [g]
WHERE ([g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)) AND (@_outer_Id = [g].[SquadId])");
        }

        public override async Task Include_with_order_by_constant(bool isAsync)
        {
            await base.Include_with_order_by_constant(isAsync);

            AssertSql(
                @"SELECT [s].[Id], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
ORDER BY (SELECT 1), [s].[Id]",
                //
                @"SELECT [s.Members].[Nickname], [s.Members].[SquadId], [s.Members].[AssignedCityName], [s.Members].[CityOrBirthName], [s.Members].[Discriminator], [s.Members].[FullName], [s.Members].[HasSoulPatch], [s.Members].[LeaderNickname], [s.Members].[LeaderSquadId], [s.Members].[Rank]
FROM [Gears] AS [s.Members]
INNER JOIN (
    SELECT [s0].[Id], 42 AS [c]
    FROM [Squads] AS [s0]
) AS [t] ON [s.Members].[SquadId] = [t].[Id]
WHERE [s.Members].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[c], [t].[Id]");
        }

        public override void Include_groupby_constant()
        {
            base.Include_groupby_constant();

            AssertSql(
                @"SELECT [s].[Id], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
ORDER BY (SELECT 1), [s].[Id]",
                //
                @"SELECT [s.Members].[Nickname], [s.Members].[SquadId], [s.Members].[AssignedCityName], [s.Members].[CityOrBirthName], [s.Members].[Discriminator], [s.Members].[FullName], [s.Members].[HasSoulPatch], [s.Members].[LeaderNickname], [s.Members].[LeaderSquadId], [s.Members].[Rank]
FROM [Gears] AS [s.Members]
INNER JOIN (
    SELECT [s0].[Id], 1 AS [c]
    FROM [Squads] AS [s0]
) AS [t] ON [s.Members].[SquadId] = [t].[Id]
WHERE [s.Members].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[c], [t].[Id]");
        }

        public override async Task Correlated_collection_order_by_constant(bool isAsync)
        {
            await base.Correlated_collection_order_by_constant(isAsync);

            AssertSql(
                @"SELECT [s].[Nickname], [s].[FullName]
FROM [Gears] AS [s]
WHERE [s].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY (SELECT 1) DESC, [s].[Nickname], [s].[SquadId], [s].[FullName]",
                //
                @"SELECT [t].[c], [t].[Nickname], [t].[SquadId], [t].[FullName], [s.Weapons].[Name], [s.Weapons].[OwnerFullName]
FROM [Weapons] AS [s.Weapons]
INNER JOIN (
    SELECT 1 AS [c], [s0].[Nickname], [s0].[SquadId], [s0].[FullName]
    FROM [Gears] AS [s0]
    WHERE [s0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [s.Weapons].[OwnerFullName] = [t].[FullName]
ORDER BY [t].[c] DESC, [t].[Nickname], [t].[SquadId], [t].[FullName]");
        }

        public override async Task Select_subquery_projecting_single_constant_null_of_non_mapped_type(bool isAsync)
        {
            await base.Select_subquery_projecting_single_constant_null_of_non_mapped_type(isAsync);

            AssertSql(
                @"SELECT [s].[Name], [s].[Id]
FROM [Squads] AS [s]",
                //
                @"@_outer_Id='1'

SELECT TOP(1) 1
FROM [Gears] AS [g]
WHERE ([g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)) AND (@_outer_Id = [g].[SquadId])",
                //
                @"@_outer_Id='2'

SELECT TOP(1) 1
FROM [Gears] AS [g]
WHERE ([g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)) AND (@_outer_Id = [g].[SquadId])");
        }

        public override async Task Select_subquery_projecting_single_constant_of_non_mapped_type(bool isAsync)
        {
            await base.Select_subquery_projecting_single_constant_of_non_mapped_type(isAsync);

            AssertSql(
                @"SELECT [s].[Name], [s].[Id]
FROM [Squads] AS [s]",
                //
                @"@_outer_Id='1'

SELECT TOP(1) 1
FROM [Gears] AS [g]
WHERE ([g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)) AND (@_outer_Id = [g].[SquadId])",
                //
                @"@_outer_Id='2'

SELECT TOP(1) 1
FROM [Gears] AS [g]
WHERE ([g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)) AND (@_outer_Id = [g].[SquadId])");
        }

        public override async Task Include_with_order_by_constant_null_of_non_mapped_type(bool isAsync)
        {
            await base.Include_with_order_by_constant_null_of_non_mapped_type(isAsync);

            AssertSql(
                "");
        }

        public override void Include_groupby_constant_null_of_non_mapped_type()
        {
            base.Include_groupby_constant_null_of_non_mapped_type();

            AssertSql(
                "");
        }

        public override async Task Correlated_collection_order_by_constant_null_of_non_mapped_type(bool isAsync)
        {
            await base.Correlated_collection_order_by_constant_null_of_non_mapped_type(isAsync);

            AssertSql(
                @"SELECT [s].[Nickname], [s].[FullName]
FROM [Gears] AS [s]
WHERE [s].[Discriminator] IN (N'Officer', N'Gear')",
                //
                @"@_outer_FullName='Damon Baird' (Size = 450)

SELECT [w].[Name]
FROM [Weapons] AS [w]
WHERE @_outer_FullName = [w].[OwnerFullName]",
                //
                @"@_outer_FullName='Augustus Cole' (Size = 450)

SELECT [w].[Name]
FROM [Weapons] AS [w]
WHERE @_outer_FullName = [w].[OwnerFullName]",
                //
                @"@_outer_FullName='Dominic Santiago' (Size = 450)

SELECT [w].[Name]
FROM [Weapons] AS [w]
WHERE @_outer_FullName = [w].[OwnerFullName]",
                //
                @"@_outer_FullName='Marcus Fenix' (Size = 450)

SELECT [w].[Name]
FROM [Weapons] AS [w]
WHERE @_outer_FullName = [w].[OwnerFullName]",
                //
                @"@_outer_FullName='Garron Paduk' (Size = 450)

SELECT [w].[Name]
FROM [Weapons] AS [w]
WHERE @_outer_FullName = [w].[OwnerFullName]");
        }

        public override void GroupBy_composite_key_with_Include()
        {
            base.GroupBy_composite_key_with_Include();

            AssertSql(
                @"SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOrBirthName], [o].[Discriminator], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank]
FROM [Gears] AS [o]
WHERE [o].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [o].[Rank], (SELECT 1), [o].[Nickname], [o].[FullName]",
                //
                @"SELECT [o.Weapons].[Id], [o.Weapons].[AmmunitionType], [o.Weapons].[IsAutomatic], [o.Weapons].[Name], [o.Weapons].[OwnerFullName], [o.Weapons].[SynergyWithId]
FROM [Weapons] AS [o.Weapons]
INNER JOIN (
    SELECT [o0].[FullName], [o0].[Rank], 1 AS [c], [o0].[Nickname]
    FROM [Gears] AS [o0]
    WHERE [o0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [o.Weapons].[OwnerFullName] = [t].[FullName]
ORDER BY [t].[Rank], [t].[c], [t].[Nickname], [t].[FullName]");
        }

        public override async Task Include_collection_OrderBy_aggregate(bool isAsync)
        {
            await base.Include_collection_OrderBy_aggregate(isAsync);

            AssertSql(
                @"SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOrBirthName], [o].[Discriminator], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank]
FROM [Gears] AS [o]
WHERE [o].[Discriminator] = N'Officer'
ORDER BY (
    SELECT COUNT(*)
    FROM [Weapons] AS [w]
    WHERE [o].[FullName] = [w].[OwnerFullName]
), [o].[Nickname], [o].[SquadId]",
                //
                @"SELECT [o.Reports].[Nickname], [o.Reports].[SquadId], [o.Reports].[AssignedCityName], [o.Reports].[CityOrBirthName], [o.Reports].[Discriminator], [o.Reports].[FullName], [o.Reports].[HasSoulPatch], [o.Reports].[LeaderNickname], [o.Reports].[LeaderSquadId], [o.Reports].[Rank]
FROM [Gears] AS [o.Reports]
INNER JOIN (
    SELECT [o0].[Nickname], [o0].[SquadId], (
        SELECT COUNT(*)
        FROM [Weapons] AS [w0]
        WHERE [o0].[FullName] = [w0].[OwnerFullName]
    ) AS [c]
    FROM [Gears] AS [o0]
    WHERE [o0].[Discriminator] = N'Officer'
) AS [t] ON ([o.Reports].[LeaderNickname] = [t].[Nickname]) AND ([o.Reports].[LeaderSquadId] = [t].[SquadId])
WHERE [o.Reports].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[c], [t].[Nickname], [t].[SquadId]");
        }

        public override async Task Include_collection_with_complex_OrderBy2(bool isAsync)
        {
            await base.Include_collection_with_complex_OrderBy2(isAsync);

            AssertSql(
                @"SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOrBirthName], [o].[Discriminator], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank]
FROM [Gears] AS [o]
WHERE [o].[Discriminator] = N'Officer'
ORDER BY (
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE [o].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]
), [o].[Nickname], [o].[SquadId]",
                //
                @"SELECT [o.Reports].[Nickname], [o.Reports].[SquadId], [o.Reports].[AssignedCityName], [o.Reports].[CityOrBirthName], [o.Reports].[Discriminator], [o.Reports].[FullName], [o.Reports].[HasSoulPatch], [o.Reports].[LeaderNickname], [o.Reports].[LeaderSquadId], [o.Reports].[Rank]
FROM [Gears] AS [o.Reports]
INNER JOIN (
    SELECT [o0].[Nickname], [o0].[SquadId], (
        SELECT TOP(1) [w0].[IsAutomatic]
        FROM [Weapons] AS [w0]
        WHERE [o0].[FullName] = [w0].[OwnerFullName]
        ORDER BY [w0].[Id]
    ) AS [c]
    FROM [Gears] AS [o0]
    WHERE [o0].[Discriminator] = N'Officer'
) AS [t] ON ([o.Reports].[LeaderNickname] = [t].[Nickname]) AND ([o.Reports].[LeaderSquadId] = [t].[SquadId])
WHERE [o.Reports].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[c], [t].[Nickname], [t].[SquadId]");
        }

        public override async Task Include_collection_with_complex_OrderBy3(bool isAsync)
        {
            await base.Include_collection_with_complex_OrderBy3(isAsync);

            AssertSql(
                @"SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOrBirthName], [o].[Discriminator], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank]
FROM [Gears] AS [o]
WHERE [o].[Discriminator] = N'Officer'
ORDER BY COALESCE((
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE [o].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]
), 0), [o].[Nickname], [o].[SquadId]",
                //
                @"SELECT [o.Reports].[Nickname], [o.Reports].[SquadId], [o.Reports].[AssignedCityName], [o.Reports].[CityOrBirthName], [o.Reports].[Discriminator], [o.Reports].[FullName], [o.Reports].[HasSoulPatch], [o.Reports].[LeaderNickname], [o.Reports].[LeaderSquadId], [o.Reports].[Rank]
FROM [Gears] AS [o.Reports]
INNER JOIN (
    SELECT [o0].[Nickname], [o0].[SquadId], CAST(COALESCE((
        SELECT TOP(1) [w0].[IsAutomatic]
        FROM [Weapons] AS [w0]
        WHERE [o0].[FullName] = [w0].[OwnerFullName]
        ORDER BY [w0].[Id]
    ), 0) AS bit) AS [c]
    FROM [Gears] AS [o0]
    WHERE [o0].[Discriminator] = N'Officer'
) AS [t] ON ([o.Reports].[LeaderNickname] = [t].[Nickname]) AND ([o.Reports].[LeaderSquadId] = [t].[SquadId])
WHERE [o.Reports].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[c], [t].[Nickname], [t].[SquadId]");
        }

        public override async Task Correlated_collection_with_complex_OrderBy(bool isAsync)
        {
            await base.Correlated_collection_with_complex_OrderBy(isAsync);

            AssertSql(
                @"SELECT [o].[Nickname], [o].[SquadId]
FROM [Gears] AS [o]
WHERE [o].[Discriminator] = N'Officer'
ORDER BY (
    SELECT COUNT(*)
    FROM [Weapons] AS [w0]
    WHERE [o].[FullName] = [w0].[OwnerFullName]
), [o].[Nickname], [o].[SquadId]",
                //
                @"SELECT [o.Reports].[Nickname], [o.Reports].[SquadId], [o.Reports].[AssignedCityName], [o.Reports].[CityOrBirthName], [o.Reports].[Discriminator], [o.Reports].[FullName], [o.Reports].[HasSoulPatch], [o.Reports].[LeaderNickname], [o.Reports].[LeaderSquadId], [o.Reports].[Rank], [t].[c], [t].[Nickname], [t].[SquadId]
FROM [Gears] AS [o.Reports]
INNER JOIN (
    SELECT (
        SELECT COUNT(*)
        FROM [Weapons] AS [w1]
        WHERE [o0].[FullName] = [w1].[OwnerFullName]
    ) AS [c], [o0].[Nickname], [o0].[SquadId]
    FROM [Gears] AS [o0]
    WHERE [o0].[Discriminator] = N'Officer'
) AS [t] ON ([o.Reports].[LeaderNickname] = [t].[Nickname]) AND ([o.Reports].[LeaderSquadId] = [t].[SquadId])
WHERE [o.Reports].[Discriminator] IN (N'Officer', N'Gear') AND ([o.Reports].[HasSoulPatch] = 0)
ORDER BY [t].[c], [t].[Nickname], [t].[SquadId]");
        }

#if !Test21
        public override async Task Correlated_collection_with_very_complex_order_by(bool isAsync)
        {
            await base.Correlated_collection_with_very_complex_order_by(isAsync);

            AssertSql(
                @"SELECT [o].[Nickname], [o].[SquadId]
FROM [Gears] AS [o]
WHERE [o].[Discriminator] = N'Officer'
ORDER BY (
    SELECT COUNT(*)
    FROM [Weapons] AS [w0]
    WHERE ([w0].[IsAutomatic] = CASE
        WHEN CASE
            WHEN COALESCE((
                SELECT TOP(1) [g0].[HasSoulPatch]
                FROM [Gears] AS [g0]
                WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND ([g0].[Nickname] = N'Marcus')
            ), 0) = 1
            THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
        END = 1
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END) AND ([o].[FullName] = [w0].[OwnerFullName])
), [o].[Nickname], [o].[SquadId]",
                //
                @"SELECT [o.Reports].[Nickname], [o.Reports].[SquadId], [o.Reports].[AssignedCityName], [o.Reports].[CityOrBirthName], [o.Reports].[Discriminator], [o.Reports].[FullName], [o.Reports].[HasSoulPatch], [o.Reports].[LeaderNickname], [o.Reports].[LeaderSquadId], [o.Reports].[Rank], [t].[c], [t].[Nickname], [t].[SquadId]
FROM [Gears] AS [o.Reports]
INNER JOIN (
    SELECT (
        SELECT COUNT(*)
        FROM [Weapons] AS [w1]
        WHERE ([w1].[IsAutomatic] = CASE
            WHEN CASE
                WHEN CASE
                    WHEN COALESCE((
                        SELECT TOP(1) [g1].[HasSoulPatch]
                        FROM [Gears] AS [g1]
                        WHERE [g1].[Discriminator] IN (N'Officer', N'Gear') AND ([g1].[Nickname] = N'Marcus')
                    ), 0) = 1
                    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
                END = 1
                THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
            END = 1
            THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
        END) AND ([o0].[FullName] = [w1].[OwnerFullName])
    ) AS [c], [o0].[Nickname], [o0].[SquadId]
    FROM [Gears] AS [o0]
    WHERE [o0].[Discriminator] = N'Officer'
) AS [t] ON ([o.Reports].[LeaderNickname] = [t].[Nickname]) AND ([o.Reports].[LeaderSquadId] = [t].[SquadId])
WHERE [o.Reports].[Discriminator] IN (N'Officer', N'Gear') AND ([o.Reports].[HasSoulPatch] = 0)
ORDER BY [t].[c], [t].[Nickname], [t].[SquadId]");
        }
#endif

        public override async Task Cast_to_derived_type_after_OfType_works(bool isAsync)
        {
            await base.Cast_to_derived_type_after_OfType_works(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] = N'Officer'");
        }

        public override async Task Select_subquery_boolean(bool isAsync)
        {
            await base.Select_subquery_boolean(isAsync);

            AssertSql(
                @"SELECT CAST(COALESCE((
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]
), 0) AS bit)
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Select_subquery_boolean_with_pushdown(bool isAsync)
        {
            await base.Select_subquery_boolean_with_pushdown(isAsync);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]
)
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Select_subquery_int_with_inside_cast_and_coalesce(bool isAsync)
        {
            await base.Select_subquery_int_with_inside_cast_and_coalesce(isAsync);

            AssertSql(
                @"SELECT COALESCE((
    SELECT TOP(1) [w].[Id]
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]
), 42)
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Select_subquery_int_with_outside_cast_and_coalesce(bool isAsync)
        {
            await base.Select_subquery_int_with_outside_cast_and_coalesce(isAsync);

            AssertSql(
                @"SELECT COALESCE(COALESCE((
    SELECT TOP(1) [w].[Id]
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]
), 0), 42)
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Select_subquery_int_with_pushdown_and_coalesce(bool isAsync)
        {
            await base.Select_subquery_int_with_pushdown_and_coalesce(isAsync);

            AssertSql(
                @"SELECT COALESCE((
    SELECT TOP(1) [w].[Id]
    FROM [Weapons] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]
), 42)
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Select_subquery_boolean_empty(bool isAsync)
        {
            await base.Select_subquery_boolean_empty(isAsync);

            AssertSql(
                @"SELECT CAST(COALESCE((
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE ([w].[Name] = N'BFG') AND ([g].[FullName] = [w].[OwnerFullName])
    ORDER BY [w].[Id]
), 0) AS bit)
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Select_subquery_boolean_empty_with_pushdown(bool isAsync)
        {
            await base.Select_subquery_boolean_empty_with_pushdown(isAsync);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE ([w].[Name] = N'BFG') AND ([g].[FullName] = [w].[OwnerFullName])
    ORDER BY [w].[Id]
)
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override async Task Select_subquery_distinct_singleordefault_boolean1(bool isAsync)
        {
            await base.Select_subquery_distinct_singleordefault_boolean1(isAsync);

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)",
                //
                @"@_outer_FullName='Damon Baird' (Size = 450)

SELECT TOP(2) [t0].[IsAutomatic]
FROM (
    SELECT DISTINCT [w0].*
    FROM [Weapons] AS [w0]
    WHERE (CHARINDEX(N'Lancer', [w0].[Name]) > 0) AND (@_outer_FullName = [w0].[OwnerFullName])
) AS [t0]",
                //
                @"@_outer_FullName='Marcus Fenix' (Size = 450)

SELECT TOP(2) [t0].[IsAutomatic]
FROM (
    SELECT DISTINCT [w0].*
    FROM [Weapons] AS [w0]
    WHERE (CHARINDEX(N'Lancer', [w0].[Name]) > 0) AND (@_outer_FullName = [w0].[OwnerFullName])
) AS [t0]");
        }

        public override async Task Select_subquery_distinct_singleordefault_boolean2(bool isAsync)
        {
            await base.Select_subquery_distinct_singleordefault_boolean2(isAsync);

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)",
                //
                @"@_outer_FullName='Damon Baird' (Size = 450)

SELECT DISTINCT TOP(2) [w0].[IsAutomatic]
FROM [Weapons] AS [w0]
WHERE (CHARINDEX(N'Lancer', [w0].[Name]) > 0) AND (@_outer_FullName = [w0].[OwnerFullName])",
                //
                @"@_outer_FullName='Marcus Fenix' (Size = 450)

SELECT DISTINCT TOP(2) [w0].[IsAutomatic]
FROM [Weapons] AS [w0]
WHERE (CHARINDEX(N'Lancer', [w0].[Name]) > 0) AND (@_outer_FullName = [w0].[OwnerFullName])");
        }

        public override async Task Select_subquery_distinct_singleordefault_boolean_with_pushdown(bool isAsync)
        {
            await base.Select_subquery_distinct_singleordefault_boolean_with_pushdown(isAsync);

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)",
                //
                @"@_outer_FullName='Damon Baird' (Size = 450)

SELECT TOP(2) [t1].[IsAutomatic]
FROM (
    SELECT DISTINCT [w1].*
    FROM [Weapons] AS [w1]
    WHERE (CHARINDEX(N'Lancer', [w1].[Name]) > 0) AND (@_outer_FullName = [w1].[OwnerFullName])
) AS [t1]",
                //
                @"@_outer_FullName='Marcus Fenix' (Size = 450)

SELECT TOP(2) [t1].[IsAutomatic]
FROM (
    SELECT DISTINCT [w1].*
    FROM [Weapons] AS [w1]
    WHERE (CHARINDEX(N'Lancer', [w1].[Name]) > 0) AND (@_outer_FullName = [w1].[OwnerFullName])
) AS [t1]");
        }

        public override async Task Select_subquery_distinct_singleordefault_boolean_empty1(bool isAsync)
        {
            await base.Select_subquery_distinct_singleordefault_boolean_empty1(isAsync);

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)",
                //
                @"@_outer_FullName='Damon Baird' (Size = 450)

SELECT TOP(2) [t0].[IsAutomatic]
FROM (
    SELECT DISTINCT [w0].*
    FROM [Weapons] AS [w0]
    WHERE ([w0].[Name] = N'BFG') AND (@_outer_FullName = [w0].[OwnerFullName])
) AS [t0]",
                //
                @"@_outer_FullName='Marcus Fenix' (Size = 450)

SELECT TOP(2) [t0].[IsAutomatic]
FROM (
    SELECT DISTINCT [w0].*
    FROM [Weapons] AS [w0]
    WHERE ([w0].[Name] = N'BFG') AND (@_outer_FullName = [w0].[OwnerFullName])
) AS [t0]");
        }

        public override async Task Select_subquery_distinct_singleordefault_boolean_empty2(bool isAsync)
        {
            await base.Select_subquery_distinct_singleordefault_boolean_empty2(isAsync);

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)",
                //
                @"@_outer_FullName='Damon Baird' (Size = 450)

SELECT DISTINCT TOP(2) [w0].[IsAutomatic]
FROM [Weapons] AS [w0]
WHERE ([w0].[Name] = N'BFG') AND (@_outer_FullName = [w0].[OwnerFullName])",
                //
                @"@_outer_FullName='Marcus Fenix' (Size = 450)

SELECT DISTINCT TOP(2) [w0].[IsAutomatic]
FROM [Weapons] AS [w0]
WHERE ([w0].[Name] = N'BFG') AND (@_outer_FullName = [w0].[OwnerFullName])");
        }

        public override async Task Select_subquery_distinct_singleordefault_boolean_empty_with_pushdown(bool isAsync)
        {
            await base.Select_subquery_distinct_singleordefault_boolean_empty_with_pushdown(isAsync);

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)",
                //
                @"@_outer_FullName='Damon Baird' (Size = 450)

SELECT TOP(2) [t0].[IsAutomatic]
FROM (
    SELECT DISTINCT [w0].*
    FROM [Weapons] AS [w0]
    WHERE ([w0].[Name] = N'BFG') AND (@_outer_FullName = [w0].[OwnerFullName])
) AS [t0]",
                //
                @"@_outer_FullName='Marcus Fenix' (Size = 450)

SELECT TOP(2) [t0].[IsAutomatic]
FROM (
    SELECT DISTINCT [w0].*
    FROM [Weapons] AS [w0]
    WHERE ([w0].[Name] = N'BFG') AND (@_outer_FullName = [w0].[OwnerFullName])
) AS [t0]");
        }

        public override async Task Cast_subquery_to_base_type_using_typed_ToList(bool isAsync)
        {
            await base.Cast_subquery_to_base_type_using_typed_ToList(isAsync);

            AssertSql(
                @"SELECT [c].[Name]
FROM [Cities] AS [c]
WHERE [c].[Name] = N'Ephyra'
ORDER BY [c].[Name]",
                //
                @"SELECT [t].[Name], [c.StationedGears].[CityOrBirthName], [c.StationedGears].[FullName], [c.StationedGears].[HasSoulPatch], [c.StationedGears].[LeaderNickname], [c.StationedGears].[LeaderSquadId], [c.StationedGears].[Nickname], [c.StationedGears].[Rank], [c.StationedGears].[SquadId], [c.StationedGears].[AssignedCityName]
FROM [Gears] AS [c.StationedGears]
INNER JOIN (
    SELECT [c0].[Name]
    FROM [Cities] AS [c0]
    WHERE [c0].[Name] = N'Ephyra'
) AS [t] ON [c.StationedGears].[AssignedCityName] = [t].[Name]
WHERE [c.StationedGears].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[Name]");
        }

        public override async Task Cast_ordered_subquery_to_base_type_using_typed_ToArray(bool isAsync)
        {
            await base.Cast_ordered_subquery_to_base_type_using_typed_ToArray(isAsync);

            AssertSql(
                @"SELECT [c].[Name]
FROM [Cities] AS [c]
WHERE [c].[Name] = N'Ephyra'
ORDER BY [c].[Name]",
                //
                @"SELECT [t].[Name], [c.StationedGears].[CityOrBirthName], [c.StationedGears].[FullName], [c.StationedGears].[HasSoulPatch], [c.StationedGears].[LeaderNickname], [c.StationedGears].[LeaderSquadId], [c.StationedGears].[Nickname], [c.StationedGears].[Rank], [c.StationedGears].[SquadId], [c.StationedGears].[AssignedCityName]
FROM [Gears] AS [c.StationedGears]
INNER JOIN (
    SELECT [c0].[Name]
    FROM [Cities] AS [c0]
    WHERE [c0].[Name] = N'Ephyra'
) AS [t] ON [c.StationedGears].[AssignedCityName] = [t].[Name]
WHERE [c.StationedGears].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[Name], [c.StationedGears].[Nickname] DESC");
        }

#if !Test21
        public override async Task Correlated_collection_with_complex_order_by_funcletized_to_constant_bool(bool isAsync)
        {
            await base.Correlated_collection_with_complex_order_by_funcletized_to_constant_bool(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY (SELECT 1) DESC, [g].[Nickname], [g].[SquadId], [g].[FullName]",
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
#endif

        public override async Task Double_order_by_on_nullable_bool_coming_from_optional_navigation(bool isAsync)
        {
            await base.Double_order_by_on_nullable_bool_coming_from_optional_navigation(isAsync);

            AssertSql(
                @"SELECT [w.SynergyWith].[Id], [w.SynergyWith].[AmmunitionType], [w.SynergyWith].[IsAutomatic], [w.SynergyWith].[Name], [w.SynergyWith].[OwnerFullName], [w.SynergyWith].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w.SynergyWith] ON [w].[SynergyWithId] = [w.SynergyWith].[Id]
ORDER BY [w.SynergyWith].[IsAutomatic]");
        }

        public override async Task Double_order_by_on_Like(bool isAsync)
        {
            await base.Double_order_by_on_Like(isAsync);

            AssertSql(
                @"SELECT [w.SynergyWith].[Id], [w.SynergyWith].[AmmunitionType], [w.SynergyWith].[IsAutomatic], [w.SynergyWith].[Name], [w.SynergyWith].[OwnerFullName], [w.SynergyWith].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w.SynergyWith] ON [w].[SynergyWithId] = [w.SynergyWith].[Id]
ORDER BY CASE
    WHEN [w.SynergyWith].[Name] LIKE N'%Lancer'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override async Task Double_order_by_on_is_null(bool isAsync)
        {
            await base.Double_order_by_on_is_null(isAsync);

            AssertSql(
                @"SELECT [w.SynergyWith].[Id], [w.SynergyWith].[AmmunitionType], [w.SynergyWith].[IsAutomatic], [w.SynergyWith].[Name], [w.SynergyWith].[OwnerFullName], [w.SynergyWith].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w.SynergyWith] ON [w].[SynergyWithId] = [w.SynergyWith].[Id]
ORDER BY CASE
    WHEN [w.SynergyWith].[Name] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override async Task Double_order_by_on_string_compare(bool isAsync)
        {
            await base.Double_order_by_on_string_compare(isAsync);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
ORDER BY CASE
    WHEN [w].[Name] = N'Marcus'' Lancer'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override async Task Double_order_by_binary_expression(bool isAsync)
        {
            await base.Double_order_by_binary_expression(isAsync);

            AssertSql(
                @"SELECT [w].[Id] + 2 AS [Binary]
FROM [Weapons] AS [w]
ORDER BY [Binary]");
        }

        public override async Task String_compare_with_null_conditional_argument(bool isAsync)
        {
            await base.String_compare_with_null_conditional_argument(isAsync);

            AssertSql(
                @"SELECT [w.SynergyWith].[Id], [w.SynergyWith].[AmmunitionType], [w.SynergyWith].[IsAutomatic], [w.SynergyWith].[Name], [w.SynergyWith].[OwnerFullName], [w.SynergyWith].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w.SynergyWith] ON [w].[SynergyWithId] = [w.SynergyWith].[Id]
ORDER BY CASE
    WHEN [w.SynergyWith].[Name] = N'Marcus'' Lancer'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override async Task String_compare_with_null_conditional_argument2(bool isAsync)
        {
            await base.String_compare_with_null_conditional_argument2(isAsync);

            AssertSql(
                @"SELECT [w.SynergyWith].[Id], [w.SynergyWith].[AmmunitionType], [w.SynergyWith].[IsAutomatic], [w.SynergyWith].[Name], [w.SynergyWith].[OwnerFullName], [w.SynergyWith].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w.SynergyWith] ON [w].[SynergyWithId] = [w.SynergyWith].[Id]
ORDER BY CASE
    WHEN N'Marcus'' Lancer' = [w.SynergyWith].[Name]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override async Task String_concat_with_null_conditional_argument(bool isAsync)
        {
            await base.String_concat_with_null_conditional_argument(isAsync);

            AssertSql(
                @"SELECT [w.SynergyWith].[Id], [w.SynergyWith].[AmmunitionType], [w.SynergyWith].[IsAutomatic], [w.SynergyWith].[Name], [w.SynergyWith].[OwnerFullName], [w.SynergyWith].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w.SynergyWith] ON [w].[SynergyWithId] = [w.SynergyWith].[Id]
ORDER BY [w.SynergyWith].[Name] + CAST(5 AS nvarchar(max))");
        }

        public override async Task String_concat_with_null_conditional_argument2(bool isAsync)
        {
            await base.String_concat_with_null_conditional_argument2(isAsync);

            AssertSql(
                @"SELECT [w.SynergyWith].[Id], [w.SynergyWith].[AmmunitionType], [w.SynergyWith].[IsAutomatic], [w.SynergyWith].[Name], [w.SynergyWith].[OwnerFullName], [w.SynergyWith].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w.SynergyWith] ON [w].[SynergyWithId] = [w.SynergyWith].[Id]
ORDER BY [w.SynergyWith].[Name] + N'Marcus'' Lancer'");
        }

        public override async Task String_concat_on_various_types(bool isAsync)
        {
            await base.String_concat_on_various_types(isAsync);

            AssertSql(
                "");
        }

        public override async Task Time_of_day_datetimeoffset(bool isAsync)
        {
            await base.Time_of_day_datetimeoffset(isAsync);

            AssertSql(@"SELECT CAST([m].[Timeline] AS time)
FROM [Missions] AS [m]");
        }

        public override async Task GroupBy_Property_Include_Select_Average(bool isAsync)
        {
            await base.GroupBy_Property_Include_Select_Average(isAsync);

            AssertSql(
                @"SELECT AVG(CAST([g].[SquadId] AS float))
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
GROUP BY [g].[Rank]");
        }

        public override async Task GroupBy_Property_Include_Select_Sum(bool isAsync)
        {
            await base.GroupBy_Property_Include_Select_Sum(isAsync);

            AssertSql(
                @"SELECT SUM([g].[SquadId])
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
GROUP BY [g].[Rank]");
        }

        public override async Task GroupBy_Property_Include_Select_Count(bool isAsync)
        {
            await base.GroupBy_Property_Include_Select_Count(isAsync);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
GROUP BY [g].[Rank]");
        }

        public override async Task GroupBy_Property_Include_Select_LongCount(bool isAsync)
        {
            await base.GroupBy_Property_Include_Select_LongCount(isAsync);

            AssertSql(
                @"SELECT COUNT_BIG(*)
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
GROUP BY [g].[Rank]");
        }

        public override async Task GroupBy_Property_Include_Select_Min(bool isAsync)
        {
            await base.GroupBy_Property_Include_Select_Min(isAsync);

            AssertSql(
                @"SELECT MIN([g].[SquadId])
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
GROUP BY [g].[Rank]");
        }

        public override async Task GroupBy_Property_Include_Aggregate_with_anonymous_selector(bool isAsync)
        {
            await base.GroupBy_Property_Include_Aggregate_with_anonymous_selector(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname] AS [Key], COUNT(*) AS [c]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
GROUP BY [g].[Nickname]
ORDER BY [Key]");
        }

        public override async Task Group_by_entity_key_with_include_on_that_entity_with_key_in_result_selector(bool isAsync)
        {
            await base.Group_by_entity_key_with_include_on_that_entity_with_key_in_result_selector(isAsync);

            AssertSql(
                "");
        }

        public override async Task Group_by_entity_key_with_include_on_that_entity_with_key_in_result_selector_using_EF_Property(bool isAsync)
        {
            await base.Group_by_entity_key_with_include_on_that_entity_with_key_in_result_selector_using_EF_Property(isAsync);

            AssertSql(
                "");
        }

        public override async Task Group_by_with_include_with_entity_in_result_selector(bool isAsync)
        {
            await base.Group_by_with_include_with_entity_in_result_selector(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.CityOfBirth].[Name], [g.CityOfBirth].[Location]
FROM [Gears] AS [g]
INNER JOIN [Cities] AS [g.CityOfBirth] ON [g].[CityOrBirthName] = [g.CityOfBirth].[Name]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Rank]");
        }

        public override async Task GroupBy_Property_Include_Select_Max(bool isAsync)
        {
            await base.GroupBy_Property_Include_Select_Max(isAsync);

            AssertSql(
                @"SELECT MAX([g].[SquadId])
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
GROUP BY [g].[Rank]");
        }

        public override async Task Include_with_group_by_and_FirstOrDefault_gets_properly_applied(bool isAsync)
        {
            await base.Include_with_group_by_and_FirstOrDefault_gets_properly_applied(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.CityOfBirth].[Name], [g.CityOfBirth].[Location]
FROM [Gears] AS [g]
INNER JOIN [Cities] AS [g.CityOfBirth] ON [g].[CityOrBirthName] = [g.CityOfBirth].[Name]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Rank]");
        }

        public override async Task Include_collection_with_Cast_to_base(bool isAsync)
        {
            await base.Include_collection_with_Cast_to_base(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[FullName]",
                //
                @"SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId]
FROM [Weapons] AS [g.Weapons]
INNER JOIN (
    SELECT [g0].[FullName]
    FROM [Gears] AS [g0]
    WHERE [g0].[Discriminator] = N'Officer'
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
ORDER BY [t].[FullName]");
        }

        public override async Task Multiple_includes_with_client_method_around_qsre_and_also_projecting_included_collection()
        {
            await base.Multiple_includes_with_client_method_around_qsre_and_also_projecting_included_collection();

            AssertSql(
                @"SELECT [s].[Id], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE [s].[Name] = N'Delta'
ORDER BY [s].[Id]",
                //
                @"SELECT [s.Members].[Nickname], [s.Members].[SquadId], [s.Members].[AssignedCityName], [s.Members].[CityOrBirthName], [s.Members].[Discriminator], [s.Members].[FullName], [s.Members].[HasSoulPatch], [s.Members].[LeaderNickname], [s.Members].[LeaderSquadId], [s.Members].[Rank]
FROM [Gears] AS [s.Members]
INNER JOIN (
    SELECT [s0].[Id]
    FROM [Squads] AS [s0]
    WHERE [s0].[Name] = N'Delta'
) AS [t] ON [s.Members].[SquadId] = [t].[Id]
WHERE [s.Members].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[Id], [s.Members].[FullName]",
                //
                @"SELECT [s.Members.Weapons].[Id], [s.Members.Weapons].[AmmunitionType], [s.Members.Weapons].[IsAutomatic], [s.Members.Weapons].[Name], [s.Members.Weapons].[OwnerFullName], [s.Members.Weapons].[SynergyWithId]
FROM [Weapons] AS [s.Members.Weapons]
INNER JOIN (
    SELECT DISTINCT [s.Members0].[FullName], [t0].[Id]
    FROM [Gears] AS [s.Members0]
    INNER JOIN (
        SELECT [s1].[Id]
        FROM [Squads] AS [s1]
        WHERE [s1].[Name] = N'Delta'
    ) AS [t0] ON [s.Members0].[SquadId] = [t0].[Id]
    WHERE [s.Members0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t1] ON [s.Members.Weapons].[OwnerFullName] = [t1].[FullName]
ORDER BY [t1].[Id], [t1].[FullName]");
        }

        public override async Task OrderBy_same_expression_containing_IsNull_correctly_deduplicates_the_ordering(bool isAsync)
        {
            await base.OrderBy_same_expression_containing_IsNull_correctly_deduplicates_the_ordering(isAsync);

            AssertSql(
                @"SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL
    THEN CASE
        WHEN CAST(LEN([g].[Nickname]) AS int) = 5
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END ELSE NULL
END
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY CASE
    WHEN CASE
        WHEN [g].[LeaderNickname] IS NOT NULL
        THEN CASE
            WHEN CAST(LEN([g].[Nickname]) AS int) = 5
            THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
        END ELSE NULL
    END IS NOT NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override async Task GetValueOrDefault_in_projection(bool isAsync)
        {
            await base.GetValueOrDefault_in_projection(isAsync);

            AssertSql(
                @"SELECT COALESCE([w].[SynergyWithId], 0)
FROM [Weapons] AS [w]");
        }

        public override async Task GetValueOrDefault_in_filter(bool isAsync)
        {
            await base.GetValueOrDefault_in_filter(isAsync);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE COALESCE([w].[SynergyWithId], 0) = 0");
        }

        public override async Task GetValueOrDefault_in_filter_non_nullable_column(bool isAsync)
        {
            await base.GetValueOrDefault_in_filter_non_nullable_column(isAsync);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE COALESCE([w].[Id], 0) = 0");
        }

        public override async Task GetValueOrDefault_on_DateTimeOffset(bool isAsync)
        {
            await base.GetValueOrDefault_on_DateTimeOffset(isAsync);

            AssertSql(
                @"SELECT [m].[Id], [m].[CodeName], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]");
        }

        public override async Task GetValueOrDefault_in_order_by(bool isAsync)
        {
            await base.GetValueOrDefault_in_order_by(isAsync);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
ORDER BY COALESCE([w].[SynergyWithId], 0), [w].[Id]");
        }

        public override async Task GetValueOrDefault_with_argument(bool isAsync)
        {
            await base.GetValueOrDefault_with_argument(isAsync);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE COALESCE([w].[SynergyWithId], [w].[Id]) = 1");
        }

        public override async Task GetValueOrDefault_with_argument_complex(bool isAsync)
        {
            await base.GetValueOrDefault_with_argument_complex(isAsync);

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE COALESCE([w].[SynergyWithId], CAST(LEN([w].[Name]) AS int) + 42) > 10");
        }

        public override async Task Filter_with_compex_predicate_containig_subquery(bool isAsync)
        {
            await base.Filter_with_compex_predicate_containig_subquery(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[FullName] <> N'Dom') AND (
    SELECT TOP(1) [w].[Id]
    FROM [Weapons] AS [w]
    WHERE ([w].[IsAutomatic] = 1) AND ([g].[FullName] = [w].[OwnerFullName])
    ORDER BY [w].[Id]
) IS NOT NULL)");
        }

        public override async Task Query_with_complex_let_containing_ordering_and_filter_projecting_firstOrDefefault_element_of_let(bool isAsync)
        {
            await base.Query_with_complex_let_containing_ordering_and_filter_projecting_firstOrDefefault_element_of_let(isAsync);

            AssertSql(
                @"SELECT [g].[Nickname], (
    SELECT TOP(1) [w].[Name]
    FROM [Weapons] AS [w]
    WHERE ([w].[IsAutomatic] = 1) AND ([g].[FullName] = [w].[OwnerFullName])
    ORDER BY [w].[AmmunitionType] DESC
) AS [WeaponName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] <> N'Dom')");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        private void AssertContainsSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, assertOrder: false);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
