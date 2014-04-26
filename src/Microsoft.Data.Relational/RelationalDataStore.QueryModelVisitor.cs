// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;

namespace Microsoft.Data.Relational
{
    public partial class RelationalDataStore
    {
        private class QueryModelVisitor : EntityQueryModelVisitor
        {
            private static readonly MethodInfo _entityScanMethodInfo
                = typeof(QueryModelVisitor).GetTypeInfo().GetDeclaredMethod("EntityScan");

            public QueryModelVisitor()
                : base(_entityScanMethodInfo, p => new QueryModelVisitor(p))
            {
            }

            private QueryModelVisitor(EntityQueryModelVisitor parentQueryModelVisitor)
                : base(parentQueryModelVisitor, _entityScanMethodInfo, p => new QueryModelVisitor(p))
            {
            }

            [UsedImplicitly]
            private static IEnumerable<TEntity> EntityScan<TEntity>(QueryContext queryContext)
            {
                var entityType = queryContext.Model.GetEntityType(typeof(TEntity));

                var sql = new StringBuilder();

                sql.Append("SELECT ")
                    .AppendJoin(entityType.Properties.Select(p => p.StorageName))
                    .AppendLine()
                    .Append("FROM ")
                    .AppendLine(entityType.StorageName);

                return new Enumerable<TEntity>((RelationalQueryContext)queryContext, sql.ToString(), entityType);
            }

            private sealed class Enumerable<T> : IAsyncEnumerable<T>
            {
                private readonly RelationalQueryContext _queryContext;
                private readonly string _sql;
                private readonly IEntityType _entityType;

                public Enumerable(RelationalQueryContext queryContext, string sql, IEntityType entityType)
                {
                    _queryContext = queryContext;
                    _sql = sql;
                    _entityType = entityType;
                }

                public IAsyncEnumerator<T> GetAsyncEnumerator()
                {
                    return new Enumerator<T>(_queryContext, _sql, _entityType);
                }

                IAsyncEnumerator IAsyncEnumerable.GetAsyncEnumerator()
                {
                    return GetAsyncEnumerator();
                }

                public IEnumerator<T> GetEnumerator()
                {
                    return GetAsyncEnumerator();
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }
            }

            private sealed class Enumerator<T> : IAsyncEnumerator<T>
            {
                private readonly RelationalQueryContext _queryContext;
                private readonly string _sql;
                private readonly IEntityType _entityType;

                private RelationalConnection _connection;
                private DbCommand _command;
                private DbDataReader _reader;

                public Enumerator(RelationalQueryContext queryContext, string sql, IEntityType entityType)
                {
                    _queryContext = queryContext;
                    _sql = sql;
                    _entityType = entityType;
                }

                public Task<bool> MoveNextAsync(
                    CancellationToken cancellationToken = default(CancellationToken))
                {
                    return _reader == null
                        ? InitializeAndReadAsync(cancellationToken)
                        : _reader.ReadAsync(cancellationToken);
                }

                private async Task<bool> InitializeAndReadAsync(
                    CancellationToken cancellationToken = default(CancellationToken))
                {
                    _connection = _queryContext.Connection;

                    await _connection.OpenAsync(cancellationToken);

                    _command = _connection.DbConnection.CreateCommand();
                    _command.CommandText = _sql;

                    _queryContext.Logger.WriteSql(_sql);

                    _reader = await _command.ExecuteReaderAsync(cancellationToken);

                    return await _reader.ReadAsync(cancellationToken);
                }

                public bool MoveNext()
                {
                    if (_reader == null)
                    {
                        _connection = _queryContext.Connection;
                        _connection.Open();

                        _command = _connection.DbConnection.CreateCommand();
                        _command.CommandText = _sql;

                        _queryContext.Logger.WriteSql(_sql);

                        _reader = _command.ExecuteReader();
                    }

                    return _reader.Read();
                }

                public T Current
                {
                    get
                    {
                        if (_reader == null)
                        {
                            return default(T);
                        }

                        return (T)_queryContext.StateManager
                            .GetOrMaterializeEntry(
                                _entityType, _queryContext.ValueReaderFactory.Create(_reader)).Entity;
                    }
                }

                object IEnumerator.Current
                {
                    get { return Current; }
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

                    if (_connection != null)
                    {
                        _connection.Close();
                    }
                }

                public void Reset()
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
