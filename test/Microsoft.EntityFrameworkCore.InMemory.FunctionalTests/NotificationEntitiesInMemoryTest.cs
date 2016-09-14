// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class NotificationEntitiesInMemoryTest
        : NotificationEntitiesTestBase<InMemoryTestStore, NotificationEntitiesInMemoryTest.NotificationEntitiesInMemoryFixture>
    {
        public static readonly string DatabaseName = "NotificationEntities";

        public NotificationEntitiesInMemoryTest(NotificationEntitiesInMemoryFixture fixture)
            : base(fixture)
        {
        }

        public class NotificationEntitiesInMemoryFixture : NotificationEntitiesFixtureBase
        {
            private readonly DbContextOptions _options;

            public NotificationEntitiesInMemoryFixture()
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .AddSingleton(TestInMemoryModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();

                _options = new DbContextOptionsBuilder()
                    .UseInMemoryDatabase()
                    .UseInternalServiceProvider(serviceProvider).Options;
            }

            public override InMemoryTestStore CreateTestStore()
                => InMemoryTestStore.GetOrCreateShared(DatabaseName, EnsureCreated);

            public override DbContext CreateContext()
                => new DbContext(_options);
        }
    }
}
