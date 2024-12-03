// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class TemporalTranslationsCosmosTest : TemporalTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public TemporalTranslationsCosmosTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper) : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region DateTime

    public override async Task DateTime_Now(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.DateTime_Now(async));

        AssertSql();
    }

    public override Task DateTime_UtcNow(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.DateTime_UtcNow(a);

                AssertSql(
                    """
@myDatetime=?

SELECT VALUE c
FROM root c
WHERE (GetCurrentDateTime() != @myDatetime)
""");
            });

    public override async Task DateTime_Today(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.DateTime_Today(async));

        AssertSql();
    }

    public override async Task DateTime_Date(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.DateTime_Date(async));

        AssertSql();
    }

    public override Task DateTime_AddYear(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.DateTime_AddYear(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("yyyy", DateTimeAdd("yyyy", 1, c["DateTime"])) = 1999)
""");
            });

    public override Task DateTime_Year(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.DateTime_Year(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("yyyy", c["DateTime"]) = 1998)
""");
            });

    public override Task DateTime_Month(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.DateTime_Month(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("mm", c["DateTime"]) = 5)
""");
            });

    public override async Task DateTime_DayOfYear(bool async)
    {
        // DateTime.DayOfYear not supported by Cosmos
        await AssertTranslationFailed(() => base.DateTime_DayOfYear(async));

        AssertSql();
    }

    public override Task DateTime_Day(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.DateTime_Day(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("dd", c["DateTime"]) = 4)
""");
            });

    public override Task DateTime_Hour(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.DateTime_Hour(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("hh", c["DateTime"]) = 15)
""");
            });

    public override Task DateTime_Minute(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.DateTime_Minute(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("mi", c["DateTime"]) = 30)
""");
            });

    public override Task DateTime_Second(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.DateTime_Second(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("ss", c["DateTime"]) = 10)
""");
            });

    public override Task DateTime_Millisecond(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.DateTime_Millisecond(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("ms", c["DateTime"]) = 123)
""");
            });

    public override async Task DateTime_TimeOfDay(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.DateTime_TimeOfDay(async));

        AssertSql();
    }

    public override async Task DateTime_subtract_and_TotalDays(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.DateTime_subtract_and_TotalDays(async));

        AssertSql();
    }

    #endregion DateTime

    #region DateOnly

    public override Task DateOnly_Year(bool async)
        => AssertTranslationFailed(() => base.DateOnly_Year(async));

    public override Task DateOnly_Month(bool async)
        => AssertTranslationFailed(() => base.DateOnly_Month(async));

    public override Task DateOnly_Day(bool async)
        => AssertTranslationFailed(() => base.DateOnly_Day(async));

    public override Task DateOnly_DayOfYear(bool async)
        => AssertTranslationFailed(() => base.DateOnly_DayOfYear(async));

    public override Task DateOnly_DayOfWeek(bool async)
        => AssertTranslationFailed(() => base.DateOnly_DayOfWeek(async));

    public override Task DateOnly_AddYears(bool async)
        => AssertTranslationFailed(() => base.DateOnly_AddYears(async));

    public override Task DateOnly_AddMonths(bool async)
        => AssertTranslationFailed(() => base.DateOnly_AddMonths(async));

    public override Task DateOnly_AddDays(bool async)
        => AssertTranslationFailed(() => base.DateOnly_AddDays(async));

    public override Task DateOnly_FromDateTime(bool async)
        => AssertTranslationFailed(() => base.DateOnly_FromDateTime(async));

    public override Task DateOnly_FromDateTime_compared_to_property(bool async)
        => AssertTranslationFailed(() => base.DateOnly_FromDateTime(async));

    public override Task DateOnly_FromDateTime_compared_to_constant_and_parameter(bool async)
        => AssertTranslationFailed(() => base.DateOnly_FromDateTime(async));

    #endregion DateOnly

    #region TimeOnly

    public override Task TimeOnly_Hour(bool async)
        => AssertTranslationFailed(() => base.TimeOnly_Hour(async));

    public override Task TimeOnly_Minute(bool async)
        => AssertTranslationFailed(() => base.TimeOnly_Minute(async));

    public override Task TimeOnly_Second(bool async)
        => AssertTranslationFailed(() => base.TimeOnly_Second(async));

    public override Task TimeOnly_Millisecond(bool async)
        => AssertTranslationFailed(() => base.TimeOnly_Millisecond(async));

    public override Task TimeOnly_Microsecond(bool async)
        => AssertTranslationFailed(() => base.TimeOnly_Microsecond(async));

    public override Task TimeOnly_Nanosecond(bool async)
        => AssertTranslationFailed(() => base.TimeOnly_Nanosecond(async));

    public override Task TimeOnly_AddHours(bool async)
        => AssertTranslationFailed(() => base.TimeOnly_AddHours(async));

    public override Task TimeOnly_AddMinutes(bool async)
        => AssertTranslationFailed(() => base.TimeOnly_AddMinutes(async));

    public override Task TimeOnly_Add_TimeSpan(bool async)
        => AssertTranslationFailed(() => base.TimeOnly_Add_TimeSpan(async));

    public override Task TimeOnly_IsBetween(bool async)
        => AssertTranslationFailed(() => base.TimeOnly_IsBetween(async));

    public override async Task TimeOnly_subtract_TimeOnly(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Fixture.NoSyncTest(
                async, async a =>
                {
                    // See #35311
                    await Assert.ThrowsAsync<EqualException>(() => base.TimeOnly_subtract_TimeOnly(a));

                    AssertSql(
                        """
SELECT VALUE c
FROM root c
WHERE ((c["TimeOnly"] - "03:00:00") = "12:30:10")
""");
                });
        }
    }

    public override Task TimeOnly_FromDateTime_compared_to_property(bool async)
        => AssertTranslationFailed(() => base.TimeOnly_FromDateTime_compared_to_property(async));

    public override Task TimeOnly_FromDateTime_compared_to_parameter(bool async)
        => AssertTranslationFailed(() => base.TimeOnly_FromDateTime_compared_to_parameter(async));

    public override Task TimeOnly_FromDateTime_compared_to_constant(bool async)
        => AssertTranslationFailed(() => base.TimeOnly_FromDateTime_compared_to_constant(async));

    public override Task TimeOnly_FromTimeSpan_compared_to_property(bool async)
        => AssertTranslationFailed(() => base.TimeOnly_FromTimeSpan_compared_to_property(async));

    public override Task TimeOnly_FromTimeSpan_compared_to_parameter(bool async)
        => AssertTranslationFailed(() => base.TimeOnly_FromTimeSpan_compared_to_parameter(async));

    public override Task Order_by_TimeOnly_FromTimeSpan(bool async)
        => AssertTranslationFailed(() => base.Order_by_TimeOnly_FromTimeSpan(async));

    #endregion TimeOnly

    #region DateTimeOffset

    public override async Task DateTimeOffset_Now(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.DateTimeOffset_Now(async));

        AssertSql();
    }

    public override Task DateTimeOffset_UtcNow(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.DateTimeOffset_UtcNow(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["DateTimeOffset"] != GetCurrentDateTime())
""");
            });

    public override async Task DateTimeOffset_Date(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.DateTimeOffset_Date(async));

        AssertSql();
    }

    public override async Task DateTimeOffset_Year(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Fixture.NoSyncTest(
                async, async a =>
                {
                    // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
                    await Assert.ThrowsAsync<EqualException>(() => base.DateTimeOffset_Year(a));

                    AssertSql(
                        """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("yyyy", c["DateTimeOffset"]) = 1998)
