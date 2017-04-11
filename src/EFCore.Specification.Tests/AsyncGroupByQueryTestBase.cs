// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
// ReSharper disable ConvertToExpressionBodyWhenPossible

// ReSharper disable AccessToDisposedClosure
// ReSharper disable StringStartsWithIsCultureSpecific
// ReSharper disable AccessToModifiedClosure
// ReSharper disable StringEndsWithIsCultureSpecific
namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class AsyncGroupByQueryTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryFixtureBase, new()
    {
        [ConditionalFact]
        public virtual async Task GroupBy_anonymous()
        {
            await AssertQuery<Customer>(cs =>
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
        public virtual async Task GroupBy_anonymous_with_where()
        {
            var countries = new[] { "Argentina", "Austria", "Brazil", "France", "Germany", "USA" };

            await AssertQuery<Customer>(cs =>
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
        public virtual async Task GroupBy_anonymous_subquery_Key()
        {
            await AssertQuery<Customer>(cs =>
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
        public virtual async Task GroupBy_anonymous_subquery_Element()
        {
            await AssertQuery<Customer>(cs =>
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
        public virtual async Task GroupBy_nested_order_by_enumerable()
        {
            await AssertQuery<Customer>(cs =>
                cs.Select(c => new { c.Country, c.CustomerID })
                    .OrderBy(a => a.Country)
                    .GroupBy(a => a.Country)
                    .Select(g => g.OrderBy(a => a.CustomerID)),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_SelectMany()
        {
            await AssertQuery<Customer>(
                cs => cs.GroupBy(c => c.City).SelectMany(g => g),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_simple()
        {
            await AssertQuery<Order>(
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
        public virtual async Task GroupBy_simple2()
        {
            await AssertQuery<Order>(
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
        public virtual async Task GroupBy_first()
        {
            await AssertQuery<Order>(
                os => os.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Cast<object>().FirstAsync(),
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
        public virtual async Task GroupBy_Average()
        {
            await AssertQuery<Order>(os =>
                    os.GroupBy(o => o.CustomerID).Select(g => g.Average(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Count()
        {
            await AssertQuery<Order>(os =>
                    os.GroupBy(o => o.CustomerID).Select(g => g.Count()));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_LongCount()
        {
            await AssertQuery<Order>(os =>
                    os.GroupBy(o => o.CustomerID).Select(g => g.LongCount()));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Max()
        {
            await AssertQuery<Order>(os =>
                    os.GroupBy(o => o.CustomerID).Select(g => g.Max(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Min()
        {
            await AssertQuery<Order>(os =>
                    os.GroupBy(o => o.CustomerID).Select(g => g.Min(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Sum()
        {
            await AssertQuery<Order>(os =>
                    os.GroupBy(o => o.CustomerID).Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Shadow()
        {
            await AssertQuery<Employee>(es =>
                es.Where(e => EF.Property<string>(e, "Title") == "Sales Representative"
                              && e.EmployeeID == 1)
                    .GroupBy(e => EF.Property<string>(e, "Title"))
                    .Select(g => EF.Property<string>(g.First(), "Title")));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Shadow2()
        {
            await AssertQuery<Employee>(es =>
                es.Where(e => EF.Property<string>(e, "Title") == "Sales Representative"
                              && e.EmployeeID == 1)
                    .GroupBy(e => EF.Property<string>(e, "Title"))
                    .Select(g => g.First()));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Shadow3()
        {
            await AssertQuery<Employee>(es =>
                es.Where(e => e.EmployeeID == 1)
                    .GroupBy(e => e.EmployeeID)
                    .Select(g => EF.Property<string>(g.First(), "Title")));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Sum_Min_Max_Avg()
        {
            await AssertQuery<Order>(os =>
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
        public virtual async Task GroupBy_with_result_selector()
        {
            await AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID, (k, g) =>
                    new
                    {
                        Sum = g.Sum(o => o.OrderID),
                        MinAsync = g.Min(o => o.OrderID),
                        MaxAsync = g.Max(o => o.OrderID),
                        Avg = g.Average(o => o.OrderID)
                    }));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_with_element_selector_Average()
        {
            await AssertQuery<Order>(os =>
                    os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Average()));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_with_element_selector_Max()
        {
            await AssertQuery<Order>(os =>
                    os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Max()));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_with_element_selector_Min()
        {
            await AssertQuery<Order>(os =>
                    os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Min()));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_with_element_selector_Sum()
        {
            await AssertQuery<Order>(os =>
                    os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Sum()));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_with_element_selector()
        {
            await AssertQuery<Order>(os =>
                    os.GroupBy(o => o.CustomerID, o => o.OrderID)
                        .OrderBy(g => g.Key)
                        .Select(g => g.OrderBy(o => o)),
                asserter:
                (l2oResults, efResults) =>
                {
                    var l2oObjects
                        = l2oResults
                            .SelectMany(q1 => (IEnumerable<int>)q1);

                    var efObjects
                        = efResults
                            .SelectMany(q1 => (IEnumerable<int>)q1);

                    Assert.Equal(l2oObjects, efObjects);
                });
        }

        [ConditionalFact]
        public virtual async Task GroupBy_with_element_selector2()
        {
            await AssertQuery<Order>(os =>
                    os.GroupBy(o => o.CustomerID)
                        .OrderBy(g => g.Key)
                        .Select(g => g.OrderBy(o => o.OrderID)),
                asserter:
                (l2oResults, efResults) =>
                {
                    var l2oObjects
                        = l2oResults
                            .SelectMany(q1 => (IEnumerable<Order>)q1);

                    var efObjects
                        = efResults
                            .SelectMany(q1 => (IEnumerable<Order>)q1);

                    Assert.Equal(l2oObjects, efObjects);
                });
        }

        [ConditionalFact]
        public virtual async Task GroupBy_with_element_selector3()
        {
            await AssertQuery<Employee>(es =>
                    es.GroupBy(e => e.EmployeeID)
                        .OrderBy(g => g.Key)
                        .Select(g => g.Select(e => new { Title = EF.Property<string>(e, "Title"), e }).ToList()),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_with_element_selector_sum_max()
        {
            await AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID, o => o.OrderID)
                    .Select(g => new { Sum = g.Sum(), MaxAsync = g.Max() }));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_with_anonymous_element()
        {
            await AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID, o => new { o.OrderID })
                    .Select(g => g.Sum(x => x.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_with_two_part_key()
        {
            await AssertQuery<Order>(os =>
                os.GroupBy(o => new { o.CustomerID, o.OrderDate })
                    .Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_with_two_part_key_in_projection_whole()
        {
            await AssertQuery<Order>(os =>
                os.GroupBy(o => new { o.CustomerID, o.OrderDate })
                    .Select(g => new { g.Key, Sum = g.Sum(o => o.OrderID) }));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_with_two_part_key_in_projection_split()
        {
            await AssertQuery<Order>(os =>
                os.GroupBy(o => new { o.CustomerID, o.OrderDate })
                    .Select(g => new { g.Key.CustomerID, g.Key.OrderDate, Sum = g.Sum(o => o.OrderID) }));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_with_nested_key_in_projection_whole()
        {
            await AssertQuery<Order>(os =>
                os.GroupBy(o => new { o.CustomerID, n = new { o.OrderDate } })
                    .Select(g => new { g.Key, Sum = g.Sum(o => o.OrderID) }));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_with_nested_key_in_projection_split()
        {
            await AssertQuery<Order>(os =>
                os.GroupBy(o => new { o.CustomerID, n = new { o.OrderDate } })
                    .Select(g => new { g.Key.CustomerID, g.Key.n.OrderDate, Sum = g.Sum(o => o.OrderID) }));
        }

        [ConditionalFact]
        public virtual async Task OrderBy_GroupBy()
        {
            await AssertQuery<Order>(os =>
                os.OrderBy(o => o.OrderID)
                    .GroupBy(o => o.CustomerID)
                    .Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task OrderBy_GroupBy_SelectMany()
        {
            await AssertQuery<Order>(os =>
                    os.OrderBy(o => o.OrderID)
                        .GroupBy(o => o.CustomerID)
                        .SelectMany(g => g),
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual async Task OrderBy_GroupBy_SelectMany_shadow()
        {
            await AssertQuery<Employee>(es =>
                es.OrderBy(e => e.EmployeeID)
                    .GroupBy(e => e.EmployeeID)
                    .SelectMany(g => g)
                    .Select(g => EF.Property<string>(g, "Title")));
        }

        [ConditionalFact]
        public virtual async Task Distinct_GroupBy()
        {
            await AssertQuery<Order>(os =>
                    os.Distinct()
                        .GroupBy(o => o.CustomerID)
                        .OrderBy(g => g.Key)
                        .Select(g => new { g.Key, c = g.Count() }),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Distinct()
        {
            await AssertQuery<Order>(os =>
                    os.GroupBy(o => o.CustomerID).Distinct().Select(g => g.Key));
        }

        protected NorthwindContext CreateContext()
        {
            return Fixture.CreateContext();
        }

        protected AsyncGroupByQueryTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }

        protected TFixture Fixture { get; }

        private async Task AssertQuery<TItem>(
            Func<IQueryable<TItem>, Task<int>> query,
            bool assertOrder = false)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { await query(NorthwindData.Set<TItem>()) },
                    new[] { await query(context.Set<TItem>()) },
                    assertOrder);
            }
        }

        private async Task AssertQuery<TItem>(
            Func<IQueryable<TItem>, Task<long>> query,
            bool assertOrder = false)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { await query(NorthwindData.Set<TItem>()) },
                    new[] { await query(context.Set<TItem>()) },
                    assertOrder);
            }
        }

        private async Task AssertQuery<TItem>(
            Func<IQueryable<TItem>, Task<bool>> query,
            bool assertOrder = false)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { await query(NorthwindData.Set<TItem>()) },
                    new[] { await query(context.Set<TItem>()) },
                    assertOrder);
            }
        }

        private async Task AssertQuery<TItem>(
            Func<IQueryable<TItem>, Task<decimal>> query,
            bool assertOrder = false,
            Action<decimal, decimal> asserter = null)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { await query(NorthwindData.Set<TItem>()) },
                    new[] { await query(context.Set<TItem>()) },
                    assertOrder,
                    asserter != null ? ((l2os, efs) => asserter(l2os.Single(), efs.Single())) : (Action<IList<decimal>, IList<decimal>>)null);
            }
        }

        private async Task AssertQuery<TItem>(
            Func<IQueryable<TItem>, Task<double>> query,
            bool assertOrder = false)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { await query(NorthwindData.Set<TItem>()) },
                    new[] { await query(context.Set<TItem>()) },
                    assertOrder);
            }
        }

        private async Task AssertQuery<TItem>(
            Func<IQueryable<TItem>, Task<TItem>> query,
            bool assertOrder = false)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { await query(NorthwindData.Set<TItem>()) },
                    new[] { await query(context.Set<TItem>()) },
                    assertOrder);
            }
        }

        private async Task AssertQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, Task<object>> query,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { await query(NorthwindData.Set<TItem1>(), NorthwindData.Set<TItem2>()) },
                    new[] { await query(context.Set<TItem1>(), context.Set<TItem2>()) },
                    assertOrder);
            }
        }

        private async Task AssertQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, Task<bool>> query,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { await query(NorthwindData.Set<TItem1>(), NorthwindData.Set<TItem2>()) },
                    new[] { await query(context.Set<TItem1>(), context.Set<TItem2>()) },
                    assertOrder);
            }
        }

        private async Task AssertQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, Task<int>> query,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { await query(NorthwindData.Set<TItem1>(), NorthwindData.Set<TItem2>()) },
                    new[] { await query(context.Set<TItem1>(), context.Set<TItem2>()) },
                    assertOrder);
            }
        }

        private async Task AssertQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, Task<long>> query,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { await query(NorthwindData.Set<TItem1>(), NorthwindData.Set<TItem2>()) },
                    new[] { await query(context.Set<TItem1>(), context.Set<TItem2>()) },
                    assertOrder);
            }
        }

        private async Task AssertQuery<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, Task<bool>> query,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { await query(NorthwindData.Set<TItem1>(), NorthwindData.Set<TItem2>(), NorthwindData.Set<TItem3>()) },
                    new[] { await query(context.Set<TItem1>(), context.Set<TItem2>(), context.Set<TItem3>()) },
                    assertOrder);
            }
        }

        private async Task AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<IQueryable<object>>> query,
            bool assertOrder = false,
            Action<IList<IQueryable<object>>, IList<IQueryable<object>>> asserter = null)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    await query(context.Set<TItem>()).ToArrayAsync(),
                    assertOrder,
                    asserter);
            }
        }

        protected async Task AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<object>> query,
            bool assertOrder = false,
            int entryCount = 0,
            Action<IList<object>, IList<object>> asserter = null)
            where TItem : class
        {
            await AssertQuery(query, query, assertOrder, entryCount, asserter);
        }

        protected async Task AssertQuery<TItem>(
            Func<IQueryable<TItem>, Task<object>> query,
            bool assertOrder = false,
            int entryCount = 0,
            Action<object, object> asserter = null)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { await query(NorthwindData.Set<TItem>()) },
                    new[] { await query(context.Set<TItem>()) },
                    assertOrder,
                    asserter != null ? ((l2os, efs) => asserter(l2os.Single(), efs.Single())) : (Action<IList<object>, IList<object>>)null);

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        private async Task AssertQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> query,
            bool assertOrder = false,
            int? entryCount = null,
            Action<IList<object>, IList<object>> asserter = null)
            where TItem1 : class
            where TItem2 : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    query(NorthwindData.Set<TItem1>(), NorthwindData.Set<TItem2>()).ToArray(),
                    await query(context.Set<TItem1>(), context.Set<TItem2>()).ToArrayAsync(),
                    assertOrder,
                    asserter);

                if (entryCount != null)
                {
                    Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
                }
            }
        }

        private async Task AssertQuery<TItem1, TItem2, TItem3>(
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
                    await query(context.Set<TItem1>(), context.Set<TItem2>(), context.Set<TItem3>()).ToArrayAsync(),
                    assertOrder);
            }
        }

        private async Task AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<int>> query,
            bool assertOrder = false,
            int entryCount = 0)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    await query(context.Set<TItem>()).ToArrayAsync(),
                    assertOrder);

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }
        private async Task AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<int?>> query,
            bool assertOrder = false,
            int entryCount = 0)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    await query(context.Set<TItem>()).ToArrayAsync(),
                    assertOrder);

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        private async Task AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<long>> query, bool assertOrder = false)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    await query(context.Set<TItem>()).ToArrayAsync(),
                    assertOrder);
            }
        }

        private async Task AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<double>> query,
            bool assertOrder = false,
            int entryCount = 0)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    await query(context.Set<TItem>()).ToArrayAsync(),
                    assertOrder);

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        private async Task AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<double?>> query,
            bool assertOrder = false,
            int entryCount = 0)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    await query(context.Set<TItem>()).ToArrayAsync(),
                    assertOrder);

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        protected async Task AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<object>> efQuery,
            Func<IQueryable<TItem>, IQueryable<object>> l2oQuery,
            bool assertOrder = false,
            int entryCount = 0,
            Action<IList<object>, IList<object>> asserter = null)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    l2oQuery(NorthwindData.Set<TItem>()).ToArray(),
                    await efQuery(context.Set<TItem>()).ToArrayAsync(),
                    assertOrder,
                    asserter);

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        private async Task AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<bool>> query, bool assertOrder = false)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    await query(context.Set<TItem>()).ToArrayAsync(),
                    assertOrder);
            }
        }
    }
}
