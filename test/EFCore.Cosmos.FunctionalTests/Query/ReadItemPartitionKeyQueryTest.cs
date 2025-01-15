// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class ReadItemPartitionKeyQueryTest : ReadItemPartitionKeyQueryTestBase<
    ReadItemPartitionKeyQueryTest.ReadItemPartitionKeyQueryFixture>
{
    public ReadItemPartitionKeyQueryTest(ReadItemPartitionKeyQueryFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Key_with_special_characters_1()
    {
        await base.Key_with_special_characters_1();

        AssertSql("""ReadItem(["Cat|1"], Cat|1)""");
    }

    public override async Task Key_with_special_characters_2()
    {
        await base.Key_with_special_characters_2();

        AssertSql("""ReadItem(["Cat2||"], Cat2||)""");
    }

    public override async Task Key_with_special_characters_3()
    {
        await base.Key_with_special_characters_3();

        AssertSql("""ReadItem(["Cat|3|$|5"], Cat|3|$|5)""");
    }

    public override async Task Key_with_special_characters_4()
    {
        await base.Key_with_special_characters_4();

        AssertSql(
            """
ReadItem(["|Cat|"], |Cat|)
""");
    }

    public override async Task Predicate_with_hierarchical_partition_key()
    {
        await base.Predicate_with_hierarchical_partition_key();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["$type"] = "HierarchicalPartitionKeyEntity")
""");
    }

    public override async Task Predicate_with_only_hierarchical_partition_key()
    {
        await base.Predicate_with_only_hierarchical_partition_key();

        AssertSql("""ReadItem(["PK1a",1.0,true], PK1a|1|True)""");
    }

    public override async Task Predicate_with_single_partition_key()
    {
        await base.Predicate_with_single_partition_key();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task Predicate_with_only_single_partition_key()
    {
        await base.Predicate_with_only_single_partition_key();

        AssertSql("""ReadItem(["PK1a"], PK1a)""");
    }

    public override async Task Predicate_with_partial_values_in_hierarchical_partition_key()
    {
        await base.Predicate_with_partial_values_in_hierarchical_partition_key();

        // Not ReadItem because no primary key value, but partial partition key value is extracted
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["$type"] = "HierarchicalPartitionKeyEntity")
""");
    }

    public override async Task Predicate_with_partial_values_and_gap_in_hierarchical_partition_key()
    {
        await base.Predicate_with_partial_values_and_gap_in_hierarchical_partition_key();

        // Not ReadItem because no primary key value, but partial partition key value is extracted
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "HierarchicalPartitionKeyEntity") AND c["PartitionKey3"])
""");
    }

    [ConditionalFact]
    public override async Task Predicate_with_partial_values_in_only_hierarchical_partition_key()
    {
        await base.Predicate_with_partial_values_in_only_hierarchical_partition_key();

        // Not ReadItem because part of primary key value missing
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["$type"] = "OnlyHierarchicalPartitionKeyEntity")
""");
    }

    public override async Task Predicate_with_hierarchical_partition_key_and_additional_things_in_predicate()
    {
        await base.Predicate_with_hierarchical_partition_key_and_additional_things_in_predicate();

        // Not ReadItem because no primary key value and additional predicate
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "HierarchicalPartitionKeyEntity") AND CONTAINS(c["Payload"], "3"))
""");
    }

    public override async Task Predicate_with_only_hierarchical_partition_key_and_additional_things_in_predicate()
    {
        await base.Predicate_with_only_hierarchical_partition_key_and_additional_things_in_predicate();

        // Not ReadItem because additional filter
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "OnlyHierarchicalPartitionKeyEntity") AND CONTAINS(c["Payload"], "3"))
""");
    }

    public override async Task WithPartitionKey_with_hierarchical_partition_key()
    {
        await base.WithPartitionKey_with_hierarchical_partition_key();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["$type"] = "HierarchicalPartitionKeyEntity")
