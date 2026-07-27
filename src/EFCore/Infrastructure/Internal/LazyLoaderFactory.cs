// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class LazyLoaderFactory : ILazyLoaderFactory
{
    private readonly ICurrentDbContext _currentContext;
    private readonly IDiagnosticsLogger<DbLoggerCategory.Infrastructure> _logger;

    // Use WeakReference to allow ILazyLoader instances to be GC'ed during enumeration,
    // preventing them from being rooted by the factory.
    //
    // List<WeakReference> is chosen over ConditionalWeakTable to avoid a 30-50%
    // performance regression.
    //
    // While the list does not self-compact, it is explicitly cleared during
    // ResetState/Dispose to prevent memory accumulation in pooled contexts.
    private readonly List<WeakReference<ILazyLoader>> _loaders = [];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public LazyLoaderFactory(
        ICurrentDbContext currentContext,
        IDiagnosticsLogger<DbLoggerCategory.Infrastructure> logger)
    {
        _currentContext = currentContext;
        _logger = logger;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ILazyLoader Create()
    {
        var loader = new LazyLoader(_currentContext, _logger);
        _loaders.Add(new WeakReference<ILazyLoader>(loader));
        return loader;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void Dispose()
    {
        foreach (var weakReference in _loaders)
        {
            if(weakReference.TryGetTarget(out var loader)) 
                loader.Dispose();
        }

        _loaders.Clear();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void ResetState()
        => Dispose();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Task ResetStateAsync(CancellationToken cancellationToken = default)
    {
        Dispose();

        return Task.CompletedTask;
    }
}
