// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class LazyLoader : ILazyLoader, IDisposable
    {
        private bool _disposed;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IDiagnosticsLogger<DbLoggerCategory.Infrastructure> Logger { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual DbContext Context { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // ReSharper disable once AssignNullToNotNullAttribute
        public virtual void Load(object entity, [CallerMemberName] string navigationName = null)
        {
            Check.NotNull(entity, nameof(entity));
            Check.NotEmpty(navigationName, nameof(navigationName));

            if (ShouldLoad(entity, navigationName, out var entry))
            {
                entry.Load();
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Task LoadAsync(
            object entity,
            CancellationToken cancellationToken = default,
            // ReSharper disable once AssignNullToNotNullAttribute
            [CallerMemberName] string navigationName = null)
        {
            Check.NotNull(entity, nameof(entity));
            Check.NotEmpty(navigationName, nameof(navigationName));

            return ShouldLoad(entity, navigationName, out var entry)
                ? entry.LoadAsync(cancellationToken)
                : Task.CompletedTask;
        }

        private bool ShouldLoad(object entity, string navigationName, out NavigationEntry navigationEntry)
        {
            EntityEntry GetEntryWithoutDetectChanges()
            {
                var autoDetectChangesEnabled = Context.ChangeTracker.AutoDetectChangesEnabled;
                try
                {
                    Context.ChangeTracker.AutoDetectChangesEnabled = false;

                    return Context.Entry(entity);
                }
                finally
                {
                    Context.ChangeTracker.AutoDetectChangesEnabled = autoDetectChangesEnabled;
                }
            }

            if (_disposed)
            {
                Logger.LazyLoadOnDisposedContextWarning(Context, entity, navigationName);
            }
            else if (Context.ChangeTracker.LazyLoadingEnabled)
            {
                var entityEntry = GetEntryWithoutDetectChanges();
                navigationEntry = entityEntry.Navigation(navigationName);

                if (entityEntry.State == EntityState.Detached)
                {
                    var value = navigationEntry.CurrentValue;
                    if (value == null
                        || (navigationEntry.Metadata.IsCollection()
                            && !((IEnumerable)value).Any()))
                    {
                        Logger.DetachedLazyLoadingWarning(Context, entity, navigationName);
                    }
                }
                else if (!navigationEntry.IsLoaded)
                {
                    Logger.NavigationLazyLoading(Context, entity, navigationName);

                    return true;
                }
            }

            navigationEntry = null;
            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Dispose() => _disposed = true;
    }
}
