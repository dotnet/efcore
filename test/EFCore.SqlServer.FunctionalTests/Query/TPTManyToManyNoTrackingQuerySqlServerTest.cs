// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class TPTManyToManyNoTrackingQuerySqlServerTest : TPTManyToManyNoTrackingQueryRelationalTestBase<
    TPTManyToManyQuerySqlServerFixture>
{
    public TPTManyToManyNoTrackingQuerySqlServerTest(TPTManyToManyQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Skip_navigation_all(bool async)
    {
        await base.Skip_navigation_all(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name]
FROM [EntityOnes] AS [e]
WHERE NOT EXISTS (
    SELECT 1
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    WHERE [e].[Id] = [j].[OneId] AND ([e0].[Name] NOT LIKE N'%B%' OR [e0].[Name] IS NULL))
""");
    }

    public override async Task Skip_navigation_any_without_predicate(bool async)
    {
        await base.Skip_navigation_any_without_predicate(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name]
FROM [EntityOnes] AS [e]
WHERE EXISTS (
    SELECT 1
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityThrees] AS [e0] ON [j].[ThreeId] = [e0].[Id]
    WHERE [e].[Id] = [j].[OneId] AND [e0].[Name] LIKE N'%B%')
""");
    }

    public override async Task Skip_navigation_any_with_predicate(bool async)
    {
        await base.Skip_navigation_any_with_predicate(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name]
FROM [EntityOnes] AS [e]
WHERE EXISTS (
    SELECT 1
    FROM [EntityOneEntityTwo] AS [e0]
    INNER JOIN [EntityTwos] AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
    WHERE [e].[Id] = [e0].[OneSkipSharedId] AND [e1].[Name] LIKE N'%B%')
""");
    }

    public override async Task Skip_navigation_contains(bool async)
    {
        await base.Skip_navigation_contains(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name]
FROM [EntityOnes] AS [e]
WHERE EXISTS (
    SELECT 1
    FROM [JoinOneToThreePayloadFullShared] AS [j]
    INNER JOIN [EntityThrees] AS [e0] ON [j].[ThreeId] = [e0].[Id]
    WHERE [e].[Id] = [j].[OneId] AND [e0].[Id] = 1)
""");
    }

    public override async Task Skip_navigation_count_without_predicate(bool async)
    {
        await base.Skip_navigation_count_without_predicate(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name]
FROM [EntityOnes] AS [e]
WHERE (
    SELECT COUNT(*)
    FROM [JoinOneSelfPayload] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[LeftId] = [e0].[Id]
    WHERE [e].[Id] = [j].[RightId]) > 0
""");
    }

    public override async Task Skip_navigation_count_with_predicate(bool async)
    {
        await base.Skip_navigation_count_with_predicate(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name]
FROM [EntityOnes] AS [e]
ORDER BY (
    SELECT COUNT(*)
    FROM [JoinOneToBranch] AS [j]
    INNER JOIN (
        SELECT [r].[Id], [r].[Name]
        FROM [Roots] AS [r]
        INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
    ) AS [s] ON [j].[EntityBranchId] = [s].[Id]
    WHERE [e].[Id] = [j].[EntityOneId] AND [s].[Name] LIKE N'L%'), [e].[Id]
""");
    }

    public override async Task Skip_navigation_long_count_without_predicate(bool async)
    {
        await base.Skip_navigation_long_count_without_predicate(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId]
FROM [EntityTwos] AS [e]
WHERE (
    SELECT COUNT_BIG(*)
    FROM [JoinTwoToThree] AS [j]
    INNER JOIN [EntityThrees] AS [e0] ON [j].[ThreeId] = [e0].[Id]
    WHERE [e].[Id] = [j].[TwoId]) > CAST(0 AS bigint)
""");
    }

    public override async Task Skip_navigation_long_count_with_predicate(bool async)
    {
        await base.Skip_navigation_long_count_with_predicate(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId]
FROM [EntityTwos] AS [e]
ORDER BY (
    SELECT COUNT_BIG(*)
    FROM [EntityTwoEntityTwo] AS [e0]
    INNER JOIN [EntityTwos] AS [e1] ON [e0].[SelfSkipSharedLeftId] = [e1].[Id]
    WHERE [e].[Id] = [e0].[SelfSkipSharedRightId] AND [e1].[Name] LIKE N'L%') DESC, [e].[Id]
""");
    }

    public override async Task Skip_navigation_select_many_average(bool async)
    {
        await base.Skip_navigation_select_many_average(async);

        AssertSql(
            """
SELECT AVG(CAST([s].[Key1] AS float))
FROM [EntityTwos] AS [e]
INNER JOIN (
    SELECT [e1].[Key1], [e0].[TwoSkipSharedId]
    FROM [EntityCompositeKeyEntityTwo] AS [e0]
    INNER JOIN [EntityCompositeKeys] AS [e1] ON [e0].[CompositeKeySkipSharedKey1] = [e1].[Key1] AND [e0].[CompositeKeySkipSharedKey2] = [e1].[Key2] AND [e0].[CompositeKeySkipSharedKey3] = [e1].[Key3]
) AS [s] ON [e].[Id] = [s].[TwoSkipSharedId]
""");
    }

    public override async Task Skip_navigation_select_many_max(bool async)
    {
        await base.Skip_navigation_select_many_max(async);

        AssertSql(
            """
SELECT MAX([s].[Key1])
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [e0].[Key1], [j].[ThreeId]
    FROM [JoinThreeToCompositeKeyFull] AS [j]
    INNER JOIN [EntityCompositeKeys] AS [e0] ON [j].[CompositeId1] = [e0].[Key1] AND [j].[CompositeId2] = [e0].[Key2] AND [j].[CompositeId3] = [e0].[Key3]
) AS [s] ON [e].[Id] = [s].[ThreeId]
""");
    }

    public override async Task Skip_navigation_select_many_min(bool async)
    {
        await base.Skip_navigation_select_many_min(async);

        AssertSql(
            """
SELECT MIN([s0].[Id])
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [s].[Id], [e0].[ThreeSkipSharedId]
    FROM [EntityRootEntityThree] AS [e0]
    INNER JOIN (
        SELECT [r].[Id]
        FROM [Roots] AS [r]
    ) AS [s] ON [e0].[RootSkipSharedId] = [s].[Id]
) AS [s0] ON [e].[Id] = [s0].[ThreeSkipSharedId]
""");
    }

    public override async Task Skip_navigation_select_many_sum(bool async)
    {
        await base.Skip_navigation_select_many_sum(async);

        AssertSql(
            """
SELECT COALESCE(SUM([s].[Key1]), 0)
FROM [Roots] AS [r]
INNER JOIN (
    SELECT [e0].[Key1], [e].[RootSkipSharedId]
    FROM [EntityCompositeKeyEntityRoot] AS [e]
    INNER JOIN [EntityCompositeKeys] AS [e0] ON [e].[CompositeKeySkipSharedKey1] = [e0].[Key1] AND [e].[CompositeKeySkipSharedKey2] = [e0].[Key2] AND [e].[CompositeKeySkipSharedKey3] = [e0].[Key3]
) AS [s] ON [r].[Id] = [s].[RootSkipSharedId]
""");
    }

    public override async Task Skip_navigation_select_subquery_average(bool async)
    {
        await base.Skip_navigation_select_subquery_average(async);

        AssertSql(
            """
SELECT (
    SELECT AVG(CAST([e].[Key1] AS float))
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN [EntityCompositeKeys] AS [e] ON [j].[CompositeId1] = [e].[Key1] AND [j].[CompositeId2] = [e].[Key2] AND [j].[CompositeId3] = [e].[Key3]
    WHERE [r].[Id] = [j].[LeafId])
FROM [Roots] AS [r]
INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
INNER JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
""");
    }

    public override async Task Skip_navigation_select_subquery_max(bool async)
    {
        await base.Skip_navigation_select_subquery_max(async);

        AssertSql(
            """
SELECT (
    SELECT MAX([e0].[Id])
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e].[Id] = [j].[TwoId])
FROM [EntityTwos] AS [e]
""");
    }

    public override async Task Skip_navigation_select_subquery_min(bool async)
    {
        await base.Skip_navigation_select_subquery_min(async);

        AssertSql(
            """
SELECT (
    SELECT MIN([e0].[Id])
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e].[Id] = [j].[ThreeId])
FROM [EntityThrees] AS [e]
""");
    }

    public override async Task Skip_navigation_select_subquery_sum(bool async)
    {
        await base.Skip_navigation_select_subquery_sum(async);

        AssertSql(
            """
SELECT (
    SELECT COALESCE(SUM([e1].[Id]), 0)
    FROM [EntityOneEntityTwo] AS [e0]
    INNER JOIN [EntityOnes] AS [e1] ON [e0].[OneSkipSharedId] = [e1].[Id]
    WHERE [e].[Id] = [e0].[TwoSkipSharedId])
FROM [EntityTwos] AS [e]
""");
    }

    public override async Task Skip_navigation_order_by_first_or_default(bool async)
    {
        await base.Skip_navigation_order_by_first_or_default(async);

        AssertSql(
            """
SELECT [s0].[Id], [s0].[Name]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [s].[Id], [s].[Name], [s].[ThreeId]
    FROM (
        SELECT [e0].[Id], [e0].[Name], [j].[ThreeId], ROW_NUMBER() OVER(PARTITION BY [j].[ThreeId] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinOneToThreePayloadFullShared] AS [j]
        INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    ) AS [s]
    WHERE [s].[row] <= 1
) AS [s0] ON [e].[Id] = [s0].[ThreeId]
""");
    }

    public override async Task Skip_navigation_order_by_single_or_default(bool async)
    {
        await base.Skip_navigation_order_by_single_or_default(async);

        AssertSql(
            """
SELECT [s0].[Id], [s0].[Name]
FROM [EntityOnes] AS [e]
OUTER APPLY (
    SELECT TOP(1) [s].[Id], [s].[Name]
    FROM (
        SELECT TOP(1) [e0].[Id], [e0].[Name]
        FROM [JoinOneSelfPayload] AS [j]
        INNER JOIN [EntityOnes] AS [e0] ON [j].[RightId] = [e0].[Id]
        WHERE [e].[Id] = [j].[LeftId]
        ORDER BY [e0].[Id]
    ) AS [s]
    ORDER BY [s].[Id]
) AS [s0]
""");
    }

    public override async Task Skip_navigation_order_by_last_or_default(bool async)
    {
        await base.Skip_navigation_order_by_last_or_default(async);

        AssertSql(
            """
SELECT [s0].[Id], [s0].[Name]
FROM [Roots] AS [r]
INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
LEFT JOIN (
    SELECT [s].[Id], [s].[Name], [s].[EntityBranchId]
    FROM (
        SELECT [e].[Id], [e].[Name], [j].[EntityBranchId], ROW_NUMBER() OVER(PARTITION BY [j].[EntityBranchId] ORDER BY [e].[Id] DESC) AS [row]
        FROM [JoinOneToBranch] AS [j]
        INNER JOIN [EntityOnes] AS [e] ON [j].[EntityOneId] = [e].[Id]
    ) AS [s]
    WHERE [s].[row] <= 1
) AS [s0] ON [r].[Id] = [s0].[EntityBranchId]
""");
    }

    public override async Task Skip_navigation_order_by_reverse_first_or_default(bool async)
    {
        await base.Skip_navigation_order_by_reverse_first_or_default(async);

        AssertSql(
            """
SELECT [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[ReferenceInverseId]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId], [s].[ThreeId]
    FROM (
        SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], [j].[ThreeId], ROW_NUMBER() OVER(PARTITION BY [j].[ThreeId] ORDER BY [e0].[Id] DESC) AS [row]
        FROM [JoinTwoToThree] AS [j]
        INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    ) AS [s]
    WHERE [s].[row] <= 1
) AS [s0] ON [e].[Id] = [s0].[ThreeId]
""");
    }

    public override async Task Skip_navigation_cast(bool async)
    {
        await base.Skip_navigation_cast(async);

        AssertSql(
            """
SELECT [e].[Key1], [e].[Key2], [e].[Key3], [s0].[Id], [s0].[Name], [s0].[Number], [s0].[IsGreen], [s0].[LeafId], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [s].[Id], [s].[Name], [s].[Number], [s].[IsGreen], [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN (
        SELECT [r].[Id], [r].[Name], [b].[Number], [l].[IsGreen]
        FROM [Roots] AS [r]
        INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
        INNER JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
    ) AS [s] ON [j].[LeafId] = [s].[Id]
) AS [s0] ON [e].[Key1] = [s0].[CompositeId1] AND [e].[Key2] = [s0].[CompositeId2] AND [e].[Key3] = [s0].[CompositeId3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [s0].[LeafId], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3]
""");
    }

    public override async Task Skip_navigation_of_type(bool async)
    {
        await base.Skip_navigation_of_type(async);

        AssertSql(
            """
SELECT [e].[Key1], [e].[Key2], [e].[Key3], [s0].[Id], [s0].[Name], [s0].[Number], [s0].[IsGreen], [s0].[Discriminator], [s0].[RootSkipSharedId], [s0].[CompositeKeySkipSharedKey1], [s0].[CompositeKeySkipSharedKey2], [s0].[CompositeKeySkipSharedKey3]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [s].[Id], [s].[Name], [s].[Number], [s].[IsGreen], [s].[Discriminator], [e0].[RootSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3]
    FROM [EntityCompositeKeyEntityRoot] AS [e0]
    INNER JOIN (
        SELECT [r].[Id], [r].[Name], [b].[Number], [l].[IsGreen], CASE
            WHEN [l0].[Id] IS NOT NULL THEN N'EntityLeaf2'
            WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
            WHEN [b].[Id] IS NOT NULL THEN N'EntityBranch'
        END AS [Discriminator]
        FROM [Roots] AS [r]
        LEFT JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
        LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
        LEFT JOIN [Leaf2s] AS [l0] ON [r].[Id] = [l0].[Id]
    ) AS [s] ON [e0].[RootSkipSharedId] = [s].[Id]
    WHERE [s].[Discriminator] = N'EntityLeaf'
) AS [s0] ON [e].[Key1] = [s0].[CompositeKeySkipSharedKey1] AND [e].[Key2] = [s0].[CompositeKeySkipSharedKey2] AND [e].[Key3] = [s0].[CompositeKeySkipSharedKey3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [s0].[RootSkipSharedId], [s0].[CompositeKeySkipSharedKey1], [s0].[CompositeKeySkipSharedKey2], [s0].[CompositeKeySkipSharedKey3]
""");
    }

    public override async Task Join_with_skip_navigation(bool async)
    {
        await base.Join_with_skip_navigation(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId], [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId]
FROM [EntityTwos] AS [e]
INNER JOIN [EntityTwos] AS [e0] ON [e].[Id] = (
    SELECT TOP(1) [e2].[Id]
    FROM [EntityTwoEntityTwo] AS [e1]
    INNER JOIN [EntityTwos] AS [e2] ON [e1].[SelfSkipSharedRightId] = [e2].[Id]
    WHERE [e0].[Id] = [e1].[SelfSkipSharedLeftId]
    ORDER BY [e2].[Id])
""");
    }

    public override async Task Left_join_with_skip_navigation(bool async)
    {
        await base.Left_join_with_skip_navigation(async);

        AssertSql(
            """
SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [e0].[Key1], [e0].[Key2], [e0].[Key3], [e0].[Name]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN [EntityCompositeKeys] AS [e0] ON (
    SELECT TOP(1) [e2].[Id]
    FROM [EntityCompositeKeyEntityTwo] AS [e1]
    INNER JOIN [EntityTwos] AS [e2] ON [e1].[TwoSkipSharedId] = [e2].[Id]
    WHERE [e].[Key1] = [e1].[CompositeKeySkipSharedKey1] AND [e].[Key2] = [e1].[CompositeKeySkipSharedKey2] AND [e].[Key3] = [e1].[CompositeKeySkipSharedKey3]
    ORDER BY [e2].[Id]) = (
    SELECT TOP(1) [e3].[Id]
    FROM [JoinThreeToCompositeKeyFull] AS [j]
    INNER JOIN [EntityThrees] AS [e3] ON [j].[ThreeId] = [e3].[Id]
    WHERE [e0].[Key1] = [j].[CompositeId1] AND [e0].[Key2] = [j].[CompositeId2] AND [e0].[Key3] = [j].[CompositeId3]
    ORDER BY [e3].[Id])
ORDER BY [e].[Key1], [e0].[Key1], [e].[Key2], [e0].[Key2]
""");
    }

    public override async Task Select_many_over_skip_navigation(bool async)
    {
        await base.Select_many_over_skip_navigation(async);

        AssertSql(
            """
SELECT [s].[Id], [s].[CollectionInverseId], [s].[Name], [s].[ReferenceInverseId]
FROM [Roots] AS [r]
INNER JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId], [e].[RootSkipSharedId]
    FROM [EntityRootEntityThree] AS [e]
    INNER JOIN [EntityThrees] AS [e0] ON [e].[ThreeSkipSharedId] = [e0].[Id]
) AS [s] ON [r].[Id] = [s].[RootSkipSharedId]
""");
    }

    public override async Task Select_many_over_skip_navigation_where(bool async)
    {
        await base.Select_many_over_skip_navigation_where(async);

        AssertSql(
            """
