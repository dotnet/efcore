// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     A factory for instances of <see cref="EntityQueryModelVisitor" />.
    /// </summary>
    public class RelationalQueryModelVisitorFactory : EntityQueryModelVisitorFactory
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationalQueryModelVisitorFactory(
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
            [NotNull] IRelationalAnnotationProvider relationalAnnotationProvider,
            [NotNull] IIncludeExpressionVisitorFactory includeExpressionVisitorFactory,
            [NotNull] ISqlTranslatingExpressionVisitorFactory sqlTranslatingExpressionVisitorFactory,
            [NotNull] ICompositePredicateExpressionVisitorFactory compositePredicateExpressionVisitorFactory,
            [NotNull] IConditionalRemovingExpressionVisitorFactory conditionalRemovingExpressionVisitorFactory,
            [NotNull] IQueryFlattenerFactory queryFlattenerFactory,
            [NotNull] IDbContextOptions contextOptions)
            : base(
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
            Check.NotNull(relationalAnnotationProvider, nameof(relationalAnnotationProvider));
            Check.NotNull(includeExpressionVisitorFactory, nameof(includeExpressionVisitorFactory));
            Check.NotNull(sqlTranslatingExpressionVisitorFactory, nameof(sqlTranslatingExpressionVisitorFactory));
            Check.NotNull(compositePredicateExpressionVisitorFactory, nameof(compositePredicateExpressionVisitorFactory));
            Check.NotNull(conditionalRemovingExpressionVisitorFactory, nameof(conditionalRemovingExpressionVisitorFactory));
            Check.NotNull(queryFlattenerFactory, nameof(queryFlattenerFactory));
            Check.NotNull(contextOptions, nameof(contextOptions));

            RelationalAnnotationProvider = relationalAnnotationProvider;
            IncludeExpressionVisitorFactory = includeExpressionVisitorFactory;
            SqlTranslatingExpressionVisitorFactory = sqlTranslatingExpressionVisitorFactory;
            CompositePredicateExpressionVisitorFactory = compositePredicateExpressionVisitorFactory;
            ConditionalRemovingExpressionVisitorFactory = conditionalRemovingExpressionVisitorFactory;
            QueryFlattenerFactory = queryFlattenerFactory;
            ContextOptions = contextOptions;
        }

        /// <summary>
        ///     Gets the relational annotation provider.
        /// </summary>
        /// <value>
        ///     The relational annotation provider.
        /// </value>
        protected virtual IRelationalAnnotationProvider RelationalAnnotationProvider { get; }

        /// <summary>
        ///     Gets the include expression visitor factory.
        /// </summary>
        /// <value>
        ///     The include expression visitor factory.
        /// </value>
        protected virtual IIncludeExpressionVisitorFactory IncludeExpressionVisitorFactory { get; }

        /// <summary>
        ///     Gets the SQL translating expression visitor factory.
        /// </summary>
        /// <value>
        ///     The SQL translating expression visitor factory.
        /// </value>
        protected virtual ISqlTranslatingExpressionVisitorFactory SqlTranslatingExpressionVisitorFactory { get; }

        /// <summary>
        ///     Gets the composite predicate expression visitor factory.
        /// </summary>
        /// <value>
        ///     The composite predicate expression visitor factory.
        /// </value>
        protected virtual ICompositePredicateExpressionVisitorFactory CompositePredicateExpressionVisitorFactory { get; }

        /// <summary>
        ///     Gets the conditional removing expression visitor factory.
        /// </summary>
        /// <value>
        ///     The conditional removing expression visitor factory.
        /// </value>
        protected virtual IConditionalRemovingExpressionVisitorFactory ConditionalRemovingExpressionVisitorFactory { get; }

        /// <summary>
        ///     Gets the query flattener factory.
        /// </summary>
        /// <value>
        ///     The query flattener factory.
        /// </value>
        protected virtual IQueryFlattenerFactory QueryFlattenerFactory { get; }

        /// <summary>
        ///     Gets options for controlling the context.
        /// </summary>
        /// <value>
        ///     Options that control the context.
        /// </value>
        protected virtual IDbContextOptions ContextOptions { get; }

        /// <summary>
        ///     Creates a new EntityQueryModelVisitor.
        /// </summary>
        /// <param name="queryCompilationContext"> Compilation context for the query. </param>
        /// <param name="parentEntityQueryModelVisitor"> The visitor for the outer query. </param>
        /// <returns>
        ///     An EntityQueryModelVisitor.
        /// </returns>
        public override EntityQueryModelVisitor Create(
            QueryCompilationContext queryCompilationContext,
            EntityQueryModelVisitor parentEntityQueryModelVisitor)
            => new RelationalQueryModelVisitor(
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
                RelationalAnnotationProvider,
                IncludeExpressionVisitorFactory,
                SqlTranslatingExpressionVisitorFactory,
                CompositePredicateExpressionVisitorFactory,
                ConditionalRemovingExpressionVisitorFactory,
                QueryFlattenerFactory,
                ContextOptions,
                (RelationalQueryCompilationContext)Check.NotNull(queryCompilationContext, nameof(queryCompilationContext)),
                (RelationalQueryModelVisitor)parentEntityQueryModelVisitor);
    }
}
