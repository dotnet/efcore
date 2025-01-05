// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations.Operators;

public class BitwiseOperatorTranslationsSqlServerTest : BitwiseOperatorTranslationsTestBase<BasicTypesQuerySqlServerFixture>
{
    public BitwiseOperatorTranslationsSqlServerTest(BasicTypesQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Or(bool async)
    {
        await base.Or(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST([b].[Int] AS bigint) | [b].[Long] = CAST(7 AS bigint)
""",
            //
            """
SELECT CAST([b].[Int] AS bigint) | [b].[Long]
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task Or_over_boolean(bool async)
    {
        await base.Or_over_boolean(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[Int] = 12 OR [b].[String] = N'Seattle'
""",
            //
            """
SELECT CASE
    WHEN [b].[Int] = 12 OR [b].[String] = N'Seattle' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task Or_multiple(bool async)
    {
        await base.Or_multiple(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST([b].[Int] | [b].[Short] AS bigint) | [b].[Long] = CAST(7 AS bigint)
""");
    }

    public override async Task And(bool async)
    {
        await base.And(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[Int] & [b].[Short] = 2
""",
            //
            """
SELECT [b].[Int] & [b].[Short]
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task And_over_boolean(bool async)
    {
        await base.And_over_boolean(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[Int] = 8 AND [b].[String] = N'Seattle'
""",
            //
            """
SELECT CASE
    WHEN [b].[Int] = 8 AND [b].[String] = N'Seattle' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task Xor(bool async)
    {
        await base.Xor(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[Int] ^ [b].[Short] = 1
""",
            //
            """
SELECT [b].[Int] ^ [b].[Short]
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task Xor_over_boolean(bool async)
    {
        await base.Xor_over_boolean(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE ~CAST([b].[Int] ^ [b].[Short] AS bit) ^ CASE
    WHEN [b].[String] = N'Seattle' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CAST(1 AS bit)
""");
    }

    public override async Task Complement(bool async)
    {
        await base.Complement(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE ~[b].[Int] = -9
""");
    }

    public override async Task And_or_over_boolean(bool async)
    {
        await base.And_or_over_boolean(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE ([b].[Int] = 12 AND [b].[Short] = CAST(12 AS smallint)) OR [b].[String] = N'Seattle'
""");
    }

    public override async Task Or_with_logical_or(bool async)
    {
        await base.Or_with_logical_or(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[Int] = 12 OR [b].[Short] = CAST(12 AS smallint) OR [b].[String] = N'Seattle'
""");
    }

    public override async Task And_with_logical_and(bool async)
    {
        await base.And_with_logical_and(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[Int] = 8 AND [b].[Short] = CAST(8 AS smallint) AND [b].[String] = N'Seattle'
""");
    }

    public override async Task Or_with_logical_and(bool async)
    {
        await base.Or_with_logical_and(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE ([b].[Int] = 8 OR [b].[Short] = CAST(9 AS smallint)) AND [b].[String] = N'Seattle'
""");
    }

    public override async Task And_with_logical_or(bool async)
    {
        await base.And_with_logical_or(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE ([b].[Int] = 12 AND [b].[Short] = CAST(12 AS smallint)) OR [b].[String] = N'Seattle'
""");
    }

    public override Task Left_shift(bool async)
        => AssertTranslationFailed(() => base.Left_shift(async));

    public override Task Right_shift(bool async)
        => AssertTranslationFailed(() => base.Right_shift(async));

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
