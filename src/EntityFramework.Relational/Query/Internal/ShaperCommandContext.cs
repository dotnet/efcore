// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Query.Internal
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
                            && (value.GetType() != typeof(string))
                            && (value.GetType() != typeof(byte[])))
                        {
                            // TODO: This doesn't always need to be deep.
                            // We could add a LINQ operator parameter attribute to tell us.
                            return StructuralComparisons
                                .StructuralEqualityComparer.Equals(value, otherValue);
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
            return _commandCache.GetOrAdd(
                new CommandCacheKey(parameters),
                cck => QuerySqlGeneratorFactory().GenerateSql(parameters));
        }

        public virtual void NotifyReaderCreated([NotNull] DbDataReader dataReader)
            => LazyInitializer
                .EnsureInitialized(
                    ref _valueBufferFactory,
                    () => QuerySqlGeneratorFactory()
                        .CreateValueBufferFactory(_valueBufferFactoryFactory, dataReader));
    }
}
