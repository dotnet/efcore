// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.TestModels.TransportationModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

[SqlServerCondition(SqlServerCondition.SupportsTemporalTablesCascadeDelete)]
public class TemporalTableSqlServerTest : NonSharedModelTestBase
{
    protected override string StoreName
        => "TemporalTableSqlServerTest";

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Temporal_owned_basic(bool async)
    {
        var contextFactory = await InitializeAsync<MyContext26451>();
        using (var context = contextFactory.CreateContext())
        {
            var date = new DateTime(2000, 1, 1);

            var query = context.MainEntitiesDifferentTable.TemporalAsOf(date);
            var _ = async ? await query.ToListAsync() : query.ToList();
        }

        AssertSql(
            """
SELECT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime], [o].[MainEntityDifferentTableId], [o].[Description], [o].[EndTime], [o].[StartTime]
FROM [MainEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
LEFT JOIN [OwnedEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o] ON [m].[Id] = [o].[MainEntityDifferentTableId]
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Temporal_owned_join(bool async)
    {
        var contextFactory = await InitializeAsync<MyContext26451>();
        using (var context = contextFactory.CreateContext())
        {
            var date = new DateTime(2000, 1, 1);

            var query = context.MainEntitiesDifferentTable
                .TemporalAsOf(date)
                .Join(context.MainEntitiesDifferentTable, o => o.Id, i => i.Id, (o, i) => new { o, i });

            var _ = async ? await query.ToListAsync() : query.ToList();
        }

        AssertSql(
            """
SELECT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime], [o].[MainEntityDifferentTableId], [o].[Description], [o].[EndTime], [o].[StartTime], [m0].[Id], [m0].[Description], [m0].[EndTime], [m0].[StartTime], [o0].[MainEntityDifferentTableId], [o0].[Description], [o0].[EndTime], [o0].[StartTime]
FROM [MainEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
INNER JOIN [MainEntityDifferentTable] AS [m0] ON [m].[Id] = [m0].[Id]
LEFT JOIN [OwnedEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o] ON [m].[Id] = [o].[MainEntityDifferentTableId]
LEFT JOIN [OwnedEntityDifferentTable] AS [o0] ON [m0].[Id] = [o0].[MainEntityDifferentTableId]
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Temporal_owned_set_operation(bool async)
    {
        var contextFactory = await InitializeAsync<MyContext26451>();
        using (var context = contextFactory.CreateContext())
        {
            var date = new DateTime(2000, 1, 1);

            var query = context.MainEntitiesDifferentTable
                .TemporalAsOf(date)
                .Union(context.MainEntitiesDifferentTable.TemporalAsOf(date));

            var _ = async ? await query.ToListAsync() : query.ToList();
        }

        AssertSql(
            """
SELECT [u].[Id], [u].[Description], [u].[EndTime], [u].[StartTime], [o].[MainEntityDifferentTableId], [o].[Description], [o].[EndTime], [o].[StartTime]
FROM (
    SELECT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime]
    FROM [MainEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
    UNION
    SELECT [m0].[Id], [m0].[Description], [m0].[EndTime], [m0].[StartTime]
    FROM [MainEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m0]
) AS [u]
LEFT JOIN [OwnedEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o] ON [u].[Id] = [o].[MainEntityDifferentTableId]
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Temporal_owned_FromSql(bool async)
    {
        var contextFactory = await InitializeAsync<MyContext26451>();
        using (var context = contextFactory.CreateContext())
        {
            var date = new DateTime(2000, 1, 1);

            var query = context.MainEntitiesDifferentTable.FromSqlRaw(
                """
SELECT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime]
FROM [MainEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
""");

            var _ = async ? await query.ToListAsync() : query.ToList();
        }

        // just making sure we don't do anything weird here - there is no way to extract temporal information
        // from the FromSql so owned entity will always be treated as a regular query
        AssertSql(
            """
SELECT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime], [o].[MainEntityDifferentTableId], [o].[Description], [o].[EndTime], [o].[StartTime]
FROM (
    SELECT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime]
    FROM [MainEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
) AS [m]
LEFT JOIN [OwnedEntityDifferentTable] AS [o] ON [m].[Id] = [o].[MainEntityDifferentTableId]
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Temporal_owned_subquery(bool async)
    {
        var contextFactory = await InitializeAsync<MyContext26451>();
        using (var context = contextFactory.CreateContext())
        {
            var date = new DateTime(2000, 1, 1);

            var query = context.MainEntitiesDifferentTable
                .TemporalAsOf(date)
                .Distinct()
                .OrderByDescending(x => x.Id)
                .Take(3);

            var _ = async ? await query.ToListAsync() : query.ToList();
        }

        AssertSql(
            """
@__p_0='3'

SELECT TOP(@__p_0) [m0].[Id], [m0].[Description], [m0].[EndTime], [m0].[StartTime], [m0].[MainEntityDifferentTableId], [m0].[Description0], [m0].[EndTime0], [m0].[StartTime0]
FROM (
    SELECT DISTINCT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime], [o].[MainEntityDifferentTableId], [o].[Description] AS [Description0], [o].[EndTime] AS [EndTime0], [o].[StartTime] AS [StartTime0]
    FROM [MainEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
    LEFT JOIN [OwnedEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o] ON [m].[Id] = [o].[MainEntityDifferentTableId]
) AS [m0]
ORDER BY [m0].[Id] DESC
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Temporal_owned_complex(bool async)
    {
        var contextFactory = await InitializeAsync<MyContext26451>();
        using (var context = contextFactory.CreateContext())
        {
            var date = new DateTime(2000, 1, 1);

            var query = context.MainEntitiesDifferentTable.TemporalAsOf(date)
                .Join(context.MainEntitiesDifferentTable, x => x.Id, x => x.Id, (o, i) => new { o, i })
                .Distinct().OrderByDescending(x => x.o.Id).Take(3)
                .Join(context.MainEntitiesDifferentTable, xx => xx.o.Id, x => x.Id, (o, i) => new { o, i });

            var _ = async ? await query.ToListAsync() : query.ToList();
        }

        AssertSql(
            """
@__p_0='3'

SELECT [s0].[Id], [s0].[Description], [s0].[EndTime], [s0].[StartTime], [s0].[MainEntityDifferentTableId], [s0].[Description1], [s0].[EndTime1], [s0].[StartTime1], [s0].[Id0], [s0].[Description0], [s0].[EndTime0], [s0].[StartTime0], [s0].[MainEntityDifferentTableId0], [s0].[Description2], [s0].[EndTime2], [s0].[StartTime2], [m1].[Id], [m1].[Description], [m1].[EndTime], [m1].[StartTime], [o1].[MainEntityDifferentTableId], [o1].[Description], [o1].[EndTime], [o1].[StartTime]
FROM (
    SELECT TOP(@__p_0) [s].[Id], [s].[Description], [s].[EndTime], [s].[StartTime], [s].[Id0], [s].[Description0], [s].[EndTime0], [s].[StartTime0], [s].[MainEntityDifferentTableId], [s].[Description1], [s].[EndTime1], [s].[StartTime1], [s].[MainEntityDifferentTableId0], [s].[Description2], [s].[EndTime2], [s].[StartTime2]
    FROM (
        SELECT DISTINCT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime], [m0].[Id] AS [Id0], [m0].[Description] AS [Description0], [m0].[EndTime] AS [EndTime0], [m0].[StartTime] AS [StartTime0], [o].[MainEntityDifferentTableId], [o].[Description] AS [Description1], [o].[EndTime] AS [EndTime1], [o].[StartTime] AS [StartTime1], [o0].[MainEntityDifferentTableId] AS [MainEntityDifferentTableId0], [o0].[Description] AS [Description2], [o0].[EndTime] AS [EndTime2], [o0].[StartTime] AS [StartTime2]
        FROM [MainEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
        INNER JOIN [MainEntityDifferentTable] AS [m0] ON [m].[Id] = [m0].[Id]
        LEFT JOIN [OwnedEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o] ON [m].[Id] = [o].[MainEntityDifferentTableId]
        LEFT JOIN [OwnedEntityDifferentTable] AS [o0] ON [m0].[Id] = [o0].[MainEntityDifferentTableId]
    ) AS [s]
    ORDER BY [s].[Id] DESC
) AS [s0]
INNER JOIN [MainEntityDifferentTable] AS [m1] ON [s0].[Id] = [m1].[Id]
LEFT JOIN [OwnedEntityDifferentTable] AS [o1] ON [m1].[Id] = [o1].[MainEntityDifferentTableId]
ORDER BY [s0].[Id] DESC
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Temporal_owned_complex_with_nontrivial_alias(bool async)
    {
        var contextFactory = await InitializeAsync<MyContext26451>();
        using (var context = contextFactory.CreateContext())
        {
            var date = new DateTime(2000, 1, 1);

            var query = context.MainEntitiesDifferentTable
                .Join(context.MainEntitiesDifferentTable.TemporalAsOf(date), x => x.Id, x => x.Id, (o, i) => new { o, i })
                .Distinct().OrderByDescending(x => x.o.Id).Take(3)
                .Join(context.MainEntitiesDifferentTable, xx => xx.o.Id, x => x.Id, (o, i) => new { o, i });

            var _ = async ? await query.ToListAsync() : query.ToList();
        }

        AssertSql(
            """
@__p_0='3'

SELECT [s0].[Id], [s0].[Description], [s0].[EndTime], [s0].[StartTime], [s0].[MainEntityDifferentTableId], [s0].[Description1], [s0].[EndTime1], [s0].[StartTime1], [s0].[Id0], [s0].[Description0], [s0].[EndTime0], [s0].[StartTime0], [s0].[MainEntityDifferentTableId0], [s0].[Description2], [s0].[EndTime2], [s0].[StartTime2], [m1].[Id], [m1].[Description], [m1].[EndTime], [m1].[StartTime], [o1].[MainEntityDifferentTableId], [o1].[Description], [o1].[EndTime], [o1].[StartTime]
FROM (
    SELECT TOP(@__p_0) [s].[Id], [s].[Description], [s].[EndTime], [s].[StartTime], [s].[Id0], [s].[Description0], [s].[EndTime0], [s].[StartTime0], [s].[MainEntityDifferentTableId], [s].[Description1], [s].[EndTime1], [s].[StartTime1], [s].[MainEntityDifferentTableId0], [s].[Description2], [s].[EndTime2], [s].[StartTime2]
    FROM (
        SELECT DISTINCT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime], [m0].[Id] AS [Id0], [m0].[Description] AS [Description0], [m0].[EndTime] AS [EndTime0], [m0].[StartTime] AS [StartTime0], [o].[MainEntityDifferentTableId], [o].[Description] AS [Description1], [o].[EndTime] AS [EndTime1], [o].[StartTime] AS [StartTime1], [o0].[MainEntityDifferentTableId] AS [MainEntityDifferentTableId0], [o0].[Description] AS [Description2], [o0].[EndTime] AS [EndTime2], [o0].[StartTime] AS [StartTime2]
        FROM [MainEntityDifferentTable] AS [m]
        INNER JOIN [MainEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m0] ON [m].[Id] = [m0].[Id]
        LEFT JOIN [OwnedEntityDifferentTable] AS [o] ON [m].[Id] = [o].[MainEntityDifferentTableId]
        LEFT JOIN [OwnedEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o0] ON [m0].[Id] = [o0].[MainEntityDifferentTableId]
    ) AS [s]
    ORDER BY [s].[Id] DESC
) AS [s0]
INNER JOIN [MainEntityDifferentTable] AS [m1] ON [s0].[Id] = [m1].[Id]
LEFT JOIN [OwnedEntityDifferentTable] AS [o1] ON [m1].[Id] = [o1].[MainEntityDifferentTableId]
ORDER BY [s0].[Id] DESC
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Temporal_owned_range_operation_negative(bool async)
    {
        var contextFactory = await InitializeAsync<MyContext26451>();
        using (var context = contextFactory.CreateContext())
        {
            var message = async
                ? (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => context.MainEntitiesDifferentTable.TemporalAll().ToListAsync())).Message
                : Assert.Throws<InvalidOperationException>(() => context.MainEntitiesDifferentTable.TemporalAll().ToList()).Message;

            Assert.Equal(
                SqlServerStrings.TemporalNavigationExpansionOnlySupportedForAsOf("AsOf"),
                message);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Temporal_owned_mapped_to_same_table(bool async)
    {
        var contextFactory = await InitializeAsync<MyContext26451>();
        using (var context = contextFactory.CreateContext())
        {
            var date = new DateTime(2000, 1, 1);
            var query = context.MainEntitiesSameTable.TemporalAsOf(date);

            var _ = async ? await query.ToListAsync() : query.ToList();
        }

        AssertSql(
            """
SELECT [m].[Id], [m].[EndTime], [m].[Name], [m].[StartTime], [m].[EndTime], [m].[OwnedEntity_Name], [m].[OwnedEntity_Number], [m].[StartTime], [m].[OwnedEntity_Nested_Name], [m].[OwnedEntity_Nested_Number]
FROM [MainEntitiesSameTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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
            """
SELECT [m].[Id], [m].[Name], [m].[PeriodEnd], [m].[PeriodStart], [o].[MainEntityManyId], [o].[Id], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart]
FROM [MainEntitiesMany] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
LEFT JOIN [OwnedEntityMany] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o] ON [m].[Id] = [o].[MainEntityManyId]
ORDER BY [m].[Id], [o].[MainEntityManyId]
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Temporal_owned_with_union(bool async)
    {
        var contextFactory = await InitializeAsync<MyContext26451>();
        using (var context = contextFactory.CreateContext())
        {
            var date = new DateTime(2000, 1, 1);
            var query = context.MainEntitiesMany.TemporalAsOf(date)
                .Union(context.MainEntitiesMany.TemporalAsOf(date).Where(e => e.Id < 30));

            var _ = async ? await query.ToListAsync() : query.ToList();
        }

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[PeriodEnd], [u].[PeriodStart], [o].[MainEntityManyId], [o].[Id], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart]
FROM (
    SELECT [m].[Id], [m].[Name], [m].[PeriodEnd], [m].[PeriodStart]
    FROM [MainEntitiesMany] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
    UNION
    SELECT [m0].[Id], [m0].[Name], [m0].[PeriodEnd], [m0].[PeriodStart]
    FROM [MainEntitiesMany] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m0]
    WHERE [m0].[Id] < 30
) AS [u]
LEFT JOIN [OwnedEntityMany] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o] ON [u].[Id] = [o].[MainEntityManyId]
ORDER BY [u].[Id], [o].[MainEntityManyId]
""");
    }

    public class MainEntityDifferentTable
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public OwnedEntityDifferentTable OwnedEntity { get; set; }
    }

    public class OwnedEntityDifferentTable
    {
        public string Description { get; set; }
    }

    public class MainEntitySameTable
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public OwnedEntitySameTable OwnedEntity { get; set; }
    }

    public class OwnedEntitySameTable
    {
        public string Name { get; set; }
        public int Number { get; set; }

        public OwnedEntitySameTableNested Nested { get; set; }
    }

    public class OwnedEntitySameTableNested
    {
        public string Name { get; set; }
        public int Number { get; set; }
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
    }

    public class MyContext26451(DbContextOptions options) : DbContext(options)
    {
        public DbSet<MainEntityDifferentTable> MainEntitiesDifferentTable { get; set; }
        public DbSet<MainEntitySameTable> MainEntitiesSameTable { get; set; }
        public DbSet<MainEntityMany> MainEntitiesMany { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MainEntityDifferentTable>().ToTable(
                "MainEntityDifferentTable", tb => tb.IsTemporal(
                    ttb =>
                    {
                        ttb.HasPeriodStart("StartTime");
                        ttb.HasPeriodEnd("EndTime");
                        ttb.UseHistoryTable("ConfHistory");
                    }));
            modelBuilder.Entity<MainEntityDifferentTable>().Property(me => me.Id).UseIdentityColumn();
            modelBuilder.Entity<MainEntityDifferentTable>().OwnsOne(me => me.OwnedEntity).WithOwner();
            modelBuilder.Entity<MainEntityDifferentTable>().OwnsOne(
                me => me.OwnedEntity, oe =>
                {
                    oe.ToTable(
                        "OwnedEntityDifferentTable", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.HasPeriodStart("StartTime");
                                ttb.HasPeriodEnd("EndTime");
                                ttb.UseHistoryTable("OwnedEntityHistory");
                            }));
                });

            modelBuilder.Entity<MainEntitySameTable>(
                eb =>
                {
                    eb.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.HasPeriodStart("StartTime").HasColumnName("StartTime");
                                ttb.HasPeriodEnd("EndTime").HasColumnName("EndTime");
                            }));

                    eb.OwnsOne(
                        x => x.OwnedEntity, oeb =>
                        {
                            oeb.WithOwner();
                            oeb.ToTable(
                                tb => tb.IsTemporal(
                                    ttb =>
                                    {
                                        ttb.HasPeriodStart("StartTime").HasColumnName("StartTime");
                                        ttb.HasPeriodEnd("EndTime").HasColumnName("EndTime");
                                    }));
                            oeb.OwnsOne(
                                x => x.Nested, neb =>
                                {
                                    neb.WithOwner();
                                    neb.ToTable(
                                        tb => tb.IsTemporal(
                                            ttb =>
                                            {
                                                ttb.HasPeriodStart("StartTime").HasColumnName("StartTime");
                                                ttb.HasPeriodEnd("EndTime").HasColumnName("EndTime");
                                            }));
                                });
                        });
                });

            modelBuilder.Entity<MainEntityMany>(
                eb =>
                {
                    eb.ToTable(tb => tb.IsTemporal());
                    eb.OwnsMany(x => x.OwnedCollection, oeb => oeb.ToTable(tb => tb.IsTemporal()));
                });
        }
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Temporal_can_query_shared_derived_hierarchy(bool async)
    {
        var contectFactory = await InitializeAsync(OnModelCreating);
        using var context = contectFactory.CreateContext();
        var query = context.Set<FuelTank>().TemporalAsOf(new DateTime(2000, 1, 1));
        var _ = async ? await query.ToListAsync() : query.ToList();

        AssertSql(
            """
SELECT [v].[Name], [v].[Capacity], [v].[FuelTank_Discriminator], [v].[End], [v].[FuelType], [v].[Start], [v].[GrainGeometry]
FROM [Vehicles] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [v]
WHERE [v].[Capacity] IS NOT NULL AND [v].[FuelTank_Discriminator] IS NOT NULL
""");
    }

