// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations.Operators;

public class MiscellaneousOperatorTranslationsSqlServerTest : MiscellaneousOperatorTranslationsTestBase<BasicTypesQuerySqlServerFixture>
{
    public MiscellaneousOperatorTranslationsSqlServerTest(BasicTypesQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Conditional(bool async)
    {
        await base.Conditional(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CASE
    WHEN [b].[Int] = 8 THEN [b].[String]
    ELSE N'Foo'
END = N'Seattle'
""");
    }

    public override async Task Coalesce(bool async)
    {
        await base.Coalesce(async);

        AssertSql(
            """
SELECT [n].[Id], [n].[Bool], [n].[Byte], [n].[ByteArray], [n].[DateOnly], [n].[DateTime], [n].[DateTimeOffset], [n].[Decimal], [n].[Double], [n].[Enum], [n].[FlagsEnum], [n].[Float], [n].[Guid], [n].[Int], [n].[Long], [n].[Short], [n].[String], [n].[TimeOnly], [n].[TimeSpan]
FROM [NullableBasicTypesEntities] AS [n]
WHERE COALESCE([n].[String], N'Unknown') = N'Seattle'
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
