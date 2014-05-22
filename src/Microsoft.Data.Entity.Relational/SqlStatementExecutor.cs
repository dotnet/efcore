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

namespace Microsoft.Data.Entity.Relational
{
    public class SqlStatementExecutor
    {
        public virtual Task ExecuteNonQueryAsync(
            [NotNull] DbConnection connection,
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
                            await CreateCommand(connection, statement).ExecuteNonQueryAsync(cancellationToken);
                        }
                        return Task.FromResult<object>(null);
                    },
                cancellationToken);
        }

        public virtual Task<object> ExecuteScalarAsync(
            [NotNull] DbConnection connection,
            [NotNull] SqlStatement statement,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(statement, "statement");

            return ExecuteAsync(
                connection,
                () => CreateCommand(connection, statement).ExecuteScalarAsync(cancellationToken),
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
                await connection.OpenAsync(cancellationToken);
            }

            try
            {
                return await action();
            }
            finally
            {
                if (!connectionWasOpen)
                {
                    connection.Close();
                }
            }
        }

        public virtual void ExecuteNonQuery(
            [NotNull] DbConnection connection,
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
                            CreateCommand(connection, statement).ExecuteNonQuery();
                        }
                        return null;
                    });
        }

        public virtual object ExecuteScalar(
            [NotNull] DbConnection connection,
            [NotNull] SqlStatement statement)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(statement, "statement");

            return Execute(
                connection,
                () => CreateCommand(connection, statement).ExecuteScalar());
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
                    connection.Close();
                }
            }
        }

        private static DbCommand CreateCommand(DbConnection connection, SqlStatement statement)
        {
            var command = connection.CreateCommand();
            command.CommandText = statement.Sql;
            return command;
        }
    }
}
