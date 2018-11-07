// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
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
        ///     Gets the fast query method.
        /// </summary>
        /// <value>
        ///     The fast query method.
        /// </value>
        public virtual MethodInfo FastQueryMethod => _fastQueryMethodInfo;

        private static readonly MethodInfo _fastQueryMethodInfo
            = typeof(QueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_FastQuery));

        // ReSharper disable once InconsistentNaming
        private static IEnumerable<TEntity> _FastQuery<TEntity>(
            RelationalQueryContext relationalQueryContext,
            ShaperCommandContext shaperCommandContext,
            Func<DbDataReader, DbContext, TEntity> materializer,
            Type contextType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            relationalQueryContext.Connection.Open();

            RelationalDataReader dataReader;

            try
            {
                var relationalCommand
                    = shaperCommandContext
                        .GetRelationalCommand(relationalQueryContext.ParameterValues);

                dataReader
                    = relationalCommand.ExecuteReader(
                        relationalQueryContext.Connection,
                        relationalQueryContext.ParameterValues);
            }
            catch
            {
                // If failure happens creating the data reader, then it won't be available to
                // handle closing the connection, so do it explicitly here to preserve ref counting.
                relationalQueryContext.Connection.Close();

                throw;
            }

            var dbDataReader = dataReader.DbDataReader;

            try
            {
                using (dataReader)
                {
                    using (relationalQueryContext.ConcurrencyDetector.EnterCriticalSection()) // TODO: IDisposable box?
                    {
                        while (true)
                        {
                            bool hasNext;

                            try
                            {
                                hasNext = dataReader.Read();
                            }
                            catch (Exception exception)
                            {
                                logger.QueryIterationFailed(contextType, exception);

                                throw;
                            }

                            if (hasNext)
                            {
                                yield return materializer(dbDataReader, relationalQueryContext.Context);
                            }
                            else
                            {
                                yield break;
                            }
                        }
                    }
                }
            }
            finally
            {
                relationalQueryContext.Connection?.Close();
                relationalQueryContext.Dispose();
            }
        }

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
            => new QueryingEnumerable<T>(
                (RelationalQueryContext)queryContext,
                shaperCommandContext,
                shaper);

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
                in _Query((RelationalQueryContext)queryContext, shaperCommandContext))
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
            ShaperCommandContext shaperCommandContext)
            => new QueryingEnumerable<ValueBuffer>(
                (RelationalQueryContext)queryContext,
                shaperCommandContext,
                IdentityShaper.Instance);

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
        private static TResult GetResult<TResult>(IEnumerable<ValueBuffer> valueBuffers, bool throwOnNullResult)
        {
            using (var enumerator = valueBuffers.GetEnumerator())
            {
                if (enumerator.MoveNext())
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
        ///     Gets the group by method.
        /// </summary>
        /// <value>
        ///     The group by method.
        /// </value>
        public virtual MethodInfo GroupByMethod => _groupByMethodInfo;

        private static readonly MethodInfo _groupByMethodInfo
            = typeof(QueryMethodProvider)
                .GetTypeInfo()
                .GetDeclaredMethod(nameof(_GroupBy));

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
                    var grouping = new Grouping<TKey, TElement>(currentKey)
                    {
                        element
                    };

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
        ///     Gets the group join method.
        /// </summary>
        /// <value>
        ///     The group join method.
        /// </value>
        public virtual MethodInfo GroupJoinMethod => _groupJoinMethodInfo;

        private static readonly MethodInfo _groupJoinMethodInfo
            = typeof(QueryMethodProvider)
                .GetTypeInfo()
                .GetDeclaredMethod(nameof(_GroupJoin));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<TResult> _GroupJoin<TOuter, TInner, TKey, TResult>(
            RelationalQueryContext queryContext,
            IEnumerable<ValueBuffer> source,
            IShaper<TOuter> outerShaper,
            IShaper<TInner> innerShaper,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
        {
            using (var sourceEnumerator = source.GetEnumerator())
            {
                var comparer = EqualityComparer<TKey>.Default;
                var hasNext = sourceEnumerator.MoveNext();
                var nextOuter = default(TOuter);

                while (hasNext)
                {
                    var outer
#pragma warning disable IDE0034 // Simplify 'default' expression - Equals(object, object) causes default(object)
                        = Equals(nextOuter, default(TOuter))
#pragma warning restore IDE0034 // Simplify 'default' expression
                            ? outerShaper.Shape(queryContext, sourceEnumerator.Current)
                            : nextOuter;

                    nextOuter = default;

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

                            nextOuter = default;

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

                            inners.Add(inner);
                        }

                        yield return resultSelector(outer, inners);
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo InjectParametersMethod => _injectParametersMethodInfo;

        private static readonly MethodInfo _injectParametersMethodInfo
            = typeof(QueryMethodProvider)
                .GetTypeInfo()
                .GetDeclaredMethod(nameof(_InjectParameters));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<TElement> _InjectParameters<TElement>(
            QueryContext queryContext,
            IEnumerable<TElement> source,
            string[] parameterNames,
            object[] parameterValues)
        {
            for (var i = 0; i < parameterNames.Length; i++)
            {
                queryContext.SetParameter(parameterNames[i], parameterValues[i]);
            }

            foreach (var element in source)
            {
                yield return element;
            }
        }
    }
}
