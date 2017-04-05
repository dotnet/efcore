// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class GearsOfWarQuerySqlServerTest : GearsOfWarQueryTestBase<SqlServerTestStore, GearsOfWarQuerySqlServerFixture>
    {
        private readonly ITestOutputHelper _testOutputHelper;

        // ReSharper disable once UnusedParameter.Local
        public GearsOfWarQuerySqlServerTest(GearsOfWarQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            _testOutputHelper = testOutputHelper;

            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        public override void Entity_equality_empty()
        {
            base.Entity_equality_empty();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] IS NULL AND ([g].[SquadId] = 0))",
                Sql);
        }

        public override void Include_multiple_one_to_one_and_one_to_many()
        {
            base.Include_multiple_one_to_one_and_one_to_many();

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
ORDER BY [g].[FullName]

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE EXISTS (
    SELECT 1
    FROM [CogTag] AS [t]
    LEFT JOIN (
        SELECT [g].*
        FROM [Gear] AS [g]
        WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
    WHERE [w].[OwnerFullName] = [g].[FullName])
ORDER BY [w].[OwnerFullName]",
                Sql);
        }

        public override void Include_multiple_one_to_one_and_one_to_many_self_reference()
        {
            base.Include_multiple_one_to_one_and_one_to_many_self_reference();

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
ORDER BY [g].[FullName]

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE EXISTS (
    SELECT 1
    FROM [CogTag] AS [t]
    LEFT JOIN (
        SELECT [g].*
        FROM [Gear] AS [g]
        WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
    WHERE [w].[OwnerFullName] = [g].[FullName])
ORDER BY [w].[OwnerFullName]",
                Sql);
        }

        public override void Include_multiple_one_to_one_and_one_to_one_and_one_to_many()
        {
            base.Include_multiple_one_to_one_and_one_to_one_and_one_to_many();

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [s].[Id], [s].[InternalNumber], [s].[Name]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
LEFT JOIN [Squad] AS [s] ON [g].[SquadId] = [s].[Id]
ORDER BY [s].[Id]

SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOrBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gear] AS [g0]
WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND EXISTS (
    SELECT 1
    FROM [CogTag] AS [t]
    LEFT JOIN (
        SELECT [g].*
        FROM [Gear] AS [g]
        WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
    LEFT JOIN [Squad] AS [s] ON [g].[SquadId] = [s].[Id]
    WHERE [g0].[SquadId] = [s].[Id])
ORDER BY [g0].[SquadId]",
                Sql);
        }

        public override void Include_multiple_one_to_one_optional_and_one_to_one_required()
        {
            base.Include_multiple_one_to_one_optional_and_one_to_one_required();

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOrBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t.Gear.Squad].[Id], [t.Gear.Squad].[InternalNumber], [t.Gear.Squad].[Name]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gear] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
LEFT JOIN [Squad] AS [t.Gear.Squad] ON [t0].[SquadId] = [t.Gear.Squad].[Id]",
                Sql);
        }

        public override void Include_multiple_circular()
        {
            base.Include_multiple_circular();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location]
FROM [Gear] AS [g]
INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [c].[Name]

SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOrBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gear] AS [g0]
WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND EXISTS (
    SELECT 1
    FROM [Gear] AS [g]
    INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g0].[AssignedCityName] = [c].[Name]))
ORDER BY [g0].[AssignedCityName]",
                Sql);
        }

        public override void Include_multiple_circular_with_filter()
        {
            base.Include_multiple_circular_with_filter();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location]
FROM [Gear] AS [g]
INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] = N'Marcus')
ORDER BY [c].[Name]

SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOrBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gear] AS [g0]
WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND EXISTS (
    SELECT 1
    FROM [Gear] AS [g]
    INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
    WHERE ([g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] = N'Marcus')) AND ([g0].[AssignedCityName] = [c].[Name]))
ORDER BY [g0].[AssignedCityName]",
                Sql);
        }

        public override void Include_using_alternate_key()
        {
            base.Include_using_alternate_key();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] = N'Marcus')
ORDER BY [g].[FullName]

SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId]
FROM [Weapon] AS [g.Weapons]
INNER JOIN (
    SELECT [g0].[FullName]
    FROM [Gear] AS [g0]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND ([g0].[Nickname] = N'Marcus')
) AS [t] ON [g.Weapons].[OwnerFullName] = [t].[FullName]
ORDER BY [t].[FullName]",
                Sql);
        }

        public override void Include_multiple_include_then_include()
        {
            base.Include_multiple_include_then_include();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location], [c2].[Name], [c2].[Location], [c4].[Name], [c4].[Location], [c6].[Name], [c6].[Location]
FROM [Gear] AS [g]
LEFT JOIN [City] AS [c] ON [g].[AssignedCityName] = [c].[Name]
LEFT JOIN [City] AS [c2] ON [g].[AssignedCityName] = [c2].[Name]
INNER JOIN [City] AS [c4] ON [g].[CityOrBirthName] = [c4].[Name]
INNER JOIN [City] AS [c6] ON [g].[CityOrBirthName] = [c6].[Name]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname], [c].[Name], [c2].[Name], [c4].[Name], [c6].[Name]

SELECT [g3].[Nickname], [g3].[SquadId], [g3].[AssignedCityName], [g3].[CityOrBirthName], [g3].[Discriminator], [g3].[FullName], [g3].[HasSoulPatch], [g3].[LeaderNickname], [g3].[LeaderSquadId], [g3].[Rank], [c7].[Id], [c7].[GearNickName], [c7].[GearSquadId], [c7].[Note]
FROM [Gear] AS [g3]
INNER JOIN (
    SELECT DISTINCT [g].[Nickname], [c].[Name], [c2].[Name] AS [Name0], [c4].[Name] AS [Name1], [c6].[Name] AS [Name2]
    FROM [Gear] AS [g]
    LEFT JOIN [City] AS [c] ON [g].[AssignedCityName] = [c].[Name]
    LEFT JOIN [City] AS [c2] ON [g].[AssignedCityName] = [c2].[Name]
    INNER JOIN [City] AS [c4] ON [g].[CityOrBirthName] = [c4].[Name]
    INNER JOIN [City] AS [c6] ON [g].[CityOrBirthName] = [c6].[Name]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [c60] ON [g3].[AssignedCityName] = [c60].[Name2]
LEFT JOIN [CogTag] AS [c7] ON ([c7].[GearNickName] = [g3].[Nickname]) AND ([c7].[GearSquadId] = [g3].[SquadId])
WHERE [g3].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [c60].[Nickname], [c60].[Name], [c60].[Name0], [c60].[Name1], [c60].[Name2]

SELECT [g2].[Nickname], [g2].[SquadId], [g2].[AssignedCityName], [g2].[CityOrBirthName], [g2].[Discriminator], [g2].[FullName], [g2].[HasSoulPatch], [g2].[LeaderNickname], [g2].[LeaderSquadId], [g2].[Rank], [c5].[Id], [c5].[GearNickName], [c5].[GearSquadId], [c5].[Note]
FROM [Gear] AS [g2]
INNER JOIN (
    SELECT DISTINCT [g].[Nickname], [c].[Name], [c2].[Name] AS [Name0], [c4].[Name] AS [Name1]
    FROM [Gear] AS [g]
    LEFT JOIN [City] AS [c] ON [g].[AssignedCityName] = [c].[Name]
    LEFT JOIN [City] AS [c2] ON [g].[AssignedCityName] = [c2].[Name]
    INNER JOIN [City] AS [c4] ON [g].[CityOrBirthName] = [c4].[Name]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [c40] ON [g2].[CityOrBirthName] = [c40].[Name1]
LEFT JOIN [CogTag] AS [c5] ON ([c5].[GearNickName] = [g2].[Nickname]) AND ([c5].[GearSquadId] = [g2].[SquadId])
WHERE [g2].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [c40].[Nickname], [c40].[Name], [c40].[Name0], [c40].[Name1]

SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOrBirthName], [g1].[Discriminator], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[Rank], [c3].[Id], [c3].[GearNickName], [c3].[GearSquadId], [c3].[Note]
FROM [Gear] AS [g1]
INNER JOIN (
    SELECT DISTINCT [g].[Nickname], [c].[Name], [c2].[Name] AS [Name0]
    FROM [Gear] AS [g]
    LEFT JOIN [City] AS [c] ON [g].[AssignedCityName] = [c].[Name]
    LEFT JOIN [City] AS [c2] ON [g].[AssignedCityName] = [c2].[Name]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [c20] ON [g1].[AssignedCityName] = [c20].[Name0]
LEFT JOIN [CogTag] AS [c3] ON ([c3].[GearNickName] = [g1].[Nickname]) AND ([c3].[GearSquadId] = [g1].[SquadId])
WHERE [g1].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [c20].[Nickname], [c20].[Name], [c20].[Name0]

SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOrBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], [c1].[Id], [c1].[GearNickName], [c1].[GearSquadId], [c1].[Note]
FROM [Gear] AS [g0]
INNER JOIN (
    SELECT DISTINCT [g].[Nickname], [c].[Name]
    FROM [Gear] AS [g]
    LEFT JOIN [City] AS [c] ON [g].[AssignedCityName] = [c].[Name]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [c0] ON [g0].[CityOrBirthName] = [c0].[Name]
LEFT JOIN [CogTag] AS [c1] ON ([c1].[GearNickName] = [g0].[Nickname]) AND ([c1].[GearSquadId] = [g0].[SquadId])
WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [c0].[Nickname], [c0].[Name]",
                Sql);
        }

        public override void Include_navigation_on_derived_type()
        {
            base.Include_navigation_on_derived_type();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId]

