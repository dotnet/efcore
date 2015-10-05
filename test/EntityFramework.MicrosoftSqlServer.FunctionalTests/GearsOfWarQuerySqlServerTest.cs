// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class GearsOfWarQuerySqlServerTest : GearsOfWarQueryTestBase<SqlServerTestStore, GearsOfWarQuerySqlServerFixture>
    {
        public override void Include_multiple_one_to_one_and_one_to_many()
        {
            base.Include_multiple_one_to_one_and_one_to_many();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
) AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
ORDER BY [g].[FullName]

SELECT [w].[Id], [w].[AmmunitionType], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
INNER JOIN (
    SELECT DISTINCT [g].[FullName]
    FROM [CogTag] AS [t]
    LEFT JOIN (
        SELECT [g].*
        FROM [Gear] AS [g]
        WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
    ) AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
) AS [g] ON [w].[OwnerFullName] = [g].[FullName]
ORDER BY [g].[FullName]",
                Sql);
        }

        public override void Include_multiple_one_to_one_and_one_to_many_self_reference()
        {
            base.Include_multiple_one_to_one_and_one_to_many_self_reference();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
) AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
ORDER BY [g].[FullName]

SELECT [w].[Id], [w].[AmmunitionType], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
INNER JOIN (
    SELECT DISTINCT [g].[FullName]
    FROM [CogTag] AS [t]
    LEFT JOIN (
        SELECT [g].*
        FROM [Gear] AS [g]
        WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
    ) AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
) AS [g] ON [w].[OwnerFullName] = [g].[FullName]
ORDER BY [g].[FullName]",
                Sql);
        }

        public override void Include_multiple_one_to_one_and_one_to_one_and_one_to_many()
        {
            base.Include_multiple_one_to_one_and_one_to_one_and_one_to_many();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [s].[Id], [s].[InternalNumber], [s].[Name]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
) AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
LEFT JOIN [Squad] AS [s] ON [g].[SquadId] = [s].[Id]
ORDER BY [s].[Id]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
INNER JOIN (
    SELECT DISTINCT [s].[Id]
    FROM [CogTag] AS [t]
    LEFT JOIN (
        SELECT [g].*
        FROM [Gear] AS [g]
        WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
    ) AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
    LEFT JOIN [Squad] AS [s] ON [g].[SquadId] = [s].[Id]
) AS [s] ON [g].[SquadId] = [s].[Id]
WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
ORDER BY [s].[Id]", Sql);
        }

        public override void Include_multiple_one_to_one_optional_and_one_to_one_required()
        {
            base.Include_multiple_one_to_one_optional_and_one_to_one_required();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [s].[Id], [s].[InternalNumber], [s].[Name]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
) AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
LEFT JOIN [Squad] AS [s] ON [g].[SquadId] = [s].[Id]", Sql);
        }

        public override void Include_multiple_circular()
        {
            base.Include_multiple_circular();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location]
FROM [Gear] AS [g]
INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
WHERE [g].[Discriminator] IN ('Officer', 'Gear')
ORDER BY [c].[Name]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
INNER JOIN (
    SELECT DISTINCT [c].[Name]
    FROM [Gear] AS [g]
    INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
    WHERE [g].[Discriminator] IN ('Officer', 'Gear')
) AS [c] ON [g].[AssignedCityName] = [c].[Name]
WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
ORDER BY [c].[Name]",
                Sql);
        }

        public override void Include_multiple_circular_with_filter()
        {
            base.Include_multiple_circular_with_filter();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location]
FROM [Gear] AS [g]
INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
WHERE [g].[Discriminator] IN ('Officer', 'Gear') AND ([g].[Nickname] = 'Marcus')
ORDER BY [c].[Name]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
INNER JOIN (
    SELECT DISTINCT [c].[Name]
    FROM [Gear] AS [g]
    INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
    WHERE [g].[Discriminator] IN ('Officer', 'Gear') AND ([g].[Nickname] = 'Marcus')
) AS [c] ON [g].[AssignedCityName] = [c].[Name]
WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
ORDER BY [c].[Name]",
                Sql);
        }

        public override void Include_using_alternate_key()
        {
            base.Include_using_alternate_key();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN ('Officer', 'Gear') AND ([g].[Nickname] = 'Marcus')
ORDER BY [g].[FullName]

SELECT [w].[Id], [w].[AmmunitionType], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
INNER JOIN (
    SELECT DISTINCT [g].[FullName]
    FROM [Gear] AS [g]
    WHERE [g].[Discriminator] IN ('Officer', 'Gear') AND ([g].[Nickname] = 'Marcus')
) AS [g] ON [w].[OwnerFullName] = [g].[FullName]
ORDER BY [g].[FullName]",
                Sql);
        }

        public override void Include_multiple_include_then_include()
        {
            base.Include_multiple_include_then_include();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location], [c0].[Name], [c0].[Location], [c1].[Name], [c1].[Location], [c2].[Name], [c2].[Location]
