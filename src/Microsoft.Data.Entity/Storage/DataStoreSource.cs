// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStoreSource
    {
        public abstract DataStore GetStore([NotNull] ContextConfiguration configuration);
        public abstract DataStoreCreator GetCreator([NotNull] ContextConfiguration configuration);
        public abstract DataStoreConnection GetConnection([NotNull] ContextConfiguration configuration);
        public abstract bool IsAvailable([NotNull] ContextConfiguration configuration);
        public abstract bool IsConfigured([NotNull] ContextConfiguration configuration);
        public abstract string Name { get; }
    }
}
