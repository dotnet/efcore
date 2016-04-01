// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.FunctionalTests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.FunctionalTests.TestUtilities.Xunit;
using Xunit;

// ReSharper disable UseCollectionCountProperty

// ReSharper disable AccessToDisposedClosure
// ReSharper disable PossibleUnintendedReferenceComparison

namespace Microsoft.EntityFrameworkCore.FunctionalTests
{
    [MonoVersionCondition(Min = "4.2.0", SkipReason = "Queries fail on Mono < 4.2.0 due to differences in the implementation of LINQ")]
    public abstract class QueryNavigationsTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryFixtureBase, new()
    {
        [ConditionalFact]
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

        [ConditionalFact]
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

        // issue 4539
        ////[ConditionalFact]
        public virtual void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar()
        {
            using (var context = CreateContext())
            {
                var orders
                    = (from o1 in context.Set<Order>().Where(o => o.OrderID < 10300)
                       from o2 in context.Set<Order>().Where(o => o.OrderID < 10400)
                       where o1.Customer.City == o2.Customer.City
                       select new { o1, o2 }).ToList();

                Assert.Equal(223, orders.Count);
            }
        }

        // issue 4539
        ////[ConditionalFact]
        public virtual void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected()
        {
            using (var context = CreateContext())
            {
                var orders
                    = (from o1 in context.Set<Order>().Where(o => o.OrderID < 10300)
                       from o2 in context.Set<Order>().Where(o => o.OrderID < 10400)
                       where o1.Customer.City == o2.Customer.City
                       select new { o1.CustomerID, C2 = o2.CustomerID }).ToList();

                Assert.Equal(223, orders.Count);
            }
        }

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual void Select_Where_Navigation_Null_Reverse()
        {
            using (var context = CreateContext())
            {
                var query = (from e in context.Set<Employee>()
                             where null == e.Manager
                             select e);

                var result = query.ToList();

                Assert.Equal(1, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigation_Null_Deep()
        {
            List<Employee> expected;
            using (var context = CreateContext())
            {
                expected = context.Employees.Include(e => e.Manager.Manager).ToList()
                    .Where(e => e.Manager == null || e.Manager.Manager == null).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var employees
                    = (from e in context.Set<Employee>()
                       where e.Manager.Manager == null
                       select e).ToList();

                Assert.Equal(expected.Count, employees.Count);
                foreach (var employee in employees)
                {
                    Assert.True(expected.Select(e => e.EmployeeID).Contains(employee.EmployeeID));
                }
            }
        }

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual void Select_Where_Navigation_Included()
        {
            using (var context = CreateContext())
            {
                var query = from o in context.Set<Order>().Include(o => o.Customer)
                            where o.Customer.City == "Seattle"
                            select o;

                var result = query.ToList();

                Assert.Equal(14, result.Count);
                Assert.True(result.All(o => o.Customer != null));
            }
        }

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual void Select_Singleton_Navigation_With_Member_Access()
        {
            List<Order> expected;
            using (var context = CreateContext())
            {
                expected = context.Orders.Include(o => o.Customer)
                    .ToList()
                    .Where(o => o.Customer?.City == "Seattle")
                    .Where(o => o.Customer?.Phone != "555 555 5555")
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from o in context.Set<Order>()
                            where o.Customer.City == "Seattle"
                            where o.Customer.Phone != "555 555 5555"
                            select new { A = o.Customer, B = o.Customer.City };

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultElement in result)
                {
                    Assert.True(expected.Any(e => e.CustomerID == resultElement.A.CustomerID && e.Customer?.City == resultElement.B));
                }
            }
        }

        [ConditionalFact]
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
                Assert.True(orders.All(o => (o.A != null) && (o.B != null)));
            }
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigations()
        {
            using (var context = CreateContext())
            {
                var orders
                    = (from o in context.Set<Order>()
                       where (o.Customer.City == "Seattle")
                             && (o.Customer.Phone != "555 555 5555")
                       select o).ToList();

                Assert.Equal(14, orders.Count);
            }
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigation_Multiple_Access()
        {
            List<string> expected;
            using (var context = CreateContext())
            {
                expected = context.Orders.Include(o => o.Customer).ToList()
                    .Where(o => o.Customer?.City == "Seattle"
                                && o.Customer?.Phone != "555 555 5555")
                    .Select(e => e.CustomerID)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from o in context.Set<Order>()
                            where (o.Customer.City == "Seattle")
                                  && (o.Customer.Phone != "555 555 5555")
                            select o;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultElement in result)
                {
                    expected.Contains(resultElement.CustomerID);
                }
            }
        }

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual void Select_Navigations()
        {
            using (var context = CreateContext())
            {
                var orders
                    = (from o in context.Set<Order>()
                       select new { A = o.Customer, B = o.Customer }).ToList();

                Assert.Equal(830, orders.Count);
                Assert.True(orders.All(o => (o.A != null) && (o.B != null)));
            }
        }

        [ConditionalFact]
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
                Assert.True(orders.All(o => (o.A != null) && (o.B != null)));
            }
        }

