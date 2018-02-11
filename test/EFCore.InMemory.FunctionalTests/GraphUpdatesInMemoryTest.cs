// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class GraphUpdatesInMemoryTest
        : GraphUpdatesTestBase<GraphUpdatesInMemoryTest.GraphUpdatesInMemoryFixture>
    {
        public GraphUpdatesInMemoryTest(GraphUpdatesInMemoryFixture fixture)
            : base(fixture)
        {
        }

        public override DbUpdateException Optional_One_to_one_relationships_are_one_to_one()
        {
            // FK uniqueness not enforced in in-memory database
            return null;
        }

        public override DbUpdateException Required_One_to_one_relationships_are_one_to_one()
        {
            // FK uniqueness not enforced in in-memory database
            return null;
        }

        public override DbUpdateException Optional_One_to_one_with_AK_relationships_are_one_to_one()
        {
            // FK uniqueness not enforced in in-memory database
            return null;
        }

        public override DbUpdateException Required_One_to_one_with_AK_relationships_are_one_to_one()
        {
            // FK uniqueness not enforced in in-memory database
            return null;
        }

        public override void Save_required_one_to_one_changed_by_reference_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            // Cascade delete not supported by in-memory database
        }

        public override void Save_required_non_PK_one_to_one_changed_by_reference_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            // Cascade delete not supported by in-memory database
        }

        public override DbUpdateException Save_required_one_to_one_changed_by_reference(ChangeMechanism changeMechanism)
        {
            // Cascade delete not supported by in-memory database
            return null;
        }

        public override void Save_removed_required_many_to_one_dependents(ChangeMechanism changeMechanism)
        {
            // Cascade delete not supported by in-memory database
        }

        public override void Save_required_non_PK_one_to_one_changed_by_reference(ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            // Cascade delete not supported by in-memory database
        }

        public override void Sever_required_one_to_one_with_alternate_key(ChangeMechanism changeMechanism)
        {
            // Cascade delete not supported by in-memory database
        }

        public override DbUpdateException Sever_required_one_to_one(ChangeMechanism changeMechanism)
        {
            // Cascade delete not supported by in-memory database
            return null;
        }

        public override void Sever_required_non_PK_one_to_one(ChangeMechanism changeMechanism)
        {
            // Cascade delete not supported by in-memory database
        }

        public override void Sever_required_non_PK_one_to_one_with_alternate_key(ChangeMechanism changeMechanism)
        {
            // Cascade delete not supported by in-memory database
        }

        public override DbUpdateException Required_many_to_one_dependents_are_cascade_deleted_in_store()
        {
            // Cascade delete not supported by in-memory database
            return null;
        }

        public override DbUpdateException Required_one_to_one_are_cascade_deleted_in_store()
        {
            // Cascade delete not supported by in-memory database
            return null;
        }

        public override DbUpdateException Required_non_PK_one_to_one_are_cascade_deleted_in_store()
        {
            // Cascade delete not supported by in-memory database
            return null;
        }

        public override DbUpdateException Required_many_to_one_dependents_with_alternate_key_are_cascade_deleted_in_store()
        {
            // Cascade delete not supported by in-memory database
            return null;
        }

        public override DbUpdateException Required_one_to_one_with_alternate_key_are_cascade_deleted_in_store()
        {
            // Cascade delete not supported by in-memory database
            return null;
        }

        public override DbUpdateException Required_non_PK_one_to_one_with_alternate_key_are_cascade_deleted_in_store()
        {
            // Cascade delete not supported by in-memory database
            return null;
        }

        public override DbUpdateException Optional_many_to_one_dependents_are_orphaned_in_store()
        {
            // Cascade nulls not supported by in-memory database
            return null;
        }

        public override DbUpdateException Optional_one_to_one_are_orphaned_in_store()
        {
            // Cascade nulls not supported by in-memory database
            return null;
        }

        public override DbUpdateException Optional_many_to_one_dependents_with_alternate_key_are_orphaned_in_store()
        {
            // Cascade nulls not supported by in-memory database
            return null;
        }

        public override DbUpdateException Optional_one_to_one_with_alternate_key_are_orphaned_in_store()
        {
            // Cascade nulls not supported by in-memory database
            return null;
        }

        public override DbUpdateException Required_non_PK_one_to_one_with_alternate_key_are_cascade_detached_when_Added()
        {
            // Cascade nulls not supported by in-memory database
            return null;
        }

        public override DbUpdateException Required_one_to_one_are_cascade_detached_when_Added()
        {
            // Cascade nulls not supported by in-memory database
            return null;
        }

        public override DbUpdateException Required_one_to_one_with_alternate_key_are_cascade_detached_when_Added()
        {
            // Cascade nulls not supported by in-memory database
            return null;
        }

        public override DbUpdateException Required_non_PK_one_to_one_are_cascade_detached_when_Added()
        {
            // Cascade nulls not supported by in-memory database
            return null;
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

        public class GraphUpdatesInMemoryFixture : GraphUpdatesFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => InMemoryTestStoreFactory.Instance;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).ConfigureWarnings(w => w.Log(InMemoryEventId.TransactionIgnoredWarning));
        }
    }
}