""");
    }

    public override async Task WithPartitionKey_with_only_hierarchical_partition_key()
    {
        await base.WithPartitionKey_with_only_hierarchical_partition_key();

        // This could be ReadItem because all primary key values have been supplied, but it is a weird corner case.
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["$type"] = "OnlyHierarchicalPartitionKeyEntity")
""");
    }

    public override async Task WithPartitionKey_with_single_partition_key()
    {
        await base.WithPartitionKey_with_single_partition_key();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task WithPartitionKey_with_only_single_partition_key()
    {
        await base.WithPartitionKey_with_only_single_partition_key();

        // This could be ReadItem because the primary key value has been supplied, but it is a weird corner case.
        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task WithPartitionKey_with_partial_value_in_hierarchical_partition_key()
    {
        await base.WithPartitionKey_with_partial_value_in_hierarchical_partition_key();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["$type"] = "HierarchicalPartitionKeyEntity")
""");
    }

    public override async Task Both_WithPartitionKey_and_predicate_comparisons_with_different_values()
    {
        await base.Both_WithPartitionKey_and_predicate_comparisons_with_different_values();

        // Not ReadItem because no primary key value, among other things.
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["PartitionKey"] = "PK2")
""");
    }

    public override async Task Both_WithPartitionKey_and_predicate_comparisons_with_different_values_with_only_partition_key()
    {
        await base.Both_WithPartitionKey_and_predicate_comparisons_with_different_values_with_only_partition_key();

        // Not ReadItem because conflicting primary key values, among other things.
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["PartitionKey"] = "PK2a")
""");
    }

    public override async Task Both_WithPartitionKey_and_predicate_comparisons_with_same_values()
    {
        await base.Both_WithPartitionKey_and_predicate_comparisons_with_same_values();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["PartitionKey"] = "PK1")
""");
    }

    public override async Task Both_WithPartitionKey_and_predicate_comparisons_with_same_values_with_only_partition_key()
    {
        await base.Both_WithPartitionKey_and_predicate_comparisons_with_same_values_with_only_partition_key();

        AssertSql("""ReadItem(["PK1a"], PK1a)""");
    }

    public override async Task ReadItem_with_hierarchical_partition_key()
    {
        await base.ReadItem_with_hierarchical_partition_key();

        AssertSql("""ReadItem(["PK1",1.0,true], 31887258-bdf9-49b8-89b2-01b6aa741a4a)""");
    }

    public override async Task ReadItem_with_only_hierarchical_partition_key()
    {
        await base.ReadItem_with_only_hierarchical_partition_key();

        AssertSql("""ReadItem(["PK1a",1.0,true], PK1a|1|True)""");
    }

    public override async Task ReadItem_with_single_partition_key_constant()
    {
        await base.ReadItem_with_single_partition_key_constant();

        AssertSql("""ReadItem(["PK1"], b29bced8-e1e5-420e-82d7-1c7a51703d34)""");
    }

    public override async Task ReadItem_with_only_single_partition_key_constant()
    {
        await base.ReadItem_with_only_single_partition_key_constant();

        AssertSql("""ReadItem(["PK1a"], PK1a)""");
    }

    public override async Task ReadItem_with_single_partition_key_parameter()
    {
        await base.ReadItem_with_single_partition_key_parameter();

        AssertSql("""ReadItem(["PK1"], b29bced8-e1e5-420e-82d7-1c7a51703d34)""");
    }

    public override async Task ReadItem_with_only_single_partition_key_parameter()
    {
        await base.ReadItem_with_only_single_partition_key_parameter();

        AssertSql("""ReadItem(["PK1a"], PK1a)""");
    }

    public override async Task ReadItem_with_SingleAsync()
    {
        await base.ReadItem_with_SingleAsync();

        AssertSql("""ReadItem(["PK1"], b29bced8-e1e5-420e-82d7-1c7a51703d34)""");
    }

    public override async Task ReadItem_with_SingleAsync_with_only_partition_key()
    {
        await base.ReadItem_with_SingleAsync_with_only_partition_key();

        AssertSql("""ReadItem(["PK1a"], PK1a)""");
    }

    public override async Task ReadItem_with_inverse_comparison()
    {
        await base.ReadItem_with_inverse_comparison();

        AssertSql("""ReadItem(["PK1"], b29bced8-e1e5-420e-82d7-1c7a51703d34)""");
    }

    public override async Task ReadItem_with_inverse_comparison_with_only_partition_key()
    {
        await base.ReadItem_with_inverse_comparison_with_only_partition_key();

        AssertSql("""ReadItem(["PK1a"], PK1a)""");
    }

    public override async Task ReadItem_with_EF_Property()
    {
        await base.ReadItem_with_EF_Property();

        AssertSql("""ReadItem(["PK1"], b29bced8-e1e5-420e-82d7-1c7a51703d34)""");
    }

    public override async Task ReadItem_with_WithPartitionKey()
    {
        await base.ReadItem_with_WithPartitionKey();

        AssertSql("""ReadItem(["PK1"], b29bced8-e1e5-420e-82d7-1c7a51703d34)""");
    }

    public override async Task ReadItem_with_WithPartitionKey_with_only_partition_key()
    {
        await base.ReadItem_with_WithPartitionKey_with_only_partition_key();

        AssertSql("""ReadItem(["PK1a"], PK1a)""");
    }

    public override async Task Multiple_incompatible_predicate_comparisons_cause_no_ReadItem()
    {
        await base.Multiple_incompatible_predicate_comparisons_cause_no_ReadItem();

        // Not ReadItem because conflicting primary key values
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["id"] = "b29bced8-e1e5-420e-82d7-1c7a51703d34") AND (c["id"] = "3307a33b-7f28-49ef-9857-48f4e3ebcaed"))
""");
    }

    public override async Task Multiple_incompatible_predicate_comparisons_cause_no_ReadItem_with_only_partition_key()
    {
        await base.Multiple_incompatible_predicate_comparisons_cause_no_ReadItem_with_only_partition_key();

        // Not ReadItem because conflicting primary key values
        AssertSql(
            """
