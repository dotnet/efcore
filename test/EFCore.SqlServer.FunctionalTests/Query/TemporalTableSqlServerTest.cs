// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.TestModels.TransportationModel;

namespace Microsoft.EntityFrameworkCore.Query;

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

            var query = context.MainEntitiesDifferentTable.TemporalAsOf(date);
            var _ = async ? await query.ToListAsync() : query.ToList();
        }

        AssertSql(
            @"SELECT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime], [o].[MainEntityDifferentTableId], [o].[Description], [o].[EndTime], [o].[StartTime]
FROM [MainEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
LEFT JOIN [OwnedEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o] ON [m].[Id] = [o].[MainEntityDifferentTableId]");
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

            var query = context.MainEntitiesDifferentTable
                .TemporalAsOf(date)
                .Join(context.MainEntitiesDifferentTable, o => o.Id, i => i.Id, (o, i) => new { o, i });

            var _ = async ? await query.ToListAsync() : query.ToList();
        }

        AssertSql(
            @"SELECT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime], [o].[MainEntityDifferentTableId], [o].[Description], [o].[EndTime], [o].[StartTime], [m0].[Id], [m0].[Description], [m0].[EndTime], [m0].[StartTime], [o0].[MainEntityDifferentTableId], [o0].[Description], [o0].[EndTime], [o0].[StartTime]
FROM [MainEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
INNER JOIN [MainEntityDifferentTable] AS [m0] ON [m].[Id] = [m0].[Id]
LEFT JOIN [OwnedEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o] ON [m].[Id] = [o].[MainEntityDifferentTableId]
LEFT JOIN [OwnedEntityDifferentTable] AS [o0] ON [m0].[Id] = [o0].[MainEntityDifferentTableId]");
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

            var query = context.MainEntitiesDifferentTable
                .TemporalAsOf(date)
                .Union(context.MainEntitiesDifferentTable.TemporalAsOf(date));

            var _ = async ? await query.ToListAsync() : query.ToList();
        }

        AssertSql(
            @"SELECT [t].[Id], [t].[Description], [t].[EndTime], [t].[StartTime], [o].[MainEntityDifferentTableId], [o].[Description], [o].[EndTime], [o].[StartTime]
FROM (
    SELECT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime]
    FROM [MainEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
    UNION
    SELECT [m0].[Id], [m0].[Description], [m0].[EndTime], [m0].[StartTime]
    FROM [MainEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m0]
) AS [t]
LEFT JOIN [OwnedEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o] ON [t].[Id] = [o].[MainEntityDifferentTableId]");
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

            var query = context.MainEntitiesDifferentTable.FromSqlRaw(
                @"SELECT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime]
FROM [MainEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]");

            var _ = async ? await query.ToListAsync() : query.ToList();
        }

        // just making sure we don't do anything weird here - there is no way to extract temporal information
        // from the FromSql so owned entity will always be treated as a regular query
        AssertSql(
            @"SELECT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime], [o].[MainEntityDifferentTableId], [o].[Description], [o].[EndTime], [o].[StartTime]
FROM (
    SELECT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime]
    FROM [MainEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
) AS [m]
LEFT JOIN [OwnedEntityDifferentTable] AS [o] ON [m].[Id] = [o].[MainEntityDifferentTableId]");
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

            var query = context.MainEntitiesDifferentTable
            .TemporalAsOf(date)
            .Distinct()
            .OrderByDescending(x => x.Id)
            .Take(3);

            var _ = async ? await query.ToListAsync() : query.ToList();
        }

        AssertSql(
            @"@__p_0='3'

SELECT [t0].[Id], [t0].[Description], [t0].[EndTime], [t0].[StartTime], [o].[MainEntityDifferentTableId], [o].[Description], [o].[EndTime], [o].[StartTime]
FROM (
    SELECT TOP(@__p_0) [t].[Id], [t].[Description], [t].[EndTime], [t].[StartTime]
    FROM (
        SELECT DISTINCT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime]
        FROM [MainEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
    ) AS [t]
    ORDER BY [t].[Id] DESC
) AS [t0]
LEFT JOIN [OwnedEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o] ON [t0].[Id] = [o].[MainEntityDifferentTableId]
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

            var query = context.MainEntitiesDifferentTable.TemporalAsOf(date)
                .Join(context.MainEntitiesDifferentTable, x => x.Id, x => x.Id, (o, i) => new { o, i })
                .Distinct().OrderByDescending(x => x.o.Id).Take(3)
                .Join(context.MainEntitiesDifferentTable, xx => xx.o.Id, x => x.Id, (o, i) => new { o, i });

            var _ = async ? await query.ToListAsync() : query.ToList();
        }

        AssertSql(
            @"@__p_0='3'

SELECT [t0].[Id], [t0].[Description], [t0].[EndTime], [t0].[StartTime], [o].[MainEntityDifferentTableId], [o].[Description], [o].[EndTime], [o].[StartTime], [t0].[Id0], [t0].[Description0], [t0].[EndTime0], [t0].[StartTime0], [o0].[MainEntityDifferentTableId], [o0].[Description], [o0].[EndTime], [o0].[StartTime], [m1].[Id], [m1].[Description], [m1].[EndTime], [m1].[StartTime], [o1].[MainEntityDifferentTableId], [o1].[Description], [o1].[EndTime], [o1].[StartTime]
FROM (
    SELECT TOP(@__p_0) [t].[Id], [t].[Description], [t].[EndTime], [t].[StartTime], [t].[Id0], [t].[Description0], [t].[EndTime0], [t].[StartTime0]
    FROM (
        SELECT DISTINCT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime], [m0].[Id] AS [Id0], [m0].[Description] AS [Description0], [m0].[EndTime] AS [EndTime0], [m0].[StartTime] AS [StartTime0]
        FROM [MainEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
        INNER JOIN [MainEntityDifferentTable] AS [m0] ON [m].[Id] = [m0].[Id]
    ) AS [t]
    ORDER BY [t].[Id] DESC
) AS [t0]
INNER JOIN [MainEntityDifferentTable] AS [m1] ON [t0].[Id] = [m1].[Id]
LEFT JOIN [OwnedEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o] ON [t0].[Id] = [o].[MainEntityDifferentTableId]
LEFT JOIN [OwnedEntityDifferentTable] AS [o0] ON [t0].[Id0] = [o0].[MainEntityDifferentTableId]
LEFT JOIN [OwnedEntityDifferentTable] AS [o1] ON [m1].[Id] = [o1].[MainEntityDifferentTableId]
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

            var query = context.MainEntitiesDifferentTable
                .Join(context.MainEntitiesDifferentTable.TemporalAsOf(date), x => x.Id, x => x.Id, (o, i) => new { o, i })
                .Distinct().OrderByDescending(x => x.o.Id).Take(3)
                .Join(context.MainEntitiesDifferentTable, xx => xx.o.Id, x => x.Id, (o, i) => new { o, i });

            var _ = async ? await query.ToListAsync() : query.ToList();
        }

        AssertSql(
            @"@__p_0='3'

SELECT [t0].[Id], [t0].[Description], [t0].[EndTime], [t0].[StartTime], [o].[MainEntityDifferentTableId], [o].[Description], [o].[EndTime], [o].[StartTime], [t0].[Id0], [t0].[Description0], [t0].[EndTime0], [t0].[StartTime0], [o0].[MainEntityDifferentTableId], [o0].[Description], [o0].[EndTime], [o0].[StartTime], [m1].[Id], [m1].[Description], [m1].[EndTime], [m1].[StartTime], [o1].[MainEntityDifferentTableId], [o1].[Description], [o1].[EndTime], [o1].[StartTime]
FROM (
    SELECT TOP(@__p_0) [t].[Id], [t].[Description], [t].[EndTime], [t].[StartTime], [t].[Id0], [t].[Description0], [t].[EndTime0], [t].[StartTime0]
    FROM (
        SELECT DISTINCT [m].[Id], [m].[Description], [m].[EndTime], [m].[StartTime], [m0].[Id] AS [Id0], [m0].[Description] AS [Description0], [m0].[EndTime] AS [EndTime0], [m0].[StartTime] AS [StartTime0]
        FROM [MainEntityDifferentTable] AS [m]
        INNER JOIN [MainEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m0] ON [m].[Id] = [m0].[Id]
    ) AS [t]
    ORDER BY [t].[Id] DESC
) AS [t0]
INNER JOIN [MainEntityDifferentTable] AS [m1] ON [t0].[Id] = [m1].[Id]
LEFT JOIN [OwnedEntityDifferentTable] AS [o] ON [t0].[Id] = [o].[MainEntityDifferentTableId]
LEFT JOIN [OwnedEntityDifferentTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o0] ON [t0].[Id0] = [o0].[MainEntityDifferentTableId]
LEFT JOIN [OwnedEntityDifferentTable] AS [o1] ON [m1].[Id] = [o1].[MainEntityDifferentTableId]
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
                    () => context.MainEntitiesDifferentTable.TemporalAll().ToListAsync())).Message
                : Assert.Throws<InvalidOperationException>(() => context.MainEntitiesDifferentTable.TemporalAll().ToList()).Message;

            Assert.Equal(
                SqlServerStrings.TemporalNavigationExpansionOnlySupportedForAsOf("AsOf"),
                message);
        }
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
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
            @"SELECT [m].[Id], [m].[EndTime], [m].[Name], [m].[StartTime], [m].[EndTime], [m].[OwnedEntity_Name], [m].[OwnedEntity_Number], [m].[StartTime], [m].[OwnedEntity_Nested_Name], [m].[OwnedEntity_Nested_Number]
FROM [MainEntitiesSameTable] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]");
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
            @"SELECT [m].[Id], [m].[Name], [m].[PeriodEnd], [m].[PeriodStart], [o].[MainEntityManyId], [o].[Id], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart]
