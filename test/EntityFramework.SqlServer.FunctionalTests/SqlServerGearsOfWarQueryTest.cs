// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerGearsOfWarQueryTest : GearsOfWarQueryTestBase<SqlServerTestStore, SqlServerGearsOfWarQueryFixture>
    {
        public SqlServerGearsOfWarQueryTest(SqlServerGearsOfWarQueryFixture fixture)
            : base(fixture)
        {
        }

        public override void Include_multiple_one_to_one_and_one_to_many()
        {
            base.Include_multiple_one_to_one_and_one_to_many();

            Assert.Equal(
                @"SELECT [t].[GearNickName], [t].[GearSquadId], [t].[Id], [t].[Note], [g].[AssignedCityId], [g].[CityOrBirthName], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Nickname], [g].[Rank], [g].[SquadId]
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
                @"SELECT [t].[GearNickName], [t].[GearSquadId], [t].[Id], [t].[Note], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Nickname], [g].[Rank], [g].[SquadId]
FROM [CogTag] AS [t]
LEFT JOIN [Gear] AS [g] ON ([t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId])
ORDER BY [g].[Nickname], [g].[SquadId]

SELECT [g].[AssignedCityName], [g].[CityOrBirthName], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Nickname], [g].[Rank], [g].[SquadId]
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
                @"TBD", Sql);
        }

        public override void Include_multiple_one_to_one_optional_and_one_to_one_required()
        {
            base.Include_multiple_one_to_one_optional_and_one_to_one_required();

            Assert.Equal(
                @"TBD", Sql);
        }

        public override void Include_multiple_circular()
        {
            base.Include_multiple_circular();

            Assert.Equal(
                @"SELECT [g].[AssignedCityName], [g].[CityOrBirthName], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Nickname], [g].[Rank], [g].[SquadId], [c].[Location], [c].[Name]
FROM [Gear] AS [g]
INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
ORDER BY [c].[Name]

SELECT [g].[AssignedCityName], [g].[CityOrBirthName], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Nickname], [g].[Rank], [g].[SquadId]
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
                @"SELECT [g].[AssignedCityName], [g].[CityOrBirthName], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Nickname], [g].[Rank], [g].[SquadId], [c].[Location], [c].[Name]
FROM [Gear] AS [g]
INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
WHERE [g].[Nickname] = @p0
ORDER BY [c].[Name]

SELECT [g].[AssignedCityName], [g].[CityOrBirthName], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Nickname], [g].[Rank], [g].[SquadId]
FROM [Gear] AS [g]
INNER JOIN (
    SELECT DISTINCT [c].[Name]
    FROM [Gear] AS [g]
    INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
    WHERE [g].[Nickname] = @p0
) AS [c] ON [g].[AssignedCityName] = [c].[Name]
ORDER BY [c].[Name]",
                Sql);
        }

        private static string Sql
        {
            get { return TestSqlLoggerFactory.Sql; }
        }
    }
}
