// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="RelationalQueryModelVisitorFactory" />
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
    public sealed class RelationalQueryModelVisitorDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="RelationalQueryModelVisitorFactory" />.
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
        public RelationalQueryModelVisitorDependencies(
            [NotNull] IRelationalResultOperatorHandler relationalResultOperatorHandler,
            [NotNull] IRelationalAnnotationProvider relationalAnnotationProvider,
            [NotNull] IIncludeExpressionVisitorFactory includeExpressionVisitorFactory,
            [NotNull] ISqlTranslatingExpressionVisitorFactory sqlTranslatingExpressionVisitorFactory,
            [NotNull] ICompositePredicateExpressionVisitorFactory compositePredicateExpressionVisitorFactory,
            [NotNull] IConditionalRemovingExpressionVisitorFactory conditionalRemovingExpressionVisitorFactory,
            [NotNull] IQueryFlattenerFactory queryFlattenerFactory,
            [NotNull] IDbContextOptions contextOptions)
        {
            Check.NotNull(relationalResultOperatorHandler, nameof(relationalResultOperatorHandler));
            Check.NotNull(relationalAnnotationProvider, nameof(relationalAnnotationProvider));
            Check.NotNull(includeExpressionVisitorFactory, nameof(includeExpressionVisitorFactory));
            Check.NotNull(sqlTranslatingExpressionVisitorFactory, nameof(sqlTranslatingExpressionVisitorFactory));
            Check.NotNull(compositePredicateExpressionVisitorFactory, nameof(compositePredicateExpressionVisitorFactory));
            Check.NotNull(conditionalRemovingExpressionVisitorFactory, nameof(conditionalRemovingExpressionVisitorFactory));
            Check.NotNull(queryFlattenerFactory, nameof(queryFlattenerFactory));
            Check.NotNull(contextOptions, nameof(contextOptions));

            RelationalResultOperatorHandler = relationalResultOperatorHandler;
            RelationalAnnotationProvider = relationalAnnotationProvider;
            IncludeExpressionVisitorFactory = includeExpressionVisitorFactory;
            SqlTranslatingExpressionVisitorFactory = sqlTranslatingExpressionVisitorFactory;
            CompositePredicateExpressionVisitorFactory = compositePredicateExpressionVisitorFactory;
            ConditionalRemovingExpressionVisitorFactory = conditionalRemovingExpressionVisitorFactory;
            QueryFlattenerFactory = queryFlattenerFactory;
            ContextOptions = contextOptions;
        }

        /// <summary>
        ///     Gets the <see cref="IRelationalResultOperatorHandler" /> to be used when processing a query.
        /// </summary>
        public IRelationalResultOperatorHandler RelationalResultOperatorHandler { get; }

        /// <summary>
        ///     Gets the relational annotation provider.
        /// </summary>
        public IRelationalAnnotationProvider RelationalAnnotationProvider { get; }

        /// <summary>
        ///     Gets the include expression visitor factory.
        /// </summary>
        public IIncludeExpressionVisitorFactory IncludeExpressionVisitorFactory { get; }

        /// <summary>
        ///     Gets the SQL translating expression visitor factory.
        /// </summary>
        public ISqlTranslatingExpressionVisitorFactory SqlTranslatingExpressionVisitorFactory { get; }

        /// <summary>
        ///     Gets the composite predicate expression visitor factory.
        /// </summary>
        public ICompositePredicateExpressionVisitorFactory CompositePredicateExpressionVisitorFactory { get; }

        /// <summary>
        ///     Gets the conditional removing expression visitor factory.
        /// </summary>
        public IConditionalRemovingExpressionVisitorFactory ConditionalRemovingExpressionVisitorFactory { get; }

        /// <summary>
        ///     Gets the query flattener factory.
        /// </summary>
        public IQueryFlattenerFactory QueryFlattenerFactory { get; }

        /// <summary>
        ///     Gets options for controlling the context.
        /// </summary>
        public IDbContextOptions ContextOptions { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="relationalResultOperatorHandler"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalQueryModelVisitorDependencies With([NotNull] IRelationalResultOperatorHandler relationalResultOperatorHandler)
            => new RelationalQueryModelVisitorDependencies(
                relationalResultOperatorHandler,
                RelationalAnnotationProvider,
                IncludeExpressionVisitorFactory,
                SqlTranslatingExpressionVisitorFactory,
                CompositePredicateExpressionVisitorFactory,
                ConditionalRemovingExpressionVisitorFactory,
                QueryFlattenerFactory,
                ContextOptions);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="relationalAnnotationProvider"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalQueryModelVisitorDependencies With([NotNull] IRelationalAnnotationProvider relationalAnnotationProvider)
            => new RelationalQueryModelVisitorDependencies(
                RelationalResultOperatorHandler,
                relationalAnnotationProvider,
                IncludeExpressionVisitorFactory,
                SqlTranslatingExpressionVisitorFactory,
                CompositePredicateExpressionVisitorFactory,
                ConditionalRemovingExpressionVisitorFactory,
                QueryFlattenerFactory,
                ContextOptions);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="includeExpressionVisitorFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalQueryModelVisitorDependencies With([NotNull] IIncludeExpressionVisitorFactory includeExpressionVisitorFactory)
            => new RelationalQueryModelVisitorDependencies(
                RelationalResultOperatorHandler,
                RelationalAnnotationProvider,
                includeExpressionVisitorFactory,
                SqlTranslatingExpressionVisitorFactory,
                CompositePredicateExpressionVisitorFactory,
                ConditionalRemovingExpressionVisitorFactory,
                QueryFlattenerFactory,
                ContextOptions);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="sqlTranslatingExpressionVisitorFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalQueryModelVisitorDependencies With([NotNull] ISqlTranslatingExpressionVisitorFactory sqlTranslatingExpressionVisitorFactory)
            => new RelationalQueryModelVisitorDependencies(
                RelationalResultOperatorHandler,
                RelationalAnnotationProvider,
                IncludeExpressionVisitorFactory,
                sqlTranslatingExpressionVisitorFactory,
                CompositePredicateExpressionVisitorFactory,
                ConditionalRemovingExpressionVisitorFactory,
                QueryFlattenerFactory,
                ContextOptions);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="compositePredicateExpressionVisitorFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalQueryModelVisitorDependencies With([NotNull] ICompositePredicateExpressionVisitorFactory compositePredicateExpressionVisitorFactory)
            => new RelationalQueryModelVisitorDependencies(
                RelationalResultOperatorHandler,
                RelationalAnnotationProvider,
                IncludeExpressionVisitorFactory,
                SqlTranslatingExpressionVisitorFactory,
                compositePredicateExpressionVisitorFactory,
                ConditionalRemovingExpressionVisitorFactory,
                QueryFlattenerFactory,
                ContextOptions);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="conditionalRemovingExpressionVisitorFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalQueryModelVisitorDependencies With([NotNull] IConditionalRemovingExpressionVisitorFactory conditionalRemovingExpressionVisitorFactory)
            => new RelationalQueryModelVisitorDependencies(
                RelationalResultOperatorHandler,
                RelationalAnnotationProvider,
                IncludeExpressionVisitorFactory,
                SqlTranslatingExpressionVisitorFactory,
                CompositePredicateExpressionVisitorFactory,
                conditionalRemovingExpressionVisitorFactory,
                QueryFlattenerFactory,
                ContextOptions);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="queryFlattenerFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalQueryModelVisitorDependencies With([NotNull] IQueryFlattenerFactory queryFlattenerFactory)
            => new RelationalQueryModelVisitorDependencies(
                RelationalResultOperatorHandler,
                RelationalAnnotationProvider,
                IncludeExpressionVisitorFactory,
                SqlTranslatingExpressionVisitorFactory,
                CompositePredicateExpressionVisitorFactory,
                ConditionalRemovingExpressionVisitorFactory,
                queryFlattenerFactory,
                ContextOptions);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="contextOptions"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalQueryModelVisitorDependencies With([NotNull] IDbContextOptions contextOptions)
            => new RelationalQueryModelVisitorDependencies(
                RelationalResultOperatorHandler,
                RelationalAnnotationProvider,
                IncludeExpressionVisitorFactory,
                SqlTranslatingExpressionVisitorFactory,
                CompositePredicateExpressionVisitorFactory,
                ConditionalRemovingExpressionVisitorFactory,
                QueryFlattenerFactory,
                contextOptions);
    }
}
