// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query;

[SqlServerCondition(SqlServerCondition.SupportsTemporalTablesCascadeDelete)]
public class TemporalGearsOfWarQuerySqlServerTest : GearsOfWarQueryRelationalTestBase<TemporalGearsOfWarQuerySqlServerFixture>
{
#pragma warning disable IDE0060 // Remove unused parameter
    public TemporalGearsOfWarQuerySqlServerTest(TemporalGearsOfWarQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
#pragma warning restore IDE0060 // Remove unused parameter
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
    {
        serverQueryExpression = base.RewriteServerQueryExpression(serverQueryExpression);

        var temporalEntityTypes = new List<Type>
        {
            typeof(City),
            typeof(CogTag),
            typeof(Faction),
            typeof(LocustHorde),
            typeof(Gear),
            typeof(Officer),
            typeof(LocustLeader),
            typeof(LocustCommander),
            typeof(LocustHighCommand),
            typeof(Mission),
            typeof(Squad),
            typeof(SquadMission),
            typeof(Weapon),
        };

        var rewriter = new TemporalPointInTimeQueryRewriter(Fixture.ChangesDate, temporalEntityTypes);

        return rewriter.Visit(serverQueryExpression);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Include_where_list_contains_navigation(bool async)
    {
        Assert.Equal(
            "0",
            (await Assert.ThrowsAsync<EqualException>(
                () => base.Include_where_list_contains_navigation(async))).Actual);

        AssertSql(
"""
SELECT [t].[Id]
FROM [Tags] AS [t]
""",
            //
"""
@__tags_0='[]' (Size = 4000)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [t] ON [g].[Nickname] = [t].[GearNickName] AND [g].[SquadId] = [t].[GearSquadId]
WHERE [t].[Id] IS NOT NULL AND EXISTS (
    SELECT 1
    FROM OpenJson(@__tags_0) AS [t0]
    WHERE CAST([t0].[value] AS uniqueidentifier) = [t].[Id] OR ([t0].[value] IS NULL AND [t].[Id] IS NULL))
""");
    }

    public override async Task Include_where_list_contains_navigation2(bool async)
    {
        Assert.Equal(
            "0",
            (await Assert.ThrowsAsync<EqualException>(
                () => base.Include_where_list_contains_navigation2(async))).Actual);

        AssertSql(
"""
SELECT [t].[Id]
FROM [Tags] AS [t]
""",
            //
"""
@__tags_0='[]' (Size = 4000)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart]
FROM [Gears] AS [g]
INNER JOIN [Cities] AS [c] ON [g].[CityOfBirthName] = [c].[Name]
LEFT JOIN [Tags] AS [t] ON [g].[Nickname] = [t].[GearNickName] AND [g].[SquadId] = [t].[GearSquadId]
WHERE [c].[Location] IS NOT NULL AND EXISTS (
    SELECT 1
    FROM OpenJson(@__tags_0) AS [t0]
    WHERE CAST([t0].[value] AS uniqueidentifier) = [t].[Id] OR ([t0].[value] IS NULL AND [t].[Id] IS NULL))
""");
    }

    public override async Task Navigation_accessed_twice_outside_and_inside_subquery(bool async)
    {
        Assert.Equal(
            "0",
            (await Assert.ThrowsAsync<EqualException>(
                () => base.Navigation_accessed_twice_outside_and_inside_subquery(async))).Actual);

        AssertSql(
"""
SELECT [t].[Id]
FROM [Tags] AS [t]
""",
            //
"""
@__tags_0='[]' (Size = 4000)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Tags] AS [t] ON [g].[Nickname] = [t].[GearNickName] AND [g].[SquadId] = [t].[GearSquadId]
WHERE [t].[Id] IS NOT NULL AND EXISTS (
    SELECT 1
    FROM OpenJson(@__tags_0) AS [t0]
    WHERE CAST([t0].[value] AS uniqueidentifier) = [t].[Id] OR ([t0].[value] IS NULL AND [t].[Id] IS NULL))
""");
    }

    public override async Task Query_reusing_parameter_with_inner_query_doesnt_declare_duplicate_parameter(bool async)
    {
        // Test infra issue
        Assert.Equal(
            "0",
            (await Assert.ThrowsAsync<EqualException>(
                () => base.Query_reusing_parameter_with_inner_query_doesnt_declare_duplicate_parameter(async))).Actual);

        AssertSql(
"""
@__squadId_0='1'

SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Rank]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    INNER JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [g].[SquadId] = [s].[Id]
    WHERE EXISTS (
        SELECT 1
        FROM [Squads] AS [s0]
        WHERE [s0].[Id] = @__squadId_0 AND [s0].[Id] = [s].[Id])
    UNION ALL
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    INNER JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s1] ON [g0].[SquadId] = [s1].[Id]
    WHERE EXISTS (
        SELECT 1
        FROM [Squads] AS [s2]
        WHERE [s2].[Id] = @__squadId_0 AND [s2].[Id] = [s1].[Id])
) AS [t]
ORDER BY [t].[FullName]
""");
    }

    public override async Task
        Multiple_includes_with_client_method_around_entity_and_also_projecting_included_collection() // Test infra issue
    {
        // Test infra issue
        await Assert.ThrowsAsync<SingleException>(
            () => base.Multiple_includes_with_client_method_around_entity_and_also_projecting_included_collection());

        AssertSql(
"""
SELECT [s].[Name], [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[PeriodEnd], [s].[PeriodStart], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Rank], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[PeriodEnd0], [t].[PeriodStart0], [t].[SynergyWithId]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd] AS [PeriodEnd0], [w].[PeriodStart] AS [PeriodStart0], [w].[SynergyWithId]
    FROM [Gears] AS [g]
    LEFT JOIN [Weapons] AS [w] ON [g].[FullName] = [w].[OwnerFullName]
) AS [t] ON [s].[Id] = [t].[SquadId]
WHERE [s].[Name] = N'Delta'
ORDER BY [s].[Id], [t].[Nickname], [t].[SquadId]
""");
    }

    public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1() // Test infra issue
    {
        // Test infra issue
        Assert.Equal(
            "[]",
            Assert.Throws<EqualException>(
                () => base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1()).Actual);

        AssertSql(
"""
SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Gears] AS [g]
LEFT JOIN [Gears] AS [g0] ON [g].[LeaderNickname] = [g0].[Nickname]
LEFT JOIN [Weapons] AS [w] ON [g].[FullName] = [w].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId]
""");
    }

    public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2() // Test infra issue
    {
        // Test infra issue
        Assert.Equal(
            "[]",
            Assert.Throws<EqualException>(
                () => base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2()).Actual);

        AssertSql(
"""
SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank], [g].[Nickname], [g].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] AS [g]
LEFT JOIN [Gears] AS [g0] ON [g].[LeaderNickname] = [g0].[Nickname]
LEFT JOIN [Weapons] AS [w] ON [g0].[FullName] = [w].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId]
""");
    }

    public override void Byte_array_filter_by_length_parameter_compiled()
    {
        // Test infra issue
        Assert.Equal(
            "0",
            Assert.Throws<EqualException>(
                () => base.Byte_array_filter_by_length_parameter_compiled()).Actual);

        AssertSql(
"""
@__byteArrayParam='0x2A80' (Size = 8000)

SELECT COUNT(*)
FROM [Squads] AS [s]
WHERE CAST(DATALENGTH([s].[Banner]) AS int) = CAST(DATALENGTH(@__byteArrayParam) AS int)
""");
    }

    public override async Task Where_DateOnly_Year(bool async)
    {
        await base.Where_DateOnly_Year(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE DATEPART(year, [m].[Date]) = 1990
""");
    }

    public override async Task Where_DateOnly_Month(bool async)
    {
        await base.Where_DateOnly_Month(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE DATEPART(month, [m].[Date]) = 11
""");
    }

    public override async Task Where_DateOnly_Day(bool async)
    {
        await base.Where_DateOnly_Day(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE DATEPART(day, [m].[Date]) = 10
""");
    }

    public override async Task Where_DateOnly_DayOfYear(bool async)
    {
        await base.Where_DateOnly_DayOfYear(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE DATEPART(dayofyear, [m].[Date]) = 314
""");
    }

    public override async Task Where_DateOnly_DayOfWeek(bool async)
    {
        await AssertTranslationFailed(() => base.Where_DateOnly_DayOfWeek(async));

        AssertSql();
    }

    public override async Task Where_DateOnly_AddYears(bool async)
    {
        await base.Where_DateOnly_AddYears(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE DATEADD(year, CAST(3 AS int), [m].[Date]) = '1993-11-10'
""");
    }

    public override async Task Where_DateOnly_AddMonths(bool async)
    {
        await base.Where_DateOnly_AddMonths(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE DATEADD(month, CAST(3 AS int), [m].[Date]) = '1991-02-10'
""");
    }

    public override async Task Where_DateOnly_AddDays(bool async)
    {
        await base.Where_DateOnly_AddDays(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE DATEADD(day, CAST(3 AS int), [m].[Date]) = '1990-11-13'
""");
    }

    public override async Task Where_TimeOnly_Hour(bool async)
    {
        await base.Where_TimeOnly_Hour(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE DATEPART(hour, [m].[Time]) = 10
""");
    }

    public override async Task Where_TimeOnly_Minute(bool async)
    {
        await base.Where_TimeOnly_Minute(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE DATEPART(minute, [m].[Time]) = 15
""");
    }

    public override async Task Where_TimeOnly_Second(bool async)
    {
        await base.Where_TimeOnly_Second(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE DATEPART(second, [m].[Time]) = 50
""");
    }

    public override async Task Where_TimeOnly_Millisecond(bool async)
    {
        await base.Where_TimeOnly_Millisecond(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE DATEPART(millisecond, [m].[Time]) = 500
""");
    }

    public override async Task Where_TimeOnly_AddHours(bool async)
    {
        await base.Where_TimeOnly_AddHours(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE DATEADD(hour, CAST(3.0E0 AS int), [m].[Time]) = '13:15:50.5'
""");
    }

    public override async Task Where_TimeOnly_AddMinutes(bool async)
    {
        await base.Where_TimeOnly_AddMinutes(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE DATEADD(minute, CAST(3.0E0 AS int), [m].[Time]) = '10:18:50.5'
""");
    }

    public override async Task Where_TimeOnly_Add_TimeSpan(bool async)
    {
        await AssertTranslationFailed(() => base.Where_TimeOnly_Add_TimeSpan(async));

        AssertSql();
    }

    public override async Task Where_TimeOnly_IsBetween(bool async)
    {
        await base.Where_TimeOnly_IsBetween(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE CASE
    WHEN [m].[Time] >= '10:00:00' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END & CASE
    WHEN [m].[Time] < '11:00:00' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CAST(1 AS bit)
""");
    }

    public override async Task Where_TimeOnly_subtract_TimeOnly(bool async)
    {
        await AssertTranslationFailed(() => base.Where_TimeOnly_subtract_TimeOnly(async));

        AssertSql();
    }

    public override async Task Basic_query_gears(bool async)
    {
        await base.Basic_query_gears(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Accessing_derived_property_using_hard_and_soft_cast(bool async)
    {
        await base.Accessing_derived_property_using_hard_and_soft_cast(async);

        AssertSql(
"""
SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
WHERE [l].[Discriminator] = N'LocustCommander' AND ([l].[HighCommandId] <> 0 OR [l].[HighCommandId] IS NULL)
""",
            //
"""
SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
WHERE [l].[Discriminator] = N'LocustCommander' AND ([l].[HighCommandId] <> 0 OR [l].[HighCommandId] IS NULL)
""");
    }

    public override async Task Accessing_property_of_optional_navigation_in_child_projection_works(bool async)
    {
        await base.Accessing_property_of_optional_navigation_in_child_projection_works(async);

        AssertSql(
"""
SELECT CASE
    WHEN [g].[Nickname] IS NOT NULL AND [g].[SquadId] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [t].[Id], [g].[Nickname], [g].[SquadId], [t0].[Nickname], [t0].[Id], [t0].[SquadId]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
LEFT JOIN (
    SELECT [g0].[Nickname], [w].[Id], [g0].[SquadId], [w].[OwnerFullName]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [w].[OwnerFullName] = [g0].[FullName]
) AS [t0] ON [g].[FullName] = [t0].[OwnerFullName]
ORDER BY [t].[Note], [t].[Id], [g].[Nickname], [g].[SquadId], [t0].[Id], [t0].[Nickname]
""");
    }

    public override async Task Accessing_reference_navigation_collection_composition_generates_single_query(bool async)
    {
        await base.Accessing_reference_navigation_collection_composition_generates_single_query(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [t].[Id], [t].[IsAutomatic], [t].[Name], [t].[Id0]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [w].[Id], [w].[IsAutomatic], [w0].[Name], [w0].[Id] AS [Id0], [w].[OwnerFullName]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Id]
""");
    }

    public override async Task All_with_optional_navigation_is_translated_to_sql(bool async)
    {
        await base.All_with_optional_navigation_is_translated_to_sql(async);

        AssertSql(
"""
SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
        LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[Nickname] = [t].[GearNickName] AND [g].[SquadId] = [t].[GearSquadId]
        WHERE [t].[Note] = N'Foo' AND [t].[Note] IS NOT NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Anonymous_projection_take_followed_by_projecting_single_element_from_collection_navigation(bool async)
    {
        await base.Anonymous_projection_take_followed_by_projecting_single_element_from_collection_navigation(async);

        AssertSql(
"""
@__p_0='25'

SELECT [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[SynergyWithId]
FROM (
    SELECT TOP(@__p_0) [g].[FullName]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
) AS [t]
LEFT JOIN (
    SELECT [t1].[Id], [t1].[AmmunitionType], [t1].[IsAutomatic], [t1].[Name], [t1].[OwnerFullName], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[SynergyWithId]
    FROM (
        SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId], ROW_NUMBER() OVER(PARTITION BY [w].[OwnerFullName] ORDER BY [w].[Id]) AS [row]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    ) AS [t1]
    WHERE [t1].[row] <= 1
) AS [t0] ON [t].[FullName] = [t0].[OwnerFullName]
""");
    }

    public override async Task Any_with_optional_navigation_as_subquery_predicate_is_translated_to_sql(bool async)
    {
        await base.Any_with_optional_navigation_as_subquery_predicate_is_translated_to_sql(async);

        AssertSql(
"""
SELECT [s].[Name]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
WHERE NOT (EXISTS (
    SELECT 1
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[Nickname] = [t].[GearNickName] AND [g].[SquadId] = [t].[GearSquadId]
    WHERE [s].[Id] = [g].[SquadId] AND [t].[Note] = N'Dom''s Tag'))
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Set_operation_on_temporal_same_ops(bool async)
    {
        using var ctx = CreateContext();
        var date = new DateTime(2015, 1, 1);
        var query = ctx.Set<Gear>().TemporalAsOf(date).Where(g => g.HasSoulPatch).Concat(ctx.Set<Gear>().TemporalAsOf(date));
        var expected = async
            ? await query.ToListAsync()
            : query.ToList();

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2015-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
UNION ALL
SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2015-01-01T00:00:00.0000000' AS [g0]
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Set_operation_with_inheritance_on_temporal_same_ops(bool async)
    {
        using var ctx = CreateContext();
        var date = new DateTime(2015, 1, 1);
        var query = ctx.Set<Officer>().TemporalAsOf(date).Concat(ctx.Set<Officer>().TemporalAsOf(date));
        var expected = async
            ? await query.ToListAsync()
            : query.ToList();

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2015-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Discriminator] = N'Officer'
UNION ALL
SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2015-01-01T00:00:00.0000000' AS [g0]
WHERE [g0].[Discriminator] = N'Officer'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Set_operation_on_temporal_different_dates(bool async)
    {
        using var ctx = CreateContext();
        var date1 = new DateTime(2015, 1, 1);
        var date2 = new DateTime(2018, 1, 1);
        var query = ctx.Set<Gear>().TemporalAsOf(date1).Where(g => g.HasSoulPatch).Concat(ctx.Set<Gear>().TemporalAsOf(date2));

        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => async
                ? query.ToListAsync()
                : Task.FromResult(query.ToList()))).Message;

        Assert.Equal(SqlServerStrings.TemporalSetOperationOnMismatchedSources(nameof(Gear)), message);

        AssertSql();
    }

    public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result(bool async)
    {
        await base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result(async);

        AssertSql(
"""
SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Rank], [g].[Nickname], [g].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[PeriodEnd], [w0].[PeriodStart], [w0].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    WHERE [g0].[Discriminator] = N'Officer'
) AS [t] ON [g].[LeaderNickname] = [t].[Nickname]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [t].[FullName] = [w].[OwnerFullName]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0] ON [g].[FullName] = [w0].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Nickname], [t].[SquadId], [w].[Id]
""");
    }

    public override async Task Select_null_propagation_negative3(bool async)
    {
        await base.Select_null_propagation_negative3(async);

        AssertSql(
"""
SELECT [g0].[Nickname], CASE
    WHEN [g0].[Nickname] IS NOT NULL AND [g0].[SquadId] IS NOT NULL THEN CASE
        WHEN [g0].[LeaderNickname] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    ELSE NULL
END AS [Condition]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[HasSoulPatch] = CAST(1 AS bit)
ORDER BY [g0].[Nickname]
""");
    }

    public override async Task String_concat_with_null_conditional_argument(bool async)
    {
        await base.String_concat_with_null_conditional_argument(async);

        AssertSql(
"""
SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[PeriodEnd], [w0].[PeriodStart], [w0].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY COALESCE([w0].[Name], N'') + CAST(5 AS nvarchar(max))
""");
    }

    public override async Task Where_null_parameter_is_not_null(bool async)
    {
        await base.Where_null_parameter_is_not_null(async);

        AssertSql(
"""
@__p_0='False'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE @__p_0 = CAST(1 AS bit)
""");
    }

    public override async Task GroupBy_Property_Include_Select_LongCount(bool async)
    {
        await base.GroupBy_Property_Include_Select_LongCount(async);

        AssertSql(
"""
SELECT COUNT_BIG(*)
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
GROUP BY [g].[Rank]
""");
    }

    public override async Task Projecting_property_converted_to_nullable_into_element_init(bool async)
    {
        await base.Projecting_property_converted_to_nullable_into_element_init(async);

        AssertSql(
"""
SELECT CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN CAST(LEN([g].[Nickname]) AS int)
    ELSE NULL
END, CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[SquadId]
    ELSE NULL
END, CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[SquadId]
    ELSE NULL
END + 1
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[Nickname]
    ELSE NULL
END IS NOT NULL
ORDER BY [t].[Note]
""");
    }

    public override async Task OfTypeNav1(bool async)
    {
        await base.OfTypeNav1(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[Nickname] = [t].[GearNickName] AND [g].[SquadId] = [t].[GearSquadId]
LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t0] ON [g].[Nickname] = [t0].[GearNickName] AND [g].[SquadId] = [t0].[GearSquadId]
WHERE ([t].[Note] <> N'Foo' OR [t].[Note] IS NULL) AND [g].[Discriminator] = N'Officer' AND ([t0].[Note] <> N'Bar' OR [t0].[Note] IS NULL)
""");
    }

    public override async Task Select_subquery_int_with_outside_cast_and_coalesce(bool async)
    {
        await base.Select_subquery_int_with_outside_cast_and_coalesce(async);

        AssertSql(
"""
SELECT COALESCE((
    SELECT TOP(1) [w].[Id]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]), 0, 42)
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Include_with_join_and_inheritance1(bool async)
    {
        await base.Include_with_join_and_inheritance1(async);

        AssertSql(
"""
SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Rank], [c].[Name], [c].[Location], [c].[Nation], [c].[PeriodEnd], [c].[PeriodStart]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
INNER JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [g].[Discriminator] = N'Officer'
) AS [t0] ON [t].[GearSquadId] = [t0].[SquadId] AND [t].[GearNickName] = [t0].[Nickname]
INNER JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [t0].[CityOfBirthName] = [c].[Name]
""");
    }

    public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result(bool async)
    {
        await base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result(async);

        AssertSql(
"""
SELECT CASE
    WHEN [g0].[Nickname] IS NOT NULL AND [g0].[SquadId] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank], [g].[Nickname], [g].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[PeriodEnd], [w0].[PeriodStart], [w0].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[LeaderNickname] = [g0].[Nickname]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g0].[FullName] = [w].[OwnerFullName]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0] ON [g].[FullName] = [w0].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId], [w].[Id]
""");
    }

    public override async Task Where_datetimeoffset_month_component(bool async)
    {
        await base.Where_datetimeoffset_month_component(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE DATEPART(month, [m].[Timeline]) = 1
""");
    }

    public override async Task Where_datetimeoffset_milliseconds_parameter_and_constant(bool async)
    {
        await base.Where_datetimeoffset_milliseconds_parameter_and_constant(async);

        AssertSql(
"""
SELECT COUNT(*)
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE [m].[Timeline] = '1902-01-02T10:00:00.1234567+01:30'
""");
    }

    public override async Task Join_on_entity_qsre_keys_inner_key_is_navigation_composite_key(bool async)
    {
        await base.Join_on_entity_qsre_keys_inner_key_is_navigation_composite_key(async);

        AssertSql(
"""
SELECT [g].[Nickname], [t0].[Note]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN (
    SELECT [t].[Note], [g0].[Nickname], [g0].[SquadId]
    FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
    LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [t].[GearNickName] = [g0].[Nickname] AND [t].[GearSquadId] = [g0].[SquadId]
    WHERE [t].[Note] IN (N'Cole''s Tag', N'Dom''s Tag')
) AS [t0] ON [g].[Nickname] = [t0].[Nickname] AND [g].[SquadId] = [t0].[SquadId]
""");
    }

    public override async Task Where_subquery_left_join_firstordefault_boolean(bool async)
    {
        await base.Where_subquery_left_join_firstordefault_boolean(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit) AND (
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    LEFT JOIN (
        SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[PeriodEnd], [w0].[PeriodStart], [w0].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0]
        WHERE [g].[FullName] = [w0].[OwnerFullName]
    ) AS [t] ON [w].[Id] = [t].[Id]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]) = CAST(1 AS bit)
