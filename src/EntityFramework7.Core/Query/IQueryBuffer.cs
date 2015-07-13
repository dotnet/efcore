// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Query
{
    public delegate IEnumerable<EntityLoadInfo> RelatedEntitiesLoader(
        EntityKey primaryKey, Func<ValueBuffer, EntityKey> foreignKeyFactory);

    public delegate IAsyncEnumerable<EntityLoadInfo> AsyncRelatedEntitiesLoader(
        EntityKey primaryKey, Func<ValueBuffer, EntityKey> foreignKeyFactory);

    public interface IQueryBuffer
    {
        object GetEntity(
            [NotNull] IEntityType entityType,
            [NotNull] EntityKey entityKey,
            EntityLoadInfo entityLoadInfo,
            bool queryStateManager);

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
