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

    // temporary hack
    public static class ParameterNamingExtensions
    {
        private static int Count = 1;

        public static string GenerateParameterName(this Type type)
            => type.Name.Substring(0, 1).ToLower() + Count++;
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

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.MethodIsClosedFormOf(QueryableWhereMethodInfo))
            {
                var source = Visit(methodCallExpression.Arguments[0]);
                var predicate = Visit(methodCallExpression.Arguments[1]);
                var parameter = (predicate.UnwrapQuote() as LambdaExpression).Parameters[0];
                var transparentIdentifierAccessorMapping = new List<(List<string> from, List<string> to)>();
                var foundNavigations = new List<NavigationPathNode>();
                var finalProjectionPath = new List<string>();
                var newParameter = parameter;

                if (source is NavigationExpansionExpression navigationExpansionExpression)
                {
                    source = navigationExpansionExpression.Operand;
                    newParameter = navigationExpansionExpression.ParameterExpression;
                    transparentIdentifierAccessorMapping = navigationExpansionExpression.TransparentIdentifierAccessorMapping;
                    foundNavigations = navigationExpansionExpression.FoundNavigations;
                    finalProjectionPath = navigationExpansionExpression.FinalProjectionPath;
                }

                var nfev = new NavigationFindingExpressionVisitor(_model, parameter, foundNavigations);
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
                var nrev = new NavigationReplacingExpressionVisitor(_model, parameter, result.parameter, transparentIdentifierAccessorMapping);
                var newPredicate = nrev.Visit(predicate);

                var newMethodInfo = QueryableWhereMethodInfo.MakeGenericMethod(result.parameter.Type);

                var rewritten = Expression.Call(newMethodInfo, newSource, newPredicate);

                return new NavigationExpansionExpression(
                    rewritten,
                    result.parameter,
                    transparentIdentifierAccessorMapping,
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

                var outerParameter = Expression.Parameter(sourceType, sourceType.GenerateParameterName());
                var outerKeySelectorParameter = outerParameter;
                var transparentIdentifierAccessorExpression = BuildTransparentIdentifierAccessorExpression(outerParameter, transparentIdentifierAccessorPath);

                var outerKeySelectorBody = CreateKeyAccessExpression(
                    transparentIdentifierAccessorExpression,
                    navigation.IsDependentToPrincipal()
                        ? navigation.ForeignKey.Properties
                        : navigation.ForeignKey.PrincipalKey.Properties,
                    // TODO: do this on parent navigation instead?
                    addNullCheck: navigationPath.OptionalNavigationInChain(sourceType));

                var innerKeySelectorParameterType = navigationTargetEntityType.ClrType;
                var innerKeySelectorParameter = Expression.Parameter(
                    innerKeySelectorParameterType,
                    parameterExpression.Name + "." + string.Join(".", navigationPath.GeneratePath()));

                var innerKeySelectorBody = CreateKeyAccessExpression(
                    innerKeySelectorParameter,
                    navigation.IsDependentToPrincipal()
                        ? navigation.ForeignKey.PrincipalKey.Properties
                        : navigation.ForeignKey.Properties);

                // compensate for nullability difference
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

                // source is wrong here, we need to keep track of the navigation root, not the current source!
                var needsGroupJoin = navigationPath.OptionalNavigationInChain(sourceType);
                if (needsGroupJoin)
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

                    var resultSelectorInnerParameterName = "grouping";
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

                    var transparentIdentifierParameterName = resultSelectorOuterParameterName + resultSelectorInnerParameterName;
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

                    var transparentIdentifierParameterName = resultSelectorOuterParameterName + resultSelectorInnerParameterName;
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
                    if (needsGroupJoin)
                    {
                        transparentIdentifierAccessorMappingElement.to.Insert(0, nameof(TransparentIdentifier<object, object>.Outer));
                    }
                }

                navigationPathNames.Add(navigation.Name);
                transparentIdentifierAccessorMapping.Add((from: navigationPathNames.ToList(), to: new List<string> { nameof(TransparentIdentifier<object, object>.Inner) }));

                finalProjectionPath.Add("Outer");
                if (needsGroupJoin)
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

        public static NavigationPathNode Create(IEnumerable<INavigation> expansionPath)
        {
            if (expansionPath.Count() == 0)
            {
                return null;
            }

            var result = new NavigationPathNode
            {
                Navigation = expansionPath.First(),
                Children = new List<NavigationPathNode>(),
            };

            var child = Create(expansionPath.Skip(1));
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

        // TODO: store this when we generate navigation tree?
        public bool OptionalNavigationInChain(Type rootType)
        {
            if (!Navigation.ForeignKey.IsRequired
                || !Navigation.IsDependentToPrincipal())
            {
                return true;
            }

            if (Parent == null)
            {
                return Navigation.DeclaringEntityType.ClrType != rootType
                    && Navigation.DeclaringEntityType.GetAllBaseTypes().Any(t => t.ClrType == rootType);
            }
            else
            {
                return Parent.OptionalNavigationInChain(rootType);
            }
        }
    }

    public class NavigationFindingExpressionVisitor : ExpressionVisitor
    {
        private IModel _model;
        private ParameterExpression _sourceParameter;

        public List<NavigationPathNode> FoundNavigationPaths { get; }

        public NavigationFindingExpressionVisitor(IModel model, ParameterExpression sourceParameter, List<NavigationPathNode> foundNavigationPaths)
        {
            _model = model;
            _sourceParameter = sourceParameter;
            FoundNavigationPaths = foundNavigationPaths;
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var properties = Binder.GetPropertyPath(memberExpression, _model, out var parameterExpression);

            // there should be no collection navigations at this point
            // TODO: what about owned collections?
            var navigations = properties.OfType<INavigation>().ToList();
            if (parameterExpression == _sourceParameter
                && navigations.Any())
            {
                // TODO: optimize this
                var navigationPath = NavigationPathNode.Create(navigations);
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

        public NavigationReplacingExpressionVisitor(
            IModel model,
            ParameterExpression sourceParameter,
            ParameterExpression transparentIdentifierParameter,
            List<(List<string> from, List<string> to)> transparentIdentifierAccessorMapping)
        {
            _model = model;
            _sourceParameter = sourceParameter;
            _transparentIdentifierParameter = transparentIdentifierParameter;
            _transparentIdentifierAccessorMapping = transparentIdentifierAccessorMapping;
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var properties = Binder.GetPropertyPath(memberExpression, _model, out var parameterExpression);
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
            out ParameterExpression sourceExpression)
        {
            Check.NotNull(expression, nameof(expression));
            Check.NotNull(model, nameof(model));

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

            var innerProperties = GetPropertyPath(innerExpression, model, out var innerQsre);

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
    }
}
