// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    public class AzureTableStorageDataStoreCreator : DataStoreCreator
    {
        private readonly AzureTableStorageConnection _connection;

        public AzureTableStorageDataStoreCreator([NotNull] AzureTableStorageConnection connection)
        {
            Check.NotNull(connection, "connection");
            _connection = connection;
        }

        public override void CreateTables([NotNull] IModel model)
        {
            Check.NotNull(model, "model");
            foreach (var type in model.EntityTypes)
            {
                var table = _connection.GetTableReference(type.StorageName);
                if (table != null)
                {
                    table.CreateIfNotExists();
                }
            }
        }

        public override async Task CreateTablesAsync([NotNull] IModel model, CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var type in model.EntityTypes)
            {
                var table = _connection.GetTableReference(type.StorageName);
                if (table != null)
                {
                    await table.CreateIfNotExistsAsync(cancellationToken);
                }
            }
        }

        public override bool HasTables()
        {
            return _connection.Account.CreateCloudTableClient().ListTables().Any();
        }

        public override Task<bool> HasTablesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.Factory.StartNew(()=>HasTables(), cancellationToken);
        }

        public override void Create()
        {
            throw new AzureAccountException(Strings.CannotModifyAccount);
        }

        public override Task CreateAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            throw new AzureAccountException(Strings.CannotModifyAccount);
        }

        public override void Delete()
        {
            throw new AzureAccountException(Strings.CannotModifyAccount);
        }

        public override Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new AzureAccountException(Strings.CannotModifyAccount);
        }

        public override bool Exists()
        {
            //TODO implement check if account credentials are correct && server is available
            throw new NotImplementedException();
        }

        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // TODO implement as async
            throw new NotImplementedException();
        }
    }
}
