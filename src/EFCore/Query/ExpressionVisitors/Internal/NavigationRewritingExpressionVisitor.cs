// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
        private readonly EntityQueryModelVisitor _queryModelVisitor;
        private readonly List<NavigationJoin> _navigationJoins = new List<NavigationJoin>();
        private readonly NavigationRewritingQueryModelVisitor _navigationRewritingQueryModelVisitor;
        private QueryModel _queryModel;
        private QueryModel _parentQueryModel;

        private bool _insideInnerSequence;
        private bool _innerKeySelectorRequiresNullRefProtection;
        private bool _insideInnerKeySelector;
        private bool _insideOrderBy;

        private class NavigationJoin
        {
            public static void RemoveNavigationJoin(
                ICollection<NavigationJoin> navigationJoins, NavigationJoin navigationJoin)
            {
                if (!navigationJoins.Remove(navigationJoin))
                {
                    foreach (var nj in navigationJoins)
                    {
                        nj.Remove(navigationJoin);
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
            public IEnumerable<IBodyClause> AdditionalBodyClauses { get; }
            public bool DependentToPrincipal { get; }
            public QuerySourceReferenceExpression QuerySourceReferenceExpression { get; }
            public readonly List<NavigationJoin> NavigationJoins = new List<NavigationJoin>();

            public IEnumerable<NavigationJoin> Iterate()
            {
                yield return this;

                foreach (var navigationJoin in NavigationJoins.SelectMany(nj => nj.Iterate()))
                {
                    yield return navigationJoin;
                }
            }

            private void Remove(NavigationJoin navigationJoin)
                => RemoveNavigationJoin(NavigationJoins, navigationJoin);
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
        public NavigationRewritingExpressionVisitor([NotNull] EntityQueryModelVisitor queryModelVisitor, bool navigationExpansionSubquery)
        {
            Check.NotNull(queryModelVisitor, nameof(queryModelVisitor));

            _queryModelVisitor = queryModelVisitor;
            _navigationRewritingQueryModelVisitor = new NavigationRewritingQueryModelVisitor(this, _queryModelVisitor, navigationExpansionSubquery);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Rewrite([NotNull] QueryModel queryModel, [CanBeNull] QueryModel parentQueryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            _queryModel = queryModel;
            _parentQueryModel = parentQueryModel;

            _navigationRewritingQueryModelVisitor.VisitQueryModel(_queryModel);

            foreach (var navigationJoin in _navigationJoins)
            {
                InsertNavigationJoin(navigationJoin);
            }

            if (parentQueryModel != null)
            {
                _queryModel = parentQueryModel;
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

        private void InsertNavigationJoin(NavigationJoin navigationJoin)
        {
            var insertionIndex = 0;
            var bodyClause = navigationJoin.QuerySource as IBodyClause;
            if (bodyClause != null)
            {
                insertionIndex = _queryModel.BodyClauses.IndexOf(bodyClause) + 1;
            }

            if (_queryModel.MainFromClause == navigationJoin.QuerySource
                || insertionIndex > 0)
            {
                foreach (var nj in navigationJoin.Iterate())
                {
                    _queryModel.BodyClauses.Insert(insertionIndex++, nj.JoinClause ?? (IBodyClause)nj.GroupJoinClause);
                    foreach (var additionalBodyClause in nj.AdditionalBodyClauses)
                    {
                        _queryModel.BodyClauses.Insert(insertionIndex++, additionalBodyClause);
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            Rewrite(expression.QueryModel, _queryModel);

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

            if (leftNavigationJoin != null
                && rightNavigationJoin != null)
            {
                if (leftNavigationJoin.DependentToPrincipal
                    && rightNavigationJoin.DependentToPrincipal)
                {
                    newLeft = leftJoin?.OuterKeySelector;
                    newRight = rightJoin?.OuterKeySelector;

                    NavigationJoin.RemoveNavigationJoin(_navigationJoins, leftNavigationJoin);
                    NavigationJoin.RemoveNavigationJoin(_navigationJoins, rightNavigationJoin);
                }
            }
            else
            {
                if (leftNavigationJoin != null)
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

                if (rightNavigationJoin != null)
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

            var result =
                _queryModelVisitor.BindNavigationPathPropertyExpression(
                    node,
                    (ps, qs) =>
                        {
                            if (qs != null)
                            {
                                return RewriteNavigationProperties(
                                    ps.ToList(),
                                    qs,
                                    node,
                                    node.Expression,
                                    node.Member.Name,
                                    node.Type,
                                    e => Expression.MakeMemberAccess(e, node.Member),
                                    e => new NullConditionalExpression(e, e, Expression.MakeMemberAccess(e, node.Member)));
                            }

                            return null;
                        });

            if (result != null)
            {
                return result;
            }

            var newEpression = Visit(node.Expression);

            return _insideInnerKeySelector && _innerKeySelectorRequiresNullRefProtection
                ? (Expression)new NullConditionalExpression(newEpression, newEpression, Expression.MakeMemberAccess(newEpression, node.Member))
                : Expression.MakeMemberAccess(newEpression, node.Member);
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
            var newArguments = node.Arguments.Select(Visit).ToList();

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
            var newExpressions = node.Expressions.Select(Visit).ToList();

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

            if (EntityQueryModelVisitor.IsPropertyMethod(node.Method))
            {
                var result = _queryModelVisitor.BindNavigationPathPropertyExpression(
                    node,
                    (ps, qs) =>
                        {
                            if (qs != null)
                            {
                                return RewriteNavigationProperties(
                                    ps.ToList(),
                                    qs,
                                    node,
                                    node.Arguments[0],
                                    (string)((ConstantExpression)node.Arguments[1]).Value,
                                    node.Type,
                                    e => Expression.Call(node.Method, e, node.Arguments[1]),
                                    e => new NullConditionalExpression(e, e, Expression.Call(node.Method, e, node.Arguments[1])));
                            }

                            return null;
                        });

                if (result != null)
                {
                    return result;
                }

                var propertyArguments = node.Arguments.Select(Visit).ToList();

                return _insideInnerKeySelector && _innerKeySelectorRequiresNullRefProtection
                    ? (Expression)new NullConditionalExpression(propertyArguments[0], propertyArguments[0], Expression.Call(node.Method, propertyArguments[0], node.Arguments[1]))
                    : Expression.Call(node.Method, propertyArguments[0], propertyArguments[1]);
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

        private Expression RewriteNavigationProperties(
            List<IPropertyBase> properties,
            IQuerySource querySource,
            Expression expression,
            Expression declaringExpression,
            string propertyName,
            Type propertyType,
            Func<Expression, Expression> propertyCreator,
            Func<Expression, Expression> conditionalAccessPropertyCreator)
        {
            var navigations = properties.OfType<INavigation>().ToList();

            if (navigations.Any())
            {
                var outerQuerySourceReferenceExpression = new QuerySourceReferenceExpression(querySource);

                var additionalFromClauseBeingProcessed = _navigationRewritingQueryModelVisitor.AdditionalFromClauseBeingProcessed;
                if (additionalFromClauseBeingProcessed != null
                    && navigations.Last().IsCollection())
                {
                    var fromSubqueryExpression = additionalFromClauseBeingProcessed.FromExpression as SubQueryExpression;
                    if (fromSubqueryExpression != null)
                    {
                        return RewriteSelectManyInsideSubqueryIntoJoins(
                            fromSubqueryExpression, 
                            outerQuerySourceReferenceExpression, 
                            navigations, 
                            additionalFromClauseBeingProcessed);
                    }

                    return RewriteSelectManyNavigationsIntoJoins(
                        outerQuerySourceReferenceExpression,
                        navigations,
                        additionalFromClauseBeingProcessed);
                }

                if (navigations.Count == 1 && navigations[0].IsDependentToPrincipal())
                {
                    var foreignKeyMemberAccess = TryCreateForeignKeyMemberAccess(propertyName, declaringExpression, navigations[0]);
                    if (foreignKeyMemberAccess != null)
                    {
                        return foreignKeyMemberAccess;
                    }
                }

                if (_insideInnerKeySelector)
                {
                    var translated = CreateSubqueryForNavigations(
                        outerQuerySourceReferenceExpression,
                        navigations,
                        propertyCreator);

                    return translated;
                }

                var navigationResultExpression = RewriteNavigationsIntoJoins(
                    outerQuerySourceReferenceExpression,
                    navigations,
                    properties.Count == navigations.Count ? null : propertyType,
                    propertyCreator,
                    conditionalAccessPropertyCreator);

                var resultQsre = navigationResultExpression as QuerySourceReferenceExpression;
                if (resultQsre != null)
                {
                    foreach (var includeResultOperator in _queryModelVisitor.QueryCompilationContext.QueryAnnotations
                        .OfType<IncludeResultOperator>()
                        .Where(o => o.PathFromQuerySource == expression))
                    {
                        includeResultOperator.QuerySource = resultQsre.ReferencedQuerySource;
                        includeResultOperator.PathFromQuerySource = resultQsre;
                    }
                }

                return navigationResultExpression;
            }

            return default(Expression);
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

        private Expression TryCreateForeignKeyMemberAccess(string propertyName, Expression declaringExpression, INavigation navigation)
        {
            var canPerformOptimization = true;
            if (_insideOrderBy)
            {
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
                    qsreFindingVisitor.Visit(_queryModel.SelectClause.Selector);

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
                    && principalKeyProperty.ClrType == navigation.ForeignKey.Properties[0].ClrType)
                {
                    var declaringMethodCallExpression = declaringExpression as MethodCallExpression;
                    var parentDeclaringExpression = declaringMethodCallExpression != null
                                                    && EntityQueryModelVisitor.IsPropertyMethod(declaringMethodCallExpression.Method)
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
            ICollection<INavigation> navigations,
            Func<Expression, Expression> propertyCreator)
        {
            var firstNavigation = navigations.First();
            var targetEntityType = firstNavigation.GetTargetType();

            var mainFromClause
                = new MainFromClause(
                    "subQuery",
                    targetEntityType.ClrType,
                    NullAsyncQueryProvider.Instance.CreateEntityQueryableExpression(targetEntityType.ClrType));

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
                    CreateKeyComparisonExpression(leftKeyAccess, rightKeyAccess)));

            subQueryModel.ResultOperators.Add(new FirstResultOperator(returnDefaultWhenEmpty: true));

            var selectClauseExpression = (Expression)querySourceReference;

            selectClauseExpression
                = navigations
                    .Skip(1)
                    .Aggregate(
                        selectClauseExpression,
                        (current, navigation) => Expression.Property(current, navigation.Name));

            subQueryModel.SelectClause = new SelectClause(propertyCreator(selectClauseExpression));

            if (navigations.Count > 1)
            {
                var subQueryVisitor = CreateVisitorForSubQuery(navigationExpansionSubquery: true);
                subQueryVisitor.Rewrite(subQueryModel, parentQueryModel: null);
            }

            var subQuery = new SubQueryExpression(subQueryModel);

            return subQuery;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual NavigationRewritingExpressionVisitor CreateVisitorForSubQuery(bool navigationExpansionSubquery)
            => new NavigationRewritingExpressionVisitor(
                _queryModelVisitor,
                navigationExpansionSubquery);

        private static BinaryExpression CreateKeyComparisonExpression(Expression leftExpression, Expression rightExpression)
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

        private Expression RewriteNavigationsIntoJoins(
            QuerySourceReferenceExpression outerQuerySourceReferenceExpression,
            IEnumerable<INavigation> navigations,
            Type propertyType,
            Func<Expression, Expression> propertyCreator,
            Func<Expression, Expression> conditionalAccessPropertyCreator)
        {
            var querySourceReferenceExpression = outerQuerySourceReferenceExpression;
            var navigationJoins = _navigationJoins;

            var optionalNavigationInChain = _insideInnerKeySelector
                                            && _innerKeySelectorRequiresNullRefProtection;

            foreach (var navigation in navigations)
            {
                var addNullCheckToOuterKeySelector = optionalNavigationInChain;

                if (!navigation.ForeignKey.IsRequired
                    || !navigation.IsDependentToPrincipal())
                {
                    optionalNavigationInChain = true;
                }

                var targetEntityType = navigation.GetTargetType();

                if (navigation.IsCollection())
                {
                    _queryModel.MainFromClause.FromExpression 
                        = NullAsyncQueryProvider.Instance.CreateEntityQueryableExpression(targetEntityType.ClrType);

                    var innerQuerySourceReferenceExpression
                        = new QuerySourceReferenceExpression(_queryModel.MainFromClause);

                    var leftKeyAccess = CreateKeyAccessExpression(
                        querySourceReferenceExpression,
                        navigation.IsDependentToPrincipal()
                            ? navigation.ForeignKey.Properties
                            : navigation.ForeignKey.PrincipalKey.Properties);

                    var rightKeyAccess = CreateKeyAccessExpression(
                        innerQuerySourceReferenceExpression,
                        navigation.IsDependentToPrincipal()
                            ? navigation.ForeignKey.PrincipalKey.Properties
                            : navigation.ForeignKey.Properties);

                    _queryModel.BodyClauses.Add(
                        new WhereClause(
                            CreateKeyComparisonExpression(leftKeyAccess, rightKeyAccess)));

                    return _queryModel.MainFromClause.FromExpression;
                }

                var navigationJoin
                    = navigationJoins
                        .FirstOrDefault(nj =>
                            nj.QuerySource == querySourceReferenceExpression.ReferencedQuerySource
                            && nj.Navigation == navigation);

                if (navigationJoin == null)
                {
                    QuerySourceReferenceExpression innerQuerySourceReferenceExpression;
                    var joinClause = BuildJoinFromNavigation(
                        querySourceReferenceExpression,
                        navigation,
                        targetEntityType,
                        addNullCheckToOuterKeySelector,
                        out innerQuerySourceReferenceExpression);

                    if (optionalNavigationInChain)
                    {
                        RewriteNavigationIntoGroupJoin(
                            joinClause,
                            navigation,
                            targetEntityType,
                            querySourceReferenceExpression,
                            null,
                            new List<IBodyClause>(),
                            new List<ResultOperatorBase> { new DefaultIfEmptyResultOperator(null) },
                            out navigationJoin);

                        navigationJoins.Add(navigationJoin);
                    }
                    else
                    {
                        navigationJoins.Add(
                            navigationJoin
                                = new NavigationJoin(
                                    querySourceReferenceExpression.ReferencedQuerySource,
                                    navigation,
                                    joinClause,
                                    new List<IBodyClause>(),
                                    navigation.IsDependentToPrincipal(),
                                    innerQuerySourceReferenceExpression));
                    }
                }

                querySourceReferenceExpression = navigationJoin.QuerySourceReferenceExpression;
                navigationJoins = navigationJoin.NavigationJoins;
            }

            if (_insideInnerSequence && optionalNavigationInChain)
            {
                _innerKeySelectorRequiresNullRefProtection = true;
            }

            if (propertyType == null)
            {
                return querySourceReferenceExpression;
            }

            return optionalNavigationInChain
                ? conditionalAccessPropertyCreator(querySourceReferenceExpression)
                : propertyCreator(querySourceReferenceExpression);
        }

        private static void RewriteNavigationIntoGroupJoin(
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
            var defaultIfEmptyAdditionalFromClause = new AdditionalFromClause(joinClause.ItemName, joinClause.ItemType, defaultIfEmptySubquery);
            navigationJoin = new NavigationJoin(
                querySourceReferenceExpression.ReferencedQuerySource,
                navigation,
                groupJoinClause,
                new[] { defaultIfEmptyAdditionalFromClause },
                navigation.IsDependentToPrincipal(),
                new QuerySourceReferenceExpression(defaultIfEmptyAdditionalFromClause));
        }

        private Expression RewriteSelectManyNavigationsIntoJoins(
            QuerySourceReferenceExpression outerQuerySourceReferenceExpression,
            IEnumerable<INavigation> navigations,
            AdditionalFromClause additionalFromClauseBeingProcessed)
        {
            var querySourceReferenceExpression = outerQuerySourceReferenceExpression;
            var additionalJoinIndex = _queryModel.BodyClauses.IndexOf(additionalFromClauseBeingProcessed);
            var joinClauses = new List<JoinClause>();

            foreach (var navigation in navigations)
            {
                var targetEntityType = navigation.GetTargetType();

                QuerySourceReferenceExpression innerQuerySourceReferenceExpression;

                var joinClause = BuildJoinFromNavigation(
                    querySourceReferenceExpression,
                    navigation,
                    targetEntityType,
                    false,
                    out innerQuerySourceReferenceExpression);

                joinClauses.Add(joinClause);

                querySourceReferenceExpression = innerQuerySourceReferenceExpression;
            }

            _queryModel.BodyClauses.RemoveAt(additionalJoinIndex);

            for (var i = 0; i < joinClauses.Count; i++)
            {
                _queryModel.BodyClauses.Insert(additionalJoinIndex + i, joinClauses[i]);
            }

            var querySourceMapping = new QuerySourceMapping();
            querySourceMapping.AddMapping(additionalFromClauseBeingProcessed, querySourceReferenceExpression);

            _queryModel.TransformExpressions(e =>
                ReferenceReplacingExpressionVisitor
                    .ReplaceClauseReferences(e, querySourceMapping, throwOnUnmappedReferences: false));

            foreach (var includeResultOperator in _queryModelVisitor.QueryCompilationContext.QueryAnnotations.OfType<IncludeResultOperator>())
            {
                if ((includeResultOperator.PathFromQuerySource as QuerySourceReferenceExpression)?.ReferencedQuerySource
                    == additionalFromClauseBeingProcessed)
                {
                    includeResultOperator.PathFromQuerySource = querySourceReferenceExpression;
                    includeResultOperator.QuerySource = querySourceReferenceExpression.ReferencedQuerySource;
                }
            }

            return querySourceReferenceExpression;
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

                QuerySourceReferenceExpression innerQuerySourceReferenceExpression;
                var joinClause = BuildJoinFromNavigation(
                    outerQuerySourceReferenceExpression, 
                    navigation, 
                    targetEntityType, 
                    false, 
                    out innerQuerySourceReferenceExpression);

                if (navigation == collectionNavigation)
                {
                    NavigationJoin navigationJoin;
                    RewriteNavigationIntoGroupJoin(
                        joinClause,
                        navigations.Last(),
                        targetEntityType,
                        outerQuerySourceReferenceExpression,
                        fromSubqueryExpression.QueryModel.MainFromClause,
                        fromSubqueryExpression.QueryModel.BodyClauses,
                        fromSubqueryExpression.QueryModel.ResultOperators,
                        out navigationJoin);

                    _navigationJoins.Add(navigationJoin);

                    var additionalFromClauseIndex = _parentQueryModel.BodyClauses.IndexOf(additionalFromClauseBeingProcessed);
                    _parentQueryModel.BodyClauses.Remove(additionalFromClauseBeingProcessed);

                    var i = additionalFromClauseIndex;
                    foreach (var addedJoinClause in adddedJoinClauses)
                    {
                        _parentQueryModel.BodyClauses.Insert(i++, addedJoinClause);
                    }

                    var querySourceMapping = new QuerySourceMapping();
                    querySourceMapping.AddMapping(additionalFromClauseBeingProcessed, navigationJoin.QuerySourceReferenceExpression);

                    _parentQueryModel.TransformExpressions(e =>
                        ReferenceReplacingExpressionVisitor
                            .ReplaceClauseReferences(e, querySourceMapping, throwOnUnmappedReferences: false));

                    foreach (var includeResultOperator in _queryModelVisitor.QueryCompilationContext.QueryAnnotations.OfType<IncludeResultOperator>())
                    {
                        if ((includeResultOperator.PathFromQuerySource as QuerySourceReferenceExpression)?.ReferencedQuerySource
                            == additionalFromClauseBeingProcessed)
                        {
                            includeResultOperator.PathFromQuerySource = navigationJoin.QuerySourceReferenceExpression;
                            includeResultOperator.QuerySource = navigationJoin.QuerySourceReferenceExpression.ReferencedQuerySource;
                        }
                    }

                    return navigationJoin.QuerySourceReferenceExpression;
                }

                adddedJoinClauses.Add(joinClause);

                outerQuerySourceReferenceExpression = innerQuerySourceReferenceExpression;
            }

            return outerQuerySourceReferenceExpression;
        }

        private JoinClause BuildJoinFromNavigation(
            QuerySourceReferenceExpression querySourceReferenceExpression,
            INavigation navigation,
            IEntityType targetEntityType,
            bool addNullCheckToOuterKeySelector,
            out QuerySourceReferenceExpression innerQuerySourceReferenceExpression)
        {
            var outerKeySelector =
                CreateKeyAccessExpression(
                    querySourceReferenceExpression,
                    navigation.IsDependentToPrincipal()
                        ? navigation.ForeignKey.Properties
                        : navigation.ForeignKey.PrincipalKey.Properties,
                    addNullCheck: addNullCheckToOuterKeySelector);

            var joinClause
                = new JoinClause(
                    $"{querySourceReferenceExpression.ReferencedQuerySource.ItemName}.{navigation.Name}", // Interpolation okay; strings
                    targetEntityType.ClrType,
                    NullAsyncQueryProvider.Instance.CreateEntityQueryableExpression(targetEntityType.ClrType),
                    outerKeySelector,
                    Expression.Constant(null));

            innerQuerySourceReferenceExpression = new QuerySourceReferenceExpression(joinClause);

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

        private static readonly MethodInfo _propertyMethodInfo
            = typeof(EF).GetTypeInfo().GetDeclaredMethod(nameof(Property));

        private static Expression CreatePropertyExpression(Expression target, IProperty property, bool addNullCheck)
        {
            var propertyExpression = (Expression)Expression.Call(
                null,
                _propertyMethodInfo.MakeGenericMethod(property.ClrType),
                target,
                Expression.Constant(property.Name));

            return addNullCheck
                ? new NullConditionalExpression(target, target, propertyExpression)
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

            var needsTypeCompensation = (originalType != newType)
                                        && !originalType.IsNullableType()
                                        && newType.IsNullableType()
                                        && (originalType == newType.UnwrapNullableType());

            return needsTypeCompensation
                ? Expression.Convert(expression, originalType)
                : expression;
        }

        private class NavigationRewritingQueryModelVisitor : ExpressionTransformingQueryModelVisitor<NavigationRewritingExpressionVisitor>
        {
            private readonly SubqueryInjector _subqueryInjector;
            private readonly bool _navigationExpansionSubquery;

            public AdditionalFromClause AdditionalFromClauseBeingProcessed { get; private set; }

            public NavigationRewritingQueryModelVisitor(
                NavigationRewritingExpressionVisitor transformingVisitor,
                EntityQueryModelVisitor queryModelVisitor,
                bool navigationExpansionSubquery)
                : base(transformingVisitor)
            {
                _subqueryInjector = new SubqueryInjector(queryModelVisitor);
                _navigationExpansionSubquery = navigationExpansionSubquery;
            }

            public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
            {
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
                var oldInsideInnerSequence = TransformingVisitor._insideInnerSequence;
                var oldInnerKeySelectorRequiresNullRefProtection = TransformingVisitor._innerKeySelectorRequiresNullRefProtection;
                TransformingVisitor._insideInnerSequence = true;
                TransformingVisitor._innerKeySelectorRequiresNullRefProtection = false;
                joinClause.InnerSequence = TransformingVisitor.Visit(joinClause.InnerSequence);
                TransformingVisitor._insideInnerSequence = oldInsideInnerSequence;

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
                TransformingVisitor._innerKeySelectorRequiresNullRefProtection = oldInnerKeySelectorRequiresNullRefProtection;
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
                var allResultOperator = resultOperator as AllResultOperator;
                if (allResultOperator != null)
                {
                    Func<AllResultOperator, Expression> expressionExtractor = o => o.Predicate;
                    Action<AllResultOperator, Expression> adjuster = (o, e) => o.Predicate = e;
                    VisitAndAdjustResultOperatorType(allResultOperator, expressionExtractor, adjuster);

                    return;
                }

                var containsResultOperator = resultOperator as ContainsResultOperator;
                if (containsResultOperator != null)
                {
                    Func<ContainsResultOperator, Expression> expressionExtractor = o => o.Item;
                    Action<ContainsResultOperator, Expression> adjuster = (o, e) => o.Item = e;
                    VisitAndAdjustResultOperatorType(containsResultOperator, expressionExtractor, adjuster);

                    return;
                }

                var skipResultOperator = resultOperator as SkipResultOperator;
                if (skipResultOperator != null)
                {
                    Func<SkipResultOperator, Expression> expressionExtractor = o => o.Count;
                    Action<SkipResultOperator, Expression> adjuster = (o, e) => o.Count = e;
                    VisitAndAdjustResultOperatorType(skipResultOperator, expressionExtractor, adjuster);

                    return;
                }

                var takeResultOperator = resultOperator as TakeResultOperator;
                if (takeResultOperator != null)
                {
                    Func<TakeResultOperator, Expression> expressionExtractor = o => o.Count;
                    Action<TakeResultOperator, Expression> adjuster = (o, e) => o.Count = e;
                    VisitAndAdjustResultOperatorType(takeResultOperator, expressionExtractor, adjuster);

                    return;
                }

                var groupResultOperator = resultOperator as GroupResultOperator;
                if (groupResultOperator != null)
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

            private class SubqueryInjector : RelinqExpressionVisitor
            {
                private readonly EntityQueryModelVisitor _queryModelVisitor;

                public SubqueryInjector(EntityQueryModelVisitor queryModelVisitor)
                {
                    _queryModelVisitor = queryModelVisitor;
                }

                protected override Expression VisitMember(MemberExpression node)
                {
                    Check.NotNull(node, nameof(node));

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
        }
    }
}
