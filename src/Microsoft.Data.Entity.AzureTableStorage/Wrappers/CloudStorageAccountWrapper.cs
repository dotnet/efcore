// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.AzureTableStorage.Interfaces;
using Microsoft.WindowsAzure.Storage;

namespace Microsoft.Data.Entity.AzureTableStorage.Wrappers
{
    public class CloudStorageAccountWrapper : ICloudStorageAccount
    {
        private readonly CloudStorageAccount _account;

        public CloudStorageAccountWrapper(CloudStorageAccount account)
        {
            _account = account;
        }

        public CloudStorageAccountWrapper(string connectionString)
        {
            _account = CloudStorageAccount.Parse(connectionString);
        }

        public ICloudTableClient CreateCloudTableClient()
        {
            return new CloudTableClientWrapper(_account.CreateCloudTableClient());
        }
    }
}
