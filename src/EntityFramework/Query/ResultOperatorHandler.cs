// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.Data.Entity.Query
{
    using ResultHandler = Func<EntityQueryModelVisitor, ResultOperatorBase, QueryModel, Expression>;

    public class ResultOperatorHandler : IResultOperatorHandler
    {
        private static readonly Dictionary<Type, ResultHandler> _handlers
            = new Dictionary<Type, ResultHandler>
                {
                    { typeof(AllResultOperator), (v, r, q) => HandleAll(v, (AllResultOperator)r, q) },
                    { typeof(AnyResultOperator), (v, _, __) => HandleAny(v) },
                    { typeof(AverageResultOperator), (v, _, __) => HandleAverage(v) },
                    { typeof(CountResultOperator), (v, _, __) => HandleCount(v) },
                    { typeof(DefaultIfEmptyResultOperator), (v, r, q) => HandleDefaultIfEmpty(v, (DefaultIfEmptyResultOperator)r, q) },
                    { typeof(DistinctResultOperator), (v, _, __) => HandleDistinct(v) },
                    { typeof(FirstResultOperator), (v, r, __) => HandleFirst(v, (ChoiceResultOperatorBase)r) },
                    { typeof(GroupResultOperator), (v, r, q) => HandleGroup(v, (GroupResultOperator)r, q) },
                    { typeof(LastResultOperator), (v, r, __) => HandleLast(v, (ChoiceResultOperatorBase)r) },
                    { typeof(LongCountResultOperator), (v, _, __) => HandleLongCount(v) },
                    { typeof(MinResultOperator), (v, _, __) => HandleMin(v) },
                    { typeof(MaxResultOperator), (v, _, __) => HandleMax(v) },
                    { typeof(SingleResultOperator), (v, r, __) => HandleSingle(v, (ChoiceResultOperatorBase)r) },
                    { typeof(SkipResultOperator), (v, r, __) => HandleSkip(v, (SkipResultOperator)r) },
                    { typeof(SumResultOperator), (v, _, __) => HandleSum(v) },
                    { typeof(TakeResultOperator), (v, r, __) => HandleTake(v, (TakeResultOperator)r) }
                };

        public virtual Expression HandleResultOperator(
            EntityQueryModelVisitor entityQueryModelVisitor,
            ResultOperatorBase resultOperator,
            QueryModel queryModel)
        {
            Check.NotNull(entityQueryModelVisitor, "entityQueryModelVisitor");
            Check.NotNull(resultOperator, "resultOperator");
            Check.NotNull(queryModel, "queryModel");

            ResultHandler handler;
            if (!_handlers.TryGetValue(resultOperator.GetType(), out handler))
            {
                throw new NotImplementedException();
            }

            return handler(entityQueryModelVisitor, resultOperator, queryModel);
        }

        private static readonly MethodInfo _all = GetMethod("All", 1);

        private static Expression HandleAll(
            EntityQueryModelVisitor entityQueryModelVisitor,
            AllResultOperator allResultOperator,
            QueryModel queryModel)
        {
            var predicate
                = entityQueryModelVisitor
                    .ReplaceClauseReferences(
                        allResultOperator.Predicate,
                        queryModel.MainFromClause);

            return Expression.Call(
                _all.MakeGenericMethod(typeof(QuerySourceScope)),
                entityQueryModelVisitor.CreateScope(
                    entityQueryModelVisitor.Expression,
                    entityQueryModelVisitor.StreamedSequenceInfo.ResultItemType,
                    queryModel.MainFromClause),
                Expression.Lambda(predicate, EntityQueryModelVisitor.QuerySourceScopeParameter));
        }

        private static readonly MethodInfo _any = GetMethod("Any");

        private static Expression HandleAny(EntityQueryModelVisitor entityQueryModelVisitor)
        {
            return Expression.Call(
                _any.MakeGenericMethod(entityQueryModelVisitor.StreamedSequenceInfo.ResultItemType),
                entityQueryModelVisitor.Expression);
        }

        private static Expression HandleAverage(EntityQueryModelVisitor entityQueryModelVisitor)
        {
            return HandleAggregate(entityQueryModelVisitor, "Average");
        }

        private static readonly MethodInfo _count = GetMethod("Count");

        private static Expression HandleCount(EntityQueryModelVisitor entityQueryModelVisitor)
        {
            return Expression.Call(
                _count.MakeGenericMethod(entityQueryModelVisitor.StreamedSequenceInfo.ResultItemType),
                entityQueryModelVisitor.Expression);
        }

        private static readonly MethodInfo _defaultIfEmpty = GetMethod("DefaultIfEmpty");
        private static readonly MethodInfo _defaultIfEmptyArg = GetMethod("DefaultIfEmpty", 1);

        private static Expression HandleDefaultIfEmpty(
            EntityQueryModelVisitor entityQueryModelVisitor,
            DefaultIfEmptyResultOperator defaultIfEmptyResultOperator,
            QueryModel queryModel)
        {
            if (defaultIfEmptyResultOperator.OptionalDefaultValue == null)
            {
                return Expression.Call(
                    _defaultIfEmpty
                        .MakeGenericMethod(entityQueryModelVisitor.StreamedSequenceInfo.ResultItemType),
                    entityQueryModelVisitor.Expression);
            }

            var optionalDefaultValue
                = entityQueryModelVisitor
                    .ReplaceClauseReferences(
                        defaultIfEmptyResultOperator.OptionalDefaultValue,
                        queryModel.MainFromClause);

            return Expression.Call(
                _defaultIfEmptyArg.MakeGenericMethod(typeof(QuerySourceScope)),
                entityQueryModelVisitor.CreateScope(
                    entityQueryModelVisitor.Expression,
                    entityQueryModelVisitor.StreamedSequenceInfo.ResultItemType,
                    queryModel.MainFromClause),
                optionalDefaultValue);
        }

        private static readonly MethodInfo _distinct = GetMethod("Distinct");

        private static Expression HandleDistinct(EntityQueryModelVisitor entityQueryModelVisitor)
        {
            return Expression.Call(
                _distinct.MakeGenericMethod(entityQueryModelVisitor.StreamedSequenceInfo.ResultItemType),
                entityQueryModelVisitor.Expression);
        }

        private static readonly MethodInfo _first = GetMethod("First");
        private static readonly MethodInfo _firstOrDefault = GetMethod("FirstOrDefault");

        private static Expression HandleFirst(
            EntityQueryModelVisitor entityQueryModelVisitor, ChoiceResultOperatorBase choiceResultOperator)
        {
            return Expression.Call(
                (choiceResultOperator.ReturnDefaultWhenEmpty
                    ? _firstOrDefault
                    : _first)
                    .MakeGenericMethod(entityQueryModelVisitor.StreamedSequenceInfo.ResultItemType),
                entityQueryModelVisitor.Expression);
        }

        private static Expression HandleGroup(
            EntityQueryModelVisitor entityQueryModelVisitor,
            GroupResultOperator groupResultOperator,
            QueryModel queryModel)
        {
            var keySelector
                = entityQueryModelVisitor
                    .ReplaceClauseReferences(
                        groupResultOperator.KeySelector,
                        queryModel.MainFromClause);

            var elementSelector
                = entityQueryModelVisitor
                    .ReplaceClauseReferences(
                        groupResultOperator.ElementSelector,
                        queryModel.MainFromClause);

            return Expression.Call(
                _groupBy.MakeGenericMethod(
                    typeof(QuerySourceScope),
                    groupResultOperator.KeySelector.Type,
                    groupResultOperator.ElementSelector.Type),
                entityQueryModelVisitor.CreateScope(
                    entityQueryModelVisitor.Expression,
                    entityQueryModelVisitor.StreamedSequenceInfo.ResultItemType,
                    queryModel.MainFromClause),
                Expression.Lambda(keySelector, EntityQueryModelVisitor.QuerySourceScopeParameter),
                Expression.Lambda(elementSelector, EntityQueryModelVisitor.QuerySourceScopeParameter));
        }

        private static readonly MethodInfo _groupBy
            = typeof(ResultOperatorHandler).GetTypeInfo().GetDeclaredMethod("_GroupBy");

        [UsedImplicitly]
        private static IEnumerable<IGrouping<TKey, TElement>> _GroupBy<TSource, TKey, TElement>(
            IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return source.GroupBy(keySelector, elementSelector);
        }

        private static readonly MethodInfo _last = GetMethod("Last");
        private static readonly MethodInfo _lastOrDefault = GetMethod("LastOrDefault");

        private static Expression HandleLast(
            EntityQueryModelVisitor entityQueryModelVisitor, ChoiceResultOperatorBase choiceResultOperator)
        {
            return Expression.Call(
                (choiceResultOperator.ReturnDefaultWhenEmpty
                    ? _lastOrDefault
                    : _last).MakeGenericMethod(entityQueryModelVisitor.StreamedSequenceInfo.ResultItemType),
                entityQueryModelVisitor.Expression);
        }

        private static readonly MethodInfo _longCount = GetMethod("LongCount");

        private static Expression HandleLongCount(EntityQueryModelVisitor entityQueryModelVisitor)
        {
            return Expression.Call(
                _longCount.MakeGenericMethod(entityQueryModelVisitor.StreamedSequenceInfo.ResultItemType),
                entityQueryModelVisitor.Expression);
        }

        private static Expression HandleMin(EntityQueryModelVisitor entityQueryModelVisitor)
        {
            return HandleAggregate(entityQueryModelVisitor, "Min");
        }

        private static Expression HandleMax(EntityQueryModelVisitor entityQueryModelVisitor)
        {
            return HandleAggregate(entityQueryModelVisitor, "Max");
        }

        private static readonly MethodInfo _single = GetMethod("Single");
        private static readonly MethodInfo _singleOrDefault = GetMethod("SingleOrDefault");

        private static Expression HandleSingle(
            EntityQueryModelVisitor entityQueryModelVisitor, ChoiceResultOperatorBase choiceResultOperator)
        {
            return Expression.Call(
                (choiceResultOperator.ReturnDefaultWhenEmpty
                    ? _singleOrDefault
                    : _single)
                    .MakeGenericMethod(entityQueryModelVisitor.StreamedSequenceInfo.ResultItemType),
                entityQueryModelVisitor.Expression);
        }

        private static readonly MethodInfo _skip = GetMethod("Skip", 1);

        private static Expression HandleSkip(
            EntityQueryModelVisitor entityQueryModelVisitor, SkipResultOperator skipResultOperator)
        {
            return Expression.Call(
                _skip.MakeGenericMethod(entityQueryModelVisitor.StreamedSequenceInfo.ResultItemType),
                entityQueryModelVisitor.Expression, skipResultOperator.Count);
        }

        private static Expression HandleSum(EntityQueryModelVisitor entityQueryModelVisitor)
        {
            return HandleAggregate(entityQueryModelVisitor, "Sum");
        }

        private static readonly MethodInfo _take = GetMethod("Take", 1);

        private static Expression HandleTake(
            EntityQueryModelVisitor entityQueryModelVisitor, TakeResultOperator takeResultOperator)
        {
            return Expression.Call(
                _take.MakeGenericMethod(entityQueryModelVisitor.StreamedSequenceInfo.ResultItemType),
                entityQueryModelVisitor.Expression, takeResultOperator.Count);
        }

        private static Expression HandleAggregate(EntityQueryModelVisitor entityQueryModelVisitor, string methodName)
        {
            var itemType = entityQueryModelVisitor.StreamedSequenceInfo.ResultItemType;
            var minMethods = GetMethods(methodName).ToList();

            var minMethod
                = minMethods
                    .FirstOrDefault(mi => mi.GetParameters()[0].ParameterType
                                          == typeof(IEnumerable<>).MakeGenericType(itemType))
                  ?? minMethods.Single(mi => mi.IsGenericMethod)
                      .MakeGenericMethod(itemType);

            return Expression.Call(minMethod, entityQueryModelVisitor.Expression);
        }

        private static MethodInfo GetMethod(string name, int parameterCount = 0)
        {
            return GetMethods(name, parameterCount).Single();
        }

        private static IEnumerable<MethodInfo> GetMethods(string name, int parameterCount = 0)
        {
            return typeof(Enumerable).GetTypeInfo().GetDeclaredMethods(name)
                .Where(mi => mi.GetParameters().Length == parameterCount + 1);
        }
    }
}
