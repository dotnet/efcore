// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ValueGeneration
{
    public abstract class SimpleValueGenerator : ValueGenerator
    {
        public abstract object Next([NotNull] IProperty property);

        public override object Next(IProperty property, DbContextService<DataStoreServices> dataStoreServices)
        {
            Check.NotNull(property, nameof(property));

            return Next(property);
        }
    }
}
