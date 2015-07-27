// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class SimpleNullSentinelEntityKeyFactory<TKey> : EntityKeyFactory
    {
        public SimpleNullSentinelEntityKeyFactory([NotNull] IKey key)
            : base(key)
        {
        }

        public override EntityKey Create(
            IReadOnlyList<IProperty> properties, ValueBuffer valueBuffer)
            => Create(valueBuffer[properties[0].Index]);

        public override EntityKey Create(
            IReadOnlyList<IProperty> properties, IPropertyAccessor propertyAccessor)
            => Create(propertyAccessor[properties[0]]);

        private EntityKey Create(object value)
            => value != null
                ? new SimpleEntityKey<TKey>(Key, (TKey)value)
                : EntityKey.InvalidEntityKey;
    }
}
