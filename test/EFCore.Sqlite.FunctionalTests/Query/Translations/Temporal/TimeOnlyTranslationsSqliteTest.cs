// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class TimeOnlyTranslationsSqliteTest : TimeOnlyTranslationsTestBase<BasicTypesQuerySqliteFixture>
{
    public TimeOnlyTranslationsSqliteTest(BasicTypesQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Hour(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Hour(async));

        AssertSql();
    }

    public override async Task Minute(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Minute(async));

        AssertSql();
    }

    public override async Task Second(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Second(async));

        AssertSql();
    }

    public override async Task Millisecond(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Millisecond(async));

        AssertSql();
    }

    public override async Task Microsecond(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Microsecond(async));

        AssertSql();
    }

    public override async Task Nanosecond(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Nanosecond(async));

        AssertSql();
    }

    public override async Task AddHours(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.AddHours(async));

        AssertSql();
    }

    public override async Task AddMinutes(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.AddMinutes(async));

        AssertSql();
    }

    public override async Task Add_TimeSpan(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Add_TimeSpan(async));

        AssertSql();
    }

    public override async Task IsBetween(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.IsBetween(async));

        AssertSql();
    }

    public override async Task Subtract(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Subtract(async));

        AssertSql();
    }

    public override async Task FromDateTime_compared_to_property(bool async)
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(() => base.FromDateTime_compared_to_property(async));

        AssertSql();
    }

    public override async Task FromDateTime_compared_to_parameter(bool async)
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(() => base.FromDateTime_compared_to_parameter(async));

        AssertSql();
    }

    public override async Task FromDateTime_compared_to_constant(bool async)
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(() => base.FromDateTime_compared_to_constant(async));

        AssertSql();
    }

    public override async Task FromTimeSpan_compared_to_property(bool async)
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(() => base.FromTimeSpan_compared_to_property(async));

        AssertSql();
    }

    public override async Task FromTimeSpan_compared_to_parameter(bool async)
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(() => base.FromTimeSpan_compared_to_parameter(async));

        AssertSql();
    }

    public override async Task Order_by_FromTimeSpan(bool async)
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(() => base.Order_by_FromTimeSpan(async));

        AssertSql();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
