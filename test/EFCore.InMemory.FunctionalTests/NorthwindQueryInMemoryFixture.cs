// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class NorthwindQueryInMemoryFixture : NorthwindQueryFixtureBase
    {
        private readonly DbContextOptions _options;

        private readonly TestLoggerFactory _testLoggerFactory = new TestLoggerFactory();

        public NorthwindQueryInMemoryFixture()
        {
            _options = BuildOptions();

            using (var context = CreateContext())
            {
                NorthwindData.Seed(context);
            }
        }

        public override DbContextOptions BuildOptions(IServiceCollection serviceCollection = null)
            => new DbContextOptionsBuilder()
                .UseInMemoryDatabase(nameof(NorthwindQueryInMemoryFixture))
                .UseInternalServiceProvider(
                    (serviceCollection ?? new ServiceCollection())
                        .AddEntityFrameworkInMemoryDatabase()
                        .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                        .AddSingleton<ILoggerFactory>(_testLoggerFactory)
                        .BuildServiceProvider()).Options;

        public override NorthwindContext CreateContext(
            QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.TrackAll)
            => new NorthwindContext(_options, queryTrackingBehavior);
    }
}
