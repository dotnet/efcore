// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query
{
    public class EntityTrackingInfo
    {
        public EntityTrackingInfo(
            [NotNull] QuerySourceReferenceExpression querySourceReferenceExpression,
            [NotNull] IEntityType entityType)
        {
            Check.NotNull(querySourceReferenceExpression, nameof(querySourceReferenceExpression));
            Check.NotNull(entityType, nameof(entityType));

            QuerySourceReferenceExpression = querySourceReferenceExpression;
            EntityType = entityType;
        }

        public virtual QuerySourceReferenceExpression QuerySourceReferenceExpression { get; }
        public virtual IEntityType EntityType { get; }

        public virtual IQuerySource QuerySource => QuerySourceReferenceExpression.ReferencedQuerySource;
    }
}
