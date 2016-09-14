// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class LoadInMemoryTest
        : LoadTestBase<InMemoryTestStore, LoadInMemoryTest.LoadInMemoryFixture>
    {
        public LoadInMemoryTest(LoadInMemoryFixture fixture)
            : base(fixture)
        {
        }

        [Theory] // Skipped for in-memory. See #6123
        public override Task Load_many_to_one_reference_to_principal_already_loaded_untyped(bool async)
            => Task.FromResult(0);

        [Theory] // Skipped for in-memory. See #6123
        public override Task Load_many_to_one_reference_to_principal_using_Query_already_loaded(bool async)
            => Task.FromResult(0);

        [Theory] // Skipped for in-memory. See #6123
        public override Task Load_many_to_one_reference_to_principal_using_Query_already_loaded_untyped(bool async)
            => Task.FromResult(0);

        [Theory] // Skipped for in-memory. See #6123
        public override Task Load_many_to_one_reference_to_principal_already_loaded(bool async)
            => Task.FromResult(0);

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
            {
                return InMemoryTestStore.GetOrCreateShared(DatabaseName, () =>
                    {
                        using (var context = new LoadContext(_options))
                        {
                            Seed(context);
                        }
                    });
            }

            public override DbContext CreateContext(InMemoryTestStore testStore)
                => new LoadContext(_options);
        }
    }
}