SELECT [o.Reports].[Nickname], [o.Reports].[SquadId], [o.Reports].[AssignedCityName], [o.Reports].[CityOrBirthName], [o.Reports].[Discriminator], [o.Reports].[FullName], [o.Reports].[HasSoulPatch], [o.Reports].[LeaderNickname], [o.Reports].[LeaderSquadId], [o.Reports].[Rank]
FROM [Gear] AS [o.Reports]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId]
    FROM [Gear] AS [g0]
    WHERE [g0].[Discriminator] = N'Officer'
) AS [t] ON ([o.Reports].[LeaderNickname] = [t].[Nickname]) AND ([o.Reports].[LeaderSquadId] = [t].[SquadId])
WHERE [o.Reports].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[Nickname], [t].[SquadId]",
                Sql);
        }

        public override void String_based_Include_navigation_on_derived_type()
        {
            base.String_based_Include_navigation_on_derived_type();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId]

SELECT [o.Reports].[Nickname], [o.Reports].[SquadId], [o.Reports].[AssignedCityName], [o.Reports].[CityOrBirthName], [o.Reports].[Discriminator], [o.Reports].[FullName], [o.Reports].[HasSoulPatch], [o.Reports].[LeaderNickname], [o.Reports].[LeaderSquadId], [o.Reports].[Rank]
FROM [Gear] AS [o.Reports]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId]
    FROM [Gear] AS [g0]
    WHERE [g0].[Discriminator] = N'Officer'
) AS [t] ON ([o.Reports].[LeaderNickname] = [t].[Nickname]) AND ([o.Reports].[LeaderSquadId] = [t].[SquadId])
WHERE [o.Reports].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[Nickname], [t].[SquadId]",
                Sql);
        }

        public override void Select_Where_Navigation_Included()
        {
            base.Select_Where_Navigation_Included();

            AssertSql(
                @"SELECT [o].[Id], [o].[GearNickName], [o].[GearSquadId], [o].[Note], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [CogTag] AS [o]
LEFT JOIN (
    SELECT [o.Gear].*
    FROM [Gear] AS [o.Gear]
    WHERE [o.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([o].[GearNickName] = [t].[Nickname]) AND ([o].[GearSquadId] = [t].[SquadId])
WHERE [t].[Nickname] = N'Marcus'",
                Sql);
        }

        public override void Include_with_join_reference1()
        {
            base.Include_with_join_reference1();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.CityOfBirth].[Name], [g.CityOfBirth].[Location]
FROM [Gear] AS [g]
INNER JOIN [City] AS [g.CityOfBirth] ON [g].[CityOrBirthName] = [g.CityOfBirth].[Name]
INNER JOIN [CogTag] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')",
                Sql);
        }

        public override void Include_with_join_reference2()
        {
            base.Include_with_join_reference2();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.CityOfBirth].[Name], [g.CityOfBirth].[Location]
FROM [CogTag] AS [t]
INNER JOIN [Gear] AS [g] ON ([t].[GearSquadId] = [g].[SquadId]) AND ([t].[GearNickName] = [g].[Nickname])
INNER JOIN [City] AS [g.CityOfBirth] ON [g].[CityOrBirthName] = [g.CityOfBirth].[Name]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')",
                Sql);
        }

        public override void Include_with_join_collection1()
        {
            base.Include_with_join_collection1();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
INNER JOIN [CogTag] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[FullName]

SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId]
FROM [Weapon] AS [g.Weapons]
INNER JOIN (
    SELECT DISTINCT [g0].[FullName]
    FROM [Gear] AS [g0]
    INNER JOIN [CogTag] AS [t0] ON ([g0].[SquadId] = [t0].[GearSquadId]) AND ([g0].[Nickname] = [t0].[GearNickName])
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t1] ON [g.Weapons].[OwnerFullName] = [t1].[FullName]
ORDER BY [t1].[FullName]",
                Sql);
        }

        public override void Include_with_join_collection2()
        {
            base.Include_with_join_collection2();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [CogTag] AS [t]
INNER JOIN [Gear] AS [g] ON ([t].[GearSquadId] = [g].[SquadId]) AND ([t].[GearNickName] = [g].[Nickname])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[FullName]

SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId]
FROM [Weapon] AS [g.Weapons]
INNER JOIN (
    SELECT DISTINCT [g0].[FullName]
    FROM [CogTag] AS [t0]
    INNER JOIN [Gear] AS [g0] ON ([t0].[GearSquadId] = [g0].[SquadId]) AND ([t0].[GearNickName] = [g0].[Nickname])
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
) AS [t1] ON [g.Weapons].[OwnerFullName] = [t1].[FullName]
ORDER BY [t1].[FullName]",
                Sql);
        }

        public override void Include_where_list_contains_navigation()
        {
            base.Include_where_list_contains_navigation();

            Assert.StartsWith(
                @"SELECT [t].[Id]
FROM [CogTag] AS [t]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Tag].[Id], [g.Tag].[GearNickName], [g.Tag].[GearSquadId], [g.Tag].[Note]
FROM [Gear] AS [g]
LEFT JOIN [CogTag] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g.Tag].[Id] IS NOT NULL AND [g.Tag].[Id] IN ('",
                Sql);

            Assert.EndsWith(
                @"'))",
                Sql);
        }

        public override void Include_where_list_contains_navigation2()
        {
            base.Include_where_list_contains_navigation2();

            Assert.StartsWith(
                @"SELECT [t].[Id]
FROM [CogTag] AS [t]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Tag].[Id], [g.Tag].[GearNickName], [g.Tag].[GearSquadId], [g.Tag].[Note]
FROM [Gear] AS [g]
LEFT JOIN [CogTag] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
INNER JOIN [City] AS [g.CityOfBirth] ON [g].[CityOrBirthName] = [g.CityOfBirth].[Name]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g.CityOfBirth].[Location] IS NOT NULL AND [g.Tag].[Id] IN ('",
                Sql);

            Assert.EndsWith(
                @"'))",
                Sql);
        }

        public override void Navigation_accessed_twice_outside_and_inside_subquery()
        {
            base.Navigation_accessed_twice_outside_and_inside_subquery();

            Assert.StartsWith(
                @"SELECT [t].[Id]
FROM [CogTag] AS [t]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
LEFT JOIN [CogTag] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g.Tag].[Id] IS NOT NULL AND [g.Tag].[Id] IN ('",
                Sql);

            Assert.EndsWith(
                @"'))",
                Sql);
        }

        public override void Include_with_join_multi_level()
        {
            base.Include_with_join_multi_level();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location]
FROM [Gear] AS [g]
INNER JOIN [CogTag] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [c].[Name]

SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOrBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gear] AS [g0]
WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND EXISTS (
    SELECT 1
    FROM [Gear] AS [g]
    INNER JOIN [CogTag] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
    INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g0].[AssignedCityName] = [c].[Name]))
