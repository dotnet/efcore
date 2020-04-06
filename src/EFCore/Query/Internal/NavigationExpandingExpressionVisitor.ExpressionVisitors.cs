// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public partial class NavigationExpandingExpressionVisitor
    {
        /// <summary>
        ///     Expands navigations in the given tree for given source.
        ///     Optionally also expands navigations for includes.
        /// </summary>
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
                Model = navigationExpandingExpressionVisitor._queryCompilationContext.Model;
            }

            public Expression Expand(Expression expression, bool applyIncludes = false)
            {
                expression = Visit(expression);
                if (applyIncludes)
                {
                    expression = new IncludeExpandingExpressionVisitor(_navigationExpandingExpressionVisitor, _source)
                        .Visit(expression);
                }

                return expression;
            }

            protected IModel Model { get; }

            protected override Expression VisitExtension(Expression expression)
            {
                Check.NotNull(expression, nameof(expression));

                switch (expression)
                {
                    case NavigationExpansionExpression _:
                    case NavigationTreeExpression _:
                        return expression;

                    default:
                        return base.VisitExtension(expression);
                }
            }

            protected override Expression VisitMember(MemberExpression memberExpression)
            {
                Check.NotNull(memberExpression, nameof(memberExpression));

                var innerExpression = Visit(memberExpression.Expression);
                return TryExpandNavigation(innerExpression, MemberIdentity.Create(memberExpression.Member))
                    ?? memberExpression.Update(innerExpression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                Check.NotNull(methodCallExpression, nameof(methodCallExpression));

                if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var navigationName))
                {
                    source = Visit(source);
                    return TryExpandNavigation(source, MemberIdentity.Create(navigationName))
                        ?? methodCallExpression.Update(null, new[] { source, methodCallExpression.Arguments[1] });
                }

                if (methodCallExpression.TryGetIndexerArguments(Model, out source, out navigationName))
                {
                    source = Visit(source);
                    return TryExpandNavigation(source, MemberIdentity.Create(navigationName))
                        ?? methodCallExpression.Update(source, new[] { methodCallExpression.Arguments[0] });
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            private Expression TryExpandNavigation(Expression root, MemberIdentity memberIdentity)
            {
                var innerExpression = root.UnwrapTypeConversion(out var convertedType);
                if (UnwrapEntityReference(innerExpression) is EntityReference entityReference)
                {
                    var entityType = entityReference.EntityType;
                    if (convertedType != null)
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

                var targetType = navigation.TargetEntityType;
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
                    if (navigation.IsCollection)
                    {
                        var elementType = ownedExpansion.Type.TryGetSequenceType();
                        var subquery = Expression.Call(
                            QueryableMethods.AsQueryable.MakeGenericMethod(elementType),
                            ownedExpansion);

                        return new MaterializeCollectionNavigationExpression(subquery, navigation);
                    }

                    entityReference.NavigationMap[navigation] = ownedExpansion;
                    return ownedExpansion;
                }

                var innerQueryable = new QueryRootExpression(targetType);
                var innerSource = (NavigationExpansionExpression)_navigationExpandingExpressionVisitor.Visit(innerQueryable);
                if (entityReference.IncludePaths.ContainsKey(navigation))
                {
                    var innerEntityReference = (EntityReference)((NavigationTreeExpression)innerSource.PendingSelector).Value;
                    innerEntityReference.SetIncludePaths(entityReference.IncludePaths[navigation]);
                }

                var innerSourceSequenceType = innerSource.Type.GetSequenceType();
                var innerParameter = Expression.Parameter(innerSourceSequenceType, "i");
                Expression outerKey;
                if (root is NavigationExpansionExpression innerNavigationExpansionExpression
                    && innerNavigationExpansionExpression.CardinalityReducingGenericMethodInfo != null)
                {
                    // This is FirstOrDefault ending so we need to push down properties.
                    var temporaryParameter = Expression.Parameter(root.Type);
                    var temporaryKey = temporaryParameter.CreateKeyValueReadExpression(
                        navigation.IsOnDependent
                            ? navigation.ForeignKey.Properties
                            : navigation.ForeignKey.PrincipalKey.Properties,
                        makeNullable: true);
                    var newSelector = ReplacingExpressionVisitor.Replace(
                        temporaryParameter,
                        innerNavigationExpansionExpression.PendingSelector,
                        temporaryKey);
                    innerNavigationExpansionExpression.ApplySelector(newSelector);
                    outerKey = innerNavigationExpansionExpression;
                }
                else
                {
                    outerKey = root.CreateKeyValueReadExpression(
                        navigation.IsOnDependent
                            ? navigation.ForeignKey.Properties
                            : navigation.ForeignKey.PrincipalKey.Properties,
                        makeNullable: true);
                }

                var innerKey = innerParameter.CreateKeyValueReadExpression(
                    navigation.IsOnDependent
                        ? navigation.ForeignKey.PrincipalKey.Properties
                        : navigation.ForeignKey.Properties,
                    makeNullable: true);

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

                if (navigation.IsCollection)
                {
                    // This is intentionally deferred to be applied to innerSource.Source
                    // Since outerKey's reference could change if a reference navigation is expanded afterwards
                    var predicateBody = Expression.AndAlso(
                        outerKey is NewExpression newExpression
                        && newExpression.Arguments[0] is NewArrayExpression newArrayExpression
                            ? newArrayExpression.Expressions
                                .Select(e =>
                                {
                                    var left = (e as UnaryExpression)?.Operand ?? e;

                                    return Expression.NotEqual(left, Expression.Constant(null, left.Type));
                                })
                                .Aggregate((l, r) => Expression.AndAlso(l, r))
                            : Expression.NotEqual(outerKey, Expression.Constant(null, outerKey.Type)),
                        Expression.Equal(outerKey, innerKey));

                    var subquery = Expression.Call(
                        QueryableMethods.Where.MakeGenericMethod(innerSourceSequenceType),
                        innerSource,
                        Expression.Quote(
                            Expression.Lambda(
                                predicateBody, innerParameter)));

                    return new MaterializeCollectionNavigationExpression(subquery, navigation);
                }

                var outerKeySelector = _navigationExpandingExpressionVisitor.GenerateLambda(
                    outerKey, _source.CurrentParameter);
                var innerKeySelector = _navigationExpandingExpressionVisitor.ProcessLambdaExpression(
                    innerSource, Expression.Lambda(innerKey, innerParameter));

                var resultSelectorOuterParameter = Expression.Parameter(_source.SourceElementType, "o");
                var resultSelectorInnerParameter = Expression.Parameter(innerSource.SourceElementType, "i");
                var resultType = TransparentIdentifierFactory.Create(_source.SourceElementType, innerSource.SourceElementType);

                var transparentIdentifierOuterMemberInfo = resultType.GetTypeInfo().GetDeclaredField("Outer");
                var transparentIdentifierInnerMemberInfo = resultType.GetTypeInfo().GetDeclaredField("Inner");

                var resultSelector = Expression.Lambda(
                    Expression.New(
                        resultType.GetConstructors().Single(),
                        new[] { resultSelectorOuterParameter, resultSelectorInnerParameter }, transparentIdentifierOuterMemberInfo,
                        transparentIdentifierInnerMemberInfo),
                    resultSelectorOuterParameter,
                    resultSelectorInnerParameter);

                var innerJoin = !entityReference.IsOptional
                    && !derivedTypeConversion
                    && navigation.IsOnDependent
                    && navigation.ForeignKey.IsRequired;

                if (!innerJoin)
                {
                    var innerEntityReference = (EntityReference)((NavigationTreeExpression)innerSource.PendingSelector).Value;
                    innerEntityReference.MarkAsOptional();
                }

                _source.UpdateSource(
                    Expression.Call(
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

        /// <summary>
        ///     Expands an include tree. This is separate and needed because we may need to reconstruct parts of
        ///     <see cref="NewExpression"/> to apply includes.
        /// </summary>
        private sealed class IncludeExpandingExpressionVisitor : ExpandingExpressionVisitor
        {
            private readonly bool _isTracking;

            public IncludeExpandingExpressionVisitor(
                NavigationExpandingExpressionVisitor navigationExpandingExpressionVisitor,
                NavigationExpansionExpression source)
                : base(navigationExpandingExpressionVisitor, source)
            {
                _isTracking = navigationExpandingExpressionVisitor._queryCompilationContext.IsTracking;
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                Check.NotNull(extensionExpression, nameof(extensionExpression));

                switch (extensionExpression)
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

                    case MaterializeCollectionNavigationExpression _:
                        return extensionExpression;
                }

                return base.VisitExtension(extensionExpression);
            }

            protected override Expression VisitMember(MemberExpression memberExpression)
            {
                Check.NotNull(memberExpression, nameof(memberExpression));

                var innerExpression = memberExpression.Expression.UnwrapTypeConversion(out var convertedType);
                if (UnwrapEntityReference(innerExpression) is EntityReference entityReference)
                {
                    // If it is mapped property then, it would get converted to a column so we don't need to expand includes.
                    var entityType = entityReference.EntityType;
                    if (convertedType != null)
                    {
                        entityType = entityType.GetTypesInHierarchy()
                            .FirstOrDefault(et => et.ClrType == convertedType);
                        if (entityType == null)
                        {
                            return base.VisitMember(memberExpression);
                        }
                    }

                    var property = entityType.FindProperty(memberExpression.Member);
                    if (property != null)
                    {
                        return memberExpression;
                    }
                }

                return base.VisitMember(memberExpression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                Check.NotNull(methodCallExpression, nameof(methodCallExpression));

                if (methodCallExpression.TryGetEFPropertyArguments(out _, out _))
                {
                    // If it is EF.Property then, it would get converted to a column or throw
                    // so we don't need to expand includes.
                    return methodCallExpression;
                }

                if (methodCallExpression.TryGetIndexerArguments(Model, out var source, out var propertyName))
                {
                    if (UnwrapEntityReference(source) is EntityReference entityReferece)
                    {
                        // If it is mapped property then, it would get converted to a column so we don't need to expand includes.
                        var property = entityReferece.EntityType.FindProperty(propertyName);
                        if (property != null)
                        {
                            return methodCallExpression;
                        }
                    }
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            protected override Expression VisitNew(NewExpression newExpression)
            {
                Check.NotNull(newExpression, nameof(newExpression));

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
                    var inverseNavigation = navigation.Inverse;
                    if (inverseNavigation != null
                        && referenceIncludeTreeNode.ContainsKey(inverseNavigation))
                    {
                        throw new InvalidOperationException(CoreStrings.IncludeWithCycle(navigation.Name, inverseNavigation.Name));
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
                    if (!navigation.IsCollection)
                    {
                        var innerEntityReference = navigation.TargetEntityType.HasDefiningNavigation()
                            || navigation.TargetEntityType.IsOwned()
                                ? ((OwnedNavigationReference)included).EntityReference
                                : (EntityReference)((NavigationTreeExpression)included).Value;

                        included = ExpandIncludesHelper(included, innerEntityReference);
                    }

                    if (included is MaterializeCollectionNavigationExpression materializeCollectionNavigation)
                    {
                        var filterExpression = entityReference.IncludePaths[navigation].FilterExpression;
                        if (filterExpression != null)
                        {
                            var subquery = ReplacingExpressionVisitor.Replace(
                                filterExpression.Parameters[0],
                                materializeCollectionNavigation.Subquery,
                                filterExpression.Body);

                            included = materializeCollectionNavigation.Update(subquery);
                        }
                    }

                    result = new IncludeExpression(result, included, navigation);
                }

                return result;
            }
        }

        /// <summary>
        ///     <see cref="NavigationExpansionExpression"/> remembers the pending selector so we don't expand
        ///     navigations unless we need to. This visitor applies them when we need to.
        /// </summary>
        private sealed class PendingSelectorExpandingExpressionVisitor : ExpressionVisitor
        {
            private readonly NavigationExpandingExpressionVisitor _visitor;
            private readonly bool _applyIncludes;

            public PendingSelectorExpandingExpressionVisitor(
                NavigationExpandingExpressionVisitor visitor, bool applyIncludes = false)
            {
                _visitor = visitor;
                _applyIncludes = applyIncludes;
            }

            public override Expression Visit(Expression expression)
            {
                if (expression is NavigationExpansionExpression navigationExpansionExpression)
                {
                    _visitor.ApplyPendingOrderings(navigationExpansionExpression);

                    var pendingSelector = new ExpandingExpressionVisitor(_visitor, navigationExpansionExpression)
                        .Expand(navigationExpansionExpression.PendingSelector, _applyIncludes);
                    pendingSelector = _visitor._subqueryMemberPushdownExpressionVisitor.Visit(pendingSelector);
                    pendingSelector = _visitor.Visit(pendingSelector);
                    pendingSelector = Visit(pendingSelector);
                    navigationExpansionExpression.ApplySelector(pendingSelector);

                    return navigationExpansionExpression;
                }

                return base.Visit(expression);
            }
        }

        /// <summary>
        ///     Removes custom expressions from tree and converts it to LINQ again.
        /// </summary>
        private sealed class ReducingExpressionVisitor : ExpressionVisitor
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

        /// <summary>
        ///     Marks <see cref="EntityReference"/> as nullable when coming from a left join.
        ///     Nullability is required to figure out if the navigation from this entity should be a left join or
        ///     an inner join.
        /// </summary>
        private sealed class EntityReferenceOptionalMarkingExpressionVisitor : ExpressionVisitor
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

        /// <summary>
        ///     Allows self reference of query root inside query filters/defining queries.
        /// </summary>
        private sealed class SelfReferenceEntityQueryableRewritingExpressionVisitor : ExpressionVisitor
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

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                Check.NotNull(extensionExpression, nameof(extensionExpression));

                return extensionExpression is QueryRootExpression queryRootExpression
                    && queryRootExpression.EntityType == _entityType
                    ? _navigationExpandingExpressionVisitor.CreateNavigationExpansionExpression(queryRootExpression, _entityType)
                    : base.VisitExtension(extensionExpression);
            }
        }

        private sealed class RemoveRedundantNavigationComparisonExpressionVisitor : ExpressionVisitor
        {
            private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

            public RemoveRedundantNavigationComparisonExpressionVisitor(IDiagnosticsLogger<DbLoggerCategory.Query> logger)
            {
                _logger = logger;
            }

            protected override Expression VisitBinary(BinaryExpression binaryExpression)
                => (binaryExpression.NodeType == ExpressionType.Equal
                    || binaryExpression.NodeType == ExpressionType.NotEqual)
                    && TryRemoveNavigationComparison(
                        binaryExpression.NodeType, binaryExpression.Left, binaryExpression.Right, out var result)
                    ? result
                    : base.VisitBinary(binaryExpression);

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                var method = methodCallExpression.Method;
                if (method.Name == nameof(object.Equals)
                    && methodCallExpression.Object != null
                    && methodCallExpression.Arguments.Count == 1
                    && TryRemoveNavigationComparison(
                        ExpressionType.Equal, methodCallExpression.Object, methodCallExpression.Arguments[0], out var result))
                {
                    return result;
                }

                if (method.Name == nameof(object.Equals)
                    && methodCallExpression.Object == null
                    && methodCallExpression.Arguments.Count == 2
                    && TryRemoveNavigationComparison(
                        ExpressionType.Equal, methodCallExpression.Arguments[0], methodCallExpression.Arguments[1], out result))
                {
                    return result;
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            private bool TryRemoveNavigationComparison(ExpressionType nodeType, Expression left, Expression right, out Expression result)
            {
                result = null;
                var leftNavigationData = ProcessNavigationPath(left) as NavigationDataExpression;
                var rightNavigationData = ProcessNavigationPath(right) as NavigationDataExpression;

                if (leftNavigationData == null
                    && rightNavigationData == null)
                {
                    return false;
                }

                if (left.IsNullConstantExpression()
                    || right.IsNullConstantExpression())
                {
                    var nonNullNavigationData = left.IsNullConstantExpression()
                        ? rightNavigationData
                        : leftNavigationData;

                    if (nonNullNavigationData.Navigation?.IsCollection == true)
                    {
                        _logger.PossibleUnintendedCollectionNavigationNullComparisonWarning(nonNullNavigationData.Navigation);

                        result = Expression.MakeBinary(
                            nodeType, nonNullNavigationData.Inner.Current, Expression.Constant(null, nonNullNavigationData.Inner.Type));

                        return true;
                    }
                }
                else if (leftNavigationData != null
                    && rightNavigationData != null)
                {
                    if (leftNavigationData.Navigation?.IsCollection == true)
                    {
                        if (leftNavigationData.Navigation == rightNavigationData.Navigation)
                        {
                            _logger.PossibleUnintendedReferenceComparisonWarning(leftNavigationData.Current, rightNavigationData.Current);

                            result = Expression.MakeBinary(nodeType, leftNavigationData.Inner.Current, rightNavigationData.Inner.Current);
                        }
                        else
                        {
                            result = Expression.Constant(nodeType == ExpressionType.NotEqual);
                        }

                        return true;
                    }
                }

                return false;
            }

            private Expression ProcessNavigationPath(Expression expression)
            {
                switch (expression)
                {
                    case MemberExpression memberExpression:
                        var innerExpression = ProcessNavigationPath(memberExpression.Expression);
                        if (innerExpression is NavigationDataExpression navigationDataExpression
                            && navigationDataExpression.EntityType != null)
                        {
                            var navigation = navigationDataExpression.EntityType.FindNavigation(memberExpression.Member);
                            if (navigation != null)
                            {
                                return new NavigationDataExpression(expression, navigationDataExpression, navigation);
                            }
                        }

                        return expression;

                    case MethodCallExpression methodCallExpression
                    when methodCallExpression.TryGetEFPropertyArguments(out var source, out var navigationName):
                        return expression;

                    default:
                        var convertlessExpression = expression.UnwrapTypeConversion(out var convertedType);
                        if (UnwrapEntityReference(convertlessExpression) is EntityReference entityReference)
                        {
                            var entityType = entityReference.EntityType;
                            if (convertedType != null)
                            {
                                entityType = entityType.GetTypesInHierarchy()
                                    .FirstOrDefault(et => et.ClrType == convertedType);
                                if (entityType == null)
                                {
                                    return expression;
                                }
                            }

                            return new NavigationDataExpression(expression, entityType);
                        }

                        return expression;
                }
            }

            private sealed class NavigationDataExpression : Expression
            {
                public NavigationDataExpression(Expression current, IEntityType entityType)
                {
                    Navigation = default;
                    Current = current;
                    EntityType = entityType;
                }

                public NavigationDataExpression(Expression current, NavigationDataExpression inner, INavigation navigation)
                {
                    Current = current;
                    Inner = inner;
                    Navigation = navigation;
                    if (!navigation.IsCollection)
                    {
                        EntityType = navigation.TargetEntityType;
                    }
                }

                public override Type Type => Current.Type;
                public override ExpressionType NodeType => ExpressionType.Extension;

                public INavigation Navigation { get; }
                public Expression Current { get; }
                public NavigationDataExpression Inner { get; }
                public IEntityType EntityType { get; }
            }
        }
    }
}
