// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindFunctionsQueryCosmosTest : NorthwindFunctionsQueryTestBase<NorthwindQueryCosmosFixture<NoopModelCustomizer>>
{
    public NorthwindFunctionsQueryCosmosTest(
        NorthwindQueryCosmosFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override Task String_StartsWith_Literal(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.String_StartsWith_Literal(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND STARTSWITH(c["ContactName"], "M"))
""");
            });

    public override Task String_StartsWith_Parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.String_StartsWith_Parameter(a);

                AssertSql(
                    """
@__pattern_0='M'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND STARTSWITH(c["ContactName"], @__pattern_0))
""");
            });

    public override Task String_StartsWith_Identity(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.String_StartsWith_Identity(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND STARTSWITH(c["ContactName"], c["ContactName"]))
""");
            });

    public override Task String_StartsWith_Column(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.String_StartsWith_Column(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND STARTSWITH(c["ContactName"], c["ContactName"]))
""");
            });

    public override Task String_StartsWith_MethodCall(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.String_StartsWith_MethodCall(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND STARTSWITH(c["ContactName"], "M"))
""");
            });

    public override Task String_EndsWith_Literal(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.String_EndsWith_Literal(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ENDSWITH(c["ContactName"], "b"))
""");
            });

    public override Task String_EndsWith_Parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.String_EndsWith_Parameter(a);

                AssertSql(
                    """
@__pattern_0='b'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ENDSWITH(c["ContactName"], @__pattern_0))
""");
            });

    public override Task String_EndsWith_Identity(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.String_EndsWith_Identity(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ENDSWITH(c["ContactName"], c["ContactName"]))
""");
            });

    public override Task String_EndsWith_Column(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.String_EndsWith_Column(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ENDSWITH(c["ContactName"], c["ContactName"]))
""");
            });

    public override Task String_EndsWith_MethodCall(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.String_EndsWith_MethodCall(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ENDSWITH(c["ContactName"], "m"))
""");
            });

    public override Task String_Contains_Literal(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.String_Contains_Literal(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND CONTAINS(c["ContactName"], "M"))
""");
            });

    public override Task String_Contains_Identity(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.String_Contains_Identity(a);
                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND CONTAINS(c["ContactName"], c["ContactName"]))
""");
            });

    public override Task String_Contains_Column(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.String_Contains_Column(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND CONTAINS(c["ContactName"], c["ContactName"]))
""");
            });

    public override Task String_FirstOrDefault_MethodCall(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.String_FirstOrDefault_MethodCall(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (LEFT(c["ContactName"], 1) = "A"))
""");
            });

    public override Task String_LastOrDefault_MethodCall(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.String_LastOrDefault_MethodCall(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (RIGHT(c["ContactName"], 1) = "s"))
""");
            });

    public override Task String_Contains_MethodCall(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.String_Contains_MethodCall(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND CONTAINS(c["ContactName"], "M"))
""");
            });

    public override async Task String_Compare_simple_zero(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.String_Compare_simple_zero(async));

        AssertSql();
    }

    public override async Task String_Compare_simple_one(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.String_Compare_simple_one(async));

        AssertSql();
    }

    public override async Task String_compare_with_parameter(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.String_compare_with_parameter(async));

        AssertSql();
    }

    public override async Task String_Compare_simple_more_than_one(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.String_Compare_simple_more_than_one(async));

        AssertSql();
    }

    public override async Task String_Compare_nested(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.String_Compare_nested(async));

        AssertSql();
    }

    public override async Task String_Compare_multi_predicate(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.String_Compare_multi_predicate(async));

        AssertSql();
    }

    public override async Task String_Compare_to_simple_zero(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.String_Compare_to_simple_zero(async));

        AssertSql();
    }

    public override async Task String_Compare_to_simple_one(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.String_Compare_to_simple_one(async));

        AssertSql();
    }

    public override async Task String_compare_to_with_parameter(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.String_compare_to_with_parameter(async));

        AssertSql();
    }

    public override async Task String_Compare_to_simple_more_than_one(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.String_Compare_to_simple_more_than_one(async));

        AssertSql();
    }

    public override async Task String_Compare_to_nested(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.String_Compare_to_nested(async));

        AssertSql();
    }

    public override async Task String_Compare_to_multi_predicate(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.String_Compare_to_multi_predicate(async));

        AssertSql();
    }

    public override Task Where_math_abs1(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_math_abs1(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND (ABS(c["ProductID"]) > 10))
""");
            });

    public override Task Where_math_abs2(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_math_abs2(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "OrderDetail") AND (c["UnitPrice"] < 7.0)) AND (ABS(c["Quantity"]) > 10))
""");
            });

    public override Task Where_math_abs3(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_math_abs3(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "OrderDetail") AND (c["Quantity"] < 5)) AND (ABS(c["UnitPrice"]) > 10.0))
