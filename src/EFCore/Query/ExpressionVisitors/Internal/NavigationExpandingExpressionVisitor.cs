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
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    public static class ExpressionExtenssions
    {
        public static Expression UnwrapQuote(this Expression expression)
            => expression is UnaryExpression unary && expression.NodeType == ExpressionType.Quote
            ? unary.Operand
            : expression;

        public static bool IsIncludeMethod(this MethodCallExpression methodCallExpression)
            => methodCallExpression.Method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                && methodCallExpression.Method.Name == nameof(EntityFrameworkQueryableExtensions.Include);
    }

    public readonly struct TransparentIdentifier<TOuter, TInner>
    {
        [UsedImplicitly]
        public TransparentIdentifier(TOuter outer, TInner inner)
        {
            Outer = outer;
            Inner = inner;
        }

        [UsedImplicitly]
        public readonly TOuter Outer;

        [UsedImplicitly]
        public readonly TInner Inner;
    }

    public class NavigationExpandingExpressionVisitor : LinqQueryExpressionVisitorBase
    {
        private IModel _model;

        public NavigationExpandingExpressionVisitor(IModel model)
        {
            _model = model;
        }

        public virtual Expression ExpandNavigations(Expression expression)
        {
            var collectionNaviagtionRewritingExpressionVisitor = new CollectionNavigationRewritingExpressionVisitor(_model);
            var newExpression = collectionNaviagtionRewritingExpressionVisitor.Visit(expression);

            newExpression = Visit(newExpression);

            return newExpression;
        }

        private Expression ProcessSelect(MethodCallExpression methodCallExpression)
        {
            var source = Visit(methodCallExpression.Arguments[0]);
            var selector = Visit(methodCallExpression.Arguments[1]);
            var parameter = (selector.UnwrapQuote() as LambdaExpression).Parameters[0];
            var transparentIdentifierAccessorMapping = new List<(List<string> from, List<string> to)>();
            var selectorMapping = new List<(List<string> from, List<INavigation> to)>();
            var foundNavigations = new List<NavigationPathNode>();
            var finalProjectionPath = new List<string>();
            var newParameter = parameter;

            if (source is NavigationExpansionExpression navigationExpansionExpression)
            {
                source = navigationExpansionExpression.Operand;
                newParameter = navigationExpansionExpression.ParameterExpression;
                transparentIdentifierAccessorMapping = navigationExpansionExpression.TransparentIdentifierAccessorMapping;
                selectorMapping = navigationExpansionExpression.SelectorMapping;
                foundNavigations = navigationExpansionExpression.FoundNavigations;
                finalProjectionPath = navigationExpansionExpression.FinalProjectionPath;
            }

            var nfev = new NavigationFindingExpressionVisitor(_model, parameter, selectorMapping, foundNavigations);
            nfev.Visit(selector);

            var result = (source, parameter: newParameter);

            if (nfev.FoundNavigationPaths.Any())
            {
                foreach (var navigationPath in nfev.FoundNavigationPaths)
                {
                    result = AddNavigationJoin(
                        result.source,
                        result.parameter,
                        navigationPath,
                        /*navigationPathNames*/ new List<string>(),
                        finalProjectionPath,
                        transparentIdentifierAccessorMapping);
                }
            }

            var foo = new Foo(_model, parameter, selectorMapping);
            foo.Visit(selector);

            selectorMapping = foo.NewSelectorMapping;

            var newSource = result.source;
            var nrev = new NavigationReplacingExpressionVisitor(
                _model,
                parameter,
                result.parameter,
                transparentIdentifierAccessorMapping,
                selectorMapping);

            var newSelector = nrev.Visit(selector);

            var newMethodInfo = QueryableSelectMethodInfo.MakeGenericMethod(
                result.parameter.Type,
                (selector.UnwrapQuote() as LambdaExpression).Body.Type);

            var rewritten = Expression.Call(newMethodInfo, newSource, newSelector);
            finalProjectionPath.Clear();
            transparentIdentifierAccessorMapping.Clear(); // is this correct?

            return new NavigationExpansionExpression(
                rewritten,
                result.parameter,
                transparentIdentifierAccessorMapping,
                selectorMapping,
                foundNavigations,
                finalProjectionPath,
                methodCallExpression.Type);
        }

        private class Foo : ExpressionVisitor
        {
            private IModel _model;
            private ParameterExpression _lambdaParameter;
            private List<(List<string> from, List<INavigation> to)> _selectorMapping;
            private List<string> _currentPath = new List<string>();

            public List<(List<string> from, List<INavigation> to)> NewSelectorMapping { get; }
                = new List<(List<string> from, List<INavigation> to)>();

            public Foo(
                IModel model,
                ParameterExpression lambdaParameter,
                List<(List<string> from, List<INavigation> to)> selectorMapping)
            {
                _model = model;
                _lambdaParameter = lambdaParameter;
                _selectorMapping = selectorMapping;
            }

            protected override Expression VisitNew(NewExpression newExpression)
            {
                for (var i = 0; i < newExpression.Arguments.Count; i++)
                {
                    _currentPath.Add(newExpression.Members[i].Name);
                    var argument = Visit(newExpression.Arguments[i]);

                    var properties = Binder.GetPropertyPath(
                        newExpression.Arguments[i],
                        _model,
                        _selectorMapping,
                        out var parameterExpression);

                    if (parameterExpression == _lambdaParameter
                        && properties.Any()
                        && properties.All(p => p is INavigation))
                    {
                        var to = properties.Cast<INavigation>().ToList();
                        NewSelectorMapping.Add((from: _currentPath.ToList(), to));
                    }

                    _currentPath.RemoveAt(_currentPath.Count - 1);
                }

                return newExpression;
            }
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.MethodIsClosedFormOf(QueryableSelectMethodInfo))
            {
                var result = ProcessSelect(methodCallExpression);

                return result;
            }









            if (methodCallExpression.Method.MethodIsClosedFormOf(QueryableWhereMethodInfo))
            {
                var source = Visit(methodCallExpression.Arguments[0]);
                var predicate = Visit(methodCallExpression.Arguments[1]);
                var parameter = (predicate.UnwrapQuote() as LambdaExpression).Parameters[0];
                var transparentIdentifierAccessorMapping = new List<(List<string> from, List<string> to)>();
                var selectorMapping = new List<(List<string> from, List<INavigation> to)>();
                var foundNavigations = new List<NavigationPathNode>();
                var finalProjectionPath = new List<string>();
                var newParameter = parameter;

                if (source is NavigationExpansionExpression navigationExpansionExpression)
                {
                    source = navigationExpansionExpression.Operand;
                    newParameter = navigationExpansionExpression.ParameterExpression;
                    transparentIdentifierAccessorMapping = navigationExpansionExpression.TransparentIdentifierAccessorMapping;
                    selectorMapping = navigationExpansionExpression.SelectorMapping;
                    foundNavigations = navigationExpansionExpression.FoundNavigations;
                    finalProjectionPath = navigationExpansionExpression.FinalProjectionPath;
                }

                var nfev = new NavigationFindingExpressionVisitor(_model, parameter, selectorMapping, foundNavigations);
                nfev.Visit(predicate);

                var result = (source, parameter: newParameter);

                if (nfev.FoundNavigationPaths.Any())
                {
                    foreach (var navigationPath in nfev.FoundNavigationPaths)
                    {
                        result = AddNavigationJoin(
                            result.source,
                            result.parameter,
                            navigationPath,
                            /*navigationPathNames*/ new List<string>(),
                            finalProjectionPath,
                            transparentIdentifierAccessorMapping);
                    }
                }

                var newSource = result.source;
                var nrev = new NavigationReplacingExpressionVisitor(
                    _model,
                    parameter,
                    result.parameter,
                    transparentIdentifierAccessorMapping,
                    selectorMapping);

                var newPredicate = nrev.Visit(predicate);

                var newMethodInfo = QueryableWhereMethodInfo.MakeGenericMethod(result.parameter.Type);

                var rewritten = Expression.Call(newMethodInfo, newSource, newPredicate);

                return new NavigationExpansionExpression(
                    rewritten,
                    result.parameter,
                    transparentIdentifierAccessorMapping,
                    selectorMapping,
                    foundNavigations,
                    finalProjectionPath,
                    methodCallExpression.Type);
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        // TODO: DRY
        private static Expression CreateKeyAccessExpression(
            Expression target, IReadOnlyList<IProperty> properties, bool addNullCheck = false)
            => properties.Count == 1
                ? CreatePropertyExpression(target, properties[0], addNullCheck)
                : Expression.New(
                    AnonymousObject.AnonymousObjectCtor,
                    Expression.NewArrayInit(
                        typeof(object),
                        properties
                            .Select(p => Expression.Convert(CreatePropertyExpression(target, p, addNullCheck), typeof(object)))
                            .Cast<Expression>()
                            .ToArray()));

        // TODO: DRY
        private static Expression CreatePropertyExpression(Expression target, IProperty property, bool addNullCheck)
        {
            var propertyExpression = target.CreateEFPropertyExpression(property, makeNullable: false);

            var propertyDeclaringType = property.DeclaringType.ClrType;
            if (propertyDeclaringType != target.Type
                && target.Type.GetTypeInfo().IsAssignableFrom(propertyDeclaringType.GetTypeInfo()))
            {
                if (!propertyExpression.Type.IsNullableType())
                {
                    propertyExpression = Expression.Convert(propertyExpression, propertyExpression.Type.MakeNullable());
                }

                return Expression.Condition(
                    Expression.TypeIs(target, propertyDeclaringType),
                    propertyExpression,
                    Expression.Constant(null, propertyExpression.Type));
            }

            return addNullCheck
                ? new NullConditionalExpression(target, propertyExpression)
                : propertyExpression;
        }

        private (Expression source, ParameterExpression parameter)  AddNavigationJoin(
            Expression sourceExpression,
            ParameterExpression parameterExpression,
            NavigationPathNode navigationPath,
            List<string> navigationPathNames,
            List<string> finalProjectionPath,
            List<(List<string> from, List<string> to)> transparentIdentifierAccessorMapping)
        {
            var path = navigationPath.GeneratePath();

            if (!transparentIdentifierAccessorMapping.Any(m => m.from.Count == path.Count && m.from.Zip(path, (o, i) => o == i).All(r => r)))
            {
                var navigation = navigationPath.Navigation;
                var sourceType = sourceExpression.Type.GetGenericArguments()[0];

                // is this the right way to get EntityTypes?
                var navigationTargetEntityType = navigation.IsDependentToPrincipal()
                    ? navigation.ForeignKey.PrincipalEntityType
                    : navigation.ForeignKey.DeclaringEntityType;

                var entityQueryable = NullAsyncQueryProvider.Instance.CreateEntityQueryableExpression(navigationTargetEntityType.ClrType);
                var resultType = typeof(TransparentIdentifier<,>).MakeGenericType(sourceType, navigationTargetEntityType.ClrType);

                var transparentIdentifierAccessorPath = transparentIdentifierAccessorMapping.Where(
                    m => m.from.Count == navigationPathNames.Count
                        && m.from.Zip(navigationPathNames, (o, i) => o == i).All(r => r)).SingleOrDefault().to;

                var outerParameter = Expression.Parameter(sourceType, parameterExpression.Name);
                var outerKeySelectorParameter = outerParameter;
                var transparentIdentifierAccessorExpression = BuildTransparentIdentifierAccessorExpression(outerParameter, transparentIdentifierAccessorPath);

                var outerKeySelectorBody = CreateKeyAccessExpression(
                    transparentIdentifierAccessorExpression,
                    navigation.IsDependentToPrincipal()
                        ? navigation.ForeignKey.Properties
                        : navigation.ForeignKey.PrincipalKey.Properties,
                    addNullCheck: navigationPath.Optional);

                var innerKeySelectorParameterType = navigationTargetEntityType.ClrType;
                var innerKeySelectorParameter = Expression.Parameter(
                    innerKeySelectorParameterType,
                    parameterExpression.Name + "." + navigationPath.Navigation.Name);

                var innerKeySelectorBody = CreateKeyAccessExpression(
                    innerKeySelectorParameter,
                    navigation.IsDependentToPrincipal()
                        ? navigation.ForeignKey.PrincipalKey.Properties
                        : navigation.ForeignKey.Properties);

                if (outerKeySelectorBody.Type.IsNullableType()
                    && !innerKeySelectorBody.Type.IsNullableType())
                {
                    innerKeySelectorBody = Expression.Convert(innerKeySelectorBody, outerKeySelectorBody.Type);
                }
                else if (innerKeySelectorBody.Type.IsNullableType()
                    && !outerKeySelectorBody.Type.IsNullableType())
                {
                    outerKeySelectorBody = Expression.Convert(outerKeySelectorBody, innerKeySelectorBody.Type);
                }

                var outerKeySelector = Expression.Lambda(
                    outerKeySelectorBody,
                    outerKeySelectorParameter);

                var innerKeySelector = Expression.Lambda(
                    innerKeySelectorBody,
                    innerKeySelectorParameter);

                if (navigationPath.Optional)
                {
                    var groupingType = typeof(IEnumerable<>).MakeGenericType(navigationTargetEntityType.ClrType);
                    var groupJoinResultType = typeof(TransparentIdentifier<,>).MakeGenericType(sourceType, groupingType);

                    var groupJoinMethodInfo = QueryableGroupJoinMethodInfo.MakeGenericMethod(
                        sourceType,
                        navigationTargetEntityType.ClrType,
                        outerKeySelector.Body.Type,
                        groupJoinResultType);

                    var resultSelectorOuterParameterName = outerParameter.Name;
                    var resultSelectorOuterParameter = Expression.Parameter(sourceType, resultSelectorOuterParameterName);

                    var resultSelectorInnerParameterName = innerKeySelectorParameter.Name;
                    var resultSelectorInnerParameter = Expression.Parameter(groupingType, resultSelectorInnerParameterName);

                    var groupJoinResultTransparentIdentifierCtorInfo
                        = groupJoinResultType.GetTypeInfo().GetConstructors().Single();

                    var groupJoinResultSelector = Expression.Lambda(
                        Expression.New(groupJoinResultTransparentIdentifierCtorInfo, resultSelectorOuterParameter, resultSelectorInnerParameter),
                        resultSelectorOuterParameter,
                        resultSelectorInnerParameter);

                    var groupJoinMethodCall
                        = Expression.Call(
                            groupJoinMethodInfo,
                            sourceExpression,
                            entityQueryable,
                            outerKeySelector,
                            innerKeySelector,
                            groupJoinResultSelector);

                    var selectManyResultType = typeof(TransparentIdentifier<,>).MakeGenericType(groupJoinResultType, navigationTargetEntityType.ClrType);

                    var selectManyMethodInfo = QueryableSelectManyWithResultOperatorMethodInfo.MakeGenericMethod(
                        groupJoinResultType,
                        navigationTargetEntityType.ClrType,
                        selectManyResultType);

                    var defaultIfEmptyMethodInfo = EnumerableDefaultIfEmpty.MakeGenericMethod(navigationTargetEntityType.ClrType);

                    var selectManyCollectionSelectorParameter = Expression.Parameter(groupJoinResultType);
                    var selectManyCollectionSelector = Expression.Lambda(
                        Expression.Call(
                            defaultIfEmptyMethodInfo,
                            Expression.Field(selectManyCollectionSelectorParameter, nameof(TransparentIdentifier<object, object>.Inner))),
                        selectManyCollectionSelectorParameter);

                    var selectManyResultTransparentIdentifierCtorInfo
                        = selectManyResultType.GetTypeInfo().GetConstructors().Single();

                    // TODO: dont reuse parameters here?
                    var selectManyResultSelector = Expression.Lambda(
                        Expression.New(selectManyResultTransparentIdentifierCtorInfo, selectManyCollectionSelectorParameter, innerKeySelectorParameter),
                        selectManyCollectionSelectorParameter,
                        innerKeySelectorParameter);

                    var selectManyMethodCall
                        = Expression.Call(selectManyMethodInfo,
                        groupJoinMethodCall,
                        selectManyCollectionSelector,
                        selectManyResultSelector);

                    sourceType = selectManyResultSelector.ReturnType;
                    sourceExpression = selectManyMethodCall;

                    var transparentIdentifierParameterName = resultSelectorInnerParameterName;
                    var transparentIdentifierParameter = Expression.Parameter(selectManyResultSelector.ReturnType, transparentIdentifierParameterName);
                    parameterExpression = transparentIdentifierParameter;
                }
                else
                {
                    var joinMethodInfo = QueryableJoinMethodInfo.MakeGenericMethod(
                        sourceType,
                        navigationTargetEntityType.ClrType,
                        outerKeySelector.Body.Type,
                        resultType);

                    var resultSelectorOuterParameterName = outerParameter.Name;
                    var resultSelectorOuterParameter = Expression.Parameter(sourceType, resultSelectorOuterParameterName);

                    var resultSelectorInnerParameterName = innerKeySelectorParameter.Name;
                    var resultSelectorInnerParameter = Expression.Parameter(navigationTargetEntityType.ClrType, resultSelectorInnerParameterName);

                    var transparentIdentifierCtorInfo
                        = resultType.GetTypeInfo().GetConstructors().Single();

                    var resultSelector = Expression.Lambda(
                        Expression.New(transparentIdentifierCtorInfo, resultSelectorOuterParameter, resultSelectorInnerParameter),
                        resultSelectorOuterParameter,
                        resultSelectorInnerParameter);

                    var joinMethodCall = Expression.Call(
                        joinMethodInfo,
                        sourceExpression,
                        entityQueryable,
                        outerKeySelector,
                        innerKeySelector,
                        resultSelector);

                    sourceType = resultSelector.ReturnType;
                    sourceExpression = joinMethodCall;

                    var transparentIdentifierParameterName = /*resultSelectorOuterParameterName + */resultSelectorInnerParameterName;
                    var transparentIdentifierParameter = Expression.Parameter(resultSelector.ReturnType, transparentIdentifierParameterName);
                    parameterExpression = transparentIdentifierParameter;
                }

                if (navigationPathNames.Count == 0)
                {
                    transparentIdentifierAccessorMapping.Add((from: navigationPathNames.ToList(), to: new List<string>()));
                }

                foreach (var transparentIdentifierAccessorMappingElement in transparentIdentifierAccessorMapping)
                {
                    transparentIdentifierAccessorMappingElement.to.Insert(0, nameof(TransparentIdentifier<object, object>.Outer));

                    // in case of GroupJoin (optional navigation) source is hidden deeps since we also project the grouping
                    // we could remove the grouping in the future, but for nowe we need the grouping to properly recognize the LOJ pattern
                    if (navigationPath.Optional)
                    {
                        transparentIdentifierAccessorMappingElement.to.Insert(0, nameof(TransparentIdentifier<object, object>.Outer));
                    }
                }

                navigationPathNames.Add(navigation.Name);
                transparentIdentifierAccessorMapping.Add((from: navigationPathNames.ToList(), to: new List<string> { nameof(TransparentIdentifier<object, object>.Inner) }));

                finalProjectionPath.Add("Outer");
                if (navigationPath.Optional)
                {
                    finalProjectionPath.Add("Outer");
                }
            }
            else
            {
                navigationPathNames.Add(navigationPath.Navigation.Name);
            }

            var result = (source: sourceExpression, parameter: parameterExpression);
            foreach (var child in navigationPath.Children)
            {
                result = AddNavigationJoin(
                    result.source,
                    result.parameter,
                    child,
                    navigationPathNames.ToList(),
                    finalProjectionPath,
                    transparentIdentifierAccessorMapping);
            }

            return result;
        }

        // TODO: DRY
        private Expression BuildTransparentIdentifierAccessorExpression(Expression source, List<string> accessorPath)
        {
            var result = source;
            if (accessorPath != null)
            {
                foreach (var accessorPathElement in accessorPath)
                {
                    result = Expression.Field(result, accessorPathElement);
                }
            }

            return result;
        }
    }

    public class NavigationPathNode
    {
        public INavigation Navigation { get; set; }
        public bool Optional { get; set; }
        public NavigationPathNode Parent { get; set; }
        public List<NavigationPathNode> Children { get; set; }

        public List<string> GeneratePath()
        {
            if (Parent == null)
            {
                return new List<string> { Navigation.Name };
            }
            else
            {
                var result = Parent.GeneratePath();
                result.Add(Navigation.Name);

                return result;
            }
        }

        public static NavigationPathNode Create(IEnumerable<INavigation> expansionPath, bool optional)
        {
            if (expansionPath.Count() == 0)
            {
                return null;
            }

            var navigation = expansionPath.First();
            optional = optional || !navigation.ForeignKey.IsRequired || !navigation.IsDependentToPrincipal();
            var result = new NavigationPathNode
            {
                Navigation = navigation,
                Optional = optional,
                Children = new List<NavigationPathNode>(),
            };

            var child = Create(expansionPath.Skip(1), optional);
            if (child != null)
            {
                result.Children.Add(child);
                child.Parent = result;
            }

            return result;
        }

        public bool Contains(NavigationPathNode other)
        {
            if (other.Navigation != Navigation)
            {
                return false;
            }

            return other.Children.All(oc => Children.Any(c => c.Contains(oc)));
        }

        public bool TryCombine(NavigationPathNode other)
        {
            if (other.Navigation != Navigation)
            {
                return false;
            }

            foreach (var otherChild in other.Children)
            {
                var success = false;
                foreach (var child in Children)
                {
                    if (!success)
                    {
                        success = child.TryCombine(otherChild);
                    }
                }

                if (!success)
                {
                    Children.Add(otherChild);
                    otherChild.Parent = this;
                }
            }

            return true;
        }
    }

    public class NavigationFindingExpressionVisitor : ExpressionVisitor
    {
        private IModel _model;
        private ParameterExpression _sourceParameter;
        private List<(List<string> from, List<INavigation> to)> _selectorMapping;

        public List<NavigationPathNode> FoundNavigationPaths { get; }

        public NavigationFindingExpressionVisitor(
            IModel model,
            ParameterExpression sourceParameter,
            List<(List<string> from, List<INavigation> to)> selectorMapping,
            List<NavigationPathNode> foundNavigationPaths)
        {
            _model = model;
            _sourceParameter = sourceParameter;
            _selectorMapping = selectorMapping;
            FoundNavigationPaths = foundNavigationPaths;
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var properties = Binder.GetPropertyPath(
                memberExpression,
                _model,
                _selectorMapping,
                out var parameterExpression);

            // there should be no collection navigations at this point
            // TODO: what about owned collections?
            var navigations = properties.OfType<INavigation>().ToList();
            if (parameterExpression == _sourceParameter
                && navigations.Any())
            {
                var inheritanceRoot = navigations[0].ClrType != parameterExpression.Type
                    && navigations[0].DeclaringEntityType.GetAllBaseTypes().Any(t => t.ClrType == parameterExpression.Type);

                var navigationPath = NavigationPathNode.Create(navigations, inheritanceRoot);
                if (!FoundNavigationPaths.Any(p => p.Contains(navigationPath)))
                {
                    var success = false;
                    foreach (var foundNavigationPath in FoundNavigationPaths)
                    {
                        if (!success)
                        {
                            success = foundNavigationPath.TryCombine(navigationPath);
                        }
                    }

                    if (!success)
                    {
                        FoundNavigationPaths.Add(navigationPath);
                    }
                }
            }

            return base.VisitMember(memberExpression);
        }
    }

    public class NavigationReplacingExpressionVisitor : ExpressionVisitor
    {
        private IModel _model;
        private ParameterExpression _sourceParameter;
        private ParameterExpression _transparentIdentifierParameter;
        private List<(List<string> from, List<string> to)> _transparentIdentifierAccessorMapping;
        private List<(List<string> from, List<INavigation> to)> _selectorMapping;

        public NavigationReplacingExpressionVisitor(
            IModel model,
            ParameterExpression sourceParameter,
            ParameterExpression transparentIdentifierParameter,
            List<(List<string> from, List<string> to)> transparentIdentifierAccessorMapping,
            List<(List<string> from, List<INavigation> to)> selectorMapping)
        {
            _model = model;
            _sourceParameter = sourceParameter;
            _transparentIdentifierParameter = transparentIdentifierParameter;
            _transparentIdentifierAccessorMapping = transparentIdentifierAccessorMapping;
            _selectorMapping = selectorMapping;
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var properties = Binder.GetPropertyPath(memberExpression, _model, _selectorMapping, out var parameterExpression);
            if (parameterExpression == _sourceParameter
                && properties.Any())
            {
                var transparentIdentifierAccessorPath = _transparentIdentifierAccessorMapping.Where(m => m.from.Count == properties.Count && m.from.Zip(properties.Select(p => p.Name), (o, i) => o == i).All(e => e)).SingleOrDefault().to;
                if (transparentIdentifierAccessorPath != null)
                {
                    var result = BuildTransparentIdentifierAccessorExpression(_transparentIdentifierParameter, transparentIdentifierAccessorPath);

                    return result;
                }
            }

            return base.VisitMember(memberExpression);
        }

        protected override Expression VisitLambda<T>(Expression<T> lambdaExpression)
        {
            var newParameters = new List<ParameterExpression>();
            var parameterChanged = false;

            foreach (var parameter in lambdaExpression.Parameters)
            {
                if (parameter == _sourceParameter)
                {
                    newParameters.Add(_transparentIdentifierParameter);
                    parameterChanged = true;
                }
                else
                {
                    newParameters.Add(parameter);
                }
            }

            var newBody = Visit(lambdaExpression.Body);

            return parameterChanged || newBody != lambdaExpression.Body
                ? Expression.Lambda(newBody, newParameters)
                : lambdaExpression;
        }

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            if (parameterExpression == _sourceParameter)
            {
                var transparentIdentifierRootPath = _transparentIdentifierAccessorMapping.Where(m => m.from.Count == 0).SingleOrDefault().to;

                return BuildTransparentIdentifierAccessorExpression(_transparentIdentifierParameter, transparentIdentifierRootPath);
            }

            return parameterExpression;
        }

        // TODO: DRY
        private Expression BuildTransparentIdentifierAccessorExpression(Expression source, List<string> accessorPath)
        {
            var result = source;
            if (accessorPath != null)
            {
                foreach (var accessorPathElement in accessorPath)
                {
                    result = Expression.Field(result, accessorPathElement);
                }
            }

            return result;
        }
    }

    // this is a hack, need to rewrite this, but should be ok for the prototype
    public class Binder
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static List<IPropertyBase> GetPropertyPath(
            [NotNull] Expression expression,
            [NotNull] IModel model,
            [NotNull] List<(List<string> from, List<INavigation> to)> selectorMapping,
            out ParameterExpression sourceParameter)
        {
            Check.NotNull(expression, nameof(expression));
            Check.NotNull(model, nameof(model));
            Check.NotNull(selectorMapping, nameof(selectorMapping));

            var result = GetPropertPathInternal(expression, model, selectorMapping, out var sourceExpression);
            sourceParameter = (ParameterExpression)sourceExpression;

            return result;
        }

        private static List<IPropertyBase> GetPropertPathInternal(
            Expression expression,
            IModel model,
            List<(List<string> from, List<INavigation> to)> selectorMapping,
            out Expression sourceExpression)
        {
            expression = expression.RemoveNullConditional();
            var memberExpression = expression as MemberExpression;
            var methodCallExpression = expression as MethodCallExpression;

            Expression innerExpression = null;
            string propertyName = null;
            if (memberExpression != null)
            {
                innerExpression = memberExpression.Expression;
                propertyName = memberExpression?.Member.Name;
            }
            else if (methodCallExpression != null)
            {
                if (methodCallExpression.Method.IsEFPropertyMethod())
                {
                    // this was a direct call to EF.Property()
                    innerExpression = methodCallExpression.Arguments[0];
                    propertyName = (string)(methodCallExpression.Arguments[1] as ConstantExpression)?.Value;
                }
                else if (methodCallExpression.Method.IsEFIndexer())
                {
                    // this was an indexer call
                    innerExpression = methodCallExpression.Object;
                    propertyName = (string)(methodCallExpression.Arguments[0] as ConstantExpression)?.Value;
                }
            }

            if (innerExpression == null)
            {
                sourceExpression = expression as ParameterExpression;
                return new List<IPropertyBase>();
            }

            // in case of inheritance there might be convert to derived type here, so we want to check it first
            var entityType = model.FindEntityType(innerExpression.Type);

            innerExpression = innerExpression.RemoveConvert();

            if (entityType == null)
            {
                entityType = model.FindEntityType(innerExpression.Type);
            }


            //var fubarson = TryGetPropertyPathFromSelectorMappings(innerExpression, selectorMapping, out var innerQsre2);

            var innerProperties = GetPropertPathInternal(innerExpression, model, selectorMapping, out var innerQsre);
            if (innerQsre == null
                && innerProperties.Count == 0
                && selectorMapping.Any())
            {
                var navigations = TryGetPropertyPathFromSelectorMappings(innerExpression, selectorMapping, out innerQsre);
                if (navigations != null && navigations.Any())
                {
                    innerProperties = navigations.OfType<IPropertyBase>().ToList();
                }
            }

            if (entityType == null)
            {
                if (innerProperties.Count > 0)
                {
                    entityType = (innerProperties[innerProperties.Count - 1] as INavigation)?.GetTargetType();
                }
                else if (innerQsre != null)
                {
                    entityType = model.FindEntityType(innerQsre.Type);
                    // TODO: we don't have this information at the moment, need to test the ramifications
                    //entityType = queryCompilationContext.FindEntityType(innerQsre.ReferencedQuerySource);
                }

                if (entityType == null)
                {
                    sourceExpression = null;
                    innerProperties.Clear();

                    return innerProperties;
                }
            }

            var property = propertyName == null
                ? null
                : (IPropertyBase)entityType.FindProperty(propertyName)
                  ?? entityType.FindNavigation(propertyName);

            if (property == null)
            {
                if ((methodCallExpression?.Method).IsEFPropertyMethod())
                {
                    throw new InvalidOperationException(
                        CoreStrings.PropertyNotFound(propertyName, entityType.DisplayName()));
                }

                sourceExpression = null;
                innerProperties.Clear();

                return innerProperties;
            }

            innerProperties.Add(property);
            sourceExpression = innerQsre;

            return innerProperties;
        }

        private static List<INavigation> TryGetPropertyPathFromSelectorMappings(
            Expression expression,
            List<(List<string> from, List<INavigation> to)> selectorMapping,
            out Expression sourceExpression)
        {
            foreach (var selectorMappingElement in selectorMapping)
            {
                var currentExpression = expression;

                var matchFound = true;
                for (var i = selectorMappingElement.from.Count - 1; i >= 0; i--)
                {
                    if (currentExpression is MemberExpression memberExpression
                        && selectorMappingElement.from[i] == memberExpression.Member.Name)
                    {
                        currentExpression = memberExpression.Expression;
                    }
                    else
                    {
                        matchFound = false;
                        break;
                    }
                }

                if (matchFound && currentExpression is ParameterExpression)
                {
                    sourceExpression = currentExpression;
                    return selectorMappingElement.to.ToList();
                }
            }

            sourceExpression = null;

            return null;
        }
    }
}
