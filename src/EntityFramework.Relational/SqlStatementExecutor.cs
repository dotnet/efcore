// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Utilities;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational
{
    public class SqlStatementExecutor
    {
        private readonly LazyRef<ILogger> _logger;

        public SqlStatementExecutor([NotNull] ILoggerFactory loggerFactory)
        {
            Check.NotNull(loggerFactory, "loggerFactory");

            _logger = new LazyRef<ILogger>(() => loggerFactory.Create<SqlStatementExecutor>());
        }

        protected virtual ILogger Logger
        {
            get { return _logger.Value; }
        }

        public virtual Task ExecuteNonQueryAsync(
            [NotNull] DbConnection connection,
            [CanBeNull] DbTransaction transaction,
            [NotNull] IEnumerable<SqlStatement> statements,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(statements, "statements");

            return ExecuteAsync(
                connection,
                async () =>
                    {
                        foreach (var statement in statements)
                        {
                            Logger.WriteSql(statement.Sql);

                            await CreateCommand(connection, transaction, statement).ExecuteNonQueryAsync(cancellationToken)
                                .WithCurrentCulture();
                        }
                        return Task.FromResult<object>(null);
                    },
                cancellationToken);
        }

        public virtual Task<object> ExecuteScalarAsync(
            [NotNull] DbConnection connection,
            [CanBeNull] DbTransaction transaction,
            [NotNull] SqlStatement statement,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(statement, "statement");

            return ExecuteAsync(
                connection,
                () =>
                    {
                        Logger.WriteSql(statement.Sql);

                        return CreateCommand(connection, transaction, statement).ExecuteScalarAsync(cancellationToken);
                    },
                cancellationToken);
        }

        public virtual async Task<object> ExecuteAsync(
            [NotNull] DbConnection connection,
            [NotNull] Func<Task<object>> action,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(connection, "connection");

            // TODO Deal with suppressing transactions etc.

            var connectionWasOpen = connection.State == ConnectionState.Open;
            if (!connectionWasOpen)
            {
                Logger.OpeningConnection(connection.ConnectionString);

                await connection.OpenAsync(cancellationToken).WithCurrentCulture();
            }

            try
            {
                return await action().WithCurrentCulture();
            }
            finally
            {
                if (!connectionWasOpen)
                {
                    Logger.ClosingConnection(connection.ConnectionString);

                    connection.Close();
                }
            }
        }

        public virtual void ExecuteNonQuery(
            [NotNull] DbConnection connection,
            [CanBeNull] DbTransaction transaction,
            [NotNull] IEnumerable<SqlStatement> statements)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(statements, "statements");

            Execute(
                connection,
                () =>
                    {
                        foreach (var statement in statements)
                        {
                            Logger.WriteSql(statement.Sql);

                            CreateCommand(connection, transaction, statement).ExecuteNonQuery();
                        }
                        return null;
                    });
        }

        public virtual object ExecuteScalar(
            [NotNull] DbConnection connection,
            [CanBeNull] DbTransaction transaction,
            [NotNull] SqlStatement statement)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(statement, "statement");

            return Execute(
                connection,
                () =>
                    {
                        Logger.WriteSql(statement.Sql);

                        return CreateCommand(connection, transaction, statement).ExecuteScalar();
                    });
        }

        public virtual object Execute(
            [NotNull] DbConnection connection,
            [NotNull] Func<object> action)
        {
            Check.NotNull(connection, "connection");

            // TODO Deal with suppressing transactions etc.

            var connectionWasOpen = connection.State == ConnectionState.Open;
            if (!connectionWasOpen)
            {
                Logger.OpeningConnection(connection.ConnectionString);

                connection.Open();
            }

            try
            {
                return action();
            }
            finally
            {
                if (!connectionWasOpen)
                {
                    Logger.ClosingConnection(connection.ConnectionString);

                    connection.Close();
                }
            }
        }

        protected virtual DbCommand CreateCommand(
            DbConnection connection,
            DbTransaction transaction,
            SqlStatement statement)
        {
            var command = connection.CreateCommand();
            command.CommandText = statement.Sql;
            command.Transaction = transaction;
            return command;
        }
    }
}
