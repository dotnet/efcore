// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class InMemoryGraphUpdatesTest : GraphUpdatesTestBase<InMemoryTestStore, InMemoryGraphUpdatesTest.InMemoryGraphUpdatesFixture>
    {
        public InMemoryGraphUpdatesTest(InMemoryGraphUpdatesFixture fixture)
            : base(fixture)
        {
        }

        public class InMemoryGraphUpdatesFixture : GraphUpdatesFixtureBase
        {
            public static readonly string DatabaseName = "GraphUpdatesTest";

            private readonly IServiceProvider _serviceProvider;

            public InMemoryGraphUpdatesFixture()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFramework()
                    .AddInMemoryStore()
                    .ServiceCollection
                    .AddSingleton<InMemoryModelSource>(p => new TestInMemoryModelSource(OnModelCreating))
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
