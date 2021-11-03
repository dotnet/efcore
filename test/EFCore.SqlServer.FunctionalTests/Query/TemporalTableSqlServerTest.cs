// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
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
                @"SELECT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime], [o].[MainEntityId], [o].[Description], [o].[EndTime], [o].[StartTime]
FROM [MainEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
LEFT JOIN [OwnedEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o] ON [m].[Id] = [o].[MainEntityId]");
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
                @"SELECT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime], [o].[MainEntityId], [o].[Description], [o].[EndTime], [o].[StartTime], [m0].[Id], [m0].[Description], [m0].[EndTime], [m0].[StartTime], [o0].[MainEntityId], [o0].[Description], [o0].[EndTime], [o0].[StartTime]
FROM [MainEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
INNER JOIN [MainEntity] AS [m0] ON [m].[Id] = [m0].[Id]
LEFT JOIN [OwnedEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o] ON [m].[Id] = [o].[MainEntityId]
LEFT JOIN [OwnedEntity] AS [o0] ON [m0].[Id] = [o0].[MainEntityId]");
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
                @"SELECT [t].[Id], [t].[Description], [t].[EndTime], [t].[StartTime], [o].[MainEntityId], [o].[Description], [o].[EndTime], [o].[StartTime]
FROM (
    SELECT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime]
    FROM [MainEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
    UNION
    SELECT [m0].[Id], [m0].[Description], [m0].[EndTime], [m0].[StartTime]
    FROM [MainEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m0]
) AS [t]
LEFT JOIN [OwnedEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o] ON [t].[Id] = [o].[MainEntityId]");
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
                    @"SELECT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime]
FROM [MainEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]");

                var _ = async ? await query.ToListAsync() : query.ToList();
            }

            // just making sure we don't do anything weird here - there is no way to extract temporal information
            // from the FromSql so owned entity will always be treated as a regular query
            AssertSql(
                @"SELECT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime], [o].[MainEntityId], [o].[Description], [o].[EndTime], [o].[StartTime]
FROM (
    SELECT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime]
    FROM [MainEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
) AS [m]
LEFT JOIN [OwnedEntity] AS [o] ON [m].[Id] = [o].[MainEntityId]");
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

SELECT [t0].[Id], [t0].[Description], [t0].[EndTime], [t0].[StartTime], [o].[MainEntityId], [o].[Description], [o].[EndTime], [o].[StartTime]
FROM (
    SELECT TOP(@__p_0) [t].[Id], [t].[Description], [t].[EndTime], [t].[StartTime]
    FROM (
        SELECT DISTINCT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime]
        FROM [MainEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
    ) AS [t]
    ORDER BY [t].[Id] DESC
) AS [t0]
LEFT JOIN [OwnedEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o] ON [t0].[Id] = [o].[MainEntityId]
ORDER BY [t0].[Id] DESC");
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

SELECT [t0].[Id], [t0].[Description], [t0].[EndTime], [t0].[StartTime], [o].[MainEntityId], [o].[Description], [o].[EndTime], [o].[StartTime], [t0].[Id0], [t0].[Description0], [t0].[EndTime0], [t0].[StartTime0], [o0].[MainEntityId], [o0].[Description], [o0].[EndTime], [o0].[StartTime], [m1].[Id], [m1].[Description], [m1].[EndTime], [m1].[StartTime], [o1].[MainEntityId], [o1].[Description], [o1].[EndTime], [o1].[StartTime]
FROM (
    SELECT TOP(@__p_0) [t].[Id], [t].[Description], [t].[EndTime], [t].[StartTime], [t].[Id0], [t].[Description0], [t].[EndTime0], [t].[StartTime0]
    FROM (
        SELECT DISTINCT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime], [m0].[Id] AS [Id0], [m0].[Description] AS [Description0], [m0].[EndTime] AS [EndTime0], [m0].[StartTime] AS [StartTime0]
        FROM [MainEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
        INNER JOIN [MainEntity] AS [m0] ON [m].[Id] = [m0].[Id]
    ) AS [t]
    ORDER BY [t].[Id] DESC
) AS [t0]
INNER JOIN [MainEntity] AS [m1] ON [t0].[Id] = [m1].[Id]
LEFT JOIN [OwnedEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o] ON [t0].[Id] = [o].[MainEntityId]
LEFT JOIN [OwnedEntity] AS [o0] ON [t0].[Id0] = [o0].[MainEntityId]
LEFT JOIN [OwnedEntity] AS [o1] ON [m1].[Id] = [o1].[MainEntityId]
ORDER BY [t0].[Id] DESC");
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

SELECT [t0].[Id], [t0].[Description], [t0].[EndTime], [t0].[StartTime], [o].[MainEntityId], [o].[Description], [o].[EndTime], [o].[StartTime], [t0].[Id0], [t0].[Description0], [t0].[EndTime0], [t0].[StartTime0], [o0].[MainEntityId], [o0].[Description], [o0].[EndTime], [o0].[StartTime], [m1].[Id], [m1].[Description], [m1].[EndTime], [m1].[StartTime], [o1].[MainEntityId], [o1].[Description], [o1].[EndTime], [o1].[StartTime]
FROM (
    SELECT TOP(@__p_0) [t].[Id], [t].[Description], [t].[EndTime], [t].[StartTime], [t].[Id0], [t].[Description0], [t].[EndTime0], [t].[StartTime0]
    FROM (
        SELECT DISTINCT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime], [m0].[Id] AS [Id0], [m0].[Description] AS [Description0], [m0].[EndTime] AS [EndTime0], [m0].[StartTime] AS [StartTime0]
        FROM [MainEntity] AS [m]
        INNER JOIN [MainEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m0] ON [m].[Id] = [m0].[Id]
    ) AS [t]
    ORDER BY [t].[Id] DESC
) AS [t0]
INNER JOIN [MainEntity] AS [m1] ON [t0].[Id] = [m1].[Id]
LEFT JOIN [OwnedEntity] AS [o] ON [t0].[Id] = [o].[MainEntityId]
LEFT JOIN [OwnedEntity] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o0] ON [t0].[Id0] = [o0].[MainEntityId]
LEFT JOIN [OwnedEntity] AS [o1] ON [m1].[Id] = [o1].[MainEntityId]
ORDER BY [t0].[Id] DESC");
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

        public class MainEntity
        {
            public int Id { get; set; }
            public string Description { get; set; }
            public OwnedEntity OwnedEntity { get; set; }
        }

        public class OwnedEntity
        {
            public string Description { get; set; }
        }

        public class MyContext26451 : DbContext
        {
            public MyContext26451(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<MainEntity> MainEntities { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MainEntity>().ToTable("MainEntity", tb => tb.IsTemporal(ttb =>
                {
                    ttb.HasPeriodStart("StartTime");
                    ttb.HasPeriodEnd("EndTime");
                    ttb.UseHistoryTable("ConfHistory");
                }));
                modelBuilder.Entity<MainEntity>().Property(me => me.Id).UseIdentityColumn();
                modelBuilder.Entity<MainEntity>().OwnsOne(me => me.OwnedEntity).WithOwner();
                modelBuilder.Entity<MainEntity>().OwnsOne(me => me.OwnedEntity, oe =>
                {
                    oe.ToTable("OwnedEntity", tb => tb.IsTemporal(ttb =>
                    {
                        ttb.HasPeriodStart("StartTime");
                        ttb.HasPeriodEnd("EndTime");
                        ttb.UseHistoryTable("OwnedEntityHistory");
                    }));
                });
            }
        }
    }
}
