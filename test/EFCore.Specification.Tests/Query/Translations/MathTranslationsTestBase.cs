// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public abstract class MathTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Abs_decimal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(od => Math.Abs(od.Decimal) == 9.5m));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Abs_int(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Abs(b.Int) == 9));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Abs_double(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Abs(b.Double) == 9.5));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Abs_float(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Abs(b.Float) == 9.5));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Ceiling(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Ceiling(b.Double) == 9));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Ceiling_float(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Ceiling(b.Float) == 9));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Floor_decimal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Floor(b.Decimal) == 8));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Floor_double(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Floor(b.Double) == 8));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Floor_float(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Floor(b.Float) == 8));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Exp(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Exp(b.Double) > 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Exp_float(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Exp(b.Float) > 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Power(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Pow(b.Int, 2) == 64));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Power_float(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Pow(b.Float, 2) > 73 && MathF.Pow(b.Float, 2) < 74));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Round_decimal(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Round(b.Decimal) == 9));

        await AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => Math.Round(b.Decimal)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Round_double(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Round(b.Double) == 9));

        await AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => Math.Round(b.Double)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Round_float(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Round(b.Float) == 9));

        await AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => MathF.Round(b.Float)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Round_with_digits_decimal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Round(b.Decimal, 1) == 255.1m));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Round_with_digits_double(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Round(b.Double, 1) == 255.1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Round_with_digits_float(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Round(b.Float, 1) == 255.1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Truncate_decimal(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Truncate(b.Decimal) == 8));

        await AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => Math.Truncate(b.Decimal)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Truncate_double(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Truncate(b.Double) == 8));

        await AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => Math.Truncate(b.Double)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Truncate_float(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Truncate(b.Float) == 8));

        await AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => MathF.Truncate(b.Float)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Truncate_project_and_order_by_it_twice(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>()
                .Select(b => new { A = Math.Truncate(b.Double) })
                .OrderBy(r => r.A)
                // ReSharper disable once MultipleOrderBy
                .OrderBy(r => r.A),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Truncate_project_and_order_by_it_twice2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>()
                .Select(b => new { A = Math.Truncate(b.Double) })
                .OrderBy(r => r.A)
                // ReSharper disable once MultipleOrderBy
                .OrderByDescending(r => r.A),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Truncate_project_and_order_by_it_twice3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>()
                .Select(b => new { A = Math.Truncate(b.Double) })
                .OrderByDescending(r => r.A)
                .ThenBy(r => r.A),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Log(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Double > 0 && Math.Log(b.Double) != 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Log_float(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Float > 0 && MathF.Log(b.Float) != 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Log_with_newBase(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Double > 0 && Math.Log(b.Double, 7) != 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Log_with_newBase_float(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Float > 0 && MathF.Log(b.Float, 7) != 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Log10(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Double > 0 && Math.Log10(b.Double) != 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Log10_float(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Float > 0 && MathF.Log10(b.Float) != 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Log2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Double > 0 && Math.Log2(b.Double) != 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sqrt(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Double > 0 && Math.Sqrt(b.Double) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sqrt_float(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Float > 0 && MathF.Sqrt(b.Float) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sign(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Sign(b.Double) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sign_float(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Sign(b.Float) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Max(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Max(b.Int, b.Short - 3) == b.Int));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Max_nested(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>()
                .Where(b => Math.Max(b.Short - 3, Math.Max(b.Int, 1)) == b.Int));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Max_nested_twice(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>()
                .Where(b => Math.Max(Math.Max(1, Math.Max(b.Int, 2)), b.Short - 3) == b.Int));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Min(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>()
                .Where(b => Math.Min(b.Int, b.Short + 3) == b.Int));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Min_nested(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>()
                .Where(b => Math.Min(b.Short + 3, Math.Min(b.Int, 99999)) == b.Int));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Min_nested_twice(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>()
                .Where(b => Math.Min(Math.Min(99999, Math.Min(b.Int, 99998)), b.Short + 3) == b.Int));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Degrees(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => double.RadiansToDegrees(b.Double) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Degrees_float(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => float.RadiansToDegrees(b.Float) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Radians(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => double.DegreesToRadians(b.Double) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Radians_float(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => float.DegreesToRadians(b.Float) > 0));

    #region Trigonometry

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Acos(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Double >= -1 && b.Double <= 1 && Math.Acos(b.Double) > 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Acos_float(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Float >= -1 && b.Float <= 1 && MathF.Acos(b.Float) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Acosh(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Acosh(b.Double + 1) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Asin(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Double >= -1 && b.Double <= 1 && Math.Asin(b.Double) > double.MinValue));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Asin_float(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Float >= -1 && b.Float <= 1 && MathF.Asin(b.Float) > double.MinValue));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Asinh(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Asinh(b.Double) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Atan(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Atan(b.Double) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Atan_float(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Atan(b.Float) > 0));


    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Atanh(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Atanh(b.Double) > double.MinValue));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Atan2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Atan2(b.Double, 1) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Atan2_float(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Atan2(b.Float, 1) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Cos(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Cos(b.Double) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Cos_float(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Cos(b.Float) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Cosh(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Cosh(b.Double) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sin(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Sin(b.Double) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sin_float(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Sin(b.Float) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sinh(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Sinh(b.Double) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Tan(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Tan(b.Double) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Tan_float(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Tan(b.Float) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Tanh(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => Math.Tanh(b.Double) > 0));

    #endregion Trigonometry
}
