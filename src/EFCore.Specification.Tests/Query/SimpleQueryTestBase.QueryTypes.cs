// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
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
        [ConditionalFact]
        public virtual void QueryType_simple()
        {
            AssertQuery<CustomerView>(cvs => cvs);
        }

        [ConditionalFact]
        public virtual void QueryType_where_simple()
        {
            AssertQuery<CustomerView>(
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

        [ConditionalFact]
        public virtual void QueryType_with_defining_query()
        {
            AssertQuery<OrderQuery>(ovs => ovs.Where(ov => ov.CustomerID == "ALFKI"));
        }

        [ConditionalFact]
        public virtual void QueryType_with_mixed_tracking()
        {
            AssertQuery<Customer, OrderQuery>(
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

        [ConditionalFact]
        public virtual void QueryType_with_included_nav()
        {
            AssertIncludeQuery<OrderQuery>(
                ovs => from ov in ovs.Include(ov => ov.Customer)
                       where ov.CustomerID == "ALFKI"
                       select ov,
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<OrderQuery>(ov => ov.Customer, "Customer")
                });
        }

        [ConditionalFact]
        public virtual void QueryType_with_included_navs_multi_level()
        {
            AssertIncludeQuery<OrderQuery>(
                ovs => from ov in ovs.Include(ov => ov.Customer.Orders)
                       where ov.CustomerID == "ALFKI"
                       select ov,
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<OrderQuery>(ov => ov.Customer, "Customer"),
                    new ExpectedInclude<Customer>(c => c.Orders, "Orders")
                });
        }

        [ConditionalFact]
        public virtual void QueryType_select_where_navigation()
        {
            AssertQuery<OrderQuery>(
                ovs => from ov in ovs
                       where ov.Customer.City == "Seattle"
                       select ov);
        }

        [ConditionalFact]
        public virtual void QueryType_select_where_navigation_multi_level()
        {
            AssertQuery<OrderQuery>(
                ovs => from ov in ovs
                       where ov.Customer.Orders.Any()
                       select ov);
        }
    }
}
