// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion.Visitors
{
    public partial class NavigationExpandingVisitor : ExpressionVisitor
    {
        private readonly QueryCompilationContext _queryCompilationContext;

        public NavigationExpandingVisitor([NotNull] QueryCompilationContext queryCompilationContext)
        {
            _queryCompilationContext = queryCompilationContext;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case NavigationBindingExpression _:
                case CustomRootExpression _:
                case NavigationExpansionRootExpression _:
                case NavigationExpansionExpression _:
                case IncludeExpression _:
                    return extensionExpression;
            }

            return base.VisitExtension(extensionExpression);
        }

        private Expression ProcessMemberPushdown(
            Expression source,
            NavigationExpansionExpression navigationExpansionExpression,
            bool efProperty,
            MemberInfo memberInfo,
            string propertyName,
            Type resultType)
        {
            // in case of nested FirstOrDefaults, we need to dig into the inner most - that's where the member finally gets pushed down to
            if (navigationExpansionExpression.State.PendingSelector.Body is NavigationExpansionExpression navigationExpansionPendingSelector
                && navigationExpansionPendingSelector.State.PendingCardinalityReducingOperator != null)
            {
                var newPendingSelector = (NavigationExpansionExpression)ProcessMemberPushdown(source, navigationExpansionPendingSelector, efProperty, memberInfo, propertyName, resultType);

                var newStateNested = new NavigationExpansionExpressionState(
                    navigationExpansionExpression.State.CurrentParameter,
                    navigationExpansionExpression.State.SourceMappings,
                    Expression.Lambda(newPendingSelector, navigationExpansionExpression.State.CurrentParameter),
                    applyPendingSelector: true,
                    navigationExpansionExpression.State.PendingOrderings,
                    navigationExpansionExpression.State.PendingIncludeChain,
                    // we need to remap cardinality reducing operator since it's source type has now changed
                    navigationExpansionExpression.State.PendingCardinalityReducingOperator.GetGenericMethodDefinition().MakeGenericMethod(newPendingSelector.Type),
                    navigationExpansionExpression.State.CustomRootMappings,
                    navigationExpansionExpression.State.MaterializeCollectionNavigation);

                return new NavigationExpansionExpression(
                    navigationExpansionExpression.Operand,
                    newStateNested,
                    resultType);
            }

            var selectorParameter = Expression.Parameter(source.Type, navigationExpansionExpression.State.CurrentParameter.Name);

            var selectorBody = efProperty
                ? (Expression)Expression.Call(EF.PropertyMethod.MakeGenericMethod(resultType),
                    selectorParameter,
                    Expression.Constant(propertyName))
                : Expression.MakeMemberAccess(selectorParameter, memberInfo);

            if (navigationExpansionExpression.State.PendingCardinalityReducingOperator.MethodIsClosedFormOf(LinqMethodHelpers.QueryableFirstOrDefaultMethodInfo)
                || navigationExpansionExpression.State.PendingCardinalityReducingOperator.MethodIsClosedFormOf(LinqMethodHelpers.QueryableFirstOrDefaultPredicateMethodInfo)
                || navigationExpansionExpression.State.PendingCardinalityReducingOperator.MethodIsClosedFormOf(LinqMethodHelpers.QueryableSingleOrDefaultMethodInfo)
                || navigationExpansionExpression.State.PendingCardinalityReducingOperator.MethodIsClosedFormOf(LinqMethodHelpers.QueryableSingleOrDefaultPredicateMethodInfo)
                || navigationExpansionExpression.State.PendingCardinalityReducingOperator.MethodIsClosedFormOf(LinqMethodHelpers.EnumerableFirstOrDefaultMethodInfo)
                || navigationExpansionExpression.State.PendingCardinalityReducingOperator.MethodIsClosedFormOf(LinqMethodHelpers.EnumerableFirstOrDefaultPredicateMethodInfo)
                || navigationExpansionExpression.State.PendingCardinalityReducingOperator.MethodIsClosedFormOf(LinqMethodHelpers.EnumerableSingleOrDefaultMethodInfo)
                || navigationExpansionExpression.State.PendingCardinalityReducingOperator.MethodIsClosedFormOf(LinqMethodHelpers.EnumerableSingleOrDefaultPredicateMethodInfo))
            {
                if (!selectorBody.Type.IsNullableType())
                {
                    selectorBody = Expression.Convert(selectorBody, selectorBody.Type.MakeNullable());
                }
            }

            var selector = Expression.Lambda(selectorBody, selectorParameter);
            var remappedSelectorBody = ReplacingExpressionVisitor.Replace(
                selectorParameter, navigationExpansionExpression.State.PendingSelector.Body, selector.Body);

            var binder = new NavigationPropertyBindingVisitor(
                navigationExpansionExpression.State.CurrentParameter,
                navigationExpansionExpression.State.SourceMappings);

            var boundSelectorBody = binder.Visit(remappedSelectorBody);
            if (boundSelectorBody is NavigationBindingExpression navigationBindingExpression
                && navigationBindingExpression.NavigationTreeNode.Navigation != null)
            {
                if (navigationBindingExpression.NavigationTreeNode.IsCollection)
                {
                    var lastNavigation = navigationBindingExpression.NavigationTreeNode.Navigation;
                    var collectionNavigationElementType = lastNavigation.ForeignKey.DeclaringEntityType.ClrType;
                    var entityQueryable = NullAsyncQueryProvider.Instance.CreateEntityQueryableExpression(collectionNavigationElementType);
                    var outerParameter = Expression.Parameter(collectionNavigationElementType, collectionNavigationElementType.GenerateParameterName());

                    var outerKeyAccess = NavigationExpansionHelpers.CreateKeyAccessExpression(
                        outerParameter,
                        lastNavigation.ForeignKey.Properties);

                    var innerParameter = Expression.Parameter(navigationExpansionExpression.Type);
                    var innerKeyAccessLambda = Expression.Lambda(
                        NavigationExpansionHelpers.CreateKeyAccessExpression(
                            innerParameter,
                            lastNavigation.ForeignKey.PrincipalKey.Properties),
                        innerParameter);

                    var combinedKeySelectorBody = ReplacingExpressionVisitor.Replace(
                        innerKeyAccessLambda.Parameters[0], navigationExpansionExpression.State.PendingSelector.Body, innerKeyAccessLambda.Body);
                    if (outerKeyAccess.Type != combinedKeySelectorBody.Type)
                    {
                        if (combinedKeySelectorBody.Type.IsNullableType())
                        {
                            outerKeyAccess = Expression.Convert(outerKeyAccess, combinedKeySelectorBody.Type);
                        }
                        else
                        {
                            combinedKeySelectorBody = Expression.Convert(combinedKeySelectorBody, outerKeyAccess.Type);
                        }
                    }

                    var rewrittenState = new NavigationExpansionExpressionState(
                        navigationExpansionExpression.State.CurrentParameter,
                        navigationExpansionExpression.State.SourceMappings,
                        Expression.Lambda(combinedKeySelectorBody, navigationExpansionExpression.State.CurrentParameter),
                        applyPendingSelector: true,
                        navigationExpansionExpression.State.PendingOrderings,
                        navigationExpansionExpression.State.PendingIncludeChain,
                        // we need to remap cardinality reducing operator since it's source type has now changed
                        navigationExpansionExpression.State.PendingCardinalityReducingOperator.GetGenericMethodDefinition().MakeGenericMethod(combinedKeySelectorBody.Type),
                        navigationExpansionExpression.State.CustomRootMappings,
                        materializeCollectionNavigation: null);

                    var rewrittenNavigationExpansionExpression = new NavigationExpansionExpression(navigationExpansionExpression.Operand, rewrittenState, combinedKeySelectorBody.Type);
                    var inner = new NavigationExpansionReducingVisitor().Visit(rewrittenNavigationExpansionExpression);

                    var predicate = Expression.Lambda(
                        Expression.Equal(outerKeyAccess, inner),
                        outerParameter);

                    var whereMethodInfo = LinqMethodHelpers.QueryableWhereMethodInfo.MakeGenericMethod(collectionNavigationElementType);
                    var rewritten = Expression.Call(
                        whereMethodInfo,
                        entityQueryable,
                        predicate);

                    var entityType = lastNavigation.ForeignKey.DeclaringEntityType;

                    return NavigationExpansionHelpers.CreateNavigationExpansionRoot(rewritten, entityType, materializeCollectionNavigation: null);
                }
                else
                {
                    return ProcessSelectCore(
                        navigationExpansionExpression.Operand,
                        navigationExpansionExpression.State,
                        selector,
                        selectorBody.Type);
                }
            }

            var newState = new NavigationExpansionExpressionState(
                navigationExpansionExpression.State.CurrentParameter,
                navigationExpansionExpression.State.SourceMappings,
                Expression.Lambda(boundSelectorBody, navigationExpansionExpression.State.CurrentParameter),
                applyPendingSelector: true,
                navigationExpansionExpression.State.PendingOrderings,
                navigationExpansionExpression.State.PendingIncludeChain,
                // we need to remap cardinality reducing operator since it's source type has now changed
                navigationExpansionExpression.State.PendingCardinalityReducingOperator.GetGenericMethodDefinition().MakeGenericMethod(boundSelectorBody.Type),
                navigationExpansionExpression.State.CustomRootMappings,
                navigationExpansionExpression.State.MaterializeCollectionNavigation);

            // TODO: expand navigations

            var result = new NavigationExpansionExpression(
                navigationExpansionExpression.Operand,
                newState,
                selectorBody.Type);

            return resultType != result.Type
                ? (Expression)Expression.Convert(result, resultType)
                : result;
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var newExpression = Visit(memberExpression.Expression);

            return newExpression is NavigationExpansionExpression navigationExpansionExpression
                && navigationExpansionExpression.State.PendingCardinalityReducingOperator != null
                ? ProcessMemberPushdown(newExpression, navigationExpansionExpression, efProperty: false, memberExpression.Member, propertyName: null, memberExpression.Type)
                : memberExpression.Update(newExpression);
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            var leftConstantNull = binaryExpression.Left.IsNullConstantExpression();
            var rightConstantNull = binaryExpression.Right.IsNullConstantExpression();

            // collection comparison must be optimized out before we visit the left and right
            // otherwise collections would be rewritten and harder to identify
            if (binaryExpression.NodeType == ExpressionType.Equal
                || binaryExpression.NodeType == ExpressionType.NotEqual)
            {
                var leftParent = default(Expression);
                var leftNavigation = default(INavigation);
                var rightParent = default(Expression);
                var rightNavigation = default(INavigation);

                // TODO: this isn't robust and won't work for weak entity types
                // also, add support for EF.Property and maybe convert node around the navigation
                if (binaryExpression.Left is MemberExpression leftMember
                    && leftMember.Type.TryGetSequenceType() is Type leftSequenceType
                    && leftSequenceType != null
                    && _queryCompilationContext.Model.FindEntityType(leftMember.Expression.Type) is IEntityType leftParentEntityType)
                {
                    leftNavigation = leftParentEntityType.FindNavigation(leftMember.Member.Name);
                    if (leftNavigation != null)
                    {
                        leftParent = leftMember.Expression;
                    }
                }

                if (binaryExpression.Right is MemberExpression rightMember
                    && rightMember.Type.TryGetSequenceType() is Type rightSequenceType
                    && rightSequenceType != null
                    && _queryCompilationContext.Model.FindEntityType(rightMember.Expression.Type) is IEntityType rightParentEntityType)
                {
                    rightNavigation = rightParentEntityType.FindNavigation(rightMember.Member.Name);
                    if (rightNavigation != null)
                    {
                        rightParent = rightMember.Expression;
                    }
                }

                if (leftNavigation != null
                    && leftNavigation.IsCollection()
                    && leftNavigation == rightNavigation)
                {
                    var rewritten = Expression.MakeBinary(binaryExpression.NodeType, leftParent, rightParent);

                    return Visit(rewritten);
                }

                if (leftNavigation != null
                    && leftNavigation.IsCollection()
                    && rightConstantNull)
                {
                    var rewritten = Expression.MakeBinary(binaryExpression.NodeType, leftParent, Expression.Constant(null));

                    return Visit(rewritten);
                }

                if (rightNavigation != null
                    && rightNavigation.IsCollection()
                    && leftConstantNull)
                {
                    var rewritten = Expression.MakeBinary(binaryExpression.NodeType, Expression.Constant(null), rightParent);

                    return Visit(rewritten);
                }
            }

            var newLeft = Visit(binaryExpression.Left);
            var newRight = Visit(binaryExpression.Right);

            if (binaryExpression.NodeType == ExpressionType.Equal
                || binaryExpression.NodeType == ExpressionType.NotEqual)
            {
                var leftNavigationExpansionExpression = newLeft as NavigationExpansionExpression;
                var rightNavigationExpansionExpression = newRight as NavigationExpansionExpression;
                var leftNavigationBindingExpression = default(NavigationBindingExpression);
                var rightNavigationBindingExpression = default(NavigationBindingExpression);

                if (leftNavigationExpansionExpression?.State.PendingCardinalityReducingOperator != null)
                {
                    leftNavigationBindingExpression = leftNavigationExpansionExpression.State.PendingSelector.Body as NavigationBindingExpression;
                }

                if (rightNavigationExpansionExpression?.State.PendingCardinalityReducingOperator != null)
                {
                    rightNavigationBindingExpression = rightNavigationExpansionExpression.State.PendingSelector.Body as NavigationBindingExpression;
                }

                if (leftNavigationBindingExpression != null
                    && rightConstantNull)
                {
                    (newLeft, newRight) = CreateNullComparisonArguments(leftNavigationBindingExpression, leftNavigationExpansionExpression);
                }

                if (rightNavigationBindingExpression != null
                    && leftConstantNull)
                {
                    (newRight, newLeft) = CreateNullComparisonArguments(rightNavigationBindingExpression, rightNavigationExpansionExpression);
                }

                var result = binaryExpression.NodeType == ExpressionType.Equal
                    ? Expression.Equal(newLeft, newRight)
                    : Expression.NotEqual(newLeft, newRight);

                return result;
            }

            return binaryExpression.Update(newLeft, binaryExpression.Conversion, newRight);
        }

        private (NavigationExpansionExpression navigationExpression, Expression nullKeyExpression) CreateNullComparisonArguments(
            NavigationBindingExpression navigationBindingExpression,
            NavigationExpansionExpression navigationExpansionExpression)
        {
            var navigationKeyAccessExpression = NavigationExpansionHelpers.CreateKeyAccessExpression(
                navigationBindingExpression,
                new[] { navigationBindingExpression.EntityType.FindPrimaryKey().Properties.First() },
                addNullCheck: true);

            var nullKeyExpression = NavigationExpansionHelpers.CreateNullKeyExpression(
                navigationKeyAccessExpression.Type,
                keyCount: 1);

            var newNavigationExpansionExpressionState = new NavigationExpansionExpressionState(
                navigationExpansionExpression.State.CurrentParameter,
                navigationExpansionExpression.State.SourceMappings,
                Expression.Lambda(navigationKeyAccessExpression, navigationExpansionExpression.State.PendingSelector.Parameters[0]),
                applyPendingSelector: true,
                navigationExpansionExpression.State.PendingOrderings,
                navigationExpansionExpression.State.PendingIncludeChain,
                // we need to remap cardinality reducing operator since it's source type has now changed
                navigationExpansionExpression.State.PendingCardinalityReducingOperator?.GetGenericMethodDefinition().MakeGenericMethod(navigationKeyAccessExpression.Type),
                navigationExpansionExpression.State.CustomRootMappings,
                navigationExpansionExpression.State.MaterializeCollectionNavigation);

            var navigationExpression = new NavigationExpansionExpression(
                navigationExpansionExpression.Operand,
                newNavigationExpansionExpressionState,
                navigationKeyAccessExpression.Type);

            return (navigationExpression, nullKeyExpression);
        }
    }
}
