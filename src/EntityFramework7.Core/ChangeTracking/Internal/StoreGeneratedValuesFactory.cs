// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class StoreGeneratedValuesFactory : IStoreGeneratedValuesFactory
    {
        public virtual StoreGeneratedValues Create(InternalEntityEntry entry, IEnumerable<IProperty> properties)
            => new StoreGeneratedValues(entry, properties);
    }
}
