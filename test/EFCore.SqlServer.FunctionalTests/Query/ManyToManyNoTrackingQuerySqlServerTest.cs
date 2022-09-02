// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class ManyToManyNoTrackingQuerySqlServerTest
    : ManyToManyNoTrackingQueryRelationalTestBase<ManyToManyQuerySqlServerFixture>
{
    public ManyToManyNoTrackingQuerySqlServerTest(ManyToManyQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override bool CanExecuteQueryString
        => true;

    public override async Task Skip_navigation_all(bool async)
    {
        await base.Skip_navigation_all(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[Name]
FROM [EntityOnes] AS [e]
WHERE NOT EXISTS (
    SELECT 1
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    WHERE [e].[Id] = [j].[OneId] AND NOT ([e0].[Name] LIKE N'%B%'))");
    }

    public override async Task Skip_navigation_any_without_predicate(bool async)
    {
        await base.Skip_navigation_any_without_predicate(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[Name]
FROM [EntityOnes] AS [e]
WHERE EXISTS (
    SELECT 1
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityThrees] AS [e0] ON [j].[ThreeId] = [e0].[Id]
    WHERE [e].[Id] = [j].[OneId] AND ([e0].[Name] LIKE N'%B%'))");
    }

    public override async Task Skip_navigation_any_with_predicate(bool async)
    {
        await base.Skip_navigation_any_with_predicate(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[Name]
FROM [EntityOnes] AS [e]
WHERE EXISTS (
    SELECT 1
    FROM [EntityOneEntityTwo] AS [e0]
    INNER JOIN [EntityTwos] AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
    WHERE [e].[Id] = [e0].[OneSkipSharedId] AND ([e1].[Name] LIKE N'%B%'))");
    }

    public override async Task Skip_navigation_contains(bool async)
    {
        await base.Skip_navigation_contains(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[Name]
FROM [EntityOnes] AS [e]
WHERE EXISTS (
    SELECT 1
    FROM [JoinOneToThreePayloadFullShared] AS [j]
    INNER JOIN [EntityThrees] AS [e0] ON [j].[ThreeId] = [e0].[Id]
    WHERE [e].[Id] = [j].[OneId] AND [e0].[Id] = 1)");
    }

    public override async Task Skip_navigation_count_without_predicate(bool async)
    {
        await base.Skip_navigation_count_without_predicate(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[Name]
FROM [EntityOnes] AS [e]
WHERE (
    SELECT COUNT(*)
    FROM [JoinOneSelfPayload] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[LeftId] = [e0].[Id]
    WHERE [e].[Id] = [j].[RightId]) > 0");
    }

    public override async Task Skip_navigation_count_with_predicate(bool async)
    {
        await base.Skip_navigation_count_with_predicate(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[Name]
FROM [EntityOnes] AS [e]
ORDER BY (
    SELECT COUNT(*)
    FROM [JoinOneToBranch] AS [j]
    INNER JOIN (
        SELECT [e0].[Id], [e0].[Discriminator], [e0].[Name], [e0].[Number], [e0].[IsGreen]
        FROM [EntityRoots] AS [e0]
        WHERE [e0].[Discriminator] IN (N'EntityBranch', N'EntityLeaf')
    ) AS [t] ON [j].[EntityBranchId] = [t].[Id]
    WHERE [e].[Id] = [j].[EntityOneId] AND [t].[Name] IS NOT NULL AND ([t].[Name] LIKE N'L%')), [e].[Id]");
    }

    public override async Task Skip_navigation_long_count_without_predicate(bool async)
    {
        await base.Skip_navigation_long_count_without_predicate(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId]
FROM [EntityTwos] AS [e]
WHERE (
    SELECT COUNT_BIG(*)
    FROM [JoinTwoToThree] AS [j]
    INNER JOIN [EntityThrees] AS [e0] ON [j].[ThreeId] = [e0].[Id]
    WHERE [e].[Id] = [j].[TwoId]) > CAST(0 AS bigint)");
    }

    public override async Task Skip_navigation_long_count_with_predicate(bool async)
    {
        await base.Skip_navigation_long_count_with_predicate(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId]
FROM [EntityTwos] AS [e]
ORDER BY (
    SELECT COUNT_BIG(*)
    FROM [EntityTwoEntityTwo] AS [e0]
    INNER JOIN [EntityTwos] AS [e1] ON [e0].[SelfSkipSharedLeftId] = [e1].[Id]
    WHERE [e].[Id] = [e0].[SelfSkipSharedRightId] AND [e1].[Name] IS NOT NULL AND ([e1].[Name] LIKE N'L%')) DESC, [e].[Id]");
    }

    public override async Task Skip_navigation_select_many_average(bool async)
    {
        await base.Skip_navigation_select_many_average(async);

        AssertSql(
            @"SELECT AVG(CAST([t].[Key1] AS float))
FROM [EntityTwos] AS [e]
INNER JOIN (
    SELECT [e1].[Key1], [e0].[TwoSkipSharedId]
    FROM [EntityCompositeKeyEntityTwo] AS [e0]
    INNER JOIN [EntityCompositeKeys] AS [e1] ON [e0].[CompositeKeySkipSharedKey1] = [e1].[Key1] AND [e0].[CompositeKeySkipSharedKey2] = [e1].[Key2] AND [e0].[CompositeKeySkipSharedKey3] = [e1].[Key3]
) AS [t] ON [e].[Id] = [t].[TwoSkipSharedId]");
    }

    public override async Task Skip_navigation_select_many_max(bool async)
    {
        await base.Skip_navigation_select_many_max(async);

        AssertSql(
            @"SELECT MAX([t].[Key1])
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [e0].[Key1], [j].[ThreeId]
    FROM [JoinThreeToCompositeKeyFull] AS [j]
    INNER JOIN [EntityCompositeKeys] AS [e0] ON [j].[CompositeId1] = [e0].[Key1] AND [j].[CompositeId2] = [e0].[Key2] AND [j].[CompositeId3] = [e0].[Key3]
) AS [t] ON [e].[Id] = [t].[ThreeId]");
    }

    public override async Task Skip_navigation_select_many_min(bool async)
    {
        await base.Skip_navigation_select_many_min(async);

        AssertSql(
            @"SELECT MIN([t].[Id])
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [e1].[Id], [e0].[ThreeSkipSharedId]
    FROM [EntityRootEntityThree] AS [e0]
    INNER JOIN [EntityRoots] AS [e1] ON [e0].[RootSkipSharedId] = [e1].[Id]
) AS [t] ON [e].[Id] = [t].[ThreeSkipSharedId]");
    }

    public override async Task Skip_navigation_select_many_sum(bool async)
    {
        await base.Skip_navigation_select_many_sum(async);

        AssertSql(
            @"SELECT COALESCE(SUM([t].[Key1]), 0)
FROM [EntityRoots] AS [e]
INNER JOIN (
    SELECT [e1].[Key1], [e0].[RootSkipSharedId]
    FROM [EntityCompositeKeyEntityRoot] AS [e0]
    INNER JOIN [EntityCompositeKeys] AS [e1] ON [e0].[CompositeKeySkipSharedKey1] = [e1].[Key1] AND [e0].[CompositeKeySkipSharedKey2] = [e1].[Key2] AND [e0].[CompositeKeySkipSharedKey3] = [e1].[Key3]
) AS [t] ON [e].[Id] = [t].[RootSkipSharedId]");
    }

    public override async Task Skip_navigation_select_subquery_average(bool async)
    {
        await base.Skip_navigation_select_subquery_average(async);

        AssertSql(
            @"SELECT (
    SELECT AVG(CAST([e0].[Key1] AS float))
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN [EntityCompositeKeys] AS [e0] ON [j].[CompositeId1] = [e0].[Key1] AND [j].[CompositeId2] = [e0].[Key2] AND [j].[CompositeId3] = [e0].[Key3]
    WHERE [e].[Id] = [j].[LeafId])
FROM [EntityRoots] AS [e]
WHERE [e].[Discriminator] = N'EntityLeaf'");
    }

    public override async Task Skip_navigation_select_subquery_max(bool async)
    {
        await base.Skip_navigation_select_subquery_max(async);

        AssertSql(
            @"SELECT (
    SELECT MAX([e0].[Id])
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e].[Id] = [j].[TwoId])
FROM [EntityTwos] AS [e]");
    }

    public override async Task Skip_navigation_select_subquery_min(bool async)
    {
        await base.Skip_navigation_select_subquery_min(async);

        AssertSql(
            @"SELECT (
    SELECT MIN([e0].[Id])
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e].[Id] = [j].[ThreeId])
FROM [EntityThrees] AS [e]");
    }

    public override async Task Skip_navigation_select_subquery_sum(bool async)
    {
        await base.Skip_navigation_select_subquery_sum(async);

        AssertSql(
            @"SELECT (
    SELECT COALESCE(SUM([e1].[Id]), 0)
    FROM [EntityOneEntityTwo] AS [e0]
    INNER JOIN [EntityOnes] AS [e1] ON [e0].[OneSkipSharedId] = [e1].[Id]
    WHERE [e].[Id] = [e0].[TwoSkipSharedId])
FROM [EntityTwos] AS [e]");
    }

    public override async Task Skip_navigation_order_by_first_or_default(bool async)
    {
        await base.Skip_navigation_order_by_first_or_default(async);

        AssertSql(
            @"SELECT [t0].[Id], [t0].[Name]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [t].[Id], [t].[Name], [t].[ThreeId]
    FROM (
        SELECT [e0].[Id], [e0].[Name], [j].[ThreeId], ROW_NUMBER() OVER(PARTITION BY [j].[ThreeId] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinOneToThreePayloadFullShared] AS [j]
        INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [e].[Id] = [t0].[ThreeId]");
    }

    public override async Task Skip_navigation_order_by_single_or_default(bool async)
    {
        await base.Skip_navigation_order_by_single_or_default(async);

        AssertSql(
            @"SELECT [t0].[Id], [t0].[Name]
FROM [EntityOnes] AS [e]
OUTER APPLY (
    SELECT TOP(1) [t].[Id], [t].[Name]
    FROM (
        SELECT TOP(1) [e0].[Id], [e0].[Name]
        FROM [JoinOneSelfPayload] AS [j]
        INNER JOIN [EntityOnes] AS [e0] ON [j].[RightId] = [e0].[Id]
        WHERE [e].[Id] = [j].[LeftId]
        ORDER BY [e0].[Id]
    ) AS [t]
    ORDER BY [t].[Id]
) AS [t0]");
    }

    public override async Task Skip_navigation_order_by_last_or_default(bool async)
    {
        await base.Skip_navigation_order_by_last_or_default(async);

        AssertSql(
            @"SELECT [t0].[Id], [t0].[Name]
FROM [EntityRoots] AS [e]
LEFT JOIN (
    SELECT [t].[Id], [t].[Name], [t].[EntityBranchId]
    FROM (
        SELECT [e0].[Id], [e0].[Name], [j].[EntityBranchId], ROW_NUMBER() OVER(PARTITION BY [j].[EntityBranchId] ORDER BY [e0].[Id] DESC) AS [row]
        FROM [JoinOneToBranch] AS [j]
        INNER JOIN [EntityOnes] AS [e0] ON [j].[EntityOneId] = [e0].[Id]
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [e].[Id] = [t0].[EntityBranchId]
WHERE [e].[Discriminator] IN (N'EntityBranch', N'EntityLeaf')");
    }

    public override async Task Skip_navigation_order_by_reverse_first_or_default(bool async)
    {
        await base.Skip_navigation_order_by_reverse_first_or_default(async);

        AssertSql(
            @"SELECT [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId], [t].[ThreeId]
    FROM (
        SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], [j].[ThreeId], ROW_NUMBER() OVER(PARTITION BY [j].[ThreeId] ORDER BY [e0].[Id] DESC) AS [row]
        FROM [JoinTwoToThree] AS [j]
        INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [e].[Id] = [t0].[ThreeId]");
    }

    public override async Task Skip_navigation_cast(bool async)
    {
        await base.Skip_navigation_cast(async);

        AssertSql(
            @"SELECT [e].[Key1], [e].[Key2], [e].[Key3], [t0].[Id], [t0].[Discriminator], [t0].[Name], [t0].[Number], [t0].[IsGreen], [t0].[LeafId], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t].[Number], [t].[IsGreen], [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN (
        SELECT [e0].[Id], [e0].[Discriminator], [e0].[Name], [e0].[Number], [e0].[IsGreen]
        FROM [EntityRoots] AS [e0]
        WHERE [e0].[Discriminator] = N'EntityLeaf'
    ) AS [t] ON [j].[LeafId] = [t].[Id]
) AS [t0] ON [e].[Key1] = [t0].[CompositeId1] AND [e].[Key2] = [t0].[CompositeId2] AND [e].[Key3] = [t0].[CompositeId3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [t0].[LeafId], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3]");
    }

    public override async Task Skip_navigation_of_type(bool async)
    {
        await base.Skip_navigation_of_type(async);

        AssertSql(
            @"SELECT [e].[Key1], [e].[Key2], [e].[Key3], [t].[Id], [t].[Discriminator], [t].[Name], [t].[Number], [t].[IsGreen], [t].[RootSkipSharedId], [t].[CompositeKeySkipSharedKey1], [t].[CompositeKeySkipSharedKey2], [t].[CompositeKeySkipSharedKey3]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [e1].[Id], [e1].[Discriminator], [e1].[Name], [e1].[Number], [e1].[IsGreen], [e0].[RootSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3]
    FROM [EntityCompositeKeyEntityRoot] AS [e0]
    INNER JOIN [EntityRoots] AS [e1] ON [e0].[RootSkipSharedId] = [e1].[Id]
    WHERE [e1].[Discriminator] = N'EntityLeaf'
) AS [t] ON [e].[Key1] = [t].[CompositeKeySkipSharedKey1] AND [e].[Key2] = [t].[CompositeKeySkipSharedKey2] AND [e].[Key3] = [t].[CompositeKeySkipSharedKey3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [t].[RootSkipSharedId], [t].[CompositeKeySkipSharedKey1], [t].[CompositeKeySkipSharedKey2], [t].[CompositeKeySkipSharedKey3]");
    }

    public override async Task Join_with_skip_navigation(bool async)
    {
        await base.Join_with_skip_navigation(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId], [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId]
FROM [EntityTwos] AS [e]
INNER JOIN [EntityTwos] AS [e0] ON [e].[Id] = (
    SELECT TOP(1) [e2].[Id]
    FROM [EntityTwoEntityTwo] AS [e1]
    INNER JOIN [EntityTwos] AS [e2] ON [e1].[SelfSkipSharedRightId] = [e2].[Id]
    WHERE [e0].[Id] = [e1].[SelfSkipSharedLeftId]
    ORDER BY [e2].[Id])");
    }

    public override async Task Left_join_with_skip_navigation(bool async)
    {
        await base.Left_join_with_skip_navigation(async);

        AssertSql(
            @"SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [e0].[Key1], [e0].[Key2], [e0].[Key3], [e0].[Name]
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
ORDER BY [e].[Key1], [e0].[Key1], [e].[Key2], [e0].[Key2]");
    }

    public override async Task Select_many_over_skip_navigation(bool async)
    {
        await base.Select_many_over_skip_navigation(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[CollectionInverseId], [t].[Name], [t].[ReferenceInverseId]
FROM [EntityRoots] AS [e]
INNER JOIN (
    SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], [e0].[RootSkipSharedId]
    FROM [EntityRootEntityThree] AS [e0]
    INNER JOIN [EntityThrees] AS [e1] ON [e0].[ThreeSkipSharedId] = [e1].[Id]
) AS [t] ON [e].[Id] = [t].[RootSkipSharedId]");
    }

    public override async Task Select_many_over_skip_navigation_where(bool async)
    {
        await base.Select_many_over_skip_navigation_where(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId]
FROM [EntityOnes] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], [j].[OneId]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
) AS [t] ON [e].[Id] = [t].[OneId]");
    }

    public override async Task Select_many_over_skip_navigation_order_by_skip(bool async)
    {
        await base.Select_many_over_skip_navigation_order_by_skip(async);

        AssertSql(
            @"SELECT [t0].[Id], [t0].[CollectionInverseId], [t0].[Name], [t0].[ReferenceInverseId]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [t].[Id], [t].[CollectionInverseId], [t].[Name], [t].[ReferenceInverseId], [t].[OneId]
    FROM (
        SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId], [j].[OneId], ROW_NUMBER() OVER(PARTITION BY [j].[OneId] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinOneToThreePayloadFull] AS [j]
        INNER JOIN [EntityThrees] AS [e0] ON [j].[ThreeId] = [e0].[Id]
    ) AS [t]
    WHERE 2 < [t].[row]
) AS [t0] ON [e].[Id] = [t0].[OneId]");
    }

    public override async Task Select_many_over_skip_navigation_order_by_take(bool async)
    {
        await base.Select_many_over_skip_navigation_order_by_take(async);

        AssertSql(
            @"SELECT [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId], [t].[OneSkipSharedId]
    FROM (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], [e0].[OneSkipSharedId], ROW_NUMBER() OVER(PARTITION BY [e0].[OneSkipSharedId] ORDER BY [e1].[Id]) AS [row]
        FROM [EntityOneEntityTwo] AS [e0]
        INNER JOIN [EntityTwos] AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
    ) AS [t]
    WHERE [t].[row] <= 2
) AS [t0] ON [e].[Id] = [t0].[OneSkipSharedId]");
    }

    public override async Task Select_many_over_skip_navigation_order_by_skip_take(bool async)
    {
        await base.Select_many_over_skip_navigation_order_by_skip_take(async);

        AssertSql(
            @"SELECT [t0].[Id], [t0].[CollectionInverseId], [t0].[Name], [t0].[ReferenceInverseId]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [t].[Id], [t].[CollectionInverseId], [t].[Name], [t].[ReferenceInverseId], [t].[OneId]
    FROM (
        SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId], [j].[OneId], ROW_NUMBER() OVER(PARTITION BY [j].[OneId] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinOneToThreePayloadFullShared] AS [j]
        INNER JOIN [EntityThrees] AS [e0] ON [j].[ThreeId] = [e0].[Id]
    ) AS [t]
    WHERE 2 < [t].[row] AND [t].[row] <= 5
) AS [t0] ON [e].[Id] = [t0].[OneId]");
    }

    public override async Task Select_many_over_skip_navigation_of_type(bool async)
    {
        await base.Select_many_over_skip_navigation_of_type(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t].[Number], [t].[IsGreen]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [e1].[Id], [e1].[Discriminator], [e1].[Name], [e1].[Number], [e1].[IsGreen], [e0].[ThreeSkipSharedId]
    FROM [EntityRootEntityThree] AS [e0]
    INNER JOIN [EntityRoots] AS [e1] ON [e0].[RootSkipSharedId] = [e1].[Id]
    WHERE [e1].[Discriminator] IN (N'EntityBranch', N'EntityLeaf')
) AS [t] ON [e].[Id] = [t].[ThreeSkipSharedId]");
    }

    public override async Task Select_many_over_skip_navigation_cast(bool async)
    {
        await base.Select_many_over_skip_navigation_cast(async);

        AssertSql(
            @"SELECT [t0].[Id], [t0].[Discriminator], [t0].[Name], [t0].[Number], [t0].[IsGreen]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t].[Number], [t].[IsGreen], [j].[EntityOneId]
    FROM [JoinOneToBranch] AS [j]
    INNER JOIN (
        SELECT [e0].[Id], [e0].[Discriminator], [e0].[Name], [e0].[Number], [e0].[IsGreen]
        FROM [EntityRoots] AS [e0]
        WHERE [e0].[Discriminator] IN (N'EntityBranch', N'EntityLeaf')
    ) AS [t] ON [j].[EntityBranchId] = [t].[Id]
) AS [t0] ON [e].[Id] = [t0].[EntityOneId]");
    }

    public override async Task Select_skip_navigation(bool async)
    {
        await base.Select_skip_navigation(async);

        AssertSql(
            @"SELECT [e].[Id], [t].[Id], [t].[Name], [t].[LeftId], [t].[RightId]
FROM [EntityOnes] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[Name], [j].[LeftId], [j].[RightId]
    FROM [JoinOneSelfPayload] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[LeftId] = [e0].[Id]
) AS [t] ON [e].[Id] = [t].[RightId]
ORDER BY [e].[Id], [t].[LeftId], [t].[RightId]");
    }

    public override async Task Select_skip_navigation_multiple(bool async)
    {
        await base.Select_skip_navigation_multiple(async);

        AssertSql(
            @"SELECT [e].[Id], [t].[Id], [t].[CollectionInverseId], [t].[Name], [t].[ReferenceInverseId], [t].[ThreeId], [t].[TwoId], [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId], [t0].[SelfSkipSharedLeftId], [t0].[SelfSkipSharedRightId], [t1].[Key1], [t1].[Key2], [t1].[Key3], [t1].[Name], [t1].[TwoSkipSharedId], [t1].[CompositeKeySkipSharedKey1], [t1].[CompositeKeySkipSharedKey2], [t1].[CompositeKeySkipSharedKey3]
FROM [EntityTwos] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId], [j].[ThreeId], [j].[TwoId]
    FROM [JoinTwoToThree] AS [j]
    INNER JOIN [EntityThrees] AS [e0] ON [j].[ThreeId] = [e0].[Id]
) AS [t] ON [e].[Id] = [t].[TwoId]
LEFT JOIN (
    SELECT [e2].[Id], [e2].[CollectionInverseId], [e2].[ExtraId], [e2].[Name], [e2].[ReferenceInverseId], [e1].[SelfSkipSharedLeftId], [e1].[SelfSkipSharedRightId]
    FROM [EntityTwoEntityTwo] AS [e1]
    INNER JOIN [EntityTwos] AS [e2] ON [e1].[SelfSkipSharedLeftId] = [e2].[Id]
) AS [t0] ON [e].[Id] = [t0].[SelfSkipSharedRightId]
LEFT JOIN (
    SELECT [e4].[Key1], [e4].[Key2], [e4].[Key3], [e4].[Name], [e3].[TwoSkipSharedId], [e3].[CompositeKeySkipSharedKey1], [e3].[CompositeKeySkipSharedKey2], [e3].[CompositeKeySkipSharedKey3]
    FROM [EntityCompositeKeyEntityTwo] AS [e3]
    INNER JOIN [EntityCompositeKeys] AS [e4] ON [e3].[CompositeKeySkipSharedKey1] = [e4].[Key1] AND [e3].[CompositeKeySkipSharedKey2] = [e4].[Key2] AND [e3].[CompositeKeySkipSharedKey3] = [e4].[Key3]
) AS [t1] ON [e].[Id] = [t1].[TwoSkipSharedId]
ORDER BY [e].[Id], [t].[ThreeId], [t].[TwoId], [t].[Id], [t0].[SelfSkipSharedLeftId], [t0].[SelfSkipSharedRightId], [t0].[Id], [t1].[TwoSkipSharedId], [t1].[CompositeKeySkipSharedKey1], [t1].[CompositeKeySkipSharedKey2], [t1].[CompositeKeySkipSharedKey3], [t1].[Key1], [t1].[Key2]");
    }

    public override async Task Select_skip_navigation_first_or_default(bool async)
    {
        await base.Select_skip_navigation_first_or_default(async);

        AssertSql(
            @"SELECT [t0].[Key1], [t0].[Key2], [t0].[Key3], [t0].[Name]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [t].[Key1], [t].[Key2], [t].[Key3], [t].[Name], [t].[ThreeId]
    FROM (
        SELECT [e0].[Key1], [e0].[Key2], [e0].[Key3], [e0].[Name], [j].[ThreeId], ROW_NUMBER() OVER(PARTITION BY [j].[ThreeId] ORDER BY [e0].[Key1], [e0].[Key2]) AS [row]
        FROM [JoinThreeToCompositeKeyFull] AS [j]
        INNER JOIN [EntityCompositeKeys] AS [e0] ON [j].[CompositeId1] = [e0].[Key1] AND [j].[CompositeId2] = [e0].[Key2] AND [j].[CompositeId3] = [e0].[Key3]
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [e].[Id] = [t0].[ThreeId]
ORDER BY [e].[Id]");
    }

    public override async Task Include_skip_navigation(bool async)
    {
        await base.Include_skip_navigation(async);

        AssertSql(
            @"SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [t].[Id], [t].[Discriminator], [t].[Name], [t].[Number], [t].[Slumber], [t].[IsGreen], [t].[IsBrown], [t].[RootSkipSharedId], [t].[CompositeKeySkipSharedKey1], [t].[CompositeKeySkipSharedKey2], [t].[CompositeKeySkipSharedKey3]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [e1].[Id], [e1].[Discriminator], [e1].[Name], [e1].[Number], [e1].[Slumber], [e1].[IsGreen], [e1].[IsBrown], [e0].[RootSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3]
    FROM [EntityCompositeKeyEntityRoot] AS [e0]
    INNER JOIN [EntityRoots] AS [e1] ON [e0].[RootSkipSharedId] = [e1].[Id]
) AS [t] ON [e].[Key1] = [t].[CompositeKeySkipSharedKey1] AND [e].[Key2] = [t].[CompositeKeySkipSharedKey2] AND [e].[Key3] = [t].[CompositeKeySkipSharedKey3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [t].[RootSkipSharedId], [t].[CompositeKeySkipSharedKey1], [t].[CompositeKeySkipSharedKey2], [t].[CompositeKeySkipSharedKey3]");
    }

    public override async Task Include_skip_navigation_then_reference(bool async)
    {
        await base.Include_skip_navigation_then_reference(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId], [t].[Id], [t].[Name], [t].[Id0], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name0], [t].[ReferenceInverseId], [t].[OneId], [t].[TwoId]
FROM [EntityTwos] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId], [j].[OneId], [j].[TwoId]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
) AS [t] ON [e].[Id] = [t].[TwoId]
ORDER BY [e].[Id], [t].[OneId], [t].[TwoId], [t].[Id]");
    }

    public override async Task Include_skip_navigation_then_include_skip_navigation(bool async)
    {
        await base.Include_skip_navigation_then_include_skip_navigation(async);

        AssertSql(
            @"SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [t1].[Id], [t1].[Discriminator], [t1].[Name], [t1].[Number], [t1].[IsGreen], [t1].[LeafId], [t1].[CompositeId1], [t1].[CompositeId2], [t1].[CompositeId3], [t1].[Id0], [t1].[Name0], [t1].[EntityBranchId], [t1].[EntityOneId]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t].[Number], [t].[IsGreen], [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [t0].[Id] AS [Id0], [t0].[Name] AS [Name0], [t0].[EntityBranchId], [t0].[EntityOneId]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN (
        SELECT [e0].[Id], [e0].[Discriminator], [e0].[Name], [e0].[Number], [e0].[IsGreen]
        FROM [EntityRoots] AS [e0]
        WHERE [e0].[Discriminator] = N'EntityLeaf'
    ) AS [t] ON [j].[LeafId] = [t].[Id]
    LEFT JOIN (
        SELECT [e1].[Id], [e1].[Name], [j0].[EntityBranchId], [j0].[EntityOneId]
        FROM [JoinOneToBranch] AS [j0]
        INNER JOIN [EntityOnes] AS [e1] ON [j0].[EntityOneId] = [e1].[Id]
    ) AS [t0] ON [t].[Id] = [t0].[EntityBranchId]
) AS [t1] ON [e].[Key1] = [t1].[CompositeId1] AND [e].[Key2] = [t1].[CompositeId2] AND [e].[Key3] = [t1].[CompositeId3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [t1].[LeafId], [t1].[CompositeId1], [t1].[CompositeId2], [t1].[CompositeId3], [t1].[Id], [t1].[EntityBranchId], [t1].[EntityOneId]");
    }

    public override async Task Include_skip_navigation_then_include_reference_and_skip_navigation(bool async)
    {
        await base.Include_skip_navigation_then_include_reference_and_skip_navigation(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId], [t0].[Id], [t0].[Name], [t0].[Id0], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name0], [t0].[ReferenceInverseId], [t0].[OneId], [t0].[ThreeId], [t0].[Id1], [t0].[Name1], [t0].[LeftId], [t0].[RightId]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId], [j].[OneId], [j].[ThreeId], [t].[Id] AS [Id1], [t].[Name] AS [Name1], [t].[LeftId], [t].[RightId]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
    LEFT JOIN (
        SELECT [e2].[Id], [e2].[Name], [j0].[LeftId], [j0].[RightId]
        FROM [JoinOneSelfPayload] AS [j0]
        INNER JOIN [EntityOnes] AS [e2] ON [j0].[RightId] = [e2].[Id]
    ) AS [t] ON [e0].[Id] = [t].[LeftId]
) AS [t0] ON [e].[Id] = [t0].[ThreeId]
ORDER BY [e].[Id], [t0].[OneId], [t0].[ThreeId], [t0].[Id], [t0].[Id0], [t0].[LeftId], [t0].[RightId]");
    }

    public override async Task Include_skip_navigation_and_reference(bool async)
    {
        await base.Include_skip_navigation_and_reference(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId], [e0].[Id], [t].[Id], [t].[Name], [t].[OneSkipSharedId], [t].[TwoSkipSharedId], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId]
FROM [EntityTwos] AS [e]
LEFT JOIN [EntityThrees] AS [e0] ON [e].[Id] = [e0].[ReferenceInverseId]
LEFT JOIN (
    SELECT [e2].[Id], [e2].[Name], [e1].[OneSkipSharedId], [e1].[TwoSkipSharedId]
    FROM [EntityOneEntityTwo] AS [e1]
    INNER JOIN [EntityOnes] AS [e2] ON [e1].[OneSkipSharedId] = [e2].[Id]
) AS [t] ON [e].[Id] = [t].[TwoSkipSharedId]
ORDER BY [e].[Id], [e0].[Id], [t].[OneSkipSharedId], [t].[TwoSkipSharedId]");
    }

    public override async Task Filtered_include_skip_navigation_where(bool async)
    {
        await base.Filtered_include_skip_navigation_where(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId], [t].[Id], [t].[Name], [t].[OneId], [t].[ThreeId]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[Name], [j].[OneId], [j].[ThreeId]
    FROM [JoinOneToThreePayloadFullShared] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [t] ON [e].[Id] = [t].[ThreeId]
ORDER BY [e].[Id], [t].[OneId], [t].[ThreeId]");
    }

    public override async Task Filtered_include_skip_navigation_order_by(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId], [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId], [t].[ThreeId], [t].[TwoId]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], [j].[ThreeId], [j].[TwoId]
    FROM [JoinTwoToThree] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
) AS [t] ON [e].[Id] = [t].[ThreeId]
ORDER BY [e].[Id], [t].[Id], [t].[ThreeId]");
    }

    public override async Task Filtered_include_skip_navigation_order_by_skip(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId], [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId], [t0].[SelfSkipSharedLeftId], [t0].[SelfSkipSharedRightId]
FROM [EntityTwos] AS [e]
LEFT JOIN (
    SELECT [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId], [t].[SelfSkipSharedLeftId], [t].[SelfSkipSharedRightId]
    FROM (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], [e0].[SelfSkipSharedLeftId], [e0].[SelfSkipSharedRightId], ROW_NUMBER() OVER(PARTITION BY [e0].[SelfSkipSharedLeftId] ORDER BY [e1].[Id]) AS [row]
        FROM [EntityTwoEntityTwo] AS [e0]
        INNER JOIN [EntityTwos] AS [e1] ON [e0].[SelfSkipSharedRightId] = [e1].[Id]
    ) AS [t]
    WHERE 2 < [t].[row]
) AS [t0] ON [e].[Id] = [t0].[SelfSkipSharedLeftId]
ORDER BY [e].[Id], [t0].[SelfSkipSharedLeftId], [t0].[Id]");
    }

    public override async Task Filtered_include_skip_navigation_order_by_take(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_take(async);

        AssertSql(
            @"SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId], [t0].[TwoSkipSharedId], [t0].[CompositeKeySkipSharedKey1], [t0].[CompositeKeySkipSharedKey2], [t0].[CompositeKeySkipSharedKey3]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId], [t].[TwoSkipSharedId], [t].[CompositeKeySkipSharedKey1], [t].[CompositeKeySkipSharedKey2], [t].[CompositeKeySkipSharedKey3]
    FROM (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], [e0].[TwoSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], ROW_NUMBER() OVER(PARTITION BY [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3] ORDER BY [e1].[Id]) AS [row]
        FROM [EntityCompositeKeyEntityTwo] AS [e0]
        INNER JOIN [EntityTwos] AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
    ) AS [t]
    WHERE [t].[row] <= 2
) AS [t0] ON [e].[Key1] = [t0].[CompositeKeySkipSharedKey1] AND [e].[Key2] = [t0].[CompositeKeySkipSharedKey2] AND [e].[Key3] = [t0].[CompositeKeySkipSharedKey3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [t0].[CompositeKeySkipSharedKey1], [t0].[CompositeKeySkipSharedKey2], [t0].[CompositeKeySkipSharedKey3], [t0].[Id]");
    }

    public override async Task Filtered_include_skip_navigation_order_by_skip_take(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip_take(async);

        AssertSql(
            @"SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [t0].[Id], [t0].[CollectionInverseId], [t0].[Name], [t0].[ReferenceInverseId], [t0].[Id0]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [t].[Id], [t].[CollectionInverseId], [t].[Name], [t].[ReferenceInverseId], [t].[Id0], [t].[CompositeId1], [t].[CompositeId2], [t].[CompositeId3]
    FROM (
        SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId], [j].[Id] AS [Id0], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], ROW_NUMBER() OVER(PARTITION BY [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinThreeToCompositeKeyFull] AS [j]
        INNER JOIN [EntityThrees] AS [e0] ON [j].[ThreeId] = [e0].[Id]
    ) AS [t]
    WHERE 1 < [t].[row] AND [t].[row] <= 3
) AS [t0] ON [e].[Key1] = [t0].[CompositeId1] AND [e].[Key2] = [t0].[CompositeId2] AND [e].[Key3] = [t0].[CompositeId3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[Id]");
    }

    public override async Task Filtered_then_include_skip_navigation_where(bool async)
    {
        await base.Filtered_then_include_skip_navigation_where(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[Number], [e].[Slumber], [e].[IsGreen], [e].[IsBrown], [t0].[Id], [t0].[CollectionInverseId], [t0].[Name], [t0].[ReferenceInverseId], [t0].[RootSkipSharedId], [t0].[ThreeSkipSharedId], [t0].[Id0], [t0].[Name0], [t0].[OneId], [t0].[ThreeId]
FROM [EntityRoots] AS [e]
LEFT JOIN (
    SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], [e0].[RootSkipSharedId], [e0].[ThreeSkipSharedId], [t].[Id] AS [Id0], [t].[Name] AS [Name0], [t].[OneId], [t].[ThreeId]
    FROM [EntityRootEntityThree] AS [e0]
    INNER JOIN [EntityThrees] AS [e1] ON [e0].[ThreeSkipSharedId] = [e1].[Id]
    LEFT JOIN (
        SELECT [e2].[Id], [e2].[Name], [j].[OneId], [j].[ThreeId]
        FROM [JoinOneToThreePayloadFullShared] AS [j]
        INNER JOIN [EntityOnes] AS [e2] ON [j].[OneId] = [e2].[Id]
        WHERE [e2].[Id] < 10
    ) AS [t] ON [e1].[Id] = [t].[ThreeId]
) AS [t0] ON [e].[Id] = [t0].[RootSkipSharedId]
ORDER BY [e].[Id], [t0].[RootSkipSharedId], [t0].[ThreeSkipSharedId], [t0].[Id], [t0].[OneId], [t0].[ThreeId]");
    }

    public override async Task Filtered_then_include_skip_navigation_order_by_skip_take(bool async)
    {
        await base.Filtered_then_include_skip_navigation_order_by_skip_take(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[Number], [e].[Slumber], [e].[IsGreen], [e].[IsBrown], [t1].[Key1], [t1].[Key2], [t1].[Key3], [t1].[Name], [t1].[RootSkipSharedId], [t1].[CompositeKeySkipSharedKey1], [t1].[CompositeKeySkipSharedKey2], [t1].[CompositeKeySkipSharedKey3], [t1].[Id], [t1].[CollectionInverseId], [t1].[Name0], [t1].[ReferenceInverseId], [t1].[Id0]
FROM [EntityRoots] AS [e]
LEFT JOIN (
    SELECT [e1].[Key1], [e1].[Key2], [e1].[Key3], [e1].[Name], [e0].[RootSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], [t0].[Id], [t0].[CollectionInverseId], [t0].[Name] AS [Name0], [t0].[ReferenceInverseId], [t0].[Id0], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3]
    FROM [EntityCompositeKeyEntityRoot] AS [e0]
    INNER JOIN [EntityCompositeKeys] AS [e1] ON [e0].[CompositeKeySkipSharedKey1] = [e1].[Key1] AND [e0].[CompositeKeySkipSharedKey2] = [e1].[Key2] AND [e0].[CompositeKeySkipSharedKey3] = [e1].[Key3]
    LEFT JOIN (
        SELECT [t].[Id], [t].[CollectionInverseId], [t].[Name], [t].[ReferenceInverseId], [t].[Id0], [t].[CompositeId1], [t].[CompositeId2], [t].[CompositeId3]
        FROM (
            SELECT [e2].[Id], [e2].[CollectionInverseId], [e2].[Name], [e2].[ReferenceInverseId], [j].[Id] AS [Id0], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], ROW_NUMBER() OVER(PARTITION BY [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3] ORDER BY [e2].[Id]) AS [row]
            FROM [JoinThreeToCompositeKeyFull] AS [j]
            INNER JOIN [EntityThrees] AS [e2] ON [j].[ThreeId] = [e2].[Id]
        ) AS [t]
        WHERE 1 < [t].[row] AND [t].[row] <= 3
    ) AS [t0] ON [e1].[Key1] = [t0].[CompositeId1] AND [e1].[Key2] = [t0].[CompositeId2] AND [e1].[Key3] = [t0].[CompositeId3]
) AS [t1] ON [e].[Id] = [t1].[RootSkipSharedId]
ORDER BY [e].[Id], [t1].[RootSkipSharedId], [t1].[CompositeKeySkipSharedKey1], [t1].[CompositeKeySkipSharedKey2], [t1].[CompositeKeySkipSharedKey3], [t1].[Key1], [t1].[Key2], [t1].[Key3], [t1].[CompositeId1], [t1].[CompositeId2], [t1].[CompositeId3], [t1].[Id]");
    }

    public override async Task Filtered_include_skip_navigation_where_then_include_skip_navigation(bool async)
    {
        await base.Filtered_include_skip_navigation_where_then_include_skip_navigation(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[Number], [e].[IsGreen], [t0].[Key1], [t0].[Key2], [t0].[Key3], [t0].[Name], [t0].[LeafId], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name0], [t0].[ReferenceInverseId], [t0].[TwoSkipSharedId], [t0].[CompositeKeySkipSharedKey1], [t0].[CompositeKeySkipSharedKey2], [t0].[CompositeKeySkipSharedKey3]
FROM [EntityRoots] AS [e]
LEFT JOIN (
    SELECT [e0].[Key1], [e0].[Key2], [e0].[Key3], [e0].[Name], [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name] AS [Name0], [t].[ReferenceInverseId], [t].[TwoSkipSharedId], [t].[CompositeKeySkipSharedKey1], [t].[CompositeKeySkipSharedKey2], [t].[CompositeKeySkipSharedKey3]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN [EntityCompositeKeys] AS [e0] ON [j].[CompositeId1] = [e0].[Key1] AND [j].[CompositeId2] = [e0].[Key2] AND [j].[CompositeId3] = [e0].[Key3]
    LEFT JOIN (
        SELECT [e2].[Id], [e2].[CollectionInverseId], [e2].[ExtraId], [e2].[Name], [e2].[ReferenceInverseId], [e1].[TwoSkipSharedId], [e1].[CompositeKeySkipSharedKey1], [e1].[CompositeKeySkipSharedKey2], [e1].[CompositeKeySkipSharedKey3]
        FROM [EntityCompositeKeyEntityTwo] AS [e1]
        INNER JOIN [EntityTwos] AS [e2] ON [e1].[TwoSkipSharedId] = [e2].[Id]
    ) AS [t] ON [e0].[Key1] = [t].[CompositeKeySkipSharedKey1] AND [e0].[Key2] = [t].[CompositeKeySkipSharedKey2] AND [e0].[Key3] = [t].[CompositeKeySkipSharedKey3]
    WHERE [e0].[Key1] < 5
) AS [t0] ON [e].[Id] = [t0].[LeafId]
WHERE [e].[Discriminator] = N'EntityLeaf'
ORDER BY [e].[Id], [t0].[LeafId], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[Key1], [t0].[Key2], [t0].[Key3], [t0].[TwoSkipSharedId], [t0].[CompositeKeySkipSharedKey1], [t0].[CompositeKeySkipSharedKey2], [t0].[CompositeKeySkipSharedKey3]");
    }

    public override async Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[Name], [t1].[Id], [t1].[CollectionInverseId], [t1].[ExtraId], [t1].[Name], [t1].[ReferenceInverseId], [t1].[OneId], [t1].[TwoId], [t1].[Id0], [t1].[CollectionInverseId0], [t1].[Name0], [t1].[ReferenceInverseId0], [t1].[ThreeId], [t1].[TwoId0]
