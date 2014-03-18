// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class EntitySetFinder
    {
        private readonly ThreadSafeDictionaryCache<Type, IReadOnlyList<EntitySetProperty>> _cache
            = new ThreadSafeDictionaryCache<Type, IReadOnlyList<EntitySetProperty>>();

        public virtual IReadOnlyList<EntitySetProperty> FindSets([NotNull] EntityContext context)
        {
            Check.NotNull(context, "context");

            return _cache.GetOrAdd(context.GetType(), FindSets);
        }

        private static EntitySetProperty[] FindSets(Type contextType)
        {
            return contextType.GetRuntimeProperties()
                .Where(
                    p => !p.IsStatic()
                         && !p.GetIndexParameters().Any()
                         && p.DeclaringType != typeof(EntityContext)
                         && p.PropertyType.GetTypeInfo().IsGenericType
                         && p.PropertyType.GetGenericTypeDefinition() == typeof(EntitySet<>))
                .OrderBy(p => p.Name)
                .Select(p => new EntitySetProperty(
                    p.DeclaringType, p.Name,
                    p.PropertyType.GetTypeInfo().GenericTypeArguments.Single(), p.SetMethod != null))
                .ToArray();
        }

        public struct EntitySetProperty
        {
            private readonly Type _contextType;
            private readonly string _name;
            private readonly Type _entityType;
            private readonly bool _hasSetter;

            public EntitySetProperty([NotNull] Type contextType, [NotNull] string name, [NotNull] Type entityType, bool hasSetter)
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
