// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStoreSource
    {
        public abstract DataStore GetStore([NotNull] DbContextConfiguration configuration);
        public abstract Database GetDatabase([NotNull] DbContextConfiguration configuration);
        public abstract DataStoreCreator GetCreator([NotNull] DbContextConfiguration configuration);
        public abstract DataStoreConnection GetConnection([NotNull] DbContextConfiguration configuration);
        public abstract ValueGeneratorCache GetValueGeneratorCache([NotNull] DbContextConfiguration configuration);
        public abstract bool IsAvailable([NotNull] DbContextConfiguration configuration);
        public abstract bool IsConfigured([NotNull] DbContextConfiguration configuration);
        public abstract string Name { get; }
    }
}
