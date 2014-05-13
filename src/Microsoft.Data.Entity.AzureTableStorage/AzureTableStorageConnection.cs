// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Linq;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    public class AzureTableStorageConnection : DataStoreConnection
    {
        private readonly string _connectionString;
        private readonly CloudStorageAccount _account;

        public AzureTableStorageConnection(DbContextConfiguration configuration)
        {
            var storeConfig = configuration
                .ContextOptions
                .Extensions
                .OfType<AzureTableStorageConfigurationExtension>()
                .Single();

            _connectionString = storeConfig.ConnectionString;

            _account = CloudStorageAccount.Parse(_connectionString);
        }

        public CloudStorageAccount Account
        {
            get { return _account; }
        }

        public CloudTable GetTableReference(string tableName)
        {
            return _account.CreateCloudTableClient().GetTableReference(tableName);
        }
    }
}
