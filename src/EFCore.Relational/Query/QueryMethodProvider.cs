// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     Provides reflection objects for late-binding to synchronous relational query operations.
    /// </summary>
    public class QueryMethodProvider : IQueryMethodProvider
    {
        /// <summary>
        ///     Gets the shaped query method.
        /// </summary>
        /// <value>
        ///     The shaped query method.
        /// </value>
        public virtual MethodInfo ShapedQueryMethod => _shapedQueryMethodInfo;

        private static readonly MethodInfo _shapedQueryMethodInfo
            = typeof(QueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_ShapedQuery));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<T> _ShapedQuery<T>(
            QueryContext queryContext,
            ShaperCommandContext shaperCommandContext,
            IShaper<T> shaper)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var valueBuffer
                in new QueryingEnumerable(
                    (RelationalQueryContext)queryContext,
                    shaperCommandContext,
                    queryIndex: null))
            {
                yield return shaper.Shape(queryContext, valueBuffer);
            }
        }

        /// <summary>
        ///     Gets the default if empty shaped query method.
        /// </summary>
        /// <value>
        ///     The default if empty shaped query method.
        /// </value>
        public virtual MethodInfo DefaultIfEmptyShapedQueryMethod => _defaultIfEmptyShapedQueryMethodInfo;

        private static readonly MethodInfo _defaultIfEmptyShapedQueryMethodInfo
            = typeof(QueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_DefaultIfEmptyShapedQuery));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<T> _DefaultIfEmptyShapedQuery<T>(
            QueryContext queryContext,
            ShaperCommandContext shaperCommandContext,
            IShaper<T> shaper)
        {
            var checkedEmpty = false;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var valueBuffer
                in new QueryingEnumerable(
                    (RelationalQueryContext)queryContext,
                    shaperCommandContext,
                    queryIndex: null))
            {
                if (!checkedEmpty)
                {
                    var empty = true;

                    for (var i = 0; i < valueBuffer.Count; i++)
                    {
                        empty &= valueBuffer[i] == null;
                    }

                    if (empty)
                    {
                        yield break;
                    }

                    checkedEmpty = true;
                }

                yield return shaper.Shape(queryContext, valueBuffer);
            }
        }

        // TODO: Pass shaper to underlying enumerable

        /// <summary>
        ///     Gets the query method.
        /// </summary>
        /// <value>
        ///     The query method.
        /// </value>
        public virtual MethodInfo QueryMethod => _queryMethodInfo;

        private static readonly MethodInfo _queryMethodInfo
            = typeof(QueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_Query));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<ValueBuffer> _Query(
            QueryContext queryContext,
            ShaperCommandContext shaperCommandContext,
            int? queryIndex)
            => new QueryingEnumerable(
                (RelationalQueryContext)queryContext,
                shaperCommandContext,
                queryIndex);

        /// <summary>
        ///     Gets the get result method.
        /// </summary>
        /// <value>
        ///     The get result method.
        /// </value>
        public virtual MethodInfo GetResultMethod => _getResultMethodInfo;

        private static readonly MethodInfo _getResultMethodInfo
            = typeof(QueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(GetResult));

        [UsedImplicitly]
        private static TResult GetResult<TResult>(IEnumerable<ValueBuffer> valueBuffers)
        {
            using (var enumerator = valueBuffers.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    return enumerator.Current[0] == null
                        ? default(TResult)
                        : (TResult)enumerator.Current[0];
                }
            }

            return default(TResult);
        }

        /// <summary>
        ///     Gets the group by method.
        /// </summary>
        /// <value>
        ///     The group by method.
        /// </value>
        public virtual MethodInfo GroupByMethod => _groupByMethodInfo;

        private static readonly MethodInfo _groupByMethodInfo
            = typeof(QueryMethodProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_GroupBy));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<IGrouping<TKey, TElement>> _GroupBy<TSource, TKey, TElement>(
            IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector)
        {
            using (var sourceEnumerator = source.GetEnumerator())
            {
                var comparer = EqualityComparer<TKey>.Default;
                var hasNext = sourceEnumerator.MoveNext();

                while (hasNext)
                {
                    var currentKey = keySelector(sourceEnumerator.Current);
                    var element = elementSelector(sourceEnumerator.Current);
                    var grouping = new Grouping<TKey, TElement>(currentKey) { element };

                    while (true)
                    {
                        hasNext = sourceEnumerator.MoveNext();

                        if (!hasNext)
                        {
                            break;
                        }

                        if (!comparer.Equals(currentKey, keySelector(sourceEnumerator.Current)))
                        {
                            break;
                        }

                        grouping.Add(elementSelector(sourceEnumerator.Current));
                    }

                    yield return grouping;
                }
            }
        }

        /// <summary>
        ///     Gets the type of the group join include.
        /// </summary>
        /// <value>
        ///     The type of the group join include.
        /// </value>
        public virtual Type GroupJoinIncludeType => typeof(GroupJoinInclude);

        /// <summary>
        ///     Creates a group join include used to describe an Include operation that should
        ///     be performed as part of a GroupJoin.
        /// </summary>
        /// <param name="navigationPath"> The included navigation path. </param>
        /// <param name="querySourceRequiresTracking"> true if this query source requires tracking. </param>
        /// <param name="existingGroupJoinInclude"> A possibly null existing group join include. </param>
        /// <param name="relatedEntitiesLoaders"> The related entities loaders. </param>
        /// <returns>
        ///     A new group join include.
        /// </returns>
        public virtual object CreateGroupJoinInclude(
            IReadOnlyList<INavigation> navigationPath,
            bool querySourceRequiresTracking,
            object existingGroupJoinInclude,
            object relatedEntitiesLoaders)
        {
            var previousGroupJoinInclude
                = new GroupJoinInclude(
                    navigationPath,
                    (IReadOnlyList<Func<QueryContext, IRelatedEntitiesLoader>>)relatedEntitiesLoaders,
                    querySourceRequiresTracking);

            var groupJoinInclude = existingGroupJoinInclude as GroupJoinInclude;

            if (groupJoinInclude != null)
            {
                groupJoinInclude.SetPrevious(previousGroupJoinInclude);

                return null;
            }

            return previousGroupJoinInclude;
        }

        /// <summary>
        ///     Gets the group join method.
        /// </summary>
        /// <value>
        ///     The group join method.
        /// </value>
        public virtual MethodInfo GroupJoinMethod => _groupJoinMethodInfo;

        private static readonly MethodInfo _groupJoinMethodInfo
            = typeof(QueryMethodProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_GroupJoin));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<TResult> _GroupJoin<TOuter, TInner, TKey, TResult>(
            RelationalQueryContext queryContext,
            IEnumerable<ValueBuffer> source,
            IShaper<TOuter> outerShaper,
            IShaper<TInner> innerShaper,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
            GroupJoinInclude outerGroupJoinInclude,
            GroupJoinInclude innerGroupJoinInclude)
        {
            var outerGroupJoinIncludeContext = outerGroupJoinInclude?.CreateIncludeContext(queryContext);
            var innerGroupJoinIncludeContext = innerGroupJoinInclude?.CreateIncludeContext(queryContext);
            var outerAccessor = outerGroupJoinInclude?.EntityAccessor as Func<TOuter, object>;
            var innerAccessor = innerGroupJoinInclude?.EntityAccessor as Func<TInner, object>;

            try
            {
                using (var sourceEnumerator = source.GetEnumerator())
                {
                    var comparer = EqualityComparer<TKey>.Default;
                    var hasNext = sourceEnumerator.MoveNext();
                    var nextOuter = default(TOuter);

                    while (hasNext)
                    {
                        var outer
                            = Equals(nextOuter, default(TOuter))
                                ? outerShaper.Shape(queryContext, sourceEnumerator.Current)
                                : nextOuter;

                        nextOuter = default(TOuter);

                        outerGroupJoinIncludeContext?.Include(outerAccessor != null ? outerAccessor(outer) : outer);

                        var inner = innerShaper.Shape(queryContext, sourceEnumerator.Current);
                        var inners = new List<TInner>();

                        if (inner == null)
                        {
                            yield return resultSelector(outer, inners);

                            hasNext = sourceEnumerator.MoveNext();
                        }
                        else
                        {
                            var currentGroupKey = innerKeySelector(inner);

                            innerGroupJoinIncludeContext?.Include(innerAccessor != null ? innerAccessor(inner) : inner);

                            inners.Add(inner);

                            while (true)
                            {
                                hasNext = sourceEnumerator.MoveNext();

                                if (!hasNext)
                                {
                                    break;
                                }

                                nextOuter = outerShaper.Shape(queryContext, sourceEnumerator.Current);

                                if (!Equals(outer, nextOuter))
                                {
                                    break;
                                }

                                nextOuter = default(TOuter);

                                inner = innerShaper.Shape(queryContext, sourceEnumerator.Current);

                                if (inner == null)
                                {
                                    break;
                                }

                                var innerKey = innerKeySelector(inner);

                                if (!comparer.Equals(currentGroupKey, innerKey))
                                {
                                    break;
                                }

                                innerGroupJoinIncludeContext?.Include(inner);

                                inners.Add(inner);
                            }

                            yield return resultSelector(outer, inners);
                        }
                    }
                }
            }
            finally
            {
                innerGroupJoinIncludeContext?.Dispose();
                outerGroupJoinIncludeContext?.Dispose();
            }
        }

        /// <summary>
        ///     Gets the include method.
        /// </summary>
        /// <value>
        ///     The include method.
        /// </value>
        public virtual MethodInfo IncludeMethod => _includeMethodInfo;

        private static readonly MethodInfo _includeMethodInfo
            = typeof(QueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_Include));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<T> _Include<T>(
            RelationalQueryContext queryContext,
            IEnumerable<T> innerResults,
            Func<T, object> entityAccessor,
            IReadOnlyList<INavigation> navigationPath,
            IReadOnlyList<Func<QueryContext, IRelatedEntitiesLoader>> relatedEntitiesLoaderFactories,
            bool querySourceRequiresTracking)
        {
            queryContext.BeginIncludeScope();

            var relatedEntitiesLoaders
                = relatedEntitiesLoaderFactories.Select(f => f(queryContext))
                    .ToArray();

            try
            {
                foreach (var innerResult in innerResults)
                {
                    queryContext.QueryBuffer
                        .Include(
                            queryContext,
                            entityAccessor == null
                                ? innerResult
                                : entityAccessor(innerResult), // TODO: Compile time?
                            navigationPath,
                            relatedEntitiesLoaders,
                            querySourceRequiresTracking);

                    yield return innerResult;
                }
            }
            finally // Need this to run even if innerResults is not fully consumed.
            {
                foreach (var relatedEntitiesLoader in relatedEntitiesLoaders)
                {
                    relatedEntitiesLoader.Dispose();
                }

                queryContext.EndIncludeScope();
            }
        }

        /// <summary>
        ///     Gets the type of the related entities loader.
        /// </summary>
        /// <value>
        ///     The type of the related entities loader.
        /// </value>
        public virtual Type RelatedEntitiesLoaderType => typeof(IRelatedEntitiesLoader);

        /// <summary>
        ///     Gets the create reference related entities loader method.
        /// </summary>
        /// <value>
        ///     The create reference related entities loader method.
        /// </value>
        public virtual MethodInfo CreateReferenceRelatedEntitiesLoaderMethod
            => _createReferenceRelatedEntitiesLoaderMethod;

        private static readonly MethodInfo _createReferenceRelatedEntitiesLoaderMethod
            = typeof(QueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_CreateReferenceRelatedEntitiesLoader));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IRelatedEntitiesLoader _CreateReferenceRelatedEntitiesLoader(
            int valueBufferOffset,
            int queryIndex,
            Func<ValueBuffer, object> materializer)
            => new ReferenceRelatedEntitiesLoader(valueBufferOffset, queryIndex, materializer);

        private class ReferenceRelatedEntitiesLoader : IRelatedEntitiesLoader
        {
            private readonly int _valueBufferOffset;
            private readonly int _queryIndex;
            private readonly Func<ValueBuffer, object> _materializer;

            public ReferenceRelatedEntitiesLoader(
                int valueBufferOffset,
                int queryIndex,
                Func<ValueBuffer, object> materializer)
            {
                _valueBufferOffset = valueBufferOffset;
                _queryIndex = queryIndex;
                _materializer = materializer;
            }

            public IEnumerable<EntityLoadInfo> Load(QueryContext queryContext, IIncludeKeyComparer keyComparer)
            {
                var valueBuffer
                    = ((RelationalQueryContext)queryContext)
                        .GetIncludeValueBuffer(_queryIndex).WithOffset(_valueBufferOffset);

                yield return new EntityLoadInfo(valueBuffer, _materializer);
            }

            public void Dispose()
            {
                // no-op
            }
        }

        /// <summary>
        ///     Gets the create collection related entities loader method.
        /// </summary>
        /// <value>
        ///     The create collection related entities loader method.
        /// </value>
        public virtual MethodInfo CreateCollectionRelatedEntitiesLoaderMethod
            => _createCollectionRelatedEntitiesLoaderMethod;

        private static readonly MethodInfo _createCollectionRelatedEntitiesLoaderMethod
            = typeof(QueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_CreateCollectionRelatedEntitiesLoader));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IRelatedEntitiesLoader _CreateCollectionRelatedEntitiesLoader(
            QueryContext queryContext,
            ShaperCommandContext shaperCommandContext,
            int queryIndex,
            Func<ValueBuffer, object> materializer)
            => new CollectionRelatedEntitiesLoader(queryContext, shaperCommandContext, queryIndex, materializer);

        private class CollectionRelatedEntitiesLoader : IRelatedEntitiesLoader
        {
            private readonly IncludeCollectionIterator _includeCollectionIterator;
            private readonly Func<ValueBuffer, object> _materializer;

            public CollectionRelatedEntitiesLoader(
                QueryContext queryContext,
                ShaperCommandContext shaperCommandContext,
                int queryIndex,
                Func<ValueBuffer, object> materializer)
            {
                _includeCollectionIterator
                    = new IncludeCollectionIterator(
                        _Query(queryContext, shaperCommandContext, queryIndex)
                            .GetEnumerator());

                _materializer = materializer;
            }

            public IEnumerable<EntityLoadInfo> Load(QueryContext queryContext, IIncludeKeyComparer keyComparer)
                => _includeCollectionIterator
                    .GetRelatedValues(keyComparer)
                    .Select(vr => new EntityLoadInfo(vr, _materializer));

            public void Dispose() => _includeCollectionIterator?.Dispose();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo InjectParametersMethod => _injectParametersMethodInfo;

        private static readonly MethodInfo _injectParametersMethodInfo
            = typeof(QueryMethodProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_InjectParameters));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<TElement> _InjectParameters<TElement>(
            QueryContext queryContext,
            IEnumerable<TElement> source,
            string[] parameterNames,
            object[] parameterValues)
            => new ParameterInjector<TElement>(queryContext, source, parameterNames, parameterValues);

        private sealed class ParameterInjector<TElement> : IEnumerable<TElement>
        {
            private readonly QueryContext _queryContext;
            private readonly IEnumerable<TElement> _innerEnumerable;
            private readonly string[] _parameterNames;
            private readonly object[] _parameterValues;

            public ParameterInjector(
                QueryContext queryContext,
                IEnumerable<TElement> innerEnumerable,
                string[] parameterNames,
                object[] parameterValues)
            {
                _queryContext = queryContext;
                _innerEnumerable = innerEnumerable;
                _parameterNames = parameterNames;
                _parameterValues = parameterValues;
            }

            public IEnumerator<TElement> GetEnumerator() => new InjectParametersEnumerator(this);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private sealed class InjectParametersEnumerator : IEnumerator<TElement>
            {
                private readonly ParameterInjector<TElement> _parameterInjector;
                private readonly IEnumerator<TElement> _innerEnumerator;
                private bool _disposed;

                public InjectParametersEnumerator(ParameterInjector<TElement> parameterInjector)
                {
                    _parameterInjector = parameterInjector;

                    for (var i = 0; i < _parameterInjector._parameterNames.Length; i++)
                    {
                        _parameterInjector._queryContext.AddParameter(
                            _parameterInjector._parameterNames[i],
                            _parameterInjector._parameterValues[i]);
                    }

                    _innerEnumerator = _parameterInjector._innerEnumerable.GetEnumerator();
                }

                public TElement Current => _innerEnumerator.Current;

                object IEnumerator.Current => _innerEnumerator.Current;

                public bool MoveNext() => _innerEnumerator.MoveNext();

                public void Reset() => _innerEnumerator.Reset();

                public void Dispose()
                {
                    if (!_disposed)
                    {
                        _innerEnumerator.Dispose();

                        foreach (var parameterName in _parameterInjector._parameterNames)
                        {
                            _parameterInjector._queryContext.RemoveParameter(parameterName);
                        }

                        _disposed = true;
                    }
                }
            }
        }
    }
}
