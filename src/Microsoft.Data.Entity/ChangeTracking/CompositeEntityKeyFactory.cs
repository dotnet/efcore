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
        public override EntityKey Create(IEntityType entityType, IReadOnlyList<IProperty> properties, StateEntry entry)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotNull(properties, "properties");
            Check.NotNull(entry, "entry");

            return Create(entityType, properties.Select(p => entry[p]).ToArray());
        }

        public override EntityKey Create(IEntityType entityType, IReadOnlyList<IProperty> properties, IValueReader valueReader)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotNull(properties, "properties");
            Check.NotNull(valueReader, "valueReader");

            // TODO: Consider using strongly typed ReadValue instead of always object
            return Create(entityType, properties.Select(p => valueReader.ReadValue<object>(p.Index)).ToArray());
        }

        public override EntityKey Create(IEntityType entityType, IReadOnlyList<IProperty> properties, Sidecar sidecar)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotNull(properties, "properties");
            Check.NotNull(sidecar, "sidecar");

            return Create(entityType, properties.Select(p => sidecar[p]).ToArray());
        }

        private static CompositeEntityKey Create(IEntityType entityType, object[] values)
        {
            return values.Any(v => v == null) ? null : new CompositeEntityKey(entityType, values);
        }
    }
}
