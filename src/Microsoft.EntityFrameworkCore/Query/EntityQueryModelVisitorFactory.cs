// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         Creates instances of <see cref="EntityQueryModelVisitor"/>.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public abstract class EntityQueryModelVisitorFactory : IEntityQueryModelVisitorFactory
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityQueryModelVisitorFactory"/> class.
        /// </summary>
        /// <param name="queryOptimizer"> The <see cref="IQueryOptimizer"/> to be used when processing a query. </param>
        /// <param name="navigationRewritingExpressionVisitorFactory"> The <see cref="INavigationRewritingExpressionVisitorFactory"/> to be used when processing a query. </param>
        /// <param name="subQueryMemberPushDownExpressionVisitor"> The <see cref="ISubQueryMemberPushDownExpressionVisitor"/> to be used when processing a query. </param>
        /// <param name="querySourceTracingExpressionVisitorFactory"> The <see cref="IQuerySourceTracingExpressionVisitorFactory"/> to be used when processing a query. </param>
        /// <param name="entityResultFindingExpressionVisitorFactory"> The <see cref="IEntityResultFindingExpressionVisitorFactory"/> to be used when processing a query. </param>
        /// <param name="taskBlockingExpressionVisitor"> The <see cref="ITaskBlockingExpressionVisitor"/> to be used when processing a query. </param>
        /// <param name="memberAccessBindingExpressionVisitorFactory"> The <see cref="IMemberAccessBindingExpressionVisitorFactory"/> to be used when processing a query. </param>
        /// <param name="orderingExpressionVisitorFactory"> The <see cref="IOrderingExpressionVisitorFactory"/> to be used when processing a query. </param>
        /// <param name="projectionExpressionVisitorFactory"> The <see cref="IProjectionExpressionVisitorFactory"/> to be used when processing a query. </param>
        /// <param name="entityQueryableExpressionVisitorFactory"> The <see cref="IEntityQueryableExpressionVisitorFactory"/> to be used when processing a query. </param>
        /// <param name="queryAnnotationExtractor"> The <see cref="IQueryAnnotationExtractor"/> to be used when processing a query. </param>
        /// <param name="resultOperatorHandler"> The <see cref="IResultOperatorHandler"/> to be used when processing a query. </param>
        /// <param name="entityMaterializerSource"> The <see cref="IEntityMaterializerSource"/> to be used when processing a query. </param>
        /// <param name="expressionPrinter"> The <see cref="IExpressionPrinter"/> to be used when processing a query. </param>
        protected EntityQueryModelVisitorFactory(
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

        /// <summary>
        ///     Gets the <see cref="IQueryOptimizer"/> to be used when processing a query.
        /// </summary>
        protected virtual IQueryOptimizer QueryOptimizer { get; }

        /// <summary>
        ///     Gets the <see cref="INavigationRewritingExpressionVisitorFactory"/> to be used when processing a query.
        /// </summary>
        protected virtual INavigationRewritingExpressionVisitorFactory NavigationRewritingExpressionVisitorFactory { get; }

        /// <summary>
        ///     Gets the <see cref="ISubQueryMemberPushDownExpressionVisitor"/> to be used when processing a query.
        /// </summary>
        protected virtual ISubQueryMemberPushDownExpressionVisitor SubQueryMemberPushDownExpressionVisitor { get; }

        /// <summary>
        ///     Gets the <see cref="IQuerySourceTracingExpressionVisitorFactory"/> to be used when processing a query.
        /// </summary>
        protected virtual IQuerySourceTracingExpressionVisitorFactory QuerySourceTracingExpressionVisitorFactory { get; }

        /// <summary>
        ///     Gets the <see cref="IEntityResultFindingExpressionVisitorFactory"/> to be used when processing a query.
        /// </summary>
        protected virtual IEntityResultFindingExpressionVisitorFactory EntityResultFindingExpressionVisitorFactory { get; }

        /// <summary>
        ///     Gets the <see cref="ITaskBlockingExpressionVisitor"/> to be used when processing a query.
        /// </summary>
        protected virtual ITaskBlockingExpressionVisitor TaskBlockingExpressionVisitor { get; }

        /// <summary>
        ///     Gets the <see cref="IMemberAccessBindingExpressionVisitorFactory"/> to be used when processing a query.
        /// </summary>
        protected virtual IMemberAccessBindingExpressionVisitorFactory MemberAccessBindingExpressionVisitorFactory { get; }

        /// <summary>
        ///     Gets the <see cref="IOrderingExpressionVisitorFactory"/> to be used when processing a query.
        /// </summary>
        protected virtual IOrderingExpressionVisitorFactory OrderingExpressionVisitorFactory { get; }

        /// <summary>
        ///     Gets the <see cref="IProjectionExpressionVisitorFactory"/> to be used when processing a query.
        /// </summary>
        protected virtual IProjectionExpressionVisitorFactory ProjectionExpressionVisitorFactory { get; }

        /// <summary>
        ///     Gets the <see cref="IEntityQueryableExpressionVisitorFactory"/> to be used when processing a query.
        /// </summary>
        protected virtual IEntityQueryableExpressionVisitorFactory EntityQueryableExpressionVisitorFactory { get; }

        /// <summary>
        ///     Gets the <see cref="IExpressionPrinter"/> to be used when processing a query.
        /// </summary>
        protected virtual IExpressionPrinter ExpressionPrinter { get; }

        /// <summary>
        ///     Gets the <see cref="IEntityMaterializerSource"/> to be used when processing a query.
        /// </summary>
        protected virtual IEntityMaterializerSource EntityMaterializerSource { get; }

        /// <summary>
        ///     Gets the <see cref="IResultOperatorHandler"/> to be used when processing a query.
        /// </summary>
        protected virtual IResultOperatorHandler ResultOperatorHandler { get; }

        /// <summary>
        ///     Gets the <see cref="IQueryAnnotationExtractor"/> to be used when processing a query.
        /// </summary>
        protected virtual IQueryAnnotationExtractor QueryAnnotationExtractor { get; }

        /// <summary>
        ///     Creates a new <see cref="EntityQueryModelVisitor"/>.
        /// </summary>
        /// <param name="queryCompilationContext">
        ///     Compilation context for the query.
        /// </param>
        /// <param name="parentEntityQueryModelVisitor">
        ///     The visitor for the outer query.
        /// </param>
        /// <returns> The new created visitor. </returns>
        public abstract EntityQueryModelVisitor Create(
            QueryCompilationContext queryCompilationContext,
            EntityQueryModelVisitor parentEntityQueryModelVisitor);
    }
}
