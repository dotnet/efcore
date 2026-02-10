// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public abstract class MathTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [ConditionalFact]
    public virtual Task Abs_decimal()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(od => Math.Abs(od.Decimal) == 9.5m));

    [ConditionalFact]
    public virtual Task Abs_int()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Abs(b.Int) == 9));

    [ConditionalFact]
    public virtual Task Abs_double()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Abs(b.Double) == 9.5));

    [ConditionalFact]
    public virtual Task Abs_float()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Abs(b.Float) == 9.5));

    [ConditionalFact]
    public virtual Task Ceiling()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Ceiling(b.Double) == 9));

    [ConditionalFact]
    public virtual Task Ceiling_float()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Ceiling(b.Float) == 9));

    [ConditionalFact]
    public virtual Task Floor_decimal()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Floor(b.Decimal) == 8));

    [ConditionalFact]
    public virtual Task Floor_double()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Floor(b.Double) == 8));

    [ConditionalFact]
    public virtual Task Floor_float()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Floor(b.Float) == 8));

    [ConditionalFact]
    public virtual Task Exp()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Exp(b.Double) > 1));

    [ConditionalFact]
    public virtual Task Exp_float()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Exp(b.Float) > 1));

    [ConditionalFact]
    public virtual Task Power()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Pow(b.Int, 2) == 64));

    [ConditionalFact]
    public virtual Task Power_float()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Pow(b.Float, 2) > 73 && MathF.Pow(b.Float, 2) < 74));

    [ConditionalFact]
    public virtual async Task Round_decimal()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Round(b.Decimal) == 9));

        await AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => Math.Round(b.Decimal)));
    }

    [ConditionalFact]
    public virtual async Task Round_double()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Round(b.Double) == 9));

        await AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => Math.Round(b.Double)));
    }

    [ConditionalFact]
    public virtual async Task Round_float()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Round(b.Float) == 9));

        await AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => MathF.Round(b.Float)));
    }

    [ConditionalFact]
    public virtual Task Round_with_digits_decimal()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Round(b.Decimal, 1) == 255.1m));

    [ConditionalFact]
    public virtual Task Round_with_digits_double()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Round(b.Double, 1) == 255.1));

    [ConditionalFact]
    public virtual Task Round_with_digits_float()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Round(b.Float, 1) == 255.1));

    [ConditionalFact]
    public virtual async Task Truncate_decimal()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Truncate(b.Decimal) == 8));

        await AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => Math.Truncate(b.Decimal)));
    }

    [ConditionalFact]
    public virtual async Task Truncate_double()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Truncate(b.Double) == 8));

        await AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => Math.Truncate(b.Double)));
    }

    [ConditionalFact]
    public virtual async Task Truncate_float()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Truncate(b.Float) == 8));

        await AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => MathF.Truncate(b.Float)));
    }

    [ConditionalFact]
    public virtual Task Truncate_project_and_order_by_it_twice()
        => AssertQuery(
            ss => ss.Set<BasicTypesEntity>()
                .Select(b => new { A = Math.Truncate(b.Double) })
                .OrderBy(r => r.A)
                // ReSharper disable once MultipleOrderBy
                .OrderBy(r => r.A),
            assertOrder: true);

    [ConditionalFact]
    public virtual Task Truncate_project_and_order_by_it_twice2()
        => AssertQuery(
            ss => ss.Set<BasicTypesEntity>()
                .Select(b => new { A = Math.Truncate(b.Double) })
                .OrderBy(r => r.A)
                // ReSharper disable once MultipleOrderBy
                .OrderByDescending(r => r.A),
            assertOrder: true);

    [ConditionalFact]
    public virtual Task Truncate_project_and_order_by_it_twice3()
        => AssertQuery(
            ss => ss.Set<BasicTypesEntity>()
                .Select(b => new { A = Math.Truncate(b.Double) })
                .OrderByDescending(r => r.A)
                .ThenBy(r => r.A),
            assertOrder: true);

    [ConditionalFact]
    public virtual Task Log()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Double > 0 && Math.Log(b.Double) != 0));

    [ConditionalFact]
    public virtual Task Log_float()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Float > 0 && MathF.Log(b.Float) != 0));

    [ConditionalFact]
    public virtual Task Log_with_newBase()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Double > 0 && Math.Log(b.Double, 7) != 0));

    [ConditionalFact]
    public virtual Task Log_with_newBase_float()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Float > 0 && MathF.Log(b.Float, 7) != 0));

    [ConditionalFact]
    public virtual Task Log10()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Double > 0 && Math.Log10(b.Double) != 0));

    [ConditionalFact]
    public virtual Task Log10_float()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Float > 0 && MathF.Log10(b.Float) != 0));

    [ConditionalFact]
    public virtual Task Log2()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Double > 0 && Math.Log2(b.Double) != 0));

    [ConditionalFact]
    public virtual Task Sqrt()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Double > 0 && Math.Sqrt(b.Double) > 0));

    [ConditionalFact]
    public virtual Task Sqrt_float()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Float > 0 && MathF.Sqrt(b.Float) > 0));

    [ConditionalFact]
    public virtual Task Sign()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Sign(b.Double) > 0));

    [ConditionalFact]
    public virtual Task Sign_float()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Sign(b.Float) > 0));

    [ConditionalFact]
    public virtual Task Max()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Max(b.Int, b.Short - 3) == b.Int));

    [ConditionalFact]
    public virtual Task Max_nested()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>()
            .Where(b => Math.Max(b.Short - 3, Math.Max(b.Int, 1)) == b.Int));

    [ConditionalFact]
    public virtual Task Max_nested_twice()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>()
            .Where(b => Math.Max(Math.Max(1, Math.Max(b.Int, 2)), b.Short - 3) == b.Int));

    [ConditionalFact]
    public virtual Task Min()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>()
            .Where(b => Math.Min(b.Int, b.Short + 3) == b.Int));

    [ConditionalFact]
    public virtual Task Min_nested()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>()
            .Where(b => Math.Min(b.Short + 3, Math.Min(b.Int, 99999)) == b.Int));

    [ConditionalFact]
    public virtual Task Min_nested_twice()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>()
            .Where(b => Math.Min(Math.Min(99999, Math.Min(b.Int, 99998)), b.Short + 3) == b.Int));

    [ConditionalFact]
    public virtual Task Degrees()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => double.RadiansToDegrees(b.Double) > 0));

    [ConditionalFact]
    public virtual Task Degrees_float()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => float.RadiansToDegrees(b.Float) > 0));

    [ConditionalFact]
    public virtual Task Radians()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => double.DegreesToRadians(b.Double) > 0));

    [ConditionalFact]
    public virtual Task Radians_float()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => float.DegreesToRadians(b.Float) > 0));

    #region Trigonometry

    [ConditionalFact]
    public virtual Task Acos()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Double >= -1 && b.Double <= 1 && Math.Acos(b.Double) > 1));

    [ConditionalFact]
    public virtual Task Acos_float()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Float >= -1 && b.Float <= 1 && MathF.Acos(b.Float) > 0));

    [ConditionalFact]
    public virtual Task Acosh()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Acosh(b.Double + 1) > 0));

    [ConditionalFact]
    public virtual Task Asin()
        => AssertQuery(ss
            => ss.Set<BasicTypesEntity>().Where(b => b.Double >= -1 && b.Double <= 1 && Math.Asin(b.Double) > double.MinValue));

    [ConditionalFact]
    public virtual Task Asin_float()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Float >= -1 && b.Float <= 1 && MathF.Asin(b.Float) > double.MinValue));

    [ConditionalFact]
    public virtual Task Asinh()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Asinh(b.Double) > 0));

    [ConditionalFact]
    public virtual Task Atan()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Atan(b.Double) > 0));

    [ConditionalFact]
    public virtual Task Atan_float()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Atan(b.Float) > 0));

    [ConditionalFact]
    public virtual Task Atanh()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Atanh(b.Double) > double.MinValue));

    [ConditionalFact]
    public virtual Task Atan2()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Atan2(b.Double, 1) > 0));

    [ConditionalFact]
    public virtual Task Atan2_float()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Atan2(b.Float, 1) > 0));

    [ConditionalFact]
    public virtual Task Cos()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Cos(b.Double) > 0));

    [ConditionalFact]
    public virtual Task Cos_float()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Cos(b.Float) > 0));

    [ConditionalFact]
    public virtual Task Cosh()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Cosh(b.Double) > 0));

    [ConditionalFact]
    public virtual Task Sin()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Sin(b.Double) > 0));

    [ConditionalFact]
    public virtual Task Sin_float()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Sin(b.Float) > 0));

    [ConditionalFact]
    public virtual Task Sinh()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Sinh(b.Double) > 0));

    [ConditionalFact]
    public virtual Task Tan()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Tan(b.Double) > 0));

    [ConditionalFact]
    public virtual Task Tan_float()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => MathF.Tan(b.Float) > 0));

    [ConditionalFact]
    public virtual Task Tanh()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => Math.Tanh(b.Double) > 0));

    #endregion Trigonometry
}
