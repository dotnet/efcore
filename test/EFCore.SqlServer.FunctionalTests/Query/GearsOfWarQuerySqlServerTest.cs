// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class GearsOfWarQuerySqlServerTest : GearsOfWarQueryTestBase<SqlServerTestStore, GearsOfWarQuerySqlServerFixture>
    {
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
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Tag].[Id], [g.Tag].[GearNickName], [g.Tag].[GearSquadId], [g.Tag].[Note]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g.Tag].[Id] IS NOT NULL AND [g.Tag].[Id] IN (",
                Fixture.TestSqlLoggerFactory.SqlStatements[1]);
        }

        public override void Include_where_list_contains_navigation2()
        {
            base.Include_where_list_contains_navigation2();

            AssertSql(
                @"SELECT [t].[Id]
FROM [Tags] AS [t]");

            Assert.Contains(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Tag].[Id], [g.Tag].[GearNickName], [g.Tag].[GearSquadId], [g.Tag].[Note]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
INNER JOIN [Cities] AS [g.CityOfBirth] ON [g].[CityOrBirthName] = [g.CityOfBirth].[Name]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g.CityOfBirth].[Location] IS NOT NULL AND [g.Tag].[Id] IN (",
                Fixture.TestSqlLoggerFactory.SqlStatements[1]);
        }

        public override void Navigation_accessed_twice_outside_and_inside_subquery()
        {
            base.Navigation_accessed_twice_outside_and_inside_subquery();

            AssertSql(
                @"SELECT [t].[Id]
FROM [Tags] AS [t]");

            Assert.Contains(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g.Tag].[Id] IS NOT NULL AND [g.Tag].[Id] IN (",
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
ORDER BY [w.Owner.CityOfBirth].[Name]");
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
                @"@__ammunitionType_0='Cartridge'

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] = @__ammunitionType_0");
        }

        public override void Where_nullable_enum_with_nullable_parameter()
        {
            base.Where_nullable_enum_with_nullable_parameter();

            AssertSql(
                @"@__ammunitionType_0='Cartridge' (Nullable = true)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] = @__ammunitionType_0",
                //
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] IS NULL");
        }

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
        public override void Where_bitwise_and_nullable_enum_with_constant()
        {
            base.Where_bitwise_and_nullable_enum_with_constant();

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & 1) > 0");
        }

        [ConditionalFact]
        public override void Where_bitwise_and_nullable_enum_with_null_constant()
        {
            base.Where_bitwise_and_nullable_enum_with_null_constant();

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & NULL) > 0");
        }

        [ConditionalFact]
        public override void Where_bitwise_and_nullable_enum_with_non_nullable_parameter()
        {
            base.Where_bitwise_and_nullable_enum_with_non_nullable_parameter();

            AssertSql(
                @"@__ammunitionType_0='Cartridge'

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & @__ammunitionType_0) > 0");
        }

        [ConditionalFact]
        public override void Where_bitwise_and_nullable_enum_with_nullable_parameter()
        {
            base.Where_bitwise_and_nullable_enum_with_nullable_parameter();

            AssertSql(
                @"@__ammunitionType_0='Cartridge' (Nullable = true)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & @__ammunitionType_0) > 0",
                //
                @"@__ammunitionType_0='' (DbType = Int32)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & @__ammunitionType_0) > 0");
        }

        [ConditionalFact]
        public override void Where_bitwise_or_enum()
        {
            base.Where_bitwise_or_enum();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] | 1) > 0)");
        }

        [ConditionalFact]
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
                @"@__parameter_0='Corporal'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & @__parameter_0) = @__parameter_0)");
        }

        public override void Where_has_flag_with_nullable_parameter()
        {
            base.Where_has_flag_with_nullable_parameter();

            AssertSql(
                @"@__parameter_0='Corporal' (Nullable = true)

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
                @"@__ammunitionType_1='Cartridge' (Nullable = true)
@__ammunitionType_0='Cartridge' (Nullable = true)

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
                @"SELECT [ct1].[Id], [ct1].[GearNickName], [ct1].[GearSquadId], [ct1].[Note], [ct2].[Id], [ct2].[GearNickName], [ct2].[GearSquadId], [ct2].[Note]
FROM [Tags] AS [ct1]
LEFT JOIN (
    SELECT [ct1.Gear].*
    FROM [Gears] AS [ct1.Gear]
    WHERE [ct1.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([ct1].[GearNickName] = [t].[Nickname]) AND ([ct1].[GearSquadId] = [t].[SquadId])
CROSS JOIN [Tags] AS [ct2]
LEFT JOIN (
    SELECT [ct2.Gear].*
    FROM [Gears] AS [ct2.Gear]
    WHERE [ct2.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([ct2].[GearNickName] = [t0].[Nickname]) AND ([ct2].[GearSquadId] = [t0].[SquadId])
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
                @"SELECT [ct].[Id], [ct].[GearNickName], [ct].[GearSquadId], [ct].[Note]
FROM [Tags] AS [ct]
LEFT JOIN (
    SELECT [ct.Gear].*
    FROM [Gears] AS [ct.Gear]
    WHERE [ct.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([ct].[GearNickName] = [t].[Nickname]) AND ([ct].[GearSquadId] = [t].[SquadId])
WHERE [t].[Nickname] = N'Marcus'");
        }

        public override void Select_Where_Navigation_Client()
        {
            base.Select_Where_Navigation_Client();

            AssertSql(
                @"SELECT [o].[Id], [o].[GearNickName], [o].[GearSquadId], [o].[Note], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [Tags] AS [o]
LEFT JOIN (
    SELECT [o.Gear].*
    FROM [Gears] AS [o.Gear]
    WHERE [o.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([o].[GearNickName] = [t].[Nickname]) AND ([o].[GearSquadId] = [t].[SquadId])
WHERE [o].[GearNickName] IS NOT NULL OR [o].[GearSquadId] IS NOT NULL");
        }

        public override void Select_Where_Navigation_Equals_Navigation()
        {
            base.Select_Where_Navigation_Equals_Navigation();

            AssertSql(
                @"SELECT [ct1].[Id], [ct1].[GearNickName], [ct1].[GearSquadId], [ct1].[Note], [ct2].[Id], [ct2].[GearNickName], [ct2].[GearSquadId], [ct2].[Note]
FROM [Tags] AS [ct1]
LEFT JOIN (
    SELECT [ct1.Gear].*
    FROM [Gears] AS [ct1.Gear]
    WHERE [ct1.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([ct1].[GearNickName] = [t].[Nickname]) AND ([ct1].[GearSquadId] = [t].[SquadId])
CROSS JOIN [Tags] AS [ct2]
LEFT JOIN (
    SELECT [ct2.Gear].*
    FROM [Gears] AS [ct2.Gear]
    WHERE [ct2.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([ct2].[GearNickName] = [t0].[Nickname]) AND ([ct2].[GearSquadId] = [t0].[SquadId])
WHERE (([t].[Nickname] = [t0].[Nickname]) OR ([t].[Nickname] IS NULL AND [t0].[Nickname] IS NULL)) AND (([t].[SquadId] = [t0].[SquadId]) OR ([t].[SquadId] IS NULL AND [t0].[SquadId] IS NULL))");
        }

        public override void Select_Where_Navigation_Null()
        {
            base.Select_Where_Navigation_Null();

            AssertSql(
                @"SELECT [ct].[Id], [ct].[GearNickName], [ct].[GearSquadId], [ct].[Note]
FROM [Tags] AS [ct]
WHERE [ct].[GearNickName] IS NULL AND [ct].[GearSquadId] IS NULL");
        }

        public override void Select_Where_Navigation_Null_Reverse()
        {
            base.Select_Where_Navigation_Null_Reverse();

            AssertSql(
                @"SELECT [ct].[Id], [ct].[GearNickName], [ct].[GearSquadId], [ct].[Note]
FROM [Tags] AS [ct]
WHERE [ct].[GearNickName] IS NULL AND [ct].[GearSquadId] IS NULL");
        }

        public override void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected()
        {
            base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected();

            AssertSql(
                @"SELECT [ct1].[Id] AS [Id1], [ct2].[Id] AS [Id2]
FROM [Tags] AS [ct1]
LEFT JOIN (
    SELECT [ct1.Gear].*
    FROM [Gears] AS [ct1.Gear]
    WHERE [ct1.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([ct1].[GearNickName] = [t].[Nickname]) AND ([ct1].[GearSquadId] = [t].[SquadId])
CROSS JOIN [Tags] AS [ct2]
LEFT JOIN (
    SELECT [ct2.Gear].*
    FROM [Gears] AS [ct2.Gear]
    WHERE [ct2.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([ct2].[GearNickName] = [t0].[Nickname]) AND ([ct2].[GearSquadId] = [t0].[SquadId])
WHERE ([t].[Nickname] = [t0].[Nickname]) OR ([t].[Nickname] IS NULL AND [t0].[Nickname] IS NULL)");
        }

        public override void Optional_Navigation_Null_Coalesce_To_Clr_Type()
        {
            base.Optional_Navigation_Null_Coalesce_To_Clr_Type();

            AssertSql(
                @"SELECT TOP(1) CAST(COALESCE([w.SynergyWith].[IsAutomatic], 0) AS bit) AS [IsAutomatic]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w.SynergyWith] ON [w].[SynergyWithId] = [w.SynergyWith].[Id]");
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
) AS [t0]",
                //
                @"@_outer_FullName='Marcus Fenix' (Size = 450)

SELECT TOP(1) [t0].[IsAutomatic]
FROM (
    SELECT DISTINCT [w0].*
    FROM [Weapons] AS [w0]
    WHERE @_outer_FullName = [w0].[OwnerFullName]
) AS [t0]");
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
                @"@_outer_FullName1='Damon Baird' (Size = 450)

SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapons] AS [w0]
WHERE @_outer_FullName1 = [w0].[OwnerFullName]",
                //
                @"@_outer_FullName='Damon Baird' (Size = 450)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE @_outer_FullName = [w].[OwnerFullName]",
                //
                @"@_outer_FullName1='Marcus Fenix' (Size = 450)

SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapons] AS [w0]
WHERE @_outer_FullName1 = [w0].[OwnerFullName]",
                //
                @"@_outer_FullName='Marcus Fenix' (Size = 450)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE @_outer_FullName = [w].[OwnerFullName]");
        }

        public override void Where_subquery_concat_firstordefault_boolean()
        {
            base.Where_subquery_concat_firstordefault_boolean();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)",
                //
                @"@_outer_FullName1='Damon Baird' (Size = 450)

SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapons] AS [w0]
WHERE @_outer_FullName1 = [w0].[OwnerFullName]",
                //
                @"@_outer_FullName='Damon Baird' (Size = 450)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE @_outer_FullName = [w].[OwnerFullName]",
                //
                @"@_outer_FullName1='Marcus Fenix' (Size = 450)

SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapons] AS [w0]
WHERE @_outer_FullName1 = [w0].[OwnerFullName]",
                //
                @"@_outer_FullName='Marcus Fenix' (Size = 450)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE @_outer_FullName = [w].[OwnerFullName]");
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
                @"@_outer_Nickname2='Baird' (Size = 4000)
@_outer_SquadId2='1'

SELECT [g2].[Nickname], [g2].[SquadId], [g2].[AssignedCityName], [g2].[CityOrBirthName], [g2].[Discriminator], [g2].[FullName], [g2].[HasSoulPatch], [g2].[LeaderNickname], [g2].[LeaderSquadId], [g2].[Rank]
FROM [Gears] AS [g2]
WHERE [g2].[Discriminator] IN (N'Officer', N'Gear') AND ((@_outer_Nickname2 = [g2].[LeaderNickname]) AND (@_outer_SquadId2 = [g2].[LeaderSquadId]))",
                //
                @"@_outer_Nickname1='Baird' (Size = 4000)
@_outer_SquadId1='1'

SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOrBirthName], [g1].[Discriminator], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[Rank]
FROM [Gears] AS [g1]
WHERE [g1].[Discriminator] IN (N'Officer', N'Gear') AND ((@_outer_Nickname1 = [g1].[LeaderNickname]) AND (@_outer_SquadId1 = [g1].[LeaderSquadId]))",
                //
                @"@_outer_Nickname2='Marcus' (Size = 4000)
@_outer_SquadId2='1'

SELECT [g2].[Nickname], [g2].[SquadId], [g2].[AssignedCityName], [g2].[CityOrBirthName], [g2].[Discriminator], [g2].[FullName], [g2].[HasSoulPatch], [g2].[LeaderNickname], [g2].[LeaderSquadId], [g2].[Rank]
FROM [Gears] AS [g2]
WHERE [g2].[Discriminator] IN (N'Officer', N'Gear') AND ((@_outer_Nickname2 = [g2].[LeaderNickname]) AND (@_outer_SquadId2 = [g2].[LeaderSquadId]))",
                //
                @"@_outer_Nickname1='Marcus' (Size = 4000)
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
FROM [Tags] AS [ct]
INNER JOIN [Gears] AS [g] ON ([ct].[GearNickName] = [g].[Nickname]) AND ([ct].[GearSquadId] = [g].[SquadId])
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
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE CASE
    WHEN [t0].[HasSoulPatch] = 1
    THEN 1 ELSE [t0].[HasSoulPatch]
END <> 1");
        }

        public override void Optional_navigation_type_compensation_works_with_predicate_negated_complex2()
        {
            base.Optional_navigation_type_compensation_works_with_predicate_negated_complex2();

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gears] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE CASE
    WHEN ([t0].[HasSoulPatch] <> 1) AND [t0].[HasSoulPatch] IS NOT NULL
    THEN 0 ELSE [t0].[HasSoulPatch]
END <> 1");
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
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL");
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
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL");
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
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL");
        }

        public override void Select_correlated_filtered_collection()
        {
            base.Select_correlated_filtered_collection();

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND [g].[CityOrBirthName] IN (N'Ephyra', N'Hanover')",
                //
                @"@_outer_FullName='Augustus Cole' (Size = 450)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE (([w].[Name] <> N'Lancer') OR [w].[Name] IS NULL) AND (@_outer_FullName = [w].[OwnerFullName])",
                //
                @"@_outer_FullName='Dominic Santiago' (Size = 450)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE (([w].[Name] <> N'Lancer') OR [w].[Name] IS NULL) AND (@_outer_FullName = [w].[OwnerFullName])");
        }

        public override void Select_correlated_filtered_collection_with_composite_key()
        {
            base.Select_correlated_filtered_collection_with_composite_key();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] = N'Officer'",
                //
                @"@_outer_Nickname='Baird' (Size = 4000)
@_outer_SquadId='1'

SELECT [r].[Nickname], [r].[SquadId], [r].[AssignedCityName], [r].[CityOrBirthName], [r].[Discriminator], [r].[FullName], [r].[HasSoulPatch], [r].[LeaderNickname], [r].[LeaderSquadId], [r].[Rank]
FROM [Gears] AS [r]
WHERE ([r].[Discriminator] IN (N'Officer', N'Gear') AND ([r].[Nickname] <> N'Dom')) AND ((@_outer_Nickname = [r].[LeaderNickname]) AND (@_outer_SquadId = [r].[LeaderSquadId]))",
                //
                @"@_outer_Nickname='Marcus' (Size = 4000)
@_outer_SquadId='1'

SELECT [r].[Nickname], [r].[SquadId], [r].[AssignedCityName], [r].[CityOrBirthName], [r].[Discriminator], [r].[FullName], [r].[HasSoulPatch], [r].[LeaderNickname], [r].[LeaderSquadId], [r].[Rank]
FROM [Gears] AS [r]
WHERE ([r].[Discriminator] IN (N'Officer', N'Gear') AND ([r].[Nickname] <> N'Dom')) AND ((@_outer_Nickname = [r].[LeaderNickname]) AND (@_outer_SquadId = [r].[LeaderSquadId]))");
        }

        public override void Select_correlated_filtered_collection_works_with_caching()
        {
            base.Select_correlated_filtered_collection_works_with_caching();

            AssertContainsSql(
                @"SELECT [t].[GearNickName]
FROM [Tags] AS [t]",
                //
                @"@_outer_GearNickName='Paduk' (Size = 450)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] = @_outer_GearNickName)",
                //
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND [g].[Nickname] IS NULL",
                //
                @"@_outer_GearNickName='Baird' (Size = 450)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] = @_outer_GearNickName)",
                //
                @"@_outer_GearNickName='Dom' (Size = 450)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] = @_outer_GearNickName)",
                //
                @"@_outer_GearNickName='Cole Train' (Size = 450)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] = @_outer_GearNickName)",
                //
                @"@_outer_GearNickName='Marcus' (Size = 450)

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

        public override void DateTimeOffset_Date_works()
        {
            base.DateTimeOffset_Date_works();

            AssertSql(
                @"@__Date_0='01/01/0001 00:00:00'

SELECT [m].[Id], [m].[CodeName], [m].[Timeline]
FROM [Missions] AS [m]
WHERE CONVERT(date, [m].[Timeline]) > @__Date_0");
        }

        public override void DateTimeOffset_Datepart_works()
        {
            base.DateTimeOffset_Datepart_works();

            AssertSql(
                @"SELECT [m].[Id], [m].[CodeName], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(month, [m].[Timeline]) = 5");
        }

        public override void DateTimeOffset_DateAdd_AddYears()
        {
            base.DateTimeOffset_DateAdd_AddYears();

            AssertSql(
                @"SELECT DATEADD(year, 1, [m].[Timeline])
FROM [Missions] AS [m]
ORDER BY [m].[Timeline]");
        }

        public override void DateTimeOffset_DateAdd_AddMonths()
        {
            base.DateTimeOffset_DateAdd_AddMonths();

            AssertSql(
                @"SELECT DATEADD(month, 1, [m].[Timeline])
FROM [Missions] AS [m]
ORDER BY [m].[Timeline]");
        }

        public override void DateTimeOffset_DateAdd_AddDays()
        {
            base.DateTimeOffset_DateAdd_AddDays();

            AssertSql(
                @"SELECT DATEADD(day, 1E0, [m].[Timeline])
FROM [Missions] AS [m]
ORDER BY [m].[Timeline]");
        }

        public override void DateTimeOffset_DateAdd_AddHours()
        {
            base.DateTimeOffset_DateAdd_AddHours();

            AssertSql(
                @"SELECT DATEADD(hour, 1E0, [m].[Timeline])
FROM [Missions] AS [m]
ORDER BY [m].[Timeline]");
        }

        public override void DateTimeOffset_DateAdd_AddMinutes()
        {
            base.DateTimeOffset_DateAdd_AddMinutes();

            AssertSql(
                @"SELECT DATEADD(minute, 1E0, [m].[Timeline])
FROM [Missions] AS [m]
ORDER BY [m].[Timeline]");
        }

        public override void DateTimeOffset_DateAdd_AddSeconds()
        {
            base.DateTimeOffset_DateAdd_AddSeconds();

            AssertSql(
                @"SELECT DATEADD(second, 1E0, [m].[Timeline])
FROM [Missions] AS [m]
ORDER BY [m].[Timeline]");
        }

        public override void DateTimeOffset_DateAdd_AddMilliseconds()
        {
            base.DateTimeOffset_DateAdd_AddMilliseconds();

            AssertSql(
                @"SELECT DATEADD(millisecond, 300E0, [m].[Timeline])
FROM [Missions] AS [m]
ORDER BY [m].[Timeline]");
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
                @"@_outer_Nickname='Baird' (Size = 4000)
@_outer_SquadId='1'

SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOrBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gears] AS [g0]
WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND ((@_outer_Nickname = [g0].[LeaderNickname]) AND (@_outer_SquadId = [g0].[LeaderSquadId]))",
                //
                @"@_outer_Nickname='Marcus' (Size = 4000)
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
) AS [t] ON [f].[CommanderName] = [t].[Name]
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
) AS [t] ON [f].[CommanderName] = [t].[Name]
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
) AS [t] ON [f].[CommanderName] = [t].[Name]
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
) AS [t] ON [f].[CommanderName] = [t].[Name]
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
    ) AS [t0] ON [f0].[CommanderName] = [t0].[Name]
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
) AS [t] ON [f].[CommanderName] = [t].[Name]
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
    ) AS [t0] ON [f0].[CommanderName] = [t0].[Name]
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
) AS [t] ON [f2].[CommanderName] = [t].[Name]
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
    ) AS [t0] ON [f20].[CommanderName] = [t0].[Name]
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
) AS [t] ON [f].[CommanderName] = [t].[Name]
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
                @"SELECT [h].[Id] AS [Id0], [h].[CapitalName], [h].[Discriminator], [h].[Name], [h].[CommanderName], [h].[Eradicated], [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[ThreatLevel], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t0].[Id], [t0].[CapitalName], [t0].[Discriminator], [t0].[Name], [t0].[CommanderName], [t0].[Eradicated]
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
ORDER BY [t0].[Id]",
                //
                @"SELECT [h.Commander.CommandingFaction.Leaders].[Name], [h.Commander.CommandingFaction.Leaders].[Discriminator], [h.Commander.CommandingFaction.Leaders].[LocustHordeId], [h.Commander.CommandingFaction.Leaders].[ThreatLevel], [h.Commander.CommandingFaction.Leaders].[DefeatedByNickname], [h.Commander.CommandingFaction.Leaders].[DefeatedBySquadId]
FROM [LocustLeaders] AS [h.Commander.CommandingFaction.Leaders]
INNER JOIN (
    SELECT DISTINCT [t2].[Id]
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
ORDER BY [t3].[Id]");
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
WHERE [h].[Discriminator] = N'LocustHorde'",
                //
                @"@_outer_Nickname='Marcus' (Size = 4000)
@_outer_SquadId='1'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ((@_outer_Nickname = [g].[LeaderNickname]) AND (@_outer_SquadId = [g].[LeaderSquadId]))",
                //
                @"@_outer_Nickname='' (Size = 4000) (DbType = String)
@_outer_SquadId='' (Nullable = false) (DbType = Int32)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ((@_outer_Nickname = [g].[LeaderNickname]) AND (@_outer_SquadId = [g].[LeaderSquadId]))");
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
) AS [t] ON [f].[CommanderName] = [t].[Name]
LEFT JOIN (
    SELECT [f.Commander.DefeatedBy].*
    FROM [Gears] AS [f.Commander.DefeatedBy]
    WHERE [f.Commander.DefeatedBy].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[DefeatedByNickname] = [t0].[Nickname]) AND ([t].[DefeatedBySquadId] = [t0].[SquadId])
WHERE ([f].[Discriminator] = N'LocustHorde') AND ([f].[Discriminator] = N'LocustHorde')",
                //
                @"@_outer_Nickname='Marcus' (Size = 4000)
@_outer_SquadId='1'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ((@_outer_Nickname = [g].[LeaderNickname]) AND (@_outer_SquadId = [g].[LeaderSquadId]))",
                //
                @"@_outer_Nickname='' (Size = 4000) (DbType = String)
@_outer_SquadId='' (Nullable = false) (DbType = Int32)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gears] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ((@_outer_Nickname = [g].[LeaderNickname]) AND (@_outer_SquadId = [g].[LeaderSquadId]))");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        private void AssertContainsSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, assertOrder: false);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
