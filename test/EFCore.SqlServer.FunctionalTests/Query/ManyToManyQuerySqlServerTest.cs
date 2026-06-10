// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class ManyToManyQuerySqlServerTest : ManyToManyQueryRelationalTestBase<ManyToManyQuerySqlServerFixture>
{
    public ManyToManyQuerySqlServerTest(ManyToManyQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

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
        SELECT [e0].[Id], [e0].[Name]
        FROM [EntityRoots] AS [e0]
        WHERE [e0].[Discriminator] IN (N'EntityBranch', N'EntityLeaf')
    ) AS [e1] ON [j].[EntityBranchId] = [e1].[Id]
    WHERE [e].[Id] = [j].[EntityOneId] AND [e1].[Name] LIKE N'L%'), [e].[Id]
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
SELECT MIN([s].[Id])
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [e1].[Id], [e0].[ThreeSkipSharedId]
    FROM [EntityRootEntityThree] AS [e0]
    INNER JOIN [EntityRoots] AS [e1] ON [e0].[RootSkipSharedId] = [e1].[Id]
) AS [s] ON [e].[Id] = [s].[ThreeSkipSharedId]
""");
    }

    public override async Task Skip_navigation_select_many_sum(bool async)
    {
        await base.Skip_navigation_select_many_sum(async);

        AssertSql(
            """
SELECT COALESCE(SUM([s].[Key1]), 0)
FROM [EntityRoots] AS [e]
INNER JOIN (
    SELECT [e1].[Key1], [e0].[RootSkipSharedId]
    FROM [EntityCompositeKeyEntityRoot] AS [e0]
    INNER JOIN [EntityCompositeKeys] AS [e1] ON [e0].[CompositeKeySkipSharedKey1] = [e1].[Key1] AND [e0].[CompositeKeySkipSharedKey2] = [e1].[Key2] AND [e0].[CompositeKeySkipSharedKey3] = [e1].[Key3]
) AS [s] ON [e].[Id] = [s].[RootSkipSharedId]
""");
    }

    public override async Task Skip_navigation_select_subquery_average(bool async)
    {
        await base.Skip_navigation_select_subquery_average(async);

        AssertSql(
            """
SELECT (
    SELECT AVG(CAST([e0].[Key1] AS float))
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN [EntityCompositeKeys] AS [e0] ON [j].[CompositeId1] = [e0].[Key1] AND [j].[CompositeId2] = [e0].[Key2] AND [j].[CompositeId3] = [e0].[Key3]
    WHERE [e].[Id] = [j].[LeafId])
FROM [EntityRoots] AS [e]
WHERE [e].[Discriminator] = N'EntityLeaf'
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
FROM [EntityRoots] AS [e]
LEFT JOIN (
    SELECT [s].[Id], [s].[Name], [s].[EntityBranchId]
    FROM (
        SELECT [e0].[Id], [e0].[Name], [j].[EntityBranchId], ROW_NUMBER() OVER(PARTITION BY [j].[EntityBranchId] ORDER BY [e0].[Id] DESC) AS [row]
        FROM [JoinOneToBranch] AS [j]
        INNER JOIN [EntityOnes] AS [e0] ON [j].[EntityOneId] = [e0].[Id]
    ) AS [s]
    WHERE [s].[row] <= 1
) AS [s0] ON [e].[Id] = [s0].[EntityBranchId]
WHERE [e].[Discriminator] IN (N'EntityBranch', N'EntityLeaf')
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
SELECT [e].[Key1], [e].[Key2], [e].[Key3], [s].[Id], [s].[Discriminator], [s].[Name], [s].[Number], [s].[IsGreen], [s].[LeafId], [s].[CompositeId1], [s].[CompositeId2], [s].[CompositeId3]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [e1].[Id], [e1].[Discriminator], [e1].[Name], [e1].[Number], [e1].[IsGreen], [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN (
        SELECT [e0].[Id], [e0].[Discriminator], [e0].[Name], [e0].[Number], [e0].[IsGreen]
        FROM [EntityRoots] AS [e0]
        WHERE [e0].[Discriminator] = N'EntityLeaf'
    ) AS [e1] ON [j].[LeafId] = [e1].[Id]
) AS [s] ON [e].[Key1] = [s].[CompositeId1] AND [e].[Key2] = [s].[CompositeId2] AND [e].[Key3] = [s].[CompositeId3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [s].[LeafId], [s].[CompositeId1], [s].[CompositeId2], [s].[CompositeId3]
""");
    }

    public override async Task Skip_navigation_of_type(bool async)
    {
        await base.Skip_navigation_of_type(async);

        AssertSql(
            """
SELECT [e].[Key1], [e].[Key2], [e].[Key3], [s].[Id], [s].[Discriminator], [s].[Name], [s].[Number], [s].[IsGreen], [s].[RootSkipSharedId], [s].[CompositeKeySkipSharedKey1], [s].[CompositeKeySkipSharedKey2], [s].[CompositeKeySkipSharedKey3]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [e1].[Id], [e1].[Discriminator], [e1].[Name], [e1].[Number], [e1].[IsGreen], [e0].[RootSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3]
    FROM [EntityCompositeKeyEntityRoot] AS [e0]
    INNER JOIN [EntityRoots] AS [e1] ON [e0].[RootSkipSharedId] = [e1].[Id]
    WHERE [e1].[Discriminator] = N'EntityLeaf'
) AS [s] ON [e].[Key1] = [s].[CompositeKeySkipSharedKey1] AND [e].[Key2] = [s].[CompositeKeySkipSharedKey2] AND [e].[Key3] = [s].[CompositeKeySkipSharedKey3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [s].[RootSkipSharedId], [s].[CompositeKeySkipSharedKey1], [s].[CompositeKeySkipSharedKey2], [s].[CompositeKeySkipSharedKey3]
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
FROM [EntityRoots] AS [e]
INNER JOIN (
    SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], [e0].[RootSkipSharedId]
    FROM [EntityRootEntityThree] AS [e0]
    INNER JOIN [EntityThrees] AS [e1] ON [e0].[ThreeSkipSharedId] = [e1].[Id]
) AS [s] ON [e].[Id] = [s].[RootSkipSharedId]
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
SELECT [s].[Id], [s].[Discriminator], [s].[Name], [s].[Number], [s].[IsGreen]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [e1].[Id], [e1].[Discriminator], [e1].[Name], [e1].[Number], [e1].[IsGreen], [e0].[ThreeSkipSharedId]
    FROM [EntityRootEntityThree] AS [e0]
    INNER JOIN [EntityRoots] AS [e1] ON [e0].[RootSkipSharedId] = [e1].[Id]
    WHERE [e1].[Discriminator] IN (N'EntityBranch', N'EntityLeaf')
) AS [s] ON [e].[Id] = [s].[ThreeSkipSharedId]
""");
    }

    public override async Task Select_many_over_skip_navigation_cast(bool async)
    {
        await base.Select_many_over_skip_navigation_cast(async);

        AssertSql(
            """
