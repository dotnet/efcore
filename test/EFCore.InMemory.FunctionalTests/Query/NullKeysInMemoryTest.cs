// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
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
                    .AddEntityFrameworkInMemoryDatabase()
                    .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider(validateScopes: true);

                _options = new DbContextOptionsBuilder()
                    .UseInMemoryDatabase(nameof(NullKeysInMemoryFixture))
                    .UseInternalServiceProvider(serviceProvider).Options;

                EnsureCreated();
            }

            public override DbContext CreateContext()
                => new DbContext(_options);
        }
    }
}
