// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Query.ExpressionVisitors;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public abstract class EntityQueryModelVisitorFactory : IEntityQueryModelVisitorFactory
    {
        protected EntityQueryModelVisitorFactory(
            [NotNull] IModel model,
            [NotNull] IQueryOptimizer queryOptimizer,
            [NotNull] INavigationRewritingExpressionVisitorFactory navigationRewritingExpressionVisitorFactory,
            [NotNull] ISubQueryMemberPushDownExpressionVisitor subQueryMemberPushDownExpressionVisitor,
            [NotNull] IQuerySourceTracingExpressionVisitorFactory querySourceTracingExpressionVisitorFactory,
            [NotNull] IEntityResultFindingExpressionVisitorFactory entityResultFindingExpressionVisitorFactory,
            [NotNull] ITaskBlockingExpressionVisitor taskBlockingExpressionVisitor,
            [NotNull] IMemberAccessBindingExpressionVisitorFactory memberAccessBindingExpressionVisitorFactory,
            [NotNull] IOrderingExpressionVisitorFactory orderingExpressionVisitorFactory,
            [NotNull] IProjectionExpressionVisitorFactory projectionExpressionVisitorFactory,
            [NotNull] IEntityQueryableExpressionVisitorFactory entityQueryableExpressionVisitorFactory,
            [NotNull] IQueryAnnotationExtractor queryAnnotationExtractor,
            [NotNull] IResultOperatorHandler resultOperatorHandler,
            [NotNull] IEntityMaterializerSource entityMaterializerSource,
            [NotNull] IExpressionPrinter expressionPrinter)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(queryOptimizer, nameof(queryOptimizer));
            Check.NotNull(navigationRewritingExpressionVisitorFactory, nameof(navigationRewritingExpressionVisitorFactory));
            Check.NotNull(subQueryMemberPushDownExpressionVisitor, nameof(subQueryMemberPushDownExpressionVisitor));
            Check.NotNull(querySourceTracingExpressionVisitorFactory, nameof(querySourceTracingExpressionVisitorFactory));
            Check.NotNull(entityResultFindingExpressionVisitorFactory, nameof(entityResultFindingExpressionVisitorFactory));
            Check.NotNull(taskBlockingExpressionVisitor, nameof(taskBlockingExpressionVisitor));
            Check.NotNull(memberAccessBindingExpressionVisitorFactory, nameof(memberAccessBindingExpressionVisitorFactory));
            Check.NotNull(orderingExpressionVisitorFactory, nameof(orderingExpressionVisitorFactory));
            Check.NotNull(projectionExpressionVisitorFactory, nameof(projectionExpressionVisitorFactory));
            Check.NotNull(entityQueryableExpressionVisitorFactory, nameof(entityQueryableExpressionVisitorFactory));
            Check.NotNull(queryAnnotationExtractor, nameof(queryAnnotationExtractor));
            Check.NotNull(resultOperatorHandler, nameof(resultOperatorHandler));
            Check.NotNull(entityMaterializerSource, nameof(entityMaterializerSource));
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            Model = model;
            QueryOptimizer = queryOptimizer;
            NavigationRewritingExpressionVisitorFactory = navigationRewritingExpressionVisitorFactory;
            SubQueryMemberPushDownExpressionVisitor = subQueryMemberPushDownExpressionVisitor;
            QuerySourceTracingExpressionVisitorFactory = querySourceTracingExpressionVisitorFactory;
            EntityResultFindingExpressionVisitorFactory = entityResultFindingExpressionVisitorFactory;
            TaskBlockingExpressionVisitor = taskBlockingExpressionVisitor;
            MemberAccessBindingExpressionVisitorFactory = memberAccessBindingExpressionVisitorFactory;
            OrderingExpressionVisitorFactory = orderingExpressionVisitorFactory;
            ProjectionExpressionVisitorFactory = projectionExpressionVisitorFactory;
            EntityQueryableExpressionVisitorFactory = entityQueryableExpressionVisitorFactory;
            QueryAnnotationExtractor = queryAnnotationExtractor;
            ResultOperatorHandler = resultOperatorHandler;
            EntityMaterializerSource = entityMaterializerSource;
            ExpressionPrinter = expressionPrinter;
        }

        protected virtual IModel Model { get; }
        protected virtual IQueryOptimizer QueryOptimizer { get; }
        protected virtual INavigationRewritingExpressionVisitorFactory NavigationRewritingExpressionVisitorFactory { get; }
        protected virtual ISubQueryMemberPushDownExpressionVisitor SubQueryMemberPushDownExpressionVisitor { get; }
        protected virtual IQuerySourceTracingExpressionVisitorFactory QuerySourceTracingExpressionVisitorFactory { get; }
        protected virtual IEntityResultFindingExpressionVisitorFactory EntityResultFindingExpressionVisitorFactory { get; }
        protected virtual ITaskBlockingExpressionVisitor TaskBlockingExpressionVisitor { get; }
        protected virtual IMemberAccessBindingExpressionVisitorFactory MemberAccessBindingExpressionVisitorFactory { get; }
        protected virtual IOrderingExpressionVisitorFactory OrderingExpressionVisitorFactory { get; }
        protected virtual IProjectionExpressionVisitorFactory ProjectionExpressionVisitorFactory { get; }
        protected virtual IEntityQueryableExpressionVisitorFactory EntityQueryableExpressionVisitorFactory { get; }
        protected virtual IExpressionPrinter ExpressionPrinter { get; }
        protected virtual IEntityMaterializerSource EntityMaterializerSource { get; }
        protected virtual IResultOperatorHandler ResultOperatorHandler { get; }
        protected virtual IQueryAnnotationExtractor QueryAnnotationExtractor { get; }

        public abstract EntityQueryModelVisitor Create(
            QueryCompilationContext queryCompilationContext,
            EntityQueryModelVisitor parentEntityQueryModelVisitor);
    }
}
