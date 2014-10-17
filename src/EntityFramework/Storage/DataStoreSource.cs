// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStoreSource
    {
        public abstract DataStoreServices StoreServices { get; }
        public abstract bool IsAvailable { get; }
        public abstract bool IsConfigured { get; }
        public abstract string Name { get; }
        public abstract DbContextOptions ContextOptions { get; }

        public virtual void AutoConfigure()
        {
        }
    }
}
