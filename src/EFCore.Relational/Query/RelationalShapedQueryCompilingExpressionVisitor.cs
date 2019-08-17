// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class RelationalShapedQueryCompilingExpressionVisitor : ShapedQueryCompilingExpressionVisitor
    {
        private readonly Type _contextType;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;
        private readonly ISet<string> _tags;

        public RelationalShapedQueryCompilingExpressionVisitor(
            ShapedQueryCompilingExpressionVisitorDependencies dependencies,
            RelationalShapedQueryCompilingExpressionVisitorDependencies relationalDependencies,
            QueryCompilationContext queryCompilationContext)
            : base(dependencies, queryCompilationContext)
        {
            RelationalDependencies = relationalDependencies;

            _contextType = queryCompilationContext.ContextType;
            _logger = queryCompilationContext.Logger;
            _tags = queryCompilationContext.Tags;
        }

        protected virtual RelationalShapedQueryCompilingExpressionVisitorDependencies RelationalDependencies { get; }

        protected override Expression VisitShapedQueryExpression(ShapedQueryExpression shapedQueryExpression)
        {
            var selectExpression = (SelectExpression)shapedQueryExpression.QueryExpression;
            selectExpression.ApplyTags(_tags);

            var dataReaderParameter = Expression.Parameter(typeof(DbDataReader), "dataReader");
            var resultCoordinatorParameter = Expression.Parameter(typeof(ResultCoordinator), "resultCoordinator");
            var indexMapParameter = Expression.Parameter(typeof(int[]), "indexMap");

            var shaper = new ShaperExpressionProcessingExpressionVisitor(
                selectExpression,
                dataReaderParameter,
                resultCoordinatorParameter,
                indexMapParameter)
                .Inject(shapedQueryExpression.ShaperExpression);

            shaper = InjectEntityMaterializers(shaper);

            shaper = new RelationalProjectionBindingRemovingExpressionVisitor(selectExpression, dataReaderParameter)
                .Visit(shaper);
            shaper = new CustomShaperCompilingExpressionVisitor(
                dataReaderParameter, resultCoordinatorParameter, IsTracking)
                .Visit(shaper);

            if (selectExpression.IsNonComposedFromSql())
            {
                shaper = new IndexMapInjectingExpressionVisitor(indexMapParameter).Visit(shaper);
            }

            var shaperLambda = (LambdaExpression)shaper;

            return Expression.New(
                (IsAsync
                    ? typeof(AsyncQueryingEnumerable<>)
                    : typeof(QueryingEnumerable<>)).MakeGenericType(shaperLambda.ReturnType).GetConstructors()[0],
                Expression.Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
                Expression.Constant(RelationalDependencies.QuerySqlGeneratorFactory),
                Expression.Constant(RelationalDependencies.SqlExpressionFactory),
                Expression.Constant(RelationalDependencies.ParameterNameGeneratorFactory),
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
            public ResultCoordinator()
            {
                ResultContext = new ResultContext();
            }

            public ResultContext ResultContext { get; }
            public bool ResultReady { get; set; }
            public bool? HasNext { get; set; }
            public IList<CollectionMaterializationContext> Collections { get; } = new List<CollectionMaterializationContext>();

            public void SetCollectionMaterializationContext(
                int collectionId, CollectionMaterializationContext collectionMaterializationContext)
            {
                while (Collections.Count <= collectionId)
                {
                    Collections.Add(null);
                }

                Collections[collectionId] = collectionMaterializationContext;
            }
        }

        private class ResultContext
        {
            public object[] Values { get; set; }
        }

        private class CollectionMaterializationContext
        {
            public CollectionMaterializationContext(object parent, object collection, object[] parentIdentifier, object[] outerIdentifier)
            {
                Parent = parent;
                Collection = collection;
                ParentIdentifier = parentIdentifier;
                OuterIdentifier = outerIdentifier;
                ResultContext = new ResultContext();
            }

            public ResultContext ResultContext { get; }
            public object Parent { get; }
            public object Collection { get; }
            public object Current { get; private set; }
            public object[] ParentIdentifier { get; }
            public object[] OuterIdentifier { get; }
            public object[] SelfIdentifier { get; private set; }

            public void UpdateSelfIdentifier(object[] selfIdentifier)
            {
                SelfIdentifier = selfIdentifier;
            }
        }
    }
}
