// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Utilities;
using Northwind;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class IncludeNorthwindAsyncTestBase
    {
        [Fact]
        public virtual async Task Include_collection()
        {
            using (var context = CreateContext())
            {
                var customers
                    = await context.Set<Customer>()
                        .Include(c => c.Orders)
                        .ToListAsync();

                Assert.Equal(91, customers.Count);
                Assert.Equal(830, customers.Where(c => c.Orders != null).SelectMany(c => c.Orders).Count());
                Assert.True(customers.Where(c => c.Orders != null).SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(91 + 830, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_collection_order_by_key()
        {
            using (var context = CreateContext())
            {
                var customers
                    = await context.Set<Customer>()
                        .Include(c => c.Orders)
                        .OrderBy(c => c.CustomerID)
                        .ToListAsync();

                Assert.Equal(91, customers.Count);
                Assert.Equal(830, customers.Where(c => c.Orders != null).SelectMany(c => c.Orders).Count());
                Assert.True(customers.Where(c => c.Orders != null).SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(91 + 830, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_collection_order_by_non_key()
        {
            using (var context = CreateContext())
            {
                var customers
                    = await context.Set<Customer>()
                        .Include(c => c.Orders)
                        .OrderBy(c => c.City)
                        .ToListAsync();

                Assert.Equal(91, customers.Count);
                Assert.Equal(830, customers.Where(c => c.Orders != null).SelectMany(c => c.Orders).Count());
                Assert.True(customers.Where(c => c.Orders != null).SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(91 + 830, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_collection_as_no_tracking()
        {
            using (var context = CreateContext())
            {
                var customers
                    = await context.Set<Customer>()
                        .Include(c => c.Orders)
                        .AsNoTracking()
                        .ToListAsync();

                Assert.Equal(91, customers.Count);
                Assert.Equal(830, customers.Where(c => c.Orders != null).SelectMany(c => c.Orders).Count());
                Assert.True(customers.Where(c => c.Orders != null).SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_collection_as_no_tracking2()
        {
            using (var context = CreateContext())
            {
                var customers
                    = await context.Set<Customer>()
                        .AsNoTracking()
                        .OrderBy(c => c.CustomerID)
                        .Take(5)
                        .Include(c => c.Orders)
                        .ToListAsync();

                Assert.Equal(5, customers.Count);
                Assert.Equal(48, customers.Where(c => c.Orders != null).SelectMany(c => c.Orders).Count());
                Assert.True(customers.Where(c => c.Orders != null).SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_reference()
        {
            using (var context = CreateContext())
            {
                var orders
                    = await context.Set<Order>()
                        .Include(o => o.Customer)
                        .ToListAsync();

                Assert.Equal(830, orders.Count);
                Assert.True(orders.All(o => o.Customer != null));
                Assert.Equal(89, orders.Select(o => o.Customer).Distinct().Count());
                Assert.Equal(830 + 89, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_reference_as_no_tracking()
        {
            using (var context = CreateContext())
            {
                var orders
                    = await context.Set<Order>()
                        .Include(o => o.Customer)
                        .AsNoTracking()
                        .ToListAsync();

                Assert.Equal(830, orders.Count);
                Assert.True(orders.All(o => o.Customer != null));
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_collection_principal_already_tracked()
        {
            using (var context = CreateContext())
            {
                var customer1
                    = await context.Set<Customer>()
                        .SingleAsync(c => c.CustomerID == "ALFKI");

                Assert.Equal(1, context.ChangeTracker.Entries().Count());

                var customer2
                    = await context.Set<Customer>()
                        .Include(c => c.Orders)
                        .SingleAsync(c => c.CustomerID == "ALFKI");

                Assert.Same(customer1, customer2);
                Assert.Equal(6, customer2.Orders.Count);
                Assert.True(customer2.Orders.All(o => o.Customer != null));
                Assert.Equal(1 + 6, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_collection_principal_already_tracked_as_no_tracking()
        {
            using (var context = CreateContext())
            {
                var customer1
                    = await context.Set<Customer>()
                        .SingleAsync(c => c.CustomerID == "ALFKI");

                Assert.Equal(1, context.ChangeTracker.Entries().Count());

                var customer2
                    = await context.Set<Customer>()
                        .Include(c => c.Orders)
                        .AsNoTracking()
                        .SingleAsync(c => c.CustomerID == "ALFKI");

                Assert.Same(customer1, customer2);
                Assert.Equal(6, customer2.Orders.Count);
                Assert.True(customer2.Orders.All(o => o.Customer != null));
                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_reference_dependent_already_tracked()
        {
            using (var context = CreateContext())
            {
                var orders1
                    = await context.Set<Order>()
                        .Where(o => o.CustomerID == "ALFKI")
                        .ToListAsync();

                Assert.Equal(6, context.ChangeTracker.Entries().Count());

                var orders2
                    = await context.Set<Order>()
                        .Include(o => o.Customer)
                        .ToListAsync();

                Assert.True(orders1.All(o1 => orders2.Contains(o1, ReferenceEqualityComparer.Instance)));
                Assert.True(orders2.All(o => o.Customer != null));
                Assert.Equal(830 + 89, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_collection_dependent_already_tracked()
        {
            using (var context = CreateContext())
            {
                var orders
                    = await context.Set<Order>()
                        .Where(o => o.CustomerID == "ALFKI")
                        .ToListAsync();

                Assert.Equal(6, context.ChangeTracker.Entries().Count());

                var customer
                    = await context.Set<Customer>()
                        .Include(c => c.Orders)
                        .SingleAsync(c => c.CustomerID == "ALFKI");

                Assert.Equal(orders, customer.Orders, ReferenceEqualityComparer.Instance);
                Assert.Equal(6, customer.Orders.Count);
                Assert.True(customer.Orders.All(o => o.Customer != null));
                Assert.Equal(6 + 1, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_collection_dependent_already_tracked_as_no_tracking()
        {
            using (var context = CreateContext())
            {
                var orders
                    = await context.Set<Order>()
                        .Where(o => o.CustomerID == "ALFKI")
                        .ToListAsync();

                Assert.Equal(6, context.ChangeTracker.Entries().Count());

                var customer
                    = await context.Set<Customer>()
                        .Include(c => c.Orders)
                        .AsNoTracking()
                        .SingleAsync(c => c.CustomerID == "ALFKI");

                Assert.Equal(orders, customer.Orders, ReferenceEqualityComparer.Instance);
                Assert.Equal(6, customer.Orders.Count);
                Assert.True(customer.Orders.All(o => o.Customer != null));
                Assert.Equal(6, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_collection_with_filter()
        {
            using (var context = CreateContext())
            {
                var customers
                    = await context.Set<Customer>()
                        .Include(c => c.Orders)
                        .Where(c => c.CustomerID == "ALFKI")
                        .ToListAsync();

                Assert.Equal(1, customers.Count);
                Assert.Equal(6, customers.SelectMany(c => c.Orders).Count());
                Assert.True(customers.SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(1 + 6, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_reference_with_filter()
        {
            using (var context = CreateContext())
            {
                var orders
                    = await context.Set<Order>()
                        .Include(o => o.Customer)
                        .Where(o => o.CustomerID == "ALFKI")
                        .ToListAsync();

                Assert.Equal(6, orders.Count);
                Assert.True(orders.All(o => o.Customer != null));
                Assert.Equal(1, orders.Select(o => o.Customer).Distinct().Count());
                Assert.Equal(6 + 1, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_collection_with_filter_reordered()
        {
            using (var context = CreateContext())
            {
                var customers
                    = await context.Set<Customer>()
                        .Where(c => c.CustomerID == "ALFKI")
                        .Include(c => c.Orders)
                        .ToListAsync();

                Assert.Equal(1, customers.Count);
                Assert.Equal(6, customers.SelectMany(c => c.Orders).Count());
                Assert.True(customers.SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(1 + 6, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_reference_with_filter_reordered()
        {
            using (var context = CreateContext())
            {
                var orders
                    = await context.Set<Order>()
                        .Where(o => o.CustomerID == "ALFKI")
                        .Include(o => o.Customer)
                        .ToListAsync();

                Assert.Equal(6, orders.Count);
                Assert.True(orders.All(o => o.Customer != null));
                Assert.Equal(1, orders.Select(o => o.Customer).Distinct().Count());
                Assert.Equal(6 + 1, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_collection_when_projection()
        {
            using (var context = CreateContext())
            {
                var productIds
                    = await context.Set<Customer>()
                        .Include(c => c.Orders)
                        .Select(c => c.CustomerID)
                        .ToListAsync();

                Assert.Equal(91, productIds.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_reference_when_projection()
        {
            using (var context = CreateContext())
            {
                var orders
                    = await context.Set<Order>()
                        .Include(o => o.Customer)
                        .Select(o => o.CustomerID)
                        .ToListAsync();

                Assert.Equal(830, orders.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_multiple_collection()
        {
            using (var context = CreateContext())
            {
                var customers
                    = await (from c1 in context.Set<Customer>()
                        .Include(c => c.Orders)
                        .OrderBy(c => c.CustomerID)
                        .Take(2)
                        from c2 in context.Set<Customer>()
                            .Include(c => c.Orders)
                            .OrderBy(c => c.CustomerID)
                            .Skip(2)
                            .Take(2)
                        select new { c1, c2 })
                        .ToListAsync();

                Assert.Equal(4, customers.Count);
                Assert.Equal(20, customers.SelectMany(c => c.c1.Orders).Count());
                Assert.True(customers.SelectMany(c => c.c1.Orders).All(o => o.Customer != null));
                Assert.Equal(40, customers.SelectMany(c => c.c2.Orders).Count());
                Assert.True(customers.SelectMany(c => c.c2.Orders).All(o => o.Customer != null));
                Assert.Equal(34, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_multiple_reference()
        {
            using (var context = CreateContext())
            {
                var orders
                    = await (from o1 in context.Set<Order>()
                        .Include(o => o.Customer)
                        .OrderBy(o => o.CustomerID)
                        .Take(2)
                        from o2 in context.Set<Order>()
                            .Include(o => o.Customer)
                            .OrderBy(o => o.CustomerID)
                            .Skip(2)
                            .Take(2)
                        select new { o1, o2 })
                        .ToListAsync();

                Assert.Equal(4, orders.Count);
                Assert.True(orders.All(o => o.o1.Customer != null));
                Assert.True(orders.All(o => o.o2.Customer != null));
                Assert.Equal(1, orders.Select(o => o.o1.Customer).Distinct().Count());
                Assert.Equal(1, orders.Select(o => o.o2.Customer).Distinct().Count());
                Assert.Equal(5, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_multiple_reference2()
        {
            using (var context = CreateContext())
            {
                var orders
                    = await (from o1 in context.Set<Order>()
                        .Include(o => o.Customer)
                        .OrderBy(o => o.OrderID)
                        .Take(2)
                        from o2 in context.Set<Order>()
                            .OrderBy(o => o.OrderID)
                            .Skip(2)
                            .Take(2)
                        select new { o1, o2 })
                        .ToListAsync();

                Assert.Equal(4, orders.Count);
                Assert.True(orders.All(o => o.o1.Customer != null));
                Assert.True(orders.All(o => o.o2.Customer == null));
                Assert.Equal(2, orders.Select(o => o.o1.Customer).Distinct().Count());
                Assert.Equal(6, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_multiple_reference3()
        {
            using (var context = CreateContext())
            {
                var orders
                    = await (from o1 in context.Set<Order>()
                        .OrderBy(o => o.OrderID)
                        .Take(2)
                        from o2 in context.Set<Order>()
                            .OrderBy(o => o.OrderID)
                            .Include(o => o.Customer)
                            .Skip(2)
                            .Take(2)
                        select new { o1, o2 })
                        .ToListAsync();

                Assert.Equal(4, orders.Count);
                Assert.True(orders.All(o => o.o1.Customer == null));
                Assert.True(orders.All(o => o.o2.Customer != null));
                Assert.Equal(2, orders.Select(o => o.o2.Customer).Distinct().Count());
                Assert.Equal(6, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_multiple_collection_result_operator()
        {
            using (var context = CreateContext())
            {
                var customers
                    = await (from c1 in context.Set<Customer>()
                        .Include(c => c.Orders)
                        .OrderBy(c => c.CustomerID)
                        .Take(2)
                        from c2 in context.Set<Customer>()
                            .Include(c => c.Orders)
                            .OrderBy(c => c.CustomerID)
                            .Skip(2)
                            .Take(2)
                        select new { c1, c2 })
                        .Take(1)
                        .ToListAsync();

                Assert.Equal(1, customers.Count);
                Assert.Equal(6, customers.SelectMany(c => c.c1.Orders).Count());
                Assert.True(customers.SelectMany(c => c.c1.Orders).All(o => o.Customer != null));
                Assert.Equal(7, customers.SelectMany(c => c.c2.Orders).Count());
                Assert.True(customers.SelectMany(c => c.c2.Orders).All(o => o.Customer != null));
                Assert.Equal(15, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_multiple_collection_result_operator2()
        {
            using (var context = CreateContext())
            {
                var customers
                    = await (from c1 in context.Set<Customer>()
                        .Include(c => c.Orders)
                        .OrderBy(c => c.CustomerID)
                        .Take(2)
                        from c2 in context.Set<Customer>()
                            .OrderBy(c => c.CustomerID)
                            .Skip(2)
                            .Take(2)
                        select new { c1, c2 })
                        .Take(1)
                        .ToListAsync();

                Assert.Equal(1, customers.Count);
                Assert.Equal(6, customers.SelectMany(c => c.c1.Orders).Count());
                Assert.True(customers.SelectMany(c => c.c1.Orders).All(o => o.Customer != null));
                Assert.True(customers.All(c => c.c2.Orders == null));
                Assert.Equal(8, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_collection_on_additional_from_clause()
        {
            using (var context = CreateContext())
            {
                var customers
                    = await (from c1 in context.Set<Customer>().OrderBy(c => c.CustomerID).Take(5)
                        from c2 in context.Set<Customer>().Include(c => c.Orders)
                        select c2)
                        .ToListAsync();

                Assert.Equal(455, customers.Count);
                Assert.Equal(4150, customers.SelectMany(c => c.Orders).Count());
                Assert.True(customers.SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(455 + 466, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_collection_on_additional_from_clause2()
        {
            using (var context = CreateContext())
            {
                var customers
                    = await (from c1 in context.Set<Customer>().OrderBy(c => c.CustomerID).Take(5)
                        from c2 in context.Set<Customer>().Include(c => c.Orders)
                        select c1)
                        .ToListAsync();

                Assert.Equal(455, customers.Count);
                Assert.True(customers.All(c => c.Orders == null));
                Assert.Equal(5, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_collection_on_additional_from_clause_with_filter()
        {
            using (var context = CreateContext())
            {
                var customers
                    = await (from c1 in context.Set<Customer>()
                        from c2 in context.Set<Customer>()
                            .Include(c => c.Orders)
                            .Where(c => c.CustomerID == "ALFKI")
                        select c2)
                        .ToListAsync();

                Assert.Equal(91, customers.Count);
                Assert.Equal(546, customers.SelectMany(c => c.Orders).Count());
                Assert.True(customers.SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(1 + 6, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_collection_on_join_clause_with_filter()
        {
            using (var context = CreateContext())
            {
                var customers
                    = await (from c in context.Set<Customer>().Include(c => c.Orders)
                        join o in context.Set<Order>() on c.CustomerID equals o.CustomerID
                        where c.CustomerID == "ALFKI"
                        select c)
                        .ToListAsync();

                Assert.Equal(6, customers.Count);
                Assert.Equal(36, customers.SelectMany(c => c.Orders).Count());
                Assert.True(customers.SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(1 + 6, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual async Task Include_collection_on_join_clause_with_order_by_and_filter()
        {
            using (var context = CreateContext())
            {
                var customers
                    = await (from c in context.Set<Customer>().Include(c => c.Orders)
                        join o in context.Set<Order>() on c.CustomerID equals o.CustomerID
                        where c.CustomerID == "ALFKI"
                        orderby c.City
                        select c)
                        .ToListAsync();

                Assert.Equal(6, customers.Count);
                Assert.Equal(36, customers.SelectMany(c => c.Orders).Count());
                Assert.True(customers.SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(1 + 6, context.ChangeTracker.Entries().Count());
            }
        }

        protected abstract DbContext CreateContext();
    }
}
