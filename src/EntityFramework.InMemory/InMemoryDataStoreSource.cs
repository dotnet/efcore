// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryDataStoreSource : DataStoreSource<IInMemoryDataStoreServices, InMemoryOptionsExtension>
    {
        public InMemoryDataStoreSource([NotNull] DbContextServices services, [NotNull] IDbContextOptions options)
            : base(services, options)
        {
        }

        public override bool IsAvailable => true;

        public override string Name => typeof(InMemoryDataStore).Name;
    }
}
