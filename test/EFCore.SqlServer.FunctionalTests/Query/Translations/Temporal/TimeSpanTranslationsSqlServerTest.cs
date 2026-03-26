// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class TimeSpanTranslationsSqlServerTest : TimeSpanTranslationsTestBase<BasicTypesQuerySqlServerFixture>
{
    public TimeSpanTranslationsSqlServerTest(BasicTypesQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Hours()
    {
        await base.Hours();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(hour, [b].[TimeSpan]) = 3
""");
    }

    public override async Task Minutes()
    {
        await base.Minutes();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(minute, [b].[TimeSpan]) = 4
""");
    }

    public override async Task Seconds()
    {
        await base.Seconds();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(second, [b].[TimeSpan]) = 5
""");
    }

    public override async Task Milliseconds()
    {
        await base.Milliseconds();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(millisecond, [b].[TimeSpan]) = 678
""");
    }

    public override async Task Microseconds()
    {
        await base.Microseconds();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(microsecond, [b].[TimeSpan]) % 1000 = 912
""");
    }

    public override async Task Nanoseconds()
    {
        await base.Nanoseconds();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(nanosecond, [b].[TimeSpan]) % 1000 = 400
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
