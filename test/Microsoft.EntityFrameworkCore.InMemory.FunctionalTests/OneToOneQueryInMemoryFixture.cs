// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class OneToOneQueryInMemoryFixture : OneToOneQueryFixtureBase
    {
        private readonly DbContextOptions _options;

        public OneToOneQueryInMemoryFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton(TestInMemoryModelSource.GetFactory(OnModelCreating))
                .BuildServiceProvider();

            _options = new DbContextOptionsBuilder()
                .UseInMemoryDatabase()
                .UseInternalServiceProvider(serviceProvider).Options;

            using (var context = new DbContext(_options))
            {
                AddTestData(context);
            }
        }

        public DbContext CreateContext() => new DbContext(_options);
    }
}
