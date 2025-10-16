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

    public override async Task Hour()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Hour());

        AssertSql();
    }

    public override async Task Minute()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Minute());

        AssertSql();
    }

    public override async Task Second()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Second());

        AssertSql();
    }

    public override async Task Millisecond()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Millisecond());

        AssertSql();
    }

    public override async Task Microsecond()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Microsecond());

        AssertSql();
    }

    public override async Task Nanosecond()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Nanosecond());

        AssertSql();
    }

    public override async Task AddHours()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.AddHours());

        AssertSql();
    }

    public override async Task AddMinutes()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.AddMinutes());

        AssertSql();
    }

    public override async Task Add_TimeSpan()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Add_TimeSpan());

        AssertSql();
    }

    public override async Task IsBetween()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.IsBetween());

        AssertSql();
    }

    public override async Task Subtract()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Subtract());

        AssertSql();
    }

    public override async Task FromDateTime_compared_to_property()
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(() => base.FromDateTime_compared_to_property());

        AssertSql();
    }

    public override async Task FromDateTime_compared_to_parameter()
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(() => base.FromDateTime_compared_to_parameter());

        AssertSql();
    }

    public override async Task FromDateTime_compared_to_constant()
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(() => base.FromDateTime_compared_to_constant());

        AssertSql();
    }

    public override async Task FromTimeSpan_compared_to_property()
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(() => base.FromTimeSpan_compared_to_property());

        AssertSql();
    }

    public override async Task FromTimeSpan_compared_to_parameter()
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(() => base.FromTimeSpan_compared_to_parameter());

        AssertSql();
    }

    public override async Task Order_by_FromTimeSpan()
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(() => base.Order_by_FromTimeSpan());

        AssertSql();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
