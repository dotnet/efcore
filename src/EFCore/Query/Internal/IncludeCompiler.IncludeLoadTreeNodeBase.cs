// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Remotion.Linq;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public partial class IncludeCompiler
    {
        private abstract class IncludeLoadTreeNodeBase
        {
            protected static void AddLoadPath(
                IncludeLoadTreeNodeBase node,
                IReadOnlyList<INavigation> navigationPath,
                int index)
            {
                while (index < navigationPath.Count)
                {
                    var navigation = navigationPath[index];
                    var childNode = node.Children.SingleOrDefault(n => n.Navigation == navigation);

                    if (childNode == null)
                    {
                        node.Children.Add(childNode = new IncludeLoadTreeNode(navigation));

                        var targetType = navigation.GetTargetType();

                        var outboundNavigations
                            = targetType.GetNavigations()
                                .Concat(targetType.GetDerivedTypes().SelectMany(et => et.GetDeclaredNavigations()))
                                .Where(n => n.IsEagerLoaded);

                        foreach (var outboundNavigation in outboundNavigations)
                        {
                            AddLoadPath(childNode, new[] { outboundNavigation }, index: 0);
                        }
                    }

                    node = childNode;
                    index = index + 1;
                }
            }

            protected ICollection<IncludeLoadTreeNode> Children { get; } = new List<IncludeLoadTreeNode>();

            protected void Compile(
                QueryCompilationContext queryCompilationContext,
                QueryModel queryModel,
                bool trackingQuery,
                bool asyncQuery,
                ref int collectionIncludeId,
                QuerySourceReferenceExpression targetQuerySourceReferenceExpression)
            {
                var entityParameter
                    = Expression.Parameter(targetQuerySourceReferenceExpression.Type, name: "entity");

                var propertyExpressions = new List<Expression>();
                var blockExpressions = new List<Expression>();

                var entityType
                    = queryCompilationContext.FindEntityType(targetQuerySourceReferenceExpression.ReferencedQuerySource)
                      ?? queryCompilationContext.Model.FindEntityType(entityParameter.Type);

                if (entityType.IsQueryType)
                {
                    trackingQuery = false;
                }

                if (trackingQuery)
                {
                    blockExpressions.Add(
                        Expression.Call(
                            Expression.Property(
                                EntityQueryModelVisitor.QueryContextParameter,
                                nameof(QueryContext.QueryBuffer)),
                            _queryBufferStartTrackingMethodInfo,
                            entityParameter,
                            Expression.Constant(entityType)));
                }

                var includedIndex = 0;

                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var includeLoadTreeNode in Children)
                {
                    blockExpressions.Add(
                        includeLoadTreeNode.Compile(
                            queryCompilationContext,
                            targetQuerySourceReferenceExpression,
                            entityParameter,
                            propertyExpressions,
                            trackingQuery,
                            asyncQuery,
                            ref includedIndex,
                            ref collectionIncludeId));
                }

                if (blockExpressions.Count > 1
                    || blockExpressions.Count == 1
                    && !trackingQuery)
                {
                    AwaitTaskExpressions(asyncQuery, blockExpressions);

                    var includeExpression
                        = blockExpressions.Last().Type == typeof(Task)
                            ? new TaskBlockingExpressionVisitor()
                                .Visit(
                                    Expression.Call(
                                        _includeAsyncMethodInfo
                                            .MakeGenericMethod(targetQuerySourceReferenceExpression.Type),
                                        EntityQueryModelVisitor.QueryContextParameter,
                                        targetQuerySourceReferenceExpression,
                                        Expression.NewArrayInit(typeof(object), propertyExpressions),
                                        Expression.Lambda(
                                            Expression.Block(blockExpressions),
                                            EntityQueryModelVisitor.QueryContextParameter,
                                            entityParameter,
                                            _includedParameter,
                                            CancellationTokenParameter),
                                        CancellationTokenParameter))
                            : Expression.Call(
                                _includeMethodInfo.MakeGenericMethod(targetQuerySourceReferenceExpression.Type),
                                EntityQueryModelVisitor.QueryContextParameter,
                                targetQuerySourceReferenceExpression,
                                Expression.NewArrayInit(typeof(object), propertyExpressions),
                                Expression.Lambda(
                                    Expression.Block(typeof(void), blockExpressions),
                                    EntityQueryModelVisitor.QueryContextParameter,
                                    entityParameter,
                                    _includedParameter));

                    ApplyIncludeExpressionsToQueryModel(
                        queryModel, targetQuerySourceReferenceExpression, includeExpression);
                }
            }

            protected static void ApplyIncludeExpressionsToQueryModel(
                QueryModel queryModel,
                QuerySourceReferenceExpression querySourceReferenceExpression,
                Expression expression)
            {
                var includeReplacingExpressionVisitor = new IncludeReplacingExpressionVisitor();

                foreach (var groupResultOperator
                    in queryModel.ResultOperators.OfType<GroupResultOperator>())
                {
                    var newElementSelector
                        = includeReplacingExpressionVisitor.Replace(
                            querySourceReferenceExpression,
                            expression,
                            groupResultOperator.ElementSelector);

                    if (!ReferenceEquals(newElementSelector, groupResultOperator.ElementSelector))
                    {
                        groupResultOperator.ElementSelector = newElementSelector;

                        return;
                    }
                }

                queryModel.SelectClause.TransformExpressions(
                    e => includeReplacingExpressionVisitor.Replace(
                        querySourceReferenceExpression,
                        expression,
                        e));
            }

            protected static void AwaitTaskExpressions(bool asyncQuery, List<Expression> blockExpressions)
            {
                if (asyncQuery)
                {
                    var taskExpressions = new List<Expression>();

                    foreach (var expression in blockExpressions.ToArray())
                    {
                        if (expression.Type == typeof(Task))
                        {
                            blockExpressions.Remove(expression);
                            taskExpressions.Add(expression);
                        }
                    }

                    if (taskExpressions.Count > 0)
                    {
                        blockExpressions.Add(
                            taskExpressions.Count == 1
                                ? taskExpressions[index: 0]
                                : Expression.Call(
                                    _awaitManyMethodInfo,
                                    Expression.NewArrayInit(
                                        typeof(Func<Task>),
                                        taskExpressions.Select(e => Expression.Lambda(e)))));
                    }
                }
            }

            private static readonly MethodInfo _awaitManyMethodInfo
                = typeof(IncludeLoadTreeNodeBase).GetTypeInfo()
                    .GetDeclaredMethod(nameof(_AwaitMany));

            // ReSharper disable once InconsistentNaming
            private static async Task _AwaitMany(IReadOnlyList<Func<Task>> taskFactories)
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < taskFactories.Count; i++)
                {
                    await taskFactories[i]();
                }
            }

            private class IncludeReplacingExpressionVisitor : RelinqExpressionVisitor
            {
                private QuerySourceReferenceExpression _querySourceReferenceExpression;
                private Expression _includeExpression;

                public Expression Replace(
                    QuerySourceReferenceExpression querySourceReferenceExpression,
                    Expression includeExpression,
                    Expression searchedExpression)
                {
                    _querySourceReferenceExpression = querySourceReferenceExpression;
                    _includeExpression = includeExpression;

                    return Visit(searchedExpression);
                }

                protected override Expression VisitQuerySourceReference(
                    QuerySourceReferenceExpression querySourceReferenceExpression)
                {
                    if (ReferenceEquals(querySourceReferenceExpression, _querySourceReferenceExpression))
                    {
                        _querySourceReferenceExpression = null;

                        return _includeExpression;
                    }

                    return querySourceReferenceExpression;
                }
            }
        }
    }
}
