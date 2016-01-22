// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    public class RequiresMaterializationExpressionVisitor : ExpressionVisitorBase
    {
        private readonly IModel _model;
        private readonly EntityQueryModelVisitor _queryModelVisitor;
        private readonly Dictionary<IQuerySource, int> _querySources = new Dictionary<IQuerySource, int>();

        private QueryModel _queryModel;
        private Expression _parentSelector;

        public RequiresMaterializationExpressionVisitor(
            [NotNull] IModel model,
            [NotNull] EntityQueryModelVisitor queryModelVisitor)
        {
            _model = model;
            _queryModelVisitor = queryModelVisitor;
        }

        public virtual ISet<IQuerySource> FindQuerySourcesRequiringMaterialization([NotNull] QueryModel queryModel)
        {
            _queryModel = queryModel;
            _parentSelector = queryModel.SelectClause.Selector;

            _queryModel.TransformExpressions(Visit);

            var querySources
                = new HashSet<IQuerySource>(
                    _querySources.Where(kv => kv.Value > 0).Select(kv => kv.Key));

            return querySources;
        }

        protected override Expression VisitQuerySourceReference(
            QuerySourceReferenceExpression expression)
        {
            if (!_querySources.ContainsKey(expression.ReferencedQuerySource))
            {
                _querySources.Add(expression.ReferencedQuerySource, 0);
            }

            if (_model.FindEntityType(expression.Type) != null)
            {
                _querySources[expression.ReferencedQuerySource]++;
            }

            return base.VisitQuerySourceReference(expression);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var newExpression = base.VisitMember(node);

            if (node.Expression != null)
            {
                _queryModelVisitor
                    .BindMemberExpression(
                        node,
                        (property, querySource) =>
                            {
                                if (querySource != null)
                                {
                                    _querySources[querySource]--;
                                }
                            });
            }

            return newExpression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var newExpression = base.VisitMethodCall(node);

            _queryModelVisitor
                .BindMethodCallExpression(
                    node,
                    (property, querySource) =>
                        {
                            if (querySource != null)
                            {
                                _querySources[querySource]--;
                            }
                        });

            return newExpression;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var oldParentSelector = _parentSelector;

            var leftSubQueryExpression = node.Left as SubQueryExpression;

            if ((leftSubQueryExpression != null)
                && (_model.FindEntityType(leftSubQueryExpression.Type) != null))
            {
                _parentSelector = leftSubQueryExpression.QueryModel.SelectClause.Selector;

                leftSubQueryExpression.QueryModel.TransformExpressions(Visit);
            }
            else
            {
                Visit(node.Left);
            }

            var rightSubQueryExpression = node.Right as SubQueryExpression;

            if ((rightSubQueryExpression != null)
                && (_model.FindEntityType(rightSubQueryExpression.Type) != null))
            {
                _parentSelector = rightSubQueryExpression.QueryModel.SelectClause.Selector;

                rightSubQueryExpression.QueryModel.TransformExpressions(Visit);
            }
            else
            {
                Visit(node.Right);
            }

            _parentSelector = oldParentSelector;

            return node;
        }

        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            var oldParentSelector = _parentSelector;

            _parentSelector = expression.QueryModel.SelectClause.Selector;

            expression.QueryModel.TransformExpressions(Visit);

            _parentSelector = oldParentSelector;

            var querySourceReferenceExpression
                = expression.QueryModel.SelectClause.Selector
                    as QuerySourceReferenceExpression;

            if (querySourceReferenceExpression != null)
            {
                var querySourceTracingExpressionVisitor = new QuerySourceTracingExpressionVisitor();

                var resultQuerySource
                    = querySourceTracingExpressionVisitor
                        .FindResultQuerySourceReferenceExpression(
                            _parentSelector,
                            querySourceReferenceExpression.ReferencedQuerySource);

                if ((resultQuerySource == null)
                    && !(expression.QueryModel.ResultOperators.LastOrDefault() is OfTypeResultOperator))
                {
                    _querySources[querySourceReferenceExpression.ReferencedQuerySource]--;
                }
            }

            return expression;
        }
    }
}
