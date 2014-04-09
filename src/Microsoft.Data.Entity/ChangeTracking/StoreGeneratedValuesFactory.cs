// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class StoreGeneratedValuesFactory
    {
        public virtual StoreGeneratedValues Create([NotNull] StateEntry stateEntry, [NotNull] IEnumerable<IProperty> properties)
        {
            Check.NotNull(stateEntry, "stateEntry");

            return new StoreGeneratedValues(stateEntry, properties);
        }
    }
}
