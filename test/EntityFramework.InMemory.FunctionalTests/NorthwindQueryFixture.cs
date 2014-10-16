// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Advanced;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Logging;
using Northwind;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class NorthwindQueryFixture : NorthwindQueryFixtureBase
    {
        private readonly DbContextOptions _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly Model _model;

        public NorthwindQueryFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddInMemoryStore()
                    .UseLoggerFactory<LoggerFactory>()
                    .ServiceCollection
                    .BuildServiceProvider();

            _model = CreateModel();

            _options
                = new DbContextOptions()
                    .UseModel(_model)
                    .UseInMemoryStore();

            using (var context = CreateContext())
            {
                Seed(context);
            }
        }

        public DbContext CreateContext(bool persist = true)
        {
            if (persist)
            {
                return new DbContext(_serviceProvider, _options);
            }

            var options = new DbContextOptions()
                .UseModel(_model)
                .UseInMemoryStore(persist: false);

            return new DbContext(_serviceProvider, options);
        }

        public void Seed(DbContext context)
        {
            var titleProperty
                = _model.GetEntityType(typeof(Employee)).GetProperty("Title");

            context.Set<Customer>().AddRange(NorthwindData.CreateCustomers());

            foreach (var employee in NorthwindData.CreateEmployees())
            {
                context.Set<Employee>().Add(employee);
                context.ChangeTracker.Entry(employee).StateEntry[titleProperty] = employee.Title;
            }

            context.Set<Order>().AddRange(NorthwindData.CreateOrders());
            context.Set<Product>().AddRange(NorthwindData.CreateProducts());
            context.Set<OrderDetail>().AddRange(NorthwindData.CreateOrderDetails());

            context.SaveChanges();
        }
    }
}
