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
    /// <inheritdoc />
    public class EntityFinderSource : IEntityFinderSource
    {
        private static readonly MethodInfo _genericCreate
            = typeof(EntityFinderSource).GetTypeInfo().GetDeclaredMethod(nameof(CreateConstructor));

        private readonly ConcurrentDictionary<Type, Func<IStateManager, IDbSetSource, IDbSetCache, IEntityType, IEntityFinder>> _cache
            = new ConcurrentDictionary<Type, Func<IStateManager, IDbSetSource, IDbSetCache, IEntityType, IEntityFinder>>();

        /// <inheritdoc />
        public virtual IEntityFinder Create(
            IStateManager stateManager,
            IDbSetSource setSource,
            IDbSetCache setCache,
            IEntityType type)
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
