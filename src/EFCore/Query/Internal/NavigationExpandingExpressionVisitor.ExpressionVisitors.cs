// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Infrastructure.ExpressionExtensions;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

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
        private readonly INavigationExpansionExtensibilityHelper _extensibilityHelper;

        public ExpandingExpressionVisitor(
            NavigationExpandingExpressionVisitor navigationExpandingExpressionVisitor,
            NavigationExpansionExpression source,
            INavigationExpansionExtensibilityHelper extensibilityHelper)
        {
            _navigationExpandingExpressionVisitor = navigationExpandingExpressionVisitor;
            _source = source;
            _extensibilityHelper = extensibilityHelper;
            Model = navigationExpandingExpressionVisitor._queryCompilationContext.Model;
        }

        public Expression Expand(Expression expression, bool applyIncludes = false)
        {
            expression = Visit(expression);
            if (applyIncludes)
            {
                expression = new IncludeExpandingExpressionVisitor(_navigationExpandingExpressionVisitor, _source, _extensibilityHelper)
                    .Visit(expression);
            }

            return expression;
        }

        protected IModel Model { get; }

        protected override Expression VisitExtension(Expression expression)
        {
            switch (expression)
            {
                case NavigationExpansionExpression:
                case NavigationTreeExpression:
                    return expression;

                default:
                    return base.VisitExtension(expression);
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

            if (methodCallExpression.TryGetIndexerArguments(Model, out source, out navigationName))
            {
                source = Visit(source);
                return TryExpandNavigation(source, MemberIdentity.Create(navigationName))
                    ?? methodCallExpression.Update(source, new[] { methodCallExpression.Arguments[0] });
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        private Expression? TryExpandNavigation(Expression? root, MemberIdentity memberIdentity)
        {
            if (root == null)
            {
                return null;
            }

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

                var navigation = memberIdentity.MemberInfo is not null
                    ? entityType.FindNavigation(memberIdentity.MemberInfo)
                    : memberIdentity.Name is not null
                        ? entityType.FindNavigation(memberIdentity.Name)
                        : null;
                if (navigation is not null)
                {
                    return ExpandNavigation(root, entityReference, navigation, convertedType is not null);
                }

                var skipNavigation = memberIdentity.MemberInfo is not null
                    ? entityType.FindSkipNavigation(memberIdentity.MemberInfo)
                    : memberIdentity.Name is not null
                        ? entityType.FindSkipNavigation(memberIdentity.Name)
                        : null;
                if (skipNavigation is not null)
                {
                    return ExpandSkipNavigation(root, entityReference, skipNavigation, convertedType is not null);
                }

                var property = memberIdentity.MemberInfo != null
                    ? entityType.FindProperty(memberIdentity.MemberInfo)
                    : memberIdentity.Name is not null
                        ? entityType.FindProperty(memberIdentity.Name)
                        : null;
                if (property?.IsPrimitiveCollection == true)
                {
                    return new PrimitiveCollectionReference(root, property);
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
            if (targetType.IsOwned())
            {
                if (entityReference.ForeignKeyExpansionMap.TryGetValue(
                        (navigation.ForeignKey, navigation.IsOnDependent), out var ownedExpansion))
                {
                    return ownedExpansion;
                }

                // make sure that we can actually expand this navigation (later)
                _extensibilityHelper.ValidateQueryRootCreation(targetType, entityReference.EntityQueryRootExpression);

                var ownedEntityReference = new EntityReference(targetType, entityReference.EntityQueryRootExpression);
                _navigationExpandingExpressionVisitor.PopulateEagerLoadedNavigations(ownedEntityReference.IncludePaths);
                ownedEntityReference.MarkAsOptional();
                if (entityReference.IncludePaths.TryGetValue(navigation, out var includePath))
                {
                    ownedEntityReference.IncludePaths.Merge(includePath);
                }

                ownedExpansion = new OwnedNavigationReference(root, navigation, ownedEntityReference);
                if (navigation.IsCollection)
                {
                    var elementType = ownedExpansion.Type.GetSequenceType();
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
                // ExpandFK handles both collection & reference navigation for second pseudo-navigation
                // Value known to be non-null
                secondaryExpansion = ExpandForeignKey(
                    primaryExpansion, UnwrapEntityReference(primaryExpansion)!, inverseNavigation.ForeignKey,
                    !inverseNavigation.IsOnDependent, derivedTypeConversion: false);
            }
            else
            {
                var secondaryForeignKey = inverseNavigation.ForeignKey;
                // First pseudo-navigation is a collection
                if (secondaryForeignKey.IsUnique
                    || !inverseNavigation.IsOnDependent)
                {
                    // Second pseudo-navigation is a reference
                    var secondTargetType = navigation.TargetEntityType;
                    // we can use the entity reference here. If the join entity wasn't temporal,
                    // the query root creation validator would have thrown the exception when it was being created
                    _extensibilityHelper.ValidateQueryRootCreation(secondTargetType, entityReference.EntityQueryRootExpression);
                    var innerQueryable = _extensibilityHelper.CreateQueryRoot(secondTargetType, entityReference.EntityQueryRootExpression);
                    var innerSource = (NavigationExpansionExpression)_navigationExpandingExpressionVisitor.Visit(innerQueryable);

                    if (includeTree != null)
                    {
                        // Value known to be non-null
                        UnwrapEntityReference(innerSource.PendingSelector)!.IncludePaths.Merge(includeTree);
                    }

                    var sourceElementType = primaryExpansion.Type.GetSequenceType();
                    var outerKeyParameter = Expression.Parameter(sourceElementType);
                    var outerKey = outerKeyParameter.CreateKeyValuesExpression(
                        !inverseNavigation.IsOnDependent
                            ? secondaryForeignKey.Properties
                            : secondaryForeignKey.PrincipalKey.Properties,
                        makeNullable: true);
                    var outerKeySelector = Expression.Lambda(outerKey, outerKeyParameter);

                    var innerSourceElementType = innerSource.Type.GetSequenceType();
                    var innerKeyParameter = Expression.Parameter(innerSourceElementType);
                    var innerKey = innerKeyParameter.CreateKeyValuesExpression(
                        !inverseNavigation.IsOnDependent
                            ? secondaryForeignKey.PrincipalKey.Properties
                            : secondaryForeignKey.Properties,
                        makeNullable: true);
                    var innerKeySelector = Expression.Lambda(innerKey, innerKeyParameter);

                    var resultSelector = Expression.Lambda(innerKeyParameter, outerKeyParameter, innerKeyParameter);

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
                    // Second pseudo-navigation is a collection
                    var secondTargetType = navigation.TargetEntityType;

                    _extensibilityHelper.ValidateQueryRootCreation(secondTargetType, entityReference.EntityQueryRootExpression);
                    var innerQueryable = _extensibilityHelper.CreateQueryRoot(secondTargetType, entityReference.EntityQueryRootExpression);
                    var innerSource = (NavigationExpansionExpression)_navigationExpandingExpressionVisitor.Visit(innerQueryable);

                    if (includeTree != null)
                    {
                        // Value known to be non-null
                        UnwrapEntityReference(innerSource.PendingSelector)!.IncludePaths.Merge(includeTree);
                    }

                    var sourceElementType = primaryExpansion.Type.GetSequenceType();
                    var outerSourceParameter = Expression.Parameter(sourceElementType);
                    var outerKey = outerSourceParameter.CreateKeyValuesExpression(
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
                            Expression.Quote(
                                Expression.Lambda(
                                    ExpressionExtensions.CreateEqualsExpression(outerKey, innerKey), innerSourceParameter))),
                        outerSourceParameter);

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
            var navigation = onDependent ? foreignKey.DependentToPrincipal : foreignKey.PrincipalToDependent;
            if (entityReference.ForeignKeyExpansionMap.TryGetValue((foreignKey, onDependent), out var expansion))
            {
                if (navigation != null
                    && entityReference.IncludePaths.TryGetValue(navigation, out var pendingIncludeTree))
                {
                    var cachedEntityReference = UnwrapEntityReference(expansion);
                    cachedEntityReference?.IncludePaths.Merge(pendingIncludeTree);
                }

                return expansion;
            }

            var collection = !foreignKey.IsUnique && !onDependent;
            var targetType = onDependent ? foreignKey.PrincipalEntityType : foreignKey.DeclaringEntityType;

            Check.DebugAssert(!targetType.IsOwned(), "Owned entity expanding foreign key.");

            _extensibilityHelper.ValidateQueryRootCreation(targetType, entityReference.EntityQueryRootExpression);
            var innerQueryable = _extensibilityHelper.CreateQueryRoot(targetType, entityReference.EntityQueryRootExpression);
            var innerSource = (NavigationExpansionExpression)_navigationExpandingExpressionVisitor.Visit(innerQueryable);

            // Value known to be non-null
            var innerEntityReference = UnwrapEntityReference(innerSource.PendingSelector)!;

            // We detect and copy over include for navigation being expanded automatically
            if (navigation != null
                && entityReference.IncludePaths.TryGetValue(navigation, out var includeTree))
            {
                innerEntityReference.IncludePaths.Merge(includeTree);
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
                            .Aggregate(Expression.AndAlso)
                        : Expression.NotEqual(outerKey, Expression.Constant(null, outerKey.Type)),
                    ExpressionExtensions.CreateEqualsExpression(outerKey, innerKey));

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

            var transparentIdentifierOuterMemberInfo = resultType.GetTypeInfo().GetDeclaredField("Outer")!;
            var transparentIdentifierInnerMemberInfo = resultType.GetTypeInfo().GetDeclaredField("Inner")!;

            var resultSelector = Expression.Lambda(
                Expression.New(
                    resultType.GetConstructors().Single(),
                    new[] { resultSelectorOuterParameter, resultSelectorInnerParameter },
                    transparentIdentifierOuterMemberInfo,
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
    }

    /// <summary>
    ///     Expands an include tree. This is separate and needed because we may need to reconstruct parts of
    ///     <see cref="NewExpression" /> to apply includes.
    /// </summary>
    private sealed class IncludeExpandingExpressionVisitor : ExpandingExpressionVisitor
    {
        private static readonly MethodInfo FetchJoinEntityMethodInfo =
            typeof(IncludeExpandingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(FetchJoinEntity))!;

        private readonly bool _queryStateManager;
        private readonly bool _ignoreAutoIncludes;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

        public IncludeExpandingExpressionVisitor(
            NavigationExpandingExpressionVisitor navigationExpandingExpressionVisitor,
            NavigationExpansionExpression source,
            INavigationExpansionExtensibilityHelper extensibilityHelper)
            : base(navigationExpandingExpressionVisitor, source, extensibilityHelper)
        {
            _logger = navigationExpandingExpressionVisitor._queryCompilationContext.Logger;
            _queryStateManager = navigationExpandingExpressionVisitor._queryCompilationContext.QueryTrackingBehavior is
                QueryTrackingBehavior.TrackAll or QueryTrackingBehavior.NoTrackingWithIdentityResolution;
            _ignoreAutoIncludes = navigationExpandingExpressionVisitor._queryCompilationContext.IgnoreAutoIncludes;
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            if (binaryExpression.NodeType is ExpressionType.Equal or ExpressionType.NotEqual)
            {
                // This could be entity equality. We don't want to expand include nodes over them
                // as either they translate or throw.
                var leftEntityReference = IsEntityReference(binaryExpression.Left);
                var rightEntityReference = IsEntityReference(binaryExpression.Right);
                if (leftEntityReference || rightEntityReference)
                {
                    return binaryExpression;
                }
            }

            return base.VisitBinary(binaryExpression);

            bool IsEntityReference(Expression expression)
                => TryGetEntityType(expression) != null;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
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

                case MaterializeCollectionNavigationExpression:
                case IncludeExpression:
                    return extensionExpression;
            }

            return base.VisitExtension(extensionExpression);
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            if (memberExpression.Expression != null)
            {
                // If it is mapped property then, it would get converted to a column so we don't need to expand includes.
                var entityType = TryGetEntityType(memberExpression.Expression);
                var property = entityType?.FindProperty(memberExpression.Member);
                if (property != null)
                {
                    return memberExpression;
                }
            }

            return base.VisitMember(memberExpression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.TryGetEFPropertyArguments(out _, out _))
            {
                // If it is EF.Property then, it would get converted to a column or throw
                // so we don't need to expand includes.
                return methodCallExpression;
            }

            if (methodCallExpression.TryGetIndexerArguments(Model, out var source, out var propertyName))
            {
                // If it is mapped property then, it would get converted to a column so we don't need to expand includes.
                var entityType = TryGetEntityType(source);
                var property = entityType?.FindProperty(propertyName);
                if (property != null)
                {
                    return methodCallExpression;
                }
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

        protected override Expression VisitTypeBinary(TypeBinaryExpression typeBinaryExpression)
            => typeBinaryExpression;

        private static IEntityType? TryGetEntityType(Expression expression)
        {
            var innerExpression = expression.UnwrapTypeConversion(out var convertedType);
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

                return entityType;
            }

            return null;
        }

        private bool ReconstructAnonymousType(
            Expression currentRoot,
            NewExpression newExpression,
            [NotNullWhen(true)] out Expression? replacement)
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
                var newRoot = Expression.MakeMemberAccess(currentRoot, newExpression.Members![i]);
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

        private static void VerifyNoCycles(IncludeTreeNode includeTreeNode)
        {
            foreach (var (navigation, referenceIncludeTreeNode) in includeTreeNode)
            {
                var inverseNavigation = navigation.Inverse;
                if (inverseNavigation != null
                    && referenceIncludeTreeNode.ContainsKey(inverseNavigation))
                {
                    throw new InvalidOperationException(CoreStrings.IncludeWithCycle(navigation.Name, inverseNavigation.Name));
                }

                VerifyNoCycles(referenceIncludeTreeNode);
            }
        }

        private Expression ExpandIncludesHelper(Expression root, EntityReference entityReference, INavigationBase? previousNavigation)
        {
            var result = root;
            var convertedRoot = root;
            foreach (var (navigationBase, includeTreeNode) in entityReference.IncludePaths)
            {
                if (!navigationBase.IsCollection
                    && previousNavigation?.Inverse == navigationBase)
                {
                    // This skips one-to-one navigations which are pointing to each other.
                    if (!navigationBase.IsEagerLoaded)
                    {
                        _logger.NavigationBaseIncludeIgnored(navigationBase);
                    }

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
                    _ => throw new InvalidOperationException(CoreStrings.UnhandledNavigationBase(navigationBase.GetType()))
                };

                _logger.NavigationBaseIncluded(navigationBase);

                // Collection will expand it's includes when reducing the navigationExpansionExpression
                if (!navigationBase.IsCollection)
                {
                    // Value known to be non-null
                    included = ExpandIncludesHelper(included, UnwrapEntityReference(included)!, navigationBase);
                }
                else
                {
                    var materializeCollectionNavigation = (MaterializeCollectionNavigationExpression)included;
                    var subquery = materializeCollectionNavigation.Subquery;
                    if (!_ignoreAutoIncludes
                        && navigationBase is INavigation
                        && navigationBase.Inverse is INavigation inverseNavigation
                        && subquery is MethodCallExpression { Method.IsGenericMethod: true } subqueryMethodCallExpression)
                    {
                        EntityReference? innerEntityReference = null;
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
                            // This skips inverse navigation of a collection navigation if they are pointing to each other.
                            // Not a skip navigation
                            if (innerEntityReference.IncludePaths.ContainsKey(inverseNavigation)
                                && !inverseNavigation.IsEagerLoaded)
                            {
                                _logger.NavigationBaseIncludeIgnored(inverseNavigation);
                            }

                            innerEntityReference.IncludePaths.Remove(inverseNavigation);
                        }
                    }

                    var filterExpression = entityReference.IncludePaths[navigationBase].FilterExpression;
                    if (_queryStateManager
                        && navigationBase is ISkipNavigation skipNavigation
                        && subquery is MethodCallExpression { Method.IsGenericMethod: true } joinMethodCallExpression
                        && joinMethodCallExpression.Method.GetGenericMethodDefinition()
                        == (skipNavigation.Inverse.ForeignKey.IsRequired
                            ? QueryableMethods.Join
                            : QueryableExtensions.LeftJoinMethodInfo)
                        && joinMethodCallExpression.Arguments[4] is UnaryExpression
                        {
                            NodeType: ExpressionType.Quote,
                            Operand: LambdaExpression resultSelectorLambda
                        }
                        && resultSelectorLambda.Body == resultSelectorLambda.Parameters[1])
                    {
                        var joinParameter = resultSelectorLambda.Parameters[0];
                        var targetParameter = resultSelectorLambda.Parameters[1];
                        if (filterExpression == null)
                        {
                            var newResultSelector = Expression.Quote(
                                Expression.Lambda(
                                    Expression.Call(
                                        FetchJoinEntityMethodInfo.MakeGenericMethod(joinParameter.Type, targetParameter.Type),
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

                            var transparentIdentifierOuterMemberInfo = resultType.GetTypeInfo().GetDeclaredField("Outer")!;
                            var transparentIdentifierInnerMemberInfo = resultType.GetTypeInfo().GetDeclaredField("Inner")!;

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
                                        FetchJoinEntityMethodInfo.MakeGenericMethod(joinParameter.Type, targetParameter.Type),
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

                result = new IncludeExpression(result, included, navigationBase, includeTreeNode.SetLoaded);
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
        private readonly INavigationExpansionExtensibilityHelper _extensibilityHelper;

        public PendingSelectorExpandingExpressionVisitor(
            NavigationExpandingExpressionVisitor visitor,
            INavigationExpansionExtensibilityHelper extensibilityHelper,
            bool applyIncludes = false)
        {
            _visitor = visitor;
            _extensibilityHelper = extensibilityHelper;
            _applyIncludes = applyIncludes;
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            if (expression is NavigationExpansionExpression navigationExpansionExpression)
            {
                _visitor.ApplyPendingOrderings(navigationExpansionExpression);

                var pendingSelector = new ExpandingExpressionVisitor(_visitor, navigationExpansionExpression, _extensibilityHelper)
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
        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
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
                        var arguments = new List<Expression> { result };
                        arguments.AddRange(navigationExpansionExpression.CardinalityReducingMethodArguments.Select(x => Visit(x)));

                        result = Expression.Call(
                            navigationExpansionExpression.CardinalityReducingGenericMethodInfo.MakeGenericMethod(
                                result.Type.GetSequenceType()),
                            arguments.ToArray());
                    }

                    return result;
                }

                case OwnedNavigationReference ownedNavigationReference:
                    return Visit(ownedNavigationReference.Parent).CreateEFPropertyExpression(ownedNavigationReference.Navigation);

                case PrimitiveCollectionReference queryablePropertyReference:
                    return Visit(queryablePropertyReference.Parent).CreateEFPropertyExpression(queryablePropertyReference.Property);

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
        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
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
            => extensionExpression is EntityQueryRootExpression entityQueryRootExpression
                && entityQueryRootExpression.EntityType == _entityType
                    ? _navigationExpandingExpressionVisitor.CreateNavigationExpansionExpression(entityQueryRootExpression, _entityType)
                    : base.VisitExtension(extensionExpression);
    }

    private sealed class CloningExpressionVisitor : ExpressionVisitor
    {
        private readonly Dictionary<NavigationTreeNode, NavigationTreeNode> _clonedMap = new(ReferenceEqualityComparer.Instance);

        public NavigationTreeNode Clone(NavigationTreeNode navigationTreeNode)
        {
            _clonedMap.Clear();

            return (NavigationTreeNode)Visit(navigationTreeNode);
        }

        public IReadOnlyDictionary<NavigationTreeNode, NavigationTreeNode> ClonedNodesMap
            => _clonedMap;

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            switch (expression)
            {
                case EntityReference entityReference:
                    return entityReference.Snapshot();

                case NavigationTreeExpression navigationTreeExpression:
                    if (!_clonedMap.TryGetValue(navigationTreeExpression, out var clonedNavigationTreeExpression))
                    {
                        clonedNavigationTreeExpression = new NavigationTreeExpression(Visit(navigationTreeExpression.Value));
                        _clonedMap[navigationTreeExpression] = clonedNavigationTreeExpression;
                    }

                    return clonedNavigationTreeExpression;

                case NavigationTreeNode navigationTreeNode:
                    if (!_clonedMap.TryGetValue(navigationTreeNode, out var clonedNavigationTreeNode))
                    {
                        clonedNavigationTreeNode = new NavigationTreeNode(
                            (NavigationTreeNode)Visit(navigationTreeNode.Left!),
                            (NavigationTreeNode)Visit(navigationTreeNode.Right!));
                        _clonedMap[navigationTreeNode] = clonedNavigationTreeNode;
                    }

                    return clonedNavigationTreeNode;

                default:
                    return base.Visit(expression);
            }
        }
    }

    private sealed class GroupingElementReplacingExpressionVisitor : ExpressionVisitor
    {
        private readonly CloningExpressionVisitor _cloningExpressionVisitor;
        private readonly ParameterExpression _parameterExpression;
        private readonly NavigationExpansionExpression _navigationExpansionExpression;
        private readonly Expression? _keyAccessExpression;
        private readonly MemberInfo? _keyMemberInfo;

        public GroupingElementReplacingExpressionVisitor(
            ParameterExpression parameterExpression,
            GroupByNavigationExpansionExpression groupByNavigationExpansionExpression)
        {
            _parameterExpression = parameterExpression;
            _navigationExpansionExpression = groupByNavigationExpansionExpression.GroupingEnumerable;
            _keyAccessExpression = Expression.MakeMemberAccess(
                groupByNavigationExpansionExpression.CurrentParameter,
                groupByNavigationExpansionExpression.CurrentParameter.Type.GetTypeInfo().GetDeclaredProperty(
                    nameof(IGrouping<int, int>.Key))!);
            _keyMemberInfo = parameterExpression.Type.GetTypeInfo().GetDeclaredProperty(nameof(IGrouping<int, int>.Key))!;
            _cloningExpressionVisitor = new CloningExpressionVisitor();
        }

        public GroupingElementReplacingExpressionVisitor(
            ParameterExpression parameterExpression,
            NavigationExpansionExpression navigationExpansionExpression)
        {
            _parameterExpression = parameterExpression;
            _navigationExpansionExpression = navigationExpansionExpression;
            _cloningExpressionVisitor = new CloningExpressionVisitor();
        }

        public bool ContainsGrouping { get; private set; }

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            if (expression == _parameterExpression)
            {
                ContainsGrouping = true;
            }

            return base.Visit(expression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.IsGenericMethod
                && (methodCallExpression.Method.GetGenericMethodDefinition() == QueryableMethods.AsQueryable
                    || methodCallExpression.Method.GetGenericMethodDefinition() == EnumerableMethods.ToList
                    || methodCallExpression.Method.GetGenericMethodDefinition() == EnumerableMethods.ToArray)
                && methodCallExpression.Arguments[0] == _parameterExpression)
            {
                var currentTree = _cloningExpressionVisitor.Clone(_navigationExpansionExpression.CurrentTree);

                var navigationExpansionExpression = new NavigationExpansionExpression(
                    _navigationExpansionExpression.Source,
                    currentTree,
                    new ReplacingExpressionVisitor(
                            _cloningExpressionVisitor.ClonedNodesMap.Keys.ToList(),
                            _cloningExpressionVisitor.ClonedNodesMap.Values.ToList())
                        .Visit(_navigationExpansionExpression.PendingSelector),
                    _navigationExpansionExpression.CurrentParameter.Name!);

                return methodCallExpression.Update(null, new[] { navigationExpansionExpression });
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
            => memberExpression.Member == _keyMemberInfo
                && memberExpression.Expression == _parameterExpression
                    ? _keyAccessExpression!
                    : base.VisitMember(memberExpression);
    }

    private sealed class RemoveRedundantNavigationComparisonExpressionVisitor : ExpressionVisitor
    {
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

        public RemoveRedundantNavigationComparisonExpressionVisitor(IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            _logger = logger;
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
            => binaryExpression.NodeType is ExpressionType.Equal or ExpressionType.NotEqual
                && TryRemoveNavigationComparison(
                    binaryExpression.NodeType, binaryExpression.Left, binaryExpression.Right, out var result)
                    ? result
                    : base.VisitBinary(binaryExpression);

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var method = methodCallExpression.Method;
            if (method.Name == nameof(object.Equals)
                && methodCallExpression is { Object: not null, Arguments.Count: 1 }
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

        private bool TryRemoveNavigationComparison(
            ExpressionType nodeType,
            Expression left,
            Expression right,
            [NotNullWhen(true)] out Expression? result)
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
                    ? rightNavigationData!
                    : leftNavigationData!;

                if (nonNullNavigationData.Navigation?.IsCollection == true)
                {
                    _logger.PossibleUnintendedCollectionNavigationNullComparisonWarning(nonNullNavigationData.Navigation);

                    // Inner would be non-null when navigation is non-null
                    result = Expression.MakeBinary(
                        nodeType, nonNullNavigationData.Inner!.Current, Expression.Constant(null, nonNullNavigationData.Inner.Type));

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
                        // Inner would be non-null when navigation is non-null
                        result = Expression.MakeBinary(nodeType, leftNavigationData.Inner!.Current, rightNavigationData.Inner!.Current);
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

        private static Expression ProcessNavigationPath(Expression expression)
        {
            switch (expression)
            {
                case MemberExpression { Expression: not null } memberExpression:
                    var innerExpression = ProcessNavigationPath(memberExpression.Expression);
                    if (innerExpression is NavigationDataExpression { EntityType: not null } navigationDataExpression)
                    {
                        var navigation = navigationDataExpression.EntityType.FindNavigation(memberExpression.Member);
                        if (navigation != null)
                        {
                            return new NavigationDataExpression(expression, navigationDataExpression, navigation);
                        }
                    }

                    return expression;

                case MethodCallExpression methodCallExpression
                    when methodCallExpression.TryGetEFPropertyArguments(out _, out _):
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

            public INavigation? Navigation { get; }
            public Expression Current { get; }
            public NavigationDataExpression? Inner { get; }
            public IEntityType? EntityType { get; }
        }
    }
}
