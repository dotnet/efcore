// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
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
                QuerySourceReferenceExpression querySourceReferenceExpression)
            {
                QuerySource = querySource;
                Navigation = navigation;
                JoinClause = joinClause;
                QuerySourceReferenceExpression = querySourceReferenceExpression;
            }

            public IQuerySource QuerySource { get; }
            public INavigation Navigation { get; }
            public JoinClause JoinClause { get; }
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

        public NavigationRewritingExpressionVisitor([NotNull] EntityQueryModelVisitor queryModelVisitor)
        {
            Check.NotNull(queryModelVisitor, nameof(queryModelVisitor));

            _queryModelVisitor = queryModelVisitor;
            _navigationRewritingQueryModelVisitor = new NavigationRewritingQueryModelVisitor(this, _queryModelVisitor);
        }

        private NavigationRewritingExpressionVisitor(
            EntityQueryModelVisitor queryModelVisitor, IAsyncQueryProvider entityQueryProvider, NavigationRewritingExpressionVisitor parentvisitor)
            : this(queryModelVisitor)
        {
            _entityQueryProvider = entityQueryProvider;
            _parentvisitor = parentvisitor;
        }

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
                    _queryModel.BodyClauses.Insert(insertionIndex++, nj.JoinClause);
                }
            }
            else
            {
                _parentvisitor.InsertNavigationJoin(navigationJoin);
            }
        }

        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            var navigationRewritingExpressionVisitor = CreateVisitorForSubQuery();

            navigationRewritingExpressionVisitor.Rewrite(expression.QueryModel);

            return expression;
        }

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

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var newLeft = Visit(node.Left);
            var newRight = Visit(node.Right);

            var leftNavigationJoin
                = _navigationJoins
                    .SelectMany(nj => nj.Iterate())
                    .FirstOrDefault(nj => ReferenceEquals(nj.QuerySourceReferenceExpression, newLeft));

            var rightNavigationJoin
                = _navigationJoins
                    .SelectMany(nj => nj.Iterate())
                    .FirstOrDefault(nj => ReferenceEquals(nj.QuerySourceReferenceExpression, newRight));

            if ((leftNavigationJoin != null)
                && (rightNavigationJoin != null))
            {
                newLeft = leftNavigationJoin.JoinClause.OuterKeySelector;
                newRight = rightNavigationJoin.JoinClause.OuterKeySelector;

                NavigationJoin.RemoveNavigationJoin(_navigationJoins, leftNavigationJoin);
                NavigationJoin.RemoveNavigationJoin(_navigationJoins, rightNavigationJoin);
            }
            else
            {
                if (leftNavigationJoin != null)
                {
                    var constantExpression = newRight as ConstantExpression;

                    if ((constantExpression != null)
                        && (constantExpression.Value == null))
                    {
                        newLeft = leftNavigationJoin.JoinClause.OuterKeySelector;

                        NavigationJoin.RemoveNavigationJoin(_navigationJoins, leftNavigationJoin);

                        if (IsCompositeKey(newLeft.Type))
                        {
                            newRight = CreateNullCompositeKey(newLeft);
                        }
                    }
                    else
                    {
                        newLeft = leftNavigationJoin.JoinClause.InnerKeySelector;
                    }
                }

                if (rightNavigationJoin != null)
                {
                    var constantExpression = newLeft as ConstantExpression;

                    if ((constantExpression != null)
                        && (constantExpression.Value == null))
                    {
                        newRight = rightNavigationJoin.JoinClause.OuterKeySelector;

                        NavigationJoin.RemoveNavigationJoin(_navigationJoins, rightNavigationJoin);

                        if (IsCompositeKey(newRight.Type))
                        {
                            newLeft = CreateNullCompositeKey(newRight);
                        }
                    }
                    else
                    {
                        newRight = rightNavigationJoin.JoinClause.InnerKeySelector;
                    }
                }
            }

            return Expression.MakeBinary(
                node.NodeType,
                newLeft,
                newRight,
                node.IsLiftedToNull,
                node.Method);
        }

        private static NewExpression CreateNullCompositeKey(Expression otherExpression)
            => Expression.New(
                _compositeKeyCtor,
                Expression.NewArrayInit(
                    typeof(object),
                    Enumerable.Repeat(
                        Expression.Constant(null),
                        ((NewArrayExpression)((NewExpression)otherExpression).Arguments.Single()).Expressions.Count)));

        protected override Expression VisitMember(MemberExpression node)
        {
            Check.NotNull(node, nameof(node));

            return
                _queryModelVisitor.BindNavigationPathMemberExpression(
                    node,
                    (ps, qs) =>
                        {
                            var properties = ps.ToList();
                            var navigations = properties.OfType<INavigation>().ToList();

                            if (navigations.Any())
                            {
                                if ((navigations.Count == 1)
                                    && navigations[0].IsDependentToPrincipal())
                                {
                                    var foreignKeyMemberAccess = CreateForeignKeyMemberAccess(node, navigations[0]);
                                    if (foreignKeyMemberAccess != null)
                                    {
                                        return foreignKeyMemberAccess;
                                    }
                                }

                                var outerQuerySourceReferenceExpression = new QuerySourceReferenceExpression(qs);

                                if (_navigationRewritingQueryModelVisitor.InsideInnerKeySelector)
                                {
                                    var translated = CreateSubqueryForNavigations(outerQuerySourceReferenceExpression, navigations, node);

                                    return translated;
                                }

                                var innerQuerySourceReferenceExpression
                                    = CreateJoinsForNavigations(outerQuerySourceReferenceExpression, navigations);

                                return properties.Count == navigations.Count
                                    ? innerQuerySourceReferenceExpression
                                    : Expression.MakeMemberAccess(innerQuerySourceReferenceExpression, node.Member);
                            }

                            return default(Expression);
                        })
                ?? base.VisitMember(node);
        }

        private static Expression CreateForeignKeyMemberAccess(MemberExpression memberExpression, INavigation navigation)
        {
            var principalKey = navigation.ForeignKey.PrincipalKey;
            if (principalKey.Properties.Count == 1)
            {
                Debug.Assert(navigation.ForeignKey.Properties.Count == 1);

                var principalKeyProperty = principalKey.Properties[0];
                if (principalKeyProperty.Name == memberExpression.Member.Name 
                    && principalKeyProperty.ClrType == navigation.ForeignKey.Properties[0].ClrType)
                {
                    var declaringExpression = ((MemberExpression)memberExpression.Expression).Expression;
                    var foreignKeyPropertyExpression = CreateKeyAccessExpression(declaringExpression, navigation.ForeignKey.Properties);

                    return foreignKeyPropertyExpression;
                }
            }

            return null;
        }

        private Expression CreateSubqueryForNavigations(
            Expression outerQuerySourceReferenceExpression,
            ICollection<INavigation> navigations,
            MemberExpression memberExpression)
        {
            var firstNavigation = navigations.First();
            var targetEntityType = firstNavigation.GetTargetType();

            var mainFromClause
                = new MainFromClause(
                    _queryModel.GetNewName("subQuery"),
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

            subQueryModel.SelectClause = new SelectClause(Expression.MakeMemberAccess(selectClauseExpression, memberExpression.Member));

            if (navigations.Count > 1)
            {
                var subQueryVisitor = CreateVisitorForSubQuery();
                subQueryVisitor.Rewrite(subQueryModel);
            }

            var subQuery = new SubQueryExpression(subQueryModel);

            return subQuery;
        }

        public virtual NavigationRewritingExpressionVisitor CreateVisitorForSubQuery()
            => new NavigationRewritingExpressionVisitor(_queryModelVisitor, _entityQueryProvider, this);

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

        private Expression CreateJoinsForNavigations(
            QuerySourceReferenceExpression outerQuerySourceReferenceExpression,
            IEnumerable<INavigation> navigations)
        {
            var querySourceReferenceExpression = outerQuerySourceReferenceExpression;
            var navigationJoins = _navigationJoins;

            foreach (var navigation in navigations)
            {
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
                            (nj.QuerySource == querySourceReferenceExpression.ReferencedQuerySource)
                            && (nj.Navigation == navigation));

                if (navigationJoin == null)
                {
                    var joinClause
                        = new JoinClause(
                            $"{querySourceReferenceExpression.ReferencedQuerySource.ItemName}.{navigation.Name}",
                            targetEntityType.ClrType,
                            CreateEntityQueryable(targetEntityType),
                            CreateKeyAccessExpression(
                                querySourceReferenceExpression,
                                navigation.IsDependentToPrincipal()
                                    ? navigation.ForeignKey.Properties
                                    : navigation.ForeignKey.PrincipalKey.Properties),
                            Expression.Constant(null));

                    var innerQuerySourceReferenceExpression
                        = new QuerySourceReferenceExpression(joinClause);

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

                    navigationJoins.Add(
                        navigationJoin
                            = new NavigationJoin(
                                querySourceReferenceExpression.ReferencedQuerySource,
                                navigation,
                                joinClause,
                                innerQuerySourceReferenceExpression));
                }

                querySourceReferenceExpression = navigationJoin.QuerySourceReferenceExpression;
                navigationJoins = navigationJoin.NavigationJoins;
            }

            return querySourceReferenceExpression;
        }

        private static Expression CreateKeyAccessExpression(Expression target, IReadOnlyList<IProperty> properties)
        {
            return properties.Count == 1
                ? CreatePropertyExpression(target, properties[0])
                : Expression.New(
                    _compositeKeyCtor,
                    Expression.NewArrayInit(
                        typeof(object),
                        properties
                            .Select(p => Expression.Convert(CreatePropertyExpression(target, p), typeof(object)))
                            .Cast<Expression>()
                            .ToArray()));
        }

        private static readonly MethodInfo PropertyMethodInfo 
            = typeof(EF).GetTypeInfo().GetDeclaredMethod(nameof(Property));

        [CallsMakeGenericMethod(nameof(Property), typeof(TypeArgumentCategory.Properties), TargetType = typeof(EF))]
        [CallsMakeGenericMethod(nameof(Property), typeof(TypeArgumentCategory.NavigationProperties), TargetType = typeof(EF))]
        private static Expression CreatePropertyExpression(Expression target, IProperty property)
            => Expression.Call(
                null,
                PropertyMethodInfo.MakeGenericMethod(property.ClrType),
                target,
                Expression.Constant(property.Name));

        private static readonly ConstructorInfo _compositeKeyCtor
            = typeof(CompositeKey).GetTypeInfo().DeclaredConstructors.Single();

        public static bool IsCompositeKey([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            return type == typeof(CompositeKey);
        }

        private struct CompositeKey
        {
            public static bool operator ==(CompositeKey x, CompositeKey y) => x.Equals(y);
            public static bool operator !=(CompositeKey x, CompositeKey y) => !x.Equals(y);

            private readonly object[] _values;

            [UsedImplicitly]
            public CompositeKey(object[] values)
            {
                _values = values;
            }

            public override bool Equals(object obj)
                => _values.SequenceEqual(((CompositeKey)obj)._values);

            public override int GetHashCode() => 0;
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
        private static EntityQueryable<TResult> _CreateEntityQueryable<TResult>(IAsyncQueryProvider entityQueryProvider)
            => new EntityQueryable<TResult>(entityQueryProvider);

        private class NavigationRewritingQueryModelVisitor : QueryModelVisitorBase
        {
            private readonly NavigationRewritingExpressionVisitor _parentVisitor;
            private readonly SubqueryInjector _subqueryInjector;

            public bool InsideInnerKeySelector { get; private set; }

            public NavigationRewritingQueryModelVisitor(NavigationRewritingExpressionVisitor parentVisitor, EntityQueryModelVisitor queryModelVisitor)
            {
                _parentVisitor = parentVisitor;
                _subqueryInjector = new SubqueryInjector(queryModelVisitor);
            }

            public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
                => fromClause.TransformExpressions(_parentVisitor.Visit);

            public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
                => fromClause.TransformExpressions(_parentVisitor.Visit);

            public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
                => VisitJoinClauseInternal(joinClause);

            public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, GroupJoinClause groupJoinClause)
                => VisitJoinClauseInternal(joinClause);

            private void VisitJoinClauseInternal(JoinClause joinClause)
            {
                joinClause.InnerSequence = _parentVisitor.Visit(joinClause.InnerSequence);
                joinClause.OuterKeySelector = _parentVisitor.Visit(joinClause.OuterKeySelector);

                var oldInsideInnerKeySelector = InsideInnerKeySelector;
                InsideInnerKeySelector = true;
                joinClause.InnerKeySelector = _parentVisitor.Visit(joinClause.InnerKeySelector);
                InsideInnerKeySelector = oldInsideInnerKeySelector;
            }

            public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
                => whereClause.TransformExpressions(_parentVisitor.Visit);

            public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
                => orderByClause.TransformExpressions(_parentVisitor.Visit);

            public override void VisitOrdering(Ordering ordering, QueryModel queryModel, OrderByClause orderByClause, int index)
                => ordering.TransformExpressions(_parentVisitor.Visit);

            public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
            {
                var newSelector = _subqueryInjector.Visit(selectClause.Selector);

                selectClause.Selector = newSelector;

                selectClause.TransformExpressions(_parentVisitor.Visit);
            }

            public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
                => resultOperator.TransformExpressions(_parentVisitor.Visit);

            private class SubqueryInjector : RelinqExpressionVisitor
            {
                private readonly EntityQueryModelVisitor _queryModelVisitor;
                public SubqueryInjector(EntityQueryModelVisitor queryModelVisitor)
                {
                    _queryModelVisitor = queryModelVisitor;
                }

                protected override Expression VisitSubQuery(SubQueryExpression expression)
                {
                    return expression;
                }

                protected override Expression VisitMember(MemberExpression node)
                {
                    Check.NotNull(node, nameof(node));

                    return
                        _queryModelVisitor.BindNavigationPathMemberExpression(
                            node,
                            (properties, querySource) =>
                            {
                                var navigations = properties.OfType<INavigation>().ToList();
                                var collectionNavigation = navigations.Where(n => n.IsCollection()).SingleOrDefault();

                                return collectionNavigation != null
                                    ? InjectSubquery(node, collectionNavigation)
                                    : default(Expression);
                            })
                        ?? base.VisitMember(node);
                }

                private Expression InjectSubquery(Expression expression, INavigation collectionNavigation)
                {
                    var targetType = collectionNavigation.GetTargetType().ClrType;
                    var mainFromClause = new MainFromClause(targetType.Name.Substring(0, 1).ToLower(), targetType, expression);
                    var selecor = new QuerySourceReferenceExpression(mainFromClause);

                    var subqueryModel = new QueryModel(mainFromClause, new SelectClause(selecor));
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

                private static ICollection<TEntity> MaterializeCollectionNavigation<TEntity>(INavigation navigation, IEnumerable<object> elements)
                {
                    var collection = navigation.GetCollectionAccessor().Create(elements);

                    return (ICollection<TEntity>)collection;
                }
            }
        }
    }
}
