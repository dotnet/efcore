// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindFunctionsQuerySqliteTest : NorthwindFunctionsQueryTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
    {
        public NorthwindFunctionsQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        // SQLite client-eval
        public override async Task Convert_ToByte(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Convert_ToByte(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Convert_ToDecimal(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Convert_ToDecimal(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Convert_ToDouble(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Convert_ToDouble(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Convert_ToInt16(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Convert_ToInt16(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Convert_ToInt32(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Convert_ToInt32(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Convert_ToInt64(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Convert_ToInt64(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Convert_ToString(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Convert_ToString(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Projecting_Math_Truncate_and_ordering_by_it_twice(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Projecting_Math_Truncate_and_ordering_by_it_twice(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Projecting_Math_Truncate_and_ordering_by_it_twice2(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Projecting_Math_Truncate_and_ordering_by_it_twice2(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Projecting_Math_Truncate_and_ordering_by_it_twice3(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Projecting_Math_Truncate_and_ordering_by_it_twice3(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Where_functions_nested(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_functions_nested(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Where_guid_newguid(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_guid_newguid(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Where_math_abs3(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_math_abs3(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Where_math_acos(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_math_acos(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Where_math_asin(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_math_asin(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Where_math_atan(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_math_atan(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Where_math_atan2(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_math_atan2(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Where_math_ceiling1(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_math_ceiling1(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Where_math_ceiling2(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_math_ceiling2(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Where_math_cos(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_math_cos(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Where_math_exp(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_math_exp(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Where_math_floor(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_math_floor(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Where_math_log(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_math_log(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Where_math_log_new_base(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_math_log_new_base(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Where_math_log10(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_math_log10(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Where_math_power(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_math_power(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Where_math_round(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_math_round(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Where_math_round2(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_math_round2(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Where_math_sign(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_math_sign(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Where_math_sin(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_math_sin(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Where_math_sqrt(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_math_sqrt(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Where_math_tan(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_math_tan(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Where_math_truncate(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_math_truncate(async)))
                .Message);
        }

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
WHERE (""c"".""ContactName"" = '') OR (""c"".""ContactName"" IS NOT NULL AND (""c"".""ContactName"" IS NOT NULL AND (((""c"".""ContactName"" LIKE ""c"".""ContactName"" || '%') AND (substr(""c"".""ContactName"", 1, length(""c"".""ContactName"")) = ""c"".""ContactName"")) OR (""c"".""ContactName"" = ''))))");
        }

        public override async Task String_StartsWith_Column(bool async)
        {
            await base.String_StartsWith_Column(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (""c"".""ContactName"" = '') OR (""c"".""ContactName"" IS NOT NULL AND (""c"".""ContactName"" IS NOT NULL AND (((""c"".""ContactName"" LIKE ""c"".""ContactName"" || '%') AND (substr(""c"".""ContactName"", 1, length(""c"".""ContactName"")) = ""c"".""ContactName"")) OR (""c"".""ContactName"" = ''))))");
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
WHERE (""c"".""ContactName"" = '') OR (""c"".""ContactName"" IS NOT NULL AND (""c"".""ContactName"" IS NOT NULL AND ((substr(""c"".""ContactName"", -length(""c"".""ContactName"")) = ""c"".""ContactName"") OR (""c"".""ContactName"" = ''))))");
        }

        public override async Task String_EndsWith_Column(bool async)
        {
            await base.String_EndsWith_Column(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (""c"".""ContactName"" = '') OR (""c"".""ContactName"" IS NOT NULL AND (""c"".""ContactName"" IS NOT NULL AND ((substr(""c"".""ContactName"", -length(""c"".""ContactName"")) = ""c"".""ContactName"") OR (""c"".""ContactName"" = ''))))");
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

        public override async Task Substring_with_zero_startindex(bool async)
        {
            await base.Substring_with_zero_startindex(async);

            AssertSql(
                @"SELECT substr(""c"".""ContactName"", 0 + 1, 3)
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" = 'ALFKI'");
        }

        public override async Task Substring_with_constant(bool async)
        {
            await base.Substring_with_constant(async);

            AssertSql(
                @"SELECT substr(""c"".""ContactName"", 1 + 1, 3)
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" = 'ALFKI'");
        }

        public override async Task Substring_with_closure(bool async)
        {
            await base.Substring_with_closure(async);

            AssertSql(
                @"@__start_0='2' (DbType = String)

SELECT substr(""c"".""ContactName"", @__start_0 + 1, 3)
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" = 'ALFKI'");
        }

        public override async Task Substring_with_Index_of(bool async)
        {
            await base.Substring_with_Index_of(async);

            AssertSql(
                @"SELECT substr(""c"".""ContactName"", (instr(""c"".""ContactName"", 'a') - 1) + 1, 3)
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" = 'ALFKI'");
        }

        public override async Task Substring_with_zero_length(bool async)
        {
            await base.Substring_with_zero_length(async);

            AssertSql(
                @"SELECT substr(""c"".""ContactName"", 2 + 1, 0)
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" = 'ALFKI'");
        }

        public override async Task Where_math_abs1(bool async)
        {
            await base.Where_math_abs1(async);

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""ProductID"", ""o"".""Discount"", ""o"".""Quantity"", ""o"".""UnitPrice""
FROM ""Order Details"" AS ""o""
WHERE abs(""o"".""ProductID"") > 10");
        }

        public override async Task Where_math_abs2(bool async)
        {
            await base.Where_math_abs2(async);

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""ProductID"", ""o"".""Discount"", ""o"".""Quantity"", ""o"".""UnitPrice""
FROM ""Order Details"" AS ""o""
WHERE abs(""o"".""Quantity"") > 10");
        }

        public override async Task Where_math_abs_uncorrelated(bool async)
        {
            await base.Where_math_abs_uncorrelated(async);

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""ProductID"", ""o"".""Discount"", ""o"".""Quantity"", ""o"".""UnitPrice""
FROM ""Order Details"" AS ""o""
WHERE 10 < ""o"".""ProductID""");
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

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
