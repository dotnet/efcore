// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
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
            [NotNull] QueryContext queryContext,
            [CanBeNull] object entity,
            [NotNull] IReadOnlyList<INavigation> navigationPath,
            [NotNull] IReadOnlyList<IRelatedEntitiesLoader> relatedEntitiesLoaders,
            bool queryStateManager);

        Task IncludeAsync(
            [NotNull] QueryContext queryContext,
            [CanBeNull] object entity,
            [NotNull] IReadOnlyList<INavigation> navigationPath,
            [NotNull] IReadOnlyList<IAsyncRelatedEntitiesLoader> relatedEntitiesLoaders,
            bool queryStateManager,
            CancellationToken cancellationToken);
    }
}
