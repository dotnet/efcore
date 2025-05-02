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

    public override async Task Max(bool async)
    {
        await base.Max(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE GREATEST([b].[Int], [b].[Short] - CAST(3 AS smallint)) = [b].[Int]
""");
    }

    public override async Task Max_nested(bool async)
    {
        await base.Max_nested(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE GREATEST([b].[Short] - CAST(3 AS smallint), [b].[Int], 1) = [b].[Int]
""");
    }

    public override async Task Max_nested_twice(bool async)
    {
        await base.Max_nested_twice(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE GREATEST(1, [b].[Int], 2, [b].[Short] - CAST(3 AS smallint)) = [b].[Int]
""");
    }

    public override async Task Min(bool async)
    {
        await base.Min(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE LEAST([b].[Int], [b].[Short] + CAST(3 AS smallint)) = [b].[Int]
""");
    }

    public override async Task Min_nested(bool async)
    {
        await base.Min_nested(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE LEAST([b].[Short] + CAST(3 AS smallint), [b].[Int], 99999) = [b].[Int]
""");
    }

    public override async Task Min_nested_twice(bool async)
    {
        await base.Min_nested_twice(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE LEAST(99999, [b].[Int], 99998, [b].[Short] + CAST(3 AS smallint)) = [b].[Int]
""");
    }

    public override async Task Log2(bool async)
        => await AssertTranslationFailed(() => base.Log2(async));

    public override async Task Acosh(bool async)
        => await AssertTranslationFailed(() => base.Acosh(async));

    public override async Task Asinh(bool async)
        => await AssertTranslationFailed(() => base.Asinh(async));

    public override async Task Atanh(bool async)
        => await AssertTranslationFailed(() => base.Atanh(async));

    public override async Task Cosh(bool async)
        => await AssertTranslationFailed(() => base.Cosh(async));

    public override async Task Sinh(bool async)
        => await AssertTranslationFailed(() => base.Sinh(async));

    public override async Task Tan(bool async)
    {
        await base.Tan(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE TAN([b].[Double]) > 0.0E0
""");
    }

    public override async Task Tan_float(bool async)
    {
        await base.Tan_float(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE TAN([b].[Float]) > CAST(0 AS real)
""");
    }

    public override async Task Tanh(bool async)
        => await AssertTranslationFailed(() => base.Tanh(async));

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
