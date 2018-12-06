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
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable CA1034 // Nested types should not be visible

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
                    context.Database.EnsureCreatedResiliently();
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
                        new Person
                        {
                            Name = "John Doe"
                        },
                        new Person
                        {
                            Name = "Joe Bloggs"
                        });

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
                        CoreStrings.ErrorMaterializingPropertyNullReference("ZeroKey", "Id", typeof(int)),
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
                    context.Products.Add(
                        new Product
                        {
                            Name = "Product 1"
                        });
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
                    context.Products.Add(
                        new Product
                        {
                            Name = "Product 1"
                        });
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
                    var order11 = new Order
                    {
                        Name = "Order11"
                    };
                    var order12 = new Order
                    {
                        Name = "Order12"
                    };
                    var order21 = new Order
                    {
                        Name = "Order21"
                    };
                    var order22 = new Order
                    {
                        Name = "Order22"
                    };
                    var order23 = new Order
                    {
                        Name = "Order23"
                    };

                    var customer1 = new Customer
                    {
                        FirstName = "Customer",
                        LastName = "One",
                        Orders = new List<Order>
                        {
                            order11,
                            order12
                        }
                    };
                    var customer2 = new Customer
                    {
                        FirstName = "Customer",
                        LastName = "Two",
                        Orders = new List<Order>
                        {
                            order21,
                            order22,
                            order23
                        }
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
                        m.HasKey(
                            c => new
                            {
                                c.FirstName,
                                c.LastName
                            });
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
                        new Project
                        {
                            Name = "Projects 1"
                        },
                        new Project
                        {
                            Name = "Projects 2"
                        },
                        new Project
                        {
                            Name = "Projects 3"
                        }
                    };

                    context.Projects.AddRange(projects);

                    var users = new[]
                    {
                        new User
                        {
                            Name = "Users 1"
                        },
                        new User
                        {
                            Name = "Users 2"
                        },
                        new User
                        {
                            Name = "Users 3"
                        }
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
                    var drogon = new Dragon
                    {
                        Name = "Drogon"
                    };
                    var rhaegal = new Dragon
                    {
                        Name = "Rhaegal"
                    };
                    var viserion = new Dragon
                    {
                        Name = "Viserion"
                    };
                    var balerion = new Dragon
                    {
                        Name = "Balerion"
                    };

                    var aerys = new Targaryen
                    {
                        Name = "Aerys II"
                    };
                    var details = new Details
                    {
                        FullName = @"Daenerys Stormborn of the House Targaryen, the First of Her Name, the Unburnt, Queen of Meereen,
Queen of the Andals and the Rhoynar and the First Men, Khaleesi of the Great Grass Sea, Breaker of Chains, and Mother of Dragons"
                    };

                    var daenerys = new Targaryen
                    {
                        Name = "Daenerys",
                        Details = details,
                        Dragons = new List<Dragon>
                        {
                            drogon,
                            rhaegal,
                            viserion
                        }
                    };
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
            Execute1742(
                new CustomerDetails_1742
                {
                    FirstName = "Foo",
                    LastName = "Bar"
                });
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
                    var o111 = new Order3758
                    {
                        Name = "O111"
                    };
                    var o112 = new Order3758
                    {
                        Name = "O112"
                    };
                    var o121 = new Order3758
                    {
                        Name = "O121"
                    };
                    var o122 = new Order3758
                    {
                        Name = "O122"
                    };
                    var o131 = new Order3758
                    {
                        Name = "O131"
                    };
                    var o132 = new Order3758
                    {
                        Name = "O132"
                    };
                    var o141 = new Order3758
                    {
                        Name = "O141"
                    };

                    var o211 = new Order3758
                    {
                        Name = "O211"
                    };
                    var o212 = new Order3758
                    {
                        Name = "O212"
                    };
                    var o221 = new Order3758
                    {
                        Name = "O221"
                    };
                    var o222 = new Order3758
                    {
                        Name = "O222"
                    };
                    var o231 = new Order3758
                    {
                        Name = "O231"
                    };
                    var o232 = new Order3758
                    {
                        Name = "O232"
                    };
                    var o241 = new Order3758
                    {
                        Name = "O241"
                    };

                    var c1 = new Customer3758
                    {
                        Name = "C1",
                        Orders1 = new List<Order3758>
                        {
                            o111,
                            o112
                        },
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
                        Orders1 = new List<Order3758>
                        {
                            o211,
                            o212
                        },
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

                    parent1.ChildCollection = new List<IChild3409>
                    {
                        child1
                    };
                    child1.SelfReferenceCollection = new List<IChild3409>
                    {
                        child2,
                        child3
                    };

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
                                select new
                                {
                                    One = 1,
                                    Coalesce = eRootJoined ?? eVersion
                                };

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
                                select new
                                {
                                    Root = eRootJoined,
                                    Coalesce = eRootJoined ?? eVersion
                                };

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
                                select new
                                {
                                    One = 1,
                                    Coalesce = eRootJoined ?? (eVersion ?? eRootJoined)
                                };

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
                                select new
                                {
                                    One = eRootJoined,
                                    Two = 2,
                                    Coalesce = eRootJoined ?? (eVersion ?? eRootJoined)
                                };

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
                                select new
                                {
                                    eRootJoined,
                                    eVersion,
                                    foo = eRootJoined ?? eVersion
                                };

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
                    var c11 = new Child3101
                    {
                        Name = "c11"
                    };
                    var c12 = new Child3101
                    {
                        Name = "c12"
                    };
                    var c13 = new Child3101
                    {
                        Name = "c13"
                    };
                    var c21 = new Child3101
                    {
                        Name = "c21"
                    };
                    var c22 = new Child3101
                    {
                        Name = "c22"
                    };
                    var c31 = new Child3101
                    {
                        Name = "c31"
                    };
                    var c32 = new Child3101
                    {
                        Name = "c32"
                    };

                    context.Children.AddRange(c11, c12, c13, c21, c22, c31, c32);

                    var e1 = new Entity3101
                    {
                        Id = 1,
                        Children = new[] { c11, c12, c13 }
                    };
                    var e2 = new Entity3101
                    {
                        Id = 2,
                        Children = new[] { c21, c22 }
                    };
                    var e3 = new Entity3101
                    {
                        Id = 3,
                        Children = new[] { c31, c32 }
                    };

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
                    var query = context.Contacts.OfType<ServiceOperatorContact6986>().Select(
                        c => new
                        {
                            c,
                            Prop = EF.Property<int>(c, "ServiceOperator6986Id")
                        }).ToList();

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
                        new Employer6986
                        {
                            Name = "UWE"
                        },
                        new Employer6986
                        {
                            Name = "Hewlett Packard"
                        });

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
                    context.Add(
                        new Product
                        {
                            Name = "Product1"
                        });
                    context.Add(
                        new SpecialProduct
                        {
                            Name = "SpecialProduct"
                        });
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
                        @"SELECT [e].[Id], [e].[Permission], [e].[PermissionByte], [e].[PermissionShort]
FROM [Entity] AS [e]
WHERE ([e].[Permission] & CAST(17179869184 AS bigint)) = CAST(17179869184 AS bigint)");
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
                        @"SELECT [e].[Id], [e].[Permission], [e].[PermissionByte], [e].[PermissionShort]
FROM [Entity] AS [e]
WHERE ([e].[Permission] & [e].[Permission]) = [e].[Permission]");
                }
            }
        }

        [Fact]
        public virtual void Byte_enum_has_flag_does_not_apply_explicit_cast_for_non_constant()
        {
            using (CreateDatabase8538())
            {
                using (var context = new MyContext8538(_options))
                {
                    var query = context.Entity.Where(e => e.PermissionByte.HasFlag(e.PermissionByte)).ToList();

                    Assert.Equal(3, query.Count);

                    AssertSql(
                        @"SELECT [e].[Id], [e].[Permission], [e].[PermissionByte], [e].[PermissionShort]
FROM [Entity] AS [e]
WHERE ([e].[PermissionByte] & [e].[PermissionByte]) = [e].[PermissionByte]");
                }
            }
        }

        [Fact]
        public virtual void Enum_has_flag_applies_explicit_cast_for_short_constant()
        {
            using (CreateDatabase8538())
            {
                using (var context = new MyContext8538(_options))
                {
                    var query = context.Entity.Where(e => e.PermissionShort.HasFlag(PermissionShort.READ_WRITE)).ToList();

                    Assert.Equal(1, query.Count);

                    AssertSql(
                        @"SELECT [e].[Id], [e].[Permission], [e].[PermissionByte], [e].[PermissionShort]
FROM [Entity] AS [e]
WHERE ([e].[PermissionShort] & CAST(4 AS smallint)) = CAST(4 AS smallint)");
                }
            }
        }

        public class Entity8538
        {
            public int Id { get; set; }
            public Permission Permission { get; set; }
            public PermissionByte PermissionByte { get; set; }
            public PermissionShort PermissionShort { get; set; }
        }

        [Flags]
#pragma warning disable CA2217 // Do not mark enums with FlagsAttribute
        public enum PermissionByte : byte
#pragma warning restore CA2217 // Do not mark enums with FlagsAttribute
        {
            NONE = 1,
            READ_ONLY = 2,
            READ_WRITE = 4
        }

        [Flags]
#pragma warning disable CA2217 // Do not mark enums with FlagsAttribute
        public enum PermissionShort : short
#pragma warning restore CA2217 // Do not mark enums with FlagsAttribute
        {
            NONE = 1,
            READ_ONLY = 2,
            READ_WRITE = 4
        }

        [Flags]
#pragma warning disable CA2217 // Do not mark enums with FlagsAttribute
        public enum Permission : long
#pragma warning restore CA2217 // Do not mark enums with FlagsAttribute
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
                        new Entity8538
                        {
                            Permission = Permission.NONE,
                            PermissionByte = PermissionByte.NONE,
                            PermissionShort = PermissionShort.NONE
                        },
                        new Entity8538
                        {
                            Permission = Permission.READ_ONLY,
                            PermissionByte = PermissionByte.READ_ONLY,
                            PermissionShort = PermissionShort.READ_ONLY
                        },
                        new Entity8538
                        {
                            Permission = Permission.READ_WRITE,
                            PermissionByte = PermissionByte.READ_WRITE,
                            PermissionShort = PermissionShort.READ_WRITE
                        }
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
                context => ClearLog());
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

        #region Bug9202/9210

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
                    Assert.True(result[0].Cast.All(a => a.Details != null));

                    AssertSql(
                        @"SELECT [m].[Id], [m].[Title], [m].[Id], [m].[Details_Info]
FROM [Movies] AS [m]
ORDER BY [m].[Id]",
                        //
                        @"SELECT [m.Cast].[Id], [m.Cast].[Movie9202Id], [m.Cast].[Name], [m.Cast].[Id], [m.Cast].[Details_Info]
FROM [Actors] AS [m.Cast]
INNER JOIN (
    SELECT DISTINCT [m0].[Id]
    FROM [Movies] AS [m0]
) AS [t] ON [m.Cast].[Movie9202Id] = [t].[Id]
ORDER BY [t].[Id]");
                }
            }
        }

        [Fact]
        public void Include_collection_for_entity_with_owned_type_works_string()
        {
            using (CreateDatabase9202())
            {
                using (var context = new MyContext9202(_options))
                {
                    var query = context.Movies.Include("Cast");
                    var result = query.ToList();

                    Assert.Equal(1, result.Count);
                    Assert.Equal(3, result[0].Cast.Count);
                    Assert.NotNull(result[0].Details);
                    Assert.True(result[0].Cast.All(a => a.Details != null));

                    AssertSql(
                        @"SELECT [m].[Id], [m].[Title], [m].[Id], [m].[Details_Info]
FROM [Movies] AS [m]
ORDER BY [m].[Id]",
                        //
                        @"SELECT [m.Cast].[Id], [m.Cast].[Movie9202Id], [m.Cast].[Name], [m.Cast].[Id], [m.Cast].[Details_Info]
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
                    var av = new Actor9202
                    {
                        Name = "Alicia Vikander",
                        Details = new Details9202
                        {
                            Info = "Best actor ever made"
                        }
                    };
                    var oi = new Actor9202
                    {
                        Name = "Oscar Isaac",
                        Details = new Details9202
                        {
                            Info = "Best actor ever made"
                        }
                    };
                    var dg = new Actor9202
                    {
                        Name = "Domhnall Gleeson",
                        Details = new Details9202
                        {
                            Info = "Best actor ever made"
                        }
                    };
                    var em = new Movie9202
                    {
                        Title = "Ex Machina",
                        Cast = new List<Actor9202>
                        {
                            av,
                            oi,
                            dg
                        },
                        Details = new Details9202
                        {
                            Info = "Best movie ever made"
                        }
                    };
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
                modelBuilder.Entity<Actor9202>().OwnsOne(m => m.Details);
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
            public Details9202 Details { get; set; }
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
                        @"SELECT TOP(2) [foo].[AddOne]([w].[Val])
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
                        @"SELECT TOP(2) [dbo].[AddTwo]([w].[Val])
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
                    var w1 = new Widget9214
                    {
                        Val = 1
                    };
                    var w2 = new Widget9214
                    {
                        Val = 2
                    };
                    var w3 = new Widget9214
                    {
                        Val = 3
                    };
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
                            "[dbo].[GetPersonAndVoteCount]  @id, @Value out",
                            new SqlParameter
                            {
                                ParameterName = "id",
                                Value = 1
                            },
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
                        new Blog9277
                        {
                            SomeValue = 1
                        },
                        new Blog9277
                        {
                            SomeValue = 2
                        },
                        new Blog9277
                        {
                            SomeValue = 3
                        }
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
                    Assert.Equal(true, result.All(r => r.Students.Count > 0));
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
                    Assert.True(result.All(r => r.Students.Count > 0));
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
                        new PersonTeacher9038
                        {
                            Name = "Ms. Frizzle"
                        },
                        new PersonTeacher9038
                        {
                            Name = "Mr. Garrison",
                            Family = famalies[0]
                        }
                    };
                    var students = new List<PersonKid9038>
                    {
                        new PersonKid9038
                        {
                            Name = "Arnold",
                            Grade = 2,
                            Teacher = teachers[0]
                        },
                        new PersonKid9038
                        {
                            Name = "Eric",
                            Grade = 4,
                            Teacher = teachers[1],
                            Family = famalies[1]
                        }
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
        END AS [c0]
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
                        new Address9735
                        {
                            Name = "An A"
                        },
                        new Customer9735
                        {
                            Name = "A B",
                            AddressId = 1
                        }
                    );
                    context.SaveChanges();

                    ClearLog();
                });
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
                    context.Parents.Add(
                        new Parent9892
                        {
                            Name = "Parent1"
                        });
                    context.Parents.Add(
                        new Parent9892
                        {
                            Name = "Parent2"
                        });
                    context.Parents.Add(
                        new Parent9892
                        {
                            Name = "Parent3"
                        });

                    context.OtherParents.Add(
                        new OtherParent9892
                        {
                            Name = "OtherParent1"
                        });
                    context.OtherParents.Add(
                        new OtherParent9892
                        {
                            Name = "OtherParent2"
                        });

                    context.SaveChanges();

                    context.Children.Add(
                        new Child9892
                        {
                            ParentId = 1,
                            OtherParentId = 1
                        });
                    context.Children.Add(
                        new Child9892
                        {
                            ParentId = 1,
                            OtherParentId = 2
                        });
                    context.Children.Add(
                        new Child9892
                        {
                            ParentId = 2,
                            OtherParentId = 1
                        });
                    context.Children.Add(
                        new Child9892
                        {
                            ParentId = 2,
                            OtherParentId = 2
                        });

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
                            Configuration = new Configuration9468
                            {
                                Processed = true
                            }
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

        #region Bug10635

        [Fact]
        public void Include_with_order_by_on_interface_key()
        {
            using (CreateDatabase10635())
            {
                using (var context = new MyContext10635(_options))
                {
                    var query1 = context.Parents.Include(p => p.Children).OrderBy(p => ((IEntity10635)p).Id).ToList();

                    AssertSql(
                        @"SELECT [p].[Id], [p].[Name]
FROM [Parents] AS [p]
ORDER BY [p].[Id]",
                        //
                        @"SELECT [p.Children].[Id], [p.Children].[Name], [p.Children].[Parent10635Id], [p.Children].[ParentId]
FROM [Children] AS [p.Children]
INNER JOIN (
    SELECT [p0].[Id]
    FROM [Parents] AS [p0]
) AS [t] ON [p.Children].[Parent10635Id] = [t].[Id]
ORDER BY [t].[Id]");

                    ClearLog();

                    var query2 = context.Parents.Include(p => p.Children).OrderBy(p => EF.Property<int>(p, "Id")).ToList();

                    AssertSql(
                        @"SELECT [p].[Id], [p].[Name]
FROM [Parents] AS [p]
ORDER BY [p].[Id]",
                        //
                        @"SELECT [p.Children].[Id], [p.Children].[Name], [p.Children].[Parent10635Id], [p.Children].[ParentId]
FROM [Children] AS [p.Children]
INNER JOIN (
    SELECT [p0].[Id]
    FROM [Parents] AS [p0]
) AS [t] ON [p.Children].[Parent10635Id] = [t].[Id]
ORDER BY [t].[Id]");
                }
            }
        }

        [Fact]
        public void Correlated_collection_with_order_by_on_interface_key()
        {
            using (CreateDatabase10635())
            {
                using (var context = new MyContext10635(_options))
                {
                    var query1 = context.Parents.OrderBy(p => ((IEntity10635)p).Id).Select(p => p.Children.ToList()).ToList();

                    AssertSql(
                        @"SELECT [p].[Id]
FROM [Parents] AS [p]
ORDER BY [p].[Id]",
                        //
                        @"SELECT [p.Children].[Id], [p.Children].[Name], [p.Children].[Parent10635Id], [p.Children].[ParentId], [t].[Id]
FROM [Children] AS [p.Children]
INNER JOIN (
    SELECT [p0].[Id]
    FROM [Parents] AS [p0]
) AS [t] ON [p.Children].[Parent10635Id] = [t].[Id]
ORDER BY [t].[Id]");

                    ClearLog();

                    var query2 = context.Parents.OrderBy(p => EF.Property<int>(p, "Id")).Select(p => p.Children.ToList()).ToList();

                    AssertSql(
                        @"SELECT [p].[Id]
FROM [Parents] AS [p]
ORDER BY [p].[Id]",
                        //
                        @"SELECT [p.Children].[Id], [p.Children].[Name], [p.Children].[Parent10635Id], [p.Children].[ParentId], [t].[Id]
FROM [Children] AS [p.Children]
INNER JOIN (
    SELECT [p0].[Id]
    FROM [Parents] AS [p0]
) AS [t] ON [p.Children].[Parent10635Id] = [t].[Id]
ORDER BY [t].[Id]");
                }
            }
        }

        private SqlServerTestStore CreateDatabase10635()
        {
            return CreateTestStore(
                () => new MyContext10635(_options),
                context =>
                {
                    var c11 = new Child10635
                    {
                        Name = "Child111"
                    };
                    var c12 = new Child10635
                    {
                        Name = "Child112"
                    };
                    var c13 = new Child10635
                    {
                        Name = "Child113"
                    };
                    var c21 = new Child10635
                    {
                        Name = "Child121"
                    };

                    var p1 = new Parent10635
                    {
                        Name = "Parent1",
                        Children = new[] { c11, c12, c13 }
                    };
                    var p2 = new Parent10635
                    {
                        Name = "Parent2",
                        Children = new[] { c21 }
                    };
                    context.Parents.AddRange(p1, p2);
                    context.Children.AddRange(c11, c12, c13, c21);
                    context.SaveChanges();

                    ClearLog();
                });
        }

        public class MyContext10635 : DbContext
        {
            public MyContext10635(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Parent10635> Parents { get; set; }
            public DbSet<Child10635> Children { get; set; }
        }

        public interface IEntity10635
        {
            int Id { get; set; }
        }

        public class Parent10635 : IEntity10635
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public virtual ICollection<Child10635> Children { get; set; }
        }

        public class Child10635 : IEntity10635
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int ParentId { get; set; }
        }

        #endregion

        #region Bug10168

        [Fact]
        public void Row_number_paging_with_owned_type()
        {
            using (var context = new MyContext10168(Fixture.TestSqlLoggerFactory))
            {
                context.Database.EnsureClean();
                context.Add(
                    new Note
                    {
                        Text = "Foo Bar",
                        User = new User10168
                        {
                            Fullname = "Full1",
                            Email = "abc@def.com"
                        }
                    });

                context.SaveChanges();
                ClearLog();
            }

            using (var context = new MyContext10168(Fixture.TestSqlLoggerFactory))
            {
                var query = context.Note.Where(x => x.Text == "Foo Bar")
                    .Skip(0)
                    .Take(100)
                    .ToList();

                var result = Assert.Single(query);
                Assert.NotNull(result.User);
                Assert.Equal("Full1", result.User.Fullname);

                AssertSql(
                    @"@__p_0='?' (DbType = Int32)
@__p_1='?' (DbType = Int32)

SELECT [t].[Id], [t].[Text], [t].[Id], [t].[User_Email], [t].[User_Fullname]
FROM (
    SELECT [x].[Id], [x].[Text], [x].[User_Email], [x].[User_Fullname], ROW_NUMBER() OVER(ORDER BY @@RowCount) AS [__RowNumber__]
    FROM [Note] AS [x]
    WHERE [x].[Text] = N'Foo Bar'
) AS [t]
WHERE ([t].[__RowNumber__] > @__p_0) AND ([t].[__RowNumber__] <= (@__p_0 + @__p_1))");
            }
        }

        public class MyContext10168 : DbContext
        {
            private readonly ILoggerFactory _loggerFactory;

            public MyContext10168(ILoggerFactory loggerFactory)
            {
                _loggerFactory = loggerFactory;
            }

            public DbSet<Note> Note { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder
                    .UseLoggerFactory(_loggerFactory)
                    .UseSqlServer(
                        SqlServerTestStore.CreateConnectionString("RowNumberPaging_Owned"),
                        b => b.UseRowNumberForPaging());
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Note>().OwnsOne(n => n.User);
            }
        }

        public class Note
        {
            [Key]
            public Guid Id { get; set; }

            public string Text { get; set; }
            public User10168 User { get; set; }
        }

        public class User10168
        {
            public Guid Id { get; set; }
            public string Fullname { get; set; }
            public string Email { get; set; }
        }

        #endregion

        #region Bug10301

        [Fact]
        public virtual void MultiContext_query_filter_test()
        {
            using (CreateDatabase10301())
            {
                using (var context = new FilterContext10301(_options))
                {
                    Assert.Empty(context.Blogs.ToList());

                    context.Tenant = 1;
                    Assert.Single(context.Blogs.ToList());

                    context.Tenant = 2;
                    Assert.Equal(2, context.Blogs.Count());

                    AssertSql(
                        @"@__ef_filter__Tenant_0='0'

SELECT [e].[Id], [e].[SomeValue]
FROM [Blogs] AS [e]
WHERE [e].[SomeValue] = @__ef_filter__Tenant_0",
                        //
                        @"@__ef_filter__Tenant_0='1'

SELECT [e].[Id], [e].[SomeValue]
FROM [Blogs] AS [e]
WHERE [e].[SomeValue] = @__ef_filter__Tenant_0",
                        //
                        @"@__ef_filter__Tenant_0='2'

SELECT COUNT(*)
FROM [Blogs] AS [e]
WHERE [e].[SomeValue] = @__ef_filter__Tenant_0");
                }
            }
        }

        private SqlServerTestStore CreateDatabase10301()
        {
            return CreateTestStore(
                () => new FilterContext10301(_options),
                context =>
                {
                    context.AddRange(
                        new Blog10301
                        {
                            SomeValue = 1
                        },
                        new Blog10301
                        {
                            SomeValue = 2
                        },
                        new Blog10301
                        {
                            SomeValue = 2
                        }
                    );

                    context.SaveChanges();

                    ClearLog();
                });
        }

        public class FilterContextBase10301 : DbContext
        {
            public int Tenant { get; set; }

            public FilterContextBase10301(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Blog10301> Blogs { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog10301>().HasQueryFilter(e => e.SomeValue == Tenant);
            }
        }

        public class Blog10301
        {
            public int Id { get; set; }
            public int SomeValue { get; set; }
        }

        public class FilterContext10301 : FilterContextBase10301
        {
            public FilterContext10301(DbContextOptions options)
                : base(options)
            {
            }
        }

        #endregion

        #region Bug11104

        [Fact]
        public virtual void QueryBuffer_requirement_is_computed_when_querying_base_type_while_derived_type_has_shadow_prop()
        {
            using (CreateDatabase11104())
            {
                using (var context = new MyContext11104(_options))
                {
                    var query = context.Bases.ToList();

                    var derived1 = Assert.Single(query);
                    Assert.Equal(derived1.GetType(), typeof(Derived1));

                    AssertSql(
                        @"SELECT [b].[Id], [b].[IsTwo], [b].[MoreStuffId]
FROM [Bases] AS [b]
WHERE [b].[IsTwo] IN (1, 0)");
                }
            }
        }

        private SqlServerTestStore CreateDatabase11104()
        {
            return CreateTestStore(
                () => new MyContext11104(_options),
                context =>
                {
                    context.AddRange(
                        new Derived1
                        {
                            IsTwo = false
                        }
                    );

                    context.SaveChanges();

                    ClearLog();
                });
        }

        public class MyContext11104 : DbContext
        {
            public DbSet<Base> Bases { get; set; }

            public MyContext11104(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Base>()
                    .HasDiscriminator(x => x.IsTwo)
                    .HasValue<Derived1>(false)
                    .HasValue<Derived2>(true);
            }
        }

        public abstract class Base
        {
            public int Id { get; set; }
            public bool IsTwo { get; set; }
        }

        public class Derived1 : Base
        {
            public Stuff MoreStuff { get; set; }
        }

        public class Derived2 : Base
        {
        }

        public class Stuff
        {
            public int Id { get; set; }
        }

        #endregion

        #region Bug11818_11831

        [Fact]
        public virtual void GroupJoin_Anonymous_projection_GroupBy_Aggregate_join_elimination()
        {
            using (CreateDatabase11818())
            {
                using (var context = new MyContext11818(_options))
                {
                    var query = (from e in context.Set<Entity11818>()
                                 join a in context.Set<AnotherEntity11818>()
                                     on e.Id equals a.Id into grouping
                                 from a in grouping.DefaultIfEmpty()
                                 select new
                                 {
                                     ename = e.Name,
                                     aname = a.Name
                                 })
                        .GroupBy(g => g.aname)
                        .Select(
                            g => new
                            {
                                g.Key,
                                cnt = g.Count() + 5
                            })
                        .ToList();

                    AssertSql(
                        @"SELECT [e].[Name] AS [Key], COUNT(*) + 5 AS [cnt]
FROM [Table] AS [e]
GROUP BY [e].[Name]");
                }
            }
        }

        [Fact]
        public virtual void GroupJoin_Anonymous_projection_GroupBy_Aggregate_join_elimination_2()
        {
            using (CreateDatabase11818())
            {
                using (var context = new MyContext11818(_options))
                {
                    var query = (from e in context.Set<Entity11818>()
                                 join a in context.Set<AnotherEntity11818>()
                                     on e.Id equals a.Id into grouping
                                 from a in grouping.DefaultIfEmpty()
                                 join m in context.Set<MaumarEntity11818>()
                                     on e.Id equals m.Id into grouping2
                                 from m in grouping2.DefaultIfEmpty()
                                 select new
                                 {
                                     aname = a.Name,
                                     mname = m.Name
                                 })
                        .GroupBy(
                            g => new
                            {
                                g.aname,
                                g.mname
                            })
                        .Select(
                            g => new
                            {
                                MyKey = g.Key.aname,
                                cnt = g.Count() + 5
                            })
                        .ToList();

                    AssertSql(
                        @"SELECT [e].[Name] AS [MyKey], COUNT(*) + 5 AS [cnt]
FROM [Table] AS [e]
GROUP BY [e].[Name], [e].[MaumarEntity11818_Name]");
                }
            }
        }

        [Fact(Skip = "Issue #11870")]
        public virtual void GroupJoin_Anonymous_projection_GroupBy_Aggregate_join_elimination_3()
        {
            using (CreateDatabase11818())
            {
                using (var context = new MyContext11818(_options))
                {
                    var query = (from e in context.Set<Entity11818>()
                                 join a in context.Set<AnotherEntity11818>()
                                     on e.Id equals a.Id into grouping
                                 from a in grouping.DefaultIfEmpty()
                                 join m in context.Set<MaumarEntity11818>()
                                     on e.Id equals m.Id into grouping2
                                 from m in grouping2.DefaultIfEmpty()
                                 select new
                                 {
                                     aname = a.Name,
                                     mname = m.Name
                                 })
                        .GroupBy(
                            g => new
                            {
                                g.aname,
                                g.mname
                            }).DefaultIfEmpty()
                        .Select(
                            g => new
                            {
                                MyKey = g.Key.aname,
                                cnt = g.Count() + 5
                            })
                        .ToList();

                    AssertSql(
                        "");
                }
            }
        }

        [Fact(Skip = "Issue #11871")]
        public virtual void GroupJoin_Anonymous_projection_GroupBy_Aggregate_join_elimination_4()
        {
            using (CreateDatabase11818())
            {
                using (var context = new MyContext11818(_options))
                {
                    var query = (from e in context.Set<Entity11818>()
                                 join a in context.Set<AnotherEntity11818>()
                                     on e.Id equals a.Id into grouping
                                 from a in grouping.DefaultIfEmpty()
                                 join m in context.Set<MaumarEntity11818>()
                                     on e.Id equals m.Id into grouping2
                                 from m in grouping2.DefaultIfEmpty()
                                 select new
                                 {
                                     aname = a.Name,
                                     mname = m.Name
                                 })
                        .OrderBy(g => g.aname)
                        .GroupBy(
                            g => new
                            {
                                g.aname,
                                g.mname
                            }).FirstOrDefault()
                        .Select(
                            g => new
                            {
                                MyKey = g.aname,
                                cnt = g.mname
                            })
                        .ToList();

                    AssertSql(
                        "");
                }
            }
        }

        private SqlServerTestStore CreateDatabase11818()
        {
            return CreateTestStore(
                () => new MyContext11818(_options),
                context =>
                {
                    context.SaveChanges();

                    ClearLog();
                });
        }

        public class MyContext11818 : DbContext
        {
            public MyContext11818(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Entity11818>().ToTable("Table");
                modelBuilder.Entity<AnotherEntity11818>().ToTable("Table");
                modelBuilder.Entity<MaumarEntity11818>().ToTable("Table");

                modelBuilder.Entity<Entity11818>()
                    .HasOne<AnotherEntity11818>()
                    .WithOne()
                    .HasForeignKey<AnotherEntity11818>(b => b.Id);

                modelBuilder.Entity<Entity11818>()
                    .HasOne<MaumarEntity11818>()
                    .WithOne()
                    .HasForeignKey<MaumarEntity11818>(b => b.Id);
            }
        }

        public class Entity11818
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class AnotherEntity11818
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class MaumarEntity11818
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion

        #region Bug11803_11791

        [Fact(Skip = "See issue#13587")]
        public virtual void Query_filter_with_db_set_should_not_block_other_filters()
        {
            using (CreateDatabase11803())
            {
                using (var context = new MyContext11803(_options))
                {
                    context.Factions.ToList();

                    AssertSql(
                        @"SELECT [f].[Id], [f].[Name]
FROM [Factions] AS [f]
WHERE EXISTS (
    SELECT 1
    FROM [Leaders] AS [l]
    WHERE [l].[Name] = N'Crach an Craite')");
                }
            }
        }

        [Fact(Skip = "Issue#13361")]
        public virtual void Query_type_used_inside_defining_query()
        {
            using (CreateDatabase11803())
            {
                using (var context = new MyContext11803(_options))
                {
                    context.LeadersQuery.ToList();

                    AssertSql(
                        @"SELECT [t].[Name]
FROM (
    SELECT [l].[Name]
    FROM [Leaders] AS [l]
    WHERE ([l].[Name] LIKE N'Bran' + N'%' AND (LEFT([l].[Name], LEN(N'Bran')) = N'Bran')) AND (([l].[Name] <> N'Foo') OR [l].[Name] IS NULL)
) AS [t]
WHERE ([t].[Name] <> N'Bar') OR [t].[Name] IS NULL");
                }
            }
        }

        private SqlServerTestStore CreateDatabase11803()
        {
            return CreateTestStore(
                () => new MyContext11803(_options),
                context =>
                {
                    var f1 = new Faction
                    {
                        Name = "Skeliege"
                    };
                    var f2 = new Faction
                    {
                        Name = "Monsters"
                    };
                    var f3 = new Faction
                    {
                        Name = "Nilfgaard"
                    };
                    var f4 = new Faction
                    {
                        Name = "Northern Realms"
                    };
                    var f5 = new Faction
                    {
                        Name = "Scioia'tael"
                    };

                    var l11 = new Leader
                    {
                        Faction = f1,
                        Name = "Bran Tuirseach"
                    };
                    var l12 = new Leader
                    {
                        Faction = f1,
                        Name = "Crach an Craite"
                    };
                    var l13 = new Leader
                    {
                        Faction = f1,
                        Name = "Eist Tuirseach"
                    };
                    var l14 = new Leader
                    {
                        Faction = f1,
                        Name = "Harald the Cripple"
                    };

                    context.Factions.AddRange(f1, f2, f3, f4, f5);
                    context.Leaders.AddRange(l11, l12, l13, l14);

                    context.SaveChanges();

                    ClearLog();
                });
        }

        public class MyContext11803 : DbContext
        {
            public DbSet<Faction> Factions { get; set; }
            public DbSet<Leader> Leaders { get; set; }
            public DbQuery<LeaderQuery> LeadersQuery { get; set; }

            public MyContext11803(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Leader>().HasQueryFilter(l => l.Name.StartsWith("Bran")); // this one is ignored
                modelBuilder.Entity<Faction>().HasQueryFilter(f => Leaders.Any(l => l.Name == "Crach an Craite"));

                modelBuilder
                    .Query<FactionQuery>()
                    .ToQuery(
                        () => Set<Leader>()
                            .Where(lq => lq.Name != "Foo")
                            .Select(
                                lq => new FactionQuery
                                {
                                    Name = lq.Name
                                }));

                modelBuilder
                    .Query<LeaderQuery>()
                    .ToQuery(
                        () => Query<FactionQuery>()
                            .Where(fq => fq.Name != "Bar")
                            .Select(
                                fq => new LeaderQuery
                                {
                                    Name = "Not Bar"
                                }));
            }
        }

        public class Faction
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public List<Leader> Leaders { get; set; }
        }

        public class Leader
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Faction Faction { get; set; }
        }

        public class FactionQuery
        {
            public string Name { get; set; }
        }

        public class LeaderQuery
        {
            public string Name { get; set; }
        }

        #endregion

        #region Bug11923

        public static bool ClientMethod11923(int id) => true;

        [Fact]
        public virtual void Collection_without_setter_materialized_correctly()
        {
            using (CreateDatabase11923())
            {
                using (var context = new MyContext11923(_options))
                {
                    var query1 = context.Blogs
                        .Select(
                            b => new
                            {
                                Collection1 = b.Posts1,
                                Collection2 = b.Posts2,
                                Collection3 = b.Posts3
                            }).ToList();

                    var query2 = context.Blogs
                        .Select(
                            b => new
                            {
                                Collection1 = b.Posts1.OrderBy(p => p.Id).First().Comments.Count,
                                Collection2 = b.Posts2.OrderBy(p => p.Id).First().Comments.Count,
                                Collection3 = b.Posts3.OrderBy(p => p.Id).First().Comments.Count
                            }).ToList();

                    var query3 = context.Blogs
                        .Select(
                            b => new
                            {
                                Collection1 = b.Posts1.OrderBy(p => p.Id),
                                Collection2 = b.Posts2.OrderBy(p => p.Id),
                                Collection3 = b.Posts3.OrderBy(p => p.Id)
                            }).ToList();

                    var query4 = context.Blogs
                        .Where(b => ClientMethod11923(b.Id))
                        .Select(
                            b => new
                            {
                                Collection1 = b.Posts1,
                                Collection2 = b.Posts2,
                                Collection3 = b.Posts3
                            }).ToList();

                    var query5 = context.Blogs
                        .Where(b => ClientMethod11923(b.Id))
                        .Select(
                            b => new
                            {
                                Collection1 = b.Posts1.OrderBy(p => p.Id).First().Comments.Count,
                                Collection2 = b.Posts2.OrderBy(p => p.Id).First().Comments.Count,
                                Collection3 = b.Posts3.OrderBy(p => p.Id).First().Comments.Count
                            }).ToList();

                    var query6 = context.Blogs
                        .Where(b => ClientMethod11923(b.Id))
                        .Select(
                            b => new
                            {
                                Collection1 = b.Posts1.OrderBy(p => p.Id),
                                Collection2 = b.Posts2.OrderBy(p => p.Id),
                                Collection3 = b.Posts3.OrderBy(p => p.Id)
                            }).ToList();
                }
            }
        }

        private SqlServerTestStore CreateDatabase11923()
        {
            return CreateTestStore(
                () => new MyContext11923(_options),
                context =>
                {
                    var p111 = new Post11923
                    {
                        Name = "P111"
                    };
                    var p112 = new Post11923
                    {
                        Name = "P112"
                    };
                    var p121 = new Post11923
                    {
                        Name = "P121"
                    };
                    var p122 = new Post11923
                    {
                        Name = "P122"
                    };
                    var p123 = new Post11923
                    {
                        Name = "P123"
                    };
                    var p131 = new Post11923
                    {
                        Name = "P131"
                    };

                    var p211 = new Post11923
                    {
                        Name = "P211"
                    };
                    var p212 = new Post11923
                    {
                        Name = "P212"
                    };
                    var p221 = new Post11923
                    {
                        Name = "P221"
                    };
                    var p222 = new Post11923
                    {
                        Name = "P222"
                    };
                    var p223 = new Post11923
                    {
                        Name = "P223"
                    };
                    var p231 = new Post11923
                    {
                        Name = "P231"
                    };

                    var b1 = new Blog11923
                    {
                        Name = "B1"
                    };
                    var b2 = new Blog11923
                    {
                        Name = "B2"
                    };

                    b1.Posts1.AddRange(new[] { p111, p112 });
                    b1.Posts2.AddRange(new[] { p121, p122, p123 });
                    b1.Posts3.Add(p131);

                    b2.Posts1.AddRange(new[] { p211, p212 });
                    b2.Posts2.AddRange(new[] { p221, p222, p223 });
                    b2.Posts3.Add(p231);

                    context.Blogs.AddRange(b1, b2);
                    context.Posts.AddRange(p111, p112, p121, p122, p123, p131, p211, p212, p221, p222, p223, p231);
                    context.SaveChanges();

                    ClearLog();
                });
        }

        public class MyContext11923 : DbContext
        {
            public DbSet<Blog11923> Blogs { get; set; }
            public DbSet<Post11923> Posts { get; set; }
            public DbSet<Comment11923> Comments { get; set; }

            public MyContext11923(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog11923>(
                    b =>
                    {
                        b.HasMany(e => e.Posts1).WithOne().HasForeignKey("BlogId1");
                        b.HasMany(e => e.Posts2).WithOne().HasForeignKey("BlogId2");
                        b.HasMany(e => e.Posts3).WithOne().HasForeignKey("BlogId3");
                    });

                modelBuilder.Entity<Post11923>();
            }
        }

        public class Blog11923
        {
            public Blog11923()
            {
                Posts1 = new List<Post11923>();
                Posts2 = new CustomCollection11923();
                Posts3 = new HashSet<Post11923>();
            }

            public Blog11923(List<Post11923> posts1, CustomCollection11923 posts2, HashSet<Post11923> posts3)
            {
                Posts1 = posts1;
                Posts2 = posts2;
                Posts3 = posts3;
            }

            public int Id { get; set; }
            public string Name { get; set; }

            public List<Post11923> Posts1 { get; }
            public CustomCollection11923 Posts2 { get; }
            public HashSet<Post11923> Posts3 { get; }
        }

        public class Post11923
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public List<Comment11923> Comments { get; set; }
        }

        public class Comment11923
        {
            public int Id { get; set; }
        }

        public class CustomCollection11923 : List<Post11923>
        {
        }

        #endregion

        #region Bug11885

        [Fact]
        public virtual void Average_with_cast()
        {
            using (CreateDatabase11885())
            {
                using (var context = new MyContext11885(_options))
                {
                    var prices = context.Prices.ToList();

                    ClearLog();

                    Assert.Equal(prices.Average(e => e.Price), context.Prices.Average(e => e.Price));
                    Assert.Equal(prices.Average(e => e.IntColumn), context.Prices.Average(e => e.IntColumn));
                    Assert.Equal(prices.Average(e => e.NullableIntColumn), context.Prices.Average(e => e.NullableIntColumn));
                    Assert.Equal(prices.Average(e => e.LongColumn), context.Prices.Average(e => e.LongColumn));
                    Assert.Equal(prices.Average(e => e.NullableLongColumn), context.Prices.Average(e => e.NullableLongColumn));
                    Assert.Equal(prices.Average(e => e.FloatColumn), context.Prices.Average(e => e.FloatColumn));
                    Assert.Equal(prices.Average(e => e.NullableFloatColumn), context.Prices.Average(e => e.NullableFloatColumn));
                    Assert.Equal(prices.Average(e => e.DoubleColumn), context.Prices.Average(e => e.DoubleColumn));
                    Assert.Equal(prices.Average(e => e.NullableDoubleColumn), context.Prices.Average(e => e.NullableDoubleColumn));
                    Assert.Equal(prices.Average(e => e.DecimalColumn), context.Prices.Average(e => e.DecimalColumn));
                    Assert.Equal(prices.Average(e => e.NullableDecimalColumn), context.Prices.Average(e => e.NullableDecimalColumn));

                    AssertSql(
                        @"SELECT AVG([e].[Price])
FROM [Prices] AS [e]",
                        //
                        @"SELECT AVG(CAST([e].[IntColumn] AS float))
FROM [Prices] AS [e]",
                        //
                        @"SELECT AVG(CAST([e].[NullableIntColumn] AS float))
FROM [Prices] AS [e]",
                        //
                        @"SELECT AVG(CAST([e].[LongColumn] AS float))
FROM [Prices] AS [e]",
                        //
                        @"SELECT AVG(CAST([e].[NullableLongColumn] AS float))
FROM [Prices] AS [e]",
                        //
                        @"SELECT CAST(AVG([e].[FloatColumn]) AS real)
FROM [Prices] AS [e]",
                        //
                        @"SELECT CAST(AVG([e].[NullableFloatColumn]) AS real)
FROM [Prices] AS [e]",
                        //
                        @"SELECT AVG([e].[DoubleColumn])
FROM [Prices] AS [e]",
                        //
                        @"SELECT AVG([e].[NullableDoubleColumn])
FROM [Prices] AS [e]",
                        //
                        @"SELECT AVG([e].[DecimalColumn])
FROM [Prices] AS [e]",
                        //
                        @"SELECT AVG([e].[NullableDecimalColumn])
FROM [Prices] AS [e]");
                }
            }
        }

        private SqlServerTestStore CreateDatabase11885()
        {
            return CreateTestStore(
                () => new MyContext11885(_options),
                context =>
                {
                    context.AddRange(
                        new Price11885
                        {
                            IntColumn = 1,
                            NullableIntColumn = 1,
                            LongColumn = 1000,
                            NullableLongColumn = 1000,
                            FloatColumn = 0.1F,
                            NullableFloatColumn = 0.1F,
                            DoubleColumn = 0.000001,
                            NullableDoubleColumn = 0.000001,
                            DecimalColumn = 1.0m,
                            NullableDecimalColumn = 1.0m,
                            Price = 0.00112000m
                        },
                        new Price11885
                        {
                            IntColumn = 2,
                            NullableIntColumn = 2,
                            LongColumn = 2000,
                            NullableLongColumn = 2000,
                            FloatColumn = 0.2F,
                            NullableFloatColumn = 0.2F,
                            DoubleColumn = 0.000002,
                            NullableDoubleColumn = 0.000002,
                            DecimalColumn = 2.0m,
                            NullableDecimalColumn = 2.0m,
                            Price = 0.00232111m
                        },
                        new Price11885
                        {
                            IntColumn = 3,
                            LongColumn = 3000,
                            FloatColumn = 0.3F,
                            DoubleColumn = 0.000003,
                            DecimalColumn = 3.0m,
                            Price = 0.00345223m
                        }
                        );

                    context.SaveChanges();

                    ClearLog();
                });
        }

        public class MyContext11885 : DbContext
        {
            public DbSet<Price11885> Prices { get; set; }

            public MyContext11885(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Price11885>(
                    b =>
                    {
                        b.Property(e => e.Price).HasColumnType("DECIMAL(18, 8)");
                        b.Property(e => e.DecimalColumn).HasColumnType("DECIMAL(18, 2)");
                        b.Property(e => e.NullableDecimalColumn).HasColumnType("DECIMAL(18, 2)");
                    });
            }
        }

        public class Price11885
        {
            public int Id { get; set; }
            public int IntColumn { get; set; }
            public int? NullableIntColumn { get; set; }
            public long LongColumn { get; set; }
            public long? NullableLongColumn { get; set; }
            public float FloatColumn { get; set; }
            public float? NullableFloatColumn { get; set; }
            public double DoubleColumn { get; set; }
            public double? NullableDoubleColumn { get; set; }
            public decimal DecimalColumn { get; set; }
            public decimal? NullableDecimalColumn { get; set; }
            public decimal Price { get; set; }
        }

        #endregion

        #region Bug12582

        [Fact]
        public virtual void Include_collection_with_OfType_base()
        {
            using (CreateDatabase12582())
            {
                using (var context = new MyContext12582(_options))
                {
                    var query = context.Employees
                        .Include(i => i.Devices)
                        .OfType<IEmployee12582>()
                        .ToList();

                    Assert.Equal(1, query.Count);

                    var employee = (Employee12582)query[0];
                    Assert.Equal(2, employee.Devices.Count);
                }
            }
        }

        [Fact]
        public virtual void Correlated_collection_with_OfType_base()
        {
            using (CreateDatabase12582())
            {
                using (var context = new MyContext12582(_options))
                {
                    var query = context.Employees
                        .Select(e => e.Devices.Where(d => d.Device != "foo").Cast<IEmployeeDevice12582>())
                        .ToList();

                    Assert.Equal(1, query.Count);
                    var result = query[0];
                    Assert.Equal(2, result.Count());
                }
            }
        }

        private SqlServerTestStore CreateDatabase12582()
        {
            return CreateTestStore(
                () => new MyContext12582(_options),
                context =>
                {
                    var d1 = new EmployeeDevice12582 { Device = "d1" };
                    var d2 = new EmployeeDevice12582 { Device = "d2" };
                    var e = new Employee12582 { Devices = new List<EmployeeDevice12582> { d1, d2 }, Name = "e" };

                    context.Devices.AddRange(d1, d2);
                    context.Employees.Add(e);
                    context.SaveChanges();

                    ClearLog();
                });
        }

        public interface IEmployee12582
        {
            string Name { get; set; }
        }

        public class Employee12582 : IEmployee12582
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public ICollection<EmployeeDevice12582> Devices { get; set; }
        }

        public interface IEmployeeDevice12582
        {
            string Device { get; set; }
        }

        public class EmployeeDevice12582 : IEmployeeDevice12582
        {
            public int Id { get; set; }
            public int EmployeeId { get; set; }
            public string Device { get; set; }
            public Employee12582 Employee { get; set; }
        }

        public class MyContext12582 : DbContext
        {
            public DbSet<Employee12582> Employees { get; set; }
            public DbSet<EmployeeDevice12582> Devices { get; set; }

            public MyContext12582(DbContextOptions options)
                : base(options)
            {
            }
        }

        #endregion

        #region Bug12748

        [Fact]
        public virtual void Correlated_collection_correctly_associates_entities_with_byte_array_keys()
        {
            using (CreateDatabase12748())
            {
                using (var context = new MyContext12748(_options))
                {
                    var query = from blog in context.Blogs
                                select new
                                {
                                    blog.Name,
                                    Comments = blog.Comments.Select(u => new
                                    {
                                        u.Id,
                                    }).ToArray(),
                                };
                    var result = query.ToList();
                    Assert.Equal(1, result[0].Comments.Count());
                }
            }
        }

        private SqlServerTestStore CreateDatabase12748()
        {
            return CreateTestStore(
                () => new MyContext12748(_options),
                context =>
                {
                    context.Blogs.Add(new Blog12748 { Name = Encoding.UTF8.GetBytes("Awesome Blog") });
                    context.Comments.Add(new Comment12748 { BlogName = Encoding.UTF8.GetBytes("Awesome Blog") });
                    context.SaveChanges();
                    ClearLog();
                });
        }

        public class MyContext12748 : DbContext
        {
            public DbSet<Blog12748> Blogs { get; set; }
            public DbSet<Comment12748> Comments { get; set; }
            public MyContext12748(DbContextOptions options)
               : base(options)
            {
            }
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
            }
        }
        public class Blog12748
        {
            [Key]
            public byte[] Name { get; set; }
            public List<Comment12748> Comments { get; set; }
        }
        public class Comment12748
        {
            public int Id { get; set; }
            public byte[] BlogName { get; set; }
            public Blog12748 Blog { get; set; }
        }

        #endregion

        #region Bug13025

        [Fact]
        public virtual void Find_underlying_property_after_GroupJoin_DefaultIfEmpty()
        {
            using (CreateDatabase13025())
            {
                using (var context = new MyContext13025(_options))
                {
                    var query = (from e in context.Employees
                                 join d in context.EmployeeDevices
                                    on e.Id equals d.EmployeeId into grouping
                                 from j in grouping.DefaultIfEmpty()
                                 select new Holder13025
                                 {
                                     Name = e.Name,
                                     DeviceId = j.DeviceId
                                 }).ToList();
                }
            }
        }

        public class Holder13025
        {
            public string Name { get; set; }
            public int? DeviceId { get; set; }
        }

        private SqlServerTestStore CreateDatabase13025()
        {
            return CreateTestStore(
                () => new MyContext13025(_options),
                context =>
                {
                    context.AddRange(
                        new Employee13025
                        {
                            Name = "Test1",
                            Devices = new List<EmployeeDevice13025>
                            {
                                new EmployeeDevice13025
                                {
                                    DeviceId = 1,
                                    Device = "Battery"
                                }
                            }
                        });

                    context.SaveChanges();
                    ClearLog();
                });
        }

        public class MyContext13025 : DbContext
        {
            public DbSet<Employee13025> Employees { get; set; }
            public DbSet<EmployeeDevice13025> EmployeeDevices { get; set; }
            public MyContext13025(DbContextOptions options)
               : base(options)
            {
            }
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
            }
        }

        public class Employee13025
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public ICollection<EmployeeDevice13025> Devices { get; set; }
        }

        public class EmployeeDevice13025
        {
            public int Id { get; set; }
            public short DeviceId { get; set; }
            public int EmployeeId { get; set; }
            public string Device { get; set; }
            public Employee13025 Employee { get; set; }
        }

        #endregion

        #region Bug12170

        [Fact]
        public virtual void Weak_entities_with_query_filter_subquery_flattening()
        {
            using (CreateDatabase12170())
            {
                using (var context = new MyContext12170(_options))
                {
                    var result = context.Definitions.Any();
                }
            }
        }

        private SqlServerTestStore CreateDatabase12170()
        {
            return CreateTestStore(
                () => new MyContext12170(_options),
                context =>
                {
                    context.SaveChanges();
                    ClearLog();
                });
        }

        public class MyContext12170 : DbContext
        {
            public virtual DbSet<Definition12170> Definitions { get; set; }
            public virtual DbSet<DefinitionHistory12170> DefinitionHistories { get; set; }

            public MyContext12170(DbContextOptions options)
               : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Definition12170>().HasQueryFilter(md => md.ChangeInfo.RemovedPoint.Timestamp == null);
                modelBuilder.Entity<Definition12170>().HasOne(h => h.LatestHistoryEntry).WithMany();
                modelBuilder.Entity<Definition12170>().HasMany(h => h.HistoryEntries).WithOne(h => h.Definition);

                modelBuilder.Entity<DefinitionHistory12170>().OwnsOne(h => h.EndedPoint);
            }
        }

        [Owned]
        public class OptionalChangePoint12170
        {
            public DateTime? Timestamp { get; set; }
        }

        [Owned]
        public class MasterChangeInfo12170
        {
            public virtual OptionalChangePoint12170 RemovedPoint { get; set; }
        }

        public partial class DefinitionHistory12170
        {
            public int Id { get; set; }
            public int MacGuffinDefinitionID { get; set; }
            public virtual Definition12170 Definition { get; set; }
            public OptionalChangePoint12170 EndedPoint { get; set; }
        }

        public partial class Definition12170
        {
            public int Id { get; set; }
            public virtual MasterChangeInfo12170 ChangeInfo { get; set; }

            public virtual ICollection<DefinitionHistory12170> HistoryEntries { get; set; }
            public virtual DefinitionHistory12170 LatestHistoryEntry { get; set; }
            public int? LatestHistoryEntryID { get; set; }
        }

        #endregion

        #region Bug11944

        [Fact]
        public virtual void Include_collection_works_when_defined_on_intermediate_type()
        {
            using (CreateDatabase11944())
            {
                using (var context = new MyContext11944(_options))
                {
                    var query = context.Schools.Include(s => ((ElementarySchool11944)s).Students);
                    var result = query.ToList();

                    Assert.Equal(2, result.Count);
                    Assert.Equal(2, result.OfType<ElementarySchool11944>().Single().Students.Count);
                }
            }
        }

        [Fact]
        public virtual void Correlated_collection_works_when_defined_on_intermediate_type()
        {
            using (CreateDatabase11944())
            {
                using (var context = new MyContext11944(_options))
                {
                    var query = context.Schools.Select(s => ((ElementarySchool11944)s).Students.Where(ss => true).ToList());
                    var result = query.ToList();

                    Assert.Equal(2, result.Count);
                    Assert.True(result.Any(r => r.Count() == 2));
                }
            }
        }

        private SqlServerTestStore CreateDatabase11944()
        {
            return CreateTestStore(
                () => new MyContext11944(_options),
                context =>
                {
                    var student1 = new Student11944();
                    var student2 = new Student11944();
                    var school = new School11944();
                    var elementarySchool = new ElementarySchool11944 { Students = new List<Student11944> { student1, student2 } };

                    context.Students.AddRange(student1, student2);
                    context.Schools.AddRange(school);
                    context.ElementarySchools.Add(elementarySchool);

                    context.SaveChanges();
                    ClearLog();
                });
        }

        public class MyContext11944 : DbContext
        {
            public DbSet<Student11944> Students { get; set; }
            public DbSet<School11944> Schools { get; set; }
            public DbSet<ElementarySchool11944> ElementarySchools { get; set; }

            public MyContext11944(DbContextOptions options)
               : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ElementarySchool11944>().HasMany(s => s.Students).WithOne(s => s.School);
            }
        }

        public class Student11944
        {
            public int Id { get; set; }
            public ElementarySchool11944 School { get; set; }
        }

        public class School11944
        {
            public int Id { get; set; }
        }

        public abstract class PrimarySchool11944 : School11944
        {
            public List<Student11944> Students { get; set; }
        }

        public class ElementarySchool11944 : PrimarySchool11944
        {
        }

        #endregion

        #region Bug13118

        [Fact]
        public virtual void DateTime_Contains_with_smalldatetime_generates_correct_literal()
        {
            using (CreateDatabase13118())
            {
                using (var context = new MyContext13118(_options))
                {
                    var testDateList = new List<DateTime>() { new DateTime(2018, 10, 07) };
                    var findRecordsWithDateInList = context.ReproEntity
                        .Where(a => testDateList.Contains(a.MyTime))
                        .ToList();

                    Assert.Single(findRecordsWithDateInList);

                    AssertSql(
                        @"SELECT [a].[Id], [a].[MyTime]
FROM [ReproEntity] AS [a]
WHERE [a].[MyTime] IN ('2018-10-07T00:00:00.000')");
                }
            }
        }

        private SqlServerTestStore CreateDatabase13118()
        {
            return CreateTestStore(
                () => new MyContext13118(_options),
                context =>
                {
                    context.AddRange(
                        new ReproEntity13118
                        {
                            MyTime = new DateTime(2018, 10, 07)
                        },
                        new ReproEntity13118
                        {
                            MyTime = new DateTime(2018, 10, 08)
                        });

                    context.SaveChanges();
                    ClearLog();
                });
        }

        public class MyContext13118 : DbContext
        {
            public virtual DbSet<ReproEntity13118> ReproEntity { get; set; }

            public MyContext13118(DbContextOptions options)
               : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ReproEntity13118>(e => e.Property("MyTime").HasColumnType("smalldatetime"));
            }
        }

        public class ReproEntity13118
        {
            public Guid Id { get; set; }
            public DateTime MyTime { get; set; }
        }

        #endregion

        #region Bug12732

        [Fact]
        public virtual void Nested_contains_with_enum()
        {
            using (CreateDatabase12732())
            {
                using (var context = new MyContext12732(_options))
                {
                    var key = Guid.Parse("5f221fb9-66f4-442a-92c9-d97ed5989cc7");
                    var keys = new List<Guid> { Guid.Parse("0a47bcb7-a1cb-4345-8944-c58f82d6aac7"), key };
                    var todoTypes = new List<TodoType> { TodoType.foo0 };

                    var query = context.Todos
                        .Where(x => keys.Contains(todoTypes.Contains(x.Type) ? key : key))
                        .ToList();

                    Assert.Single(query);

                    AssertSql(
                        @"@__key_2='5f221fb9-66f4-442a-92c9-d97ed5989cc7'

SELECT [x].[Id], [x].[Type]
FROM [Todos] AS [x]
WHERE CASE
    WHEN [x].[Type] IN (0)
    THEN @__key_2 ELSE @__key_2
END IN ('0a47bcb7-a1cb-4345-8944-c58f82d6aac7', '5f221fb9-66f4-442a-92c9-d97ed5989cc7')");
                }
            }
        }

        private SqlServerTestStore CreateDatabase12732()
        {
            return CreateTestStore(
                () => new MyContext12732(_options),
                context =>
                {
                    context.Add(
                        new Todo
                        {
                            Type = TodoType.foo0
                        });
                    context.SaveChanges();
                    ClearLog();
                });
        }

        private class MyContext12732 : DbContext
        {
            public DbSet<Todo> Todos { get; set; }

            public MyContext12732(DbContextOptions options)
               : base(options)
            {
            }
        }

        private class Todo
        {
            public Guid Id { get; set; }
            public TodoType Type { get; set; }
        }

        private enum TodoType
        {
            foo0 = 0
        }

        #endregion

        #region Bug13157

        [Fact]
        public virtual void Correlated_subquery_with_owned_navigation_being_compared_to_null_works()
        {
            using (CreateDatabase13157())
            {
                using (var context = new MyContext13157(_options))
                {
                    var partners = context.Partners
                        .Select(x => new
                        {
                            Addresses = x.Addresses.Select(y => new
                            {
                                Turnovers = y.Turnovers == null ? null : new
                                {
                                    y.Turnovers.AmountIn
                                },
                            }).ToList()
                        }).ToList();

                    Assert.Single(partners);
                    Assert.Single(partners[0].Addresses);
                    Assert.NotNull(partners[0].Addresses[0].Turnovers);
                    Assert.Equal(10, partners[0].Addresses[0].Turnovers.AmountIn);

                    AssertSql(
                        @"SELECT [x].[Id]
FROM [Partners] AS [x]
ORDER BY [x].[Id]",
                        //
                        @"SELECT [t].[Id], CASE
    WHEN [x.Addresses].[Id] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END, [x.Addresses].[Turnovers_AmountIn] AS [AmountIn], [x.Addresses].[Partner13157Id]
FROM [Address13157] AS [x.Addresses]
INNER JOIN (
    SELECT [x0].[Id]
    FROM [Partners] AS [x0]
) AS [t] ON [x.Addresses].[Partner13157Id] = [t].[Id]
ORDER BY [t].[Id]");
                }
            }
        }

        private SqlServerTestStore CreateDatabase13157()
        {
            return CreateTestStore(
                () => new MyContext13157(_options),
                context =>
                {
                    context.AddRange(
                        new Partner13157
                        {
                            Addresses = new List<Address13157>
                            {
                                new Address13157
                                {
                                    Turnovers = new AddressTurnovers13157
                                    {
                                        AmountIn = 10
                                    }
                                }
                            }
                        }
                        );

                    context.SaveChanges();
                    ClearLog();
                });
        }

        public class MyContext13157 : DbContext
        {
            public virtual DbSet<Partner13157> Partners { get; set; }

            public MyContext13157(DbContextOptions options)
               : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Address13157>().OwnsOne(x => x.Turnovers);
            }
        }

        public class Partner13157
        {
            public int Id { get; set; }
            public ICollection<Address13157> Addresses { get; set; }
        }

        public class Address13157
        {
            public int Id { get; set; }
            public AddressTurnovers13157 Turnovers { get; set; }
        }

        public class AddressTurnovers13157
        {
            public int AmountIn { get; set; }
        }

        #endregion

        #region Bug13346

        [Fact(Skip = "See issue#13587")]
        public virtual void ToQuery_can_define_in_own_terms_using_FromSql()
        {
            using (CreateDatabase13346())
            {
                using (var context = new MyContext13346(_options))
                {
                    var query = context.Query<OrderSummary13346>().ToList();

                    Assert.Equal(4, query.Count);

                    AssertSql(
                        "SELECT o.Amount From Orders AS o");
                }
            }
        }

        private SqlServerTestStore CreateDatabase13346()
        {
            return CreateTestStore(
                () => new MyContext13346(_options),
                context =>
                {
                    context.AddRange(
                        new Order13346 { Amount = 1},
                        new Order13346 { Amount = 2},
                        new Order13346 { Amount = 3},
                        new Order13346 { Amount = 4}
                        );

                    context.SaveChanges();
                    ClearLog();
                });
        }

        public class MyContext13346 : DbContext
        {
            public virtual DbSet<Order13346> Orders { get; set; }

            public MyContext13346(DbContextOptions options)
               : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Query<OrderSummary13346>().ToQuery(
                    () => Query<OrderSummary13346>()
                            .FromSql("SELECT o.Amount From Orders AS o"));
            }
        }

        public class Order13346
        {
            public int Id { get; set; }
            public int Amount { get; set; }
        }

        public class OrderSummary13346
        {
            public int Amount { get; set; }
        }

        #endregion

        #region Bug13079

        [Fact]
        public virtual void Multilevel_owned_entities_determine_correct_nullability()
        {
            using (CreateDatabase13079())
            {
                using (var context = new MyContext13079(_options))
                {
                    context.Add(new BaseEntity13079());
                    context.SaveChanges();

                    AssertSql(
                        @"@p0='BaseEntity13079' (Nullable = false) (Size = 4000)

SET NOCOUNT ON;
INSERT INTO [BaseEntities] ([Discriminator])
VALUES (@p0);
SELECT [Id]
FROM [BaseEntities]
WHERE @@ROWCOUNT = 1 AND [Id] = scope_identity();");
                }
            }
        }

        private SqlServerTestStore CreateDatabase13079()
        {
            return CreateTestStore(
                () => new MyContext13079(_options),
                context => ClearLog());
        }

        public class MyContext13079 : DbContext
        {
            public virtual DbSet<BaseEntity13079> BaseEntities { get; set; }

            public MyContext13079(DbContextOptions options)
               : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<DerivedEntity13079>().OwnsOne(e => e.Data, b => b.OwnsOne(e => e.SubData));
            }
        }

        public class BaseEntity13079
        {
            public int Id { get; set; }
        }

        public class DerivedEntity13079 : BaseEntity13079
        {
            public int Property { get; set; }
            public OwnedData13079 Data { get; set; }
        }

        public class OwnedData13079
        {
            public int Property { get; set; }
            public OwnedSubData13079 SubData { get; set; }
        }

        public class OwnedSubData13079
        {
            public int Property { get; set; }
        }

        #endregion

        #region Bug13587

        [Fact]
        public virtual void Type_casting_inside_sum()
        {
            using (CreateDatabase13587())
            {
                using (var context = new MyContext13587(_options))
                {
                    var result = context.InventoryPools.Sum(p => (decimal)p.Quantity);

                    AssertSql(
                        @"SELECT SUM(CAST([p].[Quantity] AS decimal(18,2)))
FROM [InventoryPools] AS [p]");
                }
            }
        }

        private SqlServerTestStore CreateDatabase13587()
        {
            return CreateTestStore(
                () => new MyContext13587(_options),
                context => {
                    context.InventoryPools.Add(new InventoryPool13587
                    {
                        Quantity = 2,
                    });

                    context.SaveChanges();

                    ClearLog();
                });
        }

        public class MyContext13587 : DbContext
        {
            public virtual DbSet<InventoryPool13587> InventoryPools { get; set; }

            public MyContext13587(DbContextOptions options)
               : base(options)
            {
            }
        }

        public class InventoryPool13587
        {
            public int Id { get; set; }
            public double Quantity { get; set; }
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
                context.Database.EnsureCreatedResiliently();
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
