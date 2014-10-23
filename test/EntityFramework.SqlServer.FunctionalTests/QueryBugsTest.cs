// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Advanced;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class QueryBugsTest
    {
        [Fact]
        public async Task First_ix_async_bug_603()
        {
            using (var context = new MyContext())
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

                context.Products.Add(new Product { Name = "Product 1" });
                context.SaveChanges();
            }

            using (var ctx = new MyContext())
            {
                var product = await ctx.Products.FirstAsync();

                ctx.Products.Remove(product);

                await ctx.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task First_or_default_ix_async_bug_603()
        {
            using (var context = new MyContext())
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

                context.Products.Add(new Product { Name = "Product 1" });
                context.SaveChanges();
            }

            using (var ctx = new MyContext())
            {
                var product = await ctx.Products.FirstOrDefaultAsync();

                ctx.Products.Remove(product);

                await ctx.SaveChangesAsync();
            }
        }

        private class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class MyContext : DbContext
        {
            public DbSet<Product> Products { get; set; }

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseSqlServer(SqlServerTestDatabase.CreateConnectionString("Repro603"));
            }
        }

        [Fact]
        public void Include_on_entity_with_composite_key_One_To_Many_bugs_925_926()
        {
            CreateDatabase925();

            var loggingFactory = new TestSqlLoggerFactory();
            var serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .UseLoggerFactory(loggingFactory)
                    .ServiceCollection
                    .BuildServiceProvider();

            using (var ctx = new MyContext925(serviceProvider))
            {
                var query = ctx.Customers.Include(c => c.Orders).OrderBy(c => c.FirstName).ThenBy(c => c.LastName);
                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal(2, result[0].Orders.Count);
                Assert.Equal(3, result[1].Orders.Count);

                var expectedSql =
@"SELECT [c].[FirstName], [c].[LastName]
FROM [Customer] AS [c]
ORDER BY [c].[FirstName], [c].[LastName]

SELECT [o].[CustomerId0], [o].[CustomerId1], [o].[Id], [o].[Name]
FROM [Order] AS [o]
INNER JOIN (
    SELECT DISTINCT [c].[FirstName], [c].[LastName]
    FROM [Customer] AS [c]
) AS [c] ON ([o].[CustomerId0] = [c].[FirstName] AND [o].[CustomerId1] = [c].[LastName])
ORDER BY [c].[FirstName], [c].[LastName]";

                Assert.Equal(expectedSql, TestSqlLoggerFactory.Logger.Sql);
            }
        }

        [Fact]
        public void Include_on_entity_with_composite_key_Many_To_One_bugs_925_926()
        {
            CreateDatabase925();

            var loggingFactory = new TestSqlLoggerFactory();
            var serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .UseLoggerFactory(loggingFactory)
                    .ServiceCollection
                    .BuildServiceProvider();

            using (var ctx = new MyContext925(serviceProvider))
            {
                var query = ctx.Orders.Include(o => o.Customer);
                var result = query.ToList();

                Assert.Equal(5, result.Count);
                Assert.NotNull(result[0].Customer);
                Assert.NotNull(result[1].Customer);
                Assert.NotNull(result[2].Customer);
                Assert.NotNull(result[3].Customer);
                Assert.NotNull(result[4].Customer);

                var expectedSql =
@"SELECT [o].[CustomerId0], [o].[CustomerId1], [o].[Id], [o].[Name], [c].[FirstName], [c].[LastName]
FROM [Order] AS [o]
LEFT JOIN [Customer] AS [c] ON ([c].[FirstName] = [o].[CustomerId0] AND [c].[LastName] = [o].[CustomerId1])";

                Assert.Equal(expectedSql, TestSqlLoggerFactory.Logger.Sql);
            }
        }

        private void CreateDatabase925()
        {
            using (var context = new MyContext925())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var order11 = new Order { Name = "Order11" };
                var order12 = new Order { Name = "Order12" };
                var order21 = new Order { Name = "Order21" };
                var order22 = new Order { Name = "Order22" };
                var order23 = new Order { Name = "Order23" };

                var customer1 = new Customer { FirstName = "Customer", LastName = "One", Orders = new List<Order> { order11, order12 } };
                var customer2 = new Customer { FirstName = "Customer", LastName = "Two", Orders = new List<Order> { order21, order22, order23 } };

                context.Customers.AddRange(new[] { customer1, customer2 });
                context.Orders.AddRange(new[] { order11, order12, order21, order22, order23 });
                context.SaveChanges();
            }
        }

        public class Customer
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public List<Order> Orders { get; set; }
        }

        public class Order
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Customer Customer { get; set; }
        }

        public class MyContext925 : DbContext
        {
            public MyContext925()
            {
            }

            public MyContext925(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
            }

            public DbSet<Customer> Customers { get; set; }
            public DbSet<Order> Orders { get; set; }

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseSqlServer(SqlServerTestDatabase.CreateConnectionString("Repro925"));
            }

            protected override void OnModelCreating(Entity.Metadata.ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>(m =>
                    {
                        m.Key(c => new { c.FirstName, c.LastName });
                        m.OneToMany(c => c.Orders, o => o.Customer);
                    });
            }
        }
    }
}
