// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests.Helpers
{
    public class CloudTableFixture : IDisposable
    {
        private CloudTable _table;

        public CloudTableFixture()
        {
            _table = null;
            DeleteOnDispose = false;
        }

        public bool DeleteOnDispose { get; set; }

        public CloudTable GetOrCreateTable(string tableName, string connectionString)
        {
            if (_table == null)
            {
                var account = CloudStorageAccount.Parse(connectionString);
                var tableClient = account.CreateCloudTableClient();
                _table = tableClient.GetTableReference(tableName);
                _table.CreateIfNotExists();
            }
            return _table;
        }

        public void Dispose()
        {
            if (DeleteOnDispose && _table != null)
            {
                _table.DeleteIfExists();
            }
        }
    }
}
