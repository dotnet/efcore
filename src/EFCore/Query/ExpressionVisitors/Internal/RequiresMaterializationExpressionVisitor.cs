// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class RequiresMaterializationExpressionVisitor : ExpressionVisitorBase
    {
        private static readonly ISet<Type> _aggregateResultOperators = new HashSet<Type>
        {
            typeof(AverageResultOperator),
            typeof(CountResultOperator),
            typeof(LongCountResultOperator),
            typeof(MaxResultOperator),
            typeof(MinResultOperator),
            typeof(SumResultOperator)
        };

        private readonly IModel _model;
        private readonly EntityQueryModelVisitor _queryModelVisitor;
        private readonly Stack<QueryModel> _queryModelStack = new Stack<QueryModel>();
        private readonly Dictionary<IQuerySource, int> _querySourceReferences = new Dictionary<IQuerySource, int>();

        private readonly QuerySourceTracingExpressionVisitor _querySourceTracingExpressionVisitor
            = new QuerySourceTracingExpressionVisitor();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RequiresMaterializationExpressionVisitor(
            [NotNull] IModel model,
            [NotNull] EntityQueryModelVisitor queryModelVisitor)
        {
            _model = model;
            _queryModelVisitor = queryModelVisitor;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ISet<IQuerySource> FindQuerySourcesRequiringMaterialization([NotNull] QueryModel queryModel)
        {
            // Top-level query source result operators need to be promoted manually here
            // because unlike subquerys' result operators, they won't be promoted via
            // HandleUnderlyingQuerySources
            foreach (var querySourceResultOperator in queryModel.ResultOperators.OfType<IQuerySource>())
            {
                PromoteQuerySource(querySourceResultOperator);
            }

            _queryModelStack.Push(queryModel);

            queryModel.TransformExpressions(Visit);

            _queryModelStack.Pop();

            AdjustForResultOperators(queryModel);

            var querySources =
                from entry in _querySourceReferences
                where entry.Value > 0
                select entry.Key;

            return new HashSet<IQuerySource>(querySources);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitQuerySourceReference(
            QuerySourceReferenceExpression expression)
        {
            PromoteQuerySource(expression.ReferencedQuerySource);

            return base.VisitQuerySourceReference(expression);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitExtension(Expression node)
        {
            if (node is NullConditionalExpression nullConditionalExpression)
            {
                Visit(nullConditionalExpression.AccessOperation);

                return node;
            }

            if (node is NullSafeEqualExpression nullConditionalEqualExpression)
            {
                Visit(nullConditionalEqualExpression.OuterKeyNullCheck);
                Visit(nullConditionalEqualExpression.EqualExpression);

                return node;
            }

            return base.VisitExtension(node);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMember(MemberExpression node)
        {
            var newExpression = base.VisitMember(node);

            if (node.Expression != null)
            {
                if (node.Expression.Type.IsGrouping()
                    && node.Member.Name == "Key")
                {
                    if (node.Expression is QuerySourceReferenceExpression qsre)
                    {
                        DemoteQuerySource(qsre.ReferencedQuerySource);
                    }
                }
                else
                {
                    _queryModelVisitor.BindMemberExpression(
                        node, (property, querySource) =>
                        {
                            if (querySource != null)
                            {
                                DemoteQuerySource(querySource);
                            }
                        });
                }
            }

            return newExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var newExpression = (MethodCallExpression)base.VisitMethodCall(node);

            _queryModelVisitor.BindMethodCallExpression(
                node, (property, querySource) =>
                {
                    if (querySource != null)
                    {
                        DemoteQuerySource(querySource);
                    }
                });

            if (AnonymousObject.IsGetValueExpression(node, out var querySourceReferenceExpression))
            {
                DemoteQuerySource(querySourceReferenceExpression.ReferencedQuerySource);
            }

            foreach (var subQueryExpression in newExpression.Arguments.OfType<SubQueryExpression>())
            {
                if (subQueryExpression.QueryModel.ResultOperators.LastOrDefault() is IQuerySource querySourceResultOperator)
                {
                    PromoteQuerySource(querySourceResultOperator);
                }
                else if (subQueryExpression.QueryModel.SelectClause.Selector is QuerySourceReferenceExpression qsre)
                {
                    PromoteQuerySource(qsre.ReferencedQuerySource);
                }
            }

            return newExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression node)
            => node.Update(
                VisitBinaryOperand(node.Left, node.NodeType),
                node.Conversion,
                VisitBinaryOperand(node.Right, node.NodeType));

        private Expression VisitBinaryOperand(Expression operand, ExpressionType comparison)
        {
            if (comparison == ExpressionType.Equal
                || comparison == ExpressionType.NotEqual)
            {
                if (operand is SubQueryExpression subQueryExpression
                    && _queryModelVisitor.QueryCompilationContext.DuplicateQueryModels.Contains(subQueryExpression.QueryModel))
                {
                    _queryModelStack.Push(subQueryExpression.QueryModel);

                    subQueryExpression.QueryModel.TransformExpressions(Visit);

                    _queryModelStack.Pop();

                    return operand;
                }
            }

            return Visit(operand);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            if (!IsGroupByAggregateSubQuery(expression.QueryModel))
            {
                _queryModelStack.Push(expression.QueryModel);

                expression.QueryModel.TransformExpressions(Visit);

                _queryModelStack.Pop();

                AdjustForResultOperators(expression.QueryModel);

                var parentQueryModel = _queryModelStack.Peek();

                var referencedQuerySource = expression.QueryModel.SelectClause.Selector.TryGetReferencedQuerySource();

                if (referencedQuerySource != null)
                {
                    var parentQuerySource = parentQueryModel.SelectClause.Selector.TryGetReferencedQuerySource();
                    if (parentQuerySource != null)
                    {
                        var resultSetOperators = GetSetResultOperatorSourceExpressions(parentQueryModel.ResultOperators);
                        if (resultSetOperators.Any(r => r.Equals(expression))
                            && _querySourceReferences[parentQuerySource] > 0)
                        {
                            PromoteQuerySource(referencedQuerySource);
                        }
                    }
                }
            }

            return expression;
        }

        private bool IsGroupByAggregateSubQuery(QueryModel queryModel)
        {
            if (queryModel.MainFromClause.FromExpression.Type.IsGrouping()
                && queryModel.BodyClauses.Count == 0
                && queryModel.ResultOperators.Count == 1
                && _aggregateResultOperators.Contains(queryModel.ResultOperators[0].GetType())
                && _queryModelStack.Count > 0
                && !_queryModelStack.Peek().BodyClauses.OfType<IQuerySource>().Any())
            {
                var groupResultOperator
                    = (GroupResultOperator)
                    ((SubQueryExpression)
                        ((FromClauseBase)queryModel.MainFromClause.FromExpression.TryGetReferencedQuerySource())
                        .FromExpression)
                    .QueryModel.ResultOperators.Last();

                MemberAccessBindingExpressionVisitor.GetPropertyPath(
                    queryModel.SelectClause.Selector, _queryModelVisitor.QueryCompilationContext, out var qsre);

                if (qsre != null
                    || queryModel.SelectClause.Selector.RemoveConvert() is ConstantExpression
                    || groupResultOperator.ElementSelector.NodeType == ExpressionType.New
                    || groupResultOperator.ElementSelector.NodeType == ExpressionType.MemberInit)
                {
                    return true;
                }
            }

            return false;
        }

        private void DemoteQuerySource(IQuerySource querySource)
        {
            HandleUnderlyingQuerySources(querySource, DemoteQuerySource);

            if (_querySourceReferences.ContainsKey(querySource))
            {
                _querySourceReferences[querySource]--;
            }
        }

        private void PromoteQuerySource(IQuerySource querySource)
        {
            HandleUnderlyingQuerySources(querySource, PromoteQuerySource);

            if (!_querySourceReferences.ContainsKey(querySource))
            {
                _querySourceReferences[querySource] = 1;
            }
            else
            {
                _querySourceReferences[querySource]++;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void HandleUnderlyingQuerySources([NotNull] IQuerySource querySource, [NotNull] Action<IQuerySource> action)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(action, nameof(action));

            if (querySource is GroupResultOperator groupResultOperator)
            {
                var keySelectorExpression
                    = groupResultOperator.KeySelector is SubQueryExpression keySelectorSubQuery
                        ? keySelectorSubQuery.QueryModel.SelectClause.Selector
                        : groupResultOperator.KeySelector;

                if (keySelectorExpression is QuerySourceReferenceExpression keySelectorQsre)
                {
                    action(keySelectorQsre.ReferencedQuerySource);
                }

                var elementSelectorExpression
                    = groupResultOperator.ElementSelector is SubQueryExpression elementSelectorSubQuery
                        ? elementSelectorSubQuery.QueryModel.SelectClause.Selector
                        : groupResultOperator.ElementSelector;

                if (elementSelectorExpression is QuerySourceReferenceExpression elementSelectorQsre)
                {
                    action(elementSelectorQsre.ReferencedQuerySource);
                }
            }
            else if (querySource is GroupJoinClause groupJoinClause)
            {
                action(groupJoinClause.JoinClause);
            }
            else
            {
                var underlyingExpression
                    = (querySource as FromClauseBase)?.FromExpression
                      ?? (querySource as JoinClause)?.InnerSequence;

                if (underlyingExpression is SubQueryExpression subQueryExpression)
                {
                    var finalResultOperator = subQueryExpression.QueryModel.ResultOperators.LastOrDefault();

                    if (finalResultOperator is IQuerySource querySourceResultOperator)
                    {
                        action(querySourceResultOperator);
                    }
                    else if (subQueryExpression.QueryModel.SelectClause.Selector is QuerySourceReferenceExpression qsre)
                    {
                        action(qsre.ReferencedQuerySource);
                    }
                }
                else if (underlyingExpression is QuerySourceReferenceExpression qsre)
                {
                    action(qsre.ReferencedQuerySource);
                }
            }
        }

        private static IEnumerable<IQuerySource> TraverseQuerySources(Expression expression)
        {
            switch (expression)
            {
                case QuerySourceReferenceExpression qsre:
                    yield return qsre.ReferencedQuerySource;
                    break;

                case NewExpression newExpression:
                    foreach (var arg in newExpression.Arguments)
                    {
                        foreach (var querySource in TraverseQuerySources(arg))
                        {
                            yield return querySource;
                        }
                    }

                    break;

                case MemberInitExpression memberInitExpression:
                    foreach (var memberBinding in memberInitExpression.Bindings)
                    {
                        if (memberBinding is MemberAssignment memberAssignment)
                        {
                            foreach (var querySource in TraverseQuerySources(memberAssignment.Expression))
                            {
                                yield return querySource;
                            }
                        }
                    }

                    break;
            }
        }

        private void AdjustForResultOperators(QueryModel queryModel)
        {
            var referencedQuerySource
                = queryModel.SelectClause.Selector.TryGetReferencedQuerySource()
                  ?? queryModel.MainFromClause.FromExpression.TryGetReferencedQuerySource();

            // If there is any CastResultOperator then we need to do client eval unless it is doing same cast.
            if (queryModel.ResultOperators.OfType<CastResultOperator>().Any()
                && referencedQuerySource != null)
            {
                PromoteQuerySource(referencedQuerySource);

                return;
            }

            var isSubQuery = _queryModelStack.Count > 0;
            var finalResultOperator = queryModel.ResultOperators.LastOrDefault();

            if (isSubQuery
                && finalResultOperator is GroupResultOperator groupResultOperator
                && queryModel.ResultOperators.OfType<GroupResultOperator>().Count() == 1)
            {
                if (!(groupResultOperator.KeySelector is MemberInitExpression))
                {
                    // This is to compensate for the fact that we have to demote querysources found in selector for GroupBy
                    // TODO: See #11215
                    foreach (var querySource in TraverseQuerySources(queryModel.SelectClause.Selector))
                    {
                        DemoteQuerySource(querySource);
                    }

                    if (groupResultOperator.ElementSelector is QuerySourceReferenceExpression qsre)
                    {
                        DemoteQuerySource(qsre.ReferencedQuerySource);
                    }

                    return;
                }
            }

            // The selector may not have been a QSRE but this query model may still have something that needs adjusted.
            // Example:
            // context.Orders.GroupBy(o => o.CustomerId).Select(g => new { g.Key, g.Sum(o => o.TotalAmount) })
            // The g.Sum(...) will result in a subquery model like { from Order o in [g] select o.TotalAmount => Sum() }.
            // In that case we need to ensure that the referenced query source [g] is demoted.
            if (referencedQuerySource == null)
            {
                return;
            }

            // If the GroupResultOperator is not last (as captured by above)
            // then we promote first GroupResultOperator to fall through streaming group by
            var firstGroupResultOperator = queryModel.ResultOperators.OfType<GroupResultOperator>().FirstOrDefault();

            if (firstGroupResultOperator != null)
            {
                PromoteQuerySource(firstGroupResultOperator);
            }

            var unreachableFromParentSelector =
                isSubQuery && _querySourceTracingExpressionVisitor
                    .FindResultQuerySourceReferenceExpression(
                        _queryModelStack.Peek().SelectClause.Selector,
                        referencedQuerySource) == null;

            if (finalResultOperator is SingleResultOperator
                || finalResultOperator is FirstResultOperator
                || finalResultOperator is LastResultOperator)
            {
                // If not a subquery or if reachable from the parent selector
                // we would not want to fall through to one of the next blocks.
                if (unreachableFromParentSelector)
                {
                    DemoteQuerySourceAndUnderlyingFromClause(referencedQuerySource);
                }

                return;
            }

            if (ConvergesToSingleValue(queryModel)
                // This is to preserve QuerySource from MainFromClause when selector is ConstantExpression
                // Since we would cause client evaluation from ConstantExpression.
                // TODO: See #11215
                && !(queryModel.SelectClause.Selector is ConstantExpression))
            {
                // This is a top-level query that was not Single/First/Last
                // but returns a single/scalar value (Avg/Min/Max/etc.)
                // or a subquery that belongs to some outer-level query that returns
                // a single or scalar value. The referenced query source should be
                // re-promoted later if necessary.

                // For top-level Contains we cannot translate it since Item is not Expression
                if (!isSubQuery
                    && finalResultOperator is ContainsResultOperator containsResultOperator)
                {
                    return;
                }

                // If we are selecting from Grouping source but it is not GroupByAggregate Query
                // then do not demote Grouping source since we still need to create groups.
                if (queryModel.MainFromClause.FromExpression.Type.IsGrouping()
                    && !IsGroupByAggregateSubQuery(queryModel))
                {
                    return;
                }

                DemoteQuerySourceAndUnderlyingFromClause(referencedQuerySource);
                return;
            }

            if (isSubQuery && (unreachableFromParentSelector || finalResultOperator is DefaultIfEmptyResultOperator))
            {
                DemoteQuerySourceAndUnderlyingFromClause(referencedQuerySource);
            }
        }

        private static bool ConvergesToSingleValue(QueryModel queryModel)
        {
            var outputInfo = queryModel.GetOutputDataInfo();

            return outputInfo is StreamedSingleValueInfo || outputInfo is StreamedScalarValueInfo;
        }

        private void DemoteQuerySourceAndUnderlyingFromClause(IQuerySource querySource)
        {
            DemoteQuerySource(querySource);

            var underlyingQuerySource
                = (querySource as FromClauseBase)?.FromExpression.TryGetReferencedQuerySource();

            if (underlyingQuerySource != null)
            {
                DemoteQuerySource(underlyingQuerySource);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<Expression> GetSetResultOperatorSourceExpressions([NotNull] IEnumerable<ResultOperatorBase> resultOperators)
        {
            Check.NotNull(resultOperators, nameof(resultOperators));

            foreach (var resultOperator in resultOperators)
            {
                if (resultOperator is ConcatResultOperator concatOperator)
                {
                    yield return concatOperator.Source2;
                }
                else if (resultOperator is ExceptResultOperator exceptOperator)
                {
                    yield return exceptOperator.Source2;
                }
                else if (resultOperator is IntersectResultOperator intersectOperator)
                {
                    yield return intersectOperator.Source2;
                }
                else if (resultOperator is UnionResultOperator unionOperator)
                {
                    yield return unionOperator.Source2;
                }
            }
        }
    }
}
