// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class DbSetSource
    {
        private static readonly MethodInfo _genericCreate
            = typeof(DbSetSource).GetTypeInfo().GetDeclaredMethods("CreateConstructor").Single();

        private readonly ThreadSafeDictionaryCache<Type, Func<DbContext, DbSet>> _cache
            = new ThreadSafeDictionaryCache<Type, Func<DbContext, DbSet>>();

        public virtual DbSet Create([NotNull] DbContext context, [NotNull] Type type)
        {
            Check.NotNull(context, "context");
            Check.NotNull("type", "type");

            var factory = _cache.GetOrAdd(
                type,
                t => (Func<DbContext, DbSet>)_genericCreate.MakeGenericMethod(type).Invoke(null, null));

            return factory(context);
        }

        [UsedImplicitly]
        private static Func<DbContext, DbSet> CreateConstructor<TEntity>() where TEntity : class
        {
            return c => new DbSet<TEntity>(c);
        }
    }
}
