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
    public override async Task Hours(bool async)
    {
        await AssertTranslationFailed(() => base.Hours(async));

        AssertSql();
    }

    // Translate TimeSpan members, #18844
    public override async Task Minutes(bool async)
    {
        await AssertTranslationFailed(() => base.Minutes(async));

        AssertSql();
    }

    public override async Task Seconds(bool async)
    {
        await AssertTranslationFailed(() => base.Seconds(async));

        AssertSql();
    }

    // Translate TimeSpan members, #18844
    public override async Task Milliseconds(bool async)
    {
        await AssertTranslationFailed(() => base.Milliseconds(async));

        AssertSql();
    }

    // Translate TimeSpan members, #18844
    public override async Task Microseconds(bool async)
    {
        await AssertTranslationFailed(() => base.Microseconds(async));

        AssertSql();
    }

    // Translate TimeSpan members, #18844
    public override async Task Nanoseconds(bool async)
    {
        await AssertTranslationFailed(() => base.Nanoseconds(async));

        AssertSql();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
