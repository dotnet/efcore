﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class ReadItemPartitionKeyQueryDiscriminatorInIdTest
    : ReadItemPartitionKeyQueryInheritanceTestBase<ReadItemPartitionKeyQueryDiscriminatorInIdTest.ReadItemPartitionKeyQueryFixture>
{
    public ReadItemPartitionKeyQueryDiscriminatorInIdTest(ReadItemPartitionKeyQueryFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Predicate_with_hierarchical_partition_key()
    {
        await base.Predicate_with_hierarchical_partition_key();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["Discriminator"] IN ("HierarchicalPartitionKeyEntity", "DerivedHierarchicalPartitionKeyEntity")
""");
    }

    public override async Task Predicate_with_only_hierarchical_partition_key()
    {
        await base.Predicate_with_only_hierarchical_partition_key();

        // Not ReadItem because discriminator value in the JSON id is unknown
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["Discriminator"] IN ("OnlyHierarchicalPartitionKeyEntity", "DerivedOnlyHierarchicalPartitionKeyEntity")
""");
    }

    public override async Task Predicate_with_single_partition_key()
    {
        await base.Predicate_with_single_partition_key();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["Discriminator"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity")
""");
    }

    public override async Task Predicate_with_only_single_partition_key()
    {
        await base.Predicate_with_only_single_partition_key();

        // Not ReadItem because discriminator value in the JSON id is unknown
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["Discriminator"] IN ("OnlySinglePartitionKeyEntity", "DerivedOnlySinglePartitionKeyEntity")
""");
    }

    public override async Task Predicate_with_partial_values_in_hierarchical_partition_key()
    {
        await base.Predicate_with_partial_values_in_hierarchical_partition_key();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("HierarchicalPartitionKeyEntity", "DerivedHierarchicalPartitionKeyEntity") AND ((c["PartitionKey1"] = "PK1") AND (c["PartitionKey2"] = 1)))
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
WHERE (c["Discriminator"] IN ("OnlyHierarchicalPartitionKeyEntity", "DerivedOnlyHierarchicalPartitionKeyEntity") AND ((c["PartitionKey1"] = "PK1a") AND (c["PartitionKey2"] = 1)))
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
WHERE (c["Discriminator"] IN ("HierarchicalPartitionKeyEntity", "DerivedHierarchicalPartitionKeyEntity") AND CONTAINS(c["Payload"], "3"))
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
WHERE (c["Discriminator"] IN ("OnlyHierarchicalPartitionKeyEntity", "DerivedOnlyHierarchicalPartitionKeyEntity") AND CONTAINS(c["Payload"], "3"))
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
WHERE c["Discriminator"] IN ("HierarchicalPartitionKeyEntity", "DerivedHierarchicalPartitionKeyEntity")
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
WHERE c["Discriminator"] IN ("OnlyHierarchicalPartitionKeyEntity", "DerivedOnlyHierarchicalPartitionKeyEntity")
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
WHERE c["Discriminator"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity")
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
WHERE c["Discriminator"] IN ("OnlySinglePartitionKeyEntity", "DerivedOnlySinglePartitionKeyEntity")
""");
    }

    public override async Task WithPartitionKey_with_missing_value_in_hierarchical_partition_key()
    {
        await base.WithPartitionKey_with_missing_value_in_hierarchical_partition_key();

        AssertSql();
    }

    public override async Task Both_WithPartitionKey_and_predicate_comparisons_with_different_values()
    {
        await base.Both_WithPartitionKey_and_predicate_comparisons_with_different_values();

        // Not ReadItem because no primary key value, among other things.
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity") AND (c["PartitionKey"] = "PK2"))
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
WHERE (c["Discriminator"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity") AND (c["PartitionKey"] = "PK2a"))
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
WHERE (c["Discriminator"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity") AND (c["PartitionKey"] = "PK1"))
""");
    }

    public override async Task Both_WithPartitionKey_and_predicate_comparisons_with_same_values_with_only_partition_key()
    {
        await base.Both_WithPartitionKey_and_predicate_comparisons_with_same_values_with_only_partition_key();

        // Not ReadItem because discriminator value in the JSON id is unknown
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("OnlySinglePartitionKeyEntity", "DerivedOnlySinglePartitionKeyEntity") AND (c["PartitionKey"] = "PK1a"))
""");
    }

    public override async Task ReadItem_with_hierarchical_partition_key()
    {
        await base.ReadItem_with_hierarchical_partition_key();

        // Not ReadItem because discriminator value in the JSON id is unknown
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("HierarchicalPartitionKeyEntity", "DerivedHierarchicalPartitionKeyEntity") AND (c["Id"] = 1))
""");
    }

    public override async Task ReadItem_with_only_hierarchical_partition_key()
    {
        await base.ReadItem_with_only_hierarchical_partition_key();

        // Not ReadItem because discriminator value in the JSON id is unknown
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["Discriminator"] IN ("OnlyHierarchicalPartitionKeyEntity", "DerivedOnlyHierarchicalPartitionKeyEntity")
""");
    }

    public override async Task ReadItem_with_single_partition_key_constant()
    {
        await base.ReadItem_with_single_partition_key_constant();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity") AND (c["Id"] = 1))
""");
    }

    public override async Task ReadItem_with_only_single_partition_key_constant()
    {
        await base.ReadItem_with_only_single_partition_key_constant();

        // Not ReadItem because discriminator value in the JSON id is unknown
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["Discriminator"] IN ("OnlySinglePartitionKeyEntity", "DerivedOnlySinglePartitionKeyEntity")
""");
    }

    public override async Task ReadItem_with_single_partition_key_parameter()
    {
        await base.ReadItem_with_single_partition_key_parameter();

        // Not ReadItem because discriminator value in the JSON id is unknown
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity") AND (c["Id"] = 1))
""");
    }

    public override async Task ReadItem_with_only_single_partition_key_parameter()
    {
        await base.ReadItem_with_only_single_partition_key_parameter();

        // Not ReadItem because discriminator value in the JSON id is unknown
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["Discriminator"] IN ("OnlySinglePartitionKeyEntity", "DerivedOnlySinglePartitionKeyEntity")
""");
    }

    public override async Task ReadItem_with_SingleAsync()
    {
        await base.ReadItem_with_SingleAsync();

        // Not ReadItem because discriminator value in the JSON id is unknown
        AssertSql("""
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity") AND (c["Id"] = 1))
OFFSET 0 LIMIT 2
""");
    }

    public override async Task ReadItem_with_SingleAsync_with_only_partition_key()
    {
        await base.ReadItem_with_SingleAsync_with_only_partition_key();

        // Not ReadItem because discriminator value in the JSON id is unknown
        AssertSql("""
SELECT VALUE c
FROM root c
WHERE c["Discriminator"] IN ("OnlySinglePartitionKeyEntity", "DerivedOnlySinglePartitionKeyEntity")
OFFSET 0 LIMIT 2
""");
    }

    public override async Task ReadItem_with_inverse_comparison()
    {
        await base.ReadItem_with_inverse_comparison();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity") AND (1 = c["Id"]))
""");
    }

    public override async Task ReadItem_with_inverse_comparison_with_only_partition_key()
    {
        await base.ReadItem_with_inverse_comparison_with_only_partition_key();

        // Not ReadItem because discriminator value in the JSON id is unknown
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["Discriminator"] IN ("OnlySinglePartitionKeyEntity", "DerivedOnlySinglePartitionKeyEntity")
""");
    }

    public override async Task ReadItem_with_EF_Property()
    {
        await base.ReadItem_with_EF_Property();

        // Not ReadItem because discriminator value in the JSON id is unknown
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity") AND (c["Id"] = 1))
""");
    }

    public override async Task ReadItem_with_WithPartitionKey()
    {
        await base.ReadItem_with_WithPartitionKey();

        // Not ReadItem because discriminator value in the JSON id is unknown
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity") AND (c["Id"] = 1))
""");
    }

    public override async Task ReadItem_with_WithPartitionKey_with_only_partition_key()
    {
        await base.ReadItem_with_WithPartitionKey_with_only_partition_key();
AssertSql(
    """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("OnlySinglePartitionKeyEntity", "DerivedOnlySinglePartitionKeyEntity") AND (c["PartitionKey"] = "PK1a"))
