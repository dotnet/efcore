// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Storage;
using System;
using System.Threading.Tasks;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    class AzureTableStorageDataStoreCreator : DataStoreCreator
    {
        private readonly AzureTableStorageConnection _connection;

        public AzureTableStorageDataStoreCreator(AzureTableStorageConnection connection)
        {
            _connection = connection;
        }

        public override void Create(Microsoft.Data.Entity.Metadata.IModel model)
        {
            foreach (var type in model.EntityTypes)
            {
                var table = _connection.GetTableReference(type.StorageName);
                table.CreateIfNotExists();
            }
        }

        public override async Task CreateAsync(Microsoft.Data.Entity.Metadata.IModel model, System.Threading.CancellationToken cancellationToken)
        {
            foreach (var type in model.EntityTypes)
            {
                var table = _connection.GetTableReference(type.StorageName);
                await table.CreateIfNotExistsAsync();
            }
        }

        public override void Delete()
        {
            throw new NotImplementedException();
        }

        public override Task DeleteAsync(System.Threading.CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override bool Exists()
        {
            throw new NotImplementedException();
        }

        public override Task<bool> ExistsAsync(System.Threading.CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
