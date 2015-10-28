// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class EntityTrackingInfoFactory : IEntityTrackingInfoFactory
    {
        private readonly IKeyValueFactorySource _keyValueFactorySource;

        public EntityTrackingInfoFactory(
            [NotNull] IKeyValueFactorySource keyValueFactorySource)
        {
            _keyValueFactorySource = keyValueFactorySource;
        }

        public virtual EntityTrackingInfo Create(
            QueryCompilationContext queryCompilationContext,
            QuerySourceReferenceExpression querySourceReferenceExpression,
            IEntityType entityType)
        {
            var trackingInfo = new EntityTrackingInfo(
                _keyValueFactorySource,
                queryCompilationContext,
                querySourceReferenceExpression,
                entityType);

            return trackingInfo;
        }
    }
}
