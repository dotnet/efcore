// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Cosmos
{
    public class OptimisticConcurrencyCosmosTest : OptimisticConcurrencyTestBase<F1CosmosFixture>
    {
        public OptimisticConcurrencyCosmosTest(F1CosmosFixture fixture)
            : base(fixture)
        {
            fixture.Reseed();
        }

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

        public override Task Deleting_then_updating_the_same_entity_results_in_DbUpdateConcurrencyException_which_can_be_resolved_with_store_values()
            => Task.CompletedTask;

        protected override IDbContextTransaction BeginTransaction(DatabaseFacade facade) => new FakeDbContextTransaction();

        private class FakeDbContextTransaction : IDbContextTransaction
        {
            public Guid TransactionId => new Guid();

            public void Commit()
            {
            }

            public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

            public void Dispose()
            {
            }

            public ValueTask DisposeAsync() => default;

            public void Rollback()
            {
            }

            public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        }
    }
}