FROM [EntityOnes] AS [e]
OUTER APPLY (
    SELECT [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId], [t].[OneId], [t].[TwoId], [t0].[Id] AS [Id0], [t0].[CollectionInverseId] AS [CollectionInverseId0], [t0].[Name] AS [Name0], [t0].[ReferenceInverseId] AS [ReferenceInverseId0], [t0].[ThreeId], [t0].[TwoId] AS [TwoId0]
    FROM (
        SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], [j].[OneId], [j].[TwoId]
        FROM [JoinOneToTwo] AS [j]
        INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
        WHERE [e].[Id] = [j].[OneId]
        ORDER BY [e0].[Id]
        OFFSET 1 ROWS FETCH NEXT 2 ROWS ONLY
    ) AS [t]
    LEFT JOIN (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], [j0].[ThreeId], [j0].[TwoId]
        FROM [JoinTwoToThree] AS [j0]
        INNER JOIN [EntityThrees] AS [e1] ON [j0].[ThreeId] = [e1].[Id]
        WHERE [e1].[Id] < 10
    ) AS [t0] ON [t].[Id] = [t0].[TwoId]
) AS [t1]
ORDER BY [e].[Id], [t1].[Id], [t1].[OneId], [t1].[TwoId], [t1].[ThreeId], [t1].[TwoId0]");
    }

    public override async Task Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take(bool async)
    {
        await base.Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[Name], [t1].[Id], [t1].[CollectionInverseId], [t1].[ExtraId], [t1].[Name], [t1].[ReferenceInverseId], [t1].[OneId], [t1].[TwoId], [t1].[Id0], [t1].[CollectionInverseId0], [t1].[Name0], [t1].[ReferenceInverseId0], [t1].[ThreeId], [t1].[TwoId0]
FROM [EntityOnes] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], [j].[OneId], [j].[TwoId], [t0].[Id] AS [Id0], [t0].[CollectionInverseId] AS [CollectionInverseId0], [t0].[Name] AS [Name0], [t0].[ReferenceInverseId] AS [ReferenceInverseId0], [t0].[ThreeId], [t0].[TwoId] AS [TwoId0]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    LEFT JOIN (
        SELECT [t].[Id], [t].[CollectionInverseId], [t].[Name], [t].[ReferenceInverseId], [t].[ThreeId], [t].[TwoId]
        FROM (
            SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], [j0].[ThreeId], [j0].[TwoId], ROW_NUMBER() OVER(PARTITION BY [j0].[TwoId] ORDER BY [e1].[Id]) AS [row]
            FROM [JoinTwoToThree] AS [j0]
            INNER JOIN [EntityThrees] AS [e1] ON [j0].[ThreeId] = [e1].[Id]
        ) AS [t]
        WHERE 1 < [t].[row] AND [t].[row] <= 3
    ) AS [t0] ON [e0].[Id] = [t0].[TwoId]
    WHERE [e0].[Id] < 10
) AS [t1] ON [e].[Id] = [t1].[OneId]
ORDER BY [e].[Id], [t1].[OneId], [t1].[TwoId], [t1].[Id], [t1].[TwoId0], [t1].[Id0]");
    }

    public override async Task Filter_include_on_skip_navigation_combined(bool async)
    {
        await base.Filter_include_on_skip_navigation_combined(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId], [t].[Id], [t].[Name], [t].[Id0], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name0], [t].[ReferenceInverseId], [t].[OneId], [t].[TwoId], [t].[Id1], [t].[CollectionInverseId0], [t].[ExtraId0], [t].[Name1], [t].[ReferenceInverseId0]
