// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class OwnedQueryInMemoryFixture : OwnedQueryFixtureBase
    {
        private readonly DbContextOptions _options;

        public OwnedQueryInMemoryFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(new TestLoggerFactory())
                .BuildServiceProvider(validateScopes: true);

            _options = new DbContextOptionsBuilder()
                .UseInMemoryDatabase(nameof(OwnedQueryInMemoryFixture))
                .UseInternalServiceProvider(serviceProvider).Options;

            using (var context = new DbContext(_options))
            {
                AddTestData(context);
            }
        }

        public DbContext CreateContext() => new DbContext(_options);
    }
}
