// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NorthwindQueryFixtureBase<TModelCustomizer> : SharedStoreFixtureBase<NorthwindContext>, IFilteredQueryFixtureBase
        where TModelCustomizer : IModelCustomizer, new()
    {
        public Func<DbContext> GetContextCreator()
            => () => CreateContext();

        private readonly Dictionary<(bool, string, string), ISetSource> _expectedDataCache = new Dictionary<(bool, string, string), ISetSource>();

        public virtual ISetSource GetExpectedData()
            => new NorthwindData();

        public virtual ISetSource GetFilteredExpectedData(DbContext context)
        {
            var applyFilters = typeof(TModelCustomizer) == typeof(NorthwindQueryFiltersCustomizer);
            var tenantPrefix = applyFilters ? ((NorthwindContext)context).TenantPrefix : null;
            var searchTerm = applyFilters ? ((NorthwindContext)context).SearchTerm : null;

            if (_expectedDataCache.TryGetValue((applyFilters, tenantPrefix, searchTerm), out var cachedResult))
            {
                return cachedResult;
            }

            var expectedData = new NorthwindData();
            if (applyFilters)
            {
                var customers = expectedData.Customers.Where(c => c.CompanyName.StartsWith(tenantPrefix)).ToArray();
                var customerQueriesWithQueryFilter = expectedData.CustomerQueriesWithQueryFilter.Where(cq => cq.CompanyName.StartsWith(searchTerm)).ToArray();
                var employees = expectedData.Employees.Where(e => e.Address.StartsWith("A")).ToArray();
                var products = expectedData.Products.Where(p => p.Discontinued).ToArray();
                var orders = expectedData.Orders.Where(o => o.Customer.CompanyName.StartsWith(tenantPrefix)).ToArray();
                var orderDetails = expectedData.OrderDetails.Where(od => od.Order.Customer.CompanyName.StartsWith(tenantPrefix) && od.Quantity > 50).ToArray();

                foreach (var product in products)
                {
                    product.OrderDetails = product.OrderDetails.Where(od => od.Quantity > 50).ToList();
                }

                foreach (var order in orders)
                {
                    order.OrderDetails = order.OrderDetails.Where(od => od.Quantity > 50).ToList();
                }

                foreach (var orderDetail in orderDetails)
                {
                    orderDetail.Order = orderDetail.Order.Customer.CompanyName.StartsWith(tenantPrefix) ? orderDetail.Order : null;
                    orderDetail.Product = orderDetail.Product.Discontinued ? orderDetail.Product : null;
                }

                expectedData = new NorthwindData(
                    customers,
                    expectedData.CustomerQueries,
                    customerQueriesWithQueryFilter,
                    employees,
                    products,
                    expectedData.ProductQueries,
                    orders,
                    orderDetails);
            }

            _expectedDataCache[(applyFilters, tenantPrefix, searchTerm)] = expectedData;

            return expectedData;
        }

        public IReadOnlyDictionary<Type, object> GetEntitySorters()
            => new Dictionary<Type, Func<object, object>>
            {
                { typeof(Customer), e => ((Customer)e)?.CustomerID },
                { typeof(CustomerQuery), e => ((CustomerQuery)e)?.CompanyName },
                { typeof(Order), e => ((Order)e)?.OrderID },
                { typeof(OrderQuery), e => ((OrderQuery)e)?.CustomerID },
                { typeof(Employee), e => ((Employee)e)?.EmployeeID },
                { typeof(Product), e => ((Product)e)?.ProductID },
                { typeof(ProductQuery), e => ((ProductQuery)e)?.ProductID },
                { typeof(OrderDetail), e => (((OrderDetail)e)?.OrderID.ToString(), ((OrderDetail)e)?.ProductID.ToString()) }
            }.ToDictionary(e => e.Key, e => (object)e.Value);

        public IReadOnlyDictionary<Type, object> GetEntityAsserters()
            => null;

        protected override string StoreName { get; } = "Northwind";

        protected override bool UsePooling
            => typeof(TModelCustomizer) == typeof(NoopModelCustomizer);

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            => new TModelCustomizer().Customize(modelBuilder, context);

        protected override void Seed(NorthwindContext context)
            => NorthwindData.Seed(context);

        protected override Task SeedAsync(NorthwindContext context)
            => NorthwindData.SeedAsync(context);

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(
                c => c
                    .Log(CoreEventId.RowLimitingOperationWithoutOrderByWarning)
                    .Log(CoreEventId.FirstWithoutOrderByAndFilterWarning)
                    .Log(CoreEventId.PossibleUnintendedCollectionNavigationNullComparisonWarning)
                    .Log(CoreEventId.PossibleUnintendedReferenceComparisonWarning));
    }
}