ORDER BY [g0].[AssignedCityName]",
                Sql);
        }

        public override void Include_with_join_and_inheritance1()
        {
            base.Include_with_join_and_inheritance1();

            AssertSql(
                @"SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOrBirthName], [o].[Discriminator], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], [o.CityOfBirth].[Name], [o.CityOfBirth].[Location]
FROM [CogTag] AS [t]
INNER JOIN [Gear] AS [o] ON ([t].[GearSquadId] = [o].[SquadId]) AND ([t].[GearNickName] = [o].[Nickname])
INNER JOIN [City] AS [o.CityOfBirth] ON [o].[CityOrBirthName] = [o.CityOfBirth].[Name]
WHERE [o].[Discriminator] = N'Officer'",
                Sql);
        }

        public override void Include_with_join_and_inheritance2()
        {
            base.Include_with_join_and_inheritance2();

            AssertSql(
                @"SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOrBirthName], [o].[Discriminator], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank]
FROM [Gear] AS [o]
INNER JOIN [CogTag] AS [t] ON ([o].[SquadId] = [t].[GearSquadId]) AND ([o].[Nickname] = [t].[GearNickName])
WHERE [o].[Discriminator] = N'Officer'
ORDER BY [o].[FullName]

SELECT [o.Weapons].[Id], [o.Weapons].[AmmunitionType], [o.Weapons].[IsAutomatic], [o.Weapons].[Name], [o.Weapons].[OwnerFullName], [o.Weapons].[SynergyWithId]
FROM [Weapon] AS [o.Weapons]
INNER JOIN (
    SELECT DISTINCT [o0].[FullName]
    FROM [Gear] AS [o0]
    INNER JOIN [CogTag] AS [t0] ON ([o0].[SquadId] = [t0].[GearSquadId]) AND ([o0].[Nickname] = [t0].[GearNickName])
    WHERE [o0].[Discriminator] = N'Officer'
) AS [t1] ON [o.Weapons].[OwnerFullName] = [t1].[FullName]
ORDER BY [t1].[FullName]",
                Sql);
        }

        public override void Include_with_join_and_inheritance3()
        {
            base.Include_with_join_and_inheritance3();

            AssertSql(
                @"SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOrBirthName], [o].[Discriminator], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank]
FROM [CogTag] AS [t]
INNER JOIN [Gear] AS [o] ON ([t].[GearSquadId] = [o].[SquadId]) AND ([t].[GearNickName] = [o].[Nickname])
WHERE [o].[Discriminator] = N'Officer'
ORDER BY [o].[Nickname], [o].[SquadId]

SELECT [o.Reports].[Nickname], [o.Reports].[SquadId], [o.Reports].[AssignedCityName], [o.Reports].[CityOrBirthName], [o.Reports].[Discriminator], [o.Reports].[FullName], [o.Reports].[HasSoulPatch], [o.Reports].[LeaderNickname], [o.Reports].[LeaderSquadId], [o.Reports].[Rank]
FROM [Gear] AS [o.Reports]
INNER JOIN (
    SELECT DISTINCT [o0].[Nickname], [o0].[SquadId]
    FROM [CogTag] AS [t0]
    INNER JOIN [Gear] AS [o0] ON ([t0].[GearSquadId] = [o0].[SquadId]) AND ([t0].[GearNickName] = [o0].[Nickname])
    WHERE [o0].[Discriminator] = N'Officer'
) AS [t1] ON ([o.Reports].[LeaderNickname] = [t1].[Nickname]) AND ([o.Reports].[LeaderSquadId] = [t1].[SquadId])
WHERE [o.Reports].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t1].[Nickname], [t1].[SquadId]",
                Sql);
        }

        public override void Include_with_nested_navigation_in_order_by()
        {
            base.Include_with_nested_navigation_in_order_by();

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [Weapon] AS [w]
LEFT JOIN (
    SELECT [w.Owner].*
    FROM [Gear] AS [w.Owner]
    WHERE [w.Owner].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [w].[OwnerFullName] = [t].[FullName]
LEFT JOIN [City] AS [w.Owner.CityOfBirth] ON [t].[CityOrBirthName] = [w.Owner.CityOfBirth].[Name]
ORDER BY [w.Owner.CityOfBirth].[Name]",
                Sql);
        }

        public override void Where_enum()
        {
            base.Where_enum();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Rank] = 2)",
                Sql);
        }

        public override void Where_nullable_enum_with_constant()
        {
            base.Where_nullable_enum_with_constant();

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE [w].[AmmunitionType] = 1",
                Sql);
        }

        public override void Where_nullable_enum_with_null_constant()
        {
            base.Where_nullable_enum_with_null_constant();

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE [w].[AmmunitionType] IS NULL",
                Sql);
        }

        public override void Where_nullable_enum_with_non_nullable_parameter()
        {
            base.Where_nullable_enum_with_non_nullable_parameter();

            AssertSql(
                @"@__ammunitionType_0: Cartridge

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE [w].[AmmunitionType] = @__ammunitionType_0",
                Sql);
        }

        public override void Where_nullable_enum_with_nullable_parameter()
        {
            base.Where_nullable_enum_with_nullable_parameter();

            AssertSql(
                @"@__ammunitionType_0: Cartridge (Nullable = true)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE [w].[AmmunitionType] = @__ammunitionType_0

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE [w].[AmmunitionType] IS NULL",
                Sql);
        }

        [ConditionalFact]
        public override void Where_bitwise_and_enum()
        {
            base.Where_bitwise_and_enum();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) > 0)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) = 1)",
                Sql);
        }

        [ConditionalFact]
        public override void Where_bitwise_and_integral()
        {
            base.Where_bitwise_and_integral();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) = 1)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) = 1)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) = 1)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) = 1)",
                Sql);
        }

        [ConditionalFact]
        public override void Where_bitwise_and_nullable_enum_with_constant()
        {
            base.Where_bitwise_and_nullable_enum_with_constant();

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE ([w].[AmmunitionType] & 1) > 0",
                Sql);
        }

        [ConditionalFact]
        public override void Where_bitwise_and_nullable_enum_with_null_constant()
        {
            base.Where_bitwise_and_nullable_enum_with_null_constant();

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE ([w].[AmmunitionType] & NULL) > 0",
                Sql);
        }

        [ConditionalFact]
        public override void Where_bitwise_and_nullable_enum_with_non_nullable_parameter()
        {
            base.Where_bitwise_and_nullable_enum_with_non_nullable_parameter();

            AssertSql(
                @"@__ammunitionType_0: Cartridge

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE ([w].[AmmunitionType] & @__ammunitionType_0) > 0",
                Sql);
        }

        [ConditionalFact]
        public override void Where_bitwise_and_nullable_enum_with_nullable_parameter()
        {
            base.Where_bitwise_and_nullable_enum_with_nullable_parameter();

            AssertSql(
                @"@__ammunitionType_0: Cartridge (Nullable = true)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE ([w].[AmmunitionType] & @__ammunitionType_0) > 0

@__ammunitionType_0:  (DbType = Int32)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE ([w].[AmmunitionType] & @__ammunitionType_0) > 0",
                Sql);
        }

        [ConditionalFact]
        public override void Where_bitwise_or_enum()
        {
            base.Where_bitwise_or_enum();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] | 1) > 0)",
                Sql);
        }

        [ConditionalFact]
        public override void Bitwise_projects_values_in_select()
        {
            base.Bitwise_projects_values_in_select();

            AssertSql(
                @"SELECT TOP(1) CASE
    WHEN ([g].[Rank] & 1) = 1
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END, CASE
    WHEN ([g].[Rank] & 1) = 2
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END, [g].[Rank] & 1
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) = 1)",
                Sql);
        }

        public override void Where_enum_has_flag()
        {
            base.Where_enum_has_flag();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) = 1)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 5) = 5)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) = 1)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) = 1)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ((1 & [g].[Rank]) = [g].[Rank])",
                Sql);
        }

        public override void Where_enum_has_flag_subquery()
        {
            base.Where_enum_has_flag_subquery();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & (
    SELECT TOP(1) [x].[Rank]
    FROM [Gear] AS [x]
    WHERE [x].[Discriminator] IN (N'Officer', N'Gear')
    ORDER BY [x].[Nickname], [x].[SquadId]
)) = (
    SELECT TOP(1) [x].[Rank]
    FROM [Gear] AS [x]
    WHERE [x].[Discriminator] IN (N'Officer', N'Gear')
    ORDER BY [x].[Nickname], [x].[SquadId]
))

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ((1 & (
    SELECT TOP(1) [x].[Rank]
    FROM [Gear] AS [x]
    WHERE [x].[Discriminator] IN (N'Officer', N'Gear')
    ORDER BY [x].[Nickname], [x].[SquadId]
)) = (
    SELECT TOP(1) [x].[Rank]
    FROM [Gear] AS [x]
    WHERE [x].[Discriminator] IN (N'Officer', N'Gear')
    ORDER BY [x].[Nickname], [x].[SquadId]
))",
                Sql);
        }

        public override void Where_enum_has_flag_with_non_nullable_parameter()
        {
            base.Where_enum_has_flag_with_non_nullable_parameter();

            AssertSql(
                @"@__parameter_0: Corporal

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & @__parameter_0) = @__parameter_0)",
                Sql);
        }

        public override void Where_has_flag_with_nullable_parameter()
        {
            base.Where_has_flag_with_nullable_parameter();

            AssertSql(
                @"@__parameter_0: Corporal (Nullable = true)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & @__parameter_0) = @__parameter_0)",
                Sql);
        }

        public override void Select_enum_has_flag()
        {
            base.Select_enum_has_flag();

            AssertSql(
                @"SELECT TOP(1) CASE
    WHEN ([g].[Rank] & 1) = 1
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END, CASE
    WHEN ([g].[Rank] & 2) = 2
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & 1) = 1)",
                Sql);
        }

        public override void Where_count_subquery_without_collision()
        {
            base.Where_count_subquery_without_collision();

            AssertSql(
                @"SELECT [w].[Nickname], [w].[SquadId], [w].[AssignedCityName], [w].[CityOrBirthName], [w].[Discriminator], [w].[FullName], [w].[HasSoulPatch], [w].[LeaderNickname], [w].[LeaderSquadId], [w].[Rank]
FROM [Gear] AS [w]
WHERE [w].[Discriminator] IN (N'Officer', N'Gear') AND ((
    SELECT COUNT(*)
    FROM [Weapon] AS [w0]
    WHERE [w].[FullName] = [w0].[OwnerFullName]
) = 2)",
                Sql);
        }

        public override void Where_any_subquery_without_collision()
        {
            base.Where_any_subquery_without_collision();

            AssertSql(
                @"SELECT [w].[Nickname], [w].[SquadId], [w].[AssignedCityName], [w].[CityOrBirthName], [w].[Discriminator], [w].[FullName], [w].[HasSoulPatch], [w].[LeaderNickname], [w].[LeaderSquadId], [w].[Rank]
FROM [Gear] AS [w]
WHERE [w].[Discriminator] IN (N'Officer', N'Gear') AND EXISTS (
    SELECT 1
    FROM [Weapon] AS [w0]
    WHERE [w].[FullName] = [w0].[OwnerFullName])",
                Sql);
        }

        public override void Select_inverted_boolean()
        {
            base.Select_inverted_boolean();

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = 0
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
FROM [Weapon] AS [w]
WHERE [w].[IsAutomatic] = 1",
                Sql);
        }

        public override void Select_comparison_with_null()
        {
            base.Select_comparison_with_null();

            AssertSql(
                @"@__ammunitionType_1: Cartridge (Nullable = true)
@__ammunitionType_0: Cartridge (Nullable = true)

SELECT [w].[Id], CASE
    WHEN [w].[AmmunitionType] = @__ammunitionType_1
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
FROM [Weapon] AS [w]
WHERE [w].[AmmunitionType] = @__ammunitionType_0

SELECT [w].[Id], CASE
    WHEN [w].[AmmunitionType] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
FROM [Weapon] AS [w]
WHERE [w].[AmmunitionType] IS NULL",
                Sql);
        }

        public override void Select_ternary_operation_with_boolean()
        {
            base.Select_ternary_operation_with_boolean();

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = 1
    THEN 1 ELSE 0
END
FROM [Weapon] AS [w]",
                Sql);
        }

        public override void Select_ternary_operation_with_inverted_boolean()
        {
            base.Select_ternary_operation_with_inverted_boolean();

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = 0
    THEN 1 ELSE 0
END
FROM [Weapon] AS [w]",
                Sql);
        }

        public override void Select_ternary_operation_with_has_value_not_null()
        {
            base.Select_ternary_operation_with_has_value_not_null();

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN [w].[AmmunitionType] IS NOT NULL AND ([w].[AmmunitionType] = 1)
    THEN N'Yes' ELSE N'No'
END
FROM [Weapon] AS [w]
WHERE [w].[AmmunitionType] IS NOT NULL AND ([w].[AmmunitionType] = 1)",
                Sql);
        }

        public override void Select_ternary_operation_multiple_conditions()
        {
            base.Select_ternary_operation_multiple_conditions();

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN ([w].[AmmunitionType] = 2) AND ([w].[SynergyWithId] = 1)
    THEN N'Yes' ELSE N'No'
END
FROM [Weapon] AS [w]",
                Sql);
        }

        public override void Select_ternary_operation_multiple_conditions_2()
        {
            base.Select_ternary_operation_multiple_conditions_2();

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN ([w].[IsAutomatic] = 0) AND ([w].[SynergyWithId] = 1)
    THEN N'Yes' ELSE N'No'
END
FROM [Weapon] AS [w]",
                Sql);
        }

        public override void Select_multiple_conditions()
        {
            base.Select_multiple_conditions();

            AssertSql(
                @"SELECT [w].[Id], CASE
    WHEN ([w].[IsAutomatic] = 0) AND ([w].[SynergyWithId] = 1)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
FROM [Weapon] AS [w]",
                Sql);
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
END
FROM [Weapon] AS [w]",
                Sql);
        }

        public override void Null_propagation_optimization1()
        {
            base.Null_propagation_optimization1();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[LeaderNickname] = N'Marcus')",
                Sql);
        }

        public override void Null_propagation_optimization2()
        {
            base.Null_propagation_optimization2();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (RIGHT([g].[LeaderNickname], LEN(N'us')) = N'us')",
                Sql);
        }

        public override void Null_propagation_optimization3()
        {
            base.Null_propagation_optimization3();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (RIGHT([g].[LeaderNickname], LEN(N'us')) = N'us')",
                Sql);
        }

        public override void Null_propagation_optimization4()
        {
            base.Null_propagation_optimization4();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (LEN([g].[LeaderNickname]) = 5)",
                Sql);
        }

        public override void Null_propagation_optimization5()
        {
            base.Null_propagation_optimization5();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (LEN([g].[LeaderNickname]) = 5)",
                Sql);
        }

        public override void Null_propagation_optimization6()
        {
            base.Null_propagation_optimization6();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (LEN([g].[LeaderNickname]) = 5)",
                Sql);
        }

        public override void Select_null_propagation_negative1()
        {
            base.Select_null_propagation_negative1();

            AssertSql(
                @"SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL
    THEN CASE
        WHEN LEN([g].[Nickname]) = 5
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END ELSE NULL
END
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')",
                Sql);
        }

        public override void Select_null_propagation_negative2()
        {
            base.Select_null_propagation_negative2();

            AssertSql(
                @"SELECT CASE
    WHEN [g1].[LeaderNickname] IS NOT NULL
    THEN [g2].[LeaderNickname] ELSE NULL
END
FROM [Gear] AS [g1]
CROSS JOIN [Gear] AS [g2]
WHERE [g1].[Discriminator] IN (N'Officer', N'Gear')",
                Sql);
        }

        public override void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar()
        {
            base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar();

            AssertSql(
                @"SELECT [ct1].[Id], [ct1].[GearNickName], [ct1].[GearSquadId], [ct1].[Note], [ct2].[Id], [ct2].[GearNickName], [ct2].[GearSquadId], [ct2].[Note]
FROM [CogTag] AS [ct1]
LEFT JOIN (
    SELECT [ct1.Gear].*
    FROM [Gear] AS [ct1.Gear]
    WHERE [ct1.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([ct1].[GearNickName] = [t].[Nickname]) AND ([ct1].[GearSquadId] = [t].[SquadId])
CROSS JOIN [CogTag] AS [ct2]
LEFT JOIN (
    SELECT [ct2.Gear].*
    FROM [Gear] AS [ct2.Gear]
    WHERE [ct2.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([ct2].[GearNickName] = [t0].[Nickname]) AND ([ct2].[GearSquadId] = [t0].[SquadId])
WHERE ([t].[Nickname] = [t0].[Nickname]) OR ([t].[Nickname] IS NULL AND [t0].[Nickname] IS NULL)",
                Sql);
        }

        public override void Select_Singleton_Navigation_With_Member_Access()
        {
            base.Select_Singleton_Navigation_With_Member_Access();

            AssertSql(
                @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [CogTag] AS [ct]
LEFT JOIN (
    SELECT [ct.Gear].*
    FROM [Gear] AS [ct.Gear]
    WHERE [ct.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([ct].[GearNickName] = [t].[Nickname]) AND ([ct].[GearSquadId] = [t].[SquadId])
WHERE ([t].[Nickname] = N'Marcus') AND (([t].[CityOrBirthName] <> N'Ephyra') OR [t].[CityOrBirthName] IS NULL)",
                Sql);
        }

        public override void Select_Where_Navigation()
        {
            base.Select_Where_Navigation();

            AssertSql(
                @"SELECT [ct].[Id], [ct].[GearNickName], [ct].[GearSquadId], [ct].[Note]
FROM [CogTag] AS [ct]
LEFT JOIN (
    SELECT [ct.Gear].*
    FROM [Gear] AS [ct.Gear]
    WHERE [ct.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([ct].[GearNickName] = [t].[Nickname]) AND ([ct].[GearSquadId] = [t].[SquadId])
WHERE [t].[Nickname] = N'Marcus'",
                Sql);
        }

        public override void Select_Where_Navigation_Client()
        {
            base.Select_Where_Navigation_Client();

            AssertSql(
                @"SELECT [o].[Id], [o].[GearNickName], [o].[GearSquadId], [o].[Note], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [CogTag] AS [o]
LEFT JOIN (
    SELECT [o.Gear].*
    FROM [Gear] AS [o.Gear]
    WHERE [o.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([o].[GearNickName] = [t].[Nickname]) AND ([o].[GearSquadId] = [t].[SquadId])
WHERE [o].[GearNickName] IS NOT NULL OR [o].[GearSquadId] IS NOT NULL",
                Sql);
        }

        public override void Select_Where_Navigation_Equals_Navigation()
        {
            base.Select_Where_Navigation_Equals_Navigation();

            AssertSql(
                @"SELECT [ct1].[Id], [ct1].[GearNickName], [ct1].[GearSquadId], [ct1].[Note], [ct2].[Id], [ct2].[GearNickName], [ct2].[GearSquadId], [ct2].[Note]
FROM [CogTag] AS [ct1]
LEFT JOIN (
    SELECT [ct1.Gear].*
    FROM [Gear] AS [ct1.Gear]
    WHERE [ct1.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([ct1].[GearNickName] = [t].[Nickname]) AND ([ct1].[GearSquadId] = [t].[SquadId])
CROSS JOIN [CogTag] AS [ct2]
LEFT JOIN (
    SELECT [ct2.Gear].*
    FROM [Gear] AS [ct2.Gear]
    WHERE [ct2.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([ct2].[GearNickName] = [t0].[Nickname]) AND ([ct2].[GearSquadId] = [t0].[SquadId])
WHERE (([t].[Nickname] = [t0].[Nickname]) OR ([t].[Nickname] IS NULL AND [t0].[Nickname] IS NULL)) AND (([t].[SquadId] = [t0].[SquadId]) OR ([t].[SquadId] IS NULL AND [t0].[SquadId] IS NULL))",
                Sql);
        }

        public override void Select_Where_Navigation_Null()
        {
            base.Select_Where_Navigation_Null();

            AssertSql(
                @"SELECT [ct].[Id], [ct].[GearNickName], [ct].[GearSquadId], [ct].[Note]
FROM [CogTag] AS [ct]
WHERE [ct].[GearNickName] IS NULL AND [ct].[GearSquadId] IS NULL",
                Sql);
        }

        public override void Select_Where_Navigation_Null_Reverse()
        {
            base.Select_Where_Navigation_Null_Reverse();

            AssertSql(
                @"SELECT [ct].[Id], [ct].[GearNickName], [ct].[GearSquadId], [ct].[Note]
FROM [CogTag] AS [ct]
WHERE [ct].[GearNickName] IS NULL AND [ct].[GearSquadId] IS NULL",
                Sql);
        }

        public override void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected()
        {
            base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected();

            AssertSql(
                @"SELECT [ct1].[Id], [ct2].[Id]
FROM [CogTag] AS [ct1]
LEFT JOIN (
    SELECT [ct1.Gear].*
    FROM [Gear] AS [ct1.Gear]
    WHERE [ct1.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([ct1].[GearNickName] = [t].[Nickname]) AND ([ct1].[GearSquadId] = [t].[SquadId])
CROSS JOIN [CogTag] AS [ct2]
LEFT JOIN (
    SELECT [ct2.Gear].*
    FROM [Gear] AS [ct2.Gear]
    WHERE [ct2.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([ct2].[GearNickName] = [t0].[Nickname]) AND ([ct2].[GearSquadId] = [t0].[SquadId])
WHERE ([t].[Nickname] = [t0].[Nickname]) OR ([t].[Nickname] IS NULL AND [t0].[Nickname] IS NULL)",
                Sql);
        }

        public override void Optional_Navigation_Null_Coalesce_To_Clr_Type()
        {
            base.Optional_Navigation_Null_Coalesce_To_Clr_Type();

            AssertSql(
                @"SELECT TOP(1) CAST(COALESCE([w.SynergyWith].[IsAutomatic], 0) AS bit)
FROM [Weapon] AS [w]
LEFT JOIN [Weapon] AS [w.SynergyWith] ON [w].[SynergyWithId] = [w.SynergyWith].[Id]",
                Sql);
        }

        public override void Where_subquery_boolean()
        {
            base.Where_subquery_boolean();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ((
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapon] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
) = 1)",
                Sql);
        }

        public override void Singleton_Navigation_With_Member_Access()
        {
            base.Singleton_Navigation_With_Member_Access();

            AssertSql(
                @"SELECT [t].[CityOrBirthName]
FROM [CogTag] AS [ct]
LEFT JOIN (
    SELECT [ct.Gear].*
    FROM [Gear] AS [ct.Gear]
    WHERE [ct.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON ([ct].[GearNickName] = [t].[Nickname]) AND ([ct].[GearSquadId] = [t].[SquadId])
WHERE ([t].[Nickname] = N'Marcus') AND (([t].[CityOrBirthName] <> N'Ephyra') OR [t].[CityOrBirthName] IS NULL)",
                Sql);
        }

        public override void GroupJoin_Composite_Key()
        {
            base.GroupJoin_Composite_Key();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [CogTag] AS [ct]
INNER JOIN [Gear] AS [g] ON ([ct].[GearNickName] = [g].[Nickname]) AND ([ct].[GearSquadId] = [g].[SquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')",
                Sql);
        }

        public override void Join_navigation_translated_to_subquery_composite_key()
        {
            base.Join_navigation_translated_to_subquery_composite_key();

            AssertSql(
                @"SELECT [g].[FullName], [t].[Note]
FROM [Gear] AS [g]
INNER JOIN [CogTag] AS [t] ON [g].[FullName] = (
    SELECT TOP(1) [subQuery0].[FullName]
    FROM [Gear] AS [subQuery0]
    WHERE [subQuery0].[Discriminator] IN (N'Officer', N'Gear') AND (([subQuery0].[Nickname] = [t].[GearNickName]) AND ([subQuery0].[SquadId] = [t].[GearSquadId]))
)
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')",
                Sql);
        }

        public override void Collection_with_inheritance_and_join_include_joined()
        {
            base.Collection_with_inheritance_and_join_include_joined();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Tag].[Id], [g.Tag].[GearNickName], [g.Tag].[GearSquadId], [g.Tag].[Note]
FROM [CogTag] AS [t]
INNER JOIN [Gear] AS [g] ON ([t].[GearSquadId] = [g].[SquadId]) AND ([t].[GearNickName] = [g].[Nickname])
LEFT JOIN [CogTag] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE [g].[Discriminator] = N'Officer'",
                Sql);
        }

        public override void Collection_with_inheritance_and_join_include_source()
        {
            base.Collection_with_inheritance_and_join_include_source();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Tag].[Id], [g.Tag].[GearNickName], [g.Tag].[GearSquadId], [g.Tag].[Note]
FROM [Gear] AS [g]
LEFT JOIN [CogTag] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
INNER JOIN [CogTag] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
WHERE [g].[Discriminator] = N'Officer'",
                Sql);
        }

        public override void Non_unicode_string_literal_is_used_for_non_unicode_column()
        {
            base.Non_unicode_string_literal_is_used_for_non_unicode_column();

            AssertSql(
                @"SELECT [c].[Name], [c].[Location]
FROM [City] AS [c]
WHERE [c].[Location] = 'Unknown'",
                Sql);
        }

        public override void Non_unicode_string_literal_is_used_for_non_unicode_column_right()
        {
            base.Non_unicode_string_literal_is_used_for_non_unicode_column_right();

            AssertSql(
                @"SELECT [c].[Name], [c].[Location]
FROM [City] AS [c]
WHERE 'Unknown' = [c].[Location]",
                Sql);
        }

        public override void Non_unicode_parameter_is_used_for_non_unicode_column()
        {
            base.Non_unicode_parameter_is_used_for_non_unicode_column();

            AssertSql(
                @"@__value_0: Unknown (Size = 100) (DbType = AnsiString)

SELECT [c].[Name], [c].[Location]
FROM [City] AS [c]
WHERE [c].[Location] = @__value_0",
                Sql);
        }

        public override void Non_unicode_string_literals_in_contains_is_used_for_non_unicode_column()
        {
            base.Non_unicode_string_literals_in_contains_is_used_for_non_unicode_column();

            AssertSql(
                @"SELECT [c].[Name], [c].[Location]
FROM [City] AS [c]
WHERE [c].[Location] IN ('Unknown', 'Jacinto''s location', 'Ephyra''s location')",
                Sql);
        }

        public override void Non_unicode_string_literals_is_used_for_non_unicode_column_with_subquery()
        {
            base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_subquery();

            AssertSql(
                @"SELECT [c].[Name], [c].[Location]
FROM [City] AS [c]
WHERE ([c].[Location] = 'Unknown') AND ((
    SELECT COUNT(*)
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] = N'Paduk')) AND ([c].[Name] = [g].[CityOrBirthName])
) = 1)", Sql);
        }

        public override void Non_unicode_string_literals_is_used_for_non_unicode_column_in_subquery()
        {
            base.Non_unicode_string_literals_is_used_for_non_unicode_column_in_subquery();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
INNER JOIN [City] AS [g.CityOfBirth] ON [g].[CityOrBirthName] = [g.CityOfBirth].[Name]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Nickname] = N'Marcus') AND ([g.CityOfBirth].[Location] = 'Jacinto''s location'))",
                Sql);
        }

        public override void Non_unicode_string_literals_is_used_for_non_unicode_column_with_contains()
        {
            base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_contains();

            AssertSql(
                @"SELECT [c].[Name], [c].[Location]
FROM [City] AS [c]
WHERE CHARINDEX(N'Jacinto', [c].[Location]) > 0",
                Sql);
        }

        public override void Non_unicode_string_literals_is_used_for_non_unicode_column_with_concat()
        {
            base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_concat();

            AssertSql(
                @"SELECT [c].[Name], [c].[Location]
FROM [City] AS [c]
WHERE CHARINDEX('Add', [c].[Location] + 'Added') > 0",
                Sql);
        }

        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [Gear] AS [g]
LEFT JOIN (
    SELECT [g2].*
    FROM [Gear] AS [g2]
    WHERE [g2].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [g].[LeaderNickname] = [t].[Nickname]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[FullName]

SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId]
FROM [Weapon] AS [g.Weapons]
INNER JOIN (
    SELECT DISTINCT [g0].[FullName]
    FROM [Gear] AS [g0]
    LEFT JOIN (
        SELECT [g20].*
        FROM [Gear] AS [g20]
        WHERE [g20].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t0] ON [g0].[LeaderNickname] = [t0].[Nickname]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND [t0].[Nickname] IS NULL
) AS [t1] ON [g.Weapons].[OwnerFullName] = [t1].[FullName]
ORDER BY [t1].[FullName]",
                Sql);
        }

        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2();

            AssertSql(
                @"SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOrBirthName], [g1].[Discriminator], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[Rank], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [Gear] AS [g1]
LEFT JOIN (
    SELECT [g2].*
    FROM [Gear] AS [g2]
    WHERE [g2].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [g1].[LeaderNickname] = [t].[Nickname]
WHERE [g1].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[FullName]

SELECT [g2.Weapons].[Id], [g2.Weapons].[AmmunitionType], [g2.Weapons].[IsAutomatic], [g2.Weapons].[Name], [g2.Weapons].[OwnerFullName], [g2.Weapons].[SynergyWithId]
FROM [Weapon] AS [g2.Weapons]
INNER JOIN (
    SELECT DISTINCT [t0].[FullName]
    FROM [Gear] AS [g10]
    LEFT JOIN (
        SELECT [g20].*
        FROM [Gear] AS [g20]
        WHERE [g20].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t0] ON [g10].[LeaderNickname] = [t0].[Nickname]
    WHERE [g10].[Discriminator] IN (N'Officer', N'Gear') AND [t0].[Nickname] IS NOT NULL
) AS [t1] ON [g2.Weapons].[OwnerFullName] = [t1].[FullName]
ORDER BY [t1].[FullName]",
                Sql);
        }

        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [Gear] AS [g]
LEFT JOIN (
    SELECT [g2].*
    FROM [Gear] AS [g2]
    WHERE [g2].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [g].[LeaderNickname] = [t].[Nickname]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[FullName], [g].[FullName]

SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId]
FROM [Weapon] AS [g.Weapons]
INNER JOIN (
    SELECT DISTINCT [g1].[FullName], [t2].[FullName] AS [c0]
    FROM [Gear] AS [g1]
    LEFT JOIN (
        SELECT [g21].*
        FROM [Gear] AS [g21]
        WHERE [g21].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t2] ON [g1].[LeaderNickname] = [t2].[Nickname]
    WHERE [g1].[Discriminator] IN (N'Officer', N'Gear') AND [t2].[Nickname] IS NULL
) AS [t3] ON [g.Weapons].[OwnerFullName] = [t3].[FullName]
ORDER BY [t3].[c0], [t3].[FullName]

SELECT [g2.Weapons].[Id], [g2.Weapons].[AmmunitionType], [g2.Weapons].[IsAutomatic], [g2.Weapons].[Name], [g2.Weapons].[OwnerFullName], [g2.Weapons].[SynergyWithId]
FROM [Weapon] AS [g2.Weapons]
INNER JOIN (
    SELECT DISTINCT [t0].[FullName]
    FROM [Gear] AS [g0]
    LEFT JOIN (
        SELECT [g20].*
        FROM [Gear] AS [g20]
        WHERE [g20].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t0] ON [g0].[LeaderNickname] = [t0].[Nickname]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND [t0].[Nickname] IS NOT NULL
) AS [t1] ON [g2.Weapons].[OwnerFullName] = [t1].[FullName]
ORDER BY [t1].[FullName]",
                Sql);
        }

        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [Gear] AS [g]
LEFT JOIN (
    SELECT [g2].*
    FROM [Gear] AS [g2]
    WHERE [g2].[Discriminator] = N'Officer'
) AS [t] ON [g].[LeaderNickname] = [t].[Nickname]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[FullName], [g].[FullName]

SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId]
FROM [Weapon] AS [g.Weapons]
INNER JOIN (
    SELECT DISTINCT [g1].[FullName], [t2].[FullName] AS [c0]
    FROM [Gear] AS [g1]
    LEFT JOIN (
        SELECT [g21].*
        FROM [Gear] AS [g21]
        WHERE [g21].[Discriminator] = N'Officer'
    ) AS [t2] ON [g1].[LeaderNickname] = [t2].[Nickname]
    WHERE [g1].[Discriminator] IN (N'Officer', N'Gear') AND [t2].[Nickname] IS NULL
) AS [t3] ON [g.Weapons].[OwnerFullName] = [t3].[FullName]
ORDER BY [t3].[c0], [t3].[FullName]

SELECT [g2.Weapons].[Id], [g2.Weapons].[AmmunitionType], [g2.Weapons].[IsAutomatic], [g2.Weapons].[Name], [g2.Weapons].[OwnerFullName], [g2.Weapons].[SynergyWithId]
FROM [Weapon] AS [g2.Weapons]
INNER JOIN (
    SELECT DISTINCT [t0].[FullName]
    FROM [Gear] AS [g0]
    LEFT JOIN (
        SELECT [g20].*
        FROM [Gear] AS [g20]
        WHERE [g20].[Discriminator] = N'Officer'
    ) AS [t0] ON [g0].[LeaderNickname] = [t0].[Nickname]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND [t0].[Nickname] IS NOT NULL
) AS [t1] ON [g2.Weapons].[OwnerFullName] = [t1].[FullName]
ORDER BY [t1].[FullName]",
                Sql);
        }

        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [Gear] AS [g]
LEFT JOIN (
    SELECT [g2].*
    FROM [Gear] AS [g2]
    WHERE [g2].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [g].[LeaderNickname] = [t].[Nickname]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [t].[FullName], [g].[FullName]

SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId]
FROM [Weapon] AS [g.Weapons]
INNER JOIN (
    SELECT DISTINCT [g1].[FullName], [t2].[FullName] AS [c0]
    FROM [Gear] AS [g1]
    LEFT JOIN (
        SELECT [g21].*
        FROM [Gear] AS [g21]
        WHERE [g21].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t2] ON [g1].[LeaderNickname] = [t2].[Nickname]
    WHERE [g1].[Discriminator] IN (N'Officer', N'Gear') AND [t2].[Nickname] IS NULL
) AS [t3] ON [g.Weapons].[OwnerFullName] = [t3].[FullName]
ORDER BY [t3].[c0], [t3].[FullName]

SELECT [g2.Weapons].[Id], [g2.Weapons].[AmmunitionType], [g2.Weapons].[IsAutomatic], [g2.Weapons].[Name], [g2.Weapons].[OwnerFullName], [g2.Weapons].[SynergyWithId]
FROM [Weapon] AS [g2.Weapons]
INNER JOIN (
    SELECT DISTINCT [t0].[FullName]
    FROM [Gear] AS [g0]
    LEFT JOIN (
        SELECT [g20].*
        FROM [Gear] AS [g20]
        WHERE [g20].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t0] ON [g0].[LeaderNickname] = [t0].[Nickname]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND [t0].[Nickname] IS NOT NULL
) AS [t1] ON [g2.Weapons].[OwnerFullName] = [t1].[FullName]
ORDER BY [t1].[FullName]",
                Sql);
        }

        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [Gear] AS [g]
