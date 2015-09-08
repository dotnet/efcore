// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Data.Entity.Internal
{
    public class DbSetFinder : IDbSetFinder
    {
        private readonly ThreadSafeDictionaryCache<Type, IReadOnlyList<DbSetProperty>> _cache
            = new ThreadSafeDictionaryCache<Type, IReadOnlyList<DbSetProperty>>();

        public virtual IReadOnlyList<DbSetProperty> FindSets(DbContext context) => _cache.GetOrAdd(context.GetType(), FindSets);

        private static DbSetProperty[] FindSets(Type contextType)
            => contextType.GetRuntimeProperties()
                .Where(
                    p => !p.IsStatic()
                         && !p.GetIndexParameters().Any()
                         && p.DeclaringType != typeof(DbContext)
                         && p.PropertyType.GetTypeInfo().IsGenericType
                         && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .OrderBy(p => p.Name)
                .Select(p => new DbSetProperty(
                    p.DeclaringType, p.Name,
                    p.PropertyType.GetTypeInfo().GenericTypeArguments.Single(), p.SetMethod != null))
                .ToArray();
    }
}