""");
            });

    public override Task Where_math_abs_uncorrelated(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_math_abs_uncorrelated(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "OrderDetail") AND (c["UnitPrice"] < 7.0)) AND (10 < c["ProductID"]))
""");
            });

    public override async Task Where_math_ceiling1(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_math_ceiling1(async));

        AssertSql();
    }

    public override Task Where_math_ceiling2(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_math_ceiling2(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "OrderDetail") AND (c["Quantity"] < 5)) AND (CEILING(c["UnitPrice"]) > 10.0))
""");
            });

    public override Task Where_math_floor(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_math_floor(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "OrderDetail") AND (c["Quantity"] < 5)) AND (FLOOR(c["UnitPrice"]) > 10.0))
""");
            });

    public override async Task Where_math_power(bool async)
    {
        // Convert node. Issue #25120.
        await AssertTranslationFailed(() => base.Where_math_power(async));

        AssertSql();
    }

    public override async Task Where_math_square(bool async)
    {
        // Convert node. Issue #25120.
        await AssertTranslationFailed(() => base.Where_math_square(async));

        AssertSql();
    }

    public override Task Where_math_round(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_math_round(async);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "OrderDetail") AND (c["Quantity"] < 5)) AND (ROUND(c["UnitPrice"]) > 10.0))
""");
            });

    public override Task Sum_over_round_works_correctly_in_projection(bool async)
        => AssertTranslationFailed(() => base.Sum_over_round_works_correctly_in_projection(async));

    public override Task Sum_over_round_works_correctly_in_projection_2(bool async)
        => AssertTranslationFailed(() => base.Sum_over_round_works_correctly_in_projection_2(async));

    public override Task Sum_over_truncate_works_correctly_in_projection(bool async)
        => AssertTranslationFailed(() => base.Sum_over_truncate_works_correctly_in_projection(async));

    public override Task Sum_over_truncate_works_correctly_in_projection_2(bool async)
        => AssertTranslationFailed(() => base.Sum_over_truncate_works_correctly_in_projection_2(async));

    public override Task Select_math_round_int(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_math_round_int(a);

                AssertSql(
                    """
SELECT c["OrderID"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] < 10250))
""");
            });

    public override Task Select_math_truncate_int(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_math_truncate_int(a);

                AssertSql(
                    """
SELECT c["OrderID"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] < 10250))
""");
            });

    public override async Task Where_math_round2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_math_round2(async));

        AssertSql();
    }

    public override Task Where_math_truncate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_math_truncate(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "OrderDetail") AND (c["Quantity"] < 5)) AND (TRUNC(c["UnitPrice"]) > 10.0))
""");
            });

    public override async Task Where_math_exp(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_math_exp(async));

        AssertSql();
    }

    public override async Task Where_math_log10(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_math_log10(async));

        AssertSql();
    }

    public override async Task Where_math_log(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_math_log(async));

        AssertSql();
    }

    public override async Task Where_math_log_new_base(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_math_log_new_base(async));

        AssertSql();
    }

    public override async Task Where_math_sqrt(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_math_sqrt(async));

        AssertSql();
    }

    public override async Task Where_math_acos(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_math_acos(async));

        AssertSql();
    }

    public override async Task Where_math_asin(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_math_asin(async));

        AssertSql();
    }

    public override async Task Where_math_atan(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_math_atan(async));

        AssertSql();
    }

    public override async Task Where_math_atan2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_math_atan2(async));

        AssertSql();
    }

    public override async Task Where_math_cos(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_math_cos(async));

        AssertSql();
    }

    public override async Task Where_math_sin(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_math_sin(async));

        AssertSql();
    }

    public override async Task Where_math_tan(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_math_tan(async));

        AssertSql();
    }

    public override Task Where_math_sign(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_math_sign(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "OrderDetail") AND (c["OrderID"] = 11077)) AND (SIGN(c["Discount"]) > 0))
