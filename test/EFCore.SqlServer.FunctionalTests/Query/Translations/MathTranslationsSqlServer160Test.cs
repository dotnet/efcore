// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations;

[SqlServerCondition(SqlServerCondition.SupportsFunctions2022)]
public class MathTranslationsSqlServer160Test : MathTranslationsTestBase<BasicTypesQuerySqlServer160Fixture>
{
    public MathTranslationsSqlServer160Test(BasicTypesQuerySqlServer160Fixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Max()
    {
        await base.Max();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE GREATEST([b].[Int], [b].[Short] - CAST(3 AS smallint)) = [b].[Int]
""");
    }

    public override async Task Max_nested()
    {
        await base.Max_nested();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE GREATEST([b].[Short] - CAST(3 AS smallint), [b].[Int], 1) = [b].[Int]
""");
    }

    public override async Task Max_nested_twice()
    {
        await base.Max_nested_twice();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE GREATEST(1, [b].[Int], 2, [b].[Short] - CAST(3 AS smallint)) = [b].[Int]
""");
    }

    public override async Task Min()
    {
        await base.Min();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE LEAST([b].[Int], [b].[Short] + CAST(3 AS smallint)) = [b].[Int]
""");
    }

    public override async Task Min_nested()
    {
        await base.Min_nested();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE LEAST([b].[Short] + CAST(3 AS smallint), [b].[Int], 99999) = [b].[Int]
""");
    }

    public override async Task Min_nested_twice()
    {
        await base.Min_nested_twice();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE LEAST(99999, [b].[Int], 99998, [b].[Short] + CAST(3 AS smallint)) = [b].[Int]
""");
    }

    public override async Task Log2()
        => await AssertTranslationFailed(() => base.Log2());

    public override async Task Acosh()
        => await AssertTranslationFailed(() => base.Acosh());

    public override async Task Asinh()
        => await AssertTranslationFailed(() => base.Asinh());

    public override async Task Atanh()
        => await AssertTranslationFailed(() => base.Atanh());

    public override async Task Cosh()
        => await AssertTranslationFailed(() => base.Cosh());

    public override async Task Sinh()
        => await AssertTranslationFailed(() => base.Sinh());

    public override async Task Tan()
    {
        await base.Tan();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE TAN([b].[Double]) > 0.0E0
""");
    }

    public override async Task Tan_float()
    {
        await base.Tan_float();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE TAN([b].[Float]) > CAST(0 AS real)
""");
    }

    public override async Task Tanh()
        => await AssertTranslationFailed(() => base.Tanh());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
