// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

namespace Microsoft.EntityFrameworkCore.Query;

public class TPCGearsOfWarQuerySqlServerTest : TPCGearsOfWarQueryRelationalTestBase<TPCGearsOfWarQuerySqlServerFixture>
{
#pragma warning disable IDE0060 // Remove unused parameter
    public TPCGearsOfWarQuerySqlServerTest(TPCGearsOfWarQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
#pragma warning restore IDE0060 // Remove unused parameter
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override bool CanExecuteQueryString
        => true;

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Negate_on_binary_expression(bool async)
    {
        await base.Negate_on_binary_expression(async);

        AssertSql(
            @"SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE [s].[Id] = -([s].[Id] + [s].[Id])");
    }

    public override async Task Negate_on_column(bool async)
    {
        await base.Negate_on_column(async);

        AssertSql(
            @"SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE [s].[Id] = -[s].[Id]");
    }

    public override async Task Double_negate_on_column(bool async)
    {
        await base.Double_negate_on_column(async);

        AssertSql(
            @"SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE -(-[s].[Id]) = [s].[Id]");
    }

    public override async Task Negate_on_like_expression(bool async)
    {
        await base.Negate_on_like_expression(async);

        AssertSql(
            @"SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE [s].[Name] IS NOT NULL AND NOT ([s].[Name] LIKE N'us%')");
    }

    public override async Task Entity_equality_empty(bool async)
    {
        await base.Entity_equality_empty(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE 0 = 1");
    }

    public override async Task Include_multiple_one_to_one_and_one_to_many(bool async)
    {
        await base.Include_multiple_one_to_one_and_one_to_many(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
LEFT JOIN [Weapons] AS [w] ON [t0].[FullName] = [w].[OwnerFullName]
ORDER BY [t].[Id], [t0].[Nickname], [t0].[SquadId]");
    }

    public override async Task Include_multiple_one_to_one_optional_and_one_to_one_required(bool async)
    {
        await base.Include_multiple_one_to_one_optional_and_one_to_one_required(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator], [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
LEFT JOIN [Squads] AS [s] ON [t0].[SquadId] = [s].[Id]");
    }

    public override async Task Include_multiple_circular(bool async)
    {
        await base.Include_multiple_circular(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [c].[Name], [c].[Location], [c].[Nation], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN [Cities] AS [c] ON [t].[CityOfBirthName] = [c].[Name]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON [c].[Name] = [t0].[AssignedCityName]
ORDER BY [t].[Nickname], [t].[SquadId], [c].[Name], [t0].[Nickname]");
    }

    public override async Task Include_multiple_circular_with_filter(bool async)
    {
        await base.Include_multiple_circular_with_filter(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [c].[Name], [c].[Location], [c].[Nation], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN [Cities] AS [c] ON [t].[CityOfBirthName] = [c].[Name]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON [c].[Name] = [t0].[AssignedCityName]
WHERE [t].[Nickname] = N'Marcus'
ORDER BY [t].[Nickname], [t].[SquadId], [c].[Name], [t0].[Nickname]");
    }

    public override async Task Include_using_alternate_key(bool async)
    {
        await base.Include_using_alternate_key(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Weapons] AS [w] ON [t].[FullName] = [w].[OwnerFullName]
WHERE [t].[Nickname] = N'Marcus'
ORDER BY [t].[Nickname], [t].[SquadId]");
    }

    public override async Task Include_navigation_on_derived_type(bool async)
    {
        await base.Include_navigation_on_derived_type(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON [t].[Nickname] = [t0].[LeaderNickname] AND [t].[SquadId] = [t0].[LeaderSquadId]
WHERE [t].[Discriminator] = N'Officer'
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Nickname]");
    }

    public override async Task String_based_Include_navigation_on_derived_type(bool async)
    {
        await base.String_based_Include_navigation_on_derived_type(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON [t].[Nickname] = [t0].[LeaderNickname] AND [t].[SquadId] = [t0].[LeaderSquadId]
WHERE [t].[Discriminator] = N'Officer'
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Nickname]");
    }

    public override async Task Select_Where_Navigation_Included(bool async)
    {
        await base.Select_Where_Navigation_Included(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE [t0].[Nickname] = N'Marcus'");
    }

    public override async Task Include_with_join_reference1(bool async)
    {
        await base.Include_with_join_reference1(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [c].[Name], [c].[Location], [c].[Nation]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN [Tags] AS [t0] ON [t].[SquadId] = [t0].[GearSquadId] AND [t].[Nickname] = [t0].[GearNickName]
INNER JOIN [Cities] AS [c] ON [t].[CityOfBirthName] = [c].[Name]");
    }

    public override async Task Include_with_join_reference2(bool async)
    {
        await base.Include_with_join_reference2(async);

        AssertSql(
            @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator], [c].[Name], [c].[Location], [c].[Nation]
FROM [Tags] AS [t]
INNER JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearSquadId] = [t0].[SquadId] AND [t].[GearNickName] = [t0].[Nickname]
INNER JOIN [Cities] AS [c] ON [t0].[CityOfBirthName] = [c].[Name]");
    }

    public override async Task Include_with_join_collection1(bool async)
    {
        await base.Include_with_join_collection1(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Id], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN [Tags] AS [t0] ON [t].[SquadId] = [t0].[GearSquadId] AND [t].[Nickname] = [t0].[GearNickName]
LEFT JOIN [Weapons] AS [w] ON [t].[FullName] = [w].[OwnerFullName]
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Id]");
    }

    public override async Task Include_with_join_collection2(bool async)
    {
        await base.Include_with_join_collection2(async);

        AssertSql(
            @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator], [t].[Id], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Tags] AS [t]
INNER JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearSquadId] = [t0].[SquadId] AND [t].[GearNickName] = [t0].[Nickname]
LEFT JOIN [Weapons] AS [w] ON [t0].[FullName] = [w].[OwnerFullName]
ORDER BY [t].[Id], [t0].[Nickname], [t0].[SquadId]");
    }

    public override async Task Include_where_list_contains_navigation(bool async)
    {
        await base.Include_where_list_contains_navigation(async);

        AssertSql(
            @"SELECT [t].[Id]
FROM [Tags] AS [t]",
            //
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Id], [t0].[GearNickName], [t0].[GearSquadId], [t0].[IssueDate], [t0].[Note]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName] AND [t].[SquadId] = [t0].[GearSquadId]
WHERE [t0].[Id] IS NOT NULL AND [t0].[Id] IN ('34c8d86e-a4ac-4be5-827f-584dda348a07', 'df36f493-463f-4123-83f9-6b135deeb7ba', 'a8ad98f9-e023-4e2a-9a70-c2728455bd34', '70534e05-782c-4052-8720-c2c54481ce5f', 'a7be028a-0cf2-448f-ab55-ce8bc5d8cf69', 'b39a6fba-9026-4d69-828e-fd7068673e57')");
    }

    public override async Task Include_where_list_contains_navigation2(bool async)
    {
        await base.Include_where_list_contains_navigation2(async);

        AssertSql(
            @"SELECT [t].[Id]
FROM [Tags] AS [t]",
            //
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Id], [t0].[GearNickName], [t0].[GearSquadId], [t0].[IssueDate], [t0].[Note]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN [Cities] AS [c] ON [t].[CityOfBirthName] = [c].[Name]
LEFT JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName] AND [t].[SquadId] = [t0].[GearSquadId]
WHERE [c].[Location] IS NOT NULL AND [t0].[Id] IN ('34c8d86e-a4ac-4be5-827f-584dda348a07', 'df36f493-463f-4123-83f9-6b135deeb7ba', 'a8ad98f9-e023-4e2a-9a70-c2728455bd34', '70534e05-782c-4052-8720-c2c54481ce5f', 'a7be028a-0cf2-448f-ab55-ce8bc5d8cf69', 'b39a6fba-9026-4d69-828e-fd7068673e57')");
    }

    public override async Task Navigation_accessed_twice_outside_and_inside_subquery(bool async)
    {
        await base.Navigation_accessed_twice_outside_and_inside_subquery(async);

        AssertSql(
            @"SELECT [t].[Id]
FROM [Tags] AS [t]",
            //
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName] AND [t].[SquadId] = [t0].[GearSquadId]
WHERE [t0].[Id] IS NOT NULL AND [t0].[Id] IN ('34c8d86e-a4ac-4be5-827f-584dda348a07', 'df36f493-463f-4123-83f9-6b135deeb7ba', 'a8ad98f9-e023-4e2a-9a70-c2728455bd34', '70534e05-782c-4052-8720-c2c54481ce5f', 'a7be028a-0cf2-448f-ab55-ce8bc5d8cf69', 'b39a6fba-9026-4d69-828e-fd7068673e57')");
    }

    public override async Task Include_with_join_multi_level(bool async)
    {
        await base.Include_with_join_multi_level(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [c].[Name], [c].[Location], [c].[Nation], [t0].[Id], [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN [Tags] AS [t0] ON [t].[SquadId] = [t0].[GearSquadId] AND [t].[Nickname] = [t0].[GearNickName]
INNER JOIN [Cities] AS [c] ON [t].[CityOfBirthName] = [c].[Name]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t1] ON [c].[Name] = [t1].[AssignedCityName]
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Id], [c].[Name], [t1].[Nickname]");
    }

    public override async Task Include_with_join_and_inheritance1(bool async)
    {
        await base.Include_with_join_and_inheritance1(async);

        AssertSql(
            @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator], [c].[Name], [c].[Location], [c].[Nation]
FROM [Tags] AS [t]
INNER JOIN (
    SELECT [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t1]
    WHERE [t1].[Discriminator] = N'Officer'
) AS [t0] ON [t].[GearSquadId] = [t0].[SquadId] AND [t].[GearNickName] = [t0].[Nickname]
INNER JOIN [Cities] AS [c] ON [t0].[CityOfBirthName] = [c].[Name]");
    }

    public override async Task Include_with_join_and_inheritance_with_orderby_before_and_after_include(bool async)
    {
        await base.Include_with_join_and_inheritance_with_orderby_before_and_after_include(async);

        AssertSql(
            @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator], [t].[Id], [t2].[Nickname], [t2].[SquadId], [t2].[AssignedCityName], [t2].[CityOfBirthName], [t2].[FullName], [t2].[HasSoulPatch], [t2].[LeaderNickname], [t2].[LeaderSquadId], [t2].[Rank], [t2].[Discriminator]
FROM [Tags] AS [t]
INNER JOIN (
    SELECT [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t1]
    WHERE [t1].[Discriminator] = N'Officer'
) AS [t0] ON [t].[GearSquadId] = [t0].[SquadId] AND [t].[GearNickName] = [t0].[Nickname]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t2] ON [t0].[Nickname] = [t2].[LeaderNickname] AND [t0].[SquadId] = [t2].[LeaderSquadId]
ORDER BY [t0].[HasSoulPatch], [t0].[Nickname] DESC, [t].[Id], [t0].[SquadId], [t2].[Nickname]");
    }

    public override async Task Include_with_join_and_inheritance2(bool async)
    {
        await base.Include_with_join_and_inheritance2(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Id], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN [Tags] AS [t0] ON [t].[SquadId] = [t0].[GearSquadId] AND [t].[Nickname] = [t0].[GearNickName]
LEFT JOIN [Weapons] AS [w] ON [t].[FullName] = [w].[OwnerFullName]
WHERE [t].[Discriminator] = N'Officer'
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Id]");
    }