LEFT JOIN (
    SELECT [g2].*
    FROM [Gear] AS [g2]
    WHERE [g2].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [g].[LeaderNickname] = [t].[Nickname]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[FullName], [t].[FullName]

SELECT [g.Weapons].[Id], [g.Weapons].[AmmunitionType], [g.Weapons].[IsAutomatic], [g.Weapons].[Name], [g.Weapons].[OwnerFullName], [g.Weapons].[SynergyWithId]
FROM [Weapon] AS [g.Weapons]
INNER JOIN (
    SELECT DISTINCT [g0].[FullName]
    FROM [Gear] AS [g0]
    LEFT JOIN (
        SELECT [g20].*
        FROM [Gear] AS [g20]
        WHERE [g20].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t0] ON [g0].[LeaderNickname] = [t0].[Nickname]
    WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND ([g0].[Nickname] IS NOT NULL AND [t0].[Nickname] IS NULL)
) AS [t1] ON [g.Weapons].[OwnerFullName] = [t1].[FullName]
ORDER BY [t1].[FullName]

SELECT [g2.Weapons].[Id], [g2.Weapons].[AmmunitionType], [g2.Weapons].[IsAutomatic], [g2.Weapons].[Name], [g2.Weapons].[OwnerFullName], [g2.Weapons].[SynergyWithId]
FROM [Weapon] AS [g2.Weapons]
INNER JOIN (
    SELECT DISTINCT [t2].[FullName], [g1].[FullName] AS [c0]
    FROM [Gear] AS [g1]
    LEFT JOIN (
        SELECT [g21].*
        FROM [Gear] AS [g21]
        WHERE [g21].[Discriminator] IN (N'Officer', N'Gear')
    ) AS [t2] ON [g1].[LeaderNickname] = [t2].[Nickname]
    WHERE [g1].[Discriminator] IN (N'Officer', N'Gear') AND [t2].[Nickname] IS NOT NULL
) AS [t3] ON [g2.Weapons].[OwnerFullName] = [t3].[FullName]
ORDER BY [t3].[c0], [t3].[FullName]",
                Sql);
        }

        public override void Coalesce_operator_in_predicate()
        {
            base.Coalesce_operator_in_predicate();

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE COALESCE([w].[IsAutomatic], 0) = 1",
                Sql);
        }

        public override void Coalesce_operator_in_predicate_with_other_conditions()
        {
            base.Coalesce_operator_in_predicate_with_other_conditions();

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE ([w].[AmmunitionType] = 1) AND (COALESCE([w].[IsAutomatic], 0) = 1)",
                Sql);
        }

        public override void Coalesce_operator_in_projection_with_other_conditions()
        {
            base.Coalesce_operator_in_projection_with_other_conditions();

            AssertSql(
                @"SELECT CASE
    WHEN ([w].[AmmunitionType] = 1) AND (COALESCE([w].[IsAutomatic], 0) = 1)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
FROM [Weapon] AS [w]",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_predicate()
        {
            base.Optional_navigation_type_compensation_works_with_predicate();

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gear] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE (([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL) AND ([t0].[HasSoulPatch] = 1)",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_predicate2()
        {
            base.Optional_navigation_type_compensation_works_with_predicate2();

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gear] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE [t0].[HasSoulPatch] = 1",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_predicate_negated()
        {
            base.Optional_navigation_type_compensation_works_with_predicate_negated();

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gear] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE ([t0].[HasSoulPatch] <> 1) AND [t0].[HasSoulPatch] IS NOT NULL",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_predicate_negated_complex1()
        {
            base.Optional_navigation_type_compensation_works_with_predicate_negated_complex1();

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gear] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE CASE
    WHEN [t0].[HasSoulPatch] = 1
    THEN 1 ELSE [t0].[HasSoulPatch]
END <> 1",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_predicate_negated_complex2()
        {
            base.Optional_navigation_type_compensation_works_with_predicate_negated_complex2();

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gear] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE CASE
    WHEN ([t0].[HasSoulPatch] <> 1) AND [t0].[HasSoulPatch] IS NOT NULL
    THEN 0 ELSE [t0].[HasSoulPatch]
END <> 1",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_conditional_expression()
        {
            base.Optional_navigation_type_compensation_works_with_conditional_expression();

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gear] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE CASE
    WHEN [t0].[HasSoulPatch] = 1
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = 1",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_binary_expression()
        {
            base.Optional_navigation_type_compensation_works_with_binary_expression();

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gear] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE ([t0].[HasSoulPatch] = 1) OR (CHARINDEX(N'Cole', [t].[Note]) > 0)",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_projection()
        {
            base.Optional_navigation_type_compensation_works_with_projection();

            AssertSql(
                @"SELECT [t0].[SquadId]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gear] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_projection_into_anonymous_type()
        {
            base.Optional_navigation_type_compensation_works_with_projection_into_anonymous_type();

            AssertSql(
                @"SELECT [t0].[SquadId]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gear] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_DTOs()
        {
            base.Optional_navigation_type_compensation_works_with_DTOs();

            AssertSql(
                @"SELECT [t0].[SquadId]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gear] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_list_initializers()
        {
            base.Optional_navigation_type_compensation_works_with_list_initializers();

            AssertSql(
                @"SELECT [t0].[SquadId], [t0].[SquadId] + 1
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gear] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_array_initializers()
        {
            base.Optional_navigation_type_compensation_works_with_array_initializers();

            AssertSql(
                @"SELECT [t0].[SquadId]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gear] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_orderby()
        {
            base.Optional_navigation_type_compensation_works_with_orderby();

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gear] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL
ORDER BY [t0].[SquadId]",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_groupby()
        {
            base.Optional_navigation_type_compensation_works_with_groupby();

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t0].[SquadId]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gear] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL
ORDER BY [t0].[SquadId]",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_all()
        {
            base.Optional_navigation_type_compensation_works_with_all();

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [CogTag] AS [t]
        LEFT JOIN (
            SELECT [t.Gear].*
            FROM [Gear] AS [t.Gear]
            WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
        ) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
        WHERE (([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL) AND ([t0].[HasSoulPatch] = 0))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_contains()
        {
            base.Optional_navigation_type_compensation_works_with_contains();

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gear] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE (([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL) AND [t0].[SquadId] IN (
    SELECT [g].[SquadId]
    FROM [Gear] AS [g]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
)",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_skip()
        {
            base.Optional_navigation_type_compensation_works_with_skip();

            AssertSql(
                @"SELECT [t0].[SquadId]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gear] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_take()
        {
            base.Optional_navigation_type_compensation_works_with_take();

            AssertSql(
                @"SELECT [t0].[SquadId]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [t.Gear].*
    FROM [Gear] AS [t.Gear]
    WHERE [t.Gear].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL",
                Sql);
        }

        public override void Select_correlated_filtered_collection()
        {
            base.Select_correlated_filtered_collection();

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND [g].[CityOrBirthName] IN (N'Ephyra', N'Hanover')

@_outer_FullName: Augustus Cole (Size = 450)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE (([w].[Name] <> N'Lancer') OR [w].[Name] IS NULL) AND (@_outer_FullName = [w].[OwnerFullName])

@_outer_FullName: Dominic Santiago (Size = 450)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE (([w].[Name] <> N'Lancer') OR [w].[Name] IS NULL) AND (@_outer_FullName = [w].[OwnerFullName])",
                Sql);
        }

        public override void Select_correlated_filtered_collection_with_composite_key()
        {
            base.Select_correlated_filtered_collection_with_composite_key();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] = N'Officer'

@_outer_Nickname: Baird (Size = 4000)
@_outer_SquadId: 1

SELECT [r].[Nickname], [r].[SquadId], [r].[AssignedCityName], [r].[CityOrBirthName], [r].[Discriminator], [r].[FullName], [r].[HasSoulPatch], [r].[LeaderNickname], [r].[LeaderSquadId], [r].[Rank]
FROM [Gear] AS [r]
WHERE ([r].[Discriminator] IN (N'Officer', N'Gear') AND ([r].[Nickname] <> N'Dom')) AND ((@_outer_Nickname = [r].[LeaderNickname]) AND (@_outer_SquadId = [r].[LeaderSquadId]))

@_outer_Nickname: Marcus (Size = 4000)
@_outer_SquadId: 1

SELECT [r].[Nickname], [r].[SquadId], [r].[AssignedCityName], [r].[CityOrBirthName], [r].[Discriminator], [r].[FullName], [r].[HasSoulPatch], [r].[LeaderNickname], [r].[LeaderSquadId], [r].[Rank]
FROM [Gear] AS [r]
WHERE ([r].[Discriminator] IN (N'Officer', N'Gear') AND ([r].[Nickname] <> N'Dom')) AND ((@_outer_Nickname = [r].[LeaderNickname]) AND (@_outer_SquadId = [r].[LeaderSquadId]))",
                Sql);
        }

        public override void Select_correlated_filtered_collection_works_with_caching()
        {
            base.Select_correlated_filtered_collection_works_with_caching();

            Assert.Contains(
                @"SELECT [t].[GearNickName]
FROM [CogTag] AS [t]",
                Sql);

            Assert.Contains(
                @"@_outer_GearNickName: Cole Train (Size = 450)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] = @_outer_GearNickName)",
                Sql);

            Assert.Contains(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND [g].[Nickname] IS NULL",
                Sql);
        }

        public override void Join_predicate_value_equals_condition()
        {
            base.Join_predicate_value_equals_condition();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
INNER JOIN [Weapon] AS [w] ON [w].[SynergyWithId] IS NOT NULL
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')",
                Sql);
        }

        public override void Join_predicate_value()
        {
            base.Join_predicate_value();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
INNER JOIN [Weapon] AS [w] ON [g].[HasSoulPatch] = 1
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')",
                Sql);
        }

        public override void Join_predicate_condition_equals_condition()
        {
            base.Join_predicate_condition_equals_condition();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
INNER JOIN [Weapon] AS [w] ON CASE
    WHEN [g].[FullName] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN [w].[SynergyWithId] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')",
                Sql);
        }

        public override void Left_join_predicate_value_equals_condition()
        {
            base.Left_join_predicate_value_equals_condition();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
LEFT JOIN [Weapon] AS [w] ON [w].[SynergyWithId] IS NOT NULL
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')",
                Sql);
        }

        public override void Left_join_predicate_value()
        {
            base.Left_join_predicate_value();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
LEFT JOIN [Weapon] AS [w] ON [g].[HasSoulPatch] = 1
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')",
                Sql);
        }

        public override void Left_join_predicate_condition_equals_condition()
        {
            base.Left_join_predicate_condition_equals_condition();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
LEFT JOIN [Weapon] AS [w] ON CASE
    WHEN [g].[FullName] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN [w].[SynergyWithId] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')",
                Sql);
        }

        public override void DateTimeOffset_Date_works()
        {
            base.DateTimeOffset_Date_works();

            AssertSql(
                @"@__Date_0: 01/01/0001 00:00:00

SELECT [m].[Id], [m].[CodeName], [m].[Timeline]
FROM [Mission] AS [m]
WHERE CONVERT(date, [m].[Timeline]) > @__Date_0",
                Sql);
        }

        public override void DateTimeOffset_Datepart_works()
        {
            base.DateTimeOffset_Datepart_works();

            AssertSql(
                @"SELECT [m].[Id], [m].[CodeName], [m].[Timeline]
FROM [Mission] AS [m]
WHERE DATEPART(month, [m].[Timeline]) = 5",
                Sql);
        }

        public override void Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used()
        {
            base.Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used();

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOrBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname]
ORDER BY [t].[GearNickName]",
                Sql);
        }

        public override void Complex_predicate_with_AndAlso_and_nullable_bool_property()
        {
            base.Complex_predicate_with_AndAlso_and_nullable_bool_property();

            AssertSql(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
LEFT JOIN (
    SELECT [w.Owner].*
    FROM [Gear] AS [w.Owner]
    WHERE [w.Owner].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [w].[OwnerFullName] = [t].[FullName]
WHERE ([w].[Id] <> 50) AND ([t].[HasSoulPatch] = 0)",
                Sql);
        }

        public override void Distinct_with_optional_navigation_is_translated_to_sql()
        {
            base.Distinct_with_optional_navigation_is_translated_to_sql();

            AssertSql(
                @"SELECT DISTINCT [g].[HasSoulPatch]
FROM [Gear] AS [g]
LEFT JOIN [CogTag] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g.Tag].[Note] <> N'Foo') OR [g.Tag].[Note] IS NULL)",
                Sql);
        }

        public override void Sum_with_optional_navigation_is_translated_to_sql()
        {
            base.Sum_with_optional_navigation_is_translated_to_sql();

            AssertSql(
                @"SELECT SUM([g].[SquadId])
FROM [Gear] AS [g]
LEFT JOIN [CogTag] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g.Tag].[Note] <> N'Foo') OR [g.Tag].[Note] IS NULL)",
                Sql);
        }

        public override void Count_with_optional_navigation_is_translated_to_sql()
        {
            base.Count_with_optional_navigation_is_translated_to_sql();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Gear] AS [g]
LEFT JOIN [CogTag] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g.Tag].[Note] <> N'Foo') OR [g.Tag].[Note] IS NULL)",
                Sql);
        }

        public override void Count_with_unflattened_groupjoin_is_evaluated_on_client()
        {
            base.Count_with_unflattened_groupjoin_is_evaluated_on_client();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [Gear] AS [g]
LEFT JOIN [CogTag] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname], [g].[SquadId]",
                Sql);
        }

        public override void Distinct_with_unflattened_groupjoin_is_evaluated_on_client()
        {
            base.Distinct_with_unflattened_groupjoin_is_evaluated_on_client();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note]
