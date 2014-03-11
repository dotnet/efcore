// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class EntityKeyFactorySource
    {
        private readonly ThreadSafeLazyRef<ImmutableDictionary<Type, EntityKeyFactory>> _keyFactories
            = new ThreadSafeLazyRef<ImmutableDictionary<Type, EntityKeyFactory>>(() => ImmutableDictionary<Type, EntityKeyFactory>.Empty);

        public virtual EntityKeyFactory GetKeyFactory([NotNull] IReadOnlyList<IProperty> keyProperties)
        {
            Check.NotNull(keyProperties, "keyProperties");

            if (keyProperties.Count == 1)
            {
                var propertyType = keyProperties[0].PropertyType;
                if (!_keyFactories.Value.ContainsKey(propertyType))
                {
                    _keyFactories.ExchangeValue(
                        d => d.ContainsKey(propertyType)
                            ? d
                            : d.Add(propertyType, (EntityKeyFactory)Activator.CreateInstance(
                                typeof(SimpleEntityKeyFactory<>).MakeGenericType(propertyType))));
                }

                return _keyFactories.Value[propertyType];
            }

            return CompositeEntityKeyFactory.Instance;
        }
    }
}
