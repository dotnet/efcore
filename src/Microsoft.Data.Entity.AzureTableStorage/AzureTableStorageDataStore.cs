// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Remotion.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    public class AzureStorageDataStore : DataStore
    {
        private readonly AzureTableStorageConnection _connection;

        public AzureStorageDataStore(DbContextConfiguration configuration, AzureTableStorageConnection connection)
            : base(configuration)
        {
            _connection = connection;
        }

        public override IEnumerable<TResult> Query<TResult>(QueryModel queryModel, StateManager stateManager)
        {
            var queryExecutor = new AzureTableStorageQueryModelVisitor().CreateQueryExecutor<TResult>(queryModel);
            var queryContext = new AzureTableStorageQueryContext(Model, Logger, stateManager, _connection);

            return queryExecutor(queryContext);
        }

        public override IAsyncEnumerable<TResult> AsyncQuery<TResult>(QueryModel queryModel, StateManager stateManager)
        {
            // TODO This should happen properly async
            return Query<TResult>(queryModel, stateManager).ToAsyncEnumerable();
        }

        public override Task<int> SaveChangesAsync(IEnumerable<StateEntry> stateEntries, CancellationToken cancellationToken)
        {
            var typeGroups = stateEntries.GroupBy(e => e.EntityType);

            foreach (var typeGroup in typeGroups)
            {
                var table = _connection.GetTableReference(typeGroup.Key.StorageName);
                // TODO Use batches - was hanging for some reason
                var batch = new TableBatchOperation();

                // TODO Break up into batches of 100 (tried batching but it just seemed to hang)
                foreach (var entry in typeGroup)
                {
                    switch (entry.EntityState)
                    {
                        case EntityState.Added:
                            table.Execute(TableOperation.Insert((ITableEntity)entry.Entity));
                            break;

                        case EntityState.Deleted:
                            table.Execute(TableOperation.Delete((ITableEntity)entry.Entity));
                            break;

                        case EntityState.Modified:
                            table.Execute(TableOperation.Replace((ITableEntity)entry.Entity));
                            break;

                        default:
                            break;
                    }   
                }
            }

            // TODO Return affected rows 
            // TODO Maybe need to check result of operations? (not sure if you can fail without an exception)
            return Task.FromResult(stateEntries.Count());
        }
    }
}
