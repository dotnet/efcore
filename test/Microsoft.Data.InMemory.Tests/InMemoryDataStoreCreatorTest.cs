// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.InMemory.Tests
{
    public class InMemoryDataStoreCreatorTest
    {
        [Fact]
        public async Task Create_is_no_op()
        {
            var creator = new InMemoryDataStoreCreator(Mock.Of<InMemoryDataStore>());

            creator.Create(Mock.Of<IModel>());
            await creator.CreateAsync(Mock.Of<IModel>());
        }

        [Fact]
        public async Task Exists_returns_true()
        {
            var creator = new InMemoryDataStoreCreator(Mock.Of<InMemoryDataStore>());

            Assert.True(creator.Exists());
            Assert.True(await creator.ExistsAsync());
        }

        [Fact]
        public async Task Delete_clears_all_in_memory_data()
        {
            await Delete_clears_all_in_memory_data_test(async: false);
        }

        [Fact]
        public async Task DeleteAsync_clears_all_in_memory_data()
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
                    await context.Database.DeleteAsync();    
                }
                else
                {
                    context.Database.Delete();    
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

            protected override void OnConfiguring(EntityConfigurationBuilder builder)
            {
                builder.UseInMemoryStore();
            }
        }

        private class Fraggle
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
