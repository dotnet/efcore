// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

[SqlServerCondition(SqlServerCondition.SupportsTemporalTablesCascadeDelete)]
public class TemporalManyToManyQuerySqlServerTest : ManyToManyQueryRelationalTestBase<TemporalManyToManyQuerySqlServerFixture>
{
    public TemporalManyToManyQuerySqlServerTest(TemporalManyToManyQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
    {
        serverQueryExpression = base.RewriteServerQueryExpression(serverQueryExpression);

        var temporalEntityTypes = new List<Type>
        {
            typeof(EntityOne),
            typeof(EntityTwo),
            typeof(EntityThree),
            typeof(EntityCompositeKey),
            typeof(EntityRoot),
            typeof(EntityBranch),
            typeof(EntityLeaf),
            typeof(EntityBranch2),
            typeof(EntityLeaf2),
            typeof(EntityTableSharing1),
            typeof(EntityTableSharing2),
            typeof(UnidirectionalEntityOne),
            typeof(UnidirectionalEntityTwo),
            typeof(UnidirectionalEntityThree),
            typeof(UnidirectionalEntityCompositeKey),
            typeof(UnidirectionalEntityRoot),
            typeof(UnidirectionalEntityBranch),
            typeof(UnidirectionalEntityLeaf),
        };

        var rewriter = new TemporalPointInTimeQueryRewriter(Fixture.ChangesDate, temporalEntityTypes);

        return rewriter.Visit(serverQueryExpression);
    }

    public override async Task Skip_navigation_all(bool async)
    {
        await base.Skip_navigation_all(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
WHERE NOT EXISTS (
    SELECT 1
    FROM [JoinOneToTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[TwoId] = [e0].[Id]
    WHERE [e].[Id] = [j].[OneId] AND ([e0].[Name] NOT LIKE N'%B%' OR [e0].[Name] IS NULL))
""");
    }

    public override async Task Skip_navigation_any_without_predicate(bool async)
    {
        await base.Skip_navigation_any_without_predicate(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
WHERE EXISTS (
    SELECT 1
    FROM [JoinOneToThreePayloadFull] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[ThreeId] = [e0].[Id]
    WHERE [e].[Id] = [j].[OneId] AND [e0].[Name] LIKE N'%B%')
""");
    }

    public override async Task Skip_navigation_any_with_predicate(bool async)
    {
        await base.Skip_navigation_any_with_predicate(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
WHERE EXISTS (
    SELECT 1
    FROM [EntityOneEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
    WHERE [e].[Id] = [e0].[OneSkipSharedId] AND [e1].[Name] LIKE N'%B%')
""");
    }

    public override async Task Skip_navigation_contains(bool async)
    {
        await base.Skip_navigation_contains(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
WHERE EXISTS (
    SELECT 1
    FROM [JoinOneToThreePayloadFullShared] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[ThreeId] = [e0].[Id]
    WHERE [e].[Id] = [j].[OneId] AND [e0].[Id] = 1)
""");
    }

    public override async Task Skip_navigation_count_without_predicate(bool async)
    {
        await base.Skip_navigation_count_without_predicate(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
WHERE (
    SELECT COUNT(*)
    FROM [JoinOneSelfPayload] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[LeftId] = [e0].[Id]
    WHERE [e].[Id] = [j].[RightId]) > 0
""");
    }

    public override async Task Skip_navigation_count_with_predicate(bool async)
    {
        await base.Skip_navigation_count_with_predicate(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
ORDER BY (
    SELECT COUNT(*)
    FROM [JoinOneToBranch] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN (
        SELECT [e0].[Id], [e0].[Name]
        FROM [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
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
SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId]
FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
WHERE (
    SELECT COUNT_BIG(*)
    FROM [JoinTwoToThree] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[ThreeId] = [e0].[Id]
    WHERE [e].[Id] = [j].[TwoId]) > CAST(0 AS bigint)
""");
    }

    public override async Task Skip_navigation_long_count_with_predicate(bool async)
    {
        await base.Skip_navigation_long_count_with_predicate(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId]
FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
ORDER BY (
    SELECT COUNT_BIG(*)
    FROM [EntityTwoEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[SelfSkipSharedLeftId] = [e1].[Id]
    WHERE [e].[Id] = [e0].[SelfSkipSharedRightId] AND [e1].[Name] LIKE N'L%') DESC, [e].[Id]
""");
    }

    public override async Task Skip_navigation_select_many_average(bool async)
    {
        await base.Skip_navigation_select_many_average(async);

        AssertSql(
            """
SELECT AVG(CAST([s].[Key1] AS float))
FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
INNER JOIN (
    SELECT [e1].[Key1], [e0].[TwoSkipSharedId]
    FROM [EntityCompositeKeyEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    INNER JOIN [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[CompositeKeySkipSharedKey1] = [e1].[Key1] AND [e0].[CompositeKeySkipSharedKey2] = [e1].[Key2] AND [e0].[CompositeKeySkipSharedKey3] = [e1].[Key3]
) AS [s] ON [e].[Id] = [s].[TwoSkipSharedId]
""");
    }

    public override async Task Skip_navigation_select_many_max(bool async)
    {
        await base.Skip_navigation_select_many_max(async);

        AssertSql(
            """
SELECT MAX([s].[Key1])
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
INNER JOIN (
    SELECT [e0].[Key1], [j].[ThreeId]
    FROM [JoinThreeToCompositeKeyFull] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[CompositeId1] = [e0].[Key1] AND [j].[CompositeId2] = [e0].[Key2] AND [j].[CompositeId3] = [e0].[Key3]
) AS [s] ON [e].[Id] = [s].[ThreeId]
""");
    }

    public override async Task Skip_navigation_select_many_min(bool async)
    {
        await base.Skip_navigation_select_many_min(async);

        AssertSql(
            """
SELECT MIN([s].[Id])
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
INNER JOIN (
    SELECT [e1].[Id], [e0].[ThreeSkipSharedId]
    FROM [EntityRootEntityThree] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    INNER JOIN [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[RootSkipSharedId] = [e1].[Id]
) AS [s] ON [e].[Id] = [s].[ThreeSkipSharedId]
""");
    }

    public override async Task Skip_navigation_select_many_sum(bool async)
    {
        await base.Skip_navigation_select_many_sum(async);

        AssertSql(
            """
SELECT COALESCE(SUM([s].[Key1]), 0)
FROM [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
INNER JOIN (
    SELECT [e1].[Key1], [e0].[RootSkipSharedId]
    FROM [EntityCompositeKeyEntityRoot] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    INNER JOIN [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[CompositeKeySkipSharedKey1] = [e1].[Key1] AND [e0].[CompositeKeySkipSharedKey2] = [e1].[Key2] AND [e0].[CompositeKeySkipSharedKey3] = [e1].[Key3]
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
    FROM [JoinCompositeKeyToLeaf] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[CompositeId1] = [e0].[Key1] AND [j].[CompositeId2] = [e0].[Key2] AND [j].[CompositeId3] = [e0].[Key3]
    WHERE [e].[Id] = [j].[LeafId])
FROM [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
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
    FROM [JoinOneToTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e].[Id] = [j].[TwoId])
FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
""");
    }

    public override async Task Skip_navigation_select_subquery_min(bool async)
    {
        await base.Skip_navigation_select_subquery_min(async);

        AssertSql(
            """
SELECT (
    SELECT MIN([e0].[Id])
    FROM [JoinOneToThreePayloadFull] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e].[Id] = [j].[ThreeId])
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
""");
    }

    public override async Task Skip_navigation_select_subquery_sum(bool async)
    {
        await base.Skip_navigation_select_subquery_sum(async);

        AssertSql(
            """
SELECT (
    SELECT COALESCE(SUM([e1].[Id]), 0)
    FROM [EntityOneEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[OneSkipSharedId] = [e1].[Id]
    WHERE [e].[Id] = [e0].[TwoSkipSharedId])
FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
""");
    }

    public override async Task Skip_navigation_order_by_first_or_default(bool async)
    {
        await base.Skip_navigation_order_by_first_or_default(async);

        AssertSql(
            """
SELECT [s0].[Id], [s0].[Name], [s0].[PeriodEnd], [s0].[PeriodStart]
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [s].[Id], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart], [s].[ThreeId]
    FROM (
        SELECT [e0].[Id], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [j].[ThreeId], ROW_NUMBER() OVER(PARTITION BY [j].[ThreeId] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinOneToThreePayloadFullShared] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
        INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[OneId] = [e0].[Id]
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
SELECT [s0].[Id], [s0].[Name], [s0].[PeriodEnd], [s0].[PeriodStart]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
OUTER APPLY (
    SELECT TOP(1) [s].[Id], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart]
    FROM (
        SELECT TOP(1) [e0].[Id], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart]
        FROM [JoinOneSelfPayload] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
        INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[RightId] = [e0].[Id]
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
SELECT [s0].[Id], [s0].[Name], [s0].[PeriodEnd], [s0].[PeriodStart]
FROM [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [s].[Id], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart], [s].[EntityBranchId]
    FROM (
        SELECT [e0].[Id], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [j].[EntityBranchId], ROW_NUMBER() OVER(PARTITION BY [j].[EntityBranchId] ORDER BY [e0].[Id] DESC) AS [row]
        FROM [JoinOneToBranch] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
        INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[EntityOneId] = [e0].[Id]
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
SELECT [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[PeriodEnd], [s0].[PeriodStart], [s0].[ReferenceInverseId]
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart], [s].[ReferenceInverseId], [s].[ThreeId]
    FROM (
        SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [e0].[ReferenceInverseId], [j].[ThreeId], ROW_NUMBER() OVER(PARTITION BY [j].[ThreeId] ORDER BY [e0].[Id] DESC) AS [row]
        FROM [JoinTwoToThree] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
        INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[TwoId] = [e0].[Id]
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
SELECT [e].[Key1], [e].[Key2], [e].[Key3], [s].[Id], [s].[Discriminator], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart], [s].[Number], [s].[IsGreen], [s].[LeafId], [s].[CompositeId1], [s].[CompositeId2], [s].[CompositeId3]
FROM [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [e1].[Id], [e1].[Discriminator], [e1].[Name], [e1].[PeriodEnd], [e1].[PeriodStart], [e1].[Number], [e1].[IsGreen], [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3]
    FROM [JoinCompositeKeyToLeaf] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN (
        SELECT [e0].[Id], [e0].[Discriminator], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [e0].[Number], [e0].[IsGreen]
        FROM [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
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
SELECT [e].[Key1], [e].[Key2], [e].[Key3], [s].[Id], [s].[Discriminator], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart], [s].[Number], [s].[IsGreen], [s].[RootSkipSharedId], [s].[CompositeKeySkipSharedKey1], [s].[CompositeKeySkipSharedKey2], [s].[CompositeKeySkipSharedKey3]
FROM [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [e1].[Id], [e1].[Discriminator], [e1].[Name], [e1].[PeriodEnd], [e1].[PeriodStart], [e1].[Number], [e1].[IsGreen], [e0].[RootSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3]
    FROM [EntityCompositeKeyEntityRoot] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    INNER JOIN [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[RootSkipSharedId] = [e1].[Id]
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
SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId], [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [e0].[ReferenceInverseId]
FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [e].[Id] = (
    SELECT TOP(1) [e2].[Id]
    FROM [EntityTwoEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1]
    INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e2] ON [e1].[SelfSkipSharedRightId] = [e2].[Id]
    WHERE [e0].[Id] = [e1].[SelfSkipSharedLeftId]
    ORDER BY [e2].[Id])
""");
    }

    public override async Task Left_join_with_skip_navigation(bool async)
    {
        await base.Left_join_with_skip_navigation(async);

        AssertSql(
            """
SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e0].[Key1], [e0].[Key2], [e0].[Key3], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart]
FROM [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON (
    SELECT TOP(1) [e2].[Id]
    FROM [EntityCompositeKeyEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1]
    INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e2] ON [e1].[TwoSkipSharedId] = [e2].[Id]
    WHERE [e].[Key1] = [e1].[CompositeKeySkipSharedKey1] AND [e].[Key2] = [e1].[CompositeKeySkipSharedKey2] AND [e].[Key3] = [e1].[CompositeKeySkipSharedKey3]
    ORDER BY [e2].[Id]) = (
    SELECT TOP(1) [e3].[Id]
    FROM [JoinThreeToCompositeKeyFull] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e3] ON [j].[ThreeId] = [e3].[Id]
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
SELECT [s].[Id], [s].[CollectionInverseId], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart], [s].[ReferenceInverseId]
FROM [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
INNER JOIN (
    SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[PeriodEnd], [e1].[PeriodStart], [e1].[ReferenceInverseId], [e0].[RootSkipSharedId]
    FROM [EntityRootEntityThree] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[ThreeSkipSharedId] = [e1].[Id]
) AS [s] ON [e].[Id] = [s].[RootSkipSharedId]
""");
    }

    public override async Task Select_many_over_skip_navigation_where(bool async)
    {
        await base.Select_many_over_skip_navigation_where(async);

        AssertSql(
            """
SELECT [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart], [s].[ReferenceInverseId]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [e0].[ReferenceInverseId], [j].[OneId]
    FROM [JoinOneToTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[TwoId] = [e0].[Id]
) AS [s] ON [e].[Id] = [s].[OneId]
""");
    }

    public override async Task Select_many_over_skip_navigation_order_by_skip(bool async)
    {
        await base.Select_many_over_skip_navigation_order_by_skip(async);

        AssertSql(
            """
SELECT [s0].[Id], [s0].[CollectionInverseId], [s0].[Name], [s0].[PeriodEnd], [s0].[PeriodStart], [s0].[ReferenceInverseId]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
INNER JOIN (
    SELECT [s].[Id], [s].[CollectionInverseId], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart], [s].[ReferenceInverseId], [s].[OneId]
    FROM (
        SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [e0].[ReferenceInverseId], [j].[OneId], ROW_NUMBER() OVER(PARTITION BY [j].[OneId] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinOneToThreePayloadFull] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
        INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[ThreeId] = [e0].[Id]
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
SELECT [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[PeriodEnd], [s0].[PeriodStart], [s0].[ReferenceInverseId]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
INNER JOIN (
    SELECT [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart], [s].[ReferenceInverseId], [s].[OneSkipSharedId]
    FROM (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[PeriodEnd], [e1].[PeriodStart], [e1].[ReferenceInverseId], [e0].[OneSkipSharedId], ROW_NUMBER() OVER(PARTITION BY [e0].[OneSkipSharedId] ORDER BY [e1].[Id]) AS [row]
        FROM [EntityOneEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
        INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
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
SELECT [s0].[Id], [s0].[CollectionInverseId], [s0].[Name], [s0].[PeriodEnd], [s0].[PeriodStart], [s0].[ReferenceInverseId]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
INNER JOIN (
    SELECT [s].[Id], [s].[CollectionInverseId], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart], [s].[ReferenceInverseId], [s].[OneId]
    FROM (
        SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [e0].[ReferenceInverseId], [j].[OneId], ROW_NUMBER() OVER(PARTITION BY [j].[OneId] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinOneToThreePayloadFullShared] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
        INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[ThreeId] = [e0].[Id]
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
SELECT [s].[Id], [s].[Discriminator], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart], [s].[Number], [s].[IsGreen]
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
INNER JOIN (
    SELECT [e1].[Id], [e1].[Discriminator], [e1].[Name], [e1].[PeriodEnd], [e1].[PeriodStart], [e1].[Number], [e1].[IsGreen], [e0].[ThreeSkipSharedId]
    FROM [EntityRootEntityThree] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    INNER JOIN [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[RootSkipSharedId] = [e1].[Id]
    WHERE [e1].[Discriminator] IN (N'EntityBranch', N'EntityLeaf')
) AS [s] ON [e].[Id] = [s].[ThreeSkipSharedId]
""");
    }

    public override async Task Select_many_over_skip_navigation_cast(bool async)
    {
        await base.Select_many_over_skip_navigation_cast(async);

        AssertSql(
            """
SELECT [s].[Id], [s].[Discriminator], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart], [s].[Number], [s].[IsGreen]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
INNER JOIN (
    SELECT [e1].[Id], [e1].[Discriminator], [e1].[Name], [e1].[PeriodEnd], [e1].[PeriodStart], [e1].[Number], [e1].[IsGreen], [j].[EntityOneId]
    FROM [JoinOneToBranch] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN (
        SELECT [e0].[Id], [e0].[Discriminator], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [e0].[Number], [e0].[IsGreen]
        FROM [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
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
SELECT [e].[Id], [s].[Id], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart], [s].[LeftId], [s].[RightId]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [j].[LeftId], [j].[RightId]
    FROM [JoinOneSelfPayload] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[LeftId] = [e0].[Id]
) AS [s] ON [e].[Id] = [s].[RightId]
ORDER BY [e].[Id], [s].[LeftId], [s].[RightId]
""");
    }

    public override async Task Select_skip_navigation_multiple(bool async)
    {
        await base.Select_skip_navigation_multiple(async);

        AssertSql(
            """
SELECT [e].[Id], [s].[Id], [s].[CollectionInverseId], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart], [s].[ReferenceInverseId], [s].[ThreeId], [s].[TwoId], [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[PeriodEnd], [s0].[PeriodStart], [s0].[ReferenceInverseId], [s0].[SelfSkipSharedLeftId], [s0].[SelfSkipSharedRightId], [s1].[Key1], [s1].[Key2], [s1].[Key3], [s1].[Name], [s1].[PeriodEnd], [s1].[PeriodStart], [s1].[TwoSkipSharedId], [s1].[CompositeKeySkipSharedKey1], [s1].[CompositeKeySkipSharedKey2], [s1].[CompositeKeySkipSharedKey3]
FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [e0].[ReferenceInverseId], [j].[ThreeId], [j].[TwoId]
    FROM [JoinTwoToThree] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[ThreeId] = [e0].[Id]
) AS [s] ON [e].[Id] = [s].[TwoId]
LEFT JOIN (
    SELECT [e2].[Id], [e2].[CollectionInverseId], [e2].[ExtraId], [e2].[Name], [e2].[PeriodEnd], [e2].[PeriodStart], [e2].[ReferenceInverseId], [e1].[SelfSkipSharedLeftId], [e1].[SelfSkipSharedRightId]
    FROM [EntityTwoEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1]
    INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e2] ON [e1].[SelfSkipSharedLeftId] = [e2].[Id]
) AS [s0] ON [e].[Id] = [s0].[SelfSkipSharedRightId]
LEFT JOIN (
    SELECT [e4].[Key1], [e4].[Key2], [e4].[Key3], [e4].[Name], [e4].[PeriodEnd], [e4].[PeriodStart], [e3].[TwoSkipSharedId], [e3].[CompositeKeySkipSharedKey1], [e3].[CompositeKeySkipSharedKey2], [e3].[CompositeKeySkipSharedKey3]
    FROM [EntityCompositeKeyEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e3]
    INNER JOIN [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e4] ON [e3].[CompositeKeySkipSharedKey1] = [e4].[Key1] AND [e3].[CompositeKeySkipSharedKey2] = [e4].[Key2] AND [e3].[CompositeKeySkipSharedKey3] = [e4].[Key3]
) AS [s1] ON [e].[Id] = [s1].[TwoSkipSharedId]
ORDER BY [e].[Id], [s].[ThreeId], [s].[TwoId], [s].[Id], [s0].[SelfSkipSharedLeftId], [s0].[SelfSkipSharedRightId], [s0].[Id], [s1].[TwoSkipSharedId], [s1].[CompositeKeySkipSharedKey1], [s1].[CompositeKeySkipSharedKey2], [s1].[CompositeKeySkipSharedKey3], [s1].[Key1], [s1].[Key2]
""");
    }

    public override async Task Select_skip_navigation_first_or_default(bool async)
    {
        await base.Select_skip_navigation_first_or_default(async);

        AssertSql(
            """
SELECT [s0].[Key1], [s0].[Key2], [s0].[Key3], [s0].[Name], [s0].[PeriodEnd], [s0].[PeriodStart]
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [s].[Key1], [s].[Key2], [s].[Key3], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart], [s].[ThreeId]
    FROM (
        SELECT [e0].[Key1], [e0].[Key2], [e0].[Key3], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [j].[ThreeId], ROW_NUMBER() OVER(PARTITION BY [j].[ThreeId] ORDER BY [e0].[Key1], [e0].[Key2]) AS [row]
        FROM [JoinThreeToCompositeKeyFull] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
        INNER JOIN [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[CompositeId1] = [e0].[Key1] AND [j].[CompositeId2] = [e0].[Key2] AND [j].[CompositeId3] = [e0].[Key3]
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
SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [s].[RootSkipSharedId], [s].[CompositeKeySkipSharedKey1], [s].[CompositeKeySkipSharedKey2], [s].[CompositeKeySkipSharedKey3], [s].[PeriodEnd], [s].[PeriodStart], [s].[Id], [s].[Discriminator], [s].[Name], [s].[PeriodEnd0], [s].[PeriodStart0], [s].[Number], [s].[Slumber], [s].[IsGreen], [s].[IsBrown]
FROM [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [e0].[RootSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], [e0].[PeriodEnd], [e0].[PeriodStart], [e1].[Id], [e1].[Discriminator], [e1].[Name], [e1].[PeriodEnd] AS [PeriodEnd0], [e1].[PeriodStart] AS [PeriodStart0], [e1].[Number], [e1].[Slumber], [e1].[IsGreen], [e1].[IsBrown]
    FROM [EntityCompositeKeyEntityRoot] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    INNER JOIN [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[RootSkipSharedId] = [e1].[Id]
) AS [s] ON [e].[Key1] = [s].[CompositeKeySkipSharedKey1] AND [e].[Key2] = [s].[CompositeKeySkipSharedKey2] AND [e].[Key3] = [s].[CompositeKeySkipSharedKey3]
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [s].[RootSkipSharedId], [s].[CompositeKeySkipSharedKey1], [s].[CompositeKeySkipSharedKey2], [s].[CompositeKeySkipSharedKey3]
""");
    }

    public override async Task Include_skip_navigation_then_reference(bool async)
    {
        await base.Include_skip_navigation_then_reference(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId], [s].[OneId], [s].[TwoId], [s].[JoinOneToTwoExtraId], [s].[PeriodEnd], [s].[PeriodStart], [s].[Id], [s].[Name], [s].[PeriodEnd0], [s].[PeriodStart0], [s].[Id0], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name0], [s].[PeriodEnd1], [s].[PeriodStart1], [s].[ReferenceInverseId]
FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [j].[PeriodEnd], [j].[PeriodStart], [e0].[Id], [e0].[Name], [e0].[PeriodEnd] AS [PeriodEnd0], [e0].[PeriodStart] AS [PeriodStart0], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[PeriodEnd] AS [PeriodEnd1], [e1].[PeriodStart] AS [PeriodStart1], [e1].[ReferenceInverseId]
    FROM [JoinOneToTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
) AS [s] ON [e].[Id] = [s].[TwoId]
ORDER BY [e].[Id], [s].[OneId], [s].[TwoId], [s].[Id]
""");
    }

    public override async Task Include_skip_navigation_then_include_skip_navigation(bool async)
    {
        await base.Include_skip_navigation_then_include_skip_navigation(async);

        AssertSql(
            """
SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [s0].[LeafId], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3], [s0].[PeriodEnd], [s0].[PeriodStart], [s0].[Id], [s0].[Discriminator], [s0].[Name], [s0].[PeriodEnd0], [s0].[PeriodStart0], [s0].[Number], [s0].[IsGreen], [s0].[EntityBranchId], [s0].[EntityOneId], [s0].[PeriodEnd1], [s0].[PeriodStart1], [s0].[Id0], [s0].[Name0], [s0].[PeriodEnd00], [s0].[PeriodStart00]
FROM [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [j].[PeriodEnd], [j].[PeriodStart], [e1].[Id], [e1].[Discriminator], [e1].[Name], [e1].[PeriodEnd] AS [PeriodEnd0], [e1].[PeriodStart] AS [PeriodStart0], [e1].[Number], [e1].[IsGreen], [s].[EntityBranchId], [s].[EntityOneId], [s].[PeriodEnd] AS [PeriodEnd1], [s].[PeriodStart] AS [PeriodStart1], [s].[Id] AS [Id0], [s].[Name] AS [Name0], [s].[PeriodEnd0] AS [PeriodEnd00], [s].[PeriodStart0] AS [PeriodStart00]
    FROM [JoinCompositeKeyToLeaf] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN (
        SELECT [e0].[Id], [e0].[Discriminator], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [e0].[Number], [e0].[IsGreen]
        FROM [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
        WHERE [e0].[Discriminator] = N'EntityLeaf'
    ) AS [e1] ON [j].[LeafId] = [e1].[Id]
    LEFT JOIN (
        SELECT [j0].[EntityBranchId], [j0].[EntityOneId], [j0].[PeriodEnd], [j0].[PeriodStart], [e2].[Id], [e2].[Name], [e2].[PeriodEnd] AS [PeriodEnd0], [e2].[PeriodStart] AS [PeriodStart0]
        FROM [JoinOneToBranch] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j0]
        INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e2] ON [j0].[EntityOneId] = [e2].[Id]
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
SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId], [s0].[OneId], [s0].[ThreeId], [s0].[Payload], [s0].[PeriodEnd], [s0].[PeriodStart], [s0].[Id], [s0].[Name], [s0].[PeriodEnd0], [s0].[PeriodStart0], [s0].[Id0], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name0], [s0].[PeriodEnd1], [s0].[PeriodStart1], [s0].[ReferenceInverseId], [s0].[LeftId], [s0].[RightId], [s0].[Payload0], [s0].[PeriodEnd2], [s0].[PeriodStart2], [s0].[Id1], [s0].[Name1], [s0].[PeriodEnd00], [s0].[PeriodStart00]
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [j].[PeriodEnd], [j].[PeriodStart], [e0].[Id], [e0].[Name], [e0].[PeriodEnd] AS [PeriodEnd0], [e0].[PeriodStart] AS [PeriodStart0], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[PeriodEnd] AS [PeriodEnd1], [e1].[PeriodStart] AS [PeriodStart1], [e1].[ReferenceInverseId], [s].[LeftId], [s].[RightId], [s].[Payload] AS [Payload0], [s].[PeriodEnd] AS [PeriodEnd2], [s].[PeriodStart] AS [PeriodStart2], [s].[Id] AS [Id1], [s].[Name] AS [Name1], [s].[PeriodEnd0] AS [PeriodEnd00], [s].[PeriodStart0] AS [PeriodStart00]
    FROM [JoinOneToThreePayloadFull] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
    LEFT JOIN (
        SELECT [j0].[LeftId], [j0].[RightId], [j0].[Payload], [j0].[PeriodEnd], [j0].[PeriodStart], [e2].[Id], [e2].[Name], [e2].[PeriodEnd] AS [PeriodEnd0], [e2].[PeriodStart] AS [PeriodStart0]
        FROM [JoinOneSelfPayload] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j0]
        INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e2] ON [j0].[RightId] = [e2].[Id]
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
SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId], [e0].[Id], [s].[OneSkipSharedId], [s].[TwoSkipSharedId], [s].[PeriodEnd], [s].[PeriodStart], [s].[Id], [s].[Name], [s].[PeriodEnd0], [s].[PeriodStart0], [e0].[CollectionInverseId], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [e0].[ReferenceInverseId]
FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [e].[Id] = [e0].[ReferenceInverseId]
LEFT JOIN (
    SELECT [e1].[OneSkipSharedId], [e1].[TwoSkipSharedId], [e1].[PeriodEnd], [e1].[PeriodStart], [e2].[Id], [e2].[Name], [e2].[PeriodEnd] AS [PeriodEnd0], [e2].[PeriodStart] AS [PeriodStart0]
    FROM [EntityOneEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e2] ON [e1].[OneSkipSharedId] = [e2].[Id]
) AS [s] ON [e].[Id] = [s].[TwoSkipSharedId]
ORDER BY [e].[Id], [e0].[Id], [s].[OneSkipSharedId], [s].[TwoSkipSharedId]
""");
    }

    public override async Task Include_skip_navigation_then_include_inverse_works_for_tracking_query(bool async)
    {
        await base.Include_skip_navigation_then_include_inverse_works_for_tracking_query(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId], [s0].[OneId], [s0].[ThreeId], [s0].[Payload], [s0].[PeriodEnd], [s0].[PeriodStart], [s0].[Id], [s0].[Name], [s0].[PeriodEnd0], [s0].[PeriodStart0], [s0].[OneId0], [s0].[ThreeId0], [s0].[Payload0], [s0].[PeriodEnd1], [s0].[PeriodStart1], [s0].[Id0], [s0].[CollectionInverseId], [s0].[Name0], [s0].[PeriodEnd00], [s0].[PeriodStart00], [s0].[ReferenceInverseId]
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [j].[PeriodEnd], [j].[PeriodStart], [e0].[Id], [e0].[Name], [e0].[PeriodEnd] AS [PeriodEnd0], [e0].[PeriodStart] AS [PeriodStart0], [s].[OneId] AS [OneId0], [s].[ThreeId] AS [ThreeId0], [s].[Payload] AS [Payload0], [s].[PeriodEnd] AS [PeriodEnd1], [s].[PeriodStart] AS [PeriodStart1], [s].[Id] AS [Id0], [s].[CollectionInverseId], [s].[Name] AS [Name0], [s].[PeriodEnd0] AS [PeriodEnd00], [s].[PeriodStart0] AS [PeriodStart00], [s].[ReferenceInverseId]
    FROM [JoinOneToThreePayloadFullShared] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN (
        SELECT [j0].[OneId], [j0].[ThreeId], [j0].[Payload], [j0].[PeriodEnd], [j0].[PeriodStart], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[PeriodEnd] AS [PeriodEnd0], [e1].[PeriodStart] AS [PeriodStart0], [e1].[ReferenceInverseId]
        FROM [JoinOneToThreePayloadFullShared] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j0]
        INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [j0].[ThreeId] = [e1].[Id]
    ) AS [s] ON [e0].[Id] = [s].[OneId]
) AS [s0] ON [e].[Id] = [s0].[ThreeId]
ORDER BY [e].[Id], [s0].[OneId], [s0].[ThreeId], [s0].[Id], [s0].[OneId0], [s0].[ThreeId0]
""");
    }

    public override async Task Filtered_include_skip_navigation_where(bool async)
    {
        await base.Filtered_include_skip_navigation_where(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId], [s].[OneId], [s].[ThreeId], [s].[Payload], [s].[PeriodEnd], [s].[PeriodStart], [s].[Id], [s].[Name], [s].[PeriodEnd0], [s].[PeriodStart0]
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [j].[PeriodEnd], [j].[PeriodStart], [e0].[Id], [e0].[Name], [e0].[PeriodEnd] AS [PeriodEnd0], [e0].[PeriodStart] AS [PeriodStart0]
    FROM [JoinOneToThreePayloadFullShared] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[OneId] = [e0].[Id]
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
SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId], [s].[ThreeId], [s].[TwoId], [s].[PeriodEnd], [s].[PeriodStart], [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[PeriodEnd0], [s].[PeriodStart0], [s].[ReferenceInverseId]
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [j].[ThreeId], [j].[TwoId], [j].[PeriodEnd], [j].[PeriodStart], [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[PeriodEnd] AS [PeriodEnd0], [e0].[PeriodStart] AS [PeriodStart0], [e0].[ReferenceInverseId]
    FROM [JoinTwoToThree] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[TwoId] = [e0].[Id]
) AS [s] ON [e].[Id] = [s].[ThreeId]
ORDER BY [e].[Id], [s].[Id], [s].[ThreeId]
""");
    }

    public override async Task Filtered_include_skip_navigation_order_by_skip(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId], [s0].[SelfSkipSharedLeftId], [s0].[SelfSkipSharedRightId], [s0].[PeriodEnd], [s0].[PeriodStart], [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[PeriodEnd0], [s0].[PeriodStart0], [s0].[ReferenceInverseId]
FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [s].[SelfSkipSharedLeftId], [s].[SelfSkipSharedRightId], [s].[PeriodEnd], [s].[PeriodStart], [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[PeriodEnd0], [s].[PeriodStart0], [s].[ReferenceInverseId]
    FROM (
        SELECT [e0].[SelfSkipSharedLeftId], [e0].[SelfSkipSharedRightId], [e0].[PeriodEnd], [e0].[PeriodStart], [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[PeriodEnd] AS [PeriodEnd0], [e1].[PeriodStart] AS [PeriodStart0], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [e0].[SelfSkipSharedLeftId] ORDER BY [e1].[Id]) AS [row]
        FROM [EntityTwoEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
        INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[SelfSkipSharedRightId] = [e1].[Id]
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
SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [s0].[TwoSkipSharedId], [s0].[CompositeKeySkipSharedKey1], [s0].[CompositeKeySkipSharedKey2], [s0].[CompositeKeySkipSharedKey3], [s0].[PeriodEnd], [s0].[PeriodStart], [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[PeriodEnd0], [s0].[PeriodStart0], [s0].[ReferenceInverseId]
FROM [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [s].[TwoSkipSharedId], [s].[CompositeKeySkipSharedKey1], [s].[CompositeKeySkipSharedKey2], [s].[CompositeKeySkipSharedKey3], [s].[PeriodEnd], [s].[PeriodStart], [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[PeriodEnd0], [s].[PeriodStart0], [s].[ReferenceInverseId]
    FROM (
        SELECT [e0].[TwoSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], [e0].[PeriodEnd], [e0].[PeriodStart], [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[PeriodEnd] AS [PeriodEnd0], [e1].[PeriodStart] AS [PeriodStart0], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3] ORDER BY [e1].[Id]) AS [row]
        FROM [EntityCompositeKeyEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
        INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
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
SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [s0].[Id], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3], [s0].[PeriodEnd], [s0].[PeriodStart], [s0].[ThreeId], [s0].[Id0], [s0].[CollectionInverseId], [s0].[Name], [s0].[PeriodEnd0], [s0].[PeriodStart0], [s0].[ReferenceInverseId]
FROM [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [s].[Id], [s].[CompositeId1], [s].[CompositeId2], [s].[CompositeId3], [s].[PeriodEnd], [s].[PeriodStart], [s].[ThreeId], [s].[Id0], [s].[CollectionInverseId], [s].[Name], [s].[PeriodEnd0], [s].[PeriodStart0], [s].[ReferenceInverseId]
    FROM (
        SELECT [j].[Id], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [j].[PeriodEnd], [j].[PeriodStart], [j].[ThreeId], [e0].[Id] AS [Id0], [e0].[CollectionInverseId], [e0].[Name], [e0].[PeriodEnd] AS [PeriodEnd0], [e0].[PeriodStart] AS [PeriodStart0], [e0].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinThreeToCompositeKeyFull] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
        INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[ThreeId] = [e0].[Id]
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
SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[Number], [e].[Slumber], [e].[IsGreen], [e].[IsBrown], [s0].[RootSkipSharedId], [s0].[ThreeSkipSharedId], [s0].[PeriodEnd], [s0].[PeriodStart], [s0].[Id], [s0].[CollectionInverseId], [s0].[Name], [s0].[PeriodEnd0], [s0].[PeriodStart0], [s0].[ReferenceInverseId], [s0].[OneId], [s0].[ThreeId], [s0].[Payload], [s0].[PeriodEnd1], [s0].[PeriodStart1], [s0].[Id0], [s0].[Name0], [s0].[PeriodEnd00], [s0].[PeriodStart00]
FROM [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [e0].[RootSkipSharedId], [e0].[ThreeSkipSharedId], [e0].[PeriodEnd], [e0].[PeriodStart], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[PeriodEnd] AS [PeriodEnd0], [e1].[PeriodStart] AS [PeriodStart0], [e1].[ReferenceInverseId], [s].[OneId], [s].[ThreeId], [s].[Payload], [s].[PeriodEnd] AS [PeriodEnd1], [s].[PeriodStart] AS [PeriodStart1], [s].[Id] AS [Id0], [s].[Name] AS [Name0], [s].[PeriodEnd0] AS [PeriodEnd00], [s].[PeriodStart0] AS [PeriodStart00]
    FROM [EntityRootEntityThree] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[ThreeSkipSharedId] = [e1].[Id]
    LEFT JOIN (
        SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [j].[PeriodEnd], [j].[PeriodStart], [e2].[Id], [e2].[Name], [e2].[PeriodEnd] AS [PeriodEnd0], [e2].[PeriodStart] AS [PeriodStart0]
        FROM [JoinOneToThreePayloadFullShared] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
        INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e2] ON [j].[OneId] = [e2].[Id]
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
SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[Number], [e].[Slumber], [e].[IsGreen], [e].[IsBrown], [s1].[RootSkipSharedId], [s1].[CompositeKeySkipSharedKey1], [s1].[CompositeKeySkipSharedKey2], [s1].[CompositeKeySkipSharedKey3], [s1].[PeriodEnd], [s1].[PeriodStart], [s1].[Key1], [s1].[Key2], [s1].[Key3], [s1].[Name], [s1].[PeriodEnd0], [s1].[PeriodStart0], [s1].[Id], [s1].[CompositeId1], [s1].[CompositeId2], [s1].[CompositeId3], [s1].[PeriodEnd1], [s1].[PeriodStart1], [s1].[ThreeId], [s1].[Id0], [s1].[CollectionInverseId], [s1].[Name0], [s1].[PeriodEnd00], [s1].[PeriodStart00], [s1].[ReferenceInverseId]
FROM [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [e0].[RootSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], [e0].[PeriodEnd], [e0].[PeriodStart], [e1].[Key1], [e1].[Key2], [e1].[Key3], [e1].[Name], [e1].[PeriodEnd] AS [PeriodEnd0], [e1].[PeriodStart] AS [PeriodStart0], [s0].[Id], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3], [s0].[PeriodEnd] AS [PeriodEnd1], [s0].[PeriodStart] AS [PeriodStart1], [s0].[ThreeId], [s0].[Id0], [s0].[CollectionInverseId], [s0].[Name] AS [Name0], [s0].[PeriodEnd0] AS [PeriodEnd00], [s0].[PeriodStart0] AS [PeriodStart00], [s0].[ReferenceInverseId]
    FROM [EntityCompositeKeyEntityRoot] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    INNER JOIN [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[CompositeKeySkipSharedKey1] = [e1].[Key1] AND [e0].[CompositeKeySkipSharedKey2] = [e1].[Key2] AND [e0].[CompositeKeySkipSharedKey3] = [e1].[Key3]
    LEFT JOIN (
        SELECT [s].[Id], [s].[CompositeId1], [s].[CompositeId2], [s].[CompositeId3], [s].[PeriodEnd], [s].[PeriodStart], [s].[ThreeId], [s].[Id0], [s].[CollectionInverseId], [s].[Name], [s].[PeriodEnd0], [s].[PeriodStart0], [s].[ReferenceInverseId]
        FROM (
            SELECT [j].[Id], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [j].[PeriodEnd], [j].[PeriodStart], [j].[ThreeId], [e2].[Id] AS [Id0], [e2].[CollectionInverseId], [e2].[Name], [e2].[PeriodEnd] AS [PeriodEnd0], [e2].[PeriodStart] AS [PeriodStart0], [e2].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3] ORDER BY [e2].[Id]) AS [row]
            FROM [JoinThreeToCompositeKeyFull] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
            INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e2] ON [j].[ThreeId] = [e2].[Id]
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
SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[Number], [e].[IsGreen], [s0].[LeafId], [s0].[CompositeId1], [s0].[CompositeId2], [s0].[CompositeId3], [s0].[PeriodEnd], [s0].[PeriodStart], [s0].[Key1], [s0].[Key2], [s0].[Key3], [s0].[Name], [s0].[PeriodEnd0], [s0].[PeriodStart0], [s0].[TwoSkipSharedId], [s0].[CompositeKeySkipSharedKey1], [s0].[CompositeKeySkipSharedKey2], [s0].[CompositeKeySkipSharedKey3], [s0].[PeriodEnd1], [s0].[PeriodStart1], [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name0], [s0].[PeriodEnd00], [s0].[PeriodStart00], [s0].[ReferenceInverseId]
FROM [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [j].[PeriodEnd], [j].[PeriodStart], [e0].[Key1], [e0].[Key2], [e0].[Key3], [e0].[Name], [e0].[PeriodEnd] AS [PeriodEnd0], [e0].[PeriodStart] AS [PeriodStart0], [s].[TwoSkipSharedId], [s].[CompositeKeySkipSharedKey1], [s].[CompositeKeySkipSharedKey2], [s].[CompositeKeySkipSharedKey3], [s].[PeriodEnd] AS [PeriodEnd1], [s].[PeriodStart] AS [PeriodStart1], [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name] AS [Name0], [s].[PeriodEnd0] AS [PeriodEnd00], [s].[PeriodStart0] AS [PeriodStart00], [s].[ReferenceInverseId]
    FROM [JoinCompositeKeyToLeaf] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[CompositeId1] = [e0].[Key1] AND [j].[CompositeId2] = [e0].[Key2] AND [j].[CompositeId3] = [e0].[Key3]
    LEFT JOIN (
        SELECT [e1].[TwoSkipSharedId], [e1].[CompositeKeySkipSharedKey1], [e1].[CompositeKeySkipSharedKey2], [e1].[CompositeKeySkipSharedKey3], [e1].[PeriodEnd], [e1].[PeriodStart], [e2].[Id], [e2].[CollectionInverseId], [e2].[ExtraId], [e2].[Name], [e2].[PeriodEnd] AS [PeriodEnd0], [e2].[PeriodStart] AS [PeriodStart0], [e2].[ReferenceInverseId]
        FROM [EntityCompositeKeyEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1]
        INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e2] ON [e1].[TwoSkipSharedId] = [e2].[Id]
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
SELECT [e].[Id], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [s1].[OneId], [s1].[TwoId], [s1].[JoinOneToTwoExtraId], [s1].[PeriodEnd], [s1].[PeriodStart], [s1].[Id], [s1].[CollectionInverseId], [s1].[ExtraId], [s1].[Name], [s1].[PeriodEnd0], [s1].[PeriodStart0], [s1].[ReferenceInverseId], [s1].[ThreeId], [s1].[TwoId0], [s1].[PeriodEnd1], [s1].[PeriodStart1], [s1].[Id0], [s1].[CollectionInverseId0], [s1].[Name0], [s1].[PeriodEnd00], [s1].[PeriodStart00], [s1].[ReferenceInverseId0]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
OUTER APPLY (
    SELECT [s].[OneId], [s].[TwoId], [s].[JoinOneToTwoExtraId], [s].[PeriodEnd], [s].[PeriodStart], [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[PeriodEnd0], [s].[PeriodStart0], [s].[ReferenceInverseId], [s0].[ThreeId], [s0].[TwoId] AS [TwoId0], [s0].[PeriodEnd] AS [PeriodEnd1], [s0].[PeriodStart] AS [PeriodStart1], [s0].[Id] AS [Id0], [s0].[CollectionInverseId] AS [CollectionInverseId0], [s0].[Name] AS [Name0], [s0].[PeriodEnd0] AS [PeriodEnd00], [s0].[PeriodStart0] AS [PeriodStart00], [s0].[ReferenceInverseId] AS [ReferenceInverseId0]
    FROM (
        SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [j].[PeriodEnd], [j].[PeriodStart], [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[PeriodEnd] AS [PeriodEnd0], [e0].[PeriodStart] AS [PeriodStart0], [e0].[ReferenceInverseId]
        FROM [JoinOneToTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
        INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[TwoId] = [e0].[Id]
        WHERE [e].[Id] = [j].[OneId]
        ORDER BY [e0].[Id]
        OFFSET 1 ROWS FETCH NEXT 2 ROWS ONLY
    ) AS [s]
    LEFT JOIN (
        SELECT [j0].[ThreeId], [j0].[TwoId], [j0].[PeriodEnd], [j0].[PeriodStart], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[PeriodEnd] AS [PeriodEnd0], [e1].[PeriodStart] AS [PeriodStart0], [e1].[ReferenceInverseId]
        FROM [JoinTwoToThree] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j0]
        INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [j0].[ThreeId] = [e1].[Id]
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
SELECT [e].[Id], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [s1].[OneId], [s1].[TwoId], [s1].[JoinOneToTwoExtraId], [s1].[PeriodEnd], [s1].[PeriodStart], [s1].[Id], [s1].[CollectionInverseId], [s1].[ExtraId], [s1].[Name], [s1].[PeriodEnd0], [s1].[PeriodStart0], [s1].[ReferenceInverseId], [s1].[ThreeId], [s1].[TwoId0], [s1].[PeriodEnd1], [s1].[PeriodStart1], [s1].[Id0], [s1].[CollectionInverseId0], [s1].[Name0], [s1].[PeriodEnd00], [s1].[PeriodStart00], [s1].[ReferenceInverseId0]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [j].[PeriodEnd], [j].[PeriodStart], [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[PeriodEnd] AS [PeriodEnd0], [e0].[PeriodStart] AS [PeriodStart0], [e0].[ReferenceInverseId], [s0].[ThreeId], [s0].[TwoId] AS [TwoId0], [s0].[PeriodEnd] AS [PeriodEnd1], [s0].[PeriodStart] AS [PeriodStart1], [s0].[Id] AS [Id0], [s0].[CollectionInverseId] AS [CollectionInverseId0], [s0].[Name] AS [Name0], [s0].[PeriodEnd0] AS [PeriodEnd00], [s0].[PeriodStart0] AS [PeriodStart00], [s0].[ReferenceInverseId] AS [ReferenceInverseId0]
    FROM [JoinOneToTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[TwoId] = [e0].[Id]
    LEFT JOIN (
        SELECT [s].[ThreeId], [s].[TwoId], [s].[PeriodEnd], [s].[PeriodStart], [s].[Id], [s].[CollectionInverseId], [s].[Name], [s].[PeriodEnd0], [s].[PeriodStart0], [s].[ReferenceInverseId]
        FROM (
            SELECT [j0].[ThreeId], [j0].[TwoId], [j0].[PeriodEnd], [j0].[PeriodStart], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[PeriodEnd] AS [PeriodEnd0], [e1].[PeriodStart] AS [PeriodStart0], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j0].[TwoId] ORDER BY [e1].[Id]) AS [row]
            FROM [JoinTwoToThree] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j0]
            INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [j0].[ThreeId] = [e1].[Id]
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
SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId], [s].[OneId], [s].[TwoId], [s].[JoinOneToTwoExtraId], [s].[PeriodEnd], [s].[PeriodStart], [s].[Id], [s].[Name], [s].[PeriodEnd0], [s].[PeriodStart0], [s].[Id0], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name0], [s].[PeriodEnd1], [s].[PeriodStart1], [s].[ReferenceInverseId], [s].[Id1], [s].[CollectionInverseId0], [s].[ExtraId0], [s].[Name1], [s].[PeriodEnd2], [s].[PeriodStart2], [s].[ReferenceInverseId0]
FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [j].[PeriodEnd], [j].[PeriodStart], [e0].[Id], [e0].[Name], [e0].[PeriodEnd] AS [PeriodEnd0], [e0].[PeriodStart] AS [PeriodStart0], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[PeriodEnd] AS [PeriodEnd1], [e1].[PeriodStart] AS [PeriodStart1], [e1].[ReferenceInverseId], [e2].[Id] AS [Id1], [e2].[CollectionInverseId] AS [CollectionInverseId0], [e2].[ExtraId] AS [ExtraId0], [e2].[Name] AS [Name1], [e2].[PeriodEnd] AS [PeriodEnd2], [e2].[PeriodStart] AS [PeriodStart2], [e2].[ReferenceInverseId] AS [ReferenceInverseId0]
    FROM [JoinOneToTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
    LEFT JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e2] ON [e0].[Id] = [e2].[CollectionInverseId]
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
SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId], [s2].[OneId], [s2].[ThreeId], [s2].[Payload], [s2].[PeriodEnd], [s2].[PeriodStart], [s2].[Id], [s2].[Name], [s2].[PeriodEnd0], [s2].[PeriodStart0], [s2].[OneId0], [s2].[TwoId], [s2].[JoinOneToTwoExtraId], [s2].[PeriodEnd1], [s2].[PeriodStart1], [s2].[Id0], [s2].[CollectionInverseId], [s2].[ExtraId], [s2].[Name0], [s2].[PeriodEnd00], [s2].[PeriodStart00], [s2].[ReferenceInverseId], [s2].[EntityBranchId], [s2].[EntityOneId], [s2].[PeriodEnd2], [s2].[PeriodStart2], [s2].[Id1], [s2].[Discriminator], [s2].[Name1], [s2].[PeriodEnd01], [s2].[PeriodStart01], [s2].[Number], [s2].[IsGreen]
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [j].[PeriodEnd], [j].[PeriodStart], [e0].[Id], [e0].[Name], [e0].[PeriodEnd] AS [PeriodEnd0], [e0].[PeriodStart] AS [PeriodStart0], [s0].[OneId] AS [OneId0], [s0].[TwoId], [s0].[JoinOneToTwoExtraId], [s0].[PeriodEnd] AS [PeriodEnd1], [s0].[PeriodStart] AS [PeriodStart1], [s0].[Id] AS [Id0], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name] AS [Name0], [s0].[PeriodEnd0] AS [PeriodEnd00], [s0].[PeriodStart0] AS [PeriodStart00], [s0].[ReferenceInverseId], [s1].[EntityBranchId], [s1].[EntityOneId], [s1].[PeriodEnd] AS [PeriodEnd2], [s1].[PeriodStart] AS [PeriodStart2], [s1].[Id] AS [Id1], [s1].[Discriminator], [s1].[Name] AS [Name1], [s1].[PeriodEnd0] AS [PeriodEnd01], [s1].[PeriodStart0] AS [PeriodStart01], [s1].[Number], [s1].[IsGreen]
    FROM [JoinOneToThreePayloadFull] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN (
        SELECT [s].[OneId], [s].[TwoId], [s].[JoinOneToTwoExtraId], [s].[PeriodEnd], [s].[PeriodStart], [s].[Id], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name], [s].[PeriodEnd0], [s].[PeriodStart0], [s].[ReferenceInverseId]
        FROM (
            SELECT [j0].[OneId], [j0].[TwoId], [j0].[JoinOneToTwoExtraId], [j0].[PeriodEnd], [j0].[PeriodStart], [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[PeriodEnd] AS [PeriodEnd0], [e1].[PeriodStart] AS [PeriodStart0], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j0].[OneId] ORDER BY [e1].[Id]) AS [row]
            FROM [JoinOneToTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j0]
            INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [j0].[TwoId] = [e1].[Id]
        ) AS [s]
        WHERE 1 < [s].[row] AND [s].[row] <= 3
    ) AS [s0] ON [e0].[Id] = [s0].[OneId]
    LEFT JOIN (
        SELECT [j1].[EntityBranchId], [j1].[EntityOneId], [j1].[PeriodEnd], [j1].[PeriodStart], [e3].[Id], [e3].[Discriminator], [e3].[Name], [e3].[PeriodEnd] AS [PeriodEnd0], [e3].[PeriodStart] AS [PeriodStart0], [e3].[Number], [e3].[IsGreen]
        FROM [JoinOneToBranch] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j1]
        INNER JOIN (
            SELECT [e2].[Id], [e2].[Discriminator], [e2].[Name], [e2].[PeriodEnd], [e2].[PeriodStart], [e2].[Number], [e2].[IsGreen]
            FROM [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e2]
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
SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId], [s].[OneId], [s].[ThreeId], [s].[Payload], [s].[PeriodEnd], [s].[PeriodStart], [s].[Id], [s].[Name], [s].[PeriodEnd0], [s].[PeriodStart0], [s].[Id0], [s].[CollectionInverseId], [s].[ExtraId], [s].[Name0], [s].[PeriodEnd1], [s].[PeriodStart1], [s].[ReferenceInverseId]
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [j].[PeriodEnd], [j].[PeriodStart], [e0].[Id], [e0].[Name], [e0].[PeriodEnd] AS [PeriodEnd0], [e0].[PeriodStart] AS [PeriodStart0], [e2].[Id] AS [Id0], [e2].[CollectionInverseId], [e2].[ExtraId], [e2].[Name] AS [Name0], [e2].[PeriodEnd] AS [PeriodEnd1], [e2].[PeriodStart] AS [PeriodStart1], [e2].[ReferenceInverseId]
    FROM [JoinOneToThreePayloadFull] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[PeriodEnd], [e1].[PeriodStart], [e1].[ReferenceInverseId]
        FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1]
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
SELECT [e].[Id], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [s0].[Id], [s0].[CollectionInverseId], [s0].[ExtraId], [s0].[Name], [s0].[PeriodEnd], [s0].[PeriodStart], [s0].[ReferenceInverseId], [s0].[ThreeId], [s0].[TwoId], [s0].[PeriodEnd0], [s0].[PeriodStart0], [s0].[Id0], [s0].[CollectionInverseId0], [s0].[Name0], [s0].[PeriodEnd00], [s0].[PeriodStart00], [s0].[ReferenceInverseId0]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [e0].[ReferenceInverseId], [s].[ThreeId], [s].[TwoId], [s].[PeriodEnd] AS [PeriodEnd0], [s].[PeriodStart] AS [PeriodStart0], [s].[Id] AS [Id0], [s].[CollectionInverseId] AS [CollectionInverseId0], [s].[Name] AS [Name0], [s].[PeriodEnd0] AS [PeriodEnd00], [s].[PeriodStart0] AS [PeriodStart00], [s].[ReferenceInverseId] AS [ReferenceInverseId0]
    FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    LEFT JOIN (
        SELECT [j].[ThreeId], [j].[TwoId], [j].[PeriodEnd], [j].[PeriodStart], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[PeriodEnd] AS [PeriodEnd0], [e1].[PeriodStart] AS [PeriodStart0], [e1].[ReferenceInverseId]
        FROM [JoinTwoToThree] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
        INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [j].[ThreeId] = [e1].[Id]
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

        AssertSql(
            @"");
    }

    public override async Task Filtered_includes_accessed_via_different_path_are_merged(bool async)
    {
        await base.Filtered_includes_accessed_via_different_path_are_merged(async);

        AssertSql(
            @"");
    }

    public override async Task Throws_when_different_filtered_then_include_via_different_paths(bool async)
    {
        await base.Throws_when_different_filtered_then_include_via_different_paths(async);

        AssertSql(
            @"");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
