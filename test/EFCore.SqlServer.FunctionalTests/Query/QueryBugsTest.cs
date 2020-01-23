// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Caching.Memory;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable IDE0063 // Use simple 'using' statement

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
#pragma warning disable IDE0060 // Remove unused parameter
        public QueryBugsTest(SqlServerFixture fixture, ITestOutputHelper testOutputHelper)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected SqlServerFixture Fixture { get; }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Where_equals_DateTime_Now(bool async)
        {
            using (CreateDateTimeStore())
            {
                Fixture.TestSqlLoggerFactory.Clear();

                using var context = new DateTimeContext(_options);
                var query = context.Dates.Where(
                    d => d.DateTime2_2 == DateTime.Now
                        || d.DateTime2_7 == DateTime.Now
                        || d.DateTime == DateTime.Now
                        || d.SmallDateTime == DateTime.Now);

                var results = async
                    ? await query.ToListAsync()
                    : query.ToList();

                Assert.Empty(results);

                AssertSql(
                    @"SELECT [d].[Id], [d].[DateTime], [d].[DateTime2], [d].[DateTime2_0], [d].[DateTime2_1], [d].[DateTime2_2], [d].[DateTime2_3], [d].[DateTime2_4], [d].[DateTime2_5], [d].[DateTime2_6], [d].[DateTime2_7], [d].[SmallDateTime]
FROM [Dates] AS [d]
WHERE ((([d].[DateTime2_2] = GETDATE()) OR ([d].[DateTime2_7] = GETDATE())) OR ([d].[DateTime] = GETDATE())) OR ([d].[SmallDateTime] = GETDATE())");
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Where_not_equals_DateTime_Now(bool async)
        {
            using (CreateDateTimeStore())
            {
                Fixture.TestSqlLoggerFactory.Clear();

                using var context = new DateTimeContext(_options);
                var query = context.Dates.Where(
                    d => d.DateTime2_2 != DateTime.Now
                        && d.DateTime2_7 != DateTime.Now
                        && d.DateTime != DateTime.Now
                        && d.SmallDateTime != DateTime.Now);

                var results = async
                    ? await query.ToListAsync()
                    : query.ToList();

                Assert.Single(results);

                AssertSql(
                    @"SELECT [d].[Id], [d].[DateTime], [d].[DateTime2], [d].[DateTime2_0], [d].[DateTime2_1], [d].[DateTime2_2], [d].[DateTime2_3], [d].[DateTime2_4], [d].[DateTime2_5], [d].[DateTime2_6], [d].[DateTime2_7], [d].[SmallDateTime]
FROM [Dates] AS [d]
WHERE ((([d].[DateTime2_2] <> GETDATE()) AND ([d].[DateTime2_7] <> GETDATE())) AND ([d].[DateTime] <> GETDATE())) AND ([d].[SmallDateTime] <> GETDATE())");
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Where_equals_new_DateTime(bool async)
        {
            using (CreateDateTimeStore())
            {
                Fixture.TestSqlLoggerFactory.Clear();

                using var context = new DateTimeContext(_options);
                var query = context.Dates.Where(
                    d => d.SmallDateTime == new DateTime(1970, 9, 3, 12, 0, 0)
                        && d.DateTime == new DateTime(1971, 9, 3, 12, 0, 10, 220)
                        && d.DateTime2 == new DateTime(1972, 9, 3, 12, 0, 10, 333)
                        && d.DateTime2_0 == new DateTime(1973, 9, 3, 12, 0, 10)
                        && d.DateTime2_1 == new DateTime(1974, 9, 3, 12, 0, 10, 500)
                        && d.DateTime2_2 == new DateTime(1975, 9, 3, 12, 0, 10, 660)
                        && d.DateTime2_3 == new DateTime(1976, 9, 3, 12, 0, 10, 777)
                        && d.DateTime2_4 == new DateTime(1977, 9, 3, 12, 0, 10, 888)
                        && d.DateTime2_5 == new DateTime(1978, 9, 3, 12, 0, 10, 999)
                        && d.DateTime2_6 == new DateTime(1979, 9, 3, 12, 0, 10, 111)
                        && d.DateTime2_7 == new DateTime(1980, 9, 3, 12, 0, 10, 222));

                var results = async
                    ? await query.ToListAsync()
                    : query.ToList();

                Assert.Single(results);

                AssertSql(
                    @"SELECT [d].[Id], [d].[DateTime], [d].[DateTime2], [d].[DateTime2_0], [d].[DateTime2_1], [d].[DateTime2_2], [d].[DateTime2_3], [d].[DateTime2_4], [d].[DateTime2_5], [d].[DateTime2_6], [d].[DateTime2_7], [d].[SmallDateTime]
FROM [Dates] AS [d]
WHERE (((((((((([d].[SmallDateTime] = '1970-09-03T12:00:00') AND ([d].[DateTime] = '1971-09-03T12:00:10.220')) AND ([d].[DateTime2] = '1972-09-03T12:00:10.3330000')) AND ([d].[DateTime2_0] = '1973-09-03T12:00:10')) AND ([d].[DateTime2_1] = '1974-09-03T12:00:10.5')) AND ([d].[DateTime2_2] = '1975-09-03T12:00:10.66')) AND ([d].[DateTime2_3] = '1976-09-03T12:00:10.777')) AND ([d].[DateTime2_4] = '1977-09-03T12:00:10.8880')) AND ([d].[DateTime2_5] = '1978-09-03T12:00:10.99900')) AND ([d].[DateTime2_6] = '1979-09-03T12:00:10.111000')) AND ([d].[DateTime2_7] = '1980-09-03T12:00:10.2220000')");
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Where_contains_DateTime_literals(bool async)
        {
            var dateTimes = new[]
            {
                new DateTime(1970, 9, 3, 12, 0, 0),
                new DateTime(1971, 9, 3, 12, 0, 10, 220),
                new DateTime(1972, 9, 3, 12, 0, 10, 333),
                new DateTime(1973, 9, 3, 12, 0, 10),
                new DateTime(1974, 9, 3, 12, 0, 10, 500),
                new DateTime(1975, 9, 3, 12, 0, 10, 660),
                new DateTime(1976, 9, 3, 12, 0, 10, 777),
                new DateTime(1977, 9, 3, 12, 0, 10, 888),
                new DateTime(1978, 9, 3, 12, 0, 10, 999),
                new DateTime(1979, 9, 3, 12, 0, 10, 111),
                new DateTime(1980, 9, 3, 12, 0, 10, 222)
            };

            using (CreateDateTimeStore())
            {
                Fixture.TestSqlLoggerFactory.Clear();

                using var context = new DateTimeContext(_options);
                var query = context.Dates.Where(
                    d => dateTimes.Contains(d.SmallDateTime)
                        && dateTimes.Contains(d.DateTime)
                        && dateTimes.Contains(d.DateTime2)
                        && dateTimes.Contains(d.DateTime2_0)
                        && dateTimes.Contains(d.DateTime2_1)
                        && dateTimes.Contains(d.DateTime2_2)
                        && dateTimes.Contains(d.DateTime2_3)
                        && dateTimes.Contains(d.DateTime2_4)
                        && dateTimes.Contains(d.DateTime2_5)
                        && dateTimes.Contains(d.DateTime2_6)
                        && dateTimes.Contains(d.DateTime2_7));

                var results = async
                    ? await query.ToListAsync()
                    : query.ToList();

                Assert.Single(results);

                AssertSql(
                    @"SELECT [d].[Id], [d].[DateTime], [d].[DateTime2], [d].[DateTime2_0], [d].[DateTime2_1], [d].[DateTime2_2], [d].[DateTime2_3], [d].[DateTime2_4], [d].[DateTime2_5], [d].[DateTime2_6], [d].[DateTime2_7], [d].[SmallDateTime]
FROM [Dates] AS [d]
WHERE ((((((((([d].[SmallDateTime] IN ('1970-09-03T12:00:00', '1971-09-03T12:00:10', '1972-09-03T12:00:10', '1973-09-03T12:00:10', '1974-09-03T12:00:10', '1975-09-03T12:00:10', '1976-09-03T12:00:10', '1977-09-03T12:00:10', '1978-09-03T12:00:10', '1979-09-03T12:00:10', '1980-09-03T12:00:10') AND [d].[DateTime] IN ('1970-09-03T12:00:00.000', '1971-09-03T12:00:10.220', '1972-09-03T12:00:10.333', '1973-09-03T12:00:10.000', '1974-09-03T12:00:10.500', '1975-09-03T12:00:10.660', '1976-09-03T12:00:10.777', '1977-09-03T12:00:10.888', '1978-09-03T12:00:10.999', '1979-09-03T12:00:10.111', '1980-09-03T12:00:10.222')) AND [d].[DateTime2] IN ('1970-09-03T12:00:00.0000000', '1971-09-03T12:00:10.2200000', '1972-09-03T12:00:10.3330000', '1973-09-03T12:00:10.0000000', '1974-09-03T12:00:10.5000000', '1975-09-03T12:00:10.6600000', '1976-09-03T12:00:10.7770000', '1977-09-03T12:00:10.8880000', '1978-09-03T12:00:10.9990000', '1979-09-03T12:00:10.1110000', '1980-09-03T12:00:10.2220000')) AND [d].[DateTime2_0] IN ('1970-09-03T12:00:00', '1971-09-03T12:00:10', '1972-09-03T12:00:10', '1973-09-03T12:00:10', '1974-09-03T12:00:10', '1975-09-03T12:00:10', '1976-09-03T12:00:10', '1977-09-03T12:00:10', '1978-09-03T12:00:10', '1979-09-03T12:00:10', '1980-09-03T12:00:10')) AND [d].[DateTime2_1] IN ('1970-09-03T12:00:00.0', '1971-09-03T12:00:10.2', '1972-09-03T12:00:10.3', '1973-09-03T12:00:10.0', '1974-09-03T12:00:10.5', '1975-09-03T12:00:10.6', '1976-09-03T12:00:10.7', '1977-09-03T12:00:10.8', '1978-09-03T12:00:10.9', '1979-09-03T12:00:10.1', '1980-09-03T12:00:10.2')) AND [d].[DateTime2_2] IN ('1970-09-03T12:00:00.00', '1971-09-03T12:00:10.22', '1972-09-03T12:00:10.33', '1973-09-03T12:00:10.00', '1974-09-03T12:00:10.50', '1975-09-03T12:00:10.66', '1976-09-03T12:00:10.77', '1977-09-03T12:00:10.88', '1978-09-03T12:00:10.99', '1979-09-03T12:00:10.11', '1980-09-03T12:00:10.22')) AND [d].[DateTime2_3] IN ('1970-09-03T12:00:00.000', '1971-09-03T12:00:10.220', '1972-09-03T12:00:10.333', '1973-09-03T12:00:10.000', '1974-09-03T12:00:10.500', '1975-09-03T12:00:10.660', '1976-09-03T12:00:10.777', '1977-09-03T12:00:10.888', '1978-09-03T12:00:10.999', '1979-09-03T12:00:10.111', '1980-09-03T12:00:10.222')) AND [d].[DateTime2_4] IN ('1970-09-03T12:00:00.0000', '1971-09-03T12:00:10.2200', '1972-09-03T12:00:10.3330', '1973-09-03T12:00:10.0000', '1974-09-03T12:00:10.5000', '1975-09-03T12:00:10.6600', '1976-09-03T12:00:10.7770', '1977-09-03T12:00:10.8880', '1978-09-03T12:00:10.9990', '1979-09-03T12:00:10.1110', '1980-09-03T12:00:10.2220')) AND [d].[DateTime2_5] IN ('1970-09-03T12:00:00.00000', '1971-09-03T12:00:10.22000', '1972-09-03T12:00:10.33300', '1973-09-03T12:00:10.00000', '1974-09-03T12:00:10.50000', '1975-09-03T12:00:10.66000', '1976-09-03T12:00:10.77700', '1977-09-03T12:00:10.88800', '1978-09-03T12:00:10.99900', '1979-09-03T12:00:10.11100', '1980-09-03T12:00:10.22200')) AND [d].[DateTime2_6] IN ('1970-09-03T12:00:00.000000', '1971-09-03T12:00:10.220000', '1972-09-03T12:00:10.333000', '1973-09-03T12:00:10.000000', '1974-09-03T12:00:10.500000', '1975-09-03T12:00:10.660000', '1976-09-03T12:00:10.777000', '1977-09-03T12:00:10.888000', '1978-09-03T12:00:10.999000', '1979-09-03T12:00:10.111000', '1980-09-03T12:00:10.222000')) AND [d].[DateTime2_7] IN ('1970-09-03T12:00:00.0000000', '1971-09-03T12:00:10.2200000', '1972-09-03T12:00:10.3330000', '1973-09-03T12:00:10.0000000', '1974-09-03T12:00:10.5000000', '1975-09-03T12:00:10.6600000', '1976-09-03T12:00:10.7770000', '1977-09-03T12:00:10.8880000', '1978-09-03T12:00:10.9990000', '1979-09-03T12:00:10.1110000', '1980-09-03T12:00:10.2220000')");
            }
        }

        private class DatesAndPrunes
        {
            public int Id { get; set; }

            [Column(TypeName = "smalldatetime")]
            public DateTime SmallDateTime { get; set; }

            [Column(TypeName = "datetime")]
            public DateTime DateTime { get; set; }

            [Column(TypeName = "datetime2")]
            public DateTime DateTime2 { get; set; }

            [Column(TypeName = "datetime2(0)")]
            public DateTime DateTime2_0 { get; set; }

            [Column(TypeName = "datetime2(1)")]
            public DateTime DateTime2_1 { get; set; }

            [Column(TypeName = "datetime2(2)")]
            public DateTime DateTime2_2 { get; set; }

            [Column(TypeName = "datetime2(3)")]
            public DateTime DateTime2_3 { get; set; }

            [Column(TypeName = "datetime2(4)")]
            public DateTime DateTime2_4 { get; set; }

            [Column(TypeName = "datetime2(5)")]
            public DateTime DateTime2_5 { get; set; }

            [Column(TypeName = "datetime2(6)")]
            public DateTime DateTime2_6 { get; set; }

            [Column(TypeName = "datetime2(7)")]
            public DateTime DateTime2_7 { get; set; }
        }

        private class DateTimeContext : DbContext
        {
            public DateTimeContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<DatesAndPrunes> Dates { get; set; }
        }

        private SqlServerTestStore CreateDateTimeStore()
            => CreateTestStore(
                () => new DateTimeContext(_options),
                c =>
                {
                    c.Add(
                        new DatesAndPrunes
                        {
                            SmallDateTime = new DateTime(1970, 9, 3, 12, 0, 0),
                            DateTime = new DateTime(1971, 9, 3, 12, 0, 10, 220),
                            DateTime2 = new DateTime(1972, 9, 3, 12, 0, 10, 333),
                            DateTime2_0 = new DateTime(1973, 9, 3, 12, 0, 10),
                            DateTime2_1 = new DateTime(1974, 9, 3, 12, 0, 10, 500),
                            DateTime2_2 = new DateTime(1975, 9, 3, 12, 0, 10, 660),
                            DateTime2_3 = new DateTime(1976, 9, 3, 12, 0, 10, 777),
                            DateTime2_4 = new DateTime(1977, 9, 3, 12, 0, 10, 888),
                            DateTime2_5 = new DateTime(1978, 9, 3, 12, 0, 10, 999),
                            DateTime2_6 = new DateTime(1979, 9, 3, 12, 0, 10, 111),
                            DateTime2_7 = new DateTime(1980, 9, 3, 12, 0, 10, 222)
                        });

                    c.SaveChanges();
                });

        #region Bug6901

        [ConditionalFact]
        public void Left_outer_join_bug_6091()
        {
            using var testStore = SqlServerTestStore.CreateInitialized("QueryBugsTest");
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

            using var context = new Bug6091Context(Fixture.CreateOptions(testStore));
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

        [ConditionalFact]
        public async Task Multiple_optional_navs_should_not_deadlock_bug_5481()
        {
            using var testStore = SqlServerTestStore.CreateInitialized("QueryBugsTest");
            using var context = new DeadlockContext(Fixture.CreateOptions(testStore));
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

        [ConditionalFact(Skip = "Issue#15751")]
        public void Query_when_null_key_in_database_should_throw()
        {
            using var testStore = SqlServerTestStore.CreateInitialized("QueryBugsTest");
            testStore.ExecuteNonQuery(
                @"CREATE TABLE ZeroKey (Id int);
                      INSERT ZeroKey VALUES (NULL)");

            using var context = new NullKeyContext(Fixture.CreateOptions(testStore));
            Assert.Equal(
                CoreStrings.ErrorMaterializingPropertyNullReference("ZeroKey", "Id", typeof(int)),
                Assert.Throws<InvalidOperationException>(() => context.ZeroKeys.ToList()).Message);
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

        [ConditionalFact]
        public async Task First_FirstOrDefault_ix_async_bug_603()
        {
            using (CreateDatabase603())
            {
                using (var context = new MyContext603(_options))
                {
                    context.Products.Add(
                        new Product { Name = "Product 1" });
                    context.SaveChanges();
                }

                using var ctx = new MyContext603(_options);
                var product = await ctx.Products.OrderBy(p => p.Id).FirstAsync();

                ctx.Products.Remove(product);

                await ctx.SaveChangesAsync();
            }

            using (CreateDatabase603())
            {
                using (var context = new MyContext603(_options))
                {
                    context.Products.Add(
                        new Product { Name = "Product 1" });
                    context.SaveChanges();
                }

                using var ctx = new MyContext603(_options);
                var product = await ctx.Products.OrderBy(p => p.Id).FirstOrDefaultAsync();

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

        [ConditionalFact]
        public void Include_on_entity_with_composite_key_One_To_Many_bugs_925_926()
        {
            using (CreateDatabase925())
            {
                using var ctx = new MyContext925(_options);
                var query = ctx.Customers.Include(c => c.Orders).OrderBy(c => c.FirstName).ThenBy(c => c.LastName);
                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal(2, result[0].Orders.Count);
                Assert.Equal(3, result[1].Orders.Count);

                AssertSql(
                    @"SELECT [c].[FirstName], [c].[LastName], [o].[Id], [o].[CustomerFirstName], [o].[CustomerLastName], [o].[Name]
FROM [Customer] AS [c]
LEFT JOIN [Order] AS [o] ON ([c].[FirstName] = [o].[CustomerFirstName]) AND ([c].[LastName] = [o].[CustomerLastName])
ORDER BY [c].[FirstName], [c].[LastName], [o].[Id]");
            }
        }

        [ConditionalFact]
        public void Include_on_entity_with_composite_key_Many_To_One_bugs_925_926()
        {
            using (CreateDatabase925())
            {
                using var ctx = new MyContext925(_options);
                var query = ctx.Orders.Include(o => o.Customer);
                var result = query.ToList();

                Assert.Equal(5, result.Count);
                Assert.NotNull(result[0].Customer);
                Assert.NotNull(result[1].Customer);
                Assert.NotNull(result[2].Customer);
                Assert.NotNull(result[3].Customer);
                Assert.NotNull(result[4].Customer);

                AssertSql(
                    @"SELECT [o].[Id], [o].[CustomerFirstName], [o].[CustomerLastName], [o].[Name], [c].[FirstName], [c].[LastName]
FROM [Order] AS [o]
LEFT JOIN [Customer] AS [c] ON ([o].[CustomerFirstName] = [c].[FirstName]) AND ([o].[CustomerLastName] = [c].[LastName])");
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

        private class Customer
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public List<Order> Orders { get; set; }
        }

        private class Order
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Customer Customer { get; set; }
        }

        private class MyContext925 : DbContext
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
                            c => new { c.FirstName, c.LastName });
                        m.HasMany(c => c.Orders).WithOne(o => o.Customer);
                    });

                modelBuilder.Entity<Order>().ToTable("Order");
            }
        }

        #endregion

        #region Bug963

        [ConditionalFact]
        public void Include_on_optional_navigation_One_To_Many_963()
        {
            using (CreateDatabase963())
            {
                using var ctx = new MyContext963(_options);
                ctx.Targaryens.Include(t => t.Dragons).ToList();
            }
        }

        [ConditionalFact]
        public void Include_on_optional_navigation_Many_To_One_963()
        {
            using (CreateDatabase963())
            {
                using var ctx = new MyContext963(_options);
                ctx.Dragons.Include(d => d.Mother).ToList();
            }
        }

        [ConditionalFact]
        public void Include_on_optional_navigation_One_To_One_principal_963()
        {
            using (CreateDatabase963())
            {
                using var ctx = new MyContext963(_options);
                ctx.Targaryens.Include(t => t.Details).ToList();
            }
        }

        [ConditionalFact]
        public void Include_on_optional_navigation_One_To_One_dependent_963()
        {
            using (CreateDatabase963())
            {
                using var ctx = new MyContext963(_options);
                ctx.Details.Include(d => d.Targaryen).ToList();
            }
        }

        [ConditionalFact]
        public void Join_on_optional_navigation_One_To_Many_963()
        {
            using (CreateDatabase963())
            {
                using var ctx = new MyContext963(_options);
                (from t in ctx.Targaryens
                 join d in ctx.Dragons on t.Id equals d.MotherId
                 select d).ToList();
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

        private class Targaryen
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Details Details { get; set; }

            public List<Dragon> Dragons { get; set; }
        }

        private class Dragon
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int? MotherId { get; set; }
            public Targaryen Mother { get; set; }
        }

        private class Details
        {
            public int Id { get; set; }
            public int? TargaryenId { get; set; }
            public Targaryen Targaryen { get; set; }
            public string FullName { get; set; }
        }

        // TODO: replace with GearsOfWar context when it's refactored properly
        private class MyContext963 : DbContext
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

        [ConditionalFact]
        public void Compiler_generated_local_closure_produces_valid_parameter_name_1742()
        {
            Execute1742(
                new CustomerDetails_1742 { FirstName = "Foo", LastName = "Bar" });
        }

        private void Execute1742(CustomerDetails_1742 details)
        {
            using (CreateDatabase925())
            {
                using var ctx = new MyContext925(_options);
                var firstName = details.FirstName;
                ctx.Customers.Where(c => c.FirstName == firstName && c.LastName == details.LastName).ToList();

                // issue #16057
                //                    AssertSql(
                //                        @"@__firstName_0='Foo' (Size = 450)
                //@__8__locals1_details_LastName_1='Bar' (Size = 450)

                //SELECT [c].[FirstName], [c].[LastName]
                //FROM [Customer] AS [c]
                //WHERE (([c].[FirstName] = @__firstName_0) AND @__firstName_0 IS NOT NULL) AND (([c].[LastName] = @__8__locals1_details_LastName_1) AND @__8__locals1_details_LastName_1 IS NOT NULL)");
            }
        }

        private class CustomerDetails_1742
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        #endregion

        #region Bug3758

        [ConditionalFact]
        public void Customer_collections_materialize_properly_3758()
        {
            using (CreateDatabase3758())
            {
                using var ctx = new MyContext3758(_options);
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

        private class MyContext3758 : DbContext
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

        private class Customer3758
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public ICollection<Order3758> Orders1 { get; set; }
            public MyGenericCollection3758<Order3758> Orders2 { get; set; }
            public MyNonGenericCollection3758 Orders3 { get; set; }
            public MyInvalidCollection3758<Order3758> Orders4 { get; set; }
        }

        private class Order3758
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class MyGenericCollection3758<TElement> : List<TElement>
        {
        }

        private class MyNonGenericCollection3758 : List<Order3758>
        {
        }

        private class MyInvalidCollection3758<TElement> : List<TElement>
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

        [ConditionalFact]
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

                    Assert.Single(results);
                    Assert.Equal(1, results[0].ChildCollection.Count);
                    Assert.Equal(2, results[0].ChildCollection.Single().SelfReferenceCollection.Count);
                }

                using (var context = new MyContext3409(_options))
                {
                    var results = context.Children
                        .Select(
                            c => new { c.SelfReferenceBackNavigation, c.SelfReferenceBackNavigation.ParentBackNavigation })
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

        private class MyContext3409 : DbContext
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

        private interface IParent3409
        {
            int Id { get; set; }

            ICollection<IChild3409> ChildCollection { get; set; }
        }

        private interface IChild3409
        {
            int Id { get; set; }

            int? ParentBackNavigationId { get; set; }
            IParent3409 ParentBackNavigation { get; set; }

            ICollection<IChild3409> SelfReferenceCollection { get; set; }
            int? SelfReferenceBackNavigationId { get; set; }
            IChild3409 SelfReferenceBackNavigation { get; set; }
        }

        private class Parent3409 : IParent3409
        {
            public int Id { get; set; }

            public ICollection<IChild3409> ChildCollection { get; set; }
        }

        private class Child3409 : IChild3409
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

        [ConditionalFact]
        public virtual void Repro3101_simple_coalesce1()
        {
            using (CreateDatabase3101())
            {
                using var ctx = new MyContext3101(_options);
                var query = from eVersion in ctx.Entities.Include(e => e.Children)
                            join eRoot in ctx.Entities
                                on eVersion.RootEntityId equals eRoot.Id
                                into RootEntities
                            from eRootJoined in RootEntities.DefaultIfEmpty()
                            select eRootJoined ?? eVersion;

                var result = query.ToList();
                Assert.True(result.All(e => e.Children.Count > 0));
            }
        }

        [ConditionalFact]
        public virtual void Repro3101_simple_coalesce2()
        {
            using (CreateDatabase3101())
            {
                using var ctx = new MyContext3101(_options);
                var query = from eVersion in ctx.Entities
                            join eRoot in ctx.Entities.Include(e => e.Children)
                                on eVersion.RootEntityId equals eRoot.Id
                                into RootEntities
                            from eRootJoined in RootEntities.DefaultIfEmpty()
                            select eRootJoined ?? eVersion;

                var result = query.ToList();
                Assert.Equal(2, result.Count(e => e.Children.Count > 0));
            }
        }

        [ConditionalFact]
        public virtual void Repro3101_simple_coalesce3()
        {
            using (CreateDatabase3101())
            {
                using var ctx = new MyContext3101(_options);
                var query = from eVersion in ctx.Entities.Include(e => e.Children)
                            join eRoot in ctx.Entities.Include(e => e.Children)
                                on eVersion.RootEntityId equals eRoot.Id
                                into RootEntities
                            from eRootJoined in RootEntities.DefaultIfEmpty()
                            select eRootJoined ?? eVersion;

                var result = query.ToList();
                Assert.True(result.All(e => e.Children.Count > 0));
            }
        }

        [ConditionalFact]
        public virtual void Repro3101_complex_coalesce1()
        {
            using (CreateDatabase3101())
            {
                using var ctx = new MyContext3101(_options);
                var query = from eVersion in ctx.Entities.Include(e => e.Children)
                            join eRoot in ctx.Entities
                                on eVersion.RootEntityId equals eRoot.Id
                                into RootEntities
                            from eRootJoined in RootEntities.DefaultIfEmpty()
                            select new { One = 1, Coalesce = eRootJoined ?? eVersion };

                var result = query.ToList();
                Assert.True(result.All(e => e.Coalesce.Children.Count > 0));
            }
        }

        [ConditionalFact]
        public virtual void Repro3101_complex_coalesce2()
        {
            using (CreateDatabase3101())
            {
                using var ctx = new MyContext3101(_options);
                var query = from eVersion in ctx.Entities
                            join eRoot in ctx.Entities.Include(e => e.Children)
                                on eVersion.RootEntityId equals eRoot.Id
                                into RootEntities
                            from eRootJoined in RootEntities.DefaultIfEmpty()
                            select new { Root = eRootJoined, Coalesce = eRootJoined ?? eVersion };

                var result = query.ToList();
                Assert.Equal(2, result.Count(e => e.Coalesce.Children.Count > 0));
            }
        }

        [ConditionalFact]
        public virtual void Repro3101_nested_coalesce1()
        {
            using (CreateDatabase3101())
            {
                using var ctx = new MyContext3101(_options);
                var query = from eVersion in ctx.Entities
                            join eRoot in ctx.Entities.Include(e => e.Children)
                                on eVersion.RootEntityId equals eRoot.Id
                                into RootEntities
                            from eRootJoined in RootEntities.DefaultIfEmpty()
                                // ReSharper disable once ConstantNullCoalescingCondition
                            select new { One = 1, Coalesce = eRootJoined ?? (eVersion ?? eRootJoined) };

                var result = query.ToList();
                Assert.Equal(2, result.Count(e => e.Coalesce.Children.Count > 0));
            }
        }

        [ConditionalFact]
        public virtual void Repro3101_nested_coalesce2()
        {
            using (CreateDatabase3101())
            {
                using var ctx = new MyContext3101(_options);
                var query = from eVersion in ctx.Entities.Include(e => e.Children)
                            join eRoot in ctx.Entities
                                on eVersion.RootEntityId equals eRoot.Id
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

        [ConditionalFact]
        public virtual void Repro3101_conditional()
        {
            using (CreateDatabase3101())
            {
                using var ctx = new MyContext3101(_options);
                var query = from eVersion in ctx.Entities.Include(e => e.Children)
                            join eRoot in ctx.Entities
                                on eVersion.RootEntityId equals eRoot.Id
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

        [ConditionalFact]
        public virtual void Repro3101_coalesce_tracking()
        {
            using (CreateDatabase3101())
            {
                using var ctx = new MyContext3101(_options);
                var query = from eVersion in ctx.Entities
                            join eRoot in ctx.Entities
                                on eVersion.RootEntityId equals eRoot.Id
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

        private class MyContext3101 : DbContext
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

        private class Entity3101
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

        private class Child3101
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion

        #region Bug6986

        [ConditionalFact]
        public virtual void Repro6986_can_query_base_type_when_derived_types_contain_shadow_properties()
        {
            using (CreateDatabase6986())
            {
                using var context = new ReproContext6986(_options);
                var query = context.Contacts.ToList();

                Assert.Equal(4, query.Count);
                Assert.Equal(2, query.OfType<EmployerContact6986>().Count());
                Assert.Single(query.OfType<ServiceOperatorContact6986>());
            }
        }

        [ConditionalFact]
        public virtual void Repro6986_can_include_dependent_to_principal_navigation_of_derived_type_with_shadow_fk()
        {
            using (CreateDatabase6986())
            {
                using var context = new ReproContext6986(_options);
                var query = context.Contacts.OfType<ServiceOperatorContact6986>().Include(e => e.ServiceOperator6986).ToList();

                Assert.Single(query);
                Assert.NotNull(query[0].ServiceOperator6986);
            }
        }

        [ConditionalFact]
        public virtual void Repro6986_can_project_shadow_property_using_ef_property()
        {
            using (CreateDatabase6986())
            {
                using var context = new ReproContext6986(_options);
                var query = context.Contacts.OfType<ServiceOperatorContact6986>().Select(
                    c => new { c, Prop = EF.Property<int>(c, "ServiceOperator6986Id") }).ToList();

                Assert.Single(query);
                Assert.Equal(1, query[0].Prop);
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
                        new Contact6986 { UserName = "noroles@esoterix.co.uk" });
                    context.SaveChanges();
                });
        }

        private class ReproContext6986 : DbContext
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

        private class EmployerContact6986 : Contact6986
        {
            [Required]
            public Employer6986 Employer6986 { get; set; }
        }

        private class ServiceOperatorContact6986 : Contact6986
        {
            [Required]
            public ServiceOperator6986 ServiceOperator6986 { get; set; }
        }

        private class Contact6986
        {
            public int Id { get; set; }
            public string UserName { get; set; }
            public bool IsPrimary { get; set; }
        }

        private class Employer6986
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<EmployerContact6986> Contacts { get; set; }
        }

        private class ServiceOperator6986
        {
            public int Id { get; set; }
            public List<ServiceOperatorContact6986> Contacts { get; set; }
        }

        #endregion

        #region Bug5456

        [ConditionalFact]
        public virtual void Repro5456_include_group_join_is_per_query_context()
        {
            using (CreateDatabase5456())
            {
                Parallel.For(
                    0, 10, i =>
                    {
                        using var ctx = new MyContext5456(_options);
                        var result = ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).ToList();

                        Assert.Equal(198, result.Count);
                    });
            }
        }

        [ConditionalFact]
        public virtual async Task Repro5456_include_group_join_is_per_query_context_async()
        {
            using (CreateDatabase5456())
            {
                await Task.WhenAll(
                    Enumerable.Range(0, 10)
                        .Select(
                            async i =>
                            {
                                using var ctx = new MyContext5456(_options);
                                var result = await ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).ToListAsync();

                                Assert.Equal(198, result.Count);
                            }));
            }
        }

        [ConditionalFact]
        public virtual void Repro5456_multiple_include_group_join_is_per_query_context()
        {
            using (CreateDatabase5456())
            {
                Parallel.For(
                    0, 10, i =>
                    {
                        using var ctx = new MyContext5456(_options);
                        var result = ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).Include(x => x.Comments).ToList();

                        Assert.Equal(198, result.Count);
                    });
            }
        }

        [ConditionalFact]
        public virtual async Task Repro5456_multiple_include_group_join_is_per_query_context_async()
        {
            using (CreateDatabase5456())
            {
                await Task.WhenAll(
                    Enumerable.Range(0, 10)
                        .Select(
                            async i =>
                            {
                                using var ctx = new MyContext5456(_options);
                                var result = await ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).Include(x => x.Comments)
                                    .ToListAsync();

                                Assert.Equal(198, result.Count);
                            }));
            }
        }

        [ConditionalFact]
        public virtual void Repro5456_multi_level_include_group_join_is_per_query_context()
        {
            using (CreateDatabase5456())
            {
                Parallel.For(
                    0, 10, i =>
                    {
                        using var ctx = new MyContext5456(_options);
                        var result = ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).ThenInclude(b => b.Author).ToList();

                        Assert.Equal(198, result.Count);
                    });
            }
        }

        [ConditionalFact]
        public virtual async Task Repro5456_multi_level_include_group_join_is_per_query_context_async()
        {
            using (CreateDatabase5456())
            {
                await Task.WhenAll(
                    Enumerable.Range(0, 10)
                        .Select(
                            async i =>
                            {
                                using var ctx = new MyContext5456(_options);
                                var result = await ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).ThenInclude(b => b.Author)
                                    .ToListAsync();

                                Assert.Equal(198, result.Count);
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
                                    new Post5456 { Comments = new List<Comment5456> { new Comment5456(), new Comment5456() } },
                                    new Post5456()
                                },
                                Author = new Author5456()
                            });
                    }

                    context.SaveChanges();
                });
        }

        private class MyContext5456 : DbContext
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

        private class Blog5456
        {
            public int Id { get; set; }
            public List<Post5456> Posts { get; set; }
            public Author5456 Author { get; set; }
        }

        private class Author5456
        {
            public int Id { get; set; }
            public List<Blog5456> Blogs { get; set; }
        }

        private class Post5456
        {
            public int Id { get; set; }
            public Blog5456 Blog { get; set; }
            public List<Comment5456> Comments { get; set; }
        }

        private class Comment5456
        {
            public int Id { get; set; }
            public Post5456 Blog { get; set; }
        }

        #endregion

        #region Bug7359

        [ConditionalFact]
        public virtual void Discriminator_type_is_handled_correctly_in_materialization_bug_7359()
        {
            using (CreateDatabase7359())
            {
                using var ctx = new MyContext7359(_options);
                var query = ctx.Products.OfType<SpecialProduct>().ToList();

                Assert.Single(query);
            }
        }

        [ConditionalFact]
        public virtual void Discriminator_type_is_handled_correctly_with_is_operator_bug_7359()
        {
            using (CreateDatabase7359())
            {
                using var ctx = new MyContext7359(_options);
                var query = ctx.Products.Where(p => p is SpecialProduct).ToList();

                Assert.Single(query);
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
                        new Product { Name = "Product1" });
                    context.Add(
                        new SpecialProduct { Name = "SpecialProduct" });
                    context.SaveChanges();
                });
        }

        #endregion

        #region Bug7312

        [ConditionalFact]
        public virtual void Reference_include_on_derived_type_with_sibling_works_bug_7312()
        {
            using (CreateDatabase7312())
            {
                using var context = new MyContext7312(_options);
                var query = context.Proposal.OfType<ProposalLeave7312>().Include(l => l.LeaveType).ToList();

                Assert.Single(query);
            }
        }

        private class Proposal7312
        {
            public int Id { get; set; }
        }

        private class ProposalCustom7312 : Proposal7312
        {
            public string Name { get; set; }
        }

        private class ProposalLeave7312 : Proposal7312
        {
            public DateTime LeaveStart { get; set; }
            public virtual ProposalLeaveType7312 LeaveType { get; set; }
        }

        private class ProposalLeaveType7312
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
                        new ProposalCustom7312 { Name = "CustomProposal" },
                        new ProposalLeave7312 { LeaveStart = DateTime.Now, LeaveType = new ProposalLeaveType7312() }
                    );
                    context.SaveChanges();
                });
        }

        #endregion

        #region Bug8282

        [ConditionalFact]
        public virtual void Entity_passed_to_DTO_constructor_works()
        {
            using (CreateDatabase8282())
            {
                using var context = new MyContext8282(_options);
                var query = context.Entity.Select(e => new EntityDto8282(e)).ToList();

                Assert.Single(query);
            }
        }

        private class Entity8282
        {
            public int Id { get; set; }
        }

        private class EntityDto8282
        {
            public EntityDto8282(Entity8282 entity)
            {
                Id = entity.Id;
            }

            public int Id { get; }
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

        [ConditionalFact]
        public virtual void Enum_has_flag_applies_explicit_cast_for_long_constant()
        {
            using (CreateDatabase8538())
            {
                using var context = new MyContext8538(_options);
                var query = context.Entity.Where(e => e.Permission.HasFlag(Permission.READ_WRITE)).ToList();

                Assert.Single(query);

                AssertSql(
                    @"SELECT [e].[Id], [e].[Permission], [e].[PermissionByte], [e].[PermissionShort]
FROM [Entity] AS [e]
WHERE ([e].[Permission] & CAST(17179869184 AS bigint)) = CAST(17179869184 AS bigint)");
            }
        }

        [ConditionalFact]
        public virtual void Enum_has_flag_does_not_apply_explicit_cast_for_non_constant()
        {
            using (CreateDatabase8538())
            {
                using var context = new MyContext8538(_options);
                var query = context.Entity.Where(e => e.Permission.HasFlag(e.Permission)).ToList();

                Assert.Equal(3, query.Count);

                AssertSql(
                    @"SELECT [e].[Id], [e].[Permission], [e].[PermissionByte], [e].[PermissionShort]
FROM [Entity] AS [e]
WHERE ([e].[Permission] & [e].[Permission]) = [e].[Permission]");
            }
        }

        [ConditionalFact]
        public virtual void Byte_enum_has_flag_does_not_apply_explicit_cast_for_non_constant()
        {
            using (CreateDatabase8538())
            {
                using var context = new MyContext8538(_options);
                var query = context.Entity.Where(e => e.PermissionByte.HasFlag(e.PermissionByte)).ToList();

                Assert.Equal(3, query.Count);

                AssertSql(
                    @"SELECT [e].[Id], [e].[Permission], [e].[PermissionByte], [e].[PermissionShort]
FROM [Entity] AS [e]
WHERE ([e].[PermissionByte] & [e].[PermissionByte]) = [e].[PermissionByte]");
            }
        }

        [ConditionalFact]
        public virtual void Enum_has_flag_applies_explicit_cast_for_short_constant()
        {
            using (CreateDatabase8538())
            {
                using var context = new MyContext8538(_options);
                var query = context.Entity.Where(e => e.PermissionShort.HasFlag(PermissionShort.READ_WRITE)).ToList();

                Assert.Single(query);

                AssertSql(
                    @"SELECT [e].[Id], [e].[Permission], [e].[PermissionByte], [e].[PermissionShort]
FROM [Entity] AS [e]
WHERE ([e].[PermissionShort] & CAST(4 AS smallint)) = CAST(4 AS smallint)");
            }
        }

        private class Entity8538
        {
            public int Id { get; set; }
            public Permission Permission { get; set; }
            public PermissionByte PermissionByte { get; set; }
            public PermissionShort PermissionShort { get; set; }
        }

        [Flags]
#pragma warning disable CA2217 // Do not mark enums with FlagsAttribute
        private enum PermissionByte : byte
#pragma warning restore CA2217 // Do not mark enums with FlagsAttribute
        {
            NONE = 1,
            READ_ONLY = 2,
            READ_WRITE = 4
        }

        [Flags]
#pragma warning disable CA2217 // Do not mark enums with FlagsAttribute
        private enum PermissionShort : short
#pragma warning restore CA2217 // Do not mark enums with FlagsAttribute
        {
            NONE = 1,
            READ_ONLY = 2,
            READ_WRITE = 4
        }

        [Flags]
#pragma warning disable CA2217 // Do not mark enums with FlagsAttribute
        private enum Permission : long
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

        [ConditionalFact]
        public virtual void Variable_from_closure_is_parametrized()
        {
            using (CreateDatabase8909())
            {
                using var context = new MyContext8909(_options);
                context.Cache.Compact(1);

                var id = 1;
                context.Entities.Where(c => c.Id == id).ToList();
                Assert.Equal(2, context.Cache.Count);

                id = 2;
                context.Entities.Where(c => c.Id == id).ToList();
                Assert.Equal(2, context.Cache.Count);

                AssertSql(
                    @"@__id_0='1'

SELECT [e].[Id], [e].[Name]
FROM [Entities] AS [e]
WHERE [e].[Id] = @__id_0",
                    //
                    @"@__id_0='2'

SELECT [e].[Id], [e].[Name]
FROM [Entities] AS [e]
WHERE [e].[Id] = @__id_0");
            }
        }

        [ConditionalFact]
        public virtual void Variable_from_nested_closure_is_parametrized()
        {
            using (CreateDatabase8909())
            {
                using var context = new MyContext8909(_options);
                context.Cache.Compact(1);

                var id = 0;
                // ReSharper disable once AccessToModifiedClosure
                Expression<Func<Entity8909, bool>> whereExpression = c => c.Id == id;

                id = 1;
                context.Entities.Where(whereExpression).ToList();
                Assert.Equal(2, context.Cache.Count);

                id = 2;
                context.Entities.Where(whereExpression).ToList();
                Assert.Equal(2, context.Cache.Count);

                AssertSql(
                    @"@__id_0='1'

SELECT [e].[Id], [e].[Name]
FROM [Entities] AS [e]
WHERE [e].[Id] = @__id_0",
                    //
                    @"@__id_0='2'

SELECT [e].[Id], [e].[Name]
FROM [Entities] AS [e]
WHERE [e].[Id] = @__id_0");
            }
        }

        [ConditionalFact]
        public virtual void Variable_from_multi_level_nested_closure_is_parametrized()
        {
            using (CreateDatabase8909())
            {
                using var context = new MyContext8909(_options);
                context.Cache.Compact(1);

                var id = 0;
                // ReSharper disable once AccessToModifiedClosure
                Expression<Func<Entity8909, bool>> whereExpression = c => c.Id == id;
                Expression<Func<Entity8909, bool>> containsExpression =
                    c => context.Entities.Where(whereExpression).Select(e => e.Id).Contains(c.Id);

                id = 1;
                context.Entities.Where(containsExpression).ToList();
                Assert.Equal(2, context.Cache.Count);

                id = 2;
                context.Entities.Where(containsExpression).ToList();
                Assert.Equal(2, context.Cache.Count);

                AssertSql(
                    @"@__id_0='1'

SELECT [e].[Id], [e].[Name]
FROM [Entities] AS [e]
WHERE [e].[Id] IN (
    SELECT [e0].[Id]
    FROM [Entities] AS [e0]
    WHERE [e0].[Id] = @__id_0
)",
                    //
                    @"@__id_0='2'

SELECT [e].[Id], [e].[Name]
FROM [Entities] AS [e]
WHERE [e].[Id] IN (
    SELECT [e0].[Id]
    FROM [Entities] AS [e0]
    WHERE [e0].[Id] = @__id_0
)");
            }
        }

        [ConditionalFact]
        public virtual void Relational_command_cache_creates_new_entry_when_parameter_nullability_changes()
        {
            using (CreateDatabase8909())
            {
                using var context = new MyContext8909(_options);
                context.Cache.Compact(1);

                var name = "A";

                context.Entities.Where(e => e.Name == name).ToList();
                Assert.Equal(2, context.Cache.Count);

                name = null;
                context.Entities.Where(e => e.Name == name).ToList();
                Assert.Equal(3, context.Cache.Count);

                AssertSql(
                    @"@__name_0='A' (Size = 4000)

SELECT [e].[Id], [e].[Name]
FROM [Entities] AS [e]
WHERE [e].[Name] = @__name_0",
                    //
                    @"SELECT [e].[Id], [e].[Name]
FROM [Entities] AS [e]
WHERE [e].[Name] IS NULL");
            }
        }

        [ConditionalFact]
        public virtual void Query_cache_entries_are_evicted_as_necessary()
        {
            using (CreateDatabase8909())
            {
                using var context = new MyContext8909(_options);
                context.Cache.Compact(1);
                Assert.Equal(0, context.Cache.Count);

                var entityParam = Expression.Parameter(typeof(Entity8909), "e");
                var idPropertyInfo = context.Model.FindEntityType((typeof(Entity8909)))
                    .FindProperty(nameof(Entity8909.Id))
                    .PropertyInfo;
                for (var i = 0; i < 1100; i++)
                {
                    var conditionBody = Expression.Equal(
                        Expression.MakeMemberAccess(entityParam, idPropertyInfo),
                        Expression.Constant(i));
                    var whereExpression = Expression.Lambda<Func<Entity8909, bool>>(conditionBody, entityParam);
                    context.Entities.Where(whereExpression).GetEnumerator();
                }

                Assert.True(context.Cache.Count <= 1024);
            }
        }

        private SqlServerTestStore CreateDatabase8909()
        {
            return CreateTestStore(
                () => new MyContext8909(_options),
                context => ClearLog());
        }

        private class MyContext8909 : DbContext
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

                    return (MemoryCache)typeof(CompiledQueryCache)
                        .GetField("_memoryCache", BindingFlags.Instance | BindingFlags.NonPublic)
                        ?.GetValue(compiledQueryCache);
                }
            }
        }

        private class Entity8909
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion

        #region Bug9202/9210

        [ConditionalFact]
        public void Include_collection_for_entity_with_owned_type_works()
        {
            using (CreateDatabase9202())
            {
                using var context = new MyContext9202(_options);
                var query = context.Movies.Include(m => m.Cast);
                var result = query.ToList();

                Assert.Single(result);
                Assert.Equal(3, result[0].Cast.Count);
                Assert.NotNull(result[0].Details);
                Assert.True(result[0].Cast.All(a => a.Details != null));

                AssertSql(
                    @"SELECT [m].[Id], [m].[Title], [t].[Id], [t].[Details_Info], [t1].[Id], [t1].[Movie9202Id], [t1].[Name], [t1].[Id0], [t1].[Details_Info]
FROM [Movies] AS [m]
LEFT JOIN (
    SELECT [m0].[Id], [m0].[Details_Info], [m1].[Id] AS [Id0]
    FROM [Movies] AS [m0]
    INNER JOIN [Movies] AS [m1] ON [m0].[Id] = [m1].[Id]
    WHERE [m0].[Details_Info] IS NOT NULL
) AS [t] ON [m].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [a].[Id], [a].[Movie9202Id], [a].[Name], [t0].[Id] AS [Id0], [t0].[Details_Info]
    FROM [Actors] AS [a]
    LEFT JOIN (
        SELECT [a0].[Id], [a0].[Details_Info], [a1].[Id] AS [Id0]
        FROM [Actors] AS [a0]
        INNER JOIN [Actors] AS [a1] ON [a0].[Id] = [a1].[Id]
        WHERE [a0].[Details_Info] IS NOT NULL
    ) AS [t0] ON [a].[Id] = [t0].[Id]
) AS [t1] ON [m].[Id] = [t1].[Movie9202Id]
ORDER BY [m].[Id], [t1].[Id]");
            }
        }

        [ConditionalFact]
        public void Include_collection_for_entity_with_owned_type_works_string()
        {
            using (CreateDatabase9202())
            {
                using var context = new MyContext9202(_options);
                var query = context.Movies.Include("Cast");
                var result = query.ToList();

                Assert.Single(result);
                Assert.Equal(3, result[0].Cast.Count);
                Assert.NotNull(result[0].Details);
                Assert.True(result[0].Cast.All(a => a.Details != null));

                AssertSql(
                    @"SELECT [m].[Id], [m].[Title], [t].[Id], [t].[Details_Info], [t1].[Id], [t1].[Movie9202Id], [t1].[Name], [t1].[Id0], [t1].[Details_Info]
FROM [Movies] AS [m]
LEFT JOIN (
    SELECT [m0].[Id], [m0].[Details_Info], [m1].[Id] AS [Id0]
    FROM [Movies] AS [m0]
    INNER JOIN [Movies] AS [m1] ON [m0].[Id] = [m1].[Id]
    WHERE [m0].[Details_Info] IS NOT NULL
) AS [t] ON [m].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [a].[Id], [a].[Movie9202Id], [a].[Name], [t0].[Id] AS [Id0], [t0].[Details_Info]
    FROM [Actors] AS [a]
    LEFT JOIN (
        SELECT [a0].[Id], [a0].[Details_Info], [a1].[Id] AS [Id0]
        FROM [Actors] AS [a0]
        INNER JOIN [Actors] AS [a1] ON [a0].[Id] = [a1].[Id]
        WHERE [a0].[Details_Info] IS NOT NULL
    ) AS [t0] ON [a].[Id] = [t0].[Id]
) AS [t1] ON [m].[Id] = [t1].[Movie9202Id]
ORDER BY [m].[Id], [t1].[Id]");
            }
        }

        private SqlServerTestStore CreateDatabase9202()
        {
            return CreateTestStore(
                () => new MyContext9202(_options),
                context =>
                {
                    var av = new Actor9202 { Name = "Alicia Vikander", Details = new Details9202 { Info = "Best actor ever made" } };
                    var oi = new Actor9202 { Name = "Oscar Isaac", Details = new Details9202 { Info = "Best actor ever made" } };
                    var dg = new Actor9202 { Name = "Domhnall Gleeson", Details = new Details9202 { Info = "Best actor ever made" } };
                    var em = new Movie9202
                    {
                        Title = "Ex Machina",
                        Cast = new List<Actor9202>
                        {
                            av,
                            oi,
                            dg
                        },
                        Details = new Details9202 { Info = "Best movie ever made" }
                    };
                    context.Actors.AddRange(av, oi, dg);
                    context.Movies.Add(em);
                    context.SaveChanges();

                    ClearLog();
                });
        }

        private class MyContext9202 : DbContext
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

        private class Movie9202
        {
            public int Id { get; set; }
            public string Title { get; set; }

            public List<Actor9202> Cast { get; set; }

            public Details9202 Details { get; set; }
        }

        private class Actor9202
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Details9202 Details { get; set; }
        }

        private class Details9202
        {
            public string Info { get; set; }
        }

        #endregion

        #region Bug9214

        [ConditionalFact]
        public void Default_schema_applied_when_no_function_schema()
        {
            using (CreateDatabase9214())
            {
                using var context = new MyContext9214(_options);
                var result = context.Widgets.Where(w => w.Val == 1).Select(w => MyContext9214.AddOne(w.Val)).Single();

                Assert.Equal(2, result);

                AssertSql(
                    @"SELECT TOP(2) [foo].[AddOne]([w].[Val])
FROM [foo].[Widgets] AS [w]
WHERE [w].[Val] = 1");
            }
        }

        [ConditionalFact]
        public void Default_schema_function_schema_overrides()
        {
            using (CreateDatabase9214())
            {
                using var context = new MyContext9214(_options);
                var result = context.Widgets.Where(w => w.Val == 1).Select(w => MyContext9214.AddTwo(w.Val)).Single();

                Assert.Equal(3, result);

                AssertSql(
                    @"SELECT TOP(2) [dbo].[AddTwo]([w].[Val])
FROM [foo].[Widgets] AS [w]
WHERE [w].[Val] = 1");
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

                    context.Database.ExecuteSqlRaw(
                        @"CREATE FUNCTION foo.AddOne (@num int)
                                                            RETURNS int
                                                                AS
                                                            BEGIN
                                                                return @num + 1 ;
                                                            END");

                    context.Database.ExecuteSqlRaw(
                        @"CREATE FUNCTION dbo.AddTwo (@num int)
                                                            RETURNS int
                                                                AS
                                                            BEGIN
                                                                return @num + 2 ;
                                                            END");

                    ClearLog();
                });
        }

        private class MyContext9214 : DbContext
        {
            public DbSet<Widget9214> Widgets { get; set; }

#pragma warning disable IDE0060 // Remove unused parameter
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
#pragma warning restore IDE0060 // Remove unused parameter

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

        private class Widget9214
        {
            public int Id { get; set; }
            public int Val { get; set; }
        }

        #endregion

        #region Bug9277

        [ConditionalFact]
        public virtual void From_sql_gets_value_of_out_parameter_in_stored_procedure()
        {
            using (CreateDatabase9277())
            {
                using var context = new MyContext9277(_options);
                var valueParam = new SqlParameter
                {
                    ParameterName = "Value",
                    Value = 0,
                    Direction = ParameterDirection.Output,
                    SqlDbType = SqlDbType.Int
                };

                Assert.Equal(0, valueParam.Value);

                var blogs = context.Blogs.FromSqlRaw(
                        "[dbo].[GetPersonAndVoteCount]  @id, @Value out",
                        new SqlParameter { ParameterName = "id", Value = 1 },
                        valueParam)
                    .ToList();

                Assert.Single(blogs);
                Assert.Equal(1, valueParam.Value);
            }
        }

        private SqlServerTestStore CreateDatabase9277()
        {
            return CreateTestStore(
                () => new MyContext9277(_options),
                context =>
                {
                    context.Database.ExecuteSqlRaw(
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

        private class MyContext9277 : DbContext
        {
            public MyContext9277(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Blog9277> Blogs { get; set; }
        }

        private class Blog9277
        {
            public int Id { get; set; }
            public int SomeValue { get; set; }
        }

        #endregion

        #region Bug9038

        [ConditionalFact]
        public virtual async Task Include_collection_optional_reference_collection_9038()
        {
            using (CreateDatabase9038())
            {
                using var context = new MyContext9038(_options);
                var result = await context.People.OfType<PersonTeacher9038>()
                    .Include(m => m.Students)
                    .ThenInclude(m => m.Family)
                    .ThenInclude(m => m.Members)
                    .ToListAsync();

                Assert.Equal(2, result.Count);
                Assert.True(result.All(r => r.Students.Count > 0));
            }
        }

        [ConditionalFact]
        public async Task Include_optional_reference_collection_another_collection()
        {
            using (CreateDatabase9038())
            {
                using var context = new MyContext9038(_options);
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

        private abstract class Person9038
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public int? TeacherId { get; set; }

            public PersonFamily9038 Family { get; set; }
        }

        private class PersonKid9038 : Person9038
        {
            public int Grade { get; set; }

            public PersonTeacher9038 Teacher { get; set; }
        }

        private class PersonTeacher9038 : Person9038
        {
            public ICollection<PersonKid9038> Students { get; set; }
        }

        private class PersonFamily9038
        {
            public int Id { get; set; }

            public string LastName { get; set; }

            public ICollection<Person9038> Members { get; set; }
        }

        private class MyContext9038 : DbContext
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
                        new PersonFamily9038 { LastName = "Garrison" }, new PersonFamily9038 { LastName = "Cartman" }
                    };
                    var teachers = new List<PersonTeacher9038>
                    {
                        new PersonTeacher9038 { Name = "Ms. Frizzle" },
                        new PersonTeacher9038 { Name = "Mr. Garrison", Family = famalies[0] }
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

        [ConditionalFact]
        // TODO: Convert to test in IncludeTestBase once issue #9742 is fixed
        public virtual void Repro9735()
        {
            using (CreateDatabase9735())
            {
                using var context = new MyContext9735(_options);
                var result = context.Customers
                    .Include(b => b.Orders)
                    .OrderBy(b => b.Address.Id > 0)
                    .ThenBy(b => b.CustomerDetails != null ? b.CustomerDetails.Name : string.Empty)
                    .Take(2)
                    .ToList();

                Assert.Single(result);

                AssertSql(
                    @"@__p_0='2'

SELECT [t].[Id], [t].[AddressId], [t].[CustomerDetailsId], [t].[Name], [t].[Id0], [o].[Id], [o].[CustomerId], [o].[Name]
FROM (
    SELECT TOP(@__p_0) [c].[Id], [c].[AddressId], [c].[CustomerDetailsId], [c].[Name], [a].[Id] AS [Id0], CASE
        WHEN [a].[Id] > 0 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [c], CASE
        WHEN [c0].[Id] IS NOT NULL THEN [c0].[Name]
        ELSE N''
    END AS [c0]
    FROM [Customers] AS [c]
    INNER JOIN [Address9735] AS [a] ON [c].[AddressId] = [a].[Id]
    LEFT JOIN [CustomerDetails9735] AS [c0] ON [c].[CustomerDetailsId] = [c0].[Id]
    ORDER BY CASE
        WHEN [a].[Id] > 0 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END, CASE
        WHEN [c0].[Id] IS NOT NULL THEN [c0].[Name]
        ELSE N''
    END
) AS [t]
LEFT JOIN [Order9735] AS [o] ON [t].[Id] = [o].[CustomerId]
ORDER BY [t].[c], [t].[c0], [t].[Id], [t].[Id0], [o].[Id]");
            }
        }

        private class Address9735
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class Customer9735
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int AddressId { get; set; }
            public virtual Address9735 Address { get; set; }
            public virtual List<Order9735> Orders { get; set; }
            public virtual CustomerDetails9735 CustomerDetails { get; set; }
        }

        private class Order9735
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int CustomerId { get; set; }
            public virtual Customer9735 Customer { get; set; }
        }

        private class CustomerDetails9735
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class MyContext9735 : DbContext
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

        #region Bug9468

        [ConditionalFact]
        public virtual void Conditional_expression_with_conditions_does_not_collapse_if_nullable_bool()
        {
            using (CreateDatabase9468())
            {
                using var context = new MyContext9468(_options);
                var query = context.Carts.Select(
                    t => new { Processing = t.Configuration != null ? !t.Configuration.Processed : (bool?)null }).ToList();

                Assert.Single(query.Where(t => t.Processing == null));
                Assert.Single(query.Where(t => t.Processing == true));
                Assert.Single(query.Where(t => t.Processing == false));

                AssertSql(
                    @"SELECT CASE
    WHEN [c0].[Id] IS NOT NULL THEN CASE
        WHEN [c0].[Processed] <> CAST(1 AS bit) THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    ELSE NULL
END AS [Processing]
FROM [Carts] AS [c]
LEFT JOIN [Configuration9468] AS [c0] ON [c].[ConfigurationId] = [c0].[Id]");
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
                        new Cart9468 { Configuration = new Configuration9468 { Processed = true } },
                        new Cart9468 { Configuration = new Configuration9468() }
                    );

                    context.SaveChanges();

                    ClearLog();
                });
        }

        private class MyContext9468 : DbContext
        {
            public MyContext9468(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Cart9468> Carts { get; set; }
        }

        private class Cart9468
        {
            public int Id { get; set; }
            public int? ConfigurationId { get; set; }
            public Configuration9468 Configuration { get; set; }
        }

        private class Configuration9468
        {
            public int Id { get; set; }
            public bool Processed { get; set; }
        }

        #endregion

        #region Bug10635

        [ConditionalFact]
        public void Include_with_order_by_on_interface_key()
        {
            using (CreateDatabase10635())
            {
                using var context = new MyContext10635(_options);
                var query = context.Parents.Include(p => p.Children).OrderBy(p => p.Id).ToList();

                AssertSql(
                    @"SELECT [p].[Id], [p].[Name], [c].[Id], [c].[Name], [c].[Parent10635Id], [c].[ParentId]
FROM [Parents] AS [p]
LEFT JOIN [Children] AS [c] ON [p].[Id] = [c].[Parent10635Id]
ORDER BY [p].[Id], [c].[Id]");
            }
        }

        [ConditionalFact]
        public void Correlated_collection_with_order_by_on_interface_key()
        {
            using (CreateDatabase10635())
            {
                using var context = new MyContext10635(_options);
                var query = context.Parents.OrderBy(p => p.Id).Select(p => p.Children.ToList()).ToList();

                AssertSql(
                    @"SELECT [p].[Id], [c].[Id], [c].[Name], [c].[Parent10635Id], [c].[ParentId]
FROM [Parents] AS [p]
LEFT JOIN [Children] AS [c] ON [p].[Id] = [c].[Parent10635Id]
ORDER BY [p].[Id], [c].[Id]");
            }
        }

        private SqlServerTestStore CreateDatabase10635()
        {
            return CreateTestStore(
                () => new MyContext10635(_options),
                context =>
                {
                    var c11 = new Child10635 { Name = "Child111" };
                    var c12 = new Child10635 { Name = "Child112" };
                    var c13 = new Child10635 { Name = "Child113" };
                    var c21 = new Child10635 { Name = "Child121" };

                    var p1 = new Parent10635 { Name = "Parent1", Children = new[] { c11, c12, c13 } };
                    var p2 = new Parent10635 { Name = "Parent2", Children = new[] { c21 } };
                    context.Parents.AddRange(p1, p2);
                    context.Children.AddRange(c11, c12, c13, c21);
                    context.SaveChanges();

                    ClearLog();
                });
        }

        private class MyContext10635 : DbContext
        {
            public MyContext10635(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Parent10635> Parents { get; set; }
            public DbSet<Child10635> Children { get; set; }
        }

        private interface IEntity10635
        {
            int Id { get; set; }
        }

        private class Parent10635 : IEntity10635
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public virtual ICollection<Child10635> Children { get; set; }
        }

        private class Child10635 : IEntity10635
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int ParentId { get; set; }
        }

        #endregion

        #region Bug10301

        [ConditionalFact]
        public virtual void MultiContext_query_filter_test()
        {
            using (CreateDatabase10301())
            {
                using var context = new FilterContext10301(_options);
                Assert.Empty(context.Blogs.ToList());

                context.Tenant = 1;
                Assert.Single(context.Blogs.ToList());

                context.Tenant = 2;
                Assert.Equal(2, context.Blogs.Count());

                AssertSql(
                    @"@__ef_filter__Tenant_0='0'

SELECT [b].[Id], [b].[SomeValue]
FROM [Blogs] AS [b]
WHERE [b].[SomeValue] = @__ef_filter__Tenant_0",
                    //
                    @"@__ef_filter__Tenant_0='1'

SELECT [b].[Id], [b].[SomeValue]
FROM [Blogs] AS [b]
WHERE [b].[SomeValue] = @__ef_filter__Tenant_0",
                    //
                    @"@__ef_filter__Tenant_0='2'

SELECT COUNT(*)
FROM [Blogs] AS [b]
WHERE [b].[SomeValue] = @__ef_filter__Tenant_0");
            }
        }

        private SqlServerTestStore CreateDatabase10301()
        {
            return CreateTestStore(
                () => new FilterContext10301(_options),
                context =>
                {
                    context.AddRange(
                        new Blog10301 { SomeValue = 1 },
                        new Blog10301 { SomeValue = 2 },
                        new Blog10301 { SomeValue = 2 }
                    );

                    context.SaveChanges();

                    ClearLog();
                });
        }

        private class FilterContextBase10301 : DbContext
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

        private class Blog10301
        {
            public int Id { get; set; }
            public int SomeValue { get; set; }
        }

        private class FilterContext10301 : FilterContextBase10301
        {
            public FilterContext10301(DbContextOptions options)
                : base(options)
            {
            }
        }

        #endregion

        #region Bug11104

        [ConditionalFact]
        public virtual void QueryBuffer_requirement_is_computed_when_querying_base_type_while_derived_type_has_shadow_prop()
        {
            using (CreateDatabase11104())
            {
                using var context = new MyContext11104(_options);
                var query = context.Bases.ToList();

                var derived1 = Assert.Single(query);
                Assert.Equal(typeof(Derived1), derived1.GetType());

                AssertSql(
                    @"SELECT [b].[Id], [b].[IsTwo], [b].[MoreStuffId]
FROM [Bases] AS [b]
WHERE [b].[IsTwo] IN (CAST(0 AS bit), CAST(1 AS bit))");
            }
        }

        private SqlServerTestStore CreateDatabase11104()
        {
            return CreateTestStore(
                () => new MyContext11104(_options),
                context =>
                {
                    context.AddRange(
                        new Derived1 { IsTwo = false }
                    );

                    context.SaveChanges();

                    ClearLog();
                });
        }

        private class MyContext11104 : DbContext
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

        private abstract class Base
        {
            public int Id { get; set; }
            public bool IsTwo { get; set; }
        }

        private class Derived1 : Base
        {
            public Stuff MoreStuff { get; set; }
        }

        private class Derived2 : Base
        {
        }

        private class Stuff
        {
            public int Id { get; set; }
        }

        #endregion

        #region Bug11818_11831

        [ConditionalFact]
        public virtual void GroupJoin_Anonymous_projection_GroupBy_Aggregate_join_elimination()
        {
            using (CreateDatabase11818())
            {
                using var context = new MyContext11818(_options);
                var query = (from e in context.Set<Entity11818>()
                             join a in context.Set<AnotherEntity11818>()
                                 on e.Id equals a.Id into grouping
                             from a in grouping.DefaultIfEmpty()
                             select new { ename = e.Name, aname = a.Name })
                    .GroupBy(g => g.aname)
                    .Select(
                        g => new { g.Key, cnt = g.Count() + 5 })
                    .ToList();

                AssertSql(
                    @"SELECT [t2].[Name] AS [Key], COUNT(*) + 5 AS [cnt]
FROM [Table] AS [t]
LEFT JOIN (
    SELECT [t0].[Id], [t0].[Name], [t1].[Id] AS [Id0]
    FROM [Table] AS [t0]
    INNER JOIN [Table] AS [t1] ON [t0].[Id] = [t1].[Id]
    WHERE [t0].[Name] IS NOT NULL
) AS [t2] ON [t].[Id] = [t2].[Id]
GROUP BY [t2].[Name]");
            }
        }

        [ConditionalFact]
        public virtual void GroupJoin_Anonymous_projection_GroupBy_Aggregate_join_elimination_2()
        {
            using (CreateDatabase11818())
            {
                using var context = new MyContext11818(_options);
                var query = (from e in context.Set<Entity11818>()
                             join a in context.Set<AnotherEntity11818>()
                                 on e.Id equals a.Id into grouping
                             from a in grouping.DefaultIfEmpty()
                             join m in context.Set<MaumarEntity11818>()
                                 on e.Id equals m.Id into grouping2
                             from m in grouping2.DefaultIfEmpty()
                             select new { aname = a.Name, mname = m.Name })
                    .GroupBy(
                        g => new { g.aname, g.mname })
                    .Select(
                        g => new { MyKey = g.Key.aname, cnt = g.Count() + 5 })
                    .ToList();

                AssertSql(
                    @"SELECT [t2].[Name] AS [MyKey], COUNT(*) + 5 AS [cnt]
FROM [Table] AS [t]
LEFT JOIN (
    SELECT [t0].[Id], [t0].[Name], [t1].[Id] AS [Id0]
    FROM [Table] AS [t0]
    INNER JOIN [Table] AS [t1] ON [t0].[Id] = [t1].[Id]
    WHERE [t0].[Name] IS NOT NULL
) AS [t2] ON [t].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [t3].[Id], [t3].[MaumarEntity11818_Name], [t4].[Id] AS [Id0]
    FROM [Table] AS [t3]
    INNER JOIN [Table] AS [t4] ON [t3].[Id] = [t4].[Id]
    WHERE [t3].[MaumarEntity11818_Name] IS NOT NULL
) AS [t5] ON [t].[Id] = [t5].[Id]
GROUP BY [t2].[Name], [t5].[MaumarEntity11818_Name]");
            }
        }

        [ConditionalFact(Skip = "Issue #11871")]
        public virtual void GroupJoin_Anonymous_projection_GroupBy_Aggregate_join_elimination_4()
        {
            using (CreateDatabase11818())
            {
                using var context = new MyContext11818(_options);
                var query = (from e in context.Set<Entity11818>()
                             join a in context.Set<AnotherEntity11818>()
                                 on e.Id equals a.Id into grouping
                             from a in grouping.DefaultIfEmpty()
                             join m in context.Set<MaumarEntity11818>()
                                 on e.Id equals m.Id into grouping2
                             from m in grouping2.DefaultIfEmpty()
                             select new { aname = a.Name, mname = m.Name })
                    .OrderBy(g => g.aname)
                    .GroupBy(
                        g => new { g.aname, g.mname }).FirstOrDefault()
                    .Select(
                        g => new { MyKey = g.aname, cnt = g.mname })
                    .ToList();

                AssertSql(
                    "");
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

        private class MyContext11818 : DbContext
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

        private class Entity11818
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class AnotherEntity11818
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class MaumarEntity11818
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion

        #region Bug11803_11791

        [ConditionalFact]
        public virtual void Query_filter_with_db_set_should_not_block_other_filters()
        {
            using (CreateDatabase11803())
            {
                using var context = new MyContext11803(_options);
                context.Factions.ToList();

                AssertSql(
                    @"SELECT [f].[Id], [f].[Name]
FROM [Factions] AS [f]
WHERE EXISTS (
    SELECT 1
    FROM [Leaders] AS [l]
    WHERE ([l].[Name] IS NOT NULL AND ([l].[Name] LIKE N'Bran%')) AND ([l].[Name] = N'Crach an Craite'))");
            }
        }

        [ConditionalFact(Skip = "Issue#18903")]
        public virtual void Keyless_type_used_inside_defining_query()
        {
            using (CreateDatabase11803())
            {
                using var context = new MyContext11803(_options);
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

        private SqlServerTestStore CreateDatabase11803()
        {
            return CreateTestStore(
                () => new MyContext11803(_options),
                context =>
                {
                    var f1 = new Faction { Name = "Skeliege" };
                    var f2 = new Faction { Name = "Monsters" };
                    var f3 = new Faction { Name = "Nilfgaard" };
                    var f4 = new Faction { Name = "Northern Realms" };
                    var f5 = new Faction { Name = "Scioia'tael" };

                    var l11 = new Leader { Faction = f1, Name = "Bran Tuirseach" };
                    var l12 = new Leader { Faction = f1, Name = "Crach an Craite" };
                    var l13 = new Leader { Faction = f1, Name = "Eist Tuirseach" };
                    var l14 = new Leader { Faction = f1, Name = "Harald the Cripple" };

                    context.Factions.AddRange(f1, f2, f3, f4, f5);
                    context.Leaders.AddRange(l11, l12, l13, l14);

                    context.SaveChanges();

                    ClearLog();
                });
        }

        private class MyContext11803 : DbContext
        {
            public DbSet<Faction> Factions { get; set; }
            public DbSet<Leader> Leaders { get; set; }
            public DbSet<LeaderQuery> LeadersQuery { get; set; }

            public MyContext11803(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Leader>().HasQueryFilter(l => l.Name.StartsWith("Bran")); // this one is ignored
                modelBuilder.Entity<Faction>().HasQueryFilter(f => Leaders.Any(l => l.Name == "Crach an Craite"));

                modelBuilder
                    .Entity<FactionQuery>()
                    .HasNoKey()
                    .ToQuery(
                        () => Set<Leader>()
                            .Where(lq => lq.Name != "Foo")
                            .Select(
                                lq => new FactionQuery { Name = lq.Name }));

                modelBuilder
                    .Entity<LeaderQuery>()
                    .HasNoKey()
                    .ToQuery(
                        () => Set<FactionQuery>()
                            .Where(fq => fq.Name != "Bar")
                            .Select(
                                fq => new LeaderQuery { Name = "Not Bar" }));
            }
        }

        private class Faction
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public List<Leader> Leaders { get; set; }
        }

        private class Leader
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Faction Faction { get; set; }
        }

        private class FactionQuery
        {
            public string Name { get; set; }
        }

        private class LeaderQuery
        {
            public string Name { get; set; }
        }

        #endregion

        #region Bug11923

#pragma warning disable IDE0060 // Remove unused parameter
        private static bool ClientMethod11923(int id) => true;
#pragma warning restore IDE0060 // Remove unused parameter

        [ConditionalFact(Skip = "Issue #17244")]
        public virtual void Collection_without_setter_materialized_correctly()
        {
            using (CreateDatabase11923())
            {
                using var context = new MyContext11923(_options);
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

        private SqlServerTestStore CreateDatabase11923()
        {
            return CreateTestStore(
                () => new MyContext11923(_options),
                context =>
                {
                    var p111 = new Post11923 { Name = "P111" };
                    var p112 = new Post11923 { Name = "P112" };
                    var p121 = new Post11923 { Name = "P121" };
                    var p122 = new Post11923 { Name = "P122" };
                    var p123 = new Post11923 { Name = "P123" };
                    var p131 = new Post11923 { Name = "P131" };

                    var p211 = new Post11923 { Name = "P211" };
                    var p212 = new Post11923 { Name = "P212" };
                    var p221 = new Post11923 { Name = "P221" };
                    var p222 = new Post11923 { Name = "P222" };
                    var p223 = new Post11923 { Name = "P223" };
                    var p231 = new Post11923 { Name = "P231" };

                    var b1 = new Blog11923 { Name = "B1" };
                    var b2 = new Blog11923 { Name = "B2" };

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

        private class MyContext11923 : DbContext
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

        private class Blog11923
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

        private class Post11923
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public List<Comment11923> Comments { get; set; }
        }

        private class Comment11923
        {
            public int Id { get; set; }
        }

        private class CustomCollection11923 : List<Post11923>
        {
        }

        #endregion

        #region Bug11885

        [ConditionalFact]
        public virtual void Average_with_cast()
        {
            using (CreateDatabase11885())
            {
                using var context = new MyContext11885(_options);
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
                    @"SELECT AVG([p].[Price])
FROM [Prices] AS [p]",
                    //
                    @"SELECT AVG(CAST([p].[IntColumn] AS float))
FROM [Prices] AS [p]",
                    //
                    @"SELECT AVG(CAST([p].[NullableIntColumn] AS float))
FROM [Prices] AS [p]",
                    //
                    @"SELECT AVG(CAST([p].[LongColumn] AS float))
FROM [Prices] AS [p]",
                    //
                    @"SELECT AVG(CAST([p].[NullableLongColumn] AS float))
FROM [Prices] AS [p]",
                    //
                    @"SELECT CAST(AVG([p].[FloatColumn]) AS real)
FROM [Prices] AS [p]",
                    //
                    @"SELECT CAST(AVG([p].[NullableFloatColumn]) AS real)
FROM [Prices] AS [p]",
                    //
                    @"SELECT AVG([p].[DoubleColumn])
FROM [Prices] AS [p]",
                    //
                    @"SELECT AVG([p].[NullableDoubleColumn])
FROM [Prices] AS [p]");
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

        private class MyContext11885 : DbContext
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

        private class Price11885
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

        [ConditionalFact(Skip = "Issue #17244")]
        public virtual void Include_collection_with_OfType_base()
        {
            using (CreateDatabase12582())
            {
                using var context = new MyContext12582(_options);
                var query = context.Employees
                    .Include(i => i.Devices)
                    .OfType<IEmployee12582>()
                    .ToList();

                Assert.Single(query);

                var employee = (Employee12582)query[0];
                Assert.Equal(2, employee.Devices.Count);
            }
        }

        [ConditionalFact]
        public virtual void Correlated_collection_with_OfType_base()
        {
            using (CreateDatabase12582())
            {
                using var context = new MyContext12582(_options);
                var query = context.Employees
                    .Select(e => e.Devices.Where(d => d.Device != "foo").Cast<IEmployeeDevice12582>())
                    .ToList();

                Assert.Single(query);
                var result = query[0];
                Assert.Equal(2, result.Count());
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

        private interface IEmployee12582
        {
            string Name { get; set; }
        }

        private class Employee12582 : IEmployee12582
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public ICollection<EmployeeDevice12582> Devices { get; set; }
        }

        private interface IEmployeeDevice12582
        {
            string Device { get; set; }
        }

        private class EmployeeDevice12582 : IEmployeeDevice12582
        {
            public int Id { get; set; }
            public int EmployeeId { get; set; }
            public string Device { get; set; }
            public Employee12582 Employee { get; set; }
        }

        private class MyContext12582 : DbContext
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

        [ConditionalFact]
        public virtual void Correlated_collection_correctly_associates_entities_with_byte_array_keys()
        {
            using (CreateDatabase12748())
            {
                using var context = new MyContext12748(_options);
                var query = from blog in context.Blogs
                            select new
                            {
                                blog.Name,
                                Comments = blog.Comments.Select(
                                    u => new { u.Id }).ToArray()
                            };
                var result = query.ToList();
                Assert.Single(result[0].Comments);
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

        private class MyContext12748 : DbContext
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

        private class Blog12748
        {
            [Key]
            public byte[] Name { get; set; }

            public List<Comment12748> Comments { get; set; }
        }

        private class Comment12748
        {
            public int Id { get; set; }
            public byte[] BlogName { get; set; }
            public Blog12748 Blog { get; set; }
        }

        #endregion

        #region Bug13025

        [ConditionalFact]
        public virtual void Find_underlying_property_after_GroupJoin_DefaultIfEmpty()
        {
            using (CreateDatabase13025())
            {
                using var context = new MyContext13025(_options);
                var query = (from e in context.Employees
                             join d in context.EmployeeDevices
                                 on e.Id equals d.EmployeeId into grouping
                             from j in grouping.DefaultIfEmpty()
                             select new Holder13025 { Name = e.Name, DeviceId = j.DeviceId }).ToList();
            }
        }

        private class Holder13025
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
                            Devices = new List<EmployeeDevice13025> { new EmployeeDevice13025 { DeviceId = 1, Device = "Battery" } }
                        });

                    context.SaveChanges();
                    ClearLog();
                });
        }

        private class MyContext13025 : DbContext
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

        private class Employee13025
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public ICollection<EmployeeDevice13025> Devices { get; set; }
        }

        private class EmployeeDevice13025
        {
            public int Id { get; set; }
            public short DeviceId { get; set; }
            public int EmployeeId { get; set; }
            public string Device { get; set; }
            public Employee13025 Employee { get; set; }
        }

        #endregion

        #region Bug12170

        [ConditionalFact]
        public virtual void Weak_entities_with_query_filter_subquery_flattening()
        {
            using (CreateDatabase12170())
            {
                using var context = new MyContext12170(_options);
                var result = context.Definitions.Any();
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

        private class MyContext12170 : DbContext
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
        private class OptionalChangePoint12170
        {
            public DateTime? Timestamp { get; set; }
        }

        [Owned]
        private class MasterChangeInfo12170
        {
            public virtual OptionalChangePoint12170 RemovedPoint { get; set; }
        }

        private class DefinitionHistory12170
        {
            public int Id { get; set; }
            public int MacGuffinDefinitionID { get; set; }
            public virtual Definition12170 Definition { get; set; }
            public OptionalChangePoint12170 EndedPoint { get; set; }
        }

        private class Definition12170
        {
            public int Id { get; set; }
            public virtual MasterChangeInfo12170 ChangeInfo { get; set; }

            public virtual ICollection<DefinitionHistory12170> HistoryEntries { get; set; }
            public virtual DefinitionHistory12170 LatestHistoryEntry { get; set; }
            public int? LatestHistoryEntryID { get; set; }
        }

        #endregion

        #region Bug11944

        [ConditionalFact]
        public virtual void Include_collection_works_when_defined_on_intermediate_type()
        {
            using (CreateDatabase11944())
            {
                using var context = new MyContext11944(_options);
                var query = context.Schools.Include(s => ((ElementarySchool11944)s).Students);
                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal(2, result.OfType<ElementarySchool11944>().Single().Students.Count);
            }
        }

        [ConditionalFact]
        public virtual void Correlated_collection_works_when_defined_on_intermediate_type()
        {
            using (CreateDatabase11944())
            {
                using var context = new MyContext11944(_options);
                var query = context.Schools.Select(s => ((ElementarySchool11944)s).Students.Where(ss => true).ToList());
                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Contains(result, r => r.Count() == 2);
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

        private class MyContext11944 : DbContext
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

        private class Student11944
        {
            public int Id { get; set; }
            public ElementarySchool11944 School { get; set; }
        }

        private class School11944
        {
            public int Id { get; set; }
        }

        private abstract class PrimarySchool11944 : School11944
        {
            public List<Student11944> Students { get; set; }
        }

        private class ElementarySchool11944 : PrimarySchool11944
        {
        }

        #endregion

        #region Bug13118

        [ConditionalFact]
        public virtual void DateTime_Contains_with_smalldatetime_generates_correct_literal()
        {
            using (CreateDatabase13118())
            {
                using var context = new MyContext13118(_options);
                var testDateList = new List<DateTime> { new DateTime(2018, 10, 07) };
                var findRecordsWithDateInList = context.ReproEntity
                    .Where(a => testDateList.Contains(a.MyTime))
                    .ToList();

                Assert.Single(findRecordsWithDateInList);

                AssertSql(
                    @"SELECT [r].[Id], [r].[MyTime]
FROM [ReproEntity] AS [r]
WHERE [r].[MyTime] IN ('2018-10-07T00:00:00')");
            }
        }

        private SqlServerTestStore CreateDatabase13118()
        {
            return CreateTestStore(
                () => new MyContext13118(_options),
                context =>
                {
                    context.AddRange(
                        new ReproEntity13118 { MyTime = new DateTime(2018, 10, 07) },
                        new ReproEntity13118 { MyTime = new DateTime(2018, 10, 08) });

                    context.SaveChanges();
                    ClearLog();
                });
        }

        private class MyContext13118 : DbContext
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

        private class ReproEntity13118
        {
            public Guid Id { get; set; }
            public DateTime MyTime { get; set; }
        }

        #endregion

        #region Bug12732

        [ConditionalFact]
        public virtual void Nested_contains_with_enum()
        {
            using (CreateDatabase12732())
            {
                using var context = new MyContext12732(_options);
                var key = Guid.Parse("5f221fb9-66f4-442a-92c9-d97ed5989cc7");
                var keys = new List<Guid> { Guid.Parse("0a47bcb7-a1cb-4345-8944-c58f82d6aac7"), key };
                var todoTypes = new List<TodoType> { TodoType.foo0 };

                var query = context.Todos
                    .Where(x => keys.Contains(todoTypes.Contains(x.Type) ? key : key))
                    .ToList();

                Assert.Single(query);

                AssertSql(
                    @"@__key_2='5f221fb9-66f4-442a-92c9-d97ed5989cc7'

SELECT [t].[Id], [t].[Type]
FROM [Todos] AS [t]
WHERE CASE
    WHEN [t].[Type] IN (0) THEN @__key_2
    ELSE @__key_2
END IN ('0a47bcb7-a1cb-4345-8944-c58f82d6aac7', '5f221fb9-66f4-442a-92c9-d97ed5989cc7')");
            }
        }

        private SqlServerTestStore CreateDatabase12732()
        {
            return CreateTestStore(
                () => new MyContext12732(_options),
                context =>
                {
                    context.Add(
                        new Todo { Type = TodoType.foo0 });
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

        [ConditionalFact]
        public virtual void Correlated_subquery_with_owned_navigation_being_compared_to_null_works()
        {
            using (CreateDatabase13157())
            {
                using var context = new MyContext13157(_options);
                var partners = context.Partners
                    .Select(
                        x => new
                        {
                            Addresses = x.Addresses.Select(
                                y => new
                                {
                                    Turnovers = y.Turnovers == null
                                        ? null
                                        : new { y.Turnovers.AmountIn }
                                }).ToList()
                        }).ToList();

                Assert.Single(partners);
                Assert.Single(partners[0].Addresses);
                Assert.NotNull(partners[0].Addresses[0].Turnovers);
                Assert.Equal(10, partners[0].Addresses[0].Turnovers.AmountIn);

                AssertSql(
                    @"SELECT [p].[Id], [t0].[c], [t0].[Turnovers_AmountIn], [t0].[Id]
FROM [Partners] AS [p]
LEFT JOIN (
    SELECT CASE
        WHEN [t].[Id] IS NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [c], [t].[Turnovers_AmountIn], [a].[Id], [a].[Partner13157Id]
    FROM [Address13157] AS [a]
    LEFT JOIN (
        SELECT [a0].[Id], [a0].[Turnovers_AmountIn], [a1].[Id] AS [Id0]
        FROM [Address13157] AS [a0]
        INNER JOIN [Address13157] AS [a1] ON [a0].[Id] = [a1].[Id]
        WHERE [a0].[Turnovers_AmountIn] IS NOT NULL
    ) AS [t] ON [a].[Id] = [t].[Id]
) AS [t0] ON [p].[Id] = [t0].[Partner13157Id]
ORDER BY [p].[Id], [t0].[Id]");
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
                                new Address13157 { Turnovers = new AddressTurnovers13157 { AmountIn = 10 } }
                            }
                        }
                    );

                    context.SaveChanges();
                    ClearLog();
                });
        }

        private class MyContext13157 : DbContext
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

        private class Partner13157
        {
            public int Id { get; set; }
            public ICollection<Address13157> Addresses { get; set; }
        }

        private class Address13157
        {
            public int Id { get; set; }
            public AddressTurnovers13157 Turnovers { get; set; }
        }

        private class AddressTurnovers13157
        {
            public int AmountIn { get; set; }
        }

        #endregion

        #region Bug13346

        [ConditionalFact]
        public virtual void ToQuery_can_define_in_own_terms_using_FromSql()
        {
            using (CreateDatabase13346())
            {
                using var context = new MyContext13346(_options);
                var query = context.Set<OrderSummary13346>().ToList();

                Assert.Equal(4, query.Count);

                AssertSql(
                    "SELECT o.Amount From Orders AS o");
            }
        }

        private SqlServerTestStore CreateDatabase13346()
        {
            return CreateTestStore(
                () => new MyContext13346(_options),
                context =>
                {
                    context.AddRange(
                        new Order13346 { Amount = 1 },
                        new Order13346 { Amount = 2 },
                        new Order13346 { Amount = 3 },
                        new Order13346 { Amount = 4 }
                    );

                    context.SaveChanges();
                    ClearLog();
                });
        }

        private class MyContext13346 : DbContext
        {
            public virtual DbSet<Order13346> Orders { get; set; }

            public MyContext13346(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderSummary13346>()
                    .HasNoKey()
                    .ToQuery(
                        () => Set<OrderSummary13346>()
                            .FromSqlRaw("SELECT o.Amount From Orders AS o"));
            }
        }

        private class Order13346
        {
            public int Id { get; set; }
            public int Amount { get; set; }
        }

        private class OrderSummary13346
        {
            public int Amount { get; set; }
        }

        #endregion

        #region Bug13079

        [ConditionalFact]
        public virtual void Multilevel_owned_entities_determine_correct_nullability()
        {
            using (CreateDatabase13079())
            {
                using var context = new MyContext13079(_options);
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

        private SqlServerTestStore CreateDatabase13079()
        {
            return CreateTestStore(
                () => new MyContext13079(_options),
                context => ClearLog());
        }

        private class MyContext13079 : DbContext
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

        private class BaseEntity13079
        {
            public int Id { get; set; }
        }

        private class DerivedEntity13079 : BaseEntity13079
        {
            public int Property { get; set; }
            public OwnedData13079 Data { get; set; }
        }

        private class OwnedData13079
        {
            public int Property { get; set; }
            public OwnedSubData13079 SubData { get; set; }
        }

        private class OwnedSubData13079
        {
            public int Property { get; set; }
        }

        #endregion

        #region Bug13587

        [ConditionalFact]
        public virtual void Type_casting_inside_sum()
        {
            using (CreateDatabase13587())
            {
                using var context = new MyContext13587(_options);
                var result = context.InventoryPools.Sum(p => (decimal)p.Quantity);

                AssertSql(
                    @"SELECT SUM(CAST([i].[Quantity] AS decimal(18,2)))
FROM [InventoryPools] AS [i]");
            }
        }

        private SqlServerTestStore CreateDatabase13587()
        {
            return CreateTestStore(
                () => new MyContext13587(_options),
                context =>
                {
                    context.InventoryPools.Add(
                        new InventoryPool13587 { Quantity = 2 });

                    context.SaveChanges();

                    ClearLog();
                });
        }

        private class MyContext13587 : DbContext
        {
            public virtual DbSet<InventoryPool13587> InventoryPools { get; set; }

            public MyContext13587(DbContextOptions options)
                : base(options)
            {
            }
        }

        private class InventoryPool13587
        {
            public int Id { get; set; }
            public double Quantity { get; set; }
        }

        #endregion

        #region Bug12518

        [ConditionalFact]
        public virtual void Projecting_entity_with_value_converter_and_include_works()
        {
            using (CreateDatabase12518())
            {
                using var context = new MyContext12518(_options);
                var result = context.Parents.Include(p => p.Child).FirstOrDefault();

                AssertSql(
                    @"SELECT TOP(1) [p].[Id], [p].[ChildId], [c].[Id], [c].[ParentId], [c].[ULongRowVersion]
FROM [Parents] AS [p]
LEFT JOIN [Children] AS [c] ON [p].[ChildId] = [c].[Id]");
            }
        }

        private SqlServerTestStore CreateDatabase12518()
        {
            return CreateTestStore(
                () => new MyContext12518(_options),
                context =>
                {
                    context.Parents.Add(new Parent12518());
                    context.SaveChanges();

                    ClearLog();
                });
        }

        private class MyContext12518 : DbContext
        {
            public virtual DbSet<Parent12518> Parents { get; set; }
            public virtual DbSet<Child12518> Children { get; set; }

            public MyContext12518(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                var child = modelBuilder.Entity<Child12518>();
                child.HasOne(_ => _.Parent)
                    .WithOne(_ => _.Child)
                    .HasForeignKey<Parent12518>(_ => _.ChildId);
                child.Property(x => x.ULongRowVersion)
                    .HasConversion(new NumberToBytesConverter<ulong>())
                    .IsRowVersion()
                    .IsRequired()
                    .HasColumnType("RowVersion");

                modelBuilder.Entity<Parent12518>();
            }
        }

        private class Parent12518
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public Guid? ChildId { get; set; }
            public Child12518 Child { get; set; }
        }

        private class Child12518
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public ulong ULongRowVersion { get; set; }
            public Guid ParentId { get; set; }
            public Parent12518 Parent { get; set; }
        }

        #endregion

        #region Bug12549

        [ConditionalFact]
        public virtual void Union_and_insert_12549()
        {
            using (CreateDatabase12549())
            {
                using var context = new MyContext12549(_options);
                var id1 = 1;
                var id2 = 2;

                var ids1 = context.Set<Table1_12549>()
                    .Where(x => x.Id == id1)
                    .Select(x => x.Id);

                var ids2 = context.Set<Table2_12549>()
                    .Where(x => x.Id == id2)
                    .Select(x => x.Id);

                var results = ids1.Union(ids2).ToList();

                context.AddRange(new Table1_12549(), new Table2_12549(), new Table1_12549(), new Table2_12549());
                context.SaveChanges();
            }
        }

        private SqlServerTestStore CreateDatabase12549()
            => CreateTestStore(() => new MyContext12549(_options), context => { });

        private class MyContext12549 : DbContext
        {
            public DbSet<Table1_12549> Table1 { get; set; }
            public DbSet<Table2_12549> Table2 { get; set; }

            public MyContext12549(DbContextOptions options)
                : base(options)
            {
            }
        }

        private class Table1_12549
        {
            public int Id { get; set; }
        }

        private class Table2_12549
        {
            public int Id { get; set; }
        }

        #endregion

        #region Bug16233

        [ConditionalFact]
        public virtual void Derived_reference_is_skipped_when_base_type()
        {
            using (CreateDatabase16233())
            {
                using var context = new MyContext16233(_options);
                var result = context.Bases.Include(p => ((DerivedType16233)p).Reference).OrderBy(b => b.Id).ToList();

                Assert.Equal(3, result.Count);
                Assert.NotNull(Assert.IsType<DerivedType16233>(result[1]).Reference);
                Assert.Null(Assert.IsType<DerivedType16233>(result[2]).Reference);
                Assert.True(context.Entry(Assert.IsType<DerivedType16233>(result[2])).Reference("Reference").IsLoaded);

                AssertSql(
                    @"SELECT [b].[Id], [b].[Discriminator], [r].[Id], [r].[DerivedTypeId]
FROM [Bases] AS [b]
LEFT JOIN [Reference16233] AS [r] ON [b].[Id] = [r].[DerivedTypeId]
WHERE [b].[Discriminator] IN (N'BaseType16233', N'DerivedType16233')
ORDER BY [b].[Id]");
            }
        }

        [ConditionalFact]
        public virtual void Derived_reference_is_skipped_when_base_type_no_tracking()
        {
            using (CreateDatabase16233())
            {
                using var context = new MyContext16233(_options);
                var result = context.Bases.AsNoTracking().Include(p => ((DerivedType16233)p).Reference).OrderBy(b => b.Id).ToList();

                Assert.Equal(3, result.Count);
                Assert.NotNull(Assert.IsType<DerivedType16233>(result[1]).Reference);
                Assert.NotNull(Assert.IsType<DerivedType16233>(result[1]).Reference.DerivedType);
                Assert.Null(Assert.IsType<DerivedType16233>(result[2]).Reference);

                AssertSql(
                    @"SELECT [b].[Id], [b].[Discriminator], [r].[Id], [r].[DerivedTypeId]
FROM [Bases] AS [b]
LEFT JOIN [Reference16233] AS [r] ON [b].[Id] = [r].[DerivedTypeId]
WHERE [b].[Discriminator] IN (N'BaseType16233', N'DerivedType16233')
ORDER BY [b].[Id]");
            }
        }

        private SqlServerTestStore CreateDatabase16233()
        {
            return CreateTestStore(
                () => new MyContext16233(_options),
                context =>
                {
                    context.AddRange(
                        new BaseType16233(),
                        new DerivedType16233 { Reference = new Reference16233() },
                        new DerivedType16233());

                    context.SaveChanges();

                    ClearLog();
                });
        }

        private class MyContext16233 : DbContext
        {
            public virtual DbSet<BaseType16233> Bases { get; set; }
            public virtual DbSet<DerivedType16233> Derived { get; set; }

            public MyContext16233(DbContextOptions options)
                : base(options)
            {
            }
        }

        private class BaseType16233
        {
            public int Id { get; set; }
        }

        private class DerivedType16233 : BaseType16233
        {
            public Reference16233 Reference { get; set; }
        }

        private class Reference16233
        {
            public int Id { get; set; }
            public int DerivedTypeId { get; set; }
            public DerivedType16233 DerivedType { get; set; }
        }

        #endregion

        #region Bug15684

        [ConditionalFact]
        public virtual void Projection_failing_with_EnumToStringConverter()
        {
            using (CreateDatabase15684())
            {
                using var context = new MyContext15684(_options);
                var query = from p in context.Products
                            join c in context.Categories on p.CategoryId equals c.Id into grouping
                            from c in grouping.DefaultIfEmpty()
                            select new ProductDto15684
                            {
                                Id = p.Id,
                                Name = p.Name,
                                CategoryName = c == null ? "Other" : c.Name,
                                CategoryStatus = c == null ? CategoryStatus15684.Active : c.Status
                            };
                var result = query.ToList();
                Assert.Equal(2, result.Count);
                AssertSql(
                    @"SELECT [p].[Id], [p].[Name], CASE
    WHEN [c].[Id] IS NULL THEN N'Other'
    ELSE [c].[Name]
END AS [CategoryName], CASE
    WHEN [c].[Id] IS NULL THEN N'Active'
    ELSE [c].[Status]
END AS [CategoryStatus]
FROM [Products] AS [p]
LEFT JOIN [Categories] AS [c] ON [p].[CategoryId] = [c].[Id]");
            }
        }

        private SqlServerTestStore CreateDatabase15684()
            => CreateTestStore(
                () => new MyContext15684(_options),
                context =>
                {
                    context.Products.Add(
                        new Product15684
                        {
                            Name = "Apple",
                            Category = new Category15684 { Name = "Fruit", Status = CategoryStatus15684.Active }
                        });

                    context.Products.Add(new Product15684 { Name = "Bike" });

                    context.SaveChanges();

                    ClearLog();
                });

        private class MyContext15684 : DbContext
        {
            public DbSet<Category15684> Categories { get; set; }
            public DbSet<Product15684> Products { get; set; }

            public MyContext15684(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<Category15684>()
                    .Property(e => e.Status)
                    .HasConversion(new EnumToStringConverter<CategoryStatus15684>());
            }
        }

        private class Product15684
        {
            [Key]
            public int Id { get; set; }

            [Required]
            public string Name { get; set; }

            public int? CategoryId { get; set; }

            public Category15684 Category { get; set; }
        }

        private class Category15684
        {
            [Key]
            public int Id { get; set; }

            [Required]
            public string Name { get; set; }

            public CategoryStatus15684 Status { get; set; }
        }

        private class ProductDto15684
        {
            public string CategoryName { get; set; }
            public CategoryStatus15684 CategoryStatus { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public enum CategoryStatus15684
        {
            Active = 0,
            Removed = 1
        }

        #endregion

        #region Bug15204

        private MemberInfo GetMemberInfo(Type type, string name)
        {
            return type.GetProperty(name);
        }

        [ConditionalFact]
        public virtual void Null_check_removal_applied_recursively()
        {
            using (CreateDatabase15204())
            {
                var userParam = Expression.Parameter(typeof(TBuilding15204), "s");
                var builderProperty = Expression.MakeMemberAccess(userParam, GetMemberInfo(typeof(TBuilding15204), "Builder"));
                var cityProperty = Expression.MakeMemberAccess(builderProperty, GetMemberInfo(typeof(TBuilder15204), "City"));
                var nameProperty = Expression.MakeMemberAccess(cityProperty, GetMemberInfo(typeof(TCity15204), "Name"));

                //{s => (IIF((IIF((s.Builder == null), null, s.Builder.City) == null), null, s.Builder.City.Name) == "Leeds")}
                var selection = Expression.Lambda<Func<TBuilding15204, bool>>(
                    Expression.Equal(
                        Expression.Condition(
                            Expression.Equal(
                                Expression.Condition(
                                    Expression.Equal(
                                        builderProperty,
                                        Expression.Constant(null, typeof(TBuilder15204))),
                                    Expression.Constant(null, typeof(TCity15204)),
                                    cityProperty),
                                Expression.Constant(null, typeof(TCity15204))),
                            Expression.Constant(null, typeof(string)),
                            nameProperty),
                        Expression.Constant("Leeds", typeof(string))),
                    userParam);

                using var context = new MyContext15204(_options);
                var query = context.BuildingSet
                    .Where(selection)
                    .Include(a => a.Builder).ThenInclude(a => a.City)
                    .Include(a => a.Mandator).ToList();

                Assert.True(query.Count == 1);
                Assert.True(query.First().Builder.City.Name == "Leeds");
                Assert.True(query.First().LongName == "Two L2");

                AssertSql(
                    @"SELECT [b].[Id], [b].[BuilderId], [b].[Identity], [b].[LongName], [b].[MandatorId], [b0].[Id], [b0].[CityId], [b0].[Name], [c].[Id], [c].[Name], [m].[Id], [m].[Identity], [m].[Name]
FROM [BuildingSet] AS [b]
INNER JOIN [Builder] AS [b0] ON [b].[BuilderId] = [b0].[Id]
INNER JOIN [City] AS [c] ON [b0].[CityId] = [c].[Id]
INNER JOIN [MandatorSet] AS [m] ON [b].[MandatorId] = [m].[Id]
WHERE [c].[Name] = N'Leeds'");
            }
        }

        private SqlServerTestStore CreateDatabase15204()
            => CreateTestStore(
                () => new MyContext15204(_options),
                context =>
                {
                    var london = new TCity15204 { Name = "London" };
                    var sam = new TBuilder15204 { Name = "Sam", City = london };

                    context.MandatorSet.Add(
                        new TMandator15204
                        {
                            Identity = Guid.NewGuid(),
                            Name = "One",
                            Buildings = new List<TBuilding15204>
                            {
                                new TBuilding15204
                                {
                                    Identity = Guid.NewGuid(),
                                    LongName = "One L1",
                                    Builder = sam
                                },
                                new TBuilding15204
                                {
                                    Identity = Guid.NewGuid(),
                                    LongName = "One L2",
                                    Builder = sam
                                }
                            }
                        });
                    context.MandatorSet.Add(
                        new TMandator15204
                        {
                            Identity = Guid.NewGuid(),
                            Name = "Two",
                            Buildings = new List<TBuilding15204>
                            {
                                new TBuilding15204
                                {
                                    Identity = Guid.NewGuid(),
                                    LongName = "Two L1",
                                    Builder = new TBuilder15204 { Name = "John", City = london }
                                },
                                new TBuilding15204
                                {
                                    Identity = Guid.NewGuid(),
                                    LongName = "Two L2",
                                    Builder = new TBuilder15204 { Name = "Mark", City = new TCity15204 { Name = "Leeds" } }
                                }
                            }
                        });

                    context.SaveChanges();

                    ClearLog();
                });

        private class MyContext15204 : DbContext
        {
            public DbSet<TMandator15204> MandatorSet { get; set; }
            public DbSet<TBuilding15204> BuildingSet { get; set; }
            public DbSet<TBuilder15204> Builder { get; set; }
            public DbSet<TCity15204> City { get; set; }

            public MyContext15204(DbContextOptions options)
                : base(options)
            {
                ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                ChangeTracker.AutoDetectChangesEnabled = false;
            }
        }

        private class TBuilding15204
        {
            public int Id { get; set; }
            public Guid Identity { get; set; }
            public string LongName { get; set; }
            public int BuilderId { get; set; }
            public TBuilder15204 Builder { get; set; }
            public TMandator15204 Mandator { get; set; }
            public int MandatorId { get; set; }
        }

        private class TBuilder15204
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int CityId { get; set; }
            public TCity15204 City { get; set; }
        }

        private class TCity15204
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class TMandator15204
        {
            public int Id { get; set; }
            public Guid Identity { get; set; }
            public string Name { get; set; }
            public virtual ICollection<TBuilding15204> Buildings { get; set; }
        }

        #endregion

        #region Bug15518

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Nested_queries_does_not_cause_concurrency_exception_sync(bool tracking)
        {
            using (CreateDatabase15518())
            {
                using var context = new MyContext15518(_options);
                var query = context.Repos.OrderBy(r => r.Id).Where(r => r.Id > 0);
                query = tracking ? query.AsTracking() : query.AsNoTracking();

                foreach (var a in query)
                {
                    foreach (var b in query)
                    {
                    }
                }
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Nested_queries_does_not_cause_concurrency_exception_async(bool tracking)
        {
            using (CreateDatabase15518())
            {
                using var context = new MyContext15518(_options);
                var query = context.Repos.OrderBy(r => r.Id).Where(r => r.Id > 0);
                query = tracking ? query.AsTracking() : query.AsNoTracking();

                await foreach (var a in query.AsAsyncEnumerable())
                {
                    await foreach (var b in query.AsAsyncEnumerable())
                    {
                    }
                }
            }
        }

        private SqlServerTestStore CreateDatabase15518()
            => CreateTestStore(
                () => new MyContext15518(_options),
                context =>
                {
                    context.AddRange(
                        new Repo15518 { Name = "London" },
                        new Repo15518 { Name = "New York" });

                    context.SaveChanges();

                    ClearLog();
                });

        private class MyContext15518 : DbContext
        {
            public DbSet<Repo15518> Repos { get; set; }

            public MyContext15518(DbContextOptions options)
                : base(options)
            {
            }
        }

        private class Repo15518
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion

        #region Bug8864

        [ConditionalFact]
        public virtual void Select_nested_projection()
        {
            using (CreateDatabase8864())
            {
                using var context = new MyContext8864(_options);
                var customers = context.Customers
                    .Select(c => new { Customer = c, CustomerAgain = Get(context, c.Id) })
                    .ToList();

                Assert.Equal(2, customers.Count);

                foreach (var customer in customers)
                {
                    Assert.Same(customer.Customer, customer.CustomerAgain);
                }
            }
        }

        private static Customer8864 Get(MyContext8864 context, int id)
            => context.Customers.Single(c => c.Id == id);

        private SqlServerTestStore CreateDatabase8864()
            => CreateTestStore(
                () => new MyContext8864(_options),
                context =>
                {
                    context.AddRange(
                        new Customer8864 { Name = "Alan" },
                        new Customer8864 { Name = "Elon" });

                    context.SaveChanges();

                    ClearLog();
                });

        private class MyContext8864 : DbContext
        {
            public DbSet<Customer8864> Customers { get; set; }

            public MyContext8864(DbContextOptions options)
                : base(options)
            {
            }
        }

        private class Customer8864
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion

        #region Bug7983

        [ConditionalFact]
        public virtual void New_instances_in_projection_are_not_shared_across_results()
        {
            using (CreateDatabase7983())
            {
                using var context = new MyContext7983(_options);
                var list = context.Posts.Select(p => new PostDTO7983().From(p)).ToList();

                Assert.Equal(3, list.Count);
                Assert.Equal(new[] { "First", "Second", "Third" }, list.Select(dto => dto.Title));

                AssertSql(
                    @"SELECT [p].[Id], [p].[BlogId], [p].[Title]
FROM [Posts] AS [p]");
            }
        }

        private SqlServerTestStore CreateDatabase7983()
            => CreateTestStore(
                () => new MyContext7983(_options),
                context =>
                {
                    context.Add(
                        new Blog7983
                        {
                            Posts = new List<Post7983>
                            {
                                new Post7983 { Title = "First" },
                                new Post7983 { Title = "Second" },
                                new Post7983 { Title = "Third" }
                            }
                        });

                    context.SaveChanges();

                    ClearLog();
                });

        private class MyContext7983 : DbContext
        {
            public DbSet<Blog7983> Blogs { get; set; }
            public DbSet<Post7983> Posts { get; set; }

            public MyContext7983(DbContextOptions options)
                : base(options)
            {
            }
        }

        private class Blog7983
        {
            public int Id { get; set; }
            public string Title { get; set; }

            public ICollection<Post7983> Posts { get; set; }
        }

        private class Post7983
        {
            public int Id { get; set; }
            public string Title { get; set; }

            public int? BlogId { get; set; }
            public Blog7983 Blog { get; set; }
        }

        private class PostDTO7983
        {
            public string Title { get; set; }

            public PostDTO7983 From(Post7983 post)
            {
                Title = post.Title;
                return this;
            }
        }

        #endregion

        #region Bug17253

        [ConditionalFact]
        public virtual void Self_reference_in_query_filter_works()
        {
            using (CreateDatabase17253())
            {
                using var context = new MyContext17253(_options);
                var query = context.EntitiesWithQueryFilterSelfReference.Where(e => e.Name != "Foo");
                var result = query.ToList();

                AssertSql(
                    @"SELECT [e].[Id], [e].[Name]
FROM [EntitiesWithQueryFilterSelfReference] AS [e]
WHERE EXISTS (
    SELECT 1
    FROM [EntitiesWithQueryFilterSelfReference] AS [e0]) AND (([e].[Name] <> N'Foo') OR [e].[Name] IS NULL)");
            }
        }

        [ConditionalFact]
        public virtual void Self_reference_in_query_filter_works_when_nested()
        {
            using (CreateDatabase17253())
            {
                using var context = new MyContext17253(_options);
                var query = context.EntitiesReferencingEntityWithQueryFilterSelfReference.Where(e => e.Name != "Foo");
                var result = query.ToList();

                AssertSql(
                    @"SELECT [e].[Id], [e].[Name]
FROM [EntitiesReferencingEntityWithQueryFilterSelfReference] AS [e]
WHERE EXISTS (
    SELECT 1
    FROM [EntitiesWithQueryFilterSelfReference] AS [e0]
    WHERE EXISTS (
        SELECT 1
        FROM [EntitiesWithQueryFilterSelfReference] AS [e1])) AND (([e].[Name] <> N'Foo') OR [e].[Name] IS NULL)");
            }
        }

        private class MyContext17253 : DbContext
        {
            public DbSet<EntityWithQueryFilterSelfReference> EntitiesWithQueryFilterSelfReference { get; set; }

            public DbSet<EntityReferencingEntityWithQueryFilterSelfReference> EntitiesReferencingEntityWithQueryFilterSelfReference
            {
                get;
                set;
            }

            public DbSet<EntityWithQueryFilterCycle1> EntitiesWithQueryFilterCycle1 { get; set; }
            public DbSet<EntityWithQueryFilterCycle2> EntitiesWithQueryFilterCycle2 { get; set; }
            public DbSet<EntityWithQueryFilterCycle3> EntitiesWithQueryFilterCycle3 { get; set; }

            public MyContext17253(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<EntityWithQueryFilterSelfReference>().HasQueryFilter(e => EntitiesWithQueryFilterSelfReference.Any());
                modelBuilder.Entity<EntityReferencingEntityWithQueryFilterSelfReference>()
                    .HasQueryFilter(e => Set<EntityWithQueryFilterSelfReference>().Any());

                modelBuilder.Entity<EntityWithQueryFilterCycle1>().HasQueryFilter(e => EntitiesWithQueryFilterCycle2.Any());
                modelBuilder.Entity<EntityWithQueryFilterCycle2>().HasQueryFilter(e => Set<EntityWithQueryFilterCycle3>().Any());
                modelBuilder.Entity<EntityWithQueryFilterCycle3>().HasQueryFilter(e => EntitiesWithQueryFilterCycle1.Any());
            }
        }

        private SqlServerTestStore CreateDatabase17253()
            => CreateTestStore(
                () => new MyContext17253(_options),
                context =>
                {
                    context.EntitiesWithQueryFilterSelfReference.Add(
                        new EntityWithQueryFilterSelfReference { Name = "EntityWithQueryFilterSelfReference" });
                    context.EntitiesReferencingEntityWithQueryFilterSelfReference.Add(
                        new EntityReferencingEntityWithQueryFilterSelfReference
                        {
                            Name = "EntityReferencingEntityWithQueryFilterSelfReference"
                        });

                    context.EntitiesWithQueryFilterCycle1.Add(new EntityWithQueryFilterCycle1 { Name = "EntityWithQueryFilterCycle1_1" });
                    context.EntitiesWithQueryFilterCycle2.Add(new EntityWithQueryFilterCycle2 { Name = "EntityWithQueryFilterCycle2_1" });
                    context.EntitiesWithQueryFilterCycle3.Add(new EntityWithQueryFilterCycle3 { Name = "EntityWithQueryFilterCycle3_1" });

                    context.SaveChanges();

                    ClearLog();
                });

        private class EntityWithQueryFilterSelfReference
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class EntityReferencingEntityWithQueryFilterSelfReference
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class EntityWithQueryFilterCycle1
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class EntityWithQueryFilterCycle2
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class EntityWithQueryFilterCycle3
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion

        #region Bug17276_17099_16759

        [ConditionalFact]
        public virtual void Expression_tree_constructed_via_interface_works_17276()
        {
            using (CreateDatabase17276())
            {
                using var context = new MyContext17276(_options);
                var query = List17276(context.RemovableEntities);

                AssertSql(
                    @"SELECT [r].[Id], [r].[IsRemoved], [r].[Removed], [r].[RemovedByUser], [t].[Id], [t].[OwnedEntity_OwnedValue]
FROM [RemovableEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[Id], [r0].[OwnedEntity_OwnedValue], [r1].[Id] AS [Id0]
    FROM [RemovableEntities] AS [r0]
    INNER JOIN [RemovableEntities] AS [r1] ON [r0].[Id] = [r1].[Id]
    WHERE [r0].[OwnedEntity_OwnedValue] IS NOT NULL
) AS [t] ON [r].[Id] = [t].[Id]
WHERE [r].[IsRemoved] <> CAST(1 AS bit)");
            }
        }

        [ConditionalFact]
        public virtual void Expression_tree_constructed_via_interface_for_navigation_works_17099()
        {
            using (CreateDatabase17276())
            {
                using var context = new MyContext17276(_options);
                var query = context.Parents
                    .Where(p => EF.Property<bool>(EF.Property<IRemovable17276>(p, "RemovableEntity"), "IsRemoved"))
                    .ToList();

                AssertSql(
                    @"SELECT [p].[Id], [p].[RemovableEntityId]
FROM [Parents] AS [p]
LEFT JOIN [RemovableEntities] AS [r] ON [p].[RemovableEntityId] = [r].[Id]
WHERE [r].[IsRemoved] = CAST(1 AS bit)");
            }
        }

        [ConditionalFact]
        public virtual void Expression_tree_constructed_via_interface_for_owned_navigation_works_17505()
        {
            using (CreateDatabase17276())
            {
                using var context = new MyContext17276(_options);
                var query = context.RemovableEntities
                    .Where(p => EF.Property<string>(EF.Property<IOwned>(p, "OwnedEntity"), "OwnedValue") == "Abc")
                    .ToList();

                AssertSql(
                    @"SELECT [r].[Id], [r].[IsRemoved], [r].[Removed], [r].[RemovedByUser], [t].[Id], [t].[OwnedEntity_OwnedValue]
FROM [RemovableEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[Id], [r0].[OwnedEntity_OwnedValue], [r1].[Id] AS [Id0]
    FROM [RemovableEntities] AS [r0]
    INNER JOIN [RemovableEntities] AS [r1] ON [r0].[Id] = [r1].[Id]
    WHERE [r0].[OwnedEntity_OwnedValue] IS NOT NULL
) AS [t] ON [r].[Id] = [t].[Id]
WHERE [t].[OwnedEntity_OwnedValue] = N'Abc'");
            }
        }

        [ConditionalFact]
        public virtual void Expression_tree_constructed_via_interface_works_16759()
        {
            using (CreateDatabase17276())
            {
                using var context = new MyContext17276(_options);
                var specification = new Specification17276<Parent17276>(1);
                var entities = context.Set<Parent17276>().Where(specification.Criteria).ToList();

                AssertSql(
                    @"@__id_0='1'

SELECT [p].[Id], [p].[RemovableEntityId]
FROM [Parents] AS [p]
WHERE [p].[Id] = @__id_0");
            }
        }

        private class MyContext17276 : DbContext
        {
            public DbSet<RemovableEntity17276> RemovableEntities { get; set; }
            public DbSet<Parent17276> Parents { get; set; }

            public MyContext17276(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
            }
        }

        private SqlServerTestStore CreateDatabase17276()
            => CreateTestStore(
                () => new MyContext17276(_options),
                context =>
                {
                    context.SaveChanges();

                    ClearLog();
                });

        private static List<T> List17276<T>(IQueryable<T> query)
            where T : IRemovable17276
        {
            return query.Where(x => !x.IsRemoved).ToList();
        }

        private interface IRemovable17276
        {
            bool IsRemoved { get; set; }

            string RemovedByUser { get; set; }

            DateTime? Removed { get; set; }
        }

        private class RemovableEntity17276 : IRemovable17276
        {
            public int Id { get; set; }
            public bool IsRemoved { get; set; }
            public string RemovedByUser { get; set; }
            public DateTime? Removed { get; set; }
            public OwnedEntity OwnedEntity { get; set; }
        }

        private class Parent17276 : IHasId17276<int>
        {
            public int Id { get; set; }
            public RemovableEntity17276 RemovableEntity { get; set; }
        }

        [Owned]
        private class OwnedEntity : IOwned
        {
            public string OwnedValue { get; set; }
        }

        private interface IHasId17276<out T>
        {
            T Id { get; }
        }

        private interface IOwned
        {
            string OwnedValue { get; }
        }

        private class Specification17276<T>
            where T : IHasId17276<int>
        {
            public Expression<Func<T, bool>> Criteria { get; }

            public Specification17276(int id)
            {
                Criteria = t => t.Id == id;
            }
        }

        #endregion

        #region Bug6864

        [ConditionalFact]
        public virtual void Implicit_cast_6864()
        {
            using (CreateDatabase6864())
            {
                using var context = new MyContext6864(_options);
                // Verify no client eval
                var query = context.Foos.Where(f => f.String == new Bar6864(1337)).ToList();

                AssertSql(
                    @"SELECT [f].[Id], [f].[String]
FROM [Foos] AS [f]
WHERE [f].[String] = N'1337'");
            }
        }

        [ConditionalFact]
        public virtual void Access_property_of_closure_6864()
        {
            using (CreateDatabase6864())
            {
                using var context = new MyContext6864(_options);
                // Verify no client eval
                var bar = new Bar6864(1337);
                var query = context.Foos.Where(f => f.String == bar.Value).ToList();

                AssertSql(
                    @"@__bar_Value_0='1337' (Size = 4000)

SELECT [f].[Id], [f].[String]
FROM [Foos] AS [f]
WHERE [f].[String] = @__bar_Value_0");
            }
        }

        [ConditionalFact]
        public virtual void Call_method_on_closure_6864()
        {
            using (CreateDatabase6864())
            {
                using var context = new MyContext6864(_options);
                // Verify no client eval
                var bar = new Bar6864(1337);
                var query = context.Foos.Where(f => f.String == bar.ToString()).ToList();

                AssertSql(
                    @"@__ToString_0='1337' (Size = 4000)

SELECT [f].[Id], [f].[String]
FROM [Foos] AS [f]
WHERE [f].[String] = @__ToString_0");
            }
        }

        [ConditionalFact]
        public virtual void Implicitly_cast_closure_6864()
        {
            using (CreateDatabase6864())
            {
                using var context = new MyContext6864(_options);
                // Verify no client eval
                var bar = new Bar6864(1337);
                var query = context.Foos.Where(f => f.String == bar).ToList();

                AssertSql(
                    @"@__p_0='1337' (Size = 4000)

SELECT [f].[Id], [f].[String]
FROM [Foos] AS [f]
WHERE [f].[String] = @__p_0");
            }
        }

        [ConditionalFact]
        public virtual void Implicitly_cast_return_value_6864()
        {
            using (CreateDatabase6864())
            {
                using var context = new MyContext6864(_options);
                // Verify no client eval
                var query = context.Foos.Where(f => f.String == new Bar6864(1337).Clone()).ToList();

                AssertSql(
                    @"SELECT [f].[Id], [f].[String]
FROM [Foos] AS [f]
WHERE [f].[String] = N'1337'");
            }
        }

        private class MyContext6864 : DbContext
        {
            public DbSet<FooEntity6864> Foos { get; set; }

            public MyContext6864(DbContextOptions options)
                : base(options)
            {
            }
        }

        private SqlServerTestStore CreateDatabase6864()
            => CreateTestStore(
                () => new MyContext6864(_options),
                context =>
                {
                    context.SaveChanges();

                    ClearLog();
                });

        private class FooEntity6864
        {
            public int Id { get; set; }
            public string String { get; set; }
        }

        private class Bar6864
        {
            private readonly int _value;

            public Bar6864(int value)
            {
                _value = value;
            }

            public string Value => _value.ToString();
            public override string ToString() => Value;
            public static implicit operator string(Bar6864 bar) => bar.Value;
            public Bar6864 Clone() => new Bar6864(_value);
        }

        #endregion

        #region Bug9582

        [ConditionalFact]
        public virtual void Setting_IsUnicode_generates_unicode_literal_in_SQL()
        {
            using (CreateDatabase9582())
            {
                using var context = new MyContext9582(_options);
                // Verify SQL
                var query = context.Set<TipoServicio9582>().Where(xx => xx.Nombre.Contains("lla")).ToList();

                AssertSql(
                    @"SELECT [t].[Id], [t].[Nombre]
FROM [TipoServicio9582] AS [t]
WHERE CHARINDEX('lla', [t].[Nombre]) > 0");
            }
        }

        private class MyContext9582 : DbContext
        {
            public MyContext9582(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<TipoServicio9582>(
                    builder =>
                    {
                        builder.HasKey(ts => ts.Id);

                        builder.Property(ts => ts.Id).IsRequired();
                        builder.Property(ts => ts.Nombre).IsRequired().HasMaxLength(20);
                    });

                foreach (var property in modelBuilder.Model.GetEntityTypes()
                    .SelectMany(e => e.GetProperties().Where(p => p.ClrType == typeof(string))))
                {
                    property.SetIsUnicode(false);
                }
            }
        }

        private SqlServerTestStore CreateDatabase9582()
            => CreateTestStore(
                () => new MyContext9582(_options),
                context =>
                {
                    context.SaveChanges();

                    ClearLog();
                });

        private class TipoServicio9582
        {
            public int Id { get; set; }
            public string Nombre { get; set; }
        }

        #endregion

        #region Bug7222

        [ConditionalFact]
        public virtual void Inlined_dbcontext_is_not_leaking()
        {
            using (CreateDatabase7222())
            {
                using var context = new MyContext7222(_options);
                var entities = context.Blogs.Select(b => context.ClientMethod(b)).ToList();

                AssertSql(
                    @"SELECT [b].[Id]
FROM [Blogs] AS [b]");
            }
        }

        [ConditionalFact]
        public virtual void Implicit_DbContext_throws_memory_leak()
        {
            using (CreateDatabase7222())
            {
                using var context = new MyContext7222(_options);
                Assert.Throws<InvalidOperationException>(() => context.RunQuery());
            }
        }

        private class MyContext7222 : DbContext
        {
            public DbSet<Blog7222> Blogs { get; set; }

            public MyContext7222()
            {
            }

            public MyContext7222(DbContextOptions options)
                : base(options)
            {
            }

            public void RunQuery()
            {
                Blogs.Select(b => ClientMethod(b)).ToList();
            }

            public int ClientMethod(Blog7222 blog) => blog.Id;
        }

        private SqlServerTestStore CreateDatabase7222()
            => CreateTestStore(
                () => new MyContext7222(_options),
                context =>
                {
                    context.SaveChanges();

                    ClearLog();
                });

        private class Blog7222
        {
            public int Id { get; set; }
        }

        #endregion

        #region Bug17644

        [ConditionalFact]
        public virtual async Task Return_type_of_First_is_preserved()
        {
            using (CreateDatabase17644())
            {
                using var context = new MyContext17644(_options);
                var personsToFind = await context.Persons.Where(p => p.Age >= 21)
                    .Select(p => new PersonDetailView17644 { Name = p.Name, Age = p.Age })
                    .FirstAsync<PersonView17644>();

                AssertSql(
                    @"SELECT TOP(1) [p].[Name], [p].[Age]
FROM [Persons] AS [p]
WHERE [p].[Age] >= 21");
            }
        }

        [ConditionalFact]
        public virtual async Task Return_type_of_FirstOrDefault_is_preserved()
        {
            using (CreateDatabase17644())
            {
                using var context = new MyContext17644(_options);
                var personsToFind = await context.Persons.Where(p => p.Age >= 21)
                    .Select(p => new PersonDetailView17644 { Name = p.Name, Age = p.Age })
                    .FirstOrDefaultAsync<PersonView17644>();

                AssertSql(
                    @"SELECT TOP(1) [p].[Name], [p].[Age]
FROM [Persons] AS [p]
WHERE [p].[Age] >= 21");
            }
        }

        [ConditionalFact]
        public virtual async Task Return_type_of_Single_is_preserved()
        {
            using (CreateDatabase17644())
            {
                using var context = new MyContext17644(_options);
                var personsToFind = await context.Persons.Where(p => p.Age >= 21)
                    .Select(p => new PersonDetailView17644 { Name = p.Name, Age = p.Age })
                    .SingleAsync<PersonView17644>();

                AssertSql(
                    @"SELECT TOP(2) [p].[Name], [p].[Age]
FROM [Persons] AS [p]
WHERE [p].[Age] >= 21");
            }
        }

        [ConditionalFact]
        public virtual async Task Return_type_of_SingleOrDefault_is_preserved()
        {
            using (CreateDatabase17644())
            {
                using var context = new MyContext17644(_options);
                var personsToFind = await context.Persons.Where(p => p.Age >= 21)
                    .Select(p => new PersonDetailView17644 { Name = p.Name, Age = p.Age })
                    .SingleOrDefaultAsync<PersonView17644>();

                AssertSql(
                    @"SELECT TOP(2) [p].[Name], [p].[Age]
FROM [Persons] AS [p]
WHERE [p].[Age] >= 21");
            }
        }

        [ConditionalFact]
        public virtual async Task Return_type_of_Last_is_preserved()
        {
            using (CreateDatabase17644())
            {
                using var context = new MyContext17644(_options);
                var personsToFind = await context.Persons.Where(p => p.Age >= 21)
                    .OrderBy(p => p.Id)
                    .Select(p => new PersonDetailView17644 { Name = p.Name, Age = p.Age })
                    .LastAsync<PersonView17644>();

                AssertSql(
                    @"SELECT TOP(1) [p].[Name], [p].[Age]
FROM [Persons] AS [p]
WHERE [p].[Age] >= 21
ORDER BY [p].[Id] DESC");
            }
        }

        [ConditionalFact]
        public virtual async Task Return_type_of_LastOrDefault_is_preserved()
        {
            using (CreateDatabase17644())
            {
                using var context = new MyContext17644(_options);
                var personsToFind = await context.Persons.Where(p => p.Age >= 21)
                    .OrderBy(p => p.Id)
                    .Select(p => new PersonDetailView17644 { Name = p.Name, Age = p.Age })
                    .LastOrDefaultAsync<PersonView17644>();

                AssertSql(
                    @"SELECT TOP(1) [p].[Name], [p].[Age]
FROM [Persons] AS [p]
WHERE [p].[Age] >= 21
ORDER BY [p].[Id] DESC");
            }
        }

        private class MyContext17644 : DbContext
        {
            public DbSet<Person17644> Persons { get; set; }

            public MyContext17644()
            {
            }

            public MyContext17644(DbContextOptions options)
                : base(options)
            {
            }
        }

        private SqlServerTestStore CreateDatabase17644()
            => CreateTestStore(
                () => new MyContext17644(_options),
                context =>
                {
                    var person = new Person17644 { Name = "John Doe", Age = 21 };
                    context.Persons.Add(person);
                    context.SaveChanges();

                    ClearLog();
                });

        private class Person17644
        {
            public int Id { get; set; }
            public string Name { set; get; }
            public int Age { set; get; }
        }

        private class PersonView17644
        {
            public string Name { set; get; }
        }

        private class PersonDetailView17644 : PersonView17644
        {
            public int Age { set; get; }
        }

        #endregion

        #region Bug11023

        [ConditionalFact]
        public virtual async Task Async_correlated_projection_with_first()
        {
            using (CreateDatabase11023())
            {
                using var context = new MyContext11023(_options);
                var query = await context.Entities
                    .Select(e => new { ThingIds = e.Values.First().Things.Select(t => t.Subthing.ThingId).ToList() })
                    .ToListAsync();

                var result = Assert.Single(query);
                Assert.Equal(new[] { 1, 2 }, result.ThingIds);

                AssertSql(
                    @"SELECT [e].[Id], [t0].[ThingId], [t0].[Id]
FROM [Entities] AS [e]
OUTER APPLY (
    SELECT [s].[ThingId], [t].[Id]
    FROM [Things] AS [t]
    LEFT JOIN [Subthings] AS [s] ON [t].[Id] = [s].[ThingId]
    WHERE (
        SELECT TOP(1) [v].[Id]
        FROM [Values] AS [v]
        WHERE [e].[Id] = [v].[Entity11023Id]) IS NOT NULL AND (((
        SELECT TOP(1) [v0].[Id]
        FROM [Values] AS [v0]
        WHERE [e].[Id] = [v0].[Entity11023Id]) = [t].[Value11023Id]) OR ((
        SELECT TOP(1) [v0].[Id]
        FROM [Values] AS [v0]
        WHERE [e].[Id] = [v0].[Entity11023Id]) IS NULL AND [t].[Value11023Id] IS NULL))
) AS [t0]
ORDER BY [e].[Id], [t0].[Id]");
            }
        }

        private class MyContext11023 : DbContext
        {
            public DbSet<Entity11023> Entities { get; set; }
            public DbSet<Value11023> Values { get; set; }
            public DbSet<Thing11023> Things { get; set; }
            public DbSet<Subthing11023> Subthings { get; set; }

            public MyContext11023(DbContextOptions options)
                : base(options)
            {
            }
        }

        private SqlServerTestStore CreateDatabase11023()
            => CreateTestStore(
                () => new MyContext11023(_options),
                context =>
                {
                    context.Add(
                        new Entity11023
                        {
                            Values = new List<Value11023>
                            {
                                new Value11023
                                {
                                    Things = new List<Thing11023>
                                    {
                                        new Thing11023 { Subthing = new Subthing11023() },
                                        new Thing11023 { Subthing = new Subthing11023() }
                                    }
                                }
                            }
                        });

                    context.SaveChanges();

                    ClearLog();
                });

        private class Entity11023
        {
            public int Id { get; set; }
            public ICollection<Value11023> Values { get; set; }
        }

        private class Value11023
        {
            public int Id { get; set; }
            public ICollection<Thing11023> Things { get; set; }
        }

        private class Thing11023
        {
            public int Id { get; set; }
            public Subthing11023 Subthing { get; set; }
        }

        private class Subthing11023
        {
            public int Id { get; set; }
            public int ThingId { get; set; }
            public Thing11023 Thing { get; set; }
        }

        #endregion

        #region Issue7973

        [ConditionalFact]
        public virtual void SelectMany_with_collection_selector_having_subquery()
        {
            using (CreateDatabase7973())
            {
                using var context = new MyContext7973(_options);
                var users = (from user in context.Users
                             from organisation in context.Organisations.Where(o => o.OrganisationUsers.Any()).DefaultIfEmpty()
                             select new { UserId = user.Id, OrgId = organisation.Id }).ToList();

                Assert.Equal(2, users.Count);

                AssertSql(
                    @"SELECT [u].[Id] AS [UserId], [t0].[Id] AS [OrgId]
FROM [Users] AS [u]
CROSS JOIN (
    SELECT [t].[Id]
    FROM (
        SELECT NULL AS [empty]
    ) AS [empty]
    LEFT JOIN (
        SELECT [o].[Id]
        FROM [Organisations] AS [o]
        WHERE EXISTS (
            SELECT 1
            FROM [OrganisationUser7973] AS [o0]
            WHERE [o].[Id] = [o0].[OrganisationId])
    ) AS [t] ON 1 = 1
) AS [t0]");
            }
        }

        private class MyContext7973 : DbContext
        {
            public DbSet<User7973> Users { get; set; }
            public DbSet<Organisation7973> Organisations { get; set; }

            public MyContext7973(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrganisationUser7973>().HasKey(ou => new { ou.OrganisationId, ou.UserId });
                modelBuilder.Entity<OrganisationUser7973>().HasOne(ou => ou.Organisation).WithMany(o => o.OrganisationUsers)
                    .HasForeignKey(ou => ou.OrganisationId);
                modelBuilder.Entity<OrganisationUser7973>().HasOne(ou => ou.User).WithMany(u => u.OrganisationUsers)
                    .HasForeignKey(ou => ou.UserId);
            }
        }

        private SqlServerTestStore CreateDatabase7973()
            => CreateTestStore(
                () => new MyContext7973(_options),
                context =>
                {
                    context.AddRange(
                        new OrganisationUser7973 { Organisation = new Organisation7973(), User = new User7973() },
                        new Organisation7973(),
                        new User7973());

                    context.SaveChanges();
                    ClearLog();
                });

        private class User7973
        {
            public int Id { get; set; }
            public List<OrganisationUser7973> OrganisationUsers { get; set; }
        }

        private class Organisation7973
        {
            public int Id { get; set; }
            public List<OrganisationUser7973> OrganisationUsers { get; set; }
        }

        private class OrganisationUser7973
        {
            public int OrganisationId { get; set; }
            public Organisation7973 Organisation { get; set; }

            public int UserId { get; set; }
            public User7973 User { get; set; }
        }

        #endregion

        #region Issue10447

        [ConditionalFact]
        public virtual void Nested_include_queries_do_not_populate_navigation_twice()
        {
            using (CreateDatabase10447())
            {
                using var context = new MyContext10447(_options);
                var query = context.Blogs.Include(b => b.Posts);

                foreach (var blog in query)
                {
                    query.ToList();
                }

                Assert.Collection(
                    query,
                    b => Assert.Equal(3, b.Posts.Count),
                    b => Assert.Equal(2, b.Posts.Count),
                    b => Assert.Single(b.Posts));
            }
        }

        private class MyContext10447 : DbContext
        {
            public DbSet<Blog10447> Blogs { get; set; }

            public MyContext10447(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
            }
        }

        private SqlServerTestStore CreateDatabase10447()
            => CreateTestStore(
                () => new MyContext10447(_options),
                context =>
                {
                    context.AddRange(
                        new Blog10447
                        {
                            Posts = new List<Post10447>
                            {
                                new Post10447(),
                                new Post10447(),
                                new Post10447()
                            }
                        },
                        new Blog10447 { Posts = new List<Post10447> { new Post10447(), new Post10447() } },
                        new Blog10447 { Posts = new List<Post10447> { new Post10447() } });

                    context.SaveChanges();
                    ClearLog();
                });

        private class Blog10447
        {
            public int Id { get; set; }
            public List<Post10447> Posts { get; set; }
        }

        private class Post10447
        {
            public int Id { get; set; }

            public Blog10447 Blog { get; set; }
        }

        #endregion

        #region Issue12456

        [ConditionalFact]
        public virtual void Let_multiple_references_with_reference_to_outer()
        {
            using (CreateDatabase12456())
            {
                using var context = new MyContext12456(_options);
                var users = (from a in context.Activities
                             let cs = context.CompetitionSeasons
                                 .First(s => s.StartDate <= a.DateTime && a.DateTime < s.EndDate)
                             select new { cs.Id, Points = a.ActivityType.Points.Where(p => p.CompetitionSeason == cs) }).ToList();

                AssertSql(
                    @"SELECT (
    SELECT TOP(1) [c].[Id]
    FROM [CompetitionSeasons] AS [c]
    WHERE ([c].[StartDate] <= [a].[DateTime]) AND ([a].[DateTime] < [c].[EndDate])), [a].[Id], [a0].[Id], [t].[Id], [t].[ActivityTypeId], [t].[CompetitionSeasonId], [t].[Points], [t].[Id0]
FROM [Activities] AS [a]
INNER JOIN [ActivityType12456] AS [a0] ON [a].[ActivityTypeId] = [a0].[Id]
OUTER APPLY (
    SELECT [a1].[Id], [a1].[ActivityTypeId], [a1].[CompetitionSeasonId], [a1].[Points], [c0].[Id] AS [Id0]
    FROM [ActivityTypePoints12456] AS [a1]
    INNER JOIN [CompetitionSeasons] AS [c0] ON [a1].[CompetitionSeasonId] = [c0].[Id]
    WHERE ([c0].[Id] = (
        SELECT TOP(1) [c1].[Id]
        FROM [CompetitionSeasons] AS [c1]
        WHERE ([c1].[StartDate] <= [a].[DateTime]) AND ([a].[DateTime] < [c1].[EndDate]))) AND ([a0].[Id] = [a1].[ActivityTypeId])
) AS [t]
ORDER BY [a].[Id], [a0].[Id], [t].[Id], [t].[Id0]");
            }
        }

        [ConditionalFact]
        public virtual void Let_multiple_references_with_reference_to_outer_2()
        {
            using (CreateDatabase12456())
            {
                using var context = new MyContext12456(_options);
                var users = context.Activities
                    .Select(
                        a => new
                        {
                            Activity = a,
                            CompetitionSeason = context.CompetitionSeasons
                                .First(s => s.StartDate <= a.DateTime && a.DateTime < s.EndDate)
                        })
                    .Select(
                        a => new
                        {
                            a.Activity,
                            CompetitionSeasonId = a.CompetitionSeason.Id,
                            Points = a.Activity.Points
                                ?? a.Activity.ActivityType.Points
                                    .Where(p => p.CompetitionSeason == a.CompetitionSeason)
                                    .Select(p => p.Points).SingleOrDefault()
                        }).ToList();

                AssertSql(
                    @"SELECT [a0].[Id], [a0].[ActivityTypeId], [a0].[DateTime], [a0].[Points], (
    SELECT TOP(1) [c].[Id]
    FROM [CompetitionSeasons] AS [c]
    WHERE ([c].[StartDate] <= [a0].[DateTime]) AND ([a0].[DateTime] < [c].[EndDate])) AS [CompetitionSeasonId], COALESCE([a0].[Points], (
    SELECT TOP(1) [a].[Points]
    FROM [ActivityTypePoints12456] AS [a]
    INNER JOIN [CompetitionSeasons] AS [c0] ON [a].[CompetitionSeasonId] = [c0].[Id]
    WHERE ([a1].[Id] = [a].[ActivityTypeId]) AND ([c0].[Id] = (
        SELECT TOP(1) [c1].[Id]
        FROM [CompetitionSeasons] AS [c1]
        WHERE ([c1].[StartDate] <= [a0].[DateTime]) AND ([a0].[DateTime] < [c1].[EndDate]))))) AS [Points]
FROM [Activities] AS [a0]
INNER JOIN [ActivityType12456] AS [a1] ON [a0].[ActivityTypeId] = [a1].[Id]");
            }
        }

        private class MyContext12456 : DbContext
        {
            public DbSet<Activity12456> Activities { get; set; }
            public DbSet<CompetitionSeason12456> CompetitionSeasons { get; set; }

            public MyContext12456(DbContextOptions options)
                : base(options)
            {
            }
        }

        private SqlServerTestStore CreateDatabase12456()
            => CreateTestStore(
                () => new MyContext12456(_options),
                context =>
                {
                    ClearLog();
                });

        private class CompetitionSeason12456
        {
            public int Id { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public List<ActivityTypePoints12456> ActivityTypePoints { get; set; }
        }

        private class Point12456
        {
            public int Id { get; set; }
            public CompetitionSeason12456 CompetitionSeason { get; set; }
            public int? Points { get; set; }
        }

        private class ActivityType12456
        {
            public int Id { get; set; }
            public List<ActivityTypePoints12456> Points { get; set; }
        }

        private class ActivityTypePoints12456
        {
            public int Id { get; set; }
            public int ActivityTypeId { get; set; }
            public int CompetitionSeasonId { get; set; }
            public int Points { get; set; }

            public ActivityType12456 ActivityType { get; set; }
            public CompetitionSeason12456 CompetitionSeason { get; set; }
        }

        private class Activity12456
        {
            public int Id { get; set; }
            public int ActivityTypeId { get; set; }
            public DateTime DateTime { get; set; }
            public int? Points { get; set; }
            public ActivityType12456 ActivityType { get; set; }
        }

        #endregion

        #region Issue15137

        [ConditionalFact]
        public virtual async Task Run_something()
        {
            using (CreateDatabase15137())
            {
                using var context = new MyContext15137(_options);
                var container = await context.Trades
                    .Select(
                        x => new
                        {
                            x.Id,
                            Assets = x.Assets.AsQueryable()
                                .Select(
                                    y => new
                                    {
                                        y.Id,
                                        Contract = new
                                        {
                                            y.Contract.Id,
                                            Season = new
                                            {
                                                y.Contract.Season.Id,
                                                IsPastTradeDeadline =
                                                    (y.Contract.Season.Games.Max(z => (int?)z.GameNumber) ?? 0) > 10
                                            }
                                        }
                                    })
                                .ToList()
                        })
                    .SingleAsync();

                AssertSql(
                    @"SELECT [t0].[Id], [t1].[Id], [t1].[Id0], [t1].[Id1], [t1].[c]
FROM (
    SELECT TOP(2) [t].[Id]
    FROM [Trades] AS [t]
) AS [t0]
LEFT JOIN (
    SELECT [d0].[Id], [d1].[Id] AS [Id0], [d2].[Id] AS [Id1], CASE
        WHEN COALESCE((
            SELECT MAX([d].[GameNumber])
            FROM [DbGame] AS [d]
            WHERE [d2].[Id] IS NOT NULL AND ([d2].[Id] = [d].[SeasonId])), 0) > 10 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [c], [d0].[DbTradeId]
    FROM [DbTradeAsset] AS [d0]
    INNER JOIN [DbContract] AS [d1] ON [d0].[ContractId] = [d1].[Id]
    LEFT JOIN [DbSeason] AS [d2] ON [d1].[SeasonId] = [d2].[Id]
) AS [t1] ON [t0].[Id] = [t1].[DbTradeId]
ORDER BY [t0].[Id], [t1].[Id], [t1].[Id0]");
            }
        }

        private class MyContext15137 : DbContext
        {
            public DbSet<DbTrade> Trades { get; set; }

            public MyContext15137(DbContextOptions options)
                : base(options)
            {
            }
        }

        private SqlServerTestStore CreateDatabase15137()
            => CreateTestStore(
                () => new MyContext15137(_options),
                context =>
                {
                    var dbTrade = new DbTrade
                    {
                        Assets = new List<DbTradeAsset>
                        {
                            new DbTradeAsset
                            {
                                Contract = new DbContract
                                {
                                    Season = new DbSeason { Games = new List<DbGame> { new DbGame { GameNumber = 1 } } }
                                }
                            }
                        }
                    };

                    context.Trades.Add(dbTrade);
                    context.SaveChanges();

                    ClearLog();
                });

        private class DbTrade
        {
            public int Id { get; set; }
            public List<DbTradeAsset> Assets { get; set; }
        }

        private class DbTradeAsset
        {
            public int Id { get; set; }
            public int ContractId { get; set; }

            public DbContract Contract { get; set; }
        }

        private class DbContract
        {
            public int Id { get; set; }

            public DbSeason Season { get; set; }
        }

        private class DbSeason
        {
            public int Id { get; set; }

            public List<DbGame> Games { get; set; }
        }

        private class DbGame
        {
            public int Id { get; set; }
            public int GameNumber { get; set; }

            public DbSeason Season { get; set; }
        }

        #endregion

        #region Issue13517

        [ConditionalFact]
        public void Query_filter_with_pk_fk_optimization_bug_13517()
        {
            using var _ = CreateDatabase13517();
            using var context = new BugContext13517(_options);

            context.Entities.Select(
                s =>
                    new BugEntityDto13517
                    {
                        Id = s.Id,
                        RefEntity = s.RefEntity == null
                            ? null
                            : new BugRefEntityDto13517 { Id = s.RefEntity.Id, Public = s.RefEntity.Public },
                        RefEntityId = s.RefEntityId
                    }).Single(p => p.Id == 1);

            AssertSql(
                @"SELECT TOP(2) [e].[Id], CASE
    WHEN [t].[Id] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [t].[Id], [t].[Public], [e].[RefEntityId]
FROM [Entities] AS [e]
LEFT JOIN (
    SELECT [r].[Id], [r].[Public]
    FROM [RefEntities] AS [r]
    WHERE [r].[Public] = CAST(1 AS bit)
) AS [t] ON [e].[RefEntityId] = [t].[Id]
WHERE [e].[Id] = 1");
        }

        private SqlServerTestStore CreateDatabase13517()
            => CreateTestStore(
                () => new BugContext13517(_options),
                context =>
                {
                    var refEntity = new BugRefEntity13517 { Public = false };
                    context.RefEntities.Add(refEntity);
                    context.Entities.Add(new BugEntity13517 { RefEntity = refEntity });
                    context.SaveChanges();

                    ClearLog();
                });

        private class BugEntity13517
        {
            public int Id { get; set; }
            public int? RefEntityId { get; set; }
            public BugRefEntity13517 RefEntity { get; set; }
        }

        private class BugRefEntity13517
        {
            public int Id { get; set; }
            public bool Public { get; set; }
        }

        private class BugEntityDto13517
        {
            public int Id { get; set; }
            public int? RefEntityId { get; set; }
            public BugRefEntityDto13517 RefEntity { get; set; }
        }

        private class BugRefEntityDto13517
        {
            public int Id { get; set; }
            public bool Public { get; set; }
        }

        private class BugContext13517 : DbContext
        {
            public DbSet<BugEntity13517> Entities { get; set; }
            public DbSet<BugRefEntity13517> RefEntities { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<BugRefEntity13517>().HasQueryFilter(f => f.Public);
            }

            public BugContext13517(DbContextOptions options)
                : base(options)
            {
            }
        }

        #endregion

        #region Issue17794

        [ConditionalFact]
        public void Double_convert_interface_created_expression_tree()
        {
            using var _ = CreateDatabase17794();
            using var context = new BugContext17794(_options);

            var expression = HasAction17794<Offer17794>(OfferActions17794.Accepted);
            var query = context.Offers.Where(expression).Count();

            Assert.Equal(1, query);

            AssertSql(
                @"@__action_0='1'

SELECT COUNT(*)
FROM [Offers] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM [OfferActions] AS [o0]
    WHERE ([o].[Id] = [o0].[OfferId]) AND ([o0].[Action] = @__action_0))");
        }

        private SqlServerTestStore CreateDatabase17794()
            => CreateTestStore(
                () => new BugContext17794(_options),
                context =>
                {
                    context.Add(new Offer17794
                    {
                        Actions = new List<OfferAction17794>
                        {
                            new OfferAction17794
                            {
                                Action = OfferActions17794.Accepted
                            }
                        }
                    });

                    context.SaveChanges();

                    ClearLog();
                });

        private static Expression<Func<T, bool>> HasAction17794<T>(OfferActions17794 action)
            where T : IOffer17794
        {
            Expression<Func<OfferAction17794, bool>> predicate = oa => oa.Action == action;

            return v => v.Actions.AsQueryable().Any(predicate);
        }

        private interface IOffer17794
        {
            ICollection<OfferAction17794> Actions { get; set; }
        }

        private class Offer17794 : IOffer17794
        {
            public int Id { get; set; }

            public ICollection<OfferAction17794> Actions { get; set; }
        }

        private enum OfferActions17794 : int
        {
            Accepted = 1,
            Declined = 2
        }

        private class OfferAction17794
        {
            public int Id { get; set; }

            [Required]
            public Offer17794 Offer { get; set; }
            public int OfferId { get; set; }

            [Required]
            public OfferActions17794 Action { get; set; }
        }

        private class BugContext17794 : DbContext
        {
            public DbSet<Offer17794> Offers { get; set; }
            public DbSet<OfferAction17794> OfferActions { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {

            }

            public BugContext17794(DbContextOptions options)
                : base(options)
            {
            }
        }

        #endregion

        #region Issue18087

        [ConditionalFact]
        public void Cast_to_implemented_interface_is_removed_from_expression_tree()
        {
            using var _ = CreateDatabase18087();
            using var context = new BugContext18087(_options);

            var queryBase = (IQueryable)context.MockEntities;
            var id = 1;
            var query = queryBase.Cast<IDomainEntity>().FirstOrDefault(x => x.Id == id);

            Assert.Equal(1, query.Id);

            AssertSql(
                @"@__id_0='1'

SELECT TOP(1) [m].[Id], [m].[Name], [m].[NavigationEntityId]
FROM [MockEntities] AS [m]
WHERE [m].[Id] = @__id_0");
        }

        [ConditionalFact]
        public void Cast_to_object_is_removed_from_expression_tree()
        {
            using var _ = CreateDatabase18087();
            using var context = new BugContext18087(_options);

            var queryBase = (IQueryable)context.MockEntities;
            var query = queryBase.Cast<object>().Count();

            Assert.Equal(3, query);

            AssertSql(
                @"SELECT COUNT(*)
FROM [MockEntities] AS [m]");
        }

        [ConditionalFact]
        public void Cast_to_non_implemented_interface_is_not_removed_from_expression_tree()
        {
            using var _ = CreateDatabase18087();
            using var context = new BugContext18087(_options);

            var queryBase = (IQueryable)context.MockEntities;
            var id = 1;

            var message = Assert.Throws<InvalidOperationException>(
                () => queryBase.Cast<IDummyEntity>().FirstOrDefault(x => x.Id == id)).Message;

            Assert.Equal(
                CoreStrings.TranslationFailed(@"DbSet<MockEntity>    .Cast<IDummyEntity>()    .Where(e => e.Id == __id_0)"),
                message.Replace("\r", "").Replace("\n", ""));
        }

        private SqlServerTestStore CreateDatabase18087()
            => CreateTestStore(
                () => new BugContext18087(_options),
                context =>
                {
                    context.AddRange(new MockEntity()
                    {
                        Name = "Entity1",
                        NavigationEntity = null
                    },
                    new MockEntity()
                    {
                        Name = "Entity2",
                        NavigationEntity = null
                    },
                    new MockEntity()
                    {
                        Name = "NewEntity",
                        NavigationEntity = null
                    });

                    context.SaveChanges();

                    ClearLog();
                });

        private interface IDomainEntity
        {
            int Id { get; set; }
        }

        private interface IDummyEntity
        {
            int Id { get; set; }
        }

        private class MockEntity : IDomainEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public MockEntity NavigationEntity { get; set; }
        }

        private class BugContext18087 : DbContext
        {
            public BugContext18087(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<MockEntity> MockEntities { get; set; }
        }

        #endregion

        #region Issue18759

        [ConditionalFact]
        public void Query_filter_with_null_constant()
        {
            using var _ = CreateDatabase18759();
            using var context = new BugContext18759(_options);

            var people = context.People.ToList();

            AssertSql(
                @"SELECT [p].[Id], [p].[UserDeleteId]
FROM [People] AS [p]
LEFT JOIN [User18759] AS [u] ON [p].[UserDeleteId] = [u].[Id]
WHERE [u].[Id] IS NOT NULL");
        }

        private SqlServerTestStore CreateDatabase18759()
            => CreateTestStore(
                () => new BugContext18759(_options),
                context =>
                {
                    ClearLog();
                });

        private class Person18759
        {
            public int Id { get; set; }
            public User18759 UserDelete { get; set; }
        }

        private class User18759
        {
            public int Id { get; set; }
        }

        private class BugContext18759 : DbContext
        {
            public DbSet<Person18759> People { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Person18759>().HasQueryFilter(p => p.UserDelete != null);

            public BugContext18759(DbContextOptions options)
                : base(options)
            {
            }
        }

        #endregion

        #region Issue19138

        [ConditionalFact]
        public void Accessing_scalar_property_in_derived_type_projection_does_not_load_owned_navigations()
        {
            using var _ = CreateDatabase19138();
            using var context = new BugContext19138(_options);

            var result = context.BaseEntities
                .Select(b => context.OtherEntities.Where(o => o.OtherEntityData == ((SubEntity19138)b).Data).FirstOrDefault())
                .ToList();

            Assert.Equal("A", Assert.Single(result).OtherEntityData);

            AssertSql(
                @"SELECT [t0].[Id], [t0].[OtherEntityData]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [t].[Id], [t].[OtherEntityData]
    FROM (
        SELECT [o].[Id], [o].[OtherEntityData], ROW_NUMBER() OVER(PARTITION BY [o].[OtherEntityData] ORDER BY [o].[Id]) AS [row]
        FROM [OtherEntities] AS [o]
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [b].[Data] = [t0].[OtherEntityData]
WHERE [b].[Discriminator] IN (N'BaseEntity19138', N'SubEntity19138')");
        }

        private SqlServerTestStore CreateDatabase19138()
            => CreateTestStore(
                () => new BugContext19138(_options),
                context =>
                {
                    context.Add(new OtherEntity19138 { OtherEntityData = "A" });
                    context.Add(new SubEntity19138 { Data = "A" });

                    context.SaveChanges();

                    ClearLog();
                });

        private class BaseEntity19138
        {
            public int Id { get; set; }
        }

        private class SubEntity19138 : BaseEntity19138
        {
            public string Data { get; set; }
            public Owned19138 Owned { get; set; }
        }

        private class Owned19138
        {
            public string OwnedData { get; set; }
        }

        private class OtherEntity19138
        {
            public int Id { get; set; }
            public string OtherEntityData { get; set; }
        }

        private class BugContext19138 : DbContext
        {
            public DbSet<BaseEntity19138> BaseEntities { get; set; }
            public DbSet<OtherEntity19138> OtherEntities { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<BaseEntity19138>();
                modelBuilder.Entity<SubEntity19138>().OwnsOne(se => se.Owned);
                modelBuilder.Entity<OtherEntity19138>();
            }

            public BugContext19138(DbContextOptions options)
                : base(options)
            {
            }
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
