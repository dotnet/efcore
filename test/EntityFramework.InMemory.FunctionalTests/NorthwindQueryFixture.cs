// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Northwind;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class NorthwindQueryFixture : NorthwindQueryFixtureBase
    {
        private readonly DbContextOptions _options;
        private readonly IServiceProvider _serviceProvider;

        public NorthwindQueryFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddInMemoryStore()
                    .ServiceCollection
                    .BuildServiceProvider();

            var model = CreateModel();

            var titleProperty
                = model.GetEntityType(typeof(Employee)).GetProperty("Title");

            _options
                = new DbContextOptions()
                    .UseModel(model)
                    .UseInMemoryStore();

            using (var context = new DbContext(_serviceProvider, _options))
            {
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

        public DbContext CreateContext()
        {
            return new DbContext(_serviceProvider, _options);
        }
    }
}
