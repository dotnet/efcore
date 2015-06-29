// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class GearsOfWarQuerySqlServerTest : GearsOfWarQueryTestBase<SqlServerTestStore, GearsOfWarQuerySqlServerFixture>
    {
        public GearsOfWarQuerySqlServerTest(GearsOfWarQuerySqlServerFixture fixture)
            : base(fixture)
        {
        }

        protected override void ClearLog()
        {
            TestSqlLoggerFactory.Reset();
        }

        public override void Include_multiple_one_to_one_and_one_to_many()
        {
            base.Include_multiple_one_to_one_and_one_to_many();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN [Gear] AS [g] ON ([t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId])
ORDER BY [g].[Nickname], [g].[SquadId]

SELECT [w].[Id], [w].[Name], [w].[OwnerNickname], [w].[OwnerSquadId], [w].[SynergyWithId]
FROM [Weapon] AS [w]
INNER JOIN (
    SELECT DISTINCT [g].[Nickname], [g].[SquadId]
    FROM [CogTag] AS [t]
    LEFT JOIN [Gear] AS [g] ON ([t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId])
) AS [g] ON ([w].[OwnerNickname] = [g].[Nickname] AND [w].[OwnerSquadId] = [g].[SquadId])
ORDER BY [g].[Nickname], [g].[SquadId]",
                Sql);
        }

        public override void Include_multiple_one_to_one_and_one_to_many_self_reference()
        {
            base.Include_multiple_one_to_one_and_one_to_many_self_reference();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN [Gear] AS [g] ON ([t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId])
ORDER BY [g].[Nickname], [g].[SquadId]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
INNER JOIN (
    SELECT DISTINCT [g].[Nickname], [g].[SquadId]
    FROM [CogTag] AS [t]
    LEFT JOIN [Gear] AS [g] ON ([t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId])
) AS [g0] ON ([g].[LeaderNickname] = [g0].[Nickname] AND [g].[LeaderSquadId] = [g0].[SquadId])
ORDER BY [g0].[Nickname], [g0].[SquadId]",
                Sql);
        }

        public override void Include_multiple_one_to_one_and_one_to_one_and_one_to_many()
        {
            base.Include_multiple_one_to_one_and_one_to_one_and_one_to_many();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [s].[Id], [s].[InternalNumber], [s].[Name]
FROM [CogTag] AS [t]
LEFT JOIN [Gear] AS [g] ON ([t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId])
LEFT JOIN [Squad] AS [s] ON [g].[SquadId] = [s].[Id]
ORDER BY [s].[Id]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
INNER JOIN (
    SELECT DISTINCT [s].[Id]
    FROM [CogTag] AS [t]
    LEFT JOIN [Gear] AS [g] ON ([t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId])
    LEFT JOIN [Squad] AS [s] ON [g].[SquadId] = [s].[Id]
) AS [s] ON [g].[SquadId] = [s].[Id]
ORDER BY [s].[Id]", Sql);
        }

        public override void Include_multiple_one_to_one_optional_and_one_to_one_required()
        {
            base.Include_multiple_one_to_one_optional_and_one_to_one_required();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [s].[Id], [s].[InternalNumber], [s].[Name]
FROM [CogTag] AS [t]
LEFT JOIN [Gear] AS [g] ON ([t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId])
LEFT JOIN [Squad] AS [s] ON [g].[SquadId] = [s].[Id]", Sql);
        }

        public override void Include_multiple_circular()
        {
            base.Include_multiple_circular();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location]
FROM [Gear] AS [g]
INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
ORDER BY [c].[Name]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
INNER JOIN (
    SELECT DISTINCT [c].[Name]
    FROM [Gear] AS [g]
    INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
) AS [c] ON [g].[AssignedCityName] = [c].[Name]
ORDER BY [c].[Name]",
                Sql);
        }

        public override void Include_multiple_circular_with_filter()
        {
            base.Include_multiple_circular_with_filter();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location]
FROM [Gear] AS [g]
INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
WHERE [g].[Nickname] = 'Marcus'
ORDER BY [c].[Name]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
INNER JOIN (
    SELECT DISTINCT [c].[Name]
    FROM [Gear] AS [g]
    INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
    WHERE [g].[Nickname] = 'Marcus'
) AS [c] ON [g].[AssignedCityName] = [c].[Name]
ORDER BY [c].[Name]",
                Sql);
        }

        public override void Include_multiple_include_then_include()
        {
            base.Include_multiple_include_then_include();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location], [c0].[Name], [c0].[Location], [c1].[Name], [c1].[Location], [c2].[Name], [c2].[Location]
