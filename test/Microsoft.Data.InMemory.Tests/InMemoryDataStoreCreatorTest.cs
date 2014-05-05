// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.DependencyInjection;
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

            protected override void OnConfiguring(DbContextOptions builder)
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
