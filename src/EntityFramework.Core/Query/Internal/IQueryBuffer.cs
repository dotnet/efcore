// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Query.Internal
{
    public delegate IEnumerable<EntityLoadInfo> RelatedEntitiesLoader(
        IKeyValue primaryKeyValue, Func<ValueBuffer, IKeyValue> foreignKeyFactory);

    public delegate IAsyncEnumerable<EntityLoadInfo> AsyncRelatedEntitiesLoader(
        IKeyValue primaryKeyValue, Func<ValueBuffer, IKeyValue> foreignKeyFactory);

    public interface IQueryBuffer
    {
        void BeginTrackingQuery();

        object GetEntity(
            [NotNull] IKeyValue keyValue,
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
