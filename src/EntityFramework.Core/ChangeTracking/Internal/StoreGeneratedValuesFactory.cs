// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class StoreGeneratedValuesFactory
    {
        public virtual StoreGeneratedValues Create([NotNull] InternalEntityEntry entry, [NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.NotNull(entry, "entry");
            Check.NotNull(properties, "properties");

            return new StoreGeneratedValues(entry, properties);
        }
    }
}