SELECT [s].[Id], [s].[Discriminator], [s].[Name], [s].[Number], [s].[IsGreen]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [e1].[Id], [e1].[Discriminator], [e1].[Name], [e1].[Number], [e1].[IsGreen], [j].[EntityOneId]
    FROM [JoinOneToBranch] AS [j]
    INNER JOIN (
        SELECT [e0].[Id], [e0].[Discriminator], [e0].[Name], [e0].[Number], [e0].[IsGreen]
        FROM [EntityRoots] AS [e0]
        WHERE [e0].[Discriminator] IN (N'EntityBranch', N'EntityLeaf')
    ) AS [e1] ON [j].[EntityBranchId] = [e1].[Id]
) AS [s] ON [e].[Id] = [s].[EntityOneId]
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
SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [s].[RootSkipSharedId], [s].[CompositeKeySkipSharedKey1], [s].[CompositeKeySkipSharedKey2], [s].[CompositeKeySkipSharedKey3], [s].[Id], [s].[Discriminator], [s].[Name], [s].[Number], [s].[Slumber], [s].[IsGreen], [s].[IsBrown]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [e0].[RootSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], [e1].[Id], [e1].[Discriminator], [e1].[Name], [e1].[Number], [e1].[Slumber], [e1].[IsGreen], [e1].[IsBrown]
    FROM [EntityCompositeKeyEntityRoot] AS [e0]
    INNER JOIN [EntityRoots] AS [e1] ON [e0].[RootSkipSharedId] = [e1].[Id]
) AS [s] ON [e].[Key1] = [s].[CompositeKeySkipSharedKey1] AND [e].[Key2] = [s].[CompositeKeySkipSharedKey2] AND [e].[Key3] = [s].[CompositeKeySkipSharedKey3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [s].[RootSkipSharedId], [s].[CompositeKeySkipSharedKey1], [s].[CompositeKeySkipSharedKey2], [s].[CompositeKeySkipSharedKey3]
""");
    }

    public override async Task Include_skip_navigation_then_reference(bool async)
    {
        await base.Include_skip_navigation_then_reference(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId], [s].[OneId], [s].[TwoId], [s].[JoinOneToTwoExtraId], [s].[Id], [s].[Name], [s].[Id0], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name0], [s].[ReferenceInverseId]
FROM [EntityTwos] AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId]
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
SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [s0].[LeafId], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3], [s0].[Id], [s0].[Discriminator], [s0].[Name], [s0].[Number], [s0].[IsGreen], [s0].[EntityBranchId], [s0].[EntityOneId], [s0].[Id0], [s0].[Name0]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [e1].[Id], [e1].[Discriminator], [e1].[Name], [e1].[Number], [e1].[IsGreen], [s].[EntityBranchId], [s].[EntityOneId], [s].[Id] AS [Id0], [s].[Name] AS [Name0]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN (
        SELECT [e0].[Id], [e0].[Discriminator], [e0].[Name], [e0].[Number], [e0].[IsGreen]
        FROM [EntityRoots] AS [e0]
        WHERE [e0].[Discriminator] = N'EntityLeaf'
    ) AS [e1] ON [j].[LeafId] = [e1].[Id]
    LEFT JOIN (
        SELECT [j0].[EntityBranchId], [j0].[EntityOneId], [e2].[Id], [e2].[Name]
        FROM [JoinOneToBranch] AS [j0]
        INNER JOIN [EntityOnes] AS [e2] ON [j0].[EntityOneId] = [e2].[Id]
    ) AS [s] ON [e1].[Id] = [s].[EntityBranchId]
) AS [s0] ON [e].[Key1] = [s0].[CompositeId1] AND [e].[Key2] = [s0].[CompositeId2] AND [e].[Key3] = [s0].[CompositeId3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [s0].[LeafId], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3], [s0].[Id], [s0].[EntityBranchId], [s0].[EntityOneId]
""");
    }

    public override async Task Include_skip_navigation_then_include_reference_and_skip_navigation(bool async)
    {
        await base.Include_skip_navigation_then_include_reference_and_skip_navigation(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId], [s0].[OneId], [s0].[ThreeId], [s0].[Payload], [s0].[Id], [s0].[Name], [s0].[Id0], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name0], [s0].[ReferenceInverseId], [s0].[LeftId], [s0].[RightId], [s0].[Payload0], [s0].[Id1], [s0].[Name1]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId], [s].[LeftId], [s].[RightId], [s].[Payload] AS [Payload0], [s].[Id] AS [Id1], [s].[Name] AS [Name1]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
    LEFT JOIN (
        SELECT [j0].[LeftId], [j0].[RightId], [j0].[Payload], [e2].[Id], [e2].[Name]
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
SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId], [e0].[Id], [s].[OneSkipSharedId], [s].[TwoSkipSharedId], [s].[Id], [s].[Name], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId]
FROM [EntityTwos] AS [e]
LEFT JOIN [EntityThrees] AS [e0] ON [e].[Id] = [e0].[ReferenceInverseId]
LEFT JOIN (
    SELECT [e1].[OneSkipSharedId], [e1].[TwoSkipSharedId], [e2].[Id], [e2].[Name]
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
SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId], [s].[OneId], [s].[ThreeId], [s].[Payload], [s].[Id], [s].[Name]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [e0].[Id], [e0].[Name]
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
SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId], [s].[ThreeId], [s].[TwoId], [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [j].[ThreeId], [j].[TwoId], [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId]
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
SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId], [s0].[SelfSkipSharedLeftId], [s0].[SelfSkipSharedRightId], [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[ReferenceInverseId]
FROM [EntityTwos] AS [e]
LEFT JOIN (
    SELECT [s].[SelfSkipSharedLeftId], [s].[SelfSkipSharedRightId], [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId]
    FROM (
        SELECT [e0].[SelfSkipSharedLeftId], [e0].[SelfSkipSharedRightId], [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [e0].[SelfSkipSharedLeftId] ORDER BY [e1].[Id]) AS [row]
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
SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [s0].[TwoSkipSharedId], [s0].[CompositeKeySkipSharedKey1], [s0].[CompositeKeySkipSharedKey2], [s0].[CompositeKeySkipSharedKey3], [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[ReferenceInverseId]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [s].[TwoSkipSharedId], [s].[CompositeKeySkipSharedKey1], [s].[CompositeKeySkipSharedKey2], [s].[CompositeKeySkipSharedKey3], [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId]
    FROM (
        SELECT [e0].[TwoSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3] ORDER BY [e1].[Id]) AS [row]
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
SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [s0].[Id], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3], [s0].[ThreeId], [s0].[Id0], [s0].[CollectionInverseId], [s0].[Name], [s0].[ReferenceInverseId]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [s].[Id], [s].[CompositeId1], [s].[CompositeId2], [s].[CompositeId3], [s].[ThreeId], [s].[Id0], [s].[CollectionInverseId], [s].[Name], [s].[ReferenceInverseId]
    FROM (
        SELECT [j].[Id], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [j].[ThreeId], [e0].[Id] AS [Id0], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinThreeToCompositeKeyFull] AS [j]
        INNER JOIN [EntityThrees] AS [e0] ON [j].[ThreeId] = [e0].[Id]
    ) AS [s]
    WHERE 1 < [s].[row] AND [s].[row] <= 3
) AS [s0] ON [e].[Key1] = [s0].[CompositeId1] AND [e].[Key2] = [s0].[CompositeId2] AND [e].[Key3] = [s0].[CompositeId3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3], [s0].[Id0]
""");
    }

    public override async Task Filtered_then_include_skip_navigation_where(bool async)
    {
        await base.Filtered_then_include_skip_navigation_where(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[Number], [e].[Slumber], [e].[IsGreen], [e].[IsBrown], [s0].[RootSkipSharedId], [s0].[ThreeSkipSharedId], [s0].[Id], [s0].[CollectionInverseId], [s0].[Name], [s0].[ReferenceInverseId], [s0].[OneId], [s0].[ThreeId], [s0].[Payload], [s0].[Id0], [s0].[Name0]
FROM [EntityRoots] AS [e]
LEFT JOIN (
    SELECT [e0].[RootSkipSharedId], [e0].[ThreeSkipSharedId], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], [s].[OneId], [s].[ThreeId], [s].[Payload], [s].[Id] AS [Id0], [s].[Name] AS [Name0]
    FROM [EntityRootEntityThree] AS [e0]
    INNER JOIN [EntityThrees] AS [e1] ON [e0].[ThreeSkipSharedId] = [e1].[Id]
    LEFT JOIN (
        SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [e2].[Id], [e2].[Name]
        FROM [JoinOneToThreePayloadFullShared] AS [j]
        INNER JOIN [EntityOnes] AS [e2] ON [j].[OneId] = [e2].[Id]
        WHERE [e2].[Id] < 10
    ) AS [s] ON [e1].[Id] = [s].[ThreeId]
) AS [s0] ON [e].[Id] = [s0].[RootSkipSharedId]
ORDER BY [e].[Id], [s0].[RootSkipSharedId], [s0].[ThreeSkipSharedId], [s0].[Id], [s0].[OneId], [s0].[ThreeId]
""");
    }

    public override async Task Filtered_then_include_skip_navigation_order_by_skip_take(bool async)
    {
        await base.Filtered_then_include_skip_navigation_order_by_skip_take(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[Number], [e].[Slumber], [e].[IsGreen], [e].[IsBrown], [s1].[RootSkipSharedId], [s1].[CompositeKeySkipSharedKey1], [s1].[CompositeKeySkipSharedKey2], [s1].[CompositeKeySkipSharedKey3], [s1].[Key1], [s1].[Key2], [s1].[Key3], [s1].[Name], [s1].[Id], [s1].[CompositeId1], [s1].[CompositeId2], [s1].[CompositeId3], [s1].[ThreeId], [s1].[Id0], [s1].[CollectionInverseId], [s1].[Name0], [s1].[ReferenceInverseId]
FROM [EntityRoots] AS [e]
LEFT JOIN (
    SELECT [e0].[RootSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], [e1].[Key1], [e1].[Key2], [e1].[Key3], [e1].[Name], [s0].[Id], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3], [s0].[ThreeId], [s0].[Id0], [s0].[CollectionInverseId], [s0].[Name] AS [Name0], [s0].[ReferenceInverseId]
    FROM [EntityCompositeKeyEntityRoot] AS [e0]
    INNER JOIN [EntityCompositeKeys] AS [e1] ON [e0].[CompositeKeySkipSharedKey1] = [e1].[Key1] AND [e0].[CompositeKeySkipSharedKey2] = [e1].[Key2] AND [e0].[CompositeKeySkipSharedKey3] = [e1].[Key3]
    LEFT JOIN (
        SELECT [s].[Id], [s].[CompositeId1], [s].[CompositeId2], [s].[CompositeId3], [s].[ThreeId], [s].[Id0], [s].[CollectionInverseId], [s].[Name], [s].[ReferenceInverseId]
        FROM (
            SELECT [j].[Id], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [j].[ThreeId], [e2].[Id] AS [Id0], [e2].[CollectionInverseId], [e2].[Name], [e2].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3] ORDER BY [e2].[Id]) AS [row]
            FROM [JoinThreeToCompositeKeyFull] AS [j]
            INNER JOIN [EntityThrees] AS [e2] ON [j].[ThreeId] = [e2].[Id]
        ) AS [s]
        WHERE 1 < [s].[row] AND [s].[row] <= 3
    ) AS [s0] ON [e1].[Key1] = [s0].[CompositeId1] AND [e1].[Key2] = [s0].[CompositeId2] AND [e1].[Key3] = [s0].[CompositeId3]
) AS [s1] ON [e].[Id] = [s1].[RootSkipSharedId]
ORDER BY [e].[Id], [s1].[RootSkipSharedId], [s1].[CompositeKeySkipSharedKey1], [s1].[CompositeKeySkipSharedKey2], [s1].[CompositeKeySkipSharedKey3], [s1].[Key1], [s1].[Key2], [s1].[Key3], [s1].[CompositeId1], [s1].[CompositeId2], [s1].[CompositeId3], [s1].[Id0]
""");
    }

    public override async Task Filtered_include_skip_navigation_where_then_include_skip_navigation(bool async)
    {
        await base.Filtered_include_skip_navigation_where_then_include_skip_navigation(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[Number], [e].[IsGreen], [s0].[LeafId], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3], [s0].[Key1], [s0].[Key2], [s0].[Key3], [s0].[Name], [s0].[TwoSkipSharedId], [s0].[CompositeKeySkipSharedKey1], [s0].[CompositeKeySkipSharedKey2], [s0].[CompositeKeySkipSharedKey3], [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name0], [s0].[ReferenceInverseId]
FROM [EntityRoots] AS [e]
LEFT JOIN (
    SELECT [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [e0].[Key1], [e0].[Key2], [e0].[Key3], [e0].[Name], [s].[TwoSkipSharedId], [s].[CompositeKeySkipSharedKey1], [s].[CompositeKeySkipSharedKey2], [s].[CompositeKeySkipSharedKey3], [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name] AS [Name0], [s].[ReferenceInverseId]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN [EntityCompositeKeys] AS [e0] ON [j].[CompositeId1] = [e0].[Key1] AND [j].[CompositeId2] = [e0].[Key2] AND [j].[CompositeId3] = [e0].[Key3]
    LEFT JOIN (
        SELECT [e1].[TwoSkipSharedId], [e1].[CompositeKeySkipSharedKey1], [e1].[CompositeKeySkipSharedKey2], [e1].[CompositeKeySkipSharedKey3], [e2].[Id], [e2].[CollectionInverseId], [e2].[ExtraId], [e2].[Name], [e2].[ReferenceInverseId]
        FROM [EntityCompositeKeyEntityTwo] AS [e1]
        INNER JOIN [EntityTwos] AS [e2] ON [e1].[TwoSkipSharedId] = [e2].[Id]
    ) AS [s] ON [e0].[Key1] = [s].[CompositeKeySkipSharedKey1] AND [e0].[Key2] = [s].[CompositeKeySkipSharedKey2] AND [e0].[Key3] = [s].[CompositeKeySkipSharedKey3]
    WHERE [e0].[Key1] < 5
) AS [s0] ON [e].[Id] = [s0].[LeafId]
WHERE [e].[Discriminator] = N'EntityLeaf'
ORDER BY [e].[Id], [s0].[LeafId], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3], [s0].[Key1], [s0].[Key2], [s0].[Key3], [s0].[TwoSkipSharedId], [s0].[CompositeKeySkipSharedKey1], [s0].[CompositeKeySkipSharedKey2], [s0].[CompositeKeySkipSharedKey3]
""");
    }

    public override async Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], [s1].[OneId], [s1].[TwoId], [s1].[JoinOneToTwoExtraId], [s1].[Id], [s1].[CollectionInverseId], [s1].[ExtraId], [s1].[Name], [s1].[ReferenceInverseId], [s1].[ThreeId], [s1].[TwoId0], [s1].[Id0], [s1].[CollectionInverseId0], [s1].[Name0], [s1].[ReferenceInverseId0]
