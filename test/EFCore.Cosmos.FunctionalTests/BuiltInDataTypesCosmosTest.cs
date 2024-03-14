// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class BuiltInDataTypesCosmosTest(BuiltInDataTypesCosmosTest.BuiltInDataTypesCosmosFixture fixture)
    : BuiltInDataTypesTestBase<BuiltInDataTypesCosmosTest.BuiltInDataTypesCosmosFixture>(fixture)
{
    [ConditionalTheory(Skip = "Issue #17246 No Explicit Convert")]
    public override Task Can_filter_projection_with_inline_enum_variable(bool async)
        => base.Can_filter_projection_with_inline_enum_variable(async);

    [ConditionalTheory(Skip = "Issue #17246 No Explicit Convert")]
    public override Task Can_filter_projection_with_captured_enum_variable(bool async)
        => base.Can_filter_projection_with_captured_enum_variable(async);

    [ConditionalFact(Skip = "Issue #17246 No Explicit Convert")]
    public override Task Can_query_with_null_parameters_using_any_nullable_data_type()
        => base.Can_query_with_null_parameters_using_any_nullable_data_type();

    [ConditionalFact(Skip = "Issue #16920")]
    public override Task Can_insert_and_read_back_with_string_key()
        => base.Can_insert_and_read_back_with_string_key();

    [ConditionalFact(Skip = "Issue #16920")]
    public override Task Can_insert_and_read_back_with_binary_key()
        => base.Can_insert_and_read_back_with_binary_key();

    public override Task Can_perform_query_with_max_length()
        // TODO: Better translation of sequential equality #17246
        => Task.CompletedTask;

    [ConditionalFact(Skip = "Issue #17670")]
    public override Task Can_read_back_mapped_enum_from_collection_first_or_default()
        => base.Can_read_back_mapped_enum_from_collection_first_or_default();

    [ConditionalFact(Skip = "Issue #17246")]
    public override Task Can_read_back_bool_mapped_as_int_through_navigation()
        => base.Can_read_back_bool_mapped_as_int_through_navigation();

    public override async Task Object_to_string_conversion()
    {
        await base.Object_to_string_conversion();

        AssertSql(
            """
SELECT c["TestSignedByte"], c["TestByte"], c["TestInt16"], c["TestUnsignedInt16"], c["TestInt32"], c["TestUnsignedInt32"], c["TestInt64"], c["TestUnsignedInt64"], c["TestSingle"], c["TestDouble"], c["TestDecimal"], c["TestCharacter"], c["TestDateTime"], c["TestDateTimeOffset"], c["TestTimeSpan"], c["TestDateOnly"], c["TestTimeOnly"]
FROM root c
WHERE ((c["Discriminator"] = "BuiltInDataTypes") AND (c["Id"] = 13))
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class BuiltInDataTypesCosmosFixture : BuiltInDataTypesFixtureBase
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

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        public override DateTime DefaultDateTime
            => new();

        public override bool PreservesDateTimeKind
            => true;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            var shadowJObject = (Property)modelBuilder.Entity<BuiltInDataTypesShadow>().Property("__jObject").Metadata;
            shadowJObject.SetConfigurationSource(ConfigurationSource.Convention);
            var nullableShadowJObject = (Property)modelBuilder.Entity<BuiltInNullableDataTypesShadow>().Property("__jObject").Metadata;
            nullableShadowJObject.SetConfigurationSource(ConfigurationSource.Convention);
        }
    }
}
