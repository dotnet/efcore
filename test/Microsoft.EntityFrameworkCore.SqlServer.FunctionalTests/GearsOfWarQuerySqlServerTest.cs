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
        public GearsOfWarQuerySqlServerTest(GearsOfWarQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        public override void Entity_equality_empty()
        {
            base.Entity_equality_empty();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] IS NULL AND ([g].[SquadId] = 0))",
                Sql);
        }

        public override void Include_multiple_one_to_one_and_one_to_many()
        {
            base.Include_multiple_one_to_one_and_one_to_many();

            Assert.Equal(
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

            Assert.Equal(
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

            Assert.Equal(
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

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [s].[Id], [s].[InternalNumber], [s].[Name]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
LEFT JOIN [Squad] AS [s] ON [g].[SquadId] = [s].[Id]",
                Sql);
        }

        public override void Include_multiple_circular()
        {
            base.Include_multiple_circular();

            Assert.Equal(
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

            Assert.Equal(
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

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] = N'Marcus')
ORDER BY [g].[FullName]

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE EXISTS (
    SELECT 1
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Nickname] = N'Marcus')) AND ([w].[OwnerFullName] = [g].[FullName]))
ORDER BY [w].[OwnerFullName]",
                Sql);
        }

        public override void Include_multiple_include_then_include()
        {
            base.Include_multiple_include_then_include();

            Assert.Equal(
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

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId]

SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOrBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
FROM [Gear] AS [g0]
WHERE [g0].[Discriminator] IN (N'Officer', N'Gear') AND EXISTS (
    SELECT 1
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = N'Officer') AND (([g0].[LeaderNickname] = [g].[Nickname]) AND ([g0].[LeaderSquadId] = [g].[SquadId])))
ORDER BY [g0].[LeaderNickname], [g0].[LeaderSquadId]",
                Sql);
        }

        public override void Select_Where_Navigation_Included()
        {
            base.Select_Where_Navigation_Included();

            Assert.Equal(
                @"SELECT [o].[Id], [o].[GearNickName], [o].[GearSquadId], [o].[Note], [o.Gear].[Nickname], [o.Gear].[SquadId], [o.Gear].[AssignedCityName], [o.Gear].[CityOrBirthName], [o.Gear].[Discriminator], [o.Gear].[FullName], [o.Gear].[HasSoulPatch], [o.Gear].[LeaderNickname], [o.Gear].[LeaderSquadId], [o.Gear].[Rank], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [CogTag] AS [o]
LEFT JOIN [Gear] AS [o.Gear] ON ([o].[GearNickName] = [o.Gear].[Nickname]) AND ([o].[GearSquadId] = [o.Gear].[SquadId])
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [g] ON ([o].[GearNickName] = [g].[Nickname]) AND ([o].[GearSquadId] = [g].[SquadId])
WHERE [o.Gear].[Nickname] = N'Marcus'
ORDER BY [o].[GearNickName], [o].[GearSquadId]",
                Sql);
        }

        public override void Include_with_join_reference1()
        {
            base.Include_with_join_reference1();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location]
FROM [Gear] AS [g]
INNER JOIN [CogTag] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')",
                Sql);
        }

        public override void Include_with_join_reference2()
        {
            base.Include_with_join_reference2();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location]
FROM [CogTag] AS [t]
INNER JOIN [Gear] AS [g] ON ([t].[GearSquadId] = [g].[SquadId]) AND ([t].[GearNickName] = [g].[Nickname])
INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]",
                Sql);
        }

        public override void Include_with_join_collection1()
        {
            base.Include_with_join_collection1();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
INNER JOIN [CogTag] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[FullName]

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE EXISTS (
    SELECT 1
    FROM [Gear] AS [g]
    INNER JOIN [CogTag] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([w].[OwnerFullName] = [g].[FullName]))
ORDER BY [w].[OwnerFullName]",
                Sql);
        }

        public override void Include_with_join_collection2()
        {
            base.Include_with_join_collection2();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [CogTag] AS [t]
INNER JOIN [Gear] AS [g] ON ([t].[GearSquadId] = [g].[SquadId]) AND ([t].[GearNickName] = [g].[Nickname])
ORDER BY [g].[FullName]

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE EXISTS (
    SELECT 1
    FROM [CogTag] AS [t]
    INNER JOIN [Gear] AS [g] ON ([t].[GearSquadId] = [g].[SquadId]) AND ([t].[GearNickName] = [g].[Nickname])
    WHERE [w].[OwnerFullName] = [g].[FullName])
ORDER BY [w].[OwnerFullName]",
                Sql);
        }

        public override void Include_where_list_contains_navigation()
        {
            base.Include_where_list_contains_navigation();

            Assert.Equal(
                @"SELECT [t].[Id]
FROM [CogTag] AS [t]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Tag].[Id], [g.Tag].[GearNickName], [g.Tag].[GearSquadId], [g.Tag].[Note], [c].[Id], [c].[GearNickName], [c].[GearSquadId], [c].[Note]
FROM [Gear] AS [g]
LEFT JOIN [CogTag] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
LEFT JOIN [CogTag] AS [c] ON ([c].[GearNickName] = [g].[Nickname]) AND ([c].[GearSquadId] = [g].[SquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND [g.Tag].[Id] IS NOT NULL
ORDER BY [g].[Nickname], [g].[SquadId]",
                Sql);
        }

        public override void Include_where_list_contains_navigation2()
        {
            base.Include_where_list_contains_navigation2();

            Assert.Equal(
                @"SELECT [t].[Id]
FROM [CogTag] AS [t]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Tag].[Id], [g.Tag].[GearNickName], [g.Tag].[GearSquadId], [g.Tag].[Note], [c].[Id], [c].[GearNickName], [c].[GearSquadId], [c].[Note]
FROM [Gear] AS [g]
LEFT JOIN [CogTag] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
INNER JOIN [City] AS [g.CityOfBirth] ON [g].[CityOrBirthName] = [g.CityOfBirth].[Name]
LEFT JOIN [CogTag] AS [c] ON ([c].[GearNickName] = [g].[Nickname]) AND ([c].[GearSquadId] = [g].[SquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND [g.CityOfBirth].[Location] IS NOT NULL
ORDER BY [g].[Nickname], [g].[SquadId]",
                Sql);
        }

        public override void Navigation_accessed_twice_outside_and_inside_subquery()
        {
            base.Navigation_accessed_twice_outside_and_inside_subquery();

            Assert.Equal(
                @"SELECT [t].[Id]
FROM [CogTag] AS [t]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Tag].[Id], [g.Tag].[GearNickName], [g.Tag].[GearSquadId], [g.Tag].[Note]
FROM [Gear] AS [g]
LEFT JOIN [CogTag] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND [g.Tag].[Id] IS NOT NULL
ORDER BY [g].[Nickname], [g].[SquadId]",
                Sql);
        }

        public override void Include_with_join_multi_level()
        {
            base.Include_with_join_multi_level();

            Assert.Equal(
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

            Assert.Equal(
                @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOrBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [c].[Name], [c].[Location]
FROM [CogTag] AS [t]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOrBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
    FROM [Gear] AS [g0]
    WHERE [g0].[Discriminator] = N'Officer'
) AS [t0] ON ([t].[GearSquadId] = [t0].[SquadId]) AND ([t].[GearNickName] = [t0].[Nickname])
INNER JOIN [City] AS [c] ON [t0].[CityOrBirthName] = [c].[Name]",
                Sql);
        }

        public override void Include_with_join_and_inheritance2()
        {
            base.Include_with_join_and_inheritance2();

            Assert.Equal(
                @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOrBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
    FROM [Gear] AS [g0]
    WHERE [g0].[Discriminator] = N'Officer'
) AS [t]
INNER JOIN [CogTag] AS [t0] ON ([t].[SquadId] = [t0].[GearSquadId]) AND ([t].[Nickname] = [t0].[GearNickName])
ORDER BY [t].[FullName]

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOrBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
        FROM [Gear] AS [g0]
        WHERE [g0].[Discriminator] = N'Officer'
    ) AS [t]
    INNER JOIN [CogTag] AS [t0] ON ([t].[SquadId] = [t0].[GearSquadId]) AND ([t].[Nickname] = [t0].[GearNickName])
    WHERE [w].[OwnerFullName] = [t].[FullName])
