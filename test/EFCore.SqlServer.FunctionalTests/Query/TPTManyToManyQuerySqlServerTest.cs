// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class TPTManyToManyQuerySqlServerTest : TPTManyToManyQueryRelationalTestBase<TPTManyToManyQuerySqlServerFixture>
{
    public TPTManyToManyQuerySqlServerTest(TPTManyToManyQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override bool CanExecuteQueryString
        => true;

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

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
        SELECT [r].[Id], [r].[Name], [b].[Number], [l].[IsGreen], CASE
            WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
        END AS [Discriminator]
        FROM [Roots] AS [r]
        INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
        LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
    ) AS [t] ON [j].[EntityBranchId] = [t].[Id]
    WHERE [e].[Id] = [j].[EntityOneId] AND ([t].[Name] IS NOT NULL) AND ([t].[Name] LIKE N'L%')), [e].[Id]");
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
    WHERE [e].[Id] = [e0].[SelfSkipSharedRightId] AND ([e1].[Name] IS NOT NULL) AND ([e1].[Name] LIKE N'L%')) DESC, [e].[Id]");
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
            @"SELECT MIN([t0].[Id])
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [t].[Id], [e0].[ThreeSkipSharedId]
    FROM [EntityRootEntityThree] AS [e0]
    INNER JOIN (
        SELECT [r].[Id]
        FROM [Roots] AS [r]
    ) AS [t] ON [e0].[RootSkipSharedId] = [t].[Id]
) AS [t0] ON [e].[Id] = [t0].[ThreeSkipSharedId]");
    }

    public override async Task Skip_navigation_select_many_sum(bool async)
    {
        await base.Skip_navigation_select_many_sum(async);

        AssertSql(
            @"SELECT COALESCE(SUM([t].[Key1]), 0)
FROM [Roots] AS [r]
INNER JOIN (
    SELECT [e0].[Key1], [e].[RootSkipSharedId]
    FROM [EntityCompositeKeyEntityRoot] AS [e]
    INNER JOIN [EntityCompositeKeys] AS [e0] ON [e].[CompositeKeySkipSharedKey1] = [e0].[Key1] AND [e].[CompositeKeySkipSharedKey2] = [e0].[Key2] AND [e].[CompositeKeySkipSharedKey3] = [e0].[Key3]
) AS [t] ON [r].[Id] = [t].[RootSkipSharedId]");
    }

    public override async Task Skip_navigation_select_subquery_average(bool async)
    {
        await base.Skip_navigation_select_subquery_average(async);

        AssertSql(
            @"SELECT (
    SELECT AVG(CAST([e].[Key1] AS float))
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN [EntityCompositeKeys] AS [e] ON [j].[CompositeId1] = [e].[Key1] AND [j].[CompositeId2] = [e].[Key2] AND [j].[CompositeId3] = [e].[Key3]
    WHERE [r].[Id] = [j].[LeafId])
FROM [Roots] AS [r]
INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
INNER JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]");
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
FROM [Roots] AS [r]
INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
LEFT JOIN (
    SELECT [t].[Id], [t].[Name], [t].[EntityBranchId]
    FROM (
        SELECT [e].[Id], [e].[Name], [j].[EntityBranchId], ROW_NUMBER() OVER(PARTITION BY [j].[EntityBranchId] ORDER BY [e].[Id] DESC) AS [row]
        FROM [JoinOneToBranch] AS [j]
        INNER JOIN [EntityOnes] AS [e] ON [j].[EntityOneId] = [e].[Id]
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [r].[Id] = [t0].[EntityBranchId]");
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
            @"SELECT [e].[Key1], [e].[Key2], [e].[Key3], [t0].[Id], [t0].[Name], [t0].[Number], [t0].[IsGreen], [t0].[LeafId], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [t].[Id], [t].[Name], [t].[Number], [t].[IsGreen], [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN (
        SELECT [r].[Id], [r].[Name], [b].[Number], [l].[IsGreen]
        FROM [Roots] AS [r]
        INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
        INNER JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
    ) AS [t] ON [j].[LeafId] = [t].[Id]
) AS [t0] ON [e].[Key1] = [t0].[CompositeId1] AND [e].[Key2] = [t0].[CompositeId2] AND [e].[Key3] = [t0].[CompositeId3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [t0].[LeafId], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3]");
    }

    public override async Task Skip_navigation_of_type(bool async)
    {
        await base.Skip_navigation_of_type(async);

        AssertSql(
            @"SELECT [e].[Key1], [e].[Key2], [e].[Key3], [t0].[Id], [t0].[Name], [t0].[Number], [t0].[IsGreen], [t0].[Discriminator], [t0].[RootSkipSharedId], [t0].[CompositeKeySkipSharedKey1], [t0].[CompositeKeySkipSharedKey2], [t0].[CompositeKeySkipSharedKey3]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [t].[Id], [t].[Name], [t].[Number], [t].[IsGreen], [t].[Discriminator], [e0].[RootSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3]
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
    ) AS [t] ON [e0].[RootSkipSharedId] = [t].[Id]
    WHERE [t].[Discriminator] = N'EntityLeaf'
) AS [t0] ON [e].[Key1] = [t0].[CompositeKeySkipSharedKey1] AND [e].[Key2] = [t0].[CompositeKeySkipSharedKey2] AND [e].[Key3] = [t0].[CompositeKeySkipSharedKey3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [t0].[RootSkipSharedId], [t0].[CompositeKeySkipSharedKey1], [t0].[CompositeKeySkipSharedKey2], [t0].[CompositeKeySkipSharedKey3]");
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
FROM [Roots] AS [r]
INNER JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId], [e].[RootSkipSharedId]
    FROM [EntityRootEntityThree] AS [e]
    INNER JOIN [EntityThrees] AS [e0] ON [e].[ThreeSkipSharedId] = [e0].[Id]
) AS [t] ON [r].[Id] = [t].[RootSkipSharedId]");
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
            @"SELECT [t0].[Id], [t0].[Name], [t0].[Number], [t0].[IsGreen], [t0].[Discriminator]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [t].[Id], [t].[Name], [t].[Number], [t].[IsGreen], [t].[Discriminator], [e0].[ThreeSkipSharedId]
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
    ) AS [t] ON [e0].[RootSkipSharedId] = [t].[Id]
    WHERE [t].[Discriminator] IN (N'EntityBranch', N'EntityLeaf')
) AS [t0] ON [e].[Id] = [t0].[ThreeSkipSharedId]");
    }

    public override async Task Select_many_over_skip_navigation_cast(bool async)
    {
        await base.Select_many_over_skip_navigation_cast(async);

        AssertSql(
            @"SELECT [t0].[Id], [t0].[Name], [t0].[Number], [t0].[IsGreen], [t0].[Discriminator]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [t].[Id], [t].[Name], [t].[Number], [t].[IsGreen], [t].[Discriminator], [j].[EntityOneId]
    FROM [JoinOneToBranch] AS [j]
    INNER JOIN (
        SELECT [r].[Id], [r].[Name], [b].[Number], [l].[IsGreen], CASE
            WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
        END AS [Discriminator]
        FROM [Roots] AS [r]
        INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
        LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
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
            @"SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [t0].[RootSkipSharedId], [t0].[CompositeKeySkipSharedKey1], [t0].[CompositeKeySkipSharedKey2], [t0].[CompositeKeySkipSharedKey3], [t0].[Id], [t0].[Name], [t0].[Number], [t0].[Slumber], [t0].[IsGreen], [t0].[IsBrown], [t0].[Discriminator]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [e0].[RootSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], [t].[Id], [t].[Name], [t].[Number], [t].[Slumber], [t].[IsGreen], [t].[IsBrown], [t].[Discriminator]
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
    ) AS [t] ON [e0].[RootSkipSharedId] = [t].[Id]
) AS [t0] ON [e].[Key1] = [t0].[CompositeKeySkipSharedKey1] AND [e].[Key2] = [t0].[CompositeKeySkipSharedKey2] AND [e].[Key3] = [t0].[CompositeKeySkipSharedKey3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [t0].[RootSkipSharedId], [t0].[CompositeKeySkipSharedKey1], [t0].[CompositeKeySkipSharedKey2], [t0].[CompositeKeySkipSharedKey3]");
    }

    public override async Task Include_skip_navigation_then_reference(bool async)
    {
        await base.Include_skip_navigation_then_reference(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId], [t].[OneId], [t].[TwoId], [t].[JoinOneToTwoExtraId], [t].[Id], [t].[Name], [t].[Id0], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name0], [t].[ReferenceInverseId]
FROM [EntityTwos] AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId]
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
            @"SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [t1].[LeafId], [t1].[CompositeId1], [t1].[CompositeId2], [t1].[CompositeId3], [t1].[Id], [t1].[Name], [t1].[Number], [t1].[IsGreen], [t1].[EntityBranchId], [t1].[EntityOneId], [t1].[Id0], [t1].[Name0]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [t].[Id], [t].[Name], [t].[Number], [t].[IsGreen], [t0].[EntityBranchId], [t0].[EntityOneId], [t0].[Id] AS [Id0], [t0].[Name] AS [Name0]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN (
        SELECT [r].[Id], [r].[Name], [b].[Number], [l].[IsGreen]
        FROM [Roots] AS [r]
        INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
        INNER JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
    ) AS [t] ON [j].[LeafId] = [t].[Id]
    LEFT JOIN (
        SELECT [j0].[EntityBranchId], [j0].[EntityOneId], [e0].[Id], [e0].[Name]
        FROM [JoinOneToBranch] AS [j0]
        INNER JOIN [EntityOnes] AS [e0] ON [j0].[EntityOneId] = [e0].[Id]
    ) AS [t0] ON [t].[Id] = [t0].[EntityBranchId]
) AS [t1] ON [e].[Key1] = [t1].[CompositeId1] AND [e].[Key2] = [t1].[CompositeId2] AND [e].[Key3] = [t1].[CompositeId3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [t1].[LeafId], [t1].[CompositeId1], [t1].[CompositeId2], [t1].[CompositeId3], [t1].[Id], [t1].[EntityBranchId], [t1].[EntityOneId]");
    }

    public override async Task Include_skip_navigation_then_include_reference_and_skip_navigation(bool async)
    {
        await base.Include_skip_navigation_then_include_reference_and_skip_navigation(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId], [t0].[OneId], [t0].[ThreeId], [t0].[Payload], [t0].[Id], [t0].[Name], [t0].[Id0], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name0], [t0].[ReferenceInverseId], [t0].[LeftId], [t0].[RightId], [t0].[Payload0], [t0].[Id1], [t0].[Name1]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId], [t].[LeftId], [t].[RightId], [t].[Payload] AS [Payload0], [t].[Id] AS [Id1], [t].[Name] AS [Name1]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
    LEFT JOIN (
        SELECT [j0].[LeftId], [j0].[RightId], [j0].[Payload], [e2].[Id], [e2].[Name]
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
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId], [e0].[Id], [t].[OneSkipSharedId], [t].[TwoSkipSharedId], [t].[Id], [t].[Name], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId]
FROM [EntityTwos] AS [e]
LEFT JOIN [EntityThrees] AS [e0] ON [e].[Id] = [e0].[ReferenceInverseId]
LEFT JOIN (
    SELECT [e1].[OneSkipSharedId], [e1].[TwoSkipSharedId], [e2].[Id], [e2].[Name]
    FROM [EntityOneEntityTwo] AS [e1]
    INNER JOIN [EntityOnes] AS [e2] ON [e1].[OneSkipSharedId] = [e2].[Id]
) AS [t] ON [e].[Id] = [t].[TwoSkipSharedId]
ORDER BY [e].[Id], [e0].[Id], [t].[OneSkipSharedId], [t].[TwoSkipSharedId]");
    }

    public override async Task Filtered_include_skip_navigation_where(bool async)
    {
        await base.Filtered_include_skip_navigation_where(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId], [t].[OneId], [t].[ThreeId], [t].[Payload], [t].[Id], [t].[Name]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [e0].[Id], [e0].[Name]
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
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId], [t].[ThreeId], [t].[TwoId], [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [j].[ThreeId], [j].[TwoId], [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId]
    FROM [JoinTwoToThree] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
) AS [t] ON [e].[Id] = [t].[ThreeId]
ORDER BY [e].[Id], [t].[Id], [t].[ThreeId]");
    }

    public override async Task Filtered_include_skip_navigation_order_by_skip(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId], [t0].[SelfSkipSharedLeftId], [t0].[SelfSkipSharedRightId], [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId]
