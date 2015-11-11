// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors.Internal
{
    public class NavigationRewritingExpressionVisitor : RelinqExpressionVisitor
    {
        private readonly EntityQueryModelVisitor _queryModelVisitor;
        private readonly List<NavigationJoin> _navigationJoins = new List<NavigationJoin>();
        private readonly NavigationRewritingQueryModelVisitor _navigationRewritingQueryModelVisitor;

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
            _navigationRewritingQueryModelVisitor = new NavigationRewritingQueryModelVisitor(this);
        }

        private NavigationRewritingExpressionVisitor(
            EntityQueryModelVisitor queryModelVisitor, IAsyncQueryProvider entityQueryProvider)
            : this(queryModelVisitor)
        {
            _entityQueryProvider = entityQueryProvider;
        }

        public virtual void Rewrite([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            _queryModel = queryModel;

            _navigationRewritingQueryModelVisitor.VisitQueryModel(_queryModel);

            var insertionIndex = 0;

            foreach (var navigationJoin in _navigationJoins)
            {
                var bodyClause = navigationJoin.QuerySource as IBodyClause;

                if (bodyClause != null)
                {
                    insertionIndex = queryModel.BodyClauses.IndexOf(bodyClause) + 1;
                }

                var i = insertionIndex;

                foreach (var nj in navigationJoin.Iterate())
                {
                    queryModel.BodyClauses.Insert(i++, nj.JoinClause);
                }
            }
        }

        protected override Expression VisitSubQuery(SubQueryExpression subQueryExpression)
        {
            var navigationRewritingExpressionVisitor = CreateVisitorForSubQuery();

            navigationRewritingExpressionVisitor.Rewrite(subQueryExpression.QueryModel);

            return subQueryExpression;
        }

        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            if (_entityQueryProvider == null)
            {
                _entityQueryProvider
                    = (constantExpression.Value as IQueryable)?.Provider as IAsyncQueryProvider;
            }

            return constantExpression;
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            var newLeft = Visit(binaryExpression.Left);
            var newRight = Visit(binaryExpression.Right);

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
                binaryExpression.NodeType,
                newLeft,
                newRight,
                binaryExpression.IsLiftedToNull,
                binaryExpression.Method);
        }

        private static NewExpression CreateNullCompositeKey(Expression otherExpression)
            => Expression.New(
                _compositeKeyCtor,
                Expression.NewArrayInit(
                    typeof(object),
                    Enumerable.Repeat(
                        Expression.Constant(null),
                        ((NewArrayExpression)((NewExpression)otherExpression).Arguments.Single()).Expressions.Count)));

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));

            return
                _queryModelVisitor.BindNavigationPathMemberExpression(
                    memberExpression,
                    (ps, qs) =>
                        {
                            var properties = ps.ToList();
                            var navigations = properties.OfType<INavigation>().ToList();

                            if (navigations.Any())
                            {
                                if ((navigations.Count == 1)
                                    && navigations[0].IsDependentToPrincipal())
                                {
                                    var foreignKeyMemberAccess = CreateForeignKeyMemberAccess(memberExpression, navigations[0]);
                                    if (foreignKeyMemberAccess != null)
                                    {
                                        return foreignKeyMemberAccess;
                                    }
                                }

                                var outerQuerySourceReferenceExpression = new QuerySourceReferenceExpression(qs);

                                if (_navigationRewritingQueryModelVisitor.InsideInnerKeySelector)
                                {
                                    var translated = CreateSubqueryForNavigations(outerQuerySourceReferenceExpression, navigations, memberExpression);

                                    return translated;
                                }

                                var innerQuerySourceReferenceExpression
                                    = CreateJoinsForNavigations(outerQuerySourceReferenceExpression, navigations);

                                return properties.Count == navigations.Count
                                    ? innerQuerySourceReferenceExpression
                                    : Expression.MakeMemberAccess(innerQuerySourceReferenceExpression, memberExpression.Member);
                            }

                            return default(Expression);
                        })
                ?? base.VisitMember(memberExpression);
        }

        private Expression CreateForeignKeyMemberAccess(MemberExpression memberExpression, INavigation navigation)
        {
            var principalKey = navigation.ForeignKey.PrincipalKey;
            if (principalKey.Properties.Count == 1)
            {
                var principalKeyProperty = principalKey.Properties[0];
                if (principalKeyProperty.Name == memberExpression.Member.Name)
                {
                    Debug.Assert(navigation.ForeignKey.Properties.Count == 1);

                    var declaringExpression = ((MemberExpression)memberExpression.Expression).Expression;
                    var foreignKeyPropertyExpression = CreateKeyAccessExpression(declaringExpression, navigation.ForeignKey.Properties);

                    return foreignKeyPropertyExpression.Type != principalKeyProperty.ClrType
                        ? Expression.Convert(foreignKeyPropertyExpression, principalKeyProperty.ClrType)
                        : foreignKeyPropertyExpression;
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
            => new NavigationRewritingExpressionVisitor(_queryModelVisitor, _entityQueryProvider);

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
                        innerKeySelector
                            = Expression.Convert(
                                innerKeySelector,
                                joinClause.OuterKeySelector.Type);
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

        private static Expression CreatePropertyExpression(Expression target, IProperty property)
            => Expression.Call(
                null,
                EntityQueryModelVisitor.PropertyMethodInfo.MakeGenericMethod(property.ClrType),
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

            public bool InsideInnerKeySelector { get; private set; }

            public NavigationRewritingQueryModelVisitor(NavigationRewritingExpressionVisitor parentVisitor)
            {
                _parentVisitor = parentVisitor;
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

            public override void VisitOrdering(Ordering ordering, QueryModel queryModel, OrderByClause orderByClause, int index)
                => ordering.TransformExpressions(_parentVisitor.Visit);

            public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
                => selectClause.TransformExpressions(_parentVisitor.Visit);
        }
    }
}
