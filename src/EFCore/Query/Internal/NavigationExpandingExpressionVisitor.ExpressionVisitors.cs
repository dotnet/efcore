// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public partial class NavigationExpandingExpressionVisitor
    {
        private class ExpandingExpressionVisitor : ExpressionVisitor
        {
            private readonly NavigationExpandingExpressionVisitor _navigationExpandingExpressionVisitor;
            private readonly NavigationExpansionExpression _source;

            public ExpandingExpressionVisitor(
                NavigationExpandingExpressionVisitor navigationExpandingExpressionVisitor,
                NavigationExpansionExpression source)
            {
                _navigationExpandingExpressionVisitor = navigationExpandingExpressionVisitor;
                _source = source;
            }

            protected override Expression VisitExtension(Expression expression)
            {
                switch (expression)
                {
                    case NavigationExpansionExpression _:
                    case NavigationTreeExpression _:
                        return expression;

                    default:
                        return base.VisitExtension(expression);
                }
            }

            protected EntityReference UnwrapEntityReference(Expression expression)
            {
                switch (expression)
                {
                    case EntityReference entityReference:
                        return entityReference;

                    case NavigationTreeExpression navigationTreeExpression:
                        return UnwrapEntityReference(navigationTreeExpression.Value);

                    case NavigationExpansionExpression navigationExpansionExpression
                        when navigationExpansionExpression.CardinalityReducingGenericMethodInfo != null:
                        return UnwrapEntityReference(navigationExpansionExpression.PendingSelector);

                    case OwnedNavigationReference ownedNavigationReference:
                        return ownedNavigationReference.EntityReference;

                    case NullConditionalExpression nullConditionalExpression:
                        return UnwrapEntityReference(nullConditionalExpression.AccessOperation);

                    default:
                        return null;
                }
            }

            protected override Expression VisitMember(MemberExpression memberExpression)
            {
                var innerExpression = Visit(memberExpression.Expression);
                return TryExpandNavigation(innerExpression, MemberIdentity.Create(memberExpression.Member))
                    ?? memberExpression.Update(innerExpression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var navigationName))
                {
                    source = Visit(source);
                    return TryExpandNavigation(source, MemberIdentity.Create(navigationName))
                        ?? methodCallExpression.Update(null, new[] { source, methodCallExpression.Arguments[1] });
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            private Expression TryExpandNavigation(Expression root, MemberIdentity memberIdentity)
            {
                Type convertedType = null;
                var innerExpression = root;
                if (innerExpression is UnaryExpression unaryExpression
                    && unaryExpression.NodeType == ExpressionType.Convert)
                {
                    innerExpression = unaryExpression.Operand;
                    if (unaryExpression.Type != typeof(object)
                        && unaryExpression.Type != innerExpression.Type)
                    {
                        convertedType = unaryExpression.Type;
                    }
                }

                if (UnwrapEntityReference(innerExpression) is EntityReference entityReference)
                {
                    var entityType = entityReference.EntityType;
                    if (convertedType != null
                        && !(convertedType.IsInterface
                             && convertedType.IsAssignableFrom(entityType.ClrType)))
                    {
                        entityType = entityType.GetTypesInHierarchy()
                            .FirstOrDefault(et => et.ClrType == convertedType);
                        if (entityType == null)
                        {
                            return null;
                        }
                    }

                    var navigation = memberIdentity.MemberInfo != null
                        ? entityType.FindNavigation(memberIdentity.MemberInfo)
                        : entityType.FindNavigation(memberIdentity.Name);
                    if (navigation != null)
                    {
                        return ExpandNavigation(root, entityReference, navigation, convertedType != null);
                    }
                }

                return null;
            }

            protected Expression ExpandNavigation(
                Expression root, EntityReference entityReference, INavigation navigation, bool derivedTypeConversion)
            {
                if (entityReference.NavigationMap.TryGetValue(navigation, out var expansion))
                {
                    return expansion;
                }

                var targetType = navigation.GetTargetType();
                if (targetType.HasDefiningNavigation()
                    || targetType.IsOwned())
                {
                    var ownedEntityReference = new EntityReference(targetType);
                    ownedEntityReference.MarkAsOptional();
                    if (entityReference.IncludePaths.ContainsKey(navigation))
                    {
                        ownedEntityReference.SetIncludePaths(entityReference.IncludePaths[navigation]);
                    }

                    var ownedExpansion = new OwnedNavigationReference(root, navigation, ownedEntityReference);
                    if (navigation.IsCollection())
                    {
                        var elementType = ownedExpansion.Type.TryGetSequenceType();
                        var subquery = Expression.Call(
                            QueryableMethods.AsQueryable.MakeGenericMethod(elementType),
                            ownedExpansion);

                        return new MaterializeCollectionNavigationExpression(subquery, navigation);
                    }
                    else
                    {
                        entityReference.NavigationMap[navigation] = ownedExpansion;
                        return ownedExpansion;
                    }
                }

                var innerQueryableType = targetType.ClrType;
                var innerQueryable = NullAsyncQueryProvider.Instance.CreateEntityQueryableExpression(innerQueryableType);
                var innerSource = (NavigationExpansionExpression)_navigationExpandingExpressionVisitor.Visit(innerQueryable);
                if (entityReference.IncludePaths.ContainsKey(navigation))
                {
                    var innerIncludeTreeNode = entityReference.IncludePaths[navigation];
                    var innerEntityReference = (EntityReference)((NavigationTreeExpression)innerSource.PendingSelector).Value;
                    innerEntityReference.SetIncludePaths(innerIncludeTreeNode);
                }

                var innerSoureSequenceType = innerSource.Type.GetSequenceType();
                var innerParameter = Expression.Parameter(innerSoureSequenceType, "i");
                Expression outerKey;
                if (root is NavigationExpansionExpression innerNavigationExpansionExpression
                    && innerNavigationExpansionExpression.CardinalityReducingGenericMethodInfo != null)
                {
                    // This is FirstOrDefault ending so we need to push down properties.
                    var temporaryParameter = Expression.Parameter(root.Type);
                    var temporaryKey = CreateKeyAccessExpression(temporaryParameter,
                        navigation.IsDependentToPrincipal()
                            ? navigation.ForeignKey.Properties
                            : navigation.ForeignKey.PrincipalKey.Properties);
                    var newSelector = ReplacingExpressionVisitor.Replace(
                        temporaryParameter,
                        innerNavigationExpansionExpression.PendingSelector,
                        temporaryKey);
                    innerNavigationExpansionExpression.ApplySelector(newSelector);
                    outerKey = innerNavigationExpansionExpression;
                }
                else
                {
                    outerKey = CreateKeyAccessExpression(root,
                        navigation.IsDependentToPrincipal()
                            ? navigation.ForeignKey.Properties
                            : navigation.ForeignKey.PrincipalKey.Properties);
                }

                var innerKey = CreateKeyAccessExpression(innerParameter,
                    navigation.IsDependentToPrincipal()
                        ? navigation.ForeignKey.PrincipalKey.Properties
                        : navigation.ForeignKey.Properties);

                if (outerKey.Type != innerKey.Type)
                {
                    if (!outerKey.Type.IsNullableType())
                    {
                        outerKey = Expression.Convert(outerKey, outerKey.Type.MakeNullable());
                    }

                    if (!innerKey.Type.IsNullableType())
                    {
                        innerKey = Expression.Convert(innerKey, innerKey.Type.MakeNullable());
                    }
                }

                if (navigation.IsCollection())
                {
                    // This is intentionally deferred to be applied to innerSource.Source
                    // Since outerKey's reference could change if a reference navigation is expanded afterwards
                    var subquery = Expression.Call(
                        QueryableMethods.Where.MakeGenericMethod(innerSoureSequenceType),
                        innerSource,
                        Expression.Quote(
                            Expression.Lambda(
                                Expression.Equal(outerKey, innerKey), innerParameter)));

                    return new MaterializeCollectionNavigationExpression(subquery, navigation);
                }
                else
                {
                    var outerKeySelector = _navigationExpandingExpressionVisitor.GenerateLambda(
                        outerKey, _source.CurrentParameter);
                    var innerKeySelector = _navigationExpandingExpressionVisitor.GenerateLambda(
                        _navigationExpandingExpressionVisitor.ExpandNavigationsInLambdaExpression(
                            innerSource,
                            Expression.Lambda(innerKey, innerParameter)),
                        innerSource.CurrentParameter);

                    var resultSelectorOuterParameter = Expression.Parameter(_source.SourceElementType, "o");
                    var resultSelectorInnerParameter = Expression.Parameter(innerSource.SourceElementType, "i");
                    var resultType = TransparentIdentifierFactory.Create(_source.SourceElementType, innerSource.SourceElementType);

                    var transparentIdentifierOuterMemberInfo = resultType.GetTypeInfo().GetDeclaredField("Outer");
                    var transparentIdentifierInnerMemberInfo = resultType.GetTypeInfo().GetDeclaredField("Inner");

                    var resultSelector = Expression.Lambda(
                        Expression.New(
                            resultType.GetTypeInfo().GetConstructors().Single(),
                            new[] { resultSelectorOuterParameter, resultSelectorInnerParameter },
                            new[] { transparentIdentifierOuterMemberInfo, transparentIdentifierInnerMemberInfo }),
                        resultSelectorOuterParameter,
                        resultSelectorInnerParameter);

                    var innerJoin = !entityReference.IsOptional && !derivedTypeConversion
                        && navigation.IsDependentToPrincipal() && navigation.ForeignKey.IsRequired;

                    if (!innerJoin)
                    {
                        var innerEntityReference = (EntityReference)((NavigationTreeExpression)innerSource.PendingSelector).Value;
                        innerEntityReference.MarkAsOptional();
                    }

                    _source.UpdateSource(Expression.Call(
                        (innerJoin
                            ? QueryableMethods.Join
                            : QueryableExtensions.LeftJoinMethodInfo).MakeGenericMethod(
                                _source.SourceElementType,
                                innerSource.SourceElementType,
                                outerKeySelector.ReturnType,
                                resultSelector.ReturnType),
                        _source.Source,
                        innerSource.Source,
                        outerKeySelector,
                        innerKeySelector,
                        resultSelector));

                    entityReference.NavigationMap[navigation] = innerSource.PendingSelector;

                    _source.UpdateCurrentTree(new NavigationTreeNode(_source.CurrentTree, innerSource.CurrentTree));

                    return innerSource.PendingSelector;
                }
            }

            private static Expression CreateKeyAccessExpression(Expression target, IReadOnlyList<IProperty> properties)
                => properties.Count == 1
                    ? target.CreateEFPropertyExpression(properties[0])
                    : Expression.New(
                        AnonymousObject.AnonymousObjectCtor,
                        Expression.NewArrayInit(
                            typeof(object),
                            properties
                                .Select(p => Expression.Convert(target.CreateEFPropertyExpression(p), typeof(object)))
                                .ToArray()));
        }

        private class IncludeExpandingExpressionVisitor : ExpandingExpressionVisitor
        {
            private readonly bool _isTracking;

            public IncludeExpandingExpressionVisitor(
                NavigationExpandingExpressionVisitor navigationExpandingExpressionVisitor,
                NavigationExpansionExpression source,
                bool tracking)
                : base(navigationExpandingExpressionVisitor, source)
            {
                _isTracking = tracking;
            }

            public override Expression Visit(Expression expression)
            {
                switch (expression)
                {
                    case NavigationTreeExpression navigationTreeExpression:
                        if (navigationTreeExpression.Value is EntityReference entityReference)
                        {
                            return ExpandInclude(navigationTreeExpression, entityReference);
                        }

                        if (navigationTreeExpression.Value is NewExpression newExpression)
                        {
                            if (ReconstructAnonymousType(navigationTreeExpression, newExpression, out var replacement))
                            {
                                return replacement;
                            }
                        }
                        break;

                    case OwnedNavigationReference ownedNavigationReference:
                        return ExpandInclude(ownedNavigationReference, ownedNavigationReference.EntityReference);
                }

                return base.Visit(expression);
            }

            protected override Expression VisitMember(MemberExpression memberExpression)
            {
                if (UnwrapEntityReference(memberExpression.Expression) is EntityReference)
                {
                    // If it matches then it is property access. All navigation accesses are already expanded.
                    return memberExpression;
                }

                return base.VisitMember(memberExpression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.TryGetEFPropertyArguments(out var _, out var __))
                {
                    // If it matches then it is property access. All navigation accesses are already expanded.
                    return methodCallExpression;
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            protected override Expression VisitNew(NewExpression newExpression)
            {
                var arguments = new Expression[newExpression.Arguments.Count];
                for (var i = 0; i < newExpression.Arguments.Count; i++)
                {
                    var argument = newExpression.Arguments[i];
                    arguments[i] = argument is EntityReference entityReference
                        ? ExpandInclude(argument, entityReference)
                        : Visit(argument);
                }

                return newExpression.Update(arguments);
            }

            private bool ReconstructAnonymousType(Expression currentRoot, NewExpression newExpression, out Expression replacement)
            {
                replacement = null;
                var changed = false;
                if (newExpression.Arguments.Count > 0
                    && newExpression.Members == null)
                {
                    return changed;
                }

                var arguments = new Expression[newExpression.Arguments.Count];
                for (var i = 0; i < newExpression.Arguments.Count; i++)
                {
                    var argument = newExpression.Arguments[i];
                    var newRoot = Expression.MakeMemberAccess(currentRoot, newExpression.Members[i]);
                    if (argument is EntityReference entityReference)
                    {
                        changed = true;
                        arguments[i] = ExpandInclude(newRoot, entityReference);
                    }
                    else if (argument is NewExpression innerNewExpression)
                    {
                        if (ReconstructAnonymousType(newRoot, innerNewExpression, out var innerReplacement))
                        {
                            changed = true;
                            arguments[i] = innerReplacement;
                        }
                        else
                        {
                            arguments[i] = newRoot;
                        }
                    }
                    else
                    {
                        arguments[i] = newRoot;
                    }
                }

                if (changed)
                {
                    replacement = newExpression.Update(arguments);
                }

                return changed;
            }

            private Expression ExpandInclude(Expression root, EntityReference entityReference)
            {
                if (!_isTracking)
                {
                    VerifyNoCycles(entityReference.IncludePaths);
                }

                return ExpandIncludesHelper(root, entityReference);
            }

            private void VerifyNoCycles(IncludeTreeNode includeTreeNode)
            {
                foreach (var keyValuePair in includeTreeNode)
                {
                    var navigation = keyValuePair.Key;
                    var referenceIncludeTreeNode = keyValuePair.Value;
                    var inverseNavigation = navigation.FindInverse();
                    if (inverseNavigation != null
                        && referenceIncludeTreeNode.ContainsKey(inverseNavigation))
                    {
                        throw new InvalidOperationException(
                            $"The Include path '{navigation.Name}->{inverseNavigation.Name}' results in a cycle. " +
                            $"Cycles are not allowed in no-tracking queries. " +
                            $"Either use a tracking query or remove the cycle.");
                    }

                    VerifyNoCycles(referenceIncludeTreeNode);
                }
            }

            private Expression ExpandIncludesHelper(Expression root, EntityReference entityReference)
            {
                var result = root;
                var convertedRoot = root;
                foreach (var kvp in entityReference.IncludePaths)
                {
                    var navigation = kvp.Key;
                    var converted = false;
                    if (entityReference.EntityType != navigation.DeclaringEntityType
                        && entityReference.EntityType.IsAssignableFrom(navigation.DeclaringEntityType))
                    {
                        converted = true;
                        convertedRoot = Expression.Convert(root, navigation.DeclaringEntityType.ClrType);
                    }

                    var included = ExpandNavigation(convertedRoot, entityReference, navigation, converted);
                    // Collection will expand it's includes when reducing the navigationExpansionExpression
                    if (!navigation.IsCollection())
                    {
                        var innerEntityReference = navigation.GetTargetType().HasDefiningNavigation()
                            || navigation.GetTargetType().IsOwned()
                            ? ((OwnedNavigationReference)included).EntityReference
                            : (EntityReference)((NavigationTreeExpression)included).Value;

                        included = ExpandIncludesHelper(included, innerEntityReference);
                    }

                    result = new IncludeExpression(result, included, navigation);
                }

                return result;
            }
        }

        private class IncludeApplyingExpressionVisitor : ExpressionVisitor
        {
            private readonly NavigationExpandingExpressionVisitor _visitor;
            private readonly bool _isTracking;

            public IncludeApplyingExpressionVisitor(NavigationExpandingExpressionVisitor visitor, bool tracking)
            {
                _visitor = visitor;
                _isTracking = tracking;
            }

            public override Expression Visit(Expression expression)
            {
                if (expression is NavigationExpansionExpression navigationExpansionExpression)
                {
                    var innerVisitor = new IncludeExpandingExpressionVisitor(_visitor, navigationExpansionExpression, _isTracking);
                    var pendingSelector = innerVisitor.Visit(navigationExpansionExpression.PendingSelector);
                    pendingSelector = _visitor.Visit(pendingSelector);
                    pendingSelector = Visit(pendingSelector);

                    navigationExpansionExpression.ApplySelector(pendingSelector);

                    return navigationExpansionExpression;
                }

                return base.Visit(expression);
            }
        }

        private class PendingSelectorExpandingExpressionVisitor : ExpressionVisitor
        {
            private readonly NavigationExpandingExpressionVisitor _visitor;

            public PendingSelectorExpandingExpressionVisitor(NavigationExpandingExpressionVisitor visitor)
            {
                _visitor = visitor;
            }

            public override Expression Visit(Expression expression)
            {
                if (expression is NavigationExpansionExpression navigationExpansionExpression)
                {
                    _visitor.ApplyPendingOrderings(navigationExpansionExpression);

                    var pendingSelector = _visitor.ExpandNavigationsInExpression(
                        navigationExpansionExpression, navigationExpansionExpression.PendingSelector);

                    pendingSelector = Visit(pendingSelector);

                    navigationExpansionExpression.ApplySelector(pendingSelector);

                    return navigationExpansionExpression;
                }

                return base.Visit(expression);
            }
        }

        private class ReducingExpressionVisitor : ExpressionVisitor
        {
            public override Expression Visit(Expression expression)
            {
                switch (expression)
                {
                    case NavigationTreeExpression navigationTreeExpression:
                        return navigationTreeExpression.GetExpression();

                    case NavigationExpansionExpression navigationExpansionExpression:
                    {
                        var pendingSelector = Visit(navigationExpansionExpression.PendingSelector);
                        Expression result;
                        var source = Visit(navigationExpansionExpression.Source);
                        if (pendingSelector == navigationExpansionExpression.CurrentParameter)
                        {
                            // identity projection
                            result = source;
                        }
                        else
                        {
                            var selectorLambda = Expression.Lambda(pendingSelector, navigationExpansionExpression.CurrentParameter);

                            result = Expression.Call(
                                QueryableMethods.Select.MakeGenericMethod(
                                    navigationExpansionExpression.SourceElementType,
                                    selectorLambda.ReturnType),
                                source,
                                Expression.Quote(selectorLambda));
                        }

                        if (navigationExpansionExpression.CardinalityReducingGenericMethodInfo != null)
                        {
                            result = Expression.Call(
                                navigationExpansionExpression.CardinalityReducingGenericMethodInfo.MakeGenericMethod(
                                    result.Type.TryGetSequenceType()),
                                result);
                        }

                        return result;
                    }

                    case OwnedNavigationReference ownedNavigationReference:
                        return Visit(ownedNavigationReference.Parent).CreateEFPropertyExpression(ownedNavigationReference.Navigation);

                    case IncludeExpression includeExpression:
                        var entityExpression = Visit(includeExpression.EntityExpression);
                        var navigationExpression = ReplacingExpressionVisitor.Replace(
                            includeExpression.EntityExpression,
                            entityExpression,
                            includeExpression.NavigationExpression);

                        navigationExpression = Visit(navigationExpression);

                        return includeExpression.Update(entityExpression, navigationExpression);

                    default:
                        return base.Visit(expression);
                }
            }
        }

        private class EntityReferenceOptionalMarkingExpressionVisitor : ExpressionVisitor
        {
            public override Expression Visit(Expression expression)
            {
                if (expression is EntityReference entityReference)
                {
                    entityReference.MarkAsOptional();

                    return entityReference;
                }

                return base.Visit(expression);
            }
        }

        private class SelfReferenceEntityQueryableRewritingExpressionVisitor : ExpressionVisitor
        {
            private readonly NavigationExpandingExpressionVisitor _navigationExpandingExpressionVisitor;
            private readonly IEntityType _entityType;

            public SelfReferenceEntityQueryableRewritingExpressionVisitor(
                NavigationExpandingExpressionVisitor navigationExpandingExpressionVisitor,
                IEntityType entityType)
            {
                _navigationExpandingExpressionVisitor = navigationExpandingExpressionVisitor;
                _entityType = entityType;
            }

            protected override Expression VisitConstant(ConstantExpression constantExpression)
            {
                if (constantExpression.IsEntityQueryable())
                {
                    var entityType = _navigationExpandingExpressionVisitor._queryCompilationContext.Model.FindEntityType(((IQueryable)constantExpression.Value).ElementType);
                    if (entityType == _entityType)
                    {
                        return _navigationExpandingExpressionVisitor.CreateNavigationExpansionExpression(constantExpression, entityType);
                    }
                }

                return base.VisitConstant(constantExpression);
            }
        }
    }
}
