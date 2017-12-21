// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Caching.Memory;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable AccessToDisposedClosure
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Local
namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryBugsTest : IClassFixture<SqlServerFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public QueryBugsTest(SqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected SqlServerFixture Fixture { get; }

        #region Bug6901

        [Fact]
        public void Left_outer_join_bug_6091()
        {
            using (var testStore = SqlServerTestStore.CreateInitialized("QueryBugsTest"))
            {
                testStore.ExecuteNonQuery(
                    @"
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

                using (var context = new Bug6091Context(Fixture.CreateOptions(testStore)))
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
            public Bug6091Context(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>().ToTable("Customers");
            }

            public DbSet<Customer> Customers { get; set; }
            public DbSet<Postcode> Postcodes { get; set; }

            // ReSharper disable once MemberHidesStaticFromOuterClass
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
            using (var testStore = SqlServerTestStore.CreateInitialized("QueryBugsTest"))
            {
                using (var context = new DeadlockContext(Fixture.CreateOptions(testStore)))
                {
                    context.Database.EnsureCreated();
                    context.EnsureSeeded();

                    var count
                        = await context.Persons
                            .Where(
                                p => p.AddressOne != null && p.AddressOne.Street.Contains("Low Street")
                                     || p.AddressTwo != null && p.AddressTwo.Street.Contains("Low Street"))
                            .CountAsync();

                    Assert.Equal(0, count);
                }
            }
        }

        private class DeadlockContext : DbContext
        {
            public DeadlockContext(DbContextOptions options)
                : base(options)
            {
            }

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

                modelBuilder.Entity<Address>().Property(a => a.Street).IsRequired();

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
            using (var testStore = SqlServerTestStore.CreateInitialized("QueryBugsTest"))
            {
                testStore.ExecuteNonQuery(
                    @"CREATE TABLE ZeroKey (Id int);
                      INSERT ZeroKey VALUES (NULL)");

                using (var context = new NullKeyContext(Fixture.CreateOptions(testStore)))
                {
                    Assert.Equal(
                        CoreStrings.InvalidKeyValue("ZeroKey", "Id"),
                        Assert.Throws<InvalidOperationException>(() => context.ZeroKeys.ToList()).Message);
                }
            }
        }

        private class NullKeyContext : DbContext
        {
            public NullKeyContext(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ZeroKey>().ToTable("ZeroKey");
            }

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
                    var product = await ctx.Products.OrderBy(p => p.Id).FirstAsync();

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
                    var product = await ctx.Products.OrderBy(p => p.Id).FirstOrDefaultAsync();

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
            {
                modelBuilder.Entity<Product>().ToTable("Product");
            }
        }

        private SqlServerTestStore CreateDatabase603()
        {
            return CreateTestStore(() => new MyContext603(_options), null);
        }

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

                    AssertSql(
                        @"SELECT [c].[FirstName], [c].[LastName]
FROM [Customer] AS [c]
ORDER BY [c].[FirstName], [c].[LastName]",
                        //
                        @"SELECT [c.Orders].[Id], [c.Orders].[CustomerFirstName], [c.Orders].[CustomerLastName], [c.Orders].[Name]
FROM [Order] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[FirstName], [c0].[LastName]
    FROM [Customer] AS [c0]
) AS [t] ON ([c.Orders].[CustomerFirstName] = [t].[FirstName]) AND ([c.Orders].[CustomerLastName] = [t].[LastName])
ORDER BY [t].[FirstName], [t].[LastName]");
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

                    AssertSql(expectedSql);
                }
            }
        }

        private SqlServerTestStore CreateDatabase925()
        {
            return CreateTestStore(
                () => new MyContext925(_options),
                context =>
                    {
                        var order11 = new Order { Name = "Order11" };
                        var order12 = new Order { Name = "Order12" };
                        var order21 = new Order { Name = "Order21" };
                        var order22 = new Order { Name = "Order22" };
                        var order23 = new Order { Name = "Order23" };

                        var customer1 = new Customer
                        {
                            FirstName = "Customer",
                            LastName = "One",
                            Orders = new List<Order> { order11, order12 }
                        };
                        var customer2 = new Customer
                        {
                            FirstName = "Customer",
                            LastName = "Two",
                            Orders = new List<Order> { order21, order22, order23 }
                        };

                        context.Customers.AddRange(customer1, customer2);
                        context.Orders.AddRange(order11, order12, order21, order22, order23);
                        context.SaveChanges();

                        ClearLog();
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
            public MyContext925(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Customer> Customers { get; set; }
            public DbSet<Order> Orders { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>(
                    m =>
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

                    var query = from p in context.Projects
                                select new ProjectView
                                {
                                    Permissions
                                        = from u in p.ProjectUsers
                                          select new PermissionView
                                          {
                                              UserName = u.User.Name
                                          }
                                };

                    var target = context.ProjectUsers.OrderBy(u => u.Id).First();

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

            // ReSharper disable once CollectionNeverUpdated.Local
            public ISet<ProjectUser> ProjectUsers { get; set; }
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

            public DbSet<Project> Projects { get; set; }
            public DbSet<ProjectUser> ProjectUsers { get; set; }
            public DbSet<User> Users { get; set; }
        }

        private SqlServerTestStore CreateDatabase7293()
        {
            return CreateTestStore(
                () => new Context7293(_options),
                context =>
                    {
                        var projects = new[]
                        {
                            new Project { Name = "Projects 1" },
                            new Project { Name = "Projects 2" },
                            new Project { Name = "Projects 3" }
                        };

                        context.Projects.AddRange(projects);

                        var users = new[]
                        {
                            new User { Name = "Users 1" },
                            new User { Name = "Users 2" },
                            new User { Name = "Users 3" }
                        };

                        context.Users.AddRange(users);

                        var permissions = (from project in projects
                                           from user in users
                                           select new ProjectUser
                                           {
                                               ProjectId = project.Id,
                                               Project = project,
                                               UserId = user.Id,
                                               User = user
                                           }).ToList();

                        context.ProjectUsers.AddRange(permissions);
                        context.SaveChanges();
                    });
        }

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
        {
            return CreateTestStore(
                () => new MyContext963(_options),
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
                modelBuilder.Entity<Targaryen>(
                    m =>
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
        {
            Execute1742(new CustomerDetails_1742 { FirstName = "Foo", LastName = "Bar" });
        }

        public void Execute1742(CustomerDetails_1742 details)
        {
            using (CreateDatabase925())
            {
                using (var ctx = new MyContext925(_options))
                {
                    var firstName = details.FirstName;

                    ctx.Customers.Where(c => c.FirstName == firstName && c.LastName == details.LastName).ToList();

                    const string expectedSql
                        = @"@__firstName_0='Foo' (Size = 450)
@__8__locals1_details_LastName_1='Bar' (Size = 450)

SELECT [c].[FirstName], [c].[LastName]
FROM [Customer] AS [c]
WHERE ([c].[FirstName] = @__firstName_0) AND ([c].[LastName] = @__8__locals1_details_LastName_1)";

                    AssertSql(expectedSql);
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
                        CoreStrings.NavigationCannotCreateType(
                            "Orders4", typeof(Customer3758).Name,
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
                modelBuilder.Entity<Customer3758>(
                    b =>
                        {
                            b.ToTable("Customer3758");

                            b.HasMany(e => e.Orders1).WithOne().HasForeignKey("CustomerId1");
                            b.HasMany(e => e.Orders2).WithOne().HasForeignKey("CustomerId2");
                            b.HasMany(e => e.Orders3).WithOne().HasForeignKey("CustomerId3");
                            b.HasMany(e => e.Orders4).WithOne().HasForeignKey("CustomerId4");
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
                var _ = argument;
            }
        }

        private SqlServerTestStore CreateDatabase3758()
        {
            return CreateTestStore(
                () => new MyContext3758(_options),
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
                        context.Orders.AddRange(
                            o111, o112, o121, o122,
                            o131, o132, o141, o211,
                            o212, o221, o222, o231,
                            o232, o241);

                        context.SaveChanges();
                    });
        }

        #endregion

        #region Bug3409

        [Fact]
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
                        .Select(
                            c => new
                            {
                                c.SelfReferenceBackNavigation,
                                c.SelfReferenceBackNavigation.ParentBackNavigation
                            })
                        .ToList();

                    Assert.Equal(3, results.Count);
                    Assert.Equal(2, results.Count(c => c.SelfReferenceBackNavigation != null));
                    Assert.Equal(2, results.Count(c => c.ParentBackNavigation != null));
                }

                using (var context = new MyContext3409(_options))
                {
                    var results = context.Children
                        .Select(
                            c => new
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
                    Assert.Equal(2, results.Count(c => c.ParentBackNavigationB != null));
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
        {
            return CreateTestStore(
                () => new MyContext3409(_options),
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
                                    // ReSharper disable once ConstantNullCoalescingCondition
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
                                    // ReSharper disable once ConstantNullCoalescingCondition
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
                                    // ReSharper disable once MergeConditionalExpression
#pragma warning disable IDE0029 // Use coalesce expression
                                select eRootJoined != null ? eRootJoined : eVersion;
#pragma warning restore IDE0029 // Use coalesce expression

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

                    query.ToList();

                    Assert.True(ctx.ChangeTracker.Entries().Any());
                }
            }
        }

        private SqlServerTestStore CreateDatabase3101()
        {
            return CreateTestStore(
                () => new MyContext3101(_options),
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
        {
            return CreateTestStore(
                () => new ReproContext6986(_options),
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
                                ServiceOperator6986 = context.ServiceOperators.OrderBy(o => o.Id).First()
                            },
                            new EmployerContact6986
                            {
                                UserName = "uwe@esoterix.co.uk",
                                Employer6986 = context.Employers.OrderBy(e => e.Id).First(e => e.Name == "UWE")
                            },
                            new EmployerContact6986
                            {
                                UserName = "hp@esoterix.co.uk",
                                Employer6986 = context.Employers.OrderBy(e => e.Id).First(e => e.Name == "Hewlett Packard")
                            },
                            new Contact6986
                            {
                                UserName = "noroles@esoterix.co.uk"
                            });
                        context.SaveChanges();
                    });
        }

        public class ReproContext6986 : DbContext
        {
            public ReproContext6986(DbContextOptions options)
                : base(options)
            {
            }

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
                Parallel.For(
                    0, 10, i =>
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
        public virtual async Task Repro5456_include_group_join_is_per_query_context_async()
        {
            using (CreateDatabase5456())
            {
                await Task.WhenAll(
                    Enumerable.Range(0, 10)
                        .Select(
                            async i =>
                                {
                                    using (var ctx = new MyContext5456(_options))
                                    {
                                        var result = await ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).ToListAsync();

                                        Assert.Equal(198, result.Count);
                                    }
                                }));
            }
        }

        [Fact]
        public virtual void Repro5456_multiple_include_group_join_is_per_query_context()
        {
            using (CreateDatabase5456())
            {
                Parallel.For(
                    0, 10, i =>
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
        public virtual async Task Repro5456_multiple_include_group_join_is_per_query_context_async()
        {
            using (CreateDatabase5456())
            {
                await Task.WhenAll(
                    Enumerable.Range(0, 10)
                        .Select(
                            async i =>
                                {
                                    using (var ctx = new MyContext5456(_options))
                                    {
                                        var result = await ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).Include(x => x.Comments).ToListAsync();

                                        Assert.Equal(198, result.Count);
                                    }
                                }));
            }
        }

        [Fact]
        public virtual void Repro5456_multi_level_include_group_join_is_per_query_context()
        {
            using (CreateDatabase5456())
            {
                Parallel.For(
                    0, 10, i =>
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
        public virtual async Task Repro5456_multi_level_include_group_join_is_per_query_context_async()
        {
            using (CreateDatabase5456())
            {
                await Task.WhenAll(
                    Enumerable.Range(0, 10)
                        .Select(
                            async i =>
                                {
                                    using (var ctx = new MyContext5456(_options))
                                    {
                                        var result = await ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).ThenInclude(b => b.Author).ToListAsync();

                                        Assert.Equal(198, result.Count);
                                    }
                                }));
            }
        }

        private SqlServerTestStore CreateDatabase5456()
        {
            return CreateTestStore(
                () => new MyContext5456(_options),
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
        {
            return CreateTestStore(
                () => new MyContext7359(_options),
                context =>
                    {
                        context.Add(new Product { Name = "Product1" });
                        context.Add(new SpecialProduct { Name = "SpecialProduct" });
                        context.SaveChanges();
                    });
        }

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
        {
            return CreateTestStore(
                () => new MyContext7312(_options),
                context =>
                    {
                        context.AddRange(
                            new Proposal7312(),
                            new ProposalCustom7312
                            {
                                Name = "CustomProposal"
                            },
                            new ProposalLeave7312
                            {
                                LeaveStart = DateTime.Now,
                                LeaveType = new ProposalLeaveType7312()
                            }
                        );
                        context.SaveChanges();
                    });
        }

        #endregion

        #region Bug8282

        [Fact]
        public virtual void Entity_passed_to_DTO_constructor_works()
        {
            using (CreateDatabase8282())
            {
                using (var context = new MyContext8282(_options))
                {
                    var query = context.Entity.Select(e => new EntityDto8282(e)).ToList();

                    Assert.Equal(1, query.Count);
                }
            }
        }

        public class Entity8282
        {
            public int Id { get; set; }
        }

        public class EntityDto8282
        {
            public EntityDto8282(Entity8282 entity)
            {
                Id = entity.Id;
            }

            public int Id { get; set; }
        }

        private class MyContext8282 : DbContext
        {
            public MyContext8282(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Entity8282> Entity { get; set; }
        }

        private SqlServerTestStore CreateDatabase8282()
        {
            return CreateTestStore(
                () => new MyContext8282(_options),
                context =>
                    {
                        context.AddRange(
                            new Entity8282()
                        );
                        context.SaveChanges();
                    });
        }

        #endregion

        #region Bug8538

        [Fact]
        public virtual void Enum_has_flag_applies_explicit_cast_for_long_constant()
        {
            using (CreateDatabase8538())
            {
                using (var context = new MyContext8538(_options))
                {
                    var query = context.Entity.Where(e => e.Permission.HasFlag(Permission.READ_WRITE)).ToList();

                    Assert.Equal(1, query.Count);

                    AssertSql(
                        @"SELECT [e].[Id], [e].[Permission]
FROM [Entity] AS [e]
WHERE ([e].[Permission] & CAST(17179869184 AS bigint)) = 17179869184");
                }
            }
        }

        [Fact]
        public virtual void Enum_has_flag_does_not_apply_explicit_cast_for_non_constant()
        {
            using (CreateDatabase8538())
            {
                using (var context = new MyContext8538(_options))
                {
                    var query = context.Entity.Where(e => e.Permission.HasFlag(e.Permission)).ToList();

                    Assert.Equal(3, query.Count);

                    AssertSql(
                        @"SELECT [e].[Id], [e].[Permission]
FROM [Entity] AS [e]
WHERE ([e].[Permission] & [e].[Permission]) = [e].[Permission]");
                }
            }
        }

        public class Entity8538
        {
            public int Id { get; set; }
            public Permission Permission { get; set; }
        }

        [Flags]
        public enum Permission : long
        {
            NONE = 0x01,
            READ_ONLY = 0x02,
            READ_WRITE = 0x400000000 // 36 bits
        }

        private class MyContext8538 : DbContext
        {
            public MyContext8538(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Entity8538> Entity { get; set; }
        }

        private SqlServerTestStore CreateDatabase8538()
        {
            return CreateTestStore(
                () => new MyContext8538(_options),
                context =>
                    {
                        context.AddRange(
                            new Entity8538 { Permission = Permission.NONE },
                            new Entity8538 { Permission = Permission.READ_ONLY },
                            new Entity8538 { Permission = Permission.READ_WRITE }
                        );
                        context.SaveChanges();

                        ClearLog();
                    });
        }

        #endregion

        #region Bug8909

        [Fact]
        public virtual void Variable_from_closure_is_parametrized()
        {
            using (CreateDatabase8909())
            {
                using (var context = new MyContext8909(_options))
                {
                    context.Cache.Compact(1);

                    var id = 1;
                    context.Entities.Where(c => c.Id == id).ToList();
                    Assert.Equal(1, context.Cache.Count);

                    id = 2;
                    context.Entities.Where(c => c.Id == id).ToList();
                    Assert.Equal(1, context.Cache.Count);

                    AssertSql(
                        @"@__id_0='1'

SELECT [c].[Id], [c].[Name]
FROM [Entities] AS [c]
WHERE [c].[Id] = @__id_0",
                        //
                        @"@__id_0='2'

SELECT [c].[Id], [c].[Name]
FROM [Entities] AS [c]
WHERE [c].[Id] = @__id_0");
                }
            }
        }

        [Fact]
        public virtual void Variable_from_nested_closure_is_parametrized()
        {
            using (CreateDatabase8909())
            {
                using (var context = new MyContext8909(_options))
                {
                    context.Cache.Compact(1);

                    var id = 0;
                    // ReSharper disable once AccessToModifiedClosure
                    Expression<Func<Entity8909, bool>> whereExpression = c => c.Id == id;

                    id = 1;
                    context.Entities.Where(whereExpression).ToList();
                    Assert.Equal(1, context.Cache.Count);

                    id = 2;
                    context.Entities.Where(whereExpression).ToList();
                    Assert.Equal(1, context.Cache.Count);

                    AssertSql(
                        @"@__id_0='1'

SELECT [c].[Id], [c].[Name]
FROM [Entities] AS [c]
WHERE [c].[Id] = @__id_0",
                        //
                        @"@__id_0='2'

SELECT [c].[Id], [c].[Name]
FROM [Entities] AS [c]
WHERE [c].[Id] = @__id_0");
                }
            }
        }

        [Fact]
        public virtual void Variable_from_multi_level_nested_closure_is_parametrized()
        {
            using (CreateDatabase8909())
            {
                using (var context = new MyContext8909(_options))
                {
                    context.Cache.Compact(1);

                    var id = 0;
                    // ReSharper disable once AccessToModifiedClosure
                    Expression<Func<Entity8909, bool>> whereExpression = c => c.Id == id;
                    Expression<Func<Entity8909, bool>> containsExpression = c => context.Entities.Where(whereExpression).Select(e => e.Id).Contains(c.Id);

                    id = 1;
                    context.Entities.Where(containsExpression).ToList();
                    Assert.Equal(1, context.Cache.Count);

                    id = 2;
                    context.Entities.Where(containsExpression).ToList();
                    Assert.Equal(1, context.Cache.Count);

                    AssertSql(
                        @"@__id_0='1'

SELECT [c].[Id], [c].[Name]
FROM [Entities] AS [c]
WHERE [c].[Id] IN (
    SELECT [c0].[Id]
    FROM [Entities] AS [c0]
    WHERE [c0].[Id] = @__id_0
)",
                        //
                        @"@__id_0='2'

SELECT [c].[Id], [c].[Name]
FROM [Entities] AS [c]
WHERE [c].[Id] IN (
    SELECT [c0].[Id]
    FROM [Entities] AS [c0]
    WHERE [c0].[Id] = @__id_0
)");
                }
            }
        }

        private SqlServerTestStore CreateDatabase8909()
        {
            return CreateTestStore(
                () => new MyContext8909(_options),
                context => { ClearLog(); });
        }

        public class MyContext8909 : DbContext
        {
            public MyContext8909(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Entity8909> Entities { get; set; }

            public MemoryCache Cache
            {
                get
                {
                    var compiledQueryCache = this.GetService<ICompiledQueryCache>();

                    return (MemoryCache)typeof(CompiledQueryCache).GetTypeInfo()
                        .GetField("_memoryCache", BindingFlags.Instance | BindingFlags.NonPublic)
                        ?.GetValue(compiledQueryCache);
                }
            }
        }

        public class Entity8909
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion

        #region Bug9202

        [Fact]
        public void Include_collection_for_entity_with_owned_type_works()
        {
            using (CreateDatabase9202())
            {
                using (var context = new MyContext9202(_options))
                {
                    var query = context.Movies.Include(m => m.Cast);
                    var result = query.ToList();

                    Assert.Equal(1, result.Count);
                    Assert.Equal(3, result[0].Cast.Count);
                    Assert.NotNull(result[0].Details);

                    AssertSql(
                        @"SELECT [m].[Id], [m].[Title], [m].[Id], [m].[Details_Info]
FROM [Movies] AS [m]
ORDER BY [m].[Id]",
                        //
                        @"SELECT [m.Cast].[Id], [m.Cast].[Movie9202Id], [m.Cast].[Name]
FROM [Actors] AS [m.Cast]
INNER JOIN (
    SELECT DISTINCT [m0].[Id]
    FROM [Movies] AS [m0]
) AS [t] ON [m.Cast].[Movie9202Id] = [t].[Id]
ORDER BY [t].[Id]");
                }
            }
        }

        private SqlServerTestStore CreateDatabase9202()
        {
            return CreateTestStore(
                () => new MyContext9202(_options),
                context =>
                    {
                        var av = new Actor9202 { Name = "Alicia Vikander" };
                        var oi = new Actor9202 { Name = "Oscar Isaac" };
                        var dg = new Actor9202 { Name = "Domhnall Gleeson" };
                        var em = new Movie9202 { Title = "Ex Machina", Cast = new List<Actor9202> { av, oi, dg }, Details = new Details9202 { Info = "Best movie ever made" } };
                        context.Actors.AddRange(av, oi, dg);
                        context.Movies.Add(em);
                        context.SaveChanges();

                        ClearLog();
                    });
        }

        public class MyContext9202 : DbContext
        {
            public MyContext9202(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Movie9202> Movies { get; set; }
            public DbSet<Actor9202> Actors { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Movie9202>().HasMany(m => m.Cast).WithOne();
                modelBuilder.Entity<Movie9202>().OwnsOne(m => m.Details);
            }
        }

        public class Movie9202
        {
            public int Id { get; set; }
            public string Title { get; set; }

            public List<Actor9202> Cast { get; set; }

            public Details9202 Details { get; set; }
        }

        public class Actor9202
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Details9202
        {
            public string Info { get; set; }
        }

        #endregion

        #region Bug9214

        [Fact]
        public void Default_schema_applied_when_no_function_schema()
        {
            using (CreateDatabase9214())
            {
                using (var context = new MyContext9214(_options))
                {
                    var result = context.Widgets.Where(w => w.Val == 1).Select(w => MyContext9214.AddOne(w.Val)).Single();

                    Assert.Equal(2, result);

                    AssertSql(
                        @"SELECT TOP(2) [foo].AddOne([w].[Val])
FROM [foo].[Widgets] AS [w]
WHERE [w].[Val] = 1");
                }
            }
        }

        [Fact]
        public void Default_schema_function_schema_overrides()
        {
            using (CreateDatabase9214())
            {
                using (var context = new MyContext9214(_options))
                {
                    var result = context.Widgets.Where(w => w.Val == 1).Select(w => MyContext9214.AddTwo(w.Val)).Single();

                    Assert.Equal(3, result);

                    AssertSql(
                        @"SELECT TOP(2) [dbo].AddTwo([w].[Val])
FROM [foo].[Widgets] AS [w]
WHERE [w].[Val] = 1");
                }
            }
        }

        private SqlServerTestStore CreateDatabase9214()
        {
            return CreateTestStore(
                () => new MyContext9214(_options),
                context =>
                    {
                        var w1 = new Widget9214 { Val = 1 };
                        var w2 = new Widget9214 { Val = 2 };
                        var w3 = new Widget9214 { Val = 3 };
                        context.Widgets.AddRange(w1, w2, w3);
                        context.SaveChanges();

                        context.Database.ExecuteSqlCommand(
                            @"CREATE FUNCTION foo.AddOne (@num int)
                                                            RETURNS int
                                                                AS
                                                            BEGIN  
                                                                return @num + 1 ;
                                                            END");

                        context.Database.ExecuteSqlCommand(
                            @"CREATE FUNCTION dbo.AddTwo (@num int)
                                                            RETURNS int
                                                                AS
                                                            BEGIN  
                                                                return @num + 2 ;
                                                            END");

                        ClearLog();
                    });
        }

        public class MyContext9214 : DbContext
        {
            public DbSet<Widget9214> Widgets { get; set; }

            public static int AddOne(int num)
            {
                throw new Exception();
            }

            public static int AddTwo(int num)
            {
                throw new Exception();
            }

            public static int AddThree(int num)
            {
                throw new Exception();
            }

            public MyContext9214(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.HasDefaultSchema("foo");

                modelBuilder.Entity<Widget9214>().ToTable("Widgets", "foo");

                modelBuilder.HasDbFunction(typeof(MyContext9214).GetMethod(nameof(AddOne)));
                modelBuilder.HasDbFunction(typeof(MyContext9214).GetMethod(nameof(AddTwo))).HasSchema("dbo");
            }
        }

        public class Widget9214
        {
            public int Id { get; set; }
            public int Val { get; set; }
        }

        #endregion

        #region Bug9277

        [Fact]
        public virtual void From_sql_gets_value_of_out_parameter_in_stored_procedure()
        {
            using (CreateDatabase9277())
            {
                using (var context = new MyContext9277(_options))
                {
                    var valueParam = new SqlParameter
                    {
                        ParameterName = "Value",
                        Value = 0,
                        Direction = ParameterDirection.Output,
                        SqlDbType = SqlDbType.Int
                    };

                    Assert.Equal(0, valueParam.Value);

                    var blogs = context.Blogs.FromSql(
                            @"[dbo].[GetPersonAndVoteCount]  @id, @Value out",
                            new SqlParameter { ParameterName = "id", Value = 1 },
                            valueParam)
                        .ToList();

                    Assert.Equal(1, blogs.Count);
                    Assert.Equal(1, valueParam.Value);
                }
            }
        }

        private SqlServerTestStore CreateDatabase9277()
        {
            return CreateTestStore(
                () => new MyContext9277(_options),
                context =>
                    {
                        context.Database.ExecuteSqlCommand(
                            @"CREATE PROCEDURE [dbo].[GetPersonAndVoteCount]
 (
    @id int,
    @Value int OUTPUT
)
AS
BEGIN
    SELECT @Value = SomeValue
    FROM dbo.Blogs
    WHERE Id = @id;
    SELECT *
    FROM dbo.Blogs
    WHERE Id = @id;
    END");

                        context.AddRange(
                            new Blog9277 { SomeValue = 1 },
                            new Blog9277 { SomeValue = 2 },
                            new Blog9277 { SomeValue = 3 }
                        );

                        context.SaveChanges();

                        ClearLog();
                    });
        }

        public class MyContext9277 : DbContext
        {
            public MyContext9277(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Blog9277> Blogs { get; set; }
        }

        public class Blog9277
        {
            public int Id { get; set; }
            public int SomeValue { get; set; }
        }

        #endregion

        #region Bug9038

        [Fact]
        public virtual async Task Include_collection_optional_reference_collection_9038()
        {
            using (CreateDatabase9038())
            {
                using (var context = new MyContext9038(_options))
                {
                    var result = await context.People.OfType<PersonTeacher9038>()
                        .Include(m => m.Students)
                        .ThenInclude(m => m.Family)
                        .ThenInclude(m => m.Members)
                        .ToListAsync();

                    Assert.Equal(2, result.Count);
                    Assert.Equal(true, result.All(r => r.Students.Any()));
                }
            }
        }

        [Fact]
        public async Task Include_optional_reference_collection_another_collection()
        {
            using (CreateDatabase9038())
            {
                using (var context = new MyContext9038(_options))
                {
                    var result = await context.Set<PersonTeacher9038>()
                        .Include(m => m.Family.Members)
                        .Include(m => m.Students)
                        .ToListAsync();

                    Assert.Equal(2, result.Count);
                    Assert.True(result.All(r => r.Students.Any()));
                    Assert.Null(result.Single(t => t.Name == "Ms. Frizzle").Family);
                    Assert.NotNull(result.Single(t => t.Name == "Mr. Garrison").Family);
                }
            }
        }

        public abstract class Person9038
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public int? TeacherId { get; set; }

            public PersonFamily9038 Family { get; set; }
        }

        public class PersonKid9038 : Person9038
        {
            public int Grade { get; set; }

            public PersonTeacher9038 Teacher { get; set; }
        }

        public class PersonTeacher9038 : Person9038
        {
            public ICollection<PersonKid9038> Students { get; set; }
        }

        public class PersonFamily9038
        {
            public int Id { get; set; }

            public string LastName { get; set; }

            public ICollection<Person9038> Members { get; set; }
        }

        public class MyContext9038 : DbContext
        {
            public MyContext9038(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Person9038> People { get; set; }

            public DbSet<PersonFamily9038> Families { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<PersonTeacher9038>().HasBaseType<Person9038>();
                modelBuilder.Entity<PersonKid9038>().HasBaseType<Person9038>();
                modelBuilder.Entity<PersonFamily9038>();

                modelBuilder.Entity<PersonKid9038>(
                    entity =>
                        {
                            entity.Property("Discriminator")
                                .HasMaxLength(63);
                            entity.HasIndex("Discriminator");

                            entity.HasOne(m => m.Teacher)
                                .WithMany(m => m.Students)
                                .HasForeignKey(m => m.TeacherId)
                                .HasPrincipalKey(m => m.Id)
                                .OnDelete(DeleteBehavior.Restrict);
                        });
            }
        }

        private SqlServerTestStore CreateDatabase9038()
        {
            return CreateTestStore(
                () => new MyContext9038(_options),
                context =>
                    {
                        var famalies = new List<PersonFamily9038>
                        {
                            new PersonFamily9038
                            {
                                LastName = "Garrison"
                            },
                            new PersonFamily9038
                            {
                                LastName = "Cartman"
                            }
                        };
                        var teachers = new List<PersonTeacher9038>
                        {
                            new PersonTeacher9038 { Name = "Ms. Frizzle" },
                            new PersonTeacher9038 { Name = "Mr. Garrison", Family = famalies[0] }
                        };
                        var students = new List<PersonKid9038>
                        {
                            new PersonKid9038 { Name = "Arnold", Grade = 2, Teacher = teachers[0] },
                            new PersonKid9038 { Name = "Eric", Grade = 4, Teacher = teachers[1], Family = famalies[1] }
                        };

                        context.People.AddRange(teachers);
                        context.People.AddRange(students);
                        context.SaveChanges();

                        ClearLog();
                    });
        }

        #endregion

        #region Bug9735

        [Fact]
        // TODO: Convert to test in IncludeTestBase once issue #9742 is fixed
        public virtual void Repro9735()
        {
            using (CreateDatabase9735())
            {
                using (var context = new MyContext9735(_options))
                {
                    var result = context.Customers
                        .Include(b => b.Orders)
                        .OrderBy(b => b.Address.Id > 0)
                        .ThenBy(b => b.CustomerDetails != null ? b.CustomerDetails.Name : string.Empty)
                        .Take(2)
                        .ToList();

                    Assert.Equal(1, result.Count);

                    AssertSql(
                        @"@__p_0='2'

SELECT TOP(@__p_0) [b].[Id], [b].[AddressId], [b].[CustomerDetailsId], [b].[Name]
FROM [Customers] AS [b]
LEFT JOIN [CustomerDetails9735] AS [b.CustomerDetails] ON [b].[CustomerDetailsId] = [b.CustomerDetails].[Id]
ORDER BY CASE
    WHEN [b].[AddressId] > 0
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END, CASE
    WHEN [b].[CustomerDetailsId] IS NOT NULL
    THEN [b.CustomerDetails].[Name] ELSE N''
END, [b].[Id]",
                        //
                        @"@__p_0='2'

SELECT [b.Orders].[Id], [b.Orders].[CustomerId], [b.Orders].[Name]
FROM [Order9735] AS [b.Orders]
INNER JOIN (
    SELECT DISTINCT [t].*
    FROM (
        SELECT TOP(@__p_0) [b0].[Id], CASE
            WHEN [b0].[AddressId] > 0
            THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
        END AS [c], CASE
            WHEN [b0].[CustomerDetailsId] IS NOT NULL
            THEN [b.CustomerDetails0].[Name] ELSE N''
        END AS [c0], [b0].[AddressId], [b0].[CustomerDetailsId], [b.CustomerDetails0].[Name]
        FROM [Customers] AS [b0]
        LEFT JOIN [CustomerDetails9735] AS [b.CustomerDetails0] ON [b0].[CustomerDetailsId] = [b.CustomerDetails0].[Id]
        ORDER BY [c], [c0], [b0].[Id]
    ) AS [t]
) AS [t0] ON [b.Orders].[CustomerId] = [t0].[Id]
ORDER BY [t0].[c], [t0].[c0], [t0].[Id]");
                }
            }
        }

        public class Address9735
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Customer9735
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int AddressId { get; set; }
            public virtual Address9735 Address { get; set; }
            public virtual List<Order9735> Orders { get; set; }
            public virtual CustomerDetails9735 CustomerDetails { get; set; }
        }

        public class Order9735
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int CustomerId { get; set; }
            public virtual Customer9735 Customer { get; set; }
        }

        public class CustomerDetails9735
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class MyContext9735 : DbContext
        {
            public MyContext9735(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Customer9735> Customers { get; set; }
        }

        private SqlServerTestStore CreateDatabase9735()
        {
            return CreateTestStore(
                () => new MyContext9735(_options),
                context =>
                    {
                        context.AddRange(
                            new Address9735 { Name = "An A" },
                            new Customer9735 { Name = "A B", AddressId = 1 }
                        );
                        context.SaveChanges();

                        ClearLog();
                    });
        }

        #endregion

        #region Bug9791

        [Fact]
        public void Exception_when_null_and_filters_disabled()
        {
            using (CreateDatabase9791())
            {
                using (var context = new MyContext9791(_options))
                {
                    Assert.Throws<InvalidOperationException>(() => context.Blogs
                        .IgnoreQueryFilters()
                        .Where(e => !e.IsDeleted && context.TenantIds.Contains(e.TenantId)).ToList());
                }
            }
        }

        [Fact]
        public virtual void Context_bound_variable_works_correctly()
        {
            using (CreateDatabase9791())
            {
                using (var context = new MyContext9791(_options))
                {
                    // This throws because the default value of TenantIds is null which is NRE
                    Assert.Throws<InvalidOperationException>(() => context.Blogs.ToList());
                }

                using (var context = new MyContext9791(_options))
                {
                    context.TenantIds = new List<int>();
                    var query = context.Blogs.ToList();

                    Assert.Empty(query);
                }

                using (var context = new MyContext9791(_options))
                {
                    context.TenantIds = new List<int> { 1 };
                    var query = context.Blogs.ToList();

                    Assert.Single(query);
                }

                using (var context = new MyContext9791(_options))
                {
                    context.TenantIds = new List<int> { 1, 2 };
                    var query = context.Blogs.ToList();

                    Assert.Equal(2, query.Count);
                }

                AssertSql(
                    @"SELECT [e].[Id], [e].[IsDeleted], [e].[TenantId]
FROM [Blogs] AS [e]
WHERE 0 = 1",
                    //
                    @"SELECT [e].[Id], [e].[IsDeleted], [e].[TenantId]
FROM [Blogs] AS [e]
WHERE ([e].[IsDeleted] = 0) AND [e].[TenantId] IN (1)",
                    //
                    @"SELECT [e].[Id], [e].[IsDeleted], [e].[TenantId]
FROM [Blogs] AS [e]
WHERE ([e].[IsDeleted] = 0) AND [e].[TenantId] IN (1, 2)");
            }
        }

        private SqlServerTestStore CreateDatabase9791()
        {
            return CreateTestStore(
                () => new MyContext9791(_options),
                context =>
                    {
                        context.AddRange(
                            new Blog9791 { IsDeleted = false, TenantId = 1 },
                            new Blog9791 { IsDeleted = false, TenantId = 2 },
                            new Blog9791 { IsDeleted = false, TenantId = 3 },
                            new Blog9791 { IsDeleted = true, TenantId = 1 },
                            new Blog9791 { IsDeleted = true, TenantId = 2 },
                            new Blog9791 { IsDeleted = true, TenantId = 3 }
                        );
                        context.SaveChanges();

                        ClearLog();
                    });
        }

        public class MyContext9791 : DbContext
        {
            public MyContext9791(DbContextOptions options)
                : base(options)
            {
            }

            public List<int> TenantIds
            {
                get;
                set;
            }

            public DbSet<Blog9791> Blogs { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Blog9791>()
                    .HasQueryFilter(e => !e.IsDeleted && TenantIds.Contains(e.TenantId));
            }
        }

        public class Blog9791
        {
            public int Id { get; set; }
            public bool IsDeleted { get; set; }
            public int TenantId { get; set; }
        }

        #endregion

        #region Bug9825

        [Fact]
        public virtual void Context_bound_variable_works_correctly_in_short_circuit_optimization_9825()
        {
            using (CreateDatabase9825())
            {
                using (var context = new MyContext9825(_options))
                {
                    context.IsModerated = true;
                    var query = context.Users.ToList();

                    Assert.Single(query);
                }

                using (var context = new MyContext9825(_options))
                {
                    context.IsModerated = false;
                    var query = context.Users.ToList();

                    Assert.Single(query);
                }

                using (var context = new MyContext9825(_options))
                {
                    context.IsModerated = null;
                    var query = context.Users.ToList();

                    Assert.Equal(2, query.Count);
                }

                AssertSql(
                    @"@__ef_filter__IsModerated_0='True' (Nullable = true)
@__ef_filter__IsModerated_1='True' (Nullable = true)

SELECT [x].[Id], [x].[IsDeleted], [x].[IsModerated]
FROM [Users] AS [x]
WHERE ([x].[IsDeleted] = 0) AND (@__ef_filter__IsModerated_0 IS NULL OR (@__ef_filter__IsModerated_1 = [x].[IsModerated]))",
                    //
                    @"@__ef_filter__IsModerated_0='False' (Nullable = true)
@__ef_filter__IsModerated_1='False' (Nullable = true)

SELECT [x].[Id], [x].[IsDeleted], [x].[IsModerated]
FROM [Users] AS [x]
WHERE ([x].[IsDeleted] = 0) AND (@__ef_filter__IsModerated_0 IS NULL OR (@__ef_filter__IsModerated_1 = [x].[IsModerated]))",
                    //
                    @"@__ef_filter__IsModerated_0='' (DbType = String)

SELECT [x].[Id], [x].[IsDeleted], [x].[IsModerated]
FROM [Users] AS [x]
WHERE ([x].[IsDeleted] = 0) AND (@__ef_filter__IsModerated_0 IS NULL OR [x].[IsModerated] IS NULL)");
            }
        }

        [Fact]
        public virtual void Context_bound_variable_with_member_chain_works_correctly_9825()
        {
            using (CreateDatabase9825())
            {
                using (var context = new MyContext9825(_options))
                {
                    context.IndirectionFlag = new Indirection { Enabled = true };
                    var query = context.Chains.ToList();

                    Assert.Equal(2, query.Count);
                }

                using (var context = new MyContext9825(_options))
                {
                    context.IndirectionFlag = new Indirection { Enabled = false };
                    var query = context.Chains.ToList();

                    Assert.Equal(2, query.Count);
                }

                using (var context = new MyContext9825(_options))
                {
                    context.IndirectionFlag = null;
                    var exception = Assert.Throws<NullReferenceException>(() => context.Chains.ToList());
                    Assert.Equal("Object reference not set to an instance of an object.", exception.Message);
                    Assert.StartsWith(
                        @"   at lambda_method(Closure , QueryContext )", exception.StackTrace);
                }

                AssertSql(
                    @"@__ef_filter__Enabled_0='True'

SELECT [x].[Id], [x].[IsDeleted], [x].[IsModerated]
FROM [Chains] AS [x]
WHERE @__ef_filter__Enabled_0 = [x].[IsDeleted]",
                    //
                    @"@__ef_filter__Enabled_0='False'

SELECT [x].[Id], [x].[IsDeleted], [x].[IsModerated]
FROM [Chains] AS [x]
WHERE @__ef_filter__Enabled_0 = [x].[IsDeleted]");
            }
        }

        [Fact]
        public virtual void Local_variable_in_query_filter_throws_if_cannot_be_evaluated_9825()
        {
            using (CreateDatabase9825())
            {
                using (var context = new MyContext9825(_options))
                {
                    context.IsModerated = true;
                    var exception = Assert.Throws<InvalidOperationException>(() => context.Locals.ToList());
                    Assert.Equal(CoreStrings.ExpressionParameterizationExceptionSensitive("value(Microsoft.EntityFrameworkCore.Query.QueryBugsTest+MyContext9825+<>c__DisplayClass33_0).local.Enabled"), exception.Message);
                }
            }
        }

        [Fact]
        public virtual void Local_variable_does_not_clash_with_filter_parameter()
        {
            using (CreateDatabase9825())
            {
                using (var context = new MyContext9825(_options))
                {
                    // ReSharper disable once ConvertToConstant.Local
                    var IsModerated = false;
                    var query = context.Users.Where(e => e.IsModerated == IsModerated).ToList();

                    Assert.Single(query);

                    AssertSql(
                        @"@__ef_filter__IsModerated_0='' (DbType = String)
@__IsModerated_0='False'

SELECT [x].[Id], [x].[IsDeleted], [x].[IsModerated]
FROM [Users] AS [x]
WHERE (([x].[IsDeleted] = 0) AND (@__ef_filter__IsModerated_0 IS NULL OR [x].[IsModerated] IS NULL)) AND ([x].[IsModerated] = @__IsModerated_0)");
                }
            }
        }

        [Fact]
        public virtual void Complex_filter_gets_prefixed_name()
        {
            using (CreateDatabase9825())
            {
                using (var context = new MyContext9825(_options))
                {
                    context.BasePrice = 1;
                    context.CustomPrice = 2;
                    var query = context.Complexes.ToList();

                    Assert.Single(query);

                    AssertSql(
                        @"@__ef_filter__BasePrice_0='1'
@__ef_filter__CustomPrice_1='2'

SELECT [x].[Id], [x].[IsEnabled]
FROM [Complexes] AS [x]
WHERE ([x].[IsEnabled] = 1) AND ((@__ef_filter__BasePrice_0 + @__ef_filter__CustomPrice_1) > 0)");
                }
            }
        }

        private SqlServerTestStore CreateDatabase9825()
        {
            return CreateTestStore(
                () => new MyContext9825(_options),
                context =>
                    {
                        context.AddRange(
                            new EntityWithContextBoundComplexExpression9825 { IsDeleted = false, IsModerated = false },
                            new EntityWithContextBoundComplexExpression9825 { IsDeleted = true, IsModerated = false },
                            new EntityWithContextBoundComplexExpression9825 { IsDeleted = false, IsModerated = true },
                            new EntityWithContextBoundComplexExpression9825 { IsDeleted = true, IsModerated = true },
                            new EntityWithContextBoundMemberChain9825 { IsDeleted = false, IsModerated = false },
                            new EntityWithContextBoundMemberChain9825 { IsDeleted = true, IsModerated = false },
                            new EntityWithContextBoundMemberChain9825 { IsDeleted = false, IsModerated = true },
                            new EntityWithContextBoundMemberChain9825 { IsDeleted = true, IsModerated = true },
                            new EntityWithLocalVariableAccessInFilter9825 { IsDeleted = false, IsModerated = false },
                            new EntityWithLocalVariableAccessInFilter9825 { IsDeleted = true, IsModerated = false },
                            new EntityWithLocalVariableAccessInFilter9825 { IsDeleted = false, IsModerated = true },
                            new EntityWithLocalVariableAccessInFilter9825 { IsDeleted = true, IsModerated = true },
                            new EntityWithComplexContextBoundExpression9825 { IsEnabled = true },
                            new EntityWithComplexContextBoundExpression9825 { IsEnabled = false }
                        );

                        context.SaveChanges();

                        ClearLog();
                    });
        }

        public class MyContext9825 : DbContext
        {
            public MyContext9825(DbContextOptions options)
                : base(options)
            {
            }

            public bool? IsModerated { get; set; }
            public int BasePrice { get; set; }
            public int CustomPrice { get; set; }
            public Indirection IndirectionFlag { get; set; }

            public DbSet<EntityWithContextBoundComplexExpression9825> Users { get; set; }
            public DbSet<EntityWithContextBoundMemberChain9825> Chains { get; set; }
            public DbSet<EntityWithLocalVariableAccessInFilter9825> Locals { get; set; }
            public DbSet<EntityWithComplexContextBoundExpression9825> Complexes { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<EntityWithContextBoundComplexExpression9825>()
                    .HasQueryFilter(x => !x.IsDeleted && (IsModerated == null || IsModerated == x.IsModerated));

                modelBuilder.Entity<EntityWithContextBoundMemberChain9825>()
                    .HasQueryFilter(x => IndirectionFlag.Enabled == x.IsDeleted);

                var local = new Indirection();
                local = null;
                modelBuilder.Entity<EntityWithLocalVariableAccessInFilter9825>()
                    .HasQueryFilter(x => local.Enabled == x.IsDeleted);

                modelBuilder.Entity<EntityWithComplexContextBoundExpression9825>()
                    .HasQueryFilter(x => x.IsEnabled && (BasePrice + CustomPrice > 0));
            }
        }

        public class Indirection
        {
            public bool Enabled { get; set; }
        }

        public class EntityWithContextBoundComplexExpression9825
        {
            public int Id { get; set; }
            public bool IsDeleted { get; set; }
            public bool IsModerated { get; set; }
        }

        public class EntityWithContextBoundMemberChain9825
        {
            public int Id { get; set; }
            public bool IsDeleted { get; set; }
            public bool IsModerated { get; set; }
        }

        public class EntityWithLocalVariableAccessInFilter9825
        {
            public int Id { get; set; }
            public bool IsDeleted { get; set; }
            public bool IsModerated { get; set; }
        }

        public class EntityWithComplexContextBoundExpression9825
        {
            public int Id { get; set; }
            public bool IsEnabled { get; set; }
        }

        #endregion

        #region Bug9892

        [Fact]
        public virtual void GroupJoin_to_parent_with_no_child_works_9892()
        {
            using (CreateDatabase9892())
            {
                using (var context = new MyContext9892(_options))
                {
                    var results = (
                        from p in context.Parents
                        join c in (
                                from x in context.Children
                                select new
                                {
                                    x.ParentId,
                                    OtherParent = x.OtherParent.Name
                                })
                            on p.Id equals c.ParentId into child
                        select new
                        {
                            ParentId = p.Id,
                            ParentName = p.Name,
                            Children = child.Select(c => c.OtherParent)
                        }).ToList();

                    Assert.Equal(3, results.Count);
                    Assert.Single(results.Where(t => !t.Children.Any()));
                }
            }
        }

        private SqlServerTestStore CreateDatabase9892()
        {
            return CreateTestStore(
                () => new MyContext9892(_options),
                context =>
                    {
                        context.Parents.Add(new Parent9892 { Name = "Parent1" });
                        context.Parents.Add(new Parent9892 { Name = "Parent2" });
                        context.Parents.Add(new Parent9892 { Name = "Parent3" });

                        context.OtherParents.Add(new OtherParent9892 { Name = "OtherParent1" });
                        context.OtherParents.Add(new OtherParent9892 { Name = "OtherParent2" });

                        context.SaveChanges();

                        context.Children.Add(new Child9892 { ParentId = 1, OtherParentId = 1 });
                        context.Children.Add(new Child9892 { ParentId = 1, OtherParentId = 2 });
                        context.Children.Add(new Child9892 { ParentId = 2, OtherParentId = 1 });
                        context.Children.Add(new Child9892 { ParentId = 2, OtherParentId = 2 });

                        context.SaveChanges();

                        ClearLog();
                    });
        }

        public class MyContext9892 : DbContext
        {
            public MyContext9892(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Parent9892> Parents { get; set; }
            public DbSet<Child9892> Children { get; set; }
            public DbSet<OtherParent9892> OtherParents { get; set; }
        }

        public class Parent9892
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<Child9892> Children { get; set; }
        }

        public class OtherParent9892
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Child9892
        {
            public int Id { get; set; }
            public int ParentId { get; set; }
            public Parent9892 Parent { get; set; }
            public int OtherParentId { get; set; }
            public OtherParent9892 OtherParent { get; set; }
        }

        #endregion

        #region Bug9468

        [Fact]
        public virtual void Conditional_expression_with_conditions_does_not_collapse_if_nullable_bool()
        {
            using (CreateDatabase9468())
            {
                using (var context = new MyContext9468(_options))
                {
                    var query = context.Carts.Select(
                        t => new
                        {
                            Processing = t.Configuration != null ? !t.Configuration.Processed : (bool?)null
                        }).ToList();

                    Assert.Single(query.Where(t => t.Processing == null));
                    Assert.Single(query.Where(t => t.Processing == true));
                    Assert.Single(query.Where(t => t.Processing == false));

                    AssertSql(
                        @"SELECT CASE
    WHEN [t].[ConfigurationId] IS NOT NULL
    THEN CASE
        WHEN [t.Configuration].[Processed] = 0
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END ELSE NULL
END AS [Processing]
FROM [Carts] AS [t]
LEFT JOIN [Configuration9468] AS [t.Configuration] ON [t].[ConfigurationId] = [t.Configuration].[Id]");
                }
            }
        }

        private SqlServerTestStore CreateDatabase9468()
        {
            return CreateTestStore(
                () => new MyContext9468(_options),
                context =>
                    {
                        context.AddRange(
                            new Cart9468(),
                            new Cart9468
                            {
                                Configuration = new Configuration9468 { Processed = true }
                            },
                            new Cart9468
                            {
                                Configuration = new Configuration9468()
                            }
                        );

                        context.SaveChanges();

                        ClearLog();
                    });
        }

        public class MyContext9468 : DbContext
        {
            public MyContext9468(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Cart9468> Carts { get; set; }
        }

        public class Cart9468
        {
            public int Id { get; set; }
            public int? ConfigurationId { get; set; }
            public Configuration9468 Configuration { get; set; }
        }

        public class Configuration9468
        {
            public int Id { get; set; }
            public bool Processed { get; set; }
        }

        #endregion

        #region Bug10271

        [Fact]
        public virtual void Static_member_from_non_dbContext_class_is_inlined_in_queryFilter()
        {
            using (CreateDatabase10271())
            {
                using (var context = new MyContext10271(_options))
                {
                    var query = context.Blogs.ToList();

                    var blog = Assert.Single(query);

                    AssertSql(
                        @"SELECT [b].[Id], [b].[Processed]
FROM [Blogs] AS [b]
WHERE [b].[Processed] = 1");
                }
            }
        }

        [Fact]
        public virtual void Local_variable_from_OnModelCreating_is_inlined_in_queryFilter()
        {
            using (CreateDatabase10271())
            {
                using (var context = new MyContext10271(_options))
                {
                    var query = context.Posts.ToList();

                    var blog = Assert.Single(query);

                    AssertSql(
                        @"SELECT [p].[Id], [p].[TenantId]
FROM [Posts] AS [p]
WHERE [p].[TenantId] = 1");
                }
            }
        }

        [Fact]
        public virtual void Context_variable_captured_in_multi_level_expression_tree_is_parametrized()
        {
            using (CreateDatabase10271())
            {
                using (var context = new MyContext10271(_options))
                {
                    Assert.Empty(context.Comments.ToList());

                    context.Value = 1;
                    var query = context.Comments.ToList();

                    var blog = Assert.Single(query);

                    AssertSql(
                        @"SELECT [c].[Id], [c].[Include]
FROM [Comments] AS [c]",
                        @"SELECT [c].[Id], [c].[Include]
FROM [Comments] AS [c]");
                }
            }
        }

        private SqlServerTestStore CreateDatabase10271()
        {
            return CreateTestStore(
                () => new MyContext10271(_options),
                context =>
                {
                    context.AddRange(
                        new Blog10271 { Processed = true },
                        new Blog10271 { Processed = false },
                        new Post10271 { TenantId = 1 },
                        new Post10271 { TenantId = 2 },
                        new Comment10271 { Include = true },
                        new Comment10271 { Include = false }
                    );

                    context.SaveChanges();

                    ClearLog();
                });
        }

        public class MyContext10271 : DbContext
        {
            public MyContext10271(DbContextOptions options)
                : base(options)
            {
            }

            public int Value { get; set; }

            public DbSet<Blog10271> Blogs { get; set; }
            public DbSet<Post10271> Posts { get; set; }
            public DbSet<Comment10271> Comments { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog10271>()
                    .HasQueryFilter(b => b.Processed == Blog10271.Enabled);

                var tenantId = 1;
                modelBuilder.Entity<Post10271>()
                    .HasQueryFilter(p => p.TenantId == tenantId);

                Expression<Func<int, bool>> predicate = c => c == Value;
                Expression<Func<Comment10271, bool>> filter
                    = c => c.Id == new List<int> { 1, 2, 3 }.AsQueryable().Where(predicate).FirstOrDefault();

                modelBuilder.Entity<Comment10271>()
                    .HasQueryFilter(filter);
            }
        }

        public class Blog10271
        {
            public static bool Enabled = true;

            public int Id { get; set; }
            public bool Processed { get; set; }
        }

        public class Post10271
        {
            public int Id { get; set; }
            public int TenantId { get; set; }
        }

        public class Comment10271
        {
            public int Id { get; set; }
            public bool Include { get; set; }
        }

        #endregion

        #region Bug10463

        [Fact]
        public virtual void Filter_referencing_set()
        {
            using (CreateDatabase10463())
            {
                using (var context = new MyContext10463(_options))
                {
                    var query = context.Blogs.ToList();
                }
            }
        }

        [Fact]
        public virtual void Filter_referencing_set_with_closure()
        {
            using (CreateDatabase10463())
            {
                using (var context = new MyContext10463(_options))
                {
                    var query = context.Posts.ToList();
                }
            }
        }

        private SqlServerTestStore CreateDatabase10463()
        {
            return CreateTestStore(
                () => new MyContext10463(_options),
                context =>
                {
                    context.SaveChanges();

                    ClearLog();
                });
        }

        public class MyContext10463 : DbContext
        {
            public MyContext10463(DbContextOptions options)
                : base(options)
            {
            }

            public int Value { get; set; }

            public DbSet<Blog10463> Blogs { get; set; }
            public DbSet<Post10463> Posts { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog10463>()
                    .HasQueryFilter(b => Posts.Any(p => p.BlogId == b.Id));

                SetPostsFilter(modelBuilder, this);
            }

            private static void SetPostsFilter(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.Entity<Post10463>()
                    .HasQueryFilter(p => context.Set<Blog10463>().Any(b => b.Id == p.BlogId));
            }
        }

        public class Blog10463
        {
            public int Id { get; set; }
            public ICollection<Post10463> Posts { get; set; }
        }

        public class Post10463
        {
            public int Id { get; set; }
            public int BlogId { get; set; }
        }

        #endregion

        private DbContextOptions _options;

        private SqlServerTestStore CreateTestStore<TContext>(
            Func<TContext> contextCreator,
            Action<TContext> contextInitializer)
            where TContext : DbContext, IDisposable
        {
            var testStore = SqlServerTestStore.CreateInitialized("QueryBugsTest", multipleActiveResultSets: true);

            _options = Fixture.CreateOptions(testStore);

            using (var context = contextCreator())
            {
                context.Database.EnsureCreated();
                contextInitializer?.Invoke(context);
            }
            return testStore;
        }

        protected void ClearLog()
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        private void AssertSql(params string[] expected)
        {
            Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
        }
    }
}
