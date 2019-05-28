// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public partial class RelationalShapedQueryCompilingExpressionVisitor : ShapedQueryCompilingExpressionVisitor
    {
        private readonly IQuerySqlGeneratorFactory2 _querySqlGeneratorFactory;
        private readonly Type _contextType;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;
        private static ParameterExpression _resultCoordinatorParameter
            = Expression.Parameter(typeof(ResultCoordinator), "resultCoordinator");

        public RelationalShapedQueryCompilingExpressionVisitor(
            IEntityMaterializerSource entityMaterializerSource,
            IQuerySqlGeneratorFactory2 querySqlGeneratorFactory,
            Type contextType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger,
            bool trackQueryResults,
            bool async)
            : base(entityMaterializerSource, trackQueryResults, async)
        {
            _querySqlGeneratorFactory = querySqlGeneratorFactory;
            _contextType = contextType;
            _logger = logger;
        }

        protected override Expression VisitShapedQueryExpression(ShapedQueryExpression shapedQueryExpression)
        {
            var shaperBody = InjectEntityMaterializer(shapedQueryExpression.ShaperExpression);

            var selectExpression = (SelectExpression)shapedQueryExpression.QueryExpression;

            shaperBody = new RelationalProjectionBindingRemovingExpressionVisitor(selectExpression).Visit(shaperBody);

            shaperBody = new IncludeCompilingExpressionVisitor(TrackQueryResults).Visit(shaperBody);

            var shaperLambda = Expression.Lambda(
                shaperBody,
                QueryCompilationContext2.QueryContextParameter,
                RelationalProjectionBindingRemovingExpressionVisitor.DataReaderParameter,
                _resultCoordinatorParameter);

            if (Async)
            {
                return Expression.New(
                    typeof(AsyncQueryingEnumerable<>).MakeGenericType(shaperLambda.ReturnType.GetGenericArguments().Single()).GetConstructors()[0],
                    Expression.Convert(QueryCompilationContext2.QueryContextParameter, typeof(RelationalQueryContext)),
                    Expression.Constant(_querySqlGeneratorFactory),
                    Expression.Constant(selectExpression),
                    Expression.Constant(shaperLambda.Compile()),
                    Expression.Constant(_contextType),
                    Expression.Constant(_logger));
            }

            return Expression.New(
                typeof(QueryingEnumerable<>).MakeGenericType(shaperLambda.ReturnType).GetConstructors()[0],
                Expression.Convert(QueryCompilationContext2.QueryContextParameter, typeof(RelationalQueryContext)),
                Expression.Constant(_querySqlGeneratorFactory),
                Expression.Constant(selectExpression),
                Expression.Constant(shaperLambda.Compile()),
                Expression.Constant(_contextType),
                Expression.Constant(_logger));
        }

        private class ResultCoordinator
        {
            public bool? HasNext { get; set; }
            public object[] KeyValues { get; set; }
        }
    }
}
