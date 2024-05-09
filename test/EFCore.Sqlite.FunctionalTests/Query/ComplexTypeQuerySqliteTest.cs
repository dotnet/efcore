// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class ComplexTypeQuerySqliteTest : ComplexTypeQueryRelationalTestBase<
    ComplexTypeQuerySqliteTest.ComplexTypeQuerySqliteFixture>
{
    public ComplexTypeQuerySqliteTest(ComplexTypeQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Filter_on_property_inside_complex_type(bool async)
    {
        await base.Filter_on_property_inside_complex_type(async);

        AssertSql(
            """
SELECT "c"."Id", "c"."Name", "c"."BillingAddress_AddressLine1", "c"."BillingAddress_AddressLine2", "c"."BillingAddress_ZipCode", "c"."BillingAddress_Country_Code", "c"."BillingAddress_Country_FullName", "c"."ShippingAddress_AddressLine1", "c"."ShippingAddress_AddressLine2", "c"."ShippingAddress_ZipCode", "c"."ShippingAddress_Country_Code", "c"."ShippingAddress_Country_FullName"
FROM "Customer" AS "c"
WHERE "c"."ShippingAddress_ZipCode" = 7728
""");
    }

    public override async Task Filter_on_property_inside_nested_complex_type(bool async)
    {
        await base.Filter_on_property_inside_nested_complex_type(async);

        AssertSql(
            """
SELECT "c"."Id", "c"."Name", "c"."BillingAddress_AddressLine1", "c"."BillingAddress_AddressLine2", "c"."BillingAddress_ZipCode", "c"."BillingAddress_Country_Code", "c"."BillingAddress_Country_FullName", "c"."ShippingAddress_AddressLine1", "c"."ShippingAddress_AddressLine2", "c"."ShippingAddress_ZipCode", "c"."ShippingAddress_Country_Code", "c"."ShippingAddress_Country_FullName"
FROM "Customer" AS "c"
WHERE "c"."ShippingAddress_Country_Code" = 'DE'
""");
    }

    public override async Task Filter_on_property_inside_complex_type_after_subquery(bool async)
    {
        await base.Filter_on_property_inside_complex_type_after_subquery(async);

        AssertSql(
            """
@__p_0='1'

SELECT DISTINCT "t"."Id", "t"."Name", "t"."BillingAddress_AddressLine1", "t"."BillingAddress_AddressLine2", "t"."BillingAddress_ZipCode", "t"."BillingAddress_Country_Code", "t"."BillingAddress_Country_FullName", "t"."ShippingAddress_AddressLine1", "t"."ShippingAddress_AddressLine2", "t"."ShippingAddress_ZipCode", "t"."ShippingAddress_Country_Code", "t"."ShippingAddress_Country_FullName"
FROM (
    SELECT "c"."Id", "c"."Name", "c"."BillingAddress_AddressLine1", "c"."BillingAddress_AddressLine2", "c"."BillingAddress_ZipCode", "c"."BillingAddress_Country_Code", "c"."BillingAddress_Country_FullName", "c"."ShippingAddress_AddressLine1", "c"."ShippingAddress_AddressLine2", "c"."ShippingAddress_ZipCode", "c"."ShippingAddress_Country_Code", "c"."ShippingAddress_Country_FullName"
    FROM "Customer" AS "c"
    ORDER BY "c"."Id"
    LIMIT -1 OFFSET @__p_0
) AS "t"
WHERE "t"."ShippingAddress_ZipCode" = 7728
""");
    }

    public override async Task Filter_on_property_inside_nested_complex_type_after_subquery(bool async)
    {
        await base.Filter_on_property_inside_nested_complex_type_after_subquery(async);

        AssertSql(
            """
@__p_0='1'

SELECT DISTINCT "t"."Id", "t"."Name", "t"."BillingAddress_AddressLine1", "t"."BillingAddress_AddressLine2", "t"."BillingAddress_ZipCode", "t"."BillingAddress_Country_Code", "t"."BillingAddress_Country_FullName", "t"."ShippingAddress_AddressLine1", "t"."ShippingAddress_AddressLine2", "t"."ShippingAddress_ZipCode", "t"."ShippingAddress_Country_Code", "t"."ShippingAddress_Country_FullName"
FROM (
    SELECT "c"."Id", "c"."Name", "c"."BillingAddress_AddressLine1", "c"."BillingAddress_AddressLine2", "c"."BillingAddress_ZipCode", "c"."BillingAddress_Country_Code", "c"."BillingAddress_Country_FullName", "c"."ShippingAddress_AddressLine1", "c"."ShippingAddress_AddressLine2", "c"."ShippingAddress_ZipCode", "c"."ShippingAddress_Country_Code", "c"."ShippingAddress_Country_FullName"
    FROM "Customer" AS "c"
    ORDER BY "c"."Id"
    LIMIT -1 OFFSET @__p_0
) AS "t"
WHERE "t"."ShippingAddress_Country_Code" = 'DE'
""");
    }

    public override async Task Filter_on_required_property_inside_required_complex_type_on_optional_navigation(bool async)
    {
        await base.Filter_on_required_property_inside_required_complex_type_on_optional_navigation(async);

        AssertSql(
            """
SELECT "c"."Id", "c"."OptionalCustomerId", "c"."RequiredCustomerId", "c0"."Id", "c0"."Name", "c0"."BillingAddress_AddressLine1", "c0"."BillingAddress_AddressLine2", "c0"."BillingAddress_ZipCode", "c0"."BillingAddress_Country_Code", "c0"."BillingAddress_Country_FullName", "c0"."ShippingAddress_AddressLine1", "c0"."ShippingAddress_AddressLine2", "c0"."ShippingAddress_ZipCode", "c0"."ShippingAddress_Country_Code", "c0"."ShippingAddress_Country_FullName", "c1"."Id", "c1"."Name", "c1"."BillingAddress_AddressLine1", "c1"."BillingAddress_AddressLine2", "c1"."BillingAddress_ZipCode", "c1"."BillingAddress_Country_Code", "c1"."BillingAddress_Country_FullName", "c1"."ShippingAddress_AddressLine1", "c1"."ShippingAddress_AddressLine2", "c1"."ShippingAddress_ZipCode", "c1"."ShippingAddress_Country_Code", "c1"."ShippingAddress_Country_FullName"
FROM "CustomerGroup" AS "c"
LEFT JOIN "Customer" AS "c0" ON "c"."OptionalCustomerId" = "c0"."Id"
INNER JOIN "Customer" AS "c1" ON "c"."RequiredCustomerId" = "c1"."Id"
WHERE "c0"."ShippingAddress_ZipCode" <> 7728 OR "c0"."ShippingAddress_ZipCode" IS NULL
""");
    }

    public override async Task Filter_on_required_property_inside_required_complex_type_on_required_navigation(bool async)
    {
        await base.Filter_on_required_property_inside_required_complex_type_on_required_navigation(async);

        AssertSql(
            """
SELECT "c"."Id", "c"."OptionalCustomerId", "c"."RequiredCustomerId", "c1"."Id", "c1"."Name", "c1"."BillingAddress_AddressLine1", "c1"."BillingAddress_AddressLine2", "c1"."BillingAddress_ZipCode", "c1"."BillingAddress_Country_Code", "c1"."BillingAddress_Country_FullName", "c1"."ShippingAddress_AddressLine1", "c1"."ShippingAddress_AddressLine2", "c1"."ShippingAddress_ZipCode", "c1"."ShippingAddress_Country_Code", "c1"."ShippingAddress_Country_FullName", "c0"."Id", "c0"."Name", "c0"."BillingAddress_AddressLine1", "c0"."BillingAddress_AddressLine2", "c0"."BillingAddress_ZipCode", "c0"."BillingAddress_Country_Code", "c0"."BillingAddress_Country_FullName", "c0"."ShippingAddress_AddressLine1", "c0"."ShippingAddress_AddressLine2", "c0"."ShippingAddress_ZipCode", "c0"."ShippingAddress_Country_Code", "c0"."ShippingAddress_Country_FullName"
FROM "CustomerGroup" AS "c"
INNER JOIN "Customer" AS "c0" ON "c"."RequiredCustomerId" = "c0"."Id"
LEFT JOIN "Customer" AS "c1" ON "c"."OptionalCustomerId" = "c1"."Id"
WHERE "c0"."ShippingAddress_ZipCode" <> 7728
""");
    }

    // This test fails because when OptionalCustomer is null, we get all-null results because of the LEFT JOIN, and we materialize this
    // as an empty ShippingAddress instead of null (see SQL). The proper solution here would be to project the Customer ID just for the
    // purpose of knowing that it's there.
    public override async Task Project_complex_type_via_optional_navigation(bool async)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Project_complex_type_via_optional_navigation(async));

        Assert.Equal(RelationalStrings.CannotProjectNullableComplexType("Customer.ShippingAddress#Address"), exception.Message);
    }

    public override async Task Project_complex_type_via_required_navigation(bool async)
    {
        await base.Project_complex_type_via_required_navigation(async);

        AssertSql(
            """
SELECT "c0"."ShippingAddress_AddressLine1", "c0"."ShippingAddress_AddressLine2", "c0"."ShippingAddress_ZipCode", "c0"."ShippingAddress_Country_Code", "c0"."ShippingAddress_Country_FullName"
FROM "CustomerGroup" AS "c"
INNER JOIN "Customer" AS "c0" ON "c"."RequiredCustomerId" = "c0"."Id"
""");
    }

    public override async Task Load_complex_type_after_subquery_on_entity_type(bool async)
    {
        await base.Load_complex_type_after_subquery_on_entity_type(async);

        AssertSql(
            """
@__p_0='1'

SELECT DISTINCT "t"."Id", "t"."Name", "t"."BillingAddress_AddressLine1", "t"."BillingAddress_AddressLine2", "t"."BillingAddress_ZipCode", "t"."BillingAddress_Country_Code", "t"."BillingAddress_Country_FullName", "t"."ShippingAddress_AddressLine1", "t"."ShippingAddress_AddressLine2", "t"."ShippingAddress_ZipCode", "t"."ShippingAddress_Country_Code", "t"."ShippingAddress_Country_FullName"
FROM (
    SELECT "c"."Id", "c"."Name", "c"."BillingAddress_AddressLine1", "c"."BillingAddress_AddressLine2", "c"."BillingAddress_ZipCode", "c"."BillingAddress_Country_Code", "c"."BillingAddress_Country_FullName", "c"."ShippingAddress_AddressLine1", "c"."ShippingAddress_AddressLine2", "c"."ShippingAddress_ZipCode", "c"."ShippingAddress_Country_Code", "c"."ShippingAddress_Country_FullName"
    FROM "Customer" AS "c"
    ORDER BY "c"."Id"
    LIMIT -1 OFFSET @__p_0
) AS "t"
""");
    }

    public override async Task Select_complex_type(bool async)
    {
        await base.Select_complex_type(async);

        AssertSql(
            """
SELECT "c"."ShippingAddress_AddressLine1", "c"."ShippingAddress_AddressLine2", "c"."ShippingAddress_ZipCode", "c"."ShippingAddress_Country_Code", "c"."ShippingAddress_Country_FullName"
FROM "Customer" AS "c"
""");
    }

    public override async Task Select_nested_complex_type(bool async)
    {
        await base.Select_nested_complex_type(async);

        AssertSql(
            """
SELECT "c"."ShippingAddress_Country_Code", "c"."ShippingAddress_Country_FullName"
FROM "Customer" AS "c"
""");
    }

    public override async Task Select_single_property_on_nested_complex_type(bool async)
    {
        await base.Select_single_property_on_nested_complex_type(async);

        AssertSql(
            """
SELECT "c"."ShippingAddress_Country_FullName"
FROM "Customer" AS "c"
""");
    }

    public override async Task Select_complex_type_Where(bool async)
    {
        await base.Select_complex_type_Where(async);

        AssertSql(
            """
SELECT "c"."ShippingAddress_AddressLine1", "c"."ShippingAddress_AddressLine2", "c"."ShippingAddress_ZipCode", "c"."ShippingAddress_Country_Code", "c"."ShippingAddress_Country_FullName"
FROM "Customer" AS "c"
WHERE "c"."ShippingAddress_ZipCode" = 7728
""");
    }

    public override async Task Select_complex_type_Distinct(bool async)
    {
        await base.Select_complex_type_Distinct(async);

        AssertSql(
            """
SELECT DISTINCT "c"."ShippingAddress_AddressLine1", "c"."ShippingAddress_AddressLine2", "c"."ShippingAddress_ZipCode", "c"."ShippingAddress_Country_Code", "c"."ShippingAddress_Country_FullName"
FROM "Customer" AS "c"
""");
    }

    public override async Task Complex_type_equals_complex_type(bool async)
    {
        await base.Complex_type_equals_complex_type(async);

        AssertSql(
            """
SELECT "c"."Id", "c"."Name", "c"."BillingAddress_AddressLine1", "c"."BillingAddress_AddressLine2", "c"."BillingAddress_ZipCode", "c"."BillingAddress_Country_Code", "c"."BillingAddress_Country_FullName", "c"."ShippingAddress_AddressLine1", "c"."ShippingAddress_AddressLine2", "c"."ShippingAddress_ZipCode", "c"."ShippingAddress_Country_Code", "c"."ShippingAddress_Country_FullName"
FROM "Customer" AS "c"
WHERE "c"."ShippingAddress_AddressLine1" = "c"."BillingAddress_AddressLine1" AND ("c"."ShippingAddress_AddressLine2" = "c"."BillingAddress_AddressLine2" OR ("c"."ShippingAddress_AddressLine2" IS NULL AND "c"."BillingAddress_AddressLine2" IS NULL)) AND "c"."ShippingAddress_ZipCode" = "c"."BillingAddress_ZipCode"
""");
    }

    public override async Task Complex_type_equals_constant(bool async)
    {
        await base.Complex_type_equals_constant(async);

        AssertSql(
            """
SELECT "c"."Id", "c"."Name", "c"."BillingAddress_AddressLine1", "c"."BillingAddress_AddressLine2", "c"."BillingAddress_ZipCode", "c"."BillingAddress_Country_Code", "c"."BillingAddress_Country_FullName", "c"."ShippingAddress_AddressLine1", "c"."ShippingAddress_AddressLine2", "c"."ShippingAddress_ZipCode", "c"."ShippingAddress_Country_Code", "c"."ShippingAddress_Country_FullName"
FROM "Customer" AS "c"
WHERE "c"."ShippingAddress_AddressLine1" = '804 S. Lakeshore Road' AND "c"."ShippingAddress_AddressLine2" IS NULL AND "c"."ShippingAddress_ZipCode" = 38654 AND "c"."ShippingAddress_Country_Code" = 'US' AND "c"."ShippingAddress_Country_FullName" = 'United States'
""");
    }

    public override async Task Complex_type_equals_parameter(bool async)
    {
        await base.Complex_type_equals_parameter(async);

        AssertSql(
            """
@__entity_equality_address_0_AddressLine1='804 S. Lakeshore Road' (Size = 21)
@__entity_equality_address_0_ZipCode='38654' (Nullable = true)
@__entity_equality_address_0_Country_Code='US' (Size = 2)
@__entity_equality_address_0_Country_FullName='United States' (Size = 13)

SELECT "c"."Id", "c"."Name", "c"."BillingAddress_AddressLine1", "c"."BillingAddress_AddressLine2", "c"."BillingAddress_ZipCode", "c"."BillingAddress_Country_Code", "c"."BillingAddress_Country_FullName", "c"."ShippingAddress_AddressLine1", "c"."ShippingAddress_AddressLine2", "c"."ShippingAddress_ZipCode", "c"."ShippingAddress_Country_Code", "c"."ShippingAddress_Country_FullName"
FROM "Customer" AS "c"
WHERE "c"."ShippingAddress_AddressLine1" = @__entity_equality_address_0_AddressLine1 AND "c"."ShippingAddress_AddressLine2" IS NULL AND "c"."ShippingAddress_ZipCode" = @__entity_equality_address_0_ZipCode AND "c"."ShippingAddress_Country_Code" = @__entity_equality_address_0_Country_Code AND "c"."ShippingAddress_Country_FullName" = @__entity_equality_address_0_Country_FullName
""");
    }

    public override async Task Complex_type_equals_null(bool async)
    {
        await base.Complex_type_equals_null(async);

        AssertSql();
    }

    public override async Task Subquery_over_complex_type(bool async)
    {
        await base.Subquery_over_complex_type(async);

        AssertSql();
    }

    public override async Task Contains_over_complex_type(bool async)
    {
        await base.Contains_over_complex_type(async);

        AssertSql(
            """
@__entity_equality_address_0_AddressLine1='804 S. Lakeshore Road' (Size = 21)
@__entity_equality_address_0_ZipCode='38654' (Nullable = true)
@__entity_equality_address_0_Country_Code='US' (Size = 2)
@__entity_equality_address_0_Country_FullName='United States' (Size = 13)

SELECT "c"."Id", "c"."Name", "c"."BillingAddress_AddressLine1", "c"."BillingAddress_AddressLine2", "c"."BillingAddress_ZipCode", "c"."BillingAddress_Country_Code", "c"."BillingAddress_Country_FullName", "c"."ShippingAddress_AddressLine1", "c"."ShippingAddress_AddressLine2", "c"."ShippingAddress_ZipCode", "c"."ShippingAddress_Country_Code", "c"."ShippingAddress_Country_FullName"
FROM "Customer" AS "c"
WHERE EXISTS (
    SELECT 1
    FROM "Customer" AS "c0"
    WHERE "c0"."ShippingAddress_AddressLine1" = @__entity_equality_address_0_AddressLine1 AND "c0"."ShippingAddress_AddressLine2" IS NULL AND "c0"."ShippingAddress_ZipCode" = @__entity_equality_address_0_ZipCode AND "c0"."ShippingAddress_Country_Code" = @__entity_equality_address_0_Country_Code AND "c0"."ShippingAddress_Country_FullName" = @__entity_equality_address_0_Country_FullName)
""");
    }

    public override async Task Concat_complex_type(bool async)
    {
        await base.Concat_complex_type(async);

        AssertSql(
            """
SELECT "c"."ShippingAddress_AddressLine1", "c"."ShippingAddress_AddressLine2", "c"."ShippingAddress_ZipCode", "c"."ShippingAddress_Country_Code", "c"."ShippingAddress_Country_FullName"
FROM "Customer" AS "c"
WHERE "c"."Id" = 1
UNION ALL
SELECT "c0"."ShippingAddress_AddressLine1", "c0"."ShippingAddress_AddressLine2", "c0"."ShippingAddress_ZipCode", "c0"."ShippingAddress_Country_Code", "c0"."ShippingAddress_Country_FullName"
FROM "Customer" AS "c0"
WHERE "c0"."Id" = 2
""");
    }

    public override async Task Concat_entity_type_containing_complex_property(bool async)
    {
        await base.Concat_entity_type_containing_complex_property(async);

        AssertSql(
            """
SELECT "c"."Id", "c"."Name", "c"."BillingAddress_AddressLine1", "c"."BillingAddress_AddressLine2", "c"."BillingAddress_ZipCode", "c"."BillingAddress_Country_Code", "c"."BillingAddress_Country_FullName", "c"."ShippingAddress_AddressLine1", "c"."ShippingAddress_AddressLine2", "c"."ShippingAddress_ZipCode", "c"."ShippingAddress_Country_Code", "c"."ShippingAddress_Country_FullName"
FROM "Customer" AS "c"
WHERE "c"."Id" = 1
UNION ALL
SELECT "c0"."Id", "c0"."Name", "c0"."BillingAddress_AddressLine1", "c0"."BillingAddress_AddressLine2", "c0"."BillingAddress_ZipCode", "c0"."BillingAddress_Country_Code", "c0"."BillingAddress_Country_FullName", "c0"."ShippingAddress_AddressLine1", "c0"."ShippingAddress_AddressLine2", "c0"."ShippingAddress_ZipCode", "c0"."ShippingAddress_Country_Code", "c0"."ShippingAddress_Country_FullName"
FROM "Customer" AS "c0"
WHERE "c0"."Id" = 2
""");
    }

    public override async Task Union_entity_type_containing_complex_property(bool async)
    {
        await base.Union_entity_type_containing_complex_property(async);

        AssertSql(
            """
SELECT "c"."Id", "c"."Name", "c"."BillingAddress_AddressLine1", "c"."BillingAddress_AddressLine2", "c"."BillingAddress_ZipCode", "c"."BillingAddress_Country_Code", "c"."BillingAddress_Country_FullName", "c"."ShippingAddress_AddressLine1", "c"."ShippingAddress_AddressLine2", "c"."ShippingAddress_ZipCode", "c"."ShippingAddress_Country_Code", "c"."ShippingAddress_Country_FullName"
FROM "Customer" AS "c"
WHERE "c"."Id" = 1
UNION
SELECT "c0"."Id", "c0"."Name", "c0"."BillingAddress_AddressLine1", "c0"."BillingAddress_AddressLine2", "c0"."BillingAddress_ZipCode", "c0"."BillingAddress_Country_Code", "c0"."BillingAddress_Country_FullName", "c0"."ShippingAddress_AddressLine1", "c0"."ShippingAddress_AddressLine2", "c0"."ShippingAddress_ZipCode", "c0"."ShippingAddress_Country_Code", "c0"."ShippingAddress_Country_FullName"
FROM "Customer" AS "c0"
WHERE "c0"."Id" = 2
""");
    }

    public override async Task Union_complex_type(bool async)
    {
        await base.Union_complex_type(async);

        AssertSql(
            """
SELECT "c"."ShippingAddress_AddressLine1", "c"."ShippingAddress_AddressLine2", "c"."ShippingAddress_ZipCode", "c"."ShippingAddress_Country_Code", "c"."ShippingAddress_Country_FullName"
FROM "Customer" AS "c"
WHERE "c"."Id" = 1
UNION
SELECT "c0"."ShippingAddress_AddressLine1", "c0"."ShippingAddress_AddressLine2", "c0"."ShippingAddress_ZipCode", "c0"."ShippingAddress_Country_Code", "c0"."ShippingAddress_Country_FullName"
FROM "Customer" AS "c0"
WHERE "c0"."Id" = 2
""");
    }

    public override async Task Concat_property_in_complex_type(bool async)
    {
        await base.Concat_property_in_complex_type(async);

        AssertSql(
            """
SELECT "c"."ShippingAddress_AddressLine1"
FROM "Customer" AS "c"
UNION ALL
SELECT "c0"."BillingAddress_AddressLine1" AS "ShippingAddress_AddressLine1"
FROM "Customer" AS "c0"
""");
    }

    public override async Task Union_property_in_complex_type(bool async)
    {
        await base.Union_property_in_complex_type(async);

        AssertSql(
            """
SELECT "c"."ShippingAddress_AddressLine1"
FROM "Customer" AS "c"
UNION
SELECT "c0"."BillingAddress_AddressLine1" AS "ShippingAddress_AddressLine1"
FROM "Customer" AS "c0"
""");
    }

    public override async Task Concat_two_different_complex_type(bool async)
    {
        await base.Concat_two_different_complex_type(async);

        AssertSql();
    }

    public override async Task Union_two_different_complex_type(bool async)
    {
        await base.Union_two_different_complex_type(async);

        AssertSql();
    }

    public override async Task Filter_on_property_inside_struct_complex_type(bool async)
    {
        await base.Filter_on_property_inside_struct_complex_type(async);

        AssertSql(
            """
SELECT "v"."Id", "v"."Name", "v"."BillingAddress_AddressLine1", "v"."BillingAddress_AddressLine2", "v"."BillingAddress_ZipCode", "v"."BillingAddress_Country_Code", "v"."BillingAddress_Country_FullName", "v"."ShippingAddress_AddressLine1", "v"."ShippingAddress_AddressLine2", "v"."ShippingAddress_ZipCode", "v"."ShippingAddress_Country_Code", "v"."ShippingAddress_Country_FullName"
FROM "ValuedCustomer" AS "v"
WHERE "v"."ShippingAddress_ZipCode" = 7728
""");
    }

    public override async Task Filter_on_property_inside_nested_struct_complex_type(bool async)
    {
        await base.Filter_on_property_inside_nested_struct_complex_type(async);

        AssertSql(
            """
SELECT "v"."Id", "v"."Name", "v"."BillingAddress_AddressLine1", "v"."BillingAddress_AddressLine2", "v"."BillingAddress_ZipCode", "v"."BillingAddress_Country_Code", "v"."BillingAddress_Country_FullName", "v"."ShippingAddress_AddressLine1", "v"."ShippingAddress_AddressLine2", "v"."ShippingAddress_ZipCode", "v"."ShippingAddress_Country_Code", "v"."ShippingAddress_Country_FullName"
FROM "ValuedCustomer" AS "v"
WHERE "v"."ShippingAddress_Country_Code" = 'DE'
""");
    }

    public override async Task Filter_on_property_inside_struct_complex_type_after_subquery(bool async)
    {
        await base.Filter_on_property_inside_struct_complex_type_after_subquery(async);

        AssertSql(
            """
@__p_0='1'

SELECT DISTINCT "t"."Id", "t"."Name", "t"."BillingAddress_AddressLine1", "t"."BillingAddress_AddressLine2", "t"."BillingAddress_ZipCode", "t"."BillingAddress_Country_Code", "t"."BillingAddress_Country_FullName", "t"."ShippingAddress_AddressLine1", "t"."ShippingAddress_AddressLine2", "t"."ShippingAddress_ZipCode", "t"."ShippingAddress_Country_Code", "t"."ShippingAddress_Country_FullName"
FROM (
    SELECT "v"."Id", "v"."Name", "v"."BillingAddress_AddressLine1", "v"."BillingAddress_AddressLine2", "v"."BillingAddress_ZipCode", "v"."BillingAddress_Country_Code", "v"."BillingAddress_Country_FullName", "v"."ShippingAddress_AddressLine1", "v"."ShippingAddress_AddressLine2", "v"."ShippingAddress_ZipCode", "v"."ShippingAddress_Country_Code", "v"."ShippingAddress_Country_FullName"
    FROM "ValuedCustomer" AS "v"
    ORDER BY "v"."Id"
    LIMIT -1 OFFSET @__p_0
) AS "t"
WHERE "t"."ShippingAddress_ZipCode" = 7728
""");
    }

    public override async Task Filter_on_property_inside_nested_struct_complex_type_after_subquery(bool async)
    {
        await base.Filter_on_property_inside_nested_struct_complex_type_after_subquery(async);

        AssertSql(
            """
@__p_0='1'

SELECT DISTINCT "t"."Id", "t"."Name", "t"."BillingAddress_AddressLine1", "t"."BillingAddress_AddressLine2", "t"."BillingAddress_ZipCode", "t"."BillingAddress_Country_Code", "t"."BillingAddress_Country_FullName", "t"."ShippingAddress_AddressLine1", "t"."ShippingAddress_AddressLine2", "t"."ShippingAddress_ZipCode", "t"."ShippingAddress_Country_Code", "t"."ShippingAddress_Country_FullName"
FROM (
    SELECT "v"."Id", "v"."Name", "v"."BillingAddress_AddressLine1", "v"."BillingAddress_AddressLine2", "v"."BillingAddress_ZipCode", "v"."BillingAddress_Country_Code", "v"."BillingAddress_Country_FullName", "v"."ShippingAddress_AddressLine1", "v"."ShippingAddress_AddressLine2", "v"."ShippingAddress_ZipCode", "v"."ShippingAddress_Country_Code", "v"."ShippingAddress_Country_FullName"
    FROM "ValuedCustomer" AS "v"
    ORDER BY "v"."Id"
    LIMIT -1 OFFSET @__p_0
) AS "t"
WHERE "t"."ShippingAddress_Country_Code" = 'DE'
""");
    }

    public override async Task Filter_on_required_property_inside_required_struct_complex_type_on_optional_navigation(bool async)
    {
        await base.Filter_on_required_property_inside_required_struct_complex_type_on_optional_navigation(async);

        AssertSql(
            """
SELECT "v"."Id", "v"."OptionalCustomerId", "v"."RequiredCustomerId", "v0"."Id", "v0"."Name", "v0"."BillingAddress_AddressLine1", "v0"."BillingAddress_AddressLine2", "v0"."BillingAddress_ZipCode", "v0"."BillingAddress_Country_Code", "v0"."BillingAddress_Country_FullName", "v0"."ShippingAddress_AddressLine1", "v0"."ShippingAddress_AddressLine2", "v0"."ShippingAddress_ZipCode", "v0"."ShippingAddress_Country_Code", "v0"."ShippingAddress_Country_FullName", "v1"."Id", "v1"."Name", "v1"."BillingAddress_AddressLine1", "v1"."BillingAddress_AddressLine2", "v1"."BillingAddress_ZipCode", "v1"."BillingAddress_Country_Code", "v1"."BillingAddress_Country_FullName", "v1"."ShippingAddress_AddressLine1", "v1"."ShippingAddress_AddressLine2", "v1"."ShippingAddress_ZipCode", "v1"."ShippingAddress_Country_Code", "v1"."ShippingAddress_Country_FullName"
FROM "ValuedCustomerGroup" AS "v"
LEFT JOIN "ValuedCustomer" AS "v0" ON "v"."OptionalCustomerId" = "v0"."Id"
INNER JOIN "ValuedCustomer" AS "v1" ON "v"."RequiredCustomerId" = "v1"."Id"
WHERE "v0"."ShippingAddress_ZipCode" <> 7728 OR "v0"."ShippingAddress_ZipCode" IS NULL
""");
    }

    public override async Task Filter_on_required_property_inside_required_struct_complex_type_on_required_navigation(bool async)
    {
        await base.Filter_on_required_property_inside_required_struct_complex_type_on_required_navigation(async);

        AssertSql(
            """
SELECT "v"."Id", "v"."OptionalCustomerId", "v"."RequiredCustomerId", "v1"."Id", "v1"."Name", "v1"."BillingAddress_AddressLine1", "v1"."BillingAddress_AddressLine2", "v1"."BillingAddress_ZipCode", "v1"."BillingAddress_Country_Code", "v1"."BillingAddress_Country_FullName", "v1"."ShippingAddress_AddressLine1", "v1"."ShippingAddress_AddressLine2", "v1"."ShippingAddress_ZipCode", "v1"."ShippingAddress_Country_Code", "v1"."ShippingAddress_Country_FullName", "v0"."Id", "v0"."Name", "v0"."BillingAddress_AddressLine1", "v0"."BillingAddress_AddressLine2", "v0"."BillingAddress_ZipCode", "v0"."BillingAddress_Country_Code", "v0"."BillingAddress_Country_FullName", "v0"."ShippingAddress_AddressLine1", "v0"."ShippingAddress_AddressLine2", "v0"."ShippingAddress_ZipCode", "v0"."ShippingAddress_Country_Code", "v0"."ShippingAddress_Country_FullName"
FROM "ValuedCustomerGroup" AS "v"
INNER JOIN "ValuedCustomer" AS "v0" ON "v"."RequiredCustomerId" = "v0"."Id"
LEFT JOIN "ValuedCustomer" AS "v1" ON "v"."OptionalCustomerId" = "v1"."Id"
WHERE "v0"."ShippingAddress_ZipCode" <> 7728
""");
    }

    // This test fails because when OptionalCustomer is null, we get all-null results because of the LEFT JOIN, and we materialize this
    // as an empty ShippingAddress instead of null (see SQL). The proper solution here would be to project the Customer ID just for the
    // purpose of knowing that it's there.
    public override async Task Project_struct_complex_type_via_optional_navigation(bool async)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Project_struct_complex_type_via_optional_navigation(async));

        Assert.Equal(RelationalStrings.CannotProjectNullableComplexType("ValuedCustomer.ShippingAddress#AddressStruct"), exception.Message);
    }

    public override async Task Project_struct_complex_type_via_required_navigation(bool async)
    {
        await base.Project_struct_complex_type_via_required_navigation(async);

        AssertSql(
            """
SELECT "v0"."ShippingAddress_AddressLine1", "v0"."ShippingAddress_AddressLine2", "v0"."ShippingAddress_ZipCode", "v0"."ShippingAddress_Country_Code", "v0"."ShippingAddress_Country_FullName"
FROM "ValuedCustomerGroup" AS "v"
INNER JOIN "ValuedCustomer" AS "v0" ON "v"."RequiredCustomerId" = "v0"."Id"
""");
    }

    public override async Task Load_struct_complex_type_after_subquery_on_entity_type(bool async)
    {
        await base.Load_struct_complex_type_after_subquery_on_entity_type(async);

        AssertSql(
            """
@__p_0='1'

SELECT DISTINCT "t"."Id", "t"."Name", "t"."BillingAddress_AddressLine1", "t"."BillingAddress_AddressLine2", "t"."BillingAddress_ZipCode", "t"."BillingAddress_Country_Code", "t"."BillingAddress_Country_FullName", "t"."ShippingAddress_AddressLine1", "t"."ShippingAddress_AddressLine2", "t"."ShippingAddress_ZipCode", "t"."ShippingAddress_Country_Code", "t"."ShippingAddress_Country_FullName"
FROM (
    SELECT "v"."Id", "v"."Name", "v"."BillingAddress_AddressLine1", "v"."BillingAddress_AddressLine2", "v"."BillingAddress_ZipCode", "v"."BillingAddress_Country_Code", "v"."BillingAddress_Country_FullName", "v"."ShippingAddress_AddressLine1", "v"."ShippingAddress_AddressLine2", "v"."ShippingAddress_ZipCode", "v"."ShippingAddress_Country_Code", "v"."ShippingAddress_Country_FullName"
    FROM "ValuedCustomer" AS "v"
    ORDER BY "v"."Id"
    LIMIT -1 OFFSET @__p_0
) AS "t"
""");
    }

    public override async Task Select_struct_complex_type(bool async)
    {
        await base.Select_struct_complex_type(async);

        AssertSql(
            """
SELECT "v"."ShippingAddress_AddressLine1", "v"."ShippingAddress_AddressLine2", "v"."ShippingAddress_ZipCode", "v"."ShippingAddress_Country_Code", "v"."ShippingAddress_Country_FullName"
FROM "ValuedCustomer" AS "v"
""");
    }

    public override async Task Select_nested_struct_complex_type(bool async)
    {
        await base.Select_nested_struct_complex_type(async);

        AssertSql(
            """
SELECT "v"."ShippingAddress_Country_Code", "v"."ShippingAddress_Country_FullName"
FROM "ValuedCustomer" AS "v"
""");
    }

    public override async Task Select_single_property_on_nested_struct_complex_type(bool async)
    {
        await base.Select_single_property_on_nested_struct_complex_type(async);

        AssertSql(
            """
SELECT "v"."ShippingAddress_Country_FullName"
FROM "ValuedCustomer" AS "v"
""");
    }

    public override async Task Select_struct_complex_type_Where(bool async)
    {
        await base.Select_struct_complex_type_Where(async);

        AssertSql(
            """
SELECT "v"."ShippingAddress_AddressLine1", "v"."ShippingAddress_AddressLine2", "v"."ShippingAddress_ZipCode", "v"."ShippingAddress_Country_Code", "v"."ShippingAddress_Country_FullName"
FROM "ValuedCustomer" AS "v"
WHERE "v"."ShippingAddress_ZipCode" = 7728
""");
    }

    public override async Task Select_struct_complex_type_Distinct(bool async)
    {
        await base.Select_struct_complex_type_Distinct(async);

        AssertSql(
            """
SELECT DISTINCT "v"."ShippingAddress_AddressLine1", "v"."ShippingAddress_AddressLine2", "v"."ShippingAddress_ZipCode", "v"."ShippingAddress_Country_Code", "v"."ShippingAddress_Country_FullName"
FROM "ValuedCustomer" AS "v"
""");
    }

    public override async Task Struct_complex_type_equals_struct_complex_type(bool async)
    {
        await base.Struct_complex_type_equals_struct_complex_type(async);

        AssertSql(
            """
SELECT "v"."Id", "v"."Name", "v"."BillingAddress_AddressLine1", "v"."BillingAddress_AddressLine2", "v"."BillingAddress_ZipCode", "v"."BillingAddress_Country_Code", "v"."BillingAddress_Country_FullName", "v"."ShippingAddress_AddressLine1", "v"."ShippingAddress_AddressLine2", "v"."ShippingAddress_ZipCode", "v"."ShippingAddress_Country_Code", "v"."ShippingAddress_Country_FullName"
FROM "ValuedCustomer" AS "v"
WHERE "v"."ShippingAddress_AddressLine1" = "v"."BillingAddress_AddressLine1" AND ("v"."ShippingAddress_AddressLine2" = "v"."BillingAddress_AddressLine2" OR ("v"."ShippingAddress_AddressLine2" IS NULL AND "v"."BillingAddress_AddressLine2" IS NULL)) AND "v"."ShippingAddress_ZipCode" = "v"."BillingAddress_ZipCode"
""");
    }

    public override async Task Struct_complex_type_equals_constant(bool async)
    {
        await base.Struct_complex_type_equals_constant(async);

        AssertSql(
            """
SELECT "v"."Id", "v"."Name", "v"."BillingAddress_AddressLine1", "v"."BillingAddress_AddressLine2", "v"."BillingAddress_ZipCode", "v"."BillingAddress_Country_Code", "v"."BillingAddress_Country_FullName", "v"."ShippingAddress_AddressLine1", "v"."ShippingAddress_AddressLine2", "v"."ShippingAddress_ZipCode", "v"."ShippingAddress_Country_Code", "v"."ShippingAddress_Country_FullName"
FROM "ValuedCustomer" AS "v"
WHERE "v"."ShippingAddress_AddressLine1" = '804 S. Lakeshore Road' AND "v"."ShippingAddress_AddressLine2" IS NULL AND "v"."ShippingAddress_ZipCode" = 38654 AND "v"."ShippingAddress_Country_Code" = 'US' AND "v"."ShippingAddress_Country_FullName" = 'United States'
""");
    }

    public override async Task Struct_complex_type_equals_parameter(bool async)
    {
        await base.Struct_complex_type_equals_parameter(async);

        AssertSql(
            """
@__entity_equality_address_0_AddressLine1='804 S. Lakeshore Road' (Size = 21)
@__entity_equality_address_0_ZipCode='38654' (Nullable = true)
@__entity_equality_address_0_Country_Code='US' (Size = 2)
@__entity_equality_address_0_Country_FullName='United States' (Size = 13)

SELECT "v"."Id", "v"."Name", "v"."BillingAddress_AddressLine1", "v"."BillingAddress_AddressLine2", "v"."BillingAddress_ZipCode", "v"."BillingAddress_Country_Code", "v"."BillingAddress_Country_FullName", "v"."ShippingAddress_AddressLine1", "v"."ShippingAddress_AddressLine2", "v"."ShippingAddress_ZipCode", "v"."ShippingAddress_Country_Code", "v"."ShippingAddress_Country_FullName"
FROM "ValuedCustomer" AS "v"
WHERE "v"."ShippingAddress_AddressLine1" = @__entity_equality_address_0_AddressLine1 AND "v"."ShippingAddress_AddressLine2" IS NULL AND "v"."ShippingAddress_ZipCode" = @__entity_equality_address_0_ZipCode AND "v"."ShippingAddress_Country_Code" = @__entity_equality_address_0_Country_Code AND "v"."ShippingAddress_Country_FullName" = @__entity_equality_address_0_Country_FullName
""");
    }

    public override async Task Subquery_over_struct_complex_type(bool async)
    {
        await base.Subquery_over_struct_complex_type(async);

        AssertSql();
    }

    public override async Task Contains_over_struct_complex_type(bool async)
    {
        await base.Contains_over_struct_complex_type(async);

        AssertSql(
            """
@__entity_equality_address_0_AddressLine1='804 S. Lakeshore Road' (Size = 21)
@__entity_equality_address_0_ZipCode='38654' (Nullable = true)
@__entity_equality_address_0_Country_Code='US' (Size = 2)
@__entity_equality_address_0_Country_FullName='United States' (Size = 13)

SELECT "v"."Id", "v"."Name", "v"."BillingAddress_AddressLine1", "v"."BillingAddress_AddressLine2", "v"."BillingAddress_ZipCode", "v"."BillingAddress_Country_Code", "v"."BillingAddress_Country_FullName", "v"."ShippingAddress_AddressLine1", "v"."ShippingAddress_AddressLine2", "v"."ShippingAddress_ZipCode", "v"."ShippingAddress_Country_Code", "v"."ShippingAddress_Country_FullName"
FROM "ValuedCustomer" AS "v"
WHERE EXISTS (
    SELECT 1
    FROM "ValuedCustomer" AS "v0"
    WHERE "v0"."ShippingAddress_AddressLine1" = @__entity_equality_address_0_AddressLine1 AND "v0"."ShippingAddress_AddressLine2" IS NULL AND "v0"."ShippingAddress_ZipCode" = @__entity_equality_address_0_ZipCode AND "v0"."ShippingAddress_Country_Code" = @__entity_equality_address_0_Country_Code AND "v0"."ShippingAddress_Country_FullName" = @__entity_equality_address_0_Country_FullName)
""");
    }

    public override async Task Concat_struct_complex_type(bool async)
    {
        await base.Concat_struct_complex_type(async);

        AssertSql(
            """
SELECT "v"."ShippingAddress_AddressLine1", "v"."ShippingAddress_AddressLine2", "v"."ShippingAddress_ZipCode", "v"."ShippingAddress_Country_Code", "v"."ShippingAddress_Country_FullName"
FROM "ValuedCustomer" AS "v"
WHERE "v"."Id" = 1
UNION ALL
SELECT "v0"."ShippingAddress_AddressLine1", "v0"."ShippingAddress_AddressLine2", "v0"."ShippingAddress_ZipCode", "v0"."ShippingAddress_Country_Code", "v0"."ShippingAddress_Country_FullName"
FROM "ValuedCustomer" AS "v0"
WHERE "v0"."Id" = 2
""");
    }

    public override async Task Concat_entity_type_containing_struct_complex_property(bool async)
    {
        await base.Concat_entity_type_containing_struct_complex_property(async);

        AssertSql(
            """
SELECT "v"."Id", "v"."Name", "v"."BillingAddress_AddressLine1", "v"."BillingAddress_AddressLine2", "v"."BillingAddress_ZipCode", "v"."BillingAddress_Country_Code", "v"."BillingAddress_Country_FullName", "v"."ShippingAddress_AddressLine1", "v"."ShippingAddress_AddressLine2", "v"."ShippingAddress_ZipCode", "v"."ShippingAddress_Country_Code", "v"."ShippingAddress_Country_FullName"
FROM "ValuedCustomer" AS "v"
WHERE "v"."Id" = 1
UNION ALL
SELECT "v0"."Id", "v0"."Name", "v0"."BillingAddress_AddressLine1", "v0"."BillingAddress_AddressLine2", "v0"."BillingAddress_ZipCode", "v0"."BillingAddress_Country_Code", "v0"."BillingAddress_Country_FullName", "v0"."ShippingAddress_AddressLine1", "v0"."ShippingAddress_AddressLine2", "v0"."ShippingAddress_ZipCode", "v0"."ShippingAddress_Country_Code", "v0"."ShippingAddress_Country_FullName"
FROM "ValuedCustomer" AS "v0"
WHERE "v0"."Id" = 2
""");
    }

    public override async Task Union_entity_type_containing_struct_complex_property(bool async)
    {
        await base.Union_entity_type_containing_struct_complex_property(async);

        AssertSql(
            """
SELECT "v"."Id", "v"."Name", "v"."BillingAddress_AddressLine1", "v"."BillingAddress_AddressLine2", "v"."BillingAddress_ZipCode", "v"."BillingAddress_Country_Code", "v"."BillingAddress_Country_FullName", "v"."ShippingAddress_AddressLine1", "v"."ShippingAddress_AddressLine2", "v"."ShippingAddress_ZipCode", "v"."ShippingAddress_Country_Code", "v"."ShippingAddress_Country_FullName"
FROM "ValuedCustomer" AS "v"
WHERE "v"."Id" = 1
UNION
SELECT "v0"."Id", "v0"."Name", "v0"."BillingAddress_AddressLine1", "v0"."BillingAddress_AddressLine2", "v0"."BillingAddress_ZipCode", "v0"."BillingAddress_Country_Code", "v0"."BillingAddress_Country_FullName", "v0"."ShippingAddress_AddressLine1", "v0"."ShippingAddress_AddressLine2", "v0"."ShippingAddress_ZipCode", "v0"."ShippingAddress_Country_Code", "v0"."ShippingAddress_Country_FullName"
FROM "ValuedCustomer" AS "v0"
WHERE "v0"."Id" = 2
""");
    }

    public override async Task Union_struct_complex_type(bool async)
    {
        await base.Union_struct_complex_type(async);

        AssertSql(
            """
SELECT "v"."ShippingAddress_AddressLine1", "v"."ShippingAddress_AddressLine2", "v"."ShippingAddress_ZipCode", "v"."ShippingAddress_Country_Code", "v"."ShippingAddress_Country_FullName"
FROM "ValuedCustomer" AS "v"
WHERE "v"."Id" = 1
UNION
SELECT "v0"."ShippingAddress_AddressLine1", "v0"."ShippingAddress_AddressLine2", "v0"."ShippingAddress_ZipCode", "v0"."ShippingAddress_Country_Code", "v0"."ShippingAddress_Country_FullName"
FROM "ValuedCustomer" AS "v0"
WHERE "v0"."Id" = 2
""");
    }

    public override async Task Concat_property_in_struct_complex_type(bool async)
    {
        await base.Concat_property_in_struct_complex_type(async);

        AssertSql(
            """
SELECT "v"."ShippingAddress_AddressLine1"
FROM "ValuedCustomer" AS "v"
UNION ALL
SELECT "v0"."BillingAddress_AddressLine1" AS "ShippingAddress_AddressLine1"
FROM "ValuedCustomer" AS "v0"
""");
    }

    public override async Task Union_property_in_struct_complex_type(bool async)
    {
        await base.Union_property_in_struct_complex_type(async);

        AssertSql(
            """
SELECT "v"."ShippingAddress_AddressLine1"
FROM "ValuedCustomer" AS "v"
UNION
SELECT "v0"."BillingAddress_AddressLine1" AS "ShippingAddress_AddressLine1"
FROM "ValuedCustomer" AS "v0"
""");
    }

    public override async Task Concat_two_different_struct_complex_type(bool async)
    {
        await base.Concat_two_different_struct_complex_type(async);

        AssertSql();
    }

    public override async Task Union_two_different_struct_complex_type(bool async)
    {
        await base.Union_two_different_struct_complex_type(async);

        AssertSql();
    }

    public override async Task Project_same_entity_with_nested_complex_type_twice_with_pushdown(bool async)
    {
        await base.Project_same_entity_with_nested_complex_type_twice_with_pushdown(async);

        AssertSql(
"""
SELECT "t"."Id", "t"."Name", "t"."BillingAddress_AddressLine1", "t"."BillingAddress_AddressLine2", "t"."BillingAddress_ZipCode", "t"."BillingAddress_Country_Code", "t"."BillingAddress_Country_FullName", "t"."ShippingAddress_AddressLine1", "t"."ShippingAddress_AddressLine2", "t"."ShippingAddress_ZipCode", "t"."ShippingAddress_Country_Code", "t"."ShippingAddress_Country_FullName", "t"."Id0", "t"."Name0", "t"."BillingAddress_AddressLine10", "t"."BillingAddress_AddressLine20", "t"."BillingAddress_ZipCode0", "t"."BillingAddress_Country_Code0", "t"."BillingAddress_Country_FullName0", "t"."ShippingAddress_AddressLine10", "t"."ShippingAddress_AddressLine20", "t"."ShippingAddress_ZipCode0", "t"."ShippingAddress_Country_Code0", "t"."ShippingAddress_Country_FullName0"
FROM (
    SELECT DISTINCT "c"."Id", "c"."Name", "c"."BillingAddress_AddressLine1", "c"."BillingAddress_AddressLine2", "c"."BillingAddress_ZipCode", "c"."BillingAddress_Country_Code", "c"."BillingAddress_Country_FullName", "c"."ShippingAddress_AddressLine1", "c"."ShippingAddress_AddressLine2", "c"."ShippingAddress_ZipCode", "c"."ShippingAddress_Country_Code", "c"."ShippingAddress_Country_FullName", "c0"."Id" AS "Id0", "c0"."Name" AS "Name0", "c0"."BillingAddress_AddressLine1" AS "BillingAddress_AddressLine10", "c0"."BillingAddress_AddressLine2" AS "BillingAddress_AddressLine20", "c0"."BillingAddress_ZipCode" AS "BillingAddress_ZipCode0", "c0"."BillingAddress_Country_Code" AS "BillingAddress_Country_Code0", "c0"."BillingAddress_Country_FullName" AS "BillingAddress_Country_FullName0", "c0"."ShippingAddress_AddressLine1" AS "ShippingAddress_AddressLine10", "c0"."ShippingAddress_AddressLine2" AS "ShippingAddress_AddressLine20", "c0"."ShippingAddress_ZipCode" AS "ShippingAddress_ZipCode0", "c0"."ShippingAddress_Country_Code" AS "ShippingAddress_Country_Code0", "c0"."ShippingAddress_Country_FullName" AS "ShippingAddress_Country_FullName0"
    FROM "Customer" AS "c"
    CROSS JOIN "Customer" AS "c0"
) AS "t"
""");
    }

    public override async Task Project_same_nested_complex_type_twice_with_pushdown(bool async)
    {
        await base.Project_same_nested_complex_type_twice_with_pushdown(async);

        AssertSql(
"""
SELECT "t"."BillingAddress_AddressLine1", "t"."BillingAddress_AddressLine2", "t"."BillingAddress_ZipCode", "t"."BillingAddress_Country_Code", "t"."BillingAddress_Country_FullName", "t"."BillingAddress_AddressLine10", "t"."BillingAddress_AddressLine20", "t"."BillingAddress_ZipCode0", "t"."BillingAddress_Country_Code0", "t"."BillingAddress_Country_FullName0"
FROM (
    SELECT DISTINCT "c"."BillingAddress_AddressLine1", "c"."BillingAddress_AddressLine2", "c"."BillingAddress_ZipCode", "c"."BillingAddress_Country_Code", "c"."BillingAddress_Country_FullName", "c0"."BillingAddress_AddressLine1" AS "BillingAddress_AddressLine10", "c0"."BillingAddress_AddressLine2" AS "BillingAddress_AddressLine20", "c0"."BillingAddress_ZipCode" AS "BillingAddress_ZipCode0", "c0"."BillingAddress_Country_Code" AS "BillingAddress_Country_Code0", "c0"."BillingAddress_Country_FullName" AS "BillingAddress_Country_FullName0"
    FROM "Customer" AS "c"
    CROSS JOIN "Customer" AS "c0"
) AS "t"
""");
    }

    public override async Task Project_same_entity_with_nested_complex_type_twice_with_double_pushdown(bool async)
    {
        await base.Project_same_entity_with_nested_complex_type_twice_with_double_pushdown(async);

        AssertSql(
"""
@__p_0='50'

SELECT "t0"."Id", "t0"."Name", "t0"."BillingAddress_AddressLine1", "t0"."BillingAddress_AddressLine2", "t0"."BillingAddress_ZipCode", "t0"."BillingAddress_Country_Code", "t0"."BillingAddress_Country_FullName", "t0"."ShippingAddress_AddressLine1", "t0"."ShippingAddress_AddressLine2", "t0"."ShippingAddress_ZipCode", "t0"."ShippingAddress_Country_Code", "t0"."ShippingAddress_Country_FullName", "t0"."Id0", "t0"."Name0", "t0"."BillingAddress_AddressLine10", "t0"."BillingAddress_AddressLine20", "t0"."BillingAddress_ZipCode0", "t0"."BillingAddress_Country_Code0", "t0"."BillingAddress_Country_FullName0", "t0"."ShippingAddress_AddressLine10", "t0"."ShippingAddress_AddressLine20", "t0"."ShippingAddress_ZipCode0", "t0"."ShippingAddress_Country_Code0", "t0"."ShippingAddress_Country_FullName0"
FROM (
    SELECT DISTINCT "t"."Id", "t"."Name", "t"."BillingAddress_AddressLine1", "t"."BillingAddress_AddressLine2", "t"."BillingAddress_ZipCode", "t"."BillingAddress_Country_Code", "t"."BillingAddress_Country_FullName", "t"."ShippingAddress_AddressLine1", "t"."ShippingAddress_AddressLine2", "t"."ShippingAddress_ZipCode", "t"."ShippingAddress_Country_Code", "t"."ShippingAddress_Country_FullName", "t"."Id0", "t"."Name0", "t"."BillingAddress_AddressLine10", "t"."BillingAddress_AddressLine20", "t"."BillingAddress_ZipCode0", "t"."BillingAddress_Country_Code0", "t"."BillingAddress_Country_FullName0", "t"."ShippingAddress_AddressLine10", "t"."ShippingAddress_AddressLine20", "t"."ShippingAddress_ZipCode0", "t"."ShippingAddress_Country_Code0", "t"."ShippingAddress_Country_FullName0"
    FROM (
        SELECT "c"."Id", "c"."Name", "c"."BillingAddress_AddressLine1", "c"."BillingAddress_AddressLine2", "c"."BillingAddress_ZipCode", "c"."BillingAddress_Country_Code", "c"."BillingAddress_Country_FullName", "c"."ShippingAddress_AddressLine1", "c"."ShippingAddress_AddressLine2", "c"."ShippingAddress_ZipCode", "c"."ShippingAddress_Country_Code", "c"."ShippingAddress_Country_FullName", "c0"."Id" AS "Id0", "c0"."Name" AS "Name0", "c0"."BillingAddress_AddressLine1" AS "BillingAddress_AddressLine10", "c0"."BillingAddress_AddressLine2" AS "BillingAddress_AddressLine20", "c0"."BillingAddress_ZipCode" AS "BillingAddress_ZipCode0", "c0"."BillingAddress_Country_Code" AS "BillingAddress_Country_Code0", "c0"."BillingAddress_Country_FullName" AS "BillingAddress_Country_FullName0", "c0"."ShippingAddress_AddressLine1" AS "ShippingAddress_AddressLine10", "c0"."ShippingAddress_AddressLine2" AS "ShippingAddress_AddressLine20", "c0"."ShippingAddress_ZipCode" AS "ShippingAddress_ZipCode0", "c0"."ShippingAddress_Country_Code" AS "ShippingAddress_Country_Code0", "c0"."ShippingAddress_Country_FullName" AS "ShippingAddress_Country_FullName0"
        FROM "Customer" AS "c"
        CROSS JOIN "Customer" AS "c0"
        ORDER BY "c"."Id", "c0"."Id"
        LIMIT @__p_0
    ) AS "t"
) AS "t0"
""");
    }

    public override async Task Project_same_nested_complex_type_twice_with_double_pushdown(bool async)
    {
        await base.Project_same_nested_complex_type_twice_with_double_pushdown(async);

        AssertSql(
"""
@__p_0='50'

SELECT "t0"."BillingAddress_AddressLine1", "t0"."BillingAddress_AddressLine2", "t0"."BillingAddress_ZipCode", "t0"."BillingAddress_Country_Code", "t0"."BillingAddress_Country_FullName", "t0"."BillingAddress_AddressLine10", "t0"."BillingAddress_AddressLine20", "t0"."BillingAddress_ZipCode0", "t0"."BillingAddress_Country_Code0", "t0"."BillingAddress_Country_FullName0"
FROM (
    SELECT DISTINCT "t"."BillingAddress_AddressLine1", "t"."BillingAddress_AddressLine2", "t"."BillingAddress_ZipCode", "t"."BillingAddress_Country_Code", "t"."BillingAddress_Country_FullName", "t"."BillingAddress_AddressLine10", "t"."BillingAddress_AddressLine20", "t"."BillingAddress_ZipCode0", "t"."BillingAddress_Country_Code0", "t"."BillingAddress_Country_FullName0"
    FROM (
        SELECT "c"."BillingAddress_AddressLine1", "c"."BillingAddress_AddressLine2", "c"."BillingAddress_ZipCode", "c"."BillingAddress_Country_Code", "c"."BillingAddress_Country_FullName", "c0"."BillingAddress_AddressLine1" AS "BillingAddress_AddressLine10", "c0"."BillingAddress_AddressLine2" AS "BillingAddress_AddressLine20", "c0"."BillingAddress_ZipCode" AS "BillingAddress_ZipCode0", "c0"."BillingAddress_Country_Code" AS "BillingAddress_Country_Code0", "c0"."BillingAddress_Country_FullName" AS "BillingAddress_Country_FullName0"
        FROM "Customer" AS "c"
        CROSS JOIN "Customer" AS "c0"
        ORDER BY "c"."Id", "c0"."Id"
        LIMIT @__p_0
    ) AS "t"
) AS "t0"
""");
    }

    public override async Task Project_same_entity_with_struct_nested_complex_type_twice_with_pushdown(bool async)
    {
        await base.Project_same_entity_with_struct_nested_complex_type_twice_with_pushdown(async);

        AssertSql(
"""
SELECT "t"."Id", "t"."Name", "t"."BillingAddress_AddressLine1", "t"."BillingAddress_AddressLine2", "t"."BillingAddress_ZipCode", "t"."BillingAddress_Country_Code", "t"."BillingAddress_Country_FullName", "t"."ShippingAddress_AddressLine1", "t"."ShippingAddress_AddressLine2", "t"."ShippingAddress_ZipCode", "t"."ShippingAddress_Country_Code", "t"."ShippingAddress_Country_FullName", "t"."Id0", "t"."Name0", "t"."BillingAddress_AddressLine10", "t"."BillingAddress_AddressLine20", "t"."BillingAddress_ZipCode0", "t"."BillingAddress_Country_Code0", "t"."BillingAddress_Country_FullName0", "t"."ShippingAddress_AddressLine10", "t"."ShippingAddress_AddressLine20", "t"."ShippingAddress_ZipCode0", "t"."ShippingAddress_Country_Code0", "t"."ShippingAddress_Country_FullName0"
FROM (
    SELECT DISTINCT "v"."Id", "v"."Name", "v"."BillingAddress_AddressLine1", "v"."BillingAddress_AddressLine2", "v"."BillingAddress_ZipCode", "v"."BillingAddress_Country_Code", "v"."BillingAddress_Country_FullName", "v"."ShippingAddress_AddressLine1", "v"."ShippingAddress_AddressLine2", "v"."ShippingAddress_ZipCode", "v"."ShippingAddress_Country_Code", "v"."ShippingAddress_Country_FullName", "v0"."Id" AS "Id0", "v0"."Name" AS "Name0", "v0"."BillingAddress_AddressLine1" AS "BillingAddress_AddressLine10", "v0"."BillingAddress_AddressLine2" AS "BillingAddress_AddressLine20", "v0"."BillingAddress_ZipCode" AS "BillingAddress_ZipCode0", "v0"."BillingAddress_Country_Code" AS "BillingAddress_Country_Code0", "v0"."BillingAddress_Country_FullName" AS "BillingAddress_Country_FullName0", "v0"."ShippingAddress_AddressLine1" AS "ShippingAddress_AddressLine10", "v0"."ShippingAddress_AddressLine2" AS "ShippingAddress_AddressLine20", "v0"."ShippingAddress_ZipCode" AS "ShippingAddress_ZipCode0", "v0"."ShippingAddress_Country_Code" AS "ShippingAddress_Country_Code0", "v0"."ShippingAddress_Country_FullName" AS "ShippingAddress_Country_FullName0"
    FROM "ValuedCustomer" AS "v"
    CROSS JOIN "ValuedCustomer" AS "v0"
) AS "t"
""");
    }

    public override async Task Project_same_struct_nested_complex_type_twice_with_pushdown(bool async)
    {
        await base.Project_same_struct_nested_complex_type_twice_with_pushdown(async);

        AssertSql(
"""
SELECT "t"."BillingAddress_AddressLine1", "t"."BillingAddress_AddressLine2", "t"."BillingAddress_ZipCode", "t"."BillingAddress_Country_Code", "t"."BillingAddress_Country_FullName", "t"."BillingAddress_AddressLine10", "t"."BillingAddress_AddressLine20", "t"."BillingAddress_ZipCode0", "t"."BillingAddress_Country_Code0", "t"."BillingAddress_Country_FullName0"
FROM (
    SELECT DISTINCT "v"."BillingAddress_AddressLine1", "v"."BillingAddress_AddressLine2", "v"."BillingAddress_ZipCode", "v"."BillingAddress_Country_Code", "v"."BillingAddress_Country_FullName", "v0"."BillingAddress_AddressLine1" AS "BillingAddress_AddressLine10", "v0"."BillingAddress_AddressLine2" AS "BillingAddress_AddressLine20", "v0"."BillingAddress_ZipCode" AS "BillingAddress_ZipCode0", "v0"."BillingAddress_Country_Code" AS "BillingAddress_Country_Code0", "v0"."BillingAddress_Country_FullName" AS "BillingAddress_Country_FullName0"
    FROM "ValuedCustomer" AS "v"
    CROSS JOIN "ValuedCustomer" AS "v0"
) AS "t"
""");
    }

    public override async Task Project_same_entity_with_struct_nested_complex_type_twice_with_double_pushdown(bool async)
    {
        await base.Project_same_entity_with_struct_nested_complex_type_twice_with_double_pushdown(async);

        AssertSql(
"""
@__p_0='50'

SELECT "t0"."Id", "t0"."Name", "t0"."BillingAddress_AddressLine1", "t0"."BillingAddress_AddressLine2", "t0"."BillingAddress_ZipCode", "t0"."BillingAddress_Country_Code", "t0"."BillingAddress_Country_FullName", "t0"."ShippingAddress_AddressLine1", "t0"."ShippingAddress_AddressLine2", "t0"."ShippingAddress_ZipCode", "t0"."ShippingAddress_Country_Code", "t0"."ShippingAddress_Country_FullName", "t0"."Id0", "t0"."Name0", "t0"."BillingAddress_AddressLine10", "t0"."BillingAddress_AddressLine20", "t0"."BillingAddress_ZipCode0", "t0"."BillingAddress_Country_Code0", "t0"."BillingAddress_Country_FullName0", "t0"."ShippingAddress_AddressLine10", "t0"."ShippingAddress_AddressLine20", "t0"."ShippingAddress_ZipCode0", "t0"."ShippingAddress_Country_Code0", "t0"."ShippingAddress_Country_FullName0"
FROM (
    SELECT DISTINCT "t"."Id", "t"."Name", "t"."BillingAddress_AddressLine1", "t"."BillingAddress_AddressLine2", "t"."BillingAddress_ZipCode", "t"."BillingAddress_Country_Code", "t"."BillingAddress_Country_FullName", "t"."ShippingAddress_AddressLine1", "t"."ShippingAddress_AddressLine2", "t"."ShippingAddress_ZipCode", "t"."ShippingAddress_Country_Code", "t"."ShippingAddress_Country_FullName", "t"."Id0", "t"."Name0", "t"."BillingAddress_AddressLine10", "t"."BillingAddress_AddressLine20", "t"."BillingAddress_ZipCode0", "t"."BillingAddress_Country_Code0", "t"."BillingAddress_Country_FullName0", "t"."ShippingAddress_AddressLine10", "t"."ShippingAddress_AddressLine20", "t"."ShippingAddress_ZipCode0", "t"."ShippingAddress_Country_Code0", "t"."ShippingAddress_Country_FullName0"
    FROM (
        SELECT "v"."Id", "v"."Name", "v"."BillingAddress_AddressLine1", "v"."BillingAddress_AddressLine2", "v"."BillingAddress_ZipCode", "v"."BillingAddress_Country_Code", "v"."BillingAddress_Country_FullName", "v"."ShippingAddress_AddressLine1", "v"."ShippingAddress_AddressLine2", "v"."ShippingAddress_ZipCode", "v"."ShippingAddress_Country_Code", "v"."ShippingAddress_Country_FullName", "v0"."Id" AS "Id0", "v0"."Name" AS "Name0", "v0"."BillingAddress_AddressLine1" AS "BillingAddress_AddressLine10", "v0"."BillingAddress_AddressLine2" AS "BillingAddress_AddressLine20", "v0"."BillingAddress_ZipCode" AS "BillingAddress_ZipCode0", "v0"."BillingAddress_Country_Code" AS "BillingAddress_Country_Code0", "v0"."BillingAddress_Country_FullName" AS "BillingAddress_Country_FullName0", "v0"."ShippingAddress_AddressLine1" AS "ShippingAddress_AddressLine10", "v0"."ShippingAddress_AddressLine2" AS "ShippingAddress_AddressLine20", "v0"."ShippingAddress_ZipCode" AS "ShippingAddress_ZipCode0", "v0"."ShippingAddress_Country_Code" AS "ShippingAddress_Country_Code0", "v0"."ShippingAddress_Country_FullName" AS "ShippingAddress_Country_FullName0"
        FROM "ValuedCustomer" AS "v"
        CROSS JOIN "ValuedCustomer" AS "v0"
        ORDER BY "v"."Id", "v0"."Id"
        LIMIT @__p_0
    ) AS "t"
) AS "t0"
""");
    }

    public override async Task Project_same_struct_nested_complex_type_twice_with_double_pushdown(bool async)
    {
        await base.Project_same_struct_nested_complex_type_twice_with_double_pushdown(async);

        AssertSql(
"""
@__p_0='50'

SELECT "t0"."BillingAddress_AddressLine1", "t0"."BillingAddress_AddressLine2", "t0"."BillingAddress_ZipCode", "t0"."BillingAddress_Country_Code", "t0"."BillingAddress_Country_FullName", "t0"."BillingAddress_AddressLine10", "t0"."BillingAddress_AddressLine20", "t0"."BillingAddress_ZipCode0", "t0"."BillingAddress_Country_Code0", "t0"."BillingAddress_Country_FullName0"
FROM (
    SELECT DISTINCT "t"."BillingAddress_AddressLine1", "t"."BillingAddress_AddressLine2", "t"."BillingAddress_ZipCode", "t"."BillingAddress_Country_Code", "t"."BillingAddress_Country_FullName", "t"."BillingAddress_AddressLine10", "t"."BillingAddress_AddressLine20", "t"."BillingAddress_ZipCode0", "t"."BillingAddress_Country_Code0", "t"."BillingAddress_Country_FullName0"
    FROM (
        SELECT "v"."BillingAddress_AddressLine1", "v"."BillingAddress_AddressLine2", "v"."BillingAddress_ZipCode", "v"."BillingAddress_Country_Code", "v"."BillingAddress_Country_FullName", "v0"."BillingAddress_AddressLine1" AS "BillingAddress_AddressLine10", "v0"."BillingAddress_AddressLine2" AS "BillingAddress_AddressLine20", "v0"."BillingAddress_ZipCode" AS "BillingAddress_ZipCode0", "v0"."BillingAddress_Country_Code" AS "BillingAddress_Country_Code0", "v0"."BillingAddress_Country_FullName" AS "BillingAddress_Country_FullName0"
        FROM "ValuedCustomer" AS "v"
        CROSS JOIN "ValuedCustomer" AS "v0"
        ORDER BY "v"."Id", "v0"."Id"
        LIMIT @__p_0
    ) AS "t"
) AS "t0"
""");
    }

    public override async Task Union_of_same_entity_with_nested_complex_type_projected_twice_with_pushdown(bool async)
    {
        await base.Union_of_same_entity_with_nested_complex_type_projected_twice_with_pushdown(async);

        AssertSql(
"""
@__p_0='50'

SELECT "t"."Id", "t"."Name", "t"."BillingAddress_AddressLine1", "t"."BillingAddress_AddressLine2", "t"."BillingAddress_ZipCode", "t"."BillingAddress_Country_Code", "t"."BillingAddress_Country_FullName", "t"."ShippingAddress_AddressLine1", "t"."ShippingAddress_AddressLine2", "t"."ShippingAddress_ZipCode", "t"."ShippingAddress_Country_Code", "t"."ShippingAddress_Country_FullName", "t"."Id0", "t"."Name0", "t"."BillingAddress_AddressLine10", "t"."BillingAddress_AddressLine20", "t"."BillingAddress_ZipCode0", "t"."BillingAddress_Country_Code0", "t"."BillingAddress_Country_FullName0", "t"."ShippingAddress_AddressLine10", "t"."ShippingAddress_AddressLine20", "t"."ShippingAddress_ZipCode0", "t"."ShippingAddress_Country_Code0", "t"."ShippingAddress_Country_FullName0"
FROM (
    SELECT "c"."Id", "c"."Name", "c"."BillingAddress_AddressLine1", "c"."BillingAddress_AddressLine2", "c"."BillingAddress_ZipCode", "c"."BillingAddress_Country_Code", "c"."BillingAddress_Country_FullName", "c"."ShippingAddress_AddressLine1", "c"."ShippingAddress_AddressLine2", "c"."ShippingAddress_ZipCode", "c"."ShippingAddress_Country_Code", "c"."ShippingAddress_Country_FullName", "c0"."Id" AS "Id0", "c0"."Name" AS "Name0", "c0"."BillingAddress_AddressLine1" AS "BillingAddress_AddressLine10", "c0"."BillingAddress_AddressLine2" AS "BillingAddress_AddressLine20", "c0"."BillingAddress_ZipCode" AS "BillingAddress_ZipCode0", "c0"."BillingAddress_Country_Code" AS "BillingAddress_Country_Code0", "c0"."BillingAddress_Country_FullName" AS "BillingAddress_Country_FullName0", "c0"."ShippingAddress_AddressLine1" AS "ShippingAddress_AddressLine10", "c0"."ShippingAddress_AddressLine2" AS "ShippingAddress_AddressLine20", "c0"."ShippingAddress_ZipCode" AS "ShippingAddress_ZipCode0", "c0"."ShippingAddress_Country_Code" AS "ShippingAddress_Country_Code0", "c0"."ShippingAddress_Country_FullName" AS "ShippingAddress_Country_FullName0"
    FROM "Customer" AS "c"
    CROSS JOIN "Customer" AS "c0"
    UNION
    SELECT "c1"."Id", "c1"."Name", "c1"."BillingAddress_AddressLine1", "c1"."BillingAddress_AddressLine2", "c1"."BillingAddress_ZipCode", "c1"."BillingAddress_Country_Code", "c1"."BillingAddress_Country_FullName", "c1"."ShippingAddress_AddressLine1", "c1"."ShippingAddress_AddressLine2", "c1"."ShippingAddress_ZipCode", "c1"."ShippingAddress_Country_Code", "c1"."ShippingAddress_Country_FullName", "c2"."Id" AS "Id0", "c2"."Name" AS "Name0", "c2"."BillingAddress_AddressLine1" AS "BillingAddress_AddressLine10", "c2"."BillingAddress_AddressLine2" AS "BillingAddress_AddressLine20", "c2"."BillingAddress_ZipCode" AS "BillingAddress_ZipCode0", "c2"."BillingAddress_Country_Code" AS "BillingAddress_Country_Code0", "c2"."BillingAddress_Country_FullName" AS "BillingAddress_Country_FullName0", "c2"."ShippingAddress_AddressLine1" AS "ShippingAddress_AddressLine10", "c2"."ShippingAddress_AddressLine2" AS "ShippingAddress_AddressLine20", "c2"."ShippingAddress_ZipCode" AS "ShippingAddress_ZipCode0", "c2"."ShippingAddress_Country_Code" AS "ShippingAddress_Country_Code0", "c2"."ShippingAddress_Country_FullName" AS "ShippingAddress_Country_FullName0"
    FROM "Customer" AS "c1"
    CROSS JOIN "Customer" AS "c2"
) AS "t"
ORDER BY "t"."Id", "t"."Id0"
LIMIT @__p_0
""");
    }

    public override async Task Union_of_same_entity_with_nested_complex_type_projected_twice_with_double_pushdown(bool async)
    {
        await base.Union_of_same_entity_with_nested_complex_type_projected_twice_with_double_pushdown(async);

        AssertSql(
"""
@__p_0='50'

SELECT "t1"."Id", "t1"."Name", "t1"."BillingAddress_AddressLine1", "t1"."BillingAddress_AddressLine2", "t1"."BillingAddress_ZipCode", "t1"."BillingAddress_Country_Code", "t1"."BillingAddress_Country_FullName", "t1"."ShippingAddress_AddressLine1", "t1"."ShippingAddress_AddressLine2", "t1"."ShippingAddress_ZipCode", "t1"."ShippingAddress_Country_Code", "t1"."ShippingAddress_Country_FullName", "t1"."Id0", "t1"."Name0", "t1"."BillingAddress_AddressLine10", "t1"."BillingAddress_AddressLine20", "t1"."BillingAddress_ZipCode0", "t1"."BillingAddress_Country_Code0", "t1"."BillingAddress_Country_FullName0", "t1"."ShippingAddress_AddressLine10", "t1"."ShippingAddress_AddressLine20", "t1"."ShippingAddress_ZipCode0", "t1"."ShippingAddress_Country_Code0", "t1"."ShippingAddress_Country_FullName0"
FROM (
    SELECT DISTINCT "t0"."Id", "t0"."Name", "t0"."BillingAddress_AddressLine1", "t0"."BillingAddress_AddressLine2", "t0"."BillingAddress_ZipCode", "t0"."BillingAddress_Country_Code", "t0"."BillingAddress_Country_FullName", "t0"."ShippingAddress_AddressLine1", "t0"."ShippingAddress_AddressLine2", "t0"."ShippingAddress_ZipCode", "t0"."ShippingAddress_Country_Code", "t0"."ShippingAddress_Country_FullName", "t0"."Id0", "t0"."Name0", "t0"."BillingAddress_AddressLine10", "t0"."BillingAddress_AddressLine20", "t0"."BillingAddress_ZipCode0", "t0"."BillingAddress_Country_Code0", "t0"."BillingAddress_Country_FullName0", "t0"."ShippingAddress_AddressLine10", "t0"."ShippingAddress_AddressLine20", "t0"."ShippingAddress_ZipCode0", "t0"."ShippingAddress_Country_Code0", "t0"."ShippingAddress_Country_FullName0"
    FROM (
        SELECT "t"."Id", "t"."Name", "t"."BillingAddress_AddressLine1", "t"."BillingAddress_AddressLine2", "t"."BillingAddress_ZipCode", "t"."BillingAddress_Country_Code", "t"."BillingAddress_Country_FullName", "t"."ShippingAddress_AddressLine1", "t"."ShippingAddress_AddressLine2", "t"."ShippingAddress_ZipCode", "t"."ShippingAddress_Country_Code", "t"."ShippingAddress_Country_FullName", "t"."Id0", "t"."Name0", "t"."BillingAddress_AddressLine10", "t"."BillingAddress_AddressLine20", "t"."BillingAddress_ZipCode0", "t"."BillingAddress_Country_Code0", "t"."BillingAddress_Country_FullName0", "t"."ShippingAddress_AddressLine10", "t"."ShippingAddress_AddressLine20", "t"."ShippingAddress_ZipCode0", "t"."ShippingAddress_Country_Code0", "t"."ShippingAddress_Country_FullName0"
        FROM (
            SELECT "c"."Id", "c"."Name", "c"."BillingAddress_AddressLine1", "c"."BillingAddress_AddressLine2", "c"."BillingAddress_ZipCode", "c"."BillingAddress_Country_Code", "c"."BillingAddress_Country_FullName", "c"."ShippingAddress_AddressLine1", "c"."ShippingAddress_AddressLine2", "c"."ShippingAddress_ZipCode", "c"."ShippingAddress_Country_Code", "c"."ShippingAddress_Country_FullName", "c0"."Id" AS "Id0", "c0"."Name" AS "Name0", "c0"."BillingAddress_AddressLine1" AS "BillingAddress_AddressLine10", "c0"."BillingAddress_AddressLine2" AS "BillingAddress_AddressLine20", "c0"."BillingAddress_ZipCode" AS "BillingAddress_ZipCode0", "c0"."BillingAddress_Country_Code" AS "BillingAddress_Country_Code0", "c0"."BillingAddress_Country_FullName" AS "BillingAddress_Country_FullName0", "c0"."ShippingAddress_AddressLine1" AS "ShippingAddress_AddressLine10", "c0"."ShippingAddress_AddressLine2" AS "ShippingAddress_AddressLine20", "c0"."ShippingAddress_ZipCode" AS "ShippingAddress_ZipCode0", "c0"."ShippingAddress_Country_Code" AS "ShippingAddress_Country_Code0", "c0"."ShippingAddress_Country_FullName" AS "ShippingAddress_Country_FullName0"
            FROM "Customer" AS "c"
            CROSS JOIN "Customer" AS "c0"
            UNION
            SELECT "c1"."Id", "c1"."Name", "c1"."BillingAddress_AddressLine1", "c1"."BillingAddress_AddressLine2", "c1"."BillingAddress_ZipCode", "c1"."BillingAddress_Country_Code", "c1"."BillingAddress_Country_FullName", "c1"."ShippingAddress_AddressLine1", "c1"."ShippingAddress_AddressLine2", "c1"."ShippingAddress_ZipCode", "c1"."ShippingAddress_Country_Code", "c1"."ShippingAddress_Country_FullName", "c2"."Id" AS "Id0", "c2"."Name" AS "Name0", "c2"."BillingAddress_AddressLine1" AS "BillingAddress_AddressLine10", "c2"."BillingAddress_AddressLine2" AS "BillingAddress_AddressLine20", "c2"."BillingAddress_ZipCode" AS "BillingAddress_ZipCode0", "c2"."BillingAddress_Country_Code" AS "BillingAddress_Country_Code0", "c2"."BillingAddress_Country_FullName" AS "BillingAddress_Country_FullName0", "c2"."ShippingAddress_AddressLine1" AS "ShippingAddress_AddressLine10", "c2"."ShippingAddress_AddressLine2" AS "ShippingAddress_AddressLine20", "c2"."ShippingAddress_ZipCode" AS "ShippingAddress_ZipCode0", "c2"."ShippingAddress_Country_Code" AS "ShippingAddress_Country_Code0", "c2"."ShippingAddress_Country_FullName" AS "ShippingAddress_Country_FullName0"
            FROM "Customer" AS "c1"
            CROSS JOIN "Customer" AS "c2"
        ) AS "t"
        ORDER BY "t"."Id", "t"."Id0"
        LIMIT @__p_0
    ) AS "t0"
) AS "t1"
ORDER BY "t1"."Id", "t1"."Id0"
LIMIT @__p_0
""");
    }

    public override async Task Union_of_same_nested_complex_type_projected_twice_with_pushdown(bool async)
    {
        await base.Union_of_same_nested_complex_type_projected_twice_with_pushdown(async);

        AssertSql(
"""
@__p_0='50'

SELECT "t"."BillingAddress_AddressLine1", "t"."BillingAddress_AddressLine2", "t"."BillingAddress_ZipCode", "t"."BillingAddress_Country_Code", "t"."BillingAddress_Country_FullName", "t"."BillingAddress_AddressLine10", "t"."BillingAddress_AddressLine20", "t"."BillingAddress_ZipCode0", "t"."BillingAddress_Country_Code0", "t"."BillingAddress_Country_FullName0"
FROM (
    SELECT "c"."BillingAddress_AddressLine1", "c"."BillingAddress_AddressLine2", "c"."BillingAddress_ZipCode", "c"."BillingAddress_Country_Code", "c"."BillingAddress_Country_FullName", "c0"."BillingAddress_AddressLine1" AS "BillingAddress_AddressLine10", "c0"."BillingAddress_AddressLine2" AS "BillingAddress_AddressLine20", "c0"."BillingAddress_ZipCode" AS "BillingAddress_ZipCode0", "c0"."BillingAddress_Country_Code" AS "BillingAddress_Country_Code0", "c0"."BillingAddress_Country_FullName" AS "BillingAddress_Country_FullName0"
    FROM "Customer" AS "c"
    CROSS JOIN "Customer" AS "c0"
    UNION
    SELECT "c1"."BillingAddress_AddressLine1", "c1"."BillingAddress_AddressLine2", "c1"."BillingAddress_ZipCode", "c1"."BillingAddress_Country_Code", "c1"."BillingAddress_Country_FullName", "c2"."BillingAddress_AddressLine1" AS "BillingAddress_AddressLine10", "c2"."BillingAddress_AddressLine2" AS "BillingAddress_AddressLine20", "c2"."BillingAddress_ZipCode" AS "BillingAddress_ZipCode0", "c2"."BillingAddress_Country_Code" AS "BillingAddress_Country_Code0", "c2"."BillingAddress_Country_FullName" AS "BillingAddress_Country_FullName0"
    FROM "Customer" AS "c1"
    CROSS JOIN "Customer" AS "c2"
) AS "t"
ORDER BY "t"."BillingAddress_ZipCode", "t"."BillingAddress_ZipCode0"
LIMIT @__p_0
""");
    }

    public override async Task Union_of_same_nested_complex_type_projected_twice_with_double_pushdown(bool async)
    {
        await base.Union_of_same_nested_complex_type_projected_twice_with_double_pushdown(async);

        AssertSql(
"""
@__p_0='50'

SELECT "t1"."BillingAddress_AddressLine1", "t1"."BillingAddress_AddressLine2", "t1"."BillingAddress_ZipCode", "t1"."BillingAddress_Country_Code", "t1"."BillingAddress_Country_FullName", "t1"."BillingAddress_AddressLine10", "t1"."BillingAddress_AddressLine20", "t1"."BillingAddress_ZipCode0", "t1"."BillingAddress_Country_Code0", "t1"."BillingAddress_Country_FullName0"
FROM (
    SELECT DISTINCT "t0"."BillingAddress_AddressLine1", "t0"."BillingAddress_AddressLine2", "t0"."BillingAddress_ZipCode", "t0"."BillingAddress_Country_Code", "t0"."BillingAddress_Country_FullName", "t0"."BillingAddress_AddressLine10", "t0"."BillingAddress_AddressLine20", "t0"."BillingAddress_ZipCode0", "t0"."BillingAddress_Country_Code0", "t0"."BillingAddress_Country_FullName0"
    FROM (
        SELECT "t"."BillingAddress_AddressLine1", "t"."BillingAddress_AddressLine2", "t"."BillingAddress_ZipCode", "t"."BillingAddress_Country_Code", "t"."BillingAddress_Country_FullName", "t"."BillingAddress_AddressLine10", "t"."BillingAddress_AddressLine20", "t"."BillingAddress_ZipCode0", "t"."BillingAddress_Country_Code0", "t"."BillingAddress_Country_FullName0"
        FROM (
            SELECT "c"."BillingAddress_AddressLine1", "c"."BillingAddress_AddressLine2", "c"."BillingAddress_ZipCode", "c"."BillingAddress_Country_Code", "c"."BillingAddress_Country_FullName", "c0"."BillingAddress_AddressLine1" AS "BillingAddress_AddressLine10", "c0"."BillingAddress_AddressLine2" AS "BillingAddress_AddressLine20", "c0"."BillingAddress_ZipCode" AS "BillingAddress_ZipCode0", "c0"."BillingAddress_Country_Code" AS "BillingAddress_Country_Code0", "c0"."BillingAddress_Country_FullName" AS "BillingAddress_Country_FullName0"
            FROM "Customer" AS "c"
            CROSS JOIN "Customer" AS "c0"
            UNION
            SELECT "c1"."BillingAddress_AddressLine1", "c1"."BillingAddress_AddressLine2", "c1"."BillingAddress_ZipCode", "c1"."BillingAddress_Country_Code", "c1"."BillingAddress_Country_FullName", "c2"."BillingAddress_AddressLine1" AS "BillingAddress_AddressLine10", "c2"."BillingAddress_AddressLine2" AS "BillingAddress_AddressLine20", "c2"."BillingAddress_ZipCode" AS "BillingAddress_ZipCode0", "c2"."BillingAddress_Country_Code" AS "BillingAddress_Country_Code0", "c2"."BillingAddress_Country_FullName" AS "BillingAddress_Country_FullName0"
            FROM "Customer" AS "c1"
            CROSS JOIN "Customer" AS "c2"
        ) AS "t"
        ORDER BY "t"."BillingAddress_ZipCode", "t"."BillingAddress_ZipCode0"
        LIMIT @__p_0
    ) AS "t0"
) AS "t1"
ORDER BY "t1"."BillingAddress_ZipCode", "t1"."BillingAddress_ZipCode0"
LIMIT @__p_0
""");
    }

    public override async Task Same_entity_with_complex_type_projected_twice_with_pushdown_as_part_of_another_projection(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Same_entity_with_complex_type_projected_twice_with_pushdown_as_part_of_another_projection(async))).Message);

    public override async Task Same_complex_type_projected_twice_with_pushdown_as_part_of_another_projection(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Same_complex_type_projected_twice_with_pushdown_as_part_of_another_projection(async))).Message);

    public override async Task Projecting_property_of_complex_type_using_left_join_with_pushdown(bool async)
    {
        await base.Projecting_property_of_complex_type_using_left_join_with_pushdown(async);

        AssertSql(
"""
SELECT "t"."BillingAddress_ZipCode"
FROM "CustomerGroup" AS "c"
LEFT JOIN (
    SELECT "c0"."Id", "c0"."BillingAddress_ZipCode"
    FROM "Customer" AS "c0"
    WHERE "c0"."Id" > 5
) AS "t" ON "c"."Id" = "t"."Id"
""");
    }

    public override async Task Projecting_complex_from_optional_navigation_using_conditional(bool async)
    {
        await base.Projecting_complex_from_optional_navigation_using_conditional(async);

        AssertSql(
"""
@__p_0='20'

SELECT "t0"."ShippingAddress_ZipCode"
FROM (
    SELECT DISTINCT "t"."ShippingAddress_AddressLine1", "t"."ShippingAddress_AddressLine2", "t"."ShippingAddress_ZipCode", "t"."ShippingAddress_Country_Code", "t"."ShippingAddress_Country_FullName"
    FROM (
        SELECT "c0"."ShippingAddress_AddressLine1", "c0"."ShippingAddress_AddressLine2", "c0"."ShippingAddress_ZipCode", "c0"."ShippingAddress_Country_Code", "c0"."ShippingAddress_Country_FullName"
        FROM "CustomerGroup" AS "c"
        LEFT JOIN "Customer" AS "c0" ON "c"."OptionalCustomerId" = "c0"."Id"
        ORDER BY "c0"."ShippingAddress_ZipCode"
        LIMIT @__p_0
    ) AS "t"
) AS "t0"
""");
    }

    public override async Task Project_entity_with_complex_type_pushdown_and_then_left_join(bool async)
    {
        await base.Project_entity_with_complex_type_pushdown_and_then_left_join(async);

        AssertSql(
"""
@__p_0='20'
@__p_1='30'

SELECT "t0"."BillingAddress_ZipCode" AS "Zip1", "t1"."ShippingAddress_ZipCode" AS "Zip2"
FROM (
    SELECT DISTINCT "t"."Id", "t"."Name", "t"."BillingAddress_AddressLine1", "t"."BillingAddress_AddressLine2", "t"."BillingAddress_ZipCode", "t"."BillingAddress_Country_Code", "t"."BillingAddress_Country_FullName", "t"."ShippingAddress_AddressLine1", "t"."ShippingAddress_AddressLine2", "t"."ShippingAddress_ZipCode", "t"."ShippingAddress_Country_Code", "t"."ShippingAddress_Country_FullName"
    FROM (
        SELECT "c"."Id", "c"."Name", "c"."BillingAddress_AddressLine1", "c"."BillingAddress_AddressLine2", "c"."BillingAddress_ZipCode", "c"."BillingAddress_Country_Code", "c"."BillingAddress_Country_FullName", "c"."ShippingAddress_AddressLine1", "c"."ShippingAddress_AddressLine2", "c"."ShippingAddress_ZipCode", "c"."ShippingAddress_Country_Code", "c"."ShippingAddress_Country_FullName"
        FROM "Customer" AS "c"
        ORDER BY "c"."Id"
        LIMIT @__p_0
    ) AS "t"
) AS "t0"
LEFT JOIN (
    SELECT DISTINCT "t2"."Id", "t2"."Name", "t2"."BillingAddress_AddressLine1", "t2"."BillingAddress_AddressLine2", "t2"."BillingAddress_ZipCode", "t2"."BillingAddress_Country_Code", "t2"."BillingAddress_Country_FullName", "t2"."ShippingAddress_AddressLine1", "t2"."ShippingAddress_AddressLine2", "t2"."ShippingAddress_ZipCode", "t2"."ShippingAddress_Country_Code", "t2"."ShippingAddress_Country_FullName"
    FROM (
        SELECT "c0"."Id", "c0"."Name", "c0"."BillingAddress_AddressLine1", "c0"."BillingAddress_AddressLine2", "c0"."BillingAddress_ZipCode", "c0"."BillingAddress_Country_Code", "c0"."BillingAddress_Country_FullName", "c0"."ShippingAddress_AddressLine1", "c0"."ShippingAddress_AddressLine2", "c0"."ShippingAddress_ZipCode", "c0"."ShippingAddress_Country_Code", "c0"."ShippingAddress_Country_FullName"
        FROM "Customer" AS "c0"
        ORDER BY "c0"."Id" DESC
        LIMIT @__p_1
    ) AS "t2"
) AS "t1" ON "t0"."Id" = "t1"."Id"
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class ComplexTypeQuerySqliteFixture : ComplexTypeQueryRelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
