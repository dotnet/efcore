// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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

        public override void Entity_equality_empty()
        {
            base.Entity_equality_empty();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] IS NULL AND ([g].[SquadId] = 0))");
        }

        public override void Include_multiple_one_to_one_and_one_to_many()
        {
            base.Include_multiple_one_to_one_and_one_to_many();

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

        public override void Include_multiple_one_to_one_and_one_to_many_self_reference()
        {
            base.Include_multiple_one_to_one_and_one_to_many_self_reference();

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

        public override void Include_multiple_one_to_one_and_one_to_one_and_one_to_many()
        {
            base.Include_multiple_one_to_one_and_one_to_one_and_one_to_many();

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

        public override void Include_multiple_one_to_one_optional_and_one_to_one_required()
        {
            base.Include_multiple_one_to_one_optional_and_one_to_one_required();

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

        public override void Include_multiple_circular()
        {
            base.Include_multiple_circular();

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

        public override void Include_multiple_circular_with_filter()
        {
            base.Include_multiple_circular_with_filter();

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

        public override void Include_using_alternate_key()
        {
            base.Include_using_alternate_key();

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

        public override void Include_multiple_include_then_include()
        {
            base.Include_multiple_include_then_include();

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

        public override void Include_navigation_on_derived_type()
        {
            base.Include_navigation_on_derived_type();

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

        public override void String_based_Include_navigation_on_derived_type()
        {
            base.String_based_Include_navigation_on_derived_type();

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

        public override void Select_Where_Navigation_Included()
        {
            base.Select_Where_Navigation_Included();

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

        public override void Include_with_join_reference1()
        {
            base.Include_with_join_reference1();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.CityOfBirth].[Name], [g.CityOfBirth].[Location]
FROM [Gears] AS [g]
INNER JOIN [Cities] AS [g.CityOfBirth] ON [g].[CityOrBirthName] = [g.CityOfBirth].[Name]
INNER JOIN [Tags] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override void Include_with_join_reference2()
        {
            base.Include_with_join_reference2();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.CityOfBirth].[Name], [g.CityOfBirth].[Location]
FROM [Tags] AS [t]
INNER JOIN [Gears] AS [g] ON ([t].[GearSquadId] = [g].[SquadId]) AND ([t].[GearNickName] = [g].[Nickname])
INNER JOIN [Cities] AS [g.CityOfBirth] ON [g].[CityOrBirthName] = [g.CityOfBirth].[Name]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override void Include_with_join_collection1()
        {
            base.Include_with_join_collection1();

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

        public override void Include_with_join_collection2()
        {
            base.Include_with_join_collection2();

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

        public override void Include_where_list_contains_navigation()
        {
            base.Include_where_list_contains_navigation();

            AssertSql(
                @"SELECT [t].[Id]
FROM [Tags] AS [t]");

            Assert.Contains(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Tag].[Id], [g.Tag].[GearNickName], [g.Tag].[GearSquadId], [g.Tag].[Note]" + _eol +
                @"FROM [Gears] AS [g]" + _eol +
                @"LEFT JOIN [Tags] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])" + _eol +
                @"WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g.Tag].[Id] IS NOT NULL AND [g.Tag].[Id] IN (",
                Fixture.TestSqlLoggerFactory.SqlStatements[1]);
        }

        public override void Include_where_list_contains_navigation2()
        {
            base.Include_where_list_contains_navigation2();

            AssertSql(
                @"SELECT [t].[Id]
FROM [Tags] AS [t]");

            Assert.Contains(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Tag].[Id], [g.Tag].[GearNickName], [g.Tag].[GearSquadId], [g.Tag].[Note]" + _eol +
                @"FROM [Gears] AS [g]" + _eol +
                @"LEFT JOIN [Tags] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])" + _eol +
                @"INNER JOIN [Cities] AS [g.CityOfBirth] ON [g].[CityOrBirthName] = [g.CityOfBirth].[Name]" + _eol +
                @"WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g.CityOfBirth].[Location] IS NOT NULL AND [g.Tag].[Id] IN (",
                Fixture.TestSqlLoggerFactory.SqlStatements[1]);
        }

        public override void Navigation_accessed_twice_outside_and_inside_subquery()
        {
            base.Navigation_accessed_twice_outside_and_inside_subquery();

            AssertSql(
                @"SELECT [t].[Id]
FROM [Tags] AS [t]");

            Assert.Contains(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]" + _eol +
                @"FROM [Gears] AS [g]" + _eol +
                @"LEFT JOIN [Tags] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])" + _eol +
                @"WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g.Tag].[Id] IS NOT NULL AND [g.Tag].[Id] IN (",
                Fixture.TestSqlLoggerFactory.SqlStatements[1]);
        }

        public override void Include_with_join_multi_level()
        {
            base.Include_with_join_multi_level();

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

        public override void Include_with_join_and_inheritance1()
        {
            base.Include_with_join_and_inheritance1();

            AssertSql(
                @"SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOrBirthName], [o].[Discriminator], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], [o.CityOfBirth].[Name], [o.CityOfBirth].[Location]
FROM [Tags] AS [t]
INNER JOIN [Gears] AS [o] ON ([t].[GearSquadId] = [o].[SquadId]) AND ([t].[GearNickName] = [o].[Nickname])
INNER JOIN [Cities] AS [o.CityOfBirth] ON [o].[CityOrBirthName] = [o.CityOfBirth].[Name]
WHERE [o].[Discriminator] = N'Officer'");
        }

        public override void Include_with_join_and_inheritance2()
        {
            base.Include_with_join_and_inheritance2();

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

        public override void Include_with_join_and_inheritance3()
        {
            base.Include_with_join_and_inheritance3();

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

        public override void Include_with_nested_navigation_in_order_by()
        {
            base.Include_with_nested_navigation_in_order_by();

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

        public override void Where_enum()
        {
            base.Where_enum();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Rank] = 2)");
        }

        public override void Where_nullable_enum_with_constant()
        {
            base.Where_nullable_enum_with_constant();

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] = 1");
        }

        public override void Where_nullable_enum_with_null_constant()
        {
            base.Where_nullable_enum_with_null_constant();

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] IS NULL");
        }

        public override void Where_nullable_enum_with_non_nullable_parameter()
        {
            base.Where_nullable_enum_with_non_nullable_parameter();

            AssertSql(
                @"@__ammunitionType_0='1'

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] = @__ammunitionType_0");
        }

        public override void Where_nullable_enum_with_nullable_parameter()
        {
            base.Where_nullable_enum_with_nullable_parameter();

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

        public override void Where_bitwise_and_enum()
        {
            base.Where_bitwise_and_enum();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) > 0)",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) = 1)");
        }

        public override void Where_bitwise_and_integral()
        {
            base.Where_bitwise_and_integral();

            AssertSql(
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
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) = 1)",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) = 1)");
        }

        public override void Where_bitwise_and_nullable_enum_with_constant()
        {
            base.Where_bitwise_and_nullable_enum_with_constant();

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & 1) > 0");
        }

        public override void Where_bitwise_and_nullable_enum_with_null_constant()
        {
            base.Where_bitwise_and_nullable_enum_with_null_constant();

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & NULL) > 0");
        }

        public override void Where_bitwise_and_nullable_enum_with_non_nullable_parameter()
        {
            base.Where_bitwise_and_nullable_enum_with_non_nullable_parameter();

            AssertSql(
                @"@__ammunitionType_0='1'

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & @__ammunitionType_0) > 0");
        }

        public override void Where_bitwise_and_nullable_enum_with_nullable_parameter()
        {
            base.Where_bitwise_and_nullable_enum_with_nullable_parameter();

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
        }

        public override void Where_bitwise_or_enum()
        {
            base.Where_bitwise_or_enum();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] | 1) > 0)");
        }

        public override void Bitwise_projects_values_in_select()
        {
            base.Bitwise_projects_values_in_select();

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

        public override void Where_enum_has_flag()
        {
            base.Where_enum_has_flag();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) = 1)",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 5) = 5)",
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

        public override void Where_enum_has_flag_subquery()
        {
            base.Where_enum_has_flag_subquery();

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

        public override void Where_enum_has_flag_subquery_client_eval()
        {
            base.Where_enum_has_flag_subquery_client_eval();

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

        public override void Where_enum_has_flag_with_non_nullable_parameter()
        {
            base.Where_enum_has_flag_with_non_nullable_parameter();

            AssertSql(
                @"@__parameter_0='1'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & @__parameter_0) = @__parameter_0)");
        }

        public override void Where_has_flag_with_nullable_parameter()
        {
            base.Where_has_flag_with_nullable_parameter();

            AssertSql(
                @"@__parameter_0='1' (Nullable = true)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & @__parameter_0) = @__parameter_0)");
        }

        public override void Select_enum_has_flag()
        {
            base.Select_enum_has_flag();

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

        public override void Where_count_subquery_without_collision()
        {
            base.Where_count_subquery_without_collision();

            AssertSql(
                @"SELECT [w].[Nickname], [w].[SquadId], [w].[AssignedCityName], [w].[CityOrBirthName], [w].[Discriminator], [w].[FullName], [w].[HasSoulPatch], [w].[LeaderNickname], [w].[LeaderSquadId], [w].[Rank]
FROM [Gears] AS [w]
WHERE [w].[Discriminator] IN (N'Officer', N'Gear') AND ((
    SELECT COUNT(*)
    FROM [Weapons] AS [w0]
    WHERE [w].[FullName] = [w0].[OwnerFullName]
) = 2)");
        }

        public override void Where_any_subquery_without_collision()
        {
            base.Where_any_subquery_without_collision();

            AssertSql(
                @"SELECT [w].[Nickname], [w].[SquadId], [w].[AssignedCityName], [w].[CityOrBirthName], [w].[Discriminator], [w].[FullName], [w].[HasSoulPatch], [w].[LeaderNickname], [w].[LeaderSquadId], [w].[Rank]
FROM [Gears] AS [w]
WHERE [w].[Discriminator] IN (N'Officer', N'Gear') AND EXISTS (
    SELECT 1
    FROM [Weapons] AS [w0]
    WHERE [w].[FullName] = [w0].[OwnerFullName])");
        }

        public override void Select_inverted_boolean()
        {
            base.Select_inverted_boolean();

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = 0
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END AS [Manual]
FROM [Weapons] AS [w]
WHERE [w].[IsAutomatic] = 1");
        }

        public override void Select_comparison_with_null()
        {
            base.Select_comparison_with_null();

            AssertSql(
                @"@__ammunitionType_1='1' (Nullable = true)
@__ammunitionType_0='1' (Nullable = true)

SELECT [w].[Id], CASE
    WHEN [w].[AmmunitionType] = @__ammunitionType_1
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

        public override void Select_ternary_operation_with_boolean()
        {
            base.Select_ternary_operation_with_boolean();

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = 1
    THEN 1 ELSE 0
END AS [Num]
FROM [Weapons] AS [w]");
        }

        public override void Select_ternary_operation_with_inverted_boolean()
        {
            base.Select_ternary_operation_with_inverted_boolean();

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = 0
    THEN 1 ELSE 0
END AS [Num]
FROM [Weapons] AS [w]");
        }

        public override void Select_ternary_operation_with_has_value_not_null()
        {
            base.Select_ternary_operation_with_has_value_not_null();

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN [w].[AmmunitionType] IS NOT NULL AND ([w].[AmmunitionType] = 1)
    THEN N'Yes' ELSE N'No'
END AS [IsCartidge]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] IS NOT NULL AND ([w].[AmmunitionType] = 1)");
        }

        public override void Select_ternary_operation_multiple_conditions()
        {
            base.Select_ternary_operation_multiple_conditions();

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN ([w].[AmmunitionType] = 2) AND ([w].[SynergyWithId] = 1)
    THEN N'Yes' ELSE N'No'
END AS [IsCartidge]
FROM [Weapons] AS [w]");
        }

        public override void Select_ternary_operation_multiple_conditions_2()
        {
            base.Select_ternary_operation_multiple_conditions_2();

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN ([w].[IsAutomatic] = 0) AND ([w].[SynergyWithId] = 1)
    THEN N'Yes' ELSE N'No'
END AS [IsCartidge]
FROM [Weapons] AS [w]");
        }

        public override void Select_multiple_conditions()
        {
            base.Select_multiple_conditions();

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN ([w].[IsAutomatic] = 0) AND ([w].[SynergyWithId] = 1)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END AS [IsCartidge]
FROM [Weapons] AS [w]");
        }

        public override void Select_nested_ternary_operations()
        {
            base.Select_nested_ternary_operations();

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

        public override void Null_propagation_optimization1()
        {
            base.Null_propagation_optimization1();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[LeaderNickname] = N'Marcus')");
        }

        public override void Null_propagation_optimization2()
        {
            base.Null_propagation_optimization2();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (RIGHT([g].[LeaderNickname], LEN(N'us')) = N'us')");
        }

        public override void Null_propagation_optimization3()
        {
            base.Null_propagation_optimization3();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (RIGHT([g].[LeaderNickname], LEN(N'us')) = N'us')");
        }

        public override void Null_propagation_optimization4()
        {
            base.Null_propagation_optimization4();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (CAST(LEN([g].[LeaderNickname]) AS int) = 5)");
        }

        public override void Null_propagation_optimization5()
        {
            base.Null_propagation_optimization5();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (CAST(LEN([g].[LeaderNickname]) AS int) = 5)");
        }

        public override void Null_propagation_optimization6()
        {
            base.Null_propagation_optimization6();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (CAST(LEN([g].[LeaderNickname]) AS int) = 5)");
        }

        public override void Select_null_propagation_optimization7()
        {
            base.Select_null_propagation_optimization7();

            AssertSql(
                @"SELECT [g].[LeaderNickname] + [g].[LeaderNickname]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override void Select_null_propagation_optimization8()
        {
            base.Select_null_propagation_optimization8();

            AssertSql(
                @"SELECT [g].[LeaderNickname] + [g].[LeaderNickname]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override void Select_null_propagation_optimization9()
        {
            base.Select_null_propagation_optimization9();

            AssertSql(
                @"SELECT CAST(LEN([g].[FullName]) AS int)
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override void Select_null_propagation_negative1()
        {
            base.Select_null_propagation_negative1();

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

        public override void Select_null_propagation_negative2()
        {
            base.Select_null_propagation_negative2();

            AssertSql(
                @"SELECT CASE
    WHEN [g1].[LeaderNickname] IS NOT NULL
    THEN [g2].[LeaderNickname] ELSE NULL
END
FROM [Gears] AS [g1]
CROSS JOIN [Gears] AS [g2]
WHERE [g1].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override void Select_null_propagation_negative3()
        {
            base.Select_null_propagation_negative3();

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

        public override void Select_null_propagation_negative4()
        {
            base.Select_null_propagation_negative4();

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

        public override void Select_null_propagation_negative5()
        {
            base.Select_null_propagation_negative5();

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

        public override void Select_null_propagation_negative6()
        {
            base.Select_null_propagation_negative6();

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

        public override void Select_null_propagation_negative7()
        {
            base.Select_null_propagation_negative7();

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

        public override void Select_null_propagation_negative8()
        {
            base.Select_null_propagation_negative8();

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

        public override void Select_null_propagation_works_for_navigations_with_composite_keys()
        {
            base.Select_null_propagation_works_for_navigations_with_composite_keys();

            AssertSql(
                @"SELECT [t0].[Nickname]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])");
        }

        public override void Select_null_propagation_works_for_multiple_navigations_with_composite_keys()
        {
            base.Select_null_propagation_works_for_multiple_navigations_with_composite_keys();

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

        public override void Select_conditional_with_anonymous_type_and_null_constant()
        {
            base.Select_conditional_with_anonymous_type_and_null_constant();

            AssertSql(
                @"SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END, [g].[HasSoulPatch]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]");
        }

        public override void Select_conditional_with_anonymous_types()
        {
            base.Select_conditional_with_anonymous_types();

            AssertSql(
                @"SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END, [g].[Nickname] AS [Name], [g].[FullName] AS [Name0]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [Name]");
        }

        public override void Where_conditional_with_anonymous_type()
        {
            base.Where_conditional_with_anonymous_type();

            AssertSql(
                @"SELECT [g].[LeaderNickname], [g].[HasSoulPatch], [g].[Nickname]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]");
        }

        public override void Select_coalesce_with_anonymous_types()
        {
            base.Select_coalesce_with_anonymous_types();

            AssertSql(
                @"SELECT [g].[LeaderNickname] AS [Name], [g].[FullName] AS [Name0]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname]");
        }

        public override void Where_coalesce_with_anonymous_types()
        {
            base.Where_coalesce_with_anonymous_types();

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

        public override void Where_member_access_on_anonymous_type()
        {
            base.Where_member_access_on_anonymous_type();

            AssertSql(
                @"SELECT [g].[Nickname]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[LeaderNickname] = N'Marcus')");
        }

        public override void Where_compare_anonymous_types_with_uncorrelated_members()
        {
            base.Where_compare_anonymous_types_with_uncorrelated_members();

            AssertSql(
                @"SELECT [g].[Nickname]
FROM [Gears] AS [g]
WHERE 0 = 1");
        }

        public override void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar()
        {
            base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar();

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

        public override void Select_Singleton_Navigation_With_Member_Access()
        {
            base.Select_Singleton_Navigation_With_Member_Access();

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

        public override void Select_Where_Navigation()
        {
            base.Select_Where_Navigation();

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

        public override void Select_Where_Navigation_Client()
        {
            base.Select_Where_Navigation_Client();

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

        public override void Select_Where_Navigation_Equals_Navigation()
        {
            base.Select_Where_Navigation_Equals_Navigation();

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

        public override void Select_Where_Navigation_Null()
        {
            base.Select_Where_Navigation_Null();

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [Tags] AS [t]
WHERE [t].[GearNickName] IS NULL AND [t].[GearSquadId] IS NULL");
        }

        public override void Select_Where_Navigation_Null_Reverse()
        {
            base.Select_Where_Navigation_Null_Reverse();

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [Tags] AS [t]
WHERE [t].[GearNickName] IS NULL AND [t].[GearSquadId] IS NULL");
        }

        public override void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected()
        {
            base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected();

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

        public override void Optional_Navigation_Null_Coalesce_To_Clr_Type()
        {
            base.Optional_Navigation_Null_Coalesce_To_Clr_Type();

            AssertSql(
                @"SELECT TOP(1) CAST(COALESCE([w.SynergyWith].[IsAutomatic], 0) AS bit) AS [IsAutomatic]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w.SynergyWith] ON [w].[SynergyWithId] = [w.SynergyWith].[Id]
ORDER BY [w].[Id]");
        }

        public override void Where_subquery_boolean()
        {
            base.Where_subquery_boolean();

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

        public override void Where_subquery_distinct_firstordefault_boolean()
        {
            base.Where_subquery_distinct_firstordefault_boolean();

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

        public override void Where_subquery_distinct_first_boolean()
        {
            base.Where_subquery_distinct_first_boolean();

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

        public override void Where_subquery_distinct_singleordefault_boolean()
        {
            base.Where_subquery_distinct_singleordefault_boolean();

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
                @"");
        }

        public override void Where_subquery_distinct_last_boolean()
        {
            base.Where_subquery_distinct_last_boolean();

            AssertSql(
                @"");
        }

        public override void Where_subquery_distinct_orderby_firstordefault_boolean()
        {
            base.Where_subquery_distinct_orderby_firstordefault_boolean();

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

        public override void Where_subquery_union_firstordefault_boolean()
        {
            base.Where_subquery_union_firstordefault_boolean();

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

        public override void Where_subquery_concat_firstordefault_boolean()
        {
            base.Where_subquery_concat_firstordefault_boolean();

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

        public override void Concat_with_count()
        {
            base.Concat_with_count();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')",
                //
                @"SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOrBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gears] AS [g0]
WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override void Concat_scalars_with_count()
        {
            base.Concat_scalars_with_count();

            AssertSql(
                @"SELECT [g].[Nickname]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')",
                //
                @"SELECT [g2].[FullName]
FROM [Gears] AS [g2]
WHERE [g2].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override void Concat_anonymous_with_count()
        {
            base.Concat_anonymous_with_count();

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
                @"");
        }

        public override void Select_navigation_with_concat_and_count()
        {
            base.Select_navigation_with_concat_and_count();

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

        public override void Concat_with_groupings()
        {
            base.Concat_with_groupings();

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

        public override void Concat_with_collection_navigations()
        {
            base.Concat_with_collection_navigations();

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

        public override void Union_with_collection_navigations()
        {
            base.Union_with_collection_navigations();

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

        public override void Select_subquery_distinct_firstordefault()
        {
            base.Select_subquery_distinct_firstordefault();

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

        public override void Singleton_Navigation_With_Member_Access()
        {
            base.Singleton_Navigation_With_Member_Access();

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

        public override void GroupJoin_Composite_Key()
        {
            base.GroupJoin_Composite_Key();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Tags] AS [t]
INNER JOIN [Gears] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override void Join_navigation_translated_to_subquery_composite_key()
        {
            base.Join_navigation_translated_to_subquery_composite_key();

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

        public override void Collection_with_inheritance_and_join_include_joined()
        {
            base.Collection_with_inheritance_and_join_include_joined();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Tag].[Id], [g.Tag].[GearNickName], [g.Tag].[GearSquadId], [g.Tag].[Note]
FROM [Tags] AS [t]
INNER JOIN [Gears] AS [g] ON ([t].[GearSquadId] = [g].[SquadId]) AND ([t].[GearNickName] = [g].[Nickname])
LEFT JOIN [Tags] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE [g].[Discriminator] = N'Officer'");
        }

        public override void Collection_with_inheritance_and_join_include_source()
        {
            base.Collection_with_inheritance_and_join_include_source();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Tag].[Id], [g.Tag].[GearNickName], [g.Tag].[GearSquadId], [g.Tag].[Note]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
INNER JOIN [Tags] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
WHERE [g].[Discriminator] = N'Officer'");
        }

        public override void Non_unicode_string_literal_is_used_for_non_unicode_column()
        {
            base.Non_unicode_string_literal_is_used_for_non_unicode_column();

            AssertSql(
                @"SELECT [c].[Name], [c].[Location]
FROM [Cities] AS [c]
WHERE [c].[Location] = 'Unknown'");
        }

        public override void Non_unicode_string_literal_is_used_for_non_unicode_column_right()
        {
            base.Non_unicode_string_literal_is_used_for_non_unicode_column_right();

            AssertSql(
                @"SELECT [c].[Name], [c].[Location]
FROM [Cities] AS [c]
WHERE 'Unknown' = [c].[Location]");
        }

        public override void Non_unicode_parameter_is_used_for_non_unicode_column()
        {
            base.Non_unicode_parameter_is_used_for_non_unicode_column();

            AssertSql(
                @"@__value_0='Unknown' (Size = 100) (DbType = AnsiString)

SELECT [c].[Name], [c].[Location]
FROM [Cities] AS [c]
WHERE [c].[Location] = @__value_0");
        }

        public override void Non_unicode_string_literals_in_contains_is_used_for_non_unicode_column()
        {
            base.Non_unicode_string_literals_in_contains_is_used_for_non_unicode_column();

            AssertSql(
                @"SELECT [c].[Name], [c].[Location]
FROM [Cities] AS [c]
WHERE [c].[Location] IN ('Unknown', 'Jacinto''s location', 'Ephyra''s location')");
        }

        public override void Non_unicode_string_literals_is_used_for_non_unicode_column_with_subquery()
        {
            base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_subquery();

            AssertSql(
                @"SELECT [c].[Name], [c].[Location]
FROM [Cities] AS [c]
WHERE ([c].[Location] = 'Unknown') AND ((
    SELECT COUNT(*)
    FROM [Gears] AS [g]
    WHERE ([g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] = N'Paduk')) AND ([c].[Name] = [g].[CityOrBirthName])
) = 1)");
        }

        public override void Non_unicode_string_literals_is_used_for_non_unicode_column_in_subquery()
        {
            base.Non_unicode_string_literals_is_used_for_non_unicode_column_in_subquery();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
INNER JOIN [Cities] AS [g.CityOfBirth] ON [g].[CityOrBirthName] = [g.CityOfBirth].[Name]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Nickname] = N'Marcus') AND ([g.CityOfBirth].[Location] = 'Jacinto''s location'))");
        }

        public override void Non_unicode_string_literals_is_used_for_non_unicode_column_with_contains()
        {
            base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_contains();

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

        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3();

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

        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result();

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

        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result();

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

        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result();

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

        public override void Coalesce_operator_in_predicate()
        {
            base.Coalesce_operator_in_predicate();

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE COALESCE([w].[IsAutomatic], 0) = 1");
        }

        public override void Coalesce_operator_in_predicate_with_other_conditions()
        {
            base.Coalesce_operator_in_predicate_with_other_conditions();

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] = 1) AND (COALESCE([w].[IsAutomatic], 0) = 1)");
        }

        public override void Coalesce_operator_in_projection_with_other_conditions()
        {
            base.Coalesce_operator_in_projection_with_other_conditions();

            AssertSql(
                @"SELECT CASE
    WHEN ([w].[AmmunitionType] = 1) AND (COALESCE([w].[IsAutomatic], 0) = 1)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
FROM [Weapons] AS [w]");
        }

        public override void Optional_navigation_type_compensation_works_with_predicate()
        {
            base.Optional_navigation_type_compensation_works_with_predicate();

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

        public override void Optional_navigation_type_compensation_works_with_predicate2()
        {
            base.Optional_navigation_type_compensation_works_with_predicate2();

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

        public override void Optional_navigation_type_compensation_works_with_predicate_negated()
        {
            base.Optional_navigation_type_compensation_works_with_predicate_negated();

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

        public override void Optional_navigation_type_compensation_works_with_predicate_negated_complex1()
        {
            base.Optional_navigation_type_compensation_works_with_predicate_negated_complex1();

            AssertSql(
                @"");
        }

        public override void Optional_navigation_type_compensation_works_with_predicate_negated_complex2()
        {
            base.Optional_navigation_type_compensation_works_with_predicate_negated_complex2();

            AssertSql(
                @"");
        }

        public override void Optional_navigation_type_compensation_works_with_conditional_expression()
        {
            base.Optional_navigation_type_compensation_works_with_conditional_expression();

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

        public override void Optional_navigation_type_compensation_works_with_binary_expression()
        {
            base.Optional_navigation_type_compensation_works_with_binary_expression();

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

        public override void Optional_navigation_type_compensation_works_with_projection()
        {
            base.Optional_navigation_type_compensation_works_with_projection();

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

        public override void Optional_navigation_type_compensation_works_with_projection_into_anonymous_type()
        {
            base.Optional_navigation_type_compensation_works_with_projection_into_anonymous_type();

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

        public override void Optional_navigation_type_compensation_works_with_DTOs()
        {
            base.Optional_navigation_type_compensation_works_with_DTOs();

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

        public override void Optional_navigation_type_compensation_works_with_list_initializers()
        {
            base.Optional_navigation_type_compensation_works_with_list_initializers();

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

        public override void Optional_navigation_type_compensation_works_with_array_initializers()
        {
            base.Optional_navigation_type_compensation_works_with_array_initializers();

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

        public override void Optional_navigation_type_compensation_works_with_orderby()
        {
            base.Optional_navigation_type_compensation_works_with_orderby();

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

        public override void Optional_navigation_type_compensation_works_with_groupby()
        {
            base.Optional_navigation_type_compensation_works_with_groupby();

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

        public override void Optional_navigation_type_compensation_works_with_all()
        {
            base.Optional_navigation_type_compensation_works_with_all();

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

        public override void Optional_navigation_type_compensation_works_with_contains()
        {
            base.Optional_navigation_type_compensation_works_with_contains();

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

        public override void Optional_navigation_type_compensation_works_with_skip()
        {
            base.Optional_navigation_type_compensation_works_with_skip();

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

        public override void Optional_navigation_type_compensation_works_with_take()
        {
            base.Optional_navigation_type_compensation_works_with_take();

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

        public override void Select_correlated_filtered_collection()
        {
            base.Select_correlated_filtered_collection();

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

        public override void Select_correlated_filtered_collection_with_composite_key()
        {
            base.Select_correlated_filtered_collection_with_composite_key();

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

        public override void Select_correlated_filtered_collection_works_with_caching()
        {
            base.Select_correlated_filtered_collection_works_with_caching();

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

        public override void Join_predicate_value_equals_condition()
        {
            base.Join_predicate_value_equals_condition();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
INNER JOIN [Weapons] AS [w] ON [w].[SynergyWithId] IS NOT NULL
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override void Join_predicate_value()
        {
            base.Join_predicate_value();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
INNER JOIN [Weapons] AS [w] ON [g].[HasSoulPatch] = 1
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override void Join_predicate_condition_equals_condition()
        {
            base.Join_predicate_condition_equals_condition();

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

        public override void Left_join_predicate_value_equals_condition()
        {
            base.Left_join_predicate_value_equals_condition();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Weapons] AS [w] ON [w].[SynergyWithId] IS NOT NULL
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override void Left_join_predicate_value()
        {
            base.Left_join_predicate_value();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Weapons] AS [w] ON [g].[HasSoulPatch] = 1
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override void Left_join_predicate_condition_equals_condition()
        {
            base.Left_join_predicate_condition_equals_condition();

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

        public override void Where_datetimeoffset_now()
        {
            base.Where_datetimeoffset_now();

            AssertSql(
                @"SELECT [m].[Id], [m].[CodeName], [m].[Timeline]
FROM [Missions] AS [m]
WHERE [m].[Timeline] <> CAST(GETDATE() AS datetimeoffset)");
        }

        public override void Where_datetimeoffset_utcnow()
        {
            base.Where_datetimeoffset_utcnow();

            AssertSql(
                @"SELECT [m].[Id], [m].[CodeName], [m].[Timeline]
FROM [Missions] AS [m]
WHERE [m].[Timeline] <> CAST(GETUTCDATE() AS datetimeoffset)");
        }

        public override void Where_datetimeoffset_date_component()
        {
            base.Where_datetimeoffset_date_component();

            AssertSql(
                @"@__Date_0='0001-01-01T00:00:00'

SELECT [m].[Id], [m].[CodeName], [m].[Timeline]
FROM [Missions] AS [m]
WHERE CONVERT(date, [m].[Timeline]) > @__Date_0");
        }

        public override void Where_datetimeoffset_year_component()
        {
            base.Where_datetimeoffset_year_component();

            AssertSql(
                @"SELECT [m].[Id], [m].[CodeName], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(year, [m].[Timeline]) = 2");
        }

        public override void Where_datetimeoffset_month_component()
        {
            base.Where_datetimeoffset_month_component();

            AssertSql(
                @"SELECT [m].[Id], [m].[CodeName], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(month, [m].[Timeline]) = 1");
        }

        public override void Where_datetimeoffset_dayofyear_component()
        {
            base.Where_datetimeoffset_dayofyear_component();

            AssertSql(
                @"SELECT [m].[Id], [m].[CodeName], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(dayofyear, [m].[Timeline]) = 2");
        }

        public override void Where_datetimeoffset_day_component()
        {
            base.Where_datetimeoffset_day_component();

            AssertSql(
                @"SELECT [m].[Id], [m].[CodeName], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(day, [m].[Timeline]) = 2");
        }

        public override void Where_datetimeoffset_hour_component()
        {
            base.Where_datetimeoffset_hour_component();

            AssertSql(
                @"SELECT [m].[Id], [m].[CodeName], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(hour, [m].[Timeline]) = 10");
        }

        public override void Where_datetimeoffset_minute_component()
        {
            base.Where_datetimeoffset_minute_component();

            AssertSql(
                @"SELECT [m].[Id], [m].[CodeName], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(minute, [m].[Timeline]) = 0");
        }

        public override void Where_datetimeoffset_second_component()
        {
            base.Where_datetimeoffset_second_component();

            AssertSql(
                @"SELECT [m].[Id], [m].[CodeName], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(second, [m].[Timeline]) = 0");
        }

        public override void Where_datetimeoffset_millisecond_component()
        {
            base.Where_datetimeoffset_millisecond_component();

            AssertSql(
                @"SELECT [m].[Id], [m].[CodeName], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(millisecond, [m].[Timeline]) = 0");
        }

        public override void DateTimeOffset_DateAdd_AddMonths()
        {
            base.DateTimeOffset_DateAdd_AddMonths();

            AssertSql(
                @"SELECT DATEADD(month, 1, [m].[Timeline])
FROM [Missions] AS [m]");
        }

        public override void DateTimeOffset_DateAdd_AddDays()
        {
            base.DateTimeOffset_DateAdd_AddDays();

            AssertSql(
                @"SELECT DATEADD(day, 1E0, [m].[Timeline])
FROM [Missions] AS [m]");
        }

        public override void DateTimeOffset_DateAdd_AddHours()
        {
            base.DateTimeOffset_DateAdd_AddHours();

            AssertSql(
                @"SELECT DATEADD(hour, 1E0, [m].[Timeline])
FROM [Missions] AS [m]");
        }

        public override void DateTimeOffset_DateAdd_AddMinutes()
        {
            base.DateTimeOffset_DateAdd_AddMinutes();

            AssertSql(
                @"SELECT DATEADD(minute, 1E0, [m].[Timeline])
FROM [Missions] AS [m]");
        }

        public override void DateTimeOffset_DateAdd_AddSeconds()
        {
            base.DateTimeOffset_DateAdd_AddSeconds();

            AssertSql(
                @"SELECT DATEADD(second, 1E0, [m].[Timeline])
FROM [Missions] AS [m]");
        }

        public override void DateTimeOffset_DateAdd_AddMilliseconds()
        {
            base.DateTimeOffset_DateAdd_AddMilliseconds();

            AssertSql(
                @"SELECT DATEADD(millisecond, 300E0, [m].[Timeline])
FROM [Missions] AS [m]");
        }

        public override void Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used()
        {
            base.Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used();

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

        public override void Complex_predicate_with_AndAlso_and_nullable_bool_property()
        {
            base.Complex_predicate_with_AndAlso_and_nullable_bool_property();

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

        public override void Distinct_with_optional_navigation_is_translated_to_sql()
        {
            base.Distinct_with_optional_navigation_is_translated_to_sql();

            AssertSql(
                @"SELECT DISTINCT [g].[HasSoulPatch]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g.Tag].[Note] <> N'Foo') OR [g.Tag].[Note] IS NULL)");
        }

        public override void Sum_with_optional_navigation_is_translated_to_sql()
        {
            base.Sum_with_optional_navigation_is_translated_to_sql();

            AssertSql(
                @"SELECT SUM([g].[SquadId])
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g.Tag].[Note] <> N'Foo') OR [g.Tag].[Note] IS NULL)");
        }

        public override void Count_with_optional_navigation_is_translated_to_sql()
        {
            base.Count_with_optional_navigation_is_translated_to_sql();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g.Tag].[Note] <> N'Foo') OR [g.Tag].[Note] IS NULL)");
        }

        public override void Count_with_unflattened_groupjoin_is_evaluated_on_client()
        {
            base.Count_with_unflattened_groupjoin_is_evaluated_on_client();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname], [g].[SquadId]");
        }

        public override void Distinct_with_unflattened_groupjoin_is_evaluated_on_client()
        {
            base.Distinct_with_unflattened_groupjoin_is_evaluated_on_client();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname], [g].[SquadId]");
        }

        public override void FirstOrDefault_with_manually_created_groupjoin_is_translated_to_sql()
        {
            base.FirstOrDefault_with_manually_created_groupjoin_is_translated_to_sql();

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

        public override void Any_with_optional_navigation_as_subquery_predicate_is_translated_to_sql()
        {
            base.Any_with_optional_navigation_as_subquery_predicate_is_translated_to_sql();

            AssertSql(
                @"SELECT [s].[Name]
FROM [Squads] AS [s]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Gears] AS [m]
    LEFT JOIN [Tags] AS [m.Tag] ON ([m].[Nickname] = [m.Tag].[GearNickName]) AND ([m].[SquadId] = [m.Tag].[GearSquadId])
    WHERE ([m].[Discriminator] IN (N'Officer', N'Gear') AND ([m.Tag].[Note] = N'Dom''s Tag')) AND ([s].[Id] = [m].[SquadId]))");
        }

        public override void All_with_optional_navigation_is_translated_to_sql()
        {
            base.All_with_optional_navigation_is_translated_to_sql();

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

        public override void Non_flattened_GroupJoin_with_result_operator_evaluates_on_the_client()
        {
            base.Non_flattened_GroupJoin_with_result_operator_evaluates_on_the_client();

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

        public override void Client_side_equality_with_parameter_works_with_optional_navigations()
        {
            base.Client_side_equality_with_parameter_works_with_optional_navigations();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Tag].[Note]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')");
        }

        public override void Contains_with_local_nullable_guid_list_closure()
        {
            base.Contains_with_local_nullable_guid_list_closure();

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

        public override void Where_and_order_by_are_properly_lifted_from_subquery_created_by_tracking()
        {
            base.Where_and_order_by_are_properly_lifted_from_subquery_created_by_tracking();

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

        public override void Subquery_containing_SelectMany_projecting_main_from_clause_gets_lifted()
        {
            base.Subquery_containing_SelectMany_projecting_main_from_clause_gets_lifted();

            AssertSql(
                @"SELECT [gear].[FullName]
FROM [Gears] AS [gear]
CROSS JOIN [Tags] AS [tag]
WHERE [gear].[Discriminator] IN (N'Officer', N'Gear') AND ([gear].[HasSoulPatch] = 1)
ORDER BY [gear].[FullName], [tag].[Note]");
        }

        public override void Subquery_containing_join_projecting_main_from_clause_gets_lifted()
        {
            base.Subquery_containing_join_projecting_main_from_clause_gets_lifted();

            AssertSql(
                @"SELECT [gear].[Nickname]
FROM [Gears] AS [gear]
INNER JOIN [Tags] AS [tag] ON [gear].[Nickname] = [tag].[GearNickName]
WHERE [gear].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [gear].[Nickname], [tag].[Note]");
        }

        public override void Subquery_containing_left_join_projecting_main_from_clause_gets_lifted()
        {
            base.Subquery_containing_left_join_projecting_main_from_clause_gets_lifted();

            AssertSql(
                @"SELECT [gear].[Nickname]
FROM [Gears] AS [gear]
LEFT JOIN [Tags] AS [tag] ON [gear].[Nickname] = [tag].[GearNickName]
WHERE [gear].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [gear].[Nickname], [gear].[Rank]");
        }

        public override void Subquery_containing_join_gets_lifted_clashing_names()
        {
            base.Subquery_containing_join_gets_lifted_clashing_names();

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

        public override void Subquery_is_not_lifted_from_additional_from_clause()
        {
            base.Subquery_is_not_lifted_from_additional_from_clause();

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

        public override void Subquery_with_result_operator_is_not_lifted()
        {
            base.Subquery_with_result_operator_is_not_lifted();

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

        public override void Select_length_of_string_property()
        {
            base.Select_length_of_string_property();

            AssertSql(
                @"SELECT [w].[Name], CAST(LEN([w].[Name]) AS int) AS [Length]
FROM [Weapons] AS [w]");
        }

        public override void Client_method_on_collection_navigation_in_predicate()
        {
            base.Client_method_on_collection_navigation_in_predicate();

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

        public override void Client_method_on_collection_navigation_in_predicate_accessed_by_ef_property()
        {
            base.Client_method_on_collection_navigation_in_predicate_accessed_by_ef_property();

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

        public override void Client_method_on_collection_navigation_in_order_by()
        {
            base.Client_method_on_collection_navigation_in_order_by();

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

        public override void Client_method_on_collection_navigation_in_additional_from_clause()
        {
            base.Client_method_on_collection_navigation_in_additional_from_clause();

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

        public override void Client_method_on_collection_navigation_in_outer_join_key()
        {
            base.Client_method_on_collection_navigation_in_outer_join_key();

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
) AS [t] ON CASE
    WHEN [f].[Discriminator] = N'LocustHorde'
    THEN [f].[CommanderName] ELSE NULL
END = [t].[Name]
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
) AS [t] ON CASE
    WHEN [f].[Discriminator] = N'LocustHorde'
    THEN [f].[CommanderName] ELSE NULL
END = [t].[Name]
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
) AS [t] ON CASE
    WHEN [f].[Discriminator] = N'LocustHorde'
    THEN [f].[CommanderName] ELSE NULL
END = [t].[Name]
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
                @"SELECT [lh].[Id], [lh].[CapitalName], [lh].[Discriminator], [lh].[Name], [lh].[CommanderName], [lh].[Eradicated], [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[ThreatLevel], [t].[DefeatedByNickname], [t].[DefeatedBySquadId]
FROM [Factions] AS [lh]
LEFT JOIN (
    SELECT [lh.Commander].*
    FROM [LocustLeaders] AS [lh.Commander]
    WHERE [lh.Commander].[Discriminator] = N'LocustCommander'
) AS [t] ON [lh].[CommanderName] = [t].[Name]
WHERE [lh].[Discriminator] = N'LocustHorde'
ORDER BY [lh].[Name], [lh].[Id]",
                //
                @"SELECT [lh.Leaders].[Name], [lh.Leaders].[Discriminator], [lh.Leaders].[LocustHordeId], [lh.Leaders].[ThreatLevel], [lh.Leaders].[DefeatedByNickname], [lh.Leaders].[DefeatedBySquadId]
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
                @"SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated], [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[ThreatLevel], [t].[DefeatedByNickname], [t].[DefeatedBySquadId]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [f.Commander].*
    FROM [LocustLeaders] AS [f.Commander]
    WHERE [f.Commander].[Discriminator] = N'LocustCommander'
) AS [t] ON CASE
    WHEN [f].[Discriminator] = N'LocustHorde'
    THEN [f].[CommanderName] ELSE NULL
END = [t].[Name]
WHERE ([f].[Discriminator] = N'LocustHorde') AND ([f].[Discriminator] = N'LocustHorde')
ORDER BY [f].[Name], [f].[Id]",
                //
                @"SELECT [f.Leaders].[Name], [f.Leaders].[Discriminator], [f.Leaders].[LocustHordeId], [f.Leaders].[ThreatLevel], [f.Leaders].[DefeatedByNickname], [f.Leaders].[DefeatedBySquadId]
FROM [LocustLeaders] AS [f.Leaders]
INNER JOIN (
    SELECT DISTINCT [f0].[Id], [f0].[Name]
    FROM [Factions] AS [f0]
    LEFT JOIN (
        SELECT [f.Commander0].*
        FROM [LocustLeaders] AS [f.Commander0]
        WHERE [f.Commander0].[Discriminator] = N'LocustCommander'
    ) AS [t0] ON CASE
        WHEN [f0].[Discriminator] = N'LocustHorde'
        THEN [f0].[CommanderName] ELSE NULL
    END = [t0].[Name]
    WHERE ([f0].[Discriminator] = N'LocustHorde') AND ([f0].[Discriminator] = N'LocustHorde')
) AS [t1] ON [f.Leaders].[LocustHordeId] = [t1].[Id]
WHERE [f.Leaders].[Discriminator] IN (N'LocustCommander', N'LocustLeader')
ORDER BY [t1].[Name], [t1].[Id]");
        }

        public override void Include_on_derived_entity_using_subquery_with_cast_AsNoTracking()
        {
            base.Include_on_derived_entity_using_subquery_with_cast_AsNoTracking();

            AssertSql(
                @"SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated], [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[ThreatLevel], [t].[DefeatedByNickname], [t].[DefeatedBySquadId]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [f.Commander].*
    FROM [LocustLeaders] AS [f.Commander]
    WHERE [f.Commander].[Discriminator] = N'LocustCommander'
) AS [t] ON CASE
    WHEN [f].[Discriminator] = N'LocustHorde'
    THEN [f].[CommanderName] ELSE NULL
END = [t].[Name]
WHERE ([f].[Discriminator] = N'LocustHorde') AND ([f].[Discriminator] = N'LocustHorde')
ORDER BY [f].[Name], [f].[Id]",
                //
                @"SELECT [f.Leaders].[Name], [f.Leaders].[Discriminator], [f.Leaders].[LocustHordeId], [f.Leaders].[ThreatLevel], [f.Leaders].[DefeatedByNickname], [f.Leaders].[DefeatedBySquadId]
FROM [LocustLeaders] AS [f.Leaders]
INNER JOIN (
    SELECT DISTINCT [f0].[Id], [f0].[Name]
    FROM [Factions] AS [f0]
    LEFT JOIN (
        SELECT [f.Commander0].*
        FROM [LocustLeaders] AS [f.Commander0]
        WHERE [f.Commander0].[Discriminator] = N'LocustCommander'
    ) AS [t0] ON CASE
        WHEN [f0].[Discriminator] = N'LocustHorde'
        THEN [f0].[CommanderName] ELSE NULL
    END = [t0].[Name]
    WHERE ([f0].[Discriminator] = N'LocustHorde') AND ([f0].[Discriminator] = N'LocustHorde')
) AS [t1] ON [f.Leaders].[LocustHordeId] = [t1].[Id]
WHERE [f.Leaders].[Discriminator] IN (N'LocustCommander', N'LocustLeader')
ORDER BY [t1].[Name], [t1].[Id]");
        }

        public override void Include_on_derived_entity_using_subquery_with_cast_cross_product_base_entity()
        {
            base.Include_on_derived_entity_using_subquery_with_cast_cross_product_base_entity();

            AssertSql(
                @"SELECT [f2].[Id], [f2].[CapitalName], [f2].[Discriminator], [f2].[Name], [f2].[CommanderName], [f2].[Eradicated], [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[ThreatLevel], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [ff].[Id], [ff].[CapitalName], [ff].[Discriminator], [ff].[Name], [ff].[CommanderName], [ff].[Eradicated], [ff.Capital].[Name], [ff.Capital].[Location]
FROM [Factions] AS [f2]
LEFT JOIN (
    SELECT [f2.Commander].*
    FROM [LocustLeaders] AS [f2.Commander]
    WHERE [f2.Commander].[Discriminator] = N'LocustCommander'
) AS [t] ON CASE
    WHEN [f2].[Discriminator] = N'LocustHorde'
    THEN [f2].[CommanderName] ELSE NULL
END = [t].[Name]
CROSS JOIN [Factions] AS [ff]
LEFT JOIN [Cities] AS [ff.Capital] ON [ff].[CapitalName] = [ff.Capital].[Name]
WHERE ([f2].[Discriminator] = N'LocustHorde') AND ([f2].[Discriminator] = N'LocustHorde')
ORDER BY [f2].[Name], [ff].[Name], [f2].[Id]",
                //
                @"SELECT [f2.Leaders].[Name], [f2.Leaders].[Discriminator], [f2.Leaders].[LocustHordeId], [f2.Leaders].[ThreatLevel], [f2.Leaders].[DefeatedByNickname], [f2.Leaders].[DefeatedBySquadId]
FROM [LocustLeaders] AS [f2.Leaders]
INNER JOIN (
    SELECT DISTINCT [f20].[Id], [f20].[Name], [ff0].[Name] AS [Name0]
    FROM [Factions] AS [f20]
    LEFT JOIN (
        SELECT [f2.Commander0].*
        FROM [LocustLeaders] AS [f2.Commander0]
        WHERE [f2.Commander0].[Discriminator] = N'LocustCommander'
    ) AS [t0] ON CASE
        WHEN [f20].[Discriminator] = N'LocustHorde'
        THEN [f20].[CommanderName] ELSE NULL
    END = [t0].[Name]
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
) AS [t] ON CASE
    WHEN [f].[Discriminator] = N'LocustHorde'
    THEN [f].[CommanderName] ELSE NULL
END = [t].[Name]
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
                @"SELECT [h].[Id] AS [Id0], [t0].[Id]
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
                @"SELECT [h.Commander.CommandingFaction.Leaders].[Name], [h.Commander.CommandingFaction.Leaders].[Discriminator], [h.Commander.CommandingFaction.Leaders].[LocustHordeId], [h.Commander.CommandingFaction.Leaders].[ThreatLevel], [h.Commander.CommandingFaction.Leaders].[DefeatedByNickname], [h.Commander.CommandingFaction.Leaders].[DefeatedBySquadId], [t3].[Id], [t3].[Id0]
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
) AS [t] ON CASE
    WHEN [f].[Discriminator] = N'LocustHorde'
    THEN [f].[CommanderName] ELSE NULL
END = [t].[Name]
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
    ) AS [t1] ON CASE
        WHEN [f0].[Discriminator] = N'LocustHorde'
        THEN [f0].[CommanderName] ELSE NULL
    END = [t1].[Name]
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

        public override void Include_reference_on_derived_type_using_string()
        {
            base.Include_reference_on_derived_type_using_string();

            AssertSql(
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [LocustLeaders] AS [l]
LEFT JOIN (
    SELECT [l.DefeatedBy].*
    FROM [Gears] AS [l.DefeatedBy]
    WHERE [l.DefeatedBy].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON (CASE
    WHEN [l].[Discriminator] = N'LocustCommander'
    THEN [l].[DefeatedByNickname] ELSE NULL
END = [t].[Nickname]) AND (CASE
    WHEN [l].[Discriminator] = N'LocustCommander'
    THEN [l].[DefeatedBySquadId] ELSE NULL
END = [t].[SquadId])
WHERE [l].[Discriminator] IN (N'LocustCommander', N'LocustLeader')");
        }

        public override void Include_reference_on_derived_type_using_string_nested1()
        {
            base.Include_reference_on_derived_type_using_string_nested1();

            AssertSql(
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [l.DefeatedBy.Squad].[Id], [l.DefeatedBy.Squad].[InternalNumber], [l.DefeatedBy.Squad].[Name]
FROM [LocustLeaders] AS [l]
LEFT JOIN (
    SELECT [l.DefeatedBy].*
    FROM [Gears] AS [l.DefeatedBy]
    WHERE [l.DefeatedBy].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON (CASE
    WHEN [l].[Discriminator] = N'LocustCommander'
    THEN [l].[DefeatedByNickname] ELSE NULL
END = [t].[Nickname]) AND (CASE
    WHEN [l].[Discriminator] = N'LocustCommander'
    THEN [l].[DefeatedBySquadId] ELSE NULL
END = [t].[SquadId])
LEFT JOIN [Squads] AS [l.DefeatedBy.Squad] ON [t].[SquadId] = [l.DefeatedBy.Squad].[Id]
WHERE [l].[Discriminator] IN (N'LocustCommander', N'LocustLeader')");
        }

        public override void Include_reference_on_derived_type_using_string_nested2()
        {
            base.Include_reference_on_derived_type_using_string_nested2();

            AssertSql(
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [LocustLeaders] AS [l]
LEFT JOIN (
    SELECT [l.DefeatedBy].*
    FROM [Gears] AS [l.DefeatedBy]
    WHERE [l.DefeatedBy].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON (CASE
    WHEN [l].[Discriminator] = N'LocustCommander'
    THEN [l].[DefeatedByNickname] ELSE NULL
END = [t].[Nickname]) AND (CASE
    WHEN [l].[Discriminator] = N'LocustCommander'
    THEN [l].[DefeatedBySquadId] ELSE NULL
END = [t].[SquadId])
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
    ) AS [t0] ON (CASE
        WHEN [l0].[Discriminator] = N'LocustCommander'
        THEN [l0].[DefeatedByNickname] ELSE NULL
    END = [t0].[Nickname]) AND (CASE
        WHEN [l0].[Discriminator] = N'LocustCommander'
        THEN [l0].[DefeatedBySquadId] ELSE NULL
    END = [t0].[SquadId])
    WHERE [l0].[Discriminator] IN (N'LocustCommander', N'LocustLeader')
) AS [t1] ON ([l.DefeatedBy.Reports].[LeaderNickname] = [t1].[Nickname]) AND ([l.DefeatedBy.Reports].[LeaderSquadId] = [t1].[SquadId])
WHERE [l.DefeatedBy.Reports].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t1].[Nickname], [t1].[SquadId]");
        }

        public override void Include_reference_on_derived_type_using_lambda()
        {
            base.Include_reference_on_derived_type_using_lambda();

            AssertSql(
                @"SELECT [ll].[Name], [ll].[Discriminator], [ll].[LocustHordeId], [ll].[ThreatLevel], [ll].[DefeatedByNickname], [ll].[DefeatedBySquadId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [LocustLeaders] AS [ll]
LEFT JOIN (
    SELECT [ll.DefeatedBy].*
    FROM [Gears] AS [ll.DefeatedBy]
    WHERE [ll.DefeatedBy].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON (CASE
    WHEN [ll].[Discriminator] = N'LocustCommander'
    THEN [ll].[DefeatedByNickname] ELSE NULL
END = [t].[Nickname]) AND (CASE
    WHEN [ll].[Discriminator] = N'LocustCommander'
    THEN [ll].[DefeatedBySquadId] ELSE NULL
END = [t].[SquadId])
WHERE [ll].[Discriminator] IN (N'LocustCommander', N'LocustLeader')");
        }

        public override void Include_reference_on_derived_type_using_lambda_with_soft_cast()
        {
            base.Include_reference_on_derived_type_using_lambda_with_soft_cast();

            AssertSql(
                @"SELECT [ll].[Name], [ll].[Discriminator], [ll].[LocustHordeId], [ll].[ThreatLevel], [ll].[DefeatedByNickname], [ll].[DefeatedBySquadId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [LocustLeaders] AS [ll]
LEFT JOIN (
    SELECT [ll.DefeatedBy].*
    FROM [Gears] AS [ll.DefeatedBy]
    WHERE [ll.DefeatedBy].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON (CASE
    WHEN [ll].[Discriminator] = N'LocustCommander'
    THEN [ll].[DefeatedByNickname] ELSE NULL
END = [t].[Nickname]) AND (CASE
    WHEN [ll].[Discriminator] = N'LocustCommander'
    THEN [ll].[DefeatedBySquadId] ELSE NULL
END = [t].[SquadId])
WHERE [ll].[Discriminator] IN (N'LocustCommander', N'LocustLeader')");
        }

        public override void Include_reference_on_derived_type_using_lambda_with_tracking()
        {
            base.Include_reference_on_derived_type_using_lambda_with_tracking();

            AssertSql(
                @"SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[ThreatLevel], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [LocustLeaders] AS [l]
LEFT JOIN (
    SELECT [l.DefeatedBy].*
    FROM [Gears] AS [l.DefeatedBy]
    WHERE [l.DefeatedBy].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON (CASE
    WHEN [l].[Discriminator] = N'LocustCommander'
    THEN [l].[DefeatedByNickname] ELSE NULL
END = [t].[Nickname]) AND (CASE
    WHEN [l].[Discriminator] = N'LocustCommander'
    THEN [l].[DefeatedBySquadId] ELSE NULL
END = [t].[SquadId])
WHERE [l].[Discriminator] IN (N'LocustCommander', N'LocustLeader')");
        }

        public override void Include_collection_on_derived_type_using_string()
        {
            base.Include_collection_on_derived_type_using_string();

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

        public override void Include_collection_on_derived_type_using_lambda()
        {
            base.Include_collection_on_derived_type_using_lambda();

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

        public override void Include_collection_on_derived_type_using_lambda_with_soft_cast()
        {
            base.Include_collection_on_derived_type_using_lambda_with_soft_cast();

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

        public override void Include_base_navigation_on_derived_entity()
        {
            base.Include_base_navigation_on_derived_entity();

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

        public override void ThenInclude_collection_on_derived_after_base_reference()
        {
            base.ThenInclude_collection_on_derived_after_base_reference();

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

        public override void ThenInclude_collection_on_derived_after_derived_reference()
        {
            base.ThenInclude_collection_on_derived_after_derived_reference();

            AssertSql(
                @"SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated], [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[ThreatLevel], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOrBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [f.Commander].*
    FROM [LocustLeaders] AS [f.Commander]
    WHERE [f.Commander].[Discriminator] = N'LocustCommander'
) AS [t] ON CASE
    WHEN [f].[Discriminator] = N'LocustHorde'
    THEN [f].[CommanderName] ELSE NULL
END = [t].[Name]
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
    ) AS [t1] ON CASE
        WHEN [f0].[Discriminator] = N'LocustHorde'
        THEN [f0].[CommanderName] ELSE NULL
    END = [t1].[Name]
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

        public override void ThenInclude_collection_on_derived_after_derived_collection()
        {
            base.ThenInclude_collection_on_derived_after_derived_collection();

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

        public override void ThenInclude_reference_on_derived_after_derived_collection()
        {
            base.ThenInclude_reference_on_derived_after_derived_collection();

            AssertSql(
                @"SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated]
FROM [Factions] AS [f]
WHERE [f].[Discriminator] = N'LocustHorde'
ORDER BY [f].[Id]",
                //
                @"SELECT [f.Leaders].[Name], [f.Leaders].[Discriminator], [f.Leaders].[LocustHordeId], [f.Leaders].[ThreatLevel], [f.Leaders].[DefeatedByNickname], [f.Leaders].[DefeatedBySquadId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [LocustLeaders] AS [f.Leaders]
LEFT JOIN (
    SELECT [l.DefeatedBy].*
    FROM [Gears] AS [l.DefeatedBy]
    WHERE [l.DefeatedBy].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON (CASE
    WHEN [f.Leaders].[Discriminator] = N'LocustCommander'
    THEN [f.Leaders].[DefeatedByNickname] ELSE NULL
END = [t].[Nickname]) AND (CASE
    WHEN [f.Leaders].[Discriminator] = N'LocustCommander'
    THEN [f.Leaders].[DefeatedBySquadId] ELSE NULL
END = [t].[SquadId])
INNER JOIN (
    SELECT [f0].[Id]
    FROM [Factions] AS [f0]
    WHERE [f0].[Discriminator] = N'LocustHorde'
) AS [t0] ON [f.Leaders].[LocustHordeId] = [t0].[Id]
WHERE [f.Leaders].[Discriminator] IN (N'LocustCommander', N'LocustLeader')
ORDER BY [t0].[Id]");
        }

        public override void Multiple_derived_included_on_one_method()
        {
            base.Multiple_derived_included_on_one_method();

            AssertSql(
                @"SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[CommanderName], [f].[Eradicated], [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[ThreatLevel], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOrBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank]
FROM [Factions] AS [f]
LEFT JOIN (
    SELECT [f.Commander].*
    FROM [LocustLeaders] AS [f.Commander]
    WHERE [f.Commander].[Discriminator] = N'LocustCommander'
) AS [t] ON CASE
    WHEN [f].[Discriminator] = N'LocustHorde'
    THEN [f].[CommanderName] ELSE NULL
END = [t].[Name]
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
    ) AS [t1] ON CASE
        WHEN [f0].[Discriminator] = N'LocustHorde'
        THEN [f0].[CommanderName] ELSE NULL
    END = [t1].[Name]
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

        public override void Include_on_derived_multi_level()
        {
            base.Include_on_derived_multi_level();

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

        public override void Projecting_nullable_bool_in_conditional_works()
        {
            base.Projecting_nullable_bool_in_conditional_works();

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

        public override void Enum_ToString_is_client_eval()
        {
            base.Enum_ToString_is_client_eval();

            AssertSql(
                @"SELECT [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[SquadId], [g].[Nickname]");
        }

        public override void Correlated_collections_basic_projection()
        {
            base.Correlated_collections_basic_projection();

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

        public override void Correlated_collections_basic_projection_explicit_to_list()
        {
            base.Correlated_collections_basic_projection_explicit_to_list();

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

        public override void Correlated_collections_basic_projection_explicit_to_array()
        {
            base.Correlated_collections_basic_projection_explicit_to_array();

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

        public override void Correlated_collections_basic_projection_ordered()
        {
            base.Correlated_collections_basic_projection_ordered();

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

        public override void Correlated_collections_basic_projection_composite_key()
        {
            base.Correlated_collections_basic_projection_composite_key();

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

        public override void Correlated_collections_basic_projecting_single_property()
        {
            base.Correlated_collections_basic_projecting_single_property();

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

        public override void Correlated_collections_basic_projecting_constant()
        {
            base.Correlated_collections_basic_projecting_constant();

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

        public override void Correlated_collections_projection_of_collection_thru_navigation()
        {
            base.Correlated_collections_projection_of_collection_thru_navigation();

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

        public override void Correlated_collections_project_anonymous_collection_result()
        {
            base.Correlated_collections_project_anonymous_collection_result();

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

        public override void Correlated_collections_nested()
        {
            base.Correlated_collections_nested();

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

        public override void Correlated_collections_nested_mixed_streaming_with_buffer1()
        {
            base.Correlated_collections_nested_mixed_streaming_with_buffer1();

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

        public override void Correlated_collections_nested_mixed_streaming_with_buffer2()
        {
            base.Correlated_collections_nested_mixed_streaming_with_buffer2();

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

        public override void Correlated_collections_nested_with_custom_ordering()
        {
            base.Correlated_collections_nested_with_custom_ordering();

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

        public override void Correlated_collections_same_collection_projected_multiple_times()
        {
            base.Correlated_collections_same_collection_projected_multiple_times();

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

        public override void Correlated_collections_similar_collection_projected_multiple_times()
        {
            base.Correlated_collections_similar_collection_projected_multiple_times();

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

        public override void Correlated_collections_different_collections_projected()
        {
            base.Correlated_collections_different_collections_projected();

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

        public override void Correlated_collections_multiple_nested_complex_collections()
        {
            base.Correlated_collections_multiple_nested_complex_collections();

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

        public override void Correlated_collections_inner_subquery_selector_references_outer_qsre()
        {
            base.Correlated_collections_inner_subquery_selector_references_outer_qsre();

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

        public override void Correlated_collections_inner_subquery_predicate_references_outer_qsre()
        {
            base.Correlated_collections_inner_subquery_predicate_references_outer_qsre();

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

        public override void Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up()
        {
            base.Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up();

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

        public override void Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up()
        {
            base.Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up();

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

        public override void Correlated_collections_on_select_many()
        {
            base.Correlated_collections_on_select_many();

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

        public override void Correlated_collections_with_Skip()
        {
            base.Correlated_collections_with_Skip();

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

        public override void Correlated_collections_with_Take()
        {
            base.Correlated_collections_with_Take();

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

        public override void Correlated_collections_with_Distinct()
        {
            base.Correlated_collections_with_Distinct();

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

        public override void Correlated_collections_with_FirstOrDefault()
        {
            base.Correlated_collections_with_FirstOrDefault();

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

        public override void Correlated_collections_on_left_join_with_predicate()
        {
            base.Correlated_collections_on_left_join_with_predicate();

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

        public override void Correlated_collections_on_left_join_with_null_value()
        {
            base.Correlated_collections_on_left_join_with_null_value();

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

        public override void Correlated_collections_left_join_with_self_reference()
        {
            base.Correlated_collections_left_join_with_self_reference();

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

        public override void Correlated_collections_deeply_nested_left_join()
        {
            base.Correlated_collections_deeply_nested_left_join();

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

        public override void Correlated_collections_from_left_join_with_additional_elements_projected_of_that_join()
        {
            base.Correlated_collections_from_left_join_with_additional_elements_projected_of_that_join();

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

        public override void Correlated_collections_complex_scenario1()
        {
            base.Correlated_collections_complex_scenario1();

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

        public override void Correlated_collections_complex_scenario2()
        {
            base.Correlated_collections_complex_scenario2();

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

        private void AssertContainsSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, assertOrder: false);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
