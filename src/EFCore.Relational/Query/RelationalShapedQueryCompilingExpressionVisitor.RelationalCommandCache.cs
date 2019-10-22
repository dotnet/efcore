// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class RelationalShapedQueryCompilingExpressionVisitor
    {
        private class RelationalCommandCache
        {
            private readonly ConcurrentDictionary<CommandCacheKey, IRelationalCommand> _commandCache
                = new ConcurrentDictionary<CommandCacheKey, IRelationalCommand>(CommandCacheKeyComparer.Instance);
            private readonly ISqlExpressionFactory _sqlExpressionFactory;
            private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;
            private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;
            private readonly SelectExpression _selectExpression;
            private readonly ParameterValueBasedSelectExpressionOptimizer _parameterValueBasedSelectExpressionOptimizer;

            public RelationalCommandCache(
                ISqlExpressionFactory sqlExpressionFactory,
                IParameterNameGeneratorFactory parameterNameGeneratorFactory,
                IQuerySqlGeneratorFactory querySqlGeneratorFactory,
                SelectExpression selectExpression)
            {
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
                var key = new CommandCacheKey(parameters);

                if (_commandCache.TryGetValue(key, out var relationalCommand))
                {
                    return relationalCommand;
                }

                var selectExpression = _parameterValueBasedSelectExpressionOptimizer.Optimize(_selectExpression, parameters);

                relationalCommand = _querySqlGeneratorFactory.Create().GetCommand(selectExpression);

                if (ReferenceEquals(selectExpression, _selectExpression))
                {
                    _commandCache.TryAdd(key, relationalCommand);
                }

                return relationalCommand;
            }

            private sealed class CommandCacheKeyComparer : IEqualityComparer<CommandCacheKey>
            {
                public static readonly CommandCacheKeyComparer Instance = new CommandCacheKeyComparer();

                private CommandCacheKeyComparer()
                {
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool Equals(CommandCacheKey x, CommandCacheKey y)
                {
                    if (x.ParameterValues.Count > 0)
                    {
                        foreach (var parameterValue in x.ParameterValues)
                        {
                            var value = parameterValue.Value;

                            if (!y.ParameterValues.TryGetValue(parameterValue.Key, out var otherValue))
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

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public int GetHashCode(CommandCacheKey obj) => 0;
            }

            private readonly struct CommandCacheKey
            {
                public readonly IReadOnlyDictionary<string, object> ParameterValues;

                public CommandCacheKey(IReadOnlyDictionary<string, object> parameterValues)
                    => ParameterValues = parameterValues;
            }
        }
    }
}
