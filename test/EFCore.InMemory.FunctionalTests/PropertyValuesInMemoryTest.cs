// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class PropertyValuesInMemoryTest(PropertyValuesInMemoryTest.PropertyValuesInMemoryFixture fixture)
    : PropertyValuesTestBase<PropertyValuesInMemoryTest.PropertyValuesInMemoryFixture>(fixture)
{
    // Complex collection tests - InMemory provider doesn't support complex collections yet
    public override Task Complex_collection_current_values_can_be_accessed_as_a_property_dictionary()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Complex_collection_current_values_can_be_accessed_as_a_property_dictionary);

    public override Task Complex_collection_original_values_can_be_accessed_as_a_property_dictionary()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Complex_collection_original_values_can_be_accessed_as_a_property_dictionary);

    public override Task Complex_collection_store_values_can_be_accessed_as_a_property_dictionary()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Complex_collection_store_values_can_be_accessed_as_a_property_dictionary);

    public override Task Complex_collection_store_values_can_be_accessed_asynchronously_as_a_property_dictionary()
        => Assert.ThrowsAsync<InvalidOperationException>(
            base.Complex_collection_store_values_can_be_accessed_asynchronously_as_a_property_dictionary);

    public override void Setting_complex_collection_values_from_object_works()
        => Assert.Throws<InvalidOperationException>(base.Setting_complex_collection_values_from_object_works);

    public override void Setting_complex_collection_original_values_from_object_with_nulls_works()
        => Assert.Throws<InvalidOperationException>(base.Setting_complex_collection_original_values_from_object_with_nulls_works);

    public override void Setting_complex_collection_original_values_from_dictionary_with_nulls_works()
        => Assert.Throws<InvalidOperationException>(base.Setting_complex_collection_original_values_from_dictionary_with_nulls_works);

    public override void Setting_complex_collection_current_values_from_dictionary_works()
        => Assert.Throws<InvalidOperationException>(base.Setting_complex_collection_current_values_from_dictionary_works);

    public override void SetValues_throws_for_nested_complex_collection_with_non_list_value()
        => Assert.Throws<InvalidOperationException>(base.SetValues_throws_for_nested_complex_collection_with_non_list_value);

    public override void SetValues_throws_for_complex_collection_with_non_list_value()
        => Assert.Throws<InvalidOperationException>(base.SetValues_throws_for_complex_collection_with_non_list_value);

    public override void SetValues_throws_for_complex_collection_with_non_dictionary_item()
        => Assert.Throws<InvalidOperationException>(base.SetValues_throws_for_complex_collection_with_non_dictionary_item);

    public override void SetValues_throws_for_nested_complex_collection_with_non_dictionary_item()
        => Assert.Throws<InvalidOperationException>(base.SetValues_throws_for_nested_complex_collection_with_non_dictionary_item);

    public override void Setting_complex_collection_current_values_from_DTO_with_complex_metadata_access_works()
        => Assert.Throws<InvalidOperationException>(
            base.Setting_complex_collection_current_values_from_DTO_with_complex_metadata_access_works);

    public override void Setting_complex_collection_values_from_DTO_with_nulls_works()
        => Assert.Throws<InvalidOperationException>(base.Setting_complex_collection_values_from_DTO_with_nulls_works);

    public override void Setting_complex_collection_current_values_from_dictionary_with_nulls_works()
        => Assert.Throws<InvalidOperationException>(base.Setting_complex_collection_current_values_from_dictionary_with_nulls_works);

    public override void Setting_complex_collection_current_values_from_object_with_nulls_works()
        => Assert.Throws<InvalidOperationException>(base.Setting_complex_collection_current_values_from_object_with_nulls_works);

    public override void Using_complex_property_value_not_list_throws()
        => Assert.Throws<InvalidOperationException>(base.Using_complex_property_value_not_list_throws);

    public override void Setting_complex_collection_via_string_indexer_sets_the_value()
        => Assert.Throws<InvalidOperationException>(() => base.Setting_complex_collection_via_string_indexer_sets_the_value());

    public class PropertyValuesInMemoryFixture : PropertyValuesFixtureBase
    {
        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder)
                .ConfigureWarnings(w => w.Ignore(CoreEventId.MappedComplexPropertyIgnoredWarning)
                    .Ignore(CoreEventId.MappedEntityTypeIgnoredWarning))
                .EnableSensitiveDataLogging(false);

        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            // The in-memory database can store struct values in a property, so nested struct complex types are configured explicitly
            modelBuilder.Entity<Building>(b =>
            {
                b.ComplexProperty(
                    e => e.Culture, cb =>
                    {
                        cb.ComplexProperty(e => e.Manufacturer, ConfigureManufacturer);
                        cb.ComplexProperty(e => e.License, ConfigureLicense);
                    });
                b.ComplexProperty(
                    e => e.Milk, cb =>
                    {
                        cb.ComplexProperty(e => e.Manufacturer, ConfigureManufacturer);
                        cb.ComplexProperty(e => e.License, ConfigureLicense);
                    });
                b.ComplexProperty(
                    e => e.OptionalMilk, cb =>
                    {
                        cb.ComplexProperty(e => e.Manufacturer, ConfigureManufacturer);
                        cb.ComplexProperty(e => e.License, ConfigureLicense);
                    });
            });

            // In-memory database doesn't support complex collections
            modelBuilder.Ignore<School>();
        }

        private static void ConfigureManufacturer(ComplexPropertyBuilder<Manufacturer> builder)
        {
            builder.ComplexProperty(e => e.Tag);
            builder.ComplexProperty(e => e.Tog);
        }

        private static void ConfigureLicense(ComplexPropertyBuilder<License> builder)
        {
            builder.ComplexProperty(e => e.Tag);
            builder.ComplexProperty(e => e.Tog);
        }
    }
}
