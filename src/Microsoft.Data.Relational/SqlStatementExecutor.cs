// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational
{
    public class SqlStatementExecutor
    {
        public virtual async Task ExecuteNonQueryAsync(
            [NotNull] DbConnection connection,
            [NotNull] IEnumerable<SqlStatement> statements,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(statements, "statements");

            // TODO Deal with suppressing transactions etc.

            var connectionWasOpen = connection.State == ConnectionState.Open;
            if (!connectionWasOpen)
            {
                await connection.OpenAsync(cancellationToken);
            }

            try
            {
                foreach (var statement in statements)
                {
                    await CreateCommand(connection, statement).ExecuteNonQueryAsync(cancellationToken);
                }
            }
            finally
            {
                if (connectionWasOpen)
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

            // TODO Deal with suppressing transactions etc.

            var connectionWasOpen = connection.State == ConnectionState.Open;
            if (!connectionWasOpen)
            {
                connection.Open();
            }

            try
            {
                foreach (var statement in statements)
                {
                    CreateCommand(connection, statement).ExecuteNonQuery();
                }
            }
            finally
            {
                if (connectionWasOpen)
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
