// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
@__entity_equality_address_0_Code='US' (Size = 2)
@__entity_equality_address_0_FullName='United States' (Size = 13)

SELECT "c"."Id", "c"."Name", "c"."BillingAddress_AddressLine1", "c"."BillingAddress_AddressLine2", "c"."BillingAddress_ZipCode", "c"."BillingAddress_Country_Code", "c"."BillingAddress_Country_FullName", "c"."ShippingAddress_AddressLine1", "c"."ShippingAddress_AddressLine2", "c"."ShippingAddress_ZipCode", "c"."ShippingAddress_Country_Code", "c"."ShippingAddress_Country_FullName"
FROM "Customer" AS "c"
WHERE "c"."ShippingAddress_AddressLine1" = @__entity_equality_address_0_AddressLine1 AND "c"."ShippingAddress_AddressLine2" IS NULL AND "c"."ShippingAddress_ZipCode" = @__entity_equality_address_0_ZipCode AND "c"."ShippingAddress_Country_Code" = @__entity_equality_address_0_Code AND "c"."ShippingAddress_Country_FullName" = @__entity_equality_address_0_FullName
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
@__entity_equality_address_0_Code='US' (Size = 2)
@__entity_equality_address_0_FullName='United States' (Size = 13)

SELECT "c"."Id", "c"."Name", "c"."BillingAddress_AddressLine1", "c"."BillingAddress_AddressLine2", "c"."BillingAddress_ZipCode", "c"."BillingAddress_Country_Code", "c"."BillingAddress_Country_FullName", "c"."ShippingAddress_AddressLine1", "c"."ShippingAddress_AddressLine2", "c"."ShippingAddress_ZipCode", "c"."ShippingAddress_Country_Code", "c"."ShippingAddress_Country_FullName"
FROM "Customer" AS "c"
WHERE EXISTS (
    SELECT 1
    FROM "Customer" AS "c0"
    WHERE "c0"."ShippingAddress_AddressLine1" = @__entity_equality_address_0_AddressLine1 AND "c0"."ShippingAddress_AddressLine2" IS NULL AND "c0"."ShippingAddress_ZipCode" = @__entity_equality_address_0_ZipCode AND "c0"."ShippingAddress_Country_Code" = @__entity_equality_address_0_Code AND "c0"."ShippingAddress_Country_FullName" = @__entity_equality_address_0_FullName)
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
@__entity_equality_address_0_Code='US' (Size = 2)
@__entity_equality_address_0_FullName='United States' (Size = 13)

SELECT "v"."Id", "v"."Name", "v"."BillingAddress_AddressLine1", "v"."BillingAddress_AddressLine2", "v"."BillingAddress_ZipCode", "v"."BillingAddress_Country_Code", "v"."BillingAddress_Country_FullName", "v"."ShippingAddress_AddressLine1", "v"."ShippingAddress_AddressLine2", "v"."ShippingAddress_ZipCode", "v"."ShippingAddress_Country_Code", "v"."ShippingAddress_Country_FullName"
FROM "ValuedCustomer" AS "v"
WHERE "v"."ShippingAddress_AddressLine1" = @__entity_equality_address_0_AddressLine1 AND "v"."ShippingAddress_AddressLine2" IS NULL AND "v"."ShippingAddress_ZipCode" = @__entity_equality_address_0_ZipCode AND "v"."ShippingAddress_Country_Code" = @__entity_equality_address_0_Code AND "v"."ShippingAddress_Country_FullName" = @__entity_equality_address_0_FullName
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
@__entity_equality_address_0_Code='US' (Size = 2)
@__entity_equality_address_0_FullName='United States' (Size = 13)

SELECT "v"."Id", "v"."Name", "v"."BillingAddress_AddressLine1", "v"."BillingAddress_AddressLine2", "v"."BillingAddress_ZipCode", "v"."BillingAddress_Country_Code", "v"."BillingAddress_Country_FullName", "v"."ShippingAddress_AddressLine1", "v"."ShippingAddress_AddressLine2", "v"."ShippingAddress_ZipCode", "v"."ShippingAddress_Country_Code", "v"."ShippingAddress_Country_FullName"
FROM "ValuedCustomer" AS "v"
WHERE EXISTS (
    SELECT 1
    FROM "ValuedCustomer" AS "v0"
    WHERE "v0"."ShippingAddress_AddressLine1" = @__entity_equality_address_0_AddressLine1 AND "v0"."ShippingAddress_AddressLine2" IS NULL AND "v0"."ShippingAddress_ZipCode" = @__entity_equality_address_0_ZipCode AND "v0"."ShippingAddress_Country_Code" = @__entity_equality_address_0_Code AND "v0"."ShippingAddress_Country_FullName" = @__entity_equality_address_0_FullName)
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
