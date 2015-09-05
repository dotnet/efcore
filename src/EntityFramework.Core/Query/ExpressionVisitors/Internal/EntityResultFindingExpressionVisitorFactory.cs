// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors.Internal
{
    public class EntityResultFindingExpressionVisitorFactory : IEntityResultFindingExpressionVisitorFactory
    {
        private readonly IModel _model;
        private readonly IEntityTrackingInfoFactory _entityTrackingInfoFactory;

        public EntityResultFindingExpressionVisitorFactory(
            [NotNull] IModel model,
            [NotNull] IEntityTrackingInfoFactory entityTrackingInfoFactory)
        {
            _model = model;
            _entityTrackingInfoFactory = entityTrackingInfoFactory;
        }

        public virtual EntityResultFindingExpressionVisitor Create([NotNull] QueryCompilationContext queryCompilationContext)
            => new EntityResultFindingExpressionVisitor(
                _model,
                _entityTrackingInfoFactory,
                queryCompilationContext);
    }
}