FROM [MainEntitiesMany] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
LEFT JOIN [OwnedEntityMany] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o] ON [m].[Id] = [o].[MainEntityManyId]
ORDER BY [m].[Id], [o].[MainEntityManyId]");
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
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
            @"SELECT [t].[Id], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [o].[MainEntityManyId], [o].[Id], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart]
FROM (
    SELECT [m].[Id], [m].[Name], [m].[PeriodEnd], [m].[PeriodStart]
    FROM [MainEntitiesMany] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m]
    UNION
    SELECT [m0].[Id], [m0].[Name], [m0].[PeriodEnd], [m0].[PeriodStart]
    FROM [MainEntitiesMany] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [m0]
    WHERE [m0].[Id] < 30
) AS [t]
LEFT JOIN [OwnedEntityMany] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [o] ON [t].[Id] = [o].[MainEntityManyId]
ORDER BY [t].[Id], [o].[MainEntityManyId]");
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

    public class MyContext26451 : DbContext
    {
        public MyContext26451(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<MainEntityDifferentTable> MainEntitiesDifferentTable { get; set; }
        public DbSet<MainEntitySameTable> MainEntitiesSameTable { get; set; }
        public DbSet<MainEntityMany> MainEntitiesMany { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MainEntityDifferentTable>().ToTable("MainEntityDifferentTable", tb => tb.IsTemporal(ttb =>
            {
                ttb.HasPeriodStart("StartTime");
                ttb.HasPeriodEnd("EndTime");
                ttb.UseHistoryTable("ConfHistory");
            }));
            modelBuilder.Entity<MainEntityDifferentTable>().Property(me => me.Id).UseIdentityColumn();
            modelBuilder.Entity<MainEntityDifferentTable>().OwnsOne(me => me.OwnedEntity).WithOwner();
            modelBuilder.Entity<MainEntityDifferentTable>().OwnsOne(me => me.OwnedEntity, oe =>
            {
                oe.ToTable("OwnedEntityDifferentTable", tb => tb.IsTemporal(ttb =>
                {
                    ttb.HasPeriodStart("StartTime");
                    ttb.HasPeriodEnd("EndTime");
                    ttb.UseHistoryTable("OwnedEntityHistory");
                }));
            });

            modelBuilder.Entity<MainEntitySameTable>(eb =>
            {
                eb.ToTable(tb => tb.IsTemporal(ttb =>
                {
                    ttb.HasPeriodStart("StartTime").HasColumnName("StartTime");
                    ttb.HasPeriodEnd("EndTime").HasColumnName("EndTime");
                }));

                eb.OwnsOne(x => x.OwnedEntity, oeb =>
                {
                    oeb.WithOwner();
                    oeb.ToTable(tb => tb.IsTemporal(ttb =>
                    {
                        ttb.HasPeriodStart("StartTime").HasColumnName("StartTime");
                        ttb.HasPeriodEnd("EndTime").HasColumnName("EndTime");
                    }));
                    oeb.OwnsOne(x => x.Nested, neb =>
                    {
                        neb.WithOwner();
                        neb.ToTable(tb => tb.IsTemporal(ttb =>
                        {
                            ttb.HasPeriodStart("StartTime").HasColumnName("StartTime");
                            ttb.HasPeriodEnd("EndTime").HasColumnName("EndTime");
                        }));
                    });
                });
            });

            modelBuilder.Entity<MainEntityMany>(eb =>
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
            @"SELECT [v].[Name], [v].[Capacity], [v].[FuelTank_Discriminator], [v].[End], [v].[FuelType], [v].[Start], [v].[GrainGeometry]
FROM [Vehicles] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [v]
INNER JOIN (
    SELECT [v0].[Name], [v0].[Discriminator], [v0].[End], [v0].[SeatingCapacity], [v0].[Start], [v0].[AttachedVehicleName]
    FROM [Vehicles] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [v0]
    WHERE [v0].[Discriminator] IN (N'PoweredVehicle', N'CompositeVehicle')
) AS [t] ON [v].[Name] = [t].[Name]
WHERE [v].[Capacity] IS NOT NULL AND [v].[FuelTank_Discriminator] IS NOT NULL
UNION
SELECT [v1].[Name], [v1].[Capacity], [v1].[FuelTank_Discriminator], [v1].[End], [v1].[FuelType], [v1].[Start], [v1].[GrainGeometry]
FROM [Vehicles] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [v1]
INNER JOIN (
    SELECT [v2].[Name], [v2].[Computed], [v2].[Description], [v2].[Engine_Discriminator], [v2].[End], [v2].[Start], [t2].[Name] AS [Name0]
    FROM [Vehicles] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [v2]
    INNER JOIN (
        SELECT [v3].[Name], [v3].[Discriminator], [v3].[End], [v3].[SeatingCapacity], [v3].[Start], [v3].[AttachedVehicleName]
        FROM [Vehicles] FOR SYSTEM_TIME AS OF '2000-01-01T00:00:00.0000000' AS [v3]
        WHERE [v3].[Discriminator] IN (N'PoweredVehicle', N'CompositeVehicle')
    ) AS [t2] ON [v2].[Name] = [t2].[Name]
    WHERE [v2].[Engine_Discriminator] IN (N'ContinuousCombustionEngine', N'IntermittentCombustionEngine', N'SolidRocket')
) AS [t1] ON [v1].[Name] = [t1].[Name]
WHERE [v1].[Capacity] IS NOT NULL AND [v1].[FuelTank_Discriminator] IS NOT NULL");
    }

    protected Task<ContextFactory<TransportationContext>> InitializeAsync(
        Action<ModelBuilder> onModelCreating, bool seed = true)
    {
        return InitializeAsync<TransportationContext>(
            onModelCreating, shouldLogCategory: _ => true, seed: seed ? c => c.Seed() : null);
    }

    protected virtual void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vehicle>(
            eb =>
            {
                eb.ToTable(tb => tb.IsTemporal(ttb =>
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
            .ToTable("Vehicles", tb => tb.IsTemporal(ttb =>
            {
                ttb.HasPeriodStart("Start").HasColumnName("Start");
                ttb.HasPeriodEnd("End").HasColumnName("End");
            }));

        modelBuilder.Entity<Operator>().ToTable("Vehicles", tb => tb.IsTemporal(ttb =>
        {
            ttb.HasPeriodStart("Start").HasColumnName("Start");
            ttb.HasPeriodEnd("End").HasColumnName("End");
        }));

        modelBuilder.Entity<OperatorDetails>().ToTable("Vehicles", tb => tb.IsTemporal(ttb =>
        {
            ttb.HasPeriodStart("Start").HasColumnName("Start");
            ttb.HasPeriodEnd("End").HasColumnName("End");
        }));

        modelBuilder.Entity<FuelTank>().ToTable("Vehicles", tb => tb.IsTemporal(ttb =>
        {
            ttb.HasPeriodStart("Start").HasColumnName("Start");
            ttb.HasPeriodEnd("End").HasColumnName("End");
        }));
    }
}