SELECT [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId]
FROM [EntityOnes] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], [j].[OneId]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
) AS [s] ON [e].[Id] = [s].[OneId]
""");
    }

    public override async Task Select_many_over_skip_navigation_order_by_skip(bool async)
    {
        await base.Select_many_over_skip_navigation_order_by_skip(async);

        AssertSql(
            """
SELECT [s0].[Id], [s0].[CollectionInverseId], [s0].[Name], [s0].[ReferenceInverseId]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [s].[Id], [s].[CollectionInverseId], [s].[Name], [s].[ReferenceInverseId], [s].[OneId]
    FROM (
        SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId], [j].[OneId], ROW_NUMBER() OVER(PARTITION BY [j].[OneId] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinOneToThreePayloadFull] AS [j]
        INNER JOIN [EntityThrees] AS [e0] ON [j].[ThreeId] = [e0].[Id]
    ) AS [s]
    WHERE 2 < [s].[row]
) AS [s0] ON [e].[Id] = [s0].[OneId]
""");
    }

    public override async Task Select_many_over_skip_navigation_order_by_take(bool async)
    {
        await base.Select_many_over_skip_navigation_order_by_take(async);

        AssertSql(
            """
SELECT [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[ReferenceInverseId]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId], [s].[OneSkipSharedId]
    FROM (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], [e0].[OneSkipSharedId], ROW_NUMBER() OVER(PARTITION BY [e0].[OneSkipSharedId] ORDER BY [e1].[Id]) AS [row]
        FROM [EntityOneEntityTwo] AS [e0]
        INNER JOIN [EntityTwos] AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
    ) AS [s]
    WHERE [s].[row] <= 2
) AS [s0] ON [e].[Id] = [s0].[OneSkipSharedId]
""");
    }

    public override async Task Select_many_over_skip_navigation_order_by_skip_take(bool async)
    {
        await base.Select_many_over_skip_navigation_order_by_skip_take(async);

        AssertSql(
            """
SELECT [s0].[Id], [s0].[CollectionInverseId], [s0].[Name], [s0].[ReferenceInverseId]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [s].[Id], [s].[CollectionInverseId], [s].[Name], [s].[ReferenceInverseId], [s].[OneId]
    FROM (
        SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId], [j].[OneId], ROW_NUMBER() OVER(PARTITION BY [j].[OneId] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinOneToThreePayloadFullShared] AS [j]
        INNER JOIN [EntityThrees] AS [e0] ON [j].[ThreeId] = [e0].[Id]
    ) AS [s]
    WHERE 2 < [s].[row] AND [s].[row] <= 5
) AS [s0] ON [e].[Id] = [s0].[OneId]
""");
    }

    public override async Task Select_many_over_skip_navigation_of_type(bool async)
    {
        await base.Select_many_over_skip_navigation_of_type(async);

        AssertSql(
            """
SELECT [s0].[Id], [s0].[Name], [s0].[Number], [s0].[IsGreen], [s0].[Discriminator]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [s].[Id], [s].[Name], [s].[Number], [s].[IsGreen], [s].[Discriminator], [e0].[ThreeSkipSharedId]
    FROM [EntityRootEntityThree] AS [e0]
    INNER JOIN (
        SELECT [r].[Id], [r].[Name], [b].[Number], [l].[IsGreen], CASE
            WHEN [l0].[Id] IS NOT NULL THEN N'EntityLeaf2'
            WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
            WHEN [b].[Id] IS NOT NULL THEN N'EntityBranch'
        END AS [Discriminator]
        FROM [Roots] AS [r]
        LEFT JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
        LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
        LEFT JOIN [Leaf2s] AS [l0] ON [r].[Id] = [l0].[Id]
    ) AS [s] ON [e0].[RootSkipSharedId] = [s].[Id]
    WHERE [s].[Discriminator] IN (N'EntityBranch', N'EntityLeaf')
) AS [s0] ON [e].[Id] = [s0].[ThreeSkipSharedId]
""");
    }

    public override async Task Select_many_over_skip_navigation_cast(bool async)
    {
        await base.Select_many_over_skip_navigation_cast(async);

        AssertSql(
            """
SELECT [s0].[Id], [s0].[Name], [s0].[Number], [s0].[IsGreen], [s0].[Discriminator]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [s].[Id], [s].[Name], [s].[Number], [s].[IsGreen], [s].[Discriminator], [j].[EntityOneId]
    FROM [JoinOneToBranch] AS [j]
    INNER JOIN (
        SELECT [r].[Id], [r].[Name], [b].[Number], [l].[IsGreen], CASE
            WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
        END AS [Discriminator]
        FROM [Roots] AS [r]
        INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
        LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
    ) AS [s] ON [j].[EntityBranchId] = [s].[Id]
) AS [s0] ON [e].[Id] = [s0].[EntityOneId]
""");
    }

    public override async Task Select_skip_navigation(bool async)
    {
        await base.Select_skip_navigation(async);

        AssertSql(
            """
SELECT [e].[Id], [s].[Id], [s].[Name], [s].[LeftId], [s].[RightId]
FROM [EntityOnes] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[Name], [j].[LeftId], [j].[RightId]
    FROM [JoinOneSelfPayload] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[LeftId] = [e0].[Id]
) AS [s] ON [e].[Id] = [s].[RightId]
ORDER BY [e].[Id], [s].[LeftId], [s].[RightId]
""");
    }

    public override async Task Select_skip_navigation_multiple(bool async)
    {
        await base.Select_skip_navigation_multiple(async);

        AssertSql(
            """
SELECT [e].[Id], [s].[Id], [s].[CollectionInverseId], [s].[Name], [s].[ReferenceInverseId], [s].[ThreeId], [s].[TwoId], [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[ReferenceInverseId], [s0].[SelfSkipSharedLeftId], [s0].[SelfSkipSharedRightId], [s1].[Key1], [s1].[Key2], [s1].[Key3], [s1].[Name], [s1].[TwoSkipSharedId], [s1].[CompositeKeySkipSharedKey1], [s1].[CompositeKeySkipSharedKey2], [s1].[CompositeKeySkipSharedKey3]
FROM [EntityTwos] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId], [j].[ThreeId], [j].[TwoId]
    FROM [JoinTwoToThree] AS [j]
    INNER JOIN [EntityThrees] AS [e0] ON [j].[ThreeId] = [e0].[Id]
) AS [s] ON [e].[Id] = [s].[TwoId]
LEFT JOIN (
    SELECT [e2].[Id], [e2].[CollectionInverseId], [e2].[ExtraId], [e2].[Name], [e2].[ReferenceInverseId], [e1].[SelfSkipSharedLeftId], [e1].[SelfSkipSharedRightId]
    FROM [EntityTwoEntityTwo] AS [e1]
    INNER JOIN [EntityTwos] AS [e2] ON [e1].[SelfSkipSharedLeftId] = [e2].[Id]
) AS [s0] ON [e].[Id] = [s0].[SelfSkipSharedRightId]
LEFT JOIN (
    SELECT [e4].[Key1], [e4].[Key2], [e4].[Key3], [e4].[Name], [e3].[TwoSkipSharedId], [e3].[CompositeKeySkipSharedKey1], [e3].[CompositeKeySkipSharedKey2], [e3].[CompositeKeySkipSharedKey3]
    FROM [EntityCompositeKeyEntityTwo] AS [e3]
    INNER JOIN [EntityCompositeKeys] AS [e4] ON [e3].[CompositeKeySkipSharedKey1] = [e4].[Key1] AND [e3].[CompositeKeySkipSharedKey2] = [e4].[Key2] AND [e3].[CompositeKeySkipSharedKey3] = [e4].[Key3]
) AS [s1] ON [e].[Id] = [s1].[TwoSkipSharedId]
ORDER BY [e].[Id], [s].[ThreeId], [s].[TwoId], [s].[Id], [s0].[SelfSkipSharedLeftId], [s0].[SelfSkipSharedRightId], [s0].[Id], [s1].[TwoSkipSharedId], [s1].[CompositeKeySkipSharedKey1], [s1].[CompositeKeySkipSharedKey2], [s1].[CompositeKeySkipSharedKey3], [s1].[Key1], [s1].[Key2]
""");
    }

    public override async Task Select_skip_navigation_first_or_default(bool async)
    {
        await base.Select_skip_navigation_first_or_default(async);

        AssertSql(
            """
SELECT [s0].[Key1], [s0].[Key2], [s0].[Key3], [s0].[Name]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [s].[Key1], [s].[Key2], [s].[Key3], [s].[Name], [s].[ThreeId]
    FROM (
        SELECT [e0].[Key1], [e0].[Key2], [e0].[Key3], [e0].[Name], [j].[ThreeId], ROW_NUMBER() OVER(PARTITION BY [j].[ThreeId] ORDER BY [e0].[Key1], [e0].[Key2]) AS [row]
        FROM [JoinThreeToCompositeKeyFull] AS [j]
        INNER JOIN [EntityCompositeKeys] AS [e0] ON [j].[CompositeId1] = [e0].[Key1] AND [j].[CompositeId2] = [e0].[Key2] AND [j].[CompositeId3] = [e0].[Key3]
    ) AS [s]
    WHERE [s].[row] <= 1
) AS [s0] ON [e].[Id] = [s0].[ThreeId]
ORDER BY [e].[Id]
""");
    }

    public override async Task Include_skip_navigation(bool async)
    {
        await base.Include_skip_navigation(async);

        AssertSql(
            """
SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [s0].[Id], [s0].[Name], [s0].[Number], [s0].[Slumber], [s0].[IsGreen], [s0].[IsBrown], [s0].[Discriminator], [s0].[RootSkipSharedId], [s0].[CompositeKeySkipSharedKey1], [s0].[CompositeKeySkipSharedKey2], [s0].[CompositeKeySkipSharedKey3]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [s].[Id], [s].[Name], [s].[Number], [s].[Slumber], [s].[IsGreen], [s].[IsBrown], [s].[Discriminator], [e0].[RootSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3]
    FROM [EntityCompositeKeyEntityRoot] AS [e0]
    INNER JOIN (
        SELECT [r].[Id], [r].[Name], [b].[Number], [b0].[Slumber], [l].[IsGreen], [l0].[IsBrown], CASE
            WHEN [l0].[Id] IS NOT NULL THEN N'EntityLeaf2'
            WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
            WHEN [b].[Id] IS NOT NULL THEN N'EntityBranch'
        END AS [Discriminator]
        FROM [Roots] AS [r]
        LEFT JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
        LEFT JOIN [Branch2s] AS [b0] ON [r].[Id] = [b0].[Id]
        LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
        LEFT JOIN [Leaf2s] AS [l0] ON [r].[Id] = [l0].[Id]
    ) AS [s] ON [e0].[RootSkipSharedId] = [s].[Id]
) AS [s0] ON [e].[Key1] = [s0].[CompositeKeySkipSharedKey1] AND [e].[Key2] = [s0].[CompositeKeySkipSharedKey2] AND [e].[Key3] = [s0].[CompositeKeySkipSharedKey3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [s0].[RootSkipSharedId], [s0].[CompositeKeySkipSharedKey1], [s0].[CompositeKeySkipSharedKey2], [s0].[CompositeKeySkipSharedKey3]
""");
    }

    public override async Task Include_skip_navigation_then_reference(bool async)
    {
        await base.Include_skip_navigation_then_reference(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId], [s].[Id], [s].[Name], [s].[Id0], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name0], [s].[ReferenceInverseId], [s].[OneId], [s].[TwoId]
FROM [EntityTwos] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId], [j].[OneId], [j].[TwoId]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
) AS [s] ON [e].[Id] = [s].[TwoId]
ORDER BY [e].[Id], [s].[OneId], [s].[TwoId], [s].[Id]
""");
    }

    public override async Task Include_skip_navigation_then_include_skip_navigation(bool async)
    {
        await base.Include_skip_navigation_then_include_skip_navigation(async);

        AssertSql(
            """
SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [s1].[Id], [s1].[Name], [s1].[Number], [s1].[IsGreen], [s1].[LeafId], [s1].[CompositeId1], [s1].[CompositeId2], [s1].[CompositeId3], [s1].[Id0], [s1].[Name0], [s1].[EntityBranchId], [s1].[EntityOneId]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [s].[Id], [s].[Name], [s].[Number], [s].[IsGreen], [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [s0].[Id] AS [Id0], [s0].[Name] AS [Name0], [s0].[EntityBranchId], [s0].[EntityOneId]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN (
        SELECT [r].[Id], [r].[Name], [b].[Number], [l].[IsGreen]
        FROM [Roots] AS [r]
        INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
        INNER JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
    ) AS [s] ON [j].[LeafId] = [s].[Id]
    LEFT JOIN (
        SELECT [e0].[Id], [e0].[Name], [j0].[EntityBranchId], [j0].[EntityOneId]
        FROM [JoinOneToBranch] AS [j0]
        INNER JOIN [EntityOnes] AS [e0] ON [j0].[EntityOneId] = [e0].[Id]
    ) AS [s0] ON [s].[Id] = [s0].[EntityBranchId]
) AS [s1] ON [e].[Key1] = [s1].[CompositeId1] AND [e].[Key2] = [s1].[CompositeId2] AND [e].[Key3] = [s1].[CompositeId3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [s1].[LeafId], [s1].[CompositeId1], [s1].[CompositeId2], [s1].[CompositeId3], [s1].[Id], [s1].[EntityBranchId], [s1].[EntityOneId]
""");
    }

    public override async Task Include_skip_navigation_then_include_reference_and_skip_navigation(bool async)
    {
        await base.Include_skip_navigation_then_include_reference_and_skip_navigation(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId], [s0].[Id], [s0].[Name], [s0].[Id0], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name0], [s0].[ReferenceInverseId], [s0].[OneId], [s0].[ThreeId], [s0].[Id1], [s0].[Name1], [s0].[LeftId], [s0].[RightId]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId], [j].[OneId], [j].[ThreeId], [s].[Id] AS [Id1], [s].[Name] AS [Name1], [s].[LeftId], [s].[RightId]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
    LEFT JOIN (
        SELECT [e2].[Id], [e2].[Name], [j0].[LeftId], [j0].[RightId]
        FROM [JoinOneSelfPayload] AS [j0]
        INNER JOIN [EntityOnes] AS [e2] ON [j0].[RightId] = [e2].[Id]
    ) AS [s] ON [e0].[Id] = [s].[LeftId]
) AS [s0] ON [e].[Id] = [s0].[ThreeId]
ORDER BY [e].[Id], [s0].[OneId], [s0].[ThreeId], [s0].[Id], [s0].[Id0], [s0].[LeftId], [s0].[RightId]
""");
    }

    public override async Task Include_skip_navigation_and_reference(bool async)
    {
        await base.Include_skip_navigation_and_reference(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId], [e0].[Id], [s].[Id], [s].[Name], [s].[OneSkipSharedId], [s].[TwoSkipSharedId], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId]
FROM [EntityTwos] AS [e]
LEFT JOIN [EntityThrees] AS [e0] ON [e].[Id] = [e0].[ReferenceInverseId]
LEFT JOIN (
    SELECT [e2].[Id], [e2].[Name], [e1].[OneSkipSharedId], [e1].[TwoSkipSharedId]
    FROM [EntityOneEntityTwo] AS [e1]
    INNER JOIN [EntityOnes] AS [e2] ON [e1].[OneSkipSharedId] = [e2].[Id]
) AS [s] ON [e].[Id] = [s].[TwoSkipSharedId]
ORDER BY [e].[Id], [e0].[Id], [s].[OneSkipSharedId], [s].[TwoSkipSharedId]
""");
    }

    public override async Task Filtered_include_skip_navigation_where(bool async)
    {
        await base.Filtered_include_skip_navigation_where(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId], [s].[Id], [s].[Name], [s].[OneId], [s].[ThreeId]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[Name], [j].[OneId], [j].[ThreeId]
    FROM [JoinOneToThreePayloadFullShared] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [s] ON [e].[Id] = [s].[ThreeId]
ORDER BY [e].[Id], [s].[OneId], [s].[ThreeId]
""");
    }

    public override async Task Filtered_include_skip_navigation_order_by(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId], [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId], [s].[ThreeId], [s].[TwoId]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], [j].[ThreeId], [j].[TwoId]
    FROM [JoinTwoToThree] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
) AS [s] ON [e].[Id] = [s].[ThreeId]
ORDER BY [e].[Id], [s].[Id], [s].[ThreeId]
""");
    }

    public override async Task Filtered_include_skip_navigation_order_by_skip(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId], [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[ReferenceInverseId], [s0].[SelfSkipSharedLeftId], [s0].[SelfSkipSharedRightId]
FROM [EntityTwos] AS [e]
LEFT JOIN (
    SELECT [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId], [s].[SelfSkipSharedLeftId], [s].[SelfSkipSharedRightId]
    FROM (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], [e0].[SelfSkipSharedLeftId], [e0].[SelfSkipSharedRightId], ROW_NUMBER() OVER(PARTITION BY [e0].[SelfSkipSharedLeftId] ORDER BY [e1].[Id]) AS [row]
        FROM [EntityTwoEntityTwo] AS [e0]
        INNER JOIN [EntityTwos] AS [e1] ON [e0].[SelfSkipSharedRightId] = [e1].[Id]
    ) AS [s]
    WHERE 2 < [s].[row]
) AS [s0] ON [e].[Id] = [s0].[SelfSkipSharedLeftId]
ORDER BY [e].[Id], [s0].[SelfSkipSharedLeftId], [s0].[Id]
""");
    }

    public override async Task Filtered_include_skip_navigation_order_by_take(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_take(async);

        AssertSql(
            """
SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[ReferenceInverseId], [s0].[TwoSkipSharedId], [s0].[CompositeKeySkipSharedKey1], [s0].[CompositeKeySkipSharedKey2], [s0].[CompositeKeySkipSharedKey3]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId], [s].[TwoSkipSharedId], [s].[CompositeKeySkipSharedKey1], [s].[CompositeKeySkipSharedKey2], [s].[CompositeKeySkipSharedKey3]
    FROM (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], [e0].[TwoSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], ROW_NUMBER() OVER(PARTITION BY [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3] ORDER BY [e1].[Id]) AS [row]
        FROM [EntityCompositeKeyEntityTwo] AS [e0]
        INNER JOIN [EntityTwos] AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
    ) AS [s]
    WHERE [s].[row] <= 2
) AS [s0] ON [e].[Key1] = [s0].[CompositeKeySkipSharedKey1] AND [e].[Key2] = [s0].[CompositeKeySkipSharedKey2] AND [e].[Key3] = [s0].[CompositeKeySkipSharedKey3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [s0].[CompositeKeySkipSharedKey1], [s0].[CompositeKeySkipSharedKey2], [s0].[CompositeKeySkipSharedKey3], [s0].[Id]
""");
    }

    public override async Task Filtered_include_skip_navigation_order_by_skip_take(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip_take(async);

        AssertSql(
            """
SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [s0].[Id], [s0].[CollectionInverseId], [s0].[Name], [s0].[ReferenceInverseId], [s0].[Id0]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [s].[Id], [s].[CollectionInverseId], [s].[Name], [s].[ReferenceInverseId], [s].[Id0], [s].[CompositeId1], [s].[CompositeId2], [s].[CompositeId3]
    FROM (
        SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId], [j].[Id] AS [Id0], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], ROW_NUMBER() OVER(PARTITION BY [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinThreeToCompositeKeyFull] AS [j]
        INNER JOIN [EntityThrees] AS [e0] ON [j].[ThreeId] = [e0].[Id]
    ) AS [s]
    WHERE 1 < [s].[row] AND [s].[row] <= 3
) AS [s0] ON [e].[Key1] = [s0].[CompositeId1] AND [e].[Key2] = [s0].[CompositeId2] AND [e].[Key3] = [s0].[CompositeId3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3], [s0].[Id]
""");
    }

    public override async Task Filtered_then_include_skip_navigation_where(bool async)
    {
        await base.Filtered_then_include_skip_navigation_where(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [b].[Number], [b0].[Slumber], [l].[IsGreen], [l0].[IsBrown], CASE
    WHEN [l0].[Id] IS NOT NULL THEN N'EntityLeaf2'
    WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
    WHEN [b].[Id] IS NOT NULL THEN N'EntityBranch'
END AS [Discriminator], [s0].[Id], [s0].[CollectionInverseId], [s0].[Name], [s0].[ReferenceInverseId], [s0].[RootSkipSharedId], [s0].[ThreeSkipSharedId], [s0].[Id0], [s0].[Name0], [s0].[OneId], [s0].[ThreeId]
FROM [Roots] AS [r]
LEFT JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
LEFT JOIN [Branch2s] AS [b0] ON [r].[Id] = [b0].[Id]
LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
LEFT JOIN [Leaf2s] AS [l0] ON [r].[Id] = [l0].[Id]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId], [e].[RootSkipSharedId], [e].[ThreeSkipSharedId], [s].[Id] AS [Id0], [s].[Name] AS [Name0], [s].[OneId], [s].[ThreeId]
    FROM [EntityRootEntityThree] AS [e]
    INNER JOIN [EntityThrees] AS [e0] ON [e].[ThreeSkipSharedId] = [e0].[Id]
    LEFT JOIN (
        SELECT [e1].[Id], [e1].[Name], [j].[OneId], [j].[ThreeId]
        FROM [JoinOneToThreePayloadFullShared] AS [j]
        INNER JOIN [EntityOnes] AS [e1] ON [j].[OneId] = [e1].[Id]
        WHERE [e1].[Id] < 10
    ) AS [s] ON [e0].[Id] = [s].[ThreeId]
) AS [s0] ON [r].[Id] = [s0].[RootSkipSharedId]
ORDER BY [r].[Id], [s0].[RootSkipSharedId], [s0].[ThreeSkipSharedId], [s0].[Id], [s0].[OneId], [s0].[ThreeId]
""");
    }

    public override async Task Filtered_then_include_skip_navigation_order_by_skip_take(bool async)
    {
        await base.Filtered_then_include_skip_navigation_order_by_skip_take(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [b].[Number], [b0].[Slumber], [l].[IsGreen], [l0].[IsBrown], CASE
    WHEN [l0].[Id] IS NOT NULL THEN N'EntityLeaf2'
    WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
    WHEN [b].[Id] IS NOT NULL THEN N'EntityBranch'
END AS [Discriminator], [s1].[Key1], [s1].[Key2], [s1].[Key3], [s1].[Name], [s1].[RootSkipSharedId], [s1].[CompositeKeySkipSharedKey1], [s1].[CompositeKeySkipSharedKey2], [s1].[CompositeKeySkipSharedKey3], [s1].[Id], [s1].[CollectionInverseId], [s1].[Name0], [s1].[ReferenceInverseId], [s1].[Id0]
FROM [Roots] AS [r]
LEFT JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
LEFT JOIN [Branch2s] AS [b0] ON [r].[Id] = [b0].[Id]
LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
LEFT JOIN [Leaf2s] AS [l0] ON [r].[Id] = [l0].[Id]
LEFT JOIN (
    SELECT [e0].[Key1], [e0].[Key2], [e0].[Key3], [e0].[Name], [e].[RootSkipSharedId], [e].[CompositeKeySkipSharedKey1], [e].[CompositeKeySkipSharedKey2], [e].[CompositeKeySkipSharedKey3], [s0].[Id], [s0].[CollectionInverseId], [s0].[Name] AS [Name0], [s0].[ReferenceInverseId], [s0].[Id0], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3]
    FROM [EntityCompositeKeyEntityRoot] AS [e]
    INNER JOIN [EntityCompositeKeys] AS [e0] ON [e].[CompositeKeySkipSharedKey1] = [e0].[Key1] AND [e].[CompositeKeySkipSharedKey2] = [e0].[Key2] AND [e].[CompositeKeySkipSharedKey3] = [e0].[Key3]
    LEFT JOIN (
        SELECT [s].[Id], [s].[CollectionInverseId], [s].[Name], [s].[ReferenceInverseId], [s].[Id0], [s].[CompositeId1], [s].[CompositeId2], [s].[CompositeId3]
        FROM (
            SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], [j].[Id] AS [Id0], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], ROW_NUMBER() OVER(PARTITION BY [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3] ORDER BY [e1].[Id]) AS [row]
            FROM [JoinThreeToCompositeKeyFull] AS [j]
            INNER JOIN [EntityThrees] AS [e1] ON [j].[ThreeId] = [e1].[Id]
        ) AS [s]
        WHERE 1 < [s].[row] AND [s].[row] <= 3
    ) AS [s0] ON [e0].[Key1] = [s0].[CompositeId1] AND [e0].[Key2] = [s0].[CompositeId2] AND [e0].[Key3] = [s0].[CompositeId3]
) AS [s1] ON [r].[Id] = [s1].[RootSkipSharedId]
ORDER BY [r].[Id], [s1].[RootSkipSharedId], [s1].[CompositeKeySkipSharedKey1], [s1].[CompositeKeySkipSharedKey2], [s1].[CompositeKeySkipSharedKey3], [s1].[Key1], [s1].[Key2], [s1].[Key3], [s1].[CompositeId1], [s1].[CompositeId2], [s1].[CompositeId3], [s1].[Id]
""");
    }

    public override async Task Filtered_include_skip_navigation_where_then_include_skip_navigation(bool async)
    {
        await base.Filtered_include_skip_navigation_where_then_include_skip_navigation(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [b].[Number], [l].[IsGreen], [s0].[Key1], [s0].[Key2], [s0].[Key3], [s0].[Name], [s0].[LeafId], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3], [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name0], [s0].[ReferenceInverseId], [s0].[TwoSkipSharedId], [s0].[CompositeKeySkipSharedKey1], [s0].[CompositeKeySkipSharedKey2], [s0].[CompositeKeySkipSharedKey3]
FROM [Roots] AS [r]
INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
INNER JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
LEFT JOIN (
    SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name] AS [Name0], [s].[ReferenceInverseId], [s].[TwoSkipSharedId], [s].[CompositeKeySkipSharedKey1], [s].[CompositeKeySkipSharedKey2], [s].[CompositeKeySkipSharedKey3]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN [EntityCompositeKeys] AS [e] ON [j].[CompositeId1] = [e].[Key1] AND [j].[CompositeId2] = [e].[Key2] AND [j].[CompositeId3] = [e].[Key3]
    LEFT JOIN (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], [e0].[TwoSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3]
        FROM [EntityCompositeKeyEntityTwo] AS [e0]
        INNER JOIN [EntityTwos] AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
    ) AS [s] ON [e].[Key1] = [s].[CompositeKeySkipSharedKey1] AND [e].[Key2] = [s].[CompositeKeySkipSharedKey2] AND [e].[Key3] = [s].[CompositeKeySkipSharedKey3]
    WHERE [e].[Key1] < 5
) AS [s0] ON [r].[Id] = [s0].[LeafId]
ORDER BY [r].[Id], [s0].[LeafId], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3], [s0].[Key1], [s0].[Key2], [s0].[Key3], [s0].[TwoSkipSharedId], [s0].[CompositeKeySkipSharedKey1], [s0].[CompositeKeySkipSharedKey2], [s0].[CompositeKeySkipSharedKey3]
""");
    }

    public override async Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], [s1].[Id], [s1].[CollectionInverseId], [s1].[ExtraId], [s1].[Name], [s1].[ReferenceInverseId], [s1].[OneId], [s1].[TwoId], [s1].[Id0], [s1].[CollectionInverseId0], [s1].[Name0], [s1].[ReferenceInverseId0], [s1].[ThreeId], [s1].[TwoId0]
FROM [EntityOnes] AS [e]
OUTER APPLY (
    SELECT [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId], [s].[OneId], [s].[TwoId], [s0].[Id] AS [Id0], [s0].[CollectionInverseId] AS [CollectionInverseId0], [s0].[Name] AS [Name0], [s0].[ReferenceInverseId] AS [ReferenceInverseId0], [s0].[ThreeId], [s0].[TwoId] AS [TwoId0]
    FROM (
        SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], [j].[OneId], [j].[TwoId]
        FROM [JoinOneToTwo] AS [j]
        INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
        WHERE [e].[Id] = [j].[OneId]
        ORDER BY [e0].[Id]
        OFFSET 1 ROWS FETCH NEXT 2 ROWS ONLY
    ) AS [s]
    LEFT JOIN (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], [j0].[ThreeId], [j0].[TwoId]
        FROM [JoinTwoToThree] AS [j0]
        INNER JOIN [EntityThrees] AS [e1] ON [j0].[ThreeId] = [e1].[Id]
        WHERE [e1].[Id] < 10
    ) AS [s0] ON [s].[Id] = [s0].[TwoId]
) AS [s1]
ORDER BY [e].[Id], [s1].[Id], [s1].[OneId], [s1].[TwoId], [s1].[ThreeId], [s1].[TwoId0]
""");
    }

    public override async Task Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take(bool async)
    {
        await base.Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], [s1].[Id], [s1].[CollectionInverseId], [s1].[ExtraId], [s1].[Name], [s1].[ReferenceInverseId], [s1].[OneId], [s1].[TwoId], [s1].[Id0], [s1].[CollectionInverseId0], [s1].[Name0], [s1].[ReferenceInverseId0], [s1].[ThreeId], [s1].[TwoId0]
FROM [EntityOnes] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], [j].[OneId], [j].[TwoId], [s0].[Id] AS [Id0], [s0].[CollectionInverseId] AS [CollectionInverseId0], [s0].[Name] AS [Name0], [s0].[ReferenceInverseId] AS [ReferenceInverseId0], [s0].[ThreeId], [s0].[TwoId] AS [TwoId0]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    LEFT JOIN (
        SELECT [s].[Id], [s].[CollectionInverseId], [s].[Name], [s].[ReferenceInverseId], [s].[ThreeId], [s].[TwoId]
        FROM (
            SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], [j0].[ThreeId], [j0].[TwoId], ROW_NUMBER() OVER(PARTITION BY [j0].[TwoId] ORDER BY [e1].[Id]) AS [row]
            FROM [JoinTwoToThree] AS [j0]
            INNER JOIN [EntityThrees] AS [e1] ON [j0].[ThreeId] = [e1].[Id]
        ) AS [s]
        WHERE 1 < [s].[row] AND [s].[row] <= 3
    ) AS [s0] ON [e0].[Id] = [s0].[TwoId]
    WHERE [e0].[Id] < 10
) AS [s1] ON [e].[Id] = [s1].[OneId]
ORDER BY [e].[Id], [s1].[OneId], [s1].[TwoId], [s1].[Id], [s1].[TwoId0], [s1].[Id0]
""");
    }

    public override async Task Filter_include_on_skip_navigation_combined(bool async)
    {
        await base.Filter_include_on_skip_navigation_combined(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId], [s].[Id], [s].[Name], [s].[Id0], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name0], [s].[ReferenceInverseId], [s].[OneId], [s].[TwoId], [s].[Id1], [s].[CollectionInverseId0], [s].[ExtraId0], [s].[Name1], [s].[ReferenceInverseId0]
