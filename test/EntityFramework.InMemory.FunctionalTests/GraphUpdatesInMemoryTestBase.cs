// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestUtilities.Xunit;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Framework.DependencyInjection;
using Xunit;

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

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Principal, false)]
        public override void Save_required_one_to_one_changed_by_reference_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            // Cascade delete not supported by in-memory database
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Principal, false)]
        public override void Save_required_non_PK_one_to_one_changed_by_reference_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            // Cascade delete not supported by in-memory database
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Principal)]
        public override void Save_required_one_to_one_changed_by_reference(ChangeMechanism changeMechanism)
        {
            // Cascade delete not supported by in-memory database
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Principal)]
        public override void Save_removed_required_many_to_one_dependents(ChangeMechanism changeMechanism)
        {
            // Cascade delete not supported by in-memory database
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Principal, false)]
        public override void Save_required_non_PK_one_to_one_changed_by_reference(ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            // Cascade delete not supported by in-memory database
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Principal)]
        public override void Sever_required_one_to_one_with_alternate_key(ChangeMechanism changeMechanism)
        {
            // Cascade delete not supported by in-memory database
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Principal)]
        public override void Sever_required_one_to_one(ChangeMechanism changeMechanism)
        {
            // Cascade delete not supported by in-memory database
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Principal)]
        public override void Sever_required_non_PK_one_to_one(ChangeMechanism changeMechanism)
        {
            // Cascade delete not supported by in-memory database
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Principal)]
        public override void Sever_required_non_PK_one_to_one_with_alternate_key(ChangeMechanism changeMechanism)
        {
            // Cascade delete not supported by in-memory database
        }

        public abstract class GraphUpdatesInMemoryFixtureBase : GraphUpdatesFixtureBase
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly DbContextOptionsBuilder _optionsBuilder;

            protected GraphUpdatesInMemoryFixtureBase()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFramework()
                    .AddInMemoryDatabase()
                    .ServiceCollection()
                    .AddSingleton(TestInMemoryModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();

                _optionsBuilder = new DbContextOptionsBuilder();
                _optionsBuilder.UseInMemoryDatabase();
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
            {
                var optionsBuilder = new DbContextOptionsBuilder();
                optionsBuilder.UseInMemoryDatabase();

                return new GraphUpdatesContext(_serviceProvider, optionsBuilder.Options);
            }

            public class InMemoryGraphUpdatesTestStore : InMemoryTestStore
            {
                private readonly IServiceProvider _serviceProvider;

                public InMemoryGraphUpdatesTestStore(IServiceProvider serviceProvider)
                {
                    _serviceProvider = serviceProvider;
                }

                public override void Dispose()
                {
                    _serviceProvider.GetRequiredService<IInMemoryStore>().Clear();

                    base.Dispose();
                }
            }
        }
    }
}
