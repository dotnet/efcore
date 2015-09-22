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
    public class RelationalQueryModelVisitorFactory : EntityQueryModelVisitorFactory
    {
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
            : base(
                model,
                queryOptimizer,
                navigationRewritingExpressionVisitorFactory,
                subQueryMemberPushDownExpressionVisitor,
                querySourceTracingExpressionVisitorFactory,
                entityResultFindingExpressionVisitorFactory,
                taskBlockingExpressionVisitor,
                memberAccessBindingExpressionVisitorFactory,
                orderingExpressionVisitorFactory,
                projectionExpressionVisitorFactory,
                entityQueryableExpressionVisitorFactory,
                queryAnnotationExtractor,
                resultOperatorHandler,
                entityMaterializerSource,
                expressionPrinter)
        {
            Check.NotNull(relationalMetadataExtensionProvider, nameof(relationalMetadataExtensionProvider));
            Check.NotNull(includeExpressionVisitorFactory, nameof(includeExpressionVisitorFactory));
            Check.NotNull(sqlTranslatingExpressionVisitorFactory, nameof(sqlTranslatingExpressionVisitorFactory));
            Check.NotNull(compositePredicateExpressionVisitorFactory, nameof(compositePredicateExpressionVisitorFactory));
            Check.NotNull(queryFlatteningExpressionVisitorFactory, nameof(queryFlatteningExpressionVisitorFactory));
            Check.NotNull(shapedQueryFindingExpressionVisitorFactory, nameof(shapedQueryFindingExpressionVisitorFactory));

            RelationalMetadataExtensionProvider = relationalMetadataExtensionProvider;
            IncludeExpressionVisitorFactory = includeExpressionVisitorFactory;
            SqlTranslatingExpressionVisitorFactory = sqlTranslatingExpressionVisitorFactory;
            CompositePredicateExpressionVisitorFactory = compositePredicateExpressionVisitorFactory;
            QueryFlatteningExpressionVisitorFactory = queryFlatteningExpressionVisitorFactory;
            ShapedQueryFindingExpressionVisitorFactory = shapedQueryFindingExpressionVisitorFactory;
        }

        protected virtual IRelationalMetadataExtensionProvider RelationalMetadataExtensionProvider { get; }
        protected virtual IIncludeExpressionVisitorFactory IncludeExpressionVisitorFactory { get; }
        protected virtual ISqlTranslatingExpressionVisitorFactory SqlTranslatingExpressionVisitorFactory { get; }
        protected virtual ICompositePredicateExpressionVisitorFactory CompositePredicateExpressionVisitorFactory { get; }
        protected virtual IQueryFlatteningExpressionVisitorFactory QueryFlatteningExpressionVisitorFactory { get; }
        protected virtual IShapedQueryFindingExpressionVisitorFactory ShapedQueryFindingExpressionVisitorFactory { get; }

        public override EntityQueryModelVisitor Create(
            QueryCompilationContext queryCompilationContext,
            EntityQueryModelVisitor parentEntityQueryModelVisitor)
            => new RelationalQueryModelVisitor(
                Model,
                QueryOptimizer,
                NavigationRewritingExpressionVisitorFactory,
                SubQueryMemberPushDownExpressionVisitor,
                QuerySourceTracingExpressionVisitorFactory,
                EntityResultFindingExpressionVisitorFactory,
                TaskBlockingExpressionVisitor,
                MemberAccessBindingExpressionVisitorFactory,
                OrderingExpressionVisitorFactory,
                ProjectionExpressionVisitorFactory,
                EntityQueryableExpressionVisitorFactory,
                QueryAnnotationExtractor,
                ResultOperatorHandler,
                EntityMaterializerSource,
                ExpressionPrinter,
                RelationalMetadataExtensionProvider,
                IncludeExpressionVisitorFactory,
                SqlTranslatingExpressionVisitorFactory,
                CompositePredicateExpressionVisitorFactory,
                QueryFlatteningExpressionVisitorFactory,
                ShapedQueryFindingExpressionVisitorFactory,
                (RelationalQueryCompilationContext)Check.NotNull(queryCompilationContext, nameof(queryCompilationContext)),
                (RelationalQueryModelVisitor)parentEntityQueryModelVisitor);
    }
}
