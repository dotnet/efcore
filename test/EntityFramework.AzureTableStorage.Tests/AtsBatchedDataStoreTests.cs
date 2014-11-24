// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.AzureTableStorage.Adapters;
using Microsoft.Data.Entity.AzureTableStorage.Query;
using Microsoft.Data.Entity.AzureTableStorage.Requests;
using Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Update;
using Microsoft.Framework.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests
{
    using ResultTaskList = IList<TableResult>;

    public class AtsBatchedDataStoreTests : IClassFixture<Mock<AtsConnection>>
    {
        private readonly Mock<AtsConnection> _connection;
        private readonly Queue<IList<TableResult>> _queue;

        public AtsBatchedDataStoreTests(Mock<AtsConnection> connection)
        {
            _connection = connection;
            _connection.SetupGet(s => s.Batching).Returns(true);
            _queue = new Queue<IList<TableResult>>();
        }

        private static DbContextConfiguration BuildConfig()
        {
            var config = new Mock<DbContextConfiguration>();
            config.Setup(c => c.Context).Returns(new Mock<DbContext>().Object);
            return config.Object;
        }

        private void QueueResult(params IList<TableResult>[] results)
        {
            foreach (var result in results)
            {
                _queue.Enqueue(result.ToList());
            }

            _connection.Setup(s => s.ExecuteRequestAsync(
                It.IsAny<TableBatchRequest>(),
                It.IsAny<ILogger>(),
                It.IsAny<CancellationToken>()))
                .Returns(() => Task.Run(() => _queue.Dequeue()));
        }

        private void QueueError(TableResult badResult)
        {
            var res = new RequestResult { HttpStatusCode = badResult.HttpStatusCode };
            var exception = new StorageException(res, "error", Mock.Of<Exception>());
            _connection.Setup(s => s.ExecuteRequestAsync(
                It.IsAny<TableBatchRequest>(),
                It.IsAny<ILogger>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);
        }

        [Fact]
        public void It_separates_by_partition_key()
        {
            QueueResult(new[] { TestTableResult.OK(), TestTableResult.OK(), TestTableResult.OK() }, new[] { TestTableResult.OK(), TestTableResult.OK(), TestTableResult.OK() });
            var entries = new List<StateEntry>
                {
                    TestStateEntry.Mock().WithState(EntityState.Added).WithProperty("PartitionKey", "A"),
                    TestStateEntry.Mock().WithState(EntityState.Modified).WithProperty("PartitionKey", "A"),
                    TestStateEntry.Mock().WithState(EntityState.Deleted).WithProperty("PartitionKey", "A"),
                    TestStateEntry.Mock().WithState(EntityState.Added).WithProperty("PartitionKey", "B"),
                    TestStateEntry.Mock().WithState(EntityState.Modified).WithProperty("PartitionKey", "B"),
                    TestStateEntry.Mock().WithState(EntityState.Deleted).WithProperty("PartitionKey", "B"),
                };
            Assert.Equal(6, CreateStore().SaveChangesAsync(entries).Result);
        }

        [Fact]
        public void It_throws_dbupdate_exception()
        {
            QueueError(TestTableResult.BadRequest());
            var entries = new List<StateEntry>
                {
                    TestStateEntry.Mock().WithState(EntityState.Added).WithProperty("PartitionKey", "A"),
                };
            Assert.Equal(
                Strings.SaveChangesFailed,
                Assert.ThrowsAsync<DbUpdateException>(() => CreateStore().SaveChangesAsync(entries)).Result.Message);
        }

        [Fact]
        public void It_throws_concurrency_exception()
        {
            QueueError(TestTableResult.WithStatus(HttpStatusCode.PreconditionFailed));
            var entries = new List<StateEntry>
                {
                    TestStateEntry.Mock().WithState(EntityState.Modified).WithProperty("PartitionKey", "A"),
                };
            Assert.Equal(
                Strings.ETagPreconditionFailed,
                Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => CreateStore().SaveChangesAsync(entries)).Result.Message);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(99)]
        [InlineData(100)]
        [InlineData(101)]
        [InlineData(199)]
        [InlineData(200)]
        [InlineData(201)]
        public void It_saves_changes(int expectedChanges)
        {
            var entries = new List<StateEntry>(expectedChanges);
            var responses = new List<TableResult>(expectedChanges);
            for (var i = 0; i < expectedChanges; i++)
            {
                entries.Add(TestStateEntry.Mock().WithState(EntityState.Added).WithType("TestType").WithProperty("PartitionKey", "A"));
                responses.Add(TestTableResult.OK());
                if (responses.Count >= 100)
                {
                    QueueResult(responses);
                    responses = new List<TableResult>();
                }
            }
            if (responses.Any())
            {
                QueueResult(responses);
            }

            var actualChanges = CreateStore().SaveChangesAsync(entries).Result;
            Assert.Equal(expectedChanges, actualChanges);
        }

        private AtsDataStore CreateStore()
        {
            var store = new AtsDataStore(
                Mock.Of<StateManager>(),
                new ContextService<IModel>(() => null),
                Mock.Of<EntityKeyFactorySource>(),
                Mock.Of<EntityMaterializerSource>(),
                Mock.Of<ClrCollectionAccessorSource>(),
                Mock.Of<ClrPropertySetterSource>(),
                _connection.Object,
                new AtsQueryFactory(new AtsValueReaderFactory()),
                new TableEntityAdapterFactory(),
                new ContextService<DbContext>(Mock.Of<DbContext>()),
                new LoggerFactory());
            return store;
        }
    }
}
