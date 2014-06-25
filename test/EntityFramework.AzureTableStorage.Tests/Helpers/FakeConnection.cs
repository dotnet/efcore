// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.AzureTableStorage.Interfaces;
using Microsoft.Data.Entity.AzureTableStorage.Query;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers
{
    public class FakeConnection : AtsConnection
    {
        public ConcurrentDictionary<string, TestCloudTable> Tables = new ConcurrentDictionary<string, TestCloudTable>();
        private readonly Queue<ITableResult> _queue = new Queue<ITableResult>();

        public override ICloudTable GetTableReference(string tableName)
        {
            TestCloudTable table;
            if (Tables.TryGetValue(tableName, out table))
            {
                return table;
            }
            return new TestCloudTable(this, tableName);
        }

        public void QueueResult(ITableResult nextResult)
        {
            _queue.Enqueue(nextResult);
        }

        public int CreateTableRequests { get; private set; }

        public void ClearQueue()
        {
            Tables = new ConcurrentDictionary<string, TestCloudTable>();
        }

        public class TestCloudTable : ICloudTable
        {
            public TestCloudTable(FakeConnection context, string name)
            {
                _context = context;
                Name = name;
            }

            public string Name { get; private set; }

            private readonly FakeConnection _context;

            public Task<ITableResult> ExecuteAsync(TableOperation operation, CancellationToken cancellationToken = new CancellationToken())
            {
                return Task.FromResult(_context._queue.Dequeue());
            }

            public Task<IList<ITableResult>> ExecuteBatchAsync(TableBatchOperation batch, CancellationToken cancellationToken = new CancellationToken())
            {
                return Task.FromResult<IList<ITableResult>>(_context._queue.Take(batch.Count).ToList());
            }

            public bool CreateIfNotExists()
            {
                if (_context.Tables.ContainsKey(Name))
                {
                    return false;
                }
                _context.Tables.GetOrAdd(Name, this);
                return true;
            }

            public Task<bool> CreateIfNotExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.Run(() => CreateIfNotExists(), cancellationToken);
            }

            public Task<bool> DeleteIfExistsAsync(CancellationToken cancellationToken = new CancellationToken())
            {
                return Task.Run(() => DeleteIfExists(), cancellationToken);
            }

            public bool DeleteIfExists()
            {
                if (_context.Tables.ContainsKey(Name))
                {
                    TestCloudTable value;
                    _context.Tables.TryRemove(Name, out value);
                    return true;
                }
                return false;
            }

            public IEnumerable<TElement> ExecuteQuery<TElement>(AtsTableQuery query, Func<AtsNamedValueBuffer, TElement> resolver) where TElement : class
            {
                throw new NotImplementedException();
            }
        }
    }
}