ORDER BY [w].[OwnerFullName]",
                Sql);
        }

        public override void Include_with_join_and_inheritance3()
        {
            base.Include_with_join_and_inheritance3();

            Assert.Equal(
                @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOrBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank]
FROM [CogTag] AS [t]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOrBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
    FROM [Gear] AS [g0]
    WHERE [g0].[Discriminator] = N'Officer'
) AS [t0] ON ([t].[GearSquadId] = [t0].[SquadId]) AND ([t].[GearNickName] = [t0].[Nickname])
ORDER BY [t0].[Nickname], [t0].[SquadId]

SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOrBirthName], [g1].[Discriminator], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[Rank]
FROM [Gear] AS [g1]
WHERE [g1].[Discriminator] IN (N'Officer', N'Gear') AND EXISTS (
    SELECT 1
    FROM [CogTag] AS [t]
    INNER JOIN (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOrBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
        FROM [Gear] AS [g0]
        WHERE [g0].[Discriminator] = N'Officer'
    ) AS [t0] ON ([t].[GearSquadId] = [t0].[SquadId]) AND ([t].[GearNickName] = [t0].[Nickname])
    WHERE ([g1].[LeaderNickname] = [t0].[Nickname]) AND ([g1].[LeaderSquadId] = [t0].[SquadId]))
