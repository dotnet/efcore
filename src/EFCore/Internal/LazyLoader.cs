// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Transient"/>. This means that each
    ///         entity instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class LazyLoader : ILazyLoader
    {
        private bool _disposed;
        private IDictionary<string, bool> _loadedStates;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public LazyLoader(
            [NotNull] ICurrentDbContext currentContext,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Infrastructure> logger)
        {
            Check.NotNull(currentContext, nameof(currentContext));
            Check.NotNull(logger, nameof(logger));

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
            if (_loadedStates == null)
            {
                _loadedStates = new Dictionary<string, bool>();
            }

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
                entry.Load();
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Task LoadAsync(
            object entity,
            CancellationToken cancellationToken = default,
            // ReSharper disable once AssignNullToNotNullAttribute
            [CallerMemberName] string navigationName = "")
        {
            Check.NotNull(entity, nameof(entity));
            Check.NotEmpty(navigationName, nameof(navigationName));

            return ShouldLoad(entity, navigationName, out var entry)
                ? entry.LoadAsync(cancellationToken)
                : Task.CompletedTask;
        }

        private bool ShouldLoad(object entity, string navigationName,
            out NavigationEntry navigationEntry)
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
        public virtual void Dispose() => _disposed = true;
    }
}
