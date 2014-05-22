// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class AsyncEnumerableMethodProvider : IEnumerableMethodProvider
    {
        public virtual MethodInfo QueryValues
        {
            get { return _queryValuesMethodInfo; }
        }

        private static readonly MethodInfo _queryValuesMethodInfo
            = typeof(AsyncEnumerableMethodProvider).GetTypeInfo()
                .GetDeclaredMethod("_QueryValues");

        [UsedImplicitly]
        private static IAsyncEnumerable<IValueReader> _QueryValues(QueryContext queryContext, EntityQuery entityQuery)
        {
            var relationalQueryContext = (RelationalQueryContext)queryContext;

            return new AsyncEnumerable<IValueReader>(
                relationalQueryContext.Connection,
                entityQuery.ToString(),
                r => relationalQueryContext.ValueReaderFactory.Create(r),
                queryContext.Logger);
        }

        public virtual MethodInfo QueryEntities
        {
            get { return _queryEntitiesMethodInfo; }
        }

        private static readonly MethodInfo _queryEntitiesMethodInfo
            = typeof(AsyncEnumerableMethodProvider).GetTypeInfo()
                .GetDeclaredMethod("_QueryEntities");

        [UsedImplicitly]
        private static IAsyncEnumerable<TEntity> _QueryEntities<TEntity>(QueryContext queryContext, EntityQuery entityQuery)
        {
            var relationalQueryContext = ((RelationalQueryContext)queryContext);

            return new AsyncEnumerable<TEntity>(
                relationalQueryContext.Connection,
                entityQuery.ToString(),
                r => (TEntity)queryContext.StateManager
                    .GetOrMaterializeEntry(
                        queryContext.Model.GetEntityType(typeof(TEntity)),
                        relationalQueryContext.ValueReaderFactory.Create(r)).Entity,
                queryContext.Logger);
        }

        private sealed class AsyncEnumerable<T> : IAsyncEnumerable<T>
        {
            private readonly RelationalConnection _connection;
            private readonly string _sql;
            private readonly Func<DbDataReader, T> _shaper;
            private readonly ILogger _logger;

            public AsyncEnumerable(
                RelationalConnection connection,
                string sql,
                Func<DbDataReader, T> shaper,
                ILogger logger)
            {
                _connection = connection;
                _sql = sql;
                _shaper = shaper;
                _logger = logger;
            }

            public IAsyncEnumerator<T> GetEnumerator()
            {
                return new AsyncEnumerator(this);
            }

            private sealed class AsyncEnumerator : IAsyncEnumerator<T>
            {
                private readonly AsyncEnumerable<T> _enumerable;

                private DbCommand _command;
                private DbDataReader _reader;

                public AsyncEnumerator(AsyncEnumerable<T> enumerable)
                {
                    _enumerable = enumerable;
                }

                public Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    return _reader == null
                        ? InitializeAndReadAsync(cancellationToken)
                        : _reader.ReadAsync(cancellationToken);
                }

                private async Task<bool> InitializeAndReadAsync(
                    CancellationToken cancellationToken = default(CancellationToken))
                {
                    await _enumerable._connection.OpenAsync(cancellationToken);

                    _command = _enumerable._connection.DbConnection.CreateCommand();
                    _command.CommandText = _enumerable._sql;

                    _enumerable._logger.WriteSql(_enumerable._sql);

                    _reader = await _command.ExecuteReaderAsync(cancellationToken);

                    return await _reader.ReadAsync(cancellationToken);
                }

                public T Current
                {
                    get
                    {
                        if (_reader == null)
                        {
                            return default(T);
                        }

                        return _enumerable._shaper(_reader);
                    }
                }

                public void Dispose()
                {
                    if (_reader != null)
                    {
                        _reader.Dispose();
                    }

                    if (_command != null)
                    {
                        _command.Dispose();
                    }

                    if (_enumerable._connection != null)
                    {
                        _enumerable._connection.Close();
                    }
                }
            }
        }
    }
}
