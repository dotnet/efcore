// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindFunctionsQueryCosmosTest : NorthwindFunctionsQueryTestBase<NorthwindQueryCosmosFixture<NoopModelCustomizer>>
    {
        public NorthwindFunctionsQueryCosmosTest(
            NorthwindQueryCosmosFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task String_StartsWith_Literal(bool async)
        {
            await base.String_StartsWith_Literal(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND ((c[""ContactName""] != null) AND ((""M"" != null) AND STARTSWITH(c[""ContactName""], ""M""))))");
        }

        public override async Task String_StartsWith_Identity(bool async)
        {
            await base.String_StartsWith_Identity(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND ((c[""ContactName""] = """") OR ((c[""ContactName""] != null) AND ((c[""ContactName""] != null) AND STARTSWITH(c[""ContactName""], c[""ContactName""])))))");
        }

        public override async Task String_StartsWith_Column(bool async)
        {
            await base.String_StartsWith_Column(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND ((c[""ContactName""] = """") OR ((c[""ContactName""] != null) AND ((c[""ContactName""] != null) AND STARTSWITH(c[""ContactName""], c[""ContactName""])))))");
        }

        public override async Task String_StartsWith_MethodCall(bool async)
        {
            await base.String_StartsWith_MethodCall(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND ((c[""ContactName""] != null) AND ((""M"" != null) AND STARTSWITH(c[""ContactName""], ""M""))))");
        }

        public override async Task String_EndsWith_Literal(bool async)
        {
            await base.String_EndsWith_Literal(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND ((c[""ContactName""] != null) AND ((""b"" != null) AND ENDSWITH(c[""ContactName""], ""b""))))");
        }

        public override async Task String_EndsWith_Identity(bool async)
        {
            await base.String_EndsWith_Identity(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND ((c[""ContactName""] = """") OR ((c[""ContactName""] != null) AND ((c[""ContactName""] != null) AND ENDSWITH(c[""ContactName""], c[""ContactName""])))))");
        }

        public override async Task String_EndsWith_Column(bool async)
        {
            await base.String_EndsWith_Column(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND ((c[""ContactName""] = """") OR ((c[""ContactName""] != null) AND ((c[""ContactName""] != null) AND ENDSWITH(c[""ContactName""], c[""ContactName""])))))");
        }

        public override async Task String_EndsWith_MethodCall(bool async)
        {
            await base.String_EndsWith_MethodCall(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND ((c[""ContactName""] != null) AND ((""m"" != null) AND ENDSWITH(c[""ContactName""], ""m""))))");
        }

        public override async Task String_Contains_Literal(bool async)
        {
            await base.String_Contains_Literal(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND CONTAINS(c[""ContactName""], ""M""))");
        }

        public override async Task String_Contains_Identity(bool async)
        {
            await base.String_Contains_Identity(async);
            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND CONTAINS(c[""ContactName""], c[""ContactName""]))");
        }

        public override async Task String_Contains_Column(bool async)
        {
            await base.String_Contains_Column(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND CONTAINS(c[""ContactName""], c[""ContactName""]))");
        }

        public override async Task String_FirstOrDefault_MethodCall(bool async)
        {
            await base.String_FirstOrDefault_MethodCall(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (LEFT(c[""ContactName""], 1) = ""A""))");
        }

        public override async Task String_LastOrDefault_MethodCall(bool async)
        {
            await base.String_LastOrDefault_MethodCall(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (RIGHT(c[""ContactName""], 1) = ""s""))");
        }

        public override async Task String_Contains_MethodCall(bool async)
        {
            await base.String_Contains_MethodCall(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND CONTAINS(c[""ContactName""], ""M""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_Compare_simple_zero(bool async)
        {
            await base.String_Compare_simple_zero(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_Compare_simple_one(bool async)
        {
            await base.String_Compare_simple_one(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_compare_with_parameter(bool async)
        {
            await base.String_compare_with_parameter(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_Compare_simple_more_than_one(bool async)
        {
            await base.String_Compare_simple_more_than_one(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_Compare_nested(bool async)
        {
            await base.String_Compare_nested(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_Compare_multi_predicate(bool async)
        {
            await base.String_Compare_multi_predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_Compare_to_simple_zero(bool async)
        {
            await base.String_Compare_to_simple_zero(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_Compare_to_simple_one(bool async)
        {
            await base.String_Compare_to_simple_one(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_compare_to_with_parameter(bool async)
        {
            await base.String_compare_to_with_parameter(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_Compare_to_simple_more_than_one(bool async)
        {
            await base.String_Compare_to_simple_more_than_one(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_Compare_to_nested(bool async)
        {
            await base.String_Compare_to_nested(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_Compare_to_multi_predicate(bool async)
        {
            await base.String_Compare_to_multi_predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Where_math_abs1(bool async)
        {
            await base.Where_math_abs1(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND (ABS(c[""ProductID""]) > 10))");
        }

        public override async Task Where_math_abs2(bool async)
        {
            await base.Where_math_abs2(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (((c[""Discriminator""] = ""OrderDetail"") AND (c[""UnitPrice""] < 7.0)) AND (ABS(c[""Quantity""]) > 10))");
        }

        public override async Task Where_math_abs3(bool async)
        {
            await base.Where_math_abs3(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (((c[""Discriminator""] = ""OrderDetail"") AND (c[""Quantity""] < 5)) AND (ABS(c[""UnitPrice""]) > 10.0))");
        }

        public override async Task Where_math_abs_uncorrelated(bool async)
        {
            await base.Where_math_abs_uncorrelated(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (((c[""Discriminator""] = ""OrderDetail"") AND (c[""UnitPrice""] < 7.0)) AND (10 < c[""ProductID""]))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_ceiling1(bool async)
        {
            await base.Where_math_ceiling1(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_ceiling2(bool async)
        {
            await base.Where_math_ceiling2(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        public override async Task Where_math_floor(bool async)
        {
            await base.Where_math_floor(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (((c[""Discriminator""] = ""OrderDetail"") AND (c[""Quantity""] < 5)) AND (FLOOR(c[""UnitPrice""]) > 10.0))");
        }

        [ConditionalTheory(Skip = "Issue #25120")]
        public override async Task Where_math_power(bool async)
        {
            await base.Where_math_power(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        [ConditionalTheory(Skip = "Issue #25120")]
        public override async Task Where_math_square(bool async)
        {
            await base.Where_math_square(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        public override async Task Where_math_round(bool async)
        {
            await base.Where_math_round(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (((c[""Discriminator""] = ""OrderDetail"") AND (c[""Quantity""] < 5)) AND (ROUND(c[""UnitPrice""]) > 10.0))");
        }

        public override async Task Select_math_round_int(bool async)
        {
            await base.Select_math_round_int(async);

            AssertSql(
                @"SELECT c[""OrderID""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] < 10250))");
        }

        public override async Task Select_math_truncate_int(bool async)
        {
            await base.Select_math_truncate_int(async);

            AssertSql(
                @"SELECT c[""OrderID""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] < 10250))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_round2(bool async)
        {
            await base.Where_math_round2(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (ROUND(c[""UnitPrice""]) > 100.0))");
        }

        public override async Task Where_math_truncate(bool async)
        {
            await base.Where_math_truncate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (((c[""Discriminator""] = ""OrderDetail"") AND (c[""Quantity""] < 5)) AND (TRUNC(c[""UnitPrice""]) > 10.0))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_exp(bool async)
        {
            await base.Where_math_exp(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] = 11077))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_log10(bool async)
        {
            await base.Where_math_log10(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND ((c[""OrderID""] = 11077) AND (c[""Discount""] > 0.0)))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_log(bool async)
        {
            await base.Where_math_log(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND ((c[""OrderID""] = 11077) AND (c[""Discount""] > 0.0)))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_log_new_base(bool async)
        {
            await base.Where_math_log_new_base(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND ((c[""OrderID""] = 11077) AND (c[""Discount""] > 0.0)))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_sqrt(bool async)
        {
            await base.Where_math_sqrt(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] = 11077))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_acos(bool async)
        {
            await base.Where_math_acos(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] = 11077))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_asin(bool async)
        {
            await base.Where_math_asin(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] = 11077))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_atan(bool async)
        {
            await base.Where_math_atan(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] = 11077))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_atan2(bool async)
        {
            await base.Where_math_atan2(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] = 11077))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_cos(bool async)
        {
            await base.Where_math_cos(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] = 11077))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_sin(bool async)
        {
            await base.Where_math_sin(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] = 11077))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_tan(bool async)
        {
            await base.Where_math_tan(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] = 11077))");
        }

        public override async Task Where_math_sign(bool async)
        {
            await base.Where_math_sign(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] = 11077)) AND (SIGN(c[""Discount""]) > 0))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_min(bool async)
        {
            await base.Where_math_min(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] = 11077))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_max(bool async)
        {
            await base.Where_math_max(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] = 11077))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_mathf_abs1(bool async)
        {
            await base.Where_mathf_abs1(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ABS(CAST([p].[ProductID] AS real)) > CAST(10 AS real)");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_mathf_ceiling1(bool async)
        {
            await base.Where_mathf_ceiling1(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[UnitPrice] < 7.0) AND (CEILING([o].[Discount]) > CAST(0 AS real))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_mathf_floor(bool async)
        {
            await base.Where_mathf_floor(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[Quantity] < CAST(5 AS smallint)) AND (FLOOR(CAST([o].[UnitPrice] AS real)) > CAST(10 AS real))");
        }

        [ConditionalTheory]
        public override async Task Where_mathf_power(bool async)
        {
            await base.Where_mathf_power(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (POWER(c[""Discount""], 3.0) > 0.005))");
        }

        [ConditionalTheory]
        public override async Task Where_mathf_square(bool async)
        {
            await base.Where_mathf_square(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (POWER(c[""Discount""], 2.0) > 0.05))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_mathf_round2(bool async)
        {
            await base.Where_mathf_round2(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ROUND(CAST([o].[UnitPrice] AS real), 2) > CAST(100 AS real)");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_mathf_truncate(bool async)
        {
            await base.Where_mathf_truncate(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[Quantity] < CAST(5 AS smallint)) AND (ROUND(CAST([o].[UnitPrice] AS real), 0, 1) > CAST(10 AS real))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_mathf_exp(bool async)
        {
            await base.Where_mathf_exp(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (EXP([o].[Discount]) > CAST(1 AS real))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_mathf_log10(bool async)
        {
            await base.Where_mathf_log10(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE (([o].[OrderID] = 11077) AND ([o].[Discount] > CAST(0 AS real))) AND (LOG10([o].[Discount]) < CAST(0 AS real))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_mathf_log(bool async)
        {
            await base.Where_mathf_log(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE (([o].[OrderID] = 11077) AND ([o].[Discount] > CAST(0 AS real))) AND (LOG([o].[Discount]) < CAST(0 AS real))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_mathf_log_new_base(bool async)
        {
            await base.Where_mathf_log_new_base(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE (([o].[OrderID] = 11077) AND ([o].[Discount] > CAST(0 AS real))) AND (LOG([o].[Discount], CAST(7 AS real)) < CAST(0 AS real))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_mathf_sqrt(bool async)
        {
            await base.Where_mathf_sqrt(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (SQRT([o].[Discount]) > CAST(0 AS real))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_mathf_acos(bool async)
        {
            await base.Where_mathf_acos(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (ACOS([o].[Discount]) > CAST(1 AS real))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_mathf_asin(bool async)
        {
            await base.Where_mathf_asin(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (ASIN([o].[Discount]) > CAST(0 AS real))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_mathf_atan(bool async)
        {
            await base.Where_mathf_atan(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (ATAN([o].[Discount]) > CAST(0 AS real))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_mathf_atan2(bool async)
        {
            await base.Where_mathf_atan2(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (ATN2([o].[Discount], CAST(1 AS real)) > CAST(0 AS real))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_mathf_cos(bool async)
        {
            await base.Where_mathf_cos(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (COS([o].[Discount]) > CAST(0 AS real))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_mathf_sin(bool async)
        {
            await base.Where_mathf_sin(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (SIN([o].[Discount]) > CAST(0 AS real))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_mathf_tan(bool async)
        {
            await base.Where_mathf_tan(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (TAN([o].[Discount]) > CAST(0 AS real))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_mathf_sign(bool async)
        {
            await base.Where_mathf_sign(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (SIGN([o].[Discount]) > 0)");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_guid_newguid(bool async)
        {
            await base.Where_guid_newguid(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        public override async Task Where_string_to_upper(bool async)
        {
            await base.Where_string_to_upper(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (UPPER(c[""CustomerID""]) = ""ALFKI""))");
        }

        public override async Task Where_string_to_lower(bool async)
        {
            await base.Where_string_to_lower(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (LOWER(c[""CustomerID""]) = ""alfki""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_functions_nested(bool async)
        {
            await base.Where_functions_nested(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Convert_ToBoolean(bool async)
        {
            await base.Convert_ToBoolean(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Convert_ToByte(bool async)
        {
            await base.Convert_ToByte(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Convert_ToDecimal(bool async)
        {
            await base.Convert_ToDecimal(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Convert_ToDouble(bool async)
        {
            await base.Convert_ToDouble(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Convert_ToInt16(bool async)
        {
            await base.Convert_ToInt16(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Convert_ToInt32(bool async)
        {
            await base.Convert_ToInt32(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Convert_ToInt64(bool async)
        {
            await base.Convert_ToInt64(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Convert_ToString(bool async)
        {
            await base.Convert_ToString(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Indexof_with_emptystring(bool async)
        {
            await base.Indexof_with_emptystring(async);

            AssertSql(
                @"SELECT INDEX_OF(c[""ContactName""], """") AS c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Indexof_with_starting_position(bool async)
        {
            await base.Indexof_with_starting_position(async);

            AssertSql(
                @"SELECT INDEX_OF(c[""ContactName""], ""a"", 3) AS c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Replace_with_emptystring(bool async)
        {
            await base.Replace_with_emptystring(async);

            AssertSql(
                @"SELECT REPLACE(c[""ContactName""], ""ari"", """") AS c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Replace_using_property_arguments(bool async)
        {
            await base.Replace_using_property_arguments(async);

            AssertSql(
                @"SELECT REPLACE(c[""ContactName""], c[""ContactName""], c[""CustomerID""]) AS c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Substring_with_one_arg_with_zero_startindex(bool async)
        {
            await base.Substring_with_one_arg_with_zero_startindex(async);

            AssertSql(
                @"SELECT c[""ContactName""]
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (SUBSTRING(c[""CustomerID""], 0, LENGTH(c[""CustomerID""])) = ""ALFKI""))");
        }

        public override async Task Substring_with_one_arg_with_constant(bool async)
        {
            await base.Substring_with_one_arg_with_constant(async);

            AssertSql(
                @"SELECT c[""ContactName""]
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (SUBSTRING(c[""CustomerID""], 1, LENGTH(c[""CustomerID""])) = ""LFKI""))");
        }

        public override async Task Substring_with_one_arg_with_closure(bool async)
        {
            await base.Substring_with_one_arg_with_closure(async);

            AssertSql(
                @"@__start_0='2'

SELECT c[""ContactName""]
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (SUBSTRING(c[""CustomerID""], @__start_0, LENGTH(c[""CustomerID""])) = ""FKI""))");
        }

        public override async Task Substring_with_two_args_with_zero_startindex(bool async)
        {
            await base.Substring_with_two_args_with_zero_startindex(async);

            AssertSql(
                @"SELECT LEFT(c[""ContactName""], 3) AS c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Substring_with_two_args_with_zero_length(bool async)
        {
            await base.Substring_with_two_args_with_zero_length(async);

            AssertSql(
                @"SELECT SUBSTRING(c[""ContactName""], 2, 0) AS c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Substring_with_two_args_with_constant(bool async)
        {
            await base.Substring_with_two_args_with_constant(async);

            AssertSql(
                @"SELECT SUBSTRING(c[""ContactName""], 1, 3) AS c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Substring_with_two_args_with_closure(bool async)
        {
            await base.Substring_with_two_args_with_closure(async);

            AssertSql(
                @"@__start_0='2'

SELECT SUBSTRING(c[""ContactName""], @__start_0, 3) AS c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Substring_with_two_args_with_Index_of(bool async)
        {
            await base.Substring_with_two_args_with_Index_of(async);

            AssertSql(
                @"SELECT SUBSTRING(c[""ContactName""], INDEX_OF(c[""ContactName""], ""a""), 3) AS c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task IsNullOrEmpty_in_predicate(bool async)
        {
            await base.IsNullOrEmpty_in_predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task IsNullOrWhiteSpace_in_predicate_on_non_nullable_column(bool async)
        {
            return base.IsNullOrWhiteSpace_in_predicate_on_non_nullable_column(async);
        }

        public override void IsNullOrEmpty_in_projection()
        {
            base.IsNullOrEmpty_in_projection();

            AssertSql(
                @"SELECT c[""CustomerID""], c[""Region""]
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override void IsNullOrEmpty_negated_in_projection()
        {
            base.IsNullOrEmpty_negated_in_projection();

            AssertSql(
                @"SELECT c[""CustomerID""], c[""Region""]
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task IsNullOrWhiteSpace_in_predicate(bool async)
        {
            await base.IsNullOrWhiteSpace_in_predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task TrimStart_without_arguments_in_predicate(bool async)
        {
            await base.TrimStart_without_arguments_in_predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (LTRIM(c[""ContactTitle""]) = ""Owner""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task TrimStart_with_char_argument_in_predicate(bool async)
        {
            await base.TrimStart_with_char_argument_in_predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task TrimStart_with_char_array_argument_in_predicate(bool async)
        {
            await base.TrimStart_with_char_array_argument_in_predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task TrimEnd_without_arguments_in_predicate(bool async)
        {
            await base.TrimEnd_without_arguments_in_predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (RTRIM(c[""ContactTitle""]) = ""Owner""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task TrimEnd_with_char_argument_in_predicate(bool async)
        {
            await base.TrimEnd_with_char_argument_in_predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task TrimEnd_with_char_array_argument_in_predicate(bool async)
        {
            await base.TrimEnd_with_char_array_argument_in_predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Trim_without_argument_in_predicate(bool async)
        {
            await base.Trim_without_argument_in_predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (TRIM(c[""ContactTitle""]) = ""Owner""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Trim_with_char_argument_in_predicate(bool async)
        {
            await base.Trim_with_char_argument_in_predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Trim_with_char_array_argument_in_predicate(bool async)
        {
            await base.Trim_with_char_array_argument_in_predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Order_by_length_twice(bool async)
        {
            await base.Order_by_length_twice(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Order_by_length_twice_followed_by_projection_of_naked_collection_navigation(bool async)
        {
            await base.Order_by_length_twice_followed_by_projection_of_naked_collection_navigation(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Static_string_equals_in_predicate(bool async)
        {
            await base.Static_string_equals_in_predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ANATR""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Static_equals_nullable_datetime_compared_to_non_nullable(bool async)
        {
            await base.Static_equals_nullable_datetime_compared_to_non_nullable(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Static_equals_int_compared_to_long(bool async)
        {
            await base.Static_equals_int_compared_to_long(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Projecting_Math_Truncate_and_ordering_by_it_twice(bool async)
        {
            await base.Projecting_Math_Truncate_and_ordering_by_it_twice(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] < 10250))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Projecting_Math_Truncate_and_ordering_by_it_twice2(bool async)
        {
            await base.Projecting_Math_Truncate_and_ordering_by_it_twice2(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] < 10250))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Projecting_Math_Truncate_and_ordering_by_it_twice3(bool async)
        {
            await base.Projecting_Math_Truncate_and_ordering_by_it_twice3(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] < 10250))");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task DateTime_Compare_to_simple_zero(bool async, bool compareTo)
        {
            return base.DateTime_Compare_to_simple_zero(async, compareTo);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task TimeSpan_Compare_to_simple_zero(bool async, bool compareTo)
        {
            return base.TimeSpan_Compare_to_simple_zero(async, compareTo);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Int_Compare_to_simple_zero(bool async)
        {
            return base.Int_Compare_to_simple_zero(async);
        }

        public override Task Regex_IsMatch_MethodCall(bool async)
        {
            return AssertTranslationFailed(() => base.Regex_IsMatch_MethodCall(async));
        }

        public override Task Regex_IsMatch_MethodCall_constant_input(bool async)
        {
            return AssertTranslationFailed(() => base.Regex_IsMatch_MethodCall_constant_input(async));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Case_insensitive_string_comparison_instance(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.Equals("alFkI", StringComparison.OrdinalIgnoreCase)),
                entryCount: 1);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND STRINGEQUALS(c[""CustomerID""], ""alFkI"", true))");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Case_insensitive_string_comparison_static(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => string.Equals(c.CustomerID, "alFkI", StringComparison.OrdinalIgnoreCase)),
            entryCount: 1);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND STRINGEQUALS(c[""CustomerID""], ""alFkI"", true))");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
