// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class TimeSpanTranslationsSqliteTest : TimeSpanTranslationsTestBase<BasicTypesQuerySqliteFixture>
{
    public TimeSpanTranslationsSqliteTest(BasicTypesQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // Translate TimeSpan members, #18844
    public override async Task Hours()
    {
        await AssertTranslationFailed(() => base.Hours());

        AssertSql();
    }

    // Translate TimeSpan members, #18844
    public override async Task Minutes()
    {
        await AssertTranslationFailed(() => base.Minutes());

        AssertSql();
    }

    public override async Task Seconds()
    {
        await AssertTranslationFailed(() => base.Seconds());

        AssertSql();
    }

    // Translate TimeSpan members, #18844
    public override async Task Milliseconds()
    {
        await AssertTranslationFailed(() => base.Milliseconds());

        AssertSql();
    }

    // Translate TimeSpan members, #18844
    public override async Task Microseconds()
    {
        await AssertTranslationFailed(() => base.Microseconds());

        AssertSql();
    }

    // Translate TimeSpan members, #18844
    public override async Task Nanoseconds()
    {
        await AssertTranslationFailed(() => base.Nanoseconds());

        AssertSql();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
