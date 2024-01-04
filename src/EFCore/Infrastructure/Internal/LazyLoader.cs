// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class LazyLoader : ILazyLoader, IInjectableService
{
    private QueryTrackingBehavior? _queryTrackingBehavior;
    private bool _disposed;
    private bool _detached;
    private IDictionary<string, bool>? _loadedStates;
    private readonly ConcurrentDictionary<(object Entity, string NavigationName), bool> _isLoading = new(NavEntryEqualityComparer.Instance);
    private HashSet<string>? _nonLazyNavigations;

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
    public virtual void Injected(DbContext context, object entity, ParameterBindingInfo bindingInfo)
    {
        _queryTrackingBehavior = bindingInfo.QueryTrackingBehavior;
        _nonLazyNavigations ??= InitNavigationsMetadata(bindingInfo.StructuralType as IEntityType
            ?? throw new NotImplementedException("Navigations on complex types are not supported"));
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
    public virtual bool IsLoaded(object entity, string navigationName = "")
        => _loadedStates != null
            && _loadedStates.TryGetValue(navigationName, out var loaded)
            && loaded;

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
    protected virtual DbContext? Context { get; set; }

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

        var navEntry = (entity, navigationName);
        if (_isLoading.TryAdd(navEntry, true))
        {
            try
            {
                // ShouldLoad is called after _isLoading.Add because it could attempt to load the property. See #13138.
                if (ShouldLoad(entity, navigationName, out var entry))
                {
                    try
                    {
                        entry.Load(
                            _queryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution
                                ? LoadOptions.ForceIdentityResolution
                                : LoadOptions.None);
                    }
                    catch
                    {
                        entry.IsLoaded = false;
                        throw;
                    }
                }
            }
            finally
            {
                _isLoading.TryRemove(navEntry, out _);
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

        var navEntry = (entity, navigationName);
        if (_isLoading.TryAdd(navEntry, true))
        {
            try
            {
                // ShouldLoad is called after _isLoading.Add because it could attempt to load the property. See #13138.
                if (ShouldLoad(entity, navigationName, out var entry))
                {
                    try
                    {
                        await entry.LoadAsync(
                            _queryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution
                                ? LoadOptions.ForceIdentityResolution
                                : LoadOptions.None,
                            cancellationToken).ConfigureAwait(false);
                    }
                    catch
                    {
                        entry.IsLoaded = false;
                        throw;
                    }
                }
            }
            finally
            {
                _isLoading.TryRemove(navEntry, out _);
            }
        }
    }

    private sealed class NavEntryEqualityComparer : IEqualityComparer<(object Entity, string NavigationName)>
    {
        public static readonly NavEntryEqualityComparer Instance = new();

        private NavEntryEqualityComparer()
        {
        }

        public bool Equals((object Entity, string NavigationName) x, (object Entity, string NavigationName) y)
            => ReferenceEquals(x.Entity, y.Entity)
                && string.Equals(x.NavigationName, y.NavigationName, StringComparison.Ordinal);

        public int GetHashCode((object Entity, string NavigationName) obj)
            => HashCode.Combine(RuntimeHelpers.GetHashCode(obj.Entity), obj.NavigationName.GetHashCode());
    }

    private bool ShouldLoad(object entity, string navigationName, [NotNullWhen(true)] out NavigationEntry? navigationEntry)
    {
        if (!_detached && !IsLoaded(entity, navigationName))
        {
            if (_nonLazyNavigations == null
                || !_nonLazyNavigations.Contains(navigationName))
            {
                if (_disposed)
                {
                    Logger.LazyLoadOnDisposedContextWarning(Context, entity, navigationName);
                }
                else if (Context!.ChangeTracker.LazyLoadingEnabled)
                {
                    navigationEntry = Context.Entry(entity).Navigation(navigationName); // Will use local-DetectChanges, if enabled.
                    if (!navigationEntry.IsLoaded) // Check again because the nav may be loaded without the loader knowing
                    {
                        Logger.NavigationLazyLoading(Context, entity, navigationName);

                        return true;
                    }
                }
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
    {
        Context = null;
        _disposed = true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool Detaching(DbContext context, object entity)
    {
        _detached = true;
        Dispose();
        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Attaching(DbContext context, IEntityType entityType, object entity)
    {
        _disposed = false;
        _detached = false;
        Context = context;
        _nonLazyNavigations ??= InitNavigationsMetadata(entityType);
    }

    private HashSet<string> InitNavigationsMetadata(IEntityType entityType)
        => entityType!.GetNavigations()
            .Cast<INavigationBase>()
            .Concat(entityType.GetSkipNavigations())
            .Where(n => !n.LazyLoadingEnabled)
            .Select(t => t.Name)
            .ToHashSet();
}
