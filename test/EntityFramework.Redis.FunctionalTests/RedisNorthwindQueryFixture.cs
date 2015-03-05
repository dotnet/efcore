// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Redis.Extensions;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using EntityFramework.Redis.FunctionalTests;

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
                .UseRedis("127.0.0.1", RedisTestConfig.RedisPort);

            using (var context = CreateContext())
            {
                NorthwindData.Seed(context);
            }

        }        
        public override NorthwindContext CreateContext()
        {
            return new RedisNorthwindContext(_serviceProvider, _options);
        }
    }
}
