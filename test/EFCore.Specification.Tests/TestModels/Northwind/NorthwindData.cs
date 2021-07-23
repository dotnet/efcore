﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        public Customer[] Customers { get; }
        public CustomerQuery[] CustomerQueries { get; }
        public CustomerQueryWithQueryFilter[] CustomerQueriesWithQueryFilter { get; }
        public Employee[] Employees { get; }
        public Product[] Products { get; }
        public ProductQuery[] ProductQueries { get; }
        public Order[] Orders { get; }
        public OrderQuery[] OrderQueries { get; }
        public OrderDetail[] OrderDetails { get; }

        public NorthwindData()
        {
            Customers = CreateCustomers();
            Employees = CreateEmployees();
            Products = CreateProducts();
            Orders = CreateOrders();
            OrderDetails = CreateOrderDetails();

            var customerQueries = new List<CustomerQuery>();
            CustomerQueriesWithQueryFilter = new CustomerQueryWithQueryFilter[0];

            foreach (var customer in Customers)
            {
                customer.Orders = new List<Order>();

                customerQueries.Add(
                    new CustomerQuery
                    {
                        Address = customer.Address,
                        City = customer.City,
                        CompanyName = customer.CompanyName,
                        ContactName = customer.ContactName,
                        ContactTitle = customer.ContactTitle
                    });
            }

            CustomerQueries = customerQueries.ToArray();

            var productQueries = new List<ProductQuery>();

            foreach (var product in Products)
            {
                product.OrderDetails = new List<OrderDetail>();

                if (!product.Discontinued)
                {
                    productQueries.Add(
                        new ProductQuery
                        {
                            CategoryName = "Food",
                            ProductID = product.ProductID,
                            ProductName = product.ProductName
                        });
                }
            }

            ProductQueries = productQueries.ToArray();

            var orderQueries = new List<OrderQuery>();

            foreach (var order in Orders)
            {
                order.OrderDetails = new List<OrderDetail>();

                var customer = Customers.First(c => c.CustomerID == order.CustomerID);
                order.Customer = customer;
                customer.Orders.Add(order);

                orderQueries.Add(
                    new OrderQuery { CustomerID = order.CustomerID, Customer = order.Customer });
            }

            OrderQueries = orderQueries.ToArray();

            foreach (var orderDetail in OrderDetails)
            {
                var order = Orders.First(o => o.OrderID == orderDetail.OrderID);
                orderDetail.Order = order;
                order.OrderDetails.Add(orderDetail);

                var product = Products.First(p => p.ProductID == orderDetail.ProductID);
                orderDetail.Product = product;
                product.OrderDetails.Add(orderDetail);
            }

            foreach (var employee in Employees)
            {
                var manager = Employees.FirstOrDefault(e => employee.ReportsTo == e.EmployeeID);
                employee.Manager = manager;
            }
        }

        public NorthwindData(
            Customer[] customers,
            CustomerQuery[] customerQueries,
            CustomerQueryWithQueryFilter[] customerQueriesWithQueryFilter,
            Employee[] employees,
            Product[] products,
            ProductQuery[] productQueries,
            Order[] orders,
            OrderDetail[] orderDetails)
        {
            Customers = customers;
            CustomerQueries = customerQueries;
            CustomerQueriesWithQueryFilter = customerQueriesWithQueryFilter;
            Employees = employees;
            Products = products;
            ProductQueries = productQueries;
            Orders = orders;
            OrderDetails = orderDetails;
        }

        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
        {
            if (typeof(TEntity) == typeof(Customer))
            {
                return (IQueryable<TEntity>)Customers.AsQueryable();
            }

            if (typeof(TEntity) == typeof(Employee))
            {
                return (IQueryable<TEntity>)Employees.AsQueryable();
            }

            if (typeof(TEntity) == typeof(Order))
            {
                return (IQueryable<TEntity>)Orders.AsQueryable();
            }

            if (typeof(TEntity) == typeof(OrderDetail))
            {
                return (IQueryable<TEntity>)OrderDetails.AsQueryable();
            }

            if (typeof(TEntity) == typeof(Product))
            {
                return (IQueryable<TEntity>)Products.AsQueryable();
            }

            if (typeof(TEntity) == typeof(CustomerQuery))
            {
                return (IQueryable<TEntity>)CustomerQueries.AsQueryable();
            }

            if (typeof(TEntity) == typeof(OrderQuery))
            {
                return (IQueryable<TEntity>)OrderQueries.AsQueryable();
            }

            if (typeof(TEntity) == typeof(ProductQuery))
            {
                return (IQueryable<TEntity>)ProductQueries.AsQueryable();
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
