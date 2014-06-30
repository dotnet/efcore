// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.AzureTableStorage.Adapters;
using Microsoft.Data.Entity.AzureTableStorage.Interfaces;
using Microsoft.Data.Entity.AzureTableStorage.Requests;
using Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Framework.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests
{
    public class AtsDataStoreTests : AtsDataStore, IClassFixture<Mock<AtsConnection>>
    {
        private readonly Mock<AtsConnection> _connection;

        public AtsDataStoreTests(Mock<AtsConnection> connection)
            : base(connection.Object, new TableEntityAdapterFactory())
        {
            _connection = connection;
        }

        private Task<TResult>[] SetupResults<TResult>(IEnumerable<TResult> tableResults)
        {
            var batch = new List<Task<TResult>>();
            foreach (var tableResult in tableResults)
            {
                var taskSource = new TaskCompletionSource<TResult>();
                taskSource.SetResult(tableResult);
                batch.Add(taskSource.Task);
            }
            return batch.ToArray();
        }

        [Fact]
        public void It_counts_results()
        {
            var results = SetupResults<ITableResult>(new[] { TestTableResult.OK(), TestTableResult.OK() });
            var succeeded = InspectResults(results);
            Assert.Equal(2, succeeded);
        }

        [Fact]
        public void It_throws_exception()
        {
            var exceptedBatch = new TaskCompletionSource<ITableResult>();
            exceptedBatch.SetException(new AggregateException());
            Assert.Throws<AggregateException>(() => InspectResults(new[] { exceptedBatch.Task }));
        }

        [Fact]
        public void It_fails_bad_tasks()
        {
            var results = SetupResults<ITableResult>(new[] { TestTableResult.OK(), TestTableResult.BadRequest(), TestTableResult.OK() });
            Assert.Throws<DbUpdateException>(() => InspectResults(results));
        }

        [Theory]
        [InlineData(EntityState.Added, TableOperationType.Insert)]
        [InlineData(EntityState.Modified, TableOperationType.InsertOrMerge)]
        [InlineData(EntityState.Deleted, TableOperationType.Delete)]
        [InlineData(EntityState.Unknown, null)]
        [InlineData(EntityState.Unchanged, null)]
        public void It_maps_entity_state_to_table_operations(EntityState entityState, TableOperationType operationType)
        {
            var entry = TestStateEntry.Mock().WithState(entityState);
            var request = CreateRequest(new AtsTable(), entry);

            if (request == null)
            {
                Assert.True(EntityState.Unknown.HasFlag(entityState) || EntityState.Unchanged.HasFlag(entityState));
            }
            else
            {
                var propInfo = typeof(TableOperation).GetProperty("OperationType", BindingFlags.NonPublic | BindingFlags.Instance);
                var type = (TableOperationType)propInfo.GetValue(request.Operation);
                Assert.Equal(operationType, type);
            }
        }

        [Fact]
        public void It_saves_changes()
        {
            _connection.Setup(s => s.ExecuteRequestAsync(
                It.IsAny<AtsAsyncRequest<ITableResult>>(),
                It.IsAny<ILogger>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => TestTableResult.OK()));
            var testEntries = new List<StateEntry> { TestStateEntry.Mock().WithState(EntityState.Added).WithType("Test1") };
            var changes = SaveChangesAsync(testEntries).Result;
            Assert.Equal(1, changes);
        }
    }
}
