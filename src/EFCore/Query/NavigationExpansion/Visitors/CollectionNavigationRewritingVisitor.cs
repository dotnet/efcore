// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion.Visitors
{
    /// <summary>
    ///     Rewrites collection navigations into subqueries, e.g.:
    ///     customers.Select(c => c.Order.OrderDetails.Where(...)) => customers.Select(c => orderDetails.Where(od => od.OrderId == c.Order.Id).Where(...))
    /// </summary>
    public class CollectionNavigationRewritingVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _sourceParameter;

        public CollectionNavigationRewritingVisitor(ParameterExpression sourceParameter)
        {
            _sourceParameter = sourceParameter;
        }

        protected override Expression VisitLambda<T>(Expression<T> lambdaExpression)
        {
            var newBody = Visit(lambdaExpression.Body);

            return newBody != lambdaExpression.Body
                ? Expression.Lambda(newBody, lambdaExpression.Parameters)
                : lambdaExpression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            // include collections are expanded separately later - during NavigationExpansionExpression.Reduce()
            if (methodCallExpression.IsIncludeMethod())
            {
                return methodCallExpression;
            }

            if (methodCallExpression.Method.MethodIsClosedFormOf(LinqMethodHelpers.QueryableSelectManyMethodInfo))
            {
                return methodCallExpression;
            }

            if (methodCallExpression.Method.MethodIsClosedFormOf(LinqMethodHelpers.QueryableSelectManyWithResultOperatorMethodInfo))
            {
                var newResultSelector = Visit(methodCallExpression.Arguments[2]);

                return newResultSelector != methodCallExpression.Arguments[2]
                    ? methodCallExpression.Update(methodCallExpression.Object, new[] { methodCallExpression.Arguments[0], methodCallExpression.Arguments[1], newResultSelector })
                    : methodCallExpression;
            }

            // collection.Exists(predicate) -> Enumerable.Any(collection, predicate)
            if (methodCallExpression.Method.Name == nameof(List<int>.Exists)
                && methodCallExpression.Method.DeclaringType.IsGenericType
                && methodCallExpression.Method.DeclaringType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var newCaller = RemoveMaterializeCollection(Visit(methodCallExpression.Object));
                var newPredicate = Visit(methodCallExpression.Arguments[0]);

                return Expression.Call(
                    LinqMethodHelpers.EnumerableAnyPredicateMethodInfo.MakeGenericMethod(newCaller.Type.GetSequenceType()),
                    newCaller,
                    Expression.Lambda(
                        ((LambdaExpression)newPredicate).Body,
                        ((LambdaExpression)newPredicate).Parameters[0]));
            }

            // collection.Contains(element) -> Enumerable.Any(collection, c => c == element)
            if (methodCallExpression.Method.Name == nameof(List<int>.Contains)
                && methodCallExpression.Arguments.Count == 1
                && methodCallExpression.Object is NavigationBindingExpression navigationBindingCaller
                && navigationBindingCaller.NavigationTreeNode.IsCollection)
            {
                var newCaller = RemoveMaterializeCollection(Visit(methodCallExpression.Object));
                var newArgument = Visit(methodCallExpression.Arguments[0]);

                var lambdaParameter = Expression.Parameter(newCaller.Type.GetSequenceType(), newCaller.Type.GetSequenceType().GenerateParameterName());
                var lambda = Expression.Lambda(
                    Expression.Equal(lambdaParameter, newArgument),
                    lambdaParameter);

                return Expression.Call(
                    LinqMethodHelpers.EnumerableAnyPredicateMethodInfo.MakeGenericMethod(newCaller.Type.GetSequenceType()),
                    newCaller,
                    lambda);
            }

            var newObject = RemoveMaterializeCollection(Visit(methodCallExpression.Object));
            var newArguments = new List<Expression>();

            var argumentsChanged = false;
            foreach (var argument in methodCallExpression.Arguments)
            {
                var newArgument = RemoveMaterializeCollection(Visit(argument));
                newArguments.Add(newArgument);
                if (newArgument != argument)
                {
                    argumentsChanged = true;
                }
            }

            return newObject != methodCallExpression.Object || argumentsChanged
                ? methodCallExpression.Update(newObject, newArguments)
                : methodCallExpression;
        }

        private Expression RemoveMaterializeCollection(Expression expression)
        {
            if (expression is NavigationExpansionExpression navigationExpansionExpression
                && navigationExpansionExpression.State.MaterializeCollectionNavigation != null)
            {
                navigationExpansionExpression.State.MaterializeCollectionNavigation = null;

                return new NavigationExpansionExpression(
                    navigationExpansionExpression.Operand,
                    navigationExpansionExpression.State,
                    navigationExpansionExpression.Operand.Type);
            }

            if (expression is NavigationExpansionRootExpression navigationExpansionRootExpression
                && navigationExpansionRootExpression.NavigationExpansion.State.MaterializeCollectionNavigation != null)
            {
                navigationExpansionRootExpression.NavigationExpansion.State.MaterializeCollectionNavigation = null;

                var rewritten = new NavigationExpansionExpression(
                    navigationExpansionRootExpression.NavigationExpansion.Operand,
                    navigationExpansionRootExpression.NavigationExpansion.State,
                    navigationExpansionRootExpression.NavigationExpansion.Operand.Type);

                return new NavigationExpansionRootExpression(rewritten, navigationExpansionRootExpression.Mapping);
            }

            return expression;
        }

        public static Expression CreateCollectionNavigationExpression(
            NavigationTreeNode navigationTreeNode, ParameterExpression rootParameter, SourceMapping sourceMapping)
        {
            var collectionEntityType = navigationTreeNode.Navigation.ForeignKey.DeclaringEntityType;

            Expression operand;
            if (navigationTreeNode.IncludeState == NavigationState.Pending
                || navigationTreeNode.ExpansionState == NavigationState.Pending)
            {
                var entityQueryable = (Expression)NullAsyncQueryProvider.Instance.CreateEntityQueryableExpression(collectionEntityType.ClrType);

                var outerBinding = new NavigationBindingExpression(
                    rootParameter,
                    navigationTreeNode.Parent,
                    navigationTreeNode.Navigation.DeclaringEntityType,
                    sourceMapping,
                    navigationTreeNode.Navigation.DeclaringEntityType.ClrType);

                var outerKeyAccess = NavigationExpansionHelpers.CreateKeyAccessExpression(
                    outerBinding,
                    navigationTreeNode.Navigation.ForeignKey.PrincipalKey.Properties,
                    addNullCheck: outerBinding.NavigationTreeNode.Optional);

                var collectionCurrentParameter = Expression.Parameter(
                    collectionEntityType.ClrType, collectionEntityType.ClrType.GenerateParameterName());

                var innerKeyAccess = NavigationExpansionHelpers.CreateKeyAccessExpression(
                    collectionCurrentParameter,
                    navigationTreeNode.Navigation.ForeignKey.Properties);

                var predicate = Expression.Lambda(
                    CreateKeyComparisonExpressionForCollectionNavigationSubquery(
                        outerKeyAccess,
                        innerKeyAccess,
                        outerBinding),
                    collectionCurrentParameter);

                operand = Expression.Call(
                    LinqMethodHelpers.QueryableWhereMethodInfo.MakeGenericMethod(collectionEntityType.ClrType),
                    entityQueryable,
                    predicate);
            }
            else
            {
                operand = new NavigationBindingExpression(
                    rootParameter,
                    navigationTreeNode,
                    collectionEntityType,
                    sourceMapping,
                    collectionEntityType.ClrType);
            }

            var result = NavigationExpansionHelpers.CreateNavigationExpansionRoot(
                operand, collectionEntityType, navigationTreeNode.Navigation);

            // this is needed for cases like: root.Include(r => r.Collection).ThenInclude(c => c.Reference).Select(r => r.Collection)
            // result should be elements of the collection navigation with their 'Reference' included
            var newSourceMapping = result.State.SourceMappings.Single();
            IncludeHelpers.CopyIncludeInformation(navigationTreeNode, newSourceMapping.NavigationTree, newSourceMapping);

            return result;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is NavigationBindingExpression navigationBindingExpression)
            {
                if (navigationBindingExpression.RootParameter == _sourceParameter
                    && navigationBindingExpression.NavigationTreeNode.Parent != null
                    && navigationBindingExpression.NavigationTreeNode.IsCollection)
                {
                    var lastNavigation = navigationBindingExpression.NavigationTreeNode.Navigation;
                    return lastNavigation.ForeignKey.IsOwnership
                        ? NavigationExpansionHelpers.CreateNavigationExpansionRoot(navigationBindingExpression, lastNavigation.GetTargetType(), lastNavigation)
                        : CreateCollectionNavigationExpression(navigationBindingExpression.NavigationTreeNode, navigationBindingExpression.RootParameter, navigationBindingExpression.SourceMapping);
                }
                else
                {
                    return extensionExpression;
                }
            }

            return base.VisitExtension(extensionExpression);
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var newExpression = RemoveMaterializeCollection(Visit(memberExpression.Expression));
            if (newExpression != memberExpression.Expression)
            {
                if (memberExpression.Member.Name == nameof(List<int>.Count))
                {
                    var countMethod = LinqMethodHelpers.QueryableCountMethodInfo.MakeGenericMethod(newExpression.Type.GetSequenceType());
                    var result = Expression.Call(instance: null, countMethod, newExpression);

                    return result;
                }

                return memberExpression.Update(newExpression);
            }

            return memberExpression;
        }

        private static Expression CreateKeyComparisonExpressionForCollectionNavigationSubquery(
            Expression outerKeyExpression,
            Expression innerKeyExpression,
            Expression collectionRootExpression)
        {
            if (outerKeyExpression.Type != innerKeyExpression.Type)
            {
                if (outerKeyExpression.Type.IsNullableType())
                {
                    Debug.Assert(outerKeyExpression.Type.UnwrapNullableType() == innerKeyExpression.Type);

                    innerKeyExpression = Expression.Convert(innerKeyExpression, outerKeyExpression.Type);
                }
                else
                {
                    Debug.Assert(innerKeyExpression.Type.IsNullableType());
                    Debug.Assert(innerKeyExpression.Type.UnwrapNullableType() == outerKeyExpression.Type);

                    outerKeyExpression = Expression.Convert(outerKeyExpression, innerKeyExpression.Type);
                }
            }

            var outerNullProtection
                = Expression.NotEqual(
                    collectionRootExpression,
                    Expression.Constant(null, collectionRootExpression.Type));

            return new CorrelationPredicateExpression(
                outerNullProtection,
                Expression.Equal(outerKeyExpression, innerKeyExpression));
        }
    }
}
