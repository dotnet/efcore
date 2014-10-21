// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class InMemoryNorthwindQueryFixture : NorthwindQueryFixtureBase
    {
        private readonly DbContextOptions _options;
        private readonly IServiceProvider _serviceProvider;

        public InMemoryNorthwindQueryFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddInMemoryStore()
                    .ServiceCollection
                    .BuildServiceProvider();

            _options
                = new DbContextOptions()
                    .UseModel(CreateModel())
                    .UseInMemoryStore();

            using (var context = CreateContext())
            {
                NorthwindData.Seed(context);
            }
        }

        public override NorthwindContext CreateContext()
        {
            return new NorthwindContext(_serviceProvider, _options);
        }
    }
}