""");
    }

    public override async Task Double_order_by_on_is_null(bool async)
    {
        await base.Double_order_by_on_is_null(async);

        AssertSql(
"""
SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[PeriodEnd], [w0].[PeriodStart], [w0].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY CASE
    WHEN [w0].[Name] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Navigation_access_on_derived_entity_using_cast(bool async)
    {
        await base.Navigation_access_on_derived_entity_using_cast(async);

        AssertSql(
"""
SELECT [f].[Name], [t].[ThreatLevel] AS [Threat]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
LEFT JOIN (
    SELECT [l].[Name], [l].[ThreatLevel]
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
ORDER BY [f].[Name]
""");
    }

    public override async Task Cast_result_operator_on_subquery_is_properly_lifted_to_a_convert(bool async)
    {
        await base.Cast_result_operator_on_subquery_is_properly_lifted_to_a_convert(async);

        AssertSql(
"""
SELECT [f].[Eradicated]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
""");
    }

    public override async Task GroupBy_with_boolean_groupin_key_thru_navigation_access(bool async)
    {
        await base.GroupBy_with_boolean_groupin_key_thru_navigation_access(async);

        AssertSql(
"""
SELECT [g].[HasSoulPatch], LOWER([s].[Name]) AS [Name]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
LEFT JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [g].[SquadId] = [s].[Id]
GROUP BY [g].[HasSoulPatch], [s].[Name]
""");
    }

    public override async Task Byte_array_contains_literal(bool async)
    {
        await base.Byte_array_contains_literal(async);

        AssertSql(
"""
SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
WHERE CHARINDEX(0x01, [s].[Banner]) > 0
""");
    }

    public override async Task SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector(bool async)
    {
        await base.SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector(async);

        AssertSql(
"""
@__isAutomatic_0='True'

SELECT [g].[Nickname], [g].[FullName], CASE
    WHEN [t].[Id] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Collection]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [w].[Id], [w].[OwnerFullName]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [w].[IsAutomatic] = @__isAutomatic_0
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_predicate_negated_complex1(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_predicate_negated_complex1(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE CASE
    WHEN [g].[HasSoulPatch] = CAST(1 AS bit) THEN CAST(1 AS bit)
    ELSE [g].[HasSoulPatch]
END = CAST(0 AS bit)
""");
    }

    public override async Task Double_order_by_on_Like(bool async)
    {
        await base.Double_order_by_on_Like(async);

        AssertSql(
"""
SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[PeriodEnd], [w0].[PeriodStart], [w0].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY CASE
    WHEN [w0].[Name] LIKE N'%Lancer' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Where_bitwise_and_nullable_enum_with_constant(bool async)
    {
        await base.Where_bitwise_and_nullable_enum_with_constant(async);

        AssertSql(
"""
SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE [w].[AmmunitionType] & 1 > 0
""");
    }

    public override async Task Where_subquery_distinct_firstordefault_boolean_with_pushdown(bool async)
    {
        await base.Where_subquery_distinct_firstordefault_boolean_with_pushdown(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit) AND (
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
    ) AS [t]
    ORDER BY [t].[Id]) = CAST(1 AS bit)
""");
    }

    public override async Task Correlated_collection_with_top_level_Count(bool async)
    {
        await base.Correlated_collection_with_top_level_Count(async);

        AssertSql(
"""
SELECT COUNT(*)
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Subquery_with_result_operator_is_not_lifted(bool async)
    {
        await base.Subquery_with_result_operator_is_not_lifted(async);

        AssertSql(
"""
@__p_0='2'

SELECT [t].[FullName]
FROM (
    SELECT TOP(@__p_0) [g].[FullName], [g].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [g].[HasSoulPatch] = CAST(0 AS bit)
    ORDER BY [g].[FullName]
) AS [t]
ORDER BY [t].[Rank]
""");
    }

    public override async Task Include_with_join_reference2(bool async)
    {
        await base.Include_with_join_reference2(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [c].[Name], [c].[Location], [c].[Nation], [c].[PeriodEnd], [c].[PeriodStart]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
INNER JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearSquadId] = [g].[SquadId] AND [t].[GearNickName] = [g].[Nickname]
INNER JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g].[CityOfBirthName] = [c].[Name]
""");
    }

    public override async Task Conditional_expression_with_test_being_simplified_to_constant_simple(bool async)
    {
        await base.Conditional_expression_with_test_being_simplified_to_constant_simple(async);

        AssertSql(
"""
@__prm_0='True'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE CASE
    WHEN [g].[HasSoulPatch] = @__prm_0 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CAST(1 AS bit)
""");
    }

    public override async Task Include_reference_on_derived_type_using_lambda_with_soft_cast(bool async)
    {
        await base.Include_reference_on_derived_type_using_lambda_with_soft_cast(async);

        AssertSql(
"""
SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [l].[DefeatedByNickname] = [g].[Nickname] AND [l].[DefeatedBySquadId] = [g].[SquadId]
""");
    }

    public override async Task Null_propagation_optimization1(bool async)
    {
        await base.Null_propagation_optimization1(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[LeaderNickname] = N'Marcus' AND [g].[LeaderNickname] IS NOT NULL
""");
    }

    public override async Task DateTimeOffset_DateAdd_AddMonths(bool async)
    {
        await base.DateTimeOffset_DateAdd_AddMonths(async);

        AssertSql(
"""
SELECT DATEADD(month, CAST(1 AS int), [m].[Timeline])
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
""");
    }

    public override async Task Where_datetimeoffset_minute_component(bool async)
    {
        await base.Where_datetimeoffset_minute_component(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE DATEPART(minute, [m].[Timeline]) = 0
""");
    }

    public override async Task Projecting_property_converted_to_nullable_with_conditional(bool async)
    {
        await base.Projecting_property_converted_to_nullable_with_conditional(async);

        AssertSql(
"""
SELECT CASE
    WHEN [t].[Note] <> N'K.I.A.' OR [t].[Note] IS NULL THEN CASE
        WHEN [t].[GearNickName] IS NOT NULL THEN [g].[SquadId]
        ELSE NULL
    END
    ELSE -1
END
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
""");
    }

    public override async Task Include_collection_with_complex_OrderBy2(bool async)
    {
        await base.Include_collection_with_complex_OrderBy2(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[Nickname] = [g0].[LeaderNickname] AND [g].[SquadId] = [g0].[LeaderSquadId]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY (
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]), [g].[Nickname], [g].[SquadId], [g0].[Nickname]
""");
    }

    public override async Task ToString_guid_property_projection(bool async)
    {
        await base.ToString_guid_property_projection(async);

        AssertSql(
"""
SELECT [t].[GearNickName] AS [A], CONVERT(varchar(36), [t].[Id]) AS [B]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
""");
    }

    public override async Task Group_by_nullable_property_and_project_the_grouping_key_HasValue(bool async)
    {
        await base.Group_by_nullable_property_and_project_the_grouping_key_HasValue(async);

        AssertSql(
"""
SELECT CASE
    WHEN [w].[SynergyWithId] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
GROUP BY [w].[SynergyWithId]
""");
    }

    public override async Task Correlated_collections_naked_navigation_with_ToArray(bool async)
    {
        await base.Correlated_collections_naked_navigation_with_ToArray(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] = [w].[OwnerFullName]
WHERE [g].[Nickname] <> N'Marcus'
ORDER BY [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_all(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_all(async);

        AssertSql(
"""
SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
        LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
        WHERE ([t].[Note] <> N'K.I.A.' OR [t].[Note] IS NULL) AND [g].[HasSoulPatch] = CAST(0 AS bit)) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Projecting_property_converted_to_nullable_with_addition_and_final_projection(bool async)
    {
        await base.Projecting_property_converted_to_nullable_with_addition_and_final_projection(async);

        AssertSql(
"""
SELECT [t].[Note], CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[SquadId]
    ELSE NULL
END + 1 AS [Value]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[Nickname]
    ELSE NULL
END IS NOT NULL
""");
    }

    public override async Task Multiple_order_bys_are_properly_lifted_from_subquery_created_by_include(bool async)
    {
        await base.Multiple_order_bys_are_properly_lifted_from_subquery_created_by_include(async);

        AssertSql(
"""
SELECT [g].[FullName]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(0 AS bit)
ORDER BY [g].[FullName]
""");
    }

    public override async Task Correlated_collections_nested_with_custom_ordering(bool async)
    {
        await base.Correlated_collections_nested_with_custom_ordering(async);

        AssertSql(
"""
SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t0].[FullName], [t0].[Nickname], [t0].[SquadId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [g0].[FullName], [g0].[Nickname], [g0].[SquadId], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[PeriodEnd], [t].[PeriodStart], [t].[SynergyWithId], [g0].[Rank], [g0].[LeaderNickname], [g0].[LeaderSquadId]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    LEFT JOIN (
        SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [w].[Name] <> N'Bar' OR [w].[Name] IS NULL
    ) AS [t] ON [g0].[FullName] = [t].[OwnerFullName]
    WHERE [g0].[FullName] <> N'Foo'
) AS [t0] ON [g].[Nickname] = [t0].[LeaderNickname] AND [g].[SquadId] = [t0].[LeaderSquadId]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[HasSoulPatch] DESC, [g].[Nickname], [g].[SquadId], [t0].[Rank], [t0].[Nickname], [t0].[SquadId], [t0].[IsAutomatic]
""");
    }

    public override async Task Correlated_collections_with_funky_orderby_complex_scenario2(bool async)
    {
        await base.Correlated_collections_with_funky_orderby_complex_scenario2(async);

        AssertSql(
"""
SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t0].[FullName], [t0].[Nickname], [t0].[SquadId], [t0].[Id], [t0].[Nickname0], [t0].[SquadId0], [t0].[Id0], [t0].[Nickname00], [t0].[HasSoulPatch], [t0].[SquadId00]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [g0].[FullName], [g0].[Nickname], [g0].[SquadId], [t].[Id], [t].[Nickname] AS [Nickname0], [t].[SquadId] AS [SquadId0], [t].[Id0], [t].[Nickname0] AS [Nickname00], [t].[HasSoulPatch], [t].[SquadId0] AS [SquadId00], [g0].[HasSoulPatch] AS [HasSoulPatch0], [t].[IsAutomatic], [t].[Name], [g0].[LeaderNickname], [g0].[LeaderSquadId]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    LEFT JOIN (
        SELECT [w].[Id], [g1].[Nickname], [g1].[SquadId], [s].[Id] AS [Id0], [g2].[Nickname] AS [Nickname0], [g2].[HasSoulPatch], [g2].[SquadId] AS [SquadId0], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g1] ON [w].[OwnerFullName] = [g1].[FullName]
        LEFT JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [g1].[SquadId] = [s].[Id]
        LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g2] ON [s].[Id] = [g2].[SquadId]
    ) AS [t] ON [g0].[FullName] = [t].[OwnerFullName]
) AS [t0] ON [g].[Nickname] = [t0].[LeaderNickname] AND [g].[SquadId] = [t0].[LeaderSquadId]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[HasSoulPatch], [g].[LeaderNickname], [g].[FullName], [g].[Nickname], [g].[SquadId], [t0].[FullName], [t0].[HasSoulPatch0] DESC, [t0].[Nickname], [t0].[SquadId], [t0].[IsAutomatic], [t0].[Name] DESC, [t0].[Id], [t0].[Nickname0], [t0].[SquadId0], [t0].[Id0], [t0].[Nickname00]
""");
    }

    public override async Task Union_with_collection_navigations(bool async)
    {
        await base.Union_with_collection_navigations(async);

        AssertSql(
"""
SELECT (
    SELECT COUNT(*)
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
        FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
        WHERE [g].[Nickname] = [g0].[LeaderNickname] AND [g].[SquadId] = [g0].[LeaderSquadId]
        UNION
        SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOfBirthName], [g1].[Discriminator], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[PeriodEnd], [g1].[PeriodStart], [g1].[Rank]
        FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g1]
        WHERE [g].[Nickname] = [g1].[LeaderNickname] AND [g].[SquadId] = [g1].[LeaderSquadId]
    ) AS [t])
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Discriminator] = N'Officer'
""");
    }

    public override async Task Correlated_collections_complex_scenario2(bool async)
    {
        await base.Correlated_collections_complex_scenario2(async);

        AssertSql(
"""
SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t0].[FullName], [t0].[Nickname], [t0].[SquadId], [t0].[Id], [t0].[Nickname0], [t0].[SquadId0], [t0].[Id0], [t0].[Nickname00], [t0].[HasSoulPatch], [t0].[SquadId00]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [g0].[FullName], [g0].[Nickname], [g0].[SquadId], [t].[Id], [t].[Nickname] AS [Nickname0], [t].[SquadId] AS [SquadId0], [t].[Id0], [t].[Nickname0] AS [Nickname00], [t].[HasSoulPatch], [t].[SquadId0] AS [SquadId00], [g0].[LeaderNickname], [g0].[LeaderSquadId]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    LEFT JOIN (
        SELECT [w].[Id], [g1].[Nickname], [g1].[SquadId], [s].[Id] AS [Id0], [g2].[Nickname] AS [Nickname0], [g2].[HasSoulPatch], [g2].[SquadId] AS [SquadId0], [w].[OwnerFullName]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g1] ON [w].[OwnerFullName] = [g1].[FullName]
        LEFT JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [g1].[SquadId] = [s].[Id]
        LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g2] ON [s].[Id] = [g2].[SquadId]
    ) AS [t] ON [g0].[FullName] = [t].[OwnerFullName]
) AS [t0] ON [g].[Nickname] = [t0].[LeaderNickname] AND [g].[SquadId] = [t0].[LeaderSquadId]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [t0].[Nickname], [t0].[SquadId], [t0].[Id], [t0].[Nickname0], [t0].[SquadId0], [t0].[Id0], [t0].[Nickname00]
""");
    }

    public override async Task Subquery_containing_left_join_projecting_main_from_clause_gets_lifted(bool async)
    {
        await base.Subquery_containing_left_join_projecting_main_from_clause_gets_lifted(async);

        AssertSql(
"""
SELECT [g].[Nickname]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[Nickname] = [t].[GearNickName]
ORDER BY [g].[Nickname]
""");
    }

    public override async Task Conditional_expression_with_test_being_simplified_to_constant_complex(bool async)
    {
        await base.Conditional_expression_with_test_being_simplified_to_constant_complex(async);

        AssertSql(
"""
@__prm_0='True'
@__prm2_1='Dom's Lancer' (Size = 4000)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE CASE
    WHEN [g].[HasSoulPatch] = @__prm_0 THEN CASE
        WHEN (
            SELECT TOP(1) [w].[Name]
            FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
            WHERE [w].[Id] = [g].[SquadId]) = @__prm2_1 AND (
            SELECT TOP(1) [w].[Name]
            FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
            WHERE [w].[Id] = [g].[SquadId]) IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    ELSE CAST(0 AS bit)
END = CAST(1 AS bit)
""");
    }

    public override async Task Nav_rewrite_with_convert2(bool async)
    {
        await base.Nav_rewrite_with_convert2(async);

        AssertSql(
"""
SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[PeriodEnd], [f].[PeriodStart], [f].[ServerAddress], [f].[CommanderName], [f].[Eradicated]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
LEFT JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [f].[CapitalName] = [c].[Name]
LEFT JOIN (
    SELECT [l].[Name]
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
WHERE ([c].[Name] <> N'Foo' OR [c].[Name] IS NULL) AND ([t].[Name] <> N'Bar' OR [t].[Name] IS NULL)
""");
    }

    public override async Task DateTimeOffset_DateAdd_AddHours(bool async)
    {
        await base.DateTimeOffset_DateAdd_AddHours(async);

        AssertSql(
"""
SELECT DATEADD(hour, CAST(1.0E0 AS int), [m].[Timeline])
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
""");
    }

    public override async Task String_concat_with_null_conditional_argument2(bool async)
    {
        await base.String_concat_with_null_conditional_argument2(async);

        AssertSql(
"""
SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[PeriodEnd], [w0].[PeriodStart], [w0].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY COALESCE([w0].[Name], N'') + N'Marcus'' Lancer'
""");
    }

    public override async Task Trying_to_access_unmapped_property_in_projection(bool async)
    {
        await base.Trying_to_access_unmapped_property_in_projection(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Outer_parameter_in_join_key(bool async)
    {
        await base.Outer_parameter_in_join_key(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [t0].[Note], [t0].[Id], [t0].[Nickname], [t0].[SquadId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
OUTER APPLY (
    SELECT [t].[Note], [t].[Id], [g0].[Nickname], [g0].[SquadId]
    FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
    INNER JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[FullName] = [g0].[FullName]
) AS [t0]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [t0].[Id], [t0].[Nickname]
""");
    }

    public override async Task Select_subquery_distinct_singleordefault_boolean2(bool async)
    {
        await base.Select_subquery_distinct_singleordefault_boolean2(async);

        AssertSql(
"""
SELECT COALESCE((
    SELECT DISTINCT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName] AND [w].[Name] LIKE N'%Lancer%'), CAST(0 AS bit))
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
""");
    }

    public override async Task Include_collection_on_derived_type_using_string(bool async)
    {
        await base.Include_collection_on_derived_type_using_string(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[Nickname] = [g0].[LeaderNickname] AND [g].[SquadId] = [g0].[LeaderSquadId]
ORDER BY [g].[Nickname], [g].[SquadId], [g0].[Nickname]
""");
    }

    public override async Task Project_navigation_defined_on_derived_from_entity_with_inheritance_using_soft_cast(bool async)
    {
        await base.Project_navigation_defined_on_derived_from_entity_with_inheritance_using_soft_cast(async);

        AssertSql(
"""
SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], CASE
    WHEN [g].[Nickname] IS NULL OR [g].[SquadId] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsNull], [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[PeriodEnd], [f].[PeriodStart], [f].[ServerAddress], [f].[CommanderName], [f].[Eradicated], CASE
    WHEN [f].[Id] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsNull], [l0].[Id], [l0].[IsOperational], [l0].[Name], [l0].[PeriodEnd], [l0].[PeriodStart], CASE
    WHEN [l0].[Id] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsNull]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [l].[DefeatedByNickname] = [g].[Nickname] AND [l].[DefeatedBySquadId] = [g].[SquadId]
LEFT JOIN [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f] ON [l].[Name] = [f].[CommanderName]
LEFT JOIN [LocustHighCommands] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0] ON [l].[HighCommandId] = [l0].[Id]
""");
    }

    public override async Task Null_propagation_optimization6(bool async)
    {
        await base.Null_propagation_optimization6(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN CAST(LEN([g].[LeaderNickname]) AS int)
    ELSE NULL
END = 5 AND CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN CAST(LEN([g].[LeaderNickname]) AS int)
    ELSE NULL
END IS NOT NULL
""");
    }

    public override async Task Select_coalesce_with_anonymous_types(bool async)
    {
        await base.Select_coalesce_with_anonymous_types(async);

        AssertSql(
"""
SELECT [g].[LeaderNickname], [g].[FullName]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
ORDER BY [g].[Nickname]
""");
    }

    public override async Task Composite_key_entity_not_equal(bool async)
    {
        await base.Composite_key_entity_not_equal(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
CROSS JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
WHERE [g].[Nickname] <> [g0].[Nickname] OR [g].[SquadId] <> [g0].[SquadId]
""");
    }

    public override async Task Where_subquery_distinct_orderby_firstordefault_boolean_with_pushdown(bool async)
    {
        await base.Where_subquery_distinct_orderby_firstordefault_boolean_with_pushdown(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit) AND (
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
    ) AS [t]
    ORDER BY [t].[Id]) = CAST(1 AS bit)
""");
    }

    public override async Task Cast_to_derived_followed_by_include_and_FirstOrDefault(bool async)
    {
        await base.Cast_to_derived_followed_by_include_and_FirstOrDefault(async);

        AssertSql(
"""
SELECT TOP(1) [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [l].[DefeatedByNickname] = [g].[Nickname] AND [l].[DefeatedBySquadId] = [g].[SquadId]
WHERE [l].[Name] LIKE N'%Queen%'
""");
    }

    public override async Task Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up(bool async)
    {
        await base.Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up(async);

        AssertSql(
"""
SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t0].[FullName], [t0].[Nickname], [t0].[SquadId], [t0].[Name], [t0].[Nickname0], [t0].[Id]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
OUTER APPLY (
    SELECT [g0].[FullName], [g0].[Nickname], [g0].[SquadId], [t].[Name], [t].[Nickname] AS [Nickname0], [t].[Id]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    LEFT JOIN (
        SELECT [w].[Name], [g].[Nickname], [w].[Id], [w].[OwnerFullName]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [w].[Name] <> N'Bar' OR [w].[Name] IS NULL
    ) AS [t] ON [g0].[FullName] = [t].[OwnerFullName]
    WHERE [g].[Nickname] = [g0].[LeaderNickname] AND [g].[SquadId] = [g0].[LeaderSquadId] AND [g0].[FullName] <> N'Foo'
) AS [t0]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [t0].[Nickname], [t0].[SquadId]
""");
    }

    public override async Task
        Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection_multiple_grouping_keys(
            bool async)
    {
        await base
            .Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection_multiple_grouping_keys(
                async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [t].[IsAutomatic], [t].[Name], [t].[Count]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
OUTER APPLY (
    SELECT [w].[IsAutomatic], [w].[Name], COUNT(*) AS [Count]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    GROUP BY [w].[IsAutomatic], [w].[Name]
) AS [t]
ORDER BY [g].[Nickname], [g].[SquadId], [t].[IsAutomatic]
""");
    }

    public override async Task Member_access_on_derived_materialized_entity_using_cast(bool async)
    {
        await base.Member_access_on_derived_materialized_entity_using_cast(async);

        AssertSql(
"""
SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[PeriodEnd], [f].[PeriodStart], [f].[ServerAddress], [f].[CommanderName], [f].[Eradicated]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
ORDER BY [f].[Name]
""");
    }

    public override async Task Where_bool_column_or_Contains(bool async)
    {
        await base.Where_bool_column_or_Contains(async);

        AssertSql(
"""
@__values_0='[false,true]' (Size = 4000)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit) AND EXISTS (
    SELECT 1
    FROM OpenJson(@__values_0) AS [v]
    WHERE CAST([v].[value] AS bit) = [g].[HasSoulPatch])
""");
    }

    public override async Task Subquery_containing_join_projecting_main_from_clause_gets_lifted(bool async)
    {
        await base.Subquery_containing_join_projecting_main_from_clause_gets_lifted(async);

        AssertSql(
"""
SELECT [g].[Nickname]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[Nickname] = [t].[GearNickName]
ORDER BY [g].[Nickname]
""");
    }

    public override async Task Include_with_group_by_and_FirstOrDefault_gets_properly_applied(bool async)
    {
        await base.Include_with_group_by_and_FirstOrDefault_gets_properly_applied(async);

        AssertSql(
"""
SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Rank], [t0].[Name], [t0].[Location], [t0].[Nation], [t0].[PeriodEnd0], [t0].[PeriodStart0]
FROM (
    SELECT [g].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    GROUP BY [g].[Rank]
) AS [t]
LEFT JOIN (
    SELECT [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[Discriminator], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Rank], [t1].[Name], [t1].[Location], [t1].[Nation], [t1].[PeriodEnd0], [t1].[PeriodStart0]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank], [c].[Name], [c].[Location], [c].[Nation], [c].[PeriodEnd] AS [PeriodEnd0], [c].[PeriodStart] AS [PeriodStart0], ROW_NUMBER() OVER(PARTITION BY [g0].[Rank] ORDER BY [g0].[Nickname], [g0].[SquadId], [c].[Name]) AS [row]
        FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
        INNER JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g0].[CityOfBirthName] = [c].[Name]
        WHERE [g0].[HasSoulPatch] = CAST(1 AS bit)
    ) AS [t1]
    WHERE [t1].[row] <= 1
) AS [t0] ON [t].[Rank] = [t0].[Rank]
""");
    }

    public override async Task Select_null_propagation_negative6(bool async)
    {
        await base.Select_null_propagation_negative6(async);

        AssertSql(
"""
SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN CASE
        WHEN CAST(LEN([g].[LeaderNickname]) AS int) <> CAST(LEN([g].[LeaderNickname]) AS int) THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    ELSE NULL
END
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Cast_to_derived_type_causes_client_eval(bool async)
    {
        await base.Cast_to_derived_type_causes_client_eval(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_negated_predicate(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_negated_predicate(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE ([t].[Note] <> N'K.I.A.' OR [t].[Note] IS NULL) AND [g].[HasSoulPatch] = CAST(0 AS bit)
""");
    }

    public override async Task Project_one_value_type_from_empty_collection(bool async)
    {
        await base.Project_one_value_type_from_empty_collection(async);

        AssertSql(
"""
SELECT [s].[Name], COALESCE((
    SELECT TOP(1) [g].[SquadId]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [s].[Id] = [g].[SquadId] AND [g].[HasSoulPatch] = CAST(1 AS bit)), 0) AS [SquadId]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
WHERE [s].[Name] = N'Kilo'
""");
    }

    public override async Task Contains_on_nullable_array_produces_correct_sql(bool async)
    {
        await base.Contains_on_nullable_array_produces_correct_sql(async);

        AssertSql(
"""
@__cities_0='["Ephyra",null]' (Size = 4000)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g].[AssignedCityName] = [c].[Name]
WHERE [g].[SquadId] < 2 AND EXISTS (
    SELECT 1
    FROM OpenJson(@__cities_0) AS [c0]
    WHERE CAST([c0].[value] AS nvarchar(450)) = [c].[Name] OR ([c0].[value] IS NULL AND [c].[Name] IS NULL))
""");
    }

    public override async Task Subquery_containing_SelectMany_projecting_main_from_clause_gets_lifted(bool async)
    {
        await base.Subquery_containing_SelectMany_projecting_main_from_clause_gets_lifted(async);

        AssertSql(
"""
SELECT [g].[FullName]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
CROSS JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
ORDER BY [g].[FullName]
""");
    }

    public override async Task SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector_not_equal(bool async)
    {
        await base.SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector_not_equal(async);

        AssertSql(
"""
@__isAutomatic_0='True'

SELECT [g].[Nickname], [g].[FullName], CASE
    WHEN [t].[Id] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Collection]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [w].[Id], [w].[OwnerFullName]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [w].[IsAutomatic] <> @__isAutomatic_0
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
""");
    }

    public override async Task Navigation_based_on_complex_expression6(bool async)
    {
        await base.Navigation_based_on_complex_expression6(async);

        AssertSql(
"""
SELECT CASE
    WHEN [t0].[Name] = N'Queen Myrrah' AND [t0].[Name] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [t0].[Name], [t0].[Discriminator], [t0].[LocustHordeId], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[ThreatLevel], [t0].[ThreatLevelByte], [t0].[ThreatLevelNullableByte], [t0].[DefeatedByNickname], [t0].[DefeatedBySquadId], [t0].[HighCommandId], [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[PeriodEnd], [t].[PeriodStart], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
CROSS JOIN (
    SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t]
LEFT JOIN (
    SELECT [l0].[Name], [l0].[Discriminator], [l0].[LocustHordeId], [l0].[PeriodEnd], [l0].[PeriodStart], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId]
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    WHERE [l0].[Discriminator] = N'LocustCommander'
) AS [t0] ON [f].[CommanderName] = [t0].[Name]
""");
    }

    public override async Task Join_with_order_by_without_skip_or_take(bool async)
    {
        await base.Join_with_order_by_without_skip_or_take(async);

        AssertSql(
"""
SELECT [w].[Name], [g].[FullName]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] = [w].[OwnerFullName]
""");
    }

    public override async Task GroupBy_Property_Include_Aggregate_with_anonymous_selector(bool async)
    {
        await base.GroupBy_Property_Include_Aggregate_with_anonymous_selector(async);

        AssertSql(
"""
SELECT [g].[Nickname] AS [Key], COUNT(*) AS [c]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
GROUP BY [g].[Nickname]
ORDER BY [g].[Nickname]
""");
    }

    public override async Task Where_conditional_equality_2(bool async)
    {
        await base.Where_conditional_equality_2(async);

        AssertSql(
"""
SELECT [g].[Nickname]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[LeaderNickname] IS NULL
ORDER BY [g].[Nickname]
""");
    }

    public override async Task Non_unicode_string_literal_is_used_for_non_unicode_column(bool async)
    {
        await base.Non_unicode_string_literal_is_used_for_non_unicode_column(async);

        AssertSql(
"""
SELECT [c].[Name], [c].[Location], [c].[Nation], [c].[PeriodEnd], [c].[PeriodStart]
FROM [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c]
WHERE [c].[Location] = 'Unknown'
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_predicate_negated_complex2(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_predicate_negated_complex2(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE CASE
    WHEN [g].[HasSoulPatch] = CAST(0 AS bit) THEN CAST(0 AS bit)
    ELSE [g].[HasSoulPatch]
END = CAST(0 AS bit)
""");
    }

    public override async Task Filtered_collection_projection_with_order_comparison_predicate_converted_to_join3(bool async)
    {
        await base.Filtered_collection_projection_with_order_comparison_predicate_converted_to_join3(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] = [w].[OwnerFullName] AND [g].[SquadId] >= [w].[Id]
ORDER BY [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Navigation_based_on_complex_expression4(bool async)
    {
        await base.Navigation_based_on_complex_expression4(async);

        AssertSql(
"""
SELECT CAST(1 AS bit), [t0].[Name], [t0].[Discriminator], [t0].[LocustHordeId], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[ThreatLevel], [t0].[ThreatLevelByte], [t0].[ThreatLevelNullableByte], [t0].[DefeatedByNickname], [t0].[DefeatedBySquadId], [t0].[HighCommandId], [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[PeriodEnd], [t].[PeriodStart], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
CROSS JOIN (
    SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t]
LEFT JOIN (
    SELECT [l0].[Name], [l0].[Discriminator], [l0].[LocustHordeId], [l0].[PeriodEnd], [l0].[PeriodStart], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId]
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    WHERE [l0].[Discriminator] = N'LocustCommander'
) AS [t0] ON [f].[CommanderName] = [t0].[Name]
""");
    }

    public override async Task Include_with_order_by_constant(bool async)
    {
        await base.Include_with_order_by_constant(async);

        AssertSql(
"""
SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [s].[Id] = [g].[SquadId]
ORDER BY [s].[Id], [g].[Nickname]
""");
    }

    public override async Task Select_null_propagation_negative7(bool async)
    {
        await base.Select_null_propagation_negative7(async);

        AssertSql(
"""
SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN CAST(1 AS bit)
    ELSE NULL
END
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion(bool async)
    {
        await base.Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion(async);

        AssertSql(
"""
SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Rank]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
CROSS APPLY (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE EXISTS (
        SELECT 1
        FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        WHERE [l0].[ThreatLevelNullableByte] = [l].[ThreatLevelNullableByte] OR ([l0].[ThreatLevelNullableByte] IS NULL AND [l].[ThreatLevelNullableByte] IS NULL))
) AS [t]
""");
    }

    public override async Task Where_subquery_boolean(bool async)
    {
        await base.Where_subquery_boolean(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE COALESCE((
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]), CAST(0 AS bit)) = CAST(1 AS bit)
""");
    }

    public override async Task Correlated_collections_basic_projection_explicit_to_list(bool async)
    {
        await base.Correlated_collections_basic_projection_explicit_to_list(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[PeriodEnd], [t].[PeriodStart], [t].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit) OR [w].[Name] <> N'foo' OR [w].[Name] IS NULL
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
WHERE [g].[Nickname] <> N'Marcus'
ORDER BY [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Select_ternary_operation_with_has_value_not_null(bool async)
    {
        await base.Select_ternary_operation_with_has_value_not_null(async);

        AssertSql(
"""
SELECT [w].[Id], CASE
    WHEN [w].[AmmunitionType] IS NOT NULL AND [w].[AmmunitionType] = 1 THEN N'Yes'
    ELSE N'No'
END AS [IsCartridge]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE [w].[AmmunitionType] IS NOT NULL AND [w].[AmmunitionType] = 1
""");
    }

    public override async Task Collection_navigation_access_on_derived_entity_using_cast(bool async)
    {
        await base.Collection_navigation_access_on_derived_entity_using_cast(async);

        AssertSql(
"""
SELECT [f].[Name], (
    SELECT COUNT(*)
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    WHERE [f].[Id] = [l].[LocustHordeId]) AS [LeadersCount]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
ORDER BY [f].[Name]
""");
    }

    public override async Task String_compare_with_null_conditional_argument2(bool async)
    {
        await base.String_compare_with_null_conditional_argument2(async);

        AssertSql(
"""
SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[PeriodEnd], [w0].[PeriodStart], [w0].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY CASE
    WHEN N'Marcus'' Lancer' = [w0].[Name] AND [w0].[Name] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task GroupBy_Property_Include_Select_Max(bool async)
    {
        await base.GroupBy_Property_Include_Select_Max(async);

        AssertSql(
"""
SELECT MAX([g].[SquadId])
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
GROUP BY [g].[Rank]
""");
    }

    public override async Task SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector_order_comparison(bool async)
    {
        await base.SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector_order_comparison(async);

        AssertSql(
"""
@__prm_0='1'

SELECT [g].[Nickname], [g].[FullName], CASE
    WHEN [t].[Id] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Collection]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [w].[Id], [w].[OwnerFullName]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [w].[Id] > @__prm_0
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
""");
    }

    public override async Task Select_null_propagation_works_for_navigations_with_composite_keys(bool async)
    {
        await base.Select_null_propagation_works_for_navigations_with_composite_keys(async);

        AssertSql(
"""
SELECT [g].[Nickname]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
""");
    }

    public override async Task Include_base_navigation_on_derived_entity(bool async)
    {
        await base.Include_base_navigation_on_derived_entity(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[Nickname] = [t].[GearNickName] AND [g].[SquadId] = [t].[GearSquadId]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] = [w].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Id]
""");
    }

    public override async Task Include_with_join_collection2(bool async)
    {
        await base.Include_with_join_collection2(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [t].[Id], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
INNER JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearSquadId] = [g].[SquadId] AND [t].[GearNickName] = [g].[Nickname]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] = [w].[OwnerFullName]
ORDER BY [t].[Id], [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Correlated_collection_with_complex_OrderBy(bool async)
    {
        await base.Correlated_collection_with_complex_OrderBy(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    WHERE [g0].[HasSoulPatch] = CAST(0 AS bit)
) AS [t] ON [g].[Nickname] = [t].[LeaderNickname] AND [g].[SquadId] = [t].[LeaderSquadId]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY (
    SELECT COUNT(*)
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]), [g].[Nickname], [g].[SquadId], [t].[Nickname]
""");
    }

    public override async Task Complex_predicate_with_AndAlso_and_nullable_bool_property(bool async)
    {
        await base.Complex_predicate_with_AndAlso_and_nullable_bool_property(async);

        AssertSql(
"""
SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [w].[OwnerFullName] = [g].[FullName]
WHERE [w].[Id] <> 50 AND [g].[HasSoulPatch] = CAST(0 AS bit)
""");
    }

    public override async Task Where_datetimeoffset_second_component(bool async)
    {
        await base.Where_datetimeoffset_second_component(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE DATEPART(second, [m].[Timeline]) = 0
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_binary_expression(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_binary_expression(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit) OR [t].[Note] LIKE N'%Cole%'
""");
    }

    public override async Task SelectMany_predicate_with_non_equality_comparison_with_Take_doesnt_convert_to_join(bool async)
    {
        await base.SelectMany_predicate_with_non_equality_comparison_with_Take_doesnt_convert_to_join(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[PeriodEnd], [t].[PeriodStart], [t].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
CROSS APPLY (
    SELECT TOP(3) [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [w].[OwnerFullName] <> [g].[FullName] OR [w].[OwnerFullName] IS NULL
    ORDER BY [w].[Id]
) AS [t]
ORDER BY [g].[Nickname], [t].[Id]
""");
    }

    public override async Task Include_with_complex_order_by(bool async)
    {
        await base.Include_with_complex_order_by(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[PeriodEnd], [w0].[PeriodStart], [w0].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0] ON [g].[FullName] = [w0].[OwnerFullName]
ORDER BY (
    SELECT TOP(1) [w].[Name]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName] AND [w].[Name] LIKE N'%Gnasher%'), [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Reference_include_chain_loads_correctly_when_middle_is_null(bool async)
    {
        await base.Reference_include_chain_loads_correctly_when_middle_is_null(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
LEFT JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [g].[SquadId] = [s].[Id]
ORDER BY [t].[Note]
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_projection_into_anonymous_type(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_projection_into_anonymous_type(async);

        AssertSql(
"""
SELECT [g].[SquadId]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE [t].[Note] <> N'K.I.A.' OR [t].[Note] IS NULL
""");
    }

    public override async Task SelectMany_predicate_after_navigation_with_non_equality_comparison_DefaultIfEmpty_converted_to_left_join(
        bool async)
    {
        await base.SelectMany_predicate_after_navigation_with_non_equality_comparison_DefaultIfEmpty_converted_to_left_join(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[PeriodEnd], [t].[PeriodStart], [t].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[PeriodEnd], [w0].[PeriodStart], [w0].[SynergyWithId]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
) AS [t] ON [g].[FullName] <> [t].[OwnerFullName] OR [t].[OwnerFullName] IS NULL
ORDER BY [g].[Nickname], [t].[Id]
""");
    }

    public override async Task Project_collection_navigation_nested_composite_key(bool async)
    {
        await base.Project_collection_navigation_nested_composite_key(async);

        AssertSql(
"""
SELECT [t].[Id], [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON ([g].[Nickname] = [g0].[LeaderNickname] OR ([g].[Nickname] IS NULL AND [g0].[LeaderNickname] IS NULL)) AND [g].[SquadId] = [g0].[LeaderSquadId]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [t].[Id], [g].[Nickname], [g].[SquadId], [g0].[Nickname]
""");
    }

    public override async Task Cast_to_derived_followed_by_multiple_includes(bool async)
    {
        await base.Cast_to_derived_followed_by_multiple_includes(async);

        AssertSql(
"""
SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [l].[DefeatedByNickname] = [g].[Nickname] AND [l].[DefeatedBySquadId] = [g].[SquadId]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] = [w].[OwnerFullName]
WHERE [l].[Name] LIKE N'%Queen%'
ORDER BY [l].[Name], [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Where_any_subquery_without_collision(bool async)
    {
        await base.Where_any_subquery_without_collision(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE EXISTS (
    SELECT 1
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName])
""");
    }

    public override async Task Select_subquery_int_with_pushdown_and_coalesce(bool async)
    {
        await base.Select_subquery_int_with_pushdown_and_coalesce(async);

        AssertSql(
"""
SELECT COALESCE((
    SELECT TOP(1) [w].[Id]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]), 42)
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Select_subquery_distinct_singleordefault_boolean_empty2(bool async)
    {
        await base.Select_subquery_distinct_singleordefault_boolean_empty2(async);

        AssertSql(
"""
SELECT COALESCE((
    SELECT DISTINCT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName] AND [w].[Name] = N'BFG'), CAST(0 AS bit))
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
""");
    }

    public override async Task DateTimeOffset_DateAdd_AddYears(bool async)
    {
        await base.DateTimeOffset_DateAdd_AddYears(async);

        AssertSql(
"""
SELECT DATEADD(year, CAST(1 AS int), [m].[Timeline])
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
""");
    }

    public override async Task Logical_operation_with_non_null_parameter_optimizes_null_checks(bool async)
    {
        await base.Logical_operation_with_non_null_parameter_optimizes_null_checks(async);

        AssertSql(
"""
@__prm_0='True'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] <> @__prm_0
""",
            //
"""
@__prm_0='False'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] <> @__prm_0
""");
    }

    public override async Task Include_collection_with_Cast_to_base(bool async)
    {
        await base.Include_collection_with_Cast_to_base(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] = [w].[OwnerFullName]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Select_Where_Navigation_Scalar_Equals_Navigation_Scalar(bool async)
    {
        await base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart], [t0].[Id], [t0].[GearNickName], [t0].[GearSquadId], [t0].[IssueDate], [t0].[Note], [t0].[PeriodEnd], [t0].[PeriodStart]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
CROSS JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t0]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [t0].[GearNickName] = [g0].[Nickname] AND [t0].[GearSquadId] = [g0].[SquadId]
WHERE [g].[Nickname] = [g0].[Nickname] OR ([g].[Nickname] IS NULL AND [g0].[Nickname] IS NULL)
""");
    }

    public override async Task Left_join_projection_using_conditional_tracking(bool async)
    {
        await base.Left_join_projection_using_conditional_tracking(async);

        AssertSql(
"""
SELECT CASE
    WHEN [g0].[Nickname] IS NULL OR [g0].[SquadId] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[LeaderNickname] = [g0].[Nickname]
""");
    }

    public override async Task GroupBy_with_boolean_grouping_key(bool async)
    {
        await base.GroupBy_with_boolean_grouping_key(async);

        AssertSql(
"""
SELECT [t].[CityOfBirthName], [t].[HasSoulPatch], [t].[IsMarcus], COUNT(*) AS [Count]
FROM (
    SELECT [g].[CityOfBirthName], [g].[HasSoulPatch], CASE
        WHEN [g].[Nickname] = N'Marcus' THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsMarcus]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
) AS [t]
GROUP BY [t].[CityOfBirthName], [t].[HasSoulPatch], [t].[IsMarcus]
""");
    }

    public override async Task Correlated_collections_with_Distinct(bool async)
    {
        await base.Correlated_collections_with_Distinct(async);

        AssertSql(
"""
SELECT [s].[Id], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Rank]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
OUTER APPLY (
    SELECT DISTINCT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Rank]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
        FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
        WHERE [s].[Id] = [g].[SquadId]
        ORDER BY [g].[Nickname]
        OFFSET 0 ROWS
    ) AS [t]
) AS [t0]
ORDER BY [s].[Name], [s].[Id], [t0].[Nickname]
""");
    }

    public override async Task Correlated_collections_basic_projecting_single_property(bool async)
    {
        await base.Correlated_collections_basic_projecting_single_property(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [t].[Name], [t].[Id]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [w].[Name], [w].[Id], [w].[OwnerFullName]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit) OR [w].[Name] <> N'foo' OR [w].[Name] IS NULL
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
WHERE [g].[Nickname] <> N'Marcus'
ORDER BY [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Select_null_propagation_negative1(bool async)
    {
        await base.Select_null_propagation_negative1(async);

        AssertSql(
"""
SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN CASE
        WHEN CAST(LEN([g].[Nickname]) AS int) = 5 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    ELSE NULL
END
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Join_inner_source_custom_projection_followed_by_filter(bool async)
    {
        await base.Join_inner_source_custom_projection_followed_by_filter(async);

        AssertSql(
"""
SELECT CASE
    WHEN [f].[Name] = N'Locust' THEN CAST(1 AS bit)
    ELSE NULL
END AS [IsEradicated], [f].[CommanderName], [f].[Name]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
INNER JOIN [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f] ON [l].[Name] = [f].[CommanderName]
WHERE CASE
    WHEN [f].[Name] = N'Locust' THEN CAST(1 AS bit)
    ELSE NULL
END <> CAST(1 AS bit) OR CASE
    WHEN [f].[Name] = N'Locust' THEN CAST(1 AS bit)
    ELSE NULL
END IS NULL
""");
    }

    public override async Task Collection_navigation_ofType_filter_works(bool async)
    {
        await base.Collection_navigation_ofType_filter_works(async);

        AssertSql(
"""
SELECT [c].[Name], [c].[Location], [c].[Nation], [c].[PeriodEnd], [c].[PeriodStart]
FROM [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [c].[Name] = [g].[CityOfBirthName] AND [g].[Discriminator] = N'Officer' AND [g].[Nickname] = N'Marcus')
""");
    }

    public override async Task DateTimeOffset_DateAdd_AddMinutes(bool async)
    {
        await base.DateTimeOffset_DateAdd_AddMinutes(async);

        AssertSql(
"""
SELECT DATEADD(minute, CAST(1.0E0 AS int), [m].[Timeline])
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
""");
    }

    public override async Task Where_with_enum_flags_parameter(bool async)
    {
        await base.Where_with_enum_flags_parameter(async);

        AssertSql(
"""
@__rank_0='1' (Nullable = true)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Rank] & @__rank_0 = @__rank_0
""",
            //
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""",
            //
"""
@__rank_0='2' (Nullable = true)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Rank] | @__rank_0 <> @__rank_0
""",
            //
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE 0 = 1
""");
    }

    public override async Task Select_null_propagation_negative9(bool async)
    {
        await base.Select_null_propagation_negative9(async);

        AssertSql(
"""
SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN COALESCE(CASE
        WHEN CAST(LEN([g].[Nickname]) AS int) = 5 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END, CAST(0 AS bit))
    ELSE NULL
END
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Comparing_two_collection_navigations_composite_key(bool async)
    {
        await base.Comparing_two_collection_navigations_composite_key(async);

        AssertSql(
"""
SELECT [g].[Nickname] AS [Nickname1], [g0].[Nickname] AS [Nickname2]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
CROSS JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
WHERE [g].[Nickname] = [g0].[Nickname] AND [g].[SquadId] = [g0].[SquadId]
ORDER BY [g].[Nickname]
""");
    }

    public override async Task Projecting_property_converted_to_nullable_with_addition(bool async)
    {
        await base.Projecting_property_converted_to_nullable_with_addition(async);

        AssertSql(
"""
SELECT [t].[Note], CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [g].[Nickname], [g].[SquadId], [g].[HasSoulPatch]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[SquadId]
    ELSE NULL
END + 1 = 2
""");
    }

    public override async Task Correlated_collections_nested(bool async)
    {
        await base.Correlated_collections_nested(async);

        AssertSql(
"""
SELECT [s].[Id], [t0].[SquadId], [t0].[MissionId], [t0].[Id], [t0].[SquadId0], [t0].[MissionId0], [t0].[PeriodEnd], [t0].[PeriodStart]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
LEFT JOIN (
    SELECT [s0].[SquadId], [s0].[MissionId], [m].[Id], [t].[SquadId] AS [SquadId0], [t].[MissionId] AS [MissionId0], [t].[PeriodEnd], [t].[PeriodStart]
    FROM [SquadMissions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s0]
    INNER JOIN [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m] ON [s0].[MissionId] = [m].[Id]
    LEFT JOIN (
        SELECT [s1].[SquadId], [s1].[MissionId], [s1].[PeriodEnd], [s1].[PeriodStart]
        FROM [SquadMissions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s1]
        WHERE [s1].[SquadId] < 7
    ) AS [t] ON [m].[Id] = [t].[MissionId]
    WHERE [s0].[MissionId] < 42
) AS [t0] ON [s].[Id] = [t0].[SquadId]
ORDER BY [s].[Id], [t0].[SquadId], [t0].[MissionId], [t0].[Id], [t0].[SquadId0]
""");
    }

    public override async Task Where_subquery_concat_firstordefault_boolean(bool async)
    {
        await base.Where_subquery_concat_firstordefault_boolean(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit) AND (
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
        UNION ALL
        SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[PeriodEnd], [w0].[PeriodStart], [w0].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0]
        WHERE [g].[FullName] = [w0].[OwnerFullName]
    ) AS [t]
    ORDER BY [t].[Id]) = CAST(1 AS bit)
""");
    }

    public override async Task Where_subquery_distinct_lastordefault_boolean(bool async)
    {
        await base.Where_subquery_distinct_lastordefault_boolean(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE (
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
    ) AS [t]
    ORDER BY [t].[Id] DESC) = CAST(0 AS bit)
ORDER BY [g].[Nickname]
""");
    }

    public override async Task Concat_with_collection_navigations(bool async)
    {
        await base.Concat_with_collection_navigations(async);

        AssertSql(
"""
SELECT (
    SELECT COUNT(*)
    FROM (
        SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
        UNION
        SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[PeriodEnd], [w0].[PeriodStart], [w0].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0]
        WHERE [g].[FullName] = [w0].[OwnerFullName]
    ) AS [t])
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
""");
    }

    public override async Task Join_on_entity_qsre_keys(bool async)
    {
        await base.Join_on_entity_qsre_keys(async);

        AssertSql(
"""
SELECT [w].[Name] AS [Name1], [w0].[Name] AS [Name2]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
INNER JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0] ON [w].[Id] = [w0].[Id]
""");
    }

    public override async Task Projecting_property_converted_to_nullable_with_comparison(bool async)
    {
        await base.Projecting_property_converted_to_nullable_with_comparison(async);

        AssertSql(
"""
SELECT [t].[Note], CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [g].[Nickname], [g].[SquadId], [g].[HasSoulPatch]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[SquadId]
    ELSE NULL
END = 1
""");
    }

    public override async Task Distinct_with_optional_navigation_is_translated_to_sql(bool async)
    {
        await base.Distinct_with_optional_navigation_is_translated_to_sql(async);

        AssertSql(
"""
SELECT DISTINCT [g].[HasSoulPatch]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[Nickname] = [t].[GearNickName] AND [g].[SquadId] = [t].[GearSquadId]
WHERE [t].[Note] <> N'Foo' OR [t].[Note] IS NULL
""");
    }

    public override async Task Where_member_access_on_anonymous_type(bool async)
    {
        await base.Where_member_access_on_anonymous_type(async);

        AssertSql(
"""
SELECT [g].[Nickname]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[LeaderNickname] = N'Marcus'
""");
    }

    public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result(bool async)
    {
        await base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank], [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[PeriodEnd], [w0].[PeriodStart], [w0].[SynergyWithId], [w1].[Id], [w1].[AmmunitionType], [w1].[IsAutomatic], [w1].[Name], [w1].[OwnerFullName], [w1].[PeriodEnd], [w1].[PeriodStart], [w1].[SynergyWithId], [w2].[Id], [w2].[AmmunitionType], [w2].[IsAutomatic], [w2].[Name], [w2].[OwnerFullName], [w2].[PeriodEnd], [w2].[PeriodStart], [w2].[SynergyWithId], CASE
    WHEN [g0].[Nickname] IS NOT NULL AND [g0].[SquadId] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [w3].[Id], [w3].[AmmunitionType], [w3].[IsAutomatic], [w3].[Name], [w3].[OwnerFullName], [w3].[PeriodEnd], [w3].[PeriodStart], [w3].[SynergyWithId], [w4].[Id], [w4].[AmmunitionType], [w4].[IsAutomatic], [w4].[Name], [w4].[OwnerFullName], [w4].[PeriodEnd], [w4].[PeriodStart], [w4].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[LeaderNickname] = [g0].[Nickname]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] = [w].[OwnerFullName]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0] ON [g0].[FullName] = [w0].[OwnerFullName]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w1] ON [g0].[FullName] = [w1].[OwnerFullName]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w2] ON [g].[FullName] = [w2].[OwnerFullName]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w3] ON [g0].[FullName] = [w3].[OwnerFullName]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w4] ON [g].[FullName] = [w4].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId], [w].[Id], [w0].[Id], [w1].[Id], [w2].[Id], [w3].[Id]
