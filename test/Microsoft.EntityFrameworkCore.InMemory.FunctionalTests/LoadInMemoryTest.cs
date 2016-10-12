// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class LoadInMemoryTest
        : LoadTestBase<InMemoryTestStore, LoadInMemoryTest.LoadInMemoryFixture>
    {
        public LoadInMemoryTest(LoadInMemoryFixture fixture)
            : base(fixture)
        {
        }

        public class LoadInMemoryFixture : LoadFixtureBase
        {
            public const string DatabaseName = "LoadTest";
            private readonly DbContextOptions _options;

            public LoadInMemoryFixture()
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .AddSingleton(TestInMemoryModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();

                _options = new DbContextOptionsBuilder()
                    .UseInMemoryDatabase(DatabaseName)
                    .UseInternalServiceProvider(serviceProvider)
                    .Options;
            }

            public override InMemoryTestStore CreateTestStore()
                => InMemoryTestStore.GetOrCreateShared(DatabaseName, () =>
                    {
                        using (var context = new LoadContext(_options))
                        {
                            Seed(context);
                        }
                    });

            public override DbContext CreateContext(InMemoryTestStore testStore)
                => new LoadContext(_options);
        }
    }
}
