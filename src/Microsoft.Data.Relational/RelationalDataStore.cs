// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Relational.Update;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational
{
    public abstract class RelationalDataStore : DataStore
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;

        protected RelationalDataStore([NotNull] string connectionString, [NotNull] ILogger logger)
        {
            Check.NotEmpty(connectionString, "connectionString");
            Check.NotNull(logger, "logger");

            _connectionString = connectionString;
            _logger = logger;
        }

        protected abstract SqlGenerator SqlGenerator { get; }

        public virtual string ConnectionString
        {
            get { return _connectionString; }
        }

        public override async Task<int> SaveChangesAsync(
            IEnumerable<StateEntry> stateEntries,
            IModel model,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(stateEntries, "stateEntries");
            Check.NotNull(model, "model");

            //TODO: this should be cached
            var database = new DatabaseBuilder().Build(model);

            var commands = new CommandBatchPreparer().BatchCommands(stateEntries, database);

            using (var connection = CreateConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                var executor = new BatchExecutor(commands, SqlGenerator);
                await executor.ExecuteAsync(connection, cancellationToken);
            }

            // TODO Return the actual results once we can get them
            return stateEntries.Count();
        }

        public override IAsyncEnumerable<TResult> Query<TResult>(
            Type type, IModel model, StateManager stateManager)
        {
            Check.NotNull(model, "model");
            Check.NotNull(stateManager, "stateManager");

            var entityType = model.GetEntityType(type);
            var sql = new StringBuilder();

            sql.Append("SELECT ")
                .AppendJoin(entityType.Properties.Select(p => p.StorageName))
                .AppendLine()
                .Append("FROM ")
                .AppendLine(entityType.StorageName);

            return new Enumerable<TResult>(
                () => CreateConnection(_connectionString),
                sql.ToString(),
                _logger,
                entityType,
                stateManager);
        }

        public abstract DbConnection CreateConnection([NotNull] string connectionString);

        public virtual DbConnection CreateConnection()
        {
            return CreateConnection(_connectionString);
        }

        private sealed class Enumerable<T> : IAsyncEnumerable<T>
        {
            private readonly Func<DbConnection> _connectionFactory;
            private readonly string _sql;
            private readonly ILogger _logger;
            private readonly IEntityType _entityType;
            private readonly StateManager _stateManager;

            public Enumerable(
                Func<DbConnection> connectionFactory,
                string sql,
                ILogger logger,
                IEntityType entityType,
                StateManager stateManager)
            {
                _connectionFactory = connectionFactory;
                _sql = sql;
                _logger = logger;
                _entityType = entityType;
                _stateManager = stateManager;
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator()
            {
                return new Enumerator<T>(_connectionFactory, _sql, _logger, _entityType, _stateManager);
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
            private readonly Func<DbConnection> _connectionFactory;
            private readonly string _sql;
            private readonly ILogger _logger;
            private readonly IEntityType _entityType;
            private readonly StateManager _stateManager;

            private DbConnection _connection;
            private DbCommand _command;
            private DbDataReader _reader;

            public Enumerator(
                Func<DbConnection> connectionFactory,
                string sql,
                ILogger logger,
                IEntityType entityType,
                StateManager stateManager)
            {
                _connectionFactory = connectionFactory;
                _sql = sql;
                _logger = logger;
                _entityType = entityType;
                _stateManager = stateManager;
            }

            public Task<bool> MoveNextAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                return _reader == null
                    ? InitializeAndReadAsync(cancellationToken)
                    : _reader.ReadAsync(cancellationToken);
            }

            private async Task<bool> InitializeAndReadAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                _connection = _connectionFactory();

                await _connection.OpenAsync(cancellationToken);

                _command = _connection.CreateCommand();
                _command.CommandText = _sql;

                _logger.WriteSql(_sql);

                _reader = await _command.ExecuteReaderAsync(cancellationToken);

                return await _reader.ReadAsync(cancellationToken);
            }

            public bool MoveNext()
            {
                if (_reader == null)
                {
                    _connection = _connectionFactory();
                    _connection.Open();

                    _command = _connection.CreateCommand();
                    _command.CommandText = _sql;

                    _logger.WriteSql(_sql);

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

                    var values = new object[_reader.FieldCount];

                    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                    _reader.GetValues(values);

                    return (T)_stateManager.GetOrMaterializeEntry(_entityType, values).Entity;
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
                    _connection.Dispose();
                }
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }
}
