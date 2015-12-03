// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Query.Internal
{
    public delegate IEnumerable<EntityLoadInfo> RelatedEntitiesLoader(
        IIncludeKeyComparer keyComparer);

    public delegate IAsyncEnumerable<EntityLoadInfo> AsyncRelatedEntitiesLoader(
        IIncludeKeyComparer keyComparer);

    public interface IQueryBuffer
    {
        object GetEntity(
            [NotNull] IKey key,
            EntityLoadInfo entityLoadInfo,
            bool queryStateManager,
            bool throwOnNullKey);

        object GetPropertyValue(
            [NotNull] object entity,
            [NotNull] IProperty property);

        void StartTracking(
            [NotNull] object entity,
            [NotNull] EntityTrackingInfo entityTrackingInfo);

        void Include(
            [CanBeNull] object entity,
            [NotNull] IReadOnlyList<INavigation> navigationPath,
            [NotNull] IReadOnlyList<RelatedEntitiesLoader> relatedEntitiesLoaders,
            bool queryStateManager);

        Task IncludeAsync(
            [CanBeNull] object entity,
            [NotNull] IReadOnlyList<INavigation> navigationPath,
            [NotNull] IReadOnlyList<AsyncRelatedEntitiesLoader> relatedEntitiesLoaders,
            CancellationToken cancellationToken,
            bool queryStateManager);
    }
}
