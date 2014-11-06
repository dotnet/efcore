// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.Logging;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryDataStoreCreatorTest
    {
        [Fact]
        public void EnsureCreated_returns_true_for_first_use_of_persistent_database_and_false_thereafter()
        {
            var model = CreateModel();
            var configuration = CreateConfiguration(new DbContextOptions().UseInMemoryStore(persist: true));
            var entityType = model.GetEntityType(typeof(Test));
            var persistentDatabase = new InMemoryDatabase(new LoggerFactory());

            var inMemoryDataStore = new InMemoryDataStore(configuration, persistentDatabase, new LoggerFactory());

            var creator = new InMemoryDataStoreCreator(inMemoryDataStore);

            Assert.True(creator.EnsureCreated(model));
            Assert.False(creator.EnsureCreated(model));
            Assert.False(creator.EnsureCreated(model));
        }

        [Fact]
        public void EnsureCreated_returns_true_for_first_use_of_non_persistent_database_and_false_thereafter()
        {
            var model = CreateModel();
            var configuration = CreateConfiguration(new DbContextOptions().UseInMemoryStore(persist: false));
            var entityType = model.GetEntityType(typeof(Test));
            var nonPersistentDatabase = new InMemoryDatabase(new LoggerFactory());
            var inMemoryDataStore = new InMemoryDataStore(configuration, nonPersistentDatabase, new LoggerFactory());

            var creator = new InMemoryDataStoreCreator(inMemoryDataStore);

            Assert.True(creator.EnsureCreated(model));
            Assert.False(creator.EnsureCreated(model));
            Assert.False(creator.EnsureCreated(model));
        }

        [Fact]
        public async Task EnsureCreatedAsync_returns_true_for_first_use_of_persistent_database_and_false_thereafter()
        {
            var model = CreateModel();
            var configuration = CreateConfiguration(new DbContextOptions().UseInMemoryStore(persist: true));
            var entityType = model.GetEntityType(typeof(Test));
            var persistentDatabase = new InMemoryDatabase(new LoggerFactory());
            var inMemoryDataStore = new InMemoryDataStore(configuration, persistentDatabase, new LoggerFactory());

            var creator = new InMemoryDataStoreCreator(inMemoryDataStore);

            Assert.True(await creator.EnsureCreatedAsync(model));
            Assert.False(await creator.EnsureCreatedAsync(model));
            Assert.False(await creator.EnsureCreatedAsync(model));
        }

        [Fact]
        public async Task EnsureCreatedAsync_returns_true_for_first_use_of_non_persistent_database_and_false_thereafter()
        {
            var model = CreateModel();
            var configuration = CreateConfiguration(new DbContextOptions().UseInMemoryStore(persist: false));
            var entityType = model.GetEntityType(typeof(Test));
            var nonPersistentDatabase = new InMemoryDatabase(new LoggerFactory());
            var inMemoryDataStore = new InMemoryDataStore(configuration, nonPersistentDatabase, new LoggerFactory());

            var creator = new InMemoryDataStoreCreator(inMemoryDataStore);

            Assert.True(await creator.EnsureCreatedAsync(model));
            Assert.False(await creator.EnsureCreatedAsync(model));
            Assert.False(await creator.EnsureCreatedAsync(model));
        }

        [Fact]
        public async Task EnsureDeleted_clears_all_in_memory_data_and_returns_true()
        {
            await Delete_clears_all_in_memory_data_test(async: false);
        }

        [Fact]
        public async Task EnsureDeletedAsync_clears_all_in_memory_data_and_returns_true()
        {
            await Delete_clears_all_in_memory_data_test(async: true);
        }

        private static async Task Delete_clears_all_in_memory_data_test(bool async)
        {
            using (var context = new FraggleContext())
            {
                context.Fraggles.AddRange(
                    new[]
                        {
                            new Fraggle { Id = 1, Name = "Gobo" },
                            new Fraggle { Id = 2, Name = "Monkey" },
                            new Fraggle { Id = 3, Name = "Red" },
                            new Fraggle { Id = 4, Name = "Wembley" },
                            new Fraggle { Id = 5, Name = "Boober" },
                            new Fraggle { Id = 6, Name = "Uncle Traveling Matt" }
                        });

                await context.SaveChangesAsync();
            }

            using (var context = new FraggleContext())
            {
                Assert.Equal(6, await context.Fraggles.CountAsync());

                if (async)
                {
                    Assert.True(await context.Database.EnsureDeletedAsync());
                }
                else
                {
                    Assert.True(context.Database.EnsureDeleted());
                }

                Assert.Equal(0, await context.Fraggles.CountAsync());
            }

            using (var context = new FraggleContext())
            {
                Assert.Equal(0, await context.Fraggles.CountAsync());

                if (async)
                {
                    Assert.False(await context.Database.EnsureDeletedAsync());
                }
                else
                {
                    Assert.False(context.Database.EnsureDeleted());
                }
            }
        }

        private class FraggleContext : DbContext
        {
            public DbSet<Fraggle> Fraggles { get; set; }

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseInMemoryStore();
            }
        }

        private class Fraggle
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private static IModel CreateModel()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<Test>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(c => c.Name);
                });

            return model;
        }

        private class Test
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private static DbContextConfiguration CreateConfiguration(DbContextOptions options)
        {
            return new DbContext(options).Configuration;
        }
    }
}
