// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public abstract class GraphUpdatesInMemoryTestBase<TFixture> : GraphUpdatesTestBase<InMemoryTestStore, TFixture>
        where TFixture : GraphUpdatesInMemoryTestBase<TFixture>.GraphUpdatesInMemoryFixtureBase, new()
    {
        protected GraphUpdatesInMemoryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        public abstract class GraphUpdatesInMemoryFixtureBase : GraphUpdatesFixtureBase
        {
            private readonly IServiceProvider _serviceProvider;

            protected GraphUpdatesInMemoryFixtureBase()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFramework()
                    .AddInMemoryStore()
                    .ServiceCollection()
                    .AddSingleton(TestInMemoryModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();
            }

            public override InMemoryTestStore CreateTestStore()
            {
                var store = new InMemoryGraphUpdatesTestStore(_serviceProvider);
                Seed(store.Context);
                return store;
            }

            public override DbContext CreateContext(InMemoryTestStore testStore)
            {
                return ((InMemoryGraphUpdatesTestStore)testStore).Context;
            }
        }

        public class InMemoryGraphUpdatesTestStore : InMemoryTestStore
        {
            public InMemoryGraphUpdatesTestStore(IServiceProvider serviceProvider)
            {
                var options = new DbContextOptions();
                options.UseInMemoryStore(persist: true);

                Context = new GraphUpdatesContext(serviceProvider, options);

                Context.Database.EnsureCreated();
            }

            public DbContext Context { get; }

            public override void Dispose()
            {
                Context.Database.EnsureDeleted();

                base.Dispose();
            }
        }
    }
}