""");
            });

    public override async Task Where_math_min(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_math_min(async));

        AssertSql();
    }

    public override async Task Where_math_min_nested(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_math_min(async));

        AssertSql();
    }

    public override async Task Where_math_min_nested_twice(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_math_min(async));

        AssertSql();
    }

    public override async Task Where_math_max(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_math_max(async));

        AssertSql();
    }

    public override async Task Where_math_max_nested(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_math_max(async));

        AssertSql();
    }

    public override async Task Where_math_max_nested_twice(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_math_max(async));

        AssertSql();
    }

    public override async Task Where_math_degrees(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_math_degrees(async));

        AssertSql();
    }

    public override async Task Where_math_radians(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_math_radians(async));

        AssertSql();
    }

    public override async Task Where_mathf_abs1(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_mathf_abs1(async));

        AssertSql();
    }

    public override Task Where_mathf_ceiling1(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_mathf_ceiling1(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "OrderDetail") AND (c["UnitPrice"] < 7.0)) AND (CEILING(c["Discount"]) > 0.0))
""");
            });

    public override async Task Where_mathf_floor(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_mathf_floor(async));

        AssertSql();
    }

    public override Task Where_mathf_power(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_mathf_power(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "OrderDetail") AND (POWER(c["Discount"], 3.0) > 0.005))
""");
            });

    public override Task Where_mathf_square(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_mathf_square(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "OrderDetail") AND (POWER(c["Discount"], 2.0) > 0.05))
""");
            });

    public override async Task Where_mathf_round2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_mathf_round2(async));

        AssertSql();
    }

    public override async Task Where_mathf_truncate(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_mathf_truncate(async));

        AssertSql();
    }

    public override Task Where_mathf_exp(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_mathf_exp(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "OrderDetail") AND (c["OrderID"] = 11077)) AND (EXP(c["Discount"]) > 1.0))
""");
            });

    public override Task Where_mathf_log10(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_mathf_log10(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "OrderDetail") AND ((c["OrderID"] = 11077) AND (c["Discount"] > 0.0))) AND (LOG10(c["Discount"]) < 0.0))
""");
            });

    public override Task Where_mathf_log(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_mathf_log(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "OrderDetail") AND ((c["OrderID"] = 11077) AND (c["Discount"] > 0.0))) AND (LOG(c["Discount"]) < 0.0))
""");
            });

    public override Task Where_mathf_log_new_base(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_mathf_log_new_base(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "OrderDetail") AND ((c["OrderID"] = 11077) AND (c["Discount"] > 0.0))) AND (LOG(c["Discount"], 7.0) < -1.0))
""");
            });

    public override Task Where_mathf_sqrt(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_mathf_sqrt(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "OrderDetail") AND (c["OrderID"] = 11077)) AND (SQRT(c["Discount"]) > 0.0))
""");
            });

    public override Task Where_mathf_acos(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_mathf_acos(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "OrderDetail") AND (c["OrderID"] = 11077)) AND (ACOS(c["Discount"]) > 1.0))
""");
            });

    public override Task Where_mathf_asin(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_mathf_asin(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "OrderDetail") AND (c["OrderID"] = 11077)) AND (ASIN(c["Discount"]) > 0.0))
""");
            });

    public override Task Where_mathf_atan(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_mathf_atan(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "OrderDetail") AND (c["OrderID"] = 11077)) AND (ATAN(c["Discount"]) > 0.0))
""");
            });

    public override Task Where_mathf_atan2(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_mathf_atan2(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "OrderDetail") AND (c["OrderID"] = 11077)) AND (ATN2(c["Discount"], 1.0) > 0.0))
