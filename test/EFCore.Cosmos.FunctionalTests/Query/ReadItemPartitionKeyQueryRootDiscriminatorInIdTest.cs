// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class ReadItemPartitionKeyQueryRootDiscriminatorInIdTest
    : ReadItemPartitionKeyQueryInheritanceTestBase<ReadItemPartitionKeyQueryRootDiscriminatorInIdTest.ReadItemPartitionKeyQueryFixture>
{
    public ReadItemPartitionKeyQueryRootDiscriminatorInIdTest(ReadItemPartitionKeyQueryFixture fixture, ITestOutputHelper testOutputHelper)
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

        AssertSql("""ReadItem(["Cat|1"], FancyDiscriminatorEntity|Cat^|1)""");
    }

    public override async Task Key_with_special_characters_2()
    {
        await base.Key_with_special_characters_2();

        AssertSql("""ReadItem(["Cat2||"], FancyDiscriminatorEntity|Cat2^|^|)""");
    }

    public override async Task Key_with_special_characters_3()
    {
        await base.Key_with_special_characters_3();

        AssertSql("""ReadItem(["Cat|3|$|5"], FancyDiscriminatorEntity|Cat^|3^|$^|5)""");
    }

    public override async Task Key_with_special_characters_4()
    {
        await base.Key_with_special_characters_4();

        AssertSql("""ReadItem(["|Cat|"], FancyDiscriminatorEntity|^|Cat^|)""");
    }

    public override async Task Predicate_with_hierarchical_partition_key()
    {
        await base.Predicate_with_hierarchical_partition_key();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["$type"] IN ("HierarchicalPartitionKeyEntity", "DerivedHierarchicalPartitionKeyEntity")
""");
    }

    public override async Task Predicate_with_only_hierarchical_partition_key()
    {
        await base.Predicate_with_only_hierarchical_partition_key();

        AssertSql("""ReadItem(["PK1a",1.0,true], OnlyHierarchicalPartitionKeyEntity|PK1a|1|True)""");
    }

    public override async Task Predicate_with_single_partition_key()
    {
        await base.Predicate_with_single_partition_key();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["$type"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity")
""");
    }

    public override async Task Predicate_with_only_single_partition_key()
    {
        await base.Predicate_with_only_single_partition_key();

        AssertSql("""ReadItem(["PK1a"], OnlySinglePartitionKeyEntity|PK1a)""");
    }

    public override async Task Predicate_with_partial_values_in_hierarchical_partition_key()
    {
        await base.Predicate_with_partial_values_in_hierarchical_partition_key();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["$type"] IN ("HierarchicalPartitionKeyEntity", "DerivedHierarchicalPartitionKeyEntity")
""");
    }

    public override async Task Predicate_with_partial_values_and_gap_in_hierarchical_partition_key()
    {
        await base.Predicate_with_partial_values_and_gap_in_hierarchical_partition_key();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["$type"] IN ("HierarchicalPartitionKeyEntity", "DerivedHierarchicalPartitionKeyEntity") AND c["PartitionKey3"])
""");
    }

    public override async Task Predicate_with_partial_values_in_only_hierarchical_partition_key()
    {
        await base.Predicate_with_partial_values_in_only_hierarchical_partition_key();

        // Not ReadItem because part of primary key value missing
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["$type"] IN ("OnlyHierarchicalPartitionKeyEntity", "DerivedOnlyHierarchicalPartitionKeyEntity")
""");
    }

    [ConditionalFact] // #33960
    public override async Task Predicate_with_hierarchical_partition_key_and_additional_things_in_predicate()
    {
        await base.Predicate_with_hierarchical_partition_key_and_additional_things_in_predicate();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["$type"] IN ("HierarchicalPartitionKeyEntity", "DerivedHierarchicalPartitionKeyEntity") AND CONTAINS(c["Payload"], "3"))
""");
    }

    [ConditionalFact] // #33960
    public override async Task Predicate_with_only_hierarchical_partition_key_and_additional_things_in_predicate()
    {
        await base.Predicate_with_only_hierarchical_partition_key_and_additional_things_in_predicate();

        // Not ReadItem because additional filter
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["$type"] IN ("OnlyHierarchicalPartitionKeyEntity", "DerivedOnlyHierarchicalPartitionKeyEntity") AND CONTAINS(c["Payload"], "3"))
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
WHERE c["$type"] IN ("HierarchicalPartitionKeyEntity", "DerivedHierarchicalPartitionKeyEntity")
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
WHERE c["$type"] IN ("OnlyHierarchicalPartitionKeyEntity", "DerivedOnlyHierarchicalPartitionKeyEntity")
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
WHERE c["$type"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity")
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
WHERE c["$type"] IN ("OnlySinglePartitionKeyEntity", "DerivedOnlySinglePartitionKeyEntity")
""");
    }

    public override async Task WithPartitionKey_with_partial_value_in_hierarchical_partition_key()
    {
        await base.WithPartitionKey_with_partial_value_in_hierarchical_partition_key();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["$type"] IN ("HierarchicalPartitionKeyEntity", "DerivedHierarchicalPartitionKeyEntity")
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
WHERE (c["$type"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity") AND (c["PartitionKey"] = "PK2"))
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
WHERE (c["$type"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity") AND (c["PartitionKey"] = "PK2a"))
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
WHERE (c["$type"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity") AND (c["PartitionKey"] = "PK1"))
""");
    }

    public override async Task Both_WithPartitionKey_and_predicate_comparisons_with_same_values_with_only_partition_key()
    {
        await base.Both_WithPartitionKey_and_predicate_comparisons_with_same_values_with_only_partition_key();

        AssertSql("""ReadItem(["PK1a"], OnlySinglePartitionKeyEntity|PK1a)""");
    }

    public override async Task ReadItem_with_hierarchical_partition_key()
    {
        await base.ReadItem_with_hierarchical_partition_key();

        AssertSql("""ReadItem(["PK1",1.0,true], HierarchicalPartitionKeyEntity|31887258-bdf9-49b8-89b2-01b6aa741a4a)""");
    }

    public override async Task ReadItem_with_only_hierarchical_partition_key()
    {
        await base.ReadItem_with_only_hierarchical_partition_key();

        AssertSql("""ReadItem(["PK1a",1.0,true], OnlyHierarchicalPartitionKeyEntity|PK1a|1|True)""");
    }

    public override async Task ReadItem_with_single_partition_key_constant()
    {
        await base.ReadItem_with_single_partition_key_constant();

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|b29bced8-e1e5-420e-82d7-1c7a51703d34)""");
    }

    public override async Task ReadItem_with_only_single_partition_key_constant()
    {
        await base.ReadItem_with_only_single_partition_key_constant();

        AssertSql("""ReadItem(["PK1a"], OnlySinglePartitionKeyEntity|PK1a)""");
    }

    public override async Task ReadItem_with_single_partition_key_parameter()
    {
        await base.ReadItem_with_single_partition_key_parameter();

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|b29bced8-e1e5-420e-82d7-1c7a51703d34)""");
    }

    public override async Task ReadItem_with_only_single_partition_key_parameter()
    {
        await base.ReadItem_with_only_single_partition_key_parameter();

        AssertSql("""ReadItem(["PK1a"], OnlySinglePartitionKeyEntity|PK1a)""");
    }

    public override async Task ReadItem_with_SingleAsync()
    {
        await base.ReadItem_with_SingleAsync();

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|b29bced8-e1e5-420e-82d7-1c7a51703d34)""");
    }

    public override async Task ReadItem_with_SingleAsync_with_only_partition_key()
    {
        await base.ReadItem_with_SingleAsync_with_only_partition_key();

        AssertSql("""ReadItem(["PK1a"], OnlySinglePartitionKeyEntity|PK1a)""");
    }

    public override async Task ReadItem_with_inverse_comparison()
    {
        await base.ReadItem_with_inverse_comparison();

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|b29bced8-e1e5-420e-82d7-1c7a51703d34)""");
    }

    public override async Task ReadItem_with_inverse_comparison_with_only_partition_key()
    {
        await base.ReadItem_with_inverse_comparison_with_only_partition_key();

        AssertSql("""ReadItem(["PK1a"], OnlySinglePartitionKeyEntity|PK1a)""");
    }

    public override async Task ReadItem_with_EF_Property()
    {
        await base.ReadItem_with_EF_Property();

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|b29bced8-e1e5-420e-82d7-1c7a51703d34)""");
    }

    public override async Task ReadItem_with_WithPartitionKey()
    {
        await base.ReadItem_with_WithPartitionKey();

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|b29bced8-e1e5-420e-82d7-1c7a51703d34)""");
    }

    public override async Task ReadItem_with_WithPartitionKey_with_only_partition_key()
    {
        await base.ReadItem_with_WithPartitionKey_with_only_partition_key();

        AssertSql("""ReadItem(["PK1a"], OnlySinglePartitionKeyEntity|PK1a)""");
    }

    public override async Task Multiple_incompatible_predicate_comparisons_cause_no_ReadItem()
    {
        await base.Multiple_incompatible_predicate_comparisons_cause_no_ReadItem();

        // Not ReadItem because conflicting primary key values
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["$type"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity") AND ((c["Id"] = "b29bced8-e1e5-420e-82d7-1c7a51703d34") AND (c["Id"] = "3307a33b-7f28-49ef-9857-48f4e3ebcaed")))
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
WHERE (c["$type"] IN ("OnlySinglePartitionKeyEntity", "DerivedOnlySinglePartitionKeyEntity") AND ((c["PartitionKey"] = "PK1a") AND (c["PartitionKey"] = @partitionKey)))
""");
    }

    public override async Task ReadItem_with_no_partition_key()
    {
        await base.ReadItem_with_no_partition_key();

        AssertSql("""ReadItem(None, NoPartitionKeyEntity|1)""");
    }

    public override async Task ReadItem_is_not_used_without_partition_key()
    {
        await base.ReadItem_is_not_used_without_partition_key();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["$type"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity") AND (c["Id"] = "b29bced8-e1e5-420e-82d7-1c7a51703d34"))
""");
    }

    public override async Task ReadItem_with_non_existent_id()
    {
        await base.ReadItem_with_non_existent_id();

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|50b66960-35be-40c5-bc3d-4c9f2799d4d1)""");
    }

    public override async Task ReadItem_with_AsNoTracking()
    {
        await base.ReadItem_with_AsNoTracking();

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|b29bced8-e1e5-420e-82d7-1c7a51703d34)""");
    }

    public override async Task ReadItem_with_AsNoTrackingWithIdentityResolution()
    {
        await base.ReadItem_with_AsNoTrackingWithIdentityResolution();

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|b29bced8-e1e5-420e-82d7-1c7a51703d34)""");
    }

    public override async Task ReadItem_with_shared_container()
    {
        await base.ReadItem_with_shared_container();

        AssertSql("""ReadItem(["PK1"], SharedContainerEntity1|1)""");
    }

    public override async Task ReadItem_for_base_type_with_shared_container()
    {
        await base.ReadItem_for_base_type_with_shared_container();

        AssertSql("""ReadItem(["PK2"], SharedContainerEntity2|4)""");
    }

    public override async Task ReadItem_for_child_type_with_shared_container()
    {
        await base.ReadItem_for_child_type_with_shared_container();

        AssertSql("""ReadItem(["PK2"], SharedContainerEntity2|5)""");
    }

    public override async Task Predicate_with_hierarchical_partition_key_leaf()
    {
        await base.Predicate_with_hierarchical_partition_key_leaf();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["$type"] = "DerivedHierarchicalPartitionKeyEntity")
""");
    }

    public override async Task Predicate_with_only_hierarchical_partition_key_leaf()
    {
        await base.Predicate_with_only_hierarchical_partition_key_leaf();

        AssertSql("""ReadItem(["PK1c",1.0,true], OnlyHierarchicalPartitionKeyEntity|PK1c|1|True)""");
    }

    public override async Task Predicate_with_single_partition_key_leaf()
    {
        await base.Predicate_with_single_partition_key_leaf();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["$type"] = "DerivedSinglePartitionKeyEntity")
