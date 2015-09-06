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
        public CompositeEntityKeyFactory([NotNull] IKey key)
            : base(key)
        {
        }

        public override EntityKey Create(IReadOnlyList<IProperty> properties, ValueBuffer valueBuffer)
            => Create(properties, p => valueBuffer[p.Index]);

        public override EntityKey Create(IReadOnlyList<IProperty> properties, IPropertyAccessor propertyAccessor)
            => Create(properties, p => propertyAccessor[p]);

        private EntityKey Create(IReadOnlyList<IProperty> properties, Func<IProperty, object> reader)
        {
            var components = new object[properties.Count];
            var principalProperties = Key.Properties;

            for (var i = 0; i < properties.Count; i++)
            {
                var value = reader(properties[i]);

                if (value == null
                    || Equals(value, principalProperties[i].SentinelValue))
                {
                    return EntityKey.InvalidEntityKey;
                }

                components[i] = value;
            }

            return new CompositeEntityKey(Key, components);
        }
    }
}
