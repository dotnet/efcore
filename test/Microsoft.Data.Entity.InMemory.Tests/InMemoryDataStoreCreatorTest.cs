// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryDataStoreCreatorTest
    {
        [Fact]
        public async Task Create_is_no_op()
        {
            var creator = new InMemoryDataStoreCreator(Mock.Of<InMemoryDataStore>());

            creator.Create();
            await creator.CreateAsync();
        }

        [Fact]
        public async Task CreateTables_is_no_op()
        {
            var creator = new InMemoryDataStoreCreator(Mock.Of<InMemoryDataStore>());

            creator.CreateTables(Mock.Of<IModel>());
            await creator.CreateTablesAsync(Mock.Of<IModel>());
        }

        [Fact]
        public async Task EnsureCreated_is_no_op_and_returns_false()
        {
            var creator = new InMemoryDataStoreCreator(Mock.Of<InMemoryDataStore>());

            Assert.False(creator.EnsureCreated(Mock.Of<IModel>()));
            Assert.False(await creator.EnsureCreatedAsync(Mock.Of<IModel>()));
        }

        [Fact]
        public async Task Exists_returns_true()
        {
            var creator = new InMemoryDataStoreCreator(Mock.Of<InMemoryDataStore>());

            Assert.True(creator.Exists());
            Assert.True(await creator.ExistsAsync());
        }

        [Fact]
        public async Task HasTables_returns_true()
        {
            var creator = new InMemoryDataStoreCreator(Mock.Of<InMemoryDataStore>());

            Assert.True(creator.HasTables());
            Assert.True(await creator.HasTablesAsync());
        }

        [Fact]
        public async Task Delete_clears_all_in_memory_data()
        {
            await Delete_clears_all_in_memory_data_test(useEnsureMethod: false, async: false);
        }

        [Fact]
        public async Task DeleteAsync_clears_all_in_memory_data()
        {
            await Delete_clears_all_in_memory_data_test(useEnsureMethod: false, async: true);
        }

        [Fact]
        public async Task EnsureDeleted_clears_all_in_memory_data_and_returns_true()
        {
            await Delete_clears_all_in_memory_data_test(useEnsureMethod: true, async: false);
        }

        [Fact]
        public async Task EnsureDeletedAsync_clears_all_in_memory_data_and_returns_true()
        {
            await Delete_clears_all_in_memory_data_test(useEnsureMethod: true, async: true);
        }

        private static async Task Delete_clears_all_in_memory_data_test(bool useEnsureMethod, bool async)
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
                    if (useEnsureMethod)
                    {
                        Assert.True(await context.Database.EnsureDeletedAsync());
                    }
                    else
                    {
                        await context.Database.DeleteAsync();
                    }
                }
                else
                {
                    if (useEnsureMethod)
                    {
                        Assert.True(context.Database.EnsureDeleted());
                    }
                    else
                    {
                        context.Database.Delete();
                    }
                }

                // Exists still returns true because in-memory database is always available
                Assert.True(context.Database.Exists());
                Assert.True(await context.Database.ExistsAsync());

                Assert.Equal(0, await context.Fraggles.CountAsync());
            }

            using (var context = new FraggleContext())
            {
                Assert.Equal(0, await context.Fraggles.CountAsync());
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
    }
}
