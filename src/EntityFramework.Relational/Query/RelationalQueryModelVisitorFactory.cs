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
    public class RelationalQueryModelVisitorFactory : IEntityQueryModelVisitorFactory
    {
        private readonly IModel _model;
        private readonly IQueryOptimizer _queryOptimizer;
        private readonly INavigationRewritingExpressionVisitorFactory _navigationRewritingExpressionVisitorFactory;
        private readonly ISubQueryMemberPushDownExpressionVisitor _subQueryMemberPushDownExpressionVisitor;
        private readonly IQuerySourceTracingExpressionVisitorFactory _querySourceTracingExpressionVisitorFactory;
        private readonly IEntityResultFindingExpressionVisitorFactory _entityResultFindingExpressionVisitorFactory;
        private readonly ITaskBlockingExpressionVisitor _taskBlockingExpressionVisitor;
        private readonly IMemberAccessBindingExpressionVisitorFactory _memberAccessBindingExpressionVisitorFactory;
        private readonly IOrderingExpressionVisitorFactory _orderingExpressionVisitorFactory;
        private readonly IProjectionExpressionVisitorFactory _projectionExpressionVisitorFactory;
        private readonly IEntityQueryableExpressionVisitorFactory _entityQueryableExpressionVisitorFactory;
        private readonly IQueryAnnotationExtractor _queryAnnotationExtractor;
        private readonly IResultOperatorHandler _resultOperatorHandler;
        private readonly IEntityMaterializerSource _entityMaterializerSource;
        private readonly IExpressionPrinter _expressionPrinter;
        private readonly IRelationalMetadataExtensionProvider _relationalMetadataExtensionProvider;
        private readonly IIncludeExpressionVisitorFactory _includeExpressionVisitorFactory;
        private readonly ISqlTranslatingExpressionVisitorFactory _sqlTranslatingExpressionVisitorFactory;
        private readonly ICompositePredicateExpressionVisitorFactory _compositePredicateExpressionVisitorFactory;
        private readonly IQueryFlatteningExpressionVisitorFactory _queryFlatteningExpressionVisitorFactory;
        private readonly IShapedQueryFindingExpressionVisitorFactory _shapedQueryFindingExpressionVisitorFactory;

        public RelationalQueryModelVisitorFactory(
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
            [NotNull] IExpressionPrinter expressionPrinter,
            [NotNull] IRelationalMetadataExtensionProvider relationalMetadataExtensionProvider,
            [NotNull] IIncludeExpressionVisitorFactory includeExpressionVisitorFactory,
            [NotNull] ISqlTranslatingExpressionVisitorFactory sqlTranslatingExpressionVisitorFactory,
            [NotNull] ICompositePredicateExpressionVisitorFactory compositePredicateExpressionVisitorFactory,
            [NotNull] IQueryFlatteningExpressionVisitorFactory queryFlatteningExpressionVisitorFactory,
            [NotNull] IShapedQueryFindingExpressionVisitorFactory shapedQueryFindingExpressionVisitorFactory)
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
            Check.NotNull(relationalMetadataExtensionProvider, nameof(relationalMetadataExtensionProvider));
            Check.NotNull(includeExpressionVisitorFactory, nameof(includeExpressionVisitorFactory));
            Check.NotNull(sqlTranslatingExpressionVisitorFactory, nameof(sqlTranslatingExpressionVisitorFactory));
            Check.NotNull(compositePredicateExpressionVisitorFactory, nameof(compositePredicateExpressionVisitorFactory));
            Check.NotNull(queryFlatteningExpressionVisitorFactory, nameof(queryFlatteningExpressionVisitorFactory));
            Check.NotNull(shapedQueryFindingExpressionVisitorFactory, nameof(shapedQueryFindingExpressionVisitorFactory));

            _model = model;
            _queryOptimizer = queryOptimizer;
            _navigationRewritingExpressionVisitorFactory = navigationRewritingExpressionVisitorFactory;
            _subQueryMemberPushDownExpressionVisitor = subQueryMemberPushDownExpressionVisitor;
            _querySourceTracingExpressionVisitorFactory = querySourceTracingExpressionVisitorFactory;
            _entityResultFindingExpressionVisitorFactory = entityResultFindingExpressionVisitorFactory;
            _taskBlockingExpressionVisitor = taskBlockingExpressionVisitor;
            _memberAccessBindingExpressionVisitorFactory = memberAccessBindingExpressionVisitorFactory;
            _orderingExpressionVisitorFactory = orderingExpressionVisitorFactory;
            _projectionExpressionVisitorFactory = projectionExpressionVisitorFactory;
            _entityQueryableExpressionVisitorFactory = entityQueryableExpressionVisitorFactory;
            _queryAnnotationExtractor = queryAnnotationExtractor;
            _resultOperatorHandler = resultOperatorHandler;
            _entityMaterializerSource = entityMaterializerSource;
            _expressionPrinter = expressionPrinter;
            _relationalMetadataExtensionProvider = relationalMetadataExtensionProvider;
            _includeExpressionVisitorFactory = includeExpressionVisitorFactory;
            _sqlTranslatingExpressionVisitorFactory = sqlTranslatingExpressionVisitorFactory;
            _compositePredicateExpressionVisitorFactory = compositePredicateExpressionVisitorFactory;
            _queryFlatteningExpressionVisitorFactory = queryFlatteningExpressionVisitorFactory;
            _shapedQueryFindingExpressionVisitorFactory = shapedQueryFindingExpressionVisitorFactory;
        }

        public virtual EntityQueryModelVisitor Create([NotNull] QueryCompilationContext queryCompilationContext)
            => Create(queryCompilationContext, null);

        public virtual EntityQueryModelVisitor Create(
            [NotNull] QueryCompilationContext queryCompilationContext,
            [CanBeNull] EntityQueryModelVisitor parentEntityQueryModelVisitor)
            =>
            new RelationalQueryModelVisitor(
                _model,
                _queryOptimizer,
                _navigationRewritingExpressionVisitorFactory,
                _subQueryMemberPushDownExpressionVisitor,
                _querySourceTracingExpressionVisitorFactory,
                _entityResultFindingExpressionVisitorFactory,
                _taskBlockingExpressionVisitor,
                _memberAccessBindingExpressionVisitorFactory,
                _orderingExpressionVisitorFactory,
                _projectionExpressionVisitorFactory,
                _entityQueryableExpressionVisitorFactory,
                _queryAnnotationExtractor,
                _resultOperatorHandler,
                _entityMaterializerSource,
                _expressionPrinter,
                _relationalMetadataExtensionProvider,
                _includeExpressionVisitorFactory,
                _sqlTranslatingExpressionVisitorFactory,
                _compositePredicateExpressionVisitorFactory,
                _queryFlatteningExpressionVisitorFactory,
                _shapedQueryFindingExpressionVisitorFactory,
                (RelationalQueryCompilationContext)Check.NotNull(queryCompilationContext, nameof(queryCompilationContext)),
                (RelationalQueryModelVisitor)parentEntityQueryModelVisitor);
    }
}