""");
    }

    public override async Task Predicate_with_only_single_partition_key_leaf()
    {
        await base.Predicate_with_only_single_partition_key_leaf();

        AssertSql("""ReadItem(["PK1c"], OnlySinglePartitionKeyEntity|PK1c)""");
    }

    public override async Task Predicate_with_partial_values_in_hierarchical_partition_key_leaf()
    {
        await base.Predicate_with_partial_values_in_hierarchical_partition_key_leaf();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["$type"] = "DerivedHierarchicalPartitionKeyEntity")
""");
    }

    public override async Task Predicate_with_partial_values_in_only_hierarchical_partition_key_leaf()
    {
        await base.Predicate_with_partial_values_in_only_hierarchical_partition_key_leaf();

        // Not ReadItem because part of primary key value missing
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["$type"] = "DerivedOnlyHierarchicalPartitionKeyEntity")
""");
    }

    [ConditionalFact] // #33960
    public override async Task Predicate_with_hierarchical_partition_key_and_additional_things_in_predicate_leaf()
    {
        await base.Predicate_with_hierarchical_partition_key_and_additional_things_in_predicate_leaf();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "DerivedHierarchicalPartitionKeyEntity") AND CONTAINS(c["Payload"], "3"))
""");
    }

    [ConditionalFact] // #33960
    public override async Task Predicate_with_only_hierarchical_partition_key_and_additional_things_in_predicate_leaf()
    {
        await base.Predicate_with_only_hierarchical_partition_key_and_additional_things_in_predicate_leaf();

        // Not ReadItem because additional filter
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "DerivedOnlyHierarchicalPartitionKeyEntity") AND CONTAINS(c["Payload"], "3"))
""");
    }

    public override async Task WithPartitionKey_with_hierarchical_partition_key_leaf()
    {
        await base.WithPartitionKey_with_hierarchical_partition_key_leaf();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["$type"] = "DerivedHierarchicalPartitionKeyEntity")
""");
    }

    public override async Task WithPartitionKey_with_only_hierarchical_partition_key_leaf()
    {
        await base.WithPartitionKey_with_only_hierarchical_partition_key_leaf();

        // This could be ReadItem because all primary key values have been supplied, but it is a weird corner case.
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["$type"] = "DerivedOnlyHierarchicalPartitionKeyEntity")
""");
    }

    public override async Task WithPartitionKey_with_single_partition_key_leaf()
    {
        await base.WithPartitionKey_with_single_partition_key_leaf();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["$type"] = "DerivedSinglePartitionKeyEntity")
""");
    }

    public override async Task WithPartitionKey_with_only_single_partition_key_leaf()
    {
        await base.WithPartitionKey_with_only_single_partition_key_leaf();

        // This could be ReadItem because the primary key value has been supplied, but it is a weird corner case.
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["$type"] = "DerivedOnlySinglePartitionKeyEntity")
""");
    }

    public override async Task WithPartitionKey_with_partial_value_in_hierarchical_partition_key_leaf()
    {
        await base.WithPartitionKey_with_partial_value_in_hierarchical_partition_key_leaf();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["$type"] = "DerivedHierarchicalPartitionKeyEntity")
""");
    }

    public override async Task Both_WithPartitionKey_and_predicate_comparisons_with_different_values_leaf()
    {
        await base.Both_WithPartitionKey_and_predicate_comparisons_with_different_values_leaf();

        // Not ReadItem because no primary key value, among other things.
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "DerivedSinglePartitionKeyEntity") AND (c["PartitionKey"] = "PK2"))
""");
    }

    public override async Task Both_WithPartitionKey_and_predicate_comparisons_with_different_values_with_only_partition_key_leaf()
    {
        await base.Both_WithPartitionKey_and_predicate_comparisons_with_different_values_with_only_partition_key_leaf();

        // Not ReadItem because conflicting primary key values, among other things.
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "DerivedSinglePartitionKeyEntity") AND (c["PartitionKey"] = "PK2c"))
""");
    }

    public override async Task Both_WithPartitionKey_and_predicate_comparisons_with_same_values_leaf()
    {
        await base.Both_WithPartitionKey_and_predicate_comparisons_with_same_values_leaf();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "DerivedSinglePartitionKeyEntity") AND (c["PartitionKey"] = "PK1"))
