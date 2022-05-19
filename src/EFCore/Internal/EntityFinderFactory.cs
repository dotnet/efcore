// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class EntityFinderFactory : IEntityFinderFactory
{
    private readonly IEntityFinderSource _entityFinderSource;
    private readonly IStateManager _stateManager;
    private readonly IDbSetSource _setSource;
    private readonly IDbSetCache _setCache;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public EntityFinderFactory(
        IEntityFinderSource entityFinderSource,
        IStateManager stateManager,
        IDbSetSource setSource,
        IDbSetCache setCache)
    {
        _entityFinderSource = entityFinderSource;
        _stateManager = stateManager;
        _setSource = setSource;
        _setCache = setCache;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEntityFinder Create(IEntityType type)
        => _entityFinderSource.Create(_stateManager, _setSource, _setCache, type);
}
