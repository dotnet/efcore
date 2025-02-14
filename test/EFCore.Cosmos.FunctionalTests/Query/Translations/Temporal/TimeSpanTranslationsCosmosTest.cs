// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class TimeSpanTranslationsCosmosTest : TimeSpanTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public TimeSpanTranslationsCosmosTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper) : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task Hours(bool async)
        => AssertTranslationFailed(() => base.Hours(async));

    public override Task Minutes(bool async)
        => AssertTranslationFailed(() => base.Minutes(async));

    public override Task Seconds(bool async)
        => AssertTranslationFailed(() => base.Seconds(async));

    public override Task Milliseconds(bool async)
        => AssertTranslationFailed(() => base.Milliseconds(async));

    public override Task Microseconds(bool async)
        => AssertTranslationFailed(() => base.Microseconds(async));

    public override Task Nanoseconds(bool async)
        => AssertTranslationFailed(() => base.Nanoseconds(async));

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
