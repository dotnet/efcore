// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.InMemory.Utilities;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;

namespace Microsoft.Data.Entity.InMemory
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
            [NotNull] DbContextConfiguration configuration,
            [NotNull] InMemoryDatabase persistentDatabase)
            : base(configuration)
        {
            Check.NotNull(configuration, "configuration");
            Check.NotNull(persistentDatabase, "persistentDatabase");

            var storeConfig = configuration.ContextOptions.Extensions
                .OfType<InMemoryConfigurationExtension>()
                .FirstOrDefault();

            _persist = (storeConfig != null ? (bool?)storeConfig.Persist : null) ?? true;

            _database = new ThreadSafeLazyRef<InMemoryDatabase>(
                () => _persist
                    ? persistentDatabase
                    : new InMemoryDatabase(configuration.LoggerFactory));
        }

        public virtual InMemoryDatabase Database
        {
            get { return _database.Value; }
        }

        public override Task<int> SaveChangesAsync(
            IReadOnlyList<StateEntry> stateEntries,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(stateEntries, "stateEntries");

            return Task.FromResult(_database.Value.ExecuteTransaction(stateEntries));
        }

        public override IEnumerable<TResult> Query<TResult>(QueryModel queryModel, StateManager stateManager)
        {
            Check.NotNull(queryModel, "queryModel");
            Check.NotNull(stateManager, "stateManager");

            var queryExecutor = new QueryModelVisitor().CreateQueryExecutor<TResult>(queryModel);
            var queryContext = new InMemoryQueryContext(Model, Logger, stateManager, _database.Value);

            return queryExecutor(queryContext);
        }

        public override IAsyncEnumerable<TResult> AsyncQuery<TResult>(QueryModel queryModel, StateManager stateManager)
        {
            return Query<TResult>(queryModel, stateManager).ToAsyncEnumerable();
        }
    }
}
