// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.EntityFrameworkCore.Query
{
    using ResultHandler = Func<EntityQueryModelVisitor, ResultOperatorBase, QueryModel, Expression>;

    /// <summary>
    ///     The default client-eval result operator handler.
    /// </summary>
    public class ResultOperatorHandler : IResultOperatorHandler
    {
        private static readonly Dictionary<Type, ResultHandler> _handlers
            = new Dictionary<Type, ResultHandler>
            {
                { typeof(AllResultOperator), (v, r, q) => HandleAll(v, (AllResultOperator)r, q) },
                { typeof(AnyResultOperator), (v, _, __) => HandleAny(v) },
                { typeof(AverageResultOperator), (v, _, __) => HandleAverage(v) },
                { typeof(CastResultOperator), (v, r, __) => HandleCast(v, (CastResultOperator)r) },
                { typeof(ConcatResultOperator), (v, r, __) => HandleConcat(v, (ConcatResultOperator)r) },
                { typeof(CountResultOperator), (v, _, __) => HandleCount(v) },
                { typeof(ContainsResultOperator), (v, r, q) => HandleContains(v, (ContainsResultOperator)r, q) },
                { typeof(DefaultIfEmptyResultOperator), (v, r, q) => HandleDefaultIfEmpty(v, (DefaultIfEmptyResultOperator)r, q) },
                { typeof(DistinctResultOperator), (v, _, __) => HandleDistinct(v) },
                { typeof(ExceptResultOperator), (v, r, __) => HandleExcept(v, (ExceptResultOperator)r) },
                { typeof(FirstResultOperator), (v, r, __) => HandleFirst(v, (ChoiceResultOperatorBase)r) },
                { typeof(GroupResultOperator), (v, r, q) => HandleGroup(v, (GroupResultOperator)r, q) },
                { typeof(IntersectResultOperator), (v, r, __) => HandleIntersect(v, (IntersectResultOperator)r) },
                { typeof(LastResultOperator), (v, r, __) => HandleLast(v, (ChoiceResultOperatorBase)r) },
                { typeof(LongCountResultOperator), (v, _, __) => HandleLongCount(v) },
                { typeof(MinResultOperator), (v, _, __) => HandleMin(v) },
                { typeof(MaxResultOperator), (v, _, __) => HandleMax(v) },
                { typeof(OfTypeResultOperator), (v, r, q) => HandleOfType(v, (OfTypeResultOperator)r) },
                { typeof(SingleResultOperator), (v, r, __) => HandleSingle(v, (ChoiceResultOperatorBase)r) },
                { typeof(SkipResultOperator), (v, r, __) => HandleSkip(v, (SkipResultOperator)r) },
                { typeof(SumResultOperator), (v, _, __) => HandleSum(v) },
                { typeof(TakeResultOperator), (v, r, __) => HandleTake(v, (TakeResultOperator)r) },
                { typeof(UnionResultOperator), (v, r, __) => HandleUnion(v, (UnionResultOperator)r) }
            };

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResultOperatorHandler" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public ResultOperatorHandler([NotNull] ResultOperatorHandlerDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));
        }

        /// <summary>
        ///     Handles the result operator.
        /// </summary>
        /// <param name="entityQueryModelVisitor"> The entity query model visitor. </param>
        /// <param name="resultOperator"> The result operator. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <returns>
        ///     An compiled query expression fragment representing the result operator.
        /// </returns>
        public virtual Expression HandleResultOperator(
            EntityQueryModelVisitor entityQueryModelVisitor,
            ResultOperatorBase resultOperator,
            QueryModel queryModel)
        {
            Check.NotNull(entityQueryModelVisitor, nameof(entityQueryModelVisitor));
            Check.NotNull(resultOperator, nameof(resultOperator));
            Check.NotNull(queryModel, nameof(queryModel));

            if (!_handlers.TryGetValue(resultOperator.GetType(), out var handler))
            {
                throw new NotImplementedException(resultOperator.GetType().ToString());
            }

            return handler(entityQueryModelVisitor, resultOperator, queryModel);
        }

        private static Expression HandleAll(
            EntityQueryModelVisitor entityQueryModelVisitor,
            AllResultOperator allResultOperator,
            QueryModel queryModel)
        {
            var sequenceType
                = entityQueryModelVisitor.Expression.Type.GetSequenceType();

            var predicate
                = entityQueryModelVisitor
                    .ReplaceClauseReferences(
                        allResultOperator.Predicate,
                        queryModel.MainFromClause);

            return CallWithPossibleCancellationToken(
                entityQueryModelVisitor.LinqOperatorProvider.All
                    .MakeGenericMethod(sequenceType),
                entityQueryModelVisitor.Expression,
                Expression.Lambda(predicate, entityQueryModelVisitor.CurrentParameter));
        }

        private static Expression HandleAny(EntityQueryModelVisitor entityQueryModelVisitor)
            => CallWithPossibleCancellationToken(
                entityQueryModelVisitor.LinqOperatorProvider.Any
                    .MakeGenericMethod(entityQueryModelVisitor.Expression.Type.GetSequenceType()),
                entityQueryModelVisitor.Expression);

        private static Expression HandleAverage(EntityQueryModelVisitor entityQueryModelVisitor)
            => HandleAggregate(entityQueryModelVisitor, "Average");

        private static Expression HandleCast(
            EntityQueryModelVisitor entityQueryModelVisitor, CastResultOperator castResultOperator)
        {
            var resultItemTypeInfo
                = entityQueryModelVisitor.Expression.Type
                    .GetSequenceType().GetTypeInfo();

            return castResultOperator.CastItemType.GetTypeInfo()
                .IsAssignableFrom(resultItemTypeInfo)
                ? entityQueryModelVisitor.Expression
                : Expression.Call(
                entityQueryModelVisitor.LinqOperatorProvider
                    .Cast.MakeGenericMethod(castResultOperator.CastItemType),
                entityQueryModelVisitor.Expression);
        }

        private static Expression HandleConcat(
            EntityQueryModelVisitor entityQueryModelVisitor,
            ConcatResultOperator concatResultOperator)
            => HandleSetOperation(
                entityQueryModelVisitor,
                concatResultOperator.Source2,
                entityQueryModelVisitor.LinqOperatorProvider.Concat);

        private static Expression HandleCount(EntityQueryModelVisitor entityQueryModelVisitor)
            => CallWithPossibleCancellationToken(
                entityQueryModelVisitor.LinqOperatorProvider
                    .Count.MakeGenericMethod(entityQueryModelVisitor.Expression.Type.GetSequenceType()),
                entityQueryModelVisitor.Expression);

        private static Expression HandleContains(
            EntityQueryModelVisitor entityQueryModelVisitor,
            ContainsResultOperator containsResultOperator,
            QueryModel queryModel)
        {
            var item
                = entityQueryModelVisitor
                    .ReplaceClauseReferences(
                        containsResultOperator.Item,
                        queryModel.MainFromClause);

            return CallWithPossibleCancellationToken(
                entityQueryModelVisitor.LinqOperatorProvider.Contains
                    .MakeGenericMethod(entityQueryModelVisitor.Expression.Type.GetSequenceType()),
                entityQueryModelVisitor.Expression,
                item);
        }

        private static Expression HandleDefaultIfEmpty(
            EntityQueryModelVisitor entityQueryModelVisitor,
            DefaultIfEmptyResultOperator defaultIfEmptyResultOperator,
            QueryModel queryModel)
        {
            if (defaultIfEmptyResultOperator.OptionalDefaultValue == null)
            {
                return Expression.Call(
                    entityQueryModelVisitor.LinqOperatorProvider.DefaultIfEmpty
                        .MakeGenericMethod(entityQueryModelVisitor.Expression.Type.GetSequenceType()),
                    entityQueryModelVisitor.Expression);
            }

            var optionalDefaultValue
                = entityQueryModelVisitor
                    .ReplaceClauseReferences(
                        defaultIfEmptyResultOperator.OptionalDefaultValue,
                        queryModel.MainFromClause);

            return Expression.Call(
                entityQueryModelVisitor.LinqOperatorProvider.DefaultIfEmptyArg
                    .MakeGenericMethod(entityQueryModelVisitor.Expression.Type.GetSequenceType()),
                entityQueryModelVisitor.Expression,
                optionalDefaultValue);
        }

        private static Expression HandleDistinct(EntityQueryModelVisitor entityQueryModelVisitor)
            => Expression.Call(
                entityQueryModelVisitor.LinqOperatorProvider.Distinct
                    .MakeGenericMethod(entityQueryModelVisitor.Expression.Type.GetSequenceType()),
                entityQueryModelVisitor.Expression);

        private static Expression HandleExcept(
            EntityQueryModelVisitor entityQueryModelVisitor,
            ExceptResultOperator exceptResultOperator)
            => HandleSetOperation(
                entityQueryModelVisitor,
                exceptResultOperator.Source2,
                entityQueryModelVisitor.LinqOperatorProvider.Except);

        private static Expression HandleFirst(
            EntityQueryModelVisitor entityQueryModelVisitor, ChoiceResultOperatorBase choiceResultOperator)
            => CallWithPossibleCancellationToken(
                (choiceResultOperator.ReturnDefaultWhenEmpty
                    ? entityQueryModelVisitor.LinqOperatorProvider.FirstOrDefault
                    : entityQueryModelVisitor.LinqOperatorProvider.First)
                .MakeGenericMethod(entityQueryModelVisitor.Expression.Type.GetSequenceType()),
                entityQueryModelVisitor.Expression);

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

            var taskLiftingExpressionVisitor = new TaskLiftingExpressionVisitor();
            var asyncElementSelector = taskLiftingExpressionVisitor.LiftTasks(elementSelector);

            var expression
                = asyncElementSelector == elementSelector
                    ? Expression.Call(
                        entityQueryModelVisitor.LinqOperatorProvider.GroupBy
                            .MakeGenericMethod(
                                entityQueryModelVisitor.Expression.Type.GetSequenceType(),
                                keySelector.Type,
                                elementSelector.Type),
                        entityQueryModelVisitor.Expression,
                        Expression.Lambda(keySelector, entityQueryModelVisitor.CurrentParameter),
                        Expression.Lambda(elementSelector, entityQueryModelVisitor.CurrentParameter))
                    : Expression.Call(
                        _groupByAsync
                            .MakeGenericMethod(
                                entityQueryModelVisitor.Expression.Type.GetSequenceType(),
                                keySelector.Type,
                                elementSelector.Type),
                        entityQueryModelVisitor.Expression,
                        Expression.Lambda(keySelector, entityQueryModelVisitor.CurrentParameter),
                        Expression.Lambda(
                            asyncElementSelector,
                            entityQueryModelVisitor.CurrentParameter,
                            taskLiftingExpressionVisitor.CancellationTokenParameter));

            entityQueryModelVisitor.CurrentParameter
                = Expression.Parameter(expression.Type.GetSequenceType(), groupResultOperator.ItemName);

            entityQueryModelVisitor.QueryCompilationContext.AddOrUpdateMapping(groupResultOperator, entityQueryModelVisitor.CurrentParameter);

            return expression;
        }

        private static readonly MethodInfo _groupByAsync
            = typeof(ResultOperatorHandler)
                .GetTypeInfo()
                .GetDeclaredMethod(nameof(_GroupByAsync));

        // ReSharper disable once InconsistentNaming
        private static IAsyncEnumerable<IGrouping<TKey, TElement>> _GroupByAsync<TSource, TKey, TElement>(
            IAsyncEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, CancellationToken, Task<TElement>> elementSelector)
            => new AsyncGroupByAsyncEnumerable<TSource, TKey, TElement>(source, keySelector, elementSelector);

        private sealed class AsyncGroupByAsyncEnumerable<TSource, TKey, TElement>
            : IAsyncEnumerable<IGrouping<TKey, TElement>>
        {
            private readonly IAsyncEnumerable<TSource> _source;
            private readonly Func<TSource, TKey> _keySelector;
            private readonly Func<TSource, CancellationToken, Task<TElement>> _elementSelector;

            public AsyncGroupByAsyncEnumerable(
                IAsyncEnumerable<TSource> source,
                Func<TSource, TKey> keySelector,
                Func<TSource, CancellationToken, Task<TElement>> elementSelector)
            {
                _source = source;
                _keySelector = keySelector;
                _elementSelector = elementSelector;
            }

            public IAsyncEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
                => new GroupByAsyncEnumerator(this);

            private sealed class GroupByAsyncEnumerator : IAsyncEnumerator<IGrouping<TKey, TElement>>
            {
                private readonly AsyncGroupByAsyncEnumerable<TSource, TKey, TElement> _groupByAsyncEnumerable;
                private readonly IEqualityComparer<TKey> _comparer;

                private IEnumerator<IGrouping<TKey, TSource>> _lookupEnumerator;
                private bool _hasNext;

                public GroupByAsyncEnumerator(
                    AsyncGroupByAsyncEnumerable<TSource, TKey, TElement> groupByAsyncEnumerable)
                {
                    _groupByAsyncEnumerable = groupByAsyncEnumerable;
                    _comparer = EqualityComparer<TKey>.Default;
                }

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (_lookupEnumerator == null)
                    {
                        _lookupEnumerator
                            = (await _groupByAsyncEnumerable._source
                                .ToLookup(
                                    _groupByAsyncEnumerable._keySelector,
                                    e => e,
                                    _comparer,
                                    cancellationToken)).GetEnumerator();

                        _hasNext = _lookupEnumerator.MoveNext();
                    }

                    if (_hasNext)
                    {
                        var grouping = new Grouping<TKey, TElement>(_lookupEnumerator.Current.Key);

                        foreach (var item in _lookupEnumerator.Current)
                        {
                            grouping.Add(await _groupByAsyncEnumerable._elementSelector(item, cancellationToken));
                        }

                        Current = grouping;

                        _hasNext = _lookupEnumerator.MoveNext();

                        return true;
                    }

                    return false;
                }

                public IGrouping<TKey, TElement> Current { get; private set; }

                public void Dispose() => _lookupEnumerator?.Dispose();
            }
        }

        private static Expression HandleIntersect(
            EntityQueryModelVisitor entityQueryModelVisitor,
            IntersectResultOperator intersectResultOperator)
            => HandleSetOperation(
                entityQueryModelVisitor,
                intersectResultOperator.Source2,
                entityQueryModelVisitor.LinqOperatorProvider.Intersect);

        private static Expression HandleLast(
            EntityQueryModelVisitor entityQueryModelVisitor,
            ChoiceResultOperatorBase choiceResultOperator)
        {
            if (entityQueryModelVisitor.Expression is MethodCallExpression methodCallExpression
                && (methodCallExpression.Method
                    .MethodIsClosedFormOf(entityQueryModelVisitor.LinqOperatorProvider.Select)
                    || methodCallExpression.Method
                    .MethodIsClosedFormOf(AsyncLinqOperatorProvider.SelectAsyncMethod)))
            {
                // Push Last down below Select

                return
                    methodCallExpression.Update(
                        methodCallExpression.Object,
                        new[]
                        {
                            Expression.Call(
                                entityQueryModelVisitor.LinqOperatorProvider.ToSequence
                                    .MakeGenericMethod(methodCallExpression.Arguments[0].Type.GetSequenceType()),
                                Expression.Lambda(
                                    CallWithPossibleCancellationToken(
                                        (choiceResultOperator.ReturnDefaultWhenEmpty
                                            ? entityQueryModelVisitor.LinqOperatorProvider.LastOrDefault
                                            : entityQueryModelVisitor.LinqOperatorProvider.Last)
                                        .MakeGenericMethod(methodCallExpression.Arguments[0].Type.GetSequenceType()),
                                        methodCallExpression.Arguments[0]))),
                            methodCallExpression.Arguments[1]
                        });
            }

            return CallWithPossibleCancellationToken(
                (choiceResultOperator.ReturnDefaultWhenEmpty
                    ? entityQueryModelVisitor.LinqOperatorProvider.LastOrDefault
                    : entityQueryModelVisitor.LinqOperatorProvider.Last)
                .MakeGenericMethod(entityQueryModelVisitor.Expression.Type.GetSequenceType()),
                entityQueryModelVisitor.Expression);
        }

        private static Expression HandleLongCount(EntityQueryModelVisitor entityQueryModelVisitor)
            => CallWithPossibleCancellationToken(
                entityQueryModelVisitor.LinqOperatorProvider.LongCount
                    .MakeGenericMethod(entityQueryModelVisitor.Expression.Type.GetSequenceType()),
                entityQueryModelVisitor.Expression);

        private static Expression HandleMin(EntityQueryModelVisitor entityQueryModelVisitor)
            => HandleAggregate(entityQueryModelVisitor, "Min");

        private static Expression HandleMax(EntityQueryModelVisitor entityQueryModelVisitor)
            => HandleAggregate(entityQueryModelVisitor, "Max");

        private static Expression HandleOfType(
            EntityQueryModelVisitor entityQueryModelVisitor,
            OfTypeResultOperator ofTypeResultOperator)
            => Expression.Call(
                entityQueryModelVisitor.LinqOperatorProvider.OfType
                    .MakeGenericMethod(ofTypeResultOperator.SearchedItemType),
                entityQueryModelVisitor.Expression);

        private static Expression HandleSingle(
            EntityQueryModelVisitor entityQueryModelVisitor, ChoiceResultOperatorBase choiceResultOperator)
            => CallWithPossibleCancellationToken(
                (choiceResultOperator.ReturnDefaultWhenEmpty
                    ? entityQueryModelVisitor.LinqOperatorProvider.SingleOrDefault
                    : entityQueryModelVisitor.LinqOperatorProvider.Single)
                .MakeGenericMethod(entityQueryModelVisitor.Expression.Type.GetSequenceType()),
                entityQueryModelVisitor.Expression);

        private static Expression HandleSkip(
            EntityQueryModelVisitor entityQueryModelVisitor,
            SkipResultOperator skipResultOperator)
        {
            var countExpression
                = new DefaultQueryExpressionVisitor(entityQueryModelVisitor)
                    .Visit(skipResultOperator.Count);

            if (entityQueryModelVisitor.Expression is MethodCallExpression methodCallExpression
                && (methodCallExpression.Method
                    .MethodIsClosedFormOf(entityQueryModelVisitor.LinqOperatorProvider.Select)
                    || methodCallExpression.Method
                    .MethodIsClosedFormOf(AsyncLinqOperatorProvider.SelectAsyncMethod)))
            {
                // Push Skip down below Select

                return
                    methodCallExpression.Update(
                        methodCallExpression.Object,
                        new[]
                        {
                            Expression.Call(
                                entityQueryModelVisitor.LinqOperatorProvider.Skip
                                    .MakeGenericMethod(methodCallExpression.Arguments[0].Type.GetSequenceType()),
                                methodCallExpression.Arguments[0],
                                countExpression),
                            methodCallExpression.Arguments[1]
                        });
            }

            return Expression.Call(
                entityQueryModelVisitor.LinqOperatorProvider.Skip
                    .MakeGenericMethod(entityQueryModelVisitor.Expression.Type.GetSequenceType()),
                entityQueryModelVisitor.Expression,
                countExpression);
        }

        private static Expression HandleSum(EntityQueryModelVisitor entityQueryModelVisitor)
            => HandleAggregate(entityQueryModelVisitor, "Sum");

        private static Expression HandleTake(
            EntityQueryModelVisitor entityQueryModelVisitor, TakeResultOperator takeResultOperator)
        {
            var countExpression
                = new DefaultQueryExpressionVisitor(entityQueryModelVisitor)
                    .Visit(takeResultOperator.Count);

            if (entityQueryModelVisitor.Expression is MethodCallExpression methodCallExpression
                && (methodCallExpression.Method
                    .MethodIsClosedFormOf(entityQueryModelVisitor.LinqOperatorProvider.Select)
                    || methodCallExpression.Method
                    .MethodIsClosedFormOf(AsyncLinqOperatorProvider.SelectAsyncMethod)))
            {
                // Push Take down below Select

                return
                    methodCallExpression.Update(
                        methodCallExpression.Object,
                        new[]
                        {
                            Expression.Call(
                                entityQueryModelVisitor.LinqOperatorProvider.Take
                                    .MakeGenericMethod(methodCallExpression.Arguments[0].Type.GetSequenceType()),
                                methodCallExpression.Arguments[0],
                                countExpression),
                            methodCallExpression.Arguments[1]
                        });
            }

            return Expression.Call(
                entityQueryModelVisitor.LinqOperatorProvider.Take
                    .MakeGenericMethod(entityQueryModelVisitor.Expression.Type.GetSequenceType()),
                entityQueryModelVisitor.Expression,
                countExpression);
        }

        private static Expression HandleUnion(
            EntityQueryModelVisitor entityQueryModelVisitor,
            UnionResultOperator unionResultOperator)
            => HandleSetOperation(
                entityQueryModelVisitor,
                unionResultOperator.Source2,
                entityQueryModelVisitor.LinqOperatorProvider.Union);

        private static Expression HandleSetOperation(
            EntityQueryModelVisitor entityQueryModelVisitor,
            Expression secondSource,
            MethodInfo setMethodInfo)
        {
            var source2 = entityQueryModelVisitor.ReplaceClauseReferences(secondSource);

            var resultType = entityQueryModelVisitor.Expression.Type.GetSequenceType();
            var sourceType = source2.Type.GetSequenceType();
            while (!resultType.GetTypeInfo().IsAssignableFrom(sourceType.GetTypeInfo()))
            {
                resultType = resultType.GetTypeInfo().BaseType;
            }

            return Expression.Call(
                setMethodInfo.MakeGenericMethod(resultType),
                entityQueryModelVisitor.Expression,
                source2);
        }

        private static Expression HandleAggregate(EntityQueryModelVisitor entityQueryModelVisitor, string methodName)
            => CallWithPossibleCancellationToken(
                entityQueryModelVisitor.LinqOperatorProvider.GetAggregateMethod(
                    methodName,
                    entityQueryModelVisitor.Expression.Type.GetSequenceType()),
                entityQueryModelVisitor.Expression);

        private static readonly PropertyInfo _cancellationTokenProperty
            = typeof(QueryContext).GetTypeInfo()
                .GetDeclaredProperty("CancellationToken");

        /// <summary>
        ///     Call a client operator that may have a cancellation token.
        /// </summary>
        /// <param name="methodInfo"> The method to call. </param>
        /// <param name="arguments"> A variable-length parameters list containing arguments. </param>
        /// <returns>
        ///     A method call expression.
        /// </returns>
        public static Expression CallWithPossibleCancellationToken(
            [NotNull] MethodInfo methodInfo, [CanBeNull] params Expression[] arguments)
        {
            Check.NotNull(methodInfo, nameof(methodInfo));

            return methodInfo.GetParameters().Last().ParameterType == typeof(CancellationToken)
                ? Expression.Call(
                    methodInfo,
                    arguments
                        .AsEnumerable()
                        .Concat(
                            new[]
                            {
                                Expression.Property(
                                    EntityQueryModelVisitor.QueryContextParameter,
                                    _cancellationTokenProperty)
                            }))
                : Expression.Call(methodInfo, arguments);
        }
    }
}
