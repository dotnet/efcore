// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
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
        ParameterTranslationMode collectionParameterTranslationMode)
    {
        _memoryCache = memoryCache;
        _querySqlGeneratorFactory = querySqlGeneratorFactory;
        _queryExpression = queryExpression;
        _relationalParameterBasedSqlProcessor = relationalParameterBasedSqlProcessorFactory.Create(
            new RelationalParameterBasedSqlProcessorParameters(useRelationalNulls, collectionParameterTranslationMode));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IRelationalCommandTemplate GetRelationalCommandTemplate(Dictionary<string, object?> parameters)
        => GetRelationalCommandTemplate(parameters, parameterNamePrefix: null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <remarks>
    ///     When <paramref name="parameterNamePrefix" /> is given, all parameter names in the generated SQL are prefixed
    ///     with it, so that the command can be combined with other commands into a single batch.
    /// </remarks>
    public virtual IRelationalCommandTemplate GetRelationalCommandTemplate(
        Dictionary<string, object?> parameters,
        string? parameterNamePrefix)
    {
        var cacheKey = new CommandCacheKey(_queryExpression, parameters, parameterNamePrefix);

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
                    var queryExpression = _relationalParameterBasedSqlProcessor.Process(
                        _queryExpression, parameters, out var canCache);

                    if (parameterNamePrefix is not null)
                    {
                        queryExpression = new ParameterNamePrefixingExpressionVisitor(parameterNamePrefix).Visit(queryExpression);
                    }

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
    /// <remarks>
    ///     The returned delegate has the cache as its target, which allows <see cref="TryGetFromResolver" /> to recover
    ///     the cache from a resolver at runtime (e.g. to generate the command with prefixed parameter names for a batch).
    /// </remarks>
    public static RelationalCommandResolver CreateResolver(RelationalCommandCache relationalCommandCache)
        => relationalCommandCache.GetRelationalCommandTemplate;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool TryGetFromResolver(
        RelationalCommandResolver relationalCommandResolver,
        [NotNullWhen(true)] out RelationalCommandCache? relationalCommandCache)
    {
        relationalCommandCache = relationalCommandResolver.Target as RelationalCommandCache;

        return relationalCommandCache is not null;
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

    private readonly struct CommandCacheKey : IEquatable<CommandCacheKey>
    {
        private readonly Expression _queryExpression;
        private readonly Dictionary<string, ParameterInfo> _parameterInfos;
        private readonly string? _parameterNamePrefix;

        internal CommandCacheKey(
            Expression queryExpression,
            IReadOnlyDictionary<string, object?> parameterValues,
            string? parameterNamePrefix)
        {
            _queryExpression = queryExpression;
            _parameterNamePrefix = parameterNamePrefix;
            _parameterInfos = [];

            foreach (var (key, value) in parameterValues)
            {
                _parameterInfos[key] = new ParameterInfo
                {
                    IsNull = value is null,
                    ObjectArrayLength = value is object[] arr ? arr.Length : null
                };
            }
        }

        public override bool Equals(object? obj)
            => obj is CommandCacheKey commandCacheKey
                && Equals(commandCacheKey);

        public bool Equals(CommandCacheKey commandCacheKey)
        {
            // Intentionally reference equal, don't check internal components
            if (!ReferenceEquals(_queryExpression, commandCacheKey._queryExpression)
                || _parameterNamePrefix != commandCacheKey._parameterNamePrefix)
            {
                return false;
            }

            Check.DebugAssert(
                _parameterInfos.Count == commandCacheKey._parameterInfos.Count,
                "Parameter Count mismatch between identical queries");

            if (_parameterInfos.Count > 0)
            {
                foreach (var (key, info) in _parameterInfos)
                {
                    if (!commandCacheKey._parameterInfos.TryGetValue(key, out var otherInfo) || info != otherInfo)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override int GetHashCode()
            => HashCode.Combine(RuntimeHelpers.GetHashCode(_queryExpression), _parameterNamePrefix);
    }

    // Note that we keep only the null-ness of parameters (and array length for FromSql object arrays),
    // and avoid referencing the actual parameter data (see #34028).
    private readonly record struct ParameterInfo(bool IsNull, int? ObjectArrayLength);
}
