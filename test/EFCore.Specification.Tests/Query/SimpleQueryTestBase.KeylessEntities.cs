// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract partial class SimpleQueryTestBase<TFixture>
    {
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task KeylessEntity_simple(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<CustomerView>());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task KeylessEntity_where_simple(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<CustomerView>().Where(c => c.City == "London"));
        }

        [ConditionalFact]
        public virtual void KeylessEntity_by_database_view()
        {
            using (var context = CreateContext())
            {
                var results = context.Set<ProductQuery>().ToArray();

                Assert.Equal(69, results.Length);
            }
        }

        [ConditionalFact]
        public virtual void Auto_initialized_view_set()
        {
            using (var context = CreateContext())
            {
                var results = context.CustomerQueries.ToArray();

                Assert.Equal(91, results.Length);
            }
        }

        [ConditionalFact]
        public virtual void KeylessEntity_with_nav_defining_query()
        {
            using (var context = CreateContext())
            {
                var results
                    = context.Set<CustomerQuery>()
                        .Where(cq => cq.OrderCount > 0)
                        .ToArray();

                Assert.Equal(4, results.Length);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task KeylessEntity_with_defining_query(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<OrderQuery>().Where(ov => ov.CustomerID == "ALFKI"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task KeylessEntity_with_defining_query_and_correlated_collection(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<OrderQuery>().Where(ov => ov.CustomerID == "ALFKI").Select(ov => ov.Customer)
                    .OrderBy(c => c.CustomerID)
                    .Select(cv => cv.Orders.Where(cc => true).ToList()),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a),
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task KeylessEntity_with_mixed_tracking(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from c in ss.Set<Customer>()
                      from o in ss.Set<OrderQuery>().Where(ov => ov.CustomerID == c.CustomerID)
                      select new { c, o },
                e => e.c.CustomerID,
                entryCount: 89);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task KeylessEntity_with_included_nav(bool isAsync)
        {
            using (var ctx = CreateContext())
            {
                if (isAsync)
                {
                    await Assert.ThrowsAsync<InvalidOperationException>(
                        () => (from ov in ctx.Set<OrderQuery>().Include(ov => ov.Customer)
                               where ov.CustomerID == "ALFKI"
                               select ov).ToListAsync());
                }
                else
                {
                    await Assert.ThrowsAsync<InvalidOperationException>(
                        () => Task.FromResult(
                            (from ov in ctx.Set<OrderQuery>().Include(ov => ov.Customer)
                             where ov.CustomerID == "ALFKI"
                             select ov).ToList()));
                }
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task KeylessEntity_with_included_navs_multi_level(bool isAsync)
        {
            using (var ctx = CreateContext())
            {
                if (isAsync)
                {
                    await Assert.ThrowsAsync<InvalidOperationException>(
                        () => (from ov in ctx.Set<OrderQuery>().Include(ov => ov.Customer.Orders)
                               where ov.CustomerID == "ALFKI"
                               select ov).ToListAsync());
                }
                else
                {
                    await Assert.ThrowsAsync<InvalidOperationException>(
                        () => Task.FromResult(
                            (from ov in ctx.Set<OrderQuery>().Include(ov => ov.Customer.Orders)
                             where ov.CustomerID == "ALFKI"
                             select ov).ToList()));
                }
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task KeylessEntity_select_where_navigation(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from ov in ss.Set<OrderQuery>()
                      where ov.Customer.City == "Seattle"
                      select ov);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task KeylessEntity_select_where_navigation_multi_level(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from ov in ss.Set<OrderQuery>()
                      where ov.Customer.Orders.Any()
                      select ov);
        }
    }
}
