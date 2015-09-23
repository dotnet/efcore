// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class IncludeExpressionVisitorFactory : IIncludeExpressionVisitorFactory
    {
        private readonly ISelectExpressionFactory _selectExpressionFactory;
        private readonly IMaterializerFactory _materializerFactory;
        private readonly ICommandBuilderFactory _commandBuilderFactory;
        private readonly IRelationalAnnotationProvider _relationalAnnotationProvider;
        private readonly ISqlQueryGeneratorFactory _sqlQueryGeneratorFactory;

        public IncludeExpressionVisitorFactory(
            [NotNull] ISelectExpressionFactory selectExpressionFactory,
            [NotNull] IMaterializerFactory materializerFactory,
            [NotNull] ICommandBuilderFactory commandBuilderFactory,
            [NotNull] IRelationalAnnotationProvider relationalAnnotationProvider,
            [NotNull] ISqlQueryGeneratorFactory sqlQueryGeneratorFactory)
        {
            Check.NotNull(selectExpressionFactory, nameof(selectExpressionFactory));
            Check.NotNull(materializerFactory, nameof(materializerFactory));
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));
            Check.NotNull(relationalAnnotationProvider, nameof(relationalAnnotationProvider));
            Check.NotNull(sqlQueryGeneratorFactory, nameof(sqlQueryGeneratorFactory));

            _selectExpressionFactory = selectExpressionFactory;
            _materializerFactory = materializerFactory;
            _commandBuilderFactory = commandBuilderFactory;
            _relationalAnnotationProvider = relationalAnnotationProvider;
            _sqlQueryGeneratorFactory = sqlQueryGeneratorFactory;
        }

        public virtual ExpressionVisitor Create(
            [NotNull] IQuerySource querySource,
            [NotNull] IReadOnlyList<INavigation> navigationPath,
            [NotNull] RelationalQueryCompilationContext queryCompilationContext,
            [NotNull] IReadOnlyList<int> queryIndexes,
            bool querySourceRequiresTracking)
            => new IncludeExpressionVisitor(
                _selectExpressionFactory,
                _materializerFactory,
                _commandBuilderFactory,
                _relationalAnnotationProvider,
                _sqlQueryGeneratorFactory,
                Check.NotNull(querySource, nameof(querySource)),
                Check.NotNull(navigationPath, nameof(navigationPath)),
                Check.NotNull(queryCompilationContext, nameof(queryCompilationContext)),
                Check.NotNull(queryIndexes, nameof(queryIndexes)),
                querySourceRequiresTracking);
    }
}
