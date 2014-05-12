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
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Relational.Utilities;
using Microsoft.Data.Entity.Storage;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Relational
{
    public abstract partial class RelationalDataStore : DataStore
    {
        private readonly DatabaseBuilder _databaseBuilder;
        private readonly CommandBatchPreparer _batchPreparer;
        private readonly BatchExecutor _batchExecutor;
        private readonly RelationalConnection _connection;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected RelationalDataStore()
        {
        }

        protected RelationalDataStore(
            [NotNull] DbContextConfiguration configuration,
            [NotNull] RelationalConnection connection,
            [NotNull] DatabaseBuilder databaseBuilder,
            [NotNull] CommandBatchPreparer batchPreparer,
            [NotNull] BatchExecutor batchExecutor)
            : base(configuration)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(databaseBuilder, "databaseBuilder");
            Check.NotNull(batchPreparer, "batchPreparer");
            Check.NotNull(batchExecutor, "batchExecutor");

            _databaseBuilder = databaseBuilder;
            _batchPreparer = batchPreparer;
            _batchExecutor = batchExecutor;
            _connection = connection;
        }

        protected virtual RelationalValueReaderFactory ValueReaderFactory
        {
            get { return new RelationalTypedValueReaderFactory(); }
        }

        public override async Task<int> SaveChangesAsync(
            IReadOnlyList<StateEntry> stateEntries,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(stateEntries, "stateEntries");

            var commands = _batchPreparer.BatchCommands(stateEntries);

            await _connection.OpenAsync(cancellationToken);
            try
            {
                await _batchExecutor.ExecuteAsync(commands, cancellationToken);
            }
            finally
            {
                _connection.Close();
            }

            // TODO Return the actual results once we can get them
            return stateEntries.Count();
        }

        public override IEnumerable<TResult> Query<TResult>(QueryModel queryModel, StateManager stateManager)
        {
            Check.NotNull(queryModel, "queryModel");
            Check.NotNull(stateManager, "stateManager");

            var queryExecutor = new QueryModelVisitor().CreateQueryExecutor<TResult>(queryModel);
            var queryContext = new RelationalQueryContext(Model, Logger, stateManager, _connection, ValueReaderFactory);

            return queryExecutor(queryContext);
        }

        public override IAsyncEnumerable<TResult> AsyncQuery<TResult>(QueryModel queryModel, StateManager stateManager)
        {
            Check.NotNull(queryModel, "queryModel");
            Check.NotNull(stateManager, "stateManager");

            var queryExecutor = new AsyncQueryModelVisitor().CreateQueryExecutor<TResult>(queryModel);
            var queryContext = new RelationalQueryContext(Model, Logger, stateManager, _connection, ValueReaderFactory);

            return queryExecutor(queryContext);
        }
    }
}