FROM [Gear] AS [g]
LEFT JOIN [CogTag] AS [t] ON ([g].[Nickname] = [t].[GearNickName]) AND ([g].[SquadId] = [t].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[Nickname], [g].[SquadId]",
                Sql);
        }

        public override void FirstOrDefault_with_manually_created_groupjoin_is_translated_to_sql()
        {
            base.FirstOrDefault_with_manually_created_groupjoin_is_translated_to_sql();

            AssertSql(
                @"SELECT TOP(1) [s].[Id], [s].[InternalNumber], [s].[Name]
FROM [Squad] AS [s]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [t] ON [s].[Id] = [t].[SquadId]
WHERE [s].[Name] = N'Kilo'",
                Sql);
        }

        public override void Any_with_optional_navigation_as_subquery_predicate_is_translated_to_sql()
        {
            base.Any_with_optional_navigation_as_subquery_predicate_is_translated_to_sql();

            AssertSql(
                @"SELECT [s].[Name]
FROM [Squad] AS [s]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Gear] AS [m]
    LEFT JOIN [CogTag] AS [m.Tag] ON ([m].[Nickname] = [m.Tag].[GearNickName]) AND ([m].[SquadId] = [m.Tag].[GearSquadId])
    WHERE ([m].[Discriminator] IN (N'Officer', N'Gear') AND ([m.Tag].[Note] = N'Dom''s Tag')) AND ([s].[Id] = [m].[SquadId]))",
                Sql);
        }

        public override void All_with_optional_navigation_is_translated_to_sql()
        {
            base.All_with_optional_navigation_is_translated_to_sql();

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Gear] AS [g]
        LEFT JOIN [CogTag] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
        WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g.Tag].[Note] = N'Foo'))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void Non_flattened_GroupJoin_with_result_operator_evaluates_on_the_client()
        {
            base.Non_flattened_GroupJoin_with_result_operator_evaluates_on_the_client();

            AssertSql(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOrBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [t0] ON ([t].[GearNickName] = [t0].[Nickname]) AND ([t].[GearSquadId] = [t0].[SquadId])
ORDER BY [t].[GearNickName], [t].[GearSquadId]",
                Sql);
        }

        public override void Client_side_equality_with_parameter_works_with_optional_navigations()
        {
            base.Client_side_equality_with_parameter_works_with_optional_navigations();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Tag].[Note]
