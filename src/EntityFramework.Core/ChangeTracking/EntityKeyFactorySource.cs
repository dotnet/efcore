// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class EntityKeyFactorySource
    {
        private readonly CompositeEntityKeyFactory _compositeKeyFactory;

        private readonly ThreadSafeDictionaryCache<Type, EntityKeyFactory> _cache
            = new ThreadSafeDictionaryCache<Type, EntityKeyFactory>();

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected EntityKeyFactorySource()
        {
        }

        public EntityKeyFactorySource([NotNull] CompositeEntityKeyFactory compositeKeyFactory)
        {
            Check.NotNull(compositeKeyFactory, "compositeKeyFactory");

            _compositeKeyFactory = compositeKeyFactory;
        }

        public virtual EntityKeyFactory GetKeyFactory([NotNull] IReadOnlyList<IProperty> keyProperties)
        {
            Check.NotNull(keyProperties, "keyProperties");

            if (keyProperties.Count == 1)
            {
                var keyType = keyProperties[0].PropertyType;

                // Use composite key for anything with structural (e.g. byte[]) properties even if they are
                // not composite because it is setup to do structural comparisons and the generic typing
                // advantages of the simple key don't really apply anyway.
                if (!typeof(IStructuralEquatable).GetTypeInfo().IsAssignableFrom(keyType.GetTypeInfo()))
                {
                    return _cache.GetOrAdd(
                        keyType,
                        t => (EntityKeyFactory)(t.IsNullableType()
                            ? Activator.CreateInstance(typeof(SimpleEntityKeyFactory<>).MakeGenericType(t.UnwrapNullableType()))
                            : Activator.CreateInstance(typeof(SimpleEntityKeyFactory<>).MakeGenericType(t))));
                }
            }

            return _compositeKeyFactory;
        }
    }
}