""");
    }

    public override async Task Both_WithPartitionKey_and_predicate_comparisons_with_same_values_with_only_partition_key_leaf()
    {
        await base.Both_WithPartitionKey_and_predicate_comparisons_with_same_values_with_only_partition_key_leaf();

        AssertSql("""ReadItem(["PK1c"], OnlySinglePartitionKeyEntity|PK1c)""");
    }

    public override async Task ReadItem_with_hierarchical_partition_key_leaf()
    {
        await base.ReadItem_with_hierarchical_partition_key_leaf();

        AssertSql("""ReadItem(["PK1",1.0,true], HierarchicalPartitionKeyEntity|316c846c-787f-44b9-aadf-272f1658c5ff)""");
    }

    public override async Task ReadItem_with_only_hierarchical_partition_key_leaf()
    {
        await base.ReadItem_with_only_hierarchical_partition_key_leaf();

        AssertSql("""ReadItem(["PK1c",1.0,true], OnlyHierarchicalPartitionKeyEntity|PK1c|1|True)""");
    }

    public override async Task ReadItem_with_single_partition_key_constant_leaf()
    {
        await base.ReadItem_with_single_partition_key_constant_leaf();

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|188d3253-81be-4a87-b58f-a2bd07e6b98c)""");
    }

    public override async Task ReadItem_with_only_single_partition_key_constant_leaf()
    {
        await base.ReadItem_with_only_single_partition_key_constant_leaf();

        AssertSql("""ReadItem(["PK1c"], OnlySinglePartitionKeyEntity|PK1c)""");
    }

    public override async Task ReadItem_with_single_partition_key_parameter_leaf()
    {
        await base.ReadItem_with_single_partition_key_parameter_leaf();

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|188d3253-81be-4a87-b58f-a2bd07e6b98c)""");
    }

    public override async Task ReadItem_with_only_single_partition_key_parameter_leaf()
    {
        await base.ReadItem_with_only_single_partition_key_parameter_leaf();

        AssertSql("""ReadItem(["PK1c"], OnlySinglePartitionKeyEntity|PK1c)""");
    }

    public override async Task ReadItem_with_SingleAsync_leaf()
    {
        await base.ReadItem_with_SingleAsync_leaf();

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|188d3253-81be-4a87-b58f-a2bd07e6b98c)""");
    }

    public override async Task ReadItem_with_SingleAsync_with_only_partition_key_leaf()
    {
        await base.ReadItem_with_SingleAsync_with_only_partition_key_leaf();

        AssertSql("""ReadItem(["PK1c"], OnlySinglePartitionKeyEntity|PK1c)""");
    }

    public override async Task ReadItem_with_inverse_comparison_leaf()
    {
        await base.ReadItem_with_inverse_comparison_leaf();

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|188d3253-81be-4a87-b58f-a2bd07e6b98c)""");
    }

    public override async Task ReadItem_with_inverse_comparison_with_only_partition_key_leaf()
    {
        await base.ReadItem_with_inverse_comparison_with_only_partition_key_leaf();

        AssertSql("""ReadItem(["PK1c"], OnlySinglePartitionKeyEntity|PK1c)""");
    }

    public override async Task ReadItem_with_EF_Property_leaf()
    {
        await base.ReadItem_with_EF_Property_leaf();

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|188d3253-81be-4a87-b58f-a2bd07e6b98c)""");
    }

    public override async Task ReadItem_with_WithPartitionKey_leaf()
    {
        await base.ReadItem_with_WithPartitionKey_leaf();

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|188d3253-81be-4a87-b58f-a2bd07e6b98c)""");
    }

    public override async Task ReadItem_with_WithPartitionKey_with_only_partition_key_leaf()
    {
        await base.ReadItem_with_WithPartitionKey_with_only_partition_key_leaf();

        AssertSql("""ReadItem(["PK1c"], OnlySinglePartitionKeyEntity|PK1c)""");
    }

    public override async Task Multiple_incompatible_predicate_comparisons_cause_no_ReadItem_leaf()
    {
        await base.Multiple_incompatible_predicate_comparisons_cause_no_ReadItem_leaf();

        // Not ReadItem because conflicting primary key values
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "DerivedSinglePartitionKeyEntity") AND ((c["Id"] = "188d3253-81be-4a87-b58f-a2bd07e6b98c") AND (c["Id"] = "11f8d1fd-7472-46f5-9e20-16af42b3b8d1")))
""");
    }

    public override async Task Multiple_incompatible_predicate_comparisons_cause_no_ReadItem_with_only_partition_key_leaf()
    {
        await base.Multiple_incompatible_predicate_comparisons_cause_no_ReadItem_with_only_partition_key_leaf();

        // Not ReadItem because conflicting primary key values
        AssertSql(
            """
@partitionKey='PK1c'

SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "DerivedOnlySinglePartitionKeyEntity") AND ((c["PartitionKey"] = "PK1c") AND (c["PartitionKey"] = @partitionKey)))
""");
    }

    public override async Task ReadItem_with_no_partition_key_leaf()
    {
        await base.ReadItem_with_no_partition_key_leaf();

        AssertSql("""ReadItem(None, NoPartitionKeyEntity|11)""");
    }

    public override async Task ReadItem_is_not_used_without_partition_key_leaf()
    {
        await base.ReadItem_is_not_used_without_partition_key_leaf();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "DerivedSinglePartitionKeyEntity") AND (c["Id"] = "188d3253-81be-4a87-b58f-a2bd07e6b98c"))
