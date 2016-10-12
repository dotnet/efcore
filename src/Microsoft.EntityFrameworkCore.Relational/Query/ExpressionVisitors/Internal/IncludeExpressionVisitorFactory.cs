// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class IncludeExpressionVisitorFactory : IIncludeExpressionVisitorFactory
    {
        private readonly ISelectExpressionFactory _selectExpressionFactory;
        private readonly ICompositePredicateExpressionVisitorFactory _compositePredicateExpressionVisitorFactory;
        private readonly IMaterializerFactory _materializerFactory;
        private readonly IShaperCommandContextFactory _shaperCommandContextFactory;
        private readonly IRelationalAnnotationProvider _relationalAnnotationProvider;
        private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IncludeExpressionVisitorFactory(
            [NotNull] ISelectExpressionFactory selectExpressionFactory,
            [NotNull] ICompositePredicateExpressionVisitorFactory compositePredicateExpressionVisitorFactory,
            [NotNull] IMaterializerFactory materializerFactory,
            [NotNull] IShaperCommandContextFactory shaperCommandContextFactory,
            [NotNull] IRelationalAnnotationProvider relationalAnnotationProvider,
            [NotNull] IQuerySqlGeneratorFactory querySqlGeneratorFactory)
        {
            Check.NotNull(selectExpressionFactory, nameof(selectExpressionFactory));
            Check.NotNull(compositePredicateExpressionVisitorFactory, nameof(compositePredicateExpressionVisitorFactory));
            Check.NotNull(materializerFactory, nameof(materializerFactory));
            Check.NotNull(shaperCommandContextFactory, nameof(shaperCommandContextFactory));
            Check.NotNull(relationalAnnotationProvider, nameof(relationalAnnotationProvider));
            Check.NotNull(querySqlGeneratorFactory, nameof(querySqlGeneratorFactory));

            _selectExpressionFactory = selectExpressionFactory;
            _compositePredicateExpressionVisitorFactory = compositePredicateExpressionVisitorFactory;
            _materializerFactory = materializerFactory;
            _shaperCommandContextFactory = shaperCommandContextFactory;
            _relationalAnnotationProvider = relationalAnnotationProvider;
            _querySqlGeneratorFactory = querySqlGeneratorFactory;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ExpressionVisitor Create(
            IQuerySource querySource,
            IReadOnlyList<INavigation> navigationPath,
            RelationalQueryCompilationContext relationalQueryCompilationContext,
            IReadOnlyList<int> queryIndexes,
            bool querySourceRequiresTracking)
            => new IncludeExpressionVisitor(
                _selectExpressionFactory,
                _compositePredicateExpressionVisitorFactory,
                _materializerFactory,
                _shaperCommandContextFactory,
                _relationalAnnotationProvider,
                _querySqlGeneratorFactory,
                Check.NotNull(querySource, nameof(querySource)),
                Check.NotNull(navigationPath, nameof(navigationPath)),
                Check.NotNull(relationalQueryCompilationContext, nameof(relationalQueryCompilationContext)),
                Check.NotNull(queryIndexes, nameof(queryIndexes)),
                querySourceRequiresTracking);
    }
}
