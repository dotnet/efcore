// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable AccessToDisposedClosure
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Local
namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class QueryBugsTest : IClassFixture<SqlServerFixture>
    {
        [Fact]
        public async Task Multiple_optional_navs_should_not_deadlock_bug_5481()
        {
            using (var testStore = SqlServerTestStore.CreateScratch())
            {
                using (var context = new DeadlockContext(testStore.ConnectionString))
                {
                    context.Database.EnsureClean();
                    context.EnsureSeeded();

                    var count
                        = await context.Persons
                              .Where(p => (p.AddressOne != null && p.AddressOne.Street.Contains("Low Street"))
                                          || (p.AddressTwo != null && p.AddressTwo.Street.Contains("Low Street")))
                                            .CountAsync();

                    Assert.Equal(0, count);
                }
            }
        }

        private class DeadlockContext : DbContext
        {
            private readonly string _connectionString;

            public DeadlockContext(string connectionString)
            {
                _connectionString = connectionString;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlServer(_connectionString);

            public DbSet<Person> Persons { get; set; }
            public DbSet<Address> Addresses { get; set; }

            public class Address
            {
                public int Id { get; set; }
                public string Street { get; set; }
                public int PersonId { get; set; }
                public Person Person { get; set; }
            }

            public class Person
            {
                public int Id { get; set; }
                public string Name { get; set; }
                public int? AddressOneId { get; set; }
                public Address AddressOne { get; set; }
                public int? AddressTwoId { get; set; }
                public Address AddressTwo { get; set; }
            }

            public void EnsureSeeded()
            {
                if (!Persons.Any())
                {
                    AddRange(
                        new Person { Name = "John Doe" },
                        new Person { Name = "Joe Bloggs" });

                    SaveChanges();
                }
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Person>().HasKey(p => p.Id);

                modelBuilder.Entity<Person>().Property(p => p.Name)
                    .IsRequired();

                modelBuilder.Entity<Person>().HasOne(p => p.AddressOne)
                    .WithMany()
                    .HasForeignKey(p => p.AddressOneId)
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<Person>().Property(p => p.AddressOneId);

                modelBuilder.Entity<Person>().HasOne(p => p.AddressTwo)
                    .WithMany()
                    .HasForeignKey(p => p.AddressTwoId)
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<Person>().Property(p => p.AddressTwoId);

                modelBuilder.Entity<Address>().HasKey(a => a.Id);

                modelBuilder.Entity<Address>().Property(a => a.Street).IsRequired(true);

                modelBuilder.Entity<Address>().HasOne(a => a.Person)
                    .WithMany()
                    .HasForeignKey(a => a.PersonId)
                    .OnDelete(DeleteBehavior.Restrict);
            }
        }

        [Fact]
        public void Query_when_null_key_in_database_should_throw()
        {
            using (var testStore = SqlServerTestStore.CreateScratch())
            {
                testStore.ExecuteNonQuery(
                    @"CREATE TABLE ZeroKey (Id int);
                      INSERT ZeroKey VALUES (NULL)");

                using (var context = new NullKeyContext(testStore.ConnectionString))
                {
                    Assert.Equal(
                        CoreStrings.InvalidKeyValue("ZeroKey"),
                        Assert.Throws<InvalidOperationException>(() => context.ZeroKeys.ToList()).Message);
                }
            }
        }

        private class NullKeyContext : DbContext
        {
            private readonly string _connectionString;

            public NullKeyContext(string connectionString)
            {
                _connectionString = connectionString;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlServer(_connectionString);

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<ZeroKey>().ToTable("ZeroKey");

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
                context.Database.EnsureClean();

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
                context.Database.EnsureClean();

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
            private readonly IServiceProvider _serviceProvider;

            public MyContext603(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public DbSet<Product> Products { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseSqlServer(SqlServerTestStore.CreateConnectionString("Repro603"))
                    .UseInternalServiceProvider(_serviceProvider);

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Product>().ToTable("Product");
        }

        [Fact]
        public void Include_on_entity_with_composite_key_One_To_Many_bugs_925_926()
        {
            CreateDatabase925();

            var loggingFactory = new TestSqlLoggerFactory();
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddSingleton<ILoggerFactory>(loggingFactory)
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
WHERE EXISTS (
    SELECT 1
    FROM [Customer] AS [c]
    WHERE ([o].[CustomerFirstName] = [c].[FirstName]) AND ([o].[CustomerLastName] = [c].[LastName]))
ORDER BY [o].[CustomerFirstName], [o].[CustomerLastName]";

                Assert.Equal(expectedSql, Sql);
            }
        }

        [Fact]
        public void Include_on_entity_with_composite_key_Many_To_One_bugs_925_926()
        {
            CreateDatabase925();

            var loggingFactory = new TestSqlLoggerFactory();
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddSingleton<ILoggerFactory>(loggingFactory)
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

                Assert.Equal(expectedSql, Sql);
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
            private readonly IServiceProvider _serviceProvider;

            public MyContext925(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public DbSet<Customer> Customers { get; set; }
            public DbSet<Order> Orders { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .EnableSensitiveDataLogging()
                    .UseSqlServer(SqlServerTestStore.CreateConnectionString("Repro925"))
                    .UseInternalServiceProvider(_serviceProvider);

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>(m =>
                {
                    m.ToTable("Customer");
                    m.HasKey(c => new { c.FirstName, c.LastName });
                    m.HasMany(c => c.Orders).WithOne(o => o.Customer);
                });

                modelBuilder.Entity<Order>().ToTable("Order");
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
            private readonly IServiceProvider _serviceProvider;

            public MyContext963(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public DbSet<Targaryen> Targaryens { get; set; }
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public DbSet<Details> Details { get; set; }
            public DbSet<Dragon> Dragons { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseSqlServer(SqlServerTestStore.CreateConnectionString("Repro963"))
                    .UseInternalServiceProvider(_serviceProvider);

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Targaryen>(m =>
                {
                    m.ToTable("Targaryen");
                    m.HasKey(t => t.Id);
                    m.HasMany(t => t.Dragons).WithOne(d => d.Mother).HasForeignKey(d => d.MotherId);
                    m.HasOne(t => t.Details).WithOne(d => d.Targaryen).HasForeignKey<Details>(d => d.TargaryenId);
                });

                modelBuilder.Entity<Dragon>().ToTable("Dragon");
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
                .AddEntityFrameworkSqlServer()
                .AddSingleton<ILoggerFactory>(loggingFactory)
                .BuildServiceProvider();

            using (var ctx = new MyContext925(serviceProvider))
            {
                var firstName = details.FirstName;

                ctx.Customers.Where(c => c.FirstName == firstName && c.LastName == details.LastName).ToList();

                const string expectedSql
                    = @"@__firstName_0: Foo (Size = 450)
@__8__locals1_details_LastName_1: Bar (Size = 450)

SELECT [c].[FirstName], [c].[LastName]
FROM [Customer] AS [c]
WHERE ([c].[FirstName] = @__firstName_0) AND ([c].[LastName] = @__8__locals1_details_LastName_1)";

                Assert.Equal(expectedSql, Sql);
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

        [Fact]
        public void Customer_collections_materialize_properly_3758()
        {
            CreateDatabase3758();

            var loggingFactory = new TestSqlLoggerFactory();
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddSingleton<ILoggerFactory>(loggingFactory)
                .BuildServiceProvider();

            using (var ctx = new MyContext3758(serviceProvider))
            {
                var query1 = ctx.Customers.Select(c => c.Orders1);
                var result1 = query1.ToList();

                Assert.Equal(2, result1.Count);
                Assert.IsType<HashSet<Order3758>>(result1[0]);
                Assert.Equal(2, result1[0].Count);
                Assert.Equal(2, result1[1].Count);

                var query2 = ctx.Customers.Select(c => c.Orders2);
                var result2 = query2.ToList();

                Assert.Equal(2, result2.Count);
                Assert.IsType<MyGenericCollection3758<Order3758>>(result2[0]);
                Assert.Equal(2, result2[0].Count);
                Assert.Equal(2, result2[1].Count);

                var query3 = ctx.Customers.Select(c => c.Orders3);
                var result3 = query3.ToList();

                Assert.Equal(2, result3.Count);
                Assert.IsType<MyNonGenericCollection3758>(result3[0]);
                Assert.Equal(2, result3[0].Count);
                Assert.Equal(2, result3[1].Count);

                var query4 = ctx.Customers.Select(c => c.Orders4);

                Assert.Equal(
                    CoreStrings.NavigationCannotCreateType("Orders4", typeof(Customer3758).Name,
                        typeof(MyInvalidCollection3758<Order3758>).ShortDisplayName()),
                    Assert.Throws<InvalidOperationException>(() => query4.ToList()).Message);
            }
        }

        public class MyContext3758 : DbContext
        {
            private readonly IServiceProvider _serviceProvider;

            public MyContext3758(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public DbSet<Customer3758> Customers { get; set; }
            public DbSet<Order3758> Orders { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseSqlServer(SqlServerTestStore.CreateConnectionString("Repro3758"))
                    .UseInternalServiceProvider(_serviceProvider);

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer3758>(b =>
                {
                    b.ToTable("Customer3758");

                    b.HasMany(e => e.Orders1).WithOne();
                    b.HasMany(e => e.Orders2).WithOne();
                    b.HasMany(e => e.Orders3).WithOne();
                    b.HasMany(e => e.Orders4).WithOne();
                });

                modelBuilder.Entity<Order3758>().ToTable("Order3758");
            }
        }

        public class Customer3758
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public ICollection<Order3758> Orders1 { get; set; }
            public MyGenericCollection3758<Order3758> Orders2 { get; set; }
            public MyNonGenericCollection3758 Orders3 { get; set; }
            public MyInvalidCollection3758<Order3758> Orders4 { get; set; }
        }

        public class Order3758
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class MyGenericCollection3758<TElement> : List<TElement>
        {
        }

        public class MyNonGenericCollection3758 : List<Order3758>
        {
        }

        public class MyInvalidCollection3758<TElement> : List<TElement>
        {
            public MyInvalidCollection3758(int argument)
            {
            }
        }

        private void CreateDatabase3758()
        {
            CreateTestStore(
                "Repro3758",
                _fixture.ServiceProvider,
                (sp, co) => new MyContext3758(sp),
                context =>
                {
                    var o111 = new Order3758 { Name = "O111" };
                    var o112 = new Order3758 { Name = "O112" };
                    var o121 = new Order3758 { Name = "O121" };
                    var o122 = new Order3758 { Name = "O122" };
                    var o131 = new Order3758 { Name = "O131" };
                    var o132 = new Order3758 { Name = "O132" };
                    var o141 = new Order3758 { Name = "O141" };

                    var o211 = new Order3758 { Name = "O211" };
                    var o212 = new Order3758 { Name = "O212" };
                    var o221 = new Order3758 { Name = "O221" };
                    var o222 = new Order3758 { Name = "O222" };
                    var o231 = new Order3758 { Name = "O231" };
                    var o232 = new Order3758 { Name = "O232" };
                    var o241 = new Order3758 { Name = "O241" };

                    var c1 = new Customer3758
                    {
                        Name = "C1",
                        Orders1 = new List<Order3758> { o111, o112 },
                        Orders2 = new MyGenericCollection3758<Order3758>(),
                        Orders3 = new MyNonGenericCollection3758(),
                        Orders4 = new MyInvalidCollection3758<Order3758>(42)
                    };

                    c1.Orders2.AddRange(new[] { o121, o122 });
                    c1.Orders3.AddRange(new[] { o131, o132 });
                    c1.Orders4.Add(o141);

                    var c2 = new Customer3758
                    {
                        Name = "C2",
                        Orders1 = new List<Order3758> { o211, o212 },
                        Orders2 = new MyGenericCollection3758<Order3758>(),
                        Orders3 = new MyNonGenericCollection3758(),
                        Orders4 = new MyInvalidCollection3758<Order3758>(42)
                    };

                    c2.Orders2.AddRange(new[] { o221, o222 });
                    c2.Orders3.AddRange(new[] { o231, o232 });
                    c2.Orders4.Add(o241);

                    context.Customers.AddRange(c1, c2);
                    context.Orders.AddRange(o111, o112, o121, o122, o131, o132, o141, o211, o212, o221, o222, o231, o232, o241);

                    context.SaveChanges();
                });
        }

        [Fact]
        public void ThenInclude_with_interface_navigations_3409()
        {
            CreateDatabase3409();

            var loggingFactory = new TestSqlLoggerFactory();
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddSingleton<ILoggerFactory>(loggingFactory)
                .BuildServiceProvider();

            using (var context = new MyContext3409(serviceProvider))
            {
                var results = context.Parents
                    .Include(p => p.ChildCollection)
                    .ThenInclude(c => c.SelfReferenceCollection)
                    .ToList();

                Assert.Equal(1, results.Count);
                Assert.Equal(1, results[0].ChildCollection.Count);
                Assert.Equal(2, results[0].ChildCollection.Single().SelfReferenceCollection.Count);
            }

            using (var context = new MyContext3409(serviceProvider))
            {
                var results = context.Children
                    .Include(c => c.SelfReferenceBackNavigation)
                    .ThenInclude(c => c.ParentBackNavigation)
                    .ToList();

                Assert.Equal(3, results.Count);
                Assert.Equal(2, results.Count(c => c.SelfReferenceBackNavigation != null));
                Assert.Equal(1, results.Count(c => c.ParentBackNavigation != null));
            }
        }

        public class MyContext3409 : DbContext
        {
            public DbSet<Parent3409> Parents { get; set; }
            public DbSet<Child3409> Children { get; set; }

            private readonly IServiceProvider _serviceProvider;

            public MyContext3409(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseSqlServer(SqlServerTestStore.CreateConnectionString("Repro3409"))
                    .UseInternalServiceProvider(_serviceProvider);

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Parent3409>()
                    .HasMany(p => (ICollection<Child3409>)p.ChildCollection)
                    .WithOne(c => (Parent3409)c.ParentBackNavigation);

                modelBuilder.Entity<Child3409>()
                    .HasMany(c => (ICollection<Child3409>)c.SelfReferenceCollection)
                    .WithOne(c => (Child3409)c.SelfReferenceBackNavigation);
            }
        }

        public interface IParent3409
        {
            int Id { get; set; }

            ICollection<IChild3409> ChildCollection { get; set; }
        }

        public interface IChild3409
        {
            int Id { get; set; }

            int? ParentBackNavigationId { get; set; }
            IParent3409 ParentBackNavigation { get; set; }

            ICollection<IChild3409> SelfReferenceCollection { get; set; }
            int? SelfReferenceBackNavigationId { get; set; }
            IChild3409 SelfReferenceBackNavigation { get; set; }
        }

        public class Parent3409 : IParent3409
        {
            public int Id { get; set; }

            public ICollection<IChild3409> ChildCollection { get; set; }
        }

        public class Child3409 : IChild3409
        {
            public int Id { get; set; }

            public int? ParentBackNavigationId { get; set; }
            public IParent3409 ParentBackNavigation { get; set; }

            public ICollection<IChild3409> SelfReferenceCollection { get; set; }
            public int? SelfReferenceBackNavigationId { get; set; }
            public IChild3409 SelfReferenceBackNavigation { get; set; }
        }

        private void CreateDatabase3409()
        {
            CreateTestStore(
                "Repro3409",
                _fixture.ServiceProvider,
                (sp, co) => new MyContext3409(sp),
                context =>
                {
                    var parent1 = new Parent3409();

                    var child1 = new Child3409();
                    var child2 = new Child3409();
                    var child3 = new Child3409();

                    parent1.ChildCollection = new List<IChild3409> { child1 };
                    child1.SelfReferenceCollection = new List<IChild3409> { child2, child3 };

                    context.Parents.AddRange(parent1);
                    context.Children.AddRange(child1, child2, child3);

                    context.SaveChanges();
                });
        }

        [Fact]
        public virtual void Repro3101_simple_coalesce1()
        {
            CreateDatabase3101();

            var loggingFactory = new TestSqlLoggerFactory();
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddSingleton<ILoggerFactory>(loggingFactory)
                .BuildServiceProvider();

            using (var ctx = new MyContext3101(serviceProvider))
            {
                var query = from eVersion in ctx.Entities.Include(e => e.Children)
                            join eRoot in ctx.Entities
                            on eVersion.RootEntityId equals (int?)eRoot.Id
                            into RootEntities
                            from eRootJoined in RootEntities.DefaultIfEmpty()
                            select eRootJoined ?? eVersion;

                var result = query.ToList();
                Assert.True(result.All(e => e.Children.Count > 0));
            }
        }

        [Fact]
        public virtual void Repro3101_simple_coalesce2()
        {
            CreateDatabase3101();

            var loggingFactory = new TestSqlLoggerFactory();
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddSingleton<ILoggerFactory>(loggingFactory)
                .BuildServiceProvider();

            using (var ctx = new MyContext3101(serviceProvider))
            {
                var query = from eVersion in ctx.Entities
                            join eRoot in ctx.Entities.Include(e => e.Children)
                            on eVersion.RootEntityId equals (int?)eRoot.Id
                            into RootEntities
                            from eRootJoined in RootEntities.DefaultIfEmpty()
                            select eRootJoined ?? eVersion;

                var result = query.ToList();
                Assert.Equal(2, result.Count(e => e.Children.Count > 0));
            }
        }

        [Fact]
        public virtual void Repro3101_simple_coalesce3()
        {
            CreateDatabase3101();

            var loggingFactory = new TestSqlLoggerFactory();
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddSingleton<ILoggerFactory>(loggingFactory)
                .BuildServiceProvider();

            using (var ctx = new MyContext3101(serviceProvider))
            {
                var query = from eVersion in ctx.Entities.Include(e => e.Children)
                            join eRoot in ctx.Entities.Include(e => e.Children)
                            on eVersion.RootEntityId equals (int?)eRoot.Id
                            into RootEntities
                            from eRootJoined in RootEntities.DefaultIfEmpty()
                            select eRootJoined ?? eVersion;

                var result = query.ToList();
                Assert.True(result.All(e => e.Children.Count > 0));
            }
        }

        [Fact]
        public virtual void Repro3101_complex_coalesce1()
        {
            CreateDatabase3101();

            var loggingFactory = new TestSqlLoggerFactory();
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddSingleton<ILoggerFactory>(loggingFactory)
                .BuildServiceProvider();

            using (var ctx = new MyContext3101(serviceProvider))
            {
                var query = from eVersion in ctx.Entities.Include(e => e.Children)
                            join eRoot in ctx.Entities
                            on eVersion.RootEntityId equals (int?)eRoot.Id
                            into RootEntities
                            from eRootJoined in RootEntities.DefaultIfEmpty()
                            select new { One = 1, Coalesce = eRootJoined ?? eVersion };

                var result = query.ToList();
                Assert.True(result.All(e => e.Coalesce.Children.Count > 0));
            }
        }

        [Fact]
        public virtual void Repro3101_complex_coalesce2()
        {
            CreateDatabase3101();

            var loggingFactory = new TestSqlLoggerFactory();
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddSingleton<ILoggerFactory>(loggingFactory)
                .BuildServiceProvider();

            using (var ctx = new MyContext3101(serviceProvider))
            {
                var query = from eVersion in ctx.Entities
                            join eRoot in ctx.Entities.Include(e => e.Children)
                            on eVersion.RootEntityId equals (int?)eRoot.Id
                            into RootEntities
                            from eRootJoined in RootEntities.DefaultIfEmpty()
                            select new { Root = eRootJoined, Coalesce = eRootJoined ?? eVersion };

                var result = query.ToList();
                Assert.Equal(2, result.Count(e => e.Coalesce.Children.Count > 0));
            }
        }

        [Fact]
        public virtual void Repro3101_nested_coalesce1()
        {
            CreateDatabase3101();

            var loggingFactory = new TestSqlLoggerFactory();
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddSingleton<ILoggerFactory>(loggingFactory)
                .BuildServiceProvider();

            using (var ctx = new MyContext3101(serviceProvider))
            {
                var query = from eVersion in ctx.Entities
                            join eRoot in ctx.Entities.Include(e => e.Children)
                            on eVersion.RootEntityId equals (int?)eRoot.Id
                            into RootEntities
                            from eRootJoined in RootEntities.DefaultIfEmpty()
                            select new { One = 1, Coalesce = eRootJoined ?? (eVersion ?? eRootJoined) };

                var result = query.ToList();
                Assert.Equal(2, result.Count(e => e.Coalesce.Children.Count > 0));
            }
        }

        [Fact]
        public virtual void Repro3101_nested_coalesce2()
        {
            CreateDatabase3101();

            var loggingFactory = new TestSqlLoggerFactory();
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddSingleton<ILoggerFactory>(loggingFactory)
                .BuildServiceProvider();

            using (var ctx = new MyContext3101(serviceProvider))
            {
                var query = from eVersion in ctx.Entities.Include(e => e.Children)
                            join eRoot in ctx.Entities
                            on eVersion.RootEntityId equals (int?)eRoot.Id
                            into RootEntities
                            from eRootJoined in RootEntities.DefaultIfEmpty()
                            select new { One = eRootJoined, Two = 2, Coalesce = eRootJoined ?? (eVersion ?? eRootJoined) };

                var result = query.ToList();
                Assert.True(result.All(e => e.Coalesce.Children.Count > 0));
            }
        }

        [Fact]
        public virtual void Repro3101_conditional()
        {
            CreateDatabase3101();

            var loggingFactory = new TestSqlLoggerFactory();
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddSingleton<ILoggerFactory>(loggingFactory)
                .BuildServiceProvider();

            using (var ctx = new MyContext3101(serviceProvider))
            {
                var query = from eVersion in ctx.Entities.Include(e => e.Children)
                            join eRoot in ctx.Entities
                            on eVersion.RootEntityId equals (int?)eRoot.Id
                            into RootEntities
                            from eRootJoined in RootEntities.DefaultIfEmpty()
                            select eRootJoined != null ? eRootJoined : eVersion;

                var result = query.ToList();
                Assert.True(result.All(e => e.Children.Count > 0));
            }
        }

        [Fact]
        public virtual void Repro3101_coalesce_tracking()
        {
            CreateDatabase3101();

            var loggingFactory = new TestSqlLoggerFactory();
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddSingleton<ILoggerFactory>(loggingFactory)
                .BuildServiceProvider();

            using (var ctx = new MyContext3101(serviceProvider))
            {
                var query = from eVersion in ctx.Entities
                            join eRoot in ctx.Entities
                            on eVersion.RootEntityId equals (int?)eRoot.Id
                            into RootEntities
                            from eRootJoined in RootEntities.DefaultIfEmpty()
                            select new { eRootJoined, eVersion, foo = eRootJoined ?? eVersion };

                var result = query.ToList();

                var foo = ctx.ChangeTracker.Entries().ToList();
                Assert.True(ctx.ChangeTracker.Entries().Count() > 0);
            }
        }

        private const string FileLineEnding = @"
";

        protected virtual void ClearLog() => TestSqlLoggerFactory.Reset();

        private static string Sql => TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);

        private void CreateDatabase3101()
        {
            CreateTestStore(
                "Repro3101",
                _fixture.ServiceProvider,
                (sp, co) => new MyContext3101(sp),
                context =>
                {
                    var c11 = new Child3101 { Name = "c11" };
                    var c12 = new Child3101 { Name = "c12" };
                    var c13 = new Child3101 { Name = "c13" };
                    var c21 = new Child3101 { Name = "c21" };
                    var c22 = new Child3101 { Name = "c22" };
                    var c31 = new Child3101 { Name = "c31" };
                    var c32 = new Child3101 { Name = "c32" };

                    context.Children.AddRange(c11, c12, c13, c21, c22, c31, c32);

                    var e1 = new Entity3101 { Id = 1, Children = new[] { c11, c12, c13 } };
                    var e2 = new Entity3101 { Id = 2, Children = new[] { c21, c22 } };
                    var e3 = new Entity3101 { Id = 3, Children = new[] { c31, c32 } };

                    e2.RootEntity = e1;

                    context.Entities.AddRange(e1, e2, e3);
                    context.SaveChanges();
                });
        }

        public class MyContext3101 : DbContext
        {
            private readonly IServiceProvider _serviceProvider;

            public MyContext3101(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public DbSet<Entity3101> Entities { get; set; }

            public DbSet<Child3101> Children { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlServer(SqlServerTestStore.CreateConnectionString("Repro3101"));

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Entity3101>().Property(e => e.Id).ValueGeneratedNever();
            }
        }

        public class Entity3101
        {
            public Entity3101()
            {
                Children = new Collection<Child3101>();
            }

            public int Id { get; set; }

            public int? RootEntityId { get; set; }

            public Entity3101 RootEntity { get; set; }

            public ICollection<Child3101> Children { get; set; }
        }

        public class Child3101
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Fact]
        public virtual void Repro5456_include_group_join_is_per_query_context()
        {
            CreateDatabase5456();

            Parallel.For(0, 10, i =>
            {
                using (var ctx = new MyContext5456())
                {
                    var result = ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).ToList();

                    Assert.Equal(198, result.Count);
                }
            });
        }

        [Fact]
        public virtual void Repro5456_include_group_join_is_per_query_context_async()
        {
            CreateDatabase5456();

            Parallel.For(0, 10, async i =>
            {
                using (var ctx = new MyContext5456())
                {
                    var result = await ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).ToListAsync();

                    Assert.Equal(198, result.Count);
                }
            });
        }

        [Fact]
        public virtual void Repro5456_multiple_include_group_join_is_per_query_context()
        {
            CreateDatabase5456();

            Parallel.For(0, 10, i =>
            {
                using (var ctx = new MyContext5456())
                {
                    var result = ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).Include(x => x.Comments).ToList();

                    Assert.Equal(198, result.Count);
                }
            });
        }

        [Fact]
        public virtual void Repro5456_multiple_include_group_join_is_per_query_context_async()
        {
            CreateDatabase5456();

            Parallel.For(0, 10, async i =>
            {
                using (var ctx = new MyContext5456())
                {
                    var result = await ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).Include(x => x.Comments).ToListAsync();

                    Assert.Equal(198, result.Count);
                }
            });
        }

        [Fact]
        public virtual void Repro5456_multi_level_include_group_join_is_per_query_context()
        {
            CreateDatabase5456();

            Parallel.For(0, 10, i =>
            {
                using (var ctx = new MyContext5456())
                {
                    var result = ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).ThenInclude(b => b.Author).ToList();

                    Assert.Equal(198, result.Count);
                }
            });
        }

        [Fact]
        public virtual void Repro5456_multi_level_include_group_join_is_per_query_context_async()
        {
            CreateDatabase5456();

            Parallel.For(0, 10, async i =>
            {
                using (var ctx = new MyContext5456())
                {
                    var result = await ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).ThenInclude(b => b.Author).ToListAsync();

                    Assert.Equal(198, result.Count);
                }
            });
        }

        private void CreateDatabase5456()
        {
            CreateTestStore(
                "Repro5456",
                _fixture.ServiceProvider,
                (sp, co) => new MyContext5456(),
                context =>
                {
                    for (var i = 0; i < 100; i++)
                    {
                        context.Add(
                            new Blog5456
                            {
                                Posts = new List<Post5456>
                                {
                                    new Post5456
                                    {
                                        Comments = new List<Comment5456>
                                        {
                                            new Comment5456(),
                                            new Comment5456()
                                        }
                                    },
                                    new Post5456()
                                },
                                Author = new Author5456()
                            });
                    }
                    context.SaveChanges();
                });
        }

        public class MyContext5456 : DbContext
        {
            public DbSet<Blog5456> Blogs { get; set; }
            public DbSet<Post5456> Posts { get; set; }
            public DbSet<Comment5456> Comments { get; set; }
            public DbSet<Author5456> Authors { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlServer(SqlServerTestStore.CreateConnectionString("Repro5456"));
        }

        public class Blog5456
        {
            public int Id { get; set; }
            public List<Post5456> Posts { get; set; }
            public Author5456 Author { get; set; }
        }

        public class Author5456
        {
            public int Id { get; set; }
            public List<Blog5456> Blogs { get; set; }
        }

        public class Post5456
        {
            public int Id { get; set; }
            public Blog5456 Blog { get; set; }
            public List<Comment5456> Comments { get; set; }
        }

        public class Comment5456
        {
            public int Id { get; set; }
            public Post5456 Blog { get; set; }
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
                    context.Database.EnsureClean();
                    contextInitializer(context);

                    TestSqlLoggerFactory.Reset();
                }
            });
        }
    }
}
