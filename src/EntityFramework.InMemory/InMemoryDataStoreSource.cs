// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryDataStoreSource : DataStoreSource<InMemoryDataStoreServices, InMemoryOptionsExtension>
    {
        public InMemoryDataStoreSource([NotNull] DbContextConfiguration configuration)
            : base(configuration)
        {
        }

        public override bool IsAvailable
        {
            get { return true; }
        }

        public override string Name
        {
            get { return typeof(InMemoryDataStore).Name; }
        }
    }
}
