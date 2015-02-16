// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.ValueGeneration
{
    public abstract class ValueGenerator<TValue> : ValueGenerator
    {
        public new abstract TValue Next([NotNull] DbContextService<DataStoreServices> dataStoreServices);

        protected override object NextValue(DbContextService<DataStoreServices> dataStoreServices) => Next(dataStoreServices);
    }
}