FROM [EntityTwos] AS [e]
LEFT JOIN (
    SELECT [t].[SelfSkipSharedLeftId], [t].[SelfSkipSharedRightId], [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId]
    FROM (
        SELECT [e0].[SelfSkipSharedLeftId], [e0].[SelfSkipSharedRightId], [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [e0].[SelfSkipSharedLeftId] ORDER BY [e1].[Id]) AS [row]
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
            @"SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [t0].[TwoSkipSharedId], [t0].[CompositeKeySkipSharedKey1], [t0].[CompositeKeySkipSharedKey2], [t0].[CompositeKeySkipSharedKey3], [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [t].[TwoSkipSharedId], [t].[CompositeKeySkipSharedKey1], [t].[CompositeKeySkipSharedKey2], [t].[CompositeKeySkipSharedKey3], [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId]
    FROM (
        SELECT [e0].[TwoSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3] ORDER BY [e1].[Id]) AS [row]
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
            @"SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [t0].[Id], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[ThreeId], [t0].[Id0], [t0].[CollectionInverseId], [t0].[Name], [t0].[ReferenceInverseId]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [t].[Id], [t].[CompositeId1], [t].[CompositeId2], [t].[CompositeId3], [t].[ThreeId], [t].[Id0], [t].[CollectionInverseId], [t].[Name], [t].[ReferenceInverseId]
    FROM (
        SELECT [j].[Id], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [j].[ThreeId], [e0].[Id] AS [Id0], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinThreeToCompositeKeyFull] AS [j]
        INNER JOIN [EntityThrees] AS [e0] ON [j].[ThreeId] = [e0].[Id]
    ) AS [t]
    WHERE 1 < [t].[row] AND [t].[row] <= 3
) AS [t0] ON [e].[Key1] = [t0].[CompositeId1] AND [e].[Key2] = [t0].[CompositeId2] AND [e].[Key3] = [t0].[CompositeId3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[Id0]");
    }

    public override async Task Filtered_then_include_skip_navigation_where(bool async)
    {
        await base.Filtered_then_include_skip_navigation_where(async);

        AssertSql(
            @"SELECT [r].[Id], [r].[Name], [b].[Number], [b0].[Slumber], [l].[IsGreen], [l0].[IsBrown], CASE
    WHEN [l0].[Id] IS NOT NULL THEN N'EntityLeaf2'
    WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
    WHEN [b].[Id] IS NOT NULL THEN N'EntityBranch'
END AS [Discriminator], [t0].[RootSkipSharedId], [t0].[ThreeSkipSharedId], [t0].[Id], [t0].[CollectionInverseId], [t0].[Name], [t0].[ReferenceInverseId], [t0].[OneId], [t0].[ThreeId], [t0].[Payload], [t0].[Id0], [t0].[Name0]
FROM [Roots] AS [r]
LEFT JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
LEFT JOIN [Branch2s] AS [b0] ON [r].[Id] = [b0].[Id]
LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
LEFT JOIN [Leaf2s] AS [l0] ON [r].[Id] = [l0].[Id]
LEFT JOIN (
    SELECT [e].[RootSkipSharedId], [e].[ThreeSkipSharedId], [e0].[Id], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId], [t].[OneId], [t].[ThreeId], [t].[Payload], [t].[Id] AS [Id0], [t].[Name] AS [Name0]
    FROM [EntityRootEntityThree] AS [e]
    INNER JOIN [EntityThrees] AS [e0] ON [e].[ThreeSkipSharedId] = [e0].[Id]
    LEFT JOIN (
        SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [e1].[Id], [e1].[Name]
        FROM [JoinOneToThreePayloadFullShared] AS [j]
        INNER JOIN [EntityOnes] AS [e1] ON [j].[OneId] = [e1].[Id]
        WHERE [e1].[Id] < 10
    ) AS [t] ON [e0].[Id] = [t].[ThreeId]
) AS [t0] ON [r].[Id] = [t0].[RootSkipSharedId]
ORDER BY [r].[Id], [t0].[RootSkipSharedId], [t0].[ThreeSkipSharedId], [t0].[Id], [t0].[OneId], [t0].[ThreeId]");
    }

    public override async Task Filtered_then_include_skip_navigation_order_by_skip_take(bool async)
    {
        await base.Filtered_then_include_skip_navigation_order_by_skip_take(async);

        AssertSql(
            @"SELECT [r].[Id], [r].[Name], [b].[Number], [b0].[Slumber], [l].[IsGreen], [l0].[IsBrown], CASE
    WHEN [l0].[Id] IS NOT NULL THEN N'EntityLeaf2'
    WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
    WHEN [b].[Id] IS NOT NULL THEN N'EntityBranch'
END AS [Discriminator], [t1].[RootSkipSharedId], [t1].[CompositeKeySkipSharedKey1], [t1].[CompositeKeySkipSharedKey2], [t1].[CompositeKeySkipSharedKey3], [t1].[Key1], [t1].[Key2], [t1].[Key3], [t1].[Name], [t1].[Id], [t1].[CompositeId1], [t1].[CompositeId2], [t1].[CompositeId3], [t1].[ThreeId], [t1].[Id0], [t1].[CollectionInverseId], [t1].[Name0], [t1].[ReferenceInverseId]
FROM [Roots] AS [r]
LEFT JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
LEFT JOIN [Branch2s] AS [b0] ON [r].[Id] = [b0].[Id]
LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
LEFT JOIN [Leaf2s] AS [l0] ON [r].[Id] = [l0].[Id]
LEFT JOIN (
    SELECT [e].[RootSkipSharedId], [e].[CompositeKeySkipSharedKey1], [e].[CompositeKeySkipSharedKey2], [e].[CompositeKeySkipSharedKey3], [e0].[Key1], [e0].[Key2], [e0].[Key3], [e0].[Name], [t0].[Id], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[ThreeId], [t0].[Id0], [t0].[CollectionInverseId], [t0].[Name] AS [Name0], [t0].[ReferenceInverseId]
    FROM [EntityCompositeKeyEntityRoot] AS [e]
    INNER JOIN [EntityCompositeKeys] AS [e0] ON [e].[CompositeKeySkipSharedKey1] = [e0].[Key1] AND [e].[CompositeKeySkipSharedKey2] = [e0].[Key2] AND [e].[CompositeKeySkipSharedKey3] = [e0].[Key3]
    LEFT JOIN (
        SELECT [t].[Id], [t].[CompositeId1], [t].[CompositeId2], [t].[CompositeId3], [t].[ThreeId], [t].[Id0], [t].[CollectionInverseId], [t].[Name], [t].[ReferenceInverseId]
        FROM (
            SELECT [j].[Id], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [j].[ThreeId], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3] ORDER BY [e1].[Id]) AS [row]
            FROM [JoinThreeToCompositeKeyFull] AS [j]
            INNER JOIN [EntityThrees] AS [e1] ON [j].[ThreeId] = [e1].[Id]
        ) AS [t]
        WHERE 1 < [t].[row] AND [t].[row] <= 3
    ) AS [t0] ON [e0].[Key1] = [t0].[CompositeId1] AND [e0].[Key2] = [t0].[CompositeId2] AND [e0].[Key3] = [t0].[CompositeId3]
) AS [t1] ON [r].[Id] = [t1].[RootSkipSharedId]
ORDER BY [r].[Id], [t1].[RootSkipSharedId], [t1].[CompositeKeySkipSharedKey1], [t1].[CompositeKeySkipSharedKey2], [t1].[CompositeKeySkipSharedKey3], [t1].[Key1], [t1].[Key2], [t1].[Key3], [t1].[CompositeId1], [t1].[CompositeId2], [t1].[CompositeId3], [t1].[Id0]");
    }

    public override async Task Filtered_include_skip_navigation_where_then_include_skip_navigation(bool async)
    {
        await base.Filtered_include_skip_navigation_where_then_include_skip_navigation(async);

        AssertSql(
            @"SELECT [r].[Id], [r].[Name], [b].[Number], [l].[IsGreen], [t0].[LeafId], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[Key1], [t0].[Key2], [t0].[Key3], [t0].[Name], [t0].[TwoSkipSharedId], [t0].[CompositeKeySkipSharedKey1], [t0].[CompositeKeySkipSharedKey2], [t0].[CompositeKeySkipSharedKey3], [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name0], [t0].[ReferenceInverseId]
FROM [Roots] AS [r]
INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
INNER JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
LEFT JOIN (
    SELECT [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [t].[TwoSkipSharedId], [t].[CompositeKeySkipSharedKey1], [t].[CompositeKeySkipSharedKey2], [t].[CompositeKeySkipSharedKey3], [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name] AS [Name0], [t].[ReferenceInverseId]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN [EntityCompositeKeys] AS [e] ON [j].[CompositeId1] = [e].[Key1] AND [j].[CompositeId2] = [e].[Key2] AND [j].[CompositeId3] = [e].[Key3]
    LEFT JOIN (
        SELECT [e0].[TwoSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId]
        FROM [EntityCompositeKeyEntityTwo] AS [e0]
        INNER JOIN [EntityTwos] AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
    ) AS [t] ON [e].[Key1] = [t].[CompositeKeySkipSharedKey1] AND [e].[Key2] = [t].[CompositeKeySkipSharedKey2] AND [e].[Key3] = [t].[CompositeKeySkipSharedKey3]
    WHERE [e].[Key1] < 5
) AS [t0] ON [r].[Id] = [t0].[LeafId]
ORDER BY [r].[Id], [t0].[LeafId], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[Key1], [t0].[Key2], [t0].[Key3], [t0].[TwoSkipSharedId], [t0].[CompositeKeySkipSharedKey1], [t0].[CompositeKeySkipSharedKey2], [t0].[CompositeKeySkipSharedKey3]");
    }

    public override async Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[Name], [t1].[OneId], [t1].[TwoId], [t1].[JoinOneToTwoExtraId], [t1].[Id], [t1].[CollectionInverseId], [t1].[ExtraId], [t1].[Name], [t1].[ReferenceInverseId], [t1].[ThreeId], [t1].[TwoId0], [t1].[Id0], [t1].[CollectionInverseId0], [t1].[Name0], [t1].[ReferenceInverseId0]
FROM [EntityOnes] AS [e]
OUTER APPLY (
    SELECT [t].[OneId], [t].[TwoId], [t].[JoinOneToTwoExtraId], [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId], [t0].[ThreeId], [t0].[TwoId] AS [TwoId0], [t0].[Id] AS [Id0], [t0].[CollectionInverseId] AS [CollectionInverseId0], [t0].[Name] AS [Name0], [t0].[ReferenceInverseId] AS [ReferenceInverseId0]
    FROM (
        SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId]
        FROM [JoinOneToTwo] AS [j]
        INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
        WHERE [e].[Id] = [j].[OneId]
        ORDER BY [e0].[Id]
        OFFSET 1 ROWS FETCH NEXT 2 ROWS ONLY
    ) AS [t]
    LEFT JOIN (
        SELECT [j0].[ThreeId], [j0].[TwoId], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId]
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
            @"SELECT [e].[Id], [e].[Name], [t1].[OneId], [t1].[TwoId], [t1].[JoinOneToTwoExtraId], [t1].[Id], [t1].[CollectionInverseId], [t1].[ExtraId], [t1].[Name], [t1].[ReferenceInverseId], [t1].[ThreeId], [t1].[TwoId0], [t1].[Id0], [t1].[CollectionInverseId0], [t1].[Name0], [t1].[ReferenceInverseId0]
FROM [EntityOnes] AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], [t0].[ThreeId], [t0].[TwoId] AS [TwoId0], [t0].[Id] AS [Id0], [t0].[CollectionInverseId] AS [CollectionInverseId0], [t0].[Name] AS [Name0], [t0].[ReferenceInverseId] AS [ReferenceInverseId0]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    LEFT JOIN (
        SELECT [t].[ThreeId], [t].[TwoId], [t].[Id], [t].[CollectionInverseId], [t].[Name], [t].[ReferenceInverseId]
        FROM (
            SELECT [j0].[ThreeId], [j0].[TwoId], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j0].[TwoId] ORDER BY [e1].[Id]) AS [row]
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
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId], [t].[OneId], [t].[TwoId], [t].[JoinOneToTwoExtraId], [t].[Id], [t].[Name], [t].[Id0], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name0], [t].[ReferenceInverseId], [t].[Id1], [t].[CollectionInverseId0], [t].[ExtraId0], [t].[Name1], [t].[ReferenceInverseId0]
FROM [EntityTwos] AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId], [e2].[Id] AS [Id1], [e2].[CollectionInverseId] AS [CollectionInverseId0], [e2].[ExtraId] AS [ExtraId0], [e2].[Name] AS [Name1], [e2].[ReferenceInverseId] AS [ReferenceInverseId0]
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
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId], [t3].[OneId], [t3].[ThreeId], [t3].[Payload], [t3].[Id], [t3].[Name], [t3].[OneId0], [t3].[TwoId], [t3].[JoinOneToTwoExtraId], [t3].[Id0], [t3].[CollectionInverseId], [t3].[ExtraId], [t3].[Name0], [t3].[ReferenceInverseId], [t3].[EntityBranchId], [t3].[EntityOneId], [t3].[Id1], [t3].[Name1], [t3].[Number], [t3].[IsGreen], [t3].[Discriminator]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [e0].[Id], [e0].[Name], [t0].[OneId] AS [OneId0], [t0].[TwoId], [t0].[JoinOneToTwoExtraId], [t0].[Id] AS [Id0], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name] AS [Name0], [t0].[ReferenceInverseId], [t1].[EntityBranchId], [t1].[EntityOneId], [t1].[Id] AS [Id1], [t1].[Name] AS [Name1], [t1].[Number], [t1].[IsGreen], [t1].[Discriminator]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN (
        SELECT [t].[OneId], [t].[TwoId], [t].[JoinOneToTwoExtraId], [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId]
        FROM (
            SELECT [j0].[OneId], [j0].[TwoId], [j0].[JoinOneToTwoExtraId], [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j0].[OneId] ORDER BY [e1].[Id]) AS [row]
            FROM [JoinOneToTwo] AS [j0]
            INNER JOIN [EntityTwos] AS [e1] ON [j0].[TwoId] = [e1].[Id]
        ) AS [t]
        WHERE 1 < [t].[row] AND [t].[row] <= 3
    ) AS [t0] ON [e0].[Id] = [t0].[OneId]
    LEFT JOIN (
        SELECT [j1].[EntityBranchId], [j1].[EntityOneId], [t2].[Id], [t2].[Name], [t2].[Number], [t2].[IsGreen], [t2].[Discriminator]
        FROM [JoinOneToBranch] AS [j1]
        INNER JOIN (
            SELECT [r].[Id], [r].[Name], [b].[Number], [l].[IsGreen], CASE
                WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
            END AS [Discriminator]
            FROM [Roots] AS [r]
            INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
            LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
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
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId], [t0].[OneId], [t0].[ThreeId], [t0].[Payload], [t0].[Id], [t0].[Name], [t0].[Id0], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name0], [t0].[ReferenceInverseId]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [e0].[Id], [e0].[Name], [t].[Id] AS [Id0], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name] AS [Name0], [t].[ReferenceInverseId]
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
            @"SELECT [e].[Id], [e].[Name], [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId], [t0].[ThreeId], [t0].[TwoId], [t0].[Id0], [t0].[CollectionInverseId0], [t0].[Name0], [t0].[ReferenceInverseId0]