FROM [EntityTwos] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId], [j].[OneId], [j].[TwoId], [e2].[Id] AS [Id1], [e2].[CollectionInverseId] AS [CollectionInverseId0], [e2].[ExtraId] AS [ExtraId0], [e2].[Name] AS [Name1], [e2].[ReferenceInverseId] AS [ReferenceInverseId0]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
    LEFT JOIN [EntityTwos] AS [e2] ON [e0].[Id] = [e2].[CollectionInverseId]
    WHERE [e0].[Id] < 10
) AS [s] ON [e].[Id] = [s].[TwoId]
ORDER BY [e].[Id], [s].[OneId], [s].[TwoId], [s].[Id], [s].[Id0]
""");
    }

    public override async Task Filter_include_on_skip_navigation_combined_with_filtered_then_includes(bool async)
    {
        await base.Filter_include_on_skip_navigation_combined_with_filtered_then_includes(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId], [s3].[Id], [s3].[Name], [s3].[OneId], [s3].[ThreeId], [s3].[Id0], [s3].[CollectionInverseId], [s3].[ExtraId], [s3].[Name0], [s3].[ReferenceInverseId], [s3].[OneId0], [s3].[TwoId], [s3].[Id1], [s3].[Name1], [s3].[Number], [s3].[IsGreen], [s3].[Discriminator], [s3].[EntityBranchId], [s3].[EntityOneId]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[Name], [j].[OneId], [j].[ThreeId], [s1].[Id] AS [Id0], [s1].[CollectionInverseId], [s1].[ExtraId], [s1].[Name] AS [Name0], [s1].[ReferenceInverseId], [s1].[OneId] AS [OneId0], [s1].[TwoId], [s2].[Id] AS [Id1], [s2].[Name] AS [Name1], [s2].[Number], [s2].[IsGreen], [s2].[Discriminator], [s2].[EntityBranchId], [s2].[EntityOneId]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN (
        SELECT [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[ReferenceInverseId], [s0].[OneId], [s0].[TwoId]
        FROM (
            SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], [j0].[OneId], [j0].[TwoId], ROW_NUMBER() OVER(PARTITION BY [j0].[OneId] ORDER BY [e1].[Id]) AS [row]
            FROM [JoinOneToTwo] AS [j0]
            INNER JOIN [EntityTwos] AS [e1] ON [j0].[TwoId] = [e1].[Id]
        ) AS [s0]
        WHERE 1 < [s0].[row] AND [s0].[row] <= 3
    ) AS [s1] ON [e0].[Id] = [s1].[OneId]
    LEFT JOIN (
        SELECT [s].[Id], [s].[Name], [s].[Number], [s].[IsGreen], [s].[Discriminator], [j1].[EntityBranchId], [j1].[EntityOneId]
        FROM [JoinOneToBranch] AS [j1]
        INNER JOIN (
            SELECT [r].[Id], [r].[Name], [b].[Number], [l].[IsGreen], CASE
                WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
            END AS [Discriminator]
            FROM [Roots] AS [r]
            INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
            LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
        ) AS [s] ON [j1].[EntityBranchId] = [s].[Id]
        WHERE [s].[Id] < 20
    ) AS [s2] ON [e0].[Id] = [s2].[EntityOneId]
    WHERE [e0].[Id] < 10
) AS [s3] ON [e].[Id] = [s3].[ThreeId]
ORDER BY [e].[Id], [s3].[OneId], [s3].[ThreeId], [s3].[Id], [s3].[OneId0], [s3].[Id0], [s3].[TwoId], [s3].[EntityBranchId], [s3].[EntityOneId]
""");
    }

    public override async Task Filtered_include_on_skip_navigation_then_filtered_include_on_navigation(bool async)
    {
        await base.Filtered_include_on_skip_navigation_then_filtered_include_on_navigation(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId], [s].[Id], [s].[Name], [s].[OneId], [s].[ThreeId], [s].[Id0], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name0], [s].[ReferenceInverseId]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[Name], [j].[OneId], [j].[ThreeId], [e2].[Id] AS [Id0], [e2].[CollectionInverseId], [e2].[ExtraId], [e2].[Name] AS [Name0], [e2].[ReferenceInverseId]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId]
        FROM [EntityTwos] AS [e1]
        WHERE [e1].[Id] < 5
    ) AS [e2] ON [e0].[Id] = [e2].[CollectionInverseId]
    WHERE [e0].[Id] > 15
) AS [s] ON [e].[Id] = [s].[ThreeId]
ORDER BY [e].[Id], [s].[OneId], [s].[ThreeId], [s].[Id]
""");
    }

    public override async Task Filtered_include_on_navigation_then_filtered_include_on_skip_navigation(bool async)
    {
        await base.Filtered_include_on_navigation_then_filtered_include_on_skip_navigation(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[ReferenceInverseId], [s0].[Id0], [s0].[CollectionInverseId0], [s0].[Name0], [s0].[ReferenceInverseId0], [s0].[ThreeId], [s0].[TwoId]
FROM [EntityOnes] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], [s].[Id] AS [Id0], [s].[CollectionInverseId] AS [CollectionInverseId0], [s].[Name] AS [Name0], [s].[ReferenceInverseId] AS [ReferenceInverseId0], [s].[ThreeId], [s].[TwoId]
    FROM [EntityTwos] AS [e0]
    LEFT JOIN (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], [j].[ThreeId], [j].[TwoId]
        FROM [JoinTwoToThree] AS [j]
        INNER JOIN [EntityThrees] AS [e1] ON [j].[ThreeId] = [e1].[Id]
        WHERE [e1].[Id] < 5
    ) AS [s] ON [e0].[Id] = [s].[TwoId]
    WHERE [e0].[Id] > 15
) AS [s0] ON [e].[Id] = [s0].[CollectionInverseId]
ORDER BY [e].[Id], [s0].[Id], [s0].[ThreeId], [s0].[TwoId]
""");
    }

    public override async Task Includes_accessed_via_different_path_are_merged(bool async)
    {
        await base.Includes_accessed_via_different_path_are_merged(async);

        AssertSql(" ");
    }

    public override async Task Filtered_includes_accessed_via_different_path_are_merged(bool async)
    {
        await base.Filtered_includes_accessed_via_different_path_are_merged(async);

        AssertSql(" ");
    }

    public override async Task Include_skip_navigation_split(bool async)
    {
        await base.Include_skip_navigation_split(async);

        AssertSql(
            """
SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name]
FROM [EntityCompositeKeys] AS [e]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3]
""",
            //
            """
SELECT [s0].[Id], [s0].[Name], [s0].[Number], [s0].[Slumber], [s0].[IsGreen], [s0].[IsBrown], [s0].[Discriminator], [e].[Key1], [e].[Key2], [e].[Key3]
FROM [EntityCompositeKeys] AS [e]
INNER JOIN (
    SELECT [s].[Id], [s].[Name], [s].[Number], [s].[Slumber], [s].[IsGreen], [s].[IsBrown], [s].[Discriminator], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3]
    FROM [EntityCompositeKeyEntityRoot] AS [e0]
    INNER JOIN (
        SELECT [r].[Id], [r].[Name], [b].[Number], [b0].[Slumber], [l].[IsGreen], [l0].[IsBrown], CASE
            WHEN [l0].[Id] IS NOT NULL THEN N'EntityLeaf2'
            WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
            WHEN [b].[Id] IS NOT NULL THEN N'EntityBranch'
        END AS [Discriminator]
        FROM [Roots] AS [r]
        LEFT JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
        LEFT JOIN [Branch2s] AS [b0] ON [r].[Id] = [b0].[Id]
        LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
        LEFT JOIN [Leaf2s] AS [l0] ON [r].[Id] = [l0].[Id]
    ) AS [s] ON [e0].[RootSkipSharedId] = [s].[Id]
) AS [s0] ON [e].[Key1] = [s0].[CompositeKeySkipSharedKey1] AND [e].[Key2] = [s0].[CompositeKeySkipSharedKey2] AND [e].[Key3] = [s0].[CompositeKeySkipSharedKey3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3]
""");
    }

    public override async Task Include_skip_navigation_then_reference_split(bool async)
    {
        await base.Include_skip_navigation_then_reference_split(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId]
FROM [EntityTwos] AS [e]
ORDER BY [e].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Name], [s].[Id0], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name0], [s].[ReferenceInverseId], [e].[Id]
FROM [EntityTwos] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId], [j].[TwoId]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
) AS [s] ON [e].[Id] = [s].[TwoId]
ORDER BY [e].[Id]
""");
    }

    public override async Task Include_skip_navigation_then_include_skip_navigation_split(bool async)
    {
        await base.Include_skip_navigation_then_include_skip_navigation_split(async);

        AssertSql(
            """
SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name]
FROM [EntityCompositeKeys] AS [e]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3]
""",
            //
            """
SELECT [s0].[Id], [s0].[Name], [s0].[Number], [s0].[IsGreen], [e].[Key1], [e].[Key2], [e].[Key3], [s0].[LeafId], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3]
FROM [EntityCompositeKeys] AS [e]
INNER JOIN (
    SELECT [s].[Id], [s].[Name], [s].[Number], [s].[IsGreen], [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN (
        SELECT [r].[Id], [r].[Name], [b].[Number], [l].[IsGreen]
        FROM [Roots] AS [r]
        INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
        INNER JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
    ) AS [s] ON [j].[LeafId] = [s].[Id]
) AS [s0] ON [e].[Key1] = [s0].[CompositeId1] AND [e].[Key2] = [s0].[CompositeId2] AND [e].[Key3] = [s0].[CompositeId3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [s0].[LeafId], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3], [s0].[Id]
""",
            //
            """
SELECT [s1].[Id], [s1].[Name], [e].[Key1], [e].[Key2], [e].[Key3], [s0].[LeafId], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3], [s0].[Id]
FROM [EntityCompositeKeys] AS [e]
INNER JOIN (
    SELECT [s].[Id], [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN (
        SELECT [r].[Id]
        FROM [Roots] AS [r]
        INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
        INNER JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
    ) AS [s] ON [j].[LeafId] = [s].[Id]
) AS [s0] ON [e].[Key1] = [s0].[CompositeId1] AND [e].[Key2] = [s0].[CompositeId2] AND [e].[Key3] = [s0].[CompositeId3]
INNER JOIN (
    SELECT [e0].[Id], [e0].[Name], [j0].[EntityBranchId]
    FROM [JoinOneToBranch] AS [j0]
    INNER JOIN [EntityOnes] AS [e0] ON [j0].[EntityOneId] = [e0].[Id]
) AS [s1] ON [s0].[Id] = [s1].[EntityBranchId]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [s0].[LeafId], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3], [s0].[Id]
""");
    }

    public override async Task Include_skip_navigation_then_include_reference_and_skip_navigation_split(bool async)
    {
        await base.Include_skip_navigation_then_include_reference_and_skip_navigation_split(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId]
FROM [EntityThrees] AS [e]
ORDER BY [e].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Name], [s].[Id0], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name0], [s].[ReferenceInverseId], [e].[Id], [s].[OneId], [s].[ThreeId]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId], [j].[OneId], [j].[ThreeId]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
) AS [s] ON [e].[Id] = [s].[ThreeId]
ORDER BY [e].[Id], [s].[OneId], [s].[ThreeId], [s].[Id], [s].[Id0]
""",
            //
            """
SELECT [s0].[Id], [s0].[Name], [e].[Id], [s].[OneId], [s].[ThreeId], [s].[Id], [s].[Id0]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e1].[Id] AS [Id0], [j].[OneId], [j].[ThreeId]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
) AS [s] ON [e].[Id] = [s].[ThreeId]
INNER JOIN (
    SELECT [e2].[Id], [e2].[Name], [j0].[LeftId]
    FROM [JoinOneSelfPayload] AS [j0]
    INNER JOIN [EntityOnes] AS [e2] ON [j0].[RightId] = [e2].[Id]
) AS [s0] ON [s].[Id] = [s0].[LeftId]
ORDER BY [e].[Id], [s].[OneId], [s].[ThreeId], [s].[Id], [s].[Id0]
""");
    }

    public override async Task Include_skip_navigation_and_reference_split(bool async)
    {
        await base.Include_skip_navigation_and_reference_split(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId], [e0].[Id], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId]
FROM [EntityTwos] AS [e]
LEFT JOIN [EntityThrees] AS [e0] ON [e].[Id] = [e0].[ReferenceInverseId]
ORDER BY [e].[Id], [e0].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Name], [e].[Id], [e0].[Id]
FROM [EntityTwos] AS [e]
LEFT JOIN [EntityThrees] AS [e0] ON [e].[Id] = [e0].[ReferenceInverseId]
INNER JOIN (
    SELECT [e2].[Id], [e2].[Name], [e1].[TwoSkipSharedId]
    FROM [EntityOneEntityTwo] AS [e1]
    INNER JOIN [EntityOnes] AS [e2] ON [e1].[OneSkipSharedId] = [e2].[Id]
) AS [s] ON [e].[Id] = [s].[TwoSkipSharedId]
ORDER BY [e].[Id], [e0].[Id]
""");
    }

    public override async Task Filtered_include_skip_navigation_where_split(bool async)
    {
        await base.Filtered_include_skip_navigation_where_split(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId]
FROM [EntityThrees] AS [e]
ORDER BY [e].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Name], [e].[Id]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e0].[Name], [j].[ThreeId]
    FROM [JoinOneToThreePayloadFullShared] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [s] ON [e].[Id] = [s].[ThreeId]
ORDER BY [e].[Id]
""");
    }

    public override async Task Filtered_include_skip_navigation_order_by_split(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_split(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId]
FROM [EntityThrees] AS [e]
ORDER BY [e].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId], [e].[Id]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], [j].[ThreeId]
    FROM [JoinTwoToThree] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
) AS [s] ON [e].[Id] = [s].[ThreeId]
ORDER BY [e].[Id], [s].[Id]
""");
    }

    public override async Task Filtered_include_skip_navigation_order_by_skip_split(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip_split(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId]
FROM [EntityTwos] AS [e]
ORDER BY [e].[Id]
""",
            //
            """
SELECT [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[ReferenceInverseId], [e].[Id]
FROM [EntityTwos] AS [e]
INNER JOIN (
    SELECT [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId], [s].[SelfSkipSharedLeftId]
    FROM (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], [e0].[SelfSkipSharedLeftId], ROW_NUMBER() OVER(PARTITION BY [e0].[SelfSkipSharedLeftId] ORDER BY [e1].[Id]) AS [row]
        FROM [EntityTwoEntityTwo] AS [e0]
        INNER JOIN [EntityTwos] AS [e1] ON [e0].[SelfSkipSharedRightId] = [e1].[Id]
    ) AS [s]
    WHERE 2 < [s].[row]
) AS [s0] ON [e].[Id] = [s0].[SelfSkipSharedLeftId]
ORDER BY [e].[Id], [s0].[SelfSkipSharedLeftId], [s0].[Id]
""");
    }

    public override async Task Filtered_include_skip_navigation_order_by_take_split(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_take_split(async);

        AssertSql(
            """
SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name]
FROM [EntityCompositeKeys] AS [e]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3]
""",
            //
            """
SELECT [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[ReferenceInverseId], [e].[Key1], [e].[Key2], [e].[Key3]
FROM [EntityCompositeKeys] AS [e]
INNER JOIN (
    SELECT [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId], [s].[CompositeKeySkipSharedKey1], [s].[CompositeKeySkipSharedKey2], [s].[CompositeKeySkipSharedKey3]
    FROM (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], ROW_NUMBER() OVER(PARTITION BY [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3] ORDER BY [e1].[Id]) AS [row]
        FROM [EntityCompositeKeyEntityTwo] AS [e0]
        INNER JOIN [EntityTwos] AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
    ) AS [s]
    WHERE [s].[row] <= 2
) AS [s0] ON [e].[Key1] = [s0].[CompositeKeySkipSharedKey1] AND [e].[Key2] = [s0].[CompositeKeySkipSharedKey2] AND [e].[Key3] = [s0].[CompositeKeySkipSharedKey3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [s0].[CompositeKeySkipSharedKey1], [s0].[CompositeKeySkipSharedKey2], [s0].[CompositeKeySkipSharedKey3], [s0].[Id]
""");
    }

    public override async Task Filtered_include_skip_navigation_order_by_skip_take_split(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip_take_split(async);

        AssertSql(
            """
SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name]
FROM [EntityCompositeKeys] AS [e]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3]
""",
            //
            """
SELECT [s0].[Id], [s0].[CollectionInverseId], [s0].[Name], [s0].[ReferenceInverseId], [e].[Key1], [e].[Key2], [e].[Key3]
FROM [EntityCompositeKeys] AS [e]
INNER JOIN (
    SELECT [s].[Id], [s].[CollectionInverseId], [s].[Name], [s].[ReferenceInverseId], [s].[CompositeId1], [s].[CompositeId2], [s].[CompositeId3]
    FROM (
        SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], ROW_NUMBER() OVER(PARTITION BY [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinThreeToCompositeKeyFull] AS [j]
        INNER JOIN [EntityThrees] AS [e0] ON [j].[ThreeId] = [e0].[Id]
    ) AS [s]
    WHERE 1 < [s].[row] AND [s].[row] <= 3
) AS [s0] ON [e].[Key1] = [s0].[CompositeId1] AND [e].[Key2] = [s0].[CompositeId2] AND [e].[Key3] = [s0].[CompositeId3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3], [s0].[Id]
""");
    }

    public override async Task Filtered_then_include_skip_navigation_where_split(bool async)
    {
        await base.Filtered_then_include_skip_navigation_where_split(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [b].[Number], [b0].[Slumber], [l].[IsGreen], [l0].[IsBrown], CASE
    WHEN [l0].[Id] IS NOT NULL THEN N'EntityLeaf2'
    WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
    WHEN [b].[Id] IS NOT NULL THEN N'EntityBranch'
END AS [Discriminator]
FROM [Roots] AS [r]
LEFT JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
LEFT JOIN [Branch2s] AS [b0] ON [r].[Id] = [b0].[Id]
LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
LEFT JOIN [Leaf2s] AS [l0] ON [r].[Id] = [l0].[Id]
ORDER BY [r].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[CollectionInverseId], [s].[Name], [s].[ReferenceInverseId], [r].[Id], [s].[RootSkipSharedId], [s].[ThreeSkipSharedId]
FROM [Roots] AS [r]
INNER JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId], [e].[RootSkipSharedId], [e].[ThreeSkipSharedId]
    FROM [EntityRootEntityThree] AS [e]
    INNER JOIN [EntityThrees] AS [e0] ON [e].[ThreeSkipSharedId] = [e0].[Id]
) AS [s] ON [r].[Id] = [s].[RootSkipSharedId]
ORDER BY [r].[Id], [s].[RootSkipSharedId], [s].[ThreeSkipSharedId], [s].[Id]
""",
            //
            """
