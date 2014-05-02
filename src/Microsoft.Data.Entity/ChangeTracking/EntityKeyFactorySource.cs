// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

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
