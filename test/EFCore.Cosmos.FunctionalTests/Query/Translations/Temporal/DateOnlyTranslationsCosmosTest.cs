// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class DateOnlyTranslationsCosmosTest : DateOnlyTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public DateOnlyTranslationsCosmosTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper) : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task Year(bool async)
        => AssertTranslationFailed(() => base.Year(async));

    public override Task Month(bool async)
        => AssertTranslationFailed(() => base.Month(async));

    public override Task Day(bool async)
        => AssertTranslationFailed(() => base.Day(async));

    public override Task DayOfYear(bool async)
        => AssertTranslationFailed(() => base.DayOfYear(async));

    public override Task DayOfWeek(bool async)
        => AssertTranslationFailed(() => base.DayOfWeek(async));

    public override Task AddYears(bool async)
        => AssertTranslationFailed(() => base.AddYears(async));

    public override Task AddMonths(bool async)
        => AssertTranslationFailed(() => base.AddMonths(async));

    public override Task AddDays(bool async)
        => AssertTranslationFailed(() => base.AddDays(async));

    public override Task FromDateTime(bool async)
        => AssertTranslationFailed(() => base.FromDateTime(async));

    public override Task FromDateTime_compared_to_property(bool async)
        => AssertTranslationFailed(() => base.FromDateTime(async));

    public override Task FromDateTime_compared_to_constant_and_parameter(bool async)
        => AssertTranslationFailed(() => base.FromDateTime(async));

    public override Task ToDateTime_property_with_constant_TimeOnly(bool async)
        => AssertTranslationFailed(() => base.ToDateTime_property_with_constant_TimeOnly(async));

    public override Task ToDateTime_property_with_property_TimeOnly(bool async)
        => AssertTranslationFailed(() => base.ToDateTime_property_with_property_TimeOnly(async));

    public override Task ToDateTime_constant_DateTime_with_property_TimeOnly(bool async)
        => AssertTranslationFailed(() => base.ToDateTime_constant_DateTime_with_property_TimeOnly(async));

    public override Task ToDateTime_with_complex_DateTime(bool async)
        => AssertTranslationFailed(() => base.ToDateTime_with_complex_DateTime(async));

    public override Task ToDateTime_with_complex_TimeOnly(bool async)
        => AssertTranslationFailed(() => base.ToDateTime_with_complex_TimeOnly(async));

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
