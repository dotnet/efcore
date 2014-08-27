// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Redis.Extensions;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Northwind;

namespace Microsoft.Data.Entity.Redis
{
    public class RedisNorthwindQueryFixture : NorthwindQueryFixtureBase
    {
        private readonly DbContextOptions _options;
        private readonly IServiceProvider _serviceProvider;

        public RedisNorthwindQueryFixture()
        {
            var model = CreateModel();

            _options = new DbContextOptions()
                .UseModel(model)
                .UseRedis("127.0.0.1", RedisTestConfig.RedisPort);

            var services = new ServiceCollection();
            services.AddEntityFramework().AddRedis();
            _serviceProvider = services.BuildServiceProvider(); 
            
            using (var context = new DbContext(_options))
            {
                if (!TestDataExists(context))
                {
                    // delete any pre-existing data from last run
                    DeleteExistingTestData(context);

                    // recreate data for this run
                    CreateTestData(context, model);
                }
            }
        }

        public DbContext CreateContext()
        {
            return new DbContext(_serviceProvider, _options);
        }

        private static bool TestDataExists(DbContext context)
        {
            if (NorthwindData.Customers.Length == context.Set<Customer>().Count()
                && NorthwindData.Employees.Length == context.Set<Employee>().Count()
                && NorthwindData.Orders.Length == context.Set<Order>().Count()
                && NorthwindData.Products.Length == context.Set<Product>().Count()
                && NorthwindData.OrderDetails.Length == context.Set<OrderDetail>().Count())
            {
                return true;
            }

            return false;
        }

        private static void CreateTestData(DbContext context, Model model)
        {
            var titleProperty
                = model.GetEntityType(typeof(Employee)).GetProperty("Title");

            foreach (var employee in NorthwindData.Employees)
            {
                context.Set<Employee>().Add(employee);
                context.ChangeTracker.Entry(employee).StateEntry[titleProperty] = employee.Title;
            }

            context.Set<Customer>().AddRange(NorthwindData.Customers);
            context.Set<Order>().AddRange(NorthwindData.Orders);
            context.Set<Product>().AddRange(NorthwindData.Products);
            context.Set<OrderDetail>().AddRange(NorthwindData.OrderDetails);
            context.SaveChanges();
        }

        private static void DeleteExistingTestData(DbContext context)
        {
            context.Set<OrderDetail>().RemoveRange(context.Set<OrderDetail>());
            context.Set<Product>().RemoveRange(context.Set<Product>());
            context.Set<Order>().RemoveRange(context.Set<Order>());
            context.Set<Employee>().RemoveRange(context.Set<Employee>());
            context.Set<Customer>().RemoveRange(context.Set<Customer>());
            context.SaveChanges();
        }
    }
}
