// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Redis.Extensions;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;

namespace Microsoft.Data.Entity.Redis.FunctionalTests
{
    public class SimpleTest : IClassFixture<RedisFixture>, IDisposable
    {
        // [Fact] Disabled due to #1038
        public void Add_modify_and_delete_simple_poco()
        {
            using (var context = CreateContext())
            {
                var simplePoco = context.Set<SimplePoco>().Add(
                    new SimplePoco
                    {
                        PocoKey = 100,
                        Name = "A. Name",
                    });
                var changes = context.SaveChanges();
                Assert.Equal(1, changes);

                simplePoco.Name = "Updated Name";
                changes = context.SaveChanges();
                Assert.Equal(1, changes);

                context.Set<SimplePoco>().Remove(simplePoco);
                changes = context.SaveChanges();
                Assert.Equal(1, changes);
            }
        }

        // [Fact] Disabled due to #1038
        public async Task Add_modify_and_delete_simple_poco_async()
        {
            using (var context = CreateContext())
            {
                var simplePoco = context.Set<SimplePoco>().Add(
                    new SimplePoco
                        {
                            PocoKey = 100,
                            Name = "A. Name",
                        });
                var changes = await context.SaveChangesAsync();
                Assert.Equal(1, changes);

                simplePoco.Name = "Updated Name";
                changes = await context.SaveChangesAsync();
                Assert.Equal(1, changes);

                context.Set<SimplePoco>().Remove(simplePoco);
                changes = await context.SaveChangesAsync();
                Assert.Equal(1, changes);
            }
        }

        // [Fact] Disabled due to #1038
        public void Add_modify_and_delete_simple_poco_together()
        {
            using (var context = CreateContext())
            {
                var simplePoco = context.Set<SimplePoco>().Add(
                    new SimplePoco
                        {
                            PocoKey = 100,
                            Name = "A. Name",
                        });
                var simplePoco2 = context.Set<SimplePoco>().Add(
                    new SimplePoco
                        {
                            PocoKey = 101,
                            Name = "B. Name",
                        });
                var changes = context.SaveChanges();
                Assert.Equal(2, changes);

                context.Set<SimplePoco>().Add(
                    new SimplePoco
                        {
                            PocoKey = 102,
                            Name = "C. Name",
                        });
                simplePoco.Name = "Updated Name";

                context.Set<SimplePoco>().Remove(simplePoco2);
                changes = context.SaveChanges();
                Assert.Equal(3, changes);
            }
        }

        // [Fact] Disabled due to #1038
        public async Task Add_modify_and_delete_simple_poco_together_async()
        {
            using (var context = CreateContext())
            {
                var simplePoco = context.Set<SimplePoco>().Add(
                    new SimplePoco
                        {
                            PocoKey = 100,
                            Name = "A. Name",
                        });
                var simplePoco2 = context.Set<SimplePoco>().Add(
                    new SimplePoco
                        {
                            PocoKey = 101,
                            Name = "B. Name",
                        });
                var changes = await context.SaveChangesAsync();
                Assert.Equal(2, changes);

                context.Set<SimplePoco>().Add(
                    new SimplePoco
                        {
                            PocoKey = 102,
                            Name = "C. Name",
                        });
                simplePoco.Name = "Updated Name";

                context.Set<SimplePoco>().Remove(simplePoco2);
                changes = await context.SaveChangesAsync();
                Assert.Equal(3, changes);
            }
        }

        // [Fact] Disabled due to #1038
        public void Get_simple_poco_count()
        {
            using (var context = CreateContext())
            {
                context.Set<SimplePoco>().Add(
                    new SimplePoco
                        {
                            PocoKey = 200,
                            Name = "B. Name",
                        });
                var changes = context.SaveChanges();
                Assert.Equal(1, changes);

                var simplePocos =
                    from c in context.Set<SimplePoco>()
                    select c;
                Assert.Equal(1, simplePocos.Count(cust => cust.PocoKey == 200));
            }
        }

        // [Fact] Disabled due to #1038
        public void Get_simple_poco_projection()
        {
            using (var context = CreateContext())
            {
                context.Set<SimplePoco>().Add(
                    new SimplePoco
                        {
                            PocoKey = 300,
                            Name = "C. Name",
                        });
                context.Set<SimplePoco>().Add(
                    new SimplePoco
                        {
                            PocoKey = 301,
                            Name = "C. Name the 2nd",
                        });
                var changes = context.SaveChanges();
                Assert.Equal(2, changes);

                var simplePocoNames =
                    from simplePoco in context.Set<SimplePoco>()
                    where (simplePoco.PocoKey == 300 || simplePoco.PocoKey == 301)
                    select simplePoco.Name;

                var simplePocoNamesArray = simplePocoNames.ToArray();
                Assert.Equal(2, simplePocoNamesArray.Length);
                Assert.Equal("C. Name", simplePocoNamesArray[0]);
                Assert.Equal("C. Name the 2nd", simplePocoNamesArray[1]);
            }
        }

        private readonly RedisFixture _fixture;

        public SimpleTest(RedisFixture fixture)
        {
            _fixture = fixture;
        }

        public void Dispose()
        {
            using (var context = CreateContext())
            {
                context.Set<SimplePoco>().RemoveRange(context.Set<SimplePoco>());
                context.SaveChanges();
            }
        }

        private DbContext CreateContext()
        {
            var options = new DbContextOptions()
                .UseModel(CreateModel())
                .UseRedis("127.0.0.1", RedisTestConfig.RedisPort);

            return new DbContext(_fixture.ServiceProvider, options);
        }

        private IModel CreateModel()
        {
            var model = new Model();
            var builder = new ModelBuilder(model);
            builder.Entity<SimplePoco>(b =>
                {
                    b.Key(cust => cust.PocoKey);
                });

            return model;
        }
    }

    public class SimplePoco
    {
        public int PocoKey { get; set; }
        public string Name { get; set; }
    }
}
