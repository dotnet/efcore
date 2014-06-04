// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class SimpleNullableEntityKeyFactory<TKey, TNullableKey> : SimpleEntityKeyFactory<TKey>
    {
        public override EntityKey Create(IEntityType entityType, IReadOnlyList<IProperty> properties, IValueReader valueReader)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotNull(properties, "properties");
            Check.NotNull(valueReader, "valueReader");

            var value = (object)valueReader.ReadValue<TNullableKey>(properties[0].Index);

            return value == null ? null : new SimpleEntityKey<TKey>(entityType, (TKey)value);
        }
    }
}
