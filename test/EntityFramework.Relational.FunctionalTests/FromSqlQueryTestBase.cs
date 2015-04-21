// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Microsoft.Data.Entity.Tests;
using Xunit;

namespace Microsoft.Data.Entity.Relational.FunctionalTests
{
    public abstract class FromSqlQueryTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryFixtureBase, new()
    {
        [Fact]
        public virtual void From_sql_queryable_simple()
        {
            AssertQuery<Customer>(
                cs => cs.FromSql("SELECT * FROM Customers"),
                cs => cs,
                entryCount: 91);
        }

        [Fact]
        public virtual void From_sql_queryable_filter()
        {
            AssertQuery<Customer>(
                cs => cs.FromSql("SELECT * FROM Customers WHERE Customers.ContactName LIKE '%z%'"),
                cs => cs.Where(c => c.ContactName.Contains("z")),
                entryCount: 14);
        }

        [Fact]
        public virtual void From_sql_queryable_composed()
        {
            AssertQuery<Customer>(
                cs => cs.FromSql("SELECT * FROM Customers").Where(c => c.ContactName.Contains("z")),
                cs => cs.Where(c => c.ContactName.Contains("z")),
                entryCount: 14);
        }

        [Fact]
        public virtual void From_sql_queryable_multiple_line_query()
        {
            AssertQuery<Customer>(
                cs => cs.FromSql(@"SELECT *
FROM Customers
WHERE Customers.City = 'London'"),
                cs => cs.Where(c => c.City == "London"),
                entryCount: 6);
        }

        [Fact]
        public virtual void From_sql_queryable_composed_multiple_line_query()
        {
            AssertQuery<Customer>(
                cs => cs.FromSql(@"SELECT *
FROM Customers").Where(c => c.City == "London"),
                cs => cs.Where(c => c.City == "London"),
                entryCount: 6);
        }

        [Fact]
        public virtual void From_sql_queryable_with_parameters()
        {
            var city = "London";
            var contactTitle = "Sales Representative";

            AssertQuery<Customer>(
                cs => cs.FromSql(@"SELECT * FROM Customers WHERE City = {0} AND ContactTitle = {1}", city, contactTitle),
                cs => cs.Where(c => c.City == city && c.ContactTitle == contactTitle),
                entryCount: 3);
        }

        [Fact]
        public virtual void From_sql_queryable_with_parameters_and_closure()
        {
            var city = "London";
            var contactTitle = "Sales Representative";

            AssertQuery<Customer>(
                cs => cs.FromSql(@"SELECT * FROM Customers WHERE City = {0}", city).Where(c => c.ContactTitle == contactTitle),
                cs => cs.Where(c => c.City == city && c.ContactTitle == contactTitle),
                entryCount: 3);
        }

        [Fact]
        public virtual void From_sql_queryable_simple_cache_key_includes_query_string()
        {
            AssertQuery<Customer>(
                cs => cs.FromSql("SELECT * FROM Customers WHERE Customers.City = 'London'"),
                cs => cs.Where(c => c.City == "London"),
                entryCount: 6);

            AssertQuery<Customer>(
                cs => cs.FromSql("SELECT * FROM Customers WHERE Customers.City = 'Seattle'"),
                cs => cs.Where(c => c.City == "Seattle"),
                entryCount: 1);
        }

        [Fact]
        public virtual void From_sql_queryable_with_parameters_cache_key_includes_parameters()
        {
            var city = "London";
            var contactTitle = "Sales Representative";
            var sql = @"SELECT * FROM Customers WHERE City = {0} AND ContactTitle = {1}";

            AssertQuery<Customer>(
                cs => cs.FromSql(sql, city, contactTitle),
                cs => cs.Where(c => c.City == city && c.ContactTitle == contactTitle),
                entryCount: 3);

            city = "Madrid";
            contactTitle = "Accounting Manager";

            AssertQuery<Customer>(
                cs => cs.FromSql(sql, city, contactTitle),
                cs => cs.Where(c => c.City == city && c.ContactTitle == contactTitle),
                entryCount: 2);
        }

        [Fact]
        public virtual void From_sql_queryable_simple_as_no_tracking_not_composed()
        {
            AssertQuery<Customer>(
                cs => cs.FromSql("SELECT * FROM Customers").AsNoTracking(),
                cs => cs,
                entryCount: 0);
        }

        [Fact]
        public virtual void From_sql_queryable_simple_include()
        {
            AssertQuery<Customer>(
                cs => cs.FromSql("SELECT * FROM Customers").Include(c => c.Orders),
                cs => cs,
                entryCount: 921);
        }

        [Fact]
        public virtual void From_sql_queryable_simple_composed_include()
        {
            AssertQuery<Customer>(
                cs => cs.FromSql("SELECT * FROM Customers").Where(c => c.City == "London").Include(c => c.Orders),
                cs => cs.Where(c => c.City == "London"),
                entryCount: 52);
        }

        //[Fact]
        public virtual void From_sql_queryable_stored_procedure()
        {
            AssertQuery<Product>(
                cs => cs.FromSql("EXEC ProductsOnOrder"),
                cs => cs.Where(p => p.UnitsOnOrder > 0),
                entryCount: 17);
        }

        //[Fact]
        public virtual void From_sql_queryable_simple_composed_stored_procedure()
        {
            AssertQuery<Product>(
                cs => cs.FromSql(@"EXEC ProductsOnOrder").Where(p => p.UnitsInStock > 10),
                cs => cs.Where(p => p.UnitsOnOrder > 0 && p.UnitsInStock > 10),
                entryCount: 9);
        }

        //[Fact]
        public virtual void From_sql_queryable_composed_stored_procedure()
        {
            AssertQuery<Product>(
                cs => cs.FromSql(@"EXEC ProductsOnOrder").OrderBy(p => p.ProductID).Where(p => p.UnitsInStock > 10).Count(p => p.ProductName.Contains("e")),
                cs => cs.OrderBy(p => p.ProductID).Where(p => p.UnitsOnOrder > 0 && p.UnitsInStock > 10).Count(p => p.ProductName.Contains("e")));
        }

        [Fact]
        public virtual void From_sql_queryable_stored_procedure_with_include_throws()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    Strings.StoredProcedureIncludeNotSupported,
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            context.Set<Product>()
                                .FromSql("exec ProductsOnOrder")
                                .Include(p => p.OrderDetails)
                                .ToArray()
                        ).Message);
            }
        }

        [Fact]
        public virtual void From_sql_annotations_do_not_affect_successive_calls()
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    NorthwindData.Set<Customer>().Where(c => c.ContactName.Contains("z")).ToArray(),
                    context.Customers.FromSql("SELECT * FROM Customers WHERE Customers.ContactName LIKE '%z%'").ToArray(),
                    assertOrder: false);

                Assert.Equal(14, context.ChangeTracker.Entries().Count());

                TestHelpers.AssertResults(
                    NorthwindData.Set<Customer>().ToArray(),
                    context.Customers.ToArray(),
                    assertOrder: false);

                Assert.Equal(91, context.ChangeTracker.Entries().Count());
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

        private void AssertQuery<TItem>(
            Func<DbSet<TItem>, IQueryable<object>> relationalQuery,
            Func<IQueryable<TItem>, IQueryable<object>> l2oQuery,
            bool assertOrder = false,
            int entryCount = 0)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    l2oQuery(NorthwindData.Set<TItem>()).ToArray(),
                    relationalQuery(context.Set<TItem>()).ToArray(),
                    assertOrder);

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        private void AssertQuery<TItem>(
            Func<DbSet<TItem>, int> relationalQuery,
            Func<IQueryable<TItem>, int> l2oQuery,
            bool assertOrder = false)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { l2oQuery(NorthwindData.Set<TItem>()) },
                    new[] { relationalQuery(context.Set<TItem>()) },
                    assertOrder);
            }
        }
    }
}
