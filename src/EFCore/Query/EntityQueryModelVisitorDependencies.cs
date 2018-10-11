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
    ///         Service dependencies parameter class for <see cref="EntityQueryModelVisitor" />
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         Do not construct instances of this class directly from either provider or application code as the
    ///         constructor signature may change as new dependencies are added. Instead, use this type in
    ///         your constructor so that an instance will be created and injected automatically by the
    ///         dependency injection container. To create an instance with some dependent services replaced,
    ///         first resolve the object from the dependency injection container, then replace selected
    ///         services using the 'With...' methods. Do not call the constructor at any point in this process.
    ///     </para>
    /// </summary>
    public sealed class EntityQueryModelVisitorDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="EntityQueryModelVisitorFactory" />.
        ///     </para>
        ///     <para>
        ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///         directly from your code. This API may change or be removed in future releases.
        ///     </para>
        ///     <para>
        ///         Do not call this constructor directly from either provider or application code as it may change
        ///         as new dependencies are added. Instead, use this type in your constructor so that an instance
        ///         will be created and injected automatically by the dependency injection container. To create
        ///         an instance with some dependent services replaced, first resolve the object from the dependency
        ///         injection container, then replace selected services using the 'With...' methods. Do not call
        ///         the constructor at any point in this process.
        ///     </para>
        /// </summary>
        /// <param name="queryOptimizer"> The <see cref="IQueryOptimizer" /> to be used when processing the query. </param>
        /// <param name="navigationRewritingExpressionVisitorFactory">
        ///     The <see cref="INavigationRewritingExpressionVisitorFactory" /> to be used when
        ///     processing the query.
        /// </param>
        /// <param name="querySourceTracingExpressionVisitorFactory">
        ///     The <see cref="IQuerySourceTracingExpressionVisitorFactory" /> to be used when
        ///     processing the query.
        /// </param>
        /// <param name="entityResultFindingExpressionVisitorFactory">
        ///     The <see cref="IEntityResultFindingExpressionVisitorFactory" /> to be used when
        ///     processing the query.
        /// </param>
        /// <param name="eagerLoadingExpressionVisitorFactory">
        ///     The <see cref="IEagerLoadingExpressionVisitorFactory" /> to be used when
        ///     processing the query.
        /// </param>
        /// <param name="taskBlockingExpressionVisitor"> The <see cref="ITaskBlockingExpressionVisitor" /> to be used when processing the query. </param>
        /// <param name="memberAccessBindingExpressionVisitorFactory">
        ///     The <see cref="IMemberAccessBindingExpressionVisitorFactory" /> to be used when
        ///     processing the query.
        /// </param>
        /// <param name="projectionExpressionVisitorFactory">
        ///     The <see cref="IProjectionExpressionVisitorFactory" /> to be used when processing the
        ///     query.
        /// </param>
        /// <param name="entityQueryableExpressionVisitorFactory">
        ///     The <see cref="IEntityQueryableExpressionVisitorFactory" /> to be used when
        ///     processing the query.
        /// </param>
        /// <param name="queryAnnotationExtractor"> The <see cref="IQueryAnnotationExtractor" /> to be used when processing the query. </param>
        /// <param name="resultOperatorHandler"> The <see cref="IResultOperatorHandler" /> to be used when processing the query. </param>
        /// <param name="entityMaterializerSource"> The <see cref="IEntityMaterializerSource" /> to be used when processing the query. </param>
        /// <param name="expressionPrinter"> The <see cref="IExpressionPrinter" /> to be used when processing the query. </param>
        /// <param name="queryModelGenerator"> The <see cref="IQueryModelGenerator" /> to be used when processing the query. </param>
        public EntityQueryModelVisitorDependencies(
            [NotNull] IQueryOptimizer queryOptimizer,
            [NotNull] INavigationRewritingExpressionVisitorFactory navigationRewritingExpressionVisitorFactory,
            [NotNull] IQuerySourceTracingExpressionVisitorFactory querySourceTracingExpressionVisitorFactory,
            [NotNull] IEntityResultFindingExpressionVisitorFactory entityResultFindingExpressionVisitorFactory,
            [NotNull] IEagerLoadingExpressionVisitorFactory eagerLoadingExpressionVisitorFactory,
            [NotNull] ITaskBlockingExpressionVisitor taskBlockingExpressionVisitor,
            [NotNull] IMemberAccessBindingExpressionVisitorFactory memberAccessBindingExpressionVisitorFactory,
            [NotNull] IProjectionExpressionVisitorFactory projectionExpressionVisitorFactory,
            [NotNull] IEntityQueryableExpressionVisitorFactory entityQueryableExpressionVisitorFactory,
            [NotNull] IQueryAnnotationExtractor queryAnnotationExtractor,
            [NotNull] IResultOperatorHandler resultOperatorHandler,
            [NotNull] IEntityMaterializerSource entityMaterializerSource,
            [NotNull] IExpressionPrinter expressionPrinter,
            [NotNull] IQueryModelGenerator queryModelGenerator)
        {
            Check.NotNull(queryOptimizer, nameof(queryOptimizer));
            Check.NotNull(navigationRewritingExpressionVisitorFactory, nameof(navigationRewritingExpressionVisitorFactory));
            Check.NotNull(querySourceTracingExpressionVisitorFactory, nameof(querySourceTracingExpressionVisitorFactory));
            Check.NotNull(entityResultFindingExpressionVisitorFactory, nameof(entityResultFindingExpressionVisitorFactory));
            Check.NotNull(eagerLoadingExpressionVisitorFactory, nameof(eagerLoadingExpressionVisitorFactory));
            Check.NotNull(taskBlockingExpressionVisitor, nameof(taskBlockingExpressionVisitor));
            Check.NotNull(memberAccessBindingExpressionVisitorFactory, nameof(memberAccessBindingExpressionVisitorFactory));
            Check.NotNull(projectionExpressionVisitorFactory, nameof(projectionExpressionVisitorFactory));
            Check.NotNull(entityQueryableExpressionVisitorFactory, nameof(entityQueryableExpressionVisitorFactory));
            Check.NotNull(queryAnnotationExtractor, nameof(queryAnnotationExtractor));
            Check.NotNull(resultOperatorHandler, nameof(resultOperatorHandler));
            Check.NotNull(entityMaterializerSource, nameof(entityMaterializerSource));
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));
            Check.NotNull(queryModelGenerator, nameof(queryModelGenerator));

            QueryOptimizer = queryOptimizer;
            NavigationRewritingExpressionVisitorFactory = navigationRewritingExpressionVisitorFactory;
            QuerySourceTracingExpressionVisitorFactory = querySourceTracingExpressionVisitorFactory;
            EntityResultFindingExpressionVisitorFactory = entityResultFindingExpressionVisitorFactory;
            EagerLoadingExpressionVisitorFactory = eagerLoadingExpressionVisitorFactory;
            TaskBlockingExpressionVisitor = taskBlockingExpressionVisitor;
            MemberAccessBindingExpressionVisitorFactory = memberAccessBindingExpressionVisitorFactory;
            ProjectionExpressionVisitorFactory = projectionExpressionVisitorFactory;
            EntityQueryableExpressionVisitorFactory = entityQueryableExpressionVisitorFactory;
            QueryAnnotationExtractor = queryAnnotationExtractor;
            ResultOperatorHandler = resultOperatorHandler;
            EntityMaterializerSource = entityMaterializerSource;
            ExpressionPrinter = expressionPrinter;
            QueryModelGenerator = queryModelGenerator;
        }

        /// <summary>
        ///     Gets the <see cref="IQueryOptimizer" /> to be used when processing a query.
        /// </summary>
        public IQueryOptimizer QueryOptimizer { get; }

        /// <summary>
        ///     Gets the <see cref="INavigationRewritingExpressionVisitorFactory" /> to be used when processing a query.
        /// </summary>
        public INavigationRewritingExpressionVisitorFactory NavigationRewritingExpressionVisitorFactory { get; }

        /// <summary>
        ///     Gets the <see cref="IQuerySourceTracingExpressionVisitorFactory" /> to be used when processing a query.
        /// </summary>
        public IQuerySourceTracingExpressionVisitorFactory QuerySourceTracingExpressionVisitorFactory { get; }

        /// <summary>
        ///     Gets the <see cref="IEntityResultFindingExpressionVisitorFactory" /> to be used when processing a query.
        /// </summary>
        public IEntityResultFindingExpressionVisitorFactory EntityResultFindingExpressionVisitorFactory { get; }

        /// <summary>
        ///     Gets the <see cref="IEagerLoadingExpressionVisitorFactory" /> to be used when processing a query.
        /// </summary>
        public IEagerLoadingExpressionVisitorFactory EagerLoadingExpressionVisitorFactory { get; }

        /// <summary>
        ///     Gets the <see cref="ITaskBlockingExpressionVisitor" /> to be used when processing a query.
        /// </summary>
        public ITaskBlockingExpressionVisitor TaskBlockingExpressionVisitor { get; }

        /// <summary>
        ///     Gets the <see cref="IMemberAccessBindingExpressionVisitorFactory" /> to be used when processing a query.
        /// </summary>
        public IMemberAccessBindingExpressionVisitorFactory MemberAccessBindingExpressionVisitorFactory { get; }

        /// <summary>
        ///     Gets the <see cref="IProjectionExpressionVisitorFactory" /> to be used when processing a query.
        /// </summary>
        public IProjectionExpressionVisitorFactory ProjectionExpressionVisitorFactory { get; }

        /// <summary>
        ///     Gets the <see cref="IEntityQueryableExpressionVisitorFactory" /> to be used when processing a query.
        /// </summary>
        public IEntityQueryableExpressionVisitorFactory EntityQueryableExpressionVisitorFactory { get; }

        /// <summary>
        ///     Gets the <see cref="IExpressionPrinter" /> to be used when processing a query.
        /// </summary>
        public IExpressionPrinter ExpressionPrinter { get; }

        /// <summary>
        ///     Gets the <see cref="IEntityMaterializerSource" /> to be used when processing a query.
        /// </summary>
        public IEntityMaterializerSource EntityMaterializerSource { get; }

        /// <summary>
        ///     Gets the <see cref="IResultOperatorHandler" /> to be used when processing a query.
        /// </summary>
        public IResultOperatorHandler ResultOperatorHandler { get; }

        /// <summary>
        ///     Gets the <see cref="IQueryAnnotationExtractor" /> to be used when processing a query.
        /// </summary>
        public IQueryAnnotationExtractor QueryAnnotationExtractor { get; }

        /// <summary>
        ///     Gets the <see cref="IQueryModelGenerator" /> to be used when processing a query.
        /// </summary>
        public IQueryModelGenerator QueryModelGenerator { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="resultOperatorHandler"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public EntityQueryModelVisitorDependencies With([NotNull] IResultOperatorHandler resultOperatorHandler)
            => new EntityQueryModelVisitorDependencies(
                QueryOptimizer,
                NavigationRewritingExpressionVisitorFactory,
                QuerySourceTracingExpressionVisitorFactory,
                EntityResultFindingExpressionVisitorFactory,
                EagerLoadingExpressionVisitorFactory,
                TaskBlockingExpressionVisitor,
                MemberAccessBindingExpressionVisitorFactory,
                ProjectionExpressionVisitorFactory,
                EntityQueryableExpressionVisitorFactory,
                QueryAnnotationExtractor,
                resultOperatorHandler,
                EntityMaterializerSource,
                ExpressionPrinter,
                QueryModelGenerator);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="queryOptimizer"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public EntityQueryModelVisitorDependencies With([NotNull] IQueryOptimizer queryOptimizer)
            => new EntityQueryModelVisitorDependencies(
                queryOptimizer,
                NavigationRewritingExpressionVisitorFactory,
                QuerySourceTracingExpressionVisitorFactory,
                EntityResultFindingExpressionVisitorFactory,
                EagerLoadingExpressionVisitorFactory,
                TaskBlockingExpressionVisitor,
                MemberAccessBindingExpressionVisitorFactory,
                ProjectionExpressionVisitorFactory,
                EntityQueryableExpressionVisitorFactory,
                QueryAnnotationExtractor,
                ResultOperatorHandler,
                EntityMaterializerSource,
                ExpressionPrinter,
                QueryModelGenerator);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="navigationRewritingExpressionVisitorFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public EntityQueryModelVisitorDependencies With([NotNull] INavigationRewritingExpressionVisitorFactory navigationRewritingExpressionVisitorFactory)
            => new EntityQueryModelVisitorDependencies(
                QueryOptimizer,
                navigationRewritingExpressionVisitorFactory,
                QuerySourceTracingExpressionVisitorFactory,
                EntityResultFindingExpressionVisitorFactory,
                EagerLoadingExpressionVisitorFactory,
                TaskBlockingExpressionVisitor,
                MemberAccessBindingExpressionVisitorFactory,
                ProjectionExpressionVisitorFactory,
                EntityQueryableExpressionVisitorFactory,
                QueryAnnotationExtractor,
                ResultOperatorHandler,
                EntityMaterializerSource,
                ExpressionPrinter,
                QueryModelGenerator);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="querySourceTracingExpressionVisitorFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public EntityQueryModelVisitorDependencies With([NotNull] IQuerySourceTracingExpressionVisitorFactory querySourceTracingExpressionVisitorFactory)
            => new EntityQueryModelVisitorDependencies(
                QueryOptimizer,
                NavigationRewritingExpressionVisitorFactory,
                querySourceTracingExpressionVisitorFactory,
                EntityResultFindingExpressionVisitorFactory,
                EagerLoadingExpressionVisitorFactory,
                TaskBlockingExpressionVisitor,
                MemberAccessBindingExpressionVisitorFactory,
                ProjectionExpressionVisitorFactory,
                EntityQueryableExpressionVisitorFactory,
                QueryAnnotationExtractor,
                ResultOperatorHandler,
                EntityMaterializerSource,
                ExpressionPrinter,
                QueryModelGenerator);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="entityResultFindingExpressionVisitorFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public EntityQueryModelVisitorDependencies With([NotNull] IEntityResultFindingExpressionVisitorFactory entityResultFindingExpressionVisitorFactory)
            => new EntityQueryModelVisitorDependencies(
                QueryOptimizer,
                NavigationRewritingExpressionVisitorFactory,
                QuerySourceTracingExpressionVisitorFactory,
                entityResultFindingExpressionVisitorFactory,
                EagerLoadingExpressionVisitorFactory,
                TaskBlockingExpressionVisitor,
                MemberAccessBindingExpressionVisitorFactory,
                ProjectionExpressionVisitorFactory,
                EntityQueryableExpressionVisitorFactory,
                QueryAnnotationExtractor,
                ResultOperatorHandler,
                EntityMaterializerSource,
                ExpressionPrinter,
                QueryModelGenerator);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="eagerLoadingExpressionVisitorFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public EntityQueryModelVisitorDependencies With([NotNull] IEagerLoadingExpressionVisitorFactory eagerLoadingExpressionVisitorFactory)
            => new EntityQueryModelVisitorDependencies(
                QueryOptimizer,
                NavigationRewritingExpressionVisitorFactory,
                QuerySourceTracingExpressionVisitorFactory,
                EntityResultFindingExpressionVisitorFactory,
                eagerLoadingExpressionVisitorFactory,
                TaskBlockingExpressionVisitor,
                MemberAccessBindingExpressionVisitorFactory,
                ProjectionExpressionVisitorFactory,
                EntityQueryableExpressionVisitorFactory,
                QueryAnnotationExtractor,
                ResultOperatorHandler,
                EntityMaterializerSource,
                ExpressionPrinter,
                QueryModelGenerator);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="taskBlockingExpressionVisitor"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public EntityQueryModelVisitorDependencies With([NotNull] ITaskBlockingExpressionVisitor taskBlockingExpressionVisitor)
            => new EntityQueryModelVisitorDependencies(
                QueryOptimizer,
                NavigationRewritingExpressionVisitorFactory,
                QuerySourceTracingExpressionVisitorFactory,
                EntityResultFindingExpressionVisitorFactory,
                EagerLoadingExpressionVisitorFactory,
                taskBlockingExpressionVisitor,
                MemberAccessBindingExpressionVisitorFactory,
                ProjectionExpressionVisitorFactory,
                EntityQueryableExpressionVisitorFactory,
                QueryAnnotationExtractor,
                ResultOperatorHandler,
                EntityMaterializerSource,
                ExpressionPrinter,
                QueryModelGenerator);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="memberAccessBindingExpressionVisitorFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public EntityQueryModelVisitorDependencies With([NotNull] IMemberAccessBindingExpressionVisitorFactory memberAccessBindingExpressionVisitorFactory)
            => new EntityQueryModelVisitorDependencies(
                QueryOptimizer,
                NavigationRewritingExpressionVisitorFactory,
                QuerySourceTracingExpressionVisitorFactory,
                EntityResultFindingExpressionVisitorFactory,
                EagerLoadingExpressionVisitorFactory,
                TaskBlockingExpressionVisitor,
                memberAccessBindingExpressionVisitorFactory,
                ProjectionExpressionVisitorFactory,
                EntityQueryableExpressionVisitorFactory,
                QueryAnnotationExtractor,
                ResultOperatorHandler,
                EntityMaterializerSource,
                ExpressionPrinter,
                QueryModelGenerator);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="projectionExpressionVisitorFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public EntityQueryModelVisitorDependencies With([NotNull] IProjectionExpressionVisitorFactory projectionExpressionVisitorFactory)
            => new EntityQueryModelVisitorDependencies(
                QueryOptimizer,
                NavigationRewritingExpressionVisitorFactory,
                QuerySourceTracingExpressionVisitorFactory,
                EntityResultFindingExpressionVisitorFactory,
                EagerLoadingExpressionVisitorFactory,
                TaskBlockingExpressionVisitor,
                MemberAccessBindingExpressionVisitorFactory,
                projectionExpressionVisitorFactory,
                EntityQueryableExpressionVisitorFactory,
                QueryAnnotationExtractor,
                ResultOperatorHandler,
                EntityMaterializerSource,
                ExpressionPrinter,
                QueryModelGenerator);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="entityQueryableExpressionVisitorFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public EntityQueryModelVisitorDependencies With([NotNull] IEntityQueryableExpressionVisitorFactory entityQueryableExpressionVisitorFactory)
            => new EntityQueryModelVisitorDependencies(
                QueryOptimizer,
                NavigationRewritingExpressionVisitorFactory,
                QuerySourceTracingExpressionVisitorFactory,
                EntityResultFindingExpressionVisitorFactory,
                EagerLoadingExpressionVisitorFactory,
                TaskBlockingExpressionVisitor,
                MemberAccessBindingExpressionVisitorFactory,
                ProjectionExpressionVisitorFactory,
                entityQueryableExpressionVisitorFactory,
                QueryAnnotationExtractor,
                ResultOperatorHandler,
                EntityMaterializerSource,
                ExpressionPrinter,
                QueryModelGenerator);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="queryAnnotationExtractor"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public EntityQueryModelVisitorDependencies With([NotNull] IQueryAnnotationExtractor queryAnnotationExtractor)
            => new EntityQueryModelVisitorDependencies(
                QueryOptimizer,
                NavigationRewritingExpressionVisitorFactory,
                QuerySourceTracingExpressionVisitorFactory,
                EntityResultFindingExpressionVisitorFactory,
                EagerLoadingExpressionVisitorFactory,
                TaskBlockingExpressionVisitor,
                MemberAccessBindingExpressionVisitorFactory,
                ProjectionExpressionVisitorFactory,
                EntityQueryableExpressionVisitorFactory,
                queryAnnotationExtractor,
                ResultOperatorHandler,
                EntityMaterializerSource,
                ExpressionPrinter,
                QueryModelGenerator);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="entityMaterializerSource"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public EntityQueryModelVisitorDependencies With([NotNull] IEntityMaterializerSource entityMaterializerSource)
            => new EntityQueryModelVisitorDependencies(
                QueryOptimizer,
                NavigationRewritingExpressionVisitorFactory,
                QuerySourceTracingExpressionVisitorFactory,
                EntityResultFindingExpressionVisitorFactory,
                EagerLoadingExpressionVisitorFactory,
                TaskBlockingExpressionVisitor,
                MemberAccessBindingExpressionVisitorFactory,
                ProjectionExpressionVisitorFactory,
                EntityQueryableExpressionVisitorFactory,
                QueryAnnotationExtractor,
                ResultOperatorHandler,
                entityMaterializerSource,
                ExpressionPrinter,
                QueryModelGenerator);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="expressionPrinter"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public EntityQueryModelVisitorDependencies With([NotNull] IExpressionPrinter expressionPrinter)
            => new EntityQueryModelVisitorDependencies(
                QueryOptimizer,
                NavigationRewritingExpressionVisitorFactory,
                QuerySourceTracingExpressionVisitorFactory,
                EntityResultFindingExpressionVisitorFactory,
                EagerLoadingExpressionVisitorFactory,
                TaskBlockingExpressionVisitor,
                MemberAccessBindingExpressionVisitorFactory,
                ProjectionExpressionVisitorFactory,
                EntityQueryableExpressionVisitorFactory,
                QueryAnnotationExtractor,
                ResultOperatorHandler,
                EntityMaterializerSource,
                expressionPrinter,
                QueryModelGenerator);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="queryModelGenerator"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public EntityQueryModelVisitorDependencies With([NotNull] IQueryModelGenerator queryModelGenerator)
            => new EntityQueryModelVisitorDependencies(
                QueryOptimizer,
                NavigationRewritingExpressionVisitorFactory,
                QuerySourceTracingExpressionVisitorFactory,
                EntityResultFindingExpressionVisitorFactory,
                EagerLoadingExpressionVisitorFactory,
                TaskBlockingExpressionVisitor,
                MemberAccessBindingExpressionVisitorFactory,
                ProjectionExpressionVisitorFactory,
                EntityQueryableExpressionVisitorFactory,
                QueryAnnotationExtractor,
                ResultOperatorHandler,
                EntityMaterializerSource,
                ExpressionPrinter,
                queryModelGenerator);
    }
}
