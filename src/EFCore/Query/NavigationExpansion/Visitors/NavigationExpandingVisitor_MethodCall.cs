// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion.Visitors
{
    public partial class NavigationExpandingVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var propertyName))
            {
                var newSource = Visit(source);

                return newSource is NavigationExpansionExpression navigationExpansionExpression
                    && navigationExpansionExpression.State.PendingCardinalityReducingOperator != null
                    ? ProcessMemberPushdown(
                        newSource,
                        navigationExpansionExpression,
                        efProperty: true,
                        memberInfo: null,
                        propertyName,
                        methodCallExpression.Type)
                    : methodCallExpression.Update(methodCallExpression.Object, new[] { newSource, methodCallExpression.Arguments[1] });
            }

            if (methodCallExpression.Method.DeclaringType == typeof(Queryable)
                || methodCallExpression.Method.DeclaringType == typeof(QueryableExtensions)
                || methodCallExpression.Method.DeclaringType == typeof(Enumerable)
                || methodCallExpression.Method.DeclaringType == typeof(EntityFrameworkQueryableExtensions))
            {
                switch (methodCallExpression.Method.Name)
                {
                    case nameof(Queryable.Where):
                        return ProcessWhere(methodCallExpression);

                    case nameof(Queryable.Select):
                        return ProcessSelect(methodCallExpression);

                    case nameof(Queryable.OrderBy):
                    case nameof(Queryable.OrderByDescending):
                        return ProcessOrderBy(methodCallExpression);

                    case nameof(Queryable.ThenBy):
                    case nameof(Queryable.ThenByDescending):
                        return ProcessThenByBy(methodCallExpression);

                    case nameof(Queryable.Join):
                        return ProcessJoin(methodCallExpression);

                    case nameof(Queryable.GroupJoin):
                        return ProcessGroupJoin(methodCallExpression);

                    case nameof(Queryable.SelectMany):
                        return ProcessSelectMany(methodCallExpression);

                    case nameof(Queryable.All):
                        return ProcessAll(methodCallExpression);

                    case nameof(Queryable.Any):
                    case nameof(Queryable.Count):
                    case nameof(Queryable.LongCount):
                        return ProcessAnyCountLongCount(methodCallExpression);

                    case nameof(Queryable.Average):
                    case nameof(Queryable.Sum):
                    case nameof(Queryable.Min):
                    case nameof(Queryable.Max):
                        return ProcessAverageSumMinMax(methodCallExpression);

                    case nameof(Queryable.Distinct):
                        return ProcessDistinct(methodCallExpression);

                    case nameof(Queryable.DefaultIfEmpty):
                        return ProcessDefaultIfEmpty(methodCallExpression);

                    case nameof(Queryable.First):
                    case nameof(Queryable.FirstOrDefault):
                    case nameof(Queryable.Single):
                    case nameof(Queryable.SingleOrDefault):
                        return ProcessCardinalityReducingOperation(methodCallExpression);

                    case nameof(Queryable.OfType):
                        return ProcessOfType(methodCallExpression);

                    case nameof(Queryable.Skip):
                    case nameof(Queryable.Take):
                        return ProcessSkipTake(methodCallExpression);

                    case nameof(Queryable.Union):
                    case nameof(Queryable.Concat):
                    case nameof(Queryable.Intersect):
                    case nameof(Queryable.Except):
                        return ProcessSetOperation(methodCallExpression);

                    case nameof(EntityFrameworkQueryableExtensions.Include):
                    case nameof(EntityFrameworkQueryableExtensions.ThenInclude):
                        return ProcessInclude(methodCallExpression);

                    default:
                        return ProcessUnknownMethod(methodCallExpression);
                }
            }

            return ProcessUnknownMethod(methodCallExpression);
        }

        private Expression ProcessUnknownMethod(MethodCallExpression methodCallExpression)
        {
            var resultSequenceType = TryGetNonPrimitiveSequenceType(methodCallExpression.Type);

            // result is a sequence, no lambda arguments, exactly one generic argument corresponding to result sequence type
            if (methodCallExpression.Object == null
                && resultSequenceType != null
                && methodCallExpression.Arguments.All(a => a.GetLambdaOrNull() == null)
                && methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericArguments().Length == 1
                && methodCallExpression.Method.GetGenericArguments()[0] == resultSequenceType)
            {
                var argumentSequenceTypes = methodCallExpression.Arguments.Select(a => TryGetNonPrimitiveSequenceType(a.Type)).ToList();
                if (argumentSequenceTypes.FirstOrDefault() == resultSequenceType
                    && argumentSequenceTypes.Count(t => t != null) == 1)
                {
                    var source = VisitSourceExpression(methodCallExpression.Arguments[0]);
                    var preProcessResult = PreProcessTerminatingOperation(source);
                    var newArguments = methodCallExpression.Arguments.Skip(1).Select(Visit).ToList();
                    newArguments.Insert(0, preProcessResult.Source);

                    var methodInfo = methodCallExpression.Method.GetGenericMethodDefinition().MakeGenericMethod(preProcessResult.State.CurrentParameter.Type);
                    var rewritten = Expression.Call(methodInfo, newArguments);

                    return new NavigationExpansionExpression(rewritten, preProcessResult.State, methodCallExpression.Type);
                }
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        private Type TryGetNonPrimitiveSequenceType(Type type)
            => type == typeof(string) || type.IsArray ? null : type.TryGetSequenceType();

        private NavigationExpansionExpression VisitSourceExpression(Expression sourceExpression)
        {
            var result = Visit(sourceExpression);
            if (result is NavigationExpansionRootExpression navigationExpansionRootExpression)
            {
                result = navigationExpansionRootExpression.Unwrap();
            }

            if (result is NavigationExpansionExpression navigationExpansionExpression)
            {
                return navigationExpansionExpression;
            }

            // this is for sources that are not EntityQueryables, like inlined arrays/lists, or parameters, e.g. customers.Where(c => new[] { "Foo", "Bar" }.Contains(c.Id))
            var currentParameter = Expression.Parameter(result.Type.GetSequenceType());
            var customRootMapping = new List<string>();

            var state = new NavigationExpansionExpressionState(
                currentParameter,
                new List<SourceMapping>(),
                Expression.Lambda(new CustomRootExpression(currentParameter, customRootMapping, currentParameter.Type), currentParameter),
                applyPendingSelector: false,
                new List<(MethodInfo method, LambdaExpression keySelector)>(),
                pendingIncludeChain: null,
                pendingCardinalityReducingOperator: null,
                new List<List<string>> { customRootMapping },
                materializeCollectionNavigation: null);

            return new NavigationExpansionExpression(result, state, result.Type);
        }

        private Expression ProcessWhere(MethodCallExpression methodCallExpression)
        {
            var source = VisitSourceExpression(methodCallExpression.Arguments[0]);
            var predicate = methodCallExpression.Arguments[1].UnwrapLambdaFromQuote();

            var (newSource, newLambdaBody, newState) = FindAndApplyNavigations(source.Operand, predicate, source.State);
            var newPredicateBody = new NavigationPropertyUnbindingVisitor(newState.CurrentParameter)
                .Visit(newLambdaBody);
            var newPredicateLambda = Expression.Lambda(newPredicateBody, newState.CurrentParameter);
            (newSource, newState) = ApplyPendingOrderings(newSource, newState);

            var newMethodInfo = methodCallExpression.Method.GetGenericMethodDefinition().MakeGenericMethod(newState.CurrentParameter.Type);
            var rewritten = Expression.Call(newMethodInfo, newSource, newPredicateLambda);

            return new NavigationExpansionExpression(
                rewritten,
                newState,
                methodCallExpression.Type);
        }

        private Expression ProcessSelect(MethodCallExpression methodCallExpression)
        {
            var source = VisitSourceExpression(methodCallExpression.Arguments[0]);
            var selector = methodCallExpression.Arguments[1].UnwrapLambdaFromQuote();

            return ProcessSelectCore(source.Operand, source.State, selector, methodCallExpression.Type);
        }

        private Expression ProcessSelectCore(Expression source, NavigationExpansionExpressionState state, LambdaExpression selector, Type resultType)
        {
            var (newSource, newLambdaBody, newState) = FindAndApplyNavigations(source, selector, state);
            newState.PendingSelector = Expression.Lambda(newLambdaBody, newState.CurrentParameter);

            // we could force apply pending selector only for non-identity projections
            // but then we lose information about variable names, e.g. ctx.Customers.Select(x => x)
            newState.ApplyPendingSelector = true;

            (newSource, newState) = ApplyPendingOrderings(newSource, newState);

            var resultElementType = resultType.TryGetSequenceType();
            if (resultElementType != null)
            {
                if (resultElementType != newState.PendingSelector.Body.Type)
                {
                    resultType = resultType.GetGenericTypeDefinition().MakeGenericType(newState.PendingSelector.Body.Type);
                }
            }
            else
            {
                resultType = newState.PendingSelector.Body.Type;
            }

            return new NavigationExpansionExpression(
                newSource,
                newState,
                resultType);
        }

        private Expression ProcessOrderBy(MethodCallExpression methodCallExpression)
        {
            var source = VisitSourceExpression(methodCallExpression.Arguments[0]);
            var keySelector = methodCallExpression.Arguments[1].UnwrapLambdaFromQuote();

            var (newSource, newLambdaBody, newState) = FindAndApplyNavigations(source.Operand, keySelector, source.State);
            var pendingOrdering = (method: methodCallExpression.Method.GetGenericMethodDefinition(), keySelector: Expression.Lambda(newLambdaBody, newState.CurrentParameter));
            (newSource, newState) = ApplyPendingOrderings(newSource, newState);

            newState.PendingOrderings.Add(pendingOrdering);

            return new NavigationExpansionExpression(
                newSource,
                newState,
                methodCallExpression.Type);
        }

        private Expression ProcessThenByBy(MethodCallExpression methodCallExpression)
        {
            var source = VisitSourceExpression(methodCallExpression.Arguments[0]);
            var keySelector = methodCallExpression.Arguments[1].UnwrapLambdaFromQuote();

            var (newSource, newLambdaBody, newState) = FindAndApplyNavigations(source.Operand, keySelector, source.State);

            var pendingOrdering = (method: methodCallExpression.Method.GetGenericMethodDefinition(), keySelector: Expression.Lambda(newLambdaBody, newState.CurrentParameter));
            newState.PendingOrderings.Add(pendingOrdering);

            return new NavigationExpansionExpression(
                newSource,
                newState,
                methodCallExpression.Type);
        }

        private Expression ProcessSelectMany(MethodCallExpression methodCallExpression)
        {
            var outerSourceNee = VisitSourceExpression(methodCallExpression.Arguments[0]);
            var collectionSelector = methodCallExpression.Arguments[1].UnwrapLambdaFromQuote();

            var (outerSource, outerLambdaBody, outerState) = FindAndApplyNavigations(outerSourceNee.Operand, collectionSelector, outerSourceNee.State);
            (outerSource, outerState) = ApplyPendingOrderings(outerSource, outerState);

            var collectionSelectorNavigationExpansionExpression = outerLambdaBody as NavigationExpansionExpression
                ?? (outerLambdaBody as NavigationExpansionRootExpression)?.Unwrap() as NavigationExpansionExpression;

            if (collectionSelectorNavigationExpansionExpression != null)
            {
                var collectionSelectorState = collectionSelectorNavigationExpansionExpression.State;
                var collectionSelectorLambdaBody = collectionSelectorNavigationExpansionExpression.Operand;

                // in case collection selector is a "naked" collection navigation, we need to remove MaterializeCollectionNavigation
                // it's not needed for SelectMany collection selectors as they are not directly projected
                collectionSelectorState.MaterializeCollectionNavigation = null;

                if (methodCallExpression.Method.MethodIsClosedFormOf(LinqMethodHelpers.QueryableSelectManyWithResultOperatorMethodInfo)
                    && collectionSelectorState.CurrentParameter.Name != methodCallExpression.Arguments[2].UnwrapLambdaFromQuote().Parameters[1].Name)
                {
                    // TODO: should we rename the second parameter according to the second parameter of the result selector instead?
                    var newParameter = Expression.Parameter(collectionSelectorState.CurrentParameter.Type, methodCallExpression.Arguments[2].UnwrapLambdaFromQuote().Parameters[1].Name);
                    collectionSelectorState.PendingSelector = (LambdaExpression)new ExpressionReplacingVisitor(collectionSelectorState.CurrentParameter, newParameter).Visit(collectionSelectorState.PendingSelector);
                    collectionSelectorState.CurrentParameter = newParameter;
                }

                // in case collection selector body is IQueryable, we need to adjust the type to IEnumerable, to match the SelectMany signature
                // therefore the delegate type is specified explicitly
                var collectionSelectorLambdaType = typeof(Func<,>).MakeGenericType(
                    outerState.CurrentParameter.Type,
                    typeof(IEnumerable<>).MakeGenericType(collectionSelectorNavigationExpansionExpression.State.CurrentParameter.Type));

                var newCollectionSelectorLambda = Expression.Lambda(
                    collectionSelectorLambdaType,
                    collectionSelectorLambdaBody,
                    outerState.CurrentParameter);

                newCollectionSelectorLambda = (LambdaExpression)new NavigationPropertyUnbindingVisitor(outerState.CurrentParameter).Visit(newCollectionSelectorLambda);

                if (methodCallExpression.Method.MethodIsClosedFormOf(LinqMethodHelpers.QueryableSelectManyMethodInfo))
                {
                    return BuildSelectManyWithoutResultOperatorMethodCall(methodCallExpression, outerSource, outerState, newCollectionSelectorLambda, collectionSelectorState);
                }

                var resultSelector = methodCallExpression.Arguments[2].UnwrapLambdaFromQuote();

                // we need to create a new state for the collection element - in case of GroupJoin - SelectMany case, grouping is also in scope and it's navigations can be expanded independently
                var innerState = CreateSelectManyInnerState(collectionSelectorNavigationExpansionExpression.State, resultSelector.Parameters[1].Name);
                var resultSelectorRemap = RemapTwoArgumentResultSelector(resultSelector, outerState, /*collectionSelectorNavigationExpansionExpression.State*/innerState);

                var newMethodInfo = methodCallExpression.Method.GetGenericMethodDefinition().MakeGenericMethod(
                    outerState.CurrentParameter.Type,
                    collectionSelectorState.CurrentParameter.Type,
                    resultSelectorRemap.lambda.Body.Type);

                var rewritten = Expression.Call(
                    newMethodInfo,
                    outerSource,
                    newCollectionSelectorLambda,
                    resultSelectorRemap.lambda);

                // temporarily change selector to ti => ti for purpose of finding & expanding navigations in the pending selector lambda itself
                var pendingSelector = resultSelectorRemap.state.PendingSelector;
                resultSelectorRemap.state.PendingSelector = Expression.Lambda(resultSelectorRemap.state.PendingSelector.Parameters[0], resultSelectorRemap.state.PendingSelector.Parameters[0]);
                var result = FindAndApplyNavigations(rewritten, pendingSelector, resultSelectorRemap.state);
                result.State.PendingSelector = Expression.Lambda(result.LambdaBody, result.State.CurrentParameter);

                return new NavigationExpansionExpression(
                    result.Source,
                    result.State,
                    methodCallExpression.Type);
            }

            throw new InvalidOperationException("collection selector was not NavigationExpansionExpression");
        }

        private NavigationExpansionExpressionState CreateSelectManyInnerState(NavigationExpansionExpressionState collectionSelectorState, string parameterName)
        {
            var groupingElementParameter = Expression.Parameter(collectionSelectorState.CurrentParameter.Type, parameterName);

            var groupingSourceMappings = new List<SourceMapping>();
            var sourceMappingMapping = new Dictionary<SourceMapping, SourceMapping>();
            var customRootMappingMapping = new Dictionary<List<string>, List<string>>();
            var navigationTreeNodeMapping = new Dictionary<NavigationTreeNode, NavigationTreeNode>();

            foreach (var customRootMapping in collectionSelectorState.CustomRootMappings)
            {
                var newCustomRootMapping = customRootMapping.ToList();
                customRootMappingMapping[customRootMapping] = newCustomRootMapping;
            }

            foreach (var oldSourceMapping in collectionSelectorState.SourceMappings)
            {
                var newSourceMapping = new SourceMapping
                {
                    RootEntityType = oldSourceMapping.RootEntityType,
                };

                sourceMappingMapping[oldSourceMapping] = newSourceMapping;
                var newNavigationTreeRoot = NavigationTreeNode.CreateRoot(newSourceMapping, new List<string>(), oldSourceMapping.NavigationTree.Optional);

                // TODO: simply copying ToMapping might not be correct for very complex cases where the child mapping is not purely Inner/Outer but has some properties from previous anonymous projections
                // we should recognize and filter those out, however this is theoretical at this point - scenario is not supported and likely won't be in the foreseeable future
                newNavigationTreeRoot.ToMapping = oldSourceMapping.NavigationTree.ToMapping.ToList();
                newSourceMapping.NavigationTree = newNavigationTreeRoot;
                navigationTreeNodeMapping[oldSourceMapping.NavigationTree] = newNavigationTreeRoot;
                CopyNavigationTree(oldSourceMapping.NavigationTree, newNavigationTreeRoot, newSourceMapping, ref navigationTreeNodeMapping);
                groupingSourceMappings.Add(newSourceMapping);
            }

            var psr = new SelectManyCollectionPendingSelectorRemapper(
                collectionSelectorState.CurrentParameter,
                groupingElementParameter,
                sourceMappingMapping,
                navigationTreeNodeMapping,
                customRootMappingMapping);

            var groupingPendingSelectorBody = psr.Visit(collectionSelectorState.PendingSelector.Body);

            var groupingState = new NavigationExpansionExpressionState(
                groupingElementParameter,
                groupingSourceMappings,
                Expression.Lambda(groupingPendingSelectorBody, groupingElementParameter),
                collectionSelectorState.ApplyPendingSelector,
                pendingOrderings: new List<(MethodInfo method, LambdaExpression keySelector)>(),
                pendingIncludeChain: null,
                pendingCardinalityReducingOperator: null,
                customRootMappings: customRootMappingMapping.Values.ToList(),
                materializeCollectionNavigation: null);

            return groupingState;
        }

        private void CopyNavigationTree(
            NavigationTreeNode originalNavigationTree,
            NavigationTreeNode newNavigationTree,
            SourceMapping newSourceMapping,
            ref Dictionary<NavigationTreeNode, NavigationTreeNode> mapping)
        {
            foreach (var child in originalNavigationTree.Children)
            {
                var copy = NavigationTreeNode.Create(newSourceMapping, child.Navigation, newNavigationTree, include: false);
                copy.ExpansionState = child.ExpansionState;
                copy.IncludeState = child.IncludeState;

                // TODO: simply copying ToMapping might not be correct for very complex cases where the child mapping is not purely Inner/Outer but has some properties from previous anonymous projections
                // we should recognize and filter those out, however this is theoretical at this point - scenario is not supported and likely won't be in the foreseeable future
                copy.ToMapping = child.ToMapping.ToList();
                mapping[child] = copy;
                CopyNavigationTree(child, copy, newSourceMapping, ref mapping);
            }
        }

        private class SelectManyCollectionPendingSelectorRemapper : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParameter;
            private readonly ParameterExpression _newParameter;
            private readonly Dictionary<SourceMapping, SourceMapping> _sourceMappingMapping;
            private readonly Dictionary<NavigationTreeNode, NavigationTreeNode> _navigationTreeNodeMapping;
            private readonly Dictionary<List<string>, List<string>> _customRootMappingMapping;

            public SelectManyCollectionPendingSelectorRemapper(
                ParameterExpression oldParameter,
                ParameterExpression newParameter,
                Dictionary<SourceMapping, SourceMapping> sourceMappingMapping,
                Dictionary<NavigationTreeNode, NavigationTreeNode> navigationTreeNodeMapping,
                Dictionary<List<string>, List<string>> customRootMappingMapping)
            {
                _oldParameter = oldParameter;
                _newParameter = newParameter;
                _sourceMappingMapping = sourceMappingMapping;
                _navigationTreeNodeMapping = navigationTreeNodeMapping;
                _customRootMappingMapping = customRootMappingMapping;
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                if (extensionExpression is NavigationBindingExpression navigationBindingExpression
                    && navigationBindingExpression.RootParameter == _oldParameter)
                {
                    return new NavigationBindingExpression(
                        _newParameter,
                        _navigationTreeNodeMapping[navigationBindingExpression.NavigationTreeNode],
                        navigationBindingExpression.EntityType,
                        _sourceMappingMapping[navigationBindingExpression.SourceMapping],
                        navigationBindingExpression.Type);
                }

                if (extensionExpression is CustomRootExpression customRootExpression
                    && customRootExpression.RootParameter == _oldParameter)
                {
                    return new CustomRootExpression(_newParameter, _customRootMappingMapping[customRootExpression.Mapping], customRootExpression.Type);
                }

                return base.VisitExtension(extensionExpression);
            }
        }

        private Expression BuildSelectManyWithoutResultOperatorMethodCall(
            MethodCallExpression methodCallExpression,
            Expression outerSource,
            NavigationExpansionExpressionState outerState,
            LambdaExpression newCollectionSelectorLambda,
            NavigationExpansionExpressionState collectionSelectorState)
        {
            var newMethodInfo = methodCallExpression.Method.GetGenericMethodDefinition().MakeGenericMethod(
                outerState.CurrentParameter.Type,
                collectionSelectorState.CurrentParameter.Type);

            var rewritten = Expression.Call(
                newMethodInfo,
                outerSource,
                newCollectionSelectorLambda);

            return new NavigationExpansionExpression(
                rewritten,
                collectionSelectorState,
                methodCallExpression.Type);
        }

        private Expression ProcessJoin(MethodCallExpression methodCallExpression)
        {
            // TODO: move this to the big switch/case - this is for the string.Join case which would go here since it's matched by name currently
            if (!methodCallExpression.Method.MethodIsClosedFormOf(LinqMethodHelpers.QueryableJoinMethodInfo))
            {
                return base.VisitMethodCall(methodCallExpression);
            }

            var outerSource = VisitSourceExpression(methodCallExpression.Arguments[0]);
            var innerSource = VisitSourceExpression(methodCallExpression.Arguments[1]);

            var outerKeySelector = methodCallExpression.Arguments[2].UnwrapLambdaFromQuote();
            var innerKeySelector = methodCallExpression.Arguments[3].UnwrapLambdaFromQuote();
            var resultSelector = methodCallExpression.Arguments[4].UnwrapLambdaFromQuote();

            var (newOuterSource, newOuterLambdaBody, newOuterState)
                = FindAndApplyNavigations(outerSource.Operand, outerKeySelector, outerSource.State);
            var (newInnerSource, newInnerLambdaBody, newInnerState)
                = FindAndApplyNavigations(innerSource.Operand, innerKeySelector, innerSource.State);

            var newOuterKeySelectorBody = new NavigationPropertyUnbindingVisitor(newOuterState.CurrentParameter)
                .Visit(newOuterLambdaBody);
            var newInnerKeySelectorBody = new NavigationPropertyUnbindingVisitor(newInnerState.CurrentParameter)
                .Visit(newInnerLambdaBody);

            (newOuterSource, newOuterState) = ApplyPendingOrderings(newOuterSource, newOuterState);
            (newInnerSource, newInnerState) = ApplyPendingOrderings(newInnerSource, newInnerState);

            var resultSelectorRemap = RemapTwoArgumentResultSelector(resultSelector, newOuterState, newInnerState);

            var newMethodInfo = methodCallExpression.Method.GetGenericMethodDefinition().MakeGenericMethod(
                newOuterState.CurrentParameter.Type,
                newInnerState.CurrentParameter.Type,
                newOuterLambdaBody.Type,
                resultSelectorRemap.lambda.Body.Type);

            var rewritten = Expression.Call(
                newMethodInfo,
                newOuterSource,
                newInnerSource,
                Expression.Lambda(newOuterKeySelectorBody, newOuterState.CurrentParameter),
                Expression.Lambda(newInnerKeySelectorBody, newInnerState.CurrentParameter),
                Expression.Lambda(resultSelectorRemap.lambda.Body, newOuterState.CurrentParameter, newInnerState.CurrentParameter));

            // temporarily change selector to ti => ti for purpose of finding & expanding navigations in the pending selector lambda itself
            var pendingSelector = resultSelectorRemap.state.PendingSelector;
            resultSelectorRemap.state.PendingSelector = Expression.Lambda(resultSelectorRemap.state.PendingSelector.Parameters[0], resultSelectorRemap.state.PendingSelector.Parameters[0]);
            var result = FindAndApplyNavigations(rewritten, pendingSelector, resultSelectorRemap.state);
            result.State.PendingSelector = Expression.Lambda(result.LambdaBody, result.State.CurrentParameter);

            return new NavigationExpansionExpression(
                result.Source,
                result.State,
                methodCallExpression.Type);
        }

        private Expression ProcessGroupJoin(MethodCallExpression methodCallExpression)
        {
            var outerSource = VisitSourceExpression(methodCallExpression.Arguments[0]);
            var innerSource = VisitSourceExpression(methodCallExpression.Arguments[1]);

            var outerKeySelector = methodCallExpression.Arguments[2].UnwrapLambdaFromQuote();
            var innerKeySelector = methodCallExpression.Arguments[3].UnwrapLambdaFromQuote();
            var resultSelector = methodCallExpression.Arguments[4].UnwrapLambdaFromQuote();

            var (newOuterSource, newOuterLambdaBody, newOuterState)
                = FindAndApplyNavigations(outerSource.Operand, outerKeySelector, outerSource.State);
            var (newInnerSource, newInnerLambdaBody, newInnerState)
                = FindAndApplyNavigations(innerSource.Operand, innerKeySelector, innerSource.State);

            var newOuterKeySelectorBody = new NavigationPropertyUnbindingVisitor(newOuterState.CurrentParameter)
                .Visit(newOuterLambdaBody);
            var newInnerKeySelectorBody = new NavigationPropertyUnbindingVisitor(newInnerState.CurrentParameter)
                .Visit(newInnerLambdaBody);

            (newOuterSource, newOuterState) = ApplyPendingOrderings(newOuterSource, newOuterState);
            (newInnerSource, newInnerState) = ApplyPendingOrderings(newInnerSource, newInnerState);

            var resultSelectorBody = resultSelector.Body;
            var remappedResultSelectorBody = ReplacingExpressionVisitor.Replace(
                resultSelector.Parameters[0], newOuterState.PendingSelector.Body, resultSelector.Body);

            var groupingParameter = resultSelector.Parameters[1];
            var newGroupingParameter = Expression.Parameter(typeof(IEnumerable<>).MakeGenericType(newInnerState.CurrentParameter.Type), "new_" + groupingParameter.Name);

            var groupingMapping = new List<string> { nameof(TransparentIdentifier<object, object>.Inner) };

            // TODO: need to create the new state and copy includes from the old one, rather than simply copying it over to grouping
            // this shouldn't be a problem currently since we don't support queries that compose on the grouping
            // but when we do, state can't be shared - otherwise any nav expansion that affects the flattened part of the GroupJoin would be incorrectly propagated to the grouping as well
            var newGrouping = new NavigationExpansionExpression(newGroupingParameter, newInnerState, groupingParameter.Type);

            remappedResultSelectorBody = new ExpressionReplacingVisitor(
                groupingParameter,
                new NavigationExpansionRootExpression(newGrouping, groupingMapping)).Visit(remappedResultSelectorBody);

            foreach (var outerCustomRootMapping in newOuterState.CustomRootMappings)
            {
                outerCustomRootMapping.Insert(0, nameof(TransparentIdentifier<object, object>.Outer));
            }

            foreach (var outerSourceMapping in newOuterState.SourceMappings)
            {
                foreach (var navigationTreeNode in outerSourceMapping.NavigationTree.Flatten().Where(n => n.ExpansionState == NavigationState.Complete))
                {
                    navigationTreeNode.ToMapping.Insert(0, nameof(TransparentIdentifier<object, object>.Outer));
                    foreach (var fromMapping in navigationTreeNode.FromMappings)
                    {
                        fromMapping.Insert(0, nameof(TransparentIdentifier<object, object>.Outer));
                    }
                }
            }

            var resultType = typeof(TransparentIdentifier<,>).MakeGenericType(newOuterState.CurrentParameter.Type, newGroupingParameter.Type);
            var transparentIdentifierCtorInfo = resultType.GetTypeInfo().GetConstructors().Single();
            var transparentIdentifierParameter = Expression.Parameter(resultType, "groupjoin");

            var newPendingSelectorBody = new ExpressionReplacingVisitor(newOuterState.CurrentParameter, transparentIdentifierParameter).Visit(remappedResultSelectorBody);
            newPendingSelectorBody = new ExpressionReplacingVisitor(newGroupingParameter, transparentIdentifierParameter).Visit(newPendingSelectorBody);

            // for GroupJoin inner sources are not available, only the outer source mappings and the custom mappings for the grouping
            var newState = new NavigationExpansionExpressionState(
                transparentIdentifierParameter,
                newOuterState.SourceMappings,
                Expression.Lambda(newPendingSelectorBody, transparentIdentifierParameter),
                applyPendingSelector: true,
                newOuterState.PendingOrderings,
                newOuterState.PendingIncludeChain,
                newOuterState.PendingCardinalityReducingOperator,
                newOuterState.CustomRootMappings.Concat(new[] { groupingMapping }).ToList(),
                materializeCollectionNavigation: null);

            var transparentIdentifierOuterMemberInfo = resultType.GetTypeInfo().GetDeclaredField("Outer");
            var transparentIdentifierInnerMemberInfo = resultType.GetTypeInfo().GetDeclaredField("Inner");

            var lambda = Expression.Lambda(
                Expression.New(
                    transparentIdentifierCtorInfo,
                    new[] { newOuterState.CurrentParameter, newGroupingParameter },
                    new[] { transparentIdentifierOuterMemberInfo, transparentIdentifierInnerMemberInfo }),
                newOuterState.CurrentParameter,
                newGroupingParameter);

            var newMethodInfo = methodCallExpression.Method.GetGenericMethodDefinition().MakeGenericMethod(
                newOuterState.CurrentParameter.Type,
                newInnerState.CurrentParameter.Type,
                newOuterLambdaBody.Type,
                lambda.Body.Type);

            var rewritten = Expression.Call(
                newMethodInfo,
                newOuterSource,
                newInnerSource,
                Expression.Lambda(newOuterKeySelectorBody, newOuterState.CurrentParameter),
                Expression.Lambda(newInnerKeySelectorBody, newInnerState.CurrentParameter),
                lambda);

            // temporarily change selector to ti => ti for purpose of finding & expanding navigations in the pending selector lambda itself
            var pendingSelector = newState.PendingSelector;
            newState.PendingSelector = Expression.Lambda(newState.PendingSelector.Parameters[0], newState.PendingSelector.Parameters[0]);
            var result = FindAndApplyNavigations(rewritten, pendingSelector, newState);
            result.State.PendingSelector = Expression.Lambda(result.LambdaBody, result.State.CurrentParameter);

            return new NavigationExpansionExpression(
                result.Source,
                result.State,
                methodCallExpression.Type);
        }

        private Expression ProcessAll(MethodCallExpression methodCallExpression)
        {
            var source = VisitSourceExpression(methodCallExpression.Arguments[0]);
            source = RemoveIncludesFromSource(source);
            var predicate = methodCallExpression.Arguments[1].UnwrapLambdaFromQuote();

            var (newSource, newLambdaBody, newState) = FindAndApplyNavigations(source.Operand, predicate, source.State);
            var newPredicateBody = new NavigationPropertyUnbindingVisitor(newState.CurrentParameter)
                .Visit(newLambdaBody);
            (newSource, newState) = ApplyPendingOrderings(newSource, newState);

            var newMethodInfo = methodCallExpression.Method.GetGenericMethodDefinition().MakeGenericMethod(newState.CurrentParameter.Type);
            var rewritten = Expression.Call(
                newMethodInfo,
                newSource,
                Expression.Lambda(
                    newPredicateBody,
                    newState.CurrentParameter));

            return rewritten;
        }

        private Expression ProcessAnyCountLongCount(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.MethodIsClosedFormOf(LinqMethodHelpers.QueryableAnyPredicateMethodInfo)
                || methodCallExpression.Method.MethodIsClosedFormOf(LinqMethodHelpers.QueryableCountPredicateMethodInfo)
                || methodCallExpression.Method.MethodIsClosedFormOf(LinqMethodHelpers.QueryableLongCountPredicateMethodInfo))
            {
                return ProcessAnyCountLongCount(SimplifyPredicateMethod(methodCallExpression, queryable: true));
            }

            if (methodCallExpression.Method.MethodIsClosedFormOf(LinqMethodHelpers.EnumerableAnyPredicateMethodInfo)
                || methodCallExpression.Method.MethodIsClosedFormOf(LinqMethodHelpers.EnumerableCountPredicateMethodInfo)
                || methodCallExpression.Method.MethodIsClosedFormOf(LinqMethodHelpers.EnumerableLongCountPredicateMethodInfo))
            {
                return ProcessAnyCountLongCount(SimplifyPredicateMethod(methodCallExpression, queryable: false));
            }

            var source = VisitSourceExpression(methodCallExpression.Arguments[0]);
            source = RemoveIncludesFromSource(source);

            return methodCallExpression.Update(methodCallExpression.Object, new[] { source });
        }

        private NavigationExpansionExpression RemoveIncludesFromSource(NavigationExpansionExpression source)
        {
            foreach (var sourceMapping in source.State.SourceMappings)
            {
                RemoveIncludes(sourceMapping.NavigationTree);
            }

            return source.Type.IsGenericType
                && source.Type.GetGenericTypeDefinition() == typeof(IIncludableQueryable<,>)
                && source.Operand.Type != source.Type
                ? new NavigationExpansionExpression(source.Operand, source.State, source.Operand.Type)
                : source;
        }

        private void RemoveIncludes(NavigationTreeNode navigationTreeNode)
        {
            navigationTreeNode.IncludeState = NavigationState.NotNeeded;
            foreach (var child in navigationTreeNode.Children)
            {
                RemoveIncludes(child);
            }
        }

        private Expression ProcessAverageSumMinMax(MethodCallExpression methodCallExpression)
        {
            // TODO: hack - this should be resolved when/if we match based on method info
            if (methodCallExpression.Method.DeclaringType == typeof(Math))
            {
                return base.VisitMethodCall(methodCallExpression);
            }

            var source = VisitSourceExpression(methodCallExpression.Arguments[0]);
            source = RemoveIncludesFromSource(source);
            if (methodCallExpression.Arguments.Count == 2)
            {
                var selector = methodCallExpression.Arguments[1].UnwrapLambdaFromQuote();
                var (newSource, newLambdaBody, newState) = FindAndApplyNavigations(source.Operand, selector, source.State);
                var newSelectorBody = new NavigationPropertyUnbindingVisitor(newState.CurrentParameter).Visit(newLambdaBody);
                var newSelector = Expression.Lambda(newSelectorBody, newState.CurrentParameter);

                (newSource, newState) = ApplyPendingOrderings(newSource, newState);
                var newMethod = methodCallExpression.Method.GetGenericMethodDefinition();

                // Enumerable Min/Max overloads have only one type argument, Queryable have 2 but no overloads explosion
                if ((methodCallExpression.Method.Name == nameof(Enumerable.Min) || methodCallExpression.Method.Name == nameof(Enumerable.Max))
                    && newMethod.GetGenericArguments().Count() == 2)
                {
                    newMethod = newMethod.MakeGenericMethod(newState.CurrentParameter.Type, methodCallExpression.Type);
                }
                else
                {
                    newMethod = newMethod.MakeGenericMethod(newState.CurrentParameter.Type);
                }

                return Expression.Call(newMethod, newSource, newSelector);
            }

            return methodCallExpression.Update(methodCallExpression.Object, new[] { source });
        }

        private Expression ProcessDistinct(MethodCallExpression methodCallExpression)
        {
            var source = VisitSourceExpression(methodCallExpression.Arguments[0]);
            var preProcessResult = PreProcessTerminatingOperation(source);
            var rewritten = methodCallExpression.Update(methodCallExpression.Object, new[] { preProcessResult.Source });

            return new NavigationExpansionExpression(rewritten, preProcessResult.State, methodCallExpression.Type);
        }

        private Expression ProcessDefaultIfEmpty(MethodCallExpression methodCallExpression)
        {
            var source = VisitSourceExpression(methodCallExpression.Arguments[0]);
            foreach (var sourceMapping in source.State.SourceMappings)
            {
                sourceMapping.NavigationTree.MakeOptional();
            }

            // TODO: clean this up, i.e. in top level switch statement pick method based on method info, not only the name
            if (methodCallExpression.Method.MethodIsClosedFormOf(LinqMethodHelpers.QueryableDefaultIfEmptyWithDefaultValue))
            {
                var preProcessResult = PreProcessTerminatingOperation(source);
                var rewritten = methodCallExpression.Update(methodCallExpression.Object, new[] { preProcessResult.Source });

                return new NavigationExpansionExpression(rewritten, preProcessResult.State, methodCallExpression.Type);
            }
            else
            {
                var newMethodInfo = methodCallExpression.Method.GetGenericMethodDefinition().MakeGenericMethod(source.State.CurrentParameter.Type);
                var rewritten = Expression.Call(newMethodInfo, source.Operand);

                return new NavigationExpansionExpression(rewritten, source.State, methodCallExpression.Type);
            }
        }

        private Expression ProcessOfType(MethodCallExpression methodCallExpression)
        {
            var source = VisitSourceExpression(methodCallExpression.Arguments[0]);
            var preProcessResult = PreProcessTerminatingOperation(source);
            var newEntityType = _queryCompilationContext.Model.FindEntityType(methodCallExpression.Method.GetGenericArguments()[0]);

            // TODO: possible small optimization - only apply this if newEntityType is different than the old one
            if (newEntityType != null)
            {
                var newSourceMapping = new SourceMapping { RootEntityType = newEntityType };

                var newNavigationTreeRoot = NavigationTreeNode.CreateRoot(newSourceMapping, fromMapping: new List<string>(), optional: false);
                newSourceMapping.NavigationTree = newNavigationTreeRoot;
                preProcessResult.State.SourceMappings = new List<SourceMapping> { newSourceMapping };

                var newPendingSelectorParameter = Expression.Parameter(newEntityType.ClrType, preProcessResult.State.CurrentParameter.Name);

                // since we just ran preprocessing and the method is OfType, pending selector is guaranteed to be simple e => e
                var newPendingSelectorBody = new NavigationPropertyBindingVisitor(newPendingSelectorParameter, preProcessResult.State.SourceMappings).Visit(newPendingSelectorParameter);

                preProcessResult.State.CurrentParameter = newPendingSelectorParameter;
                preProcessResult.State.PendingSelector = Expression.Lambda(newPendingSelectorBody, newPendingSelectorParameter);
            }

            var rewritten = methodCallExpression.Update(methodCallExpression.Object, new[] { preProcessResult.Source });

            return new NavigationExpansionExpression(rewritten, preProcessResult.State, methodCallExpression.Type);
        }

        private Expression ProcessSkipTake(MethodCallExpression methodCallExpression)
        {
            var source = VisitSourceExpression(methodCallExpression.Arguments[0]);
            var preProcessResult = PreProcessTerminatingOperation(source);
            var newArgument = Visit(methodCallExpression.Arguments[1]);
            var rewritten = methodCallExpression.Update(methodCallExpression.Object, new[] { preProcessResult.Source, newArgument });

            return new NavigationExpansionExpression(rewritten, preProcessResult.State, methodCallExpression.Type);
        }

        private Expression ProcessSetOperation(MethodCallExpression methodCallExpression)
        {
            // TODO: We shouldn't terminate if both sides are identical, #16246

            var source1 = VisitSourceExpression(methodCallExpression.Arguments[0]);
            var preProcessResult1 = PreProcessTerminatingOperation(source1);

            var source2 = VisitSourceExpression(methodCallExpression.Arguments[1]);
            var preProcessResult2 = PreProcessTerminatingOperation(source2);

            // Extract the includes from each side and compare to make sure they're identical.
            // We don't allow set operations over operands with different includes.
            var pendingIncludeFindingVisitor = new PendingSelectorIncludeVisitor(skipCollectionNavigations: false, rewriteIncludes: false);
            pendingIncludeFindingVisitor.Visit(preProcessResult1.State.PendingSelector.Body);
            var pendingIncludes1 = pendingIncludeFindingVisitor.PendingIncludes;

            pendingIncludeFindingVisitor = new PendingSelectorIncludeVisitor(skipCollectionNavigations: false, rewriteIncludes: false);
            pendingIncludeFindingVisitor.Visit(preProcessResult2.State.PendingSelector.Body);
            var pendingIncludes2 = pendingIncludeFindingVisitor.PendingIncludes;

            if (pendingIncludes1.Count != pendingIncludes2.Count)
            {
                throw new NotSupportedException(CoreStrings.SetOperationWithDifferentIncludesInOperands);
            }

            foreach (var (i1, i2) in pendingIncludes1.Zip(pendingIncludes2, (i1, i2) => (i1, i2)))
            {
                if (i1.SourceMapping.RootEntityType != i2.SourceMapping.RootEntityType
                    || i1.NavTreeNode.Navigation != i2.NavTreeNode.Navigation)
                {
                    throw new NotSupportedException(CoreStrings.SetOperationWithDifferentIncludesInOperands);
                }
            }

            // If the siblings are different types, one is derived from the other the set operation returns the less derived type.
            // Find that.
            var clrType1 = preProcessResult1.State.CurrentParameter.Type;
            var clrType2 = preProcessResult2.State.CurrentParameter.Type;
            var parentState = clrType1.IsAssignableFrom(clrType2) ? preProcessResult1.State : preProcessResult2.State;

            var rewritten = methodCallExpression.Update(null, new[] { preProcessResult1.Source, preProcessResult2.Source });

            return new NavigationExpansionExpression(rewritten, parentState, methodCallExpression.Type);
        }

        private (Expression Source, NavigationExpansionExpressionState State) PreProcessTerminatingOperation(NavigationExpansionExpression source)
        {
            var (newSource, newState) = ApplyPendingOrderings(source.Operand, source.State);

            if (newState.ApplyPendingSelector)
            {
                var unbinder = new NavigationPropertyUnbindingVisitor(newState.CurrentParameter);
                var newSelectorBody = unbinder.Visit(newState.PendingSelector.Body);

                var pssmg = new PendingSelectorSourceMappingGenerator(newState.PendingSelector.Parameters[0], null);
                pssmg.Visit(newState.PendingSelector.Body);

                var selectorMethodInfo = newSource.Type.IsQueryableType()
                    ? LinqMethodHelpers.QueryableSelectMethodInfo
                    : LinqMethodHelpers.EnumerableSelectMethodInfo;

                selectorMethodInfo = selectorMethodInfo.MakeGenericMethod(
                    newState.CurrentParameter.Type,
                    newSelectorBody.Type);

                var resultSource = Expression.Call(
                    selectorMethodInfo,
                    newSource,
                    Expression.Lambda(newSelectorBody, newState.CurrentParameter));

                var newPendingSelectorParameter = Expression.Parameter(newSelectorBody.Type);
                var customRootMapping = new List<string>();

                Expression newPendingSelectorBody;
                if (newState.PendingSelector.Body is NavigationBindingExpression binding)
                {
                    newPendingSelectorBody = new NavigationBindingExpression(
                        newPendingSelectorParameter,
                        pssmg.BindingToSourceMapping[binding].NavigationTree,
                        pssmg.BindingToSourceMapping[binding].RootEntityType,
                        pssmg.BindingToSourceMapping[binding],
                        newPendingSelectorParameter.Type);
                }
                else
                {
                    // if there are any includes in the result we need to re-project the previous pending selector and re-create bindings based on new mappings
                    // so that we retain include information in case this was the last operation in the query (i.e. bindings won't be generated by processing further nodes)
                    var customRootExpression = new CustomRootExpression(newPendingSelectorParameter, customRootMapping, newPendingSelectorParameter.Type);
                    if (pssmg.SourceMappings.Where(sm => sm.NavigationTree.Flatten().Where(n => n.IncludeState == NavigationState.Pending).Any()).Any())
                    {
                        var selectorReprojector = new PendingSelectorReprojector(customRootExpression);
                        newPendingSelectorBody = selectorReprojector.Visit(newState.PendingSelector.Body);

                        var binder = new NavigationPropertyBindingVisitor(newPendingSelectorParameter, pssmg.SourceMappings);
                        newPendingSelectorBody = binder.Visit(newPendingSelectorBody);
                    }
                    else
                    {
                        newPendingSelectorBody = customRootExpression;
                    }
                }

                var resultState = new NavigationExpansionExpressionState(
                    newPendingSelectorParameter,
                    pssmg.SourceMappings,
                    Expression.Lambda(newPendingSelectorBody, newPendingSelectorParameter),
                    applyPendingSelector: false,
                    new List<(MethodInfo method, LambdaExpression keySelector)>(),
                    pendingIncludeChain: null,
                    pendingCardinalityReducingOperator: null,
                    new List<List<string>> { customRootMapping },
                    materializeCollectionNavigation: null);

                return (resultSource, resultState);
            }
            else
            {
                return (newSource, newState);
            }
        }

        private class PendingSelectorReprojector : ExpressionVisitor
        {
            private readonly List<string> _currentPath = new List<string>();
            private readonly CustomRootExpression _rootExpression;

            public PendingSelectorReprojector(CustomRootExpression rootExpression)
            {
                _rootExpression = rootExpression;
            }

            public override Expression Visit(Expression expression)
            {
                if (expression is NewExpression newExpression
                    && newExpression.Members != null)
                {
                    var newArguments = new List<Expression>();
                    for (var i = 0; i < newExpression.Arguments.Count; i++)
                    {
                        _currentPath.Add(newExpression.Members[i].Name);
                        var newArgument = Visit(newExpression.Arguments[i]);
                        if (newArgument == newExpression.Arguments[i])
                        {
                            newArgument = _rootExpression.BuildPropertyAccess(_currentPath);
                        }

                        newArguments.Add(newArgument);
                        _currentPath.RemoveAt(_currentPath.Count - 1);
                    }

                    return newExpression.Update(newArguments);
                }
                else
                {
                    return expression;
                }
            }
        }

        private MethodCallExpression TryConvertToLambdaInclude(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Arguments[1].Type != typeof(string))
            {
                return methodCallExpression;
            }

            var source = VisitSourceExpression(methodCallExpression.Arguments[0]);
            var includeString = (string)((ConstantExpression)methodCallExpression.Arguments[1]).Value;
            var includeElements = includeString.Split(new[] { "." }, StringSplitOptions.None);

            var result = (Expression)new NavigationExpansionRootExpression(source, new List<string>());

            // TODO: this is not always correct IF we allow includes in random places (e.g. after joins)
            var rootEntityType = source.State.SourceMappings.Single().RootEntityType;
            var navigations = FindNavigations(rootEntityType, includeElements[0]);
            if (navigations.Count == 0)
            {
                throw new InvalidOperationException("Invalid include path: '" + includeString + "' - couldn't find navigation for: '" + includeElements[0] + "'");
            }

            var foundIncludeChains = navigations.Select(n => new List<INavigation> { n }).ToList();
            if (includeElements.Length > 1)
            {
                for (var i = 1; i < includeElements.Length; i++)
                {
                    if (PopulateIncludeChains(foundIncludeChains, includeElements[i], i, out var newIncludeChains))
                    {
                        foundIncludeChains = newIncludeChains;
                    }
                    else
                    {
                        // we require at least one match for each include element
                        throw new InvalidOperationException("Invalid include path: '" + includeString + "' - couldn't find navigation for: '" + includeElements[i] + "'");
                    }
                }
            }

            foreach (var includeChain in foundIncludeChains)
            {
                var entityType = rootEntityType;
                var previousCollectionInclude = false;
                for (var i = 0; i < includeChain.Count; i++)
                {
                    var navigation = includeChain[i];
                    var parameter = Expression.Parameter(entityType.ClrType, entityType.ClrType.GenerateParameterName());
                    var fieldOrPropertyName = navigation.GetIdentifyingMemberInfo().Name;

                    var lambdaBody = entityType != navigation.DeclaringEntityType
                        ? Expression.PropertyOrField(Expression.Convert(parameter, navigation.DeclaringEntityType.ClrType), fieldOrPropertyName)
                        : Expression.PropertyOrField(parameter, fieldOrPropertyName);

                    var lambda = Expression.Lambda(lambdaBody, parameter);
                    var includeMethodInfo = i == 0
                        ? EntityFrameworkQueryableExtensions.IncludeMethodInfo.MakeGenericMethod(rootEntityType.ClrType, navigation.ClrType)
                        : previousCollectionInclude
                            ? EntityFrameworkQueryableExtensions.ThenIncludeAfterEnumerableMethodInfo.MakeGenericMethod(rootEntityType.ClrType, entityType.ClrType, navigation.ClrType)
                            : EntityFrameworkQueryableExtensions.ThenIncludeAfterReferenceMethodInfo.MakeGenericMethod(rootEntityType.ClrType, entityType.ClrType, navigation.ClrType);

                    result = Expression.Call(includeMethodInfo, result, lambda);
                    previousCollectionInclude = navigation.IsCollection();
                    entityType = navigation.GetTargetType();
                }
            }

            return (MethodCallExpression)result;
        }

        private bool PopulateIncludeChains(
            List<List<INavigation>> foundIncludeChains,
            string navigationName,
            int index,
            out List<List<INavigation>> newIncludeChains)
        {
            newIncludeChains = new List<List<INavigation>>();
            var matchFound = false;
            foreach (var includeChain in foundIncludeChains)
            {
                if (includeChain.Count == index)
                {
                    var entityType = includeChain[index - 1].GetTargetType();
                    var navigations = FindNavigations(entityType, navigationName);
                    foreach (var navigation in navigations)
                    {
                        var newIncludeChain = includeChain.ToList();
                        newIncludeChain.Add(navigation);
                        newIncludeChains.Add(newIncludeChain);
                    }

                    if (navigations.Count > 0)
                    {
                        matchFound = true;

                        continue;
                    }
                }

                newIncludeChains.Add(includeChain.ToList());
            }

            return matchFound;
        }

        private List<INavigation> FindNavigations(IEntityType entityType, string navigationName)
        {
            var result = new List<INavigation>();
            var navigation = entityType.FindNavigation(navigationName);
            if (navigation != null)
            {
                result.Add(navigation);
            }
            else
            {
                result = entityType.GetDerivedTypes().Select(dt => dt.FindDeclaredNavigation(navigationName)).Where(n => n != null).ToList();
            }

            return result;
        }

        private Expression ProcessInclude(MethodCallExpression methodCallExpression)
        {
            methodCallExpression = TryConvertToLambdaInclude(methodCallExpression);

            var source = VisitSourceExpression(methodCallExpression.Arguments[0]);
            var includeLambda = methodCallExpression.Arguments[1].UnwrapLambdaFromQuote();
            var (newSource, newState) = ApplyPendingOrderings(source.Operand, source.State);

            // just bind to mark all the necessary navigation for include in the future
            // include need to be delayed, in case they are not needed, e.g. when there is a projection on top that only projects scalars
            Expression remappedIncludeLambdaBody;
            if (methodCallExpression.Method.Name == nameof(EntityFrameworkQueryableExtensions.Include))
            {
                remappedIncludeLambdaBody = ReplacingExpressionVisitor.Replace(
                    includeLambda.Parameters[0], newState.PendingSelector.Body, includeLambda.Body);
            }
            else
            {
                // we can't use NavigationBindingVisitor for cases like root.Include(r => r.Collection).ThenInclude(r => r.Navigation)
                // because the type mismatch (trying to compose Navigation access on the ICollection from the first include
                // we manually construct navigation binding that should be a root of the new include, its EntityType being the element of the previously included collection
                // pendingIncludeLambda is only used for marking the includes - as long as the NavigationTreeNodes are correct it should be fine
                if (newState.PendingIncludeChain.NavigationTreeNode.IsCollection)
                {
                    var newIncludeLambdaRoot = new NavigationBindingExpression(
                        newState.CurrentParameter,
                        newState.PendingIncludeChain.NavigationTreeNode,
                        newState.PendingIncludeChain.EntityType,
                        newState.PendingIncludeChain.SourceMapping,
                        includeLambda.Parameters[0].Type);

                    remappedIncludeLambdaBody = new ExpressionReplacingVisitor(includeLambda.Parameters[0], newIncludeLambdaRoot).Visit(includeLambda.Body);
                }
                else
                {
                    var pendingIncludeChainLambda = Expression.Lambda(newState.PendingIncludeChain, newState.CurrentParameter);
                    remappedIncludeLambdaBody = ReplacingExpressionVisitor.Replace(
                        includeLambda.Parameters[0], pendingIncludeChainLambda.Body, includeLambda.Body);
                }
            }

            var binder = new NavigationPropertyBindingVisitor(newState.PendingSelector.Parameters[0], newState.SourceMappings, bindInclude: true);
            var boundIncludeLambdaBody = binder.Visit(remappedIncludeLambdaBody);

            if (boundIncludeLambdaBody is NavigationBindingExpression navigationBindingExpression)
            {
                newState.PendingIncludeChain = navigationBindingExpression;
            }
            else
            {
                throw new InvalidOperationException("Incorrect include argument: " + includeLambda);
            }

            return new NavigationExpansionExpression(newSource, newState, methodCallExpression.Type);
        }

        private MethodCallExpression SimplifyPredicateMethod(MethodCallExpression methodCallExpression, bool queryable)
        {
            var whereMethodInfo = queryable
                ? LinqMethodHelpers.QueryableWhereMethodInfo
                : LinqMethodHelpers.EnumerableWhereMethodInfo;

            var typeArgument = methodCallExpression.Method.GetGenericArguments()[0];
            whereMethodInfo = whereMethodInfo.MakeGenericMethod(typeArgument);
            var whereMethodCall = Expression.Call(whereMethodInfo, methodCallExpression.Arguments[0], methodCallExpression.Arguments[1]);

            var newMethodInfo = GetNewMethodInfo(methodCallExpression.Method.Name, queryable);
            newMethodInfo = newMethodInfo.MakeGenericMethod(typeArgument);

            return Expression.Call(newMethodInfo, whereMethodCall);
        }

        private MethodInfo GetNewMethodInfo(string name, bool queryable)
        {
            if (queryable)
            {
                switch (name)
                {
                    case nameof(Queryable.Count):
                        return LinqMethodHelpers.QueryableCountMethodInfo;

                    case nameof(Queryable.LongCount):
                        return LinqMethodHelpers.QueryableLongCountMethodInfo;

                    case nameof(Queryable.First):
                        return LinqMethodHelpers.QueryableFirstMethodInfo;

                    case nameof(Queryable.FirstOrDefault):
                        return LinqMethodHelpers.QueryableFirstOrDefaultMethodInfo;

                    case nameof(Queryable.Single):
                        return LinqMethodHelpers.QueryableSingleMethodInfo;

                    case nameof(Queryable.SingleOrDefault):
                        return LinqMethodHelpers.QueryableSingleOrDefaultMethodInfo;

                    case nameof(Queryable.Any):
                        return LinqMethodHelpers.QueryableAnyMethodInfo;
                }
            }
            else
            {
                switch (name)
                {
                    case nameof(Enumerable.Count):
                        return LinqMethodHelpers.EnumerableCountMethodInfo;

                    case nameof(Enumerable.LongCount):
                        return LinqMethodHelpers.EnumerableLongCountMethodInfo;

                    case nameof(Enumerable.First):
                        return LinqMethodHelpers.EnumerableFirstMethodInfo;

                    case nameof(Enumerable.FirstOrDefault):
                        return LinqMethodHelpers.EnumerableFirstOrDefaultMethodInfo;

                    case nameof(Enumerable.Single):
                        return LinqMethodHelpers.EnumerableSingleMethodInfo;

                    case nameof(Enumerable.SingleOrDefault):
                        return LinqMethodHelpers.EnumerableSingleOrDefaultMethodInfo;

                    case nameof(Enumerable.Any):
                        return LinqMethodHelpers.EnumerableAnyMethodInfo;
                }
            }

            throw new InvalidOperationException("Invalid method name: " + name);
        }

        private Expression ProcessCardinalityReducingOperation(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.MethodIsClosedFormOf(LinqMethodHelpers.QueryableFirstPredicateMethodInfo)
                || methodCallExpression.Method.MethodIsClosedFormOf(LinqMethodHelpers.QueryableFirstOrDefaultPredicateMethodInfo)
                || methodCallExpression.Method.MethodIsClosedFormOf(LinqMethodHelpers.QueryableSinglePredicateMethodInfo)
                || methodCallExpression.Method.MethodIsClosedFormOf(LinqMethodHelpers.QueryableSingleOrDefaultPredicateMethodInfo))
            {
                return Visit(SimplifyPredicateMethod(methodCallExpression, queryable: true));
            }

            if (methodCallExpression.Method.MethodIsClosedFormOf(LinqMethodHelpers.EnumerableFirstPredicateMethodInfo)
                || methodCallExpression.Method.MethodIsClosedFormOf(LinqMethodHelpers.EnumerableFirstOrDefaultPredicateMethodInfo)
                || methodCallExpression.Method.MethodIsClosedFormOf(LinqMethodHelpers.EnumerableSinglePredicateMethodInfo)
                || methodCallExpression.Method.MethodIsClosedFormOf(LinqMethodHelpers.EnumerableSingleOrDefaultPredicateMethodInfo))
            {
                return Visit(SimplifyPredicateMethod(methodCallExpression, queryable: false));
            }

            var source = VisitSourceExpression(methodCallExpression.Arguments[0]);
            var (newSource, newState) = ApplyPendingOrderings(source.Operand, source.State);
            newState.PendingCardinalityReducingOperator = methodCallExpression.Method;

            return new NavigationExpansionExpression(newSource, newState, methodCallExpression.Type);
        }

        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            if (constantExpression.Value != null
                && constantExpression.Value.GetType().IsGenericType
                && constantExpression.Value.GetType().GetGenericTypeDefinition() == typeof(EntityQueryable<>))
            {
                var elementType = constantExpression.Value.GetType().GetSequenceType();
                var entityType = _queryCompilationContext.Model.FindEntityType(elementType);

                return NavigationExpansionHelpers.CreateNavigationExpansionRoot(constantExpression, entityType, materializeCollectionNavigation: null);
            }

            return base.VisitConstant(constantExpression);
        }

        private (Expression Source, NavigationExpansionExpressionState State) ApplyPendingOrderings(
            Expression source, NavigationExpansionExpressionState state)
        {
            foreach (var pendingOrdering in state.PendingOrderings)
            {
                var remappedKeySelectorBody = new ExpressionReplacingVisitor(pendingOrdering.keySelector.Parameters[0], state.CurrentParameter).Visit(pendingOrdering.keySelector.Body);
                var newSelectorBody = new NavigationPropertyUnbindingVisitor(state.CurrentParameter).Visit(remappedKeySelectorBody);
                var newSelector = Expression.Lambda(newSelectorBody, state.CurrentParameter);
                var orderingMethod = pendingOrdering.method.MakeGenericMethod(state.CurrentParameter.Type, newSelectorBody.Type);
                source = Expression.Call(orderingMethod, source, newSelector);
            }

            state.PendingOrderings.Clear();

            return (source, state);
        }

        private (Expression Source, Expression LambdaBody, NavigationExpansionExpressionState State) FindAndApplyNavigations(
            Expression source,
            LambdaExpression lambda,
            NavigationExpansionExpressionState state)
        {
            if (state.PendingSelector == null)
            {
                return (source, lambda.Body, state);
            }

            var remappedLambdaBody = ReplacingExpressionVisitor.Replace(
                lambda.Parameters[0], state.PendingSelector.Body, lambda.Body);

            var binder = new NavigationPropertyBindingVisitor(
                state.PendingSelector.Parameters[0],
                state.SourceMappings);

            var boundLambdaBody = binder.Visit(remappedLambdaBody);
            boundLambdaBody = new NavigationComparisonOptimizingVisitor().Visit(boundLambdaBody);
            boundLambdaBody = new CollectionNavigationRewritingVisitor(state.CurrentParameter).Visit(boundLambdaBody);
            boundLambdaBody = Visit(boundLambdaBody);

            var result = (source, parameter: state.CurrentParameter);
            var applyPendingSelector = state.ApplyPendingSelector;

            foreach (var sourceMapping in state.SourceMappings)
            {
                if (sourceMapping.NavigationTree.Flatten().Any(n => n.ExpansionState == NavigationState.Pending))
                {
                    foreach (var navigationTree in sourceMapping.NavigationTree.Children.Where(n => !n.IsCollection))
                    {
                        result = NavigationExpansionHelpers.AddNavigationJoin(
                            result.source,
                            result.parameter,
                            sourceMapping,
                            navigationTree,
                            state,
                            new List<INavigation>(),
                            include: false);
                    }

                    applyPendingSelector = true;
                }
            }

            var pendingSelector = state.PendingSelector;
            if (state.CurrentParameter != result.parameter)
            {
                var pendingSelectorBody = new ExpressionReplacingVisitor(state.CurrentParameter, result.parameter).Visit(state.PendingSelector.Body);
                pendingSelector = Expression.Lambda(pendingSelectorBody, result.parameter);
                boundLambdaBody = new ExpressionReplacingVisitor(state.CurrentParameter, result.parameter).Visit(boundLambdaBody);
            }

            var newState = new NavigationExpansionExpressionState(
                result.parameter,
                state.SourceMappings,
                pendingSelector,
                applyPendingSelector,
                state.PendingOrderings,
                state.PendingIncludeChain,
                state.PendingCardinalityReducingOperator,
                state.CustomRootMappings,
                state.MaterializeCollectionNavigation);

            return (result.source, LambdaBody: boundLambdaBody, State: newState);
        }

        private (LambdaExpression lambda, NavigationExpansionExpressionState state) RemapTwoArgumentResultSelector(
            LambdaExpression resultSelector,
            NavigationExpansionExpressionState outerState,
            NavigationExpansionExpressionState innerState)
        {
            var remappedResultSelectorBody =
                ReplacingExpressionVisitor.Replace(
                    resultSelector.Parameters[0], outerState.PendingSelector.Body,
                    resultSelector.Parameters[1], innerState.PendingSelector.Body,
                    resultSelector.Body);

            var outerBinder = new NavigationPropertyBindingVisitor(
                outerState.CurrentParameter,
                outerState.SourceMappings);

            var innerBinder = new NavigationPropertyBindingVisitor(
                innerState.CurrentParameter,
                innerState.SourceMappings);

            var boundResultSelectorBody = outerBinder.Visit(remappedResultSelectorBody);
            boundResultSelectorBody = innerBinder.Visit(boundResultSelectorBody);

            foreach (var outerCustomRootMapping in outerState.CustomRootMappings)
            {
                outerCustomRootMapping.Insert(0, nameof(TransparentIdentifier<object, object>.Outer));
            }

            foreach (var outerSourceMapping in outerState.SourceMappings)
            {
                foreach (var navigationTreeNode in outerSourceMapping.NavigationTree.Flatten().Where(n => n.ExpansionState == NavigationState.Complete))
                {
                    navigationTreeNode.ToMapping.Insert(0, nameof(TransparentIdentifier<object, object>.Outer));
                    foreach (var fromMapping in navigationTreeNode.FromMappings)
                    {
                        fromMapping.Insert(0, nameof(TransparentIdentifier<object, object>.Outer));
                    }
                }
            }

            foreach (var innerCustomRootMapping in innerState.CustomRootMappings)
            {
                innerCustomRootMapping.Insert(0, nameof(TransparentIdentifier<object, object>.Inner));
            }

            foreach (var innerSourceMapping in innerState.SourceMappings)
            {
                foreach (var navigationTreeNode in innerSourceMapping.NavigationTree.Flatten().Where(n => n.ExpansionState == NavigationState.Complete))
                {
                    navigationTreeNode.ToMapping.Insert(0, nameof(TransparentIdentifier<object, object>.Inner));
                    foreach (var fromMapping in navigationTreeNode.FromMappings)
                    {
                        fromMapping.Insert(0, nameof(TransparentIdentifier<object, object>.Inner));
                    }
                }
            }

            var resultType = typeof(TransparentIdentifier<,>).MakeGenericType(outerState.CurrentParameter.Type, innerState.CurrentParameter.Type);
            var transparentIdentifierCtorInfo = resultType.GetTypeInfo().GetConstructors().Single();
            var transparentIdentifierParameter = Expression.Parameter(resultType, "join");

            var newPendingSelectorBody = new ExpressionReplacingVisitor(outerState.CurrentParameter, transparentIdentifierParameter).Visit(boundResultSelectorBody);
            newPendingSelectorBody = new ExpressionReplacingVisitor(innerState.CurrentParameter, transparentIdentifierParameter).Visit(newPendingSelectorBody);

            var newState = new NavigationExpansionExpressionState(
                transparentIdentifierParameter,
                outerState.SourceMappings.Concat(innerState.SourceMappings).ToList(),
                Expression.Lambda(newPendingSelectorBody, transparentIdentifierParameter),
                applyPendingSelector: true,
                new List<(MethodInfo method, LambdaExpression keySelector)>(),
                pendingIncludeChain: null,
                pendingCardinalityReducingOperator: null,
                outerState.CustomRootMappings.Concat(innerState.CustomRootMappings).ToList(),
                materializeCollectionNavigation: null);

            var transparentIdentifierOuterMemberInfo = resultType.GetTypeInfo().GetDeclaredField("Outer");
            var transparentIdentifierInnerMemberInfo = resultType.GetTypeInfo().GetDeclaredField("Inner");

            var lambda = Expression.Lambda(
                Expression.New(
                    transparentIdentifierCtorInfo,
                    new[] { outerState.CurrentParameter, innerState.CurrentParameter },
                    new[] { transparentIdentifierOuterMemberInfo, transparentIdentifierInnerMemberInfo }),
                outerState.CurrentParameter,
                innerState.CurrentParameter);

            return (lambda, state: newState);
        }

        private class PendingSelectorSourceMappingGenerator : ExpressionVisitor
        {
            private readonly ParameterExpression _rootParameter;
            private readonly List<string> _currentPath = new List<string>();
            private readonly IEntityType _entityTypeOverride;

            public readonly List<SourceMapping> SourceMappings = new List<SourceMapping>();

            public readonly Dictionary<NavigationBindingExpression, SourceMapping> BindingToSourceMapping
                = new Dictionary<NavigationBindingExpression, SourceMapping>();

            public PendingSelectorSourceMappingGenerator(ParameterExpression rootParameter, IEntityType entityTypeOverride)
            {
                _rootParameter = rootParameter;
                _entityTypeOverride = entityTypeOverride;
            }

            protected override Expression VisitMember(MemberExpression memberExpression) => memberExpression;
            protected override Expression VisitInvocation(InvocationExpression invocationExpression) => invocationExpression;
            protected override Expression VisitLambda<T>(Expression<T> lambdaExpression) => lambdaExpression;
            protected override Expression VisitTypeBinary(TypeBinaryExpression typeBinaryExpression) => typeBinaryExpression;

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
                => methodCallExpression.IsEFProperty()
                ? methodCallExpression
                : base.VisitMethodCall(methodCallExpression);

            protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
            {
                Visit(conditionalExpression.IfTrue);
                Visit(conditionalExpression.IfFalse);

                return conditionalExpression;
            }

            protected override Expression VisitBinary(BinaryExpression binaryExpression)
            {
                return binaryExpression.NodeType == ExpressionType.Coalesce
                    ? base.VisitBinary(binaryExpression)
                    : binaryExpression;
            }

            protected override Expression VisitNew(NewExpression newExpression)
            {
                // TODO: when constructing a DTO, there will be arguments present, but no members - is it correct to just skip in this case?
                if (newExpression.Members != null)
                {
                    for (var i = 0; i < newExpression.Arguments.Count; i++)
                    {
                        _currentPath.Add(newExpression.Members[i].Name);
                        Visit(newExpression.Arguments[i]);
                        _currentPath.RemoveAt(_currentPath.Count - 1);
                    }
                }

                return newExpression;
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                if (extensionExpression is NavigationBindingExpression navigationBindingExpression)
                {
                    if (navigationBindingExpression.RootParameter == _rootParameter)
                    {
                        var sourceMapping = new SourceMapping
                        {
                            RootEntityType = _entityTypeOverride ?? navigationBindingExpression.EntityType,
                        };

                        var navigationTreeRoot = NavigationTreeNode.CreateRoot(sourceMapping, _currentPath.ToList(), navigationBindingExpression.NavigationTreeNode.Optional);

                        IncludeHelpers.CopyIncludeInformation(navigationBindingExpression.NavigationTreeNode, navigationTreeRoot, sourceMapping);

                        sourceMapping.NavigationTree = navigationTreeRoot;

                        SourceMappings.Add(sourceMapping);
                        BindingToSourceMapping[navigationBindingExpression] = sourceMapping;
                    }

                    return extensionExpression;
                }

                if (extensionExpression is CustomRootExpression customRootExpression)
                {
                    return customRootExpression;
                }

                return base.VisitExtension(extensionExpression);
            }
        }
    }
}
