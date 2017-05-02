// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.ComplexNavigationsModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class ComplexNavigationsOwnedQueryInMemoryFixture : ComplexNavigationsOwnedQueryFixtureBase<InMemoryTestStore>
    {
        public const string DatabaseName = "InMemoryOwnedQueryTest";

        private readonly DbContextOptions _options;

        private readonly TestLoggerFactory _testLoggerFactory = new TestLoggerFactory();

        public ComplexNavigationsOwnedQueryInMemoryFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(_testLoggerFactory)
                .BuildServiceProvider();

            _options = new DbContextOptionsBuilder()
                .UseInternalServiceProvider(serviceProvider)
                .UseInMemoryDatabase(nameof(ComplexNavigationsQueryInMemoryFixture)).Options;
        }

        public override InMemoryTestStore CreateTestStore()
        {
            return InMemoryTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    using (var context = new ComplexNavigationsContext(_options))
                    {
                        ComplexNavigationsModelInitializer.Seed(context, tableSplitting: true);
                    }
                });
        }

        public override ComplexNavigationsContext CreateContext(InMemoryTestStore _)
        {
            var context = new ComplexNavigationsContext(_options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return context;
        }
    }
}