ORDER BY [g1].[LeaderNickname], [g1].[LeaderSquadId]",
                Sql);
        }

        public override void Include_with_nested_navigation_in_order_by()
        {
            base.Include_with_nested_navigation_in_order_by();

            Assert.Equal(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [w.Owner].[Nickname], [w.Owner].[SquadId], [w.Owner].[AssignedCityName], [w.Owner].[CityOrBirthName], [w.Owner].[Discriminator], [w.Owner].[FullName], [w.Owner].[HasSoulPatch], [w.Owner].[LeaderNickname], [w.Owner].[LeaderSquadId], [w.Owner].[Rank], [w.Owner.CityOfBirth].[Name], [w.Owner.CityOfBirth].[Location], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Weapon] AS [w]
LEFT JOIN [Gear] AS [w.Owner] ON [w].[OwnerFullName] = [w.Owner].[FullName]
LEFT JOIN [City] AS [w.Owner.CityOfBirth] ON [w.Owner].[CityOrBirthName] = [w.Owner.CityOfBirth].[Name]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [g] ON [w].[OwnerFullName] = [g].[FullName]
ORDER BY [w.Owner.CityOfBirth].[Name], [w].[OwnerFullName], [w.Owner].[CityOrBirthName]",
                Sql);
        }

        public override void Where_enum()
        {
            base.Where_enum();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[Rank] = 2)",
                Sql);
        }

        public override void Where_nullable_enum_with_constant()
        {
            base.Where_nullable_enum_with_constant();

            Assert.Equal(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE [w].[AmmunitionType] = 1",
                Sql);
        }

        public override void Where_nullable_enum_with_null_constant()
        {
            base.Where_nullable_enum_with_null_constant();

            Assert.Equal(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE [w].[AmmunitionType] IS NULL",
                Sql);
        }

        public override void Where_nullable_enum_with_non_nullable_parameter()
        {
            base.Where_nullable_enum_with_non_nullable_parameter();

            Assert.Equal(
                @"@__ammunitionType_0: Cartridge

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE [w].[AmmunitionType] = @__ammunitionType_0",
                Sql);
        }

        public override void Where_nullable_enum_with_nullable_parameter()
        {
            base.Where_nullable_enum_with_nullable_parameter();

            Assert.Equal(
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

            Assert.Equal(
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

            Assert.Equal(
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

            Assert.Equal(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE ([w].[AmmunitionType] & 1) > 0",
                Sql);
        }

        [ConditionalFact]
        public override void Where_bitwise_and_nullable_enum_with_null_constant()
        {
            base.Where_bitwise_and_nullable_enum_with_null_constant();

            Assert.Equal(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE ([w].[AmmunitionType] & NULL) > 0",
                Sql);
        }

        [ConditionalFact]
        public override void Where_bitwise_and_nullable_enum_with_non_nullable_parameter()
        {
            base.Where_bitwise_and_nullable_enum_with_non_nullable_parameter();

            Assert.Equal(
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

            Assert.Equal(
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

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] | 1) > 0)",
                Sql);
        }

        [ConditionalFact]
        public override void Bitwise_projects_values_in_select()
        {
            base.Bitwise_projects_values_in_select();

            Assert.Equal(
                @"SELECT TOP(1) CASE
    WHEN ([g].[Rank] & 1) = 1
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END, CASE
    WHEN ([g].[Rank] & 1) = 2
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END, [g].[Rank] & 1
FROM [Gear] AS [g]
WHERE (([g].[Discriminator] = N'Officer') OR ([g].[Discriminator] = N'Gear')) AND (([g].[Rank] & 1) = 1)",
                Sql);
        }

        public override void Where_enum_has_flag()
        {
            base.Where_enum_has_flag();

            Assert.Equal(
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

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & (
    SELECT TOP(1) [x].[Rank]
    FROM [Gear] AS [x]
    WHERE ([x].[Discriminator] = N'Officer') OR ([x].[Discriminator] = N'Gear')
    ORDER BY [x].[Nickname], [x].[SquadId]
)) = (
    SELECT TOP(1) [x].[Rank]
    FROM [Gear] AS [x]
    WHERE ([x].[Discriminator] = N'Officer') OR ([x].[Discriminator] = N'Gear')
    ORDER BY [x].[Nickname], [x].[SquadId]
))

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ((1 & (
    SELECT TOP(1) [x].[Rank]
    FROM [Gear] AS [x]
    WHERE ([x].[Discriminator] = N'Officer') OR ([x].[Discriminator] = N'Gear')
    ORDER BY [x].[Nickname], [x].[SquadId]
)) = (
    SELECT TOP(1) [x].[Rank]
    FROM [Gear] AS [x]
    WHERE ([x].[Discriminator] = N'Officer') OR ([x].[Discriminator] = N'Gear')
    ORDER BY [x].[Nickname], [x].[SquadId]
))",
                Sql);
        }

        public override void Where_enum_has_flag_with_non_nullable_parameter()
        {
            base.Where_enum_has_flag_with_non_nullable_parameter();

            Assert.Equal(
                @"@__parameter_0: Corporal

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & @__parameter_0) = @__parameter_0)",
                Sql);
        }

        public override void Where_has_flag_with_nullable_parameter()
        {
            base.Where_has_flag_with_nullable_parameter();

            Assert.Equal(
                @"@__parameter_0: Corporal (Nullable = true)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Rank] & @__parameter_0) = @__parameter_0)",
                Sql);
        }

        public override void Select_enum_has_flag()
        {
            base.Select_enum_has_flag();

            Assert.Equal(
                @"SELECT TOP(1) CASE
    WHEN ([g].[Rank] & 1) = 1
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END, CASE
    WHEN ([g].[Rank] & 2) = 2
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
FROM [Gear] AS [g]
WHERE (([g].[Discriminator] = N'Officer') OR ([g].[Discriminator] = N'Gear')) AND (([g].[Rank] & 1) = 1)",
                Sql);
        }

        public override void Where_count_subquery_without_collision()
        {
            base.Where_count_subquery_without_collision();

            Assert.Equal(
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

            Assert.Equal(
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

            Assert.Equal(
                @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = 1
    THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT)
END
FROM [Weapon] AS [w]
WHERE [w].[IsAutomatic] = 1",
                Sql);
        }

        public override void Select_comparison_with_null()
        {
            base.Select_comparison_with_null();

            Assert.Equal(
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

            Assert.Equal(
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

            Assert.Equal(
                @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = 0
    THEN 1 ELSE 0
END
FROM [Weapon] AS [w]",
                Sql);
        }

        public override void Select_ternary_operation_with_has_value_not_null()
        {
            // TODO: Optimize this query (See #4267)
            base.Select_ternary_operation_with_has_value_not_null();

            Assert.Equal(
                @"SELECT [w].[Id], CASE
    WHEN [w].[AmmunitionType] IS NOT NULL AND (([w].[AmmunitionType] = 1) AND [w].[AmmunitionType] IS NOT NULL)
    THEN N'Yes' ELSE N'No'
END
FROM [Weapon] AS [w]
WHERE [w].[AmmunitionType] IS NOT NULL AND (([w].[AmmunitionType] = 1) AND [w].[AmmunitionType] IS NOT NULL)",
                Sql);
        }

        public override void Select_ternary_operation_multiple_conditions()
        {
            base.Select_ternary_operation_multiple_conditions();

            Assert.Equal(
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

            Assert.Equal(
                @"SELECT [w].[Id], CASE
    WHEN ([w].[IsAutomatic] = 0) AND (([w].[SynergyWithId] = 1) AND [w].[SynergyWithId] IS NOT NULL)
    THEN N'Yes' ELSE N'No'
END
FROM [Weapon] AS [w]",
                Sql);
        }

        public override void Select_multiple_conditions()
        {
            base.Select_multiple_conditions();

            Assert.Equal(
                @"SELECT [w].[Id], CASE
    WHEN ([w].[IsAutomatic] = 0) AND (([w].[SynergyWithId] = 1) AND [w].[SynergyWithId] IS NOT NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
FROM [Weapon] AS [w]",
                Sql);
        }

        public override void Select_nested_ternary_operations()
        {
            base.Select_nested_ternary_operations();

            Assert.Equal(
                @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = 0
    THEN CASE
        WHEN ([w].[AmmunitionType] = 1) AND [w].[AmmunitionType] IS NOT NULL
        THEN N'ManualCartridge' ELSE N'Manual'
    END ELSE N'Auto'
END
FROM [Weapon] AS [w]",
                Sql);
        }

        public override void Null_propagation_optimization1()
        {
            base.Null_propagation_optimization1();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND ([g].[LeaderNickname] = N'Marcus')",
                Sql);
        }

        public override void Null_propagation_optimization2()
        {
            base.Null_propagation_optimization2();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (RIGHT([g].[LeaderNickname], LEN(N'us')) = N'us')",
                Sql);
        }

        public override void Null_propagation_optimization3()
        {
            base.Null_propagation_optimization3();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (RIGHT([g].[LeaderNickname], LEN(N'us')) = N'us')",
                Sql);
        }

        public override void Null_propagation_optimization4()
        {
            base.Null_propagation_optimization4();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (LEN([g].[LeaderNickname]) = 5)",
                Sql);
        }

        public override void Null_propagation_optimization5()
        {
            base.Null_propagation_optimization5();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (LEN([g].[LeaderNickname]) = 5)",
                Sql);
        }

        public override void Null_propagation_optimization6()
        {
            base.Null_propagation_optimization6();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (LEN([g].[LeaderNickname]) = 5)",
                Sql);
        }

        public override void Select_null_propagation_negative1()
        {
            base.Select_null_propagation_negative1();

            Assert.Equal(
                @"SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL
    THEN CASE
        WHEN LEN([g].[Nickname]) = 5
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END ELSE NULL
END
FROM [Gear] AS [g]
WHERE ([g].[Discriminator] = N'Officer') OR ([g].[Discriminator] = N'Gear')",
                Sql);
        }

        public override void Select_null_propagation_negative2()
        {
            base.Select_null_propagation_negative2();

            Assert.Equal(
                @"SELECT CASE
    WHEN [g1].[LeaderNickname] IS NOT NULL
    THEN [g2].[LeaderNickname] ELSE NULL
END
FROM [Gear] AS [g1]
CROSS JOIN [Gear] AS [g2]
WHERE ([g1].[Discriminator] = N'Officer') OR ([g1].[Discriminator] = N'Gear')",
                Sql);
        }

        public override void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar()
        {
            base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar();

            Assert.Equal(
                @"SELECT [ct1].[Id], [ct1].[GearNickName], [ct1].[GearSquadId], [ct1].[Note], [ct1.Gear].[Nickname], [ct1.Gear].[SquadId], [ct1.Gear].[AssignedCityName], [ct1.Gear].[CityOrBirthName], [ct1.Gear].[Discriminator], [ct1.Gear].[FullName], [ct1.Gear].[HasSoulPatch], [ct1.Gear].[LeaderNickname], [ct1.Gear].[LeaderSquadId], [ct1.Gear].[Rank], [ct2].[Id], [ct2].[GearNickName], [ct2].[GearSquadId], [ct2].[Note], [ct2.Gear].[Nickname], [ct2.Gear].[SquadId], [ct2.Gear].[AssignedCityName], [ct2.Gear].[CityOrBirthName], [ct2.Gear].[Discriminator], [ct2.Gear].[FullName], [ct2.Gear].[HasSoulPatch], [ct2.Gear].[LeaderNickname], [ct2.Gear].[LeaderSquadId], [ct2.Gear].[Rank]
FROM [CogTag] AS [ct1]
LEFT JOIN [Gear] AS [ct1.Gear] ON ([ct1].[GearNickName] = [ct1.Gear].[Nickname]) AND ([ct1].[GearSquadId] = [ct1.Gear].[SquadId])
CROSS JOIN [CogTag] AS [ct2]
LEFT JOIN [Gear] AS [ct2.Gear] ON ([ct2].[GearNickName] = [ct2.Gear].[Nickname]) AND ([ct2].[GearSquadId] = [ct2.Gear].[SquadId])
WHERE ([ct1.Gear].[Nickname] = [ct2.Gear].[Nickname]) OR ([ct1.Gear].[Nickname] IS NULL AND [ct2.Gear].[Nickname] IS NULL)
ORDER BY [ct1].[GearNickName], [ct1].[GearSquadId], [ct2].[GearNickName], [ct2].[GearSquadId]",
                Sql);
        }

        public override void Select_Singleton_Navigation_With_Member_Access()
        {
            base.Select_Singleton_Navigation_With_Member_Access();

            Assert.Equal(
                @"SELECT [ct].[Id], [ct].[GearNickName], [ct].[GearSquadId], [ct].[Note], [ct.Gear].[Nickname], [ct.Gear].[SquadId], [ct.Gear].[AssignedCityName], [ct.Gear].[CityOrBirthName], [ct.Gear].[Discriminator], [ct.Gear].[FullName], [ct.Gear].[HasSoulPatch], [ct.Gear].[LeaderNickname], [ct.Gear].[LeaderSquadId], [ct.Gear].[Rank]
FROM [CogTag] AS [ct]
LEFT JOIN [Gear] AS [ct.Gear] ON ([ct].[GearNickName] = [ct.Gear].[Nickname]) AND ([ct].[GearSquadId] = [ct.Gear].[SquadId])
WHERE (([ct.Gear].[Nickname] = N'Marcus') AND [ct.Gear].[Nickname] IS NOT NULL) AND (([ct.Gear].[CityOrBirthName] <> N'Ephyra') OR [ct.Gear].[CityOrBirthName] IS NULL)
ORDER BY [ct].[GearNickName], [ct].[GearSquadId]",
                Sql);
        }

        public override void Select_Where_Navigation()
        {
            base.Select_Where_Navigation();

            Assert.Equal(
                @"SELECT [ct].[Id], [ct].[GearNickName], [ct].[GearSquadId], [ct].[Note], [ct.Gear].[Nickname], [ct.Gear].[SquadId], [ct.Gear].[AssignedCityName], [ct.Gear].[CityOrBirthName], [ct.Gear].[Discriminator], [ct.Gear].[FullName], [ct.Gear].[HasSoulPatch], [ct.Gear].[LeaderNickname], [ct.Gear].[LeaderSquadId], [ct.Gear].[Rank]
FROM [CogTag] AS [ct]
LEFT JOIN [Gear] AS [ct.Gear] ON ([ct].[GearNickName] = [ct.Gear].[Nickname]) AND ([ct].[GearSquadId] = [ct.Gear].[SquadId])
WHERE [ct.Gear].[Nickname] = N'Marcus'
ORDER BY [ct].[GearNickName], [ct].[GearSquadId]",
                Sql);
        }

        public override void Select_Where_Navigation_Client()
        {
            base.Select_Where_Navigation_Client();

            Assert.Equal(
                @"SELECT [o].[Id], [o].[GearNickName], [o].[GearSquadId], [o].[Note], [o.Gear].[Nickname], [o.Gear].[SquadId], [o.Gear].[AssignedCityName], [o.Gear].[CityOrBirthName], [o.Gear].[Discriminator], [o.Gear].[FullName], [o.Gear].[HasSoulPatch], [o.Gear].[LeaderNickname], [o.Gear].[LeaderSquadId], [o.Gear].[Rank]
FROM [CogTag] AS [o]
LEFT JOIN [Gear] AS [o.Gear] ON ([o].[GearNickName] = [o.Gear].[Nickname]) AND ([o].[GearSquadId] = [o.Gear].[SquadId])
WHERE [o].[GearNickName] IS NOT NULL OR [o].[GearSquadId] IS NOT NULL
ORDER BY [o].[GearNickName], [o].[GearSquadId]",
                Sql);
        }

        public override void Select_Where_Navigation_Equals_Navigation()
        {
            base.Select_Where_Navigation_Equals_Navigation();

            Assert.StartsWith(
                @"SELECT [ct1].[Id], [ct1].[GearNickName], [ct1].[GearSquadId], [ct1].[Note], [ct1.Gear].[Nickname], [ct1.Gear].[SquadId], [ct1.Gear].[AssignedCityName], [ct1.Gear].[CityOrBirthName], [ct1.Gear].[Discriminator], [ct1.Gear].[FullName], [ct1.Gear].[HasSoulPatch], [ct1.Gear].[LeaderNickname], [ct1.Gear].[LeaderSquadId], [ct1.Gear].[Rank], [ct2].[Id], [ct2].[GearNickName], [ct2].[GearSquadId], [ct2].[Note], [ct2.Gear].[Nickname], [ct2.Gear].[SquadId], [ct2.Gear].[AssignedCityName], [ct2.Gear].[CityOrBirthName], [ct2.Gear].[Discriminator], [ct2.Gear].[FullName], [ct2.Gear].[HasSoulPatch], [ct2.Gear].[LeaderNickname], [ct2.Gear].[LeaderSquadId], [ct2.Gear].[Rank]
FROM [CogTag] AS [ct1]
LEFT JOIN [Gear] AS [ct1.Gear] ON ([ct1].[GearNickName] = [ct1.Gear].[Nickname]) AND ([ct1].[GearSquadId] = [ct1.Gear].[SquadId])
CROSS JOIN [CogTag] AS [ct2]
LEFT JOIN [Gear] AS [ct2.Gear] ON ([ct2].[GearNickName] = [ct2.Gear].[Nickname]) AND ([ct2].[GearSquadId] = [ct2.Gear].[SquadId])
WHERE (([ct1.Gear].[Nickname] = [ct2.Gear].[Nickname]) OR ([ct1.Gear].[Nickname] IS NULL AND [ct2.Gear].[Nickname] IS NULL)) AND (([ct1.Gear].[SquadId] = [ct2.Gear].[SquadId]) OR ([ct1.Gear].[SquadId] IS NULL AND [ct2.Gear].[SquadId] IS NULL))
ORDER BY [ct1].[GearNickName], [ct1].[GearSquadId], [ct2].[GearNickName], [ct2].[GearSquadId]",
                Sql);
        }

        public override void Select_Where_Navigation_Null()
        {
            base.Select_Where_Navigation_Null();

            Assert.Equal(
                @"SELECT [ct].[Id], [ct].[GearNickName], [ct].[GearSquadId], [ct].[Note]
FROM [CogTag] AS [ct]
WHERE [ct].[GearNickName] IS NULL AND [ct].[GearSquadId] IS NULL",
                Sql);
        }

        public override void Select_Where_Navigation_Null_Reverse()
        {
            base.Select_Where_Navigation_Null_Reverse();

            Assert.Equal(
                @"SELECT [ct].[Id], [ct].[GearNickName], [ct].[GearSquadId], [ct].[Note]
FROM [CogTag] AS [ct]
WHERE [ct].[GearNickName] IS NULL AND [ct].[GearSquadId] IS NULL",
                Sql);
        }

        public override void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected()
        {
            base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected();

            Assert.Equal(
                @"SELECT [ct1].[Id], [ct1].[GearNickName], [ct1].[GearSquadId], [ct1].[Note], [ct1.Gear].[Nickname], [ct1.Gear].[SquadId], [ct1.Gear].[AssignedCityName], [ct1.Gear].[CityOrBirthName], [ct1.Gear].[Discriminator], [ct1.Gear].[FullName], [ct1.Gear].[HasSoulPatch], [ct1.Gear].[LeaderNickname], [ct1.Gear].[LeaderSquadId], [ct1.Gear].[Rank], [ct2.Gear].[Nickname], [ct2.Gear].[SquadId], [ct2.Gear].[AssignedCityName], [ct2.Gear].[CityOrBirthName], [ct2.Gear].[Discriminator], [ct2.Gear].[FullName], [ct2.Gear].[HasSoulPatch], [ct2.Gear].[LeaderNickname], [ct2.Gear].[LeaderSquadId], [ct2.Gear].[Rank], [ct2].[Id]
FROM [CogTag] AS [ct1]
LEFT JOIN [Gear] AS [ct1.Gear] ON ([ct1].[GearNickName] = [ct1.Gear].[Nickname]) AND ([ct1].[GearSquadId] = [ct1.Gear].[SquadId])
CROSS JOIN [CogTag] AS [ct2]
LEFT JOIN [Gear] AS [ct2.Gear] ON ([ct2].[GearNickName] = [ct2.Gear].[Nickname]) AND ([ct2].[GearSquadId] = [ct2.Gear].[SquadId])
WHERE ([ct1.Gear].[Nickname] = [ct2.Gear].[Nickname]) OR ([ct1.Gear].[Nickname] IS NULL AND [ct2.Gear].[Nickname] IS NULL)
ORDER BY [ct1].[GearNickName], [ct1].[GearSquadId], [ct2].[GearNickName], [ct2].[GearSquadId]",
                Sql);
        }

        public override void Optional_Navigation_Null_Coalesce_To_Clr_Type()
        {
            base.Optional_Navigation_Null_Coalesce_To_Clr_Type();

            Assert.Equal(@"SELECT TOP(1) [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [w.SynergyWith].[Id], [w.SynergyWith].[AmmunitionType], [w.SynergyWith].[IsAutomatic], [w.SynergyWith].[Name], [w.SynergyWith].[OwnerFullName], [w.SynergyWith].[SynergyWithId]
FROM [Weapon] AS [w]
LEFT JOIN [Weapon] AS [w.SynergyWith] ON [w].[SynergyWithId] = [w.SynergyWith].[Id]
ORDER BY [w].[SynergyWithId]",
                Sql);
        }

        public override void Where_subquery_boolean()
        {
            base.Where_subquery_boolean();

            Assert.Equal(
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

            Assert.Equal(
                @"SELECT [ct].[Id], [ct].[GearNickName], [ct].[GearSquadId], [ct].[Note], [ct.Gear].[Nickname], [ct.Gear].[SquadId], [ct.Gear].[AssignedCityName], [ct.Gear].[CityOrBirthName], [ct.Gear].[Discriminator], [ct.Gear].[FullName], [ct.Gear].[HasSoulPatch], [ct.Gear].[LeaderNickname], [ct.Gear].[LeaderSquadId], [ct.Gear].[Rank]
FROM [CogTag] AS [ct]
LEFT JOIN [Gear] AS [ct.Gear] ON ([ct].[GearNickName] = [ct.Gear].[Nickname]) AND ([ct].[GearSquadId] = [ct.Gear].[SquadId])
WHERE (([ct.Gear].[Nickname] = N'Marcus') AND [ct.Gear].[Nickname] IS NOT NULL) AND (([ct.Gear].[CityOrBirthName] <> N'Ephyra') OR [ct.Gear].[CityOrBirthName] IS NULL)
ORDER BY [ct].[GearNickName], [ct].[GearSquadId]",
                Sql);
        }

        public override void GroupJoin_Composite_Key()
        {
            base.GroupJoin_Composite_Key();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [CogTag] AS [ct]
INNER JOIN [Gear] AS [g] ON ([ct].[GearNickName] = [g].[Nickname]) AND ([ct].[GearSquadId] = [g].[SquadId])",
                Sql);
        }

        public override void Join_navigation_translated_to_subquery_composite_key()
        {
            base.Join_navigation_translated_to_subquery_composite_key();

            Assert.Equal(
                @"SELECT [g].[FullName], [t].[Note]
FROM [Gear] AS [g]
INNER JOIN [CogTag] AS [t] ON [g].[FullName] = (
    SELECT TOP(1) [subQuery0].[FullName]
    FROM [Gear] AS [subQuery0]
    WHERE (([subQuery0].[Discriminator] = N'Officer') OR ([subQuery0].[Discriminator] = N'Gear')) AND (([subQuery0].[Nickname] = [t].[GearNickName]) AND ([subQuery0].[SquadId] = [t].[GearSquadId]))
)
WHERE ([g].[Discriminator] = N'Officer') OR ([g].[Discriminator] = N'Gear')",
                Sql);
        }

        public override void Collection_with_inheritance_and_join_include_joined()
        {
            base.Collection_with_inheritance_and_join_include_joined();

            Assert.Equal(
                @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOrBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [c].[Id], [c].[GearNickName], [c].[GearSquadId], [c].[Note]
FROM [CogTag] AS [t]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOrBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
    FROM [Gear] AS [g0]
    WHERE [g0].[Discriminator] = N'Officer'
) AS [t0] ON ([t].[GearSquadId] = [t0].[SquadId]) AND ([t].[GearNickName] = [t0].[Nickname])
LEFT JOIN [CogTag] AS [c] ON ([c].[GearNickName] = [t0].[Nickname]) AND ([c].[GearSquadId] = [t0].[SquadId])",
                Sql);
        }

        public override void Collection_with_inheritance_and_join_include_source()
        {
            base.Collection_with_inheritance_and_join_include_source();

            Assert.Equal(
                @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [c].[Id], [c].[GearNickName], [c].[GearSquadId], [c].[Note]
FROM (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOrBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
    FROM [Gear] AS [g0]
    WHERE [g0].[Discriminator] = N'Officer'
) AS [t]
INNER JOIN [CogTag] AS [t0] ON ([t].[SquadId] = [t0].[GearSquadId]) AND ([t].[Nickname] = [t0].[GearNickName])
LEFT JOIN [CogTag] AS [c] ON ([c].[GearNickName] = [t].[Nickname]) AND ([c].[GearSquadId] = [t].[SquadId])",
                Sql);
        }

        public override void Non_unicode_string_literal_is_used_for_non_unicode_column()
        {
            base.Non_unicode_string_literal_is_used_for_non_unicode_column();

            Assert.Equal(
                @"SELECT [c].[Name], [c].[Location]
FROM [City] AS [c]
WHERE [c].[Location] = 'Unknown'",
                Sql);
        }

        public override void Non_unicode_string_literal_is_used_for_non_unicode_column_right()
        {
            base.Non_unicode_string_literal_is_used_for_non_unicode_column_right();

            Assert.Equal(
                @"SELECT [c].[Name], [c].[Location]
FROM [City] AS [c]
WHERE 'Unknown' = [c].[Location]",
                Sql);
        }

        public override void Non_unicode_parameter_is_used_for_non_unicode_column()
        {
            base.Non_unicode_parameter_is_used_for_non_unicode_column();

            Assert.Equal(
                @"@__value_0: Unknown (Size = 100) (DbType = AnsiString)

SELECT [c].[Name], [c].[Location]
FROM [City] AS [c]
WHERE [c].[Location] = @__value_0",
                Sql);
        }

        public override void Non_unicode_string_literals_in_contains_is_used_for_non_unicode_column()
        {
            base.Non_unicode_string_literals_in_contains_is_used_for_non_unicode_column();

            Assert.Equal(
                @"SELECT [c].[Name], [c].[Location]
FROM [City] AS [c]
WHERE [c].[Location] IN ('Unknown', 'Jacinto''s location', 'Ephyra''s location')",
                Sql);
        }

        public override void Non_unicode_string_literals_is_used_for_non_unicode_column_with_subquery()
        {
            base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_subquery();

            Assert.Equal(
                @"SELECT [c].[Name], [c].[Location]
FROM [City] AS [c]
WHERE ([c].[Location] = 'Unknown') AND ((
    SELECT COUNT(*)
    FROM [Gear] AS [g]
    WHERE ((([g].[Discriminator] = N'Officer') OR ([g].[Discriminator] = N'Gear')) AND ([g].[Nickname] = N'Paduk')) AND ([c].[Name] = [g].[CityOrBirthName])
) = 1)", Sql);
        }

        public override void Non_unicode_string_literals_is_used_for_non_unicode_column_in_subquery()
        {
            base.Non_unicode_string_literals_is_used_for_non_unicode_column_in_subquery();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
INNER JOIN [City] AS [g.CityOfBirth] ON [g].[CityOrBirthName] = [g.CityOfBirth].[Name]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g].[Nickname] = N'Marcus') AND ([g.CityOfBirth].[Location] = 'Jacinto''s location'))",
                Sql);
        }

        public override void Non_unicode_string_literals_is_used_for_non_unicode_column_with_contains()
        {
            base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_contains();

            Assert.Equal(
                @"SELECT [c].[Name], [c].[Location]
FROM [City] AS [c]
WHERE CHARINDEX(N'Jacinto', [c].[Location]) > 0",
                Sql);
        }

        public override void Non_unicode_string_literals_is_used_for_non_unicode_column_with_concat()
        {
            base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_concat();

            Assert.Equal(
                @"SELECT [c].[Name], [c].[Location]
FROM [City] AS [c]
WHERE [c].[Location] + 'Added' LIKE ('%' + 'Add') + '%'",
                Sql);
        }

        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g2].[Nickname], [g2].[SquadId], [g2].[AssignedCityName], [g2].[CityOrBirthName], [g2].[Discriminator], [g2].[FullName], [g2].[HasSoulPatch], [g2].[LeaderNickname], [g2].[LeaderSquadId], [g2].[Rank]