@partitionKey='PK1a'

SELECT VALUE c
FROM root c
WHERE ((c["id"] = "PK1a") AND (c["id"] = @partitionKey))
""");
    }

    public override async Task ReadItem_with_no_partition_key()
    {
        await base.ReadItem_with_no_partition_key();

        AssertSql("""ReadItem(None, 1)""");
    }

    public override async Task ReadItem_is_not_used_without_partition_key()
    {
        await base.ReadItem_is_not_used_without_partition_key();

        // Not ReadItem because no partition key
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["id"] = "b29bced8-e1e5-420e-82d7-1c7a51703d34")
""");
    }

    public override async Task ReadItem_with_non_existent_id()
    {
        await base.ReadItem_with_non_existent_id();

        AssertSql("""ReadItem(["PK1"], 50b66960-35be-40c5-bc3d-4c9f2799d4d1)""");
    }

    public override async Task ReadItem_with_AsNoTracking()
    {
        await base.ReadItem_with_AsNoTracking();

        AssertSql("""ReadItem(["PK1"], b29bced8-e1e5-420e-82d7-1c7a51703d34)""");
    }

    public override async Task ReadItem_with_AsNoTrackingWithIdentityResolution()
    {
        await base.ReadItem_with_AsNoTrackingWithIdentityResolution();

        AssertSql("""ReadItem(["PK1"], b29bced8-e1e5-420e-82d7-1c7a51703d34)""");
    }

    public override async Task ReadItem_with_shared_container()
    {
        await base.ReadItem_with_shared_container();

        AssertSql("""ReadItem(["PK1"], 1)""");
    }

    public override async Task ReadItem_for_base_type_with_shared_container()
    {
        await base.ReadItem_for_base_type_with_shared_container();

        AssertSql("""ReadItem(["PK2"], 4)""");
    }

    public override async Task ReadItem_for_child_type_with_shared_container()
    {
        await base.ReadItem_for_child_type_with_shared_container();

        AssertSql("""ReadItem(["PK2"], 5)""");
    }

    public override async Task ReadItem_with_single_explicit_discriminator_mapping()
    {
        await base.ReadItem_with_single_explicit_discriminator_mapping();

        AssertSql("""ReadItem(["PK1"], b29bced8-e1e5-420e-82d7-1c7a51703d34)""");
    }

    public override async Task ReadItem_with_single_explicit_incorrect_discriminator_mapping()
    {
        await base.ReadItem_with_single_explicit_incorrect_discriminator_mapping();

        // No ReadItem because discriminator value is incorrect
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["id"] = "b29bced8-e1e5-420e-82d7-1c7a51703d34") AND (c["$type"] = "DerivedSinglePartitionKeyEntity"))
""");
    }

    public override async Task ReadItem_with_single_explicit_parameterized_discriminator_mapping()
    {
        await base.ReadItem_with_single_explicit_parameterized_discriminator_mapping();

        AssertSql(
            """
@discriminator='SinglePartitionKeyEntity'

SELECT VALUE c
FROM root c
WHERE ((c["id"] = "b29bced8-e1e5-420e-82d7-1c7a51703d34") AND (c["$type"] = @discriminator))
OFFSET 0 LIMIT 2
""");
    }

    public class ReadItemPartitionKeyQueryFixture : ReadItemPartitionKeyQueryFixtureBase
    {
        protected override string StoreName
            => "ReadItemPartitionKeyQueryTest";

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(
                builder.ConfigureWarnings(
                    w => w.Ignore(CosmosEventId.NoPartitionKeyDefined)));
    }
}