""");
    }

    public override async Task ReadItem_with_non_existent_id_leaf()
    {
        await base.ReadItem_with_non_existent_id_leaf();

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|b964beda-b4e1-4f5c-a729-0a35dae696fe)""");
    }

    public override async Task ReadItem_with_AsNoTracking_leaf()
    {
        await base.ReadItem_with_AsNoTracking_leaf();

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|188d3253-81be-4a87-b58f-a2bd07e6b98c)""");
    }

    public override async Task ReadItem_with_AsNoTrackingWithIdentityResolution_leaf()
    {
        await base.ReadItem_with_AsNoTrackingWithIdentityResolution_leaf();

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|188d3253-81be-4a87-b58f-a2bd07e6b98c)""");
    }

    public override async Task ReadItem_with_single_explicit_discriminator_mapping()
    {
        await base.ReadItem_with_single_explicit_discriminator_mapping();

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|b29bced8-e1e5-420e-82d7-1c7a51703d34)""");
    }

    public override async Task ReadItem_with_single_explicit_incorrect_discriminator_mapping()
    {
        await base.ReadItem_with_single_explicit_incorrect_discriminator_mapping();

        // No ReadItem because discriminator value is incorrect
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["$type"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity") AND ((c["Id"] = "b29bced8-e1e5-420e-82d7-1c7a51703d34") AND (c["$type"] = "DerivedSinglePartitionKeyEntity")))
""");
    }

    public override async Task ReadItem_with_single_explicit_parameterized_discriminator_mapping()
    {
        await base.ReadItem_with_single_explicit_parameterized_discriminator_mapping();

        // No ReadItem because discriminator check is parameterized
        AssertSql(
            """
@discriminator='SinglePartitionKeyEntity'

SELECT VALUE c
FROM root c
WHERE (c["$type"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity") AND ((c["Id"] = "b29bced8-e1e5-420e-82d7-1c7a51703d34") AND (c["$type"] = @discriminator)))
OFFSET 0 LIMIT 2
""");
    }

    public override async Task ReadItem_with_single_explicit_discriminator_mapping_leaf()
    {
        await base.ReadItem_with_single_explicit_discriminator_mapping_leaf();

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|188d3253-81be-4a87-b58f-a2bd07e6b98c)""");
    }

    public override async Task ReadItem_with_single_explicit_incorrect_discriminator_mapping_leaf()
    {
        await base.ReadItem_with_single_explicit_incorrect_discriminator_mapping_leaf();

        // No ReadItem because discriminator value is incorrect
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "DerivedSinglePartitionKeyEntity") AND ((c["Id"] = "188d3253-81be-4a87-b58f-a2bd07e6b98c") AND (c["$type"] = "SinglePartitionKeyEntity")))
""");
    }

    public override async Task ReadItem_with_single_explicit_parameterized_discriminator_mapping_leaf()
    {
        await base.ReadItem_with_single_explicit_parameterized_discriminator_mapping_leaf();

        // No ReadItem because discriminator check is parameterized
        AssertSql(
            """
@discriminator='DerivedSinglePartitionKeyEntity'

SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "DerivedSinglePartitionKeyEntity") AND ((c["Id"] = "188d3253-81be-4a87-b58f-a2bd07e6b98c") AND (c["$type"] = @discriminator)))
OFFSET 0 LIMIT 2
""");
    }

    public class ReadItemPartitionKeyQueryFixture : ReadItemPartitionKeyQueryInheritanceFixtureBase
    {
        protected override string StoreName
            => "PartitionKeyQueryRootDiscriminatorInIdTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.HasRootDiscriminatorInJsonId();
        }
    }
}
