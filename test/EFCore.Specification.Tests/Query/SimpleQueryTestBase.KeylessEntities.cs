// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    // ReSharper disable once UnusedTypeParameter
    public abstract partial class SimpleQueryTestBase<TFixture>
    {
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task KeylessEntity_simple(bool isAsync)
        {
            return AssertQuery<CustomerView>(
                isAsync,
                cvs => cvs);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task KeylessEntity_where_simple(bool isAsync)
        {
            return AssertQuery<CustomerView>(
                isAsync,
                cvs => cvs.Where(c => c.City == "London"));
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
            return AssertQuery<OrderQuery>(
                isAsync,
                ovs => ovs.Where(ov => ov.CustomerID == "ALFKI"));
        }

        [ConditionalTheory(Skip = "issue #12873")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task KeylessEntity_with_defining_query_and_correlated_collection(bool isAsync)
        {
            return AssertQuery<OrderQuery>(
                isAsync,
                ovs => ovs.Where(ov => ov.CustomerID == "ALFKI").Select(ov => ov.Customer)
                    .Select(cv => cv.Orders.Where(cc => true).ToList()));
        }

        [ConditionalTheory(Skip = "issue #15081")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task KeylessEntity_with_mixed_tracking(bool isAsync)
        {
            return AssertQuery<Customer, OrderQuery>(
                isAsync,
                (cs, ovs)
                    => from c in cs
                       from o in ovs.Where(ov => ov.CustomerID == c.CustomerID)
                       select new
                       {
                           c,
                           o
                       },
                e => e.c.CustomerID);
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
                        () => Task.FromResult((from ov in ctx.Set<OrderQuery>().Include(ov => ov.Customer)
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
                        () => Task.FromResult((from ov in ctx.Set<OrderQuery>().Include(ov => ov.Customer.Orders)
                                               where ov.CustomerID == "ALFKI"
                                               select ov).ToList()));
                }
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task KeylessEntity_select_where_navigation(bool isAsync)
        {
            return AssertQuery<OrderQuery>(
                isAsync,
                ovs => from ov in ovs
                       where ov.Customer.City == "Seattle"
                       select ov);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task KeylessEntity_select_where_navigation_multi_level(bool isAsync)
        {
            return AssertQuery<OrderQuery>(
                isAsync,
                ovs => from ov in ovs
                       where ov.Customer.Orders.Any()
                       select ov);
        }
    }
}
