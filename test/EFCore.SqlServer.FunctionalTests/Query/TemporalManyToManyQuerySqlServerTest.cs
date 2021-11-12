// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    [SqlServerCondition(SqlServerCondition.SupportsTemporalTablesCascadeDelete)]
    public class TemporalManyToManyQuerySqlServerTest : ManyToManyQueryRelationalTestBase<TemporalManyToManyQuerySqlServerFixture>
    {
#pragma warning disable IDE0060 // Remove unused parameter
        public TemporalManyToManyQuerySqlServerTest(TemporalManyToManyQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
#pragma warning restore IDE0060 // Remove unused parameter
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
        {
            var temporalEntityTypes = new List<Type>
            {
                typeof(TestModels.ManyToManyModel.EntityOne),
                typeof(TestModels.ManyToManyModel.EntityTwo),
                typeof(TestModels.ManyToManyModel.EntityThree),
                typeof(TestModels.ManyToManyModel.EntityCompositeKey),
                typeof(TestModels.ManyToManyModel.EntityRoot),
                typeof(TestModels.ManyToManyModel.EntityBranch),
                typeof(TestModels.ManyToManyModel.EntityLeaf),
            };

            var rewriter = new TemporalPointInTimeQueryRewriter(Fixture.ChangesDate, temporalEntityTypes);

            return rewriter.Visit(serverQueryExpression);
        }

        public override async Task Skip_navigation_all(bool async)
        {
            await base.Skip_navigation_all(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
WHERE NOT EXISTS (
    SELECT 1
    FROM [JoinOneToTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[TwoId] = [e0].[Id]
    WHERE ([e].[Id] = [j].[OneId]) AND NOT ([e0].[Name] LIKE N'%B%'))");
        }

        public override async Task Skip_navigation_any_without_predicate(bool async)
        {
            await base.Skip_navigation_any_without_predicate(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
WHERE EXISTS (
    SELECT 1
    FROM [JoinOneToThreePayloadFull] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[ThreeId] = [e0].[Id]
    WHERE ([e].[Id] = [j].[OneId]) AND ([e0].[Name] LIKE N'%B%'))");
        }

        public override async Task Skip_navigation_any_with_predicate(bool async)
        {
            await base.Skip_navigation_any_with_predicate(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
WHERE EXISTS (
    SELECT 1
    FROM [EntityOneEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
    WHERE ([e].[Id] = [e0].[OneSkipSharedId]) AND ([e1].[Name] LIKE N'%B%'))");
        }

        public override async Task Skip_navigation_contains(bool async)
        {
            await base.Skip_navigation_contains(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
WHERE EXISTS (
    SELECT 1
    FROM [JoinOneToThreePayloadFullShared] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[ThreeId] = [e0].[Id]
    WHERE ([e].[Id] = [j].[OneId]) AND ([e0].[Id] = 1))");
        }

        public override async Task Skip_navigation_count_without_predicate(bool async)
        {
            await base.Skip_navigation_count_without_predicate(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
WHERE (
    SELECT COUNT(*)
    FROM [JoinOneSelfPayload] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[LeftId] = [e0].[Id]
    WHERE [e].[Id] = [j].[RightId]) > 0");
        }

        public override async Task Skip_navigation_count_with_predicate(bool async)
        {
            await base.Skip_navigation_count_with_predicate(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
ORDER BY (
    SELECT COUNT(*)
    FROM [JoinOneToBranch] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN (
        SELECT [e0].[Id], [e0].[Discriminator], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [e0].[Number], [e0].[IsGreen]
        FROM [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
        WHERE [e0].[Discriminator] IN (N'EntityBranch', N'EntityLeaf')
    ) AS [t] ON [j].[EntityBranchId] = [t].[Id]
    WHERE ([e].[Id] = [j].[EntityOneId]) AND (([t].[Name] IS NOT NULL) AND ([t].[Name] LIKE N'L%'))), [e].[Id]");
        }

        public override async Task Skip_navigation_long_count_without_predicate(bool async)
        {
            await base.Skip_navigation_long_count_without_predicate(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId]
FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
WHERE (
    SELECT COUNT_BIG(*)
    FROM [JoinTwoToThree] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[ThreeId] = [e0].[Id]
    WHERE [e].[Id] = [j].[TwoId]) > CAST(0 AS bigint)");
        }

        public override async Task Skip_navigation_long_count_with_predicate(bool async)
        {
            await base.Skip_navigation_long_count_with_predicate(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId]
FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
ORDER BY (
    SELECT COUNT_BIG(*)
    FROM [EntityTwoEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[SelfSkipSharedLeftId] = [e1].[Id]
    WHERE ([e].[Id] = [e0].[SelfSkipSharedRightId]) AND (([e1].[Name] IS NOT NULL) AND ([e1].[Name] LIKE N'L%'))) DESC, [e].[Id]");
        }

        public override async Task Skip_navigation_select_many_average(bool async)
        {
            await base.Skip_navigation_select_many_average(async);

            AssertSql(
                @"SELECT AVG(CAST([t].[Key1] AS float))
FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
INNER JOIN (
    SELECT [e1].[Key1], [e0].[TwoSkipSharedId]
    FROM [EntityCompositeKeyEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    INNER JOIN [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON (([e0].[CompositeKeySkipSharedKey1] = [e1].[Key1]) AND ([e0].[CompositeKeySkipSharedKey2] = [e1].[Key2])) AND ([e0].[CompositeKeySkipSharedKey3] = [e1].[Key3])
) AS [t] ON [e].[Id] = [t].[TwoSkipSharedId]");
        }

        public override async Task Skip_navigation_select_many_max(bool async)
        {
            await base.Skip_navigation_select_many_max(async);

            AssertSql(
                @"SELECT MAX([t].[Key1])
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
INNER JOIN (
    SELECT [e0].[Key1], [j].[ThreeId]
    FROM [JoinThreeToCompositeKeyFull] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON (([j].[CompositeId1] = [e0].[Key1]) AND ([j].[CompositeId2] = [e0].[Key2])) AND ([j].[CompositeId3] = [e0].[Key3])
) AS [t] ON [e].[Id] = [t].[ThreeId]");
        }

        public override async Task Skip_navigation_select_many_min(bool async)
        {
            await base.Skip_navigation_select_many_min(async);

            AssertSql(
                @"SELECT MIN([t].[Id])
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
INNER JOIN (
    SELECT [e1].[Id], [e0].[ThreeSkipSharedId]
    FROM [EntityRootEntityThree] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    INNER JOIN [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[RootSkipSharedId] = [e1].[Id]
) AS [t] ON [e].[Id] = [t].[ThreeSkipSharedId]");
        }

        public override async Task Skip_navigation_select_many_sum(bool async)
        {
            await base.Skip_navigation_select_many_sum(async);

            AssertSql(
                @"SELECT COALESCE(SUM([t].[Key1]), 0)
FROM [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
INNER JOIN (
    SELECT [e1].[Key1], [e0].[RootSkipSharedId]
    FROM [EntityCompositeKeyEntityRoot] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    INNER JOIN [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON (([e0].[CompositeKeySkipSharedKey1] = [e1].[Key1]) AND ([e0].[CompositeKeySkipSharedKey2] = [e1].[Key2])) AND ([e0].[CompositeKeySkipSharedKey3] = [e1].[Key3])
) AS [t] ON [e].[Id] = [t].[RootSkipSharedId]");
        }

        public override async Task Skip_navigation_select_subquery_average(bool async)
        {
            await base.Skip_navigation_select_subquery_average(async);

            AssertSql(
                @"SELECT (
    SELECT AVG(CAST([e0].[Key1] AS float))
    FROM [JoinCompositeKeyToLeaf] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON (([j].[CompositeId1] = [e0].[Key1]) AND ([j].[CompositeId2] = [e0].[Key2])) AND ([j].[CompositeId3] = [e0].[Key3])
    WHERE [e].[Id] = [j].[LeafId])
FROM [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
WHERE [e].[Discriminator] = N'EntityLeaf'");
        }

        public override async Task Skip_navigation_select_subquery_max(bool async)
        {
            await base.Skip_navigation_select_subquery_max(async);

            AssertSql(
                @"SELECT (
    SELECT MAX([e0].[Id])
    FROM [JoinOneToTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e].[Id] = [j].[TwoId])
FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]");
        }

        public override async Task Skip_navigation_select_subquery_min(bool async)
        {
            await base.Skip_navigation_select_subquery_min(async);

            AssertSql(
                @"SELECT (
    SELECT MIN([e0].[Id])
    FROM [JoinOneToThreePayloadFull] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e].[Id] = [j].[ThreeId])
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]");
        }

        public override async Task Skip_navigation_select_subquery_sum(bool async)
        {
            await base.Skip_navigation_select_subquery_sum(async);

            AssertSql(
                @"SELECT (
    SELECT COALESCE(SUM([e1].[Id]), 0)
    FROM [EntityOneEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[OneSkipSharedId] = [e1].[Id]
    WHERE [e].[Id] = [e0].[TwoSkipSharedId])
FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]");
        }

        public override async Task Skip_navigation_order_by_first_or_default(bool async)
        {
            await base.Skip_navigation_order_by_first_or_default(async);

            AssertSql(
                @"SELECT [t0].[Id], [t0].[Name], [t0].[PeriodEnd], [t0].[PeriodStart]
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [t].[Id], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t].[ThreeId]
    FROM (
        SELECT [e0].[Id], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [j].[ThreeId], ROW_NUMBER() OVER(PARTITION BY [j].[ThreeId] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinOneToThreePayloadFullShared] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
        INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[OneId] = [e0].[Id]
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [e].[Id] = [t0].[ThreeId]");
        }

        public override async Task Skip_navigation_order_by_single_or_default(bool async)
        {
            await base.Skip_navigation_order_by_single_or_default(async);

            AssertSql(
                @"SELECT [t0].[Id], [t0].[Name], [t0].[PeriodEnd], [t0].[PeriodStart]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
OUTER APPLY (
    SELECT TOP(1) [t].[Id], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart]
    FROM (
        SELECT TOP(1) [e0].[Id], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart]
        FROM [JoinOneSelfPayload] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
        INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[RightId] = [e0].[Id]
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
                @"SELECT [t0].[Id], [t0].[Name], [t0].[PeriodEnd], [t0].[PeriodStart]
FROM [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [t].[Id], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t].[EntityBranchId]
    FROM (
        SELECT [e0].[Id], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [j].[EntityBranchId], ROW_NUMBER() OVER(PARTITION BY [j].[EntityBranchId] ORDER BY [e0].[Id] DESC) AS [row]
        FROM [JoinOneToBranch] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
        INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[EntityOneId] = [e0].[Id]
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [e].[Id] = [t0].[EntityBranchId]
WHERE [e].[Discriminator] IN (N'EntityBranch', N'EntityLeaf')");
        }

        public override async Task Skip_navigation_order_by_reverse_first_or_default(bool async)
        {
            await base.Skip_navigation_order_by_reverse_first_or_default(async);

            AssertSql(
                @"SELECT [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[ReferenceInverseId]
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t].[ReferenceInverseId], [t].[ThreeId]
    FROM (
        SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [e0].[ReferenceInverseId], [j].[ThreeId], ROW_NUMBER() OVER(PARTITION BY [j].[ThreeId] ORDER BY [e0].[Id] DESC) AS [row]
        FROM [JoinTwoToThree] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
        INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[TwoId] = [e0].[Id]
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [e].[Id] = [t0].[ThreeId]");
        }

        public override async Task Skip_navigation_cast(bool async)
        {
            await base.Skip_navigation_cast(async);

            AssertSql(
                @"SELECT [e].[Key1], [e].[Key2], [e].[Key3], [t0].[Id], [t0].[Discriminator], [t0].[Name], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Number], [t0].[IsGreen], [t0].[LeafId], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3]
FROM [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t].[Number], [t].[IsGreen], [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3]
    FROM [JoinCompositeKeyToLeaf] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN (
        SELECT [e0].[Id], [e0].[Discriminator], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [e0].[Number], [e0].[IsGreen]
        FROM [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
        WHERE [e0].[Discriminator] = N'EntityLeaf'
    ) AS [t] ON [j].[LeafId] = [t].[Id]
) AS [t0] ON (([e].[Key1] = [t0].[CompositeId1]) AND ([e].[Key2] = [t0].[CompositeId2])) AND ([e].[Key3] = [t0].[CompositeId3])
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [t0].[LeafId], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3]");
        }

        public override async Task Skip_navigation_of_type(bool async)
        {
            await base.Skip_navigation_of_type(async);

            AssertSql(
                @"SELECT [e].[Key1], [e].[Key2], [e].[Key3], [t].[Id], [t].[Discriminator], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t].[Number], [t].[IsGreen], [t].[RootSkipSharedId], [t].[CompositeKeySkipSharedKey1], [t].[CompositeKeySkipSharedKey2], [t].[CompositeKeySkipSharedKey3]
FROM [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [e1].[Id], [e1].[Discriminator], [e1].[Name], [e1].[PeriodEnd], [e1].[PeriodStart], [e1].[Number], [e1].[IsGreen], [e0].[RootSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3]
    FROM [EntityCompositeKeyEntityRoot] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    INNER JOIN [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[RootSkipSharedId] = [e1].[Id]
    WHERE [e1].[Discriminator] = N'EntityLeaf'
) AS [t] ON (([e].[Key1] = [t].[CompositeKeySkipSharedKey1]) AND ([e].[Key2] = [t].[CompositeKeySkipSharedKey2])) AND ([e].[Key3] = [t].[CompositeKeySkipSharedKey3])
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [t].[RootSkipSharedId], [t].[CompositeKeySkipSharedKey1], [t].[CompositeKeySkipSharedKey2], [t].[CompositeKeySkipSharedKey3]");
        }

        public override async Task Join_with_skip_navigation(bool async)
        {
            await base.Join_with_skip_navigation(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId], [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [e0].[ReferenceInverseId]
FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [e].[Id] = (
    SELECT TOP(1) [e2].[Id]
    FROM [EntityTwoEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1]
    INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e2] ON [e1].[SelfSkipSharedRightId] = [e2].[Id]
    WHERE [e0].[Id] = [e1].[SelfSkipSharedLeftId]
    ORDER BY [e2].[Id])");
        }

        public override async Task Left_join_with_skip_navigation(bool async)
        {
            await base.Left_join_with_skip_navigation(async);

            AssertSql(
                @"SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e0].[Key1], [e0].[Key2], [e0].[Key3], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart]
FROM [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON (
    SELECT TOP(1) [e2].[Id]
    FROM [EntityCompositeKeyEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1]
    INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e2] ON [e1].[TwoSkipSharedId] = [e2].[Id]
    WHERE (([e].[Key1] = [e1].[CompositeKeySkipSharedKey1]) AND ([e].[Key2] = [e1].[CompositeKeySkipSharedKey2])) AND ([e].[Key3] = [e1].[CompositeKeySkipSharedKey3])
    ORDER BY [e2].[Id]) = (
    SELECT TOP(1) [e3].[Id]
    FROM [JoinThreeToCompositeKeyFull] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e3] ON [j].[ThreeId] = [e3].[Id]
    WHERE (([e0].[Key1] = [j].[CompositeId1]) AND ([e0].[Key2] = [j].[CompositeId2])) AND ([e0].[Key3] = [j].[CompositeId3])
    ORDER BY [e3].[Id])
ORDER BY [e].[Key1], [e0].[Key1], [e].[Key2], [e0].[Key2]");
        }

        public override async Task Select_many_over_skip_navigation(bool async)
        {
            await base.Select_many_over_skip_navigation(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[CollectionInverseId], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t].[ReferenceInverseId]
FROM [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
INNER JOIN (
    SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[PeriodEnd], [e1].[PeriodStart], [e1].[ReferenceInverseId], [e0].[RootSkipSharedId]
    FROM [EntityRootEntityThree] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[ThreeSkipSharedId] = [e1].[Id]
) AS [t] ON [e].[Id] = [t].[RootSkipSharedId]");
        }

        public override async Task Select_many_over_skip_navigation_where(bool async)
        {
            await base.Select_many_over_skip_navigation_where(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t].[ReferenceInverseId]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [e0].[ReferenceInverseId], [j].[OneId]
    FROM [JoinOneToTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[TwoId] = [e0].[Id]
) AS [t] ON [e].[Id] = [t].[OneId]");
        }

        public override async Task Select_many_over_skip_navigation_order_by_skip(bool async)
        {
            await base.Select_many_over_skip_navigation_order_by_skip(async);

            AssertSql(
                @"SELECT [t0].[Id], [t0].[CollectionInverseId], [t0].[Name], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[ReferenceInverseId]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
INNER JOIN (
    SELECT [t].[Id], [t].[CollectionInverseId], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t].[ReferenceInverseId], [t].[OneId]
    FROM (
        SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [e0].[ReferenceInverseId], [j].[OneId], ROW_NUMBER() OVER(PARTITION BY [j].[OneId] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinOneToThreePayloadFull] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
        INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[ThreeId] = [e0].[Id]
    ) AS [t]
    WHERE 2 < [t].[row]
) AS [t0] ON [e].[Id] = [t0].[OneId]");
        }

        public override async Task Select_many_over_skip_navigation_order_by_take(bool async)
        {
            await base.Select_many_over_skip_navigation_order_by_take(async);

            AssertSql(
                @"SELECT [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[ReferenceInverseId]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
INNER JOIN (
    SELECT [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t].[ReferenceInverseId], [t].[OneSkipSharedId]
    FROM (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[PeriodEnd], [e1].[PeriodStart], [e1].[ReferenceInverseId], [e0].[OneSkipSharedId], ROW_NUMBER() OVER(PARTITION BY [e0].[OneSkipSharedId] ORDER BY [e1].[Id]) AS [row]
        FROM [EntityOneEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
        INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
    ) AS [t]
    WHERE [t].[row] <= 2
) AS [t0] ON [e].[Id] = [t0].[OneSkipSharedId]");
        }

        public override async Task Select_many_over_skip_navigation_order_by_skip_take(bool async)
        {
            await base.Select_many_over_skip_navigation_order_by_skip_take(async);

            AssertSql(
                @"SELECT [t0].[Id], [t0].[CollectionInverseId], [t0].[Name], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[ReferenceInverseId]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
INNER JOIN (
    SELECT [t].[Id], [t].[CollectionInverseId], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t].[ReferenceInverseId], [t].[OneId]
    FROM (
        SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [e0].[ReferenceInverseId], [j].[OneId], ROW_NUMBER() OVER(PARTITION BY [j].[OneId] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinOneToThreePayloadFullShared] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
        INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[ThreeId] = [e0].[Id]
    ) AS [t]
    WHERE (2 < [t].[row]) AND ([t].[row] <= 5)
) AS [t0] ON [e].[Id] = [t0].[OneId]");
        }

        public override async Task Select_many_over_skip_navigation_of_type(bool async)
        {
            await base.Select_many_over_skip_navigation_of_type(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t].[Number], [t].[IsGreen]
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
INNER JOIN (
    SELECT [e1].[Id], [e1].[Discriminator], [e1].[Name], [e1].[PeriodEnd], [e1].[PeriodStart], [e1].[Number], [e1].[IsGreen], [e0].[ThreeSkipSharedId]
    FROM [EntityRootEntityThree] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    INNER JOIN [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[RootSkipSharedId] = [e1].[Id]
    WHERE [e1].[Discriminator] IN (N'EntityBranch', N'EntityLeaf')
) AS [t] ON [e].[Id] = [t].[ThreeSkipSharedId]");
        }

        public override async Task Select_many_over_skip_navigation_cast(bool async)
        {
            await base.Select_many_over_skip_navigation_cast(async);

            AssertSql(
                @"SELECT [t0].[Id], [t0].[Discriminator], [t0].[Name], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Number], [t0].[IsGreen]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
INNER JOIN (
    SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t].[Number], [t].[IsGreen], [j].[EntityOneId]
    FROM [JoinOneToBranch] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN (
        SELECT [e0].[Id], [e0].[Discriminator], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [e0].[Number], [e0].[IsGreen]
        FROM [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
        WHERE [e0].[Discriminator] IN (N'EntityBranch', N'EntityLeaf')
    ) AS [t] ON [j].[EntityBranchId] = [t].[Id]
) AS [t0] ON [e].[Id] = [t0].[EntityOneId]");
        }

        public override async Task Select_skip_navigation(bool async)
        {
            await base.Select_skip_navigation(async);

            AssertSql(
                @"SELECT [e].[Id], [t].[Id], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t].[LeftId], [t].[RightId]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [j].[LeftId], [j].[RightId]
    FROM [JoinOneSelfPayload] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[LeftId] = [e0].[Id]
) AS [t] ON [e].[Id] = [t].[RightId]
ORDER BY [e].[Id], [t].[LeftId], [t].[RightId]");
        }

        public override async Task Select_skip_navigation_multiple(bool async)
        {
            await base.Select_skip_navigation_multiple(async);

            AssertSql(
                @"SELECT [e].[Id], [t].[Id], [t].[CollectionInverseId], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t].[ReferenceInverseId], [t].[ThreeId], [t].[TwoId], [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[ReferenceInverseId], [t0].[SelfSkipSharedLeftId], [t0].[SelfSkipSharedRightId], [t1].[Key1], [t1].[Key2], [t1].[Key3], [t1].[Name], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[TwoSkipSharedId], [t1].[CompositeKeySkipSharedKey1], [t1].[CompositeKeySkipSharedKey2], [t1].[CompositeKeySkipSharedKey3]
FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [e0].[ReferenceInverseId], [j].[ThreeId], [j].[TwoId]
    FROM [JoinTwoToThree] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[ThreeId] = [e0].[Id]
) AS [t] ON [e].[Id] = [t].[TwoId]
LEFT JOIN (
    SELECT [e2].[Id], [e2].[CollectionInverseId], [e2].[ExtraId], [e2].[Name], [e2].[PeriodEnd], [e2].[PeriodStart], [e2].[ReferenceInverseId], [e1].[SelfSkipSharedLeftId], [e1].[SelfSkipSharedRightId]
    FROM [EntityTwoEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1]
    INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e2] ON [e1].[SelfSkipSharedLeftId] = [e2].[Id]
) AS [t0] ON [e].[Id] = [t0].[SelfSkipSharedRightId]
LEFT JOIN (
    SELECT [e4].[Key1], [e4].[Key2], [e4].[Key3], [e4].[Name], [e4].[PeriodEnd], [e4].[PeriodStart], [e3].[TwoSkipSharedId], [e3].[CompositeKeySkipSharedKey1], [e3].[CompositeKeySkipSharedKey2], [e3].[CompositeKeySkipSharedKey3]
    FROM [EntityCompositeKeyEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e3]
    INNER JOIN [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e4] ON (([e3].[CompositeKeySkipSharedKey1] = [e4].[Key1]) AND ([e3].[CompositeKeySkipSharedKey2] = [e4].[Key2])) AND ([e3].[CompositeKeySkipSharedKey3] = [e4].[Key3])
) AS [t1] ON [e].[Id] = [t1].[TwoSkipSharedId]
ORDER BY [e].[Id], [t].[ThreeId], [t].[TwoId], [t].[Id], [t0].[SelfSkipSharedLeftId], [t0].[SelfSkipSharedRightId], [t0].[Id], [t1].[TwoSkipSharedId], [t1].[CompositeKeySkipSharedKey1], [t1].[CompositeKeySkipSharedKey2], [t1].[CompositeKeySkipSharedKey3], [t1].[Key1], [t1].[Key2]");
        }

        public override async Task Select_skip_navigation_first_or_default(bool async)
        {
            await base.Select_skip_navigation_first_or_default(async);

            AssertSql(
                @"SELECT [t0].[Key1], [t0].[Key2], [t0].[Key3], [t0].[Name], [t0].[PeriodEnd], [t0].[PeriodStart]
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [t].[Key1], [t].[Key2], [t].[Key3], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t].[ThreeId]
    FROM (
        SELECT [e0].[Key1], [e0].[Key2], [e0].[Key3], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [j].[ThreeId], ROW_NUMBER() OVER(PARTITION BY [j].[ThreeId] ORDER BY [e0].[Key1], [e0].[Key2]) AS [row]
        FROM [JoinThreeToCompositeKeyFull] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
        INNER JOIN [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON (([j].[CompositeId1] = [e0].[Key1]) AND ([j].[CompositeId2] = [e0].[Key2])) AND ([j].[CompositeId3] = [e0].[Key3])
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [e].[Id] = [t0].[ThreeId]
ORDER BY [e].[Id]");
        }

        public override async Task Include_skip_navigation(bool async)
        {
            await base.Include_skip_navigation(async);

            AssertSql(
                @"SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [t].[RootSkipSharedId], [t].[CompositeKeySkipSharedKey1], [t].[CompositeKeySkipSharedKey2], [t].[CompositeKeySkipSharedKey3], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id], [t].[Discriminator], [t].[Name], [t].[PeriodEnd0], [t].[PeriodStart0], [t].[Number], [t].[IsGreen]
FROM [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [e0].[RootSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], [e0].[PeriodEnd], [e0].[PeriodStart], [e1].[Id], [e1].[Discriminator], [e1].[Name], [e1].[PeriodEnd] AS [PeriodEnd0], [e1].[PeriodStart] AS [PeriodStart0], [e1].[Number], [e1].[IsGreen]
    FROM [EntityCompositeKeyEntityRoot] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    INNER JOIN [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[RootSkipSharedId] = [e1].[Id]
) AS [t] ON (([e].[Key1] = [t].[CompositeKeySkipSharedKey1]) AND ([e].[Key2] = [t].[CompositeKeySkipSharedKey2])) AND ([e].[Key3] = [t].[CompositeKeySkipSharedKey3])
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [t].[RootSkipSharedId], [t].[CompositeKeySkipSharedKey1], [t].[CompositeKeySkipSharedKey2], [t].[CompositeKeySkipSharedKey3]");
        }

        public override async Task Include_skip_navigation_then_reference(bool async)
        {
            await base.Include_skip_navigation_then_reference(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId], [t].[OneId], [t].[TwoId], [t].[JoinOneToTwoExtraId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id], [t].[Name], [t].[PeriodEnd0], [t].[PeriodStart0], [t].[Id0], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name0], [t].[PeriodEnd1], [t].[PeriodStart1], [t].[ReferenceInverseId]
FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [j].[PeriodEnd], [j].[PeriodStart], [e0].[Id], [e0].[Name], [e0].[PeriodEnd] AS [PeriodEnd0], [e0].[PeriodStart] AS [PeriodStart0], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[PeriodEnd] AS [PeriodEnd1], [e1].[PeriodStart] AS [PeriodStart1], [e1].[ReferenceInverseId]
    FROM [JoinOneToTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
) AS [t] ON [e].[Id] = [t].[TwoId]
ORDER BY [e].[Id], [t].[OneId], [t].[TwoId], [t].[Id]");
        }

        public override async Task Include_skip_navigation_then_include_skip_navigation(bool async)
        {
            await base.Include_skip_navigation_then_include_skip_navigation(async);

            AssertSql(
                @"SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [t1].[LeafId], [t1].[CompositeId1], [t1].[CompositeId2], [t1].[CompositeId3], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Id], [t1].[Discriminator], [t1].[Name], [t1].[PeriodEnd0], [t1].[PeriodStart0], [t1].[Number], [t1].[IsGreen], [t1].[EntityBranchId], [t1].[EntityOneId], [t1].[PeriodEnd1], [t1].[PeriodStart1], [t1].[Id0], [t1].[Name0], [t1].[PeriodEnd00], [t1].[PeriodStart00]
FROM [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [j].[PeriodEnd], [j].[PeriodStart], [t].[Id], [t].[Discriminator], [t].[Name], [t].[PeriodEnd] AS [PeriodEnd0], [t].[PeriodStart] AS [PeriodStart0], [t].[Number], [t].[IsGreen], [t0].[EntityBranchId], [t0].[EntityOneId], [t0].[PeriodEnd] AS [PeriodEnd1], [t0].[PeriodStart] AS [PeriodStart1], [t0].[Id] AS [Id0], [t0].[Name] AS [Name0], [t0].[PeriodEnd0] AS [PeriodEnd00], [t0].[PeriodStart0] AS [PeriodStart00]
    FROM [JoinCompositeKeyToLeaf] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN (
        SELECT [e0].[Id], [e0].[Discriminator], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [e0].[Number], [e0].[IsGreen]
        FROM [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
        WHERE [e0].[Discriminator] = N'EntityLeaf'
    ) AS [t] ON [j].[LeafId] = [t].[Id]
    LEFT JOIN (
        SELECT [j0].[EntityBranchId], [j0].[EntityOneId], [j0].[PeriodEnd], [j0].[PeriodStart], [e1].[Id], [e1].[Name], [e1].[PeriodEnd] AS [PeriodEnd0], [e1].[PeriodStart] AS [PeriodStart0]
        FROM [JoinOneToBranch] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j0]
        INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [j0].[EntityOneId] = [e1].[Id]
    ) AS [t0] ON [t].[Id] = [t0].[EntityBranchId]
) AS [t1] ON (([e].[Key1] = [t1].[CompositeId1]) AND ([e].[Key2] = [t1].[CompositeId2])) AND ([e].[Key3] = [t1].[CompositeId3])
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [t1].[LeafId], [t1].[CompositeId1], [t1].[CompositeId2], [t1].[CompositeId3], [t1].[Id], [t1].[EntityBranchId], [t1].[EntityOneId]");
        }

        public override async Task Include_skip_navigation_then_include_reference_and_skip_navigation(bool async)
        {
            await base.Include_skip_navigation_then_include_reference_and_skip_navigation(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId], [t0].[OneId], [t0].[ThreeId], [t0].[Payload], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id], [t0].[Name], [t0].[PeriodEnd0], [t0].[PeriodStart0], [t0].[Id0], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name0], [t0].[PeriodEnd1], [t0].[PeriodStart1], [t0].[ReferenceInverseId], [t0].[LeftId], [t0].[RightId], [t0].[Payload0], [t0].[PeriodEnd2], [t0].[PeriodStart2], [t0].[Id1], [t0].[Name1], [t0].[PeriodEnd00], [t0].[PeriodStart00]
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [j].[PeriodEnd], [j].[PeriodStart], [e0].[Id], [e0].[Name], [e0].[PeriodEnd] AS [PeriodEnd0], [e0].[PeriodStart] AS [PeriodStart0], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[PeriodEnd] AS [PeriodEnd1], [e1].[PeriodStart] AS [PeriodStart1], [e1].[ReferenceInverseId], [t].[LeftId], [t].[RightId], [t].[Payload] AS [Payload0], [t].[PeriodEnd] AS [PeriodEnd2], [t].[PeriodStart] AS [PeriodStart2], [t].[Id] AS [Id1], [t].[Name] AS [Name1], [t].[PeriodEnd0] AS [PeriodEnd00], [t].[PeriodStart0] AS [PeriodStart00]
    FROM [JoinOneToThreePayloadFull] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
    LEFT JOIN (
        SELECT [j0].[LeftId], [j0].[RightId], [j0].[Payload], [j0].[PeriodEnd], [j0].[PeriodStart], [e2].[Id], [e2].[Name], [e2].[PeriodEnd] AS [PeriodEnd0], [e2].[PeriodStart] AS [PeriodStart0]
        FROM [JoinOneSelfPayload] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j0]
        INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e2] ON [j0].[RightId] = [e2].[Id]
    ) AS [t] ON [e0].[Id] = [t].[LeftId]
) AS [t0] ON [e].[Id] = [t0].[ThreeId]
ORDER BY [e].[Id], [t0].[OneId], [t0].[ThreeId], [t0].[Id], [t0].[Id0], [t0].[LeftId], [t0].[RightId]");
        }

        public override async Task Include_skip_navigation_and_reference(bool async)
        {
            await base.Include_skip_navigation_and_reference(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId], [e0].[Id], [t].[OneSkipSharedId], [t].[TwoSkipSharedId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id], [t].[Name], [t].[PeriodEnd0], [t].[PeriodStart0], [e0].[CollectionInverseId], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [e0].[ReferenceInverseId]
FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [e].[Id] = [e0].[ReferenceInverseId]
LEFT JOIN (
    SELECT [e1].[OneSkipSharedId], [e1].[TwoSkipSharedId], [e1].[PeriodEnd], [e1].[PeriodStart], [e2].[Id], [e2].[Name], [e2].[PeriodEnd] AS [PeriodEnd0], [e2].[PeriodStart] AS [PeriodStart0]
    FROM [EntityOneEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e2] ON [e1].[OneSkipSharedId] = [e2].[Id]
) AS [t] ON [e].[Id] = [t].[TwoSkipSharedId]
ORDER BY [e].[Id], [e0].[Id], [t].[OneSkipSharedId], [t].[TwoSkipSharedId]");
        }

        public override async Task Include_skip_navigation_then_include_inverse_works_for_tracking_query(bool async)
        {
            await base.Include_skip_navigation_then_include_inverse_works_for_tracking_query(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId], [t0].[OneId], [t0].[ThreeId], [t0].[Payload], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id], [t0].[Name], [t0].[PeriodEnd0], [t0].[PeriodStart0], [t0].[OneId0], [t0].[ThreeId0], [t0].[Payload0], [t0].[PeriodEnd1], [t0].[PeriodStart1], [t0].[Id0], [t0].[CollectionInverseId], [t0].[Name0], [t0].[PeriodEnd00], [t0].[PeriodStart00], [t0].[ReferenceInverseId]
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [j].[PeriodEnd], [j].[PeriodStart], [e0].[Id], [e0].[Name], [e0].[PeriodEnd] AS [PeriodEnd0], [e0].[PeriodStart] AS [PeriodStart0], [t].[OneId] AS [OneId0], [t].[ThreeId] AS [ThreeId0], [t].[Payload] AS [Payload0], [t].[PeriodEnd] AS [PeriodEnd1], [t].[PeriodStart] AS [PeriodStart1], [t].[Id] AS [Id0], [t].[CollectionInverseId], [t].[Name] AS [Name0], [t].[PeriodEnd0] AS [PeriodEnd00], [t].[PeriodStart0] AS [PeriodStart00], [t].[ReferenceInverseId]
    FROM [JoinOneToThreePayloadFullShared] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN (
        SELECT [j0].[OneId], [j0].[ThreeId], [j0].[Payload], [j0].[PeriodEnd], [j0].[PeriodStart], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[PeriodEnd] AS [PeriodEnd0], [e1].[PeriodStart] AS [PeriodStart0], [e1].[ReferenceInverseId]
        FROM [JoinOneToThreePayloadFullShared] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j0]
        INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [j0].[ThreeId] = [e1].[Id]
    ) AS [t] ON [e0].[Id] = [t].[OneId]
) AS [t0] ON [e].[Id] = [t0].[ThreeId]
ORDER BY [e].[Id], [t0].[OneId], [t0].[ThreeId], [t0].[Id], [t0].[OneId0], [t0].[ThreeId0]");
        }

        public override async Task Filtered_include_skip_navigation_where(bool async)
        {
            await base.Filtered_include_skip_navigation_where(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId], [t].[OneId], [t].[ThreeId], [t].[Payload], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id], [t].[Name], [t].[PeriodEnd0], [t].[PeriodStart0]
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [j].[PeriodEnd], [j].[PeriodStart], [e0].[Id], [e0].[Name], [e0].[PeriodEnd] AS [PeriodEnd0], [e0].[PeriodStart] AS [PeriodStart0]
    FROM [JoinOneToThreePayloadFullShared] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[OneId] = [e0].[Id]
    WHERE [e0].[Id] < 10
) AS [t] ON [e].[Id] = [t].[ThreeId]
ORDER BY [e].[Id], [t].[OneId], [t].[ThreeId]");
        }

        public override async Task Filtered_include_skip_navigation_order_by(bool async)
        {
            await base.Filtered_include_skip_navigation_order_by(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId], [t].[ThreeId], [t].[TwoId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[PeriodEnd0], [t].[PeriodStart0], [t].[ReferenceInverseId]
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [j].[ThreeId], [j].[TwoId], [j].[PeriodEnd], [j].[PeriodStart], [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[PeriodEnd] AS [PeriodEnd0], [e0].[PeriodStart] AS [PeriodStart0], [e0].[ReferenceInverseId]
    FROM [JoinTwoToThree] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[TwoId] = [e0].[Id]
) AS [t] ON [e].[Id] = [t].[ThreeId]
ORDER BY [e].[Id], [t].[Id], [t].[ThreeId]");
        }

        public override async Task Filtered_include_skip_navigation_order_by_skip(bool async)
        {
            await base.Filtered_include_skip_navigation_order_by_skip(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId], [t0].[SelfSkipSharedLeftId], [t0].[SelfSkipSharedRightId], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[PeriodEnd0], [t0].[PeriodStart0], [t0].[ReferenceInverseId]
FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [t].[SelfSkipSharedLeftId], [t].[SelfSkipSharedRightId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[PeriodEnd0], [t].[PeriodStart0], [t].[ReferenceInverseId]
    FROM (
        SELECT [e0].[SelfSkipSharedLeftId], [e0].[SelfSkipSharedRightId], [e0].[PeriodEnd], [e0].[PeriodStart], [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[PeriodEnd] AS [PeriodEnd0], [e1].[PeriodStart] AS [PeriodStart0], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [e0].[SelfSkipSharedLeftId] ORDER BY [e1].[Id]) AS [row]
        FROM [EntityTwoEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
        INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[SelfSkipSharedRightId] = [e1].[Id]
    ) AS [t]
    WHERE 2 < [t].[row]
) AS [t0] ON [e].[Id] = [t0].[SelfSkipSharedLeftId]
ORDER BY [e].[Id], [t0].[SelfSkipSharedLeftId], [t0].[Id]");
        }

        public override async Task Filtered_include_skip_navigation_order_by_take(bool async)
        {
            await base.Filtered_include_skip_navigation_order_by_take(async);

            AssertSql(
                @"SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [t0].[TwoSkipSharedId], [t0].[CompositeKeySkipSharedKey1], [t0].[CompositeKeySkipSharedKey2], [t0].[CompositeKeySkipSharedKey3], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[PeriodEnd0], [t0].[PeriodStart0], [t0].[ReferenceInverseId]
FROM [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [t].[TwoSkipSharedId], [t].[CompositeKeySkipSharedKey1], [t].[CompositeKeySkipSharedKey2], [t].[CompositeKeySkipSharedKey3], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[PeriodEnd0], [t].[PeriodStart0], [t].[ReferenceInverseId]
    FROM (
        SELECT [e0].[TwoSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], [e0].[PeriodEnd], [e0].[PeriodStart], [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[PeriodEnd] AS [PeriodEnd0], [e1].[PeriodStart] AS [PeriodStart0], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3] ORDER BY [e1].[Id]) AS [row]
        FROM [EntityCompositeKeyEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
        INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
    ) AS [t]
    WHERE [t].[row] <= 2
) AS [t0] ON (([e].[Key1] = [t0].[CompositeKeySkipSharedKey1]) AND ([e].[Key2] = [t0].[CompositeKeySkipSharedKey2])) AND ([e].[Key3] = [t0].[CompositeKeySkipSharedKey3])
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [t0].[CompositeKeySkipSharedKey1], [t0].[CompositeKeySkipSharedKey2], [t0].[CompositeKeySkipSharedKey3], [t0].[Id]");
        }

        public override async Task Filtered_include_skip_navigation_order_by_skip_take(bool async)
        {
            await base.Filtered_include_skip_navigation_order_by_skip_take(async);

            AssertSql(
                @"SELECT [e].[Key1], [e].[Key2], [e].[Key3], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [t0].[Id], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[ThreeId], [t0].[Id0], [t0].[CollectionInverseId], [t0].[Name], [t0].[PeriodEnd0], [t0].[PeriodStart0], [t0].[ReferenceInverseId]
FROM [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [t].[Id], [t].[CompositeId1], [t].[CompositeId2], [t].[CompositeId3], [t].[PeriodEnd], [t].[PeriodStart], [t].[ThreeId], [t].[Id0], [t].[CollectionInverseId], [t].[Name], [t].[PeriodEnd0], [t].[PeriodStart0], [t].[ReferenceInverseId]
    FROM (
        SELECT [j].[Id], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [j].[PeriodEnd], [j].[PeriodStart], [j].[ThreeId], [e0].[Id] AS [Id0], [e0].[CollectionInverseId], [e0].[Name], [e0].[PeriodEnd] AS [PeriodEnd0], [e0].[PeriodStart] AS [PeriodStart0], [e0].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3] ORDER BY [e0].[Id]) AS [row]
        FROM [JoinThreeToCompositeKeyFull] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
        INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[ThreeId] = [e0].[Id]
    ) AS [t]
    WHERE (1 < [t].[row]) AND ([t].[row] <= 3)
) AS [t0] ON (([e].[Key1] = [t0].[CompositeId1]) AND ([e].[Key2] = [t0].[CompositeId2])) AND ([e].[Key3] = [t0].[CompositeId3])
ORDER BY [e].[Key1], [e].[Key2], [e].[Key3], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[Id0]");
        }

        public override async Task Filtered_then_include_skip_navigation_where(bool async)
        {
            await base.Filtered_then_include_skip_navigation_where(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[Number], [e].[IsGreen], [t0].[RootSkipSharedId], [t0].[ThreeSkipSharedId], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id], [t0].[CollectionInverseId], [t0].[Name], [t0].[PeriodEnd0], [t0].[PeriodStart0], [t0].[ReferenceInverseId], [t0].[OneId], [t0].[ThreeId], [t0].[Payload], [t0].[PeriodEnd1], [t0].[PeriodStart1], [t0].[Id0], [t0].[Name0], [t0].[PeriodEnd00], [t0].[PeriodStart00]
FROM [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [e0].[RootSkipSharedId], [e0].[ThreeSkipSharedId], [e0].[PeriodEnd], [e0].[PeriodStart], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[PeriodEnd] AS [PeriodEnd0], [e1].[PeriodStart] AS [PeriodStart0], [e1].[ReferenceInverseId], [t].[OneId], [t].[ThreeId], [t].[Payload], [t].[PeriodEnd] AS [PeriodEnd1], [t].[PeriodStart] AS [PeriodStart1], [t].[Id] AS [Id0], [t].[Name] AS [Name0], [t].[PeriodEnd0] AS [PeriodEnd00], [t].[PeriodStart0] AS [PeriodStart00]
    FROM [EntityRootEntityThree] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[ThreeSkipSharedId] = [e1].[Id]
    LEFT JOIN (
        SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [j].[PeriodEnd], [j].[PeriodStart], [e2].[Id], [e2].[Name], [e2].[PeriodEnd] AS [PeriodEnd0], [e2].[PeriodStart] AS [PeriodStart0]
        FROM [JoinOneToThreePayloadFullShared] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
        INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e2] ON [j].[OneId] = [e2].[Id]
        WHERE [e2].[Id] < 10
    ) AS [t] ON [e1].[Id] = [t].[ThreeId]
) AS [t0] ON [e].[Id] = [t0].[RootSkipSharedId]
ORDER BY [e].[Id], [t0].[RootSkipSharedId], [t0].[ThreeSkipSharedId], [t0].[Id], [t0].[OneId], [t0].[ThreeId]");
        }

        public override async Task Filtered_then_include_skip_navigation_order_by_skip_take(bool async)
        {
            await base.Filtered_then_include_skip_navigation_order_by_skip_take(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[Number], [e].[IsGreen], [t1].[RootSkipSharedId], [t1].[CompositeKeySkipSharedKey1], [t1].[CompositeKeySkipSharedKey2], [t1].[CompositeKeySkipSharedKey3], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Key1], [t1].[Key2], [t1].[Key3], [t1].[Name], [t1].[PeriodEnd0], [t1].[PeriodStart0], [t1].[Id], [t1].[CompositeId1], [t1].[CompositeId2], [t1].[CompositeId3], [t1].[PeriodEnd1], [t1].[PeriodStart1], [t1].[ThreeId], [t1].[Id0], [t1].[CollectionInverseId], [t1].[Name0], [t1].[PeriodEnd00], [t1].[PeriodStart00], [t1].[ReferenceInverseId]
FROM [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [e0].[RootSkipSharedId], [e0].[CompositeKeySkipSharedKey1], [e0].[CompositeKeySkipSharedKey2], [e0].[CompositeKeySkipSharedKey3], [e0].[PeriodEnd], [e0].[PeriodStart], [e1].[Key1], [e1].[Key2], [e1].[Key3], [e1].[Name], [e1].[PeriodEnd] AS [PeriodEnd0], [e1].[PeriodStart] AS [PeriodStart0], [t0].[Id], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[PeriodEnd] AS [PeriodEnd1], [t0].[PeriodStart] AS [PeriodStart1], [t0].[ThreeId], [t0].[Id0], [t0].[CollectionInverseId], [t0].[Name] AS [Name0], [t0].[PeriodEnd0] AS [PeriodEnd00], [t0].[PeriodStart0] AS [PeriodStart00], [t0].[ReferenceInverseId]
    FROM [EntityCompositeKeyEntityRoot] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    INNER JOIN [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON (([e0].[CompositeKeySkipSharedKey1] = [e1].[Key1]) AND ([e0].[CompositeKeySkipSharedKey2] = [e1].[Key2])) AND ([e0].[CompositeKeySkipSharedKey3] = [e1].[Key3])
    LEFT JOIN (
        SELECT [t].[Id], [t].[CompositeId1], [t].[CompositeId2], [t].[CompositeId3], [t].[PeriodEnd], [t].[PeriodStart], [t].[ThreeId], [t].[Id0], [t].[CollectionInverseId], [t].[Name], [t].[PeriodEnd0], [t].[PeriodStart0], [t].[ReferenceInverseId]
        FROM (
            SELECT [j].[Id], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [j].[PeriodEnd], [j].[PeriodStart], [j].[ThreeId], [e2].[Id] AS [Id0], [e2].[CollectionInverseId], [e2].[Name], [e2].[PeriodEnd] AS [PeriodEnd0], [e2].[PeriodStart] AS [PeriodStart0], [e2].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3] ORDER BY [e2].[Id]) AS [row]
            FROM [JoinThreeToCompositeKeyFull] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
            INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e2] ON [j].[ThreeId] = [e2].[Id]
        ) AS [t]
        WHERE (1 < [t].[row]) AND ([t].[row] <= 3)
    ) AS [t0] ON (([e1].[Key1] = [t0].[CompositeId1]) AND ([e1].[Key2] = [t0].[CompositeId2])) AND ([e1].[Key3] = [t0].[CompositeId3])
) AS [t1] ON [e].[Id] = [t1].[RootSkipSharedId]
ORDER BY [e].[Id], [t1].[RootSkipSharedId], [t1].[CompositeKeySkipSharedKey1], [t1].[CompositeKeySkipSharedKey2], [t1].[CompositeKeySkipSharedKey3], [t1].[Key1], [t1].[Key2], [t1].[Key3], [t1].[CompositeId1], [t1].[CompositeId2], [t1].[CompositeId3], [t1].[Id0]");
        }

        public override async Task Filtered_include_skip_navigation_where_then_include_skip_navigation(bool async)
        {
            await base.Filtered_include_skip_navigation_where_then_include_skip_navigation(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[Number], [e].[IsGreen], [t0].[LeafId], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Key1], [t0].[Key2], [t0].[Key3], [t0].[Name], [t0].[PeriodEnd0], [t0].[PeriodStart0], [t0].[TwoSkipSharedId], [t0].[CompositeKeySkipSharedKey1], [t0].[CompositeKeySkipSharedKey2], [t0].[CompositeKeySkipSharedKey3], [t0].[PeriodEnd1], [t0].[PeriodStart1], [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name0], [t0].[PeriodEnd00], [t0].[PeriodStart00], [t0].[ReferenceInverseId]
FROM [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [j].[LeafId], [j].[CompositeId1], [j].[CompositeId2], [j].[CompositeId3], [j].[PeriodEnd], [j].[PeriodStart], [e0].[Key1], [e0].[Key2], [e0].[Key3], [e0].[Name], [e0].[PeriodEnd] AS [PeriodEnd0], [e0].[PeriodStart] AS [PeriodStart0], [t].[TwoSkipSharedId], [t].[CompositeKeySkipSharedKey1], [t].[CompositeKeySkipSharedKey2], [t].[CompositeKeySkipSharedKey3], [t].[PeriodEnd] AS [PeriodEnd1], [t].[PeriodStart] AS [PeriodStart1], [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name] AS [Name0], [t].[PeriodEnd0] AS [PeriodEnd00], [t].[PeriodStart0] AS [PeriodStart00], [t].[ReferenceInverseId]
    FROM [JoinCompositeKeyToLeaf] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityCompositeKeys] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON (([j].[CompositeId1] = [e0].[Key1]) AND ([j].[CompositeId2] = [e0].[Key2])) AND ([j].[CompositeId3] = [e0].[Key3])
    LEFT JOIN (
        SELECT [e1].[TwoSkipSharedId], [e1].[CompositeKeySkipSharedKey1], [e1].[CompositeKeySkipSharedKey2], [e1].[CompositeKeySkipSharedKey3], [e1].[PeriodEnd], [e1].[PeriodStart], [e2].[Id], [e2].[CollectionInverseId], [e2].[ExtraId], [e2].[Name], [e2].[PeriodEnd] AS [PeriodEnd0], [e2].[PeriodStart] AS [PeriodStart0], [e2].[ReferenceInverseId]
        FROM [EntityCompositeKeyEntityTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1]
        INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e2] ON [e1].[TwoSkipSharedId] = [e2].[Id]
    ) AS [t] ON (([e0].[Key1] = [t].[CompositeKeySkipSharedKey1]) AND ([e0].[Key2] = [t].[CompositeKeySkipSharedKey2])) AND ([e0].[Key3] = [t].[CompositeKeySkipSharedKey3])
    WHERE [e0].[Key1] < 5
) AS [t0] ON [e].[Id] = [t0].[LeafId]
WHERE [e].[Discriminator] = N'EntityLeaf'
ORDER BY [e].[Id], [t0].[LeafId], [t0].[CompositeId1], [t0].[CompositeId2], [t0].[CompositeId3], [t0].[Key1], [t0].[Key2], [t0].[Key3], [t0].[TwoSkipSharedId], [t0].[CompositeKeySkipSharedKey1], [t0].[CompositeKeySkipSharedKey2], [t0].[CompositeKeySkipSharedKey3]");
        }

        public override async Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where(bool async)
        {
            await base.Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [t1].[OneId], [t1].[TwoId], [t1].[JoinOneToTwoExtraId], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Id], [t1].[CollectionInverseId], [t1].[ExtraId], [t1].[Name], [t1].[PeriodEnd0], [t1].[PeriodStart0], [t1].[ReferenceInverseId], [t1].[ThreeId], [t1].[TwoId0], [t1].[PeriodEnd1], [t1].[PeriodStart1], [t1].[Id0], [t1].[CollectionInverseId0], [t1].[Name0], [t1].[PeriodEnd00], [t1].[PeriodStart00], [t1].[ReferenceInverseId0]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
OUTER APPLY (
    SELECT [t].[OneId], [t].[TwoId], [t].[JoinOneToTwoExtraId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[PeriodEnd0], [t].[PeriodStart0], [t].[ReferenceInverseId], [t0].[ThreeId], [t0].[TwoId] AS [TwoId0], [t0].[PeriodEnd] AS [PeriodEnd1], [t0].[PeriodStart] AS [PeriodStart1], [t0].[Id] AS [Id0], [t0].[CollectionInverseId] AS [CollectionInverseId0], [t0].[Name] AS [Name0], [t0].[PeriodEnd0] AS [PeriodEnd00], [t0].[PeriodStart0] AS [PeriodStart00], [t0].[ReferenceInverseId] AS [ReferenceInverseId0]
    FROM (
        SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [j].[PeriodEnd], [j].[PeriodStart], [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[PeriodEnd] AS [PeriodEnd0], [e0].[PeriodStart] AS [PeriodStart0], [e0].[ReferenceInverseId]
        FROM [JoinOneToTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
        INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[TwoId] = [e0].[Id]
        WHERE [e].[Id] = [j].[OneId]
        ORDER BY [e0].[Id]
        OFFSET 1 ROWS FETCH NEXT 2 ROWS ONLY
    ) AS [t]
    LEFT JOIN (
        SELECT [j0].[ThreeId], [j0].[TwoId], [j0].[PeriodEnd], [j0].[PeriodStart], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[PeriodEnd] AS [PeriodEnd0], [e1].[PeriodStart] AS [PeriodStart0], [e1].[ReferenceInverseId]
        FROM [JoinTwoToThree] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j0]
        INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [j0].[ThreeId] = [e1].[Id]
        WHERE [e1].[Id] < 10
    ) AS [t0] ON [t].[Id] = [t0].[TwoId]
) AS [t1]
ORDER BY [e].[Id], [t1].[Id], [t1].[OneId], [t1].[TwoId], [t1].[ThreeId], [t1].[TwoId0]");
        }

        public override async Task Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take(bool async)
        {
            await base.Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [t1].[OneId], [t1].[TwoId], [t1].[JoinOneToTwoExtraId], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Id], [t1].[CollectionInverseId], [t1].[ExtraId], [t1].[Name], [t1].[PeriodEnd0], [t1].[PeriodStart0], [t1].[ReferenceInverseId], [t1].[ThreeId], [t1].[TwoId0], [t1].[PeriodEnd1], [t1].[PeriodStart1], [t1].[Id0], [t1].[CollectionInverseId0], [t1].[Name0], [t1].[PeriodEnd00], [t1].[PeriodStart00], [t1].[ReferenceInverseId0]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [j].[PeriodEnd], [j].[PeriodStart], [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[PeriodEnd] AS [PeriodEnd0], [e0].[PeriodStart] AS [PeriodStart0], [e0].[ReferenceInverseId], [t0].[ThreeId], [t0].[TwoId] AS [TwoId0], [t0].[PeriodEnd] AS [PeriodEnd1], [t0].[PeriodStart] AS [PeriodStart1], [t0].[Id] AS [Id0], [t0].[CollectionInverseId] AS [CollectionInverseId0], [t0].[Name] AS [Name0], [t0].[PeriodEnd0] AS [PeriodEnd00], [t0].[PeriodStart0] AS [PeriodStart00], [t0].[ReferenceInverseId] AS [ReferenceInverseId0]
    FROM [JoinOneToTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[TwoId] = [e0].[Id]
    LEFT JOIN (
        SELECT [t].[ThreeId], [t].[TwoId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id], [t].[CollectionInverseId], [t].[Name], [t].[PeriodEnd0], [t].[PeriodStart0], [t].[ReferenceInverseId]
        FROM (
            SELECT [j0].[ThreeId], [j0].[TwoId], [j0].[PeriodEnd], [j0].[PeriodStart], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[PeriodEnd] AS [PeriodEnd0], [e1].[PeriodStart] AS [PeriodStart0], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j0].[TwoId] ORDER BY [e1].[Id]) AS [row]
            FROM [JoinTwoToThree] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j0]
            INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [j0].[ThreeId] = [e1].[Id]
        ) AS [t]
        WHERE (1 < [t].[row]) AND ([t].[row] <= 3)
    ) AS [t0] ON [e0].[Id] = [t0].[TwoId]
    WHERE [e0].[Id] < 10
) AS [t1] ON [e].[Id] = [t1].[OneId]
ORDER BY [e].[Id], [t1].[OneId], [t1].[TwoId], [t1].[Id], [t1].[TwoId0], [t1].[Id0]");
        }

        public override async Task Filter_include_on_skip_navigation_combined(bool async)
        {
            await base.Filter_include_on_skip_navigation_combined(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[ExtraId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId], [t].[OneId], [t].[TwoId], [t].[JoinOneToTwoExtraId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id], [t].[Name], [t].[PeriodEnd0], [t].[PeriodStart0], [t].[Id0], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name0], [t].[PeriodEnd1], [t].[PeriodStart1], [t].[ReferenceInverseId], [t].[Id1], [t].[CollectionInverseId0], [t].[ExtraId0], [t].[Name1], [t].[PeriodEnd2], [t].[PeriodStart2], [t].[ReferenceInverseId0]
FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[TwoId], [j].[JoinOneToTwoExtraId], [j].[PeriodEnd], [j].[PeriodStart], [e0].[Id], [e0].[Name], [e0].[PeriodEnd] AS [PeriodEnd0], [e0].[PeriodStart] AS [PeriodStart0], [e1].[Id] AS [Id0], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name] AS [Name0], [e1].[PeriodEnd] AS [PeriodEnd1], [e1].[PeriodStart] AS [PeriodStart1], [e1].[ReferenceInverseId], [e2].[Id] AS [Id1], [e2].[CollectionInverseId] AS [CollectionInverseId0], [e2].[ExtraId] AS [ExtraId0], [e2].[Name] AS [Name1], [e2].[PeriodEnd] AS [PeriodEnd2], [e2].[PeriodStart] AS [PeriodStart2], [e2].[ReferenceInverseId] AS [ReferenceInverseId0]
    FROM [JoinOneToTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [e0].[Id] = [e1].[ReferenceInverseId]
    LEFT JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e2] ON [e0].[Id] = [e2].[CollectionInverseId]
    WHERE [e0].[Id] < 10
) AS [t] ON [e].[Id] = [t].[TwoId]
ORDER BY [e].[Id], [t].[OneId], [t].[TwoId], [t].[Id], [t].[Id0]");
        }

        public override async Task Filter_include_on_skip_navigation_combined_with_filtered_then_includes(bool async)
        {
            await base.Filter_include_on_skip_navigation_combined_with_filtered_then_includes(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId], [t3].[OneId], [t3].[ThreeId], [t3].[Payload], [t3].[PeriodEnd], [t3].[PeriodStart], [t3].[Id], [t3].[Name], [t3].[PeriodEnd0], [t3].[PeriodStart0], [t3].[OneId0], [t3].[TwoId], [t3].[JoinOneToTwoExtraId], [t3].[PeriodEnd1], [t3].[PeriodStart1], [t3].[Id0], [t3].[CollectionInverseId], [t3].[ExtraId], [t3].[Name0], [t3].[PeriodEnd00], [t3].[PeriodStart00], [t3].[ReferenceInverseId], [t3].[EntityBranchId], [t3].[EntityOneId], [t3].[PeriodEnd2], [t3].[PeriodStart2], [t3].[Id1], [t3].[Discriminator], [t3].[Name1], [t3].[PeriodEnd01], [t3].[PeriodStart01], [t3].[Number], [t3].[IsGreen]
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [j].[PeriodEnd], [j].[PeriodStart], [e0].[Id], [e0].[Name], [e0].[PeriodEnd] AS [PeriodEnd0], [e0].[PeriodStart] AS [PeriodStart0], [t0].[OneId] AS [OneId0], [t0].[TwoId], [t0].[JoinOneToTwoExtraId], [t0].[PeriodEnd] AS [PeriodEnd1], [t0].[PeriodStart] AS [PeriodStart1], [t0].[Id] AS [Id0], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name] AS [Name0], [t0].[PeriodEnd0] AS [PeriodEnd00], [t0].[PeriodStart0] AS [PeriodStart00], [t0].[ReferenceInverseId], [t1].[EntityBranchId], [t1].[EntityOneId], [t1].[PeriodEnd] AS [PeriodEnd2], [t1].[PeriodStart] AS [PeriodStart2], [t1].[Id] AS [Id1], [t1].[Discriminator], [t1].[Name] AS [Name1], [t1].[PeriodEnd0] AS [PeriodEnd01], [t1].[PeriodStart0] AS [PeriodStart01], [t1].[Number], [t1].[IsGreen]
    FROM [JoinOneToThreePayloadFull] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN (
        SELECT [t].[OneId], [t].[TwoId], [t].[JoinOneToTwoExtraId], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name], [t].[PeriodEnd0], [t].[PeriodStart0], [t].[ReferenceInverseId]
        FROM (
            SELECT [j0].[OneId], [j0].[TwoId], [j0].[JoinOneToTwoExtraId], [j0].[PeriodEnd], [j0].[PeriodStart], [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[PeriodEnd] AS [PeriodEnd0], [e1].[PeriodStart] AS [PeriodStart0], [e1].[ReferenceInverseId], ROW_NUMBER() OVER(PARTITION BY [j0].[OneId] ORDER BY [e1].[Id]) AS [row]
            FROM [JoinOneToTwo] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j0]
            INNER JOIN [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [j0].[TwoId] = [e1].[Id]
        ) AS [t]
        WHERE (1 < [t].[row]) AND ([t].[row] <= 3)
    ) AS [t0] ON [e0].[Id] = [t0].[OneId]
    LEFT JOIN (
        SELECT [j1].[EntityBranchId], [j1].[EntityOneId], [j1].[PeriodEnd], [j1].[PeriodStart], [t2].[Id], [t2].[Discriminator], [t2].[Name], [t2].[PeriodEnd] AS [PeriodEnd0], [t2].[PeriodStart] AS [PeriodStart0], [t2].[Number], [t2].[IsGreen]
        FROM [JoinOneToBranch] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j1]
        INNER JOIN (
            SELECT [e2].[Id], [e2].[Discriminator], [e2].[Name], [e2].[PeriodEnd], [e2].[PeriodStart], [e2].[Number], [e2].[IsGreen]
            FROM [EntityRoots] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e2]
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
                @"SELECT [e].[Id], [e].[CollectionInverseId], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[ReferenceInverseId], [t0].[OneId], [t0].[ThreeId], [t0].[Payload], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id], [t0].[Name], [t0].[PeriodEnd0], [t0].[PeriodStart0], [t0].[Id0], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name0], [t0].[PeriodEnd1], [t0].[PeriodStart1], [t0].[ReferenceInverseId]
FROM [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [j].[OneId], [j].[ThreeId], [j].[Payload], [j].[PeriodEnd], [j].[PeriodStart], [e0].[Id], [e0].[Name], [e0].[PeriodEnd] AS [PeriodEnd0], [e0].[PeriodStart] AS [PeriodStart0], [t].[Id] AS [Id0], [t].[CollectionInverseId], [t].[ExtraId], [t].[Name] AS [Name0], [t].[PeriodEnd] AS [PeriodEnd1], [t].[PeriodStart] AS [PeriodStart1], [t].[ReferenceInverseId]
    FROM [JoinOneToThreePayloadFull] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
    INNER JOIN [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0] ON [j].[OneId] = [e0].[Id]
    LEFT JOIN (
        SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[ExtraId], [e1].[Name], [e1].[PeriodEnd], [e1].[PeriodStart], [e1].[ReferenceInverseId]
        FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1]
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
                @"SELECT [e].[Id], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [t0].[Id], [t0].[CollectionInverseId], [t0].[ExtraId], [t0].[Name], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[ReferenceInverseId], [t0].[ThreeId], [t0].[TwoId], [t0].[PeriodEnd0], [t0].[PeriodStart0], [t0].[Id0], [t0].[CollectionInverseId0], [t0].[Name0], [t0].[PeriodEnd00], [t0].[PeriodStart00], [t0].[ReferenceInverseId0]
FROM [EntityOnes] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[ExtraId], [e0].[Name], [e0].[PeriodEnd], [e0].[PeriodStart], [e0].[ReferenceInverseId], [t].[ThreeId], [t].[TwoId], [t].[PeriodEnd] AS [PeriodEnd0], [t].[PeriodStart] AS [PeriodStart0], [t].[Id] AS [Id0], [t].[CollectionInverseId] AS [CollectionInverseId0], [t].[Name] AS [Name0], [t].[PeriodEnd0] AS [PeriodEnd00], [t].[PeriodStart0] AS [PeriodStart00], [t].[ReferenceInverseId] AS [ReferenceInverseId0]
    FROM [EntityTwos] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e0]
    LEFT JOIN (
        SELECT [j].[ThreeId], [j].[TwoId], [j].[PeriodEnd], [j].[PeriodStart], [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[PeriodEnd] AS [PeriodEnd0], [e1].[PeriodStart] AS [PeriodStart0], [e1].[ReferenceInverseId]
        FROM [JoinTwoToThree] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [j]
        INNER JOIN [EntityThrees] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e1] ON [j].[ThreeId] = [e1].[Id]
        WHERE [e1].[Id] < 5
    ) AS [t] ON [e0].[Id] = [t].[TwoId]
    WHERE [e0].[Id] > 15
) AS [t0] ON [e].[Id] = [t0].[CollectionInverseId]
ORDER BY [e].[Id], [t0].[Id], [t0].[ThreeId], [t0].[TwoId]");
        }

        public override async Task Includes_accessed_via_different_path_are_merged(bool async)
        {
            await base.Includes_accessed_via_different_path_are_merged(async);

            AssertSql(
                @"");
        }

        public override async Task Filered_includes_accessed_via_different_path_are_merged(bool async)
        {
            await base.Filered_includes_accessed_via_different_path_are_merged(async);

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
}
