// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Services;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.InMemory.Utilities;
using Remotion.Linq;

namespace Microsoft.Data.InMemory
{
    public partial class InMemoryDataStore : DataStore
    {
        private readonly ThreadSafeLazyRef<InMemoryDatabase> _database;

        public InMemoryDataStore()
            : this(NullLogger.Instance)
        {
        }

        public InMemoryDataStore([NotNull] ILoggerFactory loggerFactory)
            : this(Check.NotNull(loggerFactory, "loggerFactory").Create(typeof(InMemoryDataStore).Name))
        {
        }

        public InMemoryDataStore([NotNull] ILogger logger)
        {
            Check.NotNull(logger, "logger");

            _database
                = new ThreadSafeLazyRef<InMemoryDatabase>(() => new InMemoryDatabase(logger));
        }

        internal InMemoryDatabase Database
        {
            get { return _database.Value; }
        }

        public override Task<int> SaveChangesAsync(
            IEnumerable<StateEntry> stateEntries,
            IModel model,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(stateEntries, "stateEntries");
            Check.NotNull(model, "model");

            return Task.FromResult(_database.Value.ExecuteTransaction(stateEntries));
        }

        public override IAsyncEnumerable<TResult> Query<TResult>(
            QueryModel queryModel, IModel model, StateManager stateManager)
        {
            Check.NotNull(queryModel, "queryModel");
            Check.NotNull(model, "model");
            Check.NotNull(stateManager, "stateManager");

            if (!_database.HasValue)
            {
                return new CompletedAsyncEnumerable<TResult>(Enumerable.Empty<TResult>());
            }

            var queryModelVisitor = new QueryModelVisitor();
            var queryExecutor = queryModelVisitor.CreateQueryExecutor<TResult>(queryModel);
            var queryContext = new InMemoryQueryContext(model, stateManager, _database.Value);

            return new CompletedAsyncEnumerable<TResult>(queryExecutor(queryContext));
        }
    }
}
