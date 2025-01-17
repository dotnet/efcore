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

    public override Task Abs_decimal(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Abs_decimal(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (ABS(c["Decimal"]) = 9.5)
""");
            });

    public override Task Abs_int(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Abs_int(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (ABS(c["Int"]) = 9)
""");
            });

    public override Task Abs_double(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Abs_double(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (ABS(c["Double"]) = 9.5)
""");
            });

    public override Task Abs_float(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Abs_float(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (ABS(c["Float"]) = 9.5)
""");
            });

    public override Task Ceiling(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Ceiling_float(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (CEILING(c["Float"]) = 9.0)
""");
            });

    public override Task Ceiling_float(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Ceiling_float(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (CEILING(c["Float"]) = 9.0)
""");
            });

    public override Task Floor_decimal(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Floor_decimal(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (FLOOR(c["Decimal"]) = 8.0)
""");
            });

    public override Task Floor_double(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Floor_double(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (FLOOR(c["Double"]) = 8.0)
""");
            });

    public override Task Floor_float(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Floor_float(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (FLOOR(c["Float"]) = 8.0)
""");
            });

    public override Task Exp(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Exp(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (EXP(c["Double"]) > 1.0)
""");
            });

    public override Task Exp_float(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Exp_float(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (EXP(c["Float"]) > 1.0)
""");
            });

    public override async Task Power(bool async)
    {
        // Convert node. Issue #25120.
        await AssertTranslationFailed(() => base.Power(async));

        AssertSql();
    }

    public override Task Power_float(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Power_float(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((POWER(c["Float"], 2.0) > 73.0) AND (POWER(c["Float"], 2.0) < 74.0))
""");
            });

    public override Task Round_decimal(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Round_decimal(a);

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
            });

    public override Task Round_double(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Round_double(a);

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
            });

    public override Task Round_float(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Round_float(a);

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
            });

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

    public override Task Truncate_decimal(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Truncate_decimal(a);

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
            });

    public override Task Truncate_double(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Truncate_double(a);

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
            });

    public override Task Truncate_float(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Truncate_float(a);

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
            });

    public override async Task Truncate_project_and_order_by_it_twice(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Fixture.NoSyncTest(
                async, async a =>
                {
                    // Unsupported ORDER BY clause. ORDER BY item expression could not be mapped to a document path.
                    await Assert.ThrowsAsync<CosmosException>(() => base.Truncate_project_and_order_by_it_twice(a));

                    AssertSql(
                        """
SELECT VALUE TRUNC(c["Double"])
FROM root c
ORDER BY TRUNC(c["Double"])
""");
                });
        }
    }

    public override async Task Truncate_project_and_order_by_it_twice2(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Fixture.NoSyncTest(
                async, async a =>
                {
                    // Unsupported ORDER BY clause. ORDER BY item expression could not be mapped to a document path.
                    await Assert.ThrowsAsync<CosmosException>(() => base.Truncate_project_and_order_by_it_twice2(a));

                    AssertSql(
                        """
SELECT VALUE TRUNC(c["Double"])
FROM root c
ORDER BY TRUNC(c["Double"]) DESC
""");
                });
        }
    }

    public override async Task Truncate_project_and_order_by_it_twice3(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Fixture.NoSyncTest(
                async, async a =>
                {
                    // Unsupported ORDER BY clause. ORDER BY item expression could not be mapped to a document path.
                    await Assert.ThrowsAsync<CosmosException>(() => base.Truncate_project_and_order_by_it_twice3(a));

                    AssertSql(
                        """
SELECT VALUE TRUNC(c["Double"])
FROM root c
ORDER BY TRUNC(c["Double"]) DESC
""");
                });
        }
    }

    public override Task Log(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Log(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Double"] > 0.0) AND (LOG(c["Double"]) != 0.0))
""");
            });

    public override Task Log_float(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Log_float(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Float"] > 0.0) AND (LOG(c["Float"]) != 0.0))
""");
            });

    public override Task Log_with_newBase(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Log_with_newBase(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Double"] > 0.0) AND (LOG(c["Double"], 7.0) != 0.0))
""");
            });

    public override Task Log_with_newBase_float(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Log_with_newBase_float(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Float"] > 0.0) AND (LOG(c["Float"], 7.0) != 0.0))
""");
            });

    public override Task Log10(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Log10(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Double"] > 0.0) AND (LOG10(c["Double"]) != 0.0))
""");
            });

    public override Task Log10_float(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Log10_float(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Float"] > 0.0) AND (LOG10(c["Float"]) != 0.0))
""");
            });

    public override async Task Log2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Log2(async));

        AssertSql();
    }

    public override Task Sqrt(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Sqrt(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Double"] > 0.0) AND (SQRT(c["Double"]) > 0.0))
""");
            });

    public override Task Sqrt_float(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Sqrt_float(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Float"] > 0.0) AND (SQRT(c["Float"]) > 0.0))
""");
            });


    public override Task Sign(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Sign(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (SIGN(c["Double"]) > 0)
""");
            });

    public override Task Sign_float(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Sign_float(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (SIGN(c["Float"]) > 0)
""");
            });

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

    public override Task Degrees(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Degrees(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (DEGREES(c["Double"]) > 0.0)
""");
            });

    public override Task Degrees_float(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Degrees_float(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (DEGREES(c["Float"]) > 0.0)
""");
            });

    public override Task Radians(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Radians(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (RADIANS(c["Double"]) > 0.0)
""");
            });

    public override Task Radians_float(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Radians_float(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (RADIANS(c["Float"]) > 0.0)
""");
            });

    #region Trigonometry

    public override Task Acos(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Acos(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (((c["Double"] >= -1.0) AND (c["Double"] <= 1.0)) AND (ACOS(c["Double"]) > 1.0))
""");
            });

    public override Task Acos_float(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Acos_float(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (((c["Float"] >= -1.0) AND (c["Float"] <= 1.0)) AND (ACOS(c["Float"]) > 0.0))
""");
            });

    public override async Task Acosh(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Acosh(async));

        AssertSql();
    }

    public override Task Asin(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Asin(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (((c["Double"] >= -1.0) AND (c["Double"] <= 1.0)) AND (ASIN(c["Double"]) > -1.7976931348623157E+308))
""");
            });

    public override Task Asin_float(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Asin_float(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (((c["Float"] >= -1.0) AND (c["Float"] <= 1.0)) AND (ASIN(c["Float"]) > -1.7976931348623157E+308))
""");
            });

    public override async Task Asinh(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Asinh(async));

        AssertSql();
    }

    public override Task Atan(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Atan(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (ATAN(c["Double"]) > 0.0)
""");
            });

    public override Task Atan_float(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Atan_float(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (ATAN(c["Float"]) > 0.0)
""");
            });

    public override async Task Atanh(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Atanh(async));

        AssertSql();
    }

    public override Task Atan2(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Atan2(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (ATN2(c["Double"], 1.0) > 0.0)
""");
            });

    public override Task Cos(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Cos(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (COS(c["Double"]) > 0.0)
""");
            });

    public override Task Cos_float(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Cos_float(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (COS(c["Float"]) > 0.0)
""");
            });

    public override async Task Cosh(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Cosh(async));

        AssertSql();
    }

    public override Task Sin(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Sin(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (SIN(c["Double"]) > 0.0)
""");
            });

    public override Task Sin_float(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Sin_float(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (SIN(c["Float"]) > 0.0)
""");
            });

    public override async Task Sinh(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Sinh(async));

        AssertSql();
    }

    public override Task Tan(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Tan(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (TAN(c["Double"]) > 0.0)
""");
            });

    public override Task Tan_float(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Tan_float(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (TAN(c["Float"]) > 0.0)
""");
            });

    public override async Task Tanh(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Tanh(async));

        AssertSql();
    }

    public override Task Atan2_float(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Atan2_float(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (ATN2(c["Float"], 1.0) > 0.0)
""");
            });

    #endregion Trigonometry

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
