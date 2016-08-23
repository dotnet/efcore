// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.ConcurrencyModel;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class F1InMemoryFixture : F1FixtureBase<InMemoryTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public F1InMemoryFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton(TestInMemoryModelSource.GetFactory(OnModelCreating))
                .BuildServiceProvider();
        }

        public override InMemoryTestStore CreateTestStore()
        {
            var store = new InMemoryF1TestStore(_serviceProvider);

            using (var context = CreateContext(store))
            {
                ConcurrencyModelInitializer.Seed(context);
            }

            return store;
        }

        public override F1Context CreateContext(InMemoryTestStore testStore)
            => new F1Context(new DbContextOptionsBuilder()
                .UseInMemoryDatabase()
                .UseInternalServiceProvider(_serviceProvider).Options);

        public class InMemoryF1TestStore : InMemoryTestStore
        {
            private readonly IServiceProvider _serviceProvider;

            public InMemoryF1TestStore(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public override void Dispose()
            {
                _serviceProvider.GetRequiredService<IInMemoryStoreSource>().GetGlobalStore().Clear();

                base.Dispose();
            }
        }
    }
}
