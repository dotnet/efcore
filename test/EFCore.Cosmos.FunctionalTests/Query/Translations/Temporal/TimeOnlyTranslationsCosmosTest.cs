// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class TimeOnlyTranslationsCosmosTest : TimeOnlyTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public TimeOnlyTranslationsCosmosTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper) : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task Hour(bool async)
        => AssertTranslationFailed(() => base.Hour(async));

    public override Task Minute(bool async)
        => AssertTranslationFailed(() => base.Minute(async));

    public override Task Second(bool async)
        => AssertTranslationFailed(() => base.Second(async));

    public override Task Millisecond(bool async)
        => AssertTranslationFailed(() => base.Millisecond(async));

    public override Task Microsecond(bool async)
        => AssertTranslationFailed(() => base.Microsecond(async));

    public override Task Nanosecond(bool async)
        => AssertTranslationFailed(() => base.Nanosecond(async));

    public override Task AddHours(bool async)
        => AssertTranslationFailed(() => base.AddHours(async));

    public override Task AddMinutes(bool async)
        => AssertTranslationFailed(() => base.AddMinutes(async));

    public override Task Add_TimeSpan(bool async)
        => AssertTranslationFailed(() => base.Add_TimeSpan(async));

    public override Task IsBetween(bool async)
        => AssertTranslationFailed(() => base.IsBetween(async));

    public override async Task Subtract(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Fixture.NoSyncTest(
                async, async a =>
                {
                    // See #35311
                    await Assert.ThrowsAsync<EqualException>(() => base.Subtract(a));

                    AssertSql(
                        """
SELECT VALUE c
FROM root c
WHERE ((c["TimeOnly"] - "03:00:00") = "12:30:10")
""");
                });
        }
    }

    public override Task FromDateTime_compared_to_property(bool async)
        => AssertTranslationFailed(() => base.FromDateTime_compared_to_property(async));

    public override Task FromDateTime_compared_to_parameter(bool async)
        => AssertTranslationFailed(() => base.FromDateTime_compared_to_parameter(async));

    public override Task FromDateTime_compared_to_constant(bool async)
        => AssertTranslationFailed(() => base.FromDateTime_compared_to_constant(async));

    public override Task FromTimeSpan_compared_to_property(bool async)
        => AssertTranslationFailed(() => base.FromTimeSpan_compared_to_property(async));

    public override Task FromTimeSpan_compared_to_parameter(bool async)
        => AssertTranslationFailed(() => base.FromTimeSpan_compared_to_parameter(async));

    public override Task Order_by_FromTimeSpan(bool async)
        => AssertTranslationFailed(() => base.Order_by_FromTimeSpan(async));

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
