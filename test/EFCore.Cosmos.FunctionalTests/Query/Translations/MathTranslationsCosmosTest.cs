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

    public override async Task Abs_decimal(bool async)
    {
        await base.Abs_decimal(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ABS(c["Decimal"]) = 9.5)
""");
    }

    public override async Task Abs_int(bool async)
    {
        await base.Abs_int(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ABS(c["Int"]) = 9)
""");
    }

    public override async Task Abs_double(bool async)
    {
        await base.Abs_double(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ABS(c["Double"]) = 9.5)
""");
    }

    public override async Task Abs_float(bool async)
    {
        await base.Abs_float(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ABS(c["Float"]) = 9.5)
""");
    }

    public override async Task Ceiling(bool async)
    {
        await base.Ceiling_float(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (CEILING(c["Float"]) = 9.0)
""");
    }

    public override async Task Ceiling_float(bool async)
    {
        await base.Ceiling_float(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (CEILING(c["Float"]) = 9.0)
""");
    }

    public override async Task Floor_decimal(bool async)
    {
        await base.Floor_decimal(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (FLOOR(c["Decimal"]) = 8.0)
""");
    }

    public override async Task Floor_double(bool async)
    {
        await base.Floor_double(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (FLOOR(c["Double"]) = 8.0)
""");
    }

    public override async Task Floor_float(bool async)
    {
        await base.Floor_float(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (FLOOR(c["Float"]) = 8.0)
""");
    }

    public override async Task Exp(bool async)
    {
        await base.Exp(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (EXP(c["Double"]) > 1.0)
""");
    }

    public override async Task Exp_float(bool async)
    {
        await base.Exp_float(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (EXP(c["Float"]) > 1.0)
""");
    }

    public override async Task Power(bool async)
    {
        // Convert node. Issue #25120.
        await AssertTranslationFailed(() => base.Power(async));

        AssertSql();
    }

    public override async Task Power_float(bool async)
    {
        await base.Power_float(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((POWER(c["Float"], 2.0) > 73.0) AND (POWER(c["Float"], 2.0) < 74.0))
""");
    }

    public override async Task Round_decimal(bool async)
    {
        await base.Round_decimal(async);

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

    public override async Task Round_double(bool async)
    {
        await base.Round_double(async);

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

    public override async Task Round_float(bool async)
    {
        await base.Round_float(async);

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

    public override async Task Round_with_digits_decimal(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Round_with_digits_decimal(async));

        AssertSql();
    }

    public override async Task Round_with_digits_double(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Round_with_digits_decimal(async));

        AssertSql();
    }

    public override async Task Round_with_digits_float(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Round_with_digits_decimal(async));

        AssertSql();
    }

    public override async Task Truncate_decimal(bool async)
    {
        await base.Truncate_decimal(async);

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

    public override async Task Truncate_double(bool async)
    {
        await base.Truncate_double(async);

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

    public override async Task Truncate_float(bool async)
    {
        await base.Truncate_float(async);

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

    public override async Task Truncate_project_and_order_by_it_twice(bool async)
    {
        // Unsupported ORDER BY clause. ORDER BY item expression could not be mapped to a document path.
        await Assert.ThrowsAsync<CosmosException>(() => base.Truncate_project_and_order_by_it_twice(async));

        AssertSql(
            """
SELECT VALUE TRUNC(c["Double"])
FROM root c
ORDER BY TRUNC(c["Double"])
""");
    }

    public override async Task Truncate_project_and_order_by_it_twice2(bool async)
    {
        // Unsupported ORDER BY clause. ORDER BY item expression could not be mapped to a document path.
        await Assert.ThrowsAsync<CosmosException>(() => base.Truncate_project_and_order_by_it_twice2(async));

        AssertSql(
            """
SELECT VALUE TRUNC(c["Double"])
FROM root c
ORDER BY TRUNC(c["Double"]) DESC
""");
    }

    public override async Task Truncate_project_and_order_by_it_twice3(bool async)
    {
        // Unsupported ORDER BY clause. ORDER BY item expression could not be mapped to a document path.
        await Assert.ThrowsAsync<CosmosException>(() => base.Truncate_project_and_order_by_it_twice3(async));

        AssertSql(
            """
SELECT VALUE TRUNC(c["Double"])
FROM root c
ORDER BY TRUNC(c["Double"]) DESC
""");
    }

    public override async Task Log(bool async)
    {
        await base.Log(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Double"] > 0.0) AND (LOG(c["Double"]) != 0.0))
""");
    }

    public override async Task Log_float(bool async)
    {
        await base.Log_float(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Float"] > 0.0) AND (LOG(c["Float"]) != 0.0))
""");
    }

    public override async Task Log_with_newBase(bool async)
    {
        await base.Log_with_newBase(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Double"] > 0.0) AND (LOG(c["Double"], 7.0) != 0.0))
""");
    }

    public override async Task Log_with_newBase_float(bool async)
    {
        await base.Log_with_newBase_float(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Float"] > 0.0) AND (LOG(c["Float"], 7.0) != 0.0))
""");
    }

    public override async Task Log10(bool async)
    {
        await base.Log10(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Double"] > 0.0) AND (LOG10(c["Double"]) != 0.0))
""");
    }

    public override async Task Log10_float(bool async)
    {
        await base.Log10_float(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Float"] > 0.0) AND (LOG10(c["Float"]) != 0.0))
""");
    }

    public override async Task Log2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Log2(async));

        AssertSql();
    }

    public override async Task Sqrt(bool async)
    {
        await base.Sqrt(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Double"] > 0.0) AND (SQRT(c["Double"]) > 0.0))
""");
    }

    public override async Task Sqrt_float(bool async)
    {
        await base.Sqrt_float(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Float"] > 0.0) AND (SQRT(c["Float"]) > 0.0))
""");
    }

    public override async Task Sign(bool async)
    {
        await base.Sign(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (SIGN(c["Double"]) > 0)
""");
    }

    public override async Task Sign_float(bool async)
    {
        await base.Sign_float(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (SIGN(c["Float"]) > 0)
""");
    }

    public override async Task Max(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Max(async));

        AssertSql();
    }

    public override async Task Max_nested(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Max(async));

        AssertSql();
    }

    public override async Task Max_nested_twice(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Max(async));

        AssertSql();
    }

    public override async Task Min(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Min(async));

        AssertSql();
    }

    public override async Task Min_nested(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Min(async));

        AssertSql();
    }

    public override async Task Min_nested_twice(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Min(async));

        AssertSql();
    }

    public override async Task Degrees(bool async)
    {
        await base.Degrees(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DEGREES(c["Double"]) > 0.0)
""");
    }

    public override async Task Degrees_float(bool async)
    {
        await base.Degrees_float(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DEGREES(c["Float"]) > 0.0)
""");
    }

    public override async Task Radians(bool async)
    {
        await base.Radians(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (RADIANS(c["Double"]) > 0.0)
""");
    }

    public override async Task Radians_float(bool async)
    {
        await base.Radians_float(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (RADIANS(c["Float"]) > 0.0)
""");
    }

    #region Trigonometry

    public override async Task Acos(bool async)
    {
        await base.Acos(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (((c["Double"] >= -1.0) AND (c["Double"] <= 1.0)) AND (ACOS(c["Double"]) > 1.0))
""");
    }

    public override async Task Acos_float(bool async)
    {
        await base.Acos_float(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (((c["Float"] >= -1.0) AND (c["Float"] <= 1.0)) AND (ACOS(c["Float"]) > 0.0))
""");
    }

    public override async Task Acosh(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Acosh(async));

        AssertSql();
    }

    public override async Task Asin(bool async)
    {
        await base.Asin(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (((c["Double"] >= -1.0) AND (c["Double"] <= 1.0)) AND (ASIN(c["Double"]) > -1.7976931348623157E+308))
""");
    }

    public override async Task Asin_float(bool async)
    {
        await base.Asin_float(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (((c["Float"] >= -1.0) AND (c["Float"] <= 1.0)) AND (ASIN(c["Float"]) > -1.7976931348623157E+308))
""");
    }

    public override async Task Asinh(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Asinh(async));

        AssertSql();
    }

    public override async Task Atan(bool async)
    {
        await base.Atan(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ATAN(c["Double"]) > 0.0)
""");
    }

    public override async Task Atan_float(bool async)
    {
        await base.Atan_float(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ATAN(c["Float"]) > 0.0)
""");
    }

    public override async Task Atanh(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Atanh(async));

        AssertSql();
    }

    public override async Task Atan2(bool async)
    {
        await base.Atan2(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ATN2(c["Double"], 1.0) > 0.0)
""");
    }

    public override async Task Cos(bool async)
    {
        await base.Cos(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (COS(c["Double"]) > 0.0)
""");
    }

    public override async Task Cos_float(bool async)
    {
        await base.Cos_float(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (COS(c["Float"]) > 0.0)
""");
    }

    public override async Task Cosh(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Cosh(async));

        AssertSql();
    }

    public override async Task Sin(bool async)
    {
        await base.Sin(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (SIN(c["Double"]) > 0.0)
""");
    }

    public override async Task Sin_float(bool async)
    {
        await base.Sin_float(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (SIN(c["Float"]) > 0.0)
""");
    }

    public override async Task Sinh(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Sinh(async));

        AssertSql();
    }

    public override async Task Tan(bool async)
    {
        await base.Tan(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (TAN(c["Double"]) > 0.0)
""");
    }

    public override async Task Tan_float(bool async)
    {
        await base.Tan_float(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (TAN(c["Float"]) > 0.0)
""");
    }

    public override async Task Tanh(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Tanh(async));

        AssertSql();
    }

    public override async Task Atan2_float(bool async)
    {
        await base.Atan2_float(async);

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
