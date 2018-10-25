// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task QueryType_simple(bool isAsync)
        {
            return AssertQuery<CustomerView>(
                isAsync,
                cvs => cvs);
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task QueryType_where_simple(bool isAsync)
        {
            return AssertQuery<CustomerView>(
                isAsync,
                cvs => cvs.Where(c => c.City == "London"));
        }

        [ConditionalFact]
        public virtual void Query_backed_by_database_view()
        {
            using (var context = CreateContext())
            {
                var results = context.Query<ProductQuery>().ToArray();

                Assert.Equal(69, results.Length);
            }
        }

        [ConditionalFact]
        public virtual void Query_throws_for_non_query()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(CoreStrings.InvalidSetTypeEntity(nameof(Product)),
                    Assert.Throws<InvalidOperationException>(() => context.Query<Product>().ToArray()).Message);
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
        public virtual void QueryType_with_nav_defining_query()
        {
            using (var context = CreateContext())
            {
                var results
                    = context.Query<CustomerQuery>()
                        .Where(cq => cq.OrderCount > 0)
                        .ToArray();

                Assert.Equal(4, results.Length);
            }
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task QueryType_with_defining_query(bool isAsync)
        {
            return AssertQuery<OrderQuery>(
                isAsync,
                ovs => ovs.Where(ov => ov.CustomerID == "ALFKI"));
        }

        // #issue 12873
        //[Theory]
        //[MemberData(nameof(IsAsyncData))]
        public virtual Task QueryType_with_defining_query_and_correlated_collection(bool isAsync)
        {
            return AssertQuery<OrderQuery>(
                isAsync,
                ovs => ovs.Where(ov => ov.CustomerID == "ALFKI").Select(ov => ov.Customer).Select(cv => cv.Orders.Where(cc => true).ToList()));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task QueryType_with_mixed_tracking(bool isAsync)
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

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task QueryType_with_included_nav(bool isAsync)
        {
            return AssertIncludeQuery<OrderQuery>(
                isAsync,
                ovs => from ov in ovs.Include(ov => ov.Customer)
                       where ov.CustomerID == "ALFKI"
                       select ov,
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<OrderQuery>(ov => ov.Customer, "Customer")
                });
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task QueryType_with_included_navs_multi_level(bool isAsync)
        {
            return AssertIncludeQuery<OrderQuery>(
                isAsync,
                ovs => from ov in ovs.Include(ov => ov.Customer.Orders)
                       where ov.CustomerID == "ALFKI"
                       select ov,
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<OrderQuery>(ov => ov.Customer, "Customer"),
                    new ExpectedInclude<Customer>(c => c.Orders, "Orders")
                });
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task QueryType_select_where_navigation(bool isAsync)
        {
            return AssertQuery<OrderQuery>(
                isAsync,
                ovs => from ov in ovs
                       where ov.Customer.City == "Seattle"
                       select ov);
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task QueryType_select_where_navigation_multi_level(bool isAsync)
        {
            return AssertQuery<OrderQuery>(
                isAsync,
                ovs => from ov in ovs
                       where ov.Customer.Orders.Any()
                       select ov);
        }
    }
}
