// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Storage;

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
