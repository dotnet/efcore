// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class SimpleEntityKeyFactory<TKey> : EntityKeyFactory
    {
        private readonly TKey _sentinel;

        public SimpleEntityKeyFactory([CanBeNull] TKey sentinel)
        {
            _sentinel = sentinel;
        }

        public override EntityKey Create(
            IEntityType entityType, IReadOnlyList<IProperty> properties, ValueBuffer valueBuffer)
            => Create(entityType, valueBuffer[properties[0].Index]);

        public override EntityKey Create(
            IEntityType entityType, IReadOnlyList<IProperty> properties, IPropertyAccessor propertyAccessor)
            => Create(entityType, propertyAccessor[properties[0]]);

        private EntityKey Create(IEntityType entityType, object value)
        {
            if (value != null)
            {
                var typedValue = (TKey)value;
                if (!EqualityComparer<TKey>.Default.Equals(typedValue, _sentinel))
                {
                    return new SimpleEntityKey<TKey>(entityType, typedValue);
                }
            }

            return EntityKey.InvalidEntityKey;
        }
    }
}
