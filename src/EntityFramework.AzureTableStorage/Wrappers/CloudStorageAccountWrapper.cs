// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Interfaces;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.WindowsAzure.Storage;

namespace Microsoft.Data.Entity.AzureTableStorage.Wrappers
{
    public class CloudStorageAccountWrapper : ICloudStorageAccount
    {
        private readonly CloudStorageAccount _account;
        /// <summary>
        /// For testing only. Usage may result in null reference exceptions.
        /// </summary>
        internal CloudStorageAccountWrapper() { }
        public CloudStorageAccountWrapper([NotNull] CloudStorageAccount account)
        {
            Check.NotNull(account, "account");
            _account = account;
        }

        public CloudStorageAccountWrapper([NotNull] string connectionString)
        {
            Check.NotNull(connectionString, "connectionString");
            _account = CloudStorageAccount.Parse(connectionString);
        }

        public virtual ICloudTableClient CreateCloudTableClient()
        {
            return new CloudTableClientWrapper(_account.CreateCloudTableClient());
        }
    }
}