FROM [EntityOnes] AS [e]
OUTER APPLY (
    SELECT [s].[OneId], [s].[TwoId], [s].[JoinOneToTwoExtraId], [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId], [s0].[ThreeId], [s0].[TwoId] AS [TwoId0], [s0].[Id] AS [Id0], [s0].[CollectionInverseId] AS [CollectionInverseId0], [s0].[Name] AS [Name0], [s0].[ReferenceInverseId] AS [ReferenceInverseId0]
    FROM (
        SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId]
        FROM [JoinOneToTwo] AS [j]
        INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
        WHERE [e].[Id] = [j].[OneId]
        ORDER BY [e0].[Id]
        OFFSET 1 ROWS FETCH NEXT 2 ROWS ONLY
    ) AS [s]
    LEFT JOIN (
        SELECT [j0].[ThreeId], [j0].[TwoId], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId]
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
SELECT [e].[Id], [e].[Name], [s1].[OneId], [s1].[TwoId], [s1].[JoinOneToTwoExtraId], [s1].[Id], [s1].[CollectionInverseId], [s1].[ExtraId], [s1].[Name], [s1].[ReferenceInverseId], [s1].[ThreeId], [s1].[TwoId0], [s1].[Id0], [s1].[CollectionInverseId0], [s1].[Name0], [s1].[ReferenceInverseId0]
FROM [EntityOnes] AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], [s0].[ThreeId], [s0].[TwoId] AS [TwoId0], [s0].[Id] AS [Id0], [s0].[CollectionInverseId] AS [CollectionInverseId0], [s0].[Name] AS [Name0], [s0].[ReferenceInverseId] AS [ReferenceInverseId0]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    LEFT JOIN (
        SELECT [s].[ThreeId], [s].[TwoId], [s].[Id], [s].[CollectionInverseId], [s].[Name], [s].[ReferenceInverseId]
        FROM (
            SELECT [j0].[ThreeId], [j0].[TwoId], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j0].[TwoId] ORDER BY [e1].[Id]) AS [row]
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
SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId], [s].[OneId], [s].[TwoId], [s].[JoinOneToTwoExtraId], [s].[Id], [s].[Name], [s].[Id0], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name0], [s].[ReferenceInverseId], [s].[Id1], [s].[CollectionInverseId0], [s].[ExtraId0], [s].[Name1], [s].[ReferenceInverseId0]
FROM [EntityTwos] AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId], [e2].[Id] AS [Id1], [e2].[CollectionInverseId] AS [CollectionInverseId0], [e2].[ExtraId] AS [ExtraId0], [e2].[Name] AS [Name1], [e2].[ReferenceInverseId] AS [ReferenceInverseId0]
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
SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId], [s2].[OneId], [s2].[ThreeId], [s2].[Payload], [s2].[Id], [s2].[Name], [s2].[OneId0], [s2].[TwoId], [s2].[JoinOneToTwoExtraId], [s2].[Id0], [s2].[CollectionInverseId], [s2].[ExtraId], [s2].[Name0], [s2].[ReferenceInverseId], [s2].[EntityBranchId], [s2].[EntityOneId], [s2].[Id1], [s2].[Discriminator], [s2].[Name1], [s2].[Number], [s2].[IsGreen]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [e0].[Id], [e0].[Name], [s0].[OneId] AS [OneId0], [s0].[TwoId], [s0].[JoinOneToTwoExtraId], [s0].[Id] AS [Id0], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name] AS [Name0], [s0].[ReferenceInverseId], [s1].[EntityBranchId], [s1].[EntityOneId], [s1].[Id] AS [Id1], [s1].[Discriminator], [s1].[Name] AS [Name1], [s1].[Number], [s1].[IsGreen]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN (
        SELECT [s].[OneId], [s].[TwoId], [s].[JoinOneToTwoExtraId], [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId]
        FROM (
            SELECT [j0].[OneId], [j0].[TwoId], [j0].[JoinOneToTwoExtraId], [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j0].[OneId] ORDER BY [e1].[Id]) AS [row]
            FROM [JoinOneToTwo] AS [j0]
            INNER JOIN [EntityTwos] AS [e1] ON [j0].[TwoId] = [e1].[Id]
        ) AS [s]
        WHERE 1 < [s].[row] AND [s].[row] <= 3
    ) AS [s0] ON [e0].[Id] = [s0].[OneId]
    LEFT JOIN (
        SELECT [j1].[EntityBranchId], [j1].[EntityOneId], [e3].[Id], [e3].[Discriminator], [e3].[Name], [e3].[Number], [e3].[IsGreen]
        FROM [JoinOneToBranch] AS [j1]
        INNER JOIN (
            SELECT [e2].[Id], [e2].[Discriminator], [e2].[Name], [e2].[Number], [e2].[IsGreen]
            FROM [EntityRoots] AS [e2]
            WHERE [e2].[Discriminator] IN (N'EntityBranch', N'EntityLeaf')
        ) AS [e3] ON [j1].[EntityBranchId] = [e3].[Id]
        WHERE [e3].[Id] < 20
    ) AS [s1] ON [e0].[Id] = [s1].[EntityOneId]
    WHERE [e0].[Id] < 10
) AS [s2] ON [e].[Id] = [s2].[ThreeId]
ORDER BY [e].[Id], [s2].[OneId], [s2].[ThreeId], [s2].[Id], [s2].[OneId0], [s2].[Id0], [s2].[TwoId], [s2].[EntityBranchId], [s2].[EntityOneId]
""");
    }

    public override async Task Filtered_include_on_skip_navigation_then_filtered_include_on_navigation(bool async)
    {
        await base.Filtered_include_on_skip_navigation_then_filtered_include_on_navigation(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId], [s].[OneId], [s].[ThreeId], [s].[Payload], [s].[Id], [s].[Name], [s].[Id0], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name0], [s].[ReferenceInverseId]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [e0].[Id], [e0].[Name], [e2].[Id] AS [Id0], [e2].[CollectionInverseId], [e2].[ExtraId], [e2].[Name] AS [Name0], [e2].[ReferenceInverseId]
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
SELECT [e].[Id], [e].[Name], [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[ReferenceInverseId], [s0].[ThreeId], [s0].[TwoId], [s0].[Id0], [s0].[CollectionInverseId0], [s0].[Name0], [s0].[ReferenceInverseId0]
FROM [EntityOnes] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], [s].[ThreeId], [s].[TwoId], [s].[Id] AS [Id0], [s].[CollectionInverseId] AS [CollectionInverseId0], [s].[Name] AS [Name0], [s].[ReferenceInverseId] AS [ReferenceInverseId0]
    FROM [EntityTwos] AS [e0]
    LEFT JOIN (
        SELECT [j].[ThreeId], [j].[TwoId], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId]
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
SELECT [s].[RootSkipSharedId], [s].[CompositeKeySkipSharedKey1], [s].[CompositeKeySkipSharedKey2], [s].[CompositeKeySkipSharedKey3], [s].[Id], [s].[Discriminator], [s].[Name], [s].[Number], [s].[Slumber], [s].[IsGreen], [s].[IsBrown], [e].[Key1], [e].[Key2], [e].[Key3]
FROM [EntityCompositeKeys] AS [e]
INNER JOIN (
    SELECT [e0].[RootSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], [e1].[Id], [e1].[Discriminator], [e1].[Name], [e1].[Number], [e1].[Slumber], [e1].[IsGreen], [e1].[IsBrown]
    FROM [EntityCompositeKeyEntityRoot] AS [e0]
    INNER JOIN [EntityRoots] AS [e1] ON [e0].[RootSkipSharedId] = [e1].[Id]
) AS [s] ON [e].[Key1] = [s].[CompositeKeySkipSharedKey1] AND [e].[Key2] = [s].[CompositeKeySkipSharedKey2] AND [e].[Key3] = [s].[CompositeKeySkipSharedKey3]
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
SELECT [s].[OneId], [s].[TwoId], [s].[JoinOneToTwoExtraId], [s].[Id], [s].[Name], [s].[Id0], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name0], [s].[ReferenceInverseId], [e].[Id]
FROM [EntityTwos] AS [e]
INNER JOIN (
    SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId]
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
SELECT [s].[LeafId], [s].[CompositeId1], [s].[CompositeId2], [s].[CompositeId3], [s].[Id], [s].[Discriminator], [s].[Name], [s].[Number], [s].[IsGreen], [e].[Key1], [e].[Key2], [e].[Key3]
FROM [EntityCompositeKeys] AS [e]
INNER JOIN (
    SELECT [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [e1].[Id], [e1].[Discriminator], [e1].[Name], [e1].[Number], [e1].[IsGreen]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN (
        SELECT [e0].[Id], [e0].[Discriminator], [e0].[Name], [e0].[Number], [e0].[IsGreen]
        FROM [EntityRoots] AS [e0]
        WHERE [e0].[Discriminator] = N'EntityLeaf'
    ) AS [e1] ON [j].[LeafId] = [e1].[Id]
) AS [s] ON [e].[Key1] = [s].[CompositeId1] AND [e].[Key2] = [s].[CompositeId2] AND [e].[Key3] = [s].[CompositeId3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [s].[LeafId], [s].[CompositeId1], [s].[CompositeId2], [s].[CompositeId3], [s].[Id]
""",
            //
            """
SELECT [s0].[EntityBranchId], [s0].[EntityOneId], [s0].[Id], [s0].[Name], [e].[Key1], [e].[Key2], [e].[Key3], [s].[LeafId], [s].[CompositeId1], [s].[CompositeId2], [s].[CompositeId3], [s].[Id]
FROM [EntityCompositeKeys] AS [e]
INNER JOIN (
    SELECT [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [e1].[Id]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN (
        SELECT [e0].[Id]
        FROM [EntityRoots] AS [e0]
        WHERE [e0].[Discriminator] = N'EntityLeaf'
    ) AS [e1] ON [j].[LeafId] = [e1].[Id]
) AS [s] ON [e].[Key1] = [s].[CompositeId1] AND [e].[Key2] = [s].[CompositeId2] AND [e].[Key3] = [s].[CompositeId3]
INNER JOIN (
    SELECT [j0].[EntityBranchId], [j0].[EntityOneId], [e2].[Id], [e2].[Name]
    FROM [JoinOneToBranch] AS [j0]
    INNER JOIN [EntityOnes] AS [e2] ON [j0].[EntityOneId] = [e2].[Id]
) AS [s0] ON [s].[Id] = [s0].[EntityBranchId]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [s].[LeafId], [s].[CompositeId1], [s].[CompositeId2], [s].[CompositeId3], [s].[Id]
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
SELECT [s].[OneId], [s].[ThreeId], [s].[Payload], [s].[Id], [s].[Name], [s].[Id0], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name0], [s].[ReferenceInverseId], [e].[Id]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
) AS [s] ON [e].[Id] = [s].[ThreeId]
ORDER BY [e].[Id], [s].[OneId], [s].[ThreeId], [s].[Id], [s].[Id0]
""",
            //
            """
SELECT [s0].[LeftId], [s0].[RightId], [s0].[Payload], [s0].[Id], [s0].[Name], [e].[Id], [s].[OneId], [s].[ThreeId], [s].[Id], [s].[Id0]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [e0].[Id], [e1].[Id] AS [Id0]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
) AS [s] ON [e].[Id] = [s].[ThreeId]
INNER JOIN (
    SELECT [j0].[LeftId], [j0].[RightId], [j0].[Payload], [e2].[Id], [e2].[Name]
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
SELECT [s].[OneSkipSharedId], [s].[TwoSkipSharedId], [s].[Id], [s].[Name], [e].[Id], [e0].[Id]
FROM [EntityTwos] AS [e]
LEFT JOIN [EntityThrees] AS [e0] ON [e].[Id] = [e0].[ReferenceInverseId]
INNER JOIN (
    SELECT [e1].[OneSkipSharedId], [e1].[TwoSkipSharedId], [e2].[Id], [e2].[Name]
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
SELECT [s].[OneId], [s].[ThreeId], [s].[Payload], [s].[Id], [s].[Name], [e].[Id]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [e0].[Id], [e0].[Name]
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
SELECT [s].[ThreeId], [s].[TwoId], [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId], [e].[Id]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [j].[ThreeId], [j].[TwoId], [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId]
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
SELECT [s0].[SelfSkipSharedLeftId], [s0].[SelfSkipSharedRightId], [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[ReferenceInverseId], [e].[Id]
FROM [EntityTwos] AS [e]
INNER JOIN (
    SELECT [s].[SelfSkipSharedLeftId], [s].[SelfSkipSharedRightId], [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId]
    FROM (
        SELECT [e0].[SelfSkipSharedLeftId], [e0].[SelfSkipSharedRightId], [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [e0].[SelfSkipSharedLeftId] ORDER BY [e1].[Id]) AS [row]
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
SELECT [s0].[TwoSkipSharedId], [s0].[CompositeKeySkipSharedKey1], [s0].[CompositeKeySkipSharedKey2], [s0].[CompositeKeySkipSharedKey3], [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[ReferenceInverseId], [e].[Key1], [e].[Key2], [e].[Key3]
FROM [EntityCompositeKeys] AS [e]
INNER JOIN (
    SELECT [s].[TwoSkipSharedId], [s].[CompositeKeySkipSharedKey1], [s].[CompositeKeySkipSharedKey2], [s].[CompositeKeySkipSharedKey3], [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId]
    FROM (
        SELECT [e0].[TwoSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3] ORDER BY [e1].[Id]) AS [row]
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
SELECT [s0].[Id], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3], [s0].[ThreeId], [s0].[Id0], [s0].[CollectionInverseId], [s0].[Name], [s0].[ReferenceInverseId], [e].[Key1], [e].[Key2], [e].[Key3]
FROM [EntityCompositeKeys] AS [e]
INNER JOIN (
    SELECT [s].[Id], [s].[CompositeId1], [s].[CompositeId2], [s].[CompositeId3], [s].[ThreeId], [s].[Id0], [s].[CollectionInverseId], [s].[Name], [s].[ReferenceInverseId]
    FROM (
        SELECT [j].[Id], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [j].[ThreeId], [e0].[Id] AS [Id0], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinThreeToCompositeKeyFull] AS [j]
        INNER JOIN [EntityThrees] AS [e0] ON [j].[ThreeId] = [e0].[Id]
    ) AS [s]
    WHERE 1 < [s].[row] AND [s].[row] <= 3
) AS [s0] ON [e].[Key1] = [s0].[CompositeId1] AND [e].[Key2] = [s0].[CompositeId2] AND [e].[Key3] = [s0].[CompositeId3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3], [s0].[Id0]
""");
    }

    public override async Task Filtered_then_include_skip_navigation_where_split(bool async)
    {
        await base.Filtered_then_include_skip_navigation_where_split(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[Number], [e].[Slumber], [e].[IsGreen], [e].[IsBrown]
FROM [EntityRoots] AS [e]
ORDER BY [e].[Id]
""",
            //
            """
SELECT [s].[RootSkipSharedId], [s].[ThreeSkipSharedId], [s].[Id], [s].[CollectionInverseId], [s].[Name], [s].[ReferenceInverseId], [e].[Id]
FROM [EntityRoots] AS [e]
INNER JOIN (
    SELECT [e0].[RootSkipSharedId], [e0].[ThreeSkipSharedId], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId]
    FROM [EntityRootEntityThree] AS [e0]
    INNER JOIN [EntityThrees] AS [e1] ON [e0].[ThreeSkipSharedId] = [e1].[Id]
) AS [s] ON [e].[Id] = [s].[RootSkipSharedId]
ORDER BY [e].[Id], [s].[RootSkipSharedId], [s].[ThreeSkipSharedId], [s].[Id]
""",
            //
            """
SELECT [s0].[OneId], [s0].[ThreeId], [s0].[Payload], [s0].[Id], [s0].[Name], [e].[Id], [s].[RootSkipSharedId], [s].[ThreeSkipSharedId], [s].[Id]
FROM [EntityRoots] AS [e]
INNER JOIN (
    SELECT [e0].[RootSkipSharedId], [e0].[ThreeSkipSharedId], [e1].[Id]
    FROM [EntityRootEntityThree] AS [e0]
    INNER JOIN [EntityThrees] AS [e1] ON [e0].[ThreeSkipSharedId] = [e1].[Id]
) AS [s] ON [e].[Id] = [s].[RootSkipSharedId]
INNER JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [e2].[Id], [e2].[Name]
    FROM [JoinOneToThreePayloadFullShared] AS [j]
    INNER JOIN [EntityOnes] AS [e2] ON [j].[OneId] = [e2].[Id]
    WHERE [e2].[Id] < 10
) AS [s0] ON [s].[Id] = [s0].[ThreeId]
ORDER BY [e].[Id], [s].[RootSkipSharedId], [s].[ThreeSkipSharedId], [s].[Id]
""");
    }

    public override async Task Filtered_then_include_skip_navigation_order_by_skip_take_split(bool async)
    {
        await base.Filtered_then_include_skip_navigation_order_by_skip_take_split(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[Number], [e].[Slumber], [e].[IsGreen], [e].[IsBrown]
FROM [EntityRoots] AS [e]
ORDER BY [e].[Id]
""",
            //
            """
SELECT [s].[RootSkipSharedId], [s].[CompositeKeySkipSharedKey1], [s].[CompositeKeySkipSharedKey2], [s].[CompositeKeySkipSharedKey3], [s].[Key1], [s].[Key2], [s].[Key3], [s].[Name], [e].[Id]
FROM [EntityRoots] AS [e]
INNER JOIN (
    SELECT [e0].[RootSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], [e1].[Key1], [e1].[Key2], [e1].[Key3], [e1].[Name]
    FROM [EntityCompositeKeyEntityRoot] AS [e0]
    INNER JOIN [EntityCompositeKeys] AS [e1] ON [e0].[CompositeKeySkipSharedKey1] = [e1].[Key1] AND [e0].[CompositeKeySkipSharedKey2] = [e1].[Key2] AND [e0].[CompositeKeySkipSharedKey3] = [e1].[Key3]
) AS [s] ON [e].[Id] = [s].[RootSkipSharedId]
ORDER BY [e].[Id], [s].[RootSkipSharedId], [s].[CompositeKeySkipSharedKey1], [s].[CompositeKeySkipSharedKey2], [s].[CompositeKeySkipSharedKey3], [s].[Key1], [s].[Key2], [s].[Key3]
""",
            //
            """
SELECT [s1].[Id], [s1].[CompositeId1], [s1].[CompositeId2], [s1].[CompositeId3], [s1].[ThreeId], [s1].[Id0], [s1].[CollectionInverseId], [s1].[Name], [s1].[ReferenceInverseId], [e].[Id], [s].[RootSkipSharedId], [s].[CompositeKeySkipSharedKey1], [s].[CompositeKeySkipSharedKey2], [s].[CompositeKeySkipSharedKey3], [s].[Key1], [s].[Key2], [s].[Key3]
FROM [EntityRoots] AS [e]
INNER JOIN (
    SELECT [e0].[RootSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], [e1].[Key1], [e1].[Key2], [e1].[Key3]
    FROM [EntityCompositeKeyEntityRoot] AS [e0]
    INNER JOIN [EntityCompositeKeys] AS [e1] ON [e0].[CompositeKeySkipSharedKey1] = [e1].[Key1] AND [e0].[CompositeKeySkipSharedKey2] = [e1].[Key2] AND [e0].[CompositeKeySkipSharedKey3] = [e1].[Key3]
) AS [s] ON [e].[Id] = [s].[RootSkipSharedId]
INNER JOIN (
    SELECT [s0].[Id], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3], [s0].[ThreeId], [s0].[Id0], [s0].[CollectionInverseId], [s0].[Name], [s0].[ReferenceInverseId]
    FROM (
        SELECT [j].[Id], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [j].[ThreeId], [e2].[Id] AS [Id0], [e2].[CollectionInverseId], [e2].[Name], [e2].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3] ORDER BY [e2].[Id]) AS [row]
        FROM [JoinThreeToCompositeKeyFull] AS [j]
        INNER JOIN [EntityThrees] AS [e2] ON [j].[ThreeId] = [e2].[Id]
    ) AS [s0]
    WHERE 1 < [s0].[row] AND [s0].[row] <= 3
) AS [s1] ON [s].[Key1] = [s1].[CompositeId1] AND [s].[Key2] = [s1].[CompositeId2] AND [s].[Key3] = [s1].[CompositeId3]
ORDER BY [e].[Id], [s].[RootSkipSharedId], [s].[CompositeKeySkipSharedKey1], [s].[CompositeKeySkipSharedKey2], [s].[CompositeKeySkipSharedKey3], [s].[Key1], [s].[Key2], [s].[Key3], [s1].[CompositeId1], [s1].[CompositeId2], [s1].[CompositeId3], [s1].[Id0]
""");
    }

    public override async Task Filtered_include_skip_navigation_where_then_include_skip_navigation_split(bool async)
    {
        await base.Filtered_include_skip_navigation_where_then_include_skip_navigation_split(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[Number], [e].[IsGreen]
FROM [EntityRoots] AS [e]
WHERE [e].[Discriminator] = N'EntityLeaf'
ORDER BY [e].[Id]
""",
            //
            """
SELECT [s].[LeafId], [s].[CompositeId1], [s].[CompositeId2], [s].[CompositeId3], [s].[Key1], [s].[Key2], [s].[Key3], [s].[Name], [e].[Id]
FROM [EntityRoots] AS [e]
INNER JOIN (
    SELECT [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [e0].[Key1], [e0].[Key2], [e0].[Key3], [e0].[Name]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN [EntityCompositeKeys] AS [e0] ON [j].[CompositeId1] = [e0].[Key1] AND [j].[CompositeId2] = [e0].[Key2] AND [j].[CompositeId3] = [e0].[Key3]
    WHERE [e0].[Key1] < 5
) AS [s] ON [e].[Id] = [s].[LeafId]
WHERE [e].[Discriminator] = N'EntityLeaf'
ORDER BY [e].[Id], [s].[LeafId], [s].[CompositeId1], [s].[CompositeId2], [s].[CompositeId3], [s].[Key1], [s].[Key2], [s].[Key3]
""",
            //
            """
SELECT [s0].[TwoSkipSharedId], [s0].[CompositeKeySkipSharedKey1], [s0].[CompositeKeySkipSharedKey2], [s0].[CompositeKeySkipSharedKey3], [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[ReferenceInverseId], [e].[Id], [s].[LeafId], [s].[CompositeId1], [s].[CompositeId2], [s].[CompositeId3], [s].[Key1], [s].[Key2], [s].[Key3]
FROM [EntityRoots] AS [e]
INNER JOIN (
    SELECT [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [e0].[Key1], [e0].[Key2], [e0].[Key3]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN [EntityCompositeKeys] AS [e0] ON [j].[CompositeId1] = [e0].[Key1] AND [j].[CompositeId2] = [e0].[Key2] AND [j].[CompositeId3] = [e0].[Key3]
    WHERE [e0].[Key1] < 5
) AS [s] ON [e].[Id] = [s].[LeafId]
INNER JOIN (
    SELECT [e1].[TwoSkipSharedId], [e1].[CompositeKeySkipSharedKey1], [e1].[CompositeKeySkipSharedKey2], [e1].[CompositeKeySkipSharedKey3], [e2].[Id], [e2].[CollectionInverseId], [e2].[ExtraId], [e2].[Name], [e2].[ReferenceInverseId]
    FROM [EntityCompositeKeyEntityTwo] AS [e1]
    INNER JOIN [EntityTwos] AS [e2] ON [e1].[TwoSkipSharedId] = [e2].[Id]
) AS [s0] ON [s].[Key1] = [s0].[CompositeKeySkipSharedKey1] AND [s].[Key2] = [s0].[CompositeKeySkipSharedKey2] AND [s].[Key3] = [s0].[CompositeKeySkipSharedKey3]
WHERE [e].[Discriminator] = N'EntityLeaf'
ORDER BY [e].[Id], [s].[LeafId], [s].[CompositeId1], [s].[CompositeId2], [s].[CompositeId3], [s].[Key1], [s].[Key2], [s].[Key3]
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
SELECT [s0].[OneId], [s0].[TwoId], [s0].[JoinOneToTwoExtraId], [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[ReferenceInverseId], [e].[Id]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [s].[OneId], [s].[TwoId], [s].[JoinOneToTwoExtraId], [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId]
    FROM (
        SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j].[OneId] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinOneToTwo] AS [j]
        INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    ) AS [s]
    WHERE 1 < [s].[row] AND [s].[row] <= 3
) AS [s0] ON [e].[Id] = [s0].[OneId]
ORDER BY [e].[Id], [s0].[OneId], [s0].[Id], [s0].[TwoId]
""",
            //
            """
SELECT [s1].[ThreeId], [s1].[TwoId], [s1].[Id], [s1].[CollectionInverseId], [s1].[Name], [s1].[ReferenceInverseId], [e].[Id], [s0].[OneId], [s0].[TwoId], [s0].[Id]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [s].[OneId], [s].[TwoId], [s].[Id]
    FROM (
        SELECT [j].[OneId], [j].[TwoId], [e0].[Id], ROW_NUMBER() OVER(PARTITION BY [j].[OneId] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinOneToTwo] AS [j]
        INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    ) AS [s]
    WHERE 1 < [s].[row] AND [s].[row] <= 3
) AS [s0] ON [e].[Id] = [s0].[OneId]
INNER JOIN (
    SELECT [j0].[ThreeId], [j0].[TwoId], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId]
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
SELECT [s].[OneId], [s].[TwoId], [s].[JoinOneToTwoExtraId], [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[ReferenceInverseId], [e].[Id]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [s] ON [e].[Id] = [s].[OneId]
ORDER BY [e].[Id], [s].[OneId], [s].[TwoId], [s].[Id]
""",
            //
            """
SELECT [s1].[ThreeId], [s1].[TwoId], [s1].[Id], [s1].[CollectionInverseId], [s1].[Name], [s1].[ReferenceInverseId], [e].[Id], [s].[OneId], [s].[TwoId], [s].[Id]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [j].[OneId], [j].[TwoId], [e0].[Id]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [s] ON [e].[Id] = [s].[OneId]
INNER JOIN (
    SELECT [s0].[ThreeId], [s0].[TwoId], [s0].[Id], [s0].[CollectionInverseId], [s0].[Name], [s0].[ReferenceInverseId]
    FROM (
        SELECT [j0].[ThreeId], [j0].[TwoId], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j0].[TwoId] ORDER BY [e1].[Id]) AS [row]
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
SELECT [s].[OneId], [s].[TwoId], [s].[JoinOneToTwoExtraId], [s].[Id], [s].[Name], [s].[Id0], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name0], [s].[ReferenceInverseId], [e].[Id]
FROM [EntityTwos] AS [e]
INNER JOIN (
    SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId]
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
    SELECT [j].[OneId], [j].[TwoId], [e0].[Id], [e1].[Id] AS [Id0]
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
SELECT [s].[OneId], [s].[ThreeId], [s].[Payload], [s].[Id], [s].[Name], [e].[Id]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [e0].[Id], [e0].[Name]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [s] ON [e].[Id] = [s].[ThreeId]
ORDER BY [e].[Id], [s].[OneId], [s].[ThreeId], [s].[Id]
""",
            //
            """
SELECT [s1].[OneId], [s1].[TwoId], [s1].[JoinOneToTwoExtraId], [s1].[Id], [s1].[CollectionInverseId], [s1].[ExtraId], [s1].[Name], [s1].[ReferenceInverseId], [e].[Id], [s].[OneId], [s].[ThreeId], [s].[Id]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [e0].[Id]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [s] ON [e].[Id] = [s].[ThreeId]
INNER JOIN (
    SELECT [s0].[OneId], [s0].[TwoId], [s0].[JoinOneToTwoExtraId], [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[ReferenceInverseId]
    FROM (
        SELECT [j0].[OneId], [j0].[TwoId], [j0].[JoinOneToTwoExtraId], [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j0].[OneId] ORDER BY [e1].[Id]) AS [row]
        FROM [JoinOneToTwo] AS [j0]
        INNER JOIN [EntityTwos] AS [e1] ON [j0].[TwoId] = [e1].[Id]
    ) AS [s0]
    WHERE 1 < [s0].[row] AND [s0].[row] <= 3
) AS [s1] ON [s].[Id] = [s1].[OneId]
ORDER BY [e].[Id], [s].[OneId], [s].[ThreeId], [s].[Id], [s1].[OneId], [s1].[Id]
""",
            //
            """
SELECT [s2].[EntityBranchId], [s2].[EntityOneId], [s2].[Id], [s2].[Discriminator], [s2].[Name], [s2].[Number], [s2].[IsGreen], [e].[Id], [s].[OneId], [s].[ThreeId], [s].[Id]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [e0].[Id]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [s] ON [e].[Id] = [s].[ThreeId]
INNER JOIN (
    SELECT [j1].[EntityBranchId], [j1].[EntityOneId], [e3].[Id], [e3].[Discriminator], [e3].[Name], [e3].[Number], [e3].[IsGreen]
    FROM [JoinOneToBranch] AS [j1]
    INNER JOIN (
        SELECT [e2].[Id], [e2].[Discriminator], [e2].[Name], [e2].[Number], [e2].[IsGreen]
        FROM [EntityRoots] AS [e2]
        WHERE [e2].[Discriminator] IN (N'EntityBranch', N'EntityLeaf')
    ) AS [e3] ON [j1].[EntityBranchId] = [e3].[Id]
    WHERE [e3].[Id] < 20
) AS [s2] ON [s].[Id] = [s2].[EntityOneId]
ORDER BY [e].[Id], [s].[OneId], [s].[ThreeId], [s].[Id]
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
SELECT [s].[OneId], [s].[ThreeId], [s].[Payload], [s].[Id], [s].[Name], [e].[Id]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [e0].[Id], [e0].[Name]
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
    SELECT [j].[OneId], [j].[ThreeId], [e0].[Id]
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
SELECT [s].[ThreeId], [s].[TwoId], [s].[Id], [s].[CollectionInverseId], [s].[Name], [s].[ReferenceInverseId], [e].[Id], [e2].[Id]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId]
    FROM [EntityTwos] AS [e0]
    WHERE [e0].[Id] > 15
) AS [e2] ON [e].[Id] = [e2].[CollectionInverseId]
INNER JOIN (
    SELECT [j].[ThreeId], [j].[TwoId], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId]
    FROM [JoinTwoToThree] AS [j]
    INNER JOIN [EntityThrees] AS [e1] ON [j].[ThreeId] = [e1].[Id]
    WHERE [e1].[Id] < 5
) AS [s] ON [e2].[Id] = [s].[TwoId]
ORDER BY [e].[Id], [e2].[Id]
""");
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

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