SELECT [s0].[Id], [s0].[Name], [r].[Id], [s].[RootSkipSharedId], [s].[ThreeSkipSharedId], [s].[Id]
FROM [Roots] AS [r]
INNER JOIN (
    SELECT [e0].[Id], [e].[RootSkipSharedId], [e].[ThreeSkipSharedId]
    FROM [EntityRootEntityThree] AS [e]
    INNER JOIN [EntityThrees] AS [e0] ON [e].[ThreeSkipSharedId] = [e0].[Id]
) AS [s] ON [r].[Id] = [s].[RootSkipSharedId]
INNER JOIN (
    SELECT [e1].[Id], [e1].[Name], [j].[ThreeId]
    FROM [JoinOneToThreePayloadFullShared] AS [j]
    INNER JOIN [EntityOnes] AS [e1] ON [j].[OneId] = [e1].[Id]
    WHERE [e1].[Id] < 10
) AS [s0] ON [s].[Id] = [s0].[ThreeId]
ORDER BY [r].[Id], [s].[RootSkipSharedId], [s].[ThreeSkipSharedId], [s].[Id]
""");
    }

    public override async Task Filtered_then_include_skip_navigation_order_by_skip_take_split(bool async)
    {
        await base.Filtered_then_include_skip_navigation_order_by_skip_take_split(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [b].[Number], [b0].[Slumber], [l].[IsGreen], [l0].[IsBrown], CASE
    WHEN [l0].[Id] IS NOT NULL THEN N'EntityLeaf2'
    WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
    WHEN [b].[Id] IS NOT NULL THEN N'EntityBranch'
END AS [Discriminator]
FROM [Roots] AS [r]
LEFT JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
LEFT JOIN [Branch2s] AS [b0] ON [r].[Id] = [b0].[Id]
LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
LEFT JOIN [Leaf2s] AS [l0] ON [r].[Id] = [l0].[Id]
ORDER BY [r].[Id]
""",
            //
            """
SELECT [s].[Key1], [s].[Key2], [s].[Key3], [s].[Name], [r].[Id], [s].[RootSkipSharedId], [s].[CompositeKeySkipSharedKey1], [s].[CompositeKeySkipSharedKey2], [s].[CompositeKeySkipSharedKey3]
FROM [Roots] AS [r]
INNER JOIN (
    SELECT [e0].[Key1], [e0].[Key2], [e0].[Key3], [e0].[Name], [e].[RootSkipSharedId], [e].[CompositeKeySkipSharedKey1], [e].[CompositeKeySkipSharedKey2], [e].[CompositeKeySkipSharedKey3]
    FROM [EntityCompositeKeyEntityRoot] AS [e]
    INNER JOIN [EntityCompositeKeys] AS [e0] ON [e].[CompositeKeySkipSharedKey1] = [e0].[Key1] AND [e].[CompositeKeySkipSharedKey2] = [e0].[Key2] AND [e].[CompositeKeySkipSharedKey3] = [e0].[Key3]
) AS [s] ON [r].[Id] = [s].[RootSkipSharedId]
ORDER BY [r].[Id], [s].[RootSkipSharedId], [s].[CompositeKeySkipSharedKey1], [s].[CompositeKeySkipSharedKey2], [s].[CompositeKeySkipSharedKey3], [s].[Key1], [s].[Key2], [s].[Key3]
""",
            //
            """
SELECT [s1].[Id], [s1].[CollectionInverseId], [s1].[Name], [s1].[ReferenceInverseId], [r].[Id], [s].[RootSkipSharedId], [s].[CompositeKeySkipSharedKey1], [s].[CompositeKeySkipSharedKey2], [s].[CompositeKeySkipSharedKey3], [s].[Key1], [s].[Key2], [s].[Key3]
FROM [Roots] AS [r]
INNER JOIN (
    SELECT [e0].[Key1], [e0].[Key2], [e0].[Key3], [e].[RootSkipSharedId], [e].[CompositeKeySkipSharedKey1], [e].[CompositeKeySkipSharedKey2], [e].[CompositeKeySkipSharedKey3]
    FROM [EntityCompositeKeyEntityRoot] AS [e]
    INNER JOIN [EntityCompositeKeys] AS [e0] ON [e].[CompositeKeySkipSharedKey1] = [e0].[Key1] AND [e].[CompositeKeySkipSharedKey2] = [e0].[Key2] AND [e].[CompositeKeySkipSharedKey3] = [e0].[Key3]
) AS [s] ON [r].[Id] = [s].[RootSkipSharedId]
INNER JOIN (
    SELECT [s0].[Id], [s0].[CollectionInverseId], [s0].[Name], [s0].[ReferenceInverseId], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3]
    FROM (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], ROW_NUMBER() OVER(PARTITION BY [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3] ORDER BY [e1].[Id]) AS [row]
        FROM [JoinThreeToCompositeKeyFull] AS [j]
        INNER JOIN [EntityThrees] AS [e1] ON [j].[ThreeId] = [e1].[Id]
    ) AS [s0]
    WHERE 1 < [s0].[row] AND [s0].[row] <= 3
) AS [s1] ON [s].[Key1] = [s1].[CompositeId1] AND [s].[Key2] = [s1].[CompositeId2] AND [s].[Key3] = [s1].[CompositeId3]
ORDER BY [r].[Id], [s].[RootSkipSharedId], [s].[CompositeKeySkipSharedKey1], [s].[CompositeKeySkipSharedKey2], [s].[CompositeKeySkipSharedKey3], [s].[Key1], [s].[Key2], [s].[Key3], [s1].[CompositeId1], [s1].[CompositeId2], [s1].[CompositeId3], [s1].[Id]
""");
    }

    public override async Task Filtered_include_skip_navigation_where_then_include_skip_navigation_split(bool async)
    {
        await base.Filtered_include_skip_navigation_where_then_include_skip_navigation_split(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [b].[Number], [l].[IsGreen]
FROM [Roots] AS [r]
INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
INNER JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
ORDER BY [r].[Id]
""",
            //
            """
SELECT [s].[Key1], [s].[Key2], [s].[Key3], [s].[Name], [r].[Id], [s].[LeafId], [s].[CompositeId1], [s].[CompositeId2], [s].[CompositeId3]
FROM [Roots] AS [r]
INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
INNER JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
INNER JOIN (
    SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN [EntityCompositeKeys] AS [e] ON [j].[CompositeId1] = [e].[Key1] AND [j].[CompositeId2] = [e].[Key2] AND [j].[CompositeId3] = [e].[Key3]
    WHERE [e].[Key1] < 5
) AS [s] ON [r].[Id] = [s].[LeafId]
ORDER BY [r].[Id], [s].[LeafId], [s].[CompositeId1], [s].[CompositeId2], [s].[CompositeId3], [s].[Key1], [s].[Key2], [s].[Key3]
""",
            //
            """
SELECT [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[ReferenceInverseId], [r].[Id], [s].[LeafId], [s].[CompositeId1], [s].[CompositeId2], [s].[CompositeId3], [s].[Key1], [s].[Key2], [s].[Key3]
FROM [Roots] AS [r]
INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
INNER JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
INNER JOIN (
    SELECT [e].[Key1], [e].[Key2], [e].[Key3], [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN [EntityCompositeKeys] AS [e] ON [j].[CompositeId1] = [e].[Key1] AND [j].[CompositeId2] = [e].[Key2] AND [j].[CompositeId3] = [e].[Key3]
    WHERE [e].[Key1] < 5
) AS [s] ON [r].[Id] = [s].[LeafId]
INNER JOIN (
    SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3]
    FROM [EntityCompositeKeyEntityTwo] AS [e0]
    INNER JOIN [EntityTwos] AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
) AS [s0] ON [s].[Key1] = [s0].[CompositeKeySkipSharedKey1] AND [s].[Key2] = [s0].[CompositeKeySkipSharedKey2] AND [s].[Key3] = [s0].[CompositeKeySkipSharedKey3]
ORDER BY [r].[Id], [s].[LeafId], [s].[CompositeId1], [s].[CompositeId2], [s].[CompositeId3], [s].[Key1], [s].[Key2], [s].[Key3]
""");
    }

    public override async Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where_split(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where_split(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name]
FROM [EntityOnes] AS [e]
ORDER BY [e].[Id]
""",
            //
            """
SELECT [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[ReferenceInverseId], [e].[Id], [s0].[OneId], [s0].[TwoId]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId], [s].[OneId], [s].[TwoId]
    FROM (
        SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], [j].[OneId], [j].[TwoId], ROW_NUMBER() OVER(PARTITION BY [j].[OneId] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinOneToTwo] AS [j]
        INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    ) AS [s]
    WHERE 1 < [s].[row] AND [s].[row] <= 3
) AS [s0] ON [e].[Id] = [s0].[OneId]
ORDER BY [e].[Id], [s0].[OneId], [s0].[Id], [s0].[TwoId]
""",
            //
            """
SELECT [s1].[Id], [s1].[CollectionInverseId], [s1].[Name], [s1].[ReferenceInverseId], [e].[Id], [s0].[OneId], [s0].[TwoId], [s0].[Id]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [s].[Id], [s].[OneId], [s].[TwoId]
    FROM (
        SELECT [e0].[Id], [j].[OneId], [j].[TwoId], ROW_NUMBER() OVER(PARTITION BY [j].[OneId] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinOneToTwo] AS [j]
        INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    ) AS [s]
    WHERE 1 < [s].[row] AND [s].[row] <= 3
) AS [s0] ON [e].[Id] = [s0].[OneId]
INNER JOIN (
    SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], [j0].[TwoId]
    FROM [JoinTwoToThree] AS [j0]
    INNER JOIN [EntityThrees] AS [e1] ON [j0].[ThreeId] = [e1].[Id]
    WHERE [e1].[Id] < 10
) AS [s1] ON [s0].[Id] = [s1].[TwoId]
ORDER BY [e].[Id], [s0].[OneId], [s0].[Id], [s0].[TwoId]
""");
    }

    public override async Task Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take_split(bool async)
    {
        await base.Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take_split(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name]
FROM [EntityOnes] AS [e]
ORDER BY [e].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId], [e].[Id], [s].[OneId], [s].[TwoId]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], [j].[OneId], [j].[TwoId]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [s] ON [e].[Id] = [s].[OneId]
ORDER BY [e].[Id], [s].[OneId], [s].[TwoId], [s].[Id]
""",
            //
            """
SELECT [s1].[Id], [s1].[CollectionInverseId], [s1].[Name], [s1].[ReferenceInverseId], [e].[Id], [s].[OneId], [s].[TwoId], [s].[Id]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [j].[OneId], [j].[TwoId]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [s] ON [e].[Id] = [s].[OneId]
INNER JOIN (
    SELECT [s0].[Id], [s0].[CollectionInverseId], [s0].[Name], [s0].[ReferenceInverseId], [s0].[TwoId]
    FROM (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], [j0].[TwoId], ROW_NUMBER() OVER(PARTITION BY [j0].[TwoId] ORDER BY [e1].[Id]) AS [row]
        FROM [JoinTwoToThree] AS [j0]
        INNER JOIN [EntityThrees] AS [e1] ON [j0].[ThreeId] = [e1].[Id]
    ) AS [s0]
    WHERE 1 < [s0].[row] AND [s0].[row] <= 3
) AS [s1] ON [s].[Id] = [s1].[TwoId]
ORDER BY [e].[Id], [s].[OneId], [s].[TwoId], [s].[Id], [s1].[TwoId], [s1].[Id]
""");
    }

    public override async Task Filter_include_on_skip_navigation_combined_split(bool async)
    {
        await base.Filter_include_on_skip_navigation_combined_split(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId]
FROM [EntityTwos] AS [e]
ORDER BY [e].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Name], [s].[Id0], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name0], [s].[ReferenceInverseId], [e].[Id], [s].[OneId], [s].[TwoId]
FROM [EntityTwos] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId], [j].[OneId], [j].[TwoId]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
    WHERE [e0].[Id] < 10
) AS [s] ON [e].[Id] = [s].[TwoId]
ORDER BY [e].[Id], [s].[OneId], [s].[TwoId], [s].[Id], [s].[Id0]
""",
            //
            """
SELECT [e2].[Id], [e2].[CollectionInverseId], [e2].[ExtraId], [e2].[Name], [e2].[ReferenceInverseId], [e].[Id], [s].[OneId], [s].[TwoId], [s].[Id], [s].[Id0]
FROM [EntityTwos] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e1].[Id] AS [Id0], [j].[OneId], [j].[TwoId]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
    WHERE [e0].[Id] < 10
) AS [s] ON [e].[Id] = [s].[TwoId]
INNER JOIN [EntityTwos] AS [e2] ON [s].[Id] = [e2].[CollectionInverseId]
ORDER BY [e].[Id], [s].[OneId], [s].[TwoId], [s].[Id], [s].[Id0]
""");
    }

    public override async Task Filter_include_on_skip_navigation_combined_with_filtered_then_includes_split(bool async)
    {
        await base.Filter_include_on_skip_navigation_combined_with_filtered_then_includes_split(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId]
FROM [EntityThrees] AS [e]
ORDER BY [e].[Id]
""",
            //
            """
SELECT [s0].[Id], [s0].[Name], [e].[Id], [s0].[OneId], [s0].[ThreeId]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e0].[Name], [j].[OneId], [j].[ThreeId]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [s0] ON [e].[Id] = [s0].[ThreeId]
ORDER BY [e].[Id], [s0].[OneId], [s0].[ThreeId], [s0].[Id]
""",
            //
            """
SELECT [s2].[Id], [s2].[CollectionInverseId], [s2].[ExtraId], [s2].[Name], [s2].[ReferenceInverseId], [e].[Id], [s0].[OneId], [s0].[ThreeId], [s0].[Id]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [j].[OneId], [j].[ThreeId]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [s0] ON [e].[Id] = [s0].[ThreeId]
INNER JOIN (
    SELECT [s1].[Id], [s1].[CollectionInverseId], [s1].[ExtraId], [s1].[Name], [s1].[ReferenceInverseId], [s1].[OneId]
    FROM (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], [j0].[OneId], ROW_NUMBER() OVER(PARTITION BY [j0].[OneId] ORDER BY [e1].[Id]) AS [row]
        FROM [JoinOneToTwo] AS [j0]
        INNER JOIN [EntityTwos] AS [e1] ON [j0].[TwoId] = [e1].[Id]
    ) AS [s1]
    WHERE 1 < [s1].[row] AND [s1].[row] <= 3
) AS [s2] ON [s0].[Id] = [s2].[OneId]
ORDER BY [e].[Id], [s0].[OneId], [s0].[ThreeId], [s0].[Id], [s2].[OneId], [s2].[Id]
""",
            //
            """
SELECT [s3].[Id], [s3].[Name], [s3].[Number], [s3].[IsGreen], [s3].[Discriminator], [e].[Id], [s0].[OneId], [s0].[ThreeId], [s0].[Id]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [j].[OneId], [j].[ThreeId]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [s0] ON [e].[Id] = [s0].[ThreeId]
INNER JOIN (
    SELECT [s].[Id], [s].[Name], [s].[Number], [s].[IsGreen], [s].[Discriminator], [j1].[EntityOneId]
    FROM [JoinOneToBranch] AS [j1]
    INNER JOIN (
        SELECT [r].[Id], [r].[Name], [b].[Number], [l].[IsGreen], CASE
            WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
        END AS [Discriminator]
        FROM [Roots] AS [r]
        INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
        LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
    ) AS [s] ON [j1].[EntityBranchId] = [s].[Id]
    WHERE [s].[Id] < 20
) AS [s3] ON [s0].[Id] = [s3].[EntityOneId]
ORDER BY [e].[Id], [s0].[OneId], [s0].[ThreeId], [s0].[Id]
""");
    }

    public override async Task Filtered_include_on_skip_navigation_then_filtered_include_on_navigation_split(bool async)
    {
        await base.Filtered_include_on_skip_navigation_then_filtered_include_on_navigation_split(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId]
FROM [EntityThrees] AS [e]
ORDER BY [e].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Name], [e].[Id], [s].[OneId], [s].[ThreeId]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e0].[Name], [j].[OneId], [j].[ThreeId]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e0].[Id] > 15
) AS [s] ON [e].[Id] = [s].[ThreeId]
ORDER BY [e].[Id], [s].[OneId], [s].[ThreeId], [s].[Id]
""",
            //
            """
SELECT [e2].[Id], [e2].[CollectionInverseId], [e2].[ExtraId], [e2].[Name], [e2].[ReferenceInverseId], [e].[Id], [s].[OneId], [s].[ThreeId], [s].[Id]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [j].[OneId], [j].[ThreeId]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e0].[Id] > 15
) AS [s] ON [e].[Id] = [s].[ThreeId]
INNER JOIN (
    SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId]
    FROM [EntityTwos] AS [e1]
    WHERE [e1].[Id] < 5
) AS [e2] ON [s].[Id] = [e2].[CollectionInverseId]
ORDER BY [e].[Id], [s].[OneId], [s].[ThreeId], [s].[Id]
""");
    }

    public override async Task Filtered_include_on_navigation_then_filtered_include_on_skip_navigation_split(bool async)
    {
        await base.Filtered_include_on_navigation_then_filtered_include_on_skip_navigation_split(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name]
FROM [EntityOnes] AS [e]
ORDER BY [e].[Id]
""",
            //
            """
SELECT [e2].[Id], [e2].[CollectionInverseId], [e2].[ExtraId], [e2].[Name], [e2].[ReferenceInverseId], [e].[Id]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId]
    FROM [EntityTwos] AS [e0]
    WHERE [e0].[Id] > 15
) AS [e2] ON [e].[Id] = [e2].[CollectionInverseId]
ORDER BY [e].[Id], [e2].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[CollectionInverseId], [s].[Name], [s].[ReferenceInverseId], [e].[Id], [e2].[Id]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId]
    FROM [EntityTwos] AS [e0]
    WHERE [e0].[Id] > 15
) AS [e2] ON [e].[Id] = [e2].[CollectionInverseId]
INNER JOIN (
    SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], [j].[TwoId]
    FROM [JoinTwoToThree] AS [j]
    INNER JOIN [EntityThrees] AS [e1] ON [j].[ThreeId] = [e1].[Id]
    WHERE [e1].[Id] < 5
) AS [s] ON [e2].[Id] = [s].[TwoId]
ORDER BY [e].[Id], [e2].[Id]
""");
    }

    public override async Task Include_skip_navigation_then_include_inverse_throws_in_no_tracking(bool async)
    {
        await base.Include_skip_navigation_then_include_inverse_throws_in_no_tracking(async);

        AssertSql();
    }

    public override async Task Include_skip_navigation_then_include_inverse_works_for_tracking_query(bool async)
    {
        await base.Include_skip_navigation_then_include_inverse_works_for_tracking_query(async);

        AssertSql();
    }

    public override async Task Throws_when_different_filtered_include(bool async)
    {
        await base.Throws_when_different_filtered_include(async);

        AssertSql();
    }

    public override async Task Throws_when_different_filtered_then_include(bool async)
    {
        await base.Throws_when_different_filtered_then_include(async);

        AssertSql();
    }

    public override async Task Throws_when_different_filtered_then_include_via_different_paths(bool async)
    {
        await base.Throws_when_different_filtered_then_include_via_different_paths(async);

        AssertSql();
    }

    public override async Task Select_many_over_skip_navigation_where_non_equality(bool async)
    {
        await base.Select_many_over_skip_navigation_where_non_equality(async);

        AssertSql(
            """
