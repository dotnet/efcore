// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class EntitySetSource
    {
        private static readonly MethodInfo _genericCreate
            = typeof(EntitySetSource).GetTypeInfo().GetDeclaredMethods("CreateConstructor").Single();

        private readonly ThreadSafeDictionaryCache<Type, Func<EntityContext, EntitySet>> _cache
            = new ThreadSafeDictionaryCache<Type, Func<EntityContext, EntitySet>>();

        public virtual EntitySet Create([NotNull] EntityContext context, [NotNull] Type type)
        {
            Check.NotNull(context, "context");
            Check.NotNull("type", "type");

            var factory = _cache.GetOrAdd(
                type,
                t => (Func<EntityContext, EntitySet>)_genericCreate.MakeGenericMethod(type).Invoke(null, null));

            return factory(context);
        }

        private static Func<EntityContext, EntitySet> CreateConstructor<TEntity>() where TEntity : class
        {
            return c => new EntitySet<TEntity>(c);
        }
    }
}
