// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Microsoft.Data.Entity.Tests;
using Xunit;

namespace Microsoft.Data.Entity.Relational.FunctionalTests
{
    public class AsyncFromSqlQueryTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryFixtureBase, new()
    {
        [Fact]
        public virtual async Task From_sql_queryable_simple()
        {
            Assert.Equal(91,
                await AssertQuery<Customer>(
                    cs => cs.FromSql("SELECT * FROM Customers"),
                    cs => cs));
        }

        [Fact]
        public virtual async Task From_sql_queryable_filter()
        {
            Assert.Equal(14,
                await AssertQuery<Customer>(
                    cs => cs.FromSql("SELECT * FROM Customers WHERE Customers.ContactName LIKE '%z%'"),
                    cs => cs.Where(c => c.ContactName.Contains("z"))));
        }

        [Fact]
        public virtual async Task From_sql_queryable_cached_by_query()
        {
            Assert.Equal(6,
                await AssertQuery<Customer>(
                    cs => cs.FromSql("SELECT * FROM Customers WHERE Customers.City = 'London'"),
                    cs => cs.Where(c => c.City == "London")));

            Assert.Equal(1,
                await AssertQuery<Customer>(
                    cs => cs.FromSql("SELECT * FROM Customers WHERE Customers.City = 'Seattle'"),
                    cs => cs.Where(c => c.City == "Seattle")));
        }

        [Fact]
        public virtual async Task From_sql_queryable_where_simple_closure_via_query_cache()
        {
            var title = "Sales Associate";

            Assert.Equal(4,
                await AssertQuery<Customer>(
                    cs => cs.FromSql("SELECT * FROM Customers WHERE Customers.ContactName LIKE '%o%'").Where(c => c.ContactTitle == title),
                    cs => cs.Where(c => c.ContactName.Contains("o")).Where(c => c.ContactTitle == title)));

            title = "Sales Manager";

            Assert.Equal(7,
                await AssertQuery<Customer>(
                    cs => cs.FromSql("SELECT * FROM Customers WHERE Customers.ContactName LIKE '%o%'").Where(c => c.ContactTitle == title),
                    cs => cs.Where(c => c.ContactName.Contains("o")).Where(c => c.ContactTitle == title)));
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

        private async Task<int> AssertQuery<TItem>(
            Func<DbSet<TItem>, IQueryable<object>> relationalQuery,
            Func<IQueryable<TItem>, IQueryable<object>> l2oQuery,
            bool assertOrder = false,
            Action<IList<object>, IList<object>> asserter = null)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                return TestHelpers.AssertResults(
                    l2oQuery(NorthwindData.Set<TItem>()).ToArray(),
                    await relationalQuery(context.Set<TItem>()).ToArrayAsync(),
                    assertOrder,
                    asserter);
            }
        }

    }
}
