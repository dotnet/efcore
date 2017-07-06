// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class OneToOneQueryInMemoryFixture : OneToOneQueryFixtureBase
    {
        private readonly DbContextOptions _options;

        public OneToOneQueryInMemoryFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                .BuildServiceProvider(validateScopes: true);

            _options = new DbContextOptionsBuilder()
                .UseInMemoryDatabase(nameof(OneToOneQueryInMemoryFixture))
                .UseInternalServiceProvider(serviceProvider).Options;

            using (var context = new DbContext(_options))
            {
                AddTestData(context);
            }
        }

        public DbContext CreateContext() => new DbContext(_options);
    }
}