FROM [Gear] AS [g]
LEFT JOIN [CogTag] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')",
                Sql);
        }

        public override void Contains_with_local_nullable_guid_list_closure()
        {
            base.Contains_with_local_nullable_guid_list_closure();

            Assert.Equal(
                @"SELECT [e].[Id], [e].[GearNickName], [e].[GearSquadId], [e].[Note]
FROM [CogTag] AS [e]
WHERE [e].[Id] IN ('d2c26679-562b-44d1-ab96-23d1775e0926', '23cbcf9b-ce14-45cf-aafa-2c2667ebfdd3', 'ab1b82d7-88db-42bd-a132-7eef9aa68af4')",
                Sql);
        }

        public override void Unnecessary_include_doesnt_get_added_complex_when_projecting_EF_Property()
        {
            base.Unnecessary_include_doesnt_get_added_complex_when_projecting_EF_Property();

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 1)
ORDER BY [g].[Rank]",
                Sql);
        }

        public override void Order_by_is_properly_lifted_from_subquery_created_by_include()
        {
            base.Order_by_is_properly_lifted_from_subquery_created_by_include();

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 0)
ORDER BY [g].[FullName], [g].[Rank]",
                Sql);
        }

        public override void Order_by_then_by_is_properly_lifted_from_subquery_created_by_include()
        {
            base.Order_by_then_by_is_properly_lifted_from_subquery_created_by_include();

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 0)
ORDER BY [g].[FullName], [g].[Rank], [g].[Nickname] DESC",
                Sql);
        }

        public override void Multiple_order_bys_are_properly_lifted_from_subquery_created_by_include()
        {
            base.Multiple_order_bys_are_properly_lifted_from_subquery_created_by_include();

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 0)
ORDER BY [g].[FullName], [g].[Nickname] DESC, [g].[Rank]",
                Sql);
        }

        public override void Order_by_is_properly_lifted_from_subquery_with_same_order_by_in_the_outer_query()
        {
            base.Order_by_is_properly_lifted_from_subquery_with_same_order_by_in_the_outer_query();

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 0)
ORDER BY [g].[FullName]",
                Sql);
        }

        public override void Where_is_properly_lifted_from_subquery_created_by_include()
        {
            base.Where_is_properly_lifted_from_subquery_created_by_include();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Tag].[Id], [g.Tag].[GearNickName], [g.Tag].[GearSquadId], [g.Tag].[Note]
