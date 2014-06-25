// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Interfaces;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Wrappers
{
    public class CloudTableClientWrapper : ICloudTableClient
    {
        private readonly CloudTableClient _client;

        public CloudTableClientWrapper([NotNull] CloudTableClient client)
        {
            Check.NotNull(client, "client");
            _client = client;
        }

        public virtual IEnumerable<ICloudTable> ListTables()
        {
            return _client.ListTables().Select(s => new CloudTableWrapper(s));
        }

        public virtual ICloudTable GetTableReference([NotNull] string tableName)
        {
            Check.NotNull(tableName, "tableName");
            return new CloudTableWrapper(_client.GetTableReference(tableName));
        }
    }
}