FROM [Gear] AS [g]
LEFT JOIN [Gear] AS [g2] ON [g].[LeaderNickname] = [g2].[Nickname]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[LeaderNickname], [g].[FullName]

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
INNER JOIN (
    SELECT DISTINCT [g].[LeaderNickname], [g].[FullName]
    FROM [Gear] AS [g]
    LEFT JOIN [Gear] AS [g2] ON [g].[LeaderNickname] = [g2].[Nickname]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [g0] ON [w].[OwnerFullName] = [g0].[FullName]
ORDER BY [g0].[LeaderNickname], [g0].[FullName]",
                Sql);
        }

        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2();

            Assert.Equal(
                @"SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOrBirthName], [g1].[Discriminator], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[Rank], [g2].[Nickname], [g2].[SquadId], [g2].[AssignedCityName], [g2].[CityOrBirthName], [g2].[Discriminator], [g2].[FullName], [g2].[HasSoulPatch], [g2].[LeaderNickname], [g2].[LeaderSquadId], [g2].[Rank]
FROM [Gear] AS [g1]
LEFT JOIN [Gear] AS [g2] ON [g1].[LeaderNickname] = [g2].[Nickname]
WHERE [g1].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g1].[LeaderNickname], [g2].[FullName]

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
INNER JOIN (
    SELECT DISTINCT [g1].[LeaderNickname], [g2].[FullName]
    FROM [Gear] AS [g1]
    LEFT JOIN [Gear] AS [g2] ON [g1].[LeaderNickname] = [g2].[Nickname]
    WHERE [g1].[Discriminator] IN (N'Officer', N'Gear')
) AS [g21] ON [w].[OwnerFullName] = [g21].[FullName]
ORDER BY [g21].[LeaderNickname], [g21].[FullName]",
                Sql);
        }

        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g2].[Nickname], [g2].[SquadId], [g2].[AssignedCityName], [g2].[CityOrBirthName], [g2].[Discriminator], [g2].[FullName], [g2].[HasSoulPatch], [g2].[LeaderNickname], [g2].[LeaderSquadId], [g2].[Rank]