FROM [Gear] AS [g]
LEFT JOIN [City] AS [c] ON [g].[AssignedCityName] = [c].[Name]
LEFT JOIN [City] AS [c0] ON [g].[AssignedCityName] = [c0].[Name]
INNER JOIN [City] AS [c1] ON [g].[CityOrBirthName] = [c1].[Name]
INNER JOIN [City] AS [c2] ON [g].[CityOrBirthName] = [c2].[Name]
WHERE [g].[Discriminator] IN ('Officer', 'Gear')
ORDER BY [g].[Nickname], [c].[Name], [c0].[Name], [c1].[Name], [c2].[Name]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Id], [c].[GearNickName], [c].[GearSquadId], [c].[Note]
FROM [Gear] AS [g]
INNER JOIN (
    SELECT DISTINCT [g].[Nickname], [c].[Name], [c0].[Name] AS [Name0], [c1].[Name] AS [Name1], [c2].[Name] AS [Name2]
    FROM [Gear] AS [g]
    LEFT JOIN [City] AS [c] ON [g].[AssignedCityName] = [c].[Name]
    LEFT JOIN [City] AS [c0] ON [g].[AssignedCityName] = [c0].[Name]
    INNER JOIN [City] AS [c1] ON [g].[CityOrBirthName] = [c1].[Name]
    INNER JOIN [City] AS [c2] ON [g].[CityOrBirthName] = [c2].[Name]
    WHERE [g].[Discriminator] IN ('Officer', 'Gear')
) AS [c2] ON [g].[AssignedCityName] = [c2].[Name2]
LEFT JOIN [CogTag] AS [c] ON ([c].[GearNickName] = [g].[Nickname]) AND ([c].[GearSquadId] = [g].[SquadId])
WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
ORDER BY [c2].[Nickname], [c2].[Name], [c2].[Name0], [c2].[Name1], [c2].[Name2]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Id], [c].[GearNickName], [c].[GearSquadId], [c].[Note]
FROM [Gear] AS [g]
INNER JOIN (
    SELECT DISTINCT [g].[Nickname], [c].[Name], [c0].[Name] AS [Name0], [c1].[Name] AS [Name1]
    FROM [Gear] AS [g]
    LEFT JOIN [City] AS [c] ON [g].[AssignedCityName] = [c].[Name]
    LEFT JOIN [City] AS [c0] ON [g].[AssignedCityName] = [c0].[Name]
    INNER JOIN [City] AS [c1] ON [g].[CityOrBirthName] = [c1].[Name]
    WHERE [g].[Discriminator] IN ('Officer', 'Gear')
) AS [c1] ON [g].[CityOrBirthName] = [c1].[Name1]
LEFT JOIN [CogTag] AS [c] ON ([c].[GearNickName] = [g].[Nickname]) AND ([c].[GearSquadId] = [g].[SquadId])
WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
ORDER BY [c1].[Nickname], [c1].[Name], [c1].[Name0], [c1].[Name1]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Id], [c].[GearNickName], [c].[GearSquadId], [c].[Note]
FROM [Gear] AS [g]
INNER JOIN (
    SELECT DISTINCT [g].[Nickname], [c].[Name], [c0].[Name] AS [Name0]
    FROM [Gear] AS [g]
    LEFT JOIN [City] AS [c] ON [g].[AssignedCityName] = [c].[Name]
    LEFT JOIN [City] AS [c0] ON [g].[AssignedCityName] = [c0].[Name]
    WHERE [g].[Discriminator] IN ('Officer', 'Gear')
) AS [c0] ON [g].[AssignedCityName] = [c0].[Name0]
LEFT JOIN [CogTag] AS [c] ON ([c].[GearNickName] = [g].[Nickname]) AND ([c].[GearSquadId] = [g].[SquadId])
WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
ORDER BY [c0].[Nickname], [c0].[Name], [c0].[Name0]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c0].[Id], [c0].[GearNickName], [c0].[GearSquadId], [c0].[Note]
FROM [Gear] AS [g]
INNER JOIN (
    SELECT DISTINCT [g].[Nickname], [c].[Name]
    FROM [Gear] AS [g]
    LEFT JOIN [City] AS [c] ON [g].[AssignedCityName] = [c].[Name]
    WHERE [g].[Discriminator] IN ('Officer', 'Gear')
) AS [c] ON [g].[CityOrBirthName] = [c].[Name]
LEFT JOIN [CogTag] AS [c0] ON ([c0].[GearNickName] = [g].[Nickname]) AND ([c0].[GearSquadId] = [g].[SquadId])
WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
ORDER BY [c].[Nickname], [c].[Name]",
                Sql);
        }

        public override void Include_navigation_on_derived_type()
        {
            base.Include_navigation_on_derived_type();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] = 'Officer'
