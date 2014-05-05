// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

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
