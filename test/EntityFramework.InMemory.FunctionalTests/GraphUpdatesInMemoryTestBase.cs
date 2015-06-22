// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Framework.DependencyInjection;

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
                    .AddInMemoryDatabase()
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
                var optionsBuilder = new DbContextOptionsBuilder();
                optionsBuilder.UseInMemoryDatabase(persist: true);

                Context = new GraphUpdatesContext(serviceProvider, optionsBuilder.Options);

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
