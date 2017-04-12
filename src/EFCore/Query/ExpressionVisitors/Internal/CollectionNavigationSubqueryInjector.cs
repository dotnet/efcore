// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CollectionNavigationSubqueryInjector : RelinqExpressionVisitor
    {
        private readonly EntityQueryModelVisitor _queryModelVisitor;
        private bool _shouldInject;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CollectionNavigationSubqueryInjector([NotNull] EntityQueryModelVisitor queryModelVisitor, bool shouldInject = false)
        {
            _queryModelVisitor = queryModelVisitor;
            _shouldInject = shouldInject;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly MethodInfo MaterializeCollectionNavigationMethodInfo
            = typeof(CollectionNavigationSubqueryInjector).GetTypeInfo()
                .GetDeclaredMethod(nameof(MaterializeCollectionNavigation));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));

            var newMemberExpression = default(Expression);
            if (_shouldInject)
            {
                newMemberExpression = _queryModelVisitor.BindNavigationPathPropertyExpression(
                    memberExpression,
                    (properties, querySource) =>
                    {
                        var collectionNavigation = properties.OfType<INavigation>().SingleOrDefault(n => n.IsCollection());

                        return collectionNavigation != null
                                ? InjectSubquery(memberExpression, collectionNavigation)
                                : default(Expression);
                    });
            }

            return newMemberExpression ?? base.VisitMember(memberExpression);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            if (!EntityQueryModelVisitor.IsPropertyMethod(methodCallExpression.Method))
            {
                _shouldInject = true;
            }

            var newMethodCallExpression = default(Expression);
            if (_shouldInject)
            {
                newMethodCallExpression = _queryModelVisitor.BindNavigationPathPropertyExpression(
                    methodCallExpression,
                    (properties, querySource) =>
                    {
                        var collectionNavigation = properties.OfType<INavigation>().SingleOrDefault(n => n.IsCollection());

                        return collectionNavigation != null
                            ? InjectSubquery(methodCallExpression, collectionNavigation)
                            : default(Expression);
                    });
            }

            return newMethodCallExpression ?? base.VisitMethodCall(methodCallExpression);
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
                MaterializeCollectionNavigationMethodInfo.MakeGenericMethod(targetType),
                Expression.Constant(collectionNavigation), subqueryExpression);

            return resultCollectionType.GetTypeInfo().IsGenericType && resultCollectionType.GetGenericTypeDefinition() == typeof(ICollection<>)
                ? (Expression)result
                : Expression.Convert(result, resultCollectionType);
        }

        [UsedImplicitly]
        private static ICollection<TEntity> MaterializeCollectionNavigation<TEntity>(INavigation navigation, IEnumerable<object> elements)
        {
            var collection = navigation.GetCollectionAccessor().Create(elements);

            return (ICollection<TEntity>)collection;
        }
    }
}
