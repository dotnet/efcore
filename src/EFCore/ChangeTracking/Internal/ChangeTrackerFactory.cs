// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ChangeTrackerFactory : IChangeTrackerFactory
{
    private readonly DbContext _context;
    private readonly IStateManager _stateManager;
    private readonly IChangeDetector _changeDetector;
    private readonly IModel _model;
    private readonly IEntityEntryGraphIterator _graphIterator;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ChangeTrackerFactory(
        ICurrentDbContext currentContext,
        IStateManager stateManager,
        IChangeDetector changeDetector,
        IModel model,
        IEntityEntryGraphIterator graphIterator)
    {
        _context = currentContext.Context;
        _stateManager = stateManager;
        _changeDetector = changeDetector;
        _model = model;
        _graphIterator = graphIterator;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ChangeTracker Create()
        => new(_context, _stateManager, _changeDetector, _model, _graphIterator);
}
