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
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ShaperCommandContext(
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory,
            [NotNull] Func<IQuerySqlGenerator> querySqlGeneratorFactory)
        {
            ValueBufferFactoryFactory = valueBufferFactoryFactory;
            QuerySqlGeneratorFactory = querySqlGeneratorFactory;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<IQuerySqlGenerator> QuerySqlGeneratorFactory { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IRelationalValueBufferFactory ValueBufferFactory => _valueBufferFactory;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IRelationalValueBufferFactoryFactory ValueBufferFactoryFactory { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IRelationalCommand GetRelationalCommand(
            [NotNull] IReadOnlyDictionary<string, object> parameters)
        {
            var key = new CommandCacheKey(parameters);

            if (_commandCache.TryGetValue(key, out var relationalCommand))
            {
                return relationalCommand;
            }

            var generator = QuerySqlGeneratorFactory();

            relationalCommand = generator.GenerateSql(parameters);

            if (generator.IsCacheable)
            {
                _commandCache.TryAdd(key.Clone(), relationalCommand);
            }

            return relationalCommand;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
