// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    public class AtsDataStoreCreator : DataStoreCreator
    {
        private readonly AtsConnection _connection;

        public AtsDataStoreCreator([NotNull] AtsConnection connection)
        {
            Check.NotNull(connection, "connection");
            _connection = connection;
        }

        public override bool EnsureDeleted([NotNull] IModel model)
        {
            Check.NotNull(model, "model");
            var deleted = false;
            foreach (var type in model.EntityTypes)
            {
                var table = _connection.GetTableReference(type.StorageName);
                if (table != null)
                {
                    deleted |= table.DeleteIfExists();
                }
            }
            return deleted;
        }

        public override async Task<bool> EnsureDeletedAsync([NotNull] IModel model, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(model, "model");
            var tasks = new List<Task<bool>>();
            foreach (var type in model.EntityTypes)
            {
                var table = _connection.GetTableReference(type.StorageName);
                if (table != null)
                {
                    tasks.Add(table.DeleteIfExistsAsync(cancellationToken));
                }
            }
            await Task.WhenAll(tasks);
            return tasks.Any(t => t.Result);
            ;
        }

        public override bool EnsureCreated([NotNull] IModel model)
        {
            Check.NotNull(model, "model");
            var created = false;
            foreach (var type in model.EntityTypes)
            {
                var table = _connection.GetTableReference(type.StorageName);
                if (table != null)
                {
                    created |= table.CreateIfNotExists();
                }
            }
            return created;
        }

        public override async Task<bool> EnsureCreatedAsync([NotNull] IModel model, CancellationToken cancellationToken = new CancellationToken())
        {
            Check.NotNull(model, "model");
            var tasks = new List<Task<bool>>();
            foreach (var type in model.EntityTypes)
            {
                var table = _connection.GetTableReference(type.StorageName);
                if (table != null)
                {
                    tasks.Add(table.CreateIfNotExistsAsync(cancellationToken));
                }
            }
            await Task.WhenAll(tasks);
            return tasks.Any(t => t.Result);
        }
    }
}