FROM [Gear] AS [g]
LEFT JOIN [Gear] AS [g2] ON [g].[LeaderNickname] = [g2].[Nickname]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[LeaderNickname], [g].[FullName], [g2].[FullName]

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
INNER JOIN (
    SELECT DISTINCT [g].[LeaderNickname], [g].[FullName]
    FROM [Gear] AS [g]
    LEFT JOIN [Gear] AS [g2] ON [g].[LeaderNickname] = [g2].[Nickname]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [g0] ON [w].[OwnerFullName] = [g0].[FullName]
ORDER BY [g0].[LeaderNickname], [g0].[FullName]

SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapon] AS [w0]
INNER JOIN (
    SELECT DISTINCT [g].[LeaderNickname], [g].[FullName], [g2].[FullName] AS [FullName0]
    FROM [Gear] AS [g]
    LEFT JOIN [Gear] AS [g2] ON [g].[LeaderNickname] = [g2].[Nickname]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [g21] ON [w0].[OwnerFullName] = [g21].[FullName0]
ORDER BY [g21].[LeaderNickname], [g21].[FullName], [g21].[FullName0]",
                Sql);
        }

        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOrBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank]
FROM [Gear] AS [g]
LEFT JOIN (
    SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOrBirthName], [g1].[Discriminator], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[Rank]
    FROM [Gear] AS [g1]
    WHERE [g1].[Discriminator] = N'Officer'
) AS [t] ON [g].[LeaderNickname] = [t].[Nickname]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[LeaderNickname], [g].[FullName], [t].[FullName]

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
INNER JOIN (
    SELECT DISTINCT [g].[LeaderNickname], [g].[FullName]
    FROM [Gear] AS [g]
    LEFT JOIN (
        SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOrBirthName], [g1].[Discriminator], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[Rank]
        FROM [Gear] AS [g1]
        WHERE [g1].[Discriminator] = N'Officer'
    ) AS [t] ON [g].[LeaderNickname] = [t].[Nickname]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [g4] ON [w].[OwnerFullName] = [g4].[FullName]
