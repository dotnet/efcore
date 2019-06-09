// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    // ReSharper disable once UnusedTypeParameter
    public abstract partial class SimpleQueryTestBase<TFixture>
    {
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task QueryType_simple(bool isAsync)
        {
            return AssertQuery<CustomerView>(
                isAsync,
                cvs => cvs.AsNoTracking());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task QueryType_where_simple(bool isAsync)
        {
            return AssertQuery<CustomerView>(
                isAsync,
                cvs => cvs.AsNoTracking().Where(c => c.City == "London"));
        }

        [ConditionalFact]
        public virtual void Query_backed_by_database_view()
        {
            using (var context = CreateContext())
            {
                var results = context.Set<ProductQuery>().AsNoTracking().ToArray();

                Assert.Equal(69, results.Length);
            }
        }

        [ConditionalFact]
        public virtual void Auto_initialized_view_set()
        {
            using (var context = CreateContext())
            {
                var results = context.CustomerQueries.AsNoTracking().ToArray();

                Assert.Equal(91, results.Length);
            }
        }

        [ConditionalFact(Skip = "Issue#15264")]
        public virtual void QueryType_with_nav_defining_query()
        {
            using (var context = CreateContext())
            {
                var results
                    = context.Set<CustomerQuery>().AsNoTracking()
                        .Where(cq => cq.OrderCount > 0)
                        .ToArray();

                Assert.Equal(4, results.Length);
            }
        }

        [ConditionalTheory(Skip = "Issue#15264")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task QueryType_with_defining_query(bool isAsync)
        {
            return AssertQuery<OrderQuery>(
                isAsync,
                ovs => ovs.AsNoTracking().Where(ov => ov.CustomerID == "ALFKI"));
        }

        // #issue 12873
        //[ConditionalTheory]
        //[MemberData(nameof(IsAsyncData))]
        public virtual Task QueryType_with_defining_query_and_correlated_collection(bool isAsync)
        {
            return AssertQuery<OrderQuery>(
                isAsync,
                ovs => ovs.AsNoTracking().Where(ov => ov.CustomerID == "ALFKI").Select(ov => ov.Customer)
                    .Select(cv => cv.Orders.Where(cc => true).ToList()));
        }

        [ConditionalTheory(Skip = "Issue#15264")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task QueryType_with_mixed_tracking(bool isAsync)
        {
            return AssertQuery<Customer, OrderQuery>(
                isAsync,
                (cs, ovs)
                    => from c in cs
                       from o in ovs.AsNoTracking().Where(ov => ov.CustomerID == c.CustomerID)
                       select new
                       {
                           c,
                           o
                       },
                e => e.c.CustomerID);
        }

        [ConditionalTheory(Skip = "Issue#15264")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task QueryType_with_included_nav(bool isAsync)
        {
            return AssertIncludeQuery<OrderQuery>(
                isAsync,
                ovs => from ov in ovs.AsNoTracking().Include(ov => ov.Customer)
                       where ov.CustomerID == "ALFKI"
                       select ov,
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<OrderQuery>(ov => ov.Customer, "Customer")
                });
        }

        [ConditionalTheory(Skip = "Issue#15264")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task QueryType_with_included_navs_multi_level(bool isAsync)
        {
            return AssertIncludeQuery<OrderQuery>(
                isAsync,
                ovs => from ov in ovs.AsNoTracking().Include(ov => ov.Customer.Orders)
                       where ov.CustomerID == "ALFKI"
                       select ov,
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<OrderQuery>(ov => ov.Customer, "Customer"),
                    new ExpectedInclude<Customer>(c => c.Orders, "Orders")
                });
        }

        [ConditionalTheory(Skip = "Issue#15264")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task QueryType_select_where_navigation(bool isAsync)
        {
            return AssertQuery<OrderQuery>(
                isAsync,
                ovs => from ov in ovs.AsNoTracking()
                       where ov.Customer.City == "Seattle"
                       select ov);
        }

        [ConditionalTheory(Skip = "Issue#15264")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task QueryType_select_where_navigation_multi_level(bool isAsync)
        {
            return AssertQuery<OrderQuery>(
                isAsync,
                ovs => from ov in ovs.AsNoTracking()
                       where ov.Customer.Orders.Any()
                       select ov);
        }
    }
}
