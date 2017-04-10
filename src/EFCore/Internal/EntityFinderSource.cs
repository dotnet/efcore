// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EntityFinderSource : IEntityFinderSource
    {
        private static readonly MethodInfo _genericCreate
            = typeof(EntityFinderSource).GetTypeInfo().GetDeclaredMethod(nameof(CreateConstructor));

        private readonly ConcurrentDictionary<Type, Func<DbContext, IEntityType, IEntityFinder>> _cache
            = new ConcurrentDictionary<Type, Func<DbContext, IEntityType, IEntityFinder>>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEntityFinder Create(DbContext context, IEntityType type)
            => _cache.GetOrAdd(
                type.ClrType,
                t => (Func<DbContext, IEntityType, IEntityFinder>)_genericCreate.MakeGenericMethod(t).Invoke(null, null))(context, type);

        [UsedImplicitly]
        private static Func<DbContext, IEntityType, IEntityFinder> CreateConstructor<TEntity>() where TEntity : class
            => (c, t) => new EntityFinder<TEntity>(c, t);
    }
}
