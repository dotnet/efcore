// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.InMemory.Utilities;
using Remotion.Linq;

namespace Microsoft.Data.InMemory
{
    public partial class InMemoryDataStore : DataStore
    {
        private readonly bool _persist;
        private readonly ThreadSafeLazyRef<InMemoryDatabase> _database;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected InMemoryDataStore()
        {
        }

        public InMemoryDataStore(
            [NotNull] ContextConfiguration configuration,
            [NotNull] InMemoryDatabase persistentDatabase,
            [CanBeNull] ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            Check.NotNull(configuration, "configuration");
            Check.NotNull(persistentDatabase, "persistentDatabase");

            var storeConfig = configuration.EntityConfiguration.Extensions()
                .OfType<InMemoryConfigurationExtension>()
                .FirstOrDefault();

            _persist = (storeConfig != null ? (bool?)storeConfig.Persist : null) ?? true;

            _database = new ThreadSafeLazyRef<InMemoryDatabase>(
                            () => _persist
                                ? persistentDatabase
                                : new InMemoryDatabase(loggerFactory));
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

            if (!_persist && !_database.HasValue)
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
