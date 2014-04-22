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
        public const string NameKey = "Name";
        public const string ModeKey = "Mode";
        public const string PersistentMode = "Persistent";
        public const string TransientMode = "Transient";

        // TODO: Make this better
        private static ThreadSafeLazyRef<InMemoryDatabase> _persistentDatabase;

        private readonly ThreadSafeLazyRef<InMemoryDatabase> _database;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected InMemoryDataStore()
        {
        }

        public InMemoryDataStore([NotNull] ContextConfiguration configuration, [CanBeNull] ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            Check.NotNull(configuration, "configuration");

            var name = configuration.Annotations[typeof(InMemoryDataStore)][NameKey]
                       ?? configuration.Context.GetType().FullName;

            var persist = configuration.Annotations[typeof(InMemoryDataStore)][ModeKey] == PersistentMode;

            if (persist)
            {
                // TODO: Temporary hack due to scoping of store
                if (_persistentDatabase == null)
                {
                    _persistentDatabase = new ThreadSafeLazyRef<InMemoryDatabase>(() => new InMemoryDatabase(Logger));
                }
                _database = _persistentDatabase;
            }
            else
            {
                _database = new ThreadSafeLazyRef<InMemoryDatabase>(() => new InMemoryDatabase(Logger));
            }
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

            var queryModelVisitor = new QueryModelVisitor();
            var queryExecutor = queryModelVisitor.CreateQueryExecutor<TResult>(queryModel);
            var queryContext = new InMemoryQueryContext(model, stateManager, _database.Value);

            return new CompletedAsyncEnumerable<TResult>(queryExecutor(queryContext));
        }
    }
}
