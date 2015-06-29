// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Tests;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryDatabaseCreatorTest
    {
        [Fact]
        public void EnsureCreated_returns_true_for_first_use_of_persistent_database_and_false_thereafter()
        {
            var serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider();
            var model = CreateModel();
            var creator = new InMemoryDatabaseCreator(CreateStore(serviceProvider, persist: true), model);

            Assert.True(creator.EnsureCreated());
            Assert.False(creator.EnsureCreated());
            Assert.False(creator.EnsureCreated());

            creator = new InMemoryDatabaseCreator(CreateStore(serviceProvider, persist: true), model);

            Assert.False(creator.EnsureCreated());
        }

        [Fact]
        public void EnsureCreated_returns_true_for_first_use_of_non_persistent_database_and_false_thereafter()
        {
            var serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider();
            var model = CreateModel();
            var creator = new InMemoryDatabaseCreator(CreateStore(serviceProvider, persist: false), model);

            Assert.True(creator.EnsureCreated());
            Assert.False(creator.EnsureCreated());
            Assert.False(creator.EnsureCreated());

            creator = new InMemoryDatabaseCreator(CreateStore(serviceProvider, persist: false), model);

            Assert.True(creator.EnsureCreated());
            Assert.False(creator.EnsureCreated());
            Assert.False(creator.EnsureCreated());
        }

        [Fact]
        public async Task EnsureCreatedAsync_returns_true_for_first_use_of_persistent_database_and_false_thereafter()
        {
            var serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider();
            var model = CreateModel();
            var creator = new InMemoryDatabaseCreator(CreateStore(serviceProvider, persist: true), model);

            Assert.True(await creator.EnsureCreatedAsync());
            Assert.False(await creator.EnsureCreatedAsync());
            Assert.False(await creator.EnsureCreatedAsync());

            creator = new InMemoryDatabaseCreator(CreateStore(serviceProvider, persist: true), model);

            Assert.False(await creator.EnsureCreatedAsync());
        }

        [Fact]
        public async Task EnsureCreatedAsync_returns_true_for_first_use_of_non_persistent_database_and_false_thereafter()
        {
            var serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider();
            var model = CreateModel();
            var creator = new InMemoryDatabaseCreator(CreateStore(serviceProvider, persist: false), model);

            Assert.True(await creator.EnsureCreatedAsync());
            Assert.False(await creator.EnsureCreatedAsync());
            Assert.False(await creator.EnsureCreatedAsync());

            creator = new InMemoryDatabaseCreator(CreateStore(serviceProvider, persist: false), model);

            Assert.True(await creator.EnsureCreatedAsync());
            Assert.False(await creator.EnsureCreatedAsync());
            Assert.False(await creator.EnsureCreatedAsync());
        }

        private static IInMemoryDatabase CreateStore(IServiceProvider serviceProvider, bool persist)
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase(persist: persist);

            return InMemoryTestHelpers.Instance.CreateContextServices(serviceProvider, optionsBuilder.Options).GetRequiredService<IInMemoryDatabase>();
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
                context.Fraggles.AddRange(new Fraggle { Id = 1, Name = "Gobo" }, new Fraggle { Id = 2, Name = "Monkey" }, new Fraggle { Id = 3, Name = "Red" }, new Fraggle { Id = 4, Name = "Wembley" }, new Fraggle { Id = 5, Name = "Boober" }, new Fraggle { Id = 6, Name = "Uncle Traveling Matt" });

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

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseInMemoryDatabase();
            }
        }

        private class Fraggle
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private static IModel CreateModel()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            modelBuilder.Entity<Test>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(c => c.Name);
                });

            return modelBuilder.Model;
        }

        private class Test
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