""");
            });

    public override Task Where_mathf_cos(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_mathf_cos(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "OrderDetail") AND (c["OrderID"] = 11077)) AND (COS(c["Discount"]) > 0.0))
""");
            });

    public override Task Where_mathf_sin(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_mathf_sin(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "OrderDetail") AND (c["OrderID"] = 11077)) AND (SIN(c["Discount"]) > 0.0))
""");
            });

    public override Task Where_mathf_tan(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_mathf_tan(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "OrderDetail") AND (c["OrderID"] = 11077)) AND (TAN(c["Discount"]) > 0.0))
""");
            });

    public override Task Where_mathf_sign(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_mathf_sign(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "OrderDetail") AND (c["OrderID"] = 11077)) AND (SIGN(c["Discount"]) > 0))
""");
            });

    public override Task Where_mathf_degrees(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_mathf_degrees(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "OrderDetail") AND (c["OrderID"] = 11077)) AND (DEGREES(c["Discount"]) > 0.0))
""");
            });

    public override Task Where_mathf_radians(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_mathf_radians(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "OrderDetail") AND (c["OrderID"] = 11077)) AND (RADIANS(c["Discount"]) > 0.0))
""");
            });

    public override async Task Where_guid_newguid(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_guid_newguid(async));

        AssertSql();
    }

    public override Task Where_string_to_upper(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_string_to_upper(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (UPPER(c["CustomerID"]) = "ALFKI"))
""");
            });

    public override Task Where_string_to_lower(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_string_to_lower(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (LOWER(c["CustomerID"]) = "alfki"))
""");
            });

    public override async Task Where_functions_nested(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_functions_nested(async));

        AssertSql();
    }

    public override async Task Convert_ToBoolean(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Convert_ToBoolean(async));

        AssertSql();
    }

    public override async Task Convert_ToByte(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Convert_ToByte(async));

        AssertSql();
    }

    public override async Task Convert_ToDecimal(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Convert_ToDecimal(async));

        AssertSql();
    }

    public override async Task Convert_ToDouble(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Convert_ToDouble(async));

        AssertSql();
    }

    public override async Task Convert_ToInt16(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Convert_ToInt16(async));

        AssertSql();
    }

    public override async Task Convert_ToInt32(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Convert_ToInt32(async));

        AssertSql();
    }

    public override async Task Convert_ToInt64(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Convert_ToInt64(async));

        AssertSql();
    }

    public override async Task Convert_ToString(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Convert_ToString(async));

        AssertSql();
    }

    public override Task Indexof_with_emptystring(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Indexof_with_emptystring(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (INDEX_OF(c["ContactName"], "") = 0))
""");
            });

    public override Task Indexof_with_one_constant_arg(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Indexof_with_one_constant_arg(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (INDEX_OF(c["ContactName"], "a") = 1))
""");
            });

    public override Task Indexof_with_one_parameter_arg(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Indexof_with_one_parameter_arg(a);

                AssertSql(
                    """
@__pattern_0='a'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (INDEX_OF(c["ContactName"], @__pattern_0) = 1))
""");
            });

    public override Task Indexof_with_constant_starting_position(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Indexof_with_constant_starting_position(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (INDEX_OF(c["ContactName"], "a", 2) = 4))
""");
            });

    public override Task Indexof_with_parameter_starting_position(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Indexof_with_parameter_starting_position(a);

                AssertSql(
                    """
@__start_0='2'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (INDEX_OF(c["ContactName"], "a", @__start_0) = 4))
""");
            });

    public override Task Replace_with_emptystring(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Replace_with_emptystring(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (REPLACE(c["ContactName"], "ia", "") = "Mar Anders"))
""");
            });

    public override Task Replace_using_property_arguments(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Replace_using_property_arguments(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (REPLACE(c["ContactName"], c["ContactName"], c["CustomerID"]) = c["CustomerID"]))
""");
            });

    public override Task Substring_with_one_arg_with_zero_startindex(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Substring_with_one_arg_with_zero_startindex(a);

                AssertSql(
                    """
SELECT c["ContactName"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (SUBSTRING(c["CustomerID"], 0, LENGTH(c["CustomerID"])) = "ALFKI"))
""");
            });

    public override Task Substring_with_one_arg_with_constant(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Substring_with_one_arg_with_constant(a);

                AssertSql(
                    """
SELECT c["ContactName"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (SUBSTRING(c["CustomerID"], 1, LENGTH(c["CustomerID"])) = "LFKI"))
""");
            });

    public override Task Substring_with_one_arg_with_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Substring_with_one_arg_with_closure(a);

                AssertSql(
                    """
@__start_0='2'

SELECT c["ContactName"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (SUBSTRING(c["CustomerID"], @__start_0, LENGTH(c["CustomerID"])) = "FKI"))
""");
            });

    public override Task Substring_with_two_args_with_zero_startindex(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Substring_with_two_args_with_zero_startindex(a);

                AssertSql(
                    """
SELECT LEFT(c["ContactName"], 3) AS c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
""");
            });

    public override Task Substring_with_two_args_with_zero_length(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Substring_with_two_args_with_zero_length(a);

                AssertSql(
                    """
SELECT SUBSTRING(c["ContactName"], 2, 0) AS c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
""");
            });

    public override Task Substring_with_two_args_with_constant(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Substring_with_two_args_with_constant(a);

                AssertSql(
                    """
SELECT SUBSTRING(c["ContactName"], 1, 3) AS c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
""");
            });

    public override Task Substring_with_two_args_with_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Substring_with_two_args_with_closure(a);

                AssertSql(
                    """
@__start_0='2'

SELECT SUBSTRING(c["ContactName"], @__start_0, 3) AS c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
""");
            });

    public override Task Substring_with_two_args_with_Index_of(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Substring_with_two_args_with_Index_of(a);

                AssertSql(
                    """
SELECT SUBSTRING(c["ContactName"], INDEX_OF(c["ContactName"], "a"), 3) AS c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
""");
            });

    public override async Task IsNullOrEmpty_in_predicate(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.IsNullOrEmpty_in_predicate(async));

        AssertSql();
    }

    public override async Task IsNullOrEmpty_negated_in_predicate(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.IsNullOrEmpty_negated_in_predicate(async));

        AssertSql();
    }

    public override async Task IsNullOrWhiteSpace_in_predicate_on_non_nullable_column(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.IsNullOrWhiteSpace_in_predicate_on_non_nullable_column(async));

        AssertSql();
    }

    public override Task IsNullOrEmpty_in_projection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.IsNullOrEmpty_in_projection(a);

                AssertSql(
                    """
SELECT c["CustomerID"], c["Region"]
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task IsNullOrEmpty_negated_in_projection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.IsNullOrEmpty_negated_in_projection(a);

                AssertSql(
                    """
SELECT c["CustomerID"], c["Region"]
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override async Task IsNullOrWhiteSpace_in_predicate(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.IsNullOrWhiteSpace_in_predicate(async));

        AssertSql();
    }

    public override Task TrimStart_without_arguments_in_predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.TrimStart_without_arguments_in_predicate(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (LTRIM(c["ContactTitle"]) = "Owner"))
