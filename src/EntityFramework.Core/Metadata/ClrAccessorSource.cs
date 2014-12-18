// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

        protected abstract TAccessor CreateGeneric<TEntity, TValue>([NotNull] PropertyInfo property) where TEntity : class;
    }
}
