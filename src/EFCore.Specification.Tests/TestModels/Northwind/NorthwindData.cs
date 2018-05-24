// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind
{
    public partial class NorthwindData : IExpectedData
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
                    new OrderQuery
                    {
                        CustomerID = order.CustomerID,
                        Customer = order.Customer
                    });
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
                return new AsyncEnumerable<TEntity>(_customers.Cast<TEntity>());
            }

            if (typeof(TEntity) == typeof(Employee))
            {
                return new AsyncEnumerable<TEntity>(_employees.Cast<TEntity>());
            }

            if (typeof(TEntity) == typeof(Order))
            {
                return new AsyncEnumerable<TEntity>(_orders.Cast<TEntity>());
            }

            if (typeof(TEntity) == typeof(OrderDetail))
            {
                return new AsyncEnumerable<TEntity>(_orderDetails.Cast<TEntity>());
            }

            if (typeof(TEntity) == typeof(Product))
            {
                return new AsyncEnumerable<TEntity>(_products.Cast<TEntity>());
            }

            if (typeof(TEntity) == typeof(CustomerView))
            {
                return new AsyncEnumerable<TEntity>(_customerViews.Cast<TEntity>());
            }

            if (typeof(TEntity) == typeof(OrderQuery))
            {
                return new AsyncEnumerable<TEntity>(_orderQueries.Cast<TEntity>());
            }

            throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
        }

        public static void Seed(NorthwindContext context)
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

            context.SaveChanges();
        }

        private class AsyncEnumerable<T> : IAsyncQueryProvider, IOrderedQueryable<T>
        {
            private readonly EnumerableQuery<T> _enumerableQuery;

            public AsyncEnumerable(IEnumerable<T> enumerable)
            {
                _enumerableQuery = new EnumerableQuery<T>(enumerable);
            }

            private AsyncEnumerable(Expression expression)
            {
                _enumerableQuery = new EnumerableQuery<T>(expression);
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
                => new AsyncEnumerable<TElement>(RewriteShadowPropertyAccess(expression));

            public TResult Execute<TResult>(Expression expression)
                => ((IQueryProvider)_enumerableQuery)
                    .Execute<TResult>(RewriteShadowPropertyAccess(expression));

            public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
                => Task.FromResult(Execute<TResult>(RewriteShadowPropertyAccess(expression)));

            public IEnumerator<T> GetEnumerator() => ((IQueryable<T>)_enumerableQuery).GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public Expression Expression => ((IQueryable)_enumerableQuery).Expression;
            public Type ElementType => typeof(T);
            public IQueryProvider Provider => this;

            private static Expression RewriteShadowPropertyAccess(Expression expression)
                => new ShadowStateAccessRewriter().Visit(expression);

            private class ShadowStateAccessRewriter : ExpressionVisitorBase
            {
                protected override Expression VisitMethodCall(MethodCallExpression expression)
                    => expression.Method.IsEFPropertyMethod()
                        ? Expression.Property(
                            expression.Arguments[0].RemoveConvert(),
                            Expression.Lambda<Func<string>>(expression.Arguments[1]).Compile().Invoke())
                        : base.VisitMethodCall(expression);
            }

            public IQueryable CreateQuery(Expression expression)
            {
                throw new NotImplementedException();
            }

            public object Execute(Expression expression)
            {
                throw new NotImplementedException();
            }

            public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
            {
                throw new NotImplementedException();
            }
        }
    }
}