""");
    }

    public override async Task Left_join_predicate_value_equals_condition(bool async)
    {
        await base.Left_join_predicate_value_equals_condition(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [w].[SynergyWithId] IS NOT NULL
""");
    }

    public override async Task GetValueOrDefault_in_order_by(bool async)
    {
        await base.GetValueOrDefault_in_order_by(async);

        AssertSql(
"""
SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
ORDER BY COALESCE([w].[SynergyWithId], 0), [w].[Id]
""");
    }

    public override async Task GroupBy_Property_Include_Select_Sum(bool async)
    {
        await base.GroupBy_Property_Include_Select_Sum(async);

        AssertSql(
"""
SELECT COALESCE(SUM([g].[SquadId]), 0)
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
GROUP BY [g].[Rank]
""");
    }

    public override async Task Where_enum_has_flag_with_non_nullable_parameter(bool async)
    {
        await base.Where_enum_has_flag_with_non_nullable_parameter(async);

        AssertSql(
"""
@__parameter_0='2'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Rank] & @__parameter_0 = @__parameter_0
""");
    }

    public override async Task Where_TimeSpan_Seconds(bool async)
    {
        await base.Where_TimeSpan_Seconds(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE DATEPART(second, [m].[Duration]) = 1
""");
    }

    public override async Task Query_reusing_parameter_doesnt_declare_duplicate_parameter(bool async)
    {
        await base.Query_reusing_parameter_doesnt_declare_duplicate_parameter(async);

        AssertSql(
"""
@__prm_Inner_Nickname_0='Marcus' (Size = 450)

SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Rank]
FROM (
    SELECT DISTINCT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [g].[Nickname] <> @__prm_Inner_Nickname_0 AND [g].[Nickname] <> @__prm_Inner_Nickname_0
) AS [t]
ORDER BY [t].[FullName]
""");
    }

    public override async Task Correlated_collections_on_left_join_with_null_value(bool async)
    {
        await base.Correlated_collections_on_left_join_with_null_value(async);

        AssertSql(
"""
SELECT [t].[Id], [g].[Nickname], [g].[SquadId], [w].[Name], [w].[Id]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] = [w].[OwnerFullName]
ORDER BY [t].[Note], [t].[Id], [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Include_on_derived_entity_using_OfType(bool async)
    {
        await base.Include_on_derived_entity_using_OfType(async);

        AssertSql(
"""
SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[PeriodEnd], [f].[PeriodStart], [f].[ServerAddress], [f].[CommanderName], [f].[Eradicated], [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[PeriodEnd], [t].[PeriodStart], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [l0].[Name], [l0].[Discriminator], [l0].[LocustHordeId], [l0].[PeriodEnd], [l0].[PeriodStart], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
LEFT JOIN (
    SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
LEFT JOIN [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0] ON [f].[Id] = [l0].[LocustHordeId]
ORDER BY [f].[Name], [f].[Id], [t].[Name]
""");
    }

    public override async Task Select_Singleton_Navigation_With_Member_Access(bool async)
    {
        await base.Select_Singleton_Navigation_With_Member_Access(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE [g].[Nickname] = N'Marcus' AND ([g].[CityOfBirthName] <> N'Ephyra' OR [g].[CityOfBirthName] IS NULL)
""");
    }

    public override async Task GroupBy_Property_Include_Select_Min(bool async)
    {
        await base.GroupBy_Property_Include_Select_Min(async);

        AssertSql(
"""
SELECT MIN([g].[SquadId])
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
GROUP BY [g].[Rank]
""");
    }

    public override async Task Select_inverted_boolean(bool async)
    {
        await base.Select_inverted_boolean(async);

        AssertSql(
"""
SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = CAST(0 AS bit) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Manual]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE [w].[IsAutomatic] = CAST(1 AS bit)
""");
    }

    public override async Task Where_datetimeoffset_millisecond_component(bool async)
    {
        await base.Where_datetimeoffset_millisecond_component(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE DATEPART(millisecond, [m].[Timeline]) = 0
""");
    }

    public override async Task Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion_negated(
        bool async)
    {
        await base.Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion_negated(async);

        AssertSql(
"""
SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Rank]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
CROSS APPLY (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE NOT (EXISTS (
        SELECT 1
        FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        WHERE [l0].[ThreatLevelByte] = [l].[ThreatLevelByte]))
) AS [t]
""");
    }

    public override async Task Correlated_collections_basic_projection_composite_key(bool async)
    {
        await base.Correlated_collections_basic_projection_composite_key(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [t].[Nickname], [t].[FullName], [t].[SquadId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[FullName], [g0].[SquadId], [g0].[LeaderNickname], [g0].[LeaderSquadId]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    WHERE [g0].[HasSoulPatch] = CAST(0 AS bit)
) AS [t] ON [g].[Nickname] = [t].[LeaderNickname] AND [g].[SquadId] = [t].[LeaderSquadId]
WHERE [g].[Discriminator] = N'Officer' AND [g].[Nickname] <> N'Foo'
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Nickname]
""");
    }

    public override async Task Contains_on_readonly_enumerable(bool async)
    {
        await base.Contains_on_readonly_enumerable(async);

        AssertSql(
"""
SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE [w].[AmmunitionType] = 1
""");
    }

    public override async Task Projecting_property_converted_to_nullable_into_new_array(bool async)
    {
        await base.Projecting_property_converted_to_nullable_into_new_array(async);

        AssertSql(
"""
SELECT CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN CAST(LEN([g].[Nickname]) AS int)
    ELSE NULL
END, CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[SquadId]
    ELSE NULL
END, CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[SquadId]
    ELSE NULL
END + 1
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[Nickname]
    ELSE NULL
END IS NOT NULL
ORDER BY [t].[Note]
""");
    }

    public override async Task Outer_parameter_in_join_key_inner_and_outer(bool async)
    {
        await base.Outer_parameter_in_join_key_inner_and_outer(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [t0].[Note], [t0].[Id], [t0].[Nickname], [t0].[SquadId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
OUTER APPLY (
    SELECT [t].[Note], [t].[Id], [g0].[Nickname], [g0].[SquadId]
    FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
    INNER JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[FullName] = [g].[Nickname]
) AS [t0]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [t0].[Id], [t0].[Nickname]
""");
    }

    public override async Task Select_Where_Navigation_Included(bool async)
    {
        await base.Select_Where_Navigation_Included(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE [g].[Nickname] = N'Marcus'
""");
    }

    public override async Task Correlated_collections_basic_projecting_constant(bool async)
    {
        await base.Correlated_collections_basic_projecting_constant(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [t].[c], [t].[Id]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT N'BFG' AS [c], [w].[Id], [w].[OwnerFullName]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit) OR [w].[Name] <> N'foo' OR [w].[Name] IS NULL
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
WHERE [g].[Nickname] <> N'Marcus'
ORDER BY [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Select_subquery_projecting_single_constant_null_of_non_mapped_type(bool async)
    {
        await base.Select_subquery_projecting_single_constant_null_of_non_mapped_type(async);

        AssertSql(
"""
SELECT [s].[Name], [t0].[c]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
LEFT JOIN (
    SELECT [t].[c], [t].[SquadId]
    FROM (
        SELECT 1 AS [c], [g].[SquadId], ROW_NUMBER() OVER(PARTITION BY [g].[SquadId] ORDER BY [g].[Nickname], [g].[SquadId]) AS [row]
        FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
        WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [s].[Id] = [t0].[SquadId]
""");
    }

    public override async Task Include_with_client_method_and_member_access_still_applies_includes(bool async)
    {
        await base.Include_with_client_method_and_member_access_still_applies_includes(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[Nickname] = [t].[GearNickName] AND [g].[SquadId] = [t].[GearSquadId]
""");
    }

    public override async Task Include_after_Select_throws(bool async)
    {
        await base.Include_after_Select_throws(async);

        AssertSql(
"""
SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[PeriodEnd], [f].[PeriodStart], [f].[ServerAddress], [f].[CommanderName], [f].[Eradicated], [c].[Name], [c].[Location], [c].[Nation], [c].[PeriodEnd], [c].[PeriodStart]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
LEFT JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [f].[CapitalName] = [c].[Name]
""");
    }

    public override async Task Enum_closure_typed_as_underlying_type_generates_correct_parameter_type(bool async)
    {
        await base.Enum_closure_typed_as_underlying_type_generates_correct_parameter_type(async);

        AssertSql(
"""
@__prm_0='1' (Nullable = true)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE @__prm_0 = [w].[AmmunitionType]
""");
    }

    public override async Task FirstOrDefault_with_manually_created_groupjoin_is_translated_to_sql(bool async)
    {
        await base.FirstOrDefault_with_manually_created_groupjoin_is_translated_to_sql(async);

        AssertSql(
"""
SELECT TOP(1) [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [s].[Id] = [g].[SquadId]
WHERE [s].[Name] = N'Kilo'
""");
    }

    public override async Task Bitwise_operation_with_null_arguments(bool async)
    {
        await base.Bitwise_operation_with_null_arguments(async);

        AssertSql(
"""
SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE [w].[AmmunitionType] IS NULL
""",
            //
"""
SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE [w].[AmmunitionType] IS NULL
""",
            //
"""
SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE [w].[AmmunitionType] IS NULL
""",
            //
"""
SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
""",
            //
"""
@__prm_0='2' (Nullable = true)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE [w].[AmmunitionType] & @__prm_0 <> 0 OR [w].[AmmunitionType] IS NULL
""",
            //
"""
@__prm_0='1' (Nullable = true)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE [w].[AmmunitionType] & @__prm_0 = @__prm_0
""");
    }

    public override async Task Projecting_property_converted_to_nullable_with_function_call(bool async)
    {
        await base.Projecting_property_converted_to_nullable_with_function_call(async);

        AssertSql(
"""
SELECT SUBSTRING(CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[Nickname]
    ELSE NULL
END, 0 + 1, 3)
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
""");
    }

    public override async Task Byte_array_filter_by_length_literal_does_not_cast_on_varbinary_n(bool async)
    {
        await base.Byte_array_filter_by_length_literal_does_not_cast_on_varbinary_n(async);

        AssertSql(
"""
SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
WHERE DATALENGTH([s].[Banner5]) = 5
""");
    }

    public override async Task Correlated_collection_with_distinct_not_projecting_identifier_column(bool async)
    {
        await base.Correlated_collection_with_distinct_not_projecting_identifier_column(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [t].[Name], [t].[IsAutomatic]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
OUTER APPLY (
    SELECT DISTINCT [w].[Name], [w].[IsAutomatic]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
) AS [t]
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Name]
""");
    }

    public override async Task GetValueOrDefault_in_projection(bool async)
    {
        await base.GetValueOrDefault_in_projection(async);

        AssertSql(
"""
SELECT COALESCE([w].[SynergyWithId], 0)
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
""");
    }

    public override async Task Groupby_anonymous_type_with_navigations_followed_up_by_anonymous_projection_and_orderby(bool async)
    {
        await base.Groupby_anonymous_type_with_navigations_followed_up_by_anonymous_projection_and_orderby(async);

        AssertSql(
"""
SELECT [c].[Name], [c].[Location], COUNT(*) AS [Count]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [w].[OwnerFullName] = [g].[FullName]
LEFT JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g].[CityOfBirthName] = [c].[Name]
GROUP BY [c].[Name], [c].[Location]
ORDER BY [c].[Location]
""");
    }

    public override async Task Select_conditional_with_anonymous_type_and_null_constant(bool async)
    {
        await base.Select_conditional_with_anonymous_type_and_null_constant(async);

        AssertSql(
"""
SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [g].[HasSoulPatch]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
ORDER BY [g].[Nickname]
""");
    }

    public override async Task Left_join_with_GroupBy_with_composite_group_key(bool async)
    {
        await base.Left_join_with_GroupBy_with_composite_group_key(async);

        AssertSql(
"""
SELECT [g].[CityOfBirthName], [g].[HasSoulPatch]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [g].[SquadId] = [s].[Id]
LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[Nickname] = [t].[GearNickName]
GROUP BY [g].[CityOfBirthName], [g].[HasSoulPatch]
""");
    }

    public override async Task Select_multiple_conditions(bool async)
    {
        await base.Select_multiple_conditions(async);

        AssertSql(
"""
SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = CAST(0 AS bit) AND [w].[SynergyWithId] = 1 AND [w].[SynergyWithId] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsCartridge]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
""");
    }

    public override async Task Parameter_used_multiple_times_take_appropriate_inferred_type_mapping(bool async)
    {
        await base.Parameter_used_multiple_times_take_appropriate_inferred_type_mapping(async);

        AssertSql(
"""
@__place_0='Seattle' (Size = 4000)
@__place_0_1='Seattle' (Size = 100) (DbType = AnsiString)

SELECT [c].[Name], [c].[Location], [c].[Nation], [c].[PeriodEnd], [c].[PeriodStart]
FROM [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c]
WHERE [c].[Nation] = @__place_0 OR [c].[Location] = @__place_0_1 OR [c].[Location] = @__place_0_1
""");
    }

    public override async Task Left_join_predicate_value(bool async)
    {
        await base.Left_join_predicate_value(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[HasSoulPatch] = CAST(1 AS bit)
""");
    }

    public override async Task Include_collection_with_complex_OrderBy3(bool async)
    {
        await base.Include_collection_with_complex_OrderBy3(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[Nickname] = [g0].[LeaderNickname] AND [g].[SquadId] = [g0].[LeaderSquadId]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY COALESCE((
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]), CAST(0 AS bit)), [g].[Nickname], [g].[SquadId], [g0].[Nickname]
""");
    }

    public override async Task Correlated_collections_multiple_nested_complex_collections(bool async)
    {
        await base.Correlated_collections_multiple_nested_complex_collections(async);

        AssertSql(
"""
SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t].[Id], [g1].[Nickname], [g1].[SquadId], [t0].[FullName], [t0].[Nickname], [t0].[SquadId], [t0].[Id], [t0].[Nickname0], [t0].[SquadId0], [t0].[Id0], [t0].[Name], [t0].[IsAutomatic], [t0].[Id1], [t0].[Nickname00], [t0].[HasSoulPatch], [t0].[SquadId00], [t2].[Id], [t2].[AmmunitionType], [t2].[IsAutomatic], [t2].[Name], [t2].[OwnerFullName], [t2].[PeriodEnd], [t2].[PeriodStart], [t2].[SynergyWithId], [t2].[Nickname], [t2].[SquadId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[Nickname] = [t].[GearNickName] AND [g].[SquadId] = [t].[GearSquadId]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g1] ON [t].[GearNickName] = [g1].[Nickname] AND [t].[GearSquadId] = [g1].[SquadId]
LEFT JOIN (
    SELECT [g2].[FullName], [g2].[Nickname], [g2].[SquadId], [t1].[Id], [t1].[Nickname] AS [Nickname0], [t1].[SquadId] AS [SquadId0], [t1].[Id0], [t1].[Name], [t1].[IsAutomatic], [t1].[Id1], [t1].[Nickname0] AS [Nickname00], [t1].[HasSoulPatch], [t1].[SquadId0] AS [SquadId00], [g2].[Rank], [t1].[IsAutomatic0], [g2].[LeaderNickname], [g2].[LeaderSquadId]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g2]
    LEFT JOIN (
        SELECT [w].[Id], [g3].[Nickname], [g3].[SquadId], [s].[Id] AS [Id0], [w0].[Name], [w0].[IsAutomatic], [w0].[Id] AS [Id1], [g4].[Nickname] AS [Nickname0], [g4].[HasSoulPatch], [g4].[SquadId] AS [SquadId0], [w].[IsAutomatic] AS [IsAutomatic0], [w].[OwnerFullName]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g3] ON [w].[OwnerFullName] = [g3].[FullName]
        LEFT JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [g3].[SquadId] = [s].[Id]
        LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0] ON [g3].[FullName] = [w0].[OwnerFullName]
        LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g4] ON [s].[Id] = [g4].[SquadId]
        WHERE [w].[Name] <> N'Bar' OR [w].[Name] IS NULL
    ) AS [t1] ON [g2].[FullName] = [t1].[OwnerFullName]
    WHERE [g2].[FullName] <> N'Foo'
) AS [t0] ON [g].[Nickname] = [t0].[LeaderNickname] AND [g].[SquadId] = [t0].[LeaderSquadId]
LEFT JOIN (
    SELECT [w1].[Id], [w1].[AmmunitionType], [w1].[IsAutomatic], [w1].[Name], [w1].[OwnerFullName], [w1].[PeriodEnd], [w1].[PeriodStart], [w1].[SynergyWithId], [g5].[Nickname], [g5].[SquadId]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w1]
    LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g5] ON [w1].[OwnerFullName] = [g5].[FullName]
) AS [t2] ON [g1].[FullName] = [t2].[OwnerFullName]
WHERE [g].[Discriminator] = N'Officer' AND EXISTS (
    SELECT 1
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    WHERE [g].[Nickname] = [g0].[LeaderNickname] AND [g].[SquadId] = [g0].[LeaderSquadId])
ORDER BY [g].[HasSoulPatch] DESC, [t].[Note], [g].[Nickname], [g].[SquadId], [t].[Id], [g1].[Nickname], [g1].[SquadId], [t0].[Rank], [t0].[Nickname], [t0].[SquadId], [t0].[IsAutomatic0], [t0].[Id], [t0].[Nickname0], [t0].[SquadId0], [t0].[Id0], [t0].[Id1], [t0].[Nickname00], [t0].[SquadId00], [t2].[IsAutomatic], [t2].[Nickname] DESC, [t2].[Id]
""");
    }

    public override async Task Byte_array_contains_parameter(bool async)
    {
        await base.Byte_array_contains_parameter(async);

        AssertSql(
"""
@__someByte_0='1' (Size = 1)

SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
WHERE CHARINDEX(CAST(@__someByte_0 AS varbinary(max)), [s].[Banner]) > 0
""");
    }

    public override async Task GroupBy_Property_Include_Select_Count(bool async)
    {
        await base.GroupBy_Property_Include_Select_Count(async);

        AssertSql(
"""
SELECT COUNT(*)
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
GROUP BY [g].[Rank]
""");
    }

    public override async Task Where_TimeSpan_Hours(bool async)
    {
        await base.Where_TimeSpan_Hours(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE DATEPART(hour, [m].[Duration]) = 1
""");
    }

    public override async Task Select_comparison_with_null(bool async)
    {
        await base.Select_comparison_with_null(async);

        AssertSql(
"""
@__ammunitionType_0='1' (Nullable = true)

SELECT [w].[Id], CASE
    WHEN [w].[AmmunitionType] = @__ammunitionType_0 AND [w].[AmmunitionType] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Cartridge]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE [w].[AmmunitionType] = @__ammunitionType_0
""",
            //
"""
SELECT [w].[Id], CASE
    WHEN [w].[AmmunitionType] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Cartridge]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE [w].[AmmunitionType] IS NULL
""");
    }

    public override async Task Include_with_projection_of_unmapped_property_still_gets_applied(bool async)
    {
        await base.Include_with_projection_of_unmapped_property_still_gets_applied(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] = [w].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task OfTypeNav2(bool async)
    {
        await base.OfTypeNav2(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[Nickname] = [t].[GearNickName] AND [g].[SquadId] = [t].[GearSquadId]
LEFT JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g].[AssignedCityName] = [c].[Name]
WHERE ([t].[Note] <> N'Foo' OR [t].[Note] IS NULL) AND [g].[Discriminator] = N'Officer' AND ([c].[Location] <> 'Bar' OR [c].[Location] IS NULL)
""");
    }

    public override async Task Enum_matching_take_value_gets_different_type_mapping(bool async)
    {
        await base.Enum_matching_take_value_gets_different_type_mapping(async);

        AssertSql(
"""
@__p_0='1'
@__value_1='1'

SELECT TOP(@__p_0) [g].[Rank] & @__value_1
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
ORDER BY [g].[Nickname]
""");
    }

    public override async Task Include_with_join_collection1(bool async)
    {
        await base.Include_with_join_collection1(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [t].[Id], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[SquadId] = [t].[GearSquadId] AND [g].[Nickname] = [t].[GearNickName]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] = [w].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Id]
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_list_initializers(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_list_initializers(async);

        AssertSql(
"""
SELECT [g].[SquadId], [g].[SquadId] + 1
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE [t].[Note] <> N'K.I.A.' OR [t].[Note] IS NULL
ORDER BY [t].[Note]
""");
    }

    public override async Task Contains_on_collection_of_byte_subquery(bool async)
    {
        await base.Contains_on_collection_of_byte_subquery(async);

        AssertSql(
"""
SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
WHERE EXISTS (
    SELECT 1
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    WHERE [l0].[ThreatLevelByte] = [l].[ThreatLevelByte])
""");
    }

    public override async Task Left_join_predicate_condition_equals_condition(bool async)
    {
        await base.Left_join_predicate_condition_equals_condition(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [w].[SynergyWithId] IS NOT NULL
""");
    }

    public override async Task Conditional_with_conditions_evaluating_to_true_gets_optimized(bool async)
    {
        await base.Conditional_with_conditions_evaluating_to_true_gets_optimized(async);

        AssertSql(
"""
SELECT [g].[CityOfBirthName]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Correlated_collections_on_select_many(bool async)
    {
        await base.Correlated_collections_on_select_many(async);

        AssertSql(
"""
SELECT [g].[Nickname], [s].[Name], [g].[SquadId], [s].[Id], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[PeriodEnd], [t].[PeriodStart], [t].[SynergyWithId], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
CROSS JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit) OR [w].[Name] <> N'foo' OR [w].[Name] IS NULL
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    WHERE [g0].[HasSoulPatch] = CAST(0 AS bit)
) AS [t0] ON [s].[Id] = [t0].[SquadId]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
ORDER BY [g].[Nickname], [s].[Id] DESC, [g].[SquadId], [t].[Id], [t0].[Nickname]
""");
    }

    public override async Task Bitwise_operation_with_non_null_parameter_optimizes_null_checks(bool async)
    {
        await base.Bitwise_operation_with_non_null_parameter_optimizes_null_checks(async);

        AssertSql(
"""
@__ranks_0='134'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Rank] & @__ranks_0 <> 0
""",
            //
"""
@__ranks_0='134'

SELECT CASE
    WHEN [g].[Rank] | @__ranks_0 = @__ranks_0 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""",
            //
"""
@__ranks_0='134'

SELECT CASE
    WHEN [g].[Rank] | [g].[Rank] | @__ranks_0 | [g].[Rank] | @__ranks_0 = @__ranks_0 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Correlated_collections_project_anonymous_collection_result(bool async)
    {
        await base.Correlated_collections_project_anonymous_collection_result(async);

        AssertSql(
"""
SELECT [s].[Name], [s].[Id], [g].[FullName], [g].[Rank], [g].[Nickname], [g].[SquadId]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [s].[Id] = [g].[SquadId]
WHERE [s].[Id] < 20
ORDER BY [s].[Id], [g].[Nickname]
""");
    }

    public override async Task Navigation_based_on_complex_expression3(bool async)
    {
        await base.Navigation_based_on_complex_expression3(async);

        AssertSql(
"""
SELECT [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[PeriodEnd], [t].[PeriodStart], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
LEFT JOIN (
    SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
""");
    }

    public override async Task Correlated_collection_order_by_constant(bool async)
    {
        await base.Correlated_collection_order_by_constant(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [w].[Name], [w].[Id]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] = [w].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Join_predicate_value_equals_condition(bool async)
    {
        await base.Join_predicate_value_equals_condition(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [w].[SynergyWithId] IS NOT NULL
""");
    }

    public override async Task GetValueOrDefault_with_argument_complex(bool async)
    {
        await base.GetValueOrDefault_with_argument_complex(async);

        AssertSql(
"""
SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE COALESCE([w].[SynergyWithId], CAST(LEN([w].[Name]) AS int) + 42) > 10
""");
    }

    public override async Task Non_unicode_string_literals_is_used_for_non_unicode_column_in_subquery(bool async)
    {
        await base.Non_unicode_string_literals_is_used_for_non_unicode_column_in_subquery(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g].[CityOfBirthName] = [c].[Name]
WHERE [g].[Nickname] = N'Marcus' AND [c].[Location] = 'Jacinto''s location'
""");
    }

    public override async Task Where_count_subquery_without_collision(bool async)
    {
        await base.Where_count_subquery_without_collision(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE (
    SELECT COUNT(*)
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]) = 2
""");
    }

    public override async Task Select_null_parameter(bool async)
    {
        await base.Select_null_parameter(async);

        AssertSql(
"""
@__ammunitionType_0='1' (Nullable = true)

SELECT [w].[Id], @__ammunitionType_0 AS [AmmoType]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
""",
            //
"""
SELECT [w].[Id], NULL AS [AmmoType]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
""",
            //
"""
@__ammunitionType_0='2' (Nullable = true)

SELECT [w].[Id], @__ammunitionType_0 AS [AmmoType]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
""",
            //
"""
SELECT [w].[Id], NULL AS [AmmoType]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
""");
    }

    public override async Task Project_collection_navigation_nested_with_take_composite_key(bool async)
    {
        await base.Project_collection_navigation_nested_with_take_composite_key(async);

        AssertSql(
"""
SELECT [t].[Id], [g].[Nickname], [g].[SquadId], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Rank]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
LEFT JOIN (
    SELECT [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[Discriminator], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Rank]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank], ROW_NUMBER() OVER(PARTITION BY [g0].[LeaderNickname], [g0].[LeaderSquadId] ORDER BY [g0].[Nickname], [g0].[SquadId]) AS [row]
        FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    ) AS [t1]
    WHERE [t1].[row] <= 50
) AS [t0] ON ([g].[Nickname] = [t0].[LeaderNickname] OR ([g].[Nickname] IS NULL AND [t0].[LeaderNickname] IS NULL)) AND [g].[SquadId] = [t0].[LeaderSquadId]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [t].[Id], [g].[Nickname], [g].[SquadId], [t0].[Nickname]
""");
    }

    public override async Task Filter_on_subquery_projecting_one_value_type_from_empty_collection(bool async)
    {
        await base.Filter_on_subquery_projecting_one_value_type_from_empty_collection(async);

        AssertSql(
"""
SELECT [s].[Name]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
WHERE [s].[Name] = N'Kilo' AND COALESCE((
    SELECT TOP(1) [g].[SquadId]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [s].[Id] = [g].[SquadId] AND [g].[HasSoulPatch] = CAST(1 AS bit)), 0) <> 0
