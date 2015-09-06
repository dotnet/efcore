// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query
{
    public class EntityTrackingInfoFactory : IEntityTrackingInfoFactory
    {
        private readonly IEntityKeyFactorySource _entityKeyFactorySource;
        private readonly IClrAccessorSource<IClrPropertyGetter> _clrPropertyGetterSource;

        public EntityTrackingInfoFactory(
            [NotNull] IEntityKeyFactorySource entityKeyFactorySource,
            [NotNull] IClrAccessorSource<IClrPropertyGetter> clrPropertyGetterSource)
        {
            Check.NotNull(entityKeyFactorySource, nameof(entityKeyFactorySource));
            Check.NotNull(clrPropertyGetterSource, nameof(clrPropertyGetterSource));

            _entityKeyFactorySource = entityKeyFactorySource;
            _clrPropertyGetterSource = clrPropertyGetterSource;
        }

        public virtual EntityTrackingInfo Create(
            [NotNull] QueryCompilationContext queryCompilationContext,
            [NotNull] QuerySourceReferenceExpression querySourceReferenceExpression,
            [NotNull] IEntityType entityType)
        {
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));
            Check.NotNull(querySourceReferenceExpression, nameof(querySourceReferenceExpression));
            Check.NotNull(entityType, nameof(entityType));

            var trackingInfo = new EntityTrackingInfo(
                _entityKeyFactorySource,
                _clrPropertyGetterSource,
                queryCompilationContext,
                querySourceReferenceExpression,
                entityType);

            return trackingInfo;
        }
    }
}
