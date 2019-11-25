// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    public class RelationalCommandCache
    {
        private static readonly ConcurrentDictionary<object, object> _syncObjects
            = new ConcurrentDictionary<object, object>();

        private readonly IMemoryCache _memoryCache;
        private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;
        private readonly SelectExpression _selectExpression;
        private readonly ParameterValueBasedSelectExpressionOptimizer _parameterValueBasedSelectExpressionOptimizer;

        public RelationalCommandCache(
            IMemoryCache memoryCache,
            ISqlExpressionFactory sqlExpressionFactory,
            IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            IQuerySqlGeneratorFactory querySqlGeneratorFactory,
            bool useRelationalNulls,
            SelectExpression selectExpression)
        {
            _memoryCache = memoryCache;
            _querySqlGeneratorFactory = querySqlGeneratorFactory;
            _selectExpression = selectExpression;

            _parameterValueBasedSelectExpressionOptimizer = new ParameterValueBasedSelectExpressionOptimizer(
                sqlExpressionFactory,
                parameterNameGeneratorFactory,
                useRelationalNulls);
        }

        public virtual IRelationalCommand GetRelationalCommand(IReadOnlyDictionary<string, object> parameters)
        {
            var cacheKey = new CommandCacheKey(_selectExpression, parameters);

            retry:
            if (!_memoryCache.TryGetValue(cacheKey, out IRelationalCommand relationalCommand))
            {
                if (!_syncObjects.TryAdd(cacheKey, value: null))
                {
                    goto retry;
                }

                try
                {
                    var (selectExpression, canCache) =
                        _parameterValueBasedSelectExpressionOptimizer.Optimize(_selectExpression, parameters);
                    relationalCommand = _querySqlGeneratorFactory.Create().GetCommand(selectExpression);

                    if (canCache)
                    {
                        _memoryCache.Set(cacheKey, relationalCommand, new MemoryCacheEntryOptions { Size = 10 });
                    }
                }
                finally
                {
                    _syncObjects.TryRemove(cacheKey, out _);
                }
            }

            return relationalCommand;
        }

        private readonly struct CommandCacheKey
        {
            private readonly SelectExpression _selectExpression;
            private readonly IReadOnlyDictionary<string, object> _parameterValues;

            public CommandCacheKey(SelectExpression selectExpression, IReadOnlyDictionary<string, object> parameterValues)
            {
                _selectExpression = selectExpression;
                _parameterValues = parameterValues;
            }

            public override bool Equals(object obj)
                => obj != null
                    && obj is CommandCacheKey commandCacheKey
                    && Equals(commandCacheKey);

            private bool Equals(CommandCacheKey commandCacheKey)
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

            public override int GetHashCode() => 0;
        }
    }
}