FROM [Gear] AS [g]
LEFT JOIN [CogTag] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE ([g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[FullName] <> N'Augustus Cole')) AND ([g].[HasSoulPatch] = 0)
ORDER BY [g].[FullName]",
                Sql);
        }

        public override void Where_and_order_by_are_properly_lifted_from_subquery_created_by_tracking()
        {
            base.Where_and_order_by_are_properly_lifted_from_subquery_created_by_tracking();

            AssertSql(
                @"SELECT [g].[FullName]
FROM [Gear] AS [g]
WHERE ([g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[FullName] <> N'Augustus Cole')) AND ([g].[HasSoulPatch] = 0)
ORDER BY [g].[FullName], [g].[Rank]",
                Sql);
        }

        public override void Subquery_is_lifted_from_main_from_clause_of_SelectMany()
        {
            base.Subquery_is_lifted_from_main_from_clause_of_SelectMany();

            AssertSql(
                @"SELECT [g].[FullName], [g2].[FullName]
FROM [Gear] AS [g]
CROSS JOIN [Gear] AS [g2]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[HasSoulPatch] = 1) AND ([g2].[HasSoulPatch] = 0))
ORDER BY [g].[FullName], [g].[Rank]",
                Sql);
        }

        public override void Subquery_containing_SelectMany_projecting_main_from_clause_gets_lifted()
        {
            base.Subquery_containing_SelectMany_projecting_main_from_clause_gets_lifted();

            AssertSql(
                @"SELECT [gear].[FullName]
FROM [Gear] AS [gear]
CROSS JOIN [CogTag] AS [tag]
WHERE [gear].[Discriminator] IN (N'Officer', N'Gear') AND ([gear].[HasSoulPatch] = 1)
ORDER BY [gear].[FullName], [tag].[Note]",
                Sql);
        }

        public override void Subquery_containing_join_projecting_main_from_clause_gets_lifted()
        {
            base.Subquery_containing_join_projecting_main_from_clause_gets_lifted();

            AssertSql(
                @"SELECT [gear].[Nickname]
FROM [Gear] AS [gear]
INNER JOIN [CogTag] AS [tag] ON [gear].[Nickname] = [tag].[GearNickName]
WHERE [gear].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [gear].[Nickname], [tag].[Note]",
                Sql);
        }

        public override void Subquery_containing_left_join_projecting_main_from_clause_gets_lifted()
        {
            base.Subquery_containing_left_join_projecting_main_from_clause_gets_lifted();

            AssertSql(
                @"SELECT [gear].[Nickname]
FROM [Gear] AS [gear]
LEFT JOIN [CogTag] AS [tag] ON [gear].[Nickname] = [tag].[GearNickName]
WHERE [gear].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [gear].[Nickname], [gear].[Rank]",
                Sql);
        }

        public override void Subquery_containing_join_gets_lifted_clashing_names()
        {
            base.Subquery_containing_join_gets_lifted_clashing_names();

            AssertSql(
                @"SELECT [gear].[Nickname]
FROM [Gear] AS [gear]
INNER JOIN [CogTag] AS [tag] ON [gear].[Nickname] = [tag].[GearNickName]
INNER JOIN [CogTag] AS [tag0] ON [gear].[Nickname] = [tag0].[GearNickName]
WHERE [gear].[Discriminator] IN (N'Officer', N'Gear') AND (([tag].[GearNickName] <> N'Cole Train') OR [tag].[GearNickName] IS NULL)
ORDER BY [gear].[Nickname], [tag0].[Id], [tag].[Note]",
                Sql);
        }

        public override void Subquery_created_by_include_gets_lifted_nested()
        {
            base.Subquery_created_by_include_gets_lifted_nested();

            AssertSql(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.CityOfBirth].[Name], [g.CityOfBirth].[Location]
FROM [Gear] AS [g]
INNER JOIN [City] AS [g.CityOfBirth] ON [g].[CityOrBirthName] = [g.CityOfBirth].[Name]
WHERE ([g].[Discriminator] IN (N'Officer', N'Gear') AND EXISTS (
    SELECT 1
    FROM [Weapon] AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName])) AND ([g].[HasSoulPatch] = 0)