""");
    }

    public override async Task Correlated_collection_with_inner_collection_references_element_two_levels_up(bool async)
    {
        await base.Correlated_collection_with_inner_collection_references_element_two_levels_up(async);

        AssertSql(
"""
SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t].[ReportName], [t].[OfficerName], [t].[Nickname], [t].[SquadId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
OUTER APPLY (
    SELECT [g0].[FullName] AS [ReportName], [g].[FullName] AS [OfficerName], [g0].[Nickname], [g0].[SquadId]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    WHERE [g].[Nickname] = [g0].[LeaderNickname] AND [g].[SquadId] = [g0].[LeaderSquadId]
) AS [t]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Nickname]
""");
    }

    public override async Task Where_subquery_distinct_first_boolean(bool async)
    {
        await base.Where_subquery_distinct_first_boolean(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit) AND (
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
    ) AS [t]
    ORDER BY [t].[Id]) = CAST(1 AS bit)
ORDER BY [g].[Nickname]
""");
    }

    public override async Task GroupBy_Select_sum(bool async)
    {
        await base.GroupBy_Select_sum(async);

        AssertSql(
"""
SELECT COALESCE(SUM([m].[Rating]), 0.0E0)
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
GROUP BY [m].[CodeName]
""");
    }

    public override async Task Group_by_with_include_with_entity_in_result_selector(bool async)
    {
        await base.Group_by_with_include_with_entity_in_result_selector(async);

        AssertSql(
"""
SELECT [t].[Rank], [t].[c], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Rank], [t0].[Name], [t0].[Location], [t0].[Nation], [t0].[PeriodEnd0], [t0].[PeriodStart0]
FROM (
    SELECT [g].[Rank], COUNT(*) AS [c]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    GROUP BY [g].[Rank]
) AS [t]
LEFT JOIN (
    SELECT [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[Discriminator], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Rank], [t1].[Name], [t1].[Location], [t1].[Nation], [t1].[PeriodEnd0], [t1].[PeriodStart0]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank], [c].[Name], [c].[Location], [c].[Nation], [c].[PeriodEnd] AS [PeriodEnd0], [c].[PeriodStart] AS [PeriodStart0], ROW_NUMBER() OVER(PARTITION BY [g0].[Rank] ORDER BY [g0].[Nickname]) AS [row]
        FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
        INNER JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g0].[CityOfBirthName] = [c].[Name]
    ) AS [t1]
    WHERE [t1].[row] <= 1
) AS [t0] ON [t].[Rank] = [t0].[Rank]
ORDER BY [t].[Rank]
""");
    }

    public override async Task Select_subquery_boolean_empty(bool async)
    {
        await base.Select_subquery_boolean_empty(async);

        AssertSql(
"""
SELECT COALESCE((
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName] AND [w].[Name] = N'BFG'
    ORDER BY [w].[Id]), CAST(0 AS bit))
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_orderby(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_orderby(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE [t].[Note] <> N'K.I.A.' OR [t].[Note] IS NULL
ORDER BY [g].[SquadId]
""");
    }

    public override async Task Cast_OfType_works_correctly(bool async)
    {
        await base.Cast_OfType_works_correctly(async);

        AssertSql(
"""
SELECT [g].[FullName]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Discriminator] = N'Officer'
""");
    }

    public override async Task Correlated_collections_basic_projecting_constant_bool(bool async)
    {
        await base.Correlated_collections_basic_projecting_constant_bool(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [t].[c], [t].[Id]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT CAST(1 AS bit) AS [c], [w].[Id], [w].[OwnerFullName]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit) OR [w].[Name] <> N'foo' OR [w].[Name] IS NULL
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
WHERE [g].[Nickname] <> N'Marcus'
ORDER BY [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Correlated_collection_after_distinct_3_levels(bool async)
    {
        await base.Correlated_collection_after_distinct_3_levels(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t1].[Nickname], [t1].[FullName], [t1].[HasSoulPatch], [t1].[Id], [t1].[Name], [t1].[Nickname0], [t1].[FullName0], [t1].[HasSoulPatch0], [t1].[Id0]
FROM (
    SELECT DISTINCT [s].[Id], [s].[Name]
    FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
) AS [t]
OUTER APPLY (
    SELECT [t0].[Nickname], [t0].[FullName], [t0].[HasSoulPatch], [t2].[Id], [t2].[Name], [t2].[Nickname] AS [Nickname0], [t2].[FullName] AS [FullName0], [t2].[HasSoulPatch] AS [HasSoulPatch0], [t2].[Id0]
    FROM (
        SELECT DISTINCT [g].[Nickname], [g].[FullName], [g].[HasSoulPatch]
        FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
        WHERE [g].[SquadId] = [t].[Id]
    ) AS [t0]
    OUTER APPLY (
        SELECT [t].[Id], [t].[Name], [t0].[Nickname], [t0].[FullName], [t0].[HasSoulPatch], [w].[Id] AS [Id0]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [w].[OwnerFullName] = [t0].[FullName]
    ) AS [t2]
) AS [t1]
ORDER BY [t].[Id], [t1].[Nickname], [t1].[FullName], [t1].[HasSoulPatch]
""");
    }

    public override async Task Null_propagation_optimization5(bool async)
    {
        await base.Null_propagation_optimization5(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN CAST(LEN([g].[LeaderNickname]) AS int)
    ELSE NULL
END = 5 AND CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN CAST(LEN([g].[LeaderNickname]) AS int)
    ELSE NULL
END IS NOT NULL
""");
    }

    public override async Task Navigation_access_fk_on_derived_entity_using_cast(bool async)
    {
        await base.Navigation_access_fk_on_derived_entity_using_cast(async);

        AssertSql(
"""
SELECT [f].[Name], [t].[Name] AS [CommanderName]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
LEFT JOIN (
    SELECT [l].[Name]
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
ORDER BY [f].[Name]
""");
    }

    public override async Task First_on_byte_array(bool async)
    {
        await base.First_on_byte_array(async);

        AssertSql(
"""
SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
WHERE CAST(SUBSTRING([s].[Banner], 1, 1) AS tinyint) = CAST(2 AS tinyint)
""");
    }

    public override async Task
        Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation_complex(bool async)
    {
        await base.Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation_complex(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
LEFT JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [g].[SquadId] = [s].[Id]
WHERE SUBSTRING([t].[Note], 0 + 1, CAST(LEN([s].[Name]) AS int)) = [t].[GearNickName] OR (([t].[Note] IS NULL OR [s].[Name] IS NULL) AND [t].[GearNickName] IS NULL)
""");
    }

    public override async Task Project_one_value_type_with_client_projection_from_empty_collection(bool async)
    {
        await base.Project_one_value_type_with_client_projection_from_empty_collection(async);

        AssertSql(
"""
SELECT [s].[Name], [t0].[SquadId], [t0].[LeaderSquadId], [t0].[c]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
LEFT JOIN (
    SELECT [t].[SquadId], [t].[LeaderSquadId], [t].[c]
    FROM (
        SELECT [g].[SquadId], [g].[LeaderSquadId], 1 AS [c], ROW_NUMBER() OVER(PARTITION BY [g].[SquadId] ORDER BY [g].[Nickname], [g].[SquadId]) AS [row]
        FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
        WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [s].[Id] = [t0].[SquadId]
WHERE [s].[Name] = N'Kilo'
""");
    }

    public override async Task Optional_Navigation_Null_Coalesce_To_Clr_Type(bool async)
    {
        await base.Optional_Navigation_Null_Coalesce_To_Clr_Type(async);

        AssertSql(
"""
SELECT TOP(1) COALESCE([w0].[IsAutomatic], CAST(0 AS bit)) AS [IsAutomatic]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY [w].[Id]
""");
    }

    public override async Task Where_datetimeoffset_utcnow(bool async)
    {
        await base.Where_datetimeoffset_utcnow(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE [m].[Timeline] <> CAST(SYSUTCDATETIME() AS datetimeoffset)
""");
    }

    public override async Task Nav_rewrite_with_convert3(bool async)
    {
        await base.Nav_rewrite_with_convert3(async);

        AssertSql(
"""
SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[PeriodEnd], [f].[PeriodStart], [f].[ServerAddress], [f].[CommanderName], [f].[Eradicated]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
LEFT JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [f].[CapitalName] = [c].[Name]
LEFT JOIN (
    SELECT [l].[Name]
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
WHERE ([c].[Name] <> N'Foo' OR [c].[Name] IS NULL) AND ([t].[Name] <> N'Bar' OR [t].[Name] IS NULL)
""");
    }

    public override async Task Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion(bool async)
    {
        await base.Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion(async);

        AssertSql(
"""
SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Rank]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
CROSS APPLY (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE EXISTS (
        SELECT 1
        FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        WHERE [l0].[ThreatLevelByte] = [l].[ThreatLevelByte])
) AS [t]
""");
    }

    public override async Task Correlated_collections_with_funky_orderby_complex_scenario1(bool async)
    {
        await base.Correlated_collections_with_funky_orderby_complex_scenario1(async);

        AssertSql(
"""
SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t].[Id], [t].[Nickname], [t].[SquadId], [t].[Id0], [t].[Nickname0], [t].[HasSoulPatch], [t].[SquadId0]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [w].[Id], [g0].[Nickname], [g0].[SquadId], [s].[Id] AS [Id0], [g1].[Nickname] AS [Nickname0], [g1].[HasSoulPatch], [g1].[SquadId] AS [SquadId0], [w].[OwnerFullName]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [w].[OwnerFullName] = [g0].[FullName]
    LEFT JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [g0].[SquadId] = [s].[Id]
    LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g1] ON [s].[Id] = [g1].[SquadId]
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
ORDER BY [g].[FullName], [g].[Nickname] DESC, [g].[SquadId], [t].[Id], [t].[Nickname], [t].[SquadId], [t].[Id0], [t].[Nickname0]
""");
    }

    public override async Task Contains_on_collection_of_nullable_byte_subquery(bool async)
    {
        await base.Contains_on_collection_of_nullable_byte_subquery(async);

        AssertSql(
"""
SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
WHERE EXISTS (
    SELECT 1
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    WHERE [l0].[ThreatLevelNullableByte] = [l].[ThreatLevelNullableByte] OR ([l0].[ThreatLevelNullableByte] IS NULL AND [l].[ThreatLevelNullableByte] IS NULL))
""");
    }

    public override async Task Where_datetimeoffset_now(bool async)
    {
        await base.Where_datetimeoffset_now(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE [m].[Timeline] <> SYSDATETIMEOFFSET()
""");
    }

    public override async Task Select_subquery_int_with_inside_cast_and_coalesce(bool async)
    {
        await base.Select_subquery_int_with_inside_cast_and_coalesce(async);

        AssertSql(
"""
SELECT COALESCE((
    SELECT TOP(1) [w].[Id]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]), 42)
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task String_compare_with_null_conditional_argument(bool async)
    {
        await base.String_compare_with_null_conditional_argument(async);

        AssertSql(
"""
SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[PeriodEnd], [w0].[PeriodStart], [w0].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY CASE
    WHEN [w0].[Name] = N'Marcus'' Lancer' AND [w0].[Name] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Correlated_collections_basic_projection_explicit_to_array(bool async)
    {
        await base.Correlated_collections_basic_projection_explicit_to_array(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[PeriodEnd], [t].[PeriodStart], [t].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit) OR [w].[Name] <> N'foo' OR [w].[Name] IS NULL
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
WHERE [g].[Nickname] <> N'Marcus'
ORDER BY [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Subquery_is_lifted_from_additional_from_clause(bool async)
    {
        await base.Subquery_is_lifted_from_additional_from_clause(async);

        AssertSql(
"""
SELECT [g].[FullName] AS [Name1], [g0].[FullName] AS [Name2]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
CROSS JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit) AND [g0].[HasSoulPatch] = CAST(0 AS bit)
ORDER BY [g].[FullName]
""");
    }

    public override async Task DateTimeOffset_DateAdd_AddMilliseconds(bool async)
    {
        await base.DateTimeOffset_DateAdd_AddMilliseconds(async);

        AssertSql(
"""
SELECT DATEADD(millisecond, CAST(300.0E0 AS int), [m].[Timeline])
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
""");
    }

    public override async Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery(bool async)
    {
        await base.Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery(async);

        AssertSql(
"""
SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t].[Id], [g1].[Nickname], [g1].[SquadId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[SynergyWithId], [t0].[Nickname], [t0].[SquadId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[Nickname] = [t].[GearNickName] AND [g].[SquadId] = [t].[GearSquadId]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g1] ON [t].[GearNickName] = [g1].[Nickname] AND [t].[GearSquadId] = [g1].[SquadId]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId], [g2].[Nickname], [g2].[SquadId]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g2] ON [w].[OwnerFullName] = [g2].[FullName]
) AS [t0] ON [g1].[FullName] = [t0].[OwnerFullName]
WHERE [g].[Discriminator] = N'Officer' AND EXISTS (
    SELECT 1
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    WHERE [g].[Nickname] = [g0].[LeaderNickname] AND [g].[SquadId] = [g0].[LeaderSquadId])
ORDER BY [g].[HasSoulPatch] DESC, [t].[Note], [g].[Nickname], [g].[SquadId], [t].[Id], [g1].[Nickname], [g1].[SquadId], [t0].[IsAutomatic], [t0].[Nickname] DESC, [t0].[Id]
""");
    }

    public override async Task Member_access_on_derived_entity_using_cast(bool async)
    {
        await base.Member_access_on_derived_entity_using_cast(async);

        AssertSql(
"""
SELECT [f].[Name], [f].[Eradicated]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
ORDER BY [f].[Name]
""");
    }

    public override async Task Double_order_by_on_string_compare(bool async)
    {
        await base.Double_order_by_on_string_compare(async);

        AssertSql(
"""
SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
ORDER BY CASE
    WHEN [w].[Name] = N'Marcus'' Lancer' AND [w].[Name] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [w].[Id]
""");
    }

    public override async Task Join_on_entity_qsre_keys_inner_key_is_navigation(bool async)
    {
        await base.Join_on_entity_qsre_keys_inner_key_is_navigation(async);

        AssertSql(
"""
SELECT [c].[Name] AS [CityName], [t].[Nickname] AS [GearNickname]
FROM [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c]
INNER JOIN (
    SELECT [g].[Nickname], [c0].[Name]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    LEFT JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c0] ON [g].[AssignedCityName] = [c0].[Name]
) AS [t] ON [c].[Name] = [t].[Name]
""");
    }

    public override async Task Correlated_collections_with_Take(bool async)
    {
        await base.Correlated_collections_with_Take(async);

        AssertSql(
"""
SELECT [s].[Id], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Rank]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
LEFT JOIN (
    SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Rank]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], ROW_NUMBER() OVER(PARTITION BY [g].[SquadId] ORDER BY [g].[Nickname]) AS [row]
        FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    ) AS [t]
    WHERE [t].[row] <= 2
) AS [t0] ON [s].[Id] = [t0].[SquadId]
ORDER BY [s].[Name], [s].[Id], [t0].[SquadId], [t0].[Nickname]
""");
    }

    public override async Task Correlated_collection_take(bool async)
    {
        await base.Correlated_collection_take(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [c].[Name], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[SynergyWithId], [c].[Location], [c].[Nation], [c].[PeriodEnd], [c].[PeriodStart]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g].[CityOfBirthName] = [c].[Name]
LEFT JOIN (
    SELECT [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[PeriodEnd], [t].[PeriodStart], [t].[SynergyWithId]
    FROM (
        SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId], ROW_NUMBER() OVER(PARTITION BY [w].[OwnerFullName] ORDER BY [w].[Id]) AS [row]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    ) AS [t]
    WHERE [t].[row] <= 10
) AS [t0] ON [g].[FullName] = [t0].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [c].[Name]
""");
    }

    public override async Task Enum_ToString_is_client_eval(bool async)
    {
        await base.Enum_ToString_is_client_eval(async);

        AssertSql(
"""
SELECT [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
ORDER BY [g].[SquadId], [g].[Nickname]
""");
    }

    public override async Task Include_with_nested_navigation_in_order_by(bool async)
    {
        await base.Include_with_nested_navigation_in_order_by(async);

        AssertSql(
"""
SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [w].[OwnerFullName] = [g].[FullName]
LEFT JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g].[CityOfBirthName] = [c].[Name]
WHERE [g].[Nickname] <> N'Paduk' OR [g].[Nickname] IS NULL
ORDER BY [c].[Name], [w].[Id]
""");
    }

    public override async Task Negated_bool_ternary_inside_anonymous_type_in_projection(bool async)
    {
        await base.Negated_bool_ternary_inside_anonymous_type_in_projection(async);

        AssertSql(
"""
SELECT CASE
    WHEN CASE
        WHEN [g].[HasSoulPatch] = CAST(1 AS bit) THEN CAST(1 AS bit)
        ELSE COALESCE([g].[HasSoulPatch], CAST(1 AS bit))
    END = CAST(0 AS bit) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [c]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
""");
    }

    public override async Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery_complex_orderings(
        bool async)
    {
        await base.Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery_complex_orderings(async);

        AssertSql(
"""
SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t].[Id], [g1].[Nickname], [g1].[SquadId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[SynergyWithId], [t0].[Nickname], [t0].[SquadId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[Nickname] = [t].[GearNickName] AND [g].[SquadId] = [t].[GearSquadId]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g1] ON [t].[GearNickName] = [g1].[Nickname] AND [t].[GearSquadId] = [g1].[SquadId]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId], [g2].[Nickname], [g2].[SquadId], (
        SELECT COUNT(*)
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0]
        WHERE [g2].[FullName] IS NOT NULL AND [g2].[FullName] = [w0].[OwnerFullName]) AS [c]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g2] ON [w].[OwnerFullName] = [g2].[FullName]
) AS [t0] ON [g1].[FullName] = [t0].[OwnerFullName]
WHERE [g].[Discriminator] = N'Officer' AND EXISTS (
    SELECT 1
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    WHERE [g].[Nickname] = [g0].[LeaderNickname] AND [g].[SquadId] = [g0].[LeaderSquadId])
ORDER BY [g].[HasSoulPatch] DESC, [t].[Note], [g].[Nickname], [g].[SquadId], [t].[Id], [g1].[Nickname], [g1].[SquadId], [t0].[Id] DESC, [t0].[c], [t0].[Nickname]
""");
    }

    public override async Task Include_reference_on_derived_type_using_string_nested2(bool async)
    {
        await base.Include_reference_on_derived_type_using_string_nested2(async);

        AssertSql(
"""
SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Rank], [t].[Name], [t].[Location], [t].[Nation], [t].[PeriodEnd0], [t].[PeriodStart0]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [l].[DefeatedByNickname] = [g].[Nickname] AND [l].[DefeatedBySquadId] = [g].[SquadId]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank], [c].[Name], [c].[Location], [c].[Nation], [c].[PeriodEnd] AS [PeriodEnd0], [c].[PeriodStart] AS [PeriodStart0]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    INNER JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g0].[CityOfBirthName] = [c].[Name]
) AS [t] ON ([g].[Nickname] = [t].[LeaderNickname] OR ([g].[Nickname] IS NULL AND [t].[LeaderNickname] IS NULL)) AND [g].[SquadId] = [t].[LeaderSquadId]
ORDER BY [l].[Name], [g].[Nickname], [g].[SquadId], [t].[Nickname], [t].[SquadId]
""");
    }

    public override async Task Group_by_on_StartsWith_with_null_parameter_as_argument(bool async)
    {
        await base.Group_by_on_StartsWith_with_null_parameter_as_argument(async);

        AssertSql(
"""
SELECT [t].[Key]
FROM (
    SELECT CAST(0 AS bit) AS [Key]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
) AS [t]
GROUP BY [t].[Key]
""");
    }

    public override async Task Where_is_properly_lifted_from_subquery_created_by_include(bool async)
    {
        await base.Where_is_properly_lifted_from_subquery_created_by_include(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[Nickname] = [t].[GearNickName] AND [g].[SquadId] = [t].[GearSquadId]
WHERE [g].[FullName] <> N'Augustus Cole' AND [g].[HasSoulPatch] = CAST(0 AS bit)
ORDER BY [g].[FullName]
""");
    }

    public override async Task Where_datetimeoffset_dayofyear_component(bool async)
    {
        await base.Where_datetimeoffset_dayofyear_component(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE DATEPART(dayofyear, [m].[Timeline]) = 2
""");
    }

    public override async Task DateTimeOffset_DateAdd_AddDays(bool async)
    {
        await base.DateTimeOffset_DateAdd_AddDays(async);

        AssertSql(
"""
SELECT DATEADD(day, CAST(1.0E0 AS int), [m].[Timeline])
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
""");
    }

    public override async Task Null_semantics_on_nullable_bool_from_inner_join_subquery_is_fully_applied(bool async)
    {
        await base.Null_semantics_on_nullable_bool_from_inner_join_subquery_is_fully_applied(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[CapitalName], [t].[Discriminator], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t].[ServerAddress], [t].[CommanderName], [t].[Eradicated]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
INNER JOIN (
    SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[PeriodEnd], [f].[PeriodStart], [f].[ServerAddress], [f].[CommanderName], [f].[Eradicated]
    FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
    WHERE [f].[Name] = N'Swarm'
) AS [t] ON [l].[Name] = [t].[CommanderName]
WHERE [t].[Eradicated] <> CAST(1 AS bit) OR [t].[Eradicated] IS NULL
""");
    }

    public override async Task Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion_negated(bool async)
    {
        await base.Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion_negated(async);

        AssertSql(
"""
SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Rank]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
CROSS APPLY (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE NOT (EXISTS (
        SELECT 1
        FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        WHERE [l0].[ThreatLevelNullableByte] = [l].[ThreatLevelNullableByte] OR ([l0].[ThreatLevelNullableByte] IS NULL AND [l].[ThreatLevelNullableByte] IS NULL)))
) AS [t]
""");
    }

    public override async Task Select_Where_Navigation_Equals_Navigation(bool async)
    {
        await base.Select_Where_Navigation_Equals_Navigation(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart], [t0].[Id], [t0].[GearNickName], [t0].[GearSquadId], [t0].[IssueDate], [t0].[Note], [t0].[PeriodEnd], [t0].[PeriodStart]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
CROSS JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t0]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [t0].[GearNickName] = [g0].[Nickname] AND [t0].[GearSquadId] = [g0].[SquadId]
WHERE ([g].[Nickname] = [g0].[Nickname] OR ([g].[Nickname] IS NULL AND [g0].[Nickname] IS NULL)) AND ([g].[SquadId] = [g0].[SquadId] OR ([g].[SquadId] IS NULL AND [g0].[SquadId] IS NULL))
""");
    }

    public override async Task Array_access_on_byte_array(bool async)
    {
        await base.Array_access_on_byte_array(async);

        AssertSql(
"""
SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
WHERE CAST(SUBSTRING([s].[Banner5], 2 + 1, 1) AS tinyint) = CAST(6 AS tinyint)
""");
    }

    public override async Task Where_contains_on_navigation_with_composite_keys(bool async)
    {
        await base.Where_contains_on_navigation_with_composite_keys(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE EXISTS (
    SELECT 1
    FROM [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c]
    WHERE EXISTS (
        SELECT 1
        FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
        WHERE [c].[Name] = [g0].[CityOfBirthName] AND [g0].[Nickname] = [g].[Nickname] AND [g0].[SquadId] = [g].[SquadId]))
""");
    }

    public override async Task Cast_to_derived_type_after_OfType_works(bool async)
    {
        await base.Cast_to_derived_type_after_OfType_works(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Discriminator] = N'Officer'
""");
    }

    public override async Task Select_Where_Navigation(bool async)
    {
        await base.Select_Where_Navigation(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE [g].[Nickname] = N'Marcus'
""");
    }

    public override async Task Project_navigation_defined_on_base_from_entity_with_inheritance_using_soft_cast(bool async)
    {
        await base.Project_navigation_defined_on_base_from_entity_with_inheritance_using_soft_cast(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart], CASE
    WHEN [t].[Id] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsNull], [c].[Name], [c].[Location], [c].[Nation], [c].[PeriodEnd], [c].[PeriodStart], CASE
    WHEN [c].[Name] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsNull], [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart], CASE
    WHEN [s].[Id] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsNull]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[Nickname] = [t].[GearNickName] AND [g].[SquadId] = [t].[GearSquadId]
LEFT JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g].[CityOfBirthName] = [c].[Name]
LEFT JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [g].[SquadId] = [s].[Id]
""");
    }

    public override async Task Take_without_orderby_followed_by_orderBy_is_pushed_down2(bool async)
    {
        await base.Take_without_orderby_followed_by_orderBy_is_pushed_down2(async);

        AssertSql(
"""
@__p_0='999'

SELECT [t].[FullName]
FROM (
    SELECT TOP(@__p_0) [g].[FullName], [g].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [g].[HasSoulPatch] = CAST(0 AS bit)
) AS [t]
ORDER BY [t].[Rank]
""");
    }

    public override async Task OrderBy_bool_coming_from_optional_navigation(bool async)
    {
        await base.OrderBy_bool_coming_from_optional_navigation(async);

        AssertSql(
"""
SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[PeriodEnd], [w0].[PeriodStart], [w0].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY [w0].[IsAutomatic]
""");
    }

    public override async Task Where_subquery_boolean_with_pushdown(bool async)
    {
        await base.Where_subquery_boolean_with_pushdown(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE (
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]) = CAST(1 AS bit)
""");
    }

    public override async Task Where_subquery_distinct_firstordefault_boolean(bool async)
    {
        await base.Where_subquery_distinct_firstordefault_boolean(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit) AND COALESCE((
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
    ) AS [t]
    ORDER BY [t].[Id]), CAST(0 AS bit)) = CAST(1 AS bit)
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_predicate(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_predicate(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE ([t].[Note] <> N'K.I.A.' OR [t].[Note] IS NULL) AND [g].[HasSoulPatch] = CAST(1 AS bit)
""");
    }

    public override async Task Nullable_bool_comparison_is_translated_to_server(bool async)
    {
        await base.Nullable_bool_comparison_is_translated_to_server(async);

        AssertSql(
"""
SELECT CASE
    WHEN [f].[Eradicated] = CAST(1 AS bit) AND [f].[Eradicated] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsEradicated]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
""");
    }

    public override async Task Where_bitwise_and_nullable_enum_with_non_nullable_parameter(bool async)
    {
        await base.Where_bitwise_and_nullable_enum_with_non_nullable_parameter(async);

        AssertSql(
"""
@__ammunitionType_0='1'

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE [w].[AmmunitionType] & @__ammunitionType_0 > 0
""");
    }

    public override async Task Where_enum_has_flag(bool async)
    {
        await base.Where_enum_has_flag(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Rank] & 2 = 2
""",
            //
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Rank] & 18 = 18
""",
            //
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Rank] & 1 = 1
""",
            //
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Rank] & 1 = 1
""",
            //
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE 2 & [g].[Rank] = [g].[Rank]
""");
    }

    public override async Task Correlated_collections_naked_navigation_with_ToList(bool async)
    {
        await base.Correlated_collections_naked_navigation_with_ToList(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] = [w].[OwnerFullName]
WHERE [g].[Nickname] <> N'Marcus'
ORDER BY [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Collection_with_inheritance_and_join_include_source(bool async)
    {
        await base.Collection_with_inheritance_and_join_include_source(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [t0].[Id], [t0].[GearNickName], [t0].[GearSquadId], [t0].[IssueDate], [t0].[Note], [t0].[PeriodEnd], [t0].[PeriodStart]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[SquadId] = [t].[GearSquadId] AND [g].[Nickname] = [t].[GearNickName]
LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t0] ON [g].[Nickname] = [t0].[GearNickName] AND [g].[SquadId] = [t0].[GearSquadId]
WHERE [g].[Discriminator] = N'Officer'
""");
    }

    public override async Task Complex_GroupBy_after_set_operator(bool async)
    {
        await base.Complex_GroupBy_after_set_operator(async);

        AssertSql(
"""
SELECT [t].[Name], [t].[Count], COALESCE(SUM([t].[Count]), 0) AS [Sum]
FROM (
    SELECT [c].[Name], (
        SELECT COUNT(*)
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]) AS [Count]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    LEFT JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g].[AssignedCityName] = [c].[Name]
    UNION ALL
    SELECT [c0].[Name], (
        SELECT COUNT(*)
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0]
        WHERE [g0].[FullName] = [w0].[OwnerFullName]) AS [Count]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    INNER JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c0] ON [g0].[CityOfBirthName] = [c0].[Name]
) AS [t]
GROUP BY [t].[Name], [t].[Count]
""");
    }

    public override async Task Projecting_property_converted_to_nullable_into_member_assignment(bool async)
    {
        await base.Projecting_property_converted_to_nullable_into_member_assignment(async);

        AssertSql(
"""
SELECT CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[SquadId]
    ELSE NULL
END AS [Id]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[Nickname]
    ELSE NULL
END IS NOT NULL
ORDER BY [t].[Note]
""");
    }

    public override async Task Where_enum_has_flag_subquery(bool async)
    {
        await base.Where_enum_has_flag_subquery(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Rank] & COALESCE((
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId]), 0) = COALESCE((
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId]), 0)
""",
            //
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE 2 & COALESCE((
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId]), 0) = COALESCE((
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId]), 0)
""");
    }

    public override async Task Select_required_navigation_on_the_same_type_with_cast(bool async)
    {
        await base.Select_required_navigation_on_the_same_type_with_cast(async);

        AssertSql(
"""
SELECT [c].[Name]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g].[CityOfBirthName] = [c].[Name]
""");
    }

    public override async Task Query_with_complex_let_containing_ordering_and_filter_projecting_firstOrDefault_element_of_let(bool async)
    {
        await base.Query_with_complex_let_containing_ordering_and_filter_projecting_firstOrDefault_element_of_let(async);

        AssertSql(
"""
SELECT [g].[Nickname], (
    SELECT TOP(1) [w].[Name]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName] AND [w].[IsAutomatic] = CAST(1 AS bit)
    ORDER BY [w].[AmmunitionType] DESC) AS [WeaponName]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Nickname] <> N'Dom'
""");
    }

    public override async Task Select_subquery_projecting_single_constant_bool(bool async)
    {
        await base.Select_subquery_projecting_single_constant_bool(async);

        AssertSql(
"""
SELECT [s].[Name], COALESCE((
    SELECT TOP(1) CAST(1 AS bit)
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [s].[Id] = [g].[SquadId] AND [g].[HasSoulPatch] = CAST(1 AS bit)), CAST(0 AS bit)) AS [Gear]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
""");
    }

    public override async Task Null_propagation_optimization4(bool async)
    {
        await base.Null_propagation_optimization4(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE CASE
    WHEN [g].[LeaderNickname] IS NULL THEN NULL
    ELSE CAST(LEN([g].[LeaderNickname]) AS int)
END = 5 AND CASE
    WHEN [g].[LeaderNickname] IS NULL THEN NULL
    ELSE CAST(LEN([g].[LeaderNickname]) AS int)
END IS NOT NULL
""");
    }

    public override async Task Correlated_collection_with_distinct_projecting_identifier_column(bool async)
    {
        await base.Correlated_collection_with_distinct_projecting_identifier_column(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [t].[Id], [t].[Name]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
OUTER APPLY (
    SELECT DISTINCT [w].[Id], [w].[Name]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
) AS [t]
ORDER BY [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Optional_navigation_with_collection_composite_key(bool async)
    {
        await base.Optional_navigation_with_collection_composite_key(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE [g].[Discriminator] = N'Officer' AND (
    SELECT COUNT(*)
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    WHERE [g].[Nickname] IS NOT NULL AND [g].[SquadId] IS NOT NULL AND [g].[Nickname] = [g0].[LeaderNickname] AND [g].[SquadId] = [g0].[LeaderSquadId] AND [g0].[Nickname] = N'Dom') > 0
""");
    }

    public override async Task Select_as_operator(bool async)
    {
        await base.Select_as_operator(async);

        AssertSql(
"""
SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
""");
    }

    public override async Task Include_reference_on_derived_type_using_lambda_with_tracking(bool async)
    {
        await base.Include_reference_on_derived_type_using_lambda_with_tracking(async);

        AssertSql(
"""
SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [l].[DefeatedByNickname] = [g].[Nickname] AND [l].[DefeatedBySquadId] = [g].[SquadId]
""");
    }

    public override async Task DateTimeOffset_DateAdd_AddSeconds(bool async)
    {
        await base.DateTimeOffset_DateAdd_AddSeconds(async);

        AssertSql(
"""
SELECT DATEADD(second, CAST(1.0E0 AS int), [m].[Timeline])
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
""");
    }

    public override async Task CompareTo_used_with_non_unicode_string_column_and_constant(bool async)
    {
        await base.CompareTo_used_with_non_unicode_string_column_and_constant(async);

        AssertSql(
"""
SELECT [c].[Name], [c].[Location], [c].[Nation], [c].[PeriodEnd], [c].[PeriodStart]
FROM [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c]
WHERE [c].[Location] = 'Unknown'
""");
    }

    public override async Task Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected(bool async)
    {
        await base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected(async);

        AssertSql(
"""
SELECT [t].[Id] AS [Id1], [t0].[Id] AS [Id2]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
CROSS JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t0]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [t0].[GearNickName] = [g0].[Nickname] AND [t0].[GearSquadId] = [g0].[SquadId]
WHERE [g].[Nickname] = [g0].[Nickname] OR ([g].[Nickname] IS NULL AND [g0].[Nickname] IS NULL)
""");
    }

    public override async Task Where_enum_has_flag_subquery_with_pushdown(bool async)
    {
        await base.Where_enum_has_flag_subquery_with_pushdown(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Rank] & (
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId]) = (
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId]) OR (
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId]) IS NULL
""",
            //
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE 2 & (
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId]) = (
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId]) OR (
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId]) IS NULL
""");
    }

    public override async Task Select_null_parameter_is_not_null(bool async)
    {
        await base.Select_null_parameter_is_not_null(async);

        AssertSql(
"""
@__p_0='False'

SELECT @__p_0
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Select_null_propagation_optimization7(bool async)
    {
        await base.Select_null_propagation_optimization7(async);

        AssertSql(
"""
SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN [g].[LeaderNickname] + [g].[LeaderNickname]
    ELSE NULL
END
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result4(bool async)
    {
        await base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result4(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank], [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[PeriodEnd], [w0].[PeriodStart], [w0].[SynergyWithId], [w1].[Id], [w1].[AmmunitionType], [w1].[IsAutomatic], [w1].[Name], [w1].[OwnerFullName], [w1].[PeriodEnd], [w1].[PeriodStart], [w1].[SynergyWithId], [w2].[Id], [w2].[AmmunitionType], [w2].[IsAutomatic], [w2].[Name], [w2].[OwnerFullName], [w2].[PeriodEnd], [w2].[PeriodStart], [w2].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[LeaderNickname] = [g0].[Nickname]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] = [w].[OwnerFullName]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0] ON [g0].[FullName] = [w0].[OwnerFullName]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w1] ON [g0].[FullName] = [w1].[OwnerFullName]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w2] ON [g].[FullName] = [w2].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId], [w].[Id], [w0].[Id], [w1].[Id]
