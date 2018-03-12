// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EntityTrackingInfo
    {
        private readonly IEntityType _entityType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityTrackingInfo(
            [NotNull] QueryCompilationContext queryCompilationContext,
            [NotNull] QuerySourceReferenceExpression querySourceReferenceExpression,
            [NotNull] IEntityType entityType)
        {
            Check.NotNull(querySourceReferenceExpression, nameof(querySourceReferenceExpression));
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));

            QuerySourceReferenceExpression = querySourceReferenceExpression;

            var sequenceType = querySourceReferenceExpression.Type.TryGetSequenceType();

            IsEnumerableTarget
                = sequenceType != null
                  && sequenceType == entityType.ClrType;

            _entityType = entityType;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsEnumerableTarget { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual QuerySourceReferenceExpression QuerySourceReferenceExpression { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IQuerySource QuerySource => QuerySourceReferenceExpression.ReferencedQuerySource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void StartTracking(
            [NotNull] IStateManager stateManager,
            [NotNull] object entity,
            in ValueBuffer valueBuffer)
        {
            Check.NotNull(stateManager, nameof(stateManager));
            Check.NotNull(entity, nameof(entity));

            stateManager.StartTrackingFromQuery(_entityType, entity, valueBuffer, handledForeignKeys: null);
        }
    }
}
