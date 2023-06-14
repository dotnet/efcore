// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class EntityFinderSource : IEntityFinderSource
{
    private readonly ConcurrentDictionary<Type, Func<IStateManager, IDbSetSource, IDbSetCache, IEntityType, IEntityFinder>> _cache
        = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEntityFinder Create(
        IStateManager stateManager,
        IDbSetSource setSource,
        IDbSetCache setCache,
        IEntityType type)
        => _cache.GetOrAdd(
            type.ClrType,
            t => (Func<IStateManager, IDbSetSource, IDbSetCache, IEntityType, IEntityFinder>)
                typeof(EntityFinderSource).GetMethod(nameof(CreateConstructor), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(t).Invoke(null, null)!)(stateManager, setSource, setCache, type);

    [UsedImplicitly]
    private static Func<IStateManager, IDbSetSource, IDbSetCache, IEntityType, IEntityFinder> CreateConstructor<TEntity>()
        where TEntity : class
        => (s, src, c, t) => new EntityFinder<TEntity>(s, src, c, t);
}