""");
    }

    public override async Task Coalesce_operator_in_predicate_with_other_conditions(bool async)
    {
        await base.Coalesce_operator_in_predicate_with_other_conditions(async);

        AssertSql(
"""
SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE [w].[AmmunitionType] = 1 AND COALESCE([w].[IsAutomatic], CAST(0 AS bit)) = CAST(1 AS bit)
""");
    }

    public override async Task Where_subquery_union_firstordefault_boolean(bool async)
    {
        await base.Where_subquery_union_firstordefault_boolean(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit) AND (
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
        UNION
        SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[PeriodEnd], [w0].[PeriodStart], [w0].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0]
        WHERE [g].[FullName] = [w0].[OwnerFullName]
    ) AS [t]
    ORDER BY [t].[Id]) = CAST(1 AS bit)
""");
    }

    public override async Task Contains_on_byte_array_property_using_byte_column(bool async)
    {
        await base.Contains_on_byte_array_property_using_byte_column(async);

        AssertSql(
"""
SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart], [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
CROSS JOIN [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
WHERE CHARINDEX(CAST([l].[ThreatLevelByte] AS varbinary(max)), [s].[Banner]) > 0
""");
    }

    public override async Task Project_shadow_properties(bool async)
    {
        await base.Project_shadow_properties(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[AssignedCityName]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Where_nullable_enum_with_constant(bool async)
    {
        await base.Where_nullable_enum_with_constant(async);

        AssertSql(
"""
SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE [w].[AmmunitionType] = 1
""");
    }

    public override async Task Projecting_property_converted_to_nullable_and_use_it_in_order_by(bool async)
    {
        await base.Projecting_property_converted_to_nullable_and_use_it_in_order_by(async);

        AssertSql(
"""
SELECT [t].[Note], CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [g].[Nickname], [g].[SquadId], [g].[HasSoulPatch]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[Nickname]
    ELSE NULL
END IS NOT NULL
ORDER BY CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[SquadId]
    ELSE NULL
END, [t].[Note]
""");
    }

    public override async Task Join_on_entity_qsre_keys_inheritance(bool async)
    {
        await base.Join_on_entity_qsre_keys_inheritance(async);

        AssertSql(
"""
SELECT [g].[FullName] AS [GearName], [t].[FullName] AS [OfficerName]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[FullName]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    WHERE [g0].[Discriminator] = N'Officer'
) AS [t] ON [g].[Nickname] = [t].[Nickname] AND [g].[SquadId] = [t].[SquadId]
""");
    }

    public override async Task Select_Where_Navigation_Null(bool async)
    {
        await base.Select_Where_Navigation_Null(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE [g].[Nickname] IS NULL OR [g].[SquadId] IS NULL
""");
    }

    public override async Task
        Correlated_collection_with_groupby_not_projecting_identifier_column_but_only_grouping_key_in_final_projection(bool async)
    {
        await base.Correlated_collection_with_groupby_not_projecting_identifier_column_but_only_grouping_key_in_final_projection(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [t].[Key]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
OUTER APPLY (
    SELECT [w].[IsAutomatic] AS [Key]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    GROUP BY [w].[IsAutomatic]
) AS [t]
ORDER BY [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Join_predicate_condition_equals_condition(bool async)
    {
        await base.Join_predicate_condition_equals_condition(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [w].[SynergyWithId] IS NOT NULL
""");
    }

    public override async Task Select_correlated_filtered_collection_works_with_caching(bool async)
    {
        await base.Select_correlated_filtered_collection_works_with_caching(async);

        AssertSql(
"""
SELECT [t].[Id], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname]
ORDER BY [t].[Note], [t].[Id], [g].[Nickname]
""");
    }

    public override async Task Join_on_entity_qsre_keys_inner_key_is_nested_navigation(bool async)
    {
        await base.Join_on_entity_qsre_keys_inner_key_is_nested_navigation(async);

        AssertSql(
"""
SELECT [s].[Name] AS [SquadName], [t].[Name] AS [WeaponName]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
INNER JOIN (
    SELECT [w].[Name], [s0].[Id] AS [Id0]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [w].[OwnerFullName] = [g].[FullName]
    LEFT JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s0] ON [g].[SquadId] = [s0].[Id]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit)
) AS [t] ON [s].[Id] = [t].[Id0]
""");
    }

    public override async Task Correlated_collections_similar_collection_projected_multiple_times(bool async)
    {
        await base.Correlated_collections_similar_collection_projected_multiple_times(async);

        AssertSql(
"""
SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[PeriodEnd], [t].[PeriodStart], [t].[SynergyWithId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit)
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
LEFT JOIN (
    SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[PeriodEnd], [w0].[PeriodStart], [w0].[SynergyWithId]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0]
    WHERE [w0].[IsAutomatic] = CAST(0 AS bit)
) AS [t0] ON [g].[FullName] = [t0].[OwnerFullName]
ORDER BY [g].[Rank], [g].[Nickname], [g].[SquadId], [t].[OwnerFullName], [t].[Id], [t0].[IsAutomatic]
""");
    }

    public override async Task Correlated_collections_complex_scenario1(bool async)
    {
        await base.Correlated_collections_complex_scenario1(async);

        AssertSql(
"""
SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t].[Id], [t].[Nickname], [t].[SquadId], [t].[Id0], [t].[Nickname0], [t].[HasSoulPatch], [t].[SquadId0]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [w].[Id], [g0].[Nickname], [g0].[SquadId], [s].[Id] AS [Id0], [g1].[Nickname] AS [Nickname0], [g1].[HasSoulPatch], [g1].[SquadId] AS [SquadId0], [w].[OwnerFullName]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [w].[OwnerFullName] = [g0].[FullName]
    LEFT JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [g0].[SquadId] = [s].[Id]
    LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g1] ON [s].[Id] = [g1].[SquadId]
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Id], [t].[Nickname], [t].[SquadId], [t].[Id0], [t].[Nickname0]
""");
    }

    public override async Task TimeSpan_Minutes(bool async)
    {
        await base.TimeSpan_Minutes(async);

        AssertSql(
"""
SELECT DATEPART(minute, [m].[Duration])
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
""");
    }

    public override async Task Select_subquery_distinct_singleordefault_boolean1(bool async)
    {
        await base.Select_subquery_distinct_singleordefault_boolean1(async);

        AssertSql(
"""
SELECT COALESCE((
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName] AND [w].[Name] LIKE N'%Lancer%'
    ) AS [t]), CAST(0 AS bit))
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
""");
    }

    public override async Task Comparing_entities_using_Equals_inheritance(bool async)
    {
        await base.Comparing_entities_using_Equals_inheritance(async);

        AssertSql(
"""
SELECT [g].[Nickname] AS [Nickname1], [t].[Nickname] AS [Nickname2]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
CROSS JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    WHERE [g0].[Discriminator] = N'Officer'
) AS [t]
WHERE [g].[Nickname] = [t].[Nickname] AND [g].[SquadId] = [t].[SquadId]
ORDER BY [g].[Nickname], [t].[Nickname]
""");
    }

    public override async Task Where_compare_anonymous_types_with_uncorrelated_members(bool async)
    {
        await base.Where_compare_anonymous_types_with_uncorrelated_members(async);

        AssertSql(
"""
SELECT [g].[Nickname]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE 0 = 1
""");
    }

    public override async Task Order_by_is_properly_lifted_from_subquery_with_same_order_by_in_the_outer_query(bool async)
    {
        await base.Order_by_is_properly_lifted_from_subquery_with_same_order_by_in_the_outer_query(async);

        AssertSql(
"""
SELECT [g].[FullName]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(0 AS bit)
ORDER BY [g].[FullName]
""");
    }

    public override async Task Select_null_propagation_negative8(bool async)
    {
        await base.Select_null_propagation_negative8(async);

        AssertSql(
"""
SELECT CASE
    WHEN [s].[Id] IS NOT NULL THEN [c].[Name]
    ELSE NULL
END
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
LEFT JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [g].[SquadId] = [s].[Id]
LEFT JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g].[AssignedCityName] = [c].[Name]
""");
    }

    public override async Task Skip_with_orderby_followed_by_orderBy_is_pushed_down(bool async)
    {
        await base.Skip_with_orderby_followed_by_orderBy_is_pushed_down(async);

        AssertSql(
"""
@__p_0='1'

SELECT [t].[FullName]
FROM (
    SELECT [g].[FullName], [g].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [g].[HasSoulPatch] = CAST(0 AS bit)
    ORDER BY [g].[FullName]
    OFFSET @__p_0 ROWS
) AS [t]
ORDER BY [t].[Rank]
""");
    }

    public override async Task Where_nullable_enum_with_null_constant(bool async)
    {
        await base.Where_nullable_enum_with_null_constant(async);

        AssertSql(
"""
SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE [w].[AmmunitionType] IS NULL
""");
    }

    public override async Task ThenInclude_collection_on_derived_after_derived_reference(bool async)
    {
        await base.ThenInclude_collection_on_derived_after_derived_reference(async);

        AssertSql(
"""
SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[PeriodEnd], [f].[PeriodStart], [f].[ServerAddress], [f].[CommanderName], [f].[Eradicated], [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[PeriodEnd], [t].[PeriodStart], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
LEFT JOIN (
    SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[DefeatedByNickname] = [g].[Nickname] AND [t].[DefeatedBySquadId] = [g].[SquadId]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON ([g].[Nickname] = [g0].[LeaderNickname] OR ([g].[Nickname] IS NULL AND [g0].[LeaderNickname] IS NULL)) AND [g].[SquadId] = [g0].[LeaderSquadId]
ORDER BY [f].[Id], [t].[Name], [g].[Nickname], [g].[SquadId], [g0].[Nickname]
""");
    }

    public override async Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery_duplicated_orderings(
        bool async)
    {
        await base.Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery_duplicated_orderings(async);

        AssertSql(
"""
SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t].[Id], [g1].[Nickname], [g1].[SquadId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[SynergyWithId], [t0].[Nickname], [t0].[SquadId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[Nickname] = [t].[GearNickName] AND [g].[SquadId] = [t].[GearSquadId]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g1] ON [t].[GearNickName] = [g1].[Nickname] AND [t].[GearSquadId] = [g1].[SquadId]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId], [g2].[Nickname], [g2].[SquadId]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g2] ON [w].[OwnerFullName] = [g2].[FullName]
) AS [t0] ON [g1].[FullName] = [t0].[OwnerFullName]
WHERE [g].[Discriminator] = N'Officer' AND EXISTS (
    SELECT 1
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    WHERE [g].[Nickname] = [g0].[LeaderNickname] AND [g].[SquadId] = [g0].[LeaderSquadId])
ORDER BY [g].[HasSoulPatch] DESC, [t].[Note], [g].[Nickname], [g].[SquadId], [t].[Id], [g1].[Nickname], [g1].[SquadId], [t0].[IsAutomatic], [t0].[Nickname] DESC, [t0].[Id]
""");
    }

    public override async Task SelectMany_without_result_selector_and_non_equality_comparison_converted_to_join(bool async)
    {
        await base.SelectMany_without_result_selector_and_non_equality_comparison_converted_to_join(async);

        AssertSql(
"""
SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] <> [w].[OwnerFullName] OR [w].[OwnerFullName] IS NULL
""");
    }

    public override async Task Order_by_entity_qsre_with_other_orderbys(bool async)
    {
        await base.Order_by_entity_qsre_with_other_orderbys(async);

        AssertSql(
"""
SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [w].[OwnerFullName] = [g].[FullName]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY [w].[IsAutomatic], [g].[Nickname] DESC, [g].[SquadId] DESC, [w0].[Id], [w].[Name]
""");
    }

    public override async Task Select_subquery_projecting_multiple_constants_inside_anonymous(bool async)
    {
        await base.Select_subquery_projecting_multiple_constants_inside_anonymous(async);

        AssertSql(
"""
SELECT [s].[Name], [t0].[True1], [t0].[False1], [t0].[c]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
LEFT JOIN (
    SELECT [t].[True1], [t].[False1], [t].[c], [t].[SquadId]
    FROM (
        SELECT CAST(1 AS bit) AS [True1], CAST(0 AS bit) AS [False1], 1 AS [c], [g].[SquadId], ROW_NUMBER() OVER(PARTITION BY [g].[SquadId] ORDER BY [g].[Nickname], [g].[SquadId]) AS [row]
        FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
        WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [s].[Id] = [t0].[SquadId]
""");
    }

    public override async Task FirstOrDefault_navigation_access_entity_equality_in_where_predicate_apply_peneding_selector(bool async)
    {
        await base.FirstOrDefault_navigation_access_entity_equality_in_where_predicate_apply_peneding_selector(async);

        AssertSql(
"""
SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[PeriodEnd], [f].[PeriodStart], [f].[ServerAddress], [f].[CommanderName], [f].[Eradicated]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
LEFT JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [f].[CapitalName] = [c].[Name]
WHERE [c].[Name] = (
    SELECT TOP(1) [c0].[Name]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    INNER JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c0] ON [g].[CityOfBirthName] = [c0].[Name]
    ORDER BY [g].[Nickname]) OR ([c].[Name] IS NULL AND (
    SELECT TOP(1) [c0].[Name]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    INNER JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c0] ON [g].[CityOfBirthName] = [c0].[Name]
    ORDER BY [g].[Nickname]) IS NULL)
""");
    }

    public override async Task SelectMany_predicate_with_non_equality_comparison_DefaultIfEmpty_converted_to_left_join(bool async)
    {
        await base.SelectMany_predicate_with_non_equality_comparison_DefaultIfEmpty_converted_to_left_join(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] <> [w].[OwnerFullName] OR [w].[OwnerFullName] IS NULL
ORDER BY [g].[Nickname], [w].[Id]
""");
    }

    public override async Task Correlated_collections_basic_projection(bool async)
    {
        await base.Correlated_collections_basic_projection(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[PeriodEnd], [t].[PeriodStart], [t].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit) OR [w].[Name] <> N'foo' OR [w].[Name] IS NULL
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
WHERE [g].[Nickname] <> N'Marcus'
ORDER BY [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Where_bitwise_and_nullable_enum_with_null_constant(bool async)
    {
        await base.Where_bitwise_and_nullable_enum_with_null_constant(async);

        AssertSql(
"""
SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE [w].[AmmunitionType] & NULL > 0
""");
    }

    public override async Task Join_with_order_by_on_inner_sequence_navigation_translated_to_subquery_composite_key(bool async)
    {
        await base.Join_with_order_by_on_inner_sequence_navigation_translated_to_subquery_composite_key(async);

        AssertSql(
"""
SELECT [g].[FullName], [t0].[Note]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN (
    SELECT [t].[Note], [g0].[FullName]
    FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
    LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [t].[GearNickName] = [g0].[Nickname] AND [t].[GearSquadId] = [g0].[SquadId]
) AS [t0] ON [g].[FullName] = [t0].[FullName]
""");
    }

    public override async Task Non_unicode_string_literals_is_used_for_non_unicode_column_with_concat(bool async)
    {
        await base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_concat(async);

        AssertSql(
"""
SELECT [c].[Name], [c].[Location], [c].[Nation], [c].[PeriodEnd], [c].[PeriodStart]
FROM [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c]
WHERE COALESCE([c].[Location], '') + 'Added' LIKE '%Add%'
""");
    }

    public override async Task Enum_array_contains(bool async)
    {
        await base.Enum_array_contains(async);

        AssertSql(
"""
@__types_0='[null,1]' (Size = 4000)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
WHERE [w0].[Id] IS NOT NULL AND EXISTS (
    SELECT 1
    FROM OpenJson(@__types_0) AS [t]
    WHERE CAST([t].[value] AS int) = [w0].[AmmunitionType] OR ([t].[value] IS NULL AND [w0].[AmmunitionType] IS NULL))
""");
    }

    public override async Task Sum_with_optional_navigation_is_translated_to_sql(bool async)
    {
        await base.Sum_with_optional_navigation_is_translated_to_sql(async);

        AssertSql(
"""
SELECT COALESCE(SUM([g].[SquadId]), 0)
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[Nickname] = [t].[GearNickName] AND [g].[SquadId] = [t].[GearSquadId]
WHERE [t].[Note] <> N'Foo' OR [t].[Note] IS NULL
""");
    }

    public override async Task Where_TimeSpan_Milliseconds(bool async)
    {
        await base.Where_TimeSpan_Milliseconds(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE DATEPART(millisecond, [m].[Duration]) = 1
""");
    }

    public override async Task Select_subquery_distinct_singleordefault_boolean_empty_with_pushdown(bool async)
    {
        await base.Select_subquery_distinct_singleordefault_boolean_empty_with_pushdown(async);

        AssertSql(
"""
SELECT (
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName] AND [w].[Name] = N'BFG'
    ) AS [t])
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
""");
    }

    public override async Task Select_required_navigation_on_derived_type(bool async)
    {
        await base.Select_required_navigation_on_derived_type(async);

        AssertSql(
"""
SELECT [l0].[Name]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN [LocustHighCommands] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0] ON [l].[HighCommandId] = [l0].[Id]
""");
    }

    public override async Task Select_subquery_boolean_with_pushdown(bool async)
    {
        await base.Select_subquery_boolean_with_pushdown(async);

        AssertSql(
"""
SELECT (
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id])
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Multiple_derived_included_on_one_method(bool async)
    {
        await base.Multiple_derived_included_on_one_method(async);

        AssertSql(
"""
SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[PeriodEnd], [f].[PeriodStart], [f].[ServerAddress], [f].[CommanderName], [f].[Eradicated], [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[PeriodEnd], [t].[PeriodStart], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
LEFT JOIN (
    SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[DefeatedByNickname] = [g].[Nickname] AND [t].[DefeatedBySquadId] = [g].[SquadId]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON ([g].[Nickname] = [g0].[LeaderNickname] OR ([g].[Nickname] IS NULL AND [g0].[LeaderNickname] IS NULL)) AND [g].[SquadId] = [g0].[LeaderSquadId]
ORDER BY [f].[Id], [t].[Name], [g].[Nickname], [g].[SquadId], [g0].[Nickname]
""");
    }

    public override async Task Select_null_propagation_works_for_multiple_navigations_with_composite_keys(bool async)
    {
        await base.Select_null_propagation_works_for_multiple_navigations_with_composite_keys(async);

        AssertSql(
"""
SELECT CASE
    WHEN [c].[Name] IS NOT NULL THEN [c].[Name]
    ELSE NULL
END
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t0] ON ([g].[Nickname] = [t0].[GearNickName] OR ([g].[Nickname] IS NULL AND [t0].[GearNickName] IS NULL)) AND ([g].[SquadId] = [t0].[GearSquadId] OR ([g].[SquadId] IS NULL AND [t0].[GearSquadId] IS NULL))
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [t0].[GearNickName] = [g0].[Nickname] AND [t0].[GearSquadId] = [g0].[SquadId]
LEFT JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g0].[AssignedCityName] = [c].[Name]
""");
    }

    public override async Task Byte_array_filter_by_SequenceEqual(bool async)
    {
        await base.Byte_array_filter_by_SequenceEqual(async);

        AssertSql(
"""
@__byteArrayParam_0='0x0405060708' (Size = 5)

SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
WHERE [s].[Banner5] = @__byteArrayParam_0
""");
    }

    public override async Task Include_on_derived_entity_with_cast(bool async)
    {
        await base.Include_on_derived_entity_with_cast(async);

        AssertSql(
"""
SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[PeriodEnd], [f].[PeriodStart], [f].[ServerAddress], [f].[CommanderName], [f].[Eradicated], [c].[Name], [c].[Location], [c].[Nation], [c].[PeriodEnd], [c].[PeriodStart]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
LEFT JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [f].[CapitalName] = [c].[Name]
ORDER BY [f].[Id]
""");
    }

    public override async Task Include_collection_OrderBy_aggregate(bool async)
    {
        await base.Include_collection_OrderBy_aggregate(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[Nickname] = [g0].[LeaderNickname] AND [g].[SquadId] = [g0].[LeaderSquadId]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY (
    SELECT COUNT(*)
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]), [g].[Nickname], [g].[SquadId], [g0].[Nickname]
""");
    }

    public override async Task Projecting_property_converted_to_nullable_into_member_access(bool async)
    {
        await base.Projecting_property_converted_to_nullable_into_member_access(async);

        AssertSql(
"""
SELECT [g].[Nickname]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[Nickname] = [t].[GearNickName] AND [g].[SquadId] = [t].[GearSquadId]
WHERE DATEPART(month, [t].[IssueDate]) <> 5 OR [t].[IssueDate] IS NULL
ORDER BY [g].[Nickname]
""");
    }

    public override async Task Correlated_collection_with_top_level_Last_with_order_by_on_inner(bool async)
    {
        await base.Correlated_collection_with_top_level_Last_with_order_by_on_inner(async);

        AssertSql(
"""
SELECT [t].[Nickname], [t].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM (
    SELECT TOP(1) [g].[Nickname], [g].[SquadId], [g].[FullName]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    ORDER BY [g].[FullName] DESC
) AS [t]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [t].[FullName] = [w].[OwnerFullName]
ORDER BY [t].[FullName] DESC, [t].[Nickname], [t].[SquadId], [w].[Name]
""");
    }

    public override async Task Non_unicode_parameter_is_used_for_non_unicode_column(bool async)
    {
        await base.Non_unicode_parameter_is_used_for_non_unicode_column(async);

        AssertSql(
"""
@__value_0='Unknown' (Size = 100) (DbType = AnsiString)

SELECT [c].[Name], [c].[Location], [c].[Nation], [c].[PeriodEnd], [c].[PeriodStart]
FROM [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c]
WHERE [c].[Location] = @__value_0
""");
    }

    public override async Task Take_without_orderby_followed_by_orderBy_is_pushed_down3(bool async)
    {
        await base.Take_without_orderby_followed_by_orderBy_is_pushed_down3(async);

        AssertSql(
"""
@__p_0='999'

SELECT [t].[FullName]
FROM (
    SELECT TOP(@__p_0) [g].[FullName], [g].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [g].[HasSoulPatch] = CAST(0 AS bit)
) AS [t]
ORDER BY [t].[FullName], [t].[Rank]
""");
    }

    public override async Task Distinct_on_subquery_doesnt_get_lifted(bool async)
    {
        await base.Distinct_on_subquery_doesnt_get_lifted(async);

        AssertSql(
"""
SELECT [t].[HasSoulPatch]
FROM (
    SELECT DISTINCT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
) AS [t]
""");
    }

    public override async Task GetValueOrDefault_in_filter(bool async)
    {
        await base.GetValueOrDefault_in_filter(async);

        AssertSql(
"""
SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE COALESCE([w].[SynergyWithId], 0) = 0
""");
    }

    public override async Task Count_with_optional_navigation_is_translated_to_sql(bool async)
    {
        await base.Count_with_optional_navigation_is_translated_to_sql(async);

        AssertSql(
"""
SELECT COUNT(*)
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[Nickname] = [t].[GearNickName] AND [g].[SquadId] = [t].[GearSquadId]
WHERE [t].[Note] <> N'Foo' OR [t].[Note] IS NULL
""");
    }

    public override async Task Project_entity_and_collection_element(bool async)
    {
        await base.Project_entity_and_collection_element(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [g].[SquadId] = [s].[Id]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] = [w].[OwnerFullName]
LEFT JOIN (
    SELECT [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[PeriodEnd], [t].[PeriodStart], [t].[SynergyWithId]
    FROM (
        SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[PeriodEnd], [w0].[PeriodStart], [w0].[SynergyWithId], ROW_NUMBER() OVER(PARTITION BY [w0].[OwnerFullName] ORDER BY [w0].[Id]) AS [row]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0]
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [g].[FullName] = [t0].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [s].[Id]
""");
    }

    public override async Task Where_required_navigation_on_derived_type(bool async)
    {
        await base.Where_required_navigation_on_derived_type(async);

        AssertSql(
"""
SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN [LocustHighCommands] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0] ON [l].[HighCommandId] = [l0].[Id]
WHERE [l0].[IsOperational] = CAST(1 AS bit)
""");
    }

    public override async Task Composite_key_entity_not_equal_null(bool async)
    {
        await base.Composite_key_entity_not_equal_null(async);

        AssertSql(
"""
SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [l].[DefeatedByNickname] = [g].[Nickname] AND [l].[DefeatedBySquadId] = [g].[SquadId]
WHERE [l].[Discriminator] = N'LocustCommander' AND [g].[Nickname] IS NOT NULL AND [g].[SquadId] IS NOT NULL
""");
    }

    public override async Task Collection_with_inheritance_and_join_include_joined(bool async)
    {
        await base.Collection_with_inheritance_and_join_include_joined(async);

        AssertSql(
"""
SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Rank], [t1].[Id], [t1].[GearNickName], [t1].[GearSquadId], [t1].[IssueDate], [t1].[Note], [t1].[PeriodEnd], [t1].[PeriodStart]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
INNER JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [g].[Discriminator] = N'Officer'
) AS [t0] ON [t].[GearSquadId] = [t0].[SquadId] AND [t].[GearNickName] = [t0].[Nickname]
LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t1] ON [t0].[Nickname] = [t1].[GearNickName] AND [t0].[SquadId] = [t1].[GearSquadId]
""");
    }

    public override async Task Filtered_collection_projection_with_order_comparison_predicate_converted_to_join(bool async)
    {
        await base.Filtered_collection_projection_with_order_comparison_predicate_converted_to_join(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] = [w].[OwnerFullName] AND [g].[SquadId] < [w].[Id]
ORDER BY [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Where_subquery_distinct_singleordefault_boolean1(bool async)
    {
        await base.Where_subquery_distinct_singleordefault_boolean1(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit) AND COALESCE((
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName] AND [w].[Name] LIKE N'%Lancer%'
    ) AS [t]), CAST(0 AS bit)) = CAST(1 AS bit)
ORDER BY [g].[Nickname]
""");
    }

    public override async Task Select_null_propagation_optimization9(bool async)
    {
        await base.Select_null_propagation_optimization9(async);

        AssertSql(
"""
SELECT CAST(LEN([g].[FullName]) AS int)
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Select_StartsWith_with_null_parameter_as_argument(bool async)
    {
        await base.Select_StartsWith_with_null_parameter_as_argument(async);

        AssertSql(
"""
SELECT CAST(0 AS bit)
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Enum_flags_closure_typed_as_different_type_generates_correct_parameter_type(bool async)
    {
        await base.Enum_flags_closure_typed_as_different_type_generates_correct_parameter_type(async);

        AssertSql(
"""
@__prm_0='5'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE @__prm_0 & CAST([g].[Rank] AS int) = CAST([g].[Rank] AS int)
""");
    }

    public override async Task Include_navigation_on_derived_type(bool async)
    {
        await base.Include_navigation_on_derived_type(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[Nickname] = [g0].[LeaderNickname] AND [g].[SquadId] = [g0].[LeaderSquadId]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [g0].[Nickname]
""");
    }

    public override async Task Comparing_two_collection_navigations_inheritance(bool async)
    {
        await base.Comparing_two_collection_navigations_inheritance(async);

        AssertSql(
"""
SELECT [f].[Name], [t].[Nickname]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
CROSS JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[HasSoulPatch]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [g].[Discriminator] = N'Officer'
) AS [t]
LEFT JOIN (
    SELECT [l].[Name], [l].[DefeatedByNickname], [l].[DefeatedBySquadId]
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t0] ON [f].[CommanderName] = [t0].[Name]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [t0].[DefeatedByNickname] = [g0].[Nickname] AND [t0].[DefeatedBySquadId] = [g0].[SquadId]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit) AND [g0].[Nickname] = [t].[Nickname] AND [g0].[SquadId] = [t].[SquadId]
""");
    }

    public override async Task Include_collection_on_derived_type_using_lambda(bool async)
    {
        await base.Include_collection_on_derived_type_using_lambda(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[Nickname] = [g0].[LeaderNickname] AND [g].[SquadId] = [g0].[LeaderSquadId]
ORDER BY [g].[Nickname], [g].[SquadId], [g0].[Nickname]
""");
    }

    public override async Task Order_by_entity_qsre(bool async)
    {
        await base.Order_by_entity_qsre(async);

        AssertSql(
"""
SELECT [g].[FullName]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g].[AssignedCityName] = [c].[Name]
ORDER BY [c].[Name], [g].[Nickname] DESC
""");
    }

    public override async Task OfTypeNav3(bool async)
    {
        await base.OfTypeNav3(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[Nickname] = [t].[GearNickName] AND [g].[SquadId] = [t].[GearSquadId]
INNER JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] = [w].[OwnerFullName]
LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t0] ON [g].[Nickname] = [t0].[GearNickName] AND [g].[SquadId] = [t0].[GearSquadId]
WHERE ([t].[Note] <> N'Foo' OR [t].[Note] IS NULL) AND [g].[Discriminator] = N'Officer' AND ([t0].[Note] <> N'Bar' OR [t0].[Note] IS NULL)
""");
    }

    public override async Task Streaming_correlated_collection_issue_11403(bool async)
    {
        await base.Streaming_correlated_collection_issue_11403(async);

        AssertSql(
"""
SELECT [t].[Nickname], [t].[SquadId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[SynergyWithId]
FROM (
    SELECT TOP(1) [g].[Nickname], [g].[SquadId], [g].[FullName]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    ORDER BY [g].[Nickname]
) AS [t]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [w].[IsAutomatic] = CAST(0 AS bit)
) AS [t0] ON [t].[FullName] = [t0].[OwnerFullName]
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Id]
""");
    }

    public override async Task Correlated_collections_nested_mixed_streaming_with_buffer1(bool async)
    {
        await base.Correlated_collections_nested_mixed_streaming_with_buffer1(async);

        AssertSql(
"""
SELECT [s].[Id], [t0].[SquadId], [t0].[MissionId], [t0].[Id], [t0].[SquadId0], [t0].[MissionId0], [t0].[PeriodEnd], [t0].[PeriodStart]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
LEFT JOIN (
    SELECT [s0].[SquadId], [s0].[MissionId], [m].[Id], [t].[SquadId] AS [SquadId0], [t].[MissionId] AS [MissionId0], [t].[PeriodEnd], [t].[PeriodStart]
    FROM [SquadMissions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s0]
    INNER JOIN [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m] ON [s0].[MissionId] = [m].[Id]
    LEFT JOIN (
        SELECT [s1].[SquadId], [s1].[MissionId], [s1].[PeriodEnd], [s1].[PeriodStart]
        FROM [SquadMissions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s1]
        WHERE [s1].[SquadId] < 2
    ) AS [t] ON [m].[Id] = [t].[MissionId]
    WHERE [s0].[MissionId] < 3
) AS [t0] ON [s].[Id] = [t0].[SquadId]
ORDER BY [s].[Id], [t0].[SquadId], [t0].[MissionId], [t0].[Id], [t0].[SquadId0]
""");
    }

    public override async Task Time_of_day_datetimeoffset(bool async)
    {
        await base.Time_of_day_datetimeoffset(async);

        AssertSql(
"""
SELECT CONVERT(time, [m].[Timeline])
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
""");
    }

    public override async Task Join_predicate_value(bool async)
    {
        await base.Join_predicate_value(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[HasSoulPatch] = CAST(1 AS bit)
""");
    }

    public override async Task Where_conditional_equality_3(bool async)
    {
        await base.Where_conditional_equality_3(async);

        AssertSql(
"""
SELECT [g].[Nickname]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
ORDER BY [g].[Nickname]
""");
    }

    public override async Task Where_datetimeoffset_date_component(bool async)
    {
        await base.Where_datetimeoffset_date_component(async);

        AssertSql(
"""
@__Date_0='0001-01-01T00:00:00.0000000'

SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE CONVERT(date, [m].[Timeline]) > @__Date_0
""");
    }

    public override async Task OrderBy_Contains_empty_list(bool async)
    {
        await base.OrderBy_Contains_empty_list(async);

        AssertSql(
"""
@__ids_0='[]' (Size = 4000)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
ORDER BY CASE
    WHEN EXISTS (
        SELECT 1
        FROM OpenJson(@__ids_0) AS [i]
        WHERE CAST([i].[value] AS int) = [g].[SquadId]) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task FirstOrDefault_on_empty_collection_of_DateTime_in_subquery(bool async)
    {
        await base.FirstOrDefault_on_empty_collection_of_DateTime_in_subquery(async);

        AssertSql(
"""
SELECT [g].[Nickname], COALESCE((
    SELECT TOP(1) [t1].[IssueDate]
    FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t1]
    WHERE [t1].[GearNickName] = [g].[FullName]
    ORDER BY [t1].[Id]), '0001-01-01T00:00:00.0000000') AS [invalidTagIssueDate]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[Nickname] = [t].[GearNickName] AND [g].[SquadId] = [t].[GearSquadId]
WHERE [t].[IssueDate] > COALESCE((
    SELECT TOP(1) [t0].[IssueDate]
    FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t0]
    WHERE [t0].[GearNickName] = [g].[FullName]
    ORDER BY [t0].[Id]), '0001-01-01T00:00:00.0000000')
""");
    }

    public override async Task Coalesce_operator_in_predicate(bool async)
    {
        await base.Coalesce_operator_in_predicate(async);

        AssertSql(
"""
SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE COALESCE([w].[IsAutomatic], CAST(0 AS bit)) = CAST(1 AS bit)
""");
    }

    public override async Task Filter_with_new_Guid(bool async)
    {
        await base.Filter_with_new_Guid(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
WHERE [t].[Id] = 'df36f493-463f-4123-83f9-6b135deeb7ba'
""");
    }

    public override async Task Correlated_collections_nested_mixed_streaming_with_buffer2(bool async)
    {
        await base.Correlated_collections_nested_mixed_streaming_with_buffer2(async);

        AssertSql(
"""
SELECT [s].[Id], [t0].[SquadId], [t0].[MissionId], [t0].[Id], [t0].[SquadId0], [t0].[MissionId0], [t0].[PeriodEnd], [t0].[PeriodStart]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
LEFT JOIN (
    SELECT [s0].[SquadId], [s0].[MissionId], [m].[Id], [t].[SquadId] AS [SquadId0], [t].[MissionId] AS [MissionId0], [t].[PeriodEnd], [t].[PeriodStart]
    FROM [SquadMissions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s0]
    INNER JOIN [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m] ON [s0].[MissionId] = [m].[Id]
    LEFT JOIN (
        SELECT [s1].[SquadId], [s1].[MissionId], [s1].[PeriodEnd], [s1].[PeriodStart]
        FROM [SquadMissions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s1]
        WHERE [s1].[SquadId] < 7
    ) AS [t] ON [m].[Id] = [t].[MissionId]
    WHERE [s0].[MissionId] < 42
) AS [t0] ON [s].[Id] = [t0].[SquadId]
ORDER BY [s].[Id], [t0].[SquadId], [t0].[MissionId], [t0].[Id], [t0].[SquadId0]
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_array_initializers(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_array_initializers(async);

        AssertSql(
"""
SELECT [g].[SquadId]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE [t].[Note] <> N'K.I.A.' OR [t].[Note] IS NULL
""");
    }

    public override async Task Select_subquery_distinct_singleordefault_boolean_with_pushdown(bool async)
    {
        await base.Select_subquery_distinct_singleordefault_boolean_with_pushdown(async);

        AssertSql(
"""
SELECT (
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName] AND [w].[Name] LIKE N'%Lancer%'
    ) AS [t])
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
""");
    }

    public override async Task DateTimeOffset_Contains_Less_than_Greater_than(bool async)
    {
        await base.DateTimeOffset_Contains_Less_than_Greater_than(async);

        AssertSql(
"""
@__start_0='1902-01-01T10:00:00.1234567+01:30'
@__end_1='1902-01-03T10:00:00.1234567+01:30'
@__dates_2='["1902-01-02T10:00:00.1234567+01:30"]' (Size = 4000)

SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE @__start_0 <= CAST(CONVERT(date, [m].[Timeline]) AS datetimeoffset) AND [m].[Timeline] < @__end_1 AND EXISTS (
    SELECT 1
    FROM OpenJson(@__dates_2) AS [d]
    WHERE CAST([d].[value] AS datetimeoffset) = [m].[Timeline])