ORDER BY [g4].[LeaderNickname], [g4].[FullName]

SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapon] AS [w0]
INNER JOIN (
    SELECT DISTINCT [g].[LeaderNickname], [g].[FullName], [t].[FullName] AS [FullName0]
    FROM [Gear] AS [g]
    LEFT JOIN (
        SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOrBirthName], [g1].[Discriminator], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[Rank]
        FROM [Gear] AS [g1]
        WHERE [g1].[Discriminator] = N'Officer'
    ) AS [t] ON [g].[LeaderNickname] = [t].[Nickname]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [t1] ON [w0].[OwnerFullName] = [t1].[FullName0]
ORDER BY [t1].[LeaderNickname], [t1].[FullName], [t1].[FullName0]",
                Sql);
        }

        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g2].[Nickname], [g2].[SquadId], [g2].[AssignedCityName], [g2].[CityOrBirthName], [g2].[Discriminator], [g2].[FullName], [g2].[HasSoulPatch], [g2].[LeaderNickname], [g2].[LeaderSquadId], [g2].[Rank]
FROM [Gear] AS [g]
LEFT JOIN [Gear] AS [g2] ON [g].[LeaderNickname] = [g2].[Nickname]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[LeaderNickname], [g].[FullName], [g2].[FullName]

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
INNER JOIN (
    SELECT DISTINCT [g].[LeaderNickname], [g].[FullName]
    FROM [Gear] AS [g]
    LEFT JOIN [Gear] AS [g2] ON [g].[LeaderNickname] = [g2].[Nickname]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [g0] ON [w].[OwnerFullName] = [g0].[FullName]
ORDER BY [g0].[LeaderNickname], [g0].[FullName]

SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapon] AS [w0]
INNER JOIN (
    SELECT DISTINCT [g].[LeaderNickname], [g].[FullName], [g2].[FullName] AS [FullName0]
    FROM [Gear] AS [g]
    LEFT JOIN [Gear] AS [g2] ON [g].[LeaderNickname] = [g2].[Nickname]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [g21] ON [w0].[OwnerFullName] = [g21].[FullName0]
ORDER BY [g21].[LeaderNickname], [g21].[FullName], [g21].[FullName0]",
                Sql);
        }

        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g2].[Nickname], [g2].[SquadId], [g2].[AssignedCityName], [g2].[CityOrBirthName], [g2].[Discriminator], [g2].[FullName], [g2].[HasSoulPatch], [g2].[LeaderNickname], [g2].[LeaderSquadId], [g2].[Rank]
