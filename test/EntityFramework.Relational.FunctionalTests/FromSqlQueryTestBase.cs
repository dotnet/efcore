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
        public virtual void From_sql_queryable_cached_by_query()
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
        public virtual void From_sql_queryable_where_simple_closure_via_query_cache()
        {
            var title = "Sales Associate";

            AssertQuery<Customer>(
                cs => cs.FromSql("SELECT * FROM Customers WHERE Customers.ContactName LIKE '%o%'").Where(c => c.ContactTitle == title),
                cs => cs.Where(c => c.ContactName.Contains("o")).Where(c => c.ContactTitle == title),
                entryCount: 4);

            title = "Sales Manager";

            AssertQuery<Customer>(
                cs => cs.FromSql("SELECT * FROM Customers WHERE Customers.ContactName LIKE '%o%'").Where(c => c.ContactTitle == title),
                cs => cs.Where(c => c.ContactName.Contains("o")).Where(c => c.ContactTitle == title),
                entryCount: 7);
        }

        [Fact]
        public virtual void From_sql_queryable_with_multiple_line_query()
        {
            AssertQuery<Customer>(
                cs => cs.FromSql(@"SELECT *
FROM Customers
WHERE Customers.City = 'London'"),
                cs => cs.Where(c => c.City == "London"),
                entryCount: 6);
        }

        [Fact]
        public virtual void From_sql_queryable_with_params_parameters()
        {
            var city = "London";
            var contactTitle = "Sales Representative";

            AssertQuery<Customer>(
                cs => cs.FromSql(@"SELECT * FROM Customers WHERE City = {0} AND ContactTitle = {1}", city, contactTitle),
                cs => cs.Where(c => c.City == city && c.ContactTitle == contactTitle),
                entryCount: 3);
        }

        [Fact]
        public virtual void From_sql_queryable_with_params_parameters_does_not_collide_with_cache()
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
        public virtual void From_sql_annotations_do_not_modify_successive_calls()
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
    }
}
