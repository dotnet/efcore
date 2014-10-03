// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.WindowsAzure.Storage.Table;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class AtsConnectionExtensions
    {
        public static void UseBatching([NotNull] this AtsConnection connection, bool value)
        {
            Check.NotNull(connection, "connection");

            connection.Batching = value;
        }

        public static void UseRequestOptions([NotNull] this AtsConnection connection, [NotNull] TableRequestOptions options)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(options, "options");

            connection.TableRequestOptions = options;
        }

        public static void ResetRequestOptions([NotNull] this AtsConnection connection)
        {
            Check.NotNull(connection, "connection");

            connection.TableRequestOptions = null;
        }
    }
}