""");
            });

    public override async Task TrimStart_with_char_argument_in_predicate(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.TrimStart_with_char_argument_in_predicate(async));

        AssertSql();
    }

    public override async Task TrimStart_with_char_array_argument_in_predicate(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.TrimStart_with_char_array_argument_in_predicate(async));

        AssertSql();
    }

    public override Task TrimEnd_without_arguments_in_predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.TrimEnd_without_arguments_in_predicate(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (RTRIM(c["ContactTitle"]) = "Owner"))
""");
            });

    public override async Task TrimEnd_with_char_argument_in_predicate(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.TrimEnd_with_char_argument_in_predicate(async));

        AssertSql();
    }

    public override async Task TrimEnd_with_char_array_argument_in_predicate(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.TrimEnd_with_char_array_argument_in_predicate(async));

        AssertSql();
    }

    public override Task Trim_without_argument_in_predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Trim_without_argument_in_predicate(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (TRIM(c["ContactTitle"]) = "Owner"))
""");
            });

    public override async Task Trim_with_char_argument_in_predicate(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Trim_with_char_argument_in_predicate(async));

        AssertSql();
    }

    public override async Task Trim_with_char_array_argument_in_predicate(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Trim_with_char_array_argument_in_predicate(async));

        AssertSql();
    }

    public override async Task Order_by_length_twice(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Unsupported ORDER BY clause. Issue #27037.
            await Assert.ThrowsAsync<CosmosException>(() => base.Order_by_length_twice(async));

            AssertSql(
                """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY LENGTH(c["CustomerID"]), c["CustomerID"]
