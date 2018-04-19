// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public interface IQueryBuffer : IDisposable
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
        void StartTracking(
            [NotNull] object entity,
            [NotNull] IEntityType entityType);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        void IncludeCollection<TEntity, TRelated, TElement>(
            int includeId,
            [NotNull] INavigation navigation,
            [CanBeNull] INavigation inverseNavigation,
            [NotNull] IEntityType targetEntityType,
            [NotNull] IClrCollectionAccessor clrCollectionAccessor,
            [CanBeNull] IClrPropertySetter inverseClrPropertySetter,
            bool tracking,
            [NotNull] TEntity instance,
            [NotNull] Func<IEnumerable<TRelated>> valuesFactory,
            [CanBeNull] Func<TEntity, TRelated, bool> joinPredicate)
            where TRelated : TElement;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        Task IncludeCollectionAsync<TEntity, TRelated, TElement>(
            int includeId,
            [NotNull] INavigation navigation,
            [CanBeNull] INavigation inverseNavigation,
            [NotNull] IEntityType targetEntityType,
            [NotNull] IClrCollectionAccessor clrCollectionAccessor,
            [CanBeNull] IClrPropertySetter inverseClrPropertySetter,
            bool tracking,
            [NotNull] TEntity instance,
            [NotNull] Func<IAsyncEnumerable<TRelated>> valuesFactory,
            [CanBeNull] Func<TEntity, TRelated, bool> joinPredicate,
            CancellationToken cancellationToken)
            where TRelated : TElement;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        TCollection CorrelateSubquery<TInner, TOut, TCollection>(
            int correlatedCollectionId,
            [NotNull] INavigation navigation,
            [NotNull] Func<INavigation, TCollection> resultCollectionFactory,
            in MaterializedAnonymousObject outerKey,
            bool tracking,
            [NotNull] Func<IEnumerable<Tuple<TInner, MaterializedAnonymousObject, MaterializedAnonymousObject>>> correlatedCollectionFactory,
            [NotNull] Func<MaterializedAnonymousObject, MaterializedAnonymousObject, bool> correlationPredicate)
            where TCollection : ICollection<TOut>
            where TInner : TOut;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        Task<TCollection> CorrelateSubqueryAsync<TInner, TOut, TCollection>(
            int correlatedCollectionId,
            [NotNull] INavigation navigation,
            [NotNull] Func<INavigation, TCollection> resultCollectionFactory,
            MaterializedAnonymousObject outerKey,
            bool tracking,
            [NotNull] Func<IAsyncEnumerable<Tuple<TInner, MaterializedAnonymousObject, MaterializedAnonymousObject>>> correlatedCollectionFactory,
            [NotNull] Func<MaterializedAnonymousObject, MaterializedAnonymousObject, bool> correlationPredicate,
            CancellationToken cancellationToken)
            where TCollection : ICollection<TOut>
            where TInner : TOut;
    }
}
