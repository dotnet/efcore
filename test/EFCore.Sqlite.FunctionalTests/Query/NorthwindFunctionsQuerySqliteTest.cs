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

    public override Task Byte_Parse(bool async)
        => AssertTranslationFailed(() => base.Byte_Parse(async));

    public override Task Byte_Parse_Non_Numeric_Bad_Format(bool async)
        => AssertTranslationFailed(() => base.Byte_Parse_Non_Numeric_Bad_Format(async));

    public override Task Byte_Parse_Greater_Than_Max_Value_Overflows(bool async)
        => AssertTranslationFailed(() => base.Byte_Parse_Greater_Than_Max_Value_Overflows(async));

    public override Task Byte_Parse_Negative_Overflows(bool async)
        => AssertTranslationFailed(() => base.Byte_Parse_Negative_Overflows(async));

    public override Task Byte_Parse_Decimal_Bad_Format(bool async)
        => AssertTranslationFailed(() => base.Byte_Parse_Decimal_Bad_Format(async));

    public override Task Decimal_Parse(bool async)
        => AssertTranslationFailed(() => base.Decimal_Parse(async));

    public override Task Decimal_Parse_Non_Numeric_Bad_Format(bool async)
        => AssertTranslationFailed(() => base.Decimal_Parse_Non_Numeric_Bad_Format(async));

    public override Task Double_Parse(bool async)
        => AssertTranslationFailed(() => base.Double_Parse(async));

    public override Task Double_Parse_Non_Numeric_Bad_Format(bool async)
        => AssertTranslationFailed(() => base.Double_Parse_Non_Numeric_Bad_Format(async));

    public override Task Short_Parse(bool async)
        => AssertTranslationFailed(() => base.Short_Parse(async));

    public override Task Short_Parse_Non_Numeric_Bad_Format(bool async)
        => AssertTranslationFailed(() => base.Short_Parse_Non_Numeric_Bad_Format(async));

    public override Task Short_Parse_Greater_Than_Max_Value_Overflows(bool async)
        => AssertTranslationFailed(() => base.Short_Parse_Greater_Than_Max_Value_Overflows(async));

    public override Task Short_Parse_Decimal_Bad_Format(bool async)
        => AssertTranslationFailed(() => base.Short_Parse_Decimal_Bad_Format(async));

    public override Task Int_Parse(bool async)
        => AssertTranslationFailed(() => base.Int_Parse(async));

    public override Task Int_Parse_Non_Numeric_Bad_Format(bool async)
        => AssertTranslationFailed(() => base.Int_Parse_Non_Numeric_Bad_Format(async));

    public override Task Int_Parse_Decimal_Bad_Format(bool async)
        => AssertTranslationFailed(() => base.Int_Parse_Decimal_Bad_Format(async));

    public override Task Long_Parse(bool async)
        => AssertTranslationFailed(() => base.Long_Parse(async));

    public override Task Long_Parse_Non_Numeric_Bad_Format(bool async)
        => AssertTranslationFailed(() => base.Long_Parse_Non_Numeric_Bad_Format(async));

    public override Task Long_Parse_Decimal_Bad_Format(bool async)
        => AssertTranslationFailed(() => base.Long_Parse_Decimal_Bad_Format(async));

    public override async Task Client_evaluation_of_uncorrelated_method_call(bool async)
    {
        await base.Client_evaluation_of_uncorrelated_method_call(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE "o"."UnitPrice" < 7.0 AND 10 < "o"."ProductID"
""");
    }

    public override async Task Order_by_length_twice(bool async)
    {
        await base.Order_by_length_twice(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
ORDER BY length("c"."CustomerID"), "c"."CustomerID"
""");
    }

    public override async Task Order_by_length_twice_followed_by_projection_of_naked_collection_navigation(bool async)
    {
        await base.Order_by_length_twice_followed_by_projection_of_naked_collection_navigation(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
FROM "Customers" AS "c"
LEFT JOIN "Orders" AS "o" ON "c"."CustomerID" = "o"."CustomerID"
ORDER BY length("c"."CustomerID"), "c"."CustomerID"
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

    public override Task Sum_over_round_works_correctly_in_projection(bool async)
        => AssertTranslationFailed(() => base.Sum_over_round_works_correctly_in_projection(async));

    public override Task Sum_over_round_works_correctly_in_projection_2(bool async)
        => AssertTranslationFailed(() => base.Sum_over_round_works_correctly_in_projection_2(async));

    public override Task Sum_over_truncate_works_correctly_in_projection(bool async)
        => AssertTranslationFailed(() => base.Sum_over_truncate_works_correctly_in_projection(async));

    public override Task Sum_over_truncate_works_correctly_in_projection_2(bool async)
        => AssertTranslationFailed(() => base.Sum_over_truncate_works_correctly_in_projection_2(async));

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
