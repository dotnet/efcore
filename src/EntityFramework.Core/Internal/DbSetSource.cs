// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Internal
{
    public class DbSetSource : IDbSetSource
    {
        private static readonly MethodInfo _genericCreate
            = typeof(DbSetSource).GetTypeInfo().GetDeclaredMethod(nameof(DbSetSource.CreateConstructor));

        // Stores DbSet<T> objects
        private readonly ConcurrentDictionary<Type, Func<DbContext, object>> _cache
            = new ConcurrentDictionary<Type, Func<DbContext, object>>();

        [GenericMethodFactory(nameof(CreateConstructor), typeof(TypeArgumentCategory.EntityTypes))]
        public virtual object Create(DbContext context, Type type)
            => _cache.GetOrAdd(
                type,
                t => (Func<DbContext, object>)_genericCreate.MakeGenericMethod(type).Invoke(null, null))(context);

        [UsedImplicitly]
        private static Func<DbContext, object> CreateConstructor<TEntity>() where TEntity : class
            => c => new InternalDbSet<TEntity>(c);
    }
}