""");
    }

    public override Task DateTimeOffsetNow_minus_timespan(bool async)
        => AssertTranslationFailed(() => base.DateTimeOffsetNow_minus_timespan(async));

    public override async Task Conditional_with_conditions_evaluating_to_false_gets_optimized(bool async)
    {
        await base.Conditional_with_conditions_evaluating_to_false_gets_optimized(async);

        AssertSql(
"""
SELECT [g].[FullName]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Cast_ordered_subquery_to_base_type_using_typed_ToArray(bool async)
    {
        await base.Cast_ordered_subquery_to_base_type_using_typed_ToArray(async);

        AssertSql(
"""
SELECT [c].[Name], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Nickname], [g].[Rank], [g].[SquadId]
FROM [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [c].[Name] = [g].[AssignedCityName]
WHERE [c].[Name] = N'Ephyra'
ORDER BY [c].[Name], [g].[Nickname] DESC
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_predicate2(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_predicate2(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
""");
    }

    public override async Task Null_semantics_on_nullable_bool_from_left_join_subquery_is_fully_applied(bool async)
    {
        await base.Null_semantics_on_nullable_bool_from_left_join_subquery_is_fully_applied(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[CapitalName], [t].[Discriminator], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t].[ServerAddress], [t].[CommanderName], [t].[Eradicated]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[PeriodEnd], [f].[PeriodStart], [f].[ServerAddress], [f].[CommanderName], [f].[Eradicated]
    FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
    WHERE [f].[Name] = N'Swarm'
) AS [t] ON [l].[Name] = [t].[CommanderName]
WHERE [t].[Eradicated] <> CAST(1 AS bit) OR [t].[Eradicated] IS NULL
""");
    }

    public override async Task Bool_projection_from_subquery_treated_appropriately_in_where(bool async)
    {
        await base.Bool_projection_from_subquery_treated_appropriately_in_where(async);

        AssertSql(
"""
SELECT [c].[Name], [c].[Location], [c].[Nation], [c].[PeriodEnd], [c].[PeriodStart]
FROM [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c]
WHERE (
    SELECT TOP(1) [g].[HasSoulPatch]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    ORDER BY [g].[Nickname], [g].[SquadId]) = CAST(1 AS bit)
""");
    }

    public override async Task Select_correlated_filtered_collection_with_composite_key(bool async)
    {
        await base.Select_correlated_filtered_collection_with_composite_key(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    WHERE [g0].[Nickname] <> N'Dom'
) AS [t] ON [g].[Nickname] = [t].[LeaderNickname] AND [g].[SquadId] = [t].[LeaderSquadId]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Nickname]
""");
    }

    public override async Task Subquery_created_by_include_gets_lifted_nested(bool async)
    {
        await base.Subquery_created_by_include_gets_lifted_nested(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [c].[Name], [c].[Location], [c].[Nation], [c].[PeriodEnd], [c].[PeriodStart]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g].[CityOfBirthName] = [c].[Name]
WHERE EXISTS (
    SELECT 1
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]) AND [g].[HasSoulPatch] = CAST(0 AS bit)
ORDER BY [g].[Nickname]
""");
    }

    public override async Task Query_reusing_parameter_doesnt_declare_duplicate_parameter_complex(bool async)
    {
        await base.Query_reusing_parameter_doesnt_declare_duplicate_parameter_complex(async);

        AssertSql(
"""
@__entity_equality_prm_Inner_Squad_0_Id='1' (Nullable = true)

SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Rank]
FROM (
    SELECT DISTINCT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    INNER JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [g].[SquadId] = [s].[Id]
    WHERE [s].[Id] = @__entity_equality_prm_Inner_Squad_0_Id
) AS [t]
INNER JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s0] ON [t].[SquadId] = [s0].[Id]
WHERE [s0].[Id] = @__entity_equality_prm_Inner_Squad_0_Id
ORDER BY [t].[FullName]
""");
    }

    public override async Task Concat_with_count(bool async)
    {
        await base.Concat_with_count(async);

        AssertSql(
"""
SELECT COUNT(*)
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    UNION ALL
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
) AS [t]
""");
    }

    public override async Task Projecting_property_converted_to_nullable_with_function_call2(bool async)
    {
        await base.Projecting_property_converted_to_nullable_with_function_call2(async);

        AssertSql(
"""
SELECT [t].[Note], SUBSTRING([t].[Note], 0 + 1, CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[SquadId]
    ELSE NULL
END) AS [Function]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[Nickname]
    ELSE NULL
END IS NOT NULL
""");
    }

    public override async Task Double_order_by_binary_expression(bool async)
    {
        await base.Double_order_by_binary_expression(async);

        AssertSql(
"""
SELECT [w].[Id] + 2 AS [Binary]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
ORDER BY [w].[Id] + 2
""");
    }

    public override async Task Include_with_join_and_inheritance3(bool async)
    {
        await base.Include_with_join_and_inheritance3(async);

        AssertSql(
"""
SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Rank], [t].[Id], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
INNER JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [g].[Discriminator] = N'Officer'
) AS [t0] ON [t].[GearSquadId] = [t0].[SquadId] AND [t].[GearNickName] = [t0].[Nickname]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [t0].[Nickname] = [g0].[LeaderNickname] AND [t0].[SquadId] = [g0].[LeaderSquadId]
ORDER BY [t].[Id], [t0].[Nickname], [t0].[SquadId], [g0].[Nickname]
""");
    }

    public override async Task Filtered_collection_projection_with_order_comparison_predicate_converted_to_join2(bool async)
    {
        await base.Filtered_collection_projection_with_order_comparison_predicate_converted_to_join2(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] = [w].[OwnerFullName] AND [g].[SquadId] <= [w].[Id]
ORDER BY [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Where_subquery_distinct_singleordefault_boolean2(bool async)
    {
        await base.Where_subquery_distinct_singleordefault_boolean2(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit) AND COALESCE((
    SELECT DISTINCT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName] AND [w].[Name] LIKE N'%Lancer%'), CAST(0 AS bit)) = CAST(1 AS bit)
ORDER BY [g].[Nickname]
""");
    }

    public override async Task TimeSpan_Seconds(bool async)
    {
        await base.TimeSpan_Seconds(async);

        AssertSql(
"""
SELECT DATEPART(second, [m].[Duration])
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
""");
    }

    public override async Task Navigation_inside_interpolated_string_expanded(bool async)
    {
        await base.Navigation_inside_interpolated_string_expanded(async);

        AssertSql(
"""
SELECT CASE
    WHEN [w].[SynergyWithId] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [w0].[OwnerFullName]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
""");
    }

    public override async Task Include_reference_on_derived_type_using_string_nested1(bool async)
    {
        await base.Include_reference_on_derived_type_using_string_nested1(async);

        AssertSql(
"""
SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [l].[DefeatedByNickname] = [g].[Nickname] AND [l].[DefeatedBySquadId] = [g].[SquadId]
LEFT JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [g].[SquadId] = [s].[Id]
""");
    }

    public override async Task Navigation_based_on_complex_expression2(bool async)
    {
        await base.Navigation_based_on_complex_expression2(async);

        AssertSql(
"""
SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[PeriodEnd], [f].[PeriodStart], [f].[ServerAddress], [f].[CommanderName], [f].[Eradicated]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
LEFT JOIN (
    SELECT [l].[Name]
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
WHERE [t].[Name] IS NOT NULL
""");
    }

    public override async Task Where_datetimeoffset_year_component(bool async)
    {
        await base.Where_datetimeoffset_year_component(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE DATEPART(year, [m].[Timeline]) = 2
""");
    }

    public override async Task Select_ternary_operation_with_inverted_boolean(bool async)
    {
        await base.Select_ternary_operation_with_inverted_boolean(async);

        AssertSql(
"""
SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = CAST(0 AS bit) THEN 1
    ELSE 0
END AS [Num]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
""");
    }

    public override async Task Where_subquery_distinct_orderby_firstordefault_boolean(bool async)
    {
        await base.Where_subquery_distinct_orderby_firstordefault_boolean(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit) AND COALESCE((
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
    ) AS [t]
    ORDER BY [t].[Id]), CAST(0 AS bit)) = CAST(1 AS bit)
""");
    }

    public override async Task Composite_key_entity_equal_null(bool async)
    {
        await base.Composite_key_entity_equal_null(async);

        AssertSql(
"""
SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [l].[DefeatedByNickname] = [g].[Nickname] AND [l].[DefeatedBySquadId] = [g].[SquadId]
WHERE [l].[Discriminator] = N'LocustCommander' AND ([g].[Nickname] IS NULL OR [g].[SquadId] IS NULL)
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_contains(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_contains(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE ([t].[Note] <> N'K.I.A.' OR [t].[Note] IS NULL) AND EXISTS (
    SELECT 1
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    WHERE [g0].[SquadId] = [g].[SquadId])
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_DTOs(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_DTOs(async);

        AssertSql(
"""
SELECT [g].[SquadId] AS [Id]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE [t].[Note] <> N'K.I.A.' OR [t].[Note] IS NULL
""");
    }

    public override async Task Non_unicode_string_literals_in_contains_is_used_for_non_unicode_column(bool async)
    {
        await base.Non_unicode_string_literals_in_contains_is_used_for_non_unicode_column(async);

        AssertSql(
"""
@__cities_0='["Unknown","Jacinto\u0027s location","Ephyra\u0027s location"]' (Size = 4000)

SELECT [c].[Name], [c].[Location], [c].[Nation], [c].[PeriodEnd], [c].[PeriodStart]
FROM [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c]
WHERE EXISTS (
    SELECT 1
    FROM OpenJson(@__cities_0) AS [c0]
    WHERE CAST([c0].[value] AS varchar(100)) = [c].[Location] OR ([c0].[value] IS NULL AND [c].[Location] IS NULL))
""");
    }

    public override async Task Include_on_derived_type_with_order_by_and_paging(bool async)
    {
        await base.Include_on_derived_type_with_order_by_and_paging(async);

        AssertSql(
"""
@__p_0='10'

SELECT [t0].[Name], [t0].[Discriminator], [t0].[LocustHordeId], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[ThreatLevel], [t0].[ThreatLevelByte], [t0].[ThreatLevelNullableByte], [t0].[DefeatedByNickname], [t0].[DefeatedBySquadId], [t0].[HighCommandId], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[Discriminator0], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[PeriodEnd0], [t0].[PeriodStart0], [t0].[Rank], [t0].[Id], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM (
    SELECT TOP(@__p_0) [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator] AS [Discriminator0], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd] AS [PeriodEnd0], [g].[PeriodStart] AS [PeriodStart0], [g].[Rank], [t].[Id], [t].[Note]
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [l].[DefeatedByNickname] = [g].[Nickname] AND [l].[DefeatedBySquadId] = [g].[SquadId]
    LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON ([g].[Nickname] = [t].[GearNickName] OR ([g].[Nickname] IS NULL AND [t].[GearNickName] IS NULL)) AND ([g].[SquadId] = [t].[GearSquadId] OR ([g].[SquadId] IS NULL AND [t].[GearSquadId] IS NULL))
    ORDER BY [t].[Note]
) AS [t0]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [t0].[FullName] = [w].[OwnerFullName]
ORDER BY [t0].[Note], [t0].[Name], [t0].[Nickname], [t0].[SquadId], [t0].[Id]
""");
    }

    public override async Task Where_datetimeoffset_day_component(bool async)
    {
        await base.Where_datetimeoffset_day_component(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE DATEPART(day, [m].[Timeline]) = 2
""");
    }

    public override async Task Projecting_required_string_column_compared_to_null_parameter(bool async)
    {
        await base.Projecting_required_string_column_compared_to_null_parameter(async);

        AssertSql(
"""
SELECT CAST(0 AS bit)
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys(bool async)
    {
        await base.Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys(async);

        AssertSql(
"""
SELECT [g].[FullName]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[Nickname] = [t].[GearNickName] AND [g].[SquadId] = [t].[GearSquadId]
WHERE [g].[Discriminator] = N'Officer' AND EXISTS (
    SELECT 1
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    WHERE [g].[Nickname] = [g0].[LeaderNickname] AND [g].[SquadId] = [g0].[LeaderSquadId])
ORDER BY [g].[HasSoulPatch] DESC, [t].[Note]
""");
    }

    public override async Task Collection_navigation_access_on_derived_entity_using_cast_in_SelectMany(bool async)
    {
        await base.Collection_navigation_access_on_derived_entity_using_cast_in_SelectMany(async);

        AssertSql(
"""
SELECT [f].[Name], [l].[Name] AS [LeaderName]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
INNER JOIN [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l] ON [f].[Id] = [l].[LocustHordeId]
ORDER BY [l].[Name]
""");
    }

    public override async Task Filter_with_complex_predicate_containing_subquery(bool async)
    {
        await base.Filter_with_complex_predicate_containing_subquery(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[FullName] <> N'Dom' AND EXISTS (
    SELECT 1
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName] AND [w].[IsAutomatic] = CAST(1 AS bit))
""");
    }

    public override async Task Navigation_based_on_complex_expression1(bool async)
    {
        await base.Navigation_based_on_complex_expression1(async);

        AssertSql(
"""
SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[PeriodEnd], [f].[PeriodStart], [f].[ServerAddress], [f].[CommanderName], [f].[Eradicated]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
LEFT JOIN (
    SELECT [l].[Name]
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
WHERE [t].[Name] IS NOT NULL
""");
    }

    public override async Task Group_by_nullable_property_HasValue_and_project_the_grouping_key(bool async)
    {
        await base.Group_by_nullable_property_HasValue_and_project_the_grouping_key(async);

        AssertSql(
"""
SELECT [t].[Key]
FROM (
    SELECT CASE
        WHEN [w].[SynergyWithId] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [Key]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
) AS [t]
GROUP BY [t].[Key]
""");
    }

    public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3(bool async)
    {
        await base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3(async);

        AssertSql(
"""
SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank], [g].[Nickname], [g].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[PeriodEnd], [w0].[PeriodStart], [w0].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[LeaderNickname] = [g0].[Nickname]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g0].[FullName] = [w].[OwnerFullName]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0] ON [g].[FullName] = [w0].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId], [w].[Id]
""");
    }

    public override async Task Where_datetimeoffset_hour_component(bool async)
    {
        await base.Where_datetimeoffset_hour_component(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE DATEPART(hour, [m].[Timeline]) = 10
""");
    }

    public override async Task Property_access_on_derived_entity_using_cast(bool async)
    {
        await base.Property_access_on_derived_entity_using_cast(async);

        AssertSql(
"""
SELECT [f].[Name], [f].[Eradicated]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
ORDER BY [f].[Name]
""");
    }

    public override async Task Correlated_collections_deeply_nested_left_join(bool async)
    {
        await base.Correlated_collections_deeply_nested_left_join(async);

        AssertSql(
"""
SELECT [t].[Id], [g].[Nickname], [g].[SquadId], [s].[Id], [t0].[Nickname], [t0].[SquadId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[SynergyWithId]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname]
LEFT JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [g].[SquadId] = [s].[Id]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [t1].[Id], [t1].[AmmunitionType], [t1].[IsAutomatic], [t1].[Name], [t1].[OwnerFullName], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[SynergyWithId]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    LEFT JOIN (
        SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [w].[IsAutomatic] = CAST(1 AS bit)
    ) AS [t1] ON [g0].[FullName] = [t1].[OwnerFullName]
    WHERE [g0].[HasSoulPatch] = CAST(1 AS bit)
) AS [t0] ON [s].[Id] = [t0].[SquadId]
ORDER BY [t].[Note], [g].[Nickname] DESC, [t].[Id], [g].[SquadId], [s].[Id], [t0].[Nickname], [t0].[SquadId]
""");
    }

    public override async Task Constant_enum_with_same_underlying_value_as_previously_parameterized_int(bool async)
    {
        await base.Constant_enum_with_same_underlying_value_as_previously_parameterized_int(async);

        AssertSql(
"""
@__p_0='1'

SELECT TOP(@__p_0) [g].[Rank] & 1
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
ORDER BY [g].[Nickname]
""");
    }

    public override async Task Bitwise_projects_values_in_select(bool async)
    {
        await base.Bitwise_projects_values_in_select(async);

        AssertSql(
"""
SELECT TOP(1) CASE
    WHEN [g].[Rank] & 2 = 2 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [BitwiseTrue], CASE
    WHEN [g].[Rank] & 2 = 4 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [BitwiseFalse], [g].[Rank] & 2 AS [BitwiseValue]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Rank] & 2 = 2
