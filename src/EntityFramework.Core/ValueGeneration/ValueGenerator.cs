// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.ValueGeneration
{
    public abstract class ValueGenerator
    {
        public abstract object Next(
            [NotNull] IProperty property, 
            [NotNull] DbContextService<DataStoreServices> dataStoreServices);

        public abstract bool GeneratesTemporaryValues { get; }
    }
}
