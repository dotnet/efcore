// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RelationalCommandCache : IPrintableExpression
{
    private static readonly ConcurrentDictionary<object, object> Locks
        = new();

    private readonly IMemoryCache _memoryCache;
    private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;
    private readonly Expression _queryExpression;
    private readonly RelationalParameterBasedSqlProcessor _relationalParameterBasedSqlProcessor;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RelationalCommandCache(
        IMemoryCache memoryCache,
        IQuerySqlGeneratorFactory querySqlGeneratorFactory,
        IRelationalParameterBasedSqlProcessorFactory relationalParameterBasedSqlProcessorFactory,
        Expression queryExpression,
        bool useRelationalNulls,
        IReadOnlySet<string> parametersToConstantize)
    {
        _memoryCache = memoryCache;
        _querySqlGeneratorFactory = querySqlGeneratorFactory;
        _queryExpression = queryExpression;
        _relationalParameterBasedSqlProcessor = relationalParameterBasedSqlProcessorFactory.Create(
            new RelationalParameterBasedSqlProcessorParameters(useRelationalNulls, parametersToConstantize));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IRelationalCommandTemplate GetRelationalCommandTemplate(IReadOnlyDictionary<string, object?> parameters)
    {
        var cacheKey = new CommandCacheKey(_queryExpression, parameters);

        if (_memoryCache.TryGetValue(cacheKey, out IRelationalCommandTemplate? relationalCommandTemplate))
        {
            return relationalCommandTemplate!;
        }

        // When multiple threads attempt to start processing the same query (program startup / thundering
        // herd), have only one actually process and block the others.
        // Note that the following synchronization isn't perfect - some race conditions may cause concurrent
        // processing. This is benign (and rare).
        var compilationLock = Locks.GetOrAdd(cacheKey, _ => new object());
        try
        {
            lock (compilationLock)
            {
                if (!_memoryCache.TryGetValue(cacheKey, out relationalCommandTemplate))
                {
                    var queryExpression = _relationalParameterBasedSqlProcessor.Optimize(
                        _queryExpression, parameters, out var canCache);
                    relationalCommandTemplate = _querySqlGeneratorFactory.Create().GetCommand(queryExpression);

                    if (canCache)
                    {
                        _memoryCache.Set(cacheKey, relationalCommandTemplate, new MemoryCacheEntryOptions { Size = 10 });
                    }
                }

                return relationalCommandTemplate!;
            }
        }
        finally
        {
            Locks.TryRemove(cacheKey, out _);
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
        expressionPrinter.AppendLine("RelationalCommandCache.QueryExpression(");
        using (expressionPrinter.Indent())
        {
            expressionPrinter.Visit(_queryExpression);
            expressionPrinter.Append(")");
        }
    }

    private readonly struct CommandCacheKey
        : IEquatable<CommandCacheKey>
    {
        private readonly Expression _queryExpression;
        private readonly ParameterValueInfo[] _parameterValues;

        internal CommandCacheKey(Expression queryExpression, IReadOnlyDictionary<string, object?> parameterValues) {
            _queryExpression = queryExpression;
            _parameterValues = new ParameterValueInfo[parameterValues.Count];
            var i = 0;
            foreach (var (key, value) in parameterValues)
            {
                _parameterValues[i++] = new ParameterValueInfo(key, value);
            }
        }

        public override bool Equals(object? obj)
            => obj is CommandCacheKey commandCacheKey
                && Equals(commandCacheKey);

        public bool Equals(CommandCacheKey commandCacheKey)
        {
            // Intentionally reference equal, don't check internal components
            if (!ReferenceEquals(_queryExpression, commandCacheKey._queryExpression))
            {
                return false;
            }

            Check.DebugAssert(
                _parameterValues.Length == commandCacheKey._parameterValues.Length,
                "Parameter Count mismatch between identical expressions");

            for (var i = 0; i < _parameterValues.Length; i++)
            {
                var thisValue = _parameterValues[i];
                var otherValue = commandCacheKey._parameterValues[i];

                Check.DebugAssert(
                    thisValue.Key == otherValue.Key,
                    "Parameter Name mismatch between identical expressions");

                if (thisValue.IsNull != otherValue.IsNull)
                {
                    return false;
                }

                if (thisValue.ObjectArrayLength != otherValue.ObjectArrayLength)
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
            => RuntimeHelpers.GetHashCode(_queryExpression);
    }

    // Note that we keep only the nullness of parameters (and array length for FromSql object arrays), and avoid referencing the actual parameter data (see #34028).
    private readonly struct ParameterValueInfo
    {
        public string Key { get; }

        public bool IsNull { get; }

        public int? ObjectArrayLength { get; }

        internal ParameterValueInfo(string key, object? parameterValue)
        {
            Key = key;
            IsNull = parameterValue == null;
            ObjectArrayLength = parameterValue is object[] arr ? arr.Length : null;
        }
    }
}
