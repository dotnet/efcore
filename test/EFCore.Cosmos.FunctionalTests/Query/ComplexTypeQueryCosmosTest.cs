// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.TestModels.ComplexTypeModel;

namespace Microsoft.EntityFrameworkCore.Query;

public class ComplexTypeQueryCosmosTest(ComplexTypeQueryCosmosTest.ComplexTypeQueryCosmosFixture fixture) : ComplexTypeQueryTestBase<ComplexTypeQueryCosmosTest.ComplexTypeQueryCosmosFixture>(fixture)
{
    public override Task Filter_on_property_inside_complex_type_after_subquery(bool async)
        => AssertTranslationFailedWithDetails(() => base.Filter_on_property_inside_complex_type_after_subquery(async), CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Filter_on_property_inside_nested_complex_type_after_subquery(bool async)
        => AssertTranslationFailedWithDetails(() => base.Filter_on_property_inside_nested_complex_type_after_subquery(async), CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Filter_on_required_property_inside_required_complex_type_on_optional_navigation(bool async)
        => AssertTranslationFailedWithDetails(() => base.Filter_on_required_property_inside_required_complex_type_on_optional_navigation(async), CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(Customer), nameof(CustomerGroup)));

    public override Task Filter_on_required_property_inside_required_complex_type_on_required_navigation(bool async)
        => AssertTranslationFailedWithDetails(() => base.Filter_on_required_property_inside_required_complex_type_on_required_navigation(async), CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(Customer), nameof(CustomerGroup)));

