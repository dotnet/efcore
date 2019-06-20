// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public partial class RelationalShapedQueryCompilingExpressionVisitor : ShapedQueryCompilingExpressionVisitor
    {
        private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;
        private readonly Type _contextType;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

        public RelationalShapedQueryCompilingExpressionVisitor(
            IEntityMaterializerSource entityMaterializerSource,
            IQuerySqlGeneratorFactory querySqlGeneratorFactory,
            ISqlExpressionFactory sqlExpressionFactory,
            IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            Type contextType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger,
            bool trackQueryResults,
            bool async)
            : base(entityMaterializerSource, trackQueryResults, async)
        {
            _querySqlGeneratorFactory = querySqlGeneratorFactory;
            _sqlExpressionFactory = sqlExpressionFactory;
            _parameterNameGeneratorFactory = parameterNameGeneratorFactory;
            _contextType = contextType;
            _logger = logger;
        }

        protected override Expression VisitShapedQueryExpression(ShapedQueryExpression shapedQueryExpression)
        {
            var shaperBody = InjectEntityMaterializer(shapedQueryExpression.ShaperExpression);

            var selectExpression = (SelectExpression)shapedQueryExpression.QueryExpression;

            var dataReaderParameter = Expression.Parameter(typeof(DbDataReader), "dataReader");
            var indexMapParameter = Expression.Parameter(typeof(int[]), "indexMap");
            var resultCoordinatorParameter = Expression.Parameter(typeof(ResultCoordinator), "resultCoordinator");

            shaperBody = new RelationalProjectionBindingRemovingExpressionVisitor(selectExpression, dataReaderParameter)
                .Visit(shaperBody);
            shaperBody = new IncludeCompilingExpressionVisitor(dataReaderParameter, resultCoordinatorParameter, TrackQueryResults)
                .Visit(shaperBody);

            if (selectExpression.IsNonComposedFromSql())
            {
                shaperBody = new IndexMapInjectingExpressionVisitor(indexMapParameter).Visit(shaperBody);
            }

            var shaperLambda = Expression.Lambda(
                shaperBody,
                QueryCompilationContext.QueryContextParameter,
                dataReaderParameter,
                indexMapParameter,
                resultCoordinatorParameter);

            return Expression.New(
                (Async
                    ? typeof(AsyncQueryingEnumerable<>)
                    : typeof(QueryingEnumerable<>)).MakeGenericType(shaperLambda.ReturnType).GetConstructors()[0],
                Expression.Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
                Expression.Constant(_querySqlGeneratorFactory),
                Expression.Constant(_sqlExpressionFactory),
                Expression.Constant(_parameterNameGeneratorFactory),
                Expression.Constant(selectExpression),
                Expression.Constant(shaperLambda.Compile()),
                Expression.Constant(_contextType),
                Expression.Constant(_logger));
        }

        private class IndexMapInjectingExpressionVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _indexMapParameter;

            public IndexMapInjectingExpressionVisitor(ParameterExpression indexMapParameter)
            {
                _indexMapParameter = indexMapParameter;
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Object != null
                    && typeof(DbDataReader).IsAssignableFrom(methodCallExpression.Object.Type))
                {
                    var indexArgument = methodCallExpression.Arguments[0];
                    return methodCallExpression.Update(
                        methodCallExpression.Object,
                        new[]
                        {
                            Expression.ArrayIndex(_indexMapParameter, indexArgument),
                        });
                }

                return base.VisitMethodCall(methodCallExpression);
            }
        }

        private class ResultCoordinator
        {
            public bool? HasNext { get; set; }
            public object[] KeyValues { get; set; }
        }
    }
}
