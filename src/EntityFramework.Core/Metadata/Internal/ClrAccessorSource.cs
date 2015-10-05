// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public abstract class ClrAccessorSource<TAccessor> : IClrAccessorSource<TAccessor>
        where TAccessor : class
    {
        private readonly ThreadSafeDictionaryCache<Tuple<Type, string>, TAccessor> _cache
            = new ThreadSafeDictionaryCache<Tuple<Type, string>, TAccessor>();

        public virtual TAccessor GetAccessor(IPropertyBase property)
            => property as TAccessor ?? GetAccessor(property.DeclaringEntityType.ClrType, property.Name);

        public virtual TAccessor GetAccessor(Type declaringType, string propertyName)
            => _cache.GetOrAdd(Tuple.Create(declaringType, propertyName), k => Create(k.Item1.GetAnyProperty(k.Item2)));

        // TODO revisit when .NET Native supports ImpliesMethodInstantiation
        // original version used generics, which is much cleaner and performant but fails after ILC strips reflection info
        // https://github.com/aspnet/EntityFramework/issues/3477
        protected abstract TAccessor Create([NotNull] PropertyInfo property);
    }
}