FROM [Gear] AS [g]
LEFT JOIN [Gear] AS [g2] ON [g].[LeaderNickname] = [g2].[Nickname]
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[LeaderNickname], [g].[FullName], [g2].[FullName]

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
INNER JOIN (
    SELECT DISTINCT [g].[LeaderNickname], [g].[FullName]
    FROM [Gear] AS [g]
    LEFT JOIN [Gear] AS [g2] ON [g].[LeaderNickname] = [g2].[Nickname]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [g0] ON [w].[OwnerFullName] = [g0].[FullName]
ORDER BY [g0].[LeaderNickname], [g0].[FullName]

SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapon] AS [w0]
INNER JOIN (
    SELECT DISTINCT [g].[LeaderNickname], [g].[FullName], [g2].[FullName] AS [FullName0]
    FROM [Gear] AS [g]
    LEFT JOIN [Gear] AS [g2] ON [g].[LeaderNickname] = [g2].[Nickname]
    WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
) AS [g21] ON [w0].[OwnerFullName] = [g21].[FullName0]
ORDER BY [g21].[LeaderNickname], [g21].[FullName], [g21].[FullName0]",
                Sql);
        }

        public override void Coalesce_operator_in_predicate()
        {
            base.Coalesce_operator_in_predicate();

            Assert.Equal(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE COALESCE([w].[IsAutomatic], 0) = 1",
                Sql);
        }

        public override void Coalesce_operator_in_predicate_with_other_conditions()
        {
            base.Coalesce_operator_in_predicate_with_other_conditions();

            Assert.Equal(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE ([w].[AmmunitionType] = 1) AND (COALESCE([w].[IsAutomatic], 0) = 1)",
                Sql);
        }

        public override void Coalesce_operator_in_projection_with_other_conditions()
        {
            base.Coalesce_operator_in_projection_with_other_conditions();

            Assert.Equal(
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

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t.Gear].[Nickname], [t.Gear].[SquadId], [t.Gear].[AssignedCityName], [t.Gear].[CityOrBirthName], [t.Gear].[Discriminator], [t.Gear].[FullName], [t.Gear].[HasSoulPatch], [t.Gear].[LeaderNickname], [t.Gear].[LeaderSquadId], [t.Gear].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN [Gear] AS [t.Gear] ON ([t].[GearNickName] = [t.Gear].[Nickname]) AND ([t].[GearSquadId] = [t.Gear].[SquadId])
WHERE (([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL) AND ([t.Gear].[HasSoulPatch] = 1)
ORDER BY [t].[GearNickName], [t].[GearSquadId]",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_predicate2()
        {
            base.Optional_navigation_type_compensation_works_with_predicate2();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t.Gear].[Nickname], [t.Gear].[SquadId], [t.Gear].[AssignedCityName], [t.Gear].[CityOrBirthName], [t.Gear].[Discriminator], [t.Gear].[FullName], [t.Gear].[HasSoulPatch], [t.Gear].[LeaderNickname], [t.Gear].[LeaderSquadId], [t.Gear].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN [Gear] AS [t.Gear] ON ([t].[GearNickName] = [t.Gear].[Nickname]) AND ([t].[GearSquadId] = [t.Gear].[SquadId])
WHERE [t.Gear].[HasSoulPatch] = 1
ORDER BY [t].[GearNickName], [t].[GearSquadId]",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_predicate_negated()
        {
            base.Optional_navigation_type_compensation_works_with_predicate_negated();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t.Gear].[Nickname], [t.Gear].[SquadId], [t.Gear].[AssignedCityName], [t.Gear].[CityOrBirthName], [t.Gear].[Discriminator], [t.Gear].[FullName], [t.Gear].[HasSoulPatch], [t.Gear].[LeaderNickname], [t.Gear].[LeaderSquadId], [t.Gear].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN [Gear] AS [t.Gear] ON ([t].[GearNickName] = [t.Gear].[Nickname]) AND ([t].[GearSquadId] = [t.Gear].[SquadId])
WHERE ([t.Gear].[HasSoulPatch] <> 1) AND [t.Gear].[HasSoulPatch] IS NOT NULL
ORDER BY [t].[GearNickName], [t].[GearSquadId]",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_predicate_negated_complex1()
        {
            base.Optional_navigation_type_compensation_works_with_predicate_negated_complex1();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t.Gear].[Nickname], [t.Gear].[SquadId], [t.Gear].[AssignedCityName], [t.Gear].[CityOrBirthName], [t.Gear].[Discriminator], [t.Gear].[FullName], [t.Gear].[HasSoulPatch], [t.Gear].[LeaderNickname], [t.Gear].[LeaderSquadId], [t.Gear].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN [Gear] AS [t.Gear] ON ([t].[GearNickName] = [t.Gear].[Nickname]) AND ([t].[GearSquadId] = [t.Gear].[SquadId])
WHERE CASE
    WHEN [t.Gear].[HasSoulPatch] = 1
    THEN 1 ELSE [t.Gear].[HasSoulPatch]
END <> 1
ORDER BY [t].[GearNickName], [t].[GearSquadId]",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_predicate_negated_complex2()
        {
            base.Optional_navigation_type_compensation_works_with_predicate_negated_complex2();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t.Gear].[Nickname], [t.Gear].[SquadId], [t.Gear].[AssignedCityName], [t.Gear].[CityOrBirthName], [t.Gear].[Discriminator], [t.Gear].[FullName], [t.Gear].[HasSoulPatch], [t.Gear].[LeaderNickname], [t.Gear].[LeaderSquadId], [t.Gear].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN [Gear] AS [t.Gear] ON ([t].[GearNickName] = [t.Gear].[Nickname]) AND ([t].[GearSquadId] = [t.Gear].[SquadId])
WHERE CASE
    WHEN ([t.Gear].[HasSoulPatch] <> 1) AND [t.Gear].[HasSoulPatch] IS NOT NULL
    THEN 0 ELSE [t.Gear].[HasSoulPatch]
END <> 1
ORDER BY [t].[GearNickName], [t].[GearSquadId]",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_conditional_expression()
        {
            base.Optional_navigation_type_compensation_works_with_conditional_expression();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t.Gear].[Nickname], [t.Gear].[SquadId], [t.Gear].[AssignedCityName], [t.Gear].[CityOrBirthName], [t.Gear].[Discriminator], [t.Gear].[FullName], [t.Gear].[HasSoulPatch], [t.Gear].[LeaderNickname], [t.Gear].[LeaderSquadId], [t.Gear].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN [Gear] AS [t.Gear] ON ([t].[GearNickName] = [t.Gear].[Nickname]) AND ([t].[GearSquadId] = [t.Gear].[SquadId])
WHERE CASE
    WHEN [t.Gear].[HasSoulPatch] = 1
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = 1
ORDER BY [t].[GearNickName], [t].[GearSquadId]",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_binary_expression()
        {
            base.Optional_navigation_type_compensation_works_with_binary_expression();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t.Gear].[Nickname], [t.Gear].[SquadId], [t.Gear].[AssignedCityName], [t.Gear].[CityOrBirthName], [t.Gear].[Discriminator], [t.Gear].[FullName], [t.Gear].[HasSoulPatch], [t.Gear].[LeaderNickname], [t.Gear].[LeaderSquadId], [t.Gear].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN [Gear] AS [t.Gear] ON ([t].[GearNickName] = [t.Gear].[Nickname]) AND ([t].[GearSquadId] = [t.Gear].[SquadId])
WHERE ([t.Gear].[HasSoulPatch] = 1) OR (CHARINDEX(N'Cole', [t].[Note]) > 0)
ORDER BY [t].[GearNickName], [t].[GearSquadId]",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_projection()
        {
            base.Optional_navigation_type_compensation_works_with_projection();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t.Gear].[Nickname], [t.Gear].[SquadId], [t.Gear].[AssignedCityName], [t.Gear].[CityOrBirthName], [t.Gear].[Discriminator], [t.Gear].[FullName], [t.Gear].[HasSoulPatch], [t.Gear].[LeaderNickname], [t.Gear].[LeaderSquadId], [t.Gear].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN [Gear] AS [t.Gear] ON ([t].[GearNickName] = [t.Gear].[Nickname]) AND ([t].[GearSquadId] = [t.Gear].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL
ORDER BY [t].[GearNickName], [t].[GearSquadId]",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_projection_into_anonymous_type()
        {
            base.Optional_navigation_type_compensation_works_with_projection_into_anonymous_type();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t.Gear].[Nickname], [t.Gear].[SquadId], [t.Gear].[AssignedCityName], [t.Gear].[CityOrBirthName], [t.Gear].[Discriminator], [t.Gear].[FullName], [t.Gear].[HasSoulPatch], [t.Gear].[LeaderNickname], [t.Gear].[LeaderSquadId], [t.Gear].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN [Gear] AS [t.Gear] ON ([t].[GearNickName] = [t.Gear].[Nickname]) AND ([t].[GearSquadId] = [t.Gear].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL
ORDER BY [t].[GearNickName], [t].[GearSquadId]",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_DTOs()
        {
            base.Optional_navigation_type_compensation_works_with_DTOs();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t.Gear].[Nickname], [t.Gear].[SquadId], [t.Gear].[AssignedCityName], [t.Gear].[CityOrBirthName], [t.Gear].[Discriminator], [t.Gear].[FullName], [t.Gear].[HasSoulPatch], [t.Gear].[LeaderNickname], [t.Gear].[LeaderSquadId], [t.Gear].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN [Gear] AS [t.Gear] ON ([t].[GearNickName] = [t.Gear].[Nickname]) AND ([t].[GearSquadId] = [t.Gear].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL
ORDER BY [t].[GearNickName], [t].[GearSquadId]",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_list_initializers()
        {
            base.Optional_navigation_type_compensation_works_with_list_initializers();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t.Gear].[Nickname], [t.Gear].[SquadId], [t.Gear].[AssignedCityName], [t.Gear].[CityOrBirthName], [t.Gear].[Discriminator], [t.Gear].[FullName], [t.Gear].[HasSoulPatch], [t.Gear].[LeaderNickname], [t.Gear].[LeaderSquadId], [t.Gear].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN [Gear] AS [t.Gear] ON ([t].[GearNickName] = [t.Gear].[Nickname]) AND ([t].[GearSquadId] = [t.Gear].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL
ORDER BY [t].[GearNickName], [t].[GearSquadId]",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_array_initializers()
        {
            base.Optional_navigation_type_compensation_works_with_array_initializers();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t.Gear].[Nickname], [t.Gear].[SquadId], [t.Gear].[AssignedCityName], [t.Gear].[CityOrBirthName], [t.Gear].[Discriminator], [t.Gear].[FullName], [t.Gear].[HasSoulPatch], [t.Gear].[LeaderNickname], [t.Gear].[LeaderSquadId], [t.Gear].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN [Gear] AS [t.Gear] ON ([t].[GearNickName] = [t.Gear].[Nickname]) AND ([t].[GearSquadId] = [t.Gear].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL
ORDER BY [t].[GearNickName], [t].[GearSquadId]",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_orderby()
        {
            base.Optional_navigation_type_compensation_works_with_orderby();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t.Gear].[Nickname], [t.Gear].[SquadId], [t.Gear].[AssignedCityName], [t.Gear].[CityOrBirthName], [t.Gear].[Discriminator], [t.Gear].[FullName], [t.Gear].[HasSoulPatch], [t.Gear].[LeaderNickname], [t.Gear].[LeaderSquadId], [t.Gear].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN [Gear] AS [t.Gear] ON ([t].[GearNickName] = [t.Gear].[Nickname]) AND ([t].[GearSquadId] = [t.Gear].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL
ORDER BY [t.Gear].[SquadId], [t].[GearNickName], [t].[GearSquadId]",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_groupby()
        {
            base.Optional_navigation_type_compensation_works_with_groupby();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t.Gear].[Nickname], [t.Gear].[SquadId], [t.Gear].[AssignedCityName], [t.Gear].[CityOrBirthName], [t.Gear].[Discriminator], [t.Gear].[FullName], [t.Gear].[HasSoulPatch], [t.Gear].[LeaderNickname], [t.Gear].[LeaderSquadId], [t.Gear].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN [Gear] AS [t.Gear] ON ([t].[GearNickName] = [t.Gear].[Nickname]) AND ([t].[GearSquadId] = [t.Gear].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL
ORDER BY [t.Gear].[SquadId]",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_all()
        {
            base.Optional_navigation_type_compensation_works_with_all();

            Assert.Equal(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [CogTag] AS [t]
        LEFT JOIN [Gear] AS [t.Gear] ON ([t].[GearNickName] = [t.Gear].[Nickname]) AND ([t].[GearSquadId] = [t.Gear].[SquadId])
        WHERE (([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL) AND ([t.Gear].[HasSoulPatch] = 0))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_contains()
        {
            base.Optional_navigation_type_compensation_works_with_contains();

            Assert.StartsWith(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t.Gear].[Nickname], [t.Gear].[SquadId], [t.Gear].[AssignedCityName], [t.Gear].[CityOrBirthName], [t.Gear].[Discriminator], [t.Gear].[FullName], [t.Gear].[HasSoulPatch], [t.Gear].[LeaderNickname], [t.Gear].[LeaderSquadId], [t.Gear].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN [Gear] AS [t.Gear] ON ([t].[GearNickName] = [t.Gear].[Nickname]) AND ([t].[GearSquadId] = [t.Gear].[SquadId])
WHERE (([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL) AND [t.Gear].[SquadId] IN (
    SELECT [g].[SquadId]
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = N'Officer') OR ([g].[Discriminator] = N'Gear')
)
ORDER BY [t].[GearNickName], [t].[GearSquadId]",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_skip()
        {
            base.Optional_navigation_type_compensation_works_with_skip();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t.Gear].[Nickname], [t.Gear].[SquadId], [t.Gear].[AssignedCityName], [t.Gear].[CityOrBirthName], [t.Gear].[Discriminator], [t.Gear].[FullName], [t.Gear].[HasSoulPatch], [t.Gear].[LeaderNickname], [t.Gear].[LeaderSquadId], [t.Gear].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN [Gear] AS [t.Gear] ON ([t].[GearNickName] = [t.Gear].[Nickname]) AND ([t].[GearSquadId] = [t.Gear].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL
ORDER BY [t].[GearNickName], [t].[GearSquadId]",
                Sql);
        }

        public override void Optional_navigation_type_compensation_works_with_take()
        {
            base.Optional_navigation_type_compensation_works_with_take();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [t.Gear].[Nickname], [t.Gear].[SquadId], [t.Gear].[AssignedCityName], [t.Gear].[CityOrBirthName], [t.Gear].[Discriminator], [t.Gear].[FullName], [t.Gear].[HasSoulPatch], [t.Gear].[LeaderNickname], [t.Gear].[LeaderSquadId], [t.Gear].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN [Gear] AS [t.Gear] ON ([t].[GearNickName] = [t.Gear].[Nickname]) AND ([t].[GearSquadId] = [t.Gear].[SquadId])
WHERE ([t].[Note] <> N'K.I.A.') OR [t].[Note] IS NULL
ORDER BY [t].[GearNickName], [t].[GearSquadId]",
                Sql);
        }

        public override void Select_correlated_filtered_collection()
        {
            base.Select_correlated_filtered_collection();

            Assert.Equal(
                @"SELECT [g].[FullName]
FROM [Gear] AS [g]
WHERE (([g].[Discriminator] = N'Officer') OR ([g].[Discriminator] = N'Gear')) AND [g].[CityOrBirthName] IN (N'Ephyra', N'Hanover')

@_outer_FullName: Augustus Cole (Size = 450)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE (([w].[Name] <> N'Lancer') OR [w].[Name] IS NULL) AND ((@_outer_FullName = [w].[OwnerFullName]) AND [w].[OwnerFullName] IS NOT NULL)

@_outer_FullName: Dominic Santiago (Size = 450)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE (([w].[Name] <> N'Lancer') OR [w].[Name] IS NULL) AND ((@_outer_FullName = [w].[OwnerFullName]) AND [w].[OwnerFullName] IS NOT NULL)",
                Sql);
        }

        public override void Select_correlated_filtered_collection_with_composite_key()
        {
            base.Select_correlated_filtered_collection_with_composite_key();

            Assert.Equal(
                @"SELECT [t].[Nickname], [t].[SquadId]
FROM (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOrBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank]
    FROM [Gear] AS [g0]
    WHERE [g0].[Discriminator] = N'Officer'
) AS [t]

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

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
INNER JOIN [Weapon] AS [w] ON [w].[SynergyWithId] IS NOT NULL
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')",
                Sql);
        }

        public override void Join_predicate_value()
        {
            base.Join_predicate_value();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
INNER JOIN [Weapon] AS [w] ON [g].[HasSoulPatch] = 1
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')",
                Sql);
        }

        public override void Join_predicate_condition_equals_condition()
        {
            base.Join_predicate_condition_equals_condition();

            Assert.Equal(
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

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Gear] AS [g]
LEFT JOIN [Weapon] AS [w] ON [w].[SynergyWithId] IS NOT NULL
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY (SELECT 1)",
                Sql);
        }

        public override void Left_join_predicate_value()
        {
            base.Left_join_predicate_value();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Gear] AS [g]
LEFT JOIN [Weapon] AS [w] ON [g].[HasSoulPatch] = 1
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY [g].[HasSoulPatch]",
                Sql);
        }

        public override void Left_join_predicate_condition_equals_condition()
        {
            base.Left_join_predicate_condition_equals_condition();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Gear] AS [g]
LEFT JOIN [Weapon] AS [w] ON CASE
    WHEN [g].[FullName] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN [w].[SynergyWithId] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
WHERE [g].[Discriminator] IN (N'Officer', N'Gear')
ORDER BY CASE
    WHEN [g].[FullName] IS NULL
    THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT)
END",
                Sql);
        }

        public override void Complex_predicate_with_AndAlso_and_nullable_bool_property()
        {
            base.Complex_predicate_with_AndAlso_and_nullable_bool_property();

            Assert.Equal(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [w.Owner].[Nickname], [w.Owner].[SquadId], [w.Owner].[AssignedCityName], [w.Owner].[CityOrBirthName], [w.Owner].[Discriminator], [w.Owner].[FullName], [w.Owner].[HasSoulPatch], [w.Owner].[LeaderNickname], [w.Owner].[LeaderSquadId], [w.Owner].[Rank]
FROM [Weapon] AS [w]
LEFT JOIN [Gear] AS [w.Owner] ON [w].[OwnerFullName] = [w.Owner].[FullName]
WHERE ([w].[Id] <> 50) AND ([w.Owner].[HasSoulPatch] = 0)
ORDER BY [w].[OwnerFullName]",
                Sql);
        }

        public override void Distinct_with_optional_navigation_is_evaluated_on_client()
        {
            base.Distinct_with_optional_navigation_is_evaluated_on_client();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Tag].[Id], [g.Tag].[GearNickName], [g.Tag].[GearSquadId], [g.Tag].[Note]
FROM [Gear] AS [g]
LEFT JOIN [CogTag] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g.Tag].[Note] <> N'Foo') OR [g.Tag].[Note] IS NULL)
ORDER BY [g].[Nickname], [g].[SquadId]",
                Sql);
        }

        public override void Sum_with_optional_navigation_is_evaluated_on_client()
        {
            base.Sum_with_optional_navigation_is_evaluated_on_client();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [g.Tag].[Id], [g.Tag].[GearNickName], [g.Tag].[GearSquadId], [g.Tag].[Note]
FROM [Gear] AS [g]
LEFT JOIN [CogTag] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g.Tag].[Note] <> N'Foo') OR [g.Tag].[Note] IS NULL)
ORDER BY [g].[Nickname], [g].[SquadId]",
                Sql);
        }

        public override void Count_with_optional_navigation_is_translated_to_sql()
        {
            base.Count_with_optional_navigation_is_translated_to_sql();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM [Gear] AS [g]
LEFT JOIN [CogTag] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
WHERE [g].[Discriminator] IN (N'Officer', N'Gear') AND (([g.Tag].[Note] <> N'Foo') OR [g.Tag].[Note] IS NULL)",
                Sql);
        }

        public override void FirstOrDefault_with_manually_created_groupjoin_is_translated_to_sql()
        {
            base.FirstOrDefault_with_manually_created_groupjoin_is_translated_to_sql();

            Assert.Equal(
                @"SELECT TOP(1) [s].[Id], [s].[InternalNumber], [s].[Name], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Squad] AS [s]
LEFT JOIN [Gear] AS [g] ON [s].[Id] = [g].[SquadId]
WHERE [s].[Name] = N'Kilo'
ORDER BY [s].[Id]",
                Sql);
        }

        public override void Any_with_optional_navigation_as_subquery_predicate_is_translated_to_sql()
        {
            base.Any_with_optional_navigation_as_subquery_predicate_is_translated_to_sql();

            Assert.Equal(
                @"SELECT [s].[Id], [s].[Name]
FROM [Squad] AS [s]

@_outer_Id: 1

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Gear] AS [m]
        LEFT JOIN [CogTag] AS [m.Tag] ON ([m].[Nickname] = [m.Tag].[GearNickName]) AND ([m].[SquadId] = [m.Tag].[GearSquadId])
        WHERE ((([m].[Discriminator] = N'Officer') OR ([m].[Discriminator] = N'Gear')) AND ([m.Tag].[Note] = N'Dom''s Tag')) AND (@_outer_Id = [m].[SquadId]))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END

