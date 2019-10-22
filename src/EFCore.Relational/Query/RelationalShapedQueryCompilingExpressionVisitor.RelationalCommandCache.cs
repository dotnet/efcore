// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class RelationalShapedQueryCompilingExpressionVisitor
    {
        private class RelationalCommandCache
        {
            private static readonly ConcurrentDictionary<object, object> _syncObjects
                = new ConcurrentDictionary<object, object>();
            private readonly IMemoryCache _memoryCache;

            private readonly ISqlExpressionFactory _sqlExpressionFactory;
            private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;
            private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;
            private readonly SelectExpression _selectExpression;
            private readonly ParameterValueBasedSelectExpressionOptimizer _parameterValueBasedSelectExpressionOptimizer;

            public RelationalCommandCache(
                IMemoryCache memoryCache,
                ISqlExpressionFactory sqlExpressionFactory,
                IParameterNameGeneratorFactory parameterNameGeneratorFactory,
                IQuerySqlGeneratorFactory querySqlGeneratorFactory,
                SelectExpression selectExpression)
            {
                _memoryCache = memoryCache;
                _sqlExpressionFactory = sqlExpressionFactory;
                _parameterNameGeneratorFactory = parameterNameGeneratorFactory;
                _querySqlGeneratorFactory = querySqlGeneratorFactory;
                _selectExpression = selectExpression;
                _parameterValueBasedSelectExpressionOptimizer = new ParameterValueBasedSelectExpressionOptimizer(
                    _sqlExpressionFactory,
                    _parameterNameGeneratorFactory);
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
                        var selectExpression = _parameterValueBasedSelectExpressionOptimizer.Optimize(_selectExpression, parameters);
                        relationalCommand = _querySqlGeneratorFactory.Create().GetCommand(selectExpression);

                        if (ReferenceEquals(selectExpression, _selectExpression))
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
                public readonly SelectExpression _selectExpression;
                public readonly IReadOnlyDictionary<string, object> _parameterValues;

                public CommandCacheKey(SelectExpression selectExpression, IReadOnlyDictionary<string, object> parameterValues)
                {
                    _selectExpression = selectExpression;
                    _parameterValues = parameterValues;
                }

                public override bool Equals(object obj)
                    => obj != null
                       && (ReferenceEquals(this, obj)
                           || obj is CommandCacheKey commandCacheKey
                           && Equals(commandCacheKey));

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

                            if (value == null
                                != (otherValue == null))
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
}