""");
    }

    public override async Task Multiple_incompatible_predicate_comparisons_cause_no_ReadItem()
    {
        await base.Multiple_incompatible_predicate_comparisons_cause_no_ReadItem();

        // Not ReadItem because conflicting primary key values
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity") AND ((c["Id"] = 1) AND (c["Id"] = 2)))
""");
    }


    public override async Task Multiple_incompatible_predicate_comparisons_cause_no_ReadItem_with_only_partition_key()
    {
        await base.Multiple_incompatible_predicate_comparisons_cause_no_ReadItem_with_only_partition_key();

        // Not ReadItem because conflicting primary key values
        AssertSql(
            """
@__partitionKey_0='PK1a'

SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("OnlySinglePartitionKeyEntity", "DerivedOnlySinglePartitionKeyEntity") AND ((c["PartitionKey"] = "PK1a") AND (c["PartitionKey"] = @__partitionKey_0)))
""");
    }

    public override async Task ReadItem_with_no_partition_key()
    {
        await base.ReadItem_with_no_partition_key();

        // Not ReadItem because discriminator value in the JSON id is unknown
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("NoPartitionKeyEntity", "DerivedNoPartitionKeyEntity") AND (c["Id"] = 1))
""");
    }

    public override async Task ReadItem_is_not_used_without_partition_key()
    {
        await base.ReadItem_is_not_used_without_partition_key();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity") AND (c["Id"] = 1))
""");
    }

    public override async Task ReadItem_with_non_existent_id()
    {
        await base.ReadItem_with_non_existent_id();
AssertSql(
    """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity") AND (c["Id"] = 999))
""");
    }

    public override async Task ReadItem_with_AsNoTracking()
    {
        await base.ReadItem_with_AsNoTracking();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity") AND (c["Id"] = 1))
""");
    }

    public override async Task ReadItem_with_AsNoTrackingWithIdentityResolution()
    {
        await base.ReadItem_with_AsNoTrackingWithIdentityResolution();

        // Not ReadItem because discriminator value in the JSON id is unknown
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity") AND (c["Id"] = 1))
""");
    }

    public override async Task ReadItem_with_shared_container()
    {
        // This one _is_ ReadItem because SharedContainerEntity1 doesn't have any derived types.
        await base.ReadItem_with_shared_container();

        AssertSql("""ReadItem(["PK1"], SharedContainerEntity1|1)""");
    }

    public override async Task ReadItem_for_base_type_with_shared_container()
    {
        // Not ReadItem because discriminator value in the JSON id is unknown
        await base.ReadItem_for_base_type_with_shared_container();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("SharedContainerEntity2", "SharedContainerEntity2Child") AND (c["Id"] = 4))
