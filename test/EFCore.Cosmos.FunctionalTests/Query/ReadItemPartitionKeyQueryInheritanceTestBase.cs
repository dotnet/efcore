﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class ReadItemPartitionKeyQueryInheritanceTestBase<TFixture> : ReadItemPartitionKeyQueryTestBase<TFixture>
    where TFixture : ReadItemPartitionKeyQueryInheritanceFixtureBase, new()
{
    protected ReadItemPartitionKeyQueryInheritanceTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual Task Predicate_with_hierarchical_partition_key_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedHierarchicalPartitionKeyEntity>()
                .Where(e => e.PartitionKey1 == "PK1" && e.PartitionKey2 == 1 && e.PartitionKey3));

    [ConditionalFact]
    public virtual Task Predicate_with_only_hierarchical_partition_key_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedOnlyHierarchicalPartitionKeyEntity>()
                .Where(e => e.PartitionKey1 == "PK1c" && e.PartitionKey2 == 1 && e.PartitionKey3));

    [ConditionalFact]
    public virtual Task Predicate_with_single_partition_key_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedSinglePartitionKeyEntity>().Where(e => e.PartitionKey == "PK1"));

    [ConditionalFact]
    public virtual Task Predicate_with_only_single_partition_key_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedOnlySinglePartitionKeyEntity>().Where(e => e.PartitionKey == "PK1c"));

    [ConditionalFact]
    public virtual Task Predicate_with_partial_values_in_hierarchical_partition_key_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedHierarchicalPartitionKeyEntity>()
                .Where(e => e.PartitionKey1 == "PK1" && e.PartitionKey2 == 1));

    [ConditionalFact]
    public virtual Task Predicate_with_partial_values_in_only_hierarchical_partition_key_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedOnlyHierarchicalPartitionKeyEntity>()
                .Where(e => e.PartitionKey1 == "PK1c" && e.PartitionKey2 == 1));

    [ConditionalFact] // #33960
    public virtual Task Predicate_with_hierarchical_partition_key_and_additional_things_in_predicate_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedHierarchicalPartitionKeyEntity>()
                .Where(e => e.Payload.Contains("3") && e.PartitionKey1 == "PK1" && e.PartitionKey2 == 1 && e.PartitionKey3));

    [ConditionalFact] // #33960
    public virtual Task Predicate_with_only_hierarchical_partition_key_and_additional_things_in_predicate_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedOnlyHierarchicalPartitionKeyEntity>()
                .Where(e => e.Payload.Contains("3") && e.PartitionKey1 == "PK1d" && e.PartitionKey2 == 1 && e.PartitionKey3));

    [ConditionalFact]
    public virtual Task WithPartitionKey_with_hierarchical_partition_key_leaf()
    {
        var partitionKey2 = 1;

        return AssertQuery(
            async: true,
            ss => ss.Set<DerivedHierarchicalPartitionKeyEntity>().WithPartitionKey("PK1", 1, true),
            ss => ss.Set<DerivedHierarchicalPartitionKeyEntity>()
                .Where(e => e.PartitionKey1 == "PK1" && e.PartitionKey2 == partitionKey2 && e.PartitionKey3));
    }

    [ConditionalFact]
    public virtual Task WithPartitionKey_with_only_hierarchical_partition_key_leaf()
    {
        var partitionKey2 = 1;

        return AssertQuery(
            async: true,
            ss => ss.Set<DerivedOnlyHierarchicalPartitionKeyEntity>().WithPartitionKey("PK1c", 1, true),
            ss => ss.Set<DerivedOnlyHierarchicalPartitionKeyEntity>()
                .Where(e => e.PartitionKey1 == "PK1c" && e.PartitionKey2 == partitionKey2 && e.PartitionKey3));
    }

    [ConditionalFact]
    public virtual Task WithPartitionKey_with_single_partition_key_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedSinglePartitionKeyEntity>().WithPartitionKey("PK1"),
            ss => ss.Set<DerivedSinglePartitionKeyEntity>().Where(e => e.PartitionKey == "PK1"));

    [ConditionalFact]
    public virtual Task WithPartitionKey_with_only_single_partition_key_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedOnlySinglePartitionKeyEntity>().WithPartitionKey("PK1c"),
            ss => ss.Set<DerivedOnlySinglePartitionKeyEntity>().Where(e => e.PartitionKey == "PK1c"));

    [ConditionalFact]
    public virtual async Task WithPartitionKey_with_missing_value_in_hierarchical_partition_key_leaf()
    {
        var message = await Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertQuery(
                async: true,
                ss => ss.Set<DerivedHierarchicalPartitionKeyEntity>().WithPartitionKey("PK1", 1),
                ss => ss.Set<DerivedHierarchicalPartitionKeyEntity>()
                    .Where(e => e.PartitionKey1 == "PK1" && e.PartitionKey2 == 1 && e.PartitionKey3)));

        Assert.Equal(CosmosStrings.IncorrectPartitionKeyNumber(nameof(DerivedHierarchicalPartitionKeyEntity), 2, 3), message.Message);
    }

    [ConditionalFact]
    public virtual Task Both_WithPartitionKey_and_predicate_comparisons_with_different_values_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedSinglePartitionKeyEntity>().WithPartitionKey("PK1").Where(e => e.PartitionKey == "PK2"),
            ss => ss.Set<DerivedSinglePartitionKeyEntity>().Where(e => e.PartitionKey == "PK1").Where(e => e.PartitionKey == "PK2"),
            assertEmpty: true);

    [ConditionalFact]
    public virtual Task Both_WithPartitionKey_and_predicate_comparisons_with_different_values_with_only_partition_key_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedSinglePartitionKeyEntity>().WithPartitionKey("PK1c").Where(e => e.PartitionKey == "PK2c"),
            ss => ss.Set<DerivedSinglePartitionKeyEntity>().Where(e => e.PartitionKey == "PK1c").Where(e => e.PartitionKey == "PK2c"),
            assertEmpty: true);

    [ConditionalFact]
    public virtual Task Both_WithPartitionKey_and_predicate_comparisons_with_same_values_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedSinglePartitionKeyEntity>()
                .WithPartitionKey("PK1")
                .Where(e => e.PartitionKey == "PK1"));

    [ConditionalFact]
    public virtual Task Both_WithPartitionKey_and_predicate_comparisons_with_same_values_with_only_partition_key_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedOnlySinglePartitionKeyEntity>()
                .WithPartitionKey("PK1c")
                .Where(e => e.PartitionKey == "PK1c"));

    [ConditionalFact]
    public virtual Task ReadItem_with_hierarchical_partition_key_leaf()
    {
        var partitionKey2 = 1;

        return AssertQuery(
            async: true,
            ss => ss.Set<DerivedHierarchicalPartitionKeyEntity>()
                .Where(e => e.Id == 11 && e.PartitionKey1 == "PK1" && e.PartitionKey2 == partitionKey2 && e.PartitionKey3));
    }

    [ConditionalFact]
    public virtual Task ReadItem_with_only_hierarchical_partition_key_leaf()
    {
        var partitionKey2 = 1;

        return AssertQuery(
            async: true,
            ss => ss.Set<DerivedOnlyHierarchicalPartitionKeyEntity>()
                .Where(e => e.PartitionKey1 == "PK1c" && e.PartitionKey2 == partitionKey2 && e.PartitionKey3));
    }

    [ConditionalFact]
    public virtual Task ReadItem_with_single_partition_key_constant_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedSinglePartitionKeyEntity>().Where(e => e.Id == 11 && e.PartitionKey == "PK1"));

    [ConditionalFact]
    public virtual Task ReadItem_with_only_single_partition_key_constant_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedOnlySinglePartitionKeyEntity>().Where(e => e.PartitionKey == "PK1c"));

    [ConditionalFact]
    public virtual Task ReadItem_with_single_partition_key_parameter_leaf()
    {
        var partitionKey = "PK1";

        return AssertQuery(
            async: true,
            ss => ss.Set<DerivedSinglePartitionKeyEntity>().Where(e => e.Id == 11 && e.PartitionKey == partitionKey));
    }

    [ConditionalFact]
    public virtual Task ReadItem_with_only_single_partition_key_parameter_leaf()
    {
        var partitionKey = "PK1c";

        return AssertQuery(
            async: true,
            ss => ss.Set<DerivedOnlySinglePartitionKeyEntity>().Where(e => e.PartitionKey == partitionKey));
    }

    [ConditionalFact]
    public virtual Task ReadItem_with_SingleAsync_leaf()
    {
        var partitionKey = "PK1";

        return AssertSingle(
            async: true,
            ss => ss.Set<DerivedSinglePartitionKeyEntity>().Where(e => e.Id == 11 && e.PartitionKey == partitionKey));
    }

    [ConditionalFact]
    public virtual Task ReadItem_with_SingleAsync_with_only_partition_key_leaf()
    {
        var partitionKey = "PK1c";

        return AssertSingle(
            async: true,
            ss => ss.Set<DerivedOnlySinglePartitionKeyEntity>().Where(e => e.PartitionKey == partitionKey));
    }

    [ConditionalFact]
    public virtual Task ReadItem_with_inverse_comparison_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedSinglePartitionKeyEntity>().Where(e => 11 == e.Id && "PK1" == e.PartitionKey));

    [ConditionalFact]
    public virtual Task ReadItem_with_inverse_comparison_with_only_partition_key_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedOnlySinglePartitionKeyEntity>().Where(e => "PK1c" == e.PartitionKey));

    [ConditionalFact]
    public virtual Task ReadItem_with_EF_Property_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedSinglePartitionKeyEntity>().Where(
                e => EF.Property<int>(e, nameof(DerivedSinglePartitionKeyEntity.Id)) == 11
                    && EF.Property<string>(e, nameof(DerivedSinglePartitionKeyEntity.PartitionKey)) == "PK1"));

    [ConditionalFact]
    public virtual Task ReadItem_with_WithPartitionKey_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedSinglePartitionKeyEntity>().WithPartitionKey("PK1").Where(e => e.Id == 11),
            ss => ss.Set<DerivedSinglePartitionKeyEntity>().Where(e => e.PartitionKey == "PK1").Where(e => e.Id == 11));

    [ConditionalFact]
    public virtual Task ReadItem_with_WithPartitionKey_with_only_partition_key_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedOnlySinglePartitionKeyEntity>().WithPartitionKey("PK1c").Where(e => e.PartitionKey == "PK1c"),
            ss => ss.Set<DerivedOnlySinglePartitionKeyEntity>().Where(e => e.PartitionKey == "PK1c"));

    [ConditionalFact]
    public virtual Task Multiple_incompatible_predicate_comparisons_cause_no_ReadItem_leaf()
    {
        var partitionKey = "PK1";

        return AssertQuery(
            async: true,
            ss => ss.Set<DerivedSinglePartitionKeyEntity>().Where(e => e.Id == 11 && e.Id == 22 && e.PartitionKey == partitionKey),
            assertEmpty: true);
    }

    [ConditionalFact]
    public virtual Task Multiple_incompatible_predicate_comparisons_cause_no_ReadItem_with_only_partition_key_leaf()
    {
        var partitionKey = "PK1c";

        return AssertQuery(
            async: true,
            ss => ss.Set<DerivedOnlySinglePartitionKeyEntity>()
                .Where(e => e.PartitionKey == "PK1d" && e.PartitionKey == "PK1c" && e.PartitionKey == partitionKey),
            assertEmpty: true);
    }

    [ConditionalFact]
    public virtual Task ReadItem_with_no_partition_key_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedNoPartitionKeyEntity>().Where(e => e.Id == 11));

    [ConditionalFact]
    public virtual Task ReadItem_is_not_used_without_partition_key_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedSinglePartitionKeyEntity>().Where(e => e.Id == 11));

    [ConditionalFact]
    public virtual Task ReadItem_with_non_existent_id_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedSinglePartitionKeyEntity>().Where(e => e.Id == 999 && e.PartitionKey == "PK1"),
            assertEmpty: true);

    [ConditionalFact]
    public virtual Task ReadItem_with_AsNoTracking_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedSinglePartitionKeyEntity>().AsNoTracking().Where(e => e.Id == 11 && e.PartitionKey == "PK1"));

    [ConditionalFact]
    public virtual Task ReadItem_with_AsNoTrackingWithIdentityResolution_leaf()
        => AssertQuery(
            async: true,
            ss => ss.Set<DerivedSinglePartitionKeyEntity>().AsNoTrackingWithIdentityResolution()
                .Where(e => e.Id == 11 && e.PartitionKey == "PK1"));

    [ConditionalFact]
    public virtual Task ReadItem_with_single_explicit_discriminator_mapping_leaf()
    {
        var partitionKey = "PK1";

        return AssertSingle(
            async: true,
            ss => ss.Set<DerivedSinglePartitionKeyEntity>()
                .Where(
                    e => e.Id == 11
                        && EF.Property<string>(e, "Discriminator") == nameof(DerivedSinglePartitionKeyEntity)
                        && e.PartitionKey == partitionKey),
            ss => ss.Set<DerivedSinglePartitionKeyEntity>()
                .Where(e => e.Id == 11 && e.PartitionKey == partitionKey));
    }

    [ConditionalFact]
    public virtual Task ReadItem_with_single_explicit_incorrect_discriminator_mapping_leaf()
    {
        var partitionKey = "PK1";

        return AssertQuery(
            async: true,
            ss => ss.Set<DerivedSinglePartitionKeyEntity>()
                .Where(
                    e => e.Id == 11
                        && EF.Property<string>(e, "Discriminator") == nameof(SinglePartitionKeyEntity)
                        && e.PartitionKey == partitionKey),
            ss => ss.Set<DerivedSinglePartitionKeyEntity>().Where(e => false),
            assertEmpty: true);
    }

    [ConditionalFact]
    public virtual Task ReadItem_with_single_explicit_parameterized_discriminator_mapping_leaf()
    {
        var partitionKey = "PK1";
        var discriminator = nameof(DerivedSinglePartitionKeyEntity);

        return AssertSingle(
            async: true,
            ss => ss.Set<DerivedSinglePartitionKeyEntity>()
                .Where(e => e.Id == 11 && EF.Property<string>(e, "Discriminator") == discriminator && e.PartitionKey == partitionKey),
            ss => ss.Set<DerivedSinglePartitionKeyEntity>().Where(e => e.Id == 11 && e.PartitionKey == partitionKey));
    }
}
