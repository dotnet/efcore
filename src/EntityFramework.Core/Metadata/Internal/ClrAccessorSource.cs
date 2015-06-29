// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public abstract class ClrAccessorSource<TAccessor> : IClrAccessorSource<TAccessor>
        where TAccessor : class
    {
        private static readonly MethodInfo _genericCreate
            = typeof(ClrAccessorSource<TAccessor>).GetTypeInfo().GetDeclaredMethods("CreateGeneric").Single();

        private readonly ThreadSafeDictionaryCache<Tuple<Type, string>, TAccessor> _cache
            = new ThreadSafeDictionaryCache<Tuple<Type, string>, TAccessor>();

        public virtual TAccessor GetAccessor(IPropertyBase property)
            => property as TAccessor ?? GetAccessor(property.EntityType.ClrType, property.Name);

        public virtual TAccessor GetAccessor(Type declaringType, string propertyName)
            => _cache.GetOrAdd(Tuple.Create(declaringType, propertyName), k => Create(k.Item1.GetAnyProperty(k.Item2)));

        private TAccessor Create(PropertyInfo property)
        {
            var boundMethod = _genericCreate.MakeGenericMethod(property.DeclaringType, property.PropertyType, property.PropertyType.UnwrapNullableType());

            return (TAccessor)boundMethod.Invoke(this, new object[] { property });
        }

        protected abstract TAccessor CreateGeneric<TEntity, TValue, TNonNullableEnumValue>([NotNull] PropertyInfo property) where TEntity : class;
    }
}
