// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Interfaces;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.AzureTableStorage.Wrappers;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    public class AtsConnection : DataStoreConnection
    {
        private readonly string _connectionString;
        private readonly CloudStorageAccountWrapper _account;

        /// <summary>
        ///     For testing
        /// </summary>
        protected AtsConnection()
        {
        }

        public AtsConnection([NotNull] DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            var storeConfig = configuration
                .ContextOptions
                .Extensions
                .OfType<AtsOptionsExtension>()
                .Single();

            _connectionString = storeConfig.ConnectionString;

            _account = new CloudStorageAccountWrapper(_connectionString);
        }

        public virtual CloudStorageAccountWrapper Account
        {
            get { return _account; }
        }

        public virtual ICloudTable GetTableReference([NotNull] string tableName)
        {
            Check.NotNull(tableName, "tableName");
            return _account.CreateCloudTableClient().GetTableReference(tableName);
        }
    }
}
