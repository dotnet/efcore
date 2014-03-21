// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity;
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
            IEnumerable<Entity.ChangeTracking.StateEntry> stateEntries, IModel model, CancellationToken cancellationToken = default(CancellationToken))
        {
            var commands = new CommandBatchPreparer().BatchCommands(stateEntries);

            using (var connection = CreateConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                var executor = new BatchExecutor(commands, SqlGenerator);
                await executor.ExecuteAsync(connection, cancellationToken);
            }

            // TODO Return the actual results once we can get them
            return stateEntries.Count();
        }

        public override IAsyncEnumerable<object[]> Read(IEntityType type)
        {
            var sql = new StringBuilder();

            sql.Append("SELECT ")
                .AppendJoin(type.Properties.Select(p => p.StorageName))
                .AppendLine()
                .Append("FROM ")
                .AppendLine(type.StorageName);

            return new DbAsyncEnumerable(() => CreateConnection(_connectionString), sql.ToString(), _logger);
        }

        public abstract DbConnection CreateConnection([NotNull] string connectionString);

        private sealed class DbAsyncEnumerable : IAsyncEnumerable<object[]>
        {
            private readonly Func<DbConnection> _connectionFactory;
            private readonly string _sql;
            private readonly ILogger _logger;

            public DbAsyncEnumerable(Func<DbConnection> connectionFactory, string sql, ILogger logger)
            {
                _connectionFactory = connectionFactory;
                _sql = sql;
                _logger = logger;
            }

            public IAsyncEnumerator<object[]> GetAsyncEnumerator()
            {
                return new DbAsyncEnumerator(_connectionFactory, _sql, _logger);
            }

            IAsyncEnumerator IAsyncEnumerable.GetAsyncEnumerator()
            {
                return GetAsyncEnumerator();
            }
        }

        private sealed class DbAsyncEnumerator : IAsyncEnumerator<object[]>
        {
            private readonly string _sql;
            private readonly ILogger _logger;
            private readonly Func<DbConnection> _connectionFactory;

            private DbConnection _connection;
            private DbCommand _command;
            private DbDataReader _reader;

            public DbAsyncEnumerator(Func<DbConnection> connectionFactory, string sql, ILogger logger)
            {
                _connectionFactory = connectionFactory;
                _sql = sql;
                _logger = logger;
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

            public object[] Current
            {
                get
                {
                    var values = new object[_reader.FieldCount];

                    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                    _reader.GetValues(values);

                    return values;
                }
            }

            object IAsyncEnumerator.Current
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
        }
    }
}
