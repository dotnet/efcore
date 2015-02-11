// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class DbSetFinder
    {
        private readonly ThreadSafeDictionaryCache<Type, IReadOnlyList<DbSetProperty>> _cache
            = new ThreadSafeDictionaryCache<Type, IReadOnlyList<DbSetProperty>>();

        public virtual IReadOnlyList<DbSetProperty> FindSets([NotNull] DbContext context)
        {
            Check.NotNull(context, "context");

            return _cache.GetOrAdd(context.GetType(), FindSets);
        }

        private static DbSetProperty[] FindSets(Type contextType)
        {
            return contextType.GetRuntimeProperties()
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

        public struct DbSetProperty
        {
            public DbSetProperty([NotNull] Type contextType, [NotNull] string name, [NotNull] Type entityType, bool hasSetter)
            {
                Check.NotNull(contextType, "contextType");
                Check.NotNull(name, "name");
                Check.NotNull(entityType, "entityType");
                Check.ValidEntityType(entityType, "entityType");

                ContextType = contextType;
                Name = name;
                EntityType = entityType;
                HasSetter = hasSetter;
            }

            public Type ContextType { get; }

            public string Name { get; }

            public Type EntityType { get; }

            public bool HasSetter { get; }
        }
    }
}
