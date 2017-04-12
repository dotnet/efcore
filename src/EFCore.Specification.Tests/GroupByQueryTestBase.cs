// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable AccessToDisposedClosure
// ReSharper disable StringCompareIsCultureSpecific.1
// ReSharper disable StringEndsWithIsCultureSpecific

// ReSharper disable ReplaceWithSingleCallToCount
// ReSharper disable StringStartsWithIsCultureSpecific
// ReSharper disable AccessToModifiedClosure
namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class GroupByQueryTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryFixtureBase, new()
    {
        [ConditionalFact]
        public virtual void GroupBy_anonymous()
        {
            AssertQuery<Customer>(cs =>
                    cs.Select(c => new { c.City, c.CustomerID })
                        .GroupBy(a => a.City),
                asserter: (l2oResults, efResults) =>
                {
                    var efGroupings = efResults.Cast<IGrouping<string, dynamic>>().ToList();

                    foreach (IGrouping<string, dynamic> l2oGrouping in l2oResults)
                    {
                        var efGrouping = efGroupings.Single(efg => efg.Key == l2oGrouping.Key);

                        Assert.Equal(l2oGrouping.OrderBy(o => o.CustomerID), efGrouping.OrderBy(o => o.CustomerID));
                    }
                });
        }

        [ConditionalFact]
        public virtual void GroupBy_anonymous_with_where()
        {
            var countries = new[] { "Argentina", "Austria", "Brazil", "France", "Germany", "USA" };
            AssertQuery<Customer>(cs =>
                    cs.Where(c => countries.Contains(c.Country))
                        .Select(c => new { c.City, c.CustomerID })
                        .GroupBy(a => a.City),
                asserter: (l2oResults, efResults) =>
                {
                    var efGroupings = efResults.Cast<IGrouping<string, dynamic>>().ToList();

                    foreach (IGrouping<string, dynamic> l2oGrouping in l2oResults)
                    {
                        var efGrouping = efGroupings.Single(efg => efg.Key == l2oGrouping.Key);

                        Assert.Equal(l2oGrouping.OrderBy(o => o.CustomerID), efGrouping.OrderBy(o => o.CustomerID));
                    }
                });
        }

        [ConditionalFact]
        public virtual void GroupBy_anonymous_subquery_Key()
        {
            AssertQuery<Customer>(cs =>
                cs.Select(c => new { c.City, c.CustomerID })
                    .GroupBy(a => from c2 in cs select c2),
                asserter: (l2oResults, efResults) =>
                {
                    var l2oElements = l2oResults
                        .Cast<IGrouping<IQueryable<Customer>, dynamic>>()
                        .SelectMany(g => g.Select(e => e))
                        .OrderBy(e => e.City)
                        .ThenBy(e => e.CustomerID);

                    var efElements = efResults
                        .Cast<IGrouping<IQueryable<Customer>, dynamic>>()
                        .SelectMany(g => g.Select(e => e))
                        .OrderBy(e => e.City)
                        .ThenBy(e => e.CustomerID);

                    Assert.Equal(l2oElements, efElements);
                });
        }

        [ConditionalFact]
        public virtual void GroupBy_anonymous_subquery_Element()
        {
            AssertQuery<Customer>(cs =>
                cs.GroupBy(c => c.CustomerID, c => from c2 in cs select c2),
                asserter: (l2oResults, efResults) =>
                {
                    var l2oKeys = l2oResults
                        .Cast<IGrouping<string, IQueryable<Customer>>>()
                        .Select(g => g.Key);

                    var efKeys = efResults
                        .Cast<IGrouping<string, IQueryable<Customer>>>()
                        .Select(g => g.Key);

                    Assert.Equal(l2oKeys, efKeys);
                });
        }

        [ConditionalFact]
        public virtual void GroupBy_nested_order_by_enumerable()
        {
            AssertQuery<Customer>(cs =>
                    cs.Select(c => new { c.Country, c.CustomerID })
                        .OrderBy(a => a.Country)
                        .GroupBy(a => a.Country)
                        .Select(g => g.OrderBy(a => a.CustomerID)),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void GroupBy_join_default_if_empty_anonymous()
        {
            AssertQuery<Order, OrderDetail>((os, ods) =>
                    (from order in os
                     join orderDetail in ods on order.OrderID equals orderDetail.OrderID into orderJoin
                     from orderDetail in orderJoin.DefaultIfEmpty()
                     group new
                     {
                         orderDetail.ProductID,
                         orderDetail.Quantity,
                         orderDetail.UnitPrice
                     } by new
                     {
                         order.OrderID,
                         order.OrderDate
                     })
                        .Where(x => x.Key.OrderID == 10248),
                asserter: (l2oResults, efResults) =>
                {
                    var l2oGroup = l2oResults.Cast<IGrouping<dynamic, dynamic>>().Single();
                    var efGroup = efResults.Cast<IGrouping<dynamic, dynamic>>().Single();

                    Assert.Equal(l2oGroup.Key, efGroup.Key);

                    Assert.Equal(
                        l2oGroup.OrderBy(element => element.ProductID),
                        efGroup.OrderBy(element => element.ProductID));
                });
        }

        [ConditionalFact]
        public virtual void GroupBy_SelectMany()
        {
            AssertQuery<Customer>(
                cs => cs.GroupBy(c => c.City).SelectMany(g => g),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void GroupBy_simple()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID),
                entryCount: 830,
                asserter: (l2oResults, efResults) =>
                {
                    var efGroupings = efResults.Cast<IGrouping<string, Order>>().ToList();

                    foreach (IGrouping<string, Order> l2oGrouping in l2oResults)
                    {
                        var efGrouping = efGroupings.Single(efg => efg.Key == l2oGrouping.Key);

                        Assert.Equal(l2oGrouping.OrderBy(o => o.OrderID), efGrouping.OrderBy(o => o.OrderID));
                    }
                });
        }

        [ConditionalFact]
        public virtual void GroupBy_simple2()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(g => g),
                entryCount: 830,
                asserter: (l2oResults, efResults) =>
                {
                    var efGroupings = efResults.Cast<IGrouping<string, Order>>().ToList();

                    foreach (IGrouping<string, Order> l2oGrouping in l2oResults)
                    {
                        var efGrouping = efGroupings.Single(efg => efg.Key == l2oGrouping.Key);

                        Assert.Equal(l2oGrouping.OrderBy(o => o.OrderID), efGrouping.OrderBy(o => o.OrderID));
                    }
                });
        }

        [ConditionalFact]
        public virtual void GroupBy_first()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Cast<object>().First(),
                asserter: (l2oResult, efResult) =>
                {
                    var l2oGrouping = (IGrouping<string, Order>)l2oResult;
                    var efGrouping = (IGrouping<string, Order>)efResult;

                    Assert.Equal(l2oGrouping.Key, efGrouping.Key);
                    Assert.Equal(l2oGrouping.OrderBy(o => o.OrderID), efGrouping.OrderBy(o => o.OrderID));
                },
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector_entity()
        {
            AssertQuery<Order>(os =>
                    os.GroupBy(o => o.CustomerID)
                        .OrderBy(g => g.Key)
                        .Select(g => g.OrderBy(o => o.OrderID)),
                asserter:
                (l2oResults, efResults) =>
                {
                    var l2oObjects
                        = l2oResults
                            .SelectMany(q1 => ((IEnumerable<Order>)q1));

                    var efObjects
                        = efResults
                            .SelectMany(q1 => ((IEnumerable<Order>)q1));

                    Assert.Equal(l2oObjects, efObjects);
                });
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector_entity_with_projection_Average()
        {
            AssertQuery<Order>(os =>
                    os.GroupBy(o => o.CustomerID).Select(g => g.Average(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector_entity_with_projection_Count()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID).Select(g => g.Count()));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector_entity_with_projection_LongCount()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID).Select(g => g.LongCount()));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector_entity_with_projection_Max()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID).Select(g => g.Max(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector_entity_with_projection_Min()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID).Select(g => g.Min(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector_entity_with_projection_Sum()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID).Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector_property()
        {
            AssertQuery<Order>(
                query: os =>
                    from o in os
                    group o.OrderID by o.CustomerID into og
                    orderby og.Key
                    select og.OrderBy(o => o),
                asserter:
                (l2oResults, efResults) =>
                {
                    var l2oObjects
                        = l2oResults
                            .SelectMany(q1 => ((IEnumerable<int>)q1));

                    var efObjects
                        = efResults
                            .SelectMany(q1 => ((IEnumerable<int>)q1));

                    Assert.Equal(l2oObjects, efObjects);
                });
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector_property_with_projection_Average()
        {
            AssertQuery<Order>(os =>
                    os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Average()));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector_property_with_projection_Max()
        {
            AssertQuery<Order>(os =>
                    os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Max()));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector_property_with_projection_Min()
        {
            AssertQuery<Order>(os =>
                    os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Min()));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector_property_with_projection_Sum()
        {
            AssertQuery<Order>(os =>
                    os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Sum()));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector_property_with_projection_Sum_Max()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID, o => o.OrderID)
                    .Select(g => new { Sum = g.Sum(), Max = g.Max() }));
        }

        [ConditionalFact]
        public virtual void GroupBy_Sum_Where()
        {
            AssertQuery<Order>(os =>
                    os.GroupBy(o => o.CustomerID).Select(g => g.Where(o => o.OrderDate.Value.Month == 10).Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_Shadow()
        {
            AssertQuery<Employee>(es =>
                es.Where(e => EF.Property<string>(e, "Title") == "Sales Representative"
                              && e.EmployeeID == 1)
                    .GroupBy(e => EF.Property<string>(e, "Title"))
                    .Select(g => EF.Property<string>(g.First(), "Title")));
        }

        [ConditionalFact]
        public virtual void GroupBy_Shadow2()
        {
            AssertQuery<Employee>(es =>
                es.Where(e => EF.Property<string>(e, "Title") == "Sales Representative"
                              && e.EmployeeID == 1)
                    .GroupBy(e => EF.Property<string>(e, "Title"))
                    .Select(g => g.First()));
        }

        [ConditionalFact]
        public virtual void GroupBy_Shadow3()
        {
            AssertQuery<Employee>(es =>
                es.Where(e => e.EmployeeID == 1)
                    .GroupBy(e => e.EmployeeID)
                    .Select(g => EF.Property<string>(g.First(), "Title")));
        }

        [ConditionalFact]
        public virtual void GroupBy_Sum_Min_Max_Avg()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID).Select(g =>
                    new
                    {
                        Sum = g.Sum(o => o.OrderID),
                        Min = g.Min(o => o.OrderID),
                        Max = g.Max(o => o.OrderID),
                        Avg = g.Average(o => o.OrderID)
                    }));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_result_selector()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID, (k, g) =>
                    new
                    {
                        Sum = g.Sum(o => o.OrderID),
                        Min = g.Min(o => o.OrderID),
                        Max = g.Max(o => o.OrderID),
                        Avg = g.Average(o => o.OrderID)
                    }));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector_EFProperty()
        {
            AssertQuery<Employee>(es =>
                    es.GroupBy(e => e.EmployeeID)
                        .OrderBy(g => g.Key)
                        .Select(g => g.Select(e => new { Title = EF.Property<string>(e, "Title"), e }).ToList()),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector_anonymous()
        {
            AssertQuery<Order>(
                query: os =>
                    from o in os
                    group new { o.OrderID } by o.CustomerID into og
                    select og.Sum(o => o.OrderID));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector_binary_with_projection_Average()
        {
            AssertQuery<Order>(os =>
                    os.GroupBy(o => o.CustomerID, o => o.OrderID * o.EmployeeID).Select(g => g.Average()));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector_binary_with_projection_Max()
        {
            AssertQuery<Order>(os =>
                    os.GroupBy(o => o.CustomerID, o => o.OrderID * o.EmployeeID).Select(g => g.Max()));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector_binary_with_projection_Min()
        {
            AssertQuery<Order>(os =>
                    os.GroupBy(o => o.CustomerID, o => o.OrderID * o.EmployeeID).Select(g => g.Min()));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector_binary_with_projection_Sum()
        {
            AssertQuery<Order>(os =>
                from o in os
                group o.OrderID * o.EmployeeID by o.CustomerID into og
                select og.Sum());
        }

        [ConditionalFact]
        public virtual void GroupBy_with_key_selector_anonymous_multipart()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => new { o.CustomerID, o.OrderDate })
                    .Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_key_selector_anonymous_multipart_with_projection_whole()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => new { o.CustomerID, o.OrderDate })
                    .Select(g => new { g.Key, Sum = g.Sum(o => o.OrderID) }));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_key_selector_anonymous_multipart_with_projection_split()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => new { o.CustomerID, o.OrderDate })
                    .Select(g => new { g.Key.CustomerID, g.Key.OrderDate, Sum = g.Sum(o => o.OrderID) }));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_key_selector_anonymous_nested_with_projection()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => new { o.CustomerID, n = new { o.OrderDate } })
                    .Select(g => new { Sum = g.Sum(o => o.OrderID) }));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_key_selector_anonymous_nested_with_projection_whole()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => new { o.CustomerID, n = new { o.OrderDate } })
                    .Select(g => new { g.Key, Sum = g.Sum(o => o.OrderID) }));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_key_selector_anonymous_nested_with_projection_split()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => new { o.CustomerID, n = new { o.OrderDate } })
                    .Select(g => new { g.Key.CustomerID, g.Key.n.OrderDate, Sum = g.Sum(o => o.OrderID) }));
        }

        [ConditionalFact]
        public virtual void GroupBy_in_subquery_as_left_side_of_Join()
        {
            using (var context = CreateContext())
            {
                var lastOrders =
                    from o in context.Orders
                    group o by o.CustomerID into og
                    select new
                    {
                        CustomerID = og.Key,
                        MaxId = og.Max(o => o.OrderID)
                    };

                var customersWithLastOrderFreight =
                    from lo in lastOrders
                    join c in context.Customers on lo.CustomerID equals c.CustomerID
                    join o in context.Orders on lo.MaxId equals o.OrderID
                    select new { c.ContactName, o.Freight };

                customersWithLastOrderFreight.ToList();
            }
        }

        [ConditionalFact]
        public virtual void GroupBy_in_subquery_as_right_side_of_Join()
        {
            using (var context = CreateContext())
            {
                var lastOrders =
                    from o in context.Orders
                    group o by o.CustomerID into og
                    select new
                    {
                        CustomerID = og.Key,
                        MaxId = og.Max(o => o.OrderID)
                    };

                var customersWithLastOrderFreight =
                    from c in context.Customers
                    join lo in lastOrders on c.CustomerID equals lo.CustomerID
                    join o in context.Orders on lo.MaxId equals o.OrderID
                    select new { c.ContactName, o.Freight };

                customersWithLastOrderFreight.ToList();
            }
        }

        [ConditionalFact]
        public virtual void GroupBy_in_subquery_as_right_side_of_Join_aggregate_result_operator_as_key()
        {
            using (var context = CreateContext())
            {
                var customersWithLastOrderFreight =
                    from c in context.Customers
                    join lo in (from o in context.Orders group o by o.CustomerID) on c.CustomerID equals lo.Key
                    join o in context.Orders on lo.Max(o => o.OrderID) equals o.OrderID
                    select new { c.ContactName, o.Freight };

                customersWithLastOrderFreight.ToList();
            }
        }

        [ConditionalFact]
        public virtual void GroupBy_with_key_selector_DateTimeOffset_Property()
        {
            AssertQuery<Order>(os =>
                    os.Where(o => o.OrderDate.HasValue)
                        .GroupBy(o => o.OrderDate.Value.Month),
                asserter: (l2oResults, efResults) =>
                {
                    var efGroupings = efResults.Cast<IGrouping<int, Order>>().ToList();

                    foreach (IGrouping<int, Order> l2oGrouping in l2oResults)
                    {
                        var efGrouping = efGroupings.Single(efg => efg.Key == l2oGrouping.Key);

                        Assert.Equal(l2oGrouping.OrderBy(o => o.OrderID), efGrouping.OrderBy(o => o.OrderID));
                    }
                },
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual void OrderBy_GroupBy()
        {
            AssertQuery<Order>(os =>
                os.OrderBy(o => o.OrderID)
                    .GroupBy(o => o.CustomerID)
                    .Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void OrderBy_GroupBy_SelectMany()
        {
            AssertQuery<Order>(os =>
                    os.OrderBy(o => o.OrderID)
                        .GroupBy(o => o.CustomerID)
                        .SelectMany(g => g),
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual void OrderBy_GroupBy_SelectMany_shadow()
        {
            AssertQuery<Employee>(es =>
                es.OrderBy(e => e.EmployeeID)
                    .GroupBy(e => e.EmployeeID)
                    .SelectMany(g => g)
                    .Select(g => EF.Property<string>(g, "Title")));
        }

        [ConditionalFact]
        public virtual void Select_GroupBy()
        {
            AssertQuery<Order>(
                os => os.Select(o => new ProjectedType
                {
                    Order = o.OrderID,
                    Customer = o.CustomerID
                })
                    .GroupBy(p => p.Customer),
                asserter:
                (l2oResults, efResults) =>
                {
                    var efGroupings = efResults.Cast<IGrouping<string, ProjectedType>>().ToList();

                    foreach (IGrouping<string, ProjectedType> l2oGrouping in l2oResults)
                    {
                        var efGrouping = efGroupings.Single(efg => efg.Key == l2oGrouping.Key);

                        Assert.Equal(l2oGrouping.OrderBy(p => p.Order), efGrouping.OrderBy(p => p.Order));
                    }
                });
        }

        [ConditionalFact]
        public virtual void Select_GroupBy_SelectMany()
        {
            AssertQuery<Order>(
                os => os.Select(o => new ProjectedType
                {
                    Order = o.OrderID,
                    Customer = o.CustomerID
                })
                    .GroupBy(o => o.Order)
                    .SelectMany(g => g));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_orderby()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).OrderBy(g => g.Key),
                asserter:
                (l2oResults, efResults) =>
                {
                    var efGroupings = efResults.Cast<IGrouping<string, Order>>().ToList();

                    foreach (IGrouping<string, Order> l2oGrouping in l2oResults)
                    {
                        var efGrouping = efGroupings.Single(efg => efg.Key == l2oGrouping.Key);

                        Assert.Equal(l2oGrouping.OrderBy(p => p.OrderID), efGrouping.OrderBy(p => p.OrderID));
                    }
                },
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual void GroupBy_with_orderby_and_anonymous_projection()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).OrderBy(g => g.Key).Select(g => new { Foo = "Foo", Group = g }),
                asserter:
                (l2oResults, efResults) =>
                {
                    Assert.Equal(l2oResults.Count, efResults.Count);
                    for (var i = 0; i < l2oResults.Count; i++)
                    {
                        dynamic l2oResult = l2oResults[i];
                        dynamic efResult = efResults[i];

                        Assert.Equal(l2oResult.Foo, l2oResult.Foo);
                        IGrouping<string, Order> l2oGrouping = l2oResult.Group;
                        IGrouping<string, Order> efGrouping = efResult.Group;
                        Assert.Equal(l2oGrouping.OrderBy(p => p.OrderID), efGrouping.OrderBy(p => p.OrderID));
                    }
                },
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual void GroupBy_with_orderby_take_skip_distinct()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).OrderBy(g => g.Key).Take(5).Skip(3).Distinct(),
                asserter:
                (l2oResults, efResults) =>
                {
                    var efGroupings = efResults.Cast<IGrouping<string, Order>>().ToList();

                    foreach (IGrouping<string, Order> l2oGrouping in l2oResults)
                    {
                        var efGrouping = efGroupings.Single(efg => efg.Key == l2oGrouping.Key);

                        Assert.Equal(l2oGrouping.OrderBy(p => p.OrderID), efGrouping.OrderBy(p => p.OrderID));
                    }
                },
                entryCount: 31);
        }

        [ConditionalFact]
        public virtual void Select_GroupBy_All()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    false,
                    context
                        .Set<Order>()
                        .Select(o => new ProjectedType
                        {
                            Order = o.OrderID,
                            Customer = o.CustomerID
                        })
                        .GroupBy(a => a.Customer)
                        .All(a => a.Key == "ALFKI")
                );
            }
        }

        [ConditionalFact]
        public virtual void Distinct_GroupBy()
        {
            AssertQuery<Order>(os =>
                    os.Distinct()
                        .GroupBy(o => o.CustomerID)
                        .OrderBy(g => g.Key)
                        .Select(g => new { g.Key, c = g.Count() }),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void GroupBy_Distinct()
        {
            AssertQuery<Order>(os =>
                    os.GroupBy(o => o.CustomerID).Distinct().Select(g => g.Key));
        }

        [ConditionalFact]
        public virtual void Join_GroupBy()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID
                group o by c.Country into og
                select new { og.Key, Sum = og.Sum(o => o.OrderID) });
        }

        [ConditionalFact]
        public virtual void GroupJoin_GroupBy()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID into lo
                group lo by c.Country into og
                select new { og.Key, Sum = og.Sum(lo => lo.Sum(o => o.OrderID)) });
        }

        [ConditionalFact]
        public virtual void GroupJoin_SelectMany_DefaultIfEmpty_GroupBy()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID into lo
                from o in lo.DefaultIfEmpty()
                group o by c.Country into og
                select new { og.Key, Sum = og.Sum(o => o == null ? 0 : o.OrderID) });
        }

        private class ProjectedType
        {
            public int Order { get; set; }
            public string Customer { get; set; }

            protected bool Equals(ProjectedType other) => Equals(Order, other.Order);

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                return obj.GetType() == GetType()
                       && Equals((ProjectedType)obj);
            }

            public override int GetHashCode() => Order.GetHashCode();
        }

        protected NorthwindContext CreateContext() => Fixture.CreateContext();

        protected GroupByQueryTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }

        protected TFixture Fixture { get; }

        private void AssertQuery<TItem>(
            Func<IQueryable<TItem>, int> query,
            bool assertOrder = false)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { query(NorthwindData.Set<TItem>()) },
                    new[] { query(context.Set<TItem>()) },
                    assertOrder);
            }
        }

        private void AssertQuery<TItem>(
            Func<IQueryable<TItem>, bool> query,
            bool assertOrder = false)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { query(NorthwindData.Set<TItem>()) },
                    new[] { query(context.Set<TItem>()) },
                    assertOrder);
            }
        }

        private void AssertQuery<TItem>(
            Func<IQueryable<TItem>, TItem> query,
            bool assertOrder = false)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { query(NorthwindData.Set<TItem>()) },
                    new[] { query(context.Set<TItem>()) },
                    assertOrder);
            }
        }

        private void AssertQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, object> query,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { query(NorthwindData.Set<TItem1>(), NorthwindData.Set<TItem2>()) },
                    new[] { query(context.Set<TItem1>(), context.Set<TItem2>()) },
                    assertOrder);
            }
        }

        private void AssertQuery<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, object> query,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { query(NorthwindData.Set<TItem1>(), NorthwindData.Set<TItem2>(), NorthwindData.Set<TItem3>()) },
                    new[] { query(context.Set<TItem1>(), context.Set<TItem2>(), context.Set<TItem3>()) },
                    assertOrder);
            }
        }

        private void AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<IQueryable<object>>> query,
            bool assertOrder = false,
            Action<IList<IQueryable<object>>, IList<IQueryable<object>>> asserter = null)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    query(context.Set<TItem>()).ToArray(),
                    assertOrder,
                    asserter);
            }
        }

        protected void AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<object>> query,
            bool assertOrder = false,
            int entryCount = 0,
            Action<IList<object>, IList<object>> asserter = null)
            where TItem : class
        => AssertQuery(query, query, assertOrder, entryCount, asserter);

        protected void AssertQuery<TItem>(
            Func<IQueryable<TItem>, object> query,
            bool assertOrder = false,
            int entryCount = 0,
            Action<object, object> asserter = null)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { query(NorthwindData.Set<TItem>()) },
                    new[] { query(context.Set<TItem>()) },
                    assertOrder,
                    asserter != null ? ((l2os, efs) => asserter(l2os.Single(), efs.Single())) : (Action<IList<object>, IList<object>>)null);

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        private void AssertQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> query,
            bool assertOrder = false,
            int? entryCount = null,
            Action<IList<object>, IList<object>> asserter = null)
            where TItem1 : class
            where TItem2 : class
        => AssertQuery(query, query, assertOrder, entryCount, asserter);

        private void AssertQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> efQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> l2oQuery,
            bool assertOrder = false,
            int? entryCount = null,
            Action<IList<object>, IList<object>> asserter = null)
            where TItem1 : class
            where TItem2 : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    l2oQuery(NorthwindData.Set<TItem1>(), NorthwindData.Set<TItem2>()).ToArray(),
                    efQuery(context.Set<TItem1>(), context.Set<TItem2>()).ToArray(),
                    assertOrder,
                    asserter);

                if (entryCount != null)
                {
                    Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
                }
            }
        }

        private void AssertQuery<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> query,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    query(NorthwindData.Set<TItem1>(), NorthwindData.Set<TItem2>(), NorthwindData.Set<TItem3>()).ToArray(),
                    query(context.Set<TItem1>(), context.Set<TItem2>(), context.Set<TItem3>()).ToArray(),
                    assertOrder);
            }
        }

        private void AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<int>> query,
            bool assertOrder = false,
            int entryCount = 0)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    query(context.Set<TItem>()).ToArray(),
                    assertOrder);

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        private void AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<int?>> query,
            bool assertOrder = false,
            int entryCount = 0)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    query(context.Set<TItem>()).ToArray(),
                    assertOrder);

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        private void AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<long>> query, bool assertOrder = false)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    query(context.Set<TItem>()).ToArray(),
                    assertOrder);
            }
        }

        private void AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<double>> query, bool assertOrder = false)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    query(context.Set<TItem>()).ToArray(),
                    assertOrder);
            }
        }

        private void AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<double?>> query, bool assertOrder = false)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    query(context.Set<TItem>()).ToArray(),
                    assertOrder);
            }
        }

        protected void AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<object>> efQuery,
            Func<IQueryable<TItem>, IQueryable<object>> l2oQuery,
            bool assertOrder = false,
            int entryCount = 0,
            Action<IList<object>, IList<object>> asserter = null)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                var expected = l2oQuery(NorthwindData.Set<TItem>()).ToArray();
                var actual = efQuery(context.Set<TItem>()).ToArray();

                TestHelpers.AssertResults(
                    expected,
                    actual,
                    assertOrder,
                    asserter);

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        private void AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<bool>> query, bool assertOrder = false)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    query(context.Set<TItem>()).ToArray(),
                    assertOrder);
            }
        }

        protected virtual void ClearLog()
        {
        }
    }
}