FROM [EntityTwos] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId], [j].[OneId], [j].[TwoId], [e2].[Id] AS [Id1], [e2].[CollectionInverseId] AS [CollectionInverseId0], [e2].[ExtraId] AS [ExtraId0], [e2].[Name] AS [Name1], [e2].[ReferenceInverseId] AS [ReferenceInverseId0]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
    LEFT JOIN [EntityTwos] AS [e2] ON [e0].[Id] = [e2].[CollectionInverseId]
    WHERE [e0].[Id] < 10
) AS [t] ON [e].[Id] = [t].[TwoId]
ORDER BY [e].[Id], [t].[OneId], [t].[TwoId], [t].[Id], [t].[Id0]");
    }

    public override async Task Filter_include_on_skip_navigation_combined_with_filtered_then_includes(bool async)
    {
        await base.Filter_include_on_skip_navigation_combined_with_filtered_then_includes(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId], [t3].[Id], [t3].[Name], [t3].[OneId], [t3].[ThreeId], [t3].[Id0], [t3].[CollectionInverseId], [t3].[ExtraId], [t3].[Name0], [t3].[ReferenceInverseId], [t3].[OneId0], [t3].[TwoId], [t3].[Id1], [t3].[Discriminator], [t3].[Name1], [t3].[Number], [t3].[IsGreen], [t3].[EntityBranchId], [t3].[EntityOneId]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[Name], [j].[OneId], [j].[ThreeId], [t0].[Id] AS [Id0], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name] AS [Name0], [t0].[ReferenceInverseId], [t0].[OneId] AS [OneId0], [t0].[TwoId], [t1].[Id] AS [Id1], [t1].[Discriminator], [t1].[Name] AS [Name1], [t1].[Number], [t1].[IsGreen], [t1].[EntityBranchId], [t1].[EntityOneId]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN (
        SELECT [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId], [t].[OneId], [t].[TwoId]
        FROM (
            SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], [j0].[OneId], [j0].[TwoId], ROW_NUMBER() OVER(PARTITION BY [j0].[OneId] ORDER BY [e1].[Id]) AS [row]
            FROM [JoinOneToTwo] AS [j0]
            INNER JOIN [EntityTwos] AS [e1] ON [j0].[TwoId] = [e1].[Id]
        ) AS [t]
        WHERE 1 < [t].[row] AND [t].[row] <= 3
    ) AS [t0] ON [e0].[Id] = [t0].[OneId]
    LEFT JOIN (
        SELECT [t2].[Id], [t2].[Discriminator], [t2].[Name], [t2].[Number], [t2].[IsGreen], [j1].[EntityBranchId], [j1].[EntityOneId]
        FROM [JoinOneToBranch] AS [j1]
        INNER JOIN (
            SELECT [e2].[Id], [e2].[Discriminator], [e2].[Name], [e2].[Number], [e2].[IsGreen]
            FROM [EntityRoots] AS [e2]
            WHERE [e2].[Discriminator] IN (N'EntityBranch', N'EntityLeaf')
        ) AS [t2] ON [j1].[EntityBranchId] = [t2].[Id]
        WHERE [t2].[Id] < 20
    ) AS [t1] ON [e0].[Id] = [t1].[EntityOneId]
    WHERE [e0].[Id] < 10
) AS [t3] ON [e].[Id] = [t3].[ThreeId]
ORDER BY [e].[Id], [t3].[OneId], [t3].[ThreeId], [t3].[Id], [t3].[OneId0], [t3].[Id0], [t3].[TwoId], [t3].[EntityBranchId], [t3].[EntityOneId]");
    }

    public override async Task Filtered_include_on_skip_navigation_then_filtered_include_on_navigation(bool async)
    {
        await base.Filtered_include_on_skip_navigation_then_filtered_include_on_navigation(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId], [t0].[Id], [t0].[Name], [t0].[OneId], [t0].[ThreeId], [t0].[Id0], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name0], [t0].[ReferenceInverseId]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[Name], [j].[OneId], [j].[ThreeId], [t].[Id] AS [Id0], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name] AS [Name0], [t].[ReferenceInverseId]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId]
        FROM [EntityTwos] AS [e1]
        WHERE [e1].[Id] < 5
    ) AS [t] ON [e0].[Id] = [t].[CollectionInverseId]
    WHERE [e0].[Id] > 15
) AS [t0] ON [e].[Id] = [t0].[ThreeId]
ORDER BY [e].[Id], [t0].[OneId], [t0].[ThreeId], [t0].[Id]");
    }

    public override async Task Filtered_include_on_navigation_then_filtered_include_on_skip_navigation(bool async)
    {
        await base.Filtered_include_on_navigation_then_filtered_include_on_skip_navigation(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[Name], [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId], [t0].[Id0], [t0].[CollectionInverseId0], [t0].[Name0], [t0].[ReferenceInverseId0], [t0].[ThreeId], [t0].[TwoId]
FROM [EntityOnes] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], [t].[Id] AS [Id0], [t].[CollectionInverseId] AS [CollectionInverseId0], [t].[Name] AS [Name0], [t].[ReferenceInverseId] AS [ReferenceInverseId0], [t].[ThreeId], [t].[TwoId]
    FROM [EntityTwos] AS [e0]
    LEFT JOIN (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], [j].[ThreeId], [j].[TwoId]
        FROM [JoinTwoToThree] AS [j]
        INNER JOIN [EntityThrees] AS [e1] ON [j].[ThreeId] = [e1].[Id]
        WHERE [e1].[Id] < 5
    ) AS [t] ON [e0].[Id] = [t].[TwoId]
    WHERE [e0].[Id] > 15
) AS [t0] ON [e].[Id] = [t0].[CollectionInverseId]
ORDER BY [e].[Id], [t0].[Id], [t0].[ThreeId], [t0].[TwoId]");
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
            @"SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name]
