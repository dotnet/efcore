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
using Microsoft.Data.Entity.InMemory.Query;
using Microsoft.Data.Entity.InMemory.Utilities;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Remotion.Linq;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryDataStore : DataStore
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
            [NotNull] InMemoryDatabase persistentDatabase,
            [NotNull] ILoggerFactory loggerFactory)
            : base(configuration, loggerFactory)
        {
            Check.NotNull(configuration, "configuration");
            Check.NotNull(persistentDatabase, "persistentDatabase");

            var storeConfig = configuration.ContextOptions.Extensions
                .OfType<InMemoryOptionsExtension>()
                .FirstOrDefault();

            _persist = (storeConfig != null ? (bool?)storeConfig.Persist : null) ?? true;

            _database = new ThreadSafeLazyRef<InMemoryDatabase>(
                () => _persist
                    ? persistentDatabase
                    : new InMemoryDatabase(loggerFactory));
        }

        public virtual InMemoryDatabase Database
        {
            get { return _database.Value; }
        }

        public override int SaveChanges(
            IReadOnlyList<StateEntry> stateEntries)
        {
            Check.NotNull(stateEntries, "stateEntries");

            return _database.Value.ExecuteTransaction(stateEntries);
        }

        public override Task<int> SaveChangesAsync(
            IReadOnlyList<StateEntry> stateEntries,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(stateEntries, "stateEntries");

            return Task.FromResult(_database.Value.ExecuteTransaction(stateEntries));
        }

        public override IEnumerable<TResult> Query<TResult>(QueryModel queryModel)
        {
            Check.NotNull(queryModel, "queryModel");

            var queryCompilationContext
                = new InMemoryQueryCompilationContext(
                    Model,
                    Logger,
                    EntityMaterializerSource,
                    EntityKeyFactorySource,
                    _database.Value);

            var queryExecutor = queryCompilationContext.CreateQueryModelVisitor().CreateQueryExecutor<TResult>(queryModel);
            var queryContext = new InMemoryQueryContext(Logger, CreateQueryBuffer());

            return queryExecutor(queryContext);
        }

        public override IAsyncEnumerable<TResult> AsyncQuery<TResult>(
            QueryModel queryModel, CancellationToken cancellationToken)
        {
            return Query<TResult>(queryModel).ToAsyncEnumerable();
        }

        public virtual bool EnsureDatabaseCreated([NotNull] IModel model)
        {
            return _database.Value.EnsureCreated(model);
        }
    }
}
