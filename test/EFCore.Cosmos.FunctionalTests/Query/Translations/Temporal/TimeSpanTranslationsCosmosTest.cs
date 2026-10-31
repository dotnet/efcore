// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class TimeSpanTranslationsCosmosTest : TimeSpanTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public TimeSpanTranslationsCosmosTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task Hours()
        => AssertTranslationFailed(base.Hours);

    public override Task Minutes()
        => AssertTranslationFailed(base.Minutes);

    public override Task Seconds()
        => AssertTranslationFailed(base.Seconds);

    public override Task Milliseconds()
        => AssertTranslationFailed(base.Milliseconds);

    public override Task Microseconds()
        => AssertTranslationFailed(base.Microseconds);

    public override Task Nanoseconds()
        => AssertTranslationFailed(base.Nanoseconds);

    public override Task TotalDays()
        => AssertTranslationFailed(base.TotalDays);

    public override Task TotalHours()
        => AssertTranslationFailed(base.TotalHours);

    public override Task TotalMinutes()
        => AssertTranslationFailed(base.TotalMinutes);

    public override Task TotalSeconds()
        => AssertTranslationFailed(base.TotalSeconds);

    public override Task TotalMilliseconds()
        => AssertTranslationFailed(base.TotalMilliseconds);

    public override Task TotalMicroseconds()
        => AssertTranslationFailed(base.TotalMicroseconds);

    public override Task TotalNanoseconds()
        => AssertTranslationFailed(base.TotalNanoseconds);

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
