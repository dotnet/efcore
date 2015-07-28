// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Xunit;

// ReSharper disable PossibleUnintendedReferenceComparison

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class QueryNavigationsTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryFixtureBase, new()
    {
        // TODO:
        // - Composite keys
        // - o1.Customer == o2.Customer
        // - o1.Customer.Name == foo.Customer.Name
        // - Collections, sub-queries (Any etc.).
        // - Deep paths
        // - One to ones
        // - Client eval
        // - Async

        [Fact]
        public virtual void Select_Where_Navigation()
        {
            using (var context = CreateContext())
            {
                var orders
                    = (from o in context.Set<Order>()
                        where o.Customer.City == "Seattle"
                        select o).ToList();

                Assert.Equal(14, orders.Count);
            }
        }

        [Fact]
        public virtual void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar()
        {
            using (var context = CreateContext())
            {
                var orders
                    = (from o1 in context.Set<Order>()
                       from o2 in context.Set<Order>()
                       where o1.Customer.City == o2.Customer.City
                       select new { o1, o2 }).ToList();

                Assert.Equal(14786, orders.Count);
            }
        }

        [Fact]
        public virtual void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected()
        {
            using (var context = CreateContext())
            {
                var orders
                    = (from o1 in context.Set<Order>()
                       from o2 in context.Set<Order>()
                       where o1.Customer.City == o2.Customer.City
                       select new { o1.CustomerID, C2 = o2.CustomerID }).ToList();

                Assert.Equal(14786, orders.Count);
            }
        }

        [Fact]
        public virtual void Select_Where_Navigation_Client()
        {
            using (var context = CreateContext())
            {
                var orders
                    = (from o in context.Set<Order>()
                       where o.Customer.IsLondon
                       select o).ToList();

                Assert.Equal(46, orders.Count);
            }
        }

        [Fact]
        public virtual void Select_Where_Navigation_Deep()
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = (from od in context.Set<OrderDetail>()
                        where od.Order.Customer.City == "Seattle"
                        select od).Take(1).ToList();

                Assert.Equal(1, orderDetails.Count);
            }
        }

        [Fact]
        public virtual void Select_Where_Navigation_Null()
        {
            using (var context = CreateContext())
            {
                var employees
                    = (from e in context.Set<Employee>()
                        where e.Manager == null
                        select e).ToList();

                Assert.Equal(1, employees.Count);
            }
        }

        [Fact]
        public virtual void Select_Where_Navigation_Null_Reverse()
        {
            using (var context = CreateContext())
            {
                var employees
                    = (from e in context.Set<Employee>()
                        where null == e.Manager
                        select e).ToList();

                Assert.Equal(1, employees.Count);
            }
        }

        [Fact]
        public virtual void Select_Where_Navigation_Null_Deep()
        {
            using (var context = CreateContext())
            {
                var employees
                    = (from e in context.Set<Employee>()
                        where e.Manager.Manager == null
                        select e).ToList();

                Assert.Equal(5, employees.Count);
            }
        }

        [Fact]
        public virtual void Select_Where_Navigation_Equals_Navigation()
        {
            using (var context = CreateContext())
            {
                var orders
                    = (from o1 in context.Set<Order>()
                       from o2 in context.Set<Order>()
                       where o1.Customer == o2.Customer
                       select new { o1, o2 }).ToList();

                Assert.Equal(10712, orders.Count);
            }
        }

        [Fact]
        public virtual void Select_Where_Navigation_Included()
        {
            using (var context = CreateContext())
            {
                var orders
                    = (from o in context.Set<Order>().Include(o => o.Customer)
                        where o.Customer.City == "Seattle"
                        select o).ToList();

                Assert.Equal(14, orders.Count);
                Assert.True(orders.All(o => o.Customer != null));
            }
        }

        [Fact]
        public virtual void Singleton_Navigation_With_Member_Access()
        {
            using (var context = CreateContext())
            {
                var orders
                    = (from o in context.Set<Order>()
                       where o.Customer.City == "Seattle"
                       where o.Customer.Phone != "555 555 5555"
                       select new { B = o.Customer.City }).ToList();

                Assert.Equal(14, orders.Count);
                Assert.True(orders.All(o => o.B != null));
            }
        }

        [Fact]
        public virtual void Select_Singleton_Navigation_With_Member_Access()
        {
            using (var context = CreateContext())
            {
                var orders
                    = (from o in context.Set<Order>()
                        where o.Customer.City == "Seattle"
                        where o.Customer.Phone != "555 555 5555"
                        select new { A = o.Customer, B = o.Customer.City }).ToList();

                Assert.Equal(14, orders.Count);
                Assert.True(orders.All(o => o.A != null && o.B != null));
            }
        }

        [Fact]
        public virtual void Select_Where_Navigations()
        {
            using (var context = CreateContext())
            {
                var orders
                    = (from o in context.Set<Order>()
                        where o.Customer.City == "Seattle"
                              && o.Customer.Phone != "555 555 5555"
                        select o).ToList();

                Assert.Equal(14, orders.Count);
            }
        }

        [Fact]
        public virtual void Select_Where_Navigation_Multiple_Access()
        {
            using (var context = CreateContext())
            {
                var orders
                    = (from o in context.Set<Order>()
                        where o.Customer.City == "Seattle"
                              && o.Customer.Phone != "555 555 5555"
                        select o).ToList();

                Assert.Equal(14, orders.Count);
            }
        }

        [Fact]
        public virtual void Select_Navigation()
        {
            using (var context = CreateContext())
            {
                var orders
                    = (from o in context.Set<Order>()
                        select o.Customer).ToList();

                Assert.Equal(830, orders.Count);
                Assert.True(orders.All(o => o != null));
            }
        }

        [Fact]
        public virtual void Select_Navigations()
        {
            using (var context = CreateContext())
            {
                var orders
                    = (from o in context.Set<Order>()
                        select new { A = o.Customer, B = o.Customer }).ToList();

                Assert.Equal(830, orders.Count);
                Assert.True(orders.All(o => o.A != null && o.B != null));
            }
        }

        [Fact]
        public virtual void Select_Navigations_Where_Navigations()
        {
            using (var context = CreateContext())
            {
                var orders
                    = (from o in context.Set<Order>()
                        where o.Customer.City == "Seattle"
                        where o.Customer.Phone != "555 555 5555"
                        select new { A = o.Customer, B = o.Customer }).ToList();

                Assert.Equal(14, orders.Count);
                Assert.True(orders.All(o => o.A != null && o.B != null));
            }
        }

        protected QueryNavigationsTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }

        protected TFixture Fixture { get; }

        protected NorthwindContext CreateContext() => Fixture.CreateContext();
    }
}
