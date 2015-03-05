// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Redis.Extensions;
using Xunit;

namespace Microsoft.Data.Entity.Redis.FunctionalTests
{
    public class RedisSimpleTest
    {
        [Fact]
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

                simplePoco.Entity.Name = "Updated Name";
                changes = context.SaveChanges();
                Assert.Equal(1, changes);

                context.Set<SimplePoco>().Remove(simplePoco.Entity);
                changes = context.SaveChanges();
                Assert.Equal(1, changes);
            }
        }

        [Fact]
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

                simplePoco.Entity.Name = "Updated Name";
                changes = await context.SaveChangesAsync();
                Assert.Equal(1, changes);

                context.Set<SimplePoco>().Remove(simplePoco.Entity);
                changes = await context.SaveChangesAsync();
                Assert.Equal(1, changes);
            }
        }

        [Fact]
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
                simplePoco.Entity.Name = "Updated Name";

                context.Set<SimplePoco>().Remove(simplePoco2.Entity);
                changes = context.SaveChanges();
                Assert.Equal(3, changes);
            }
        }

        [Fact]
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
                simplePoco.Entity.Name = "Updated Name";

                context.Set<SimplePoco>().Remove(simplePoco2.Entity);
                changes = await context.SaveChangesAsync();
                Assert.Equal(3, changes);
            }
        }

        [Fact]
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

        [Fact]
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

        private DbContext CreateContext()
        {
            var options = new DbContextOptions()
                .UseModel(CreateModel())
                .UseRedis("127.0.0.1", RedisTestConfig.RedisPort);

            return new DbContext(options);
        }

        private IModel CreateModel()
        {
            var model = new Model();
            var builder = new BasicModelBuilder(model);
            builder.Entity<SimplePoco>(b =>
                {
                    b.Key(cust => cust.PocoKey);
                    b.Property(cust => cust.PocoKey);
                    b.Property(cust => cust.Name);
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