ORDER BY [g].[Nickname], [g].[SquadId]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
INNER JOIN (
    SELECT DISTINCT [g].[Nickname], [g].[SquadId]
    FROM [Gear] AS [g]
    WHERE [g].[Discriminator] = 'Officer'
) AS [g0] ON ([g].[LeaderNickname] = [g0].[Nickname]) AND ([g].[LeaderSquadId] = [g0].[SquadId])
WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
ORDER BY [g0].[Nickname], [g0].[SquadId]",
                Sql);
        }

        public override void Select_Where_Navigation_Included()
        {
            base.Select_Where_Navigation_Included();

            Assert.Equal(
                @"SELECT [o].[Id], [o].[GearNickName], [o].[GearSquadId], [o].[Note], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [CogTag] AS [o]
INNER JOIN [Gear] AS [o.Gear] ON ([o].[GearNickName] = [o.Gear].[Nickname]) AND ([o].[GearSquadId] = [o.Gear].[SquadId])
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
) AS [g] ON ([o].[GearNickName] = [g].[Nickname]) AND ([o].[GearSquadId] = [g].[SquadId])
WHERE [o.Gear].[Nickname] = 'Marcus'",
                Sql);
        }

        public override void Include_with_join_reference1()
        {
            base.Include_with_join_reference1();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location]
FROM [Gear] AS [g]
INNER JOIN [CogTag] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
WHERE [g].[Discriminator] IN ('Officer', 'Gear')",
                Sql);
        }

        public override void Include_with_join_reference2()
        {
            base.Include_with_join_reference2();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location]
FROM [CogTag] AS [t]
INNER JOIN [Gear] AS [g] ON ([t].[GearSquadId] = [g].[SquadId]) AND ([t].[GearNickName] = [g].[Nickname])
INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]",
                Sql);
        }

        public override void Include_with_join_collection1()
        {
            base.Include_with_join_collection1();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
INNER JOIN [CogTag] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
WHERE [g].[Discriminator] IN ('Officer', 'Gear')
ORDER BY [g].[FullName]

SELECT [w].[Id], [w].[AmmunitionType], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
INNER JOIN (
    SELECT DISTINCT [g].[FullName]
    FROM [Gear] AS [g]
    INNER JOIN [CogTag] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
    WHERE [g].[Discriminator] IN ('Officer', 'Gear')
) AS [g] ON [w].[OwnerFullName] = [g].[FullName]
ORDER BY [g].[FullName]",
                Sql);
        }

        public override void Include_with_join_collection2()
        {
            base.Include_with_join_collection2();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [CogTag] AS [t]
INNER JOIN [Gear] AS [g] ON ([t].[GearSquadId] = [g].[SquadId]) AND ([t].[GearNickName] = [g].[Nickname])
ORDER BY [g].[FullName]

SELECT [w].[Id], [w].[AmmunitionType], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
INNER JOIN (
    SELECT DISTINCT [g].[FullName]
    FROM [CogTag] AS [t]
    INNER JOIN [Gear] AS [g] ON ([t].[GearSquadId] = [g].[SquadId]) AND ([t].[GearNickName] = [g].[Nickname])
) AS [g] ON [w].[OwnerFullName] = [g].[FullName]
ORDER BY [g].[FullName]",
                Sql);
        }

        public override void Include_with_join_multi_level()
        {
            base.Include_with_join_multi_level();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location]