""");
        }
    }

    public override async Task Order_by_length_twice_followed_by_projection_of_naked_collection_navigation(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(
            () => base.Order_by_length_twice_followed_by_projection_of_naked_collection_navigation(async));

        AssertSql();
    }

    public override Task Static_string_equals_in_predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Static_string_equals_in_predicate(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ANATR"))
""");
            });

    public override Task Static_equals_nullable_datetime_compared_to_non_nullable(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Static_equals_nullable_datetime_compared_to_non_nullable(a);

                AssertSql(
                    """
@__arg_0='1996-07-04T00:00:00'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderDate"] = @__arg_0))
""");
            });

    public override Task Static_equals_int_compared_to_long(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Static_equals_int_compared_to_long(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND false)
""");
            });

    public override async Task Projecting_Math_Truncate_and_ordering_by_it_twice(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Projecting_Math_Truncate_and_ordering_by_it_twice(async));

        AssertSql();
    }

    public override async Task Projecting_Math_Truncate_and_ordering_by_it_twice2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Projecting_Math_Truncate_and_ordering_by_it_twice2(async));

        AssertSql();
    }

    public override async Task Projecting_Math_Truncate_and_ordering_by_it_twice3(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Projecting_Math_Truncate_and_ordering_by_it_twice3(async));

        AssertSql();
    }

    public override async Task DateTime_Compare_to_simple_zero(bool async, bool compareTo)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.DateTime_Compare_to_simple_zero(async, compareTo));

        AssertSql();
    }

    public override async Task TimeSpan_Compare_to_simple_zero(bool async, bool compareTo)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.TimeSpan_Compare_to_simple_zero(async, compareTo));

        AssertSql();
    }

    public override async Task Int_Compare_to_simple_zero(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Int_Compare_to_simple_zero(async));

        AssertSql();
    }

    public override Task Regex_IsMatch_MethodCall(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Regex_IsMatch_MethodCall(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND RegexMatch(c["CustomerID"], "^T"))
""");
            });

    public override Task Regex_IsMatch_MethodCall_constant_input(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Regex_IsMatch_MethodCall_constant_input(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND RegexMatch("ALFKI", c["CustomerID"]))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Regex_IsMatch_MethodCall_With_Option_None(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    async,
                    ss => ss.Set<Customer>().Where(o => Regex.IsMatch(o.CustomerID, "^T", RegexOptions.None)));

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND RegexMatch(c["CustomerID"], "^T"))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Regex_IsMatch_MethodCall_With_Option_IgnoreCase(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    async,
                    ss => ss.Set<Customer>().Where(o => Regex.IsMatch(o.CustomerID, "^T", RegexOptions.IgnoreCase)));

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND RegexMatch(c["CustomerID"], "^T", "i"))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Regex_IsMatch_MethodCall_With_Option_Multiline(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    async,
                    ss => ss.Set<Customer>().Where(o => Regex.IsMatch(o.CustomerID, "^T", RegexOptions.Multiline)));

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND RegexMatch(c["CustomerID"], "^T", "m"))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Regex_IsMatch_MethodCall_With_Option_Singleline(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    async,
                    ss => ss.Set<Customer>().Where(o => Regex.IsMatch(o.CustomerID, "^T", RegexOptions.Singleline)));

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND RegexMatch(c["CustomerID"], "^T", "s"))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Regex_IsMatch_MethodCall_With_Option_IgnorePatternWhitespace(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    async,
                    ss => ss.Set<Customer>().Where(o => Regex.IsMatch(o.CustomerID, "^T", RegexOptions.IgnorePatternWhitespace)));

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND RegexMatch(c["CustomerID"], "^T", "x"))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Regex_IsMatch_MethodCall_With_Options_IgnoreCase_And_IgnorePatternWhitespace(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    async,
                    ss => ss.Set<Customer>().Where(
                        o => Regex.IsMatch(o.CustomerID, "^T", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace)));

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND RegexMatch(c["CustomerID"], "^T", "ix"))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Regex_IsMatch_MethodCall_With_Unsupported_Option(bool async)
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(o => Regex.IsMatch(o.CustomerID, "^T", RegexOptions.RightToLeft))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Regex_IsMatch_MethodCall_With_Any_Unsupported_Option(bool async)
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(o => Regex.IsMatch(o.CustomerID, "^T", RegexOptions.IgnoreCase | RegexOptions.RightToLeft))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Case_insensitive_string_comparison_instance(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    async,
                    ss => ss.Set<Customer>().Where(c => c.CustomerID.Equals("alFkI", StringComparison.OrdinalIgnoreCase)));

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND STRINGEQUALS(c["CustomerID"], "alFkI", true))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Case_insensitive_string_comparison_static(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    async,
                    ss => ss.Set<Customer>().Where(c => string.Equals(c.CustomerID, "alFkI", StringComparison.OrdinalIgnoreCase)));

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND STRINGEQUALS(c["CustomerID"], "alFkI", true))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Case_sensitive_string_comparison_instance(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    async,
                    ss => ss.Set<Customer>().Where(c => c.CustomerID.Equals("ALFKI", StringComparison.Ordinal)));

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND STRINGEQUALS(c["CustomerID"], "ALFKI"))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Case_sensitive_string_comparison_static(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    async,
                    ss => ss.Set<Customer>().Where(c => string.Equals(c.CustomerID, "ALFKI", StringComparison.Ordinal)));

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND STRINGEQUALS(c["CustomerID"], "ALFKI"))
""");
            });

    public override async Task Datetime_subtraction_TotalDays(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Datetime_subtraction_TotalDays(async));

        AssertSql();
    }

    public override Task String_Contains_constant_with_whitespace(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.String_Contains_constant_with_whitespace(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND CONTAINS(c["ContactName"], "     "))
""");
            });

    public override Task String_Contains_parameter_with_whitespace(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.String_Contains_parameter_with_whitespace(a);

                AssertSql(
                    """
@__pattern_0='     '

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND CONTAINS(c["ContactName"], @__pattern_0))
""");
            });

    public override Task Select_mathf_round(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_mathf_round(a);

                AssertSql(
                    """
SELECT c["OrderID"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] < 10250))
""");
            });

    public override Task Select_mathf_round2(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_mathf_round2(a);

                AssertSql(
                    """
SELECT c["UnitPrice"]
FROM root c
WHERE ((c["Discriminator"] = "OrderDetail") AND (c["Quantity"] < 5))
""");
            });

    public override Task Select_mathf_truncate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_mathf_truncate(a);

                AssertSql(
                    """
SELECT c["UnitPrice"]
FROM root c
WHERE ((c["Discriminator"] = "OrderDetail") AND (c["Quantity"] < 5))
""");
            });

    public override Task String_Join_over_non_nullable_column(bool async)
        => AssertTranslationFailed(() => base.String_Join_over_non_nullable_column(async));

    public override Task String_Join_with_predicate(bool async)
        => AssertTranslationFailed(() => base.String_Join_with_predicate(async));

    public override Task String_Join_with_ordering(bool async)
        => AssertTranslationFailed(() => base.String_Join_with_ordering(async));

    public override Task String_Join_over_nullable_column(bool async)
        => AssertTranslationFailed(() => base.String_Join_over_nullable_column(async));

    public override Task String_Concat(bool async)
        => AssertTranslationFailed(() => base.String_Concat(async));

    public override Task Where_DateOnly_FromDateTime(bool async)
        => AssertTranslationFailed(() => base.Where_DateOnly_FromDateTime(async));

    public override Task Select_ToString_IndexOf(bool async)
        => AssertTranslationFailed(() => base.Select_ToString_IndexOf(async));

    public override Task Select_IndexOf_ToString(bool async)
        => AssertTranslationFailed(() => base.Select_IndexOf_ToString(async));

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
