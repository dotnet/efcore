// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class StoreGeneratedValuesFactory
    {
        public virtual StoreGeneratedValues Create([NotNull] StateEntry stateEntry)
        {
            Check.NotNull(stateEntry, "stateEntry");

            var entityType = stateEntry.EntityType;

            return new StoreGeneratedValues(
                stateEntry,
                entityType.Properties.Where(stateEntry.NeedsStoreValue)
                    .Concat(entityType.ForeignKeys.SelectMany(f => f.Properties))
                    .Distinct()
                    .ToArray());
        }
    }
}