FROM [Gear] AS [g]
LEFT JOIN [City] AS [c] ON [g].[AssignedCityName] = [c].[Name]
LEFT JOIN [City] AS [c0] ON [g].[AssignedCityName] = [c0].[Name]
INNER JOIN [City] AS [c1] ON [g].[CityOrBirthName] = [c1].[Name]
INNER JOIN [City] AS [c2] ON [g].[CityOrBirthName] = [c2].[Name]
ORDER BY [g].[Nickname], [c].[Name], [c0].[Name], [c1].[Name], [c2].[Name]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Id], [c].[GearNickName], [c].[GearSquadId], [c].[Note]
FROM [Gear] AS [g]
INNER JOIN (
    SELECT DISTINCT [g].[Nickname], [c].[Name], [c0].[Name] AS [Name0], [c1].[Name] AS [Name1], [c2].[Name] AS [Name2]
    FROM [Gear] AS [g]
    LEFT JOIN [City] AS [c] ON [g].[AssignedCityName] = [c].[Name]
    LEFT JOIN [City] AS [c0] ON [g].[AssignedCityName] = [c0].[Name]
    INNER JOIN [City] AS [c1] ON [g].[CityOrBirthName] = [c1].[Name]
    INNER JOIN [City] AS [c2] ON [g].[CityOrBirthName] = [c2].[Name]
) AS [c2] ON [g].[AssignedCityName] = [c2].[Name2]
LEFT JOIN [CogTag] AS [c] ON ([c].[GearNickName] = [g].[Nickname] AND [c].[GearSquadId] = [g].[SquadId])
ORDER BY [c2].[Nickname], [c2].[Name]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Id], [c].[GearNickName], [c].[GearSquadId], [c].[Note]
FROM [Gear] AS [g]
INNER JOIN (
    SELECT DISTINCT [g].[Nickname], [c].[Name], [c0].[Name] AS [Name0], [c1].[Name] AS [Name1]
    FROM [Gear] AS [g]
    LEFT JOIN [City] AS [c] ON [g].[AssignedCityName] = [c].[Name]
    LEFT JOIN [City] AS [c0] ON [g].[AssignedCityName] = [c0].[Name]
    INNER JOIN [City] AS [c1] ON [g].[CityOrBirthName] = [c1].[Name]
) AS [c1] ON [g].[CityOrBirthName] = [c1].[Name1]
LEFT JOIN [CogTag] AS [c] ON ([c].[GearNickName] = [g].[Nickname] AND [c].[GearSquadId] = [g].[SquadId])
ORDER BY [c1].[Nickname], [c1].[Name]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Id], [c].[GearNickName], [c].[GearSquadId], [c].[Note]
FROM [Gear] AS [g]
INNER JOIN (
    SELECT DISTINCT [g].[Nickname], [c].[Name], [c0].[Name] AS [Name0]
    FROM [Gear] AS [g]
    LEFT JOIN [City] AS [c] ON [g].[AssignedCityName] = [c].[Name]
    LEFT JOIN [City] AS [c0] ON [g].[AssignedCityName] = [c0].[Name]
) AS [c0] ON [g].[AssignedCityName] = [c0].[Name0]
LEFT JOIN [CogTag] AS [c] ON ([c].[GearNickName] = [g].[Nickname] AND [c].[GearSquadId] = [g].[SquadId])
ORDER BY [c0].[Nickname], [c0].[Name]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c0].[Id], [c0].[GearNickName], [c0].[GearSquadId], [c0].[Note]
FROM [Gear] AS [g]
INNER JOIN (
    SELECT DISTINCT [g].[Nickname], [c].[Name]
    FROM [Gear] AS [g]
    LEFT JOIN [City] AS [c] ON [g].[AssignedCityName] = [c].[Name]
) AS [c] ON [g].[CityOrBirthName] = [c].[Name]
LEFT JOIN [CogTag] AS [c0] ON ([c0].[GearNickName] = [g].[Nickname] AND [c0].[GearSquadId] = [g].[SquadId])
ORDER BY [c].[Nickname], [c].[Name]",
                Sql);
        }

        private static string Sql
        {
            get { return TestSqlLoggerFactory.Sql; }
        }
    }
}
