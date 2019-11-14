// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class SimpleQuerySqliteTest : SimpleQueryTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
    {
        // ReSharper disable once UnusedParameter.Local
        public SimpleQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        // SQLite client-eval
        public override async Task Sum_with_division_on_decimal(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Sum_with_division_on_decimal(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Sum_with_division_on_decimal_no_significant_digits(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Sum_with_division_on_decimal_no_significant_digits(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Average_with_division_on_decimal(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Average_with_division_on_decimal(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Average_with_division_on_decimal_no_significant_digits(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Average_with_division_on_decimal_no_significant_digits(async)))
                .Message);
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
        public override async Task Query_expression_with_to_string_and_contains(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Query_expression_with_to_string_and_contains(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Where_datetimeoffset_now_component(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_datetimeoffset_now_component(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Where_datetimeoffset_utcnow_component(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_datetimeoffset_utcnow_component(async)))
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

        public override async Task Take_Skip(bool async)
        {
            await base.Take_Skip(async);

            AssertSql(
                @"@__p_0='10' (DbType = String)
@__p_1='5' (DbType = String)

SELECT ""t"".""CustomerID"", ""t"".""Address"", ""t"".""City"", ""t"".""CompanyName"", ""t"".""ContactName"", ""t"".""ContactTitle"", ""t"".""Country"", ""t"".""Fax"", ""t"".""Phone"", ""t"".""PostalCode"", ""t"".""Region""
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
    FROM ""Customers"" AS ""c""
    ORDER BY ""c"".""ContactName""
    LIMIT @__p_0
) AS ""t""
ORDER BY ""t"".""ContactName""
LIMIT -1 OFFSET @__p_1");
        }

        public override async Task Where_datetime_now(bool async)
        {
            await base.Where_datetime_now(async);

            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = String)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime'), '0'), '.') <> @__myDatetime_0) OR rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime'), '0'), '.') IS NULL");
        }

        public override async Task Where_datetime_utcnow(bool async)
        {
            await base.Where_datetime_utcnow(async);

            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = String)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', 'now'), '0'), '.') <> @__myDatetime_0) OR rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', 'now'), '0'), '.') IS NULL");
        }

        public override async Task Where_datetime_today(bool async)
        {
            await base.Where_datetime_today(async);

            AssertSql(
                @"SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE (rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime', 'start of day'), '0'), '.') = rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime', 'start of day'), '0'), '.')) OR rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime', 'start of day'), '0'), '.') IS NULL");
        }

        public override async Task Where_datetime_date_component(bool async)
        {
            await base.Where_datetime_date_component(async);

            AssertSql(
                @"@__myDatetime_0='1998-05-04T00:00:00' (DbType = String)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', ""o"".""OrderDate"", 'start of day'), '0'), '.') = @__myDatetime_0");
        }

        public override async Task Where_datetime_year_component(bool async)
        {
            await base.Where_datetime_year_component(async);

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(strftime('%Y', ""o"".""OrderDate"") AS INTEGER) = 1998");
        }

        public override async Task Where_datetime_month_component(bool async)
        {
            await base.Where_datetime_month_component(async);

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(strftime('%m', ""o"".""OrderDate"") AS INTEGER) = 4");
        }

        public override async Task Where_datetime_dayOfYear_component(bool async)
        {
            await base.Where_datetime_dayOfYear_component(async);

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(strftime('%j', ""o"".""OrderDate"") AS INTEGER) = 68");
        }

        public override async Task Where_datetime_day_component(bool async)
        {
            await base.Where_datetime_day_component(async);

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(strftime('%d', ""o"".""OrderDate"") AS INTEGER) = 4");
        }

        public override async Task Where_datetime_hour_component(bool async)
        {
            await base.Where_datetime_hour_component(async);

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(strftime('%H', ""o"".""OrderDate"") AS INTEGER) = 14");
        }

        public override async Task Where_datetime_minute_component(bool async)
        {
            await base.Where_datetime_minute_component(async);

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(strftime('%M', ""o"".""OrderDate"") AS INTEGER) = 23");
        }

        public override async Task Where_datetime_second_component(bool async)
        {
            await base.Where_datetime_second_component(async);

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(strftime('%S', ""o"".""OrderDate"") AS INTEGER) = 44");
        }

        [ConditionalTheory(Skip = "Issue#15586")]
        public override async Task Where_datetime_millisecond_component(bool async)
        {
            await base.Where_datetime_millisecond_component(async);

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE ((CAST(strftime('%f', ""o"".""OrderDate"") AS REAL) * 1000) % 1000) = 88");
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

        public override async Task Where_string_length(bool async)
        {
            await base.Where_string_length(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE length(""c"".""City"") = 6");
        }

        public override async Task Where_string_indexof(bool async)
        {
            await base.Where_string_indexof(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ((instr(""c"".""City"", 'Sea') - 1) <> -1) OR instr(""c"".""City"", 'Sea') IS NULL");
        }

        public override async Task Indexof_with_emptystring(bool async)
        {
            await base.Indexof_with_emptystring(async);

            AssertSql(
                @"SELECT instr(""c"".""ContactName"", '') - 1
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" = 'ALFKI'");
        }

        public override async Task Where_string_replace(bool async)
        {
            await base.Where_string_replace(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE replace(""c"".""City"", 'Sea', 'Rea') = 'Reattle'");
        }

        public override async Task Replace_with_emptystring(bool async)
        {
            await base.Replace_with_emptystring(async);

            AssertSql(
                @"SELECT replace(""c"".""ContactName"", 'ari', '')
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" = 'ALFKI'");
        }

        public override async Task Where_string_substring(bool async)
        {
            await base.Where_string_substring(async);

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE substr(""c"".""City"", 1 + 1, 2) = 'ea'");
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

        public override async Task Select_datetime_year_component(bool async)
        {
            await base.Select_datetime_year_component(async);

            AssertSql(
                @"SELECT CAST(strftime('%Y', ""o"".""OrderDate"") AS INTEGER)
FROM ""Orders"" AS ""o""");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Select_datetime_year_component_composed(bool async)
        {
            await AssertQueryScalar(
                async,
                ss => ss.Set<Order>().Select(o => o.OrderDate.Value.AddYears(1).Year));

            AssertSql(
                @"SELECT CAST(strftime('%Y', ""o"".""OrderDate"", CAST(1 AS TEXT) || ' years') AS INTEGER)
FROM ""Orders"" AS ""o""");
        }

        public override async Task Select_datetime_month_component(bool async)
        {
            await base.Select_datetime_month_component(async);

            AssertSql(
                @"SELECT CAST(strftime('%m', ""o"".""OrderDate"") AS INTEGER)
FROM ""Orders"" AS ""o""");
        }

        public override async Task Select_datetime_day_of_year_component(bool async)
        {
            await base.Select_datetime_day_of_year_component(async);

            AssertSql(
                @"SELECT CAST(strftime('%j', ""o"".""OrderDate"") AS INTEGER)
FROM ""Orders"" AS ""o""");
        }

        public override async Task Select_datetime_day_component(bool async)
        {
            await base.Select_datetime_day_component(async);

            AssertSql(
                @"SELECT CAST(strftime('%d', ""o"".""OrderDate"") AS INTEGER)
FROM ""Orders"" AS ""o""");
        }

        public override async Task Select_datetime_hour_component(bool async)
        {
            await base.Select_datetime_hour_component(async);

            AssertSql(
                @"SELECT CAST(strftime('%H', ""o"".""OrderDate"") AS INTEGER)
FROM ""Orders"" AS ""o""");
        }

        public override async Task Select_datetime_minute_component(bool async)
        {
            await base.Select_datetime_minute_component(async);

            AssertSql(
                @"SELECT CAST(strftime('%M', ""o"".""OrderDate"") AS INTEGER)
FROM ""Orders"" AS ""o""");
        }

        public override async Task Select_datetime_second_component(bool async)
        {
            await base.Select_datetime_second_component(async);

            AssertSql(
                @"SELECT CAST(strftime('%S', ""o"".""OrderDate"") AS INTEGER)
FROM ""Orders"" AS ""o""");
        }

        public override async Task Select_datetime_millisecond_component(bool async)
        {
            await base.Select_datetime_millisecond_component(async);

            AssertSql(
                @"SELECT (CAST(strftime('%f', ""o"".""OrderDate"") AS REAL) * 1000.0) % 1000.0
FROM ""Orders"" AS ""o""");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Select_datetime_millisecond_component_composed(bool async)
        {
            await AssertQueryScalar(
                async,
                ss => ss.Set<Order>().Select(o => o.OrderDate.Value.AddYears(1).Millisecond));

            AssertSql(
                @"SELECT (CAST(strftime('%f', ""o"".""OrderDate"", CAST(1 AS TEXT) || ' years') AS REAL) * 1000.0) % 1000.0
FROM ""Orders"" AS ""o""");
        }

        public override async Task Select_datetime_DayOfWeek_component(bool async)
        {
            await base.Select_datetime_DayOfWeek_component(async);

            AssertSql(
                @"SELECT CAST(CAST(strftime('%w', ""o"".""OrderDate"") AS INTEGER) AS INTEGER)
FROM ""Orders"" AS ""o""");
        }

        public override async Task Select_datetime_Ticks_component(bool async)
        {
            await base.Select_datetime_Ticks_component(async);

            AssertSql(
                @"SELECT CAST(((julianday(""o"".""OrderDate"") - 1721425.5) * 864000000000.0) AS INTEGER)
FROM ""Orders"" AS ""o""");
        }

        public override async Task Select_datetime_TimeOfDay_component(bool async)
        {
            await base.Select_datetime_TimeOfDay_component(async);

            AssertSql(
                @"SELECT rtrim(rtrim(strftime('%H:%M:%f', ""o"".""OrderDate""), '0'), '.')
FROM ""Orders"" AS ""o""");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Select_datetime_TimeOfDay_component_composed(bool async)
        {
            await AssertQueryScalar(
                async,
                ss => ss.Set<Order>().Select(o => o.OrderDate.Value.AddYears(1).TimeOfDay));

            AssertSql(
                @"SELECT rtrim(rtrim(strftime('%H:%M:%f', ""o"".""OrderDate"", CAST(1 AS TEXT) || ' years'), '0'), '.')
FROM ""Orders"" AS ""o""");
        }

        public override async Task Select_expression_date_add_year(bool async)
        {
            await base.Select_expression_date_add_year(async);

            AssertSql(
                @"SELECT rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', ""o"".""OrderDate"", CAST(1 AS TEXT) || ' years'), '0'), '.') AS ""OrderDate""
FROM ""Orders"" AS ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL");
        }

        public override async Task Select_expression_datetime_add_month(bool async)
        {
            await base.Select_expression_datetime_add_month(async);

            AssertSql(
                @"SELECT rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', ""o"".""OrderDate"", CAST(1 AS TEXT) || ' months'), '0'), '.') AS ""OrderDate""
FROM ""Orders"" AS ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL");
        }

        public override async Task Select_expression_datetime_add_hour(bool async)
        {
            await base.Select_expression_datetime_add_hour(async);

            AssertSql(
                @"SELECT rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', ""o"".""OrderDate"", CAST(1.0 AS TEXT) || ' hours'), '0'), '.') AS ""OrderDate""
FROM ""Orders"" AS ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL");
        }

        public override async Task Select_expression_datetime_add_minute(bool async)
        {
            await base.Select_expression_datetime_add_minute(async);

            AssertSql(
                @"SELECT rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', ""o"".""OrderDate"", CAST(1.0 AS TEXT) || ' minutes'), '0'), '.') AS ""OrderDate""
FROM ""Orders"" AS ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL");
        }

        public override async Task Select_expression_datetime_add_second(bool async)
        {
            await base.Select_expression_datetime_add_second(async);

            AssertSql(
                @"SELECT rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', ""o"".""OrderDate"", CAST(1.0 AS TEXT) || ' seconds'), '0'), '.') AS ""OrderDate""
FROM ""Orders"" AS ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL");
        }

        public override async Task Select_expression_datetime_add_ticks(bool async)
        {
            await base.Select_expression_datetime_add_ticks(async);

            AssertSql(
                @"SELECT rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', ""o"".""OrderDate"", CAST((10000 / 864000000000) AS TEXT) || ' seconds'), '0'), '.') AS ""OrderDate""
FROM ""Orders"" AS ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL");
        }

        public override async Task Select_expression_date_add_milliseconds_above_the_range(bool async)
        {
            await base.Select_expression_date_add_milliseconds_above_the_range(async);

            AssertSql(
                @"SELECT rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', ""o"".""OrderDate"", CAST((1000000000000.0 / 1000.0) AS TEXT) || ' seconds'), '0'), '.') AS ""OrderDate""
FROM ""Orders"" AS ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL");
        }

        public override async Task Select_expression_date_add_milliseconds_below_the_range(bool async)
        {
            await base.Select_expression_date_add_milliseconds_below_the_range(async);

            AssertSql(
                @"SELECT rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', ""o"".""OrderDate"", CAST((-1000000000000.0 / 1000.0) AS TEXT) || ' seconds'), '0'), '.') AS ""OrderDate""
FROM ""Orders"" AS ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL");
        }

        public override async Task Select_expression_date_add_milliseconds_large_number_divided(bool async)
        {
            await base.Select_expression_date_add_milliseconds_large_number_divided(async);

            AssertSql(
                @"@__millisecondsPerDay_0='86400000' (DbType = String)

SELECT rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', ""o"".""OrderDate"", CAST(CAST((CAST(((CAST(strftime('%f', ""o"".""OrderDate"") AS REAL) * 1000.0) % 1000.0) AS INTEGER) / @__millisecondsPerDay_0) AS REAL) AS TEXT) || ' days', CAST((CAST((CAST(((CAST(strftime('%f', ""o"".""OrderDate"") AS REAL) * 1000.0) % 1000.0) AS INTEGER) % @__millisecondsPerDay_0) AS REAL) / 1000.0) AS TEXT) || ' seconds'), '0'), '.') AS ""OrderDate""
FROM ""Orders"" AS ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL");
        }

        public override async Task Decimal_cast_to_double_works(bool async)
        {
            await base.Decimal_cast_to_double_works(async);

            AssertSql(
                @"SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""SupplierID"", ""p"".""UnitPrice"", ""p"".""UnitsInStock""
FROM ""Products"" AS ""p""
WHERE CAST(""p"".""UnitPrice"" AS REAL) > 100.0");
        }

        public override async Task Select_distinct_long_count(bool async)
        {
            await base.Select_distinct_long_count(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
    FROM ""Customers"" AS ""c""
) AS ""t""");
        }

        public override async Task Select_orderBy_skip_long_count(bool async)
        {
            await base.Select_orderBy_skip_long_count(async);

            AssertSql(
                @"@__p_0='7' (DbType = String)

SELECT COUNT(*)
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
    FROM ""Customers"" AS ""c""
    ORDER BY ""c"".""Country""
    LIMIT -1 OFFSET @__p_0
) AS ""t""");
        }

        public override async Task Select_orderBy_take_long_count(bool async)
        {
            await base.Select_orderBy_take_long_count(async);

            AssertSql(
                @"@__p_0='7' (DbType = String)

SELECT COUNT(*)
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
    FROM ""Customers"" AS ""c""
    ORDER BY ""c"".""Country""
    LIMIT @__p_0
) AS ""t""");
        }

        public override async Task Select_skip_long_count(bool async)
        {
            await base.Select_skip_long_count(async);

            AssertSql(
                @"@__p_0='7' (DbType = String)

SELECT COUNT(*)
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
    FROM ""Customers"" AS ""c""
    ORDER BY (SELECT 1)
    LIMIT -1 OFFSET @__p_0
) AS ""t""");
        }

        public override async Task Select_take_long_count(bool async)
        {
            await base.Select_take_long_count(async);

            AssertSql(
                @"@__p_0='7' (DbType = String)

SELECT COUNT(*)
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
    FROM ""Customers"" AS ""c""
    LIMIT @__p_0
) AS ""t""");
        }

        [ConditionalTheory(Skip = "SQLite bug")]
        public override Task Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_2(bool async)
            => base.Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_2(async);

        // Sqlite does not support cross/outer apply
        public override void Select_nested_collection_multi_level()
        {
        }

        public override Task SelectMany_correlated_with_outer_1(bool async) => null;

        public override Task SelectMany_correlated_with_outer_2(bool async) => null;

        public override Task SelectMany_correlated_with_outer_3(bool async) => null;

        public override Task SelectMany_correlated_with_outer_4(bool async) => null;

        public override Task Complex_nested_query_doesnt_try_binding_to_grandparent_when_parent_returns_complex_result(bool async)
            => null;

        public override Task SelectMany_correlated_subquery_hard(bool async) => null;

        public override Task SelectMany_whose_selector_references_outer_source(bool async) => null;

        public override Task AsQueryable_in_query_server_evals(bool async) => null;

        [ConditionalTheory(Skip = "Issue#17324")]
        public override Task Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(bool async)
        {
            return base.Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(async);
        }

        [ConditionalTheory(Skip = "Issue#17223")]
        public override Task Like_with_non_string_column_using_ToString(bool async)
        {
            return base.Like_with_non_string_column_using_ToString(async);
        }

        public override Task Member_binding_after_ctor_arguments_fails_with_client_eval(bool async)
        {
            return AssertTranslationFailed(() => base.Member_binding_after_ctor_arguments_fails_with_client_eval(async));
        }

        [ConditionalTheory(Skip = "Issue#17230")]
        public override Task SelectMany_with_collection_being_correlated_subquery_which_references_inner_and_outer_entity(bool async)
        {
            return base.SelectMany_with_collection_being_correlated_subquery_which_references_inner_and_outer_entity(async);
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
