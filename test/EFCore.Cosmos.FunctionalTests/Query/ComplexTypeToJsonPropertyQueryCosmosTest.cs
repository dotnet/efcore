// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.TestModels.ComplexTypeModel;

namespace Microsoft.EntityFrameworkCore.Query;

public class ComplexTypeToJsonPropertyQueryCosmosTest(
    ComplexTypeToJsonPropertyQueryCosmosTest.ComplexTypeToJsonPropertyQueryCosmosFixture fixture)
    : ComplexTypeQueryTestBase<ComplexTypeToJsonPropertyQueryCosmosTest.ComplexTypeToJsonPropertyQueryCosmosFixture>(fixture),
        IClassFixture<ComplexTypeToJsonPropertyQueryCosmosTest.ComplexTypeToJsonPropertyQueryCosmosFixture>
{
    public override Task Filter_on_property_inside_complex_type_after_subquery(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Filter_on_property_inside_complex_type_after_subquery(async), CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Filter_on_property_inside_nested_complex_type_after_subquery(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Filter_on_property_inside_nested_complex_type_after_subquery(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Filter_on_required_property_inside_required_complex_type_on_optional_navigation(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Filter_on_required_property_inside_required_complex_type_on_optional_navigation(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(Customer), nameof(CustomerGroup)));

    public override Task Filter_on_required_property_inside_required_complex_type_on_required_navigation(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Filter_on_required_property_inside_required_complex_type_on_required_navigation(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(Customer), nameof(CustomerGroup)));

    public override Task Project_complex_type_via_optional_navigation(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Project_complex_type_via_optional_navigation(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(Customer), nameof(CustomerGroup)));

    public override Task Project_complex_type_via_required_navigation(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Project_complex_type_via_required_navigation(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(Customer), nameof(CustomerGroup)));

    public override Task Load_complex_type_after_subquery_on_entity_type(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Load_complex_type_after_subquery_on_entity_type(async), CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Select_complex_type(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async async =>
            {
                await base.Select_complex_type(async);

                AssertSql(
                    """
SELECT VALUE c["ShippingAddressRenamed"]
FROM root c
""");
            });

    public override Task Select_nested_complex_type(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async async =>
            {
                await base.Select_nested_complex_type(async);

                AssertSql(
                    """
SELECT VALUE c["ShippingAddressRenamed"]["CountryRenamed"]
FROM root c
""");
            });

    public override Task Select_single_property_on_nested_complex_type(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async async =>
            {
                await base.Select_single_property_on_nested_complex_type(async);

                AssertSql(
                    """
SELECT VALUE c["ShippingAddressRenamed"]["CountryRenamed"]["FullNameRenamed"]
FROM root c
""");
            });

    public override Task Select_complex_type_Where(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async async =>
            {
                await base.Select_complex_type_Where(async);

                AssertSql(
                    """
SELECT VALUE c["ShippingAddressRenamed"]
FROM root c
WHERE (c["ShippingAddressRenamed"]["ZipCodeRenamed"] = 7728)
""");
            });

    public override Task Select_complex_type_Distinct(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async async =>
            {
                await base.Select_complex_type_Distinct(async);

                AssertSql(
                    """
SELECT DISTINCT VALUE c["ShippingAddressRenamed"]
FROM root c
""");
            });

    public override Task Complex_type_equals_complex_type(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async async =>
            {
                await base.Complex_type_equals_complex_type(async);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["ShippingAddressRenamed"] = c["BillingAddressRenamed"])
""");
            });

    // https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/288 (Complex-type equality comparisons return no results)
    public override Task Complex_type_equals_constant(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async async =>
            {
                CosmosTestEnvironment.SkipOnLinuxEmulator();

                await base.Complex_type_equals_constant(async);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["ShippingAddressRenamed"] = {"AddressLine1Renamed":"804 S. Lakeshore Road","AddressLine2Renamed":null,"TagsRenamed":["foo","bar"],"ZipCodeRenamed":38654,"CountryRenamed":{"CodeRenamed":"US","FullNameRenamed":"United States"}})
""");
            });

    // https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/288 (Complex-type equality comparisons return no results)
    public override Task Complex_type_equals_parameter(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async async =>
            {
                CosmosTestEnvironment.SkipOnLinuxEmulator();

                await base.Complex_type_equals_parameter(async);

                AssertSql(
                    """
@entity_equality_address='{"AddressLine1Renamed":"804 S. Lakeshore Road","AddressLine2Renamed":null,"TagsRenamed":["foo","bar"],"ZipCodeRenamed":38654,"CountryRenamed":{"CodeRenamed":"US","FullNameRenamed":"United States"}}'

SELECT VALUE c
FROM root c
WHERE (c["ShippingAddressRenamed"] = @entity_equality_address)
""");
            });

    public override Task Subquery_over_complex_type(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Subquery_over_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Contains_over_complex_type(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Contains_over_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Concat_complex_type(bool async)
        => AssertTranslationFailedWithDetails(() => base.Concat_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Concat_entity_type_containing_complex_property(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Concat_entity_type_containing_complex_property(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Union_entity_type_containing_complex_property(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Union_entity_type_containing_complex_property(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Union_complex_type(bool async)
        => AssertTranslationFailedWithDetails(() => base.Union_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Concat_property_in_complex_type(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Concat_property_in_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Union_property_in_complex_type(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Union_property_in_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Concat_two_different_complex_type(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Concat_two_different_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Union_two_different_complex_type(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Union_two_different_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Filter_on_property_inside_struct_complex_type(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async async =>
            {
                await base.Filter_on_property_inside_struct_complex_type(async);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["ShippingAddressRenamed"]["ZipCodeRenamed"] = 7728)
""");
            });

    public override Task Filter_on_property_inside_nested_struct_complex_type(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async async =>
            {
                await base.Filter_on_property_inside_nested_struct_complex_type(async);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["ShippingAddressRenamed"]["CountryRenamed"]["CodeRenamed"] = "DE")
""");
            });

    public override Task Filter_on_property_inside_struct_complex_type_after_subquery(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Filter_on_property_inside_struct_complex_type_after_subquery(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Filter_on_property_inside_nested_struct_complex_type_after_subquery(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Filter_on_property_inside_nested_struct_complex_type_after_subquery(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Filter_on_required_property_inside_required_struct_complex_type_on_optional_navigation(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Filter_on_required_property_inside_required_struct_complex_type_on_optional_navigation(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(ValuedCustomer), nameof(ValuedCustomerGroup)));

    public override Task Filter_on_required_property_inside_required_struct_complex_type_on_required_navigation(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Filter_on_required_property_inside_required_struct_complex_type_on_required_navigation(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(ValuedCustomer), nameof(ValuedCustomerGroup)));

    public override Task Project_struct_complex_type_via_optional_navigation(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Project_struct_complex_type_via_optional_navigation(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(ValuedCustomer), nameof(ValuedCustomerGroup)));

    public override Task Project_nullable_struct_complex_type_via_optional_navigation(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Project_nullable_struct_complex_type_via_optional_navigation(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(ValuedCustomer), nameof(ValuedCustomerGroup)));

    public override Task Project_struct_complex_type_via_required_navigation(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Project_struct_complex_type_via_required_navigation(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(ValuedCustomer), nameof(ValuedCustomerGroup)));

    public override Task Load_struct_complex_type_after_subquery_on_entity_type(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Load_struct_complex_type_after_subquery_on_entity_type(async), CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Select_struct_complex_type(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async async =>
            {
                await base.Select_struct_complex_type(async);

                AssertSql(
                    """
SELECT VALUE c["ShippingAddressRenamed"]
FROM root c
""");
            });

    public override Task Select_nested_struct_complex_type(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async async =>
            {
                await base.Select_nested_struct_complex_type(async);

                AssertSql(
                    """
SELECT VALUE c["ShippingAddressRenamed"]["CountryRenamed"]
FROM root c
""");
            });

    public override Task Select_single_property_on_nested_struct_complex_type(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async async =>
            {
                await base.Select_single_property_on_nested_struct_complex_type(async);

                AssertSql(
                    """
SELECT VALUE c["ShippingAddressRenamed"]["CountryRenamed"]["FullNameRenamed"]
FROM root c
""");
            });

    public override Task Select_struct_complex_type_Where(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async async =>
            {
                await base.Select_struct_complex_type_Where(async);

                AssertSql(
                    """
SELECT VALUE c["ShippingAddressRenamed"]
FROM root c
WHERE (c["ShippingAddressRenamed"]["ZipCodeRenamed"] = 7728)
""");
            });

    public override Task Select_struct_complex_type_Distinct(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async async =>
            {
                await base.Select_struct_complex_type_Distinct(async);

                AssertSql(
                    """
SELECT DISTINCT VALUE c["ShippingAddressRenamed"]
FROM root c
""");
            });

    public override Task Struct_complex_type_equals_struct_complex_type(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async async =>
            {
                await base.Struct_complex_type_equals_struct_complex_type(async);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["ShippingAddressRenamed"] = c["BillingAddressRenamed"])
""");
            });

    // https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/288 (Complex-type equality comparisons return no results)
    public override Task Struct_complex_type_equals_constant(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async async =>
            {
                CosmosTestEnvironment.SkipOnLinuxEmulator();

                await base.Struct_complex_type_equals_constant(async);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["ShippingAddressRenamed"] = {"AddressLine1Renamed":"804 S. Lakeshore Road","AddressLine2Renamed":null,"ZipCodeRenamed":38654,"CountryRenamed":{"CodeRenamed":"US","FullNameRenamed":"United States"}})
""");
            });

    // https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/288 (Complex-type equality comparisons return no results)
    public override Task Struct_complex_type_equals_parameter(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async async =>
            {
                CosmosTestEnvironment.SkipOnLinuxEmulator();

                await base.Struct_complex_type_equals_parameter(async);

                AssertSql(
                    """
@entity_equality_address='{"AddressLine1Renamed":"804 S. Lakeshore Road","AddressLine2Renamed":null,"ZipCodeRenamed":38654,"CountryRenamed":{"CodeRenamed":"US","FullNameRenamed":"United States"}}'

SELECT VALUE c
FROM root c
WHERE (c["ShippingAddressRenamed"] = @entity_equality_address)
""");
            });

    public override Task Subquery_over_struct_complex_type(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Subquery_over_struct_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Contains_over_struct_complex_type(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Contains_over_struct_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Concat_struct_complex_type(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Concat_struct_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Concat_entity_type_containing_struct_complex_property(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Concat_entity_type_containing_struct_complex_property(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Union_entity_type_containing_struct_complex_property(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Union_entity_type_containing_struct_complex_property(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Union_struct_complex_type(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Union_struct_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Concat_property_in_struct_complex_type(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Concat_property_in_struct_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Union_property_in_struct_complex_type(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Union_property_in_struct_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Concat_two_different_struct_complex_type(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Concat_two_different_struct_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Union_two_different_struct_complex_type(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Union_two_different_struct_complex_type(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Project_same_entity_with_nested_complex_type_twice_with_pushdown(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Project_same_entity_with_nested_complex_type_twice_with_pushdown(async),
            CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Project_same_nested_complex_type_twice_with_pushdown(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Project_same_nested_complex_type_twice_with_pushdown(async), CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Project_same_entity_with_nested_complex_type_twice_with_double_pushdown(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Project_same_entity_with_nested_complex_type_twice_with_double_pushdown(async),
            CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Project_same_nested_complex_type_twice_with_double_pushdown(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Project_same_nested_complex_type_twice_with_double_pushdown(async),
            CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Project_same_entity_with_struct_nested_complex_type_twice_with_pushdown(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Project_same_entity_with_struct_nested_complex_type_twice_with_pushdown(async),
            CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Project_same_struct_nested_complex_type_twice_with_pushdown(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Project_same_struct_nested_complex_type_twice_with_pushdown(async),
            CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Project_same_entity_with_struct_nested_complex_type_twice_with_double_pushdown(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Project_same_entity_with_struct_nested_complex_type_twice_with_double_pushdown(async),
            CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Project_same_struct_nested_complex_type_twice_with_double_pushdown(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Project_same_struct_nested_complex_type_twice_with_double_pushdown(async),
            CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Union_of_same_entity_with_nested_complex_type_projected_twice_with_pushdown(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Union_of_same_entity_with_nested_complex_type_projected_twice_with_pushdown(async),
            CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Union_of_same_entity_with_nested_complex_type_projected_twice_with_double_pushdown(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Union_of_same_entity_with_nested_complex_type_projected_twice_with_double_pushdown(async),
            CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Union_of_same_nested_complex_type_projected_twice_with_pushdown(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Union_of_same_nested_complex_type_projected_twice_with_pushdown(async),
            CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Union_of_same_nested_complex_type_projected_twice_with_double_pushdown(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Union_of_same_nested_complex_type_projected_twice_with_double_pushdown(async),
            CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Same_entity_with_complex_type_projected_twice_with_pushdown_as_part_of_another_projection(bool async)
        => AssertTranslationFailed(()
            => base.Same_entity_with_complex_type_projected_twice_with_pushdown_as_part_of_another_projection(async));

    public override Task Same_complex_type_projected_twice_with_pushdown_as_part_of_another_projection(bool async)
        => AssertTranslationFailed(() => base.Same_complex_type_projected_twice_with_pushdown_as_part_of_another_projection(async));

    // https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/288 (Complex-type equality comparisons return no results)
    public override async Task Complex_type_equals_parameter_with_nested_types_with_property_of_same_name()
    {
        CosmosTestEnvironment.SkipOnLinuxEmulator();

        await base.Complex_type_equals_parameter_with_nested_types_with_property_of_same_name();
    }

    public override Task Projecting_complex_property_does_not_auto_include_owned_types()
        => base.Projecting_complex_property_does_not_auto_include_owned_types();

    public override Task Optional_complex_type_with_discriminator()
        => base.Optional_complex_type_with_discriminator();

    public override Task Non_optional_complex_type_with_all_nullable_properties()
        => base.Non_optional_complex_type_with_all_nullable_properties();

    public override async Task Non_optional_complex_type_with_all_nullable_properties_via_left_join()
    {
        Assert.Equal(
            CosmosStrings.UpdateConflict("1"),
            (await Assert.ThrowsAsync<DbUpdateException>(
                base.Non_optional_complex_type_with_all_nullable_properties_via_left_join)).Message);
    }

    public override Task Nullable_complex_type_with_discriminator_and_shadow_property()
        => base.Nullable_complex_type_with_discriminator_and_shadow_property();

    public override Task Nullable_complex_type_with_discriminator_null_to_non_null_roundtrip()
        => base.Nullable_complex_type_with_discriminator_null_to_non_null_roundtrip();

    public override Task Nullable_complex_type_with_discriminator_non_null_to_null_roundtrip()
        => base.Nullable_complex_type_with_discriminator_non_null_to_null_roundtrip();

    public override Task Nullable_complex_type_with_discriminator_update_non_null_entity_roundtrip()
        => base.Nullable_complex_type_with_discriminator_update_non_null_entity_roundtrip();

    public override Task Nullable_complex_type_with_discriminator_set_to_different_value()
        => base.Nullable_complex_type_with_discriminator_set_to_different_value();

    public override async Task Nullable_complex_type_with_discriminator_set_to_null()
    {
        // On Cosmos, setting the discriminator shadow property to null doesn't affect materialization
        // because the complex property's data is still present in the JSON document.
        var contextFactory = await InitializeNonSharedTest<Context38119>();

        Guid entityId;
        await using (var context = contextFactory.CreateDbContext())
        {
            var entity = new Context38119.EntityType
            {
                Id = Guid.NewGuid(),
                Prop = new Context38119.OptionalComplexProperty { OptionalValue = true }
            };
            context.Add(entity);
            entityId = entity.Id;

            var discriminatorEntry = context.Entry(entity).ComplexProperty(e => e.Prop).Property("Discriminator");
            Assert.Equal("OptionalComplexProperty", discriminatorEntry.CurrentValue);
            discriminatorEntry.CurrentValue = null;
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var entity = await context.Set<Context38119.EntityType>().SingleAsync(e => e.Id == entityId);
            Assert.NotNull(entity.Prop);
            Assert.True(entity.Prop.OptionalValue);
        }
    }

    public override Task Nested_nullable_complex_type_with_discriminator_null_to_non_null_roundtrip()
        => base.Nested_nullable_complex_type_with_discriminator_null_to_non_null_roundtrip();

    public override Task Update_entity_with_nullable_complex_type_and_discriminator_does_not_throw()
        => base.Update_entity_with_nullable_complex_type_and_discriminator_does_not_throw();

    public override Task Can_query_by_complex_type_property_with_index()
        => base.Can_query_by_complex_type_property_with_index();

    public override Task Can_update_entity_with_index_on_complex_type_property()
        => base.Can_update_entity_with_index_on_complex_type_property();

    public override Task Can_delete_entity_with_index_on_complex_type_property()
        => base.Can_delete_entity_with_index_on_complex_type_property();

    public override Task Can_query_by_alternate_key_on_complex_type_property()
        => base.Can_query_by_alternate_key_on_complex_type_property();

    public override async Task Can_save_batch_swapping_alternate_key_values_on_complex_type_property()
    {
        // Unlike relational providers, Cosmos can't ORDER BY the primary-key path 'Id.Id': the model defines an
        // explicit index (on Address.PostalCode), which disables automatic indexing and therefore excludes the
        // 'Id.Id' path from the index. Order client-side instead - the test's intent (reading the alternate-key
        // values while building batch edges) is unaffected.
        var contextFactory = await InitializeNonSharedTest<Context31246>(
            seed: context =>
            {
                context.AddRange(
                    new Context31246.Person
                    {
                        Id = new Context31246.StronglyTypedId(1),
                        Address = new Context31246.Address { City = "Seattle", PostalCode = "98101" }
                    },
                    new Context31246.Person
                    {
                        Id = new Context31246.StronglyTypedId(2),
                        Address = new Context31246.Address { City = "Redmond", PostalCode = "98052" }
                    });
                return context.SaveChangesAsync();
            });

        await using (var context = contextFactory.CreateDbContext())
        {
            var people = (await context.Set<Context31246.Person>().ToListAsync()).OrderBy(p => p.Id.Id).ToList();
            people[0].Address.PostalCode = "98103";
            people[1].Address.PostalCode = "98054";
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var people = (await context.Set<Context31246.Person>().ToListAsync()).OrderBy(p => p.Id.Id).ToList();
            Assert.Equal("98103", people[0].Address.PostalCode);
            Assert.Equal("98054", people[1].Address.PostalCode);
        }
    }

    #region GroupBy

    [Theory(Skip = "#17313 Cosmos: Translate GroupBy")]
    public override async Task GroupBy_over_property_in_nested_complex_type(bool async)
    {
        await base.GroupBy_over_property_in_nested_complex_type(async);

        AssertSql(
            """

""");
    }

    [Theory(Skip = "#17313 Cosmos: Translate GroupBy")]
    public override async Task GroupBy_over_complex_type(bool async)
    {
        await base.GroupBy_over_complex_type(async);

        AssertSql(
            """

""");
    }

    [Theory(Skip = "#17313 Cosmos: Translate GroupBy")]
    public override async Task GroupBy_over_nested_complex_type(bool async)
    {
        await base.GroupBy_over_nested_complex_type(async);

        AssertSql(
            """

""");
    }

    [Theory(Skip = "#17313 Cosmos: Translate GroupBy")]
    public override async Task Entity_with_complex_type_with_group_by_and_first(bool async)
    {
        await base.Entity_with_complex_type_with_group_by_and_first(async);

        AssertSql(
            """

""");
    }

    #endregion GroupBy

    public override Task Projecting_property_of_complex_type_using_left_join_with_pushdown(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Projecting_property_of_complex_type_using_left_join_with_pushdown(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(Customer), nameof(CustomerGroup)));

    public override Task Projecting_complex_from_optional_navigation_using_conditional(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Projecting_complex_from_optional_navigation_using_conditional(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(Customer), nameof(CustomerGroup)));

    public override Task Project_entity_with_complex_type_pushdown_and_then_left_join(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Project_entity_with_complex_type_pushdown_and_then_left_join(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class ComplexTypeToJsonPropertyQueryCosmosFixture : ComplexTypeQueryFixtureBase
    {
        protected override string StoreName
            => nameof(ComplexTypeToJsonPropertyQueryCosmosTest);

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
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                RenameProperties(entityType);
            }
        }

        private void RenameProperties(IMutableTypeBase typeBase)
        {
            foreach (var property in typeBase.GetProperties())
            {
                if (property.GetJsonPropertyName() == "id")
                {
                    continue;
                }

                property.SetJsonPropertyName(property.Name + "Renamed");
            }

            foreach (var property in typeBase.GetComplexProperties())
            {
                property.SetJsonPropertyName(property.Name + "Renamed");
                RenameProperties(property.ComplexType);
            }
        }
    }
}
