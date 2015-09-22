// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Data.Entity.FunctionalTests.TestUtilities.Xunit;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public abstract class GraphUpdatesInMemoryTestBase<TFixture> : GraphUpdatesTestBase<InMemoryTestStore, TFixture>
        where TFixture : GraphUpdatesInMemoryTestBase<TFixture>.GraphUpdatesInMemoryFixtureBase, new()
    {
        protected GraphUpdatesInMemoryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact]
        public override void Required_many_to_one_dependents_are_cascade_deleted()
        {
            // Cascade delete not supported by in-memory database
        }

        public abstract class GraphUpdatesInMemoryFixtureBase : GraphUpdatesFixtureBase
        {
            private readonly IServiceProvider _serviceProvider;
            private DbContextOptionsBuilder _optionsBuilder;

            protected GraphUpdatesInMemoryFixtureBase()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFramework()
                    .AddInMemoryDatabase()
                    .ServiceCollection()
                    .AddSingleton(TestInMemoryModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();

                _optionsBuilder = new DbContextOptionsBuilder();
                _optionsBuilder.UseInMemoryDatabase(persist: true);
            }

            public override InMemoryTestStore CreateTestStore()
                => new InMemoryTestStore();

            public override DbContext CreateContext(InMemoryTestStore testStore)
                => new GraphUpdatesContext(_serviceProvider, _optionsBuilder.Options);
        }
    }
}
