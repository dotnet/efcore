// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    [SqlServerCondition(SqlServerCondition.SupportsTemporalTablesCascadeDelete)]
    public class TemporalTableSqlServerTest : NonSharedModelTestBase
    {
        protected override string StoreName => "TemporalTableSqlServerTest";

        protected TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;

        protected void AssertSql(params string[] expected) => TestSqlLoggerFactory.AssertBaseline(expected);

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Temporal_owned_basic(bool async)
        {
            var contextFactory = await InitializeAsync<MyContext26451>();
            using (var context = contextFactory.CreateContext())
            {
                var date = new DateTime(2000, 1, 1);

                var query = context.MainEntities.TemporalAsOf(date);
                var _ = async ? await query.ToListAsync() : query.ToList();
            }

            AssertSql(
                @"SELECT [m].[Id], [m].[Description], [m].[PeriodEnd], [m].[PeriodStart], [o].[MainEntityId], [o].[Description], [o].[Number], [o].[PeriodEnd], [o].[PeriodStart], [o0].[OwnedEntityMainEntityId], [o1].[OwnedEntityMainEntityId], [o1].[Id], [o1].[Name], [o1].[PeriodEnd], [o1].[PeriodStart], [o0].[Name], [o0].[PeriodEnd], [o0].[PeriodStart]
FROM [MainEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
LEFT JOIN [OwnedEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o] ON [m].[Id] = [o].[MainEntityId]
LEFT JOIN [OwnedEntityNestedOne] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o0] ON [o].[MainEntityId] = [o0].[OwnedEntityMainEntityId]
LEFT JOIN [OwnedEntityNestedMany] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o1] ON [o].[MainEntityId] = [o1].[OwnedEntityMainEntityId]
ORDER BY [m].[Id], [o].[MainEntityId], [o0].[OwnedEntityMainEntityId], [o1].[OwnedEntityMainEntityId]");
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Temporal_owned_join(bool async)
        {
            var contextFactory = await InitializeAsync<MyContext26451>();
            using (var context = contextFactory.CreateContext())
            {
                var date = new DateTime(2000, 1, 1);

                var query = context.MainEntities
                    .TemporalAsOf(date)
                    .Join(context.MainEntities, o => o.Id, i => i.Id, (o, i) => new { o, i });

                var _ = async ? await query.ToListAsync() : query.ToList();
            }

            AssertSql(
                @"SELECT [m].[Id], [m].[Description], [m].[PeriodEnd], [m].[PeriodStart], [o].[MainEntityId], [o].[Description], [o].[Number], [o].[PeriodEnd], [o].[PeriodStart], [m0].[Id], [o0].[OwnedEntityMainEntityId], [o1].[MainEntityId], [o2].[OwnedEntityMainEntityId], [o3].[OwnedEntityMainEntityId], [o3].[Id], [o3].[Name], [o3].[PeriodEnd], [o3].[PeriodStart], [o0].[Name], [o0].[PeriodEnd], [o0].[PeriodStart], [m0].[Description], [m0].[PeriodEnd], [m0].[PeriodStart], [o1].[Description], [o1].[Number], [o1].[PeriodEnd], [o1].[PeriodStart], [o4].[OwnedEntityMainEntityId], [o4].[Id], [o4].[Name], [o4].[PeriodEnd], [o4].[PeriodStart], [o2].[Name], [o2].[PeriodEnd], [o2].[PeriodStart]
FROM [MainEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
INNER JOIN [MainEntity] AS [m0] ON [m].[Id] = [m0].[Id]
LEFT JOIN [OwnedEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o] ON [m].[Id] = [o].[MainEntityId]
LEFT JOIN [OwnedEntityNestedOne] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o0] ON [o].[MainEntityId] = [o0].[OwnedEntityMainEntityId]
LEFT JOIN [OwnedEntity] AS [o1] ON [m0].[Id] = [o1].[MainEntityId]
LEFT JOIN [OwnedEntityNestedOne] AS [o2] ON [o1].[MainEntityId] = [o2].[OwnedEntityMainEntityId]
LEFT JOIN [OwnedEntityNestedMany] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o3] ON [o].[MainEntityId] = [o3].[OwnedEntityMainEntityId]
LEFT JOIN [OwnedEntityNestedMany] AS [o4] ON [o1].[MainEntityId] = [o4].[OwnedEntityMainEntityId]
ORDER BY [m].[Id], [m0].[Id], [o].[MainEntityId], [o0].[OwnedEntityMainEntityId], [o1].[MainEntityId], [o2].[OwnedEntityMainEntityId], [o3].[OwnedEntityMainEntityId], [o3].[Id], [o4].[OwnedEntityMainEntityId]");
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Temporal_owned_set_operation(bool async)
        {
            var contextFactory = await InitializeAsync<MyContext26451>();
            using (var context = contextFactory.CreateContext())
            {
                var date = new DateTime(2000, 1, 1);

                var query = context.MainEntities
                    .TemporalAsOf(date)
                    .Union(context.MainEntities.TemporalAsOf(date));

                var _ = async ? await query.ToListAsync() : query.ToList();
            }

            AssertSql(
                @"SELECT [t].[Id], [t].[Description], [t].[PeriodEnd], [t].[PeriodStart], [o].[MainEntityId], [o].[Description], [o].[Number], [o].[PeriodEnd], [o].[PeriodStart], [o0].[OwnedEntityMainEntityId], [o1].[OwnedEntityMainEntityId], [o1].[Id], [o1].[Name], [o1].[PeriodEnd], [o1].[PeriodStart], [o0].[Name], [o0].[PeriodEnd], [o0].[PeriodStart]
FROM (
    SELECT [m].[Id], [m].[Description], [m].[PeriodEnd], [m].[PeriodStart]
    FROM [MainEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
    UNION
    SELECT [m0].[Id], [m0].[Description], [m0].[PeriodEnd], [m0].[PeriodStart]
    FROM [MainEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m0]
) AS [t]
LEFT JOIN [OwnedEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o] ON [t].[Id] = [o].[MainEntityId]
LEFT JOIN [OwnedEntityNestedOne] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o0] ON [o].[MainEntityId] = [o0].[OwnedEntityMainEntityId]
LEFT JOIN [OwnedEntityNestedMany] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o1] ON [o].[MainEntityId] = [o1].[OwnedEntityMainEntityId]
ORDER BY [t].[Id], [o].[MainEntityId], [o0].[OwnedEntityMainEntityId], [o1].[OwnedEntityMainEntityId]");
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Temporal_owned_FromSql(bool async)
        {
            var contextFactory = await InitializeAsync<MyContext26451>();
            using (var context = contextFactory.CreateContext())
            {
                var date = new DateTime(2000, 1, 1);

                var query = context.MainEntities.FromSqlRaw(
                    @"SELECT [m].[Id], [m].[Description], [m].[PeriodEnd], [m].[PeriodStart]
FROM [MainEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]");

                var _ = async ? await query.ToListAsync() : query.ToList();
            }

            // just making sure we don't do anything weird here - there is no way to extract temporal information
            // from the FromSql so owned entity will always be treated as a regular query
            AssertSql(
                @"SELECT [m].[Id], [m].[Description], [m].[PeriodEnd], [m].[PeriodStart], [o].[MainEntityId], [o].[Description], [o].[Number], [o].[PeriodEnd], [o].[PeriodStart], [o0].[OwnedEntityMainEntityId], [o1].[OwnedEntityMainEntityId], [o1].[Id], [o1].[Name], [o1].[PeriodEnd], [o1].[PeriodStart], [o0].[Name], [o0].[PeriodEnd], [o0].[PeriodStart]
FROM (
    SELECT [m].[Id], [m].[Description], [m].[PeriodEnd], [m].[PeriodStart]
    FROM [MainEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
) AS [m]
LEFT JOIN [OwnedEntity] AS [o] ON [m].[Id] = [o].[MainEntityId]
LEFT JOIN [OwnedEntityNestedOne] AS [o0] ON [o].[MainEntityId] = [o0].[OwnedEntityMainEntityId]
LEFT JOIN [OwnedEntityNestedMany] AS [o1] ON [o].[MainEntityId] = [o1].[OwnedEntityMainEntityId]
ORDER BY [m].[Id], [o].[MainEntityId], [o0].[OwnedEntityMainEntityId], [o1].[OwnedEntityMainEntityId]");
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Temporal_owned_subquery(bool async)
        {
            var contextFactory = await InitializeAsync<MyContext26451>();
            using (var context = contextFactory.CreateContext())
            {
                var date = new DateTime(2000, 1, 1);

                var query = context.MainEntities
                    .TemporalAsOf(date)
                    .Distinct()
                    .OrderByDescending(x => x.Id)
                    .Take(3);

                var _ = async ? await query.ToListAsync() : query.ToList();
            }

            AssertSql(
                @"@__p_0='3'

SELECT [t0].[Id], [t0].[Description], [t0].[PeriodEnd], [t0].[PeriodStart], [o].[MainEntityId], [o].[Description], [o].[Number], [o].[PeriodEnd], [o].[PeriodStart], [o0].[MainEntityId], [o1].[OwnedEntityMainEntityId], [o2].[OwnedEntityMainEntityId], [o2].[Id], [o2].[Name], [o2].[PeriodEnd], [o2].[PeriodStart], [o1].[Name], [o1].[PeriodEnd], [o1].[PeriodStart]
FROM (
    SELECT TOP(@__p_0) [t].[Id], [t].[Description], [t].[PeriodEnd], [t].[PeriodStart]
    FROM (
        SELECT DISTINCT [m].[Id], [m].[Description], [m].[PeriodEnd], [m].[PeriodStart]
        FROM [MainEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
    ) AS [t]
    ORDER BY [t].[Id] DESC
) AS [t0]
LEFT JOIN [OwnedEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o] ON [t0].[Id] = [o].[MainEntityId]
LEFT JOIN [OwnedEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o0] ON [t0].[Id] = [o0].[MainEntityId]
LEFT JOIN [OwnedEntityNestedOne] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o1] ON [o0].[MainEntityId] = [o1].[OwnedEntityMainEntityId]
LEFT JOIN [OwnedEntityNestedMany] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o2] ON [o0].[MainEntityId] = [o2].[OwnedEntityMainEntityId]
ORDER BY [t0].[Id] DESC, [o].[MainEntityId], [o0].[MainEntityId], [o1].[OwnedEntityMainEntityId], [o2].[OwnedEntityMainEntityId]");
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Temporal_owned_complex(bool async)
        {
            var contextFactory = await InitializeAsync<MyContext26451>();
            using (var context = contextFactory.CreateContext())
            {
                var date = new DateTime(2000, 1, 1);

                var query = context.MainEntities.TemporalAsOf(date)
                    .Join(context.MainEntities, x => x.Id, x => x.Id, (o, i) => new { o, i })
                    .Distinct().OrderByDescending(x => x.o.Id).Take(3)
                    .Join(context.MainEntities, xx => xx.o.Id, x => x.Id, (o, i) => new { o, i });

                var _ = async ? await query.ToListAsync() : query.ToList();
            }

            AssertSql(
                @"@__p_0='3'

SELECT [t0].[Id], [t0].[Description], [t0].[PeriodEnd], [t0].[PeriodStart], [o].[MainEntityId], [o].[Description], [o].[Number], [o].[PeriodEnd], [o].[PeriodStart], [t0].[Id0], [m1].[Id], [o0].[OwnedEntityMainEntityId], [o1].[MainEntityId], [o2].[OwnedEntityMainEntityId], [o3].[MainEntityId], [o4].[OwnedEntityMainEntityId], [o5].[OwnedEntityMainEntityId], [o5].[Id], [o5].[Name], [o5].[PeriodEnd], [o5].[PeriodStart], [o0].[Name], [o0].[PeriodEnd], [o0].[PeriodStart], [t0].[Description0], [t0].[PeriodEnd0], [t0].[PeriodStart0], [o1].[Description], [o1].[Number], [o1].[PeriodEnd], [o1].[PeriodStart], [o6].[OwnedEntityMainEntityId], [o6].[Id], [o6].[Name], [o6].[PeriodEnd], [o6].[PeriodStart], [o2].[Name], [o2].[PeriodEnd], [o2].[PeriodStart], [m1].[Description], [m1].[PeriodEnd], [m1].[PeriodStart], [o3].[Description], [o3].[Number], [o3].[PeriodEnd], [o3].[PeriodStart], [o7].[OwnedEntityMainEntityId], [o7].[Id], [o7].[Name], [o7].[PeriodEnd], [o7].[PeriodStart], [o4].[Name], [o4].[PeriodEnd], [o4].[PeriodStart]
FROM (
    SELECT TOP(@__p_0) [t].[Id], [t].[Description], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t].[Description0], [t].[PeriodEnd0], [t].[PeriodStart0]
    FROM (
        SELECT DISTINCT [m].[Id], [m].[Description], [m].[PeriodEnd], [m].[PeriodStart], [m0].[Id] AS [Id0], [m0].[Description] AS [Description0], [m0].[PeriodEnd] AS [PeriodEnd0], [m0].[PeriodStart] AS [PeriodStart0]
        FROM [MainEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
        INNER JOIN [MainEntity] AS [m0] ON [m].[Id] = [m0].[Id]
    ) AS [t]
    ORDER BY [t].[Id] DESC
) AS [t0]
INNER JOIN [MainEntity] AS [m1] ON [t0].[Id] = [m1].[Id]
LEFT JOIN [OwnedEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o] ON [t0].[Id] = [o].[MainEntityId]
LEFT JOIN [OwnedEntityNestedOne] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o0] ON [o].[MainEntityId] = [o0].[OwnedEntityMainEntityId]
LEFT JOIN [OwnedEntity] AS [o1] ON [t0].[Id0] = [o1].[MainEntityId]
LEFT JOIN [OwnedEntityNestedOne] AS [o2] ON [o1].[MainEntityId] = [o2].[OwnedEntityMainEntityId]
LEFT JOIN [OwnedEntity] AS [o3] ON [m1].[Id] = [o3].[MainEntityId]
LEFT JOIN [OwnedEntityNestedOne] AS [o4] ON [o3].[MainEntityId] = [o4].[OwnedEntityMainEntityId]
LEFT JOIN [OwnedEntityNestedMany] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o5] ON [o].[MainEntityId] = [o5].[OwnedEntityMainEntityId]
LEFT JOIN [OwnedEntityNestedMany] AS [o6] ON [o1].[MainEntityId] = [o6].[OwnedEntityMainEntityId]
LEFT JOIN [OwnedEntityNestedMany] AS [o7] ON [o3].[MainEntityId] = [o7].[OwnedEntityMainEntityId]
ORDER BY [t0].[Id] DESC, [t0].[Id0], [m1].[Id], [o].[MainEntityId], [o0].[OwnedEntityMainEntityId], [o1].[MainEntityId], [o2].[OwnedEntityMainEntityId], [o3].[MainEntityId], [o4].[OwnedEntityMainEntityId], [o5].[OwnedEntityMainEntityId], [o5].[Id], [o6].[OwnedEntityMainEntityId], [o6].[Id], [o7].[OwnedEntityMainEntityId]");
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Temporal_owned_complex_with_nontrivial_alias(bool async)
        {
            var contextFactory = await InitializeAsync<MyContext26451>();
            using (var context = contextFactory.CreateContext())
            {
                var date = new DateTime(2000, 1, 1);

                var query = context.MainEntities
                    .Join(context.MainEntities.TemporalAsOf(date), x => x.Id, x => x.Id, (o, i) => new { o, i })
                    .Distinct().OrderByDescending(x => x.o.Id).Take(3)
                    .Join(context.MainEntities, xx => xx.o.Id, x => x.Id, (o, i) => new { o, i });

                var _ = async ? await query.ToListAsync() : query.ToList();
            }

            AssertSql(
                @"@__p_0='3'

SELECT [t0].[Id], [t0].[Description], [t0].[PeriodEnd], [t0].[PeriodStart], [o].[MainEntityId], [o].[Description], [o].[Number], [o].[PeriodEnd], [o].[PeriodStart], [t0].[Id0], [m1].[Id], [o0].[OwnedEntityMainEntityId], [o1].[MainEntityId], [o2].[OwnedEntityMainEntityId], [o3].[MainEntityId], [o4].[OwnedEntityMainEntityId], [o5].[OwnedEntityMainEntityId], [o5].[Id], [o5].[Name], [o5].[PeriodEnd], [o5].[PeriodStart], [o0].[Name], [o0].[PeriodEnd], [o0].[PeriodStart], [t0].[Description0], [t0].[PeriodEnd0], [t0].[PeriodStart0], [o1].[Description], [o1].[Number], [o1].[PeriodEnd], [o1].[PeriodStart], [o6].[OwnedEntityMainEntityId], [o6].[Id], [o6].[Name], [o6].[PeriodEnd], [o6].[PeriodStart], [o2].[Name], [o2].[PeriodEnd], [o2].[PeriodStart], [m1].[Description], [m1].[PeriodEnd], [m1].[PeriodStart], [o3].[Description], [o3].[Number], [o3].[PeriodEnd], [o3].[PeriodStart], [o7].[OwnedEntityMainEntityId], [o7].[Id], [o7].[Name], [o7].[PeriodEnd], [o7].[PeriodStart], [o4].[Name], [o4].[PeriodEnd], [o4].[PeriodStart]
FROM (
    SELECT TOP(@__p_0) [t].[Id], [t].[Description], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t].[Description0], [t].[PeriodEnd0], [t].[PeriodStart0]
    FROM (
        SELECT DISTINCT [m].[Id], [m].[Description], [m].[PeriodEnd], [m].[PeriodStart], [m0].[Id] AS [Id0], [m0].[Description] AS [Description0], [m0].[PeriodEnd] AS [PeriodEnd0], [m0].[PeriodStart] AS [PeriodStart0]
        FROM [MainEntity] AS [m]
        INNER JOIN [MainEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m0] ON [m].[Id] = [m0].[Id]
    ) AS [t]
    ORDER BY [t].[Id] DESC
) AS [t0]
INNER JOIN [MainEntity] AS [m1] ON [t0].[Id] = [m1].[Id]
LEFT JOIN [OwnedEntity] AS [o] ON [t0].[Id] = [o].[MainEntityId]
LEFT JOIN [OwnedEntityNestedOne] AS [o0] ON [o].[MainEntityId] = [o0].[OwnedEntityMainEntityId]
LEFT JOIN [OwnedEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o1] ON [t0].[Id0] = [o1].[MainEntityId]
LEFT JOIN [OwnedEntityNestedOne] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o2] ON [o1].[MainEntityId] = [o2].[OwnedEntityMainEntityId]
LEFT JOIN [OwnedEntity] AS [o3] ON [m1].[Id] = [o3].[MainEntityId]
LEFT JOIN [OwnedEntityNestedOne] AS [o4] ON [o3].[MainEntityId] = [o4].[OwnedEntityMainEntityId]
LEFT JOIN [OwnedEntityNestedMany] AS [o5] ON [o].[MainEntityId] = [o5].[OwnedEntityMainEntityId]
LEFT JOIN [OwnedEntityNestedMany] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o6] ON [o1].[MainEntityId] = [o6].[OwnedEntityMainEntityId]
LEFT JOIN [OwnedEntityNestedMany] AS [o7] ON [o3].[MainEntityId] = [o7].[OwnedEntityMainEntityId]
ORDER BY [t0].[Id] DESC, [t0].[Id0], [m1].[Id], [o].[MainEntityId], [o0].[OwnedEntityMainEntityId], [o1].[MainEntityId], [o2].[OwnedEntityMainEntityId], [o3].[MainEntityId], [o4].[OwnedEntityMainEntityId], [o5].[OwnedEntityMainEntityId], [o5].[Id], [o6].[OwnedEntityMainEntityId], [o6].[Id], [o7].[OwnedEntityMainEntityId]");
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Temporal_owned_range_operation_negative(bool async)
        {
            var contextFactory = await InitializeAsync<MyContext26451>();
            using (var context = contextFactory.CreateContext())
            {
                var message = async
                    ? (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => context.MainEntities.TemporalAll().ToListAsync())).Message
                    : Assert.Throws<InvalidOperationException>(() => context.MainEntities.TemporalAll().ToList()).Message;

                Assert.Equal(
                    SqlServerStrings.TemporalOwnedTypeMappedToDifferentTableOnlySupportedForAsOf("AsOf"),
                    message);
            }
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Temporal_owned_many(bool async)
        {
            var contextFactory = await InitializeAsync<MyContext26451>();
            using (var context = contextFactory.CreateContext())
            {
                var date = new DateTime(2000, 1, 1);
                var query = context.MainEntitiesMany.TemporalAsOf(date);

                var _ = async ? await query.ToListAsync() : query.ToList();
            }

            AssertSql(
                @"SELECT [m].[Id], [m].[Name], [m].[PeriodEnd], [m].[PeriodStart], [t].[MainEntityManyId], [t].[Id], [t].[Name], [t].[Number], [t].[PeriodEnd], [t].[PeriodStart], [t].[OwnedEntityManyMainEntityManyId], [t].[OwnedEntityManyId], [t].[OwnedEntityManyMainEntityManyId0], [t].[OwnedEntityManyId0], [t].[Id0], [t].[Name0], [t].[PeriodEnd0], [t].[PeriodStart0], [t].[Name1], [t].[PeriodEnd1], [t].[PeriodStart1]
FROM [MainEntityMany] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
LEFT JOIN (
    SELECT [o].[MainEntityManyId], [o].[Id], [o].[Name], [o].[Number], [o].[PeriodEnd], [o].[PeriodStart], [o0].[OwnedEntityManyMainEntityManyId], [o0].[OwnedEntityManyId], [o1].[OwnedEntityManyMainEntityManyId] AS [OwnedEntityManyMainEntityManyId0], [o1].[OwnedEntityManyId] AS [OwnedEntityManyId0], [o1].[Id] AS [Id0], [o1].[Name] AS [Name0], [o1].[PeriodEnd] AS [PeriodEnd0], [o1].[PeriodStart] AS [PeriodStart0], [o0].[Name] AS [Name1], [o0].[PeriodEnd] AS [PeriodEnd1], [o0].[PeriodStart] AS [PeriodStart1]
    FROM [OwnedEntityMany] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o]
    LEFT JOIN [OwnedEntityManyNestedOne] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o0] ON ([o].[MainEntityManyId] = [o0].[OwnedEntityManyMainEntityManyId]) AND ([o].[Id] = [o0].[OwnedEntityManyId])
    LEFT JOIN [OwnedEntityManyNestedMany] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o1] ON ([o].[MainEntityManyId] = [o1].[OwnedEntityManyMainEntityManyId]) AND ([o].[Id] = [o1].[OwnedEntityManyId])
) AS [t] ON [m].[Id] = [t].[MainEntityManyId]
ORDER BY [m].[Id], [t].[MainEntityManyId], [t].[Id], [t].[OwnedEntityManyMainEntityManyId], [t].[OwnedEntityManyId], [t].[OwnedEntityManyMainEntityManyId0], [t].[OwnedEntityManyId0]");
        }

        public class MainEntity
        {
            public int Id { get; set; }
            public string Description { get; set; }
            public OwnedEntity OwnedEntity { get; set; }
        }

        public class OwnedEntity
        {
            public string Description { get; set; }
            public int Number { get; set; }
            public OwnedEntityNestedOne One { get; set; }
            public List<OwnedEntityNestedMany> Many { get; set; }
        }

        public class OwnedEntityNestedOne
        {
            public string Name { get; set; }
        }

        public class OwnedEntityNestedMany
        {
            public string Name { get; set; }
        }

        public class MainEntityMany
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<OwnedEntityMany> OwnedCollection { get; set; }
        }

        public class OwnedEntityMany
        {
            public string Name { get; set; }
            public int Number { get; set; }

            public OwnedEntityManyNestedOne One { get; set; }
            public List<OwnedEntityManyNestedMany> Many { get; set; }
        }

        public class OwnedEntityManyNestedOne
        {
            public string Name { get; set; }
        }

        public class OwnedEntityManyNestedMany
        {
            public string Name { get; set; }
        }

        public class MyContext26451 : DbContext
        {
            public MyContext26451(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<MainEntity> MainEntities { get; set; }
            public DbSet<MainEntityMany> MainEntitiesMany { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MainEntity>().ToTable("MainEntity", tb => tb.IsTemporal());
                modelBuilder.Entity<MainEntity>().Property(me => me.Id);
                modelBuilder.Entity<MainEntity>().OwnsOne(me => me.OwnedEntity, oeb =>
                {
                    oeb.ToTable("OwnedEntity", tb => tb.IsTemporal());
                    oeb.OwnsOne(x => x.One, nb => nb.ToTable("OwnedEntityNestedOne", tb => tb.IsTemporal()));
                    oeb.OwnsMany(x => x.Many, nb => nb.ToTable("OwnedEntityNestedMany", tb => tb.IsTemporal()));
                });

                modelBuilder.Entity<MainEntityMany>(eb =>
                {
                    eb.ToTable("MainEntityMany", tb => tb.IsTemporal());
                    eb.OwnsMany(x => x.OwnedCollection, oeb =>
                    {
                        oeb.ToTable("OwnedEntityMany", tb => tb.IsTemporal());
                        oeb.OwnsOne(x => x.One, nb => nb.ToTable("OwnedEntityManyNestedOne", tb => tb.IsTemporal()));
                        oeb.OwnsMany(x => x.Many, nb => nb.ToTable("OwnedEntityManyNestedMany", tb => tb.IsTemporal()));
                    });
                });
            }
        }
    }
}
