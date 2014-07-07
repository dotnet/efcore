// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Storage;
using Microsoft.WindowsAzure.Storage.Table;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class DataStoreConnectionExtensions
    {
        public static void UseBatching([NotNull] this DataStoreConnection connection, bool value)
        {
            Check.NotNull(connection, "connection");

            connection.AsAtsConnection().Batching = value;
        }

        public static void UseRequestOptions([NotNull] this DataStoreConnection connection, [NotNull] TableRequestOptions options)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(options, "options");

            connection.AsAtsConnection().TableRequestOptions = options;
        }

        public static void ResetRequestOptions([NotNull] this DataStoreConnection connection)
        {
            Check.NotNull(connection, "connection");

            connection.AsAtsConnection().TableRequestOptions = null;
        }

        public static AtsConnection AsAtsConnection([NotNull] this DataStoreConnection connection)
        {
            Check.NotNull(connection, "connection");

            var atsConnection = connection as AtsConnection;
            if (atsConnection == null)
            {
                throw new InvalidOperationException(Strings.AtsConnectionNotInUse);
            }
            return atsConnection;
        }
    }
}
