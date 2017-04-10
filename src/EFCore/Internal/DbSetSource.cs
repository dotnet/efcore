// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DbSetSource : IDbSetSource
    {
        private static readonly MethodInfo _genericCreate
            = typeof(DbSetSource).GetTypeInfo().GetDeclaredMethod(nameof(CreateConstructor));

        // Stores DbSet<T> objects
        private readonly ConcurrentDictionary<Type, Func<DbContext, object>> _cache
            = new ConcurrentDictionary<Type, Func<DbContext, object>>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object Create(DbContext context, Type type)
            => _cache.GetOrAdd(
                type,
                t => (Func<DbContext, object>)_genericCreate.MakeGenericMethod(t).Invoke(null, null))(context);

        [UsedImplicitly]
        private static Func<DbContext, object> CreateConstructor<TEntity>() where TEntity : class
            => c => new InternalDbSet<TEntity>(c);
    }
}
