// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.InMemory.Utilities;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryDataStoreSource
        : DataStoreSource<
            InMemoryDataStore,
            InMemoryOptionsExtension,
            InMemoryDataStoreCreator,
            InMemoryConnection,
            InMemoryValueGeneratorCache,
            Database>
    {
        public override bool IsAvailable(DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            return true;
        }

        public override string Name
        {
            get { return typeof(InMemoryDataStore).Name; }
        }
    }
}
