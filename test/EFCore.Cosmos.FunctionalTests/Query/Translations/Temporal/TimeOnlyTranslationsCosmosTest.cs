// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class TimeOnlyTranslationsCosmosTest : TimeOnlyTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public TimeOnlyTranslationsCosmosTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task Hour()
        => AssertTranslationFailed(base.Hour);

    public override Task Minute()
        => AssertTranslationFailed(base.Minute);

    public override Task Second()
        => AssertTranslationFailed(base.Second);

    public override Task Millisecond()
        => AssertTranslationFailed(base.Millisecond);

    public override Task Microsecond()
        => AssertTranslationFailed(base.Microsecond);

    public override Task Nanosecond()
        => AssertTranslationFailed(base.Nanosecond);

    public override Task AddHours()
        => AssertTranslationFailed(base.AddHours);

    public override Task AddMinutes()
        => AssertTranslationFailed(base.AddMinutes);

    public override Task Add_TimeSpan()
        => AssertTranslationFailed(base.Add_TimeSpan);

    public override Task IsBetween()
        => AssertTranslationFailed(base.IsBetween);

    public override Task IsBetween_midnight(bool async)
        => AssertTranslationFailed(() => base.IsBetween_midnight(async));

    public override Task IsBetween_parameters(bool async, int range)
        => AssertTranslationFailed(() => base.IsBetween_parameters(async, range));

    public override Task IsBetween_boundaries(bool async)
        => AssertTranslationFailed(() => base.IsBetween_boundaries(async));

    public override Task IsBetween_precision(bool async)
        => AssertTranslationFailed(() => base.IsBetween_precision(async));

    public override Task IsBetween_negated(bool async)
        => AssertTranslationFailed(() => base.IsBetween_negated(async));

    public override Task IsBetween_nullable(bool async)
        => AssertTranslationFailed(() => base.IsBetween_nullable(async));

    public override Task IsBetween_column_bounds(bool async)
        => AssertTranslationFailed(() => base.IsBetween_column_bounds(async));

    public override async Task Subtract()
    {
        // See #35311
        await Assert.ThrowsAsync<EqualException>(base.Subtract);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["TimeOnly"] - "03:00:00") = "12:30:10")
""");
    }

    public override Task FromDateTime_compared_to_property()
        => AssertTranslationFailed(base.FromDateTime_compared_to_property);

    public override Task FromDateTime_compared_to_parameter()
        => AssertTranslationFailed(base.FromDateTime_compared_to_parameter);

    public override Task FromDateTime_compared_to_constant()
        => AssertTranslationFailed(base.FromDateTime_compared_to_constant);

    public override Task FromTimeSpan_compared_to_property()
        => AssertTranslationFailed(base.FromTimeSpan_compared_to_property);

    public override Task FromTimeSpan_compared_to_parameter()
        => AssertTranslationFailed(base.FromTimeSpan_compared_to_parameter);

    public override Task Order_by_FromTimeSpan()
        => AssertTranslationFailed(base.Order_by_FromTimeSpan);

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
