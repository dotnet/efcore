// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public abstract class FindInMemoryTest
        : FindTestBase<InMemoryTestStore, FindInMemoryTest.FindInMemoryFixture>
    {
        protected FindInMemoryTest(FindInMemoryFixture fixture)
            : base(fixture)
        {
        }

        public class FindInMemoryTestSet : FindInMemoryTest
        {
            public FindInMemoryTestSet(FindInMemoryFixture fixture)
                : base(fixture)
            {
            }

            protected override TEntity Find<TEntity>(DbContext context, params object[] keyValues)
                => context.Set<TEntity>().Find(keyValues);

            protected override Task<TEntity> FindAsync<TEntity>(DbContext context, params object[] keyValues)
                => context.Set<TEntity>().FindAsync(keyValues);
        }

        public class FindInMemoryTestContext : FindInMemoryTest
        {
            public FindInMemoryTestContext(FindInMemoryFixture fixture)
                : base(fixture)
            {
            }

            protected override TEntity Find<TEntity>(DbContext context, params object[] keyValues)
                => context.Find<TEntity>(keyValues);

            protected override Task<TEntity> FindAsync<TEntity>(DbContext context, params object[] keyValues)
                => context.FindAsync<TEntity>(keyValues);
        }

        public class FindInMemoryTestNonGeneric : FindInMemoryTest
        {
            public FindInMemoryTestNonGeneric(FindInMemoryFixture fixture)
                : base(fixture)
            {
            }

            protected override TEntity Find<TEntity>(DbContext context, params object[] keyValues)
                => (TEntity)context.Find(typeof(TEntity), keyValues);

            protected override async Task<TEntity> FindAsync<TEntity>(DbContext context, params object[] keyValues)
                => (TEntity)await context.FindAsync(typeof(TEntity), keyValues);
        }

        public class FindInMemoryFixture : FindFixtureBase
        {
            private readonly DbContextOptions _options;
            private readonly IServiceProvider _serviceProvider;

            public FindInMemoryFixture()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .AddSingleton(TestInMemoryModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();

                _options = new DbContextOptionsBuilder()
                    .UseInMemoryDatabase()
                    .UseInternalServiceProvider(_serviceProvider)
                    .Options;
            }

            public override InMemoryTestStore CreateTestStore()
            {
                return InMemoryTestStore.CreateScratch(() =>
                        {
                            using (var context = new FindContext(_options))
                            {
                                Seed(context);
                            }
                        },
                    _serviceProvider);
            }

            public override DbContext CreateContext(InMemoryTestStore testStore)
                => new FindContext(_options);
        }
    }
}
