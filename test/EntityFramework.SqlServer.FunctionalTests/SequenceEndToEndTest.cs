// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Services;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Advanced;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SequenceEndToEndTest
    {
        [Fact]
        public void Can_use_sequence_end_to_end()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .UseLoggerFactory<NullLoggerFactory>()
                .ServiceCollection
                .BuildServiceProvider();

            using (var context = new BronieContext(serviceProvider, "Bronies"))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            AddEntities(serviceProvider);
            AddEntities(serviceProvider);

            // Use a different service provider so a different generator pool is used but with
            // the same server sequence.
            serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .UseLoggerFactory<NullLoggerFactory>()
                .ServiceCollection
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

        [Fact]
        public async Task Can_use_sequence_end_to_end_async()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .UseLoggerFactory<NullLoggerFactory>()
                .ServiceCollection
                .BuildServiceProvider();

            using (var context = new BronieContext(serviceProvider, "BroniesAsync"))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            await AddEntitiesAsync(serviceProvider, "BroniesAsync");
            await AddEntitiesAsync(serviceProvider, "BroniesAsync");

            // Use a different service provider so a different generator pool is used but with
            // the same server sequence.
            serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .UseLoggerFactory<NullLoggerFactory>()
                .ServiceCollection
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
                    await context.AddAsync(new Pegasus { Name = "Rainbow Dash " + i });
                    await context.AddAsync(new Pegasus { Name = "Fluttershy " + i });
                }

                await context.SaveChangesAsync();
            }
        }

        // [Fact] Currently disabled due to GitHub issue #266
        public async Task Can_use_sequence_end_to_end_from_multiple_contexts_concurrently_async()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .UseLoggerFactory<NullLoggerFactory>()
                .ServiceCollection
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

        [Fact]
        public void Can_use_explicit_values()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .UseLoggerFactory<NullLoggerFactory>()
                .ServiceCollection
                .BuildServiceProvider();

            using (var context = new BronieContext(serviceProvider, "ExplicitBronies"))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            AddEntitiesWithIds(serviceProvider, 0);
            AddEntitiesWithIds(serviceProvider, 2);

            // Use a different service provider so a different generator pool is used but with
            // the same server sequence.
            serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .UseLoggerFactory<NullLoggerFactory>()
                .ServiceCollection
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

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseSqlServer(SqlServerTestDatabase.CreateConnectionString(_databaseName));
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Pegasus>(b =>
                    {
                        b.Key(e => e.Identifier);
                        b.Property(e => e.Identifier).GenerateValuesUsingSequence();
                    });
            }
        }

        private class Pegasus
        {
            public int Identifier { get; set; }
            public string Name { get; set; }
        }
    }
}
