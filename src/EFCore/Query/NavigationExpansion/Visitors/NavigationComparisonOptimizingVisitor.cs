// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion.Visitors
{
    public class NavigationComparisonOptimizingVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo _objectEqualsMethodInfo
            = typeof(object).GetRuntimeMethod(nameof(object.Equals), new[] { typeof(object), typeof(object) });

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            var newLeft = Visit(binaryExpression.Left);
            var newRight = Visit(binaryExpression.Right);

            if (binaryExpression.NodeType == ExpressionType.Equal
                || binaryExpression.NodeType == ExpressionType.NotEqual)
            {
                var rewritten = TryRewriteNavigationComparison(newLeft, newRight, equality: binaryExpression.NodeType == ExpressionType.Equal);
                if (rewritten != null)
                {
                    return rewritten;
                }
            }

            return binaryExpression.Update(newLeft, binaryExpression.Conversion, newRight);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Expression newLeft;
            Expression newRight;
            if (methodCallExpression.Method.Name == nameof(object.Equals)
                && methodCallExpression.Object != null
                && methodCallExpression.Arguments.Count == 1)
            {
                newLeft = Visit(methodCallExpression.Object);
                newRight = Visit(methodCallExpression.Arguments[0]);

                return TryRewriteNavigationComparison(newLeft, newRight, equality: true)
                    ?? methodCallExpression.Update(newLeft, new[] { newRight });
            }

            if (methodCallExpression.Method.Equals(_objectEqualsMethodInfo))
            {
                newLeft = methodCallExpression.Arguments[0];
                newRight = methodCallExpression.Arguments[1];

                return TryRewriteNavigationComparison(newLeft, newRight, equality: true)
                    ?? methodCallExpression.Update(null, new[] { newLeft, newRight });
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        private Expression TryRewriteNavigationComparison(Expression left, Expression right, bool equality)
        {
            var leftBinding = left as NavigationBindingExpression;
            var rightBinding = right as NavigationBindingExpression;
            var leftNullConstant = left.IsNullConstantExpression();
            var rightNullConstant = right.IsNullConstantExpression();

            Expression newLeft = null;
            Expression newRight = null;

            // comparing two different collection navigations always returns false
            if (leftBinding != null
                && rightBinding != null
                && leftBinding.NavigationTreeNode.Navigation != rightBinding.NavigationTreeNode.Navigation
                && (leftBinding.NavigationTreeNode.Navigation?.IsCollection() == true || rightBinding.NavigationTreeNode.Navigation?.IsCollection() == true))
            {
                if (leftBinding.NavigationTreeNode.Navigation.IsCollection())
                {
                    var parentTreeNode = leftBinding.NavigationTreeNode.Parent;
                    parentTreeNode.Children.Remove(leftBinding.NavigationTreeNode);
                }

                if (rightBinding.NavigationTreeNode.Navigation.IsCollection())
                {
                    var parentTreeNode = rightBinding.NavigationTreeNode.Parent;
                    parentTreeNode.Children.Remove(rightBinding.NavigationTreeNode);
                }

                return Expression.Constant(false);
            }

            if (leftBinding != null && rightBinding != null
                && leftBinding.EntityType == rightBinding.EntityType)
            {
                if (leftBinding.NavigationTreeNode.Navigation == rightBinding.NavigationTreeNode.Navigation
                    && leftBinding.NavigationTreeNode.Navigation?.IsCollection() == true)
                {
                    leftBinding = CreateParentBindingExpression(leftBinding);
                    rightBinding = CreateParentBindingExpression(rightBinding);
                }

                // TODO: what about entities without PKs?
                var primaryKeyProperties = leftBinding.EntityType.FindPrimaryKey().Properties;
                newLeft = NavigationExpansionHelpers.CreateKeyAccessExpression(leftBinding, primaryKeyProperties, addNullCheck: leftBinding.NavigationTreeNode.Optional);
                newRight = NavigationExpansionHelpers.CreateKeyAccessExpression(rightBinding, primaryKeyProperties, addNullCheck: rightBinding.NavigationTreeNode.Optional);
            }

            if (leftBinding != null
                && rightNullConstant)
            {
                if (leftBinding.NavigationTreeNode.Navigation?.IsCollection() == true)
                {
                    leftBinding = CreateParentBindingExpression(leftBinding);
                }

                // TODO: what about entities without PKs?
                var primaryKeyProperties = leftBinding.EntityType.FindPrimaryKey().Properties;
                newLeft = NavigationExpansionHelpers.CreateKeyAccessExpression(leftBinding, primaryKeyProperties, addNullCheck: leftBinding.NavigationTreeNode.Optional);
                newRight = NavigationExpansionHelpers.CreateNullKeyExpression(newLeft.Type, primaryKeyProperties.Count);
            }

            if (rightBinding != null
                && leftNullConstant)
            {
                if (rightBinding.NavigationTreeNode.Navigation?.IsCollection() == true)
                {
                    rightBinding = CreateParentBindingExpression(rightBinding);
                }

                // TODO: what about entities without PKs?
                var primaryKeyProperties = rightBinding.EntityType.FindPrimaryKey().Properties;
                newRight = NavigationExpansionHelpers.CreateKeyAccessExpression(rightBinding, primaryKeyProperties, addNullCheck: rightBinding.NavigationTreeNode.Optional);
                newLeft = NavigationExpansionHelpers.CreateNullKeyExpression(newRight.Type, primaryKeyProperties.Count);
            }

            if (newLeft == null || newRight == null)
            {
                return null;
            }

            if (newLeft.Type != newRight.Type)
            {
                if (newLeft.Type.IsNullableType())
                {
                    newRight = Expression.Convert(newRight, newLeft.Type);
                }
                else
                {
                    newLeft = Expression.Convert(newLeft, newRight.Type);
                }
            }

            return equality
                ? Expression.Equal(newLeft, newRight)
                : Expression.NotEqual(newLeft, newRight);
        }

        private NavigationBindingExpression CreateParentBindingExpression(NavigationBindingExpression navigationBindingExpression)
        {
            // TODO: idk if thats correct
            var parentNavigationEntityType = navigationBindingExpression.NavigationTreeNode.Navigation.FindInverse().GetTargetType();
            var parentTreeNode = navigationBindingExpression.NavigationTreeNode.Parent;
            parentTreeNode.Children.Remove(navigationBindingExpression.NavigationTreeNode);

            return new NavigationBindingExpression(
                navigationBindingExpression.RootParameter,
                parentTreeNode,
                parentNavigationEntityType,
                navigationBindingExpression.SourceMapping,
                parentNavigationEntityType.ClrType);
        }
    }
}
