// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind
{
    public partial class NorthwindData : ISetSource
    {
        private readonly Customer[] _customers;
        private readonly CustomerView[] _customerViews;
        private readonly Employee[] _employees;
        private readonly Product[] _products;
        private readonly Order[] _orders;
        private readonly OrderQuery[] _orderQueries;
        private readonly OrderDetail[] _orderDetails;

        public NorthwindData()
        {
            _customers = CreateCustomers();
            _employees = CreateEmployees();
            _products = CreateProducts();
            _orders = CreateOrders();
            _orderDetails = CreateOrderDetails();

            var customerViews = new List<CustomerView>();

            foreach (var customer in _customers)
            {
                customer.Orders = new List<Order>();

                customerViews.Add(
                    new CustomerView
                    {
                        Address = customer.Address,
                        City = customer.City,
                        CompanyName = customer.CompanyName,
                        ContactName = customer.ContactName,
                        ContactTitle = customer.ContactTitle
                    });
            }

            _customerViews = customerViews.ToArray();

            foreach (var product in _products)
            {
                product.OrderDetails = new List<OrderDetail>();
            }

            var orderQueries = new List<OrderQuery>();

            foreach (var order in _orders)
            {
                order.OrderDetails = new List<OrderDetail>();

                var customer = _customers.First(c => c.CustomerID == order.CustomerID);
                order.Customer = customer;
                customer.Orders.Add(order);

                orderQueries.Add(
                    new OrderQuery { CustomerID = order.CustomerID, Customer = order.Customer });
            }

            _orderQueries = orderQueries.ToArray();

            foreach (var orderDetail in _orderDetails)
            {
                var order = _orders.First(o => o.OrderID == orderDetail.OrderID);
                orderDetail.Order = order;
                order.OrderDetails.Add(orderDetail);

                var product = _products.First(p => p.ProductID == orderDetail.ProductID);
                orderDetail.Product = product;
                product.OrderDetails.Add(orderDetail);
            }

            foreach (var employee in _employees)
            {
                var manager = _employees.FirstOrDefault(e => employee.ReportsTo == e.EmployeeID);
                employee.Manager = manager;
            }
        }

        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
        {
            if (typeof(TEntity) == typeof(Customer))
            {
                return (IQueryable<TEntity>)_customers.AsQueryable();
            }

            if (typeof(TEntity) == typeof(Employee))
            {
                return (IQueryable<TEntity>)_employees.AsQueryable();
            }

            if (typeof(TEntity) == typeof(Order))
            {
                return (IQueryable<TEntity>)_orders.AsQueryable();
            }

            if (typeof(TEntity) == typeof(OrderDetail))
            {
                return (IQueryable<TEntity>)_orderDetails.AsQueryable();
            }

            if (typeof(TEntity) == typeof(Product))
            {
                return (IQueryable<TEntity>)_products.AsQueryable();
            }

            if (typeof(TEntity) == typeof(CustomerView))
            {
                return (IQueryable<TEntity>)_customerViews.AsQueryable();
            }

            if (typeof(TEntity) == typeof(OrderQuery))
            {
                return (IQueryable<TEntity>)_orderQueries.AsQueryable();
            }

            throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
        }

        public static void Seed(NorthwindContext context)
        {
            AddEntities(context);

            context.SaveChanges();
        }

        public static Task SeedAsync(NorthwindContext context)
        {
            AddEntities(context);

            return context.SaveChangesAsync();
        }

        private static void AddEntities(NorthwindContext context)
        {
            context.Set<Customer>().AddRange(CreateCustomers());

            var titleProperty = context.Model.FindEntityType(typeof(Employee)).FindProperty("Title");
            foreach (var employee in CreateEmployees())
            {
                context.Set<Employee>().Add(employee);
                context.Entry(employee).GetInfrastructure()[titleProperty] = employee.Title;
            }

            context.Set<Order>().AddRange(CreateOrders());
            context.Set<Product>().AddRange(CreateProducts());
            context.Set<OrderDetail>().AddRange(CreateOrderDetails());
        }
    }
}
