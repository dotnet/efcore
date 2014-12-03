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
            private readonly Type _contextType;
            private readonly string _name;
            private readonly Type _entityType;
            private readonly bool _hasSetter;

            public DbSetProperty([NotNull] Type contextType, [NotNull] string name, [NotNull] Type entityType, bool hasSetter)
            {
                Check.NotNull(contextType, "contextType");
                Check.NotNull(name, "name");
                Check.NotNull(entityType, "entityType");

                _contextType = contextType;
                _name = name;
                _entityType = entityType;
                _hasSetter = hasSetter;
            }

            public Type ContextType
            {
                get { return _contextType; }
            }

            public string Name
            {
                get { return _name; }
            }

            public Type EntityType
            {
                get { return _entityType; }
            }

            public bool HasSetter
            {
                get { return _hasSetter; }
            }
        }
    }
}
