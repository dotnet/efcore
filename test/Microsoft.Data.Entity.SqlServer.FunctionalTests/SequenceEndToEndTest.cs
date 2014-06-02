// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.DependencyInjection;
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
                .ServiceCollection
                .BuildServiceProvider();

            using (var context = new BronieContext(serviceProvider, "Bronies"))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                // TODO: Integrate sequence generation into Migrations
                CreateDatabaseSequence(context, context.Database.AsRelational().Connection);
            }

            AddEntities(serviceProvider);
            AddEntities(serviceProvider);

            // Use a different service provider so a different generator pool is used but with
            // the same server sequence.
            serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection
                .BuildServiceProvider();

            AddEntities(serviceProvider);

            using (var context = new BronieContext(serviceProvider, "Bronies"))
            {
                var pegasuses = context.Pegasuses.ToList();

                for (var i = 0; i < 50; i++)
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
                for (var i = 0; i < 50; i++)
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
                .ServiceCollection
                .BuildServiceProvider();

            using (var context = new BronieContext(serviceProvider, "BroniesAsync"))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                // TODO: Integrate sequence generation into Migrations
                CreateDatabaseSequence(context, context.Database.AsRelational().Connection);
            }

            await AddEntitiesAsync(serviceProvider, "BroniesAsync");
            await AddEntitiesAsync(serviceProvider, "BroniesAsync");

            // Use a different service provider so a different generator pool is used but with
            // the same server sequence.
            serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection
                .BuildServiceProvider();

            await AddEntitiesAsync(serviceProvider, "BroniesAsync");

            using (var context = new BronieContext(serviceProvider, "BroniesAsync"))
            {
                var pegasuses = await context.Pegasuses.ToListAsync();

                for (var i = 0; i < 50; i++)
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
                for (var i = 0; i < 50; i++)
                {
                    await context.AddAsync(new Pegasus { Name = "Rainbow Dash " + i });
                    await context.AddAsync(new Pegasus { Name = "Fluttershy " + i });
                }

                await context.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task Can_use_sequence_end_to_end_from_multiple_contexts_concurrently_async()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection
                .BuildServiceProvider();

            using (var context = new BronieContext(serviceProvider, "ManyBronies"))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                // TODO: Integrate sequence generation into Migrations
                CreateDatabaseSequence(context, context.Database.AsRelational().Connection);
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

        private static void CreateDatabaseSequence(BronieContext context, RelationalConnection storeConnection)
        {
            var executor = new SqlStatementExecutor();

            var operations = new SqlServerSequenceValueGeneratorFactory(executor)
                .GetUpMigrationOperations(context.Model.GetEntityType(typeof(Pegasus))
                    .GetProperty("Identifier"));

            var sql = new SqlServerMigrationOperationSqlGenerator(new SqlServerTypeMapper())
                .Generate(operations, generateIdempotentSql: false);

            // TODO: Should be able to get relational connection without cast
            var connection = storeConnection.DbConnection;

            executor.ExecuteNonQuery(connection, sql);
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
                options.UseSqlServer(TestDatabase.CreateConnectionString(_databaseName));
            }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                builder
                    .Entity<Pegasus>()
                    .Key(e => e.Identifier)
                    .Properties(s => s.Property(e => e.Identifier).UseStoreSequence("PegasusSequence", 11));
            }
        }

        private class Pegasus
        {
            public int Identifier { get; set; }
            public string Name { get; set; }
        }
    }
}
