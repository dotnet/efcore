// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="DbContext" />
    ///     </para>
    ///     <para>
    ///         This type supports the Entity Framework Core infrastructure and is not intended to be used
    ///         directly from your code. This type may change or be removed in future releases.
    ///     </para>
    /// </summary>
    public sealed class DbContextDependencies : IDbContextDependencies
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DbContextDependencies(
            [NotNull] ICurrentDbContext currentContext,
            [NotNull] IChangeDetector changeDetector,
            [NotNull] IDbSetSource setSource,
            [NotNull] IDbQuerySource querySource,
            [NotNull] IEntityFinderSource entityFinderSource,
            [NotNull] IEntityGraphAttacher entityGraphAttacher,
            [NotNull] IModel model,
            [NotNull] IAsyncQueryProvider queryProvider,
            [NotNull] IStateManager stateManager,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Update> updateLogger,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Infrastructure> infrastuctureLogger)
        {
            ChangeDetector = changeDetector;
            SetSource = setSource;
            QuerySource = querySource;
            EntityGraphAttacher = entityGraphAttacher;
            Model = model;
            QueryProvider = queryProvider;
            StateManager = stateManager;
            UpdateLogger = updateLogger;
            InfrastructureLogger = infrastuctureLogger;
            EntityFinderFactory = new EntityFinderFactory(entityFinderSource, stateManager, setSource, currentContext.Context);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IModel Model { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IDbSetSource SetSource { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IDbQuerySource QuerySource { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IEntityFinderFactory EntityFinderFactory { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IAsyncQueryProvider QueryProvider { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IStateManager StateManager { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IChangeDetector ChangeDetector { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IEntityGraphAttacher EntityGraphAttacher { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Update> UpdateLogger { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Infrastructure> InfrastructureLogger { get; }
    }
}
