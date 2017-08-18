// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CollectionNavigationIncludeExpressionRewriter : ExpressionVisitorBase
    {
        private readonly EntityQueryModelVisitor _queryModelVisitor;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CollectionNavigationIncludeExpressionRewriter(
            [NotNull] EntityQueryModelVisitor queryModelVisitor)
        {
            _queryModelVisitor = queryModelVisitor;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual List<IQueryAnnotation> CollectionNavigationIncludeResultOperators { get; }
            = new List<IQueryAnnotation>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMember(MemberExpression node)
            => _queryModelVisitor.BindNavigationPathPropertyExpression(
                   node,
                   (ps, qs) => RewritePropertyAccess(node, ps, qs)) ?? base.VisitMember(node);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression node)
            => _queryModelVisitor.BindNavigationPathPropertyExpression(
                   node,
                   (ps, qs) => RewritePropertyAccess(node, ps, qs)) ?? base.VisitMethodCall(node);

        private Expression RewritePropertyAccess(
            Expression expression,
            IReadOnlyList<IPropertyBase> properties,
            IQuerySource querySource)
        {
            if (querySource != null
                && properties.Count > 0
                && properties.All(p => p is INavigation)
                && properties[properties.Count - 1] is INavigation lastNavigation
                && lastNavigation.IsCollection())
            {
                var qsre = new QuerySourceReferenceExpression(querySource);

                CollectionNavigationIncludeResultOperators.Add(
                    new IncludeResultOperator(properties.Cast<INavigation>().ToArray(), qsre));

                var parameter = Expression.Parameter(querySource.ItemType, querySource.ItemName);
                var accessorBody = BuildCollectionAccessorExpression(parameter, properties);
                var emptyCollection = lastNavigation.GetCollectionAccessor().Create();

                return Expression.Call(
                    ProjectCollectionNavigationMethodInfo
                        .MakeGenericMethod(querySource.ItemType, expression.Type),
                    qsre,
                    Expression.Lambda(
                        Expression.Coalesce(
                            accessorBody,
                            Expression.Constant(emptyCollection, accessorBody.Type)),
                        parameter));
            }

            return expression;
        }

        private Expression BuildCollectionAccessorExpression(
            ParameterExpression parameter,
            IEnumerable<IPropertyBase> navigations)
        {
            Expression result = parameter;
            Expression memberExpression = parameter;
            foreach (var navigation in navigations)
            {
                memberExpression = memberExpression.MakeMemberAccess(navigation.PropertyInfo);
                result = new NullConditionalExpression(result, memberExpression);
            }

            return result;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitSubQuery(SubQueryExpression subQueryExpression)
            => subQueryExpression;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly MethodInfo ProjectCollectionNavigationMethodInfo
            = typeof(CollectionNavigationIncludeExpressionRewriter).GetTypeInfo()
                .GetDeclaredMethod(nameof(_ProjectCollectionNavigation));

        // ReSharper disable once InconsistentNaming
        private static TResult _ProjectCollectionNavigation<TEntity, TResult>(TEntity entity, Func<TEntity, TResult> accessor)
            => accessor(entity);
    }
}
