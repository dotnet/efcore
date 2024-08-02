﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class ReadItemPartitionKeyQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : ReadItemPartitionKeyQueryFixtureBase, new()
{
    protected ReadItemPartitionKeyQueryTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual Task Predicate_with_hierarchical_partition_key()
        => AssertQuery(
            async: true,
            ss => ss.Set<HierarchicalPartitionKeyEntity>()
                .Where(e => e.PartitionKey1 == "PK1" && e.PartitionKey2 == 1 && e.PartitionKey3));

    [ConditionalFact]
    public virtual Task Predicate_with_only_hierarchical_partition_key()
        => AssertQuery(
            async: true,
            ss => ss.Set<OnlyHierarchicalPartitionKeyEntity>()
                .Where(e => e.PartitionKey1 == "PK1a" && e.PartitionKey2 == 1 && e.PartitionKey3));

    [ConditionalFact]
    public virtual Task Predicate_with_single_partition_key()
        => AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().Where(e => e.PartitionKey == "PK1"));

    [ConditionalFact]
    public virtual Task Predicate_with_only_single_partition_key()
        => AssertQuery(
            async: true,
            ss => ss.Set<OnlySinglePartitionKeyEntity>().Where(e => e.PartitionKey == "PK1a"));

    [ConditionalFact]
    public virtual Task Predicate_with_partial_values_in_hierarchical_partition_key()
        => AssertQuery(
            async: true,
            ss => ss.Set<HierarchicalPartitionKeyEntity>()
                .Where(e => e.PartitionKey1 == "PK1" && e.PartitionKey2 == 1));

    [ConditionalFact]
    public virtual Task Predicate_with_partial_values_in_only_hierarchical_partition_key()
        => AssertQuery(
            async: true,
            ss => ss.Set<OnlyHierarchicalPartitionKeyEntity>()
                .Where(e => e.PartitionKey1 == "PK1a" && e.PartitionKey2 == 1));

    [ConditionalFact] // #33960
    public virtual Task Predicate_with_hierarchical_partition_key_and_additional_things_in_predicate()
        => AssertQuery(
            async: true,
            ss => ss.Set<HierarchicalPartitionKeyEntity>()
                .Where(e => e.Payload.Contains("3") && e.PartitionKey1 == "PK1" && e.PartitionKey2 == 1 && e.PartitionKey3));

    [ConditionalFact] // #33960
    public virtual Task Predicate_with_only_hierarchical_partition_key_and_additional_things_in_predicate()
        => AssertQuery(
            async: true,
            ss => ss.Set<OnlyHierarchicalPartitionKeyEntity>()
                .Where(e => e.Payload.Contains("3") && e.PartitionKey1 == "PK1b" && e.PartitionKey2 == 1 && e.PartitionKey3));

    [ConditionalFact]
    public virtual Task WithPartitionKey_with_hierarchical_partition_key()
    {
        var partitionKey2 = 1;

        return AssertQuery(
            async: true,
            ss => ss.Set<HierarchicalPartitionKeyEntity>().WithPartitionKey("PK1", 1, true),
            ss => ss.Set<HierarchicalPartitionKeyEntity>()
                .Where(e => e.PartitionKey1 == "PK1" && e.PartitionKey2 == partitionKey2 && e.PartitionKey3));
    }

    [ConditionalFact]
    public virtual Task WithPartitionKey_with_only_hierarchical_partition_key()
    {
        var partitionKey2 = 1;

        return AssertQuery(
            async: true,
            ss => ss.Set<OnlyHierarchicalPartitionKeyEntity>().WithPartitionKey("PK1a", 1, true),
            ss => ss.Set<OnlyHierarchicalPartitionKeyEntity>()
                .Where(e => e.PartitionKey1 == "PK1a" && e.PartitionKey2 == partitionKey2 && e.PartitionKey3));
    }

    [ConditionalFact]
    public virtual Task WithPartitionKey_with_single_partition_key()
        => AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().WithPartitionKey("PK1"),
            ss => ss.Set<SinglePartitionKeyEntity>().Where(e => e.PartitionKey == "PK1"));

    [ConditionalFact]
    public virtual Task WithPartitionKey_with_only_single_partition_key()
        => AssertQuery(
            async: true,
            ss => ss.Set<OnlySinglePartitionKeyEntity>().WithPartitionKey("PK1a"),
            ss => ss.Set<OnlySinglePartitionKeyEntity>().Where(e => e.PartitionKey == "PK1a"));

    [ConditionalFact]
    public virtual async Task WithPartitionKey_with_missing_value_in_hierarchical_partition_key()
    {
        var message = await Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertQuery(
                async: true,
                ss => ss.Set<HierarchicalPartitionKeyEntity>().WithPartitionKey("PK1", 1),
                ss => ss.Set<HierarchicalPartitionKeyEntity>()
                    .Where(e => e.PartitionKey1 == "PK1" && e.PartitionKey2 == 1 && e.PartitionKey3)));

        Assert.Equal(CosmosStrings.IncorrectPartitionKeyNumber(nameof(HierarchicalPartitionKeyEntity), 2, 3), message.Message);
    }

    [ConditionalFact]
    public virtual Task Both_WithPartitionKey_and_predicate_comparisons_with_different_values()
        => AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().WithPartitionKey("PK1").Where(e => e.PartitionKey == "PK2"),
            ss => ss.Set<SinglePartitionKeyEntity>().Where(e => e.PartitionKey == "PK1").Where(e => e.PartitionKey == "PK2"),
            assertEmpty: true);

    [ConditionalFact]
    public virtual Task Both_WithPartitionKey_and_predicate_comparisons_with_different_values_with_only_partition_key()
        => AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().WithPartitionKey("PK1a").Where(e => e.PartitionKey == "PK2a"),
            ss => ss.Set<SinglePartitionKeyEntity>().Where(e => e.PartitionKey == "PK1a").Where(e => e.PartitionKey == "PK2a"),
            assertEmpty: true);

    [ConditionalFact]
    public virtual Task Both_WithPartitionKey_and_predicate_comparisons_with_same_values()
        => AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>()
                .WithPartitionKey("PK1")
                .Where(e => e.PartitionKey == "PK1"));

    [ConditionalFact]
    public virtual Task Both_WithPartitionKey_and_predicate_comparisons_with_same_values_with_only_partition_key()
        => AssertQuery(
            async: true,
            ss => ss.Set<OnlySinglePartitionKeyEntity>()
                .WithPartitionKey("PK1a")
                .Where(e => e.PartitionKey == "PK1a"));

    [ConditionalFact]
    public virtual Task ReadItem_with_hierarchical_partition_key()
    {
        var partitionKey2 = 1;

        return AssertQuery(
            async: true,
            ss => ss.Set<HierarchicalPartitionKeyEntity>()
                .Where(e => e.Id == 1 && e.PartitionKey1 == "PK1" && e.PartitionKey2 == partitionKey2 && e.PartitionKey3));
    }

    [ConditionalFact]
    public virtual Task ReadItem_with_only_hierarchical_partition_key()
    {
        var partitionKey2 = 1;

        return AssertQuery(
            async: true,
            ss => ss.Set<OnlyHierarchicalPartitionKeyEntity>()
                .Where(e => e.PartitionKey1 == "PK1a" && e.PartitionKey2 == partitionKey2 && e.PartitionKey3));
    }

    [ConditionalFact]
    public virtual Task ReadItem_with_single_partition_key_constant()
        => AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().Where(e => e.Id == 1 && e.PartitionKey == "PK1"));

    [ConditionalFact]
    public virtual Task ReadItem_with_only_single_partition_key_constant()
        => AssertQuery(
            async: true,
            ss => ss.Set<OnlySinglePartitionKeyEntity>().Where(e => e.PartitionKey == "PK1a"));

    [ConditionalFact]
    public virtual Task ReadItem_with_single_partition_key_parameter()
    {
        var partitionKey = "PK1";

        return AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().Where(e => e.Id == 1 && e.PartitionKey == partitionKey));
    }

    [ConditionalFact]
    public virtual Task ReadItem_with_only_single_partition_key_parameter()
    {
        var partitionKey = "PK1a";

        return AssertQuery(
            async: true,
            ss => ss.Set<OnlySinglePartitionKeyEntity>().Where(e => e.PartitionKey == partitionKey));
    }

    [ConditionalFact]
    public virtual Task ReadItem_with_SingleAsync()
    {
        var partitionKey = "PK1";

        return AssertSingle(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().Where(e => e.Id == 1 && e.PartitionKey == partitionKey));
    }

    [ConditionalFact]
    public virtual Task ReadItem_with_SingleAsync_with_only_partition_key()
    {
        var partitionKey = "PK1a";

        return AssertSingle(
            async: true,
            ss => ss.Set<OnlySinglePartitionKeyEntity>().Where(e => e.PartitionKey == partitionKey));
    }

    [ConditionalFact]
    public virtual Task ReadItem_with_inverse_comparison()
        => AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().Where(e => 1 == e.Id && "PK1" == e.PartitionKey));

    [ConditionalFact]
    public virtual Task ReadItem_with_inverse_comparison_with_only_partition_key()
        => AssertQuery(
            async: true,
            ss => ss.Set<OnlySinglePartitionKeyEntity>().Where(e => "PK1a" == e.PartitionKey));

    [ConditionalFact]
    public virtual Task ReadItem_with_EF_Property()
        => AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().Where(
                e => EF.Property<int>(e, nameof(SinglePartitionKeyEntity.Id)) == 1
                    && EF.Property<string>(e, nameof(SinglePartitionKeyEntity.PartitionKey)) == "PK1"));

    [ConditionalFact]
    public virtual Task ReadItem_with_WithPartitionKey()
        => AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().WithPartitionKey("PK1").Where(e => e.Id == 1),
            ss => ss.Set<SinglePartitionKeyEntity>().Where(e => e.PartitionKey == "PK1").Where(e => e.Id == 1));

    [ConditionalFact]
    public virtual Task ReadItem_with_WithPartitionKey_with_only_partition_key()
        => AssertQuery(
            async: true,
            ss => ss.Set<OnlySinglePartitionKeyEntity>().WithPartitionKey("PK1a").Where(e => e.PartitionKey == "PK1a"),
            ss => ss.Set<OnlySinglePartitionKeyEntity>().Where(e => e.PartitionKey == "PK1a"));

    [ConditionalFact]
    public virtual Task Multiple_incompatible_predicate_comparisons_cause_no_ReadItem()
    {
        var partitionKey = "PK1";

        return AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().Where(e => e.Id == 1 && e.Id == 2 && e.PartitionKey == partitionKey),
            assertEmpty: true);
    }

    [ConditionalFact]
    public virtual Task Multiple_incompatible_predicate_comparisons_cause_no_ReadItem_with_only_partition_key()
    {
        var partitionKey = "PK1a";

        return AssertQuery(
            async: true,
            ss => ss.Set<OnlySinglePartitionKeyEntity>()
                .Where(e => e.PartitionKey == "PK1b" && e.PartitionKey == "PK1a" && e.PartitionKey == partitionKey),
            assertEmpty: true);
    }

    [ConditionalFact]
    public virtual Task ReadItem_with_no_partition_key()
        => AssertQuery(
            async: true,
            ss => ss.Set<NoPartitionKeyEntity>().Where(e => e.Id == 1));

    [ConditionalFact]
    public virtual Task ReadItem_is_not_used_without_partition_key()
        => AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().Where(e => e.Id == 1));

    [ConditionalFact]
    public virtual Task ReadItem_with_non_existent_id()
        => AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().Where(e => e.Id == 999 && e.PartitionKey == "PK1"),
            assertEmpty: true);

    [ConditionalFact]
    public virtual Task ReadItem_with_AsNoTracking()
        => AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().AsNoTracking().Where(e => e.Id == 1 && e.PartitionKey == "PK1"));

    [ConditionalFact]
    public virtual Task ReadItem_with_AsNoTrackingWithIdentityResolution()
        => AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().AsNoTrackingWithIdentityResolution().Where(e => e.Id == 1 && e.PartitionKey == "PK1"));

    [ConditionalFact]
    public virtual Task ReadItem_with_shared_container()
        => AssertQuery(
            async: true,
            ss => ss.Set<SharedContainerEntity1>().Where(e => e.Id == "1" && e.PartitionKey == "PK1"));

    [ConditionalFact]
    public virtual Task ReadItem_for_base_type_with_shared_container()
        => AssertQuery(
            async: true,
            ss => ss.Set<SharedContainerEntity2>().Where(e => e.Id == 4 && e.PartitionKey == "PK2"));

    [ConditionalFact]
    public virtual Task ReadItem_for_child_type_with_shared_container()
        => AssertQuery(
            async: true,
            ss => ss.Set<SharedContainerEntity2Child>().Where(e => e.Id == 5 && e.PartitionKey == "PK2"));

    [ConditionalFact]
    public virtual Task ReadItem_with_single_explicit_discriminator_mapping()
    {
        var partitionKey = "PK1";

        return AssertSingle(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>()
                .Where(
                    e => e.Id == 1
                        && EF.Property<string>(e, "Discriminator") == nameof(SinglePartitionKeyEntity)
                        && e.PartitionKey == partitionKey),
            ss => ss.Set<SinglePartitionKeyEntity>()
                .Where(e => e.Id == 1 && e.PartitionKey == partitionKey));
    }

    [ConditionalFact]
    public virtual Task ReadItem_with_single_explicit_incorrect_discriminator_mapping()
    {
        var partitionKey = "PK1";

        return AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>()
                .Where(
                    e => e.Id == 1
                        && EF.Property<string>(e, "Discriminator") == nameof(DerivedSinglePartitionKeyEntity)
                        && e.PartitionKey == partitionKey),
            ss => ss.Set<SinglePartitionKeyEntity>().Where(e => false),
            assertEmpty: true);
    }

    [ConditionalFact]
    public virtual Task ReadItem_with_single_explicit_parameterized_discriminator_mapping()
    {
        var partitionKey = "PK1";
        var discriminator = nameof(SinglePartitionKeyEntity);

        return AssertSingle(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>()
                .Where(e => e.Id == 1 && EF.Property<string>(e, "Discriminator") == discriminator && e.PartitionKey == partitionKey),
            ss => ss.Set<SinglePartitionKeyEntity>().Where(e => e.Id == 1 && e.PartitionKey == partitionKey));
    }

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
