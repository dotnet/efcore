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
using Remotion.Linq.Clauses.StreamedData;
using System;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class RequiresMaterializationExpressionVisitor : ExpressionVisitorBase
    {
        private readonly IModel _model;
        private readonly EntityQueryModelVisitor _queryModelVisitor;
        private readonly Stack<QueryModel> _queryModelStack = new Stack<QueryModel>();
        private readonly Dictionary<IQuerySource, int> _querySourceReferences = new Dictionary<IQuerySource, int>();

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
            AddResultOperators(queryModel);

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
            var nullConditional = node as NullConditionalExpression;

            if (nullConditional != null)
            {
                Visit(nullConditional.AccessOperation);

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
                if (node.Expression.Type.IsGrouping() && node.Member.Name == "Key")
                {
                    var groupResultOperator
                        = (((node.Expression as QuerySourceReferenceExpression)
                            ?.ReferencedQuerySource as MainFromClause)
                                ?.FromExpression as SubQueryExpression)
                                    ?.QueryModel.ResultOperators.LastOrDefault() as GroupResultOperator;

                    if (groupResultOperator == null)
                    {
                        groupResultOperator
                            = (((node.Expression as QuerySourceReferenceExpression)
                                ?.ReferencedQuerySource as GroupJoinClause)
                                    ?.JoinClause.InnerSequence as SubQueryExpression)
                                        ?.QueryModel.ResultOperators.LastOrDefault() as GroupResultOperator;
                    }

                    if (groupResultOperator != null)
                    {
                        DemoteQuerySource(groupResultOperator);
                    }
                }
                else
                {
                    _queryModelVisitor.BindMemberExpression(node, (property, querySource) =>
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
            var newExpression = base.VisitMethodCall(node);

            _queryModelVisitor.BindMethodCallExpression(node, (property, querySource) =>
            {
                if (querySource != null)
                {
                    DemoteQuerySource(querySource);
                }
            });

            return newExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            VisitBinaryOperand(node.Left, node.NodeType);
            VisitBinaryOperand(node.Right, node.NodeType);

            return node;
        }

        private Expression VisitBinaryOperand(Expression operand, ExpressionType comparison)
        {
            switch (comparison)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:

                    var isEntityTypeExpression = _model.FindEntityType(operand.Type) != null;

                    // An equality comparison of query source reference expressions
                    // that reference an entity query source does not suggest that
                    // materialization of that entity type may be required. This is true
                    // whether in a join predicate, where predicate, selector, etc.
                    if (operand is QuerySourceReferenceExpression && isEntityTypeExpression)
                    {
                        break;
                    }

                    var subQueryExpression = operand as SubQueryExpression;

                    if (subQueryExpression != null && isEntityTypeExpression)
                    {
                        _queryModelStack.Push(subQueryExpression.QueryModel);

                        subQueryExpression.QueryModel.TransformExpressions(Visit);

                        _queryModelStack.Pop();

                        break;
                    }

                    Visit(operand);

                    break;

                default:

                    Visit(operand);

                    break;
            }

            return operand;
        }

        private void AddResultOperators(QueryModel queryModel)
        {
            foreach (var resultOperator in queryModel.ResultOperators.OfType<IQuerySource>())
            {
                PromoteQuerySource(resultOperator);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            _queryModelStack.Push(expression.QueryModel);

            expression.QueryModel.TransformExpressions(Visit);

            _queryModelStack.Pop();

            AdjustForResultOperators(expression.QueryModel);

            var parentQueryModel = _queryModelStack.Peek();

            var referencedQuerySource
                = (expression.QueryModel.SelectClause.Selector
                    as QuerySourceReferenceExpression)?
                        .ReferencedQuerySource;

            if (referencedQuerySource != null)
            {                
                var parentQuerySource =
                    (parentQueryModel.SelectClause.Selector as QuerySourceReferenceExpression)
                        ?.ReferencedQuerySource;

                if (referencedQuerySource.ItemType == parentQuerySource?.ItemType)
                {
                    var resultSetOperators = GetSetResultOperatorSourceExpressions(parentQueryModel);

                    if (resultSetOperators.Any())
                    {
                        var parentStatistics = _querySourceReferences[parentQuerySource];

                        if (_querySourceReferences[parentQuerySource] > 0)
                        {
                            foreach (var sourceExpression in resultSetOperators)
                            {
                                if (sourceExpression.Equals(expression))
                                {
                                    PromoteQuerySource(referencedQuerySource);
                                }
                            }
                        }
                    }
                }
            }

            return expression;
        }

        private void DemoteQuerySource(IQuerySource querySource)
        {
            HandleUnderlyingQuerySources(querySource, DemoteQuerySource);

            if (_querySourceReferences.ContainsKey(querySource))
            {
                _querySourceReferences[querySource] -= 1;
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
                _querySourceReferences[querySource] += 1;
            }
        }

        private void HandleUnderlyingQuerySources(IQuerySource querySource, Action<IQuerySource> action)
        {
            if (querySource is GroupResultOperator)
            {
                var keySelectorQuerySource
                    = ((querySource as GroupResultOperator)
                        ?.KeySelector as QuerySourceReferenceExpression)
                            ?.ReferencedQuerySource;

                if (keySelectorQuerySource != null)
                {
                    action(keySelectorQuerySource);
                }

                var keySelectorSubQueryQuerySource
                    = (((querySource as GroupResultOperator)
                        ?.KeySelector as SubQueryExpression)
                            ?.QueryModel.SelectClause.Selector as QuerySourceReferenceExpression)
                                ?.ReferencedQuerySource;

                if (keySelectorSubQueryQuerySource != null)
                {
                    action(keySelectorSubQueryQuerySource);
                }

                var elementSelectorQuerySource
                    = ((querySource as GroupResultOperator)
                        ?.ElementSelector as QuerySourceReferenceExpression)
                            ?.ReferencedQuerySource;

                if (elementSelectorQuerySource != null)
                {
                    action(elementSelectorQuerySource);
                }

                var elementSelectorSubQueryQuerySource
                    = (((querySource as GroupResultOperator)
                        ?.ElementSelector as SubQueryExpression)
                            ?.QueryModel.SelectClause.Selector as QuerySourceReferenceExpression)
                                ?.ReferencedQuerySource;

                if (elementSelectorSubQueryQuerySource != null)
                {
                    action(elementSelectorSubQueryQuerySource);
                }
            }
            else if (querySource is GroupJoinClause)
            {
                action((querySource as GroupJoinClause).JoinClause);
            }
            else
            {
                var underlyingExpression
                    = ((querySource as FromClauseBase)?.FromExpression)
                        ?? ((querySource as JoinClause)?.InnerSequence);

                if (underlyingExpression is SubQueryExpression)
                {
                    var lastResultOperator
                        = ((SubQueryExpression)underlyingExpression)
                            .QueryModel.ResultOperators.LastOrDefault();

                    if (lastResultOperator is IQuerySource)
                    {
                        action(lastResultOperator as IQuerySource);
                    }
                    else
                    {
                        var selectorQuerySource
                            = (((SubQueryExpression)underlyingExpression)
                                .QueryModel.SelectClause.Selector as QuerySourceReferenceExpression)
                                    ?.ReferencedQuerySource;

                        if (selectorQuerySource != null)
                        {
                            action(selectorQuerySource);
                        }
                    }
                }
                else if (underlyingExpression is QuerySourceReferenceExpression)
                {
                    action(((QuerySourceReferenceExpression)underlyingExpression).ReferencedQuerySource);
                }
            }
        }

        private void AdjustForResultOperators(QueryModel queryModel)
        {
            var referencedQuerySource
               = (queryModel.SelectClause.Selector
                   as QuerySourceReferenceExpression)?
                       .ReferencedQuerySource;

            if (referencedQuerySource == null)
            {
                referencedQuerySource
                    = (queryModel.MainFromClause.FromExpression
                        as QuerySourceReferenceExpression)?
                            .ReferencedQuerySource;
            }

            if (referencedQuerySource != null)
            {
                var outputInfo = queryModel.GetOutputDataInfo();
                var itemType = outputInfo.DataType.TryGetSequenceType() ?? outputInfo.DataType;
                var isEntityQuery = _model.FindEntityType(itemType) != null;
                var isSubQuery = _queryModelStack.Count > 0;
                var convergesToSingleValue = false;

                if (outputInfo is StreamedSingleValueInfo || outputInfo is StreamedScalarValueInfo)
                {
                    convergesToSingleValue = true;
                }
                else
                {
                    foreach (var ancestorQueryModel in _queryModelStack)
                    {
                        outputInfo = ancestorQueryModel.GetOutputDataInfo();

                        if (outputInfo is StreamedSingleValueInfo || outputInfo is StreamedScalarValueInfo)
                        {
                            convergesToSingleValue = true;
                            break;
                        }
                    }
                }

                var finalResultOperator = queryModel.ResultOperators.LastOrDefault();

                if (isSubQuery && finalResultOperator is GroupResultOperator)
                {
                    DemoteQuerySource(referencedQuerySource);
                    DemoteQuerySource(finalResultOperator as IQuerySource);
                }
                else if (finalResultOperator is SingleResultOperator)
                {
                    if (isSubQuery)
                    {
                        var tracer = new QuerySourceTracingExpressionVisitor();
                        var traced = tracer.FindResultQuerySourceReferenceExpression(
                            _queryModelStack.Peek().SelectClause.Selector,
                            referencedQuerySource);

                        if (traced == null)
                        {
                            DemoteQuerySource(referencedQuerySource);
                        }
                    }
                }
                else if (finalResultOperator is FirstResultOperator || finalResultOperator is LastResultOperator)
                {
                    var choiceResultOperator = (ChoiceResultOperatorBase)finalResultOperator;

                    if (isSubQuery && choiceResultOperator.ReturnDefaultWhenEmpty)
                    {
                        var tracer = new QuerySourceTracingExpressionVisitor();
                        var traced = tracer.FindResultQuerySourceReferenceExpression(
                            _queryModelStack.Peek().SelectClause.Selector,
                            referencedQuerySource);

                        if (traced == null)
                        {
                            DemoteQuerySource(referencedQuerySource);
                        }
                    }
                }
                else if (convergesToSingleValue)
                {
                    DemoteQuerySource(referencedQuerySource);

                    var underlyingQuerySource
                        = ((referencedQuerySource as FromClauseBase)
                            ?.FromExpression as QuerySourceReferenceExpression)
                                ?.ReferencedQuerySource;

                    if (underlyingQuerySource != null)
                    {
                        DemoteQuerySource(underlyingQuerySource);
                    }
                }
                else if (isSubQuery)
                {
                    var tracer = new QuerySourceTracingExpressionVisitor();
                    var traced = tracer.FindResultQuerySourceReferenceExpression(
                        _queryModelStack.Peek().SelectClause.Selector,
                        referencedQuerySource);

                    if (traced == null)
                    {
                        DemoteQuerySource(referencedQuerySource);

                        var underlyingQuerySource
                            = ((referencedQuerySource as MainFromClause)
                                ?.FromExpression as QuerySourceReferenceExpression)
                                    ?.ReferencedQuerySource;

                        if (underlyingQuerySource != null)
                        {
                            DemoteQuerySource(underlyingQuerySource);
                        }
                    }
                }
            }
        }

        private static IEnumerable<Expression> GetSetResultOperatorSourceExpressions(QueryModel queryModel)
        {
            foreach (var resultOperator in queryModel.ResultOperators)
            {
                var concatOperator = resultOperator as ConcatResultOperator;
                if (concatOperator != null)
                {
                    yield return concatOperator.Source2;
                }

                var exceptOperator = resultOperator as ExceptResultOperator;
                if (exceptOperator != null)
                {
                    yield return exceptOperator.Source2;
                }

                var intersectOperator = resultOperator as IntersectResultOperator;
                if (intersectOperator != null)
                {
                    yield return intersectOperator.Source2;
                }

                var unionOperator = resultOperator as UnionResultOperator;
                if (unionOperator != null)
                {
                    yield return unionOperator.Source2;
                }
            }
        }
    }
}
