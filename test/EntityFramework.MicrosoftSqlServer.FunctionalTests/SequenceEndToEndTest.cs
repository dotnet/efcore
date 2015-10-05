// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestUtilities.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    [SqlServerCondition(SqlServerCondition.SupportsSequences)]
    public class SequenceEndToEndTest
    {
        [ConditionalFact]
        public void Can_use_sequence_end_to_end()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .BuildServiceProvider();

            using (var context = new BronieContext(serviceProvider, "Bronies"))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            AddEntities(serviceProvider);
            AddEntities(serviceProvider);

            // Use a different service provider so a different generator is used but with
            // the same server sequence.
            serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .BuildServiceProvider();

            AddEntities(serviceProvider);

            using (var context = new BronieContext(serviceProvider, "Bronies"))
            {
                var pegasuses = context.Pegasuses.ToList();

                for (var i = 0; i < 10; i++)
                {
                    Assert.Equal(3, pegasuses.Count(p => p.Name == "Rainbow Dash " + i));
                    Assert.Equal(3, pegasuses.Count(p => p.Name == "Fluttershy " + i));
                }
            }
        }

        private static void AddEntities(IServiceProvider serviceProvider)
        {
            using (var context = new BronieContext(serviceProvider, "Bronies"))
            {
                for (var i = 0; i < 10; i++)
                {
                    context.Add(new Pegasus { Name = "Rainbow Dash " + i });
                    context.Add(new Pegasus { Name = "Fluttershy " + i });
                }

                context.SaveChanges();
            }
        }

        [ConditionalFact]
        public async Task Can_use_sequence_end_to_end_async()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .BuildServiceProvider();

            using (var context = new BronieContext(serviceProvider, "BroniesAsync"))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            await AddEntitiesAsync(serviceProvider, "BroniesAsync");
            await AddEntitiesAsync(serviceProvider, "BroniesAsync");

            // Use a different service provider so a different generator is used but with
            // the same server sequence.
            serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .BuildServiceProvider();

            await AddEntitiesAsync(serviceProvider, "BroniesAsync");

            using (var context = new BronieContext(serviceProvider, "BroniesAsync"))
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
            using (var context = new BronieContext(serviceProvider, databaseName))
            {
                for (var i = 0; i < 10; i++)
                {
                    context.Add(new Pegasus { Name = "Rainbow Dash " + i });
                    context.Add(new Pegasus { Name = "Fluttershy " + i });
                }

                await context.SaveChangesAsync();
            }
        }

        // [ConditionalFact] Currently disabled due to GitHub issue #266
        public async Task Can_use_sequence_end_to_end_from_multiple_contexts_concurrently_async()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .BuildServiceProvider();

            using (var context = new BronieContext(serviceProvider, "ManyBronies"))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            const int threadCount = 50;

            var tests = new Func<Task>[threadCount];
            for (var i = 0; i < threadCount; i++)
            {
                var closureProvider = serviceProvider;
                tests[i] = () => AddEntitiesAsync(closureProvider, "ManyBronies");
            }

            var tasks = tests.Select(Task.Run).ToArray();

            foreach (var t in tasks)
            {
                await t;
            }

            using (var context = new BronieContext(serviceProvider, "ManyBronies"))
            {
                var pegasuses = await context.Pegasuses.ToListAsync();

                for (var i = 0; i < 50; i++)
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
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .BuildServiceProvider();

            using (var context = new BronieContext(serviceProvider, "ExplicitBronies"))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            AddEntitiesWithIds(serviceProvider, 0);
            AddEntitiesWithIds(serviceProvider, 2);

            // Use a different service provider so a different generator is used but with
            // the same server sequence.
            serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .BuildServiceProvider();

            AddEntitiesWithIds(serviceProvider, 4);

            using (var context = new BronieContext(serviceProvider, "ExplicitBronies"))
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

        private static void AddEntitiesWithIds(IServiceProvider serviceProvider, int idOffset)
        {
            using (var context = new BronieContext(serviceProvider, "ExplicitBronies"))
            {
                for (var i = 1; i < 11; i++)
                {
                    context.Add(new Pegasus { Name = "Rainbow Dash " + i, Identifier = i * 100 + idOffset });
                    context.Add(new Pegasus { Name = "Fluttershy " + i, Identifier = i * 100 + idOffset + 1 });
                }

                context.SaveChanges();
            }
        }

        private class BronieContext : DbContext
        {
            private readonly string _databaseName;

            public BronieContext(IServiceProvider serviceProvider, string databaseName)
                : base(serviceProvider)
            {
                _databaseName = databaseName;
            }

            public DbSet<Pegasus> Pegasuses { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlServer(SqlServerTestStore.CreateConnectionString(_databaseName));
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Pegasus>(b =>
                    {
                        b.HasKey(e => e.Identifier);
                        b.Property(e => e.Identifier).ForSqlServerUseSequenceHiLo();
                    });
            }
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
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .BuildServiceProvider();

            using (var context = new NullableBronieContext(serviceProvider, "NullableBronies", useSequence: true))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            AddEntitiesNullable(serviceProvider, "NullableBronies", useSequence: true);
            AddEntitiesNullable(serviceProvider, "NullableBronies", useSequence: true);
            AddEntitiesNullable(serviceProvider, "NullableBronies", useSequence: true);

            using (var context = new NullableBronieContext(serviceProvider, "NullableBronies", useSequence: true))
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
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .BuildServiceProvider();

            using (var context = new NullableBronieContext(serviceProvider, "IdentityBronies", useSequence: false))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            AddEntitiesNullable(serviceProvider, "IdentityBronies", false);
            AddEntitiesNullable(serviceProvider, "IdentityBronies", false);
            AddEntitiesNullable(serviceProvider, "IdentityBronies", false);

            using (var context = new NullableBronieContext(serviceProvider, "IdentityBronies", useSequence: false))
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
            using (var context = new NullableBronieContext(serviceProvider, databaseName, useSequence))
            {
                for (var i = 0; i < 10; i++)
                {
                    context.Add(new Unicon { Name = "Twilight Sparkle " + i });
                    context.Add(new Unicon { Name = "Rarity " + i });
                }

                context.SaveChanges();
            }
        }

        private class NullableBronieContext : DbContext
        {
            private readonly string _databaseName;
            private readonly bool _useSequence;

            public NullableBronieContext(IServiceProvider serviceProvider, string databaseName, bool useSequence)
                : base(serviceProvider)
            {
                _databaseName = databaseName;
                _useSequence = useSequence;
            }

            public DbSet<Unicon> Unicons { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlServer(SqlServerTestStore.CreateConnectionString(_databaseName));
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Unicon>(b =>
                    {
                        b.HasKey(e => e.Identifier);
                        if (_useSequence)
                        {
                            b.Property(e => e.Identifier).ForSqlServerUseSequenceHiLo();
                        }
                        else
                        {
                            b.Property(e => e.Identifier).UseSqlServerIdentityColumn();
                        }
                    });
            }
        }

        private class Unicon
        {
            public int? Identifier { get; set; }
            public string Name { get; set; }
        }
    }
}
