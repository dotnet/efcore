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
    public class DbSetSource : IDbSetSource, IDbQuerySource
    {
        private static readonly MethodInfo _genericCreateSet
            = typeof(DbSetSource).GetTypeInfo().GetDeclaredMethod(nameof(CreateSetFactory));

        private static readonly MethodInfo _genericCreateSharedTypeSet
            = typeof(DbSetSource).GetTypeInfo().GetDeclaredMethod(nameof(CreateSharedTypeSetFactory));

        private static readonly MethodInfo _genericCreateQuery
            = typeof(DbSetSource).GetTypeInfo().GetDeclaredMethod(nameof(CreateQueryFactory));

        private readonly ConcurrentDictionary<string, Func<DbContext, string, object>> _cache
            = new ConcurrentDictionary<string, Func<DbContext, string, object>>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object Create(DbContext context, Type type)
            => CreateCore(context, type.DisplayName(), type, _genericCreateSet);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object CreateSharedTypeSet(DbContext context, string entityTypeName, Type type)
            => CreateCore(context, entityTypeName, type, _genericCreateSharedTypeSet);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object CreateQuery(DbContext context, Type type)
            => CreateCore(context, type.DisplayName(), type, _genericCreateQuery);

        private object CreateCore(DbContext context, string entityTypeName, Type type, MethodInfo createMethod)
            => _cache.GetOrAdd(
                entityTypeName,
                name => (Func<DbContext, string, object>)createMethod
                    .MakeGenericMethod(type)
                    .Invoke(null, null))(context, entityTypeName);

        [UsedImplicitly]
        private static Func<DbContext, string, object> CreateSetFactory<TEntity>()
            where TEntity : class
            => (ctx, _) => new InternalDbSet<TEntity>(ctx);

        [UsedImplicitly]
        private static Func<DbContext, string, object> CreateSharedTypeSetFactory<TEntity>()
            where TEntity : class
            => (ctx, entityTypeName) => new InternalSharedTypeDbSet<TEntity>(ctx, entityTypeName);

        [UsedImplicitly]
        private static Func<DbContext, string, DbQuery<TQuery>> CreateQueryFactory<TQuery>()
            where TQuery : class
            => (ctx, _) => new InternalDbQuery<TQuery>(ctx);
    }
}
