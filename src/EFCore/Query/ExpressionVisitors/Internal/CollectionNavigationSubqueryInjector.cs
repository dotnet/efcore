// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
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

        private static readonly List<string> _collectionMaterializingMethodNames = new List<string>
        {
            nameof(Enumerable.ToArray),
            nameof(Enumerable.ToDictionary),
            nameof(Enumerable.ToList),
            nameof(Enumerable.ToLookup)
        };

        private static readonly List<MethodInfo> _collectionMaterializingMethods
            = typeof(Enumerable).GetRuntimeMethods().Where(m => _collectionMaterializingMethodNames.Contains(m.Name))
                .Concat(typeof(AsyncEnumerable).GetRuntimeMethods().Where(m => _collectionMaterializingMethodNames.Contains(m.Name))).ToList();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CollectionNavigationSubqueryInjector([NotNull] EntityQueryModelVisitor queryModelVisitor, bool shouldInject = false)
        {
            Check.NotNull(queryModelVisitor, nameof(queryModelVisitor));

            _queryModelVisitor = queryModelVisitor;
            ShouldInject = shouldInject;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual bool ShouldInject { get; set; }

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
            if (ShouldInject)
            {
                newMemberExpression = _queryModelVisitor.BindNavigationPathPropertyExpression(
                    memberExpression,
                    (properties, querySource) =>
                        {
                            var collectionNavigation = properties.OfType<INavigation>().SingleOrDefault(n => n.IsCollection());

                            return collectionNavigation != null
                                ? InjectSubquery(memberExpression, collectionNavigation)
                                : default;
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

            if (methodCallExpression.Method.MethodIsClosedFormOf(MaterializeCollectionNavigationMethodInfo)
                || IncludeCompiler.IsIncludeMethod(methodCallExpression))
            {
                return methodCallExpression;
            }

            var shouldInject = ShouldInject;
            if (!methodCallExpression.Method.IsEFPropertyMethod()
                && !_collectionMaterializingMethods.Any(m => methodCallExpression.Method.MethodIsClosedFormOf(m)))
            {
                ShouldInject = true;
            }

            var newMethodCallExpression = default(Expression);
            if (ShouldInject)
            {
                newMethodCallExpression = _queryModelVisitor.BindNavigationPathPropertyExpression(
                    methodCallExpression,
                    (properties, querySource) =>
                        {
                            var collectionNavigation = properties.OfType<INavigation>().SingleOrDefault(n => n.IsCollection());

                            return collectionNavigation != null
                                ? InjectSubquery(methodCallExpression, collectionNavigation)
                                : default;
                        });
            }

            try
            {
                return newMethodCallExpression ?? base.VisitMethodCall(methodCallExpression);
            }
            finally
            {
                ShouldInject = shouldInject;
            }
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
        private static ICollection<TEntity> MaterializeCollectionNavigation<TEntity>(
            INavigation navigation,
            IEnumerable<object> elements)
        {
            var collection = navigation.GetCollectionAccessor().Create(elements);

            return (ICollection<TEntity>)collection;
        }
    }
}
