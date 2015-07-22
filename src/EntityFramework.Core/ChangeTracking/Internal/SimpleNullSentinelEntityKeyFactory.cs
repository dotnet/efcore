// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class SimpleNullSentinelEntityKeyFactory<TKey> : EntityKeyFactory
    {
        public override EntityKey Create(
            IEntityType entityType, IReadOnlyList<IProperty> properties, ValueBuffer valueBuffer)
            => Create(entityType, valueBuffer[properties[0].Index]);

        public override EntityKey Create(
            IEntityType entityType, IReadOnlyList<IProperty> properties, IPropertyAccessor propertyAccessor)
            => Create(entityType, propertyAccessor[properties[0]]);

        private EntityKey Create(IEntityType entityType, object value)
        {
            return value != null
                ? new SimpleEntityKey<TKey>(entityType, (TKey)value)
                : EntityKey.InvalidEntityKey;
        }
    }
}
