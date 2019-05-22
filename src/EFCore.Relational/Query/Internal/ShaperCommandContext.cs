// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ShaperCommandContext
    {
        private readonly ConcurrentDictionary<CommandCacheKey, IRelationalCommand> _commandCache
            = new ConcurrentDictionary<CommandCacheKey, IRelationalCommand>(CommandCacheKeyComparer.Instance);

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

            public CommandCacheKey Clone() => new CommandCacheKey(
                new Dictionary<string, object>((Dictionary<string, object>)ParameterValues));
        }

        private IRelationalValueBufferFactory _valueBufferFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ShaperCommandContext(
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory,
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] Func<IQuerySqlGenerator> querySqlGeneratorFactory)
        {
            ValueBufferFactoryFactory = valueBufferFactoryFactory;
            QuerySqlGeneratorFactory = querySqlGeneratorFactory;
            CommandBuilderFactory = commandBuilderFactory;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Func<IQuerySqlGenerator> QuerySqlGeneratorFactory { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IRelationalValueBufferFactory ValueBufferFactory => _valueBufferFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IRelationalCommandBuilderFactory CommandBuilderFactory { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IRelationalValueBufferFactoryFactory ValueBufferFactoryFactory { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IRelationalCommand GetRelationalCommand(
            [NotNull] IReadOnlyDictionary<string, object> parameters,
            [NotNull] RelationalQueryContext relationalQueryContext)
        {
            var key = new CommandCacheKey(parameters);

            if (_commandCache.TryGetValue(key, out var relationalCommand))
            {
                return relationalCommand;
            }

            var generator = QuerySqlGeneratorFactory();

            relationalCommand = generator.GenerateSql(CommandBuilderFactory, parameters, relationalQueryContext.QueryLogger);

            if (generator.IsCacheable)
            {
                _commandCache.TryAdd(key.Clone(), relationalCommand);
            }

            return relationalCommand;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void NotifyReaderCreated([NotNull] DbDataReader dataReader)
            => NonCapturingLazyInitializer
                .EnsureInitialized(
                    ref _valueBufferFactory,
                    new FactoryAndReader(ValueBufferFactoryFactory, dataReader),
                    s => QuerySqlGeneratorFactory().CreateValueBufferFactory(s.Factory, s.Reader));

        private readonly struct FactoryAndReader
        {
            public readonly IRelationalValueBufferFactoryFactory Factory;
            public readonly DbDataReader Reader;

            public FactoryAndReader(IRelationalValueBufferFactoryFactory factory, DbDataReader reader)
            {
                Factory = factory;
                Reader = reader;
            }
        }
    }
}