FROM [Gear] AS [g]
INNER JOIN [CogTag] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
WHERE [g].[Discriminator] IN ('Officer', 'Gear')
ORDER BY [c].[Name]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
INNER JOIN (
    SELECT DISTINCT [c].[Name]
    FROM [Gear] AS [g]
    INNER JOIN [CogTag] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
    INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
    WHERE [g].[Discriminator] IN ('Officer', 'Gear')
) AS [c] ON [g].[AssignedCityName] = [c].[Name]
WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
ORDER BY [c].[Name]",
                Sql);
        }

        public override void Include_with_join_and_inheritance1()
        {
            base.Include_with_join_and_inheritance1();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Include_with_join_and_inheritance2()
        {
            base.Include_with_join_and_inheritance2();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Include_with_join_and_inheritance3()
        {
            base.Include_with_join_and_inheritance3();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Where_enum()
        {
            base.Where_enum();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN ('Officer', 'Gear') AND ([g].[Rank] = 2)",
                Sql);
        }

        public override void Where_nullable_enum_with_constant()
        {
            base.Where_nullable_enum_with_constant();

            Assert.Equal(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE [w].[AmmunitionType] = 1",
                Sql);
        }

        public override void Where_nullable_enum_with_null_constant()
        {
            base.Where_nullable_enum_with_null_constant();

            Assert.Equal(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE [w].[AmmunitionType] IS NULL",
                Sql);
        }

        public override void Where_nullable_enum_with_non_nullable_parameter()
        {
            base.Where_nullable_enum_with_non_nullable_parameter();

            Assert.Equal(
                @"@__p_0: 1

SELECT [w].[Id], [w].[AmmunitionType], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE [w].[AmmunitionType] = @__p_0",
                Sql);
        }

        public override void Where_nullable_enum_with_nullable_parameter()
        {
            base.Where_nullable_enum_with_nullable_parameter();

            Assert.Equal(
                @"@__p_0: 1

SELECT [w].[Id], [w].[AmmunitionType], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE [w].[AmmunitionType] = @__p_0

SELECT [w].[Id], [w].[AmmunitionType], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE [w].[AmmunitionType] IS NULL",
                Sql);
        }

        public override void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar()
        {
            base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar();

            Assert.Equal(
                @"SELECT [ct1].[Id], [ct1].[GearNickName], [ct1].[GearSquadId], [ct1].[Note], [ct2].[Id], [ct2].[GearNickName], [ct2].[GearSquadId], [ct2].[Note]
FROM [CogTag] AS [ct1]
INNER JOIN [Gear] AS [ct1.Gear] ON ([ct1].[GearNickName] = [ct1.Gear].[Nickname]) AND ([ct1].[GearSquadId] = [ct1.Gear].[SquadId])
CROSS JOIN [CogTag] AS [ct2]
INNER JOIN [Gear] AS [ct2.Gear] ON ([ct2].[GearNickName] = [ct2.Gear].[Nickname]) AND ([ct2].[GearSquadId] = [ct2.Gear].[SquadId])
WHERE [ct1.Gear].[Nickname] = [ct2.Gear].[Nickname]",
                Sql);
        }

        public override void Select_Singleton_Navigation_With_Member_Access()
        {
            base.Select_Singleton_Navigation_With_Member_Access();

            Assert.Equal(
                @"SELECT [ct.Gear].[Nickname], [ct.Gear].[SquadId], [ct.Gear].[AssignedCityName], [ct.Gear].[CityOrBirthName], [ct.Gear].[Discriminator], [ct.Gear].[FullName], [ct.Gear].[LeaderNickname], [ct.Gear].[LeaderSquadId], [ct.Gear].[Rank]
FROM [CogTag] AS [ct]
INNER JOIN [Gear] AS [ct.Gear] ON ([ct].[GearNickName] = [ct.Gear].[Nickname]) AND ([ct].[GearSquadId] = [ct.Gear].[SquadId])
WHERE ([ct.Gear].[Nickname] = 'Marcus') AND ([ct.Gear].[CityOrBirthName] <> 'Ephyra')",
                Sql);
        }

        public override void Select_Where_Navigation()
        {
            base.Select_Where_Navigation();

            Assert.Equal(
                @"SELECT [ct].[Id], [ct].[GearNickName], [ct].[GearSquadId], [ct].[Note]
FROM [CogTag] AS [ct]
INNER JOIN [Gear] AS [ct.Gear] ON ([ct].[GearNickName] = [ct.Gear].[Nickname]) AND ([ct].[GearSquadId] = [ct.Gear].[SquadId])
WHERE [ct.Gear].[Nickname] = 'Marcus'",
                Sql);
        }

        public override void Select_Where_Navigation_Client()
        {
            base.Select_Where_Navigation_Client();

            Assert.Equal(
                @"SELECT [o].[Id], [o].[GearNickName], [o].[GearSquadId], [o].[Note], [o.Gear].[Nickname], [o.Gear].[SquadId], [o.Gear].[AssignedCityName], [o.Gear].[CityOrBirthName], [o.Gear].[Discriminator], [o.Gear].[FullName], [o.Gear].[LeaderNickname], [o.Gear].[LeaderSquadId], [o.Gear].[Rank]
FROM [CogTag] AS [o]
INNER JOIN [Gear] AS [o.Gear] ON ([o].[GearNickName] = [o.Gear].[Nickname]) AND ([o].[GearSquadId] = [o.Gear].[SquadId])",
                Sql);
        }

        public override void Select_Where_Navigation_Equals_Navigation()
        {
            base.Select_Where_Navigation_Equals_Navigation();

            Assert.Equal(
                @"SELECT [ct1].[Id], [ct1].[GearNickName], [ct1].[GearSquadId], [ct1].[Note], [ct2].[Id], [ct2].[GearNickName], [ct2].[GearSquadId], [ct2].[Note]
FROM [CogTag] AS [ct1]
CROSS JOIN [CogTag] AS [ct2]
WHERE (([ct1].[GearNickName] = [ct2].[GearNickName]) OR ([ct1].[GearNickName] IS NULL AND [ct2].[GearNickName] IS NULL)) AND (([ct1].[GearSquadId] = [ct2].[GearSquadId]) OR ([ct1].[GearSquadId] IS NULL AND [ct2].[GearSquadId] IS NULL))",
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
                @"SELECT [ct1].[Id], [ct2].[Id]
FROM [CogTag] AS [ct1]
INNER JOIN [Gear] AS [ct1.Gear] ON ([ct1].[GearNickName] = [ct1.Gear].[Nickname]) AND ([ct1].[GearSquadId] = [ct1.Gear].[SquadId])
CROSS JOIN [CogTag] AS [ct2]
INNER JOIN [Gear] AS [ct2.Gear] ON ([ct2].[GearNickName] = [ct2.Gear].[Nickname]) AND ([ct2].[GearSquadId] = [ct2.Gear].[SquadId])
WHERE [ct1.Gear].[Nickname] = [ct2.Gear].[Nickname]",
                Sql);
        }

        public override void Singleton_Navigation_With_Member_Access()
        {
            base.Singleton_Navigation_With_Member_Access();

            Assert.Equal(
                @"SELECT [ct.Gear].[CityOrBirthName]
FROM [CogTag] AS [ct]
INNER JOIN [Gear] AS [ct.Gear] ON ([ct].[GearNickName] = [ct.Gear].[Nickname]) AND ([ct].[GearSquadId] = [ct.Gear].[SquadId])
WHERE ([ct.Gear].[Nickname] = 'Marcus') AND ([ct.Gear].[CityOrBirthName] <> 'Ephyra')",
                Sql);
        }

        public override void GroupJoin_Composite_Key()
        {
            base.GroupJoin_Composite_Key();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [CogTag] AS [ct]
LEFT JOIN [Gear] AS [g] ON ([ct].[GearNickName] = [g].[Nickname]) AND ([ct].[GearSquadId] = [g].[SquadId])
ORDER BY [ct].[GearNickName], [ct].[GearSquadId]",
                Sql);
        }

        public GearsOfWarQuerySqlServerTest(GearsOfWarQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        protected override void ClearLog() => TestSqlLoggerFactory.Reset();

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