        [ConditionalFact]
        public virtual void Select_collection_navigation_simple()
        {
            using (var context = CreateContext())
            {
                var query = from c in context.Customers
                            where c.CustomerID.StartsWith("A")
                            select new { c.Orders };

                var results = query.ToList();

                Assert.Equal(4, results.Count);
                Assert.True(results.All(r => r.Orders.Count > 0));
            }
        }

        [ConditionalFact]
        public virtual void Select_collection_navigation_multi_part()
        {
            using (var context = CreateContext())
            {
                var query = from o in context.Orders
                            where o.CustomerID == "ALFKI"
                            select new { o.Customer.Orders };

                var results = query.ToList();

                Assert.Equal(6, results.Count);
                Assert.True(results.All(r => r.Orders.Count > 0));
            }
        }

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual void Select_multiple_complex_projections()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from o in context.Orders
                       where o.CustomerID.StartsWith("A")
                       select new
                       {
                           collection1 = o.OrderDetails.Count(),
                           scalar1 = o.OrderDate,
                           any = o.OrderDetails.Select(od => od.UnitPrice).Any(up => up > 10),
                           conditional = o.CustomerID == "ALFKI" ? "50" : "10",
                           scalar2 = (int?)o.OrderID,
                           all = o.OrderDetails.All(od => od.OrderID == 42),
                           collection2 = o.OrderDetails.LongCount()
                       }).ToList();

                Assert.Equal(30, customers.Count);
            }
        }

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual void Navigation_fk_based_inside_contains()
        {
            using (var context = CreateContext())
            {
                var query
                    = from o in context.Orders
                      where new[] { "ALFKI" }.Contains(o.Customer.CustomerID)
                      select o;

                var result = query.ToList();

                Assert.Equal(6, result.Count);
                Assert.True(result.All(e => e.CustomerID == "ALFKI"));
            }
        }

        [ConditionalFact]
        public virtual void Navigation_inside_contains()
        {
            using (var context = CreateContext())
            {
                var query
                    = from o in context.Orders
                      where new[] { "Novigrad", "Seattle" }.Contains(o.Customer.City)
                      select o;

                var result = query.ToList();

                Assert.Equal(14, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Navigation_inside_contains_nested()
        {
            using (var context = CreateContext())
            {
                var query
                    = from od in context.OrderDetails
                      where new[] { "Novigrad", "Seattle" }.Contains(od.Order.Customer.City)
                      select od;

                var result = query.ToList();

                Assert.Equal(40, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Navigation_from_join_clause_inside_contains()
        {
            using (var context = CreateContext())
            {
                var query = from od in context.OrderDetails
                            join o in context.Orders on od.OrderID equals o.OrderID
                            where new[] { "USA", "Redania" }.Contains(o.Customer.Country)
                            select od;

                var result = query.ToList();

                Assert.Equal(352, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_subquery_on_navigation()
        {
            using (var context = CreateContext())
            {
                var query = from p in context.Products
                            where p.OrderDetails.Contains(context.OrderDetails.FirstOrDefault(orderDetail => orderDetail.Quantity == 1))
                            select p;

                var result = query.ToList();

                Assert.Equal(1, result.Count);
            }
        }

        // issue #4547
        ////[ConditionalFact]
        public virtual void Navigation_in_subquery_referencing_outer_query()
        {
            using (var context = CreateContext())
            {
                var query = from o in context.Orders
                            // ReSharper disable once UseMethodAny.0
                            where (from od in context.OrderDetails
                                   where o.Customer.Country == od.Order.Customer.Country
                                   select od).Count() > 0
                            select o;

                var result = query.ToList();

                Assert.Equal(830, result.Count);
            }
        }

        protected QueryNavigationsTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }

        protected TFixture Fixture { get; }

        protected NorthwindContext CreateContext() => Fixture.CreateContext();

        protected virtual void ClearLog()
        {
        }
    }
}