FROM [EntityCompositeKeys] AS [e]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3]",
            //
            @"SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t].[Number], [t].[Slumber], [t].[IsGreen], [t].[IsBrown], [e].[Key1], [e].[Key2], [e].[Key3]
FROM [EntityCompositeKeys] AS [e]
INNER JOIN (
    SELECT [e1].[Id], [e1].[Discriminator], [e1].[Name], [e1].[Number], [e1].[Slumber], [e1].[IsGreen], [e1].[IsBrown], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3]
    FROM [EntityCompositeKeyEntityRoot] AS [e0]
    INNER JOIN [EntityRoots] AS [e1] ON [e0].[RootSkipSharedId] = [e1].[Id]
) AS [t] ON [e].[Key1] = [t].[CompositeKeySkipSharedKey1] AND [e].[Key2] = [t].[CompositeKeySkipSharedKey2] AND [e].[Key3] = [t].[CompositeKeySkipSharedKey3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3]");
    }

    public override async Task Include_skip_navigation_then_reference_split(bool async)
    {
        await base.Include_skip_navigation_then_reference_split(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId]
FROM [EntityTwos] AS [e]
ORDER BY [e].[Id]",
            //
            @"SELECT [t].[Id], [t].[Name], [t].[Id0], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name0], [t].[ReferenceInverseId], [e].[Id]
FROM [EntityTwos] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId], [j].[TwoId]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
) AS [t] ON [e].[Id] = [t].[TwoId]
ORDER BY [e].[Id]");
    }

    public override async Task Include_skip_navigation_then_include_skip_navigation_split(bool async)
    {
        await base.Include_skip_navigation_then_include_skip_navigation_split(async);

        AssertSql(
            @"SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name]
FROM [EntityCompositeKeys] AS [e]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3]",
            //
            @"SELECT [t0].[Id], [t0].[Discriminator], [t0].[Name], [t0].[Number], [t0].[IsGreen], [e].[Key1], [e].[Key2], [e].[Key3], [t0].[LeafId], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3]
FROM [EntityCompositeKeys] AS [e]
INNER JOIN (
    SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t].[Number], [t].[IsGreen], [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN (
        SELECT [e0].[Id], [e0].[Discriminator], [e0].[Name], [e0].[Number], [e0].[IsGreen]
        FROM [EntityRoots] AS [e0]
        WHERE [e0].[Discriminator] = N'EntityLeaf'
    ) AS [t] ON [j].[LeafId] = [t].[Id]
) AS [t0] ON [e].[Key1] = [t0].[CompositeId1] AND [e].[Key2] = [t0].[CompositeId2] AND [e].[Key3] = [t0].[CompositeId3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [t0].[LeafId], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[Id]",
            //
            @"SELECT [t1].[Id], [t1].[Name], [e].[Key1], [e].[Key2], [e].[Key3], [t0].[LeafId], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[Id]
FROM [EntityCompositeKeys] AS [e]
INNER JOIN (
    SELECT [t].[Id], [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN (
        SELECT [e0].[Id]
        FROM [EntityRoots] AS [e0]
        WHERE [e0].[Discriminator] = N'EntityLeaf'
    ) AS [t] ON [j].[LeafId] = [t].[Id]
) AS [t0] ON [e].[Key1] = [t0].[CompositeId1] AND [e].[Key2] = [t0].[CompositeId2] AND [e].[Key3] = [t0].[CompositeId3]
INNER JOIN (
    SELECT [e1].[Id], [e1].[Name], [j0].[EntityBranchId]
    FROM [JoinOneToBranch] AS [j0]
    INNER JOIN [EntityOnes] AS [e1] ON [j0].[EntityOneId] = [e1].[Id]
) AS [t1] ON [t0].[Id] = [t1].[EntityBranchId]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [t0].[LeafId], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[Id]");
    }

    public override async Task Include_skip_navigation_then_include_reference_and_skip_navigation_split(bool async)
    {
        await base.Include_skip_navigation_then_include_reference_and_skip_navigation_split(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId]
FROM [EntityThrees] AS [e]
ORDER BY [e].[Id]",
            //
            @"SELECT [t].[Id], [t].[Name], [t].[Id0], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name0], [t].[ReferenceInverseId], [e].[Id], [t].[OneId], [t].[ThreeId]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId], [j].[OneId], [j].[ThreeId]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
) AS [t] ON [e].[Id] = [t].[ThreeId]
ORDER BY [e].[Id], [t].[OneId], [t].[ThreeId], [t].[Id], [t].[Id0]",
            //
            @"SELECT [t0].[Id], [t0].[Name], [e].[Id], [t].[OneId], [t].[ThreeId], [t].[Id], [t].[Id0]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e1].[Id] AS [Id0], [j].[OneId], [j].[ThreeId]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
) AS [t] ON [e].[Id] = [t].[ThreeId]
INNER JOIN (
    SELECT [e2].[Id], [e2].[Name], [j0].[LeftId]
    FROM [JoinOneSelfPayload] AS [j0]
    INNER JOIN [EntityOnes] AS [e2] ON [j0].[RightId] = [e2].[Id]
) AS [t0] ON [t].[Id] = [t0].[LeftId]
ORDER BY [e].[Id], [t].[OneId], [t].[ThreeId], [t].[Id], [t].[Id0]");
    }

    public override async Task Include_skip_navigation_and_reference_split(bool async)
    {
        await base.Include_skip_navigation_and_reference_split(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId], [e0].[Id], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId]
