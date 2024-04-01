// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindFunctionsQuerySqliteTest : NorthwindFunctionsQueryRelationalTestBase<
    NorthwindQuerySqliteFixture<NoopModelCustomizer>>
{
    public NorthwindFunctionsQuerySqliteTest(
        NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task Convert_ToBoolean(bool async)
        => AssertTranslationFailed(() => base.Convert_ToBoolean(async));

    public override Task Convert_ToByte(bool async)
        => AssertTranslationFailed(() => base.Convert_ToByte(async));

    public override Task Convert_ToDecimal(bool async)
        => AssertTranslationFailed(() => base.Convert_ToDecimal(async));

    public override Task Convert_ToDouble(bool async)
        => AssertTranslationFailed(() => base.Convert_ToDouble(async));

    public override Task Convert_ToInt16(bool async)
        => AssertTranslationFailed(() => base.Convert_ToInt16(async));

    public override Task Convert_ToInt32(bool async)
        => AssertTranslationFailed(() => base.Convert_ToInt32(async));

    public override Task Convert_ToInt64(bool async)
        => AssertTranslationFailed(() => base.Convert_ToInt64(async));

    public override Task Convert_ToString(bool async)
        => AssertTranslationFailed(() => base.Convert_ToString(async));

    public override async Task Projecting_Math_Truncate_and_ordering_by_it_twice(bool async)
    {
        await base.Projecting_Math_Truncate_and_ordering_by_it_twice(async);

        AssertSql(
            """
SELECT trunc(CAST("o"."OrderID" AS REAL)) AS "A"
FROM "Orders" AS "o"
WHERE "o"."OrderID" < 10250
ORDER BY trunc(CAST("o"."OrderID" AS REAL))
""");
    }

    public override async Task Projecting_Math_Truncate_and_ordering_by_it_twice2(bool async)
    {
        await base.Projecting_Math_Truncate_and_ordering_by_it_twice2(async);

        AssertSql(
            """
SELECT trunc(CAST("o"."OrderID" AS REAL)) AS "A"
FROM "Orders" AS "o"
WHERE "o"."OrderID" < 10250
ORDER BY trunc(CAST("o"."OrderID" AS REAL)) DESC
""");
    }

    public override async Task Projecting_Math_Truncate_and_ordering_by_it_twice3(bool async)
    {
        await base.Projecting_Math_Truncate_and_ordering_by_it_twice3(async);

        AssertSql(
            """
SELECT trunc(CAST("o"."OrderID" AS REAL)) AS "A"
FROM "Orders" AS "o"
WHERE "o"."OrderID" < 10250
ORDER BY trunc(CAST("o"."OrderID" AS REAL)) DESC
""");
    }

    public override async Task Where_functions_nested(bool async)
    {
        await base.Where_functions_nested(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE pow(CAST(length("c"."CustomerID") AS REAL), 2.0) = 25.0
""");
    }

    public override Task Where_guid_newguid(bool async)
        => AssertTranslationFailed(() => base.Where_guid_newguid(async));

    public override Task Where_math_abs3(bool async)
        => AssertTranslationFailed(() => base.Where_math_abs3(async));

    public override async Task Where_math_acos(bool async)
    {
        await base.Where_math_acos(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND acos(CAST("o"."Discount" AS REAL)) > 1.0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_math_acosh(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Acosh(od.Discount + 1) > 0));

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND acosh(CAST("o"."Discount" + 1 AS REAL)) > 0.0
""");
    }

    public override async Task Where_math_asin(bool async)
    {
        await base.Where_math_asin(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND asin(CAST("o"."Discount" AS REAL)) > 0.0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_math_asinh(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Asinh(od.Discount) > 0));

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND asinh(CAST("o"."Discount" AS REAL)) > 0.0
""");
    }

    public override async Task Where_math_atan(bool async)
    {
        await base.Where_math_atan(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND atan(CAST("o"."Discount" AS REAL)) > 0.0
""");
    }

    public override async Task Where_math_atan2(bool async)
    {
        await base.Where_math_atan2(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND atan2(CAST("o"."Discount" AS REAL), 1.0) > 0.0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_math_atanh(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Atanh(od.Discount) > 0));

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND atanh(CAST("o"."Discount" AS REAL)) > 0.0
""");
    }

    public override async Task Where_math_ceiling1(bool async)
    {
        await base.Where_math_ceiling1(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."UnitPrice" < 7.0 AND ceiling(CAST("o"."Discount" AS REAL)) > 0.0
""");
    }

    public override Task Where_math_ceiling2(bool async)
        => AssertTranslationFailed(() => base.Where_math_ceiling2(async));

    public override async Task Where_math_cos(bool async)
    {
        await base.Where_math_cos(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND cos(CAST("o"."Discount" AS REAL)) > 0.0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_math_cosh(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Cosh(od.Discount) > 0));

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND cosh(CAST("o"."Discount" AS REAL)) > 0.0
""");
    }

    public override async Task Where_math_exp(bool async)
    {
        await base.Where_math_exp(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND exp(CAST("o"."Discount" AS REAL)) > 1.0
""");
    }

    public override Task Where_math_floor(bool async)
        => AssertTranslationFailed(() => base.Where_math_floor(async));

    public override async Task Where_math_log(bool async)
    {
        await base.Where_math_log(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND "o"."Discount" > 0 AND ln(CAST("o"."Discount" AS REAL)) < 0.0
""");
    }

    public override async Task Where_math_log_new_base(bool async)
    {
        await base.Where_math_log_new_base(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND "o"."Discount" > 0 AND log(7.0, CAST("o"."Discount" AS REAL)) < -1.0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_math_log2(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077 && od.Discount > 0).Where(od => Math.Log2(od.Discount) < 0));

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND "o"."Discount" > 0 AND log2(CAST("o"."Discount" AS REAL)) < 0.0
""");
    }

    public override async Task Where_math_log10(bool async)
    {
        await base.Where_math_log10(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND "o"."Discount" > 0 AND log10(CAST("o"."Discount" AS REAL)) < 0.0
""");
    }

    public override async Task Where_math_power(bool async)
    {
        await base.Where_math_power(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE pow(CAST("o"."Discount" AS REAL), 3.0) > 0.004999999888241291
""");
    }

    public override async Task Where_math_square(bool async)
    {
        await base.Where_math_square(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE pow(CAST("o"."Discount" AS REAL), 2.0) > 0.05000000074505806
""");
    }

    public override Task Where_math_round(bool async)
        => AssertTranslationFailed(() => base.Where_math_round(async));

    public override Task Sum_over_round_works_correctly_in_projection(bool async)
        => AssertTranslationFailed(() => base.Sum_over_round_works_correctly_in_projection(async));

    public override Task Sum_over_round_works_correctly_in_projection_2(bool async)
        => AssertTranslationFailed(() => base.Sum_over_round_works_correctly_in_projection_2(async));

    public override Task Sum_over_truncate_works_correctly_in_projection(bool async)
        => AssertTranslationFailed(() => base.Sum_over_truncate_works_correctly_in_projection(async));

    public override Task Sum_over_truncate_works_correctly_in_projection_2(bool async)
        => AssertTranslationFailed(() => base.Sum_over_truncate_works_correctly_in_projection_2(async));

    public override Task Where_math_round2(bool async)
        => AssertTranslationFailed(() => base.Where_math_round2(async));

    public override async Task Where_math_sign(bool async)
    {
        await base.Where_math_sign(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND sign("o"."Discount") > 0
""");
    }

    public override async Task Where_math_sin(bool async)
    {
        await base.Where_math_sin(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND sin(CAST("o"."Discount" AS REAL)) > 0.0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_math_sinh(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Sinh(od.Discount) > 0));

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND sinh(CAST("o"."Discount" AS REAL)) > 0.0
""");
    }

    public override async Task Where_math_sqrt(bool async)
    {
        await base.Where_math_sqrt(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND sqrt(CAST("o"."Discount" AS REAL)) > 0.0
""");
    }

    public override async Task Where_math_tan(bool async)
    {
        await base.Where_math_tan(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND tan(CAST("o"."Discount" AS REAL)) > 0.0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_math_tanh(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Tanh(od.Discount) > 0));

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND tanh(CAST("o"."Discount" AS REAL)) > 0.0
""");
    }

    public override Task Where_math_truncate(bool async)
        => AssertTranslationFailed(() => base.Where_math_truncate(async));

    public override async Task Where_math_degrees(bool async)
    {
        await base.Where_math_degrees(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND degrees(CAST("o"."Discount" AS REAL)) > 0.0
""");
    }

    public override async Task Where_math_radians(bool async)
    {
        await base.Where_math_radians(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND radians(CAST("o"."Discount" AS REAL)) > 0.0
""");
    }

    public override async Task Where_mathf_acos(bool async)
    {
        await base.Where_mathf_acos(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND acos("o"."Discount") > 1
""");
    }

    public override async Task Where_mathf_asin(bool async)
    {
        await base.Where_mathf_asin(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND asin("o"."Discount") > 0
""");
    }

    public override async Task Where_mathf_atan(bool async)
    {
        await base.Where_mathf_atan(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND atan("o"."Discount") > 0
""");
    }

    public override async Task Where_mathf_atan2(bool async)
    {
        await base.Where_mathf_atan2(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND atan2("o"."Discount", 1) > 0
""");
    }

    public override async Task Where_mathf_ceiling1(bool async)
    {
        await base.Where_mathf_ceiling1(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."UnitPrice" < 7.0 AND ceiling("o"."Discount") > 0
""");
    }

    public override async Task Where_mathf_cos(bool async)
    {
        await base.Where_mathf_cos(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND cos("o"."Discount") > 0
""");
    }

    public override async Task Where_mathf_exp(bool async)
    {
        await base.Where_mathf_exp(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND exp("o"."Discount") > 1
""");
    }

    public override async Task Where_mathf_floor(bool async)
    {
        await base.Where_mathf_floor(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."Quantity" < 5 AND floor(CAST("o"."UnitPrice" AS REAL)) > 10
""");
    }

    public override async Task Where_mathf_log(bool async)
    {
        await base.Where_mathf_log(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND "o"."Discount" > 0 AND ln("o"."Discount") < 0
""");
    }

    public override async Task Where_mathf_log_new_base(bool async)
    {
        await base.Where_mathf_log_new_base(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND "o"."Discount" > 0 AND log(7, "o"."Discount") < -1
""");
    }

    public override async Task Where_mathf_log10(bool async)
    {
        await base.Where_mathf_log10(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND "o"."Discount" > 0 AND log10("o"."Discount") < 0
""");
    }

    public override async Task Where_mathf_power(bool async)
    {
        await base.Where_mathf_power(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE pow("o"."Discount", 3) > 0.005
""");
    }

    public override async Task Where_mathf_square(bool async)
    {
        await base.Where_mathf_square(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE pow("o"."Discount", 2) > 0.05
""");
    }

    public override async Task Where_mathf_sign(bool async)
    {
        await base.Where_mathf_sign(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND sign("o"."Discount") > 0
""");
    }

    public override async Task Where_mathf_sin(bool async)
    {
        await base.Where_mathf_sin(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND sin("o"."Discount") > 0
""");
    }

    public override async Task Where_mathf_sqrt(bool async)
    {
        await base.Where_mathf_sqrt(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND sqrt("o"."Discount") > 0
""");
    }

    public override async Task Where_mathf_tan(bool async)
    {
        await base.Where_mathf_tan(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND tan("o"."Discount") > 0
""");
    }

    public override async Task Where_mathf_truncate(bool async)
    {
        await base.Where_mathf_truncate(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."Quantity" < 5 AND trunc(CAST("o"."UnitPrice" AS REAL)) > 10
""");
    }

    public override async Task Where_mathf_degrees(bool async)
    {
        await base.Where_mathf_degrees(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND degrees("o"."Discount") > 0
""");
    }

    public override async Task Where_mathf_radians(bool async)
    {
        await base.Where_mathf_radians(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND radians("o"."Discount") > 0
""");
    }

    public override async Task String_StartsWith_Literal(bool async)
    {
        await base.String_StartsWith_Literal(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE "c"."ContactName" LIKE 'M%'
""");
    }

    public override async Task String_StartsWith_Parameter(bool async)
    {
        await base.String_StartsWith_Parameter(async);

        AssertSql(
            """
@__pattern_0_startswith='M%' (Size = 2)

SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE "c"."ContactName" LIKE @__pattern_0_startswith ESCAPE '\'
""");
    }

    public override async Task String_StartsWith_Identity(bool async)
    {
        await base.String_StartsWith_Identity(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE "c"."ContactName" IS NOT NULL AND (substr("c"."ContactName", 1, length("c"."ContactName")) = "c"."ContactName" OR "c"."ContactName" = '')
""");
    }

    public override async Task String_StartsWith_Column(bool async)
    {
        await base.String_StartsWith_Column(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE "c"."ContactName" IS NOT NULL AND (substr("c"."ContactName", 1, length("c"."ContactName")) = "c"."ContactName" OR "c"."ContactName" = '')
""");
    }

    public override async Task String_StartsWith_MethodCall(bool async)
    {
        await base.String_StartsWith_MethodCall(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE "c"."ContactName" LIKE 'M%'
""");
    }

    public override async Task String_EndsWith_Literal(bool async)
    {
        await base.String_EndsWith_Literal(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE "c"."ContactName" LIKE '%b'
""");
    }

    public override async Task String_EndsWith_Parameter(bool async)
    {
        await base.String_EndsWith_Parameter(async);

        AssertSql(
            """
@__pattern_0_endswith='%b' (Size = 2)

SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE "c"."ContactName" LIKE @__pattern_0_endswith ESCAPE '\'
""");
    }

    public override async Task String_EndsWith_Identity(bool async)
    {
        await base.String_EndsWith_Identity(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE "c"."ContactName" IS NOT NULL AND (substr("c"."ContactName", -length("c"."ContactName")) = "c"."ContactName" OR "c"."ContactName" = '')
""");
    }

    public override async Task String_EndsWith_Column(bool async)
    {
        await base.String_EndsWith_Column(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE "c"."ContactName" IS NOT NULL AND (substr("c"."ContactName", -length("c"."ContactName")) = "c"."ContactName" OR "c"."ContactName" = '')
""");
    }

    public override async Task String_EndsWith_MethodCall(bool async)
    {
        await base.String_EndsWith_MethodCall(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE "c"."ContactName" LIKE '%m'
""");
    }

    public override async Task String_Contains_Literal(bool async)
    {
        await base.String_Contains_Literal(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE "c"."ContactName" IS NOT NULL AND instr("c"."ContactName", 'M') > 0
""");
    }

    public override async Task String_Contains_Identity(bool async)
    {
        await base.String_Contains_Identity(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE "c"."ContactName" IS NOT NULL AND instr("c"."ContactName", "c"."ContactName") > 0
""");
    }

    public override async Task String_Contains_Column(bool async)
    {
        await base.String_Contains_Column(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE "c"."ContactName" IS NOT NULL AND instr("c"."ContactName", "c"."ContactName") > 0
""");
    }

    public override async Task String_FirstOrDefault_MethodCall(bool async)
    {
        await base.String_FirstOrDefault_MethodCall(async);
        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE substr("c"."ContactName", 1, 1) = 'A'
""");
    }

    public override async Task String_LastOrDefault_MethodCall(bool async)
    {
        await base.String_LastOrDefault_MethodCall(async);
        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE substr("c"."ContactName", length("c"."ContactName"), 1) = 's'
""");
    }

    public override async Task String_Contains_MethodCall(bool async)
    {
        await base.String_Contains_MethodCall(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE "c"."ContactName" IS NOT NULL AND instr("c"."ContactName", 'M') > 0
""");
    }

    public override async Task String_Join_over_non_nullable_column(bool async)
    {
        await base.String_Join_over_non_nullable_column(async);

        AssertSql(
            """
SELECT "c"."City", COALESCE(group_concat("c"."CustomerID", '|'), '') AS "Customers"
FROM "Customers" AS "c"
GROUP BY "c"."City"
""");
    }

    public override async Task String_Join_over_nullable_column(bool async)
    {
        await base.String_Join_over_nullable_column(async);

        AssertSql(
            """
SELECT "c"."City", COALESCE(group_concat(COALESCE("c"."Region", ''), '|'), '') AS "Regions"
FROM "Customers" AS "c"
GROUP BY "c"."City"
""");
    }

    public override async Task String_Join_with_predicate(bool async)
    {
        await base.String_Join_with_predicate(async);

        AssertSql(
            """
SELECT "c"."City", COALESCE(group_concat(CASE
    WHEN length("c"."ContactName") > 10 THEN "c"."CustomerID"
END, '|'), '') AS "Customers"
FROM "Customers" AS "c"
GROUP BY "c"."City"
""");
    }

    public override async Task String_Join_with_ordering(bool async)
    {
        // SQLite does not support input ordering on aggregate methods; the below does client evaluation.
        await base.String_Join_with_ordering(async);

        AssertSql(
            """
SELECT "c1"."City", "c0"."CustomerID"
FROM (
    SELECT "c"."City"
    FROM "Customers" AS "c"
    GROUP BY "c"."City"
) AS "c1"
LEFT JOIN "Customers" AS "c0" ON "c1"."City" = "c0"."City"
ORDER BY "c1"."City", "c0"."CustomerID" DESC
""");
    }

    public override async Task String_Concat(bool async)
    {
        await base.String_Concat(async);

        AssertSql(
            """
SELECT "c"."City", COALESCE(group_concat("c"."CustomerID", ''), '') AS "Customers"
FROM "Customers" AS "c"
GROUP BY "c"."City"
""");
    }

    public override async Task IsNullOrWhiteSpace_in_predicate(bool async)
    {
        await base.IsNullOrWhiteSpace_in_predicate(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE "c"."Region" IS NULL OR trim("c"."Region") = ''
""");
    }

    public override async Task Indexof_with_emptystring(bool async)
    {
        await base.Indexof_with_emptystring(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE instr("c"."ContactName", '') - 1 = 0
""");
    }

    public override async Task Indexof_with_one_constant_arg(bool async)
    {
        await base.Indexof_with_one_constant_arg(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE instr("c"."ContactName", 'a') - 1 = 1
""");
    }

    public override async Task Indexof_with_one_parameter_arg(bool async)
    {
        await base.Indexof_with_one_parameter_arg(async);

        AssertSql(
            """
@__pattern_0='a' (Size = 1)

SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE instr("c"."ContactName", @__pattern_0) - 1 = 1
""");
    }

    public override Task Indexof_with_constant_starting_position(bool async)
        => AssertTranslationFailed(() => base.Indexof_with_constant_starting_position(async));

    public override Task Indexof_with_parameter_starting_position(bool async)
        => AssertTranslationFailed(() => base.Indexof_with_parameter_starting_position(async));

    public override async Task Replace_with_emptystring(bool async)
    {
        await base.Replace_with_emptystring(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE replace("c"."ContactName", 'ia', '') = 'Mar Anders'
""");
    }

    public override async Task Replace_using_property_arguments(bool async)
    {
        await base.Replace_using_property_arguments(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE replace("c"."ContactName", "c"."ContactName", "c"."CustomerID") = "c"."CustomerID"
""");
    }

    public override async Task Substring_with_one_arg_with_zero_startindex(bool async)
    {
        await base.Substring_with_one_arg_with_zero_startindex(async);

        AssertSql(
            """
SELECT "c"."ContactName"
FROM "Customers" AS "c"
WHERE substr("c"."CustomerID", 0 + 1) = 'ALFKI'
""");
    }

    public override async Task Substring_with_one_arg_with_constant(bool async)
    {
        await base.Substring_with_one_arg_with_constant(async);

        AssertSql(
            """
SELECT "c"."ContactName"
FROM "Customers" AS "c"
WHERE substr("c"."CustomerID", 1 + 1) = 'LFKI'
""");
    }

    public override async Task Substring_with_one_arg_with_closure(bool async)
    {
        await base.Substring_with_one_arg_with_closure(async);

        AssertSql(
            """
@__start_0='2'

SELECT "c"."ContactName"
FROM "Customers" AS "c"
WHERE substr("c"."CustomerID", @__start_0 + 1) = 'FKI'
""");
    }

    public override async Task Substring_with_two_args_with_zero_startindex(bool async)
    {
        await base.Substring_with_two_args_with_zero_startindex(async);

        AssertSql(
            """
SELECT substr("c"."ContactName", 0 + 1, 3)
FROM "Customers" AS "c"
WHERE "c"."CustomerID" = 'ALFKI'
""");
    }

    public override async Task Substring_with_two_args_with_constant(bool async)
    {
        await base.Substring_with_two_args_with_constant(async);

        AssertSql(
            """
SELECT substr("c"."ContactName", 1 + 1, 3)
FROM "Customers" AS "c"
WHERE "c"."CustomerID" = 'ALFKI'
""");
    }

    public override async Task Substring_with_two_args_with_closure(bool async)
    {
        await base.Substring_with_two_args_with_closure(async);

        AssertSql(
            """
@__start_0='2'

SELECT substr("c"."ContactName", @__start_0 + 1, 3)
FROM "Customers" AS "c"
WHERE "c"."CustomerID" = 'ALFKI'
""");
    }

    public override async Task Substring_with_two_args_with_Index_of(bool async)
    {
        await base.Substring_with_two_args_with_Index_of(async);

        AssertSql(
            """
SELECT substr("c"."ContactName", (instr("c"."ContactName", 'a') - 1) + 1, 3)
FROM "Customers" AS "c"
WHERE "c"."CustomerID" = 'ALFKI'
""");
    }

    public override async Task Substring_with_two_args_with_zero_length(bool async)
    {
        await base.Substring_with_two_args_with_zero_length(async);

        AssertSql(
            """
SELECT substr("c"."ContactName", 2 + 1, 0)
FROM "Customers" AS "c"
WHERE "c"."CustomerID" = 'ALFKI'
""");
    }

    public override async Task Where_math_abs1(bool async)
    {
        await base.Where_math_abs1(async);

        AssertSql(
            """
SELECT "p"."ProductID", "p"."Discontinued", "p"."ProductName", "p"."SupplierID", "p"."UnitPrice", "p"."UnitsInStock"
FROM "Products" AS "p"
WHERE abs("p"."ProductID") > 10
""");
    }

    public override async Task Where_math_abs2(bool async)
    {
        await base.Where_math_abs2(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."UnitPrice" < 7.0 AND abs("o"."Quantity") > 10
""");
    }

    public override async Task Where_math_abs_uncorrelated(bool async)
    {
        await base.Where_math_abs_uncorrelated(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."UnitPrice" < 7.0 AND 10 < "o"."ProductID"
""");
    }

    public override async Task Select_math_round_int(bool async)
    {
        await base.Select_math_round_int(async);

        AssertSql(
            """
SELECT round(CAST("o"."OrderID" AS REAL)) AS "A"
FROM "Orders" AS "o"
WHERE "o"."OrderID" < 10250
""");
    }

    public override async Task Where_math_min(bool async)
    {
        await base.Where_math_min(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND min("o"."OrderID", "o"."ProductID") = "o"."ProductID"
""");
    }

    public override async Task Where_math_min_nested(bool async)
    {
        await base.Where_math_min_nested(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND min("o"."OrderID", "o"."ProductID", 99999) = "o"."ProductID"
""");
    }

    public override async Task Where_math_min_nested_twice(bool async)
    {
        await base.Where_math_min_nested_twice(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND min(99999, "o"."OrderID", 99998, "o"."ProductID") = "o"."ProductID"
""");
    }

    public override async Task Where_math_max(bool async)
    {
        await base.Where_math_max(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND max("o"."OrderID", "o"."ProductID") = "o"."OrderID"
""");
    }

    public override async Task Where_math_max_nested(bool async)
    {
        await base.Where_math_max_nested(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND max("o"."OrderID", "o"."ProductID", 1) = "o"."OrderID"
""");
    }

    public override async Task Where_math_max_nested_twice(bool async)
    {
        await base.Where_math_max_nested_twice(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."OrderID" = 11077 AND max(1, "o"."OrderID", 2, "o"."ProductID") = "o"."OrderID"
""");
    }

    public override async Task Where_string_to_lower(bool async)
    {
        await base.Where_string_to_lower(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE lower("c"."CustomerID") = 'alfki'
""");
    }

    public override async Task Where_string_to_upper(bool async)
    {
        await base.Where_string_to_upper(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE upper("c"."CustomerID") = 'ALFKI'
""");
    }

    public override async Task TrimStart_without_arguments_in_predicate(bool async)
    {
        await base.TrimStart_without_arguments_in_predicate(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE ltrim("c"."ContactTitle") = 'Owner'
""");
    }

    public override async Task TrimStart_with_char_argument_in_predicate(bool async)
    {
        await base.TrimStart_with_char_argument_in_predicate(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE ltrim("c"."ContactTitle", 'O') = 'wner'
""");
    }

    public override async Task TrimStart_with_char_array_argument_in_predicate(bool async)
    {
        await base.TrimStart_with_char_array_argument_in_predicate(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE ltrim("c"."ContactTitle", 'Ow') = 'ner'
""");
    }

    public override async Task TrimEnd_without_arguments_in_predicate(bool async)
    {
        await base.TrimEnd_without_arguments_in_predicate(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE rtrim("c"."ContactTitle") = 'Owner'
""");
    }

    public override async Task TrimEnd_with_char_argument_in_predicate(bool async)
    {
        await base.TrimEnd_with_char_argument_in_predicate(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE rtrim("c"."ContactTitle", 'r') = 'Owne'
""");
    }

    public override async Task TrimEnd_with_char_array_argument_in_predicate(bool async)
    {
        await base.TrimEnd_with_char_array_argument_in_predicate(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE rtrim("c"."ContactTitle", 'er') = 'Own'
""");
    }

    public override async Task Trim_without_argument_in_predicate(bool async)
    {
        await base.Trim_without_argument_in_predicate(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE trim("c"."ContactTitle") = 'Owner'
""");
    }

    public override async Task Trim_with_char_argument_in_predicate(bool async)
    {
        await base.Trim_with_char_argument_in_predicate(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE trim("c"."ContactTitle", 'O') = 'wner'
""");
    }

    public override async Task Trim_with_char_array_argument_in_predicate(bool async)
    {
        await base.Trim_with_char_array_argument_in_predicate(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE trim("c"."ContactTitle", 'Or') = 'wne'
""");
    }

    public override async Task Regex_IsMatch_MethodCall(bool async)
    {
        await base.Regex_IsMatch_MethodCall(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE "c"."CustomerID" REGEXP '^T'
""");
    }

    public override async Task Regex_IsMatch_MethodCall_constant_input(bool async)
    {
        await base.Regex_IsMatch_MethodCall_constant_input(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE 'ALFKI' REGEXP "c"."CustomerID"
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Regex_IsMatch_MethodCall_negated(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(o => !Regex.IsMatch(o.CustomerID, "^[^T]")));

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE "c"."CustomerID" NOT REGEXP '^[^T]'
""");
    }

    public override async Task IsNullOrEmpty_in_predicate(bool async)
    {
        await base.IsNullOrEmpty_in_predicate(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE "c"."Region" IS NULL OR "c"."Region" = ''
""");
    }

    public override async Task IsNullOrEmpty_in_projection(bool async)
    {
        await base.IsNullOrEmpty_in_projection(async);

        AssertSql(
            """
SELECT "c"."CustomerID" AS "Id", "c"."Region" IS NULL OR "c"."Region" = '' AS "Value"
FROM "Customers" AS "c"
""");
    }

    public override async Task IsNullOrEmpty_negated_in_predicate(bool async)
    {
        await base.IsNullOrEmpty_negated_in_predicate(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE "c"."Region" IS NOT NULL AND "c"."Region" <> ''
""");
    }

    public override Task Datetime_subtraction_TotalDays(bool async)
        => AssertTranslationFailed(() => base.Datetime_subtraction_TotalDays(async));

    public override async Task Where_DateOnly_FromDateTime(bool async)
    {
        await base.Where_DateOnly_FromDateTime(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
FROM "Orders" AS "o"
WHERE "o"."OrderDate" IS NOT NULL AND date("o"."OrderDate") = '1996-09-16'
""");
    }

    public override async Task Select_ToString_IndexOf(bool async)
    {
        await base.Select_ToString_IndexOf(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
FROM "Orders" AS "o"
WHERE instr(CAST("o"."OrderID" AS TEXT), '123') - 1 = -1
""");
    }

    public override async Task Select_IndexOf_ToString(bool async)
    {
        await base.Select_IndexOf_ToString(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
FROM "Orders" AS "o"
WHERE instr('123', CAST("o"."OrderID" AS TEXT)) - 1 = -1
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
