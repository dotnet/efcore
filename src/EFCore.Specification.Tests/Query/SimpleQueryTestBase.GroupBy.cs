// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

// ReSharper disable StringStartsWithIsCultureSpecific
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract partial class SimpleQueryTestBase<TFixture>
    {
        [ConditionalFact]
        public virtual void GroupBy_anonymous()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.City, c.CustomerID }).GroupBy(a => a.City),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.CustomerID));
        }

        [ConditionalFact]
        public virtual void GroupBy_anonymous_with_where()
        {
            var countries = new[] { "Argentina", "Austria", "Brazil", "France", "Germany", "USA" };
            AssertQuery<Customer>(
                cs => cs.Where(c => countries.Contains(c.Country))
                    .Select(c => new { c.City, c.CustomerID })
                    .GroupBy(a => a.City),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.CustomerID));
        }

        [ConditionalFact(Skip = "Test does not pass. See issue#7160")]
        public virtual void GroupBy_anonymous_subquery()
        {
            AssertQuery<Customer>(
                cs =>
                    cs.Select(c => new { c.City, c.CustomerID })
                        .GroupBy(a => from c2 in cs select c2),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void GroupBy_nested_order_by_enumerable()
        {
            AssertQuery<Customer>(
                cs =>
                    cs.Select(c => new { c.Country, c.CustomerID })
                        .OrderBy(a => a.Country)
                        .GroupBy(a => a.Country)
                        .Select(g => g.OrderBy(a => a.CustomerID)),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void GroupBy_join_default_if_empty_anonymous()
        {
            AssertQuery<Order, OrderDetail>(
                (os, ods) =>
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
                     }).Where(x => x.Key.OrderID == 10248),
                elementAsserter: GroupingAsserter<dynamic, dynamic>(d => d.ProductID));
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
                elementSorter: GroupingSorter<string, Order>(),
                elementAsserter: GroupingAsserter<string, Order>(o => o.OrderID),
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual void GroupBy_simple2()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(g => g),
                elementSorter: GroupingSorter<string, Order>(),
                elementAsserter: GroupingAsserter<string, Order>(o => o.OrderID),
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual void GroupBy_first()
        {
            AssertSingleResult<Order>(
                os => os.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Cast<object>().First(),
                asserter: GroupingAsserter<string, Order>(o => o.OrderID),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual void GroupBy_Sum()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID).Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_Count()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID).Select(g => g.Count()));
        }

        [ConditionalFact]
        public virtual void GroupBy_LongCount()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID).Select(g => g.LongCount()));
        }

        [ConditionalFact]
        public virtual void GroupBy_Shadow()
        {
            AssertQuery<Employee>(
                es =>
                    es.Where(
                            e => EF.Property<string>(e, "Title") == "Sales Representative"
                                 && e.EmployeeID == 1)
                        .GroupBy(e => EF.Property<string>(e, "Title"))
                        .Select(g => EF.Property<string>(g.First(), "Title")));
        }

        [ConditionalFact]
        public virtual void GroupBy_Shadow2()
        {
            AssertQuery<Employee>(
                es =>
                    es.Where(
                            e => EF.Property<string>(e, "Title") == "Sales Representative"
                                 && e.EmployeeID == 1)
                        .GroupBy(e => EF.Property<string>(e, "Title"))
                        .Select(g => g.First()));
        }

        [ConditionalFact]
        public virtual void GroupBy_Shadow3()
        {
            AssertQuery<Employee>(
                es =>
                    es.Where(e => e.EmployeeID == 1)
                        .GroupBy(e => e.EmployeeID)
                        .Select(g => EF.Property<string>(g.First(), "Title")));
        }

        [ConditionalFact]
        public virtual void GroupBy_Sum_Min_Max_Avg()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            Max = g.Max(o => o.OrderID),
                            Avg = g.Average(o => o.OrderID)
                        }),
                e => e.Min + " " + e.Max);
        }

        [ConditionalFact]
        public virtual void GroupBy_with_result_selector()
        {
            AssertQuery<Order>(
                os => os.GroupBy(
                    o => o.CustomerID, (k, g) =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            Max = g.Max(o => o.OrderID),
                            Avg = g.Average(o => o.OrderID)
                        }),
                e => e.Min + " " + e.Max);
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector_sum()
        {
            AssertQueryScalar<Order>(
                os => os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Sum()));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID, o => o.OrderID)
                    .OrderBy(g => g.Key)
                    .Select(g => g.OrderBy(o => o)),
                assertOrder: true,
                elementAsserter: CollectionAsserter<int>());
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector2()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID)
                    .OrderBy(g => g.Key)
                    .Select(g => g.OrderBy(o => o.OrderID)),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Order>());
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector3()
        {
            AssertQuery<Employee>(
                es => es.GroupBy(e => e.EmployeeID)
                    .OrderBy(g => g.Key)
                    .Select(g => g.Select(e => new { Title = EF.Property<string>(e, "Title"), e }).ToList()),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector_sum_max()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID, o => o.OrderID)
                    .Select(g => new { Sum = g.Sum(), Max = g.Max() }),
                e => e.Sum + " " + e.Max);
        }

        [ConditionalFact]
        public virtual void GroupBy_with_anonymous_element()
        {
            AssertQueryScalar<Order>(
                os =>
                    os.GroupBy(o => o.CustomerID, o => new { o.OrderID })
                        .Select(g => g.Sum(x => x.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_two_part_key()
        {
            AssertQueryScalar<Order>(
                os =>
                    os.GroupBy(o => new { o.CustomerID, o.OrderDate })
                        .Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_DateTimeOffset_Property()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderDate.HasValue).GroupBy(o => o.OrderDate.Value.Month),
                e => ((IGrouping<int, Order>)e).Key,
                elementAsserter: GroupingAsserter<int, Order>(o => o.OrderID),
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual void OrderBy_GroupBy()
        {
            AssertQueryScalar<Order>(
                os =>
                    os.OrderBy(o => o.OrderID)
                        .GroupBy(o => o.CustomerID)
                        .Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void OrderBy_GroupBy_SelectMany()
        {
            AssertQuery<Order>(
                os =>
                    os.OrderBy(o => o.OrderID)
                        .GroupBy(o => o.CustomerID)
                        .SelectMany(g => g),
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual void OrderBy_GroupBy_SelectMany_shadow()
        {
            AssertQuery<Employee>(
                es =>
                    es.OrderBy(e => e.EmployeeID)
                        .GroupBy(e => e.EmployeeID)
                        .SelectMany(g => g)
                        .Select(g => EF.Property<string>(g, "Title")));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_orderby()
        {
            AssertQuery<Order>(
                os => os.OrderBy(o => o.OrderID).GroupBy(o => o.CustomerID).OrderBy(g => g.Key),
                assertOrder: true,
                elementAsserter: GroupingAsserter<string, Order>(),
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual void GroupBy_with_orderby_and_anonymous_projection()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).OrderBy(g => g.Key).Select(g => new { Foo = "Foo", Group = g }),
                e => GroupingSorter<string, object>()(e.Group),
                elementAsserter: (e, a) =>
                    {
                        Assert.Equal(e.Foo, a.Foo);
                        IGrouping<string, Order> eGrouping = e.Group;
                        IGrouping<string, Order> aGrouping = a.Group;
                        Assert.Equal(eGrouping.OrderBy(p => p.OrderID), aGrouping.OrderBy(p => p.OrderID));
                    },
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual void GroupBy_with_orderby_take_skip_distinct()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).OrderBy(g => g.Key).Take(5).Skip(3).Distinct(),
                assertOrder: true,
                elementAsserter: GroupingAsserter<string, Order>(o => o.OrderID),
                entryCount: 31);
        }

        [ConditionalFact]
        public virtual void GroupBy_join_anonymous()
        {
            AssertQuery<Order, OrderDetail>(
                (os, ods) =>
                    (from order in os
                     join orderDetail in ods on order.OrderID equals orderDetail.OrderID into orderJoin
                     from orderDetail in orderJoin
                     group new
                     {
                         orderDetail.ProductID,
                         orderDetail.Quantity,
                         orderDetail.UnitPrice
                     } by new
                     {
                         order.OrderID,
                         order.OrderDate
                     }).Where(x => x.Key.OrderID == 10248),
                elementAsserter: GroupingAsserter<dynamic, dynamic>(d => d.ProductID));
        }

        [ConditionalFact]
        public virtual void Select_GroupBy_All()
        {
            using (var context = CreateContext())
            {
                Assert.False(
                    context
                        .Set<Order>()
                        .Select(
                            o => new ProjectedType
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
        public virtual void Select_GroupBy()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Order>().Select(
                    o => new ProjectedType
                    {
                        Order = o.OrderID,
                        Customer = o.CustomerID
                    }).GroupBy(p => p.Customer).ToList().OrderBy(g => g.Key + " " + g.Count()).ToList();

                var expected = Fixture.QueryAsserter.ExpectedData.Set<Order>().Select(
                    o => new ProjectedType
                    {
                        Order = o.OrderID,
                        Customer = o.CustomerID
                    }).GroupBy(p => p.Customer).ToList().OrderBy(g => g.Key + " " + g.Count()).ToList();

                Assert.Equal(expected.Count, actual.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Key, actual[i].Key);
                    Assert.Equal(expected[i].Count(), actual[i].Count());
                }
            }
        }

        [ConditionalFact]
        public virtual void Select_GroupBy_SelectMany()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Order>().Select(
                        o => new ProjectedType
                        {
                            Order = o.OrderID,
                            Customer = o.CustomerID
                        })
                    .GroupBy(o => o.Order)
                    .SelectMany(g => g).ToList().OrderBy(e => e.Order).ToList();

                var expected = Fixture.QueryAsserter.ExpectedData.Set<Order>().Select(
                        o => new ProjectedType
                        {
                            Order = o.OrderID,
                            Customer = o.CustomerID
                        })
                    .GroupBy(o => o.Order)
                    .SelectMany(g => g).ToList().OrderBy(e => e.Order).ToList();

                Assert.Equal(expected.Count, actual.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i], actual[i]);
                }
            }
        }

        [ConditionalFact]
        public virtual void Distinct_GroupBy()
        {
            AssertQuery<Order>(
                os =>
                    os.Distinct()
                        .GroupBy(o => o.CustomerID)
                        .OrderBy(g => g.Key)
                        .Select(g => new { g.Key, c = g.Count() }),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void GroupBy_Distinct()
        {
            AssertQuery<Order>(
                os =>
                    os.GroupBy(o => o.CustomerID).Distinct().Select(g => g.Key));
        }

        [ConditionalFact(Skip = "Unable to bind group by. See Issue#6658")]
        public virtual void Union_simple_groupby()
        {
            AssertQuery<Customer>(
                cs => cs.Where(s => s.ContactTitle == "Owner")
                    .Union(cs.Where(c => c.City == "México D.F."))
                    .GroupBy(c => c.City)
                    .Select(
                        g => new
                        {
                            g.Key,
                            Total = g.Count()
                        }),
                entryCount: 19);
        }

        [ConditionalFact]
        public virtual void Select_nested_collection_with_groupby()
        {
            using (var context = CreateContext())
            {
                var expected = context.Customers
                    .Include(c => c.Orders)
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .ToList()
                    .Select(
                        c => c.Orders.Any()
                            ? c.Orders.GroupBy(o => o.OrderID).Select(g => g.Key).ToArray()
                            : new int[0]).ToList();

                ClearLog();

                var query = context.Customers
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .Select(
                        c => c.Orders.Any()
                            ? c.Orders.GroupBy(o => o.OrderID).Select(g => g.Key).ToArray()
                            : new int[0]);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void OrderBy_Skip_GroupBy()
        {
            AssertQuery<Order>(
                os => os.OrderBy(o => o.OrderDate).Skip(800).GroupBy(o => o.CustomerID),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.OrderDate),
                entryCount: 30);
        }

        [ConditionalFact]
        public virtual void OrderBy_Take_GroupBy()
        {
            AssertQuery<Order>(
                os => os.OrderBy(o => o.OrderDate).Take(50).GroupBy(o => o.CustomerID),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.OrderDate),
                entryCount: 50);
        }

        [ConditionalFact]
        public virtual void OrderBy_Skip_Take_GroupBy()
        {
            AssertQuery<Order>(
                os => os.OrderBy(o => o.OrderDate).Skip(450).Take(50).GroupBy(o => o.CustomerID),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.OrderDate),
                entryCount: 50);
        }


        [ConditionalFact]
        public virtual void Select_Distinct_GroupBy()
        {
            AssertQuery<Order>(
                os => os.Select(o => new { o.CustomerID, o.EmployeeID }).OrderBy(a => a.EmployeeID).Distinct().GroupBy(o => o.CustomerID),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.EmployeeID));
        }
    }
}
