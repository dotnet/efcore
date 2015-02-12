// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class CompositeEntityKeyFactory : EntityKeyFactory
    {
        private readonly IReadOnlyList<object> _sentinels;

        public CompositeEntityKeyFactory([NotNull] IReadOnlyList<object> sentinels)
        {
            Check.NotNull(sentinels, nameof(sentinels));

            _sentinels = sentinels;
        }

        public override EntityKey Create(
            IEntityType entityType, IReadOnlyList<IProperty> properties, IValueReader valueReader)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(properties, nameof(properties));
            Check.NotNull(valueReader, nameof(valueReader));

            var components = new object[properties.Count];

            for (var i = 0; i < properties.Count; i++)
            {
                var index = properties[i].Index;

                if (valueReader.IsNull(index))
                {
                    return EntityKey.InvalidEntityKey;
                }

                // TODO: Consider using strongly typed ReadValue instead of always object
                // See issue #736
                var value = valueReader.ReadValue<object>(index);

                if (Equals(value, _sentinels[i]))
                {
                    return EntityKey.InvalidEntityKey;
                }

                components[i] = value;
            }

            return new CompositeEntityKey(entityType, components);
        }

        public override EntityKey Create(
            IEntityType entityType, IReadOnlyList<IProperty> properties, IPropertyAccessor propertyBagEntry)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(properties, nameof(properties));
            Check.NotNull(propertyBagEntry, nameof(propertyBagEntry));

            var components = new object[properties.Count];

            for (var i = 0; i < properties.Count; i++)
            {
                var value = propertyBagEntry[properties[i]];

                if (value == null
                    || Equals(value, _sentinels[i]))
                {
                    return EntityKey.InvalidEntityKey;
                }

                components[i] = value;
            }

            return new CompositeEntityKey(entityType, components);
        }
    }
}
