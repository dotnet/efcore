// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Interfaces;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Remotion.Linq;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    public class AzureTableStorageDataStore : DataStore
    {
        protected readonly AzureTableStorageConnection Connection;

        /// <summary>
        ///     Provided only for testing purposes. Do not use.
        /// </summary>
        protected AzureTableStorageDataStore(AzureTableStorageConnection connection)
        {
            Connection = connection;
        }

        public AzureTableStorageDataStore([NotNull] DbContextConfiguration configuration, [NotNull] AzureTableStorageConnection connection)
            : base(configuration)
        {
            Check.NotNull(connection, "connection");
            Connection = connection;
        }

        public override IEnumerable<TResult> Query<TResult>(QueryModel queryModel, StateManager stateManager)
        {
            Check.NotNull(queryModel, "queryModel");
            Check.NotNull(stateManager, "stateManager");

            var queryExecutor = new AzureTableStorageQueryModelVisitor().CreateQueryExecutor<TResult>(queryModel);
            var queryContext = new AzureTableStorageQueryContext(Model, Logger, stateManager, Connection);

            return queryExecutor(queryContext);
        }

        public override IAsyncEnumerable<TResult> AsyncQuery<TResult>(QueryModel queryModel, StateManager stateManager)
        {
            Check.NotNull(queryModel, "queryModel");
            Check.NotNull(stateManager, "stateManager");

            // TODO This should happen properly async
            return Query<TResult>(queryModel, stateManager).ToAsyncEnumerable();
        }

        public override async Task<int> SaveChangesAsync(IReadOnlyList<StateEntry> stateEntries, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(stateEntries, "stateEntries");

            cancellationToken.ThrowIfCancellationRequested();
            var tableGroups = stateEntries.GroupBy(s => s.EntityType);
            var allTasks = new List<Task<ITableResult>>();
            foreach (var tableGroup in tableGroups)
            {
                var table = Connection.GetTableReference(tableGroup.Key.StorageName);
                var tasks = tableGroup.Select(GetOperation)
                    .TakeWhile(operation => !cancellationToken.IsCancellationRequested)
                    .Select(operation => table.ExecuteAsync(operation, cancellationToken));
                allTasks.AddRange(tasks);

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
            await Task.WhenAll(allTasks);
            return InspectResults(allTasks);
        }

        protected int InspectResults(IList<Task<ITableResult>> tasks)
        {
            return CountTableResults(tasks, task =>
                {
                    if (task.Result.HttpStatusCode >= HttpStatusCode.BadRequest)
                    {
                        throw new DbUpdateException("Could not add entity: " + task.Result);
                    }
                    return 1;
                });
        }

        protected int CountTableResults<TTask>(IList<Task<TTask>> tasks, Func<Task<TTask>, int> inspect)
        {
            var failedTask = tasks.FirstOrDefault(t => t.Exception != null);
            if (failedTask != null
                && failedTask.Exception != null)
            {
                throw failedTask.Exception;
            }
            //TODO identify failed tasks and their associated identity: return to user.
            return tasks.Aggregate(0, (current, task) => current + inspect(task));
        }

        protected TableOperation GetOperation(StateEntry entry)
        {
            var entity = (ITableEntity)entry.Entity;

            switch (entry.EntityState)
            {
                case EntityState.Added:
                    return TableOperation.Insert(entity);

                case EntityState.Deleted:
                    entity.ETag = entity.ETag ?? "*";
                    return TableOperation.Delete(entity);

                case EntityState.Modified:
                    return TableOperation.Replace(entity);

                case EntityState.Unchanged:
                case EntityState.Unknown:
                    return null;

                default:
                    throw new ArgumentOutOfRangeException("entry", "Unknown entity state");
            }
        }
    }
}
