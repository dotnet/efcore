// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            private static readonly MethodInfo _objectEqualsMethodInfo
                = typeof(object).GetRuntimeMethod(nameof(object.Equals), new[] { typeof(object), typeof(object) });

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
                        entityType = entityType.GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive())
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

                    var skipNavigation = memberIdentity.MemberInfo != null
                        ? entityType.FindSkipNavigation(memberIdentity.MemberInfo)
                        : entityType.FindSkipNavigation(memberIdentity.Name);
                    if (skipNavigation != null)
                    {
                        return ExpandSkipNavigation(root, entityReference, skipNavigation, convertedType != null);
                    }
                }

                return null;
            }

            protected Expression ExpandNavigation(
                Expression root,
                EntityReference entityReference,
                INavigation navigation,
                bool derivedTypeConversion)
            {
                var targetType = navigation.TargetEntityType;
                if (targetType.HasDefiningNavigation()
                    || targetType.IsOwned())
                {
                    if (entityReference.ForeignKeyExpansionMap.TryGetValue(
                        (navigation.ForeignKey, navigation.IsOnDependent), out var ownedExpansion))
                    {
                        return ownedExpansion;
                    }

                    var ownedEntityReference = new EntityReference(targetType);
                    _navigationExpandingExpressionVisitor.PopulateEagerLoadedNavigations(ownedEntityReference.IncludePaths);
                    ownedEntityReference.MarkAsOptional();
                    if (entityReference.IncludePaths.TryGetValue(navigation, out var includePath))
                    {
                        ownedEntityReference.IncludePaths.Merge(includePath);
                    }

                    ownedExpansion = new OwnedNavigationReference(root, navigation, ownedEntityReference);
                    if (navigation.IsCollection)
                    {
                        var elementType = ownedExpansion.Type.TryGetSequenceType();
                        var subquery = Expression.Call(
                            QueryableMethods.AsQueryable.MakeGenericMethod(elementType),
                            ownedExpansion);

                        return new MaterializeCollectionNavigationExpression(subquery, navigation);
                    }

                    entityReference.ForeignKeyExpansionMap[(navigation.ForeignKey, navigation.IsOnDependent)] = ownedExpansion;
                    return ownedExpansion;
                }

                var expansion = ExpandForeignKey(
                    root, entityReference, navigation.ForeignKey, navigation.IsOnDependent, derivedTypeConversion);

                return navigation.IsCollection
                    ? new MaterializeCollectionNavigationExpression(expansion, navigation)
                    : expansion;
            }

            protected Expression ExpandSkipNavigation(
                Expression root,
                EntityReference entityReference,
                ISkipNavigation navigation,
                bool derivedTypeConversion)
            {
                var inverseNavigation = navigation.Inverse;
                var includeTree = entityReference.IncludePaths.TryGetValue(navigation, out var tree)
                    ? tree
                    : null;

                var primaryExpansion = ExpandForeignKey(
                    root,
                    entityReference,
                    navigation.ForeignKey,
                    navigation.IsOnDependent,
                    derivedTypeConversion);
                Expression secondaryExpansion;

                if (navigation.ForeignKey.IsUnique
                    || navigation.IsOnDependent)
                {
                    // First pseudo-navigation is a reference
                    // ExpandFK handles both collection & reference navigation for second psuedo-navigation
                    secondaryExpansion = ExpandForeignKey(
                        primaryExpansion, UnwrapEntityReference(primaryExpansion), inverseNavigation.ForeignKey,
                        !inverseNavigation.IsOnDependent, derivedTypeConversion: false);
                }
                else
                {
                    var secondaryForeignKey = inverseNavigation.ForeignKey;
                    // First psuedo-navigation is a collection
                    if (secondaryForeignKey.IsUnique
                        || !inverseNavigation.IsOnDependent)
                    {
                        // Second psuedo-navigation is a reference
                        var secondTargetType = navigation.TargetEntityType;
                        var innerQueryable = new QueryRootExpression(secondTargetType);
                        var innerSource = (NavigationExpansionExpression)_navigationExpandingExpressionVisitor.Visit(innerQueryable);

                        if (includeTree != null)
                        {
                            UnwrapEntityReference(innerSource.PendingSelector).IncludePaths.Merge(includeTree);
                        }

                        var sourceElementType = primaryExpansion.Type.GetSequenceType();
                        var outerKeyparameter = Expression.Parameter(sourceElementType);
                        var outerKey = outerKeyparameter.CreateKeyValuesExpression(
                            !inverseNavigation.IsOnDependent
                                ? secondaryForeignKey.Properties
                                : secondaryForeignKey.PrincipalKey.Properties,
                            makeNullable: true);
                        var outerKeySelector = Expression.Lambda(outerKey, outerKeyparameter);

                        var innerSourceElementType = innerSource.Type.GetSequenceType();
                        var innerKeyParameter = Expression.Parameter(innerSourceElementType);
                        var innerKey = innerKeyParameter.CreateKeyValuesExpression(
                            !inverseNavigation.IsOnDependent
                                ? secondaryForeignKey.PrincipalKey.Properties
                                : secondaryForeignKey.Properties,
                            makeNullable: true);
                        var innerKeySelector = Expression.Lambda(innerKey, innerKeyParameter);

                        var resultSelector = Expression.Lambda(innerKeyParameter, outerKeyparameter, innerKeyParameter);

                        var innerJoin = !inverseNavigation.IsOnDependent && secondaryForeignKey.IsRequired;

                        secondaryExpansion = Expression.Call(
                            (innerJoin
                                ? QueryableMethods.Join
                                : QueryableExtensions.LeftJoinMethodInfo).MakeGenericMethod(
                                sourceElementType, innerSourceElementType,
                                outerKeySelector.ReturnType,
                                resultSelector.ReturnType),
                            primaryExpansion,
                            innerSource,
                            Expression.Quote(outerKeySelector),
                            Expression.Quote(innerKeySelector),
                            Expression.Quote(resultSelector));
                    }
                    else
                    {
                        // Second psuedo-navigation is a collection
                        var secondTargetType = navigation.TargetEntityType;
                        var innerQueryable = new QueryRootExpression(secondTargetType);
                        var innerSource = (NavigationExpansionExpression)_navigationExpandingExpressionVisitor.Visit(innerQueryable);

                        if (includeTree != null)
                        {
                            UnwrapEntityReference(innerSource.PendingSelector).IncludePaths.Merge(includeTree);
                        }

                        var sourceElementType = primaryExpansion.Type.GetSequenceType();
                        var outersourceParameter = Expression.Parameter(sourceElementType);
                        var outerKey = outersourceParameter.CreateKeyValuesExpression(
                            !inverseNavigation.IsOnDependent
                                ? secondaryForeignKey.Properties
                                : secondaryForeignKey.PrincipalKey.Properties,
                            makeNullable: true);

                        var innerSourceElementType = innerSource.Type.GetSequenceType();
                        var innerSourceParameter = Expression.Parameter(innerSourceElementType);
                        var innerKey = innerSourceParameter.CreateKeyValuesExpression(
                            !inverseNavigation.IsOnDependent
                                ? secondaryForeignKey.PrincipalKey.Properties
                                : secondaryForeignKey.Properties,
                            makeNullable: true);

                        // Selector body is IQueryable, we need to adjust the type to IEnumerable, to match the SelectMany signature
                        // therefore the delegate type is specified explicitly
                        var selectorLambdaType = typeof(Func<,>).MakeGenericType(
                            sourceElementType,
                            typeof(IEnumerable<>).MakeGenericType(innerSourceElementType));

                        var selector = Expression.Lambda(
                            selectorLambdaType,
                            Expression.Call(
                                QueryableMethods.Where.MakeGenericMethod(innerSourceElementType),
                                innerSource,
                                Expression.Quote(Expression.Lambda(Expression.Equal(outerKey, innerKey), innerSourceParameter))),
                            outersourceParameter);

                        secondaryExpansion = Expression.Call(
                            QueryableMethods.SelectManyWithoutCollectionSelector.MakeGenericMethod(
                                sourceElementType, innerSourceElementType),
                            primaryExpansion,
                            Expression.Quote(selector));
                    }
                }

                return navigation.IsCollection
                    ? new MaterializeCollectionNavigationExpression(secondaryExpansion, navigation)
                    : secondaryExpansion;
            }

            private Expression ExpandForeignKey(
                Expression root,
                EntityReference entityReference,
                IForeignKey foreignKey,
                bool onDependent,
                bool derivedTypeConversion)
            {
                if (entityReference.ForeignKeyExpansionMap.TryGetValue((foreignKey, onDependent), out var expansion))
                {
                    return expansion;
                }

                var collection = !foreignKey.IsUnique && !onDependent;
                var targetType = onDependent ? foreignKey.PrincipalEntityType : foreignKey.DeclaringEntityType;

                Debug.Assert(!targetType.HasDefiningNavigation() && !targetType.IsOwned(), "Owned entity expanding foreign key.");

                var innerQueryable = new QueryRootExpression(targetType);
                var innerSource = (NavigationExpansionExpression)_navigationExpandingExpressionVisitor.Visit(innerQueryable);

                // We detect and copy over include for navigation being expanded automatically
                var navigation = onDependent ? foreignKey.DependentToPrincipal : foreignKey.PrincipalToDependent;
                var innerEntityReference = UnwrapEntityReference(innerSource.PendingSelector);
                if (navigation != null
                    && entityReference.IncludePaths.TryGetValue(navigation, out var includeTree))
                {
                    {
                        innerEntityReference.IncludePaths.Merge(includeTree);
                    }
                }

                var innerSourceSequenceType = innerSource.Type.GetSequenceType();
                var innerParameter = Expression.Parameter(innerSourceSequenceType, "i");
                Expression outerKey;
                if (root is NavigationExpansionExpression innerNavigationExpansionExpression
                    && innerNavigationExpansionExpression.CardinalityReducingGenericMethodInfo != null)
                {
                    // This is FirstOrDefault ending so we need to push down properties.
                    var temporaryParameter = Expression.Parameter(root.Type);
                    var temporaryKey = temporaryParameter.CreateKeyValuesExpression(
                        onDependent
                            ? foreignKey.Properties
                            : foreignKey.PrincipalKey.Properties,
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
                    outerKey = root.CreateKeyValuesExpression(
                        onDependent ? foreignKey.Properties : foreignKey.PrincipalKey.Properties, makeNullable: true);
                }

                var innerKey = innerParameter.CreateKeyValuesExpression(
                    onDependent ? foreignKey.PrincipalKey.Properties : foreignKey.Properties, makeNullable: true);

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

                if (collection)
                {
                    // This is intentionally deferred to be applied to innerSource.Source
                    // Since outerKey's reference could change if a reference navigation is expanded afterwards
                    var predicateBody = Expression.AndAlso(
                        outerKey is NewArrayExpression newArrayExpression
                            ? newArrayExpression.Expressions
                                .Select(
                                    e =>
                                    {
                                        var left = (e as UnaryExpression)?.Operand ?? e;

                                        return Expression.NotEqual(left, Expression.Constant(null, left.Type));
                                    })
                                .Aggregate((l, r) => Expression.AndAlso(l, r))
                            : Expression.NotEqual(outerKey, Expression.Constant(null, outerKey.Type)),
                        Expression.Call(_objectEqualsMethodInfo, AddConvertToObject(outerKey), AddConvertToObject(innerKey)));

                    // Caller should take care of wrapping MaterializeCollectionNavigation
                    return Expression.Call(
                        QueryableMethods.Where.MakeGenericMethod(innerSourceSequenceType),
                        innerSource,
                        Expression.Quote(
                            Expression.Lambda(
                                predicateBody, innerParameter)));
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
                    && onDependent
                    && foreignKey.IsRequired;

                if (!innerJoin)
                {
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
                        Expression.Quote(outerKeySelector),
                        Expression.Quote(innerKeySelector),
                        Expression.Quote(resultSelector)));

                entityReference.ForeignKeyExpansionMap[(foreignKey, onDependent)] = innerSource.PendingSelector;

                _source.UpdateCurrentTree(new NavigationTreeNode(_source.CurrentTree, innerSource.CurrentTree));

                return innerSource.PendingSelector;
            }

            private static Expression AddConvertToObject(Expression expression)
                => expression.Type.IsValueType
                    ? Expression.Convert(expression, typeof(object))
                    : expression;
        }

        /// <summary>
        ///     Expands an include tree. This is separate and needed because we may need to reconstruct parts of
        ///     <see cref="NewExpression" /> to apply includes.
        /// </summary>
        private sealed class IncludeExpandingExpressionVisitor : ExpandingExpressionVisitor
        {
            private static readonly MethodInfo _fetchJoinEntityMethodInfo =
                typeof(IncludeExpandingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(FetchJoinEntity));

            private readonly bool _queryStateManager;
            private readonly bool _ignoreAutoIncludes;
            private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

            public IncludeExpandingExpressionVisitor(
                NavigationExpandingExpressionVisitor navigationExpandingExpressionVisitor,
                NavigationExpansionExpression source)
                : base(navigationExpandingExpressionVisitor, source)
            {
                _logger = navigationExpandingExpressionVisitor._queryCompilationContext.Logger;
                _queryStateManager = navigationExpandingExpressionVisitor._queryCompilationContext.QueryTrackingBehavior
                    == QueryTrackingBehavior.TrackAll
                    || navigationExpandingExpressionVisitor._queryCompilationContext.QueryTrackingBehavior
                    == QueryTrackingBehavior.NoTrackingWithIdentityResolution;
                _ignoreAutoIncludes = navigationExpandingExpressionVisitor._queryCompilationContext.IgnoreAutoIncludes;
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
                        entityType = entityType.GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive())
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
                if (!_queryStateManager)
                {
                    VerifyNoCycles(entityReference.IncludePaths);
                }

                return ExpandIncludesHelper(root, entityReference, previousNavigation: null);
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

            private Expression ExpandIncludesHelper(Expression root, EntityReference entityReference, INavigationBase previousNavigation)
            {
                var result = root;
                var convertedRoot = root;
                foreach (var kvp in entityReference.IncludePaths)
                {
                    var navigationBase = kvp.Key;
                    if (!navigationBase.IsCollection
                        && previousNavigation?.Inverse == navigationBase)
                    {
                        continue;
                    }

                    var converted = false;
                    if (entityReference.EntityType != navigationBase.DeclaringEntityType
                        && entityReference.EntityType.IsAssignableFrom(navigationBase.DeclaringEntityType))
                    {
                        converted = true;
                        convertedRoot = Expression.Convert(root, navigationBase.DeclaringEntityType.ClrType);
                    }

                    var included = navigationBase switch
                    {
                        INavigation navigation => ExpandNavigation(convertedRoot, entityReference, navigation, converted),
                        ISkipNavigation skipNavigation => ExpandSkipNavigation(convertedRoot, entityReference, skipNavigation, converted),
                        _ => throw new InvalidOperationException(CoreStrings.UnhandledNavigationBase(navigationBase.GetType())),
                    };

                    _logger.NavigationBaseIncluded(navigationBase);

                    // Collection will expand it's includes when reducing the navigationExpansionExpression
                    if (!navigationBase.IsCollection)
                    {
                        var innerEntityReference = UnwrapEntityReference(included);

                        included = ExpandIncludesHelper(included, innerEntityReference, navigationBase);
                    }
                    else
                    {
                        var materializeCollectionNavigation = (MaterializeCollectionNavigationExpression)included;
                        var subquery = materializeCollectionNavigation.Subquery;
                        if (!_ignoreAutoIncludes
                            && navigationBase is INavigation
                            && navigationBase.Inverse != null
                            && subquery is MethodCallExpression subqueryMethodCallExpression
                            && subqueryMethodCallExpression.Method.IsGenericMethod)
                        {
                            EntityReference innerEntityReference = null;
                            if (subqueryMethodCallExpression.Method.GetGenericMethodDefinition() == QueryableMethods.Where
                                && subqueryMethodCallExpression.Arguments[0] is NavigationExpansionExpression navigationExpansionExpression)
                            {
                                innerEntityReference = UnwrapEntityReference(navigationExpansionExpression.CurrentTree);
                            }
                            else if (subqueryMethodCallExpression.Method.GetGenericMethodDefinition() == QueryableMethods.AsQueryable)
                            {
                                innerEntityReference = UnwrapEntityReference(subqueryMethodCallExpression.Arguments[0]);
                            }

                            if (innerEntityReference != null)
                            {
                                innerEntityReference.IncludePaths.Remove(navigationBase.Inverse);
                            }
                        }

                        var filterExpression = entityReference.IncludePaths[navigationBase].FilterExpression;
                        if (_queryStateManager
                            && navigationBase is ISkipNavigation
                            && subquery is MethodCallExpression joinMethodCallExpression
                            && joinMethodCallExpression.Method.IsGenericMethod
                            && joinMethodCallExpression.Method.GetGenericMethodDefinition() == QueryableMethods.Join
                            && joinMethodCallExpression.Arguments[4] is UnaryExpression unaryExpression
                            && unaryExpression.NodeType == ExpressionType.Quote
                            && unaryExpression.Operand is LambdaExpression resultSelectorLambda
                            && resultSelectorLambda.Body == resultSelectorLambda.Parameters[1])
                        {
                            var joinParameter = resultSelectorLambda.Parameters[0];
                            var targetParameter = resultSelectorLambda.Parameters[1];
                            if (filterExpression == null)
                            {
                                var newResultSelector = Expression.Quote(
                                    Expression.Lambda(
                                        Expression.Call(
                                            _fetchJoinEntityMethodInfo.MakeGenericMethod(joinParameter.Type, targetParameter.Type),
                                            joinParameter,
                                            targetParameter),
                                        joinParameter,
                                        targetParameter));

                                subquery = joinMethodCallExpression.Update(
                                    null, joinMethodCallExpression.Arguments.Take(4).Append(newResultSelector));
                            }
                            else
                            {
                                var resultType = TransparentIdentifierFactory.Create(joinParameter.Type, targetParameter.Type);

                                var transparentIdentifierOuterMemberInfo = resultType.GetTypeInfo().GetDeclaredField("Outer");
                                var transparentIdentifierInnerMemberInfo = resultType.GetTypeInfo().GetDeclaredField("Inner");

                                var newResultSelector = Expression.Quote(
                                    Expression.Lambda(
                                        Expression.New(
                                            resultType.GetConstructors().Single(),
                                            new[] { joinParameter, targetParameter },
                                            transparentIdentifierOuterMemberInfo,
                                            transparentIdentifierInnerMemberInfo),
                                        joinParameter,
                                        targetParameter));

                                var joinTypeParameters = joinMethodCallExpression.Method.GetGenericArguments();
                                joinTypeParameters[3] = resultType;
                                subquery = Expression.Call(
                                    QueryableMethods.Join.MakeGenericMethod(joinTypeParameters),
                                    joinMethodCallExpression.Arguments.Take(4).Append(newResultSelector));

                                var transparentIdentifierParameter = Expression.Parameter(resultType);
                                var transparentIdentifierInnerAccessor = Expression.MakeMemberAccess(
                                    transparentIdentifierParameter, transparentIdentifierInnerMemberInfo);

                                subquery = RemapFilterExpressionForJoinEntity(
                                    filterExpression.Parameters[0],
                                    filterExpression.Body,
                                    subquery,
                                    transparentIdentifierParameter,
                                    transparentIdentifierInnerAccessor);

                                var selector = Expression.Quote(
                                    Expression.Lambda(
                                        Expression.Call(
                                            _fetchJoinEntityMethodInfo.MakeGenericMethod(joinParameter.Type, targetParameter.Type),
                                            Expression.MakeMemberAccess(
                                                transparentIdentifierParameter, transparentIdentifierOuterMemberInfo),
                                            transparentIdentifierInnerAccessor),
                                        transparentIdentifierParameter));

                                subquery = Expression.Call(
                                    QueryableMethods.Select.MakeGenericMethod(resultType, targetParameter.Type),
                                    subquery,
                                    selector);
                            }

                            included = materializeCollectionNavigation.Update(subquery);
                        }
                        else if (filterExpression != null)
                        {
                            subquery = ReplacingExpressionVisitor.Replace(filterExpression.Parameters[0], subquery, filterExpression.Body);
                            included = materializeCollectionNavigation.Update(subquery);
                        }
                    }

                    result = new IncludeExpression(result, included, navigationBase);
                }

                return result;
            }

#pragma warning disable IDE0060 // Remove unused parameter
            private static TTarget FetchJoinEntity<TJoin, TTarget>(TJoin joinEntity, TTarget targetEntity)
                => targetEntity;
#pragma warning restore IDE0060 // Remove unused parameter

            private static Expression RemapFilterExpressionForJoinEntity(
                ParameterExpression filterParameter,
                Expression filterExpressionBody,
                Expression subquery,
                ParameterExpression transparentIdentifierParameter,
                Expression transparentIdentifierInnerAccessor)
            {
                if (filterExpressionBody == filterParameter)
                {
                    return subquery;
                }

                var methodCallExpression = (MethodCallExpression)filterExpressionBody;
                var arguments = methodCallExpression.Arguments.ToArray();
                arguments[0] = RemapFilterExpressionForJoinEntity(
                    filterParameter, arguments[0], subquery, transparentIdentifierParameter, transparentIdentifierInnerAccessor);
                var genericParameters = methodCallExpression.Method.GetGenericArguments();
                genericParameters[0] = transparentIdentifierParameter.Type;
                var method = methodCallExpression.Method.GetGenericMethodDefinition().MakeGenericMethod(genericParameters);

                if (arguments.Length == 2
                    && arguments[1].GetLambdaOrNull() is LambdaExpression lambdaExpression)
                {
                    arguments[1] = Expression.Quote(
                        Expression.Lambda(
                            ReplacingExpressionVisitor.Replace(
                                lambdaExpression.Parameters[0], transparentIdentifierInnerAccessor, lambdaExpression.Body),
                            transparentIdentifierParameter));
                }

                return Expression.Call(method, arguments);
            }
        }

        /// <summary>
        ///     <see cref="NavigationExpansionExpression" /> remembers the pending selector so we don't expand
        ///     navigations unless we need to. This visitor applies them when we need to.
        /// </summary>
        private sealed class PendingSelectorExpandingExpressionVisitor : ExpressionVisitor
        {
            private readonly NavigationExpandingExpressionVisitor _visitor;
            private readonly bool _applyIncludes;

            public PendingSelectorExpandingExpressionVisitor(
                NavigationExpandingExpressionVisitor visitor,
                bool applyIncludes = false)
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
        ///     Marks <see cref="EntityReference" /> as nullable when coming from a left join.
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
                                entityType = entityType.GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive())
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

                public override Type Type
                    => Current.Type;

                public override ExpressionType NodeType
                    => ExpressionType.Extension;

                public INavigation Navigation { get; }
                public Expression Current { get; }
                public NavigationDataExpression Inner { get; }
                public IEntityType EntityType { get; }
            }
        }
    }
}