    public override async Task Include_with_join_and_inheritance3(bool async)
    {
        await base.Include_with_join_and_inheritance3(async);

        AssertSql(
            @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator], [t].[Id], [t2].[Nickname], [t2].[SquadId], [t2].[AssignedCityName], [t2].[CityOfBirthName], [t2].[FullName], [t2].[HasSoulPatch], [t2].[LeaderNickname], [t2].[LeaderSquadId], [t2].[Rank], [t2].[Discriminator]
FROM [Tags] AS [t]
INNER JOIN (
    SELECT [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t1]
    WHERE [t1].[Discriminator] = N'Officer'
) AS [t0] ON [t].[GearSquadId] = [t0].[SquadId] AND [t].[GearNickName] = [t0].[Nickname]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t2] ON [t0].[Nickname] = [t2].[LeaderNickname] AND [t0].[SquadId] = [t2].[LeaderSquadId]
ORDER BY [t].[Id], [t0].[Nickname], [t0].[SquadId], [t2].[Nickname]");
    }

    public override async Task Include_with_nested_navigation_in_order_by(bool async)
    {
        await base.Include_with_nested_navigation_in_order_by(async);

        AssertSql(
            @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM [Weapons] AS [w]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t] ON [w].[OwnerFullName] = [t].[FullName]
LEFT JOIN [Cities] AS [c] ON [t].[CityOfBirthName] = [c].[Name]
WHERE [t].[Nickname] <> N'Paduk' OR [t].[Nickname] IS NULL
ORDER BY [c].[Name], [w].[Id]");
    }

    public override async Task Where_enum(bool async)
    {
        await base.Where_enum(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[Rank] = 4");
    }

    public override async Task Where_nullable_enum_with_constant(bool async)
    {
        await base.Where_nullable_enum_with_constant(async);

        AssertSql(
            @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] = 1");
    }

    public override async Task Where_nullable_enum_with_null_constant(bool async)
    {
        await base.Where_nullable_enum_with_null_constant(async);

        AssertSql(
            @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] IS NULL");
    }

    public override async Task Where_nullable_enum_with_non_nullable_parameter(bool async)
    {
        await base.Where_nullable_enum_with_non_nullable_parameter(async);

        AssertSql(
            @"@__ammunitionType_0='1'

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] = @__ammunitionType_0");
    }

    public override async Task Where_nullable_enum_with_nullable_parameter(bool async)
    {
        await base.Where_nullable_enum_with_nullable_parameter(async);

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

    public override async Task Where_bitwise_and_enum(bool async)
    {
        await base.Where_bitwise_and_enum(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE ([t].[Rank] & 2) > 0",
            //
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE ([t].[Rank] & 2) = 2");
    }

    public override async Task Where_bitwise_and_integral(bool async)
    {
        await base.Where_bitwise_and_integral(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE ([t].[Rank] & 1) = 1",
            //
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE (CAST([t].[Rank] AS bigint) & CAST(1 AS bigint)) = CAST(1 AS bigint)",
            //
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE (CAST([t].[Rank] AS smallint) & CAST(1 AS smallint)) = CAST(1 AS smallint)");
    }

    public override async Task Where_bitwise_and_nullable_enum_with_constant(bool async)
    {
        await base.Where_bitwise_and_nullable_enum_with_constant(async);

        AssertSql(
            @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & 1) > 0");
    }

    public override async Task Where_bitwise_and_nullable_enum_with_null_constant(bool async)
    {
        await base.Where_bitwise_and_nullable_enum_with_null_constant(async);

        AssertSql(
            @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & NULL) > 0");
    }

    public override async Task Where_bitwise_and_nullable_enum_with_non_nullable_parameter(bool async)
    {
        await base.Where_bitwise_and_nullable_enum_with_non_nullable_parameter(async);

        AssertSql(
            @"@__ammunitionType_0='1'

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & @__ammunitionType_0) > 0");
    }

    public override async Task Where_bitwise_and_nullable_enum_with_nullable_parameter(bool async)
    {
        await base.Where_bitwise_and_nullable_enum_with_nullable_parameter(async);

        AssertSql(
            @"@__ammunitionType_0='1' (Nullable = true)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & @__ammunitionType_0) > 0",
            //
            @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & NULL) > 0");
    }

    public override async Task Where_bitwise_or_enum(bool async)
    {
        await base.Where_bitwise_or_enum(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE ([t].[Rank] | 2) > 0");
    }

    public override async Task Bitwise_projects_values_in_select(bool async)
    {
        await base.Bitwise_projects_values_in_select(async);

        AssertSql(
            @"SELECT TOP(1) CASE
    WHEN ([t].[Rank] & 2) = 2 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [BitwiseTrue], CASE
    WHEN ([t].[Rank] & 2) = 4 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [BitwiseFalse], [t].[Rank] & 2 AS [BitwiseValue]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE ([t].[Rank] & 2) = 2");
    }

    public override async Task Where_enum_has_flag(bool async)
    {
        await base.Where_enum_has_flag(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE ([t].[Rank] & 2) = 2",
            //
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE ([t].[Rank] & 18) = 18",
            //
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE ([t].[Rank] & 1) = 1",
            //
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE ([t].[Rank] & 1) = 1",
            //
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE (2 & [t].[Rank]) = [t].[Rank]");
    }

    public override async Task Where_enum_has_flag_subquery(bool async)
    {
        await base.Where_enum_has_flag_subquery(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE ([t].[Rank] & COALESCE((
    SELECT TOP(1) [t0].[Rank]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t0]
    ORDER BY [t0].[Nickname], [t0].[SquadId]), 0)) = COALESCE((
    SELECT TOP(1) [t0].[Rank]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t0]
    ORDER BY [t0].[Nickname], [t0].[SquadId]), 0)",
            //
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE (2 & COALESCE((
    SELECT TOP(1) [t0].[Rank]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t0]
    ORDER BY [t0].[Nickname], [t0].[SquadId]), 0)) = COALESCE((
    SELECT TOP(1) [t0].[Rank]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t0]
    ORDER BY [t0].[Nickname], [t0].[SquadId]), 0)");
    }

    public override async Task Where_enum_has_flag_subquery_with_pushdown(bool async)
    {
        await base.Where_enum_has_flag_subquery_with_pushdown(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE ([t].[Rank] & (
    SELECT TOP(1) [t0].[Rank]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t0]
    ORDER BY [t0].[Nickname], [t0].[SquadId])) = (
    SELECT TOP(1) [t0].[Rank]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t0]
    ORDER BY [t0].[Nickname], [t0].[SquadId]) OR (
    SELECT TOP(1) [t0].[Rank]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t0]
    ORDER BY [t0].[Nickname], [t0].[SquadId]) IS NULL",
            //
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE (2 & (
    SELECT TOP(1) [t0].[Rank]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t0]
    ORDER BY [t0].[Nickname], [t0].[SquadId])) = (
    SELECT TOP(1) [t0].[Rank]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t0]
    ORDER BY [t0].[Nickname], [t0].[SquadId]) OR (
    SELECT TOP(1) [t0].[Rank]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t0]
    ORDER BY [t0].[Nickname], [t0].[SquadId]) IS NULL");
    }

    public override async Task Where_enum_has_flag_subquery_client_eval(bool async)
    {
        await base.Where_enum_has_flag_subquery_client_eval(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE ([t].[Rank] & (
    SELECT TOP(1) [t0].[Rank]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t0]
    ORDER BY [t0].[Nickname], [t0].[SquadId])) = (
    SELECT TOP(1) [t0].[Rank]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t0]
    ORDER BY [t0].[Nickname], [t0].[SquadId]) OR (
    SELECT TOP(1) [t0].[Rank]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t0]
    ORDER BY [t0].[Nickname], [t0].[SquadId]) IS NULL");
    }

    public override async Task Where_enum_has_flag_with_non_nullable_parameter(bool async)
    {
        await base.Where_enum_has_flag_with_non_nullable_parameter(async);

        AssertSql(
            @"@__parameter_0='2'

SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE ([t].[Rank] & @__parameter_0) = @__parameter_0");
    }

    public override async Task Where_has_flag_with_nullable_parameter(bool async)
    {
        await base.Where_has_flag_with_nullable_parameter(async);

        AssertSql(
            @"@__parameter_0='2' (Nullable = true)

SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE ([t].[Rank] & @__parameter_0) = @__parameter_0");
    }

    public override async Task Select_enum_has_flag(bool async)
    {
        await base.Select_enum_has_flag(async);

        AssertSql(
            @"SELECT TOP(1) CASE
    WHEN ([t].[Rank] & 2) = 2 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [hasFlagTrue], CASE
    WHEN ([t].[Rank] & 4) = 4 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [hasFlagFalse]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE ([t].[Rank] & 2) = 2");
    }

    public override async Task Where_count_subquery_without_collision(bool async)
    {
        await base.Where_count_subquery_without_collision(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE (
    SELECT COUNT(*)
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName]) = 2");
    }

    public override async Task Where_any_subquery_without_collision(bool async)
    {
        await base.Where_any_subquery_without_collision(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE EXISTS (
    SELECT 1
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName])");
    }

    public override async Task Select_inverted_boolean(bool async)
    {
        await base.Select_inverted_boolean(async);

        AssertSql(
            @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = CAST(0 AS bit) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Manual]
FROM [Weapons] AS [w]
WHERE [w].[IsAutomatic] = CAST(1 AS bit)");
    }

    public override async Task Select_comparison_with_null(bool async)
    {
        await base.Select_comparison_with_null(async);

        AssertSql(
            @"@__ammunitionType_0='1' (Nullable = true)

SELECT [w].[Id], CASE
    WHEN [w].[AmmunitionType] = @__ammunitionType_0 AND [w].[AmmunitionType] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Cartridge]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] = @__ammunitionType_0",
            //
            @"SELECT [w].[Id], CASE
    WHEN [w].[AmmunitionType] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Cartridge]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] IS NULL");
    }

    public override async Task Select_null_parameter(bool async)
    {
        await base.Select_null_parameter(async);

        AssertSql(
            @"@__ammunitionType_0='1' (Nullable = true)

SELECT [w].[Id], @__ammunitionType_0 AS [AmmoType]
FROM [Weapons] AS [w]",
            //
            @"SELECT [w].[Id], NULL AS [AmmoType]
FROM [Weapons] AS [w]",
            //
            @"@__ammunitionType_0='2' (Nullable = true)

SELECT [w].[Id], @__ammunitionType_0 AS [AmmoType]
FROM [Weapons] AS [w]",
            //
            @"SELECT [w].[Id], NULL AS [AmmoType]
FROM [Weapons] AS [w]");
    }

    public override async Task Select_ternary_operation_with_boolean(bool async)
    {
        await base.Select_ternary_operation_with_boolean(async);

        AssertSql(
            @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = CAST(1 AS bit) THEN 1
    ELSE 0
END AS [Num]
FROM [Weapons] AS [w]");
    }

    public override async Task Select_ternary_operation_with_inverted_boolean(bool async)
    {
        await base.Select_ternary_operation_with_inverted_boolean(async);

        AssertSql(
            @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = CAST(0 AS bit) THEN 1
    ELSE 0
END AS [Num]
FROM [Weapons] AS [w]");
    }

    public override async Task Select_ternary_operation_with_has_value_not_null(bool async)
    {
        await base.Select_ternary_operation_with_has_value_not_null(async);

        AssertSql(
            @"SELECT [w].[Id], CASE
    WHEN [w].[AmmunitionType] IS NOT NULL AND [w].[AmmunitionType] = 1 THEN N'Yes'
    ELSE N'No'
END AS [IsCartridge]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] IS NOT NULL AND [w].[AmmunitionType] = 1");
    }

    public override async Task Select_ternary_operation_multiple_conditions(bool async)
    {
        await base.Select_ternary_operation_multiple_conditions(async);

        AssertSql(
            @"SELECT [w].[Id], CASE
    WHEN [w].[AmmunitionType] = 2 AND [w].[SynergyWithId] = 1 THEN N'Yes'
    ELSE N'No'
END AS [IsCartridge]
FROM [Weapons] AS [w]");
    }

    public override async Task Select_ternary_operation_multiple_conditions_2(bool async)
    {
        await base.Select_ternary_operation_multiple_conditions_2(async);

        AssertSql(
            @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = CAST(0 AS bit) AND [w].[SynergyWithId] = 1 THEN N'Yes'
    ELSE N'No'
END AS [IsCartridge]
FROM [Weapons] AS [w]");
    }

    public override async Task Select_multiple_conditions(bool async)
    {
        await base.Select_multiple_conditions(async);

        AssertSql(
            @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = CAST(0 AS bit) AND [w].[SynergyWithId] = 1 AND [w].[SynergyWithId] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsCartridge]
FROM [Weapons] AS [w]");
    }

    public override async Task Select_nested_ternary_operations(bool async)
    {
        await base.Select_nested_ternary_operations(async);

        AssertSql(
            @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = CAST(0 AS bit) THEN CASE
        WHEN [w].[AmmunitionType] = 1 THEN N'ManualCartridge'
        ELSE N'Manual'
    END
    ELSE N'Auto'
END AS [IsManualCartridge]
FROM [Weapons] AS [w]");
    }

    public override async Task Null_propagation_optimization1(bool async)
    {
        await base.Null_propagation_optimization1(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[LeaderNickname] = N'Marcus' AND [t].[LeaderNickname] IS NOT NULL");
    }

    public override async Task Null_propagation_optimization2(bool async)
    {
        await base.Null_propagation_optimization2(async);

        // issue #16050
        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE CASE
    WHEN [t].[LeaderNickname] IS NULL THEN NULL
    ELSE CASE
        WHEN [t].[LeaderNickname] IS NOT NULL AND ([t].[LeaderNickname] LIKE N'%us') THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
END = CAST(1 AS bit)");
    }

    public override async Task Null_propagation_optimization3(bool async)
    {
        await base.Null_propagation_optimization3(async);

        // issue #16050
        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE CASE
    WHEN [t].[LeaderNickname] IS NOT NULL THEN CASE
        WHEN [t].[LeaderNickname] LIKE N'%us' THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    ELSE NULL
END = CAST(1 AS bit)");
    }

    public override async Task Null_propagation_optimization4(bool async)
    {
        await base.Null_propagation_optimization4(async);

        // issue #16050
        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE CASE
    WHEN [t].[LeaderNickname] IS NULL THEN NULL
    ELSE CAST(LEN([t].[LeaderNickname]) AS int)
END = 5 AND CASE
    WHEN [t].[LeaderNickname] IS NULL THEN NULL
    ELSE CAST(LEN([t].[LeaderNickname]) AS int)
END IS NOT NULL");
    }

    public override async Task Null_propagation_optimization5(bool async)
    {
        await base.Null_propagation_optimization5(async);

        // issue #16050
        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE CASE
    WHEN [t].[LeaderNickname] IS NOT NULL THEN CAST(LEN([t].[LeaderNickname]) AS int)
    ELSE NULL
END = 5 AND CASE
    WHEN [t].[LeaderNickname] IS NOT NULL THEN CAST(LEN([t].[LeaderNickname]) AS int)
    ELSE NULL
END IS NOT NULL");
    }

    public override async Task Null_propagation_optimization6(bool async)
    {
        await base.Null_propagation_optimization6(async);

        // issue #16050
        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE CASE
    WHEN [t].[LeaderNickname] IS NOT NULL THEN CAST(LEN([t].[LeaderNickname]) AS int)
    ELSE NULL
END = 5 AND CASE
    WHEN [t].[LeaderNickname] IS NOT NULL THEN CAST(LEN([t].[LeaderNickname]) AS int)
    ELSE NULL
END IS NOT NULL");
    }

    public override async Task Select_null_propagation_optimization7(bool async)
    {
        await base.Select_null_propagation_optimization7(async);

        // issue #16050
        AssertSql(
            @"SELECT CASE
    WHEN [t].[LeaderNickname] IS NOT NULL THEN [t].[LeaderNickname] + [t].[LeaderNickname]
    ELSE NULL
END
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task Select_null_propagation_optimization8(bool async)
    {
        await base.Select_null_propagation_optimization8(async);

        AssertSql(
            @"SELECT COALESCE([t].[LeaderNickname], N'') + COALESCE([t].[LeaderNickname], N'')
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task Select_null_propagation_optimization9(bool async)
    {
        await base.Select_null_propagation_optimization9(async);

        AssertSql(
            @"SELECT CAST(LEN([t].[FullName]) AS int)
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task Select_null_propagation_negative1(bool async)
    {
        await base.Select_null_propagation_negative1(async);

        AssertSql(
            @"SELECT CASE
    WHEN [t].[LeaderNickname] IS NOT NULL THEN CASE
        WHEN CAST(LEN([t].[Nickname]) AS int) = 5 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    ELSE NULL
END
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task Select_null_propagation_negative2(bool async)
    {
        await base.Select_null_propagation_negative2(async);

        AssertSql(
            @"SELECT CASE
    WHEN [t].[LeaderNickname] IS NOT NULL THEN [t0].[LeaderNickname]
    ELSE NULL
END
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
CROSS JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0]");
    }

    public override async Task Select_null_propagation_negative3(bool async)
    {
        await base.Select_null_propagation_negative3(async);

        AssertSql(
            @"SELECT [t0].[Nickname], CASE
    WHEN [t0].[Nickname] IS NOT NULL AND [t0].[SquadId] IS NOT NULL THEN CASE
        WHEN [t0].[LeaderNickname] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    ELSE NULL
END AS [Condition]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON [t].[HasSoulPatch] = CAST(1 AS bit)
ORDER BY [t0].[Nickname]");
    }

    public override async Task Select_null_propagation_negative4(bool async)
    {
        await base.Select_null_propagation_negative4(async);

        AssertSql(
            @"SELECT CASE
    WHEN [t0].[Nickname] IS NOT NULL AND [t0].[SquadId] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [t0].[Nickname]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON [t].[HasSoulPatch] = CAST(1 AS bit)
ORDER BY [t0].[Nickname]");
    }

    public override async Task Select_null_propagation_negative5(bool async)
    {
        await base.Select_null_propagation_negative5(async);

        AssertSql(
            @"SELECT CASE
    WHEN [t0].[Nickname] IS NOT NULL AND [t0].[SquadId] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [t0].[Nickname]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON [t].[HasSoulPatch] = CAST(1 AS bit)
ORDER BY [t0].[Nickname]");
    }

    public override async Task Select_null_propagation_negative6(bool async)
    {
        await base.Select_null_propagation_negative6(async);

        AssertSql(
            @"SELECT CASE
    WHEN [t].[LeaderNickname] IS NOT NULL THEN CASE
        WHEN CAST(LEN([t].[LeaderNickname]) AS int) <> CAST(LEN([t].[LeaderNickname]) AS int) THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    ELSE NULL
END
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task Select_null_propagation_negative7(bool async)
    {
        await base.Select_null_propagation_negative7(async);

        AssertSql(
            @"SELECT CASE
    WHEN [t].[LeaderNickname] IS NOT NULL THEN CAST(1 AS bit)
    ELSE NULL
END
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task Select_null_propagation_negative8(bool async)
    {
        await base.Select_null_propagation_negative8(async);

        AssertSql(
            @"SELECT CASE
    WHEN [s].[Id] IS NOT NULL THEN [c].[Name]
    ELSE NULL
END
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
LEFT JOIN [Squads] AS [s] ON [t0].[SquadId] = [s].[Id]
LEFT JOIN [Cities] AS [c] ON [t0].[AssignedCityName] = [c].[Name]");
    }

    public override async Task Select_null_propagation_negative9(bool async)
    {
        await base.Select_null_propagation_negative9(async);

        AssertSql(
            @"SELECT CASE
    WHEN [t].[LeaderNickname] IS NOT NULL THEN COALESCE(CASE
        WHEN CAST(LEN([t].[Nickname]) AS int) = 5 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END, CAST(0 AS bit))
    ELSE NULL
END
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task Select_null_propagation_works_for_navigations_with_composite_keys(bool async)
    {
        await base.Select_null_propagation_works_for_navigations_with_composite_keys(async);

        AssertSql(
            @"SELECT [t0].[Nickname]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]");
    }

    public override async Task Select_null_propagation_works_for_multiple_navigations_with_composite_keys(bool async)
    {
        await base.Select_null_propagation_works_for_multiple_navigations_with_composite_keys(async);

        AssertSql(
            @"SELECT CASE
    WHEN [c].[Name] IS NOT NULL THEN [c].[Name]
    ELSE NULL
END
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
LEFT JOIN [Tags] AS [t1] ON ([t0].[Nickname] = [t1].[GearNickName] OR ([t0].[Nickname] IS NULL AND [t1].[GearNickName] IS NULL)) AND ([t0].[SquadId] = [t1].[GearSquadId] OR ([t0].[SquadId] IS NULL AND [t1].[GearSquadId] IS NULL))
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t2] ON [t1].[GearNickName] = [t2].[Nickname] AND [t1].[GearSquadId] = [t2].[SquadId]
LEFT JOIN [Cities] AS [c] ON [t2].[AssignedCityName] = [c].[Name]");
    }

    public override async Task Select_conditional_with_anonymous_type_and_null_constant(bool async)
    {
        await base.Select_conditional_with_anonymous_type_and_null_constant(async);

        AssertSql(
            @"SELECT CASE
    WHEN [t].[LeaderNickname] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [t].[HasSoulPatch]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
ORDER BY [t].[Nickname]");
    }

    public override async Task Select_conditional_with_anonymous_types(bool async)
    {
        await base.Select_conditional_with_anonymous_types(async);

        AssertSql(
            @"SELECT CASE
    WHEN [t].[LeaderNickname] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [t].[Nickname], [t].[FullName]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
ORDER BY [t].[Nickname]");
    }

    public override async Task Where_conditional_equality_1(bool async)
    {
        await base.Where_conditional_equality_1(async);

        AssertSql(
            @"SELECT [t].[Nickname]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[LeaderNickname] IS NULL
ORDER BY [t].[Nickname]");
    }

    public override async Task Where_conditional_equality_2(bool async)
    {
        await base.Where_conditional_equality_2(async);

        AssertSql(
            @"SELECT [t].[Nickname]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[LeaderNickname] IS NULL
ORDER BY [t].[Nickname]");
    }

    public override async Task Where_conditional_equality_3(bool async)
    {
        await base.Where_conditional_equality_3(async);

        AssertSql(
            @"SELECT [t].[Nickname]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
ORDER BY [t].[Nickname]");
    }

    public override async Task Select_coalesce_with_anonymous_types(bool async)
    {
        await base.Select_coalesce_with_anonymous_types(async);

        AssertSql(
            @"SELECT [t].[LeaderNickname], [t].[FullName]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
ORDER BY [t].[Nickname]");
    }

    public override async Task Where_compare_anonymous_types(bool async)
    {
        await base.Where_compare_anonymous_types(async);

        AssertSql();
    }

    public override async Task Where_member_access_on_anonymous_type(bool async)
    {
        await base.Where_member_access_on_anonymous_type(async);

        AssertSql(
            @"SELECT [t].[Nickname]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[LeaderNickname] = N'Marcus'");
    }

    public override async Task Where_compare_anonymous_types_with_uncorrelated_members(bool async)
    {
        await base.Where_compare_anonymous_types_with_uncorrelated_members(async);

        AssertSql(
            @"SELECT [t].[Nickname]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE 0 = 1");
    }

    public override async Task Select_Where_Navigation_Scalar_Equals_Navigation_Scalar(bool async)
    {
        await base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t0].[Id], [t0].[GearNickName], [t0].[GearSquadId], [t0].[IssueDate], [t0].[Note]
FROM [Tags] AS [t]
CROSS JOIN [Tags] AS [t0]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t1] ON [t].[GearNickName] = [t1].[Nickname] AND [t].[GearSquadId] = [t1].[SquadId]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t2] ON [t0].[GearNickName] = [t2].[Nickname] AND [t0].[GearSquadId] = [t2].[SquadId]
WHERE [t1].[Nickname] = [t2].[Nickname] OR ([t1].[Nickname] IS NULL AND [t2].[Nickname] IS NULL)");
    }

    public override async Task Select_Singleton_Navigation_With_Member_Access(bool async)
    {
        await base.Select_Singleton_Navigation_With_Member_Access(async);

        AssertSql(
            @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE [t0].[Nickname] = N'Marcus' AND ([t0].[CityOfBirthName] <> N'Ephyra' OR [t0].[CityOfBirthName] IS NULL)");
    }

    public override async Task Select_Where_Navigation(bool async)
    {
        await base.Select_Where_Navigation(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE [t0].[Nickname] = N'Marcus'");
    }

    public override async Task Select_Where_Navigation_Equals_Navigation(bool async)
    {
        await base.Select_Where_Navigation_Equals_Navigation(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t0].[Id], [t0].[GearNickName], [t0].[GearSquadId], [t0].[IssueDate], [t0].[Note]
FROM [Tags] AS [t]
CROSS JOIN [Tags] AS [t0]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t1] ON [t].[GearNickName] = [t1].[Nickname] AND [t].[GearSquadId] = [t1].[SquadId]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t2] ON [t0].[GearNickName] = [t2].[Nickname] AND [t0].[GearSquadId] = [t2].[SquadId]
WHERE ([t1].[Nickname] = [t2].[Nickname] OR ([t1].[Nickname] IS NULL AND [t2].[Nickname] IS NULL)) AND ([t1].[SquadId] = [t2].[SquadId] OR ([t1].[SquadId] IS NULL AND [t2].[SquadId] IS NULL))");
    }

    public override async Task Select_Where_Navigation_Null(bool async)
    {
        await base.Select_Where_Navigation_Null(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE [t0].[Nickname] IS NULL OR [t0].[SquadId] IS NULL");
    }

    public override async Task Select_Where_Navigation_Null_Reverse(bool async)
    {
        await base.Select_Where_Navigation_Null_Reverse(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE [t0].[Nickname] IS NULL OR [t0].[SquadId] IS NULL");
    }

    public override async Task Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected(bool async)
    {
        await base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected(async);

        AssertSql(
            @"SELECT [t].[Id] AS [Id1], [t0].[Id] AS [Id2]
FROM [Tags] AS [t]
CROSS JOIN [Tags] AS [t0]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t1] ON [t].[GearNickName] = [t1].[Nickname] AND [t].[GearSquadId] = [t1].[SquadId]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t2] ON [t0].[GearNickName] = [t2].[Nickname] AND [t0].[GearSquadId] = [t2].[SquadId]
WHERE [t1].[Nickname] = [t2].[Nickname] OR ([t1].[Nickname] IS NULL AND [t2].[Nickname] IS NULL)");
    }

    public override async Task Optional_Navigation_Null_Coalesce_To_Clr_Type(bool async)
    {
        await base.Optional_Navigation_Null_Coalesce_To_Clr_Type(async);

        AssertSql(
            @"SELECT TOP(1) COALESCE([w0].[IsAutomatic], CAST(0 AS bit)) AS [IsAutomatic]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY [w].[Id]");
    }

    public override async Task Where_subquery_boolean(bool async)
    {
        await base.Where_subquery_boolean(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE COALESCE((
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]), CAST(0 AS bit)) = CAST(1 AS bit)");
    }

    public override async Task Where_subquery_boolean_with_pushdown(bool async)
    {
        await base.Where_subquery_boolean_with_pushdown(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE (
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]) = CAST(1 AS bit)");
    }

    public override async Task Where_subquery_distinct_firstordefault_boolean(bool async)
    {
        await base.Where_subquery_distinct_firstordefault_boolean(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit) AND COALESCE((
    SELECT TOP(1) [t0].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [t].[FullName] = [w].[OwnerFullName]
    ) AS [t0]
    ORDER BY [t0].[Id]), CAST(0 AS bit)) = CAST(1 AS bit)");
    }

    public override async Task Where_subquery_distinct_firstordefault_boolean_with_pushdown(bool async)
    {
        await base.Where_subquery_distinct_firstordefault_boolean_with_pushdown(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit) AND (
    SELECT TOP(1) [t0].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [t].[FullName] = [w].[OwnerFullName]
    ) AS [t0]
    ORDER BY [t0].[Id]) = CAST(1 AS bit)");
    }

    public override async Task Where_subquery_distinct_first_boolean(bool async)
    {
        await base.Where_subquery_distinct_first_boolean(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit) AND (
    SELECT TOP(1) [t0].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [t].[FullName] = [w].[OwnerFullName]
    ) AS [t0]
    ORDER BY [t0].[Id]) = CAST(1 AS bit)
ORDER BY [t].[Nickname]");
    }

    public override async Task Where_subquery_distinct_singleordefault_boolean1(bool async)
    {
        await base.Where_subquery_distinct_singleordefault_boolean1(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit) AND COALESCE((
    SELECT TOP(1) [t0].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [t].[FullName] = [w].[OwnerFullName] AND ([w].[Name] LIKE N'%Lancer%')
    ) AS [t0]), CAST(0 AS bit)) = CAST(1 AS bit)
ORDER BY [t].[Nickname]");
    }

    public override async Task Where_subquery_distinct_singleordefault_boolean2(bool async)
    {
        await base.Where_subquery_distinct_singleordefault_boolean2(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit) AND COALESCE((
    SELECT DISTINCT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName] AND ([w].[Name] LIKE N'%Lancer%')), CAST(0 AS bit)) = CAST(1 AS bit)
ORDER BY [t].[Nickname]");
    }

    public override async Task Where_subquery_distinct_singleordefault_boolean_with_pushdown(bool async)
    {
        await base.Where_subquery_distinct_singleordefault_boolean_with_pushdown(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit) AND (
    SELECT TOP(1) [t0].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [t].[FullName] = [w].[OwnerFullName] AND ([w].[Name] LIKE N'%Lancer%')
    ) AS [t0]) = CAST(1 AS bit)
ORDER BY [t].[Nickname]");
    }

    public override async Task Where_subquery_distinct_lastordefault_boolean(bool async)
    {
        await base.Where_subquery_distinct_lastordefault_boolean(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE (
    SELECT TOP(1) [t0].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [t].[FullName] = [w].[OwnerFullName]
    ) AS [t0]
    ORDER BY [t0].[Id] DESC) = CAST(0 AS bit)
ORDER BY [t].[Nickname]");
    }

    public override async Task Where_subquery_distinct_last_boolean(bool async)
    {
        await base.Where_subquery_distinct_last_boolean(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(0 AS bit) AND (
    SELECT TOP(1) [t0].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [t].[FullName] = [w].[OwnerFullName]
    ) AS [t0]
    ORDER BY [t0].[Id] DESC) = CAST(1 AS bit)
ORDER BY [t].[Nickname]");
    }

    public override async Task Where_subquery_distinct_orderby_firstordefault_boolean(bool async)
    {
        await base.Where_subquery_distinct_orderby_firstordefault_boolean(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit) AND COALESCE((
    SELECT TOP(1) [t0].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [t].[FullName] = [w].[OwnerFullName]
    ) AS [t0]
    ORDER BY [t0].[Id]), CAST(0 AS bit)) = CAST(1 AS bit)");
    }

    public override async Task Where_subquery_distinct_orderby_firstordefault_boolean_with_pushdown(bool async)
    {
        await base.Where_subquery_distinct_orderby_firstordefault_boolean_with_pushdown(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit) AND (
    SELECT TOP(1) [t0].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [t].[FullName] = [w].[OwnerFullName]
    ) AS [t0]
    ORDER BY [t0].[Id]) = CAST(1 AS bit)");
    }

    public override async Task Where_subquery_union_firstordefault_boolean(bool async)
    {
        await base.Where_subquery_union_firstordefault_boolean(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit) AND (
    SELECT TOP(1) [t0].[IsAutomatic]
    FROM (
        SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [t].[FullName] = [w].[OwnerFullName]
        UNION
        SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
        FROM [Weapons] AS [w0]
        WHERE [t].[FullName] = [w0].[OwnerFullName]
    ) AS [t0]
    ORDER BY [t0].[Id]) = CAST(1 AS bit)");
    }

    public override async Task Where_subquery_join_firstordefault_boolean(bool async)
    {
        await base.Where_subquery_join_firstordefault_boolean(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit) AND (
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    INNER JOIN (
        SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
        FROM [Weapons] AS [w0]
        WHERE [t].[FullName] = [w0].[OwnerFullName]
    ) AS [t0] ON [w].[Id] = [t0].[Id]
    WHERE [t].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]) = CAST(1 AS bit)");
    }

    public override async Task Where_subquery_left_join_firstordefault_boolean(bool async)
    {
        await base.Where_subquery_left_join_firstordefault_boolean(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit) AND (
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    LEFT JOIN (
        SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
        FROM [Weapons] AS [w0]
        WHERE [t].[FullName] = [w0].[OwnerFullName]
    ) AS [t0] ON [w].[Id] = [t0].[Id]
    WHERE [t].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]) = CAST(1 AS bit)");
    }

    public override async Task Where_subquery_concat_firstordefault_boolean(bool async)
    {
        await base.Where_subquery_concat_firstordefault_boolean(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit) AND (
    SELECT TOP(1) [t0].[IsAutomatic]
    FROM (
        SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [t].[FullName] = [w].[OwnerFullName]
        UNION ALL
        SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
        FROM [Weapons] AS [w0]
        WHERE [t].[FullName] = [w0].[OwnerFullName]
    ) AS [t0]
    ORDER BY [t0].[Id]) = CAST(1 AS bit)");
    }

    public override async Task Concat_with_count(bool async)
    {
        await base.Concat_with_count(async);

        AssertSql(
            @"SELECT COUNT(*)
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
    UNION ALL
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0]");
    }

    public override async Task Concat_scalars_with_count(bool async)
    {
        await base.Concat_scalars_with_count(async);

        AssertSql(
            @"SELECT COUNT(*)
FROM (
    SELECT [t].[Nickname]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    UNION ALL
    SELECT [t1].[FullName] AS [Nickname]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t1]
) AS [t0]");
    }

    public override async Task Concat_anonymous_with_count(bool async)
    {
        await base.Concat_anonymous_with_count(async);

        AssertSql(
            @"SELECT COUNT(*)
FROM (
    SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t].[Nickname] AS [Name]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    UNION ALL
    SELECT [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator], [t1].[FullName] AS [Name]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t1]
) AS [t0]");
    }

    public override async Task Concat_with_scalar_projection(bool async)
    {
        await base.Concat_with_scalar_projection(async);

        AssertSql(
            @"SELECT [t0].[Nickname]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
    UNION ALL
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0]");
    }

    public override async Task Select_navigation_with_concat_and_count(bool async)
    {
        await base.Select_navigation_with_concat_and_count(async);

        AssertSql(
            @"SELECT (
    SELECT COUNT(*)
    FROM (
        SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [t].[FullName] = [w].[OwnerFullName]
        UNION ALL
        SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
        FROM [Weapons] AS [w0]
        WHERE [t].[FullName] = [w0].[OwnerFullName]
    ) AS [t0])
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(0 AS bit)");
    }

    public override async Task Concat_with_collection_navigations(bool async)
    {
        await base.Concat_with_collection_navigations(async);

        AssertSql(
            @"SELECT (
    SELECT COUNT(*)
    FROM (
        SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [t].[FullName] = [w].[OwnerFullName]
        UNION
        SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
        FROM [Weapons] AS [w0]
        WHERE [t].[FullName] = [w0].[OwnerFullName]
    ) AS [t0])
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit)");
    }

    public override async Task Union_with_collection_navigations(bool async)
    {
        await base.Union_with_collection_navigations(async);

        AssertSql(
            @"SELECT (
    SELECT COUNT(*)
    FROM (
        SELECT [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator]
        FROM (
            SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
            FROM [Gears] AS [g0]
            UNION ALL
            SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
            FROM [Officers] AS [o0]
        ) AS [t1]
        WHERE [t].[Nickname] = [t1].[LeaderNickname] AND [t].[SquadId] = [t1].[LeaderSquadId]
        UNION
        SELECT [t2].[Nickname], [t2].[SquadId], [t2].[AssignedCityName], [t2].[CityOfBirthName], [t2].[FullName], [t2].[HasSoulPatch], [t2].[LeaderNickname], [t2].[LeaderSquadId], [t2].[Rank], [t2].[Discriminator]
        FROM (
            SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOfBirthName], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[Rank], N'Gear' AS [Discriminator]
            FROM [Gears] AS [g1]
            UNION ALL
            SELECT [o1].[Nickname], [o1].[SquadId], [o1].[AssignedCityName], [o1].[CityOfBirthName], [o1].[FullName], [o1].[HasSoulPatch], [o1].[LeaderNickname], [o1].[LeaderSquadId], [o1].[Rank], N'Officer' AS [Discriminator]
            FROM [Officers] AS [o1]
        ) AS [t2]
        WHERE [t].[Nickname] = [t2].[LeaderNickname] AND [t].[SquadId] = [t2].[LeaderSquadId]
    ) AS [t0])
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[Discriminator] = N'Officer'");
    }

    public override async Task Select_subquery_distinct_firstordefault(bool async)
    {
        await base.Select_subquery_distinct_firstordefault(async);

        AssertSql(
            @"SELECT (
    SELECT TOP(1) [t0].[Name]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [t].[FullName] = [w].[OwnerFullName]
    ) AS [t0]
    ORDER BY [t0].[Id])
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit)");
    }

    public override async Task Singleton_Navigation_With_Member_Access(bool async)
    {
        await base.Singleton_Navigation_With_Member_Access(async);

        AssertSql(
            @"SELECT [t0].[CityOfBirthName] AS [B]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE [t0].[Nickname] = N'Marcus' AND ([t0].[CityOfBirthName] <> N'Ephyra' OR [t0].[CityOfBirthName] IS NULL)");
    }

    public override async Task GroupJoin_Composite_Key(bool async)
    {
        await base.GroupJoin_Composite_Key(async);

        AssertSql(
            @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM [Tags] AS [t]
INNER JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]");
    }

    public override async Task Join_navigation_translated_to_subquery_composite_key(bool async)
    {
        await base.Join_navigation_translated_to_subquery_composite_key(async);

        AssertSql(
            @"SELECT [t].[FullName], [t1].[Note]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN (
    SELECT [t0].[Note], [t2].[FullName]
    FROM [Tags] AS [t0]
    LEFT JOIN (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t2] ON [t0].[GearNickName] = [t2].[Nickname] AND [t0].[GearSquadId] = [t2].[SquadId]
) AS [t1] ON [t].[FullName] = [t1].[FullName]");
    }

    public override async Task Join_with_order_by_on_inner_sequence_navigation_translated_to_subquery_composite_key(bool async)
    {
        await base.Join_with_order_by_on_inner_sequence_navigation_translated_to_subquery_composite_key(async);

        AssertSql(
            @"SELECT [t].[FullName], [t1].[Note]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN (
    SELECT [t0].[Note], [t2].[FullName]
    FROM [Tags] AS [t0]
    LEFT JOIN (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t2] ON [t0].[GearNickName] = [t2].[Nickname] AND [t0].[GearSquadId] = [t2].[SquadId]
) AS [t1] ON [t].[FullName] = [t1].[FullName]");
    }

    public override async Task Join_with_order_by_without_skip_or_take(bool async)
    {
        await base.Join_with_order_by_without_skip_or_take(async);

        AssertSql(
            @"SELECT [w].[Name], [t].[FullName]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN [Weapons] AS [w] ON [t].[FullName] = [w].[OwnerFullName]");
    }

    public override async Task Join_with_order_by_without_skip_or_take_nested(bool async)
    {
        await base.Join_with_order_by_without_skip_or_take_nested(async);

        AssertSql(
            @"SELECT [w].[Name], [t].[FullName]
FROM [Squads] AS [s]
INNER JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t] ON [s].[Id] = [t].[SquadId]
INNER JOIN [Weapons] AS [w] ON [t].[FullName] = [w].[OwnerFullName]");
    }

    public override async Task Collection_with_inheritance_and_join_include_joined(bool async)
    {
        await base.Collection_with_inheritance_and_join_include_joined(async);

        AssertSql(
            @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator], [t2].[Id], [t2].[GearNickName], [t2].[GearSquadId], [t2].[IssueDate], [t2].[Note]
FROM [Tags] AS [t]
INNER JOIN (
    SELECT [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t1]
    WHERE [t1].[Discriminator] = N'Officer'
) AS [t0] ON [t].[GearSquadId] = [t0].[SquadId] AND [t].[GearNickName] = [t0].[Nickname]
LEFT JOIN [Tags] AS [t2] ON [t0].[Nickname] = [t2].[GearNickName] AND [t0].[SquadId] = [t2].[GearSquadId]");
    }

    public override async Task Collection_with_inheritance_and_join_include_source(bool async)
    {
        await base.Collection_with_inheritance_and_join_include_source(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t1].[Id], [t1].[GearNickName], [t1].[GearSquadId], [t1].[IssueDate], [t1].[Note]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN [Tags] AS [t0] ON [t].[SquadId] = [t0].[GearSquadId] AND [t].[Nickname] = [t0].[GearNickName]
LEFT JOIN [Tags] AS [t1] ON [t].[Nickname] = [t1].[GearNickName] AND [t].[SquadId] = [t1].[GearSquadId]
WHERE [t].[Discriminator] = N'Officer'");
    }

    public override async Task Non_unicode_string_literal_is_used_for_non_unicode_column(bool async)
    {
        await base.Non_unicode_string_literal_is_used_for_non_unicode_column(async);

        AssertSql(
            @"SELECT [c].[Name], [c].[Location], [c].[Nation]
FROM [Cities] AS [c]
WHERE [c].[Location] = 'Unknown'");
    }

    public override async Task Non_unicode_string_literal_is_used_for_non_unicode_column_right(bool async)
    {
        await base.Non_unicode_string_literal_is_used_for_non_unicode_column_right(async);

        AssertSql(
            @"SELECT [c].[Name], [c].[Location], [c].[Nation]
FROM [Cities] AS [c]
WHERE 'Unknown' = [c].[Location]");
    }

    public override async Task Non_unicode_parameter_is_used_for_non_unicode_column(bool async)
    {
        await base.Non_unicode_parameter_is_used_for_non_unicode_column(async);

        AssertSql(
            @"@__value_0='Unknown' (Size = 100) (DbType = AnsiString)

SELECT [c].[Name], [c].[Location], [c].[Nation]
FROM [Cities] AS [c]
WHERE [c].[Location] = @__value_0");
    }

    public override async Task Non_unicode_string_literals_in_contains_is_used_for_non_unicode_column(bool async)
    {
        await base.Non_unicode_string_literals_in_contains_is_used_for_non_unicode_column(async);

        AssertSql(
            @"SELECT [c].[Name], [c].[Location], [c].[Nation]
FROM [Cities] AS [c]
WHERE [c].[Location] IN ('Unknown', 'Jacinto''s location', 'Ephyra''s location')");
    }

    public override async Task Non_unicode_string_literals_is_used_for_non_unicode_column_with_subquery(bool async)
    {
        await base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_subquery(async);

        AssertSql(
            @"SELECT [c].[Name], [c].[Location], [c].[Nation]
FROM [Cities] AS [c]
WHERE [c].[Location] = 'Unknown' AND (
    SELECT COUNT(*)
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    WHERE [c].[Name] = [t].[CityOfBirthName] AND [t].[Nickname] = N'Paduk') = 1");
    }

    public override async Task Non_unicode_string_literals_is_used_for_non_unicode_column_in_subquery(bool async)
    {
        await base.Non_unicode_string_literals_is_used_for_non_unicode_column_in_subquery(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN [Cities] AS [c] ON [t].[CityOfBirthName] = [c].[Name]
WHERE [t].[Nickname] = N'Marcus' AND [c].[Location] = 'Jacinto''s location'");
    }

    public override async Task Non_unicode_string_literals_is_used_for_non_unicode_column_with_contains(bool async)
    {
        await base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_contains(async);

        AssertSql(
            @"SELECT [c].[Name], [c].[Location], [c].[Nation]
FROM [Cities] AS [c]
WHERE [c].[Location] LIKE '%Jacinto%'");
    }

    public override async Task Non_unicode_string_literals_is_used_for_non_unicode_column_with_concat(bool async)
    {
        await base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_concat(async);

        AssertSql(
            @"SELECT [c].[Name], [c].[Location], [c].[Nation]
FROM [Cities] AS [c]
WHERE COALESCE([c].[Location], '') + 'Added' LIKE '%Add%'");
    }

    public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1()
    {
        base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1();

        // Issue#16897
        AssertSql(
            @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON [t].[LeaderNickname] = [t0].[Nickname]
LEFT JOIN [Weapons] AS [w] ON [t].[FullName] = [w].[OwnerFullName]
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Nickname], [t0].[SquadId]");
    }

    public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2()
    {
        base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2();

        // Issue#16897
        AssertSql(
            @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator], [t].[Nickname], [t].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON [t].[LeaderNickname] = [t0].[Nickname]
LEFT JOIN [Weapons] AS [w] ON [t0].[FullName] = [w].[OwnerFullName]
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Nickname], [t0].[SquadId]");
    }

    public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3(bool async)
    {
        await base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3(async);

        // Issue#16897
        AssertSql(
            @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator], [t].[Nickname], [t].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON [t].[LeaderNickname] = [t0].[Nickname]
LEFT JOIN [Weapons] AS [w] ON [t0].[FullName] = [w].[OwnerFullName]
LEFT JOIN [Weapons] AS [w0] ON [t].[FullName] = [w0].[OwnerFullName]
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Nickname], [t0].[SquadId], [w].[Id]");
    }

    public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result4(bool async)
    {
        await base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result4(async);

        // Issue#16897
        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator], [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId], [w1].[Id], [w1].[AmmunitionType], [w1].[IsAutomatic], [w1].[Name], [w1].[OwnerFullName], [w1].[SynergyWithId], [w2].[Id], [w2].[AmmunitionType], [w2].[IsAutomatic], [w2].[Name], [w2].[OwnerFullName], [w2].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON [t].[LeaderNickname] = [t0].[Nickname]
LEFT JOIN [Weapons] AS [w] ON [t].[FullName] = [w].[OwnerFullName]
LEFT JOIN [Weapons] AS [w0] ON [t0].[FullName] = [w0].[OwnerFullName]
LEFT JOIN [Weapons] AS [w1] ON [t0].[FullName] = [w1].[OwnerFullName]
LEFT JOIN [Weapons] AS [w2] ON [t].[FullName] = [w2].[OwnerFullName]
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Nickname], [t0].[SquadId], [w].[Id], [w0].[Id], [w1].[Id]");
    }

    public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result(bool async)
    {
        await base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result(async);

        // Issue#16897
        AssertSql(
            @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator], [t].[Nickname], [t].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t1]
    WHERE [t1].[Discriminator] = N'Officer'
) AS [t0] ON [t].[LeaderNickname] = [t0].[Nickname]
LEFT JOIN [Weapons] AS [w] ON [t0].[FullName] = [w].[OwnerFullName]
LEFT JOIN [Weapons] AS [w0] ON [t].[FullName] = [w0].[OwnerFullName]
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Nickname], [t0].[SquadId], [w].[Id]");
    }

    public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result(bool async)
    {
        await base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result(async);

        // Issue#16897
        AssertSql(
            @"SELECT CASE
    WHEN [t0].[Nickname] IS NOT NULL AND [t0].[SquadId] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator], [t].[Nickname], [t].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON [t].[LeaderNickname] = [t0].[Nickname]
LEFT JOIN [Weapons] AS [w] ON [t0].[FullName] = [w].[OwnerFullName]
LEFT JOIN [Weapons] AS [w0] ON [t].[FullName] = [w0].[OwnerFullName]
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Nickname], [t0].[SquadId], [w].[Id]");
    }

    public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result(bool async)
    {
        await base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator], [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId], [w1].[Id], [w1].[AmmunitionType], [w1].[IsAutomatic], [w1].[Name], [w1].[OwnerFullName], [w1].[SynergyWithId], [w2].[Id], [w2].[AmmunitionType], [w2].[IsAutomatic], [w2].[Name], [w2].[OwnerFullName], [w2].[SynergyWithId], CASE
    WHEN [t0].[Nickname] IS NOT NULL AND [t0].[SquadId] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [w3].[Id], [w3].[AmmunitionType], [w3].[IsAutomatic], [w3].[Name], [w3].[OwnerFullName], [w3].[SynergyWithId], [w4].[Id], [w4].[AmmunitionType], [w4].[IsAutomatic], [w4].[Name], [w4].[OwnerFullName], [w4].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON [t].[LeaderNickname] = [t0].[Nickname]
LEFT JOIN [Weapons] AS [w] ON [t].[FullName] = [w].[OwnerFullName]
LEFT JOIN [Weapons] AS [w0] ON [t0].[FullName] = [w0].[OwnerFullName]
LEFT JOIN [Weapons] AS [w1] ON [t0].[FullName] = [w1].[OwnerFullName]
LEFT JOIN [Weapons] AS [w2] ON [t].[FullName] = [w2].[OwnerFullName]
LEFT JOIN [Weapons] AS [w3] ON [t0].[FullName] = [w3].[OwnerFullName]
LEFT JOIN [Weapons] AS [w4] ON [t].[FullName] = [w4].[OwnerFullName]
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Nickname], [t0].[SquadId], [w].[Id], [w0].[Id], [w1].[Id], [w2].[Id], [w3].[Id]");
    }

    public override async Task Coalesce_operator_in_predicate(bool async)
    {
        await base.Coalesce_operator_in_predicate(async);

        AssertSql(
            @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE COALESCE([w].[IsAutomatic], CAST(0 AS bit)) = CAST(1 AS bit)");
    }

    public override async Task Coalesce_operator_in_predicate_with_other_conditions(bool async)
    {
        await base.Coalesce_operator_in_predicate_with_other_conditions(async);

        AssertSql(
            @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] = 1 AND COALESCE([w].[IsAutomatic], CAST(0 AS bit)) = CAST(1 AS bit)");
    }

    public override async Task Coalesce_operator_in_projection_with_other_conditions(bool async)
    {
        await base.Coalesce_operator_in_projection_with_other_conditions(async);

        AssertSql(
            @"SELECT CASE
    WHEN [w].[AmmunitionType] = 1 AND [w].[AmmunitionType] IS NOT NULL AND COALESCE([w].[IsAutomatic], CAST(0 AS bit)) = CAST(1 AS bit) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Weapons] AS [w]");
    }

    public override async Task Optional_navigation_type_compensation_works_with_predicate(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_predicate(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE ([t].[Note] <> N'K.I.A.' OR [t].[Note] IS NULL) AND [t0].[HasSoulPatch] = CAST(1 AS bit)");
    }

    public override async Task Optional_navigation_type_compensation_works_with_predicate2(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_predicate2(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE [t0].[HasSoulPatch] = CAST(1 AS bit)");
    }

    public override async Task Optional_navigation_type_compensation_works_with_predicate_negated(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_predicate_negated(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE [t0].[HasSoulPatch] = CAST(0 AS bit)");
    }

    public override async Task Optional_navigation_type_compensation_works_with_predicate_negated_complex1(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_predicate_negated_complex1(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE CASE
    WHEN [t0].[HasSoulPatch] = CAST(1 AS bit) THEN CAST(1 AS bit)
    ELSE [t0].[HasSoulPatch]
END = CAST(0 AS bit)");
    }

    public override async Task Optional_navigation_type_compensation_works_with_predicate_negated_complex2(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_predicate_negated_complex2(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE CASE
    WHEN [t0].[HasSoulPatch] = CAST(0 AS bit) THEN CAST(0 AS bit)
    ELSE [t0].[HasSoulPatch]
END = CAST(0 AS bit)");
    }

    public override async Task Optional_navigation_type_compensation_works_with_conditional_expression(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_conditional_expression(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE CASE
    WHEN [t0].[HasSoulPatch] = CAST(1 AS bit) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CAST(1 AS bit)");
    }

    public override async Task Optional_navigation_type_compensation_works_with_binary_expression(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_binary_expression(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE [t0].[HasSoulPatch] = CAST(1 AS bit) OR ([t].[Note] LIKE N'%Cole%')");
    }

    public override async Task Optional_navigation_type_compensation_works_with_binary_and_expression(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_binary_and_expression(async);

        AssertSql(
            @"SELECT CASE
    WHEN [t0].[HasSoulPatch] = CAST(1 AS bit) AND ([t].[Note] LIKE N'%Cole%') THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]");
    }

    public override async Task Optional_navigation_type_compensation_works_with_projection(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_projection(async);

        AssertSql(
            @"SELECT [t0].[SquadId]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE [t].[Note] <> N'K.I.A.' OR [t].[Note] IS NULL");
    }

    public override async Task Optional_navigation_type_compensation_works_with_projection_into_anonymous_type(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_projection_into_anonymous_type(async);

        AssertSql(
            @"SELECT [t0].[SquadId]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE [t].[Note] <> N'K.I.A.' OR [t].[Note] IS NULL");
    }

    public override async Task Optional_navigation_type_compensation_works_with_DTOs(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_DTOs(async);

        AssertSql(
            @"SELECT [t0].[SquadId] AS [Id]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE [t].[Note] <> N'K.I.A.' OR [t].[Note] IS NULL");
    }

    public override async Task Optional_navigation_type_compensation_works_with_list_initializers(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_list_initializers(async);

        AssertSql(
            @"SELECT [t0].[SquadId], [t0].[SquadId] + 1
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE [t].[Note] <> N'K.I.A.' OR [t].[Note] IS NULL
ORDER BY [t].[Note]");
    }

    public override async Task Optional_navigation_type_compensation_works_with_array_initializers(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_array_initializers(async);

        AssertSql(
            @"SELECT [t0].[SquadId]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE [t].[Note] <> N'K.I.A.' OR [t].[Note] IS NULL");
    }

    public override async Task Optional_navigation_type_compensation_works_with_orderby(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_orderby(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE [t].[Note] <> N'K.I.A.' OR [t].[Note] IS NULL
ORDER BY [t0].[SquadId]");
    }

    public override async Task Optional_navigation_type_compensation_works_with_all(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_all(async);

        AssertSql(
            @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Tags] AS [t]
        LEFT JOIN (
            SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
            FROM [Gears] AS [g]
            UNION ALL
            SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
            FROM [Officers] AS [o]
        ) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
        WHERE ([t].[Note] <> N'K.I.A.' OR [t].[Note] IS NULL) AND [t0].[HasSoulPatch] = CAST(0 AS bit)) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
    }

    public override async Task Optional_navigation_type_compensation_works_with_negated_predicate(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_negated_predicate(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE ([t].[Note] <> N'K.I.A.' OR [t].[Note] IS NULL) AND [t0].[HasSoulPatch] = CAST(0 AS bit)");
    }

    public override async Task Optional_navigation_type_compensation_works_with_contains(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_contains(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE ([t].[Note] <> N'K.I.A.' OR [t].[Note] IS NULL) AND EXISTS (
    SELECT 1
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t1]
    WHERE [t1].[SquadId] = [t0].[SquadId])");
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

    public override async Task Select_correlated_filtered_collection(bool async)
    {
        await base.Select_correlated_filtered_collection(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [c].[Name], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN [Cities] AS [c] ON [t].[CityOfBirthName] = [c].[Name]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
    FROM [Weapons] AS [w]
    WHERE [w].[Name] <> N'Lancer' OR [w].[Name] IS NULL
) AS [t0] ON [t].[FullName] = [t0].[OwnerFullName]
WHERE [c].[Name] IN (N'Ephyra', N'Hanover')
ORDER BY [t].[Nickname], [t].[SquadId], [c].[Name]");
    }

    public override async Task Select_correlated_filtered_collection_with_composite_key(bool async)
    {
        await base.Select_correlated_filtered_collection_with_composite_key(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t1]
    WHERE [t1].[Nickname] <> N'Dom'
) AS [t0] ON [t].[Nickname] = [t0].[LeaderNickname] AND [t].[SquadId] = [t0].[LeaderSquadId]
WHERE [t].[Discriminator] = N'Officer'
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Nickname]");
    }

    public override async Task Select_correlated_filtered_collection_works_with_caching(bool async)
    {
        await base.Select_correlated_filtered_collection_works_with_caching(async);

        AssertSql(
            @"SELECT [t].[Id], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname]
ORDER BY [t].[Note], [t].[Id], [t0].[Nickname]");
    }

    public override async Task Join_predicate_value_equals_condition(bool async)
    {
        await base.Join_predicate_value_equals_condition(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN [Weapons] AS [w] ON [w].[SynergyWithId] IS NOT NULL");
    }

    public override async Task Join_predicate_value(bool async)
    {
        await base.Join_predicate_value(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN [Weapons] AS [w] ON [t].[HasSoulPatch] = CAST(1 AS bit)");
    }

    public override async Task Join_predicate_condition_equals_condition(bool async)
    {
        await base.Join_predicate_condition_equals_condition(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN [Weapons] AS [w] ON [w].[SynergyWithId] IS NOT NULL");
    }

    public override async Task Left_join_predicate_value_equals_condition(bool async)
    {
        await base.Left_join_predicate_value_equals_condition(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Weapons] AS [w] ON [w].[SynergyWithId] IS NOT NULL");
    }

    public override async Task Left_join_predicate_value(bool async)
    {
        await base.Left_join_predicate_value(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Weapons] AS [w] ON [t].[HasSoulPatch] = CAST(1 AS bit)");
    }

    public override async Task Left_join_predicate_condition_equals_condition(bool async)
    {
        await base.Left_join_predicate_condition_equals_condition(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Weapons] AS [w] ON [w].[SynergyWithId] IS NOT NULL");
    }

    public override async Task Where_datetimeoffset_now(bool async)
    {
        await base.Where_datetimeoffset_now(async);

        AssertSql(
            @"SELECT [m].[Id], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE [m].[Timeline] <> SYSDATETIMEOFFSET()");
    }

    public override async Task Where_datetimeoffset_utcnow(bool async)
    {
        await base.Where_datetimeoffset_utcnow(async);

        AssertSql(
            @"SELECT [m].[Id], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE [m].[Timeline] <> CAST(SYSUTCDATETIME() AS datetimeoffset)");
    }

    public override async Task Where_datetimeoffset_date_component(bool async)
    {
        await base.Where_datetimeoffset_date_component(async);

        AssertSql(
            @"@__Date_0='0001-01-01T00:00:00.0000000'

SELECT [m].[Id], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE CONVERT(date, [m].[Timeline]) > @__Date_0");
    }

    public override async Task Where_datetimeoffset_year_component(bool async)
    {
        await base.Where_datetimeoffset_year_component(async);

        AssertSql(
            @"SELECT [m].[Id], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(year, [m].[Timeline]) = 2");
    }

    public override async Task Where_datetimeoffset_month_component(bool async)
    {
        await base.Where_datetimeoffset_month_component(async);

        AssertSql(
            @"SELECT [m].[Id], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(month, [m].[Timeline]) = 1");
    }

    public override async Task Where_datetimeoffset_dayofyear_component(bool async)
    {
        await base.Where_datetimeoffset_dayofyear_component(async);

        AssertSql(
            @"SELECT [m].[Id], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(dayofyear, [m].[Timeline]) = 2");
    }

    public override async Task Where_datetimeoffset_day_component(bool async)
    {
        await base.Where_datetimeoffset_day_component(async);

        AssertSql(
            @"SELECT [m].[Id], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(day, [m].[Timeline]) = 2");
    }

    public override async Task Where_datetimeoffset_hour_component(bool async)
    {
        await base.Where_datetimeoffset_hour_component(async);

        AssertSql(
            @"SELECT [m].[Id], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(hour, [m].[Timeline]) = 10");
    }

    public override async Task Where_datetimeoffset_minute_component(bool async)
    {
        await base.Where_datetimeoffset_minute_component(async);

        AssertSql(
            @"SELECT [m].[Id], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(minute, [m].[Timeline]) = 0");
    }

    public override async Task Where_datetimeoffset_second_component(bool async)
    {
        await base.Where_datetimeoffset_second_component(async);

        AssertSql(
            @"SELECT [m].[Id], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(second, [m].[Timeline]) = 0");
    }

    public override async Task Where_datetimeoffset_millisecond_component(bool async)
    {
        await base.Where_datetimeoffset_millisecond_component(async);

        AssertSql(
            @"SELECT [m].[Id], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(millisecond, [m].[Timeline]) = 0");
    }

    public override async Task DateTimeOffset_DateAdd_AddMonths(bool async)
    {
        await base.DateTimeOffset_DateAdd_AddMonths(async);

        AssertSql(
            @"SELECT DATEADD(month, CAST(1 AS int), [m].[Timeline])
FROM [Missions] AS [m]");
    }

    public override async Task DateTimeOffset_DateAdd_AddDays(bool async)
    {
        await base.DateTimeOffset_DateAdd_AddDays(async);

        AssertSql(
            @"SELECT DATEADD(day, CAST(1.0E0 AS int), [m].[Timeline])
FROM [Missions] AS [m]");
    }

    public override async Task DateTimeOffset_DateAdd_AddHours(bool async)
    {
        await base.DateTimeOffset_DateAdd_AddHours(async);

        AssertSql(
            @"SELECT DATEADD(hour, CAST(1.0E0 AS int), [m].[Timeline])
FROM [Missions] AS [m]");
    }

    public override async Task DateTimeOffset_DateAdd_AddMinutes(bool async)
    {
        await base.DateTimeOffset_DateAdd_AddMinutes(async);

        AssertSql(
            @"SELECT DATEADD(minute, CAST(1.0E0 AS int), [m].[Timeline])
FROM [Missions] AS [m]");
    }

    public override async Task DateTimeOffset_DateAdd_AddSeconds(bool async)
    {
        await base.DateTimeOffset_DateAdd_AddSeconds(async);

        AssertSql(
            @"SELECT DATEADD(second, CAST(1.0E0 AS int), [m].[Timeline])
FROM [Missions] AS [m]");
    }

    public override async Task DateTimeOffset_DateAdd_AddMilliseconds(bool async)
    {
        await base.DateTimeOffset_DateAdd_AddMilliseconds(async);

        AssertSql(
            @"SELECT DATEADD(millisecond, CAST(300.0E0 AS int), [m].[Timeline])
FROM [Missions] AS [m]");
    }

    public override async Task Where_datetimeoffset_milliseconds_parameter_and_constant(bool async)
    {
        await base.Where_datetimeoffset_milliseconds_parameter_and_constant(async);

        AssertSql(
            @"SELECT COUNT(*)
FROM [Missions] AS [m]
WHERE [m].[Timeline] = '1902-01-02T10:00:00.1234567+01:30'");
    }

    public override async Task Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used(
        bool async)
    {
        await base.Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used(async);

        AssertSql();
    }

    public override async Task Complex_predicate_with_AndAlso_and_nullable_bool_property(bool async)
    {
        await base.Complex_predicate_with_AndAlso_and_nullable_bool_property(async);

        AssertSql(
            @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t] ON [w].[OwnerFullName] = [t].[FullName]
WHERE [w].[Id] <> 50 AND [t].[HasSoulPatch] = CAST(0 AS bit)");
    }

    public override async Task Distinct_with_optional_navigation_is_translated_to_sql(bool async)
    {
        await base.Distinct_with_optional_navigation_is_translated_to_sql(async);

        AssertSql(
            @"SELECT DISTINCT [t].[HasSoulPatch]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName] AND [t].[SquadId] = [t0].[GearSquadId]
WHERE [t0].[Note] <> N'Foo' OR [t0].[Note] IS NULL");
    }

    public override async Task Sum_with_optional_navigation_is_translated_to_sql(bool async)
    {
        await base.Sum_with_optional_navigation_is_translated_to_sql(async);

        AssertSql(
            @"SELECT COALESCE(SUM([t].[SquadId]), 0)
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName] AND [t].[SquadId] = [t0].[GearSquadId]
WHERE [t0].[Note] <> N'Foo' OR [t0].[Note] IS NULL");
    }

    public override async Task Count_with_optional_navigation_is_translated_to_sql(bool async)
    {
        await base.Count_with_optional_navigation_is_translated_to_sql(async);

        AssertSql(
            @"SELECT COUNT(*)
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName] AND [t].[SquadId] = [t0].[GearSquadId]
WHERE [t0].[Note] <> N'Foo' OR [t0].[Note] IS NULL");
    }

    public override async Task FirstOrDefault_with_manually_created_groupjoin_is_translated_to_sql(bool async)
    {
        await base.FirstOrDefault_with_manually_created_groupjoin_is_translated_to_sql(async);

        AssertSql(
            @"SELECT TOP(1) [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t] ON [s].[Id] = [t].[SquadId]
WHERE [s].[Name] = N'Kilo'");
    }

    public override async Task Any_with_optional_navigation_as_subquery_predicate_is_translated_to_sql(bool async)
    {
        await base.Any_with_optional_navigation_as_subquery_predicate_is_translated_to_sql(async);

        AssertSql(
            @"SELECT [s].[Name]
FROM [Squads] AS [s]
WHERE NOT (EXISTS (
    SELECT 1
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    LEFT JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName] AND [t].[SquadId] = [t0].[GearSquadId]
    WHERE [s].[Id] = [t].[SquadId] AND [t0].[Note] = N'Dom''s Tag'))");
    }

    public override async Task All_with_optional_navigation_is_translated_to_sql(bool async)
    {
        await base.All_with_optional_navigation_is_translated_to_sql(async);

        AssertSql(
            @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM (
            SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
            FROM [Gears] AS [g]
            UNION ALL
            SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
            FROM [Officers] AS [o]
        ) AS [t]
        LEFT JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName] AND [t].[SquadId] = [t0].[GearSquadId]
        WHERE [t0].[Note] = N'Foo' AND [t0].[Note] IS NOT NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
    }

    public override async Task Contains_with_local_nullable_guid_list_closure(bool async)
    {
        await base.Contains_with_local_nullable_guid_list_closure(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
WHERE [t].[Id] IN ('d2c26679-562b-44d1-ab96-23d1775e0926', '23cbcf9b-ce14-45cf-aafa-2c2667ebfdd3', 'ab1b82d7-88db-42bd-a132-7eef9aa68af4')");
    }

    public override async Task Unnecessary_include_doesnt_get_added_complex_when_projecting_EF_Property(bool async)
    {
        await base.Unnecessary_include_doesnt_get_added_complex_when_projecting_EF_Property(async);

        AssertSql(
            @"SELECT [t].[FullName]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit)
ORDER BY [t].[Rank]");
    }

    public override async Task Multiple_order_bys_are_properly_lifted_from_subquery_created_by_include(bool async)
    {
        await base.Multiple_order_bys_are_properly_lifted_from_subquery_created_by_include(async);

        AssertSql(
            @"SELECT [t].[FullName]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(0 AS bit)
ORDER BY [t].[FullName]");
    }

    public override async Task Order_by_is_properly_lifted_from_subquery_with_same_order_by_in_the_outer_query(bool async)
    {
        await base.Order_by_is_properly_lifted_from_subquery_with_same_order_by_in_the_outer_query(async);

        AssertSql(
            @"SELECT [t].[FullName]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(0 AS bit)
ORDER BY [t].[FullName]");
    }

    public override async Task Where_is_properly_lifted_from_subquery_created_by_include(bool async)
    {
        await base.Where_is_properly_lifted_from_subquery_created_by_include(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Id], [t0].[GearNickName], [t0].[GearSquadId], [t0].[IssueDate], [t0].[Note]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName] AND [t].[SquadId] = [t0].[GearSquadId]
WHERE [t].[FullName] <> N'Augustus Cole' AND [t].[HasSoulPatch] = CAST(0 AS bit)
ORDER BY [t].[FullName]");
    }

    public override async Task Subquery_is_lifted_from_main_from_clause_of_SelectMany(bool async)
    {
        await base.Subquery_is_lifted_from_main_from_clause_of_SelectMany(async);

        AssertSql(
            @"SELECT [t].[FullName] AS [Name1], [t0].[FullName] AS [Name2]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
CROSS JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit) AND [t0].[HasSoulPatch] = CAST(0 AS bit)
ORDER BY [t].[FullName]");
    }

    public override async Task Subquery_containing_SelectMany_projecting_main_from_clause_gets_lifted(bool async)
    {
        await base.Subquery_containing_SelectMany_projecting_main_from_clause_gets_lifted(async);

        AssertSql(
            @"SELECT [t].[FullName]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
CROSS JOIN [Tags] AS [t0]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit)
ORDER BY [t].[FullName]");
    }

    public override async Task Subquery_containing_join_projecting_main_from_clause_gets_lifted(bool async)
    {
        await base.Subquery_containing_join_projecting_main_from_clause_gets_lifted(async);

        AssertSql(
            @"SELECT [t].[Nickname]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName]
ORDER BY [t].[Nickname]");
    }

    public override async Task Subquery_containing_left_join_projecting_main_from_clause_gets_lifted(bool async)
    {
        await base.Subquery_containing_left_join_projecting_main_from_clause_gets_lifted(async);

        AssertSql(
            @"SELECT [t].[Nickname]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName]
ORDER BY [t].[Nickname]");
    }

    public override async Task Subquery_containing_join_gets_lifted_clashing_names(bool async)
    {
        await base.Subquery_containing_join_gets_lifted_clashing_names(async);

        AssertSql(
            @"SELECT [t].[Nickname]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName]
INNER JOIN [Tags] AS [t1] ON [t].[Nickname] = [t1].[GearNickName]
WHERE [t0].[GearNickName] <> N'Cole Train' OR [t0].[GearNickName] IS NULL
ORDER BY [t].[Nickname], [t1].[Id]");
    }

    public override async Task Subquery_created_by_include_gets_lifted_nested(bool async)
    {
        await base.Subquery_created_by_include_gets_lifted_nested(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [c].[Name], [c].[Location], [c].[Nation]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN [Cities] AS [c] ON [t].[CityOfBirthName] = [c].[Name]
WHERE EXISTS (
    SELECT 1
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName]) AND [t].[HasSoulPatch] = CAST(0 AS bit)
ORDER BY [t].[Nickname]");
    }

    public override async Task Subquery_is_lifted_from_additional_from_clause(bool async)
    {
        await base.Subquery_is_lifted_from_additional_from_clause(async);

        AssertSql(
            @"SELECT [t].[FullName] AS [Name1], [t0].[FullName] AS [Name2]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
CROSS JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit) AND [t0].[HasSoulPatch] = CAST(0 AS bit)
ORDER BY [t].[FullName]");
    }

    public override async Task Subquery_with_result_operator_is_not_lifted(bool async)
    {
        await base.Subquery_with_result_operator_is_not_lifted(async);

        AssertSql(
            @"@__p_0='2'

SELECT [t0].[FullName]
FROM (
    SELECT TOP(@__p_0) [t].[FullName], [t].[Rank]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    WHERE [t].[HasSoulPatch] = CAST(0 AS bit)
    ORDER BY [t].[FullName]
) AS [t0]
ORDER BY [t0].[Rank]");
    }

    public override async Task Skip_with_orderby_followed_by_orderBy_is_pushed_down(bool async)
    {
        await base.Skip_with_orderby_followed_by_orderBy_is_pushed_down(async);

        AssertSql(
            @"@__p_0='1'

SELECT [t0].[FullName]
FROM (
    SELECT [t].[FullName], [t].[Rank]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    WHERE [t].[HasSoulPatch] = CAST(0 AS bit)
    ORDER BY [t].[FullName]
    OFFSET @__p_0 ROWS
) AS [t0]
ORDER BY [t0].[Rank]");
    }

    public override async Task Take_without_orderby_followed_by_orderBy_is_pushed_down1(bool async)
    {
        await base.Take_without_orderby_followed_by_orderBy_is_pushed_down1(async);

        AssertSql(
            @"@__p_0='999'

SELECT [t0].[FullName]
FROM (
    SELECT TOP(@__p_0) [t].[FullName], [t].[Rank]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    WHERE [t].[HasSoulPatch] = CAST(0 AS bit)
) AS [t0]
ORDER BY [t0].[Rank]");
    }

    public override async Task Take_without_orderby_followed_by_orderBy_is_pushed_down2(bool async)
    {
        await base.Take_without_orderby_followed_by_orderBy_is_pushed_down2(async);

        AssertSql(
            @"@__p_0='999'

SELECT [t0].[FullName]
FROM (
    SELECT TOP(@__p_0) [t].[FullName], [t].[Rank]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    WHERE [t].[HasSoulPatch] = CAST(0 AS bit)
) AS [t0]
ORDER BY [t0].[Rank]");
    }

    public override async Task Take_without_orderby_followed_by_orderBy_is_pushed_down3(bool async)
    {
        await base.Take_without_orderby_followed_by_orderBy_is_pushed_down3(async);

        AssertSql(
            @"@__p_0='999'

SELECT [t0].[FullName]
FROM (
    SELECT TOP(@__p_0) [t].[FullName], [t].[Rank]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    WHERE [t].[HasSoulPatch] = CAST(0 AS bit)
) AS [t0]
ORDER BY [t0].[FullName], [t0].[Rank]");
    }

    public override async Task Select_length_of_string_property(bool async)
    {
        await base.Select_length_of_string_property(async);

        AssertSql(
            @"SELECT [w].[Name], CAST(LEN([w].[Name]) AS int) AS [Length]
FROM [Weapons] AS [w]");
    }

    public override async Task Client_method_on_collection_navigation_in_outer_join_key(bool async)
    {
        await base.Client_method_on_collection_navigation_in_outer_join_key(async);

        AssertSql();
    }

    public override async Task Member_access_on_derived_entity_using_cast(bool async)
    {
        await base.Member_access_on_derived_entity_using_cast(async);

        AssertSql(
            @"SELECT [l].[Name], [l].[Eradicated]
FROM [LocustHordes] AS [l]
ORDER BY [l].[Name]");
    }

    public override async Task Member_access_on_derived_materialized_entity_using_cast(bool async)
    {
        await base.Member_access_on_derived_materialized_entity_using_cast(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[CapitalName], [l].[Name], [l].[ServerAddress], [l].[CommanderName], [l].[Eradicated]
FROM [LocustHordes] AS [l]
ORDER BY [l].[Name]");
    }

    public override async Task Member_access_on_derived_entity_using_cast_and_let(bool async)
    {
        await base.Member_access_on_derived_entity_using_cast_and_let(async);

        AssertSql(
            @"SELECT [l].[Name], [l].[Eradicated]
FROM [LocustHordes] AS [l]
ORDER BY [l].[Name]");
    }

    public override async Task Property_access_on_derived_entity_using_cast(bool async)
    {
        await base.Property_access_on_derived_entity_using_cast(async);

        AssertSql(
            @"SELECT [l].[Name], [l].[Eradicated]
FROM [LocustHordes] AS [l]
ORDER BY [l].[Name]");
    }

    public override async Task Navigation_access_on_derived_entity_using_cast(bool async)
    {
        await base.Navigation_access_on_derived_entity_using_cast(async);

        AssertSql(
            @"SELECT [l].[Name], [l0].[ThreatLevel] AS [Threat]
FROM [LocustHordes] AS [l]
LEFT JOIN [LocustCommanders] AS [l0] ON [l].[CommanderName] = [l0].[Name]
ORDER BY [l].[Name]");
    }

    public override async Task Navigation_access_on_derived_materialized_entity_using_cast(bool async)
    {
        await base.Navigation_access_on_derived_materialized_entity_using_cast(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[CapitalName], [l].[Name], [l].[ServerAddress], [l].[CommanderName], [l].[Eradicated], [l0].[ThreatLevel] AS [Threat]
FROM [LocustHordes] AS [l]
LEFT JOIN [LocustCommanders] AS [l0] ON [l].[CommanderName] = [l0].[Name]
ORDER BY [l].[Name]");
    }

    public override async Task Navigation_access_via_EFProperty_on_derived_entity_using_cast(bool async)
    {
        await base.Navigation_access_via_EFProperty_on_derived_entity_using_cast(async);

        AssertSql(
            @"SELECT [l].[Name], [l0].[ThreatLevel] AS [Threat]
FROM [LocustHordes] AS [l]
LEFT JOIN [LocustCommanders] AS [l0] ON [l].[CommanderName] = [l0].[Name]
ORDER BY [l].[Name]");
    }

    public override async Task Navigation_access_fk_on_derived_entity_using_cast(bool async)
    {
        await base.Navigation_access_fk_on_derived_entity_using_cast(async);

        AssertSql(
            @"SELECT [l].[Name], [l0].[Name] AS [CommanderName]
FROM [LocustHordes] AS [l]
LEFT JOIN [LocustCommanders] AS [l0] ON [l].[CommanderName] = [l0].[Name]
ORDER BY [l].[Name]");
    }

    public override async Task Collection_navigation_access_on_derived_entity_using_cast(bool async)
    {
        await base.Collection_navigation_access_on_derived_entity_using_cast(async);

        AssertSql(
            @"SELECT [l].[Name], (
    SELECT COUNT(*)
    FROM (
        SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
        FROM [LocustLeaders] AS [l0]
        UNION ALL
        SELECT [l1].[Name], [l1].[LocustHordeId], [l1].[ThreatLevel], [l1].[ThreatLevelByte], [l1].[ThreatLevelNullableByte], [l1].[DefeatedByNickname], [l1].[DefeatedBySquadId], [l1].[HighCommandId], N'LocustCommander' AS [Discriminator]
        FROM [LocustCommanders] AS [l1]
    ) AS [t]
    WHERE [l].[Id] = [t].[LocustHordeId]) AS [LeadersCount]
FROM [LocustHordes] AS [l]
ORDER BY [l].[Name]");
    }

    public override async Task Collection_navigation_access_on_derived_entity_using_cast_in_SelectMany(bool async)
    {
        await base.Collection_navigation_access_on_derived_entity_using_cast_in_SelectMany(async);

        AssertSql(
            @"SELECT [l].[Name], [t].[Name] AS [LeaderName]
FROM [LocustHordes] AS [l]
INNER JOIN (
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l0]
    UNION ALL
    SELECT [l1].[Name], [l1].[LocustHordeId], [l1].[ThreatLevel], [l1].[ThreatLevelByte], [l1].[ThreatLevelNullableByte], [l1].[DefeatedByNickname], [l1].[DefeatedBySquadId], [l1].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l1]
) AS [t] ON [l].[Id] = [t].[LocustHordeId]
ORDER BY [t].[Name]");
    }

    public override async Task Include_on_derived_entity_using_OfType(bool async)
    {
        await base.Include_on_derived_entity_using_OfType(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[CapitalName], [l].[Name], [l].[ServerAddress], [l].[CommanderName], [l].[Eradicated], [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator]
FROM [LocustHordes] AS [l]
LEFT JOIN [LocustCommanders] AS [l0] ON [l].[CommanderName] = [l0].[Name]
LEFT JOIN (
    SELECT [l1].[Name], [l1].[LocustHordeId], [l1].[ThreatLevel], [l1].[ThreatLevelByte], [l1].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l1]
    UNION ALL
    SELECT [l2].[Name], [l2].[LocustHordeId], [l2].[ThreatLevel], [l2].[ThreatLevelByte], [l2].[ThreatLevelNullableByte], [l2].[DefeatedByNickname], [l2].[DefeatedBySquadId], [l2].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l2]
) AS [t] ON [l].[Id] = [t].[LocustHordeId]
ORDER BY [l].[Name], [l].[Id], [l0].[Name]");
    }

    public override async Task Distinct_on_subquery_doesnt_get_lifted(bool async)
    {
        await base.Distinct_on_subquery_doesnt_get_lifted(async);

        AssertSql(
            @"SELECT [t0].[HasSoulPatch]
FROM (
    SELECT DISTINCT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
) AS [t0]");
    }

    public override async Task Cast_result_operator_on_subquery_is_properly_lifted_to_a_convert(bool async)
    {
        await base.Cast_result_operator_on_subquery_is_properly_lifted_to_a_convert(async);

        AssertSql(
            @"SELECT [l].[Eradicated]
FROM [LocustHordes] AS [l]");
    }

    public override async Task Comparing_two_collection_navigations_composite_key(bool async)
    {
        await base.Comparing_two_collection_navigations_composite_key(async);

        AssertSql(
            @"SELECT [t].[Nickname] AS [Nickname1], [t0].[Nickname] AS [Nickname2]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
CROSS JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0]
WHERE [t].[Nickname] = [t0].[Nickname] AND [t].[SquadId] = [t0].[SquadId]
ORDER BY [t].[Nickname]");
    }

    public override async Task Comparing_two_collection_navigations_inheritance(bool async)
    {
        await base.Comparing_two_collection_navigations_inheritance(async);

        AssertSql(
            @"SELECT [l].[Name], [t0].[Nickname]
FROM [LocustHordes] AS [l]
CROSS JOIN (
    SELECT [t].[Nickname], [t].[SquadId], [t].[HasSoulPatch]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    WHERE [t].[Discriminator] = N'Officer'
) AS [t0]
LEFT JOIN [LocustCommanders] AS [l0] ON [l].[CommanderName] = [l0].[Name]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t1] ON [l0].[DefeatedByNickname] = [t1].[Nickname] AND [l0].[DefeatedBySquadId] = [t1].[SquadId]
WHERE [t0].[HasSoulPatch] = CAST(1 AS bit) AND [t1].[Nickname] = [t0].[Nickname] AND [t1].[SquadId] = [t0].[SquadId]");
    }

    public override async Task Comparing_entities_using_Equals_inheritance(bool async)
    {
        await base.Comparing_entities_using_Equals_inheritance(async);

        AssertSql(
            @"SELECT [t].[Nickname] AS [Nickname1], [t0].[Nickname] AS [Nickname2]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
CROSS JOIN (
    SELECT [t1].[Nickname], [t1].[SquadId]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t1]
    WHERE [t1].[Discriminator] = N'Officer'
) AS [t0]
WHERE [t].[Nickname] = [t0].[Nickname] AND [t].[SquadId] = [t0].[SquadId]
ORDER BY [t].[Nickname], [t0].[Nickname]");
    }

    public override async Task Contains_on_nullable_array_produces_correct_sql(bool async)
    {
        await base.Contains_on_nullable_array_produces_correct_sql(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Cities] AS [c] ON [t].[AssignedCityName] = [c].[Name]
WHERE [t].[SquadId] < 2 AND ([c].[Name] = N'Ephyra' OR [c].[Name] IS NULL)");
    }

    public override async Task Optional_navigation_with_collection_composite_key(bool async)
    {
        await base.Optional_navigation_with_collection_composite_key(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE [t0].[Discriminator] = N'Officer' AND (
    SELECT COUNT(*)
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t1]
    WHERE [t0].[Nickname] IS NOT NULL AND [t0].[SquadId] IS NOT NULL AND [t0].[Nickname] = [t1].[LeaderNickname] AND [t0].[SquadId] = [t1].[LeaderSquadId] AND [t1].[Nickname] = N'Dom') > 0");
    }

    public override async Task Select_null_conditional_with_inheritance(bool async)
    {
        await base.Select_null_conditional_with_inheritance(async);

        AssertSql(
            @"SELECT CASE
    WHEN [l].[CommanderName] IS NOT NULL THEN [l].[CommanderName]
    ELSE NULL
END
FROM [LocustHordes] AS [l]");
    }

    public override async Task Select_null_conditional_with_inheritance_negative(bool async)
    {
        await base.Select_null_conditional_with_inheritance_negative(async);

        AssertSql(
            @"SELECT CASE
    WHEN [l].[CommanderName] IS NOT NULL THEN [l].[Eradicated]
    ELSE NULL
END
FROM [LocustHordes] AS [l]");
    }

    public override async Task Project_collection_navigation_with_inheritance1(bool async)
    {
        await base.Project_collection_navigation_with_inheritance1(async);

        AssertSql(
            @"SELECT [l].[Id], [l0].[Name], [l1].[Id], [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator]
FROM [LocustHordes] AS [l]
LEFT JOIN [LocustCommanders] AS [l0] ON [l].[CommanderName] = [l0].[Name]
LEFT JOIN [LocustHordes] AS [l1] ON [l0].[Name] = [l1].[CommanderName]
LEFT JOIN (
    SELECT [l2].[Name], [l2].[LocustHordeId], [l2].[ThreatLevel], [l2].[ThreatLevelByte], [l2].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l2]
    UNION ALL
    SELECT [l3].[Name], [l3].[LocustHordeId], [l3].[ThreatLevel], [l3].[ThreatLevelByte], [l3].[ThreatLevelNullableByte], [l3].[DefeatedByNickname], [l3].[DefeatedBySquadId], [l3].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l3]
) AS [t] ON [l1].[Id] = [t].[LocustHordeId]
ORDER BY [l].[Id], [l0].[Name], [l1].[Id]");
    }

    public override async Task Project_collection_navigation_with_inheritance2(bool async)
    {
        await base.Project_collection_navigation_with_inheritance2(async);

        AssertSql(
            @"SELECT [l].[Id], [l0].[Name], [t].[Nickname], [t].[SquadId], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM [LocustHordes] AS [l]
LEFT JOIN [LocustCommanders] AS [l0] ON [l].[CommanderName] = [l0].[Name]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t] ON [l0].[DefeatedByNickname] = [t].[Nickname] AND [l0].[DefeatedBySquadId] = [t].[SquadId]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON ([t].[Nickname] = [t0].[LeaderNickname] OR ([t].[Nickname] IS NULL AND [t0].[LeaderNickname] IS NULL)) AND [t].[SquadId] = [t0].[LeaderSquadId]
ORDER BY [l].[Id], [l0].[Name], [t].[Nickname], [t].[SquadId], [t0].[Nickname]");
    }

    public override async Task Project_collection_navigation_with_inheritance3(bool async)
    {
        await base.Project_collection_navigation_with_inheritance3(async);

        AssertSql(
            @"SELECT [l].[Id], [l0].[Name], [t].[Nickname], [t].[SquadId], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM [LocustHordes] AS [l]
LEFT JOIN [LocustCommanders] AS [l0] ON [l].[CommanderName] = [l0].[Name]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t] ON [l0].[DefeatedByNickname] = [t].[Nickname] AND [l0].[DefeatedBySquadId] = [t].[SquadId]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON ([t].[Nickname] = [t0].[LeaderNickname] OR ([t].[Nickname] IS NULL AND [t0].[LeaderNickname] IS NULL)) AND [t].[SquadId] = [t0].[LeaderSquadId]
ORDER BY [l].[Id], [l0].[Name], [t].[Nickname], [t].[SquadId], [t0].[Nickname]");
    }

    public override async Task Include_reference_on_derived_type_using_string(bool async)
    {
        await base.Include_reference_on_derived_type_using_string(async);

        AssertSql(
            @"SELECT [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[DefeatedByNickname] = [t0].[Nickname] AND [t].[DefeatedBySquadId] = [t0].[SquadId]");
    }

    public override async Task Include_reference_on_derived_type_using_string_nested1(bool async)
    {
        await base.Include_reference_on_derived_type_using_string_nested1(async);

        AssertSql(
            @"SELECT [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator], [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[DefeatedByNickname] = [t0].[Nickname] AND [t].[DefeatedBySquadId] = [t0].[SquadId]
LEFT JOIN [Squads] AS [s] ON [t0].[SquadId] = [s].[Id]");
    }

    public override async Task Include_reference_on_derived_type_using_string_nested2(bool async)
    {
        await base.Include_reference_on_derived_type_using_string_nested2(async);

        AssertSql(
            @"SELECT [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator], [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator], [t1].[Name], [t1].[Location], [t1].[Nation]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[DefeatedByNickname] = [t0].[Nickname] AND [t].[DefeatedBySquadId] = [t0].[SquadId]
LEFT JOIN (
    SELECT [t2].[Nickname], [t2].[SquadId], [t2].[AssignedCityName], [t2].[CityOfBirthName], [t2].[FullName], [t2].[HasSoulPatch], [t2].[LeaderNickname], [t2].[LeaderSquadId], [t2].[Rank], [t2].[Discriminator], [c].[Name], [c].[Location], [c].[Nation]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t2]
    INNER JOIN [Cities] AS [c] ON [t2].[CityOfBirthName] = [c].[Name]
) AS [t1] ON ([t0].[Nickname] = [t1].[LeaderNickname] OR ([t0].[Nickname] IS NULL AND [t1].[LeaderNickname] IS NULL)) AND [t0].[SquadId] = [t1].[LeaderSquadId]
ORDER BY [t].[Name], [t0].[Nickname], [t0].[SquadId], [t1].[Nickname], [t1].[SquadId]");
    }

    public override async Task Include_reference_on_derived_type_using_lambda(bool async)
    {
        await base.Include_reference_on_derived_type_using_lambda(async);

        AssertSql(
            @"SELECT [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[DefeatedByNickname] = [t0].[Nickname] AND [t].[DefeatedBySquadId] = [t0].[SquadId]");
    }

    public override async Task Include_reference_on_derived_type_using_lambda_with_soft_cast(bool async)
    {
        await base.Include_reference_on_derived_type_using_lambda_with_soft_cast(async);

        AssertSql(
            @"SELECT [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[DefeatedByNickname] = [t0].[Nickname] AND [t].[DefeatedBySquadId] = [t0].[SquadId]");
    }

    public override async Task Include_reference_on_derived_type_using_lambda_with_tracking(bool async)
    {
        await base.Include_reference_on_derived_type_using_lambda_with_tracking(async);

        AssertSql(
            @"SELECT [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[DefeatedByNickname] = [t0].[Nickname] AND [t].[DefeatedBySquadId] = [t0].[SquadId]");
    }

    public override async Task Include_collection_on_derived_type_using_string(bool async)
    {
        await base.Include_collection_on_derived_type_using_string(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON [t].[Nickname] = [t0].[LeaderNickname] AND [t].[SquadId] = [t0].[LeaderSquadId]
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Nickname]");
    }

    public override async Task Include_collection_on_derived_type_using_lambda(bool async)
    {
        await base.Include_collection_on_derived_type_using_lambda(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON [t].[Nickname] = [t0].[LeaderNickname] AND [t].[SquadId] = [t0].[LeaderSquadId]
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Nickname]");
    }

    public override async Task Include_collection_on_derived_type_using_lambda_with_soft_cast(bool async)
    {
        await base.Include_collection_on_derived_type_using_lambda_with_soft_cast(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON [t].[Nickname] = [t0].[LeaderNickname] AND [t].[SquadId] = [t0].[LeaderSquadId]
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Nickname]");
    }

    public override async Task Include_base_navigation_on_derived_entity(bool async)
    {
        await base.Include_base_navigation_on_derived_entity(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Id], [t0].[GearNickName], [t0].[GearSquadId], [t0].[IssueDate], [t0].[Note], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName] AND [t].[SquadId] = [t0].[GearSquadId]
LEFT JOIN [Weapons] AS [w] ON [t].[FullName] = [w].[OwnerFullName]
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Id]");
    }

    public override async Task ThenInclude_collection_on_derived_after_base_reference(bool async)
    {
        await base.ThenInclude_collection_on_derived_after_base_reference(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
LEFT JOIN [Weapons] AS [w] ON [t0].[FullName] = [w].[OwnerFullName]
ORDER BY [t].[Id], [t0].[Nickname], [t0].[SquadId]");
    }

    public override async Task ThenInclude_collection_on_derived_after_derived_reference(bool async)
    {
        await base.ThenInclude_collection_on_derived_after_derived_reference(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[CapitalName], [l].[Name], [l].[ServerAddress], [l].[CommanderName], [l].[Eradicated], [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM [LocustHordes] AS [l]
LEFT JOIN [LocustCommanders] AS [l0] ON [l].[CommanderName] = [l0].[Name]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t] ON [l0].[DefeatedByNickname] = [t].[Nickname] AND [l0].[DefeatedBySquadId] = [t].[SquadId]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON ([t].[Nickname] = [t0].[LeaderNickname] OR ([t].[Nickname] IS NULL AND [t0].[LeaderNickname] IS NULL)) AND [t].[SquadId] = [t0].[LeaderSquadId]
ORDER BY [l].[Id], [l0].[Name], [t].[Nickname], [t].[SquadId], [t0].[Nickname]");
    }

    public override async Task ThenInclude_collection_on_derived_after_derived_collection(bool async)
    {
        await base.ThenInclude_collection_on_derived_after_derived_collection(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator], [t1].[Nickname0], [t1].[SquadId0], [t1].[AssignedCityName0], [t1].[CityOfBirthName0], [t1].[FullName0], [t1].[HasSoulPatch0], [t1].[LeaderNickname0], [t1].[LeaderSquadId0], [t1].[Rank0], [t1].[Discriminator0]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator], [t2].[Nickname] AS [Nickname0], [t2].[SquadId] AS [SquadId0], [t2].[AssignedCityName] AS [AssignedCityName0], [t2].[CityOfBirthName] AS [CityOfBirthName0], [t2].[FullName] AS [FullName0], [t2].[HasSoulPatch] AS [HasSoulPatch0], [t2].[LeaderNickname] AS [LeaderNickname0], [t2].[LeaderSquadId] AS [LeaderSquadId0], [t2].[Rank] AS [Rank0], [t2].[Discriminator] AS [Discriminator0]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t0]
    LEFT JOIN (
        SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOfBirthName], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g1]
        UNION ALL
        SELECT [o1].[Nickname], [o1].[SquadId], [o1].[AssignedCityName], [o1].[CityOfBirthName], [o1].[FullName], [o1].[HasSoulPatch], [o1].[LeaderNickname], [o1].[LeaderSquadId], [o1].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o1]
    ) AS [t2] ON [t0].[Nickname] = [t2].[LeaderNickname] AND [t0].[SquadId] = [t2].[LeaderSquadId]
) AS [t1] ON [t].[Nickname] = [t1].[LeaderNickname] AND [t].[SquadId] = [t1].[LeaderSquadId]
ORDER BY [t].[Nickname], [t].[SquadId], [t1].[Nickname], [t1].[SquadId], [t1].[Nickname0]");
    }

    public override async Task ThenInclude_reference_on_derived_after_derived_collection(bool async)
    {
        await base.ThenInclude_reference_on_derived_after_derived_collection(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[CapitalName], [l].[Name], [l].[ServerAddress], [l].[CommanderName], [l].[Eradicated], [t1].[Name], [t1].[LocustHordeId], [t1].[ThreatLevel], [t1].[ThreatLevelByte], [t1].[ThreatLevelNullableByte], [t1].[DefeatedByNickname], [t1].[DefeatedBySquadId], [t1].[HighCommandId], [t1].[Discriminator], [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator0]
FROM [LocustHordes] AS [l]
LEFT JOIN (
    SELECT [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator] AS [Discriminator0]
    FROM (
        SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
        FROM [LocustLeaders] AS [l0]
        UNION ALL
        SELECT [l1].[Name], [l1].[LocustHordeId], [l1].[ThreatLevel], [l1].[ThreatLevelByte], [l1].[ThreatLevelNullableByte], [l1].[DefeatedByNickname], [l1].[DefeatedBySquadId], [l1].[HighCommandId], N'LocustCommander' AS [Discriminator]
        FROM [LocustCommanders] AS [l1]
    ) AS [t]
    LEFT JOIN (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t0] ON [t].[DefeatedByNickname] = [t0].[Nickname] AND [t].[DefeatedBySquadId] = [t0].[SquadId]
) AS [t1] ON [l].[Id] = [t1].[LocustHordeId]
ORDER BY [l].[Id], [t1].[Name], [t1].[Nickname]");
    }

    public override async Task Multiple_derived_included_on_one_method(bool async)
    {
        await base.Multiple_derived_included_on_one_method(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[CapitalName], [l].[Name], [l].[ServerAddress], [l].[CommanderName], [l].[Eradicated], [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM [LocustHordes] AS [l]
LEFT JOIN [LocustCommanders] AS [l0] ON [l].[CommanderName] = [l0].[Name]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t] ON [l0].[DefeatedByNickname] = [t].[Nickname] AND [l0].[DefeatedBySquadId] = [t].[SquadId]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON ([t].[Nickname] = [t0].[LeaderNickname] OR ([t].[Nickname] IS NULL AND [t0].[LeaderNickname] IS NULL)) AND [t].[SquadId] = [t0].[LeaderSquadId]
ORDER BY [l].[Id], [l0].[Name], [t].[Nickname], [t].[SquadId], [t0].[Nickname]");
    }

    public override async Task Include_on_derived_multi_level(bool async)
    {
        await base.Include_on_derived_multi_level(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator], [t0].[Id], [t0].[Banner], [t0].[Banner5], [t0].[InternalNumber], [t0].[Name], [t0].[SquadId0], [t0].[MissionId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator], [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [s0].[SquadId] AS [SquadId0], [s0].[MissionId]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t1]
    INNER JOIN [Squads] AS [s] ON [t1].[SquadId] = [s].[Id]
    LEFT JOIN [SquadMissions] AS [s0] ON [s].[Id] = [s0].[SquadId]
) AS [t0] ON [t].[Nickname] = [t0].[LeaderNickname] AND [t].[SquadId] = [t0].[LeaderSquadId]
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Nickname], [t0].[SquadId], [t0].[Id], [t0].[SquadId0]");
    }

    public override async Task Projecting_nullable_bool_in_conditional_works(bool async)
    {
        await base.Projecting_nullable_bool_in_conditional_works(async);

        AssertSql(
            @"SELECT CASE
    WHEN [t0].[Nickname] IS NOT NULL AND [t0].[SquadId] IS NOT NULL THEN [t0].[HasSoulPatch]
    ELSE CAST(0 AS bit)
END AS [Prop]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]");
    }

    public override async Task Enum_ToString_is_client_eval(bool async)
    {
        await base.Enum_ToString_is_client_eval(async);

        AssertSql(
            @"SELECT [t].[Rank]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
ORDER BY [t].[SquadId], [t].[Nickname]");
    }

    public override async Task Correlated_collections_naked_navigation_with_ToList(bool async)
    {
        await base.Correlated_collections_naked_navigation_with_ToList(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Weapons] AS [w] ON [t].[FullName] = [w].[OwnerFullName]
WHERE [t].[Nickname] <> N'Marcus'
ORDER BY [t].[Nickname], [t].[SquadId]");
    }

    public override async Task Correlated_collections_naked_navigation_with_ToList_followed_by_projecting_count(bool async)
    {
        await base.Correlated_collections_naked_navigation_with_ToList_followed_by_projecting_count(async);

        AssertSql(
            @"SELECT (
    SELECT COUNT(*)
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName])
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[Nickname] <> N'Marcus'
ORDER BY [t].[Nickname]");
    }

    public override async Task Correlated_collections_naked_navigation_with_ToArray(bool async)
    {
        await base.Correlated_collections_naked_navigation_with_ToArray(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Weapons] AS [w] ON [t].[FullName] = [w].[OwnerFullName]
WHERE [t].[Nickname] <> N'Marcus'
ORDER BY [t].[Nickname], [t].[SquadId]");
    }

    public override async Task Correlated_collections_basic_projection(bool async)
    {
        await base.Correlated_collections_basic_projection(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
    FROM [Weapons] AS [w]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit) OR [w].[Name] <> N'foo' OR [w].[Name] IS NULL
) AS [t0] ON [t].[FullName] = [t0].[OwnerFullName]
WHERE [t].[Nickname] <> N'Marcus'
ORDER BY [t].[Nickname], [t].[SquadId]");
    }

    public override async Task Correlated_collections_basic_projection_explicit_to_list(bool async)
    {
        await base.Correlated_collections_basic_projection_explicit_to_list(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
    FROM [Weapons] AS [w]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit) OR [w].[Name] <> N'foo' OR [w].[Name] IS NULL
) AS [t0] ON [t].[FullName] = [t0].[OwnerFullName]
WHERE [t].[Nickname] <> N'Marcus'
ORDER BY [t].[Nickname], [t].[SquadId]");
    }

    public override async Task Correlated_collections_basic_projection_explicit_to_array(bool async)
    {
        await base.Correlated_collections_basic_projection_explicit_to_array(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
    FROM [Weapons] AS [w]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit) OR [w].[Name] <> N'foo' OR [w].[Name] IS NULL
) AS [t0] ON [t].[FullName] = [t0].[OwnerFullName]
WHERE [t].[Nickname] <> N'Marcus'
ORDER BY [t].[Nickname], [t].[SquadId]");
    }

    public override async Task Correlated_collections_basic_projection_ordered(bool async)
    {
        await base.Correlated_collections_basic_projection_ordered(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
    FROM [Weapons] AS [w]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit) OR [w].[Name] <> N'foo' OR [w].[Name] IS NULL
) AS [t0] ON [t].[FullName] = [t0].[OwnerFullName]
WHERE [t].[Nickname] <> N'Marcus'
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Name] DESC");
    }

    public override async Task Correlated_collections_basic_projection_composite_key(bool async)
    {
        await base.Correlated_collections_basic_projection_composite_key(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t0].[Nickname], [t0].[FullName], [t0].[SquadId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [t1].[Nickname], [t1].[FullName], [t1].[SquadId], [t1].[LeaderNickname], [t1].[LeaderSquadId]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t1]
    WHERE [t1].[HasSoulPatch] = CAST(0 AS bit)
) AS [t0] ON [t].[Nickname] = [t0].[LeaderNickname] AND [t].[SquadId] = [t0].[LeaderSquadId]
WHERE [t].[Discriminator] = N'Officer' AND [t].[Nickname] <> N'Foo'
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Nickname]");
    }

    public override async Task Correlated_collections_basic_projecting_single_property(bool async)
    {
        await base.Correlated_collections_basic_projecting_single_property(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t0].[Name], [t0].[Id]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [w].[Name], [w].[Id], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit) OR [w].[Name] <> N'foo' OR [w].[Name] IS NULL
) AS [t0] ON [t].[FullName] = [t0].[OwnerFullName]
WHERE [t].[Nickname] <> N'Marcus'
ORDER BY [t].[Nickname], [t].[SquadId]");
    }

    public override async Task Correlated_collections_basic_projecting_constant(bool async)
    {
        await base.Correlated_collections_basic_projecting_constant(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t0].[c], [t0].[Id]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT N'BFG' AS [c], [w].[Id], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit) OR [w].[Name] <> N'foo' OR [w].[Name] IS NULL
) AS [t0] ON [t].[FullName] = [t0].[OwnerFullName]
WHERE [t].[Nickname] <> N'Marcus'
ORDER BY [t].[Nickname], [t].[SquadId]");
    }

    public override async Task Correlated_collections_basic_projecting_constant_bool(bool async)
    {
        await base.Correlated_collections_basic_projecting_constant_bool(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t0].[c], [t0].[Id]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT CAST(1 AS bit) AS [c], [w].[Id], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit) OR [w].[Name] <> N'foo' OR [w].[Name] IS NULL
) AS [t0] ON [t].[FullName] = [t0].[OwnerFullName]
WHERE [t].[Nickname] <> N'Marcus'
ORDER BY [t].[Nickname], [t].[SquadId]");
    }

    public override async Task Correlated_collections_projection_of_collection_thru_navigation(bool async)
    {
        await base.Correlated_collections_projection_of_collection_thru_navigation(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [s].[Id], [t0].[SquadId], [t0].[MissionId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN [Squads] AS [s] ON [t].[SquadId] = [s].[Id]
LEFT JOIN (
    SELECT [s0].[SquadId], [s0].[MissionId]
    FROM [SquadMissions] AS [s0]
    WHERE [s0].[MissionId] <> 17
) AS [t0] ON [s].[Id] = [t0].[SquadId]
WHERE [t].[Nickname] <> N'Marcus'
ORDER BY [t].[FullName], [t].[Nickname], [t].[SquadId], [s].[Id], [t0].[SquadId]");
    }

    public override async Task Correlated_collections_project_anonymous_collection_result(bool async)
    {
        await base.Correlated_collections_project_anonymous_collection_result(async);

        AssertSql(
            @"SELECT [s].[Name], [s].[Id], [t].[FullName], [t].[Rank], [t].[Nickname], [t].[SquadId]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t] ON [s].[Id] = [t].[SquadId]
WHERE [s].[Id] < 20
ORDER BY [s].[Id], [t].[Nickname]");
    }

    public override async Task Correlated_collections_nested(bool async)
    {
        await base.Correlated_collections_nested(async);

        AssertSql(
            @"SELECT [s].[Id], [t0].[SquadId], [t0].[MissionId], [t0].[Id], [t0].[SquadId0], [t0].[MissionId0]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [s0].[SquadId], [s0].[MissionId], [m].[Id], [t].[SquadId] AS [SquadId0], [t].[MissionId] AS [MissionId0]
    FROM [SquadMissions] AS [s0]
    INNER JOIN [Missions] AS [m] ON [s0].[MissionId] = [m].[Id]
    LEFT JOIN (
        SELECT [s1].[SquadId], [s1].[MissionId]
        FROM [SquadMissions] AS [s1]
        WHERE [s1].[SquadId] < 7
    ) AS [t] ON [m].[Id] = [t].[MissionId]
    WHERE [s0].[MissionId] < 42
) AS [t0] ON [s].[Id] = [t0].[SquadId]
ORDER BY [s].[Id], [t0].[SquadId], [t0].[MissionId], [t0].[Id], [t0].[SquadId0]");
    }

    public override async Task Correlated_collections_nested_mixed_streaming_with_buffer1(bool async)
    {
        await base.Correlated_collections_nested_mixed_streaming_with_buffer1(async);

        AssertSql(
            @"SELECT [s].[Id], [t0].[SquadId], [t0].[MissionId], [t0].[Id], [t0].[SquadId0], [t0].[MissionId0]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [s0].[SquadId], [s0].[MissionId], [m].[Id], [t].[SquadId] AS [SquadId0], [t].[MissionId] AS [MissionId0]
    FROM [SquadMissions] AS [s0]
    INNER JOIN [Missions] AS [m] ON [s0].[MissionId] = [m].[Id]
    LEFT JOIN (
        SELECT [s1].[SquadId], [s1].[MissionId]
        FROM [SquadMissions] AS [s1]
        WHERE [s1].[SquadId] < 2
    ) AS [t] ON [m].[Id] = [t].[MissionId]
    WHERE [s0].[MissionId] < 3
) AS [t0] ON [s].[Id] = [t0].[SquadId]
ORDER BY [s].[Id], [t0].[SquadId], [t0].[MissionId], [t0].[Id], [t0].[SquadId0]");
    }

    public override async Task Correlated_collections_nested_mixed_streaming_with_buffer2(bool async)
    {
        await base.Correlated_collections_nested_mixed_streaming_with_buffer2(async);

        AssertSql(
            @"SELECT [s].[Id], [t0].[SquadId], [t0].[MissionId], [t0].[Id], [t0].[SquadId0], [t0].[MissionId0]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [s0].[SquadId], [s0].[MissionId], [m].[Id], [t].[SquadId] AS [SquadId0], [t].[MissionId] AS [MissionId0]
    FROM [SquadMissions] AS [s0]
    INNER JOIN [Missions] AS [m] ON [s0].[MissionId] = [m].[Id]
    LEFT JOIN (
        SELECT [s1].[SquadId], [s1].[MissionId]
        FROM [SquadMissions] AS [s1]
        WHERE [s1].[SquadId] < 7
    ) AS [t] ON [m].[Id] = [t].[MissionId]
    WHERE [s0].[MissionId] < 42
) AS [t0] ON [s].[Id] = [t0].[SquadId]
ORDER BY [s].[Id], [t0].[SquadId], [t0].[MissionId], [t0].[Id], [t0].[SquadId0]");
    }

    public override async Task Correlated_collections_nested_with_custom_ordering(bool async)
    {
        await base.Correlated_collections_nested_with_custom_ordering(async);

        AssertSql(
            @"SELECT [t].[FullName], [t].[Nickname], [t].[SquadId], [t1].[FullName], [t1].[Nickname], [t1].[SquadId], [t1].[Id], [t1].[AmmunitionType], [t1].[IsAutomatic], [t1].[Name], [t1].[OwnerFullName], [t1].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [t0].[FullName], [t0].[Nickname], [t0].[SquadId], [t2].[Id], [t2].[AmmunitionType], [t2].[IsAutomatic], [t2].[Name], [t2].[OwnerFullName], [t2].[SynergyWithId], [t0].[Rank], [t0].[LeaderNickname], [t0].[LeaderSquadId]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t0]
    LEFT JOIN (
        SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [w].[Name] <> N'Bar' OR [w].[Name] IS NULL
    ) AS [t2] ON [t0].[FullName] = [t2].[OwnerFullName]
    WHERE [t0].[FullName] <> N'Foo'
) AS [t1] ON [t].[Nickname] = [t1].[LeaderNickname] AND [t].[SquadId] = [t1].[LeaderSquadId]
WHERE [t].[Discriminator] = N'Officer'
ORDER BY [t].[HasSoulPatch] DESC, [t].[Nickname], [t].[SquadId], [t1].[Rank], [t1].[Nickname], [t1].[SquadId], [t1].[IsAutomatic]");
    }

    public override async Task Correlated_collections_same_collection_projected_multiple_times(bool async)
    {
        await base.Correlated_collections_same_collection_projected_multiple_times(async);

        AssertSql(
            @"SELECT [t].[FullName], [t].[Nickname], [t].[SquadId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[SynergyWithId], [t1].[Id], [t1].[AmmunitionType], [t1].[IsAutomatic], [t1].[Name], [t1].[OwnerFullName], [t1].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
    FROM [Weapons] AS [w]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit)
) AS [t0] ON [t].[FullName] = [t0].[OwnerFullName]
LEFT JOIN (
    SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
    FROM [Weapons] AS [w0]
    WHERE [w0].[IsAutomatic] = CAST(1 AS bit)
) AS [t1] ON [t].[FullName] = [t1].[OwnerFullName]
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Id]");
    }

    public override async Task Correlated_collections_similar_collection_projected_multiple_times(bool async)
    {
        await base.Correlated_collections_similar_collection_projected_multiple_times(async);

        AssertSql(
            @"SELECT [t].[FullName], [t].[Nickname], [t].[SquadId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[SynergyWithId], [t1].[Id], [t1].[AmmunitionType], [t1].[IsAutomatic], [t1].[Name], [t1].[OwnerFullName], [t1].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
    FROM [Weapons] AS [w]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit)
) AS [t0] ON [t].[FullName] = [t0].[OwnerFullName]
LEFT JOIN (
    SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
    FROM [Weapons] AS [w0]
    WHERE [w0].[IsAutomatic] = CAST(0 AS bit)
) AS [t1] ON [t].[FullName] = [t1].[OwnerFullName]
ORDER BY [t].[Rank], [t].[Nickname], [t].[SquadId], [t0].[OwnerFullName], [t0].[Id], [t1].[IsAutomatic]");
    }

    public override async Task Correlated_collections_different_collections_projected(bool async)
    {
        await base.Correlated_collections_different_collections_projected(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t0].[Name], [t0].[IsAutomatic], [t0].[Id], [t1].[Nickname], [t1].[Rank], [t1].[SquadId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [w].[Name], [w].[IsAutomatic], [w].[Id], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit)
) AS [t0] ON [t].[FullName] = [t0].[OwnerFullName]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t1] ON [t].[Nickname] = [t1].[LeaderNickname] AND [t].[SquadId] = [t1].[LeaderSquadId]
WHERE [t].[Discriminator] = N'Officer'
ORDER BY [t].[FullName], [t].[Nickname], [t].[SquadId], [t0].[Id], [t1].[FullName], [t1].[Nickname]");
    }

    public override async Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys(bool async)
    {
        await base.Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys(async);

        AssertSql(
            @"SELECT [t].[FullName]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName] AND [t].[SquadId] = [t0].[GearSquadId]
WHERE [t].[Discriminator] = N'Officer' AND EXISTS (
    SELECT 1
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t1]
    WHERE [t].[Nickname] = [t1].[LeaderNickname] AND [t].[SquadId] = [t1].[LeaderSquadId])
ORDER BY [t].[HasSoulPatch] DESC, [t0].[Note]");
    }

    public override async Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery(bool async)
    {
        await base.Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery(async);

        AssertSql(
            @"SELECT [t].[FullName], [t].[Nickname], [t].[SquadId], [t0].[Id], [t2].[Nickname], [t2].[SquadId], [t3].[Id], [t3].[AmmunitionType], [t3].[IsAutomatic], [t3].[Name], [t3].[OwnerFullName], [t3].[SynergyWithId], [t3].[Nickname], [t3].[SquadId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName] AND [t].[SquadId] = [t0].[GearSquadId]
LEFT JOIN (
    SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOfBirthName], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g1]
    UNION ALL
    SELECT [o1].[Nickname], [o1].[SquadId], [o1].[AssignedCityName], [o1].[CityOfBirthName], [o1].[FullName], [o1].[HasSoulPatch], [o1].[LeaderNickname], [o1].[LeaderSquadId], [o1].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o1]
) AS [t2] ON [t0].[GearNickName] = [t2].[Nickname] AND [t0].[GearSquadId] = [t2].[SquadId]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [t4].[Nickname], [t4].[SquadId]
    FROM [Weapons] AS [w]
    LEFT JOIN (
        SELECT [g2].[Nickname], [g2].[SquadId], [g2].[AssignedCityName], [g2].[CityOfBirthName], [g2].[FullName], [g2].[HasSoulPatch], [g2].[LeaderNickname], [g2].[LeaderSquadId], [g2].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g2]
        UNION ALL
        SELECT [o2].[Nickname], [o2].[SquadId], [o2].[AssignedCityName], [o2].[CityOfBirthName], [o2].[FullName], [o2].[HasSoulPatch], [o2].[LeaderNickname], [o2].[LeaderSquadId], [o2].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o2]
    ) AS [t4] ON [w].[OwnerFullName] = [t4].[FullName]
) AS [t3] ON [t2].[FullName] = [t3].[OwnerFullName]
WHERE [t].[Discriminator] = N'Officer' AND EXISTS (
    SELECT 1
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t1]
    WHERE [t].[Nickname] = [t1].[LeaderNickname] AND [t].[SquadId] = [t1].[LeaderSquadId])
ORDER BY [t].[HasSoulPatch] DESC, [t0].[Note], [t].[Nickname], [t].[SquadId], [t0].[Id], [t2].[Nickname], [t2].[SquadId], [t3].[IsAutomatic], [t3].[Nickname] DESC, [t3].[Id]");
    }

    public override async Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery_duplicated_orderings(
        bool async)
    {
        await base.Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery_duplicated_orderings(async);

        AssertSql(
            @"SELECT [t].[FullName], [t].[Nickname], [t].[SquadId], [t0].[Id], [t2].[Nickname], [t2].[SquadId], [t3].[Id], [t3].[AmmunitionType], [t3].[IsAutomatic], [t3].[Name], [t3].[OwnerFullName], [t3].[SynergyWithId], [t3].[Nickname], [t3].[SquadId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName] AND [t].[SquadId] = [t0].[GearSquadId]
LEFT JOIN (
    SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOfBirthName], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g1]
    UNION ALL
    SELECT [o1].[Nickname], [o1].[SquadId], [o1].[AssignedCityName], [o1].[CityOfBirthName], [o1].[FullName], [o1].[HasSoulPatch], [o1].[LeaderNickname], [o1].[LeaderSquadId], [o1].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o1]
) AS [t2] ON [t0].[GearNickName] = [t2].[Nickname] AND [t0].[GearSquadId] = [t2].[SquadId]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [t4].[Nickname], [t4].[SquadId]
    FROM [Weapons] AS [w]
    LEFT JOIN (
        SELECT [g2].[Nickname], [g2].[SquadId], [g2].[AssignedCityName], [g2].[CityOfBirthName], [g2].[FullName], [g2].[HasSoulPatch], [g2].[LeaderNickname], [g2].[LeaderSquadId], [g2].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g2]
        UNION ALL
        SELECT [o2].[Nickname], [o2].[SquadId], [o2].[AssignedCityName], [o2].[CityOfBirthName], [o2].[FullName], [o2].[HasSoulPatch], [o2].[LeaderNickname], [o2].[LeaderSquadId], [o2].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o2]
    ) AS [t4] ON [w].[OwnerFullName] = [t4].[FullName]
) AS [t3] ON [t2].[FullName] = [t3].[OwnerFullName]
WHERE [t].[Discriminator] = N'Officer' AND EXISTS (
    SELECT 1
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t1]
    WHERE [t].[Nickname] = [t1].[LeaderNickname] AND [t].[SquadId] = [t1].[LeaderSquadId])
ORDER BY [t].[HasSoulPatch] DESC, [t0].[Note], [t].[Nickname], [t].[SquadId], [t0].[Id], [t2].[Nickname], [t2].[SquadId], [t3].[IsAutomatic], [t3].[Nickname] DESC, [t3].[Id]");
    }

    public override async Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery_complex_orderings(
        bool async)
    {
        await base.Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery_complex_orderings(async);

        AssertSql(
            @"SELECT [t].[FullName], [t].[Nickname], [t].[SquadId], [t0].[Id], [t2].[Nickname], [t2].[SquadId], [t3].[Id], [t3].[AmmunitionType], [t3].[IsAutomatic], [t3].[Name], [t3].[OwnerFullName], [t3].[SynergyWithId], [t3].[Nickname], [t3].[SquadId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName] AND [t].[SquadId] = [t0].[GearSquadId]
LEFT JOIN (
    SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOfBirthName], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g1]
    UNION ALL
    SELECT [o1].[Nickname], [o1].[SquadId], [o1].[AssignedCityName], [o1].[CityOfBirthName], [o1].[FullName], [o1].[HasSoulPatch], [o1].[LeaderNickname], [o1].[LeaderSquadId], [o1].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o1]
) AS [t2] ON [t0].[GearNickName] = [t2].[Nickname] AND [t0].[GearSquadId] = [t2].[SquadId]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [t4].[Nickname], [t4].[SquadId], (
        SELECT COUNT(*)
        FROM [Weapons] AS [w0]
        WHERE [t4].[FullName] IS NOT NULL AND [t4].[FullName] = [w0].[OwnerFullName]) AS [c]
    FROM [Weapons] AS [w]
    LEFT JOIN (
        SELECT [g2].[Nickname], [g2].[SquadId], [g2].[AssignedCityName], [g2].[CityOfBirthName], [g2].[FullName], [g2].[HasSoulPatch], [g2].[LeaderNickname], [g2].[LeaderSquadId], [g2].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g2]
        UNION ALL
        SELECT [o2].[Nickname], [o2].[SquadId], [o2].[AssignedCityName], [o2].[CityOfBirthName], [o2].[FullName], [o2].[HasSoulPatch], [o2].[LeaderNickname], [o2].[LeaderSquadId], [o2].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o2]
    ) AS [t4] ON [w].[OwnerFullName] = [t4].[FullName]
) AS [t3] ON [t2].[FullName] = [t3].[OwnerFullName]
WHERE [t].[Discriminator] = N'Officer' AND EXISTS (
    SELECT 1
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t1]
    WHERE [t].[Nickname] = [t1].[LeaderNickname] AND [t].[SquadId] = [t1].[LeaderSquadId])
ORDER BY [t].[HasSoulPatch] DESC, [t0].[Note], [t].[Nickname], [t].[SquadId], [t0].[Id], [t2].[Nickname], [t2].[SquadId], [t3].[Id] DESC, [t3].[c], [t3].[Nickname]");
    }

    public override async Task Correlated_collections_multiple_nested_complex_collections(bool async)
    {
        await base.Correlated_collections_multiple_nested_complex_collections(async);

        AssertSql(
            @"SELECT [t].[FullName], [t].[Nickname], [t].[SquadId], [t0].[Id], [t2].[Nickname], [t2].[SquadId], [t3].[FullName], [t3].[Nickname], [t3].[SquadId], [t3].[Id], [t3].[Nickname0], [t3].[SquadId0], [t3].[Id0], [t3].[Name], [t3].[IsAutomatic], [t3].[Id1], [t3].[Nickname00], [t3].[HasSoulPatch], [t3].[SquadId00], [t8].[Id], [t8].[AmmunitionType], [t8].[IsAutomatic], [t8].[Name], [t8].[OwnerFullName], [t8].[SynergyWithId], [t8].[Nickname], [t8].[SquadId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName] AND [t].[SquadId] = [t0].[GearSquadId]
LEFT JOIN (
    SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOfBirthName], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g1]
    UNION ALL
    SELECT [o1].[Nickname], [o1].[SquadId], [o1].[AssignedCityName], [o1].[CityOfBirthName], [o1].[FullName], [o1].[HasSoulPatch], [o1].[LeaderNickname], [o1].[LeaderSquadId], [o1].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o1]
) AS [t2] ON [t0].[GearNickName] = [t2].[Nickname] AND [t0].[GearSquadId] = [t2].[SquadId]
LEFT JOIN (
    SELECT [t4].[FullName], [t4].[Nickname], [t4].[SquadId], [t5].[Id], [t5].[Nickname] AS [Nickname0], [t5].[SquadId] AS [SquadId0], [t5].[Id0], [t5].[Name], [t5].[IsAutomatic], [t5].[Id1], [t5].[Nickname0] AS [Nickname00], [t5].[HasSoulPatch], [t5].[SquadId0] AS [SquadId00], [t4].[Rank], [t5].[IsAutomatic0], [t4].[LeaderNickname], [t4].[LeaderSquadId]
    FROM (
        SELECT [g2].[Nickname], [g2].[SquadId], [g2].[AssignedCityName], [g2].[CityOfBirthName], [g2].[FullName], [g2].[HasSoulPatch], [g2].[LeaderNickname], [g2].[LeaderSquadId], [g2].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g2]
        UNION ALL
        SELECT [o2].[Nickname], [o2].[SquadId], [o2].[AssignedCityName], [o2].[CityOfBirthName], [o2].[FullName], [o2].[HasSoulPatch], [o2].[LeaderNickname], [o2].[LeaderSquadId], [o2].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o2]
    ) AS [t4]
    LEFT JOIN (
        SELECT [w].[Id], [t6].[Nickname], [t6].[SquadId], [s].[Id] AS [Id0], [w0].[Name], [w0].[IsAutomatic], [w0].[Id] AS [Id1], [t7].[Nickname] AS [Nickname0], [t7].[HasSoulPatch], [t7].[SquadId] AS [SquadId0], [w].[IsAutomatic] AS [IsAutomatic0], [w].[OwnerFullName]
        FROM [Weapons] AS [w]
        LEFT JOIN (
            SELECT [g3].[Nickname], [g3].[SquadId], [g3].[AssignedCityName], [g3].[CityOfBirthName], [g3].[FullName], [g3].[HasSoulPatch], [g3].[LeaderNickname], [g3].[LeaderSquadId], [g3].[Rank], N'Gear' AS [Discriminator]
            FROM [Gears] AS [g3]
            UNION ALL
            SELECT [o3].[Nickname], [o3].[SquadId], [o3].[AssignedCityName], [o3].[CityOfBirthName], [o3].[FullName], [o3].[HasSoulPatch], [o3].[LeaderNickname], [o3].[LeaderSquadId], [o3].[Rank], N'Officer' AS [Discriminator]
            FROM [Officers] AS [o3]
        ) AS [t6] ON [w].[OwnerFullName] = [t6].[FullName]
        LEFT JOIN [Squads] AS [s] ON [t6].[SquadId] = [s].[Id]
        LEFT JOIN [Weapons] AS [w0] ON [t6].[FullName] = [w0].[OwnerFullName]
        LEFT JOIN (
            SELECT [g4].[Nickname], [g4].[SquadId], [g4].[AssignedCityName], [g4].[CityOfBirthName], [g4].[FullName], [g4].[HasSoulPatch], [g4].[LeaderNickname], [g4].[LeaderSquadId], [g4].[Rank], N'Gear' AS [Discriminator]
            FROM [Gears] AS [g4]
            UNION ALL
            SELECT [o4].[Nickname], [o4].[SquadId], [o4].[AssignedCityName], [o4].[CityOfBirthName], [o4].[FullName], [o4].[HasSoulPatch], [o4].[LeaderNickname], [o4].[LeaderSquadId], [o4].[Rank], N'Officer' AS [Discriminator]
            FROM [Officers] AS [o4]
        ) AS [t7] ON [s].[Id] = [t7].[SquadId]
        WHERE [w].[Name] <> N'Bar' OR [w].[Name] IS NULL
    ) AS [t5] ON [t4].[FullName] = [t5].[OwnerFullName]
    WHERE [t4].[FullName] <> N'Foo'
) AS [t3] ON [t].[Nickname] = [t3].[LeaderNickname] AND [t].[SquadId] = [t3].[LeaderSquadId]
LEFT JOIN (
    SELECT [w1].[Id], [w1].[AmmunitionType], [w1].[IsAutomatic], [w1].[Name], [w1].[OwnerFullName], [w1].[SynergyWithId], [t9].[Nickname], [t9].[SquadId]
    FROM [Weapons] AS [w1]
    LEFT JOIN (
        SELECT [g5].[Nickname], [g5].[SquadId], [g5].[AssignedCityName], [g5].[CityOfBirthName], [g5].[FullName], [g5].[HasSoulPatch], [g5].[LeaderNickname], [g5].[LeaderSquadId], [g5].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g5]
        UNION ALL
        SELECT [o5].[Nickname], [o5].[SquadId], [o5].[AssignedCityName], [o5].[CityOfBirthName], [o5].[FullName], [o5].[HasSoulPatch], [o5].[LeaderNickname], [o5].[LeaderSquadId], [o5].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o5]
    ) AS [t9] ON [w1].[OwnerFullName] = [t9].[FullName]
) AS [t8] ON [t2].[FullName] = [t8].[OwnerFullName]
WHERE [t].[Discriminator] = N'Officer' AND EXISTS (
    SELECT 1
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t1]
    WHERE [t].[Nickname] = [t1].[LeaderNickname] AND [t].[SquadId] = [t1].[LeaderSquadId])
ORDER BY [t].[HasSoulPatch] DESC, [t0].[Note], [t].[Nickname], [t].[SquadId], [t0].[Id], [t2].[Nickname], [t2].[SquadId], [t3].[Rank], [t3].[Nickname], [t3].[SquadId], [t3].[IsAutomatic0], [t3].[Id], [t3].[Nickname0], [t3].[SquadId0], [t3].[Id0], [t3].[Id1], [t3].[Nickname00], [t3].[SquadId00], [t8].[IsAutomatic], [t8].[Nickname] DESC, [t8].[Id]");
    }

    public override async Task Correlated_collections_inner_subquery_selector_references_outer_qsre(bool async)
    {
        await base.Correlated_collections_inner_subquery_selector_references_outer_qsre(async);

        AssertSql(
            @"SELECT [t].[FullName], [t].[Nickname], [t].[SquadId], [t0].[ReportName], [t0].[OfficerName], [t0].[Nickname], [t0].[SquadId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
OUTER APPLY (
    SELECT [t1].[FullName] AS [ReportName], [t].[FullName] AS [OfficerName], [t1].[Nickname], [t1].[SquadId]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t1]
    WHERE [t].[Nickname] = [t1].[LeaderNickname] AND [t].[SquadId] = [t1].[LeaderSquadId]
) AS [t0]
WHERE [t].[Discriminator] = N'Officer'
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Nickname]");
    }

    public override async Task Correlated_collections_inner_subquery_predicate_references_outer_qsre(bool async)
    {
        await base.Correlated_collections_inner_subquery_predicate_references_outer_qsre(async);

        AssertSql(
            @"SELECT [t].[FullName], [t].[Nickname], [t].[SquadId], [t0].[ReportName], [t0].[Nickname], [t0].[SquadId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
OUTER APPLY (
    SELECT [t1].[FullName] AS [ReportName], [t1].[Nickname], [t1].[SquadId]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t1]
    WHERE [t].[Nickname] = [t1].[LeaderNickname] AND [t].[SquadId] = [t1].[LeaderSquadId] AND [t].[FullName] <> N'Foo'
) AS [t0]
WHERE [t].[Discriminator] = N'Officer'
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Nickname]");
    }

    public override async Task Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up(bool async)
    {
        await base.Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up(async);

        AssertSql(
            @"SELECT [t].[FullName], [t].[Nickname], [t].[SquadId], [t1].[FullName], [t1].[Nickname], [t1].[SquadId], [t1].[Name], [t1].[Nickname0], [t1].[Id]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [t0].[FullName], [t0].[Nickname], [t0].[SquadId], [t2].[Name], [t2].[Nickname] AS [Nickname0], [t2].[Id], [t0].[LeaderNickname], [t0].[LeaderSquadId]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t0]
    OUTER APPLY (
        SELECT [w].[Name], [t0].[Nickname], [w].[Id]
        FROM [Weapons] AS [w]
        WHERE [t0].[FullName] = [w].[OwnerFullName] AND ([w].[Name] <> N'Bar' OR [w].[Name] IS NULL)
    ) AS [t2]
    WHERE [t0].[FullName] <> N'Foo'
) AS [t1] ON [t].[Nickname] = [t1].[LeaderNickname] AND [t].[SquadId] = [t1].[LeaderSquadId]
WHERE [t].[Discriminator] = N'Officer'
ORDER BY [t].[Nickname], [t].[SquadId], [t1].[Nickname], [t1].[SquadId]");
    }

    public override async Task Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up(bool async)
    {
        await base.Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up(async);

        AssertSql(
            @"SELECT [t].[FullName], [t].[Nickname], [t].[SquadId], [t1].[FullName], [t1].[Nickname], [t1].[SquadId], [t1].[Name], [t1].[Nickname0], [t1].[Id]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
OUTER APPLY (
    SELECT [t0].[FullName], [t0].[Nickname], [t0].[SquadId], [t2].[Name], [t2].[Nickname] AS [Nickname0], [t2].[Id]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t0]
    LEFT JOIN (
        SELECT [w].[Name], [t].[Nickname], [w].[Id], [w].[OwnerFullName]
        FROM [Weapons] AS [w]
        WHERE [w].[Name] <> N'Bar' OR [w].[Name] IS NULL
    ) AS [t2] ON [t0].[FullName] = [t2].[OwnerFullName]
    WHERE [t].[Nickname] = [t0].[LeaderNickname] AND [t].[SquadId] = [t0].[LeaderSquadId] AND [t0].[FullName] <> N'Foo'
) AS [t1]
WHERE [t].[Discriminator] = N'Officer'
ORDER BY [t].[Nickname], [t].[SquadId], [t1].[Nickname], [t1].[SquadId]");
    }

    public override async Task Correlated_collections_on_select_many(bool async)
    {
        await base.Correlated_collections_on_select_many(async);

        AssertSql(
            @"SELECT [t].[Nickname], [s].[Name], [t].[SquadId], [s].[Id], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[SynergyWithId], [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
CROSS JOIN [Squads] AS [s]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
    FROM [Weapons] AS [w]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit) OR [w].[Name] <> N'foo' OR [w].[Name] IS NULL
) AS [t0] ON [t].[FullName] = [t0].[OwnerFullName]
LEFT JOIN (
    SELECT [t2].[Nickname], [t2].[SquadId], [t2].[AssignedCityName], [t2].[CityOfBirthName], [t2].[FullName], [t2].[HasSoulPatch], [t2].[LeaderNickname], [t2].[LeaderSquadId], [t2].[Rank], [t2].[Discriminator]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t2]
    WHERE [t2].[HasSoulPatch] = CAST(0 AS bit)
) AS [t1] ON [s].[Id] = [t1].[SquadId]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit)
ORDER BY [t].[Nickname], [s].[Id] DESC, [t].[SquadId], [t0].[Id], [t1].[Nickname]");
    }

    public override async Task Correlated_collections_with_Skip(bool async)
    {
        await base.Correlated_collections_with_Skip(async);

        AssertSql(
            @"SELECT [s].[Id], [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
    FROM (
        SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], ROW_NUMBER() OVER(PARTITION BY [t].[SquadId] ORDER BY [t].[Nickname]) AS [row]
        FROM (
            SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
            FROM [Gears] AS [g]
            UNION ALL
            SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
            FROM [Officers] AS [o]
        ) AS [t]
    ) AS [t0]
    WHERE 1 < [t0].[row]
) AS [t1] ON [s].[Id] = [t1].[SquadId]
ORDER BY [s].[Name], [s].[Id], [t1].[SquadId], [t1].[Nickname]");
    }

    public override async Task Correlated_collections_with_Take(bool async)
    {
        await base.Correlated_collections_with_Take(async);

        AssertSql(
            @"SELECT [s].[Id], [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
    FROM (
        SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], ROW_NUMBER() OVER(PARTITION BY [t].[SquadId] ORDER BY [t].[Nickname]) AS [row]
        FROM (
            SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
            FROM [Gears] AS [g]
            UNION ALL
            SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
            FROM [Officers] AS [o]
        ) AS [t]
    ) AS [t0]
    WHERE [t0].[row] <= 2
) AS [t1] ON [s].[Id] = [t1].[SquadId]
ORDER BY [s].[Name], [s].[Id], [t1].[SquadId], [t1].[Nickname]");
    }

    public override async Task Correlated_collections_with_Distinct(bool async)
    {
        await base.Correlated_collections_with_Distinct(async);

        AssertSql(
            @"SELECT [s].[Id], [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator]
FROM [Squads] AS [s]
OUTER APPLY (
    SELECT DISTINCT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
    FROM (
        SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
        FROM (
            SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
            FROM [Gears] AS [g]
            UNION ALL
            SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
            FROM [Officers] AS [o]
        ) AS [t]
        WHERE [s].[Id] = [t].[SquadId]
        ORDER BY [t].[Nickname]
        OFFSET 0 ROWS
    ) AS [t0]
) AS [t1]
ORDER BY [s].[Name], [s].[Id], [t1].[Nickname]");
    }

    public override async Task Correlated_collections_with_FirstOrDefault(bool async)
    {
        await base.Correlated_collections_with_FirstOrDefault(async);

        AssertSql(
            @"SELECT (
    SELECT TOP(1) [t].[FullName]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    WHERE [s].[Id] = [t].[SquadId]
    ORDER BY [t].[Nickname])
FROM [Squads] AS [s]
ORDER BY [s].[Name]");
    }

    public override async Task Correlated_collections_on_left_join_with_predicate(bool async)
    {
        await base.Correlated_collections_on_left_join_with_predicate(async);

        AssertSql(
            @"SELECT [t0].[Nickname], [t].[Id], [t0].[SquadId], [w].[Name], [w].[Id]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname]
LEFT JOIN [Weapons] AS [w] ON [t0].[FullName] = [w].[OwnerFullName]
WHERE [t0].[HasSoulPatch] = CAST(0 AS bit)
ORDER BY [t].[Id], [t0].[Nickname], [t0].[SquadId]");
    }

    public override async Task Correlated_collections_on_left_join_with_null_value(bool async)
    {
        await base.Correlated_collections_on_left_join_with_null_value(async);

        AssertSql(
            @"SELECT [t].[Id], [t0].[Nickname], [t0].[SquadId], [w].[Name], [w].[Id]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname]
LEFT JOIN [Weapons] AS [w] ON [t0].[FullName] = [w].[OwnerFullName]
ORDER BY [t].[Note], [t].[Id], [t0].[Nickname], [t0].[SquadId]");
    }

    public override async Task Correlated_collections_left_join_with_self_reference(bool async)
    {
        await base.Correlated_collections_left_join_with_self_reference(async);

        AssertSql(
            @"SELECT [t].[Note], [t].[Id], [t0].[Nickname], [t0].[SquadId], [t2].[FullName], [t2].[Nickname], [t2].[SquadId]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [t1].[Nickname], [t1].[SquadId]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t1]
    WHERE [t1].[Discriminator] = N'Officer'
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t2] ON ([t0].[Nickname] = [t2].[LeaderNickname] OR ([t0].[Nickname] IS NULL AND [t2].[LeaderNickname] IS NULL)) AND [t0].[SquadId] = [t2].[LeaderSquadId]
ORDER BY [t].[Id], [t0].[Nickname], [t0].[SquadId], [t2].[Nickname]");
    }

    public override async Task Correlated_collections_deeply_nested_left_join(bool async)
    {
        await base.Correlated_collections_deeply_nested_left_join(async);

        AssertSql(
            @"SELECT [t].[Id], [t0].[Nickname], [t0].[SquadId], [s].[Id], [t1].[Nickname], [t1].[SquadId], [t1].[Id], [t1].[AmmunitionType], [t1].[IsAutomatic], [t1].[Name], [t1].[OwnerFullName], [t1].[SynergyWithId]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname]
LEFT JOIN [Squads] AS [s] ON [t0].[SquadId] = [s].[Id]
LEFT JOIN (
    SELECT [t2].[Nickname], [t2].[SquadId], [t3].[Id], [t3].[AmmunitionType], [t3].[IsAutomatic], [t3].[Name], [t3].[OwnerFullName], [t3].[SynergyWithId]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t2]
    LEFT JOIN (
        SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [w].[IsAutomatic] = CAST(1 AS bit)
    ) AS [t3] ON [t2].[FullName] = [t3].[OwnerFullName]
    WHERE [t2].[HasSoulPatch] = CAST(1 AS bit)
) AS [t1] ON [s].[Id] = [t1].[SquadId]
ORDER BY [t].[Note], [t0].[Nickname] DESC, [t].[Id], [t0].[SquadId], [s].[Id], [t1].[Nickname], [t1].[SquadId]");
    }

    public override async Task Correlated_collections_from_left_join_with_additional_elements_projected_of_that_join(bool async)
    {
        await base.Correlated_collections_from_left_join_with_additional_elements_projected_of_that_join(async);

        AssertSql(
            @"SELECT [w].[Id], [t].[Nickname], [t].[SquadId], [s].[Id], [t1].[Nickname], [t1].[SquadId], [t1].[Id], [t1].[AmmunitionType], [t1].[IsAutomatic], [t1].[Name], [t1].[OwnerFullName], [t1].[SynergyWithId], [t1].[Rank]
FROM [Weapons] AS [w]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t] ON [w].[OwnerFullName] = [t].[FullName]
LEFT JOIN [Squads] AS [s] ON [t].[SquadId] = [s].[Id]
LEFT JOIN (
    SELECT [t0].[Nickname], [t0].[SquadId], [t2].[Id], [t2].[AmmunitionType], [t2].[IsAutomatic], [t2].[Name], [t2].[OwnerFullName], [t2].[SynergyWithId], [t0].[Rank], [t0].[FullName]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t0]
    LEFT JOIN (
        SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
        FROM [Weapons] AS [w0]
        WHERE [w0].[IsAutomatic] = CAST(0 AS bit)
    ) AS [t2] ON [t0].[FullName] = [t2].[OwnerFullName]
) AS [t1] ON [s].[Id] = [t1].[SquadId]
ORDER BY [w].[Name], [w].[Id], [t].[Nickname], [t].[SquadId], [s].[Id], [t1].[FullName] DESC, [t1].[Nickname], [t1].[SquadId], [t1].[Id]");
    }

    public override async Task Correlated_collections_complex_scenario1(bool async)
    {
        await base.Correlated_collections_complex_scenario1(async);

        AssertSql(
            @"SELECT [t].[FullName], [t].[Nickname], [t].[SquadId], [t1].[Id], [t1].[Nickname], [t1].[SquadId], [t1].[Id0], [t1].[Nickname0], [t1].[HasSoulPatch], [t1].[SquadId0]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [w].[Id], [t0].[Nickname], [t0].[SquadId], [s].[Id] AS [Id0], [t2].[Nickname] AS [Nickname0], [t2].[HasSoulPatch], [t2].[SquadId] AS [SquadId0], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
    LEFT JOIN (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t0] ON [w].[OwnerFullName] = [t0].[FullName]
    LEFT JOIN [Squads] AS [s] ON [t0].[SquadId] = [s].[Id]
    LEFT JOIN (
        SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOfBirthName], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g1]
        UNION ALL
        SELECT [o1].[Nickname], [o1].[SquadId], [o1].[AssignedCityName], [o1].[CityOfBirthName], [o1].[FullName], [o1].[HasSoulPatch], [o1].[LeaderNickname], [o1].[LeaderSquadId], [o1].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o1]
    ) AS [t2] ON [s].[Id] = [t2].[SquadId]
) AS [t1] ON [t].[FullName] = [t1].[OwnerFullName]
ORDER BY [t].[Nickname], [t].[SquadId], [t1].[Id], [t1].[Nickname], [t1].[SquadId], [t1].[Id0], [t1].[Nickname0]");
    }

    public override async Task Correlated_collections_complex_scenario2(bool async)
    {
        await base.Correlated_collections_complex_scenario2(async);

        AssertSql(
            @"SELECT [t].[FullName], [t].[Nickname], [t].[SquadId], [t3].[FullName], [t3].[Nickname], [t3].[SquadId], [t3].[Id], [t3].[Nickname0], [t3].[SquadId0], [t3].[Id0], [t3].[Nickname00], [t3].[HasSoulPatch], [t3].[SquadId00]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [t0].[FullName], [t0].[Nickname], [t0].[SquadId], [t1].[Id], [t1].[Nickname] AS [Nickname0], [t1].[SquadId] AS [SquadId0], [t1].[Id0], [t1].[Nickname0] AS [Nickname00], [t1].[HasSoulPatch], [t1].[SquadId0] AS [SquadId00], [t0].[LeaderNickname], [t0].[LeaderSquadId]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t0]
    LEFT JOIN (
        SELECT [w].[Id], [t2].[Nickname], [t2].[SquadId], [s].[Id] AS [Id0], [t4].[Nickname] AS [Nickname0], [t4].[HasSoulPatch], [t4].[SquadId] AS [SquadId0], [w].[OwnerFullName]
        FROM [Weapons] AS [w]
        LEFT JOIN (
            SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOfBirthName], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[Rank], N'Gear' AS [Discriminator]
            FROM [Gears] AS [g1]
            UNION ALL
            SELECT [o1].[Nickname], [o1].[SquadId], [o1].[AssignedCityName], [o1].[CityOfBirthName], [o1].[FullName], [o1].[HasSoulPatch], [o1].[LeaderNickname], [o1].[LeaderSquadId], [o1].[Rank], N'Officer' AS [Discriminator]
            FROM [Officers] AS [o1]
        ) AS [t2] ON [w].[OwnerFullName] = [t2].[FullName]
        LEFT JOIN [Squads] AS [s] ON [t2].[SquadId] = [s].[Id]
        LEFT JOIN (
            SELECT [g2].[Nickname], [g2].[SquadId], [g2].[AssignedCityName], [g2].[CityOfBirthName], [g2].[FullName], [g2].[HasSoulPatch], [g2].[LeaderNickname], [g2].[LeaderSquadId], [g2].[Rank], N'Gear' AS [Discriminator]
            FROM [Gears] AS [g2]
            UNION ALL
            SELECT [o2].[Nickname], [o2].[SquadId], [o2].[AssignedCityName], [o2].[CityOfBirthName], [o2].[FullName], [o2].[HasSoulPatch], [o2].[LeaderNickname], [o2].[LeaderSquadId], [o2].[Rank], N'Officer' AS [Discriminator]
            FROM [Officers] AS [o2]
        ) AS [t4] ON [s].[Id] = [t4].[SquadId]
    ) AS [t1] ON [t0].[FullName] = [t1].[OwnerFullName]
) AS [t3] ON [t].[Nickname] = [t3].[LeaderNickname] AND [t].[SquadId] = [t3].[LeaderSquadId]
WHERE [t].[Discriminator] = N'Officer'
ORDER BY [t].[Nickname], [t].[SquadId], [t3].[Nickname], [t3].[SquadId], [t3].[Id], [t3].[Nickname0], [t3].[SquadId0], [t3].[Id0], [t3].[Nickname00]");
    }

    public override async Task Correlated_collections_with_funky_orderby_complex_scenario1(bool async)
    {
        await base.Correlated_collections_with_funky_orderby_complex_scenario1(async);

        AssertSql(
            @"SELECT [t].[FullName], [t].[Nickname], [t].[SquadId], [t1].[Id], [t1].[Nickname], [t1].[SquadId], [t1].[Id0], [t1].[Nickname0], [t1].[HasSoulPatch], [t1].[SquadId0]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [w].[Id], [t0].[Nickname], [t0].[SquadId], [s].[Id] AS [Id0], [t2].[Nickname] AS [Nickname0], [t2].[HasSoulPatch], [t2].[SquadId] AS [SquadId0], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
    LEFT JOIN (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t0] ON [w].[OwnerFullName] = [t0].[FullName]
    LEFT JOIN [Squads] AS [s] ON [t0].[SquadId] = [s].[Id]
    LEFT JOIN (
        SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOfBirthName], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g1]
        UNION ALL
        SELECT [o1].[Nickname], [o1].[SquadId], [o1].[AssignedCityName], [o1].[CityOfBirthName], [o1].[FullName], [o1].[HasSoulPatch], [o1].[LeaderNickname], [o1].[LeaderSquadId], [o1].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o1]
    ) AS [t2] ON [s].[Id] = [t2].[SquadId]
) AS [t1] ON [t].[FullName] = [t1].[OwnerFullName]
ORDER BY [t].[FullName], [t].[Nickname] DESC, [t].[SquadId], [t1].[Id], [t1].[Nickname], [t1].[SquadId], [t1].[Id0], [t1].[Nickname0]");
    }

    public override async Task Correlated_collections_with_funky_orderby_complex_scenario2(bool async)
    {
        await base.Correlated_collections_with_funky_orderby_complex_scenario2(async);

        AssertSql(
            @"SELECT [t].[FullName], [t].[Nickname], [t].[SquadId], [t3].[FullName], [t3].[Nickname], [t3].[SquadId], [t3].[Id], [t3].[Nickname0], [t3].[SquadId0], [t3].[Id0], [t3].[Nickname00], [t3].[HasSoulPatch], [t3].[SquadId00]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [t0].[FullName], [t0].[Nickname], [t0].[SquadId], [t1].[Id], [t1].[Nickname] AS [Nickname0], [t1].[SquadId] AS [SquadId0], [t1].[Id0], [t1].[Nickname0] AS [Nickname00], [t1].[HasSoulPatch], [t1].[SquadId0] AS [SquadId00], [t0].[HasSoulPatch] AS [HasSoulPatch0], [t1].[IsAutomatic], [t1].[Name], [t0].[LeaderNickname], [t0].[LeaderSquadId]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t0]
    LEFT JOIN (
        SELECT [w].[Id], [t2].[Nickname], [t2].[SquadId], [s].[Id] AS [Id0], [t4].[Nickname] AS [Nickname0], [t4].[HasSoulPatch], [t4].[SquadId] AS [SquadId0], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName]
        FROM [Weapons] AS [w]
        LEFT JOIN (
            SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOfBirthName], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[Rank], N'Gear' AS [Discriminator]
            FROM [Gears] AS [g1]
            UNION ALL
            SELECT [o1].[Nickname], [o1].[SquadId], [o1].[AssignedCityName], [o1].[CityOfBirthName], [o1].[FullName], [o1].[HasSoulPatch], [o1].[LeaderNickname], [o1].[LeaderSquadId], [o1].[Rank], N'Officer' AS [Discriminator]
            FROM [Officers] AS [o1]
        ) AS [t2] ON [w].[OwnerFullName] = [t2].[FullName]
        LEFT JOIN [Squads] AS [s] ON [t2].[SquadId] = [s].[Id]
        LEFT JOIN (
            SELECT [g2].[Nickname], [g2].[SquadId], [g2].[AssignedCityName], [g2].[CityOfBirthName], [g2].[FullName], [g2].[HasSoulPatch], [g2].[LeaderNickname], [g2].[LeaderSquadId], [g2].[Rank], N'Gear' AS [Discriminator]
            FROM [Gears] AS [g2]
            UNION ALL
            SELECT [o2].[Nickname], [o2].[SquadId], [o2].[AssignedCityName], [o2].[CityOfBirthName], [o2].[FullName], [o2].[HasSoulPatch], [o2].[LeaderNickname], [o2].[LeaderSquadId], [o2].[Rank], N'Officer' AS [Discriminator]
            FROM [Officers] AS [o2]
        ) AS [t4] ON [s].[Id] = [t4].[SquadId]
    ) AS [t1] ON [t0].[FullName] = [t1].[OwnerFullName]
) AS [t3] ON [t].[Nickname] = [t3].[LeaderNickname] AND [t].[SquadId] = [t3].[LeaderSquadId]
WHERE [t].[Discriminator] = N'Officer'
ORDER BY [t].[HasSoulPatch], [t].[LeaderNickname], [t].[FullName], [t].[Nickname], [t].[SquadId], [t3].[FullName], [t3].[HasSoulPatch0] DESC, [t3].[Nickname], [t3].[SquadId], [t3].[IsAutomatic], [t3].[Name] DESC, [t3].[Id], [t3].[Nickname0], [t3].[SquadId0], [t3].[Id0], [t3].[Nickname00]");
    }

    public override async Task Correlated_collection_with_top_level_FirstOrDefault(bool async)
    {
        await base.Correlated_collection_with_top_level_FirstOrDefault(async);

        AssertSql(
            @"SELECT [t0].[Nickname], [t0].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM (
    SELECT TOP(1) [t].[Nickname], [t].[SquadId], [t].[FullName]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    ORDER BY [t].[Nickname]
) AS [t0]
LEFT JOIN [Weapons] AS [w] ON [t0].[FullName] = [w].[OwnerFullName]
ORDER BY [t0].[Nickname], [t0].[SquadId]");
    }

    public override async Task Correlated_collection_with_top_level_Count(bool async)
    {
        await base.Correlated_collection_with_top_level_Count(async);

        AssertSql(
            @"SELECT COUNT(*)
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task Correlated_collection_with_top_level_Last_with_orderby_on_outer(bool async)
    {
        await base.Correlated_collection_with_top_level_Last_with_orderby_on_outer(async);

        AssertSql(
            @"SELECT [t0].[Nickname], [t0].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM (
    SELECT TOP(1) [t].[Nickname], [t].[SquadId], [t].[FullName]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    ORDER BY [t].[FullName]
) AS [t0]
LEFT JOIN [Weapons] AS [w] ON [t0].[FullName] = [w].[OwnerFullName]
ORDER BY [t0].[FullName], [t0].[Nickname], [t0].[SquadId]");
    }

    public override async Task Correlated_collection_with_top_level_Last_with_order_by_on_inner(bool async)
    {
        await base.Correlated_collection_with_top_level_Last_with_order_by_on_inner(async);

        AssertSql(
            @"SELECT [t0].[Nickname], [t0].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM (
    SELECT TOP(1) [t].[Nickname], [t].[SquadId], [t].[FullName]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    ORDER BY [t].[FullName] DESC
) AS [t0]
LEFT JOIN [Weapons] AS [w] ON [t0].[FullName] = [w].[OwnerFullName]
ORDER BY [t0].[FullName] DESC, [t0].[Nickname], [t0].[SquadId], [w].[Name]");
    }

    public override async Task Null_semantics_on_nullable_bool_from_inner_join_subquery_is_fully_applied(bool async)
    {
        await base.Null_semantics_on_nullable_bool_from_inner_join_subquery_is_fully_applied(async);

        AssertSql(
            @"SELECT [t0].[Id], [t0].[CapitalName], [t0].[Name], [t0].[ServerAddress], [t0].[CommanderName], [t0].[Eradicated]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
INNER JOIN (
    SELECT [l1].[Id], [l1].[CapitalName], [l1].[Name], [l1].[ServerAddress], [l1].[CommanderName], [l1].[Eradicated]
    FROM [LocustHordes] AS [l1]
    WHERE [l1].[Name] = N'Swarm'
) AS [t0] ON [t].[Name] = [t0].[CommanderName]
WHERE [t0].[Eradicated] <> CAST(1 AS bit) OR ([t0].[Eradicated] IS NULL)");
    }

    public override async Task Null_semantics_on_nullable_bool_from_left_join_subquery_is_fully_applied(bool async)
    {
        await base.Null_semantics_on_nullable_bool_from_left_join_subquery_is_fully_applied(async);

        AssertSql(
            @"SELECT [t0].[Id], [t0].[CapitalName], [t0].[Name], [t0].[ServerAddress], [t0].[CommanderName], [t0].[Eradicated]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[CapitalName], [l1].[Name], [l1].[ServerAddress], [l1].[CommanderName], [l1].[Eradicated]
    FROM [LocustHordes] AS [l1]
    WHERE [l1].[Name] = N'Swarm'
) AS [t0] ON [t].[Name] = [t0].[CommanderName]
WHERE [t0].[Eradicated] <> CAST(1 AS bit) OR ([t0].[Eradicated] IS NULL)");
    }

    public override async Task Include_on_derived_type_with_order_by_and_paging(bool async)
    {
        await base.Include_on_derived_type_with_order_by_and_paging(async);

        AssertSql(
            @"@__p_0='10'

SELECT [t2].[Name], [t2].[LocustHordeId], [t2].[ThreatLevel], [t2].[ThreatLevelByte], [t2].[ThreatLevelNullableByte], [t2].[DefeatedByNickname], [t2].[DefeatedBySquadId], [t2].[HighCommandId], [t2].[Discriminator], [t2].[Nickname], [t2].[SquadId], [t2].[AssignedCityName], [t2].[CityOfBirthName], [t2].[FullName], [t2].[HasSoulPatch], [t2].[LeaderNickname], [t2].[LeaderSquadId], [t2].[Rank], [t2].[Discriminator0] AS [Discriminator], [t2].[Id], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM (
    SELECT TOP(@__p_0) [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator] AS [Discriminator0], [t1].[Id], [t1].[Note]
    FROM (
        SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
        FROM [LocustLeaders] AS [l]
        UNION ALL
        SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
        FROM [LocustCommanders] AS [l0]
    ) AS [t]
    LEFT JOIN (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t0] ON [t].[DefeatedByNickname] = [t0].[Nickname] AND [t].[DefeatedBySquadId] = [t0].[SquadId]
    LEFT JOIN [Tags] AS [t1] ON ([t0].[Nickname] = [t1].[GearNickName] OR ([t0].[Nickname] IS NULL AND [t1].[GearNickName] IS NULL)) AND ([t0].[SquadId] = [t1].[GearSquadId] OR ([t0].[SquadId] IS NULL AND [t1].[GearSquadId] IS NULL))
    ORDER BY [t1].[Note]
) AS [t2]
LEFT JOIN [Weapons] AS [w] ON [t2].[FullName] = [w].[OwnerFullName]
ORDER BY [t2].[Note], [t2].[Name], [t2].[Nickname], [t2].[SquadId], [t2].[Id]");
    }

    public override async Task Select_required_navigation_on_derived_type(bool async)
    {
        await base.Select_required_navigation_on_derived_type(async);

        AssertSql(
            @"SELECT [l1].[Name]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
LEFT JOIN [LocustHighCommands] AS [l1] ON [t].[HighCommandId] = [l1].[Id]");
    }

    public override async Task Select_required_navigation_on_the_same_type_with_cast(bool async)
    {
        await base.Select_required_navigation_on_the_same_type_with_cast(async);

        AssertSql(
            @"SELECT [c].[Name]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN [Cities] AS [c] ON [t].[CityOfBirthName] = [c].[Name]");
    }

    public override async Task Where_required_navigation_on_derived_type(bool async)
    {
        await base.Where_required_navigation_on_derived_type(async);

        AssertSql(
            @"SELECT [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
LEFT JOIN [LocustHighCommands] AS [l1] ON [t].[HighCommandId] = [l1].[Id]
WHERE [l1].[IsOperational] = CAST(1 AS bit)");
    }

    public override async Task Outer_parameter_in_join_key(bool async)
    {
        await base.Outer_parameter_in_join_key(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t1].[Note], [t1].[Id], [t1].[Nickname], [t1].[SquadId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
OUTER APPLY (
    SELECT [t0].[Note], [t0].[Id], [t2].[Nickname], [t2].[SquadId]
    FROM [Tags] AS [t0]
    INNER JOIN (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t2] ON [t].[FullName] = [t2].[FullName]
) AS [t1]
WHERE [t].[Discriminator] = N'Officer'
ORDER BY [t].[Nickname], [t].[SquadId], [t1].[Id], [t1].[Nickname]");
    }

    public override async Task Outer_parameter_in_join_key_inner_and_outer(bool async)
    {
        await base.Outer_parameter_in_join_key_inner_and_outer(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t1].[Note], [t1].[Id], [t1].[Nickname], [t1].[SquadId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
OUTER APPLY (
    SELECT [t0].[Note], [t0].[Id], [t2].[Nickname], [t2].[SquadId]
    FROM [Tags] AS [t0]
    INNER JOIN (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t2] ON [t].[FullName] = [t].[Nickname]
) AS [t1]
WHERE [t].[Discriminator] = N'Officer'
ORDER BY [t].[Nickname], [t].[SquadId], [t1].[Id], [t1].[Nickname]");
    }

    public override async Task Outer_parameter_in_group_join_with_DefaultIfEmpty(bool async)
    {
        await base.Outer_parameter_in_group_join_with_DefaultIfEmpty(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t1].[Note], [t1].[Id], [t1].[Nickname], [t1].[SquadId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
OUTER APPLY (
    SELECT [t0].[Note], [t0].[Id], [t2].[Nickname], [t2].[SquadId]
    FROM [Tags] AS [t0]
    LEFT JOIN (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t2] ON [t].[FullName] = [t2].[FullName]
) AS [t1]
WHERE [t].[Discriminator] = N'Officer'
ORDER BY [t].[Nickname], [t].[SquadId], [t1].[Id], [t1].[Nickname]");
    }

    public override async Task Negated_bool_ternary_inside_anonymous_type_in_projection(bool async)
    {
        await base.Negated_bool_ternary_inside_anonymous_type_in_projection(async);

        AssertSql(
            @"SELECT CASE
    WHEN CASE
        WHEN [t0].[HasSoulPatch] = CAST(1 AS bit) THEN CAST(1 AS bit)
        ELSE COALESCE([t0].[HasSoulPatch], CAST(1 AS bit))
    END = CAST(0 AS bit) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [c]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]");
    }

    public override async Task Order_by_entity_qsre(bool async)
    {
        await base.Order_by_entity_qsre(async);

        AssertSql(
            @"SELECT [t].[FullName]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Cities] AS [c] ON [t].[AssignedCityName] = [c].[Name]
ORDER BY [c].[Name], [t].[Nickname] DESC");
    }

    public override async Task Order_by_entity_qsre_with_inheritance(bool async)
    {
        await base.Order_by_entity_qsre_with_inheritance(async);

        AssertSql(
            @"SELECT [t].[Name]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
INNER JOIN [LocustHighCommands] AS [l1] ON [t].[HighCommandId] = [l1].[Id]
WHERE [t].[Discriminator] = N'LocustCommander'
ORDER BY [l1].[Id], [t].[Name]");
    }

    public override async Task Order_by_entity_qsre_composite_key(bool async)
    {
        await base.Order_by_entity_qsre_composite_key(async);

        AssertSql(
            @"SELECT [w].[Name]
FROM [Weapons] AS [w]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t] ON [w].[OwnerFullName] = [t].[FullName]
ORDER BY [t].[Nickname], [t].[SquadId], [w].[Id]");
    }

    public override async Task Order_by_entity_qsre_with_other_orderbys(bool async)
    {
        await base.Order_by_entity_qsre_with_other_orderbys(async);

        AssertSql(
            @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t] ON [w].[OwnerFullName] = [t].[FullName]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY [w].[IsAutomatic], [t].[Nickname] DESC, [t].[SquadId] DESC, [w0].[Id], [w].[Name]");
    }

    public override async Task Join_on_entity_qsre_keys(bool async)
    {
        await base.Join_on_entity_qsre_keys(async);

        AssertSql(
            @"SELECT [w].[Name] AS [Name1], [w0].[Name] AS [Name2]
FROM [Weapons] AS [w]
INNER JOIN [Weapons] AS [w0] ON [w].[Id] = [w0].[Id]");
    }

    public override async Task Join_on_entity_qsre_keys_composite_key(bool async)
    {
        await base.Join_on_entity_qsre_keys_composite_key(async);

        AssertSql(
            @"SELECT [t].[FullName] AS [GearName1], [t0].[FullName] AS [GearName2]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON [t].[Nickname] = [t0].[Nickname] AND [t].[SquadId] = [t0].[SquadId]");
    }

    public override async Task Join_on_entity_qsre_keys_inheritance(bool async)
    {
        await base.Join_on_entity_qsre_keys_inheritance(async);

        AssertSql(
            @"SELECT [t].[FullName] AS [GearName], [t0].[FullName] AS [OfficerName]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN (
    SELECT [t1].[Nickname], [t1].[SquadId], [t1].[FullName]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t1]
    WHERE [t1].[Discriminator] = N'Officer'
) AS [t0] ON [t].[Nickname] = [t0].[Nickname] AND [t].[SquadId] = [t0].[SquadId]");
    }

    public override async Task Join_on_entity_qsre_keys_outer_key_is_navigation(bool async)
    {
        await base.Join_on_entity_qsre_keys_outer_key_is_navigation(async);

        AssertSql(
            @"SELECT [w].[Name] AS [Name1], [w1].[Name] AS [Name2]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
INNER JOIN [Weapons] AS [w1] ON [w0].[Id] = [w1].[Id]");
    }

    public override async Task Join_on_entity_qsre_keys_inner_key_is_navigation(bool async)
    {
        await base.Join_on_entity_qsre_keys_inner_key_is_navigation(async);

        AssertSql(
            @"SELECT [c].[Name] AS [CityName], [t0].[Nickname] AS [GearNickname]
FROM [Cities] AS [c]
INNER JOIN (
    SELECT [t].[Nickname], [c0].[Name]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    LEFT JOIN [Cities] AS [c0] ON [t].[AssignedCityName] = [c0].[Name]
) AS [t0] ON [c].[Name] = [t0].[Name]");
    }

    public override async Task Join_on_entity_qsre_keys_inner_key_is_navigation_composite_key(bool async)
    {
        await base.Join_on_entity_qsre_keys_inner_key_is_navigation_composite_key(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t1].[Note]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN (
    SELECT [t0].[Note], [t2].[Nickname], [t2].[SquadId]
    FROM [Tags] AS [t0]
    LEFT JOIN (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t2] ON [t0].[GearNickName] = [t2].[Nickname] AND [t0].[GearSquadId] = [t2].[SquadId]
    WHERE [t0].[Note] IN (N'Cole''s Tag', N'Dom''s Tag')
) AS [t1] ON [t].[Nickname] = [t1].[Nickname] AND [t].[SquadId] = [t1].[SquadId]");
    }

    public override async Task Join_on_entity_qsre_keys_inner_key_is_nested_navigation(bool async)
    {
        await base.Join_on_entity_qsre_keys_inner_key_is_nested_navigation(async);

        AssertSql(
            @"SELECT [s].[Name] AS [SquadName], [t0].[Name] AS [WeaponName]
FROM [Squads] AS [s]
INNER JOIN (
    SELECT [w].[Name], [s0].[Id] AS [Id0]
    FROM [Weapons] AS [w]
    LEFT JOIN (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t] ON [w].[OwnerFullName] = [t].[FullName]
    LEFT JOIN [Squads] AS [s0] ON [t].[SquadId] = [s0].[Id]
    WHERE [w].[IsAutomatic] = CAST(1 AS bit)
) AS [t0] ON [s].[Id] = [t0].[Id0]");
    }

    public override async Task GroupJoin_on_entity_qsre_keys_inner_key_is_nested_navigation(bool async)
    {
        await base.GroupJoin_on_entity_qsre_keys_inner_key_is_nested_navigation(async);

        AssertSql(
            @"SELECT [s].[Name] AS [SquadName], [t0].[Name] AS [WeaponName]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [w].[Name], [s0].[Id] AS [Id0]
    FROM [Weapons] AS [w]
    LEFT JOIN (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t] ON [w].[OwnerFullName] = [t].[FullName]
    LEFT JOIN [Squads] AS [s0] ON [t].[SquadId] = [s0].[Id]
) AS [t0] ON [s].[Id] = [t0].[Id0]");
    }

    public override async Task Streaming_correlated_collection_issue_11403(bool async)
    {
        await base.Streaming_correlated_collection_issue_11403(async);

        AssertSql(
            @"SELECT [t0].[Nickname], [t0].[SquadId], [t1].[Id], [t1].[AmmunitionType], [t1].[IsAutomatic], [t1].[Name], [t1].[OwnerFullName], [t1].[SynergyWithId]
FROM (
    SELECT TOP(1) [t].[Nickname], [t].[SquadId], [t].[FullName]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    ORDER BY [t].[Nickname]
) AS [t0]
LEFT JOIN (
    SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
    FROM [Weapons] AS [w]
    WHERE [w].[IsAutomatic] = CAST(0 AS bit)
) AS [t1] ON [t0].[FullName] = [t1].[OwnerFullName]
ORDER BY [t0].[Nickname], [t0].[SquadId], [t1].[Id]");
    }

    public override async Task Project_one_value_type_from_empty_collection(bool async)
    {
        await base.Project_one_value_type_from_empty_collection(async);

        AssertSql(
            @"SELECT [s].[Name], COALESCE((
    SELECT TOP(1) [t].[SquadId]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    WHERE [s].[Id] = [t].[SquadId] AND [t].[HasSoulPatch] = CAST(1 AS bit)), 0) AS [SquadId]
FROM [Squads] AS [s]
WHERE [s].[Name] = N'Kilo'");
    }

    public override async Task Project_one_value_type_converted_to_nullable_from_empty_collection(bool async)
    {
        await base.Project_one_value_type_converted_to_nullable_from_empty_collection(async);

        AssertSql(
            @"SELECT [s].[Name], (
    SELECT TOP(1) [t].[SquadId]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    WHERE [s].[Id] = [t].[SquadId] AND [t].[HasSoulPatch] = CAST(1 AS bit)) AS [SquadId]
FROM [Squads] AS [s]
WHERE [s].[Name] = N'Kilo'");
    }

    public override async Task Project_one_value_type_with_client_projection_from_empty_collection(bool async)
    {
        await base.Project_one_value_type_with_client_projection_from_empty_collection(async);

        AssertSql(
            @"SELECT [s].[Name], [t1].[SquadId], [t1].[LeaderSquadId], [t1].[c]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [t0].[SquadId], [t0].[LeaderSquadId], [t0].[c]
    FROM (
        SELECT [t].[SquadId], [t].[LeaderSquadId], 1 AS [c], ROW_NUMBER() OVER(PARTITION BY [t].[SquadId] ORDER BY [t].[Nickname], [t].[SquadId]) AS [row]
        FROM (
            SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
            FROM [Gears] AS [g]
            UNION ALL
            SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
            FROM [Officers] AS [o]
        ) AS [t]
        WHERE [t].[HasSoulPatch] = CAST(1 AS bit)
    ) AS [t0]
    WHERE [t0].[row] <= 1
) AS [t1] ON [s].[Id] = [t1].[SquadId]
WHERE [s].[Name] = N'Kilo'");
    }

    public override async Task Filter_on_subquery_projecting_one_value_type_from_empty_collection(bool async)
    {
        await base.Filter_on_subquery_projecting_one_value_type_from_empty_collection(async);

        AssertSql(
            @"SELECT [s].[Name]
FROM [Squads] AS [s]
WHERE [s].[Name] = N'Kilo' AND COALESCE((
    SELECT TOP(1) [t].[SquadId]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    WHERE [s].[Id] = [t].[SquadId] AND [t].[HasSoulPatch] = CAST(1 AS bit)), 0) <> 0");
    }

    public override async Task Select_subquery_projecting_single_constant_int(bool async)
    {
        await base.Select_subquery_projecting_single_constant_int(async);

        AssertSql(
            @"SELECT [s].[Name], COALESCE((
    SELECT TOP(1) 42
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    WHERE [s].[Id] = [t].[SquadId] AND [t].[HasSoulPatch] = CAST(1 AS bit)), 0) AS [Gear]
FROM [Squads] AS [s]");
    }

    public override async Task Select_subquery_projecting_single_constant_string(bool async)
    {
        await base.Select_subquery_projecting_single_constant_string(async);

        AssertSql(
            @"SELECT [s].[Name], (
    SELECT TOP(1) N'Foo'
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    WHERE [s].[Id] = [t].[SquadId] AND [t].[HasSoulPatch] = CAST(1 AS bit)) AS [Gear]
FROM [Squads] AS [s]");
    }

    public override async Task Select_subquery_projecting_single_constant_bool(bool async)
    {
        await base.Select_subquery_projecting_single_constant_bool(async);

        AssertSql(
            @"SELECT [s].[Name], COALESCE((
    SELECT TOP(1) CAST(1 AS bit)
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    WHERE [s].[Id] = [t].[SquadId] AND [t].[HasSoulPatch] = CAST(1 AS bit)), CAST(0 AS bit)) AS [Gear]
FROM [Squads] AS [s]");
    }

    public override async Task Select_subquery_projecting_single_constant_inside_anonymous(bool async)
    {
        await base.Select_subquery_projecting_single_constant_inside_anonymous(async);

        AssertSql(
            @"SELECT [s].[Name], [t1].[One]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [t0].[One], [t0].[SquadId]
    FROM (
        SELECT 1 AS [One], [t].[SquadId], ROW_NUMBER() OVER(PARTITION BY [t].[SquadId] ORDER BY [t].[Nickname], [t].[SquadId]) AS [row]
        FROM (
            SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
            FROM [Gears] AS [g]
            UNION ALL
            SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
            FROM [Officers] AS [o]
        ) AS [t]
        WHERE [t].[HasSoulPatch] = CAST(1 AS bit)
    ) AS [t0]
    WHERE [t0].[row] <= 1
) AS [t1] ON [s].[Id] = [t1].[SquadId]");
    }

    public override async Task Select_subquery_projecting_multiple_constants_inside_anonymous(bool async)
    {
        await base.Select_subquery_projecting_multiple_constants_inside_anonymous(async);

        AssertSql(
            @"SELECT [s].[Name], [t1].[True1], [t1].[False1], [t1].[c]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [t0].[True1], [t0].[False1], [t0].[c], [t0].[SquadId]
    FROM (
        SELECT CAST(1 AS bit) AS [True1], CAST(0 AS bit) AS [False1], 1 AS [c], [t].[SquadId], ROW_NUMBER() OVER(PARTITION BY [t].[SquadId] ORDER BY [t].[Nickname], [t].[SquadId]) AS [row]
        FROM (
            SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
            FROM [Gears] AS [g]
            UNION ALL
            SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
            FROM [Officers] AS [o]
        ) AS [t]
        WHERE [t].[HasSoulPatch] = CAST(1 AS bit)
    ) AS [t0]
    WHERE [t0].[row] <= 1
) AS [t1] ON [s].[Id] = [t1].[SquadId]");
    }

    public override async Task Include_with_order_by_constant(bool async)
    {
        await base.Include_with_order_by_constant(async);

        AssertSql(
            @"SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t] ON [s].[Id] = [t].[SquadId]
ORDER BY [s].[Id], [t].[Nickname]");
    }

    public override async Task Correlated_collection_order_by_constant(bool async)
    {
        await base.Correlated_collection_order_by_constant(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [w].[Name], [w].[Id]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Weapons] AS [w] ON [t].[FullName] = [w].[OwnerFullName]
ORDER BY [t].[Nickname], [t].[SquadId]");
    }

    public override async Task Select_subquery_projecting_single_constant_null_of_non_mapped_type(bool async)
    {
        await base.Select_subquery_projecting_single_constant_null_of_non_mapped_type(async);

        AssertSql(
            @"SELECT [s].[Name], [t1].[c]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [t0].[c], [t0].[SquadId]
    FROM (
        SELECT 1 AS [c], [t].[SquadId], ROW_NUMBER() OVER(PARTITION BY [t].[SquadId] ORDER BY [t].[Nickname], [t].[SquadId]) AS [row]
        FROM (
            SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
            FROM [Gears] AS [g]
            UNION ALL
            SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
            FROM [Officers] AS [o]
        ) AS [t]
        WHERE [t].[HasSoulPatch] = CAST(1 AS bit)
    ) AS [t0]
    WHERE [t0].[row] <= 1
) AS [t1] ON [s].[Id] = [t1].[SquadId]");
    }

    public override async Task Select_subquery_projecting_single_constant_of_non_mapped_type(bool async)
    {
        await base.Select_subquery_projecting_single_constant_of_non_mapped_type(async);

        AssertSql(
            @"SELECT [s].[Name], [t1].[c]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [t0].[c], [t0].[SquadId]
    FROM (
        SELECT 1 AS [c], [t].[SquadId], ROW_NUMBER() OVER(PARTITION BY [t].[SquadId] ORDER BY [t].[Nickname], [t].[SquadId]) AS [row]
        FROM (
            SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
            FROM [Gears] AS [g]
            UNION ALL
            SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
            FROM [Officers] AS [o]
        ) AS [t]
        WHERE [t].[HasSoulPatch] = CAST(1 AS bit)
    ) AS [t0]
    WHERE [t0].[row] <= 1
) AS [t1] ON [s].[Id] = [t1].[SquadId]");
    }

    public override async Task Include_collection_OrderBy_aggregate(bool async)
    {
        await base.Include_collection_OrderBy_aggregate(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON [t].[Nickname] = [t0].[LeaderNickname] AND [t].[SquadId] = [t0].[LeaderSquadId]
WHERE [t].[Discriminator] = N'Officer'
ORDER BY (
    SELECT COUNT(*)
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName]), [t].[Nickname], [t].[SquadId], [t0].[Nickname]");
    }

    public override async Task Include_collection_with_complex_OrderBy2(bool async)
    {
        await base.Include_collection_with_complex_OrderBy2(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON [t].[Nickname] = [t0].[LeaderNickname] AND [t].[SquadId] = [t0].[LeaderSquadId]
WHERE [t].[Discriminator] = N'Officer'
ORDER BY (
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]), [t].[Nickname], [t].[SquadId], [t0].[Nickname]");
    }

    public override async Task Include_collection_with_complex_OrderBy3(bool async)
    {
        await base.Include_collection_with_complex_OrderBy3(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON [t].[Nickname] = [t0].[LeaderNickname] AND [t].[SquadId] = [t0].[LeaderSquadId]
WHERE [t].[Discriminator] = N'Officer'
ORDER BY COALESCE((
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]), CAST(0 AS bit)), [t].[Nickname], [t].[SquadId], [t0].[Nickname]");
    }

    public override async Task Correlated_collection_with_complex_OrderBy(bool async)
    {
        await base.Correlated_collection_with_complex_OrderBy(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t1]
    WHERE [t1].[HasSoulPatch] = CAST(0 AS bit)
) AS [t0] ON [t].[Nickname] = [t0].[LeaderNickname] AND [t].[SquadId] = [t0].[LeaderSquadId]
WHERE [t].[Discriminator] = N'Officer'
ORDER BY (
    SELECT COUNT(*)
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName]), [t].[Nickname], [t].[SquadId], [t0].[Nickname]");
    }

    public override async Task Correlated_collection_with_very_complex_order_by(bool async)
    {
        await base.Correlated_collection_with_very_complex_order_by(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [t2].[Nickname], [t2].[SquadId], [t2].[AssignedCityName], [t2].[CityOfBirthName], [t2].[FullName], [t2].[HasSoulPatch], [t2].[LeaderNickname], [t2].[LeaderSquadId], [t2].[Rank], [t2].[Discriminator]
    FROM (
        SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOfBirthName], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g1]
        UNION ALL
        SELECT [o1].[Nickname], [o1].[SquadId], [o1].[AssignedCityName], [o1].[CityOfBirthName], [o1].[FullName], [o1].[HasSoulPatch], [o1].[LeaderNickname], [o1].[LeaderSquadId], [o1].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o1]
    ) AS [t2]
    WHERE [t2].[HasSoulPatch] = CAST(0 AS bit)
) AS [t1] ON [t].[Nickname] = [t1].[LeaderNickname] AND [t].[SquadId] = [t1].[LeaderSquadId]
WHERE [t].[Discriminator] = N'Officer'
ORDER BY (
    SELECT COUNT(*)
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName] AND [w].[IsAutomatic] = COALESCE((
        SELECT TOP(1) [t0].[HasSoulPatch]
        FROM (
            SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
            FROM [Gears] AS [g0]
            UNION ALL
            SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
            FROM [Officers] AS [o0]
        ) AS [t0]
        WHERE [t0].[Nickname] = N'Marcus'), CAST(0 AS bit))), [t].[Nickname], [t].[SquadId], [t1].[Nickname]");
    }

    public override async Task Cast_to_derived_type_after_OfType_works(bool async)
    {
        await base.Cast_to_derived_type_after_OfType_works(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[Discriminator] = N'Officer'");
    }

    public override async Task Select_subquery_boolean(bool async)
    {
        await base.Select_subquery_boolean(async);

        AssertSql(
            @"SELECT COALESCE((
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]), CAST(0 AS bit))
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task Select_subquery_boolean_with_pushdown(bool async)
    {
        await base.Select_subquery_boolean_with_pushdown(async);

        AssertSql(
            @"SELECT (
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id])
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task Select_subquery_int_with_inside_cast_and_coalesce(bool async)
    {
        await base.Select_subquery_int_with_inside_cast_and_coalesce(async);

        AssertSql(
            @"SELECT COALESCE((
    SELECT TOP(1) [w].[Id]
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]), 42)
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task Select_subquery_int_with_outside_cast_and_coalesce(bool async)
    {
        await base.Select_subquery_int_with_outside_cast_and_coalesce(async);

        AssertSql(
            @"SELECT COALESCE((
    SELECT TOP(1) [w].[Id]
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]), 0, 42)
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task Select_subquery_int_with_pushdown_and_coalesce(bool async)
    {
        await base.Select_subquery_int_with_pushdown_and_coalesce(async);

        AssertSql(
            @"SELECT COALESCE((
    SELECT TOP(1) [w].[Id]
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]), 42)
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task Select_subquery_int_with_pushdown_and_coalesce2(bool async)
    {
        await base.Select_subquery_int_with_pushdown_and_coalesce2(async);

        AssertSql(
            @"SELECT COALESCE((
    SELECT TOP(1) [w].[Id]
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName]
    ORDER BY [w].[Id]), (
    SELECT TOP(1) [w0].[Id]
    FROM [Weapons] AS [w0]
    WHERE [t].[FullName] = [w0].[OwnerFullName]
    ORDER BY [w0].[Id]))
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task Select_subquery_boolean_empty(bool async)
    {
        await base.Select_subquery_boolean_empty(async);

        AssertSql(
            @"SELECT COALESCE((
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName] AND [w].[Name] = N'BFG'
    ORDER BY [w].[Id]), CAST(0 AS bit))
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task Select_subquery_boolean_empty_with_pushdown(bool async)
    {
        await base.Select_subquery_boolean_empty_with_pushdown(async);

        AssertSql(
            @"SELECT (
    SELECT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName] AND [w].[Name] = N'BFG'
    ORDER BY [w].[Id])
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task Select_subquery_distinct_singleordefault_boolean1(bool async)
    {
        await base.Select_subquery_distinct_singleordefault_boolean1(async);

        AssertSql(
            @"SELECT COALESCE((
    SELECT TOP(1) [t0].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [t].[FullName] = [w].[OwnerFullName] AND ([w].[Name] LIKE N'%Lancer%')
    ) AS [t0]), CAST(0 AS bit))
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit)");
    }

    public override async Task Select_subquery_distinct_singleordefault_boolean2(bool async)
    {
        await base.Select_subquery_distinct_singleordefault_boolean2(async);

        AssertSql(
            @"SELECT COALESCE((
    SELECT DISTINCT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName] AND ([w].[Name] LIKE N'%Lancer%')), CAST(0 AS bit))
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit)");
    }

    public override async Task Select_subquery_distinct_singleordefault_boolean_with_pushdown(bool async)
    {
        await base.Select_subquery_distinct_singleordefault_boolean_with_pushdown(async);

        AssertSql(
            @"SELECT (
    SELECT TOP(1) [t0].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [t].[FullName] = [w].[OwnerFullName] AND ([w].[Name] LIKE N'%Lancer%')
    ) AS [t0])
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit)");
    }

    public override async Task Select_subquery_distinct_singleordefault_boolean_empty1(bool async)
    {
        await base.Select_subquery_distinct_singleordefault_boolean_empty1(async);

        AssertSql(
            @"SELECT COALESCE((
    SELECT TOP(1) [t0].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [t].[FullName] = [w].[OwnerFullName] AND [w].[Name] = N'BFG'
    ) AS [t0]), CAST(0 AS bit))
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit)");
    }

    public override async Task Select_subquery_distinct_singleordefault_boolean_empty2(bool async)
    {
        await base.Select_subquery_distinct_singleordefault_boolean_empty2(async);

        AssertSql(
            @"SELECT COALESCE((
    SELECT DISTINCT TOP(1) [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName] AND [w].[Name] = N'BFG'), CAST(0 AS bit))
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit)");
    }

    public override async Task Select_subquery_distinct_singleordefault_boolean_empty_with_pushdown(bool async)
    {
        await base.Select_subquery_distinct_singleordefault_boolean_empty_with_pushdown(async);

        AssertSql(
            @"SELECT (
    SELECT TOP(1) [t0].[IsAutomatic]
    FROM (
        SELECT DISTINCT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
        FROM [Weapons] AS [w]
        WHERE [t].[FullName] = [w].[OwnerFullName] AND [w].[Name] = N'BFG'
    ) AS [t0])
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit)");
    }

    public override async Task Cast_subquery_to_base_type_using_typed_ToList(bool async)
    {
        await base.Cast_subquery_to_base_type_using_typed_ToList(async);

        AssertSql(
            @"SELECT [c].[Name], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Nickname], [t].[Rank], [t].[SquadId]
FROM [Cities] AS [c]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t] ON [c].[Name] = [t].[AssignedCityName]
WHERE [c].[Name] = N'Ephyra'
ORDER BY [c].[Name], [t].[Nickname]");
    }

    public override async Task Cast_ordered_subquery_to_base_type_using_typed_ToArray(bool async)
    {
        await base.Cast_ordered_subquery_to_base_type_using_typed_ToArray(async);

        AssertSql(
            @"SELECT [c].[Name], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Nickname], [t].[Rank], [t].[SquadId]
FROM [Cities] AS [c]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t] ON [c].[Name] = [t].[AssignedCityName]
WHERE [c].[Name] = N'Ephyra'
ORDER BY [c].[Name], [t].[Nickname] DESC");
    }

    public override async Task Correlated_collection_with_complex_order_by_funcletized_to_constant_bool(bool async)
    {
        await base.Correlated_collection_with_complex_order_by_funcletized_to_constant_bool(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [w].[Name], [w].[Id]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Weapons] AS [w] ON [t].[FullName] = [w].[OwnerFullName]
ORDER BY [t].[Nickname], [t].[SquadId]");
    }

    public override async Task Double_order_by_on_nullable_bool_coming_from_optional_navigation(bool async)
    {
        await base.Double_order_by_on_nullable_bool_coming_from_optional_navigation(async);

        AssertSql(
            @"SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY [w0].[IsAutomatic], [w0].[Id]");
    }

    public override async Task Double_order_by_on_Like(bool async)
    {
        await base.Double_order_by_on_Like(async);

        AssertSql(
            @"SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY CASE
    WHEN [w0].[Name] LIKE N'%Lancer' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
    }

    public override async Task Double_order_by_on_is_null(bool async)
    {
        await base.Double_order_by_on_is_null(async);

        AssertSql(
            @"SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY CASE
    WHEN [w0].[Name] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
    }

    public override async Task Double_order_by_on_string_compare(bool async)
    {
        await base.Double_order_by_on_string_compare(async);

        AssertSql(
            @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
ORDER BY CASE
    WHEN [w].[Name] = N'Marcus'' Lancer' AND [w].[Name] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [w].[Id]");
    }

    public override async Task Double_order_by_binary_expression(bool async)
    {
        await base.Double_order_by_binary_expression(async);

        AssertSql(
            @"SELECT [w].[Id] + 2 AS [Binary]
FROM [Weapons] AS [w]
ORDER BY [w].[Id] + 2");
    }

    public override async Task String_compare_with_null_conditional_argument(bool async)
    {
        await base.String_compare_with_null_conditional_argument(async);

        AssertSql(
            @"SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY CASE
    WHEN [w0].[Name] = N'Marcus'' Lancer' AND [w0].[Name] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
    }

    public override async Task String_compare_with_null_conditional_argument2(bool async)
    {
        await base.String_compare_with_null_conditional_argument2(async);

        AssertSql(
            @"SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY CASE
    WHEN N'Marcus'' Lancer' = [w0].[Name] AND [w0].[Name] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
    }

    public override async Task String_concat_with_null_conditional_argument(bool async)
    {
        await base.String_concat_with_null_conditional_argument(async);

        AssertSql(
            @"SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY COALESCE([w0].[Name], N'') + CAST(5 AS nvarchar(max))");
    }

    public override async Task String_concat_with_null_conditional_argument2(bool async)
    {
        await base.String_concat_with_null_conditional_argument2(async);

        AssertSql(
            @"SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY COALESCE([w0].[Name], N'') + N'Marcus'' Lancer'");
    }

    public override async Task String_concat_on_various_types(bool async)
    {
        await base.String_concat_on_various_types(async);

        AssertSql(
            @"SELECT (N'HasSoulPatch ' + CAST([t].[HasSoulPatch] AS nvarchar(max))) + N' HasSoulPatch' AS [HasSoulPatch], (N'Rank ' + CAST([t].[Rank] AS nvarchar(max))) + N' Rank' AS [Rank], (N'SquadId ' + CAST([t].[SquadId] AS nvarchar(max))) + N' SquadId' AS [SquadId], (N'Rating ' + COALESCE(CAST([m].[Rating] AS nvarchar(max)), N'')) + N' Rating' AS [Rating], (N'Timeline ' + CAST([m].[Timeline] AS nvarchar(max))) + N' Timeline' AS [Timeline]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
CROSS JOIN [Missions] AS [m]
ORDER BY [t].[Nickname], [m].[Id]");
    }

    public override async Task Time_of_day_datetimeoffset(bool async)
    {
        await base.Time_of_day_datetimeoffset(async);

        AssertSql(
            @"SELECT CONVERT(time, [m].[Timeline])
FROM [Missions] AS [m]");
    }

    public override async Task GroupBy_Property_Include_Select_Average(bool async)
    {
        await base.GroupBy_Property_Include_Select_Average(async);

        AssertSql(
            @"SELECT AVG(CAST([t].[SquadId] AS float))
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
GROUP BY [t].[Rank]");
    }

    public override async Task GroupBy_Property_Include_Select_Sum(bool async)
    {
        await base.GroupBy_Property_Include_Select_Sum(async);

        AssertSql(
            @"SELECT COALESCE(SUM([t].[SquadId]), 0)
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
GROUP BY [t].[Rank]");
    }

    public override async Task GroupBy_Property_Include_Select_Count(bool async)
    {
        await base.GroupBy_Property_Include_Select_Count(async);

        AssertSql(
            @"SELECT COUNT(*)
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
GROUP BY [t].[Rank]");
    }

    public override async Task GroupBy_Property_Include_Select_LongCount(bool async)
    {
        await base.GroupBy_Property_Include_Select_LongCount(async);

        AssertSql(
            @"SELECT COUNT_BIG(*)
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
GROUP BY [t].[Rank]");
    }

    public override async Task GroupBy_Property_Include_Select_Min(bool async)
    {
        await base.GroupBy_Property_Include_Select_Min(async);

        AssertSql(
            @"SELECT MIN([t].[SquadId])
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
GROUP BY [t].[Rank]");
    }

    public override async Task GroupBy_Property_Include_Aggregate_with_anonymous_selector(bool async)
    {
        await base.GroupBy_Property_Include_Aggregate_with_anonymous_selector(async);

        AssertSql(
            @"SELECT [t].[Nickname] AS [Key], COUNT(*) AS [c]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
GROUP BY [t].[Nickname]
ORDER BY [t].[Nickname]");
    }

    public override async Task Group_by_with_include_with_entity_in_result_selector(bool async)
    {
        await base.Group_by_with_include_with_entity_in_result_selector(async);

        AssertSql(
            @"SELECT [t0].[Rank], [t0].[c], [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator], [t1].[Name], [t1].[Location], [t1].[Nation]
FROM (
    SELECT [t].[Rank], COUNT(*) AS [c]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    GROUP BY [t].[Rank]
) AS [t0]
LEFT JOIN (
    SELECT [t2].[Nickname], [t2].[SquadId], [t2].[AssignedCityName], [t2].[CityOfBirthName], [t2].[FullName], [t2].[HasSoulPatch], [t2].[LeaderNickname], [t2].[LeaderSquadId], [t2].[Rank], [t2].[Discriminator], [t2].[Name], [t2].[Location], [t2].[Nation]
    FROM (
        SELECT [t3].[Nickname], [t3].[SquadId], [t3].[AssignedCityName], [t3].[CityOfBirthName], [t3].[FullName], [t3].[HasSoulPatch], [t3].[LeaderNickname], [t3].[LeaderSquadId], [t3].[Rank], [t3].[Discriminator], [c].[Name], [c].[Location], [c].[Nation], ROW_NUMBER() OVER(PARTITION BY [t3].[Rank] ORDER BY [t3].[Nickname]) AS [row]
        FROM (
            SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
            FROM [Gears] AS [g0]
            UNION ALL
            SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
            FROM [Officers] AS [o0]
        ) AS [t3]
        INNER JOIN [Cities] AS [c] ON [t3].[CityOfBirthName] = [c].[Name]
    ) AS [t2]
    WHERE [t2].[row] <= 1
) AS [t1] ON [t0].[Rank] = [t1].[Rank]
ORDER BY [t0].[Rank]");
    }

    public override async Task GroupBy_Property_Include_Select_Max(bool async)
    {
        await base.GroupBy_Property_Include_Select_Max(async);

        AssertSql(
            @"SELECT MAX([t].[SquadId])
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
GROUP BY [t].[Rank]");
    }

    public override async Task Include_with_group_by_and_FirstOrDefault_gets_properly_applied(bool async)
    {
        await base.Include_with_group_by_and_FirstOrDefault_gets_properly_applied(async);

        AssertSql(
            @"SELECT [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator], [t1].[Name], [t1].[Location], [t1].[Nation]
FROM (
    SELECT [t].[Rank]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    GROUP BY [t].[Rank]
) AS [t0]
LEFT JOIN (
    SELECT [t2].[Nickname], [t2].[SquadId], [t2].[AssignedCityName], [t2].[CityOfBirthName], [t2].[FullName], [t2].[HasSoulPatch], [t2].[LeaderNickname], [t2].[LeaderSquadId], [t2].[Rank], [t2].[Discriminator], [t2].[Name], [t2].[Location], [t2].[Nation]
    FROM (
        SELECT [t3].[Nickname], [t3].[SquadId], [t3].[AssignedCityName], [t3].[CityOfBirthName], [t3].[FullName], [t3].[HasSoulPatch], [t3].[LeaderNickname], [t3].[LeaderSquadId], [t3].[Rank], [t3].[Discriminator], [c].[Name], [c].[Location], [c].[Nation], ROW_NUMBER() OVER(PARTITION BY [t3].[Rank] ORDER BY [t3].[Nickname], [t3].[SquadId], [c].[Name]) AS [row]
        FROM (
            SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
            FROM [Gears] AS [g0]
            UNION ALL
            SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
            FROM [Officers] AS [o0]
        ) AS [t3]
        INNER JOIN [Cities] AS [c] ON [t3].[CityOfBirthName] = [c].[Name]
        WHERE [t3].[HasSoulPatch] = CAST(1 AS bit)
    ) AS [t2]
    WHERE [t2].[row] <= 1
) AS [t1] ON [t0].[Rank] = [t1].[Rank]");
    }

    public override async Task Include_collection_with_Cast_to_base(bool async)
    {
        await base.Include_collection_with_Cast_to_base(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Weapons] AS [w] ON [t].[FullName] = [w].[OwnerFullName]
WHERE [t].[Discriminator] = N'Officer'
ORDER BY [t].[Nickname], [t].[SquadId]");
    }

    public override async Task Include_with_client_method_and_member_access_still_applies_includes(bool async)
    {
        await base.Include_with_client_method_and_member_access_still_applies_includes(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Id], [t0].[GearNickName], [t0].[GearSquadId], [t0].[IssueDate], [t0].[Note]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName] AND [t].[SquadId] = [t0].[GearSquadId]");
    }

    public override async Task Include_with_projection_of_unmapped_property_still_gets_applied(bool async)
    {
        await base.Include_with_projection_of_unmapped_property_still_gets_applied(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Weapons] AS [w] ON [t].[FullName] = [w].[OwnerFullName]
ORDER BY [t].[Nickname], [t].[SquadId]");
    }

    public override async Task Multiple_includes_with_client_method_around_entity_and_also_projecting_included_collection()
    {
        await base.Multiple_includes_with_client_method_around_entity_and_also_projecting_included_collection();

        AssertSql(
            @"SELECT [s].[Name], [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[SynergyWithId]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    LEFT JOIN [Weapons] AS [w] ON [t].[FullName] = [w].[OwnerFullName]
) AS [t0] ON [s].[Id] = [t0].[SquadId]
WHERE [s].[Name] = N'Delta'
ORDER BY [s].[Id], [t0].[Nickname], [t0].[SquadId]");
    }

    public override async Task OrderBy_same_expression_containing_IsNull_correctly_deduplicates_the_ordering(bool async)
    {
        await base.OrderBy_same_expression_containing_IsNull_correctly_deduplicates_the_ordering(async);

        AssertSql(
            @"SELECT CASE
    WHEN [t].[LeaderNickname] IS NOT NULL THEN CASE
        WHEN CAST(LEN([t].[Nickname]) AS int) = 5 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    ELSE NULL
END
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
ORDER BY CASE
    WHEN CASE
        WHEN [t].[LeaderNickname] IS NOT NULL THEN CASE
            WHEN CAST(LEN([t].[Nickname]) AS int) = 5 THEN CAST(1 AS bit)
            ELSE CAST(0 AS bit)
        END
        ELSE NULL
    END IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
    }

    public override async Task GetValueOrDefault_in_projection(bool async)
    {
        await base.GetValueOrDefault_in_projection(async);

        AssertSql(
            @"SELECT COALESCE([w].[SynergyWithId], 0)
FROM [Weapons] AS [w]");
    }

    public override async Task GetValueOrDefault_in_filter(bool async)
    {
        await base.GetValueOrDefault_in_filter(async);

        AssertSql(
            @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE COALESCE([w].[SynergyWithId], 0) = 0");
    }

    public override async Task GetValueOrDefault_in_filter_non_nullable_column(bool async)
    {
        await base.GetValueOrDefault_in_filter_non_nullable_column(async);

        AssertSql(
            @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE COALESCE([w].[Id], 0) = 0");
    }

    public override async Task GetValueOrDefault_in_order_by(bool async)
    {
        await base.GetValueOrDefault_in_order_by(async);

        AssertSql(
            @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
ORDER BY COALESCE([w].[SynergyWithId], 0), [w].[Id]");
    }

    public override async Task GetValueOrDefault_with_argument(bool async)
    {
        await base.GetValueOrDefault_with_argument(async);

        AssertSql(
            @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE COALESCE([w].[SynergyWithId], [w].[Id]) = 1");
    }

    public override async Task GetValueOrDefault_with_argument_complex(bool async)
    {
        await base.GetValueOrDefault_with_argument_complex(async);

        AssertSql(
            @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE COALESCE([w].[SynergyWithId], CAST(LEN([w].[Name]) AS int) + 42) > 10");
    }

    public override async Task Filter_with_complex_predicate_containing_subquery(bool async)
    {
        await base.Filter_with_complex_predicate_containing_subquery(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[FullName] <> N'Dom' AND EXISTS (
    SELECT 1
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName] AND [w].[IsAutomatic] = CAST(1 AS bit))");
    }

    public override async Task Query_with_complex_let_containing_ordering_and_filter_projecting_firstOrDefault_element_of_let(
        bool async)
    {
        await base.Query_with_complex_let_containing_ordering_and_filter_projecting_firstOrDefault_element_of_let(async);

        AssertSql(
            @"SELECT [t].[Nickname], (
    SELECT TOP(1) [w].[Name]
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName] AND [w].[IsAutomatic] = CAST(1 AS bit)
    ORDER BY [w].[AmmunitionType] DESC) AS [WeaponName]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[Nickname] <> N'Dom'");
    }

    public override async Task
        Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation(bool async)
    {
        await base.Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE SUBSTRING([t].[Note], 0 + 1, [t0].[SquadId]) = [t].[GearNickName] OR (([t].[Note] IS NULL OR [t0].[SquadId] IS NULL) AND [t].[GearNickName] IS NULL)");
    }

    public override async Task
        Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation_complex(bool async)
    {
        await base.Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation_complex(
            async);

        AssertSql(
            @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
LEFT JOIN [Squads] AS [s] ON [t0].[SquadId] = [s].[Id]
WHERE SUBSTRING([t].[Note], 0 + 1, CAST(LEN([s].[Name]) AS int)) = [t].[GearNickName] OR (([t].[Note] IS NULL OR [s].[Name] IS NULL) AND [t].[GearNickName] IS NULL)");
    }

    public override async Task Filter_with_new_Guid(bool async)
    {
        await base.Filter_with_new_Guid(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note]
FROM [Tags] AS [t]
WHERE [t].[Id] = 'df36f493-463f-4123-83f9-6b135deeb7ba'");
    }

    public override async Task Filter_with_new_Guid_closure(bool async)
    {
        await base.Filter_with_new_Guid_closure(async);

        AssertSql();
    }

    public override async Task OfTypeNav1(bool async)
    {
        await base.OfTypeNav1(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName] AND [t].[SquadId] = [t0].[GearSquadId]
LEFT JOIN [Tags] AS [t1] ON [t].[Nickname] = [t1].[GearNickName] AND [t].[SquadId] = [t1].[GearSquadId]
WHERE ([t0].[Note] <> N'Foo' OR [t0].[Note] IS NULL) AND [t].[Discriminator] = N'Officer' AND ([t1].[Note] <> N'Bar' OR [t1].[Note] IS NULL)");
    }

    public override async Task OfTypeNav2(bool async)
    {
        await base.OfTypeNav2(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName] AND [t].[SquadId] = [t0].[GearSquadId]
LEFT JOIN [Cities] AS [c] ON [t].[AssignedCityName] = [c].[Name]
WHERE ([t0].[Note] <> N'Foo' OR [t0].[Note] IS NULL) AND [t].[Discriminator] = N'Officer' AND ([c].[Location] <> 'Bar' OR [c].[Location] IS NULL)");
    }

    public override async Task OfTypeNav3(bool async)
    {
        await base.OfTypeNav3(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName] AND [t].[SquadId] = [t0].[GearSquadId]
INNER JOIN [Weapons] AS [w] ON [t].[FullName] = [w].[OwnerFullName]
LEFT JOIN [Tags] AS [t1] ON [t].[Nickname] = [t1].[GearNickName] AND [t].[SquadId] = [t1].[GearSquadId]
WHERE ([t0].[Note] <> N'Foo' OR [t0].[Note] IS NULL) AND [t].[Discriminator] = N'Officer' AND ([t1].[Note] <> N'Bar' OR [t1].[Note] IS NULL)");
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

    public override async Task Nav_rewrite_with_convert1(bool async)
    {
        await base.Nav_rewrite_with_convert1(async);

        AssertSql(
            @"SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId]
FROM [LocustHordes] AS [l]
LEFT JOIN [Cities] AS [c] ON [l].[CapitalName] = [c].[Name]
LEFT JOIN [LocustCommanders] AS [l0] ON [l].[CommanderName] = [l0].[Name]
WHERE [c].[Name] <> N'Foo' OR [c].[Name] IS NULL");
    }

    public override async Task Nav_rewrite_with_convert2(bool async)
    {
        await base.Nav_rewrite_with_convert2(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[CapitalName], [l].[Name], [l].[ServerAddress], [l].[CommanderName], [l].[Eradicated]
FROM [LocustHordes] AS [l]
LEFT JOIN [Cities] AS [c] ON [l].[CapitalName] = [c].[Name]
LEFT JOIN [LocustCommanders] AS [l0] ON [l].[CommanderName] = [l0].[Name]
WHERE ([c].[Name] <> N'Foo' OR [c].[Name] IS NULL) AND ([l0].[Name] <> N'Bar' OR [l0].[Name] IS NULL)");
    }

    public override async Task Nav_rewrite_with_convert3(bool async)
    {
        await base.Nav_rewrite_with_convert3(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[CapitalName], [l].[Name], [l].[ServerAddress], [l].[CommanderName], [l].[Eradicated]
FROM [LocustHordes] AS [l]
LEFT JOIN [Cities] AS [c] ON [l].[CapitalName] = [c].[Name]
LEFT JOIN [LocustCommanders] AS [l0] ON [l].[CommanderName] = [l0].[Name]
WHERE ([c].[Name] <> N'Foo' OR [c].[Name] IS NULL) AND ([l0].[Name] <> N'Bar' OR [l0].[Name] IS NULL)");
    }

    public override async Task Where_contains_on_navigation_with_composite_keys(bool async)
    {
        await base.Where_contains_on_navigation_with_composite_keys(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE EXISTS (
    SELECT 1
    FROM [Cities] AS [c]
    WHERE EXISTS (
        SELECT 1
        FROM (
            SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
            FROM [Gears] AS [g0]
            UNION ALL
            SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
            FROM [Officers] AS [o0]
        ) AS [t0]
        WHERE [c].[Name] = [t0].[CityOfBirthName] AND [t0].[Nickname] = [t].[Nickname] AND [t0].[SquadId] = [t].[SquadId]))");
    }

    public override async Task Include_with_complex_order_by(bool async)
    {
        await base.Include_with_complex_order_by(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Weapons] AS [w0] ON [t].[FullName] = [w0].[OwnerFullName]
ORDER BY (
    SELECT TOP(1) [w].[Name]
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName] AND ([w].[Name] LIKE N'%Gnasher%')), [t].[Nickname], [t].[SquadId]");
    }

    public override async Task Anonymous_projection_take_followed_by_projecting_single_element_from_collection_navigation(bool async)
    {
        await base.Anonymous_projection_take_followed_by_projecting_single_element_from_collection_navigation(async);

        AssertSql(
            @"@__p_0='25'

SELECT [t1].[Id], [t1].[AmmunitionType], [t1].[IsAutomatic], [t1].[Name], [t1].[OwnerFullName], [t1].[SynergyWithId]
FROM (
    SELECT TOP(@__p_0) [t].[FullName]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
) AS [t0]
LEFT JOIN (
    SELECT [t2].[Id], [t2].[AmmunitionType], [t2].[IsAutomatic], [t2].[Name], [t2].[OwnerFullName], [t2].[SynergyWithId]
    FROM (
        SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], ROW_NUMBER() OVER(PARTITION BY [w].[OwnerFullName] ORDER BY [w].[Id]) AS [row]
        FROM [Weapons] AS [w]
    ) AS [t2]
    WHERE [t2].[row] <= 1
) AS [t1] ON [t0].[FullName] = [t1].[OwnerFullName]");
    }

    public override async Task Bool_projection_from_subquery_treated_appropriately_in_where(bool async)
    {
        await base.Bool_projection_from_subquery_treated_appropriately_in_where(async);

        AssertSql(
            @"SELECT [c].[Name], [c].[Location], [c].[Nation]
FROM [Cities] AS [c]
WHERE (
    SELECT TOP(1) [t].[HasSoulPatch]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    ORDER BY [t].[Nickname], [t].[SquadId]) = CAST(1 AS bit)");
    }

    public override async Task DateTimeOffset_Contains_Less_than_Greater_than(bool async)
    {
        await base.DateTimeOffset_Contains_Less_than_Greater_than(async);

        AssertSql(
            @"@__start_0='1902-01-01T10:00:00.1234567+01:30'
@__end_1='1902-01-03T10:00:00.1234567+01:30'

SELECT [m].[Id], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE @__start_0 <= CAST(CONVERT(date, [m].[Timeline]) AS datetimeoffset) AND [m].[Timeline] < @__end_1 AND [m].[Timeline] = '1902-01-02T10:00:00.1234567+01:30'");
    }

    public override async Task Navigation_inside_interpolated_string_expanded(bool async)
    {
        await base.Navigation_inside_interpolated_string_expanded(async);

        AssertSql(
            @"SELECT CASE
    WHEN [w].[SynergyWithId] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [w0].[OwnerFullName]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]");
    }

    public override async Task Left_join_projection_using_coalesce_tracking(bool async)
    {
        await base.Left_join_projection_using_coalesce_tracking(async);

        AssertSql(
            @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator], [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON [t].[LeaderNickname] = [t0].[Nickname]");
    }

    public override async Task Left_join_projection_using_conditional_tracking(bool async)
    {
        await base.Left_join_projection_using_conditional_tracking(async);

        AssertSql(
            @"SELECT CASE
    WHEN [t0].[Nickname] IS NULL OR [t0].[SquadId] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON [t].[LeaderNickname] = [t0].[Nickname]");
    }

    public override async Task Project_collection_navigation_nested_with_take_composite_key(bool async)
    {
        await base.Project_collection_navigation_nested_with_take_composite_key(async);

        AssertSql(
            @"SELECT [t].[Id], [t0].[Nickname], [t0].[SquadId], [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
LEFT JOIN (
    SELECT [t2].[Nickname], [t2].[SquadId], [t2].[AssignedCityName], [t2].[CityOfBirthName], [t2].[FullName], [t2].[HasSoulPatch], [t2].[LeaderNickname], [t2].[LeaderSquadId], [t2].[Rank], [t2].[Discriminator]
    FROM (
        SELECT [t3].[Nickname], [t3].[SquadId], [t3].[AssignedCityName], [t3].[CityOfBirthName], [t3].[FullName], [t3].[HasSoulPatch], [t3].[LeaderNickname], [t3].[LeaderSquadId], [t3].[Rank], [t3].[Discriminator], ROW_NUMBER() OVER(PARTITION BY [t3].[LeaderNickname], [t3].[LeaderSquadId] ORDER BY [t3].[Nickname], [t3].[SquadId]) AS [row]
        FROM (
            SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
            FROM [Gears] AS [g0]
            UNION ALL
            SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
            FROM [Officers] AS [o0]
        ) AS [t3]
    ) AS [t2]
    WHERE [t2].[row] <= 50
) AS [t1] ON ([t0].[Nickname] = [t1].[LeaderNickname] OR ([t0].[Nickname] IS NULL AND [t1].[LeaderNickname] IS NULL)) AND [t0].[SquadId] = [t1].[LeaderSquadId]
WHERE [t0].[Discriminator] = N'Officer'
ORDER BY [t].[Id], [t0].[Nickname], [t0].[SquadId], [t1].[Nickname]");
    }

    public override async Task Project_collection_navigation_nested_composite_key(bool async)
    {
        await base.Project_collection_navigation_nested_composite_key(async);

        AssertSql(
            @"SELECT [t].[Id], [t0].[Nickname], [t0].[SquadId], [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
LEFT JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t1] ON ([t0].[Nickname] = [t1].[LeaderNickname] OR ([t0].[Nickname] IS NULL AND [t1].[LeaderNickname] IS NULL)) AND [t0].[SquadId] = [t1].[LeaderSquadId]
WHERE [t0].[Discriminator] = N'Officer'
ORDER BY [t].[Id], [t0].[Nickname], [t0].[SquadId], [t1].[Nickname]");
    }

    public override async Task Null_checks_in_correlated_predicate_are_correctly_translated(bool async)
    {
        await base.Null_checks_in_correlated_predicate_are_correctly_translated(async);

        AssertSql(
            @"SELECT [t].[Id], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId] AND [t].[Note] IS NOT NULL
ORDER BY [t].[Id], [t0].[Nickname]");
    }

    public override async Task SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector(bool async)
    {
        await base.SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector(async);

        AssertSql(
            @"@__isAutomatic_0='True'

SELECT [t].[Nickname], [t].[FullName], CASE
    WHEN [t0].[Id] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Collection]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [w].[Id], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
    WHERE [w].[IsAutomatic] = @__isAutomatic_0
) AS [t0] ON [t].[FullName] = [t0].[OwnerFullName]");
    }

    public override async Task Join_with_inner_being_a_subquery_projecting_single_property(bool async)
    {
        await base.Join_with_inner_being_a_subquery_projecting_single_property(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON [t].[Nickname] = [t0].[Nickname]");
    }

    public override async Task Join_with_inner_being_a_subquery_projecting_anonymous_type_with_single_property(bool async)
    {
        await base.Join_with_inner_being_a_subquery_projecting_anonymous_type_with_single_property(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0] ON [t].[Nickname] = [t0].[Nickname]");
    }

    public override async Task Navigation_based_on_complex_expression1(bool async)
    {
        await base.Navigation_based_on_complex_expression1(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[CapitalName], [l].[Name], [l].[ServerAddress], [l].[CommanderName], [l].[Eradicated]
FROM [LocustHordes] AS [l]
LEFT JOIN [LocustCommanders] AS [l0] ON [l].[CommanderName] = [l0].[Name]
WHERE [l0].[Name] IS NOT NULL");
    }

    public override async Task Navigation_based_on_complex_expression2(bool async)
    {
        await base.Navigation_based_on_complex_expression2(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[CapitalName], [l].[Name], [l].[ServerAddress], [l].[CommanderName], [l].[Eradicated]
FROM [LocustHordes] AS [l]
LEFT JOIN [LocustCommanders] AS [l0] ON [l].[CommanderName] = [l0].[Name]
WHERE [l0].[Name] IS NOT NULL");
    }

    public override async Task Navigation_based_on_complex_expression3(bool async)
    {
        await base.Navigation_based_on_complex_expression3(async);

        AssertSql(
            @"SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId]
FROM [LocustHordes] AS [l]
LEFT JOIN [LocustCommanders] AS [l0] ON [l].[CommanderName] = [l0].[Name]");
    }

    public override async Task Navigation_based_on_complex_expression4(bool async)
    {
        await base.Navigation_based_on_complex_expression4(async);

        AssertSql(
            @"SELECT CAST(1 AS bit), [l2].[Name], [l2].[LocustHordeId], [l2].[ThreatLevel], [l2].[ThreatLevelByte], [l2].[ThreatLevelNullableByte], [l2].[DefeatedByNickname], [l2].[DefeatedBySquadId], [l2].[HighCommandId], [t0].[Name], [t0].[LocustHordeId], [t0].[ThreatLevel], [t0].[ThreatLevelByte], [t0].[ThreatLevelNullableByte], [t0].[DefeatedByNickname], [t0].[DefeatedBySquadId], [t0].[HighCommandId], [t0].[Discriminator]
FROM [LocustHordes] AS [l]
CROSS JOIN (
    SELECT [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator]
    FROM (
        SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
        FROM [LocustLeaders] AS [l0]
        UNION ALL
        SELECT [l1].[Name], [l1].[LocustHordeId], [l1].[ThreatLevel], [l1].[ThreatLevelByte], [l1].[ThreatLevelNullableByte], [l1].[DefeatedByNickname], [l1].[DefeatedBySquadId], [l1].[HighCommandId], N'LocustCommander' AS [Discriminator]
        FROM [LocustCommanders] AS [l1]
    ) AS [t]
    WHERE [t].[Discriminator] = N'LocustCommander'
) AS [t0]
LEFT JOIN [LocustCommanders] AS [l2] ON [l].[CommanderName] = [l2].[Name]");
    }

    public override async Task Navigation_based_on_complex_expression5(bool async)
    {
        await base.Navigation_based_on_complex_expression5(async);

        AssertSql(
            @"SELECT [l2].[Name], [l2].[LocustHordeId], [l2].[ThreatLevel], [l2].[ThreatLevelByte], [l2].[ThreatLevelNullableByte], [l2].[DefeatedByNickname], [l2].[DefeatedBySquadId], [l2].[HighCommandId], [t0].[Name], [t0].[LocustHordeId], [t0].[ThreatLevel], [t0].[ThreatLevelByte], [t0].[ThreatLevelNullableByte], [t0].[DefeatedByNickname], [t0].[DefeatedBySquadId], [t0].[HighCommandId], [t0].[Discriminator]
FROM [LocustHordes] AS [l]
CROSS JOIN (
    SELECT [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator]
    FROM (
        SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
        FROM [LocustLeaders] AS [l0]
        UNION ALL
        SELECT [l1].[Name], [l1].[LocustHordeId], [l1].[ThreatLevel], [l1].[ThreatLevelByte], [l1].[ThreatLevelNullableByte], [l1].[DefeatedByNickname], [l1].[DefeatedBySquadId], [l1].[HighCommandId], N'LocustCommander' AS [Discriminator]
        FROM [LocustCommanders] AS [l1]
    ) AS [t]
    WHERE [t].[Discriminator] = N'LocustCommander'
) AS [t0]
LEFT JOIN [LocustCommanders] AS [l2] ON [l].[CommanderName] = [l2].[Name]");
    }

    public override async Task Navigation_based_on_complex_expression6(bool async)
    {
        await base.Navigation_based_on_complex_expression6(async);

        AssertSql(
            @"SELECT CASE
    WHEN [l2].[Name] = N'Queen Myrrah' AND [l2].[Name] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [l2].[Name], [l2].[LocustHordeId], [l2].[ThreatLevel], [l2].[ThreatLevelByte], [l2].[ThreatLevelNullableByte], [l2].[DefeatedByNickname], [l2].[DefeatedBySquadId], [l2].[HighCommandId], [t0].[Name], [t0].[LocustHordeId], [t0].[ThreatLevel], [t0].[ThreatLevelByte], [t0].[ThreatLevelNullableByte], [t0].[DefeatedByNickname], [t0].[DefeatedBySquadId], [t0].[HighCommandId], [t0].[Discriminator]
FROM [LocustHordes] AS [l]
CROSS JOIN (
    SELECT [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator]
    FROM (
        SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
        FROM [LocustLeaders] AS [l0]
        UNION ALL
        SELECT [l1].[Name], [l1].[LocustHordeId], [l1].[ThreatLevel], [l1].[ThreatLevelByte], [l1].[ThreatLevelNullableByte], [l1].[DefeatedByNickname], [l1].[DefeatedBySquadId], [l1].[HighCommandId], N'LocustCommander' AS [Discriminator]
        FROM [LocustCommanders] AS [l1]
    ) AS [t]
    WHERE [t].[Discriminator] = N'LocustCommander'
) AS [t0]
LEFT JOIN [LocustCommanders] AS [l2] ON [l].[CommanderName] = [l2].[Name]");
    }

    public override async Task Select_as_operator(bool async)
    {
        await base.Select_as_operator(async);

        AssertSql(
            @"SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
FROM [LocustLeaders] AS [l]
UNION ALL
SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
FROM [LocustCommanders] AS [l0]");
    }

    public override async Task Select_datetimeoffset_comparison_in_projection(bool async)
    {
        await base.Select_datetimeoffset_comparison_in_projection(async);

        AssertSql(
            @"SELECT CASE
    WHEN [m].[Timeline] > SYSDATETIMEOFFSET() THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Missions] AS [m]");
    }

    public override async Task OfType_in_subquery_works(bool async)
    {
        await base.OfType_in_subquery_works(async);

        AssertSql(
            @"SELECT [t0].[Name], [t0].[Location], [t0].[Nation]
FROM [Officers] AS [o]
INNER JOIN (
    SELECT [c].[Name], [c].[Location], [c].[Nation], [t].[LeaderNickname], [t].[LeaderSquadId]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t]
    LEFT JOIN [Cities] AS [c] ON [t].[AssignedCityName] = [c].[Name]
    WHERE [t].[Discriminator] = N'Officer'
) AS [t0] ON [o].[Nickname] = [t0].[LeaderNickname] AND [o].[SquadId] = [t0].[LeaderSquadId]");
    }

    public override async Task Nullable_bool_comparison_is_translated_to_server(bool async)
    {
        await base.Nullable_bool_comparison_is_translated_to_server(async);

        AssertSql(
            @"SELECT CASE
    WHEN [l].[Eradicated] = CAST(1 AS bit) AND ([l].[Eradicated] IS NOT NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsEradicated]
FROM [LocustHordes] AS [l]");
    }

    public override async Task Accessing_reference_navigation_collection_composition_generates_single_query(bool async)
    {
        await base.Accessing_reference_navigation_collection_composition_generates_single_query(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t0].[Id], [t0].[IsAutomatic], [t0].[Name], [t0].[Id0]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [w].[Id], [w].[IsAutomatic], [w0].[Name], [w0].[Id] AS [Id0], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
    LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
) AS [t0] ON [t].[FullName] = [t0].[OwnerFullName]
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Id]");
    }

    public override async Task Reference_include_chain_loads_correctly_when_middle_is_null(bool async)
    {
        await base.Reference_include_chain_loads_correctly_when_middle_is_null(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[IssueDate], [t].[Note], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator], [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
LEFT JOIN [Squads] AS [s] ON [t0].[SquadId] = [s].[Id]
ORDER BY [t].[Note]");
    }

    public override async Task Accessing_property_of_optional_navigation_in_child_projection_works(bool async)
    {
        await base.Accessing_property_of_optional_navigation_in_child_projection_works(async);

        AssertSql(
            @"SELECT CASE
    WHEN [t0].[Nickname] IS NOT NULL AND [t0].[SquadId] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [t].[Id], [t0].[Nickname], [t0].[SquadId], [t1].[Nickname], [t1].[Id], [t1].[SquadId]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
LEFT JOIN (
    SELECT [t2].[Nickname], [w].[Id], [t2].[SquadId], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
    LEFT JOIN (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t2] ON [w].[OwnerFullName] = [t2].[FullName]
) AS [t1] ON [t0].[FullName] = [t1].[OwnerFullName]
ORDER BY [t].[Note], [t].[Id], [t0].[Nickname], [t0].[SquadId], [t1].[Id], [t1].[Nickname]");
    }

    public override async Task Collection_navigation_ofType_filter_works(bool async)
    {
        await base.Collection_navigation_ofType_filter_works(async);

        AssertSql(
            @"SELECT [c].[Name], [c].[Location], [c].[Nation]
FROM [Cities] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    WHERE [c].[Name] = [t].[CityOfBirthName] AND [t].[Discriminator] = N'Officer' AND [t].[Nickname] = N'Marcus')");
    }

    public override async Task Query_reusing_parameter_doesnt_declare_duplicate_parameter(bool async)
    {
        await base.Query_reusing_parameter_doesnt_declare_duplicate_parameter(async);

        AssertSql(
            @"@__prm_Inner_Nickname_0='Marcus' (Size = 450)

SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM (
    SELECT DISTINCT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    WHERE [t].[Nickname] <> @__prm_Inner_Nickname_0 AND [t].[Nickname] <> @__prm_Inner_Nickname_0
) AS [t0]
ORDER BY [t0].[FullName]");
    }

    public override async Task Query_reusing_parameter_with_inner_query_doesnt_declare_duplicate_parameter(bool async)
    {
        await base.Query_reusing_parameter_with_inner_query_doesnt_declare_duplicate_parameter(async);

        AssertSql(
            @"@__squadId_0='1'

SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM (
    SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    INNER JOIN [Squads] AS [s] ON [t].[SquadId] = [s].[Id]
    WHERE EXISTS (
        SELECT 1
        FROM [Squads] AS [s0]
        WHERE [s0].[Id] = @__squadId_0 AND [s0].[Id] = [s].[Id])
    UNION ALL
    SELECT [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t1]
    INNER JOIN [Squads] AS [s1] ON [t1].[SquadId] = [s1].[Id]
    WHERE EXISTS (
        SELECT 1
        FROM [Squads] AS [s2]
        WHERE [s2].[Id] = @__squadId_0 AND [s2].[Id] = [s1].[Id])
) AS [t0]
ORDER BY [t0].[FullName]");
    }

    public override async Task Query_reusing_parameter_with_inner_query_expression_doesnt_declare_duplicate_parameter(bool async)
    {
        await base.Query_reusing_parameter_with_inner_query_expression_doesnt_declare_duplicate_parameter(async);

        AssertSql(
            @"@__gearId_0='1'

SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    WHERE [s].[Id] = [t].[SquadId] AND [t].[SquadId] = @__gearId_0 AND [t].[SquadId] = @__gearId_0)");
    }

    public override async Task Query_reusing_parameter_doesnt_declare_duplicate_parameter_complex(bool async)
    {
        await base.Query_reusing_parameter_doesnt_declare_duplicate_parameter_complex(async);

        AssertSql(
            @"@__entity_equality_prm_Inner_Squad_0_Id='1' (Nullable = true)

SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM (
    SELECT DISTINCT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    INNER JOIN [Squads] AS [s] ON [t].[SquadId] = [s].[Id]
    WHERE [s].[Id] = @__entity_equality_prm_Inner_Squad_0_Id
) AS [t0]
INNER JOIN [Squads] AS [s0] ON [t0].[SquadId] = [s0].[Id]
WHERE [s0].[Id] = @__entity_equality_prm_Inner_Squad_0_Id
ORDER BY [t0].[FullName]");
    }

    public override async Task Complex_GroupBy_after_set_operator(bool async)
    {
        await base.Complex_GroupBy_after_set_operator(async);

        AssertSql(
            @"SELECT [t0].[Name], [t0].[Count], COALESCE(SUM([t0].[Count]), 0) AS [Sum]
FROM (
    SELECT [c].[Name], (
        SELECT COUNT(*)
        FROM [Weapons] AS [w]
        WHERE [t].[FullName] = [w].[OwnerFullName]) AS [Count]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    LEFT JOIN [Cities] AS [c] ON [t].[AssignedCityName] = [c].[Name]
    UNION ALL
    SELECT [c0].[Name], (
        SELECT COUNT(*)
        FROM [Weapons] AS [w0]
        WHERE [t1].[FullName] = [w0].[OwnerFullName]) AS [Count]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t1]
    INNER JOIN [Cities] AS [c0] ON [t1].[CityOfBirthName] = [c0].[Name]
) AS [t0]
GROUP BY [t0].[Name], [t0].[Count]");
    }

    public override async Task Complex_GroupBy_after_set_operator_using_result_selector(bool async)
    {
        await base.Complex_GroupBy_after_set_operator_using_result_selector(async);

        AssertSql(
            @"SELECT [t0].[Name], [t0].[Count], COALESCE(SUM([t0].[Count]), 0) AS [Sum]
FROM (
    SELECT [c].[Name], (
        SELECT COUNT(*)
        FROM [Weapons] AS [w]
        WHERE [t].[FullName] = [w].[OwnerFullName]) AS [Count]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    LEFT JOIN [Cities] AS [c] ON [t].[AssignedCityName] = [c].[Name]
    UNION ALL
    SELECT [c0].[Name], (
        SELECT COUNT(*)
        FROM [Weapons] AS [w0]
        WHERE [t1].[FullName] = [w0].[OwnerFullName]) AS [Count]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t1]
    INNER JOIN [Cities] AS [c0] ON [t1].[CityOfBirthName] = [c0].[Name]
) AS [t0]
GROUP BY [t0].[Name], [t0].[Count]");
    }

    public override async Task Left_join_with_GroupBy_with_composite_group_key(bool async)
    {
        await base.Left_join_with_GroupBy_with_composite_group_key(async);

        AssertSql(
            @"SELECT [t].[CityOfBirthName], [t].[HasSoulPatch]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN [Squads] AS [s] ON [t].[SquadId] = [s].[Id]
LEFT JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName]
GROUP BY [t].[CityOfBirthName], [t].[HasSoulPatch]");
    }

    public override async Task GroupBy_with_boolean_grouping_key(bool async)
    {
        await base.GroupBy_with_boolean_grouping_key(async);

        AssertSql(
            @"SELECT [t0].[CityOfBirthName], [t0].[HasSoulPatch], [t0].[IsMarcus], COUNT(*) AS [Count]
FROM (
    SELECT [t].[CityOfBirthName], [t].[HasSoulPatch], CASE
        WHEN [t].[Nickname] = N'Marcus' THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsMarcus]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
) AS [t0]
GROUP BY [t0].[CityOfBirthName], [t0].[HasSoulPatch], [t0].[IsMarcus]");
    }

    public override async Task GroupBy_with_boolean_groupin_key_thru_navigation_access(bool async)
    {
        await base.GroupBy_with_boolean_groupin_key_thru_navigation_access(async);

        AssertSql(
            @"SELECT [t0].[HasSoulPatch], LOWER([s].[Name]) AS [Name]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
LEFT JOIN [Squads] AS [s] ON [t0].[SquadId] = [s].[Id]
GROUP BY [t0].[HasSoulPatch], [s].[Name]");
    }

    public override async Task Group_by_over_projection_with_multiple_properties_accessed_thru_navigation(bool async)
    {
        await base.Group_by_over_projection_with_multiple_properties_accessed_thru_navigation(async);

        AssertSql(
            @"SELECT [c].[Name]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN [Cities] AS [c] ON [t].[CityOfBirthName] = [c].[Name]
GROUP BY [c].[Name]");
    }

    public override async Task Group_by_on_StartsWith_with_null_parameter_as_argument(bool async)
    {
        await base.Group_by_on_StartsWith_with_null_parameter_as_argument(async);

        AssertSql(
            @"SELECT [t0].[Key]
FROM (
    SELECT CAST(0 AS bit) AS [Key]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
) AS [t0]
GROUP BY [t0].[Key]");
    }

    public override async Task Group_by_with_having_StartsWith_with_null_parameter_as_argument(bool async)
    {
        await base.Group_by_with_having_StartsWith_with_null_parameter_as_argument(async);

        AssertSql(
            @"SELECT [t].[FullName]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
GROUP BY [t].[FullName]
HAVING 0 = 1");
    }

    public override async Task Select_StartsWith_with_null_parameter_as_argument(bool async)
    {
        await base.Select_StartsWith_with_null_parameter_as_argument(async);

        AssertSql(
            @"SELECT CAST(0 AS bit)
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task Select_null_parameter_is_not_null(bool async)
    {
        await base.Select_null_parameter_is_not_null(async);

        AssertSql(
            @"@__p_0='False'

SELECT @__p_0
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task Where_null_parameter_is_not_null(bool async)
    {
        await base.Where_null_parameter_is_not_null(async);

        AssertSql(
            @"@__p_0='False'

SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE @__p_0 = CAST(1 AS bit)");
    }

    public override async Task OrderBy_StartsWith_with_null_parameter_as_argument(bool async)
    {
        await base.OrderBy_StartsWith_with_null_parameter_as_argument(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
ORDER BY [t].[Nickname]");
    }

    public override async Task OrderBy_Contains_empty_list(bool async)
    {
        await base.OrderBy_Contains_empty_list(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task Where_with_enum_flags_parameter(bool async)
    {
        await base.Where_with_enum_flags_parameter(async);

        AssertSql(
            @"@__rank_0='1' (Nullable = true)

SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE ([t].[Rank] & @__rank_0) = @__rank_0",
            //
            @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
FROM [Gears] AS [g]
UNION ALL
SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
FROM [Officers] AS [o]",
            //
            @"@__rank_0='2' (Nullable = true)

SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE ([t].[Rank] | @__rank_0) <> @__rank_0",
            //
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE 0 = 1");
    }

    public override async Task FirstOrDefault_navigation_access_entity_equality_in_where_predicate_apply_peneding_selector(bool async)
    {
        await base.FirstOrDefault_navigation_access_entity_equality_in_where_predicate_apply_peneding_selector(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[CapitalName], [l].[Name], [l].[ServerAddress], [l].[CommanderName], [l].[Eradicated]
FROM [LocustHordes] AS [l]
LEFT JOIN [Cities] AS [c] ON [l].[CapitalName] = [c].[Name]
WHERE [c].[Name] = (
    SELECT TOP(1) [c0].[Name]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    INNER JOIN [Cities] AS [c0] ON [t].[CityOfBirthName] = [c0].[Name]
    ORDER BY [t].[Nickname]) OR ([c].[Name] IS NULL AND (
    SELECT TOP(1) [c0].[Name]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    INNER JOIN [Cities] AS [c0] ON [t].[CityOfBirthName] = [c0].[Name]
    ORDER BY [t].[Nickname]) IS NULL)");
    }

    public override async Task Bitwise_operation_with_non_null_parameter_optimizes_null_checks(bool async)
    {
        await base.Bitwise_operation_with_non_null_parameter_optimizes_null_checks(async);

        AssertSql(
            @"@__ranks_0='134'

SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE ([t].[Rank] & @__ranks_0) <> 0",
            //
            @"@__ranks_0='134'

SELECT CASE
    WHEN ([t].[Rank] | @__ranks_0) = @__ranks_0 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]",
            //
            @"@__ranks_0='134'

SELECT CASE
    WHEN ([t].[Rank] | ([t].[Rank] | (@__ranks_0 | ([t].[Rank] | @__ranks_0)))) = @__ranks_0 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task Bitwise_operation_with_null_arguments(bool async)
    {
        await base.Bitwise_operation_with_null_arguments(async);

        AssertSql(
            @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] IS NULL",
            //
            @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] IS NULL",
            //
            @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] IS NULL",
            //
            @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]",
            //
            @"@__prm_0='2' (Nullable = true)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & @__prm_0) <> 0 OR [w].[AmmunitionType] IS NULL",
            //
            @"@__prm_0='1' (Nullable = true)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE ([w].[AmmunitionType] & @__prm_0) = @__prm_0");
    }

    public override async Task Logical_operation_with_non_null_parameter_optimizes_null_checks(bool async)
    {
        await base.Logical_operation_with_non_null_parameter_optimizes_null_checks(async);

        AssertSql(
            @"@__prm_0='True'

SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] <> @__prm_0",
            //
            @"@__prm_0='False'

SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] <> @__prm_0");
    }

    public override async Task Cast_OfType_works_correctly(bool async)
    {
        await base.Cast_OfType_works_correctly(async);

        AssertSql(
            @"SELECT [t].[FullName]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[Discriminator] = N'Officer'");
    }

    public override async Task Join_inner_source_custom_projection_followed_by_filter(bool async)
    {
        await base.Join_inner_source_custom_projection_followed_by_filter(async);

        AssertSql(
            @"SELECT CASE
    WHEN [l1].[Name] = N'Locust' THEN CAST(1 AS bit)
    ELSE NULL
END AS [IsEradicated], [l1].[CommanderName], [l1].[Name]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
INNER JOIN [LocustHordes] AS [l1] ON [t].[Name] = [l1].[CommanderName]
WHERE CASE
    WHEN [l1].[Name] = N'Locust' THEN CAST(1 AS bit)
    ELSE NULL
END <> CAST(1 AS bit) OR (CASE
    WHEN [l1].[Name] = N'Locust' THEN CAST(1 AS bit)
    ELSE NULL
END IS NULL)");
    }

    public override async Task Byte_array_contains_literal(bool async)
    {
        await base.Byte_array_contains_literal(async);

        AssertSql(
            @"SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE CHARINDEX(0x01, [s].[Banner]) > 0");
    }

    public override async Task Byte_array_filter_by_length_literal(bool async)
    {
        await base.Byte_array_filter_by_length_literal(async);

        AssertSql(
            @"SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE CAST(DATALENGTH([s].[Banner]) AS int) = 1");
    }

    public override async Task Byte_array_filter_by_length_parameter(bool async)
    {
        await base.Byte_array_filter_by_length_parameter(async);

        AssertSql(
            @"@__p_0='1'

SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE CAST(DATALENGTH([s].[Banner]) AS int) = @__p_0");
    }

    public override void Byte_array_filter_by_length_parameter_compiled()
    {
        base.Byte_array_filter_by_length_parameter_compiled();

        AssertSql(
            @"@__byteArrayParam='0x2A80' (Size = 8000)

SELECT COUNT(*)
FROM [Squads] AS [s]
WHERE CAST(DATALENGTH([s].[Banner]) AS int) = CAST(DATALENGTH(@__byteArrayParam) AS int)");
    }

    public override async Task Byte_array_contains_parameter(bool async)
    {
        await base.Byte_array_contains_parameter(async);

        AssertSql(
            @"@__someByte_0='1' (Size = 1)

SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE CHARINDEX(CAST(@__someByte_0 AS varbinary(max)), [s].[Banner]) > 0");
    }

    public override async Task Byte_array_filter_by_length_literal_does_not_cast_on_varbinary_n(bool async)
    {
        await base.Byte_array_filter_by_length_literal_does_not_cast_on_varbinary_n(async);

        AssertSql(
            @"SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE DATALENGTH([s].[Banner5]) = 5");
    }

    public override async Task Conditional_expression_with_test_being_simplified_to_constant_simple(bool isAsync)
    {
        await base.Conditional_expression_with_test_being_simplified_to_constant_simple(isAsync);

        AssertSql(
            @"@__prm_0='True'

SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE CASE
    WHEN [t].[HasSoulPatch] = @__prm_0 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CAST(1 AS bit)");
    }

    public override async Task Conditional_expression_with_test_being_simplified_to_constant_complex(bool isAsync)
    {
        await base.Conditional_expression_with_test_being_simplified_to_constant_complex(isAsync);

        AssertSql(
            @"@__prm_0='True'
@__prm2_1='Dom's Lancer' (Size = 4000)

SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE CASE
    WHEN [t].[HasSoulPatch] = @__prm_0 THEN CASE
        WHEN (
            SELECT TOP(1) [w].[Name]
            FROM [Weapons] AS [w]
            WHERE [w].[Id] = [t].[SquadId]) = @__prm2_1 AND (
            SELECT TOP(1) [w].[Name]
            FROM [Weapons] AS [w]
            WHERE [w].[Id] = [t].[SquadId]) IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    ELSE CAST(0 AS bit)
END = CAST(1 AS bit)");
    }

    public override async Task OrderBy_bool_coming_from_optional_navigation(bool async)
    {
        await base.OrderBy_bool_coming_from_optional_navigation(async);

        AssertSql(
            @"SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
ORDER BY [w0].[IsAutomatic]");
    }

    public override async Task DateTimeOffset_Date_returns_datetime(bool async)
    {
        await base.DateTimeOffset_Date_returns_datetime(async);

        AssertSql(
            @"@__dateTimeOffset_Date_0='0002-03-01T00:00:00.0000000'

SELECT [m].[Id], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE CONVERT(date, [m].[Timeline]) >= @__dateTimeOffset_Date_0");
    }

    public override async Task Conditional_with_conditions_evaluating_to_false_gets_optimized(bool async)
    {
        await base.Conditional_with_conditions_evaluating_to_false_gets_optimized(async);

        AssertSql(
            @"SELECT [t].[FullName]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task Conditional_with_conditions_evaluating_to_true_gets_optimized(bool async)
    {
        await base.Conditional_with_conditions_evaluating_to_true_gets_optimized(async);

        AssertSql(
            @"SELECT [t].[CityOfBirthName]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task Projecting_required_string_column_compared_to_null_parameter(bool async)
    {
        await base.Projecting_required_string_column_compared_to_null_parameter(async);

        AssertSql(
            @"SELECT CAST(0 AS bit)
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task Byte_array_filter_by_SequenceEqual(bool isAsync)
    {
        await base.Byte_array_filter_by_SequenceEqual(isAsync);

        AssertSql(
            @"@__byteArrayParam_0='0x0405060708' (Size = 5)

SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE [s].[Banner5] = @__byteArrayParam_0");
    }

    public override async Task Group_by_nullable_property_HasValue_and_project_the_grouping_key(bool async)
    {
        await base.Group_by_nullable_property_HasValue_and_project_the_grouping_key(async);

        AssertSql(
            @"SELECT [t].[Key]
FROM (
    SELECT CASE
        WHEN [w].[SynergyWithId] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [Key]
    FROM [Weapons] AS [w]
) AS [t]
GROUP BY [t].[Key]");
    }

    public override async Task Group_by_nullable_property_and_project_the_grouping_key_HasValue(bool async)
    {
        await base.Group_by_nullable_property_and_project_the_grouping_key_HasValue(async);

        AssertSql(
            @"SELECT CASE
    WHEN [w].[SynergyWithId] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Weapons] AS [w]
GROUP BY [w].[SynergyWithId]");
    }

    public override async Task Checked_context_with_cast_does_not_fail(bool isAsync)
    {
        await base.Checked_context_with_cast_does_not_fail(isAsync);

        AssertSql(
            @"SELECT [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
WHERE CAST([t].[ThreatLevel] AS tinyint) >= CAST(5 AS tinyint)");
    }

    public override async Task Checked_context_with_addition_does_not_fail(bool isAsync)
    {
        await base.Checked_context_with_addition_does_not_fail(isAsync);

        AssertSql(
            @"SELECT [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
WHERE CAST([t].[ThreatLevel] AS bigint) >= (CAST(5 AS bigint) + CAST([t].[ThreatLevel] AS bigint))");
    }

    public override async Task TimeSpan_Hours(bool async)
    {
        await base.TimeSpan_Hours(async);

        AssertSql(
            @"SELECT DATEPART(hour, [m].[Duration])
FROM [Missions] AS [m]");
    }

    public override async Task TimeSpan_Minutes(bool async)
    {
        await base.TimeSpan_Minutes(async);

        AssertSql(
            @"SELECT DATEPART(minute, [m].[Duration])
FROM [Missions] AS [m]");
    }

    public override async Task TimeSpan_Seconds(bool async)
    {
        await base.TimeSpan_Seconds(async);

        AssertSql(
            @"SELECT DATEPART(second, [m].[Duration])
FROM [Missions] AS [m]");
    }

    public override async Task TimeSpan_Milliseconds(bool async)
    {
        await base.TimeSpan_Milliseconds(async);

        AssertSql(
            @"SELECT DATEPART(millisecond, [m].[Duration])
FROM [Missions] AS [m]");
    }

    public override async Task Where_TimeSpan_Hours(bool async)
    {
        await base.Where_TimeSpan_Hours(async);

        AssertSql(
            @"SELECT [m].[Id], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(hour, [m].[Duration]) = 1");
    }

    public override async Task Where_TimeSpan_Minutes(bool async)
    {
        await base.Where_TimeSpan_Minutes(async);

        AssertSql(
            @"SELECT [m].[Id], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(minute, [m].[Duration]) = 1");
    }

    public override async Task Where_TimeSpan_Seconds(bool async)
    {
        await base.Where_TimeSpan_Seconds(async);

        AssertSql(
            @"SELECT [m].[Id], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(second, [m].[Duration]) = 1");
    }

    public override async Task Where_TimeSpan_Milliseconds(bool async)
    {
        await base.Where_TimeSpan_Milliseconds(async);

        AssertSql(
            @"SELECT [m].[Id], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE DATEPART(millisecond, [m].[Duration]) = 1");
    }

    public override async Task Contains_on_collection_of_byte_subquery(bool async)
    {
        await base.Contains_on_collection_of_byte_subquery(async);

        AssertSql(
            @"SELECT [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [l1].[Name], [l1].[LocustHordeId], [l1].[ThreatLevel], [l1].[ThreatLevelByte], [l1].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
        FROM [LocustLeaders] AS [l1]
        UNION ALL
        SELECT [l2].[Name], [l2].[LocustHordeId], [l2].[ThreatLevel], [l2].[ThreatLevelByte], [l2].[ThreatLevelNullableByte], [l2].[DefeatedByNickname], [l2].[DefeatedBySquadId], [l2].[HighCommandId], N'LocustCommander' AS [Discriminator]
        FROM [LocustCommanders] AS [l2]
    ) AS [t0]
    WHERE [t0].[ThreatLevelByte] = [t].[ThreatLevelByte])");
    }

    public override async Task Contains_on_collection_of_nullable_byte_subquery(bool async)
    {
        await base.Contains_on_collection_of_nullable_byte_subquery(async);

        AssertSql(
            @"SELECT [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [l1].[Name], [l1].[LocustHordeId], [l1].[ThreatLevel], [l1].[ThreatLevelByte], [l1].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
        FROM [LocustLeaders] AS [l1]
        UNION ALL
        SELECT [l2].[Name], [l2].[LocustHordeId], [l2].[ThreatLevel], [l2].[ThreatLevelByte], [l2].[ThreatLevelNullableByte], [l2].[DefeatedByNickname], [l2].[DefeatedBySquadId], [l2].[HighCommandId], N'LocustCommander' AS [Discriminator]
        FROM [LocustCommanders] AS [l2]
    ) AS [t0]
    WHERE [t0].[ThreatLevelNullableByte] = [t].[ThreatLevelNullableByte] OR ([t0].[ThreatLevelNullableByte] IS NULL AND [t].[ThreatLevelNullableByte] IS NULL))");
    }

    public override async Task Contains_on_collection_of_nullable_byte_subquery_null_constant(bool async)
    {
        await base.Contains_on_collection_of_nullable_byte_subquery_null_constant(async);

        AssertSql(
            @"SELECT [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [l1].[Name], [l1].[LocustHordeId], [l1].[ThreatLevel], [l1].[ThreatLevelByte], [l1].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
        FROM [LocustLeaders] AS [l1]
        UNION ALL
        SELECT [l2].[Name], [l2].[LocustHordeId], [l2].[ThreatLevel], [l2].[ThreatLevelByte], [l2].[ThreatLevelNullableByte], [l2].[DefeatedByNickname], [l2].[DefeatedBySquadId], [l2].[HighCommandId], N'LocustCommander' AS [Discriminator]
        FROM [LocustCommanders] AS [l2]
    ) AS [t0]
    WHERE [t0].[ThreatLevelNullableByte] IS NULL)");
    }

    public override async Task Contains_on_collection_of_nullable_byte_subquery_null_parameter(bool async)
    {
        await base.Contains_on_collection_of_nullable_byte_subquery_null_parameter(async);

        AssertSql(
            @"SELECT [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [l1].[Name], [l1].[LocustHordeId], [l1].[ThreatLevel], [l1].[ThreatLevelByte], [l1].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
        FROM [LocustLeaders] AS [l1]
        UNION ALL
        SELECT [l2].[Name], [l2].[LocustHordeId], [l2].[ThreatLevel], [l2].[ThreatLevelByte], [l2].[ThreatLevelNullableByte], [l2].[DefeatedByNickname], [l2].[DefeatedBySquadId], [l2].[HighCommandId], N'LocustCommander' AS [Discriminator]
        FROM [LocustCommanders] AS [l2]
    ) AS [t0]
    WHERE [t0].[ThreatLevelNullableByte] IS NULL)");
    }

    public override async Task Contains_on_byte_array_property_using_byte_column(bool async)
    {
        await base.Contains_on_byte_array_property_using_byte_column(async);

        AssertSql(
            @"SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator]
FROM [Squads] AS [s]
CROSS JOIN (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
WHERE CHARINDEX(CAST([t].[ThreatLevelByte] AS varbinary(max)), [s].[Banner]) > 0");
    }

    public override async Task Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion(
        bool async)
    {
        await base.Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion(async);

        AssertSql(
            @"SELECT [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
CROSS APPLY (
    SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t0]
    WHERE EXISTS (
        SELECT 1
        FROM (
            SELECT [l1].[Name], [l1].[LocustHordeId], [l1].[ThreatLevel], [l1].[ThreatLevelByte], [l1].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
            FROM [LocustLeaders] AS [l1]
            UNION ALL
            SELECT [l2].[Name], [l2].[LocustHordeId], [l2].[ThreatLevel], [l2].[ThreatLevelByte], [l2].[ThreatLevelNullableByte], [l2].[DefeatedByNickname], [l2].[DefeatedBySquadId], [l2].[HighCommandId], N'LocustCommander' AS [Discriminator]
            FROM [LocustCommanders] AS [l2]
        ) AS [t2]
        WHERE [t2].[ThreatLevelByte] = [t].[ThreatLevelByte])
) AS [t1]");
    }

    public override async Task Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion_negated(
        bool async)
    {
        await base.Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion_negated(async);

        AssertSql(
            @"SELECT [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
CROSS APPLY (
    SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t0]
    WHERE NOT (EXISTS (
        SELECT 1
        FROM (
            SELECT [l1].[Name], [l1].[LocustHordeId], [l1].[ThreatLevel], [l1].[ThreatLevelByte], [l1].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
            FROM [LocustLeaders] AS [l1]
            UNION ALL
            SELECT [l2].[Name], [l2].[LocustHordeId], [l2].[ThreatLevel], [l2].[ThreatLevelByte], [l2].[ThreatLevelNullableByte], [l2].[DefeatedByNickname], [l2].[DefeatedBySquadId], [l2].[HighCommandId], N'LocustCommander' AS [Discriminator]
            FROM [LocustCommanders] AS [l2]
        ) AS [t2]
        WHERE [t2].[ThreatLevelByte] = [t].[ThreatLevelByte]))
) AS [t1]");
    }

    public override async Task Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion(bool async)
    {
        await base.Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion(async);

        AssertSql(
            @"SELECT [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
CROSS APPLY (
    SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t0]
    WHERE EXISTS (
        SELECT 1
        FROM (
            SELECT [l1].[Name], [l1].[LocustHordeId], [l1].[ThreatLevel], [l1].[ThreatLevelByte], [l1].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
            FROM [LocustLeaders] AS [l1]
            UNION ALL
            SELECT [l2].[Name], [l2].[LocustHordeId], [l2].[ThreatLevel], [l2].[ThreatLevelByte], [l2].[ThreatLevelNullableByte], [l2].[DefeatedByNickname], [l2].[DefeatedBySquadId], [l2].[HighCommandId], N'LocustCommander' AS [Discriminator]
            FROM [LocustCommanders] AS [l2]
        ) AS [t2]
        WHERE [t2].[ThreatLevelNullableByte] = [t].[ThreatLevelNullableByte] OR ([t2].[ThreatLevelNullableByte] IS NULL AND [t].[ThreatLevelNullableByte] IS NULL))
) AS [t1]");
    }

    public override async Task Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion_negated(bool async)
    {
        await base.Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion_negated(async);

        AssertSql(
            @"SELECT [t1].[Nickname], [t1].[SquadId], [t1].[AssignedCityName], [t1].[CityOfBirthName], [t1].[FullName], [t1].[HasSoulPatch], [t1].[LeaderNickname], [t1].[LeaderSquadId], [t1].[Rank], [t1].[Discriminator]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
CROSS APPLY (
    SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t0]
    WHERE NOT (EXISTS (
        SELECT 1
        FROM (
            SELECT [l1].[Name], [l1].[LocustHordeId], [l1].[ThreatLevel], [l1].[ThreatLevelByte], [l1].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
            FROM [LocustLeaders] AS [l1]
            UNION ALL
            SELECT [l2].[Name], [l2].[LocustHordeId], [l2].[ThreatLevel], [l2].[ThreatLevelByte], [l2].[ThreatLevelNullableByte], [l2].[DefeatedByNickname], [l2].[DefeatedBySquadId], [l2].[HighCommandId], N'LocustCommander' AS [Discriminator]
            FROM [LocustCommanders] AS [l2]
        ) AS [t2]
        WHERE [t2].[ThreatLevelNullableByte] = [t].[ThreatLevelNullableByte] OR ([t2].[ThreatLevelNullableByte] IS NULL AND [t].[ThreatLevelNullableByte] IS NULL)))
) AS [t1]");
    }

    public override async Task Enum_closure_typed_as_underlying_type_generates_correct_parameter_type(bool async)
    {
        await base.Enum_closure_typed_as_underlying_type_generates_correct_parameter_type(async);

        AssertSql(
            @"@__prm_0='1' (Nullable = true)

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE @__prm_0 = [w].[AmmunitionType]");
    }

    public override async Task Enum_flags_closure_typed_as_underlying_type_generates_correct_parameter_type(bool async)
    {
        await base.Enum_flags_closure_typed_as_underlying_type_generates_correct_parameter_type(async);

        AssertSql(
            @"@__prm_0='133'

SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE (@__prm_0 & [t].[Rank]) = [t].[Rank]");
    }

    public override async Task Enum_flags_closure_typed_as_different_type_generates_correct_parameter_type(bool async)
    {
        await base.Enum_flags_closure_typed_as_different_type_generates_correct_parameter_type(async);

        AssertSql(
            @"@__prm_0='5'

SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE (@__prm_0 & CAST([t].[Rank] AS int)) = CAST([t].[Rank] AS int)");
    }

    public override async Task Constant_enum_with_same_underlying_value_as_previously_parameterized_int(bool async)
    {
        await base.Constant_enum_with_same_underlying_value_as_previously_parameterized_int(async);

        AssertSql(
            @"@__p_0='1'

SELECT TOP(@__p_0) [t].[Rank] & 1
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
ORDER BY [t].[Nickname]");
    }

    public override async Task Enum_array_contains(bool async)
    {
        await base.Enum_array_contains(async);

        AssertSql(
            @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
WHERE [w0].[Id] IS NOT NULL AND ([w0].[AmmunitionType] = 1 OR [w0].[AmmunitionType] IS NULL)");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task DataLength_function_for_string_parameter(bool async)
    {
        await AssertQueryScalar(
            async,
            ss => ss.Set<Mission>().Select(m => EF.Functions.DataLength(m.CodeName)),
            ss => ss.Set<Mission>().Select(m => (int?)(m.CodeName.Length * 2)));

        AssertSql(
            @"SELECT CAST(DATALENGTH([m].[CodeName]) AS int)
FROM [Missions] AS [m]");
    }

    public override async Task CompareTo_used_with_non_unicode_string_column_and_constant(bool async)
    {
        await base.CompareTo_used_with_non_unicode_string_column_and_constant(async);

        AssertSql(
            @"SELECT [c].[Name], [c].[Location], [c].[Nation]
FROM [Cities] AS [c]
WHERE [c].[Location] = 'Unknown'");
    }

    public override async Task Coalesce_used_with_non_unicode_string_column_and_constant(bool async)
    {
        await base.Coalesce_used_with_non_unicode_string_column_and_constant(async);

        AssertSql(
            @"SELECT COALESCE([c].[Location], 'Unknown')
FROM [Cities] AS [c]");
    }

    public override async Task Groupby_anonymous_type_with_navigations_followed_up_by_anonymous_projection_and_orderby(bool async)
    {
        await base.Groupby_anonymous_type_with_navigations_followed_up_by_anonymous_projection_and_orderby(async);

        AssertSql(
            @"SELECT [c].[Name], [c].[Location], COUNT(*) AS [Count]
FROM [Weapons] AS [w]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t] ON [w].[OwnerFullName] = [t].[FullName]
LEFT JOIN [Cities] AS [c] ON [t].[CityOfBirthName] = [c].[Name]
GROUP BY [c].[Name], [c].[Location]
ORDER BY [c].[Location]");
    }

    public override async Task SelectMany_predicate_with_non_equality_comparison_converted_to_inner_join(bool async)
    {
        await base.SelectMany_predicate_with_non_equality_comparison_converted_to_inner_join(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN [Weapons] AS [w] ON [t].[FullName] <> [w].[OwnerFullName] OR [w].[OwnerFullName] IS NULL
ORDER BY [t].[Nickname], [w].[Id]");
    }

    public override async Task SelectMany_predicate_with_non_equality_comparison_DefaultIfEmpty_converted_to_left_join(bool async)
    {
        await base.SelectMany_predicate_with_non_equality_comparison_DefaultIfEmpty_converted_to_left_join(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Weapons] AS [w] ON [t].[FullName] <> [w].[OwnerFullName] OR [w].[OwnerFullName] IS NULL
ORDER BY [t].[Nickname], [w].[Id]");
    }

    public override async Task SelectMany_predicate_after_navigation_with_non_equality_comparison_DefaultIfEmpty_converted_to_left_join(
        bool async)
    {
        await base.SelectMany_predicate_after_navigation_with_non_equality_comparison_DefaultIfEmpty_converted_to_left_join(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId]
    FROM [Weapons] AS [w]
    LEFT JOIN [Weapons] AS [w0] ON [w].[SynergyWithId] = [w0].[Id]
) AS [t0] ON [t].[FullName] <> [t0].[OwnerFullName] OR [t0].[OwnerFullName] IS NULL
ORDER BY [t].[Nickname], [t0].[Id]");
    }

    public override async Task SelectMany_without_result_selector_and_non_equality_comparison_converted_to_join(bool async)
    {
        await base.SelectMany_without_result_selector_and_non_equality_comparison_converted_to_join(async);

        AssertSql(
            @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Weapons] AS [w] ON [t].[FullName] <> [w].[OwnerFullName] OR [w].[OwnerFullName] IS NULL");
    }

    public override async Task Filtered_collection_projection_with_order_comparison_predicate_converted_to_join(bool async)
    {
        await base.Filtered_collection_projection_with_order_comparison_predicate_converted_to_join(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Weapons] AS [w] ON [t].[FullName] = [w].[OwnerFullName] AND [t].[SquadId] < [w].[Id]
ORDER BY [t].[Nickname], [t].[SquadId]");
    }

    public override async Task Filtered_collection_projection_with_order_comparison_predicate_converted_to_join2(bool async)
    {
        await base.Filtered_collection_projection_with_order_comparison_predicate_converted_to_join2(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Weapons] AS [w] ON [t].[FullName] = [w].[OwnerFullName] AND [t].[SquadId] <= [w].[Id]
ORDER BY [t].[Nickname], [t].[SquadId]");
    }

    public override async Task Filtered_collection_projection_with_order_comparison_predicate_converted_to_join3(bool async)
    {
        await base.Filtered_collection_projection_with_order_comparison_predicate_converted_to_join3(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Weapons] AS [w] ON [t].[FullName] = [w].[OwnerFullName] AND [t].[SquadId] >= [w].[Id]
ORDER BY [t].[Nickname], [t].[SquadId]");
    }

    public override async Task SelectMany_predicate_with_non_equality_comparison_with_Take_doesnt_convert_to_join(bool async)
    {
        await base.SelectMany_predicate_with_non_equality_comparison_with_Take_doesnt_convert_to_join(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
CROSS APPLY (
    SELECT TOP(3) [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
    FROM [Weapons] AS [w]
    WHERE [w].[OwnerFullName] <> [t].[FullName] OR [w].[OwnerFullName] IS NULL
    ORDER BY [w].[Id]
) AS [t0]
ORDER BY [t].[Nickname], [t0].[Id]");
    }

    public override async Task FirstOrDefault_over_int_compared_to_zero(bool async)
    {
        await base.FirstOrDefault_over_int_compared_to_zero(async);

        AssertSql(
            @"SELECT [s].[Name]
FROM [Squads] AS [s]
WHERE [s].[Name] = N'Kilo' AND COALESCE((
    SELECT TOP(1) [t].[SquadId]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    WHERE [s].[Id] = [t].[SquadId] AND [t].[HasSoulPatch] = CAST(1 AS bit)), 0) <> 0");
    }

    public override async Task Correlated_collection_with_inner_collection_references_element_two_levels_up(bool async)
    {
        await base.Correlated_collection_with_inner_collection_references_element_two_levels_up(async);

        AssertSql(
            @"SELECT [t].[FullName], [t].[Nickname], [t].[SquadId], [t0].[ReportName], [t0].[OfficerName], [t0].[Nickname], [t0].[SquadId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
OUTER APPLY (
    SELECT [t1].[FullName] AS [ReportName], [t].[FullName] AS [OfficerName], [t1].[Nickname], [t1].[SquadId]
    FROM (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t1]
    WHERE [t].[Nickname] = [t1].[LeaderNickname] AND [t].[SquadId] = [t1].[LeaderSquadId]
) AS [t0]
WHERE [t].[Discriminator] = N'Officer'
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Nickname]");
    }

    public override async Task Accessing_derived_property_using_hard_and_soft_cast(bool async)
    {
        await base.Accessing_derived_property_using_hard_and_soft_cast(async);

        AssertSql(
            @"SELECT [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
WHERE [t].[Discriminator] = N'LocustCommander' AND ([t].[HighCommandId] <> 0 OR [t].[HighCommandId] IS NULL)",
            //
            @"SELECT [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
WHERE [t].[Discriminator] = N'LocustCommander' AND ([t].[HighCommandId] <> 0 OR [t].[HighCommandId] IS NULL)");
    }

    public override async Task Cast_to_derived_followed_by_include_and_FirstOrDefault(bool async)
    {
        await base.Cast_to_derived_followed_by_include_and_FirstOrDefault(async);

        AssertSql(
            @"SELECT TOP(1) [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[DefeatedByNickname] = [t0].[Nickname] AND [t].[DefeatedBySquadId] = [t0].[SquadId]
WHERE [t].[Name] LIKE N'%Queen%'");
    }

    public override async Task Correlated_collection_take(bool async)
    {
        await base.Correlated_collection_take(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [c].[Name], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[SynergyWithId], [c].[Location], [c].[Nation]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN [Cities] AS [c] ON [t].[CityOfBirthName] = [c].[Name]
LEFT JOIN (
    SELECT [t1].[Id], [t1].[AmmunitionType], [t1].[IsAutomatic], [t1].[Name], [t1].[OwnerFullName], [t1].[SynergyWithId]
    FROM (
        SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], ROW_NUMBER() OVER(PARTITION BY [w].[OwnerFullName] ORDER BY [w].[Id]) AS [row]
        FROM [Weapons] AS [w]
    ) AS [t1]
    WHERE [t1].[row] <= 10
) AS [t0] ON [t].[FullName] = [t0].[OwnerFullName]
ORDER BY [t].[Nickname], [t].[SquadId], [c].[Name]");
    }

    public override async Task First_on_byte_array(bool async)
    {
        await base.First_on_byte_array(async);

        AssertSql(
            @"SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE CAST(SUBSTRING([s].[Banner], 1, 1) AS tinyint) = CAST(2 AS tinyint)");
    }

    public override async Task Array_access_on_byte_array(bool async)
    {
        await base.Array_access_on_byte_array(async);

        AssertSql(
            @"SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE CAST(SUBSTRING([s].[Banner5], 2 + 1, 1) AS tinyint) = CAST(6 AS tinyint)");
    }

    public override async Task Project_shadow_properties(bool async)
    {
        await base.Project_shadow_properties(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[AssignedCityName]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task Composite_key_entity_equal(bool async)
    {
        await base.Composite_key_entity_equal(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
CROSS JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0]
WHERE [t].[Nickname] = [t0].[Nickname] AND [t].[SquadId] = [t0].[SquadId]");
    }

    public override async Task Composite_key_entity_not_equal(bool async)
    {
        await base.Composite_key_entity_not_equal(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
CROSS JOIN (
    SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g0]
    UNION ALL
    SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o0]
) AS [t0]
WHERE [t].[Nickname] <> [t0].[Nickname] OR [t].[SquadId] <> [t0].[SquadId]");
    }

    public override async Task Composite_key_entity_equal_null(bool async)
    {
        await base.Composite_key_entity_equal_null(async);

        AssertSql(
            @"SELECT [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[DefeatedByNickname] = [t0].[Nickname] AND [t].[DefeatedBySquadId] = [t0].[SquadId]
WHERE [t].[Discriminator] = N'LocustCommander' AND ([t0].[Nickname] IS NULL OR [t0].[SquadId] IS NULL)");
    }

    public override async Task Composite_key_entity_not_equal_null(bool async)
    {
        await base.Composite_key_entity_not_equal_null(async);

        AssertSql(
            @"SELECT [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[DefeatedByNickname] = [t0].[Nickname] AND [t].[DefeatedBySquadId] = [t0].[SquadId]
WHERE [t].[Discriminator] = N'LocustCommander' AND [t0].[Nickname] IS NOT NULL AND [t0].[SquadId] IS NOT NULL");
    }

    public override async Task Projecting_property_converted_to_nullable_with_comparison(bool async)
    {
        await base.Projecting_property_converted_to_nullable_with_comparison(async);

        AssertSql(
            @"SELECT [t].[Note], CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [t0].[Nickname], [t0].[SquadId], [t0].[HasSoulPatch]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [t0].[SquadId]
    ELSE NULL
END = 1");
    }

    public override async Task Projecting_property_converted_to_nullable_with_addition(bool async)
    {
        await base.Projecting_property_converted_to_nullable_with_addition(async);

        AssertSql(
            @"SELECT [t].[Note], CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [t0].[Nickname], [t0].[SquadId], [t0].[HasSoulPatch]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE (CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [t0].[SquadId]
    ELSE NULL
END + 1) = 2");
    }

    public override async Task Projecting_property_converted_to_nullable_with_addition_and_final_projection(bool async)
    {
        await base.Projecting_property_converted_to_nullable_with_addition_and_final_projection(async);

        AssertSql(
            @"SELECT [t].[Note], CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [t0].[SquadId]
    ELSE NULL
END + 1 AS [Value]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [t0].[Nickname]
    ELSE NULL
END IS NOT NULL");
    }

    public override async Task Projecting_property_converted_to_nullable_with_conditional(bool async)
    {
        await base.Projecting_property_converted_to_nullable_with_conditional(async);

        AssertSql(
            @"SELECT CASE
    WHEN [t].[Note] <> N'K.I.A.' OR [t].[Note] IS NULL THEN CASE
        WHEN [t].[GearNickName] IS NOT NULL THEN [t0].[SquadId]
        ELSE NULL
    END
    ELSE -1
END
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]");
    }

    public override async Task Projecting_property_converted_to_nullable_with_function_call(bool async)
    {
        await base.Projecting_property_converted_to_nullable_with_function_call(async);

        AssertSql(
            @"SELECT SUBSTRING(CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [t0].[Nickname]
    ELSE NULL
END, 0 + 1, 3)
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]");
    }

    public override async Task Projecting_property_converted_to_nullable_with_function_call2(bool async)
    {
        await base.Projecting_property_converted_to_nullable_with_function_call2(async);

        AssertSql(
            @"SELECT [t].[Note], SUBSTRING([t].[Note], 0 + 1, CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [t0].[SquadId]
    ELSE NULL
END) AS [Function]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [t0].[Nickname]
    ELSE NULL
END IS NOT NULL");
    }

    public override async Task Projecting_property_converted_to_nullable_into_element_init(bool async)
    {
        await base.Projecting_property_converted_to_nullable_into_element_init(async);

        AssertSql(
            @"SELECT CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN CAST(LEN([t0].[Nickname]) AS int)
    ELSE NULL
END, CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [t0].[SquadId]
    ELSE NULL
END, CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [t0].[SquadId]
    ELSE NULL
END + 1
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [t0].[Nickname]
    ELSE NULL
END IS NOT NULL
ORDER BY [t].[Note]");
    }

    public override async Task Projecting_property_converted_to_nullable_into_member_assignment(bool async)
    {
        await base.Projecting_property_converted_to_nullable_into_member_assignment(async);

        AssertSql(
            @"SELECT CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [t0].[SquadId]
    ELSE NULL
END AS [Id]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [t0].[Nickname]
    ELSE NULL
END IS NOT NULL
ORDER BY [t].[Note]");
    }

    public override async Task Projecting_property_converted_to_nullable_into_new_array(bool async)
    {
        await base.Projecting_property_converted_to_nullable_into_new_array(async);

        AssertSql(
            @"SELECT CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN CAST(LEN([t0].[Nickname]) AS int)
    ELSE NULL
END, CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [t0].[SquadId]
    ELSE NULL
END, CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [t0].[SquadId]
    ELSE NULL
END + 1
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [t0].[Nickname]
    ELSE NULL
END IS NOT NULL
ORDER BY [t].[Note]");
    }

    public override async Task Projecting_property_converted_to_nullable_into_unary(bool async)
    {
        await base.Projecting_property_converted_to_nullable_into_unary(async);

        AssertSql(
            @"SELECT [t].[Note]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [t0].[Nickname]
    ELSE NULL
END IS NOT NULL AND CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [t0].[HasSoulPatch]
    ELSE NULL
END = CAST(0 AS bit)
ORDER BY [t].[Note]");
    }

    public override async Task Projecting_property_converted_to_nullable_into_member_access(bool async)
    {
        await base.Projecting_property_converted_to_nullable_into_member_access(async);

        AssertSql(
            @"SELECT [t].[Nickname]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName] AND [t].[SquadId] = [t0].[GearSquadId]
WHERE DATEPART(month, [t0].[IssueDate]) <> 5 OR [t0].[IssueDate] IS NULL
ORDER BY [t].[Nickname]");
    }

    public override async Task Projecting_property_converted_to_nullable_and_use_it_in_order_by(bool async)
    {
        await base.Projecting_property_converted_to_nullable_and_use_it_in_order_by(async);

        AssertSql(
            @"SELECT [t].[Note], CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [t0].[Nickname], [t0].[SquadId], [t0].[HasSoulPatch]
FROM [Tags] AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[GearNickName] = [t0].[Nickname] AND [t].[GearSquadId] = [t0].[SquadId]
WHERE CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [t0].[Nickname]
    ELSE NULL
END IS NOT NULL
ORDER BY CASE
    WHEN [t].[GearNickName] IS NOT NULL THEN [t0].[SquadId]
    ELSE NULL
END, [t].[Note]");
    }

    public override async Task Where_DateOnly_Year(bool async)
    {
        // DateOnly and TimeOnly. Issue #24507.
        await AssertTranslationFailed(() => base.Where_DateOnly_Year(async));

        AssertSql();
    }

    public override async Task Where_DateOnly_Month(bool async)
    {
        // DateOnly and TimeOnly. Issue #24507.
        await AssertTranslationFailed(() => base.Where_DateOnly_Month(async));

        AssertSql();
    }

    public override async Task Where_DateOnly_Day(bool async)
    {
        // DateOnly and TimeOnly. Issue #24507.
        await AssertTranslationFailed(() => base.Where_DateOnly_Day(async));

        AssertSql();
    }

    public override async Task Where_DateOnly_DayOfYear(bool async)
    {
        // DateOnly and TimeOnly. Issue #24507.
        await AssertTranslationFailed(() => base.Where_DateOnly_DayOfYear(async));

        AssertSql();
    }

    public override async Task Where_DateOnly_DayOfWeek(bool async)
    {
        // DateOnly and TimeOnly. Issue #24507.
        await AssertTranslationFailed(() => base.Where_DateOnly_DayOfWeek(async));

        AssertSql();
    }

    public override async Task Where_DateOnly_AddYears(bool async)
    {
        // DateOnly and TimeOnly. Issue #24507.
        await AssertTranslationFailed(() => base.Where_DateOnly_AddYears(async));

        AssertSql();
    }

    public override async Task Where_DateOnly_AddMonths(bool async)
    {
        // DateOnly and TimeOnly. Issue #24507.
        await AssertTranslationFailed(() => base.Where_DateOnly_AddMonths(async));

        AssertSql();
    }

    public override async Task Where_DateOnly_AddDays(bool async)
    {
        // DateOnly and TimeOnly. Issue #24507.
        await AssertTranslationFailed(() => base.Where_DateOnly_AddDays(async));

        AssertSql();
    }

    public override async Task Where_TimeOnly_Hour(bool async)
    {
        // DateOnly and TimeOnly. Issue #24507.
        await AssertTranslationFailed(() => base.Where_TimeOnly_Hour(async));

        AssertSql();
    }

    public override async Task Where_TimeOnly_Minute(bool async)
    {
        // DateOnly and TimeOnly. Issue #24507.
        await AssertTranslationFailed(() => base.Where_TimeOnly_Minute(async));

        AssertSql();
    }

    public override async Task Where_TimeOnly_Second(bool async)
    {
        // DateOnly and TimeOnly. Issue #24507.
        await AssertTranslationFailed(() => base.Where_TimeOnly_Second(async));

        AssertSql();
    }

    public override async Task Where_TimeOnly_Millisecond(bool async)
    {
        // DateOnly and TimeOnly. Issue #24507.
        await AssertTranslationFailed(() => base.Where_TimeOnly_Millisecond(async));

        AssertSql();
    }

    public override async Task Where_TimeOnly_AddHours(bool async)
    {
        // DateOnly and TimeOnly. Issue #24507.
        await AssertTranslationFailed(() => base.Where_TimeOnly_AddHours(async));

        AssertSql();
    }

    public override async Task Where_TimeOnly_AddMinutes(bool async)
    {
        // DateOnly and TimeOnly. Issue #24507.
        await AssertTranslationFailed(() => base.Where_TimeOnly_AddMinutes(async));

        AssertSql();
    }

    public override async Task Where_TimeOnly_Add_TimeSpan(bool async)
    {
        // DateOnly and TimeOnly. Issue #24507.
        await AssertTranslationFailed(() => base.Where_TimeOnly_Add_TimeSpan(async));

        AssertSql();
    }

    public override async Task Where_TimeOnly_IsBetween(bool async)
    {
        // DateOnly and TimeOnly. Issue #24507.
        await AssertTranslationFailed(() => base.Where_TimeOnly_IsBetween(async));

        AssertSql();
    }

    public override async Task Where_TimeOnly_subtract_TimeOnly(bool async)
    {
        // DateOnly and TimeOnly. Issue #24507.
        await AssertTranslationFailed(() => base.Where_TimeOnly_subtract_TimeOnly(async));

        AssertSql();
    }

    public override async Task Project_navigation_defined_on_base_from_entity_with_inheritance_using_soft_cast(bool async)
    {
        await base.Project_navigation_defined_on_base_from_entity_with_inheritance_using_soft_cast(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[Id], [t0].[GearNickName], [t0].[GearSquadId], [t0].[IssueDate], [t0].[Note], CASE
    WHEN [t0].[Id] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsNull], [c].[Name], [c].[Location], [c].[Nation], CASE
    WHEN [c].[Name] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsNull], [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], CASE
    WHEN [s].[Id] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsNull]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName] AND [t].[SquadId] = [t0].[GearSquadId]
LEFT JOIN [Cities] AS [c] ON [t].[CityOfBirthName] = [c].[Name]
LEFT JOIN [Squads] AS [s] ON [t].[SquadId] = [s].[Id]");
    }

    public override async Task Project_navigation_defined_on_derived_from_entity_with_inheritance_using_soft_cast(bool async)
    {
        await base.Project_navigation_defined_on_derived_from_entity_with_inheritance_using_soft_cast(async);

        AssertSql(
            @"SELECT [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator], CASE
    WHEN [t0].[Nickname] IS NULL OR [t0].[SquadId] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsNull], [l1].[Id], [l1].[CapitalName], [l1].[Name], [l1].[ServerAddress], [l1].[CommanderName], [l1].[Eradicated], CASE
    WHEN [l1].[Id] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsNull], [l2].[Id], [l2].[IsOperational], [l2].[Name], CASE
    WHEN [l2].[Id] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsNull]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[DefeatedByNickname] = [t0].[Nickname] AND [t].[DefeatedBySquadId] = [t0].[SquadId]
LEFT JOIN [LocustHordes] AS [l1] ON [t].[Name] = [l1].[CommanderName]
LEFT JOIN [LocustHighCommands] AS [l2] ON [t].[HighCommandId] = [l2].[Id]");
    }

    public override async Task Join_entity_with_itself_grouped_by_key_followed_by_include_skip_take(bool async)
    {
        await base.Join_entity_with_itself_grouped_by_key_followed_by_include_skip_take(async);

        AssertSql(
            @"@__p_0='0'
@__p_1='10'

SELECT [t2].[Nickname], [t2].[SquadId], [t2].[AssignedCityName], [t2].[CityOfBirthName], [t2].[FullName], [t2].[HasSoulPatch], [t2].[LeaderNickname], [t2].[LeaderSquadId], [t2].[Rank], [t2].[Discriminator], [t2].[HasSoulPatch0], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM (
    SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [t0].[HasSoulPatch] AS [HasSoulPatch0]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    INNER JOIN (
        SELECT MIN(CAST(LEN([t1].[Nickname]) AS int)) AS [c], [t1].[HasSoulPatch]
        FROM (
            SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
            FROM [Gears] AS [g0]
            UNION ALL
            SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
            FROM [Officers] AS [o0]
        ) AS [t1]
        WHERE [t1].[Nickname] <> N'Dom'
        GROUP BY [t1].[HasSoulPatch]
    ) AS [t0] ON CAST(LEN([t].[Nickname]) AS int) = [t0].[c]
    ORDER BY [t].[Nickname]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t2]
LEFT JOIN [Weapons] AS [w] ON [t2].[FullName] = [w].[OwnerFullName]
ORDER BY [t2].[Nickname], [t2].[SquadId], [t2].[HasSoulPatch0]");
    }

    public override async Task Where_bool_column_and_Contains(bool async)
    {
        await base.Where_bool_column_and_Contains(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit) AND [t].[HasSoulPatch] IN (CAST(0 AS bit), CAST(1 AS bit))");
    }

    public override async Task Where_bool_column_or_Contains(bool async)
    {
        await base.Where_bool_column_or_Contains(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE [t].[HasSoulPatch] = CAST(1 AS bit) AND [t].[HasSoulPatch] IN (CAST(0 AS bit), CAST(1 AS bit))");
    }

    public override async Task Parameter_used_multiple_times_take_appropriate_inferred_type_mapping(bool async)
    {
        await base.Parameter_used_multiple_times_take_appropriate_inferred_type_mapping(async);

        AssertSql(
            @"@__place_0='Seattle' (Size = 4000)
@__place_0_1='Seattle' (Size = 100) (DbType = AnsiString)

SELECT [c].[Name], [c].[Location], [c].[Nation]
FROM [Cities] AS [c]
WHERE [c].[Nation] = @__place_0 OR [c].[Location] = @__place_0_1");
    }

    public override async Task Enum_matching_take_value_gets_different_type_mapping(bool async)
    {
        await base.Enum_matching_take_value_gets_different_type_mapping(async);

        AssertSql(
            @"@__p_0='1'
@__value_1='1'

SELECT TOP(@__p_0) [t].[Rank] & @__value_1
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
ORDER BY [t].[Nickname]");
    }

    public override async Task SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector_order_comparison(bool async)
    {
        await base.SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector_order_comparison(async);

        AssertSql(
            @"@__prm_0='1'

SELECT [t].[Nickname], [t].[FullName], CASE
    WHEN [t0].[Id] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Collection]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [w].[Id], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
    WHERE [w].[Id] > @__prm_0
) AS [t0] ON [t].[FullName] = [t0].[OwnerFullName]");
    }

    public override async Task Project_entity_and_collection_element(bool async)
    {
        await base.Project_entity_and_collection_element(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [t0].[Id], [t0].[AmmunitionType], [t0].[IsAutomatic], [t0].[Name], [t0].[OwnerFullName], [t0].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
INNER JOIN [Squads] AS [s] ON [t].[SquadId] = [s].[Id]
LEFT JOIN [Weapons] AS [w] ON [t].[FullName] = [w].[OwnerFullName]
LEFT JOIN (
    SELECT [t1].[Id], [t1].[AmmunitionType], [t1].[IsAutomatic], [t1].[Name], [t1].[OwnerFullName], [t1].[SynergyWithId]
    FROM (
        SELECT [w0].[Id], [w0].[AmmunitionType], [w0].[IsAutomatic], [w0].[Name], [w0].[OwnerFullName], [w0].[SynergyWithId], ROW_NUMBER() OVER(PARTITION BY [w0].[OwnerFullName] ORDER BY [w0].[Id]) AS [row]
        FROM [Weapons] AS [w0]
    ) AS [t1]
    WHERE [t1].[row] <= 1
) AS [t0] ON [t].[FullName] = [t0].[OwnerFullName]
ORDER BY [t].[Nickname], [t].[SquadId], [s].[Id]");
    }

    public override async Task DateTimeOffset_DateAdd_AddYears(bool async)
    {
        await base.DateTimeOffset_DateAdd_AddYears(async);

        AssertSql(
            @"SELECT DATEADD(year, CAST(1 AS int), [m].[Timeline])
FROM [Missions] AS [m]");
    }

    public override async Task Correlated_collection_via_SelectMany_with_Distinct_missing_indentifying_columns_in_projection(bool async)
    {
        await base.Correlated_collection_via_SelectMany_with_Distinct_missing_indentifying_columns_in_projection(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t1].[HasSoulPatch]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
OUTER APPLY (
    SELECT DISTINCT [t2].[HasSoulPatch]
    FROM [Weapons] AS [w]
    LEFT JOIN (
        SELECT [g0].[Nickname], [g0].[SquadId], [g0].[AssignedCityName], [g0].[CityOfBirthName], [g0].[FullName], [g0].[HasSoulPatch], [g0].[LeaderNickname], [g0].[LeaderSquadId], [g0].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g0]
        UNION ALL
        SELECT [o0].[Nickname], [o0].[SquadId], [o0].[AssignedCityName], [o0].[CityOfBirthName], [o0].[FullName], [o0].[HasSoulPatch], [o0].[LeaderNickname], [o0].[LeaderSquadId], [o0].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o0]
    ) AS [t0] ON [w].[OwnerFullName] = [t0].[FullName]
    LEFT JOIN [Cities] AS [c] ON [t0].[AssignedCityName] = [c].[Name]
    INNER JOIN (
        SELECT [g1].[Nickname], [g1].[SquadId], [g1].[AssignedCityName], [g1].[CityOfBirthName], [g1].[FullName], [g1].[HasSoulPatch], [g1].[LeaderNickname], [g1].[LeaderSquadId], [g1].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g1]
        UNION ALL
        SELECT [o1].[Nickname], [o1].[SquadId], [o1].[AssignedCityName], [o1].[CityOfBirthName], [o1].[FullName], [o1].[HasSoulPatch], [o1].[LeaderNickname], [o1].[LeaderSquadId], [o1].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o1]
    ) AS [t2] ON [c].[Name] = [t2].[CityOfBirthName]
    WHERE [t].[FullName] = [w].[OwnerFullName]
) AS [t1]
ORDER BY [t].[Nickname], [t].[SquadId]");
    }

    public override async Task Basic_query_gears(bool async)
    {
        await base.Basic_query_gears(async);

        AssertSql(
            @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
FROM [Gears] AS [g]
UNION ALL
SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
FROM [Officers] AS [o]");
    }

    public override async Task Contains_on_readonly_enumerable(bool async)
    {
        await base.Contains_on_readonly_enumerable(async);

        AssertSql(
            @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapons] AS [w]
WHERE [w].[AmmunitionType] = 1");
    }

    public override async Task SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector_not_equal(bool async)
    {
        await base.SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector_not_equal(async);

        AssertSql(
            @"@__isAutomatic_0='True'

SELECT [t].[Nickname], [t].[FullName], CASE
    WHEN [t0].[Id] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Collection]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT [w].[Id], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
    WHERE [w].[IsAutomatic] <> @__isAutomatic_0
) AS [t0] ON [t].[FullName] = [t0].[OwnerFullName]");
    }

    public override async Task Trying_to_access_unmapped_property_in_projection(bool async)
    {
        await base.Trying_to_access_unmapped_property_in_projection(async);

        AssertSql(
            @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
FROM [Gears] AS [g]
UNION ALL
SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
FROM [Officers] AS [o]");
    }

    public override async Task Cast_to_derived_type_causes_client_eval(bool async)
    {
        await base.Cast_to_derived_type_causes_client_eval(async);

        AssertSql(
            @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
FROM [Gears] AS [g]
UNION ALL
SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
FROM [Officers] AS [o]");
    }

    public override async Task Comparison_with_value_converted_subclass(bool async)
    {
        await base.Comparison_with_value_converted_subclass(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[CapitalName], [l].[Name], [l].[ServerAddress], [l].[CommanderName], [l].[Eradicated]
FROM [LocustHordes] AS [l]
WHERE [l].[ServerAddress] = CAST(N'127.0.0.1' AS nvarchar(45))");
    }

    public override async Task FirstOrDefault_on_empty_collection_of_DateTime_in_subquery(bool async)
    {
        await base.FirstOrDefault_on_empty_collection_of_DateTime_in_subquery(async);

        AssertSql(
            @"SELECT [t].[Nickname], COALESCE((
    SELECT TOP(1) [t2].[IssueDate]
    FROM [Tags] AS [t2]
    WHERE [t2].[GearNickName] = [t].[FullName]
    ORDER BY [t2].[Id]), '0001-01-01T00:00:00.0000000') AS [invalidTagIssueDate]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN [Tags] AS [t0] ON [t].[Nickname] = [t0].[GearNickName] AND [t].[SquadId] = [t0].[GearSquadId]
WHERE [t0].[IssueDate] > COALESCE((
    SELECT TOP(1) [t1].[IssueDate]
    FROM [Tags] AS [t1]
    WHERE [t1].[GearNickName] = [t].[FullName]
    ORDER BY [t1].[Id]), '0001-01-01T00:00:00.0000000')");
    }

    public override async Task
        Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection_multiple_grouping_keys(
            bool async)
    {
        await base
            .Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection_multiple_grouping_keys(
                async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t0].[IsAutomatic], [t0].[Name], [t0].[Count]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
OUTER APPLY (
    SELECT [w].[IsAutomatic], [w].[Name], COUNT(*) AS [Count]
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName]
    GROUP BY [w].[IsAutomatic], [w].[Name]
) AS [t0]
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[IsAutomatic]");
    }

    public override async Task Sum_with_no_data_nullable_double(bool async)
    {
        await base.Sum_with_no_data_nullable_double(async);

        AssertSql(
            @"SELECT COALESCE(SUM([m].[Rating]), 0.0E0)
FROM [Missions] AS [m]
WHERE [m].[CodeName] = N'Operation Foobar'");
    }

    public override async Task ToString_guid_property_projection(bool async)
    {
        await base.ToString_guid_property_projection(async);

        AssertSql(
            @"SELECT [t].[GearNickName] AS [A], CONVERT(varchar(36), [t].[Id]) AS [B]
FROM [Tags] AS [t]");
    }

    public override async Task Correlated_collection_with_distinct_not_projecting_identifier_column(bool async)
    {
        await base.Correlated_collection_with_distinct_not_projecting_identifier_column(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t0].[Name], [t0].[IsAutomatic]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
OUTER APPLY (
    SELECT DISTINCT [w].[Name], [w].[IsAutomatic]
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName]
) AS [t0]
ORDER BY [t].[Nickname], [t].[SquadId], [t0].[Name]");
    }

    public override async Task Include_after_Select_throws(bool async)
    {
        await base.Include_after_Select_throws(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[CapitalName], [l].[Name], [l].[ServerAddress], [l].[CommanderName], [l].[Eradicated], [c].[Name], [c].[Location], [c].[Nation]
FROM [LocustHordes] AS [l]
LEFT JOIN [Cities] AS [c] ON [l].[CapitalName] = [c].[Name]");
    }

    public override async Task Cast_to_derived_followed_by_multiple_includes(bool async)
    {
        await base.Cast_to_derived_followed_by_multiple_includes(async);

        AssertSql(
            @"SELECT [t].[Name], [t].[LocustHordeId], [t].[ThreatLevel], [t].[ThreatLevelByte], [t].[ThreatLevelNullableByte], [t].[DefeatedByNickname], [t].[DefeatedBySquadId], [t].[HighCommandId], [t].[Discriminator], [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOfBirthName], [t0].[FullName], [t0].[HasSoulPatch], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [t0].[Discriminator], [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM (
    SELECT [l].[Name], [l].[LocustHordeId], [l].[ThreatLevel], [l].[ThreatLevelByte], [l].[ThreatLevelNullableByte], NULL AS [DefeatedByNickname], NULL AS [DefeatedBySquadId], NULL AS [HighCommandId], N'LocustLeader' AS [Discriminator]
    FROM [LocustLeaders] AS [l]
    UNION ALL
    SELECT [l0].[Name], [l0].[LocustHordeId], [l0].[ThreatLevel], [l0].[ThreatLevelByte], [l0].[ThreatLevelNullableByte], [l0].[DefeatedByNickname], [l0].[DefeatedBySquadId], [l0].[HighCommandId], N'LocustCommander' AS [Discriminator]
    FROM [LocustCommanders] AS [l0]
) AS [t]
LEFT JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t0] ON [t].[DefeatedByNickname] = [t0].[Nickname] AND [t].[DefeatedBySquadId] = [t0].[SquadId]
LEFT JOIN [Weapons] AS [w] ON [t0].[FullName] = [w].[OwnerFullName]
WHERE [t].[Name] LIKE N'%Queen%'
ORDER BY [t].[Name], [t0].[Nickname], [t0].[SquadId]");
    }

    public override async Task Correlated_collection_with_distinct_projecting_identifier_column(bool async)
    {
        await base.Correlated_collection_with_distinct_projecting_identifier_column(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t0].[Id], [t0].[Name]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
OUTER APPLY (
    SELECT DISTINCT [w].[Id], [w].[Name]
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName]
) AS [t0]
ORDER BY [t].[Nickname], [t].[SquadId]");
    }

    public override async Task Where_equals_method_on_nullable_with_object_overload(bool async)
    {
        await base.Where_equals_method_on_nullable_with_object_overload(async);

        AssertSql(
            @"SELECT [m].[Id], [m].[CodeName], [m].[Duration], [m].[Rating], [m].[Timeline]
FROM [Missions] AS [m]
WHERE [m].[Rating] IS NULL");
    }

    public override async Task
        Correlated_collection_with_groupby_not_projecting_identifier_column_but_only_grouping_key_in_final_projection(bool async)
    {
        await base.Correlated_collection_with_groupby_not_projecting_identifier_column_but_only_grouping_key_in_final_projection(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t0].[Key]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
OUTER APPLY (
    SELECT [w].[IsAutomatic] AS [Key]
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName]
    GROUP BY [w].[IsAutomatic]
) AS [t0]
ORDER BY [t].[Nickname], [t].[SquadId]");
    }

    public override async Task Project_derivied_entity_with_convert_to_parent(bool async)
    {
        await base.Project_derivied_entity_with_convert_to_parent(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[CapitalName], [l].[Name], [l].[ServerAddress], [l].[CommanderName], [l].[Eradicated]
FROM [LocustHordes] AS [l]");
    }

    public override async Task Include_after_SelectMany_throws(bool async)
    {
        await base.Include_after_SelectMany_throws(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator], [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [LocustHordes] AS [l]
LEFT JOIN [Cities] AS [c] ON [l].[CapitalName] = [c].[Name]
INNER JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t] ON [c].[Name] = [t].[CityOfBirthName]
INNER JOIN [Squads] AS [s] ON [t].[SquadId] = [s].[Id]");
    }

    public override async Task Correlated_collection_with_distinct_projecting_identifier_column_composite_key(bool async)
    {
        await base.Correlated_collection_with_distinct_projecting_identifier_column_composite_key(async);

        AssertSql(
            @"SELECT [s].[Id], [t0].[Nickname], [t0].[SquadId], [t0].[HasSoulPatch]
FROM [Squads] AS [s]
LEFT JOIN (
    SELECT DISTINCT [t].[Nickname], [t].[SquadId], [t].[HasSoulPatch]
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
) AS [t0] ON [s].[Id] = [t0].[SquadId]
ORDER BY [s].[Id], [t0].[Nickname]");
    }

    public override async Task Include_on_entity_that_is_not_present_in_final_projection_but_uses_TypeIs_instead(bool async)
    {
        await base.Include_on_entity_that_is_not_present_in_final_projection_but_uses_TypeIs_instead(async);

        AssertSql(
            @"SELECT [t].[Nickname], CASE
    WHEN [t].[Discriminator] = N'Officer' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsOfficer]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task GroupBy_Select_sum(bool async)
    {
        await base.GroupBy_Select_sum(async);

        AssertSql(
            @"SELECT COALESCE(SUM([m].[Rating]), 0.0E0)
FROM [Missions] AS [m]
GROUP BY [m].[CodeName]");
    }

    public override async Task ToString_boolean_property_nullable(bool async)
    {
        await base.ToString_boolean_property_nullable(async);

        AssertSql(
            @"SELECT CASE
    WHEN [l].[Eradicated] = CAST(0 AS bit) THEN N'False'
    WHEN [l].[Eradicated] = CAST(1 AS bit) THEN N'True'
    ELSE NULL
END
FROM [LocustHordes] AS [l]");
    }

    public override async Task Correlated_collection_after_distinct_3_levels(bool async)
    {
        await base.Correlated_collection_after_distinct_3_levels(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[Name], [t2].[Nickname], [t2].[FullName], [t2].[HasSoulPatch], [t2].[Id], [t2].[Name], [t2].[Nickname0], [t2].[FullName0], [t2].[HasSoulPatch0], [t2].[Id0]
FROM (
    SELECT DISTINCT [s].[Id], [s].[Name]
    FROM [Squads] AS [s]
) AS [t]
OUTER APPLY (
    SELECT [t0].[Nickname], [t0].[FullName], [t0].[HasSoulPatch], [t1].[Id], [t1].[Name], [t1].[Nickname] AS [Nickname0], [t1].[FullName] AS [FullName0], [t1].[HasSoulPatch] AS [HasSoulPatch0], [t1].[Id0]
    FROM (
        SELECT DISTINCT [t3].[Nickname], [t3].[FullName], [t3].[HasSoulPatch]
        FROM (
            SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
            FROM [Gears] AS [g]
            UNION ALL
            SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
            FROM [Officers] AS [o]
        ) AS [t3]
        WHERE [t3].[SquadId] = [t].[Id]
    ) AS [t0]
    OUTER APPLY (
        SELECT [t].[Id], [t].[Name], [t0].[Nickname], [t0].[FullName], [t0].[HasSoulPatch], [w].[Id] AS [Id0]
        FROM [Weapons] AS [w]
        WHERE [w].[OwnerFullName] = [t0].[FullName]
    ) AS [t1]
) AS [t2]
ORDER BY [t].[Id], [t2].[Nickname], [t2].[FullName], [t2].[HasSoulPatch]");
    }

    public override async Task ToString_boolean_property_non_nullable(bool async)
    {
        await base.ToString_boolean_property_non_nullable(async);

        AssertSql(
            @"SELECT CASE
    WHEN [w].[IsAutomatic] = CAST(0 AS bit) THEN N'False'
    ELSE N'True'
END
FROM [Weapons] AS [w]");
    }

    public override async Task Include_on_derived_entity_with_cast(bool async)
    {
        await base.Include_on_derived_entity_with_cast(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[CapitalName], [l].[Name], [l].[ServerAddress], [l].[CommanderName], [l].[Eradicated], [c].[Name], [c].[Location], [c].[Nation]
FROM [LocustHordes] AS [l]
LEFT JOIN [Cities] AS [c] ON [l].[CapitalName] = [c].[Name]
ORDER BY [l].[Id]");
    }

    public override async Task String_concat_nullable_expressions_are_coalesced(bool async)
    {
        await base.String_concat_nullable_expressions_are_coalesced(async);

        AssertSql(
            @"SELECT (([t].[FullName] + N'') + COALESCE([t].[LeaderNickname], N'')) + N''
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]");
    }

    public override async Task Correlated_collection_with_distinct_projecting_identifier_column_and_correlation_key(bool async)
    {
        await base.Correlated_collection_with_distinct_projecting_identifier_column_and_correlation_key(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t0].[Id], [t0].[Name], [t0].[OwnerFullName]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
LEFT JOIN (
    SELECT DISTINCT [w].[Id], [w].[Name], [w].[OwnerFullName]
    FROM [Weapons] AS [w]
) AS [t0] ON [t].[FullName] = [t0].[OwnerFullName]
ORDER BY [t].[Nickname], [t].[SquadId]");
    }

    public override async Task Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection(
        bool async)
    {
        await base.Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t0].[Key], [t0].[Count]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
OUTER APPLY (
    SELECT [w].[IsAutomatic] AS [Key], COUNT(*) AS [Count]
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName]
    GROUP BY [w].[IsAutomatic]
) AS [t0]
ORDER BY [t].[Nickname], [t].[SquadId]");
    }

    public override async Task Project_discriminator_columns(bool async)
    {
        await base.Project_discriminator_columns(async);

        AssertSql();
    }

    public override async Task
        Correlated_collection_with_groupby_with_complex_grouping_key_not_projecting_identifier_column_with_group_aggregate_in_final_projection(
            bool async)
    {
        await base
            .Correlated_collection_with_groupby_with_complex_grouping_key_not_projecting_identifier_column_with_group_aggregate_in_final_projection(
                async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t0].[Key], [t0].[Count]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
OUTER APPLY (
    SELECT [t1].[Key], COUNT(*) AS [Count]
    FROM (
        SELECT CAST(LEN([w].[Name]) AS int) AS [Key]
        FROM [Weapons] AS [w]
        WHERE [t].[FullName] = [w].[OwnerFullName]
    ) AS [t1]
    GROUP BY [t1].[Key]
) AS [t0]
ORDER BY [t].[Nickname], [t].[SquadId]");
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

        AssertSql();
    }

    public override async Task Streaming_correlated_collection_issue_11403_returning_ordered_enumerable_throws(bool async)
    {
        await base.Streaming_correlated_collection_issue_11403_returning_ordered_enumerable_throws(async);

        AssertSql();
    }

    public override async Task Select_correlated_filtered_collection_returning_queryable_throws(bool async)
    {
        await base.Select_correlated_filtered_collection_returning_queryable_throws(async);

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
            @"SELECT [s].[Id], [s].[Banner], [s].[Banner5], [s].[InternalNumber], [s].[Name]
FROM [Squads] AS [s]
WHERE NOT (EXISTS (
    SELECT 1
    FROM (
        SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
        FROM [Gears] AS [g]
        UNION ALL
        SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
        FROM [Officers] AS [o]
    ) AS [t]
    WHERE [s].[Id] = [t].[SquadId]))");
    }

    public override async Task Where_subquery_equality_to_null_without_composite_key(bool async)
    {
        await base.Where_subquery_equality_to_null_without_composite_key(async);

        AssertSql(
            @"SELECT [t].[Nickname], [t].[SquadId], [t].[AssignedCityName], [t].[CityOfBirthName], [t].[FullName], [t].[HasSoulPatch], [t].[LeaderNickname], [t].[LeaderSquadId], [t].[Rank], [t].[Discriminator]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOfBirthName], [g].[FullName], [g].[HasSoulPatch], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], N'Gear' AS [Discriminator]
    FROM [Gears] AS [g]
    UNION ALL
    SELECT [o].[Nickname], [o].[SquadId], [o].[AssignedCityName], [o].[CityOfBirthName], [o].[FullName], [o].[HasSoulPatch], [o].[LeaderNickname], [o].[LeaderSquadId], [o].[Rank], N'Officer' AS [Discriminator]
    FROM [Officers] AS [o]
) AS [t]
WHERE NOT (EXISTS (
    SELECT 1
    FROM [Weapons] AS [w]
    WHERE [t].[FullName] = [w].[OwnerFullName]))");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
