// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class LazyLoader : ILazyLoader
{
    private bool _disposed;
    private IDictionary<string, bool>? _loadedStates;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public LazyLoader(
        ICurrentDbContext currentContext,
        IDiagnosticsLogger<DbLoggerCategory.Infrastructure> logger)
    {
        Context = currentContext.Context;
        Logger = logger;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetLoaded(
        object entity,
        [CallerMemberName] string navigationName = "",
        bool loaded = true)
    {
        _loadedStates ??= new Dictionary<string, bool>();

        _loadedStates[navigationName] = loaded;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IDiagnosticsLogger<DbLoggerCategory.Infrastructure> Logger { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual DbContext Context { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    // ReSharper disable once AssignNullToNotNullAttribute
    public virtual void Load(object entity, [CallerMemberName] string navigationName = "")
    {
        Check.NotNull(entity, nameof(entity));
        Check.NotEmpty(navigationName, nameof(navigationName));

        if (ShouldLoad(entity, navigationName, out var entry))
        {
            try
            {
                entry.Load();
            }
            catch
            {
                SetLoaded(entity, navigationName, false);
                throw;
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual async Task LoadAsync(
        object entity,
        CancellationToken cancellationToken = default,
        [CallerMemberName] string navigationName = "")
    {
        Check.NotNull(entity, nameof(entity));
        Check.NotEmpty(navigationName, nameof(navigationName));

        if (ShouldLoad(entity, navigationName, out var entry))
        {
            try
            {
                await entry.LoadAsync(cancellationToken);
            }
            catch
            {
                SetLoaded(entity, navigationName, false);
                throw;
            }
        }
    }

    private bool ShouldLoad(object entity, string navigationName, [NotNullWhen(true)] out NavigationEntry? navigationEntry)
    {
        if (_loadedStates != null
            && _loadedStates.TryGetValue(navigationName, out var loaded)
            && loaded)
        {
            navigationEntry = null;
            return false;
        }

        if (_disposed)
        {
            Logger.LazyLoadOnDisposedContextWarning(Context, entity, navigationName);
        }
        else if (Context.ChangeTracker.LazyLoadingEnabled)
        {
            // Set early to avoid recursive loading overflow
            SetLoaded(entity, navigationName, loaded: true);

            var entityEntry = Context.Entry(entity); // Will use local-DetectChanges, if enabled.
            var tempNavigationEntry = entityEntry.Navigation(navigationName);

            if (entityEntry.State == EntityState.Detached)
            {
                Logger.DetachedLazyLoadingWarning(Context, entity, navigationName);
            }
            else if (!tempNavigationEntry.IsLoaded)
            {
                Logger.NavigationLazyLoading(Context, entity, navigationName);

                navigationEntry = tempNavigationEntry;

                return true;
            }
        }

        navigationEntry = null;
        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Dispose()
        => _disposed = true;
}
