// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class EntityKeyFactorySource
    {
        private readonly ThreadSafeDictionaryCache<Type, EntityKeyFactory> _cache
            = new ThreadSafeDictionaryCache<Type, EntityKeyFactory>();

        public virtual EntityKeyFactory GetKeyFactory([NotNull] IReadOnlyList<IProperty> keyProperties)
        {
            Check.NotNull(keyProperties, "keyProperties");

            return keyProperties.Count == 1
                ? _cache.GetOrAdd(
                    keyProperties[0].PropertyType,
                    t => (EntityKeyFactory)Activator.CreateInstance(typeof(SimpleEntityKeyFactory<>).MakeGenericType(t)))
                : CompositeEntityKeyFactory.Instance;
        }
    }
}
