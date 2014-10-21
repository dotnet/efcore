// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Microsoft.Data.Entity.Redis.Extensions;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.Redis.FunctionalTests
{
    public class RedisNorthwindQueryFixture : NorthwindQueryFixtureBase
    {
        private readonly DbContextOptions _options;
        private readonly IServiceProvider _serviceProvider;

        public RedisNorthwindQueryFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddRedis()
                    .ServiceCollection
                    .BuildServiceProvider();

            _options = new DbContextOptions()
                .UseModel(CreateModel())
                .UseRedis("127.0.0.1", RedisTestConfig.RedisPort);

            using (var context = CreateContext())
            {
                if (!TestDataExists(context))
                {
                    // delete any pre-existing data from last run
                    DeleteExistingTestData(context);

                    // recreate data for this run
                    NorthwindData.Seed(context);
                }
            }
        }

        public override NorthwindContext CreateContext()
        {
            return new NorthwindContext(_serviceProvider, _options);
        }

        private static bool TestDataExists(DbContext context)
        {
            return context.Set<Customer>().Any();
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
