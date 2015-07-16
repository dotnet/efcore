// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class OptimisticConcurrencySqliteTest : OptimisticConcurrencyTestBase<SqliteTestStore, F1SqliteFixture>
    {
        public OptimisticConcurrencySqliteTest(F1SqliteFixture fixture)
            : base(fixture)
        {
        }

        // Override failing tests because SQLite does not allow store-generated row versions.
        // Row version behavior could be imitated on SQLite. See Issue #2195
        // TODO move these tests into the testing just for SqlServer since they don't apply to SQLite
        public override Task Simple_concurrency_exception_can_be_resolved_with_store_values() => Task.FromResult(true);
        public override Task Simple_concurrency_exception_can_be_resolved_with_client_values() => Task.FromResult(true);
        public override Task Simple_concurrency_exception_can_be_resolved_with_new_values() => Task.FromResult(true);
        public override Task Simple_concurrency_exception_can_be_resolved_with_store_values_using_equivalent_of_accept_changes() => Task.FromResult(true);
        public override Task Simple_concurrency_exception_can_be_resolved_with_store_values_using_Reload() => Task.FromResult(true);
        public override Task Deleting_then_updating_the_same_entity_results_in_DbUpdateConcurrencyException_which_can_be_resolved_with_store_values() => Task.FromResult(true);
        public override Task Deleting_then_updating_the_same_entity_results_in_DbUpdateConcurrencyException() => Task.FromResult(true);
        public override Task Updating_then_deleting_the_same_entity_results_in_DbUpdateConcurrencyException() => Task.FromResult(true);
        public override Task Updating_then_deleting_the_same_entity_results_in_DbUpdateConcurrencyException_which_can_be_resolved_with_store_values() => Task.FromResult(true);
        public override Task Deleting_the_same_entity_twice_results_in_DbUpdateConcurrencyException() => Task.FromResult(true);
        public override Task Change_in_independent_association_after_change_in_different_concurrency_token_results_in_independent_association_exception() => Task.FromResult(true);
        public override Task Change_in_independent_association_results_in_independent_association_exception() => Task.FromResult(true);
    }
}
