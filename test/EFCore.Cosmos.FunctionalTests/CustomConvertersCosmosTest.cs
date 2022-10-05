// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos;

public class CustomConvertersCosmosTest : CustomConvertersTestBase<CustomConvertersCosmosTest.CustomConvertersCosmosFixture>
{
    public CustomConvertersCosmosTest(CustomConvertersCosmosFixture fixture)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
    }

    [ConditionalTheory(Skip = "Issue #17246 No Explicit Convert")]
    public override Task Can_filter_projection_with_inline_enum_variable(bool async)
        => base.Can_filter_projection_with_inline_enum_variable(async);

    [ConditionalTheory(Skip = "Issue #17246 No Explicit Convert")]
    public override Task Can_filter_projection_with_captured_enum_variable(bool async)
        => base.Can_filter_projection_with_captured_enum_variable(async);

    [ConditionalFact(Skip = "Issue #17246 No Explicit Convert")]
    public override void Can_query_with_null_parameters_using_any_nullable_data_type()
        => base.Can_query_with_null_parameters_using_any_nullable_data_type();

    [ConditionalFact(Skip = "Issue #16920")]
    public override void Can_insert_and_read_back_with_string_key()
        => base.Can_insert_and_read_back_with_string_key();

    [ConditionalFact(Skip = "Issue #17246 No Explicit Convert")]
    public override void Can_query_and_update_with_conversion_for_custom_type()
        => base.Can_query_and_update_with_conversion_for_custom_type();

    [ConditionalFact(Skip = "Issue #16920")]
    public override void Can_query_and_update_with_nullable_converter_on_primary_key()
        => base.Can_query_and_update_with_nullable_converter_on_primary_key();

    [ConditionalFact(Skip = "Issue #16920")]
    public override void Can_insert_and_read_back_with_binary_key()
        => base.Can_insert_and_read_back_with_binary_key();

    [ConditionalFact(Skip = "Issue #16920")]
    public override void Can_insert_and_read_back_with_case_insensitive_string_key()
        => base.Can_insert_and_read_back_with_case_insensitive_string_key();

    [ConditionalFact(Skip = "Issue #17246 No Explicit Convert")]
    public override void Can_insert_and_query_struct_to_string_converter_for_pk()
        => base.Can_insert_and_query_struct_to_string_converter_for_pk();

    [ConditionalFact(Skip = "Issue #17670")]
    public override void Can_read_back_mapped_enum_from_collection_first_or_default()
        => base.Can_read_back_mapped_enum_from_collection_first_or_default();

    [ConditionalFact(Skip = "Issue #17246")]
    public override void Can_read_back_bool_mapped_as_int_through_navigation()
        => base.Can_read_back_bool_mapped_as_int_through_navigation();

    [ConditionalFact(Skip = "Issue #17246")]
    public override void Value_conversion_is_appropriately_used_for_join_condition()
        => base.Value_conversion_is_appropriately_used_for_join_condition();

    [ConditionalFact(Skip = "Issue #17246")]
    public override void Value_conversion_is_appropriately_used_for_left_join_condition()
        => base.Value_conversion_is_appropriately_used_for_left_join_condition();

    [ConditionalFact]
    public override void Where_bool_gets_converted_to_equality_when_value_conversion_is_used()
    {
        base.Where_bool_gets_converted_to_equality_when_value_conversion_is_used();

        AssertSql(
            @"SELECT c
FROM root c
WHERE (c[""Discriminator""] IN (""Blog"", ""RssBlog"") AND (c[""IsVisible""] = ""Y""))");
    }

    [ConditionalFact]
    public override void Where_negated_bool_gets_converted_to_equality_when_value_conversion_is_used()
    {
        base.Where_negated_bool_gets_converted_to_equality_when_value_conversion_is_used();

        AssertSql(
            @"SELECT c
FROM root c
WHERE (c[""Discriminator""] IN (""Blog"", ""RssBlog"") AND NOT((c[""IsVisible""] = ""Y"")))");
    }

    [ConditionalFact]
    public override void Where_bool_gets_converted_to_equality_when_value_conversion_is_used_using_EFProperty()
    {
        base.Where_bool_gets_converted_to_equality_when_value_conversion_is_used_using_EFProperty();

        AssertSql(
            @"SELECT c
FROM root c
WHERE (c[""Discriminator""] IN (""Blog"", ""RssBlog"") AND (c[""IsVisible""] = ""Y""))");
    }

    [ConditionalFact]
    public override void Where_bool_gets_converted_to_equality_when_value_conversion_is_used_using_indexer()
    {
        base.Where_bool_gets_converted_to_equality_when_value_conversion_is_used_using_indexer();

        AssertSql(
            @"SELECT c
FROM root c
WHERE (c[""Discriminator""] IN (""Blog"", ""RssBlog"") AND NOT((c[""IndexerVisible""] = ""Aye"")))");
    }

    [ConditionalFact(Skip = "Issue#27678")]
    public override void Optional_owned_with_converter_reading_non_nullable_column()
        => base.Optional_owned_with_converter_reading_non_nullable_column();

    public override void Value_conversion_on_enum_collection_contains()
        => Assert.Contains(
            CoreStrings.TranslationFailed("")[47..],
            Assert.Throws<InvalidOperationException>(() => base.Value_conversion_on_enum_collection_contains()).Message);

    public override void GroupBy_converted_enum()
    {
        Assert.Contains(
            CoreStrings.TranslationFailed("")[21..],
            Assert.Throws<InvalidOperationException>(() => base.GroupBy_converted_enum()).Message);
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class CustomConvertersCosmosFixture : CustomConvertersFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        public override bool StrictEquality
            => true;

        public override int IntegerPrecision
            => 53;

        public override bool SupportsAnsi
            => false;

        public override bool SupportsUnicodeToAnsiConversion
            => false;

        public override bool SupportsLargeStringComparisons
            => true;

        public override bool SupportsBinaryKeys
            => true;

        public override bool SupportsDecimalComparisons
            => true;

        public override DateTime DefaultDateTime
            => new();

        public override bool PreservesDateTimeKind
            => true;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            var shadowJObject = (Property)modelBuilder.Entity<BuiltInDataTypesShadow>().Property("__jObject").Metadata;
            shadowJObject.SetConfigurationSource(ConfigurationSource.Convention);
            var nullableShadowJObject = (Property)modelBuilder.Entity<BuiltInNullableDataTypesShadow>().Property("__jObject").Metadata;
            nullableShadowJObject.SetConfigurationSource(ConfigurationSource.Convention);

            modelBuilder.Entity<SimpleCounter>(b => b.ToContainer("SimpleCounters"));
        }
    }
}
