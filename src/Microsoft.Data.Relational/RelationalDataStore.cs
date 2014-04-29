// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Relational.Update;
using Microsoft.Data.Relational.Utilities;
using Remotion.Linq;

namespace Microsoft.Data.Relational
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
            [NotNull] ContextConfiguration configuration,
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
            IEnumerable<StateEntry> stateEntries,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(stateEntries, "stateEntries");

            var database = _databaseBuilder.GetDatabase(Model);
            var commands = _batchPreparer.BatchCommands(stateEntries, database);

            try
            {
                await _connection.OpenAsync(cancellationToken);
                await _batchExecutor.ExecuteAsync(commands, cancellationToken);
            }
            finally
            {
                _connection.Close();
            }

            // TODO Return the actual results once we can get them
            return stateEntries.Count();
        }

        public override IAsyncEnumerable<TResult> Query<TResult>(
            QueryModel queryModel, StateManager stateManager)
        {
            Check.NotNull(queryModel, "queryModel");
            Check.NotNull(stateManager, "stateManager");

            var queryModelVisitor = new QueryModelVisitor();
            var queryExecutor = queryModelVisitor.CreateQueryExecutor<TResult>(queryModel);
            var queryContext = new RelationalQueryContext(Model, Logger, stateManager, _connection, ValueReaderFactory);

            // TODO: Need async in query compiler
            return new CompletedAsyncEnumerable<TResult>(queryExecutor(queryContext));
        }
    }
}
