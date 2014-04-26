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

            return keyProperties.Count == 1
                ? _cache.GetOrAdd(
                    keyProperties[0].PropertyType,
                    t => (EntityKeyFactory)Activator.CreateInstance(typeof(SimpleEntityKeyFactory<>).MakeGenericType(t)))
                : _compositeKeyFactory;
        }
    }
}
