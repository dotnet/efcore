// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class SimpleQueryCosmosTest
    {
        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_StartsWith_Literal(bool isAsync)
        {
            await base.String_StartsWith_Literal(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_StartsWith_Identity(bool isAsync)
        {
            await base.String_StartsWith_Identity(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_StartsWith_Column(bool isAsync)
        {
            await base.String_StartsWith_Column(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_StartsWith_MethodCall(bool isAsync)
        {
            await base.String_StartsWith_MethodCall(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_EndsWith_Literal(bool isAsync)
        {
            await base.String_EndsWith_Literal(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_EndsWith_Identity(bool isAsync)
        {
            await base.String_EndsWith_Identity(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_EndsWith_Column(bool isAsync)
        {
            await base.String_EndsWith_Column(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_EndsWith_MethodCall(bool isAsync)
        {
            await base.String_EndsWith_MethodCall(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_Contains_Literal(bool isAsync)
        {
            await base.String_Contains_Literal(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_Contains_Identity(bool isAsync)
        {
            await base.String_Contains_Identity(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_Contains_Column(bool isAsync)
        {
            await base.String_Contains_Column(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_Contains_MethodCall(bool isAsync)
        {
            await base.String_Contains_MethodCall(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_Compare_simple_zero(bool isAsync)
        {
            await base.String_Compare_simple_zero(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_Compare_simple_one(bool isAsync)
        {
            await base.String_Compare_simple_one(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_compare_with_parameter(bool isAsync)
        {
            await base.String_compare_with_parameter(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_Compare_simple_more_than_one(bool isAsync)
        {
            await base.String_Compare_simple_more_than_one(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_Compare_nested(bool isAsync)
        {
            await base.String_Compare_nested(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_Compare_multi_predicate(bool isAsync)
        {
            await base.String_Compare_multi_predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_Compare_to_simple_zero(bool isAsync)
        {
            await base.String_Compare_to_simple_zero(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_Compare_to_simple_one(bool isAsync)
        {
            await base.String_Compare_to_simple_one(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_compare_to_with_parameter(bool isAsync)
        {
            await base.String_compare_to_with_parameter(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_Compare_to_simple_more_than_one(bool isAsync)
        {
            await base.String_Compare_to_simple_more_than_one(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_Compare_to_nested(bool isAsync)
        {
            await base.String_Compare_to_nested(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_Compare_to_multi_predicate(bool isAsync)
        {
            await base.String_Compare_to_multi_predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_abs1(bool isAsync)
        {
            await base.Where_math_abs1(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_abs2(bool isAsync)
        {
            await base.Where_math_abs2(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_abs3(bool isAsync)
        {
            await base.Where_math_abs3(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        public override async Task Where_math_abs_uncorrelated(bool isAsync)
        {
            await base.Where_math_abs_uncorrelated(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (10 < c[""ProductID""]))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_ceiling1(bool isAsync)
        {
            await base.Where_math_ceiling1(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_ceiling2(bool isAsync)
        {
            await base.Where_math_ceiling2(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_floor(bool isAsync)
        {
            await base.Where_math_floor(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_power(bool isAsync)
        {
            await base.Where_math_power(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_round(bool isAsync)
        {
            await base.Where_math_round(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        public override async Task Select_math_round_int(bool isAsync)
        {
            await base.Select_math_round_int(isAsync);

            AssertSql(
                @"SELECT c[""OrderID""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] < 10250))");
        }

        public override async Task Select_math_truncate_int(bool isAsync)
        {
            await base.Select_math_truncate_int(isAsync);

            AssertSql(
                @"SELECT c[""OrderID""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] < 10250))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_round2(bool isAsync)
        {
            await base.Where_math_round2(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_truncate(bool isAsync)
        {
            await base.Where_math_truncate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_exp(bool isAsync)
        {
            await base.Where_math_exp(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] = 11077))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_log10(bool isAsync)
        {
            await base.Where_math_log10(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND ((c[""OrderID""] = 11077) AND (c[""Discount""] > 0.0)))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_log(bool isAsync)
        {
            await base.Where_math_log(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND ((c[""OrderID""] = 11077) AND (c[""Discount""] > 0.0)))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_log_new_base(bool isAsync)
        {
            await base.Where_math_log_new_base(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND ((c[""OrderID""] = 11077) AND (c[""Discount""] > 0.0)))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_sqrt(bool isAsync)
        {
            await base.Where_math_sqrt(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] = 11077))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_acos(bool isAsync)
        {
            await base.Where_math_acos(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] = 11077))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_asin(bool isAsync)
        {
            await base.Where_math_asin(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] = 11077))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_atan(bool isAsync)
        {
            await base.Where_math_atan(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] = 11077))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_atan2(bool isAsync)
        {
            await base.Where_math_atan2(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] = 11077))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_cos(bool isAsync)
        {
            await base.Where_math_cos(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] = 11077))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_sin(bool isAsync)
        {
            await base.Where_math_sin(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] = 11077))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_tan(bool isAsync)
        {
            await base.Where_math_tan(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] = 11077))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_sign(bool isAsync)
        {
            await base.Where_math_sign(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] = 11077))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_min(bool isAsync)
        {
            await base.Where_math_min(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] = 11077))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_math_max(bool isAsync)
        {
            await base.Where_math_max(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] = 11077))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_guid_newguid(bool isAsync)
        {
            await base.Where_guid_newguid(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_string_to_upper(bool isAsync)
        {
            await base.Where_string_to_upper(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_string_to_lower(bool isAsync)
        {
            await base.Where_string_to_lower(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_functions_nested(bool isAsync)
        {
            await base.Where_functions_nested(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Convert_ToByte(bool isAsync)
        {
            await base.Convert_ToByte(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Convert_ToDecimal(bool isAsync)
        {
            await base.Convert_ToDecimal(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Convert_ToDouble(bool isAsync)
        {
            await base.Convert_ToDouble(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Convert_ToInt16(bool isAsync)
        {
            await base.Convert_ToInt16(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Convert_ToInt32(bool isAsync)
        {
            await base.Convert_ToInt32(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Convert_ToInt64(bool isAsync)
        {
            await base.Convert_ToInt64(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Convert_ToString(bool isAsync)
        {
            await base.Convert_ToString(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Indexof_with_emptystring(bool isAsync)
        {
            await base.Indexof_with_emptystring(isAsync);

            AssertSql(
                @"SELECT c[""ContactName""]
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Replace_with_emptystring(bool isAsync)
        {
            await base.Replace_with_emptystring(isAsync);

            AssertSql(
                @"SELECT c[""ContactName""]
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Substring_with_zero_startindex(bool isAsync)
        {
            await base.Substring_with_zero_startindex(isAsync);

            AssertSql(
                @"SELECT c[""ContactName""]
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Substring_with_zero_length(bool isAsync)
        {
            await base.Substring_with_zero_length(isAsync);

            AssertSql(
                @"SELECT c[""ContactName""]
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Substring_with_constant(bool isAsync)
        {
            await base.Substring_with_constant(isAsync);

            AssertSql(
                @"SELECT c[""ContactName""]
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Substring_with_closure(bool isAsync)
        {
            await base.Substring_with_closure(isAsync);

            AssertSql(
                @"SELECT c[""ContactName""]
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Substring_with_Index_of(bool isAsync)
        {
            await base.Substring_with_Index_of(isAsync);

            AssertSql(
                @"SELECT c[""ContactName""]
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task IsNullOrEmpty_in_predicate(bool isAsync)
        {
            await base.IsNullOrEmpty_in_predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
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
        public override async Task IsNullOrWhiteSpace_in_predicate(bool isAsync)
        {
            await base.IsNullOrWhiteSpace_in_predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task TrimStart_without_arguments_in_predicate(bool isAsync)
        {
            await base.TrimStart_without_arguments_in_predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task TrimStart_with_char_argument_in_predicate(bool isAsync)
        {
            await base.TrimStart_with_char_argument_in_predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task TrimStart_with_char_array_argument_in_predicate(bool isAsync)
        {
            await base.TrimStart_with_char_array_argument_in_predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task TrimEnd_without_arguments_in_predicate(bool isAsync)
        {
            await base.TrimEnd_without_arguments_in_predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task TrimEnd_with_char_argument_in_predicate(bool isAsync)
        {
            await base.TrimEnd_with_char_argument_in_predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task TrimEnd_with_char_array_argument_in_predicate(bool isAsync)
        {
            await base.TrimEnd_with_char_array_argument_in_predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Trim_without_argument_in_predicate(bool isAsync)
        {
            await base.Trim_without_argument_in_predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Trim_with_char_argument_in_predicate(bool isAsync)
        {
            await base.Trim_with_char_argument_in_predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Trim_with_char_array_argument_in_predicate(bool isAsync)
        {
            await base.Trim_with_char_array_argument_in_predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Order_by_length_twice(bool isAsync)
        {
            await base.Order_by_length_twice(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Order_by_length_twice_followed_by_projection_of_naked_collection_navigation(bool isAsync)
        {
            await base.Order_by_length_twice_followed_by_projection_of_naked_collection_navigation(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Static_string_equals_in_predicate(bool isAsync)
        {
            await base.Static_string_equals_in_predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ANATR""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Static_equals_nullable_datetime_compared_to_non_nullable(bool isAsync)
        {
            await base.Static_equals_nullable_datetime_compared_to_non_nullable(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Static_equals_int_compared_to_long(bool isAsync)
        {
            await base.Static_equals_int_compared_to_long(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Projecting_Math_Truncate_and_ordering_by_it_twice(bool isAsync)
        {
            await base.Projecting_Math_Truncate_and_ordering_by_it_twice(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] < 10250))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Projecting_Math_Truncate_and_ordering_by_it_twice2(bool isAsync)
        {
            await base.Projecting_Math_Truncate_and_ordering_by_it_twice2(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] < 10250))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Projecting_Math_Truncate_and_ordering_by_it_twice3(bool isAsync)
        {
            await base.Projecting_Math_Truncate_and_ordering_by_it_twice3(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] < 10250))");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task DateTime_Compare_to_simple_zero(bool isAsync, bool compareTo)
        {
            return base.DateTime_Compare_to_simple_zero(isAsync, compareTo);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task TimeSpan_Compare_to_simple_zero(bool isAsync, bool compareTo)
        {
            return base.TimeSpan_Compare_to_simple_zero(isAsync, compareTo);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Int_Compare_to_simple_zero(bool isAsync)
        {
            return base.Int_Compare_to_simple_zero(isAsync);
        }
    }
}
