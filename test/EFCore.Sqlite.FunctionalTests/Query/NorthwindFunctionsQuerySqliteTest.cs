// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindFunctionsQuerySqliteTest : NorthwindFunctionsQueryRelationalTestBase<
        NorthwindQuerySqliteFixture<NoopModelCustomizer>>
    {
        public NorthwindFunctionsQuerySqliteTest(
            NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
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

        public override Task Projecting_Math_Truncate_and_ordering_by_it_twice(bool async)
            => AssertTranslationFailed(() => base.Projecting_Math_Truncate_and_ordering_by_it_twice(async));

        public override Task Projecting_Math_Truncate_and_ordering_by_it_twice2(bool async)
            => AssertTranslationFailed(() => base.Projecting_Math_Truncate_and_ordering_by_it_twice2(async));

        public override Task Projecting_Math_Truncate_and_ordering_by_it_twice3(bool async)
            => AssertTranslationFailed(() => base.Projecting_Math_Truncate_and_ordering_by_it_twice3(async));

        public override Task Where_functions_nested(bool async)
            => AssertTranslationFailed(() => base.Where_functions_nested(async));

        public override Task Where_guid_newguid(bool async)
            => AssertTranslationFailed(() => base.Where_guid_newguid(async));

        public override Task Where_math_abs3(bool async)
            => AssertTranslationFailed(() => base.Where_math_abs3(async));

        public override Task Where_math_acos(bool async)
            => AssertTranslationFailed(() => base.Where_math_acos(async));

        public override Task Where_math_asin(bool async)
            => AssertTranslationFailed(() => base.Where_math_asin(async));

        public override Task Where_math_atan(bool async)
            => AssertTranslationFailed(() => base.Where_math_atan(async));

        public override Task Where_math_atan2(bool async)
            => AssertTranslationFailed(() => base.Where_math_atan2(async));

        public override Task Where_math_ceiling1(bool async)
            => AssertTranslationFailed(() => base.Where_math_ceiling1(async));

        public override Task Where_math_ceiling2(bool async)
            => AssertTranslationFailed(() => base.Where_math_ceiling2(async));

        public override Task Where_math_cos(bool async)
            => AssertTranslationFailed(() => base.Where_math_cos(async));

        public override Task Where_math_exp(bool async)
            => AssertTranslationFailed(() => base.Where_math_exp(async));

        public override Task Where_math_floor(bool async)
            => AssertTranslationFailed(() => base.Where_math_floor(async));

        public override Task Where_math_log(bool async)
            => AssertTranslationFailed(() => base.Where_math_log(async));

        public override Task Where_math_log_new_base(bool async)
            => AssertTranslationFailed(() => base.Where_math_log_new_base(async));

        public override Task Where_math_log10(bool async)
            => AssertTranslationFailed(() => base.Where_math_log10(async));

        public override Task Where_math_power(bool async)
            => AssertTranslationFailed(() => base.Where_math_power(async));

        public override Task Where_math_square(bool async)
            => AssertTranslationFailed(() => base.Where_math_square(async));

        public override Task Where_math_round(bool async)
            => AssertTranslationFailed(() => base.Where_math_round(async));

        public override Task Where_math_round2(bool async)
            => AssertTranslationFailed(() => base.Where_math_round2(async));

        public override Task Where_math_sign(bool async)
            => AssertTranslationFailed(() => base.Where_math_sign(async));

        public override Task Where_math_sin(bool async)
            => AssertTranslationFailed(() => base.Where_math_sin(async));

        public override Task Where_math_sqrt(bool async)
            => AssertTranslationFailed(() => base.Where_math_sqrt(async));

        public override Task Where_math_tan(bool async)
            => AssertTranslationFailed(() => base.Where_math_tan(async));

        public override Task Where_math_truncate(bool async)
            => AssertTranslationFailed(() => base.Where_math_truncate(async));

        public override Task Where_mathf_acos(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_acos(async));

        public override Task Where_mathf_asin(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_asin(async));

        public override Task Where_mathf_atan(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_atan(async));

        public override Task Where_mathf_atan2(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_atan2(async));

        public override Task Where_mathf_ceiling1(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_ceiling1(async));

        public override Task Where_mathf_cos(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_cos(async));

        public override Task Where_mathf_exp(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_exp(async));

        public override Task Where_mathf_floor(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_floor(async));

        public override Task Where_mathf_log(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_log(async));

        public override Task Where_mathf_log_new_base(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_log_new_base(async));

        public override Task Where_mathf_log10(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_log10(async));

        public override Task Where_mathf_power(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_power(async));

        public override Task Where_mathf_square(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_square(async));
        public override Task Where_mathf_sign(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_sign(async));

        public override Task Where_mathf_sin(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_sin(async));

        public override Task Where_mathf_sqrt(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_sqrt(async));

        public override Task Where_mathf_tan(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_tan(async));

        public override Task Where_mathf_truncate(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_truncate(async));

        public override async Task String_StartsWith_Literal(bool async)
        {
            await base.String_StartsWith_Literal(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" IS NOT NULL AND (""c"".""ContactName"" LIKE 'M%')");
        }

        public override async Task String_StartsWith_Identity(bool async)
        {
            await base.String_StartsWith_Identity(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (""c"".""ContactName"" = '') OR (""c"".""ContactName"" IS NOT NULL AND (((""c"".""ContactName"" LIKE ""c"".""ContactName"" || '%') AND (substr(""c"".""ContactName"", 1, length(""c"".""ContactName"")) = ""c"".""ContactName"")) OR (""c"".""ContactName"" = '')))");
        }

        public override async Task String_StartsWith_Column(bool async)
        {
            await base.String_StartsWith_Column(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (""c"".""ContactName"" = '') OR (""c"".""ContactName"" IS NOT NULL AND (((""c"".""ContactName"" LIKE ""c"".""ContactName"" || '%') AND (substr(""c"".""ContactName"", 1, length(""c"".""ContactName"")) = ""c"".""ContactName"")) OR (""c"".""ContactName"" = '')))");
        }

        public override async Task String_StartsWith_MethodCall(bool async)
        {
            await base.String_StartsWith_MethodCall(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" IS NOT NULL AND (""c"".""ContactName"" LIKE 'M%')");
        }

        public override async Task String_EndsWith_Literal(bool async)
        {
            await base.String_EndsWith_Literal(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" IS NOT NULL AND (""c"".""ContactName"" LIKE '%b')");
        }

        public override async Task String_EndsWith_Identity(bool async)
        {
            await base.String_EndsWith_Identity(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (""c"".""ContactName"" = '') OR (""c"".""ContactName"" IS NOT NULL AND ((substr(""c"".""ContactName"", -length(""c"".""ContactName"")) = ""c"".""ContactName"") OR (""c"".""ContactName"" = '')))");
        }

        public override async Task String_EndsWith_Column(bool async)
        {
            await base.String_EndsWith_Column(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (""c"".""ContactName"" = '') OR (""c"".""ContactName"" IS NOT NULL AND ((substr(""c"".""ContactName"", -length(""c"".""ContactName"")) = ""c"".""ContactName"") OR (""c"".""ContactName"" = '')))");
        }

        public override async Task String_EndsWith_MethodCall(bool async)
        {
            await base.String_EndsWith_MethodCall(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" IS NOT NULL AND (""c"".""ContactName"" LIKE '%m')");
        }

        public override async Task String_Contains_Literal(bool async)
        {
            await base.String_Contains_Literal(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ('M' = '') OR (instr(""c"".""ContactName"", 'M') > 0)");
        }

        public override async Task String_Contains_Identity(bool async)
        {
            await base.String_Contains_Identity(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (""c"".""ContactName"" = '') OR (instr(""c"".""ContactName"", ""c"".""ContactName"") > 0)");
        }

        public override async Task String_Contains_Column(bool async)
        {
            await base.String_Contains_Column(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (""c"".""ContactName"" = '') OR (instr(""c"".""ContactName"", ""c"".""ContactName"") > 0)");
        }

        public override async Task String_FirstOrDefault_MethodCall(bool async)
        {
            await base.String_FirstOrDefault_MethodCall(async);
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE substr(""c"".""ContactName"", 1, 1) = 'A'");
        }

        public override async Task String_LastOrDefault_MethodCall(bool async)
        {
            await base.String_LastOrDefault_MethodCall(async);
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE substr(""c"".""ContactName"", length(""c"".""ContactName""), 1) = 's'");
        }

        public override async Task String_Contains_MethodCall(bool async)
        {
            await base.String_Contains_MethodCall(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ('M' = '') OR (instr(""c"".""ContactName"", 'M') > 0)");
        }

        public override async Task IsNullOrWhiteSpace_in_predicate(bool async)
        {
            await base.IsNullOrWhiteSpace_in_predicate(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""Region"" IS NULL OR (trim(""c"".""Region"") = '')");
        }

        public override async Task Indexof_with_emptystring(bool async)
        {
            await base.Indexof_with_emptystring(async);

            AssertSql(
                @"SELECT instr(""c"".""ContactName"", '') - 1
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" = 'ALFKI'");
        }

        public override async Task Replace_with_emptystring(bool async)
        {
            await base.Replace_with_emptystring(async);

            AssertSql(
                @"SELECT replace(""c"".""ContactName"", 'ari', '')
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" = 'ALFKI'");
        }

        public override async Task Substring_with_one_arg_with_zero_startindex(bool async)
        {
            await base.Substring_with_one_arg_with_zero_startindex(async);

            AssertSql(
                @"SELECT ""c"".""ContactName""
FROM ""Customers"" AS ""c""
WHERE substr(""c"".""CustomerID"", 0 + 1) = 'ALFKI'");
        }

        public override async Task Substring_with_one_arg_with_constant(bool async)
        {
            await base.Substring_with_one_arg_with_constant(async);

            AssertSql(
                @"SELECT ""c"".""ContactName""
FROM ""Customers"" AS ""c""
WHERE substr(""c"".""CustomerID"", 1 + 1) = 'LFKI'");
        }

        public override async Task Substring_with_one_arg_with_closure(bool async)
        {
            await base.Substring_with_one_arg_with_closure(async);

            AssertSql(
                @"@__start_0='2'

SELECT ""c"".""ContactName""
FROM ""Customers"" AS ""c""
WHERE substr(""c"".""CustomerID"", @__start_0 + 1) = 'FKI'");
        }

        public override async Task Substring_with_two_args_with_zero_startindex(bool async)
        {
            await base.Substring_with_two_args_with_zero_startindex(async);

            AssertSql(
                @"SELECT substr(""c"".""ContactName"", 0 + 1, 3)
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" = 'ALFKI'");
        }

        public override async Task Substring_with_two_args_with_constant(bool async)
        {
            await base.Substring_with_two_args_with_constant(async);

            AssertSql(
                @"SELECT substr(""c"".""ContactName"", 1 + 1, 3)
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" = 'ALFKI'");
        }

        public override async Task Substring_with_two_args_with_closure(bool async)
        {
            await base.Substring_with_two_args_with_closure(async);

            AssertSql(
                @"@__start_0='2'

SELECT substr(""c"".""ContactName"", @__start_0 + 1, 3)
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" = 'ALFKI'");
        }

        public override async Task Substring_with_two_args_with_Index_of(bool async)
        {
            await base.Substring_with_two_args_with_Index_of(async);

            AssertSql(
                @"SELECT substr(""c"".""ContactName"", (instr(""c"".""ContactName"", 'a') - 1) + 1, 3)
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" = 'ALFKI'");
        }

        public override async Task Substring_with_two_args_with_zero_length(bool async)
        {
            await base.Substring_with_two_args_with_zero_length(async);

            AssertSql(
                @"SELECT substr(""c"".""ContactName"", 2 + 1, 0)
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" = 'ALFKI'");
        }

        public override async Task Where_math_abs1(bool async)
        {
            await base.Where_math_abs1(async);

            AssertSql(
                @"SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""SupplierID"", ""p"".""UnitPrice"", ""p"".""UnitsInStock""
FROM ""Products"" AS ""p""
WHERE abs(""p"".""ProductID"") > 10");
        }

        public override async Task Where_math_abs2(bool async)
        {
            await base.Where_math_abs2(async);

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""ProductID"", ""o"".""Discount"", ""o"".""Quantity"", ""o"".""UnitPrice""
FROM ""Order Details"" AS ""o""
WHERE (""o"".""UnitPrice"" < 7.0) AND (abs(""o"".""Quantity"") > 10)");
        }

        public override async Task Where_math_abs_uncorrelated(bool async)
        {
            await base.Where_math_abs_uncorrelated(async);

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""ProductID"", ""o"".""Discount"", ""o"".""Quantity"", ""o"".""UnitPrice""
FROM ""Order Details"" AS ""o""
WHERE (""o"".""UnitPrice"" < 7.0) AND (10 < ""o"".""ProductID"")");
        }

        public override async Task Select_math_round_int(bool async)
        {
            await base.Select_math_round_int(async);

            AssertSql(
                @"SELECT round(CAST(""o"".""OrderID"" AS REAL)) AS ""A""
FROM ""Orders"" AS ""o""
WHERE ""o"".""OrderID"" < 10250");
        }

        public override async Task Where_math_min(bool async)
        {
            await base.Where_math_min(async);

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""ProductID"", ""o"".""Discount"", ""o"".""Quantity"", ""o"".""UnitPrice""
FROM ""Order Details"" AS ""o""
WHERE (""o"".""OrderID"" = 11077) AND (min(""o"".""OrderID"", ""o"".""ProductID"") = ""o"".""ProductID"")");
        }

        public override async Task Where_math_max(bool async)
        {
            await base.Where_math_max(async);

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""ProductID"", ""o"".""Discount"", ""o"".""Quantity"", ""o"".""UnitPrice""
FROM ""Order Details"" AS ""o""
WHERE (""o"".""OrderID"" = 11077) AND (max(""o"".""OrderID"", ""o"".""ProductID"") = ""o"".""OrderID"")");
        }

        public override async Task Where_string_to_lower(bool async)
        {
            await base.Where_string_to_lower(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE lower(""c"".""CustomerID"") = 'alfki'");
        }

        public override async Task Where_string_to_upper(bool async)
        {
            await base.Where_string_to_upper(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE upper(""c"".""CustomerID"") = 'ALFKI'");
        }

        public override async Task TrimStart_without_arguments_in_predicate(bool async)
        {
            await base.TrimStart_without_arguments_in_predicate(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ltrim(""c"".""ContactTitle"") = 'Owner'");
        }

        public override async Task TrimStart_with_char_argument_in_predicate(bool async)
        {
            await base.TrimStart_with_char_argument_in_predicate(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ltrim(""c"".""ContactTitle"", 'O') = 'wner'");
        }

        public override async Task TrimStart_with_char_array_argument_in_predicate(bool async)
        {
            await base.TrimStart_with_char_array_argument_in_predicate(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ltrim(""c"".""ContactTitle"", 'Ow') = 'ner'");
        }

        public override async Task TrimEnd_without_arguments_in_predicate(bool async)
        {
            await base.TrimEnd_without_arguments_in_predicate(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE rtrim(""c"".""ContactTitle"") = 'Owner'");
        }

        public override async Task TrimEnd_with_char_argument_in_predicate(bool async)
        {
            await base.TrimEnd_with_char_argument_in_predicate(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE rtrim(""c"".""ContactTitle"", 'r') = 'Owne'");
        }

        public override async Task TrimEnd_with_char_array_argument_in_predicate(bool async)
        {
            await base.TrimEnd_with_char_array_argument_in_predicate(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE rtrim(""c"".""ContactTitle"", 'er') = 'Own'");
        }

        public override async Task Trim_without_argument_in_predicate(bool async)
        {
            await base.Trim_without_argument_in_predicate(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE trim(""c"".""ContactTitle"") = 'Owner'");
        }

        public override async Task Trim_with_char_argument_in_predicate(bool async)
        {
            await base.Trim_with_char_argument_in_predicate(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE trim(""c"".""ContactTitle"", 'O') = 'wner'");
        }

        public override async Task Trim_with_char_array_argument_in_predicate(bool async)
        {
            await base.Trim_with_char_array_argument_in_predicate(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE trim(""c"".""ContactTitle"", 'Or') = 'wne'");
        }

        public override async Task Regex_IsMatch_MethodCall(bool async)
        {
            await base.Regex_IsMatch_MethodCall(async);

            AssertSql(@"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE regexp('^T', ""c"".""CustomerID"")");
        }

        public override async Task Regex_IsMatch_MethodCall_constant_input(bool async)
        {
            await base.Regex_IsMatch_MethodCall_constant_input(async);

            AssertSql(@"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE regexp(""c"".""CustomerID"", 'ALFKI')");
        }

        public override async Task IsNullOrEmpty_in_predicate(bool async)
        {
            await base.IsNullOrEmpty_in_predicate(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""Region"" IS NULL OR (""c"".""Region"" = '')");
        }

        public override async Task IsNullOrEmpty_in_projection(bool async)
        {
            await base.IsNullOrEmpty_in_projection(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"" AS ""Id"", ""c"".""Region"" IS NULL OR (""c"".""Region"" = '') AS ""Value""
FROM ""Customers"" AS ""c""");
        }

        public override async Task IsNullOrEmpty_negated_in_predicate(bool async)
        {
            await base.IsNullOrEmpty_negated_in_predicate(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""Region"" IS NOT NULL AND (""c"".""Region"" <> '')");
        }

        public override Task Datetime_subtraction_TotalDays(bool async)
        {
            return AssertTranslationFailed(() => base.Datetime_subtraction_TotalDays(async));
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
