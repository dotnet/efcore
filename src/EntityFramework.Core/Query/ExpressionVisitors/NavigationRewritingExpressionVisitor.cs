// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class NavigationRewritingExpressionVisitor : RelinqExpressionVisitor
    {
        private readonly EntityQueryModelVisitor _queryModelVisitor;
        private readonly List<NavigationJoin> _navigationJoins = new List<NavigationJoin>();

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

        private IEntityQueryProvider _entityQueryProvider;

        public NavigationRewritingExpressionVisitor([NotNull] EntityQueryModelVisitor queryModelVisitor)
        {
            Check.NotNull(queryModelVisitor, nameof(queryModelVisitor));

            _queryModelVisitor = queryModelVisitor;
        }

        private NavigationRewritingExpressionVisitor(
            EntityQueryModelVisitor queryModelVisitor, IEntityQueryProvider entityQueryProvider)
            : this(queryModelVisitor)
        {
            _entityQueryProvider = entityQueryProvider;
        }

        public virtual void Rewrite([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            _queryModel = queryModel;

            queryModel.TransformExpressions(Visit);

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
            var navigationRewritingExpressionVisitor
                = new NavigationRewritingExpressionVisitor(_queryModelVisitor, _entityQueryProvider);

            navigationRewritingExpressionVisitor.Rewrite(subQueryExpression.QueryModel);

            return subQueryExpression;
        }

        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            if (_entityQueryProvider == null)
            {
                _entityQueryProvider
                    = (constantExpression.Value as IQueryable)?.Provider as IEntityQueryProvider;
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

            if (leftNavigationJoin != null
                && rightNavigationJoin != null)
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

                    if (constantExpression != null
                        && constantExpression.Value == null)
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

                    if (constantExpression != null
                        && constantExpression.Value == null)
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
                                var outerQuerySourceReferenceExpression = new QuerySourceReferenceExpression(qs);

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

                    _queryModel.BodyClauses.Add(
                        new WhereClause(
                            Expression.Equal(
                                CreateKeyAccessExpression(
                                    querySourceReferenceExpression,
                                    navigation.ForeignKey.Properties),
                                CreateKeyAccessExpression(
                                    innerQuerySourceReferenceExpression,
                                    navigation.ForeignKey.PrincipalKey.Properties))));

                    return _queryModel.MainFromClause.FromExpression;
                }

                var navigationJoin
                    = navigationJoins
                        .FirstOrDefault(nj =>
                            nj.QuerySource == querySourceReferenceExpression.ReferencedQuerySource
                            && nj.Navigation == navigation);

                if (navigationJoin == null)
                {
                    var joinClause
                        = new JoinClause(
                            $"{querySourceReferenceExpression.ReferencedQuerySource.ItemName}.{navigation.Name}",
                            targetEntityType.ClrType,
                            CreateEntityQueryable(targetEntityType),
                            CreateKeyAccessExpression(
                                querySourceReferenceExpression,
                                navigation.ForeignKey.Properties),
                            Expression.Constant(null));

                    var innerQuerySourceReferenceExpression
                        = new QuerySourceReferenceExpression(joinClause);

                    var innerKeySelector
                        = CreateKeyAccessExpression(
                            innerQuerySourceReferenceExpression,
                            navigation.ForeignKey.PrincipalKey.Properties);

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
        private static EntityQueryable<TResult> _CreateEntityQueryable<TResult>(IEntityQueryProvider entityQueryProvider)
            => new EntityQueryable<TResult>(entityQueryProvider);
    }
}
