// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
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
    private readonly SelectExpression _selectExpression;
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
        SelectExpression selectExpression,
        IReadOnlyList<ReaderColumn?>? readerColumns,
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
    public virtual IReadOnlyList<ReaderColumn?>? ReaderColumns { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IRelationalCommandTemplate GetRelationalCommandTemplate(IReadOnlyDictionary<string, object?> parameters)
    {
        var cacheKey = new CommandCacheKey(_selectExpression, parameters);

        if (_memoryCache.TryGetValue(cacheKey, out IRelationalCommandTemplate relationalCommandTemplate))
        {
            return relationalCommandTemplate;
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
                    var selectExpression = _relationalParameterBasedSqlProcessor.Optimize(
                        _selectExpression, parameters, out var canCache);
                    relationalCommandTemplate = _querySqlGeneratorFactory.Create().GetCommand(selectExpression);

                    if (canCache)
                    {
                        _memoryCache.Set(cacheKey, relationalCommandTemplate, new MemoryCacheEntryOptions { Size = 10 });
                    }
                }

                return relationalCommandTemplate;
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
        private readonly IReadOnlyDictionary<string, object?> _parameterValues;

        public CommandCacheKey(SelectExpression selectExpression, IReadOnlyDictionary<string, object?> parameterValues)
        {
            _selectExpression = selectExpression;
            _parameterValues = parameterValues;
        }

        public override bool Equals(object? obj)
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
                foreach (var (key, value) in _parameterValues)
                {
                    if (!commandCacheKey._parameterValues.TryGetValue(key, out var otherValue))
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