""");
    }

    public override async Task ReadItem_for_child_type_with_shared_container()
    {
        await base.ReadItem_for_child_type_with_shared_container();

        // This one _is_ ReadItem because SharedContainerEntity2Child is a leaf type.
        AssertSql("""ReadItem(["PK2"], SharedContainerEntity2Child|5)""");
    }


    public override async Task Predicate_with_hierarchical_partition_key_leaf()
    {
        await base.Predicate_with_hierarchical_partition_key_leaf();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "DerivedHierarchicalPartitionKeyEntity")
""");
    }

    public override async Task Predicate_with_only_hierarchical_partition_key_leaf()
    {
        await base.Predicate_with_only_hierarchical_partition_key_leaf();

        AssertSql("""ReadItem(["PK1c",1.0,true], DerivedOnlyHierarchicalPartitionKeyEntity|PK1c|1|True)""");
    }

    public override async Task Predicate_with_single_partition_key_leaf()
    {
        await base.Predicate_with_single_partition_key_leaf();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "DerivedSinglePartitionKeyEntity")
""");
    }

    public override async Task Predicate_with_only_single_partition_key_leaf()
    {
        await base.Predicate_with_only_single_partition_key_leaf();

        AssertSql("""ReadItem(["PK1c"], DerivedOnlySinglePartitionKeyEntity|PK1c)""");
    }

    public override async Task Predicate_with_partial_values_in_hierarchical_partition_key_leaf()
    {
        await base.Predicate_with_partial_values_in_hierarchical_partition_key_leaf();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "DerivedHierarchicalPartitionKeyEntity") AND ((c["PartitionKey1"] = "PK1") AND (c["PartitionKey2"] = 1)))
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
WHERE ((c["Discriminator"] = "DerivedOnlyHierarchicalPartitionKeyEntity") AND ((c["PartitionKey1"] = "PK1c") AND (c["PartitionKey2"] = 1)))
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
WHERE ((c["Discriminator"] = "DerivedHierarchicalPartitionKeyEntity") AND CONTAINS(c["Payload"], "3"))
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
WHERE ((c["Discriminator"] = "DerivedOnlyHierarchicalPartitionKeyEntity") AND CONTAINS(c["Payload"], "3"))
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
WHERE (c["Discriminator"] = "DerivedHierarchicalPartitionKeyEntity")
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
WHERE (c["Discriminator"] = "DerivedOnlyHierarchicalPartitionKeyEntity")
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
WHERE (c["Discriminator"] = "DerivedSinglePartitionKeyEntity")
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
WHERE (c["Discriminator"] = "DerivedOnlySinglePartitionKeyEntity")
""");
    }

    public override async Task WithPartitionKey_with_missing_value_in_hierarchical_partition_key_leaf()
    {
        await base.WithPartitionKey_with_missing_value_in_hierarchical_partition_key();

        AssertSql();
    }

    public override async Task Both_WithPartitionKey_and_predicate_comparisons_with_different_values_leaf()
    {
        await base.Both_WithPartitionKey_and_predicate_comparisons_with_different_values_leaf();

        // Not ReadItem because no primary key value, among other things.
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "DerivedSinglePartitionKeyEntity") AND (c["PartitionKey"] = "PK2"))
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
WHERE ((c["Discriminator"] = "DerivedSinglePartitionKeyEntity") AND (c["PartitionKey"] = "PK2c"))
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
WHERE ((c["Discriminator"] = "DerivedSinglePartitionKeyEntity") AND (c["PartitionKey"] = "PK1"))
""");
    }

    public override async Task Both_WithPartitionKey_and_predicate_comparisons_with_same_values_with_only_partition_key_leaf()
    {
        await base.Both_WithPartitionKey_and_predicate_comparisons_with_same_values_with_only_partition_key_leaf();

        AssertSql("""ReadItem(["PK1c"], DerivedOnlySinglePartitionKeyEntity|PK1c)""");
    }

    public override async Task ReadItem_with_hierarchical_partition_key_leaf()
    {
        await base.ReadItem_with_hierarchical_partition_key_leaf();

        AssertSql("""ReadItem(["PK1",1.0,true], DerivedHierarchicalPartitionKeyEntity|11)""");
    }

    public override async Task ReadItem_with_only_hierarchical_partition_key_leaf()
    {
        await base.ReadItem_with_only_hierarchical_partition_key_leaf();

        AssertSql("""ReadItem(["PK1c",1.0,true], DerivedOnlyHierarchicalPartitionKeyEntity|PK1c|1|True)""");
    }

    public override async Task ReadItem_with_single_partition_key_constant_leaf()
    {
        await base.ReadItem_with_single_partition_key_constant_leaf();

        AssertSql("""ReadItem(["PK1"], DerivedSinglePartitionKeyEntity|11)""");
    }

    public override async Task ReadItem_with_only_single_partition_key_constant_leaf()
    {
        await base.ReadItem_with_only_single_partition_key_constant_leaf();

        AssertSql("""ReadItem(["PK1c"], DerivedOnlySinglePartitionKeyEntity|PK1c)""");
    }

    public override async Task ReadItem_with_single_partition_key_parameter_leaf()
    {
        await base.ReadItem_with_single_partition_key_parameter_leaf();

        AssertSql("""ReadItem(["PK1"], DerivedSinglePartitionKeyEntity|11)""");
    }

    public override async Task ReadItem_with_only_single_partition_key_parameter_leaf()
    {
        await base.ReadItem_with_only_single_partition_key_parameter_leaf();

        AssertSql("""ReadItem(["PK1c"], DerivedOnlySinglePartitionKeyEntity|PK1c)""");
    }

    public override async Task ReadItem_with_SingleAsync_leaf()
    {
        await base.ReadItem_with_SingleAsync_leaf();

        AssertSql("""ReadItem(["PK1"], DerivedSinglePartitionKeyEntity|11)""");
    }

    public override async Task ReadItem_with_SingleAsync_with_only_partition_key_leaf()
    {
        await base.ReadItem_with_SingleAsync_with_only_partition_key_leaf();

        AssertSql("""ReadItem(["PK1c"], DerivedOnlySinglePartitionKeyEntity|PK1c)""");
    }

    public override async Task ReadItem_with_inverse_comparison_leaf()
    {
        await base.ReadItem_with_inverse_comparison_leaf();

        AssertSql("""ReadItem(["PK1"], DerivedSinglePartitionKeyEntity|11)""");
    }

    public override async Task ReadItem_with_inverse_comparison_with_only_partition_key_leaf()
    {
        await base.ReadItem_with_inverse_comparison_with_only_partition_key_leaf();

        AssertSql("""ReadItem(["PK1c"], DerivedOnlySinglePartitionKeyEntity|PK1c)""");
    }

    public override async Task ReadItem_with_EF_Property_leaf()
    {
        await base.ReadItem_with_EF_Property_leaf();

        AssertSql("""ReadItem(["PK1"], DerivedSinglePartitionKeyEntity|11)""");
    }

    public override async Task ReadItem_with_WithPartitionKey_leaf()
    {
        await base.ReadItem_with_WithPartitionKey_leaf();

        AssertSql("""ReadItem(["PK1"], DerivedSinglePartitionKeyEntity|11)""");
    }

    public override async Task ReadItem_with_WithPartitionKey_with_only_partition_key_leaf()
    {
        await base.ReadItem_with_WithPartitionKey_with_only_partition_key_leaf();

        AssertSql("""ReadItem(["PK1c"], DerivedOnlySinglePartitionKeyEntity|PK1c)""");
    }

    public override async Task Multiple_incompatible_predicate_comparisons_cause_no_ReadItem_leaf()
    {
        await base.Multiple_incompatible_predicate_comparisons_cause_no_ReadItem_leaf();

        // Not ReadItem because conflicting primary key values
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "DerivedSinglePartitionKeyEntity") AND ((c["Id"] = 11) AND (c["Id"] = 22)))
""");
    }

    public override async Task Multiple_incompatible_predicate_comparisons_cause_no_ReadItem_with_only_partition_key_leaf()
    {
        await base.Multiple_incompatible_predicate_comparisons_cause_no_ReadItem_with_only_partition_key_leaf();

        // Not ReadItem because conflicting primary key values
        AssertSql(
            """
@__partitionKey_0='PK1c'

SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "DerivedOnlySinglePartitionKeyEntity") AND ((c["PartitionKey"] = "PK1c") AND (c["PartitionKey"] = @__partitionKey_0)))
""");
    }

    public override async Task ReadItem_with_no_partition_key_leaf()
    {
        await base.ReadItem_with_no_partition_key_leaf();

        AssertSql("""ReadItem(None, DerivedNoPartitionKeyEntity|11)""");
    }

    public override async Task ReadItem_is_not_used_without_partition_key_leaf()
    {
        await base.ReadItem_is_not_used_without_partition_key_leaf();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "DerivedSinglePartitionKeyEntity") AND (c["Id"] = 11))