FROM [EntityTwos] AS [e]
LEFT JOIN [EntityThrees] AS [e0] ON [e].[Id] = [e0].[ReferenceInverseId]
ORDER BY [e].[Id], [e0].[Id]",
            //
            @"SELECT [t].[Id], [t].[Name], [e].[Id], [e0].[Id]
FROM [EntityTwos] AS [e]
LEFT JOIN [EntityThrees] AS [e0] ON [e].[Id] = [e0].[ReferenceInverseId]
INNER JOIN (
    SELECT [e2].[Id], [e2].[Name], [e1].[TwoSkipSharedId]
    FROM [EntityOneEntityTwo] AS [e1]
    INNER JOIN [EntityOnes] AS [e2] ON [e1].[OneSkipSharedId] = [e2].[Id]
) AS [t] ON [e].[Id] = [t].[TwoSkipSharedId]
ORDER BY [e].[Id], [e0].[Id]");
    }

    public override async Task Filtered_include_skip_navigation_where_split(bool async)
    {
        await base.Filtered_include_skip_navigation_where_split(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId]
FROM [EntityThrees] AS [e]
ORDER BY [e].[Id]",
            //
            @"SELECT [t].[Id], [t].[Name], [e].[Id]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e0].[Name], [j].[ThreeId]
    FROM [JoinOneToThreePayloadFullShared] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [t] ON [e].[Id] = [t].[ThreeId]
ORDER BY [e].[Id]");
    }

    public override async Task Filtered_include_skip_navigation_order_by_split(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_split(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId]
FROM [EntityThrees] AS [e]
ORDER BY [e].[Id]",
            //
            @"SELECT [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId], [e].[Id]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], [j].[ThreeId]
    FROM [JoinTwoToThree] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
) AS [t] ON [e].[Id] = [t].[ThreeId]
ORDER BY [e].[Id], [t].[Id]");
    }

    public override async Task Filtered_include_skip_navigation_order_by_skip_split(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip_split(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId]
FROM [EntityTwos] AS [e]
ORDER BY [e].[Id]",
            //
            @"SELECT [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId], [e].[Id]
FROM [EntityTwos] AS [e]
INNER JOIN (
    SELECT [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId], [t].[SelfSkipSharedLeftId]
    FROM (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], [e0].[SelfSkipSharedLeftId], ROW_NUMBER() OVER(PARTITION BY [e0].[SelfSkipSharedLeftId] ORDER BY [e1].[Id]) AS [row]
        FROM [EntityTwoEntityTwo] AS [e0]
        INNER JOIN [EntityTwos] AS [e1] ON [e0].[SelfSkipSharedRightId] = [e1].[Id]
    ) AS [t]
    WHERE 2 < [t].[row]
) AS [t0] ON [e].[Id] = [t0].[SelfSkipSharedLeftId]
ORDER BY [e].[Id], [t0].[SelfSkipSharedLeftId], [t0].[Id]");
    }

    public override async Task Filtered_include_skip_navigation_order_by_take_split(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_take_split(async);

        AssertSql(
            @"SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name]
FROM [EntityCompositeKeys] AS [e]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3]",
            //
            @"SELECT [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId], [e].[Key1], [e].[Key2], [e].[Key3]
FROM [EntityCompositeKeys] AS [e]
INNER JOIN (
    SELECT [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId], [t].[CompositeKeySkipSharedKey1], [t].[CompositeKeySkipSharedKey2], [t].[CompositeKeySkipSharedKey3]
    FROM (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], ROW_NUMBER() OVER(PARTITION BY [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3] ORDER BY [e1].[Id]) AS [row]
        FROM [EntityCompositeKeyEntityTwo] AS [e0]
        INNER JOIN [EntityTwos] AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
    ) AS [t]
    WHERE [t].[row] <= 2
) AS [t0] ON [e].[Key1] = [t0].[CompositeKeySkipSharedKey1] AND [e].[Key2] = [t0].[CompositeKeySkipSharedKey2] AND [e].[Key3] = [t0].[CompositeKeySkipSharedKey3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [t0].[CompositeKeySkipSharedKey1], [t0].[CompositeKeySkipSharedKey2], [t0].[CompositeKeySkipSharedKey3], [t0].[Id]");
    }

    public override async Task Filtered_include_skip_navigation_order_by_skip_take_split(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip_take_split(async);

        AssertSql(
            @"SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name]
FROM [EntityCompositeKeys] AS [e]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3]",
            //
            @"SELECT [t0].[Id], [t0].[CollectionInverseId], [t0].[Name], [t0].[ReferenceInverseId], [e].[Key1], [e].[Key2], [e].[Key3]
FROM [EntityCompositeKeys] AS [e]
INNER JOIN (
    SELECT [t].[Id], [t].[CollectionInverseId], [t].[Name], [t].[ReferenceInverseId], [t].[CompositeId1], [t].[CompositeId2], [t].[CompositeId3]
    FROM (
        SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], ROW_NUMBER() OVER(PARTITION BY [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinThreeToCompositeKeyFull] AS [j]
        INNER JOIN [EntityThrees] AS [e0] ON [j].[ThreeId] = [e0].[Id]
    ) AS [t]
    WHERE 1 < [t].[row] AND [t].[row] <= 3
) AS [t0] ON [e].[Key1] = [t0].[CompositeId1] AND [e].[Key2] = [t0].[CompositeId2] AND [e].[Key3] = [t0].[CompositeId3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[Id]");
    }

    public override async Task Filtered_then_include_skip_navigation_where_split(bool async)
    {
        await base.Filtered_then_include_skip_navigation_where_split(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[Number], [e].[Slumber], [e].[IsGreen], [e].[IsBrown]
FROM [EntityRoots] AS [e]
ORDER BY [e].[Id]",
            //
            @"SELECT [t].[Id], [t].[CollectionInverseId], [t].[Name], [t].[ReferenceInverseId], [e].[Id], [t].[RootSkipSharedId], [t].[ThreeSkipSharedId]
FROM [EntityRoots] AS [e]
INNER JOIN (
    SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], [e0].[RootSkipSharedId], [e0].[ThreeSkipSharedId]
    FROM [EntityRootEntityThree] AS [e0]
    INNER JOIN [EntityThrees] AS [e1] ON [e0].[ThreeSkipSharedId] = [e1].[Id]
) AS [t] ON [e].[Id] = [t].[RootSkipSharedId]
ORDER BY [e].[Id], [t].[RootSkipSharedId], [t].[ThreeSkipSharedId], [t].[Id]",
            //
            @"SELECT [t0].[Id], [t0].[Name], [e].[Id], [t].[RootSkipSharedId], [t].[ThreeSkipSharedId], [t].[Id]
FROM [EntityRoots] AS [e]
INNER JOIN (
    SELECT [e1].[Id], [e0].[RootSkipSharedId], [e0].[ThreeSkipSharedId]
    FROM [EntityRootEntityThree] AS [e0]
    INNER JOIN [EntityThrees] AS [e1] ON [e0].[ThreeSkipSharedId] = [e1].[Id]
) AS [t] ON [e].[Id] = [t].[RootSkipSharedId]
INNER JOIN (
    SELECT [e2].[Id], [e2].[Name], [j].[ThreeId]
    FROM [JoinOneToThreePayloadFullShared] AS [j]
    INNER JOIN [EntityOnes] AS [e2] ON [j].[OneId] = [e2].[Id]
    WHERE [e2].[Id] < 10
) AS [t0] ON [t].[Id] = [t0].[ThreeId]
ORDER BY [e].[Id], [t].[RootSkipSharedId], [t].[ThreeSkipSharedId], [t].[Id]");
    }

    public override async Task Filtered_then_include_skip_navigation_order_by_skip_take_split(bool async)
    {
        await base.Filtered_then_include_skip_navigation_order_by_skip_take_split(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[Number], [e].[Slumber], [e].[IsGreen], [e].[IsBrown]
FROM [EntityRoots] AS [e]
ORDER BY [e].[Id]",
            //
            @"SELECT [t].[Key1], [t].[Key2], [t].[Key3], [t].[Name], [e].[Id], [t].[RootSkipSharedId], [t].[CompositeKeySkipSharedKey1], [t].[CompositeKeySkipSharedKey2], [t].[CompositeKeySkipSharedKey3]
FROM [EntityRoots] AS [e]
INNER JOIN (
    SELECT [e1].[Key1], [e1].[Key2], [e1].[Key3], [e1].[Name], [e0].[RootSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3]
    FROM [EntityCompositeKeyEntityRoot] AS [e0]
    INNER JOIN [EntityCompositeKeys] AS [e1] ON [e0].[CompositeKeySkipSharedKey1] = [e1].[Key1] AND [e0].[CompositeKeySkipSharedKey2] = [e1].[Key2] AND [e0].[CompositeKeySkipSharedKey3] = [e1].[Key3]
) AS [t] ON [e].[Id] = [t].[RootSkipSharedId]
ORDER BY [e].[Id], [t].[RootSkipSharedId], [t].[CompositeKeySkipSharedKey1], [t].[CompositeKeySkipSharedKey2], [t].[CompositeKeySkipSharedKey3], [t].[Key1], [t].[Key2], [t].[Key3]",
            //
            @"SELECT [t0].[Id], [t0].[CollectionInverseId], [t0].[Name], [t0].[ReferenceInverseId], [e].[Id], [t].[RootSkipSharedId], [t].[CompositeKeySkipSharedKey1], [t].[CompositeKeySkipSharedKey2], [t].[CompositeKeySkipSharedKey3], [t].[Key1], [t].[Key2], [t].[Key3]
FROM [EntityRoots] AS [e]
INNER JOIN (
    SELECT [e1].[Key1], [e1].[Key2], [e1].[Key3], [e0].[RootSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3]
    FROM [EntityCompositeKeyEntityRoot] AS [e0]
    INNER JOIN [EntityCompositeKeys] AS [e1] ON [e0].[CompositeKeySkipSharedKey1] = [e1].[Key1] AND [e0].[CompositeKeySkipSharedKey2] = [e1].[Key2] AND [e0].[CompositeKeySkipSharedKey3] = [e1].[Key3]
) AS [t] ON [e].[Id] = [t].[RootSkipSharedId]
INNER JOIN (
    SELECT [t1].[Id], [t1].[CollectionInverseId], [t1].[Name], [t1].[ReferenceInverseId], [t1].[CompositeId1], [t1].[CompositeId2], [t1].[CompositeId3]
    FROM (
        SELECT [e2].[Id], [e2].[CollectionInverseId], [e2].[Name], [e2].[ReferenceInverseId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], ROW_NUMBER() OVER(PARTITION BY [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3] ORDER BY [e2].[Id]) AS [row]
        FROM [JoinThreeToCompositeKeyFull] AS [j]
        INNER JOIN [EntityThrees] AS [e2] ON [j].[ThreeId] = [e2].[Id]
    ) AS [t1]
    WHERE 1 < [t1].[row] AND [t1].[row] <= 3
) AS [t0] ON [t].[Key1] = [t0].[CompositeId1] AND [t].[Key2] = [t0].[CompositeId2] AND [t].[Key3] = [t0].[CompositeId3]
ORDER BY [e].[Id], [t].[RootSkipSharedId], [t].[CompositeKeySkipSharedKey1], [t].[CompositeKeySkipSharedKey2], [t].[CompositeKeySkipSharedKey3], [t].[Key1], [t].[Key2], [t].[Key3], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[Id]");
    }

    public override async Task Filtered_include_skip_navigation_where_then_include_skip_navigation_split(bool async)
    {
        await base.Filtered_include_skip_navigation_where_then_include_skip_navigation_split(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[Number], [e].[IsGreen]
FROM [EntityRoots] AS [e]
WHERE [e].[Discriminator] = N'EntityLeaf'
ORDER BY [e].[Id]",
            //
            @"SELECT [t].[Key1], [t].[Key2], [t].[Key3], [t].[Name], [e].[Id], [t].[LeafId], [t].[CompositeId1], [t].[CompositeId2], [t].[CompositeId3]
FROM [EntityRoots] AS [e]
INNER JOIN (
    SELECT [e0].[Key1], [e0].[Key2], [e0].[Key3], [e0].[Name], [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN [EntityCompositeKeys] AS [e0] ON [j].[CompositeId1] = [e0].[Key1] AND [j].[CompositeId2] = [e0].[Key2] AND [j].[CompositeId3] = [e0].[Key3]
    WHERE [e0].[Key1] < 5
) AS [t] ON [e].[Id] = [t].[LeafId]
WHERE [e].[Discriminator] = N'EntityLeaf'
ORDER BY [e].[Id], [t].[LeafId], [t].[CompositeId1], [t].[CompositeId2], [t].[CompositeId3], [t].[Key1], [t].[Key2], [t].[Key3]",
            //
            @"SELECT [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId], [e].[Id], [t].[LeafId], [t].[CompositeId1], [t].[CompositeId2], [t].[CompositeId3], [t].[Key1], [t].[Key2], [t].[Key3]
FROM [EntityRoots] AS [e]
INNER JOIN (
    SELECT [e0].[Key1], [e0].[Key2], [e0].[Key3], [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN [EntityCompositeKeys] AS [e0] ON [j].[CompositeId1] = [e0].[Key1] AND [j].[CompositeId2] = [e0].[Key2] AND [j].[CompositeId3] = [e0].[Key3]
    WHERE [e0].[Key1] < 5
) AS [t] ON [e].[Id] = [t].[LeafId]
INNER JOIN (
    SELECT [e2].[Id], [e2].[CollectionInverseId], [e2].[ExtraId], [e2].[Name], [e2].[ReferenceInverseId], [e1].[CompositeKeySkipSharedKey1], [e1].[CompositeKeySkipSharedKey2], [e1].[CompositeKeySkipSharedKey3]
    FROM [EntityCompositeKeyEntityTwo] AS [e1]
    INNER JOIN [EntityTwos] AS [e2] ON [e1].[TwoSkipSharedId] = [e2].[Id]
) AS [t0] ON [t].[Key1] = [t0].[CompositeKeySkipSharedKey1] AND [t].[Key2] = [t0].[CompositeKeySkipSharedKey2] AND [t].[Key3] = [t0].[CompositeKeySkipSharedKey3]
WHERE [e].[Discriminator] = N'EntityLeaf'
ORDER BY [e].[Id], [t].[LeafId], [t].[CompositeId1], [t].[CompositeId2], [t].[CompositeId3], [t].[Key1], [t].[Key2], [t].[Key3]");
    }

    public override async Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where_split(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where_split(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[Name]
FROM [EntityOnes] AS [e]
ORDER BY [e].[Id]",
            //
            @"SELECT [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId], [e].[Id], [t0].[OneId], [t0].[TwoId]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId], [t].[OneId], [t].[TwoId]
    FROM (
        SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], [j].[OneId], [j].[TwoId], ROW_NUMBER() OVER(PARTITION BY [j].[OneId] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinOneToTwo] AS [j]
        INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    ) AS [t]
    WHERE 1 < [t].[row] AND [t].[row] <= 3
) AS [t0] ON [e].[Id] = [t0].[OneId]
ORDER BY [e].[Id], [t0].[OneId], [t0].[Id], [t0].[TwoId]",
            //
            @"SELECT [t1].[Id], [t1].[CollectionInverseId], [t1].[Name], [t1].[ReferenceInverseId], [e].[Id], [t0].[OneId], [t0].[TwoId], [t0].[Id]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [t].[Id], [t].[OneId], [t].[TwoId]
    FROM (
        SELECT [e0].[Id], [j].[OneId], [j].[TwoId], ROW_NUMBER() OVER(PARTITION BY [j].[OneId] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinOneToTwo] AS [j]
        INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    ) AS [t]
    WHERE 1 < [t].[row] AND [t].[row] <= 3
) AS [t0] ON [e].[Id] = [t0].[OneId]
INNER JOIN (
    SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], [j0].[TwoId]
    FROM [JoinTwoToThree] AS [j0]
    INNER JOIN [EntityThrees] AS [e1] ON [j0].[ThreeId] = [e1].[Id]
    WHERE [e1].[Id] < 10
) AS [t1] ON [t0].[Id] = [t1].[TwoId]
ORDER BY [e].[Id], [t0].[OneId], [t0].[Id], [t0].[TwoId]");
    }

    public override async Task Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take_split(bool async)
    {
        await base.Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take_split(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[Name]
FROM [EntityOnes] AS [e]
ORDER BY [e].[Id]",
            //
            @"SELECT [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId], [e].[Id], [t].[OneId], [t].[TwoId]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], [j].[OneId], [j].[TwoId]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [t] ON [e].[Id] = [t].[OneId]
ORDER BY [e].[Id], [t].[OneId], [t].[TwoId], [t].[Id]",
            //
            @"SELECT [t0].[Id], [t0].[CollectionInverseId], [t0].[Name], [t0].[ReferenceInverseId], [e].[Id], [t].[OneId], [t].[TwoId], [t].[Id]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [j].[OneId], [j].[TwoId]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [t] ON [e].[Id] = [t].[OneId]
INNER JOIN (
    SELECT [t1].[Id], [t1].[CollectionInverseId], [t1].[Name], [t1].[ReferenceInverseId], [t1].[TwoId]
    FROM (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], [j0].[TwoId], ROW_NUMBER() OVER(PARTITION BY [j0].[TwoId] ORDER BY [e1].[Id]) AS [row]
        FROM [JoinTwoToThree] AS [j0]
        INNER JOIN [EntityThrees] AS [e1] ON [j0].[ThreeId] = [e1].[Id]
    ) AS [t1]
    WHERE 1 < [t1].[row] AND [t1].[row] <= 3
) AS [t0] ON [t].[Id] = [t0].[TwoId]
ORDER BY [e].[Id], [t].[OneId], [t].[TwoId], [t].[Id], [t0].[TwoId], [t0].[Id]");
    }

    public override async Task Filter_include_on_skip_navigation_combined_split(bool async)
    {
        await base.Filter_include_on_skip_navigation_combined_split(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId]
FROM [EntityTwos] AS [e]
ORDER BY [e].[Id]",
            //
            @"SELECT [t].[Id], [t].[Name], [t].[Id0], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name0], [t].[ReferenceInverseId], [e].[Id], [t].[OneId], [t].[TwoId]
FROM [EntityTwos] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId], [j].[OneId], [j].[TwoId]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
    WHERE [e0].[Id] < 10
) AS [t] ON [e].[Id] = [t].[TwoId]
ORDER BY [e].[Id], [t].[OneId], [t].[TwoId], [t].[Id], [t].[Id0]",
            //
            @"SELECT [e2].[Id], [e2].[CollectionInverseId], [e2].[ExtraId], [e2].[Name], [e2].[ReferenceInverseId], [e].[Id], [t].[OneId], [t].[TwoId], [t].[Id], [t].[Id0]
FROM [EntityTwos] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e1].[Id] AS [Id0], [j].[OneId], [j].[TwoId]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
    WHERE [e0].[Id] < 10
) AS [t] ON [e].[Id] = [t].[TwoId]
INNER JOIN [EntityTwos] AS [e2] ON [t].[Id] = [e2].[CollectionInverseId]
ORDER BY [e].[Id], [t].[OneId], [t].[TwoId], [t].[Id], [t].[Id0]");
    }

    public override async Task Filter_include_on_skip_navigation_combined_with_filtered_then_includes_split(bool async)
    {
        await base.Filter_include_on_skip_navigation_combined_with_filtered_then_includes_split(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId]
FROM [EntityThrees] AS [e]
ORDER BY [e].[Id]",
            //
            @"SELECT [t].[Id], [t].[Name], [e].[Id], [t].[OneId], [t].[ThreeId]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e0].[Name], [j].[OneId], [j].[ThreeId]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [t] ON [e].[Id] = [t].[ThreeId]
ORDER BY [e].[Id], [t].[OneId], [t].[ThreeId], [t].[Id]",
            //
            @"SELECT [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId], [e].[Id], [t].[OneId], [t].[ThreeId], [t].[Id]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [j].[OneId], [j].[ThreeId]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [t] ON [e].[Id] = [t].[ThreeId]
INNER JOIN (
    SELECT [t1].[Id], [t1].[CollectionInverseId], [t1].[ExtraId], [t1].[Name], [t1].[ReferenceInverseId], [t1].[OneId]
    FROM (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], [j0].[OneId], ROW_NUMBER() OVER(PARTITION BY [j0].[OneId] ORDER BY [e1].[Id]) AS [row]
        FROM [JoinOneToTwo] AS [j0]
        INNER JOIN [EntityTwos] AS [e1] ON [j0].[TwoId] = [e1].[Id]
    ) AS [t1]
    WHERE 1 < [t1].[row] AND [t1].[row] <= 3
) AS [t0] ON [t].[Id] = [t0].[OneId]
ORDER BY [e].[Id], [t].[OneId], [t].[ThreeId], [t].[Id], [t0].[OneId], [t0].[Id]",
            //
            @"SELECT [t0].[Id], [t0].[Discriminator], [t0].[Name], [t0].[Number], [t0].[IsGreen], [e].[Id], [t].[OneId], [t].[ThreeId], [t].[Id]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [j].[OneId], [j].[ThreeId]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [t] ON [e].[Id] = [t].[ThreeId]
