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
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ClrCollectionAccessorSource
    {
        private static readonly MethodInfo _genericCreate
            = typeof(ClrCollectionAccessorSource).GetTypeInfo().GetDeclaredMethods("CreateGeneric").Single();

        private readonly ThreadSafeDictionaryCache<Tuple<Type, string>, IClrCollectionAccessor> _cache
            = new ThreadSafeDictionaryCache<Tuple<Type, string>, IClrCollectionAccessor>();

        public virtual IClrCollectionAccessor GetAccessor([NotNull] INavigation navigation)
        {
            Check.NotNull(navigation, "navigation");

            var accessor = navigation as IClrCollectionAccessor;

            if (accessor != null)
            {
                return accessor;
            }

            // TODO: Currently assumes ICollection navigation property

            return _cache.GetOrAdd(
                Tuple.Create(navigation.EntityType.Type, navigation.Name),
                k => Create(k.Item1.GetAnyProperty(k.Item2)));
        }

        private IClrCollectionAccessor Create(PropertyInfo property)
        {
            var boundMethod = _genericCreate.MakeGenericMethod(
                property.DeclaringType, property.PropertyType, property.PropertyType.TryGetElementType(typeof(ICollection<>)));

            return (IClrCollectionAccessor)boundMethod.Invoke(null, new object[] { property });
        }

        private static IClrCollectionAccessor CreateGeneric<TEntity, TCollection, TElement>(PropertyInfo property)
            where TCollection : ICollection<TElement>
        {
            var getterDelegate = (Func<TEntity, TCollection>)property.GetMethod.CreateDelegate(typeof(Func<TEntity, TCollection>));

            return new ClrICollectionAccessor<TEntity, TCollection, TElement>(getterDelegate);
        }
    }
}
