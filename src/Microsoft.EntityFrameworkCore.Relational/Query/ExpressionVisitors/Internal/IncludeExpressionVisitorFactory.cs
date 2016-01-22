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
    public class IncludeExpressionVisitorFactory : IIncludeExpressionVisitorFactory
    {
        private readonly ISelectExpressionFactory _selectExpressionFactory;
        private readonly IMaterializerFactory _materializerFactory;
        private readonly IShaperCommandContextFactory _shaperCommandContextFactory;
        private readonly IRelationalAnnotationProvider _relationalAnnotationProvider;
        private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;

        public IncludeExpressionVisitorFactory(
            [NotNull] ISelectExpressionFactory selectExpressionFactory,
            [NotNull] IMaterializerFactory materializerFactory,
            [NotNull] IShaperCommandContextFactory shaperCommandContextFactory,
            [NotNull] IRelationalAnnotationProvider relationalAnnotationProvider,
            [NotNull] IQuerySqlGeneratorFactory querySqlGeneratorFactory)
        {
            Check.NotNull(selectExpressionFactory, nameof(selectExpressionFactory));
            Check.NotNull(materializerFactory, nameof(materializerFactory));
            Check.NotNull(shaperCommandContextFactory, nameof(shaperCommandContextFactory));
            Check.NotNull(relationalAnnotationProvider, nameof(relationalAnnotationProvider));
            Check.NotNull(querySqlGeneratorFactory, nameof(querySqlGeneratorFactory));

            _selectExpressionFactory = selectExpressionFactory;
            _materializerFactory = materializerFactory;
            _shaperCommandContextFactory = shaperCommandContextFactory;
            _relationalAnnotationProvider = relationalAnnotationProvider;
            _querySqlGeneratorFactory = querySqlGeneratorFactory;
        }

        public virtual ExpressionVisitor Create(
            IQuerySource querySource,
            IReadOnlyList<INavigation> navigationPath,
            RelationalQueryCompilationContext relationalQueryCompilationContext,
            IReadOnlyList<int> queryIndexes,
            bool querySourceRequiresTracking)
            => new IncludeExpressionVisitor(
                _selectExpressionFactory,
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