FROM [EntityOnes] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], [t].[ThreeId], [t].[TwoId], [t].[Id] AS [Id0], [t].[CollectionInverseId] AS [CollectionInverseId0], [t].[Name] AS [Name0], [t].[ReferenceInverseId] AS [ReferenceInverseId0]
    FROM [EntityTwos] AS [e0]
    LEFT JOIN (
        SELECT [j].[ThreeId], [j].[TwoId], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId]
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
            @"SELECT [t0].[RootSkipSharedId], [t0].[CompositeKeySkipSharedKey1], [t0].[CompositeKeySkipSharedKey2], [t0].[CompositeKeySkipSharedKey3], [t0].[Id], [t0].[Name], [t0].[Number], [t0].[Slumber], [t0].[IsGreen], [t0].[IsBrown], [t0].[Discriminator], [e].[Key1], [e].[Key2], [e].[Key3]
FROM [EntityCompositeKeys] AS [e]
INNER JOIN (
    SELECT [e0].[RootSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], [t].[Id], [t].[Name], [t].[Number], [t].[Slumber], [t].[IsGreen], [t].[IsBrown], [t].[Discriminator]
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
    ) AS [t] ON [e0].[RootSkipSharedId] = [t].[Id]
) AS [t0] ON [e].[Key1] = [t0].[CompositeKeySkipSharedKey1] AND [e].[Key2] = [t0].[CompositeKeySkipSharedKey2] AND [e].[Key3] = [t0].[CompositeKeySkipSharedKey3]
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
            @"SELECT [t].[OneId], [t].[TwoId], [t].[JoinOneToTwoExtraId], [t].[Id], [t].[Name], [t].[Id0], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name0], [t].[ReferenceInverseId], [e].[Id]
FROM [EntityTwos] AS [e]
INNER JOIN (
    SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId]
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
            @"SELECT [t0].[LeafId], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[Id], [t0].[Name], [t0].[Number], [t0].[IsGreen], [e].[Key1], [e].[Key2], [e].[Key3]
