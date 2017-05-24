// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class PropertyValuesInMemoryTest
        : PropertyValuesTestBase<InMemoryTestStore, PropertyValuesInMemoryTest.PropertyValuesInMemoryFixture>
    {
        private const string DatabaseName = "PropertyValues";

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
                    .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
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
                    .UseInMemoryDatabase(DatabaseName)
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
                    _serviceProvider.GetRequiredService<IInMemoryStoreCache>().GetStore(DatabaseName).Clear();

                    base.Dispose();
                }
            }
        }
    }
}
