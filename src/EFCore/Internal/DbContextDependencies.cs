// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

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
            [NotNull] IChangeDetector changeDetector,
            [NotNull] IDbSetInitializer dbSetInitializer,
            [NotNull] IEntityFinderSource entityFinderSource,
            [NotNull] IEntityGraphAttacher entityGraphAttacher,
            [NotNull] IModel model,
            [NotNull] IAsyncQueryProvider queryProvider,
            [NotNull] IStateManager stateManager)
        {
            Check.NotNull(changeDetector, nameof(changeDetector));
            Check.NotNull(dbSetInitializer, nameof(dbSetInitializer));
            Check.NotNull(entityFinderSource, nameof(entityFinderSource));
            Check.NotNull(entityGraphAttacher, nameof(entityGraphAttacher));
            Check.NotNull(model, nameof(model));
            Check.NotNull(queryProvider, nameof(queryProvider));
            Check.NotNull(stateManager, nameof(stateManager));

            ChangeDetector = changeDetector;
            DbSetInitializer = dbSetInitializer;
            EntityFinderSource = entityFinderSource;
            EntityGraphAttacher = entityGraphAttacher;
            Model = model;
            QueryProvider = queryProvider;
            StateManager = stateManager;
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
        public IDbSetInitializer DbSetInitializer { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IEntityFinderSource EntityFinderSource { get; }

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
    }
}