FROM [EntityCompositeKeys] AS [e]
INNER JOIN (
    SELECT [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [t].[Id], [t].[Name], [t].[Number], [t].[IsGreen]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN (
        SELECT [r].[Id], [r].[Name], [b].[Number], [l].[IsGreen]
        FROM [Roots] AS [r]
        INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
        INNER JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
    ) AS [t] ON [j].[LeafId] = [t].[Id]
) AS [t0] ON [e].[Key1] = [t0].[CompositeId1] AND [e].[Key2] = [t0].[CompositeId2] AND [e].[Key3] = [t0].[CompositeId3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [t0].[LeafId], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[Id]",
            //
            @"SELECT [t1].[EntityBranchId], [t1].[EntityOneId], [t1].[Id], [t1].[Name], [e].[Key1], [e].[Key2], [e].[Key3], [t0].[LeafId], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[Id]
FROM [EntityCompositeKeys] AS [e]
INNER JOIN (
    SELECT [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [t].[Id]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN (
        SELECT [r].[Id]
        FROM [Roots] AS [r]
        INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
        INNER JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
    ) AS [t] ON [j].[LeafId] = [t].[Id]
) AS [t0] ON [e].[Key1] = [t0].[CompositeId1] AND [e].[Key2] = [t0].[CompositeId2] AND [e].[Key3] = [t0].[CompositeId3]
INNER JOIN (
    SELECT [j0].[EntityBranchId], [j0].[EntityOneId], [e0].[Id], [e0].[Name]
    FROM [JoinOneToBranch] AS [j0]
    INNER JOIN [EntityOnes] AS [e0] ON [j0].[EntityOneId] = [e0].[Id]
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
            @"SELECT [t].[OneId], [t].[ThreeId], [t].[Payload], [t].[Id], [t].[Name], [t].[Id0], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name0], [t].[ReferenceInverseId], [e].[Id]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
) AS [t] ON [e].[Id] = [t].[ThreeId]
ORDER BY [e].[Id], [t].[OneId], [t].[ThreeId], [t].[Id], [t].[Id0]",
            //
            @"SELECT [t0].[LeftId], [t0].[RightId], [t0].[Payload], [t0].[Id], [t0].[Name], [e].[Id], [t].[OneId], [t].[ThreeId], [t].[Id], [t].[Id0]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [e0].[Id], [e1].[Id] AS [Id0]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
) AS [t] ON [e].[Id] = [t].[ThreeId]
INNER JOIN (
    SELECT [j0].[LeftId], [j0].[RightId], [j0].[Payload], [e2].[Id], [e2].[Name]
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
            @"SELECT [t].[OneSkipSharedId], [t].[TwoSkipSharedId], [t].[Id], [t].[Name], [e].[Id], [e0].[Id]
FROM [EntityTwos] AS [e]
LEFT JOIN [EntityThrees] AS [e0] ON [e].[Id] = [e0].[ReferenceInverseId]
INNER JOIN (
    SELECT [e1].[OneSkipSharedId], [e1].[TwoSkipSharedId], [e2].[Id], [e2].[Name]
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
            @"SELECT [t].[OneId], [t].[ThreeId], [t].[Payload], [t].[Id], [t].[Name], [e].[Id]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [e0].[Id], [e0].[Name]
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
            @"SELECT [t].[ThreeId], [t].[TwoId], [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId], [e].[Id]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [j].[ThreeId], [j].[TwoId], [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId]
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
            @"SELECT [t0].[SelfSkipSharedLeftId], [t0].[SelfSkipSharedRightId], [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId], [e].[Id]
FROM [EntityTwos] AS [e]
INNER JOIN (
    SELECT [t].[SelfSkipSharedLeftId], [t].[SelfSkipSharedRightId], [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId]
    FROM (
        SELECT [e0].[SelfSkipSharedLeftId], [e0].[SelfSkipSharedRightId], [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [e0].[SelfSkipSharedLeftId] ORDER BY [e1].[Id]) AS [row]
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
            @"SELECT [t0].[TwoSkipSharedId], [t0].[CompositeKeySkipSharedKey1], [t0].[CompositeKeySkipSharedKey2], [t0].[CompositeKeySkipSharedKey3], [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId], [e].[Key1], [e].[Key2], [e].[Key3]
FROM [EntityCompositeKeys] AS [e]
INNER JOIN (
    SELECT [t].[TwoSkipSharedId], [t].[CompositeKeySkipSharedKey1], [t].[CompositeKeySkipSharedKey2], [t].[CompositeKeySkipSharedKey3], [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId]
    FROM (
        SELECT [e0].[TwoSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3] ORDER BY [e1].[Id]) AS [row]
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
            @"SELECT [t0].[Id], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[ThreeId], [t0].[Id0], [t0].[CollectionInverseId], [t0].[Name], [t0].[ReferenceInverseId], [e].[Key1], [e].[Key2], [e].[Key3]
FROM [EntityCompositeKeys] AS [e]
INNER JOIN (
    SELECT [t].[Id], [t].[CompositeId1], [t].[CompositeId2], [t].[CompositeId3], [t].[ThreeId], [t].[Id0], [t].[CollectionInverseId], [t].[Name], [t].[ReferenceInverseId]
    FROM (
        SELECT [j].[Id], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [j].[ThreeId], [e0].[Id] AS [Id0], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinThreeToCompositeKeyFull] AS [j]
        INNER JOIN [EntityThrees] AS [e0] ON [j].[ThreeId] = [e0].[Id]
    ) AS [t]
    WHERE 1 < [t].[row] AND [t].[row] <= 3
) AS [t0] ON [e].[Key1] = [t0].[CompositeId1] AND [e].[Key2] = [t0].[CompositeId2] AND [e].[Key3] = [t0].[CompositeId3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[Id0]");
    }

    public override async Task Filtered_then_include_skip_navigation_where_split(bool async)
    {
        await base.Filtered_then_include_skip_navigation_where_split(async);

        AssertSql(
            @"SELECT [r].[Id], [r].[Name], [b].[Number], [b0].[Slumber], [l].[IsGreen], [l0].[IsBrown], CASE
    WHEN [l0].[Id] IS NOT NULL THEN N'EntityLeaf2'
    WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
    WHEN [b].[Id] IS NOT NULL THEN N'EntityBranch'
END AS [Discriminator]
FROM [Roots] AS [r]
LEFT JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
LEFT JOIN [Branch2s] AS [b0] ON [r].[Id] = [b0].[Id]
LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
LEFT JOIN [Leaf2s] AS [l0] ON [r].[Id] = [l0].[Id]
ORDER BY [r].[Id]",
            //
            @"SELECT [t].[RootSkipSharedId], [t].[ThreeSkipSharedId], [t].[Id], [t].[CollectionInverseId], [t].[Name], [t].[ReferenceInverseId], [r].[Id]
FROM [Roots] AS [r]
INNER JOIN (
    SELECT [e].[RootSkipSharedId], [e].[ThreeSkipSharedId], [e0].[Id], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId]
    FROM [EntityRootEntityThree] AS [e]
    INNER JOIN [EntityThrees] AS [e0] ON [e].[ThreeSkipSharedId] = [e0].[Id]
) AS [t] ON [r].[Id] = [t].[RootSkipSharedId]
ORDER BY [r].[Id], [t].[RootSkipSharedId], [t].[ThreeSkipSharedId], [t].[Id]",
            //
            @"SELECT [t0].[OneId], [t0].[ThreeId], [t0].[Payload], [t0].[Id], [t0].[Name], [r].[Id], [t].[RootSkipSharedId], [t].[ThreeSkipSharedId], [t].[Id]
FROM [Roots] AS [r]
INNER JOIN (
    SELECT [e].[RootSkipSharedId], [e].[ThreeSkipSharedId], [e0].[Id]
    FROM [EntityRootEntityThree] AS [e]
    INNER JOIN [EntityThrees] AS [e0] ON [e].[ThreeSkipSharedId] = [e0].[Id]
) AS [t] ON [r].[Id] = [t].[RootSkipSharedId]
INNER JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [e1].[Id], [e1].[Name]
    FROM [JoinOneToThreePayloadFullShared] AS [j]
    INNER JOIN [EntityOnes] AS [e1] ON [j].[OneId] = [e1].[Id]
    WHERE [e1].[Id] < 10
) AS [t0] ON [t].[Id] = [t0].[ThreeId]
ORDER BY [r].[Id], [t].[RootSkipSharedId], [t].[ThreeSkipSharedId], [t].[Id]");
    }

    public override async Task Filtered_then_include_skip_navigation_order_by_skip_take_split(bool async)
    {
        await base.Filtered_then_include_skip_navigation_order_by_skip_take_split(async);

        AssertSql(
            @"SELECT [r].[Id], [r].[Name], [b].[Number], [b0].[Slumber], [l].[IsGreen], [l0].[IsBrown], CASE
    WHEN [l0].[Id] IS NOT NULL THEN N'EntityLeaf2'
    WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
    WHEN [b].[Id] IS NOT NULL THEN N'EntityBranch'
END AS [Discriminator]
FROM [Roots] AS [r]
LEFT JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
LEFT JOIN [Branch2s] AS [b0] ON [r].[Id] = [b0].[Id]
LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
LEFT JOIN [Leaf2s] AS [l0] ON [r].[Id] = [l0].[Id]
ORDER BY [r].[Id]",
            //
            @"SELECT [t].[RootSkipSharedId], [t].[CompositeKeySkipSharedKey1], [t].[CompositeKeySkipSharedKey2], [t].[CompositeKeySkipSharedKey3], [t].[Key1], [t].[Key2], [t].[Key3], [t].[Name], [r].[Id]
FROM [Roots] AS [r]
INNER JOIN (
    SELECT [e].[RootSkipSharedId], [e].[CompositeKeySkipSharedKey1], [e].[CompositeKeySkipSharedKey2], [e].[CompositeKeySkipSharedKey3], [e0].[Key1], [e0].[Key2], [e0].[Key3], [e0].[Name]
    FROM [EntityCompositeKeyEntityRoot] AS [e]
    INNER JOIN [EntityCompositeKeys] AS [e0] ON [e].[CompositeKeySkipSharedKey1] = [e0].[Key1] AND [e].[CompositeKeySkipSharedKey2] = [e0].[Key2] AND [e].[CompositeKeySkipSharedKey3] = [e0].[Key3]
) AS [t] ON [r].[Id] = [t].[RootSkipSharedId]
ORDER BY [r].[Id], [t].[RootSkipSharedId], [t].[CompositeKeySkipSharedKey1], [t].[CompositeKeySkipSharedKey2], [t].[CompositeKeySkipSharedKey3], [t].[Key1], [t].[Key2], [t].[Key3]",
            //
            @"SELECT [t0].[Id], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[ThreeId], [t0].[Id0], [t0].[CollectionInverseId], [t0].[Name], [t0].[ReferenceInverseId], [r].[Id], [t].[RootSkipSharedId], [t].[CompositeKeySkipSharedKey1], [t].[CompositeKeySkipSharedKey2], [t].[CompositeKeySkipSharedKey3], [t].[Key1], [t].[Key2], [t].[Key3]
FROM [Roots] AS [r]
INNER JOIN (
    SELECT [e].[RootSkipSharedId], [e].[CompositeKeySkipSharedKey1], [e].[CompositeKeySkipSharedKey2], [e].[CompositeKeySkipSharedKey3], [e0].[Key1], [e0].[Key2], [e0].[Key3]
    FROM [EntityCompositeKeyEntityRoot] AS [e]
    INNER JOIN [EntityCompositeKeys] AS [e0] ON [e].[CompositeKeySkipSharedKey1] = [e0].[Key1] AND [e].[CompositeKeySkipSharedKey2] = [e0].[Key2] AND [e].[CompositeKeySkipSharedKey3] = [e0].[Key3]
) AS [t] ON [r].[Id] = [t].[RootSkipSharedId]
INNER JOIN (
    SELECT [t1].[Id], [t1].[CompositeId1], [t1].[CompositeId2], [t1].[CompositeId3], [t1].[ThreeId], [t1].[Id0], [t1].[CollectionInverseId], [t1].[Name], [t1].[ReferenceInverseId]
    FROM (
        SELECT [j].[Id], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [j].[ThreeId], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3] ORDER BY [e1].[Id]) AS [row]
        FROM [JoinThreeToCompositeKeyFull] AS [j]
        INNER JOIN [EntityThrees] AS [e1] ON [j].[ThreeId] = [e1].[Id]
    ) AS [t1]
    WHERE 1 < [t1].[row] AND [t1].[row] <= 3
) AS [t0] ON [t].[Key1] = [t0].[CompositeId1] AND [t].[Key2] = [t0].[CompositeId2] AND [t].[Key3] = [t0].[CompositeId3]
ORDER BY [r].[Id], [t].[RootSkipSharedId], [t].[CompositeKeySkipSharedKey1], [t].[CompositeKeySkipSharedKey2], [t].[CompositeKeySkipSharedKey3], [t].[Key1], [t].[Key2], [t].[Key3], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[Id0]");
    }

    public override async Task Filtered_include_skip_navigation_where_then_include_skip_navigation_split(bool async)
    {
        await base.Filtered_include_skip_navigation_where_then_include_skip_navigation_split(async);

        AssertSql(
            @"SELECT [r].[Id], [r].[Name], [b].[Number], [l].[IsGreen]
FROM [Roots] AS [r]
INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
INNER JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
ORDER BY [r].[Id]",
            //
            @"SELECT [t].[LeafId], [t].[CompositeId1], [t].[CompositeId2], [t].[CompositeId3], [t].[Key1], [t].[Key2], [t].[Key3], [t].[Name], [r].[Id]
FROM [Roots] AS [r]
INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
INNER JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
INNER JOIN (
    SELECT [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN [EntityCompositeKeys] AS [e] ON [j].[CompositeId1] = [e].[Key1] AND [j].[CompositeId2] = [e].[Key2] AND [j].[CompositeId3] = [e].[Key3]
    WHERE [e].[Key1] < 5
) AS [t] ON [r].[Id] = [t].[LeafId]
ORDER BY [r].[Id], [t].[LeafId], [t].[CompositeId1], [t].[CompositeId2], [t].[CompositeId3], [t].[Key1], [t].[Key2], [t].[Key3]",
            //
            @"SELECT [t0].[TwoSkipSharedId], [t0].[CompositeKeySkipSharedKey1], [t0].[CompositeKeySkipSharedKey2], [t0].[CompositeKeySkipSharedKey3], [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId], [r].[Id], [t].[LeafId], [t].[CompositeId1], [t].[CompositeId2], [t].[CompositeId3], [t].[Key1], [t].[Key2], [t].[Key3]
FROM [Roots] AS [r]
INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
INNER JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
INNER JOIN (
    SELECT [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [e].[Key1], [e].[Key2], [e].[Key3]
    FROM [JoinCompositeKeyToLeaf] AS [j]
    INNER JOIN [EntityCompositeKeys] AS [e] ON [j].[CompositeId1] = [e].[Key1] AND [j].[CompositeId2] = [e].[Key2] AND [j].[CompositeId3] = [e].[Key3]
    WHERE [e].[Key1] < 5
) AS [t] ON [r].[Id] = [t].[LeafId]
INNER JOIN (
    SELECT [e0].[TwoSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId]
    FROM [EntityCompositeKeyEntityTwo] AS [e0]
    INNER JOIN [EntityTwos] AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
) AS [t0] ON [t].[Key1] = [t0].[CompositeKeySkipSharedKey1] AND [t].[Key2] = [t0].[CompositeKeySkipSharedKey2] AND [t].[Key3] = [t0].[CompositeKeySkipSharedKey3]
ORDER BY [r].[Id], [t].[LeafId], [t].[CompositeId1], [t].[CompositeId2], [t].[CompositeId3], [t].[Key1], [t].[Key2], [t].[Key3]");
    }

    public override async Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where_split(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where_split(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[Name]
FROM [EntityOnes] AS [e]
ORDER BY [e].[Id]",
            //
            @"SELECT [t0].[OneId], [t0].[TwoId], [t0].[JoinOneToTwoExtraId], [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId], [e].[Id]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [t].[OneId], [t].[TwoId], [t].[JoinOneToTwoExtraId], [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId]
    FROM (
        SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j].[OneId] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinOneToTwo] AS [j]
        INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    ) AS [t]
    WHERE 1 < [t].[row] AND [t].[row] <= 3
) AS [t0] ON [e].[Id] = [t0].[OneId]
ORDER BY [e].[Id], [t0].[OneId], [t0].[Id], [t0].[TwoId]",
            //
            @"SELECT [t1].[ThreeId], [t1].[TwoId], [t1].[Id], [t1].[CollectionInverseId], [t1].[Name], [t1].[ReferenceInverseId], [e].[Id], [t0].[OneId], [t0].[TwoId], [t0].[Id]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [t].[OneId], [t].[TwoId], [t].[Id]
    FROM (
        SELECT [j].[OneId], [j].[TwoId], [e0].[Id], ROW_NUMBER() OVER(PARTITION BY [j].[OneId] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinOneToTwo] AS [j]
        INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    ) AS [t]
    WHERE 1 < [t].[row] AND [t].[row] <= 3
) AS [t0] ON [e].[Id] = [t0].[OneId]
INNER JOIN (
    SELECT [j0].[ThreeId], [j0].[TwoId], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId]
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
            @"SELECT [t].[OneId], [t].[TwoId], [t].[JoinOneToTwoExtraId], [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId], [e].[Id]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [t] ON [e].[Id] = [t].[OneId]
ORDER BY [e].[Id], [t].[OneId], [t].[TwoId], [t].[Id]",
            //
            @"SELECT [t0].[ThreeId], [t0].[TwoId], [t0].[Id], [t0].[CollectionInverseId], [t0].[Name], [t0].[ReferenceInverseId], [e].[Id], [t].[OneId], [t].[TwoId], [t].[Id]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [j].[OneId], [j].[TwoId], [e0].[Id]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [t] ON [e].[Id] = [t].[OneId]
INNER JOIN (
    SELECT [t1].[ThreeId], [t1].[TwoId], [t1].[Id], [t1].[CollectionInverseId], [t1].[Name], [t1].[ReferenceInverseId]
    FROM (
        SELECT [j0].[ThreeId], [j0].[TwoId], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j0].[TwoId] ORDER BY [e1].[Id]) AS [row]
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
            @"SELECT [t].[OneId], [t].[TwoId], [t].[JoinOneToTwoExtraId], [t].[Id], [t].[Name], [t].[Id0], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name0], [t].[ReferenceInverseId], [e].[Id]
FROM [EntityTwos] AS [e]
INNER JOIN (
    SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId]
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
    SELECT [j].[OneId], [j].[TwoId], [e0].[Id], [e1].[Id] AS [Id0]
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
            @"SELECT [t].[OneId], [t].[ThreeId], [t].[Payload], [t].[Id], [t].[Name], [e].[Id]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [e0].[Id], [e0].[Name]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [t] ON [e].[Id] = [t].[ThreeId]
ORDER BY [e].[Id], [t].[OneId], [t].[ThreeId], [t].[Id]",
            //
            @"SELECT [t0].[OneId], [t0].[TwoId], [t0].[JoinOneToTwoExtraId], [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId], [e].[Id], [t].[OneId], [t].[ThreeId], [t].[Id]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [e0].[Id]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [t] ON [e].[Id] = [t].[ThreeId]
INNER JOIN (
    SELECT [t1].[OneId], [t1].[TwoId], [t1].[JoinOneToTwoExtraId], [t1].[Id], [t1].[CollectionInverseId], [t1].[ExtraId], [t1].[Name], [t1].[ReferenceInverseId]
    FROM (
        SELECT [j0].[OneId], [j0].[TwoId], [j0].[JoinOneToTwoExtraId], [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j0].[OneId] ORDER BY [e1].[Id]) AS [row]
        FROM [JoinOneToTwo] AS [j0]
        INNER JOIN [EntityTwos] AS [e1] ON [j0].[TwoId] = [e1].[Id]
    ) AS [t1]
    WHERE 1 < [t1].[row] AND [t1].[row] <= 3
) AS [t0] ON [t].[Id] = [t0].[OneId]
ORDER BY [e].[Id], [t].[OneId], [t].[ThreeId], [t].[Id], [t0].[OneId], [t0].[Id]",
            //
            @"SELECT [t0].[EntityBranchId], [t0].[EntityOneId], [t0].[Id], [t0].[Name], [t0].[Number], [t0].[IsGreen], [t0].[Discriminator], [e].[Id], [t].[OneId], [t].[ThreeId], [t].[Id]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [e0].[Id]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [t] ON [e].[Id] = [t].[ThreeId]
INNER JOIN (
    SELECT [j0].[EntityBranchId], [j0].[EntityOneId], [t1].[Id], [t1].[Name], [t1].[Number], [t1].[IsGreen], [t1].[Discriminator]
    FROM [JoinOneToBranch] AS [j0]
    INNER JOIN (
        SELECT [r].[Id], [r].[Name], [b].[Number], [l].[IsGreen], CASE
            WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
        END AS [Discriminator]
        FROM [Roots] AS [r]
        INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
        LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
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
            @"SELECT [t].[OneId], [t].[ThreeId], [t].[Payload], [t].[Id], [t].[Name], [e].[Id]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [e0].[Id], [e0].[Name]
    FROM [JoinOneToThreePayloadFull] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e0].[Id] > 15
) AS [t] ON [e].[Id] = [t].[ThreeId]
ORDER BY [e].[Id], [t].[OneId], [t].[ThreeId], [t].[Id]",
            //
            @"SELECT [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId], [e].[Id], [t].[OneId], [t].[ThreeId], [t].[Id]
FROM [EntityThrees] AS [e]
INNER JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [e0].[Id]
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
            @"SELECT [t0].[ThreeId], [t0].[TwoId], [t0].[Id], [t0].[CollectionInverseId], [t0].[Name], [t0].[ReferenceInverseId], [e].[Id], [t].[Id]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId]
    FROM [EntityTwos] AS [e0]
    WHERE [e0].[Id] > 15
) AS [t] ON [e].[Id] = [t].[CollectionInverseId]
INNER JOIN (
    SELECT [j].[ThreeId], [j].[TwoId], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId]
    FROM [JoinTwoToThree] AS [j]
    INNER JOIN [EntityThrees] AS [e1] ON [j].[ThreeId] = [e1].[Id]
    WHERE [e1].[Id] < 5
) AS [t0] ON [t].[Id] = [t0].[TwoId]
ORDER BY [e].[Id], [t].[Id]");
    }

    public override async Task Include_skip_navigation_then_include_inverse_works_for_tracking_query(bool async)
    {
        await base.Include_skip_navigation_then_include_inverse_works_for_tracking_query(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId], [t0].[OneId], [t0].[ThreeId], [t0].[Payload], [t0].[Id], [t0].[Name], [t0].[OneId0], [t0].[ThreeId0], [t0].[Payload0], [t0].[Id0], [t0].[CollectionInverseId], [t0].[Name0], [t0].[ReferenceInverseId]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [e0].[Id], [e0].[Name], [t].[OneId] AS [OneId0], [t].[ThreeId] AS [ThreeId0], [t].[Payload] AS [Payload0], [t].[Id] AS [Id0], [t].[CollectionInverseId], [t].[Name] AS [Name0], [t].[ReferenceInverseId]
    FROM [JoinOneToThreePayloadFullShared] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN (
        SELECT [j0].[OneId], [j0].[ThreeId], [j0].[Payload], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId]
        FROM [JoinOneToThreePayloadFullShared] AS [j0]
        INNER JOIN [EntityThrees] AS [e1] ON [j0].[ThreeId] = [e1].[Id]
    ) AS [t] ON [e0].[Id] = [t].[OneId]
) AS [t0] ON [e].[Id] = [t0].[ThreeId]
ORDER BY [e].[Id], [t0].[OneId], [t0].[ThreeId], [t0].[Id], [t0].[OneId0], [t0].[ThreeId0]");
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
            @"SELECT [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId]
