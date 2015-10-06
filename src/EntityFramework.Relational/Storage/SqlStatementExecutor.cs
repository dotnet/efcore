// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public class SqlStatementExecutor : ISqlStatementExecutor
    {
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;

        public SqlStatementExecutor([NotNull] IRelationalCommandBuilderFactory commandBuilderFactory)
        {
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));

            _commandBuilderFactory = commandBuilderFactory;
        }

        public virtual void ExecuteNonQuery(
            IRelationalConnection connection,
            IEnumerable<IRelationalCommand> relationalCommands)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(relationalCommands, nameof(relationalCommands));

            connection.Open();

            try
            {
                foreach (var command in relationalCommands)
                {
                    command.ExecuteNonQuery(connection);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        public virtual async Task ExecuteNonQueryAsync(
            IRelationalConnection connection,
            IEnumerable<IRelationalCommand> relationalCommands,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(relationalCommands, nameof(relationalCommands));

            await connection.OpenAsync(cancellationToken);

            try
            {
                foreach (var command in relationalCommands)
                {
                    await command.ExecuteNonQueryAsync(connection, cancellationToken);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        public virtual object ExecuteScalar(
            IRelationalConnection connection,
            string sql)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotEmpty(sql, nameof(sql));

            connection.Open();

            try
            {
                return CreateCommand(sql).ExecuteScalar(connection);
            }
            finally
            {
                connection.Close();
            }
        }

        public async virtual Task<object> ExecuteScalarAsync(
            IRelationalConnection connection,
            string sql,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotEmpty(sql, nameof(sql));

            await connection.OpenAsync(cancellationToken);

            try
            {
                return await CreateCommand(sql).ExecuteScalarAsync(connection, cancellationToken);
            }
            finally
            {
                connection.Close();
            }
        }

        public virtual DbDataReader ExecuteReader(
            IRelationalConnection connection,
            string sql)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotEmpty(sql, nameof(sql));

            connection.Open();

            try
            {
                return CreateCommand(sql).ExecuteReader(connection).DbDataReader;
            }
            finally
            {
                connection.Close();
            }
        }

        public virtual async Task<DbDataReader> ExecuteReaderAsync(
            IRelationalConnection connection,
            string sql,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotEmpty(sql, nameof(sql));

            await connection.OpenAsync(cancellationToken);

            try
            {
                return (await CreateCommand(sql).ExecuteReaderAsync(connection, cancellationToken)).DbDataReader;
            }
            finally
            {
                connection.Close();
            }
        }

        private IRelationalCommand CreateCommand(string sql)
            => _commandBuilderFactory.Create()
                .Append(sql)
                .BuildRelationalCommand();
    }
}
