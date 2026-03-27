// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class EntitySplittingQuerySqlServerTest : EntitySplittingQueryTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Can_query_entity_which_is_split_in_two(bool async)
    {
        await base.Can_query_entity_which_is_split_in_two(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[EntityThreeId], [e].[IntValue1], [e].[IntValue2], [s].[IntValue3], [s].[IntValue4], [e].[StringValue1], [e].[StringValue2], [s].[StringValue3], [s].[StringValue4]
FROM [EntityOne] AS [e]
INNER JOIN [SplitEntityOnePart] AS [s] ON [e].[Id] = [s].[Id]
""");
    }

    public override async Task Can_query_entity_which_is_split_selecting_only_main_properties(bool async)
    {
        await base.Can_query_entity_which_is_split_selecting_only_main_properties(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[IntValue1], [e].[StringValue1]
FROM [EntityOne] AS [e]
""");
    }

    public override async Task Can_query_entity_which_is_split_in_three(bool async)
    {
        await base.Can_query_entity_which_is_split_in_three(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[EntityThreeId], [e].[IntValue1], [e].[IntValue2], [s0].[IntValue3], [s].[IntValue4], [e].[StringValue1], [e].[StringValue2], [s0].[StringValue3], [s].[StringValue4]
FROM [EntityOne] AS [e]
INNER JOIN [SplitEntityOnePart3] AS [s] ON [e].[Id] = [s].[Id]
INNER JOIN [SplitEntityOnePart2] AS [s0] ON [e].[Id] = [s0].[Id]
""");
    }

    public override async Task Can_query_entity_which_is_split_selecting_only_part_2_properties(bool async)
    {
        await base.Can_query_entity_which_is_split_selecting_only_part_2_properties(async);

        AssertSql(
            """
SELECT [e].[Id], [s].[IntValue3], [s].[StringValue3]
FROM [EntityOne] AS [e]
INNER JOIN [SplitEntityOnePart2] AS [s] ON [e].[Id] = [s].[Id]
""");
    }

    public override async Task Can_query_entity_which_is_split_selecting_only_part_3_properties(bool async)
    {
        await base.Can_query_entity_which_is_split_selecting_only_part_3_properties(async);

        AssertSql(
            """
SELECT [e].[Id], [s].[IntValue4], [s].[StringValue4]
FROM [EntityOne] AS [e]
INNER JOIN [SplitEntityOnePart3] AS [s] ON [e].[Id] = [s].[Id]
""");
    }

    public override async Task Include_reference_to_split_entity(bool async)
    {
        await base.Include_reference_to_split_entity(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[EntityOneId], [e].[Name], [s1].[Id], [s1].[EntityThreeId], [s1].[IntValue1], [s1].[IntValue2], [s1].[IntValue3], [s1].[IntValue4], [s1].[StringValue1], [s1].[StringValue2], [s1].[StringValue3], [s1].[StringValue4]
FROM [EntityTwo] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[EntityThreeId], [e0].[IntValue1], [e0].[IntValue2], [s0].[IntValue3], [s].[IntValue4], [e0].[StringValue1], [e0].[StringValue2], [s0].[StringValue3], [s].[StringValue4]
    FROM [EntityOne] AS [e0]
    INNER JOIN [SplitEntityOnePart3] AS [s] ON [e0].[Id] = [s].[Id]
    INNER JOIN [SplitEntityOnePart2] AS [s0] ON [e0].[Id] = [s0].[Id]
) AS [s1] ON [e].[EntityOneId] = [s1].[Id]
""");
    }

    public override async Task Include_collection_to_split_entity(bool async)
    {
        await base.Include_collection_to_split_entity(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], [s1].[Id], [s1].[EntityThreeId], [s1].[IntValue1], [s1].[IntValue2], [s1].[IntValue3], [s1].[IntValue4], [s1].[StringValue1], [s1].[StringValue2], [s1].[StringValue3], [s1].[StringValue4]
FROM [EntityThree] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[EntityThreeId], [e0].[IntValue1], [e0].[IntValue2], [s0].[IntValue3], [s].[IntValue4], [e0].[StringValue1], [e0].[StringValue2], [s0].[StringValue3], [s].[StringValue4]
    FROM [EntityOne] AS [e0]
    INNER JOIN [SplitEntityOnePart3] AS [s] ON [e0].[Id] = [s].[Id]
    INNER JOIN [SplitEntityOnePart2] AS [s0] ON [e0].[Id] = [s0].[Id]
) AS [s1] ON [e].[Id] = [s1].[EntityThreeId]
ORDER BY [e].[Id]
""");
    }

    public override async Task Include_reference_to_split_entity_including_reference(bool async)
    {
        await base.Include_reference_to_split_entity_including_reference(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[EntityOneId], [e].[Name], [s1].[Id], [s1].[EntityThreeId], [s1].[IntValue1], [s1].[IntValue2], [s1].[IntValue3], [s1].[IntValue4], [s1].[StringValue1], [s1].[StringValue2], [s1].[StringValue3], [s1].[StringValue4], [e1].[Id], [e1].[Name]
FROM [EntityTwo] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[EntityThreeId], [e0].[IntValue1], [e0].[IntValue2], [s0].[IntValue3], [s].[IntValue4], [e0].[StringValue1], [e0].[StringValue2], [s0].[StringValue3], [s].[StringValue4]
    FROM [EntityOne] AS [e0]
    INNER JOIN [SplitEntityOnePart3] AS [s] ON [e0].[Id] = [s].[Id]
    INNER JOIN [SplitEntityOnePart2] AS [s0] ON [e0].[Id] = [s0].[Id]
) AS [s1] ON [e].[EntityOneId] = [s1].[Id]
LEFT JOIN [EntityThree] AS [e1] ON [s1].[EntityThreeId] = [e1].[Id]
""");
    }

    public override async Task Include_collection_to_split_entity_including_collection(bool async)
    {
        await base.Include_collection_to_split_entity_including_collection(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], [s1].[Id], [s1].[EntityThreeId], [s1].[IntValue1], [s1].[IntValue2], [s1].[IntValue3], [s1].[IntValue4], [s1].[StringValue1], [s1].[StringValue2], [s1].[StringValue3], [s1].[StringValue4], [s1].[Id0], [s1].[EntityOneId], [s1].[Name]
FROM [EntityThree] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[EntityThreeId], [e0].[IntValue1], [e0].[IntValue2], [s0].[IntValue3], [s].[IntValue4], [e0].[StringValue1], [e0].[StringValue2], [s0].[StringValue3], [s].[StringValue4], [e1].[Id] AS [Id0], [e1].[EntityOneId], [e1].[Name]
    FROM [EntityOne] AS [e0]
    INNER JOIN [SplitEntityOnePart3] AS [s] ON [e0].[Id] = [s].[Id]
    INNER JOIN [SplitEntityOnePart2] AS [s0] ON [e0].[Id] = [s0].[Id]
    LEFT JOIN [EntityTwo] AS [e1] ON [e0].[Id] = [e1].[EntityOneId]
) AS [s1] ON [e].[Id] = [s1].[EntityThreeId]
ORDER BY [e].[Id], [s1].[Id]
""");
    }

    public override async Task Include_reference_on_split_entity(bool async)
    {
        await base.Include_reference_on_split_entity(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[EntityThreeId], [e].[IntValue1], [e].[IntValue2], [s0].[IntValue3], [s].[IntValue4], [e].[StringValue1], [e].[StringValue2], [s0].[StringValue3], [s].[StringValue4], [e0].[Id], [e0].[Name]
FROM [EntityOne] AS [e]
INNER JOIN [SplitEntityOnePart3] AS [s] ON [e].[Id] = [s].[Id]
INNER JOIN [SplitEntityOnePart2] AS [s0] ON [e].[Id] = [s0].[Id]
LEFT JOIN [EntityThree] AS [e0] ON [e].[EntityThreeId] = [e0].[Id]
""");
    }

    public override async Task Include_collection_on_split_entity(bool async)
    {
        await base.Include_collection_on_split_entity(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[EntityThreeId], [e].[IntValue1], [e].[IntValue2], [s0].[IntValue3], [s].[IntValue4], [e].[StringValue1], [e].[StringValue2], [s0].[StringValue3], [s].[StringValue4], [e0].[Id], [e0].[EntityOneId], [e0].[Name]
FROM [EntityOne] AS [e]
INNER JOIN [SplitEntityOnePart3] AS [s] ON [e].[Id] = [s].[Id]
INNER JOIN [SplitEntityOnePart2] AS [s0] ON [e].[Id] = [s0].[Id]
LEFT JOIN [EntityTwo] AS [e0] ON [e].[Id] = [e0].[EntityOneId]
ORDER BY [e].[Id]
""");
    }

    public override async Task Custom_projection_trim_when_multiple_tables(bool async)
    {
        await base.Custom_projection_trim_when_multiple_tables(async);

        AssertSql(
            """
SELECT [e].[IntValue1], [s].[IntValue3], [e0].[Id], [e0].[Name]
FROM [EntityOne] AS [e]
INNER JOIN [SplitEntityOnePart2] AS [s] ON [e].[Id] = [s].[Id]
LEFT JOIN [EntityThree] AS [e0] ON [e].[EntityThreeId] = [e0].[Id]
""");
    }

    public override async Task Normal_entity_owning_a_split_reference_with_main_fragment_sharing(bool async)
    {
        await base.Normal_entity_owning_a_split_reference_with_main_fragment_sharing(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[EntityThreeId], [e].[IntValue1], [e].[IntValue2], [e].[IntValue3], [e].[IntValue4], [e].[StringValue1], [e].[StringValue2], [e].[StringValue3], [e].[StringValue4], [e].[OwnedReference_Id], [e].[OwnedReference_OwnedIntValue1], [e].[OwnedReference_OwnedIntValue2], [o0].[OwnedIntValue3], [o].[OwnedIntValue4], [e].[OwnedReference_OwnedStringValue1], [e].[OwnedReference_OwnedStringValue2], [o0].[OwnedStringValue3], [o].[OwnedStringValue4]
FROM [EntityOne] AS [e]
LEFT JOIN [OwnedReferenceExtras2] AS [o] ON [e].[Id] = [o].[EntityOneId]
LEFT JOIN [OwnedReferenceExtras1] AS [o0] ON [e].[Id] = [o0].[EntityOneId]
""");
    }

    public override async Task Normal_entity_owning_a_split_reference_with_main_fragment_sharing_custom_projection(bool async)
    {
        await base.Normal_entity_owning_a_split_reference_with_main_fragment_sharing_custom_projection(async);

        AssertSql(
            """
SELECT [e].[Id], CASE
    WHEN [e].[OwnedReference_Id] IS NOT NULL AND [e].[OwnedReference_OwnedIntValue1] IS NOT NULL AND [e].[OwnedReference_OwnedIntValue2] IS NOT NULL AND [o0].[OwnedIntValue3] IS NOT NULL AND [o].[OwnedIntValue4] IS NOT NULL THEN [o].[OwnedIntValue4]
END AS [OwnedIntValue4], CASE
    WHEN [e].[OwnedReference_Id] IS NOT NULL AND [e].[OwnedReference_OwnedIntValue1] IS NOT NULL AND [e].[OwnedReference_OwnedIntValue2] IS NOT NULL AND [o0].[OwnedIntValue3] IS NOT NULL AND [o].[OwnedIntValue4] IS NOT NULL THEN [o].[OwnedStringValue4]
END AS [OwnedStringValue4]
FROM [EntityOnes] AS [e]
LEFT JOIN [OwnedReferenceExtras2] AS [o] ON [e].[Id] = [o].[EntityOneId]
LEFT JOIN [OwnedReferenceExtras1] AS [o0] ON [e].[Id] = [o0].[EntityOneId]
""");
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override async Task Normal_entity_owning_a_split_reference_with_main_fragment_not_sharing(bool async)
    {
        await base.Normal_entity_owning_a_split_reference_with_main_fragment_not_sharing(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override async Task Normal_entity_owning_a_split_reference_with_main_fragment_not_sharing_custom_projection(bool async)
    {
        await base.Normal_entity_owning_a_split_reference_with_main_fragment_not_sharing_custom_projection(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override async Task Normal_entity_owning_a_split_collection(bool async)
    {
        await base.Normal_entity_owning_a_split_collection(async);

        AssertSql();
    }

    public override async Task Normal_entity_owning_a_split_reference_with_main_fragment_sharing_multiple_level(bool async)
    {
        await base.Normal_entity_owning_a_split_reference_with_main_fragment_sharing_multiple_level(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[EntityThreeId], [e].[IntValue1], [e].[IntValue2], [e].[IntValue3], [e].[IntValue4], [e].[StringValue1], [e].[StringValue2], [e].[StringValue3], [e].[StringValue4], [e].[OwnedReference_Id], [e].[OwnedReference_OwnedIntValue1], [e].[OwnedReference_OwnedIntValue2], [o0].[OwnedIntValue3], [o].[OwnedIntValue4], [e].[OwnedReference_OwnedStringValue1], [e].[OwnedReference_OwnedStringValue2], [o0].[OwnedStringValue3], [o].[OwnedStringValue4], [e].[OwnedReference_OwnedNestedReference_Id], [e].[OwnedReference_OwnedNestedReference_OwnedNestedIntValue1], [e].[OwnedReference_OwnedNestedReference_OwnedNestedIntValue2], [o2].[OwnedNestedIntValue3], [o1].[OwnedNestedIntValue4], [e].[OwnedReference_OwnedNestedReference_OwnedNestedStringValue1], [e].[OwnedReference_OwnedNestedReference_OwnedNestedStringValue2], [o2].[OwnedNestedStringValue3], [o1].[OwnedNestedStringValue4]
FROM [EntityOnes] AS [e]
LEFT JOIN [OwnedReferenceExtras2] AS [o] ON [e].[Id] = [o].[EntityOneId]
LEFT JOIN [OwnedReferenceExtras1] AS [o0] ON [e].[Id] = [o0].[EntityOneId]
LEFT JOIN [OwnedNestedReferenceExtras2] AS [o1] ON [e].[Id] = [o1].[OwnedReferenceEntityOneId]
LEFT JOIN [OwnedNestedReferenceExtras1] AS [o2] ON [e].[Id] = [o2].[OwnedReferenceEntityOneId]
""");
    }

    public override async Task Split_entity_owning_a_reference(bool async)
    {
        await base.Split_entity_owning_a_reference(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[EntityThreeId], [e].[IntValue1], [e].[IntValue2], [s0].[IntValue3], [s].[IntValue4], [e].[StringValue1], [e].[StringValue2], [s0].[StringValue3], [s].[StringValue4], [e].[OwnedReference_Id], [e].[OwnedReference_OwnedIntValue1], [e].[OwnedReference_OwnedIntValue2], [e].[OwnedReference_OwnedIntValue3], [e].[OwnedReference_OwnedIntValue4], [e].[OwnedReference_OwnedStringValue1], [e].[OwnedReference_OwnedStringValue2], [e].[OwnedReference_OwnedStringValue3], [e].[OwnedReference_OwnedStringValue4]
FROM [EntityOne] AS [e]
INNER JOIN [SplitEntityOnePart3] AS [s] ON [e].[Id] = [s].[Id]
INNER JOIN [SplitEntityOnePart2] AS [s0] ON [e].[Id] = [s0].[Id]
""");
    }

    public override async Task Split_entity_owning_a_collection(bool async)
    {
        await base.Split_entity_owning_a_collection(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[EntityThreeId], [e].[IntValue1], [e].[IntValue2], [s0].[IntValue3], [s].[IntValue4], [e].[StringValue1], [e].[StringValue2], [s0].[StringValue3], [s].[StringValue4], [o].[EntityOneId], [o].[Id], [o].[OwnedIntValue1], [o].[OwnedIntValue2], [o].[OwnedIntValue3], [o].[OwnedIntValue4], [o].[OwnedStringValue1], [o].[OwnedStringValue2], [o].[OwnedStringValue3], [o].[OwnedStringValue4]
FROM [EntityOne] AS [e]
INNER JOIN [SplitEntityOnePart3] AS [s] ON [e].[Id] = [s].[Id]
INNER JOIN [SplitEntityOnePart2] AS [s0] ON [e].[Id] = [s0].[Id]
LEFT JOIN [OwnedCollection] AS [o] ON [e].[Id] = [o].[EntityOneId]
ORDER BY [e].[Id], [o].[EntityOneId]
""");
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override async Task Split_entity_owning_a_split_reference_without_table_sharing(bool async)
    {
        await base.Split_entity_owning_a_split_reference_without_table_sharing(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override async Task Split_entity_owning_a_split_collection(bool async)
    {
        await base.Split_entity_owning_a_split_collection(async);

        AssertSql();
    }

    public override async Task Split_entity_owning_a_split_reference_with_table_sharing_1(bool async)
    {
        await base.Split_entity_owning_a_split_reference_with_table_sharing_1(async);

        AssertSql(
            """
SELECT [s].[Id], [s].[EntityThreeId], [s].[IntValue1], [s].[IntValue2], [s1].[IntValue3], [s0].[IntValue4], [s].[StringValue1], [s].[StringValue2], [s1].[StringValue3], [s0].[StringValue4], [s].[OwnedReference_Id], [s].[OwnedReference_OwnedIntValue1], [s].[OwnedReference_OwnedIntValue2], [s1].[OwnedReference_OwnedIntValue3], [s0].[OwnedReference_OwnedIntValue4], [s].[OwnedReference_OwnedStringValue1], [s].[OwnedReference_OwnedStringValue2], [s1].[OwnedReference_OwnedStringValue3], [s0].[OwnedReference_OwnedStringValue4]
FROM [SplitEntityOnePart1] AS [s]
INNER JOIN [SplitEntityOnePart3] AS [s0] ON [s].[Id] = [s0].[Id]
INNER JOIN [SplitEntityOnePart2] AS [s1] ON [s].[Id] = [s1].[Id]
""");
    }

    public override async Task Split_entity_owning_a_split_reference_with_table_sharing_4(bool async)
    {
        await base.Split_entity_owning_a_split_reference_with_table_sharing_4(async);

        AssertSql(
            """
SELECT [s].[Id], [s].[EntityThreeId], [s].[IntValue1], [s].[IntValue2], [s1].[IntValue3], [s0].[IntValue4], [s].[StringValue1], [s].[StringValue2], [s1].[StringValue3], [s0].[StringValue4], [s].[OwnedReference_Id], [s].[OwnedReference_OwnedIntValue1], [s].[OwnedReference_OwnedIntValue2], [s1].[OwnedReference_OwnedIntValue3], [o].[OwnedIntValue4], [s].[OwnedReference_OwnedStringValue1], [s].[OwnedReference_OwnedStringValue2], [s1].[OwnedReference_OwnedStringValue3], [o].[OwnedStringValue4]
FROM [SplitEntityOnePart1] AS [s]
INNER JOIN [SplitEntityOnePart3] AS [s0] ON [s].[Id] = [s0].[Id]
INNER JOIN [SplitEntityOnePart2] AS [s1] ON [s].[Id] = [s1].[Id]
LEFT JOIN [OwnedReferencePart3] AS [o] ON [s].[Id] = [o].[EntityOneId]
""");
    }

    public override async Task Split_entity_owning_a_split_reference_with_table_sharing_6(bool async)
    {
        await base.Split_entity_owning_a_split_reference_with_table_sharing_6(async);

        AssertSql(
            """
SELECT [s].[Id], [s].[EntityThreeId], [s].[IntValue1], [s].[IntValue2], [s1].[IntValue3], [s0].[IntValue4], [s].[StringValue1], [s].[StringValue2], [s1].[StringValue3], [s0].[StringValue4], [s1].[Id], [s1].[OwnedReference_Id], [s1].[OwnedReference_OwnedIntValue1], [s1].[OwnedReference_OwnedIntValue2], [o0].[OwnedIntValue3], [o].[OwnedIntValue4], [s1].[OwnedReference_OwnedStringValue1], [s1].[OwnedReference_OwnedStringValue2], [o0].[OwnedStringValue3], [o].[OwnedStringValue4]
FROM [SplitEntityOnePart1] AS [s]
INNER JOIN [SplitEntityOnePart3] AS [s0] ON [s].[Id] = [s0].[Id]
INNER JOIN [SplitEntityOnePart2] AS [s1] ON [s].[Id] = [s1].[Id]
LEFT JOIN [OwnedReferencePart3] AS [o] ON [s1].[Id] = [o].[EntityOneId]
LEFT JOIN [OwnedReferencePart2] AS [o0] ON [s1].[Id] = [o0].[EntityOneId]
""");
    }

    public override async Task Tph_entity_owning_a_split_reference_on_base_with_table_sharing(bool async)
    {
        await base.Tph_entity_owning_a_split_reference_on_base_with_table_sharing(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseValue], [b].[Discriminator], [b].[MiddleValue], [b].[SiblingValue], [b].[LeafValue], [b].[OwnedReference_Id], [b].[OwnedReference_OwnedIntValue1], [b].[OwnedReference_OwnedIntValue2], [o0].[OwnedIntValue3], [o].[OwnedIntValue4], [b].[OwnedReference_OwnedStringValue1], [b].[OwnedReference_OwnedStringValue2], [o0].[OwnedStringValue3], [o].[OwnedStringValue4]
FROM [BaseEntity] AS [b]
LEFT JOIN [OwnedReferencePart4] AS [o] ON [b].[Id] = [o].[BaseEntityId]
LEFT JOIN [OwnedReferencePart3] AS [o0] ON [b].[Id] = [o0].[BaseEntityId]
""");
    }

    public override async Task Tpt_entity_owning_a_split_reference_on_base_with_table_sharing(bool async)
    {
        await base.Tpt_entity_owning_a_split_reference_on_base_with_table_sharing(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseValue], [m].[MiddleValue], [s].[SiblingValue], [l].[LeafValue], CASE
    WHEN [l].[Id] IS NOT NULL THEN N'LeafEntity'
    WHEN [s].[Id] IS NOT NULL THEN N'SiblingEntity'
    WHEN [m].[Id] IS NOT NULL THEN N'MiddleEntity'
END AS [Discriminator], [b].[OwnedReference_Id], [b].[OwnedReference_OwnedIntValue1], [b].[OwnedReference_OwnedIntValue2], [o0].[OwnedIntValue3], [o].[OwnedIntValue4], [b].[OwnedReference_OwnedStringValue1], [b].[OwnedReference_OwnedStringValue2], [o0].[OwnedStringValue3], [o].[OwnedStringValue4]
FROM [BaseEntity] AS [b]
LEFT JOIN [MiddleEntity] AS [m] ON [b].[Id] = [m].[Id]
LEFT JOIN [SiblingEntity] AS [s] ON [b].[Id] = [s].[Id]
LEFT JOIN [LeafEntity] AS [l] ON [b].[Id] = [l].[Id]
LEFT JOIN [OwnedReferencePart4] AS [o] ON [b].[Id] = [o].[BaseEntityId]
LEFT JOIN [OwnedReferencePart3] AS [o0] ON [b].[Id] = [o0].[BaseEntityId]
""");
    }

    public override async Task Tph_entity_owning_a_split_reference_on_middle_with_table_sharing(bool async)
    {
        await base.Tph_entity_owning_a_split_reference_on_middle_with_table_sharing(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseValue], [b].[Discriminator], [b].[MiddleValue], [b].[SiblingValue], [b].[LeafValue], [b].[OwnedReference_Id], [b].[OwnedReference_OwnedIntValue1], [b].[OwnedReference_OwnedIntValue2], [o0].[OwnedIntValue3], [o].[OwnedIntValue4], [b].[OwnedReference_OwnedStringValue1], [b].[OwnedReference_OwnedStringValue2], [o0].[OwnedStringValue3], [o].[OwnedStringValue4]
FROM [BaseEntity] AS [b]
LEFT JOIN [OwnedReferencePart4] AS [o] ON [b].[Id] = [o].[MiddleEntityId]
LEFT JOIN [OwnedReferencePart3] AS [o0] ON [b].[Id] = [o0].[MiddleEntityId]
""");
    }

    public override async Task Tpt_entity_owning_a_split_reference_on_middle_with_table_sharing(bool async)
    {
        await base.Tpt_entity_owning_a_split_reference_on_middle_with_table_sharing(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseValue], [m].[MiddleValue], [s].[SiblingValue], [l].[LeafValue], CASE
    WHEN [l].[Id] IS NOT NULL THEN N'LeafEntity'
    WHEN [s].[Id] IS NOT NULL THEN N'SiblingEntity'
    WHEN [m].[Id] IS NOT NULL THEN N'MiddleEntity'
END AS [Discriminator], [m].[Id], [m].[OwnedReference_Id], [m].[OwnedReference_OwnedIntValue1], [m].[OwnedReference_OwnedIntValue2], [o0].[OwnedIntValue3], [o].[OwnedIntValue4], [m].[OwnedReference_OwnedStringValue1], [m].[OwnedReference_OwnedStringValue2], [o0].[OwnedStringValue3], [o].[OwnedStringValue4]
FROM [BaseEntity] AS [b]
LEFT JOIN [MiddleEntity] AS [m] ON [b].[Id] = [m].[Id]
LEFT JOIN [SiblingEntity] AS [s] ON [b].[Id] = [s].[Id]
LEFT JOIN [LeafEntity] AS [l] ON [b].[Id] = [l].[Id]
LEFT JOIN [OwnedReferencePart4] AS [o] ON [m].[Id] = [o].[MiddleEntityId]
LEFT JOIN [OwnedReferencePart3] AS [o0] ON [m].[Id] = [o0].[MiddleEntityId]
""");
    }

    public override async Task Tph_entity_owning_a_split_reference_on_leaf_with_table_sharing(bool async)
    {
        await base.Tph_entity_owning_a_split_reference_on_leaf_with_table_sharing(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseValue], [b].[Discriminator], [b].[MiddleValue], [b].[SiblingValue], [b].[LeafValue], [b].[OwnedReference_Id], [b].[OwnedReference_OwnedIntValue1], [b].[OwnedReference_OwnedIntValue2], [o0].[OwnedIntValue3], [o].[OwnedIntValue4], [b].[OwnedReference_OwnedStringValue1], [b].[OwnedReference_OwnedStringValue2], [o0].[OwnedStringValue3], [o].[OwnedStringValue4]
FROM [BaseEntity] AS [b]
LEFT JOIN [OwnedReferencePart4] AS [o] ON [b].[Id] = [o].[LeafEntityId]
LEFT JOIN [OwnedReferencePart3] AS [o0] ON [b].[Id] = [o0].[LeafEntityId]
""");
    }

    public override async Task Tpt_entity_owning_a_split_reference_on_leaf_with_table_sharing(bool async)
    {
        await base.Tpt_entity_owning_a_split_reference_on_leaf_with_table_sharing(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseValue], [m].[MiddleValue], [s].[SiblingValue], [l].[LeafValue], CASE
    WHEN [l].[Id] IS NOT NULL THEN N'LeafEntity'
    WHEN [s].[Id] IS NOT NULL THEN N'SiblingEntity'
    WHEN [m].[Id] IS NOT NULL THEN N'MiddleEntity'
END AS [Discriminator], [l].[Id], [l].[OwnedReference_Id], [l].[OwnedReference_OwnedIntValue1], [l].[OwnedReference_OwnedIntValue2], [o0].[OwnedIntValue3], [o].[OwnedIntValue4], [l].[OwnedReference_OwnedStringValue1], [l].[OwnedReference_OwnedStringValue2], [o0].[OwnedStringValue3], [o].[OwnedStringValue4]
FROM [BaseEntity] AS [b]
LEFT JOIN [MiddleEntity] AS [m] ON [b].[Id] = [m].[Id]
LEFT JOIN [SiblingEntity] AS [s] ON [b].[Id] = [s].[Id]
LEFT JOIN [LeafEntity] AS [l] ON [b].[Id] = [l].[Id]
LEFT JOIN [OwnedReferencePart4] AS [o] ON [l].[Id] = [o].[LeafEntityId]
LEFT JOIN [OwnedReferencePart3] AS [o0] ON [l].[Id] = [o0].[LeafEntityId]
""");
    }

    public override async Task Tpc_entity_owning_a_split_reference_on_leaf_with_table_sharing(bool async)
    {
        await base.Tpc_entity_owning_a_split_reference_on_leaf_with_table_sharing(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[BaseValue], [u].[MiddleValue], [u].[SiblingValue], [u].[LeafValue], [u].[Discriminator], [l0].[Id], [l0].[OwnedReference_Id], [l0].[OwnedReference_OwnedIntValue1], [l0].[OwnedReference_OwnedIntValue2], [o0].[OwnedIntValue3], [o].[OwnedIntValue4], [l0].[OwnedReference_OwnedStringValue1], [l0].[OwnedReference_OwnedStringValue2], [o0].[OwnedStringValue3], [o].[OwnedStringValue4]
FROM (
    SELECT [b].[Id], [b].[BaseValue], NULL AS [MiddleValue], NULL AS [SiblingValue], NULL AS [LeafValue], N'BaseEntity' AS [Discriminator]
    FROM [BaseEntity] AS [b]
    UNION ALL
    SELECT [m].[Id], [m].[BaseValue], [m].[MiddleValue], NULL AS [SiblingValue], NULL AS [LeafValue], N'MiddleEntity' AS [Discriminator]
    FROM [MiddleEntity] AS [m]
    UNION ALL
    SELECT [s].[Id], [s].[BaseValue], NULL AS [MiddleValue], [s].[SiblingValue], NULL AS [LeafValue], N'SiblingEntity' AS [Discriminator]
    FROM [SiblingEntity] AS [s]
    UNION ALL
    SELECT [l].[Id], [l].[BaseValue], [l].[MiddleValue], NULL AS [SiblingValue], [l].[LeafValue], N'LeafEntity' AS [Discriminator]
    FROM [LeafEntity] AS [l]
) AS [u]
LEFT JOIN [LeafEntity] AS [l0] ON [u].[Id] = [l0].[Id]
LEFT JOIN [OwnedReferencePart4] AS [o] ON [l0].[Id] = [o].[LeafEntityId]
LEFT JOIN [OwnedReferencePart3] AS [o0] ON [l0].[Id] = [o0].[LeafEntityId]
""");
    }

    public override async Task Tph_entity_owning_a_split_reference_on_base_with_table_sharing_querying_sibling(bool async)
    {
        await base.Tph_entity_owning_a_split_reference_on_base_with_table_sharing_querying_sibling(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseValue], [b].[Discriminator], [b].[SiblingValue], [b].[OwnedReference_Id], [b].[OwnedReference_OwnedIntValue1], [b].[OwnedReference_OwnedIntValue2], [o0].[OwnedIntValue3], [o].[OwnedIntValue4], [b].[OwnedReference_OwnedStringValue1], [b].[OwnedReference_OwnedStringValue2], [o0].[OwnedStringValue3], [o].[OwnedStringValue4]
FROM [BaseEntity] AS [b]
LEFT JOIN [OwnedReferencePart4] AS [o] ON [b].[Id] = [o].[BaseEntityId]
LEFT JOIN [OwnedReferencePart3] AS [o0] ON [b].[Id] = [o0].[BaseEntityId]
WHERE [b].[Discriminator] = N'SiblingEntity'
""");
    }

    public override async Task Tpt_entity_owning_a_split_reference_on_base_with_table_sharing_querying_sibling(bool async)
    {
        await base.Tpt_entity_owning_a_split_reference_on_base_with_table_sharing_querying_sibling(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseValue], [s].[SiblingValue], [b].[OwnedReference_Id], [b].[OwnedReference_OwnedIntValue1], [b].[OwnedReference_OwnedIntValue2], [o0].[OwnedIntValue3], [o].[OwnedIntValue4], [b].[OwnedReference_OwnedStringValue1], [b].[OwnedReference_OwnedStringValue2], [o0].[OwnedStringValue3], [o].[OwnedStringValue4]
FROM [BaseEntity] AS [b]
INNER JOIN [SiblingEntity] AS [s] ON [b].[Id] = [s].[Id]
LEFT JOIN [OwnedReferencePart4] AS [o] ON [b].[Id] = [o].[BaseEntityId]
LEFT JOIN [OwnedReferencePart3] AS [o0] ON [b].[Id] = [o0].[BaseEntityId]
""");
    }

    public override async Task Tph_entity_owning_a_split_reference_on_middle_with_table_sharing_querying_sibling(bool async)
    {
        await base.Tph_entity_owning_a_split_reference_on_middle_with_table_sharing_querying_sibling(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseValue], [b].[Discriminator], [b].[SiblingValue]
FROM [BaseEntity] AS [b]
WHERE [b].[Discriminator] = N'SiblingEntity'
""");
    }

    public override async Task Tpt_entity_owning_a_split_reference_on_middle_with_table_sharing_querying_sibling(bool async)
    {
        await base.Tpt_entity_owning_a_split_reference_on_middle_with_table_sharing_querying_sibling(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseValue], [s].[SiblingValue]
FROM [BaseEntity] AS [b]
INNER JOIN [SiblingEntity] AS [s] ON [b].[Id] = [s].[Id]
""");
    }

    public override async Task Tph_entity_owning_a_split_reference_on_leaf_with_table_sharing_querying_sibling(bool async)
    {
        await base.Tph_entity_owning_a_split_reference_on_leaf_with_table_sharing_querying_sibling(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseValue], [b].[Discriminator], [b].[SiblingValue]
FROM [BaseEntity] AS [b]
WHERE [b].[Discriminator] = N'SiblingEntity'
""");
    }

    public override async Task Tpt_entity_owning_a_split_reference_on_leaf_with_table_sharing_querying_sibling(bool async)
    {
        await base.Tpt_entity_owning_a_split_reference_on_leaf_with_table_sharing_querying_sibling(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseValue], [s].[SiblingValue]
FROM [BaseEntity] AS [b]
INNER JOIN [SiblingEntity] AS [s] ON [b].[Id] = [s].[Id]
""");
    }

    public override async Task Tpc_entity_owning_a_split_reference_on_leaf_with_table_sharing_querying_sibling(bool async)
    {
        await base.Tpc_entity_owning_a_split_reference_on_leaf_with_table_sharing_querying_sibling(async);

        AssertSql(
            """
SELECT [s].[Id], [s].[BaseValue], [s].[SiblingValue]
FROM [SiblingEntity] AS [s]
""");
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override async Task Tph_entity_owning_a_split_reference_on_base_without_table_sharing(bool async)
    {
        await base.Tph_entity_owning_a_split_reference_on_base_without_table_sharing(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override async Task Tpt_entity_owning_a_split_reference_on_base_without_table_sharing(bool async)
    {
        await base.Tpt_entity_owning_a_split_reference_on_base_without_table_sharing(async);

        AssertSql();
    }

    public override async Task Tpc_entity_owning_a_split_reference_on_base_without_table_sharing(bool async)
    {
        await base.Tpc_entity_owning_a_split_reference_on_base_without_table_sharing(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[BaseValue], [u].[MiddleValue], [u].[SiblingValue], [u].[LeafValue], [u].[Discriminator], [o].[BaseEntityId], [o].[Id], [o].[OwnedIntValue1], [o].[OwnedIntValue2], [o1].[OwnedIntValue3], [o0].[OwnedIntValue4], [o].[OwnedStringValue1], [o].[OwnedStringValue2], [o1].[OwnedStringValue3], [o0].[OwnedStringValue4]
FROM (
    SELECT [b].[Id], [b].[BaseValue], NULL AS [MiddleValue], NULL AS [SiblingValue], NULL AS [LeafValue], N'BaseEntity' AS [Discriminator]
    FROM [BaseEntity] AS [b]
    UNION ALL
    SELECT [m].[Id], [m].[BaseValue], [m].[MiddleValue], NULL AS [SiblingValue], NULL AS [LeafValue], N'MiddleEntity' AS [Discriminator]
    FROM [MiddleEntity] AS [m]
    UNION ALL
    SELECT [s].[Id], [s].[BaseValue], NULL AS [MiddleValue], [s].[SiblingValue], NULL AS [LeafValue], N'SiblingEntity' AS [Discriminator]
    FROM [SiblingEntity] AS [s]
    UNION ALL
    SELECT [l].[Id], [l].[BaseValue], [l].[MiddleValue], NULL AS [SiblingValue], [l].[LeafValue], N'LeafEntity' AS [Discriminator]
    FROM [LeafEntity] AS [l]
) AS [u]
LEFT JOIN [OwnedReferencePart1] AS [o] ON [u].[Id] = [o].[BaseEntityId]
LEFT JOIN [OwnedReferencePart4] AS [o0] ON [o].[BaseEntityId] = [o0].[BaseEntityId]
LEFT JOIN [OwnedReferencePart3] AS [o1] ON [o].[BaseEntityId] = [o1].[BaseEntityId]
""");
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override async Task Tph_entity_owning_a_split_reference_on_middle_without_table_sharing(bool async)
    {
        await base.Tph_entity_owning_a_split_reference_on_middle_without_table_sharing(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override async Task Tpt_entity_owning_a_split_reference_on_middle_without_table_sharing(bool async)
    {
        await base.Tpt_entity_owning_a_split_reference_on_middle_without_table_sharing(async);

        AssertSql();
    }

    public override async Task Tpc_entity_owning_a_split_reference_on_middle_without_table_sharing(bool async)
    {
        await base.Tpc_entity_owning_a_split_reference_on_middle_without_table_sharing(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[BaseValue], [u].[MiddleValue], [u].[SiblingValue], [u].[LeafValue], [u].[Discriminator], [o].[MiddleEntityId], [o].[Id], [o].[OwnedIntValue1], [o].[OwnedIntValue2], [o1].[OwnedIntValue3], [o0].[OwnedIntValue4], [o].[OwnedStringValue1], [o].[OwnedStringValue2], [o1].[OwnedStringValue3], [o0].[OwnedStringValue4]
FROM (
    SELECT [b].[Id], [b].[BaseValue], NULL AS [MiddleValue], NULL AS [SiblingValue], NULL AS [LeafValue], N'BaseEntity' AS [Discriminator]
    FROM [BaseEntity] AS [b]
    UNION ALL
    SELECT [m].[Id], [m].[BaseValue], [m].[MiddleValue], NULL AS [SiblingValue], NULL AS [LeafValue], N'MiddleEntity' AS [Discriminator]
    FROM [MiddleEntity] AS [m]
    UNION ALL
    SELECT [s].[Id], [s].[BaseValue], NULL AS [MiddleValue], [s].[SiblingValue], NULL AS [LeafValue], N'SiblingEntity' AS [Discriminator]
    FROM [SiblingEntity] AS [s]
    UNION ALL
    SELECT [l].[Id], [l].[BaseValue], [l].[MiddleValue], NULL AS [SiblingValue], [l].[LeafValue], N'LeafEntity' AS [Discriminator]
    FROM [LeafEntity] AS [l]
) AS [u]
LEFT JOIN [OwnedReferencePart1] AS [o] ON [u].[Id] = [o].[MiddleEntityId]
LEFT JOIN [OwnedReferencePart4] AS [o0] ON [o].[MiddleEntityId] = [o0].[MiddleEntityId]
LEFT JOIN [OwnedReferencePart3] AS [o1] ON [o].[MiddleEntityId] = [o1].[MiddleEntityId]
""");
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override async Task Tph_entity_owning_a_split_reference_on_leaf_without_table_sharing(bool async)
    {
        await base.Tph_entity_owning_a_split_reference_on_leaf_without_table_sharing(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override async Task Tpt_entity_owning_a_split_reference_on_leaf_without_table_sharing(bool async)
    {
        await base.Tpt_entity_owning_a_split_reference_on_leaf_without_table_sharing(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override async Task Tpc_entity_owning_a_split_reference_on_leaf_without_table_sharing(bool async)
    {
        await base.Tpc_entity_owning_a_split_reference_on_leaf_without_table_sharing(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override async Task Tph_entity_owning_a_split_collection_on_base(bool async)
    {
        await base.Tph_entity_owning_a_split_collection_on_base(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override async Task Tpt_entity_owning_a_split_collection_on_base(bool async)
    {
        await base.Tpt_entity_owning_a_split_collection_on_base(async);

        AssertSql();
    }

    public override async Task Tpc_entity_owning_a_split_collection_on_base(bool async)
    {
        await base.Tpc_entity_owning_a_split_collection_on_base(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[BaseValue], [u].[MiddleValue], [u].[SiblingValue], [u].[LeafValue], [u].[Discriminator], [s0].[BaseEntityId], [s0].[Id], [s0].[OwnedIntValue1], [s0].[OwnedIntValue2], [s0].[OwnedIntValue3], [s0].[OwnedIntValue4], [s0].[OwnedStringValue1], [s0].[OwnedStringValue2], [s0].[OwnedStringValue3], [s0].[OwnedStringValue4]
FROM (
    SELECT [b].[Id], [b].[BaseValue], NULL AS [MiddleValue], NULL AS [SiblingValue], NULL AS [LeafValue], N'BaseEntity' AS [Discriminator]
    FROM [BaseEntity] AS [b]
    UNION ALL
    SELECT [m].[Id], [m].[BaseValue], [m].[MiddleValue], NULL AS [SiblingValue], NULL AS [LeafValue], N'MiddleEntity' AS [Discriminator]
    FROM [MiddleEntity] AS [m]
    UNION ALL
    SELECT [s].[Id], [s].[BaseValue], NULL AS [MiddleValue], [s].[SiblingValue], NULL AS [LeafValue], N'SiblingEntity' AS [Discriminator]
    FROM [SiblingEntity] AS [s]
    UNION ALL
    SELECT [l].[Id], [l].[BaseValue], [l].[MiddleValue], NULL AS [SiblingValue], [l].[LeafValue], N'LeafEntity' AS [Discriminator]
    FROM [LeafEntity] AS [l]
) AS [u]
LEFT JOIN (
    SELECT [o].[BaseEntityId], [o].[Id], [o].[OwnedIntValue1], [o].[OwnedIntValue2], [o1].[OwnedIntValue3], [o0].[OwnedIntValue4], [o].[OwnedStringValue1], [o].[OwnedStringValue2], [o1].[OwnedStringValue3], [o0].[OwnedStringValue4]
    FROM [OwnedReferencePart1] AS [o]
    INNER JOIN [OwnedReferencePart4] AS [o0] ON [o].[BaseEntityId] = [o0].[BaseEntityId] AND [o].[Id] = [o0].[Id]
    INNER JOIN [OwnedReferencePart3] AS [o1] ON [o].[BaseEntityId] = [o1].[BaseEntityId] AND [o].[Id] = [o1].[Id]
) AS [s0] ON [u].[Id] = [s0].[BaseEntityId]
ORDER BY [u].[Id], [s0].[BaseEntityId]
""");
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override async Task Tph_entity_owning_a_split_collection_on_middle(bool async)
    {
        await base.Tph_entity_owning_a_split_collection_on_middle(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override async Task Tpt_entity_owning_a_split_collection_on_middle(bool async)
    {
        await base.Tpt_entity_owning_a_split_collection_on_middle(async);

        AssertSql();
    }

    public override async Task Tpc_entity_owning_a_split_collection_on_middle(bool async)
    {
        await base.Tpc_entity_owning_a_split_collection_on_middle(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[BaseValue], [u].[MiddleValue], [u].[SiblingValue], [u].[LeafValue], [u].[Discriminator], [s0].[MiddleEntityId], [s0].[Id], [s0].[OwnedIntValue1], [s0].[OwnedIntValue2], [s0].[OwnedIntValue3], [s0].[OwnedIntValue4], [s0].[OwnedStringValue1], [s0].[OwnedStringValue2], [s0].[OwnedStringValue3], [s0].[OwnedStringValue4]
FROM (
    SELECT [b].[Id], [b].[BaseValue], NULL AS [MiddleValue], NULL AS [SiblingValue], NULL AS [LeafValue], N'BaseEntity' AS [Discriminator]
    FROM [BaseEntity] AS [b]
    UNION ALL
    SELECT [m].[Id], [m].[BaseValue], [m].[MiddleValue], NULL AS [SiblingValue], NULL AS [LeafValue], N'MiddleEntity' AS [Discriminator]
    FROM [MiddleEntity] AS [m]
    UNION ALL
    SELECT [s].[Id], [s].[BaseValue], NULL AS [MiddleValue], [s].[SiblingValue], NULL AS [LeafValue], N'SiblingEntity' AS [Discriminator]
    FROM [SiblingEntity] AS [s]
    UNION ALL
    SELECT [l].[Id], [l].[BaseValue], [l].[MiddleValue], NULL AS [SiblingValue], [l].[LeafValue], N'LeafEntity' AS [Discriminator]
    FROM [LeafEntity] AS [l]
) AS [u]
LEFT JOIN (
    SELECT [o].[MiddleEntityId], [o].[Id], [o].[OwnedIntValue1], [o].[OwnedIntValue2], [o1].[OwnedIntValue3], [o0].[OwnedIntValue4], [o].[OwnedStringValue1], [o].[OwnedStringValue2], [o1].[OwnedStringValue3], [o0].[OwnedStringValue4]
    FROM [OwnedReferencePart1] AS [o]
    INNER JOIN [OwnedReferencePart4] AS [o0] ON [o].[MiddleEntityId] = [o0].[MiddleEntityId] AND [o].[Id] = [o0].[Id]
    INNER JOIN [OwnedReferencePart3] AS [o1] ON [o].[MiddleEntityId] = [o1].[MiddleEntityId] AND [o].[Id] = [o1].[Id]
) AS [s0] ON [u].[Id] = [s0].[MiddleEntityId]
ORDER BY [u].[Id], [s0].[MiddleEntityId]
""");
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override async Task Tph_entity_owning_a_split_collection_on_leaf(bool async)
    {
        await base.Tph_entity_owning_a_split_collection_on_leaf(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override async Task Tpt_entity_owning_a_split_collection_on_leaf(bool async)
    {
        await base.Tpt_entity_owning_a_split_collection_on_leaf(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override async Task Tpc_entity_owning_a_split_collection_on_leaf(bool async)
    {
        await base.Tpc_entity_owning_a_split_collection_on_leaf(async);

        AssertSql();
    }
}
