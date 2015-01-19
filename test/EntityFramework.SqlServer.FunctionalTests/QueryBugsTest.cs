// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Logging;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class QueryBugsTest : IClassFixture<SqlServerFixture>
    {
        [Fact]
        public async Task First_ix_async_bug_603()
        {
            using (var context = new MyContext(_fixture.ServiceProvider))
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

                context.Products.Add(new Product { Name = "Product 1" });
                context.SaveChanges();
            }

            using (var ctx = new MyContext(_fixture.ServiceProvider))
            {
                var product = await ctx.Products.FirstAsync();

                ctx.Products.Remove(product);

                await ctx.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task First_or_default_ix_async_bug_603()
        {
            using (var context = new MyContext(_fixture.ServiceProvider))
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

                context.Products.Add(new Product { Name = "Product 1" });
                context.SaveChanges();
            }

            using (var ctx = new MyContext(_fixture.ServiceProvider))
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
            public MyContext(IServiceProvider provider)
                : base(provider)
            {
            }

            public DbSet<Product> Products { get; set; }

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseSqlServer(SqlServerTestStore.CreateConnectionString("Repro603"));
            }
        }

        [Fact]
        public void Include_on_entity_with_composite_key_One_To_Many_bugs_925_926()
        {
            CreateDatabase925();

            var loggingFactory = new TestSqlLoggerFactory();
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer().ServiceCollection
                .AddInstance<ILoggerFactory>(loggingFactory)
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

SELECT [o].[CustomerFirstName], [o].[CustomerLastName], [o].[Id], [o].[Name]
FROM [Order] AS [o]
INNER JOIN (
    SELECT DISTINCT [c].[FirstName], [c].[LastName]
    FROM [Customer] AS [c]
) AS [c] ON ([o].[CustomerFirstName] = [c].[FirstName] AND [o].[CustomerLastName] = [c].[LastName])
ORDER BY [c].[FirstName], [c].[LastName]";

                Assert.Equal(expectedSql, TestSqlLoggerFactory.Sql);
            }
        }

        [Fact]
        public void Include_on_entity_with_composite_key_Many_To_One_bugs_925_926()
        {
            CreateDatabase925();

            var loggingFactory = new TestSqlLoggerFactory();
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer().ServiceCollection
                .AddInstance<ILoggerFactory>(loggingFactory)
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
                    @"SELECT [o].[CustomerFirstName], [o].[CustomerLastName], [o].[Id], [o].[Name], [c].[FirstName], [c].[LastName]
FROM [Order] AS [o]
LEFT JOIN [Customer] AS [c] ON ([o].[CustomerFirstName] = [c].[FirstName] AND [o].[CustomerLastName] = [c].[LastName])";

                Assert.Equal(expectedSql, TestSqlLoggerFactory.Sql);
            }
        }

        private void CreateDatabase925()
        {
            using (var context = new MyContext925(_fixture.ServiceProvider))
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

                context.Customers.Add(customer1, customer2);
                context.Orders.Add(order11, order12, order21, order22, order23);
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
            public MyContext925(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
            }

            public DbSet<Customer> Customers { get; set; }
            public DbSet<Order> Orders { get; set; }

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseSqlServer(SqlServerTestStore.CreateConnectionString("Repro925"));
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>(m =>
                    {
                        m.Key(c => new { c.FirstName, c.LastName });
                        m.HasMany(c => c.Orders).WithOne(o => o.Customer);
                    });
            }
        }

        [Fact]
        public void Include_on_optional_navigation_One_To_Many_963()
        {
            CreateDatabase963();

            using (var ctx = new MyContext963(_fixture.ServiceProvider))
            {
                ctx.Targaryens.Include(t => t.Dragons).ToList();
            }
        }

        [Fact]
        public void Include_on_optional_navigation_Many_To_One_963()
        {
            CreateDatabase963();

            using (var ctx = new MyContext963(_fixture.ServiceProvider))
            {
                ctx.Dragons.Include(d => d.Mother).ToList();
            }
        }

        [Fact]
        public void Include_on_optional_navigation_One_To_One_principal_963()
        {
            CreateDatabase963();

            using (var ctx = new MyContext963(_fixture.ServiceProvider))
            {
                ctx.Targaryens.Include(t => t.Details).ToList();
            }
        }

        [Fact]
        public void Include_on_optional_navigation_One_To_One_dependent_963()
        {
            CreateDatabase963();

            using (var ctx = new MyContext963(_fixture.ServiceProvider))
            {
                ctx.Details.Include(d => d.Targaryen).ToList();
            }
        }

        [Fact]
        public void Join_on_optional_navigation_One_To_Many_963()
        {
            CreateDatabase963();

            using (var ctx = new MyContext963(_fixture.ServiceProvider))
            {
                (from t in ctx.Targaryens
                    join d in ctx.Dragons on t.Id equals d.MotherId
                    select d).ToList();
            }
        }

        private void CreateDatabase963()
        {
            using (var context = new MyContext963(_fixture.ServiceProvider))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var drogon = new Dragon { Name = "Drogon" };
                var rhaegal = new Dragon { Name = "Rhaegal" };
                var viserion = new Dragon { Name = "Viserion" };
                var balerion = new Dragon { Name = "Balerion" };

                var aerys = new Targaryen { Name = "Aerys II" };
                var details = new Details
                    {
                        FullName = @"Daenerys Stormborn of the House Targaryen, the First of Her Name, the Unburnt, Queen of Meereen, 
Queen of the Andals and the Rhoynar and the First Men, Khaleesi of the Great Grass Sea, Breaker of Chains, and Mother of Dragons"
                    };

                var daenerys = new Targaryen { Name = "Daenerys", Details = details, Dragons = new List<Dragon> { drogon, rhaegal, viserion } };
                context.Targaryens.Add(daenerys, aerys);
                context.Dragons.Add(drogon, rhaegal, viserion, balerion);
                context.Details.Add(details);

                context.SaveChanges();
            }
        }

        public class Targaryen
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Details Details { get; set; }

            public List<Dragon> Dragons { get; set; }
        }

        public class Dragon
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int? MotherId { get; set; }
            public Targaryen Mother { get; set; }
        }

        public class Details
        {
            public int Id { get; set; }
            public int? TargaryenId { get; set; }
            public Targaryen Targaryen { get; set; }
            public string FullName { get; set; }
        }

        // TODO: replace with GearsOfWar context when it's refactored properly
        public class MyContext963 : DbContext
        {
            public MyContext963(IServiceProvider provider)
                : base(provider)
            {
            }

            public DbSet<Targaryen> Targaryens { get; set; }
            public DbSet<Details> Details { get; set; }
            public DbSet<Dragon> Dragons { get; set; }

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseSqlServer(SqlServerTestStore.CreateConnectionString("Repro963"));
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Targaryen>(m =>
                    {
                        m.Key(t => t.Id);
                        m.HasMany(t => t.Dragons).WithOne(d => d.Mother).ForeignKey(d => d.MotherId);
                        m.HasOne(t => t.Details).WithOne(d => d.Targaryen).ForeignKey<Details>(d => d.TargaryenId);
                    });
            }
        }

        private readonly SqlServerFixture _fixture;

        public QueryBugsTest(SqlServerFixture fixture)
        {
            _fixture = fixture;
        }
    }
}
