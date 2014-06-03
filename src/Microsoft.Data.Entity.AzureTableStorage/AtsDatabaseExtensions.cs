// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    public static class AtsDatabaseExtensions
    {
        public static AtsDatabase AsAzureTableStorageDatabase([NotNull] this Database database)
        {
            Check.NotNull(database, "database");
            var atsdb = database as AtsDatabase;
            if (atsdb == null)
            {
                throw new InvalidOperationException(Strings.AtsDatabaseNotInUse);
            }
            return atsdb;
        } 
    }
}