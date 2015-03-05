// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class OneToOneQueryInMemoryFixture : OneToOneQueryFixtureBase
    {
        private readonly DbContextOptions _options;
        private readonly IServiceProvider _serviceProvider;

        public OneToOneQueryInMemoryFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddInMemoryStore()
                    .ServiceCollection()
                    .BuildServiceProvider();

            var model = CreateModel();

            _options = new DbContextOptions()
                .UseModel(model);
            _options.UseInMemoryStore();

            using (var context = new DbContext(_serviceProvider, _options))
            {
                AddTestData(context);
            }
        }

        public DbContext CreateContext()
        {
            return new DbContext(_serviceProvider, _options);
        }
    }
}
