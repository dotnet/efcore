// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class OptimisticConcurrencyCosmosTest(F1CosmosFixture<byte[]> fixture)
    : OptimisticConcurrencyTestBase<F1CosmosFixture<byte[]>, byte[]>(fixture), IAsyncLifetime
{
    // Non-persisted property in query
    // Issue #17670
    public override Task Calling_GetDatabaseValues_on_owned_entity_works(bool async)
        => Task.CompletedTask;

    public override Task Calling_Reload_on_owned_entity_works(bool async)
        => Task.CompletedTask;

    // Only ETag properties can be used as concurrency tokens
    public override Task Concurrency_issue_where_the_FK_is_the_concurrency_token_can_be_handled()
        => Task.CompletedTask;

    public override void Nullable_client_side_concurrency_token_can_be_used()
    {
    }

    // ETag concurrency doesn't work after an item was deleted
    public override Task Deleting_the_same_entity_twice_results_in_DbUpdateConcurrencyException()
        => Task.CompletedTask;

    public override Task Deleting_then_updating_the_same_entity_results_in_DbUpdateConcurrencyException()
        => Task.CompletedTask;

    public override Task
        Deleting_then_updating_the_same_entity_results_in_DbUpdateConcurrencyException_which_can_be_resolved_with_store_values()
        => Task.CompletedTask;

    public override Task Attempting_to_delete_same_relationship_twice_for_many_to_many_results_in_independent_association_exception()
        => Task.CompletedTask;

    public override Task Attempting_to_add_same_relationship_twice_for_many_to_many_results_in_independent_association_exception()
        => Task.CompletedTask;

    // Uses lazy-loader, which is always sync
    public override Task Two_concurrency_issues_in_one_to_one_related_entities_can_be_handled_by_dealing_with_dependent_first()
        => CosmosTestHelpers.Instance.NoSyncTest(
            false,
            _ => base.Two_concurrency_issues_in_one_to_one_related_entities_can_be_handled_by_dealing_with_dependent_first());

    // Uses lazy-loader, which is always sync
    public override Task Two_concurrency_issues_in_one_to_many_related_entities_can_be_handled_by_dealing_with_dependent_first()
        => CosmosTestHelpers.Instance.NoSyncTest(
            false,
            _ => base.Two_concurrency_issues_in_one_to_many_related_entities_can_be_handled_by_dealing_with_dependent_first());

    protected override IDbContextTransaction BeginTransaction(DatabaseFacade facade)
        => new FakeDbContextTransaction();

    public override Task Calling_Reload_on_an_Added_entity_that_is_not_in_database_is_no_op(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(async, a => base.Calling_Reload_on_an_Added_entity_that_is_not_in_database_is_no_op(a));

    public override Task Calling_Reload_on_an_Unchanged_entity_that_is_not_in_database_detaches_it(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, a => base.Calling_Reload_on_an_Unchanged_entity_that_is_not_in_database_detaches_it(a));

    public override Task Calling_Reload_on_a_Modified_entity_that_is_not_in_database_detaches_it(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, a => base.Calling_Reload_on_a_Modified_entity_that_is_not_in_database_detaches_it(a));

    public override Task Calling_Reload_on_a_Deleted_entity_that_is_not_in_database_detaches_it(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, a => base.Calling_Reload_on_a_Deleted_entity_that_is_not_in_database_detaches_it(a));

    public override Task Calling_Reload_on_a_Detached_entity_that_is_not_in_database_detaches_it(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, a => base.Calling_Reload_on_a_Detached_entity_that_is_not_in_database_detaches_it(a));

    public override Task Calling_Reload_on_an_Unchanged_entity_makes_the_entity_unchanged(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(async, a => base.Calling_Reload_on_an_Unchanged_entity_makes_the_entity_unchanged(a));

    public override Task Calling_Reload_on_a_Modified_entity_makes_the_entity_unchanged(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(async, a => base.Calling_Reload_on_a_Modified_entity_makes_the_entity_unchanged(a));

    public override Task Calling_Reload_on_a_Deleted_entity_makes_the_entity_unchanged(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(async, a => base.Calling_Reload_on_a_Deleted_entity_makes_the_entity_unchanged(a));

    public override Task Calling_Reload_on_an_Added_entity_that_was_saved_elsewhere_makes_the_entity_unchanged(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, a => base.Calling_Reload_on_an_Added_entity_that_was_saved_elsewhere_makes_the_entity_unchanged(a));

    public override Task Calling_Reload_on_a_Detached_entity_makes_the_entity_unchanged(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(async, a => base.Calling_Reload_on_a_Detached_entity_makes_the_entity_unchanged(a));

    private class FakeDbContextTransaction : IDbContextTransaction
    {
        public Guid TransactionId
            => new();

        public void Commit()
        {
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Dispose()
        {
        }

        public ValueTask DisposeAsync()
            => default;

        public void Rollback()
        {
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    public Task InitializeAsync()
        => Fixture.ReseedAsync();

    public Task DisposeAsync()
        => Task.CompletedTask;
}
