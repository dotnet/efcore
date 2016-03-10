// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.FunctionalTests;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class NullKeysInMemoryTest : NullKeysTestBase<NullKeysInMemoryTest.NullKeysInMemoryFixture>
    {
        public NullKeysInMemoryTest(NullKeysInMemoryFixture fixture)
            : base(fixture)
        {
        }

        public class NullKeysInMemoryFixture : NullKeysFixtureBase
        {
            private readonly DbContextOptions _options;

            public NullKeysInMemoryFixture()
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFramework()
                    .AddInMemoryDatabase()
                    .AddSingleton(TestInMemoryModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();

                _options = new DbContextOptionsBuilder()
                    .UseInMemoryDatabase()
                    .UseInternalServiceProvider(serviceProvider).Options;

                EnsureCreated();
            }

            public override DbContext CreateContext() 
                => new DbContext(_options);
        }
    }
}
