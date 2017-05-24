// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class FieldMappingInMemoryTest
        : FieldMappingTestBase<InMemoryTestStore, FieldMappingInMemoryTest.FieldMappingInMemoryFixture>
    {
        private const string DatabaseName = "FieldMapping";

        public FieldMappingInMemoryTest(FieldMappingInMemoryFixture fixture)
            : base(fixture)
        {
        }

        public class FieldMappingInMemoryFixture : FieldMappingFixtureBase
        {
            private readonly IServiceProvider _serviceProvider;

            public FieldMappingInMemoryFixture()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();
            }

            public override InMemoryTestStore CreateTestStore()
            {
                var store = new InMemoryGraphUpdatesTestStore(_serviceProvider);

                using (var context = CreateContext(store))
                {
                    Seed(context);
                }

                return store;
            }

            public override DbContext CreateContext(InMemoryTestStore testStore)
                => new FieldMappingContext(new DbContextOptionsBuilder()
                    .UseInMemoryDatabase(DatabaseName)
                    .UseInternalServiceProvider(_serviceProvider)
                    .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    .Options);

            public class InMemoryGraphUpdatesTestStore : InMemoryTestStore
            {
                private readonly IServiceProvider _serviceProvider;

                public InMemoryGraphUpdatesTestStore(IServiceProvider serviceProvider)
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
