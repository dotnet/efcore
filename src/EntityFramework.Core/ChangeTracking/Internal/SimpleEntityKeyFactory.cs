// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

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
            IEntityType entityType, IReadOnlyList<IProperty> properties, IValueReader valueReader)
        {
            var index = properties[0].Index;
            if (!valueReader.IsNull(index))
            {
                var value = valueReader.ReadValue<TKey>(index);
                if (!EqualityComparer<TKey>.Default.Equals(value, _sentinel))
                {
                    return new SimpleEntityKey<TKey>(entityType, value);
                }
            }

            return EntityKey.InvalidEntityKey;
        }

        public override EntityKey Create(
            IEntityType entityType, IReadOnlyList<IProperty> properties, IPropertyAccessor propertyAccessor)
        {
            var value = propertyAccessor[properties[0]];

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
