// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.AzureTableStorage.Adapters;
using Microsoft.Data.Entity.AzureTableStorage.Requests;
using Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Update;
using Microsoft.Framework.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests
{
    public class AtsDataStoreTests : AtsDataStore, IClassFixture<Mock<AtsConnection>>
    {
        private readonly Mock<AtsConnection> _connection;
        private Queue<TableResult> _queue;

        public AtsDataStoreTests(Mock<AtsConnection> connection)
            : base(connection.Object, new TableEntityAdapterFactory())
        {
            _connection = connection;
        }

        private void QueueResult(params TableResult[] results)
        {
            _queue = new Queue<TableResult>(results.Length);
            foreach (var result in results)
            {
                _queue.Enqueue(result);
            }

            _connection.Setup(s => s.ExecuteRequestAsync(
                It.IsAny<AtsAsyncRequest<TableResult>>(),
                It.IsAny<ILogger>(),
                It.IsAny<CancellationToken>()))
                .Returns(() => Task.Run(() => _queue.Dequeue()));
        }

        private void QueueError(TableResult badRequest)
        {
            var res = new RequestResult { HttpStatusCode = badRequest.HttpStatusCode };
            var exception = new StorageException(res, "error", Mock.Of<Exception>());
            _connection.Setup(s => s.ExecuteRequestAsync(
                It.IsAny<AtsAsyncRequest<TableResult>>(),
                It.IsAny<ILogger>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);
        }

        [Fact]
        public void It_saves()
        {
            QueueResult(TestTableResult.OK(), TestTableResult.OK(), TestTableResult.OK());
            var entries = new List<StateEntry>
                {
                    TestStateEntry.Mock().WithState(EntityState.Added).WithType("Test1"),
                    TestStateEntry.Mock().WithState(EntityState.Modified).WithType("Test1"),
                    TestStateEntry.Mock().WithState(EntityState.Deleted).WithType("Test1"),
                };
            var succeeded = SaveChangesAsync(entries).Result;
            Assert.Equal(3, succeeded);
        }

        [Fact]
        public void It_throws_concurrency_exception()
        {
            QueueError(TestTableResult.WithStatus(HttpStatusCode.PreconditionFailed));
            var entries = new List<StateEntry> { TestStateEntry.Mock().WithState(EntityState.Added).WithType("Test1") };

            Assert.Equal(
                Strings.ETagPreconditionFailed,
                Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => SaveChangesAsync(entries)).Result.Message);
        }

        [Fact]
        public void It_throws_dbupdate_exception()
        {
            QueueError(TestTableResult.BadRequest());
            var entries = new List<StateEntry> { TestStateEntry.Mock().WithState(EntityState.Modified).WithType("Test1") };
            Assert.Equal(
                Strings.SaveChangesFailed,
                Assert.ThrowsAsync<DbUpdateException>(() => SaveChangesAsync(entries)).Result.Message
                );
        }

        [Theory]
        [InlineData(EntityState.Added, TableOperationType.Insert)]
        [InlineData(EntityState.Modified, TableOperationType.Merge)]
        [InlineData(EntityState.Deleted, TableOperationType.Delete)]
        [InlineData(EntityState.Unknown, null)]
        [InlineData(EntityState.Unchanged, null)]
        public void It_maps_entity_state_to_table_operations(EntityState entityState, TableOperationType operationType)
        {
            var entry = TestStateEntry.Mock().WithState(entityState);
            var request = CreateRequest(new AtsTable("Test"), entry);

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
    }
}