""");
                });
        }
    }

    public override async Task DateTimeOffset_Month(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Fixture.NoSyncTest(
                async, async a =>
                {
                    // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
                    await Assert.ThrowsAsync<EqualException>(() => base.DateTimeOffset_Month(a));

                    AssertSql(
                        """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("mm", c["DateTimeOffset"]) = 5)
""");
                });
        }
    }

    public override async Task DateTimeOffset_DayOfYear(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.DateTimeOffset_DayOfYear(async));

        AssertSql();
    }

    public override async Task DateTimeOffset_Day(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Fixture.NoSyncTest(
                async, async a =>
                {
                    // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
                    await Assert.ThrowsAsync<EqualException>(() => base.DateTimeOffset_Day(a));

                    AssertSql(
                        """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("dd", c["DateTimeOffset"]) = 4)
""");
                });
        }
    }

    public override async Task DateTimeOffset_Hour(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Fixture.NoSyncTest(
                async, async a =>
                {
                    // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
                    await Assert.ThrowsAsync<EqualException>(() => base.DateTimeOffset_Hour(a));

                    AssertSql(
                        """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("hh", c["DateTimeOffset"]) = 15)
""");
                });
        }
    }

    public override async Task DateTimeOffset_Minute(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Fixture.NoSyncTest(
                async, async a =>
                {
                    // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
                    await Assert.ThrowsAsync<EqualException>(() => base.DateTimeOffset_Minute(a));

                    AssertSql(
                        """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("mi", c["DateTimeOffset"]) = 30)
