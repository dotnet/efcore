// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Services;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryDataStoreCreatorTest
    {
        [Fact]
        public async Task EnsureCreated_returns_true_until_persistent_database_is_used_and_false_thereafter()
        {
            var model = CreateModel();
            var configuration = CreateConfiguration(new DbContextOptions().UseInMemoryStore(persist: true));
            var entityType = model.GetEntityType(typeof(Test));
            var persistentDatabase = new InMemoryDatabase(new[] { new NullLoggerFactory() });

            var inMemoryDataStore = new InMemoryDataStore(configuration, persistentDatabase);

            var creator = new InMemoryDataStoreCreator(inMemoryDataStore);

            Assert.True(creator.EnsureCreated(model));

            var customer = new Test { Id = 1, Name = "Fred" };
            var entityEntry = new ClrStateEntry(configuration, entityType, customer);
            await entityEntry.SetEntityStateAsync(EntityState.Added);
            inMemoryDataStore.SaveChanges(new[] { entityEntry });

            Assert.False(inMemoryDataStore.IsDatabaseCreated());
        }

        [Fact]
        public async Task EnsureCreated_returns_true_until_non_persistent_database_is_used_and_false_thereafter()
        {
            var model = CreateModel();
            var configuration = CreateConfiguration(new DbContextOptions().UseInMemoryStore(persist: false));
            var entityType = model.GetEntityType(typeof(Test));
            var nonPersistentDatabase = new InMemoryDatabase(new[] { new NullLoggerFactory() });
            var inMemoryDataStore = new InMemoryDataStore(configuration, nonPersistentDatabase);

            var creator = new InMemoryDataStoreCreator(inMemoryDataStore);

            Assert.True(creator.EnsureCreated(model));

            var customer = new Test { Id = 2, Name = "George" };
            var entityEntry = new ClrStateEntry(configuration, entityType, customer);
            await entityEntry.SetEntityStateAsync(EntityState.Added);
            inMemoryDataStore.SaveChanges(new[] { entityEntry });

            Assert.False(inMemoryDataStore.IsDatabaseCreated());
        }

        [Fact]
        public async Task EnsureCreatedAsync_returns_true_until_persistent_database_is_used_and_false_thereafter()
        {
            var model = CreateModel();
            var configuration = CreateConfiguration(new DbContextOptions().UseInMemoryStore(persist: true));
            var entityType = model.GetEntityType(typeof(Test));
            var persistentDatabase = new InMemoryDatabase(new[] { new NullLoggerFactory() });
            var inMemoryDataStore = new InMemoryDataStore(configuration, persistentDatabase);

            var creator = new InMemoryDataStoreCreator(inMemoryDataStore);

            Assert.True(await creator.EnsureCreatedAsync(model));

            var customer = new Test { Id = 3, Name = "Bill" };
            var entityEntry = new ClrStateEntry(configuration, entityType, customer);
            await entityEntry.SetEntityStateAsync(EntityState.Added);
            inMemoryDataStore.SaveChanges(new[] { entityEntry });

            Assert.False(await creator.EnsureCreatedAsync(model));
        }


        [Fact]
        public async Task EnsureCreatedAsync_returns_true_until_non_persistent_database_is_used_and_false_thereafter()
        {
            var model = CreateModel();
            var configuration = CreateConfiguration(new DbContextOptions().UseInMemoryStore(persist: false));
            var entityType = model.GetEntityType(typeof(Test));
            var nonPersistentDatabase = new InMemoryDatabase(new[] { new NullLoggerFactory() });
            var inMemoryDataStore = new InMemoryDataStore(configuration, nonPersistentDatabase);

            var creator = new InMemoryDataStoreCreator(inMemoryDataStore);

            Assert.True(await creator.EnsureCreatedAsync(model));

            var customer = new Test { Id = 4, Name = "Percy" };
            var entityEntry = new ClrStateEntry(configuration, entityType, customer);
            await entityEntry.SetEntityStateAsync(EntityState.Added);
            inMemoryDataStore.SaveChanges(new[] { entityEntry });

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
