// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class CompositeEntityKeyFactory : EntityKeyFactory
    {
        public override EntityKey Create(IEntityType entityType, IReadOnlyList<IProperty> properties, IValueReader valueReader)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotNull(properties, "properties");
            Check.NotNull(valueReader, "valueReader");

            // TODO: Consider using strongly typed ReadValue instead of always object
            return Create(entityType, properties.Select(p => valueReader.ReadValue<object>(p.Index)).ToArray());
        }

        public override EntityKey Create(IEntityType entityType, IReadOnlyList<IProperty> properties, IPropertyBagEntry propertyBagEntry)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotNull(properties, "properties");
            Check.NotNull(propertyBagEntry, "propertyBagEntry");

            return Create(entityType, properties.Select(p => propertyBagEntry[p]).ToArray());
        }

        private static EntityKey Create(IEntityType entityType, object[] values)
        {
            return values.Any(v => v == null) ? EntityKey.NullEntityKey : new CompositeEntityKey(entityType, values);
        }
    }
}
