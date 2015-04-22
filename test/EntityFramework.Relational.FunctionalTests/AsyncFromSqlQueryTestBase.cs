// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Xunit;

namespace Microsoft.Data.Entity.Relational.FunctionalTests
{
    public abstract class AsyncFromSqlQueryTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryFixtureBase, new()
    {
        [Fact]
        public virtual async Task From_sql_queryable_simple()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>()
                    .FromSql("SELECT * FROM Customers")
                    .ToArrayAsync();

                Assert.Equal(91, actual.Length);
                Assert.Equal(91, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task From_sql_queryable_filter()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>()
                    .FromSql("SELECT * FROM Customers WHERE Customers.ContactName LIKE '%z%'")
                    .ToArrayAsync();

                Assert.Equal(14, actual.Length);
                Assert.Equal(14, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task From_sql_queryable_composed()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>()
                    .FromSql("SELECT * FROM Customers")
                    .Where(c => c.ContactName.Contains("z"))
                    .ToArrayAsync();

                Assert.Equal(14, actual.Length);
            }
        }

        //[Fact]
        public virtual async Task From_sql_queryable_multiple_line_query()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>()
                    .FromSql(@"SELECT *
FROM Customers
WHERE Customers.City = 'London'")
                    .ToArrayAsync();

                Assert.Equal(6, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
            }
        }

        [Fact]
        public virtual async Task From_sql_queryable_composed_multiple_line_query()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>()
                    .FromSql(@"SELECT *
FROM Customers")
                    .Where(c => c.City == "London")
                    .ToArrayAsync();

                Assert.Equal(6, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
            }
        }

        //[Fact]
        public virtual async Task From_sql_queryable_with_columns_reordered()
        {
            using (var context = CreateContext())
            {
                var ascending = (await context.Set<Customer>()
                    .FromSql(@"SELECT
    Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM
    Customers
WHERE
    CustomerID = 'ALFKI'")
                    .ToArrayAsync())
                    .Single();

                var descending = (await context.Set<Customer>()
                    .FromSql(@"SELECT
    Region, PostalCode, Phone, Fax, CustomerID, Country, ContactTitle, ContactName, CompanyName, City, Address
FROM
    Customers
WHERE
    CustomerID = 'ALFKI'")
                    .ToArrayAsync())
                    .Single();

                foreach (var actual in new[] { ascending, descending })
                {
                    Assert.Equal("ALFKI", actual.CustomerID);
                    Assert.Equal("Alfreds Futterkiste", actual.CompanyName);
                    Assert.Equal("Maria Anders", actual.ContactName);
                    Assert.Equal("Sales Representative", actual.ContactTitle);
                    Assert.Equal("Obere Str. 57", actual.Address);
                    Assert.Equal("Berlin", actual.City);
                    Assert.Null(actual.Region);
                    Assert.Equal("12209", actual.PostalCode);
                    Assert.Equal("Germany", actual.Country);
                    Assert.Equal("030-0074321", actual.Phone);
                    Assert.Equal("030-0076545", actual.Fax);
                }
            }
        }

        //[Fact]
        public virtual async Task From_sql_queryable_with_parameters()
        {
            var city = "London";
            var contactTitle = "Sales Representative";

            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>()
                    .FromSql(@"SELECT * FROM Customers WHERE City = {0} AND ContactTitle = {1}", city, contactTitle)
                    .ToArrayAsync();

                Assert.Equal(3, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
                Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));
            }
        }

        [Fact]
        public virtual async Task From_sql_queryable_with_parameters_and_closure()
        {
            var city = "London";
            var contactTitle = "Sales Representative";

            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>()
                    .FromSql(@"SELECT * FROM Customers WHERE City = {0}", city)
                    .Where(c => c.ContactTitle == contactTitle)
                    .ToArrayAsync();

                Assert.Equal(3, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
                Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));
            }
        }

        //[Fact]
        public virtual async Task From_sql_queryable_simple_cache_key_includes_query_string()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>()
                    .FromSql("SELECT * FROM Customers WHERE Customers.City = 'London'")
                    .ToArrayAsync();

                Assert.Equal(6, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));

                actual = await context.Set<Customer>()
                    .FromSql("SELECT * FROM Customers WHERE Customers.City = 'Seattle'")
                    .ToArrayAsync();

                Assert.Equal(1, actual.Length);
                Assert.True(actual.All(c => c.City == "Seattle"));
            }
        }

        //[Fact]
        public virtual async Task From_sql_queryable_with_parameters_cache_key_includes_parameters()
        {
            var city = "London";
            var contactTitle = "Sales Representative";
            var sql = @"SELECT * FROM Customers WHERE City = {0} AND ContactTitle = {1}";

            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>()
                    .FromSql(sql, city, contactTitle)
                    .ToArrayAsync();

                Assert.Equal(3, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
                Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));

                city = "Madrid";
                contactTitle = "Accounting Manager";

                actual = await context.Set<Customer>()
                    .FromSql(sql, city, contactTitle)
                    .ToArrayAsync();

                Assert.Equal(2, actual.Length);
                Assert.True(actual.All(c => c.City == "Madrid"));
                Assert.True(actual.All(c => c.ContactTitle == "Accounting Manager"));
            }
        }

        [Fact]
        public virtual async Task From_sql_queryable_simple_as_no_tracking_not_composed()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>()
                    .FromSql("SELECT * FROM Customers")
                    .AsNoTracking()
                    .ToArrayAsync();

                Assert.Equal(91, actual.Length);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task From_sql_queryable_simple_include()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>()
                    .FromSql("SELECT * FROM Customers")
                    .Include(c => c.Orders)
                    .ToArrayAsync();

                Assert.Equal(830, actual.SelectMany(c => c.Orders).Count());
            }
        }

        [Fact]
        public virtual async Task From_sql_queryable_simple_composed_include()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>()
                    .FromSql("SELECT * FROM Customers")
                    .Where(c => c.City == "London")
                    .Include(c => c.Orders)
                    .ToArrayAsync();

                Assert.Equal(46, actual.SelectMany(c => c.Orders).Count());
            }
        }

        [Fact]
        public virtual async Task From_sql_annotations_do_not_affect_successive_calls()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Customers
                    .FromSql("SELECT * FROM Customers WHERE Customers.ContactName LIKE '%z%'")
                    .ToArrayAsync();

                Assert.Equal(14, actual.Length);

                actual = await context.Customers
                    .ToArrayAsync();

                Assert.Equal(91, actual.Length);
            }
        }

        [Fact]
        public virtual async Task From_sql_composed_with_nullable_predicate()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>()
                    .FromSql("SELECT * FROM Customers")
                    .Where(c => c.ContactName == c.CompanyName)
                    .ToArrayAsync();

                Assert.Equal(0, actual.Length);
            }
        }

        [Fact]
        public virtual async Task From_sql_composed_with_relational_null_comparison()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>()
                    .UseRelationalNullSemantics()
                    .FromSql("SELECT * FROM Customers")
                    .Where(c => c.ContactName == c.CompanyName)
                    .ToArrayAsync();

                Assert.Equal(0, actual.Length);
            }
        }

        protected NorthwindContext CreateContext()
        {
            return Fixture.CreateContext();
        }

        protected AsyncFromSqlQueryTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }

        protected TFixture Fixture { get; }
    }
}
