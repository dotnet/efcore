// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class QueryBugsTest
    {
        [Fact]
        public async Task First_ix_async_bug_603()
        {
            using (var context = new MyContext603())
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

                context.Products.Add(new Product603 { Name = "Product 1" });
                context.SaveChanges();
            }

            using (var ctx = new MyContext603())
            {
                var product = await ctx.Products.FirstAsync();

                ctx.Products.Remove(product);

                await ctx.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task First_or_default_ix_async_bug_603()
        {
            using (var context = new MyContext603())
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

                context.Products.Add(new Product603 { Name = "Product 1" });
                context.SaveChanges();
            }

            using (var ctx = new MyContext603())
            {
                var product = await ctx.Products.FirstOrDefaultAsync();

                ctx.Products.Remove(product);

                await ctx.SaveChangesAsync();
            }
        }

        private class Product603
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class MyContext603 : DbContext
        {
            public DbSet<Product603> Products { get; set; }

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseSqlServer(SqlServerTestDatabase.CreateConnectionString("Repro603"));
            }
        }

        [Fact]
        public void Include_on_entity_with_composite_key_One_To_Many_bugs_925_926()
        {
            CreateDatabase925();

            using (var ctx = new MyContext925())
            {
                var query = ctx.Customers.Include(c => c.Orders).OrderBy(c => c.FirstName).ThenBy(c => c.LastName);
                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal(2, result[0].Orders.Count);
                Assert.Equal(3, result[1].Orders.Count);
            }
        }


        [Fact]
        public void Include_on_entity_with_composite_key_Many_To_One_bugs_925_926()
        {
            CreateDatabase925();

            using (var ctx = new MyContext925())
            {
                var query = ctx.Orders.Include(o => o.Customer);
                var result = query.ToList();

                Assert.Equal(5, result.Count);
                Assert.Equal("One", result[0].Customer.LastName);
                Assert.Equal("One", result[1].Customer.LastName);
                Assert.Equal("Two", result[2].Customer.LastName);
                Assert.Equal("Two", result[3].Customer.LastName);
                Assert.Equal("Two", result[4].Customer.LastName);
            }
        }

        private void CreateDatabase925()
        {
            using (var context = new MyContext925())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var order11 = new Order925 { Name = "Order11" };
                var order12 = new Order925 { Name = "Order12" };
                var order21 = new Order925 { Name = "Order21" };
                var order22 = new Order925 { Name = "Order22" };
                var order23 = new Order925 { Name = "Order23" };

                var customer1 = new Customer925 { FirstName = "Customer", LastName = "One", Orders = new List<Order925> { order11, order12 } };
                var customer2 = new Customer925 { FirstName = "Customer", LastName = "Two", Orders = new List<Order925> { order21, order22, order23 } };

                context.Customers.AddRange(new[] { customer1, customer2 });
                context.Orders.AddRange(new[] { order11, order12, order21, order22, order23 });
                context.SaveChanges();
            }
        }

        public class Customer925
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public List<Order925> Orders { get; set; }
        }

        public class Order925
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Customer925 Customer { get; set; }
        }

        public class MyContext925 : DbContext
        {
            public DbSet<Customer925> Customers { get; set; }
            public DbSet<Order925> Orders { get; set; }

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseSqlServer(SqlServerTestDatabase.CreateConnectionString("Repro925"));
            }

            protected override void OnModelCreating(Entity.Metadata.ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer925>(m =>
                    {
                        m.Key(c => new { c.FirstName, c.LastName });
                        m.OneToMany(c => c.Orders, o => o.Customer);
                    });
            }
        }
    }
}
