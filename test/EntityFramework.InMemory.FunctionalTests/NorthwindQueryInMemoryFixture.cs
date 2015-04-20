// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class NorthwindQueryInMemoryFixture : NorthwindQueryFixtureBase
    {
        private readonly DbContextOptions _options;
        private readonly IServiceProvider _serviceProvider;

        public NorthwindQueryInMemoryFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddInMemoryStore()
                    .ServiceCollection()
                    .AddSingleton(TestInMemoryModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryStore();
            _options = optionsBuilder.Options;

            using (var context = CreateContext())
            {
                NorthwindData.Seed(context);
            }
        }

        public override NorthwindContext CreateContext()
        {
            var context = new NorthwindContext(_serviceProvider, _options);

            context.ChangeTracker.AutoDetectChangesEnabled = false;

            return context;
        }
    }
}
