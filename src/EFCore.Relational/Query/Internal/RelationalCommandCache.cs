// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class RelationalCommandCache : IPrintableExpression
    {
        private static readonly ConcurrentDictionary<object, object> _locks
            = new ConcurrentDictionary<object, object>();

        private readonly IMemoryCache _memoryCache;
        private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;
        private readonly SelectExpression _selectExpression;
        private readonly RelationalParameterBasedSqlProcessor _relationalParameterBasedSqlProcessor;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public RelationalCommandCache(
            [NotNull] IMemoryCache memoryCache,
            [NotNull] IQuerySqlGeneratorFactory querySqlGeneratorFactory,
            [NotNull] IRelationalParameterBasedSqlProcessorFactory relationalParameterBasedSqlProcessorFactory,
            [NotNull] SelectExpression selectExpression,
            [NotNull] IReadOnlyList<ReaderColumn> readerColumns,
            bool useRelationalNulls)
        {
            _memoryCache = memoryCache;
            _querySqlGeneratorFactory = querySqlGeneratorFactory;
            _selectExpression = selectExpression;
            ReaderColumns = readerColumns;
            _relationalParameterBasedSqlProcessor = relationalParameterBasedSqlProcessorFactory.Create(useRelationalNulls);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyList<ReaderColumn> ReaderColumns { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IRelationalCommand GetRelationalCommand([NotNull] IReadOnlyDictionary<string, object> parameters)
        {
            var cacheKey = new CommandCacheKey(_selectExpression, parameters);

            if (_memoryCache.TryGetValue(cacheKey, out IRelationalCommand relationalCommand))
            {
                return relationalCommand;
            }

            // When multiple threads attempt to start processing the same query (program startup / thundering
            // herd), have only one actually process and block the others.
            // Note that the following synchronization isn't perfect - some race conditions may cause concurrent
            // processing. This is benign (and rare).
            var compilationLock = _locks.GetOrAdd(cacheKey, _ => new object());
            try
            {
                lock (compilationLock)
                {
                    if (!_memoryCache.TryGetValue(cacheKey, out relationalCommand))
                    {
                        var selectExpression = _relationalParameterBasedSqlProcessor.Optimize(
                            _selectExpression, parameters, out var canCache);
                        relationalCommand = _querySqlGeneratorFactory.Create().GetCommand(selectExpression);

                        if (canCache)
                        {
                            _memoryCache.Set(cacheKey, relationalCommand, new MemoryCacheEntryOptions { Size = 10 });
                        }
                    }

                    return relationalCommand;
                }
            }
            finally
            {
                _locks.TryRemove(cacheKey, out _);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.AppendLine("RelationalCommandCache.SelectExpression(");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.Visit(_selectExpression);
                expressionPrinter.Append(")");
            }
        }

        private readonly struct CommandCacheKey : IEquatable<CommandCacheKey>
        {
            private readonly SelectExpression _selectExpression;
            private readonly IReadOnlyDictionary<string, object> _parameterValues;

            public CommandCacheKey(SelectExpression selectExpression, IReadOnlyDictionary<string, object> parameterValues)
            {
                _selectExpression = selectExpression;
                _parameterValues = parameterValues;
            }

            public override bool Equals(object obj)
                => obj is CommandCacheKey commandCacheKey
                    && Equals(commandCacheKey);

            public bool Equals(CommandCacheKey commandCacheKey)
            {
                if (!ReferenceEquals(_selectExpression, commandCacheKey._selectExpression))
                {
                    return false;
                }

                if (_parameterValues.Count > 0)
                {
                    foreach (var parameterValue in _parameterValues)
                    {
                        var value = parameterValue.Value;
                        if (!commandCacheKey._parameterValues.TryGetValue(parameterValue.Key, out var otherValue))
                        {
                            return false;
                        }

                        // ReSharper disable once ArrangeRedundantParentheses
                        if ((value == null) != (otherValue == null))
                        {
                            return false;
                        }

                        if (value is IEnumerable
                            && value.GetType() == typeof(object[]))
                        {
                            // FromSql parameters must have the same number of elements
                            return ((object[])value).Length == (otherValue as object[])?.Length;
                        }
                    }
                }

                return true;
            }

            public override int GetHashCode()
                => 0;
        }
    }
}
