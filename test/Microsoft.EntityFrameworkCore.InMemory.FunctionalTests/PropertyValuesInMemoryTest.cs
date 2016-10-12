// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class PropertyValuesInMemoryTest
        : PropertyValuesTestBase<InMemoryTestStore, PropertyValuesInMemoryTest.PropertyValuesInMemoryFixture>
    {
        public PropertyValuesInMemoryTest(PropertyValuesInMemoryFixture fixture)
            : base(fixture)
        {
        }

        public class PropertyValuesInMemoryFixture : PropertyValuesFixtureBase
        {
            private readonly IServiceProvider _serviceProvider;

            public PropertyValuesInMemoryFixture()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .AddSingleton(TestInMemoryModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();
            }

            public override InMemoryTestStore CreateTestStore()
            {
                var store = new InMemoryPropertyValuesTestStore(_serviceProvider);

                using (var context = CreateContext(store))
                {
                    Seed(context);
                }

                return store;
            }

            public override DbContext CreateContext(InMemoryTestStore testStore)
                => new AdvancedPatternsMasterContext(new DbContextOptionsBuilder()
                    .UseInMemoryDatabase()
                    .UseInternalServiceProvider(_serviceProvider).Options);

            public class InMemoryPropertyValuesTestStore : InMemoryTestStore
            {
                private readonly IServiceProvider _serviceProvider;

                public InMemoryPropertyValuesTestStore(IServiceProvider serviceProvider)
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
}