""");
    }

    public override async Task Comparison_with_value_converted_subclass(bool async)
    {
        await base.Comparison_with_value_converted_subclass(async);

        AssertSql(
"""
SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[PeriodEnd], [f].[PeriodStart], [f].[ServerAddress], [f].[CommanderName], [f].[Eradicated]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
WHERE [f].[ServerAddress] = CAST(N'127.0.0.1' AS nvarchar(45))
""");
    }

    public override async Task Navigation_access_on_derived_materialized_entity_using_cast(bool async)
    {
        await base.Navigation_access_on_derived_materialized_entity_using_cast(async);

        AssertSql(
"""
SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[PeriodEnd], [f].[PeriodStart], [f].[ServerAddress], [f].[CommanderName], [f].[Eradicated], [t].[ThreatLevel] AS [Threat]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
LEFT JOIN (
    SELECT [l].[Name], [l].[ThreatLevel]
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
ORDER BY [f].[Name]
""");
    }

    public override async Task Select_null_conditional_with_inheritance_negative(bool async)
    {
        await base.Select_null_conditional_with_inheritance_negative(async);

        AssertSql(
"""
SELECT CASE
    WHEN [f].[CommanderName] IS NOT NULL THEN [f].[Eradicated]
    ELSE NULL
END
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
""");
    }

    public override async Task Take_without_orderby_followed_by_orderBy_is_pushed_down1(bool async)
    {
        await base.Take_without_orderby_followed_by_orderBy_is_pushed_down1(async);

        AssertSql(
"""
@__p_0='999'

SELECT [t].[FullName]
FROM (
    SELECT TOP(@__p_0) [g].[FullName], [g].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [g].[HasSoulPatch] = CAST(0 AS bit)
) AS [t]
ORDER BY [t].[Rank]
""");
    }

    public override async Task String_based_Include_navigation_on_derived_type(bool async)
    {
        await base.String_based_Include_navigation_on_derived_type(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[Nickname] = [g0].[LeaderNickname] AND [g].[SquadId] = [g0].[LeaderSquadId]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [g0].[Nickname]
""");
    }

    public override async Task Select_ternary_operation_multiple_conditions(bool async)
    {
        await base.Select_ternary_operation_multiple_conditions(async);

        AssertSql(
"""
SELECT [w].[Id], CASE
    WHEN [w].[AmmunitionType] = 2 AND [w].[SynergyWithId] = 1 THEN N'Yes'
    ELSE N'No'
END AS [IsCartridge]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
""");
    }

    public override async Task Select_subquery_projecting_single_constant_of_non_mapped_type(bool async)
    {
        await base.Select_subquery_projecting_single_constant_of_non_mapped_type(async);

        AssertSql(
"""
SELECT [s].[Name], [t0].[c]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
LEFT JOIN (
    SELECT [t].[c], [t].[SquadId]
    FROM (
        SELECT 1 AS [c], [g].[SquadId], ROW_NUMBER() OVER(PARTITION BY [g].[SquadId] ORDER BY [g].[Nickname], [g].[SquadId]) AS [row]
        FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
        WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [s].[Id] = [t0].[SquadId]
""");
    }

    public override async Task Select_subquery_boolean(bool async)
    {
        await base.Select_subquery_boolean(async);

        AssertSql(
"""
SELECT COALESCE((
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]), CAST(0 AS bit))
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Member_access_on_derived_entity_using_cast_and_let(bool async)
    {
        await base.Member_access_on_derived_entity_using_cast_and_let(async);

        AssertSql(
"""
SELECT [f].[Name], [f].[Eradicated]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
ORDER BY [f].[Name]
""");
    }

    public override async Task ThenInclude_collection_on_derived_after_base_reference(bool async)
    {
        await base.ThenInclude_collection_on_derived_after_base_reference(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] = [w].[OwnerFullName]
ORDER BY [t].[Id], [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Include_reference_on_derived_type_using_string(bool async)
    {
        await base.Include_reference_on_derived_type_using_string(async);

        AssertSql(
"""
SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [l].[DefeatedByNickname] = [g].[Nickname] AND [l].[DefeatedBySquadId] = [g].[SquadId]
""");
    }

    public override async Task Non_unicode_string_literals_is_used_for_non_unicode_column_with_subquery(bool async)
    {
        await base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_subquery(async);

        AssertSql(
"""
SELECT [c].[Name], [c].[Location], [c].[Nation], [c].[PeriodEnd], [c].[PeriodStart]
FROM [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c]
WHERE [c].[Location] = 'Unknown' AND (
    SELECT COUNT(*)
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [c].[Name] = [g].[CityOfBirthName] AND [g].[Nickname] = N'Paduk') = 1
""");
    }

    public override async Task Project_derivied_entity_with_convert_to_parent(bool async)
    {
        await base.Project_derivied_entity_with_convert_to_parent(async);

        AssertSql(
"""
SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[PeriodEnd], [f].[PeriodStart], [f].[ServerAddress], [f].[CommanderName], [f].[Eradicated]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
""");
    }

    public override async Task Select_subquery_int_with_pushdown_and_coalesce2(bool async)
    {
        await base.Select_subquery_int_with_pushdown_and_coalesce2(async);

        AssertSql(
"""
SELECT COALESCE((
    SELECT TOP(1) [w].[Id]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]), (
    SELECT TOP(1) [w0].[Id]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0]
    WHERE [g].[FullName] = [w0].[OwnerFullName]
    ORDER BY [w0].[Id]))
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Join_with_inner_being_a_subquery_projecting_anonymous_type_with_single_property(bool async)
    {
        await base.Join_with_inner_being_a_subquery_projecting_anonymous_type_with_single_property(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[Nickname] = [g0].[Nickname]
""");
    }

    public override async Task ThenInclude_reference_on_derived_after_derived_collection(bool async)
    {
        await base.ThenInclude_reference_on_derived_after_derived_collection(async);

        AssertSql(
"""
SELECT [f].[Id], [f].[CapitalName], [f].[Discriminator], [f].[Name], [f].[PeriodEnd], [f].[PeriodStart], [f].[ServerAddress], [f].[CommanderName], [f].[Eradicated], [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[PeriodEnd], [t].[PeriodStart], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator0], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[PeriodEnd0], [t].[PeriodStart0], [t].[Rank]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
LEFT JOIN (
    SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator] AS [Discriminator0], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd] AS [PeriodEnd0], [g].[PeriodStart] AS [PeriodStart0], [g].[Rank]
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [l].[DefeatedByNickname] = [g].[Nickname] AND [l].[DefeatedBySquadId] = [g].[SquadId]
) AS [t] ON [f].[Id] = [t].[LocustHordeId]
ORDER BY [f].[Id], [t].[Name], [t].[Nickname]
""");
    }

    public override async Task Subquery_containing_join_gets_lifted_clashing_names(bool async)
    {
        await base.Subquery_containing_join_gets_lifted_clashing_names(async);

        AssertSql(
"""
SELECT [g].[Nickname]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[Nickname] = [t].[GearNickName]
INNER JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t0] ON [g].[Nickname] = [t0].[GearNickName]
WHERE [t].[GearNickName] <> N'Cole Train' OR [t].[GearNickName] IS NULL
ORDER BY [g].[Nickname], [t0].[Id]
""");
    }

    public override async Task Include_with_join_reference1(bool async)
    {
        await base.Include_with_join_reference1(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [c].[Name], [c].[Location], [c].[Nation], [c].[PeriodEnd], [c].[PeriodStart]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[SquadId] = [t].[GearSquadId] AND [g].[Nickname] = [t].[GearNickName]
INNER JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g].[CityOfBirthName] = [c].[Name]
""");
    }

    public override async Task GetValueOrDefault_in_filter_non_nullable_column(bool async)
    {
        await base.GetValueOrDefault_in_filter_non_nullable_column(async);

        AssertSql(
"""
SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE COALESCE([w].[Id], 0) = 0
""");
    }

    public override async Task Checked_context_with_cast_does_not_fail(bool async)
    {
        await base.Checked_context_with_cast_does_not_fail(async);

        AssertSql(
"""
SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
WHERE CAST([l].[ThreatLevel] AS tinyint) >= CAST(5 AS tinyint)
""");
    }

    public override async Task ThenInclude_collection_on_derived_after_derived_collection(bool async)
    {
        await base.ThenInclude_collection_on_derived_after_derived_collection(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Rank], [t].[Nickname0], [t].[SquadId0], [t].[AssignedCityName0], [t].[CityOfBirthName0], [t].[Discriminator0], [t].[FullName0], [t].[HasSoulPatch0], [t].[LeaderNickname0], [t].[LeaderSquadId0], [t].[PeriodEnd0], [t].[PeriodStart0], [t].[Rank0]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank], [g1].[Nickname] AS [Nickname0], [g1].[SquadId] AS [SquadId0], [g1].[AssignedCityName] AS [AssignedCityName0], [g1].[CityOfBirthName] AS [CityOfBirthName0], [g1].[Discriminator] AS [Discriminator0], [g1].[FullName] AS [FullName0], [g1].[HasSoulPatch] AS [HasSoulPatch0], [g1].[LeaderNickname] AS [LeaderNickname0], [g1].[LeaderSquadId] AS [LeaderSquadId0], [g1].[PeriodEnd] AS [PeriodEnd0], [g1].[PeriodStart] AS [PeriodStart0], [g1].[Rank] AS [Rank0]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g1] ON [g0].[Nickname] = [g1].[LeaderNickname] AND [g0].[SquadId] = [g1].[LeaderSquadId]
) AS [t] ON [g].[Nickname] = [t].[LeaderNickname] AND [g].[SquadId] = [t].[LeaderSquadId]
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Nickname], [t].[SquadId], [t].[Nickname0]
""");
    }

    public override async Task SelectMany_predicate_with_non_equality_comparison_converted_to_inner_join(bool async)
    {
        await base.SelectMany_predicate_with_non_equality_comparison_converted_to_inner_join(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] <> [w].[OwnerFullName] OR [w].[OwnerFullName] IS NULL
ORDER BY [g].[Nickname], [w].[Id]
""");
    }

    public override async Task Nav_rewrite_with_convert1(bool async)
    {
        await base.Nav_rewrite_with_convert1(async);

        AssertSql(
"""
SELECT [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[PeriodEnd], [t].[PeriodStart], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
LEFT JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [f].[CapitalName] = [c].[Name]
LEFT JOIN (
    SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
WHERE [c].[Name] <> N'Foo' OR [c].[Name] IS NULL
""");
    }

    public override async Task Concat_scalars_with_count(bool async)
    {
        await base.Concat_scalars_with_count(async);

        AssertSql(
"""
SELECT COUNT(*)
FROM (
    SELECT [g].[Nickname]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    UNION ALL
    SELECT [g0].[FullName] AS [Nickname]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
) AS [t]
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_projection(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_projection(async);

        AssertSql(
"""
SELECT [g].[SquadId]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE [t].[Note] <> N'K.I.A.' OR [t].[Note] IS NULL
""");
    }

    public override async Task GroupJoin_Composite_Key(bool async)
    {
        await base.GroupJoin_Composite_Key(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
INNER JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
""");
    }

    public override async Task Contains_on_collection_of_nullable_byte_subquery_null_parameter(bool async)
    {
        await base.Contains_on_collection_of_nullable_byte_subquery_null_parameter(async);

        AssertSql(
"""
SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
WHERE EXISTS (
    SELECT 1
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    WHERE [l0].[ThreatLevelNullableByte] IS NULL)
""");
    }

    public override async Task Group_by_over_projection_with_multiple_properties_accessed_thru_navigation(bool async)
    {
        await base.Group_by_over_projection_with_multiple_properties_accessed_thru_navigation(async);

        AssertSql(
"""
SELECT [c].[Name]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g].[CityOfBirthName] = [c].[Name]
GROUP BY [c].[Name]
""");
    }

    public override async Task Outer_parameter_in_group_join_with_DefaultIfEmpty(bool async)
    {
        await base.Outer_parameter_in_group_join_with_DefaultIfEmpty(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [t0].[Note], [t0].[Id], [t0].[Nickname], [t0].[SquadId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
OUTER APPLY (
    SELECT [t].[Note], [t].[Id], [g0].[Nickname], [g0].[SquadId]
    FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
    LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[FullName] = [g0].[FullName]
) AS [t0]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [t0].[Id], [t0].[Nickname]
""");
    }

    public override async Task Project_collection_navigation_with_inheritance2(bool async)
    {
        await base.Project_collection_navigation_with_inheritance2(async);

        AssertSql(
"""
SELECT [f].[Id], [t].[Name], [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
LEFT JOIN (
    SELECT [l].[Name], [l].[DefeatedByNickname], [l].[DefeatedBySquadId]
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[DefeatedByNickname] = [g].[Nickname] AND [t].[DefeatedBySquadId] = [g].[SquadId]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON ([g].[Nickname] = [g0].[LeaderNickname] OR ([g].[Nickname] IS NULL AND [g0].[LeaderNickname] IS NULL)) AND [g].[SquadId] = [g0].[LeaderSquadId]
ORDER BY [f].[Id], [t].[Name], [g].[Nickname], [g].[SquadId], [g0].[Nickname]
""");
    }

    public override async Task Correlated_collections_inner_subquery_predicate_references_outer_qsre(bool async)
    {
        await base.Correlated_collections_inner_subquery_predicate_references_outer_qsre(async);

        AssertSql(
"""
SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t].[ReportName], [t].[Nickname], [t].[SquadId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
OUTER APPLY (
    SELECT [g0].[FullName] AS [ReportName], [g0].[Nickname], [g0].[SquadId]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    WHERE [g].[Nickname] = [g0].[LeaderNickname] AND [g].[SquadId] = [g0].[LeaderSquadId] AND [g].[FullName] <> N'Foo'
) AS [t]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Nickname]
""");
    }

    public override async Task Project_discriminator_columns(bool async)
    {
        await base.Project_discriminator_columns(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[Discriminator]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""",
            //
"""
SELECT [g].[Nickname], [g].[Discriminator]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Discriminator] = N'Officer'
""",
            //
"""
SELECT [f].[Id], [f].[Discriminator]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
""",
            //
"""
SELECT [f].[Id], [f].[Discriminator]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
""",
            //
"""
SELECT [l].[Name], [l].[Discriminator]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
""",
            //
"""
SELECT [l].[Name], [l].[Discriminator]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
WHERE [l].[Discriminator] = N'LocustCommander'
""");
    }

    public override async Task Join_entity_with_itself_grouped_by_key_followed_by_include_skip_take(bool async)
    {
        await base.Join_entity_with_itself_grouped_by_key_followed_by_include_skip_take(async);

        AssertSql(
"""
@__p_0='0'
@__p_1='10'

SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Rank], [t0].[HasSoulPatch0], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [t].[HasSoulPatch] AS [HasSoulPatch0]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    INNER JOIN (
        SELECT MIN(CAST(LEN([g0].[Nickname]) AS int)) AS [c], [g0].[HasSoulPatch]
        FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
        WHERE [g0].[Nickname] <> N'Dom'
        GROUP BY [g0].[HasSoulPatch]
    ) AS [t] ON CAST(LEN([g].[Nickname]) AS int) = [t].[c]
    ORDER BY [g].[Nickname]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t0]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [t0].[FullName] = [w].[OwnerFullName]
ORDER BY [t0].[Nickname], [t0].[SquadId], [t0].[HasSoulPatch0]
""");
    }

    public override async Task Select_subquery_distinct_singleordefault_boolean_empty1(bool async)
    {
        await base.Select_subquery_distinct_singleordefault_boolean_empty1(async);

        AssertSql(
"""
SELECT COALESCE((
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName] AND [w].[Name] = N'BFG'
    ) AS [t]), CAST(0 AS bit))
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
""");
    }

    public override async Task Correlated_collections_same_collection_projected_multiple_times(bool async)
    {
        await base.Correlated_collections_same_collection_projected_multiple_times(async);

        AssertSql(
"""
SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[PeriodEnd], [t].[PeriodStart], [t].[SynergyWithId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit)
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
LEFT JOIN (
    SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[PeriodEnd], [w0].[PeriodStart], [w0].[SynergyWithId]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0]
    WHERE [w0].[IsAutomatic] = CAST(1 AS bit)
) AS [t0] ON [g].[FullName] = [t0].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Id]
""");
    }

    public override async Task OfType_in_subquery_works(bool async)
    {
        await base.OfType_in_subquery_works(async);

        AssertSql(
"""
SELECT [t].[Name], [t].[Location], [t].[Nation], [t].[PeriodEnd], [t].[PeriodStart]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN (
    SELECT [c].[Name], [c].[Location], [c].[Nation], [c].[PeriodEnd], [c].[PeriodStart], [g0].[LeaderNickname], [g0].[LeaderSquadId]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    LEFT JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g0].[AssignedCityName] = [c].[Name]
    WHERE [g0].[Discriminator] = N'Officer'
) AS [t] ON [g].[Nickname] = [t].[LeaderNickname] AND [g].[SquadId] = [t].[LeaderSquadId]
WHERE [g].[Discriminator] = N'Officer'
""");
    }

    public override async Task Where_nullable_enum_with_nullable_parameter(bool async)
    {
        await base.Where_nullable_enum_with_nullable_parameter(async);

        AssertSql(
"""
@__ammunitionType_0='1' (Nullable = true)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE [w].[AmmunitionType] = @__ammunitionType_0
""",
            //
"""
SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE [w].[AmmunitionType] IS NULL
""");
    }

    public override async Task Concat_with_scalar_projection(bool async)
    {
        await base.Concat_with_scalar_projection(async);

        AssertSql(
"""
SELECT [g].[Nickname]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
UNION ALL
SELECT [g0].[Nickname]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
""");
    }

    public override async Task Correlated_collections_different_collections_projected(bool async)
    {
        await base.Correlated_collections_different_collections_projected(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [t].[Name], [t].[IsAutomatic], [t].[Id], [g0].[Nickname], [g0].[Rank], [g0].[SquadId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [w].[Name], [w].[IsAutomatic], [w].[Id], [w].[OwnerFullName]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit)
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[Nickname] = [g0].[LeaderNickname] AND [g].[SquadId] = [g0].[LeaderSquadId]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[FullName], [g].[Nickname], [g].[SquadId], [t].[Id], [g0].[FullName], [g0].[Nickname]
""");
    }

    public override async Task Join_with_inner_being_a_subquery_projecting_single_property(bool async)
    {
        await base.Join_with_inner_being_a_subquery_projecting_single_property(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[Nickname] = [g0].[Nickname]
""");
    }

    public override async Task Non_unicode_string_literal_is_used_for_non_unicode_column_right(bool async)
    {
        await base.Non_unicode_string_literal_is_used_for_non_unicode_column_right(async);

        AssertSql(
"""
SELECT [c].[Name], [c].[Location], [c].[Nation], [c].[PeriodEnd], [c].[PeriodStart]
FROM [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c]
WHERE 'Unknown' = [c].[Location]
""");
    }

    public override async Task Checked_context_with_addition_does_not_fail(bool async)
    {
        await base.Checked_context_with_addition_does_not_fail(async);

        AssertSql(
"""
SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
WHERE CAST([l].[ThreatLevel] AS bigint) >= CAST(5 AS bigint) + CAST([l].[ThreatLevel] AS bigint)
""");
    }

    public override async Task Correlated_collections_from_left_join_with_additional_elements_projected_of_that_join(bool async)
    {
        await base.Correlated_collections_from_left_join_with_additional_elements_projected_of_that_join(async);

        AssertSql(
"""
SELECT [w].[Id], [g].[Nickname], [g].[SquadId], [s].[Id], [t0].[Nickname], [t0].[SquadId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[SynergyWithId], [t0].[Rank]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [w].[OwnerFullName] = [g].[FullName]
LEFT JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [g].[SquadId] = [s].[Id]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[PeriodEnd], [t].[PeriodStart], [t].[SynergyWithId], [g0].[Rank], [g0].[FullName]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    LEFT JOIN (
        SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[PeriodEnd], [w0].[PeriodStart], [w0].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0]
        WHERE [w0].[IsAutomatic] = CAST(0 AS bit)
    ) AS [t] ON [g0].[FullName] = [t].[OwnerFullName]
) AS [t0] ON [s].[Id] = [t0].[SquadId]
ORDER BY [w].[Name], [w].[Id], [g].[Nickname], [g].[SquadId], [s].[Id], [t0].[FullName] DESC, [t0].[Nickname], [t0].[SquadId], [t0].[Id]
""");
    }

    public override async Task Project_collection_navigation_with_inheritance3(bool async)
    {
        await base.Project_collection_navigation_with_inheritance3(async);

        AssertSql(
"""
SELECT [f].[Id], [t].[Name], [g].[Nickname], [g].[SquadId], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
LEFT JOIN (
    SELECT [l].[Name], [l].[DefeatedByNickname], [l].[DefeatedBySquadId]
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[DefeatedByNickname] = [g].[Nickname] AND [t].[DefeatedBySquadId] = [g].[SquadId]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON ([g].[Nickname] = [g0].[LeaderNickname] OR ([g].[Nickname] IS NULL AND [g0].[LeaderNickname] IS NULL)) AND [g].[SquadId] = [g0].[LeaderSquadId]
ORDER BY [f].[Id], [t].[Name], [g].[Nickname], [g].[SquadId], [g0].[Nickname]
""");
    }

    public override async Task Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up(bool async)
    {
        await base.Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up(async);

        AssertSql(
"""
SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t0].[FullName], [t0].[Nickname], [t0].[SquadId], [t0].[Name], [t0].[Nickname0], [t0].[Id]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [g0].[FullName], [g0].[Nickname], [g0].[SquadId], [t].[Name], [t].[Nickname] AS [Nickname0], [t].[Id], [g0].[LeaderNickname], [g0].[LeaderSquadId]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    OUTER APPLY (
        SELECT [w].[Name], [g0].[Nickname], [w].[Id]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [g0].[FullName] = [w].[OwnerFullName] AND ([w].[Name] <> N'Bar' OR [w].[Name] IS NULL)
    ) AS [t]
    WHERE [g0].[FullName] <> N'Foo'
) AS [t0] ON [g].[Nickname] = [t0].[LeaderNickname] AND [g].[SquadId] = [t0].[LeaderSquadId]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [t0].[Nickname], [t0].[SquadId]
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_predicate_negated(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_predicate_negated(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE [g].[HasSoulPatch] = CAST(0 AS bit)
""");
    }

    public override async Task Include_on_derived_multi_level(bool async)
    {
        await base.Include_on_derived_multi_level(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Rank], [t].[Id], [t].[Banner], [t].[Banner5], [t].[InternalNumber], [t].[Name], [t].[PeriodEnd0], [t].[PeriodStart0], [t].[SquadId0], [t].[MissionId], [t].[PeriodEnd1], [t].[PeriodStart1]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank], [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s].[PeriodEnd] AS [PeriodEnd0], [s].[PeriodStart] AS [PeriodStart0], [s0].[SquadId] AS [SquadId0], [s0].[MissionId], [s0].[PeriodEnd] AS [PeriodEnd1], [s0].[PeriodStart] AS [PeriodStart1]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    INNER JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [g0].[SquadId] = [s].[Id]
    LEFT JOIN [SquadMissions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s0] ON [s].[Id] = [s0].[SquadId]
) AS [t] ON [g].[Nickname] = [t].[LeaderNickname] AND [g].[SquadId] = [t].[LeaderSquadId]
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Nickname], [t].[SquadId], [t].[Id], [t].[SquadId0]
""");
    }

    public override async Task Null_propagation_optimization2(bool async)
    {
        await base.Null_propagation_optimization2(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE CASE
    WHEN [g].[LeaderNickname] IS NULL THEN NULL
    ELSE CASE
        WHEN [g].[LeaderNickname] IS NOT NULL AND [g].[LeaderNickname] LIKE N'%us' THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
END = CAST(1 AS bit)
""");
    }

    public override async Task Select_subquery_distinct_firstordefault(bool async)
    {
        await base.Select_subquery_distinct_firstordefault(async);

        AssertSql(
"""
SELECT (
    SELECT TOP(1) [t].[Name]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
    ) AS [t]
    ORDER BY [t].[Id])
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
""");
    }

    public override async Task Include_on_entity_that_is_not_present_in_final_projection_but_uses_TypeIs_instead(bool async)
    {
        await base.Include_on_entity_that_is_not_present_in_final_projection_but_uses_TypeIs_instead(async);

        AssertSql(
"""
SELECT [g].[Nickname], CASE
    WHEN [g].[Discriminator] = N'Officer' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsOfficer]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Composite_key_entity_equal(bool async)
    {
        await base.Composite_key_entity_equal(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
CROSS JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
WHERE [g].[Nickname] = [g0].[Nickname] AND [g].[SquadId] = [g0].[SquadId]
""");
    }

    public override async Task Select_null_conditional_with_inheritance(bool async)
    {
        await base.Select_null_conditional_with_inheritance(async);

        AssertSql(
"""
SELECT CASE
    WHEN [f].[CommanderName] IS NOT NULL THEN [f].[CommanderName]
    ELSE NULL
END
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
""");
    }

    public override async Task Select_ternary_operation_with_boolean(bool async)
    {
        await base.Select_ternary_operation_with_boolean(async);

        AssertSql(
"""
SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = CAST(1 AS bit) THEN 1
    ELSE 0
END AS [Num]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
""");
    }

    public override async Task Order_by_entity_qsre_composite_key(bool async)
    {
        await base.Order_by_entity_qsre_composite_key(async);

        AssertSql(
"""
SELECT [w].[Name]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [w].[OwnerFullName] = [g].[FullName]
ORDER BY [g].[Nickname], [g].[SquadId], [w].[Id]
""");
    }

    public override async Task Include_using_alternate_key(bool async)
    {
        await base.Include_using_alternate_key(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] = [w].[OwnerFullName]
WHERE [g].[Nickname] = N'Marcus'
ORDER BY [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Correlated_collections_with_FirstOrDefault(bool async)
    {
        await base.Correlated_collections_with_FirstOrDefault(async);

        AssertSql(
"""
SELECT (
    SELECT TOP(1) [g].[FullName]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [s].[Id] = [g].[SquadId]
    ORDER BY [g].[Nickname])
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
ORDER BY [s].[Name]
""");
    }

    public override async Task Where_subquery_distinct_last_boolean(bool async)
    {
        await base.Where_subquery_distinct_last_boolean(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(0 AS bit) AND (
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
    ) AS [t]
    ORDER BY [t].[Id] DESC) = CAST(1 AS bit)
ORDER BY [g].[Nickname]
""");
    }

    public override async Task Where_enum_has_flag_subquery_client_eval(bool async)
    {
        await base.Where_enum_has_flag_subquery_client_eval(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Rank] & (
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId]) = (
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId]) OR (
    SELECT TOP(1) [g0].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    ORDER BY [g0].[Nickname], [g0].[SquadId]) IS NULL
""");
    }

    public override async Task Correlated_collection_via_SelectMany_with_Distinct_missing_indentifying_columns_in_projection(bool async)
    {
        await base.Correlated_collection_via_SelectMany_with_Distinct_missing_indentifying_columns_in_projection(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [t].[HasSoulPatch]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
OUTER APPLY (
    SELECT DISTINCT [g1].[HasSoulPatch]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [w].[OwnerFullName] = [g0].[FullName]
    LEFT JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g0].[AssignedCityName] = [c].[Name]
    INNER JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g1] ON [c].[Name] = [g1].[CityOfBirthName]
    WHERE [g].[FullName] = [w].[OwnerFullName]
) AS [t]
ORDER BY [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Subquery_is_lifted_from_main_from_clause_of_SelectMany(bool async)
    {
        await base.Subquery_is_lifted_from_main_from_clause_of_SelectMany(async);

        AssertSql(
"""
SELECT [g].[FullName] AS [Name1], [g0].[FullName] AS [Name2]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
CROSS JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit) AND [g0].[HasSoulPatch] = CAST(0 AS bit)
ORDER BY [g].[FullName]
""");
    }

    public override async Task Select_subquery_projecting_single_constant_inside_anonymous(bool async)
    {
        await base.Select_subquery_projecting_single_constant_inside_anonymous(async);

        AssertSql(
"""
SELECT [s].[Name], [t0].[One]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
LEFT JOIN (
    SELECT [t].[One], [t].[SquadId]
    FROM (
        SELECT 1 AS [One], [g].[SquadId], ROW_NUMBER() OVER(PARTITION BY [g].[SquadId] ORDER BY [g].[Nickname], [g].[SquadId]) AS [row]
        FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
        WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [s].[Id] = [t0].[SquadId]
""");
    }

    public override async Task Include_collection_on_derived_type_using_lambda_with_soft_cast(bool async)
    {
        await base.Include_collection_on_derived_type_using_lambda_with_soft_cast(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[Nickname] = [g0].[LeaderNickname] AND [g].[SquadId] = [g0].[LeaderSquadId]
ORDER BY [g].[Nickname], [g].[SquadId], [g0].[Nickname]
""");
    }

    public override async Task TimeSpan_Milliseconds(bool async)
    {
        await base.TimeSpan_Milliseconds(async);

        AssertSql(
"""
SELECT DATEPART(millisecond, [m].[Duration])
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
""");
    }

    public override async Task Navigation_access_via_EFProperty_on_derived_entity_using_cast(bool async)
    {
        await base.Navigation_access_via_EFProperty_on_derived_entity_using_cast(async);

        AssertSql(
"""
SELECT [f].[Name], [t].[ThreatLevel] AS [Threat]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
LEFT JOIN (
    SELECT [l].[Name], [l].[ThreatLevel]
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
ORDER BY [f].[Name]
""");
    }

    public override async Task Byte_array_filter_by_length_literal(bool async)
    {
        await base.Byte_array_filter_by_length_literal(async);

        AssertSql(
"""
SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
WHERE CAST(DATALENGTH([s].[Banner]) AS int) = 1
""");
    }

    public override async Task GroupBy_Property_Include_Select_Average(bool async)
    {
        await base.GroupBy_Property_Include_Select_Average(async);

        AssertSql(
"""
SELECT AVG(CAST([g].[SquadId] AS float))
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
GROUP BY [g].[Rank]
""");
    }

    public override async Task Non_unicode_string_literals_is_used_for_non_unicode_column_with_contains(bool async)
    {
        await base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_contains(async);

        AssertSql(
"""
SELECT [c].[Name], [c].[Location], [c].[Nation], [c].[PeriodEnd], [c].[PeriodStart]
FROM [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c]
WHERE [c].[Location] LIKE '%Jacinto%'
""");
    }

    public override async Task Correlated_collection_with_complex_order_by_funcletized_to_constant_bool(bool async)
    {
        await base.Correlated_collection_with_complex_order_by_funcletized_to_constant_bool(async);

        AssertSql(
"""
@__nicknames_0='[]' (Size = 4000)

SELECT [g].[Nickname], [g].[SquadId], [w].[Name], [w].[Id]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] = [w].[OwnerFullName]
ORDER BY CASE
    WHEN EXISTS (
        SELECT 1
        FROM OpenJson(@__nicknames_0) AS [n]
        WHERE CAST([n].[value] AS nvarchar(450)) = [g].[Nickname]) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END DESC, [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Select_datetimeoffset_comparison_in_projection(bool async)
    {
        await base.Select_datetimeoffset_comparison_in_projection(async);

        AssertSql(
"""
SELECT CASE
    WHEN [m].[Timeline] > SYSDATETIMEOFFSET() THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
""");
    }

    public override async Task Include_multiple_circular_with_filter(bool async)
    {
        await base.Include_multiple_circular_with_filter(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [c].[Name], [c].[Location], [c].[Nation], [c].[PeriodEnd], [c].[PeriodStart], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g].[CityOfBirthName] = [c].[Name]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [c].[Name] = [g0].[AssignedCityName]
WHERE [g].[Nickname] = N'Marcus'
ORDER BY [g].[Nickname], [g].[SquadId], [c].[Name], [g0].[Nickname]
""");
    }

    public override async Task TimeSpan_Hours(bool async)
    {
        await base.TimeSpan_Hours(async);

        AssertSql(
"""
SELECT DATEPART(hour, [m].[Duration])
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
""");
    }

    public override async Task Singleton_Navigation_With_Member_Access(bool async)
    {
        await base.Singleton_Navigation_With_Member_Access(async);

        AssertSql(
"""
SELECT [g].[CityOfBirthName] AS [B]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE [g].[Nickname] = N'Marcus' AND ([g].[CityOfBirthName] <> N'Ephyra' OR [g].[CityOfBirthName] IS NULL)
""");
    }

    public override async Task Join_on_entity_qsre_keys_composite_key(bool async)
    {
        await base.Join_on_entity_qsre_keys_composite_key(async);

        AssertSql(
"""
SELECT [g].[FullName] AS [GearName1], [g0].[FullName] AS [GearName2]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[Nickname] = [g0].[Nickname] AND [g].[SquadId] = [g0].[SquadId]
""");
    }

    public override async Task Include_reference_on_derived_type_using_lambda(bool async)
    {
        await base.Include_reference_on_derived_type_using_lambda(async);

        AssertSql(
"""
SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [l].[DefeatedByNickname] = [g].[Nickname] AND [l].[DefeatedBySquadId] = [g].[SquadId]
""");
    }

    public override async Task Projecting_nullable_bool_in_conditional_works(bool async)
    {
        await base.Projecting_nullable_bool_in_conditional_works(async);

        AssertSql(
"""
SELECT CASE
    WHEN [g].[Nickname] IS NOT NULL AND [g].[SquadId] IS NOT NULL THEN [g].[HasSoulPatch]
    ELSE CAST(0 AS bit)
END AS [Prop]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
""");
    }

    public override async Task Where_nullable_enum_with_non_nullable_parameter(bool async)
    {
        await base.Where_nullable_enum_with_non_nullable_parameter(async);

        AssertSql(
"""
@__ammunitionType_0='1'

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE [w].[AmmunitionType] = @__ammunitionType_0
""");
    }

    public override async Task Contains_on_collection_of_nullable_byte_subquery_null_constant(bool async)
    {
        await base.Contains_on_collection_of_nullable_byte_subquery_null_constant(async);

        AssertSql(
"""
SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
WHERE EXISTS (
    SELECT 1
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    WHERE [l0].[ThreatLevelNullableByte] IS NULL)
""");
    }

    public override async Task Where_bitwise_and_enum(bool async)
    {
        await base.Where_bitwise_and_enum(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Rank] & 2 > 0
""",
            //
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Rank] & 2 = 2
""");
    }

    public override async Task Left_join_projection_using_coalesce_tracking(bool async)
    {
        await base.Left_join_projection_using_coalesce_tracking(async);

        AssertSql(
"""
SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[LeaderNickname] = [g0].[Nickname]
""");
    }

    public override async Task Navigation_based_on_complex_expression5(bool async)
    {
        await base.Navigation_based_on_complex_expression5(async);

        AssertSql(
"""
SELECT [t0].[Name], [t0].[Discriminator], [t0].[LocustHordeId], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[ThreatLevel], [t0].[ThreatLevelByte], [t0].[ThreatLevelNullableByte], [t0].[DefeatedByNickname], [t0].[DefeatedBySquadId], [t0].[HighCommandId], [t].[Name], [t].[Discriminator], [t].[LocustHordeId], [t].[PeriodEnd], [t].[PeriodStart], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
CROSS JOIN (
    SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId]
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t]
LEFT JOIN (
    SELECT [l0].[Name], [l0].[Discriminator], [l0].[LocustHordeId], [l0].[PeriodEnd], [l0].[PeriodStart], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId]
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    WHERE [l0].[Discriminator] = N'LocustCommander'
) AS [t0] ON [f].[CommanderName] = [t0].[Name]
""");
    }

    public override async Task ToString_boolean_property_nullable(bool async)
    {
        await base.ToString_boolean_property_nullable(async);

        AssertSql(
"""
SELECT CASE
    WHEN [f].[Eradicated] = CAST(0 AS bit) THEN N'False'
    WHEN [f].[Eradicated] = CAST(1 AS bit) THEN N'True'
    ELSE NULL
END
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
""");
    }

    public override async Task OrderBy_same_expression_containing_IsNull_correctly_deduplicates_the_ordering(bool async)
    {
        await base.OrderBy_same_expression_containing_IsNull_correctly_deduplicates_the_ordering(async);

        AssertSql(
"""
SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN CASE
        WHEN CAST(LEN([g].[Nickname]) AS int) = 5 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    ELSE NULL
END
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
ORDER BY CASE
    WHEN CASE
        WHEN [g].[LeaderNickname] IS NOT NULL THEN CASE
            WHEN CAST(LEN([g].[Nickname]) AS int) = 5 THEN CAST(1 AS bit)
            ELSE CAST(0 AS bit)
        END
        ELSE NULL
    END IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Select_subquery_projecting_single_constant_string(bool async)
    {
        await base.Select_subquery_projecting_single_constant_string(async);

        AssertSql(
"""
SELECT [s].[Name], (
    SELECT TOP(1) N'Foo'
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [s].[Id] = [g].[SquadId] AND [g].[HasSoulPatch] = CAST(1 AS bit)) AS [Gear]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
""");
    }

    public override async Task Select_length_of_string_property(bool async)
    {
        await base.Select_length_of_string_property(async);

        AssertSql(
"""
SELECT [w].[Name], CAST(LEN([w].[Name]) AS int) AS [Length]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_conditional_expression(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_conditional_expression(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE CASE
    WHEN [g].[HasSoulPatch] = CAST(1 AS bit) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CAST(1 AS bit)
""");
    }

    public override async Task Correlated_collections_inner_subquery_selector_references_outer_qsre(bool async)
    {
        await base.Correlated_collections_inner_subquery_selector_references_outer_qsre(async);

        AssertSql(
"""
SELECT [g].[FullName], [g].[Nickname], [g].[SquadId], [t].[ReportName], [t].[OfficerName], [t].[Nickname], [t].[SquadId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
OUTER APPLY (
    SELECT [g0].[FullName] AS [ReportName], [g].[FullName] AS [OfficerName], [g0].[Nickname], [g0].[SquadId]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    WHERE [g].[Nickname] = [g0].[LeaderNickname] AND [g].[SquadId] = [g0].[LeaderSquadId]
) AS [t]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Nickname]
""");
    }

    public override async Task Join_on_entity_qsre_keys_outer_key_is_navigation(bool async)
    {
        await base.Join_on_entity_qsre_keys_outer_key_is_navigation(async);

        AssertSql(
"""
SELECT [w].[Name] AS [Name1], [w1].[Name] AS [Name2]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
INNER JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w1] ON [w0].[Id] = [w1].[Id]
""");
    }

    public override async Task Correlated_collection_with_top_level_FirstOrDefault(bool async)
    {
        await base.Correlated_collection_with_top_level_FirstOrDefault(async);

        AssertSql(
"""
SELECT [t].[Nickname], [t].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM (
    SELECT TOP(1) [g].[Nickname], [g].[SquadId], [g].[FullName]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    ORDER BY [g].[Nickname]
) AS [t]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [t].[FullName] = [w].[OwnerFullName]
ORDER BY [t].[Nickname], [t].[SquadId]
""");
    }

    public override async Task Contains_with_local_nullable_guid_list_closure(bool async)
    {
        await base.Contains_with_local_nullable_guid_list_closure(async);

        AssertSql(
"""
@__ids_0='["d2c26679-562b-44d1-ab96-23d1775e0926","23cbcf9b-ce14-45cf-aafa-2c2667ebfdd3","ab1b82d7-88db-42bd-a132-7eef9aa68af4"]' (Size = 4000)

SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
WHERE EXISTS (
    SELECT 1
    FROM OpenJson(@__ids_0) AS [i]
    WHERE CAST([i].[value] AS uniqueidentifier) = [t].[Id])
""");
    }

    public override async Task Select_Where_Navigation_Null_Reverse(bool async)
    {
        await base.Select_Where_Navigation_Null_Reverse(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE [g].[Nickname] IS NULL OR [g].[SquadId] IS NULL
""");
    }

    public override async Task Join_with_order_by_without_skip_or_take_nested(bool async)
    {
        await base.Join_with_order_by_without_skip_or_take_nested(async);

        AssertSql(
"""
SELECT [w].[Name], [g].[FullName]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
INNER JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [s].[Id] = [g].[SquadId]
INNER JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] = [w].[OwnerFullName]
""");
    }

    public override async Task Concat_anonymous_with_count(bool async)
    {
        await base.Concat_anonymous_with_count(async);

        AssertSql(
"""
SELECT COUNT(*)
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [g].[Nickname] AS [Name]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    UNION ALL
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank], [g0].[FullName] AS [Name]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
) AS [t]
""");
    }

    public override async Task Where_subquery_distinct_singleordefault_boolean_with_pushdown(bool async)
    {
        await base.Where_subquery_distinct_singleordefault_boolean_with_pushdown(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit) AND (
    SELECT TOP(1) [t].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName] AND [w].[Name] LIKE N'%Lancer%'
    ) AS [t]) = CAST(1 AS bit)
ORDER BY [g].[Nickname]
""");
    }

    public override async Task Correlated_collection_with_distinct_projecting_identifier_column_and_correlation_key(bool async)
    {
        await base.Correlated_collection_with_distinct_projecting_identifier_column_and_correlation_key(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [t].[Id], [t].[Name], [t].[OwnerFullName]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT DISTINCT [w].[Id], [w].[Name], [w].[OwnerFullName]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
ORDER BY [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task String_concat_on_various_types(bool async)
    {
        await base.String_concat_on_various_types(async);

        AssertSql(
"""
SELECT N'HasSoulPatch ' + CAST([g].[HasSoulPatch] AS nvarchar(max)) + N' HasSoulPatch' AS [HasSoulPatch], N'Rank ' + CAST([g].[Rank] AS nvarchar(max)) + N' Rank' AS [Rank], N'SquadId ' + CAST([g].[SquadId] AS nvarchar(max)) + N' SquadId' AS [SquadId], N'Rating ' + COALESCE(CAST([m].[Rating] AS nvarchar(max)), N'') + N' Rating' AS [Rating], N'Timeline ' + CAST([m].[Timeline] AS nvarchar(max)) + N' Timeline' AS [Timeline]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
CROSS JOIN [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
ORDER BY [g].[Nickname], [m].[Id]
""");
    }

    public override async Task Correlated_collections_projection_of_collection_thru_navigation(bool async)
    {
        await base.Correlated_collections_projection_of_collection_thru_navigation(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [s].[Id], [t].[SquadId], [t].[MissionId], [t].[PeriodEnd], [t].[PeriodStart]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [g].[SquadId] = [s].[Id]
LEFT JOIN (
    SELECT [s0].[SquadId], [s0].[MissionId], [s0].[PeriodEnd], [s0].[PeriodStart]
    FROM [SquadMissions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s0]
    WHERE [s0].[MissionId] <> 17
) AS [t] ON [s].[Id] = [t].[SquadId]
WHERE [g].[Nickname] <> N'Marcus'
ORDER BY [g].[FullName], [g].[Nickname], [g].[SquadId], [s].[Id], [t].[SquadId]
""");
    }

    public override async Task Where_bitwise_and_integral(bool async)
    {
        await base.Where_bitwise_and_integral(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Rank] & 1 = 1
""",
            //
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE CAST([g].[Rank] AS bigint) & CAST(1 AS bigint) = CAST(1 AS bigint)
""",
            //
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE CAST([g].[Rank] AS smallint) & CAST(1 AS smallint) = CAST(1 AS smallint)
""");
    }

    public override async Task Coalesce_operator_in_projection_with_other_conditions(bool async)
    {
        await base.Coalesce_operator_in_projection_with_other_conditions(async);

        AssertSql(
"""
SELECT CASE
    WHEN [w].[AmmunitionType] = 1 AND [w].[AmmunitionType] IS NOT NULL AND COALESCE([w].[IsAutomatic], CAST(0 AS bit)) = CAST(1 AS bit) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
""");
    }

    public override async Task Correlated_collections_on_left_join_with_predicate(bool async)
    {
        await base.Correlated_collections_on_left_join_with_predicate(async);

        AssertSql(
"""
SELECT [g].[Nickname], [t].[Id], [g].[SquadId], [w].[Name], [w].[Id]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] = [w].[OwnerFullName]
WHERE [g].[HasSoulPatch] = CAST(0 AS bit)
ORDER BY [t].[Id], [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Select_null_propagation_negative4(bool async)
    {
        await base.Select_null_propagation_negative4(async);

        AssertSql(
"""
SELECT CASE
    WHEN [g0].[Nickname] IS NOT NULL AND [g0].[SquadId] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [g0].[Nickname]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[HasSoulPatch] = CAST(1 AS bit)
ORDER BY [g0].[Nickname]
""");
    }

    public override async Task Include_with_join_multi_level(bool async)
    {
        await base.Include_with_join_multi_level(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [c].[Name], [c].[Location], [c].[Nation], [c].[PeriodEnd], [c].[PeriodStart], [t].[Id], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[SquadId] = [t].[GearSquadId] AND [g].[Nickname] = [t].[GearNickName]
INNER JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g].[CityOfBirthName] = [c].[Name]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [c].[Name] = [g0].[AssignedCityName]
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Id], [c].[Name], [g0].[Nickname]
""");
    }

    public override async Task Select_nested_ternary_operations(bool async)
    {
        await base.Select_nested_ternary_operations(async);

        AssertSql(
"""
SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = CAST(0 AS bit) THEN CASE
        WHEN [w].[AmmunitionType] = 1 THEN N'ManualCartridge'
        ELSE N'Manual'
    END
    ELSE N'Auto'
END AS [IsManualCartridge]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
""");
    }

    public override async Task Correlated_collections_with_Skip(bool async)
    {
        await base.Correlated_collections_with_Skip(async);

        AssertSql(
"""
SELECT [s].[Id], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Rank]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
LEFT JOIN (
    SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Rank]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], ROW_NUMBER() OVER(PARTITION BY [g].[SquadId] ORDER BY [g].[Nickname]) AS [row]
        FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    ) AS [t]
    WHERE 1 < [t].[row]
) AS [t0] ON [s].[Id] = [t0].[SquadId]
ORDER BY [s].[Name], [s].[Id], [t0].[SquadId], [t0].[Nickname]
""");
    }

    public override async Task GetValueOrDefault_with_argument(bool async)
    {
        await base.GetValueOrDefault_with_argument(async);

        AssertSql(
"""
SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE COALESCE([w].[SynergyWithId], [w].[Id]) = 1
""");
    }

    public override async Task Include_after_SelectMany_throws(bool async)
    {
        await base.Include_after_SelectMany_throws(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
LEFT JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [f].[CapitalName] = [c].[Name]
INNER JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [c].[Name] = [g].[CityOfBirthName]
INNER JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [g].[SquadId] = [s].[Id]
""");
    }

    public override async Task OrderBy_StartsWith_with_null_parameter_as_argument(bool async)
    {
        await base.OrderBy_StartsWith_with_null_parameter_as_argument(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
ORDER BY [g].[Nickname]
""");
    }

    public override async Task Select_navigation_with_concat_and_count(bool async)
    {
        await base.Select_navigation_with_concat_and_count(async);

        AssertSql(
"""
SELECT (
    SELECT COUNT(*)
    FROM (
        SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
        UNION ALL
        SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[PeriodEnd], [w0].[PeriodStart], [w0].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0]
        WHERE [g].[FullName] = [w0].[OwnerFullName]
    ) AS [t])
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(0 AS bit)
""");
    }

    public override async Task Where_TimeSpan_Minutes(bool async)
    {
        await base.Where_TimeSpan_Minutes(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE DATEPART(minute, [m].[Duration]) = 1
""");
    }

    public override async Task Where_conditional_equality_1(bool async)
    {
        await base.Where_conditional_equality_1(async);

        AssertSql(
"""
SELECT [g].[Nickname]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[LeaderNickname] IS NULL
ORDER BY [g].[Nickname]
""");
    }

    public override async Task GroupJoin_on_entity_qsre_keys_inner_key_is_nested_navigation(bool async)
    {
        await base.GroupJoin_on_entity_qsre_keys_inner_key_is_nested_navigation(async);

        AssertSql(
"""
SELECT [s].[Name] AS [SquadName], [t].[Name] AS [WeaponName]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
LEFT JOIN (
    SELECT [w].[Name], [s0].[Id] AS [Id0]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [w].[OwnerFullName] = [g].[FullName]
    LEFT JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s0] ON [g].[SquadId] = [s0].[Id]
) AS [t] ON [s].[Id] = [t].[Id0]
""");
    }

    public override async Task Null_checks_in_correlated_predicate_are_correctly_translated(bool async)
    {
        await base.Null_checks_in_correlated_predicate_are_correctly_translated(async);

        AssertSql(
"""
SELECT [t].[Id], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId] AND [t].[Note] IS NOT NULL
ORDER BY [t].[Id], [g].[Nickname]
""");
    }

    public override async Task Where_enum(bool async)
    {
        await base.Where_enum(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Rank] = 4
""");
    }

    public override async Task Order_by_entity_qsre_with_inheritance(bool async)
    {
        await base.Order_by_entity_qsre_with_inheritance(async);

        AssertSql(
"""
SELECT [l].[Name]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
INNER JOIN [LocustHighCommands] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0] ON [l].[HighCommandId] = [l0].[Id]
WHERE [l].[Discriminator] = N'LocustCommander'
ORDER BY [l0].[Id], [l].[Name]
""");
    }

    public override async Task Where_has_flag_with_nullable_parameter(bool async)
    {
        await base.Where_has_flag_with_nullable_parameter(async);

        AssertSql(
"""
@__parameter_0='2' (Nullable = true)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Rank] & @__parameter_0 = @__parameter_0
""");
    }

    public override async Task Select_subquery_boolean_empty_with_pushdown(bool async)
    {
        await base.Select_subquery_boolean_empty_with_pushdown(async);

        AssertSql(
"""
SELECT (
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName] AND [w].[Name] = N'BFG'
    ORDER BY [w].[Id])
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Cast_subquery_to_base_type_using_typed_ToList(bool async)
    {
        await base.Cast_subquery_to_base_type_using_typed_ToList(async);

        AssertSql(
"""
SELECT [c].[Name], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Nickname], [g].[Rank], [g].[SquadId]
FROM [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [c].[Name] = [g].[AssignedCityName]
WHERE [c].[Name] = N'Ephyra'
ORDER BY [c].[Name], [g].[Nickname]
""");
    }

    public override async Task Select_conditional_with_anonymous_types(bool async)
    {
        await base.Select_conditional_with_anonymous_types(async);

        AssertSql(
"""
SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [g].[Nickname], [g].[FullName]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
ORDER BY [g].[Nickname]
""");
    }

    public override async Task Group_by_with_having_StartsWith_with_null_parameter_as_argument(bool async)
    {
        await base.Group_by_with_having_StartsWith_with_null_parameter_as_argument(async);

        AssertSql(
"""
SELECT [g].[FullName]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
GROUP BY [g].[FullName]
HAVING 0 = 1
""");
    }

    public override async Task Include_multiple_circular(bool async)
    {
        await base.Include_multiple_circular(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [c].[Name], [c].[Location], [c].[Nation], [c].[PeriodEnd], [c].[PeriodStart], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g].[CityOfBirthName] = [c].[Name]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [c].[Name] = [g0].[AssignedCityName]
ORDER BY [g].[Nickname], [g].[SquadId], [c].[Name], [g0].[Nickname]
""");
    }

    public override async Task Select_correlated_filtered_collection(bool async)
    {
        await base.Select_correlated_filtered_collection(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [c].[Name], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[PeriodEnd], [t].[PeriodStart], [t].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g].[CityOfBirthName] = [c].[Name]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [w].[Name] <> N'Lancer' OR [w].[Name] IS NULL
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
WHERE [c].[Name] IN (N'Ephyra', N'Hanover')
ORDER BY [g].[Nickname], [g].[SquadId], [c].[Name]
""");
    }

    public override async Task String_concat_nullable_expressions_are_coalesced(bool async)
    {
        await base.String_concat_nullable_expressions_are_coalesced(async);

        AssertSql(
"""
SELECT [g].[FullName] + N'' + COALESCE([g].[LeaderNickname], N'') + N''
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Coalesce_used_with_non_unicode_string_column_and_constant(bool async)
    {
        await base.Coalesce_used_with_non_unicode_string_column_and_constant(async);

        AssertSql(
"""
SELECT COALESCE([c].[Location], 'Unknown')
FROM [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c]
""");
    }

    public override async Task Select_null_propagation_optimization8(bool async)
    {
        await base.Select_null_propagation_optimization8(async);

        AssertSql(
"""
SELECT COALESCE([g].[LeaderNickname], N'') + COALESCE([g].[LeaderNickname], N'')
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
""");
    }

    public override async Task Where_subquery_join_firstordefault_boolean(bool async)
    {
        await base.Where_subquery_join_firstordefault_boolean(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit) AND (
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    INNER JOIN (
        SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[PeriodEnd], [w0].[PeriodStart], [w0].[SynergyWithId]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0]
        WHERE [g].[FullName] = [w0].[OwnerFullName]
    ) AS [t] ON [w].[Id] = [t].[Id]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]) = CAST(1 AS bit)
""");
    }

    public override async Task Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection(
        bool async)
    {
        await base.Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [t].[Key], [t].[Count]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
OUTER APPLY (
    SELECT [w].[IsAutomatic] AS [Key], COUNT(*) AS [Count]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]
    GROUP BY [w].[IsAutomatic]
) AS [t]
ORDER BY [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation(
        bool async)
    {
        await base.Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE SUBSTRING([t].[Note], 0 + 1, [g].[SquadId]) = [t].[GearNickName] OR (([t].[Note] IS NULL OR [g].[SquadId] IS NULL) AND [t].[GearNickName] IS NULL)
""");
    }

    public override async Task Project_collection_navigation_with_inheritance1(bool async)
    {
        await base.Project_collection_navigation_with_inheritance1(async);

        AssertSql(
"""
SELECT [f].[Id], [t].[Name], [f0].[Id], [l0].[Name], [l0].[Discriminator], [l0].[LocustHordeId], [l0].[PeriodEnd], [l0].[PeriodStart], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId]
FROM [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
LEFT JOIN (
    SELECT [l].[Name]
    FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    WHERE [l].[Discriminator] = N'LocustCommander'
) AS [t] ON [f].[CommanderName] = [t].[Name]
LEFT JOIN [Factions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f0] ON [t].[Name] = [f0].[CommanderName]
LEFT JOIN [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0] ON [f0].[Id] = [l0].[LocustHordeId]
ORDER BY [f].[Id], [t].[Name], [f0].[Id]
""");
    }

    public override async Task Include_with_join_and_inheritance2(bool async)
    {
        await base.Include_with_join_and_inheritance2(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [t].[Id], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t] ON [g].[SquadId] = [t].[GearSquadId] AND [g].[Nickname] = [t].[GearNickName]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] = [w].[OwnerFullName]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Id]
