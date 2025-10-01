// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class MathTranslationsCosmosTest : MathTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public MathTranslationsCosmosTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Abs_decimal()
    {
        await base.Abs_decimal();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ABS(c["Decimal"]) = 9.5)
""");
    }

    public override async Task Abs_int()
    {
        await base.Abs_int();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ABS(c["Int"]) = 9)
""");
    }

    public override async Task Abs_double()
    {
        await base.Abs_double();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ABS(c["Double"]) = 9.5)
""");
    }

    public override async Task Abs_float()
    {
        await base.Abs_float();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ABS(c["Float"]) = 9.5)
""");
    }

    public override async Task Ceiling()
    {
        await base.Ceiling_float();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (CEILING(c["Float"]) = 9.0)
""");
    }

    public override async Task Ceiling_float()
    {
        await base.Ceiling_float();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (CEILING(c["Float"]) = 9.0)
""");
    }

    public override async Task Floor_decimal()
    {
        await base.Floor_decimal();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (FLOOR(c["Decimal"]) = 8.0)
""");
    }

    public override async Task Floor_double()
    {
        await base.Floor_double();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (FLOOR(c["Double"]) = 8.0)
""");
    }

    public override async Task Floor_float()
    {
        await base.Floor_float();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (FLOOR(c["Float"]) = 8.0)
""");
    }

    public override async Task Exp()
    {
        await base.Exp();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (EXP(c["Double"]) > 1.0)
""");
    }

    public override async Task Exp_float()
    {
        await base.Exp_float();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (EXP(c["Float"]) > 1.0)
""");
    }

    public override async Task Power()
    {
        // Convert node. Issue #25120.
        await AssertTranslationFailed(() => base.Power());

        AssertSql();
    }

    public override async Task Power_float()
    {
        await base.Power_float();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((POWER(c["Float"], 2.0) > 73.0) AND (POWER(c["Float"], 2.0) < 74.0))
""");
    }

    public override async Task Round_decimal()
    {
        await base.Round_decimal();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ROUND(c["Decimal"]) = 9.0)
""",
            //
            """
SELECT VALUE ROUND(c["Decimal"])
FROM root c
""");
    }

    public override async Task Round_double()
    {
        await base.Round_double();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ROUND(c["Double"]) = 9.0)
""",
            //
            """
SELECT VALUE ROUND(c["Double"])
FROM root c
""");
    }

    public override async Task Round_float()
    {
        await base.Round_float();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ROUND(c["Float"]) = 9.0)
""",
            //
            """
SELECT VALUE ROUND(c["Float"])
FROM root c
""");
    }

    public override async Task Round_with_digits_decimal()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Round_with_digits_decimal());

        AssertSql();
    }

    public override async Task Round_with_digits_double()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Round_with_digits_decimal());

        AssertSql();
    }

    public override async Task Round_with_digits_float()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Round_with_digits_decimal());

        AssertSql();
    }

    public override async Task Truncate_decimal()
    {
        await base.Truncate_decimal();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (TRUNC(c["Decimal"]) = 8.0)
""",
            //
            """
SELECT VALUE TRUNC(c["Decimal"])
FROM root c
""");
    }

    public override async Task Truncate_double()
    {
        await base.Truncate_double();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (TRUNC(c["Double"]) = 8.0)
""",
            //
            """
SELECT VALUE TRUNC(c["Double"])
FROM root c
""");
    }

    public override async Task Truncate_float()
    {
        await base.Truncate_float();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (TRUNC(c["Float"]) = 8.0)
""",
            //
            """
SELECT VALUE TRUNC(c["Float"])
FROM root c
""");
    }

    public override async Task Truncate_project_and_order_by_it_twice()
    {
        // Unsupported ORDER BY clause. ORDER BY item expression could not be mapped to a document path.
        await Assert.ThrowsAsync<CosmosException>(() => base.Truncate_project_and_order_by_it_twice());

        AssertSql(
            """
SELECT VALUE TRUNC(c["Double"])
FROM root c
ORDER BY TRUNC(c["Double"])
""");
    }

    public override async Task Truncate_project_and_order_by_it_twice2()
    {
        // Unsupported ORDER BY clause. ORDER BY item expression could not be mapped to a document path.
        await Assert.ThrowsAsync<CosmosException>(() => base.Truncate_project_and_order_by_it_twice2());

        AssertSql(
            """
SELECT VALUE TRUNC(c["Double"])
FROM root c
ORDER BY TRUNC(c["Double"]) DESC
""");
    }

    public override async Task Truncate_project_and_order_by_it_twice3()
    {
        // Unsupported ORDER BY clause. ORDER BY item expression could not be mapped to a document path.
        await Assert.ThrowsAsync<CosmosException>(() => base.Truncate_project_and_order_by_it_twice3());

        AssertSql(
            """
SELECT VALUE TRUNC(c["Double"])
FROM root c
ORDER BY TRUNC(c["Double"]) DESC
""");
    }

    public override async Task Log()
    {
        await base.Log();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Double"] > 0.0) AND (LOG(c["Double"]) != 0.0))
""");
    }

    public override async Task Log_float()
    {
        await base.Log_float();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Float"] > 0.0) AND (LOG(c["Float"]) != 0.0))
""");
    }

    public override async Task Log_with_newBase()
    {
        await base.Log_with_newBase();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Double"] > 0.0) AND (LOG(c["Double"], 7.0) != 0.0))
""");
    }

    public override async Task Log_with_newBase_float()
    {
        await base.Log_with_newBase_float();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Float"] > 0.0) AND (LOG(c["Float"], 7.0) != 0.0))
""");
    }

    public override async Task Log10()
    {
        await base.Log10();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Double"] > 0.0) AND (LOG10(c["Double"]) != 0.0))
""");
    }

    public override async Task Log10_float()
    {
        await base.Log10_float();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Float"] > 0.0) AND (LOG10(c["Float"]) != 0.0))
""");
    }

    public override async Task Log2()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Log2());

        AssertSql();
    }

    public override async Task Sqrt()
    {
        await base.Sqrt();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Double"] > 0.0) AND (SQRT(c["Double"]) > 0.0))
""");
    }

    public override async Task Sqrt_float()
    {
        await base.Sqrt_float();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Float"] > 0.0) AND (SQRT(c["Float"]) > 0.0))
""");
    }

    public override async Task Sign()
    {
        await base.Sign();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (SIGN(c["Double"]) > 0)
""");
    }

    public override async Task Sign_float()
    {
        await base.Sign_float();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (SIGN(c["Float"]) > 0)
""");
    }

    public override async Task Max()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Max());

        AssertSql();
    }

    public override async Task Max_nested()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Max());

        AssertSql();
    }

    public override async Task Max_nested_twice()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Max());

        AssertSql();
    }

    public override async Task Min()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Min());

        AssertSql();
    }

    public override async Task Min_nested()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Min());

        AssertSql();
    }

    public override async Task Min_nested_twice()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Min());

        AssertSql();
    }

    public override async Task Degrees()
    {
        await base.Degrees();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DEGREES(c["Double"]) > 0.0)