SELECT [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId]
FROM [EntityOnes] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], [j].[OneId]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
) AS [s] ON [e].[Id] = [s].[OneId] AND [e].[Id] <> [s].[Id]
""");
    }

    public override async Task Contains_on_skip_collection_navigation(bool async)
    {
        await base.Contains_on_skip_collection_navigation(async);

        AssertSql(
            """
@__entity_equality_two_0_Id='1' (Nullable = true)

SELECT [e].[Id], [e].[Name]
FROM [EntityOnes] AS [e]
WHERE EXISTS (
    SELECT 1
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    WHERE [e].[Id] = [j].[OneId] AND [e0].[Id] = @__entity_equality_two_0_Id)
""");
    }

    public override async Task GetType_in_hierarchy_in_base_type(bool async)
    {
        await base.GetType_in_hierarchy_in_base_type(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [b].[Number], [b0].[Slumber], [l].[IsGreen], [l0].[IsBrown], CASE
    WHEN [l0].[Id] IS NOT NULL THEN N'EntityLeaf2'
    WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
    WHEN [b].[Id] IS NOT NULL THEN N'EntityBranch'
END AS [Discriminator]
FROM [Roots] AS [r]
LEFT JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
LEFT JOIN [Branch2s] AS [b0] ON [r].[Id] = [b0].[Id]
LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
LEFT JOIN [Leaf2s] AS [l0] ON [r].[Id] = [l0].[Id]
WHERE [l0].[Id] IS NULL AND [l].[Id] IS NULL AND [b].[Id] IS NULL
""");
    }

    public override async Task GetType_in_hierarchy_in_intermediate_type(bool async)
    {
        await base.GetType_in_hierarchy_in_intermediate_type(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [b].[Number], [b0].[Slumber], [l].[IsGreen], [l0].[IsBrown], CASE
    WHEN [l0].[Id] IS NOT NULL THEN N'EntityLeaf2'
    WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
    WHEN [b].[Id] IS NOT NULL THEN N'EntityBranch'
END AS [Discriminator]
FROM [Roots] AS [r]
LEFT JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
LEFT JOIN [Branch2s] AS [b0] ON [r].[Id] = [b0].[Id]
LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
LEFT JOIN [Leaf2s] AS [l0] ON [r].[Id] = [l0].[Id]
WHERE [l].[Id] IS NULL AND [b].[Id] IS NOT NULL
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type(bool async)
    {
        await base.GetType_in_hierarchy_in_leaf_type(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [b].[Number], [b0].[Slumber], [l].[IsGreen], [l0].[IsBrown], CASE
    WHEN [l0].[Id] IS NOT NULL THEN N'EntityLeaf2'
    WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
    WHEN [b].[Id] IS NOT NULL THEN N'EntityBranch'
END AS [Discriminator]
FROM [Roots] AS [r]
LEFT JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
LEFT JOIN [Branch2s] AS [b0] ON [r].[Id] = [b0].[Id]
LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
LEFT JOIN [Leaf2s] AS [l0] ON [r].[Id] = [l0].[Id]
WHERE [l].[Id] IS NOT NULL
""");
    }

    public override async Task GetType_in_hierarchy_in_querying_base_type(bool async)
    {
        await base.GetType_in_hierarchy_in_querying_base_type(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [b].[Number], [l].[IsGreen], CASE
    WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
END AS [Discriminator]
FROM [Roots] AS [r]
INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
WHERE 0 = 1
""");
    }

    public override async Task Filtered_include_skip_navigation_order_by_take_EF_Property(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_take_EF_Property(async);

        AssertSql(
            """
SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[ReferenceInverseId], [s0].[TwoSkipSharedId], [s0].[CompositeKeySkipSharedKey1], [s0].[CompositeKeySkipSharedKey2], [s0].[CompositeKeySkipSharedKey3]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId], [s].[TwoSkipSharedId], [s].[CompositeKeySkipSharedKey1], [s].[CompositeKeySkipSharedKey2], [s].[CompositeKeySkipSharedKey3]
    FROM (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], [e0].[TwoSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], ROW_NUMBER() OVER(PARTITION BY [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3] ORDER BY [e1].[Id]) AS [row]
        FROM [EntityCompositeKeyEntityTwo] AS [e0]
        INNER JOIN [EntityTwos] AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
    ) AS [s]
    WHERE [s].[row] <= 2
) AS [s0] ON [e].[Key1] = [s0].[CompositeKeySkipSharedKey1] AND [e].[Key2] = [s0].[CompositeKeySkipSharedKey2] AND [e].[Key3] = [s0].[CompositeKeySkipSharedKey3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [s0].[CompositeKeySkipSharedKey1], [s0].[CompositeKeySkipSharedKey2], [s0].[CompositeKeySkipSharedKey3], [s0].[Id]
""");
    }

    public override async Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where_EF_Property(
        bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where_EF_Property(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], [s1].[Id], [s1].[CollectionInverseId], [s1].[ExtraId], [s1].[Name], [s1].[ReferenceInverseId], [s1].[OneId], [s1].[TwoId], [s1].[Id0], [s1].[CollectionInverseId0], [s1].[Name0], [s1].[ReferenceInverseId0], [s1].[ThreeId], [s1].[TwoId0]
FROM [EntityOnes] AS [e]
OUTER APPLY (
    SELECT [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId], [s].[OneId], [s].[TwoId], [s0].[Id] AS [Id0], [s0].[CollectionInverseId] AS [CollectionInverseId0], [s0].[Name] AS [Name0], [s0].[ReferenceInverseId] AS [ReferenceInverseId0], [s0].[ThreeId], [s0].[TwoId] AS [TwoId0]
    FROM (
        SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], [j].[OneId], [j].[TwoId]
        FROM [JoinOneToTwo] AS [j]
        INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
        WHERE [e].[Id] = [j].[OneId]
        ORDER BY [e0].[Id]
        OFFSET 1 ROWS FETCH NEXT 2 ROWS ONLY
    ) AS [s]
    LEFT JOIN (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], [j0].[ThreeId], [j0].[TwoId]
        FROM [JoinTwoToThree] AS [j0]
        INNER JOIN [EntityThrees] AS [e1] ON [j0].[ThreeId] = [e1].[Id]
        WHERE [e1].[Id] < 10
    ) AS [s0] ON [s].[Id] = [s0].[TwoId]
) AS [s1]
ORDER BY [e].[Id], [s1].[Id], [s1].[OneId], [s1].[TwoId], [s1].[ThreeId], [s1].[TwoId0]
""");
    }

    public override async Task Include_skip_navigation_then_include_inverse_works_for_tracking_query_unidirectional(bool async)
    {
        await base.Include_skip_navigation_then_include_inverse_works_for_tracking_query_unidirectional(async);

        AssertSql();
    }

    public override async Task Skip_navigation_all_unidirectional(bool async)
    {
        await base.Skip_navigation_all_unidirectional(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name]
FROM [UnidirectionalEntityOnes] AS [u]
WHERE NOT EXISTS (
    SELECT 1
    FROM [UnidirectionalJoinOneToTwo] AS [u0]
    INNER JOIN [UnidirectionalEntityTwos] AS [u1] ON [u0].[TwoId] = [u1].[Id]
    WHERE [u].[Id] = [u0].[OneId] AND ([u1].[Name] NOT LIKE N'%B%' OR [u1].[Name] IS NULL))
""");
    }

    public override async Task Skip_navigation_any_with_predicate_unidirectional(bool async)
    {
        await base.Skip_navigation_any_with_predicate_unidirectional(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name]
FROM [UnidirectionalEntityOnes] AS [u]
WHERE EXISTS (
    SELECT 1
    FROM [UnidirectionalEntityOneUnidirectionalEntityTwo] AS [u0]
    INNER JOIN [UnidirectionalEntityTwos] AS [u1] ON [u0].[TwoSkipSharedId] = [u1].[Id]
    WHERE [u].[Id] = [u0].[UnidirectionalEntityOneId] AND [u1].[Name] LIKE N'%B%')
""");
    }

    public override async Task Skip_navigation_contains_unidirectional(bool async)
    {
        await base.Skip_navigation_contains_unidirectional(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name]
FROM [UnidirectionalEntityOnes] AS [u]
WHERE EXISTS (
    SELECT 1
    FROM [UnidirectionalJoinOneToThreePayloadFullShared] AS [u0]
    INNER JOIN [UnidirectionalEntityThrees] AS [u1] ON [u0].[ThreeId] = [u1].[Id]
    WHERE [u].[Id] = [u0].[OneId] AND [u1].[Id] = 1)
""");
    }

    public override async Task Skip_navigation_count_without_predicate_unidirectional(bool async)
    {
        await base.Skip_navigation_count_without_predicate_unidirectional(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name]
FROM [UnidirectionalEntityOnes] AS [u]
WHERE (
    SELECT COUNT(*)
    FROM [UnidirectionalJoinOneSelfPayload] AS [u0]
    INNER JOIN [UnidirectionalEntityOnes] AS [u1] ON [u0].[LeftId] = [u1].[Id]
    WHERE [u].[Id] = [u0].[RightId]) > 0
""");
    }

    public override async Task Skip_navigation_count_with_predicate_unidirectional(bool async)
    {
        await base.Skip_navigation_count_with_predicate_unidirectional(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name]
FROM [UnidirectionalEntityOnes] AS [u]
ORDER BY (
    SELECT COUNT(*)
    FROM [UnidirectionalJoinOneToBranch] AS [u0]
    INNER JOIN (
        SELECT [u1].[Id], [u1].[Name]
        FROM [UnidirectionalRoots] AS [u1]
        INNER JOIN [UnidirectionalBranches] AS [u2] ON [u1].[Id] = [u2].[Id]
    ) AS [s] ON [u0].[UnidirectionalEntityBranchId] = [s].[Id]
    WHERE [u].[Id] = [u0].[UnidirectionalEntityOneId] AND [s].[Name] LIKE N'L%'), [u].[Id]
""");
    }

    public override async Task Skip_navigation_select_subquery_average_unidirectional(bool async)
    {
        await base.Skip_navigation_select_subquery_average_unidirectional(async);

        AssertSql(
            """
SELECT (
    SELECT AVG(CAST([u3].[Key1] AS float))
    FROM [UnidirectionalJoinCompositeKeyToLeaf] AS [u2]
    INNER JOIN [UnidirectionalEntityCompositeKeys] AS [u3] ON [u2].[CompositeId1] = [u3].[Key1] AND [u2].[CompositeId2] = [u3].[Key2] AND [u2].[CompositeId3] = [u3].[Key3]
    WHERE [u].[Id] = [u2].[LeafId])
FROM [UnidirectionalRoots] AS [u]
INNER JOIN [UnidirectionalBranches] AS [u0] ON [u].[Id] = [u0].[Id]
INNER JOIN [UnidirectionalLeaves] AS [u1] ON [u].[Id] = [u1].[Id]
""");
    }

    public override async Task Skip_navigation_order_by_reverse_first_or_default_unidirectional(bool async)
    {
        await base.Skip_navigation_order_by_reverse_first_or_default_unidirectional(async);

        AssertSql(
            """
SELECT [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[ReferenceInverseId]
FROM [UnidirectionalEntityThrees] AS [u]
LEFT JOIN (
    SELECT [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId], [s].[ThreeId]
    FROM (
        SELECT [u1].[Id], [u1].[CollectionInverseId], [u1].[ExtraId], [u1].[Name], [u1].[ReferenceInverseId], [u0].[ThreeId], ROW_NUMBER() OVER(PARTITION BY [u0].[ThreeId] ORDER BY [u1].[Id] DESC) AS [row]
        FROM [UnidirectionalJoinTwoToThree] AS [u0]
        INNER JOIN [UnidirectionalEntityTwos] AS [u1] ON [u0].[TwoId] = [u1].[Id]
    ) AS [s]
    WHERE [s].[row] <= 1
) AS [s0] ON [u].[Id] = [s0].[ThreeId]
""");
    }

    public override async Task Skip_navigation_of_type_unidirectional(bool async)
    {
        await base.Skip_navigation_of_type_unidirectional(async);

        AssertSql(
            """
SELECT [u].[Key1], [u].[Key2], [u].[Key3], [s0].[Id], [s0].[Name], [s0].[Number], [s0].[IsGreen], [s0].[Discriminator], [s0].[RootSkipSharedId], [s0].[UnidirectionalEntityCompositeKeyKey1], [s0].[UnidirectionalEntityCompositeKeyKey2], [s0].[UnidirectionalEntityCompositeKeyKey3]
FROM [UnidirectionalEntityCompositeKeys] AS [u]
LEFT JOIN (
    SELECT [s].[Id], [s].[Name], [s].[Number], [s].[IsGreen], [s].[Discriminator], [u0].[RootSkipSharedId], [u0].[UnidirectionalEntityCompositeKeyKey1], [u0].[UnidirectionalEntityCompositeKeyKey2], [u0].[UnidirectionalEntityCompositeKeyKey3]
    FROM [UnidirectionalEntityCompositeKeyUnidirectionalEntityRoot] AS [u0]
    INNER JOIN (
        SELECT [u1].[Id], [u1].[Name], [u2].[Number], [u3].[IsGreen], CASE
            WHEN [u3].[Id] IS NOT NULL THEN N'UnidirectionalEntityLeaf'
            WHEN [u2].[Id] IS NOT NULL THEN N'UnidirectionalEntityBranch'
        END AS [Discriminator]
        FROM [UnidirectionalRoots] AS [u1]
        LEFT JOIN [UnidirectionalBranches] AS [u2] ON [u1].[Id] = [u2].[Id]
        LEFT JOIN [UnidirectionalLeaves] AS [u3] ON [u1].[Id] = [u3].[Id]
    ) AS [s] ON [u0].[RootSkipSharedId] = [s].[Id]
    WHERE [s].[Discriminator] = N'UnidirectionalEntityLeaf'
) AS [s0] ON [u].[Key1] = [s0].[UnidirectionalEntityCompositeKeyKey1] AND [u].[Key2] = [s0].[UnidirectionalEntityCompositeKeyKey2] AND [u].[Key3] = [s0].[UnidirectionalEntityCompositeKeyKey3]
ORDER BY [u].[Key1], [u].[Key2], [u].[Key3], [s0].[RootSkipSharedId], [s0].[UnidirectionalEntityCompositeKeyKey1], [s0].[UnidirectionalEntityCompositeKeyKey2], [s0].[UnidirectionalEntityCompositeKeyKey3]
""");
    }

    public override async Task Join_with_skip_navigation_unidirectional(bool async)
    {
        await base.Join_with_skip_navigation_unidirectional(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[CollectionInverseId], [u].[ExtraId], [u].[Name], [u].[ReferenceInverseId], [u0].[Id], [u0].[CollectionInverseId], [u0].[ExtraId], [u0].[Name], [u0].[ReferenceInverseId]
FROM [UnidirectionalEntityTwos] AS [u]
INNER JOIN [UnidirectionalEntityTwos] AS [u0] ON [u].[Id] = (
    SELECT TOP(1) [u2].[Id]
    FROM [UnidirectionalEntityTwoUnidirectionalEntityTwo] AS [u1]
    INNER JOIN [UnidirectionalEntityTwos] AS [u2] ON [u1].[SelfSkipSharedRightId] = [u2].[Id]
    WHERE [u0].[Id] = [u1].[UnidirectionalEntityTwoId]
    ORDER BY [u2].[Id])
""");
    }

    public override async Task Left_join_with_skip_navigation_unidirectional(bool async)
    {
        await base.Left_join_with_skip_navigation_unidirectional(async);

        AssertSql(
            """
SELECT [u].[Key1], [u].[Key2], [u].[Key3], [u].[Name], [u0].[Key1], [u0].[Key2], [u0].[Key3], [u0].[Name]
FROM [UnidirectionalEntityCompositeKeys] AS [u]
LEFT JOIN [UnidirectionalEntityCompositeKeys] AS [u0] ON (
    SELECT TOP(1) [u2].[Id]
    FROM [UnidirectionalEntityCompositeKeyUnidirectionalEntityTwo] AS [u1]
    INNER JOIN [UnidirectionalEntityTwos] AS [u2] ON [u1].[TwoSkipSharedId] = [u2].[Id]
    WHERE [u].[Key1] = [u1].[UnidirectionalEntityCompositeKeyKey1] AND [u].[Key2] = [u1].[UnidirectionalEntityCompositeKeyKey2] AND [u].[Key3] = [u1].[UnidirectionalEntityCompositeKeyKey3]
    ORDER BY [u2].[Id]) = (
    SELECT TOP(1) [u4].[Id]
    FROM [UnidirectionalJoinThreeToCompositeKeyFull] AS [u3]
    INNER JOIN [UnidirectionalEntityThrees] AS [u4] ON [u3].[ThreeId] = [u4].[Id]
    WHERE [u0].[Key1] = [u3].[CompositeId1] AND [u0].[Key2] = [u3].[CompositeId2] AND [u0].[Key3] = [u3].[CompositeId3]
    ORDER BY [u4].[Id])
ORDER BY [u].[Key1], [u0].[Key1], [u].[Key2], [u0].[Key2]
""");
    }

    public override async Task Select_many_over_skip_navigation_unidirectional(bool async)
    {
        await base.Select_many_over_skip_navigation_unidirectional(async);

        AssertSql(
            """
SELECT [s].[Id], [s].[CollectionInverseId], [s].[Name], [s].[ReferenceInverseId]
FROM [UnidirectionalRoots] AS [u]
INNER JOIN (
    SELECT [u1].[Id], [u1].[CollectionInverseId], [u1].[Name], [u1].[ReferenceInverseId], [u0].[UnidirectionalEntityRootId]
    FROM [UnidirectionalEntityRootUnidirectionalEntityThree] AS [u0]
    INNER JOIN [UnidirectionalEntityThrees] AS [u1] ON [u0].[ThreeSkipSharedId] = [u1].[Id]
) AS [s] ON [u].[Id] = [s].[UnidirectionalEntityRootId]
""");
    }

    public override async Task Select_many_over_skip_navigation_where_unidirectional(bool async)
    {
        await base.Select_many_over_skip_navigation_where_unidirectional(async);

        AssertSql(
            """
SELECT [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId]
FROM [UnidirectionalEntityOnes] AS [u]
LEFT JOIN (
    SELECT [u1].[Id], [u1].[CollectionInverseId], [u1].[ExtraId], [u1].[Name], [u1].[ReferenceInverseId], [u0].[OneId]
    FROM [UnidirectionalJoinOneToTwo] AS [u0]
    INNER JOIN [UnidirectionalEntityTwos] AS [u1] ON [u0].[TwoId] = [u1].[Id]
) AS [s] ON [u].[Id] = [s].[OneId]
""");
    }

    public override async Task Select_many_over_skip_navigation_order_by_take_unidirectional(bool async)
    {
        await base.Select_many_over_skip_navigation_order_by_take_unidirectional(async);

        AssertSql(
            """
SELECT [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[ReferenceInverseId]
FROM [UnidirectionalEntityOnes] AS [u]
INNER JOIN (
    SELECT [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId], [s].[UnidirectionalEntityOneId]
    FROM (
        SELECT [u1].[Id], [u1].[CollectionInverseId], [u1].[ExtraId], [u1].[Name], [u1].[ReferenceInverseId], [u0].[UnidirectionalEntityOneId], ROW_NUMBER() OVER(PARTITION BY [u0].[UnidirectionalEntityOneId] ORDER BY [u1].[Id]) AS [row]
        FROM [UnidirectionalEntityOneUnidirectionalEntityTwo] AS [u0]
        INNER JOIN [UnidirectionalEntityTwos] AS [u1] ON [u0].[TwoSkipSharedId] = [u1].[Id]
    ) AS [s]
    WHERE [s].[row] <= 2
) AS [s0] ON [u].[Id] = [s0].[UnidirectionalEntityOneId]
""");
    }

    public override async Task Select_many_over_skip_navigation_order_by_skip_take_unidirectional(bool async)
    {
        await base.Select_many_over_skip_navigation_order_by_skip_take_unidirectional(async);

        AssertSql(
            """
SELECT [s0].[Id], [s0].[CollectionInverseId], [s0].[Name], [s0].[ReferenceInverseId]
FROM [UnidirectionalEntityOnes] AS [u]
INNER JOIN (
    SELECT [s].[Id], [s].[CollectionInverseId], [s].[Name], [s].[ReferenceInverseId], [s].[OneId]
    FROM (
        SELECT [u1].[Id], [u1].[CollectionInverseId], [u1].[Name], [u1].[ReferenceInverseId], [u0].[OneId], ROW_NUMBER() OVER(PARTITION BY [u0].[OneId] ORDER BY [u1].[Id]) AS [row]
        FROM [UnidirectionalJoinOneToThreePayloadFullShared] AS [u0]
        INNER JOIN [UnidirectionalEntityThrees] AS [u1] ON [u0].[ThreeId] = [u1].[Id]
    ) AS [s]
    WHERE 2 < [s].[row] AND [s].[row] <= 5
) AS [s0] ON [u].[Id] = [s0].[OneId]
""");
    }

    public override async Task Select_many_over_skip_navigation_cast_unidirectional(bool async)
    {
        await base.Select_many_over_skip_navigation_cast_unidirectional(async);

        AssertSql(
            """
SELECT [s0].[Id], [s0].[Name], [s0].[Number], [s0].[IsGreen], [s0].[Discriminator]
FROM [UnidirectionalEntityOnes] AS [u]
INNER JOIN (
    SELECT [s].[Id], [s].[Name], [s].[Number], [s].[IsGreen], [s].[Discriminator], [u0].[UnidirectionalEntityOneId]
    FROM [UnidirectionalJoinOneToBranch] AS [u0]
    INNER JOIN (
        SELECT [u1].[Id], [u1].[Name], [u2].[Number], [u3].[IsGreen], CASE
            WHEN [u3].[Id] IS NOT NULL THEN N'UnidirectionalEntityLeaf'
        END AS [Discriminator]
        FROM [UnidirectionalRoots] AS [u1]
        INNER JOIN [UnidirectionalBranches] AS [u2] ON [u1].[Id] = [u2].[Id]
        LEFT JOIN [UnidirectionalLeaves] AS [u3] ON [u1].[Id] = [u3].[Id]
    ) AS [s] ON [u0].[UnidirectionalEntityBranchId] = [s].[Id]
) AS [s0] ON [u].[Id] = [s0].[UnidirectionalEntityOneId]
""");
    }

    public override async Task Select_skip_navigation_unidirectional(bool async)
    {
        await base.Select_skip_navigation_unidirectional(async);

        AssertSql(
            """
SELECT [u].[Id], [s].[Id], [s].[Name], [s].[LeftId], [s].[RightId]
FROM [UnidirectionalEntityOnes] AS [u]
LEFT JOIN (
    SELECT [u1].[Id], [u1].[Name], [u0].[LeftId], [u0].[RightId]
    FROM [UnidirectionalJoinOneSelfPayload] AS [u0]
    INNER JOIN [UnidirectionalEntityOnes] AS [u1] ON [u0].[LeftId] = [u1].[Id]
) AS [s] ON [u].[Id] = [s].[RightId]
ORDER BY [u].[Id], [s].[LeftId], [s].[RightId]
""");
    }

    public override async Task Include_skip_navigation_unidirectional(bool async)
    {
        await base.Include_skip_navigation_unidirectional(async);

        AssertSql(
            """
SELECT [u].[Key1], [u].[Key2], [u].[Key3], [u].[Name], [s0].[Id], [s0].[Name], [s0].[Number], [s0].[IsGreen], [s0].[Discriminator], [s0].[RootSkipSharedId], [s0].[UnidirectionalEntityCompositeKeyKey1], [s0].[UnidirectionalEntityCompositeKeyKey2], [s0].[UnidirectionalEntityCompositeKeyKey3]
FROM [UnidirectionalEntityCompositeKeys] AS [u]
LEFT JOIN (
    SELECT [s].[Id], [s].[Name], [s].[Number], [s].[IsGreen], [s].[Discriminator], [u0].[RootSkipSharedId], [u0].[UnidirectionalEntityCompositeKeyKey1], [u0].[UnidirectionalEntityCompositeKeyKey2], [u0].[UnidirectionalEntityCompositeKeyKey3]
    FROM [UnidirectionalEntityCompositeKeyUnidirectionalEntityRoot] AS [u0]
    INNER JOIN (
        SELECT [u1].[Id], [u1].[Name], [u2].[Number], [u3].[IsGreen], CASE
            WHEN [u3].[Id] IS NOT NULL THEN N'UnidirectionalEntityLeaf'
            WHEN [u2].[Id] IS NOT NULL THEN N'UnidirectionalEntityBranch'
        END AS [Discriminator]
        FROM [UnidirectionalRoots] AS [u1]
        LEFT JOIN [UnidirectionalBranches] AS [u2] ON [u1].[Id] = [u2].[Id]
        LEFT JOIN [UnidirectionalLeaves] AS [u3] ON [u1].[Id] = [u3].[Id]
    ) AS [s] ON [u0].[RootSkipSharedId] = [s].[Id]
) AS [s0] ON [u].[Key1] = [s0].[UnidirectionalEntityCompositeKeyKey1] AND [u].[Key2] = [s0].[UnidirectionalEntityCompositeKeyKey2] AND [u].[Key3] = [s0].[UnidirectionalEntityCompositeKeyKey3]
ORDER BY [u].[Key1], [u].[Key2], [u].[Key3], [s0].[RootSkipSharedId], [s0].[UnidirectionalEntityCompositeKeyKey1], [s0].[UnidirectionalEntityCompositeKeyKey2], [s0].[UnidirectionalEntityCompositeKeyKey3]
""");
    }

    public override async Task Include_skip_navigation_then_reference_unidirectional(bool async)
    {
        await base.Include_skip_navigation_then_reference_unidirectional(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[CollectionInverseId], [u].[ExtraId], [u].[Name], [u].[ReferenceInverseId], [s].[Id], [s].[Name], [s].[Id0], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name0], [s].[ReferenceInverseId], [s].[OneId], [s].[TwoId]
FROM [UnidirectionalEntityTwos] AS [u]
LEFT JOIN (
    SELECT [u1].[Id], [u1].[Name], [u2].[Id] AS [Id0], [u2].[CollectionInverseId], [u2].[ExtraId], [u2].[Name] AS [Name0], [u2].[ReferenceInverseId], [u0].[OneId], [u0].[TwoId]
    FROM [UnidirectionalJoinOneToTwo] AS [u0]
    INNER JOIN [UnidirectionalEntityOnes] AS [u1] ON [u0].[OneId] = [u1].[Id]
    LEFT JOIN [UnidirectionalEntityTwos] AS [u2] ON [u1].[Id] = [u2].[ReferenceInverseId]
) AS [s] ON [u].[Id] = [s].[TwoId]
ORDER BY [u].[Id], [s].[OneId], [s].[TwoId], [s].[Id]
""");
    }

    public override async Task Include_skip_navigation_then_include_skip_navigation_unidirectional(bool async)
    {
        await base.Include_skip_navigation_then_include_skip_navigation_unidirectional(async);

        AssertSql(
            """
SELECT [u].[Key1], [u].[Key2], [u].[Key3], [u].[Name], [s1].[Id], [s1].[Name], [s1].[Number], [s1].[IsGreen], [s1].[LeafId], [s1].[CompositeId1], [s1].[CompositeId2], [s1].[CompositeId3], [s1].[Id0], [s1].[Name0], [s1].[UnidirectionalEntityBranchId], [s1].[UnidirectionalEntityOneId]
FROM [UnidirectionalEntityCompositeKeys] AS [u]
LEFT JOIN (
    SELECT [s].[Id], [s].[Name], [s].[Number], [s].[IsGreen], [u0].[LeafId], [u0].[CompositeId1], [u0].[CompositeId2], [u0].[CompositeId3], [s0].[Id] AS [Id0], [s0].[Name] AS [Name0], [s0].[UnidirectionalEntityBranchId], [s0].[UnidirectionalEntityOneId]
    FROM [UnidirectionalJoinCompositeKeyToLeaf] AS [u0]
    INNER JOIN (
        SELECT [u1].[Id], [u1].[Name], [u2].[Number], [u3].[IsGreen]
        FROM [UnidirectionalRoots] AS [u1]
        INNER JOIN [UnidirectionalBranches] AS [u2] ON [u1].[Id] = [u2].[Id]
        INNER JOIN [UnidirectionalLeaves] AS [u3] ON [u1].[Id] = [u3].[Id]
    ) AS [s] ON [u0].[LeafId] = [s].[Id]
    LEFT JOIN (
        SELECT [u5].[Id], [u5].[Name], [u4].[UnidirectionalEntityBranchId], [u4].[UnidirectionalEntityOneId]
        FROM [UnidirectionalJoinOneToBranch] AS [u4]
        INNER JOIN [UnidirectionalEntityOnes] AS [u5] ON [u4].[UnidirectionalEntityOneId] = [u5].[Id]
    ) AS [s0] ON [s].[Id] = [s0].[UnidirectionalEntityBranchId]
) AS [s1] ON [u].[Key1] = [s1].[CompositeId1] AND [u].[Key2] = [s1].[CompositeId2] AND [u].[Key3] = [s1].[CompositeId3]
ORDER BY [u].[Key1], [u].[Key2], [u].[Key3], [s1].[LeafId], [s1].[CompositeId1], [s1].[CompositeId2], [s1].[CompositeId3], [s1].[Id], [s1].[UnidirectionalEntityBranchId], [s1].[UnidirectionalEntityOneId]
""");
    }

    public override async Task Include_skip_navigation_then_include_reference_and_skip_navigation_unidirectional(bool async)
    {
        await base.Include_skip_navigation_then_include_reference_and_skip_navigation_unidirectional(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[CollectionInverseId], [u].[Name], [u].[ReferenceInverseId], [s0].[Id], [s0].[Name], [s0].[Id0], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name0], [s0].[ReferenceInverseId], [s0].[OneId], [s0].[ThreeId], [s0].[Id1], [s0].[Name1], [s0].[LeftId], [s0].[RightId]
FROM [UnidirectionalEntityThrees] AS [u]
LEFT JOIN (
    SELECT [u1].[Id], [u1].[Name], [u2].[Id] AS [Id0], [u2].[CollectionInverseId], [u2].[ExtraId], [u2].[Name] AS [Name0], [u2].[ReferenceInverseId], [u0].[OneId], [u0].[ThreeId], [s].[Id] AS [Id1], [s].[Name] AS [Name1], [s].[LeftId], [s].[RightId]
    FROM [UnidirectionalJoinOneToThreePayloadFull] AS [u0]
    INNER JOIN [UnidirectionalEntityOnes] AS [u1] ON [u0].[OneId] = [u1].[Id]
    LEFT JOIN [UnidirectionalEntityTwos] AS [u2] ON [u1].[Id] = [u2].[ReferenceInverseId]
    LEFT JOIN (
        SELECT [u4].[Id], [u4].[Name], [u3].[LeftId], [u3].[RightId]
        FROM [UnidirectionalJoinOneSelfPayload] AS [u3]
        INNER JOIN [UnidirectionalEntityOnes] AS [u4] ON [u3].[RightId] = [u4].[Id]
    ) AS [s] ON [u1].[Id] = [s].[LeftId]
) AS [s0] ON [u].[Id] = [s0].[ThreeId]
ORDER BY [u].[Id], [s0].[OneId], [s0].[ThreeId], [s0].[Id], [s0].[Id0], [s0].[LeftId], [s0].[RightId]
""");
    }

    public override async Task Include_skip_navigation_and_reference_unidirectional(bool async)
    {
        await base.Include_skip_navigation_and_reference_unidirectional(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[CollectionInverseId], [u].[ExtraId], [u].[Name], [u].[ReferenceInverseId], [u0].[Id], [s].[Id], [s].[Name], [s].[TwoSkipSharedId], [s].[UnidirectionalEntityOneId], [u0].[CollectionInverseId], [u0].[Name], [u0].[ReferenceInverseId]
FROM [UnidirectionalEntityTwos] AS [u]
LEFT JOIN [UnidirectionalEntityThrees] AS [u0] ON [u].[Id] = [u0].[ReferenceInverseId]
LEFT JOIN (
    SELECT [u2].[Id], [u2].[Name], [u1].[TwoSkipSharedId], [u1].[UnidirectionalEntityOneId]
    FROM [UnidirectionalEntityOneUnidirectionalEntityTwo] AS [u1]
    INNER JOIN [UnidirectionalEntityOnes] AS [u2] ON [u1].[UnidirectionalEntityOneId] = [u2].[Id]
) AS [s] ON [u].[Id] = [s].[TwoSkipSharedId]
ORDER BY [u].[Id], [u0].[Id], [s].[TwoSkipSharedId], [s].[UnidirectionalEntityOneId]
""");
    }

    public override async Task Filtered_include_skip_navigation_where_unidirectional(bool async)
    {
        await base.Filtered_include_skip_navigation_where_unidirectional(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId], [s].[Id], [s].[Name], [s].[OneId], [s].[ThreeId]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[Name], [j].[OneId], [j].[ThreeId]
    FROM [JoinOneToThreePayloadFullShared] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [s] ON [e].[Id] = [s].[ThreeId]
ORDER BY [e].[Id], [s].[OneId], [s].[ThreeId]
""");
    }

    public override async Task Filtered_include_skip_navigation_order_by_unidirectional(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_unidirectional(async);
        AssertSql(
            """
SELECT [u].[Id], [u].[CollectionInverseId], [u].[Name], [u].[ReferenceInverseId], [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId], [s].[ThreeId], [s].[TwoId]
FROM [UnidirectionalEntityThrees] AS [u]
LEFT JOIN (
    SELECT [u1].[Id], [u1].[CollectionInverseId], [u1].[ExtraId], [u1].[Name], [u1].[ReferenceInverseId], [u0].[ThreeId], [u0].[TwoId]
    FROM [UnidirectionalJoinTwoToThree] AS [u0]
    INNER JOIN [UnidirectionalEntityTwos] AS [u1] ON [u0].[TwoId] = [u1].[Id]
) AS [s] ON [u].[Id] = [s].[ThreeId]
ORDER BY [u].[Id], [s].[Id], [s].[ThreeId]
""");
    }

    public override async Task Filtered_include_skip_navigation_order_by_skip_unidirectional(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip_unidirectional(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[CollectionInverseId], [u].[ExtraId], [u].[Name], [u].[ReferenceInverseId], [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[ReferenceInverseId], [s0].[SelfSkipSharedRightId], [s0].[UnidirectionalEntityTwoId]
FROM [UnidirectionalEntityTwos] AS [u]
LEFT JOIN (
    SELECT [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId], [s].[SelfSkipSharedRightId], [s].[UnidirectionalEntityTwoId]
    FROM (
        SELECT [u1].[Id], [u1].[CollectionInverseId], [u1].[ExtraId], [u1].[Name], [u1].[ReferenceInverseId], [u0].[SelfSkipSharedRightId], [u0].[UnidirectionalEntityTwoId], ROW_NUMBER() OVER(PARTITION BY [u0].[UnidirectionalEntityTwoId] ORDER BY [u1].[Id]) AS [row]
        FROM [UnidirectionalEntityTwoUnidirectionalEntityTwo] AS [u0]
        INNER JOIN [UnidirectionalEntityTwos] AS [u1] ON [u0].[SelfSkipSharedRightId] = [u1].[Id]
    ) AS [s]
    WHERE 2 < [s].[row]
) AS [s0] ON [u].[Id] = [s0].[UnidirectionalEntityTwoId]
ORDER BY [u].[Id], [s0].[UnidirectionalEntityTwoId], [s0].[Id]
""");
    }

    public override async Task Filtered_include_skip_navigation_order_by_take_unidirectional(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_take_unidirectional(async);

        AssertSql(
            """
SELECT [u].[Key1], [u].[Key2], [u].[Key3], [u].[Name], [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[ReferenceInverseId], [s0].[TwoSkipSharedId], [s0].[UnidirectionalEntityCompositeKeyKey1], [s0].[UnidirectionalEntityCompositeKeyKey2], [s0].[UnidirectionalEntityCompositeKeyKey3]
FROM [UnidirectionalEntityCompositeKeys] AS [u]
LEFT JOIN (
    SELECT [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId], [s].[TwoSkipSharedId], [s].[UnidirectionalEntityCompositeKeyKey1], [s].[UnidirectionalEntityCompositeKeyKey2], [s].[UnidirectionalEntityCompositeKeyKey3]
    FROM (
        SELECT [u1].[Id], [u1].[CollectionInverseId], [u1].[ExtraId], [u1].[Name], [u1].[ReferenceInverseId], [u0].[TwoSkipSharedId], [u0].[UnidirectionalEntityCompositeKeyKey1], [u0].[UnidirectionalEntityCompositeKeyKey2], [u0].[UnidirectionalEntityCompositeKeyKey3], ROW_NUMBER() OVER(PARTITION BY [u0].[UnidirectionalEntityCompositeKeyKey1], [u0].[UnidirectionalEntityCompositeKeyKey2], [u0].[UnidirectionalEntityCompositeKeyKey3] ORDER BY [u1].[Id]) AS [row]
        FROM [UnidirectionalEntityCompositeKeyUnidirectionalEntityTwo] AS [u0]
        INNER JOIN [UnidirectionalEntityTwos] AS [u1] ON [u0].[TwoSkipSharedId] = [u1].[Id]
    ) AS [s]
    WHERE [s].[row] <= 2
) AS [s0] ON [u].[Key1] = [s0].[UnidirectionalEntityCompositeKeyKey1] AND [u].[Key2] = [s0].[UnidirectionalEntityCompositeKeyKey2] AND [u].[Key3] = [s0].[UnidirectionalEntityCompositeKeyKey3]
ORDER BY [u].[Key1], [u].[Key2], [u].[Key3], [s0].[UnidirectionalEntityCompositeKeyKey1], [s0].[UnidirectionalEntityCompositeKeyKey2], [s0].[UnidirectionalEntityCompositeKeyKey3], [s0].[Id]
""");
    }

    public override async Task Filtered_include_skip_navigation_order_by_skip_take_unidirectional(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip_take_unidirectional(async);
        AssertSql(
            """
SELECT [u].[Key1], [u].[Key2], [u].[Key3], [u].[Name], [s0].[Id], [s0].[CollectionInverseId], [s0].[Name], [s0].[ReferenceInverseId], [s0].[Id0]
FROM [UnidirectionalEntityCompositeKeys] AS [u]
LEFT JOIN (
    SELECT [s].[Id], [s].[CollectionInverseId], [s].[Name], [s].[ReferenceInverseId], [s].[Id0], [s].[CompositeId1], [s].[CompositeId2], [s].[CompositeId3]
    FROM (
        SELECT [u1].[Id], [u1].[CollectionInverseId], [u1].[Name], [u1].[ReferenceInverseId], [u0].[Id] AS [Id0], [u0].[CompositeId1], [u0].[CompositeId2], [u0].[CompositeId3], ROW_NUMBER() OVER(PARTITION BY [u0].[CompositeId1], [u0].[CompositeId2], [u0].[CompositeId3] ORDER BY [u1].[Id]) AS [row]
        FROM [UnidirectionalJoinThreeToCompositeKeyFull] AS [u0]
        INNER JOIN [UnidirectionalEntityThrees] AS [u1] ON [u0].[ThreeId] = [u1].[Id]
    ) AS [s]
    WHERE 1 < [s].[row] AND [s].[row] <= 3
) AS [s0] ON [u].[Key1] = [s0].[CompositeId1] AND [u].[Key2] = [s0].[CompositeId2] AND [u].[Key3] = [s0].[CompositeId3]
ORDER BY [u].[Key1], [u].[Key2], [u].[Key3], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3], [s0].[Id]
""");
    }

    public override async Task Filtered_include_skip_navigation_where_then_include_skip_navigation_unidirectional(bool async)
    {
        await base.Filtered_include_skip_navigation_where_then_include_skip_navigation_unidirectional(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u0].[Number], [u1].[IsGreen], [s0].[Key1], [s0].[Key2], [s0].[Key3], [s0].[Name], [s0].[LeafId], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3], [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name0], [s0].[ReferenceInverseId], [s0].[TwoSkipSharedId], [s0].[UnidirectionalEntityCompositeKeyKey1], [s0].[UnidirectionalEntityCompositeKeyKey2], [s0].[UnidirectionalEntityCompositeKeyKey3]
