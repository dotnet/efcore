// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <inheritdoc />
    public class DbSetSource : IDbSetSource
    {
        private static readonly MethodInfo _genericCreateSet
            = typeof(DbSetSource).GetTypeInfo().GetDeclaredMethod(nameof(CreateSetFactory));

        private readonly ConcurrentDictionary<(Type Type, string Name), Func<DbContext, string, object>> _cache
            = new ConcurrentDictionary<(Type Type, string Name), Func<DbContext, string, object>>();

        /// <inheritdoc />
        public virtual object Create(DbContext context, Type type)
            => CreateCore(context, type, null, _genericCreateSet);

        /// <inheritdoc />
        public virtual object Create(DbContext context, string name, Type type)
            => CreateCore(context, type, name, _genericCreateSet);

        private object CreateCore(DbContext context, Type type, string name, MethodInfo createMethod)
            => _cache.GetOrAdd(
                (type, name),
                t => (Func<DbContext, string, object>)createMethod
                    .MakeGenericMethod(t.Type)
                    .Invoke(null, null))(context, name);

        [UsedImplicitly]
        private static Func<DbContext, string, object> CreateSetFactory<TEntity>()
            where TEntity : class
            => (c, name) => new InternalDbSet<TEntity>(c, name);
    }
}
