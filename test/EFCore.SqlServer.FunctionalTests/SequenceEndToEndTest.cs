// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class SequenceEndToEndTest : IAsyncLifetime
{
    [ConditionalFact]
    public void Can_use_sequence_end_to_end()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlServer()
            .BuildServiceProvider(validateScopes: true);

        using (var context = new BronieContext(serviceProvider, TestStore.Name))
        {
            context.Database.EnsureCreatedResiliently();
        }

        AddEntities(serviceProvider, TestStore.Name);
        AddEntities(serviceProvider, TestStore.Name);

        // Use a different service provider so a different generator is used but with
        // the same server sequence.
        serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlServer()
            .BuildServiceProvider(validateScopes: true);

        AddEntities(serviceProvider, TestStore.Name);

        using (var context = new BronieContext(serviceProvider, TestStore.Name))
        {
            var pegasuses = context.Pegasuses.ToList();

            for (var i = 0; i < 10; i++)
            {
                Assert.Equal(3, pegasuses.Count(p => p.Name == "Rainbow Dash " + i));
                Assert.Equal(3, pegasuses.Count(p => p.Name == "Fluttershy " + i));
            }
        }
    }

    private static void AddEntities(IServiceProvider serviceProvider, string name)
    {
        using var context = new BronieContext(serviceProvider, name);
        for (var i = 0; i < 10; i++)
        {
            context.Add(
                new Pegasus { Name = "Rainbow Dash " + i });
            context.Add(
                new Pegasus { Name = "Fluttershy " + i });
        }

        context.SaveChanges();
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.IsNotCI)]
    public void Can_use_sequence_end_to_end_on_multiple_databases()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlServer()
            .BuildServiceProvider(validateScopes: true);

        var dbOne = TestStore.Name + "1";
        var dbTwo = TestStore.Name + "2";

        foreach (var dbName in new[] { dbOne, dbTwo })
        {
            using var context = new BronieContext(serviceProvider, dbName);
            context.Database.EnsureDeleted();
            Thread.Sleep(100);
            context.Database.EnsureCreatedResiliently();
        }

        AddEntitiesToMultipleContexts(serviceProvider, dbOne, dbTwo);
        AddEntitiesToMultipleContexts(serviceProvider, dbOne, dbTwo);

        // Use a different service provider so a different generator is used but with
        // the same server sequence.
        serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlServer()
            .BuildServiceProvider(validateScopes: true);

        AddEntitiesToMultipleContexts(serviceProvider, dbOne, dbTwo);

        foreach (var dbName in new[] { dbOne, dbTwo })
        {
            using var context = new BronieContext(serviceProvider, dbName);
            var pegasuses = context.Pegasuses.ToList();

            for (var i = 0; i < 29; i++)
            {
                Assert.Equal(
                    dbName.EndsWith("1", StringComparison.Ordinal) ? 3 : 0,
                    pegasuses.Count(p => p.Name == "Rainbow Dash " + i));
                Assert.Equal(3, pegasuses.Count(p => p.Name == "Fluttershy " + i));
            }
        }
    }

    private static void AddEntitiesToMultipleContexts(
        IServiceProvider serviceProvider,
        string dbName1,
        string dbName2)
    {
        using var context1 = new BronieContext(serviceProvider, dbName1);
        using var context2 = new BronieContext(serviceProvider, dbName2);
        for (var i = 0; i < 29; i++)
        {
            context1.Add(
                new Pegasus { Name = "Rainbow Dash " + i });

            context2.Add(
                new Pegasus { Name = "Fluttershy " + i });

            context1.Add(
                new Pegasus { Name = "Fluttershy " + i });
        }

        context1.SaveChanges();
        context2.SaveChanges();
    }

    [ConditionalFact]
    public async Task Can_use_sequence_end_to_end_async()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlServer()
            .BuildServiceProvider(validateScopes: true);

        using (var context = new BronieContext(serviceProvider, TestStore.Name))
        {
            context.Database.EnsureCreatedResiliently();
        }

        await AddEntitiesAsync(serviceProvider, TestStore.Name);
        await AddEntitiesAsync(serviceProvider, TestStore.Name);

        // Use a different service provider so a different generator is used but with
        // the same server sequence.
        serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlServer()
            .BuildServiceProvider(validateScopes: true);

        await AddEntitiesAsync(serviceProvider, TestStore.Name);

        using (var context = new BronieContext(serviceProvider, TestStore.Name))
        {
            var pegasuses = await context.Pegasuses.ToListAsync();

            for (var i = 0; i < 10; i++)
            {
                Assert.Equal(3, pegasuses.Count(p => p.Name == "Rainbow Dash " + i));
                Assert.Equal(3, pegasuses.Count(p => p.Name == "Fluttershy " + i));
            }
        }
    }

    private static async Task AddEntitiesAsync(IServiceProvider serviceProvider, string databaseName)
    {
        using var context = new BronieContext(serviceProvider, databaseName);
        for (var i = 0; i < 10; i++)
        {
            await context.AddAsync(
                new Pegasus { Name = "Rainbow Dash " + i });
            await context.AddAsync(
                new Pegasus { Name = "Fluttershy " + i });
        }

        await context.SaveChangesAsync();
    }

    [ConditionalFact]
    public async Task Can_use_sequence_end_to_end_from_multiple_contexts_concurrently_async()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlServer()
            .BuildServiceProvider(validateScopes: true);

        using (var context = new BronieContext(serviceProvider, TestStore.Name))
        {
            context.Database.EnsureCreatedResiliently();
        }

        const int threadCount = 50;

        var tests = new Func<Task>[threadCount];
        for (var i = 0; i < threadCount; i++)
        {
            var closureProvider = serviceProvider;
            tests[i] = () => AddEntitiesAsync(closureProvider, TestStore.Name);
        }

        var tasks = tests.Select(Task.Run).ToArray();

        foreach (var t in tasks)
        {
            await t;
        }

        using (var context = new BronieContext(serviceProvider, TestStore.Name))
        {
            var pegasuses = await context.Pegasuses.ToListAsync();

            for (var i = 0; i < 10; i++)
            {
                Assert.Equal(threadCount, pegasuses.Count(p => p.Name == "Rainbow Dash " + i));
                Assert.Equal(threadCount, pegasuses.Count(p => p.Name == "Fluttershy " + i));
            }
        }
    }

    [ConditionalFact]
    public void Can_use_explicit_values()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlServer()
            .BuildServiceProvider(validateScopes: true);

        using (var context = new BronieContext(serviceProvider, TestStore.Name))
        {
            context.Database.EnsureCreatedResiliently();
        }

        AddEntitiesWithIds(serviceProvider, 0, TestStore.Name);
        AddEntitiesWithIds(serviceProvider, 2, TestStore.Name);

        // Use a different service provider so a different generator is used but with
        // the same server sequence.
        serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlServer()
            .BuildServiceProvider(validateScopes: true);

        AddEntitiesWithIds(serviceProvider, 4, TestStore.Name);

        using (var context = new BronieContext(serviceProvider, TestStore.Name))
        {
            var pegasuses = context.Pegasuses.ToList();

            for (var i = 1; i < 11; i++)
            {
                Assert.Equal(3, pegasuses.Count(p => p.Name == "Rainbow Dash " + i));
                Assert.Equal(3, pegasuses.Count(p => p.Name == "Fluttershy " + i));

                for (var j = 0; j < 6; j++)
                {
                    pegasuses.Single(p => p.Identifier == i * 100 + j);
                }
            }
        }
    }

    private static void AddEntitiesWithIds(IServiceProvider serviceProvider, int idOffset, string name)
    {
        using var context = new BronieContext(serviceProvider, name);
        for (var i = 1; i < 11; i++)
        {
            context.Add(
                new Pegasus { Name = "Rainbow Dash " + i, Identifier = i * 100 + idOffset });
            context.Add(
                new Pegasus { Name = "Fluttershy " + i, Identifier = i * 100 + idOffset + 1 });
        }

        context.SaveChanges();
    }

    private class BronieContext(IServiceProvider serviceProvider, string databaseName) : DbContext
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly string _databaseName = databaseName;

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Pegasus> Pegasuses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(_serviceProvider)
                .UseSqlServer(SqlServerTestStore.CreateConnectionString(_databaseName), b => b.ApplyConfiguration());

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Pegasus>(
                b =>
                {
                    b.HasKey(e => e.Identifier);
                    b.Property(e => e.Identifier).UseHiLo();
                });
    }

    private class Pegasus
    {
        public int Identifier { get; set; }
        public string Name { get; set; }
    }

    [ConditionalFact] // Issue #478
    public void Can_use_sequence_with_nullable_key_end_to_end()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlServer()
            .BuildServiceProvider(validateScopes: true);

        using (var context = new NullableBronieContext(serviceProvider, TestStore.Name, true))
        {
            context.Database.EnsureCreatedResiliently();
        }

        AddEntitiesNullable(serviceProvider, TestStore.Name, true);
        AddEntitiesNullable(serviceProvider, TestStore.Name, true);
        AddEntitiesNullable(serviceProvider, TestStore.Name, true);

        using (var context = new NullableBronieContext(serviceProvider, TestStore.Name, true))
        {
            var pegasuses = context.Unicons.ToList();

            for (var i = 0; i < 10; i++)
            {
                Assert.Equal(3, pegasuses.Count(p => p.Name == "Twilight Sparkle " + i));
                Assert.Equal(3, pegasuses.Count(p => p.Name == "Rarity " + i));
            }
        }
    }

    [ConditionalFact] // Issue #478
    public void Can_use_identity_with_nullable_key_end_to_end()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlServer()
            .BuildServiceProvider(validateScopes: true);

        using (var context = new NullableBronieContext(serviceProvider, TestStore.Name, false))
        {
            context.Database.EnsureCreatedResiliently();
        }

        AddEntitiesNullable(serviceProvider, TestStore.Name, false);
        AddEntitiesNullable(serviceProvider, TestStore.Name, false);
        AddEntitiesNullable(serviceProvider, TestStore.Name, false);

        using (var context = new NullableBronieContext(serviceProvider, TestStore.Name, false))
        {
            var pegasuses = context.Unicons.ToList();

            for (var i = 0; i < 10; i++)
            {
                Assert.Equal(3, pegasuses.Count(p => p.Name == "Twilight Sparkle " + i));
                Assert.Equal(3, pegasuses.Count(p => p.Name == "Rarity " + i));
            }
        }
    }

    private static void AddEntitiesNullable(IServiceProvider serviceProvider, string databaseName, bool useSequence)
    {
        using var context = new NullableBronieContext(serviceProvider, databaseName, useSequence);
        for (var i = 0; i < 10; i++)
        {
            context.Add(
                new Unicon { Name = "Twilight Sparkle " + i });
            context.Add(
                new Unicon { Name = "Rarity " + i });
        }

        context.SaveChanges();
    }

    private class NullableBronieContext(IServiceProvider serviceProvider, string databaseName, bool useSequence) : DbContext
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly string _databaseName = databaseName;
        private readonly bool _useSequence = useSequence;

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Unicon> Unicons { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(_serviceProvider)
                .UseSqlServer(SqlServerTestStore.CreateConnectionString(_databaseName), b => b.ApplyConfiguration());

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Unicon>(
                b =>
                {
                    b.HasKey(e => e.Identifier);
                    if (_useSequence)
                    {
                        b.Property(e => e.Identifier).UseHiLo();
                    }
                    else
                    {
                        b.Property(e => e.Identifier).UseIdentityColumn();
                    }
                });
    }

    private class Unicon
    {
        public int? Identifier { get; set; }
        public string Name { get; set; }
    }

    protected SqlServerTestStore TestStore { get; private set; }

    public async Task InitializeAsync()
        => TestStore = await SqlServerTestStore.CreateInitializedAsync("SequenceEndToEndTest");

    public Task DisposeAsync()
    {
        TestStore.Dispose();
        return Task.CompletedTask;
    }
}