""");
    }

    public override async Task ReadItem_with_non_existent_id_leaf()
    {
        await base.ReadItem_with_non_existent_id_leaf();

        AssertSql("""ReadItem(["PK1"], DerivedSinglePartitionKeyEntity|999)""");
    }

    public override async Task ReadItem_with_AsNoTracking_leaf()
    {
        await base.ReadItem_with_AsNoTracking_leaf();

        AssertSql("""ReadItem(["PK1"], DerivedSinglePartitionKeyEntity|11)""");
    }

    public override async Task ReadItem_with_AsNoTrackingWithIdentityResolution_leaf()
    {
        await base.ReadItem_with_AsNoTrackingWithIdentityResolution_leaf();

        AssertSql("""ReadItem(["PK1"], DerivedSinglePartitionKeyEntity|11)""");
    }

    public override async Task ReadItem_with_single_explicit_discriminator_mapping()
    {
        await base.ReadItem_with_single_explicit_discriminator_mapping();

        // Not ReadItem because discriminator value in the JSON id is unknown
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity") AND ((c["Id"] = 1) AND (c["Discriminator"] = "SinglePartitionKeyEntity")))
OFFSET 0 LIMIT 2
""");
    }

    public override async Task ReadItem_with_single_explicit_incorrect_discriminator_mapping()
    {
        await base.ReadItem_with_single_explicit_incorrect_discriminator_mapping();

        // No ReadItem because discriminator value is incorrect
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity") AND ((c["Id"] = 1) AND (c["Discriminator"] = "DerivedSinglePartitionKeyEntity")))
""");
    }

    public override async Task ReadItem_with_single_explicit_parameterized_discriminator_mapping()
    {
        await base.ReadItem_with_single_explicit_parameterized_discriminator_mapping();

        // No ReadItem because discriminator check is parameterized
        AssertSql(
            """
@__discriminator_0='SinglePartitionKeyEntity'

SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("SinglePartitionKeyEntity", "DerivedSinglePartitionKeyEntity") AND ((c["Id"] = 1) AND (c["Discriminator"] = @__discriminator_0)))
OFFSET 0 LIMIT 2
""");
    }

    public override async Task ReadItem_with_single_explicit_discriminator_mapping_leaf()
    {
        await base.ReadItem_with_single_explicit_discriminator_mapping_leaf();

        AssertSql("""ReadItem(["PK1"], DerivedSinglePartitionKeyEntity|11)""");
    }

    public override async Task ReadItem_with_single_explicit_incorrect_discriminator_mapping_leaf()
    {
        await base.ReadItem_with_single_explicit_incorrect_discriminator_mapping_leaf();

        // No ReadItem because discriminator value is incorrect
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "DerivedSinglePartitionKeyEntity") AND ((c["Id"] = 11) AND (c["Discriminator"] = "SinglePartitionKeyEntity")))
""");
    }

    public override async Task ReadItem_with_single_explicit_parameterized_discriminator_mapping_leaf()
    {
        await base.ReadItem_with_single_explicit_parameterized_discriminator_mapping_leaf();

        // No ReadItem because discriminator check is parameterized
        AssertSql(
            """
@__discriminator_0='DerivedSinglePartitionKeyEntity'

SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "DerivedSinglePartitionKeyEntity") AND ((c["Id"] = 11) AND (c["Discriminator"] = @__discriminator_0)))
OFFSET 0 LIMIT 2
""");
    }

    public class ReadItemPartitionKeyQueryFixture : ReadItemPartitionKeyQueryInheritanceFixtureBase
    {
        protected override string StoreName
            => "PartitionKeyQueryDiscriminatorInIdTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.IncludeDiscriminatorInJsonId();
        }
    }
}
