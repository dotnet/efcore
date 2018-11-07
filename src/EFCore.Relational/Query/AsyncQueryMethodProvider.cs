// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;

// ReSharper disable InconsistentNaming
// ReSharper disable ImplicitlyCapturedClosure
namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     Provides reflection objects for late-binding to asynchronous relational query operations.
    /// </summary>
    public class AsyncQueryMethodProvider : IQueryMethodProvider
    {
        /// <summary>
        ///     Gets the fast query method.
        /// </summary>
        /// <value>
        ///     The fast query method.
        /// </value>
        public virtual MethodInfo FastQueryMethod => _fastQueryMethodInfo;

        private static readonly MethodInfo _fastQueryMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_FastQuery));

        // ReSharper disable once InconsistentNaming
        private static IAsyncEnumerable<TEntity> _FastQuery<TEntity>(
            RelationalQueryContext relationalQueryContext,
            ShaperCommandContext shaperCommandContext,
            Func<DbDataReader, DbContext, TEntity> materializer,
            Type contextType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
            => new FastQueryAsyncEnumerable<TEntity>(
                relationalQueryContext,
                shaperCommandContext,
                materializer,
                contextType,
                logger);

        private class FastQueryAsyncEnumerable<TEntity> : IAsyncEnumerable<TEntity>
        {
            private readonly RelationalQueryContext _relationalQueryContext;
            private readonly ShaperCommandContext _shaperCommandContext;
            private readonly Func<DbDataReader, DbContext, TEntity> _materializer;
            private readonly Type _contextType;
            private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

            public FastQueryAsyncEnumerable(
                RelationalQueryContext relationalQueryContext,
                ShaperCommandContext shaperCommandContext,
                Func<DbDataReader, DbContext, TEntity> materializer,
                Type contextType,
                IDiagnosticsLogger<DbLoggerCategory.Query> logger)
            {
                _relationalQueryContext = relationalQueryContext;
                _shaperCommandContext = shaperCommandContext;
                _materializer = materializer;
                _contextType = contextType;
                _logger = logger;
            }

            public IAsyncEnumerator<TEntity> GetEnumerator()
                => new FastQueryAsyncEnumerator(this);

            private class FastQueryAsyncEnumerator : IAsyncEnumerator<TEntity>
            {
                private readonly RelationalQueryContext _relationalQueryContext;
                private readonly ShaperCommandContext _shaperCommandContext;
                private readonly Func<DbDataReader, DbContext, TEntity> _materializer;
                private readonly Type _contextType;
                private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

                private RelationalDataReader _dataReader;
                private DbDataReader _dbDataReader;

                private bool _disposed;

                public FastQueryAsyncEnumerator(FastQueryAsyncEnumerable<TEntity> state)
                {
                    _relationalQueryContext = state._relationalQueryContext;
                    _shaperCommandContext = state._shaperCommandContext;
                    _materializer = state._materializer;
                    _contextType = state._contextType;
                    _logger = state._logger;
                }

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    if (_dataReader == null)
                    {
                        await _relationalQueryContext.Connection.OpenAsync(cancellationToken);

                        try
                        {
                            var relationalCommand
                                = _shaperCommandContext
                                    .GetRelationalCommand(_relationalQueryContext.ParameterValues);

                            _dataReader
                                = await relationalCommand.ExecuteReaderAsync(
                                    _relationalQueryContext.Connection,
                                    _relationalQueryContext.ParameterValues, cancellationToken);
                        }
                        catch
                        {
                            //  If failure happens creating the data reader, then it won't be available to
                            //  handle closing the connection, so do it explicitly here to preserve ref counting.
                            _relationalQueryContext.Connection.Close();

                            throw;
                        }

                        _dbDataReader = _dataReader.DbDataReader;
                    }

                    using (await _relationalQueryContext.ConcurrencyDetector
                        .EnterCriticalSectionAsync(cancellationToken))
                    {
                        bool hasNext;

                        try
                        {
                            hasNext = await _dbDataReader.ReadAsync(cancellationToken);
                        }
                        catch (Exception exception)
                        {
                            _logger.QueryIterationFailed(_contextType, exception);

                            throw;
                        }

                        Current
                            = hasNext
                                ? _materializer(_dbDataReader, _relationalQueryContext.Context)
                                : default;

                        return hasNext;
                    }
                }

                public TEntity Current { get; private set; }

                public void Dispose()
                {
                    if (!_disposed)
                    {
                        try
                        {
                            _relationalQueryContext.Connection.Semaphore.Wait();

                            if (_dataReader != null)
                            {
                                _dataReader.Dispose();
                                _dataReader = null;
                                _dbDataReader = null;

                                _relationalQueryContext.Connection?.Close();
                            }

                            _disposed = true;
                        }
                        finally
                        {
                            _relationalQueryContext.Connection.Semaphore.Release();
                        }

                        _relationalQueryContext.Dispose();
                    }
                }
            }
        }

        /// <summary>
        ///     The shaped query method.
        /// </summary>
        public virtual MethodInfo ShapedQueryMethod => _shapedQueryMethodInfo;

        private static readonly MethodInfo _shapedQueryMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_ShapedQuery));

        [UsedImplicitly]
        private static IAsyncEnumerable<T> _ShapedQuery<T>(
            QueryContext queryContext,
            ShaperCommandContext shaperCommandContext,
            IShaper<T> shaper)
            => new AsyncQueryingEnumerable<T>(
                (RelationalQueryContext)queryContext,
                shaperCommandContext,
                shaper);

        /// <summary>
        ///     The default if empty shaped query method.
        /// </summary>
        public virtual MethodInfo DefaultIfEmptyShapedQueryMethod => _defaultIfEmptyShapedQueryMethodInfo;

        private static readonly MethodInfo _defaultIfEmptyShapedQueryMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_DefaultIfEmptyShapedQuery));

        [UsedImplicitly]
        private static IAsyncEnumerable<T> _DefaultIfEmptyShapedQuery<T>(
            QueryContext queryContext,
            ShaperCommandContext shaperCommandContext,
            IShaper<T> shaper)
            => new DefaultIfEmptyAsyncEnumerable(
                    _Query((RelationalQueryContext)queryContext, shaperCommandContext))
                .Select(vb => shaper.Shape(queryContext, vb));

        private sealed class DefaultIfEmptyAsyncEnumerable : IAsyncEnumerable<ValueBuffer>
        {
            private readonly IAsyncEnumerable<ValueBuffer> _source;

            public DefaultIfEmptyAsyncEnumerable(IAsyncEnumerable<ValueBuffer> source)
                => _source = source;

            public IAsyncEnumerator<ValueBuffer> GetEnumerator()
                => new DefaultIfEmptyAsyncEnumerator(_source.GetEnumerator());

            private sealed class DefaultIfEmptyAsyncEnumerator : IAsyncEnumerator<ValueBuffer>
            {
                private readonly IAsyncEnumerator<ValueBuffer> _enumerator;

                private bool _checkedEmpty;

                public DefaultIfEmptyAsyncEnumerator(IAsyncEnumerator<ValueBuffer> enumerator)
                    => _enumerator = enumerator;

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    if (!await _enumerator.MoveNext(cancellationToken))
                    {
                        return false;
                    }

                    if (!_checkedEmpty)
                    {
                        var empty = true;

                        for (var i = 0; i < _enumerator.Current.Count; i++)
                        {
                            empty &= _enumerator.Current[i] == null;
                        }

                        if (empty)
                        {
                            return false;
                        }

                        _checkedEmpty = true;
                    }

                    return true;
                }

                public ValueBuffer Current => _enumerator.Current;

                public void Dispose() => _enumerator.Dispose();
            }
        }

        /// <summary>
        ///     The query method.
        /// </summary>
        public virtual MethodInfo QueryMethod => _queryMethodInfo;

        private static readonly MethodInfo _queryMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_Query));

        [UsedImplicitly]
        private static IAsyncEnumerable<ValueBuffer> _Query(
            QueryContext queryContext,
            ShaperCommandContext shaperCommandContext)
            => new AsyncQueryingEnumerable<ValueBuffer>(
                (RelationalQueryContext)queryContext,
                shaperCommandContext,
                IdentityShaper.Instance);

        /// <summary>
        ///     The get result method.
        /// </summary>
        public virtual MethodInfo GetResultMethod => _getResultMethodInfo;

        private static readonly MethodInfo _getResultMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(GetResult));

        [UsedImplicitly]
        private static async Task<TResult> GetResult<TResult>(
            IAsyncEnumerable<ValueBuffer> valueBuffers,
            bool throwOnNullResult,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var enumerator = valueBuffers.GetEnumerator())
            {
                if (await enumerator.MoveNext(cancellationToken))
                {
                    return enumerator.Current[0] == null
                        ? !throwOnNullResult
                            ? default(TResult)
                            : throw new InvalidOperationException(RelationalStrings.NoElements)
                        : (TResult)enumerator.Current[0];
                }
            }

            return default;
        }

        /// <summary>
        ///     The group by method.
        /// </summary>
        public virtual MethodInfo GroupByMethod => _groupByMethodInfo;

        private static readonly MethodInfo _groupByMethodInfo
            = typeof(AsyncQueryMethodProvider)
                .GetTypeInfo()
                .GetDeclaredMethod(nameof(_GroupBy));

        [UsedImplicitly]
        private static IAsyncEnumerable<IGrouping<TKey, TElement>> _GroupBy<TSource, TKey, TElement>(
            IAsyncEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector)
            => new GroupByAsyncEnumerable<TSource, TKey, TElement>(source, keySelector, elementSelector);

        private sealed class GroupByAsyncEnumerable<TSource, TKey, TElement> : IAsyncEnumerable<IGrouping<TKey, TElement>>
        {
            private readonly IAsyncEnumerable<TSource> _source;
            private readonly Func<TSource, TKey> _keySelector;
            private readonly Func<TSource, TElement> _elementSelector;

            public GroupByAsyncEnumerable(
                IAsyncEnumerable<TSource> source,
                Func<TSource, TKey> keySelector,
                Func<TSource, TElement> elementSelector)
            {
                _source = source;
                _keySelector = keySelector;
                _elementSelector = elementSelector;
            }

            public IAsyncEnumerator<IGrouping<TKey, TElement>> GetEnumerator() => new GroupByAsyncEnumerator(this);

            private sealed class GroupByAsyncEnumerator : IAsyncEnumerator<IGrouping<TKey, TElement>>
            {
                private readonly GroupByAsyncEnumerable<TSource, TKey, TElement> _groupByAsyncEnumerable;
                private readonly IEqualityComparer<TKey> _comparer;

                private IAsyncEnumerator<TSource> _sourceEnumerator;
                private bool _hasNext;

                public GroupByAsyncEnumerator(GroupByAsyncEnumerable<TSource, TKey, TElement> groupByAsyncEnumerable)
                {
                    _groupByAsyncEnumerable = groupByAsyncEnumerable;
                    _comparer = EqualityComparer<TKey>.Default;
                }

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (_sourceEnumerator == null)
                    {
                        _sourceEnumerator = _groupByAsyncEnumerable._source.GetEnumerator();
                        _hasNext = await _sourceEnumerator.MoveNext(cancellationToken);
                    }

                    if (_hasNext)
                    {
                        var currentKey = _groupByAsyncEnumerable._keySelector(_sourceEnumerator.Current);
                        var element = _groupByAsyncEnumerable._elementSelector(_sourceEnumerator.Current);
                        var grouping = new Grouping<TKey, TElement>(currentKey)
                        {
                            element
                        };

                        while (true)
                        {
                            _hasNext = await _sourceEnumerator.MoveNext(cancellationToken);

                            if (!_hasNext)
                            {
                                break;
                            }

                            if (!_comparer.Equals(
                                currentKey,
                                _groupByAsyncEnumerable._keySelector(_sourceEnumerator.Current)))
                            {
                                break;
                            }

                            grouping.Add(_groupByAsyncEnumerable._elementSelector(_sourceEnumerator.Current));
                        }

                        Current = grouping;

                        return true;
                    }

                    return false;
                }

                public IGrouping<TKey, TElement> Current { get; private set; }

                public void Dispose() => _sourceEnumerator?.Dispose();
            }
        }

        /// <summary>
        ///     The group join method.
        /// </summary>
        public virtual MethodInfo GroupJoinMethod => _groupJoinMethodInfo;

        private static readonly MethodInfo _groupJoinMethodInfo
            = typeof(AsyncQueryMethodProvider)
                .GetTypeInfo()
                .GetDeclaredMethod(nameof(_GroupJoin));

        [UsedImplicitly]
        private static IAsyncEnumerable<TResult> _GroupJoin<TOuter, TInner, TKey, TResult>(
            RelationalQueryContext queryContext,
            IAsyncEnumerable<ValueBuffer> source,
            IShaper<TOuter> outerShaper,
            IShaper<TInner> innerShaper,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IAsyncEnumerable<TInner>, TResult> resultSelector)
            => new GroupJoinAsyncEnumerable<TOuter, TInner, TKey, TResult>(
                queryContext,
                source,
                outerShaper,
                innerShaper,
                innerKeySelector,
                resultSelector);

        private sealed class GroupJoinAsyncEnumerable<TOuter, TInner, TKey, TResult> : IAsyncEnumerable<TResult>
        {
            private readonly RelationalQueryContext _queryContext;
            private readonly IAsyncEnumerable<ValueBuffer> _source;
            private readonly IShaper<TOuter> _outerShaper;
            private readonly IShaper<TInner> _innerShaper;
            private readonly Func<TInner, TKey> _innerKeySelector;
            private readonly Func<TOuter, IAsyncEnumerable<TInner>, TResult> _resultSelector;
            private readonly bool _hasOuters;

            public GroupJoinAsyncEnumerable(
                RelationalQueryContext queryContext,
                IAsyncEnumerable<ValueBuffer> source,
                IShaper<TOuter> outerShaper,
                IShaper<TInner> innerShaper,
                Func<TInner, TKey> innerKeySelector,
                Func<TOuter, IAsyncEnumerable<TInner>, TResult> resultSelector)
            {
                _queryContext = queryContext;
                _source = source;
                _outerShaper = outerShaper;
                _innerShaper = innerShaper;
                _innerKeySelector = innerKeySelector;
                _resultSelector = resultSelector;
                _hasOuters = (_innerShaper as EntityShaper)?.ValueBufferOffset > 0;
            }

            public IAsyncEnumerator<TResult> GetEnumerator() => new GroupJoinAsyncEnumerator(this);

            private sealed class GroupJoinAsyncEnumerator : IAsyncEnumerator<TResult>
            {
                private readonly GroupJoinAsyncEnumerable<TOuter, TInner, TKey, TResult> _groupJoinAsyncEnumerable;
                private readonly IEqualityComparer<TKey> _comparer;

                private IAsyncEnumerator<ValueBuffer> _sourceEnumerator;
                private bool _hasNext;
                private TOuter _nextOuter;

                public GroupJoinAsyncEnumerator(
                    GroupJoinAsyncEnumerable<TOuter, TInner, TKey, TResult> groupJoinAsyncEnumerable)
                {
                    _groupJoinAsyncEnumerable = groupJoinAsyncEnumerable;
                    _comparer = EqualityComparer<TKey>.Default;
                }

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (_sourceEnumerator == null)
                    {
                        _sourceEnumerator = _groupJoinAsyncEnumerable._source.GetEnumerator();
                        _hasNext = await _sourceEnumerator.MoveNext(cancellationToken);
                        _nextOuter = default;
                    }

                    if (_hasNext)
                    {
                        var outer
#pragma warning disable IDE0034 // Simplify 'default' expression - Equals(object, object) causes default(object)
                            = Equals(_nextOuter, default(TOuter))
#pragma warning restore IDE0034 // Simplify 'default' expression
                                ? _groupJoinAsyncEnumerable._outerShaper
                                    .Shape(_groupJoinAsyncEnumerable._queryContext, _sourceEnumerator.Current)
                                : _nextOuter;

                        _nextOuter = default;

                        var inner
                            = _groupJoinAsyncEnumerable._innerShaper
                                .Shape(_groupJoinAsyncEnumerable._queryContext, _sourceEnumerator.Current);

                        var inners = new List<TInner>();

                        if (inner == null)
                        {
                            Current
                                = _groupJoinAsyncEnumerable._resultSelector(
                                    outer, inners.ToAsyncEnumerable());

                            _hasNext = await _sourceEnumerator.MoveNext(cancellationToken);

                            return true;
                        }

                        var currentGroupKey = _groupJoinAsyncEnumerable._innerKeySelector(inner);

                        inners.Add(inner);

                        while (true)
                        {
                            _hasNext = await _sourceEnumerator.MoveNext(cancellationToken);

                            if (!_hasNext)
                            {
                                break;
                            }

                            if (_groupJoinAsyncEnumerable._hasOuters)
                            {
                                _nextOuter
                                    = _groupJoinAsyncEnumerable._outerShaper
                                        .Shape(_groupJoinAsyncEnumerable._queryContext, _sourceEnumerator.Current);

                                if (!Equals(outer, _nextOuter))
                                {
                                    break;
                                }

                                _nextOuter = default;
                            }

                            inner
                                = _groupJoinAsyncEnumerable._innerShaper
                                    .Shape(_groupJoinAsyncEnumerable._queryContext, _sourceEnumerator.Current);

                            if (inner == null)
                            {
                                break;
                            }

                            var innerKey = _groupJoinAsyncEnumerable._innerKeySelector(inner);

                            if (!_comparer.Equals(currentGroupKey, innerKey))
                            {
                                break;
                            }

                            inners.Add(inner);
                        }

                        Current
                            = _groupJoinAsyncEnumerable._resultSelector(
                                outer, inners.ToAsyncEnumerable());

                        return true;
                    }

                    return false;
                }

                public TResult Current { get; private set; }

                public void Dispose()
                {
                    _sourceEnumerator?.Dispose();
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo InjectParametersMethod => _injectParametersMethodInfo;

        private static readonly MethodInfo _injectParametersMethodInfo
            = typeof(AsyncQueryMethodProvider)
                .GetTypeInfo()
                .GetDeclaredMethod(nameof(_InjectParameters));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IAsyncEnumerable<TElement> _InjectParameters<TElement>(
            QueryContext queryContext,
            IAsyncEnumerable<TElement> source,
            string[] parameterNames,
            object[] parameterValues)
            => new ParameterInjector<TElement>(queryContext, source, parameterNames, parameterValues);

        private sealed class ParameterInjector<TElement> : IAsyncEnumerable<TElement>
        {
            private readonly QueryContext _queryContext;
            private readonly IAsyncEnumerable<TElement> _innerEnumerable;
            private readonly string[] _parameterNames;
            private readonly object[] _parameterValues;

            public ParameterInjector(
                QueryContext queryContext,
                IAsyncEnumerable<TElement> innerEnumerable,
                string[] parameterNames,
                object[] parameterValues)
            {
                _queryContext = queryContext;
                _innerEnumerable = innerEnumerable;
                _parameterNames = parameterNames;
                _parameterValues = parameterValues;
            }

            IAsyncEnumerator<TElement> IAsyncEnumerable<TElement>.GetEnumerator() => new InjectParametersEnumerator(this);

            private sealed class InjectParametersEnumerator : IAsyncEnumerator<TElement>
            {
                private readonly IAsyncEnumerator<TElement> _innerEnumerator;

                public InjectParametersEnumerator(ParameterInjector<TElement> parameterInjector)
                {
                    for (var i = 0; i < parameterInjector._parameterNames.Length; i++)
                    {
                        parameterInjector._queryContext.SetParameter(
                            parameterInjector._parameterNames[i],
                            parameterInjector._parameterValues[i]);
                    }

                    _innerEnumerator = parameterInjector._innerEnumerable.GetEnumerator();
                }

                public TElement Current => _innerEnumerator.Current;

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                    => await _innerEnumerator.MoveNext(cancellationToken);

                public void Dispose() => _innerEnumerator.Dispose();
            }
        }
    }
}
