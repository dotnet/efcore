// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable StringStartsWithIsCultureSpecific
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class IncludeTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected IncludeTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        [Fact]
        public virtual void Include_reference_invalid()
        {
            Assert.Throws<InvalidOperationException>(
                () =>
                    {
                        using (var context = CreateContext())
                        {
                            return context.Set<Order>()
                                .Include(o => o.Customer.CustomerID)
                                .ToList();
                        }
                    });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_reference_and_collection_order_by(bool useString)
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = useString
                        ? context.Set<Order>()
                            .Include("Customer.Orders")
                            .OrderBy(o => o.OrderID)
                            .ToList()
                        : context.Set<Order>()
                            .Include(o => o.Customer.Orders)
                            .OrderBy(o => o.OrderID)
                            .ToList();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(o => o.Customer != null));
                Assert.True(orderDetails.All(o => o.Customer.Orders != null));
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_references_then_include_collection(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? context.Set<Order>()
                            .Include("Customer.Orders")
                            .ToList()
                        : context.Set<Order>()
                            .Include(o => o.Customer).ThenInclude(c => c.Orders)
                            .ToList();

                Assert.True(orders.Count > 0);
                Assert.True(orders.All(od => od.Customer != null));
                Assert.True(orders.All(od => od.Customer.Orders != null));
            }
        }

        [Fact]
        public virtual void Include_bad_navigation_property()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.IncludeBadNavigation("CustomerID", nameof(Customer)),
                    Assert.Throws<InvalidOperationException>(
                        () => context.Set<Order>().Include("Customer.CustomerID").ToList()).Message);
            }
        }

        [Fact]
        public virtual void Include_property_expression_invalid()
        {
            var anonymousType = new { Customer = default(Customer), OrderDetails = default(ICollection<OrderDetail>) }.GetType();
            var lambdaExpression = Expression.Lambda(
                Expression.New(
                    anonymousType.GetTypeInfo().DeclaredConstructors.First(),
                    new List<Expression>
                    {
                        Expression.MakeMemberAccess(
                            Expression.Parameter(typeof(Order), "o"),
                            typeof(Order).GetTypeInfo().DeclaredMembers.Single(m => m.Name == "Customer")),
                        Expression.MakeMemberAccess(
                            Expression.Parameter(typeof(Order), "o"),
                            typeof(Order).GetTypeInfo().DeclaredMembers.Single(m => m.Name == "OrderDetails"))
                    },
                    anonymousType.GetTypeInfo().DeclaredMembers.Single(m => m.Name == "Customer"),
                    anonymousType.GetTypeInfo().DeclaredMembers.Single(m => m.Name == "OrderDetails")
                ),
                Expression.Parameter(typeof(Order), "o"));

            Assert.Equal(
                CoreStrings.InvalidIncludeLambdaExpression("Include", lambdaExpression.ToString()),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        {
                            using (var context = CreateContext())
                            {
                                context.Set<Order>()
                                    .Include(o => new { o.Customer, o.OrderDetails })
                                    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                                    .ToList();
                            }
                        }).Message);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Then_include_collection_order_by_collection_column(bool useString)
        {
            using (var context = CreateContext())
            {
                var customer
                    = useString
                        ? context.Set<Customer>()
                            .Include("Orders.OrderDetails")
                            .Where(c => c.CustomerID.StartsWith("W"))
                            .OrderByDescending(c => c.Orders.OrderByDescending(oo => oo.OrderDate).FirstOrDefault().OrderDate)
                            .FirstOrDefault()
                        : context.Set<Customer>()
                            .Include(c => c.Orders)
                            .ThenInclude(o => o.OrderDetails)
                            .Where(c => c.CustomerID.StartsWith("W"))
                            .OrderByDescending(c => c.Orders.OrderByDescending(oo => oo.OrderDate).FirstOrDefault().OrderDate)
                            .FirstOrDefault();

                Assert.NotNull(customer);
                Assert.Equal("WHITC", customer.CustomerID);
                Assert.NotNull(customer.Orders);
                Assert.Equal(14, customer.Orders.Count);
                Assert.NotNull(customer.Orders.First().OrderDetails);
                Assert.Equal(2, customer.Orders.First().OrderDetails.Count);
                Assert.NotNull(customer.Orders.Last().OrderDetails);
                Assert.Equal(3, customer.Orders.Last().OrderDetails.Count);

                CheckIsLoaded(
                    context,
                    customer,
                    ordersLoaded: true,
                    orderDetailsLoaded: true,
                    productLoaded: false);
            }
        }

        [Fact]
        public virtual void Then_include_property_expression_invalid()
        {
            var anonymousType = new { Customer = default(Customer), OrderDetails = default(ICollection<OrderDetail>) }.GetType();
            var lambdaExpression = Expression.Lambda(
                Expression.New(
                    anonymousType.GetTypeInfo().DeclaredConstructors.First(),
                    new List<Expression>
                    {
                        Expression.MakeMemberAccess(
                            Expression.Parameter(typeof(Order), "o"),
                            typeof(Order).GetTypeInfo().DeclaredMembers.Single(m => m.Name == "Customer")),
                        Expression.MakeMemberAccess(
                            Expression.Parameter(typeof(Order), "o"),
                            typeof(Order).GetTypeInfo().DeclaredMembers.Single(m => m.Name == "OrderDetails"))
                    },
                    anonymousType.GetTypeInfo().DeclaredMembers.Single(m => m.Name == "Customer"),
                    anonymousType.GetTypeInfo().DeclaredMembers.Single(m => m.Name == "OrderDetails")
                ),
                Expression.Parameter(typeof(Order), "o"));

            Assert.Equal(
                CoreStrings.InvalidIncludeLambdaExpression("ThenInclude", lambdaExpression.ToString()),
                Assert.Throws<ArgumentException>(
                    () =>
                        {
                            using (var context = CreateContext())
                            {
                                context.Set<Customer>()
                                    .Include(o => o.Orders)
                                    .ThenInclude(o => new { o.Customer, o.OrderDetails })
                                    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                                    .ToList();
                            }
                        }).Message);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_closes_reader(bool useString)
        {
            using (var context = CreateContext())
            {
                var customer = useString
                    ? context.Set<Customer>().Include("Orders").FirstOrDefault()
                    : context.Set<Customer>().Include(c => c.Orders).FirstOrDefault();

                var products = context.Products.ToList();

                Assert.NotNull(customer);
                Assert.NotNull(products);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_when_result_operator(bool useString)
        {
            using (var context = CreateContext())
            {
                var any
                    = useString
                        ? context.Set<Customer>()
                            .Include("Orders")
                            .Any()
                        : context.Set<Customer>()
                            .Include(c => c.Orders)
                            .Any();

                Assert.True(any);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? context.Set<Customer>()
                            .Include("Orders")
                            .ToList()
                        : context.Set<Customer>()
                            .Include(c => c.Orders)
                            .ToList();

                Assert.Equal(91, customers.Count);
                Assert.Equal(830, customers.Where(c => c.Orders != null).SelectMany(c => c.Orders).Count());
                Assert.True(customers.Where(c => c.Orders != null).SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(91 + 830, context.ChangeTracker.Entries().Count());

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_then_reference(bool useString)
        {
            using (var context = CreateContext())
            {
                var products
                    = useString
                        ? context.Products
                            .Include("OrderDetails.Order")
                            .ToList()
                        : context.Products
                            .Include(p => p.OrderDetails)
                            .ThenInclude(od => od.Order)
                            .ToList();

                Assert.Equal(77, products.Count);
                Assert.Equal(2155, products.SelectMany(p => p.OrderDetails).Count());
                Assert.Equal(2155, products.SelectMany(p => p.OrderDetails).Count(od => od.Order != null));
                Assert.Equal(77 + 2155 + 830, context.ChangeTracker.Entries().Count());

                foreach (var product in products)
                {
                    CheckIsLoaded(
                        context,
                        product,
                        orderDetailsLoaded: true,
                        orderLoaded: true);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_with_last(bool useString)
        {
            using (var context = CreateContext())
            {
                var customer
                    = useString
                        ? context.Set<Customer>()
                            .Include("Orders")
                            .OrderBy(c => c.CompanyName)
                            .Last()
                        : context.Set<Customer>()
                            .Include(c => c.Orders)
                            .OrderBy(c => c.CompanyName)
                            .Last();

                Assert.NotNull(customer);
                Assert.Equal(7, customer.Orders.Count);
                Assert.Equal(8, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_with_last_no_orderby(bool useString)
        {
            using (var context = CreateContext())
            {
                var customer
                    = useString
                        ? context.Set<Customer>()
                            .Include("Orders")
                            .Last()
                        : context.Set<Customer>()
                            .Include(c => c.Orders)
                            .Last();

                Assert.NotNull(customer);
                Assert.Equal(7, customer.Orders.Count);
                Assert.Equal(8, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_skip_no_order_by(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? context.Set<Customer>()
                            .Skip(10)
                            .Include("Orders")
                            .ToList()
                        : context.Set<Customer>()
                            .Skip(10)
                            .Include(c => c.Orders)
                            .ToList();

                Assert.Equal(81, customers.Count);
                Assert.True(customers.All(c => c.Orders != null));

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_take_no_order_by(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = context.Set<Customer>()
                        .Take(10)
                        .Include(c => c.Orders)
                        .ToList();

                Assert.Equal(10, customers.Count);
                Assert.True(customers.All(c => c.Orders != null));
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_skip_take_no_order_by(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? context.Set<Customer>()
                            .Skip(10)
                            .Take(5)
                            .Include("Orders")
                            .ToList()
                        : context.Set<Customer>()
                            .Skip(10)
                            .Take(5)
                            .Include(c => c.Orders)
                            .ToList();

                Assert.Equal(5, customers.Count);
                Assert.True(customers.All(c => c.Orders != null));

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_list(bool useString)
        {
            using (var context = CreateContext())
            {
                var products
                    = useString
                        ? context.Set<Product>()
                            .Include("OrderDetails.Order")
                            .ToList()
                        : context.Set<Product>()
                            .Include(p => p.OrderDetails).ThenInclude(od => od.Order)
                            .ToList();

                Assert.Equal(77, products.Count);

                foreach (var product in products)
                {
                    CheckIsLoaded(
                        context,
                        product,
                        orderDetailsLoaded: true,
                        orderLoaded: true);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_alias_generation(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? context.Set<Order>()
                            .Include("OrderDetails")
                            .ToList()
                        : context.Set<Order>()
                            .Include(o => o.OrderDetails)
                            .ToList();

                Assert.Equal(830, orders.Count);

                foreach (var order in orders)
                {
                    CheckIsLoaded(
                        context,
                        order,
                        orderDetailsLoaded: true,
                        productLoaded: false,
                        customerLoaded: false,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_and_reference(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? context.Set<Order>()
                            .Include("OrderDetails")
                            .Include("Customer")
                            .ToList()
                        : context.Set<Order>()
                            .Include(o => o.OrderDetails)
                            .Include(o => o.Customer)
                            .ToList();

                Assert.Equal(830, orders.Count);

                foreach (var order in orders)
                {
                    CheckIsLoaded(
                        context,
                        order,
                        orderDetailsLoaded: true,
                        productLoaded: false,
                        customerLoaded: true,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_as_no_tracking(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? context.Set<Customer>()
                            .Include("Orders")
                            .AsNoTracking()
                            .ToList()
                        : context.Set<Customer>()
                            .Include(c => c.Orders)
                            .AsNoTracking()
                            .ToList();

                Assert.Equal(91, customers.Count);
                Assert.Equal(830, customers.Where(c => c.Orders != null).SelectMany(c => c.Orders).Count());
                Assert.True(customers.Where(c => c.Orders != null).SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(0, context.ChangeTracker.Entries().Count());

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: false,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_as_no_tracking2(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? context.Set<Customer>()
                            .AsNoTracking()
                            .OrderBy(c => c.CustomerID)
                            .Take(5)
                            .Include("Orders")
                            .ToList()
                        : context.Set<Customer>()
                            .AsNoTracking()
                            .OrderBy(c => c.CustomerID)
                            .Take(5)
                            .Include(c => c.Orders)
                            .ToList();

                Assert.Equal(5, customers.Count);
                Assert.Equal(48, customers.Where(c => c.Orders != null).SelectMany(c => c.Orders).Count());
                Assert.True(customers.Where(c => c.Orders != null).SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(0, context.ChangeTracker.Entries().Count());

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: false,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_dependent_already_tracked(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = context.Set<Order>()
                        .Where(o => o.CustomerID == "ALFKI")
                        .ToList();

                Assert.Equal(6, context.ChangeTracker.Entries().Count());

                var customer
                    = useString
                        ? context.Set<Customer>()
                            .Include("Orders")
                            .Single(c => c.CustomerID == "ALFKI")
                        : context.Set<Customer>()
                            .Include(c => c.Orders)
                            .Single(c => c.CustomerID == "ALFKI");

                Assert.Equal(orders, customer.Orders, ReferenceEqualityComparer.Instance);
                Assert.Equal(6, customer.Orders.Count);
                Assert.True(customer.Orders.All(o => o.Customer != null));
                Assert.Equal(6 + 1, context.ChangeTracker.Entries().Count());

                CheckIsLoaded(
                    context,
                    customer,
                    ordersLoaded: true,
                    orderDetailsLoaded: false,
                    productLoaded: false);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_dependent_already_tracked_as_no_tracking(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = context.Set<Order>()
                        .Where(o => o.CustomerID == "ALFKI")
                        .ToList();

                Assert.Equal(6, context.ChangeTracker.Entries().Count());

                var customer
                    = useString
                        ? context.Set<Customer>()
                            .Include("Orders")
                            .AsNoTracking()
                            .Single(c => c.CustomerID == "ALFKI")
                        : context.Set<Customer>()
                            .Include(c => c.Orders)
                            .AsNoTracking()
                            .Single(c => c.CustomerID == "ALFKI");

                Assert.NotEqual(orders, customer.Orders, ReferenceEqualityComparer.Instance);
                Assert.Equal(6, customer.Orders.Count);
                Assert.True(customer.Orders.All(o => o.Customer != null));
                Assert.Equal(6, context.ChangeTracker.Entries().Count());

                CheckIsLoaded(
                    context,
                    customer,
                    ordersLoaded: false,
                    orderDetailsLoaded: false,
                    productLoaded: false);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_on_additional_from_clause(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? (from c1 in context.Set<Customer>().OrderBy(c => c.CustomerID).Take(5)
                           from c2 in context.Set<Customer>().Include("Orders")
                           select c2)
                        .ToList()
                        : (from c1 in context.Set<Customer>().OrderBy(c => c.CustomerID).Take(5)
                           from c2 in context.Set<Customer>().Include(c2 => c2.Orders)
                           select c2)
                        .ToList();

                Assert.Equal(455, customers.Count);
                Assert.Equal(4150, customers.SelectMany(c => c.Orders).Count());
                Assert.True(customers.SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(455 + 466, context.ChangeTracker.Entries().Count());

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_on_additional_from_clause_no_tracking(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? (from c1 in context.Set<Customer>().OrderBy(c => c.CustomerID).Take(5)
                           from c2 in context.Set<Customer>().AsNoTracking().Include(c => c.Orders)
                           select c2)
                        .ToList()
                        : (from c1 in context.Set<Customer>().OrderBy(c => c.CustomerID).Take(5)
                           from c2 in context.Set<Customer>().AsNoTracking().Include(c => c.Orders)
                           select c2)
                        .ToList();

                Assert.Equal(455, customers.Count);
                Assert.Equal(4150, customers.SelectMany(c => c.Orders).Count());
                Assert.True(customers.SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(0, context.ChangeTracker.Entries().Count());

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: false,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_on_additional_from_clause_with_filter(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? (from c1 in context.Set<Customer>()
                           from c2 in context.Set<Customer>()
                               .Include("Orders")
                               .Where(c => c.CustomerID == "ALFKI")
                           select c2)
                        .ToList()
                        : (from c1 in context.Set<Customer>()
                           from c2 in context.Set<Customer>()
                               .Include(c => c.Orders)
                               .Where(c => c.CustomerID == "ALFKI")
                           select c2)
                        .ToList();

                Assert.Equal(91, customers.Count);
                Assert.Equal(546, customers.SelectMany(c => c.Orders).Count());
                Assert.True(customers.SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(1 + 6, context.ChangeTracker.Entries().Count());

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_on_additional_from_clause2(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? (from c1 in context.Set<Customer>().OrderBy(c => c.CustomerID).Take(5)
                           from c2 in context.Set<Customer>().Include("Orders")
                           select c1)
                        .ToList()
                        : (from c1 in context.Set<Customer>().OrderBy(c => c.CustomerID).Take(5)
                           from c2 in context.Set<Customer>().Include(c2 => c2.Orders)
                           select c1)
                        .ToList();

                Assert.Equal(455, customers.Count);
                Assert.True(customers.All(c => c.Orders == null));
                Assert.Equal(5, context.ChangeTracker.Entries().Count());

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: false,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_where_skip_take_projection(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? context.OrderDetails.Include("Order")
                            .Where(od => od.Quantity == 10)
                            .OrderBy(od => od.OrderID)
                            .ThenBy(od => od.ProductID)
                            .Skip(1)
                            .Take(2)
                            .Select(
                                od =>
                                    new
                                    {
                                        od.Order.CustomerID
                                    })
                            .ToList()
                        : context.OrderDetails.Include(od => od.Order)
                            .Where(od => od.Quantity == 10)
                            .OrderBy(od => od.OrderID)
                            .ThenBy(od => od.ProductID)
                            .Skip(1)
                            .Take(2)
                            .Select(
                                od =>
                                    new
                                    {
                                        od.Order.CustomerID
                                    })
                            .ToList();

                Assert.Equal(2, orders.Count);
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_on_join_clause_with_filter(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? (from c in context.Set<Customer>().Include("Orders")
                           join o in context.Set<Order>() on c.CustomerID equals o.CustomerID
                           where c.CustomerID == "ALFKI"
                           select c)
                        .ToList()
                        : (from c in context.Set<Customer>().Include(c => c.Orders)
                           join o in context.Set<Order>() on c.CustomerID equals o.CustomerID
                           where c.CustomerID == "ALFKI"
                           select c)
                        .ToList();

                Assert.Equal(6, customers.Count);
                Assert.Equal(36, customers.SelectMany(c => c.Orders).Count());
                Assert.True(customers.SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(1 + 6, context.ChangeTracker.Entries().Count());

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_on_join_clause_with_order_by_and_filter(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? (from c in context.Set<Customer>().Include("Orders")
                           join o in context.Set<Order>() on c.CustomerID equals o.CustomerID
                           where c.CustomerID == "ALFKI"
                           orderby c.City
                           select c)
                        .ToList()
                        : (from c in context.Set<Customer>().Include(c => c.Orders)
                           join o in context.Set<Order>() on c.CustomerID equals o.CustomerID
                           where c.CustomerID == "ALFKI"
                           orderby c.City
                           select c)
                        .ToList();

                Assert.Equal(6, customers.Count);
                Assert.Equal(36, customers.SelectMany(c => c.Orders).Count());
                Assert.True(customers.SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(1 + 6, context.ChangeTracker.Entries().Count());

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_on_group_join_clause_with_filter(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? (from c in context.Set<Customer>().Include("Orders.Customer")
                           join o in context.Set<Order>() on c.CustomerID equals o.CustomerID into g
                           where c.CustomerID == "ALFKI"
                           select new { c, g })
                        .ToList()
                        : (from c in context.Set<Customer>().Include(c => c.Orders).ThenInclude(o => o.Customer)
                           join o in context.Set<Order>() on c.CustomerID equals o.CustomerID into g
                           where c.CustomerID == "ALFKI"
                           select new { c, g })
                        .ToList();

                Assert.Equal(1, customers.Count);
                Assert.Equal(6, customers.SelectMany(c => c.c.Orders).Count());
                Assert.True(customers.SelectMany(c => c.c.Orders).All(o => o.Customer != null));
                Assert.Equal(1 + 6, context.ChangeTracker.Entries().Count());

                foreach (var customer in customers.Select(a => a.c))
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_on_inner_group_join_clause_with_filter(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? (from c in context.Set<Customer>()
                           join o in context.Set<Order>().Include("OrderDetails").Include("Customer")
                               on c.CustomerID equals o.CustomerID into g
                           where c.CustomerID == "ALFKI"
                           select new { c, g })
                        .ToList()
                        : (from c in context.Set<Customer>()
                           join o in context.Set<Order>().Include(o => o.OrderDetails).Include(o => o.Customer)
                               on c.CustomerID equals o.CustomerID into g
                           where c.CustomerID == "ALFKI"
                           select new { c, g })
                        .ToList();

                Assert.Equal(1, customers.Count);
                Assert.Equal(6, customers.SelectMany(c => c.g).Count());
                Assert.True(customers.SelectMany(c => c.g).SelectMany(o => o.OrderDetails).All(od => od.Order != null));
                Assert.Equal(1 + 6 + 12, context.ChangeTracker.Entries().Count());

                foreach (var order in customers.SelectMany(a => a.c.Orders))
                {
                    CheckIsLoaded(
                        context,
                        order,
                        orderDetailsLoaded: true,
                        productLoaded: false,
                        customerLoaded: true,
                        ordersLoaded: false);
                }
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_when_groupby(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? (from c in context.Set<Customer>().Include("Orders")
                           where c.CustomerID == "ALFKI"
                           group c by c.City)
                        .ToList()
                        : (from c in context.Set<Customer>().Include(c => c.Orders)
                           where c.CustomerID == "ALFKI"
                           group c by c.City)
                        .ToList();

                Assert.Equal(1, customers.Count);
                Assert.Equal(6, customers.SelectMany(c => c.Single().Orders).Count());
                Assert.Equal(1 + 6, context.ChangeTracker.Entries().Count());

                foreach (var customer in customers.Select(e => e.Single()))
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_when_groupby_subquery(bool useString)
        {
            using (var context = CreateContext())
            {
                var grouping
                    = useString
                        ? (from c in context.Set<Customer>()
                               .Include("Orders.OrderDetails.Product")
                           where c.CustomerID == "ALFKI"
                           group c by c.City)
                        .SingleOrDefault()
                        : (from c in context.Set<Customer>()
                               .Include(c => c.Orders)
                               .ThenInclude(o => o.OrderDetails)
                               .ThenInclude(od => od.Product)
                           where c.CustomerID == "ALFKI"
                           group c by c.City)
                        .SingleOrDefault();

                Assert.NotNull(grouping);
                Assert.Equal(6, grouping.SelectMany(c => c.Orders).Count());
                Assert.True(grouping.SelectMany(c => c.Orders).SelectMany(o => o.OrderDetails).All(od => od.Product != null));
                Assert.Equal(30, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_order_by_collection_column(bool useString)
        {
            using (var context = CreateContext())
            {
                var customer
                    = useString
                        ? context.Set<Customer>()
                            .Include("Orders")
                            .Where(c => c.CustomerID.StartsWith("W"))
                            .OrderByDescending(c => c.Orders.OrderByDescending(oo => oo.OrderDate).FirstOrDefault().OrderDate)
                            .FirstOrDefault()
                        : context.Set<Customer>()
                            .Include(c => c.Orders)
                            .Where(c => c.CustomerID.StartsWith("W"))
                            .OrderByDescending(c => c.Orders.OrderByDescending(oo => oo.OrderDate).FirstOrDefault().OrderDate)
                            .FirstOrDefault();

                Assert.NotNull(customer);
                Assert.Equal("WHITC", customer.CustomerID);
                Assert.NotNull(customer.Orders);
                Assert.Equal(14, customer.Orders.Count);

                CheckIsLoaded(
                    context,
                    customer,
                    ordersLoaded: true,
                    orderDetailsLoaded: false,
                    productLoaded: false);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_order_by_key(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? context.Set<Customer>()
                            .Include("Orders")
                            .OrderBy(c => c.CustomerID)
                            .ToList()
                        : context.Set<Customer>()
                            .Include(c => c.Orders)
                            .OrderBy(c => c.CustomerID)
                            .ToList();

                Assert.Equal(91, customers.Count);
                Assert.Equal(830, customers.Where(c => c.Orders != null).SelectMany(c => c.Orders).Count());
                Assert.True(customers.Where(c => c.Orders != null).SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(91 + 830, context.ChangeTracker.Entries().Count());

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_order_by_non_key(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? context.Set<Customer>()
                            .Include("Orders")
                            .OrderBy(c => c.City)
                            .ToList()
                        : context.Set<Customer>()
                            .Include(c => c.Orders)
                            .OrderBy(c => c.City)
                            .ToList();

                Assert.Equal(91, customers.Count);
                Assert.Equal(830, customers.Where(c => c.Orders != null).SelectMany(c => c.Orders).Count());
                Assert.True(customers.Where(c => c.Orders != null).SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(91 + 830, context.ChangeTracker.Entries().Count());

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_order_by_non_key_with_take(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? context.Set<Customer>()
                            .Include("Orders")
                            .OrderBy(c => c.ContactTitle)
                            .Take(10)
                            .ToList()
                        : context.Set<Customer>()
                            .Include(c => c.Orders)
                            .OrderBy(c => c.ContactTitle)
                            .Take(10)
                            .ToList();

                Assert.Equal(10, customers.Count);
                Assert.Equal(116, customers.Where(c => c.Orders != null).SelectMany(c => c.Orders).Count());
                Assert.True(customers.Where(c => c.Orders != null).SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(10 + 116, context.ChangeTracker.Entries().Count());

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_order_by_non_key_with_skip(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? context.Set<Customer>()
                            .Include("Orders")
                            .OrderBy(c => c.ContactTitle)
                            .Skip(10)
                            .ToList()
                        : context.Set<Customer>()
                            .Include(c => c.Orders)
                            .OrderBy(c => c.ContactTitle)
                            .Skip(10)
                            .ToList();

                Assert.Equal(81, customers.Count);
                Assert.Equal(714, customers.Where(c => c.Orders != null).SelectMany(c => c.Orders).Count());
                Assert.True(customers.Where(c => c.Orders != null).SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(81, context.ChangeTracker.Entries().Count(e => e.Entity is Customer));
                Assert.Equal(714, context.ChangeTracker.Entries().Count(e => e.Entity is Order));
                Assert.Equal(81 + 714, context.ChangeTracker.Entries().Count());

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_order_by_non_key_with_first_or_default(bool useString)
        {
            using (var context = CreateContext())
            {
                var customer
                    = useString
                        ? context.Set<Customer>()
                            .Include("Orders")
                            .OrderByDescending(c => c.CompanyName)
                            .FirstOrDefault()
                        : context.Set<Customer>()
                            .Include(c => c.Orders)
                            .OrderByDescending(c => c.CompanyName)
                            .FirstOrDefault();

                Assert.NotNull(customer);
                Assert.Equal(7, customer.Orders.Count);
                Assert.True(customer.Orders.All(o => o.Customer != null));
                Assert.Equal(1 + 7, context.ChangeTracker.Entries().Count());

                CheckIsLoaded(
                    context,
                    customer,
                    ordersLoaded: true,
                    orderDetailsLoaded: false,
                    productLoaded: false);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_order_by_subquery(bool useString)
        {
            using (var context = CreateContext())
            {
                var customer
                    = useString
                        ? context.Set<Customer>()
                            .Include("Orders")
                            .Where(c => c.CustomerID == "ALFKI")
                            .OrderBy(c => c.Orders.OrderBy(o => o.EmployeeID).Select(o => o.OrderDate).FirstOrDefault())
                            .FirstOrDefault()
                        : context.Set<Customer>()
                            .Include(c => c.Orders)
                            .Where(c => c.CustomerID == "ALFKI")
                            .OrderBy(c => c.Orders.OrderBy(o => o.EmployeeID).Select(o => o.OrderDate).FirstOrDefault())
                            .FirstOrDefault();

                Assert.NotNull(customer);
                Assert.NotNull(customer.Orders);
                Assert.Equal(6, customer.Orders.Count);

                CheckIsLoaded(
                    context,
                    customer,
                    ordersLoaded: true,
                    orderDetailsLoaded: false,
                    productLoaded: false);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_principal_already_tracked(bool useString)
        {
            using (var context = CreateContext())
            {
                var customer1
                    = context.Set<Customer>()
                        .Single(c => c.CustomerID == "ALFKI");

                Assert.Equal(1, context.ChangeTracker.Entries().Count());

                var customer2
                    = useString
                        ? context.Set<Customer>()
                            .Include("Orders")
                            .Single(c => c.CustomerID == "ALFKI")
                        : context.Set<Customer>()
                            .Include(c => c.Orders)
                            .Single(c => c.CustomerID == "ALFKI");

                Assert.Same(customer1, customer2);
                Assert.Equal(6, customer2.Orders.Count);
                Assert.True(customer2.Orders.All(o => o.Customer != null));
                Assert.Equal(1 + 6, context.ChangeTracker.Entries().Count());

                CheckIsLoaded(
                    context,
                    customer2,
                    ordersLoaded: true,
                    orderDetailsLoaded: false,
                    productLoaded: false);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_principal_already_tracked_as_no_tracking(bool useString)
        {
            using (var context = CreateContext())
            {
                var customer1
                    = context.Set<Customer>()
                        .Single(c => c.CustomerID == "ALFKI");

                Assert.Equal(1, context.ChangeTracker.Entries().Count());

                var customer2
                    = useString
                        ? context.Set<Customer>()
                            .Include("Orders")
                            .AsNoTracking()
                            .Single(c => c.CustomerID == "ALFKI")
                        : context.Set<Customer>()
                            .Include(c => c.Orders)
                            .AsNoTracking()
                            .Single(c => c.CustomerID == "ALFKI");

                Assert.Equal(customer1.CustomerID, customer2.CustomerID);
                Assert.Null(customer1.Orders);
                Assert.Equal(6, customer2.Orders.Count);
                Assert.True(customer2.Orders.All(o => o.Customer != null));
                Assert.Equal(1, context.ChangeTracker.Entries().Count());

                CheckIsLoaded(
                    context,
                    customer2,
                    ordersLoaded: false,
                    orderDetailsLoaded: false,
                    productLoaded: false);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_single_or_default_no_result(bool useString)
        {
            using (var context = CreateContext())
            {
                var customer
                    = useString
                        ? context.Set<Customer>()
                            .Include("Orders")
                            .SingleOrDefault(c => c.CustomerID == "ALFKI ?")
                        : context.Set<Customer>()
                            .Include(c => c.Orders)
                            .SingleOrDefault(c => c.CustomerID == "ALFKI ?");

                Assert.Null(customer);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_when_projection(bool useString)
        {
            using (var context = CreateContext())
            {
                var productIds
                    = useString
                        ? context.Set<Customer>()
                            .Include(c => c.Orders)
                            .Select(c => c.CustomerID)
                            .ToList()
                        : context.Set<Customer>()
                            .Include("Orders")
                            .Select(c => c.CustomerID)
                            .ToList();

                Assert.Equal(91, productIds.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_with_filter(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? context.Set<Customer>()
                            .Include("Orders")
                            .Where(c => c.CustomerID == "ALFKI")
                            .ToList()
                        : context.Set<Customer>()
                            .Include(c => c.Orders)
                            .Where(c => c.CustomerID == "ALFKI")
                            .ToList();

                Assert.Equal(1, customers.Count);
                Assert.Equal(6, customers.SelectMany(c => c.Orders).Count());
                Assert.True(customers.SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(1 + 6, context.ChangeTracker.Entries().Count());

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_with_filter_reordered(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? context.Set<Customer>()
                            .Where(c => c.CustomerID == "ALFKI")
                            .Include("Orders")
                            .ToList()
                        : context.Set<Customer>()
                            .Where(c => c.CustomerID == "ALFKI")
                            .Include(c => c.Orders)
                            .ToList();

                Assert.Equal(1, customers.Count);
                Assert.Equal(6, customers.SelectMany(c => c.Orders).Count());
                Assert.True(customers.SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(1 + 6, context.ChangeTracker.Entries().Count());

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_duplicate_collection(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? (from c1 in context.Set<Customer>()
                               .Include("Orders")
                               .OrderBy(c => c.CustomerID)
                               .Take(2)
                           from c2 in context.Set<Customer>()
                               .Include("Orders")
                               .OrderBy(c => c.CustomerID)
                               .Skip(2)
                               .Take(2)
                           select new { c1, c2 })
                        .ToList()
                        : (from c1 in context.Set<Customer>()
                               .Include(c => c.Orders)
                               .OrderBy(c => c.CustomerID)
                               .Take(2)
                           from c2 in context.Set<Customer>()
                               .Include(c => c.Orders)
                               .OrderBy(c => c.CustomerID)
                               .Skip(2)
                               .Take(2)
                           select new { c1, c2 })
                        .ToList();

                Assert.Equal(4, customers.Count);
                Assert.Equal(20, customers.SelectMany(c => c.c1.Orders).Count());
                Assert.True(customers.SelectMany(c => c.c1.Orders).All(o => o.Customer != null));
                Assert.Equal(40, customers.SelectMany(c => c.c2.Orders).Count());
                Assert.True(customers.SelectMany(c => c.c2.Orders).All(o => o.Customer != null));
                Assert.Equal(34, context.ChangeTracker.Entries().Count());

                foreach (var customer in customers.Select(e => e.c1))
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }

                foreach (var customer in customers.Select(e => e.c2))
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_duplicate_collection_result_operator(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? (from c1 in context.Set<Customer>()
                               .Include("Orders")
                               .OrderBy(c => c.CustomerID)
                               .Take(2)
                           from c2 in context.Set<Customer>()
                               .Include("Orders")
                               .OrderBy(c => c.CustomerID)
                               .Skip(2)
                               .Take(2)
                           select new { c1, c2 })
                        .Take(1)
                        .ToList()
                        : (from c1 in context.Set<Customer>()
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
                        .ToList();

                Assert.Equal(1, customers.Count);
                Assert.Equal(6, customers.SelectMany(c => c.c1.Orders).Count());
                Assert.True(customers.SelectMany(c => c.c1.Orders).All(o => o.Customer != null));
                Assert.Equal(7, customers.SelectMany(c => c.c2.Orders).Count());
                Assert.True(customers.SelectMany(c => c.c2.Orders).All(o => o.Customer != null));
                Assert.Equal(15, context.ChangeTracker.Entries().Count());

                foreach (var customer in customers.Select(e => e.c1))
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }

                foreach (var customer in customers.Select(e => e.c2))
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_duplicate_collection_result_operator2(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? (from c1 in context.Set<Customer>()
                               .Include("Orders")
                               .OrderBy(c => c.CustomerID)
                               .Take(2)
                           from c2 in context.Set<Customer>()
                               .OrderBy(c => c.CustomerID)
                               .Skip(2)
                               .Take(2)
                           select new { c1, c2 })
                        .Take(1)
                        .ToList()
                        : (from c1 in context.Set<Customer>()
                               .Include(c => c.Orders)
                               .OrderBy(c => c.CustomerID)
                               .Take(2)
                           from c2 in context.Set<Customer>()
                               .OrderBy(c => c.CustomerID)
                               .Skip(2)
                               .Take(2)
                           select new { c1, c2 })
                        .Take(1)
                        .ToList();

                Assert.Equal(1, customers.Count);
                Assert.Equal(6, customers.SelectMany(c => c.c1.Orders).Count());
                Assert.True(customers.SelectMany(c => c.c1.Orders).All(o => o.Customer != null));
                Assert.True(customers.All(c => c.c2.Orders == null));
                Assert.Equal(8, context.ChangeTracker.Entries().Count());

                foreach (var customer in customers.Select(e => e.c1))
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }

                foreach (var customer in customers.Select(e => e.c2))
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: false,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_duplicate_reference(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? (from o1 in context.Set<Order>()
                               .Include("Customer")
                               .OrderBy(o => o.CustomerID)
                               .Take(2)
                           from o2 in context.Set<Order>()
                               .Include("Customer")
                               .OrderBy(o => o.CustomerID)
                               .Skip(2)
                               .Take(2)
                           select new { o1, o2 })
                        .ToList()
                        : (from o1 in context.Set<Order>()
                               .Include(o => o.Customer)
                               .OrderBy(o => o.CustomerID)
                               .Take(2)
                           from o2 in context.Set<Order>()
                               .Include(o => o.Customer)
                               .OrderBy(o => o.CustomerID)
                               .Skip(2)
                               .Take(2)
                           select new { o1, o2 })
                        .ToList();

                Assert.Equal(4, orders.Count);
                Assert.True(orders.All(o => o.o1.Customer != null));
                Assert.True(orders.All(o => o.o2.Customer != null));
                Assert.Equal(1, orders.Select(o => o.o1.Customer).Distinct().Count());
                Assert.Equal(1, orders.Select(o => o.o2.Customer).Distinct().Count());
                Assert.Equal(5, context.ChangeTracker.Entries().Count());

                foreach (var order in orders.Select(e => e.o1))
                {
                    CheckIsLoaded(
                        context,
                        order,
                        orderDetailsLoaded: false,
                        productLoaded: false,
                        customerLoaded: true,
                        ordersLoaded: false);
                }

                foreach (var order in orders.Select(e => e.o2))
                {
                    CheckIsLoaded(
                        context,
                        order,
                        orderDetailsLoaded: false,
                        productLoaded: false,
                        customerLoaded: true,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_duplicate_reference2(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? (from o1 in context.Set<Order>()
                               .Include("Customer")
                               .OrderBy(o => o.OrderID)
                               .Take(2)
                           from o2 in context.Set<Order>()
                               .OrderBy(o => o.OrderID)
                               .Skip(2)
                               .Take(2)
                           select new { o1, o2 })
                        .ToList()
                        : (from o1 in context.Set<Order>()
                               .Include(o => o.Customer)
                               .OrderBy(o => o.OrderID)
                               .Take(2)
                           from o2 in context.Set<Order>()
                               .OrderBy(o => o.OrderID)
                               .Skip(2)
                               .Take(2)
                           select new { o1, o2 })
                        .ToList();

                Assert.Equal(4, orders.Count);
                Assert.True(orders.All(o => o.o1.Customer != null));
                Assert.True(orders.All(o => o.o2.Customer == null));
                Assert.Equal(2, orders.Select(o => o.o1.Customer).Distinct().Count());
                Assert.Equal(6, context.ChangeTracker.Entries().Count());

                foreach (var order in orders.Select(e => e.o1))
                {
                    CheckIsLoaded(
                        context,
                        order,
                        orderDetailsLoaded: false,
                        productLoaded: false,
                        customerLoaded: true,
                        ordersLoaded: false);
                }

                foreach (var order in orders.Select(e => e.o2))
                {
                    CheckIsLoaded(
                        context,
                        order,
                        orderDetailsLoaded: false,
                        productLoaded: false,
                        customerLoaded: false,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_duplicate_reference3(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? (from o1 in context.Set<Order>()
                               .OrderBy(o => o.OrderID)
                               .Take(2)
                           from o2 in context.Set<Order>()
                               .OrderBy(o => o.OrderID)
                               .Include("Customer")
                               .Skip(2)
                               .Take(2)
                           select new { o1, o2 })
                        .ToList()
                        : (from o1 in context.Set<Order>()
                               .OrderBy(o => o.OrderID)
                               .Take(2)
                           from o2 in context.Set<Order>()
                               .OrderBy(o => o.OrderID)
                               .Include(o => o.Customer)
                               .Skip(2)
                               .Take(2)
                           select new { o1, o2 })
                        .ToList();

                Assert.Equal(4, orders.Count);
                Assert.True(orders.All(o => o.o1.Customer == null));
                Assert.True(orders.All(o => o.o2.Customer != null));
                Assert.Equal(2, orders.Select(o => o.o2.Customer).Distinct().Count());
                Assert.Equal(6, context.ChangeTracker.Entries().Count());

                foreach (var order in orders.Select(e => e.o1))
                {
                    CheckIsLoaded(
                        context,
                        order,
                        orderDetailsLoaded: false,
                        productLoaded: false,
                        customerLoaded: false,
                        ordersLoaded: false);
                }

                foreach (var order in orders.Select(e => e.o2))
                {
                    CheckIsLoaded(
                        context,
                        order,
                        orderDetailsLoaded: false,
                        productLoaded: false,
                        customerLoaded: true,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_with_client_filter(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? context.Set<Customer>()
                            .Include("Orders")
                            .Where(c => c.IsLondon)
                            .ToList()
                        : context.Set<Customer>()
                            .Include(c => c.Orders)
                            .Where(c => c.IsLondon)
                            .ToList();

                Assert.Equal(6, customers.Count);
                Assert.Equal(46, customers.SelectMany(c => c.Orders).Count());
                Assert.True(customers.SelectMany(c => c.Orders).All(o => o.Customer != null));
                Assert.Equal(13, customers.First().Orders.Count); // AROUT
                Assert.Equal(9, customers.Last().Orders.Count); // SEVES
                Assert.Equal(6 + 46, context.ChangeTracker.Entries().Count());

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_multi_level_reference_and_collection_predicate(bool useString)
        {
            using (var context = CreateContext())
            {
                var order
                    = useString
                        ? context.Set<Order>()
                            .Include("Customer.Orders")
                            .Single(o => o.OrderID == 10248)
                        : context.Set<Order>()
                            .Include(o => o.Customer.Orders)
                            .Single(o => o.OrderID == 10248);

                Assert.NotNull(order.Customer);
                Assert.True(order.Customer.Orders.All(o => o != null));

                CheckIsLoaded(
                    context,
                    order,
                    orderDetailsLoaded: false,
                    productLoaded: false,
                    customerLoaded: true,
                    ordersLoaded: true);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_multi_level_collection_and_then_include_reference_predicate(bool useString)
        {
            using (var context = CreateContext())
            {
                var order
                    = useString
                        ? context.Set<Order>()
                            .Include("OrderDetails.Product")
                            .Single(o => o.OrderID == 10248)
                        : context.Set<Order>()
                            .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                            .Single(o => o.OrderID == 10248);

                Assert.NotNull(order.OrderDetails);
                Assert.True(order.OrderDetails.Count > 0);
                Assert.True(order.OrderDetails.All(od => od.Product != null));

                CheckIsLoaded(
                    context,
                    order,
                    orderDetailsLoaded: true,
                    productLoaded: true,
                    customerLoaded: false,
                    ordersLoaded: false);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_multiple_references(bool useString)
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = useString
                        ? context.Set<OrderDetail>()
                            .Include("Order")
                            .Include("Product")
                            .ToList()
                        : context.Set<OrderDetail>()
                            .Include(o => o.Order)
                            .Include(o => o.Product)
                            .ToList();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(o => o.Order != null));
                Assert.True(orderDetails.All(o => o.Product != null));
                Assert.Equal(830, orderDetails.Select(o => o.Order).Distinct().Count());
                Assert.True(orderDetails.Select(o => o.Product).Distinct().Any());

                foreach (var orderDetail in orderDetails)
                {
                    CheckIsLoaded(
                        context,
                        orderDetail,
                        orderLoaded: true,
                        productLoaded: true,
                        customerLoaded: false,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_multiple_references_and_collection_multi_level(bool useString)
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = useString
                        ? context.Set<OrderDetail>()
                            .Include("Order.Customer.Orders")
                            .Include("Product")
                            .ToList()
                        : context.Set<OrderDetail>()
                            .Include(od => od.Order.Customer.Orders)
                            .Include(od => od.Product)
                            .ToList();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));
                Assert.True(orderDetails.All(od => od.Order.Customer.Orders != null));

                foreach (var orderDetail in orderDetails)
                {
                    CheckIsLoaded(
                        context,
                        orderDetail,
                        orderLoaded: true,
                        productLoaded: true,
                        customerLoaded: true,
                        ordersLoaded: true);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_multiple_references_and_collection_multi_level_reverse(bool useString)
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = useString
                        ? context.Set<OrderDetail>()
                            .Include("Product")
                            .Include("Order.Customer.Orders")
                            .ToList()
                        : context.Set<OrderDetail>()
                            .Include(od => od.Product)
                            .Include(od => od.Order.Customer.Orders)
                            .ToList();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));
                Assert.True(orderDetails.All(od => od.Order.Customer.Orders != null));

                foreach (var orderDetail in orderDetails)
                {
                    CheckIsLoaded(
                        context,
                        orderDetail,
                        orderLoaded: true,
                        productLoaded: true,
                        customerLoaded: true,
                        ordersLoaded: true);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_multiple_references_multi_level(bool useString)
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = useString
                        ? context.Set<OrderDetail>()
                            .Include("Order.Customer")
                            .Include("Product")
                            .ToList()
                        : context.Set<OrderDetail>()
                            .Include(o => o.Order.Customer)
                            .Include(o => o.Product)
                            .ToList();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));

                foreach (var orderDetail in orderDetails)
                {
                    CheckIsLoaded(
                        context,
                        orderDetail,
                        orderLoaded: true,
                        productLoaded: true,
                        customerLoaded: true,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_multiple_references_multi_level_reverse(bool useString)
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = useString
                        ? context.Set<OrderDetail>()
                            .Include("Product")
                            .Include("Order.Customer")
                            .ToList()
                        : context.Set<OrderDetail>()
                            .Include(o => o.Product)
                            .Include(o => o.Order.Customer)
                            .ToList();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));

                foreach (var orderDetail in orderDetails)
                {
                    CheckIsLoaded(
                        context,
                        orderDetail,
                        orderLoaded: true,
                        productLoaded: true,
                        customerLoaded: true,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_reference(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? context.Set<Order>()
                            .Include("Customer")
                            .ToList()
                        : context.Set<Order>()
                            .Include(o => o.Customer)
                            .ToList();

                Assert.Equal(830, orders.Count);
                Assert.True(orders.All(o => o.Customer != null));
                Assert.Equal(89, orders.Select(o => o.Customer).Distinct().Count());
                Assert.Equal(830 + 89, context.ChangeTracker.Entries().Count());

                foreach (var order in orders)
                {
                    CheckIsLoaded(
                        context,
                        order,
                        customerLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_reference_alias_generation(bool useString)
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = useString
                        ? context.Set<OrderDetail>()
                            .Include("Order")
                            .ToList()
                        : context.Set<OrderDetail>()
                            .Include(o => o.Order)
                            .ToList();

                Assert.True(orderDetails.Any());

                foreach (var orderDetail in orderDetails)
                {
                    CheckIsLoaded(
                        context,
                        orderDetail,
                        orderLoaded: true,
                        productLoaded: false,
                        customerLoaded: false,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_reference_and_collection(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? context.Set<Order>()
                            .Include("Customer")
                            .Include("OrderDetails")
                            .ToList()
                        : context.Set<Order>()
                            .Include(o => o.Customer)
                            .Include(o => o.OrderDetails)
                            .ToList();

                Assert.Equal(830, orders.Count);

                foreach (var order in orders)
                {
                    CheckIsLoaded(
                        context,
                        order,
                        customerLoaded: true,
                        orderDetailsLoaded: true,
                        productLoaded: false,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_force_alias_uniquefication(bool useString)
        {
            using (var context = CreateContext())
            {
                var result
                    = useString
                        ? (from o in context.Set<Order>().Include("OrderDetails")
                           where o.CustomerID == "ALFKI"
                           select o)
                        .ToList()
                        : (from o in context.Set<Order>().Include(o => o.OrderDetails)
                           where o.CustomerID == "ALFKI"
                           select o)
                        .ToList();

                Assert.Equal(6, result.Count);
                Assert.True(result.SelectMany(r => r.OrderDetails).All(od => od.Order != null));

                foreach (var order in result)
                {
                    CheckIsLoaded(
                        context,
                        order,
                        customerLoaded: false,
                        orderDetailsLoaded: true,
                        productLoaded: false,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_reference_as_no_tracking(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? context.Set<Order>()
                            .Include("Customer")
                            .AsNoTracking()
                            .ToList()
                        : context.Set<Order>()
                            .Include(o => o.Customer)
                            .AsNoTracking()
                            .ToList();

                Assert.Equal(830, orders.Count);
                Assert.True(orders.All(o => o.Customer != null));
                Assert.Equal(0, context.ChangeTracker.Entries().Count());

                foreach (var order in orders)
                {
                    CheckIsLoaded(
                        context,
                        order,
                        customerLoaded: false,
                        orderDetailsLoaded: false,
                        productLoaded: false,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_reference_dependent_already_tracked(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders1
                    = context.Set<Order>()
                        .Where(o => o.CustomerID == "ALFKI")
                        .ToList();

                Assert.Equal(6, context.ChangeTracker.Entries().Count());

                var orders2
                    = useString
                        ? context.Set<Order>()
                            .Include("Customer")
                            .ToList()
                        : context.Set<Order>()
                            .Include(o => o.Customer)
                            .ToList();

                Assert.True(orders1.All(o1 => orders2.Contains(o1, ReferenceEqualityComparer.Instance)));
                Assert.True(orders2.All(o => o.Customer != null));
                Assert.Equal(830 + 89, context.ChangeTracker.Entries().Count());

                foreach (var order in orders2)
                {
                    CheckIsLoaded(
                        context,
                        order,
                        customerLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_reference_single_or_default_when_no_result(bool useString)
        {
            using (var context = CreateContext())
            {
                var order
                    = useString
                        ? context.Set<Order>()
                            .Include("Customer")
                            .SingleOrDefault(o => o.OrderID == -1)
                        : context.Set<Order>()
                            .Include(o => o.Customer)
                            .SingleOrDefault(o => o.OrderID == -1);

                Assert.Null(order);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_reference_when_projection(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? context.Set<Order>()
                            .Include("Customer")
                            .Select(o => o.CustomerID)
                            .ToList()
                        : context.Set<Order>()
                            .Include(o => o.Customer)
                            .Select(o => o.CustomerID)
                            .ToList();

                Assert.Equal(830, orders.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_reference_when_entity_in_projection(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? context.Set<Order>()
                            .Include("Customer")
                            .Select(o => new { o, o.CustomerID })
                            .ToList()
                        : context.Set<Order>()
                            .Include(o => o.Customer)
                            .Select(o => new { o, o.CustomerID })
                            .ToList();

                Assert.Equal(830, orders.Count);
                Assert.Equal(919, context.ChangeTracker.Entries().Count());

                foreach (var order in orders.Select(e => e.o))
                {
                    CheckIsLoaded(
                        context,
                        order,
                        customerLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_reference_with_filter(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? context.Set<Order>()
                            .Include("Customer")
                            .Where(o => o.CustomerID == "ALFKI")
                            .ToList()
                        : context.Set<Order>()
                            .Include(o => o.Customer)
                            .Where(o => o.CustomerID == "ALFKI")
                            .ToList();

                Assert.Equal(6, orders.Count);
                Assert.True(orders.All(o => o.Customer != null));
                Assert.Equal(1, orders.Select(o => o.Customer).Distinct().Count());
                Assert.Equal(6 + 1, context.ChangeTracker.Entries().Count());

                foreach (var order in orders)
                {
                    CheckIsLoaded(
                        context,
                        order,
                        customerLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_reference_with_filter_reordered(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? context.Set<Order>()
                            .Where(o => o.CustomerID == "ALFKI")
                            .Include("Customer")
                            .ToList()
                        : context.Set<Order>()
                            .Where(o => o.CustomerID == "ALFKI")
                            .Include(o => o.Customer)
                            .ToList();

                Assert.Equal(6, orders.Count);
                Assert.True(orders.All(o => o.Customer != null));
                Assert.Equal(1, orders.Select(o => o.Customer).Distinct().Count());
                Assert.Equal(6 + 1, context.ChangeTracker.Entries().Count());

                foreach (var order in orders)
                {
                    CheckIsLoaded(
                        context,
                        order,
                        customerLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_references_and_collection_multi_level(bool useString)
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = useString
                        ? context.Set<OrderDetail>()
                            .Include("Order.Customer.Orders")
                            .ToList()
                        : context.Set<OrderDetail>()
                            .Include(o => o.Order.Customer.Orders)
                            .ToList();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));
                Assert.True(orderDetails.All(od => od.Order.Customer.Orders != null));

                foreach (var orderDetail in orderDetails)
                {
                    CheckIsLoaded(
                        context,
                        orderDetail,
                        orderLoaded: true,
                        productLoaded: false,
                        customerLoaded: true,
                        ordersLoaded: true);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_then_include_collection(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? context.Set<Customer>()
                            .Include("Orders.OrderDetails")
                            .ToList()
                        : context.Set<Customer>()
                            .Include(c => c.Orders).ThenInclude(o => o.OrderDetails)
                            .ToList();

                Assert.Equal(91, customers.Count);
                Assert.True(customers.All(c => c.Orders != null));
                Assert.True(customers.All(c => c.Orders.All(o => o.OrderDetails != null)));

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: true,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_then_include_collection_then_include_reference(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? context.Set<Customer>()
                            .Include("Orders.OrderDetails.Product")
                            .ToList()
                        : context.Set<Customer>()
                            .Include(c => c.Orders).ThenInclude(o => o.OrderDetails).ThenInclude(od => od.Product)
                            .ToList();

                Assert.Equal(91, customers.Count);
                Assert.True(customers.All(c => c.Orders != null));
                Assert.True(customers.All(c => c.Orders.All(o => o.OrderDetails != null)));

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: true,
                        productLoaded: true);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_then_include_collection_predicate(bool useString)
        {
            using (var context = CreateContext())
            {
                var customer
                    = useString
                        ? context.Set<Customer>()
                            .Include("Orders.OrderDetails")
                            .SingleOrDefault(c => c.CustomerID == "ALFKI")
                        : context.Set<Customer>()
                            .Include(c => c.Orders).ThenInclude(o => o.OrderDetails)
                            .SingleOrDefault(c => c.CustomerID == "ALFKI");

                Assert.NotNull(customer);
                Assert.Equal(6, customer.Orders.Count);
                Assert.True(customer.Orders.SelectMany(o => o.OrderDetails).Count() >= 6);

                CheckIsLoaded(
                    context,
                    customer,
                    ordersLoaded: true,
                    orderDetailsLoaded: true,
                    productLoaded: false);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_references_and_collection_multi_level_predicate(bool useString)
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = useString
                        ? context.Set<OrderDetail>()
                            .Include("Order.Customer.Orders")
                            .Where(od => od.OrderID == 10248)
                            .ToList()
                        : context.Set<OrderDetail>()
                            .Include(od => od.Order.Customer.Orders)
                            .Where(od => od.OrderID == 10248)
                            .ToList();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));
                Assert.True(orderDetails.All(od => od.Order.Customer.Orders != null));

                foreach (var orderDetail in orderDetails)
                {
                    CheckIsLoaded(
                        context,
                        orderDetail,
                        orderLoaded: true,
                        productLoaded: false,
                        customerLoaded: true,
                        ordersLoaded: true);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_references_multi_level(bool useString)
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = useString
                        ? context.Set<OrderDetail>()
                            .Include("Order.Customer")
                            .ToList()
                        : context.Set<OrderDetail>()
                            .Include(o => o.Order.Customer)
                            .ToList();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));

                foreach (var orderDetail in orderDetails)
                {
                    CheckIsLoaded(
                        context,
                        orderDetail,
                        orderLoaded: true,
                        productLoaded: false,
                        customerLoaded: true,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_multi_level_reference_then_include_collection_predicate(bool useString)
        {
            using (var context = CreateContext())
            {
                var order
                    = useString
                        ? context.Set<Order>()
                            .Include("Customer.Orders")
                            .Single(o => o.OrderID == 10248)
                        : context.Set<Order>()
                            .Include(o => o.Customer).ThenInclude(c => c.Orders)
                            .Single(o => o.OrderID == 10248);

                Assert.NotNull(order.Customer);
                Assert.True(order.Customer.Orders.All(o => o != null));

                CheckIsLoaded(
                    context,
                    order,
                    customerLoaded: true,
                    orderDetailsLoaded: false,
                    productLoaded: false,
                    ordersLoaded: true);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_multiple_references_then_include_collection_multi_level(bool useString)
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = useString
                        ? context.Set<OrderDetail>()
                            .Include("Order.Customer.Orders")
                            .Include("Product")
                            .ToList()
                        : context.Set<OrderDetail>()
                            .Include(od => od.Order).ThenInclude(o => o.Customer).ThenInclude(c => c.Orders)
                            .Include(od => od.Product)
                            .ToList();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));
                Assert.True(orderDetails.All(od => od.Order.Customer.Orders != null));

                foreach (var orderDetail in orderDetails)
                {
                    CheckIsLoaded(
                        context,
                        orderDetail,
                        orderLoaded: true,
                        productLoaded: true,
                        customerLoaded: true,
                        ordersLoaded: true);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_multiple_references_then_include_collection_multi_level_reverse(bool useString)
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = useString
                        ? context.Set<OrderDetail>()
                            .Include(nameof(OrderDetail.Product))
                            .Include(nameof(OrderDetail.Order) + "." + nameof(Order.Customer) + "." + nameof(Customer.Orders))
                            .ToList()
                        : context.Set<OrderDetail>()
                            .Include(od => od.Product)
                            .Include(od => od.Order).ThenInclude(o => o.Customer).ThenInclude(c => c.Orders)
                            .ToList();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));
                Assert.True(orderDetails.All(od => od.Order.Customer.Orders != null));

                foreach (var orderDetail in orderDetails)
                {
                    CheckIsLoaded(
                        context,
                        orderDetail,
                        orderLoaded: true,
                        productLoaded: true,
                        customerLoaded: true,
                        ordersLoaded: true);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_multiple_references_then_include_multi_level(bool useString)
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = useString
                        ? context.Set<OrderDetail>()
                            .Include("Order.Customer")
                            .Include("Product")
                            .ToList()
                        : context.Set<OrderDetail>()
                            .Include(od => od.Order).ThenInclude(o => o.Customer)
                            .Include(od => od.Product)
                            .ToList();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));

                foreach (var orderDetail in orderDetails)
                {
                    CheckIsLoaded(
                        context,
                        orderDetail,
                        orderLoaded: true,
                        productLoaded: true,
                        customerLoaded: true,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_multiple_references_then_include_multi_level_reverse(bool useString)
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = useString
                        ? context.Set<OrderDetail>()
                            .Include("Product")
                            .Include("Order.Customer")
                            .ToList()
                        : context.Set<OrderDetail>()
                            .Include(od => od.Product)
                            .Include(od => od.Order).ThenInclude(o => o.Customer)
                            .ToList();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));

                foreach (var orderDetail in orderDetails)
                {
                    CheckIsLoaded(
                        context,
                        orderDetail,
                        orderLoaded: true,
                        productLoaded: true,
                        customerLoaded: true,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_references_then_include_collection_multi_level(bool useString)
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = useString
                        ? context.Set<OrderDetail>()
                            .Include("Order.Customer.Orders")
                            .ToList()
                        : context.Set<OrderDetail>()
                            .Include(od => od.Order).ThenInclude(o => o.Customer).ThenInclude(c => c.Orders)
                            .ToList();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));
                Assert.True(orderDetails.All(od => od.Order.Customer.Orders != null));

                foreach (var orderDetail in orderDetails)
                {
                    CheckIsLoaded(
                        context,
                        orderDetail,
                        orderLoaded: true,
                        productLoaded: false,
                        customerLoaded: true,
                        ordersLoaded: true);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_references_then_include_collection_multi_level_predicate(bool useString)
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = useString
                        ? context.Set<OrderDetail>()
                            .Include("Order.Customer.Orders")
                            .Where(od => od.OrderID == 10248)
                            .ToList()
                        : context.Set<OrderDetail>()
                            .Include(od => od.Order).ThenInclude(o => o.Customer).ThenInclude(c => c.Orders)
                            .Where(od => od.OrderID == 10248)
                            .ToList();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));
                Assert.True(orderDetails.All(od => od.Order.Customer.Orders != null));

                foreach (var orderDetail in orderDetails)
                {
                    CheckIsLoaded(
                        context,
                        orderDetail,
                        orderLoaded: true,
                        productLoaded: false,
                        customerLoaded: true,
                        ordersLoaded: true);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_references_then_include_multi_level(bool useString)
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = useString
                        ? context.Set<OrderDetail>()
                            .Include("Order.Customer")
                            .ToList()
                        : context.Set<OrderDetail>()
                            .Include(od => od.Order).ThenInclude(o => o.Customer)
                            .ToList();

                Assert.True(orderDetails.Count > 0);
                Assert.True(orderDetails.All(od => od.Order.Customer != null));

                foreach (var orderDetail in orderDetails)
                {
                    CheckIsLoaded(
                        context,
                        orderDetail,
                        orderLoaded: true,
                        productLoaded: false,
                        customerLoaded: true,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_with_complex_projection(bool useString)
        {
            using (var context = CreateContext())
            {
                var query = useString
                    ? from o in context.Orders.Include("Customer")
                      select new
                      {
                          CustomerId = new
                          {
                              Id = o.Customer.CustomerID
                          }
                      }
                    : from o in context.Orders.Include(o => o.Customer)
                      select new
                      {
                          CustomerId = new
                          {
                              Id = o.Customer.CustomerID
                          }
                      };

                var results = query.ToList();

                Assert.Equal(830, results.Count);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_with_complex_projection_does_not_change_ordering_of_projection(bool useString)
        {
            using (var context = CreateContext())
            {
                var q = useString
                    ? from c in context.Customers.Include(nameof(Customer.Orders))
                          .Where(c => c.ContactTitle == "Owner")
                          .OrderBy(c => c.CustomerID)
                      select new
                      {
                          Id = c.CustomerID,
                          TotalOrders = c.Orders.Count
                      }
                    : from c in context.Customers.Include(c => c.Orders)
                          .Where(c => c.ContactTitle == "Owner")
                          .OrderBy(c => c.CustomerID)
                      select new
                      {
                          Id = c.CustomerID,
                          TotalOrders = c.Orders.Count
                      };

                var result = q.Where(e => e.TotalOrders > 2).ToList();

                Assert.Equal(15, result.Count);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_with_take(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? context.Set<Customer>()
                            .OrderByDescending(c => c.City)
                            .Include("Orders")
                            .Take(10)
                            .ToList()
                        : context.Set<Customer>()
                            .OrderByDescending(c => c.City)
                            .Include(c => c.Orders)
                            .Take(10)
                            .ToList();

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_with_skip(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? context.Customers
                            .Include("Orders")
                            .OrderBy(c => c.ContactName)
                            .Skip(80)
                            .ToList()
                        : context.Customers
                            .Include(c => c.Orders)
                            .OrderBy(c => c.ContactName)
                            .Skip(80)
                            .ToList();

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_with_conditional_order_by(bool useString)
        {
            int customersWithPrefix;
            using (var context = CreateContext())
            {
                customersWithPrefix = context.Customers.Count(c => c.CustomerID.StartsWith("S"));
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? context.Customers
                            .OrderBy(c => c.CustomerID.StartsWith("S") ? 1 : 2)
                            .Include(c => c.Orders)
                            .ToList()
                        : context.Customers
                            .OrderBy(c => c.CustomerID.StartsWith("S") ? 1 : 2)
                            .Include("Orders")
                            .ToList();

                Assert.True(customers.Take(customersWithPrefix).All(c => c.CustomerID.StartsWith("S")));

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_specified_on_non_entity_not_supported(bool useString)
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.IncludeNotSpecifiedDirectlyOnEntityType(@"Include(""Item1.Orders"")", "Item1"),
                    Assert.Throws<InvalidOperationException>(
                        () => useString
                            ? context.Customers
                                .Select(c => new Tuple<Customer, int>(c, 5))
                                .Include(t => t.Item1.Orders)
                                .ToList()
                            : context.Customers
                                .Select(c => new Tuple<Customer, int>(c, 5))
                                .Include("Item1.Orders")
                                .ToList()).Message);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_GroupBy_Select(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? context.Orders
                            .Where(o => o.OrderID == 10248)
                            .Include("OrderDetails")
                            .GroupBy(e => e.OrderID)
                            .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                            .ToList()
                        : context.Orders
                            .Where(o => o.OrderID == 10248)
                            .Include(o => o.OrderDetails)
                            .GroupBy(e => e.OrderID)
                            .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                            .ToList();

                foreach (var order in orders)
                {
                    CheckIsLoaded(
                        context,
                        order,
                        orderDetailsLoaded: true,
                        productLoaded: false,
                        customerLoaded: false,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_reference_GroupBy_Select(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? context.Orders
                            .Where(o => o.OrderID == 10248)
                            .Include("Customer")
                            .GroupBy(e => e.OrderID)
                            .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                            .ToList()
                        : context.Orders
                            .Where(o => o.OrderID == 10248)
                            .Include(o => o.Customer)
                            .GroupBy(e => e.OrderID)
                            .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                            .ToList();

                foreach (var order in orders)
                {
                    CheckIsLoaded(
                        context,
                        order,
                        orderDetailsLoaded: false,
                        productLoaded: false,
                        customerLoaded: true,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_Join_GroupBy_Select(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? context.Orders
                            .Where(o => o.OrderID == 10248)
                            .Include("OrderDetails")
                            .Join(context.OrderDetails,
                                o => o.OrderID,
                                od => od.OrderID,
                                (o, od) => o)
                            .GroupBy(e => e.OrderID)
                            .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                            .ToList()
                        : context.Orders
                            .Where(o => o.OrderID == 10248)
                            .Include(o => o.OrderDetails)
                            .Join(context.OrderDetails,
                                o => o.OrderID,
                                od => od.OrderID,
                                (o, od) => o)
                            .GroupBy(e => e.OrderID)
                            .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                            .ToList();

                foreach (var order in orders)
                {
                    CheckIsLoaded(
                        context,
                        order,
                        orderDetailsLoaded: true,
                        productLoaded: false,
                        customerLoaded: false,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_reference_Join_GroupBy_Select(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? context.Orders
                            .Where(o => o.OrderID == 10248)
                            .Include("Customer")
                            .Join(context.OrderDetails,
                                o => o.OrderID,
                                od => od.OrderID,
                                (o, od) => o)
                            .GroupBy(e => e.OrderID)
                            .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                            .ToList()
                        : context.Orders
                            .Where(o => o.OrderID == 10248)
                            .Include(o => o.Customer)
                            .Join(context.OrderDetails,
                                o => o.OrderID,
                                od => od.OrderID,
                                (o, od) => o)
                            .GroupBy(e => e.OrderID)
                            .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                            .ToList();

                foreach (var order in orders)
                {
                    CheckIsLoaded(
                        context,
                        order,
                        orderDetailsLoaded: false,
                        productLoaded: false,
                        customerLoaded: true,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Join_Include_collection_GroupBy_Select(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? context.OrderDetails
                            .Where(od => od.OrderID == 10248)
                            .Join(
                                context.Orders.Include("OrderDetails"),
                                od => od.OrderID,
                                o => o.OrderID,
                                (od, o) => o)
                            .GroupBy(e => e.OrderID)
                            .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                            .ToList()
                        : context.OrderDetails
                            .Where(od => od.OrderID == 10248)
                            .Join(
                                context.Orders.Include(o => o.OrderDetails),
                                od => od.OrderID,
                                o => o.OrderID,
                                (od, o) => o)
                            .GroupBy(e => e.OrderID)
                            .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                            .ToList();

                foreach (var order in orders)
                {
                    CheckIsLoaded(
                        context,
                        order,
                        orderDetailsLoaded: true,
                        productLoaded: false,
                        customerLoaded: false,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Join_Include_reference_GroupBy_Select(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? context.OrderDetails
                            .Join(
                                context.Orders.Include("Customer"),
                                od => od.OrderID,
                                o => o.OrderID,
                                (od, o) => o)
                            .GroupBy(e => e.OrderID)
                            .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                            .ToList()
                        : context.OrderDetails
                            .Join(
                                context.Orders.Include(o => o.Customer),
                                od => od.OrderID,
                                o => o.OrderID,
                                (od, o) => o)
                            .GroupBy(e => e.OrderID)
                            .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                            .ToList();

                foreach (var order in orders)
                {
                    CheckIsLoaded(
                        context,
                        order,
                        orderDetailsLoaded: false,
                        productLoaded: false,
                        customerLoaded: true,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_GroupJoin_GroupBy_Select(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? context.Orders
                            .Where(o => o.OrderID == 10248)
                            .Include("OrderDetails")
                            .GroupJoin(context.OrderDetails,
                                o => o.OrderID,
                                od => od.OrderID,
                                (o, od) => o)
                            .GroupBy(e => e.OrderID)
                            .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                            .ToList()
                        : context.Orders
                            .Where(o => o.OrderID == 10248)
                            .Include(o => o.OrderDetails)
                            .GroupJoin(context.OrderDetails,
                                o => o.OrderID,
                                od => od.OrderID,
                                (o, od) => o)
                            .GroupBy(e => e.OrderID)
                            .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                            .ToList();

                foreach (var order in orders)
                {
                    CheckIsLoaded(
                        context,
                        order,
                        orderDetailsLoaded: true,
                        productLoaded: false,
                        customerLoaded: false,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_reference_GroupJoin_GroupBy_Select(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? context.Orders
                            .Where(o => o.OrderID == 10248)
                            .Include("Customer")
                            .GroupJoin(context.OrderDetails,
                                o => o.OrderID,
                                od => od.OrderID,
                                (o, od) => o)
                            .GroupBy(e => e.OrderID)
                            .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                            .ToList()
                        : context.Orders
                            .Where(o => o.OrderID == 10248)
                            .Include(o => o.Customer)
                            .GroupJoin(context.OrderDetails,
                                o => o.OrderID,
                                od => od.OrderID,
                                (o, od) => o)
                            .GroupBy(e => e.OrderID)
                            .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                            .ToList();

                foreach (var order in orders)
                {
                    CheckIsLoaded(
                        context,
                        order,
                        orderDetailsLoaded: false,
                        productLoaded: false,
                        customerLoaded: true,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void GroupJoin_Include_collection_GroupBy_Select(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? context.OrderDetails
                            .Where(od => od.OrderID == 10248)
                            .GroupJoin(
                                context.Orders.Include("OrderDetails"),
                                od => od.OrderID,
                                o => o.OrderID,
                                (od, o) => o.OrderBy(o1 => o1.OrderID).FirstOrDefault())
                            .GroupBy(e => e.OrderID)
                            .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                            .ToList()
                        : context.OrderDetails
                            .Where(od => od.OrderID == 10248)
                            .GroupJoin(
                                context.Orders.Include(o => o.OrderDetails),
                                od => od.OrderID,
                                o => o.OrderID,
                                (od, o) => o.OrderBy(o1 => o1.OrderID).FirstOrDefault())
                            .GroupBy(e => e.OrderID)
                            .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                            .ToList();

                foreach (var order in orders)
                {
                    CheckIsLoaded(
                        context,
                        order,
                        orderDetailsLoaded: true,
                        productLoaded: false,
                        customerLoaded: false,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void GroupJoin_Include_reference_GroupBy_Select(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? context.OrderDetails
                            .Where(od => od.OrderID == 10248)
                            .GroupJoin(
                                context.Orders.Include("Customer"),
                                od => od.OrderID,
                                o => o.OrderID,
                                (od, o) => o.OrderBy(o1 => o1.OrderID).FirstOrDefault())
                            .GroupBy(e => e.OrderID)
                            .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                            .ToList()
                        : context.OrderDetails
                            .Where(od => od.OrderID == 10248)
                            .GroupJoin(
                                context.Orders.Include(o => o.Customer),
                                od => od.OrderID,
                                o => o.OrderID,
                                (od, o) => o.OrderBy(o1 => o1.OrderID).FirstOrDefault())
                            .GroupBy(e => e.OrderID)
                            .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                            .ToList();

                foreach (var order in orders)
                {
                    CheckIsLoaded(
                        context,
                        order,
                        orderDetailsLoaded: false,
                        productLoaded: false,
                        customerLoaded: true,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_SelectMany_GroupBy_Select(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? (from o in context.Orders.Include("OrderDetails").Where(o => o.OrderID == 10248)
                           from od in context.OrderDetails
                           select o)
                        .GroupBy(e => e.OrderID)
                        .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                        .ToList()
                        : (from o in context.Orders.Include(o => o.OrderDetails).Where(o => o.OrderID == 10248)
                           from od in context.OrderDetails
                           select o)
                        .GroupBy(e => e.OrderID)
                        .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                        .ToList();

                foreach (var order in orders)
                {
                    CheckIsLoaded(
                        context,
                        order,
                        orderDetailsLoaded: true,
                        productLoaded: false,
                        customerLoaded: false,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_reference_SelectMany_GroupBy_Select(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? (from o in context.Orders.Include("Customer").Where(o => o.OrderID == 10248)
                           from od in context.OrderDetails
                           select o)
                        .GroupBy(e => e.OrderID)
                        .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                        .ToList()
                        : (from o in context.Orders.Include(o => o.Customer).Where(o => o.OrderID == 10248)
                           from od in context.OrderDetails
                           select o)
                        .GroupBy(e => e.OrderID)
                        .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                        .ToList();

                foreach (var order in orders)
                {
                    CheckIsLoaded(
                        context,
                        order,
                        orderDetailsLoaded: false,
                        productLoaded: false,
                        customerLoaded: true,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void SelectMany_Include_collection_GroupBy_Select(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? (from od in context.OrderDetails.Where(od => od.OrderID == 10248)
                           from o in context.Orders.Include("OrderDetails")
                           select o)
                        .GroupBy(e => e.OrderID)
                        .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                        .ToList()
                        : (from od in context.OrderDetails.Where(od => od.OrderID == 10248)
                           from o in context.Orders.Include(o => o.OrderDetails)
                           select o)
                        .GroupBy(e => e.OrderID)
                        .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                        .ToList();

                foreach (var order in orders)
                {
                    CheckIsLoaded(
                        context,
                        order,
                        orderDetailsLoaded: true,
                        productLoaded: false,
                        customerLoaded: false,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void SelectMany_Include_reference_GroupBy_Select(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? (from od in context.OrderDetails.Where(od => od.OrderID == 10248)
                           from o in context.Orders.Include("Customer")
                           select o)
                        .GroupBy(e => e.OrderID)
                        .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                        .ToList()
                        : (from od in context.OrderDetails.Where(od => od.OrderID == 10248)
                           from o in context.Orders.Include(o => o.Customer)
                           select o)
                        .GroupBy(e => e.OrderID)
                        .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault())
                        .ToList();

                foreach (var order in orders)
                {
                    CheckIsLoaded(
                        context,
                        order,
                        orderDetailsLoaded: false,
                        productLoaded: false,
                        customerLoaded: true,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_reference_distinct_is_server_evaluated(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? context.Orders
                            .Where(o => o.OrderID < 10250)
                            .Include("Customer")
                            .Distinct()
                            .ToList()
                        : context.Orders
                            .Where(o => o.OrderID < 10250)
                            .Include(o => o.Customer)
                            .Distinct()
                            .ToList();

                foreach (var order in orders)
                {
                    CheckIsLoaded(
                        context,
                        order,
                        orderDetailsLoaded: false,
                        productLoaded: false,
                        customerLoaded: true,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_distinct_is_server_evaluated(bool useString)
        {
            using (var context = CreateContext())
            {
                var customers
                    = useString
                        ? context.Customers
                            .Where(c => c.CustomerID.StartsWith("A"))
                            .Include("Orders")
                            .Distinct()
                            .ToList()
                        : context.Customers
                            .Where(c => c.CustomerID.StartsWith("A"))
                            .Include(o => o.Orders)
                            .Distinct()
                            .ToList();

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_OrderBy_object(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = useString
                        ? context.Orders
                            .Where(o => o.OrderID < 10250)
                            .Include("OrderDetails")
                            .OrderBy<Order, object>(c => c.OrderID)
                            .ToList()
                        : context.Orders
                            .Where(o => o.OrderID < 10250)
                            .Include(o => o.OrderDetails)
                            .OrderBy<Order, object>(c => c.OrderID)
                            .ToList();

                foreach (var order in orders)
                {
                    CheckIsLoaded(
                        context,
                        order,
                        orderDetailsLoaded: true,
                        productLoaded: false,
                        customerLoaded: false,
                        ordersLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_OrderBy_empty_list_contains(bool useString)
        {
            using (var context = CreateContext())
            {
                var list = new List<string>();
                var customers
                    = useString
                        ? context.Customers
                            .Include("Orders")
                            .Where(c => c.CustomerID.StartsWith("A"))
                            .OrderBy(c => list.Contains(c.CustomerID))
                            .Skip(1)
                            .ToList()
                        : context.Customers
                            .Include(c => c.Orders)
                            .Where(c => c.CustomerID.StartsWith("A"))
                            .OrderBy(c => list.Contains(c.CustomerID))
                            .Skip(1)
                            .ToList();

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_OrderBy_empty_list_does_not_contains(bool useString)
        {
            using (var context = CreateContext())
            {
                var list = new List<string>();
                var customers
                    = useString
                        ? context.Customers
                            .Include("Orders")
                            .Where(c => c.CustomerID.StartsWith("A"))
                            .OrderBy(c => !list.Contains(c.CustomerID))
                            .Skip(1)
                            .ToList()
                        : context.Customers
                            .Include(c => c.Orders)
                            .Where(c => c.CustomerID.StartsWith("A"))
                            .OrderBy(c => !list.Contains(c.CustomerID))
                            .Skip(1)
                            .ToList();

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_OrderBy_list_contains(bool useString)
        {
            using (var context = CreateContext())
            {
                var list = new List<string> { "ALFKI" };
                var customers
                    = useString
                        ? context.Customers
                            .Include("Orders")
                            .Where(c => c.CustomerID.StartsWith("A"))
                            .OrderBy(c => list.Contains(c.CustomerID))
                            .Skip(1)
                            .ToList()
                        : context.Customers
                            .Include(c => c.Orders)
                            .Where(c => c.CustomerID.StartsWith("A"))
                            .OrderBy(c => list.Contains(c.CustomerID))
                            .Skip(1)
                            .ToList();

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Include_collection_OrderBy_list_does_not_contains(bool useString)
        {
            using (var context = CreateContext())
            {
                var list = new List<string> { "ALFKI" };
                var customers
                    = useString
                        ? context.Customers
                            .Include("Orders")
                            .Where(c => c.CustomerID.StartsWith("A"))
                            .OrderBy(c => !list.Contains(c.CustomerID))
                            .Skip(1)
                            .ToList()
                        : context.Customers
                            .Include(c => c.Orders)
                            .Where(c => c.CustomerID.StartsWith("A"))
                            .OrderBy(c => !list.Contains(c.CustomerID))
                            .Skip(1)
                            .ToList();

                foreach (var customer in customers)
                {
                    CheckIsLoaded(
                        context,
                        customer,
                        ordersLoaded: true,
                        orderDetailsLoaded: false,
                        productLoaded: false);
                }
            }
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task Include_empty_collection_sets_IsLoaded(bool useString, bool async)
        {
            using (var context = CreateContext())
            {
                var customers = useString
                    ? context.Customers.Include(nameof(Customer.Orders))
                    : context.Customers.Include(e => e.Orders);

                var customer = async
                    ? await customers.SingleAsync(e => e.CustomerID == "FISSA")
                    : customers.Single(e => e.CustomerID == "FISSA");

                Assert.Empty(customer.Orders);
                Assert.True(context.Entry(customer).Collection(e => e.Orders).IsLoaded);
            }
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task Include_empty_reference_sets_IsLoaded(bool useString, bool async)
        {
            using (var context = CreateContext())
            {
                var employees = useString
                    ? context.Employees.Include(nameof(Employee.Manager))
                    : context.Employees.Include(e => e.Manager);

                var employee = async
                    ? await employees.FirstAsync(e => e.Manager == null)
                    : employees.First(e => e.Manager == null);

                Assert.Null(employee.Manager);
                Assert.True(context.Entry(employee).Reference(e => e.Manager).IsLoaded);
            }
        }

        private static void CheckIsLoaded(
            NorthwindContext context,
            Customer customer,
            bool ordersLoaded,
            bool orderDetailsLoaded,
            bool productLoaded)
        {
            context.ChangeTracker.AutoDetectChangesEnabled = false;

            Assert.Equal(ordersLoaded, context.Entry(customer).Collection(e => e.Orders).IsLoaded);
            if (customer.Orders != null)
            {
                foreach (var order in customer.Orders)
                {
                    Assert.Equal(ordersLoaded, context.Entry(order).Reference(e => e.Customer).IsLoaded);

                    Assert.Equal(orderDetailsLoaded, context.Entry(order).Collection(e => e.OrderDetails).IsLoaded);
                    if (order.OrderDetails != null)
                    {
                        foreach (var orderDetail in order.OrderDetails)
                        {
                            Assert.Equal(orderDetailsLoaded, context.Entry(orderDetail).Reference(e => e.Order).IsLoaded);

                            Assert.Equal(productLoaded, context.Entry(orderDetail).Reference(e => e.Product).IsLoaded);
                            if (orderDetail.Product != null)
                            {
                                Assert.False(context.Entry(orderDetail.Product).Collection(e => e.OrderDetails).IsLoaded);
                            }
                        }
                    }
                }
            }
        }

        private static void CheckIsLoaded(
            NorthwindContext context,
            Product product,
            bool orderDetailsLoaded,
            bool orderLoaded)
        {
            context.ChangeTracker.AutoDetectChangesEnabled = false;

            Assert.Equal(orderDetailsLoaded, context.Entry(product).Collection(e => e.OrderDetails).IsLoaded);

            if (product.OrderDetails != null)
            {
                foreach (var orderDetail in product.OrderDetails)
                {
                    Assert.Equal(orderDetailsLoaded, context.Entry(orderDetail).Reference(e => e.Product).IsLoaded);

                    Assert.Equal(orderLoaded, context.Entry(orderDetail).Reference(e => e.Order).IsLoaded);
                    if (orderDetail.Order != null)
                    {
                        Assert.False(context.Entry(orderDetail.Order).Collection(e => e.OrderDetails).IsLoaded);
                    }
                }
            }
        }

        private static void CheckIsLoaded(
            NorthwindContext context,
            Order order,
            bool orderDetailsLoaded,
            bool productLoaded,
            bool customerLoaded,
            bool ordersLoaded)
        {
            context.ChangeTracker.AutoDetectChangesEnabled = false;

            Assert.Equal(orderDetailsLoaded, context.Entry(order).Collection(e => e.OrderDetails).IsLoaded);
            if (order.OrderDetails != null)
            {
                foreach (var orderDetail in order.OrderDetails)
                {
                    Assert.Equal(orderDetailsLoaded, context.Entry(orderDetail).Reference(e => e.Order).IsLoaded);

                    Assert.Equal(productLoaded, context.Entry(orderDetail).Reference(e => e.Product).IsLoaded);
                    if (orderDetail.Product != null)
                    {
                        Assert.False(context.Entry(orderDetail.Product).Collection(e => e.OrderDetails).IsLoaded);
                    }
                }
            }

            Assert.Equal(customerLoaded, context.Entry(order).Reference(e => e.Customer).IsLoaded);
            if (order.Customer != null)
            {
                Assert.Equal(ordersLoaded, context.Entry(order.Customer).Collection(e => e.Orders).IsLoaded);
                if (ordersLoaded
                    && order.Customer.Orders != null)
                {
                    foreach (var backOrder in order.Customer.Orders)
                    {
                        Assert.Equal(ordersLoaded, context.Entry(backOrder).Reference(e => e.Customer).IsLoaded);
                    }
                }
            }
        }

        private static void CheckIsLoaded(
            NorthwindContext context,
            OrderDetail orderDetail,
            bool orderLoaded,
            bool productLoaded,
            bool customerLoaded,
            bool ordersLoaded)
        {
            context.ChangeTracker.AutoDetectChangesEnabled = false;

            Assert.Equal(orderLoaded, context.Entry(orderDetail).Reference(e => e.Order).IsLoaded);
            if (orderDetail.Order != null)
            {
                Assert.False(context.Entry(orderDetail.Order).Collection(e => e.OrderDetails).IsLoaded);

                Assert.Equal(customerLoaded, context.Entry(orderDetail.Order).Reference(e => e.Customer).IsLoaded);
                if (orderDetail.Order.Customer != null)
                {
                    Assert.Equal(ordersLoaded, context.Entry(orderDetail.Order.Customer).Collection(e => e.Orders).IsLoaded);
                    if (ordersLoaded
                        && orderDetail.Order.Customer.Orders != null)
                    {
                        foreach (var backOrder in orderDetail.Order.Customer.Orders)
                        {
                            Assert.Equal(ordersLoaded, context.Entry(backOrder).Reference(e => e.Customer).IsLoaded);
                        }
                    }
                }
            }

            Assert.Equal(productLoaded, context.Entry(orderDetail).Reference(e => e.Product).IsLoaded);
            if (orderDetail.Product != null)
            {
                Assert.False(context.Entry(orderDetail.Product).Collection(e => e.OrderDetails).IsLoaded);
            }
        }

        protected virtual void ClearLog()
        {
        }

        protected NorthwindContext CreateContext() => Fixture.CreateContext();
    }
}
