// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class RequiresMaterializationExpressionVisitor : ExpressionVisitorBase
    {
        private readonly EntityQueryModelVisitor _queryModelVisitor;
        private readonly Dictionary<IQuerySource, int> _querySources = new Dictionary<IQuerySource, int>();

        private QueryModel _queryModel;
        private Expression _parentSelector;

        public RequiresMaterializationExpressionVisitor([NotNull] EntityQueryModelVisitor queryModelVisitor)
        {
            Check.NotNull(queryModelVisitor, nameof(queryModelVisitor));

            _queryModelVisitor = queryModelVisitor;
        }

        public virtual ISet<IQuerySource> FindQuerySourcesRequiringMaterialization([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            _queryModel = queryModel;
            _parentSelector = queryModel.SelectClause.Selector;

            _queryModel.TransformExpressions(Visit);

            var querySources
                = new HashSet<IQuerySource>(
                    _querySources.Where(kv => kv.Value > 0).Select(kv => kv.Key));

            return querySources;
        }

        protected override Expression VisitQuerySourceReference(
            QuerySourceReferenceExpression querySourceReferenceExpression)
        {
            Check.NotNull(querySourceReferenceExpression, nameof(querySourceReferenceExpression));

            if (!_querySources.ContainsKey(querySourceReferenceExpression.ReferencedQuerySource))
            {
                _querySources.Add(querySourceReferenceExpression.ReferencedQuerySource, 0);
            }

            if (_queryModelVisitor.QueryCompilationContext.Model
                .FindEntityType(querySourceReferenceExpression.Type) != null)
            {
                _querySources[querySourceReferenceExpression.ReferencedQuerySource]++;
            }

            return base.VisitQuerySourceReference(querySourceReferenceExpression);
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));

            var newExpression = base.VisitMember(memberExpression);

            if (memberExpression.Expression != null)
            {
                _queryModelVisitor
                    .BindMemberExpression(
                        memberExpression,
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

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            var newExpression = base.VisitMethodCall(methodCallExpression);

            _queryModelVisitor
                .BindMethodCallExpression(
                    methodCallExpression,
                    (property, querySource) =>
                        {
                            if (querySource != null)
                            {
                                _querySources[querySource]--;
                            }
                        });

            return newExpression;
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            var oldParentSelector = _parentSelector;

            var leftSubQueryExpression = binaryExpression.Left as SubQueryExpression;

            if (leftSubQueryExpression != null
                && _queryModelVisitor.QueryCompilationContext.Model
                    .FindEntityType(leftSubQueryExpression.Type) != null)
            {
                _parentSelector = leftSubQueryExpression.QueryModel.SelectClause.Selector;

                leftSubQueryExpression.QueryModel.TransformExpressions(Visit);
            }
            else
            {
                Visit(binaryExpression.Left);
            }

            var rightSubQueryExpression = binaryExpression.Right as SubQueryExpression;

            if (rightSubQueryExpression != null
                && _queryModelVisitor.QueryCompilationContext.Model
                    .FindEntityType(rightSubQueryExpression.Type) != null)
            {
                _parentSelector = rightSubQueryExpression.QueryModel.SelectClause.Selector;

                rightSubQueryExpression.QueryModel.TransformExpressions(Visit);
            }
            else
            {
                Visit(binaryExpression.Right);
            }

            _parentSelector = oldParentSelector;

            return binaryExpression;
        }

        protected override Expression VisitSubQuery(SubQueryExpression subQueryExpression)
        {
            Check.NotNull(subQueryExpression, nameof(subQueryExpression));

            var oldParentSelector = _parentSelector;

            _parentSelector = subQueryExpression.QueryModel.SelectClause.Selector;

            subQueryExpression.QueryModel.TransformExpressions(Visit);

            _parentSelector = oldParentSelector;

            var querySourceReferenceExpression
                = subQueryExpression.QueryModel.SelectClause.Selector
                    as QuerySourceReferenceExpression;

            if (querySourceReferenceExpression != null)
            {
                var querySourceTracingExpressionVisitor = new QuerySourceTracingExpressionVisitor();

                var resultQuerySource
                    = querySourceTracingExpressionVisitor
                        .FindResultQuerySourceReferenceExpression(
                            _parentSelector,
                            querySourceReferenceExpression.ReferencedQuerySource);

                if (resultQuerySource == null)
                {
                    _querySources[querySourceReferenceExpression.ReferencedQuerySource]--;
                }
            }

            return subQueryExpression;
        }
    }
}