""");
    }

    public override async Task Include_multiple_one_to_one_and_one_to_many(bool async)
    {
        await base.Include_multiple_one_to_one_and_one_to_many(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [g].[FullName] = [w].[OwnerFullName]
ORDER BY [t].[Id], [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Select_null_propagation_negative5(bool async)
    {
        await base.Select_null_propagation_negative5(async);

        AssertSql(
"""
SELECT CASE
    WHEN [g0].[Nickname] IS NOT NULL AND [g0].[SquadId] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [g0].[Nickname]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[HasSoulPatch] = CAST(1 AS bit)
ORDER BY [g0].[Nickname]
""");
    }

    public override async Task Enum_flags_closure_typed_as_underlying_type_generates_correct_parameter_type(bool async)
    {
        await base.Enum_flags_closure_typed_as_underlying_type_generates_correct_parameter_type(async);

        AssertSql(
"""
@__prm_0='133'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE @__prm_0 & [g].[Rank] = [g].[Rank]
""");
    }

    public override async Task Where_bool_column_and_Contains(bool async)
    {
        await base.Where_bool_column_and_Contains(async);

        AssertSql(
"""
@__values_0='[false,true]' (Size = 4000)

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit) AND EXISTS (
    SELECT 1
    FROM OpenJson(@__values_0) AS [v]
    WHERE CAST([v].[value] AS bit) = [g].[HasSoulPatch])
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_binary_and_expression(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_binary_and_expression(async);

        AssertSql(
"""
SELECT CASE
    WHEN [g].[HasSoulPatch] = CAST(1 AS bit) AND [t].[Note] LIKE N'%Cole%' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
""");
    }

    public override async Task Entity_equality_empty(bool async)
    {
        await base.Entity_equality_empty(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE 0 = 1
""");
    }

    public override async Task Where_equals_method_on_nullable_with_object_overload(bool async)
    {
        await base.Where_equals_method_on_nullable_with_object_overload(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE [m].[Rating] IS NULL
""");
    }

    public override async Task Sum_with_no_data_nullable_double(bool async)
    {
        await base.Sum_with_no_data_nullable_double(async);

        AssertSql(
"""
SELECT COALESCE(SUM([m].[Rating]), 0.0E0)
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE [m].[CodeName] = N'Operation Foobar'
""");
    }

    public override async Task Query_reusing_parameter_with_inner_query_expression_doesnt_declare_duplicate_parameter(bool async)
    {
        await base.Query_reusing_parameter_with_inner_query_expression_doesnt_declare_duplicate_parameter(async);

        AssertSql(
"""
@__gearId_0='1'

SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
WHERE EXISTS (
    SELECT 1
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [s].[Id] = [g].[SquadId] AND [g].[SquadId] = @__gearId_0 AND [g].[SquadId] = @__gearId_0)
""");
    }

    public override async Task Join_navigation_translated_to_subquery_composite_key(bool async)
    {
        await base.Join_navigation_translated_to_subquery_composite_key(async);

        AssertSql(
"""
SELECT [g].[FullName], [t0].[Note]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN (
    SELECT [t].[Note], [g0].[FullName]
    FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
    LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [t].[GearNickName] = [g0].[Nickname] AND [t].[GearSquadId] = [g0].[SquadId]
) AS [t0] ON [g].[FullName] = [t0].[FullName]
""");
    }

    public override async Task Correlated_collections_naked_navigation_with_ToList_followed_by_projecting_count(bool async)
    {
        await base.Correlated_collections_naked_navigation_with_ToList_followed_by_projecting_count(async);

        AssertSql(
"""
SELECT (
    SELECT COUNT(*)
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName])
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Nickname] <> N'Marcus'
ORDER BY [g].[Nickname]
""");
    }

    public override async Task Select_null_propagation_negative2(bool async)
    {
        await base.Select_null_propagation_negative2(async);

        AssertSql(
"""
SELECT CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN [g0].[LeaderNickname]
    ELSE NULL
END
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
CROSS JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
""");
    }

    public override async Task ToString_string_property_projection(bool async)
    {
        await base.ToString_string_property_projection(async);

        AssertSql(
"""
SELECT [w].[Name]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
""");
    }

    public override async Task ToString_boolean_property_non_nullable(bool async)
    {
        await base.ToString_boolean_property_non_nullable(async);

        AssertSql(
"""
SELECT CASE
    WHEN [w].[IsAutomatic] = CAST(0 AS bit) THEN N'False'
    ELSE N'True'
END
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
""");
    }

    public override async Task Correlated_collections_basic_projection_ordered(bool async)
    {
        await base.Correlated_collections_basic_projection_ordered(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [t].[Id], [t].[AmmunitionType], [t].[IsAutomatic], [t].[Name], [t].[OwnerFullName], [t].[PeriodEnd], [t].[PeriodStart], [t].[SynergyWithId]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit) OR [w].[Name] <> N'foo' OR [w].[Name] IS NULL
) AS [t] ON [g].[FullName] = [t].[OwnerFullName]
WHERE [g].[Nickname] <> N'Marcus'
ORDER BY [g].[Nickname], [g].[SquadId], [t].[Name] DESC
""");
    }

    public override async Task Include_with_join_and_inheritance_with_orderby_before_and_after_include(bool async)
    {
        await base.Include_with_join_and_inheritance_with_orderby_before_and_after_include(async);

        AssertSql(
"""
SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Rank], [t].[Id], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
INNER JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [g].[Discriminator] = N'Officer'
) AS [t0] ON [t].[GearSquadId] = [t0].[SquadId] AND [t].[GearNickName] = [t0].[Nickname]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [t0].[Nickname] = [g0].[LeaderNickname] AND [t0].[SquadId] = [g0].[LeaderSquadId]
ORDER BY [t0].[HasSoulPatch], [t0].[Nickname] DESC, [t].[Id], [t0].[SquadId], [g0].[Nickname]
""");
    }

    public override async Task Select_enum_has_flag(bool async)
    {
        await base.Select_enum_has_flag(async);

        AssertSql(
"""
SELECT TOP(1) CASE
    WHEN [g].[Rank] & 2 = 2 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [hasFlagTrue], CASE
    WHEN [g].[Rank] & 4 = 4 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [hasFlagFalse]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Rank] & 2 = 2
""");
    }

    public override async Task Include_multiple_one_to_one_optional_and_one_to_one_required(bool async)
    {
        await base.Include_multiple_one_to_one_optional_and_one_to_one_required(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t].[PeriodEnd], [t].[PeriodStart], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
LEFT JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [g].[SquadId] = [s].[Id]
""");
    }

    public override async Task Projecting_property_converted_to_nullable_into_unary(bool async)
    {
        await base.Projecting_property_converted_to_nullable_into_unary(async);

        AssertSql(
"""
SELECT [t].[Note]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [t].[GearNickName] = [g].[Nickname] AND [t].[GearSquadId] = [g].[SquadId]
WHERE CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[Nickname]
    ELSE NULL
END IS NOT NULL AND CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [g].[HasSoulPatch]
    ELSE NULL
END = CAST(0 AS bit)
ORDER BY [t].[Note]
""");
    }

    public override async Task Correlated_collections_left_join_with_self_reference(bool async)
    {
        await base.Correlated_collections_left_join_with_self_reference(async);

        AssertSql(
"""
SELECT [t].[Note], [t].[Id], [t0].[Nickname], [t0].[SquadId], [g0].[FullName], [g0].[Nickname], [g0].[SquadId]
FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [g].[Discriminator] = N'Officer'
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON ([t0].[Nickname] = [g0].[LeaderNickname] OR ([t0].[Nickname] IS NULL AND [g0].[LeaderNickname] IS NULL)) AND [t0].[SquadId] = [g0].[LeaderSquadId]
ORDER BY [t].[Id], [t0].[Nickname], [t0].[SquadId], [g0].[Nickname]
""");
    }

    public override async Task Where_bitwise_or_enum(bool async)
    {
        await base.Where_bitwise_or_enum(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[Rank] | 2 > 0
""");
    }

    public override async Task Correlated_collection_with_very_complex_order_by(bool async)
    {
        await base.Correlated_collection_with_very_complex_order_by(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[Discriminator], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN (
    SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOfBirthName], [g1].[Discriminator], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[PeriodEnd], [g1].[PeriodStart], [g1].[Rank]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g1]
    WHERE [g1].[HasSoulPatch] = CAST(0 AS bit)
) AS [t] ON [g].[Nickname] = [t].[LeaderNickname] AND [g].[SquadId] = [t].[LeaderSquadId]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY (
    SELECT COUNT(*)
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName] AND [w].[IsAutomatic] = COALESCE((
        SELECT TOP(1) [g0].[HasSoulPatch]
        FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
        WHERE [g0].[Nickname] = N'Marcus'), CAST(0 AS bit))), [g].[Nickname], [g].[SquadId], [t].[Nickname]
""");
    }

    public override async Task Correlated_collection_with_distinct_projecting_identifier_column_composite_key(bool async)
    {
        await base.Correlated_collection_with_distinct_projecting_identifier_column_composite_key(async);

        AssertSql(
"""
SELECT [s].[Id], [t].[Nickname], [t].[SquadId], [t].[HasSoulPatch]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
LEFT JOIN (
    SELECT DISTINCT [g].[Nickname], [g].[SquadId], [g].[HasSoulPatch]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
) AS [t] ON [s].[Id] = [t].[SquadId]
ORDER BY [s].[Id], [t].[Nickname]
""");
    }

    public override async Task Null_propagation_optimization3(bool async)
    {
        await base.Null_propagation_optimization3(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE CASE
    WHEN [g].[LeaderNickname] IS NOT NULL THEN CASE
        WHEN [g].[LeaderNickname] LIKE N'%us' THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    ELSE NULL
END = CAST(1 AS bit)
""");
    }

    public override async Task Byte_array_filter_by_length_parameter(bool async)
    {
        await base.Byte_array_filter_by_length_parameter(async);

        AssertSql(
"""
@__p_0='1'

SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
WHERE CAST(DATALENGTH([s].[Banner]) AS int) = @__p_0
""");
    }

    public override async Task Select_ternary_operation_multiple_conditions_2(bool async)
    {
        await base.Select_ternary_operation_multiple_conditions_2(async);

        AssertSql(
"""
SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = CAST(0 AS bit) AND [w].[SynergyWithId] = 1 THEN N'Yes'
    ELSE N'No'
END AS [IsCartridge]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
""");
    }

    public override async Task Double_order_by_on_nullable_bool_coming_from_optional_navigation(bool async)
    {
        await base.Double_order_by_on_nullable_bool_coming_from_optional_navigation(async);

        AssertSql(
"""
SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[PeriodEnd], [w0].[PeriodStart], [w0].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY [w0].[IsAutomatic], [w0].[Id]
""");
    }

    public override async Task Where_bitwise_and_nullable_enum_with_nullable_parameter(bool async)
    {
        await base.Where_bitwise_and_nullable_enum_with_nullable_parameter(async);

        AssertSql(
"""
@__ammunitionType_0='1' (Nullable = true)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE [w].[AmmunitionType] & @__ammunitionType_0 > 0
""",
            //
"""
SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
WHERE [w].[AmmunitionType] & NULL > 0
""");
    }

    public override async Task Project_one_value_type_converted_to_nullable_from_empty_collection(bool async)
    {
        await base.Project_one_value_type_converted_to_nullable_from_empty_collection(async);

        AssertSql(
"""
SELECT [s].[Name], (
    SELECT TOP(1) [g].[SquadId]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [s].[Id] = [g].[SquadId] AND [g].[HasSoulPatch] = CAST(1 AS bit)) AS [SquadId]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
WHERE [s].[Name] = N'Kilo'
""");
    }

    public override async Task Unnecessary_include_doesnt_get_added_complex_when_projecting_EF_Property(bool async)
    {
        await base.Unnecessary_include_doesnt_get_added_complex_when_projecting_EF_Property(async);

        AssertSql(
"""
SELECT [g].[FullName]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE [g].[HasSoulPatch] = CAST(1 AS bit)
ORDER BY [g].[Rank]
""");
    }

    public override async Task Complex_GroupBy_after_set_operator_using_result_selector(bool async)
    {
        await base.Complex_GroupBy_after_set_operator_using_result_selector(async);

        AssertSql(
"""
SELECT [t].[Name], [t].[Count], COALESCE(SUM([t].[Count]), 0) AS [Sum]
FROM (
    SELECT [c].[Name], (
        SELECT COUNT(*)
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]) AS [Count]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    LEFT JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g].[AssignedCityName] = [c].[Name]
    UNION ALL
    SELECT [c0].[Name], (
        SELECT COUNT(*)
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w0]
        WHERE [g0].[FullName] = [w0].[OwnerFullName]) AS [Count]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
    INNER JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c0] ON [g0].[CityOfBirthName] = [c0].[Name]
) AS [t]
GROUP BY [t].[Name], [t].[Count]
""");
    }

    public override async Task Correlated_collection_with_top_level_Last_with_orderby_on_outer(bool async)
    {
        await base.Correlated_collection_with_top_level_Last_with_orderby_on_outer(async);

        AssertSql(
"""
SELECT [t].[Nickname], [t].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[PeriodEnd], [w].[PeriodStart], [w].[SynergyWithId]
FROM (
    SELECT TOP(1) [g].[Nickname], [g].[SquadId], [g].[FullName]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    ORDER BY [g].[FullName]
) AS [t]
LEFT JOIN [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w] ON [t].[FullName] = [w].[OwnerFullName]
ORDER BY [t].[FullName], [t].[Nickname], [t].[SquadId]
""");
    }

    public override async Task DateTimeOffset_Date_returns_datetime(bool async)
    {
        await base.DateTimeOffset_Date_returns_datetime(async);

        AssertSql(
"""
@__dateTimeOffset_Date_0='0002-03-01T00:00:00.0000000'

SELECT [m].[Id], [m].[BriefingDocument], [m].[BriefingDocumentFileExtension], [m].[CodeName], [m].[Date], [m].[Duration], [m].[PeriodEnd], [m].[PeriodStart], [m].[Rating], [m].[Time], [m].[Timeline]
FROM [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m]
WHERE CONVERT(date, [m].[Timeline]) >= @__dateTimeOffset_Date_0
""");
    }

    public override async Task Select_subquery_projecting_single_constant_int(bool async)
    {
        await base.Select_subquery_projecting_single_constant_int(async);

        AssertSql(
"""
SELECT [s].[Name], COALESCE((
    SELECT TOP(1) 42
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [s].[Id] = [g].[SquadId] AND [g].[HasSoulPatch] = CAST(1 AS bit)), 0) AS [Gear]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
""");
    }

    public override async Task FirstOrDefault_over_int_compared_to_zero(bool async)
    {
        await base.FirstOrDefault_over_int_compared_to_zero(async);

        AssertSql(
"""
SELECT [s].[Name]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
WHERE [s].[Name] = N'Kilo' AND COALESCE((
    SELECT TOP(1) [g].[SquadId]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [s].[Id] = [g].[SquadId] AND [g].[HasSoulPatch] = CAST(1 AS bit)), 0) <> 0
""");
    }

    public override async Task
        Correlated_collection_with_groupby_with_complex_grouping_key_not_projecting_identifier_column_with_group_aggregate_in_final_projection(
            bool async)
    {
        await base
            .Correlated_collection_with_groupby_with_complex_grouping_key_not_projecting_identifier_column_with_group_aggregate_in_final_projection(
                async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [t0].[Key], [t0].[Count]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
OUTER APPLY (
    SELECT [t].[Key], COUNT(*) AS [Count]
    FROM (
        SELECT CAST(LEN([w].[Name]) AS int) AS [Key]
        FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
        WHERE [g].[FullName] = [w].[OwnerFullName]
    ) AS [t]
    GROUP BY [t].[Key]
) AS [t0]
ORDER BY [g].[Nickname], [g].[SquadId]
""");
    }

    public override async Task Correlated_collection_with_distinct_not_projecting_identifier_column_also_projecting_complex_expressions(
        bool async)
    {
        await base.Correlated_collection_with_distinct_not_projecting_identifier_column_also_projecting_complex_expressions(async);

        AssertSql();
    }

    public override async Task Client_eval_followed_by_aggregate_operation(bool async)
    {
        await base.Client_eval_followed_by_aggregate_operation(async);

        AssertSql();
    }

    public override async Task Client_member_and_unsupported_string_Equals_in_the_same_query(bool async)
    {
        await base.Client_member_and_unsupported_string_Equals_in_the_same_query(async);

        AssertSql();
    }

    public override async Task Client_side_equality_with_parameter_works_with_optional_navigations(bool async)
    {
        await base.Client_side_equality_with_parameter_works_with_optional_navigations(async);

        AssertSql();
    }

    public override async Task Correlated_collection_order_by_constant_null_of_non_mapped_type(bool async)
    {
        await base.Correlated_collection_order_by_constant_null_of_non_mapped_type(async);

        AssertSql();
    }

    public override async Task GetValueOrDefault_on_DateTimeOffset(bool async)
    {
        await base.GetValueOrDefault_on_DateTimeOffset(async);

        AssertSql();
    }

    public override async Task Where_coalesce_with_anonymous_types(bool async)
    {
        await base.Where_coalesce_with_anonymous_types(async);

        AssertSql();
    }

    public override async Task Projecting_correlated_collection_followed_by_Distinct(bool async)
    {
        await base.Projecting_correlated_collection_followed_by_Distinct(async);

        AssertSql();
    }

    public override async Task Projecting_some_properties_as_well_as_correlated_collection_followed_by_Distinct(bool async)
    {
        await base.Projecting_some_properties_as_well_as_correlated_collection_followed_by_Distinct(async);

        AssertSql();
    }

    public override async Task Projecting_entity_as_well_as_correlated_collection_followed_by_Distinct(bool async)
    {
        await base.Projecting_entity_as_well_as_correlated_collection_followed_by_Distinct(async);

        AssertSql();
    }

    public override async Task Projecting_entity_as_well_as_complex_correlated_collection_followed_by_Distinct(bool async)
    {
        await base.Projecting_entity_as_well_as_complex_correlated_collection_followed_by_Distinct(async);

        AssertSql();
    }

    public override async Task Projecting_entity_as_well_as_correlated_collection_of_scalars_followed_by_Distinct(bool async)
    {
        await base.Projecting_entity_as_well_as_correlated_collection_of_scalars_followed_by_Distinct(async);

        AssertSql();
    }

    public override async Task Correlated_collection_with_distinct_3_levels(bool async)
    {
        await base.Correlated_collection_with_distinct_3_levels(async);

        AssertSql();
    }

    public override async Task Where_compare_anonymous_types(bool async)
    {
        await base.Where_compare_anonymous_types(async);

        AssertSql();
    }

    public override async Task Correlated_collection_after_distinct_3_levels_without_original_identifiers(bool async)
    {
        await base.Correlated_collection_after_distinct_3_levels_without_original_identifiers(async);

        AssertSql();
    }

    public override async Task Checked_context_throws_on_client_evaluation(bool async)
    {
        await base.Checked_context_throws_on_client_evaluation(async);

        AssertSql();
    }

    public override async Task Trying_to_access_unmapped_property_throws_informative_error(bool async)
    {
        await base.Trying_to_access_unmapped_property_throws_informative_error(async);

        AssertSql();
    }

    public override async Task Trying_to_access_unmapped_property_inside_aggregate(bool async)
    {
        await base.Trying_to_access_unmapped_property_inside_aggregate(async);

        AssertSql();
    }

    public override async Task Trying_to_access_unmapped_property_inside_subquery(bool async)
    {
        await base.Trying_to_access_unmapped_property_inside_subquery(async);

        AssertSql();
    }

    public override async Task Trying_to_access_unmapped_property_inside_join_key_selector(bool async)
    {
        await base.Trying_to_access_unmapped_property_inside_join_key_selector(async);

        AssertSql();
    }

    public override async Task Client_projection_with_nested_unmapped_property_bubbles_up_translation_failure_info(bool async)
    {
        await base.Client_projection_with_nested_unmapped_property_bubbles_up_translation_failure_info(async);

        AssertSql();
    }

    public override async Task Nav_rewrite_Distinct_with_convert()
    {
        await base.Nav_rewrite_Distinct_with_convert();

        AssertSql();
    }

    public override async Task Nav_rewrite_Distinct_with_convert_anonymous()
    {
        await base.Nav_rewrite_Distinct_with_convert_anonymous();

        AssertSql();
    }

    public override async Task Include_after_select_with_cast_throws(bool async)
    {
        await base.Include_after_select_with_cast_throws(async);

        AssertSql();
    }

    public override async Task Include_after_select_with_entity_projection_throws(bool async)
    {
        await base.Include_after_select_with_entity_projection_throws(async);

        AssertSql();
    }

    public override async Task Include_after_select_anonymous_projection_throws(bool async)
    {
        await base.Include_after_select_anonymous_projection_throws(async);

        AssertSql();
    }

    public override async Task Group_by_with_aggregate_max_on_entity_type(bool async)
    {
        await base.Group_by_with_aggregate_max_on_entity_type(async);

        AssertSql();
    }

    public override async Task Include_collection_and_invalid_navigation_using_string_throws(bool async)
    {
        await base.Include_collection_and_invalid_navigation_using_string_throws(async);

        AssertSql();
    }

    public override async Task Include_with_concat(bool async)
    {
        await base.Include_with_concat(async);

        AssertSql();
    }

    public override async Task Join_with_complex_key_selector(bool async)
    {
        await base.Join_with_complex_key_selector(async);

        AssertSql(
"""
SELECT [s].[Id], [t0].[Id] AS [TagId]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
CROSS JOIN (
    SELECT [t].[Id]
    FROM [Tags] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [t]
    WHERE [t].[Note] = N'Marcus'' Tag'
) AS [t0]
""");
    }

    public override async Task Streaming_correlated_collection_issue_11403_returning_ordered_enumerable_throws(bool async)
    {
        await base.Streaming_correlated_collection_issue_11403_returning_ordered_enumerable_throws(async);

        AssertSql();
    }

    public override async Task Optional_navigation_type_compensation_works_with_skip(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_skip(async);

        AssertSql();
    }

    public override async Task Optional_navigation_type_compensation_works_with_take(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_take(async);

        AssertSql();
    }

    public override async Task Select_correlated_filtered_collection_returning_queryable_throws(bool async)
    {
        await base.Select_correlated_filtered_collection_returning_queryable_throws(async);

        AssertSql();
    }

    public override async Task Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used(
        bool async)
    {
        await base.Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used(async);

        AssertSql();
    }

    public override async Task Client_method_on_collection_navigation_in_predicate(bool async)
    {
        await base.Client_method_on_collection_navigation_in_predicate(async);

        AssertSql();
    }

    public override async Task Client_method_on_collection_navigation_in_predicate_accessed_by_ef_property(bool async)
    {
        await base.Client_method_on_collection_navigation_in_predicate_accessed_by_ef_property(async);

        AssertSql();
    }

    public override async Task Client_method_on_collection_navigation_in_order_by(bool async)
    {
        await base.Client_method_on_collection_navigation_in_order_by(async);

        AssertSql();
    }

    public override async Task Client_method_on_collection_navigation_in_additional_from_clause(bool async)
    {
        await base.Client_method_on_collection_navigation_in_additional_from_clause(async);

        AssertSql();
    }

    public override async Task Client_method_on_collection_navigation_in_outer_join_key(bool async)
    {
        await base.Client_method_on_collection_navigation_in_outer_join_key(async);

        AssertSql();
    }

    public override async Task Include_multiple_one_to_one_and_one_to_many_self_reference(bool async)
    {
        await base.Include_multiple_one_to_one_and_one_to_many_self_reference(async);

        AssertSql();
    }

    public override async Task Include_multiple_one_to_one_and_one_to_one_and_one_to_many(bool async)
    {
        await base.Include_multiple_one_to_one_and_one_to_one_and_one_to_many(async);

        AssertSql();
    }

    public override async Task Include_multiple_include_then_include(bool async)
    {
        await base.Include_multiple_include_then_include(async);

        AssertSql();
    }

    public override async Task Select_Where_Navigation_Client(bool async)
    {
        await base.Select_Where_Navigation_Client(async);

        AssertSql();
    }

    public override async Task Where_subquery_equality_to_null_with_composite_key(bool async)
    {
        await base.Where_subquery_equality_to_null_with_composite_key(async);

        AssertSql(
"""
SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
WHERE NOT (EXISTS (
    SELECT 1
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [s].[Id] = [g].[SquadId]))
""");
    }

    public override async Task Where_subquery_equality_to_null_without_composite_key(bool async)
    {
        await base.Where_subquery_equality_to_null_without_composite_key(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
WHERE NOT (EXISTS (
    SELECT 1
    FROM [Weapons] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [w]
    WHERE [g].[FullName] = [w].[OwnerFullName]))
""");
    }

    public override async Task Include_reference_on_derived_type_using_EF_Property(bool async)
    {
        await base.Include_reference_on_derived_type_using_EF_Property(async);

        AssertSql(
"""
SELECT [l].[Name], [l].[Discriminator], [l].[LocustHordeId], [l].[PeriodEnd], [l].[PeriodStart], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], [l].[DefeatedByNickname], [l].[DefeatedBySquadId], [l].[HighCommandId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [LocustLeaders] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g] ON [l].[DefeatedByNickname] = [g].[Nickname] AND [l].[DefeatedBySquadId] = [g].[SquadId]
""");
    }

    public override async Task Include_collection_on_derived_type_using_EF_Property(bool async)
    {
        await base.Include_collection_on_derived_type_using_EF_Property(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[Nickname] = [g0].[LeaderNickname] AND [g].[SquadId] = [g0].[LeaderSquadId]
ORDER BY [g].[Nickname], [g].[SquadId], [g0].[Nickname]
""");
    }

    public override async Task EF_Property_based_Include_navigation_on_derived_type(bool async)
    {
        await base.EF_Property_based_Include_navigation_on_derived_type(async);

        AssertSql(
"""
SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
LEFT JOIN [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0] ON [g].[Nickname] = [g0].[LeaderNickname] AND [g].[SquadId] = [g0].[LeaderSquadId]
WHERE [g].[Discriminator] = N'Officer'
ORDER BY [g].[Nickname], [g].[SquadId], [g0].[Nickname]
""");
    }

    // Sequence contains no elements due to temporal filter
    public override Task ElementAt_basic_with_OrderBy(bool async)
        => Task.CompletedTask;

    public override async Task ElementAtOrDefault_basic_with_OrderBy(bool async)
    {
        await base.ElementAtOrDefault_basic_with_OrderBy(async);

        AssertSql(
"""
@__p_0='1'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
ORDER BY [g].[FullName]
OFFSET @__p_0 ROWS FETCH NEXT 1 ROWS ONLY
""");
    }

    public override async Task ElementAtOrDefault_basic_with_OrderBy_parameter(bool async)
    {
        await base.ElementAtOrDefault_basic_with_OrderBy_parameter(async);

        AssertSql(
"""
@__p_0='2'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
ORDER BY [g].[FullName]
OFFSET @__p_0 ROWS FETCH NEXT 1 ROWS ONLY
""");
    }

    public override async Task Where_subquery_with_ElementAtOrDefault_equality_to_null_with_composite_key(bool async)
    {
        await base.Where_subquery_with_ElementAtOrDefault_equality_to_null_with_composite_key(async);

        AssertSql(
"""
SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
WHERE NOT (EXISTS (
    SELECT 1
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [s].[Id] = [g].[SquadId]
    ORDER BY [g].[Nickname]
    OFFSET 2 ROWS))
""");
    }

    public override async Task Where_subquery_with_ElementAt_using_column_as_index(bool async)
    {
        await base.Where_subquery_with_ElementAt_using_column_as_index(async);

        AssertSql(
"""
SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
WHERE (
    SELECT [g].[Nickname]
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
    WHERE [s].[Id] = [g].[SquadId]
    ORDER BY [g].[Nickname]
    OFFSET [s].[Id] ROWS FETCH NEXT 1 ROWS ONLY) = N'Cole Train'
""");
    }

    public override async Task Using_indexer_on_byte_array_and_string_in_projection(bool async)
    {
        await base.Using_indexer_on_byte_array_and_string_in_projection(async);

        AssertSql(
"""
SELECT [s].[Id], CAST(SUBSTRING([s].[Banner], 0 + 1, 1) AS tinyint), [s].[Name]
FROM [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
""");
    }

    public override async Task DateTimeOffset_to_unix_time_milliseconds(bool async)
    {
        await base.DateTimeOffset_to_unix_time_milliseconds(async);

        AssertSql(
"""
@__unixEpochMilliseconds_0='0'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart], [s1].[SquadId], [s1].[MissionId], [s1].[PeriodEnd], [s1].[PeriodStart]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [g].[SquadId] = [s].[Id]
LEFT JOIN [SquadMissions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s1] ON [s].[Id] = [s1].[SquadId]
WHERE NOT (EXISTS (
    SELECT 1
    FROM [SquadMissions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s0]
    INNER JOIN [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m] ON [s0].[MissionId] = [m].[Id]
    WHERE [s].[Id] = [s0].[SquadId] AND @__unixEpochMilliseconds_0 = DATEDIFF_BIG(millisecond, '1970-01-01T00:00:00.0000000+00:00', [m].[Timeline])))
ORDER BY [g].[Nickname], [g].[SquadId], [s].[Id], [s1].[SquadId]
""");
    }

    public override async Task DateTimeOffset_to_unix_time_seconds(bool async)
    {
        await base.DateTimeOffset_to_unix_time_seconds(async);

        AssertSql(
"""
@__unixEpochSeconds_0='0'

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[Discriminator], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[PeriodEnd], [g].[PeriodStart], [g].[Rank], [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart], [s1].[SquadId], [s1].[MissionId], [s1].[PeriodEnd], [s1].[PeriodStart]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [g].[SquadId] = [s].[Id]
LEFT JOIN [SquadMissions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s1] ON [s].[Id] = [s1].[SquadId]
WHERE NOT (EXISTS (
    SELECT 1
    FROM [SquadMissions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s0]
    INNER JOIN [Missions] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m] ON [s0].[MissionId] = [m].[Id]
    WHERE [s].[Id] = [s0].[SquadId] AND @__unixEpochSeconds_0 = DATEDIFF_BIG(second, '1970-01-01T00:00:00.0000000+00:00', [m].[Timeline])))
ORDER BY [g].[Nickname], [g].[SquadId], [s].[Id], [s1].[SquadId]
""");
    }

    public override async Task Set_operator_with_navigation_in_projection_groupby_aggregate(bool async)
    {
        await base.Set_operator_with_navigation_in_projection_groupby_aggregate(async);

        AssertSql(
"""
SELECT [s].[Name], (
    SELECT COALESCE(SUM(CAST(LEN([c].[Location]) AS int)), 0)
    FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g2]
    INNER JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s0] ON [g2].[SquadId] = [s0].[Id]
    INNER JOIN [Cities] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [c] ON [g2].[CityOfBirthName] = [c].[Name]
    WHERE EXISTS (
        SELECT 1
        FROM (
            SELECT [g3].[Nickname], [g3].[SquadId], [g3].[AssignedCityName], [g3].[CityOfBirthName], [g3].[Discriminator], [g3].[FullName], [g3].[HasSoulPatch], [g3].[LeaderNickname], [g3].[LeaderSquadId], [g3].[PeriodEnd], [g3].[PeriodStart], [g3].[Rank]
            FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g3]
            UNION ALL
            SELECT [g4].[Nickname], [g4].[SquadId], [g4].[AssignedCityName], [g4].[CityOfBirthName], [g4].[Discriminator], [g4].[FullName], [g4].[HasSoulPatch], [g4].[LeaderNickname], [g4].[LeaderSquadId], [g4].[PeriodEnd], [g4].[PeriodStart], [g4].[Rank]
            FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g4]
        ) AS [t0]
        WHERE [t0].[Nickname] = N'Marcus') AND ([s].[Name] = [s0].[Name] OR ([s].[Name] IS NULL AND [s0].[Name] IS NULL))) AS [SumOfLengths]
FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g]
INNER JOIN [Squads] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [g].[SquadId] = [s].[Id]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[Discriminator], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[PeriodEnd], [g0].[PeriodStart], [g0].[Rank]
        FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g0]
        UNION ALL
        SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOfBirthName], [g1].[Discriminator], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[PeriodEnd], [g1].[PeriodStart], [g1].[Rank]
        FROM [Gears] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [g1]
    ) AS [t]
    WHERE [t].[Nickname] = N'Marcus')
GROUP BY [s].[Name]
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
