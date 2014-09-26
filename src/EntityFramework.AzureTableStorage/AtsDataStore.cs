// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Adapters;
using Microsoft.Data.Entity.AzureTableStorage.Metadata;
using Microsoft.Data.Entity.AzureTableStorage.Query;
using Microsoft.Data.Entity.AzureTableStorage.Requests;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Remotion.Linq;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    public class AtsDataStore : DataStore
    {
        private readonly AtsQueryFactory _queryFactory;
        private readonly DbContextConfiguration _configuration;
        protected readonly AtsConnection Connection;
        internal TableEntityAdapterFactory EntityFactory;
        private const int MaxBatchOperations = 100;

        /// <summary>
        ///     Provided only for testing purposes. Do not use.
        /// </summary>
        protected AtsDataStore(DbContextConfiguration configuration, AtsConnection connection, TableEntityAdapterFactory entityFactory)
        {
            _configuration = configuration;
            Connection = connection;
            EntityFactory = entityFactory;
        }

        public AtsDataStore([NotNull] DbContextConfiguration configuration,
            [NotNull] AtsConnection connection,
            [NotNull] AtsQueryFactory queryFactory,
            [NotNull] TableEntityAdapterFactory tableEntityFactory)
            : base(configuration)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(queryFactory, "queryFactory");
            Check.NotNull(configuration, "configuration");
            Check.NotNull(tableEntityFactory, "tableEntityFactory");

            _queryFactory = queryFactory;
            _configuration = configuration;
            EntityFactory = tableEntityFactory;
            Connection = connection;
        }

        public override int SaveChanges(IReadOnlyList<StateEntry> stateEntries)
        {
            Check.NotNull(stateEntries, "stateEntries");

            if (Connection.Batching)
            {
                return ExecuteBatchedChanges(stateEntries);
            }
            return ExecuteChanges(stateEntries);
        }

        public override Task<int> SaveChangesAsync(IReadOnlyList<StateEntry> stateEntries, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(stateEntries, "stateEntries");

            cancellationToken.ThrowIfCancellationRequested();
            if (Connection.Batching)
            {
                return ExecuteBatchedChangesAsync(stateEntries, cancellationToken);
            }
            return ExecuteChangesAsync(stateEntries, cancellationToken);
        }

        public override IEnumerable<TResult> Query<TResult>(
            QueryModel queryModel)
        {
            Check.NotNull(queryModel, "queryModel");

            var compilationContext = _queryFactory.MakeCompilationContext(Model);
            var queryExecutor = compilationContext.CreateQueryModelVisitor().CreateQueryExecutor<TResult>(queryModel);

            var queryContext
                = _queryFactory.MakeQueryContext(
                    Model,
                    Logger,
                    CreateQueryBuffer(),
                    StateManager,
                    Connection);

            return queryExecutor(queryContext);
        }

        public override IAsyncEnumerable<TResult> AsyncQuery<TResult>(QueryModel queryModel, CancellationToken cancellationToken)
        {
            Check.NotNull(queryModel, "queryModel");

            // TODO This should happen properly async
            return Query<TResult>(queryModel).ToAsyncEnumerable();
        }

        //TODO merge similarities with batch execution
        private int ExecuteChanges(IReadOnlyList<StateEntry> stateEntries)
        {
            var tableGroups = stateEntries.GroupBy(s => s.EntityType);
            var allResults = new List<TableResult>();

            try
            {
                foreach (var tableGroup in tableGroups)
                {
                    var table = new AtsTable(tableGroup.Key.TableName());
                    var results = tableGroup.Select(entry => CreateRequest(table, entry))
                        .Select(request => Connection.ExecuteRequest(request, Logger));
                    allResults.AddRange(results);
                }
            }
            catch (StorageException exception)
            {
                var handled = HandleStorageException(exception, stateEntries);
                if (handled != null)
                {
                    throw handled;
                }
                throw;
            }

            return allResults.Count(r => r.HttpStatusCode < (int)HttpStatusCode.BadRequest);
        }

        private async Task<int> ExecuteChangesAsync(IReadOnlyList<StateEntry> stateEntries,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var tableGroups = stateEntries.GroupBy(s => s.EntityType);
            var allTasks = new List<Task<TableResult>>();
            TableResult[] results;

            try
            {
                foreach (var tableGroup in tableGroups)
                {
                    var table = new AtsTable(tableGroup.Key.TableName());
                    var tasks = tableGroup.Select(entry => CreateRequest(table, entry))
                        .TakeWhile(operation => !cancellationToken.IsCancellationRequested)
                        .Select(request => Connection.ExecuteRequestAsync(request, Logger, cancellationToken));
                    allTasks.AddRange(tasks);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
                results = await Task.WhenAll(allTasks).WithCurrentCulture();
                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (StorageException exception)
            {
                var handled = HandleStorageException(exception, stateEntries);
                if (handled != null)
                {
                    throw handled;
                }
                throw;
            }

            return results.Count(r => r.HttpStatusCode < (int)HttpStatusCode.BadRequest);
        }

        private int ExecuteBatchedChanges(IReadOnlyList<StateEntry> stateEntries)
        {
            var tableGroups = stateEntries.GroupBy(s => s.EntityType.TableName());
            var results = new List<IList<TableResult>>();

            try
            {
                foreach (var tableGroup in tableGroups)
                {
                    var table = new AtsTable(tableGroup.Key);
                    var partitionGroups = tableGroup.GroupBy(s =>
                        {
                            var property = s.EntityType.GetPropertyByColumnName("PartitionKey");
                            return s[property];
                        }
                        );
                    foreach (var partitionGroup in partitionGroups)
                    {
                        var request = new TableBatchRequest(table);
                        foreach (var operation in partitionGroup
                            .Select(entry => CreateRequest(table, entry))
                            .Where(operation => operation != null)
                            )
                        {
                            request.Add(operation);
                            if (request.Count >= MaxBatchOperations)
                            {
                                results.Add(Connection.ExecuteRequest(request, Logger));
                                request = new TableBatchRequest(table);
                            }
                        }
                        if (request.Count != 0)
                        {
                            results.Add(Connection.ExecuteRequest(request, Logger));
                        }
                    }
                }
            }
            catch (StorageException exception)
            {
                var handled = HandleStorageException(exception, stateEntries);
                if (handled != null)
                {
                    throw handled;
                }
                throw;
            }
            return results.Sum(r => r == null ? 0 : r.Count(t => t.HttpStatusCode < (int)HttpStatusCode.BadRequest));
        }

        private async Task<int> ExecuteBatchedChangesAsync(IReadOnlyList<StateEntry> stateEntries,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var tableGroups = stateEntries.GroupBy(s => s.EntityType.TableName());
            var allBatchTasks = new List<Task<IList<TableResult>>>();
            IList<TableResult>[] results;

            try
            {
                foreach (var tableGroup in tableGroups)
                {
                    var table = new AtsTable(tableGroup.Key);
                    var partitionGroups = tableGroup.GroupBy(s =>
                        {
                            var property = s.EntityType.GetPropertyByColumnName("PartitionKey");
                            return s[property];
                        }
                        );
                    foreach (var partitionGroup in partitionGroups)
                    {
                        var request = new TableBatchRequest(table);
                        foreach (var operation in partitionGroup
                            .Select(entry => CreateRequest(table, entry))
                            .Where(operation => operation != null)
                            )
                        {
                            request.Add(operation);
                            if (request.Count >= MaxBatchOperations)
                            {
                                allBatchTasks.Add(Connection.ExecuteRequestAsync(request, Logger, cancellationToken));
                                request = new TableBatchRequest(table);
                            }
                        }
                        if (request.Count != 0)
                        {
                            allBatchTasks.Add(Connection.ExecuteRequestAsync(request, Logger, cancellationToken));
                        }
                    }
                }

                results = await Task.WhenAll(allBatchTasks).WithCurrentCulture();
            }
            catch (StorageException exception)
            {
                var handled = HandleStorageException(exception, stateEntries);
                if (handled != null)
                {
                    throw handled;
                }
                throw;
            }
            return results.Sum(r => r == null ? 0 : r.Count(t => t.HttpStatusCode < (int)HttpStatusCode.BadRequest));
        }

        protected Exception HandleStorageException(StorageException exception, IReadOnlyList<StateEntry> stateEntries)
        {
            var statusCode = exception.RequestInformation.HttpStatusCode;
            if (statusCode == (int)HttpStatusCode.PreconditionFailed)
            {
                return new DbUpdateConcurrencyException(Strings.ETagPreconditionFailed, _configuration.Context, stateEntries);
            }
            if (statusCode == (int)HttpStatusCode.NotFound)
            {
                var extendedErrorCode = exception.RequestInformation.ExtendedErrorInformation.ErrorCode;
                if (extendedErrorCode == StorageErrorCodes.ResourceNotFound)
                {
                    return new DbUpdateConcurrencyException(Strings.ResourceNotFound, _configuration.Context, stateEntries);
                }
                if (extendedErrorCode == StorageErrorCodes.TableNotFoundError)
                {
                    return new DbUpdateException(Strings.TableNotFound, _configuration.Context, stateEntries);
                }
            }
            return new DbUpdateException(Strings.SaveChangesFailed, _configuration.Context, exception, stateEntries);
        }

        protected TableOperationRequest CreateRequest(AtsTable table, StateEntry entry)
        {
            var entity = EntityFactory.CreateFromStateEntry(entry);
            switch (entry.EntityState)
            {
                case EntityState.Added:
                    return new CreateRowRequest(table, entity);

                case EntityState.Deleted:
                    return new DeleteRowRequest(table, entity);

                case EntityState.Modified:
                    return new MergeRowRequest(table, entity);

                case EntityState.Unchanged:
                case EntityState.Unknown:
                    return null;

                default:
                    throw new ArgumentOutOfRangeException("entry", "Unknown entity state");
            }
        }
    }
}