FROM [UnidirectionalRoots] AS [u]
INNER JOIN [UnidirectionalBranches] AS [u0] ON [u].[Id] = [u0].[Id]
INNER JOIN [UnidirectionalLeaves] AS [u1] ON [u].[Id] = [u1].[Id]
LEFT JOIN (
    SELECT [u3].[Key1], [u3].[Key2], [u3].[Key3], [u3].[Name], [u2].[LeafId], [u2].[CompositeId1], [u2].[CompositeId2], [u2].[CompositeId3], [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name] AS [Name0], [s].[ReferenceInverseId], [s].[TwoSkipSharedId], [s].[UnidirectionalEntityCompositeKeyKey1], [s].[UnidirectionalEntityCompositeKeyKey2], [s].[UnidirectionalEntityCompositeKeyKey3]
    FROM [UnidirectionalJoinCompositeKeyToLeaf] AS [u2]
    INNER JOIN [UnidirectionalEntityCompositeKeys] AS [u3] ON [u2].[CompositeId1] = [u3].[Key1] AND [u2].[CompositeId2] = [u3].[Key2] AND [u2].[CompositeId3] = [u3].[Key3]
    LEFT JOIN (
        SELECT [u5].[Id], [u5].[CollectionInverseId], [u5].[ExtraId], [u5].[Name], [u5].[ReferenceInverseId], [u4].[TwoSkipSharedId], [u4].[UnidirectionalEntityCompositeKeyKey1], [u4].[UnidirectionalEntityCompositeKeyKey2], [u4].[UnidirectionalEntityCompositeKeyKey3]
        FROM [UnidirectionalEntityCompositeKeyUnidirectionalEntityTwo] AS [u4]
        INNER JOIN [UnidirectionalEntityTwos] AS [u5] ON [u4].[TwoSkipSharedId] = [u5].[Id]
    ) AS [s] ON [u3].[Key1] = [s].[UnidirectionalEntityCompositeKeyKey1] AND [u3].[Key2] = [s].[UnidirectionalEntityCompositeKeyKey2] AND [u3].[Key3] = [s].[UnidirectionalEntityCompositeKeyKey3]
    WHERE [u3].[Key1] < 5
) AS [s0] ON [u].[Id] = [s0].[LeafId]
ORDER BY [u].[Id], [s0].[LeafId], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3], [s0].[Key1], [s0].[Key2], [s0].[Key3], [s0].[TwoSkipSharedId], [s0].[UnidirectionalEntityCompositeKeyKey1], [s0].[UnidirectionalEntityCompositeKeyKey2], [s0].[UnidirectionalEntityCompositeKeyKey3]
""");
    }

    public override async Task Filter_include_on_skip_navigation_combined_unidirectional(bool async)
    {
        await base.Filter_include_on_skip_navigation_combined_unidirectional(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId], [s].[Id], [s].[Name], [s].[Id0], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name0], [s].[ReferenceInverseId], [s].[OneId], [s].[TwoId], [s].[Id1], [s].[CollectionInverseId0], [s].[ExtraId0], [s].[Name1], [s].[ReferenceInverseId0]
FROM [EntityTwos] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId], [j].[OneId], [j].[TwoId], [e2].[Id] AS [Id1], [e2].[CollectionInverseId] AS [CollectionInverseId0], [e2].[ExtraId] AS [ExtraId0], [e2].[Name] AS [Name1], [e2].[ReferenceInverseId] AS [ReferenceInverseId0]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
    LEFT JOIN [EntityTwos] AS [e2] ON [e0].[Id] = [e2].[CollectionInverseId]
    WHERE [e0].[Id] < 10
) AS [s] ON [e].[Id] = [s].[TwoId]
ORDER BY [e].[Id], [s].[OneId], [s].[TwoId], [s].[Id], [s].[Id0]
""");
    }

    public override async Task Throws_when_different_filtered_include_unidirectional(bool async)
    {
        await base.Throws_when_different_filtered_include_unidirectional(async);

        AssertSql();
    }

    public override async Task Includes_accessed_via_different_path_are_merged_unidirectional(bool async)
    {
        await base.Includes_accessed_via_different_path_are_merged_unidirectional(async);

        AssertSql();
    }

    public override async Task Select_many_over_skip_navigation_where_non_equality_unidirectional(bool async)
    {
        await base.Select_many_over_skip_navigation_where_non_equality_unidirectional(async);

        AssertSql(
            """
SELECT [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId]
FROM [UnidirectionalEntityOnes] AS [u]
LEFT JOIN (
    SELECT [u1].[Id], [u1].[CollectionInverseId], [u1].[ExtraId], [u1].[Name], [u1].[ReferenceInverseId], [u0].[OneId]
    FROM [UnidirectionalJoinOneToTwo] AS [u0]
    INNER JOIN [UnidirectionalEntityTwos] AS [u1] ON [u0].[TwoId] = [u1].[Id]
) AS [s] ON [u].[Id] = [s].[OneId] AND [u].[Id] <> [s].[Id]
""");
    }

    public override async Task Contains_on_skip_collection_navigation_unidirectional(bool async)
    {
        await base.Contains_on_skip_collection_navigation_unidirectional(async);

        AssertSql(
            """
@__entity_equality_two_0_Id='1' (Nullable = true)

SELECT [u].[Id], [u].[Name]
FROM [UnidirectionalEntityOnes] AS [u]
WHERE EXISTS (
    SELECT 1
    FROM [UnidirectionalJoinOneToTwo] AS [u0]
    INNER JOIN [UnidirectionalEntityTwos] AS [u1] ON [u0].[TwoId] = [u1].[Id]
    WHERE [u].[Id] = [u0].[OneId] AND [u1].[Id] = @__entity_equality_two_0_Id)
""");
    }

    public override async Task GetType_in_hierarchy_in_base_type_unidirectional(bool async)
    {
        await base.GetType_in_hierarchy_in_base_type_unidirectional(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u0].[Number], [u1].[IsGreen], CASE
    WHEN [u1].[Id] IS NOT NULL THEN N'UnidirectionalEntityLeaf'
    WHEN [u0].[Id] IS NOT NULL THEN N'UnidirectionalEntityBranch'
END AS [Discriminator]
FROM [UnidirectionalRoots] AS [u]
LEFT JOIN [UnidirectionalBranches] AS [u0] ON [u].[Id] = [u0].[Id]
LEFT JOIN [UnidirectionalLeaves] AS [u1] ON [u].[Id] = [u1].[Id]
WHERE [u1].[Id] IS NULL AND [u0].[Id] IS NULL
""");
    }

    public override async Task GetType_in_hierarchy_in_intermediate_type_unidirectional(bool async)
    {
        await base.GetType_in_hierarchy_in_intermediate_type_unidirectional(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u0].[Number], [u1].[IsGreen], CASE
    WHEN [u1].[Id] IS NOT NULL THEN N'UnidirectionalEntityLeaf'
    WHEN [u0].[Id] IS NOT NULL THEN N'UnidirectionalEntityBranch'
END AS [Discriminator]
FROM [UnidirectionalRoots] AS [u]
LEFT JOIN [UnidirectionalBranches] AS [u0] ON [u].[Id] = [u0].[Id]
LEFT JOIN [UnidirectionalLeaves] AS [u1] ON [u].[Id] = [u1].[Id]
WHERE [u1].[Id] IS NULL AND [u0].[Id] IS NOT NULL
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_unidirectional(bool async)
    {
        await base.GetType_in_hierarchy_in_leaf_type_unidirectional(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u0].[Number], [u1].[IsGreen], CASE
    WHEN [u1].[Id] IS NOT NULL THEN N'UnidirectionalEntityLeaf'
    WHEN [u0].[Id] IS NOT NULL THEN N'UnidirectionalEntityBranch'
END AS [Discriminator]
FROM [UnidirectionalRoots] AS [u]
LEFT JOIN [UnidirectionalBranches] AS [u0] ON [u].[Id] = [u0].[Id]
LEFT JOIN [UnidirectionalLeaves] AS [u1] ON [u].[Id] = [u1].[Id]
WHERE [u1].[Id] IS NOT NULL
""");
    }

    public override async Task GetType_in_hierarchy_in_querying_base_type_unidirectional(bool async)
    {
        await base.GetType_in_hierarchy_in_querying_base_type_unidirectional(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u0].[Number], [u1].[IsGreen], CASE
    WHEN [u1].[Id] IS NOT NULL THEN N'UnidirectionalEntityLeaf'
END AS [Discriminator]
FROM [UnidirectionalRoots] AS [u]
INNER JOIN [UnidirectionalBranches] AS [u0] ON [u].[Id] = [u0].[Id]
LEFT JOIN [UnidirectionalLeaves] AS [u1] ON [u].[Id] = [u1].[Id]
WHERE 0 = 1
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
