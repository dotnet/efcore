// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

        public override void Take_Skip()
        {
            base.Take_Skip();

            AssertSql(
                @"@__p_0='10' (DbType = String)
@__p_1='5' (DbType = String)

SELECT ""t"".*
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
    FROM ""Customers"" AS ""c""
    ORDER BY ""c"".""ContactName""
    LIMIT @__p_0
) AS ""t""
ORDER BY ""t"".""ContactName""
LIMIT -1 OFFSET @__p_1");
        }

        public override void String_StartsWith_Literal()
        {
            base.String_StartsWith_Literal();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE 'M' || '%' AND (substr(""c"".""ContactName"", 1, length('M')) = 'M')");
        }

        public override void String_StartsWith_Identity()
        {
            base.String_StartsWith_Identity();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (""c"".""ContactName"" LIKE ""c"".""ContactName"" || '%' AND (substr(""c"".""ContactName"", 1, length(""c"".""ContactName"")) = ""c"".""ContactName"")) OR (""c"".""ContactName"" = '')");
        }

        public override void String_StartsWith_Column()
        {
            base.String_StartsWith_Column();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (""c"".""ContactName"" LIKE ""c"".""ContactName"" || '%' AND (substr(""c"".""ContactName"", 1, length(""c"".""ContactName"")) = ""c"".""ContactName"")) OR (""c"".""ContactName"" = '')");
        }

        public override void String_StartsWith_MethodCall()
        {
            base.String_StartsWith_MethodCall();

            AssertSql(
                @"@__LocalMethod1_0='M' (Size = 1)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (""c"".""ContactName"" LIKE @__LocalMethod1_0 || '%' AND (substr(""c"".""ContactName"", 1, length(@__LocalMethod1_0)) = @__LocalMethod1_0)) OR (@__LocalMethod1_0 = '')");
        }

        public override void String_EndsWith_Literal()
        {
            base.String_EndsWith_Literal();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE substr(""c"".""ContactName"", -length('b')) = 'b'");
        }

        public override void String_EndsWith_Identity()
        {
            base.String_EndsWith_Identity();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (substr(""c"".""ContactName"", -length(""c"".""ContactName"")) = ""c"".""ContactName"") OR (""c"".""ContactName"" = '')");
        }

        public override void String_EndsWith_Column()
        {
            base.String_EndsWith_Column();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (substr(""c"".""ContactName"", -length(""c"".""ContactName"")) = ""c"".""ContactName"") OR (""c"".""ContactName"" = '')");
        }

        public override void String_EndsWith_MethodCall()
        {
            base.String_EndsWith_MethodCall();

            AssertSql(
                @"@__LocalMethod2_0='m' (Size = 1)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (substr(""c"".""ContactName"", -length(@__LocalMethod2_0)) = @__LocalMethod2_0) OR (@__LocalMethod2_0 = '')");
        }

        public override void String_Contains_Literal()
        {
            base.String_Contains_Literal();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE instr(""c"".""ContactName"", 'M') > 0");
        }

        public override void String_Contains_Identity()
        {
            base.String_Contains_Identity();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (instr(""c"".""ContactName"", ""c"".""ContactName"") > 0) OR (""c"".""ContactName"" = '')");
        }

        public override void String_Contains_Column()
        {
            base.String_Contains_Column();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (instr(""c"".""ContactName"", ""c"".""ContactName"") > 0) OR (""c"".""ContactName"" = '')");
        }

        public override void String_Contains_MethodCall()
        {
            base.String_Contains_MethodCall();

            AssertSql(
                @"@__LocalMethod1_0='M' (Size = 1)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (instr(""c"".""ContactName"", @__LocalMethod1_0) > 0) OR (@__LocalMethod1_0 = '')");
        }

        public override void IsNullOrWhiteSpace_in_predicate()
        {
            base.IsNullOrWhiteSpace_in_predicate();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""Region"" IS NULL OR (trim(""c"".""Region"") = '')");
        }

        public override void Where_string_length()
        {
            base.Where_string_length();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE length(""c"".""City"") = 6");
        }

        public override void Where_string_indexof()
        {
            base.Where_string_indexof();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (instr(""c"".""City"", 'Sea') - 1) <> -1");
        }

        public override void Where_string_replace()
        {
            base.Where_string_replace();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE replace(""c"".""City"", 'Sea', 'Rea') = 'Reattle'");
        }

        public override void Where_string_substring()
        {
            base.Where_string_substring();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE substr(""c"".""City"", 2, 2) = 'ea'");
        }

        public override void Where_math_abs1()
        {
            base.Where_math_abs1();

            AssertSql(
                @"SELECT ""od"".""OrderID"", ""od"".""ProductID"", ""od"".""Discount"", ""od"".""Quantity"", ""od"".""UnitPrice""
FROM ""Order Details"" AS ""od""
WHERE abs(""od"".""ProductID"") > 10");
        }

        public override void Where_math_abs2()
        {
            base.Where_math_abs2();

            AssertSql(
                @"SELECT ""od"".""OrderID"", ""od"".""ProductID"", ""od"".""Discount"", ""od"".""Quantity"", ""od"".""UnitPrice""
FROM ""Order Details"" AS ""od""
WHERE abs(""od"".""Quantity"") > 10");
        }

        public override void Where_math_abs3()
        {
            base.Where_math_abs3();

            AssertSql(
                @"SELECT ""od"".""OrderID"", ""od"".""ProductID"", ""od"".""Discount"", ""od"".""Quantity"", ""od"".""UnitPrice""
FROM ""Order Details"" AS ""od""
WHERE abs(""od"".""UnitPrice"") > 10.0");
        }

        public override void Where_math_abs_uncorrelated()
        {
            base.Where_math_abs_uncorrelated();

            AssertSql(
                @"@__Abs_0='10' (DbType = String)

SELECT ""od"".""OrderID"", ""od"".""ProductID"", ""od"".""Discount"", ""od"".""Quantity"", ""od"".""UnitPrice""
FROM ""Order Details"" AS ""od""
WHERE @__Abs_0 < ""od"".""ProductID""");
        }

        public override void Where_string_to_lower()
        {
            base.Where_string_to_lower();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE lower(""c"".""CustomerID"") = 'alfki'");
        }

        public override void Where_string_to_upper()
        {
            base.Where_string_to_upper();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE upper(""c"".""CustomerID"") = 'ALFKI'");
        }

        public override void TrimStart_without_arguments_in_predicate()
        {
            base.TrimStart_without_arguments_in_predicate();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ltrim(""c"".""ContactTitle"") = 'Owner'");
        }

        public override void TrimStart_with_char_argument_in_predicate()
        {
            base.TrimStart_with_char_argument_in_predicate();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ltrim(""c"".""ContactTitle"", 'O') = 'wner'");
        }

        public override void TrimStart_with_char_array_argument_in_predicate()
        {
            base.TrimStart_with_char_array_argument_in_predicate();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ltrim(""c"".""ContactTitle"", 'Ow') = 'ner'");
        }

        public override void TrimEnd_without_arguments_in_predicate()
        {
            base.TrimEnd_without_arguments_in_predicate();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE rtrim(""c"".""ContactTitle"") = 'Owner'");
        }

        public override void TrimEnd_with_char_argument_in_predicate()
        {
            base.TrimEnd_with_char_argument_in_predicate();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE rtrim(""c"".""ContactTitle"", 'r') = 'Owne'");
        }

        public override void TrimEnd_with_char_array_argument_in_predicate()
        {
            base.TrimEnd_with_char_array_argument_in_predicate();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE rtrim(""c"".""ContactTitle"", 'er') = 'Own'");
        }

        public override void Trim_without_argument_in_predicate()
        {
            base.Trim_without_argument_in_predicate();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE trim(""c"".""ContactTitle"") = 'Owner'");
        }

        public override void Trim_with_char_argument_in_predicate()
        {
            base.Trim_with_char_argument_in_predicate();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE trim(""c"".""ContactTitle"", 'O') = 'wner'");
        }

        public override void Trim_with_char_array_argument_in_predicate()
        {
            base.Trim_with_char_array_argument_in_predicate();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE trim(""c"".""ContactTitle"", 'Or') = 'wne'");
        }

        public override void Sum_with_coalesce()
        {
            base.Sum_with_coalesce();

            AssertSql(
                @"SELECT SUM(COALESCE(""p"".""UnitPrice"", 0.0))
FROM ""Products"" AS ""p""
WHERE ""p"".""ProductID"" < 40");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
