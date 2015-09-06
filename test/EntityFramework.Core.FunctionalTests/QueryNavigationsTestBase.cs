// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Xunit;

// ReSharper disable AccessToDisposedClosure
// ReSharper disable PossibleUnintendedReferenceComparison

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class QueryNavigationsTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryFixtureBase, new()
    {
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
        public virtual async Task Select_Where_Navigation_Async()
        {
            using (var context = CreateContext())
            {
                var orders
                    = await (from o in context.Set<Order>()
                       where o.Customer.City == "Seattle"
                       select o).ToListAsync();

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
        public virtual async Task Select_Singleton_Navigation_With_Member_Access_Async()
        {
            using (var context = CreateContext())
            {
                var orders
                    = await (from o in context.Set<Order>()
                       where o.Customer.City == "Seattle"
                       where o.Customer.Phone != "555 555 5555"
                       select new { A = o.Customer, B = o.Customer.City }).ToListAsync();

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

        [Fact]
        public virtual void Collection_select_nav_prop_any()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                        select new { Any = c.Orders.Any() }).ToList();

                Assert.Equal(91, customers.Count);
                Assert.Equal(89, customers.Count(c => c.Any));
            }
        }

        [Fact]
        public virtual void Collection_where_nav_prop_any()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                        where c.Orders.Any()
                        select c).ToList();

                Assert.Equal(89, customers.Count);
            }
        }

        [Fact]
        public virtual void Collection_where_nav_prop_any_predicate()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                       where c.Orders.Any(o => o.OrderID > 0)
                       select c).ToList();

                Assert.Equal(89, customers.Count);
            }
        }

        [Fact]
        public virtual void Collection_select_nav_prop_all()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                        select new { All = c.Orders.All(o => o.CustomerID == "ALFKI") })
                        .ToList();

                Assert.Equal(91, customers.Count);
            }
        }

        [Fact]
        public virtual void Collection_select_nav_prop_all_client()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                        select new { All = c.Orders.All(o => o.ShipCity == "London") })
                        .ToList();

                Assert.Equal(91, customers.Count);
            }
        }

        [Fact]
        public virtual void Collection_where_nav_prop_all()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                        where c.Orders.All(o => o.CustomerID == "ALFKI")
                        select c).ToList();

                Assert.Equal(3, customers.Count);
            }
        }

        [Fact]
        public virtual void Collection_where_nav_prop_all_client()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                        where c.Orders.All(o => o.ShipCity == "London")
                        select c).ToList();

                Assert.Equal(2, customers.Count);
            }
        }

        [Fact]
        public virtual void Collection_select_nav_prop_count()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                        select new { c.Orders.Count }).ToList();

                Assert.Equal(91, customers.Count);
            }
        }

        [Fact]
        public virtual void Collection_where_nav_prop_count()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                        where c.Orders.Count() > 5
                        select c).ToList();

                Assert.Equal(63, customers.Count);
            }
        }

        [Fact]
        public virtual void Collection_where_nav_prop_count_reverse()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                        where 5 < c.Orders.Count()
                        select c).ToList();

                Assert.Equal(63, customers.Count);
            }
        }

        [Fact]
        public virtual void Collection_orderby_nav_prop_count()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                        orderby c.Orders.Count()
                        select c).ToList();

                Assert.Equal(91, customers.Count);
            }
        }

        [Fact]
        public virtual void Collection_select_nav_prop_long_count()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                        select new { C = c.Orders.LongCount() }).ToList();

                Assert.Equal(91, customers.Count);
            }
        }

        [Fact]
        public virtual void Collection_select_nav_prop_sum()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                        select new { Sum = c.Orders.Sum(o => o.OrderID) }).ToList();

                Assert.Equal(91, customers.Count);
            }
        }

        [Fact]
        public virtual void Collection_where_nav_prop_sum()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                        where c.Orders.Sum(o => o.OrderID) > 1000
                        select c).ToList();

                Assert.Equal(89, customers.Count);
            }
        }

        [Fact]
        public virtual async Task Collection_where_nav_prop_sum_async()
        {
            using (var context = CreateContext())
            {
                var customers
                    = await (from c in context.Set<Customer>()
                       where c.Orders.Sum(o => o.OrderID) > 1000
                       select c).ToListAsync();

                Assert.Equal(89, customers.Count);
            }
        }

        [Fact]
        public virtual void Collection_select_nav_prop_first_or_default()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                        select new { First = c.Orders.FirstOrDefault() }).ToList();

                Assert.Equal(91, customers.Count);
            }
        }

        [Fact]
        public virtual void Collection_select_nav_prop_first_or_default_then_nav_prop()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                        select new { c.Orders.FirstOrDefault().Customer }).ToList();

                Assert.Equal(91, customers.Count);
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
