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
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Internal;
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
    public class NavigationRewritingExpressionVisitor : RelinqExpressionVisitor
    {
        private readonly NavigationJoins _navigationJoins = new NavigationJoins();
        private readonly NavigationRewritingQueryModelVisitor _navigationRewritingQueryModelVisitor;
        private bool _insideInnerKeySelector;
        private bool _insideOrderBy;
        private bool _insideMaterializeCollectionNavigation;

        private class NavigationJoins : IEnumerable<NavigationJoin>
        {
            private readonly Dictionary<NavigationJoin, int> _navigationJoins = new Dictionary<NavigationJoin, int>();

            public void Add(NavigationJoin navigationJoin)
            {
                _navigationJoins.TryGetValue(navigationJoin, out var count);
                _navigationJoins[navigationJoin] = ++count;
            }

            public bool Remove(NavigationJoin navigationJoin)
            {
                if (_navigationJoins.TryGetValue(navigationJoin, out var count))
                {
                    if (count > 1)
                    {
                        _navigationJoins[navigationJoin] = --count;
                    }
                    else
                    {
                        _navigationJoins.Remove(navigationJoin);
                    }

                    return true;
                }

                return false;
            }

            public IEnumerator<NavigationJoin> GetEnumerator() => _navigationJoins.Keys.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => _navigationJoins.Keys.GetEnumerator();
        }

        private class NavigationJoin
        {
            public static void RemoveNavigationJoin(
                NavigationJoins navigationJoins, NavigationJoin navigationJoin)
            {
                if (!navigationJoins.Remove(navigationJoin))
                {
                    foreach (var nj in navigationJoins)
                    {
                        nj.Children.Remove(navigationJoin);
                    }
                }
            }

            public NavigationJoin(
                IQuerySource querySource,
                INavigation navigation,
                JoinClause joinClause,
                IEnumerable<IBodyClause> additionalBodyClauses,
                bool dependentToPrincipal,
                QuerySourceReferenceExpression querySourceReferenceExpression)
                : this(
                    querySource,
                    navigation,
                    joinClause,
                    null,
                    additionalBodyClauses,
                    dependentToPrincipal,
                    querySourceReferenceExpression)
            {
            }

            public NavigationJoin(
                IQuerySource querySource,
                INavigation navigation,
                GroupJoinClause groupJoinClause,
                IEnumerable<IBodyClause> additionalBodyClauses,
                bool dependentToPrincipal,
                QuerySourceReferenceExpression querySourceReferenceExpression)
                : this(
                    querySource,
                    navigation,
                    null,
                    groupJoinClause,
                    additionalBodyClauses,
                    dependentToPrincipal,
                    querySourceReferenceExpression)
            {
            }

            private NavigationJoin(
                IQuerySource querySource,
                INavigation navigation,
                JoinClause joinClause,
                GroupJoinClause groupJoinClause,
                IEnumerable<IBodyClause> additionalBodyClauses,
                bool dependentToPrincipal,
                QuerySourceReferenceExpression querySourceReferenceExpression)
            {
                QuerySource = querySource;
                Navigation = navigation;
                JoinClause = joinClause;
                GroupJoinClause = groupJoinClause;
                AdditionalBodyClauses = additionalBodyClauses;
                DependentToPrincipal = dependentToPrincipal;
                QuerySourceReferenceExpression = querySourceReferenceExpression;
            }

            public IQuerySource QuerySource { get; }
            public INavigation Navigation { get; }
            public JoinClause JoinClause { get; }
            public GroupJoinClause GroupJoinClause { get; }
            public bool DependentToPrincipal { get; }
            public QuerySourceReferenceExpression QuerySourceReferenceExpression { get; }
            public readonly NavigationJoins Children = new NavigationJoins();

            private IEnumerable<IBodyClause> AdditionalBodyClauses { get; }

            private bool IsInserted { get; set; }

            public IEnumerable<NavigationJoin> Iterate()
            {
                yield return this;

                foreach (var navigationJoin in Children.SelectMany(nj => nj.Iterate()))
                {
                    yield return navigationJoin;
                }
            }

            public void Insert(QueryModel queryModel)
            {
                var insertionIndex = 0;

                if (QuerySource is IBodyClause bodyClause)
                {
                    insertionIndex = queryModel.BodyClauses.IndexOf(bodyClause) + 1;
                }

                if (queryModel.MainFromClause == QuerySource
                    || insertionIndex > 0)
                {
                    foreach (var nj in Iterate())
                    {
                        nj.Insert(queryModel, ref insertionIndex);
                    }
                }
            }

            private void Insert(QueryModel queryModel, ref int insertionIndex)
            {
                if (IsInserted)
                {
                    return;
                }

                queryModel.BodyClauses.Insert(insertionIndex++, JoinClause ?? (IBodyClause)GroupJoinClause);

                foreach (var additionalBodyClause in AdditionalBodyClauses)
                {
                    queryModel.BodyClauses.Insert(insertionIndex++, additionalBodyClause);
                }

                IsInserted = true;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public NavigationRewritingExpressionVisitor([NotNull] EntityQueryModelVisitor queryModelVisitor)
            : this(queryModelVisitor, navigationExpansionSubquery: false)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public NavigationRewritingExpressionVisitor(
            [NotNull] EntityQueryModelVisitor queryModelVisitor,
            bool navigationExpansionSubquery)
        {
            Check.NotNull(queryModelVisitor, nameof(queryModelVisitor));

            QueryModelVisitor = queryModelVisitor;
            _navigationRewritingQueryModelVisitor = new NavigationRewritingQueryModelVisitor(this, queryModelVisitor, navigationExpansionSubquery);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual EntityQueryModelVisitor QueryModelVisitor { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual QueryModel QueryModel { get; set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual QueryModel ParentQueryModel { get; set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual bool ShouldRewrite(INavigation navigation) => true;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void InjectSubqueryToCollectionsInProjection([NotNull] QueryModel queryModel)
        {
            var visitor = new ProjectionSubqueryInjectingQueryModelVisitor(QueryModelVisitor);
            visitor.VisitQueryModel(queryModel);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Rewrite([NotNull] QueryModel queryModel, [CanBeNull] QueryModel parentQueryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            QueryModel = queryModel;
            ParentQueryModel = parentQueryModel;

            _navigationRewritingQueryModelVisitor.VisitQueryModel(queryModel);

            foreach (var navigationJoin in _navigationJoins)
            {
                navigationJoin.Insert(queryModel);
            }

            if (parentQueryModel != null)
            {
                QueryModel = parentQueryModel;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitUnary(UnaryExpression node)
        {
            var newOperand = Visit(node.Operand);

            return node.NodeType == ExpressionType.Convert && newOperand.Type == node.Type
                ? newOperand
                : node.Update(newOperand);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            var oldInsideInnerKeySelector = _insideInnerKeySelector;
            _insideInnerKeySelector = false;

            Rewrite(expression.QueryModel, QueryModel);

            _insideInnerKeySelector = oldInsideInnerKeySelector;

            return expression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            var newLeft = Visit(node.Left);
            var newRight = Visit(node.Right);

            if (newLeft == node.Left
                && newRight == node.Right)
            {
                return node;
            }

            var leftNavigationJoin
                = _navigationJoins
                    .SelectMany(nj => nj.Iterate())
                    .FirstOrDefault(nj => ReferenceEquals(nj.QuerySourceReferenceExpression, newLeft));

            var rightNavigationJoin
                = _navigationJoins
                    .SelectMany(nj => nj.Iterate())
                    .FirstOrDefault(nj => ReferenceEquals(nj.QuerySourceReferenceExpression, newRight));

            var leftJoin = leftNavigationJoin?.JoinClause ?? leftNavigationJoin?.GroupJoinClause?.JoinClause;
            var rightJoin = rightNavigationJoin?.JoinClause ?? rightNavigationJoin?.GroupJoinClause?.JoinClause;

            if (leftNavigationJoin?.Navigation.GetTargetType().IsOwned() == false)
            {
                if (newRight.IsNullConstantExpression())
                {
                    if (leftNavigationJoin.DependentToPrincipal)
                    {
                        newLeft = leftJoin?.OuterKeySelector;

                        NavigationJoin.RemoveNavigationJoin(_navigationJoins, leftNavigationJoin);

                        if (newLeft != null
                            && IsCompositeKey(newLeft.Type))
                        {
                            newRight = CreateNullCompositeKey(newLeft);
                        }
                    }
                }
                else
                {
                    newLeft = leftJoin?.InnerKeySelector;
                }
            }

            if (rightNavigationJoin?.Navigation.GetTargetType().IsOwned() == false)
            {
                if (newLeft.IsNullConstantExpression())
                {
                    if (rightNavigationJoin.DependentToPrincipal)
                    {
                        newRight = rightJoin?.OuterKeySelector;

                        NavigationJoin.RemoveNavigationJoin(_navigationJoins, rightNavigationJoin);

                        if (newRight != null
                            && IsCompositeKey(newRight.Type))
                        {
                            newLeft = CreateNullCompositeKey(newRight);
                        }
                    }
                }
                else
                {
                    newRight = rightJoin?.InnerKeySelector;
                }
            }

            if (node.NodeType != ExpressionType.ArrayIndex
                && node.NodeType != ExpressionType.Coalesce
                && newLeft != null
                && newRight != null
                && newLeft.Type != newRight.Type)
            {
                if (newLeft.Type.IsNullableType()
                    && !newRight.Type.IsNullableType())
                {
                    newRight = Expression.Convert(newRight, newLeft.Type);
                }
                else if (!newLeft.Type.IsNullableType()
                         && newRight.Type.IsNullableType())
                {
                    newLeft = Expression.Convert(newLeft, newRight.Type);
                }
            }

            return Expression.MakeBinary(node.NodeType, newLeft, newRight, node.IsLiftedToNull, node.Method);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        private static NewExpression CreateNullCompositeKey(Expression otherExpression)
            => Expression.New(
                AnonymousObject.AnonymousObjectCtor,
                Expression.NewArrayInit(
                    typeof(object),
                    Enumerable.Repeat(
                        Expression.Constant(null),
                        ((NewArrayExpression)((NewExpression)otherExpression).Arguments.Single()).Expressions.Count)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMember(MemberExpression node)
        {
            Check.NotNull(node, nameof(node));

            var result = QueryModelVisitor.BindNavigationPathPropertyExpression(
                node,
                (ps, qs) =>
                {
                    return qs != null
                        ? RewriteNavigationProperties(
                            ps,
                            qs,
                            node,
                            node.Expression,
                            node.Member.Name,
                            node.Type,
                            e => e.MakeMemberAccess(node.Member),
                            e => new NullConditionalExpression(e, e.MakeMemberAccess(node.Member)))
                        : null;
                });

            if (result != null)
            {
                return result;
            }

            var newExpression = Visit(node.Expression);

            var newMemberExpression = newExpression != node.Expression
                ? newExpression.MakeMemberAccess(node.Member)
                : node;

            result = NeedsNullCompensation(newExpression)
                ? (Expression)new NullConditionalExpression(
                    newExpression,
                    newMemberExpression)
                : newMemberExpression;

            return result.Type == typeof(bool?) && node.Type == typeof(bool)
                ? Expression.Equal(result, Expression.Constant(true, typeof(bool?)))
                : result;
        }

        private readonly Dictionary<QuerySourceReferenceExpression, bool> _nullCompensationNecessityMap
            = new Dictionary<QuerySourceReferenceExpression, bool>();

        private bool NeedsNullCompensation(Expression expression)
        {
            if (expression is QuerySourceReferenceExpression qsre)
            {
                if (_nullCompensationNecessityMap.TryGetValue(qsre, out var result))
                {
                    return result;
                }

                var subQuery = (qsre.ReferencedQuerySource as FromClauseBase)?.FromExpression as SubQueryExpression
                               ?? (qsre.ReferencedQuerySource as JoinClause)?.InnerSequence as SubQueryExpression;

                // if qsre is pointing to a subquery, look for DefaulIfEmpty result operators inside
                // if such operator is found then we need to add null-compensation logic
                // unless the query model has a GroupBy operator - qsre coming from groupby can never be null
                if (subQuery != null
                    && !(subQuery.QueryModel.ResultOperators.LastOrDefault() is GroupResultOperator))
                {
                    var containsDefaultIfEmptyChecker = new ContainsDefaultIfEmptyCheckingVisitor();
                    containsDefaultIfEmptyChecker.VisitQueryModel(subQuery.QueryModel);
                    if (!containsDefaultIfEmptyChecker.ContainsDefaultIfEmpty)
                    {
                        subQuery.QueryModel.TransformExpressions(
                            e => new TransformingQueryModelExpressionVisitor<ContainsDefaultIfEmptyCheckingVisitor>(containsDefaultIfEmptyChecker).Visit(e));
                    }

                    _nullCompensationNecessityMap[qsre] = containsDefaultIfEmptyChecker.ContainsDefaultIfEmpty;

                    return containsDefaultIfEmptyChecker.ContainsDefaultIfEmpty;
                }

                _nullCompensationNecessityMap[qsre] = false;
            }

            return false;
        }

        private class ContainsDefaultIfEmptyCheckingVisitor : QueryModelVisitorBase
        {
            public bool ContainsDefaultIfEmpty { get; private set; }

            public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
            {
                if (resultOperator is DefaultIfEmptyResultOperator)
                {
                    ContainsDefaultIfEmpty = true;
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            var newExpression = CompensateForNullabilityDifference(
                Visit(node.Expression),
                node.Expression.Type);

            return node.Update(newExpression);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override ElementInit VisitElementInit(ElementInit node)
        {
            var originalArgumentTypes = node.Arguments.Select(a => a.Type).ToList();
            var newArguments = VisitAndConvert(node.Arguments, nameof(VisitElementInit)).ToList();

            for (var i = 0; i < newArguments.Count; i++)
            {
                newArguments[i] = CompensateForNullabilityDifference(newArguments[i], originalArgumentTypes[i]);
            }

            return node.Update(newArguments);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            var originalExpressionTypes = node.Expressions.Select(e => e.Type).ToList();
            var newExpressions = VisitAndConvert(node.Expressions, nameof(VisitNewArray)).ToList();

            for (var i = 0; i < newExpressions.Count; i++)
            {
                newExpressions[i] = CompensateForNullabilityDifference(newExpressions[i], originalExpressionTypes[i]);
            }

            return node.Update(newExpressions);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Check.NotNull(node, nameof(node));

            if (node.Method.IsEFPropertyMethod())
            {
                var result = QueryModelVisitor.BindNavigationPathPropertyExpression(
                    node,
                    (ps, qs) =>
                    {
                        return qs != null
                            ? RewriteNavigationProperties(
                                ps,
                                qs,
                                node,
                                node.Arguments[0],
                                (string)((ConstantExpression)node.Arguments[1]).Value,
                                node.Type,
                                e => node.Arguments[0].Type != e.Type
                                    ? Expression.Call(node.Method, Expression.Convert(e, node.Arguments[0].Type), node.Arguments[1])
                                    : Expression.Call(node.Method, e, node.Arguments[1]),
                                e => node.Arguments[0].Type != e.Type
                                    ? new NullConditionalExpression(
                                        Expression.Convert(e, node.Arguments[0].Type),
                                        Expression.Call(node.Method, Expression.Convert(e, node.Arguments[0].Type), node.Arguments[1]))
                                    : new NullConditionalExpression(e, Expression.Call(node.Method, e, node.Arguments[1])))
                            : null;
                    });

                if (result != null)
                {
                    return result;
                }

                var propertyArguments = VisitAndConvert(node.Arguments, nameof(VisitMethodCall)).ToList();

                var newPropertyExpression = propertyArguments[0] != node.Arguments[0] || propertyArguments[1] != node.Arguments[1]
                    ? Expression.Call(node.Method, propertyArguments[0], node.Arguments[1])
                    : node;

                result = NeedsNullCompensation(propertyArguments[0])
                    ? (Expression)new NullConditionalExpression(propertyArguments[0], newPropertyExpression)
                    : newPropertyExpression;

                return result.Type == typeof(bool?) && node.Type == typeof(bool)
                    ? Expression.Equal(result, Expression.Constant(true, typeof(bool?)))
                    : result;
            }

            var insideMaterializeCollectionNavigation = _insideMaterializeCollectionNavigation;
            if (node.Method.MethodIsClosedFormOf(CollectionNavigationSubqueryInjector.MaterializeCollectionNavigationMethodInfo))
            {
                _insideMaterializeCollectionNavigation = true;
            }

            var newObject = Visit(node.Object);
            var newArguments = VisitAndConvert(node.Arguments, nameof(VisitMethodCall)).ToList();

            for (var i = 0; i < newArguments.Count; i++)
            {
                if (newArguments[i].Type != node.Arguments[i].Type)
                {
                    if (newArguments[i] is NullConditionalExpression nullConditionalArgument)
                    {
                        newArguments[i] = nullConditionalArgument.AccessOperation;
                    }

                    if (newArguments[i].Type != node.Arguments[i].Type)
                    {
                        newArguments[i] = Expression.Convert(newArguments[i], node.Arguments[i].Type);
                    }
                }
            }

            if (newObject != node.Object
                && newObject is NullConditionalExpression nullConditionalExpression)
            {
                var newMethodCallExpression = node.Update(nullConditionalExpression.AccessOperation, newArguments);

                return new NullConditionalExpression(newObject, newMethodCallExpression);
            }

            var newExpression = node.Update(newObject, newArguments);

            if (node.Method.MethodIsClosedFormOf(CollectionNavigationSubqueryInjector.MaterializeCollectionNavigationMethodInfo))
            {
                _insideMaterializeCollectionNavigation = insideMaterializeCollectionNavigation;
            }

            return newExpression;
        }

        private Expression RewriteNavigationProperties(
            IReadOnlyList<IPropertyBase> properties,
            IQuerySource querySource,
            Expression expression,
            Expression declaringExpression,
            string propertyName,
            Type propertyType,
            Func<Expression, Expression> propertyCreator,
            Func<Expression, Expression> conditionalAccessPropertyCreator)
        {
            var navigations = properties.OfType<INavigation>().ToList();

            if (navigations.Count > 0)
            {
                var outerQuerySourceReferenceExpression = new QuerySourceReferenceExpression(querySource);

                var additionalFromClauseBeingProcessed = _navigationRewritingQueryModelVisitor.AdditionalFromClauseBeingProcessed;
                if (additionalFromClauseBeingProcessed != null
                    && navigations.Last().IsCollection()
                    && !_insideMaterializeCollectionNavigation)
                {
                    if (additionalFromClauseBeingProcessed.FromExpression is SubQueryExpression fromSubqueryExpression)
                    {
                        if (fromSubqueryExpression.QueryModel.SelectClause.Selector is QuerySourceReferenceExpression)
                        {
                            return RewriteSelectManyInsideSubqueryIntoJoins(
                                fromSubqueryExpression,
                                outerQuerySourceReferenceExpression,
                                navigations,
                                additionalFromClauseBeingProcessed);
                        }
                    }
                    else
                    {
                        return RewriteSelectManyNavigationsIntoJoins(
                            outerQuerySourceReferenceExpression,
                            navigations,
                            additionalFromClauseBeingProcessed);
                    }
                }

                if (navigations.Count == 1
                    && navigations[0].IsDependentToPrincipal())
                {
                    var foreignKeyMemberAccess = TryCreateForeignKeyMemberAccess(propertyName, declaringExpression, navigations[0]);
                    if (foreignKeyMemberAccess != null)
                    {
                        return foreignKeyMemberAccess;
                    }
                }

                if (_insideInnerKeySelector && !_insideMaterializeCollectionNavigation)
                {
                    return CreateSubqueryForNavigations(
                        outerQuerySourceReferenceExpression,
                        properties,
                        propertyCreator);
                }

                var navigationResultExpression = RewriteNavigationsIntoJoins(
                    outerQuerySourceReferenceExpression,
                    navigations,
                    properties.Count == navigations.Count ? null : propertyType,
                    propertyCreator,
                    conditionalAccessPropertyCreator);

                if (navigationResultExpression is QuerySourceReferenceExpression resultQsre)
                {
                    foreach (var includeResultOperator in QueryModelVisitor.QueryCompilationContext.QueryAnnotations
                        .OfType<IncludeResultOperator>()
                        .Where(o => ExpressionEqualityComparer.Instance.Equals(o.PathFromQuerySource, expression)))
                    {
                        includeResultOperator.PathFromQuerySource = resultQsre;
                        includeResultOperator.QuerySource = resultQsre.ReferencedQuerySource;
                    }
                }

                return navigationResultExpression;
            }

            return default;
        }

        private class QsreWithNavigationFindingExpressionVisitor : ExpressionVisitorBase
        {
            private readonly QuerySourceReferenceExpression _searchedQsre;
            private readonly INavigation _navigation;
            private bool _navigationFound;

            public QsreWithNavigationFindingExpressionVisitor([NotNull] QuerySourceReferenceExpression searchedQsre, [NotNull] INavigation navigation)
            {
                _searchedQsre = searchedQsre;
                _navigation = navigation;
                _navigationFound = false;
                SearchedQsreFound = false;
            }

            public bool SearchedQsreFound { get; private set; }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (!_navigationFound
                    && node.Member.Name == _navigation.Name)
                {
                    _navigationFound = true;

                    return base.VisitMember(node);
                }

                _navigationFound = false;

                return node;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.IsEFPropertyMethod()
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

        private Expression TryCreateForeignKeyMemberAccess(string propertyName, Expression declaringExpression, INavigation navigation)
        {
            var canPerformOptimization = true;
            if (_insideOrderBy)
            {
                var qsre = (declaringExpression as MemberExpression)?.Expression as QuerySourceReferenceExpression;
                if (qsre == null)
                {
                    if (declaringExpression is MethodCallExpression methodCallExpression
                        && methodCallExpression.Method.IsEFPropertyMethod())
                    {
                        qsre = methodCallExpression.Arguments[0] as QuerySourceReferenceExpression;
                    }
                }

                if (qsre != null)
                {
                    var qsreFindingVisitor = new QsreWithNavigationFindingExpressionVisitor(qsre, navigation);
                    qsreFindingVisitor.Visit(QueryModel.SelectClause.Selector);

                    if (qsreFindingVisitor.SearchedQsreFound)
                    {
                        canPerformOptimization = false;
                    }
                }
            }

            if (canPerformOptimization)
            {
                var foreignKeyMemberAccess = CreateForeignKeyMemberAccess(propertyName, declaringExpression, navigation);
                if (foreignKeyMemberAccess != null)
                {
                    return foreignKeyMemberAccess;
                }
            }

            return null;
        }

        private static Expression CreateForeignKeyMemberAccess(string propertyName, Expression declaringExpression, INavigation navigation)
        {
            var principalKey = navigation.ForeignKey.PrincipalKey;
            if (principalKey.Properties.Count == 1)
            {
                Debug.Assert(navigation.ForeignKey.Properties.Count == 1);

                var principalKeyProperty = principalKey.Properties[0];
                if (principalKeyProperty.Name == propertyName
                    && principalKeyProperty.ClrType == navigation.ForeignKey.Properties[0].ClrType.UnwrapNullableType())
                {
                    var parentDeclaringExpression
                        = declaringExpression is MethodCallExpression declaringMethodCallExpression
                          && declaringMethodCallExpression.Method.IsEFPropertyMethod()
                            ? declaringMethodCallExpression.Arguments[0]
                            : (declaringExpression as MemberExpression)?.Expression;

                    if (parentDeclaringExpression != null)
                    {
                        var foreignKeyPropertyExpression = CreateKeyAccessExpression(parentDeclaringExpression, navigation.ForeignKey.Properties);

                        return foreignKeyPropertyExpression;
                    }
                }
            }

            return null;
        }

        private Expression CreateSubqueryForNavigations(
            Expression outerQuerySourceReferenceExpression,
            IReadOnlyList<IPropertyBase> properties,
            Func<Expression, Expression> propertyCreator)
        {
            var navigations = properties.OfType<INavigation>().ToList();
            var firstNavigation = navigations.First();
            var targetEntityType = firstNavigation.GetTargetType();

            var mainFromClause
                = new MainFromClause(
                    "subQuery",
                    targetEntityType.ClrType,
                    NullAsyncQueryProvider.Instance.CreateEntityQueryableExpression(targetEntityType.ClrType));

            QueryModelVisitor.QueryCompilationContext.AddOrUpdateMapping(mainFromClause, targetEntityType);
            var querySourceReference = new QuerySourceReferenceExpression(mainFromClause);
            var subQueryModel = new QueryModel(mainFromClause, new SelectClause(querySourceReference));

            var leftKeyAccess = CreateKeyAccessExpression(
                querySourceReference,
                firstNavigation.IsDependentToPrincipal()
                    ? firstNavigation.ForeignKey.PrincipalKey.Properties
                    : firstNavigation.ForeignKey.Properties);

            var rightKeyAccess = CreateKeyAccessExpression(
                outerQuerySourceReferenceExpression,
                firstNavigation.IsDependentToPrincipal()
                    ? firstNavigation.ForeignKey.Properties
                    : firstNavigation.ForeignKey.PrincipalKey.Properties);

            subQueryModel.BodyClauses.Add(
                new WhereClause(
                    CreateKeyComparisonExpressionForCollectionNavigationSubquery(
                        leftKeyAccess,
                        rightKeyAccess,
                        querySourceReference)));

            subQueryModel.ResultOperators.Add(new FirstResultOperator(returnDefaultWhenEmpty: true));

            var selectClauseExpression = (Expression)querySourceReference;

            selectClauseExpression
                = navigations
                    .Skip(1)
                    .Aggregate(
                        selectClauseExpression,
                        (current, navigation) => Expression.Property(current, navigation.Name));

            subQueryModel.SelectClause = new SelectClause(selectClauseExpression);

            if (properties.Count > navigations.Count)
            {
                subQueryModel.SelectClause = new SelectClause(propertyCreator(subQueryModel.SelectClause.Selector));
            }

            if (navigations.Count > 1)
            {
                var subQueryVisitor = CreateVisitorForSubQuery(navigationExpansionSubquery: true);
                subQueryVisitor.Rewrite(subQueryModel, parentQueryModel: null);
            }

            return new SubQueryExpression(subQueryModel);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual NavigationRewritingExpressionVisitor CreateVisitorForSubQuery(bool navigationExpansionSubquery)
            => new NavigationRewritingExpressionVisitor(
                QueryModelVisitor,
                navigationExpansionSubquery);

        private static Expression CreateKeyComparisonExpressionForCollectionNavigationSubquery(
            Expression leftExpression,
            Expression rightExpression,
            Expression leftQsre)
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

            var outerNullProtection
                = Expression.NotEqual(
                    leftQsre,
                    Expression.Constant(null, leftQsre.Type));

            return new NullSafeEqualExpression(outerNullProtection, Expression.Equal(leftExpression, rightExpression));
        }

        private Expression RewriteNavigationsIntoJoins(
            QuerySourceReferenceExpression outerQuerySourceReferenceExpression,
            IEnumerable<INavigation> navigations,
            Type propertyType,
            Func<Expression, Expression> propertyCreator,
            Func<Expression, Expression> conditionalAccessPropertyCreator)
        {
            var sourceQsre = outerQuerySourceReferenceExpression;
            Expression sourceExpression = outerQuerySourceReferenceExpression;
            var navigationJoins = _navigationJoins;

            var optionalNavigationInChain
                = NeedsNullCompensation(outerQuerySourceReferenceExpression);

            foreach (var navigation in navigations)
            {
                var addNullCheckToOuterKeySelector = optionalNavigationInChain;

                if (!navigation.ForeignKey.IsRequired
                    || !navigation.IsDependentToPrincipal()
                    || (navigation.DeclaringEntityType.ClrType != sourceExpression.Type
                        && navigation.DeclaringEntityType.GetAllBaseTypes().Any(t => t.ClrType == sourceExpression.Type)))
                {
                    optionalNavigationInChain = true;
                }

                if (!ShouldRewrite(navigation))
                {
                    if(sourceExpression.Type != navigation.DeclaringEntityType.ClrType)
                    {
                        sourceExpression = Expression.Condition(
                            Expression.TypeIs(sourceExpression, navigation.DeclaringEntityType.ClrType),
                            Expression.Convert(sourceExpression, navigation.DeclaringEntityType.ClrType),
                            Expression.Constant(null, navigation.DeclaringEntityType.ClrType));
                    }

                    sourceExpression = new NullConditionalExpression(sourceExpression, Expression.Property(sourceExpression, navigation.PropertyInfo));

                    continue;
                }

                var targetEntityType = navigation.GetTargetType();

                if (navigation.IsCollection())
                {
                    QueryModel.MainFromClause.FromExpression
                        = NullAsyncQueryProvider.Instance.CreateEntityQueryableExpression(targetEntityType.ClrType);

                    var innerQuerySourceReferenceExpression
                        = new QuerySourceReferenceExpression(QueryModel.MainFromClause);
                    QueryModelVisitor.QueryCompilationContext.AddOrUpdateMapping(QueryModel.MainFromClause, targetEntityType);

                    var leftKeyAccess = CreateKeyAccessExpression(
                        sourceExpression,
                        navigation.IsDependentToPrincipal()
                            ? navigation.ForeignKey.Properties
                            : navigation.ForeignKey.PrincipalKey.Properties);

                    var rightKeyAccess = CreateKeyAccessExpression(
                        innerQuerySourceReferenceExpression,
                        navigation.IsDependentToPrincipal()
                            ? navigation.ForeignKey.PrincipalKey.Properties
                            : navigation.ForeignKey.Properties);

                    QueryModel.BodyClauses.Add(
                        new WhereClause(
                            CreateKeyComparisonExpressionForCollectionNavigationSubquery(
                                leftKeyAccess,
                                rightKeyAccess,
                                sourceExpression)));

                    return QueryModel.MainFromClause.FromExpression;
                }

                var navigationJoin
                    = navigationJoins
                        .FirstOrDefault(nj =>
                            nj.QuerySource == (sourceExpression as QuerySourceReferenceExpression ?? sourceQsre).ReferencedQuerySource
                            && nj.Navigation == navigation);

                if (navigationJoin == null)
                {
                    var joinClause = BuildJoinFromNavigation(
                        sourceExpression,
                        navigation,
                        targetEntityType,
                        addNullCheckToOuterKeySelector,
                        out var innerQuerySourceReferenceExpression);

                    if (optionalNavigationInChain)
                    {
                        RewriteNavigationIntoGroupJoin(
                            joinClause,
                            navigation,
                            targetEntityType,
                            sourceExpression as QuerySourceReferenceExpression ?? sourceQsre,
                            null,
                            new List<IBodyClause>(),
                            new List<ResultOperatorBase>
                            {
                                new DefaultIfEmptyResultOperator(null)
                            },
                            out navigationJoin);
                    }
                    else
                    {
                        navigationJoin
                            = new NavigationJoin(
                                (sourceExpression as QuerySourceReferenceExpression ?? sourceQsre).ReferencedQuerySource,
                                navigation,
                                joinClause,
                                new List<IBodyClause>(),
                                navigation.IsDependentToPrincipal(),
                                innerQuerySourceReferenceExpression);
                    }
                }

                navigationJoins.Add(navigationJoin);

                sourceExpression = navigationJoin.QuerySourceReferenceExpression;
                sourceQsre = navigationJoin.QuerySourceReferenceExpression;
                navigationJoins = navigationJoin.Children;
            }

            return propertyType == null
                ? sourceExpression
                : optionalNavigationInChain
                ? conditionalAccessPropertyCreator(sourceExpression)
                : propertyCreator(sourceExpression);
        }

        private void RewriteNavigationIntoGroupJoin(
            JoinClause joinClause,
            INavigation navigation,
            IEntityType targetEntityType,
            QuerySourceReferenceExpression querySourceReferenceExpression,
            MainFromClause groupJoinSubqueryMainFromClause,
            ICollection<IBodyClause> groupJoinSubqueryBodyClauses,
            ICollection<ResultOperatorBase> groupJoinSubqueryResultOperators,
            out NavigationJoin navigationJoin)
        {
            var groupJoinClause
                = new GroupJoinClause(
                    joinClause.ItemName + "_group",
                    typeof(IEnumerable<>).MakeGenericType(targetEntityType.ClrType),
                    joinClause);

            var groupReferenceExpression = new QuerySourceReferenceExpression(groupJoinClause);

            var groupJoinSubqueryModelMainFromClause = new MainFromClause(joinClause.ItemName + "_groupItem", joinClause.ItemType, groupReferenceExpression);
            var newQuerySourceReferenceExpression = new QuerySourceReferenceExpression(groupJoinSubqueryModelMainFromClause);
            QueryModelVisitor.QueryCompilationContext.AddOrUpdateMapping(groupJoinSubqueryModelMainFromClause, targetEntityType);

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

            if (groupJoinSubqueryMainFromClause != null
                && (groupJoinSubqueryBodyClauses.Count > 0 || groupJoinSubqueryResultOperators.Count > 0))
            {
                var querySourceMapping = new QuerySourceMapping();
                querySourceMapping.AddMapping(groupJoinSubqueryMainFromClause, newQuerySourceReferenceExpression);

                groupJoinSubqueryModel.TransformExpressions(
                    e => ReferenceReplacingExpressionVisitor
                        .ReplaceClauseReferences(e, querySourceMapping, throwOnUnmappedReferences: false));
            }

            var defaultIfEmptySubquery = new SubQueryExpression(groupJoinSubqueryModel);
            var defaultIfEmptyAdditionalFromClause = new AdditionalFromClause(joinClause.ItemName, joinClause.ItemType, defaultIfEmptySubquery);

            navigationJoin = new NavigationJoin(
                querySourceReferenceExpression.ReferencedQuerySource,
                navigation,
                groupJoinClause,
                new[] { defaultIfEmptyAdditionalFromClause },
                navigation.IsDependentToPrincipal(),
                new QuerySourceReferenceExpression(defaultIfEmptyAdditionalFromClause));

            QueryModelVisitor.QueryCompilationContext.AddOrUpdateMapping(defaultIfEmptyAdditionalFromClause, targetEntityType);
        }

        private Expression RewriteSelectManyNavigationsIntoJoins(
            QuerySourceReferenceExpression outerQuerySourceReferenceExpression,
            IEnumerable<INavigation> navigations,
            AdditionalFromClause additionalFromClauseBeingProcessed)
        {
            Expression sourceExpression = outerQuerySourceReferenceExpression;
            var querySourceReferenceExpression = outerQuerySourceReferenceExpression;
            var additionalJoinIndex = QueryModel.BodyClauses.IndexOf(additionalFromClauseBeingProcessed);
            var joinClauses = new List<JoinClause>();

            foreach (var navigation in navigations)
            {
                var targetEntityType = navigation.GetTargetType();

                if (!ShouldRewrite(navigation))
                {
                    sourceExpression = Expression.Property(sourceExpression, navigation.PropertyInfo);

                    continue;
                }

                var joinClause = BuildJoinFromNavigation(
                    sourceExpression,
                    navigation,
                    targetEntityType,
                    false,
                    out var innerQuerySourceReferenceExpression);

                joinClauses.Add(joinClause);

                querySourceReferenceExpression = innerQuerySourceReferenceExpression;
                sourceExpression = innerQuerySourceReferenceExpression;
            }

            if (ShouldRewrite(navigations.Last()))
            {
                QueryModel.BodyClauses.RemoveAt(additionalJoinIndex);
            }
            else
            {
                ((AdditionalFromClause)QueryModel.BodyClauses[additionalJoinIndex]).FromExpression = sourceExpression;
            }

            for (var i = 0; i < joinClauses.Count; i++)
            {
                QueryModel.BodyClauses.Insert(additionalJoinIndex + i, joinClauses[i]);
            }

            if (ShouldRewrite(navigations.Last()))
            {
                var querySourceMapping = new QuerySourceMapping();
                querySourceMapping.AddMapping(additionalFromClauseBeingProcessed, querySourceReferenceExpression);

                QueryModel.TransformExpressions(
                    e => ReferenceReplacingExpressionVisitor
                        .ReplaceClauseReferences(e, querySourceMapping, throwOnUnmappedReferences: false));

                AdjustQueryCompilationContextStateAfterSelectMany(
                    querySourceMapping,
                    additionalFromClauseBeingProcessed,
                    querySourceReferenceExpression.ReferencedQuerySource);
            }

            return ShouldRewrite(navigations.Last())
                ? querySourceReferenceExpression
                : sourceExpression;
        }

        private Expression RewriteSelectManyInsideSubqueryIntoJoins(
            SubQueryExpression fromSubqueryExpression,
            QuerySourceReferenceExpression outerQuerySourceReferenceExpression,
            ICollection<INavigation> navigations,
            AdditionalFromClause additionalFromClauseBeingProcessed)
        {
            var collectionNavigation = navigations.Last();
            var adddedJoinClauses = new List<IBodyClause>();

            foreach (var navigation in navigations)
            {
                var targetEntityType = navigation.GetTargetType();

                var joinClause = BuildJoinFromNavigation(
                    outerQuerySourceReferenceExpression,
                    navigation,
                    targetEntityType,
                    false,
                    out var innerQuerySourceReferenceExpression);

                if (navigation == collectionNavigation)
                {
                    RewriteNavigationIntoGroupJoin(
                        joinClause,
                        navigations.Last(),
                        targetEntityType,
                        outerQuerySourceReferenceExpression,
                        fromSubqueryExpression.QueryModel.MainFromClause,
                        fromSubqueryExpression.QueryModel.BodyClauses,
                        fromSubqueryExpression.QueryModel.ResultOperators,
                        out var navigationJoin);

                    _navigationJoins.Add(navigationJoin);

                    var additionalFromClauseIndex = ParentQueryModel.BodyClauses.IndexOf(additionalFromClauseBeingProcessed);
                    ParentQueryModel.BodyClauses.Remove(additionalFromClauseBeingProcessed);

                    var i = additionalFromClauseIndex;
                    foreach (var addedJoinClause in adddedJoinClauses)
                    {
                        ParentQueryModel.BodyClauses.Insert(i++, addedJoinClause);
                    }

                    var querySourceMapping = new QuerySourceMapping();
                    querySourceMapping.AddMapping(additionalFromClauseBeingProcessed, navigationJoin.QuerySourceReferenceExpression);

                    ParentQueryModel.TransformExpressions(
                        e => ReferenceReplacingExpressionVisitor
                            .ReplaceClauseReferences(e, querySourceMapping, throwOnUnmappedReferences: false));

                    AdjustQueryCompilationContextStateAfterSelectMany(
                        querySourceMapping,
                        additionalFromClauseBeingProcessed,
                        navigationJoin.QuerySourceReferenceExpression.ReferencedQuerySource);

                    return navigationJoin.QuerySourceReferenceExpression;
                }

                var defaultIfEmptyOperator = fromSubqueryExpression.QueryModel.ResultOperators.OfType<DefaultIfEmptyResultOperator>().FirstOrDefault();
                if (defaultIfEmptyOperator != null)
                {
                    RewriteNavigationIntoGroupJoin(
                        joinClause,
                        navigation,
                        targetEntityType,
                        outerQuerySourceReferenceExpression,
                        null,
                        new List<IBodyClause>(),
                        new List<ResultOperatorBase>
                        {
                            defaultIfEmptyOperator
                        },
                        out var navigationJoin);

                    _navigationJoins.Add(navigationJoin);
                    outerQuerySourceReferenceExpression = navigationJoin.QuerySourceReferenceExpression;
                }
                else
                {
                    adddedJoinClauses.Add(joinClause);
                    outerQuerySourceReferenceExpression = innerQuerySourceReferenceExpression;
                }
            }

            return outerQuerySourceReferenceExpression;
        }

        private void AdjustQueryCompilationContextStateAfterSelectMany(QuerySourceMapping querySourceMapping, IQuerySource querySourceBeingProcessed, IQuerySource resultQuerySource)
        {
            foreach (var includeResultOperator in QueryModelVisitor.QueryCompilationContext.QueryAnnotations.OfType<IncludeResultOperator>())
            {
                includeResultOperator.PathFromQuerySource
                    = ReferenceReplacingExpressionVisitor.ReplaceClauseReferences(
                        includeResultOperator.PathFromQuerySource,
                        querySourceMapping,
                        throwOnUnmappedReferences: false);

                if (includeResultOperator.QuerySource == querySourceBeingProcessed)
                {
                    includeResultOperator.QuerySource = resultQuerySource;
                }
            }

            if (QueryModelVisitor.QueryCompilationContext.CorrelatedSubqueryMetadataMap != null)
            {
                foreach (var mapping in QueryModelVisitor.QueryCompilationContext.CorrelatedSubqueryMetadataMap)
                {
                    if (mapping.Value.ParentQuerySource == querySourceBeingProcessed)
                    {
                        mapping.Value.ParentQuerySource = resultQuerySource;
                    }
                }
            }
        }

        private JoinClause BuildJoinFromNavigation(
            Expression sourceExpression,
            INavigation navigation,
            IEntityType targetEntityType,
            bool addNullCheckToOuterKeySelector,
            out QuerySourceReferenceExpression innerQuerySourceReferenceExpression)
        {
            var outerKeySelector =
                CreateKeyAccessExpression(
                    sourceExpression,
                    navigation.IsDependentToPrincipal()
                        ? navigation.ForeignKey.Properties
                        : navigation.ForeignKey.PrincipalKey.Properties,
                    addNullCheckToOuterKeySelector);

            var itemName = sourceExpression is QuerySourceReferenceExpression qsre && !qsre.ReferencedQuerySource.HasGeneratedItemName()
                ? qsre.ReferencedQuerySource.ItemName
                : navigation.DeclaringEntityType.ShortName()[0].ToString().ToLowerInvariant();

            var joinClause
                = new JoinClause(
                    $"{itemName}.{navigation.Name}",
                    targetEntityType.ClrType,
                    NullAsyncQueryProvider.Instance.CreateEntityQueryableExpression(targetEntityType.ClrType),
                    outerKeySelector,
                    Expression.Constant(null));

            innerQuerySourceReferenceExpression = new QuerySourceReferenceExpression(joinClause);
            QueryModelVisitor.QueryCompilationContext.AddOrUpdateMapping(joinClause, targetEntityType);

            var innerKeySelector
                = CreateKeyAccessExpression(
                    innerQuerySourceReferenceExpression,
                    navigation.IsDependentToPrincipal()
                        ? navigation.ForeignKey.PrincipalKey.Properties
                        : navigation.ForeignKey.Properties);

            if (innerKeySelector.Type != joinClause.OuterKeySelector.Type)
            {
                if (innerKeySelector.Type.IsNullableType())
                {
                    joinClause.OuterKeySelector
                        = Expression.Convert(
                            joinClause.OuterKeySelector,
                            innerKeySelector.Type);
                }
                else
                {
                    innerKeySelector
                        = Expression.Convert(
                            innerKeySelector,
                            joinClause.OuterKeySelector.Type);
                }
            }

            joinClause.InnerKeySelector = innerKeySelector;

            return joinClause;
        }

        private static Expression CreateKeyAccessExpression(
            Expression target, IReadOnlyList<IProperty> properties, bool addNullCheck = false)
            => properties.Count == 1
                ? CreatePropertyExpression(target, properties[0], addNullCheck)
                : Expression.New(
                    AnonymousObject.AnonymousObjectCtor,
                    Expression.NewArrayInit(
                        typeof(object),
                        properties
                            .Select(p => Expression.Convert(CreatePropertyExpression(target, p, addNullCheck), typeof(object)))
                            .Cast<Expression>()
                            .ToArray()));

        private static Expression CreatePropertyExpression(Expression target, IProperty property, bool addNullCheck)
        {
            var propertyExpression = target.CreateEFPropertyExpression(property, makeNullable: false);

            var propertyDeclaringType = property.DeclaringType.ClrType;
            if (propertyDeclaringType != target.Type
                && target.Type.GetTypeInfo().IsAssignableFrom(propertyDeclaringType.GetTypeInfo()))
            {
                if (!propertyExpression.Type.IsNullableType())
                {
                    propertyExpression = Expression.Convert(propertyExpression, propertyExpression.Type.MakeNullable());
                }

                return Expression.Condition(
                    Expression.TypeIs(target, propertyDeclaringType),
                    propertyExpression,
                    Expression.Constant(null, propertyExpression.Type));
            }

            return addNullCheck
                ? new NullConditionalExpression(target, propertyExpression)
                : propertyExpression;
        }

        private static bool IsCompositeKey([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            return type == typeof(AnonymousObject);
        }

        private static Expression CompensateForNullabilityDifference(Expression expression, Type originalType)
        {
            var newType = expression.Type;

            var needsTypeCompensation
                = originalType != newType
                  && !originalType.IsNullableType()
                  && newType.IsNullableType()
                  && originalType == newType.UnwrapNullableType();

            return needsTypeCompensation
                ? Expression.Convert(expression, originalType)
                : expression;
        }

        private class NavigationRewritingQueryModelVisitor : ExpressionTransformingQueryModelVisitor<NavigationRewritingExpressionVisitor>
        {
            private readonly CollectionNavigationSubqueryInjector _subqueryInjector;
            private readonly bool _navigationExpansionSubquery;
            private readonly QueryCompilationContext _queryCompilationContext;

            public AdditionalFromClause AdditionalFromClauseBeingProcessed { get; private set; }

            public NavigationRewritingQueryModelVisitor(
                NavigationRewritingExpressionVisitor transformingVisitor,
                EntityQueryModelVisitor queryModelVisitor,
                bool navigationExpansionSubquery)
                : base(transformingVisitor)
            {
                _subqueryInjector = new CollectionNavigationSubqueryInjector(queryModelVisitor, shouldInject: true);
                _navigationExpansionSubquery = navigationExpansionSubquery;
                _queryCompilationContext = queryModelVisitor.QueryCompilationContext;
            }

            public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
            {
                base.VisitMainFromClause(fromClause, queryModel);

                var queryCompilationContext = TransformingVisitor.QueryModelVisitor.QueryCompilationContext;
                if (queryCompilationContext.FindEntityType(fromClause) == null
                    && fromClause.FromExpression is SubQueryExpression subQuery)
                {
                    var entityType = MemberAccessBindingExpressionVisitor.GetEntityType(
                        subQuery.QueryModel.SelectClause.Selector, queryCompilationContext);

                    if (entityType != null)
                    {
                        queryCompilationContext.AddOrUpdateMapping(fromClause, entityType);
                    }
                }
            }

            public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
            {
                // ReSharper disable once PatternAlwaysOfType
                if (fromClause.TryGetFlattenedGroupJoinClause()?.JoinClause is JoinClause joinClause
                    // ReSharper disable once PatternAlwaysOfType
                    && _queryCompilationContext.FindEntityType(joinClause) is IEntityType entityType)
                {
                    _queryCompilationContext.AddOrUpdateMapping(fromClause, entityType);
                }

                var oldAdditionalFromClause = AdditionalFromClauseBeingProcessed;
                AdditionalFromClauseBeingProcessed = fromClause;
                fromClause.TransformExpressions(TransformingVisitor.Visit);
                AdditionalFromClauseBeingProcessed = oldAdditionalFromClause;
            }

            public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
            {
                base.VisitWhereClause(whereClause, queryModel, index);

                if (whereClause.Predicate.Type == typeof(bool?))
                {
                    whereClause.Predicate = Expression.Equal(whereClause.Predicate, Expression.Constant(true, typeof(bool?)));
                }
            }

            public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
            {
                var originalTypes = orderByClause.Orderings.Select(o => o.Expression.Type).ToList();

                var oldInsideOrderBy = TransformingVisitor._insideOrderBy;
                TransformingVisitor._insideOrderBy = true;

                base.VisitOrderByClause(orderByClause, queryModel, index);

                TransformingVisitor._insideOrderBy = oldInsideOrderBy;

                for (var i = 0; i < orderByClause.Orderings.Count; i++)
                {
                    orderByClause.Orderings[i].Expression = CompensateForNullabilityDifference(
                        orderByClause.Orderings[i].Expression,
                        originalTypes[i]);
                }
            }

            public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
                => VisitJoinClauseInternal(joinClause);

            public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, GroupJoinClause groupJoinClause)
                => VisitJoinClauseInternal(joinClause);

            private void VisitJoinClauseInternal(JoinClause joinClause)
            {
                joinClause.InnerSequence = TransformingVisitor.Visit(joinClause.InnerSequence);

                var queryCompilationContext = TransformingVisitor.QueryModelVisitor.QueryCompilationContext;
                if (queryCompilationContext.FindEntityType(joinClause) == null
                    && joinClause.InnerSequence is SubQueryExpression subQuery)
                {
                    var entityType = MemberAccessBindingExpressionVisitor.GetEntityType(
                        subQuery.QueryModel.SelectClause.Selector, queryCompilationContext);
                    if (entityType != null)
                    {
                        queryCompilationContext.AddOrUpdateMapping(joinClause, entityType);
                    }
                }

                joinClause.OuterKeySelector = TransformingVisitor.Visit(joinClause.OuterKeySelector);

                var oldInsideInnerKeySelector = TransformingVisitor._insideInnerKeySelector;
                TransformingVisitor._insideInnerKeySelector = true;
                joinClause.InnerKeySelector = TransformingVisitor.Visit(joinClause.InnerKeySelector);

                if (joinClause.OuterKeySelector.Type.IsNullableType()
                    && !joinClause.InnerKeySelector.Type.IsNullableType())
                {
                    joinClause.InnerKeySelector = Expression.Convert(joinClause.InnerKeySelector, joinClause.InnerKeySelector.Type.MakeNullable());
                }

                if (joinClause.InnerKeySelector.Type.IsNullableType()
                    && !joinClause.OuterKeySelector.Type.IsNullableType())
                {
                    joinClause.OuterKeySelector = Expression.Convert(joinClause.OuterKeySelector, joinClause.OuterKeySelector.Type.MakeNullable());
                }

                TransformingVisitor._insideInnerKeySelector = oldInsideInnerKeySelector;
            }

            public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
            {
                selectClause.Selector = _subqueryInjector.Visit(selectClause.Selector);

                if (_navigationExpansionSubquery)
                {
                    base.VisitSelectClause(selectClause, queryModel);
                    return;
                }

                var originalType = selectClause.Selector.Type;

                base.VisitSelectClause(selectClause, queryModel);

                selectClause.Selector = CompensateForNullabilityDifference(selectClause.Selector, originalType);
            }

            public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
            {
                if (resultOperator is AllResultOperator allResultOperator)
                {
                    Expression expressionExtractor(AllResultOperator o) => o.Predicate;
                    void adjuster(AllResultOperator o, Expression e) => o.Predicate = e;
                    VisitAndAdjustResultOperatorType(allResultOperator, expressionExtractor, adjuster);

                    return;
                }

                if (resultOperator is ContainsResultOperator containsResultOperator)
                {
                    Expression expressionExtractor(ContainsResultOperator o) => o.Item;
                    void adjuster(ContainsResultOperator o, Expression e) => o.Item = e;
                    VisitAndAdjustResultOperatorType(containsResultOperator, expressionExtractor, adjuster);

                    return;
                }

                if (resultOperator is SkipResultOperator skipResultOperator)
                {
                    Expression expressionExtractor(SkipResultOperator o) => o.Count;
                    void adjuster(SkipResultOperator o, Expression e) => o.Count = e;
                    VisitAndAdjustResultOperatorType(skipResultOperator, expressionExtractor, adjuster);

                    return;
                }

                if (resultOperator is TakeResultOperator takeResultOperator)
                {
                    Expression expressionExtractor(TakeResultOperator o) => o.Count;
                    void adjuster(TakeResultOperator o, Expression e) => o.Count = e;
                    VisitAndAdjustResultOperatorType(takeResultOperator, expressionExtractor, adjuster);

                    return;
                }

                if (resultOperator is GroupResultOperator groupResultOperator)
                {
                    groupResultOperator.ElementSelector
                        = _subqueryInjector.Visit(groupResultOperator.ElementSelector);

                    var originalKeySelectorType = groupResultOperator.KeySelector.Type;
                    var originalElementSelectorType = groupResultOperator.ElementSelector.Type;

                    base.VisitResultOperator(resultOperator, queryModel, index);

                    groupResultOperator.KeySelector = CompensateForNullabilityDifference(
                        groupResultOperator.KeySelector,
                        originalKeySelectorType);

                    groupResultOperator.ElementSelector = CompensateForNullabilityDifference(
                        groupResultOperator.ElementSelector,
                        originalElementSelectorType);

                    return;
                }

                base.VisitResultOperator(resultOperator, queryModel, index);
            }

            private void VisitAndAdjustResultOperatorType<TResultOperator>(
                TResultOperator resultOperator,
                Func<TResultOperator, Expression> expressionExtractor,
                Action<TResultOperator, Expression> adjuster)
                where TResultOperator : ResultOperatorBase
            {
                var originalExpression = expressionExtractor(resultOperator);
                var originalType = originalExpression.Type;

                var translatedExpression = CompensateForNullabilityDifference(
                    TransformingVisitor.Visit(originalExpression),
                    originalType);

                adjuster(resultOperator, translatedExpression);
            }
        }

        private class ProjectionSubqueryInjectingQueryModelVisitor : QueryModelVisitorBase
        {
            private readonly CollectionNavigationSubqueryInjector _subqueryInjector;

            public ProjectionSubqueryInjectingQueryModelVisitor(EntityQueryModelVisitor queryModelVisitor)
            {
                _subqueryInjector = new CollectionNavigationSubqueryInjector(queryModelVisitor, shouldInject: true);
            }

            public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
            {
                selectClause.Selector = _subqueryInjector.Visit(selectClause.Selector);

                base.VisitSelectClause(selectClause, queryModel);
            }
        }
    }
}
