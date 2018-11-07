// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class AsyncGroupByQueryTestBase<TFixture> : QueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected AsyncGroupByQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        protected NorthwindContext CreateContext() => Fixture.CreateContext();

        #region GroupByAggregateComposition

        [ConditionalFact]
        public virtual async Task GroupBy_Select_sum_over_unmapped_property()
        {
            using (var context = CreateContext())
            {
                var query = await context.Orders
                    .GroupBy(o => o.CustomerID)
                    .Select(
                        g => new
                        {
                            g.Key,
                            Sum = g.Sum(o => o.Freight)
                        })
                    .ToListAsync();

                // Do not do deep assertion of result. We don't have data for unmapped property in EF model
                Assert.Equal(89, query.Count);
            }
        }

        [ConditionalFact]
        public virtual async Task Select_nested_collection_with_groupby()
        {
            using (var context = CreateContext())
            {
                var expected = (await context.Customers
                        .Include(c => c.Orders)
                        // ReSharper disable once StringStartsWithIsCultureSpecific
                        .Where(c => c.CustomerID.StartsWith("A"))
                        .ToListAsync())
                    .Select(
                        c => c.Orders.Any()
                            ? c.Orders.GroupBy(o => o.OrderID).Select(g => g.Key).ToArray()
                            : Array.Empty<int>()).ToList();

                var query = context.Customers
                    // ReSharper disable once StringStartsWithIsCultureSpecific
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .Select(
                        c => c.Orders.Any()
                            ? c.Orders.GroupBy(o => o.OrderID).Select(g => g.Key).ToArray()
                            : Array.Empty<int>());

                var result = await query.ToListAsync();

                Assert.Equal(expected.Count, result.Count);
            }
        }

        [ConditionalFact]
        public virtual async Task Select_GroupBy_All()
        {
            using (var context = CreateContext())
            {
                Assert.False(
                    await context
                        .Set<Order>()
                        .Select(
                            o => new ProjectedType
                            {
                                Order = o.OrderID,
                                Customer = o.CustomerID
                            })
                        .GroupBy(a => a.Customer)
                        .AllAsync(a => a.Key == "ALFKI")
                );
            }
        }

        private class ProjectedType
        {
            public int Order { get; set; }
            public string Customer { get; set; }

            private bool Equals(ProjectedType other) => Equals(Order, other.Order);

            public override bool Equals(object obj)
                => obj is null
                    ? false
                    : ReferenceEquals(this, obj)
                        ? true
                        : obj.GetType() == GetType()
                            && Equals((ProjectedType)obj);

            // ReSharper disable once NonReadonlyMemberInGetHashCode
            public override int GetHashCode() => Order.GetHashCode();
        }

        #endregion

        #region GroupByWithoutAggregate

        [ConditionalFact]
        public virtual async Task GroupBy_anonymous_key_without_aggregate()
        {
            using (var context = CreateContext())
            {
                var actual = (await context.Set<Order>()
                        .GroupBy(
                            o => new
                            {
                                o.CustomerID,
                                o.OrderDate
                            })
                        .Select(
                            g => new
                            {
                                g.Key,
                                g
                            })
                        .ToListAsync())
                    .OrderBy(g => g.Key + " " + g.g.Count()).ToList();

                var expected = Fixture.QueryAsserter.ExpectedData.Set<Order>()
                    .GroupBy(
                        o => new
                        {
                            o.CustomerID,
                            o.OrderDate
                        })
                    .Select(
                        g => new
                        {
                            g.Key,
                            g
                        })
                    .ToList()
                    .OrderBy(g => g.Key + " " + g.g.Count()).ToList();

                Assert.Equal(expected.Count, actual.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Key, actual[i].Key);
                    Assert.Equal(expected[i].g.Count(), actual[i].g.Count());
                }
            }
        }

        #endregion

        #region GroupByEntityType

        [ConditionalFact]
        public virtual async Task Select_GroupBy()
        {
            using (var context = CreateContext())
            {
                var actual = (await context.Set<Order>().Select(
                    o => new ProjectedType
                    {
                        Order = o.OrderID,
                        Customer = o.CustomerID
                    }).GroupBy(p => p.Customer).ToListAsync()).OrderBy(g => g.Key + " " + g.Count()).ToList();

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
        public virtual async Task Select_GroupBy_SelectMany()
        {
            using (var context = CreateContext())
            {
                var actual = (await context.Set<Order>().Select(
                        o => new ProjectedType
                        {
                            Order = o.OrderID,
                            Customer = o.CustomerID
                        })
                    .GroupBy(o => o.Order)
                    .SelectMany(g => g).ToListAsync()).OrderBy(e => e.Order).ToList();

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
        public virtual async Task Join_GroupBy_entity_ToList()
        {
            using (var context = CreateContext())
            {
                var actual = await (from c in context.Customers.OrderBy(c => c.CustomerID).Take(5)
                                    join o in context.Orders.OrderBy(o => o.OrderID).Take(50)
                                        on c.CustomerID equals o.CustomerID
                                    group o by c
                                    into grp
                                    select new
                                    {
                                        C = grp.Key,
                                        Os = grp.ToList()
                                    }).ToListAsync();

                var expected = (from c in Fixture.QueryAsserter.ExpectedData.Set<Customer>()
                                    .OrderBy(c => c.CustomerID).Take(5)
                                join o in Fixture.QueryAsserter.ExpectedData.Set<Order>()
                                        .OrderBy(o => o.OrderID).Take(50)
                                    on c.CustomerID equals o.CustomerID
                                group o by c
                                into grp
                                select new
                                {
                                    C = grp.Key,
                                    Os = grp.ToList()
                                }).ToList();

                Assert.Equal(expected.Count, actual.Count);

                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].C, actual[i].C);
                    Assert.Equal(expected[i].Os, actual[i].Os);
                }
            }
        }

        #endregion

        #region DoubleGroupBy

        [ConditionalFact(Skip = "Issue #11917")]
        public virtual async Task Double_GroupBy_with_aggregate()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Set<Order>()
                    .GroupBy(
                        o => new
                        {
                            o.OrderID,
                            o.OrderDate
                        })
                    .GroupBy(g => g.Key.OrderDate)
                    .Select(
                        g => new
                        {
                            g.Key,
                            Lastest = g.OrderBy(e => e.Key.OrderID).FirstOrDefault()
                        })
                    .ToListAsync();

                var expected = Fixture.QueryAsserter.ExpectedData.Set<Order>()
                    .GroupBy(
                        o => new
                        {
                            o.OrderID,
                            o.OrderDate
                        })
                    .GroupBy(g => g.Key.OrderDate)
                    .Select(
                        g => new
                        {
                            g.Key,
                            Lastest = g.OrderBy(e => e.Key.OrderID).FirstOrDefault()
                        })
                    .ToList();

                Assert.Equal(expected.Count, actual.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Key, actual[i].Key);
                    Assert.Equal(expected[i].Lastest.Key, actual[i].Lastest.Key);
                    Assert.Equal(expected[i].Lastest.Count(), actual[i].Lastest.Count());
                }
            }
        }

        #endregion
    }
}
