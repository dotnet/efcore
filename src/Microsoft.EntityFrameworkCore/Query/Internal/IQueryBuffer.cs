// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public interface IQueryBuffer
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        object GetEntity(
            [NotNull] IKey key,
            EntityLoadInfo entityLoadInfo,
            bool queryStateManager,
            bool throwOnNullKey);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        object GetPropertyValue(
            [NotNull] object entity,
            [NotNull] IProperty property);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        void StartTracking(
            [NotNull] object entity,
            [NotNull] EntityTrackingInfo entityTrackingInfo);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        void Include(
            [NotNull] QueryContext queryContext,
            [CanBeNull] object entity,
            [NotNull] IReadOnlyList<INavigation> navigationPath,
            [NotNull] IReadOnlyList<IRelatedEntitiesLoader> relatedEntitiesLoaders,
            bool queryStateManager);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        Task IncludeAsync(
            [NotNull] QueryContext queryContext,
            [CanBeNull] object entity,
            [NotNull] IReadOnlyList<INavigation> navigationPath,
            [NotNull] IReadOnlyList<IAsyncRelatedEntitiesLoader> relatedEntitiesLoaders,
            bool queryStateManager,
            CancellationToken cancellationToken);
    }
}
