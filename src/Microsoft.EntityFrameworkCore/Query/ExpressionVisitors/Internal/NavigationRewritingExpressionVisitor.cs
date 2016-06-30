// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
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
        private readonly NavigationRewritingExpressionVisitor _parentvisitor;

        private QueryModel _queryModel;

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

        private IAsyncQueryProvider _entityQueryProvider;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public NavigationRewritingExpressionVisitor([NotNull] EntityQueryModelVisitor queryModelVisitor, bool navigationExpansionSubquery = false)
        {
            Check.NotNull(queryModelVisitor, nameof(queryModelVisitor));

            _queryModelVisitor = queryModelVisitor;
            _navigationRewritingQueryModelVisitor = new NavigationRewritingQueryModelVisitor(this, _queryModelVisitor, navigationExpansionSubquery);
        }

        private NavigationRewritingExpressionVisitor(
            EntityQueryModelVisitor queryModelVisitor, 
            IAsyncQueryProvider entityQueryProvider, 
            NavigationRewritingExpressionVisitor parentvisitor, 
            bool navigationExpansionSubquery)
            : this(queryModelVisitor, navigationExpansionSubquery)
        {
            _entityQueryProvider = entityQueryProvider;
            _parentvisitor = parentvisitor;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Rewrite([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            _queryModel = queryModel;

            _navigationRewritingQueryModelVisitor.VisitQueryModel(_queryModel);

            foreach (var navigationJoin in _navigationJoins)
            {
                InsertNavigationJoin(navigationJoin);
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
                || insertionIndex > 0
                || _parentvisitor == null)
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
            else
            {
                _parentvisitor.InsertNavigationJoin(navigationJoin);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            var navigationRewritingExpressionVisitor = CreateVisitorForSubQuery();

            navigationRewritingExpressionVisitor.Rewrite(expression.QueryModel);

            return expression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (_entityQueryProvider == null)
            {
                _entityQueryProvider
                    = (node.Value as IQueryable)?.Provider as IAsyncQueryProvider;

                var parent = _parentvisitor;
                while (parent != null)
                {
                    parent._entityQueryProvider = _entityQueryProvider;
                    parent = parent._parentvisitor;
                }
            }

            return node;
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
                    var constantExpression = newRight as ConstantExpression;

                    if (constantExpression != null
                        && constantExpression.Value == null)
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
                    var constantExpression = newLeft as ConstantExpression;

                    if (constantExpression != null
                        && constantExpression.Value == null)
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

        private static NewExpression CreateNullCompositeKey(Expression otherExpression)
            => Expression.New(
                CompositeKey.CompositeKeyCtor,
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

            return
                _queryModelVisitor.BindNavigationPathPropertyExpression(
                    node,
                    (ps, qs) =>
                    {
                        return RewriteNavigationProperties(
                            ps.ToList(), 
                            qs, 
                            node.Expression,
                            node.Member.Name, 
                            node.Type,
                            e => Expression.MakeMemberAccess(e, node.Member));
                    })
                ?? base.VisitMember(node);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Check.NotNull(node, nameof(node));

            if (!EntityQueryModelVisitor.IsPropertyMethod(node.Method))
            {
                base.VisitMethodCall(node);
            }

            return
                _queryModelVisitor.BindNavigationPathPropertyExpression(
                    node,
                    (ps, qs) =>
                    {
                        return RewriteNavigationProperties(
                            ps.ToList(),
                            qs,
                            node.Arguments[0],
                            (string)((ConstantExpression)node.Arguments[1]).Value,
                            node.Type,
                            e => Expression.Call(node.Method, e, node.Arguments[1]));
                    })
                ?? base.VisitMethodCall(node);
        }

        private Expression RewriteNavigationProperties(
            List<IPropertyBase> properties, 
            IQuerySource querySource,
            Expression declaringExpression,
            string propertyName,
            Type propertyType,
            Func<Expression, Expression> propertyCreator)
        {
            var navigations = properties.OfType<INavigation>().ToList();

            if (navigations.Any())
            {
                var outerQuerySourceReferenceExpression = new QuerySourceReferenceExpression(querySource);

                if (_navigationRewritingQueryModelVisitor.AdditionalFromClauseBeingProcessed != null
                    && navigations.Last().IsCollection())
                {
                    return RewriteSelectManyNavigationsIntoJoins(
                        outerQuerySourceReferenceExpression,
                        navigations,
                        _navigationRewritingQueryModelVisitor.AdditionalFromClauseBeingProcessed);
                }

                if (navigations.Count == 1
                    && navigations[0].IsDependentToPrincipal())
                {
                    var foreignKeyMemberAccess = CreateForeignKeyMemberAccess(propertyName, declaringExpression, navigations[0]);
                    if (foreignKeyMemberAccess != null)
                    {
                        return foreignKeyMemberAccess;
                    }
                }

                if (_navigationRewritingQueryModelVisitor.InsideInnerKeySelector)
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
                    propertyCreator);

                return navigationResultExpression;
            }

            return default(Expression);
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
                    targetEntityType.ClrType, CreateEntityQueryable(targetEntityType));

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
                subQueryVisitor.Rewrite(subQueryModel);
            }

            var subQuery = new SubQueryExpression(subQueryModel);

            return subQuery;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual NavigationRewritingExpressionVisitor CreateVisitorForSubQuery(bool navigationExpansionSubquery = false)
            => new NavigationRewritingExpressionVisitor(_queryModelVisitor, _entityQueryProvider, this, navigationExpansionSubquery);

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
            Func<Expression, Expression> propertyCreator)
        {
            var querySourceReferenceExpression = outerQuerySourceReferenceExpression;
            var navigationJoins = _navigationJoins;
            var optionalNavigationInChain = false;

            foreach (var navigation in navigations)
            {
                if (!navigation.ForeignKey.IsRequired)
                {
                    optionalNavigationInChain = true;
                }

                var targetEntityType = navigation.GetTargetType();

                if (navigation.IsCollection())
                {
                    _queryModel.MainFromClause.FromExpression = CreateEntityQueryable(targetEntityType);

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
                        optionalNavigationInChain,
                        out innerQuerySourceReferenceExpression);

                    var additionalBodyClauses = new List<IBodyClause>();
                    if (optionalNavigationInChain || !navigation.ForeignKey.IsRequired)
                    {
                        var groupJoinClause
                            = new GroupJoinClause(
                                joinClause.ItemName + "_group",
                                typeof(IEnumerable<>).MakeGenericType(targetEntityType.ClrType),
                                joinClause);

                        var groupReferenceExpression = new QuerySourceReferenceExpression(groupJoinClause);

                        var defaultIfEmptyMainFromClause = new MainFromClause(joinClause.ItemName + "_groupItem", joinClause.ItemType, groupReferenceExpression);
                        var newQuerySourceReferenceExpression = new QuerySourceReferenceExpression(defaultIfEmptyMainFromClause);

                        var defaultIfEmptyQueryModel = new QueryModel(
                            defaultIfEmptyMainFromClause,
                            new SelectClause(newQuerySourceReferenceExpression));
                        defaultIfEmptyQueryModel.ResultOperators.Add(new DefaultIfEmptyResultOperator(null));

                        var defaultIfEmptySubquery = new SubQueryExpression(defaultIfEmptyQueryModel);
                        var defaultIfEmptyAdditionalFromClause = new AdditionalFromClause(joinClause.ItemName, joinClause.ItemType, defaultIfEmptySubquery);

                        additionalBodyClauses.Add(defaultIfEmptyAdditionalFromClause);

                        navigationJoins.Add(
                            navigationJoin
                                = new NavigationJoin(
                                    querySourceReferenceExpression.ReferencedQuerySource,
                                    navigation,
                                    groupJoinClause,
                                    additionalBodyClauses,
                                    navigation.IsDependentToPrincipal(),
                                    new QuerySourceReferenceExpression(defaultIfEmptyAdditionalFromClause)));
                    }
                    else
                    {
                        navigationJoins.Add(
                            navigationJoin
                                = new NavigationJoin(
                                    querySourceReferenceExpression.ReferencedQuerySource,
                                    navigation,
                                    joinClause,
                                    additionalBodyClauses,
                                    navigation.IsDependentToPrincipal(),
                                    innerQuerySourceReferenceExpression));
                    }
                }

                querySourceReferenceExpression = navigationJoin.QuerySourceReferenceExpression;
                navigationJoins = navigationJoin.NavigationJoins;
            }

            if (propertyType == null)
            {
                return querySourceReferenceExpression;
            }

            if (optionalNavigationInChain)
            {
                var memberAccessExpression = propertyCreator(querySourceReferenceExpression);
                if (!propertyType.IsNullableType())
                {
                    memberAccessExpression = Expression.Convert(memberAccessExpression, propertyType.MakeNullable());
                }

                var constantNullExpression = propertyType.IsNullableType()
                    ? Expression.Constant(null, propertyType)
                    : Expression.Constant(null, propertyType.MakeNullable());

                return Expression.Condition(
                    Expression.NotEqual(
                        querySourceReferenceExpression,
                        Expression.Constant(null, querySourceReferenceExpression.Type)),
                    memberAccessExpression,
                    constantNullExpression);
            }

            return propertyCreator(querySourceReferenceExpression);
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

            var querySourceReplacingVisitor = new QuerySourceReplacingExpressionVisitor(
                additionalFromClauseBeingProcessed, querySourceReferenceExpression);

            var queryModelVisitor = new ExpressionTransformingQueryModelVisitor(querySourceReplacingVisitor);
            queryModelVisitor.VisitQueryModel(_queryModel);

            return querySourceReferenceExpression;
        }

        private class QuerySourceReplacingExpressionVisitor : RelinqExpressionVisitor
        {
            private readonly IQuerySource _querySourceToReplace;
            private readonly QuerySourceReferenceExpression _targetQuerySourceReferenceExpression;

            public QuerySourceReplacingExpressionVisitor(
                IQuerySource querySourceToReplace,
                QuerySourceReferenceExpression targetQuerySourceReferenceExpression)
            {
                _querySourceToReplace = querySourceToReplace;
                _targetQuerySourceReferenceExpression = targetQuerySourceReferenceExpression;
            }

            protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression expression)
            {
                if (expression.ReferencedQuerySource == _querySourceToReplace)
                {
                    return _targetQuerySourceReferenceExpression;
                }

                return base.VisitQuerySourceReference(expression);
            }
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
                    CreateEntityQueryable(targetEntityType),
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
        {
            return properties.Count == 1
                ? CreatePropertyExpression(target, properties[0], addNullCheck)
                : Expression.New(
                    CompositeKey.CompositeKeyCtor,
                    Expression.NewArrayInit(
                        typeof(object),
                        properties
                            .Select(p => Expression.Convert(CreatePropertyExpression(target, p, addNullCheck), typeof(object)))
                            .Cast<Expression>()
                            .ToArray()));
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

        private static bool IsCompositeKey([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            return type == typeof(CompositeKey);
        }

        private ConstantExpression CreateEntityQueryable(IEntityType targetEntityType)
            => Expression.Constant(
                _createEntityQueryableMethod
                    .MakeGenericMethod(targetEntityType.ClrType)
                    .Invoke(null, new object[]
                    {
                        _entityQueryProvider
                    }));

        private static readonly MethodInfo _createEntityQueryableMethod
            = typeof(NavigationRewritingExpressionVisitor)
                .GetTypeInfo().GetDeclaredMethod(nameof(_CreateEntityQueryable));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static EntityQueryable<TResult> _CreateEntityQueryable<TResult>(IAsyncQueryProvider entityQueryProvider)
            => new EntityQueryable<TResult>(entityQueryProvider);

        private class NavigationRewritingQueryModelVisitor : ExpressionTransformingQueryModelVisitor
        {
            private readonly SubqueryInjector _subqueryInjector;
            private readonly bool _navigationExpansionSubquery;

            public bool InsideInnerKeySelector { get; private set; }

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
                    whereClause.Predicate = Expression.Convert(whereClause.Predicate, typeof(bool));
                }
            }

            public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
            {
                var originalTypes = orderByClause.Orderings.Select(o => o.Expression.Type).ToList();

                base.VisitOrderByClause(orderByClause, queryModel, index);

                var newTypes = orderByClause.Orderings.Select(o => o.Expression.Type).ToList();

                Debug.Assert(originalTypes.Count == newTypes.Count);

                for (var i = 0; i < newTypes.Count; i++)
                {
                    if ((originalTypes[i] != newTypes[i])
                        && !originalTypes[i].IsNullableType()
                        && newTypes[i].IsNullableType()
                        && (originalTypes[i].UnwrapNullableType() == newTypes[i].UnwrapNullableType()))
                    {
                        orderByClause.Orderings[i].Expression = Expression.Convert(orderByClause.Orderings[i].Expression, originalTypes[i]);
                    }
                }
            }

            public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
                => VisitJoinClauseInternal(joinClause);

            public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, GroupJoinClause groupJoinClause)
                => VisitJoinClauseInternal(joinClause);

            private void VisitJoinClauseInternal(JoinClause joinClause)
            {
                joinClause.InnerSequence = TransformingVisitor.Visit(joinClause.InnerSequence);
                joinClause.OuterKeySelector = TransformingVisitor.Visit(joinClause.OuterKeySelector);

                var oldInsideInnerKeySelector = InsideInnerKeySelector;
                InsideInnerKeySelector = true;
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

                InsideInnerKeySelector = oldInsideInnerKeySelector;
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

                var newType = selectClause.Selector.Type;
                if ((originalType != newType)
                    && !originalType.IsNullableType()
                    && newType.IsNullableType()
                    && (originalType.UnwrapNullableType() == newType.UnwrapNullableType()))
                {
                    selectClause.Selector = Expression.Convert(selectClause.Selector, originalType);
                }
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
                    var originalKeySelectorType = groupResultOperator.KeySelector.Type;
                    var originalElementSelectorType = groupResultOperator.ElementSelector.Type;

                    base.VisitResultOperator(resultOperator, queryModel, index);

                    var newKeySelectorType = groupResultOperator.KeySelector.Type;
                    var newElementSelectorType = groupResultOperator.ElementSelector.Type;

                    if (originalKeySelectorType != newKeySelectorType
                        && !originalKeySelectorType.IsNullableType()
                        && newKeySelectorType.IsNullableType()
                        && originalKeySelectorType.UnwrapNullableType() == newKeySelectorType.UnwrapNullableType())
                    {
                        groupResultOperator.KeySelector = Expression.Convert(groupResultOperator.KeySelector, originalKeySelectorType);
                    }

                    if (originalElementSelectorType != newElementSelectorType
                        && !originalElementSelectorType.IsNullableType()
                        && newElementSelectorType.IsNullableType()
                        && originalElementSelectorType.UnwrapNullableType() == newElementSelectorType.UnwrapNullableType())
                    {
                        groupResultOperator.ElementSelector = Expression.Convert(groupResultOperator.ElementSelector, originalElementSelectorType);
                    }

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

                var translatedExpression = TransformingVisitor.Visit(originalExpression);

                var newType = translatedExpression.Type;
                if ((originalType != newType)
                    && !originalType.IsNullableType()
                    && newType.IsNullableType()
                    && (originalType.UnwrapNullableType() == newType.UnwrapNullableType()))
                {
                    translatedExpression = Expression.Convert(translatedExpression, originalType);
                }

                adjuster(resultOperator, translatedExpression);
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
