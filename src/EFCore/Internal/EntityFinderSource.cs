// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     <para>
    ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///         directly from your code. This API may change or be removed in future releases.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public class EntityFinderSource : IEntityFinderSource
    {
        private static readonly MethodInfo _genericCreate
            = typeof(EntityFinderSource).GetTypeInfo().GetDeclaredMethod(nameof(CreateConstructor));

        private readonly ConcurrentDictionary<Type, Func<IStateManager, IDbSetSource, IDbSetCache, IEntityType, IEntityFinder>> _cache
            = new ConcurrentDictionary<Type, Func<IStateManager, IDbSetSource, IDbSetCache, IEntityType, IEntityFinder>>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEntityFinder Create(
            IStateManager stateManager, IDbSetSource setSource, IDbSetCache setCache, IEntityType type)
            => _cache.GetOrAdd(
                type.ClrType,
                t => (Func<IStateManager, IDbSetSource, IDbSetCache, IEntityType, IEntityFinder>)
                    _genericCreate.MakeGenericMethod(t).Invoke(null, null))(stateManager, setSource, setCache, type);

        [UsedImplicitly]
        private static Func<IStateManager, IDbSetSource, IDbSetCache, IEntityType, IEntityFinder> CreateConstructor<TEntity>()
            where TEntity : class
            => (s, src, c, t) => new EntityFinder<TEntity>(s, src, c, t);
    }
}
