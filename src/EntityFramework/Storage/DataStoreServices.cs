// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStoreServices
    {
        public abstract DataStore Store { get; }
        public abstract DataStoreCreator Creator { get; }
        public abstract DataStoreConnection Connection { get; }
        public abstract ValueGeneratorCache ValueGeneratorCache { get; }
        public abstract Database Database { get; }
        public abstract IModelBuilderFactory ModelBuilderFactory { get; }
    }
}
