// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ValueGeneration
{
    public abstract class ValueGenerator
    {
        public virtual object Next([NotNull] DbContextService<DataStoreServices> dataStoreServices)
        {
            Check.NotNull(dataStoreServices, nameof(dataStoreServices));

            return NextValue(dataStoreServices);
        }

        protected abstract object NextValue([NotNull] DbContextService<DataStoreServices> dataStoreServices);

        public abstract bool GeneratesTemporaryValues { get; }
    }
}
