// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class DateOnlyTranslationsCosmosTest : DateOnlyTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public DateOnlyTranslationsCosmosTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task Year()
        => AssertTranslationFailed(() => base.Year());

    public override Task Month()
        => AssertTranslationFailed(() => base.Month());

    public override Task Day()
        => AssertTranslationFailed(() => base.Day());

    public override Task DayOfYear()
        => AssertTranslationFailed(() => base.DayOfYear());

    public override Task DayOfWeek()
        => AssertTranslationFailed(() => base.DayOfWeek());

    // Cosmos does not support DateTimeDiff with years under 1601
    public override Task DayNumber()
        => AssertTranslationFailed(() => base.DayNumber());

    public override Task AddYears()
        => AssertTranslationFailed(() => base.AddYears());

    public override Task AddMonths()
        => AssertTranslationFailed(() => base.AddMonths());

    public override Task AddDays()
        => AssertTranslationFailed(() => base.AddDays());

    // Cosmos does not support DateTimeDiff with years under 1601
    public override Task DayNumber_subtraction()
        => AssertTranslationFailed(() => base.DayNumber_subtraction());

    public override Task FromDateTime()
        => AssertTranslationFailed(() => base.FromDateTime());

    public override Task FromDateTime_compared_to_property()
        => AssertTranslationFailed(() => base.FromDateTime());

    public override Task FromDateTime_compared_to_constant_and_parameter()
        => AssertTranslationFailed(() => base.FromDateTime());

    public override Task ToDateTime_property_with_constant_TimeOnly()
        => AssertTranslationFailed(() => base.ToDateTime_property_with_constant_TimeOnly());

    public override Task ToDateTime_property_with_property_TimeOnly()
        => AssertTranslationFailed(() => base.ToDateTime_property_with_property_TimeOnly());

    public override Task ToDateTime_constant_DateTime_with_property_TimeOnly()
        => AssertTranslationFailed(() => base.ToDateTime_constant_DateTime_with_property_TimeOnly());

    public override Task ToDateTime_with_complex_DateTime()
        => AssertTranslationFailed(() => base.ToDateTime_with_complex_DateTime());

    public override Task ToDateTime_with_complex_TimeOnly()
        => AssertTranslationFailed(() => base.ToDateTime_with_complex_TimeOnly());

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
