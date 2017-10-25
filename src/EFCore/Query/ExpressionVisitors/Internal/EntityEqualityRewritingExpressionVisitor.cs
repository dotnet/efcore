// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EntityEqualityRewritingExpressionVisitor : ExpressionVisitorBase
    {
        private readonly QueryCompilationContext _queryCompilationContext;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityEqualityRewritingExpressionVisitor(
            [NotNull] QueryCompilationContext queryCompilationContext)
        {
            _queryCompilationContext = queryCompilationContext;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            var newBinaryExpression = (BinaryExpression)base.VisitBinary(binaryExpression);

            if (binaryExpression.NodeType == ExpressionType.Equal
                || binaryExpression.NodeType == ExpressionType.NotEqual)
            {
                var isLeftNullConstant = newBinaryExpression.Left.IsNullConstantExpression();
                var isRightNullConstant = newBinaryExpression.Right.IsNullConstantExpression();

                if (isLeftNullConstant && isRightNullConstant)
                {
                    return newBinaryExpression;
                }

                var isNullComparison = isLeftNullConstant || isRightNullConstant;
                var nonNullExpression = isLeftNullConstant ? newBinaryExpression.Right : newBinaryExpression.Left;

                var qsre = nonNullExpression as QuerySourceReferenceExpression;

                var leftProperties = MemberAccessBindingExpressionVisitor.GetPropertyPath(
                    newBinaryExpression.Left, _queryCompilationContext, out var leftNavigationQsre);

                var rightProperties = MemberAccessBindingExpressionVisitor.GetPropertyPath(
                    newBinaryExpression.Right, _queryCompilationContext, out var rightNavigationQsre);

                if (isNullComparison)
                {
                    var nonNullNavigationQsre = isLeftNullConstant ? rightNavigationQsre : leftNavigationQsre;
                    var nonNullproperties = isLeftNullConstant ? rightProperties : leftProperties;

                    if (IsCollectionNavigation(nonNullNavigationQsre, nonNullproperties))
                    {
                        // collection navigation is only null if its parent entity is null (null propagation thru navigation)
                        // it is probable that user wanted to see if the collection is (not) empty, log warning suggesting to use Any() instead.
                        _queryCompilationContext.Logger
                            .PossibleUnintendedCollectionNavigationNullComparisonWarning(nonNullproperties);

                        var callerExpression = CreateCollectionCallerExpression(nonNullNavigationQsre, nonNullproperties);

                        return Visit(Expression.MakeBinary(newBinaryExpression.NodeType, callerExpression, Expression.Constant(null)));
                    }
                }

                var collectionNavigationComparison = TryRewriteCollectionNavigationComparison(
                    newBinaryExpression.Left,
                    newBinaryExpression.Right,
                    newBinaryExpression.NodeType,
                    leftNavigationQsre,
                    rightNavigationQsre,
                    leftProperties,
                    rightProperties);

                if (collectionNavigationComparison != null)
                {
                    return collectionNavigationComparison;
                }

                // If a reference navigation is being compared to null then don't rewrite
                if (isNullComparison
                    && qsre == null)
                {
                    return newBinaryExpression;
                }

                var entityType = _queryCompilationContext.Model.FindEntityType(nonNullExpression.Type);
                if (entityType == null)
                {
                    if (qsre != null)
                    {
                        entityType = _queryCompilationContext.FindEntityType(qsre.ReferencedQuerySource);
                    }
                    else
                    {
                        var properties = MemberAccessBindingExpressionVisitor.GetPropertyPath(
                            nonNullExpression, _queryCompilationContext, out qsre);
                        if (properties.Count > 0
                            && properties[properties.Count - 1] is INavigation navigation)
                        {
                            entityType = navigation.GetTargetType();
                        }
                    }
                }

                if (entityType != null)
                {
                    return CreateKeyComparison(
                        entityType,
                        newBinaryExpression.Left,
                        newBinaryExpression.Right,
                        newBinaryExpression.NodeType,
                        isLeftNullConstant,
                        isRightNullConstant,
                        isNullComparison);
                }
            }

            return newBinaryExpression;
        }

        private static readonly MethodInfo _objectEqualsMethodInfo
            = typeof(object).GetRuntimeMethod(nameof(object.Equals), new[] { typeof(object), typeof(object) });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var newMethodCallExpression = (MethodCallExpression)base.VisitMethodCall(methodCallExpression);

            Expression newLeftExpression = null;
            Expression newRightExpression = null;

            if (newMethodCallExpression.Method.Name == nameof(object.Equals)
                && newMethodCallExpression.Object != null
                && newMethodCallExpression.Arguments.Count == 1)
            {
                newLeftExpression = newMethodCallExpression.Object;
                newRightExpression = newMethodCallExpression.Arguments[0];
            }

            if (newMethodCallExpression.Method.Equals(_objectEqualsMethodInfo))
            {
                newLeftExpression = newMethodCallExpression.Arguments[0];
                newRightExpression = newMethodCallExpression.Arguments[1];
            }

            if (newLeftExpression != null
                && newRightExpression != null)
            {
                var leftRootEntityType = _queryCompilationContext.Model.FindEntityType(newLeftExpression.Type)?.RootType();
                var rightRootEntityType = _queryCompilationContext.Model.FindEntityType(newRightExpression.Type)?.RootType();
                if (leftRootEntityType != null
                    && leftRootEntityType == rightRootEntityType)
                {
                    return Visit(Expression.Equal(newLeftExpression, newRightExpression));
                }

                var isLeftNullConstant = newLeftExpression.IsNullConstantExpression();
                var isRightNullConstant = newRightExpression.IsNullConstantExpression();

                var leftProperties = MemberAccessBindingExpressionVisitor.GetPropertyPath(
                    newLeftExpression, _queryCompilationContext, out var leftNavigationQsre);

                var rightProperties = MemberAccessBindingExpressionVisitor.GetPropertyPath(
                    newRightExpression, _queryCompilationContext, out var rightNavigationQsre);

                if ((isLeftNullConstant || IsCollectionNavigation(leftNavigationQsre, leftProperties))
                    && (isRightNullConstant || IsCollectionNavigation(rightNavigationQsre, rightProperties)))
                {
                    return Visit(Expression.Equal(newLeftExpression, newRightExpression));
                }
            }

            return newMethodCallExpression;
        }

        private Expression TryRewriteCollectionNavigationComparison(
            Expression leftExpression,
            Expression rightExpression,
            ExpressionType expressionType,
            QuerySourceReferenceExpression leftNavigationQsre,
            QuerySourceReferenceExpression rightNavigationQsre,
            IList<IPropertyBase> leftProperties,
            IList<IPropertyBase> rightProperties)
        {
            // if both collections are the same navigations, compare their parent entities (by key)
            // otherwise we assume they are different references and return false
            if (IsCollectionNavigation(leftNavigationQsre, leftProperties)
                && IsCollectionNavigation(rightNavigationQsre, rightProperties))
            {
                _queryCompilationContext.Logger.PossibleUnintendedReferenceComparisonWarning(leftExpression, rightExpression);

                if (leftProperties[leftProperties.Count - 1].Equals(rightProperties[rightProperties.Count - 1]))
                {
                    var newLeft = CreateCollectionCallerExpression(leftNavigationQsre, leftProperties);
                    var newRight = CreateCollectionCallerExpression(rightNavigationQsre, rightProperties);

                    return CreateKeyComparison(
                        ((INavigation)leftProperties[leftProperties.Count - 1]).DeclaringEntityType,
                        newLeft,
                        newRight,
                        expressionType,
                        isLeftNullConstant: false,
                        isRightNullConstant: false,
                        isNullComparison: false);
                }

                return Expression.Constant(false);
            }

            return null;
        }

        private static Expression CreateCollectionCallerExpression(
            QuerySourceReferenceExpression qsre,
            IList<IPropertyBase> properties)
        {
            Expression result = qsre;
            for (var i = 0; i < properties.Count - 1; i++)
            {
                result = result.CreateEFPropertyExpression(properties[i], makeNullable: false);
            }

            return result;
        }

        private static bool IsCollectionNavigation(QuerySourceReferenceExpression qsre, IList<IPropertyBase> properties)
            => qsre != null
               && properties.Count > 0
               && properties[properties.Count - 1] is INavigation navigation
               && navigation.IsCollection();

        private static Expression CreateKeyComparison(
            IEntityType entityType,
            Expression left,
            Expression right,
            ExpressionType nodeType,
            bool isLeftNullConstant,
            bool isRightNullConstant,
            bool isNullComparison)
        {
            var primaryKeyProperties = entityType.FindPrimaryKey().Properties;

            var newLeftExpression = isLeftNullConstant
                ? Expression.Constant(null, typeof(object))
                : CreateKeyAccessExpression(left, primaryKeyProperties, isNullComparison);

            var newRightExpression = isRightNullConstant
                ? Expression.Constant(null, typeof(object))
                : CreateKeyAccessExpression(right, primaryKeyProperties, isNullComparison);

            return Expression.MakeBinary(nodeType, newLeftExpression, newRightExpression);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
            if (conditionalExpression.Test is BinaryExpression binaryExpression)
            {
                // Converts '[q] != null ? [q] : [s]' into '[q] ?? [s]'

                if (binaryExpression.NodeType == ExpressionType.NotEqual
                    && binaryExpression.Left is QuerySourceReferenceExpression querySourceReferenceExpression1
                    && binaryExpression.Right.IsNullConstantExpression()
                    && ReferenceEquals(conditionalExpression.IfTrue, querySourceReferenceExpression1))
                {
                    return Expression.Coalesce(conditionalExpression.IfTrue, conditionalExpression.IfFalse);
                }

                // Converts 'null != [q] ? [q] : [s]' into '[q] ?? [s]'

                if (binaryExpression.NodeType == ExpressionType.NotEqual
                    && binaryExpression.Right is QuerySourceReferenceExpression querySourceReferenceExpression2
                    && binaryExpression.Left.IsNullConstantExpression()
                    && ReferenceEquals(conditionalExpression.IfTrue, querySourceReferenceExpression2))
                {
                    return Expression.Coalesce(conditionalExpression.IfTrue, conditionalExpression.IfFalse);
                }

                // Converts '[q] == null ? [s] : [q]' into '[s] ?? [q]'

                if (binaryExpression.NodeType == ExpressionType.Equal
                    && binaryExpression.Left is QuerySourceReferenceExpression querySourceReferenceExpression3
                    && binaryExpression.Right.IsNullConstantExpression()
                    && ReferenceEquals(conditionalExpression.IfFalse, querySourceReferenceExpression3))
                {
                    return Expression.Coalesce(conditionalExpression.IfTrue, conditionalExpression.IfFalse);
                }

                // Converts 'null == [q] ? [s] : [q]' into '[s] ?? [q]'

                if (binaryExpression.NodeType == ExpressionType.Equal
                    && binaryExpression.Right is QuerySourceReferenceExpression querySourceReferenceExpression4
                    && binaryExpression.Left.IsNullConstantExpression()
                    && ReferenceEquals(conditionalExpression.IfFalse, querySourceReferenceExpression4))
                {
                    return Expression.Coalesce(conditionalExpression.IfTrue, conditionalExpression.IfFalse);
                }
            }

            return base.VisitConditional(conditionalExpression);
        }

        private static Expression CreateKeyAccessExpression(
            Expression target,
            IReadOnlyList<IProperty> properties,
            bool nullComparison)
        {
            // If comparing with null then we need only first PK property
            return properties.Count == 1 || nullComparison
                ? target.CreateEFPropertyExpression(properties[0])
                : target.CreateKeyAccessExpression(properties);
        }
    }
}
