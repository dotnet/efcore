// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class QueryModelExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string Print([NotNull] this QueryModel queryModel)
            => new QueryModelPrinter().Print(queryModel);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Dictionary<QueryModel, QueryModel> PopulateQueryModelMapping(
            [NotNull] this QueryModel queryModel,
            [NotNull] Dictionary<QueryModel, QueryModel> mapping)
        {
            var mappingPopulatingVisitor = new QueryModelMappingPopulatingVisitor(mapping);
            mappingPopulatingVisitor.Visit(new SubQueryExpression(queryModel));

            return mapping;
        }

        private class QueryModelMappingPopulatingVisitor : ExpressionVisitorBase
        {
            private readonly Dictionary<QueryModel, QueryModel> _mapping;

            public QueryModelMappingPopulatingVisitor(Dictionary<QueryModel, QueryModel> mapping)
            {
                _mapping = mapping;
            }

            protected override Expression VisitSubQuery(SubQueryExpression expression)
            {
                var queryModel = expression.QueryModel;

                var newQueryModel = new QueryModel(queryModel.MainFromClause, queryModel.SelectClause);
                ShallowCopy(queryModel, newQueryModel);

                _mapping.Add(queryModel, newQueryModel);

                queryModel.TransformExpressions(Visit);

                return expression;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static QueryModel RecreateQueryModelFromMapping(
            [NotNull] this QueryModel queryModel,
            [NotNull] Dictionary<QueryModel, QueryModel> mapping)
        {
            var recreatingVisitor = new QueryModelRecreatingVisitor(mapping);
            var resultExpression = recreatingVisitor.Visit(new SubQueryExpression(queryModel));

            return ((SubQueryExpression)resultExpression).QueryModel;
        }

        private class QueryModelRecreatingVisitor : ExpressionVisitorBase
        {
            private readonly Dictionary<QueryModel, QueryModel> _mapping;

            public QueryModelRecreatingVisitor(Dictionary<QueryModel, QueryModel> mapping)
            {
                _mapping = mapping;
            }

            protected override Expression VisitSubQuery(SubQueryExpression expression)
            {
                var queryModel = expression.QueryModel;

                var originalQueryModel = _mapping[queryModel];
                ShallowCopy(originalQueryModel, queryModel);

                queryModel.TransformExpressions(Visit);

                return expression;
            }
        }

        private static void ShallowCopy(QueryModel sourceQueryModel, QueryModel targetQueryModel)
        {
            targetQueryModel.BodyClauses.Clear();
            foreach (var bodyClause in sourceQueryModel.BodyClauses)
            {
                targetQueryModel.BodyClauses.Add(bodyClause);
            }

            targetQueryModel.ResultOperators.Clear();
            foreach (var resultOperator in sourceQueryModel.ResultOperators)
            {
                targetQueryModel.ResultOperators.Add(resultOperator);
            }

            targetQueryModel.ResultTypeOverride = sourceQueryModel.ResultTypeOverride;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static int CountQuerySourceReferences(
            [NotNull] this QueryModel queryModel, [NotNull] IQuerySource querySource)
        {
            var visitor = new ReferenceFindingExpressionVisitor(querySource);

            queryModel.TransformExpressions(visitor.Visit);

            return visitor.Count;
        }

        private class ReferenceFindingExpressionVisitor : ExpressionVisitorBase
        {
            private readonly IQuerySource _querySource;

            public ReferenceFindingExpressionVisitor(IQuerySource querySource)
            {
                _querySource = querySource;
            }

            public int Count { get; private set; }

            protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression expression)
            {
                if (expression.ReferencedQuerySource == _querySource)
                {
                    Count++;
                }

                return expression;
            }

            protected override Expression VisitSubQuery(SubQueryExpression expression)
            {
                expression.QueryModel.TransformExpressions(Visit);

                return expression;
            }
        }
    }
}