FROM [EntityOnes] AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId], [j].[OneId]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
) AS [t] ON [e].[Id] = [t].[OneId] AND [e].[Id] <> [t].[Id]");
    }

    public override async Task Contains_on_skip_collection_navigation(bool async)
    {
        await base.Contains_on_skip_collection_navigation(async);

        AssertSql(
            @"@__entity_equality_two_0_Id='1' (Nullable = true)

SELECT [e].[Id], [e].[Name]
FROM [EntityOnes] AS [e]
WHERE EXISTS (
    SELECT 1
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
    WHERE [e].[Id] = [j].[OneId] AND [e0].[Id] = @__entity_equality_two_0_Id)");
    }

    public override async Task GetType_in_hierarchy_in_base_type(bool async)
    {
        await base.GetType_in_hierarchy_in_base_type(async);

        AssertSql(
            @"SELECT [r].[Id], [r].[Name], [b].[Number], [b0].[Slumber], [l].[IsGreen], [l0].[IsBrown], CASE
    WHEN [l0].[Id] IS NOT NULL THEN N'EntityLeaf2'
    WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
    WHEN [b].[Id] IS NOT NULL THEN N'EntityBranch'
END AS [Discriminator]
FROM [Roots] AS [r]
LEFT JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
LEFT JOIN [Branch2s] AS [b0] ON [r].[Id] = [b0].[Id]
LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
LEFT JOIN [Leaf2s] AS [l0] ON [r].[Id] = [l0].[Id]
WHERE ([l0].[Id] IS NULL) AND ([l].[Id] IS NULL) AND ([b].[Id] IS NULL)");
    }

    public override async Task GetType_in_hierarchy_in_intermediate_type(bool async)
    {
        await base.GetType_in_hierarchy_in_intermediate_type(async);

        AssertSql(
            @"SELECT [r].[Id], [r].[Name], [b].[Number], [b0].[Slumber], [l].[IsGreen], [l0].[IsBrown], CASE
    WHEN [l0].[Id] IS NOT NULL THEN N'EntityLeaf2'
    WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
    WHEN [b].[Id] IS NOT NULL THEN N'EntityBranch'
END AS [Discriminator]
FROM [Roots] AS [r]
LEFT JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
LEFT JOIN [Branch2s] AS [b0] ON [r].[Id] = [b0].[Id]
LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
LEFT JOIN [Leaf2s] AS [l0] ON [r].[Id] = [l0].[Id]
WHERE ([l].[Id] IS NULL) AND ([b].[Id] IS NOT NULL)");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type(bool async)
    {
        await base.GetType_in_hierarchy_in_leaf_type(async);

        AssertSql(
            @"SELECT [r].[Id], [r].[Name], [b].[Number], [b0].[Slumber], [l].[IsGreen], [l0].[IsBrown], CASE
    WHEN [l0].[Id] IS NOT NULL THEN N'EntityLeaf2'
    WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
    WHEN [b].[Id] IS NOT NULL THEN N'EntityBranch'
END AS [Discriminator]
FROM [Roots] AS [r]
LEFT JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
LEFT JOIN [Branch2s] AS [b0] ON [r].[Id] = [b0].[Id]
LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
LEFT JOIN [Leaf2s] AS [l0] ON [r].[Id] = [l0].[Id]
WHERE [l].[Id] IS NOT NULL");
    }

    public override async Task GetType_in_hierarchy_in_querying_base_type(bool async)
    {
        await base.GetType_in_hierarchy_in_querying_base_type(async);

        AssertSql(
            @"SELECT [r].[Id], [r].[Name], [b].[Number], [l].[IsGreen], CASE
    WHEN [l].[Id] IS NOT NULL THEN N'EntityLeaf'
END AS [Discriminator]
FROM [Roots] AS [r]
INNER JOIN [Branches] AS [b] ON [r].[Id] = [b].[Id]
LEFT JOIN [Leaves] AS [l] ON [r].[Id] = [l].[Id]
WHERE 0 = 1");
    }

    public override async Task Filtered_include_skip_navigation_order_by_take_EF_Property(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_take_EF_Property(async);

        AssertSql(
            @"SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [t0].[TwoSkipSharedId], [t0].[CompositeKeySkipSharedKey1], [t0].[CompositeKeySkipSharedKey2], [t0].[CompositeKeySkipSharedKey3], [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId]
FROM [EntityCompositeKeys] AS [e]
LEFT JOIN (
    SELECT [t].[TwoSkipSharedId], [t].[CompositeKeySkipSharedKey1], [t].[CompositeKeySkipSharedKey2], [t].[CompositeKeySkipSharedKey3], [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId]
    FROM (
        SELECT [e0].[TwoSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3] ORDER BY [e1].[Id]) AS [row]
        FROM [EntityCompositeKeyEntityTwo] AS [e0]
        INNER JOIN [EntityTwos] AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
    ) AS [t]
    WHERE [t].[row] <= 2
) AS [t0] ON [e].[Key1] = [t0].[CompositeKeySkipSharedKey1] AND [e].[Key2] = [t0].[CompositeKeySkipSharedKey2] AND [e].[Key3] = [t0].[CompositeKeySkipSharedKey3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [t0].[CompositeKeySkipSharedKey1], [t0].[CompositeKeySkipSharedKey2], [t0].[CompositeKeySkipSharedKey3], [t0].[Id]");
    }

    public override async Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where_EF_Property(
        bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where_EF_Property(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[Name], [t1].[OneId], [t1].[TwoId], [t1].[JoinOneToTwoExtraId], [t1].[Id], [t1].[CollectionInverseId], [t1].[ExtraId], [t1].[Name], [t1].[ReferenceInverseId], [t1].[ThreeId], [t1].[TwoId0], [t1].[Id0], [t1].[CollectionInverseId0], [t1].[Name0], [t1].[ReferenceInverseId0]
FROM [EntityOnes] AS [e]
OUTER APPLY (
    SELECT [t].[OneId], [t].[TwoId], [t].[JoinOneToTwoExtraId], [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId], [t0].[ThreeId], [t0].[TwoId] AS [TwoId0], [t0].[Id] AS [Id0], [t0].[CollectionInverseId] AS [CollectionInverseId0], [t0].[Name] AS [Name0], [t0].[ReferenceInverseId] AS [ReferenceInverseId0]
    FROM (
        SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[ReferenceInverseId]
        FROM [JoinOneToTwo] AS [j]
        INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
        WHERE [e].[Id] = [j].[OneId]
        ORDER BY [e0].[Id]
        OFFSET 1 ROWS FETCH NEXT 2 ROWS ONLY
    ) AS [t]
    LEFT JOIN (
        SELECT [j0].[ThreeId], [j0].[TwoId], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId]
        FROM [JoinTwoToThree] AS [j0]
        INNER JOIN [EntityThrees] AS [e1] ON [j0].[ThreeId] = [e1].[Id]
        WHERE [e1].[Id] < 10
    ) AS [t0] ON [t].[Id] = [t0].[TwoId]
) AS [t1]
ORDER BY [e].[Id], [t1].[Id], [t1].[OneId], [t1].[TwoId], [t1].[ThreeId], [t1].[TwoId0]");
    }

    public override async Task Skip_navigation_all_unidirectional(bool async)
    {
        await base.Skip_navigation_all_unidirectional(async);

        AssertSql(
            @"SELECT [u].[Id], [u].[Name]
FROM [UnidirectionalEntityOnes] AS [u]
WHERE NOT EXISTS (
    SELECT 1
    FROM [UnidirectionalJoinOneToTwo] AS [u0]
    INNER JOIN [UnidirectionalEntityTwos] AS [u1] ON [u0].[TwoId] = [u1].[Id]
    WHERE [u].[Id] = [u0].[OneId] AND NOT ([u1].[Name] LIKE N'%B%'))");
    }

    public override async Task Skip_navigation_any_with_predicate_unidirectional(bool async)
    {
        await base.Skip_navigation_any_with_predicate_unidirectional(async);

        AssertSql(
            @"SELECT [u].[Id], [u].[Name]
FROM [UnidirectionalEntityOnes] AS [u]
WHERE EXISTS (
    SELECT 1
    FROM [UnidirectionalEntityOneUnidirectionalEntityTwo] AS [u0]
    INNER JOIN [UnidirectionalEntityTwos] AS [u1] ON [u0].[TwoSkipSharedId] = [u1].[Id]
    WHERE [u].[Id] = [u0].[UnidirectionalEntityOneId] AND ([u1].[Name] LIKE N'%B%'))");
    }

    public override async Task Skip_navigation_contains_unidirectional(bool async)
    {
        await base.Skip_navigation_contains_unidirectional(async);

        AssertSql(
            @"SELECT [u].[Id], [u].[Name]
FROM [UnidirectionalEntityOnes] AS [u]
WHERE EXISTS (
    SELECT 1
    FROM [UnidirectionalJoinOneToThreePayloadFullShared] AS [u0]
    INNER JOIN [UnidirectionalEntityThrees] AS [u1] ON [u0].[ThreeId] = [u1].[Id]
    WHERE [u].[Id] = [u0].[OneId] AND [u1].[Id] = 1)");
    }

    public override async Task Skip_navigation_count_without_predicate_unidirectional(bool async)
    {
        await base.Skip_navigation_count_without_predicate_unidirectional(async);

        AssertSql(
            @"SELECT [u].[Id], [u].[Name]
FROM [UnidirectionalEntityOnes] AS [u]
WHERE (
    SELECT COUNT(*)
    FROM [UnidirectionalJoinOneSelfPayload] AS [u0]
    INNER JOIN [UnidirectionalEntityOnes] AS [u1] ON [u0].[LeftId] = [u1].[Id]
    WHERE [u].[Id] = [u0].[RightId]) > 0");
    }

    public override async Task Skip_navigation_count_with_predicate_unidirectional(bool async)
    {
        await base.Skip_navigation_count_with_predicate_unidirectional(async);

        AssertSql(
            @"SELECT [u].[Id], [u].[Name]
FROM [UnidirectionalEntityOnes] AS [u]
ORDER BY (
    SELECT COUNT(*)
    FROM [UnidirectionalJoinOneToBranch] AS [u0]
    INNER JOIN (
        SELECT [u1].[Id], [u1].[Name], [u2].[Number], [u3].[IsGreen], CASE
            WHEN [u3].[Id] IS NOT NULL THEN N'UnidirectionalEntityLeaf'
        END AS [Discriminator]
        FROM [UnidirectionalRoots] AS [u1]
        INNER JOIN [UnidirectionalBranches] AS [u2] ON [u1].[Id] = [u2].[Id]
        LEFT JOIN [UnidirectionalLeaves] AS [u3] ON [u1].[Id] = [u3].[Id]
    ) AS [t] ON [u0].[UnidirectionalEntityBranchId] = [t].[Id]
    WHERE [u].[Id] = [u0].[UnidirectionalEntityOneId] AND ([t].[Name] IS NOT NULL) AND ([t].[Name] LIKE N'L%')), [u].[Id]");
    }

    public override async Task Skip_navigation_select_subquery_average_unidirectional(bool async)
    {
        await base.Skip_navigation_select_subquery_average_unidirectional(async);

        AssertSql(
            @"SELECT (
    SELECT AVG(CAST([u3].[Key1] AS float))
    FROM [UnidirectionalJoinCompositeKeyToLeaf] AS [u2]
    INNER JOIN [UnidirectionalEntityCompositeKeys] AS [u3] ON [u2].[CompositeId1] = [u3].[Key1] AND [u2].[CompositeId2] = [u3].[Key2] AND [u2].[CompositeId3] = [u3].[Key3]
    WHERE [u].[Id] = [u2].[LeafId])
FROM [UnidirectionalRoots] AS [u]
INNER JOIN [UnidirectionalBranches] AS [u0] ON [u].[Id] = [u0].[Id]
INNER JOIN [UnidirectionalLeaves] AS [u1] ON [u].[Id] = [u1].[Id]");
    }

    public override async Task Skip_navigation_order_by_reverse_first_or_default_unidirectional(bool async)
    {
        await base.Skip_navigation_order_by_reverse_first_or_default_unidirectional(async);

        AssertSql(
            @"SELECT [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId]
FROM [UnidirectionalEntityThrees] AS [u]
LEFT JOIN (
    SELECT [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId], [t].[ThreeId]
    FROM (
        SELECT [u1].[Id], [u1].[CollectionInverseId], [u1].[ExtraId], [u1].[Name], [u1].[ReferenceInverseId], [u0].[ThreeId], ROW_NUMBER() OVER(PARTITION BY [u0].[ThreeId] ORDER BY [u1].[Id] DESC) AS [row]
        FROM [UnidirectionalJoinTwoToThree] AS [u0]
        INNER JOIN [UnidirectionalEntityTwos] AS [u1] ON [u0].[TwoId] = [u1].[Id]
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [u].[Id] = [t0].[ThreeId]");
    }

    public override async Task Skip_navigation_of_type_unidirectional(bool async)
    {
        await base.Skip_navigation_of_type_unidirectional(async);

        AssertSql(
            @"SELECT [u].[Key1], [u].[Key2], [u].[Key3], [t0].[Id], [t0].[Name], [t0].[Number], [t0].[IsGreen], [t0].[Discriminator], [t0].[RootSkipSharedId], [t0].[UnidirectionalEntityCompositeKeyKey1], [t0].[UnidirectionalEntityCompositeKeyKey2], [t0].[UnidirectionalEntityCompositeKeyKey3]
FROM [UnidirectionalEntityCompositeKeys] AS [u]
LEFT JOIN (
    SELECT [t].[Id], [t].[Name], [t].[Number], [t].[IsGreen], [t].[Discriminator], [u0].[RootSkipSharedId], [u0].[UnidirectionalEntityCompositeKeyKey1], [u0].[UnidirectionalEntityCompositeKeyKey2], [u0].[UnidirectionalEntityCompositeKeyKey3]
    FROM [UnidirectionalEntityCompositeKeyUnidirectionalEntityRoot] AS [u0]
    INNER JOIN (
        SELECT [u1].[Id], [u1].[Name], [u2].[Number], [u3].[IsGreen], CASE
            WHEN [u3].[Id] IS NOT NULL THEN N'UnidirectionalEntityLeaf'
            WHEN [u2].[Id] IS NOT NULL THEN N'UnidirectionalEntityBranch'
        END AS [Discriminator]
        FROM [UnidirectionalRoots] AS [u1]
        LEFT JOIN [UnidirectionalBranches] AS [u2] ON [u1].[Id] = [u2].[Id]
        LEFT JOIN [UnidirectionalLeaves] AS [u3] ON [u1].[Id] = [u3].[Id]
    ) AS [t] ON [u0].[RootSkipSharedId] = [t].[Id]
    WHERE [t].[Discriminator] = N'UnidirectionalEntityLeaf'
) AS [t0] ON [u].[Key1] = [t0].[UnidirectionalEntityCompositeKeyKey1] AND [u].[Key2] = [t0].[UnidirectionalEntityCompositeKeyKey2] AND [u].[Key3] = [t0].[UnidirectionalEntityCompositeKeyKey3]
ORDER BY [u].[Key1], [u].[Key2], [u].[Key3], [t0].[RootSkipSharedId], [t0].[UnidirectionalEntityCompositeKeyKey1], [t0].[UnidirectionalEntityCompositeKeyKey2], [t0].[UnidirectionalEntityCompositeKeyKey3]");
    }

    public override async Task Join_with_skip_navigation_unidirectional(bool async)
    {
        await base.Join_with_skip_navigation_unidirectional(async);

        AssertSql(
            @"SELECT [u].[Id], [u].[CollectionInverseId], [u].[ExtraId], [u].[Name], [u].[ReferenceInverseId], [u0].[Id], [u0].[CollectionInverseId], [u0].[ExtraId], [u0].[Name], [u0].[ReferenceInverseId]
FROM [UnidirectionalEntityTwos] AS [u]
INNER JOIN [UnidirectionalEntityTwos] AS [u0] ON [u].[Id] = (
    SELECT TOP(1) [u2].[Id]
    FROM [UnidirectionalEntityTwoUnidirectionalEntityTwo] AS [u1]
    INNER JOIN [UnidirectionalEntityTwos] AS [u2] ON [u1].[SelfSkipSharedRightId] = [u2].[Id]
    WHERE [u0].[Id] = [u1].[UnidirectionalEntityTwoId]
    ORDER BY [u2].[Id])");
    }

    public override async Task Left_join_with_skip_navigation_unidirectional(bool async)
    {
        await base.Left_join_with_skip_navigation_unidirectional(async);

        AssertSql(
            @"SELECT [u].[Key1], [u].[Key2], [u].[Key3], [u].[Name], [u0].[Key1], [u0].[Key2], [u0].[Key3], [u0].[Name]
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
ORDER BY [u].[Key1], [u0].[Key1], [u].[Key2], [u0].[Key2]");
    }

    public override async Task Select_many_over_skip_navigation_unidirectional(bool async)
    {
        await base.Select_many_over_skip_navigation_unidirectional(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[CollectionInverseId], [t].[Name], [t].[ReferenceInverseId]
FROM [UnidirectionalRoots] AS [u]
INNER JOIN (
    SELECT [u3].[Id], [u3].[CollectionInverseId], [u3].[Name], [u3].[ReferenceInverseId], [u2].[UnidirectionalEntityRootId]
    FROM [UnidirectionalEntityRootUnidirectionalEntityThree] AS [u2]
    INNER JOIN [UnidirectionalEntityThrees] AS [u3] ON [u2].[ThreeSkipSharedId] = [u3].[Id]
) AS [t] ON [u].[Id] = [t].[UnidirectionalEntityRootId]");
    }

    public override async Task Select_many_over_skip_navigation_where_unidirectional(bool async)
    {
        await base.Select_many_over_skip_navigation_where_unidirectional(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId]
FROM [UnidirectionalEntityOnes] AS [u]
LEFT JOIN (
    SELECT [u1].[Id], [u1].[CollectionInverseId], [u1].[ExtraId], [u1].[Name], [u1].[ReferenceInverseId], [u0].[OneId]
    FROM [UnidirectionalJoinOneToTwo] AS [u0]
    INNER JOIN [UnidirectionalEntityTwos] AS [u1] ON [u0].[TwoId] = [u1].[Id]
) AS [t] ON [u].[Id] = [t].[OneId]");
    }

    public override async Task Select_many_over_skip_navigation_order_by_take_unidirectional(bool async)
    {
        await base.Select_many_over_skip_navigation_order_by_take_unidirectional(async);

        AssertSql(
            @"SELECT [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId]
FROM [UnidirectionalEntityOnes] AS [u]
INNER JOIN (
    SELECT [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId], [t].[UnidirectionalEntityOneId]
    FROM (
        SELECT [u1].[Id], [u1].[CollectionInverseId], [u1].[ExtraId], [u1].[Name], [u1].[ReferenceInverseId], [u0].[UnidirectionalEntityOneId], ROW_NUMBER() OVER(PARTITION BY [u0].[UnidirectionalEntityOneId] ORDER BY [u1].[Id]) AS [row]
        FROM [UnidirectionalEntityOneUnidirectionalEntityTwo] AS [u0]
        INNER JOIN [UnidirectionalEntityTwos] AS [u1] ON [u0].[TwoSkipSharedId] = [u1].[Id]
    ) AS [t]
    WHERE [t].[row] <= 2
) AS [t0] ON [u].[Id] = [t0].[UnidirectionalEntityOneId]");
    }

    public override async Task Select_many_over_skip_navigation_order_by_skip_take_unidirectional(bool async)
    {
        await base.Select_many_over_skip_navigation_order_by_skip_take_unidirectional(async);

        AssertSql(
            @"SELECT [t0].[Id], [t0].[CollectionInverseId], [t0].[Name], [t0].[ReferenceInverseId]
FROM [UnidirectionalEntityOnes] AS [u]
INNER JOIN (
    SELECT [t].[Id], [t].[CollectionInverseId], [t].[Name], [t].[ReferenceInverseId], [t].[OneId]
    FROM (
        SELECT [u1].[Id], [u1].[CollectionInverseId], [u1].[Name], [u1].[ReferenceInverseId], [u0].[OneId], ROW_NUMBER() OVER(PARTITION BY [u0].[OneId] ORDER BY [u1].[Id]) AS [row]
        FROM [UnidirectionalJoinOneToThreePayloadFullShared] AS [u0]
        INNER JOIN [UnidirectionalEntityThrees] AS [u1] ON [u0].[ThreeId] = [u1].[Id]
    ) AS [t]
    WHERE 2 < [t].[row] AND [t].[row] <= 5
) AS [t0] ON [u].[Id] = [t0].[OneId]");
    }

    public override async Task Select_many_over_skip_navigation_cast_unidirectional(bool async)
    {
        await base.Select_many_over_skip_navigation_cast_unidirectional(async);

        AssertSql(
            @"SELECT [t0].[Id], [t0].[Name], [t0].[Number], [t0].[IsGreen], [t0].[Discriminator]
FROM [UnidirectionalEntityOnes] AS [u]
INNER JOIN (
    SELECT [t].[Id], [t].[Name], [t].[Number], [t].[IsGreen], [t].[Discriminator], [u0].[UnidirectionalEntityOneId]
    FROM [UnidirectionalJoinOneToBranch] AS [u0]
    INNER JOIN (
        SELECT [u1].[Id], [u1].[Name], [u2].[Number], [u3].[IsGreen], CASE
            WHEN [u3].[Id] IS NOT NULL THEN N'UnidirectionalEntityLeaf'
        END AS [Discriminator]
        FROM [UnidirectionalRoots] AS [u1]
        INNER JOIN [UnidirectionalBranches] AS [u2] ON [u1].[Id] = [u2].[Id]
        LEFT JOIN [UnidirectionalLeaves] AS [u3] ON [u1].[Id] = [u3].[Id]
    ) AS [t] ON [u0].[UnidirectionalEntityBranchId] = [t].[Id]
) AS [t0] ON [u].[Id] = [t0].[UnidirectionalEntityOneId]");
    }

    public override async Task Select_skip_navigation_unidirectional(bool async)
    {
        await base.Select_skip_navigation_unidirectional(async);

        AssertSql(
            @"SELECT [u].[Id], [t].[Id], [t].[Name], [t].[LeftId], [t].[RightId]
FROM [UnidirectionalEntityOnes] AS [u]
LEFT JOIN (
    SELECT [u1].[Id], [u1].[Name], [u0].[LeftId], [u0].[RightId]
    FROM [UnidirectionalJoinOneSelfPayload] AS [u0]
    INNER JOIN [UnidirectionalEntityOnes] AS [u1] ON [u0].[LeftId] = [u1].[Id]
) AS [t] ON [u].[Id] = [t].[RightId]
ORDER BY [u].[Id], [t].[LeftId], [t].[RightId]");
    }

    public override async Task Include_skip_navigation_unidirectional(bool async)
    {
        await base.Include_skip_navigation_unidirectional(async);

        AssertSql(
            @"SELECT [u].[Key1], [u].[Key2], [u].[Key3], [u].[Name], [t0].[RootSkipSharedId], [t0].[UnidirectionalEntityCompositeKeyKey1], [t0].[UnidirectionalEntityCompositeKeyKey2], [t0].[UnidirectionalEntityCompositeKeyKey3], [t0].[Id], [t0].[Name], [t0].[Number], [t0].[IsGreen], [t0].[Discriminator]
FROM [UnidirectionalEntityCompositeKeys] AS [u]
LEFT JOIN (
    SELECT [u0].[RootSkipSharedId], [u0].[UnidirectionalEntityCompositeKeyKey1], [u0].[UnidirectionalEntityCompositeKeyKey2], [u0].[UnidirectionalEntityCompositeKeyKey3], [t].[Id], [t].[Name], [t].[Number], [t].[IsGreen], [t].[Discriminator]
    FROM [UnidirectionalEntityCompositeKeyUnidirectionalEntityRoot] AS [u0]
    INNER JOIN (
        SELECT [u1].[Id], [u1].[Name], [u2].[Number], [u3].[IsGreen], CASE
            WHEN [u3].[Id] IS NOT NULL THEN N'UnidirectionalEntityLeaf'
            WHEN [u2].[Id] IS NOT NULL THEN N'UnidirectionalEntityBranch'
        END AS [Discriminator]
        FROM [UnidirectionalRoots] AS [u1]
        LEFT JOIN [UnidirectionalBranches] AS [u2] ON [u1].[Id] = [u2].[Id]
        LEFT JOIN [UnidirectionalLeaves] AS [u3] ON [u1].[Id] = [u3].[Id]
    ) AS [t] ON [u0].[RootSkipSharedId] = [t].[Id]
) AS [t0] ON [u].[Key1] = [t0].[UnidirectionalEntityCompositeKeyKey1] AND [u].[Key2] = [t0].[UnidirectionalEntityCompositeKeyKey2] AND [u].[Key3] = [t0].[UnidirectionalEntityCompositeKeyKey3]
ORDER BY [u].[Key1], [u].[Key2], [u].[Key3], [t0].[RootSkipSharedId], [t0].[UnidirectionalEntityCompositeKeyKey1], [t0].[UnidirectionalEntityCompositeKeyKey2], [t0].[UnidirectionalEntityCompositeKeyKey3]");
    }

    public override async Task Include_skip_navigation_then_reference_unidirectional(bool async)
    {
        await base.Include_skip_navigation_then_reference_unidirectional(async);

        AssertSql(
            @"SELECT [u].[Id], [u].[CollectionInverseId], [u].[ExtraId], [u].[Name], [u].[ReferenceInverseId], [t].[OneId], [t].[TwoId], [t].[UnidirectionalJoinOneToTwoExtraId], [t].[Id], [t].[Name], [t].[Id0], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name0], [t].[ReferenceInverseId]
FROM [UnidirectionalEntityTwos] AS [u]
LEFT JOIN (
    SELECT [u0].[OneId], [u0].[TwoId], [u0].[UnidirectionalJoinOneToTwoExtraId], [u1].[Id], [u1].[Name], [u2].[Id] AS [Id0], [u2].[CollectionInverseId], [u2].[ExtraId], [u2].[Name] AS [Name0], [u2].[ReferenceInverseId]
    FROM [UnidirectionalJoinOneToTwo] AS [u0]
    INNER JOIN [UnidirectionalEntityOnes] AS [u1] ON [u0].[OneId] = [u1].[Id]
    LEFT JOIN [UnidirectionalEntityTwos] AS [u2] ON [u1].[Id] = [u2].[ReferenceInverseId]
) AS [t] ON [u].[Id] = [t].[TwoId]
ORDER BY [u].[Id], [t].[OneId], [t].[TwoId], [t].[Id]");
    }

    public override async Task Include_skip_navigation_then_include_skip_navigation_unidirectional(bool async)
    {
        await base.Include_skip_navigation_then_include_skip_navigation_unidirectional(async);

        AssertSql(
            @"SELECT [u].[Key1], [u].[Key2], [u].[Key3], [u].[Name], [t1].[LeafId], [t1].[CompositeId1], [t1].[CompositeId2], [t1].[CompositeId3], [t1].[Id], [t1].[Name], [t1].[Number], [t1].[IsGreen], [t1].[UnidirectionalEntityBranchId], [t1].[UnidirectionalEntityOneId], [t1].[Id0], [t1].[Name0]
FROM [UnidirectionalEntityCompositeKeys] AS [u]
LEFT JOIN (
    SELECT [u0].[LeafId], [u0].[CompositeId1], [u0].[CompositeId2], [u0].[CompositeId3], [t].[Id], [t].[Name], [t].[Number], [t].[IsGreen], [t0].[UnidirectionalEntityBranchId], [t0].[UnidirectionalEntityOneId], [t0].[Id] AS [Id0], [t0].[Name] AS [Name0]
    FROM [UnidirectionalJoinCompositeKeyToLeaf] AS [u0]
    INNER JOIN (
        SELECT [u1].[Id], [u1].[Name], [u2].[Number], [u3].[IsGreen]
        FROM [UnidirectionalRoots] AS [u1]
        INNER JOIN [UnidirectionalBranches] AS [u2] ON [u1].[Id] = [u2].[Id]
        INNER JOIN [UnidirectionalLeaves] AS [u3] ON [u1].[Id] = [u3].[Id]
    ) AS [t] ON [u0].[LeafId] = [t].[Id]
    LEFT JOIN (
        SELECT [u4].[UnidirectionalEntityBranchId], [u4].[UnidirectionalEntityOneId], [u5].[Id], [u5].[Name]
        FROM [UnidirectionalJoinOneToBranch] AS [u4]
        INNER JOIN [UnidirectionalEntityOnes] AS [u5] ON [u4].[UnidirectionalEntityOneId] = [u5].[Id]
    ) AS [t0] ON [t].[Id] = [t0].[UnidirectionalEntityBranchId]
) AS [t1] ON [u].[Key1] = [t1].[CompositeId1] AND [u].[Key2] = [t1].[CompositeId2] AND [u].[Key3] = [t1].[CompositeId3]
ORDER BY [u].[Key1], [u].[Key2], [u].[Key3], [t1].[LeafId], [t1].[CompositeId1], [t1].[CompositeId2], [t1].[CompositeId3], [t1].[Id], [t1].[UnidirectionalEntityBranchId], [t1].[UnidirectionalEntityOneId]");
    }

    public override async Task Include_skip_navigation_then_include_reference_and_skip_navigation_unidirectional(bool async)
    {
        await base.Include_skip_navigation_then_include_reference_and_skip_navigation_unidirectional(async);

        AssertSql(
            @"SELECT [u].[Id], [u].[CollectionInverseId], [u].[Name], [u].[ReferenceInverseId], [t0].[OneId], [t0].[ThreeId], [t0].[Payload], [t0].[Id], [t0].[Name], [t0].[Id0], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name0], [t0].[ReferenceInverseId], [t0].[LeftId], [t0].[RightId], [t0].[Payload0], [t0].[Id1], [t0].[Name1]
FROM [UnidirectionalEntityThrees] AS [u]
LEFT JOIN (
    SELECT [u0].[OneId], [u0].[ThreeId], [u0].[Payload], [u1].[Id], [u1].[Name], [u2].[Id] AS [Id0], [u2].[CollectionInverseId], [u2].[ExtraId], [u2].[Name] AS [Name0], [u2].[ReferenceInverseId], [t].[LeftId], [t].[RightId], [t].[Payload] AS [Payload0], [t].[Id] AS [Id1], [t].[Name] AS [Name1]
    FROM [UnidirectionalJoinOneToThreePayloadFull] AS [u0]
    INNER JOIN [UnidirectionalEntityOnes] AS [u1] ON [u0].[OneId] = [u1].[Id]
    LEFT JOIN [UnidirectionalEntityTwos] AS [u2] ON [u1].[Id] = [u2].[ReferenceInverseId]
    LEFT JOIN (
        SELECT [u3].[LeftId], [u3].[RightId], [u3].[Payload], [u4].[Id], [u4].[Name]
        FROM [UnidirectionalJoinOneSelfPayload] AS [u3]
        INNER JOIN [UnidirectionalEntityOnes] AS [u4] ON [u3].[RightId] = [u4].[Id]
    ) AS [t] ON [u1].[Id] = [t].[LeftId]
) AS [t0] ON [u].[Id] = [t0].[ThreeId]
ORDER BY [u].[Id], [t0].[OneId], [t0].[ThreeId], [t0].[Id], [t0].[Id0], [t0].[LeftId], [t0].[RightId]");
    }

    public override async Task Include_skip_navigation_and_reference_unidirectional(bool async)
    {
        await base.Include_skip_navigation_and_reference_unidirectional(async);

        AssertSql(
            @"SELECT [u].[Id], [u].[CollectionInverseId], [u].[ExtraId], [u].[Name], [u].[ReferenceInverseId], [u0].[Id], [t].[TwoSkipSharedId], [t].[UnidirectionalEntityOneId], [t].[Id], [t].[Name], [u0].[CollectionInverseId], [u0].[Name], [u0].[ReferenceInverseId]
FROM [UnidirectionalEntityTwos] AS [u]
LEFT JOIN [UnidirectionalEntityThrees] AS [u0] ON [u].[Id] = [u0].[ReferenceInverseId]
LEFT JOIN (
    SELECT [u1].[TwoSkipSharedId], [u1].[UnidirectionalEntityOneId], [u2].[Id], [u2].[Name]
    FROM [UnidirectionalEntityOneUnidirectionalEntityTwo] AS [u1]
    INNER JOIN [UnidirectionalEntityOnes] AS [u2] ON [u1].[UnidirectionalEntityOneId] = [u2].[Id]
) AS [t] ON [u].[Id] = [t].[TwoSkipSharedId]
ORDER BY [u].[Id], [u0].[Id], [t].[TwoSkipSharedId], [t].[UnidirectionalEntityOneId]");
    }

    public override async Task Include_skip_navigation_then_include_inverse_works_for_tracking_query_unidirectional(bool async)
    {
        await base.Include_skip_navigation_then_include_inverse_works_for_tracking_query_unidirectional(async);

        AssertSql(
            @"SELECT [u].[Id], [u].[CollectionInverseId], [u].[Name], [u].[ReferenceInverseId], [t0].[OneId], [t0].[ThreeId], [t0].[Payload], [t0].[Id], [t0].[Name], [t0].[OneId0], [t0].[ThreeId0], [t0].[Payload0], [t0].[Id0], [t0].[CollectionInverseId], [t0].[Name0], [t0].[ReferenceInverseId]
FROM [UnidirectionalEntityThrees] AS [u]
LEFT JOIN (
    SELECT [u0].[OneId], [u0].[ThreeId], [u0].[Payload], [u1].[Id], [u1].[Name], [t].[OneId] AS [OneId0], [t].[ThreeId] AS [ThreeId0], [t].[Payload] AS [Payload0], [t].[Id] AS [Id0], [t].[CollectionInverseId], [t].[Name] AS [Name0], [t].[ReferenceInverseId]
    FROM [UnidirectionalJoinOneToThreePayloadFullShared] AS [u0]
    INNER JOIN [UnidirectionalEntityOnes] AS [u1] ON [u0].[OneId] = [u1].[Id]
    LEFT JOIN (
        SELECT [u2].[OneId], [u2].[ThreeId], [u2].[Payload], [u3].[Id], [u3].[CollectionInverseId], [u3].[Name], [u3].[ReferenceInverseId]
        FROM [UnidirectionalJoinOneToThreePayloadFullShared] AS [u2]
        INNER JOIN [UnidirectionalEntityThrees] AS [u3] ON [u2].[ThreeId] = [u3].[Id]
    ) AS [t] ON [u1].[Id] = [t].[OneId]
) AS [t0] ON [u].[Id] = [t0].[ThreeId]
ORDER BY [u].[Id], [t0].[OneId], [t0].[ThreeId], [t0].[Id], [t0].[OneId0], [t0].[ThreeId0]");
    }

    public override async Task Filtered_include_skip_navigation_where_unidirectional(bool async)
    {
        await base.Filtered_include_skip_navigation_where_unidirectional(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[ReferenceInverseId], [t].[OneId], [t].[ThreeId], [t].[Payload], [t].[Id], [t].[Name]
FROM [EntityThrees] AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [e0].[Id], [e0].[Name]
    FROM [JoinOneToThreePayloadFullShared] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [t] ON [e].[Id] = [t].[ThreeId]
ORDER BY [e].[Id], [t].[OneId], [t].[ThreeId]");
    }

    public override async Task Filtered_include_skip_navigation_order_by_unidirectional(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_unidirectional(async);

        AssertSql(
            @"SELECT [u].[Id], [u].[CollectionInverseId], [u].[Name], [u].[ReferenceInverseId], [t].[ThreeId], [t].[TwoId], [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId]
FROM [UnidirectionalEntityThrees] AS [u]
LEFT JOIN (
    SELECT [u0].[ThreeId], [u0].[TwoId], [u1].[Id], [u1].[CollectionInverseId], [u1].[ExtraId], [u1].[Name], [u1].[ReferenceInverseId]
    FROM [UnidirectionalJoinTwoToThree] AS [u0]
    INNER JOIN [UnidirectionalEntityTwos] AS [u1] ON [u0].[TwoId] = [u1].[Id]
) AS [t] ON [u].[Id] = [t].[ThreeId]
ORDER BY [u].[Id], [t].[Id], [t].[ThreeId]");
    }

    public override async Task Filtered_include_skip_navigation_order_by_skip_unidirectional(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip_unidirectional(async);

        AssertSql(
            @"SELECT [u].[Id], [u].[CollectionInverseId], [u].[ExtraId], [u].[Name], [u].[ReferenceInverseId], [t0].[SelfSkipSharedRightId], [t0].[UnidirectionalEntityTwoId], [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId]
FROM [UnidirectionalEntityTwos] AS [u]
LEFT JOIN (
    SELECT [t].[SelfSkipSharedRightId], [t].[UnidirectionalEntityTwoId], [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId]
    FROM (
        SELECT [u0].[SelfSkipSharedRightId], [u0].[UnidirectionalEntityTwoId], [u1].[Id], [u1].[CollectionInverseId], [u1].[ExtraId], [u1].[Name], [u1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [u0].[UnidirectionalEntityTwoId] ORDER BY [u1].[Id]) AS [row]
        FROM [UnidirectionalEntityTwoUnidirectionalEntityTwo] AS [u0]
        INNER JOIN [UnidirectionalEntityTwos] AS [u1] ON [u0].[SelfSkipSharedRightId] = [u1].[Id]
    ) AS [t]
    WHERE 2 < [t].[row]
) AS [t0] ON [u].[Id] = [t0].[UnidirectionalEntityTwoId]
ORDER BY [u].[Id], [t0].[UnidirectionalEntityTwoId], [t0].[Id]");
    }

    public override async Task Filtered_include_skip_navigation_order_by_take_unidirectional(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_take_unidirectional(async);

        AssertSql(
            @"SELECT [u].[Key1], [u].[Key2], [u].[Key3], [u].[Name], [t0].[TwoSkipSharedId], [t0].[UnidirectionalEntityCompositeKeyKey1], [t0].[UnidirectionalEntityCompositeKeyKey2], [t0].[UnidirectionalEntityCompositeKeyKey3], [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[ReferenceInverseId]
FROM [UnidirectionalEntityCompositeKeys] AS [u]
LEFT JOIN (
    SELECT [t].[TwoSkipSharedId], [t].[UnidirectionalEntityCompositeKeyKey1], [t].[UnidirectionalEntityCompositeKeyKey2], [t].[UnidirectionalEntityCompositeKeyKey3], [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId]
    FROM (
        SELECT [u0].[TwoSkipSharedId], [u0].[UnidirectionalEntityCompositeKeyKey1], [u0].[UnidirectionalEntityCompositeKeyKey2], [u0].[UnidirectionalEntityCompositeKeyKey3], [u1].[Id], [u1].[CollectionInverseId], [u1].[ExtraId], [u1].[Name], [u1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [u0].[UnidirectionalEntityCompositeKeyKey1], [u0].[UnidirectionalEntityCompositeKeyKey2], [u0].[UnidirectionalEntityCompositeKeyKey3] ORDER BY [u1].[Id]) AS [row]
        FROM [UnidirectionalEntityCompositeKeyUnidirectionalEntityTwo] AS [u0]
        INNER JOIN [UnidirectionalEntityTwos] AS [u1] ON [u0].[TwoSkipSharedId] = [u1].[Id]
    ) AS [t]
    WHERE [t].[row] <= 2
) AS [t0] ON [u].[Key1] = [t0].[UnidirectionalEntityCompositeKeyKey1] AND [u].[Key2] = [t0].[UnidirectionalEntityCompositeKeyKey2] AND [u].[Key3] = [t0].[UnidirectionalEntityCompositeKeyKey3]
ORDER BY [u].[Key1], [u].[Key2], [u].[Key3], [t0].[UnidirectionalEntityCompositeKeyKey1], [t0].[UnidirectionalEntityCompositeKeyKey2], [t0].[UnidirectionalEntityCompositeKeyKey3], [t0].[Id]");
    }

    public override async Task Filtered_include_skip_navigation_order_by_skip_take_unidirectional(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip_take_unidirectional(async);

        AssertSql(
            @"SELECT [u].[Key1], [u].[Key2], [u].[Key3], [u].[Name], [t0].[Id], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[ThreeId], [t0].[Id0], [t0].[CollectionInverseId], [t0].[Name], [t0].[ReferenceInverseId]
FROM [UnidirectionalEntityCompositeKeys] AS [u]
LEFT JOIN (
    SELECT [t].[Id], [t].[CompositeId1], [t].[CompositeId2], [t].[CompositeId3], [t].[ThreeId], [t].[Id0], [t].[CollectionInverseId], [t].[Name], [t].[ReferenceInverseId]
    FROM (
        SELECT [u0].[Id], [u0].[CompositeId1], [u0].[CompositeId2], [u0].[CompositeId3], [u0].[ThreeId], [u1].[Id] AS [Id0], [u1].[CollectionInverseId], [u1].[Name], [u1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [u0].[CompositeId1], [u0].[CompositeId2], [u0].[CompositeId3] ORDER BY [u1].[Id]) AS [row]
        FROM [UnidirectionalJoinThreeToCompositeKeyFull] AS [u0]
        INNER JOIN [UnidirectionalEntityThrees] AS [u1] ON [u0].[ThreeId] = [u1].[Id]
    ) AS [t]
    WHERE 1 < [t].[row] AND [t].[row] <= 3
) AS [t0] ON [u].[Key1] = [t0].[CompositeId1] AND [u].[Key2] = [t0].[CompositeId2] AND [u].[Key3] = [t0].[CompositeId3]
ORDER BY [u].[Key1], [u].[Key2], [u].[Key3], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[Id0]");
    }

    public override async Task Filtered_include_skip_navigation_where_then_include_skip_navigation_unidirectional(bool async)
    {
        await base.Filtered_include_skip_navigation_where_then_include_skip_navigation_unidirectional(async);

        AssertSql(
            @"SELECT [u].[Id], [u].[Name], [u0].[Number], [u1].[IsGreen], [t0].[LeafId], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[Key1], [t0].[Key2], [t0].[Key3], [t0].[Name], [t0].[TwoSkipSharedId], [t0].[UnidirectionalEntityCompositeKeyKey1], [t0].[UnidirectionalEntityCompositeKeyKey2], [t0].[UnidirectionalEntityCompositeKeyKey3], [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name0], [t0].[ReferenceInverseId]
FROM [UnidirectionalRoots] AS [u]
INNER JOIN [UnidirectionalBranches] AS [u0] ON [u].[Id] = [u0].[Id]
INNER JOIN [UnidirectionalLeaves] AS [u1] ON [u].[Id] = [u1].[Id]
LEFT JOIN (
    SELECT [u2].[LeafId], [u2].[CompositeId1], [u2].[CompositeId2], [u2].[CompositeId3], [u3].[Key1], [u3].[Key2], [u3].[Key3], [u3].[Name], [t].[TwoSkipSharedId], [t].[UnidirectionalEntityCompositeKeyKey1], [t].[UnidirectionalEntityCompositeKeyKey2], [t].[UnidirectionalEntityCompositeKeyKey3], [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name] AS [Name0], [t].[ReferenceInverseId]
    FROM [UnidirectionalJoinCompositeKeyToLeaf] AS [u2]
    INNER JOIN [UnidirectionalEntityCompositeKeys] AS [u3] ON [u2].[CompositeId1] = [u3].[Key1] AND [u2].[CompositeId2] = [u3].[Key2] AND [u2].[CompositeId3] = [u3].[Key3]
    LEFT JOIN (
        SELECT [u4].[TwoSkipSharedId], [u4].[UnidirectionalEntityCompositeKeyKey1], [u4].[UnidirectionalEntityCompositeKeyKey2], [u4].[UnidirectionalEntityCompositeKeyKey3], [u5].[Id], [u5].[CollectionInverseId], [u5].[ExtraId], [u5].[Name], [u5].[ReferenceInverseId]
        FROM [UnidirectionalEntityCompositeKeyUnidirectionalEntityTwo] AS [u4]
        INNER JOIN [UnidirectionalEntityTwos] AS [u5] ON [u4].[TwoSkipSharedId] = [u5].[Id]
    ) AS [t] ON [u3].[Key1] = [t].[UnidirectionalEntityCompositeKeyKey1] AND [u3].[Key2] = [t].[UnidirectionalEntityCompositeKeyKey2] AND [u3].[Key3] = [t].[UnidirectionalEntityCompositeKeyKey3]
    WHERE [u3].[Key1] < 5
) AS [t0] ON [u].[Id] = [t0].[LeafId]
ORDER BY [u].[Id], [t0].[LeafId], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[Key1], [t0].[Key2], [t0].[Key3], [t0].[TwoSkipSharedId], [t0].[UnidirectionalEntityCompositeKeyKey1], [t0].[UnidirectionalEntityCompositeKeyKey2], [t0].[UnidirectionalEntityCompositeKeyKey3]");
    }

    public override async Task Filter_include_on_skip_navigation_combined_unidirectional(bool async)
    {
        await base.Filter_include_on_skip_navigation_combined_unidirectional(async);

        AssertSql(
            @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[ReferenceInverseId], [t].[OneId], [t].[TwoId], [t].[JoinOneToTwoExtraId], [t].[Id], [t].[Name], [t].[Id0], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name0], [t].[ReferenceInverseId], [t].[Id1], [t].[CollectionInverseId0], [t].[ExtraId0], [t].[Name1], [t].[ReferenceInverseId0]
FROM [EntityTwos] AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [e0].[Id], [e0].[Name], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[ReferenceInverseId], [e2].[Id] AS [Id1], [e2].[CollectionInverseId] AS [CollectionInverseId0], [e2].[ExtraId] AS [ExtraId0], [e2].[Name] AS [Name1], [e2].[ReferenceInverseId] AS [ReferenceInverseId0]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityOnes] AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
    LEFT JOIN [EntityTwos] AS [e2] ON [e0].[Id] = [e2].[CollectionInverseId]
    WHERE [e0].[Id] < 10
) AS [t] ON [e].[Id] = [t].[TwoId]
ORDER BY [e].[Id], [t].[OneId], [t].[TwoId], [t].[Id], [t].[Id0]");
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
            @"SELECT [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[ReferenceInverseId]
FROM [UnidirectionalEntityOnes] AS [u]
LEFT JOIN (
    SELECT [u1].[Id], [u1].[CollectionInverseId], [u1].[ExtraId], [u1].[Name], [u1].[ReferenceInverseId], [u0].[OneId]
    FROM [UnidirectionalJoinOneToTwo] AS [u0]
    INNER JOIN [UnidirectionalEntityTwos] AS [u1] ON [u0].[TwoId] = [u1].[Id]
) AS [t] ON [u].[Id] = [t].[OneId] AND [u].[Id] <> [t].[Id]");
    }

    public override async Task Contains_on_skip_collection_navigation_unidirectional(bool async)
    {
        await base.Contains_on_skip_collection_navigation_unidirectional(async);

        AssertSql(
            @"@__entity_equality_two_0_Id='1' (Nullable = true)

SELECT [u].[Id], [u].[Name]
FROM [UnidirectionalEntityOnes] AS [u]
WHERE EXISTS (
    SELECT 1
    FROM [UnidirectionalJoinOneToTwo] AS [u0]
    INNER JOIN [UnidirectionalEntityTwos] AS [u1] ON [u0].[TwoId] = [u1].[Id]
    WHERE [u].[Id] = [u0].[OneId] AND [u1].[Id] = @__entity_equality_two_0_Id)");
    }

    public override async Task GetType_in_hierarchy_in_base_type_unidirectional(bool async)
    {
        await base.GetType_in_hierarchy_in_base_type_unidirectional(async);

        AssertSql(
            @"SELECT [u].[Id], [u].[Name], [u0].[Number], [u1].[IsGreen], CASE
    WHEN [u1].[Id] IS NOT NULL THEN N'UnidirectionalEntityLeaf'
    WHEN [u0].[Id] IS NOT NULL THEN N'UnidirectionalEntityBranch'
END AS [Discriminator]
FROM [UnidirectionalRoots] AS [u]
LEFT JOIN [UnidirectionalBranches] AS [u0] ON [u].[Id] = [u0].[Id]
LEFT JOIN [UnidirectionalLeaves] AS [u1] ON [u].[Id] = [u1].[Id]
WHERE ([u1].[Id] IS NULL) AND ([u0].[Id] IS NULL)");
    }

    public override async Task GetType_in_hierarchy_in_intermediate_type_unidirectional(bool async)
    {
        await base.GetType_in_hierarchy_in_intermediate_type_unidirectional(async);

        AssertSql(
            @"SELECT [u].[Id], [u].[Name], [u0].[Number], [u1].[IsGreen], CASE
    WHEN [u1].[Id] IS NOT NULL THEN N'UnidirectionalEntityLeaf'
    WHEN [u0].[Id] IS NOT NULL THEN N'UnidirectionalEntityBranch'
END AS [Discriminator]
FROM [UnidirectionalRoots] AS [u]
LEFT JOIN [UnidirectionalBranches] AS [u0] ON [u].[Id] = [u0].[Id]
LEFT JOIN [UnidirectionalLeaves] AS [u1] ON [u].[Id] = [u1].[Id]
WHERE ([u1].[Id] IS NULL) AND ([u0].[Id] IS NOT NULL)");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_unidirectional(bool async)
    {
        await base.GetType_in_hierarchy_in_leaf_type_unidirectional(async);

        AssertSql(
            @"SELECT [u].[Id], [u].[Name], [u0].[Number], [u1].[IsGreen], CASE
    WHEN [u1].[Id] IS NOT NULL THEN N'UnidirectionalEntityLeaf'
    WHEN [u0].[Id] IS NOT NULL THEN N'UnidirectionalEntityBranch'
END AS [Discriminator]
FROM [UnidirectionalRoots] AS [u]
LEFT JOIN [UnidirectionalBranches] AS [u0] ON [u].[Id] = [u0].[Id]
LEFT JOIN [UnidirectionalLeaves] AS [u1] ON [u].[Id] = [u1].[Id]
WHERE [u1].[Id] IS NOT NULL");
    }

    public override async Task GetType_in_hierarchy_in_querying_base_type_unidirectional(bool async)
    {
        await base.GetType_in_hierarchy_in_querying_base_type_unidirectional(async);

        AssertSql(
            @"SELECT [u].[Id], [u].[Name], [u0].[Number], [u1].[IsGreen], CASE
    WHEN [u1].[Id] IS NOT NULL THEN N'UnidirectionalEntityLeaf'
END AS [Discriminator]
FROM [UnidirectionalRoots] AS [u]
INNER JOIN [UnidirectionalBranches] AS [u0] ON [u].[Id] = [u0].[Id]
LEFT JOIN [UnidirectionalLeaves] AS [u1] ON [u].[Id] = [u1].[Id]
WHERE 0 = 1");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
