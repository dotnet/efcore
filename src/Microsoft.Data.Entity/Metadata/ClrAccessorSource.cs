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
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public abstract class ClrAccessorSource<TAccessor>
        where TAccessor : class
    {
        private static readonly MethodInfo _genericCreate
            = typeof(ClrAccessorSource<TAccessor>).GetTypeInfo().GetDeclaredMethods("CreateGeneric").Single();

        private readonly ThreadSafeDictionaryCache<Tuple<Type, string>, TAccessor> _cache
            = new ThreadSafeDictionaryCache<Tuple<Type, string>, TAccessor>();

        public virtual TAccessor GetAccessor([NotNull] IPropertyBase property)
        {
            Check.NotNull(property, "property");

            var clrPropertySetter = property as TAccessor;

            if (clrPropertySetter != null)
            {
                return clrPropertySetter;
            }

            return GetAccessor(property.EntityType.Type, property.Name);
        }

        public virtual TAccessor GetAccessor([NotNull] Type declaringType, [NotNull] string propertyName)
        {
            Check.NotNull(declaringType, "declaringType");
            Check.NotEmpty(propertyName, "propertyName");

            return _cache.GetOrAdd(Tuple.Create(declaringType, propertyName), k => Create(k.Item1.GetAnyProperty(k.Item2)));
        }

        private TAccessor Create(PropertyInfo property)
        {
            var boundMethod = _genericCreate.MakeGenericMethod(property.DeclaringType, property.PropertyType);

            return (TAccessor)boundMethod.Invoke(this, new object[] { property });
        }

        protected abstract TAccessor CreateGeneric<TEntity, TValue>([NotNull] PropertyInfo property);
    }
}