""");
    }

    public override async Task Degrees_float()
    {
        await base.Degrees_float();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DEGREES(c["Float"]) > 0.0)
""");
    }

    public override async Task Radians()
    {
        await base.Radians();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (RADIANS(c["Double"]) > 0.0)
""");
    }

    public override async Task Radians_float()
    {
        await base.Radians_float();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (RADIANS(c["Float"]) > 0.0)
""");
    }

    #region Trigonometry

    public override async Task Acos()
    {
        await base.Acos();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (((c["Double"] >= -1.0) AND (c["Double"] <= 1.0)) AND (ACOS(c["Double"]) > 1.0))
""");
    }

    public override async Task Acos_float()
    {
        await base.Acos_float();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (((c["Float"] >= -1.0) AND (c["Float"] <= 1.0)) AND (ACOS(c["Float"]) > 0.0))
""");
    }

    public override async Task Acosh()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Acosh());

        AssertSql();
    }

    public override async Task Asin()
    {
        await base.Asin();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (((c["Double"] >= -1.0) AND (c["Double"] <= 1.0)) AND (ASIN(c["Double"]) > -1.7976931348623157E+308))
""");
    }

    public override async Task Asin_float()
    {
        await base.Asin_float();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (((c["Float"] >= -1.0) AND (c["Float"] <= 1.0)) AND (ASIN(c["Float"]) > -1.7976931348623157E+308))
""");
    }

    public override async Task Asinh()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Asinh());

        AssertSql();
    }

    public override async Task Atan()
    {
        await base.Atan();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ATAN(c["Double"]) > 0.0)
""");
    }

    public override async Task Atan_float()
    {
        await base.Atan_float();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ATAN(c["Float"]) > 0.0)
""");
    }

    public override async Task Atanh()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Atanh());

        AssertSql();
    }

    public override async Task Atan2()
    {
        await base.Atan2();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ATN2(c["Double"], 1.0) > 0.0)
""");
    }

    public override async Task Cos()
    {
        await base.Cos();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (COS(c["Double"]) > 0.0)
""");
    }

    public override async Task Cos_float()
    {
        await base.Cos_float();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (COS(c["Float"]) > 0.0)
""");
    }

    public override async Task Cosh()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Cosh());

        AssertSql();
    }

    public override async Task Sin()
    {
        await base.Sin();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (SIN(c["Double"]) > 0.0)
""");
    }

    public override async Task Sin_float()
    {
        await base.Sin_float();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (SIN(c["Float"]) > 0.0)
""");
    }

    public override async Task Sinh()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Sinh());

        AssertSql();
    }

    public override async Task Tan()
    {
        await base.Tan();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (TAN(c["Double"]) > 0.0)
""");
    }

    public override async Task Tan_float()
    {
        await base.Tan_float();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (TAN(c["Float"]) > 0.0)
""");
    }

    public override async Task Tanh()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Tanh());

        AssertSql();
    }

    public override async Task Atan2_float()
    {
        await base.Atan2_float();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ATN2(c["Float"], 1.0) > 0.0)
""");
    }

    #endregion Trigonometry

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
