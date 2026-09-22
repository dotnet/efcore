// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

[SqlServerCondition(SqlServerCondition.SupportsTemporalTablesCascadeDelete)]
public class TPHTemporalFiltersInheritanceQuerySqlServerTest : FiltersInheritanceQueryTestBase<
    TPHTemporalFiltersInheritanceQuerySqlServerFixture>
{
    public TPHTemporalFiltersInheritanceQuerySqlServerTest(
        TPHTemporalFiltersInheritanceQuerySqlServerFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
    {
        serverQueryExpression = base.RewriteServerQueryExpression(serverQueryExpression);

        var temporalEntityTypes = new List<Type>
        {
            typeof(Animal),
            typeof(Plant),
            typeof(Country),
            typeof(Drink),
        };

        var rewriter = new TemporalPointInTimeQueryRewriter(Fixture.ChangesDate, temporalEntityTypes);

        return rewriter.Visit(serverQueryExpression);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Can_use_of_type_animal(bool async)
    {
        await base.Can_use_of_type_animal(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[PeriodEnd], [a].[PeriodStart], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [a]
WHERE [a].[CountryId] = 1
ORDER BY [a].[Species]
""");
    }

    public override async Task Can_use_is_kiwi(bool async)
    {
        await base.Can_use_is_kiwi(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[PeriodEnd], [a].[PeriodStart], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [a]
WHERE [a].[CountryId] = 1 AND [a].[Discriminator] = N'Kiwi'
""");
    }

    public override async Task Can_use_is_kiwi_with_other_predicate(bool async)
    {
        await base.Can_use_is_kiwi_with_other_predicate(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[PeriodEnd], [a].[PeriodStart], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [a]
WHERE [a].[CountryId] = 1 AND [a].[Discriminator] = N'Kiwi' AND [a].[CountryId] = 1
""");
    }

    public override async Task Can_use_is_kiwi_in_projection(bool async)
    {
        await base.Can_use_is_kiwi_in_projection(async);

        AssertSql(
            """
SELECT CASE
    WHEN [a].[Discriminator] = N'Kiwi' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Animals] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [a]
WHERE [a].[CountryId] = 1
""");
    }

    public override async Task Can_use_of_type_bird(bool async)
    {
        await base.Can_use_of_type_bird(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[PeriodEnd], [a].[PeriodStart], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [a]
WHERE [a].[CountryId] = 1
ORDER BY [a].[Species]
""");
    }

    public override async Task Can_use_of_type_bird_predicate(bool async)
    {
        await base.Can_use_of_type_bird_predicate(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[PeriodEnd], [a].[PeriodStart], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [a]
WHERE [a].[CountryId] = 1
ORDER BY [a].[Species]
""");
    }

    public override async Task Can_use_of_type_bird_with_projection(bool async)
    {
        await base.Can_use_of_type_bird_with_projection(async);

        AssertSql(
            """
SELECT [a].[Name]
FROM [Animals] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [a]
WHERE [a].[CountryId] = 1
""");
    }

    public override async Task Can_use_of_type_bird_first(bool async)
    {
        await base.Can_use_of_type_bird_first(async);

        AssertSql(
            """
SELECT TOP(1) [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[PeriodEnd], [a].[PeriodStart], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [a]
WHERE [a].[CountryId] = 1
ORDER BY [a].[Species]
""");
    }

    public override async Task Can_use_of_type_kiwi(bool async)
    {
        await base.Can_use_of_type_kiwi(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[PeriodEnd], [a].[PeriodStart], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
FROM [Animals] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [a]
WHERE [a].[CountryId] = 1 AND [a].[Discriminator] = N'Kiwi'
""");
    }

    public override async Task Can_use_derived_set(bool async)
    {
        await base.Can_use_derived_set(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[PeriodEnd], [a].[PeriodStart], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group]
FROM [Animals] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [a]
WHERE [a].[Discriminator] = N'Eagle' AND [a].[CountryId] = 1
""");
    }

    public override Task Can_use_IgnoreQueryFilters_and_GetDatabaseValues(bool async)
        => Task.CompletedTask;

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
