// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Entity.AzureTableStorage.Adapters;
using Microsoft.Data.Entity.AzureTableStorage.Interfaces;
using Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers;
using Microsoft.Data.Entity.ChangeTracking;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests
{
    using ResultTaskList = IList<ITableResult>;

    public class AtsBatchedDataStoreTests : AtsDataStore, IClassFixture<FakeConnection>
    {
        private readonly FakeConnection _fakeConnection;

        private readonly TableEntityAdapterFactory _entityFactory = new TableEntityAdapterFactory();

        public AtsBatchedDataStoreTests(FakeConnection connection)
            : base(connection, new TableEntityAdapterFactory())
        {
            _fakeConnection = connection;
            _fakeConnection.Batching = true;
            _fakeConnection.ClearQueue();
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
        public void It_counts_batch_results()
        {
            var results = SetupResults<ResultTaskList>(new[] { new[] { TestTableResult.OK(), TestTableResult.OK() }, new[] { TestTableResult.OK(), TestTableResult.OK() } });
            var succeeded = InspectBatchResults(results);
            Assert.Equal(4, succeeded);
        }

        [Fact]
        public void It_fails_bad_batch_results()
        {
            var results = SetupResults<ResultTaskList>(new[] { new[] { TestTableResult.OK(), TestTableResult.OK() }, new[] { TestTableResult.OK(), TestTableResult.BadRequest(), TestTableResult.OK() } });
            Assert.Throws<DbUpdateException>(() => InspectBatchResults(results));
        }

        [Fact]
        public void It_throws_batch_exception()
        {
            var exceptedBatch = new TaskCompletionSource<ResultTaskList>();
            exceptedBatch.SetException(new AggregateException());
            Assert.Throws<AggregateException>(() => InspectBatchResults(new[] { exceptedBatch.Task }));
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
            var testEntries = new List<StateEntry>();
            for (var i = 0; i < expectedChanges; i++)
            {
                const string title = "TestType";
                _fakeConnection.QueueResult(TestTableResult.OK());
                testEntries.Add(TestStateEntry.Mock().WithState(EntityState.Added).WithType(title).WithProperty("PartitionKey", "A"));
            }
            var actualChanges = SaveChangesAsync(testEntries).Result;
            Assert.Equal(expectedChanges, actualChanges);
        }
    }
}
