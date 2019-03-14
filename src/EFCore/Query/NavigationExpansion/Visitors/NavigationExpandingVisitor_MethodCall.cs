// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion.Visitors
{
    public partial class NavigationExpandingVisitor : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.IsEFPropertyMethod())
            {
                var newSource = Visit(methodCallExpression.Arguments[0]);
                if (newSource is NavigationExpansionExpression navigationExpansionExpression
                    && navigationExpansionExpression.State.PendingCardinalityReducingOperator != null)
                {
                    return ProcessMemberPushdown(
                        newSource,
                        navigationExpansionExpression,
                        efProperty: true,
                        memberInfo: null,
                        (string)((ConstantExpression)methodCallExpression.Arguments[1]).Value,
                        methodCallExpression.Type);
                }

                return methodCallExpression.Update(methodCallExpression.Object, new[] { newSource, methodCallExpression.Arguments[1] });
            }

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

                case "AsTracking":
                case "AsNoTracking":
                    return ProcessBasicTerminatingOperation(methodCallExpression);

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

                case "Include":
                case "ThenInclude":
                    return ProcessInclude(methodCallExpression);

                //TODO: should we have relational version of this? - probably
                case "FromSqlRaw":
                    return ProcessFromRawSql(methodCallExpression);

                case nameof(EntityFrameworkQueryableExtensions.TagWith):
                    return ProcessWithTag(methodCallExpression);

                default:
                    return base.VisitMethodCall(methodCallExpression);
            }
        }

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
                pendingTags: new List<string>(),
                new List<List<string>> { customRootMapping },
                materializeCollectionNavigation: null);

            return new NavigationExpansionExpression(result, state, result.Type);
        }

        private void AdjustCurrentParameterName(
            NavigationExpansionExpressionState state,
            string newParameterName)
        {
            if (state.CurrentParameter.Name == null && newParameterName != null)
            {
                var newParameter = Expression.Parameter(state.CurrentParameter.Type, newParameterName);
                state.PendingSelector = (LambdaExpression)new ExpressionReplacingVisitor(state.CurrentParameter, newParameter).Visit(state.PendingSelector);
                state.CurrentParameter = newParameter;
            }
        }

        private Expression ProcessWhere(MethodCallExpression methodCallExpression)
        {
            var source = VisitSourceExpression(methodCallExpression.Arguments[0]);
            var predicate = methodCallExpression.Arguments[1].UnwrapQuote();
            AdjustCurrentParameterName(source.State, predicate.Parameters[0].Name);

            var appliedNavigationsResult = FindAndApplyNavigations(source.Operand, predicate, source.State);
            var newPredicateBody = new NavigationPropertyUnbindingVisitor(appliedNavigationsResult.state.CurrentParameter).Visit(appliedNavigationsResult.lambdaBody);
            var newPredicateLambda = Expression.Lambda(newPredicateBody, appliedNavigationsResult.state.CurrentParameter);
            var appliedOrderingsResult = ApplyPendingOrderings(appliedNavigationsResult.source, appliedNavigationsResult.state);

            var newMethodInfo = methodCallExpression.Method.GetGenericMethodDefinition().MakeGenericMethod(appliedOrderingsResult.state.CurrentParameter.Type);
            var rewritten = Expression.Call(newMethodInfo, appliedOrderingsResult.source, newPredicateLambda);

            return new NavigationExpansionExpression(
                rewritten,
                appliedOrderingsResult.state,
                methodCallExpression.Type);
        }

        private Expression ProcessSelect(MethodCallExpression methodCallExpression)
        {
            var source = VisitSourceExpression(methodCallExpression.Arguments[0]);
            var selector = methodCallExpression.Arguments[1].UnwrapQuote();
            AdjustCurrentParameterName(source.State, selector.Parameters[0].Name);

            return ProcessSelectCore(source.Operand, source.State, selector, methodCallExpression.Type);
        }

        private Expression ProcessSelectCore(Expression source, NavigationExpansionExpressionState state, LambdaExpression selector, Type resultType)
        {
            var appliedNavigationsResult = FindAndApplyNavigations(source, selector, state);
            appliedNavigationsResult.state.PendingSelector = Expression.Lambda(appliedNavigationsResult.lambdaBody, appliedNavigationsResult.state.CurrentParameter);

            // we could force apply pending selector only for non-identity projections
            // but then we lose information about variable names, e.g. ctx.Customers.Select(x => x)
            appliedNavigationsResult.state.ApplyPendingSelector = true;

            var appliedOrderingsResult = ApplyPendingOrderings(appliedNavigationsResult.source, appliedNavigationsResult.state);

            var resultElementType = resultType.TryGetSequenceType();
            if (resultElementType != null)
            {
                if (resultElementType != appliedOrderingsResult.state.PendingSelector.Body.Type)
                {
                    resultType = resultType.GetGenericTypeDefinition().MakeGenericType(appliedOrderingsResult.state.PendingSelector.Body.Type);
                }
            }
            else
            {
                resultType = appliedOrderingsResult.state.PendingSelector.Body.Type;
            }

            return new NavigationExpansionExpression(
                appliedOrderingsResult.source,
                appliedOrderingsResult.state,
                resultType);
        }

        private Expression ProcessOrderBy(MethodCallExpression methodCallExpression)
        {
            var source = VisitSourceExpression(methodCallExpression.Arguments[0]);
            var keySelector = methodCallExpression.Arguments[1].UnwrapQuote();
            AdjustCurrentParameterName(source.State, keySelector.Parameters[0].Name);

            var appliedNavigationsResult = FindAndApplyNavigations(source.Operand, keySelector, source.State);
            var pendingOrdering = (method: methodCallExpression.Method.GetGenericMethodDefinition(), keySelector: Expression.Lambda(appliedNavigationsResult.lambdaBody, appliedNavigationsResult.state.CurrentParameter));
            var appliedOrderingsResult = ApplyPendingOrderings(appliedNavigationsResult.source, appliedNavigationsResult.state);

            appliedOrderingsResult.state.PendingOrderings.Add(pendingOrdering);

            return new NavigationExpansionExpression(
                appliedOrderingsResult.source,
                appliedOrderingsResult.state,
                methodCallExpression.Type);
        }

        private Expression ProcessThenByBy(MethodCallExpression methodCallExpression)
        {
            var source = VisitSourceExpression(methodCallExpression.Arguments[0]);
            var keySelector = methodCallExpression.Arguments[1].UnwrapQuote();
            AdjustCurrentParameterName(source.State, keySelector.Parameters[0].Name);

            var appliedNavigationsResult = FindAndApplyNavigations(source.Operand, keySelector, source.State);

            var pendingOrdering = (method: methodCallExpression.Method.GetGenericMethodDefinition(), keySelector: Expression.Lambda(appliedNavigationsResult.lambdaBody, appliedNavigationsResult.state.CurrentParameter));
            appliedNavigationsResult.state.PendingOrderings.Add(pendingOrdering);

            return new NavigationExpansionExpression(
                appliedNavigationsResult.source,
                appliedNavigationsResult.state,
                methodCallExpression.Type);
        }

        private Expression ProcessSelectMany(MethodCallExpression methodCallExpression)
        {
            var outerSourceNee = VisitSourceExpression(methodCallExpression.Arguments[0]);
            var collectionSelector = methodCallExpression.Arguments[1].UnwrapQuote();
            AdjustCurrentParameterName(outerSourceNee.State, collectionSelector.Parameters[0].Name);

            var applyNavigsationsResult = FindAndApplyNavigations(outerSourceNee.Operand, collectionSelector, outerSourceNee.State);
            var applyOrderingsResult = ApplyPendingOrderings(applyNavigsationsResult.source, applyNavigsationsResult.state);

            var outerSource = applyOrderingsResult.source;
            var outerState = applyOrderingsResult.state;

            var collectionSelectorNavigationExpansionExpression = applyNavigsationsResult.lambdaBody as NavigationExpansionExpression
                ?? (applyNavigsationsResult.lambdaBody as NavigationExpansionRootExpression)?.Unwrap() as NavigationExpansionExpression;

            if (collectionSelectorNavigationExpansionExpression != null)
            {
                var collectionSelectorState = collectionSelectorNavigationExpansionExpression.State;
                var collectionSelectorLambdaBody = collectionSelectorNavigationExpansionExpression.Operand;

                // in case collection selector is a "naked" collection navigation, we need to remove MaterializeCollectionNavigation
                // it's not needed for SelectMany collection selectors as they are not directly projected
                collectionSelectorState.MaterializeCollectionNavigation = null;

                if (methodCallExpression.Method.MethodIsClosedFormOf(LinqMethodHelpers.QueryableSelectManyWithResultOperatorMethodInfo))
                {
                    if (outerState.CurrentParameter.Name == null
                        && outerState.CurrentParameter.Name != methodCallExpression.Arguments[2].UnwrapQuote().Parameters[0].Name)
                    {
                        var newOuterParameter = Expression.Parameter(outerState.CurrentParameter.Type, methodCallExpression.Arguments[2].UnwrapQuote().Parameters[0].Name);
                        outerState.PendingSelector = (LambdaExpression)new ExpressionReplacingVisitor(outerState.CurrentParameter, newOuterParameter).Visit(outerState.PendingSelector);
                        collectionSelectorLambdaBody = new ExpressionReplacingVisitor(outerState.CurrentParameter, newOuterParameter).Visit(collectionSelectorLambdaBody);
                        outerState.CurrentParameter = newOuterParameter;
                    }

                    if (collectionSelectorState.CurrentParameter.Name == null
                        && collectionSelectorState.CurrentParameter.Name != methodCallExpression.Arguments[2].UnwrapQuote().Parameters[1].Name)
                    {
                        var newInnerParameter = Expression.Parameter(collectionSelectorState.CurrentParameter.Type, methodCallExpression.Arguments[2].UnwrapQuote().Parameters[1].Name);
                        collectionSelectorState.PendingSelector = (LambdaExpression)new ExpressionReplacingVisitor(collectionSelectorState.CurrentParameter, newInnerParameter).Visit(collectionSelectorState.PendingSelector);
                        collectionSelectorState.CurrentParameter = newInnerParameter;
                    }
                }

                if (methodCallExpression.Method.MethodIsClosedFormOf(LinqMethodHelpers.QueryableSelectManyWithResultOperatorMethodInfo)
                    && (collectionSelectorState.CurrentParameter.Name == null
                        || collectionSelectorState.CurrentParameter.Name != methodCallExpression.Arguments[2].UnwrapQuote().Parameters[1].Name))
                {
                    // TODO: should we rename the second parameter according to the second parameter of the result selector instead?
                    var newParameter = Expression.Parameter(collectionSelectorState.CurrentParameter.Type, methodCallExpression.Arguments[2].UnwrapQuote().Parameters[1].Name);
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

                var resultSelector = methodCallExpression.Arguments[2].UnwrapQuote();

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
                result.state.PendingSelector = Expression.Lambda(result.lambdaBody, result.state.CurrentParameter);

                return new NavigationExpansionExpression(
                    result.source,
                    result.state,
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

                // TODO: simply coyping ToMapping might not be correct for very complex cases where the child mapping is not purely Inner/Outer but has some properties from preivous anonymous projections
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
                pendingTags: collectionSelectorState.PendingTags.ToList(),
                customRootMappings: customRootMappingMapping.Values.ToList(),
                materializeCollectionNavigation: null);

            collectionSelectorState.PendingTags.Clear();

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
                copy.ExpansionMode = child.ExpansionMode;
                copy.Included = child.Included;

                // TODO: simply coyping ToMapping might not be correct for very complex cases where the child mapping is not purely Inner/Outer but has some properties from preivous anonymous projections
                // we should recognize and filter those out, however this is theoretical at this point - scenario is not supported and likely won't be in the foreseeable future
                copy.ToMapping = child.ToMapping.ToList();
                mapping[child] = copy;
                CopyNavigationTree(child, copy, newSourceMapping, ref mapping);
            }
        }

        private class SelectManyCollectionPendingSelectorRemapper : ExpressionVisitor
        {
            private ParameterExpression _oldParameter;
            private ParameterExpression _newParameter;
            private Dictionary<SourceMapping, SourceMapping> _sourceMappingMapping;
            private Dictionary<NavigationTreeNode, NavigationTreeNode> _navigationTreeNodeMapping;
            private Dictionary<List<string>, List<string>> _customRootMappingMapping;

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

            var outerKeySelector = methodCallExpression.Arguments[2].UnwrapQuote();
            var innerKeySelector = methodCallExpression.Arguments[3].UnwrapQuote();
            var resultSelector = methodCallExpression.Arguments[4].UnwrapQuote();

            AdjustCurrentParameterName(outerSource.State, outerKeySelector.Parameters[0].Name);
            AdjustCurrentParameterName(innerSource.State, innerKeySelector.Parameters[0].Name);

            var outerApplyNavigationsResult = FindAndApplyNavigations(outerSource.Operand, outerKeySelector, outerSource.State);
            var innerApplyNavigationsResult = FindAndApplyNavigations(innerSource.Operand, innerKeySelector, innerSource.State);

            var newOuterKeySelectorBody = new NavigationPropertyUnbindingVisitor(outerApplyNavigationsResult.state.CurrentParameter).Visit(outerApplyNavigationsResult.lambdaBody);
            var newInnerKeySelectorBody = new NavigationPropertyUnbindingVisitor(innerApplyNavigationsResult.state.CurrentParameter).Visit(innerApplyNavigationsResult.lambdaBody);

            var outerApplyOrderingsResult = ApplyPendingOrderings(outerApplyNavigationsResult.source, outerApplyNavigationsResult.state);
            var innerApplyOrderingsResult = ApplyPendingOrderings(innerApplyNavigationsResult.source, innerApplyNavigationsResult.state);

            var resultSelectorRemap = RemapTwoArgumentResultSelector(resultSelector, outerApplyOrderingsResult.state, innerApplyOrderingsResult.state);

            var newMethodInfo = methodCallExpression.Method.GetGenericMethodDefinition().MakeGenericMethod(
                outerApplyOrderingsResult.state.CurrentParameter.Type,
                innerApplyOrderingsResult.state.CurrentParameter.Type,
                outerApplyNavigationsResult.lambdaBody.Type,
                resultSelectorRemap.lambda.Body.Type);

            var rewritten = Expression.Call(
                newMethodInfo,
                outerApplyOrderingsResult.source,
                innerApplyOrderingsResult.source,
                Expression.Lambda(newOuterKeySelectorBody, outerApplyOrderingsResult.state.CurrentParameter),
                Expression.Lambda(newInnerKeySelectorBody, innerApplyOrderingsResult.state.CurrentParameter),
                Expression.Lambda(resultSelectorRemap.lambda.Body, outerApplyOrderingsResult.state.CurrentParameter, innerApplyOrderingsResult.state.CurrentParameter));

            // temporarily change selector to ti => ti for purpose of finding & expanding navigations in the pending selector lambda itself
            var pendingSelector = resultSelectorRemap.state.PendingSelector;
            resultSelectorRemap.state.PendingSelector = Expression.Lambda(resultSelectorRemap.state.PendingSelector.Parameters[0], resultSelectorRemap.state.PendingSelector.Parameters[0]);
            var result = FindAndApplyNavigations(rewritten, pendingSelector, resultSelectorRemap.state);
            result.state.PendingSelector = Expression.Lambda(result.lambdaBody, result.state.CurrentParameter);

            return new NavigationExpansionExpression(
                result.source,
                result.state,
                methodCallExpression.Type);
        }

        private Expression ProcessGroupJoin(MethodCallExpression methodCallExpression)
        {
            var outerSource = VisitSourceExpression(methodCallExpression.Arguments[0]);
            var innerSource = VisitSourceExpression(methodCallExpression.Arguments[1]);

            var outerKeySelector = methodCallExpression.Arguments[2].UnwrapQuote();
            var innerKeySelector = methodCallExpression.Arguments[3].UnwrapQuote();
            var resultSelector = methodCallExpression.Arguments[4].UnwrapQuote();

            AdjustCurrentParameterName(outerSource.State, outerKeySelector.Parameters[0].Name);
            AdjustCurrentParameterName(innerSource.State, innerKeySelector.Parameters[0].Name);

            var outerApplyNavigationsResult = FindAndApplyNavigations(outerSource.Operand, outerKeySelector, outerSource.State);
            var innerApplyNavigationsResult = FindAndApplyNavigations(innerSource.Operand, innerKeySelector, innerSource.State);

            var newOuterKeySelectorBody = new NavigationPropertyUnbindingVisitor(outerApplyNavigationsResult.state.CurrentParameter).Visit(outerApplyNavigationsResult.lambdaBody);
            var newInnerKeySelectorBody = new NavigationPropertyUnbindingVisitor(innerApplyNavigationsResult.state.CurrentParameter).Visit(innerApplyNavigationsResult.lambdaBody);

            var outerApplyOrderingsResult = ApplyPendingOrderings(outerApplyNavigationsResult.source, outerApplyNavigationsResult.state);
            var innerApplyOrderingsResult = ApplyPendingOrderings(innerApplyNavigationsResult.source, innerApplyNavigationsResult.state);

            var resultSelectorBody = resultSelector.Body;
            var remappedResultSelectorBody = ExpressionExtensions.CombineAndRemap(resultSelector.Body, resultSelector.Parameters[0], outerApplyOrderingsResult.state.PendingSelector.Body);

            var groupingParameter = resultSelector.Parameters[1];
            var newGroupingParameter = Expression.Parameter(typeof(IEnumerable<>).MakeGenericType(innerApplyOrderingsResult.state.CurrentParameter.Type), "new_" + groupingParameter.Name);

            var groupingMapping = new List<string> { nameof(TransparentIdentifier<object, object>.Inner) };

            // TODO: need to create the new state and copy includes from the old one, rather than simply copying it over to grouping
            // this shouldn't be a problem currently since we don't support queries that compose on the grouping
            // but when we do, state can't be shared - otherwise any nav expansion that affects the flattened part of the GroupJoin would be incorrectly propagated to the grouping as well
            var newGrouping = new NavigationExpansionExpression(newGroupingParameter, innerApplyOrderingsResult.state, groupingParameter.Type);

            remappedResultSelectorBody = new ExpressionReplacingVisitor(
                groupingParameter,
                new NavigationExpansionRootExpression(newGrouping, groupingMapping)).Visit(remappedResultSelectorBody);

            foreach (var outerCustomRootMapping in outerApplyOrderingsResult.state.CustomRootMappings)
            {
                outerCustomRootMapping.Insert(0, nameof(TransparentIdentifier<object, object>.Outer));
            }

            foreach (var outerSourceMapping in outerApplyOrderingsResult.state.SourceMappings)
            {
                foreach (var navigationTreeNode in outerSourceMapping.NavigationTree.Flatten().Where(n => n.ExpansionMode == NavigationTreeNodeExpansionMode.ReferenceComplete))
                {
                    navigationTreeNode.ToMapping.Insert(0, nameof(TransparentIdentifier<object, object>.Outer));
                    foreach (var fromMapping in navigationTreeNode.FromMappings)
                    {
                        fromMapping.Insert(0, nameof(TransparentIdentifier<object, object>.Outer));
                    }
                }
            }

            var resultType = typeof(TransparentIdentifier<,>).MakeGenericType(outerApplyOrderingsResult.state.CurrentParameter.Type, newGroupingParameter.Type);
            var transparentIdentifierCtorInfo = resultType.GetTypeInfo().GetConstructors().Single();
            var transparentIdentifierParameter = Expression.Parameter(resultType, "groupjoin");

            var newPendingSelectorBody = new ExpressionReplacingVisitor(outerApplyOrderingsResult.state.CurrentParameter, transparentIdentifierParameter).Visit(remappedResultSelectorBody);
            newPendingSelectorBody = new ExpressionReplacingVisitor(newGroupingParameter, transparentIdentifierParameter).Visit(newPendingSelectorBody);

            // for GroupJoin inner sources are not available, only the outer source mappings and the custom mappings for the grouping
            var newState = new NavigationExpansionExpressionState(
                transparentIdentifierParameter,
                outerApplyOrderingsResult.state.SourceMappings,
                Expression.Lambda(newPendingSelectorBody, transparentIdentifierParameter),
                applyPendingSelector: true,
                outerApplyOrderingsResult.state.PendingOrderings,
                outerApplyOrderingsResult.state.PendingIncludeChain,
                outerApplyOrderingsResult.state.PendingCardinalityReducingOperator,
                outerApplyOrderingsResult.state.PendingTags,
                outerApplyOrderingsResult.state.CustomRootMappings.Concat(new[] { groupingMapping }).ToList(),
                materializeCollectionNavigation: null);

            var lambda = Expression.Lambda(
                Expression.New(transparentIdentifierCtorInfo, outerApplyOrderingsResult.state.CurrentParameter, newGroupingParameter),
                outerApplyOrderingsResult.state.CurrentParameter,
                newGroupingParameter);

            var newMethodInfo = methodCallExpression.Method.GetGenericMethodDefinition().MakeGenericMethod(
                outerApplyOrderingsResult.state.CurrentParameter.Type,
                innerApplyOrderingsResult.state.CurrentParameter.Type,
                outerApplyNavigationsResult.lambdaBody.Type,
                lambda.Body.Type);

            var rewritten = Expression.Call(
                newMethodInfo,
                outerApplyOrderingsResult.source,
                innerApplyOrderingsResult.source,
                Expression.Lambda(newOuterKeySelectorBody, outerApplyOrderingsResult.state.CurrentParameter),
                Expression.Lambda(newInnerKeySelectorBody, innerApplyOrderingsResult.state.CurrentParameter),
                lambda);

            // temporarily change selector to ti => ti for purpose of finding & expanding navigations in the pending selector lambda itself
            var pendingSelector = newState.PendingSelector;
            newState.PendingSelector = Expression.Lambda(newState.PendingSelector.Parameters[0], newState.PendingSelector.Parameters[0]);
            var result = FindAndApplyNavigations(rewritten, pendingSelector, newState);
            result.state.PendingSelector = Expression.Lambda(result.lambdaBody, result.state.CurrentParameter);

            return new NavigationExpansionExpression(
                result.source,
                result.state,
                methodCallExpression.Type);
        }

        private Expression ProcessAll(MethodCallExpression methodCallExpression)
        {
            var source = VisitSourceExpression(methodCallExpression.Arguments[0]);
            source = RemoveIncludesFromSource(source);
            var predicate = methodCallExpression.Arguments[1].UnwrapQuote();
            AdjustCurrentParameterName(source.State, predicate.Parameters[0].Name);

            var applyNavigationsResult = FindAndApplyNavigations(source.Operand, predicate, source.State);
            var newPredicateBody = new NavigationPropertyUnbindingVisitor(applyNavigationsResult.state.CurrentParameter).Visit(applyNavigationsResult.lambdaBody);
            var applyOrderingsResult = ApplyPendingOrderings(applyNavigationsResult.source, applyNavigationsResult.state);

            var newMethodInfo = methodCallExpression.Method.GetGenericMethodDefinition().MakeGenericMethod(applyOrderingsResult.state.CurrentParameter.Type);
            var rewritten = Expression.Call(
                newMethodInfo,
                applyOrderingsResult.source,
                Expression.Lambda(
                    newPredicateBody,
                    applyOrderingsResult.state.CurrentParameter));

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
            navigationTreeNode.Included = NavigationTreeNodeIncludeMode.NotNeeded;
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
                var selector = methodCallExpression.Arguments[1].UnwrapQuote();
                AdjustCurrentParameterName(source.State, selector.Parameters[0].Name);
                var applyNavigationsResult = FindAndApplyNavigations(source.Operand, selector, source.State);
                var newSelectorBody = new NavigationPropertyUnbindingVisitor(applyNavigationsResult.state.CurrentParameter).Visit(applyNavigationsResult.lambdaBody);
                var newSelector = Expression.Lambda(newSelectorBody, applyNavigationsResult.state.CurrentParameter);

                var applyOrderingsResult = ApplyPendingOrderings(applyNavigationsResult.source, applyNavigationsResult.state);
                var newMethod = methodCallExpression.Method.GetGenericMethodDefinition();

                // Enumerable Min/Max overloads have only one type argument, Queryable have 2 but no overloads explosion
                if ((methodCallExpression.Method.Name == nameof(Enumerable.Min) || methodCallExpression.Method.Name == nameof(Enumerable.Max))
                    && newMethod.GetGenericArguments().Count() == 2)
                {
                    newMethod = newMethod.MakeGenericMethod(applyNavigationsResult.state.CurrentParameter.Type, methodCallExpression.Type);
                }
                else
                {
                    newMethod = newMethod.MakeGenericMethod(applyNavigationsResult.state.CurrentParameter.Type);
                }

                return Expression.Call(newMethod, applyOrderingsResult.source, newSelector);
            }

            return methodCallExpression.Update(methodCallExpression.Object, new[] { source });
        }

        private Expression ProcessDistinct(MethodCallExpression methodCallExpression)
        {
            var source = VisitSourceExpression(methodCallExpression.Arguments[0]);
            var preProcessResult = PreProcessTerminatingOperation(source);
            var rewritten = methodCallExpression.Update(methodCallExpression.Object, new[] { preProcessResult.source });

            return new NavigationExpansionExpression(rewritten, preProcessResult.state, methodCallExpression.Type);
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
                var rewritten = methodCallExpression.Update(methodCallExpression.Object, new[] { preProcessResult.source });

                return new NavigationExpansionExpression(rewritten, preProcessResult.state, methodCallExpression.Type);
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
            var newEntityType = _model.FindEntityType(methodCallExpression.Method.GetGenericArguments()[0]);

            // TODO: possible small optimization - only apply this if newEntityType is different than the old one
            if (newEntityType != null)
            {
                var newSourceMapping = new SourceMapping { RootEntityType = newEntityType };

                var newNavigationTreeRoot = NavigationTreeNode.CreateRoot(newSourceMapping, fromMapping: new List<string>(), optional: false);
                newSourceMapping.NavigationTree = newNavigationTreeRoot;
                preProcessResult.state.SourceMappings = new List<SourceMapping> { newSourceMapping };

                var newPendingSelectorParameter = Expression.Parameter(newEntityType.ClrType, preProcessResult.state.CurrentParameter.Name);

                // since we just ran preprocessing and the method is OfType, pending selector is guaranteed to be simple e => e
                var newPendingSelectorBody = new NavigationPropertyBindingVisitor(newPendingSelectorParameter, preProcessResult.state.SourceMappings).Visit(newPendingSelectorParameter);

                preProcessResult.state.CurrentParameter = newPendingSelectorParameter;
                preProcessResult.state.PendingSelector = Expression.Lambda(newPendingSelectorBody, newPendingSelectorParameter);
            }

            var rewritten = methodCallExpression.Update(methodCallExpression.Object, new[] { preProcessResult.source });

            return new NavigationExpansionExpression(rewritten, preProcessResult.state, methodCallExpression.Type);
        }

        private Expression ProcessSkipTake(MethodCallExpression methodCallExpression)
        {
            var source = VisitSourceExpression(methodCallExpression.Arguments[0]);
            var preProcessResult = PreProcessTerminatingOperation(source);
            var newArgument = Visit(methodCallExpression.Arguments[1]);
            var rewritten = methodCallExpression.Update(methodCallExpression.Object, new[] { preProcessResult.source, newArgument });

            return new NavigationExpansionExpression(rewritten, preProcessResult.state, methodCallExpression.Type);
        }

        private Expression ProcessBasicTerminatingOperation(MethodCallExpression methodCallExpression)
        {
            var source = VisitSourceExpression(methodCallExpression.Arguments[0]);
            var preProcessResult = PreProcessTerminatingOperation(source);
            var newArguments = methodCallExpression.Arguments.Skip(1).ToList();
            newArguments.Insert(0, preProcessResult.source);
            var rewritten = methodCallExpression.Update(methodCallExpression.Object, newArguments);

            return new NavigationExpansionExpression(rewritten, preProcessResult.state, methodCallExpression.Type);
        }

        private (Expression source, NavigationExpansionExpressionState state) PreProcessTerminatingOperation(NavigationExpansionExpression source)
        {
            var applyOrderingsResult = ApplyPendingOrderings(source.Operand, source.State);

            if (applyOrderingsResult.state.ApplyPendingSelector)
            {
                var unbinder = new NavigationPropertyUnbindingVisitor(applyOrderingsResult.state.CurrentParameter);
                var newSelectorBody = unbinder.Visit(applyOrderingsResult.state.PendingSelector.Body);

                var pssmg = new PendingSelectorSourceMappingGenerator(applyOrderingsResult.state.PendingSelector.Parameters[0], null);
                pssmg.Visit(applyOrderingsResult.state.PendingSelector.Body);

                var selectorMethodInfo = applyOrderingsResult.source.Type.IsQueryableType()
                    ? LinqMethodHelpers.QueryableSelectMethodInfo
                    : LinqMethodHelpers.EnumerableSelectMethodInfo;

                selectorMethodInfo = selectorMethodInfo.MakeGenericMethod(
                    applyOrderingsResult.state.CurrentParameter.Type,
                    newSelectorBody.Type);

                var result = Expression.Call(
                    selectorMethodInfo,
                    applyOrderingsResult.source,
                    Expression.Lambda(newSelectorBody, applyOrderingsResult.state.CurrentParameter));

                var newPendingSelectorParameter = Expression.Parameter(newSelectorBody.Type);
                var customRootMapping = new List<string>();

                Expression newPendingSelectorBody;
                if (applyOrderingsResult.state.PendingSelector.Body is NavigationBindingExpression binding)
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
                    if (pssmg.SourceMappings.Where(sm => sm.NavigationTree.Flatten().Where(n => n.Included == NavigationTreeNodeIncludeMode.ReferencePending || n.Included == NavigationTreeNodeIncludeMode.Collection).Any()).Any())
                    {
                        var selectorReprojector = new PendingSelectorReprojector(customRootExpression);
                        newPendingSelectorBody = selectorReprojector.Visit(applyOrderingsResult.state.PendingSelector.Body);

                        var binder = new NavigationPropertyBindingVisitor(newPendingSelectorParameter, pssmg.SourceMappings);
                        newPendingSelectorBody = binder.Visit(newPendingSelectorBody);
                    }
                    else
                    {
                        newPendingSelectorBody = customRootExpression;
                    }
                }

                var newState = new NavigationExpansionExpressionState(
                    newPendingSelectorParameter,
                    pssmg.SourceMappings,
                    Expression.Lambda(newPendingSelectorBody, newPendingSelectorParameter),
                    applyPendingSelector: false,
                    new List<(MethodInfo method, LambdaExpression keySelector)>(),
                    pendingIncludeChain: null,
                    pendingCardinalityReducingOperator: null,
                    pendingTags: new List<string>(),
                    new List<List<string>> { customRootMapping },
                    materializeCollectionNavigation: null);

                return (source: result, state: newState);
            }
            else
            {
                return (applyOrderingsResult.source, applyOrderingsResult.state);
            }
        }

        private class PendingSelectorReprojector : ExpressionVisitor
        {
            private List<string> _currentPath = new List<string>();
            private CustomRootExpression _rootExpression;

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
            var includeElements = includeString.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);

            var result = (Expression)new NavigationExpansionRootExpression(source, new List<string>());

            // TODO: this is not always correct IF we allow includes in random places (e.g. after joins)
            var rootEntityType = source.State.SourceMappings.Single().RootEntityType;
            var entityType = rootEntityType;

            var previousCollectionInclude = false;
            for (var i = 0; i < includeElements.Length; i++)
            {
                var parameter = Expression.Parameter(entityType.ClrType, entityType.ClrType.GenerateParameterName());

                // TODO: issue #15381 - deal with inheritance AND case where multiple children have navigation with the same name - we need to branch out in that scenario
                var navigation = entityType.FindNavigation(includeElements[i]);
                if (navigation == null)
                {
                    throw new InvalidOperationException("Invalid include path: '" + includeString + "' - couldn't find navigation for: '" + includeElements[i] + "'");
                }

                var lambda = Expression.Lambda(Expression.PropertyOrField(parameter, navigation.PropertyInfo?.Name ?? navigation.FieldInfo.Name), parameter);
                var includeMethodInfo = i == 0
                    ? EntityFrameworkQueryableExtensions.IncludeMethodInfo.MakeGenericMethod(rootEntityType.ClrType, navigation.ClrType)
                    : previousCollectionInclude
                        ? EntityFrameworkQueryableExtensions.ThenIncludeAfterEnumerableMethodInfo.MakeGenericMethod(rootEntityType.ClrType, entityType.ClrType, navigation.ClrType)
                        : EntityFrameworkQueryableExtensions.ThenIncludeAfterReferenceMethodInfo.MakeGenericMethod(rootEntityType.ClrType, entityType.ClrType, navigation.ClrType);

                result = Expression.Call(includeMethodInfo, result, lambda);
                previousCollectionInclude = navigation.IsCollection();
                entityType = navigation.GetTargetType();
            }

            return (MethodCallExpression)result;
        }

        private Expression ProcessInclude(MethodCallExpression methodCallExpression)
        {
            methodCallExpression = TryConvertToLambdaInclude(methodCallExpression);

            var source = VisitSourceExpression(methodCallExpression.Arguments[0]);

            var includeLambda = methodCallExpression.Arguments[1].UnwrapQuote();
            AdjustCurrentParameterName(source.State, includeLambda.Parameters[0].Name);

            var applyOrderingsResult = ApplyPendingOrderings(source.Operand, source.State);

            // just bind to mark all the necessary navigation for include in the future
            // include need to be delayed, in case they are not needed, e.g. when there is a projection on top that only projects scalars
            Expression remappedIncludeLambdaBody;
            if (methodCallExpression.Method.Name == "Include")
            {
                remappedIncludeLambdaBody = ExpressionExtensions.CombineAndRemap(includeLambda.Body, includeLambda.Parameters[0], applyOrderingsResult.state.PendingSelector.Body);
            }
            else
            {
                // we can't use NavigationBindingVisitor for cases like root.Include(r => r.Collection).ThenInclude(r => r.Navigation)
                // because the type mismatch (trying to compose Navigation access on the ICollection from the first include
                // we manually construct navigation binding that should be a root of the new include, its EntityType being the element of the previously included collection
                // pendingIncludeLambda is only used for marking the includes - as long as the NavigationTreeNodes are correct it should be fine
                if (applyOrderingsResult.state.PendingIncludeChain.NavigationTreeNode.Navigation.IsCollection())
                {
                    var newIncludeLambdaRoot = new NavigationBindingExpression(
                        applyOrderingsResult.state.CurrentParameter,
                        applyOrderingsResult.state.PendingIncludeChain.NavigationTreeNode,
                        applyOrderingsResult.state.PendingIncludeChain.EntityType,
                        applyOrderingsResult.state.PendingIncludeChain.SourceMapping,
                        includeLambda.Parameters[0].Type);

                    remappedIncludeLambdaBody = new ExpressionReplacingVisitor(includeLambda.Parameters[0], newIncludeLambdaRoot).Visit(includeLambda.Body);
                }
                else
                {
                    var pendingIncludeChainLambda = Expression.Lambda(applyOrderingsResult.state.PendingIncludeChain, applyOrderingsResult.state.CurrentParameter);
                    remappedIncludeLambdaBody = ExpressionExtensions.CombineAndRemap(includeLambda.Body, includeLambda.Parameters[0], pendingIncludeChainLambda.Body);
                }
            }

            var binder = new NavigationPropertyBindingVisitor(applyOrderingsResult.state.PendingSelector.Parameters[0], applyOrderingsResult.state.SourceMappings, bindInclude: true);
            var boundIncludeLambdaBody = binder.Visit(remappedIncludeLambdaBody);

            if (boundIncludeLambdaBody is NavigationBindingExpression navigationBindingExpression)
            {
                applyOrderingsResult.state.PendingIncludeChain = navigationBindingExpression;
            }
            else
            {
                throw new InvalidOperationException("Incorrect include argument: " + includeLambda);
            }

            return new NavigationExpansionExpression(applyOrderingsResult.source, applyOrderingsResult.state, methodCallExpression.Type);
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
            var applyOrderingsResult = ApplyPendingOrderings(source.Operand, source.State);
            applyOrderingsResult.state.PendingCardinalityReducingOperator = methodCallExpression.Method.GetGenericMethodDefinition();

            return new NavigationExpansionExpression(applyOrderingsResult.source, applyOrderingsResult.state, methodCallExpression.Type);
        }

        private Expression ProcessFromRawSql(MethodCallExpression methodCallExpression)
        {
            var source = VisitSourceExpression(methodCallExpression.Arguments[0]);

            return new NavigationExpansionExpression(methodCallExpression, source.State, methodCallExpression.Type);
        }

        private Expression ProcessWithTag(MethodCallExpression methodCallExpression)
        {
            var source = VisitSourceExpression(methodCallExpression.Arguments[0]);
            var tag = (string)((ConstantExpression)methodCallExpression.Arguments[1]).Value;
            source.State.PendingTags.Add(tag);

            return source;
        }

        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            if (constantExpression.Value != null
                && constantExpression.Value.GetType().IsGenericType
                && constantExpression.Value.GetType().GetGenericTypeDefinition() == typeof(EntityQueryable<>))
            {
                var elementType = constantExpression.Value.GetType().GetSequenceType();
                var entityType = _model.FindEntityType(elementType);

                return NavigationExpansionHelpers.CreateNavigationExpansionRoot(constantExpression, entityType, materializeCollectionNavigation: null);
            }

            return base.VisitConstant(constantExpression);
        }

        private (Expression source, NavigationExpansionExpressionState state) ApplyPendingOrderings(Expression source, NavigationExpansionExpressionState state)
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

        private (Expression source, Expression lambdaBody, NavigationExpansionExpressionState state) FindAndApplyNavigations(
            Expression source,
            LambdaExpression lambda,
            NavigationExpansionExpressionState state)
        {
            if (state.PendingSelector == null)
            {
                return (source, lambda.Body, state);
            }

            var remappedLambdaBody = ExpressionExtensions.CombineAndRemap(lambda.Body, lambda.Parameters[0], state.PendingSelector.Body);

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
                if (sourceMapping.NavigationTree.Flatten().Any(n => n.ExpansionMode == NavigationTreeNodeExpansionMode.ReferencePending))
                {
                    foreach (var navigationTree in sourceMapping.NavigationTree.Children.Where(n => !n.Navigation.IsCollection()))
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
                state.PendingTags,
                state.CustomRootMappings,
                state.MaterializeCollectionNavigation);

            return (result.source, lambdaBody: boundLambdaBody, state: newState);
        }

        private (LambdaExpression lambda, NavigationExpansionExpressionState state) RemapTwoArgumentResultSelector(
            LambdaExpression resultSelector,
            NavigationExpansionExpressionState outerState,
            NavigationExpansionExpressionState innerState)
        {
            var remappedResultSelectorBody = ExpressionExtensions.CombineAndRemap(resultSelector.Body, resultSelector.Parameters[0], outerState.PendingSelector.Body);
            remappedResultSelectorBody = ExpressionExtensions.CombineAndRemap(remappedResultSelectorBody, resultSelector.Parameters[1], innerState.PendingSelector.Body);

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
                foreach (var navigationTreeNode in outerSourceMapping.NavigationTree.Flatten().Where(n => n.ExpansionMode == NavigationTreeNodeExpansionMode.ReferenceComplete))
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
                foreach (var navigationTreeNode in innerSourceMapping.NavigationTree.Flatten().Where(n => n.ExpansionMode == NavigationTreeNodeExpansionMode.ReferenceComplete))
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
                outerState.PendingTags.Concat(innerState.PendingTags).ToList(),
                outerState.CustomRootMappings.Concat(innerState.CustomRootMappings).ToList(),
                materializeCollectionNavigation: null);

            var lambda = Expression.Lambda(
                Expression.New(transparentIdentifierCtorInfo, outerState.CurrentParameter, innerState.CurrentParameter),
                outerState.CurrentParameter,
                innerState.CurrentParameter);

            return (lambda, state: newState);
        }

        private class PendingSelectorSourceMappingGenerator : ExpressionVisitor
        {
            private ParameterExpression _rootParameter;
            private List<string> _currentPath = new List<string>();
            private IEntityType _entityTypeOverride;

            public List<SourceMapping> SourceMappings = new List<SourceMapping>();

            public Dictionary<NavigationBindingExpression, SourceMapping> BindingToSourceMapping
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
                => methodCallExpression.Method.IsEFPropertyMethod()
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
