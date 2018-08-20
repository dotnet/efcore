// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
        private readonly IModel _model;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityEqualityRewritingExpressionVisitor(
            [NotNull] QueryCompilationContext queryCompilationContext)
        {
            _queryCompilationContext = queryCompilationContext;
            _model = _queryCompilationContext.Model;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            var newBinaryExpression = (BinaryExpression)base.VisitBinary(binaryExpression);

            if (newBinaryExpression.NodeType == ExpressionType.Equal
                || newBinaryExpression.NodeType == ExpressionType.NotEqual)
            {
                var newLeft = newBinaryExpression.Left;
                var newRight = newBinaryExpression.Right;

                var isLeftNullConstant = newLeft.IsNullConstantExpression();
                var isRightNullConstant = newRight.IsNullConstantExpression();

                if (isLeftNullConstant && isRightNullConstant)
                {
                    return newBinaryExpression;
                }

                var result = isLeftNullConstant || isRightNullConstant
                    ? RewriteNullEquality(newBinaryExpression.NodeType, isLeftNullConstant ? newRight : newLeft)
                    : RewriteEntityEquality(newBinaryExpression.NodeType, newLeft, newRight);

                if (result != null)
                {
                    return result;
                }
            }

            return newBinaryExpression;
        }

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
                var isLeftNullConstant = newLeftExpression.IsNullConstantExpression();
                var isRightNullConstant = newRightExpression.IsNullConstantExpression();

                if (isLeftNullConstant && isRightNullConstant)
                {
                    return newMethodCallExpression;
                }

                if (isLeftNullConstant || isRightNullConstant)
                {
                    var result = RewriteNullEquality(
                        ExpressionType.Equal, isLeftNullConstant ? newRightExpression : newLeftExpression);
                    if (result != null)
                    {
                        return result;
                    }
                }

                var leftEntityType = _model.FindEntityType(newLeftExpression.Type)
                    ?? MemberAccessBindingExpressionVisitor.GetEntityType(newLeftExpression, _queryCompilationContext);
                var rightEntityType = _model.FindEntityType(newRightExpression.Type)
                    ?? MemberAccessBindingExpressionVisitor.GetEntityType(newRightExpression, _queryCompilationContext);
                if (leftEntityType != null && rightEntityType != null)
                {
                    if (leftEntityType.RootType() == rightEntityType.RootType())
                    {
                        var result = RewriteEntityEquality(ExpressionType.Equal, newLeftExpression, newRightExpression);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                    else
                    {
                        return Expression.Constant(false);
                    }
                }
            }

            return newMethodCallExpression;
        }

        private static readonly MethodInfo _objectEqualsMethodInfo
            = typeof(object).GetRuntimeMethod(nameof(object.Equals), new[] { typeof(object), typeof(object) });

        private Expression RewriteNullEquality(ExpressionType nodeType, Expression nonNullExpression)
        {
            var properties = MemberAccessBindingExpressionVisitor
                .GetPropertyPath(nonNullExpression, _queryCompilationContext, out var qsre);

            if (properties.Count > 0
                && properties[properties.Count - 1] is INavigation lastNavigation
                && lastNavigation.IsCollection())
            {
                // collection navigation is only null if its parent entity is null (null propagation thru navigation)
                // it is probable that user wanted to see if the collection is (not) empty
                // log warning suggesting to use Any() instead.
                _queryCompilationContext.Logger
                    .PossibleUnintendedCollectionNavigationNullComparisonWarning(properties);

                return Visit(Expression.MakeBinary(
                    nodeType,
                    CreateNavigationCaller(nonNullExpression, qsre, properties),
                    Expression.Constant(null)));
            }

            if (IsInvalidSubQueryExpression(nonNullExpression))
            {
                return null;
            }

            var entityType = _model.FindEntityType(nonNullExpression.Type)
                ?? GetEntityType(properties, qsre);

            if (entityType == null
                || entityType.IsOwned())
            {
                return null;
            }

            var keyProperties = entityType.FindPrimaryKey().Properties;
            var nullCount = keyProperties.Count;

            Expression keyAccessExpression;

            // Skipping composite key with subquery since it requires to copy subquery
            // which would cause same subquery to be visited twice
            if (nullCount > 1
                && nonNullExpression.RemoveConvert() is SubQueryExpression)
            {
                return null;
            }

            if (properties.Count > 0
                && properties[properties.Count - 1] is INavigation navigation2
                && navigation2.IsDependentToPrincipal())
            {
                keyAccessExpression = CreateKeyAccessExpression(
                            CreateNavigationCaller(nonNullExpression, qsre, properties),
                            navigation2.ForeignKey.Properties,
                            nullComparison: false);
                nullCount = navigation2.ForeignKey.Properties.Count;
            }
            else
            {
                keyAccessExpression = CreateKeyAccessExpression(
                    nonNullExpression,
                    keyProperties,
                    nullComparison: true);
            }

            var nullConstantExpression
                = keyAccessExpression.Type == typeof(AnonymousObject)
                    ? Expression.New(
                        AnonymousObject.AnonymousObjectCtor,
                        Expression.NewArrayInit(
                            typeof(object),
                            Enumerable.Repeat(
                                Expression.Constant(null),
                                nullCount)))
                    : (Expression)Expression.Constant(null);

            return Expression.MakeBinary(nodeType, keyAccessExpression, nullConstantExpression);
        }

        private bool IsInvalidSubQueryExpression(Expression expression)
            => expression is SubQueryExpression subQuery
                && _queryCompilationContext.DuplicateQueryModels.Contains(subQuery.QueryModel);

        private Expression RewriteEntityEquality(ExpressionType nodeType, Expression left, Expression right)
        {
            var leftProperties = MemberAccessBindingExpressionVisitor
                .GetPropertyPath(left, _queryCompilationContext, out var leftQsre);

            var rightProperties = MemberAccessBindingExpressionVisitor
                .GetPropertyPath(right, _queryCompilationContext, out var rightQsre);

            // Collection navigations on both sides
            if (leftProperties.Count > 0
                && rightProperties.Count > 0
                && leftProperties[leftProperties.Count - 1] is INavigation leftNavigation
                && rightProperties[rightProperties.Count - 1] is INavigation rightNavigation
                && leftNavigation.IsCollection())
            {
                if (leftNavigation.Equals(rightNavigation))
                {
                    // Log a warning that comparing 2 collections causes reference comparison
                    _queryCompilationContext.Logger.PossibleUnintendedReferenceComparisonWarning(left, right);

                    return Visit(Expression.MakeBinary(
                        nodeType,
                        CreateNavigationCaller(left, leftQsre, leftProperties),
                        CreateNavigationCaller(right, rightQsre, rightProperties)));
                }

                return Expression.Constant(false);
            }

            if (IsInvalidSubQueryExpression(left)
                || IsInvalidSubQueryExpression(right))
            {
                return null;
            }

            var entityType = _model.FindEntityType(left.Type)
                ?? _model.FindEntityType(right.Type)
                ?? GetEntityType(leftProperties, leftQsre)
                ?? GetEntityType(rightProperties, rightQsre);

            if (entityType == null
                || entityType.IsOwned())
            {
                return null;
            }

            var keyProperties = entityType.FindPrimaryKey().Properties;

            // Skipping composite key with subquery since it requires to copy subquery
            // which would cause same subquery to be visited twice
            if (keyProperties.Count > 1
                && (left.RemoveConvert() is SubQueryExpression
                    || right.RemoveConvert() is SubQueryExpression))
            {
                return null;
            }

            return Expression.MakeBinary(
                nodeType,
                CreateKeyAccessExpression(left, keyProperties, nullComparison: false),
                CreateKeyAccessExpression(right, keyProperties, nullComparison: false));
        }

        private IEntityType GetEntityType(
            IReadOnlyList<IPropertyBase> properties, QuerySourceReferenceExpression querySourceReferenceExpression)
        {
            if (properties.Count > 0)
            {
                if (properties[properties.Count - 1] is INavigation navigation)
                {
                    return navigation.GetTargetType();
                }
            }
            else if (querySourceReferenceExpression != null)
            {
                return _queryCompilationContext.FindEntityType(querySourceReferenceExpression.ReferencedQuerySource);
            }

            return null;
        }

        private static Expression CreateNavigationCaller(
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

        private static Expression CreateNavigationCaller(
            Expression expression,
            QuerySourceReferenceExpression qsre,
            IList<IPropertyBase> properties)
            => AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue12738", out var isEnabled) && isEnabled
                ? CreateNavigationCaller(qsre, properties)
                : (expression as MemberExpression)?.Expression
                    ?? (expression is MethodCallExpression methodCallExpression
                        && methodCallExpression.Method.IsEFPropertyMethod()
                        ? methodCallExpression.Arguments[0]
                        : null);

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