""");
                });
        }
    }

    public override async Task DateTimeOffset_Second(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Fixture.NoSyncTest(
                async, async a =>
                {
                    // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
                    await Assert.ThrowsAsync<EqualException>(() => base.DateTimeOffset_Second(a));

                    AssertSql(
                        """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("ss", c["DateTimeOffset"]) = 10)
""");
                });
        }
    }


    public override async Task DateTimeOffset_Millisecond(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Fixture.NoSyncTest(
                async, async a =>
                {
                    // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
                    await Assert.ThrowsAsync<EqualException>(() => base.DateTimeOffset_Millisecond(a));

                    AssertSql(
                        """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("ms", c["DateTimeOffset"]) = 123)
""");
                });
        }
    }

    public override async Task DateTimeOffset_Microsecond(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Fixture.NoSyncTest(
                async, async a =>
                {
                    // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
                    await Assert.ThrowsAsync<EqualException>(() => base.DateTimeOffset_Microsecond(a));

                    AssertSql(
                        """
SELECT VALUE c
FROM root c
WHERE ((DateTimePart("mcs", c["DateTimeOffset"]) % 1000) = 456)
""");
                });
        }
    }

    public override async Task DateTimeOffset_Nanosecond(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Fixture.NoSyncTest(
                async, async a =>
                {
                    // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
                    await Assert.ThrowsAsync<EqualException>(() => base.DateTimeOffset_Nanosecond(a));

                    AssertSql(
                        """
SELECT VALUE c
FROM root c
WHERE ((DateTimePart("ns", c["DateTimeOffset"]) % 1000) = 400)
""");
                });
        }
    }

    public override Task DateTimeOffset_TimeOfDay(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.DateTimeOffset_TimeOfDay(a);

                AssertSql(
                    """
SELECT VALUE c["DateTimeOffset"]
FROM root c
""");
            });

    public override async Task DateTimeOffset_AddYears(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Fixture.NoSyncTest(
                async, async a =>
                {
                    // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
                    await Assert.ThrowsAsync<EqualException>(() => base.DateTimeOffset_AddYears(a));

                    AssertSql(
                        """
SELECT VALUE DateTimeAdd("yyyy", 1, c["DateTimeOffset"])
FROM root c
""");
                });
        }
    }

    public override async Task DateTimeOffset_AddMonths(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Fixture.NoSyncTest(
                async, async a =>
                {
                    // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
                    await Assert.ThrowsAsync<EqualException>(() => base.DateTimeOffset_AddMonths(a));

                    AssertSql(
                        """
