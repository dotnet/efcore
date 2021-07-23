// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class GraphUpdatesInMemoryTest : GraphUpdatesTestBase<GraphUpdatesInMemoryTest.InMemoryFixture>
    {
        public GraphUpdatesInMemoryTest(InMemoryFixture fixture)
            : base(fixture)
        {
        }

        public override void Required_many_to_one_dependents_are_cascade_deleted_in_store(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            // FK uniqueness not enforced in in-memory database
        }

        public override void Optional_many_to_one_dependents_are_orphaned_in_store(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            // FK uniqueness not enforced in in-memory database
        }

        public override void Required_many_to_one_dependents_with_alternate_key_are_cascade_deleted_in_store(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            // FK uniqueness not enforced in in-memory database
        }

        public override void Optional_many_to_one_dependents_with_alternate_key_are_orphaned_in_store(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            // FK uniqueness not enforced in in-memory database
        }

        public override void Optional_one_to_one_relationships_are_one_to_one(
            CascadeTiming? deleteOrphansTiming)
        {
            // FK uniqueness not enforced in in-memory database
        }

        public override void Required_one_to_one_relationships_are_one_to_one(
            CascadeTiming? deleteOrphansTiming)
        {
            // FK uniqueness not enforced in in-memory database
        }

        public override void Save_required_one_to_one_changed_by_reference(
            ChangeMechanism changeMechanism,
            CascadeTiming? deleteOrphansTiming)
        {
            // FK uniqueness not enforced in in-memory database
        }

        public override void Sever_required_one_to_one(
            ChangeMechanism changeMechanism,
            CascadeTiming? deleteOrphansTiming)
        {
            // FK uniqueness not enforced in in-memory database
        }

        public override void Required_one_to_one_are_cascade_deleted_in_store(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            // FK uniqueness not enforced in in-memory database
        }

        public override void Required_non_PK_one_to_one_are_cascade_deleted_in_store(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            // FK uniqueness not enforced in in-memory database
        }

        public override void Optional_one_to_one_are_orphaned_in_store(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            // FK uniqueness not enforced in in-memory database
        }

        public override void Required_one_to_one_are_cascade_detached_when_Added(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            // FK uniqueness not enforced in in-memory database
        }

        public override void Required_non_PK_one_to_one_are_cascade_detached_when_Added(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            // FK uniqueness not enforced in in-memory database
        }

        public override void Optional_one_to_one_with_AK_relationships_are_one_to_one(
            CascadeTiming? deleteOrphansTiming)
        {
            // FK uniqueness not enforced in in-memory database
        }

        public override void Required_one_to_one_with_AK_relationships_are_one_to_one(
            CascadeTiming? deleteOrphansTiming)
        {
            // FK uniqueness not enforced in in-memory database
        }

        public override void Required_one_to_one_with_alternate_key_are_cascade_deleted_in_store(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            // FK uniqueness not enforced in in-memory database
        }

        public override void Required_non_PK_one_to_one_with_alternate_key_are_cascade_deleted_in_store(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            // FK uniqueness not enforced in in-memory database
        }

        public override void Optional_one_to_one_with_alternate_key_are_orphaned_in_store(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            // FK uniqueness not enforced in in-memory database
        }

        public override void Required_non_PK_one_to_one_with_alternate_key_are_cascade_detached_when_Added(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            // FK uniqueness not enforced in in-memory database
        }

        public override void Required_one_to_one_with_alternate_key_are_cascade_detached_when_Added(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            // FK uniqueness not enforced in in-memory database
        }

        protected override void ExecuteWithStrategyInTransaction(
            Action<DbContext> testOperation,
            Action<DbContext> nestedTestOperation1 = null,
            Action<DbContext> nestedTestOperation2 = null,
            Action<DbContext> nestedTestOperation3 = null)
        {
            base.ExecuteWithStrategyInTransaction(testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);
            Fixture.Reseed();
        }

        public class InMemoryFixture : GraphUpdatesFixtureBase
        {
            protected override string StoreName { get; } = "GraphUpdatesTest";

            protected override ITestStoreFactory TestStoreFactory
                => InMemoryTestStoreFactory.Instance;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).ConfigureWarnings(w => w.Log(InMemoryEventId.TransactionIgnoredWarning));
        }
    }
}
