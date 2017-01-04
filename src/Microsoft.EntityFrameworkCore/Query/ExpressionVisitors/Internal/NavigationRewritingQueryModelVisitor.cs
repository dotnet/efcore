// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionVisitors;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class NavigationRewritingQueryModelVisitor : QueryModelVisitorBase
    {
        private readonly SubqueryInjector _subqueryInjector;
        private readonly EntityQueryModelVisitor _queryModelVisitor;
        private readonly IAsyncQueryProvider _entityQueryProvider;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public NavigationRewritingQueryModelVisitor(
            [NotNull] EntityQueryModelVisitor queryModelVisitor,
            [NotNull] IAsyncQueryProvider entityQueryProvider)
            : base()
        {
            Check.NotNull(queryModelVisitor, nameof(queryModelVisitor));
            Check.NotNull(entityQueryProvider, nameof(entityQueryProvider));

            _subqueryInjector = new SubqueryInjector(queryModelVisitor);
            _queryModelVisitor = queryModelVisitor;
            _entityQueryProvider = entityQueryProvider;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitQueryModel(QueryModel queryModel)
        {
            base.VisitQueryModel(queryModel);

            var duplicateNavigationClauses =
            (
                from clause in queryModel.BodyClauses.OfType<NavigationClause>()
                group clause by new { clause.HeadReferenceExpression, clause.Navigation } into clauses
                where clauses.Count() > 1
                let replacement = clauses.First().TailReferenceExpression.ReferencedQuerySource
                from duplicate in clauses.Skip(1).Select(c => c.TailReferenceExpression.ReferencedQuerySource)
                select new { duplicate, replacement }
            )
            .ToDictionary(d => d.duplicate, d => d.replacement);

            foreach (var clause in queryModel.BodyClauses.OfType<NavigationClause>().ToArray())
            {
                queryModel.BodyClauses.Remove(clause);

                if (duplicateNavigationClauses.ContainsKey(clause.TailReferenceExpression.ReferencedQuerySource))
                {
                    continue;
                }

                var insertionIndex = 0;
                var outerQuerySource = clause.HeadReferenceExpression.ReferencedQuerySource;
                var bodyClause = outerQuerySource as IBodyClause;

                if (bodyClause != null)
                {
                    insertionIndex = queryModel.BodyClauses.IndexOf(bodyClause) + 1;
                }

                if (queryModel.MainFromClause == outerQuerySource || insertionIndex > 0)
                {
                    foreach (var innerClause in clause.Flatten().SelectMany(c => c.InnerClauses))
                    {
                        queryModel.BodyClauses.Insert(insertionIndex++, innerClause);
                    }
                }
                else
                {
                    // We may be visiting a subquery and this navigation could be
                    // intended for the outer query to use. Leave it here to be picked
                    // out later.
                    queryModel.BodyClauses.Add(clause);
                }
            }

            var qsreReplacingVisitor = new QsreReplacingExpressionVisitor(duplicateNavigationClauses);

            queryModel.TransformExpressions(qsreReplacingVisitor.Visit);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
        {
            var subqueryExpression = fromClause.FromExpression as SubQueryExpression;

            if (subqueryExpression != null)
            {
                var subquerySelectManyVisitor = new SubquerySelectManyNavigationRewritingExpressionVisitor(
                    CreateVisitorContext(subqueryExpression.QueryModel),
                    queryModel,
                    fromClause);

                subqueryExpression.QueryModel.TransformExpressions(subquerySelectManyVisitor.Visit);

                return;
            }

            var selectManyVisitor = new SelectManyNavigationRewritingExpressionVisitor(
                CreateVisitorContext(queryModel),
                fromClause);

            fromClause.TransformExpressions(selectManyVisitor.Visit);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            whereClause.TransformExpressions(CreateVisitor(queryModel).Visit);

            if (whereClause.Predicate.Type == typeof(bool?))
            {
                whereClause.Predicate = Expression.Equal(whereClause.Predicate, Expression.Constant(true, typeof(bool?)));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
        {
            var originalTypes = orderByClause.Orderings.Select(o => o.Expression.Type).ToList();

            var orderByExpressionVisitor = new OrderByNavigationRewritingExpressionVisitor(
                CreateVisitorContext(queryModel));

            orderByClause.TransformExpressions(orderByExpressionVisitor.Visit);

            for (var i = 0; i < orderByClause.Orderings.Count; i++)
            {
                orderByClause.Orderings[i].Expression = CompensateForNullabilityDifference(
                    orderByClause.Orderings[i].Expression,
                    originalTypes[i]);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
        {
            VisitJoinClauseInternal(joinClause, queryModel);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, GroupJoinClause groupJoinClause)
        {
            VisitJoinClauseInternal(joinClause, queryModel);
        }

        private void VisitJoinClauseInternal(JoinClause joinClause, QueryModel queryModel)
        {
            // Inner sequence

            var innerSequenceVisitor = new InnerSequenceNavigationRewritingExpressionVisitor(CreateVisitorContext(queryModel));

            joinClause.InnerSequence = innerSequenceVisitor.Visit(joinClause.InnerSequence);

            // Outer key

            joinClause.OuterKeySelector = CreateVisitor(queryModel).Visit(joinClause.OuterKeySelector);

            // Inner key

            var innerKeySelectorVisitor = new InnerKeySelectorNavigationRewritingExpressionVisitor(
                CreateVisitorContext(queryModel),
                innerSequenceVisitor.EncounteredOptionalNavigation);

            joinClause.InnerKeySelector = innerKeySelectorVisitor.Visit(joinClause.InnerKeySelector);

            // Compensate for nullability differences

            if (joinClause.OuterKeySelector.Type.IsNullableType()
                && !joinClause.InnerKeySelector.Type.IsNullableType())
            {
                joinClause.InnerKeySelector = Expression.Convert(
                    joinClause.InnerKeySelector,
                    joinClause.InnerKeySelector.Type.MakeNullable());
            }

            if (joinClause.InnerKeySelector.Type.IsNullableType()
                && !joinClause.OuterKeySelector.Type.IsNullableType())
            {
                joinClause.OuterKeySelector = Expression.Convert(
                    joinClause.OuterKeySelector,
                    joinClause.OuterKeySelector.Type.MakeNullable());
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
        {
            selectClause.Selector = _subqueryInjector.Visit(selectClause.Selector);

            var originalType = selectClause.Selector.Type;

            selectClause.TransformExpressions(CreateVisitor(queryModel).Visit);

            selectClause.Selector = CompensateForNullabilityDifference(selectClause.Selector, originalType);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            var visitor = CreateVisitor(queryModel);

            var allResultOperator = resultOperator as AllResultOperator;
            if (allResultOperator != null)
            {
                Func<AllResultOperator, Expression> expressionExtractor = o => o.Predicate;
                Action<AllResultOperator, Expression> adjuster = (o, e) => o.Predicate = e;
                VisitAndAdjustResultOperatorType(visitor, allResultOperator, expressionExtractor, adjuster);

                return;
            }

            var containsResultOperator = resultOperator as ContainsResultOperator;
            if (containsResultOperator != null)
            {
                Func<ContainsResultOperator, Expression> expressionExtractor = o => o.Item;
                Action<ContainsResultOperator, Expression> adjuster = (o, e) => o.Item = e;
                VisitAndAdjustResultOperatorType(visitor, containsResultOperator, expressionExtractor, adjuster);

                return;
            }

            var skipResultOperator = resultOperator as SkipResultOperator;
            if (skipResultOperator != null)
            {
                Func<SkipResultOperator, Expression> expressionExtractor = o => o.Count;
                Action<SkipResultOperator, Expression> adjuster = (o, e) => o.Count = e;
                VisitAndAdjustResultOperatorType(visitor, skipResultOperator, expressionExtractor, adjuster);

                return;
            }

            var takeResultOperator = resultOperator as TakeResultOperator;
            if (takeResultOperator != null)
            {
                Func<TakeResultOperator, Expression> expressionExtractor = o => o.Count;
                Action<TakeResultOperator, Expression> adjuster = (o, e) => o.Count = e;
                VisitAndAdjustResultOperatorType(visitor, takeResultOperator, expressionExtractor, adjuster);

                return;
            }

            var groupResultOperator = resultOperator as GroupResultOperator;
            if (groupResultOperator != null)
            {
                var originalKeySelectorType = groupResultOperator.KeySelector.Type;
                var originalElementSelectorType = groupResultOperator.ElementSelector.Type;

                resultOperator.TransformExpressions(visitor.Visit);

                groupResultOperator.KeySelector = CompensateForNullabilityDifference(
                    groupResultOperator.KeySelector,
                    originalKeySelectorType);

                groupResultOperator.ElementSelector = CompensateForNullabilityDifference(
                    groupResultOperator.ElementSelector,
                    originalElementSelectorType);

                return;
            }

            resultOperator.TransformExpressions(visitor.Visit);
        }

        private void VisitAndAdjustResultOperatorType<TResultOperator>(
            NavigationRewritingExpressionVisitor visitor,
            TResultOperator resultOperator,
            Func<TResultOperator, Expression> expressionExtractor,
            Action<TResultOperator, Expression> adjuster)
            where TResultOperator : ResultOperatorBase
        {
            var originalExpression = expressionExtractor(resultOperator);
            var originalType = originalExpression.Type;

            var translatedExpression = CompensateForNullabilityDifference(
                visitor.Visit(originalExpression),
                originalType);

            adjuster(resultOperator, translatedExpression);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
        {
            fromClause.TransformExpressions(CreateVisitor(queryModel).Visit);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitGroupJoinClause(GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
        {
            groupJoinClause.TransformExpressions(CreateVisitor(queryModel).Visit);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitOrdering(Ordering ordering, QueryModel queryModel, OrderByClause orderByClause, int index)
        {
            ordering.TransformExpressions(CreateVisitor(queryModel).Visit);
        }

        private NavigationRewritingExpressionVisitorContext CreateVisitorContext(QueryModel queryModel)
        {
            return new NavigationRewritingExpressionVisitorContext(
                queryModel, _queryModelVisitor, _entityQueryProvider);
        }

        private NavigationRewritingExpressionVisitor CreateVisitor(QueryModel queryModel)
        {
            return new NavigationRewritingExpressionVisitor(CreateVisitorContext(queryModel));
        }

        private static Expression CompensateForNullabilityDifference(Expression expression, Type originalType)
        {
            var newType = expression.Type;

            var needsTypeCompensation = (originalType != newType)
                                        && !originalType.IsNullableType()
                                        && newType.IsNullableType()
                                        && (originalType == newType.UnwrapNullableType());

            return needsTypeCompensation
                ? Expression.Convert(expression, originalType)
                : expression;
        }

        private class SubqueryInjector : RelinqExpressionVisitor
        {
            private readonly EntityQueryModelVisitor _queryModelVisitor;

            public SubqueryInjector(EntityQueryModelVisitor queryModelVisitor)
            {
                _queryModelVisitor = queryModelVisitor;
            }

            protected override Expression VisitSubQuery(SubQueryExpression expression)
                => expression;

            protected override Expression VisitMember(MemberExpression node)
            {
                return
                    _queryModelVisitor.BindNavigationPathPropertyExpression(
                        node,
                        (properties, querySource) =>
                        {
                            var navigations = properties.OfType<INavigation>().ToList();
                            var collectionNavigation = navigations.SingleOrDefault(n => n.IsCollection());

                            return collectionNavigation != null
                                ? InjectSubquery(node, collectionNavigation)
                                : default(Expression);
                        })
                    ?? base.VisitMember(node);
            }

            private static Expression InjectSubquery(Expression expression, INavigation collectionNavigation)
            {
                var targetType = collectionNavigation.GetTargetType().ClrType;
                var mainFromClause = new MainFromClause(targetType.Name.Substring(0, 1).ToLowerInvariant(), targetType, expression);
                var selector = new QuerySourceReferenceExpression(mainFromClause);

                var subqueryModel = new QueryModel(mainFromClause, new SelectClause(selector));
                var subqueryExpression = new SubQueryExpression(subqueryModel);

                var resultCollectionType = collectionNavigation.GetCollectionAccessor().CollectionType;

                var result = Expression.Call(
                    _materializeCollectionNavigationMethodInfo.MakeGenericMethod(targetType),
                    Expression.Constant(collectionNavigation), subqueryExpression);

                return resultCollectionType.GetTypeInfo().IsGenericType && resultCollectionType.GetGenericTypeDefinition() == typeof(ICollection<>)
                    ? (Expression)result
                    : Expression.Convert(result, resultCollectionType);
            }

            private static readonly MethodInfo _materializeCollectionNavigationMethodInfo
                = typeof(SubqueryInjector).GetTypeInfo()
                    .GetDeclaredMethod(nameof(MaterializeCollectionNavigation));

            [UsedImplicitly]
            private static ICollection<TEntity> MaterializeCollectionNavigation<TEntity>(INavigation navigation, IEnumerable<object> elements)
            {
                var collection = navigation.GetCollectionAccessor().Create(elements);

                return (ICollection<TEntity>)collection;
            }
        }

        private class NavigationRewritingExpressionVisitorContext
        {
            private readonly EntityQueryModelVisitor _queryModelVisitor;
            private readonly IAsyncQueryProvider _entityQueryProvider;
            
            public NavigationRewritingExpressionVisitorContext(
                QueryModel queryModel,
                EntityQueryModelVisitor queryModelVisitor,
                IAsyncQueryProvider entityQueryProvider)
            {
                QueryModel = queryModel;
                _queryModelVisitor = queryModelVisitor;
                _entityQueryProvider = entityQueryProvider;
            }
            
            public QueryModel QueryModel { get; }
            
            public IEnumerable<IncludeResultOperator> IncludeResultOperators
                => _queryModelVisitor.QueryCompilationContext.QueryAnnotations.OfType<IncludeResultOperator>();
            
            public NavigationRewritingQueryModelVisitor CreateSubQueryModelVisitor()
            {
                var subQueryModelVisitor = new NavigationRewritingQueryModelVisitor(
                    _queryModelVisitor,
                    _entityQueryProvider);

                return subQueryModelVisitor;
            }
            
            public virtual TResult BindNavigationPathPropertyExpression<TResult>(
                Expression propertyExpression,
                Func<IEnumerable<IPropertyBase>, IQuerySource, TResult> propertyBinder)
            {
                return _queryModelVisitor.BindNavigationPathPropertyExpression(propertyExpression, propertyBinder);
            }
            
            public ConstantExpression CreateEntityQueryable(IEntityType targetEntityType)
            {
                var method = _createEntityQueryableMethod.MakeGenericMethod(targetEntityType.ClrType);
                var instance = method.Invoke(null, new[] { _entityQueryProvider });

                return Expression.Constant(instance);
            }

            private static readonly MethodInfo _createEntityQueryableMethod
                = typeof(NavigationRewritingExpressionVisitorContext)
                    .GetTypeInfo().GetDeclaredMethod(nameof(CreateEntityQueryableConstructor));

            [UsedImplicitly]
            private static EntityQueryable<TResult> CreateEntityQueryableConstructor<TResult>(IAsyncQueryProvider entityQueryProvider)
                => new EntityQueryable<TResult>(entityQueryProvider);
        }
        
        private class NavigationRewritingExpressionVisitor : RelinqExpressionVisitor
        {
            protected NavigationRewritingExpressionVisitorContext Context { get; }
            
            public NavigationRewritingExpressionVisitor(NavigationRewritingExpressionVisitorContext context)
            {
                Context = context;
            }
            
            protected override Expression VisitUnary(UnaryExpression node)
            {
                var newOperand = Visit(node.Operand);

                return node.NodeType == ExpressionType.Convert && newOperand.Type == node.Type
                    ? newOperand
                    : node.Update(newOperand);
            }
            
            protected override Expression VisitSubQuery(SubQueryExpression expression)
            {
                Context.CreateSubQueryModelVisitor().VisitQueryModel(expression.QueryModel);

                // The subquery may have some left behind navigation joins for us to hoist out.

                foreach (var clause in expression.QueryModel.BodyClauses.OfType<NavigationClause>().ToArray())
                {
                    expression.QueryModel.BodyClauses.Remove(clause);
                    Context.QueryModel.BodyClauses.Add(clause);
                }

                return expression;
            }
            
            protected override Expression VisitBinary(BinaryExpression node)
            {
                var newLeft = Visit(node.Left);
                var newRight = Visit(node.Right);

                if (newLeft == node.Left && newRight == node.Right)
                {
                    return node;
                }

                var navigationClauses = Context.QueryModel.BodyClauses
                    .OfType<NavigationClause>()
                    .SelectMany(nj => nj.Flatten());

                var leftNavigationClause = navigationClauses
                    .FirstOrDefault(nj => ReferenceEquals(nj.TailReferenceExpression, newLeft));

                var rightNavigationClause = navigationClauses
                    .FirstOrDefault(nj => ReferenceEquals(nj.TailReferenceExpression, newRight));

                var leftJoin = leftNavigationClause?.UnderlyingJoinClause;
                var rightJoin = rightNavigationClause?.UnderlyingJoinClause;

                if (leftNavigationClause != null && rightNavigationClause != null)
                {
                    if (leftNavigationClause.Navigation.IsDependentToPrincipal()
                        && rightNavigationClause.Navigation.IsDependentToPrincipal())
                    {
                        newLeft = leftJoin?.OuterKeySelector;
                        newRight = rightJoin?.OuterKeySelector;

                        RemoveNavigationClause(leftNavigationClause);
                        RemoveNavigationClause(rightNavigationClause);
                    }
                }
                else
                {
                    if (leftNavigationClause != null)
                    {
                        var constantExpression = newRight as ConstantExpression;

                        if (constantExpression != null && constantExpression.Value == null)
                        {
                            if (leftNavigationClause.Navigation.IsDependentToPrincipal())
                            {
                                newLeft = leftJoin?.OuterKeySelector;

                                RemoveNavigationClause(leftNavigationClause);

                                if (newLeft?.Type == typeof(CompositeKey))
                                {
                                    newRight = CreateNullCompositeKey((NewExpression)newLeft);
                                }
                            }
                        }
                        else
                        {
                            newLeft = leftJoin?.InnerKeySelector;
                        }
                    }

                    if (rightNavigationClause != null)
                    {
                        var constantExpression = newLeft as ConstantExpression;

                        if (constantExpression != null && constantExpression.Value == null)
                        {
                            if (rightNavigationClause.Navigation.IsDependentToPrincipal())
                            {
                                newRight = rightJoin?.OuterKeySelector;

                                RemoveNavigationClause(rightNavigationClause);

                                if (newRight?.Type == typeof(CompositeKey))
                                {
                                    newLeft = CreateNullCompositeKey((NewExpression)newRight);
                                }
                            }
                        }
                        else
                        {
                            newRight = rightJoin?.InnerKeySelector;
                        }
                    }
                }

                if (node.NodeType != ExpressionType.ArrayIndex
                    && node.NodeType != ExpressionType.Coalesce
                    && newLeft != null
                    && newRight != null
                    && newLeft.Type != newRight.Type)
                {
                    if (newLeft.Type.IsNullableType() && !newRight.Type.IsNullableType())
                    {
                        newRight = Expression.Convert(newRight, newLeft.Type);
                    }
                    else if (!newLeft.Type.IsNullableType() && newRight.Type.IsNullableType())
                    {
                        newLeft = Expression.Convert(newLeft, newRight.Type);
                    }
                }

                return Expression.MakeBinary(node.NodeType, newLeft, newRight, node.IsLiftedToNull, node.Method);
            }
            
            protected override Expression VisitConditional(ConditionalExpression node)
            {
                var test = Visit(node.Test);
                if (test.Type == typeof(bool?))
                {
                    test = Expression.Equal(test, Expression.Constant(true, typeof(bool?)));
                }

                var ifTrue = Visit(node.IfTrue);
                var ifFalse = Visit(node.IfFalse);

                if (ifTrue.Type.IsNullableType()
                    && !ifFalse.Type.IsNullableType())
                {
                    ifFalse = Expression.Convert(ifFalse, ifTrue.Type);
                }

                if (ifFalse.Type.IsNullableType()
                    && !ifTrue.Type.IsNullableType())
                {
                    ifTrue = Expression.Convert(ifTrue, ifFalse.Type);
                }

                return test != node.Test || ifTrue != node.IfTrue || ifFalse != node.IfFalse
                    ? Expression.Condition(test, ifTrue, ifFalse)
                    : node;
            }
            
            protected override Expression VisitMember(MemberExpression node)
            {
                var result = Context.BindNavigationPathPropertyExpression(node, (properties, querySource) =>
                {
                    return RewriteNavigationProperties(
                        properties.ToList(),
                        querySource,
                        node,
                        node.Expression,
                        node.Member.Name,
                        node.Type,
                        e => Expression.MakeMemberAccess(e, node.Member));
                });

                if (result != null)
                {
                    return result;
                }

                return Expression.MakeMemberAccess(Visit(node.Expression), node.Member);
            }
            
            protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
            {
                var newExpression = CompensateForNullabilityDifference(
                    Visit(node.Expression),
                    node.Expression.Type);

                return node.Update(newExpression);
            }
            
            protected override ElementInit VisitElementInit(ElementInit node)
            {
                var originalArgumentTypes = node.Arguments.Select(a => a.Type).ToList();
                var newArguments = node.Arguments.Select(Visit).ToList();

                for (var i = 0; i < newArguments.Count; i++)
                {
                    newArguments[i] = CompensateForNullabilityDifference(newArguments[i], originalArgumentTypes[i]);
                }

                return node.Update(newArguments);
            }
            
            protected override Expression VisitNewArray(NewArrayExpression node)
            {
                var originalExpressionTypes = node.Expressions.Select(e => e.Type).ToList();
                var newExpressions = node.Expressions.Select(Visit).ToList();

                for (var i = 0; i < newExpressions.Count; i++)
                {
                    newExpressions[i] = CompensateForNullabilityDifference(newExpressions[i], originalExpressionTypes[i]);
                }

                return node.Update(newExpressions);
            }
            
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (EntityQueryModelVisitor.IsPropertyMethod(node.Method))
                {
                    var result = Context.BindNavigationPathPropertyExpression(node, (properties, querySource) =>
                    {
                        return RewriteNavigationProperties(
                            properties.ToList(),
                            querySource,
                            node,
                            node.Arguments[0],
                            (string)((ConstantExpression)node.Arguments[1]).Value,
                            node.Type,
                            e => Expression.Call(node.Method, e, node.Arguments[1]));
                    });

                    if (result != null)
                    {
                        return result;
                    }

                    var propertyArguments = node.Arguments.Select(Visit).ToList();

                    return Expression.Call(node.Method, propertyArguments[0], propertyArguments[1]);
                }

                var newObject = Visit(node.Object);
                var newArguments = node.Arguments.Select(Visit);

                if (newObject != node.Object)
                {
                    var nullConditionalExpression = newObject as NullConditionalExpression;

                    if (nullConditionalExpression != null)
                    {
                        var newMethodCallExpression = node.Update(nullConditionalExpression.AccessOperation, newArguments);

                        return new NullConditionalExpression(newObject, node.Object, newMethodCallExpression);
                    }
                }

                return node.Update(newObject, newArguments);
            }
            
            protected virtual Expression RewriteNavigationProperties(
                List<IPropertyBase> properties,
                IQuerySource querySource,
                Expression expression,
                Expression declaringExpression,
                string propertyName,
                Type propertyType,
                Func<Expression, Expression> propertyCreator)
            {
                var navigations = properties.OfType<INavigation>().ToList();

                var foreignKeyMemberAccess = TryCreateForeignKeyMemberAccess(navigations, propertyName, declaringExpression);
                if (foreignKeyMemberAccess != null)
                {
                    return foreignKeyMemberAccess;
                }

                if (navigations.Any())
                {
                    var outerQuerySourceReferenceExpression = new QuerySourceReferenceExpression(querySource);

                    var navigationResultExpression = RewriteNavigationsIntoJoins(
                        outerQuerySourceReferenceExpression,
                        navigations,
                        properties.Count == navigations.Count ? null : propertyType,
                        propertyCreator);

                    var qsre = navigationResultExpression as QuerySourceReferenceExpression;

                    if (qsre != null)
                    {
                        foreach (var includeResultOperator in Context.IncludeResultOperators)
                        {
                            if (includeResultOperator.PathFromQuerySource == expression)
                            {
                                includeResultOperator.QuerySource = qsre.ReferencedQuerySource;
                                includeResultOperator.PathFromQuerySource = qsre;
                            }
                        }
                    }

                    return navigationResultExpression;
                }

                return default(Expression);
            }
            
            protected virtual bool CanOptimizeForeignKeyMemberAccess(
                INavigation navigation,
                Expression declaringExpression)
            {
                return true;
            }
            
            protected Expression TryCreateForeignKeyMemberAccess(
                IEnumerable<INavigation> navigations,
                string propertyName,
                Expression declaringExpression)
            {
                var navigation = navigations.FirstOrDefault();

                if (navigations.Count() == 1
                    && navigation.IsDependentToPrincipal()
                    && CanOptimizeForeignKeyMemberAccess(navigation, declaringExpression))
                {
                    var foreignKeyMemberAccess = CreateForeignKeyMemberAccess(propertyName, declaringExpression, navigation);

                    if (foreignKeyMemberAccess != null)
                    {
                        return foreignKeyMemberAccess;
                    }
                }

                return null;
            }

            private Expression RewriteNavigationsIntoJoins(
                QuerySourceReferenceExpression outerQuerySourceReferenceExpression,
                IEnumerable<INavigation> navigations,
                Type propertyType,
                Func<Expression, Expression> propertyCreator)
            {
                QuerySourceReferenceExpression innerQuerySourceReferenceExpression;
                NavigationClause navigationClause = null;
                IList navigationClauses = Context.QueryModel.BodyClauses;
                var optionalNavigationInChain = false;

                foreach (var navigation in navigations)
                {
                    var addNullCheckToOuterKeySelector = optionalNavigationInChain;

                    if (!navigation.ForeignKey.IsRequired || !navigation.IsDependentToPrincipal())
                    {
                        optionalNavigationInChain = true;
                    }

                    var targetEntityType = navigation.GetTargetType();

                    if (navigation.IsCollection())
                    {
                        Context.QueryModel.MainFromClause.FromExpression = Context.CreateEntityQueryable(targetEntityType);

                        var leftKeyAccess = CreateKeyAccessExpression(
                            outerQuerySourceReferenceExpression,
                            navigation.IsDependentToPrincipal()
                                ? navigation.ForeignKey.Properties
                                : navigation.ForeignKey.PrincipalKey.Properties);

                        var rightKeyAccess = CreateKeyAccessExpression(
                            new QuerySourceReferenceExpression(Context.QueryModel.MainFromClause),
                            navigation.IsDependentToPrincipal()
                                ? navigation.ForeignKey.PrincipalKey.Properties
                                : navigation.ForeignKey.Properties);

                        Context.QueryModel.BodyClauses.Add(
                            new WhereClause(
                                CreateKeyComparisonExpression(leftKeyAccess, rightKeyAccess)));

                        return Context.QueryModel.MainFromClause.FromExpression;
                    }

                    navigationClause =
                    (
                        from clause in navigationClauses.OfType<NavigationClause>()
                        let source1 = clause.HeadReferenceExpression.ReferencedQuerySource
                        let source2 = outerQuerySourceReferenceExpression.ReferencedQuerySource
                        where source1 == source2 && clause.Navigation == navigation
                        select clause

                    ).FirstOrDefault();

                    if (navigationClause == null)
                    {
                        var joinClause = BuildJoinFromNavigation(
                            outerQuerySourceReferenceExpression,
                            navigation,
                            targetEntityType,
                            addNullCheckToOuterKeySelector,
                            out innerQuerySourceReferenceExpression);

                        if (optionalNavigationInChain)
                        {
                            navigationClause = RewriteNavigationIntoGroupJoin(
                                joinClause,
                                navigation,
                                targetEntityType,
                                outerQuerySourceReferenceExpression,
                                null,
                                new List<IBodyClause>(),
                                new List<ResultOperatorBase> { new DefaultIfEmptyResultOperator(null) });
                        }
                        else
                        {
                            navigationClause = new NavigationClause(
                                navigation,
                                outerQuerySourceReferenceExpression,
                                innerQuerySourceReferenceExpression,
                                joinClause);
                        }

                        navigationClauses.Add(navigationClause);
                    }

                    navigationClauses = navigationClause.ChainedNavigations;

                    outerQuerySourceReferenceExpression = navigationClause.TailReferenceExpression;
                }

                if (propertyType == null)
                {
                    return outerQuerySourceReferenceExpression;
                }

                if (optionalNavigationInChain)
                {
                    return new NullConditionalExpression(
                        outerQuerySourceReferenceExpression,
                        outerQuerySourceReferenceExpression,
                        propertyCreator(outerQuerySourceReferenceExpression));
                }

                return propertyCreator(outerQuerySourceReferenceExpression);
            }
            
            protected NavigationClause RewriteNavigationIntoGroupJoin(
                JoinClause joinClause,
                INavigation navigation,
                IEntityType targetEntityType,
                QuerySourceReferenceExpression outerReferenceExpression,
                MainFromClause groupJoinSubqueryMainFromClause,
                ICollection<IBodyClause> groupJoinSubqueryBodyClauses,
                ICollection<ResultOperatorBase> groupJoinSubqueryResultOperators)
            {
                var groupJoinClause
                    = new GroupJoinClause(
                        joinClause.ItemName + "_group",
                        typeof(IEnumerable<>).MakeGenericType(targetEntityType.ClrType),
                        joinClause);

                var groupJoinReferenceExpression = new QuerySourceReferenceExpression(groupJoinClause);

                var groupJoinSubqueryModelMainFromClause = new MainFromClause(
                    joinClause.ItemName + "_groupItem",
                    joinClause.ItemType,
                    groupJoinReferenceExpression);

                var newQuerySourceReferenceExpression = new QuerySourceReferenceExpression(groupJoinSubqueryModelMainFromClause);

                var groupJoinSubqueryModel = new QueryModel(
                    groupJoinSubqueryModelMainFromClause,
                    new SelectClause(newQuerySourceReferenceExpression));

                foreach (var groupJoinSubqueryBodyClause in groupJoinSubqueryBodyClauses)
                {
                    groupJoinSubqueryModel.BodyClauses.Add(groupJoinSubqueryBodyClause);
                }

                foreach (var groupJoinSubqueryResultOperator in groupJoinSubqueryResultOperators)
                {
                    groupJoinSubqueryModel.ResultOperators.Add(groupJoinSubqueryResultOperator);
                }

                if (groupJoinSubqueryMainFromClause != null && (groupJoinSubqueryBodyClauses.Any() || groupJoinSubqueryResultOperators.Any()))
                {
                    var querySourceMapping = new QuerySourceMapping();
                    querySourceMapping.AddMapping(groupJoinSubqueryMainFromClause, newQuerySourceReferenceExpression);

                    groupJoinSubqueryModel.TransformExpressions(e =>
                        ReferenceReplacingExpressionVisitor
                            .ReplaceClauseReferences(e, querySourceMapping, throwOnUnmappedReferences: false));
                }

                var defaultIfEmptySubquery = new SubQueryExpression(groupJoinSubqueryModel);

                var defaultIfEmptyAdditionalFromClause
                    = new AdditionalFromClause(joinClause.ItemName, joinClause.ItemType, defaultIfEmptySubquery);

                var additionalFromClauseReferenceExpression
                    = new QuerySourceReferenceExpression(defaultIfEmptyAdditionalFromClause);

                return new NavigationClause(
                    navigation,
                    outerReferenceExpression,
                    additionalFromClauseReferenceExpression,
                    groupJoinClause,
                    defaultIfEmptyAdditionalFromClause);
            }
            
            protected JoinClause BuildJoinFromNavigation(
                QuerySourceReferenceExpression querySourceReferenceExpression,
                INavigation navigation,
                IEntityType targetEntityType,
                bool addNullCheckToOuterKeySelector,
                out QuerySourceReferenceExpression innerQuerySourceReferenceExpression)
            {
                var outerKeySelector = CreateKeyAccessExpression(
                    querySourceReferenceExpression,
                    navigation.IsDependentToPrincipal()
                        ? navigation.ForeignKey.Properties
                        : navigation.ForeignKey.PrincipalKey.Properties,
                    addNullCheck: addNullCheckToOuterKeySelector);

                var joinClause = new JoinClause(
                    $"{querySourceReferenceExpression.ReferencedQuerySource.ItemName}.{navigation.Name}", // Interpolation okay; strings
                    targetEntityType.ClrType,
                    Context.CreateEntityQueryable(targetEntityType),
                    outerKeySelector,
                    Expression.Constant(null));

                innerQuerySourceReferenceExpression = new QuerySourceReferenceExpression(joinClause);

                var innerKeySelector = CreateKeyAccessExpression(
                    innerQuerySourceReferenceExpression,
                    navigation.IsDependentToPrincipal()
                        ? navigation.ForeignKey.PrincipalKey.Properties
                        : navigation.ForeignKey.Properties);

                if (innerKeySelector.Type != joinClause.OuterKeySelector.Type)
                {
                    if (innerKeySelector.Type.IsNullableType())
                    {
                        joinClause.OuterKeySelector = Expression.Convert(
                            joinClause.OuterKeySelector,
                            innerKeySelector.Type);
                    }
                    else
                    {
                        innerKeySelector = Expression.Convert(
                            innerKeySelector,
                            joinClause.OuterKeySelector.Type);
                    }
                }

                joinClause.InnerKeySelector = innerKeySelector;

                return joinClause;
            }

            private void RemoveNavigationClause(NavigationClause clause)
            {
                if (!Context.QueryModel.BodyClauses.Remove(clause))
                {
                    var flattened = Context.QueryModel.BodyClauses
                        .OfType<NavigationClause>()
                        .SelectMany(nj => nj.Flatten());

                    foreach (var other in flattened)
                    {
                        other.ChainedNavigations.Remove(clause);
                    }
                }
            }
            
            protected static BinaryExpression CreateKeyComparisonExpression(Expression leftExpression, Expression rightExpression)
            {
                if (leftExpression.Type != rightExpression.Type)
                {
                    if (leftExpression.Type.IsNullableType())
                    {
                        Debug.Assert(leftExpression.Type.UnwrapNullableType() == rightExpression.Type);

                        rightExpression = Expression.Convert(rightExpression, leftExpression.Type);
                    }
                    else
                    {
                        Debug.Assert(rightExpression.Type.IsNullableType());
                        Debug.Assert(rightExpression.Type.UnwrapNullableType() == leftExpression.Type);

                        leftExpression = Expression.Convert(leftExpression, rightExpression.Type);
                    }
                }

                return Expression.Equal(leftExpression, rightExpression);
            }
            
            protected static Expression CreateKeyAccessExpression(
                Expression target,
                IReadOnlyList<IProperty> properties,
                bool addNullCheck = false)
            {
                if (properties.Count == 1)
                {
                    return CreatePropertyExpression(target, properties[0], addNullCheck);
                }

                var initializers =
                    from property in properties
                    let expression = CreatePropertyExpression(target, property, addNullCheck)
                    select Expression.Convert(expression, typeof(object)) as Expression;

                return Expression.New(
                    CompositeKey.CompositeKeyCtor,
                    Expression.NewArrayInit(
                        typeof(object),
                        initializers.ToArray()));
            }

            private static Expression CreateForeignKeyMemberAccess(string propertyName, Expression declaringExpression, INavigation navigation)
            {
                var principalProperties = navigation.ForeignKey.PrincipalKey.Properties;
                var dependentProperties = navigation.ForeignKey.Properties;

                if (principalProperties.Count == 1)
                {
                    Debug.Assert(dependentProperties.Count == 1);

                    var principalKeyProperty = principalProperties[0];

                    if (principalKeyProperty.Name == propertyName && principalKeyProperty.ClrType == dependentProperties[0].ClrType)
                    {
                        var declaringMethodCallExpression = declaringExpression as MethodCallExpression;

                        if (declaringMethodCallExpression != null
                            && EntityQueryModelVisitor.IsPropertyMethod(declaringMethodCallExpression.Method))
                        {
                            return CreateKeyAccessExpression(declaringMethodCallExpression.Arguments[0], dependentProperties);
                        }

                        var declaringMemberExpression = declaringExpression as MemberExpression;

                        if (declaringMemberExpression != null)
                        {
                            return CreateKeyAccessExpression(declaringMemberExpression.Expression, dependentProperties);
                        }
                    }
                }

                return null;
            }

            private static NewExpression CreateNullCompositeKey(NewExpression otherNewExpression)
            {
                var otherNewArrayExpression = (NewArrayExpression)otherNewExpression.Arguments.Single();

                return Expression.New(
                    CompositeKey.CompositeKeyCtor,
                    Expression.NewArrayInit(
                        typeof(object),
                        Enumerable.Repeat(
                            Expression.Constant(null),
                            otherNewArrayExpression.Expressions.Count)));
            }

            private static readonly MethodInfo _propertyMethodInfo
                = typeof(EF).GetTypeInfo().GetDeclaredMethod(nameof(Property));

            private static Expression CreatePropertyExpression(Expression target, IProperty property, bool addNullCheck)
            {
                var propertyExpression = (Expression)Expression.Call(
                    null,
                    _propertyMethodInfo.MakeGenericMethod(property.ClrType),
                    target,
                    Expression.Constant(property.Name));

                if (!addNullCheck)
                {
                    return propertyExpression;
                }

                var constantNull = property.ClrType.IsNullableType()
                    ? Expression.Constant(null, property.ClrType)
                    : Expression.Constant(null, property.ClrType.MakeNullable());

                if (!property.ClrType.IsNullableType())
                {
                    propertyExpression = Expression.Convert(propertyExpression, propertyExpression.Type.MakeNullable());
                }

                return Expression.Condition(
                    Expression.NotEqual(
                        target,
                        Expression.Constant(null, target.Type)),
                    propertyExpression,
                    constantNull);
            }
        }
        
        private class NavigationClause : IBodyClause
        {
            public NavigationClause(
                INavigation navigation,
                QuerySourceReferenceExpression headReferenceExpression,
                QuerySourceReferenceExpression tailReferenceExpression,
                params IBodyClause[] innerClauses)
            {
                Navigation = navigation;
                HeadReferenceExpression = headReferenceExpression;
                TailReferenceExpression = tailReferenceExpression;
                InnerClauses = innerClauses.ToArray();
                ChainedNavigations = new List<NavigationClause>();
            }
            
            public INavigation Navigation { get; }
            
            public IEnumerable<IBodyClause> InnerClauses { get; }
            
            public QuerySourceReferenceExpression HeadReferenceExpression { get; }
            
            public QuerySourceReferenceExpression TailReferenceExpression { get; }
            
            public List<NavigationClause> ChainedNavigations { get; }

            public JoinClause UnderlyingJoinClause
            {
                get
                {
                    var firstInnerClause = InnerClauses.First();

                    return (firstInnerClause as JoinClause) 
                        ?? (firstInnerClause as GroupJoinClause)?.JoinClause;
                }
            }
            
            public virtual void Accept(IQueryModelVisitor visitor, QueryModel queryModel, int index)
            {
                foreach (var clause in InnerClauses.Concat(ChainedNavigations))
                {
                    clause.Accept(visitor, queryModel, index);
                }
            }
            
            public virtual IBodyClause Clone(CloneContext cloneContext)
            {
                throw new NotImplementedException();
            }
            
            public virtual IEnumerable<NavigationClause> Flatten()
            {
                yield return this;

                foreach (var clause in ChainedNavigations.SelectMany(n => n.Flatten()))
                {
                    yield return clause;
                }
            }
            
            public void TransformExpressions(Func<Expression, Expression> transformation)
            {
                foreach (var clause in InnerClauses.Concat(ChainedNavigations))
                {
                    clause.TransformExpressions(transformation);
                }
            }
        }

        private class SelectManyNavigationRewritingExpressionVisitor : NavigationRewritingExpressionVisitor
        {
            private AdditionalFromClause _selectManyFromClause;
            
            public SelectManyNavigationRewritingExpressionVisitor(
                NavigationRewritingExpressionVisitorContext context,
                AdditionalFromClause selectManyFromClause)
                : base(context)
            {
                _selectManyFromClause = selectManyFromClause;
            }
            
            protected override Expression RewriteNavigationProperties(
                List<IPropertyBase> properties,
                IQuerySource querySource,
                Expression expression,
                Expression declaringExpression,
                string propertyName,
                Type propertyType,
                Func<Expression, Expression> propertyCreator)
            {
                var navigations = properties.OfType<INavigation>().ToList();

                if (navigations.Any() && navigations.Last().IsCollection())
                {
                    return RewriteSelectManyIntoJoins(querySource, navigations);
                }

                return base.RewriteNavigationProperties(
                    properties,
                    querySource,
                    expression,
                    declaringExpression,
                    propertyName,
                    propertyType,
                    propertyCreator);
            }

            private Expression RewriteSelectManyIntoJoins(
                IQuerySource querySource,
                IEnumerable<INavigation> navigations)
            {
                var outerQuerySourceReferenceExpression = new QuerySourceReferenceExpression(querySource);
                var innerQuerySourceReferenceExpression = outerQuerySourceReferenceExpression;
                var additionalJoinIndex = Context.QueryModel.BodyClauses.IndexOf(_selectManyFromClause);

                Context.QueryModel.BodyClauses.RemoveAt(additionalJoinIndex);

                foreach (var navigation in navigations)
                {
                    var targetEntityType = navigation.GetTargetType();

                    var joinClause = BuildJoinFromNavigation(
                        outerQuerySourceReferenceExpression,
                        navigation,
                        targetEntityType,
                        false,
                        out innerQuerySourceReferenceExpression);

                    Context.QueryModel.BodyClauses.Insert(additionalJoinIndex++, joinClause);

                    outerQuerySourceReferenceExpression = innerQuerySourceReferenceExpression;
                }

                var querySourceMapping = new QuerySourceMapping();
                querySourceMapping.AddMapping(_selectManyFromClause, outerQuerySourceReferenceExpression);

                Context.QueryModel.TransformExpressions(e =>
                    ReferenceReplacingExpressionVisitor
                        .ReplaceClauseReferences(e, querySourceMapping, throwOnUnmappedReferences: false));

                foreach (var includeResultOperator in Context.IncludeResultOperators)
                {
                    var qsre = includeResultOperator.PathFromQuerySource as QuerySourceReferenceExpression;

                    if (qsre?.ReferencedQuerySource == _selectManyFromClause)
                    {
                        includeResultOperator.PathFromQuerySource = outerQuerySourceReferenceExpression;
                        includeResultOperator.QuerySource = outerQuerySourceReferenceExpression.ReferencedQuerySource;
                    }
                }

                return outerQuerySourceReferenceExpression;
            }
        }
        
        private class SubquerySelectManyNavigationRewritingExpressionVisitor : NavigationRewritingExpressionVisitor
        {
            private QueryModel _parentQueryModel;
            private AdditionalFromClause _selectManyFromClause;
            
            public SubquerySelectManyNavigationRewritingExpressionVisitor(
                NavigationRewritingExpressionVisitorContext context,
                QueryModel parentQueryModel,
                AdditionalFromClause selectManyFromClause)
                : base(context)
            {
                _parentQueryModel = parentQueryModel;
                _selectManyFromClause = selectManyFromClause;
            }
            
            protected override Expression RewriteNavigationProperties(
                List<IPropertyBase> properties,
                IQuerySource querySource,
                Expression expression,
                Expression declaringExpression,
                string propertyName,
                Type propertyType,
                Func<Expression, Expression> propertyCreator)
            {
                var navigations = properties.OfType<INavigation>().ToList();

                if (navigations.Any() && navigations.Last().IsCollection())
                {
                    return RewriteSelectManyInsideSubqueryIntoJoins(querySource, navigations);
                }

                return base.RewriteNavigationProperties(
                    properties,
                    querySource,
                    expression,
                    declaringExpression,
                    propertyName,
                    propertyType,
                    propertyCreator);
            }

            private Expression RewriteSelectManyInsideSubqueryIntoJoins(
                IQuerySource querySource,
                IEnumerable<INavigation> navigations)
            {
                var outerQuerySourceReferenceExpression = new QuerySourceReferenceExpression(querySource);
                var innerQuerySourceReferenceExpression = outerQuerySourceReferenceExpression;
                var additionalJoinIndex = _parentQueryModel.BodyClauses.IndexOf(_selectManyFromClause);

                _parentQueryModel.BodyClauses.RemoveAt(additionalJoinIndex);

                foreach (var navigation in navigations.Take(navigations.Count() - 1))
                {
                    var targetEntityType = navigation.GetTargetType();

                    var joinClause = BuildJoinFromNavigation(
                        outerQuerySourceReferenceExpression,
                        navigation,
                        targetEntityType,
                        false,
                        out innerQuerySourceReferenceExpression);

                    _parentQueryModel.BodyClauses.Insert(additionalJoinIndex++, joinClause);
                    outerQuerySourceReferenceExpression = innerQuerySourceReferenceExpression;
                }

                var collectionNavigation = navigations.Last();
                var collectionItemType = collectionNavigation.GetTargetType();

                var collectionJoinClause = BuildJoinFromNavigation(
                    outerQuerySourceReferenceExpression,
                    collectionNavigation,
                    collectionItemType,
                    false,
                    out innerQuerySourceReferenceExpression);

                var navigationClause = RewriteNavigationIntoGroupJoin(
                    collectionJoinClause,
                    collectionNavigation,
                    collectionItemType,
                    outerQuerySourceReferenceExpression,
                    Context.QueryModel.MainFromClause,
                    Context.QueryModel.BodyClauses,
                    Context.QueryModel.ResultOperators);

                _parentQueryModel.BodyClauses.Add(navigationClause);

                var navigationReferenceExpression = navigationClause.TailReferenceExpression;

                var querySourceMapping = new QuerySourceMapping();
                querySourceMapping.AddMapping(_selectManyFromClause, navigationReferenceExpression);

                _parentQueryModel.TransformExpressions(e =>
                    ReferenceReplacingExpressionVisitor
                        .ReplaceClauseReferences(e, querySourceMapping, throwOnUnmappedReferences: false));

                foreach (var includeResultOperator in Context.IncludeResultOperators)
                {
                    var qsre = includeResultOperator.PathFromQuerySource as QuerySourceReferenceExpression;

                    if (qsre?.ReferencedQuerySource == _selectManyFromClause)
                    {
                        includeResultOperator.PathFromQuerySource = navigationReferenceExpression;
                        includeResultOperator.QuerySource = navigationReferenceExpression.ReferencedQuerySource;
                    }
                }

                return navigationReferenceExpression;
            }
        }
        
        private class InnerSequenceNavigationRewritingExpressionVisitor : NavigationRewritingExpressionVisitor
        {
            public bool EncounteredOptionalNavigation { get; private set; }
            
            public InnerSequenceNavigationRewritingExpressionVisitor(
                NavigationRewritingExpressionVisitorContext context)
                : base(context)
            {
            }
            
            protected override Expression RewriteNavigationProperties(
                List<IPropertyBase> properties,
                IQuerySource querySource,
                Expression expression,
                Expression declaringExpression,
                string propertyName,
                Type propertyType,
                Func<Expression, Expression> propertyCreator)
            {
                var optionalNavigations =
                    from navigation in properties.OfType<INavigation>()
                    where !navigation.IsDependentToPrincipal() ||
                        !navigation.ForeignKey.IsRequired
                    select navigation;

                if (optionalNavigations.Any())
                {
                    EncounteredOptionalNavigation = true;
                }

                return base.RewriteNavigationProperties(
                    properties,
                    querySource,
                    expression,
                    declaringExpression,
                    propertyName,
                    propertyType,
                    propertyCreator);
            }
        }
        
        private class InnerKeySelectorNavigationRewritingExpressionVisitor : NavigationRewritingExpressionVisitor
        {
            private bool _requiresNullReferenceProtection;
            
            public InnerKeySelectorNavigationRewritingExpressionVisitor(
                NavigationRewritingExpressionVisitorContext context,
                bool requiresNullReferenceProtection)
                : base(context)
            {
                _requiresNullReferenceProtection = requiresNullReferenceProtection;
            }
            
            protected override Expression VisitMember(MemberExpression node)
            {
                var result = base.VisitMember(node);

                var memberExpression = result as MemberExpression;

                if (memberExpression != null && _requiresNullReferenceProtection)
                {
                    return new NullConditionalExpression(
                        memberExpression.Expression,
                        memberExpression.Expression,
                        memberExpression);
                }

                return result;
            }
            
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                var result = base.VisitMethodCall(node);

                var methodCallExpression = result as MethodCallExpression;

                if (methodCallExpression != null && _requiresNullReferenceProtection)
                {
                    return new NullConditionalExpression(
                        methodCallExpression.Arguments[0],
                        methodCallExpression.Arguments[0],
                        methodCallExpression);
                }

                return result;
            }
            
            protected override Expression RewriteNavigationProperties(
                List<IPropertyBase> properties,
                IQuerySource querySource,
                Expression expression,
                Expression declaringExpression,
                string propertyName,
                Type propertyType,
                Func<Expression, Expression> propertyCreator)
            {
                var navigations = properties.OfType<INavigation>().ToList();

                var foreignKeyMemberAccess = TryCreateForeignKeyMemberAccess(navigations, propertyName, declaringExpression);

                if (foreignKeyMemberAccess != null)
                {
                    return foreignKeyMemberAccess;
                }

                if (navigations.Any())
                {
                    return CreateSubqueryForNavigations(
                        querySource,
                        navigations,
                        propertyCreator);
                }

                return base.RewriteNavigationProperties(
                    properties,
                    querySource,
                    expression,
                    declaringExpression,
                    propertyName,
                    propertyType,
                    propertyCreator);
            }

            private Expression CreateSubqueryForNavigations(
                IQuerySource querySource,
                ICollection<INavigation> navigations,
                Func<Expression, Expression> propertyCreator)
            {
                var firstNavigation = navigations.First();
                var targetEntityType = firstNavigation.GetTargetType();

                var mainFromClause = new MainFromClause(
                    "subQuery",
                    targetEntityType.ClrType,
                    Context.CreateEntityQueryable(targetEntityType));

                var outerQuerySourceReferenceExpression = new QuerySourceReferenceExpression(querySource);
                var innerQuerySourceReferenceExpression = new QuerySourceReferenceExpression(mainFromClause);
                var subQueryModel = new QueryModel(mainFromClause, new SelectClause(innerQuerySourceReferenceExpression));

                var leftKeyAccess = CreateKeyAccessExpression(
                    innerQuerySourceReferenceExpression,
                    firstNavigation.IsDependentToPrincipal()
                        ? firstNavigation.ForeignKey.PrincipalKey.Properties
                        : firstNavigation.ForeignKey.Properties);

                var rightKeyAccess = CreateKeyAccessExpression(
                    outerQuerySourceReferenceExpression,
                    firstNavigation.IsDependentToPrincipal()
                        ? firstNavigation.ForeignKey.Properties
                        : firstNavigation.ForeignKey.PrincipalKey.Properties);

                subQueryModel.BodyClauses.Add(new WhereClause(
                    CreateKeyComparisonExpression(leftKeyAccess, rightKeyAccess)));

                subQueryModel.ResultOperators.Add(new FirstResultOperator(returnDefaultWhenEmpty: true));

                var selectClauseExpression = navigations.Skip(1).Aggregate(
                    (Expression)innerQuerySourceReferenceExpression,
                    (current, navigation) => Expression.Property(current, navigation.Name));

                subQueryModel.SelectClause = new SelectClause(propertyCreator(selectClauseExpression));

                if (navigations.Count > 1)
                {
                    Context.CreateSubQueryModelVisitor().VisitQueryModel(subQueryModel);

                    if (!_requiresNullReferenceProtection)
                    {
                        var newSelector = subQueryModel.SelectClause.Selector;
                        if (newSelector.NodeType == ExpressionType.Convert)
                        {
                            var unaryExpression = (UnaryExpression)newSelector;
                            var originalType = unaryExpression.Operand.Type;

                            if (originalType.IsNullableType() 
                                && originalType.UnwrapNullableType() == newSelector.Type)
                            {
                                subQueryModel.SelectClause.Selector = unaryExpression.Operand;
                            }
                        }
                    }
                }

                return new SubQueryExpression(subQueryModel);
            }
        }
        
        private class OrderByNavigationRewritingExpressionVisitor : NavigationRewritingExpressionVisitor
        {
            public OrderByNavigationRewritingExpressionVisitor(
                NavigationRewritingExpressionVisitorContext context)
                : base(context)
            {
            }
            
            protected override bool CanOptimizeForeignKeyMemberAccess(INavigation navigation, Expression declaringExpression)
            {
                var canPerformOptimization = base.CanOptimizeForeignKeyMemberAccess(navigation, declaringExpression);

                var qsre = (declaringExpression as MemberExpression)?.Expression as QuerySourceReferenceExpression;
                if (qsre == null)
                {
                    var methodCallExpression = declaringExpression as MethodCallExpression;
                    if (methodCallExpression != null && EntityQueryModelVisitor.IsPropertyMethod(methodCallExpression.Method))
                    {
                        qsre = methodCallExpression.Arguments[0] as QuerySourceReferenceExpression;
                    }
                }

                if (qsre != null)
                {
                    var qsreFindingVisitor = new QsreWithNavigationFindingExpressionVisitor(qsre, navigation);
                    qsreFindingVisitor.Visit(Context.QueryModel.SelectClause.Selector);

                    if (qsreFindingVisitor.SearchedQsreFound)
                    {
                        canPerformOptimization = false;
                    }
                }

                return canPerformOptimization;
            }
        }
        
        private class QsreReplacingExpressionVisitor : RelinqExpressionVisitor
        {
            private readonly IDictionary<IQuerySource, IQuerySource> _replacementDictionary;
            
            public QsreReplacingExpressionVisitor(IDictionary<IQuerySource, IQuerySource> replacementDictionary)
            {
                _replacementDictionary = replacementDictionary;
            }
            
            protected override Expression VisitSubQuery(SubQueryExpression expression)
            {
                expression.QueryModel.TransformExpressions(Visit);

                return expression;
            }
            
            protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression expression)
            {
                IQuerySource replacement;

                if (_replacementDictionary.TryGetValue(expression.ReferencedQuerySource, out replacement))
                {
                    return new QuerySourceReferenceExpression(replacement);
                }

                return expression;
            }
        }
        
        private class QsreWithNavigationFindingExpressionVisitor : ExpressionVisitorBase
        {
            private readonly QuerySourceReferenceExpression _searchedQsre;
            private readonly INavigation _navigation;
            private bool _navigationFound;
            
            public QsreWithNavigationFindingExpressionVisitor(QuerySourceReferenceExpression searchedQsre, INavigation navigation)
            {
                _searchedQsre = searchedQsre;
                _navigation = navigation;
                _navigationFound = false;
                SearchedQsreFound = false;
            }
            
            public bool SearchedQsreFound { get; private set; }
            
            protected override Expression VisitMember(MemberExpression node)
            {
                if (!_navigationFound && node.Member.Name == _navigation.Name)
                {
                    _navigationFound = true;

                    return base.VisitMember(node);
                }

                _navigationFound = false;

                return node;
            }
            
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (EntityQueryModelVisitor.IsPropertyMethod(node.Method)
                    && !_navigationFound
                    && (string)((ConstantExpression)node.Arguments[1]).Value == _navigation.Name)
                {
                    _navigationFound = true;

                    return base.VisitMethodCall(node);
                }

                _navigationFound = false;

                return node;
            }
            
            protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression expression)
            {
                if (_navigationFound && expression.ReferencedQuerySource == _searchedQsre.ReferencedQuerySource)
                {
                    SearchedQsreFound = true;
                }
                else
                {
                    _navigationFound = false;
                }

                return expression;
            }
        }
    }
}