@_outer_Id: 2

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Gear] AS [m]
        LEFT JOIN [CogTag] AS [m.Tag] ON ([m].[Nickname] = [m.Tag].[GearNickName]) AND ([m].[SquadId] = [m.Tag].[GearSquadId])
        WHERE ((([m].[Discriminator] = N'Officer') OR ([m].[Discriminator] = N'Gear')) AND ([m.Tag].[Note] = N'Dom''s Tag')) AND (@_outer_Id = [m].[SquadId]))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void All_with_optional_navigation_is_translated_to_sql()
        {
            base.All_with_optional_navigation_is_translated_to_sql();

            Assert.Equal(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Gear] AS [g]
        LEFT JOIN [CogTag] AS [g.Tag] ON ([g].[Nickname] = [g.Tag].[GearNickName]) AND ([g].[SquadId] = [g.Tag].[GearSquadId])
        WHERE (([g].[Discriminator] = N'Officer') OR ([g].[Discriminator] = N'Gear')) AND (([g.Tag].[Note] = N'Foo') AND [g.Tag].[Note] IS NOT NULL))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void Non_flattened_GroupJoin_with_result_operator_evaluates_on_the_client()
        {
            base.Non_flattened_GroupJoin_with_result_operator_evaluates_on_the_client();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN [Gear] AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
ORDER BY [t].[GearNickName], [t].[GearSquadId]",
                Sql);
        }

        protected override void ClearLog() => TestSqlLoggerFactory.Reset();

        private const string FileLineEnding = @"
";

        private static string Sql => TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);
    }
}
