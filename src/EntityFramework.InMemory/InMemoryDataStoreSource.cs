// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryDataStoreSource : DataStoreSource<InMemoryDataStore, IInMemoryDataStoreServices, InMemoryOptionsExtension>
    {
        public override void AutoConfigure(DbContextOptionsBuilder optionsBuilder)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            optionsBuilder.UseInMemoryStore();
        }
    }
}
