// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
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

        private readonly ThreadSafeLazyRef<ImmutableDictionary<Tuple<Type, string>, TAccessor>> _setters
            = new ThreadSafeLazyRef<ImmutableDictionary<Tuple<Type, string>, TAccessor>>(() => ImmutableDictionary<Tuple<Type, string>, TAccessor>.Empty);

        public virtual TAccessor GetAccessor([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            var clrPropertySetter = property as TAccessor;

            if (clrPropertySetter != null)
            {
                return clrPropertySetter;
            }

            return GetAccessor(property.EntityType.Type, property.Name);
        }

        public virtual TAccessor GetAccessor([NotNull] Type propertyType, [NotNull] string propertyName)
        {
            Check.NotNull(propertyType, "propertyType");
            Check.NotEmpty(propertyName, "propertyName");

            var key = Tuple.Create(propertyType, propertyName);

            TAccessor clrPropertySetter;
            if (!_setters.Value.TryGetValue(key, out clrPropertySetter))
            {
                var accessor = Create(propertyType.GetAnyProperty(propertyName));
                _setters.ExchangeValue(d => d.ContainsKey(key) ? d : d.Add(key, accessor));
                clrPropertySetter = _setters.Value[key];
            }

            return clrPropertySetter;
        }

        private TAccessor Create(PropertyInfo property)
        {
            var boundMethod = _genericCreate.MakeGenericMethod(property.DeclaringType, property.PropertyType);

            return (TAccessor)boundMethod.Invoke(this, new object[] { property });
        }

        protected abstract TAccessor CreateGeneric<TEntity, TValue>([NotNull] PropertyInfo property);
    }
}
