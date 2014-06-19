// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class SimpleEntityKeyFactory<TKey> : EntityKeyFactory
    {
        public override EntityKey Create(IEntityType entityType, IReadOnlyList<IProperty> properties, IValueReader valueReader)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotNull(properties, "properties");
            Check.NotNull(valueReader, "valueReader");

            return new SimpleEntityKey<TKey>(entityType, valueReader.ReadValue<TKey>(properties[0].Index));
        }

        public override EntityKey Create(IEntityType entityType, IReadOnlyList<IProperty> properties, IPropertyBagEntry propertyBagEntry)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotNull(properties, "properties");
            Check.NotNull(propertyBagEntry, "propertyBagEntry");

            return Create(entityType, propertyBagEntry[properties[0]]);
        }

        private static EntityKey Create(IEntityType entityType, object value)
        {
            return value == null ? EntityKey.NullEntityKey : new SimpleEntityKey<TKey>(entityType, (TKey)value);
        }
    }
}