SELECT VALUE DateTimeAdd("mm", 1, c["DateTimeOffset"])
FROM root c
""");
                });
        }
    }

    public override async Task DateTimeOffset_AddDays(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Fixture.NoSyncTest(
                async, async a =>
                {
                    // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
                    await Assert.ThrowsAsync<EqualException>(() => base.DateTimeOffset_AddSeconds(a));

                    AssertSql(
                        """
SELECT VALUE DateTimeAdd("ss", 1.0, c["DateTimeOffset"])
FROM root c
""");
                });
        }
    }

    public override async Task DateTimeOffset_AddHours(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Fixture.NoSyncTest(
                async, async a =>
                {
                    // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
                    await Assert.ThrowsAsync<EqualException>(() => base.DateTimeOffset_AddHours(a));

                    AssertSql(
                        """
SELECT VALUE DateTimeAdd("hh", 1.0, c["DateTimeOffset"])
FROM root c
""");
                });
        }
    }

    public override async Task DateTimeOffset_AddMinutes(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Fixture.NoSyncTest(
                async, async a =>
                {
                    // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
                    await Assert.ThrowsAsync<EqualException>(() => base.DateTimeOffset_AddMinutes(a));

                    AssertSql(
                        """
SELECT VALUE DateTimeAdd("mi", 1.0, c["DateTimeOffset"])
FROM root c
""");
                });
        }
    }

    public override async Task DateTimeOffset_AddSeconds(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Fixture.NoSyncTest(
                async, async a =>
                {
                    // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
                    await Assert.ThrowsAsync<EqualException>(() => base.DateTimeOffset_AddSeconds(a));

                    AssertSql(
                        """
SELECT VALUE DateTimeAdd("ss", 1.0, c["DateTimeOffset"])
FROM root c
""");
                });
        }
    }

    public override async Task DateTimeOffset_AddMilliseconds(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Fixture.NoSyncTest(
                async, async a =>
                {
                    // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
                    await Assert.ThrowsAsync<EqualException>(() => base.DateTimeOffset_AddMilliseconds(a));

                    AssertSql(
                        """
SELECT VALUE DateTimeAdd("ms", 300.0, c["DateTimeOffset"])
FROM root c
""");
                });
        }
    }

    public override Task DateTimeOffset_ToUnixTimeMilliseconds(bool async)
        => AssertTranslationFailed(() => base.DateTimeOffset_ToUnixTimeMilliseconds(async));

    public override Task DateTimeOffset_ToUnixTimeSecond(bool async)
        => AssertTranslationFailed(() => base.DateTimeOffset_ToUnixTimeSecond(async));

    public override Task DateTimeOffset_milliseconds_parameter_and_constant(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.DateTimeOffset_milliseconds_parameter_and_constant(a);

                AssertSql(
                    """
SELECT VALUE COUNT(1)
FROM root c
WHERE (c["DateTimeOffset"] = "1902-01-02T10:00:00.1234567+01:30")
""");
            });

    #endregion DateTimeOffset

    #region TimeSpan

    public override Task TimeSpan_Hours(bool async)
        => AssertTranslationFailed(() => base.TimeSpan_Hours(async));

    public override Task TimeSpan_Minutes(bool async)
        => AssertTranslationFailed(() => base.TimeSpan_Minutes(async));

    public override Task TimeSpan_Seconds(bool async)
        => AssertTranslationFailed(() => base.TimeSpan_Seconds(async));

    public override Task TimeSpan_Milliseconds(bool async)
        => AssertTranslationFailed(() => base.TimeSpan_Milliseconds(async));

    public override Task TimeSpan_Microseconds(bool async)
        => AssertTranslationFailed(() => base.TimeSpan_Microseconds(async));

    public override Task TimeSpan_Nanoseconds(bool async)
        => AssertTranslationFailed(() => base.TimeSpan_Nanoseconds(async));

    #endregion TimeSpan

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