    protected Task<ContextFactory<TransportationContext>> InitializeAsync(
        Action<ModelBuilder> onModelCreating,
        bool seed = true)
        => InitializeAsync<TransportationContext>(
            onModelCreating, shouldLogCategory: _ => true, seed: seed ? c => c.SeedAsync() : null);

    protected virtual void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vehicle>(
            eb =>
            {
                eb.ToTable(
                    tb => tb.IsTemporal(
                        ttb =>
                        {
                            ttb.HasPeriodStart("Start").HasColumnName("Start");
                            ttb.HasPeriodEnd("End").HasColumnName("End");
                        }));
                eb.HasDiscriminator<string>("Discriminator");
                eb.Property<string>("Discriminator").HasColumnName("Discriminator");
                eb.ToTable("Vehicles");
            });

        modelBuilder.Entity<CompositeVehicle>();

        modelBuilder.Entity<Engine>()
            .ToTable(
                "Vehicles", tb => tb.IsTemporal(
                    ttb =>
                    {
                        ttb.HasPeriodStart("Start").HasColumnName("Start");
                        ttb.HasPeriodEnd("End").HasColumnName("End");
                    }));

        modelBuilder.Entity<Operator>().ToTable(
            "Vehicles", tb => tb.IsTemporal(
                ttb =>
                {
                    ttb.HasPeriodStart("Start").HasColumnName("Start");
                    ttb.HasPeriodEnd("End").HasColumnName("End");
                }));

        modelBuilder.Entity<OperatorDetails>().ToTable(
            "Vehicles", tb => tb.IsTemporal(
                ttb =>
                {
                    ttb.HasPeriodStart("Start").HasColumnName("Start");
                    ttb.HasPeriodEnd("End").HasColumnName("End");
                }));

        modelBuilder.Entity<FuelTank>().ToTable(
            "Vehicles", tb => tb.IsTemporal(
                ttb =>
                {
                    ttb.HasPeriodStart("Start").HasColumnName("Start");
                    ttb.HasPeriodEnd("End").HasColumnName("End");
                }));
    }
}