INNER JOIN (
    SELECT [t1].[Id], [t1].[Discriminator], [t1].[Name], [t1].[Number], [t1].[IsGreen], [j0].[EntityOneId]
    FROM [JoinOneToBranch] AS [j0]
    INNER JOIN (
        SELECT [e1].[Id], [e1].[Discriminator], [e1].[Name], [e1].[Number], [e1].[IsGreen]
        FROM [EntityRoots] AS [e1]
        WHERE [e1].[Discriminator] IN (N'EntityBranch', N'EntityLeaf')
    ) AS [t1] ON [j0].[EntityBranchId] = [t1].[Id]
    WHERE [t1].[Id] < 20
) AS [t0] ON [t].[Id] = [t0].[EntityOneId]
ORDER BY [e].[Id], [t].[OneId], [t].[ThreeId], [t].[Id]");
    }

    public override async Task Filtered_include_on_skip_navigation_then_filtered_include_on_navigation_split(bool async)
    {
        await base.Filtered_include_on_skip_navigation_then_filtered_include_on_navigation_split(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId]
FROM [EntityThrees] AS [e]
ORDER BY [e].[Id]",
            //
            @"SELECT [t].[Id], [t].[Name], [e].[Id], [t].[OneId], [t].[ThreeId]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e0].[Name], [j].[OneId], [j].[ThreeId]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e0].[Id] > 15
) AS [t] ON [e].[Id] = [t].[ThreeId]
ORDER BY [e].[Id], [t].[OneId], [t].[ThreeId], [t].[Id]",
            //
            @"SELECT [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId], [e].[Id], [t].[OneId], [t].[ThreeId], [t].[Id]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [j].[OneId], [j].[ThreeId]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e0].[Id] > 15
) AS [t] ON [e].[Id] = [t].[ThreeId]
INNER JOIN (
    SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId]
    FROM [EntityTwos] AS [e1]
    WHERE [e1].[Id] < 5
) AS [t0] ON [t].[Id] = [t0].[CollectionInverseId]
ORDER BY [e].[Id], [t].[OneId], [t].[ThreeId], [t].[Id]");
    }

    public override async Task Filtered_include_on_navigation_then_filtered_include_on_skip_navigation_split(bool async)
    {
        await base.Filtered_include_on_navigation_then_filtered_include_on_skip_navigation_split(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[Name]
FROM [EntityOnes] AS [e]
ORDER BY [e].[Id]",
            //
            @"SELECT [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId], [e].[Id]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId]
    FROM [EntityTwos] AS [e0]
    WHERE [e0].[Id] > 15
) AS [t] ON [e].[Id] = [t].[CollectionInverseId]
ORDER BY [e].[Id], [t].[Id]",
            //
            @"SELECT [t0].[Id], [t0].[CollectionInverseId], [t0].[Name], [t0].[ReferenceInverseId], [e].[Id], [t].[Id]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId]
    FROM [EntityTwos] AS [e0]
    WHERE [e0].[Id] > 15
) AS [t] ON [e].[Id] = [t].[CollectionInverseId]
INNER JOIN (
    SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], [j].[TwoId]
    FROM [JoinTwoToThree] AS [j]
    INNER JOIN [EntityThrees] AS [e1] ON [j].[ThreeId] = [e1].[Id]
    WHERE [e1].[Id] < 5
) AS [t0] ON [t].[Id] = [t0].[TwoId]
ORDER BY [e].[Id], [t].[Id]");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
