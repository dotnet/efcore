// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable AccessToDisposedClosure
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Local

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class QueryBugsTest : IClassFixture<SqlServerFixture>
    {
        [Fact]
        public void Query_when_sentinel_key_in_database_should_throw()
        {
            using (var testStore = SqlServerTestStore.CreateScratch())
            {
                testStore.ExecuteNonQuery(
                    @"CREATE TABLE ZeroKey (Id int);
                      INSERT ZeroKey VALUES (0)");

                using (var context = new SentinelKeyContext(testStore.Connection.ConnectionString))
                {
                    Assert.Equal(
                        RelationalStrings.InvalidKeyValue("ZeroKey"),
                        Assert.Throws<InvalidOperationException>(() => context.ZeroKeys.ToList()).Message);
                }
            }
        }

        [Fact]
        public void Query_when_null_sentinel_key_in_database_should_throw()
        {
            using (var testStore = SqlServerTestStore.CreateScratch())
            {
                testStore.ExecuteNonQuery(
                    @"CREATE TABLE ZeroKey (Id int);
                      INSERT ZeroKey VALUES (NULL)");

                using (var context = new SentinelKeyContext(testStore.Connection.ConnectionString))
                {
                    Assert.Equal(
                        RelationalStrings.InvalidKeyValue("ZeroKey"),
                        Assert.Throws<InvalidOperationException>(() => context.ZeroKeys.ToList()).Message);
                }
            }
        }

        private class SentinelKeyContext : DbContext
        {
            private readonly string _connectionString;

            public SentinelKeyContext(string connectionString)
            {
                _connectionString = connectionString;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlServer(_connectionString);

            public DbSet<ZeroKey> ZeroKeys { get; set; }

            public class ZeroKey
            {
                public int Id { get; set; }
            }
        }

        [Fact]
        public async Task First_FirstOrDefault_ix_async_bug_603()
        {
            using (var context = new MyContext603(_fixture.ServiceProvider))
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

                context.Products.Add(new Product { Name = "Product 1" });
                context.SaveChanges();
            }

            using (var ctx = new MyContext603(_fixture.ServiceProvider))
            {
                var product = await ctx.Products.FirstAsync();

                ctx.Products.Remove(product);

                await ctx.SaveChangesAsync();
            }

            using (var context = new MyContext603(_fixture.ServiceProvider))
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

                context.Products.Add(new Product { Name = "Product 1" });
                context.SaveChanges();
            }

            using (var ctx = new MyContext603(_fixture.ServiceProvider))
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

        private class MyContext603 : DbContext
        {
            public MyContext603(IServiceProvider provider)
                : base(provider)
            {
            }

            public DbSet<Product> Products { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlServer(SqlServerTestStore.CreateConnectionString("Repro603"));
        }

        [Fact]
        public void Include_on_entity_with_composite_key_One_To_Many_bugs_925_926()
        {
            CreateDatabase925();

            var loggingFactory = new TestSqlLoggerFactory();
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
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

SELECT [o].[Id], [o].[CustomerFirstName], [o].[CustomerLastName], [o].[Name]
FROM [Order] AS [o]
INNER JOIN (
    SELECT DISTINCT [c].[FirstName], [c].[LastName]
    FROM [Customer] AS [c]
) AS [c] ON ([o].[CustomerFirstName] = [c].[FirstName]) AND ([o].[CustomerLastName] = [c].[LastName])
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
                .AddSqlServer().
                ServiceCollection()
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
                    @"SELECT [o].[Id], [o].[CustomerFirstName], [o].[CustomerLastName], [o].[Name], [c].[FirstName], [c].[LastName]
FROM [Order] AS [o]
LEFT JOIN [Customer] AS [c] ON ([o].[CustomerFirstName] = [c].[FirstName]) AND ([o].[CustomerLastName] = [c].[LastName])";

                Assert.Equal(expectedSql, TestSqlLoggerFactory.Sql);
            }
        }

        private void CreateDatabase925()
        {
            CreateTestStore(
                "Repro925",
                _fixture.ServiceProvider,
                (sp, co) => new MyContext925(sp),
                context =>
                    {
                        var order11 = new Order { Name = "Order11" };
                        var order12 = new Order { Name = "Order12" };
                        var order21 = new Order { Name = "Order21" };
                        var order22 = new Order { Name = "Order22" };
                        var order23 = new Order { Name = "Order23" };

                        var customer1 = new Customer { FirstName = "Customer", LastName = "One", Orders = new List<Order> { order11, order12 } };
                        var customer2 = new Customer { FirstName = "Customer", LastName = "Two", Orders = new List<Order> { order21, order22, order23 } };

                        context.Customers.AddRange(customer1, customer2);
                        context.Orders.AddRange(order11, order12, order21, order22, order23);
                        context.SaveChanges();
                    });
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

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseSqlServer(SqlServerTestStore.CreateConnectionString("Repro925"))
                    .LogSqlParameterValues();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>(m =>
                    {
                        m.HasKey(c => new { c.FirstName, c.LastName });
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
            CreateTestStore(
                "Repro963",
                _fixture.ServiceProvider,
                (sp, co) => new MyContext963(sp),
                context =>
                    {
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
                        context.Targaryens.AddRange(daenerys, aerys);
                        context.Dragons.AddRange(drogon, rhaegal, viserion, balerion);
                        context.Details.Add(details);

                        context.SaveChanges();
                    });
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
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public DbSet<Details> Details { get; set; }
            public DbSet<Dragon> Dragons { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlServer(SqlServerTestStore.CreateConnectionString("Repro963"));

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Targaryen>(m =>
                    {
                        m.HasKey(t => t.Id);
                        m.HasMany(t => t.Dragons).WithOne(d => d.Mother).ForeignKey(d => d.MotherId);
                        m.HasOne(t => t.Details).WithOne(d => d.Targaryen).ForeignKey<Details>(d => d.TargaryenId);
                    });
            }
        }

        [Fact]
        public void Compiler_generated_local_closure_produces_valid_parameter_name_1742()
            => Execute1742(new CustomerDetails_1742 { FirstName = "Foo", LastName = "Bar" });

        public void Execute1742(CustomerDetails_1742 details)
        {
            CreateDatabase925();

            var loggingFactory = new TestSqlLoggerFactory();
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .AddInstance<ILoggerFactory>(loggingFactory)
                .BuildServiceProvider();

            using (var ctx = new MyContext925(serviceProvider))
            {
                var firstName = details.FirstName;

                ctx.Customers.Where(c => c.FirstName == firstName && c.LastName == details.LastName).ToList();

                const string expectedSql
                    = @"@__firstName_0: Foo
@__8__locals1_details_LastName_1: Bar

SELECT [c].[FirstName], [c].[LastName]
FROM [Customer] AS [c]
WHERE ([c].[FirstName] = @__firstName_0) AND ([c].[LastName] = @__8__locals1_details_LastName_1)";

                Assert.Equal(expectedSql, TestSqlLoggerFactory.Sql);
            }
        }

        public class CustomerDetails_1742
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        private readonly SqlServerFixture _fixture;

        public QueryBugsTest(SqlServerFixture fixture)
        {
            _fixture = fixture;
        }

        private static void CreateTestStore<TContext>(
            string databaseName,
            IServiceProvider serviceProvider,
            Func<IServiceProvider, DbContextOptions, TContext> contextCreator,
            Action<TContext> contextInitializer)
            where TContext : DbContext, IDisposable
        {
            var connectionString = SqlServerTestStore.CreateConnectionString(databaseName);
            SqlServerTestStore.GetOrCreateShared(databaseName, () =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder();
                    optionsBuilder.UseSqlServer(connectionString);

                    using (var context = contextCreator(serviceProvider, optionsBuilder.Options))
                    {
                        if (context.Database.EnsureCreated())
                        {
                            contextInitializer(context);
                        }

                        TestSqlLoggerFactory.SqlStatements.Clear();
                    }
                });
        }
    }
}
