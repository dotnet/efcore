// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable AccessToDisposedClosure
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Local
namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class QueryBugsTest : IClassFixture<SqlServerFixture>
    {
        private readonly SqlServerFixture _fixture;

        public QueryBugsTest(SqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        {
            _fixture = fixture;

            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        #region Bug6901

        [Fact]
        public void Left_outer_join_bug_6091()
        {
            using (var testStore = SqlServerTestStore.GetOrCreateShared("QueryBugsTest", null))
            {
                testStore.ExecuteNonQuery(@"
CREATE TABLE [dbo].[Customers](
    [CustomerID] [int] NOT NULL PRIMARY KEY,
    [CustomerName] [varchar](120) NULL,
    [PostcodeID] [int] NULL);

CREATE TABLE [dbo].[Postcodes](
    [PostcodeID] [int] NOT NULL PRIMARY KEY,
    [PostcodeValue] [varchar](100) NOT NULL,
    [TownName] [varchar](255) NOT NULL);

INSERT [dbo].[Customers] ([CustomerID], [CustomerName], [PostcodeID]) VALUES (1, N'Sam Tippet', 5);
INSERT [dbo].[Customers] ([CustomerID], [CustomerName], [PostcodeID]) VALUES (2, N'William Greig', 2);
INSERT [dbo].[Customers] ([CustomerID], [CustomerName], [PostcodeID]) VALUES (3, N'Steve Jones', 3);
INSERT [dbo].[Customers] ([CustomerID], [CustomerName], [PostcodeID]) VALUES (4, N'Jim Warren', NULL);
INSERT [dbo].[Customers] ([CustomerID], [CustomerName], [PostcodeID]) VALUES (5, N'Andrew Smith', 5);

INSERT [dbo].[Postcodes] ([PostcodeID], [PostcodeValue], [TownName]) VALUES (2, N'1000', N'Town 1');
INSERT [dbo].[Postcodes] ([PostcodeID], [PostcodeValue], [TownName]) VALUES (3, N'2000', N'Town 2');
INSERT [dbo].[Postcodes] ([PostcodeID], [PostcodeValue], [TownName]) VALUES (4, N'3000', N'Town 3');
INSERT [dbo].[Postcodes] ([PostcodeID], [PostcodeValue], [TownName]) VALUES (5, N'4000', N'Town 4');
");
                var loggingFactory = new TestSqlLoggerFactory();
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkSqlServer()
                    .AddSingleton<ILoggerFactory>(loggingFactory)
                    .BuildServiceProvider();

                using (var context = new Bug6091Context(serviceProvider, testStore.ConnectionString))
                {
                    var customers
                        = from customer in context.Customers
                          join postcode in context.Postcodes
                          on customer.PostcodeID equals postcode.PostcodeID into custPCTmp
                          from custPC in custPCTmp.DefaultIfEmpty()
                          select new
                          {
                              customer.CustomerID,
                              customer.CustomerName,
                              TownName = custPC == null ? string.Empty : custPC.TownName,
                              PostcodeValue = custPC == null ? string.Empty : custPC.PostcodeValue
                          };

                    var results = customers.ToList();

                    Assert.Equal(5, results.Count);
                    Assert.True(results[3].CustomerName != results[4].CustomerName);
                }
            }
        }

        private class Bug6091Context : DbContext
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly string _connectionString;

            public Bug6091Context(IServiceProvider serviceProvider, string connectionString)
            {
                _serviceProvider = serviceProvider;
                _connectionString = connectionString;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseInternalServiceProvider(_serviceProvider).UseSqlServer(_connectionString, b => b.ApplyConfiguration());

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Customer>().ToTable("Customers");

            public DbSet<Customer> Customers { get; set; }
            public DbSet<Postcode> Postcodes { get; set; }

            public class Customer
            {
                public int CustomerID { get; set; }
                public string CustomerName { get; set; }
                public int? PostcodeID { get; set; }
            }

            public class Postcode
            {
                public int PostcodeID { get; set; }
                public string PostcodeValue { get; set; }
                public string TownName { get; set; }
            }
        }

        #endregion

        #region Bug5481
        [Fact]
        public async Task Multiple_optional_navs_should_not_deadlock_bug_5481()
        {
            using (var testStore = SqlServerTestStore.Create("QueryBugsTest"))
            {
                using (var context = new DeadlockContext(testStore.ConnectionString))
                {
                    context.Database.EnsureCreated();
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
                => optionsBuilder.UseSqlServer(_connectionString, b => b.ApplyConfiguration());

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

        #endregion

        [Fact]
        public void Query_when_null_key_in_database_should_throw()
        {
            using (var testStore = SqlServerTestStore.GetOrCreateShared("QueryBugsTest", null))
            {
                testStore.ExecuteNonQuery(
                    @"CREATE TABLE ZeroKey (Id int);
                      INSERT ZeroKey VALUES (NULL)");

                using (var context = new NullKeyContext(testStore.ConnectionString))
                {
                    Assert.Equal(
                        CoreStrings.InvalidKeyValue("ZeroKey", "Id"),
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
                => optionsBuilder.UseSqlServer(_connectionString, b => b.ApplyConfiguration());

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<ZeroKey>().ToTable("ZeroKey");

            public DbSet<ZeroKey> ZeroKeys { get; set; }

            public class ZeroKey
            {
                public int Id { get; set; }
            }
        }

        #region Bug603

        [Fact]
        public async Task First_FirstOrDefault_ix_async_bug_603()
        {
            using (CreateDatabase603())
            {
                using (var context = new MyContext603(_options))
                {
                    context.Products.Add(new Product { Name = "Product 1" });
                    context.SaveChanges();
                }

                using (var ctx = new MyContext603(_options))
                {
                    var product = await ctx.Products.FirstAsync();

                    ctx.Products.Remove(product);

                    await ctx.SaveChangesAsync();
                }
            }

            using (CreateDatabase603())
            {
                using (var context = new MyContext603(_options))
                {
                    context.Products.Add(new Product { Name = "Product 1" });
                    context.SaveChanges();
                }

                using (var ctx = new MyContext603(_options))
                {
                    var product = await ctx.Products.FirstOrDefaultAsync();

                    ctx.Products.Remove(product);

                    await ctx.SaveChangesAsync();
                }
            }
        }

        private class Product
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        private class MyContext603 : DbContext
        {
            public MyContext603(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Product> Products { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Product>().ToTable("Product");
        }

        private SqlServerTestStore CreateDatabase603()
            => CreateTestStore(() => new MyContext603(_options), null);

        #endregion

        #region Bugs925_926
        [Fact]
        public void Include_on_entity_with_composite_key_One_To_Many_bugs_925_926()
        {
            using (CreateDatabase925())
            {
                using (var ctx = new MyContext925(_options))
                {
                    var query = ctx.Customers.Include(c => c.Orders).OrderBy(c => c.FirstName).ThenBy(c => c.LastName);
                    var result = query.ToList();

                    Assert.Equal(2, result.Count);
                    Assert.Equal(2, result[0].Orders.Count);
                    Assert.Equal(3, result[1].Orders.Count);

                    Assert.Equal(
                        @"SELECT [c].[FirstName], [c].[LastName]
FROM [Customer] AS [c]
ORDER BY [c].[FirstName], [c].[LastName]

SELECT [c.Orders].[Id], [c.Orders].[CustomerFirstName], [c.Orders].[CustomerLastName], [c.Orders].[Name]
FROM [Order] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[FirstName], [c0].[LastName]
    FROM [Customer] AS [c0]
) AS [t] ON ([c.Orders].[CustomerFirstName] = [t].[FirstName]) AND ([c.Orders].[CustomerLastName] = [t].[LastName])
ORDER BY [t].[FirstName], [t].[LastName]", 
                        Sql);
                }
            }
        }

        [Fact]
        public void Include_on_entity_with_composite_key_Many_To_One_bugs_925_926()
        {
            using (CreateDatabase925())
            {
                using (var ctx = new MyContext925(_options))
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
                        @"SELECT [o].[Id], [o].[CustomerFirstName], [o].[CustomerLastName], [o].[Name], [o.Customer].[FirstName], [o.Customer].[LastName]
FROM [Order] AS [o]
LEFT JOIN [Customer] AS [o.Customer] ON ([o].[CustomerFirstName] = [o.Customer].[FirstName]) AND ([o].[CustomerLastName] = [o.Customer].[LastName])";

                    Assert.Equal(expectedSql, Sql);
                }
            }
        }

        private SqlServerTestStore CreateDatabase925()
            => CreateTestStore(() => new MyContext925(_options),
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
            public MyContext925(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Customer> Customers { get; set; }
            public DbSet<Order> Orders { get; set; }

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

        #endregion

        #region Bug7293

        [Fact]
        public void GroupJoin_expansion_when_optional_nav_in_projection()
        {
            using (CreateDatabase7293())
            {
                using (var context = new Context7293(_options))
                {
                    //TestSqlLoggerFactory.CaptureOutput(_testOutputHelper);

                    var query = from p in context.Project
                                select new ProjectView
                                {
                                    Permissions
                                        = from u in p.User
                                          select new PermissionView
                                          {
                                              UserName = u.User.Name
                                          }
                                };

                    var target = context.ProjectUser.First();

                    query.SingleOrDefault(item => item.Id == target.ProjectId);
                }
            }
        }

        private interface IHasKey
        {
            Guid Id { get; set; }
        }

        private class Project : IHasKey
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public ISet<ProjectUser> User { get; set; }
        }

        private class ProjectUser : IHasKey
        {
            public Guid Id { get; set; }
            public Guid ProjectId { get; set; }
            public Project Project { get; set; }
            public Guid UserId { get; set; }
            public User User { get; set; }
        }

        private class User : IHasKey
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        private class ProjectView : IHasKey
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public IEnumerable<PermissionView> Permissions { get; set; }
        }

        private class PermissionView : IHasKey
        {
            public Guid Id { get; set; }
            public Guid UserId { get; set; }
            public string UserName { get; set; }

        }

        private class Context7293 : DbContext
        {
            public Context7293(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Project> Project { get; set; }
            public DbSet<ProjectUser> ProjectUser { get; set; }
            public DbSet<User> User { get; set; }
        }

        private SqlServerTestStore CreateDatabase7293()
            => CreateTestStore(() => new Context7293(_options),
                context =>
                    {
                        var projects = new[]
                        {
                            new Project { Name = "Project 1" },
                            new Project { Name = "Project 2" },
                            new Project { Name = "Project 3" }
                        };

                        context.Project.AddRange(projects);

                        var users = new[]
                        {
                            new User { Name = "User 1" },
                            new User { Name = "User 2" },
                            new User { Name = "User 3" }
                        };

                        context.User.AddRange(users);

                        var permissions = (from project in projects
                                           from user in users
                                           select new ProjectUser
                                           {
                                               ProjectId = project.Id,
                                               Project = project,
                                               UserId = user.Id,
                                               User = user
                                           }).ToList();

                        context.ProjectUser.AddRange(permissions);
                        context.SaveChanges();
                    });

        #endregion

        #region Bug963

        [Fact]
        public void Include_on_optional_navigation_One_To_Many_963()
        {
            using (CreateDatabase963())
            {
                using (var ctx = new MyContext963(_options))
                {
                    ctx.Targaryens.Include(t => t.Dragons).ToList();
                }
            }
        }

        [Fact]
        public void Include_on_optional_navigation_Many_To_One_963()
        {
            using (CreateDatabase963())
            {
                using (var ctx = new MyContext963(_options))
                {
                    ctx.Dragons.Include(d => d.Mother).ToList();
                }
            }
        }

        [Fact]
        public void Include_on_optional_navigation_One_To_One_principal_963()
        {
            using (CreateDatabase963())
            {
                using (var ctx = new MyContext963(_options))
                {
                    ctx.Targaryens.Include(t => t.Details).ToList();
                }
            }
        }

        [Fact]
        public void Include_on_optional_navigation_One_To_One_dependent_963()
        {
            using (CreateDatabase963())
            {
                using (var ctx = new MyContext963(_options))
                {
                    ctx.Details.Include(d => d.Targaryen).ToList();
                }
            }
        }

        [Fact]
        public void Join_on_optional_navigation_One_To_Many_963()
        {
            using (CreateDatabase963())
            {
                using (var ctx = new MyContext963(_options))
                {
                    (from t in ctx.Targaryens
                     join d in ctx.Dragons on t.Id equals d.MotherId
                     select d).ToList();
                }
            }
        }

        private SqlServerTestStore CreateDatabase963()
            => CreateTestStore(() => new MyContext963(_options),
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
            public MyContext963(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Targaryen> Targaryens { get; set; }
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public DbSet<Details> Details { get; set; }
            public DbSet<Dragon> Dragons { get; set; }

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

        #endregion

        #region Bug1742
        [Fact]
        public void Compiler_generated_local_closure_produces_valid_parameter_name_1742()
            => Execute1742(new CustomerDetails_1742 { FirstName = "Foo", LastName = "Bar" });

        public void Execute1742(CustomerDetails_1742 details)
        {
            using (CreateDatabase925())
            {
                using (var ctx = new MyContext925(_options))
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
        }

        public class CustomerDetails_1742
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        #endregion

        #region Bug3758

        [Fact]
        public void Customer_collections_materialize_properly_3758()
        {
            using (CreateDatabase3758())
            {
                using (var ctx = new MyContext3758(_options))
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
        }

        public class MyContext3758 : DbContext
        {
            public MyContext3758(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Customer3758> Customers { get; set; }
            public DbSet<Order3758> Orders { get; set; }

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

        private SqlServerTestStore CreateDatabase3758()
            => CreateTestStore(() => new MyContext3758(_options),
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
        #endregion

        #region Bug3409

        [Fact(Skip = "Issue #7573")]
        public void ThenInclude_with_interface_navigations_3409()
        {
            using (CreateDatabase3409())
            {
                using (var context = new MyContext3409(_options))
                {
                    var results = context.Parents
                        .Include(p => p.ChildCollection)
                        .ThenInclude(c => c.SelfReferenceCollection)
                        .ToList();

                    Assert.Equal(1, results.Count);
                    Assert.Equal(1, results[0].ChildCollection.Count);
                    Assert.Equal(2, results[0].ChildCollection.Single().SelfReferenceCollection.Count);
                }

                using (var context = new MyContext3409(_options))
                {
                    var results = context.Children
                        .Select(c => new
                        {
                            c.SelfReferenceBackNavigation,
                            c.SelfReferenceBackNavigation.ParentBackNavigation
                        })
                        .ToList();

                    Assert.Equal(3, results.Count);
                    Assert.Equal(2, results.Count(c => c.SelfReferenceBackNavigation != null));
                    Assert.Equal(1, results.Count(c => c.ParentBackNavigation != null));
                }

                using (var context = new MyContext3409(_options))
                {
                    var results = context.Children
                        .Select(c => new
                        {
                            SelfReferenceBackNavigation
                            = EF.Property<IChild3409>(c, "SelfReferenceBackNavigation"),
                            ParentBackNavigationB
                            = EF.Property<IParent3409>(
                                EF.Property<IChild3409>(c, "SelfReferenceBackNavigation"),
                                "ParentBackNavigation")
                        })
                        .ToList();

                    Assert.Equal(3, results.Count);
                    Assert.Equal(2, results.Count(c => c.SelfReferenceBackNavigation != null));
                    Assert.Equal(1, results.Count(c => c.ParentBackNavigationB != null));
                }

                using (var context = new MyContext3409(_options))
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
        }

        public class MyContext3409 : DbContext
        {
            public DbSet<Parent3409> Parents { get; set; }
            public DbSet<Child3409> Children { get; set; }

            public MyContext3409(DbContextOptions options)
                : base(options)
            {
            }

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

        private SqlServerTestStore CreateDatabase3409()
            => CreateTestStore(() => new MyContext3409(_options),
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

        #endregion

        #region Bug3101

        [Fact]
        public virtual void Repro3101_simple_coalesce1()
        {
            using (CreateDatabase3101())
            {
                using (var ctx = new MyContext3101(_options))
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
        }

        [Fact]
        public virtual void Repro3101_simple_coalesce2()
        {
            using (CreateDatabase3101())
            {
                using (var ctx = new MyContext3101(_options))
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
        }

        [Fact]
        public virtual void Repro3101_simple_coalesce3()
        {
            using (CreateDatabase3101())
            {
                using (var ctx = new MyContext3101(_options))
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
        }

        [Fact]
        public virtual void Repro3101_complex_coalesce1()
        {
            using (CreateDatabase3101())
            {
                using (var ctx = new MyContext3101(_options))
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
        }

        [Fact]
        public virtual void Repro3101_complex_coalesce2()
        {
            using (CreateDatabase3101())
            {
                using (var ctx = new MyContext3101(_options))
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
        }

        [Fact]
        public virtual void Repro3101_nested_coalesce1()
        {
            using (CreateDatabase3101())
            {
                using (var ctx = new MyContext3101(_options))
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
        }

        [Fact]
        public virtual void Repro3101_nested_coalesce2()
        {
            using (CreateDatabase3101())
            {
                using (var ctx = new MyContext3101(_options))
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
        }

        [Fact]
        public virtual void Repro3101_conditional()
        {
            using (CreateDatabase3101())
            {
                using (var ctx = new MyContext3101(_options))
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
        }

        [Fact]
        public virtual void Repro3101_coalesce_tracking()
        {
            using (CreateDatabase3101())
            {
                using (var ctx = new MyContext3101(_options))
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
        }

        private SqlServerTestStore CreateDatabase3101()
            => CreateTestStore(() => new MyContext3101(_options),
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

        public class MyContext3101 : DbContext
        {
            public MyContext3101(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Entity3101> Entities { get; set; }

            public DbSet<Child3101> Children { get; set; }

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

        #endregion

        #region Bug6986

        [Fact]
        public virtual void Repro6986_can_query_base_type_when_derived_types_contain_shadow_properties()
        {
            using (CreateDatabase6986())
            {
                using (var context = new ReproContext6986(_options))
                {
                    var query = context.Contacts.ToList();

                    Assert.Equal(4, query.Count);
                    Assert.Equal(2, query.OfType<EmployerContact6986>().Count());
                    Assert.Equal(1, query.OfType<ServiceOperatorContact6986>().Count());
                }
            }
        }

        [Fact]
        public virtual void Repro6986_can_include_dependent_to_principal_navigation_of_derived_type_with_shadow_fk()
        {
            using (CreateDatabase6986())
            {
                using (var context = new ReproContext6986(_options))
                {
                    var query = context.Contacts.OfType<ServiceOperatorContact6986>().Include(e => e.ServiceOperator6986).ToList();

                    Assert.Equal(1, query.Count);
                    Assert.NotNull(query[0].ServiceOperator6986);
                }
            }
        }

        [Fact]
        public virtual void Repro6986_can_project_shadow_property_using_ef_property()
        {
            using (CreateDatabase6986())
            {
                using (var context = new ReproContext6986(_options))
                {
                    var query = context.Contacts.OfType<ServiceOperatorContact6986>().Select(c => new { c, Prop = EF.Property<int>(c, "ServiceOperator6986Id") }).ToList();

                    Assert.Equal(1, query.Count);
                    Assert.Equal(1, query[0].Prop);
                }
            }
        }

        private SqlServerTestStore CreateDatabase6986()
            => CreateTestStore(() => new ReproContext6986(_options),
                context =>
                {
                    context.ServiceOperators.Add(new ServiceOperator6986());
                    context.Employers.AddRange(
                        new Employer6986 { Name = "UWE" },
                        new Employer6986 { Name = "Hewlett Packard" });

                    context.SaveChanges();

                    context.Contacts.AddRange(
                        new ServiceOperatorContact6986
                        {
                            UserName = "service.operator@esoterix.co.uk",
                            ServiceOperator6986 = context.ServiceOperators.First()
                        },
                        new EmployerContact6986
                        {
                            UserName = "uwe@esoterix.co.uk",
                            Employer6986 = context.Employers.First(e => e.Name == "UWE")
                        },
                        new EmployerContact6986
                        {
                            UserName = "hp@esoterix.co.uk",
                            Employer6986 = context.Employers.First(e => e.Name == "Hewlett Packard")
                        },
                        new Contact6986
                        {
                            UserName = "noroles@esoterix.co.uk",
                        });
                    context.SaveChanges();
                });

        public class ReproContext6986 : DbContext
        {

            public ReproContext6986(DbContextOptions options)
                : base(options)
            { }

            public DbSet<Contact6986> Contacts { get; set; }
            public DbSet<EmployerContact6986> EmployerContacts { get; set; }
            public DbSet<Employer6986> Employers { get; set; }
            public DbSet<ServiceOperatorContact6986> ServiceOperatorContacts { get; set; }
            public DbSet<ServiceOperator6986> ServiceOperators { get; set; }
        }

        public class EmployerContact6986 : Contact6986
        {
            [Required]
            public Employer6986 Employer6986 { get; set; }
        }

        public class ServiceOperatorContact6986 : Contact6986
        {
            [Required]
            public ServiceOperator6986 ServiceOperator6986 { get; set; }
        }

        public class Contact6986
        {
            public int Id { get; set; }
            public string UserName { get; set; }
            public bool IsPrimary { get; set; }
        }

        public class Employer6986
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<EmployerContact6986> Contacts { get; set; }
        }

        public class ServiceOperator6986
        {
            public int Id { get; set; }
            public List<ServiceOperatorContact6986> Contacts { get; set; }
        }

        #endregion

        #region Bug5456

        [Fact]
        public virtual void Repro5456_include_group_join_is_per_query_context()
        {
            using (CreateDatabase5456())
            {
                Parallel.For(0, 10, i =>
                    {
                        using (var ctx = new MyContext5456(_options))
                        {
                            var result = ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).ToList();

                            Assert.Equal(198, result.Count);
                        }
                    });
            }
        }

        [Fact]
        public virtual void Repro5456_include_group_join_is_per_query_context_async()
        {
            using (CreateDatabase5456())
            {
                Parallel.For(0, 10, async i =>
                    {
                        using (var ctx = new MyContext5456(_options))
                        {
                            var result = await ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).ToListAsync();

                            Assert.Equal(198, result.Count);
                        }
                    });
            }
        }

        [Fact]
        public virtual void Repro5456_multiple_include_group_join_is_per_query_context()
        {
            using (CreateDatabase5456())
            {
                Parallel.For(0, 10, i =>
                    {
                        using (var ctx = new MyContext5456(_options))
                        {
                            var result = ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).Include(x => x.Comments).ToList();

                            Assert.Equal(198, result.Count);
                        }
                    });
            }
        }

        [Fact]
        public virtual void Repro5456_multiple_include_group_join_is_per_query_context_async()
        {
            using (CreateDatabase5456())
            {
                Parallel.For(0, 10, async i =>
                    {
                        using (var ctx = new MyContext5456(_options))
                        {
                            var result = await ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).Include(x => x.Comments).ToListAsync();

                            Assert.Equal(198, result.Count);
                        }
                    });
            }
        }

        [Fact]
        public virtual void Repro5456_multi_level_include_group_join_is_per_query_context()
        {
            using (CreateDatabase5456())
            {
                Parallel.For(0, 10, i =>
                    {
                        using (var ctx = new MyContext5456(_options))
                        {
                            var result = ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).ThenInclude(b => b.Author).ToList();

                            Assert.Equal(198, result.Count);
                        }
                    });
            }
        }

        [Fact]
        public virtual void Repro5456_multi_level_include_group_join_is_per_query_context_async()
        {
            using (CreateDatabase5456())
            {
                Parallel.For(0, 10, async i =>
                    {
                        using (var ctx = new MyContext5456(_options))
                        {
                            var result = await ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).ThenInclude(b => b.Author).ToListAsync();

                            Assert.Equal(198, result.Count);
                        }
                    });
            }
        }

        private SqlServerTestStore CreateDatabase5456()
            => CreateTestStore(() => new MyContext5456(_options),
                context =>
                    {
                        for (var i = 0; i < 100; i++)
                        {
                            context.Add(new Blog5456
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

        public class MyContext5456 : DbContext
        {
            public MyContext5456(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Blog5456> Blogs { get; set; }
            public DbSet<Post5456> Posts { get; set; }
            public DbSet<Comment5456> Comments { get; set; }
            public DbSet<Author5456> Authors { get; set; }
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

        #endregion

        #region Bug7359

        [Fact]
        public virtual void Discriminator_type_is_handled_correctly_in_materialization_bug_7359()
        {
            using (CreateDatabase7359())
            {
                using (var ctx = new MyContext7359(_options))
                {
                    var query = ctx.Products.OfType<SpecialProduct>().ToList();

                    Assert.Equal(1, query.Count);
                }
            }
        }

        [Fact]
        public virtual void Discriminator_type_is_handled_correctly_with_is_operator_bug_7359()
        {
            using (CreateDatabase7359())
            {
                using (var ctx = new MyContext7359(_options))
                {
                    var query = ctx.Products.Where(p => p is SpecialProduct).ToList();

                    Assert.Equal(1, query.Count);
                }
            }
        }

        private class SpecialProduct : Product
        {
        }

        private class MyContext7359 : DbContext
        {
            public MyContext7359(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Product> Products { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<SpecialProduct>();
                modelBuilder.Entity<Product>()
                    .HasDiscriminator<int?>("Discriminator")
                    .HasValue(0)
                    .HasValue<SpecialProduct>(1);
            }
        }

        private SqlServerTestStore CreateDatabase7359()
            => CreateTestStore(() => new MyContext7359(_options),
                context =>
                    {
                        context.Add(new Product { Name = "Product1" });
                        context.Add(new SpecialProduct { Name = "SpecialProduct" });
                        context.SaveChanges();
                    });

        #endregion

        #region Bug7312

        [Fact]
        public virtual void Reference_include_on_derived_type_with_sibling_works_bug_7312()
        {
            using (CreateDatabase7312())
            {
                using (var context = new MyContext7312(_options))
                {
                    var query = context.Proposal.OfType<ProposalLeave7312>().Include(l => l.LeaveType).ToList();

                    Assert.Equal(1, query.Count);
                }
            }
        }

        public class Proposal7312
        {
            public int Id { get; set; }
        }

        public class ProposalCustom7312 : Proposal7312
        {
            public string Name { get; set; }
        }

        public class ProposalLeave7312 : Proposal7312
        {
            public DateTime LeaveStart { get; set; }
            public virtual ProposalLeaveType7312 LeaveType { get; set; }
        }

        public class ProposalLeaveType7312
        {
            public int Id { get; set; }
            public ICollection<ProposalLeave7312> ProposalLeaves { get; set; }
        }

        private class MyContext7312 : DbContext
        {
            public MyContext7312(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Proposal7312> Proposal { get; set; }
            public DbSet<ProposalCustom7312> ProposalCustoms { get; set; }
            public DbSet<ProposalLeave7312> ProposalLeaves { get; set; }
        }

        private SqlServerTestStore CreateDatabase7312()
            => CreateTestStore(() => new MyContext7312(_options),
                context =>
                {
                    context.AddRange(
                        new Proposal7312(),
                        new ProposalCustom7312
                        {
                            Name = "CustomProposal",
                        },
                        new ProposalLeave7312
                        {
                            LeaveStart = DateTime.Now,
                            LeaveType = new ProposalLeaveType7312()
                        }
                    );
                    context.SaveChanges();
                });

        #endregion


        private DbContextOptions _options;

        private SqlServerTestStore CreateTestStore<TContext>(
            Func<TContext> contextCreator,
            Action<TContext> contextInitializer)
            where TContext : DbContext, IDisposable
        {
            var testStore = SqlServerTestStore.Create("QueryBugsTest");

            _options = new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                .UseSqlServer(testStore.ConnectionString, b => b.ApplyConfiguration())
                .UseInternalServiceProvider(_fixture.ServiceProvider)
                .Options;

            using (var context = contextCreator())
            {
                context.Database.EnsureCreated();
                contextInitializer?.Invoke(context);
            }

            TestSqlLoggerFactory.Reset();
            return testStore;
        }

        private const string FileLineEnding = @"
";

        protected virtual void ClearLog() => TestSqlLoggerFactory.Reset();

        private static string Sql => TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);
    }
}