    public override Task Project_complex_type_via_optional_navigation(bool async)
        => AssertTranslationFailedWithDetails(() => base.Project_complex_type_via_optional_navigation(async), CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(Customer), nameof(CustomerGroup)));

    public override Task Project_complex_type_via_required_navigation(bool async)
        => AssertTranslationFailedWithDetails(() => base.Project_complex_type_via_required_navigation(async), CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(Customer), nameof(CustomerGroup)));

    public override Task Load_complex_type_after_subquery_on_entity_type(bool async)
        => AssertTranslationFailedWithDetails(() => base.Load_complex_type_after_subquery_on_entity_type(async), CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Select_complex_type(bool async)
    => CosmosTestHelpers.Instance.NoSyncTest(async, async (async) =>
    {
        await base.Select_complex_type(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    });

    public override Task Select_nested_complex_type(bool async)
    => CosmosTestHelpers.Instance.NoSyncTest(async, async (async) =>
    {
        await base.Select_nested_complex_type(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    });

    public override Task Select_single_property_on_nested_complex_type(bool async)
    => CosmosTestHelpers.Instance.NoSyncTest(async, async (async) =>
    {
        await base.Select_single_property_on_nested_complex_type(async);

        AssertSql(
            """
SELECT VALUE c["ShippingAddress"]["Country"]["FullName"]
FROM root c
""");
    });

    public override Task Select_complex_type_Where(bool async)
    => CosmosTestHelpers.Instance.NoSyncTest(async, async (async) =>
    {
        await base.Select_complex_type_Where(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["ShippingAddress"]["ZipCode"] = 7728)
""");
    });

    public override async Task Select_complex_type_Distinct(bool async)
        => await AssertTranslationFailed(async () => await base.Select_complex_type_Distinct(async)); // Cosmos: Projecting out nested documents retrieves the entire document #34067

    public override Task Complex_type_equals_complex_type(bool async)
    => CosmosTestHelpers.Instance.NoSyncTest(async, async (async) =>
    {
        await base.Complex_type_equals_complex_type(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (((((c["ShippingAddress"]["AddressLine1"] = c["BillingAddress"]["AddressLine1"]) AND (c["ShippingAddress"]["AddressLine2"] = c["BillingAddress"]["AddressLine2"])) AND (c["ShippingAddress"]["Tags"] = c["BillingAddress"]["Tags"])) AND (c["ShippingAddress"]["ZipCode"] = c["BillingAddress"]["ZipCode"])) AND ((c["ShippingAddress"]["Country"]["Code"] = c["BillingAddress"]["Country"]["Code"]) AND (c["ShippingAddress"]["Country"]["FullName"] = c["BillingAddress"]["Country"]["FullName"])))
""");
    });

    public override Task Complex_type_equals_constant(bool async)
    => CosmosTestHelpers.Instance.NoSyncTest(async, async (async) =>
    {
        await base.Complex_type_equals_constant(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (((((c["ShippingAddress"]["AddressLine1"] = "804 S. Lakeshore Road") AND (c["ShippingAddress"]["AddressLine2"] = null)) AND (c["ShippingAddress"]["Tags"] = ["foo","bar"])) AND (c["ShippingAddress"]["ZipCode"] = 38654)) AND ((c["ShippingAddress"]["Country"]["Code"] = "US") AND (c["ShippingAddress"]["Country"]["FullName"] = "United States")))
""");
    });

    public override Task Complex_type_equals_parameter(bool async)
    => CosmosTestHelpers.Instance.NoSyncTest(async, async (async) =>
    {
        await base.Complex_type_equals_parameter(async);

        AssertSql(
            """
@entity_equality_address_AddressLine1='804 S. Lakeshore Road'
@entity_equality_address_AddressLine2=null
@entity_equality_address_Tags='["foo","bar"]'
@entity_equality_address_ZipCode='38654'
@entity_equality_entity_equality_address_Country_Code='US'
@entity_equality_entity_equality_address_Country_FullName='United States'

SELECT VALUE c
FROM root c
WHERE (((((c["ShippingAddress"]["AddressLine1"] = @entity_equality_address_AddressLine1) AND (c["ShippingAddress"]["AddressLine2"] = @entity_equality_address_AddressLine2)) AND (c["ShippingAddress"]["Tags"] = @entity_equality_address_Tags)) AND (c["ShippingAddress"]["ZipCode"] = @entity_equality_address_ZipCode)) AND ((c["ShippingAddress"]["Country"]["Code"] = @entity_equality_entity_equality_address_Country_Code) AND (c["ShippingAddress"]["Country"]["FullName"] = @entity_equality_entity_equality_address_Country_FullName)))
""");
    });

    public override Task Subquery_over_complex_type(bool async)
        => AssertTranslationFailedWithDetails(() => base.Subquery_over_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Contains_over_complex_type(bool async)
        => AssertTranslationFailedWithDetails(() => base.Contains_over_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Concat_complex_type(bool async)
        => AssertTranslationFailedWithDetails(() => base.Concat_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Concat_entity_type_containing_complex_property(bool async)
        => AssertTranslationFailedWithDetails(() => base.Concat_entity_type_containing_complex_property(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Union_entity_type_containing_complex_property(bool async)
        => AssertTranslationFailedWithDetails(() => base.Union_entity_type_containing_complex_property(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Union_complex_type(bool async)
        => AssertTranslationFailedWithDetails(() => base.Union_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Concat_property_in_complex_type(bool async)
        => AssertTranslationFailedWithDetails(() => base.Concat_property_in_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Union_property_in_complex_type(bool async)
        => AssertTranslationFailedWithDetails(() => base.Union_property_in_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Concat_two_different_complex_type(bool async)
        => AssertTranslationFailedWithDetails(() => base.Concat_two_different_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Union_two_different_complex_type(bool async)
        => AssertTranslationFailedWithDetails(() => base.Union_two_different_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Filter_on_property_inside_struct_complex_type(bool async)
    => CosmosTestHelpers.Instance.NoSyncTest(async, async (async) =>
    {
        await base.Filter_on_property_inside_struct_complex_type(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["ShippingAddress"]["ZipCode"] = 7728)
""");
    });

    public override Task Filter_on_property_inside_nested_struct_complex_type(bool async)
    => CosmosTestHelpers.Instance.NoSyncTest(async, async (async) =>
    {
        await base.Filter_on_property_inside_nested_struct_complex_type(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["ShippingAddress"]["Country"]["Code"] = "DE")
""");
    });

    public override Task Filter_on_property_inside_struct_complex_type_after_subquery(bool async)
        => AssertTranslationFailedWithDetails(() => base.Filter_on_property_inside_struct_complex_type_after_subquery(async), CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Filter_on_property_inside_nested_struct_complex_type_after_subquery(bool async)
        => AssertTranslationFailedWithDetails(() => base.Filter_on_property_inside_nested_struct_complex_type_after_subquery(async), CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Filter_on_required_property_inside_required_struct_complex_type_on_optional_navigation(bool async)
        => AssertTranslationFailedWithDetails(() => base.Filter_on_required_property_inside_required_struct_complex_type_on_optional_navigation(async), CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(ValuedCustomer), nameof(ValuedCustomerGroup)));

    public override Task Filter_on_required_property_inside_required_struct_complex_type_on_required_navigation(bool async)
        => AssertTranslationFailedWithDetails(() => base.Filter_on_required_property_inside_required_struct_complex_type_on_required_navigation(async), CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(ValuedCustomer), nameof(ValuedCustomerGroup)));

    public override Task Project_struct_complex_type_via_optional_navigation(bool async)
        => AssertTranslationFailedWithDetails(() => base.Project_struct_complex_type_via_optional_navigation(async), CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(ValuedCustomer), nameof(ValuedCustomerGroup)));

    public override Task Project_nullable_struct_complex_type_via_optional_navigation(bool async)
        => AssertTranslationFailedWithDetails(() => base.Project_nullable_struct_complex_type_via_optional_navigation(async), CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(ValuedCustomer), nameof(ValuedCustomerGroup)));

    public override Task Project_struct_complex_type_via_required_navigation(bool async)
        => AssertTranslationFailedWithDetails(() => base.Project_struct_complex_type_via_required_navigation(async), CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(ValuedCustomer), nameof(ValuedCustomerGroup)));

    public override Task Load_struct_complex_type_after_subquery_on_entity_type(bool async)
        => AssertTranslationFailedWithDetails(() => base.Load_struct_complex_type_after_subquery_on_entity_type(async), CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Select_struct_complex_type(bool async)
    => CosmosTestHelpers.Instance.NoSyncTest(async, async (async) =>
    {
        await base.Select_struct_complex_type(async);

        AssertSql(
                """
SELECT VALUE c
FROM root c
""");
    });

    public override Task Select_nested_struct_complex_type(bool async)
    => CosmosTestHelpers.Instance.NoSyncTest(async, async (async) =>
    {
        await base.Select_nested_struct_complex_type(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    });

    public override Task Select_single_property_on_nested_struct_complex_type(bool async)
    => CosmosTestHelpers.Instance.NoSyncTest(async, async (async) =>
    {
        await base.Select_single_property_on_nested_struct_complex_type(async);

        AssertSql(
            """
SELECT VALUE c["ShippingAddress"]["Country"]["FullName"]
FROM root c
""");
    });

    public override Task Select_struct_complex_type_Where(bool async)
    => CosmosTestHelpers.Instance.NoSyncTest(async, async (async) =>
    {
        await base.Select_struct_complex_type_Where(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["ShippingAddress"]["ZipCode"] = 7728)
""");
    });

    public override Task Select_struct_complex_type_Distinct(bool async)
        => AssertTranslationFailed(() => base.Select_struct_complex_type_Distinct(async)); // #34067

    public override Task Struct_complex_type_equals_struct_complex_type(bool async)
    => CosmosTestHelpers.Instance.NoSyncTest(async, async (async) =>
    {
        await base.Struct_complex_type_equals_struct_complex_type(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((((c["ShippingAddress"]["AddressLine1"] = c["BillingAddress"]["AddressLine1"]) AND (c["ShippingAddress"]["AddressLine2"] = c["BillingAddress"]["AddressLine2"])) AND (c["ShippingAddress"]["ZipCode"] = c["BillingAddress"]["ZipCode"])) AND ((c["ShippingAddress"]["Country"]["Code"] = c["BillingAddress"]["Country"]["Code"]) AND (c["ShippingAddress"]["Country"]["FullName"] = c["BillingAddress"]["Country"]["FullName"])))
""");
    });

    public override Task Struct_complex_type_equals_constant(bool async)
    => CosmosTestHelpers.Instance.NoSyncTest(async, async (async) =>
    {
        await base.Struct_complex_type_equals_constant(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((((c["ShippingAddress"]["AddressLine1"] = "804 S. Lakeshore Road") AND (c["ShippingAddress"]["AddressLine2"] = null)) AND (c["ShippingAddress"]["ZipCode"] = 38654)) AND ((c["ShippingAddress"]["Country"]["Code"] = "US") AND (c["ShippingAddress"]["Country"]["FullName"] = "United States")))
""");
    });

    public override Task Struct_complex_type_equals_parameter(bool async)
    => CosmosTestHelpers.Instance.NoSyncTest(async, async (async) =>
    {
        await base.Struct_complex_type_equals_parameter(async);

        AssertSql(
            """
@entity_equality_address_AddressLine1='804 S. Lakeshore Road'
@entity_equality_address_AddressLine2=null
@entity_equality_address_ZipCode='38654'
@entity_equality_entity_equality_address_Country_Code='US'
@entity_equality_entity_equality_address_Country_FullName='United States'

SELECT VALUE c
FROM root c
WHERE ((((c["ShippingAddress"]["AddressLine1"] = @entity_equality_address_AddressLine1) AND (c["ShippingAddress"]["AddressLine2"] = @entity_equality_address_AddressLine2)) AND (c["ShippingAddress"]["ZipCode"] = @entity_equality_address_ZipCode)) AND ((c["ShippingAddress"]["Country"]["Code"] = @entity_equality_entity_equality_address_Country_Code) AND (c["ShippingAddress"]["Country"]["FullName"] = @entity_equality_entity_equality_address_Country_FullName)))
""");
    });

    public override Task Subquery_over_struct_complex_type(bool async)
        => AssertTranslationFailedWithDetails(() => base.Subquery_over_struct_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Contains_over_struct_complex_type(bool async)
        => AssertTranslationFailedWithDetails(() => base.Contains_over_struct_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Concat_struct_complex_type(bool async)
        => AssertTranslationFailedWithDetails(() => base.Concat_struct_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Concat_entity_type_containing_struct_complex_property(bool async)
        => AssertTranslationFailedWithDetails(() => base.Concat_entity_type_containing_struct_complex_property(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Union_entity_type_containing_struct_complex_property(bool async)
        => AssertTranslationFailedWithDetails(() => base.Union_entity_type_containing_struct_complex_property(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Union_struct_complex_type(bool async)
        => AssertTranslationFailedWithDetails(() => base.Union_struct_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Concat_property_in_struct_complex_type(bool async)
        => AssertTranslationFailedWithDetails(() => base.Concat_property_in_struct_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Union_property_in_struct_complex_type(bool async)
        => AssertTranslationFailedWithDetails(() => base.Union_property_in_struct_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Concat_two_different_struct_complex_type(bool async)
        => AssertTranslationFailedWithDetails(() => base.Concat_two_different_struct_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Union_two_different_struct_complex_type(bool async)
        => AssertTranslationFailedWithDetails(() => base.Union_two_different_struct_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Project_same_entity_with_nested_complex_type_twice_with_pushdown(bool async)
        => AssertTranslationFailedWithDetails(() => base.Project_same_entity_with_nested_complex_type_twice_with_pushdown(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Project_same_nested_complex_type_twice_with_pushdown(bool async)
        => AssertTranslationFailedWithDetails(() => base.Project_same_nested_complex_type_twice_with_pushdown(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Project_same_entity_with_nested_complex_type_twice_with_double_pushdown(bool async)
        => AssertTranslationFailedWithDetails(() => base.Project_same_entity_with_nested_complex_type_twice_with_double_pushdown(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Project_same_nested_complex_type_twice_with_double_pushdown(bool async)
        => AssertTranslationFailedWithDetails(() => base.Project_same_nested_complex_type_twice_with_double_pushdown(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Project_same_entity_with_struct_nested_complex_type_twice_with_pushdown(bool async)
        => AssertTranslationFailedWithDetails(() => base.Project_same_entity_with_struct_nested_complex_type_twice_with_pushdown(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Project_same_struct_nested_complex_type_twice_with_pushdown(bool async)
        => AssertTranslationFailedWithDetails(() => base.Project_same_struct_nested_complex_type_twice_with_pushdown(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Project_same_entity_with_struct_nested_complex_type_twice_with_double_pushdown(bool async)
        => AssertTranslationFailedWithDetails(() => base.Project_same_entity_with_struct_nested_complex_type_twice_with_double_pushdown(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Project_same_struct_nested_complex_type_twice_with_double_pushdown(bool async)
        => AssertTranslationFailedWithDetails(() => base.Project_same_struct_nested_complex_type_twice_with_double_pushdown(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Union_of_same_entity_with_nested_complex_type_projected_twice_with_pushdown(bool async)
        => AssertTranslationFailedWithDetails(() => base.Union_of_same_entity_with_nested_complex_type_projected_twice_with_pushdown(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Union_of_same_entity_with_nested_complex_type_projected_twice_with_double_pushdown(bool async)
        => AssertTranslationFailedWithDetails(() => base.Union_of_same_entity_with_nested_complex_type_projected_twice_with_double_pushdown(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Union_of_same_nested_complex_type_projected_twice_with_pushdown(bool async)
        => AssertTranslationFailedWithDetails(() => base.Union_of_same_nested_complex_type_projected_twice_with_pushdown(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Union_of_same_nested_complex_type_projected_twice_with_double_pushdown(bool async)
        => AssertTranslationFailedWithDetails(() => base.Union_of_same_nested_complex_type_projected_twice_with_double_pushdown(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Same_entity_with_complex_type_projected_twice_with_pushdown_as_part_of_another_projection(bool async)
        => AssertTranslationFailed(() => base.Same_entity_with_complex_type_projected_twice_with_pushdown_as_part_of_another_projection(async));

    public override Task Same_complex_type_projected_twice_with_pushdown_as_part_of_another_projection(bool async)
        => AssertTranslationFailedWithDetails(() => base.Same_complex_type_projected_twice_with_pushdown_as_part_of_another_projection(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);


    #region GroupBy

    [ConditionalTheory(Skip = "#17313 Cosmos: Translate GroupBy")]
    public override async Task GroupBy_over_property_in_nested_complex_type(bool async)
    {
        await base.GroupBy_over_property_in_nested_complex_type(async);

        AssertSql(
            """

""");
    }

    [ConditionalTheory(Skip = "#17313 Cosmos: Translate GroupBy")]
    public override async Task GroupBy_over_complex_type(bool async)
    {
        await base.GroupBy_over_complex_type(async);

        AssertSql(
            """

""");
    }

    [ConditionalTheory(Skip = "#17313 Cosmos: Translate GroupBy")]
    public override async Task GroupBy_over_nested_complex_type(bool async)
    {
        await base.GroupBy_over_nested_complex_type(async);

        AssertSql(
            """

""");
    }

    [ConditionalTheory(Skip = "#17313 Cosmos: Translate GroupBy")]
    public override async Task Entity_with_complex_type_with_group_by_and_first(bool async)
    {
        await base.Entity_with_complex_type_with_group_by_and_first(async);

        AssertSql(
            """

""");
    }

    #endregion GroupBy

    public override Task Projecting_property_of_complex_type_using_left_join_with_pushdown(bool async)
        => AssertTranslationFailedWithDetails(() => base.Projecting_property_of_complex_type_using_left_join_with_pushdown(async), CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(Customer), nameof(CustomerGroup)));

    public override Task Projecting_complex_from_optional_navigation_using_conditional(bool async)
        => AssertTranslationFailedWithDetails(() => base.Projecting_complex_from_optional_navigation_using_conditional(async), CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(Customer), nameof(CustomerGroup)));

    public override Task Project_entity_with_complex_type_pushdown_and_then_left_join(bool async)
        => AssertTranslationFailedWithDetails(() => base.Project_entity_with_complex_type_pushdown_and_then_left_join(async), CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class ComplexTypeQueryCosmosFixture : ComplexTypeQueryFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
           => base.AddOptions(builder)
               .ConfigureWarnings(w => w.Ignore(CosmosEventId.NoPartitionKeyDefined).Ignore(CoreEventId.MappedEntityTypeIgnoredWarning));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);
            modelBuilder.Entity<Customer>().ToContainer("Customers");
            modelBuilder.Entity<CustomerGroup>().ToContainer("CustomerGroups");
            modelBuilder.Entity<ValuedCustomer>().ToContainer("ValuedCustomers");
            modelBuilder.Entity<ValuedCustomerGroup>().ToContainer("ValuedCustomerGroups");
        }
    }
}
