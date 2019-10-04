// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable AccessToDisposedClosure
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class IncludeAsyncTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected IncludeAsyncTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual async Task Include_collection_order_by_subquery()
        {
            using (var context = CreateContext())
            {
                var customer
                    = await context.Set<Customer>()
                        .Include(c => c.Orders)
                        .Where(c => c.CustomerID == "ALFKI")
                        .OrderBy(c => c.Orders.OrderBy(o => o.EmployeeID).Select(o => o.OrderDate).FirstOrDefault())
                        .FirstOrDefaultAsync();

                Assert.NotNull(customer);
                Assert.NotNull(customer.Orders);
                Assert.Equal(6, customer.Orders.Count);
            }
        }

        [ConditionalFact]
        public virtual async Task Include_closes_reader()
        {
            using (var context = CreateContext())
            {
                var customer = await context.Set<Customer>().Include(c => c.Orders).FirstOrDefaultAsync();
                var products = await context.Products.ToListAsync();

                Assert.NotNull(customer);
                Assert.NotNull(products);
            }
        }

        [ConditionalFact]
        public virtual async Task Include_collection_alias_generation()
        {
            using (var context = CreateContext())
            {
                var orders
                    = await context.Set<Order>()
                        .Include(o => o.OrderDetails)
                        .ToListAsync();

                Assert.Equal(830, orders.Count);
            }
        }

        [ConditionalFact]
        public virtual async Task Include_collection_and_reference()
        {
            using (var context = CreateContext())
            {
                var orders
                    = await context.Set<Order>()
                        .Include(o => o.OrderDetails)
                        .Include(o => o.Customer)
                        .ToListAsync();

                Assert.Equal(830, orders.Count);
            }
        }

        [ConditionalFact]
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
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        [ConditionalFact]
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
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        [ConditionalFact]
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

        [ConditionalFact]
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

                Assert.NotEqual(orders, customer.Orders, ReferenceEqualityComparer.Instance);
                Assert.Equal(6, customer.Orders.Count);
                Assert.True(customer.Orders.All(o => o.Customer != null));
                Assert.Equal(6, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual async Task Include_collection_on_additional_from_clause_no_tracking()
        {
            using (var context = CreateContext())
            {
                var customers
                    = await (from c1 in context.Set<Customer>().OrderBy(c => c.CustomerID).Take(5)
                             from c2 in context.Set<Customer>().AsNoTracking().Include(c => c.Orders)
                             select c2)
                        .ToListAsync();

                Assert.Equal(455, customers.Count);
                Assert.Equal(4150, customers.SelectMany(c => c.Orders).Count());
                Assert.True(customers.SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact(Skip = "Issue #17068")]
        public virtual async Task Include_collection_on_group_join_clause_with_filter()
        {
            using (var context = CreateContext())
            {
                var customers
                    = await (from c in context.Set<Customer>().Include(c => c.Orders)
                             join o in context.Set<Order>() on c.CustomerID equals o.CustomerID into g
                             where c.CustomerID == "ALFKI"
                             select new { c, g })
                        .ToListAsync();

                Assert.Single(customers);
                Assert.Equal(6, customers.SelectMany(c => c.c.Orders).Count());
                Assert.True(customers.SelectMany(c => c.c.Orders).All(o => o.Customer != null));
                Assert.Equal(1 + 6, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact(Skip = "Issue#17068")]
        public virtual async Task Include_collection_on_inner_group_join_clause_with_filter()
        {
            using (var context = CreateContext())
            {
                var customers
                    = await (from c in context.Set<Customer>()
                             join o in context.Set<Order>().Include(o => o.OrderDetails)
                                 on c.CustomerID equals o.CustomerID into g
                             where c.CustomerID == "ALFKI"
                             select new { c, g })
                        .ToListAsync();

                Assert.Single(customers);
                Assert.Equal(6, customers.SelectMany(c => c.g).Count());
                Assert.True(customers.SelectMany(c => c.g).SelectMany(o => o.OrderDetails).All(od => od.Order != null));
                Assert.Equal(1 + 6 + 12, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact(Skip = "Issue #17068")]
        public virtual async Task Include_collection_when_groupby()
        {
            using (var context = CreateContext())
            {
                var customers
                    = await (from c in context.Set<Customer>().Include(c => c.Orders)
                             where c.CustomerID == "ALFKI"
                             group c by c.City)
                        .ToListAsync();

                Assert.Single(customers);
                Assert.Equal(6, customers.SelectMany(c => c.Single().Orders).Count());
                Assert.Equal(1 + 6, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual async Task Include_collection_principal_already_tracked()
        {
            using (var context = CreateContext())
            {
                var customer1
                    = await context.Set<Customer>()
                        .SingleAsync(c => c.CustomerID == "ALFKI");

                Assert.Single(context.ChangeTracker.Entries());

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

        [ConditionalFact]
        public virtual async Task Include_collection_principal_already_tracked_as_no_tracking()
        {
            using (var context = CreateContext())
            {
                var customer1
                    = await context.Set<Customer>()
                        .SingleAsync(c => c.CustomerID == "ALFKI");

                Assert.Single(context.ChangeTracker.Entries());

                var customer2
                    = await context.Set<Customer>()
                        .Include(c => c.Orders)
                        .AsNoTracking()
                        .SingleAsync(c => c.CustomerID == "ALFKI");

                Assert.Null(customer1.Orders);
                Assert.Equal(6, customer2.Orders.Count);
                Assert.True(customer2.Orders.All(o => o.Customer != null));
                Assert.Single(context.ChangeTracker.Entries());
            }
        }

        [ConditionalFact]
        public virtual async Task Include_collection_single_or_default_no_result()
        {
            using (var context = CreateContext())
            {
                var customer
                    = await context.Set<Customer>()
                        .Include(c => c.Orders)
                        .SingleOrDefaultAsync(c => c.CustomerID == "ALFKI ?");

                Assert.Null(customer);
            }
        }

        [ConditionalFact]
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
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        [ConditionalFact]
        public virtual async Task Include_collection_with_filter()
        {
            using (var context = CreateContext())
            {
                var customers
                    = await context.Set<Customer>()
                        .Include(c => c.Orders)
                        .Where(c => c.CustomerID == "ALFKI")
                        .ToListAsync();

                Assert.Single(customers);
                Assert.Equal(6, customers.SelectMany(c => c.Orders).Count());
                Assert.True(customers.SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(1 + 6, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual async Task Include_collection_with_filter_reordered()
        {
            using (var context = CreateContext())
            {
                var customers
                    = await context.Set<Customer>()
                        .Where(c => c.CustomerID == "ALFKI")
                        .Include(c => c.Orders)
                        .ToListAsync();

                Assert.Single(customers);
                Assert.Equal(6, customers.SelectMany(c => c.Orders).Count());
                Assert.True(customers.SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(1 + 6, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual async Task Include_duplicate_collection()
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

        [ConditionalFact]
        public virtual async Task Include_duplicate_collection_result_operator()
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

                Assert.Single(customers);
                Assert.Equal(6, customers.SelectMany(c => c.c1.Orders).Count());
                Assert.True(customers.SelectMany(c => c.c1.Orders).All(o => o.Customer != null));
                Assert.Equal(7, customers.SelectMany(c => c.c2.Orders).Count());
                Assert.True(customers.SelectMany(c => c.c2.Orders).All(o => o.Customer != null));
                Assert.Equal(15, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual async Task Include_duplicate_collection_result_operator2()
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

                Assert.Single(customers);
                Assert.Equal(6, customers.SelectMany(c => c.c1.Orders).Count());
                Assert.True(customers.SelectMany(c => c.c1.Orders).All(o => o.Customer != null));
                Assert.True(customers.All(c => c.c2.Orders == null));
                Assert.Equal(8, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual async Task Include_duplicate_reference()
        {
            using (var context = CreateContext())
            {
                var orders
                    = await (from o1 in context.Set<Order>()
                                 .Include(o => o.Customer)
                                 .OrderBy(o => o.CustomerID)
                                 .ThenBy(o => o.OrderID)
                                 .Take(2)
                             from o2 in context.Set<Order>()
                                 .Include(o => o.Customer)
                                 .OrderBy(o => o.CustomerID)
                                 .ThenBy(o => o.OrderID)
                                 .Skip(2)
                                 .Take(2)
                             select new { o1, o2 })
                        .ToListAsync();

                Assert.Equal(4, orders.Count);
                Assert.True(orders.All(o => o.o1.Customer != null));
                Assert.True(orders.All(o => o.o2.Customer != null));
                Assert.Single(orders.Select(o => o.o1.Customer).Distinct());
                Assert.Single(orders.Select(o => o.o2.Customer).Distinct());
                Assert.Equal(5, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual async Task Include_duplicate_reference2()
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

        [ConditionalFact]
        public virtual async Task Include_duplicate_reference3()
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

        [ConditionalFact]
        public virtual async Task Include_multi_level_reference_and_collection_predicate()
        {
            using (var context = CreateContext())
            {
                var order
                    = await context.Set<Order>()
                        .Include(o => o.Customer.Orders)
                        .SingleAsync(o => o.OrderID == 10248);

                Assert.NotNull(order.Customer);
                Assert.True(order.Customer.Orders.All(o => o != null));
            }
        }

        [ConditionalFact]
        public virtual async Task Include_collection_with_client_filter()
        {
            using (var context = CreateContext())
            {
                Assert.Contains(
                    CoreStrings.TranslationFailed("").Substring(21),
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => context.Set<Customer>()
                            .Include(c => c.Orders)
                            .Where(c => c.IsLondon)
                            .ToListAsync())).Message);
            }
        }

        [ConditionalFact]
        public virtual async Task Include_multi_level_collection_and_then_include_reference_predicate()
        {
            using (var context = CreateContext())
            {
                var order
                    = await context.Set<Order>()
                        .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                        .SingleAsync(o => o.OrderID == 10248);

                Assert.NotNull(order.OrderDetails);
                Assert.True(order.OrderDetails.Count > 0);
                Assert.True(order.OrderDetails.All(od => od.Product != null));
            }
        }

        [ConditionalFact]
        public virtual async Task Include_multiple_references()
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = await context.Set<OrderDetail>()
                        .Include(o => o.Order)
                        .Include(o => o.Product)
                        .ToListAsync();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(o => o.Order != null));
                Assert.True(orderDetails.All(o => o.Product != null));
                Assert.Equal(830, orderDetails.Select(o => o.Order).Distinct().Count());
                Assert.True(orderDetails.Select(o => o.Product).Distinct().Any());
            }
        }

        [ConditionalFact]
        public virtual async Task Include_multiple_references_and_collection_multi_level()
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = await context.Set<OrderDetail>()
                        .Include(od => od.Order.Customer.Orders)
                        .Include(od => od.Product)
                        .ToListAsync();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));
                Assert.True(orderDetails.All(od => od.Order.Customer.Orders != null));
            }
        }

        [ConditionalFact]
        public virtual async Task Include_multiple_references_and_collection_multi_level_reverse()
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = await context.Set<OrderDetail>()
                        .Include(od => od.Product)
                        .Include(od => od.Order.Customer.Orders)
                        .ToListAsync();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));
                Assert.True(orderDetails.All(od => od.Order.Customer.Orders != null));
            }
        }

        [ConditionalFact]
        public virtual async Task Include_multiple_references_multi_level()
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = await context.Set<OrderDetail>()
                        .Include(od => od.Order.Customer)
                        .Include(od => od.Product)
                        .ToListAsync();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));
            }
        }

        [ConditionalFact]
        public virtual async Task Include_multiple_references_multi_level_reverse()
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = await context.Set<OrderDetail>()
                        .Include(od => od.Product)
                        .Include(od => od.Order.Customer)
                        .ToListAsync();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));
            }
        }

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual async Task Include_reference_alias_generation()
        {
            using (var context = CreateContext())
            {
                var orders
                    = await context.Set<OrderDetail>()
                        .Include(o => o.Order)
                        .ToListAsync();

                Assert.True(orders.Count > 0);
            }
        }

        [ConditionalFact]
        public virtual async Task Include_reference_and_collection()
        {
            using (var context = CreateContext())
            {
                var orders
                    = await context.Set<Order>()
                        .Include(o => o.Customer)
                        .Include(o => o.OrderDetails)
                        .ToListAsync();

                Assert.Equal(830, orders.Count);
            }
        }

        [ConditionalFact]
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
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual async Task Include_reference_single_or_default_when_no_result()
        {
            using (var context = CreateContext())
            {
                var order
                    = await context.Set<Order>()
                        .Include(o => o.Customer)
                        .SingleOrDefaultAsync(o => o.OrderID == -1);

                Assert.Null(order);
            }
        }

        [ConditionalFact]
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
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        [ConditionalFact]
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
                Assert.Single(orders.Select(o => o.Customer).Distinct());
                Assert.Equal(6 + 1, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
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
                Assert.Single(orders.Select(o => o.Customer).Distinct());
                Assert.Equal(6 + 1, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual async Task Include_references_and_collection_multi_level()
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = await context.Set<OrderDetail>()
                        .Include(od => od.Order.Customer.Orders)
                        .ToListAsync();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));
                Assert.True(orderDetails.All(od => od.Order.Customer.Orders != null));
            }
        }

        [ConditionalFact]
        public virtual async Task Include_collection_then_include_collection()
        {
            using (var context = CreateContext())
            {
                var customers
                    = await context.Set<Customer>()
                        .Include(c => c.Orders).ThenInclude(o => o.OrderDetails)
                        .ToListAsync();

                Assert.Equal(91, customers.Count);
                Assert.True(customers.All(c => c.Orders != null));
                Assert.True(customers.All(c => c.Orders.All(o => o.OrderDetails != null)));
            }
        }

        [ConditionalFact]
        public virtual async Task Include_collection_then_include_collection_then_include_reference()
        {
            using (var context = CreateContext())
            {
                var customers
                    = await context.Set<Customer>()
                        .Include(c => c.Orders).ThenInclude(o => o.OrderDetails).ThenInclude(od => od.Product)
                        .ToListAsync();

                Assert.Equal(91, customers.Count);
                Assert.True(customers.All(c => c.Orders != null));
                Assert.True(customers.All(c => c.Orders.All(o => o.OrderDetails != null)));
            }
        }

        [ConditionalFact]
        public virtual async Task Include_collection_then_include_collection_predicate()
        {
            using (var context = CreateContext())
            {
                var customer
                    = await context.Set<Customer>()
                        .Include(c => c.Orders).ThenInclude(o => o.OrderDetails)
                        .SingleOrDefaultAsync(c => c.CustomerID == "ALFKI");

                Assert.NotNull(customer);
                Assert.Equal(6, customer.Orders.Count);
                Assert.True(customer.Orders.SelectMany(o => o.OrderDetails).Count() >= 6);
            }
        }

        [ConditionalFact]
        public virtual async Task Include_references_and_collection_multi_level_predicate()
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = await context.Set<OrderDetail>()
                        .Include(od => od.Order.Customer.Orders)
                        .Where(od => od.OrderID == 10248)
                        .ToListAsync();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));
                Assert.True(orderDetails.All(od => od.Order.Customer.Orders != null));
            }
        }

        [ConditionalFact]
        public virtual async Task Include_references_multi_level()
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = await context.Set<OrderDetail>()
                        .Include(od => od.Order.Customer)
                        .ToListAsync();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));
            }
        }

        [ConditionalFact]
        public virtual async Task Include_multi_level_reference_then_include_collection_predicate()
        {
            using (var context = CreateContext())
            {
                var order
                    = await context.Set<Order>()
                        .Include(o => o.Customer).ThenInclude(c => c.Orders)
                        .SingleAsync(o => o.OrderID == 10248);

                Assert.NotNull(order.Customer);
                Assert.True(order.Customer.Orders.All(o => o != null));
            }
        }

        [ConditionalFact]
        public virtual async Task Include_multiple_references_then_include_collection_multi_level()
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = await context.Set<OrderDetail>()
                        .Include(od => od.Order).ThenInclude(o => o.Customer).ThenInclude(c => c.Orders)
                        .Include(od => od.Product)
                        .ToListAsync();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));
                Assert.True(orderDetails.All(od => od.Order.Customer.Orders != null));
            }
        }

        [ConditionalFact]
        public virtual async Task Include_multiple_references_then_include_collection_multi_level_reverse()
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = await context.Set<OrderDetail>()
                        .Include(od => od.Product)
                        .Include(od => od.Order).ThenInclude(o => o.Customer).ThenInclude(c => c.Orders)
                        .ToListAsync();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));
                Assert.True(orderDetails.All(od => od.Order.Customer.Orders != null));
            }
        }

        [ConditionalFact]
        public virtual async Task Include_multiple_references_then_include_multi_level()
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = await context.Set<OrderDetail>()
                        .Include(od => od.Order).ThenInclude(o => o.Customer)
                        .Include(od => od.Product)
                        .ToListAsync();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));
            }
        }

        [ConditionalFact]
        public virtual async Task Include_multiple_references_then_include_multi_level_reverse()
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = await context.Set<OrderDetail>()
                        .Include(od => od.Product)
                        .Include(od => od.Order).ThenInclude(o => o.Customer)
                        .ToListAsync();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));
            }
        }

        [ConditionalFact]
        public virtual async Task Include_references_then_include_collection_multi_level()
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = await context.Set<OrderDetail>()
                        .Include(od => od.Order).ThenInclude(o => o.Customer).ThenInclude(c => c.Orders)
                        .ToListAsync();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));
                Assert.True(orderDetails.All(od => od.Order.Customer.Orders != null));
            }
        }

        [ConditionalFact]
        public virtual async Task Include_references_then_include_collection_multi_level_predicate()
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = await context.Set<OrderDetail>()
                        .Include(od => od.Order).ThenInclude(o => o.Customer).ThenInclude(c => c.Orders)
                        .Where(od => od.OrderID == 10248)
                        .ToListAsync();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));
                Assert.True(orderDetails.All(od => od.Order.Customer.Orders != null));
            }
        }

        [ConditionalFact]
        public virtual async Task Include_references_then_include_multi_level()
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = await context.Set<OrderDetail>()
                        .Include(od => od.Order).ThenInclude(o => o.Customer)
                        .ToListAsync();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));
            }
        }

        protected NorthwindContext CreateContext() => Fixture.CreateContext();
    }
}