ORDER BY [g].[Nickname], [g].[Rank]",
                Sql);
        }

        public override void Subquery_is_not_lifted_from_additional_from_clause()
        {
            base.Subquery_is_not_lifted_from_additional_from_clause();

            AssertSql(
                @"SELECT [g1].[FullName]
FROM [Gear] AS [g1]
WHERE [g1].[Discriminator] IN (N'Officer', N'Gear') AND ([g1].[HasSoulPatch] = 1)
ORDER BY [g1].[FullName]

SELECT [g0].[HasSoulPatch], [g0].[FullName]
FROM [Gear] AS [g0]
WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g0].[Rank]

SELECT [g0].[HasSoulPatch], [g0].[FullName]
FROM [Gear] AS [g0]
WHERE [g0].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g0].[Rank]",
                Sql);
        }

        public override void Subquery_with_result_operator_is_not_lifted()
        {
            base.Subquery_with_result_operator_is_not_lifted();

            AssertSql(
                @"@__p_0: 2

SELECT [t].[FullName]
FROM (
    SELECT TOP(@__p_0) [g].*
    FROM [Gear] AS [g]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[HasSoulPatch] = 0)
    ORDER BY [g].[FullName]
) AS [t]
ORDER BY [t].[Rank], [t].[FullName]",
                Sql);
        }

        protected override void ClearLog() => TestSqlLoggerFactory.Reset();

        private const string FileLineEnding = @"
";

        private static string Sql => TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);

        private void AssertSql(string expected, string actual)
        {
            TestHelpers.AssertBaseline(expected, actual, _testOutputHelper);
        }
    }
}
