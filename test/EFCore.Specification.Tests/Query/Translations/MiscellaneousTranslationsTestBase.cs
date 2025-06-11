// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public abstract class MiscellaneousTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    #region Random

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Random_on_EF_Functions(bool async)
        => AssertCount(
            async,
            ss => ss.Set<BasicTypesEntity>(),
            ss => ss.Set<BasicTypesEntity>(),
            ss => EF.Functions.Random() >= 0 && EF.Functions.Random() < 1,
            c => true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Random_Shared_Next_with_no_args(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.Int < (Random.Shared.Next() - 2147483647)),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Random_Shared_Next_with_one_arg(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.Int > Random.Shared.Next(5) - 2147483647));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Random_Shared_Next_with_two_args(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.Int > Random.Shared.Next(0, 10) - 2147483647));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Random_new_Next_with_no_args(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.Int > new Random(15).Next() - 2147483647));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Random_new_Next_with_one_arg(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.Int > new Random(15).Next(5) - 2147483647));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Random_new_Next_with_two_args(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.Int > new Random(15).Next(0, 10) - 2147483647));

    #endregion Random

    #region Convert

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Convert_ToBoolean(bool async)
    {
        var convertMethods = new List<Expression<Func<BasicTypesEntity, bool>>>
        {
            o => Convert.ToBoolean(o.Bool),
            o => Convert.ToBoolean(o.Byte),
            o => Convert.ToBoolean(o.Decimal),
            o => Convert.ToBoolean(o.Double),
            o => Convert.ToBoolean(o.Float),
            o => Convert.ToBoolean(o.Short),
            o => Convert.ToBoolean(o.Int),
            o => Convert.ToBoolean(o.Long),
            o => Convert.ToBoolean((object)o.Int)
        };

        foreach (var convertMethod in convertMethods)
        {
            await AssertQuery(async, ss => ss.Set<BasicTypesEntity>().Where(convertMethod));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Convert_ToByte(bool async)
    {
        var convertMethods = new List<Expression<Func<BasicTypesEntity, bool>>>
        {
            o => Convert.ToByte(o.Bool) == 1,
            o => Convert.ToByte(o.Byte) == 8,
            o => o.Decimal >= 0 && o.Decimal <= 255 && Convert.ToByte(o.Decimal) == 12,
            o => o.Double >= 0 && o.Double <= 255 && Convert.ToByte(o.Double) == 12,
            o => o.Float >= 0 && o.Float <= 255 && Convert.ToByte(o.Float) == 12,
            o => o.Short >= 0 && o.Short <= 255 && Convert.ToByte(o.Short) == 12,
            o => o.Int >= 0 && o.Int <= 255 && Convert.ToByte(o.Int) == 12,
            o => o.Long >= 0 && o.Long <= 255 && Convert.ToByte(o.Long) == 12,
            o => o.Int >= 0 && o.Int <= 255 && Convert.ToByte(Convert.ToString(o.Int)) == 12,
            o => o.Int >= 0 && o.Int <= 255 && Convert.ToByte((object)o.Int) == 12
        };

        foreach (var convertMethod in convertMethods)
        {
            await AssertQuery(async, ss => ss.Set<BasicTypesEntity>().Where(convertMethod));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Convert_ToDecimal(bool async)
    {
        var convertMethods = new List<Expression<Func<BasicTypesEntity, bool>>>
        {
            o => Convert.ToDecimal(o.Bool) == 1,
            o => Convert.ToDecimal(o.Byte) == 8,
            o => Convert.ToDecimal(o.Decimal) == 8.6m,
            o => Convert.ToDecimal(o.Double) == 8.6m,
            o => Convert.ToDecimal(o.Float) == 8.6m,
            o => Convert.ToDecimal(o.Short) == 8,
            o => Convert.ToDecimal(o.Int) == 8,
            o => Convert.ToDecimal(o.Long) == 8,
            o => Convert.ToDecimal(Convert.ToString(o.Int)) == 8,
            o => Convert.ToDecimal((object)o.Int) == 8
        };

        foreach (var convertMethod in convertMethods)
        {
            await AssertQuery(async, ss => ss.Set<BasicTypesEntity>().Where(convertMethod));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Convert_ToDouble(bool async)
    {
        var convertMethods = new List<Expression<Func<BasicTypesEntity, bool>>>
        {
            o => Convert.ToDouble(o.Bool) == 1,
            o => Convert.ToDouble(o.Byte) == 8,
            o => Convert.ToDouble(o.Decimal) == 8.6d,
            o => Convert.ToDouble(o.Double) > 8d && Convert.ToDouble(o.Double) < 9d,
            o => Convert.ToDouble(o.Float) > 8d && Convert.ToDouble(o.Float) < 9d,
            o => Convert.ToDouble(o.Short) == 8,
            o => Convert.ToDouble(o.Int) == 8,
            o => Convert.ToDouble(o.Long) == 8,
            o => Convert.ToDouble(Convert.ToString(o.Int)) == 8,
            o => Convert.ToDouble((object)o.Int) == 8
        };

        foreach (var convertMethod in convertMethods)
        {
            await AssertQuery(async, ss => ss.Set<BasicTypesEntity>().Where(convertMethod));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Convert_ToInt16(bool async)
    {
        var convertMethods = new List<Expression<Func<BasicTypesEntity, bool>>>
        {
            o => Convert.ToInt16(o.Bool) == 1,
            o => Convert.ToInt16(o.Byte) == 12,
            o => Convert.ToInt16(o.Decimal) == 12,
            o => Convert.ToInt16(o.Double) == 12,
            o => Convert.ToInt16(o.Float) == 12,
            o => Convert.ToInt16(o.Short) == 12,
            o => Convert.ToInt16(o.Int) == 12,
            o => Convert.ToInt16(o.Long) == 12,
            o => Convert.ToInt16(Convert.ToString(o.Int)) == 12,
            o => Convert.ToInt16((object)o.Int) == 12
        };

        foreach (var convertMethod in convertMethods)
        {
            await AssertQuery(async, ss => ss.Set<BasicTypesEntity>().Where(convertMethod));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Convert_ToInt32(bool async)
    {
        var convertMethods = new List<Expression<Func<BasicTypesEntity, bool>>>
        {
            o => Convert.ToInt32(o.Bool) == 1,
            o => Convert.ToInt32(o.Byte) == 12,
            o => Convert.ToInt32(o.Decimal) == 12,
            o => Convert.ToInt32(o.Double) == 12,
            o => Convert.ToInt32(o.Float) == 12,
            o => Convert.ToInt32(o.Short) == 12,
            o => Convert.ToInt32(o.Int) == 12,
            o => Convert.ToInt32(o.Long) == 12,
            o => Convert.ToInt32(Convert.ToString(o.Int)) == 12,
            o => Convert.ToInt32((object)o.Int) == 12
        };

        foreach (var convertMethod in convertMethods)
        {
            await AssertQuery(async, ss => ss.Set<BasicTypesEntity>().Where(convertMethod));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Convert_ToInt64(bool async)
    {
        var convertMethods = new List<Expression<Func<BasicTypesEntity, bool>>>
        {
            o => Convert.ToInt64(o.Bool) == 1,
            o => Convert.ToInt64(o.Byte) == 12,
            o => Convert.ToInt64(o.Decimal) == 12,
            o => Convert.ToInt64(o.Double) == 12,
            o => Convert.ToInt64(o.Float) == 12,
            o => Convert.ToInt64(o.Short) == 12,
            o => Convert.ToInt64(o.Int) == 12,
            o => Convert.ToInt64(o.Long) == 12,
            o => Convert.ToInt64(Convert.ToString(o.Int)) == 12,
            o => Convert.ToInt64((object)o.Int) == 12
        };

        foreach (var convertMethod in convertMethods)
        {
            await AssertQuery(async, ss => ss.Set<BasicTypesEntity>().Where(convertMethod));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Convert_ToString(bool async)
    {
        // Actual convert-to-string behavior varies across databases for most types and cannot be asserted upon here
        // (e.g. boolean converts to 1/0 on SQL Server, true/false on PG).
        var convertMethods = new List<Expression<Func<BasicTypesEntity, bool>>>
        {
            o => Convert.ToString(o.Bool) != "",
            o => Convert.ToString(o.Byte) == "8",
            o => Convert.ToString(o.Decimal) != "",
            o => Convert.ToString(o.Double) != "",
            o => Convert.ToString(o.Float) != "",
            o => Convert.ToString(o.Short) == "8",
            o => Convert.ToString(o.Int) == "8",
            o => Convert.ToString(o.Long) == "8",
            o => Convert.ToString(o.String) == "Seattle",
            o => Convert.ToString((object)o.String) == "Seattle",
            o => Convert.ToString(o.DateTime).Contains("1998")
        };

        foreach (var convertMethod in convertMethods)
        {
            await AssertQuery(async, ss => ss.Set<BasicTypesEntity>().Where(convertMethod));
        }
    }

    #endregion Convert

    #region Compare

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Int_Compare_to_simple_zero(bool async)
    {
        var orderId = 8;

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.Int.CompareTo(orderId) == 0));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => 0 != c.Int.CompareTo(orderId)));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.Int.CompareTo(orderId) > 0));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => 0 >= c.Int.CompareTo(orderId)));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => 0 < c.Int.CompareTo(orderId)));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.Int.CompareTo(orderId) <= 0));
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public virtual async Task DateTime_Compare_to_simple_zero(bool async, bool compareTo)
    {
        var dateTime = new DateTime(1998, 5, 4, 15, 30, 10);

        if (compareTo)
        {
            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => c.DateTime.CompareTo(dateTime) == 0));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 != c.DateTime.CompareTo(dateTime)));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => c.DateTime.CompareTo(dateTime) > 0));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 >= c.DateTime.CompareTo(dateTime)));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 < c.DateTime.CompareTo(dateTime)));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => c.DateTime.CompareTo(dateTime) <= 0));
        }
        else
        {
            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => DateTime.Compare(c.DateTime, dateTime) == 0));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 != DateTime.Compare(c.DateTime, dateTime)));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => DateTime.Compare(c.DateTime, dateTime) > 0));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 >= DateTime.Compare(c.DateTime, dateTime)));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 < DateTime.Compare(c.DateTime, dateTime)));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => DateTime.Compare(c.DateTime, dateTime) <= 0));
        }
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public virtual async Task TimeSpan_Compare_to_simple_zero(bool async, bool compareTo)
    {
        var timeSpan = new TimeSpan(1, 2, 3);

        if (compareTo)
        {
            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => c.TimeSpan.CompareTo(timeSpan) == 0));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 != c.TimeSpan.CompareTo(timeSpan)));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => c.TimeSpan.CompareTo(timeSpan) > 0));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 >= c.TimeSpan.CompareTo(timeSpan)));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 < c.TimeSpan.CompareTo(timeSpan)));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => c.TimeSpan.CompareTo(timeSpan) <= 0));
        }
        else
        {
            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => TimeSpan.Compare(c.TimeSpan, timeSpan) == 0));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 != TimeSpan.Compare(c.TimeSpan, timeSpan)));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => TimeSpan.Compare(c.TimeSpan, timeSpan) > 0));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 >= TimeSpan.Compare(c.TimeSpan, timeSpan)));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 < TimeSpan.Compare(c.TimeSpan, timeSpan)));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => TimeSpan.Compare(c.TimeSpan, timeSpan) <= 0));
        }
    }

    #endregion
}
