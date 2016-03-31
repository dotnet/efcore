// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class ShaperCommandContext
    {
        private readonly ConcurrentDictionary<CommandCacheKey, IRelationalCommand> _commandCache
            = new ConcurrentDictionary<CommandCacheKey, IRelationalCommand>();

        private struct CommandCacheKey
        {
            private readonly IReadOnlyDictionary<string, object> _parameterValues;

            public CommandCacheKey(IReadOnlyDictionary<string, object> parameterValues)
            {
                _parameterValues = parameterValues;
            }

            public override bool Equals(object obj)
            {
                if (_parameterValues.Count > 0)
                {
                    var other = (CommandCacheKey)obj;

                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (var parameterValue in _parameterValues)
                    {
                        var value = parameterValue.Value;
                        var otherValue = other._parameterValues[parameterValue.Key];

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

        private readonly IRelationalValueBufferFactoryFactory _valueBufferFactoryFactory;

        private IRelationalValueBufferFactory _valueBufferFactory;

        public ShaperCommandContext(
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory,
            [NotNull] Func<IQuerySqlGenerator> querySqlGeneratorFactory)
        {
            _valueBufferFactoryFactory = valueBufferFactoryFactory;
            QuerySqlGeneratorFactory = querySqlGeneratorFactory;
        }

        public virtual Func<IQuerySqlGenerator> QuerySqlGeneratorFactory { get; }

        public virtual IRelationalValueBufferFactory ValueBufferFactory => _valueBufferFactory;

        public virtual IRelationalCommand GetRelationalCommand(
            [NotNull] IReadOnlyDictionary<string, object> parameters)
        {
            IRelationalCommand relationalCommand;
            var key = new CommandCacheKey(parameters);

            if (_commandCache.TryGetValue(key, out relationalCommand))
            {
                return relationalCommand;
            }

            var generator = QuerySqlGeneratorFactory();
            relationalCommand = generator.GenerateSql(parameters);

            if (generator.IsCacheable)
            {
                _commandCache.TryAdd(key, relationalCommand);
            }

            return relationalCommand;
        }

        public virtual void NotifyReaderCreated([NotNull] DbDataReader dataReader)
            => NonCapturingLazyInitializer
                .EnsureInitialized(
                    ref _valueBufferFactory,
                    new FactoryAndReader(_valueBufferFactoryFactory, dataReader),
                    s => QuerySqlGeneratorFactory()
                        .CreateValueBufferFactory(s.Factory, s.Reader));

        private struct FactoryAndReader
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
