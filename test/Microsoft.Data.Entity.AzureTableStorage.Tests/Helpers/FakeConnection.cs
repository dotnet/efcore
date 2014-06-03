// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.AzureTableStorage.Interfaces;
using Microsoft.Data.Entity.AzureTableStorage.Query;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests
{
    public class FakeConnection : AzureTableStorageConnection
    {
        private Dictionary<string, TestCloudTable> _tables = new Dictionary<string, TestCloudTable>();

        public override ICloudTable GetTableReference(string tableName)
        {
            _tables[tableName] = _tables.ContainsKey(tableName) ? _tables[tableName] : new TestCloudTable(this);
            return _tables[tableName];
        }

        public void QueueResult(string tableName, ITableResult nextResult)
        {
            ((TestCloudTable)GetTableReference(tableName)).Queue.Enqueue(nextResult);
        }

        public int CreateTableRequests { get; private set; }

        public void ClearQueue()
        {
            _tables = new Dictionary<string, TestCloudTable>();
        }

        private class TestCloudTable : ICloudTable
        {
            public TestCloudTable(FakeConnection context)
            {
                _context = context;
            }

            public readonly Queue<ITableResult> Queue = new Queue<ITableResult>();
            private readonly FakeConnection _context;

            public Task<ITableResult> ExecuteAsync(TableOperation operation, CancellationToken cancellationToken = new CancellationToken())
            {
                return Task.FromResult<ITableResult>(Queue.Dequeue());
            }

            public Task<IList<ITableResult>> ExecuteBatchAsync(TableBatchOperation batch, CancellationToken cancellationToken = new CancellationToken())
            {
                return Task.FromResult<IList<ITableResult>>(Queue.Take(batch.Count).ToList());
            }

            public void CreateIfNotExists()
            {
                _context.CreateTableRequests++;
            }

            public Task CreateIfNotExistsAsync(CancellationToken cancellationToken = new CancellationToken())
            {
                _context.CreateTableRequests++;
                return Task.Factory.StartNew(() => true);
            }

            public IEnumerable<TElement> ExecuteQuery<TElement>(AtsTableQuery query, Func<AtsNamedValueBuffer, TElement> resolver) where TElement : class
            {
                throw new NotImplementedException();
            }

            public void DeleteIfExists()
            {
                throw new NotImplementedException();
            }
        }
    }
}
