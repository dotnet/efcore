// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Microsoft.Data.Entity.Relational.Internal;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class FromSqlQueryTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryFixtureBase, new()
    {
        [Fact]
        public virtual void From_sql_queryable_simple()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>()
                    .FromSql("SELECT * FROM Customers WHERE Customers.ContactName LIKE '%z%'")
                    .ToArray();

                Assert.Equal(14, actual.Length);
                Assert.Equal(14, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual void From_sql_queryable_simple_columns_out_of_order()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>()
                    .FromSql("SELECT [Region], [PostalCode], [Phone], [Fax], [CustomerID], [Country], [ContactTitle], [ContactName], [CompanyName], [City], [Address] FROM Customers")
                    .ToArray();

                Assert.Equal(91, actual.Length);
                Assert.Equal(91, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual void From_sql_queryable_simple_columns_out_of_order_and_extra_columns()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>()
                    .FromSql("SELECT [Region], [PostalCode], [PostalCode] AS Foo, [Phone], [Fax], [CustomerID], [Country], [ContactTitle], [ContactName], [CompanyName], [City], [Address] FROM Customers")
                    .ToArray();

                Assert.Equal(91, actual.Length);
                Assert.Equal(91, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual void From_sql_queryable_simple_columns_out_of_order_and_not_enough_columns_throws()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    Strings.FromSqlMissingColumn("Region"),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            context.Set<Customer>()
                                .FromSql("SELECT [PostalCode], [Phone], [Fax], [CustomerID], [Country], [ContactTitle], [ContactName], [CompanyName], [City], [Address] FROM Customers")
                                .ToArray()
                        ).Message);
            }
        }

        [Fact]
        public virtual void From_sql_queryable_composed()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>()
                    .FromSql("SELECT * FROM Customers")
                    .Where(c => c.ContactName.Contains("z"))
                    .ToArray();

                Assert.Equal(14, actual.Length);
            }
        }

        [Fact]
        public virtual void From_sql_queryable_multiple_composed()
        {
            using (var context = CreateContext())
            {
                var actual
                    = (from c in context.Set<Customer>().FromSql("SELECT * FROM Customers")
                        from o in context.Set<Order>().FromSql("SELECT * FROM Orders")
                        where c.CustomerID == o.CustomerID
                        select new { c, o })
                        .ToArray();

                Assert.Equal(830, actual.Length);
            }
        }

        [Fact]
        public virtual void From_sql_queryable_multiple_composed_with_closure_parameters()
        {
            var startDate = new DateTime(1997, 1, 1);
            var endDate = new DateTime(1998, 1, 1);

            using (var context = CreateContext())
            {
                var actual
                    = (from c in context.Set<Customer>().FromSql(@"SELECT * FROM Customers")
                        from o in context.Set<Order>().FromSql("SELECT * FROM Orders WHERE OrderDate BETWEEN {0} AND {1}",
                            startDate,
                            endDate)
                        where c.CustomerID == o.CustomerID
                        select new { c, o })
                        .ToArray();

                Assert.Equal(411, actual.Length);
            }
        }

        [Fact]
        public virtual void From_sql_queryable_multiple_composed_with_parameters_and_closure_parameters()
        {
            var city = "London";
            var startDate = new DateTime(1997, 1, 1);
            var endDate = new DateTime(1998, 1, 1);

            using (var context = CreateContext())
            {
                var actual
                    = (from c in context.Set<Customer>().FromSql(@"SELECT * FROM Customers WHERE City = {0}",
                        city)
                        from o in context.Set<Order>().FromSql("SELECT * FROM Orders WHERE OrderDate BETWEEN {0} AND {1}",
                            startDate,
                            endDate)
                        where c.CustomerID == o.CustomerID
                        select new { c, o })
                        .ToArray();

                Assert.Equal(25, actual.Length);
            }
        }

        [Fact]
        public virtual void From_sql_queryable_multiple_line_query()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>()
                    .FromSql(@"SELECT *
FROM Customers
WHERE Customers.City = 'London'")
                    .ToArray();

                Assert.Equal(6, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
            }
        }

        [Fact]
        public virtual void From_sql_queryable_composed_multiple_line_query()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>()
                    .FromSql(@"SELECT *
FROM Customers")
                    .Where(c => c.City == "London")
                    .ToArray();

                Assert.Equal(6, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
            }
        }

        [Fact]
        public virtual void From_sql_queryable_with_parameters()
        {
            var city = "London";
            var contactTitle = "Sales Representative";

            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>()
                    .FromSql(@"SELECT * FROM Customers WHERE City = {0} AND ContactTitle = {1}", city, contactTitle)
                    .ToArray();

                Assert.Equal(3, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
                Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));
            }
        }

        [Fact]
        public virtual void From_sql_queryable_with_parameters_and_closure()
        {
            var city = "London";
            var contactTitle = "Sales Representative";

            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>()
                    .FromSql(@"SELECT * FROM Customers WHERE City = {0}", city)
                    .Where(c => c.ContactTitle == contactTitle)
                    .ToArray();

                Assert.Equal(3, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
                Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));
            }
        }

        [Fact]
        public virtual void From_sql_queryable_simple_cache_key_includes_query_string()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>()
                    .FromSql("SELECT * FROM Customers WHERE Customers.City = 'London'")
                    .ToArray();

                Assert.Equal(6, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));

                actual = context.Set<Customer>()
                    .FromSql("SELECT * FROM Customers WHERE Customers.City = 'Seattle'")
                    .ToArray();

                Assert.Equal(1, actual.Length);
                Assert.True(actual.All(c => c.City == "Seattle"));
            }
        }

        [Fact]
        public virtual void From_sql_queryable_with_parameters_cache_key_includes_parameters()
        {
            var city = "London";
            var contactTitle = "Sales Representative";
            var sql = @"SELECT * FROM Customers WHERE City = {0} AND ContactTitle = {1}";

            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>()
                    .FromSql(sql, city, contactTitle)
                    .ToArray();

                Assert.Equal(3, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
                Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));

                city = "Madrid";
                contactTitle = "Accounting Manager";

                actual = context.Set<Customer>()
                    .FromSql(sql, city, contactTitle)
                    .ToArray();

                Assert.Equal(2, actual.Length);
                Assert.True(actual.All(c => c.City == "Madrid"));
                Assert.True(actual.All(c => c.ContactTitle == "Accounting Manager"));
            }
        }

        [Fact]
        public virtual void From_sql_queryable_simple_as_no_tracking_not_composed()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>()
                    .FromSql("SELECT * FROM Customers")
                    .AsNoTracking()
                    .ToArray();

                Assert.Equal(91, actual.Length);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual void From_sql_queryable_simple_projection_not_composed()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>()
                    .FromSql("SELECT * FROM Customers")
                    .Select(c => new { c.CustomerID, c.City })
                    .AsNoTracking()
                    .ToArray();

                Assert.Equal(91, actual.Length);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual void From_sql_queryable_simple_include()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>()
                    .FromSql("SELECT * FROM Customers")
                    .Include(c => c.Orders)
                    .ToArray();

                Assert.Equal(830, actual.SelectMany(c => c.Orders).Count());
            }
        }

        [Fact]
        public virtual void From_sql_queryable_simple_composed_include()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>()
                    .FromSql("SELECT * FROM Customers")
                    .Where(c => c.City == "London")
                    .Include(c => c.Orders)
                    .ToArray();

                Assert.Equal(46, actual.SelectMany(c => c.Orders).Count());
            }
        }

        [Fact]
        public virtual void From_sql_annotations_do_not_affect_successive_calls()
        {
            using (var context = CreateContext())
            {
                var actual = context.Customers
                    .FromSql("SELECT * FROM Customers WHERE Customers.ContactName LIKE '%z%'")
                    .ToArray();

                Assert.Equal(14, actual.Length);

                actual = context.Customers
                    .ToArray();

                Assert.Equal(91, actual.Length);
            }
        }

        [Fact]
        public virtual void From_sql_composed_with_nullable_predicate()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>()
                    .FromSql("SELECT * FROM Customers")
                    .Where(c => c.ContactName == c.CompanyName)
                    .ToArray();

                Assert.Equal(0, actual.Length);
            }
        }

        [Fact]
        public virtual void From_sql_composed_with_relational_null_comparison()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>()
                    .UseRelationalNullSemantics()
                    .FromSql("SELECT * FROM Customers")
                    .Where(c => c.ContactName == c.CompanyName)
                    .ToArray();

                Assert.Equal(0, actual.Length);
            }
        }

        protected NorthwindContext CreateContext()
        {
            return Fixture.CreateContext();
        }

        protected FromSqlQueryTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }

        protected TFixture Fixture { get; }
    }
}
