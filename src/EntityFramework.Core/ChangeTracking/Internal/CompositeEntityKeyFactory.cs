// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class CompositeEntityKeyFactory : EntityKeyFactory
    {
        private readonly IReadOnlyList<object> _sentinels;

        public CompositeEntityKeyFactory(
            [NotNull] IReadOnlyList<object> sentinels)
        {
            _sentinels = sentinels;
        }

        public override EntityKey Create(
            IEntityType entityType, IReadOnlyList<IProperty> properties, ValueBuffer valueBuffer)
            => Create(entityType, properties, p => valueBuffer[p.Index]);

        public override EntityKey Create(
            IEntityType entityType, IReadOnlyList<IProperty> properties, IPropertyAccessor propertyAccessor)
            => Create(entityType, properties, p => propertyAccessor[p]);

        private EntityKey Create(
            IEntityType entityType, IReadOnlyList<IProperty> properties, Func<IProperty, object> reader)
        {
            var components = new object[properties.Count];

            for (var i = 0; i < properties.Count; i++)
            {
                var value = reader(properties[i]);

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
